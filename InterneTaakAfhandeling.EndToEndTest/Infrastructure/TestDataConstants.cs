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
            public const string HistorieFirst = "8001321011";
            public const string HistorieSecond = "8001321012";
            public const string UnassignedForReassignment = "8001321013";
            public const string WithCurrentUserAssigned = "8001321014";
            public const string WithTeamAssignmentOnly = "8001321015";
            public const string WithNoAssignments = "8001321016";
            public const string WithCurrentUserAssignedViaObjectRegisterId = "8001321017";
            public const string WithTeamAssignmentNotCurrentUser = "8001321018";
        }

         public static class Zaken
        {
            public const string TestZaakIdentificatie = "ZAAK-2023-002";
        }

        public static class Doorsturen
        {
            // Search query that returns at least one medewerker in the test objectenregister
            public const string TestMedewerkerSearchQuery = "integratie";
            // Search query that returns a medewerker WITHOUT an email address in the test objectenregister
            // Must be populated with a real test medewerker before Scenario 4 can pass
            public const string TestMedewerkerNoEmailSearchQuery = "";
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