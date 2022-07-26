using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NetCoreIdentity.Web.Models;
using NetCoreIdentity.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mapster;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Security.Claims;

namespace NetCoreIdentity.Web.Controllers
{

    // Önemli : Eğer kullanıcı ile alakalı önemli bir bilgi değiştirildi ise şifre ya da kullanıcı adı gibi, Önemsiz ise gerek yoktur. 
    // "AspNetUsers" tablosundaki SecurityStamp kolonu da güncellenmelidir.
    // Örneğin mobil uygulama üzerinden şifre değiştirldi. Fakat Web üzerinden de eski şifre ile girişi yapıldıktan sonra cookie de eski SecurityStamp değeri  tutulduğu için
    // eski şifre ile giriş yapabilir bunu engellemek için "SecurityStamp" kolonuda güncellenmelidir. Çünkü Identity kütüphanesi SecurityStamp üzerinden kontrol yapar.
    // Default olarak 30 dk bir db deki SecurityStamp değer ile cookie deki SecurityStamp değeri karşılaştırılır. 3, dk boyunca eski şifre ile giriş yapabilmesini engellemek için update edilmelidir.
    // Çünkü veritanındaki SecurityStamp ile  Cookie de tutulan SecurityStamp değeri aynı olmalıdır
    // Ayrıca kullanıcı birden fazla şifre yenileme talabinde bulunmuş olabilir. önceki şifre yenileme linklerini üzeerinde işlem yapılmasını engellemek için gereklidir.
    // çünkü şifre yenileme linki ile gödnerilen token değeri içinde SecurityStamp değeri de bulunur

    [Authorize]
    public class MemberController : BaseController
    {   
        public MemberController(ILogger<HomeController> logger, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager):base(logger, userManager,signInManager,null)
        { 
        }

        public IActionResult Index()
        {
            // User.Identity.AuthenticationType => Kullanıcı nasıl üye oldu onunla alakalı bilgiyi tutar
            // User.Identity  kimlik kartı gibidir. Her kullanıcı için oluşur. Kullanıcı login olduğu zaman otomatik bilgiler dolar
            AppUser user = CurrentUser; 

            UserViewModel userViewModel = user.Adapt<UserViewModel>();  // Mapster kütüphanesi ile otomatik mapping işlemi

            return View(userViewModel);
        }

        public IActionResult PasswordChange()
        { 

            return View();
        }

        [HttpPost]
        public IActionResult PasswordChange(PasswordChangeViewModel passwordChangeViewModel)
        {

            AppUser user = CurrentUser;

            if (user != null)
            {
                bool exist = _userManager.CheckPasswordAsync(user, passwordChangeViewModel.PasswordOld).Result;
                if (exist)
                {
                    IdentityResult result = _userManager.ChangePasswordAsync(user, passwordChangeViewModel.PasswordOld, passwordChangeViewModel.PasswordNew).Result;

                    if (result.Succeeded)
                    {
                        _userManager.UpdateSecurityStampAsync(user);  // Kullanıcı adı veya şifre değiştirildi ise  UpdateSecurityStampAsync mutlaka çalıştırılmalıdır. Eski bilgilerle giriş yapılmasını engellemek için

                        // Çıkış yaptırıp 
                        _signInManager.SignOutAsync();
                        // Tekrar giriş yaptırdık. Kullanıcıyı login sayfasına yönlendirmeden Cookie arka planda tekrar oluşacak. BU şekilde yapmasaydık, kütüphane otomatik olarak bir süre sonra login sayfasına yönlendirecekti.
                        _signInManager.PasswordSignInAsync(user, passwordChangeViewModel.PasswordNew, true, false);    
                         
                        ViewBag.Success = "true";
                    }
                    else
                    {
                        AddModelError(result);
                    }

                }
                else
                {
                    ModelState.AddModelError("", "Eski şifreniz yanlış");

                }
            }

            return View(passwordChangeViewModel);
        }

