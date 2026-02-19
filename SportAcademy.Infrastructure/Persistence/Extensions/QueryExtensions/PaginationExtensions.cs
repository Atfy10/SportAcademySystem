using Microsoft.EntityFrameworkCore;
using SportAcademy.Application.Common.Pagination;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace SportAcademy.Infrastructure.Persistence.Extensions.QueryExtensions;

internal static class PaginationExtensions
{
    public async static Task<PagedData<T>> ToPagedDataAsync<T>(
        this IQueryable<T> query,
        PageRequest page,
        CancellationToken ct = default)
    {
        var total = await query.CountAsync(ct);

        var items = await query
            .Skip((page.Page - 1) * page.PageSize)
            .Take(page.PageSize)
            .ToListAsync(ct);

        return new PagedData<T>
        {
            Items = items,
            TotalCount = total,
            Page = page.Page,
            PageSize = page.PageSize
        };
    }
}

