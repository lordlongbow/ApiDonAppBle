using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiDonAppBle.Models

{
    public class LoginView 
    {
        //id password email nombre apellido doreccion avatar
       
        [Required]
        public string Email {get;set;}
        [Required]
        public string Password {get;set;}
    }
}