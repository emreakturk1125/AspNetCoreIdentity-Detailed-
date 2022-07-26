using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetCoreIdentity.Web.Models
{
    //Role tablosundaki kolonların migration işlemi ile otomatik oluşması için IdentityUser  classının miras alınması gerekir

    public class AppRole : IdentityRole
    {
    }
}
