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

		#region REGISTER

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

		#endregion REGISTER

		#region LOGIN

		[HttpGet]
		public IActionResult Login() {
			return View();
		}

		[HttpPost]
		public async Task<IActionResult> LoginAsync(LoginModel model) {
			var user = await userManager.FindByNameAsync(model.UserName);
			if (ModelState.IsValid && user != null) {
				if (user == null) {
					Console.WriteLine("user not found");
				} else if (await userManager.CheckPasswordAsync(user, model.Password)) {
					Console.WriteLine("password did not match!");
				}

				if (user != null && await userManager.CheckPasswordAsync(user, model.Password)) {
					/*Add claims*/
					var Identity = new ClaimsIdentity(IdentityConstants.ApplicationScheme);
					Identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.Id));
					Identity.AddClaim(new Claim(ClaimTypes.Name, user.UserName));
					await HttpContext.SignInAsync(IdentityConstants.ApplicationScheme, new ClaimsPrincipal(Identity));
					Console.WriteLine("LOGGED IN!");

					var principal = await claimsPrincipalFactory.CreateAsync(user);

					/*2FA*/

					if (await userManager.GetTwoFactorEnabledAsync(user)) {
						var validProviders = await userManager.GetValidTwoFactorProvidersAsync(user);
						if (validProviders.Count > 0) {
							foreach (var provider in validProviders) {
								Console.WriteLine($"valid provider: {provider}");
							}
						} else {
							Console.WriteLine("No valid providers found");
						}

						if (validProviders.Contains("Email")) {
							var token = await userManager.GenerateTwoFactorTokenAsync(user, "Email");
							System.IO.File.WriteAllText("email2sv.txt", token);
							//Send token to email or phone

							await HttpContext.SignInAsync(IdentityConstants.TwoFactorUserIdScheme, Store2FA(user.Id, "Email"));
							return RedirectToAction("TwoFactor");
						} else {
							Console.WriteLine("Valid providers does not contain EMAIL");
							return View();
						}
					} else {
						Console.WriteLine("two factor not enabled!");
						return View();
					}

					/*Sign In*/
					await HttpContext.SignInAsync(IdentityConstants.ApplicationScheme, principal);
				}

				//var signInResult = await signInManager.PasswordSignInAsync(model.UserName, model.Password, false, false);
				//var isLockedOut = await userManager.IsLockedOutAsync(user);
				//if (signInResult.Succeeded && !isLockedOut) {
				//	Console.WriteLine("Logged in!");
				//	await userManager.SetLockoutEndDateAsync(user, DateTime.UtcNow);
				//	return RedirectToAction("Index");
				//} else if (isLockedOut) {
				//	await userManager.ResetAccessFailedCountAsync(user);
				//	Console.WriteLine("User is lockedout for x minutes");
				//	//send email about lockout
				//	//email to link where userManager.ResetAccessFailedCountAsync(user)
				//} else {
				//	Console.WriteLine("Lockout increment");
				//	await userManager.AccessFailedAsync(user);
				//}

				//ModelState.AddModelError("", "Invalid Credentials");
			}
			Console.WriteLine("Modelstate invalid");
			return View();
		}

		#endregion LOGIN

		#region CONFIRM EMAIL

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

		#endregion CONFIRM EMAIL

		#region FORGOT PASSWORD

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

		#endregion FORGOT PASSWORD

		#region RESET PASSWORD

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

		#endregion RESET PASSWORD

		#region TWO FACTOR AUTH

		private ClaimsPrincipal Store2FA(string userId, string provider) {
			var identity = new ClaimsIdentity(new List<Claim> {
				new Claim("sub", userId),
				new Claim("amr", provider)
			}, IdentityConstants.TwoFactorUserIdScheme);
			return new ClaimsPrincipal(identity);
		}

		[HttpGet]
		public IActionResult TwoFactor() {
			return View();
		}

		[HttpPost]
		public async Task<IActionResult> TwoFactorAsync(TwoFactorModel model) {
			var result = await HttpContext.AuthenticateAsync(IdentityConstants.TwoFactorUserIdScheme);

			if (!result.Succeeded) {
				ModelState.AddModelError("", "login request has expired");
				return View();
			}

			if (ModelState.IsValid) {
				var user = await userManager.FindByIdAsync(result.Principal.FindFirstValue("sub"));

				if (user != null) {
					var isValid = await userManager.VerifyTwoFactorTokenAsync(user, result.Principal.FindFirstValue("amr"), model.Token);
					if (isValid) {
						await HttpContext.SignOutAsync(IdentityConstants.TwoFactorUserIdScheme);

						var claimsPrincipal = await claimsPrincipalFactory.CreateAsync(user);
						await HttpContext.SignInAsync(IdentityConstants.ApplicationScheme, claimsPrincipal);

						return RedirectToAction("Index");
					}
					ModelState.AddModelError("", "Invalid token");
					return View();
				}
				ModelState.AddModelError("", "invalid request");
			}
			return View();
		}

		#endregion TWO FACTOR AUTH

		#region Error

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error() {
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}

		#endregion Error
	}
}