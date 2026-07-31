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

        public static class Afdelingen
        {
            /// <summary>
            /// Afdeling key (as passed to GetOrCreateAfdelingActor) for an afdeling without a
            /// valid email address configured in the test objectenregister.
            /// Set to a valid value when test data is configured; tests using this are marked
            /// Inconclusive until then.
            /// </summary>
            public const string ZonderEmailKey = "";
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

        public static class Groepsmailbox
        {
            /// <summary>
            /// Identificatie of an afdeling with no groepsmailbox (blank/whitespace Email) and zero
            /// medewerkers linked to it, so the doorstuurscherm renders the blocking warning-alert
            /// (Task #542 scenario 4) instead of a required-but-unreachable medewerker field.
            /// Set to a valid value when test data is configured; tests using this are marked Inconclusive until then.
            /// </summary>
            public const string AfdelingZonderMailboxEnZonderMedewerkersKey = "";

            /// <summary>
            /// Identificatie of an afdeling/groep whose Email field contains only whitespace, to verify
            /// it is treated identically to a fully absent Email (Task #538 edge case). UI-indistinguishable
            /// from AfdelingZonderMailboxEnZonderMedewerkersKey, so a distinct record is only meaningful if
            /// verified via a direct API assertion rather than the rendered UI state.
            /// Set to a valid value when test data is configured; tests using this are marked Inconclusive until then.
            /// </summary>
            public const string AfdelingMetWhitespaceEmailKey = "";

            /// <summary>
            /// Search query (scoped medewerker combobox, within an afdeling zonder groepsmailbox) matching
            /// a medewerker without a valid email address, so neither the medewerker nor the afdeling
            /// resolves to a usable notification recipient (Task #543 dead-end scenario).
            /// Set to a valid value when test data is configured; tests using this are marked Inconclusive until then.
            /// </summary>
            public const string AfdelingZonderMailboxMetMedewerkerZonderEmailSearchQuery = "";
        }

    }
}