namespace RaphCare.Mobile.Core.Common.Collections;

/// <summary>
/// Rules for refreshing collections bound to MAUI <c>Picker</c> (and similar) controls.
/// Clearing ItemsSource while SelectedItem still points at a removed row can close the Android process.
/// </summary>
public static class MauiPickerCollectionRules
{
    /// <summary>
    /// True when <paramref name="selected"/> is null or is not present in <paramref name="items"/>.
    /// Callers must clear selection before <c>Clear()</c> when this returns false.
    /// </summary>
    public static bool IsClearSafe<T>(IEnumerable<T> items, T? selected)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(items);
        if (selected is null)
            return true;

        foreach (var item in items)
        {
            if (ReferenceEquals(item, selected))
                return false;
        }

        return true;
    }

    /// <summary>
    /// Picks <paramref name="preferredId"/> when it appears in <paramref name="ids"/>; otherwise the first id.
    /// </summary>
    public static Guid? PickPreferredOrFirst(IReadOnlyList<Guid> ids, Guid? preferredId)
    {
        ArgumentNullException.ThrowIfNull(ids);
        if (ids.Count == 0)
            return null;

        if (preferredId is { } preferred && preferred != Guid.Empty)
        {
            for (var i = 0; i < ids.Count; i++)
            {
                if (ids[i] == preferred)
                    return preferred;
            }
        }

        return ids[0];
    }
}
