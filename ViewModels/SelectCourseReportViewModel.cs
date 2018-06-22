using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StudyAssistant.Web.Models;

namespace StudyAssistant.Web.ViewModels
{
    public class SelectCourseReportViewModel
    {
        public int courseId {get; set;}
        public ChartType chartType;
    }
}
