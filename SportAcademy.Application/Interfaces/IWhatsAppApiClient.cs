namespace SportAcademy.Application.Interfaces;

/// <summary>
/// The seam a real WhatsApp Business API integration plugs into once a provider is chosen
/// (Twilio, Meta's Cloud API directly, etc.) - WhatsAppChannelSender depends on this, not on any
/// provider SDK, so picking a provider later is a single DI registration swap
/// (DependencyInjection.cs's IWhatsAppApiClient line) plus one new implementation class, with no
/// change to the channel-dispatch/outbox pipeline around it.
///
/// Shaped around a TEMPLATE send, not a raw text send, because every WhatsApp Business API
/// (Twilio and Meta's Cloud API alike) requires business-initiated messages - anything sent
/// outside a 24-hour customer-service window opened by the recipient - to use a pre-approved
/// message template, not arbitrary text. templateName and parameters are provider- and
/// account-specific (an approved template's own name and its named placeholder values); nothing
/// in this codebase can invent those - they only exist once a real provider account is set up and
/// templates are submitted and approved through it.
/// </summary>
public interface IWhatsAppApiClient
{
    /// <summary>True while a real provider is configured behind this interface - lets
    /// WhatsAppChannelSender fail fast with a clear "not configured" reason instead of a
    /// provider-shaped error that doesn't actually come from a provider.</summary>
    bool IsConfigured { get; }

    Task<WhatsAppSendResult> SendTemplateMessageAsync(
        string toPhoneNumberE164,
        string templateName,
        IReadOnlyDictionary<string, string> parameters,
        CancellationToken ct = default);
}

public readonly record struct WhatsAppSendResult(bool Succeeded, string? ProviderMessageId, string? Error);
