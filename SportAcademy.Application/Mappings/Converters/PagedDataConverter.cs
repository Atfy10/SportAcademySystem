using AutoMapper;
using SportAcademy.Application.Common.Pagination;

namespace Application.Common.Mapping.Converters;

public class PagedDataConverter<TSource, TDestination>
    : ITypeConverter<PagedData<TSource>, PagedData<TDestination>>
{
    public PagedData<TDestination> Convert(
        PagedData<TSource> source,
        PagedData<TDestination> destination,
        ResolutionContext context)
    {
        return new PagedData<TDestination>
        {
            Items = context.Mapper.Map<IReadOnlyCollection<TDestination>>(source.Items),
            TotalCount = source.TotalCount,
            Page = source.Page,
            PageSize = source.PageSize
        };
    }
}