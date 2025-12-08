namespace InterneTaakAfhandeling.EndToEndTest.Infrastructure
{
    /// <summary>
    /// Centralized test data constants used across E2E tests
    /// </summary>
    
     public static class TestDataConstants
    {
        public static class ContactverzoekNummers
        {
            public const string WithZaak = "8001321008";
            public const string WithoutZaak = "8001321009";
            public const string WithPartij = "8001321010";
        }

         public static class Zaken
        {
            public const string TestZaakIdentificatie = "ZAAK-2023-002";
        }

        public static class Partijen
        {
            public const string TestBsn = "999992223";
            
            public static class Contactnaam
            {
                public const string Voorletters = "C";
                public const string Voornaam = "Christina";
                public const string Achternaam = "Burck";
                public const string VoorvoegselAchternaam = "du";
            }
        }

    }
}