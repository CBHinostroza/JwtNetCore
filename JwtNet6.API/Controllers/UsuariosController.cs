using JwtNet6.Models.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
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
        private readonly JwtContext _jwtContext;

        public UsuariosController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IOptions<JwtSettings> JwtSettings,
            JwtContext jwtContext)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _JwtSettings = JwtSettings.Value;
            _jwtContext = jwtContext;
        }

        [AllowAnonymous]
        [HttpPost("Login")]
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

            //Response
            UserAuthenticateResponse response = new UserAuthenticateResponse()
            {
                Id = usuario.Id,
                Email = usuario.Email,
                UserName = usuario.UserName,
                IsVerified = usuario.EmailConfirmed,
                roles = (List<string>)await _userManager.GetRolesAsync(usuario).ConfigureAwait(false),
                Token = new JwtSecurityTokenHandler().WriteToken(await GenerateJwToken(usuario)),
                RefreshToken = GenerateRefreshToken(usuario)
            };

            return Ok(response);
        }

        [HttpPost("Guardar")]
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

        [AllowAnonymous]
        [HttpPost("Refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] UserAccessTokenRequest model)
        {

            var principal = GetPrincipalFromExpiredToken(model.AccessToken);

            if (principal == null)
            {
                return BadRequest(UtilitariosHelper.GetTokenInvalido());
            }

#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            string username = principal.Identity.Name;
#pragma warning restore CS8602 // Dereference of a possibly null reference.
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.


            var token = await _jwtContext.UserRefreshTokens.FirstOrDefaultAsync(t => t.UserName == username);

            if (token == null ||
                token.RefreshToken != model.RefreshToken
                || token.RefreshTokenExpiryTime <= DateTime.Now)
            {
                return BadRequest(UtilitariosHelper.GetTokenInvalido());
            }

            var usuario = await _userManager.FindByNameAsync(token.UserName);
            var newAccessToken = await GenerateJwToken(usuario);
            var newRefreshToken = GenerateRefreshToken(usuario);

            return Ok(new
            {
                accessToken = new JwtSecurityTokenHandler().WriteToken(newAccessToken),
                refreshToken = newRefreshToken
            });
        }

        private string GenerateRefreshToken(ApplicationUser model)
        {
            //Registramos el RefreshToken por usuario o actualizamos la información

            var UserRefreshTokens = _jwtContext.UserRefreshTokens.FirstOrDefault(t => t.UserName == model.UserName);

            if (UserRefreshTokens == null)
            {
                var RefreshTokens = new UserRefreshTokens()
                {
                    RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(_JwtSettings.DurationInInDays),
                    RefreshToken = UtilitariosHelper.RandomTokenStrin(),
                    UserName = model.UserName,
                    IsActive = true,
                    Created = DateTime.UtcNow,
                    CreatedByIp = UtilitariosHelper.GetIpAddress(),
                    Revoked = null,
                    RevokedByIp = null,
                    COD_USUAR_CREAC = model.UserName,
                    COD_USUAR_MODIF = model.UserName,
                    FEC_USUAR_CREAC = DateTime.Now,
                    FEC_USUAR_MODIF = DateTime.Now
                };
                _jwtContext.UserRefreshTokens.Add(RefreshTokens);
                _jwtContext.SaveChanges();
                return RefreshTokens.RefreshToken;
            }
            else
            {
                UserRefreshTokens.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(_JwtSettings.DurationInInDays);
                UserRefreshTokens.RefreshToken = UtilitariosHelper.RandomTokenStrin();
                UserRefreshTokens.Created = DateTime.UtcNow;
                UserRefreshTokens.CreatedByIp = UtilitariosHelper.GetIpAddress();
                UserRefreshTokens.COD_USUAR_MODIF = model.UserName;
                UserRefreshTokens.FEC_USUAR_MODIF = DateTime.UtcNow;
                _jwtContext.UserRefreshTokens.Update(UserRefreshTokens);
                _jwtContext.SaveChanges();
                return UserRefreshTokens.RefreshToken;
            }
        }
        private ClaimsPrincipal? GetPrincipalFromExpiredToken(string? token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateLifetime = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_JwtSettings.Key))
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);
            if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("Token Invalido");

            return principal;
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

            string ipAddress = UtilitariosHelper.GetIpAddress();

            var claims = new[]
            {
                //CHA: Claims de configuración
                new Claim(JwtRegisteredClaimNames.Sub,model.UserName),
                new Claim(ClaimTypes.Name,model.UserName),
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
    }
}


