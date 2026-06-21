using System.Reflection;

namespace RaphCare.Domain.Common;

/// <summary>
/// Smart enum (enumeration) base class for type-safe, extensible domain values.
/// Use for VisitStatus, InsurancePlanType, DeviceType, PermissionCodes, etc.
/// </summary>
public abstract class Enumeration(int id, string name) : IEquatable<Enumeration>, IComparable<Enumeration>
{
    public int Id { get; } = id;
    public string Name { get; } = name ?? throw new ArgumentNullException(nameof(name));

    public override string ToString() => Name;

    public override bool Equals(object? obj) => Equals(obj as Enumeration);

    public bool Equals(Enumeration? other) => other is not null && GetType() == other.GetType() && Id == other.Id;

    public override int GetHashCode() => HashCode.Combine(GetType(), Id);

    public int CompareTo(Enumeration? other) => other is null ? 1 : Id.CompareTo(other.Id);

    public static bool operator ==(Enumeration? left, Enumeration? right)
    {
        if (left is null && right is null)
            return true;
        if (left is null || right is null)
            return false;
        return left.Equals(right);
    }

    public static bool operator !=(Enumeration? left, Enumeration? right) => !(left == right);

    public static bool operator <(Enumeration? left, Enumeration? right) =>
        left is null ? right is not null : right is not null && left.CompareTo(right) < 0;

    public static bool operator <=(Enumeration? left, Enumeration? right) =>
        left is null || right is null ? left is null && right is null : left.CompareTo(right) <= 0;

    public static bool operator >(Enumeration? left, Enumeration? right) =>
        left is not null && right is not null && left.CompareTo(right) > 0;

    public static bool operator >=(Enumeration? left, Enumeration? right) =>
        left is not null && (right is null || left.CompareTo(right) >= 0);

    /// <summary>
    /// Returns all defined values for the enumeration type T.
    /// </summary>
    public static IReadOnlyList<T> GetAll<T>() where T : Enumeration
    {
        var type = typeof(T);
        var fields = type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(f => f.FieldType == type)
            .ToArray();

        return fields
            .Select(f => (T)f.GetValue(null)!)
            .OrderBy(e => e.Id)
            .ToList();
    }

    /// <summary>
    /// Gets the enumeration value by its integer id.
    /// </summary>
    /// <exception cref="InvalidOperationException">When no value matches the given id.</exception>
    public static T FromValue<T>(int value) where T : Enumeration
    {
        var match = GetAll<T>().FirstOrDefault(e => e.Id == value);
        return match ?? throw new InvalidOperationException($"'{value}' is not a valid {typeof(T).Name} value.");
    }

    /// <summary>
    /// Gets the enumeration value by its name (case-insensitive).
    /// </summary>
    /// <exception cref="InvalidOperationException">When no value matches the given name.</exception>
    public static T FromName<T>(string name) where T : Enumeration
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentNullException(nameof(name));

        var match = GetAll<T>().FirstOrDefault(e => string.Equals(e.Name, name.Trim(), StringComparison.OrdinalIgnoreCase));
        return match ?? throw new InvalidOperationException($"'{name}' is not a valid {typeof(T).Name} name.");
    }

    /// <summary>
    /// Tries to get the enumeration value by its integer id.
    /// </summary>
    public static bool TryFromValue<T>(int value, out T? result) where T : Enumeration
    {
        result = GetAll<T>().FirstOrDefault(e => e.Id == value);
        return result is not null;
    }

    /// <summary>
    /// Tries to get the enumeration value by its name (case-insensitive).
    /// </summary>
    public static bool TryFromName<T>(string name, out T? result) where T : Enumeration
    {
        result = null;
        if (string.IsNullOrWhiteSpace(name))
            return false;

        result = GetAll<T>().FirstOrDefault(e => string.Equals(e.Name, name.Trim(), StringComparison.OrdinalIgnoreCase));
        return result is not null;
    }
}
