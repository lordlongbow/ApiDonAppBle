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
        public IActionResult Registro([FromBody] Usuario User)
        {
            try
            {
                var hashed = Convert.ToBase64String(
                    KeyDerivation.Pbkdf2(
                        password: User.Password,
                        salt: Encoding.ASCII.GetBytes(_config["Salt"]),
                        prf: KeyDerivationPrf.HMACSHA1,
                        iterationCount: 1000,
                        numBytesRequested: 256 / 8
                    )
                );
                User.Password = hashed;
                _contexto.Add(User);
                _contexto.SaveChanges();
                return Ok(User);
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
                return BadRequest("Credenciales Incorrectas");
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
            return Ok(new { Token = token });
        }

        // Obtener perfil del usuario autenticado
        [HttpGet("miPerfil")]
        [Authorize]
        public IActionResult MiPerfil()
        {
            try
            {
                var email = User.Identity.Name;
                var usuario = _contexto.Usuario.FirstOrDefault(u => u.Email == email);

                if (usuario == null)
                {
                    return NotFound();
                }

                return Ok(usuario);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Edita el perfil del usuario autenticado
        [HttpPut("EditarPerfil")]
        [Authorize]
        public IActionResult EditarPerfil([FromBody] Usuario usuarioEditar)
        {
            try
            {
                var email = User.Identity.Name;
                var usuario = _contexto.Usuario.FirstOrDefault(u => u.Email == email);

                if (usuario == null)
                {
                    return NotFound();
                }

                usuario.Nombre = usuarioEditar.Nombre;
                usuario.Apellido = usuarioEditar.Apellido;
                usuario.Direccion = usuarioEditar.Direccion;

                _contexto.Entry(usuario).State = EntityState.Modified;
                _contexto.SaveChanges();

                return Ok(usuario);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Esta funcion genera el token y retornar un token
        private string GenerarToken(string username)
        {
            var usuario = _contexto.Usuario.FirstOrDefault(u => u.Email == username);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, usuario.Email),
                new Claim("FullName", usuario.Nombre + " " + usuario.Apellido)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var credenciales = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(30000),
                signingCredentials: credenciales
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        //Editar foto de Usuario
        [HttpPut("actualizar/foto/{id}")]
        [Authorize]
        public async Task<IActionResult> ActualizarFotoPerfil(int id, [FromForm] IFormFile foto)
        {
            var usuarioLogueago = await _contexto.Usuario.FirstOrDefaultAsync(
                u => u.Email == User.Identity.Name && u.IdUsuario == id
            );

            if (usuarioLogueago == null)
            {
                return BadRequest("Datos incorrectos");
            }
            if (foto == null)
            {
                return BadRequest("Datos incorrectos");
            }

            string wwwPath = _environment.WebRootPath;
            string path = Path.Combine(wwwPath, "fotos", foto.FileName);

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            string FileName =
                "fotoperfil" + usuarioLogueago.IdUsuario + Path.GetExtension(foto.FileName);
            string nombreFoto = Path.Combine(path, FileName);
            usuarioLogueago.Avatar = Path.Combine("/fotos", nombreFoto);
            _contexto.Usuario.Update(usuarioLogueago);
            await _contexto.SaveChangesAsync();
            return Ok(usuarioLogueago);
        }

        [HttpPost("cambiopassword")]
        [Authorize]
        public IActionResult cambiopassword([FromForm] Usuario usuarioLogueago)
        {
            return Ok();
        }
    }
}
