using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudyAssistant.Web.Core.Domain
{
    public class User : IdentityUser<int>
    {
        [Display(Name = "For- og mellomnavn")]
        public string FirstMiddleName { get; set; }
        [Display(Name = "Etternavn")]
        public string LastName { get; set; }
        [Display(Name = "Opprettet")]
        public DateTime DateCreated { get; set; } = DateTime.UtcNow;
        [Display(Name = "Husk meg")]
        public bool RememberMe { get; set; } = false;
    }
}
