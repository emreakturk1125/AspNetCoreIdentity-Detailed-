using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using NetCoreIdentity.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace NetCoreIdentity.Web.ClaimProvider
{
    // Claim Bazlı yetkilendirme
    // Bu sınıfın çalışması için Startup.cs de de düzenleme yapılması gerekir. 
    // Identity kütüphanesi kullanıcı bilgilerini claim'a  atar
    public class ClaimProvider : IClaimsTransformation
    {
        public UserManager<AppUser> UserManager { get; set; }
        public ClaimProvider(UserManager<AppUser> userManager)
        {
            this.UserManager = userManager;
        }
        public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
        {
            if (principal != null && principal.Identity.IsAuthenticated)
            {
                ClaimsIdentity identity = principal.Identity as ClaimsIdentity;
                AppUser user = await UserManager.FindByNameAsync(identity.Name);
                if (user != null)
                {
                    if (user.Birthday != null)
                    {
                        var today = DateTime.Today;
                        var age = today.Year - user.Birthday?.Year;
                        if (age > 18)
                        {
                            // Startup.cs de de düzenleme yapılması gerekir. 
                            if (!principal.HasClaim(c => c.Type == "violance"))
                            {
                                Claim violanceClaim = new Claim("violance",true.ToString(), ClaimValueTypes.String, "Internal");
                                identity.AddClaim(violanceClaim);
                            }
                        }
                    }


                    if (user.City != null)
                    {
                        // Startup.cs de de düzenleme yapılması gerekir. 
                        if (!principal.HasClaim(c => c.Type == "city"))
                        {
                            Claim cityClaim = new Claim("city", user.City, ClaimValueTypes.String, "Internal");
                            identity.AddClaim(cityClaim);
                        }
                    }
                }
            }

            return principal;
        }
    }
}
