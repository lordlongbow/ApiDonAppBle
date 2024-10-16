using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiDonAppBle.Models
{
    public class Comentario 
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdComentario {get;set;}
       
        [Required]
        public string Texto {get;set;}
       
        [Required]
        public DateTime Fecha {get;set;}
       
        public Publicacion? Publicacion {get;set;}
        
        [ForeignKey("Publicacion")]
        public int IdPublicacion {get;set;}
       

        [ForeignKey("IdUsuario")]
        public int IdUsuario {get;set;}

    }
}