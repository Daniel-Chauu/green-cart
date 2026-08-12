namespace GreenCart.Dtos.Responses.Auth
{
    public class AuthResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public string? RefreshToken { get; set; }
        public string TokenType { get; set; } = "Bearer";
        public int ExpiresIn { get; set; } // in seconds (e.g. 900 for 15 mins)
        public UserProfileResponse User { get; set; } = null!;
    }
}
