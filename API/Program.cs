
using API.Data;
using API.Entities;
using API.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using static API.Extensions.ServiceExtensions;

namespace API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
            });

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.ConfigureOptions<ConfigureSwaggerOptions>();

            builder.Services.ConfigCORS(builder.Configuration);
            builder.Services.ConfigDatabase(builder.Configuration);
            builder.Services.ConfigureAPIVersioning(builder.Configuration);
            builder.Services.ConfigRegister();
            builder.Services.AddIdentityServices(builder.Configuration);
            builder.Services.AddSignalR();

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var context = services.GetRequiredService<ConnectifyDbContext>();
                    var userManager = services.GetRequiredService<UserManager<AppUser>>();
                    var roleManager = services.GetRequiredService<RoleManager<AppRole>>();
                    await context.Database.MigrateAsync();
                    await SeedData.SeedUsers(userManager, roleManager);
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "An error occurred during migration");
                }
            }

            app.ConfigureSwagger(app.Configuration);

            app.ConfigureExceptionHandler(app.Logger);

            app.UseCors();

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
