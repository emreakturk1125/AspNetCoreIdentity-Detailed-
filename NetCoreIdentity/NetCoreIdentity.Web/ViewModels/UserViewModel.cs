using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace NetCoreIdentity.Web.ViewModels
{
    public class UserViewModel
    {
        [Required(ErrorMessage ="Kullanıcı Adı gereklidir.")]
        [Display(Name = "Kullanıcı Adı")]
        public string UserName { get; set; }

        [Display(Name = "Tel No")] 
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Email gereklidir.")]
        [Display(Name = "Mail")]
        [EmailAddress(ErrorMessage = "Email adresi doğru formatta değil")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Şifre gereklidir.")]
        [Display(Name = "Şifre")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Doğum Tarihi")]

        [DataType(DataType.Date)]
        public DateTime? Birthday { get; set; }

        [Display(Name = "Resim")]
        public string Picture { get; set; }

        [Display(Name = "Şehir")]
        public string City { get; set; }

        [Display(Name = "Cinsiyet")]
        public Gender Gender { get; set; }
    }
}
