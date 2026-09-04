namespace RaphCare.Ui.Components;

/// <summary>Option for <see cref="SearchableSelect{TValue}"/>.</summary>
public sealed record SelectOption<TValue>(TValue Value, string Label);
