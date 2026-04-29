namespace ECommerceProject.Business.Models.Carts;

public class CartOperationResult
{
    private CartOperationResult(bool succeeded, string? message)
    {
        Succeeded = succeeded;
        Message = message;
    }

    public bool Succeeded { get; }

    public string? Message { get; }

    public static CartOperationResult Success(string? message = null)
    {
        return new CartOperationResult(true, message);
    }

    public static CartOperationResult Failure(string message)
    {
        return new CartOperationResult(false, message);
    }
}
