using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PluralsightIdentity.Models {

	public class MyUser {
		public string Id { get; set; }
		public string UserName { get; set; }
		public string NormalizedUsername { get; set; }
		public string PasswordHash { get; set; }
	}
}