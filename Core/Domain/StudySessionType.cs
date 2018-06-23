using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;


namespace studyAssistant.Core.Domain
{
    public class StudySessionType
    {
        [Display(Name = "Arbeidsform")]
        public int Id { get; set; }
        [Display(Name = "Tittel")]
        [StringLength(500)]
        [Required]
        public string Title { get; set; }
        [Display(Name = "Beskrivelse")]
        public string Description { get; set; }

        public ICollection<StudySession> StudyActivities { get; set; }
    }
}
