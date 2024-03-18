using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using LicenseValidationServer.Database;
using LicenseValidationServer.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

var config = builder.Configuration;

builder.Services.AddDbContext<AppDbContext>(opt =>
{
    opt.UseNpgsql("Host=localhost;Port=5432;Database=licensedb;User Id=licensedb_admin;Password=;Encoding=UTF8");
    //opt.UseNpgsql(config.GetConnectionString("ConnectionString"));
});


builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();
app.UseRouting();
app.MapControllers(); // Maps urls with controllers




var datac = new DataCleanup();
await datac.DeleteAndReCreateDatabaseAsync(app);

app.Run("http://localhost:5000");
