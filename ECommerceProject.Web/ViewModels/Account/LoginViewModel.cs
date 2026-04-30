using System.ComponentModel.DataAnnotations;

namespace ECommerceProject.Web.ViewModels.Account;

public class LoginViewModel
{
    [Required(ErrorMessage = "E-posta boş olamaz.")]
    [EmailAddress]
    [Display(Name = "E-posta")]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "Şifre boş olamaz.")]
    [DataType(DataType.Password)]
    [Display(Name = "Şifre")]
    public string Password { get; set; } = null!;

    [Display(Name = "Beni hatırla")]
    public bool RememberMe { get; set; }
}
