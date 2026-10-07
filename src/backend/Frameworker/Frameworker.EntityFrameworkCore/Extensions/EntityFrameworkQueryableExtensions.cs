using Frameworker.EntityFrameworkCore.Impl;

namespace Frameworker.EntityFrameworkCore.Extensions;

public static class EntityFrameworkQueryableExtensions
{
    public static async Task<PagedList<T>> ToPagedList<T>(this IQueryable<T> queryable, int pageIndex, int pageSize)
    {
        var count = await queryable.CountAsync();
        var items = await queryable.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
        return new PagedList<T>(items, pageIndex, pageSize, count);
    }
}