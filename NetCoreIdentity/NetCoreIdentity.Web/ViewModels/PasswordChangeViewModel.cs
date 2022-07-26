using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace NetCoreIdentity.Web.ViewModels
{
    public class PasswordChangeViewModel
    {
        [Display(Name = "Eski Şifreniz")]
        [Required(ErrorMessage = "Eski şifreniz gereklidir")]
        [DataType(DataType.Password)]
        [MinLength(4,ErrorMessage = "Şifreniz en az 4 karakterli olmalıdır.")]
        public string PasswordOld { get; set; }

        [Display(Name = "Yeni Şifreniz")]
        [Required(ErrorMessage = "Yeni şifreniz gereklidir")]
        [DataType(DataType.Password)]
        [MinLength(4, ErrorMessage = "Şifreniz en az 4 karakterli olmalıdır.")]
        public string PasswordNew { get; set; }

        [Display(Name = "Tekrar Yeni Şifreniz")]
        [Required(ErrorMessage = "Yeni şifreniz tekrar gereklidir")]
        [DataType(DataType.Password)]
        [MinLength(4, ErrorMessage = "Şifreniz en az 4 karakterli olmalıdır.")]
        [Compare("PasswordNew",ErrorMessage = "Yeni şifreniz ve onay şifreniz birbirinden farklıdır. ")]
        public string PasswordConfirm { get; set; }
    }
}
