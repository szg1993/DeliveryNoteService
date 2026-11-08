using System.Linq.Expressions;

namespace Medusa.Enap.Common.Core.Persistence;

public interface INavigationPropertyValidator
{
    /// <summary>
    /// Returns true if the entity is tracked and has a remote counterpart in the DB.
    /// Returns false if the entity is newly created or otherwise detached.
    /// </summary>
    bool IsTracked<TModel>(TModel model)
        where TModel : class;

    /// <summary>
    /// Returns true if the navigation property <paramref name="prop"/> is loaded, i.e. included in the query.
    /// Returns false if not loaded.
    /// </summary>
    bool IsLoaded<TModel, TProp>(TModel model, Expression<Func<TModel, TProp?>> prop)
        where TModel : class
        where TProp : class;

    /// <summary>
    /// Returns true if the navigation property <paramref name="prop"/> is loaded, i.e. included in the query.
    /// Returns false if not loaded or just partially loaded.
    /// </summary>
    bool IsCollectionLoaded<TModel, TProp>(TModel model, Expression<Func<TModel, IEnumerable<TProp>>> prop)
        where TModel : class
        where TProp : class;
}

public static class NavigationPropertyValidatorExtensions
{
    /// <summary>
    /// Throws if the parent model <paramref name="model"/> is loaded
    /// but the navigation property <paramref name="prop"/> is not.
    /// <br/><br/>
    /// See <see cref="INavigationPropertyValidator.IsTracked{TModel}(TModel)"/>><br/>
    /// See <see cref="INavigationPropertyValidator.IsLoaded{TModel, TProp}(TModel, Expression{Func{TModel, TProp?}})"/>
    /// </summary>
    /// <exception cref="InvalidOperationException">Property <paramref name="prop"/> must be loaded.</exception>
    /// <returns>The property value or null if the property is optional and loaded and has no value.</returns>
    public static TProp? ValidateLoaded<TModel, TProp>(this INavigationPropertyValidator validator,
        TModel model, Expression<Func<TModel, TProp?>> prop)
        where TModel : class
        where TProp : class
    {
        if (validator.IsTracked(model) && !validator.IsLoaded(model, prop))
            throw new InvalidOperationException($"{GetPropertyName(prop)} must be loaded");

        return prop.Compile().Invoke(model);
    }

    /// <summary>
    /// Throws if the parent model <paramref name="model"/> is loaded
    /// but the navigation property <paramref name="prop"/> is not.
    /// <br/><br/>
    /// See <see cref="INavigationPropertyValidator.IsTracked{TModel}(TModel)"/>><br/>
    /// See <see cref="INavigationPropertyValidator.IsCollectionLoaded{TModel, TProp}(TModel, Expression{Func{TModel, IEnumerable{TProp}}})"/>
    /// </summary>
    /// <exception cref="InvalidOperationException">Property <paramref name="prop"/> must be loaded.</exception>
    /// <returns>The collection property value.</returns>
    public static IEnumerable<TProp> ValidateCollectionLoaded<TModel, TProp>(this INavigationPropertyValidator validator,
        TModel model, Expression<Func<TModel, IEnumerable<TProp>>> prop)
        where TModel : class
        where TProp : class
    {
        if (validator.IsTracked(model) && !validator.IsCollectionLoaded(model, prop))
            throw new InvalidOperationException($"{GetPropertyName(prop)} must be loaded");

        return prop.Compile().Invoke(model);
    }

    private static string GetPropertyName<TModel, TProp>(
        Expression<Func<TModel, TProp>> expression)
    {
        if (expression.Body is not MemberExpression member)
            throw new ArgumentException("Expected property access.");

        return $"{expression.Parameters[0].Type.Name}.{member.Member.Name}";
    }
}
