namespace Common.Core.Identity;

public interface IIdentityContext
{
    bool IsAuthenticated { get; }
    string? AuthToken { get; }
    int? UserId { get; }
    string? UserName { get; }
    string? Audience { get; }
    bool IsSysAdmin { get; }
}