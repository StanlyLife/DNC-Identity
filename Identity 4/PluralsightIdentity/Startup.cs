using System;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PluralsightIdentity.Data;
using PluralsightIdentity.Interfaces;
using PluralsightIdentity.Models;
using PluralsightIdentity.TokenProviders;

namespace PluralsightIdentity {

	public class Startup {

		public Startup(IConfiguration configuration) {
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		public void ConfigureServices(IServiceCollection services) {
			services.AddControllersWithViews();

			services.AddDbContext<MyApplicationDbContext>(options => {
				var connectionString = "Data Source=(LocalDb)\\MSSQLLocalDB;" +
											   "database=DncIdentity2;" +
											   "trusted_connection=yes;";
				var myMigrationAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;
				options.UseSqlServer(connectionString, sql => {
					sql.MigrationsAssembly(myMigrationAssembly);
				});
			});

			services.AddIdentity<MyUser, IdentityRole>(options => {
				options.SignIn.RequireConfirmedEmail = true;
				options.Password.RequiredLength = 5;
				options.Password.RequireNonAlphanumeric = false;
				options.Password.RequireDigit = false;

				options.User.RequireUniqueEmail = true;
				options.Tokens.EmailConfirmationTokenProvider = "emailConf";

				options.Lockout.AllowedForNewUsers = true;
				options.Lockout.MaxFailedAccessAttempts = 3;
				options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
			}).AddEntityFrameworkStores<MyApplicationDbContext>()
			.AddDefaultTokenProviders()
			.AddTokenProvider<EmailConfirmationTokenProvider<MyUser>>("emailConf")
			.AddPasswordValidator<DoesNotContainPasswordValidator<MyUser>>();

			services.Configure<DataProtectionTokenProviderOptions>(options => {
				options.TokenLifespan = TimeSpan.FromMinutes(30);
			});

			services.Configure<EmailConfirmationTokenProviderOptions>(options => {
				options.TokenLifespan = TimeSpan.FromDays(7);
			});

			/*This is not needed as we changed AddIndentityCore to AddIdentity*/
			/*AddIdentity provides its own authentication*/
			//services.AddAuthentication("Name.Of.Scheme").AddCookie("Name.Of.Scheme", options => {
			//	options.LoginPath = "/home/login";
			//});

			services.ConfigureApplicationCookie(o => o.LoginPath = "/home/login");

			services.AddScoped<IUserStore<MyUser>,
			UserOnlyStore<MyUser, MyApplicationDbContext>>();

			services.AddScoped<IUserClaimsPrincipalFactory<MyUser>, MyUserClaimsPrincipalFactory>();
		}

		public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
			if (env.IsDevelopment()) {
				app.UseDeveloperExceptionPage();
			} else {
				app.UseExceptionHandler("/Home/Error");
				app.UseHsts();
			}
			app.UseHttpsRedirection();
			app.UseStaticFiles();

			app.UseRouting();

			app.UseAuthentication();
			app.UseAuthorization();

			app.UseEndpoints(endpoints => {
				endpoints.MapControllerRoute(
					name: "default",
					pattern: "{controller=Home}/{action=Index}/{id?}");
			});
		}
	}
}