using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiDonAppBle.Models
{
    public class Etiqueta 
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdEtiqueta {get;set;}
       
        [Required]
        public string Descripcion {get;set;}
       
    }
}