using API.Errors;
using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Data.SqlClient;
using System.Net;

namespace API.Extensions
{
    public static class ApplicationExtensions
    {
        #region ExceptionHandler
        public static void ConfigureExceptionHandler(this IApplicationBuilder app, ILogger logger)
        {
            app.UseExceptionHandler(appError =>
            {
                appError.Run(async context =>
                {
                    var contextFeature = context.Features.Get<IExceptionHandlerFeature>();
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    context.Response.ContentType = "application/json";

                    if (contextFeature != null)
                    {
                        logger.LogError(contextFeature.Error, contextFeature.Error.Message);

                        var errorDetail = contextFeature.Error as APIException;
                        string message = contextFeature.Error.Message ?? "Internal Server Error.";

                        if (errorDetail != null)
                        {
                            context.Response.StatusCode = (int)errorDetail.StatusCode;
                            message = errorDetail.Message;
                        }

                        if (contextFeature.Error is SqlException updateErr)
                        {
                            message = updateErr?.InnerException?.Message ?? updateErr?.Message ?? "";
                        }

                        Exception errorRes = new APIException(message, statusCode: (HttpStatusCode)context.Response.StatusCode);
                        await context.Response.WriteAsync(errorRes.ToString());
                    }
                });
            });
        }
        #endregion

        #region Swagger
        public static void ConfigureSwagger(this WebApplication app, IConfiguration configuration)
        {
            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(options =>
                {
                    var apiVersionDescriptions = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
                    foreach (var description in apiVersionDescriptions.ApiVersionDescriptions.Reverse())
                    {
                        options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json",
                            description.GroupName.ToUpperInvariant());
                    }

                    options.DisplayRequestDuration();
                }).UseCors();
            }
        }
        #endregion
    }
}
