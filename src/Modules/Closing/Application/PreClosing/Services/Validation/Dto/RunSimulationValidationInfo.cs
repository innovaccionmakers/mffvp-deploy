
namespace Closing.Application.PreClosing.Services.Validation.Dto;

public sealed record RunSimulationValidationInfo(
       bool IsFirstClosingDay,
       FirstDayStateResult FirstDayState 
   );
