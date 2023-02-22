using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Voupon.Common.Azure.Blob;
using Voupon.Rewards.WebApp.Common.Blob.Queries;
using Voupon.Rewards.WebApp.Common.MasterMemberProfiles.Queries;
using Voupon.Rewards.WebApp.Common.Orders.Queries;
using Voupon.Rewards.WebApp.Common.ProductCategories.Queries;
using Voupon.Rewards.WebApp.Common.ShippingCost.Models;
using Voupon.Rewards.WebApp.Infrastructures.Enums;
using Voupon.Rewards.WebApp.Services.Aggregators.Queries;
using Voupon.Rewards.WebApp.Services.Cart.Commands;
using Voupon.Rewards.WebApp.Services.Cart.Models;
using Voupon.Rewards.WebApp.Services.Cart.Queries;
using Voupon.Rewards.WebApp.Services.Logger;
using Voupon.Rewards.WebApp.Services.Profile.Page;
using Voupon.Rewards.WebApp.ViewModels;
using static Voupon.Rewards.WebApp.Common.MasterMemberProfiles.Queries.MasterMemberProfileDetailsQueryHandler;
using static Voupon.Rewards.WebApp.Services.Deal.Page.DetailPage;

namespace Voupon.Rewards.WebApp.Controllers
{
   
    public class TNTController : BaseController
    {
        [Route("tnc/first-time-rm20-off")]
        public IActionResult FirstTimeRM20Off()
        {
            return View("~/views/tnc/FirstTimeRM20Off.cshtml");
        }

        [Route("tnc/cny2022")]
        public IActionResult CNY2020()
        {
            return View("~/views/tnc/CNY2022.cshtml");
        }

        [Route("tnc/march50off2022")]
        public IActionResult March50Off2022()
        {
            return View("~/views/tnc/March50Off2022.cshtml");
        }

        [Route("tnc/double-discounts-wednesday")]
        public IActionResult DoubleDiscountWednesday()
        {
            return View("~/views/tnc/DoubleDiscountsWednesday.cshtml");
        }

        [Route("tnc/dvp")]
        public IActionResult DVP()
        {
            return View("~/views/tnc/dvp.cshtml");
        }
        [Route("tnc/raya80off2022")]
        public IActionResult Raya80off2022()
        {
            return View("~/views/tnc/Raya80Off2022.cshtml");
        }

        [Route("tnc/mothers-day-2022")]
        public IActionResult MothersDay2022()
        {
            return View("~/views/tnc/MothersDay2022.cshtml");
        }

        [Route("tnc/fathers-day-2022")]
        public IActionResult FathersDay2022()
        {
            return View("~/views/tnc/FathersDay2022.cshtml");
        }
    }
}