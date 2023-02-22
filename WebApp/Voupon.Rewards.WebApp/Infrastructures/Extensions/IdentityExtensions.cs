using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using Voupon.Common;

namespace Voupon.Rewards.WebApp.Infrastructures.Extensions
{
    public static class IdentityExtensions
    {
        public static string GetClaimValue(this IPrincipal currentPrincipal, string key)
        {
            var identity = currentPrincipal.Identity as ClaimsIdentity;
            if (identity == null)
                return null;

            var claim = identity.Claims.FirstOrDefault(c => c.Type == key);
            return claim?.Value;
        }

        public static int GetAvailablePoints(this IIdentity identity)
        {
            if (((ClaimsIdentity)identity).FindFirst("AvailablePoints") != null)
            {
                return int.Parse(((ClaimsIdentity)identity).FindFirst("AvailablePoints").Value);
            }
            return 0;
        }

        public static string GetAccountUserId(this IIdentity identity)
        {
            if (((ClaimsIdentity)identity).FindFirst("UserId") != null)
            {
                return ((ClaimsIdentity)identity).FindFirst("UserId").Value;
            }
            return "";
        }

        public static string GetMobileCountryCode(this IIdentity identity)
        {
            if (((ClaimsIdentity)identity).FindFirst("MobileCountryCode") != null)
            {
                return ((ClaimsIdentity)identity).FindFirst("MobileCountryCode").Value;
            }
            return "";
        }

        public static string GetMobileNumber(this IIdentity identity)
        {
            if (((ClaimsIdentity)identity).FindFirst("MobileNumber") != null)
            {
                return ((ClaimsIdentity)identity).FindFirst("MobileNumber").Value;
            }
            return "";
        }

        public static string GetAddressLine1(this IIdentity identity)
        {
            if (((ClaimsIdentity)identity).FindFirst("AddressLine1") != null)
            {
                return ((ClaimsIdentity)identity).FindFirst("AddressLine1").Value;
            }
            return "";
        }

        public static string GetAddressLine2(this IIdentity identity)
        {
            if (((ClaimsIdentity)identity).FindFirst("AddressLine2") != null)
            {
                return ((ClaimsIdentity)identity).FindFirst("AddressLine2").Value;
            }
            return "";
        }

        public static string GetAddressPostcode(this IIdentity identity)
        {
            if (((ClaimsIdentity)identity).FindFirst("Postcode") != null)
            {
                return ((ClaimsIdentity)identity).FindFirst("Postcode").Value;
            }
            return "";
        }

        public static string GetAddressCity(this IIdentity identity)
        {
            if (((ClaimsIdentity)identity).FindFirst("City") != null)
            {
                return ((ClaimsIdentity)identity).FindFirst("City").Value;
            }
            return "";
        }

        public static string GetAddressState(this IIdentity identity)
        {
            if (((ClaimsIdentity)identity).FindFirst("State") != null)
            {
                return ((ClaimsIdentity)identity).FindFirst("State").Value;
            }
            return "";
        }

        public static int GetAddressCountryId(this IIdentity identity)
        {
            if (((ClaimsIdentity)identity).FindFirst("CountryId") != null)
            {
                return int.Parse(((ClaimsIdentity)identity).FindFirst("CountryId").Value);
            }
            return 0;
        }

        public static string GetAddressCountryName(this IIdentity identity)
        {
            if (((ClaimsIdentity)identity).FindFirst("CountryName") != null)
            {
                return ((ClaimsIdentity)identity).FindFirst("CountryName").Value;
            }
            return "";
        }

        public static string GetRole(this IIdentity identity)
        {
            if (((ClaimsIdentity)identity).FindFirst("Role") != null)
            {
                return ((ClaimsIdentity)identity).FindFirst("Role").Value;
            }
            return "";
        }

        public static string GetEmail(this IIdentity identity)
        {
            if (((ClaimsIdentity)identity).FindFirst("Email") != null)
            {
                return ((ClaimsIdentity)identity).FindFirst("Email").Value;
            }
            return "";
        }

        public static int GetClientProfileId(this IIdentity identity)
        {

            if (((ClaimsIdentity)identity).FindFirst("ClientProfileId") != null)
            {
                return int.Parse(((ClaimsIdentity)identity).FindFirst("ClientProfileId").Value);
            }
            return 0;
        }

        public static string GetUsername(this IIdentity identity)
        {
            if (((ClaimsIdentity)identity).FindFirst("Username") != null)
            {
                return ((ClaimsIdentity)identity).FindFirst("Username").Value;
            }
            return "";
        }

        public static string GetFirstName(this IIdentity identity)
        {

            if (((ClaimsIdentity)identity).FindFirst("FirstName") != null)
            {
                return ((ClaimsIdentity)identity).FindFirst("FirstName").Value;
            }
            return "";
        }

        public static string GetLastName(this IIdentity identity)
        {

            if (((ClaimsIdentity)identity).FindFirst("LastName") != null)
            {
                return ((ClaimsIdentity)identity).FindFirst("LastName").Value;
            }
            return "";
        }

        public static string GetLocale(this IIdentity identity)
        {
            if (((ClaimsIdentity)identity).FindFirst("Locale") != null)
            {
                return ((ClaimsIdentity)identity).FindFirst("Locale").Value;
            }
            return "";
        }

        public static int GetPartnerWebsiteId(this IIdentity identity)
        {
            if (((ClaimsIdentity)identity).FindFirst("PartnerWebsiteID") != null)
            {
                return int.Parse(((ClaimsIdentity)identity).FindFirst("PartnerWebsiteID").Value);
            }
            return 0;
        }

        public static string GetPartnerWebsiteName(this IIdentity identity)
        {
            if (((ClaimsIdentity)identity).FindFirst("PartnerWebsiteName") != null)
            {
                return ((ClaimsIdentity)identity).FindFirst("PartnerWebsiteName").Value;
            }
            return "";
        }

        public static string GetPartnerWebsiteUrl(this IIdentity identity)
        {
            if (((ClaimsIdentity)identity).FindFirst("PartnerWebsiteUrl") != null)
            {
                return ((ClaimsIdentity)identity).FindFirst("PartnerWebsiteUrl").Value;
            }
            return "";
        }

        public static bool IsPArtnerAdmin(this IIdentity identity)
        {
            if (((ClaimsIdentity)identity).FindFirst("PartnerAdmin") != null)
            {
                return (((ClaimsIdentity)identity).FindFirst("PartnerAdmin").Value == "1");
            }
            return false;
        }

        public static int GetMasterMemberId(this IIdentity identity)
        {
            if (((ClaimsIdentity)identity).FindFirst("MasterMemberProfileId") != null)
            {
                return int.Parse(((ClaimsIdentity)identity).FindFirst("MasterMemberProfileId").Value);
            }
            return 0;
        }
    }
}
