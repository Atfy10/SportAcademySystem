using Microsoft.EntityFrameworkCore;
using SportAcademy.Application.Common.Pagination;
using System.Linq.Expressions;
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

    public static PagedData<T> ToPagedData<T>(
        this IEnumerable<T> enumerable,
        PageRequest page)
    {
        var total = enumerable.ToList().Count;

        var items = enumerable
            .Skip((page.Page - 1) * page.PageSize)
            .Take(page.PageSize)
            .ToList();

        return new PagedData<T>
        {
            Items = items,
            TotalCount = total,
            Page = page.Page,
            PageSize = page.PageSize
        };
    }

    public static async Task<PagedData<TResult>> ToGroupedPagedDataAsync<TSource, TKey, TElement, TResult>(
        this IQueryable<TSource> query, 
        PageRequest page, 
        Expression<Func<TSource, TKey>> keySelector, 
        Expression<Func<TSource, TElement>> elementSelector, 
        Func<TKey, List<TElement>, TResult> resultSelector, 
        CancellationToken ct = default)
    { 
        // total rows count (قبل grouping)
        var totalCount = await query.CountAsync(ct);

        // DB pagination
        var pageData = await query
            .Skip((page.Page - 1) * page.PageSize)
            .Take(page.PageSize)
            .ToListAsync(ct);

        // in-memory grouping
        var compiledKey = keySelector.Compile(); 

        var compiledElement = elementSelector.Compile(); 

        var grouped = pageData
            .GroupBy(compiledKey)
            .Select(g => resultSelector(g.Key, g.Select(compiledElement)
                .ToList())
            )
            .ToList(); 

        return new PagedData<TResult> 
        { 
            Items = grouped, 
            TotalCount = totalCount, 
            Page = page.Page, 
            PageSize = page.PageSize 
        };
    }


}

