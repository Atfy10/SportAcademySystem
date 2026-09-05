namespace SportAcademy.Infrastructure.Options;

public class EmailSettings
{
    /// <summary>
    /// Which provider actually sends: "Resend" or "SendGrid" (case-insensitive). Both are
    /// implemented; this picks one at startup (see Program.cs), so switching is a config change
    /// and a restart rather than a deploy. Only the matching key below needs to be set.
    /// </summary>
    public string Provider { get; set; } = "Resend";

    public string ResendApiKey { get; set; } = string.Empty;
    public string SendGridApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Must belong to a domain verified with whichever provider is active - both reject sends
    /// from an unverified address.
    /// </summary>
    public string FromEmail { get; set; } = "noreply@auraacademys.com";
    public string FromName { get; set; } = "AURA Academy";
}
