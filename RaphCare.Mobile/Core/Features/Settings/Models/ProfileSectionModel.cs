using System.Collections.ObjectModel;

namespace RaphCare.Mobile.Core.Features.Settings.Models;

/// <summary>Section title + rows for the profile hub.</summary>
public sealed class ProfileSectionModel
{
    public required string Title { get; init; }
    public required ObservableCollection<ProfileMenuRowModel> Items { get; init; }
}
