using System.ComponentModel.DataAnnotations;

namespace ECommerceProject.Web.ViewModels.Account;

public class LoginViewModel
{
    [Required(ErrorMessage = "E-posta bos olamaz.")]
    [EmailAddress]
    [Display(Name = "E-posta")]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "Sifre bos olamaz.")]
    [DataType(DataType.Password)]
    [Display(Name = "Sifre")]
    public string Password { get; set; } = null!;

    [Display(Name = "Beni hatirla")]
    public bool RememberMe { get; set; }
}
