using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StudyAssistant.Web.Data;
using StudyAssistant.Web.Core.Domain;

namespace StudyAssistant.Web.Controllers
{
    [Authorize]
    public class CalendarController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public CalendarController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

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