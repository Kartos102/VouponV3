using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Voupon.Merchant.WebApp.Controllers
{
    public class AppController : Controller
    {
        public IActionResult Index()
        {
            if(User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Dashboard", new { area = "app" });
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }
    }
}