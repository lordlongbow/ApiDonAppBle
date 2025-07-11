using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiDonAppBle.Models
{
    public class Comentario
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdComentario { get; set; }

        [Required]
        public string Texto { get; set; }

        [Required]
        public DateTime Fecha { get; set; }

        [ForeignKey("Publicacion")]
        public int IdPublicacion { get; set; }
        public Publicacion? Publicacion { get; set; }

       
        [ForeignKey("Usuario")]
        public int IdUsuario { get; set; }
        public Usuario Usuario { get; set; }
    }
}
