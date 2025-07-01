using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiDonAppBle.Models
{
    public class MensajePrivado
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdMensajePrivado { get; set; }

        [Required]
        public string Texto { get; set; }

        public DateTime Fecha { get; set; }

        [ForeignKey("Emisor")]
        public int IdEmisor { get; set; }
        public Usuario Emisor { get; set; }

        [ForeignKey("Receptor")]
        public int IdReceptor { get; set; }
        public Usuario Receptor { get; set; }
    }
}
