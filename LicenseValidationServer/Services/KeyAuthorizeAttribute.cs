using LicenseValidationServer.Models;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace LicenseValidationServer.Services;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public class KeyAuthorizeAttribute : Attribute, IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        // Access the service provider from the HttpContext
        var serviceProvider = context.HttpContext.RequestServices;

        // Resolve the AppDbContext from the service provider
        var dbContext = serviceProvider.GetService<AppDbContext>();

        if (dbContext == null)
        {
            throw new InvalidOperationException("AppDbContext not registered with the service provider.");
        }

        // Extract the token from the Authorization header
        string authorizationHeader = context.HttpContext.Request.Headers["Authorization"];

        if (string.IsNullOrWhiteSpace(authorizationHeader) || !authorizationHeader.StartsWith("Bearer "))
        {
            context.Result = new Microsoft.AspNetCore.Mvc.UnauthorizedResult();
            return;
        }

        string token = authorizationHeader.Substring("Bearer ".Length).Trim();

        // Asynchronously validate the token
        bool isTokenValid = ValidateTokenAsync(token, dbContext);

        if (!isTokenValid)
        {
            context.Result = new Microsoft.AspNetCore.Mvc.UnauthorizedResult();
        }

        context.Result = null;
    }

    private bool ValidateTokenAsync(string token, AppDbContext dbContext)
    {
        // Replace this with your actual logic to check the token against the database using dbContext
        var res = dbContext.Client.FirstOrDefaultAsync(x => x.authorization_token == token);
        return res != null;
    }
}