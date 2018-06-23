using System;
using System.ComponentModel.DataAnnotations;

namespace studyAssistant.Core.Domain
{
    public partial class StudySession
    {
        public int Id { get; set; }
        
        [Display(Name = "Tittel")]
        [StringLength(250, MinimumLength = 5, ErrorMessage = "Lengden på tittelen må være mellom 5 og 250 tegn")]
        [Required(ErrorMessage = "Du må oppgi en tittel på studieøkten")]
        public string Title { get; set; }
        
        [StringLength(1500, ErrorMessage = "Beskrivelsen kan ikke være lenger enn 1500 tegn")]
        [Display(Name = "Beskrivelse")]
        public string Description { get; set; }

        [Display(Name = "Startdato")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}")]
        [Required(ErrorMessage = "Du må oppgi en startdato for studieøkten")]
        public DateTime StartDate { get; set; }
        
        [Display(Name = "Starttidspunkt")]
        [RegularExpression("^([0-1]?[0-9]|[2][0-3]):([0-5][0-9])(:[0-5][0-9])?$", ErrorMessage = "Klokkeslettet for oppstart må være mellom 00:00 og 23:59")]
        [DisplayFormat(DataFormatString = "{0:hh\\:mm}", ApplyFormatInEditMode = true)]
        [Required(ErrorMessage = "Du må oppgi et starttidspunkt for studieøkten")]
        public TimeSpan StartTime { get; set; }

        [Display(Name = "Varighet")]
        [RegularExpression("^([0-1]?[1-9]|[2][0-3]):([0-5][0-9])(:[0-5][0-9])?$", ErrorMessage = "Varigheten på studieøkten må være mellom 01:00 og 23:59")]
        [DisplayFormat(DataFormatString = "{0:hh\\:mm}", ApplyFormatInEditMode = true)]
        [Required(ErrorMessage = "Du må oppgi en varighet for studieøkten")]
        public TimeSpan Duration { get; set; }
        
        [RegularExpression(@"^[1-9]\d*$", ErrorMessage = "Du må velge et fag du skal jobbe med")]
        [Display(Name = "Fag")]
        [Required(ErrorMessage = "Du må velge et fag du skal jobbe med")]
        public int CourseId { get; set; }
        
        [RegularExpression(@"^[^0][1-9]*", ErrorMessage = "Du må velge arbeidsform for studieøkten")]
        [Display(Name = "Arbeidsform")]
        [Required(ErrorMessage = "Du må velge arbeidsform for studieøkten")]
        public int StudySessionTypeId { get; set; }
        
        [Display(Name = "Opprettet")]
        public DateTime DateCreated { get; set; } = DateTime.Now;
        
        [Display(Name = "Er fullført")]
        public bool IsCompleted { get; set; } = false;
        
        [Display(Name = "Fag")]
        public Course Course { get; set; }
        
        [Display(Name = "Arbeidsform")]
        public StudySessionType StudySessionType { get; set; }

        public DateTime GetStudySessionStart()
        {
            return StartDate.Add(StartTime);
        }

        public DateTime GetStudySessionEnd()
        {
            return GetStudySessionStart().Add(Duration);
        }
    }
}
