using System.ComponentModel.DataAnnotations;

namespace StudyAssistant.Web.ViewModels
{
    public class AccountCreateViewModel
    {
        [Required(ErrorMessage = "Du må oppgi et brukernavn.")]
        [MaxLength(256, ErrorMessage = "Brukernavnet kan ikke være lenger enn 256 tegn.")]
        public string UserName { get; set; }
        [Required(ErrorMessage = "Du må oppgi en epost-adresse."), RegularExpression(@"^[a-zA-Z0-9.!#$%&’*+/=?^_`{|}~-]+@[a-zA-Z0-9-]+(?:\.[a-zA-Z0-9-]+)*$", ErrorMessage = "Du må oppgi en gyldig epost-adresse."), MaxLength(256)]
        public string Email { get; set; }
        [Required(ErrorMessage = "Du må velge et passord."), DataType(DataType.Password)]
        [RegularExpression(@"^(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9])(?=.*?[#?!@$%^&*-]).{8,}$", ErrorMessage = "Passordet må være minst 8 tegn langt og må inneholde minimum 1 tall, 1 spesialtegn og 1 stor bokstav.")]
        public string Password { get; set; }
        
    }
}
