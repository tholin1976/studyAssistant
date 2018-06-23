using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using studyAssistant.Models;

namespace studyAssistant.ViewModels
{
    public class SelectCourseReportViewModel
    {
        public int courseId {get; set;}
        public ChartType chartType;
    }
}
