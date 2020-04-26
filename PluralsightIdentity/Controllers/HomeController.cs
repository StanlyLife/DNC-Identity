using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PluralsightIdentity.Models;

namespace PluralsightIdentity.Controllers {

	public class HomeController : Controller {
		private readonly ILogger<HomeController> _logger;
		private readonly UserManager<MyUser> userManager;

		public HomeController(ILogger<HomeController> logger, UserManager<MyUser> userManager) {
			_logger = logger;
			this.userManager = userManager;
		}

		public IActionResult Index() {
			return View();
		}

		[Authorize]
		public IActionResult About() {
			return View();
		}

		public IActionResult Privacy() {
			return View();
		}

		[HttpGet]
		public IActionResult Register() {
			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> RegisterAsync(RegisterModel model) {
			if (ModelState.IsValid) {
				var user = await userManager.FindByNameAsync(model.UserName);

				if (user == null) {
					user = new MyUser {
						Id = Guid.NewGuid().ToString(),
						UserName = model.UserName
					};

					var result = await userManager.CreateAsync(user, model.Password);
				}

				return View("Success");
			}

			return View();
		}

		[HttpGet]
		public IActionResult Login() {
			return View();
		}

		[HttpPost]
		public async Task<IActionResult> LoginAsync(LoginModel model) {
			if (ModelState.IsValid) {
				var user = await userManager.FindByNameAsync(model.UserName);

				if (user != null && await userManager.CheckPasswordAsync(user, model.Password)) {
					var Identity = new ClaimsIdentity("NameOfIssuer");
					Identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.Id));
					Identity.AddClaim(new Claim(ClaimTypes.Name, user.UserName));
					await HttpContext.SignInAsync("NameOfPrincipal", new ClaimsPrincipal(Identity));

					return RedirectToAction("Index");
				}

				ModelState.AddModelError("", "Invalid Credentials");
			}
			return View();
		}

		#region Error

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error() {
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}

		#endregion Error
	}
}