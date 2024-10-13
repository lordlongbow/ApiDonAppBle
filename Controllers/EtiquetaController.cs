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
    public class EtiquetaController : ControllerBase
    {
        private readonly DataContext _contexto;
        private readonly IConfiguration _config;
        private readonly IWebHostEnvironment _enviroment;

        public EtiquetaController(
            DataContext contexto,
            IConfiguration config,
            IWebHostEnvironment enviroment
        )
        {
            _contexto = contexto;
            _config = config;
            _enviroment = enviroment;
        }

        //crear etiqueta

        [HttpPost]
        public IActionResult CrearEtiqueta([FromBody] Etiqueta nuevaEtiqueta)
        {
            
            try
            {
                _contexto.Etiqueta.Add(nuevaEtiqueta);
                _contexto.SaveChanges();
                return Ok(nuevaEtiqueta);
                
            }
            catch (System.Exception)
            {
                
            return BadRequest();
            }
        }

        //eliminar etiqueta
        [HttpDelete("{id}")]
        public IActionResult EliminarEtiqueta(int id)
        {
            var categoria = _contexto.Etiqueta.SingleOrDefault(c => c.IdEtiqueta == id);

            if (categoria == null)
            {
                return NotFound("Etiqueta no encontrada");
            }

            _contexto.Etiqueta.Remove(categoria);
            _contexto.SaveChanges();
            return Ok("Etiqueta eliminada");
        }

        //editar etiqueta
        [HttpPut("{id}")]
        public IActionResult EditarEtiqueta(int id, [FromBody] Etiqueta EtiquetaActualizada)
        {
            var etiqueta = _contexto.Etiqueta.SingleOrDefault(c => c.IdEtiqueta == id);

            if (etiqueta == null)
            {
                return NotFound("Categoría no encontrada");
            }

            etiqueta.Descripcion = EtiquetaActualizada.Descripcion;

            _contexto.Etiqueta.Update(etiqueta);
            _contexto.SaveChanges();

            return Ok(etiqueta);
        }

        //ver todas las categorias

        [HttpGet]
        public IActionResult VerTodas()
        {
            var etiquetas = _contexto.Etiqueta.ToList();

            if (etiquetas != null)
            {
                return Ok(etiquetas);
            }

            return NotFound("No se encontraron etiquetas");
        }

        [HttpGet("BuscarPorEtiqueta/{IdEtiqueta}")]
        [AllowAnonymous]
        public IActionResult BuscarPorEtiqueta(int IdEtiqueta)
        {
            var publicaciones = _contexto.Publicacion
                .Where(p => p.IdEtiqueta == IdEtiqueta && p.Estado == Estado.Publica)
                .ToList();
            
            return NotFound("No se encontraron publicaciones para la categoría seleccionada.");
        }
    }
}
