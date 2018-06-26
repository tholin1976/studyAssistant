using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using studyAssistant.Core.Domain;
using studyAssistant.Data;
using studyAssistant.Models;
using studyAssistant.ViewModels;

namespace studyAssistant.Controllers
{
	/// <summary>
	/// Handles all the requests dealing with user accounts
	/// </summary>
	[Authorize]
	public class AccountController : Controller
	{
		private readonly UserManager<User> _userManager;
		private readonly SignInManager<User> _signInManager;
        private readonly ApplicationDbContext _context;

		/// <summary>
		/// Class constructor
		/// </summary>
		/// <param name="userManager">The userManager object that gets injected into the controller</param>
		/// <param name="signInManager">The signInManager object that gets injected into the controller</param>
		/// <param name="context">The ApplicationDbContext object that gets injected into the controller</param>
		public AccountController(
				UserManager<User> userManager,
				SignInManager<User> signInManager,
				ApplicationDbContext context)
		{
			_userManager = userManager;
			_signInManager = signInManager;
            _context = context;
		}

		/// <summary>
		/// Returns the view /Account/Login.cshtml
		/// </summary>
		/// <param name="returnUrl">The users's url of origin</param>
		/// <returns></returns>
		[AllowAnonymous]
		[HttpGet]
		public IActionResult Login(string returnUrl)
        {
            ViewBag.returnUrl = returnUrl;
            return View();
        }

		/// <summary>
		/// Logs the user in to the system and redirects him / her to the dashboard if successful
		/// </summary>
		/// <param name="details">The user's login details</param>
		/// <param name="returnUrl"></param>
		/// <returns>Redirect if successful, the Login view if not</returns>
		[HttpPost]
		[AllowAnonymous]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Login(AccountLoginViewModel details, string returnUrl)
		{
			if (ModelState.IsValid)
			{
				var user = await _userManager.FindByNameAsync(details.UserName);
				
			    if (user != null)
				{
				   var result = await _signInManager.PasswordSignInAsync(user, details.Password, false, false);
					if (result.Succeeded)
					{
						return Redirect(returnUrl ?? "/Dashboard");
					}
					else
					{
					    TempData["Message"] = new SystemMessage(MessageType.Warning, "Feil passord.").GetSystemMessage();
					}
				}
			    else
			    {
			        TempData["Message"] = new SystemMessage(MessageType.Warning, "Ukjent brukernavn.").GetSystemMessage();
			    }
			}
			return View(details);
		}

		/// <summary>
		/// Logs out the user and then redirects him / her to the homepage
		/// </summary>
		/// <returns>RedirectToAction</returns>
        public async Task<IActionResult> Logout()
	    {
	        await _signInManager.SignOutAsync();
	        return RedirectToAction("Index", "Home");
	    }

		/// <summary>
		/// Returns the view /Account/Create.cshtml
		/// </summary>
		/// <returns></returns>
        [AllowAnonymous]
        [HttpGet]
		public ViewResult Create()
		{
            return View();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="model">Object containing the new user account data</param>
		/// <param name="returnUrl">The url the user will be redirected to</param>
		/// <returns>Returns a RedirectToAction</returns>
        [AllowAnonymous]
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(AccountCreateViewModel model, string returnUrl)
		{
            
			if (ModelState.IsValid)
			{
				User user = new User
				{
					UserName = model.UserName,
					Email = model.Email
				};

			    var existingUser = await _userManager.FindByNameAsync(user.UserName);

			    if (existingUser != null)
			    {
			        TempData["Message"] =
			            new SystemMessage(MessageType.Warning, "Brukernavnet er opptatt. Vennligst velg et annet.")
			                .GetSystemMessage();
			        return RedirectToAction("Create", model);
			    }

				IdentityResult result = await _userManager.CreateAsync(user, model.Password);
				
                // If a new user was created..
				if (result.Succeeded)
				{
                    // Redirect user to the location specified in returnUrl if not empty, else redirect to /Dashboard
				    TempData["Message"] = new SystemMessage(MessageType.Success, "Din konto ble registrert. Velkommen!")
				        .GetSystemMessage();
				    await _signInManager.PasswordSignInAsync(user, model.Password, false, false);
                    return Redirect(returnUrl ?? "/Dashboard");
                }
                // A new user was NOT created for some reason
                else
				{
				    TempData["Message"] =
				        new SystemMessage(MessageType.Critical, "Det oppstod en feil under oppretting av brukerkontoen din.")
				            .GetSystemMessage();
				}
			}

			return View(model);
		}


	}
}