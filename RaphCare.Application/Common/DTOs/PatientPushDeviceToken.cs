namespace RaphCare.Application.Common.DTOs;

/// <summary>Minimal push target row for patient push delivery.</summary>
public sealed record PatientPushDeviceToken(string DeviceToken, string Platform);
