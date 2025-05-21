using System;
using System.Collections.Generic;
using InterneTaakAfhandeling.Common.Services.OpenKlantApi.Models;

namespace InterneTaakAfhandeling.Common.Services.OpenKlantApi.Mapper
{
    public static class InternetakenMapper
    {
        public static InternetakenUpdateRequest MapToUpdateRequest(this Internetaken internetaken)
        {
            if (internetaken == null)
                throw new ArgumentNullException(nameof(internetaken));

            return new InternetakenUpdateRequest
            {
                Nummer = internetaken.Nummer,
                GevraagdeHandeling = internetaken.GevraagdeHandeling,
                AanleidinggevendKlantcontact = new UuidObject { Uuid = Guid.Parse(internetaken.AanleidinggevendKlantcontact.Uuid) },
                ToegewezenAanActor = internetaken.ToegewezenAanActor != null
                    ? new UuidObject { Uuid = Guid.Parse(internetaken.ToegewezenAanActor.Uuid) }
                    : throw new InvalidOperationException("ToegewezenAanActor cannot be null"), 
                Toelichting = internetaken.Toelichting ?? string.Empty,
                Status = internetaken.Status
            };
        }
    }
}
