using RaphCare.Client.Contracts;
using RaphCare.Client.Contracts.Interfaces;
using RaphCare.Client.Models.Support;
using RaphCare.Client.Services.Base;

namespace RaphCare.Client.Services;

public sealed class PatientSupportService(HttpClient httpClient) : BaseHttpService(httpClient), IPatientSupportService
{
    public async Task<Response<PatientSupportContentViewModel>> GetContentAsync(CancellationToken cancellationToken = default)
    {
        var result = await GetAsync<ContentDto>("api/patient/support", cancellationToken).ConfigureAwait(false);
        if (!result.IsSuccess || result.Data is null)
            return Response<PatientSupportContentViewModel>.Failure(result.ErrorMessage ?? "Could not load support content.", result.StatusCode);

        return Response<PatientSupportContentViewModel>.Success(Map(result.Data));
    }

    public async Task<Response<Guid>> SubmitMessageAsync(
        SubmitPatientSupportMessageRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await PostAsync<TicketDto>("api/patient/support/messages", request, cancellationToken).ConfigureAwait(false);
        if (!result.IsSuccess || result.Data is null)
            return Response<Guid>.Failure(result.ErrorMessage ?? "Could not send message.", result.StatusCode);

        return Response<Guid>.Success(result.Data.TicketId);
    }

    private static PatientSupportContentViewModel Map(ContentDto d) =>
        new()
        {
            SupportEmail = d.SupportEmail ?? string.Empty,
            SupportPhoneE164 = d.SupportPhoneE164 ?? string.Empty,
            SupportPhoneDisplay = d.SupportPhoneDisplay ?? string.Empty,
            Faq = (d.Faq ?? Array.Empty<FaqDto>())
                .Select(f => new PatientSupportFaqItemViewModel
                {
                    Question = f.Question ?? string.Empty,
                    Answer = f.Answer ?? string.Empty
                })
                .ToArray()
        };

    private sealed class ContentDto
    {
        public string? SupportEmail { get; set; }
        public string? SupportPhoneE164 { get; set; }
        public string? SupportPhoneDisplay { get; set; }
        public FaqDto[]? Faq { get; set; }
    }

    private sealed class FaqDto
    {
        public string? Question { get; set; }
        public string? Answer { get; set; }
    }

    private sealed class TicketDto
    {
        public Guid TicketId { get; set; }
    }
}
