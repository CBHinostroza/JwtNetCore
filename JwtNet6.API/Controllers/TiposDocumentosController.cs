namespace JwtNet6.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TiposDocumentosController : ControllerBase
    {
        private readonly JwtContext _jwtContext;

        public TiposDocumentosController(JwtContext jwtContext)
        {
            this._jwtContext = jwtContext;
        }

        [HttpGet("ListarTiposDocumentos")]
        public async Task<IEnumerable<TBT_TIPOS_DOCUM>> ListarTiposDocumentos()
        {
            return await _jwtContext.TBT_TIPOS_DOCUM.ToListAsync();
        }
    }
}
