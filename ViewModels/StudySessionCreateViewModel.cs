using System.Collections.Generic;
using StudyAssistant.Web.Core.Domain;

namespace StudyAssistant.Web.ViewModels
{
    public class StudySessionCreateViewModel
    {
        
        public StudySession StudySession { get; set; }

        
        public IEnumerable<Course> Courses  { get; set; }

        public IEnumerable<StudySessionType> StudySessionTypes { get; set; }

        }

}
