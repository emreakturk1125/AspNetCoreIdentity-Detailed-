using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace NetCoreIdentity.Web.ViewModels
{
    public class LoginViewModel
    {
        [Display(Name = "Mail Adresiniz")]
        [Required(ErrorMessage = "Mail alanı gereklidir.")] 
        [EmailAddress(ErrorMessage = "Bu mail adresi geçersizdir.")]
        public string Email { get; set; }

        [Display(Name = "Şifreniz")]
        [Required(ErrorMessage = "Şifre alanı gereklidir.")]
        [DataType(DataType.Password)]
        [MinLength(4,ErrorMessage = "Şifreniz en az 4 karakterli olmalıdır")]
        public string Password { get; set; }

        [Display(Name = "Beni Hatırla")]
        public bool RememberMe { get; set; }
    }
}
