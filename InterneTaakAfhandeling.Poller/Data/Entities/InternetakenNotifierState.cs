namespace InterneTaakAfhandeling.Poller.Data.Entities;


public class InternetakenNotifierState
{
    public Guid Id { get; set; }
    public Guid LastInternetakenId { get; set; }
    public DateTimeOffset LastInternetakenToegewezenOp { get; set; }
    public DateTimeOffset LastRunAt { get; set; }

    // Timestamp indicating when the last poller successfully processed all the internetaken.
    public DateTimeOffset? LastSuccessAt { get; set; }
    public int FailureCount { get; set; }
    public bool IsRunning { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public string? Remark { get; set; }
} 