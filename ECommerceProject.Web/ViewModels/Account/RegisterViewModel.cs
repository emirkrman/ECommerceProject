using System.ComponentModel.DataAnnotations;

namespace ECommerceProject.Web.ViewModels.Account;

public class RegisterViewModel
{
    [Required(ErrorMessage = "Ad soyad boş olamaz.")]
    [StringLength(100, MinimumLength = 3)]
    [Display(Name = "Ad Soyad")]
    public string FullName { get; set; } = null!;

    [Required(ErrorMessage = "E-posta boş olamaz.")]
    [EmailAddress]
    [StringLength(150)]
    [Display(Name = "E-posta")]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "Şifre boş olamaz.")]
    [StringLength(100, MinimumLength = 6)]
    [DataType(DataType.Password)]
    [Display(Name = "Şifre")]
    public string Password { get; set; } = null!;

    [Required(ErrorMessage = "Şifre tekrarı boş olamaz.")]
    [DataType(DataType.Password)]
    [Compare(nameof(Password))]
    [Display(Name = "Şifre Tekrar")]
    public string ConfirmPassword { get; set; } = null!;
}
