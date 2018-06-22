using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudyAssistant.Web.Core.Domain;
using StudyAssistant.Web.Data;
using StudyAssistant.Web.ViewModels;

namespace StudyAssistant.Web.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public DashboardController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        
        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.GetUserAsync(HttpContext.User);
            var vm = new DashboardViewModel
            {
                Assignments = await _context.GetAssignments(currentUser.Id, null, true, 7)
                    .OrderBy(a => a.Deadline)
                    .ToListAsync(),

                StudySessions = await _context.GetStudySessionsByUser(currentUser.Id, true)
                    .Where(s => s.StartDate.Date == DateTime.Now.Date)
                    .OrderBy(s => s.StartTime)
                    .ToListAsync()
            };

            return View(vm);
        }
    }
}