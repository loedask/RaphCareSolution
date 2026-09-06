using RaphCare.Client.Contracts;
using RaphCare.Client.Contracts.Interfaces;
using RaphCare.Client.Models.Api;
using RaphCare.Client.Models.Appointments;
using RaphCare.Client.Models.MentalHealth;
using RaphCare.Client.Services.Base;

namespace RaphCare.Client.Services;

public sealed class PatientMentalHealthService(HttpClient httpClient) : BaseHttpService(httpClient), IPatientMentalHealthService
{
    public async Task<Response<PatientMentalHealthContentViewModel>> GetContentAsync(CancellationToken cancellationToken = default)
    {
        var result = await GetAsync<MentalHealthContentDto>("api/patient/mental-health/content", cancellationToken).ConfigureAwait(false);
        if (!result.IsSuccess || result.Data is null)
            return Response<PatientMentalHealthContentViewModel>.Failure(result.ErrorMessage ?? "Could not load content.", result.StatusCode);
        return Response<PatientMentalHealthContentViewModel>.Success(Map(result.Data));
    }

    public async Task<Response<IReadOnlyList<PatientMoodCheckInViewModel>>> GetMyMoodCheckInsAsync(
        int pageSize = 14,
        CancellationToken cancellationToken = default)
    {
        var size = pageSize is < 1 or > 50 ? 14 : pageSize;
        var result = await GetAsync<List<MoodCheckInDto>>(
                $"api/patient/mental-health/mood-checkins?pageSize={size}",
                cancellationToken)
            .ConfigureAwait(false);
        if (!result.IsSuccess)
            return Response<IReadOnlyList<PatientMoodCheckInViewModel>>.Failure(
                result.ErrorMessage ?? "Could not load mood check-ins.",
                result.StatusCode);

        IReadOnlyList<PatientMoodCheckInViewModel> items = (result.Data ?? [])
            .Select(d => new PatientMoodCheckInViewModel
            {
                Id = d.Id,
                LoggedAt = d.LoggedAt,
                MoodScore = d.MoodScore
            })
            .ToList();
        return Response<IReadOnlyList<PatientMoodCheckInViewModel>>.Success(items);
    }

    public async Task<Response<Guid>> LogMoodCheckInAsync(int moodScore, string? notes, CancellationToken cancellationToken = default)
    {
        var result = await PostAsync<CreatedGuidApiResponse>(
                "api/patient/mental-health/mood-checkin",
                new { moodScore, notes },
                cancellationToken)
            .ConfigureAwait(false);

        if (!result.IsSuccess || result.Data is null)
            return Response<Guid>.Failure(result.ErrorMessage ?? "Check-in failed.", result.StatusCode);
        return Response<Guid>.Success(result.Data.Id);
    }

    public async Task<Response<MentalHealthInstrument>> GetInstrumentAsync(
        string assessmentType = "PHQ-9",
        CancellationToken cancellationToken = default)
    {
        var encoded = Uri.EscapeDataString(assessmentType);
        var result = await GetAsync<InstrumentDto>(
                $"api/patient/mental-health/instruments/{encoded}",
                cancellationToken)
            .ConfigureAwait(false);
        if (!result.IsSuccess || result.Data is null)
            return Response<MentalHealthInstrument>.Failure(
                result.ErrorMessage ?? "Instrument not found.",
                result.StatusCode);
        return Response<MentalHealthInstrument>.Success(MapInstrument(result.Data));
    }

    public async Task<Response<PagedApiResult<MentalHealthAssessmentListItem>>> GetAssessmentsAsync(
        Guid? clinicId = null,
        int pageNumber = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = $"api/patient/mental-health/assessments?pageNumber={pageNumber}&pageSize={pageSize}";
        if (clinicId is Guid cid)
            query += $"&clinicId={cid:D}";

        var result = await GetAsync<PagedApiResult<AssessmentListDto>>(query, cancellationToken)
            .ConfigureAwait(false);
        if (!result.IsSuccess || result.Data is null)
            return Response<PagedApiResult<MentalHealthAssessmentListItem>>.Failure(
                result.ErrorMessage ?? "Could not load assessments.",
                result.StatusCode);

        return Response<PagedApiResult<MentalHealthAssessmentListItem>>.Success(
            new PagedApiResult<MentalHealthAssessmentListItem>
            {
                Items = result.Data.Items?.Select(MapListItem).ToList() ?? [],
                TotalCount = result.Data.TotalCount,
                PageNumber = result.Data.PageNumber,
                PageSize = result.Data.PageSize
            });
    }

