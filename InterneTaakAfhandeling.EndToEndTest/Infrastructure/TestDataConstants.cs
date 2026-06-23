namespace InterneTaakAfhandeling.EndToEndTest.Infrastructure
{
    /// <summary>
    /// Centralized test data constants used across E2E tests
    /// </summary>
    
     public static class TestDataConstants
    {
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

        public static class Doorsturen
        {
            /// <summary>
            /// Search query that matches at least one medewerker in the test objectenregister.
            /// Must return results when used with the /api/medewerkers?search= endpoint.
            /// </summary>
            public const string TestMedewerkerSearchQuery = "integratie";

            /// <summary>
            /// Search query that returns no medewerker results.
            /// </summary>
            public const string TestMedewerkerSearchQueryNoResults = "ZZZZNONEXISTENT";

            /// <summary>
            /// Search query matching a medewerker without an email address.
            /// Set to a valid value when test data is configured; tests using this are marked Inconclusive until then.
            /// </summary>
            public const string TestMedewerkerNoEmailSearchQuery = "";
        }

    }
}