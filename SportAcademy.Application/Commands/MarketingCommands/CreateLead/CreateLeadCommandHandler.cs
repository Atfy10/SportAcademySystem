using MediatR;
using SportAcademy.Application.Common.Email;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities.Marketing;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Commands.MarketingCommands.CreateLead;

public class CreateLeadCommandHandler : IRequestHandler<CreateLeadCommand, Result<Guid>>
{
    // A submission answered faster than this is treated as a bot filling the form
    // programmatically - no real person reads the fields and types an answer in under 3s.
    private static readonly TimeSpan MinHumanFillTime = TimeSpan.FromSeconds(3);

    private readonly IBaseRepository<Lead, Guid> _leadRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;
    private readonly IMarketingSettingsProvider _marketingSettings;
    private readonly string _operation = OperationType.Add.ToString();

    public CreateLeadCommandHandler(
        IBaseRepository<Lead, Guid> leadRepository,
        IUnitOfWork unitOfWork,
        IEmailService emailService,
        IMarketingSettingsProvider marketingSettings)
    {
        _leadRepository = leadRepository;
        _unitOfWork = unitOfWork;
        _emailService = emailService;
        _marketingSettings = marketingSettings;
    }

    public async Task<Result<Guid>> Handle(CreateLeadCommand request, CancellationToken ct)
    {
        var isSpam = IsSpamSubmission(request);

        var lead = new Lead
        {
            Id = Guid.NewGuid(),
            FullName = request.FullName,
            AcademyName = request.AcademyName,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            City = request.City,
            BranchCount = request.BranchCount,
            TraineeCountBand = request.TraineeCountBand,
            Message = request.Message,
            Locale = request.Locale,
            SourcePage = request.SourcePage,
            UtmSource = request.UtmSource,
            UtmMedium = request.UtmMedium,
            UtmCampaign = request.UtmCampaign,
            Referrer = request.Referrer,
            IpHash = request.IpHash,
            Status = isSpam ? LeadStatus.Spam : LeadStatus.New,
            CreatedAt = DateTime.UtcNow,
        };

        await _leadRepository.AddAsync(lead, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        // Never let a real lead's submission fail on an email hiccup - the row is already
        // saved, and a bot never reaches this branch (isSpam short-circuits above).
        if (!isSpam)
        {
            try
            {
                await SendNotificationEmailAsync(lead, ct);
                await SendAcknowledgementEmailAsync(lead, ct);
            }
            catch
            {
                // Swallow: IEmailService implementations already log/queue their own failures
                // (see FileLoggingEmailServiceDecorator) - a Resend outage must not turn an
                // otherwise-successful lead submission into a 500 for the visitor.
            }
        }

        // The response is intentionally identical whether the submission was real or flagged
        // as spam - a bot (or a human testing the honeypot) never learns which happened.
        return Result<Guid>.Success(lead.Id, _operation, "Lead submitted.", 201);
    }

    private static bool IsSpamSubmission(CreateLeadCommand request)
    {
        if (!string.IsNullOrWhiteSpace(request.HoneypotValue))
            return true;

        if (request.FormRenderedAtUtc is { } renderedAt &&
            DateTime.UtcNow - renderedAt < MinHumanFillTime)
            return true;

        return false;
    }

    private Task SendNotificationEmailAsync(Lead lead, CancellationToken ct)
    {
        var body = EmailTemplate.Render(
            preheader: $"New demo request from {lead.AcademyName}",
            heading: "New lead from the public site",
            bodyHtml: $"""
                <p><strong>{System.Net.WebUtility.HtmlEncode(lead.FullName)}</strong> at <strong>{System.Net.WebUtility.HtmlEncode(lead.AcademyName)}</strong> requested a demo.</p>
                <p>
                  Email: {System.Net.WebUtility.HtmlEncode(lead.Email)}<br/>
                  Phone: {System.Net.WebUtility.HtmlEncode(lead.PhoneNumber)}<br/>
                  City: {System.Net.WebUtility.HtmlEncode(lead.City ?? "-")}<br/>
                  Branches: {lead.BranchCount?.ToString() ?? "-"}<br/>
                  Source: {System.Net.WebUtility.HtmlEncode(lead.SourcePage ?? "-")}
                </p>
                <p>{System.Net.WebUtility.HtmlEncode(lead.Message ?? "")}</p>
                """,
            footerNote: "Reply directly to the lead's email, or open this lead in the Platform console.");

        return _emailService.SendAsync(_marketingSettings.SalesInboxEmail, $"New lead: {lead.AcademyName}", body, ct);
    }

    private Task SendAcknowledgementEmailAsync(Lead lead, CancellationToken ct)
    {
        var isArabic = lead.Locale == "ar";

        var body = EmailTemplate.Render(
            preheader: isArabic ? "شكرًا لتواصلك مع أورا" : "Thanks for reaching out to AURA",
            heading: isArabic ? "تم استلام طلبك" : "We've received your request",
            bodyHtml: isArabic
                ? $"<p>شكرًا لك {System.Net.WebUtility.HtmlEncode(lead.FullName)}، سيتواصل معك فريق أورا خلال يوم عمل واحد.</p>"
                : $"<p>Thanks, {System.Net.WebUtility.HtmlEncode(lead.FullName)} - the AURA team will reach out within one business day.</p>");

        var subject = isArabic ? "تم استلام طلبك - أورا أكاديمي" : "We've received your request - AURA Academy";
        return _emailService.SendAsync(lead.Email, subject, body, ct);
    }
}
