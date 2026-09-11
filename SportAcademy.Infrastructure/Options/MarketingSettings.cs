namespace SportAcademy.Infrastructure.Options;

public class MarketingSettings
{
    // Where a new-lead notification is sent. The domain is already Resend-verified for
    // noreply@auraacademys.com (see EmailSettings), so this inbox just needs to exist and be
    // monitored - no separate DNS/provider setup.
    public string SalesInboxEmail { get; set; } = "sales@auraacademys.com";
}
