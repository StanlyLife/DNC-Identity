using System.ComponentModel.DataAnnotations;

namespace PluralsightIdentity.Controllers {

	public class TwoFactorModel {

		[Required]
		public string Token { get; set; }
	}
}