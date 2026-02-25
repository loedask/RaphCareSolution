using RaphCare.Domain.Common;

namespace RaphCare.Domain.Patients;

/// <summary>
/// Lifestyle and behavioural profile for a patient (1-1 with Patient).
/// </summary>
public class LifestyleProfile : BaseEntity
{
    public Guid PatientId { get; set; }
    public bool IsSmoker { get; set; }
    public bool UsesAlcohol { get; set; }
    public bool ExercisesRegularly { get; set; }
    public string? DietaryNotes { get; set; }

    public Patient Patient { get; set; } = null!;
}
