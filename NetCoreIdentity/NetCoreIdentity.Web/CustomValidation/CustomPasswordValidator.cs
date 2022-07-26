using Microsoft.AspNetCore.Identity;
using NetCoreIdentity.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetCoreIdentity.Web.CustomValidation
{
    // CustomPasswordValidator için Startup.cs de ayarlamalar yapmak gerekiyor.
    public class CustomPasswordValidator : IPasswordValidator<AppUser>
    {
        public Task<IdentityResult> ValidateAsync(UserManager<AppUser> manager, AppUser user, string password)
        {
            List<IdentityError> errors = new List<IdentityError>();

            if (password.ToLower().Contains(user.UserName.ToLower()))  // şifre kullanıcı adı içermesin
            {
                if (!user.Email.ToLower().Contains(user.UserName.ToLower()))   
                {
                    errors.Add(new IdentityError() { Code = "PasswordContaisUsername", Description = "Şifre alanı kullanıcı adı içeremez" });
                }
            }
            if (password.ToLower().Contains("1234")) // şifre ardışık sayı içermesin
            {
                errors.Add(new IdentityError() { Code = "PasswordContais1234", Description = "Şifre alanı ardışık sayı içeremez" });
            }

            if (password.ToLower().Contains(user.Email.ToLower())) // şifre email içermesin
            {
                errors.Add(new IdentityError() { Code = "PasswordContaisEmail", Description = "Şifre alanı email içeremez" });
            }

            if (errors.Count == 0)
            {
                return Task.FromResult(IdentityResult.Success);
            }

            return Task.FromResult(IdentityResult.Failed(errors.ToArray()));


        }
    }
}
