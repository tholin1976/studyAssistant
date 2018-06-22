using System;

namespace StudyAssistant.Web.Core.Domain
{
    public class CalendarEvent  
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public DateTime Date { get; set; }
        public string Type { get; set; }
    }
}
