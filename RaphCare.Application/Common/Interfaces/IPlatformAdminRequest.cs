namespace RaphCare.Application.Common.Interfaces;

/// <summary>
/// Marks a MediatR request gated by platform-admin HTTP policies (e.g. <c>RequirePlatformAdmin</c>).
/// Skips the default MediatR authentication check; ASP.NET authorization on the controller remains authoritative.
/// </summary>
public interface IPlatformAdminRequest;
