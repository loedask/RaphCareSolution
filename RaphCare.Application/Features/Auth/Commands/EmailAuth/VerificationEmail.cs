using System.Net;

namespace RaphCare.Application.Features.Auth.Commands.EmailAuth;

/// <summary>Subject and bodies for the 6-digit verification email.</summary>
internal static class VerificationEmail
{
    internal const string Subject = "Your RaphCare verification code";

    internal static (string PlainBody, string HtmlBody) Create(string code)
    {
        var plain =
            $"""
            Your RaphCare verification code is {code}.

            Enter it on the website or in the app to continue. It expires in 10 minutes.

            If you did not request this, you can ignore this email.
            """;

        var encoded = WebUtility.HtmlEncode(code);
        var html =
            $"""
            <!DOCTYPE html>
            <html lang="en">
            <head>
              <meta charset="utf-8" />
              <meta name="viewport" content="width=device-width, initial-scale=1" />
              <title>{Subject}</title>
            </head>
            <body style="margin:0;padding:0;background-color:#f1f5f9;font-family:'Segoe UI',Helvetica,Arial,sans-serif;">
              <div style="display:none;max-height:0;overflow:hidden;opacity:0;">
                Your verification code is {encoded}. It expires in 10 minutes.
              </div>
              <table role="presentation" width="100%" cellspacing="0" cellpadding="0" style="background-color:#f1f5f9;padding:24px 12px;">
                <tr>
                  <td align="center">
                    <table role="presentation" width="480" cellspacing="0" cellpadding="0" style="max-width:480px;width:100%;background-color:#ffffff;border:1px solid #e2e8f0;">
                      <tr>
                        <td style="background-color:#0d9488;padding:20px 28px;">
                          <p style="margin:0;font-size:20px;font-weight:700;color:#ffffff;">RaphCare</p>
                          <p style="margin:4px 0 0;font-size:11px;letter-spacing:0.12em;color:#ccfbf1;">HEALTHCARE PLATFORM</p>
                        </td>
                      </tr>
                      <tr>
                        <td style="padding:28px;">
                          <p style="margin:0 0 8px;font-size:18px;font-weight:700;color:#0f172a;">Your verification code</p>
                          <p style="margin:0 0 20px;font-size:15px;line-height:1.5;color:#64748b;">Enter this code to continue. It expires in 10 minutes.</p>
                          <table role="presentation" width="100%" cellspacing="0" cellpadding="0">
                            <tr>
                              <td align="center" style="background-color:#f0fdfa;border:1px solid #99f6e4;padding:18px 12px;">
                                <p style="margin:0;font-size:32px;font-weight:700;letter-spacing:0.28em;color:#0f766e;font-family:Consolas,'Courier New',monospace;">{encoded}</p>
                              </td>
                            </tr>
                          </table>
                          <p style="margin:20px 0 0;font-size:13px;line-height:1.5;color:#64748b;">If you did not request this, you can ignore this email.</p>
                        </td>
                      </tr>
                    </table>
                  </td>
                </tr>
              </table>
            </body>
            </html>
            """;

        return (plain, html);
    }
}
