using ECommerceProject.Entity.Concrete;

namespace ECommerceProject.Business.Models.Accounts;

public class AccountResult
{
    private AccountResult(bool succeeded, AppUser? user, string? errorField, string? errorMessage)
    {
        Succeeded = succeeded;
        User = user;
        ErrorField = errorField;
        ErrorMessage = errorMessage;
    }

    public bool Succeeded { get; }
    public AppUser? User { get; }
    public string? ErrorField { get; }
    public string? ErrorMessage { get; }

    public static AccountResult Success(AppUser user)
    {
        return new AccountResult(true, user, null, null);
    }

    public static AccountResult Failure(string errorMessage, string? errorField = null)
    {
        return new AccountResult(false, null, errorField, errorMessage);
    }
}
