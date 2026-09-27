using SportAcademy.Application.Interfaces;

namespace SportAcademy.Infrastructure.Implementations;

/// <summary>
/// Default IWhatsAppApiClient until a real provider is chosen (see that interface's own remarks)
/// - always reports IsConfigured: false and fails every send with a clear reason.
/// WhatsAppChannelSender uses IsConfigured to skip without retrying rather than burning a retry
/// budget against a channel that was never going to work.
/// </summary>
public class NotConfiguredWhatsAppApiClient : IWhatsAppApiClient
{
    public bool IsConfigured => false;

    public Task<WhatsAppSendResult> SendTemplateMessageAsync(
        string toPhoneNumberE164,
        string templateName,
        IReadOnlyDictionary<string, string> parameters,
        CancellationToken ct = default)
        => Task.FromResult(new WhatsAppSendResult(false, null, "No WhatsApp provider is configured yet."));
}
