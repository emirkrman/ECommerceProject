using System.ComponentModel.DataAnnotations;

namespace ECommerceProject.Web.ViewModels.Account;

public class RegisterViewModel
{
    [Required]
    [StringLength(100, MinimumLength = 3)]
    [Display(Name = "Ad Soyad")]
    public string FullName { get; set; } = null!;

    [Required]
    [EmailAddress]
    [StringLength(150)]
    [Display(Name = "E-posta")]
    public string Email { get; set; } = null!;

    [Required]
    [StringLength(100, MinimumLength = 6)]
    [DataType(DataType.Password)]
    [Display(Name = "Sifre")]
    public string Password { get; set; } = null!;

    [Required]
    [DataType(DataType.Password)]
    [Compare(nameof(Password))]
    [Display(Name = "Sifre Tekrar")]
    public string ConfirmPassword { get; set; } = null!;
}
