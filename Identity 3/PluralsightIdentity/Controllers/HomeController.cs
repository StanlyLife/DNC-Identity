using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
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
		private readonly IUserClaimsPrincipalFactory<MyUser> claimsPrincipalFactory;
		private readonly SignInManager<MyUser> signInManager;

		public HomeController(ILogger<HomeController> logger, UserManager<MyUser> userManager, IUserClaimsPrincipalFactory<MyUser> claimsPrincipalFactory, SignInManager<MyUser> signInManager) {
			_logger = logger;
			this.userManager = userManager;
			this.claimsPrincipalFactory = claimsPrincipalFactory;
			this.signInManager = signInManager;
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
					Console.WriteLine($"Username = {model.UserName}");
					user = new MyUser {
						Id = Guid.NewGuid().ToString(),
						UserName = model.UserName,
					};

					var result = await userManager.CreateAsync(user, model.Password);
					if (result.Succeeded) {
						return View("Success");
					} else {
						foreach (var err in result.Errors.ToList()) {
							Console.WriteLine($"Error: {err}");
						}
						return View();
					}
				}
				Console.WriteLine("User already exists");
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
				/*
				if (user != null && await userManager.CheckPasswordAsync(user, model.Password)) {
					//var Identity = new ClaimsIdentity("Identity.Application");
					//Identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.Id));
					//Identity.AddClaim(new Claim(ClaimTypes.Name, user.UserName));
					//await HttpContext.SignInAsync("Identity.Application", new ClaimsPrincipal(Identity));
					//Console.WriteLine("LOGGED IN!");

					var principal = await claimsPrincipalFactory.CreateAsync(user);

					await HttpContext.SignInAsync("Identity.Application", principal);
				}
				*/

				var signInResult = await signInManager.PasswordSignInAsync(model.UserName, model.Password, false, false);

				if (signInResult.Succeeded) {
					Console.WriteLine("Logged in!");
					return RedirectToAction("Index");
				}

				ModelState.AddModelError("", "Invalid Credentials");
			}
			Console.WriteLine("Modelstate invalid");
			return View();
		}

		[HttpGet]
		public IActionResult ForgotPassWord() {
			return View();
		}

		[HttpPost]
		public IActionResult ForgotPassWord(ForgotPassword model) {
			return View();
		}

		[HttpGet]
		public IActionResult ResetPassWord() {
			return View();
		}

		[HttpPost]
		public IActionResult ResetPassWord(ResetPasswordModel model) {
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