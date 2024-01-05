using System.ComponentModel.DataAnnotations;

namespace Enlap_Voting_Backend.Dtos
{
	public class LoginDto
	{


		[Required]
		[EmailAddress]
		public string Email { get; set; }

		[Required]
		[DataType(DataType.Password)]
		public string Password { get; set; }
	}
}
