using System.Text;
using InterneTaakAfhandeling.Poller.Services.Openklant.Models;

namespace InterneTaakAfhandeling.Poller.Services.Emailservices.Content;

public interface IEmailContentService
{
    string BuildInternetakenEmailContent(InternetakenItem request);
}

public class EmailContentService : IEmailContentService
{
    public string BuildInternetakenEmailContent(InternetakenItem request)
    {
        return new StringBuilder()
            .AppendLine($"Internetaken Number: {request.Nummer}")
            .AppendLine()
            .AppendLine("Requested Action:")
            .AppendLine(string.Join(Environment.NewLine, request.GevraagdeHandeling))
            .AppendLine()
            .AppendLine($"Explanation: {request.Toelichting ?? "No explanation provided"}")
            .AppendLine()
            .AppendLine($"Status: {request.Status}")
            .ToString();
    }
}
