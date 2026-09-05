namespace RaphCare.Mobile.Core.Common.Icons;

/// <summary>
/// MAUI image resource names for app icons (SVG under <c>Resources/Images</c>).
/// Teal stroke icons for soft wells; <c>*OnAccent</c> white icons for gradient wells.
/// </summary>
public static class MonochromeIconKeys
{
    public const string Calendar = "icon_calendar.png";
    public const string CalendarOnAccent = "icon_calendar_on_accent.png";
    public const string Clipboard = "icon_clipboard.png";
    public const string ClipboardOnAccent = "icon_clipboard_on_accent.png";
    public const string Shield = "icon_shield.png";
    public const string ShieldOnAccent = "icon_shield_on_accent.png";
    public const string Watch = "icon_watch.png";
    public const string WatchOnAccent = "icon_watch_on_accent.png";
    public const string Brain = "icon_brain.png";
    public const string BrainOnAccent = "icon_brain_on_accent.png";
    public const string Sparkles = "icon_sparkles.png";
    public const string SparklesOnAccent = "icon_sparkles_on_accent.png";
    public const string Heart = "icon_heart.png";
    public const string HeartOnAccent = "icon_heart_on_accent.png";
    public const string Activity = "icon_activity.png";
    public const string ActivityOnAccent = "icon_activity_on_accent.png";
    public const string Droplet = "icon_droplet.png";
    public const string DropletOnAccent = "icon_droplet_on_accent.png";
    public const string User = "icon_user.png";
    public const string UserOnAccent = "icon_user_on_accent.png";
    public const string Hospital = "icon_hospital.png";
    public const string HospitalOnAccent = "icon_hospital_on_accent.png";
    public const string Lock = "icon_lock.png";
    public const string LockOnAccent = "icon_lock_on_accent.png";
    public const string Globe = "icon_globe.png";
    public const string GlobeOnAccent = "icon_globe_on_accent.png";
    public const string Phone = "icon_phone.png";
    public const string PhoneOnAccent = "icon_phone_on_accent.png";
    public const string Users = "icon_users.png";
    public const string UsersOnAccent = "icon_users_on_accent.png";
    public const string CreditCard = "icon_credit_card.png";
    public const string CreditCardOnAccent = "icon_credit_card_on_accent.png";
    public const string Receipt = "icon_receipt.png";
    public const string ReceiptOnAccent = "icon_receipt_on_accent.png";
    public const string Bell = "icon_bell.png";
    public const string BellOnAccent = "icon_bell_on_accent.png";
    public const string Eye = "icon_eye.png";
    public const string EyeOnAccent = "icon_eye_on_accent.png";
    public const string Help = "icon_help.png";
    public const string HelpOnAccent = "icon_help_on_accent.png";
    public const string Message = "icon_message.png";
    public const string MessageOnAccent = "icon_message_on_accent.png";
    public const string Mail = "icon_mail.png";
    public const string MailOnAccent = "icon_mail_on_accent.png";
    public const string Upload = "icon_upload.png";
    public const string UploadOnAccent = "icon_upload_on_accent.png";
    public const string Download = "icon_download.png";
    public const string DownloadOnAccent = "icon_download_on_accent.png";
    public const string Trash = "icon_trash.png";
    public const string TrashOnAccent = "icon_trash_on_accent.png";
    public const string Mic = "icon_mic.png";
    public const string MicOnAccent = "icon_mic_on_accent.png";

    /// <summary>All catalog keys used by Home / Profile / Settings chrome.</summary>
    public static IReadOnlyList<string> All { get; } =
    [
        Calendar, CalendarOnAccent, Clipboard, ClipboardOnAccent, Shield, ShieldOnAccent,
        Watch, WatchOnAccent, Brain, BrainOnAccent, Sparkles, SparklesOnAccent,
        Heart, HeartOnAccent, Activity, ActivityOnAccent, Droplet, DropletOnAccent,
        User, UserOnAccent, Hospital, HospitalOnAccent, Lock, LockOnAccent,
        Globe, GlobeOnAccent, Phone, PhoneOnAccent, Users, UsersOnAccent,
        CreditCard, CreditCardOnAccent, Receipt, ReceiptOnAccent, Bell, BellOnAccent,
        Eye, EyeOnAccent, Help, HelpOnAccent, Message, MessageOnAccent,
        Mail, MailOnAccent, Upload, UploadOnAccent, Download, DownloadOnAccent,
        Trash, TrashOnAccent, Mic, MicOnAccent,
    ];

    /// <summary>
    /// True when <paramref name="value"/> looks like a multicolored emoji / surrogate glyph
    /// rather than a MAUI image resource name (e.g. <c>icon_heart.png</c>).
    /// </summary>
    public static bool LooksLikeEmojiGlyph(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return false;

        foreach (var c in value)
        {
            if (char.IsSurrogate(c))
                return true;
            if (c > 0xFF)
                return true;
        }

        return false;
    }
}
