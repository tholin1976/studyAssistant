using System.ComponentModel.DataAnnotations;

namespace StudyAssistant.Web.ViewModels
{
    public class AccountLoginViewModel
    {
        [Required(ErrorMessage = "Du må oppgi brukernavn"), MaxLength(256)]
        public string UserName { get; set; }
        [Required(ErrorMessage = "Du må oppgi passord"), DataType(DataType.Password)]
        public string Password { get; set; }
       }
}
