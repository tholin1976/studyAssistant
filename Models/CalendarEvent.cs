using System;

namespace studyAssistant.Models
{
	/// <summary>
	/// Holds data about a single assignment / study session for display in a calendar
	/// </summary>
    public class CalendarEvent  
    {
		/// <summary>
		/// The event id
		/// </summary>
        public int Id { get; set; }
		/// <summary>
		/// The event title
		/// </summary>
        public string Title { get; set; }
		/// <summary>
		/// The event date / time
		/// </summary>
        public DateTime Date { get; set; }
		/// <summary>
		/// Type of event, either "StudySession" or "Assignment"
		/// </summary>
        public string Type { get; set; }
    }
}
