using System.Linq.Expressions;

namespace Medusa.Enap.Common.Core.Utils;

/// <summary>
/// Can be used as "using static ExpressionUtils" making the utils more readable
/// </summary>
public static class ExpressionUtils
{
    public static Expression<Func<T, bool>> Not<T>(Expression<Func<T, bool>> expression)
    {
        var parameter = expression.Parameters[0];
        var body = Expression.Not(expression.Body);
        return Expression.Lambda<Func<T, bool>>(body, parameter);
    }

    public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> expr1, Expression<Func<T, bool>> expr2)
    {
        var param = Expression.Parameter(typeof(T));

        var body = Expression.AndAlso(
            Expression.Invoke(expr1, param),
            Expression.Invoke(expr2, param));

        return Expression.Lambda<Func<T, bool>>(body, param);
    }

    public static Expression<Func<T, bool>> Or<T>(
        this Expression<Func<T, bool>> left,
        Expression<Func<T, bool>> right)
    {
        var param = left.Parameters[0];
        var rightBody = ReplaceParameter(right.Parameters[0], param, right.Body);
        return Expression.Lambda<Func<T, bool>>(
            Expression.OrElse(left.Body, rightBody),
            param);
    }

    public static Expression<Func<TOuter, bool>> ApplyTo<TOuter, TInner>(
        this Expression<Func<TInner, bool>> inner,
        Expression<Func<TOuter, TInner>> selector)
    {
        var param = selector.Parameters[0];
        var body = ReplaceParameter(inner.Parameters[0], selector.Body, inner.Body);

        return Expression.Lambda<Func<TOuter, bool>>(body, param);
    }

    /// <summary>
    /// Turns a (TParam => TResult) function expression into a (TParent => TResult) function expression
    /// based on (TParent => TParam) selector
    /// </summary>
    public static Expression<Func<TParent, TResult>> ReplaceParam<TParent, TParam, TResult>(
       this Expression<Func<TParam, TResult>> condition,
       Expression<Func<TParent, TParam>> selector)
    {
        var param = selector.Parameters[0];
        var body = ReplaceParameter(condition.Parameters[0], selector.Body, condition.Body);
        return Expression.Lambda<Func<TParent, TResult>>(body!, param);
    }

    private static Expression ReplaceParameter(
            Expression source,
            Expression target,
            Expression body)
    {
        return new ReplaceExpressionVisitor(source, target).Visit(body)!;
    }

    private sealed class ReplaceExpressionVisitor(Expression oldExpr, Expression newExpr) : ExpressionVisitor
    {
        public override Expression? Visit(Expression? node)
        {
            if (node == oldExpr)
                return newExpr;

            return base.Visit(node);
        }
    }
}