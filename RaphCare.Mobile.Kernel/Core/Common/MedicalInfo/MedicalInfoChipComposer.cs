namespace RaphCare.Mobile.Core.Common.MedicalInfo;

/// <summary>
/// Merges preset chips with free-text notes into the single string fields used by patient medical-info APIs.
/// </summary>
public static class MedicalInfoChipComposer
{
    /// <summary>
    /// Builds a stored value from selected chip ids and optional free-text notes.
    /// Exclusive chips replace other chip labels; notes are appended when present.
    /// </summary>
    public static string Compose(
        IReadOnlyList<MedicalInfoChipSpec> chips,
        IEnumerable<string> selectedIds,
        string? otherNotes)
    {
        ArgumentNullException.ThrowIfNull(chips);
        ArgumentNullException.ThrowIfNull(selectedIds);

        var selected = new HashSet<string>(selectedIds, StringComparer.OrdinalIgnoreCase);
        var exclusive = chips.FirstOrDefault(c => c.IsExclusive && selected.Contains(c.Id));
        var parts = new List<string>();

        if (!string.IsNullOrEmpty(exclusive.Id))
        {
            parts.Add(exclusive.Label);
        }
        else
        {
            foreach (var chip in chips)
            {
                if (chip.IsExclusive)
                    continue;
                if (selected.Contains(chip.Id))
                    parts.Add(chip.Label);
            }
        }

        var other = otherNotes?.Trim() ?? string.Empty;
        if (other.Length > 0
            && !parts.Exists(p => string.Equals(p, other, StringComparison.OrdinalIgnoreCase)))
        {
            parts.Add(other);
        }

        return string.Join(", ", parts);
    }

    /// <summary>
    /// Splits a stored value into matched chip ids and remaining free text.
    /// Longer labels win first so multi-word chips are not partially consumed.
    /// </summary>
    /// <param name="noneAliases">Optional phrases that select the exclusive chip (for example NKDA).</param>
    public static (IReadOnlySet<string> SelectedIds, string OtherNotes) Parse(
        string? stored,
        IReadOnlyList<MedicalInfoChipSpec> chips,
        IReadOnlyList<string>? noneAliases = null)
    {
        ArgumentNullException.ThrowIfNull(chips);

        var selected = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (string.IsNullOrWhiteSpace(stored))
            return (selected, string.Empty);

        var remaining = stored.Trim();
        foreach (var chip in chips.OrderByDescending(c => c.Label.Length))
        {
            if (string.IsNullOrWhiteSpace(chip.Label))
                continue;

            var index = remaining.IndexOf(chip.Label, StringComparison.OrdinalIgnoreCase);
            if (index < 0)
                continue;

            selected.Add(chip.Id);
            remaining = remaining.Remove(index, chip.Label.Length);
        }

        if (noneAliases is { Count: > 0 })
        {
            var exclusiveId = chips.FirstOrDefault(c => c.IsExclusive).Id;
            if (!string.IsNullOrEmpty(exclusiveId))
            {
                foreach (var alias in noneAliases)
                {
                    if (string.IsNullOrWhiteSpace(alias))
                        continue;

                    var index = remaining.IndexOf(alias, StringComparison.OrdinalIgnoreCase);
                    if (index < 0)
                        continue;

                    selected.Add(exclusiveId);
                    remaining = remaining.Remove(index, alias.Length);
                }
            }
        }

        if (selected.Any(id => chips.Any(c => c.IsExclusive && string.Equals(c.Id, id, StringComparison.OrdinalIgnoreCase))))
        {
            selected.RemoveWhere(id => chips.Any(c =>
                !c.IsExclusive && string.Equals(c.Id, id, StringComparison.OrdinalIgnoreCase)));
        }

        remaining = CleanupSeparators(remaining);
        return (selected, remaining);
    }

    private static string CleanupSeparators(string value)
    {
        var cleaned = value
            .Replace(" ,", ",", StringComparison.Ordinal)
            .Replace(",,", ",", StringComparison.Ordinal)
            .Trim(' ', ',', ';', '|');
        while (cleaned.Contains(", ,", StringComparison.Ordinal))
            cleaned = cleaned.Replace(", ,", ",", StringComparison.Ordinal);
        return cleaned.Trim(' ', ',', ';', '|');
    }
}
