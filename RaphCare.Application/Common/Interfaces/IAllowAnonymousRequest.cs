namespace RaphCare.Application.Common.Interfaces;

/// <summary>
/// Marks a MediatR request that may run without an authenticated user (e.g. registration, OTP, public clinic list).
/// </summary>
public interface IAllowAnonymousRequest;
