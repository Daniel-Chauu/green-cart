namespace GreenCart.Dtos.Responses.Auth
{
    public class VerifyResetCodeResponse
    {
        public bool IsValid { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
