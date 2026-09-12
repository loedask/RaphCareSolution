using System.Security.Cryptography;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Domain.Clinical;

namespace RaphCare.Application.Features.Organization;

internal static class ConsultQueueCode
{
    private const string Alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

    public static async Task<string> AllocateAsync(
        IRepository<ConsultTicket> tickets,
        CancellationToken cancellationToken)
    {
        for (var attempt = 0; attempt < 24; attempt++)
        {
            var code = Create();
            var existing = await tickets.SearchAsync(
                q => q.Where(t =>
                    t.QueueCode == code
                    && (t.Status == "Waiting" || t.Status == "Called")),
                1,
                1,
                cancellationToken: cancellationToken).ConfigureAwait(false);
            if (existing.TotalCount == 0)
                return code;
        }

        throw new InvalidOperationException("Could not allocate a consult queue code.");
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
