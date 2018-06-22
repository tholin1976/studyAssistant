using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace StudyAssistant.Web.Core.Domain
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
