namespace RaphCare.Ui.Components;

/// <summary>
/// Optional cascading labels for searchable select (Portal passes localized copy).
/// </summary>
public sealed class SearchableSelectLabels
{
    public string Search { get; init; } = "Search";

    public string NoMatchingOptions { get; init; } = "No matching options";
}
