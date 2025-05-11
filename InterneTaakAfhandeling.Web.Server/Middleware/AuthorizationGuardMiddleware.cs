using InterneTaakAfhandeling.Web.Server.Authentication;
using Microsoft.AspNetCore.Http;

namespace InterneTaakAfhandeling.Web.Server.Middleware
{
    public class AuthorizationGuardMiddleware(RequestDelegate next)
    {
        private readonly RequestDelegate _next = next;
        private const string ForbiddenMessage = "De gebruiker beschikt niet over de vereiste claims om deze actie uit te voeren.";

        public async Task InvokeAsync(HttpContext context, ITAUser user)
        {
            if (context.User.Identity?.IsAuthenticated == true &&
                (string.IsNullOrWhiteSpace(user.Email) || string.IsNullOrWhiteSpace(user.Id)))
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsync(ForbiddenMessage);
                return;
            }

            await _next(context);
        }
    }
}