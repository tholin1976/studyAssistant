using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using studyAssistant.Core.Domain;
using studyAssistant.Data;
using studyAssistant.Models;


namespace studyAssistant.Controllers
{
    /// <summary>
	/// Handles all the requests to the route /Assignment
	/// </summary>
    [Authorize]
    public class AssignmentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

		/// <summary>
		/// Class constructor
		/// </summary>
		/// <param name="userManager">The UserManager object used for injection</param>
		/// <param name="context">The ApplicationDBContext object used for injection</param>
        public AssignmentController(UserManager<User> userManager, ApplicationDbContext context)
        {
            _context = context;
            _userManager = userManager;
        }

		/// <summary>
		/// Displays a list of the user's assignments, both active and inactive, with searching, sorting and paging functionality.
		/// </summary>
		/// <param name="sortOrder">Which columns to sort by</param>
		/// <param name="searchString">The search string supplied by the user</param>
		/// <param name="currentFilter">Stores the search string for navigation between pages</param>
		/// <param name="page">Number of the page to display</param>
		/// <param name="onlyActiveAssignments">True = only shows active assignments, false = shows all assignments</param>
		/// <returns>View with a model consisting of a paginated list of assignments</returns>
        public async Task<IActionResult> Index(string sortOrder, string searchString, string currentFilter, int? page, bool onlyActiveAssignments)
        {
            var currentUser = await _userManager.GetUserAsync(HttpContext.User);
           
            const int pageSize = 3;

            ViewData["CurrentSort"] = sortOrder;
            ViewData["sortTitleParam"] = String.IsNullOrEmpty(sortOrder) ? "Title_desc" : "";
            ViewData["sortDateParam"] = sortOrder == "Deadline" ? "Deadline_desc" : "Deadline";
            ViewData["sortCourseParam"] = sortOrder == "CourseId" ? "CourseId_desc" : "CourseId";

            if (onlyActiveAssignments)
            {
                ViewData["onlyActiveAssignments"] = "checked";
                ViewData["onlyActiveAssignmentsValue"] = true;
            }
            else
            {
                ViewData["onlyActiveAssignmentsValue"] = false;
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

            return View(await PaginatedList<Assignment>.CreateAsync(
                _context.GetAssignmentsPagedList(currentUser.Id, searchString, sortOrder, descending,
                    onlyActiveAssignments), page ?? 1, pageSize));
        }

		/// <summary>
		/// Stores the user's courses in ViewBag and displays the view /Assignment/Create.cshtml
		/// </summary>
		/// <returns>View with a model consisting of an Assignment object</returns>
        public async Task<IActionResult> Create()
        {
            SystemMessage msg;

            var currentUser = await _userManager.GetUserAsync(HttpContext.User);
            var assignment = new Assignment();

            try
            {
                ViewBag.Courses = await _context.PopulateCourseData(currentUser.Id);
            }
            catch (Exception)
            {
                msg = new SystemMessage(MessageType.Warning,
                    "Fikk ikke hentet faginformasjonen. Vennligst prøv igjen senere.");
                ViewBag.Message = msg.GetSystemMessage();
            }            

            return View(assignment);
        }

		/// <summary>
		/// Receives an Assignment object from the request body, passes it on to the ApplicationDBContext object which then tries to save it.
		/// </summary>
		/// <param name="assignment">The Assignment object received from the request body</param>
		/// <returns>RedirectToAction(Index) if successfull, View(assignment) if unsuccessfull</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Assignment assignment)
        {
            SystemMessage msg;

            if (!ModelState.IsValid)
            {
                // Custom validation

                return View(assignment);
            }

			
            var currentUser = await _userManager.GetUserAsync(HttpContext.User);

            try
            {
				// Trying to save the new assignment
                await _context.Assignments.AddAsync(assignment);
                var saveresult = await _context.SaveChangesAsync();
                msg = new SystemMessage(MessageType.Success, "Oppgaven ble lagret!");
            }

            catch (DbUpdateException)
            {
                msg = new SystemMessage(MessageType.Critical,
                    "Kunne ikke lagre oppgaven. Vennligst prøv igjen, og ta kontakt hvis problemet ikke løser seg.");
                ViewBag.Message = msg.GetSystemMessage();
                
				// Making the user's course list available to the view
	            ViewBag.Courses = await _context.PopulateCourseData(currentUser.Id);
                return View(assignment);
            }
            
            
            TempData["Message"] = msg.GetSystemMessage();
            return RedirectToAction("Index");
        }

		/// <summary>
		/// Receives an assignment id from the request body, checks that it exists and that it really belongs to the user and then displays it if successfull
		/// </summary>
		/// <param name="id">The assignment id</param>
		/// <returns>RedirectToAction(Index) with a status message in TempData indicating success or failure</returns>
        public async Task<IActionResult> Edit(int id)
        {
            CultureInfo cultureInfo = Thread.CurrentThread.CurrentCulture;

            var currentUser = await _userManager.GetUserAsync(HttpContext.User);

            var assignment = await _context.GetAssignmentById(id);

            if (assignment == null)
            {
                TempData["Message"] = new SystemMessage(MessageType.Warning, "Fant ikke oppgaven").GetSystemMessage();
                return RedirectToAction(nameof(Index));
            }

            if (assignment.Course.UserId != currentUser.Id)
            {
                TempData["Message"] = new SystemMessage(MessageType.Warning,
                    "Oppgaven du prøvde å endre, tilhører en annen bruker.").GetSystemMessage();
                return RedirectToAction("Index");
            }

            


            ViewBag.Courses = await _context.PopulateCourseData(currentUser.Id);
            return View(assignment);
        }


		/// <summary>
		/// Receives an assignment id from the request body, passes it to the ApplicationDBContext which then retrieves the persisted version and tries to update it.
		/// </summary>
		/// <param name="id"></param>
		/// <returns>RedirectToAction(Index) with a status message stored in TempData</returns>
        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPost(int? id)
        {
            SystemMessage msg;

            if (id == null)
            {
                msg = new SystemMessage(MessageType.Warning,
                    "Det oppstod en feil under lagring. Vennligst prøv igjen senere.");
            }
          
            Assignment assignmentToUpdate = await _context.GetAssignmentById(id);

            await TryUpdateModelAsync<Assignment>(assignmentToUpdate);

            try
            {
                _context.Assignments.Update(assignmentToUpdate);
                var saveResult = await _context.SaveChangesAsync();
                msg = new SystemMessage(MessageType.Success, "Endringen ble lagret.");
            }
            catch (DbUpdateException)
            {
                msg = new SystemMessage(MessageType.Critical,
                    "Kunne ikke lagre endringen. Prøv igjen senere eller ta kontakt med systemadministrator om problemet vedvarer.");
            }
            
            TempData["Message"] = msg.GetSystemMessage();
            return RedirectToAction("Index");
        }


		/// <summary>
		/// Receives an assignment id from the request body, checks that it exists and that it belongs to the user, and finally displays a delete form to the user
		/// </summary>
		/// <param name="id"></param>
		/// <returns>View(Assignment) if successfull, RedirectToAction(Index) if unsuccessfull</returns>
        public async Task<IActionResult> Delete(int id)
        {
            var currentUser = await _userManager.GetUserAsync(HttpContext.User);
            var assignment = await _context.GetAssignmentById(id);

            if (assignment == null)
            {
                TempData["Message"] = new SystemMessage(MessageType.Warning, "Fant ikke oppgaven.").GetSystemMessage();
                return RedirectToAction("Index");
            }

            if (assignment.Course.UserId != currentUser.Id)
            {
                TempData["Message"] = new SystemMessage(MessageType.Warning, "Oppgaven tilhører en annen bruker.")
                    .GetSystemMessage();
                return RedirectToAction("Index");
            }

            return View(assignment);
        }


		/// <summary>
		/// Receives an assignment id from the request body, passes it to the ApplicationDBContext object, which checks that it
		/// exists and that it belongs to the user trying to delete it. Finally it attempts to delete it if successfull.
		/// </summary>
		/// <param name="id"></param>
		/// <returns>RedirectToAction(Index) with a status message stored in TempData</returns>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            SystemMessage msg;
            var assignment = await _context.GetAssignmentById(id);
            var currentUser = await _userManager.GetUserAsync(HttpContext.User);
            if (assignment == null)
            {
                msg = new SystemMessage(MessageType.Warning,
                    "Oppgaven eksisterer ikke. Ta kontakt med systemadministrator om problemet gjentar seg.");
            }

            if (assignment.Course.UserId != currentUser.Id)
            {
                msg = new SystemMessage(MessageType.Warning,
                    "Oppgaven tilhører en annen bruker.");
            }
            try
            {
                _context.Assignments.Remove(assignment);
                await _context.SaveChangesAsync();
                msg = new SystemMessage(MessageType.Success, "Oppgaven ble slettet."); 
            }
            catch (DbUpdateException)
            {
                msg = new SystemMessage(MessageType.Critical,
                    "Klarte ikke å slette oppgaven. Ta kontakt med systemadministrator om problemet vedvarer.");
            }

            TempData["Message"] = msg.GetSystemMessage();
            return RedirectToAction(nameof(Index));
        }

