using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetCoreIdentity.Web.CustomValidation
{
    // CustomIdentityErrorDescriber için Startup.cs de ayarlamalar yapmak gerekiyor.

    // İstediğin validation işlemini override ederek türkçeleştirebilirsin
    public class CustomIdentityErrorDescriber : IdentityErrorDescriber
    {
        public override IdentityError InvalidUserName(string userName)
        {
            return new IdentityError
            {
                Code = "InvalidUserName",
                Description = $"Bu kullanıcı adı({userName}) geçersizdir."
            };
        }

        public override IdentityError DuplicateEmail(string email)
        {
            return new IdentityError
            {
                Code = "DuplicateEmail",
                Description = $"Bu mail adresi({email}) kullanılmaktadır."
            };
        }

        public override IdentityError DuplicateRoleName(string role)
        {
            return new IdentityError
            {
                Code = "DuplicateRoleName",
                Description = $"Bu rol adı({role}) kullanılmaktadır."
            };
        }

        public override IdentityError PasswordTooShort(int length)
        {
            return new IdentityError
            {
                Code = "PasswordTooShort",
                Description = $"Şifrenizde en az {length} karakter olmalıdır"
            };
        }

        public override IdentityError DuplicateUserName(string userName)
        {
            return new IdentityError
            {
                Code = "InvalidUserName",
                Description = $"Bu kullanıcı adı({userName}) zaten kullanılmaktadır."
            };
        }

       
    }
}
