using System.Security.Cryptography;

namespace RaphCare.Domain.Organization;

/// <summary>Random token for the public collection waiting-room URL.</summary>
public static class ClinicCollectionDisplayToken
{
    public const int Length = 12;
    private const string Alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

    public static string Generate()
    {
        Span<char> buffer = stackalloc char[Length];
        var bytes = RandomNumberGenerator.GetBytes(Length);
        for (var i = 0; i < Length; i++)
            buffer[i] = Alphabet[bytes[i] % Alphabet.Length];
        return new string(buffer);
    }
}
