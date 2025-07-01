using System.Security.Claims;
using ApiDonAppBle.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiDonAppBlea.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PublicacionController : ControllerBase
{
    private readonly DataContext _context;
    private readonly IWebHostEnvironment _environment;

    public PublicacionController(DataContext contexto, IWebHostEnvironment environment)
    {
        _context = contexto;
        _environment = environment;
    }

    [HttpGet]
    public IActionResult GetPublicaciones()
    {
        var publicaciones = _context.Publicacion
            .Include(p => p.Usuario)
            .Where(p => p.Disponibilidad == true)
            .OrderByDescending(p => p.Fecha)
            .Select(p => new PublicacionDTO
            {
                IdPublicacion = p.IdPublicacion,
                Titulo = p.Titulo,
                Descripcion = p.Descripcion,
                Fecha = p.Fecha,
                FotoPublicacion = p.FotoPublicacion,
                Disponibilidad = p.Disponibilidad,
                Categoria = p.Categoria.ToString(),
                Usuario = new UsuarioDTO
                {
                    Nombre = p.Usuario.Nombre,
                    Apellido = p.Usuario.Apellido
                }
            })
            .ToList();

        return Ok(publicaciones);
    }



    [HttpGet("{id}")]
    public ActionResult<PublicacionDTO> DetallesPublicacion(int id)
    {
        var publicacion = _context.Publicacion
            .Include(p => p.Usuario)
            .FirstOrDefault(p => p.IdPublicacion == id);

        if (publicacion == null)
            return NotFound();

        var publicaionView = new PublicacionDTO
        {
            IdPublicacion = publicacion.IdPublicacion,
            Titulo = publicacion.Titulo,
            Descripcion = publicacion.Descripcion,
            Fecha = publicacion.Fecha,
            FotoPublicacion = publicacion.FotoPublicacion,
            Disponibilidad = publicacion.Disponibilidad,
            Categoria = publicacion.Categoria.ToString(),
            IdUsuario = publicacion.Usuario.IdUsuario,
            Usuario = new UsuarioDTO
            {

                Nombre = publicacion.Usuario?.Nombre,
                Apellido = publicacion.Usuario?.Apellido
            }
        };

        return Ok(publicaionView);
    }


    [HttpGet("MisPublicaciones")]
    [Authorize]
    public IActionResult MisPublicaciones()
    {
        var usuario = _context.Usuario.FirstOrDefault(x => x.Email == User.Identity.Name);

        if (usuario == null)
        {
            return Unauthorized();
        }

        var misPublicaciones = _context.Publicacion
            .Where(x => x.IdUsuario == usuario.IdUsuario)
            .Include(p => p.Usuario)
            .Select(p => new PublicacionDTO
            {
                IdPublicacion = p.IdPublicacion,
                Titulo = p.Titulo,
                Descripcion = p.Descripcion,
                Fecha = p.Fecha,
                FotoPublicacion = p.FotoPublicacion,
                Disponibilidad = p.Disponibilidad,
                Categoria = p.Categoria.ToString(),
                Usuario = new UsuarioDTO
                {
                    Nombre = p.Usuario.Nombre,
                    Apellido = p.Usuario.Apellido
                }
            })
            .ToList();

        return Ok(misPublicaciones);
    }


    [Authorize]
    [HttpPost("cargarPublicacion")]
    public async Task<IActionResult> CargarPublicacion(
        [FromForm] string titulo,
        [FromForm] string descripcion,
        [FromForm] string categoria,
        [FromForm] IFormFile fotoPublicacion)
    {
        var usuarioLogueado = _context.Usuario.FirstOrDefault(x => x.Email == User.Identity.Name);

        if (usuarioLogueado == null)
            return Unauthorized("Usuario no autorizado.");

        try
        {
            var publicacion = new Publicacion
            {
                IdUsuario = usuarioLogueado.IdUsuario,
                Titulo = titulo,
                Descripcion = descripcion,
                Fecha = DateTime.Today,
                Disponibilidad = true,
                Estado = Estado.Publica,
                Categoria = Enum.Parse<CategoriaEnum>(categoria)
            };

            if (fotoPublicacion != null)
            {
                string wwwPath = _environment.WebRootPath;
                string uploadsPath = Path.Combine(wwwPath, "Uploads");

                if (!Directory.Exists(uploadsPath))
                    Directory.CreateDirectory(uploadsPath);

                string fileName = $"fotoPublicacion_{Guid.NewGuid():N}_{usuarioLogueado.IdUsuario}{Path.GetExtension(fotoPublicacion.FileName)}";
                string fullPath = Path.Combine(uploadsPath, fileName);

                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await fotoPublicacion.CopyToAsync(stream);
                }

                publicacion.FotoPublicacion = Path.Combine("Uploads", fileName);
            }

            await _context.Publicacion.AddAsync(publicacion);
            await _context.SaveChangesAsync();

            return Ok(publicacion);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { mensaje = ex.Message });
        }
    }


    [Authorize]
    [HttpPut("editarPublicacion/{id}")]
    public async Task<IActionResult> EditarPublicacion(
        int id,
        [FromForm] string titulo,
        [FromForm] string descripcion,
        [FromForm] string disponibilidad,
        [FromForm] string categoria,
        [FromForm] IFormFile? FotoPublicacionIFormFile)
    {
        var usuarioLogueado = _context.Usuario.FirstOrDefault(x => x.Email == User.Identity.Name);

        if (usuarioLogueado == null)
            return Unauthorized("Usuario no autorizado.");

        var publicacion = await _context.Publicacion.FindAsync(id);
        if (publicacion == null)
            return NotFound("Publicación no encontrada.");

        if (publicacion.IdUsuario != usuarioLogueado.IdUsuario)
            return BadRequest("No puedes editar esta publicación.");

        try
        {
            publicacion.Titulo = titulo;
            publicacion.Descripcion = descripcion;
            publicacion.Disponibilidad = bool.Parse(disponibilidad);
            publicacion.Categoria = Enum.Parse<CategoriaEnum>(categoria);

            if (FotoPublicacionIFormFile != null)
            {
                string wwwPath = _environment.WebRootPath;
                string uploadsPath = Path.Combine(wwwPath, "Uploads");

                if (!Directory.Exists(uploadsPath))
                    Directory.CreateDirectory(uploadsPath);

                string fileName = $"fotoPublicacion_{Guid.NewGuid():N}_{usuarioLogueado.IdUsuario}{Path.GetExtension(FotoPublicacionIFormFile.FileName)}";
                string fullPath = Path.Combine(uploadsPath, fileName);

                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await FotoPublicacionIFormFile.CopyToAsync(stream);
                }

                publicacion.FotoPublicacion = Path.Combine("Uploads", fileName);
            }

            await _context.SaveChangesAsync();


            return Ok(new
            {
                id = publicacion.IdPublicacion,
                titulo = publicacion.Titulo,
                descripcion = publicacion.Descripcion,
                fotoPublicacion = publicacion.FotoPublicacion,
                disponibilidad = publicacion.Disponibilidad,
                categoria = publicacion.Categoria
            });


        }
        catch (Exception ex)
        {
            return StatusCode(500, new { mensaje = "Error interno: " + ex.Message });
        }
    }


    [HttpPut("cambioEstadoPublicacion/{id}")]
    [Authorize]
    public async Task<IActionResult> CambioEstadoPublicacion(int id)
    {


        var usuario = _context.Usuario.FirstOrDefault(x => x.Email == User.Identity.Name);
        var publicacionEncontrada = await _context.Publicacion
            .FirstOrDefaultAsync(p => p.IdPublicacion == id);

        if (publicacionEncontrada == null)
        {
            return NotFound("Publicacion no encontrada");
        }

        if (usuario == null || publicacionEncontrada.IdUsuario != usuario.IdUsuario)
        {
            return Forbid("No tienes permisos para cambiar el estado de esta publicación.");
        }

        publicacionEncontrada.Estado = publicacionEncontrada.Estado == Estado.Publica ? Estado.Privada : Estado.Publica;
        _context.Publicacion.Update(publicacionEncontrada);
        await _context.SaveChangesAsync();

        return Ok(new { estado = publicacionEncontrada.Estado });

    }


    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> EliminarPublicacion(int id)
    {
        var usuario = await _context.Usuario
            .FirstOrDefaultAsync(u => u.Email == User.Identity.Name);

        if (usuario == null)
        {
            return Unauthorized("Usuario no válido");
        }

        var publicacionEncontrada = await _context.Publicacion
            .FirstOrDefaultAsync(p => p.IdPublicacion == id && p.IdUsuario == usuario.IdUsuario);

        if (publicacionEncontrada == null)
        {
            return NotFound("Publicación no encontrada o no pertenece al usuario actual");
        }

        _context.Publicacion.Remove(publicacionEncontrada);
        await _context.SaveChangesAsync();

        return Ok(new { mensaje = "Publicación eliminada correctamente" });
    }


}