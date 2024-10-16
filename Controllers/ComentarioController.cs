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
        public IActionResult TraerComentarios(int IdPublicacion){
            
            var comentarios = _contexto.Comentario
                .Where(c => c.IdPublicacion == IdPublicacion)
                .ToList();
            return Ok(comentarios);
        }
        //crear Comentario
        [HttpPost("comentar/{idPublicacion}")]
        [Authorize]
        public IActionResult CrearComentario(int IdPublicacion, [FromForm] Comentario comentario){
            var usuarioLogueado = _contexto.Usuario
                .Where(u => u.Email == User.Identity.Name)
                .FirstOrDefault();
            if (usuarioLogueado == null || !User.Identity.IsAuthenticated)
            {   
                return BadRequest("No tienes credenciales para realizar esta accion");
            }

            var publicacion = _contexto.Publicacion
                .Where(p => p.IdPublicacion == IdPublicacion)
                .FirstOrDefault();

            if (publicacion == null)
            {
                return NotFound("Publicacion no encontrada");
            }
            
            Comentario comentarios = new Comentario();
            comentarios.Texto = comentario.Texto;
            comentarios.IdPublicacion = publicacion.IdPublicacion;
            comentarios.IdUsuario = usuarioLogueado.IdUsuario;
            comentarios.Fecha = DateTime.Now;

            _contexto.Comentario.Add(comentarios);
            _contexto.SaveChanges();
            return Ok(comentario);
        }

        //Editar Comentario
        [HttpPut("{IdComenatario}")]
        [Authorize]
        public IActionResult EditarComentario(int IdComenatario, [FromForm] Comentario comentario){
            var usuarioLogueado = _contexto.Usuario
                .Where(u => u.Email == User.Identity.Name)
                .FirstOrDefault();
            if (usuarioLogueado == null || !User.Identity.IsAuthenticated)
            {   
                return BadRequest("No tienes credenciales para realizar esta accion");
            }

            var coment = _contexto.Comentario
                .Where(c => c.IdComentario == IdComenatario)
                .FirstOrDefault();



            if(coment == null)
            {
                return NotFound("Comentario no econtrado");
            }
            else if(coment.IdUsuario != usuarioLogueado.IdUsuario)
            {
                return BadRequest("No tienes credenciales para realizar esta accion");
            }
            else
            {
                coment.Texto = comentario.Texto;
                coment.Fecha = DateTime.Now;
                _contexto.SaveChanges();    
            }
            return Ok(comentario);
            }
        
        //Eliminar Comentario
        [HttpDelete("{IdComenatario}")]
        [Authorize]
        public IActionResult BorrarComentario(int IdComenatario){
            var usuarioLogueado = _contexto.Usuario
                .Where(u => u.Email == User.Identity.Name)
                .FirstOrDefault();
            if (usuarioLogueado == null || !User.Identity.IsAuthenticated)
            {   
                return BadRequest("No tienes credenciales para realizar esta accion");
            }

            var comentario = _contexto.Comentario
                .Where(c => c.IdComentario == IdComenatario)
                .FirstOrDefault();
            if(comentario== null)
            {
                return NotFound("Comentario no econtrado");
            }
              else if(comentario.IdUsuario != usuarioLogueado.IdUsuario)
            {
                return BadRequest("No tienes credenciales para realizar esta accion");
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
