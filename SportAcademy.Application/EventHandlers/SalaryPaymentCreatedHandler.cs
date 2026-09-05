using MediatR;
using SportAcademy.Application.Events;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Helpers;

namespace SportAcademy.Application.EventHandlers;

public sealed class SalaryPaymentCreatedHandler(
    INotificationService notificationService,
    ISalaryPaymentRepository salaryPaymentRepository)
    : INotificationHandler<SalaryPaymentCreatedEvent>
{
    public async Task Handle(SalaryPaymentCreatedEvent notification, CancellationToken cancellationToken)
    {
        var payment = await salaryPaymentRepository.GetByIdWithIncludesAsync(notification.SalaryPaymentId, cancellationToken);
        if (payment is null) return;

        var employeeName = payment.Employee is not null
            ? $"{payment.Employee.FirstName} {payment.Employee.LastName}"
            : "an employee";
        var branchName = payment.Branch?.Name ?? "a branch";

        // Only whoever can actually approve it - Salary.Approve is deliberately withheld from
        // Accountant (the usual filer) so they can't approve their own request. See the
        // comment on SalaryPayment for why.
        await notificationService.SendNotificationToGroupsAsync(
            [NotificationGroupNames.Admins, NotificationGroupNames.Owners],
            "New Salary Payment Request",
            $"A salary payment of {payment.Amount + payment.Bonus} {payment.Currency} for {employeeName} ({branchName}) needs approval",
            NotificationType.Info);
    }
}
