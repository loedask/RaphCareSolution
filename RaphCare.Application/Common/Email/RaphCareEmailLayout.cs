using System.Globalization;
using System.Net;
using System.Text;

namespace RaphCare.Application.Common.Email;

/// <summary>Shared HTML shell for RaphCare outbound mail (verification, invitations, support).</summary>
public static class RaphCareEmailLayout
{
    public const string BrandName = "RaphCare";
    public const string Tagline = "Connected healthcare platform";

    internal const string PageBackground = "#f3f1ec";
    internal const string CardBackground = "#ffffff";
    internal const string Accent = "#0d9488";
    internal const string AccentDark = "#0f766e";
    internal const string Text = "#1c1917";
    internal const string Muted = "#5c5a57";
    internal const string CalloutBackground = "#f7f5f1";
    internal const string Font = "'Segoe UI',Helvetica,Arial,sans-serif";

    public static string Wrap(string innerHtml, string? preheader = null)
    {
        var preview = Encode(string.IsNullOrWhiteSpace(preheader) ? BrandName : preheader);
        return
            $"""
            <!DOCTYPE html>
            <html lang="en">
            <head>
              <meta charset="utf-8" />
              <meta name="viewport" content="width=device-width, initial-scale=1" />
              <title>{Encode(BrandName)}</title>
            </head>
            <body style="margin:0;padding:0;background-color:{PageBackground};font-family:{Font};">
              <div style="display:none;max-height:0;overflow:hidden;opacity:0;">{preview}</div>
              <table role="presentation" width="100%" cellspacing="0" cellpadding="0" bgcolor="{PageBackground}" style="background-color:{PageBackground};padding:32px 16px;">
                <tr>
                  <td align="center">
                    {Logo()}
                    <table role="presentation" width="600" cellspacing="0" cellpadding="0" style="max-width:600px;width:100%;background-color:{CardBackground};border-radius:24px;">
                      <tr>
                        <td bgcolor="{Accent}" style="background-color:{Accent};padding:18px 32px;border-radius:24px 24px 0 0;">
                          <p style="margin:0;font-size:16px;font-weight:700;color:#ffffff;">{Encode(Tagline)}</p>
                        </td>
                      </tr>
                      <tr>
                        <td style="padding:36px 40px 32px;">
                          {innerHtml}
                          <p style="margin:28px 0 0;font-size:15px;line-height:1.5;color:{Text};">The RaphCare team</p>
                        </td>
                      </tr>
                    </table>
                    <p style="margin:24px 0 0;font-size:12px;line-height:1.6;color:{Muted};text-align:center;">
                      This email was sent automatically by {Encode(BrandName)}.<br />
                      If you were not expecting it, you can ignore it.
                    </p>
                  </td>
                </tr>
              </table>
            </body>
            </html>
            """;
    }

    public static string FromPlainBody(string plainBody) =>
        Wrap(PlainToHtml(plainBody), FirstLine(plainBody));

    public static string Heading(string text) =>
        $"<p style=\"margin:0 0 16px;font-size:22px;font-weight:700;color:{Text};\">{Encode(text)}</p>";

    public static string Greeting(string? name)
    {
        var hello = string.IsNullOrWhiteSpace(name) ? "Hello," : $"Hello {name.Trim()},";
        return $"<p style=\"margin:0 0 12px;font-size:16px;font-weight:700;color:{Text};\">{Encode(hello)}</p>";
    }

    public static string Paragraph(string text) =>
        $"<p style=\"margin:0 0 12px;font-size:15px;line-height:1.6;color:{Text};\">{Encode(text)}</p>";

    public static string Button(string href, string label)
    {
        var safeHref = Encode(href);
        var safeLabel = Encode(label);
        return
            $"""
            <table role="presentation" cellspacing="0" cellpadding="0" style="margin:24px 0 8px;">
              <tr>
                <td bgcolor="{Accent}" style="background-color:{Accent};border-radius:8px;">
                  <a href="{safeHref}" style="display:inline-block;padding:14px 28px;font-size:14px;font-weight:700;color:#ffffff;text-decoration:none;">{safeLabel}</a>
                </td>
              </tr>
            </table>
            """;
    }

