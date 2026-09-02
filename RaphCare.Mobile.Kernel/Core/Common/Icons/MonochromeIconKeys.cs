namespace RaphCare.Mobile.Core.Common.Icons;

/// <summary>
/// MAUI image resource names for tintable line icons (SVG under <c>Resources/Images</c>).
/// Prefer these over emoji so Home / Profile stay monochrome like the admin Web UI.
/// </summary>
public static class MonochromeIconKeys
{
    public const string Calendar = "icon_calendar.png";
    public const string CalendarOnAccent = "icon_calendar_on_accent.png";
    public const string Clipboard = "icon_clipboard.png";
    public const string Shield = "icon_shield.png";
    public const string Watch = "icon_watch.png";
    public const string Brain = "icon_brain.png";
    public const string Sparkles = "icon_sparkles.png";
    public const string Heart = "icon_heart.png";
    public const string Activity = "icon_activity.png";
    public const string Droplet = "icon_droplet.png";
    public const string User = "icon_user.png";
    public const string Hospital = "icon_hospital.png";
    public const string Lock = "icon_lock.png";
    public const string Globe = "icon_globe.png";
    public const string Phone = "icon_phone.png";
    public const string Users = "icon_users.png";
    public const string CreditCard = "icon_credit_card.png";
    public const string Receipt = "icon_receipt.png";
    public const string Bell = "icon_bell.png";
    public const string Eye = "icon_eye.png";
    public const string Help = "icon_help.png";
    public const string Message = "icon_message.png";
    public const string Mail = "icon_mail.png";
    public const string Upload = "icon_upload.png";
    public const string Download = "icon_download.png";
    public const string Trash = "icon_trash.png";

    /// <summary>All catalog keys used by Home / Profile / Settings chrome.</summary>
    public static IReadOnlyList<string> All { get; } =
    [
        Calendar, CalendarOnAccent, Clipboard, Shield, Watch, Brain, Sparkles, Heart, Activity, Droplet,
        User, Hospital, Lock, Globe, Phone, Users, CreditCard, Receipt, Bell, Eye, Help,
        Message, Mail, Upload, Download, Trash,
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
