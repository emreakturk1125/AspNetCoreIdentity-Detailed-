using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetCoreIdentity.Web.Requirements
{
    // Startup.cs de tanımlama yapılması gerekir.
    public class ExpireDateExchangeRequirements : IAuthorizationRequirement
    {
    }

    public class ExpireDateExchangeHandle : AuthorizationHandler<ExpireDateExchangeRequirements>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ExpireDateExchangeRequirements requirement)
        {
            if (context.User != null & context.User.Identity != null)
            {
                var claim = context.User.Claims.Where(x => x.Type == "ExpireDateExchange" && x.Value != null).FirstOrDefault();
                if (claim != null)
                {
                    if (DateTime.Now < Convert.ToDateTime(claim.Value))
                    {
                        context.Succeed(requirement);    // Şartı sağlarsa Authorization işlemi başarılı
                    }
                    else
                    {
                        context.Fail();                 // Şartı sağlamaz ise Authorization işlemi başarısız ve AccessDenied'a yönlendirecek
                    }
                }
            }
            return Task.CompletedTask;
        }
    }
}
