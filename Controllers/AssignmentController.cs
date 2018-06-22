using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StudyAssistant.Web.Core.Domain;
using StudyAssistant.Web.Data;
using StudyAssistant.Web.Models;
using StudyAssistant.Web.ViewModels;

namespace StudyAssistant.Web.Controllers
{
    
    [Authorize]
    public class AssignmentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public AssignmentController(UserManager<User> userManager, ApplicationDbContext context)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(string sortOrder, string currentFilter, string searchString, int? page, bool onlyActiveAssignments)
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


            //var activeAssignments = await _context.Assignments
            //    .AsNoTracking()
            //    .Include(a => a.Course)
            //    .Where(a => a.Course.UserId == currentUser.Id)
            //    .Where(a => a.DateCompleted == null)
            //    .ToListAsync();

            return View(await PaginatedList<Assignment>.CreateAsync(
                _context.GetAssignmentsPagedList(currentUser.Id, searchString, sortOrder, descending,
                    onlyActiveAssignments), page ?? 1, pageSize));

            //return View(await _context.GetAssignments(currentUser.Id, null, true).ToListAsync());
        }

        public async Task<IActionResult> Create()
        {
            SystemMessage msg;

            var currentUser = await _userManager.GetUserAsync(HttpContext.User);
            var assignment = new Assignment();

            try
            {
                ViewBag.Courses = await _context.PopulateCourseData(currentUser.Id);
            }
            catch (Exception ex)
            {
                msg = new SystemMessage(MessageType.Warning,
                    "Fikk ikke hentet faginformasjonen. Vennligst prøv igjen senere.");
                ViewBag.Message = msg.GetSystemMessage();
            }            

            return View(assignment);
        }

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
                await _context.Assignments.AddAsync(assignment);
                var saveresult = await _context.SaveChangesAsync();
                msg = new SystemMessage(MessageType.Success, "Oppgaven ble lagret!");
            }

            catch (DbUpdateException)
            {
                msg = new SystemMessage(MessageType.Critical,
                    "Kunne ikke lagre oppgaven. Vennligst prøv igjen, og ta kontakt hvis problemet ikke løser seg.");
                ViewBag.Message = msg.GetSystemMessage();
                ViewBag.Courses = await _context.PopulateCourseData(currentUser.Id);
                return View(assignment);
            }
            
            
            TempData["Message"] = msg.GetSystemMessage();
            return RedirectToAction("Index");
        }

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