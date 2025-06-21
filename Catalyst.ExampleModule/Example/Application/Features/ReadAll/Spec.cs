using System.Linq.Expressions;
using Catalyst.Data.Abstraction;
using LinqKit;
using Microsoft.EntityFrameworkCore;

namespace Catalyst.ExampleModule.Example.Application.Features.ReadAll;

public class Spec : BaseSpecification<Domain.Models.ExampleEntity>
{
    public Spec(int pageNumber, int pageSize, string? searchTerm, Request.ExampleEntityFilters filters)
    {
        AddIncludes();
        ApplyOrderBy(d => d.Id);
        ApplyPaging((pageNumber - 1) * pageSize, pageSize);
        DisableTracking();

        ApplySearchCondition(searchTerm);
        ApplyFilterCondition(filters);
    }

    public Spec(string? searchTerm, Request.ExampleEntityFilters filters)
    {
        AddIncludes();
        DisableTracking();

        ApplySearchCondition(searchTerm);
        ApplyFilterCondition(filters);
    }

    private void ApplySearchCondition(string? searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return;

        var textTerm = searchTerm.Trim();
        var predicate = PredicateBuilder.New<Domain.Models.ExampleEntity>(false);

        Expression<Func<Domain.Models.ExampleEntity, string?>>[] textProps =
        [
            d => d.Type.Name,
            d => d.SerialNumber,
        ];
        predicate = textProps.Aggregate(predicate, (current, prop) =>
            current.Or(BuildSafeLike(prop, $"%{textTerm}%")));

        // if (MacAddress.IsMac(textTerm)) // Assuming MacAddress has such a utility
        // {
        //     predicate = predicate.Or(d =>
        //         EF.Functions.ILike(d.MacAddress.Value, $"%{new MacAddress(textTerm).Value}%"));
        // }


        if (predicate.IsStarted)
            ApplyCriteria(predicate);
    }

    private void ApplyFilterCondition(Request.ExampleEntityFilters f)
    {
        // Changed to filter by StopIds using Contains on ExampleEntity.StopId
        if (f.TypeIds?.Any() ?? false)
            ApplyCriteria(d => f.TypeIds.Contains(d.TypeId));

        if (f.GeneralStatus?.Any() ?? false)
            ApplyCriteria(d => f.GeneralStatus.Contains(((byte)d.GeneralStatus).ToString()));
    }

    private void AddIncludes()
    {
        AddInclude(d => d.Type);
    }

    // Helper for building ILike expressions safely, especially for navigated properties.
    // Ensures EF Core can translate it to SQL.
    private Expression<Func<Domain.Models.ExampleEntity, bool>> BuildSafeLike(
        Expression<Func<Domain.Models.ExampleEntity, string?>> propSelector,
        string pattern)
    {
        // Parameter from the original expression (e.g., 'd' from d => d.Stop.Name)
        var parameter = propSelector.Parameters.Single();
        // Body of the original expression (e.g., d.Stop.Name)
        var propertyAccess = propSelector.Body;

        // Check if the propertyAccess itself can be null (e.g. string? ManufacturerName)
        // or if it's a navigation property that can be null (e.g., d.Stop might be null)
        Expression? nullCheck = null;
        var currentExpression = propertyAccess;

        // Traverse up the expression tree for navigation properties (e.g., d.Stop.Name -> d.Stop)
        while (currentExpression is MemberExpression memberExpression &&
               memberExpression.Expression != parameter) // Stop when we reach the root parameter 'd'
        {
            var notNullExpression = Expression.NotEqual(memberExpression.Expression!, Expression.Constant(null));
            nullCheck = nullCheck == null ? notNullExpression : Expression.AndAlso(notNullExpression, nullCheck);
            currentExpression = memberExpression.Expression;
        }

        // Final null check for the property itself if it's directly on ExampleEntity or the last part of navigation
        var finalPropertyNullCheck =
            Expression.NotEqual(propertyAccess, Expression.Constant(null, propertyAccess.Type));
        nullCheck = nullCheck == null ? finalPropertyNullCheck : Expression.AndAlso(nullCheck, finalPropertyNullCheck);

        // The ILike call
        var functionsProperty = Expression.Property(null, typeof(EF), nameof(EF.Functions));
        var ilikeCall = Expression.Call(
            typeof(NpgsqlDbFunctionsExtensions), // Or RelationalDbFunctionsExtensions for general EF Core
            nameof(NpgsqlDbFunctionsExtensions.ILike), // or "ILike" if using a specific provider extension method
            null, // No generic type arguments
            functionsProperty,
            propertyAccess,
            Expression.Constant(pattern)
        );

        // Combine null check with ILike: (nullCheck == null || (nullCheck && ilikeCall))
        // More robust: if nullCheck is not null, then (nullCheck AND ilikeCall), else just ilikeCall
        var finalExpressionBody = nullCheck == null ? (Expression)ilikeCall : Expression.AndAlso(nullCheck, ilikeCall);

        return Expression.Lambda<Func<Domain.Models.ExampleEntity, bool>>(finalExpressionBody, parameter);
    }
}