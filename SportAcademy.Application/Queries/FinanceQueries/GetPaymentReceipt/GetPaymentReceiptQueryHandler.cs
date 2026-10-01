using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.FinanceDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.BaseExceptions;

namespace SportAcademy.Application.Queries.FinanceQueries.GetPaymentReceipt;

public class GetPaymentReceiptQueryHandler : IRequestHandler<GetPaymentReceiptQuery, Result<PaymentReceiptDto>>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IUserRepository _userRepository;
    private readonly ICurrentLanguageProvider _languageProvider;
    private readonly string _operation = OperationType.Get.ToString();

    public GetPaymentReceiptQueryHandler(
        IPaymentRepository paymentRepository,
        IUserRepository userRepository,
        ICurrentLanguageProvider languageProvider)
    {
        _paymentRepository = paymentRepository;
        _userRepository = userRepository;
        _languageProvider = languageProvider;
    }

    public async Task<Result<PaymentReceiptDto>> Handle(GetPaymentReceiptQuery request, CancellationToken ct)
    {
        var payment = await _paymentRepository.GetForReceiptAsync(request.PaymentNumber, ct)
            ?? throw new IdNotFoundException("Payment", request.PaymentNumber);

        var lang = _languageProvider.Language;

        // Resolve every staff name on the receipt once (recorder + whoever refunded/voided).
        var userIds = payment.Refunds.Select(r => r.RefundedByUserId)
            .Append(payment.RecordedByUserId)
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .Distinct()
            .ToList();
        var names = new Dictionary<Guid, string>();
        foreach (var id in userIds)
            names[id] = await _userRepository.GetDisplayNameAsync(id, ct);

        var allocations = payment.Allocations
            .OrderBy(a => a.Id)
            .Select(a =>
            {
                var invoice = a.Invoice;
                var subscription = invoice.Lines
                    .Select(l => l.SubscriptionDetails)
                    .FirstOrDefault(sd => sd is not null);
                var sport = subscription?.SportPrice?.SportSubscriptionType?.Sport;
                var ev = invoice.Lines.Select(l => l.Event).FirstOrDefault(e => e is not null);

                return new PaymentReceiptAllocationDto(
                    a.InvoiceId,
                    invoice.InvoiceNumber,
                    a.Amount,
                    a.ReversedAmount,
                    invoice.GrandTotal,
                    invoice.DiscountTotal,
                    invoice.AmountPaid,
                    invoice.GrandTotal - invoice.AmountPaid,
                    invoice.DueDate,
                    sport is null ? null
                        : sport.Translations.Where(t => t.LangCode == lang).Select(t => t.Name).FirstOrDefault() ?? sport.Name,
                    subscription?.SportPrice?.SportSubscriptionType?.SubscriptionType?.Name.ToString(),
                    subscription?.StartDate,
                    subscription?.EndDate,
                    ev?.Id,
                    ev?.Title,
                    ev is null ? null : SportAcademy.Domain.Services.TenantCalendar.ToLocal(ev.StartsAt));
            })
            .ToList();

        var trainees = payment.Allocations
            .Select(a => a.Invoice.Trainee)
            .Where(t => t is not null)
            .DistinctBy(t => t!.Id)
            .Select(t => new PaymentReceiptTraineeDto(
                t!.Id,
                $"{t.FirstName} {t.LastName}",
                t.PhoneNumber,
                t.TraineeCode?.Value))
            .ToList();

        var payers = payment.Allocations
            .Select(a => a.Invoice)
            .Where(i => i.Trainee is null && !string.IsNullOrWhiteSpace(i.PayerName))
            .DistinctBy(i => (i.PayerName, i.PayerPhone))
            .Select(i => new PaymentReceiptPayerDto(i.PayerName!, i.PayerPhone))
            .ToList();

        var refunds = payment.Refunds
            .OrderBy(r => r.RefundedAt)
            .Select(r => new PaymentRefundDto(
                r.Id, r.Kind, r.Amount, r.Reason, r.RefundedAt,
                r.RefundedByUserId is { } by ? names.GetValueOrDefault(by) : null))
            .ToList();

        var dto = new PaymentReceiptDto(
            payment.PaymentNumber,
            payment.Amount,
            payment.RefundedAmount,
            payment.PaymentType.Translations.Where(t => t.LangCode == lang).Select(t => t.Name).FirstOrDefault()
                ?? payment.PaymentType.Name,
            payment.Status,
            payment.PaidDate,
            payment.Branch.Translations.Where(t => t.LangCode == lang).Select(t => t.Name).FirstOrDefault()
                ?? payment.Branch.Name,
            payment.Currency,
            payment.Reference,
            payment.Notes,
            allocations,
            trainees,
            payment.RecordedByUserId is { } recordedBy ? names.GetValueOrDefault(recordedBy) : null,
            refunds,
            payers);

        return Result<PaymentReceiptDto>.Success(dto, _operation);
    }
}
