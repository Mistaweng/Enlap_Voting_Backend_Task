using CloudinaryDotNet;
using Enlap_Voting_Backend.Context;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json.Serialization;
using System.Text;
using Enlap_Voting_Backend.Services;
using Enlap_Voting_Backend.Implementations;
using Enlap_Voting_Backend.Models;
using Enlap_Voting_Backend.Enums;
using Enlap_Voting_Backend.Helpers;

namespace Enlap_Voting_Backend.Extensions
{
	public static class DbRegistryExtension
	{
		public static void AddDbContextAndConfigurations(this IServiceCollection services, IConfiguration configuration)
		{

			services.AddIdentity<AppUser, AppUserRole>() 
					.AddEntityFrameworkStores<ApplicationDbContext>()
					.AddDefaultTokenProviders();

			services.AddScoped<RoleManager<AppUserRole>>();

			services.AddAuthorization(options =>
			{
				options.AddPolicy("AdminPolicy", policy => policy.RequireRole(UserRoles.Admin.ToString()));
				options.AddPolicy("VoterPolicy", policy => policy.RequireRole(UserRoles.Voter.ToString()));
				options.AddPolicy("ContestantPolicy", policy => policy.RequireRole(UserRoles.Contestant.ToString()));
			});
			services.AddDbContextPool<ApplicationDbContext>(options =>
			{
				options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));
			});


			services.AddControllers()
					.AddJsonOptions(options =>
					{
						options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;
					});

			var cloudinarySettings = configuration.GetSection("Cloudinary");

			var account = new Account(
				cloudinarySettings["CloudName"],
				cloudinarySettings["ApiKey"],
				cloudinarySettings["ApiSecret"]);

			var cloudinary = new Cloudinary(account);

			services.AddSingleton(cloudinary);




			//Password configuration
			services.Configure<IdentityOptions>(options =>
			{
				options.Password.RequireDigit = true;
				options.Password.RequireLowercase = true;
				options.Password.RequireNonAlphanumeric = true;
				options.Password.RequireUppercase = true;
			});


			// Repo Registration
			services.AddScoped<IAuthService, AuthService>();
			services.AddScoped<IAdminSignupService, AdminSignupService>();
			services.AddScoped<IVoterSignupService, VoterSignupService>();
			services.AddScoped<IContestantSignupService, ContestantSignupService>();


			// Connecting frontend
			services.AddCors(options =>
			{
				options.AddPolicy("AllowAllOrigins", builder =>
				{
					builder
						.WithOrigins("http://localhost:3000", "https://enlabhub-frontend.vercel.app")
						.AllowAnyHeader()
						.AllowAnyMethod()
						.AllowCredentials();
				});
			});

		}
	}


	public static class RoleSeederExtensions
	{

		public static async Task SeedRolesAsync(this IApplicationBuilder app)
		{
			using (var scope = app.ApplicationServices.CreateScope())
			{
				var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<AppUserRole>>();
				await RoleSeeder.SeedRolesAsync(roleManager);
			}
		}	

		public static void SeedRoles(this IApplicationBuilder app)
		{
			using (var scope = app.ApplicationServices.CreateScope())
			{
				var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<AppUserRole>>();
				RoleSeeder.SeedRolesAsync(roleManager).Wait();
			}
		}
	}



}