		/// <summary>
		/// Receives an assignment id from the request body, passes it to the ApplicationDBContext object, which then tries to set the assignment's completion date / time to the current date / time
		/// </summary>
		/// <param name="id">The id of the assignment to finish</param>
		/// <returns>RedirectToAction(Index) with a stored status message in TempData</returns>
        public async Task<IActionResult> Finish(int id)
        {
            SystemMessage msg;

            try
            {
                await _context.FinishAssignmentAsync(id);
                msg = new SystemMessage(MessageType.Success, "Status på oppgaven ble satt til fullført!");
            }
            catch (Exception)
            {
                msg = new SystemMessage(MessageType.Warning, "Klarte ikke å endre status på oppgaven.");
            }

            TempData["Message"] = msg.GetSystemMessage();
            return RedirectToAction("Index");
        }

		/// <summary>
		/// Receives an assignment id from the request body, passes it to the ApplicationDBContext object, which retrieves it. If it exists and it belongs to the current user, it gets sent to the view
		/// </summary>
		/// <param name="id"></param>
		/// <returns>View(assignment) if successfull, RedirectToAction(Index) with a status message in TempData if unsuccessfull</returns>
        public async Task<IActionResult> Info(int? id)
        {
            if (id == null)
            {
                TempData["Message"] =
                    new SystemMessage(MessageType.Warning, "Ingen oppgitt oppgave å lete etter").GetSystemMessage();
                return RedirectToAction(nameof(Index));
            }

            var assignment = await _context.GetAssignmentById(id);

            if (assignment == null)
            {
                TempData["Message"] =
                    new SystemMessage(MessageType.Warning, "Klarte ikke å hente informasjon om oppgaven.").GetSystemMessage();
                return RedirectToAction("Index");
            }

            var currentUser = await _userManager.GetUserAsync(HttpContext.User);
            if (assignment.Course.UserId != currentUser.Id)
            {
                TempData["Message"] = new SystemMessage(MessageType.Warning,
                    "Oppgaven tilhører en annen bruker.").GetSystemMessage();
                return RedirectToAction("Index");
            }


            return View(assignment);
            
        }
    }
}