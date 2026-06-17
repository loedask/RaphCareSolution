namespace RaphCare.Application.Common.DTOs;

/// <summary>
/// Base for application-layer data transfer objects that expose entity identity.
/// </summary>
public abstract class BaseDto
{
    public Guid Id { get; set; }
}

