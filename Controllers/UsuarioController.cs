
using Microsoft.AspNetCore.Mvc;
using ApiDonAppBle.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace ApiDonAppBle.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsuarioController : ControllerBase
    {
        private readonly DataContext _contexto;
        private readonly IConfiguration _config;
        private readonly IWebHostEnvironment _environment;

        public UsuarioController(
            DataContext contexto,
            IConfiguration config,
            IWebHostEnvironment environment
        )
        {
            _contexto = contexto;
            _config = config;
            _environment = environment;
        }

        // Registro de usuario
        [HttpPost("Registrar")]
        public IActionResult Registro([FromBody] Usuario user)
        {
            try
            {
                var hashed = Convert.ToBase64String(
                    KeyDerivation.Pbkdf2(
                        password: user.Password,
                        salt: Encoding.ASCII.GetBytes(_config["Salt"]),
                        prf: KeyDerivationPrf.HMACSHA1,
                        iterationCount: 1000,
                        numBytesRequested: 256 / 8
                    )
                );
                user.Password = hashed;
                _contexto.Add(user);
                _contexto.SaveChanges();

                return Ok(new { mensaje = "Usuario registrado correctamente" });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Login de usuario
        [HttpPost("login")]
        public IActionResult Login([FromForm] LoginView lv)
        {
            var usuario = _contexto.Usuario.FirstOrDefault(u => u.Email == lv.Email);

            if (usuario == null)
            {
                return BadRequest("Credenciales incorrectas");
            }

            var hashed = Convert.ToBase64String(
                KeyDerivation.Pbkdf2(
                    password: lv.Password,
                    salt: Encoding.ASCII.GetBytes(_config["Salt"]),
                    prf: KeyDerivationPrf.HMACSHA1,
                    iterationCount: 1000,
                    numBytesRequested: 256 / 8
                )
            );

            if (usuario.Password != hashed)
            {
                return BadRequest("Credenciales incorrectas");
            }

            var token = GenerarToken(usuario.Email);

            return Ok(new { token = token });
        }

        private string GenerarToken(string email)
        {
            var usuario = _contexto.Usuario.FirstOrDefault(u => u.Email == email);
            if (usuario == null) return null;

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, usuario.Email),
                new Claim("FullName", $"{usuario.Nombre} {usuario.Apellido}"),
                new Claim(ClaimTypes.NameIdentifier, usuario.IdUsuario.ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var credenciales = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(3),
                signingCredentials: credenciales
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

     
        [HttpGet("miPerfil")]
        [Authorize]
        public IActionResult MiPerfil()
        {
            var email = User.Identity.Name;
            var usuario = _contexto.Usuario.FirstOrDefault(u => u.Email == email);

            if (!User.Identity.IsAuthenticated || usuario == null)
            {
                return Unauthorized("No autorizado o usuario no encontrado");
            }

            return Ok(usuario);
        }

        // Editar perfil
        [HttpPut("EditarPerfil")]
        [Authorize]
        public IActionResult EditarPerfil(
     [FromForm(Name = "idUsuario")] int idUsuario,
     [FromForm] string Nombre,
     [FromForm] string Apellido,
     [FromForm] string Direccion)
        {
            int IdUsuario = idUsuario;

            var email = User.Identity.Name;
            var usuario = _contexto.Usuario.FirstOrDefault(u => u.Email == email && u.IdUsuario == IdUsuario);

           


            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized("No autorizado");
            }



            if (usuario == null)
            {
                return NotFound("Usuario no encontrado");
            }


           

            usuario.Nombre = Nombre;
            usuario.Apellido = Apellido;
            usuario.Direccion = Direccion;

            _contexto.Entry(usuario).State = EntityState.Modified;
            _contexto.SaveChanges();

    
            return Ok(usuario);
        }

       
        [HttpPost("cambiopassword")]
        [Authorize]
        public async Task<IActionResult> CambioPassword([FromForm] Usuario usuarioEntrada)
        {
            var usuario = await _contexto.Usuario
                .FirstOrDefaultAsync(u => u.Email == User.Identity.Name && u.IdUsuario == usuarioEntrada.IdUsuario);

            if (usuario == null || !User.Identity.IsAuthenticated)
            {
                return BadRequest("No autorizado o datos inválidos");
            }

            var hashed = Convert.ToBase64String(
                KeyDerivation.Pbkdf2(
                    password: usuarioEntrada.Password,
                    salt: Encoding.ASCII.GetBytes(_config["Salt"]),
                    prf: KeyDerivationPrf.HMACSHA1,
                    iterationCount: 1000,
                    numBytesRequested: 256 / 8
                )
            );

            usuario.Password = hashed;
            await _contexto.SaveChangesAsync();
            return Ok("Contraseña actualizada");
        }

        // Actualizar foto de perfil
        [HttpPut("actualizar/foto/{id}")]
        [Authorize]
        public async Task<IActionResult> ActualizarFotoPerfil(int id, [FromForm] IFormFile foto)
        {

            

            var usuario = await _contexto.Usuario.FirstOrDefaultAsync(
                u => u.Email == User.Identity.Name && u.IdUsuario == id
            );

            if (usuario == null || foto == null)
            {
                return BadRequest("Datos inválidos");
            }

            string wwwPath = _environment.WebRootPath;
            string fotosDir = Path.Combine(wwwPath, "fotos");

            if (!Directory.Exists(fotosDir))
            {
                Directory.CreateDirectory(fotosDir);
            }

            string fileName = $"fotoperfil{usuario.IdUsuario}{Path.GetExtension(foto.FileName)}";
            string fullPath = Path.Combine(fotosDir, fileName);


            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await foto.CopyToAsync(stream);
            }

            usuario.Avatar = Path.Combine("/fotos", fileName);
            _contexto.Usuario.Update(usuario);

            await _contexto.SaveChangesAsync();


            return Ok(usuario);
        }

    }
}
