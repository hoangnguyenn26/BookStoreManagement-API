
using Bookstore.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestPlatform.TestHost;

namespace Bookstore.Api.IntegrationTests
{
    public class BookstoreApiFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                // --- Ghi đè Cấu hình DbContext ---
                var dbContextDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));

                if (dbContextDescriptor != null)
                {
                    services.Remove(dbContextDescriptor);
                }
                services.RemoveAll<ApplicationDbContext>();
                var dbName = $"BookstoreTestDb_{Guid.NewGuid()}";
                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseInMemoryDatabase(dbName);
                });
                // --- Build ServiceProvider để lấy DbContext và Seed Data (nếu cần) ---
                var sp = services.BuildServiceProvider();

                using (var scope = sp.CreateScope())
                {
                    var scopedServices = scope.ServiceProvider;
                    var db = scopedServices.GetRequiredService<ApplicationDbContext>();
                    var logger = scopedServices.GetRequiredService<ILogger<BookstoreApiFactory>>();

                    try
                    {
                        db.Database.EnsureCreated();
                        SeedDataForTests(db);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "An error occurred seeding the testing database. Error: {Message}", ex.Message);
                    }
                }

            });
        }

        //Hàm seed dữ liệu mẫu cho môi trường test
        private static void SeedDataForTests(ApplicationDbContext context)
        {
            // Chỉ seed nếu chưa có dữ liệu
            if (!context.Roles.Any())
            {
                var adminRoleId = Guid.NewGuid();
                var userRoleId = Guid.NewGuid();
                var staffRoleId = Guid.NewGuid();
                var seedDate = DateTime.UtcNow;

                context.Roles.AddRange(
                    new Domain.Entities.Role { Id = adminRoleId, Name = "Admin", CreatedAtUtc = seedDate, UpdatedAtUtc = seedDate },
                    new Domain.Entities.Role { Id = userRoleId, Name = "User", CreatedAtUtc = seedDate, UpdatedAtUtc = seedDate },
                    new Domain.Entities.Role { Id = staffRoleId, Name = "Staff", CreatedAtUtc = seedDate, UpdatedAtUtc = seedDate }
                );
                context.SaveChanges();
            }
        }
    }
}