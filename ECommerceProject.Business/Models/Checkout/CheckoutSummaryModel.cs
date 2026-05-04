namespace ECommerceProject.Business.Models.Checkout;

public class CheckoutSummaryModel
{
    public List<CheckoutItemModel> Items { get; set; } = new();

    public List<CheckoutAddressModel> SavedAddresses { get; set; } = new();

    public List<CheckoutCardModel> SavedCards { get; set; } = new();

}
