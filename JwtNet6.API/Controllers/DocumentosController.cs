namespace JwtNet6.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DocumentosController : ControllerBase
    {
        private readonly JwtContext _jwtContext;

        public DocumentosController(JwtContext jwtContext)
        {
            this._jwtContext = jwtContext;
        }

        [HttpGet("ListarDocumentos")]
        public async Task<IEnumerable<TBT_TIPOS_DOCUM>> ListarDocumentos()
        {
            return await _jwtContext.TBT_TIPOS_DOCUM.ToListAsync();
        }
    }
}
