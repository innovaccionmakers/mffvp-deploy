using HotChocolate.Resolvers;
using Operations.Presentation.GraphQL.Inputs;

namespace MFFVP.BFF.Middlewares;

public class CompleteRequestCreateContributionMiddleware
{
    private readonly FieldDelegate _next;

    public CompleteRequestCreateContributionMiddleware(FieldDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(IMiddlewareContext context)
    {
        var input = context.ArgumentValue<CreateContributionInput>("aporte");

        var executionDate = input.ExecutionDate ?? new DateTime();
        var salesUser = input.SalesUser ?? "Makers";
        var channel = input.Channel ?? "Makers";

        input.ExecutionDate = executionDate;
        input.SalesUser = salesUser;
        input.Channel = channel;

        // No need to call SetArgumentValue; input object is already updated

        await _next(context);
    }
}
