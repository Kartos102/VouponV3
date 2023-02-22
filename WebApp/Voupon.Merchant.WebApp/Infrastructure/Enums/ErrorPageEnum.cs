using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Voupon.Merchant.WebApp.Infrastructure.Enums
{
    public static class ErrorPageEnum
    {
        public static string NOT_ALLOWED_PAGE = "~/Views/Error/NotAllowed.cshtml";
        public static string INVALID_REQUEST_PAGE  = "~/Views/Error/InvalidRequest.cshtml";      
        public static string ERROR_404 = "~/Views/Error/Error404.cshtml";
        public static string ERROR_500 = "~/Views/Error/Error500.cshtml";
    }
}
