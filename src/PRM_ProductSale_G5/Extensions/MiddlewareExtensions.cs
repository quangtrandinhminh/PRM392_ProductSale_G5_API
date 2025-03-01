namespace PRM_ProductSale_G5.Extensions;

using PRM_ProductSale_G5.Middlewares;

public static class MiddlewareExtensions
{
    public static WebApplication UseApplicationMiddleware(this WebApplication app)
    {
        // Use custom error handling middleware
        app.UseMiddleware<ErrorHandlerMiddleware>();

        // Use CORS
        app.UseCors("_myAllowSpecificOrigins");

        // HTTPS redirection
        app.UseHttpsRedirection();

        // Authentication and Authorization
        app.UseAuthentication();
        app.UseAuthorization();

        return app;
    }
}