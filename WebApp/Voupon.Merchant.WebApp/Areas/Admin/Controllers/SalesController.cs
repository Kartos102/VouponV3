using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Voupon.Merchant.WebApp.ViewModels;
using Voupon.Common.Azure.Blob;
using Voupon.Merchant.WebApp.Common.Services.Blob.Commands.Create;
using Voupon.Merchant.WebApp.Common.Services.StatusTypes.Queries;
using Voupon.Merchant.WebApp.Common.Services.ProductCategories.Queries;
using Voupon.Merchant.WebApp.Common.Services.ProductSubcategories.Queries;
using Voupon.Merchant.WebApp.Common.Services.DealTypes.Queries;
using Voupon.Merchant.WebApp.Common.Services.ExpirationTypes.Queries;
using Voupon.Merchant.WebApp.Common.Services.Outlets.Queries;
using Voupon.Merchant.WebApp.Common.Services.Merchants.Queries;
using Voupon.Merchant.WebApp.Common.Services.Products.Queries;
using Voupon.Merchant.WebApp.Common.Services.DealExpirations.Queries;
using Voupon.Merchant.WebApp.Common.Services.Products.Command;
using Microsoft.AspNetCore.Identity;
using Voupon.Merchant.WebApp.Common.Services.ProductOutlets.Queries;
using Voupon.Merchant.WebApp.Common.Services.ProductOutlets.Commands;
using Voupon.Merchant.WebApp.Common.Services.ProductCategories.Commands;
using Voupon.Merchant.WebApp.Common.Services.ProductSubcategories.Commands;
using Voupon.Merchant.WebApp.Common.Services.ProductSubcategories.Commandspdate;
using Voupon.Merchant.WebApp.Common.Services.DealExpirations.Commands;
using Voupon.Merchant.WebApp.Common.Services.Blob.Commands.Delete;
using Voupon.Merchant.WebApp.Common.Services.Blob.Queries;
using Voupon.Common.Enum;
using Voupon.Merchant.WebApp.Common.Services.Products.Models;
using Voupon.Merchant.WebApp.Common.Services.DiscountTypes.Queries;
using Voupon.Merchant.WebApp.Common.Services.ProductDiscounts.Queries;
using Voupon.Merchant.WebApp.Common.Services.ProductDiscounts.Command;
using Voupon.Merchant.WebApp.Common.Services.LuckyDraws.Queries;
using Voupon.Merchant.WebApp.Common.Services.Postcodes.Queries;
using Voupon.Merchant.WebApp.Common.Services.Districts.Queries;
using Voupon.Merchant.WebApp.Common.Services.Provinces.Queries;
using Voupon.Merchant.WebApp.Common.Services.Countries.Queries;
using Voupon.Merchant.WebApp.Areas.Admin.ViewModels.Products;
using Voupon.Merchant.WebApp.Common.Services.Outlets.Commands;
using Voupon.Merchant.WebApp.Infrastructure.Enums;
using Voupon.Merchant.WebApp.Areas.Admin.Services.Products.Pages;
using Voupon.Merchant.WebApp.Common.Services.Orders.Queries;
using static Voupon.Merchant.WebApp.Common.Services.Orders.Queries.OrderByDateRangeQuery;
using static Voupon.Merchant.WebApp.Common.Services.OrderItemsExternal.Queries.ExternalOrderItemQuery;
using Voupon.Merchant.WebApp.Common.Services.OrderItemsExternal.Commands;
using static Voupon.Merchant.WebApp.Common.Services.OrderItemsExternal.Queries.ExternalOrderDetailsByOrderItmeShortIdQueryHandler;
using Voupon.Merchant.WebApp.Common.Services.OrderItemsExternal.Queries;
using System.IO;
using OfficeOpenXml;
using Voupon.Merchant.WebApp.Common.Services.Orders.Commands;
using Voupon.Merchant.WebApp.Common.Services.Utility.Queries;
using static Voupon.Merchant.WebApp.Common.Services.OrderItemsExternal.Queries.OrderDetailsByOrderIdQueryHandler;
using Voupon.Merchant.WebApp.Common.Services.OrderItems.Commands;
using static Voupon.Merchant.WebApp.Common.Services.Orders.Queries.RefundOrderItemForMerchantsQuery;
using OfficeOpenXml.Core.ExcelPackage;
using Voupon.Merchant.WebApp.Infrastructure.Extensions;

