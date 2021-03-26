using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace MongoDB.EntityFramework.Core
{
    public interface IDocumentQueryable<TEntity>
    {
        Task<List<TEntity>> ToListAsync();

        Task<List<TEntity>> ToListAsync(Expression<Func<TEntity, bool>> predicate);

        Task<List<TEntity>> ToListAsync(PagedOptions<TEntity> pagedOptions);

        Task<List<TEntity>> ToListAsync(Expression<Func<TEntity, bool>> predicate, PagedOptions<TEntity> pagedOptions);

        Task<TEntity> FirstOrDefaultAsync();

        Task<TEntity> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate);

        IDocumentQueryable<TEntity> Skip(int count);

        IDocumentQueryable<TEntity> Take(int count);

        IDocumentQueryable<TEntity> Limit(int count);

        IDocumentQueryable<TEntity> Where(Expression<Func<TEntity, bool>> predicate);

        IOrderedQueryable<TEntity> OrderBy(Expression<Func<TEntity, object>> fieldSelector);

        IOrderedQueryable<TEntity> OrderByDescending(Expression<Func<TEntity, object>> fieldSelector);

        IDocumentQueryable<TEntity> AsNoTracking();
    }

    public interface IOrderedQueryable<TEntity> : IDocumentQueryable<TEntity>
    {
        IOrderedQueryable<TEntity> ThenBy(Expression<Func<TEntity, object>> fieldSelector);

        IOrderedQueryable<TEntity> ThenByDescending(Expression<Func<TEntity, object>> fieldSelector);
    }

    public class PagedOptions<TEntity>
    {
        internal PagedOptions()
        {
        }

        public int Page { get; internal set; }

        public int PageSize { get; internal set; }

        public long Total { get; internal set; }

        public bool ComputeTotal { get; internal set; }

        public string FieldName { get; set; }

        public object FieldValue { get; set; }

        public bool GreaterThanFieldValue { get; set; }

        public int SkipCount => (Page - 1) * PageSize;

        public int LimitCount => PageSize;

        public PagedMode Mode => ClassicMode ? PagedMode.Classic : ByRangeMode ? PagedMode.ByRange : PagedMode.None;

        private bool ClassicMode => Page > 0 && PageSize > 0;

        private bool ByRangeMode => PageSize > 0 && FieldValue != null && !string.IsNullOrWhiteSpace(FieldName);
    }

    public enum PagedMode
    {
        None,
        Classic,
        ByRange,
    }

    public static class Paged
    {
        public static PagedOptions<TEntity> BuildPagedOptions<TEntity>(int page, int pageSize)
        {
            return new PagedOptions<TEntity> { Page = page, PageSize = pageSize, ComputeTotal = false };
        }

        public static PagedOptions<TEntity> BuildPagedOptions<TEntity>(int page, int pageSize, bool computeTotal)
        {
            return new PagedOptions<TEntity> { Page = page, PageSize = pageSize, ComputeTotal = computeTotal };
        }

        public static PagedOptions<TEntity> BuildNextRangeOptions<TEntity>(int pageSize, string fieldName = null, object lastFieldValue = null)
        {
            return new PagedOptions<TEntity> { PageSize = pageSize, FieldName = fieldName, FieldValue = lastFieldValue, GreaterThanFieldValue = true };
        }

        public static PagedOptions<TEntity> BuildPreviusRangeOptions<TEntity>(int pageSize, string fieldName, object firstFieldValue)
        {
            return new PagedOptions<TEntity> { PageSize = pageSize, FieldName = fieldName, FieldValue = firstFieldValue, GreaterThanFieldValue = false };
        }
    }
}