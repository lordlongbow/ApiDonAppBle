using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiDonAppBle.Models
{


    public enum Estado
    {
        Publica,
        Privada
    }

    public class Publicacion
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdPublicacion { get; set; }

        [Required]
        public string Titulo { get; set; }

        [Required]
        public DateTime Fecha { get; set; }

        // El estado será público por defecto
        public Estado Estado { get; set; } = Estado.Publica;

        [NotMapped] //esto es para que no se inserte en la base de datos y quede en el servidor
        public IFormFile? FotoPublicacionIFormFile { get; set; }
        public string? FotoPublicacion { get; set; }
        public string? VideoPublicacion { get; set; }

        [Required]
        public bool Disponibilidad { get; set; }

        
        public Usuario? Usuario { get; set; }

        [ForeignKey("Usuario")]
        public int IdUsuario { get; set; }

        public CategoriaEnum Categoria { get; set; }

        [ForeignKey("IdComenatario")]
        public int IdComenatario { get; set; }


        [ForeignKey("IdEtiqueta")]
        public int IdEtiqueta { get; set; }

        public ICollection<Etiqueta>? Etiquetas { get; set; }
    }


}