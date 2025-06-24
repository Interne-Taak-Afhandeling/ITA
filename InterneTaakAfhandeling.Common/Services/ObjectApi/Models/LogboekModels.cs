using System.ComponentModel.DataAnnotations;

namespace InterneTaakAfhandeling.Common.Services.ObjectApi.Models;

public class LogboekModel
{
    public required string Type { get; set; }
    public required LogboekRecord Record { get; set; }
}

public class LogboekRecord
{
    public required string TypeVersion { get; set; }
    public required DateOnly StartAt { get; set; }
    public required LogboekData Data { get; set; }
}

public class LogboekData
{
    public required ObjectIdentificator HeeftBetrekkingOp { get; set; }
    public required List<ActiviteitData> Activiteiten { get; set; }
}

public class ActiviteitData
{
    public required DateTimeOffset Datum { get; set; }
    public required string Type { get; set; }
    public required string Omschrijving { get; set; }
    public required List<ObjectIdentificator> HeeftBetrekkingOp { get; set; }
}

public class ObjectIdentificator
{
    public required string CodeRegister { get; set; }
    public required string CodeObjecttype { get; set; }
    public required string CodeSoortObjectId { get; set; }
    public required string ObjectId { get; set; }
}

public class LogboekOptions
{
    [Required] public required string Type { get; init; }

    [Required] public required string TypeVersion { get; init; }
}