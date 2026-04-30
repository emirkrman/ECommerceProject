namespace ECommerceProject.Business.Models.Checkout;

public class CheckoutRequest
{
    public int? SelectedAddressId { get; set; }

    public int? SelectedCardId { get; set; }

    public bool SaveCard { get; set; }

    public string AddressTitle { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public string PhoneNumber { get; set; } = null!;

    public string City { get; set; } = null!;

    public string District { get; set; } = null!;

    public string AddressLine { get; set; } = null!;

    public string CardHolderName { get; set; } = null!;

    public string CardNumber { get; set; } = null!;

    public string ExpiryMonth { get; set; } = null!;

    public string ExpiryYear { get; set; } = null!;

    public string Cvv { get; set; } = null!;
}
