using ECommerceProject.Entity.Common;

namespace ECommerceProject.Entity.Concrete;

public class UserPaymentCard : BaseEntity
{
    public int UserId { get; set; }

    public AppUser? User { get; set; }

    public string CardHolderName { get; set; } = null!;

    public string CardNumber { get; set; } = null!;

    public string ExpiryMonth { get; set; } = null!;

    public string ExpiryYear { get; set; } = null!;

    public string Cvv { get; set; } = null!;

    public string CardLastFour { get; set; } = null!;
}
