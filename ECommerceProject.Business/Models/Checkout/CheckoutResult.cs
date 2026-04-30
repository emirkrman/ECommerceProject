namespace ECommerceProject.Business.Models.Checkout;

public class CheckoutResult
{
    private CheckoutResult(bool succeeded, int? orderId, string message)
    {
        Succeeded = succeeded;
        OrderId = orderId;
        Message = message;
    }

    public bool Succeeded { get; }

    public int? OrderId { get; }

    public string Message { get; }

    public static CheckoutResult Success(int orderId, string message)
    {
        return new CheckoutResult(true, orderId, message);
    }

    public static CheckoutResult Failure(string message, int? orderId = null)
    {
        return new CheckoutResult(false, orderId, message);
    }
}
