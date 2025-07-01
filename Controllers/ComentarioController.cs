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

namespace ApiDonAppBle
{
    [ApiController]
    [Route("api/[controller]")]
    public class ComentarioController : ControllerBase
    {
        private readonly DataContext _contexto;
        private readonly IConfiguration _config;
        private readonly IWebHostEnvironment _enviroment;

        public ComentarioController(
            DataContext contexto,
            IConfiguration config,
            IWebHostEnvironment enviroment
        )
        {
            _contexto = contexto;
            _config = config;
            _enviroment = enviroment;
        }

        //traer Comentarios de una publicacion en particular

        [HttpGet("comentarios/{IdPublicacion}")]
        public IActionResult TraerComentarios(int IdPublicacion)
        {
            var comentarios = _contexto.Comentario
                .Where(c => c.IdPublicacion == IdPublicacion)
                .Include(c => c.Usuario)
                .OrderByDescending(c => c.Fecha)
                .Select(c => new ComentarioDTO
                {
                    IdComentario = c.IdComentario,
                    Texto = c.Texto,
                    Fecha = c.Fecha,
                    NombreUsuario = c.Usuario.Nombre + " " + c.Usuario.Apellido,
                    IdUsuario = c.Usuario.IdUsuario
                })
                .ToList();


            return Ok(comentarios);
        }



        [HttpPost("comentar/{idPublicacion}")]
        [Authorize]
        public async Task<IActionResult> CrearComentario(int idPublicacion, [FromForm] string texto)
        {
            var usuarioLogueado = await _contexto.Usuario
                .FirstOrDefaultAsync(u => u.Email == User.Identity.Name);

            if (usuarioLogueado == null || !User.Identity.IsAuthenticated)
            {
                return Unauthorized("No tienes credenciales para realizar esta acción");
            }

            var publicacion = await _contexto.Publicacion
                .FirstOrDefaultAsync(p => p.IdPublicacion == idPublicacion);

            if (publicacion == null)
            {
                return NotFound("Publicación no encontrada");
            }

            var comentario = new Comentario
            {
                Texto = texto,
                Fecha = DateTime.Now,
                IdPublicacion = publicacion.IdPublicacion,
                IdUsuario = usuarioLogueado.IdUsuario
            };

            _contexto.Comentario.Add(comentario);
            await _contexto.SaveChangesAsync();


            comentario.Usuario = usuarioLogueado;



            return Ok(comentario);
        }

        [HttpPut("editar/{IdComentario}")]
        [Authorize]
        public IActionResult EditarComentario(int IdComentario, [FromForm] string Texto)
        {
            var usuarioLogueado = _contexto.Usuario
                .Where(u => u.Email == User.Identity.Name)
                .FirstOrDefault();

            if (usuarioLogueado == null || !User.Identity.IsAuthenticated)
                return BadRequest("No tienes credenciales");

            var coment = _contexto.Comentario
                .FirstOrDefault(c => c.IdComentario == IdComentario);

            if (coment == null)
                return NotFound("Comentario no encontrado");

            if (coment.IdUsuario != usuarioLogueado.IdUsuario)
                return BadRequest("No tienes permiso para modificar este comentario");

            coment.Texto = Texto;
            coment.Fecha = DateTime.Now;

            _contexto.SaveChanges();

            return Ok(new ComentarioDTO
            {
                IdComentario = coment.IdComentario,
                Texto = coment.Texto,
                Fecha = coment.Fecha,
                NombreUsuario = usuarioLogueado.Nombre + " " + usuarioLogueado.Apellido,
                IdUsuario = coment.IdUsuario
            });
        }


        [HttpDelete("{IdComentario}")]
        [Authorize]
        public IActionResult BorrarComentario(int IdComentario)
        {
            var usuarioLogueado = _contexto.Usuario
                .Where(u => u.Email == User.Identity.Name)
                .FirstOrDefault();
            if (usuarioLogueado == null || !User.Identity.IsAuthenticated)
            {
                return BadRequest("No tienes credenciales para realizar esta acción");
            }

            var comentario = _contexto.Comentario
                .Where(c => c.IdComentario == IdComentario)
                .FirstOrDefault();

            if (comentario == null)
            {
                return NotFound("Comentario no encontrado");
            }
            else if (comentario.IdUsuario != usuarioLogueado.IdUsuario)
            {
                return BadRequest("No tienes permiso para eliminar este comentario");
            }
            else
            {
                _contexto.Comentario.Remove(comentario);
                _contexto.SaveChanges();
                return Ok("Comentario borrado");
            }
        }
    }
}