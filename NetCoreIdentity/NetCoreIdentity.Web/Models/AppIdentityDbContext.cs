using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NetCoreIdentity.Web.ViewModels;

namespace NetCoreIdentity.Web.Models
{

    // Startup dosyasına ekle AppIdentityDbContext
    public class AppIdentityDbContext : IdentityDbContext<AppUser,AppRole,string>   // AppUser ile AppRole Eşleştirmesini string baz lı yapalım demektir.
    {
        public AppIdentityDbContext(DbContextOptions<AppIdentityDbContext> options) : base(options)
        {

        }

        public DbSet<Customer> Customers { get; set; }

        public DbSet<NetCoreIdentity.Web.ViewModels.RoleViewModel> RoleViewModel { get; set; }
    }
}
