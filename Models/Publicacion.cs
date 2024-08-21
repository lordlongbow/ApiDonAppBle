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
        //id titulo fecha foto disponibilidad idUsuario
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdPublicacion {get;set;}
        [Required]
        public string Titulo {get;set;}
        [Required]
        public DateTime Fecha {get;set;}
        public Estado Estado { get; set; } = Estado.Publica;
        [Required]
        public string FotoPublicacion {get;set;}
        public string VideoPublicacion {get;set;}
        [Required]
        public bool Disponibilidad {get;set;}
        [ForeignKey("IdUsuario")]
        public int IdUsuario {get;set;}
        public Usuario Usuario {get;set;}
        //[ForeignKey("IdCategoria")]
        //public int IdCategoria {get;set;}
        public Categoria Categoria {get;set;}
        public ICollection<Comentario> Comentarios {get;set;}


    }

}