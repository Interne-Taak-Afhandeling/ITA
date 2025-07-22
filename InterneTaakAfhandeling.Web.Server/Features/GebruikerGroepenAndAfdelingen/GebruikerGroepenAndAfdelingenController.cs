using InterneTaakAfhandeling.Common.Exceptions;
using InterneTaakAfhandeling.Common.Helpers;
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
    public class GebruikerGroepenAndAfdelingenController(ITAUser user, IObjectApiClient objectApiClient) : Controller
    {
        [ProducesResponseType(typeof(MedewerkerResponse),
            StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        [HttpGet()]
        public async Task<IActionResult> GetGebruikerGroepenAndAfdelingen()
        {
            var result = await GetGebruikerGroepenAndAfdelingenAsync();
            return Ok(result);
        }
        private async Task<MedewerkerResponse?> GetGebruikerGroepenAndAfdelingenAsync()
        {
            var results = await objectApiClient.GetObjectsByIdentificatie(user.ObjectregisterMedewerkerId);
            if (results.Count > 1)
            {
                throw new ConflictException($"Meerdere medewerkers gevonden met dezelfde identificatie {SecureLogging.SanitizeForLogging(user.ObjectregisterMedewerkerId)}");
            }
            return
                new MedewerkerResponse
                {
                    Afdelingen = [.. (results.Single().Data.Afdelingen ?? []).Select(x => x.Afdelingnaam)],
                    Groepen = [.. (results.Single().Data.Groepen ?? []).Select(x => x.Groepsnaam)]
                };

        }

       }

    public class MedewerkerResponse
    {
        public List<string>? Groepen { get; init; }
        public List<string>? Afdelingen { get; init; }
    }


}

