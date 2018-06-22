using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudyAssistant.Web.Core.Domain;
using StudyAssistant.Web.Data;
using System.Linq;
using ChartJSCore.Models;
using StudyAssistant.Web.Models;

namespace StudyAssistant.Web.Controllers
{

    [Authorize]
    public class CourseController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly ApplicationDbContext _context;


        public CourseController(UserManager<User> userManager, ApplicationDbContext context)
        {
            _context = context;
            _userManager = userManager;

        }

        public async Task<ViewResult> Index(string sortOrder, string currentFilter, string searchString, int? page, bool onlyActiveCourses)
        {
            const int pageSize = 20;

            ViewData["CurrentSort"] = sortOrder;
            ViewData["sortTitleParam"] = String.IsNullOrEmpty(sortOrder) ? "Title_desc" : "";
            ViewData["sortDateParam"] = sortOrder == "DateFrom" ? "DateFrom_desc" : "DateFrom";

            if (onlyActiveCourses)
            {
                ViewData["onlyActiveCourses"] = "checked";
                ViewData["onlyActiveCoursesvalue"] = true;
            }
            else
            {
                ViewData["onlyActiveCoursesValue"] = false;
            }

            if (searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            if (String.IsNullOrEmpty(sortOrder))
            {
                sortOrder = "Title";
            }

            bool descending = false;


            if (sortOrder.EndsWith("_desc"))
            {
                sortOrder = sortOrder.Substring(0, sortOrder.Length - 5);
                descending = true;
            }


            var currentUser = await _userManager.GetUserAsync(HttpContext.User);


            return View(await PaginatedList<Course>.CreateAsync(_context.GetCoursesPagedList(currentUser.Id, searchString, sortOrder, descending, onlyActiveCourses), page ?? 1, pageSize));
        }

        [HttpGet]
        public ViewResult Create()
        {

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Course course)
        {
            // Returns user to the form if invalid input exists
            if (!ModelState.IsValid)
            {
                return View(course);
            }


            var currentUser = await _userManager.GetUserAsync(HttpContext.User);

            course.UserId = currentUser.Id;

            try
            {
                _context.Courses.Add(course);
                await _context.SaveChangesAsync();
                TempData["Message"] =
                    new SystemMessage(MessageType.Success, "Det nye faget ble lagret.").GetSystemMessage();
            }

            catch (Exception)
            {
                TempData["Message"] =
                    new SystemMessage(MessageType.Critical,
                        "Det oppstod en feil under lagring. Vennligst prøv igjen senere eller kontakt systemadministrator hvis feilen vedvarer.");
                return View(course);
            }

            return RedirectToAction("Index");



        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var currentUser = await _userManager.GetUserAsync(HttpContext.User);

            var course = await _context.Courses.FindAsync(id);
            
            if (course == null)
            {
                TempData["Message"] = new SystemMessage(MessageType.Warning, "Fant ikke faget.").GetSystemMessage();
                return RedirectToAction("Index");
            }

            if (course.UserId != currentUser.Id)
            {
                TempData["Message"] =
                    new SystemMessage(MessageType.Warning, "Faget du ønsker å endre, tilhører en annen bruker.")
                        .GetSystemMessage();
                return RedirectToAction("Index");
            }
            return View(course);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Course course)
        {
            if (!ModelState.IsValid)
            {
                // Custom validation
                return View(course);
            }

            var currentUser = await _userManager.GetUserAsync(HttpContext.User);

            course.UserId = currentUser.Id;
            try
            {
                _context.Courses.Update(course);
                await _context.SaveChangesAsync();
                TempData["Message"] =
                    new SystemMessage(MessageType.Success, "Endringene ble lagret.").GetSystemMessage();
            }
            catch (Exception)
            {
                TempData["Message"] = 
                    new SystemMessage(MessageType.Critical, "Det oppstod en feil under lagring av endringene.").GetSystemMessage();

                return View(course);
            }

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                TempData["Message"] = new SystemMessage(MessageType.Warning, "Fag-id mangler.").GetSystemMessage();
                return RedirectToAction("Index");
            }

            var course = await _context.GetCourseById(id);

            var currentUser = await _userManager.GetUserAsync(HttpContext.User);

            if (course == null)
            {
                TempData["Message"] = new SystemMessage(MessageType.Warning, "Fant ikke faget.").GetSystemMessage();
                return RedirectToAction("Index");
            }

            if (course.UserId != currentUser.Id)
            {
                TempData["Message"] =
                    new SystemMessage(MessageType.Warning, "Faget du ønsker å endre, tilhører en annen bruker.")
                        .GetSystemMessage();
                return RedirectToAction("Index");
            }

            
            return View(course);
        }


        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var course = await _context.GetCourseById(id);

            if (course == null)
            {
                TempData["Message"] = new SystemMessage(MessageType.Warning, "Fant ikke faget.").GetSystemMessage();
                return RedirectToAction("Index");
            }

            try
            {
                _context.Courses.Remove(course);
                await _context.SaveChangesAsync();
                TempData["Message"] = new SystemMessage(MessageType.Success, "Faget ble slettet.").GetSystemMessage();
            }
            catch (Exception)
            {
                TempData["Message"] = new SystemMessage(MessageType.Critical, "Det oppstod en feil under sletting.").GetSystemMessage();
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Info(int id)
        {

            var course = await _context.GetCourseById(id);

            if (course == null)
            {
                TempData["Message"] = new SystemMessage(MessageType.Warning, "Fant ikke faget.").GetSystemMessage();
                return RedirectToAction("Index");
            }

            ViewBag.CourseStudyTime = await _context.GetStudyTimeByCourse(id);

            return View(course);
        }


    }
}