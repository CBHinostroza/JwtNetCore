namespace JwtNet6.Models.Models
{
    public class UserRefreshTokens : LogAuditoria
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string UserName { get; set; } = string.Empty;
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }
        public bool IsActive { get; set; }
        public DateTime? Created { get; set; }
        public string? CreatedByIp { get; set; }
        public DateTime? Revoked { get; set; }
        public string? RevokedByIp { get; set; }
    }
}
