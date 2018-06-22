using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StudyAssistant.Web.Core.Domain;
using StudyAssistant.Web.Data;
using StudyAssistant.Web.Models;
using StudyAssistant.Web.ViewModels;

namespace StudyAssistant.Web.Controllers
{
	[Authorize]
	public class AccountController : Controller
	{
		private readonly UserManager<User> _userManager;
		private readonly SignInManager<User> _signInManager;
        private readonly ApplicationDbContext _context;

		public AccountController(
				UserManager<User> userManager,
				SignInManager<User> signInManager,
				ApplicationDbContext context)
		{
			_userManager = userManager;
			_signInManager = signInManager;
            _context = context;
		}

		
		[AllowAnonymous]
		[HttpGet]
		public IActionResult Login(string returnUrl)
        {
            ViewBag.returnUrl = returnUrl;
            return View();
        }

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

        public async Task<IActionResult> Logout()
	    {
            // Logs out user and then redirects him / her to homepage
	        await _signInManager.SignOutAsync();
	        return RedirectToAction("Index", "Home");
	    }

        [AllowAnonymous]
        [HttpGet]
		public ViewResult Create()
		{
            return View();
		}

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