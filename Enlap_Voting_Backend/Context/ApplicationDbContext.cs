using Enlap_Voting_Backend.Implementations;
using Enlap_Voting_Backend.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata;

namespace Enlap_Voting_Backend.Context
{
	public class ApplicationDbContext : DbContext
	{
		public DbSet<AppUser> AppicationUsers { get; set; }
		public DbSet<AppUserRole> AppUserRoles { get; set; }
		public DbSet<UserOTP> userOTPs { get; set; }
        public DbSet<AppUserPermission> Permissions { get; set; }
        //public DbSet<Election> Elections { get; set; }
        //public DbSet<Contestant> Contestants { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
			: base(options)
		{
		}

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			// Fluent API configurations
			// Configure relationships, indexes, etc.
		}
	}

}
