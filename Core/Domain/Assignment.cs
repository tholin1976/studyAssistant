using System;
using System.ComponentModel.DataAnnotations;
namespace studyAssistant.Core.Domain
{
	/// <summary>
	/// Holds a single user assignment
	/// </summary>
    public class Assignment
    {

		/// <summary>
		/// The assignment id
		/// </summary>
        public int Id { get; set; }
      
		/// <summary>
		/// The assignment title
		/// </summary>
        [Display(Name = "Oppgavetittel")]
        [StringLength(250, ErrorMessage = "Oppgavetittel kan ikke være lenger enn 250 tegn")]
        [Required(ErrorMessage = "Du må oppgi en tittel på oppgaven")]
        public string Title { get; set; }
      
		/// <summary>
		/// The assignment description
		/// </summary>
        [StringLength(1500, ErrorMessage = "Beskrivelsen kan ikke være lenger enn 1500 tegn")]
        [Display(Name = "Beskrivelse")]
        public string Description { get; set; }
      
		/// <summary>
		/// The assignment deadline
		/// </summary>
        [Display(Name = "Tidsfrist")]
        [Required(ErrorMessage = "Du må oppgi en tidsfrist for oppgaven")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-ddTHH:mm}", ApplyFormatInEditMode = true)]
        public DateTime Deadline { get; set; }
       
		/// <summary>
		/// The date and time the assignment was created
		/// </summary>
        [Display(Name = "Opprettet")]
        public DateTime DateCreated { get; set; } = DateTime.UtcNow;
 
		/// <summary>
		/// The date and time the assignment was finished
		/// </summary>
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-ddTHH:mm}", ApplyFormatInEditMode = true)]
        [Display(Name = "Fullført")]
        public DateTime? DateCompleted { get; set; }
        
		/// <summary>
		/// The assignment's grade result
		/// </summary>
        [Display(Name = "Karakter")]
        public Grade? Grade { get; set; }
        
		/// <summary>
		/// Navigational property for Entity Framework Core. Holds the assignment's owner
		/// </summary>
        [Display(Name = "Fag")]
        public Course Course { get; set; }
        
		/// <summary>
		/// A foreign key for the assignment's owner
		/// </summary>
        [RegularExpression(@"^[^0][1-9]*", ErrorMessage = "Du må velge et tilknyttet fag for oppgaven.")]
        [Display(Name = "Fag")]
        public int? CourseId { get; set; }
    }
}
