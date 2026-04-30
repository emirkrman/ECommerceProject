namespace ECommerceProject.Business.Models.Checkout;

public class CheckoutAddressModel
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public string PhoneNumber { get; set; } = null!;

    public string City { get; set; } = null!;

    public string District { get; set; } = null!;

    public string AddressLine { get; set; } = null!;
}
