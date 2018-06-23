using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using studyAssistant.Core.Domain;
using studyAssistant.Data;
using studyAssistant.Models;
using studyAssistant.ViewModels;

namespace studyAssistant.Controllers
{
    
    [Authorize]
    public class StudySessionController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public StudySessionController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        
        public async Task<IActionResult> Index(string sortOrder, string currentFilter, string searchString, int? page, bool onlyActiveStudySessions)
        {
            var currentUser = await _userManager.GetUserAsync(HttpContext.User);
            
            const int pageSize = 10;

            ViewData["CurrentSort"] = sortOrder;
            ViewData["sortTitleParam"] = String.IsNullOrEmpty(sortOrder) ? "Title_desc" : "";
            ViewData["sortDateParam"] = sortOrder == "StartDate" ? "StartDate_desc" : "StartDate";
            ViewData["sortCourseParam"] = sortOrder == "CourseId" ? "CourseId_desc" : "CourseId";
            ViewData["sortSessionTypeParam"] = sortOrder == "SessionTypeId" ? "SessionTypeId_desc" : "SessionTypeId";
            if (onlyActiveStudySessions)
            {
                ViewData["onlyActiveStudySessions"] = "checked";
                ViewData["onlyActiveStudySessionsValue"] = true;
            }
            else
            {
                ViewData["onlyActiveStudySessionsValue"] = false;
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


            //var activeStudySessions = await _context.GetStudySessionsByUser(currentUser.Id, true).ToListAsync();
            
            //if (activeStudySessions != null)
            //{
            //    return View(activeStudySessions);
            //}
          
            return View(await PaginatedList<StudySession>.CreateAsync(
                _context.GetStudySessionsPagedList(currentUser.Id, searchString, sortOrder, descending,
                    onlyActiveStudySessions), page ?? 1, pageSize));
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            try
            {
                await PrepareCreateEditView();
                var studySession = new StudySession
                {
                        StartDate = DateTime.Now.Date,
                        StartTime = DateTime.Now.TimeOfDay,
                        Duration = new TimeSpan(1, 0, 0)
                };
                return View(studySession);
            }
            catch (Exception)
            {
                TempData["Message"] = new SystemMessage(MessageType.Critical,
                    "Det oppstod en feil under henting av informasjon fra databasen.").GetSystemMessage();
            }

            return RedirectToAction("Index");
        }

        protected async Task<bool> StudySessionConflictExists(StudySession newSession)
        {
            var currentUser = await _userManager.GetUserAsync(HttpContext.User);
            var activeStudySessions = await _context.GetStudySessionsByUser(currentUser.Id, true).ToListAsync();
            DateTime newSessionStart, newSessionEnd, existingSessionStart, existingSessionEnd;

            bool studySessionConflict = false;

            if (activeStudySessions.Count > 0)
            {
                activeStudySessions.Remove(newSession);
                foreach (var existingSession in activeStudySessions)
                {
                    newSessionStart = newSession.GetStudySessionStart();
                    newSessionEnd = newSession.GetStudySessionEnd();
                    existingSessionStart = existingSession.GetStudySessionStart();
                    existingSessionEnd = existingSession.GetStudySessionEnd();

                     studySessionConflict = newSessionStart < existingSessionEnd && existingSessionStart < newSessionEnd;
                    
                    if (studySessionConflict)
                    {
                        studySessionConflict = true;
                        break;
                    }
                }
            }
            return studySessionConflict;
        }
 [HttpPost]
 [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(StudySession model)
        {
            

            if (!ModelState.IsValid)
            {
                await PrepareCreateEditView();
                return View(model);
            }

            try
            {

                if (await StudySessionConflictExists(model))
                {
                    TempData["Message"] = new SystemMessage(MessageType.Warning,
                        "Studieøkten du planlegger kolliderer med en som allerede er planlagt.").GetSystemMessage();
                    await PrepareCreateEditView();
                    return View(model);
                }

                _context.StudySessions.Add(model);
                await _context.SaveChangesAsync();
                TempData["Message"] =
                    new SystemMessage(MessageType.Success, $"Studieøkten {model.Title} ble lagret.").GetSystemMessage();
            }
            catch (Exception)
            {
                TempData["Message"] = new SystemMessage(MessageType.Critical, "Det oppstod en feil under lagring")
                    .GetSystemMessage();
                await PrepareCreateEditView();
                return View(model);
            }

            return RedirectToAction("Index");
        }

        private async Task PrepareCreateEditView()
        {
            var currentUser = await _userManager.GetUserAsync(HttpContext.User);
            ViewBag.Courses = await _context.GetCourses(currentUser.Id, true).ToListAsync();
            ViewBag.StudySessionTypes = await _context.StudySessionTypes.ToListAsync();
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                TempData["Message"] =
                    new SystemMessage(MessageType.Warning, "Trenger id til en studieøkt for å slette den.")
                        .GetSystemMessage();
                return RedirectToAction("Index");
            }
            var currentUser = await _userManager.GetUserAsync(HttpContext.User);

            var studySession = await _context.GetStudySessionById(id);

            if (studySession.Course.UserId != currentUser.Id)
            {
                TempData["Message"] =
                    new SystemMessage(MessageType.Warning, "Studieøkten du ønsker å endre, tilhører en annen bruker.")
                        .GetSystemMessage();
                return RedirectToAction("Index");
            }

            if (studySession == null)
            {
                TempData["Message"] =
                    new SystemMessage(MessageType.Warning, "Fant ikke studieøkten.").GetSystemMessage();
                return RedirectToAction("Index");
            }
            

            if (studySession.Course.UserId != currentUser.Id)
            {
                TempData["Message"] = new SystemMessage(MessageType.Warning,
                    "Oppgaven du ønsker å slette, tilhører en annen bruker.").GetSystemMessage();
                return RedirectToAction("Index");
            }

           return View(studySession);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            await PrepareCreateEditView();
            var vm = await _context.GetStudySessionById(id);
            if ( vm == null)
            {
                TempData["Message"] =
                    new SystemMessage(MessageType.Warning, "Fant ikke studieøkten.").GetSystemMessage();
                return RedirectToAction("Index");
            }

            var currentUser = await _userManager.GetUserAsync(HttpContext.User);

            if (vm.Course.UserId != currentUser.Id)
            {
                TempData["Message"] =
                    new SystemMessage(MessageType.Warning, "Studieøkten du ønsker å endre, tilhører en annen bruker.")
                        .GetSystemMessage();
                return RedirectToAction("Index");
            }
            return View(vm);
        }

        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPost(int? id)
        {
            StudySession studySessionToUpdate = await _context.GetStudySessionById(id);

            var updateResult = await TryUpdateModelAsync(studySessionToUpdate);
           
            if (updateResult)
            {
                try
                {
                    if (await StudySessionConflictExists(studySessionToUpdate))
                    {
                        TempData["Message"] = new SystemMessage(MessageType.Warning,
                            "Studieøkten du planlegger kolliderer med en som allerede er planlagt.").GetSystemMessage();
                        await PrepareCreateEditView();
                        return View(studySessionToUpdate);
                    }
                    await _context.SaveChangesAsync();
                    TempData["Message"] = new SystemMessage(MessageType.Success, "Studieøkten ble oppdatert.")
                        .GetSystemMessage();
                    return RedirectToAction("Index");
                }
                catch (Exception)
                {
                    TempData["Message"] =
                        new SystemMessage(MessageType.Critical, "Det oppstod en feil under oppdatering av studieøkten.")
                            .GetSystemMessage();
                }
            }
            return View(studySessionToUpdate);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var studySession = await _context.GetStudySessionById(id);

            if (studySession == null)
            {
                TempData["Message"] =
                    new SystemMessage(MessageType.Warning, "Fant ikke studieøkten.").GetSystemMessage();
                return RedirectToAction("Index");
            }

            try
            {
                _context.StudySessions.Remove(studySession);
                await _context.SaveChangesAsync();
                TempData["Message"] = new SystemMessage(MessageType.Success, "Studieøkten ble slettet.").GetSystemMessage();
            }
            catch (Exception)
            {
                TempData["Message"] = new SystemMessage(MessageType.Critical, "Det oppstod en feil under sletting.")
                    .GetSystemMessage();
            }

            return RedirectToAction("Index");
        }

        public async Task<ActionResult> Info(int? id)
        {
            if (id != null)
            {
                var studySession = await _context.GetStudySessionById(id);   
                
                if (studySession == null)
                {
                    TempData["Message"] =
                        new SystemMessage(MessageType.Warning, "Fant ikke studieøkten.").GetSystemMessage();
                    return RedirectToAction("Index");
                }

                var currentUser = await _userManager.GetUserAsync(HttpContext.User);
                if (studySession.Course.UserId != currentUser.Id)
                {
                    TempData["Message"] =
                        new SystemMessage(MessageType.Warning, "Studieøkten du ønsker å vise, tilhører en annen bruker.")
                            .GetSystemMessage();
                    return RedirectToAction("Index");
                }
                
                return View(studySession);
            }
            else
            {
                TempData["Message"] = new SystemMessage(MessageType.Warning, "Ingen id for studieøkten er oppgitt.")
                    .GetSystemMessage();
                return RedirectToAction("Index");
            }

        }

        public async Task<IActionResult> Finish(int id)
        {
            var updateResult = await _context.FinishStudySessionAsync(id);
            
            if (updateResult > 0)
            {
                TempData["Message"] =
                    new SystemMessage(MessageType.Success, "Statusen på studieøkten er endret til gjennomført.")
                        .GetSystemMessage();
            }
            else
            {
                TempData["Message"] =
                    new SystemMessage(MessageType.Critical,
                        "Det oppstod en feil under endring av status på studieøkten.").GetSystemMessage();
            }

            return RedirectToAction("Index");
        }
    }
}