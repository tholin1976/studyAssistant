using System.ComponentModel.DataAnnotations;

namespace studyAssistant.ViewModels
{
	/// <summary>
	/// Class designed to hold form data during a user login 
	/// </summary>
    public class AccountLoginViewModel
    {
		/// <summary>
		/// Holds the user's user name
		/// </summary>
        [Required(ErrorMessage = "Du må oppgi brukernavn"), MaxLength(256)]
        public string UserName { get; set; }

		/// <summary>
		/// Holds the user's password
		/// </summary>
        [Required(ErrorMessage = "Du må oppgi passord"), DataType(DataType.Password)]
        public string Password { get; set; }
       }
}
