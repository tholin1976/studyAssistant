using System;
using System.ComponentModel.DataAnnotations;
namespace studyAssistant.Core.Domain
{
    public class Assignment
    {

        public int Id { get; set; }
      
        [Display(Name = "Oppgavetittel")]
        [StringLength(250, ErrorMessage = "Oppgavetittel kan ikke være lenger enn 250 tegn")]
        [Required(ErrorMessage = "Du må oppgi en tittel på oppgaven")]
        public string Title { get; set; }
      
        [StringLength(1500, ErrorMessage = "Beskrivelsen kan ikke være lenger enn 1500 tegn")]
        [Display(Name = "Beskrivelse")]
        public string Description { get; set; }
      
        [Display(Name = "Tidsfrist")]
        [Required(ErrorMessage = "Du må oppgi en tidsfrist for oppgaven")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-ddTHH:mm}", ApplyFormatInEditMode = true)]
        public DateTime Deadline { get; set; }
       
        [Display(Name = "Opprettet")]
        public DateTime DateCreated { get; set; } = DateTime.UtcNow;
 
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-ddTHH:mm}", ApplyFormatInEditMode = true)]
        [Display(Name = "Fullført")]
        public DateTime? DateCompleted { get; set; }
        
        [Display(Name = "Karakter")]
        public Grade? Grade { get; set; }
        
        [Display(Name = "Fag")]
        public Course Course { get; set; }
        
        [RegularExpression(@"^[^0][1-9]*", ErrorMessage = "Du må velge et tilknyttet fag for oppgaven.")]
        [Display(Name = "Fag")]
        public int? CourseId { get; set; }
    }
}
