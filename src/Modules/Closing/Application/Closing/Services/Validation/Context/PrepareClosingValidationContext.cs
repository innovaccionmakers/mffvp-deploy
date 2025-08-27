using Closing.Application.Closing.Services.Validation.Dto;

namespace Closing.Application.Closing.Services.Validation.Context;

public sealed class PrepareClosingValidationContext
{
    public DateTime ClosingDate { get; internal set; }
    public DateTime CurrentDate { get; internal set; }
    public bool IsFirstClosingDay { get; internal set; }
    public ConfigFlags ConfigFlags { get; internal set; }
}