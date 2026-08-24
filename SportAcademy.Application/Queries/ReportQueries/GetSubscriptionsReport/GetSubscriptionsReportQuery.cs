using MediatR;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.SubscriptionDetailsDtos;

namespace SportAcademy.Application.Queries.ReportQueries.GetSubscriptionsReport;

public record GetSubscriptionsReportQuery(
    DateTime? From, DateTime? To, int? BranchId, int? SportId, string? Status, PageRequest? Page)
    : IRequest<Result<PagedData<SubscriptionDetailsDto>>>;
