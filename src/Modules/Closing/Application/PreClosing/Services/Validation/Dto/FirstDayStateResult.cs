using Common.SharedKernel.Domain;

namespace Closing.Application.PreClosing.Services.Validation.Dto;

public sealed record FirstDayStateResult(
    bool IsFirstDay,
    bool HasPandL,
    bool HasTreasury,
    bool HasClientOps,
    bool HasInitialFundUnitValue,
    bool IsInitialFundUnitValueValid,
    decimal? InitialFundUnitValue,
    Result Failure);