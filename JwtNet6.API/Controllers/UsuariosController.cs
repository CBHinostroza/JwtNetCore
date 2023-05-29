using JwtNet6.Models.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace JwtNet6.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuariosController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly JwtSettings _JwtSettings;

        public UsuariosController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IOptions<JwtSettings> JwtSettings)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _JwtSettings = JwtSettings.Value;
        }

        [HttpPost("Login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] UserAuthenticateRequest model)
        {
            //Validamos las anotaciones del modelo
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            //Buscamos el usuario

            var usuario = await _userManager.FindByNameAsync(model.Username);
            if (usuario == null)
                return BadRequest(UtilitariosHelper.GetUsuarioCredenciales());

            //Autenticar por Usuario y Contraseña
            var resultado = await _signInManager.PasswordSignInAsync(model.Username, model.Password, false, false);

            //Retornar 404 en caso la autenticación sea incorrecto
            if (!resultado.Succeeded)
            {
                if (resultado.IsLockedOut)
                {
                    return BadRequest("El usuario se encuentra Bloqueado.");
                }

                return BadRequest(UtilitariosHelper.GetUsuarioCredenciales());
            }

            //Generar Token
            JwtSecurityToken jwtSecurityToken = await GenerateJwToken(usuario);
            UserAuthenticateResponse response = new UserAuthenticateResponse();
            response.Id = usuario.Id;
            response.JwtToken = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
            response.Email = usuario.Email;
            response.UserName = usuario.UserName;

            var roles = await _userManager.GetRolesAsync(usuario).ConfigureAwait(false);
            response.roles = roles.ToList();
            response.IsVerified = usuario.EmailConfirmed;

            var refreshToken = GenerateRefreshToken(GeneratedIPAddress());
            response.RefreshToken = refreshToken.Token;

            return Ok(response);
        }

        [HttpPost("Guardar")]
        [AllowAnonymous]
        public async Task<IActionResult> Guardar([FromBody] UserCreateRequest model)
        {
            //Validamos las anotaciones del modelo
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            //Validamos si el usuario ya fue registrado

            var usuario = await _userManager.FindByNameAsync(model.Username);

            if (usuario != null)
                return BadRequest(UtilitariosHelper.GetUsuarioExiste());

            //Nuevo modelo usuario
            var registro = new ApplicationUser()
            {
                UserName = model.Username,
                Email = model.Email
            };

            //Creamos el usuario
            var resultado = await _userManager.CreateAsync(registro, model.Password);

            //Retornar 404 en caso la creación tenga errores
            if (!resultado.Succeeded)
            {
                foreach (var item in resultado.Errors)
                {
                    ModelState.AddModelError(item.GetType().Name, item.Description);
                }

                return BadRequest(ModelState);
            }

            return Ok(resultado);
        }

        private async Task<JwtSecurityToken> GenerateJwToken(ApplicationUser model)
        {
            #region Obtener permisos y roles del usuario en caso tengamos configurado

            var userClaims = await _userManager.GetClaimsAsync(model);
            var roles = await _userManager.GetRolesAsync(model);

            var rolesClaims = new List<Claim>();

            foreach (var item in roles)
            {
                rolesClaims.Add(new Claim("Roles", item));
            }

            #endregion

            string ipAddress = IpHelper.GetIpAddress();

            var claims = new[]
            {
                //CHA: Claims de configuración
                new Claim(JwtRegisteredClaimNames.Sub,model.UserName),
                new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Email,model.Email),

                //CHA: Claims personales que almacenaremos
                new Claim("uid",model.Id),
                new Claim("ip",ipAddress),

            }
            .Union(userClaims)
            .Union(rolesClaims);

            var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_JwtSettings.Key));
            var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);

            var JwtSecurityToken = new JwtSecurityToken(
                _JwtSettings.Issuer,
                _JwtSettings.Audience,
                claims,
                expires: DateTime.Now.AddMinutes(_JwtSettings.DurationInMinutes),
                signingCredentials: signingCredentials
                );


            return JwtSecurityToken;
        }

        private RefreshToken GenerateRefreshToken(string ipAddress)
        {
            return new RefreshToken()
            {
                Token = RandomTokenStrin(),
                Expires = DateTime.Now.AddMinutes(5),
                Created = DateTime.Now,
                CreatedByIp = ipAddress
            };
        }

        private string RandomTokenStrin()
        {
            using var random = RandomNumberGenerator.Create();
            var randomBytes = new byte[40];
            random.GetBytes(randomBytes);
            return BitConverter.ToString(randomBytes).Replace("-", "");
        }

        private string GeneratedIPAddress()
        {
            if (Request.Headers.ContainsKey("X-Forwarded-For"))
                return Request.Headers["X-Forwarded-For"];
            else
                return HttpContext.Connection.RemoteIpAddress!.MapToIPv4().ToString();
        }
    }
}


