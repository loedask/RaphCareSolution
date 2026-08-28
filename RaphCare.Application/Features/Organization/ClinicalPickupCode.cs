using System.Security.Cryptography;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Domain.Clinical;

namespace RaphCare.Application.Features.Organization;

internal static class ClinicalPickupCode
{
    private const string Alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

    public static async Task<string> AllocateAsync(
        IRepository<Prescription> prescriptions,
        IRepository<LabRequest> labRequests,
        CancellationToken cancellationToken)
    {
        for (var attempt = 0; attempt < 24; attempt++)
        {
            var code = Create();
            var rx = await prescriptions.SearchAsync(
                q => q.Where(p => p.PickupCode == code),
                1,
                1,
                cancellationToken: cancellationToken).ConfigureAwait(false);
            if (rx.TotalCount > 0)
                continue;

            var labs = await labRequests.SearchAsync(
                q => q.Where(l => l.PickupCode == code),
                1,
                1,
                cancellationToken: cancellationToken).ConfigureAwait(false);
            if (labs.TotalCount > 0)
                continue;

            return code;
        }

        throw new InvalidOperationException("Could not allocate a pickup code.");
    }

    private static string Create()
    {
        Span<char> buffer = stackalloc char[6];
        Span<byte> bytes = stackalloc byte[6];
        RandomNumberGenerator.Fill(bytes);
        for (var i = 0; i < buffer.Length; i++)
            buffer[i] = Alphabet[bytes[i] % Alphabet.Length];
        return new string(buffer);
    }
}
