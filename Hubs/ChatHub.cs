using Microsoft.AspNetCore.SignalR;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace ApiDonAppBle.Hubs
{
    public class ChatHub : Hub
    {
        private readonly IConfiguration _config;

        public ChatHub(IConfiguration config)
        {
            _config = config;
        }

        public async Task UnirseAlGrupo(string userId, string token)
        {
            if (ValidarToken(token))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, userId);
            }
            else
            {
                await Clients.Caller.SendAsync("Error", "Token inválido, no se pudo unir al grupo.");
            }
        }

        public async Task MensajeAUsuario(string destinatarioId, string emisor, string mensaje, string token)
        {
            if (ValidarToken(token))
            {
                await Clients.Group(destinatarioId).SendAsync("RecibirMensaje", emisor, mensaje);
            }
            else
            {
                await Clients.Caller.SendAsync("Error", "Token inválido, no se pudo enviar el mensaje.");
            }
        }

        private bool ValidarToken(string token)
        {
            if (string.IsNullOrEmpty(token)) return false;

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_config["Jwt:Key"]);

            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidIssuer = _config["Jwt:Issuer"],
                    ValidAudience = _config["Jwt:Audience"],
                    ValidateLifetime = true
                }, out SecurityToken validatedToken);

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
