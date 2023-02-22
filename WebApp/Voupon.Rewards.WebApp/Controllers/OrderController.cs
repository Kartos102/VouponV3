using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Voupon.Rewards.WebApp.Common.Orders.Queries;
using Voupon.Rewards.WebApp.Common.ProductReview.Command;
using Voupon.Rewards.WebApp.Services.Cart.Models;
using Voupon.Rewards.WebApp.Services.Cart.Queries;
using Voupon.Rewards.WebApp.Services.Order.Pages;
using Voupon.Rewards.WebApp.ViewModels;
using static Voupon.Rewards.WebApp.Services.Order.Pages.InStoreRedemptionPage;
using static Voupon.Rewards.WebApp.Services.Order.Pages.RefundsPage;
using System.Linq.Dynamic.Core;
using Voupon.Merchant.WebApp.Common.Services.OrderItemsExternal.Queries;
using Voupon.Rewards.WebApp.Services.Order.Commands;

namespace Voupon.Rewards.WebApp.Controllers
{

    [Route("Order")]
    public class OrderController : BaseController
    {
        [Authorize]
        [HttpGet]
        [Route("History")]
        public async Task<IActionResult> History()
        {
            return View();
        }

        [Authorize]
        [HttpGet]
        [Route("ShippingDetails/{id}")]
        public async Task<IActionResult> ShippingDetails(string id, int type)
        {
            return View();
        }

        [Authorize]
        [HttpGet]
        [Route("GetOrderHistory")]
        public async Task<ApiResponseViewModel> GetOrderHistory()
        {

            ApiResponseViewModel response = await Mediator.Send(new OrderHistoryListQuery() { Id = GetMasterMemberId(Request.HttpContext) });
            if (response.Successful)
            {
                var orderHistoryList = (List<OrderHistoryModel>)response.Data;
                orderHistoryList = orderHistoryList.Where(x => x.OrderStatus == 2).OrderByDescending(x => x.CreatedAt).ToList();
                response.Data = orderHistoryList;
                return response;
            }
            else
            {
                return response;
            }

        }
        [Authorize]
        [HttpGet]
        [Route("Get-Shippind-Details-By-Order-ShopId")]
        public async Task<ApiResponseViewModel> GetShippindDetailsByOrderShopId(Guid orderShopId, int type)
        {
            var query = new ApiResponseViewModel();
            if (type == 2)
            {
                query = await Mediator.Send(new GetExternalOrderShippindDetailsByOrderShopId
                {
                    OrderShopId = orderShopId
                });
            }
            else if(type == 1)
            {
                query = await Mediator.Send(new OrderItemRedemptionTokenQuery() { OrderItemId = orderShopId });
            }

            return query;
        }

        [Authorize]
        [HttpGet]
        [Route("GetOrderHistoryChunk")]
        public async Task<ApiResponseViewModel> GetOrderHistoryChunk(string start, string length, string searchValue)
        {
            try
            {
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt32(start) : 0;
                int recordsTotal = 0;

                ApiResponseViewModel response = await Mediator.Send(new OrderHistoryListQuery() { Id = GetMasterMemberId(Request.HttpContext) });

                if (response.Successful)
                {
                    var orderHistoryList = (List<OrderItems>)response.Data;
                    orderHistoryList = orderHistoryList.Where(x => x.OrderStatus == 2).OrderByDescending(x => x.CreatedAt).ToList();

                    if (!string.IsNullOrEmpty(searchValue))
                    {
                        orderHistoryList = orderHistoryList.Where(x => x.ProductTitle.ToLower().Contains(searchValue)).ToList();

                    }
                    recordsTotal = orderHistoryList.Count();
                    var data = orderHistoryList.Skip(skip).Take(pageSize).ToList();
                    var jsonData = new { recordsFiltered = data.Count(), recordsTotal = recordsTotal, data = data.OrderByDescending(x => x.CreatedAt) };
                    response.Data = jsonData;
                    return response;
                }
                else
                {
                    return response;
                }
            }
            catch (Exception ex)
            {
                ApiResponseViewModel badRequestResponse = new ApiResponseViewModel();
                badRequestResponse.Successful = false;
                badRequestResponse.Message = ex.Message;
                return badRequestResponse;
            }
        }

        [HttpGet]
        [Route("Refunds")]
        public async Task<IActionResult> Refunds()
        {

            var response = await Mediator.Send(new RefundsPage() { MasterMemberProfileId = GetMasterMemberId(Request.HttpContext) });
            if (response.Successful)
            {
                return View((RefundsPageViewModel)response.Data);
            }
            else
            {
                return View();
            }
        }


        [HttpPost]
        [Route("ReviewProduct/{id}")]
        public async Task<IActionResult> ReviewProduct(int id, int Rating, int MerchantId, string Comment, Guid orderItemId)
        {
            ApiResponseViewModel response = await Mediator.Send(new AddReviewCommand() { ProductId = id, MasterMemberProfileId = GetMasterMemberId(Request.HttpContext), Comment = Comment, Rating = Rating, MerchantId = MerchantId, OrderItemId = orderItemId });
            if (response.Successful)
            {
                return Ok(response);
            }
            else
            {
                return BadRequest(response);
            }

        }

        [Authorize]
        [HttpGet]
        [Route("Pending")]
        public async Task<IActionResult> Pending()
        {

            ApiResponseViewModel response = await Mediator.Send(new PendingPaymentListQuery() { Id = GetMasterMemberId(Request.HttpContext) });
            if (response.Successful)
            {
                var orderHistoryList = (List<OrderHistoryModel>)response.Data;
                orderHistoryList = orderHistoryList.Where(x => x.OrderStatus == 1).OrderByDescending(x => x.CreatedAt).ToList();
                return View(orderHistoryList);
            }
            else
            {
                return View();
            }

        }

        [HttpGet]
        [Route("GetRedemptionDetails")]
        public async Task<IActionResult> GetRedemptionDetails(Guid orderItemId)
        {
            ApiResponseViewModel result = await Mediator.Send(new OrderItemRedemptionTokenQuery() { OrderItemId = orderItemId });
            if (result.Successful)
            {
                return Ok(result);
            }
            return BadRequest(result.Message);
        }

        [HttpGet]
        [Route("store-redemption/{id}")]
        public async Task<IActionResult> InStoreRedemption(Guid id)
        {
            ApiResponseViewModel result = await Mediator.Send(new InStoreRedemptionPage()
            {
                Id = id,
                MasterMemberProfileId = 0
            });
            if (result.Successful)
            {
                return View((InStoreRedemptionPageViewModel)result.Data);
            }
            return BadRequest(result.Message);
        }

        [Authorize]
        [HttpPost]
        [Route("CancelOrder")]
        public async Task<IActionResult> CancelOrder(Guid orderId)
        {
            var result = await Mediator.Send(new CancelOrderCommand
            {
                OrderId = orderId,
                MasterMemberProfileId = GetMasterMemberId(Request.HttpContext)
            });
            return Ok(result);
        }

    }
}