namespace RaphCare.Mobile.Core.Common.MedicalInfo;

/// <summary>Stable chip id plus display label for self-reported medical lists.</summary>
public readonly record struct MedicalInfoChipSpec(string Id, string Label, bool IsExclusive);
