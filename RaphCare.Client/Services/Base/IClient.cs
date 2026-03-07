namespace RaphCare.Client.Services.Base;

/// <summary>
/// Extended IClient: exposes HttpClient for BaseHttpService. Patient operations use the generated methods (PatientsGETAsync, PatientsGET2Async, PatientsPOSTAsync, PatientsPUTAsync).
/// </summary>
public partial interface IClient
{
    HttpClient HttpClient { get; }
}
