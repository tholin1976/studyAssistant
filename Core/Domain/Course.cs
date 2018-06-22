using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudyAssistant.Web.Core.Domain
{
    public enum Grade
    {
        A = 6, B = 5, C = 4, D = 3, E = 2, F = 0, Ingen = -1
    }

    public class Course
    {

        public int Id { get; set; }
        
        [Display(Name = "Fagtittel")]
        [Required(ErrorMessage = "Fagtittel mangler.")]
        public string Title { get; set; }
        [Display(Name = "Beskrivelse")]
        public string Description { get; set; }
        
        [Display(Name = "Studiepoeng")]
        [RegularExpression(@"^(?=.*[1-9])([0-9]{0,3}(?:,[0-9]{1,})?)$", ErrorMessage = "Angi studiepoeng som heltall eller desimaltall med komma.")]
        [Required(ErrorMessage = "Studiepoeng mangler.")]
        public decimal Credits { get; set; }

        [NotMapped]
        public double WorkLoad => (double) Credits * 25.2;

        [NotMapped] 
        public bool IsActive => DateTime.Now.CompareTo(DateFrom) > 0 && DateTime.Now.CompareTo(DateTo) < 0;

        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}")]
        [Display(Name = "Fra dato")]
        [DataType(DataType.Date, ErrorMessage = "Fra-dato må være i formatet dd.mm.YYYY.")]
        [Required(ErrorMessage = "Fra-dato mangler.")]
        public DateTime DateFrom { get; set; }
        
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}")]
        [Display(Name = "Til dato")]
        [DataType(DataType.Date, ErrorMessage = "Til-dato må være i formatet dd.mm.YYYY.")]
        [Required(ErrorMessage = "Til-dato mangler.")]
        public DateTime DateTo { get; set; }

        [Display(Name = "Opprettet")]
        public DateTime DateCreated { get; set; } = DateTime.UtcNow;
        [Display(Name = "Karakter")]
        public Grade? Grade { get; set; } = Domain.Grade.Ingen;

        [Display(Name = "Bruker")]
        public int UserId { get; set; }
        
        public User User { get; set; }
        public ICollection<Assignment> Assignments { get; set; }
        public ICollection<StudySession> StudySessions { get; set; }
        }


}
