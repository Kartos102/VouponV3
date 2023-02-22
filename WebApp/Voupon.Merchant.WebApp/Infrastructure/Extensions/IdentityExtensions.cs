using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;

namespace Voupon.Merchant.WebApp.Infrastructure.Extensions
{
    public static class IdentityExtensions
    {
        public static int GetMerchantId(this IIdentity identity)
        {
            if (((ClaimsIdentity)identity).FindFirst("MerchantId") != null)
            {
                return int.Parse( (((ClaimsIdentity)identity).FindFirst("MerchantId").Value) != "" ? ((ClaimsIdentity)identity).FindFirst("MerchantId").Value : "0");
            }
            return 0;
        }
        
        public static string GetUserId(this IIdentity identity)
        {
            if (((ClaimsIdentity)identity).FindFirst("UserId") != null)
            {
                return  (((ClaimsIdentity)identity).FindFirst("UserId").Value) != "" ? ((ClaimsIdentity)identity).FindFirst("UserId").Value : "";
            }
            return "";
        }
        
        public static string GetUserName(this IIdentity identity)
        {
            if (((ClaimsIdentity)identity).FindFirst("UserName") != null)
            {
                return  (((ClaimsIdentity)identity).FindFirst("UserName").Value) != "" ? ((ClaimsIdentity)identity).FindFirst("UserName").Value : "";
            }
            return "";
        }
    }
}
