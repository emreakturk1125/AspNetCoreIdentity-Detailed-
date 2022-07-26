using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace NetCoreIdentity.Web.Models
{
    //AppUser tablosundaki kolonların migration işlemi ile otomatik oluşması için IdentityUser  classının miras alınması gerekir
    public class AppUser : IdentityUser
    {
        [StringLength(50)]
        public string City { get; set; }

        [StringLength(500)]
        public string Picture { get; set; }
        public DateTime? Birthday { get; set; }
        public int Gender { get; set; }

    }
}
