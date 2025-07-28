namespace Security.Application.Abstractions.Services.Auditing;

public interface IClientInfoService
{
    string GetClientIpAddress();
    string GetUserName();
    string GetActionDescription();
}