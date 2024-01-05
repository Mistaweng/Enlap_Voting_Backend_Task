

using Enlap_Voting_Backend.Enums;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Enlap_Voting_Backend.Models
{
	public class AppUser : IdentityUser
	{
        public string FirstName { get; set; }
		public string LastName { get; set; }
        //public string Email { get; set; }
        [Required]
        public string Password { get; set; }
        //public string UserName { get; set; }
        public UserRoles UserRole { get; set; } // Admin, Voter, Contestant

		//public override string UserName
		//{
		//	get => Email;
		//	set => Email = value;
		//}
	}

	
}
