﻿namespace InterneTaakAfhandeling.Poller.Data.Entities;


public class InternetakenNotifierState
{
    public Guid Id { get; set; }
    public DateTimeOffset LastRunAt { get; set; }
    public Guid LastInternetakenId { get; set; }
    public DateTimeOffset? LastSuccessAt { get; set; }
    public int FailureCount { get; set; }
    public bool IsRunning { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public string? Remark { get; set; }
}
