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

			services.AddIdentity<MyUser, IdentityRole>(options => {
				options.Password.RequiredLength = 1;
				options.Password.RequireNonAlphanumeric = false;
				options.Password.RequireDigit = false;
			}).AddEntityFrameworkStores<MyApplicationDbContext>();

			/*This is not needed as we changed AddIndentityCore to AddIdentity*/
			/*AddIdentity provides its own authentication*/
			//services.AddAuthentication("Name.Of.Scheme").AddCookie("Name.Of.Scheme", options => {
			//	options.LoginPath = "/home/login";
			//});

			services.ConfigureApplicationCookie(o => o.LoginPath = "/home/login");

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