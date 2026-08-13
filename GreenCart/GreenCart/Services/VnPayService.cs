using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GreenCart.Configuration;
using GreenCart.Dtos.Responses.Payments;
using GreenCart.Entities.Enums;
using GreenCart.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GreenCart.Services
{
    public class VnPayService : IVnPayService
    {
        private readonly VnPaySettings _settings;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<VnPayService> _logger;

        public VnPayService(
            IOptions<VnPaySettings> settings,
            IUnitOfWork unitOfWork,
            ILogger<VnPayService> logger)
        {
            _settings = settings.Value;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<VnPayPaymentResponse> CreatePaymentUrlAsync(
            int orderId,
            int userId,
            string ipAddress,
            CancellationToken cancellationToken = default)
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(orderId);
            if (order == null || order.IsDeleted)
            {
                throw new KeyNotFoundException($"Order with ID {orderId} not found.");
            }

            if (order.UserId != userId)
            {
                throw new UnauthorizedAccessException("You are not authorized to pay for this order.");
            }

            if (!string.Equals(order.PaymentMethod, "VNPAY", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException($"Order {order.OrderCode} was not placed using VNPAY payment method.");
            }

            if (order.PaymentStatus == PaymentStatus.Paid)
            {
                throw new InvalidOperationException($"Order {order.OrderCode} is already paid.");
            }

            var timeZoneById = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            var timeNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZoneById);

            var vnpTxnRef = $"{order.OrderCode}_{timeNow:HHmmss}";
            var vnpAmount = Convert.ToInt64(order.TotalAmount * 100);

            var vnpParams = new SortedDictionary<string, string>(StringComparer.Ordinal)
            {
                { "vnp_Version", _settings.Version },
                { "vnp_Command", _settings.Command },
                { "vnp_TmnCode", _settings.TmnCode },
                { "vnp_Amount", vnpAmount.ToString() },
                { "vnp_CurrCode", _settings.CurrCode },
                { "vnp_TxnRef", vnpTxnRef },
                { "vnp_OrderInfo", $"Payment for order {order.OrderCode}" },
                { "vnp_OrderType", "other" },
                { "vnp_Locale", "vn" },
                { "vnp_ReturnUrl", _settings.ReturnUrl },
                { "vnp_IpAddr", string.IsNullOrEmpty(ipAddress) || ipAddress == "::1" ? "127.0.0.1" : ipAddress },
                { "vnp_CreateDate", timeNow.ToString("yyyyMMddHHmmss") },
                { "vnp_ExpireDate", timeNow.AddMinutes(15).ToString("yyyyMMddHHmmss") }
            };

            // Build raw data for hashing & query string
            var rawData = new StringBuilder();
            var queryString = new StringBuilder();

            foreach (var kv in vnpParams)
            {
                if (!string.IsNullOrEmpty(kv.Value))
                {
                    rawData.Append(WebUtility.UrlEncode(kv.Key)).Append('=').Append(WebUtility.UrlEncode(kv.Value)).Append('&');
                    queryString.Append(WebUtility.UrlEncode(kv.Key)).Append('=').Append(WebUtility.UrlEncode(kv.Value)).Append('&');
                }
            }

            var rawDataString = rawData.ToString().TrimEnd('&');
            var queryStringData = queryString.ToString().TrimEnd('&');

            var vnpSecureHash = ComputeHmacSha512(_settings.HashSecret, rawDataString);

            var paymentUrl = $"{_settings.BaseUrl}?{queryStringData}&vnp_SecureHash={vnpSecureHash}";

            _logger.LogInformation("Generated VNPAY payment URL for Order {OrderCode} (TxnRef: {TxnRef}).", order.OrderCode, vnpTxnRef);

            return new VnPayPaymentResponse
            {
                PaymentUrl = paymentUrl,
                OrderId = order.Id,
                OrderCode = order.OrderCode,
                Amount = order.TotalAmount
            };
        }

        public async Task<VnPayReturnResponse> HandleReturnAsync(
            IQueryCollection query,
            CancellationToken cancellationToken = default)
        {
            var vnpParams = new SortedDictionary<string, string>(StringComparer.Ordinal);
            string receivedHash = string.Empty;

            foreach (var key in query.Keys)
            {
                var value = query[key].ToString();
                if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_"))
                {
                    if (key.Equals("vnp_SecureHash", StringComparison.OrdinalIgnoreCase))
                    {
                        receivedHash = value;
                    }
                    else if (!key.Equals("vnp_SecureHashType", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(value))
                    {
                        vnpParams.Add(key, value);
                    }
                }
            }

            var rawData = new StringBuilder();
            foreach (var kv in vnpParams)
            {
                rawData.Append(WebUtility.UrlEncode(kv.Key)).Append('=').Append(WebUtility.UrlEncode(kv.Value)).Append('&');
            }

            var rawDataString = rawData.ToString().TrimEnd('&');
            var calculatedHash = ComputeHmacSha512(_settings.HashSecret, rawDataString);

            if (!calculatedHash.Equals(receivedHash, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("VNPAY Return signature verification failed. Calculated: {Calc}, Received: {Recv}", calculatedHash, receivedHash);
                throw new InvalidOperationException("Invalid VNPAY signature.");
            }

            var vnpTxnRef = vnpParams.GetValueOrDefault("vnp_TxnRef") ?? string.Empty;
            var responseCode = vnpParams.GetValueOrDefault("vnp_ResponseCode") ?? string.Empty;
            var transactionNo = vnpParams.GetValueOrDefault("vnp_TransactionNo") ?? string.Empty;
            var amountStr = vnpParams.GetValueOrDefault("vnp_Amount") ?? "0";

            var orderCode = vnpTxnRef.Contains('_') ? vnpTxnRef.Split('_')[0] : vnpTxnRef;

            var orders = await _unitOfWork.Orders.FindAsync(o => o.OrderCode.ToLower() == orderCode.ToLower() && !o.IsDeleted);
            var order = orders.FirstOrDefault();

            if (order == null)
            {
                throw new KeyNotFoundException($"Order with code {orderCode} not found.");
            }

            bool isSuccess = responseCode == "00";
            if (isSuccess)
            {
                order.PaymentStatus = PaymentStatus.Paid;
                order.Status = OrderStatus.Confirmed;
                order.PaymentMethod = "VNPAY";
                order.UpdatedAt = DateTime.UtcNow;

                _unitOfWork.Orders.Update(order);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("VNPAY Return: Order {OrderCode} successfully marked as Paid and Status as Confirmed.", order.OrderCode);
            }
            else
            {
                if (order.PaymentStatus != PaymentStatus.Paid)
                {
                    order.PaymentStatus = PaymentStatus.Failed;
                    order.UpdatedAt = DateTime.UtcNow;

                    _unitOfWork.Orders.Update(order);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                }

                _logger.LogWarning("VNPAY Return: Order {OrderCode} payment failed with response code {ResponseCode}.", order.OrderCode, responseCode);
            }

            decimal amount = Convert.ToDecimal(amountStr) / 100m;

            return new VnPayReturnResponse
            {
                IsSuccess = isSuccess,
                Message = isSuccess ? "Payment successful via VNPAY." : GetVnPayErrorMessage(responseCode),
                OrderId = order.Id,
                TransactionId = transactionNo,
                Amount = amount,
                PaymentMethod = "VNPAY"
            };
        }

        public async Task<VnPayIpnResponse> HandleIpnAsync(
            IQueryCollection query,
            CancellationToken cancellationToken = default)
        {
            var vnpParams = new SortedDictionary<string, string>(StringComparer.Ordinal);
            string receivedHash = string.Empty;

            foreach (var key in query.Keys)
            {
                var value = query[key].ToString();
                if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_"))
                {
                    if (key.Equals("vnp_SecureHash", StringComparison.OrdinalIgnoreCase))
                    {
                        receivedHash = value;
                    }
                    else if (!key.Equals("vnp_SecureHashType", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(value))
                    {
                        vnpParams.Add(key, value);
                    }
                }
            }

            var rawData = new StringBuilder();
            foreach (var kv in vnpParams)
            {
                rawData.Append(WebUtility.UrlEncode(kv.Key)).Append('=').Append(WebUtility.UrlEncode(kv.Value)).Append('&');
            }

            var rawDataString = rawData.ToString().TrimEnd('&');
            var calculatedHash = ComputeHmacSha512(_settings.HashSecret, rawDataString);

            // 1. Validate signature
            if (!calculatedHash.Equals(receivedHash, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("VNPAY IPN signature validation failed.");
                return new VnPayIpnResponse { RspCode = "97", Message = "Invalid signature" };
            }

            var vnpTxnRef = vnpParams.GetValueOrDefault("vnp_TxnRef") ?? string.Empty;
            var responseCode = vnpParams.GetValueOrDefault("vnp_ResponseCode") ?? string.Empty;
            var vnpAmountStr = vnpParams.GetValueOrDefault("vnp_Amount") ?? "0";

            var orderCode = vnpTxnRef.Contains('_') ? vnpTxnRef.Split('_')[0] : vnpTxnRef;

            // 2. Validate order existence
            var orders = await _unitOfWork.Orders.FindAsync(o => o.OrderCode.ToLower() == orderCode.ToLower() && !o.IsDeleted);
            var order = orders.FirstOrDefault();

            if (order == null)
            {
                _logger.LogWarning("VNPAY IPN: Order {OrderCode} not found.", orderCode);
                return new VnPayIpnResponse { RspCode = "01", Message = "Order not found" };
            }

            // 3. Validate amount
            long vnpAmount = Convert.ToInt64(vnpAmountStr);
            long expectedAmount = Convert.ToInt64(order.TotalAmount * 100);

            if (vnpAmount != expectedAmount)
            {
                _logger.LogWarning("VNPAY IPN: Invalid amount for Order {OrderCode}. Expected: {Expected}, Received: {Received}", order.OrderCode, expectedAmount, vnpAmount);
                return new VnPayIpnResponse { RspCode = "04", Message = "Invalid amount" };
            }

            // 4. Idempotency Check (order already confirmed)
            if (order.PaymentStatus == PaymentStatus.Paid)
            {
                _logger.LogInformation("VNPAY IPN: Order {OrderCode} already confirmed as Paid.", order.OrderCode);
                return new VnPayIpnResponse { RspCode = "02", Message = "Order already confirmed" };
            }

            // 5. Update Payment Status & Order Status
            if (responseCode == "00")
            {
                order.PaymentStatus = PaymentStatus.Paid;
                order.Status = OrderStatus.Confirmed;
                order.PaymentMethod = "VNPAY";
                order.UpdatedAt = DateTime.UtcNow;

                _unitOfWork.Orders.Update(order);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("VNPAY IPN: Order {OrderCode} payment confirmed as Paid and Status as Confirmed.", order.OrderCode);
            }
            else
            {
                order.PaymentStatus = PaymentStatus.Failed;
                order.UpdatedAt = DateTime.UtcNow;

                _unitOfWork.Orders.Update(order);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogWarning("VNPAY IPN: Order {OrderCode} payment failed with code {ResponseCode}.", order.OrderCode, responseCode);
            }

            return new VnPayIpnResponse { RspCode = "00", Message = "Confirm Success" };
        }

        public async Task<PaymentStatus> GetTransactionStatusAsync(
            int orderId,
            CancellationToken cancellationToken = default)
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(orderId);
            if (order == null || order.IsDeleted)
            {
                throw new KeyNotFoundException($"Order with ID {orderId} not found.");
            }

            return order.PaymentStatus;
        }

        private static string ComputeHmacSha512(string key, string inputData)
        {
            var hash = new StringBuilder();
            var keyBytes = Encoding.UTF8.GetBytes(key);
            var inputBytes = Encoding.UTF8.GetBytes(inputData);

            using var hmac = new HMACSHA512(keyBytes);
            var hashBytes = hmac.ComputeHash(inputBytes);

            foreach (var b in hashBytes)
            {
                hash.Append(b.ToString("x2"));
            }

            return hash.ToString().ToUpper();
        }

        private static string GetVnPayErrorMessage(string responseCode)
        {
            return responseCode switch
            {
                "00" => "Transaction successful.",
                "07" => "Transaction suspected of fraud.",
                "09" => "Card/Account is not registered for Internet Banking.",
                "10" => "Customer incorrectly entered card/account information 3 times.",
                "11" => "Payment deadline has expired.",
                "12" => "Customer card/account is locked.",
                "13" => "Customer entered incorrect transaction authentication password (OTP).",
                "24" => "Customer cancelled the transaction.",
                "51" => "Customer account has insufficient balance.",
                "65" => "Customer account has exceeded daily transaction limit.",
                "75" => "Payment bank is under maintenance.",
                "79" => "Customer entered incorrect payment password too many times.",
                _ => $"Payment failed with VNPAY error code {responseCode}."
            };
        }
    }
}
