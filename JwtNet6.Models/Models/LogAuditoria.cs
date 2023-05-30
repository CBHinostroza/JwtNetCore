namespace JwtNet6.Models.Models
{
    public class LogAuditoria
    {
        public string COD_USUAR_CREAC { get; set; } = string.Empty;
        public DateTime FEC_USUAR_CREAC { get; set; }
        public string COD_USUAR_MODIF { get; set; } = string.Empty;
        public DateTime FEC_USUAR_MODIF { get; set; }
    }
}
