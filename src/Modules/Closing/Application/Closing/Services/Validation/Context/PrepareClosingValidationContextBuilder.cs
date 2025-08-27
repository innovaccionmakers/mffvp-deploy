
using Closing.Application.Closing.Services.Validation.Dto;

namespace Closing.Application.Closing.Services.Validation.Context;

public sealed class PrepareClosingValidationContextBuilder
{
    private readonly PrepareClosingValidationContext context = new();

    public PrepareClosingValidationContextBuilder WithClosingDate(DateTime closingDate)
    {
        context.ClosingDate = closingDate;
        return this;
    }

    public PrepareClosingValidationContextBuilder WithIsFirstClosingDay(bool isFirstClosingDay)
    {
        context.IsFirstClosingDay = isFirstClosingDay;
        return this;
    }

    public PrepareClosingValidationContextBuilder WithConfigFlags(ConfigFlags flags)
    {
        context.ConfigFlags = flags;
        return this;
    }

    public PrepareClosingValidationContext Build() => context;
}