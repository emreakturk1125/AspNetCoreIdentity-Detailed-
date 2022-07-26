using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NetCoreIdentity.Web.Models;
using NetCoreIdentity.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace NetCoreIdentity.Web.Controllers
{

    // Yazdığınız Attribute lar üzerinden Client Side Validation yapılmasını istiyorsan
    // Client Side Validation için "jquery-validation" ve "jquery-validation-unobtrusive" kütüphanesi eklemelidir.
    // Proje'ye Sağ Tık > Add > Client-Side Library  => "jquery-validate" & "jquery-validation-unobtrusive" 
    public class HomeController : BaseController   // BaseControllerdan miras aldık
    {
        public HomeController(ILogger<HomeController> logger, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager) : base(logger,userManager,signInManager,null)
        { 
        }

        public IActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Member");

            }

            return View();
        }

        [HttpGet]
        public IActionResult Login(string returnUrl)
        {
            TempData["ReturnUrl"] = returnUrl;            // Login olmadan sayfayalara giriş yapmaya çalışırsa, sayfaya yönlendirme linkini geçici olarak tut demektir.
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel userLogin)
         { 

            if (ModelState.IsValid)
            {
                AppUser user = await _userManager.FindByEmailAsync(userLogin.Email);

                if (user != null)
                {

                    if (await _userManager.IsLockedOutAsync(user))
                    {
                        ModelState.AddModelError("", "Hesabınız bir süreliğine kilitlenmiştir.");
                        return View(userLogin);
                    }

                    if (!_userManager.IsEmailConfirmedAsync(user).Result)
                    {
                        ModelState.AddModelError("", "Email adresiniz onaylanmamıştır. Lütfen e-posta adresinizi kontrol ediniz.");
                        return View(userLogin);
                    }

                    // Kullanıcı tekrar Login olacağı için Kullanıcı ile ilgili cookie varsa sil demektir. Yani çıkış yapılıyor
                    await _signInManager.SignOutAsync();
                    // Tekrardan giriş yapılıyor, userLogin.RememberMe değerine göre ya session süresi boyunca ya da startup dosyasında ayarladığımı cookie süresi geçerli olacak
                    Microsoft.AspNetCore.Identity.SignInResult result = await _signInManager.PasswordSignInAsync(user, userLogin.Password, userLogin.RememberMe, false);  

                    if (result.Succeeded)
                    {
                        await _userManager.ResetAccessFailedCountAsync(user);                           // Başarılı giriş yaptığında, daha önceki başarısız giriş sayısını sıfırla demektir.

                        if (TempData["ReturnUrl"] != null)                                              // Geçici olarak tutulan yönlendirme linki
                        {
                            return Redirect(TempData["ReturnUrl"].ToString());
                        }

                        return RedirectToAction("Index", "Member");
                    }
                    else
                    {
                        await _userManager.AccessFailedAsync(user);                                      // Başarılı bir giriş yapamadıysa, başarısız giriş sayısını arttır. Üst üste yanlış parola girmiş olabilir.
                         
                        int failEnteringCount = await _userManager.GetAccessFailedCountAsync(user);
                        ModelState.AddModelError("", $"{failEnteringCount} kez başarısız giriş.");


                        if (failEnteringCount == 3)                                                      // 3 Defa başarısız giriş yaptı ise 20 dk giriş yapmasını engelle
                        {
                            await _userManager.SetLockoutEndDateAsync(user, new DateTimeOffset(DateTime.Now.AddMinutes(20)));
                            ModelState.AddModelError("", "Hesabınız 3 başarısız girişten dolayı 20 dakika süreliğine kilitlenmiştir.");
                        }
                        else
                        {
                            ModelState.AddModelError("", "Kullanıcı adresiniz ya da şifreniz yanlış.");
                        }

                    }

                }
                else
                {
                    ModelState.AddModelError("", "Bu email adresine kayıtlı kullanıcı bulunamamıştır.");
                }

            }

            return View(userLogin);
        }

        [HttpGet]
        public IActionResult SignUp()
        {
            return View();
        }

        // Yazdığınız Attribute lar üzerinden Client Side Validation yapılmasını istiyorsan
        // Client Side Validation için "jquery-validation" ve "jquery-validation-unobtrusive" kütüphanesi eklemelidir.
        // Proje'ye Sağ Tık > Add > Client-Side Library  => "jquery-validate" & "jquery-validation-unobtrusive" 

        [HttpPost]
        public async Task<IActionResult> SignUp(UserViewModel userViewModel)
        {
            if (ModelState.IsValid)
            {
                AppUser user = new AppUser
                {
                    UserName = userViewModel.UserName,
                    Email = userViewModel.Email,
                    PhoneNumber = userViewModel.PhoneNumber
                };
                IdentityResult result = await _userManager.CreateAsync(user, userViewModel.Password);

                if (result.Succeeded)
                {
                    string confirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    string link = Url.Action("ConfirmEmail", "Home", new
                    {
                        userId = user.Id,
                        token = confirmationToken,
                    },protocol:HttpContext.Request.Scheme);

                    Helper.EmailConfirmation.SendEmail(link, user.Email);

                    ViewBag.Success = "true";
                }

                if (!result.Succeeded)
                {
                    AddModelError(result);
                }
                else
                {
                    return RedirectToAction("Login");

                }

            }
            return View(userViewModel);
        }

        [HttpGet]
        public IActionResult ResetPassword()
        {
            return View();
        }
        [HttpPost]
        public IActionResult ResetPassword(PasswordResetViewModel passwordResetViewModel)
        {
            AppUser user = _userManager.FindByEmailAsync(passwordResetViewModel.Email).Result;

            if (user != null)
            {
                // içinde user bilgilerinin olduğu bir token oluşturulur
                string passwordResetToken = _userManager.GeneratePasswordResetTokenAsync(user).Result;   // Startup.cs de AddDefaultTokenProviders() eklenmelidir.

                // mail adresine gönderilecek link
                string passwordResetLink = Url.Action("ResetPasswordConfirm","Home",new { userId = user.Id, token = passwordResetToken },HttpContext.Request.Scheme);

                Helper.PasswordReset.PasswordResetSendEmail(passwordResetLink,user.Email);

                ViewBag.Status = "success";  // başarılı ise view tarafında mesajı göstermek için
                ViewBag.Message = "Şifre yenileme linki e-posta adresinize gönderilmiştir.";

            }
            else
            {
                ModelState.AddModelError("", "Sistemde kayıtlı email adresi bulunamamıştır.");
            }

            return View(passwordResetViewModel);
        }

        [HttpGet]
        public IActionResult ResetPasswordConfirm(string userId, string token)
        {
            TempData["userId"] = userId;
            TempData["token"] = token;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ResetPasswordConfirm([Bind("PasswordNew,PasswordNew")]PasswordResetViewModel passwordResetViewModel)
        {
            string token;
            string userId;
            try
            {
                 token = TempData["token"].ToString();
                 userId = TempData["userId"].ToString();
            }
            catch (Exception)
            {
                return RedirectToAction("Login");
            }

            AppUser user = await _userManager.FindByIdAsync(userId);

            if (user != null)
            {
                var result = await _userManager.ResetPasswordAsync(user, token, passwordResetViewModel.PasswordNew);

                if (result.Succeeded)
                {
                    // Önemli : Eğer kullanıcı ile alakalı önemli bir bilgi değiştirildi ise şifre ya da kullanıcı adı gibi, Önemsiz ise gerek yoktur. 
                    // "AspNetUsers" tablosundaki SecurityStamp kolonu da güncellenmelidir.
                    // Örneğin mobil uygulama üzerinden şifre değiştirldi. Fakat Web üzerinden de eski şifre ile girişi yapıldıktan sonra cookie de eski SecurityStamp değeri  tutulduğu için
                    // eski şifre ile giriş yapabilir bunu engellemek için "SecurityStamp" kolonuda güncellenmelidir. Çünkü Identity kütüphanesi SecurityStamp üzerinden kontrol yapar
                    // Default olarak 30 dk bir db deki SecurityStamp değer ile cookie deki SecurityStamp değeri karşılaştırılır. 3, dk boyunca eski şifre ile giriş yapabilmesini engellemek için update edilmelidir.
                    // Çünkü veritanındaki SecurityStamp ile  Cookie de tutulan SecurityStamp değeri aynı olmalıdır
                    // Ayrıca kullanıcı birden fazla şifre yenileme talabinde bulunmuş olabilir. önceki şifre yenileme linklerini üzeerinde işlem yapılmasını engellemek için gereklidir.
                    // çünkü şifre yenileme linki ile gödnerilen token değeri içinde SecurityStamp değeri de bulunur

                    await _userManager.UpdateSecurityStampAsync(user);
                     
                    ViewBag.Status = "success";  // başarılı ise view tarafında mesajı göstermek için
                    ViewBag.Message = "Şifreniz başarıyla yenilenmiştir. Yeni şifreniz ile giriş yapabilirsiniz.";

                }
                else
                {
                    //foreach (var item in result.Errors)
                    //{
                        ModelState.AddModelError("", "Şifre yenileme linki geçersizdir. Lütfen şifrenizi tekrardan yenileyiniz");
                    //}
                }
            }
            else
            {
                ModelState.AddModelError("", "Hata lütfen daha sonra tekrar deneyiniz.");

            }

            return View(passwordResetViewModel);
        }

        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId);
            IdentityResult result = await _userManager.ConfirmEmailAsync(user, token);
            if (result.Succeeded)
            {
                ViewBag.Status = "Email adresiniz onaylanmıştır. Login ekranından giriş yapabilirsiniz.";
            }
            else
            {
                ViewBag.Status = "Bir hata meydana geldi. Lütfen daha sonra tekrar deneyiniz.";

            }
            return View();
        }
         
        public IActionResult FacebookLogin(string ReturnUrl)

        {
            string RedirectUrl = Url.Action("ExternalResponse", "Home", new { ReturnUrl = ReturnUrl });

            var properties = _signInManager.ConfigureExternalAuthenticationProperties("Facebook", RedirectUrl);

            return new ChallengeResult("Facebook", properties);
        }

        public IActionResult GoogleLogin(string ReturnUrl)

        {
            string RedirectUrl = Url.Action("ExternalResponse", "Home", new { ReturnUrl = ReturnUrl });

            var properties = _signInManager.ConfigureExternalAuthenticationProperties("Google", RedirectUrl);

            return new ChallengeResult("Google", properties);
        }

        public IActionResult MicrosoftLogin(string ReturnUrl)

        {
            string RedirectUrl = Url.Action("ExternalResponse", "Home", new { ReturnUrl = ReturnUrl });

            var properties = _signInManager.ConfigureExternalAuthenticationProperties("Microsoft", RedirectUrl);

            return new ChallengeResult("Microsoft", properties);
        }

        public async Task<IActionResult> ExternalResponse(string ReturnUrl = "/")
        {
            
            ExternalLoginInfo info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return RedirectToAction("LogIn");
            }
            else
            {
                Microsoft.AspNetCore.Identity.SignInResult result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, true);

                if (result.Succeeded)
                {
                    return Redirect(ReturnUrl);
                }
                else
                {
                    AppUser user = new AppUser();

                    user.Email = info.Principal.FindFirst(ClaimTypes.Email).Value;
                    string ExternalUserId = info.Principal.FindFirst(ClaimTypes.NameIdentifier).Value;

                    if (info.Principal.HasClaim(x => x.Type == ClaimTypes.Name))
                    {
                        string userName = info.Principal.FindFirst(ClaimTypes.Name).Value;

                        userName = userName.Replace(' ', '-').ToLower() + ExternalUserId.Substring(0, 5).ToString();

                        user.UserName = userName;
                    }
                    else
                    {
                        user.UserName = info.Principal.FindFirst(ClaimTypes.Email).Value;
                    }

                    AppUser user2 = await _userManager.FindByEmailAsync(user.Email);

                    if (user2 == null)
                    {
                        IdentityResult createResult = await _userManager.CreateAsync(user);

                        if (createResult.Succeeded)
                        {
                            IdentityResult loginResult = await _userManager.AddLoginAsync(user, info);

                            if (loginResult.Succeeded)
                            {
                                //     await _signInManager.SignInAsync(user, true);

                                await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, true);

                                return Redirect(ReturnUrl);
                            }
                            else
                            {
                                AddModelError(loginResult);
                            }
                        }
                        else
                        {
                            AddModelError(createResult);
                        }
                    }
                    else
                    {
                        IdentityResult loginResult = await _userManager.AddLoginAsync(user2, info);

                        await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, true);

                        return Redirect(ReturnUrl);
                    }
                }
            }

            List<string> errors = ModelState.Values.SelectMany(x => x.Errors).Select(y => y.ErrorMessage).ToList();

            return View("Error", errors);
        }

        public ActionResult Error()
        {
            return View();

        }
    }
}
