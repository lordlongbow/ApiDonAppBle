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
    public class PublicacionController : ControllerBase
    {
        private readonly DataContext _contexto;
        private readonly IConfiguration _config;
        private readonly IWebHostEnvironment _environment;

        public PublicacionController(
            DataContext contexto,
            IConfiguration config,
            IWebHostEnvironment environment
        )
        {
            _contexto = contexto;
            _config = config;
            _environment = environment;
        }

        //CRUD de las publicaciones

        //ver publicaciones sin estar logueado

        [HttpGet]
        [AllowAnonymous]
        public IActionResult VerTodas()
        {
            var publicacionesPublicas = _contexto.Publicacion
                .Where(p => p.Estado == Estado.Publica)
                .ToList();

            if (publicacionesPublicas.Any())
            {
                return Ok(publicacionesPublicas);
            }

            return NotFound("No se encontraron publicaciones públicas.");
        }

        //ver Todas las publicaciones estando logueado

        [HttpGet]
        [Authorize]
        public IActionResult VerTodas([FromBody] Usuario usuarioLogueago)
        {
            try
            {
                var usuario = _contexto.Usuario
                    .Where(u => u.Email == User.Identity.Name)
                    .FirstOrDefault();
                if (usuario == null)
                {
                    return BadRequest("Debes Loguearte");
                }
                var publicaciones = _contexto.Publicacion.ToList();
                return Ok(publicaciones);
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        //ver detalle de publicacion

        [HttpGet]
        [Authorize]
        public IActionResult DetallePublicacion(
            [FromBody] Usuario usuarioLogueago,
            int idpublicacion
        )
        {
            return Ok();
        }

        //agregar publicacion
        [HttpPost]
        [Authorize]
        public IActionResult CargarPublicacion(
            [FromForm] Publicacion publicacion,
            [FromForm] IFormFile fotoPublicacion,
            [FromForm] IFormFile videoPublicacion
        )
        {
            //verificar que es el usuario logueado
            var usuarioLogueado = _contexto.Usuario
                .Where(u => u.Email == User.Identity.Name)
                .FirstOrDefault();
            //cargar los datos que me vienen del formulario
            if (usuarioLogueado.Email == User.Identity.Name && User.Identity.IsAuthenticated)
            {
                publicacion.IdUsuario = usuarioLogueado.IdUsuario;
                publicacion.Fecha = DateTime.Today;
                publicacion.Disponibilidad = true;
                if (publicacion.Estado != Estado.Publica)
                {
                    publicacion.Estado = Estado.Privada;
                }

                //ver si es foto o video

                if (fotoPublicacion != null)
                {
                    string wwwPath = _environment.WebRootPath;
                    string path = Path.Combine(wwwPath, "Uploads");
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                    string fileName =
                        "fotoPublicacion_"
                        + usuarioLogueado.IdUsuario
                        + Path.GetExtension(fotoPublicacion.FileName);
                    string pathCompleto = Path.Combine(path, fileName);
                    using (FileStream stream = new FileStream(pathCompleto, FileMode.Create))
                    {
                        fotoPublicacion.CopyTo(stream);
                    }
                    publicacion.FotoPublicacion = Path.Combine("/Uploads", fileName);
                }

                if (videoPublicacion != null)
                {
                    string wwwPath = _environment.WebRootPath;
                    string path = Path.Combine(wwwPath, "Uploads");
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                    string fileName =
                        "videoPublicacion_"
                        + usuarioLogueado.IdUsuario
                        + Path.GetExtension(videoPublicacion.FileName);
                    string pathCompleto = Path.Combine(path, fileName);
                    using (FileStream stream = new FileStream(pathCompleto, FileMode.Create))
                    {
                        videoPublicacion.CopyTo(stream);
                    }
                    publicacion.VideoPublicacion = Path.Combine("/Uploads", fileName);
                }

                //guardar la publicaicon
                _contexto.Publicacion.Add(publicacion);
                _contexto.SaveChanges();
                return Ok(publicacion);
            }

            return BadRequest();
        }

        //Editar una publicaciones

        [HttpPut("actualizarPublicacion/{id}")]
        [Authorize]
        public IActionResult ActualizarPublicacion(int id, [FromBody] Publicacion publicacion)
        {
            // Verificar el usuario
            var usuarioLogueado = _contexto.Usuario.SingleOrDefault(
                u => u.Email == User.Identity.Name
            );

            if (usuarioLogueado == null || !User.Identity.IsAuthenticated)
            {
                return Unauthorized("Usuario no autorizado.");
            }

            // Buscar la publicación
            var publicacionDb = _contexto.Publicacion.SingleOrDefault(p => p.IdPublicacion == id);

            if (publicacionDb == null)
            {
                return NotFound("Publicación no encontrada.");
            }

            if (publicacionDb.IdUsuario != usuarioLogueado.IdUsuario)
            {
                return Forbid("No tiene permiso para actualizar esta publicación.");
            }

            // Actualizar los datos
            publicacionDb.Titulo = publicacion.Titulo;
            publicacionDb.Fecha = DateTime.Now;

            if (!string.IsNullOrEmpty(publicacion.FotoPublicacion))
            {
                publicacionDb.FotoPublicacion = publicacion.FotoPublicacion;
            }

            if (!string.IsNullOrEmpty(publicacion.VideoPublicacion))
            {
                publicacionDb.VideoPublicacion = publicacion.VideoPublicacion;
            }

            // Guardar los cambios
            _contexto.Publicacion.Update(publicacionDb);
            _contexto.SaveChanges();

            // Retornar el resultado
            return Ok(publicacionDb);
        }

        //borrar una publicacion
        [HttpDelete("EliminarPubliacion")]
        [Authorize]
        public IActionResult BorrarPublicacion(int id)
        {
            //controlar que sea el usuario correcto
            var usuarioLogueado = _contexto.Usuario
                .Where(u => u.Email == User.Identity.Name)
                .FirstOrDefault();

            if (usuarioLogueado == null || !User.Identity.IsAuthenticated)
            {
                return BadRequest("No tienes credenciales para realizar esta accion");
            }

            //buscar si existe la publicacion

            var publicacionDb = _contexto.Publicacion.SingleOrDefault(
                p =>
                    p.IdPublicacion == id
                    && User.Identity.Name == usuarioLogueado.Email
                    && User.Identity.IsAuthenticated
            );

            if (publicacionDb == null)
            {
                return BadRequest("Publicacion No existente");
            }

            //eliminar la publicaicion y retornar

            _contexto.Publicacion.Remove(publicacionDb);
            _contexto.SaveChanges();

            return Ok();
        }

        //ver MIS publicaciones

        [HttpGet]
        [Authorize("MisPublicaciones")]
        public IActionResult MisPublicaciones([FromBody] Usuario usuarioLogueado)
        {
            var usuario = _contexto.Usuario.SingleOrDefault(
                u => u.Email == User.Identity.Name && User.Identity.IsAuthenticated
            );

            if (usuario == null)
            {
                return Unauthorized("Debes Loguearte");
            }
            var misPublicaciones = _contexto.Publicacion
                .Where(p => p.IdUsuario == usuario.IdUsuario)
                .ToList();

            if (misPublicaciones.Count > 0)
            {
                return Ok(misPublicaciones);
            }
            return NotFound("Aun no cargas Publicacaiones");
        }

        [HttpPut("CambiarDisponibilidad/{id}")]
        [Authorize]
        public IActionResult CambiarDisponibilidad(int id)
        {
            var publicacionACambiar = _contexto.Publicacion.SingleOrDefault(
                publicacion => publicacion.IdPublicacion == id
            );

            if (publicacionACambiar == null)
            {
                return NotFound("Publicación no encontrada.");
            }

            publicacionACambiar.Disponibilidad = !publicacionACambiar.Disponibilidad;

            _contexto.SaveChanges();

            return Ok(publicacionACambiar);
        }
    }
}
