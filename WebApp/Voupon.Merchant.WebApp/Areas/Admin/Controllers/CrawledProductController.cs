using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Voupon.Merchant.WebApp.Areas.Admin.Services.CrawledProduct.Commands;
using Voupon.Merchant.WebApp.Common.Services.CrawledProduct.Queries;
using Voupon.Merchant.WebApp.ViewModels;


namespace Voupon.Merchant.WebApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/[controller]")]
    [Authorize(Roles = "Admin")]
    public class CrawledProductController : BaseAdminController
    {
        public IActionResult Index()
        {
            return View();
        }


        [HttpGet]
        [Route("GetCrawledProductList")]
        public async Task<ApiResponseViewModel> GetCrawledProductList(string searchValue,int limit,int offset)
        {
            return await Mediator.Send(new CrawledProductListQuery()
            {
                searchValue = searchValue,
                limit = limit,
                offset = offset
            });
        }

        [HttpPost]
        [Route("UpdateCrawledProductStatus")]
        public async Task<ApiResponseViewModel> UpdateCrawledProductStatus(UpdateCrawledStatusCommand request)
        {
            return await Mediator.Send(new UpdateCrawledStatusCommand()
            {
                Id = request.Id,
                Status = request.Status,
            });
        }

    }




}
