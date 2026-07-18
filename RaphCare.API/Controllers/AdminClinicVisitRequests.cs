namespace RaphCare.API.Controllers;

public sealed class StartAdminClinicVisitRequest
{
    public string? Summary { get; set; }
}

public sealed class CompleteAdminClinicVisitRequest
{
    public string? Summary { get; set; }
}

public sealed class CreateAdminClinicVisitVitalRequest
{
    public string Type { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public string? Unit { get; set; }
    public DateTime? RecordedAt { get; set; }
}
