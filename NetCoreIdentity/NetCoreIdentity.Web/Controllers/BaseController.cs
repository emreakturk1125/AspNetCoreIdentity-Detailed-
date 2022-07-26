using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NetCoreIdentity.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetCoreIdentity.Web.Controllers
{
    // protected : bu class'ı miras alanda kullanabilsin demektir.
    public class BaseController : Controller
    {
        protected readonly ILogger<HomeController> _logger;
        protected UserManager<AppUser> _userManager;                                                    // Identity kütüphanesinden otomatik gelen class
        protected SignInManager<AppUser> _signInManager;                                                // Identity kütüphanesinden otomatik gelen class
        protected RoleManager<AppRole> _roleManager;                                                    // Identity kütüphanesinden otomatik gelen class
        protected AppUser CurrentUser => _userManager.FindByNameAsync(User.Identity.Name).Result;

        public BaseController(ILogger<HomeController> logger, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, RoleManager<AppRole> roleManager)
        {
            _logger = logger;
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }

        public void AddModelError(IdentityResult result)
        {
            foreach (var item in result.Errors)
            {
                ModelState.AddModelError("", item.Description);
            }
        }

    }
}
