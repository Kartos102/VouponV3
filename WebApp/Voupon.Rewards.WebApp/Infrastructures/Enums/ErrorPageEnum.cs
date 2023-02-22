using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Voupon.Rewards.WebApp.Infrastructures.Enums
{
    public static class ErrorPageEnum
    {
        public static string ACCESS_DENIED = "~/Views/Error/AccessDenied.cshtml";
        public static string INVALID_REQUEST_PAGE = "~/Views/Error/InvalidRequest.cshtml";
        public static string ERROR_404 = "~/Views/Error/Error404.cshtml";
        public static string ERROR_500 = "~/Views/Error/Error500.cshtml";
        public static string Produc_No_Longer_Available = "~/Views/Error/ProductNoLongerAvailable.cshtml";
    }
}
