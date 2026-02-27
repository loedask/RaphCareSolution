namespace RaphCare.Client.Models;

public class UpdatePatientRequest
{
    public Guid Id { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
}
