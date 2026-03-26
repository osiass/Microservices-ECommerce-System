using System.ComponentModel.DataAnnotations;

namespace Common.DTOs;

public class UserRegisterDto
{
    [Required(ErrorMessage = "Kullanıcı adı zorunludur.")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Kullanıcı adı 3-50 karakter arasında olmalıdır.")]
    public string UserName { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Eposta zorunludur.")]
    [EmailAddress(ErrorMessage = "Geçersiz eposta adresi.")]
    public string Email { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Şifre zorunludur.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Şifre en az 6 karakter olmalıdır.")]
    public string Password { get; set; } = string.Empty;
}