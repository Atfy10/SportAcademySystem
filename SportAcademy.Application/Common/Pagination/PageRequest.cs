namespace SportAcademy.Application.Common.Pagination
{
    public sealed class PageRequest
    {
        public const int MaxPageSize = 100;
        public const int DefaultPageSize = 10;

        public int Page { get; }
        public int PageSize { get; }
        public int Skip => (Page - 1) * PageSize;

        private PageRequest(int page, int size)
        {
            Page = page;
            PageSize = size;
        }

        public static PageRequest Create(int? page, int? size)
        {
            var validPage = (page ?? 0) < 1 ? 1 : page!.Value;

            var validSize = size switch
            {
                null => DefaultPageSize,
                <= 0 => DefaultPageSize,
                > MaxPageSize => MaxPageSize,
                _ => size.Value,
            };

            return new PageRequest(validPage, validSize);
        }
    }
}
