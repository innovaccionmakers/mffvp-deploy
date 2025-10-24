namespace Common.SharedKernel.Application.Abstractions;

public interface IUserService
{
    string GetUserName();
    string GetUserId();
    bool IsAuthenticated();
}
