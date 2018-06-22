using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StudyAssistant.Web.Core.Domain;

namespace StudyAssistant.Web.ViewModels
{
    public class AssignmentCreateViewModel
    {
        public Assignment Assignment;

        public IEnumerable<Course> Courses;
    }
}
