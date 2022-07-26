using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NetCoreIdentity.Web.CustomValidation;
using NetCoreIdentity.Web.Models;
using NetCoreIdentity.Web.Requirements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetCoreIdentity.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
         
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<IAuthorizationHandler, ExpireDateExchangeHandle>();
            services.AddControllersWithViews();
            services.AddDbContext<AppIdentityDbContext>(opts =>
            {
                opts.UseSqlServer(Configuration["ConnectionStrings:DefaultConnectionString"]);
            });

            services.AddAuthorization(opts =>
            {
                opts.AddPolicy("AnkaraPolicy", policy =>                   
                {
                    policy.RequireClaim("city", "ankara"); // Claim bazlı yetkilendirme, Kullanıcının key : city, value : ankara değerine sahip olmalı 
                });
                opts.AddPolicy("ViolancePolicy", policy =>
                {
                    policy.RequireClaim("violance");  
                });
                opts.AddPolicy("ExchangePolicy", policy =>
                {
                    policy.AddRequirements(new ExpireDateExchangeRequirements());
                });
            });

            services.AddAuthentication().AddFacebook(opt =>
            {
                opt.AppId = Configuration["Authentication:Facebook:AppId"];
                opt.AppSecret = Configuration["Authentication:Facebook:AppSecret"];
            });
                                                                       
            // Default olarak set edilmiş valdation kurallarını alttaki gibi düzenleme yapabiliriz
            services.AddIdentity<AppUser, AppRole>(opt => {                                                                     // Lamda işareti ile yapılan kısım aslında, constructor da isteilen class'a değer set etmek içindir.

                opt.User.RequireUniqueEmail = true;                                                                             // Email uniqe olmalıdır
                opt.User.AllowedUserNameCharacters = "abcçdefgğhıijklmnopqrsştuüvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._";  // Kullanıcı adı için izin verilecek karakterler
                opt.Password.RequiredLength = 4;
                opt.Password.RequireNonAlphanumeric = false;
                opt.Password.RequireLowercase = false;
                opt.Password.RequireUppercase = false;
                opt.Password.RequireDigit = false;

            }).AddPasswordValidator<CustomPasswordValidator>().AddUserValidator<CustomUserValidator>().AddErrorDescriber<CustomIdentityErrorDescriber>().AddEntityFrameworkStores<AppIdentityDbContext>().AddDefaultTokenProviders();  // Identity
             
            // Cookie işlemleri
            CookieBuilder cookieBuilder = new CookieBuilder();
            cookieBuilder.Name = "MyCookies";
            cookieBuilder.HttpOnly = false;                                    // Client Side tarafında cookie bilgilerine erişimi önlemek için 
            cookieBuilder.SameSite = SameSiteMode.Lax;                         // Cookie güvenliği için bu ayarlar önemli SameSiteMode.Lax, SameSiteMode.Strict  Aynı siteden cookie bilgilerinin gelmesi için yapılan ayarlar
            cookieBuilder.SecurePolicy = CookieSecurePolicy.SameAsRequest;     // CookieSecurePolicy.Always => Browser, https üzerinden bir istek geldiyse kullanıcının cookie bilgilerini sunucuya gönderir
            // Cookie işlemleri
            services.ConfigureApplicationCookie(opt =>
            {
                opt.LoginPath = new PathString("/Home/Login");                 // Kullanıcı, üye olmadan sadece kullanıcıların erişebileceği bir sayfaya tıklarsa direkt Login sayfasına yönlendir
                opt.LogoutPath = new PathString("/Member/LogOut");                 
                opt.Cookie = cookieBuilder;
                opt.SlidingExpiration = true;                                  // Cookie süresini uzatmak içindir. Yani he giriş yaptığında yukarıda verdiğimiz süre kadar (60) uzatmak içindir
                opt.ExpireTimeSpan = System.TimeSpan.FromDays(60);             // opt.ExpireTimeSpan = System.TimeSpan.FromMinutes(60);           // Kullanıcı 1 kez login olduktan sonra 60 dk boyunca siteye erişim yapabilecek
                opt.AccessDeniedPath = new PathString("/Member/AccessDenied"); // Kullanıcı, üye olduktan sonra yetkisi olmayan bir sayfaya gitmeye çalışırsa direkt AccessDenied sayfasına yönlendir
            });

            services.AddScoped<IClaimsTransformation, ClaimProvider.ClaimProvider>();  // ClaimProvider
        }
         
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error"); 
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseAuthentication();   // Identity işleri için olmalıdır.
            app.UseAuthorization();
      
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
             
        }
    }
}
