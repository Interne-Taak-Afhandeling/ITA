namespace InterneTaakAfhandeling.Web.Server.Exceptions
{
   //public class ConflictException : Exception
   // {
   //     public string? Code { get; set; }

   //     public ConflictException(string message) : base(message) { }

   //     public ConflictException(string message, string code) : base(message)
   //     {
   //         Code = code;
   //     }
   // }


    public class ITAException
    {
        public string? Code { get; set; }

        public string? Message { get; set; }
    }
}
