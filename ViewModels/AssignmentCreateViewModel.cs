using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using studyAssistant.Core.Domain;

namespace studyAssistant.ViewModels
{
    public class AssignmentCreateViewModel
    {
        public Assignment Assignment;

        public IEnumerable<Course> Courses;
    }
}
