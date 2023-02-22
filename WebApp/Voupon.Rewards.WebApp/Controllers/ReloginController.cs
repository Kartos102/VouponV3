using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Voupon.Rewards.WebApp.Infrastructures.Enums;
using Voupon.Rewards.WebApp.Services.Deal.Page;
using Voupon.Rewards.WebApp.ViewModels;
using static Voupon.Rewards.WebApp.Services.Deal.Page.DetailPage;
using Voupon.Rewards.WebApp.Services.Deal.Commands;
using Voupon.Rewards.WebApp.Common.Services.ProductVariations.Queries;
using Voupon.Rewards.WebApp.Common.Blob.Queries;
using Voupon.Common.Azure.Blob;
using Voupon.Rewards.WebApp.Common.Services.ProductShipping.Queries;
using Voupon.Rewards.WebApp.Services.Aggregators.Queries;
using Newtonsoft.Json;
using Voupon.Rewards.WebApp.Common.Products.Queries;
using Voupon.Rewards.WebApp.Common.Products.Models;

namespace Voupon.Rewards.WebApp.Controllers
{
    public class ReloginController : BaseController
    {
        public IActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }
    }
}