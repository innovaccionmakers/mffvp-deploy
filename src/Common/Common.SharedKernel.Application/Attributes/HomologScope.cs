using System.Linq.Expressions;
using System.Reflection;

namespace Common.SharedKernel.Application.Attributes;

public static class HomologScope
{
    public static string Of<TCommand>(
        Expression<Func<TCommand, object?>> propertySelector)
        where TCommand : class
    {
        var propertyAccess = propertySelector.Body switch
        {
            MemberExpression direct => direct,
            UnaryExpression { Operand: MemberExpression boxed } => boxed,
            _ => throw new ArgumentException(
                "La expresión debe ser un acceso directo a propiedad (p => p.Prop).",
                nameof(propertySelector))
        };

        var scopeAttribute = propertyAccess.Member
            .GetCustomAttribute<HomologScopeAttribute>(false);

        return scopeAttribute?.Scope
               ?? throw new InvalidOperationException(
                   $"La propiedad '{propertyAccess.Member.Name}' no tiene HomologScopeAttribute.");
    }
}