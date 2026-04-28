using System.ComponentModel.DataAnnotations;

namespace ECommerceProject.Web.ViewModels.Account;

public class RegisterViewModel
{
    [Required(ErrorMessage = "Ad Soyad bos olamaz.")]
    [StringLength(100, MinimumLength = 3)]
    [Display(Name = "Ad Soyad")]
    public string FullName { get; set; } = null!;

    [Required(ErrorMessage = "E-posta bos olamaz.")]
    [EmailAddress]
    [StringLength(150)]
    [Display(Name = "E-posta")]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "Sifre bos olamaz.")]
    [StringLength(100, MinimumLength = 6)]
    [DataType(DataType.Password)]
    [Display(Name = "Sifre")]
    public string Password { get; set; } = null!;

    [Required(ErrorMessage = "Sifre Tekrar bos olamaz.")]
    [DataType(DataType.Password)]
    [Compare(nameof(Password))]
    [Display(Name = "Sifre Tekrar")]
    public string ConfirmPassword { get; set; } = null!;
}
