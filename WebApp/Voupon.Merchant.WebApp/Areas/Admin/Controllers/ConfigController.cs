using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Voupon.Merchant.WebApp.Areas.Admin.Services.Config.Commands;
using Voupon.Merchant.WebApp.Areas.Admin.Services.Config.Pages;
using Voupon.Merchant.WebApp.Areas.Admin.Services.Config.Queries.List;
using Voupon.Merchant.WebApp.Areas.Admin.Services.SendInvouicesEmailCommand.Commands;
using Voupon.Merchant.WebApp.Common.Services.Products.Command;
using Voupon.Merchant.WebApp.Controllers;
using Voupon.Merchant.WebApp.Infrastructure.Enums;
using static Voupon.Merchant.WebApp.Areas.Admin.Services.Config.Pages.IndexPage;

namespace Voupon.Merchant.WebApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/[controller]")]
    [Authorize(Roles = "Admin")]
    public class ConfigController : BaseController
    {
        public async Task<IActionResult> Index()
        {
            var viewModel = await Mediator.Send(new IndexPage());
            if (!viewModel.Successful)
            {
                return View(ErrorPageEnum.INVALID_REQUEST_PAGE);
            }
            return View((IndexPageViewModel)viewModel.Data);
        }

        [Route("update-config")]
        [HttpPost]
        public async Task<IActionResult> UpdateConfig(UpdateConfigCommand command)
        {
            if (command.RinggitPerVpoints == 0)
            {
                return BadRequest("Commercial Rate can't be Zero");
            }
            command.UpdatedBy = new Guid(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var result = await Mediator.Send(command);
            if (!result.Successful)
            {
                return BadRequest(result.Message);
            }
            else if (!(bool)result.Data)
            {
                var resultOfUpdatingProducts = await Mediator.Send(new UpdateAllProductsWithVpointsByCommercialRateCommand());
                if (!resultOfUpdatingProducts.Successful)
                {
                    return BadRequest(result.Message);
                }
            }
            return Ok(result);
        }

        [Route("Send-Invouices-Email")]
        [HttpPost]
        public async Task<IActionResult> Send(SendInvouicesEmailCommand command)
        {
            var result = await Mediator.Send(command);
            return Ok(result);
        }

        [Route("AggregatorMaxOrderFilter")]
        public async Task<IActionResult> AggregatorMaxOrderFilter()
        {
            var viewModel = await Mediator.Send(new MaxOrderFilterPage());
            if (!viewModel.Successful)
            {
                return View(ErrorPageEnum.INVALID_REQUEST_PAGE);
            }
            return View(viewModel.Data);
        }

        [Route("max-order-filter")]
        public async Task<IActionResult> GetMaxOrderFilter()
        {
            var viewModel = await Mediator.Send(new ListMaxOrderQuery());
            if (!viewModel.Successful)
            {
                new BadRequestObjectResult(viewModel);
            }
            return new OkObjectResult(viewModel);
        }

        [Route("max-order-filter")]
        [HttpPost]
        public async Task<IActionResult> CreateMaxOrderQuantity(CreateMaxOrderQuantityCommand command)
        {
            var viewModel = await Mediator.Send(command);
            if (!viewModel.Successful)
            {
                new BadRequestObjectResult(viewModel);
            }
            return new OkObjectResult(viewModel);
        }

        [Route("update-max-order-quantity-filter")]
        [HttpPost]
        public async Task<IActionResult> UpdateMaxOrderQuantity(UpdateMaxOrderCommand command)
        {
            var viewModel = await Mediator.Send(command);
            if (!viewModel.Successful)
            {
                new BadRequestObjectResult(viewModel);
            }
            return new OkObjectResult(viewModel);
        }

        [Route("update-max-order-quantity-status")]
        [HttpPost]
        public async Task<IActionResult> UpdateMaxOrderQuantityStatus(UpdateMaxOrderStatusCommand command)
        {
            var viewModel = await Mediator.Send(command);
            if (!viewModel.Successful)
            {
                new BadRequestObjectResult(viewModel);
            }
            return new OkObjectResult(viewModel);
        }

        [Route("AggregatorItemExcludeFilter")]
        public async Task<IActionResult> AggregatorItemExcludeFilter()
        {
            var viewModel = await Mediator.Send(new ItemFilterPage());
            if (!viewModel.Successful)
            {
                return View(ErrorPageEnum.INVALID_REQUEST_PAGE);
            }
            return View(viewModel.Data);
        }

        [Route("item-filter")]
        public async Task<IActionResult> ItemFilter()
        {
            var viewModel = await Mediator.Send(new ListItemFilterQuery());
            if (!viewModel.Successful)
            {
                new BadRequestObjectResult(viewModel);
            }
            return new OkObjectResult(viewModel);
        }

        [Route("item-filter")]
        [HttpPost]
        public async Task<IActionResult> CreateItemFilter(CreateItemExcludeFilterCommand command)
        {
            var viewModel = await Mediator.Send(command);
            if (!viewModel.Successful)
            {
                new BadRequestObjectResult(viewModel);
            }
            return new OkObjectResult(viewModel);
        }

        [Route("update-item-filter")]
        [HttpPost]
        public async Task<IActionResult> UpdateItemFilter(UpdateItemFilterCommand command)
        {
            var viewModel = await Mediator.Send(command);
            if (!viewModel.Successful)
            {
                new BadRequestObjectResult(viewModel);
            }
            return new OkObjectResult(viewModel);
        }

        [Route("update-item-filter-status")]
        [HttpPost]
        public async Task<IActionResult> UpdateItemFilterStatus(UpdateItemFilterStatusCommand command)
        {
            var viewModel = await Mediator.Send(command);
            if (!viewModel.Successful)
            {
                new BadRequestObjectResult(viewModel);
            }
            return new OkObjectResult(viewModel);
        }

        [Route("AggregatorMerchantExcludeFilter")]
        public async Task<IActionResult> AggregatorMerchantExcludeFilter()
        {
            var viewModel = await Mediator.Send(new MerchantFilterPage());
            if (!viewModel.Successful)
            {
                return View(ErrorPageEnum.INVALID_REQUEST_PAGE);
            }
            return View(viewModel.Data);
        }

        [Route("merchant-filter")]
        public async Task<IActionResult> MerchantFilter()
        {
            var viewModel = await Mediator.Send(new ListMerchantFilterQuery());
            if (!viewModel.Successful)
            {
                new BadRequestObjectResult(viewModel);
            }
            return new OkObjectResult(viewModel);
        }

        [Route("merchant-filter")]
        [HttpPost]
        public async Task<IActionResult> CreateMerchantFilter(CreateMerchantExcludeFilterCommand command)
        {
            var viewModel = await Mediator.Send(command);
            if (!viewModel.Successful)
            {
                new BadRequestObjectResult(viewModel);
            }
            return new OkObjectResult(viewModel);
        }

        [Route("update-merchant-filter")]
        [HttpPost]
        public async Task<IActionResult> UpdateMerchantFilter(UpdateMerchantExcludeFilterCommand command)
        {
            var viewModel = await Mediator.Send(command);
            if (!viewModel.Successful)
            {
                new BadRequestObjectResult(viewModel);
            }
            return new OkObjectResult(viewModel);
        }

        [Route("update-merchant-filter-status")]
        [HttpPost]
        public async Task<IActionResult> UpdateMerchantFilterStatus(UpdateMerchantFilterStatusCommand command)
        {
            var viewModel = await Mediator.Send(command);
            if (!viewModel.Successful)
            {
                new BadRequestObjectResult(viewModel);
            }
            return new OkObjectResult(viewModel);
        }


        [Route("AggregatorKeywordFilter")]
        public async Task<IActionResult> AggregatorKeywordFilter()
        {
            var viewModel = await Mediator.Send(new KeywordFilterPage());
            if (!viewModel.Successful)
            {
                return View(ErrorPageEnum.INVALID_REQUEST_PAGE);
            }
            return View(viewModel.Data);
        }

        [Route("keyword-filter")]
        public async Task<IActionResult> GetKeywordFilter()
        {
            var viewModel = await Mediator.Send(new ListKeywordFilterQuery());
            if (!viewModel.Successful)
            {
                new BadRequestObjectResult(viewModel);
            }
            return new OkObjectResult(viewModel);
        }

        [Route("keyword-filter")]
        [HttpPost]
        public async Task<IActionResult> CreateKeywordFilter(CreateKeywordFilterCommand command)
        {
            var viewModel = await Mediator.Send(command);
            if (!viewModel.Successful)
            {
                new BadRequestObjectResult(viewModel);
            }
            return new OkObjectResult(viewModel);
        }

        [Route("update-keyword-filter")]
        [HttpPost]
        public async Task<IActionResult> UpdateKeywordFilter(UpdateKeywordFilterCommand command)
        {
            var viewModel = await Mediator.Send(command);
            if (!viewModel.Successful)
            {
                new BadRequestObjectResult(viewModel);
            }
            return new OkObjectResult(viewModel);
        }

        [Route("update-keyword-filter-status")]
        [HttpPost]
        public async Task<IActionResult> UpdateMerchantFilterStatus(UpdateKeywordFilterStatusCommand command)
        {
            var viewModel = await Mediator.Send(command);
            if (!viewModel.Successful)
            {
                new BadRequestObjectResult(viewModel);
            }
            return new OkObjectResult(viewModel);
        }

        [Route("AdditionalDiscounts")]
        public async Task<IActionResult> AdditionalDiscounts()
        {
            var viewModel = await Mediator.Send(new AdditionalDiscountPage());
            if (!viewModel.Successful)
            {
                return View(ErrorPageEnum.INVALID_REQUEST_PAGE);
            }
            return View(viewModel.Data);
        }

        [Route("additional-discounts")]
        public async Task<IActionResult> GetAdditionalDiscount()
        {
            var viewModel = await Mediator.Send(new ListAdditionalDiscountQuery());
            if (!viewModel.Successful)
            {
                new BadRequestObjectResult(viewModel);
            }
            return new OkObjectResult(viewModel);
        }

        [Route("additional-discounts")]
        [HttpPost]
        public async Task<IActionResult> CreateAdditionalDiscounts(CreateAdditionalDiscountCommand command)
        {
            var viewModel = await Mediator.Send(command);
            if (!viewModel.Successful)
            {
                new BadRequestObjectResult(viewModel);
            }
            return new OkObjectResult(viewModel);
        }

        [Route("update-additional-discounts")]
        [HttpPost]
        public async Task<IActionResult> UpdateAdditionalDiscounts(UpdateAdditionalDiscountCommand command)
        {
            var viewModel = await Mediator.Send(command);
            if (!viewModel.Successful)
            {
                new BadRequestObjectResult(viewModel);
            }
            return new OkObjectResult(viewModel);
        }

        [Route("update-additional-discounts-status")]
        [HttpPost]
        public async Task<IActionResult> UpdateAdditionalDiscountsStatus(UpdateAdditionalDiscountStatusCommand command)
        {
            var viewModel = await Mediator.Send(command);
            if (!viewModel.Successful)
            {
                new BadRequestObjectResult(viewModel);
            }
            return new OkObjectResult(viewModel);
        }


    }
}