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

        public CategoriaController(
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
        public IActionResult CrearEtiqueta([FromBody] Categoria nuevaEtiqueta)
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

        //eliminar categoria
        [HttpDelete("{id}")]
        public IActionResult EliminarCategoria(int id)
        {
            var categoria = _contexto.Categoria.SingleOrDefault(c => c.IdCategoria == id);

            if (categoria == null)
            {
                return NotFound("Categoría no encontrada");
            }

            _contexto.Categoria.Remove(categoria);
            _contexto.SaveChanges();
            return Ok("Categoría eliminada");
        }

        //editar categoria
        [HttpPut("{id}")]
        public IActionResult EditarCategoria(int id, [FromBody] Categoria categoriaActualizada)
        {
            var categoria = _contexto.Categoria.SingleOrDefault(c => c.IdCategoria == id);

            if (categoria == null)
            {
                return NotFound("Categoría no encontrada");
            }

            categoria.Descripcion = categoriaActualizada.Descripcion;

            _contexto.Categoria.Update(categoria);
            _contexto.SaveChanges();

            return Ok(categoria);
        }

        //ver todas las categorias

        [HttpGet]
        public IActionResult VerTodas()
        {
            var categorias = _contexto.Categoria.ToList();

            if (categorias != null)
            {
                return Ok(categorias);
            }

            return NotFound("No se encontraron categorías");
        }

        [HttpGet("BuscarPorCategoria/{idCategoria}")]
        [AllowAnonymous]
        public IActionResult BuscarPorCategoria(int idCategoria)
        {
            var publicaciones = _contexto.Publicacion
                .Where(p => p.IdCategoria == idCategoria && p.Estado == Estado.Publica)
                .ToList();

            if (publicaciones.Any())
            {
                return Ok(publicaciones);
            }

            return NotFound("No se encontraron publicaciones para la categoría seleccionada.");
        }
    }
}
