using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace InterneTaakAfhandeling.Web.Server.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception");

                context.Response.ContentType = "application/json";

                context.Response.StatusCode = ex switch
                { 
                    ConflictException => StatusCodes.Status409Conflict,
                    _ => StatusCodes.Status500InternalServerError
                };

                var result = JsonSerializer.Serialize(new ITAException
                {
                    Message = ex.Message,
                    Code = (ex as ConflictException)?.Code                    
                });

                await context.Response.WriteAsync(result);
            }
        }
    }
    

    public class ConflictException(string message, string? code = null) : Exception(message)
    {
        public string? Code { get; } = code;
    }
    public class ITAException
    {
        public string? Code { get; set; }

        public string? Message { get; set; }
    }
}

