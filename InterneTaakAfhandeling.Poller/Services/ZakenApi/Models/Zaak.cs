using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterneTaakAfhandeling.Poller.Services.ZakenApi.Models
{
    public class Zaak
    { 
            public string Url { get; set; }
            public string Uuid { get; set; }
            public string Identificatie { get; set; }
            public string Bronorganisatie { get; set; }
            public string Omschrijving { get; set; }
            public string Toelichting { get; set; }
            public string Zaaktype { get; set; }
            public DateTimeOffset Registratiedatum { get; set; }
            public string VerantwoordelijkeOrganisatie { get; set; }
            public DateTimeOffset? Startdatum { get; set; }
            public DateTimeOffset? Einddatum { get; set; }
            public DateTimeOffset? EinddatumGepland { get; set; }
            public DateTimeOffset? UiterlijkeEinddatumAfdoening { get; set; }
            public DateTimeOffset? Publicatiedatum { get; set; }
            public string Communicatiekanaal { get; set; }
            public string CommunicatiekanaalNaam { get; set; }
            public List<object> ProductenOfDiensten { get; set; }
            public string Vertrouwelijkheidaanduiding { get; set; }
            public string Betalingsindicatie { get; set; }
            public string BetalingsindicatieWeergave { get; set; }
            public DateTimeOffset? LaatsteBetaaldatum { get; set; }
            public object Zaakgeometrie { get; set; }
            public object Verlenging { get; set; }
            public Opschorting Opschorting { get; set; }
            public string Selectielijstklasse { get; set; }
            public object Hoofdzaak { get; set; }
            public List<object> Deelzaken { get; set; }
            public List<object> RelevanteAndereZaken { get; set; }
            public List<object> Eigenschappen { get; set; }
            public List<string> Rollen { get; set; }
            public string Status { get; set; }
            public List<object> Zaakinformatieobjecten { get; set; }
            public List<object> Zaakobjecten { get; set; }
            public List<object> Kenmerken { get; set; }
            public string Archiefnominatie { get; set; }
            public string Archiefstatus { get; set; }
            public DateTimeOffset? Archiefactiedatum { get; set; }
            public object Resultaat { get; set; }
            public string OpdrachtgevendeOrganisatie { get; set; }
            public string Processobjectaard { get; set; }
            public DateTimeOffset? StartdatumBewaartermijn { get; set; }
            public Processobject Processobject { get; set; }
            public string ZaaksysteemId { get; set; }
        }

        public class Opschorting
        {
            public bool Indicatie { get; set; }
            public string Reden { get; set; }
        }

        public class Processobject
        {
            public string Datumkenmerk { get; set; }
            public string Identificatie { get; set; }
            public string Objecttype { get; set; }
            public string Registratie { get; set; }
        }

    
}
