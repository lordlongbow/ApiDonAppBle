using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using ApiDonAppBle.Models;

namespace ApiDonAppBle.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MensajePrivadoController : ControllerBase
    {
        private readonly DataContext _context;

        public MensajePrivadoController(DataContext context)
        {
            _context = context;
        }

    [HttpPost("{idReceptor}")]
[Authorize]
public async Task<IActionResult> EnviarMensajePrivado(int idReceptor, [FromForm] string texto)
{
    Console.WriteLine($"[DEBUG] Llegó solicitud a EnviarMensajePrivado");
    Console.WriteLine($"[DEBUG] idReceptor recibido: {idReceptor}");
    Console.WriteLine($"[DEBUG] Texto recibido: {texto}");

    var emisor = await _context.Usuario.FirstOrDefaultAsync(u => u.Email == User.Identity.Name);
    if (emisor == null)
    {
        Console.WriteLine("[DEBUG] No se pudo identificar al emisor");
        return Unauthorized();
    }

    var receptor = await _context.Usuario.FindAsync(idReceptor);
    if (receptor == null)
    {
        Console.WriteLine("[DEBUG] No se encontró al receptor con id: " + idReceptor);
        return NotFound("Receptor no encontrado");
    }

    var mensaje = new MensajePrivado
    {
        Texto = texto,
        Fecha = DateTime.Now,
        IdEmisor = emisor.IdUsuario,
        IdReceptor = idReceptor
    };

    _context.MensajePrivado.Add(mensaje);
    await _context.SaveChangesAsync();

    Console.WriteLine("[DEBUG] Mensaje guardado correctamente");

    return Ok();
}

        [HttpGet("{idOtroUsuario}")]
        [Authorize]
        public IActionResult ObtenerMensajesPrivados(int idOtroUsuario)
        {
            var usuarioLogueado = _context.Usuario.FirstOrDefault(u => u.Email == User.Identity.Name);
            if (usuarioLogueado == null)
                return Unauthorized();

            var mensajes = _context.MensajePrivado
                .Where(m =>
                    (m.IdEmisor == usuarioLogueado.IdUsuario && m.IdReceptor == idOtroUsuario) ||
                    (m.IdEmisor == idOtroUsuario && m.IdReceptor == usuarioLogueado.IdUsuario))
                .Include(m => m.Emisor)
                .OrderBy(m => m.Fecha)
                .Select(m => new MensajePrivadoDTO
                {
                    Texto = m.Texto,
                    EmisorNombre = m.Emisor.Nombre + " " + m.Emisor.Apellido,
                    Fecha = m.Fecha
                })
                .ToList();

            return Ok(mensajes);
        }
    }
}
