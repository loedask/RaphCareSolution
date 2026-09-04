# Azure Blob file storage

Private files for patient profile photos and voice onboarding audio. Hosted environments use Azure Blob. Local Development uses a folder on the API host.

Photos and recordings are not public. The API streams them after auth and clinic checks. Do not put SAS or public blob URLs in the database.

## What is stored

| Kind | Container | Key shape |
|------|-----------|-----------|
| Profile photo | `patient-photos` | `{patientId}.jpg` (or `.png` / `.webp`) |
| Voice onboarding audio | `voice-recordings` | `{patientId}/{recordingId}.wav` (or matching audio extension) |

`VoiceRecording.StorageUrl` holds the private key (`voice-recordings/...`), not a public URL.

## Configuration

Set `AzureStorage:ConnectionString` on the API (App Service application settings in Staging). Empty connection string is allowed only in Development; then files go under `App_Data/object-store`.

```text
AzureStorage__ConnectionString
AzureStorage__PhotosContainer=patient-photos
AzureStorage__VoiceRecordingsContainer=voice-recordings
```

Create the account and wire `raphcare-api` with `scripts/New-RaphCareAzureBlobStorage.ps1` (az CLI, same login as other Azure scripts).

## Apps

- **Mobile:** upload and view the patient's own photo (`api/patient/profile/photo`). Voice sign-up posts audio to `api/onboarding/voice`; the API keeps the file after transcription.
- **Web admin:** hospital patient chart loads the photo from `api/admin/clinics/{clinicId}/patients/{patientId}/photo` when the staff member belongs to that hospital and the patient is linked there.
- **API:** same contracts; Development can run without Azure.
