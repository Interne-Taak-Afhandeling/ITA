using InterneTaakAfhandeling.Common.Services.OpenKlantApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterneTaakAfhandeling.Common.Services.OpenklantApi
{
    //todo:
    //rename the folder that contains tjhe api clients from 'Services' to 'ApiClients'
    //move this file to a new folder 'Services'
    //(not doing it now. to many open pr's)


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
