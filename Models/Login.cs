using System.ComponentModel.DataAnnotations;

namespace BoardingHouseApp.Models
{
    public class LoginModel
    {
        [Required(ErrorMessage = "Tên đăng nhập hoặc Email là bắt buộc")]
        [Display(Name = "Tên đăng nhập hoặc Email")]
        public string? Username { get; set; }

        [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string? Password { get; set; }

        [Display(Name = "Ghi nhớ đăng nhập")]
        public bool RememberMe { get; set; }
    }
}