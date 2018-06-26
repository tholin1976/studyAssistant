using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using studyAssistant.Core.Domain;
using studyAssistant.Data;
using studyAssistant.Models;

namespace studyAssistant.Controllers
{

	/// <summary>
	/// Handles the requests to the route /Course
	/// </summary>
	[Authorize]
    public class CourseController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly ApplicationDbContext _context;

		/// <summary>
		/// Class constructor
		/// </summary>
		/// <param name="userManager">The UserManager object used for dependency injection</param>
		/// <param name="context">The ApplicationDBContext object used for dependency injection</param>
		public CourseController(UserManager<User> userManager, ApplicationDbContext context)
        {
            _context = context;
            _userManager = userManager;

        }

		/// <summary>
		/// Displays a list of the current user's courses with pagination, search and sorting functionality
		/// </summary>
		/// <param name="sortOrder">The column to sort by</param>
		/// <param name="currentFilter">The current searchString stored for navigation</param>
		/// <param name="searchString">The string to search for</param>
		/// <param name="page">The page number to display</param>
		/// <param name="onlyActiveCourses">True = display only active courses, false = display all courses</param>
		/// <returns>View</returns>
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

		/// <summary>
		/// Displays a form for creating a new course
		/// </summary>
		/// <returns>View</returns>
        [HttpGet]
        public ViewResult Create()
        {

            return View();
        }

		/// <summary>
		/// Receives a new Course object from the request body, passes it to the ApplicationDBContext object which then tries to save it
		/// </summary>
		/// <param name="course">The new Course object</param>
		/// <returns>RedirectToAction(Index) if successfull, View(course) if unsuccessfull</returns>
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

		/// <summary>
		/// Receives a course id from the request body, passes it to the ApplicationDbContext object, which retrieves it. It is then displayed in an edit form if it exists and belongs to the current user
		/// </summary>
		/// <param name="id">The id of the course to edit</param>
		/// <returns>View(course) if successfull, RedirectToAction(Index) if unsuccessfull</returns>
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

		/// <summary>
		/// Retrieves an edited Course object from the request body, passes it to the ApplicationDbContext object which then tries to save it.
		/// </summary>
		/// <param name="course">The edited Course object</param>
		/// <returns>RedirectToAction(Index) if successful, View(course) if unsuccessfull</returns>
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

		/// <summary>
		/// Receives a course id from the request body, passes it to the ApplicationDBContext, which retrieves the course if it exists, and then finally displays a
		/// delete warning form if it belongs to the current user
		/// </summary>
		/// <param name="id">The id of the course to delete</param>
		/// <returns>View(course) if successfull, RedirectToAction(Index) if unsuccessfull</returns>
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

		/// <summary>
		/// Receives a course id from the request body, passes it to the ApplicationDBContext object which retrieves it if it exists and then tries to delete it.
		/// </summary>
		/// <param name="id">The id of the course to delete</param>
		/// <returns>RedirectToAction(Index) with a status message stored in TempData</returns>
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

		/// <summary>
		/// Receives a course id from the request body, passes it to the ApplicationDbContext object, which then retrieves it if it exists. Then the course is passed to the view if it belongs to the current user.
		/// </summary>
		/// <param name="id">The id</param>
		/// <returns>View(course) if successfull, RedirectToAction(Index) if unsuccessfull</returns>
        [HttpGet]
        public async Task<IActionResult> Info(int id)
        {

            var course = await _context.GetCourseById(id);

            if (course == null)
            {
                TempData["Message"] = new SystemMessage(MessageType.Warning, "Fant ikke faget.").GetSystemMessage();
                return RedirectToAction("Index");
            }

	        var currentUser = await _userManager.GetUserAsync(HttpContext.User);

	        if (course.UserId != currentUser.Id)
	        {
		        TempData["Message"] =
			        new SystemMessage(MessageType.Warning, "Faget du ønsker å endre, tilhører en annen bruker.")
				        .GetSystemMessage();
		        return RedirectToAction("Index");
	        }


            ViewBag.CourseStudyTime = await _context.GetStudyTimeByCourse(id);

            return View(course);
        }


    }
}