namespace RaphCare.Mobile.Core.Common.Configuration;

/// <summary>Optional defaults for the book-appointment form (clinic/provider Guids from your environment).</summary>
public class AppointmentsMobileOptions
{
    public const string SectionName = "Appointments";

    /// <summary>When set, pre-fills clinic id on the book screen.</summary>
    public string? DefaultClinicId { get; set; }

    /// <summary>When set, pre-fills provider id on the book screen.</summary>
    public string? DefaultProviderId { get; set; }
}
