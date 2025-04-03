using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InterneTaakAfhandeling.Web.Server.Features.Environment
{
    [Route("api/environment")]
    [ApiController]
    [Authorize]
    public class EnvironmentController(ResourcesConfig resourcesConfig) : ControllerBase
    {
        private readonly ResourcesConfig _resourcesConfig = resourcesConfig;

        [HttpGet("resources")]
        public IActionResult GetResources()
        {
            var response = new
            {
                _resourcesConfig.Theme,
                _resourcesConfig.LogoUrl,
                _resourcesConfig.FaviconUrl,
                _resourcesConfig.TokensUrl
            };

            return Ok(response);
        }
    }
}
