using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace GreenCart.Middleware
{
    public class LoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<LoggingMiddleware> _logger;

        public LoggingMiddleware(RequestDelegate next, ILogger<LoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();

            // 1. Read & Log Request Body
            context.Request.EnableBuffering();
            var requestBody = await ReadStreamAsync(context.Request.Body, leaveOpen: true);
            context.Request.Body.Position = 0;

            _logger.LogInformation(
                "[HTTP Request] {Method} {Path}{QueryString} | Body: {RequestBody}",
                context.Request.Method,
                context.Request.Path,
                context.Request.QueryString,
                string.IsNullOrWhiteSpace(requestBody) ? "<empty>" : Truncate(requestBody, 500));

            // 2. Intercept Response Stream
            var originalResponseBodyStream = context.Response.Body;
            using var responseBodyStream = new MemoryStream();
            context.Response.Body = responseBodyStream;

            try
            {
                await _next(context);

                stopwatch.Stop();

                // 3. Read & Log Response Body
                responseBodyStream.Position = 0;
                var responseBody = await ReadStreamAsync(responseBodyStream, leaveOpen: true);
                responseBodyStream.Position = 0;

                _logger.LogInformation(
                    "[HTTP Response] {Method} {Path} -> {StatusCode} in {ElapsedMs}ms | Body: {ResponseBody}",
                    context.Request.Method,
                    context.Request.Path,
                    context.Response.StatusCode,
                    stopwatch.ElapsedMilliseconds,
                    string.IsNullOrWhiteSpace(responseBody) ? "<empty>" : Truncate(responseBody, 500));

                await responseBodyStream.CopyToAsync(originalResponseBodyStream);
            }
            finally
            {
                context.Response.Body = originalResponseBodyStream;
            }
        }

        private static async Task<string> ReadStreamAsync(Stream stream, bool leaveOpen)
        {
            using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, leaveOpen: leaveOpen);
            return await reader.ReadToEndAsync();
        }

        private static string Truncate(string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return value.Length <= maxLength ? value : value[..maxLength] + "... [truncated]";
        }
    }
}
