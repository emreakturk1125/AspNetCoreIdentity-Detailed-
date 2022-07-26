using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NetCoreIdentity.Web.Models;
using NetCoreIdentity.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mapster;
using Microsoft.AspNetCore.Authorization;

namespace NetCoreIdentity.Web.Controllers
{
    [Authorize(Roles ="Admin")]
    public class AdminController : BaseController
    {  
        public AdminController(UserManager<AppUser> userManager,RoleManager<AppRole> roleManager) :base(null,userManager,null,roleManager)
        {
            _userManager = userManager;
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult RoleCreate()
        {
            return View();
        }

        [HttpPost]
        public IActionResult RoleCreate(RoleViewModel roleViewModel)
        {
            AppRole role = new AppRole();
            role.Name = roleViewModel.Name;

            IdentityResult result = _roleManager.CreateAsync(role).Result;

            if (result.Succeeded)
            {
                return RedirectToAction("Roles");
            }
            else
            {
                AddModelError(result);
            }

            return View();
        }

        public IActionResult Roles()
        {
            return View(_roleManager.Roles.ToList());
        }

        public IActionResult Users()
        {
            return View(_userManager.Users.ToList());
        }

        public IActionResult RoleDelete(string id)
        {
            AppRole role = _roleManager.FindByIdAsync(id).Result;
            if (role != null)
            {
                IdentityResult result = _roleManager.DeleteAsync(role).Result;
            } 
            return RedirectToAction("Roles");
        }

        [HttpGet]
        public IActionResult RoleUpdate(string id)
        {
            AppRole role = _roleManager.FindByIdAsync(id).Result;

            if (role != null)
            {
                return View(role.Adapt<RoleViewModel>());
            }

            return RedirectToAction("Roles");
        }

        [HttpPost]
        public IActionResult RoleUpdate(RoleViewModel roleViewModel)
        {
            AppRole role = _roleManager.FindByIdAsync(roleViewModel.Id).Result;
            role.Name = roleViewModel.Name;

            if (role != null)
            {
                IdentityResult result =  _roleManager.UpdateAsync(role).Result;
                if (result.Succeeded)
                {
                    return RedirectToAction("Roles");
                }
                else
                {
                    AddModelError(result);
                }
            }
            else
            {
                ModelState.AddModelError("", "Güncelleme işlemi başarısız.");
            }

            return View(roleViewModel);
        }

        [HttpGet]
        public IActionResult RoleAssign(string id)
        {
            TempData["UserId"] = id;
            AppUser user = _userManager.FindByIdAsync(id).Result;
            ViewBag.UserName = user.UserName;
            IQueryable<AppRole> roles = _roleManager.Roles;
            List<string> userRoles =   _userManager.GetRolesAsync(user).Result as List<string>;

            List<RoleAssignViewModel> roleAssignViewModels = new List<RoleAssignViewModel>();
            foreach (var role in roles)
            {
                RoleAssignViewModel roleModel = new RoleAssignViewModel();
                roleModel.RoleId = role.Id;
                roleModel.RoleName = role.Name;

                if (userRoles.Contains(role.ToString()))
                {
                    roleModel.Exist = true;
                }
                else
                {
                    roleModel.Exist = false;
                }

                roleAssignViewModels.Add(roleModel);
            }

            return View(roleAssignViewModels);
        }

        [HttpPost]
        public async Task<IActionResult> RoleAssign(List<RoleAssignViewModel> roleAssignViewModels)
        {
            AppUser user = _userManager.FindByIdAsync(TempData["UserId"].ToString()).Result;

            foreach (var role in roleAssignViewModels)
            { 
                if (role.Exist)
                {
                    await _userManager.AddToRoleAsync(user, role.RoleName);
                }
                else
                {
                    await _userManager.RemoveFromRoleAsync(user, role.RoleName);

                }
            }

            return RedirectToAction("Users");
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Claims()
        { 
            return View(User.Claims.ToList());
        }
    }
}
