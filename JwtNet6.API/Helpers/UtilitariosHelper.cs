using System.Net;
using System.Security.Cryptography;

namespace JwtNet6.API.Helpers
{
    public class UtilitariosHelper
    {

        public static string GetUsuarioCredenciales()
        {
            return "Usuario o Contraseña incorrectos.";
        }
        public static string GetUsuarioExiste()
        {
            return "Nombre de Usuario ya existe.";
        }
        public static string GetTokenInvalido()
        {
            return "Token de acceso o token de actualización no válido.";
        }

        public static string RandomTokenStrin()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
        public static string GetIpAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());

            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }

            return string.Empty;
        }
    }
}
