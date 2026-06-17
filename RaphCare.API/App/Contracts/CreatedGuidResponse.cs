namespace RaphCare.API.App.Contracts;

/// <summary>Response body for POST endpoints that return a single created resource id (OpenAPI).</summary>
public class CreatedGuidResponse
{
    public Guid Id { get; set; }
}
