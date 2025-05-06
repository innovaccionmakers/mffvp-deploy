using Common.SharedKernel.Domain;
using Common.SharedKernel.Presentation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using FluentValidation;

namespace Common.SharedKernel.Presentation.Filters
{
    public sealed class TechnicalValidationFilter<TRequest> : IEndpointFilter
    where TRequest : class
    {
        public async ValueTask<object?> InvokeAsync(
            EndpointFilterInvocationContext ctx,
            EndpointFilterDelegate next)
        {
            var validator = ctx.HttpContext.RequestServices.GetService<IValidator<TRequest>>();
            if (validator is null)
                return await next(ctx);

            var request = ctx.Arguments.OfType<TRequest>().FirstOrDefault();
            if (request is null)
                return await next(ctx);

            var validation = await validator.ValidateAsync(request);
            if (validation.IsValid)
                return await next(ctx);

            var errors = validation.Errors
                .Select(e => Error.Validation($"Validation.{e.PropertyName}", e.ErrorMessage))
                .ToArray();

            var error = new ValidationError(errors);

            var failure = Result.Failure(error);

            return ApiResults.Problem(failure);
        }
    }
}
