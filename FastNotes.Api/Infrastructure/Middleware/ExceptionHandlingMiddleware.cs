using FastNotes.Api.Infrastructure.Errors;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace FastNotes.Api.Infrastructure.Middleware;

public static class ExceptionHandlingMiddleware
{
    public static void UseGlobalExceptionHandling(this IApplicationBuilder app)
    {
        app.UseExceptionHandler(appBuilder =>
        {
            appBuilder.Run(async context =>
            {
                var logger = context.RequestServices
                    .GetRequiredService<ILoggerFactory>()
                    .CreateLogger("GlobalExceptionHandler");

                var feature = context.Features.Get<IExceptionHandlerFeature>();
                var exception = feature?.Error;

                if (exception == null)
                    return;

                ProblemDetails problem;

                switch (exception)
                {
                    case ApiException apiEx:
                        problem = new ProblemDetails
                        {
                            Status = apiEx.StatusCode,
                            Title = apiEx.Title,
                            Detail = apiEx.Message
                        };

                        if (apiEx is ValidationException ve)
                        {
                            problem.Extensions["errors"] = ve.Errors;
                        }
                        break;

                    default:
                        logger.LogError(exception, "Unhandled exception");

                        problem = new ProblemDetails
                        {
                            Status = StatusCodes.Status500InternalServerError,
                            Title = "Internal server error",
                            Detail = "An unexpected error occurred."
                        };
                        break;
                }

                context.Response.StatusCode = problem.Status!.Value;
                context.Response.ContentType = "application/problem+json";
                await context.Response.WriteAsJsonAsync(problem);
            });
        });
    }
}
