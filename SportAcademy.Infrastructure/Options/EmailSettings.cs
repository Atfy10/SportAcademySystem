namespace SportAcademy.Infrastructure.Options;

public class EmailSettings
{
    public string Provider { get; set; } = "SendGrid";
    public string SendGridApiKey { get; set; } = string.Empty;
    public string FromEmail { get; set; } = "noreply@auraacademy.com";
    public string FromName { get; set; } = "AURA Academy";
}
