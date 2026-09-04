using MediatR;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Patients.Commands.CreatePatient;
using RaphCare.Domain.Patients;
using RaphCare.Domain.Patients.Enums;

namespace RaphCare.Application.Features.Onboarding.Commands.CreatePatientFromVoice;

public class CreatePatientFromVoiceHandler(
    ISpeechToTextService speechToTextService,
    IMediator mediator,
    IRepository<VoiceRecording> voiceRecordingRepository,
    IRepository<Patient> patientRepository,
    IUnitOfWork unitOfWork,
    IPatientIdentityTimelineService patientIdentityTimelineService,
    IVoiceRecordingStorage voiceRecordingStorage) : IRequestHandler<CreatePatientFromVoiceCommand, CreatePatientFromVoiceResult>
{
    private readonly ISpeechToTextService _speechToTextService = speechToTextService;
    private readonly IMediator _mediator = mediator;
    private readonly IRepository<VoiceRecording> _voiceRecordingRepository = voiceRecordingRepository;
    private readonly IRepository<Patient> _patientRepository = patientRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IPatientIdentityTimelineService _patientIdentityTimelineService = patientIdentityTimelineService;
    private readonly IVoiceRecordingStorage _voiceRecordingStorage = voiceRecordingStorage;

    public async Task<CreatePatientFromVoiceResult> Handle(CreatePatientFromVoiceCommand request, CancellationToken cancellationToken)
    {
        await using var audioBuffer = new MemoryStream();
        await request.AudioStream.CopyToAsync(audioBuffer, cancellationToken).ConfigureAwait(false);
        audioBuffer.Position = 0;

        var transcription = await _speechToTextService.TranscribeAsync(
            audioBuffer,
            request.Language,
            cancellationToken);
        audioBuffer.Position = 0;

        var fields = transcription.ExtractedFields;
        var firstName = GetField(fields, "FirstName") ?? "Unknown";
        var lastName = GetField(fields, "LastName") ?? "Unknown";
        var dateOfBirth = ParseDateOfBirth(GetField(fields, "DateOfBirth")) ?? new DateTime(1990, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        var createPatient = new CreatePatientCommand
        {
            ClinicId = request.ClinicId,
            FirstName = firstName,
            LastName = lastName,
            DateOfBirth = dateOfBirth,
            PhoneNumber = request.PhoneNumber,
            SourceSystem = request.ClinicId.ToString()
        };

        var patientId = await _mediator.Send(createPatient, cancellationToken);

        var patient = await _patientRepository.GetByIdAsync(patientId, cancellationToken);
        if (patient != null && !string.IsNullOrWhiteSpace(request.PhoneNumber))
        {
            var oldPhoneNumber = patient.PhoneNumber;
            var newPhoneNumber = request.PhoneNumber.Trim();
            patient.PhoneNumber = newPhoneNumber;
            await _patientRepository.UpdateAsync(patient, cancellationToken);

            await _patientIdentityTimelineService.RecordEventAsync(
                patient.Id,
                PatientIdentityEventType.PatientUpdated,
                new
                {
                    PhoneNumber = new
                    {
                        OldValue = oldPhoneNumber,
                        NewValue = newPhoneNumber
                    }
                },
                performedByUserId: null,
                cancellationToken);
        }

        var recording = new VoiceRecording
        {
            PatientId = patientId,
            Language = request.Language
        };
        var contentType = string.IsNullOrWhiteSpace(request.ContentType) ? "audio/wav" : request.ContentType;
        var saved = await _voiceRecordingStorage
            .SaveAsync(patientId, recording.Id, audioBuffer, contentType, cancellationToken)
            .ConfigureAwait(false);
        recording.StorageUrl = saved.StorageKey;
        recording.DurationSeconds = saved.DurationSeconds;
        await _voiceRecordingRepository.AddAsync(recording, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new CreatePatientFromVoiceResult
        {
            PatientId = patientId,
            Transcription = transcription.FullText
        };
    }

    private static string? GetField(Dictionary<string, string> fields, string key)
    {
        if (fields.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value))
            return value.Trim();
        return null;
    }

    private static DateTime? ParseDateOfBirth(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        if (DateTime.TryParse(value, out var d)) return DateTime.SpecifyKind(d, DateTimeKind.Utc);
        return null;
    }
}
