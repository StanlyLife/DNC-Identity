using System.ComponentModel.DataAnnotations;

namespace PluralsightIdentity.Controllers {

	public class ForgotPassword {

		[Required]
		[EmailAddress]
		public string Email { get; set; }
	}
}