    public async Task<Response<MentalHealthAssessmentDetail>> SubmitAssessmentAsync(
        SubmitPatientMentalHealthAssessmentRequest request,
        CancellationToken cancellationToken = default)
    {
        var body = new
        {
            clinicId = request.ClinicId,
            assessmentType = request.AssessmentType,
            answers = request.Answers.Select(a => new { order = a.Order, numericScore = a.NumericScore })
        };

        var result = await PostAsync<AssessmentDetailDto>(
                "api/patient/mental-health/assessments",
                body,
                cancellationToken)
            .ConfigureAwait(false);
        if (!result.IsSuccess || result.Data is null)
            return Response<MentalHealthAssessmentDetail>.Failure(
                result.ErrorMessage ?? "Could not save the assessment.",
                result.StatusCode);
        return Response<MentalHealthAssessmentDetail>.Success(MapDetail(result.Data));
    }

    private static PatientMentalHealthContentViewModel Map(MentalHealthContentDto d) =>
        new()
        {
            InsightTitle = d.InsightTitle ?? string.Empty,
            InsightBody = d.InsightBody ?? string.Empty,
            MedicalDisclaimer = d.MedicalDisclaimer ?? string.Empty
        };

    private static MentalHealthAssessmentListItem MapListItem(AssessmentListDto d) => new()
    {
        Id = d.Id,
        ClinicId = d.ClinicId,
        PatientId = d.PatientId,
        AssessmentType = d.AssessmentType ?? string.Empty,
        AssessedAt = d.AssessedAt,
        TotalScore = d.TotalScore,
        SeverityLevel = d.SeverityLevel ?? string.Empty,
        Summary = d.Summary ?? string.Empty
    };

    private static MentalHealthAssessmentDetail MapDetail(AssessmentDetailDto d) => new()
    {
        Id = d.Id,
        ClinicId = d.ClinicId,
        PatientId = d.PatientId,
        AssessmentType = d.AssessmentType ?? string.Empty,
        AssessedAt = d.AssessedAt,
        TotalScore = d.TotalScore,
        SeverityLevel = d.SeverityLevel ?? string.Empty,
        Summary = d.Summary ?? string.Empty,
        Items = d.Items?.Select(i => new MentalHealthAssessmentItem
        {
            Order = i.Order,
            QuestionText = i.QuestionText ?? string.Empty,
            NumericScore = i.NumericScore,
            ResponseValue = i.ResponseValue ?? string.Empty
        }).ToList() ?? []
    };

    private static MentalHealthInstrument MapInstrument(InstrumentDto d) => new()
    {
        AssessmentType = d.AssessmentType ?? string.Empty,
        Title = d.Title ?? string.Empty,
        Instructions = d.Instructions ?? string.Empty,
        Questions = d.Questions?.Select(q => new MentalHealthInstrumentQuestion
        {
            Order = q.Order,
            QuestionText = q.QuestionText ?? string.Empty
        }).ToList() ?? [],
        Options = d.Options?.Select(o => new MentalHealthInstrumentOption
        {
            NumericScore = o.NumericScore,
            Label = o.Label ?? string.Empty
        }).ToList() ?? []
    };

    private sealed class MentalHealthContentDto
    {
        public string? InsightTitle { get; set; }
        public string? InsightBody { get; set; }
        public string? MedicalDisclaimer { get; set; }
    }

    private sealed class MoodCheckInDto
    {
        public Guid Id { get; set; }
        public DateTime LoggedAt { get; set; }
        public int MoodScore { get; set; }
    }

    private class AssessmentListDto
    {
        public Guid Id { get; set; }
        public Guid ClinicId { get; set; }
        public Guid PatientId { get; set; }
        public string? AssessmentType { get; set; }
        public DateTime AssessedAt { get; set; }
        public decimal? TotalScore { get; set; }
        public string? SeverityLevel { get; set; }
        public string? Summary { get; set; }
    }

    private sealed class AssessmentDetailDto : AssessmentListDto
    {
        public List<AssessmentItemDto>? Items { get; set; }
    }

    private sealed class AssessmentItemDto
    {
        public int Order { get; set; }
        public string? QuestionText { get; set; }
        public int? NumericScore { get; set; }
        public string? ResponseValue { get; set; }
    }

    private sealed class InstrumentDto
    {
        public string? AssessmentType { get; set; }
        public string? Title { get; set; }
        public string? Instructions { get; set; }
        public List<InstrumentQuestionDto>? Questions { get; set; }
        public List<InstrumentOptionDto>? Options { get; set; }
    }

    private sealed class InstrumentQuestionDto
    {
        public int Order { get; set; }
        public string? QuestionText { get; set; }
    }

    private sealed class InstrumentOptionDto
    {
        public int NumericScore { get; set; }
        public string? Label { get; set; }
    }
}
