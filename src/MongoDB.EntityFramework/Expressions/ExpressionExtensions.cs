using System;
using System.Linq.Expressions;

namespace MongoDB.EntityFramework.Expressions
{
    public static class ExpressionExtensions
    {
        public static Expression<Func<TEntity, bool>> AndAlso<TEntity>(this Expression<Func<TEntity, bool>> left, Expression<Func<TEntity, bool>> right)
        {
            var rightBody = new ExpressionParameterReplacer(right.Parameters, left.Parameters).Visit(right.Body);
            var body = Expression.AndAlso(left.Body, rightBody);

            var combined = Expression.Lambda<Func<TEntity, bool>>(body, left.Parameters);

            return combined;
        }

        public static Expression<Func<TEntity, bool>> AndAlso<TEntity>(this Expression<Func<TEntity, bool>>[] predicates)
        {
            if (predicates is null || predicates.Length == 0)
            {
                return null;
            }

            var predicate = predicates[0];

            for (int i = 1; i < predicates.Length; i++)
            {
                predicate = predicate.AndAlso(predicates[i]);
            }

            return predicate;
        }

        public static Expression<Func<TEntity, bool>> OrElse<TEntity>(this Expression<Func<TEntity, bool>> left, Expression<Func<TEntity, bool>> right)
        {
            var rightBody = new ExpressionParameterReplacer(right.Parameters, left.Parameters).Visit(right.Body);
            var body = Expression.OrElse(left.Body, rightBody);

            var combined = Expression.Lambda<Func<TEntity, bool>>(body, left.Parameters);

            return combined;
        }

        public static Expression<Func<TEntity, bool>> OrElse<TEntity>(this Expression<Func<TEntity, bool>>[] predicates)
        {
            if (predicates is null || predicates.Length == 0)
            {
                return null;
            }

            var predicate = predicates[0];

            for (int i = 1; i < predicates.Length; i++)
            {
                predicate = predicate.OrElse(predicates[i]);
            }

            return predicate;
        }
    }
}