using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using studyAssistant.Data;
using studyAssistant.Core.Domain;
using studyAssistant.Models;

namespace studyAssistant.Controllers
{
	/// <summary>
	/// Handles the requests to the route /Calendar
	/// </summary>
    [Authorize]
    public class CalendarController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

		/// <summary>
		/// Class constructor
		/// </summary>
		/// <param name="context">The ApplicationDBContext object used for dependency injection</param>
		/// <param name="userManager">The UserManager object used for dependency injection</param>
        public CalendarController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

		/// <summary>
		/// Gets the current user's active study sessions and assignments and displays them in a calendar
		/// </summary>
		/// <returns>View(events)</returns>
        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.GetUserAsync(HttpContext.User);

            var activeStudySessions = await _context.GetStudySessionsByUser(currentUser.Id, false).ToListAsync();

            var activeAssignments = await _context.GetAssignments(currentUser.Id, null, true).ToListAsync();

            var events = new List<CalendarEvent>();

            foreach (StudySession studySession in activeStudySessions)
            {
                events.Add(new CalendarEvent()
                {
                    Id = studySession.Id,
                    Title = studySession.Title,
                    Date = studySession.GetStudySessionStart(),
                    Type = "StudySession"
                });
            }

            foreach (Assignment assignment in activeAssignments)
            {
                events.Add(new CalendarEvent()
                {
                    Id = assignment.Id,
                    Title = assignment.Title,
                    Date = assignment.Deadline,
                    Type = "Assignment"
                }
                );
            }

            return View(events);
        }
    }
}