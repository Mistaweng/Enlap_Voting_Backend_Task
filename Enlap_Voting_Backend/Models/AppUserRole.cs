using Microsoft.AspNetCore.Identity;

namespace Enlap_Voting_Backend.Models
{
	public class AppUserRole : IdentityRole<string>
	{
        public Guid Id { get; set; }
        public string Name { get; set; }
    }
}
