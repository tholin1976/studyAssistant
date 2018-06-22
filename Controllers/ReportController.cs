using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using ChartJSCore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudyAssistant.Web.Core.Domain;
using StudyAssistant.Web.Data;
using StudyAssistant.Web.Models;
using StudyAssistant.Web.Utilities;

namespace StudyAssistant.Web.Controllers
{
    [Authorize]
    public class ReportController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public ReportController(ApplicationDbContext context,
            UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost, ActionName("Time")]
        public async Task<IActionResult> ProgressByCoursePost(int courseId, ChartType chartType)
        {
            var course = await _context.GetCourseById(courseId);

            if (course == null)
            {
                TempData["Message"] = new SystemMessage(MessageType.Warning, "Fant ikke faget.").GetSystemMessage();
                return RedirectToAction("Time");
            }
            var chartMaker = new ChartMaker(_context);
            var currentUser = await _userManager.GetUserAsync(HttpContext.User);
            
            ViewBag.Courses = await _context.PopulateCourseData(currentUser.Id);
            ViewBag.chartTitle = chartType.GetDisplayName();
            ViewBag.courseTitle = course.Title;

            switch (chartType)
            {
                case ChartType.Progression:
                    ViewBag.Chart = await chartMaker.GenerateCourseChart(course, ChartType.Progression, "Dine arbeidstimer sammenlignet med fagets arbeidsmengde");
                    break;
                case ChartType.Workload:
                    ViewBag.Chart = await chartMaker.GenerateCourseChart(course, ChartType.Workload, "Gjennomførte og gjenværende arbeidstimer");
                    break;
            }
            
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Time()
        {
            var currentUser = await _userManager.GetUserAsync(HttpContext.User);
            
            ViewBag.Courses = await _context.PopulateCourseData(currentUser.Id);
            
            return View();

        }
    }
}