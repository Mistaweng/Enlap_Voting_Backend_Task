namespace Enlap_Voting_Backend.Models
{
	public class UserOTP : BaseEntity
	{
		public string Email { get; set; }
		public string OTP { get; set; }
		public DateTime Expiration { get; set; }
	}
}