        [HttpGet]
        public IActionResult UserEdit()
        {
            AppUser user = CurrentUser;

            UserViewModel userViewModel = user.Adapt<UserViewModel>();

            ViewBag.Gender = new SelectList(Enum.GetNames(typeof(Gender)));

            return View(userViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> UserEdit(UserViewModel userViewModel, IFormFile userPicture)
        {
            ModelState.Remove("Password"); // UserViewModel de Password lanı var fakat View tarafında bu alan boş geliyor. ModelState'e takılmaması için Remove ediyoruz
           
            ViewBag.Gender = new SelectList(Enum.GetNames(typeof(Gender)));

            if (ModelState.IsValid)
            {
                AppUser user = CurrentUser;

                if (userPicture != null && userPicture.Length > 0)
                {
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(userPicture.FileName);
                    var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Picture/", fileName);
                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        await userPicture.CopyToAsync(stream);
                        user.Picture = "/Picture/" + fileName;
                    }
                }

                user.UserName = userViewModel.UserName;
                user.Email = userViewModel.Email;
                user.PhoneNumber = userViewModel.PhoneNumber;
                user.City = userViewModel.City;
                user.Birthday = userViewModel.Birthday;
                user.Gender = (int)userViewModel.Gender;

                IdentityResult result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    await _userManager.UpdateSecurityStampAsync(user);   
                    await _signInManager.SignOutAsync();
                    await _signInManager.SignInAsync(user, true);
                    ViewBag.Success = "true";
                }
                else
                {
                    AddModelError(result);
                }

            }


            return View(userViewModel);
        }

        public void LogOut()
        {
            // Startup.cs dosyasında tanımlama yaptık     opt.LogoutPath = new PathString("/Member/LogOut");      
            // view kısımında da Yönlendireceğimiz sayfayı "asp-route-returnUrl = "/Home/Index""  şekilde tanımladık.
            _signInManager.SignOutAsync();
             
        }

        public IActionResult AccessDenied(string returnUrl)
        {
            if (returnUrl.ToLower().Contains("violancepage"))
            {
                ViewBag.Message = "18 Yaşında küçük olduğunuz için, şiddet içeren bu sayfaya erişemezsiniz.";
            }
            else if (returnUrl.ToLower().Contains("ankarapage"))
            {
                ViewBag.Message = "Bu sayfaya şehiri ankara olanlar erişebilir.";
            }
            else if (returnUrl.ToLower().Contains("exchangepage"))
            {
                ViewBag.Message = "30 günlük ücretsiz deneme hakkınız sona ermiştir.";
            }
            else 
            {
                ViewBag.Message = "Bu sayfaya erişim izniniz yoktur. Erişim izni almak için site yöneticisi ile görüşünüz.";
            }
            return View();
        }

        [Authorize(Roles = "Editor,Admin")]
        public IActionResult Editor()
        {
            return View();
        }

        [Authorize(Roles = "Manager,Admin")]
        public IActionResult Manager()
        {
            return View();
        }

        // Claim Bazlı Yetkilendirme (Şehiri Ankara olan kişiler bu sayfayı görsün.
        // ClaimProvider.cs de ayarlamalar yaptık.
        // Startup.cs de ayarlama yaptık.

        [Authorize(Policy= "AnkaraPolicy")]
        public IActionResult AnkaraPage()
        {
            return View();
        }

        [Authorize(Policy = "ViolancePolicy")]
        public IActionResult ViolancePage()
        {
            return View();
        }

        // Belirli bir süreliğine kullanıcıya sayfayı göster, süre dolunca gösterme
        public async Task<IActionResult> ExchangeRedirect()
        {
            bool result = User.HasClaim(c => c.Type == "ExpireDateExchange");
            if (!result)
            {
                Claim expireDateExchange = new Claim("ExpireDateExchange", DateTime.Now.AddDays(30).Date.ToShortDateString(),ClaimValueTypes.String,"Internal");
                await _userManager.AddClaimAsync(CurrentUser,expireDateExchange);
                await _signInManager.SignOutAsync();
                await _signInManager.SignInAsync(CurrentUser, true);
            }
            return RedirectToAction("ExchangePage");
        }

        [Authorize(Policy = "ExchangePolicy")]
        public IActionResult ExchangePage()
        {
            return View();
        }
    }
}
