namespace JwtNet6.Models.Entities
{
    public class TBT_TIPOS_DOCUM : Auditoria
    {
        [Key]
        public int IDD_TIPOS_DOCUM { get; set; }
        public string DES_TIPOS_DOCUM { get; set; } = string.Empty;
    }
}
