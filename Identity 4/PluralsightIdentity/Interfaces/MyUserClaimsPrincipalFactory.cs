﻿using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using PluralsightIdentity.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PluralsightIdentity.Interfaces {

	public class MyUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<MyUser> {

		public MyUserClaimsPrincipalFactory(UserManager<MyUser> userManager, IOptions<IdentityOptions> optionsAccessor) : base(userManager, optionsAccessor) {
		}

		protected override async Task<ClaimsIdentity> GenerateClaimsAsync(MyUser user) {
			var identity = await base.GenerateClaimsAsync(user);
			identity.AddClaim(new Claim("locale", user.Locale));
			return identity;
		}
	}
}