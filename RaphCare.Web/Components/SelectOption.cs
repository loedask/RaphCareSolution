namespace RaphCare.Web.Components;

/// <summary>A labeled value shown in <see cref="SearchableSelect{TValue}"/>.</summary>
public sealed record SelectOption<TValue>(TValue Value, string Label);
