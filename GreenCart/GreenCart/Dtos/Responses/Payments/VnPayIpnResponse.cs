namespace GreenCart.Dtos.Responses.Payments
{
    public class VnPayIpnResponse
    {
        public string RspCode { get; set; } = string.Empty; // "00" = Success
        public string Message { get; set; } = string.Empty;
    }
}
