using Microsoft.EntityFrameworkCore;

namespace PlatformService.Data
{
	public static class PrepDb
	{
		public static void PrepPopulation(IApplicationBuilder app, bool isProd)
		{
			using( var serviceScope = app.ApplicationServices.CreateScope())
			{
				SeedData(serviceScope.ServiceProvider.GetService<AppDbContext>(), isProd);
			}
		}

		private static void SeedData(AppDbContext context, bool isProd)
		{
			if (isProd)
			{
				Console.WriteLine("--> Attempting to aplly migrations...");
				try
				{
                    context.Database.Migrate();
                }
				catch (Exception ex)
				{
					Console.WriteLine($"--> Could not run migrations: {ex.Message}");
				}
			}

			if (!context.Platforms.Any())
			{
                Console.WriteLine("Seeding data");

				context.Platforms.AddRange(
					new Models.Platform() {Name="Dot Net", Publisher="Microsoft", Cost="Free"},
                    new Models.Platform() { Name = "SQL SERVER", Publisher = "Microsoft", Cost = "Free" },
                    new Models.Platform() { Name = "Azure", Publisher = "Microsoft", Cost = "Free" }
                );

                context.SaveChanges();
            }
			else
			{
				Console.WriteLine("We alredy have data");
			}
		}
	}
}