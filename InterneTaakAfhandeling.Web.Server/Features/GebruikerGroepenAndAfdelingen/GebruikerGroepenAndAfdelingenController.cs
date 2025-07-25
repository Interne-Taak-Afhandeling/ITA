using InterneTaakAfhandeling.Common.Exceptions;
using InterneTaakAfhandeling.Common.Helpers;
using InterneTaakAfhandeling.Common.Services.ObjectApi;
using InterneTaakAfhandeling.Common.Services.ObjectApi.Models;
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
        [ProducesResponseType(typeof(List<string>),
            StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        [HttpGet()]
        public async Task<IActionResult> GetGebruikerGroepenAndAfdelingen()
        {
            var result = await GetGebruikerGroepenAndAfdelingenAsync();
            return Ok(result);
        }
        private async Task<List<string>> GetGebruikerGroepenAndAfdelingenAsync()
        {
            var results = await objectApiClient.GetObjectsByIdentificatie(user.ObjectregisterMedewerkerId);
            if (results.Count > 1)
            {
                throw new ConflictException($"Meerdere medewerkers gevonden met dezelfde identificatie {SecureLogging.SanitizeAndTruncate(user.ObjectregisterMedewerkerId, 5)}");
            }

            var result = results.Single().Data;
            //return new List<string>();
            return (result.Afdelingen ?? Enumerable.Empty<Afdeling>())
                .Select(a => a.Afdelingnaam)
                .Concat((result.Groepen ?? Enumerable.Empty<Groep>())
                    .Select(g => g.Groepsnaam))
                .ToList();
        }
    }



}

