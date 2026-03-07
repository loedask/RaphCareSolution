namespace RaphCare.Client.ApiRoutes;

/// <summary>
/// API route constants organized by feature. Use only when extending Swagger (e.g. documentation, grouping).
/// Do not use in client services; services must communicate via IClient (Generated/ClientService.cs) only.
/// </summary>
public static class ApiRoutes
{
    public static class Patients
    {
        public const string Base = "api/patients";
        public static string ById(Guid id) => $"api/patients/{id}";
        public static string List(int pageNumber = 1, int pageSize = 20) => $"api/patients?pageNumber={pageNumber}&pageSize={pageSize}";
    }

    public static class Appointments
    {
        public const string Base = "api/appointments";
        public static string ById(Guid id) => $"api/appointments/{id}";
        public static string List(int pageNumber = 1, int pageSize = 20) => $"api/appointments?pageNumber={pageNumber}&pageSize={pageSize}";
    }

    public static class Devices
    {
        public const string Base = "api/devices";
        public static string ById(Guid id) => $"api/devices/{id}";
    }

    public static class Insurance
    {
        public const string ProfilesBase = "api/insurance";
        public static string ProfileById(Guid id) => $"api/insurance/{id}";
        public static string ProfilesList(int pageNumber = 1, int pageSize = 20) => $"api/insurance?pageNumber={pageNumber}&pageSize={pageSize}";
    }

    public static class Billing
    {
        public const string Base = "api/billing";
        public static string ById(Guid id) => $"api/billing/{id}";
    }

    public static class Clinical
    {
        public const string VisitsBase = "api/clinical";
        public static string VisitById(Guid id) => $"api/clinical/{id}";
        public static string VisitsList(int pageNumber = 1, int pageSize = 20, Guid? clinicId = null) =>
            clinicId.HasValue
                ? $"api/clinical?pageNumber={pageNumber}&pageSize={pageSize}&clinicId={clinicId}"
                : $"api/clinical?pageNumber={pageNumber}&pageSize={pageSize}";
    }

    public static class Telemedicine
    {
        public const string SessionsBase = "api/telemedicine";
        public static string SessionById(Guid id) => $"api/telemedicine/{id}";
    }

    public static class MentalHealth
    {
        public const string Assessments = "api/mentalhealth/assessments";
    }

    public static class AI
    {
        public const string Summary = "api/ai/summary";
    }

    public static class Reporting
    {
        public static string Dashboard(Guid? clinicId = null, DateTime? snapshotDate = null)
        {
            var q = new List<string>();
            if (clinicId.HasValue) q.Add($"clinicId={clinicId}");
            if (snapshotDate.HasValue) q.Add($"snapshotDate={snapshotDate:O}");
            return q.Count == 0 ? "api/reporting/dashboard" : "api/reporting/dashboard?" + string.Join("&", q);
        }
    }
}
