using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Voupon.Merchant.WebApp.Areas.Admin.Services.Analytics.Queries.List;
using Voupon.Merchant.WebApp.Areas.Admin.Services.Config.Commands;
using Voupon.Merchant.WebApp.Areas.Admin.Services.Config.Pages;
using Voupon.Merchant.WebApp.Areas.Admin.Services.Config.Queries.List;
using Voupon.Merchant.WebApp.Areas.Admin.Services.CrawlerMerchant.Commands;
using Voupon.Merchant.WebApp.Areas.Admin.Services.SendInvouicesEmailCommand.Commands;
using Voupon.Merchant.WebApp.Areas.Admin.ViewModels.CrawlerMerchant;
using Voupon.Merchant.WebApp.Common.Services.CrawlerMerchant.Models;
using Voupon.Merchant.WebApp.Common.Services.CrawlerMerchant.Queries;
using Voupon.Merchant.WebApp.Common.Services.Products.Command;
using Voupon.Merchant.WebApp.Controllers;
using Voupon.Merchant.WebApp.Infrastructure.Enums;
using Voupon.Merchant.WebApp.ViewModels;
using static Voupon.Merchant.WebApp.Areas.Admin.Services.Config.Pages.IndexPage;


namespace Voupon.Merchant.WebApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/[controller]")]
    [Authorize(Roles = "Admin")]
    public class CrawlerMerchantController : BaseAdminController
    {
        public async Task<IActionResult> Index() {


            return View();
        }


        [HttpGet]
        [Route("GetCrawlerMerchantList")]
        public async Task<ApiResponseViewModel> GetCrawlerMerchantList(string searchValue, int limit, int offset)
        {
            return await Mediator.Send(new CrawlerMerchantListQuery() {
                searchValue = searchValue,
                limit = limit,
                offset = offset
            });
        }


        [HttpPost]
        [Route("UpdateCrawlerMerchantStatus")]
        public async Task<ApiResponseViewModel> UpdateCrawlerStatus(UpdateCrawlerStatusCommand request)
        {
            return await Mediator.Send(new UpdateCrawlerStatusCommand() { 
                Id = request.Id,
                Status = request.Status,
            });
        }

        [HttpPost]
        [Route("UpdateCrawlerMerchant")]
        public async Task<ApiResponseViewModel> UpdateCrawler(UpdateCrawlerCommand request)
        {
            return await Mediator.Send(new UpdateCrawlerCommand()
            {
                Id = request.Id,
                ShopeeUrl = request.ShopeeUrl,
                MerchantName = request.MerchantName,
            });
        }

        [HttpPost]
        [Route("AddBulkCrawler")]
        public async Task<ApiResponseViewModel> AddBulkCrawler(CreateBulkCrawlerCommand request)
        {
            return await Mediator.Send(new CreateBulkCrawlerCommand() { ShopeeUrls = request.ShopeeUrls });
    
        }

        [HttpPost]
        [Route("RemoveCrawlerMerchant")]
        public async Task<ApiResponseViewModel> RemoveCrawlerMerchant(DeleteCrawlerCommand request)
        {
            return await Mediator.Send(new DeleteCrawlerCommand() { Id = request.Id });

        }



    }
}
