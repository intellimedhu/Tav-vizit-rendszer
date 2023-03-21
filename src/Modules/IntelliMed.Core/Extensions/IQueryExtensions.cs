using OrchardCore.ContentManagement.Records;

namespace YesSql
{
    public static class IQueryExtensions
    {
        public static IQuery<T, ContentItemIndex> LatestAndPublished<T>(this IQuery<T> query) where T : class =>
            query.With<ContentItemIndex>(x => x.Latest && x.Published);

        public static IQuery<T> Slice<T>(this IQuery<T> query, int? offset = null, int? limit = null) where T : class
        {
            if (offset.HasValue && offset > 0)
            {
                query = query.Skip(offset.Value);
            }

            if (limit.HasValue && limit > 0)
            {
                query = query.Take(limit.Value);
            }

            return query;
        }
    }
}
