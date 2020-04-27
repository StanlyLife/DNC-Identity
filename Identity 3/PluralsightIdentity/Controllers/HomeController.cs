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
						Email = model.UserName
					};

					var result = await userManager.CreateAsync(user, model.Password);
					if (result.Succeeded) {
						var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
						var confirmationEmail = Url.Action("ConfirmEmailAdress", "Home", new { token = token, email = user.Email }, Request.Scheme);
						//Send link to email
						System.IO.File.WriteAllText("ConfirmEmailLink.txt", confirmationEmail);
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
		public async Task<IActionResult> ConfirmEmailAdress(string token, string email) {
			var user = await userManager.FindByEmailAsync(email);
			if (user != null) {
				var result = await userManager.ConfirmEmailAsync(user, token);
				if (result.Succeeded) {
					return View("Success");
				}
			}
			return View("Error");
		}

		[HttpGet]
		public IActionResult Login() {
			return View();
		}

		[HttpPost]
		public async Task<IActionResult> LoginAsync(LoginModel model) {
			var user = await userManager.FindByNameAsync(model.UserName);
			if (ModelState.IsValid && user != null) {
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
				var isLockedOut = await userManager.IsLockedOutAsync(user);
				if (signInResult.Succeeded && !isLockedOut) {
					Console.WriteLine("Logged in!");
					return RedirectToAction("Index");
				} else if (isLockedOut) {
					Console.WriteLine("User is lockedout for x minutes");
					//send email about lockout
				} else {
					Console.WriteLine("Lockout increment");
					await userManager.AccessFailedAsync(user);
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
		public async Task<IActionResult> ForgotPassWordAsync(ForgotPassword model) {
			if (ModelState.IsValid) {
				var user = await userManager.FindByEmailAsync(model.Email);
				if (user != null) {
					var token = await userManager.GeneratePasswordResetTokenAsync(user);
					var resetUrl = Url.Action("ResetPassword", "Home",
						new { token = token, email = user.Email }, Request.Scheme);

					System.IO.File.WriteAllText("resetLink.txt", resetUrl);
					//Send email to user
					return View("Success");
				} else {
					//Send email, you do not have an account
					Console.WriteLine("No account with that username found");
				}
			}

			return View();
		}

		[HttpGet]
		public IActionResult ResetPassWord(string token, string email) {
			return View(new ResetPasswordModel { Token = token, Email = email });
		}

		[HttpPost]
		public async Task<IActionResult> ResetPassWordAsync(ResetPasswordModel model) {
			if (ModelState.IsValid) {
				var user = await userManager.FindByEmailAsync(model.Email);

				if (user != null) {
					var result = await userManager.ResetPasswordAsync(user, model.Token, model.Password);

					if (!result.Succeeded) {
						foreach (var error in result.Errors) {
							ModelState.AddModelError("", error.ToString());
						}
						Console.WriteLine("Failed");
						return View();
					}
					return View("Success");
				}
				ModelState.AddModelError("", "Invalid Request!");
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