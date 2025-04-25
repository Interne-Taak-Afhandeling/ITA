using Duende.IdentityModel;
using System.Security.Claims; 
using Microsoft.AspNetCore.Mvc;

namespace InterneTaakAfhandeling.Web.Server.Features.Base
{
    [Route("api/[controller]")]
    [ApiController]
    public class BaseController : ControllerBase
    {
        protected string? UserEmail
        {
            get
            { 
                return User.FindFirstValue(JwtClaimTypes.Email);
            }
        }
    }
}
