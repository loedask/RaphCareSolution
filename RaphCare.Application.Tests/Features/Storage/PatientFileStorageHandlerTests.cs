using MediatR;
using RaphCare.Application.Common.DTOs;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Onboarding.Commands.CreatePatientFromVoice;
using RaphCare.Application.Features.Organization.DTOs;
using RaphCare.Application.Features.Organization.Queries.GetAdminClinicPatientPhoto;
using RaphCare.Application.Features.PatientProfile;
using RaphCare.Application.Features.PatientProfile.Commands.UploadMyPatientProfilePhoto;
using RaphCare.Domain.Patients;
using Xunit;

namespace RaphCare.Application.Tests.Features.Storage;

public sealed class PatientFileStorageHandlerTests
{
    [Fact]
    public async Task VoiceOnboardingStoresRecordingKeyInsteadOfPlaceholderAfterTranscriptionConsumesStream()
    {
        var patientId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        var patient = new Patient { FirstName = "Jane", LastName = "Doe" };
        SetId(patient, patientId);

        var recordings = new FakeRepository<VoiceRecording>();
        var voiceStorage = new CapturingVoiceStorage();
        var handler = new CreatePatientFromVoiceHandler(
            new ConsumingSpeechToText(),
            new StubMediator(patientId),
            recordings,
            new FakeRepository<Patient>([patient]),
            new FakeUnitOfWork(),
            new NoOpIdentityTimeline(),
            voiceStorage);

        await using var audio = new MemoryStream("voice-bytes-that-must-survive-stt"u8.ToArray());
        await handler.Handle(
            new CreatePatientFromVoiceCommand
            {
                AudioStream = audio,
                Language = "en-ZA",
                PhoneNumber = "+27831234567",
                ClinicId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                ContentType = "audio/wav"
            },
            CancellationToken.None);

        var recording = Assert.Single(recordings.Items);
        Assert.NotEqual("placeholder", recording.StorageUrl);
        Assert.Equal(voiceStorage.LastKey, recording.StorageUrl);
        Assert.True(voiceStorage.LastLength > 0);
        Assert.Equal(patientId, recording.PatientId);
    }

    [Fact]
    public async Task ProfilePhotoUploadPersistsRelativePathFromStorage()
    {
        var patientId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");
        var patient = new Patient { FirstName = "Ada", LastName = "K." };
        SetId(patient, patientId);
        var photoStorage = new InMemoryPhotoStorage();
        var handler = new UploadMyPatientProfilePhotoHandler(
            new FakeRepository<Patient>([patient]),
            new FakeRepository<PatientProfile>(),
            photoStorage,
            new FakeUnitOfWork(),
            new FakeCurrentUser(Guid.NewGuid(), patientId));

        await using var photo = new MemoryStream([0xFF, 0xD8, 0xFF]);
        var url = await handler.Handle(
            new UploadMyPatientProfilePhotoCommand { Content = photo, ContentType = "image/jpeg" },
            CancellationToken.None);

        Assert.Equal(PatientProfilePhotoUrls.RelativePhotoPath, url);
        Assert.Equal($"{patientId:N}.jpg", photoStorage.SavedPath);
    }

