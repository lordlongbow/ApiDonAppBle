using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiDonAppBle.Models

{
    public class Usuario 
    {
        //id password email nombre apellido doreccion avatar
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdUsuario {get;set;}
        public string Nombre {get;set;}
        public string Apellido {get;set;}
        [Required]
        public string Email {get;set;}
        [Required]
        public string Password {get;set;}
        public string? Direccion {get;set;}
        public string? Avatar {get;set;}
      //  public ICollection<Publicacion>? Publicaciones {get;set;}
    }
}