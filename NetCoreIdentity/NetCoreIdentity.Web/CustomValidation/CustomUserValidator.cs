using Microsoft.AspNetCore.Identity;
using NetCoreIdentity.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetCoreIdentity.Web.CustomValidation
{
    // CustomUserValidator için Startup.cs de ayarlamalar yapmak gerekiyor.

    public class CustomUserValidator : IUserValidator<AppUser>
    {
        public Task<IdentityResult> ValidateAsync(UserManager<AppUser> manager, AppUser user)
        {
            List<IdentityError> errors = new List<IdentityError>();

            List<string> Digits = new List<string> { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };
  
            if (Digits.Contains(user.UserName[0].ToString()))
            {
                errors.Add(new IdentityError() { Code = "UsernameContainsFirstLetterDigitContains", Description = "Kullanıcı adının ilk karakteri sayısal değer içeremez" });
            }

            if (errors.Count == 0)
            {
                return Task.FromResult(IdentityResult.Success);
            }

            return Task.FromResult(IdentityResult.Failed(errors.ToArray()));
        }
    }
}
