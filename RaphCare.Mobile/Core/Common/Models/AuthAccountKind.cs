namespace RaphCare.Mobile.Core.Common.Models;

/// <summary>How the current session was established (email JWT vs Microsoft Entra).</summary>
public enum AuthAccountKind
{
    Unknown = 0,
    Email = 1,
    Entra = 2,
}