namespace Voupon.Merchant.WebApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/[controller]")]
    [Authorize(Roles = "Admin")]
    public class SalesController : BaseAdminController
    {
        [Route("CheckPayment")]
        public IActionResult CheckPayment()
        {
            return View();
        }

        [Route("Search")]
        public IActionResult Search()
        {
            return View();
        }

        [Route("Refunds")]
        public IActionResult Refunds()
        {
            return View();
        }
        
        [Route("MerchantRefunds")]
        public IActionResult MerchantRefunds()
        {
            return View();
        }

        [Route("AggregatorOrders")]
        public IActionResult AggregatorOrders()
        {
            return View();
        }

        [HttpGet]
        [Route("GetOrderDetailsByOrderId")]
        public async Task<ApiResponseViewModel> GetOrderDetailsByOrderId(string orderId)
        {

            ApiResponseViewModel response = new ApiResponseViewModel();
            response = await Mediator.Send(new OrderDetailsByOrderIdQuery() { OrderId = orderId });
            if (response.Successful)
            {
                var data = (OrderDetailsModel)response.Data;
                //data.Order.OrderItems = null;
                response.Data = data;
            }
            return response;
        }

        [HttpGet]
        [Route("GetOrderDetailsForMerchantOrderByOrderId")]
        public async Task<ApiResponseViewModel> GetOrderDetailsForMerchantOrderByOrderId(string orderId)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();
            response = await Mediator.Send(new OrderDetailsByOrderItmeIdForMerchantRefundQuery() { OrderId = orderId});
            if (response.Successful)
            {
                var data = (OrderDetailsModel)response.Data;
                //data.Order.OrderItems = null;
                response.Data = data;
            }
            return response;
        }

        [HttpPost]
        [Route("RefundMerchantOrderItem")]
        public async Task<ApiResponseViewModel> RefundMerchantOrderItem(CreateOrderItemWithQuantityPerMerchantAdminRefund command)
        {
            ApiResponseViewModel apiResponseViewModel = new ApiResponseViewModel();
            
            command.IsAdminForMerchantRefund = true;
            command.UserId = new Guid(User.Identity.GetUserId());

            
            return await Mediator.Send(command);
        }

        [HttpGet]
        [Route("GetOrderDetailsByShortOrderId")]
        public async Task<ApiResponseViewModel> GetOrderDetailsByShortOrderId(string externalOrderId)
        {

            ApiResponseViewModel response = new ApiResponseViewModel();
            response = await Mediator.Send(new ExternalOrderDetailsByOrderItmeShortIdQuery() { ExternalOrderId = externalOrderId });
            if (response.Successful)
            {
                var data = (ExternalOrderDetailsModel)response.Data;
                //data.Order.OrderItems = null;
                response.Data = data;
            }
            return response;
        }
        [HttpGet]
        [Route("GetOrderDetailsForMerchatOrderByShortOrderId")]
        public async Task<ApiResponseViewModel> GetOrderDetailsForMerchatOrderByShortOrderId(string shortOrderItemId)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();
            response = await Mediator.Send(new OrderDetailsWithOrderShortIdQuery() { ShortOrderItemId = shortOrderItemId });
            if (response.Successful)
            {
                var data = (OrderItemDetailsViewModel)response.Data;
                data.OrderItem.Order.OrderItems = null;
                data.OrderItem.Refunds = null;
                response.Data = data;
            }
            return response;
        }

        [HttpGet]
        [Route("get-external-order-status-list")]
        public async Task<ApiResponseViewModel> GetExternalOrderStatusList()
        {

            ApiResponseViewModel response = new ApiResponseViewModel();
            response = await Mediator.Send(new GetExternalOrderStatusListQuery());
            return response;
        }

        [HttpPost]
        [Route("Update-external-order-status-by-order-id")]
        public async Task<ApiResponseViewModel> UpdateExternalOrderStatusByOrderId(string externalOrderId, byte statusId)
        {

            ApiResponseViewModel response = new ApiResponseViewModel();
            response = await Mediator.Send(new UpdateExternalOrderStatusByOrderIdCommand() { ExternalOrderId = new Guid(externalOrderId), StatusId = statusId, LastUpdatedByUserName = (User.Identity.GetUserName()) });

            return response;
        }

        [HttpPost]
        [Route("Update-external-order-id-by-shop-id")]
        public async Task<ApiResponseViewModel> UpdateExternalOrderIdByShopId(string externalOrderShopId, string orderExternalId)
        {

            ApiResponseViewModel response = new ApiResponseViewModel();
            response = await Mediator.Send(new UpdateExternalOrderIdByShopIdCommand() { ExternalOrderShopId = new Guid(externalOrderShopId), LastUpdatedByUserName = (User.Identity.GetUserName()), ExternalOrderId = orderExternalId });

            return response;
        }

        [HttpPost]
        [Route("Update-external-order-status-by-shop-id")]
        public async Task<ApiResponseViewModel> UpdateExternalOrderStatusByShopId(string externalOrderShopId, byte statusId, string orderExternalId)
        {

            ApiResponseViewModel response = new ApiResponseViewModel();
            response = await Mediator.Send(new UpdateExternalOrderStatusByShopIdCommand() { ExternalOrderShopId = new Guid(externalOrderShopId), StatusId = statusId, LastUpdatedByUserName = (User.Identity.GetUserName()), ExternalOrderId = orderExternalId });

            return response;
        }

        [HttpPost]
        [Route("Update-external-order-status")]
        public async Task<ApiResponseViewModel> UpdateExternalOrderStatus(string externalOrderId, byte statusId, string trakingNo)
        {

            ApiResponseViewModel response = new ApiResponseViewModel();
            response = await Mediator.Send(new UpdateExternalOrderStatusCommand() { ExternalOrderId = new Guid(externalOrderId), StatusId = statusId, TrakingNo = trakingNo, LastUpdatedByUserName = (User.Identity.GetUserName()) });

            return response;
        }

        [HttpPost]
        [Route("Update-merchant-order-item-status")]
        public async Task<ApiResponseViewModel> UpdateMerchantOrderItmeStatus(string orderItemId, byte statusId)
        {

            ApiResponseViewModel response = new ApiResponseViewModel();
            response = await Mediator.Send(new UpdateMerchantOrderItemStatusCommand() { OrderItemId = new Guid(orderItemId), StatusId = statusId });

            return response;
        }

        [HttpGet]
        [Route("Aggregator-Items")]
        public async Task<ApiResponseViewModel> GetPendingActionItems(string start, string length, string searchValue, DateTime from, DateTime to, byte websiteTypeId, bool isCompleted)
        {

            try
            {
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt32(start) : 0;
                int recordsTotal = 0;
                ApiResponseViewModel response = await Mediator.Send(new ExternalOrderItemQuery() { From = from, To = to, WebsiteTypeId = websiteTypeId, IsConmpleted = isCompleted });
                if (response.Successful)
                {
                    var externalOrderItems = (List<OrderShopExternalViewModel>)response.Data;

                    if (!string.IsNullOrEmpty(searchValue))
                    {
                        externalOrderItems = externalOrderItems.Where(m => m.OrderItemViewModels.Any(x => x.ProductTitle.ToLower().Contains(searchValue))
                        || (m.OrderItemViewModels.Any(x => x.ProductTitle != null) ? m.OrderItemViewModels.Any(x => x.ProductTitle.ToString().Contains(searchValue)) : false)
                        || (m.OrderItemViewModels.Any(x => x.ShortOrderId != null) ? m.OrderItemViewModels.Any(x => x.ShortOrderId.Contains(searchValue)) : false)
                        ).ToList();

                    }
                    recordsTotal = externalOrderItems.Count();
                    //var data = externalOrderItems.Skip(skip).Take(pageSize).ToList();
                    var groupedByOrderData = externalOrderItems.GroupBy(x => x.OrderId);
                    var OrdersList = new List<ExternalOrdersModel>();
                    foreach (var orderData in groupedByOrderData)
                    {
                        var model = new ExternalOrdersModel()
                        {
                            OrderShortId = orderData.FirstOrDefault().OrderShortId,
                            OrderId = orderData.Key,
                            CreatedAt = orderData.FirstOrDefault().CreatedAt,
                            Email = orderData.FirstOrDefault().Email,
                            LastUpdatedAt = orderData.ToList().Max(x => x.LastUpdatedAt),
                            LastUpdatedByUserId = orderData.ToList().Where(x => x.LastUpdatedAt == orderData.ToList().Max(x => x.LastUpdatedAt)).FirstOrDefault() != null ? orderData.ToList().Where(x => x.LastUpdatedAt == orderData.ToList().Max(x => x.LastUpdatedAt)).FirstOrDefault().LastUpdatedByUserId : null,
                            Revenue = orderData.ToList().Sum(x => x.Revenue) + orderData.ToList().Sum(x => x.ShippingCost),
                            Paid = orderData.ToList().FirstOrDefault().Paid,
                            Status = orderData.ToList().Min(x => x.OverAllItemsStatus),
                            PromoCodeDiscount = orderData.FirstOrDefault().PromoCodeDiscount,
                            PromoCodeName = orderData.FirstOrDefault().PromoCodeName,
                            orderShopExternalViewModel = orderData.ToList(),
                            ShippingCost = orderData.ToList().Sum(x => x.ShippingCost),
                            VpointsDiscount = orderData.ToList().Sum(x => x.VpointsDiscount),
                            
                        };
                        if (orderData.Select(x => x.OrderStatus).FirstOrDefault() == 1)
                        {
                            model.Status = 7;
                            continue;
                        }
                        OrdersList.Add(model);
                    }
                    var data = OrdersList.Skip(skip).Take(pageSize).ToList();
                    var jsonData = new { recordsFiltered = data.Count(), recordsTotal = recordsTotal, data = data };
                    response.Data = jsonData;
                    return response;
                }
                return response;
            }
            catch (Exception ex)
            {
                ApiResponseViewModel badRequestResponse = new ApiResponseViewModel();
                badRequestResponse.Successful = false;
                badRequestResponse.Message = ex.Message;
                return badRequestResponse;
            }
        }

        [HttpPost]
        [Route("Download-excel")]
        public async Task<IActionResult> DownloadExcel(string start, string length, string searchValue, DateTime from, DateTime to, byte websiteTypeId)
        {

            try
            {
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt32(start) : 0;
                ApiResponseViewModel response = await Mediator.Send(new ExternalOrderItemQuery() { From = from, To = to, WebsiteTypeId = websiteTypeId });
                if (response.Successful)
                {


                    var shop = (List<OrderShopExternalViewModel>)response.Data;


                    var externalOrderItems = shop.SelectMany(x => x.OrderItemViewModels).Select(z => new ExternalOrderItemViewModel
                    {
                        ExternalUrl = z.ExternalUrl,
                        Id = z.Id,
                        ProductTitle = z.ProductTitle,
                        Quantity = z.Quantity,
                        Revenue = z.Revenue,
                        ShortOrderId = z.ShortOrderId,
                        Status = z.Status,
                        Variant = z.Variant,
                        LastUpdatedAt = z.LastUpdatedAt,
                        LastUpdatedByUserId = z.LastUpdatedByUserId
                    });

                    if (!string.IsNullOrEmpty(searchValue))
                    {
                        externalOrderItems = externalOrderItems.Where(m => m.ProductTitle.ToLower().Contains(searchValue)
                        || (m.ProductTitle != null ? m.ProductTitle.ToString().Contains(searchValue) : false)
                        || (m.ShortOrderId != null ? m.ShortOrderId.Contains(searchValue) : false)
                        ).ToList();

                    }

                    //  TODO
                    /*
                    using (var excel = new ExcelPackage()
                    {
                        var wks = excel.Workbook.Worksheets.Add("StudentReportCard");
                        wks.Cells[1, 1].LoadFromCollection(externalOrderItems, PrintHeaders: true);
                        return File(excel.GetAsByteArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"{from}_{to}_{websiteTypeId}.xlsx");
                    };
                    */
                    return null;

                }
                else
                {
                    return StatusCode(501);
                }
            }
            catch (Exception ex)
            {
                ApiResponseViewModel badRequestResponse = new ApiResponseViewModel();
                badRequestResponse.Successful = false;
                badRequestResponse.Message = ex.Message;
                return StatusCode(500, ex);
            }
        }

        [HttpGet]
        [Route("Refund-Items")]
        public async Task<ApiResponseViewModel> GetRefundnItems(string start, string length, string searchValue)
        {
            try
            {
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt32(start) : 0;
                int recordsTotal = 0;
                ApiResponseViewModel response = await Mediator.Send(new RefundOrderItemForMerchantsQuery());
                if (response.Successful && response.Data != null)
                {


                    var pendingSalesData = (RefundViewModel)response.Data;
                    var pendingSalesList = pendingSalesData.PendingHandlingItems;

                    if (!string.IsNullOrEmpty(searchValue))
                    {
                        pendingSalesList = pendingSalesList.Where(m => m.ProductTitle.ToLower().Contains(searchValue)
                        || (m.OrderItemId != null ? m.OrderItemId.ToString().Contains(searchValue) : false)
                        || (m.ShortOrderId != null ? m.ShortOrderId.Contains(searchValue) : false)
                        ).ToList();

                    }
                    recordsTotal = pendingSalesList.Count();




                    var groupedByOrderData = pendingSalesList.GroupBy(x => x.OrderId);
                    var OrdersList = new List<RefundsViewModel>();
                    foreach (var orderData in groupedByOrderData)
                    {
                        var model = new RefundsViewModel()
                        {
                            OrderId = orderData.Key,
                            CreatedAt = orderData.FirstOrDefault().CreatedAt,
                            Email = orderData.FirstOrDefault().Email,
                            //LastUpdatedAt = orderData.ToList().Max(x => x.LastUpdatedAt),
                            //LastUpdatedByUserId = orderData.ToList().Where(x => x.LastUpdatedAt == orderData.ToList().Max(x => x.LastUpdatedAt)).FirstOrDefault() != null ? orderData.ToList().Where(x => x.LastUpdatedAt == orderData.ToList().Max(x => x.LastUpdatedAt)).FirstOrDefault().LastUpdatedByUserId : null,
                            Revenue = orderData.ToList().Sum(x => x.Revenue.HasValue == true ? x.Revenue.Value : 0),
                            Status = orderData.ToList().Min(x => x.Status),
                            ShippingAddress = orderData.FirstOrDefault().ShippingAddress,
                            ShippingName = orderData.FirstOrDefault().ShippingName,
                            PromoCodeDiscount = orderData.FirstOrDefault().PromoCodeDiscount,
                            PromoCodeName = orderData.FirstOrDefault().PromoCodeName,
                            pendingHandlingItemViewModels = orderData.ToList()
                        };
                        //if (orderData.Select(x => x.OrderStatus).FirstOrDefault() == 1)
                        //{
                        //    model.Status = 7;
                        //    continue;
                        //}
                        if (model.Status == 5 && orderData.ToList().Where(x => x.Status > 7).Count() > 0)
                        {
                            model.Status = orderData.ToList().Where(x => x.Status > 7).Min(x => x.Status);
                        }
                        OrdersList.Add(model);
                    }

                    //foreach(var order in OrdersList)
                    //{
                    //    foreach(var orderItem in order.pendingHandlingItemViewModels)
                    //    {
                    //        if(order.pendingHandlingItemViewModels.Where(x=> x.ProductId == orderItem.ProductId && x.Type == 3).ToList().Count > 0)
                    //        {

                    //        }
                    //    }
                    //}






                    var data = OrdersList.Skip(skip).Take(pageSize).ToList();
                    //pendingSalesData.PendingHandlingItems = data;
                    var jsonData = new { recordsFiltered = data.Count(), recordsTotal = recordsTotal, data = data };
                    response.Data = jsonData;
                    return response;
                }
                return response;
            }
            catch (Exception ex)
            {
                ApiResponseViewModel badRequestResponse = new ApiResponseViewModel();
                badRequestResponse.Successful = false;
                badRequestResponse.Message = ex.Message;
                return badRequestResponse;
            }
        }

        [HttpPost]
        [Route("Refund")]
        public async Task<ApiResponseViewModel> Refund(CreateOrderExternalItemRefundCommand command)
        {
            ApiResponseViewModel apiResponseViewModel = new ApiResponseViewModel();
            command.UserId = new Guid(User.Identity.GetUserId());
            return await Mediator.Send(command);
        }

        [HttpPost]
        [Route("MerchantsRefunds")]
        public async Task<ApiResponseViewModel> MerchantsRefunds(CreateOrderExternalItemRefundCommand command)
        {
            ApiResponseViewModel apiResponseViewModel = new ApiResponseViewModel();
            command.UserId = new Guid(User.Identity.GetUserId());
            return await Mediator.Send(command);
        }

        [HttpPost]
        [Route("FullRefund")]
        public async Task<ApiResponseViewModel> FullRefund(CreateOrderFullRefundCommand command)
        {
            ApiResponseViewModel apiResponseViewModel = new ApiResponseViewModel();
            command.UserId = new Guid(User.Identity.GetUserId());
            command.IsAdminForMerchantRefund = true;
            return await Mediator.Send(command);
        }

        [HttpGet]
        [Route("Get-Shippind-Details-By-Order-ShopId")]
        public async Task<ApiResponseViewModel> GetShippindDetailsByOrderShopId(string orderShopId)
        {
            var query = new GetExternalOrderShippindDetailsByOrderShopId
            {
                OrderShopId = new Guid(orderShopId)
            };
            // To Add treating of the json data and passing the Json as well as the status


            return await Mediator.Send(query);
        }


        [HttpGet]
        [Route("refunds-by-date/{from}/{to}")]
        public async Task<ApiResponseViewModel> RefundByDate(DateTime from, DateTime to)
        {
            var query = new RefundByDateQuery
            {
                MerchantId = 0,
                From = from,
                To = to
            };

            return await Mediator.Send(query);
        }

        [HttpGet]
        [Route("sales-by-date/{from}/{to}")]
        public async Task<ApiResponseViewModel> SalesByDate(DateTime from, DateTime to, string start, string length)
        {
            try
            {
                var query = new OrderByDateRangeQuery
                {
                    From = from,
                    To = to
                };
                ApiResponseViewModel apiResponse = await Mediator.Send(query);
                return apiResponse;
                /*
                if (apiResponse.Successful)
                {
                    int pageSize = length != null ? Convert.ToInt32(length) : 0;
                    int skip = start != null ? Convert.ToInt32(start) : 0;
                    int recordsTotal = 0;
                    var salesList = (List<OrderViewModel>)apiResponse.Data;

                    recordsTotal = salesList.Count();
                    var data = salesList.Skip(skip).Take(pageSize).ToList();
                    var jsonData = new { recordsFiltered = data.Count(), recordsTotal = recordsTotal, data = data };
                    apiResponse.Data = jsonData;
                    return apiResponse;
                }
                return apiResponse;
                */
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
        [Route("sales-by-short-id/{id}")]
        public async Task<ApiResponseViewModel> SalesByShortId(string id)
        {
            var query = new OrderByShortIdQuery
            {
                Id = id
            };

            return await Mediator.Send(query);
        }


    }
}