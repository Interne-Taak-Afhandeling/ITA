using InterneTaakAfhandeling.Common.ApiClients.OpenKlantApi;
using InterneTaakAfhandeling.Common.ApiClients.OpenKlantApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterneTaakAfhandeling.Common.Services.ContactMomenten
{

    public interface IContactmomentenService
    {
        string? GetZaakOnderwerpObject(Klantcontact klantContact);

    }


    public class ContactmomentenService : IContactmomentenService
    {
        public string? GetZaakOnderwerpObject(Klantcontact klantContact)
        {
            return klantContact
                .Expand?
                .GingOverOnderwerpobjecten?
                .Where(x =>
                    x.Onderwerpobjectidentificator?.CodeObjecttype == KnownZgwIdentificators.OnderwerpobjectidentificatorCodeObjecttype &&
                    x.Onderwerpobjectidentificator?.CodeRegister == KnownZgwIdentificators.OnderwerpobjectidentificatorCodeRegister
                )
                .FirstOrDefault()?
                .Onderwerpobjectidentificator?
                .ObjectId;
        }


    }
}
