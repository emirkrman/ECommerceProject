using System.ComponentModel.DataAnnotations;

namespace ECommerceProject.Web.ViewModels.Account;

public class LoginViewModel
{
    [Required]
    [EmailAddress]
    [Display(Name = "E-posta")]
    public string Email { get; set; } = null!;

    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "Sifre")]
    public string Password { get; set; } = null!;

    [Display(Name = "Beni hatirla")]
    public bool RememberMe { get; set; }
}
