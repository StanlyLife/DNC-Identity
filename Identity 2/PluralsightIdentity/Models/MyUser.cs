using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PluralsightIdentity.Models {

	public class MyUser : IdentityUser {
		public string Locale { get; set; } = "NO-osl";

		public string OrgId { get; set; }
	}

	public class Organization {
		public string Id { get; set; }
		public string Name { get; set; }
	}
}