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
        public string Descripcion { get; set; }

        [Required]
        public DateTime Fecha { get; set; }

        [Column(TypeName = "VARCHAR(50)")]
        public Estado Estado { get; set; } = Estado.Publica;

        [NotMapped]
        public IFormFile? FotoPublicacionIFormFile { get; set; }
        public string? FotoPublicacion { get; set; }
        public string? VideoPublicacion { get; set; }

        [Required]
        public bool Disponibilidad { get; set; }


        [ForeignKey("IdUsuario")]
        public Usuario? Usuario { get; set; }

        public int IdUsuario { get; set; }


        [Column(TypeName = "VARCHAR(250)")]
        public CategoriaEnum Categoria { get; set; }


        [ForeignKey("IdComenatario")]
        public int? IdComenatario { get; set; }


        [ForeignKey("IdEtiqueta")]
        public int? IdEtiqueta { get; set; }


    }


}