    public static string FallbackUrl(string href)
    {
        var safeHref = Encode(href);
        return
            $"""
            <p style="margin:16px 0 0;font-size:13px;line-height:1.5;color:{Muted};">If the button does not work, copy this address into your browser:</p>
            <p style="margin:6px 0 0;font-size:13px;line-height:1.5;word-break:break-all;"><a href="{safeHref}" style="color:{AccentDark};text-decoration:none;">{safeHref}</a></p>
            """;
    }

    public static string CodeBox(string code)
    {
        var encoded = Encode(code);
        return
            $"""
            <table role="presentation" width="100%" cellspacing="0" cellpadding="0" style="margin:20px 0;">
              <tr>
                <td align="center" bgcolor="{CalloutBackground}" style="background-color:{CalloutBackground};padding:22px 16px;border-radius:12px;">
                  <p style="margin:0;font-size:32px;font-weight:700;letter-spacing:0.28em;color:{AccentDark};font-family:Consolas,'Courier New',monospace;">{encoded}</p>
                </td>
              </tr>
            </table>
            """;
    }

    public static string InfoBox(IReadOnlyList<(string Label, string Value)> rows)
    {
        var sb = new StringBuilder();
        sb.Append($"<table role=\"presentation\" width=\"100%\" cellspacing=\"0\" cellpadding=\"0\" style=\"margin:16px 0;background-color:{CalloutBackground};border-radius:12px;\">");
        sb.Append("<tr><td style=\"padding:18px 20px;\">");
        for (var i = 0; i < rows.Count; i++)
        {
            var (label, value) = rows[i];
            var top = i == 0 ? "0" : "10px";
            sb.Append(CultureInfo.InvariantCulture, $"<p style=\"margin:{top} 0 0;font-size:13px;line-height:1.5;color:{Muted};\"><strong style=\"color:{Text};\">{Encode(label)}:</strong> {Encode(value)}</p>");
        }

        sb.Append("</td></tr></table>");
        return sb.ToString();
    }

    public static string MessageBox(string message) =>
        $"<table role=\"presentation\" width=\"100%\" cellspacing=\"0\" cellpadding=\"0\" style=\"margin:16px 0;background-color:{CalloutBackground};border-radius:12px;\"><tr><td style=\"padding:18px 20px;font-size:15px;line-height:1.6;color:{Text};\">{Encode(message).Replace("\n", "<br />", StringComparison.Ordinal)}</td></tr></table>";

    public static string Encode(string? value) => WebUtility.HtmlEncode(value ?? string.Empty);

    private static string Logo() =>
        $"""
        <table role="presentation" cellspacing="0" cellpadding="0" style="margin:0 0 20px;">
          <tr>
            <td bgcolor="{Accent}" width="28" height="28" align="center" valign="middle" style="background-color:{Accent};color:#ffffff;font-size:18px;font-weight:700;border-radius:8px;">+</td>
            <td style="padding-left:10px;font-size:20px;font-weight:700;color:{Text};">{Encode(BrandName)}</td>
          </tr>
        </table>
        """;

    private static string PlainToHtml(string plainBody)
    {
        var blocks = plainBody.Replace("\r\n", "\n", StringComparison.Ordinal).Split("\n\n", StringSplitOptions.None);
        var sb = new StringBuilder();
        foreach (var block in blocks)
        {
            var text = block.Trim();
            if (text.Length == 0)
                continue;
            sb.Append(CultureInfo.InvariantCulture, $"<p style=\"margin:0 0 12px;font-size:15px;line-height:1.6;color:{Text};\">{Encode(text).Replace("\n", "<br />", StringComparison.Ordinal)}</p>");
        }

        return sb.ToString();
    }

    private static string FirstLine(string plainBody)
    {
        using var reader = new StringReader(plainBody);
        while (reader.ReadLine() is { } line)
        {
            if (!string.IsNullOrWhiteSpace(line))
                return line.Trim();
        }

        return BrandName;
    }
}

/// <summary>Subject plus plain and HTML bodies for one outbound message.</summary>
public sealed record EmailContent(string Subject, string PlainBody, string HtmlBody);
