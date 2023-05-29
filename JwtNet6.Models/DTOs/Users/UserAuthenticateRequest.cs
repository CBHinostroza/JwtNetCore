namespace JwtNet6.Models.DTOs.Users
{
    public class UserAuthenticateRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