    [Fact]
    public async Task StaffPatientPhotoReturnsNullWhenCallerHasNoClinicMembership()
    {
        var clinicId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var patientId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var photos = new InMemoryPhotoStorage();
        photos.Seed(patientId, [1, 2, 3], "image/jpeg");

        var handler = new GetAdminClinicPatientPhotoHandler(
            new FakeCurrentUser(Guid.NewGuid(), null),
            new FakeMembership(false),
            new FakePatientQuery(new AdminClinicPatientDetailDto { PatientId = patientId }),
            photos);

        var result = await handler.Handle(
            new GetAdminClinicPatientPhotoQuery { ClinicId = clinicId, PatientId = patientId },
            CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task StaffPatientPhotoReturnsNullWhenPatientIsNotLinkedToClinic()
    {
        var clinicId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var patientId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var photos = new InMemoryPhotoStorage();
        photos.Seed(patientId, [1, 2, 3], "image/jpeg");

        var handler = new GetAdminClinicPatientPhotoHandler(
            new FakeCurrentUser(Guid.NewGuid(), null),
            new FakeMembership(true),
            new FakePatientQuery(null),
            photos);

        var result = await handler.Handle(
            new GetAdminClinicPatientPhotoQuery { ClinicId = clinicId, PatientId = patientId },
            CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task StaffPatientPhotoReturnsBytesWhenMembershipAndClinicLinkAllowIt()
    {
        var clinicId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var patientId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var photos = new InMemoryPhotoStorage();
        photos.Seed(patientId, [9, 8, 7], "image/png");

        var handler = new GetAdminClinicPatientPhotoHandler(
            new FakeCurrentUser(Guid.NewGuid(), null),
            new FakeMembership(true),
            new FakePatientQuery(new AdminClinicPatientDetailDto { PatientId = patientId, FirstName = "Ada" }),
            photos);

        var result = await handler.Handle(
            new GetAdminClinicPatientPhotoQuery { ClinicId = clinicId, PatientId = patientId },
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("image/png", result!.ContentType);
        using var reader = new MemoryStream();
        await result.Content.CopyToAsync(reader);
        Assert.Equal(new byte[] { 9, 8, 7 }, reader.ToArray());
    }

    private static void SetId<T>(T entity, Guid id) where T : class
    {
        var property = typeof(T).GetProperty("Id")
            ?? throw new InvalidOperationException($"{typeof(T).Name} has no Id.");
        property.SetValue(entity, id);
    }
}

file sealed class ConsumingSpeechToText : ISpeechToTextService
{
    public async Task<TranscriptionResult> TranscribeAsync(
        Stream audioStream,
        string language,
        CancellationToken cancellationToken = default)
    {
        var buffer = new byte[1024];
        while (await audioStream.ReadAsync(buffer, cancellationToken).ConfigureAwait(false) > 0)
        {
        }

        return new TranscriptionResult
        {
            FullText = "Jane Doe",
            ExtractedFields = new Dictionary<string, string>
            {
                ["FirstName"] = "Jane",
                ["LastName"] = "Doe"
            }
        };
    }
}

file sealed class StubMediator(Guid patientId) : IMediator
{
    public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        if (typeof(TResponse) == typeof(Guid))
            return Task.FromResult((TResponse)(object)patientId);
        return Task.FromResult(default(TResponse)!);
    }

    public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default)
        where TRequest : IRequest
    {
        return Task.CompletedTask;
    }

    public Task<object?> Send(object request, CancellationToken cancellationToken = default) =>
        Task.FromResult<object?>(null);

    public IAsyncEnumerable<TResponse> CreateStream<TResponse>(
        IStreamRequest<TResponse> request,
        CancellationToken cancellationToken = default) =>
        EmptyAsync<TResponse>();

    public IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken = default) =>
        EmptyAsync<object?>();

    public Task Publish(object notification, CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
        where TNotification : INotification =>
        Task.CompletedTask;

    private static async IAsyncEnumerable<T> EmptyAsync<T>()
    {
        await Task.CompletedTask;
        yield break;
    }
}

file sealed class CapturingVoiceStorage : IVoiceRecordingStorage
{
    public string LastKey { get; private set; } = string.Empty;
    public long LastLength { get; private set; }

    public async Task<VoiceRecordingSaveResult> SaveAsync(
        Guid patientId,
        Guid recordingId,
        Stream content,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        await using var copy = new MemoryStream();
        await content.CopyToAsync(copy, cancellationToken).ConfigureAwait(false);
        LastLength = copy.Length;
        LastKey = $"voice-recordings/{patientId:N}/{recordingId:N}.wav";
        return new VoiceRecordingSaveResult { StorageKey = LastKey, DurationSeconds = 1 };
    }
}

file sealed class InMemoryPhotoStorage : IPatientProfilePhotoStorage
{
    private readonly Dictionary<Guid, (byte[] Bytes, string ContentType)> _photos = [];
    public string? SavedPath { get; private set; }

    public void Seed(Guid patientId, byte[] bytes, string contentType) =>
        _photos[patientId] = (bytes, contentType);

    public async Task<PatientProfilePhotoSaveResult> SaveAsync(
        Guid patientId,
        Stream content,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        await using var copy = new MemoryStream();
        await content.CopyToAsync(copy, cancellationToken).ConfigureAwait(false);
        _photos[patientId] = (copy.ToArray(), contentType);
        SavedPath = $"{patientId:N}.jpg";
        return new PatientProfilePhotoSaveResult { RelativePath = SavedPath, ContentType = contentType };
    }

    public Task<PatientProfilePhotoReadResult?> OpenReadAsync(Guid patientId, CancellationToken cancellationToken = default)
    {
        if (!_photos.TryGetValue(patientId, out var photo))
            return Task.FromResult<PatientProfilePhotoReadResult?>(null);

        return Task.FromResult<PatientProfilePhotoReadResult?>(new PatientProfilePhotoReadResult
        {
            Content = new MemoryStream(photo.Bytes),
            ContentType = photo.ContentType
        });
    }

    public Task DeleteAsync(Guid patientId, CancellationToken cancellationToken = default)
    {
        _photos.Remove(patientId);
        return Task.CompletedTask;
    }
}

file sealed class FakePatientQuery(AdminClinicPatientDetailDto? detail) : IAdminClinicPatientQueryService
{
    public Task<PagedResult<AdminClinicPatientListItemDto>> GetPatientsAsync(
        Guid clinicId,
        int pageNumber,
        int pageSize,
        string? search = null,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(new PagedResult<AdminClinicPatientListItemDto>
        {
            Items = [],
            TotalCount = 0,
            PageNumber = pageNumber,
            PageSize = pageSize
        });

    public Task<AdminClinicPatientDetailDto?> GetPatientDetailAsync(
        Guid clinicId,
        Guid patientId,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(detail);

    public Task<AdminClinicVisitClinicalDocumentationDto> GetVisitClinicalDocumentationAsync(
        Guid visitId,
        DateTime visitStart,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(new AdminClinicVisitClinicalDocumentationDto());
}

file sealed class FakeCurrentUser(Guid userId, Guid? patientId) : ICurrentUserService
{
    public string? UserId { get; } = userId.ToString();
    public Guid? CurrentUserId { get; } = userId;
    public string? UserName => "tester";
    public Guid? CurrentPatientId { get; } = patientId;
    public bool IsAuthenticated => true;
    public string? IpAddress => null;
    public string? UserAgent => null;
}

file sealed class FakeMembership(bool hasMembership) : IClinicStaffMembershipService
{
    public Task EnsureMembershipAsync(Guid applicationUserId, Guid clinicId, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    public Task<IReadOnlyList<Guid>> GetClinicIdsForUserAsync(Guid applicationUserId, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<Guid>>([]);

    public Task<bool> HasMembershipAsync(Guid applicationUserId, Guid clinicId, CancellationToken cancellationToken = default) =>
        Task.FromResult(hasMembership);

    public Task<int> GetActiveStaffCountAsync(Guid clinicId, CancellationToken cancellationToken = default) =>
        Task.FromResult(1);

    public Task<IReadOnlyList<ClinicStaffMembershipEntry>> GetStaffMembershipsAsync(
        Guid clinicId,
        CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<ClinicStaffMembershipEntry>>([]);

    public Task<bool> DeactivateMembershipAsync(
        Guid applicationUserId,
        Guid clinicId,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(true);

    public Task RecordInvitationSentAsync(Guid applicationUserId, Guid clinicId, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;
}

file sealed class FakeUnitOfWork : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => Task.FromResult(0);
}

file sealed class NoOpIdentityTimeline : IPatientIdentityTimelineService
{
    public Task RecordEventAsync(
        Guid patientId,
        RaphCare.Domain.Patients.Enums.PatientIdentityEventType eventType,
        object? eventData,
        Guid? performedByUserId,
        CancellationToken ct) =>
        Task.CompletedTask;
}

file sealed class FakeRepository<T> : IRepository<T> where T : class
{
    public List<T> Items { get; }

    public FakeRepository(IEnumerable<T>? items = null)
    {
        Items = items?.ToList() ?? [];
    }

    public Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var match = Items.FirstOrDefault(item =>
        {
            var value = typeof(T).GetProperty("Id")?.GetValue(item);
            return value is Guid guid && guid == id;
        });
        return Task.FromResult(match);
    }

    public Task<IReadOnlyList<T>> ListAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<T>>(Items);

    public Task<PagedResult<T>> SearchAsync(
        Func<IQueryable<T>, IQueryable<T>>? queryShaper,
        int pageNumber,
        int pageSize,
        bool applyDefaultIdOrdering = true,
        CancellationToken cancellationToken = default)
    {
        IQueryable<T> query = Items.AsQueryable();
        if (queryShaper is not null)
            query = queryShaper(query);
        var list = query.ToList();
        return Task.FromResult(new PagedResult<T>
        {
            Items = list.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList(),
            TotalCount = list.Count,
            PageNumber = pageNumber,
            PageSize = pageSize
        });
    }

    public Task AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        Items.Add(entity);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(T entity, CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
    {
        Items.Remove(entity);
        return Task.CompletedTask;
    }
}
