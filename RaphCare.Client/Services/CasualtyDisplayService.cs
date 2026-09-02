using System.Net.Http.Json;
using RaphCare.Client.Contracts;
using RaphCare.Client.Contracts.Interfaces;
using RaphCare.Client.Models;
using RaphCare.Client.Services.Base;

namespace RaphCare.Client.Services;

public sealed class CasualtyDisplayService(IHttpClientFactory httpClientFactory) : ICasualtyDisplayService
{
    public async Task<Response<CasualtyDisplayBoard>> GetBoardAsync(
        string token,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(token))
            return Response<CasualtyDisplayBoard>.Failure("Missing waiting-screen link.");

        try
        {
            var http = httpClientFactory.CreateClient(ServiceRegistration.WebhookHttpClientName);
            using var response = await http
                .GetAsync($"api/display/casualty/{Uri.EscapeDataString(token.Trim())}", cancellationToken)
                .ConfigureAwait(false);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return Response<CasualtyDisplayBoard>.Failure("This waiting screen was not found.", 404);
            if (!response.IsSuccessStatusCode)
                return Response<CasualtyDisplayBoard>.Failure("Could not load the waiting screen.", (int)response.StatusCode);

            var dto = await response.Content.ReadFromJsonAsync<BoardDto>(ApiJson.Options, cancellationToken)
                .ConfigureAwait(false);
            if (dto is null)
                return Response<CasualtyDisplayBoard>.Failure("Could not load the waiting screen.");

            return Response<CasualtyDisplayBoard>.Success(new CasualtyDisplayBoard
            {
                ClinicName = dto.ClinicName ?? string.Empty,
                NowServing = MapTicket(dto.NowServing),
                Waiting = dto.Waiting?.Select(MapTicket).Where(t => t is not null).Cast<CasualtyDisplayTicket>().ToList()
                    ?? []
            });
        }
        catch (HttpRequestException)
        {
            return Response<CasualtyDisplayBoard>.Failure("We couldn't reach the server. Check your connection and try again.");
        }
    }

    private static CasualtyDisplayTicket? MapTicket(TicketDto? dto)
    {
        if (dto is null || string.IsNullOrWhiteSpace(dto.QueueCode))
            return null;
        return new CasualtyDisplayTicket
        {
            QueueCode = dto.QueueCode,
            TriageLevel = dto.TriageLevel ?? string.Empty
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
        public string? QueueCode { get; set; }
        public string? TriageLevel { get; set; }
    }
}
