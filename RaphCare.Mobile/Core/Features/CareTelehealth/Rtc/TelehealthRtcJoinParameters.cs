namespace RaphCare.Mobile.Core.Features.CareTelehealth.Rtc;

/// <summary>Arguments to join an Agora channel (from API join-info).</summary>
public sealed record TelehealthRtcJoinParameters(string AppId, string ChannelName, string Token, int Uid);
