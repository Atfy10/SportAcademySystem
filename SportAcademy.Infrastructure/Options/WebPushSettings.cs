namespace SportAcademy.Infrastructure.Options;

/// <summary>
/// VAPID (RFC 8292) keypair identifying this server to push services (FCM, Mozilla's, etc.) -
/// self-issued, not a third-party account. Generate once with
/// WebPush.VapidHelper.GenerateVapidKeys() and store PublicKey/PrivateKey as secrets (env vars /
/// user-secrets in dev, never committed) - PublicKey is also handed to the browser as-is via
/// PushSubscriptionsController's vapid-public-key endpoint for PushManager.subscribe(). Blank in
/// source control; PushChannelSender treats an unconfigured keypair as "channel not usable" and
/// fails every send with ShouldRetry:false rather than throwing.
/// </summary>
public class WebPushSettings
{
    public string PublicKey { get; set; } = string.Empty;
    public string PrivateKey { get; set; } = string.Empty;

    /// <summary>A contact URI push services may use to reach the sender if something's wrong -
    /// "mailto:" address or an "https:" URL, per RFC 8292.</summary>
    public string Subject { get; set; } = "mailto:support@auraacademys.com";
}
