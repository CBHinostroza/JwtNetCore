using System.Net;

namespace JwtNet6.API.Helpers
{
    public class IpHelper
    {
        //CHA: Obtener la dirección IP Pública, donde se realiza peticiones
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
