namespace ECommerceProject.Business.Models.Orders;

public class OrderAddressModel
{
    public string Title { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public string PhoneNumber { get; set; } = null!;

    public string City { get; set; } = null!;

    public string District { get; set; } = null!;

    public string AddressLine { get; set; } = null!;
}
