using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiDonAppBle.Models
{
    public class Publicacion 
    {
        //id titulo fecha foto disponibilidad idUsuario
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdPublicacion {get;set;}
        [Required]
        public string Titulo {get;set;}
        [Required]
        public DateTime Fecha {get;set;}
        [Required]
        public string FotoPublicacion {get;set;}
        [Required]
        public bool Disponibilidad {get;set;}
        [ForeignKey("IdUsuario")]
        public int IdUsuario {get;set;}
        [ForeignKey("IdCategoria")]
        public int IdCategoria {get;set;}

    }
}