using LicenseValidationServer.Data;
using LicenseValidationServer.Models;
using Microsoft.EntityFrameworkCore;

namespace LicenseValidationServer.Database
{
    /// WILL DELETE AND RECREATE THE DATABASE. DO NOT USE UNLESS YOU WANT TO DELETE ALL THE DATA! 
    public class DataCleanup
    {
        /// <summary>
        /// WILL DELETE AND RECREATE THE DATABASE. DO NOT USE UNLESS YOU WANT TO DELETE ALL THE DATA! 
        /// </summary>
        /// <param name="app">reference to WebApplication</param>
        public async Task DeleteAndReCreateDatabaseAsync(WebApplication? app)
        {
            if (app == null)
                return;

            var scope = app.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            app.Lifetime.ApplicationStarted.Register(async () =>
            {
                try
                {
                    context.Database.EnsureDeleted();
                    context.Database.EnsureCreated();
                    context.Database.Migrate();
                    DbInitializer.Initialize(context);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
                finally
                {
                    scope.Dispose();
                }
            });
        }
    }
}