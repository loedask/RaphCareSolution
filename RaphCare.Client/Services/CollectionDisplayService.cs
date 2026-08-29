using System.Net.Http.Json;
using RaphCare.Client.Contracts;
using RaphCare.Client.Contracts.Interfaces;
using RaphCare.Client.Models;
using RaphCare.Client.Services.Base;

namespace RaphCare.Client.Services;

public sealed class CollectionDisplayService(IHttpClientFactory httpClientFactory) : ICollectionDisplayService
{
    public async Task<Response<CollectionDisplayBoard>> GetBoardAsync(
        string token,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(token))
            return Response<CollectionDisplayBoard>.Failure("Missing waiting-screen link.");

        try
        {
            var http = httpClientFactory.CreateClient(ServiceRegistration.WebhookHttpClientName);
            using var response = await http
                .GetAsync($"api/display/collection/{Uri.EscapeDataString(token.Trim())}", cancellationToken)
                .ConfigureAwait(false);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return Response<CollectionDisplayBoard>.Failure("This waiting screen was not found.", 404);
            if (!response.IsSuccessStatusCode)
                return Response<CollectionDisplayBoard>.Failure("Could not load the waiting screen.", (int)response.StatusCode);

            var dto = await response.Content.ReadFromJsonAsync<BoardDto>(ApiJson.Options, cancellationToken)
                .ConfigureAwait(false);
            if (dto is null)
                return Response<CollectionDisplayBoard>.Failure("Could not load the waiting screen.");

            return Response<CollectionDisplayBoard>.Success(new CollectionDisplayBoard
            {
                ClinicName = dto.ClinicName ?? string.Empty,
                NowServing = MapTicket(dto.NowServing),
                Waiting = dto.Waiting?.Select(MapTicket).Where(t => t is not null).Cast<CollectionDisplayTicket>().ToList()
                    ?? []
            });
        }
        catch (HttpRequestException)
        {
            return Response<CollectionDisplayBoard>.Failure("We couldn't reach the server. Check your connection and try again.");
        }
    }

    private static CollectionDisplayTicket? MapTicket(TicketDto? dto)
    {
        if (dto is null || string.IsNullOrWhiteSpace(dto.PickupCode))
            return null;
        return new CollectionDisplayTicket
        {
            PickupCode = dto.PickupCode,
            Kind = dto.Kind ?? string.Empty
        };
    }

    private sealed class BoardDto
    {
        public string? ClinicName { get; set; }
        public TicketDto? NowServing { get; set; }
        public List<TicketDto>? Waiting { get; set; }
    }

    private sealed class TicketDto
    {
        public string? PickupCode { get; set; }
        public string? Kind { get; set; }
    }
}
