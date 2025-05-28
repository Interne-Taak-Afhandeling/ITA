using System.Security.Claims;
using Duende.IdentityModel;
using InterneTaakAfhandeling.Web.Server.Authentication;
using InterneTaakAfhandeling.Web.Server.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InterneTaakAfhandeling.Web.Server.Features.MyInterneTaken
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public class MyInterneTakenController(IUserService userService, ITAUser user) : Controller
    {
           private readonly IUserService _userService = userService ?? throw new ArgumentNullException(nameof(userService));


        [ProducesResponseType(typeof(List<InterneTaakAfhandeling.Common.Services.OpenKlantApi.Models.Internetaak>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        [HttpGet("internetaken")]
        public async Task<IActionResult> GetInternetaken()
        {
            var result = await _userService.GetInterneTakenByAssignedUser(user);

            Console.WriteLine($"Found {result.Count} internetaken for user {user.Email}");
            foreach (var item in result)
            {
                Console.WriteLine($"Internetaak: {item.Uuid}, Status: {item.Status}, Onderwerp: {item.AanleidinggevendKlantcontact?.Onderwerp}");
            }

            var json = System.Text.Json.JsonSerializer.Serialize(result, new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = true,
                ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve
            });

            return Ok(result);
        }
 
      
    }
}
