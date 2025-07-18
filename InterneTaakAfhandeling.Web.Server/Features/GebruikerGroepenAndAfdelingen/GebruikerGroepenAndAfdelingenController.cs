using InterneTaakAfhandeling.Common.Exceptions;
using InterneTaakAfhandeling.Common.Services.ObjectApi;
using InterneTaakAfhandeling.Web.Server.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InterneTaakAfhandeling.Web.Server.Features.GebruikerGroepenAndAfdelingen
{
    [Route("api/gebruiker-groepen-and-afdelingen")]
    [ApiController]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public class GebruikerGroepenAndAfdelingenController(ILogger<GebruikerGroepenAndAfdelingenController> logger, ITAUser user, IObjectApiClient objectApiClient) : Controller
    {
        private readonly ILogger<GebruikerGroepenAndAfdelingenController> _logger = logger;
        private readonly IObjectApiClient _objectApiClient = objectApiClient;

        [ProducesResponseType(typeof(MedewerkerResponse),
            StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        [HttpGet()]
        public async Task<IActionResult> GetGebruikerGroepenAndAfdelingen()
        {
            var result = await GetGebruikerGroepenAndAfdelingen(user);
            return Ok(result);
        }
        private async Task<MedewerkerResponse?> GetGebruikerGroepenAndAfdelingen(ITAUser user)
        {
            var results = await _objectApiClient.GetObjectsByIdentificatie(user.ObjectregisterMedewerkerId);
            if (results.Count > 1)
            {
                throw new ConflictException($"Meerdere medewerkers gevonden met dezelfde identificatie {MaskString(user.ObjectregisterMedewerkerId)}");
            }
            return
                new MedewerkerResponse()
                {
                    Afdelingen = [.. (results.First().Data.Afdelingen ?? []).Select(x => x.Afdelingnaam)],
                    Groepen = [.. (results.First().Data.Groepen ?? []).Select(x => x.Groepsnaam)]
                };

        }

        public static string MaskString(string input, int visibleCount = 4, char maskChar = '*') =>
       string.IsNullOrEmpty(input) || visibleCount >= input.Length ? input : new string(maskChar, input.Length - visibleCount) + input[^visibleCount..];
    }

    public class MedewerkerResponse
    {
        public List<string>? Groepen { get; set; }
        public List<string>? Afdelingen { get; set; }
    }


}

