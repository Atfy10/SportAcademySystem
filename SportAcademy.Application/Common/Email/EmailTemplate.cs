namespace SportAcademy.Application.Common.Email;

/// <summary>
/// Renders every outgoing email inside the same branded shell (matches the frontend's
/// primary teal - see index.css --primary: 185 85% 35% / #0D98A5). Table-based layout with
/// every style inlined on purpose: this has to render correctly in Outlook desktop's Word
/// engine, which ignores &lt;style&gt; blocks, CSS gradients, and border-radius on some
/// versions - so no gradients here even though the web UI uses one for its logo mark.
/// </summary>
public static class EmailTemplate
{
    private const string BrandTeal = "#0D98A5";
    private const string HeadingColor = "#1F242E";
    private const string BodyTextColor = "#3C4453";
    private const string MutedTextColor = "#8B93A1";
    private const string PageBackground = "#F6F7F8";
    private const string BorderColor = "#E7EAEE";

    /// <param name="preheader">Short summary shown in the inbox preview line, hidden in the body.</param>
    /// <param name="heading">Main heading inside the card.</param>
    /// <param name="bodyHtml">Body content - simple HTML (paragraphs, etc.) is fine.</param>
    /// <param name="ctaText">Button label. Pass null to render no button (e.g. informational-only emails).</param>
    /// <param name="ctaUrl">Button target. Required when <paramref name="ctaText"/> is set.</param>
    /// <param name="footerNote">Short reassurance/disclaimer line above the copyright line.</param>
    public static string Render(
        string preheader,
        string heading,
        string bodyHtml,
        string? ctaText = null,
        string? ctaUrl = null,
        string? footerNote = null)
    {
        var ctaBlock = ctaText is not null && ctaUrl is not null
            ? $"""
               <table role="presentation" cellpadding="0" cellspacing="0" style="margin:28px 0 4px 0;">
                 <tr>
                   <td style="border-radius:8px;background-color:{BrandTeal};">
                     <a href="{ctaUrl}" target="_blank" style="display:inline-block;padding:12px 28px;font-size:15px;font-weight:600;color:#ffffff;text-decoration:none;border-radius:8px;">{ctaText}</a>
                   </td>
                 </tr>
               </table>
               <p style="margin:16px 0 0 0;font-size:13px;line-height:20px;color:{MutedTextColor};word-break:break-all;">
                 Or copy and paste this link into your browser:<br/>
                 <a href="{ctaUrl}" style="color:{BrandTeal};">{ctaUrl}</a>
               </p>
               """
            : string.Empty;

        var footerBlock = footerNote is not null
            ? $"""<p style="margin:0 0 8px 0;font-size:12px;line-height:18px;color:{MutedTextColor};">{footerNote}</p>"""
            : string.Empty;

        return $"""
            <!doctype html>
            <html lang="en">
              <body style="margin:0;padding:0;background-color:{PageBackground};font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',Roboto,Helvetica,Arial,sans-serif;">
                <span style="display:none;font-size:1px;color:{PageBackground};line-height:1px;max-height:0;max-width:0;opacity:0;overflow:hidden;">{preheader}</span>
                <table role="presentation" width="100%" cellpadding="0" cellspacing="0" style="background-color:{PageBackground};padding:32px 16px;">
                  <tr>
                    <td align="center">
                      <table role="presentation" width="600" cellpadding="0" cellspacing="0" style="max-width:600px;width:100%;background-color:#ffffff;border-radius:12px;overflow:hidden;">
                        <tr>
                          <td style="background-color:{BrandTeal};padding:28px 32px;text-align:center;">
                            <span style="font-size:22px;font-weight:700;color:#ffffff;letter-spacing:0.5px;">AURA <span style="font-weight:400;opacity:0.85;">ACADEMY</span></span>
                          </td>
                        </tr>
                        <tr>
                          <td style="padding:36px 32px 24px 32px;">
                            <h1 style="margin:0 0 16px 0;font-size:20px;line-height:28px;color:{HeadingColor};">{heading}</h1>
                            <div style="font-size:15px;line-height:24px;color:{BodyTextColor};">
                              {bodyHtml}
                            </div>
                            {ctaBlock}
                          </td>
                        </tr>
                        <tr>
                          <td style="padding:24px 32px;background-color:{PageBackground};border-top:1px solid {BorderColor};">
                            {footerBlock}
                            <p style="margin:0;font-size:12px;color:#A7ADB8;">&copy; {DateTime.UtcNow.Year} AURA Academy. All rights reserved.</p>
                          </td>
                        </tr>
                      </table>
                    </td>
                  </tr>
                </table>
              </body>
            </html>
            """;
    }
}
