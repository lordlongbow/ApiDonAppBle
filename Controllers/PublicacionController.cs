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
    public ActionResult<IEnumerable<Publicacion>> GetPublicaciones()
    {

        var publicaciones = _context.Publicacion.ToList();

        if (publicaciones.Count != 0)
        {
            return Ok(publicaciones);
        }
        else
        {
            return NotFound("Aun no cargas Publicacaiones");
        }
    }

    [HttpGet("{id}")]
    public Publicacion DetallesPublicacion(int id)
    {

        var Publicacion = _context.Publicacion.Where(x => x.IdPublicacion == id).FirstOrDefault();
        return Publicacion;
    }

    [HttpGet("MisPublicaciones")]
    [Authorize]
    public IActionResult misPublicaciones()
    {

        var usuario = _context.Usuario.Where(x => x.Email == User.Identity.Name).FirstOrDefault();

        var misPublicaciones = _context.Publicacion.Where(x => x.IdUsuario == usuario.IdUsuario).ToList();
        if (misPublicaciones.Count != 0)
        {
            return Ok(misPublicaciones);
        }
        else
        {
            return NotFound("Aun no cargas Publicacaiones");
        }

    }

    [HttpPost("cargarPublicacion")]
    [Authorize]
    public async Task<IActionResult> CargarPublicacion([FromForm] Publicacion publicacion)
    {
        // Obtén el usuario autenticado
        var usuarioLogueado = _context.Usuario.FirstOrDefault(x => x.Email == User.Identity.Name);

        if (usuarioLogueado == null)
        {
            return Unauthorized("Usuario no autorizado.");
        }

        try
        {
            // Cargar los datos del formulario
            publicacion.IdUsuario = usuarioLogueado.IdUsuario;
            publicacion.Fecha = DateTime.Today;
            publicacion.Disponibilidad = true;
           // publicacion.Comentarios ??= new List<Comentario>();
            publicacion.Estado = publicacion.Estado != Estado.Publica ? Estado.Privada : publicacion.Estado;
            publicacion.Etiquetas ??= new List<Etiqueta>();

            // Procesar foto
            if (publicacion.FotoPublicacionIFormFile != null)
            {
                string wwwPath = _environment.WebRootPath;
                string path = Path.Combine(wwwPath, "Uploads");
                Console.WriteLine(path);

                try
                {
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                }
                catch (Exception ex)
                {
                    return BadRequest(new { messagess = $"Error al crear la carpeta: {ex.Message}" });
                }

                string fileName = $"fotoPublicacion_{Guid.NewGuid().ToString("N")}{usuarioLogueado.IdUsuario}{Path.GetExtension(publicacion.FotoPublicacionIFormFile.FileName)}";
                string pathCompleto = Path.Combine(path, fileName);

                using (var stream = new FileStream(pathCompleto, FileMode.Create))
                {
                    await publicacion.FotoPublicacionIFormFile.CopyToAsync(stream);
                }

                publicacion.FotoPublicacion = Path.Combine("/Uploads", fileName);
            }
            else
            {
                publicacion.FotoPublicacion = null;
            }

            // Guardar la publicación
            await _context.Publicacion.AddAsync(publicacion);
            await _context.SaveChangesAsync();

            return Ok(publicacion);
        }
        catch (Exception ex)
        {
            return BadRequest(new { messagerr = ex.Message });
        }
    }


    [HttpPut("editarPublicacion/{id}")]
    [Authorize]
    public async Task<IActionResult> EditarPublicacion(int id, [FromForm] Publicacion publicacion)
    {
        var publicacionEncontrada = await _context.Publicacion
            .FirstOrDefaultAsync(p => p.IdPublicacion == id);

        if (publicacionEncontrada == null)
        {
            return NotFound("Publicacion no encontrada");
        }

        if (publicacion.Titulo == null)
        {
            publicacionEncontrada.Titulo = publicacionEncontrada.Titulo;
        }
        else
        {
            publicacionEncontrada.Titulo = publicacion.Titulo;
        }
        if (publicacion.FotoPublicacionIFormFile != null)
        {
            string wwwPath = _environment.WebRootPath;
            string path = Path.Combine(wwwPath, "Uploads");
            Console.WriteLine(path);
        }
        else
        {
            publicacionEncontrada.FotoPublicacion = publicacion.FotoPublicacion;
        }
        if (publicacion.Disponibilidad == null)
        {
            publicacionEncontrada.Disponibilidad = publicacion.Disponibilidad;
        }
        else
        {
            publicacionEncontrada.Disponibilidad = publicacionEncontrada.Disponibilidad;
        }


        _context.Publicacion.Update(publicacionEncontrada);
        await _context.SaveChangesAsync();

        return Ok(publicacionEncontrada);
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
        var publicacionEncontrada = await _context.Publicacion
            .FirstOrDefaultAsync(p => p.IdPublicacion == id);   

        if (publicacionEncontrada == null)
        {
            return NotFound("Publicacion no encontrada");
        }

        _context.Publicacion.Remove(publicacionEncontrada);
        await _context.SaveChangesAsync();

        return Ok("Publicacion eliminada");
    }


}