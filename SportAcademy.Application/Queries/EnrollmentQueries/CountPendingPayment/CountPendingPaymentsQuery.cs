using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Application.Queries.EnrollmentQueries.CountPendingPayment;

public record CountPendingPaymentsQuery : IRequest<Result<int>>;
