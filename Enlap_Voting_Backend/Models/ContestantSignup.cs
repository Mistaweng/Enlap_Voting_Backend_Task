namespace Enlap_Voting_Backend.Models
{
	public class ContestantSignup : AppUser
	{
		public string ValidId { get; set; }
		public string PictureUrl { get; set; } // Stored in Cloudinary
	}
}
