namespace JwtNet6.Models.DTOs.Users
{
    public class UserAuthenticateResponse
    {
        public string Id { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public List<string> roles { get; set; } = new List<string>();
        public bool IsVerified { get; set; }
        public string Token { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
    }
}
