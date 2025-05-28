 
using System.ComponentModel.DataAnnotations;
using InterneTaakAfhandeling.Web.Server.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace InterneTaakAfhandeling.Web.Server.Middleware
{
    public class ExceptionToProblemDetailsMapper : IExceptionHandler
    {
        private readonly ILogger<ExceptionToProblemDetailsMapper> _logger;
        private readonly ProblemDetailsFactory _problemDetailsFactory;

        public ExceptionToProblemDetailsMapper(
            ILogger<ExceptionToProblemDetailsMapper> logger,
            ProblemDetailsFactory problemDetailsFactory)
        {
            _logger = logger;
            _problemDetailsFactory = problemDetailsFactory;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            _logger.LogError(exception, "An unhandled exception has occurred");
             
            var statusCode = GetStatusCode(exception);
             
            var problemDetails = _problemDetailsFactory.CreateProblemDetails(
                httpContext,
                statusCode: statusCode,
                title: GetTitle(exception),
                detail: exception.Message,
                type: GetType(exception)
            ); 
            if (exception is ConflictException conflictEx && !string.IsNullOrEmpty(conflictEx.Code))
            {
                problemDetails.Extensions["conflictCode"] = conflictEx.Code;
            }
             
            if (exception is ValidationException validationEx)
            {
                var validationProblemDetails = new ValidationProblemDetails
                {
                    Status = statusCode,
                    Title = problemDetails.Title,
                    Detail = problemDetails.Detail,
                    Type = problemDetails.Type,
                    Instance = problemDetails.Instance
                };

                // Add validation errors
                validationProblemDetails.Errors.Add(
                    "validation",
                    new[] { validationEx.Message }
                );

                await httpContext.Response.WriteAsJsonAsync(
                    validationProblemDetails,
                    cancellationToken: cancellationToken
                );

                return true;
            }

            await httpContext.Response.WriteAsJsonAsync(
                problemDetails,
                cancellationToken: cancellationToken
            );

            return true;
        }

        private static int GetStatusCode(Exception exception) => exception switch
        {
            ConflictException => StatusCodes.Status409Conflict,
            ValidationException => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status500InternalServerError
        };

        private static string GetTitle(Exception exception) => exception switch
        {
            ConflictException => "Conflict Error",
            ValidationException => "Validation Error",
            _ => "An unexpected error occurred"
        };

        private static string GetType(Exception exception) => exception switch
        {
            ConflictException => "https://developer.mozilla.org/en-US/docs/Web/HTTP/Status/409",
            ValidationException => "https://developer.mozilla.org/en-US/docs/Web/HTTP/Status/400",
            _ => "https://developer.mozilla.org/en-US/docs/Web/HTTP/Status/500"
        };
    }

  
}