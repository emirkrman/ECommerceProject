using System.ComponentModel.DataAnnotations;

namespace ECommerceProject.Web.ViewModels.Checkout;

public class CheckoutViewModel
{
    public List<CheckoutItemViewModel> Items { get; set; } = new();

    public List<CheckoutAddressViewModel> SavedAddresses { get; set; } = new();

    public List<CheckoutCardViewModel> SavedCards { get; set; } = new();

    public decimal CartTotal => Items.Sum(item => item.Subtotal);

    public int? SelectedAddressId { get; set; }

    public int? SelectedCardId { get; set; }

    [Display(Name = "Kartımı Kaydet")]
    public bool SaveCard { get; set; }

    [Display(Name = "Adres Başlığı")]
    public string? AddressTitle { get; set; }

    [Display(Name = "Ad Soyad")]
    public string? FullName { get; set; }

    [Display(Name = "Telefon")]
    public string? PhoneNumber { get; set; }

    [Display(Name = "Şehir")]
    public string? City { get; set; }

    [Display(Name = "İlçe")]
    public string? District { get; set; }

    [Display(Name = "Adres")]
    public string? AddressLine { get; set; }

    [Display(Name = "Kart Üzerindeki İsim")]
    public string? CardHolderName { get; set; }

    [RegularExpression(@"^\d{4}[- ]?\d{4}[- ]?\d{4}[- ]?\d{4}$",
        ErrorMessage = "Kart numarası 16 haneli olmalıdır.")]
    [Display(Name = "Kart Numarası")]
    public string? CardNumber { get; set; }

    [RegularExpression(@"^(0[1-9]|1[0-2])$", ErrorMessage = "Ay 01-12 arasında 2 haneli olmalıdır.")]
    [Display(Name = "Ay")]
    public string? ExpiryMonth { get; set; }

    [RegularExpression(@"^\d{4}$", ErrorMessage = "Yıl 4 haneli olmalıdır.")]
    [Display(Name = "Yıl")]
    public string? ExpiryYear { get; set; }

    [RegularExpression(@"^\d{3}$", ErrorMessage = "CVV 3 haneli olmalıdır.")]
    [Display(Name = "CVV")]
    public string? Cvv { get; set; }
}
