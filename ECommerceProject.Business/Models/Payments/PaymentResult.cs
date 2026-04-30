namespace ECommerceProject.Business.Models.Payments;

public class PaymentResult
{
    private PaymentResult(bool succeeded, string message)
    {
        Succeeded = succeeded;
        Message = message;
    }

    public bool Succeeded { get; }

    public string Message { get; }

    public static PaymentResult Success(string message = "Ödeme başarılı.")
    {
        return new PaymentResult(true, message);
    }

    public static PaymentResult Failure(string message = "Ödeme başarısız.")
    {
        return new PaymentResult(false, message);
    }
}
