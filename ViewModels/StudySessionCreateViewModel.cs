using System.Collections.Generic;
using studyAssistant.Core.Domain;

namespace studyAssistant.ViewModels
{
    public class StudySessionCreateViewModel
    {
        
        public StudySession StudySession { get; set; }

        
        public IEnumerable<Course> Courses  { get; set; }

        public IEnumerable<StudySessionType> StudySessionTypes { get; set; }

        }

}
