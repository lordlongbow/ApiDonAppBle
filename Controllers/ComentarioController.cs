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
        [HttpGet]
        public IActionResult TraerComentarios(int IdPublicacion){
            var publicacion = _contexto.Publicacion.Include(p=>p.Comentarios).FirstOrDefault(p=>p.IdPublicacion==IdPublicacion);

            if(publicacion == null){
                return NotFound("Publicacion no encontrada");
            }
            return Ok(publicacion.Comentarios);
        }
        //crear Comentario
        [HttpPost]
        [Authorize]
        public IActionResult CrearComentario([FromBody] Comentario comentario){
            var usuarioLogueado = _contexto.Usuario
                .Where(u => u.Email == User.Identity.Name)
                .FirstOrDefault();
            if (usuarioLogueado == null || !User.Identity.IsAuthenticated)
            {   
                return BadRequest("No tienes credenciales para realizar esta accion");
            }

            var publicacion = _contexto.Publicacion
                .Where(p => p.IdPublicacion == comentario.IdPublicacion)
                .FirstOrDefault();
            if (publicacion == null)
            {
                return NotFound("Publicacion no encontrada");
            }
            
            publicacion.Comentarios.Add(comentario);
            _contexto.SaveChanges();
            return Ok(comentario);
        }

        //Editar Comentario
        [HttpPut("{IdComenatario}")]
        [Authorize]
        public IActionResult EditarComentario(int IdComenatario, [FromBody] Comentario comentario){
            var usuarioLogueado = _contexto.Usuario
                .Where(u => u.Email == User.Identity.Name)
                .FirstOrDefault();
            if (usuarioLogueado == null || !User.Identity.IsAuthenticated)
            {   
                return BadRequest("No tienes credenciales para realizar esta accion");
            }

            var publicacion = _contexto.Publicacion.Where(p => p.IdPublicacion == comentario.IdPublicacion).FirstOrDefault();
            if (publicacion == null){
                return NotFound("Publicacion no encontrada");
            }
            
            var comentarioEditado = publicacion.Comentarios
                .Where(c => c.IdComentario == IdComenatario)
                .FirstOrDefault();
            if (comentarioEditado == null){
                return NotFound("No se encuentra el comentario");
            }
            comentarioEditado.Texto = comentario.Texto;
            comentarioEditado.Fecha = DateTime.Now;
            
            _contexto.SaveChanges();
            return Ok(comentarioEditado);
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

            var publicacion = _contexto.Publicacion 
                .Where(p => p.IdPublicacion == IdComenatario)
                .FirstOrDefault();      

            if (publicacion == null){       
                return NotFound("Publicacion no encontrada");   
            }

            var comentario = publicacion.Comentarios.Where(c => c.IdComentario == IdComenatario).FirstOrDefault();
            if(comentario== null)
            {
                return NotFound("Comentario no econtrado");
            }  

            publicacion.Comentarios.Remove(comentario);
            _contexto.SaveChanges();
            return Ok(comentario);
        }
        
    }
}
