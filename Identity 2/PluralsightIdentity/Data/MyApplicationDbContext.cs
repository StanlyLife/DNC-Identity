using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PluralsightIdentity.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PluralsightIdentity.Data {

	public class MyApplicationDbContext : IdentityDbContext<MyUser> {

		public MyApplicationDbContext(DbContextOptions<MyApplicationDbContext> options) : base(options) {
			Database.EnsureCreated();
		}

		protected override void OnModelCreating(ModelBuilder builder) {
			base.OnModelCreating(builder);

			builder.Entity<MyUser>(user => {
				user.HasIndex(x =>
					x.Locale
				).IsUnique(false);
			});

			builder.Entity<Organization>(org => {
				org.ToTable("Organizations");
				org.HasKey(x => x.Id);

				org.HasMany<MyUser>().WithOne().HasForeignKey(X => X.OrgId).IsRequired(false);
			});
		}
	}
}