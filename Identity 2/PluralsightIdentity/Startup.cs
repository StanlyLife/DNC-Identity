using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using PluralsightIdentity.Data;
using PluralsightIdentity.Interfaces;
using PluralsightIdentity.Models;

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

			services.AddIdentityCore<MyUser>(options => {
				options.Password.RequiredLength = 1;
				options.Password.RequireNonAlphanumeric = false;
				options.Password.RequireDigit = false;
			});

			services.AddAuthentication("Name.Of.Scheme").AddCookie("Name.Of.Scheme", options => {
				options.LoginPath = "/home/login";
			});

			services.AddScoped<IUserStore<MyUser>,
			UserOnlyStore<MyUser, MyApplicationDbContext>>();
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