using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Voupon.Merchant.WebApp.Areas.Admin.Services.GoogleMerchant.Commands;
using Voupon.Merchant.WebApp.Areas.Admin.Services.GoogleMerchant.Queries.List;
using Voupon.Merchant.WebApp.ViewModels;

namespace Voupon.Merchant.WebApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/[controller]")]
    [Authorize(Roles = "Admin")]
    public class GoogleMerchantController : BaseAdminController
    {
        public IActionResult Index()
        {
            return View();
        }

        [Route("get-keywords")]
        public async Task<ApiResponseViewModel> GetKeywords()
        {
            return await Mediator.Send(new ListKeywordQuery());
        }

        [Route("delete-keywords")]
        public async Task<ApiResponseViewModel> DeleteKeywords(Guid id)
        {
            return await Mediator.Send(new DeleteKeywordCommand
            {
                Id = id
            });
        }

        [Route("update-keywords")]
        public async Task<ApiResponseViewModel> UpdateKeywords(KeywordViewModel model)
        {
            return await Mediator.Send(new UpdateKeywordCommand
            {
                Id = model.Id,
                Keyword = model.Keyword,
                SortBy = model.SortBy,
                TotalListing = model.TotalListing
            });
        }

        [Route("create-keywords")]
        public async Task<ApiResponseViewModel> CreateKeywords(KeywordViewModel model)
        {
            return await Mediator.Send(new CreateKeywordCommand
            {
                Id = model.Id,
                Keyword = model.Keyword,
                SortBy = model.SortBy,
                TotalListing = model.TotalListing
            });
        }

        [Route("upload-file")]
        public async Task<ApiResponseViewModel> UploadFile()
        {
            var fileContents = HttpContext.Request.Form.Files;

            var result = await Mediator.Send(new CreateKeywordFromFileCommand
            {
                Files = fileContents
            });

            return result;
        }
    }
}
