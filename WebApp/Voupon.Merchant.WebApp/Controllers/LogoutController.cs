using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Merchant.WebApp.Services.Identity.Commands;

namespace Voupon.Merchant.WebApp.Controllers
{
    public class LogoutController : BaseController
    {
        private readonly SignInManager<Users> signInManager;
        public LogoutController(SignInManager<Users> signInManager)
        {
            this.signInManager = signInManager;
        }
        public async Task<IActionResult> Index()
        {
            await signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}