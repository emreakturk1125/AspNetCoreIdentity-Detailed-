using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Razor.TagHelpers;
using NetCoreIdentity.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetCoreIdentity.Web.CustomTagHelper
{
    [HtmlTargetElement("td",Attributes = "user-roles")]
    public class UserRolesName : TagHelper
    {
        public UserManager<AppUser> UserManager { get; set; }

        public UserRolesName(UserManager<AppUser> userRolesName)
        {
            this.UserManager = userRolesName;
        }

        [HtmlAttributeName("user-roles")]
        public string UserId { get; set; }
        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            AppUser user = await UserManager.FindByIdAsync(UserId);
            List<string> roles = await UserManager.GetRolesAsync(user) as List<string>;
            string html = string.Empty;
            roles.ToList().ForEach(x =>
            {
                html += $"<span class='badge badge-info'>{x}</span>";
            });

            output.Content.SetHtmlContent(html);
        }
    }
}
