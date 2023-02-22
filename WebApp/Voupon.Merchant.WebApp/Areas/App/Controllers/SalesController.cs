using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SendGrid;
using SendGrid.Helpers.Mail;
using Voupon.Merchant.WebApp.Common.Services.InStoreRedemptionTokens.Queries;
using Voupon.Merchant.WebApp.Common.Services.DigitalRedemptionTokens.Queries;
using Voupon.Merchant.WebApp.ViewModels;
using Voupon.Merchant.WebApp.Common.Services.DeliveryRedemptionTokens.Queries;
using Voupon.Common.QRGenerator;
using Voupon.Merchant.WebApp.Common.Services.DigitalRedemptionTokens.Commands;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Merchant.WebApp.Common.Services.DeliveryRedemptionTokens.Commands;
using Voupon.Merchant.WebApp.Common.Services.Merchants.Models;
using Voupon.Merchant.WebApp.Common.Services.InStoreRedemptionTokens.Commands;
using Voupon.Merchant.WebApp.Common.Services.Orders.Queries;
using Voupon.Merchant.WebApp.Areas.App.Services.GifteeVouchers.Queries.List;
using Voupon.Merchant.WebApp.Areas.App.Services.GifteeVouchers.Commands;
using Voupon.Merchant.WebApp.Areas.App.Services.GifteeVouchers.Commands.Create;
using Voupon.Merchant.WebApp.Common.Services.SalesHistoryRedemptionTokens.Queries;
using Voupon.Merchant.WebApp.Common.Services.Utility.Queries;
using Voupon.Merchant.WebApp.Common.Services.Orders.Commands;
using static Voupon.Merchant.WebApp.Common.Services.Orders.Queries.PendingHandlingOrderItemQuery;
using System.Linq.Dynamic.Core;
using Voupon.Merchant.WebApp.Common.Services.OrderItemsExternal.Queries;
using static Voupon.Merchant.WebApp.Common.Services.OrderItemsExternal.Queries.OrderDetailsByOrderIdQueryHandler;
using static Voupon.Merchant.WebApp.Common.Services.Orders.Queries.RefundOrderItemQuery;
using Voupon.Merchant.WebApp.Areas.App.Services.GifteeVouchers.Queries.Single;
using System.IO;
using static Voupon.Merchant.WebApp.Areas.App.Services.GifteeVouchers.Queries.Single.GifteeProductQuery;
using System.Net;
using Voupon.Merchant.WebApp.Infrastructure.Extensions;

namespace Voupon.Merchant.WebApp.Areas.App.Controllers
{
    public class SalesHistoryRedemptionReport
    {
        public string MerchantName { get; set; }
        public string TotalRevenue { get; set; }
        public string Date { get; set; }
        public List<SalesHistoryRedemptionTokens> RedemptionList { get; set; }
    }

    public class SalesHistoryRedemptionTokens
    {
        public string ProductTransID { get; set; }
        public string ProductName { get; set; }
        public string Revenue { get; set; }
        public string PurchasedDate { get; set; }
        public string RedeemedDate { get; set; }
        public string Outlet { get; set; }
        public string Type { get; set; }
    }
    public class OutletRedemptionReport
    {
        public string MerchantName { get; set; }
        public string TotalRevenue { get; set; }
        public string Date { get; set; }
        public List<InStoreRedemptionTokens> RedemptionList { get; set; }
    }

    public class DigitalRedemptionReport
    {
        public string MerchantName { get; set; }
        public string TotalRevenue { get; set; }
        public string Date { get; set; }
        public List<DigitalRedemptionTokens> RedemptionList { get; set; }
    }

    public class DeliveryRedemptionReport
    {
        public string MerchantName { get; set; }
        public string TotalRevenue { get; set; }
        public string Date { get; set; }
        public List<DeliveryRedemptionTokens> RedemptionList { get; set; }
    }

    public class PendingSalesRedemptionTokens
    {
        public int Id { get; set; }

        public int ProductId { get; set; }
        public Guid OrderItemId { get; set; }
        public string ProductTitle { get; set; }
        public string Token { get; set; }
        public bool IsRedeemed { get; set; }
        public bool IsActivated { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? RedeemedAt { get; set; }
        public decimal? Revenue { get; set; }
        public string Email { get; set; }
        public string Status { get; set; }
        public int Type { get; set; }



        public string ShortOrderItemId { get; set; }
        public string ShortOrderId { get; set; }
    }

    [Area("App")]
    [Route("App/[controller]")]
    [Authorize(Roles = "Merchant,Staff")]
    public class SalesController : BaseAppController
    {
        public async Task<IActionResult> Index()
        {
            var userid = User.Identity.GetUserId();
            var MerchantId = User.Claims.FirstOrDefault(x => x.Type == "MerchantId").Value;

            if (String.IsNullOrEmpty(MerchantId))
            {
                return View(Infrastructure.Enums.ErrorPageEnum.NOT_ALLOWED_PAGE);
                // return BadRequest();
            }
            var merchantId = Int32.Parse(MerchantId);
            return View(merchantId);
        }
        [Route("PendingAction")]

        public IActionResult PendingAction()
        {
            var userid = User.Identity.GetUserId();
            var MerchantId = User.Claims.FirstOrDefault(x => x.Type == "MerchantId").Value;

            if (String.IsNullOrEmpty(MerchantId))
            {
                return View(Infrastructure.Enums.ErrorPageEnum.NOT_ALLOWED_PAGE);
                // return BadRequest();
            }
            var merchantId = Int32.Parse(MerchantId);
            return View(merchantId);
        }

        [Route("Refunds")]
        public IActionResult Refunds()
        {
            var userid = User.Identity.GetUserId();
            var MerchantId = User.Claims.FirstOrDefault(x => x.Type == "MerchantId").Value;

            if (String.IsNullOrEmpty(MerchantId))
            {
                return View(Infrastructure.Enums.ErrorPageEnum.NOT_ALLOWED_PAGE);
                // return BadRequest();
            }
            var merchantId = Int32.Parse(MerchantId);
            return View(merchantId);
        }

        [HttpGet]
        [Route("refunds-by-date/{from}/{to}")]
        public async Task<ApiResponseViewModel> RefundByDate(DateTime from, DateTime to)
        {
            var merchantId = int.Parse(User.Claims.FirstOrDefault(x => x.Type == "MerchantId").Value);
            var query = new RefundByDateQuery
            {
                MerchantId = merchantId,
                From = from,
                To = to
            };

            return await Mediator.Send(query);
        }

        [Route("Search")]
        public IActionResult Search()
        {
            return View();
        }


        [HttpGet]
        [Route("order-item-by-date/{from}/{to}")]
        public async Task<ApiResponseViewModel> OrderItemByDate(DateTime from, DateTime to)
        {
            var merchantId = int.Parse(User.Claims.FirstOrDefault(x => x.Type == "MerchantId").Value);
            var query = new OrderItemByDateRangeQuery
            {
                From = from,
                To = to,
                MerchantId = merchantId
            };

            return await Mediator.Send(query);
        }

        [HttpGet]
        [Route("order-item-by-short-id/{id}")]
        public async Task<ApiResponseViewModel> OrderItemByShortId(string id)
        {
            var merchantId = int.Parse(User.Claims.FirstOrDefault(x => x.Type == "MerchantId").Value);
            var query = new OrderItemByShortIdQuery
            {
                Id = id,
            };

            return await Mediator.Send(query);
        }

        [HttpGet]
        [Route("order-item-by-order-short-id/{id}")]
        public async Task<ApiResponseViewModel> OrderItemByOrderShortId(string id)
        {
            var merchantId = int.Parse(User.Claims.FirstOrDefault(x => x.Type == "MerchantId").Value);
            var query = new OrderItemByShortOrderIdQuery
            {
                Id = id,
                MerchantId = merchantId
            };

            return await Mediator.Send(query);
        }

        [HttpGet]
        [Route("order-item-by-email/{email}")]
        public async Task<ApiResponseViewModel> OrderItemByEmail(string email)
        {
            var merchantId = int.Parse(User.Claims.FirstOrDefault(x => x.Type == "MerchantId").Value);
            var query = new OrderItemByEmailQuery
            {
                Email = email,
                MerchantId = merchantId
            };

            return await Mediator.Send(query);
        }

        [Route("Outlet")]
        public async Task<IActionResult> Outlet()
        {
            var userid = User.Identity.GetUserId();
            var MerchantId = User.Claims.FirstOrDefault(x => x.Type == "MerchantId").Value;

            if (String.IsNullOrEmpty(MerchantId))
            {
                return View(Infrastructure.Enums.ErrorPageEnum.NOT_ALLOWED_PAGE);
                //return BadRequest();
            }
            var merchantId = Int32.Parse(MerchantId);

            //await Mediator.Send(new CreateInStoreRedemptionTokenCommand()
            //{
            //    MerchantId = merchantId,
            //    StartDate = DateTime.Now,
            //    ExpiredDate = DateTime.Now.AddDays(10),
            //    ProductTitle = "Product A",
            //    MasterMemberProfileId = 2,
            //    OrderItemId = Guid.NewGuid(),
            //    RedemptionInfo = "",
            //    Revenue = 20,
            //    ProductId = 34,
            //    Email = "fong@vodus.com"
            //});

            return View(merchantId);
        }

        [Route("Digital")]
        public async Task<IActionResult> Digital()
        {
            var userid = User.Identity.GetUserId();
            var MerchantId = User.Claims.FirstOrDefault(x => x.Type == "MerchantId").Value;

            if (String.IsNullOrEmpty(MerchantId))
            {
                return View(Infrastructure.Enums.ErrorPageEnum.NOT_ALLOWED_PAGE);
                // return BadRequest();
            }
            var merchantId = Int32.Parse(MerchantId);
            //await Mediator.Send(new CreateDigitalRedemptionTokensCommand()
            //{
            //    MerchantId = merchantId,
            //    StartDate = DateTime.Now,
            //    ExpiredDate = DateTime.Now.AddDays(10),
            //    ProductTitle = "Digital Product A",
            //    MasterMemberProfileId = 2,
            //    OrderItemId = Guid.NewGuid(),
            //    RedemptionInfo = "",
            //    Revenue = 20,
            //    ProductId = 34,
            //    Email = "fong@vodus.com"
            //});
            return View(merchantId);
        }

        [Route("GifteeProducts")]
        public async Task<IActionResult> GifteeProducts()
        {
            var userid = User.Identity.GetUserId();
            var MerchantId = User.Claims.FirstOrDefault(x => x.Type == "MerchantId").Value;

            if (String.IsNullOrEmpty(MerchantId) || MerchantId != "1")
            {
                return View(Infrastructure.Enums.ErrorPageEnum.NOT_ALLOWED_PAGE);
                // return BadRequest();
            }

            var merchantId = Int32.Parse(MerchantId);

            return View(merchantId);
        }
        [Route("GifteeVouchers")]
        public async Task<IActionResult> GifteeVouchers()
        {
            var userid = User.Identity.GetUserId();
            var MerchantId = User.Claims.FirstOrDefault(x => x.Type == "MerchantId").Value;

            if (String.IsNullOrEmpty(MerchantId) || MerchantId != "1")
            {
                return View(Infrastructure.Enums.ErrorPageEnum.NOT_ALLOWED_PAGE);
                // return BadRequest();
            }

            var merchantId = Int32.Parse(MerchantId);

            return View(merchantId);
        }

        [Route("Delivery")]
        public async Task<IActionResult> Delivery()
        {
            var userid = User.Identity.GetUserId();
            var MerchantId = User.Claims.FirstOrDefault(x => x.Type == "MerchantId").Value;

            if (String.IsNullOrEmpty(MerchantId))
            {
                return View(Infrastructure.Enums.ErrorPageEnum.NOT_ALLOWED_PAGE);
                // return BadRequest();
            }
            var merchantId = Int32.Parse(MerchantId);
            //await Mediator.Send(new CreateDeliveryRedemptionTokensCommand()
            //{
            //    MerchantId = merchantId,
            //    StartDate = DateTime.Now,
            //    ExpiredDate = DateTime.Now.AddDays(10),
            //    ProductTitle = "Delivery Product A",
            //    MasterMemberProfileId = 2,
            //    OrderItemId = Guid.NewGuid(),
            //    RedemptionInfo = "",
            //    Revenue = 20,
            //    ProductId = 34,
            //    Email="fong@vodus.com"
            //});
            return View(merchantId);
        }

        [Route("SalesHistoryRedemptionReport")]
        public async Task<IActionResult> SalesHistoryRedemptionReport(string startDate, string endDate)
        {
            var userid = User.Identity.GetUserId();
            var MerchantId = User.Claims.FirstOrDefault(x => x.Type == "MerchantId").Value;

            if (String.IsNullOrEmpty(MerchantId))
            {
                return View(Infrastructure.Enums.ErrorPageEnum.NOT_ALLOWED_PAGE);
                // return BadRequest();
            }
            var merchantId = Int32.Parse(MerchantId);

            var merchantResponse = await Mediator.Send(new Common.Services.Merchants.Queries.MerchantQuery() { MerchantId = merchantId });
            if (!merchantResponse.Successful)
            {
                return View(Infrastructure.Enums.ErrorPageEnum.INVALID_REQUEST_PAGE);
                //return BadRequest();
            }
            //Product/ TransID /Product Name /Revenue(RM) /Purchased Date /Redeemed Date /Outlet
            MerchantModel merchant = (MerchantModel)merchantResponse.Data;
            SalesHistoryRedemptionReport report = new SalesHistoryRedemptionReport();
            report.MerchantName = merchant.DisplayName;
            report.TotalRevenue = "RM 0.00";
            report.Date = "";
            report.RedemptionList = new List<SalesHistoryRedemptionTokens>();
            ApiResponseViewModel response = new ApiResponseViewModel();
            DateTime StartDate = DateTime.Parse(startDate);
            DateTime EndDate = DateTime.Parse(endDate).AddHours(23).AddMinutes(59).AddSeconds(59);
            report.Date = StartDate.ToString("dd/MM/yyyy") + " ~ " + EndDate.ToString("dd/MM/yyyy");
            decimal revenue = 0;
            //Outlet
            response = await Mediator.Send(new InStoreRedemptionWithDateQuery() { MerchantId = merchantId, StartDate = StartDate, EndDate = EndDate });
            if (response.Successful)
            {
                List<InStoreRedemptionTokens> redemptionTokens = (List<InStoreRedemptionTokens>)response.Data;
                foreach (var item in redemptionTokens)
                {
                    SalesHistoryRedemptionTokens salesHistoryRedemptionTokens = new SalesHistoryRedemptionTokens();
                    salesHistoryRedemptionTokens.ProductName = item.ProductTitle;
                    salesHistoryRedemptionTokens.ProductTransID = item.CreatedAt.ToString("yyMMdd") + item.OrderItemId.ToString().Split("-").First().ToUpper();
                    salesHistoryRedemptionTokens.Revenue = item.Revenue.Value.ToString("F2");
                    salesHistoryRedemptionTokens.PurchasedDate = item.CreatedAt.ToString("dd/MM/yyyy");
                    salesHistoryRedemptionTokens.RedeemedDate = item.RedeemedAt.Value.ToString("dd/MM/yyyy");
                    if (salesHistoryRedemptionTokens.Outlet != null)
                    {
                        salesHistoryRedemptionTokens.Outlet = item.Outlet.Name;
                    }
                    else
                    {
                        salesHistoryRedemptionTokens.Outlet = "-";
                    }
                    salesHistoryRedemptionTokens.Type = "Outlet";

                    report.RedemptionList.Add(salesHistoryRedemptionTokens);
                }
                revenue = redemptionTokens.Sum(x => x.Revenue).Value;
            }
            else
            {
                return View(Infrastructure.Enums.ErrorPageEnum.INVALID_REQUEST_PAGE);
            }

            //Digital
            response = await Mediator.Send(new DigitalRedemptionWithDateQuery() { MerchantId = merchantId, StartDate = StartDate, EndDate = EndDate });
            if (response.Successful)
            {
                List<DigitalRedemptionTokens> redemptionTokens = (List<DigitalRedemptionTokens>)response.Data;
                foreach (var item in redemptionTokens)
                {
                    SalesHistoryRedemptionTokens salesHistoryRedemptionTokens = new SalesHistoryRedemptionTokens();
                    salesHistoryRedemptionTokens.ProductName = item.ProductTitle;
                    salesHistoryRedemptionTokens.ProductTransID = item.CreatedAt.ToString("yyMMdd") + item.OrderItemId.ToString().Split("-").First().ToUpper();
                    salesHistoryRedemptionTokens.Revenue = item.Revenue.Value.ToString("F2");
                    salesHistoryRedemptionTokens.PurchasedDate = item.CreatedAt.ToString("dd/MM/yyyy");
                    salesHistoryRedemptionTokens.RedeemedDate = item.RedeemedAt.Value.ToString("dd/MM/yyyy");
                    salesHistoryRedemptionTokens.Outlet = "-";
                    salesHistoryRedemptionTokens.Type = "E-voucher";


                    report.RedemptionList.Add(salesHistoryRedemptionTokens);
                }
                revenue += redemptionTokens.Sum(x => x.Revenue).Value;
            }
            else
            {
                return View(Infrastructure.Enums.ErrorPageEnum.INVALID_REQUEST_PAGE);
            }

            //Delivery
            response = await Mediator.Send(new DeliveryRedemptionWithDateQuery() { MerchantId = merchantId, StartDate = StartDate, EndDate = EndDate });
            if (response.Successful)
            {
                List<DeliveryRedemptionTokens> redemptionTokens = (List<DeliveryRedemptionTokens>)response.Data;
                foreach (var item in redemptionTokens)
                {
                    SalesHistoryRedemptionTokens salesHistoryRedemptionTokens = new SalesHistoryRedemptionTokens();
                    salesHistoryRedemptionTokens.ProductName = item.ProductTitle;
                    salesHistoryRedemptionTokens.ProductTransID = item.CreatedAt.ToString("yyMMdd") + item.OrderItemId.ToString().Split("-").First().ToUpper();
                    salesHistoryRedemptionTokens.Revenue = item.Revenue.Value.ToString("F2");
                    salesHistoryRedemptionTokens.PurchasedDate = item.CreatedAt.ToString("dd/MM/yyyy");
                    salesHistoryRedemptionTokens.RedeemedDate = item.RedeemedAt.Value.ToString("dd/MM/yyyy");
                    salesHistoryRedemptionTokens.Outlet = "-";
                    salesHistoryRedemptionTokens.Type = "Ecommerce";


                    report.RedemptionList.Add(salesHistoryRedemptionTokens);
                }
                revenue += redemptionTokens.Sum(x => x.Revenue).Value;
            }
            else
            {
                return View(Infrastructure.Enums.ErrorPageEnum.INVALID_REQUEST_PAGE);
            }
            //report.RedemptionList = report.RedemptionList.OrderBy(x => x.RedeemedDate);
            report.TotalRevenue = "RM " + revenue.ToString("F2");

            return new Rotativa.AspNetCore.ViewAsPdf("SalesHistoryRedemptionReport", report);// { FileName = "OutletReport.pdf" };
        }
        [Route("OutletRedemptionReport")]
        public async Task<IActionResult> OutletRedemptionReport(string startDate, string endDate)
        {
            var userid = User.Identity.GetUserId();
            var MerchantId = User.Claims.FirstOrDefault(x => x.Type == "MerchantId").Value;

            if (String.IsNullOrEmpty(MerchantId))
            {
                return View(Infrastructure.Enums.ErrorPageEnum.NOT_ALLOWED_PAGE);
                // return BadRequest();
            }
            var merchantId = Int32.Parse(MerchantId);

            var merchantResponse = await Mediator.Send(new Common.Services.Merchants.Queries.MerchantQuery() { MerchantId = merchantId });
            if (!merchantResponse.Successful)
            {
                return View(Infrastructure.Enums.ErrorPageEnum.INVALID_REQUEST_PAGE);
                //return BadRequest();
            }
            MerchantModel merchant = (MerchantModel)merchantResponse.Data;
            OutletRedemptionReport report = new OutletRedemptionReport();
            report.MerchantName = merchant.DisplayName;
            report.TotalRevenue = "RM 0.00";
            report.Date = "";
            report.RedemptionList = new List<InStoreRedemptionTokens>();
            ApiResponseViewModel response = new ApiResponseViewModel();
            DateTime StartDate = DateTime.Parse(startDate);
            DateTime EndDate = DateTime.Parse(endDate).AddHours(23).AddMinutes(59).AddSeconds(59);
            report.Date = StartDate.ToString("dd/MM/yyyy") + " ~ " + EndDate.ToString("dd/MM/yyyy");
            response = await Mediator.Send(new InStoreRedemptionWithDateQuery() { MerchantId = merchantId, StartDate = StartDate, EndDate = EndDate });
            if (response.Successful)
            {
                List<InStoreRedemptionTokens> redemptionTokens = (List<InStoreRedemptionTokens>)response.Data;
                report.RedemptionList = redemptionTokens.OrderBy(x => x.RedeemedAt).ToList();
                report.TotalRevenue = "RM " + redemptionTokens.Sum(x => x.Revenue).Value.ToString("F2");
            }
            else
            {
                return View(Infrastructure.Enums.ErrorPageEnum.INVALID_REQUEST_PAGE);
            }
            return new Rotativa.AspNetCore.ViewAsPdf("OutletRedemptionReport", report);// { FileName = "OutletReport.pdf" };
        }

        [Route("DigitalRedemptionReport")]
        public async Task<IActionResult> DigitalRedemptionReport(string startDate, string endDate)
        {
            var userid = User.Identity.GetUserId();
            var MerchantId = User.Claims.FirstOrDefault(x => x.Type == "MerchantId").Value;

            if (String.IsNullOrEmpty(MerchantId))
            {
                return View(Infrastructure.Enums.ErrorPageEnum.NOT_ALLOWED_PAGE);
                // return BadRequest();
            }
            var merchantId = Int32.Parse(MerchantId);

            var merchantResponse = await Mediator.Send(new Common.Services.Merchants.Queries.MerchantQuery() { MerchantId = merchantId });
            if (!merchantResponse.Successful)
            {
                return View(Infrastructure.Enums.ErrorPageEnum.INVALID_REQUEST_PAGE);
                //return BadRequest();
            }
            MerchantModel merchant = (MerchantModel)merchantResponse.Data;
            DigitalRedemptionReport report = new DigitalRedemptionReport();
            report.MerchantName = merchant.DisplayName;
            report.TotalRevenue = "RM 0.00";
            report.Date = "";
            report.RedemptionList = new List<DigitalRedemptionTokens>();
            ApiResponseViewModel response = new ApiResponseViewModel();
            DateTime StartDate = DateTime.Parse(startDate);
            DateTime EndDate = DateTime.Parse(endDate).AddHours(23).AddMinutes(59).AddSeconds(59);
            report.Date = StartDate.ToString("dd/MM/yyyy") + " ~ " + EndDate.ToString("dd/MM/yyyy");
            response = await Mediator.Send(new DigitalRedemptionWithDateQuery() { MerchantId = merchantId, StartDate = StartDate, EndDate = EndDate });
            if (response.Successful)
            {
                List<DigitalRedemptionTokens> redemptionTokens = (List<DigitalRedemptionTokens>)response.Data;
                report.RedemptionList = redemptionTokens.OrderBy(x => x.RedeemedAt).ToList();
                report.TotalRevenue = "RM " + redemptionTokens.Sum(x => x.Revenue).Value.ToString("F2");
            }
            else
            {
                return View(Infrastructure.Enums.ErrorPageEnum.INVALID_REQUEST_PAGE);
            }
            return new Rotativa.AspNetCore.ViewAsPdf("DigitalRedemptionReport", report);// { FileName = "OutletReport.pdf" };           
        }

        [Route("DeliveryRedemptionReport")]
        public async Task<IActionResult> DeliveryRedemptionReport(string startDate, string endDate)
        {
            var userid = User.Identity.GetUserId();
            var MerchantId = User.Claims.FirstOrDefault(x => x.Type == "MerchantId").Value;

            if (String.IsNullOrEmpty(MerchantId))
            {
                return View(Infrastructure.Enums.ErrorPageEnum.NOT_ALLOWED_PAGE);
                //return BadRequest();
            }
            var merchantId = Int32.Parse(MerchantId);

            var merchantResponse = await Mediator.Send(new Common.Services.Merchants.Queries.MerchantQuery() { MerchantId = merchantId });
            if (!merchantResponse.Successful)
            {
                return View(Infrastructure.Enums.ErrorPageEnum.INVALID_REQUEST_PAGE);
                //return BadRequest();
            }
            MerchantModel merchant = (MerchantModel)merchantResponse.Data;
            DeliveryRedemptionReport report = new DeliveryRedemptionReport();
            report.MerchantName = merchant.DisplayName;
            report.TotalRevenue = "RM 0.00";
            report.Date = "";
            report.RedemptionList = new List<DeliveryRedemptionTokens>();
            ApiResponseViewModel response = new ApiResponseViewModel();
            DateTime StartDate = DateTime.Parse(startDate);
            DateTime EndDate = DateTime.Parse(endDate).AddHours(23).AddMinutes(59).AddSeconds(59);
            report.Date = StartDate.ToString("dd/MM/yyyy") + " ~ " + EndDate.ToString("dd/MM/yyyy");
            response = await Mediator.Send(new DeliveryRedemptionWithDateQuery() { MerchantId = merchantId, StartDate = StartDate, EndDate = EndDate });
            if (response.Successful)
            {
                List<DeliveryRedemptionTokens> redemptionTokens = (List<DeliveryRedemptionTokens>)response.Data;
                report.RedemptionList = redemptionTokens.OrderBy(x => x.RedeemedAt).ToList();
                report.TotalRevenue = "RM " + redemptionTokens.Sum(x => x.Revenue).Value.ToString("F2");
            }
            else
            {
                return View(Infrastructure.Enums.ErrorPageEnum.INVALID_REQUEST_PAGE);
            }
            return new Rotativa.AspNetCore.ViewAsPdf("DeliveryRedemptionReport", report);// { FileName = "OutletReport.pdf" };           
        }

        [HttpPost]
        [Route("UpdateDeliveryRedemptionTokenForAllOrderItems")]
        public async Task<ApiResponseViewModel> UpdateDeliveryRedemptionTokenForAllOrderItems(Guid Id, string Token, string CourierProvider, string CourierUrl)
        {
            int merchantId = Int32.Parse(User.Claims.FirstOrDefault(x => x.Type == "MerchantId").Value);
            if (merchantId == 0)
            {
                ApiResponseViewModel badRequestResponse = new ApiResponseViewModel();
                badRequestResponse.Successful = false;
                badRequestResponse.Message = "Invalid credentials";
                return badRequestResponse;
            }
            ApiResponseViewModel apiResponseViewModel = new ApiResponseViewModel();
            apiResponseViewModel = await Mediator.Send(new UpdateDeliveryRedemptionTokensCommandForOrderMerchant() { Id = Id, Token = Token, CourierProvider = CourierProvider, CourierProviderUrl = CourierUrl, MerchantId = merchantId });
            return apiResponseViewModel;
        }

        [HttpPost]
        [Route("UpdateDeliveryRedemptionToken")]
        public async Task<ApiResponseViewModel> UpdateDeliveryRedemptionToken(int Id, string Token, string CourierProvider, string CourierUrl)
        {
            if (!await VerifyUserAccessWithDeliveryRedemptionId(Id))
            {
                ApiResponseViewModel badRequestResponse = new ApiResponseViewModel();
                badRequestResponse.Successful = false;
                badRequestResponse.Message = "Invalid credentials";
                return badRequestResponse;
            }
            ApiResponseViewModel apiResponseViewModel = new ApiResponseViewModel();
            apiResponseViewModel = await Mediator.Send(new UpdateDeliveryRedemptionTokensCommand() { Id = Id, Token = Token, CourierProvider = CourierProvider, CourierProviderUrl = CourierUrl });
            return apiResponseViewModel;
        }

        [HttpPost]
        [Route("SendTrackingNotification")]
        public async Task<ApiResponseViewModel> SendTrackingNotification()
        {
            ApiResponseViewModel apiResponseViewModel = new ApiResponseViewModel();
            var apiKey = "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";
            string templateId = "bc595bd9-9817-4f0d-a53c-62aa1abf8864";
            string subject = "Tracking Number Notification";
            var from = new SendGrid.Helpers.Mail.EmailAddress("noreply@vodus.my", "Vodus");
            var to = new SendGrid.Helpers.Mail.EmailAddress("fong@vodus.my", "Example User");
            var client = new SendGridClient(apiKey);
            var msg = new SendGridMessage();
            msg.From = from;
            msg.TemplateId = templateId;
            msg.Personalizations = new System.Collections.Generic.List<Personalization>();
            var personalization = new Personalization();
            personalization.Substitutions = new Dictionary<string, string>();
            personalization.Substitutions.Add("-productTitle-", "Product Name");
            personalization.Substitutions.Add("-courierName-", "Courier Name");
            personalization.Substitutions.Add("-courierUrl-", @"https://www.tracking.my/abx");
            personalization.Substitutions.Add("-courierTrackingNumber-", "Tracking Number");
            personalization.Subject = subject;
            personalization.Tos = new List<EmailAddress>();
            personalization.Tos.Add(to);
            msg.Personalizations.Add(personalization);
            var response = await client.SendEmailAsync(msg).ConfigureAwait(false);

            if (response.StatusCode == System.Net.HttpStatusCode.Accepted)
            {
                apiResponseViewModel.Successful = true;
                apiResponseViewModel.Message = "Successfully email to customer";
            }
            else
            {
                apiResponseViewModel.Successful = false;
                apiResponseViewModel.Message = "Failed email to customer";

            }
            return apiResponseViewModel;
        }

        [HttpPost]
        [Route("SendDigitalRedemptionNotification")]
        public async Task<ApiResponseViewModel> SendDigitalRedemptionNotification()
        {
            ApiResponseViewModel apiResponseViewModel = new ApiResponseViewModel();
            var apiKey = "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";
            string templateId = "a8a70ad6-3571-4d34-b0d0-3a2176974021";
            string subject = "Digital Redemption Notification";
            var from = new SendGrid.Helpers.Mail.EmailAddress("noreply@vodus.my", "Vodus");
            var to = new SendGrid.Helpers.Mail.EmailAddress("fong@vodus.my", "Example User");
            var client = new SendGridClient(apiKey);
            var msg = new SendGridMessage();
            msg.From = from;
            msg.TemplateId = templateId;
            msg.Personalizations = new System.Collections.Generic.List<Personalization>();
            var personalization = new Personalization();
            personalization.Substitutions = new Dictionary<string, string>();
            personalization.Substitutions.Add("-productTitle-", "Product Name");
            personalization.Substitutions.Add("-voucherCode-", "Redemption Code");
            personalization.Substitutions.Add("-redemptionFlow-", "<p>Input Redmption Flow</p>");
            personalization.Subject = subject;
            personalization.Tos = new List<EmailAddress>();
            personalization.Tos.Add(to);
            msg.Personalizations.Add(personalization);
            var response = await client.SendEmailAsync(msg).ConfigureAwait(false);

            if (response.StatusCode == System.Net.HttpStatusCode.Accepted)
            {
                apiResponseViewModel.Successful = true;
                apiResponseViewModel.Message = "Successfully email to customer";
            }
            else
            {
                apiResponseViewModel.Successful = false;
                apiResponseViewModel.Message = "Failed email to customer";

            }
            return apiResponseViewModel;
        }

        [HttpGet]
        [Authorize(Roles = "Merchant")]
        [Route("DownloadGifteeImages/{id}")]
        public async Task<ActionResult> DownloadGifteeImages(int id)
        {
           
            ApiResponseViewModel apiResponseViewModel = new ApiResponseViewModel();

            var result = await Mediator.Send(new GifteeProductQuery() { Id = id });
           
            if(result.Successful)
            {
                var gifteeData = (GifteeItemResponseModel)result.Data;
                using (WebClient webClient = new WebClient())
                {
                    byte[] data = webClient.DownloadData(gifteeData.ItemImageUrl);
                    return File(data, "image/png", $"Giftee-image-{id.ToString()}.png");
                }
            }
            else
            {
                return NotFound();
            }
           
        }

        [HttpPost]
        [Authorize(Roles = "Merchant")]
        [Route("CreateGuifteeDigitalRedemptionToken")]
        public async Task<ApiResponseViewModel> CreateGuifteeDigitalRedemptionToken(int Id, int ProductId)
        {
            var userName = User.Identity.GetUserName();

            if (userName.ToLower() != "merchant@vodus.my")
            {
                ApiResponseViewModel badRequestResponse = new ApiResponseViewModel();
                badRequestResponse.Successful = false;
                badRequestResponse.Message = "Invalid credentials";
                return badRequestResponse;
            }
            if (!await VerifyUserAccessWithDigitalRedemptionId(Id))
            {
                ApiResponseViewModel badRequestResponse = new ApiResponseViewModel();
                badRequestResponse.Successful = false;
                badRequestResponse.Message = "Invalid credentials";
                return badRequestResponse;
            }
            ApiResponseViewModel apiResponseViewModel = new ApiResponseViewModel();

            var result = await Mediator.Send(new CreateGifteedigitalRedemptionTokensCommand() { Id = Id, ProductId = ProductId });

            if (result != null)
            {
                if (result.Successful)
                {
                    //var DigitalRedemptionToken = (DigitalRedemptionTokens)result.Data;
                    apiResponseViewModel.Successful = true;
                    apiResponseViewModel.Message = result.Message;
                    //apiResponseViewModel.Data = DigitalRedemptionToken;
                }
                else
                {
                    apiResponseViewModel.Successful = false;
                    apiResponseViewModel.Message = result.Message;

                }
            }
            return apiResponseViewModel;

        }

        [HttpPost]
        [Route("UpdateDigitalRedemptionToken")]
        public async Task<ApiResponseViewModel> UpdateDigitalRedemptionToken(int Id, string Token)
        {
            if (!await VerifyUserAccessWithDigitalRedemptionId(Id))
            {
                ApiResponseViewModel badRequestResponse = new ApiResponseViewModel();
                badRequestResponse.Successful = false;
                badRequestResponse.Message = "Invalid credentials";
                return badRequestResponse;
            }
            ApiResponseViewModel apiResponseViewModel = new ApiResponseViewModel();
            apiResponseViewModel = await Mediator.Send(new UpdateDigitalRedemptionTokensCommand() { Id = Id, Token = Token });
            if (apiResponseViewModel.Successful)
            {
                var data = (DigitalRedemptionTokens)apiResponseViewModel.Data;

                var apiKey = "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";
                string templateId = "a8a70ad6-3571-4d34-b0d0-3a2176974021";
                string subject = "Digital Redemption Notification";
                var from = new SendGrid.Helpers.Mail.EmailAddress("noreply@vodus.my", "Vodus");
                var to = new SendGrid.Helpers.Mail.EmailAddress(data.Email, "Valued Customer");
                var client = new SendGridClient(apiKey);
                var msg = new SendGridMessage();
                msg.From = from;
                msg.TemplateId = templateId;
                msg.Personalizations = new System.Collections.Generic.List<Personalization>();
                var personalization = new Personalization();
                personalization.Substitutions = new Dictionary<string, string>();
                personalization.Substitutions.Add("-productTitle-", data.ProductTitle);
                personalization.Substitutions.Add("-voucherCode-", data.Token);
                personalization.Substitutions.Add("-redemptionFlow-", data.RedemptionInfo);
                personalization.Subject = subject;
                personalization.Tos = new List<EmailAddress>();
                personalization.Tos.Add(to);
                msg.Personalizations.Add(personalization);
                var response = await client.SendEmailAsync(msg).ConfigureAwait(false);

                if (response.StatusCode == System.Net.HttpStatusCode.Accepted)
                {
                    apiResponseViewModel.Successful = true;
                    apiResponseViewModel.Message = "Successfully email to customer";
                }
                else
                {
                    apiResponseViewModel.Successful = false;
                    apiResponseViewModel.Message = "Failed email to customer";

                }
            }
            apiResponseViewModel.Data = null;
            return apiResponseViewModel;
        }

        [HttpPost]
        [Route("SendInStoreRedemptionNotification")]
        public async Task<ApiResponseViewModel> SendInStoreRedemptionNotification()
        {
            string voucherCode = Voupon.Common.QRGenerator.QRCodeGenerator.GenerateQRCodeBase64String("ABCDEFG", 250, 250, 10);
            ApiResponseViewModel apiResponseViewModel = new ApiResponseViewModel();
            var apiKey = "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";
            string templateId = "7c1392ca-6c4d-44e8-8b90-37a8fd0f7fb2";
            string subject = "In-Store Redemption Notification";
            var from = new SendGrid.Helpers.Mail.EmailAddress("noreply@vodus.my", "Vodus");
            var to = new SendGrid.Helpers.Mail.EmailAddress("fong@vodus.my", "Example User");
            var client = new SendGridClient(apiKey);
            var msg = new SendGridMessage();
            msg.From = from;
            msg.TemplateId = templateId;
            msg.Personalizations = new System.Collections.Generic.List<Personalization>();
            var personalization = new Personalization();
            personalization.Substitutions = new Dictionary<string, string>();
            personalization.Substitutions.Add("-productTitle-", "Product Name");
            personalization.Substitutions.Add("-voucherCode-", "cid:voucher");
            personalization.Substitutions.Add("-redemptionFlow-", "<p>Input Redmption Flow</p>");
            personalization.Subject = subject;
            personalization.Tos = new List<EmailAddress>();
            personalization.Tos.Add(to);
            msg.Personalizations.Add(personalization);
            msg.AddAttachment(new Attachment()
            {
                Content = voucherCode,
                Type = "image/png",
                Filename = "voucher.png",
                ContentId = "voucher",
                Disposition = "inline"
            });
            var response = await client.SendEmailAsync(msg).ConfigureAwait(false);

            if (response.StatusCode == System.Net.HttpStatusCode.Accepted)
            {
                apiResponseViewModel.Successful = true;
                apiResponseViewModel.Message = "Successfully email to customer";
            }
            else
            {
                apiResponseViewModel.Successful = false;
                apiResponseViewModel.Message = "Failed email to customer";

            }
            return apiResponseViewModel;
        }

        [HttpPost]
        [Route("SendInStoreSalesReport")]
        public async Task<ApiResponseViewModel> SendInStoreSalesReport(int merchantId, string startDate, string endDate)
        {
            if (!VerifyUserAccess(merchantId))
            {
                ApiResponseViewModel badRequestResponse = new ApiResponseViewModel();
                badRequestResponse.Successful = false;
                badRequestResponse.Message = "Invalid credentials";
                return badRequestResponse;
            }
            string voucherCode = Voupon.Common.QRGenerator.QRCodeGenerator.GenerateQRCodeBase64String("ABCDEFG", 250, 250, 10);
            ApiResponseViewModel apiResponseViewModel = new ApiResponseViewModel();
            var apiKey = "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";
            string templateId = "7c1392ca-6c4d-44e8-8b90-37a8fd0f7fb2";
            string subject = "In-Store Redemption Notification";
            var from = new SendGrid.Helpers.Mail.EmailAddress("noreply@vodus.my", "Vodus");
            var to = new SendGrid.Helpers.Mail.EmailAddress("fong@vodus.my", "Example User");
            var client = new SendGridClient(apiKey);
            var msg = new SendGridMessage();
            msg.From = from;
            msg.TemplateId = templateId;
            msg.Personalizations = new System.Collections.Generic.List<Personalization>();
            var personalization = new Personalization();
            personalization.Substitutions = new Dictionary<string, string>();
            personalization.Substitutions.Add("-productTitle-", "Product Name");
            personalization.Substitutions.Add("-voucherCode-", "cid:voucher");
            personalization.Substitutions.Add("-redemptionFlow-", "<p>Input Redmption Flow</p>");
            personalization.Subject = subject;
            personalization.Tos = new List<EmailAddress>();
            personalization.Tos.Add(to);
            msg.Personalizations.Add(personalization);
            msg.AddAttachment(new Attachment()
            {
                Content = voucherCode,
                Type = "image/png",
                Filename = "voucher.png",
                ContentId = "voucher",
                Disposition = "inline"
            });
            var response = await client.SendEmailAsync(msg).ConfigureAwait(false);

            if (response.StatusCode == System.Net.HttpStatusCode.Accepted)
            {
                apiResponseViewModel.Successful = true;
                apiResponseViewModel.Message = "Successfully email to customer";
            }
            else
            {
                apiResponseViewModel.Successful = false;
                apiResponseViewModel.Message = "Failed email to customer";

            }
            return apiResponseViewModel;
        }


        [HttpGet]
        [Route("Refund-Items")]
        public async Task<ApiResponseViewModel> GetRefundnItems(string start, string length, string searchValue)
        {
            var merchantId = int.Parse(User.Claims.FirstOrDefault(x => x.Type == "MerchantId").Value);

            if (!VerifyUserAccess(merchantId))
            {
                ApiResponseViewModel badRequestResponse = new ApiResponseViewModel();
                badRequestResponse.Successful = false;
                badRequestResponse.Message = "Invalid credentials";
                return badRequestResponse;
            }
            try
            {
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt32(start) : 0;
                int recordsTotal = 0;
                ApiResponseViewModel response = await Mediator.Send(new RefundOrderItemQuery() { MerchantId = merchantId });
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

        [HttpGet]
        [Route("Pending-Action-Items")]
        public async Task<ApiResponseViewModel> GetPendingActionItems(string start, string length, string searchValue)
        {
            var merchantId = int.Parse(User.Claims.FirstOrDefault(x => x.Type == "MerchantId").Value);

            if (!VerifyUserAccess(merchantId))
            {
                ApiResponseViewModel badRequestResponse = new ApiResponseViewModel();
                badRequestResponse.Successful = false;
                badRequestResponse.Message = "Invalid credentials";
                return badRequestResponse;
            }
            try
            {
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt32(start) : 0;
                int recordsTotal = 0;
                ApiResponseViewModel response = await Mediator.Send(new PendingHandlingOrderItemQuery() { MerchantId = merchantId });
                if (response.Successful && response.Data != null)
                {


                    var pendingSalesData = (PendingHandlingViewModel)response.Data;
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
                    var OrdersList = new List<PendingOrdersViewModel>();
                    foreach (var orderData in groupedByOrderData)
                    {
                        var model = new PendingOrdersViewModel()
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
                            pendingHandlingItemViewModels = orderData.ToList(),
                            ShortOrderId = orderData.FirstOrDefault().ShortOrderId,
                        };
                        //if (orderData.Select(x => x.OrderStatus).FirstOrDefault() == 1)
                        //{
                        //    model.Status = 7;
                        //    continue;
                        //}
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






                    var data = OrdersList.OrderByDescending(x => x.CreatedAt).Skip(skip).Take(pageSize).ToList();
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
        [HttpGet]
        [Route("GetOrderDetailsByOrderId")]
        public async Task<ApiResponseViewModel> GetOrderDetailsByOrderId(string orderId)
        {
            int merchantId = int.Parse(User.Claims.FirstOrDefault(x => x.Type == "MerchantId").Value);
            ApiResponseViewModel response = new ApiResponseViewModel();
            response = await Mediator.Send(new OrderDetailsByOrderIdAndMerchantIdQuery() { OrderId = orderId, MerchantId = merchantId });
            if (response.Successful)
            {
                var data = (OrderDetailsModel)response.Data;
                //data.Order.OrderItems = null;
                response.Data = data;
            }
            return response;
        }

        [HttpGet]
        [Route("GetPendingSalesRedemptionTokensWithMerchantId/{merchantId}")]
        public async Task<ApiResponseViewModel> GetPendingSalesRedemptionTokensWithMerchantId(int merchantId)
        {
            if (!VerifyUserAccess(merchantId))
            {
                ApiResponseViewModel badRequestResponse = new ApiResponseViewModel();
                badRequestResponse.Successful = false;
                badRequestResponse.Message = "Invalid credentials";
                return badRequestResponse;
            }
            List<PendingSalesRedemptionTokens> pendingSalesRedemptionTokensList = new List<PendingSalesRedemptionTokens>();
            ApiResponseViewModel response = new ApiResponseViewModel();

            // Outlet

            response = await Mediator.Send(new InStoreRedemptionTokensWithMerchantIdQuery() { MerchantId = merchantId });
            if (response.Successful)
            {
                var data = (List<InStoreRedemptionTokensViewModel>)response.Data;

                //  TODO move int to enums
                foreach (var item in data.Where(x => DateTime.Now.Subtract(x.CreatedAt).TotalDays <= 14).Where(x => x.IsRedeemed == false).ToList())
                {
                    PendingSalesRedemptionTokens pendingSales = new PendingSalesRedemptionTokens();
                    pendingSales.Id = item.Id;
                    pendingSales.ProductId = item.ProductId;
                    pendingSales.ProductTitle = item.ProductTitle;
                    pendingSales.ShortOrderItemId = item.ShortOrderItemId;
                    pendingSales.ShortOrderId = item.ShortOrderId;

                    pendingSales.Email = item.Email;
                    pendingSales.Revenue = item.Revenue;
                    pendingSales.RedeemedAt = item.RedeemedAt;
                    pendingSales.CreatedAt = item.CreatedAt;
                    pendingSales.Type = 1;
                    if (item.Token != null && item.Token != "")
                    {
                        pendingSales.Status = item.Token;

                    }
                    else
                    {
                        pendingSales.Status = "Not Redeemed";

                    }
                    pendingSalesRedemptionTokensList.Add(pendingSales);
                }

            }
            // Digital

            response = await Mediator.Send(new DigitalRedemptionTokensWithMerchantIdQuery() { MerchantId = merchantId });
            if (response.Successful)
            {
                var data = (List<DigitalRedemptionTokensViewModel>)response.Data;
                foreach (var item in data.Where(x => DateTime.Now.Subtract(x.CreatedAt).TotalDays <= 14).Where(x => x.IsRedeemed == false).ToList())
                {
                    PendingSalesRedemptionTokens pendingSales = new PendingSalesRedemptionTokens();
                    pendingSales.ProductId = item.ProductId;
                    pendingSales.Id = item.Id;
                    pendingSales.ProductTitle = item.ProductTitle;
                    pendingSales.ShortOrderItemId = item.ShortOrderItemId;
                    pendingSales.ShortOrderId = item.ShortOrderId;

                    pendingSales.Email = item.Email;
                    pendingSales.Revenue = item.Revenue;
                    pendingSales.RedeemedAt = item.RedeemedAt;
                    pendingSales.CreatedAt = item.CreatedAt;
                    pendingSales.Type = 3;
                    if (item.Token != null && item.Token != "")
                    {
                        pendingSales.Status = "Already Created e-voucher";

                    }
                    else
                    {
                        pendingSales.Status = "Pending e-voucher code";
                    }
                    pendingSalesRedemptionTokensList.Add(pendingSales);
                }
            }

            // Delivery

            response = await Mediator.Send(new DeliveryRedemptionTokensWithMerchantIdQuery() { MerchantId = merchantId });
            if (response.Successful)
            {
                var data = (List<DeliveryRedemptionTokensViewModel>)response.Data;
                foreach (var item in data.Where(x => DateTime.Now.Subtract(x.CreatedAt).TotalDays <= 14).Where(x => x.IsRedeemed == false).ToList())
                {
                    PendingSalesRedemptionTokens pendingSales = new PendingSalesRedemptionTokens();
                    pendingSales.Id = item.Id;
                    pendingSales.ProductId = item.ProductId;
                    pendingSales.ProductTitle = item.ProductTitle;
                    pendingSales.ShortOrderItemId = item.ShortOrderItemId;
                    pendingSales.ShortOrderId = item.ShortOrderId;
                    pendingSales.Email = item.Email;
                    pendingSales.Revenue = item.Revenue;
                    pendingSales.RedeemedAt = item.RedeemedAt;
                    pendingSales.CreatedAt = item.CreatedAt;
                    pendingSales.Type = 2;
                    if (item.Token != null && item.Token != "")
                    {
                        pendingSales.Status = item.Token;

                    }
                    else
                    {
                        pendingSales.Status = "Pending shipment";
                    }
                    pendingSalesRedemptionTokensList.Add(pendingSales);
                }
            }
            response.Data = pendingSalesRedemptionTokensList.OrderByDescending(x => x.CreatedAt);
            return response;
        }

        [HttpGet]
        [Route("GetInStoreRedemptionTokensWithMerchantId/{merchantId}")]
        public async Task<ApiResponseViewModel> GetInStoreRedemptionTokensWithMerchantId(int merchantId)
        {
            if (!VerifyUserAccess(merchantId))
            {
                ApiResponseViewModel badRequestResponse = new ApiResponseViewModel();
                badRequestResponse.Successful = false;
                badRequestResponse.Message = "Invalid credentials";
                return badRequestResponse;
            }
            ApiResponseViewModel response = new ApiResponseViewModel();
            response = await Mediator.Send(new InStoreRedemptionTokensWithMerchantIdQuery() { MerchantId = merchantId });
            if (response.Successful)
            {
                var data = (List<InStoreRedemptionTokensViewModel>)response.Data;
                foreach (var item in data)
                {
                    item.OrderItem.Order.OrderItems = null;
                    item.OrderItem.InStoreRedemptionTokens = null;
                }
                response.Data = data.Where(x => DateTime.Now.Subtract(x.CreatedAt).TotalDays <= 14).ToList();
            }
            return response;
        }

        [HttpGet]
        [Route("GetDigitalRedemptionTokensWithMerchantId/{merchantId}")]
        public async Task<ApiResponseViewModel> GetDigitalRedemptionTokensWithMerchantId(int merchantId)
        {
            if (!VerifyUserAccess(merchantId))
            {
                ApiResponseViewModel badRequestResponse = new ApiResponseViewModel();
                badRequestResponse.Successful = false;
                badRequestResponse.Message = "Invalid credentials";
                return badRequestResponse;
            }
            ApiResponseViewModel response = new ApiResponseViewModel();
            response = await Mediator.Send(new DigitalRedemptionTokensWithMerchantIdQuery() { MerchantId = merchantId });
            if (response.Successful)
            {
                var data = (List<DigitalRedemptionTokensViewModel>)response.Data;
                response.Data = data.Where(x => DateTime.Now.Subtract(x.CreatedAt).TotalDays <= 14).ToList();
            }
            return response;
        }

        [HttpGet]
        [Route("GetDeliveryRedemptionTokensWithMerchantId/{merchantId}")]
        public async Task<ApiResponseViewModel> GetDeliveryRedemptionTokensWithMerchantId(int merchantId)
        {
            if (!VerifyUserAccess(merchantId))
            {
                ApiResponseViewModel badRequestResponse = new ApiResponseViewModel();
                badRequestResponse.Successful = false;
                badRequestResponse.Message = "Invalid credentials";
                return badRequestResponse;
            }
            ApiResponseViewModel response = new ApiResponseViewModel();
            response = await Mediator.Send(new DeliveryRedemptionTokensWithMerchantIdQuery() { MerchantId = merchantId });
            if (response.Successful)
            {
                var data = (List<DeliveryRedemptionTokensViewModel>)response.Data;
                response.Data = data.Where(x => DateTime.Now.Subtract(x.CreatedAt).TotalDays <= 14).ToList();
            }
            return response;
        }
        #region Giftee Vouchers and Products Methods
        [HttpGet]
        [Route("GetOrderDetailsByShortOrderId")]
        public async Task<ApiResponseViewModel> GetOrderDetailsByShortOrderId(int merchantId, string shortOrderItemId)
        {
            if (!VerifyUserAccess(merchantId))
            {
                ApiResponseViewModel badRequestResponse = new ApiResponseViewModel();
                badRequestResponse.Successful = false;
                badRequestResponse.Message = "Invalid credentials";
                return badRequestResponse;
            }
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

        #region TestingGifteeAPI
        [HttpGet]
        [Route("GetTestingAPI")]
        public async Task<ApiResponseViewModel> GetTestingAPI()
        {
            return await Mediator.Send(new CreateNewGifteeVoucherTestingCommand() { });
        }

        #endregion

        [HttpGet]
        [Route("GetOrderItemDetailsByShortOrderId")]
        public async Task<ApiResponseViewModel> GetOrderItemDetailsByShortOrderId(string shortOrderItemId)
        {
            var merchantId = int.Parse(User.Claims.FirstOrDefault(x => x.Type == "MerchantId").Value);
            return await GetOrderDetailsByShortOrderId(merchantId, shortOrderItemId);
        }

        [HttpGet]
        [Route("GetDetailsByShortOrderId")]
        public async Task<ApiResponseViewModel> GetDetailsByShortOrderId(string shortOrderItemId)
        {
            var userid = User.Identity.GetUserId();
            var merchantId = User.Claims.FirstOrDefault(x => x.Type == "MerchantId").Value;
            if (String.IsNullOrEmpty(merchantId))
            {
                ApiResponseViewModel badRequestResponse = new ApiResponseViewModel();
                badRequestResponse.Successful = false;
                badRequestResponse.Message = "Invalid credentials";
                return badRequestResponse;
            }
            if (!VerifyUserAccess(Int16.Parse(merchantId)))
            {
                ApiResponseViewModel badRequestResponse = new ApiResponseViewModel();
                badRequestResponse.Successful = false;
                badRequestResponse.Message = "Invalid credentials";
                return badRequestResponse;
            }
            ApiResponseViewModel response = new ApiResponseViewModel();
            response = await Mediator.Send(new OrderDetailsWithOrderShortIdQuery() { ShortOrderItemId = shortOrderItemId });
            if (response.Successful)
            {
                var data = (OrderItems)response.Data;
                data.Order.OrderItems = null;
                response.Data = data;
            }
            return response;
        }
        [HttpPost]
        [Route("AddGifteeProduct")]
        public async Task<ApiResponseViewModel> AddGifteeProduct(int merchantId, string productName, string externalId)
        {
            var userid = User.Identity.GetUserId();
            var MerchantId = User.Claims.FirstOrDefault(x => x.Type == "MerchantId").Value;
            if (String.IsNullOrEmpty(MerchantId) || MerchantId != "1")
            {
                ApiResponseViewModel badRequestResponse = new ApiResponseViewModel();
                badRequestResponse.Successful = false;
                badRequestResponse.Message = "Invalid credentials";
                return badRequestResponse;
            }
            if (!VerifyUserAccess(merchantId))
            {
                ApiResponseViewModel badRequestResponse = new ApiResponseViewModel();
                badRequestResponse.Successful = false;
                badRequestResponse.Message = "Invalid credentials";
                return badRequestResponse;
            }
            ApiResponseViewModel response = new ApiResponseViewModel();
            response = await Mediator.Send(new CreateNewGifteeVoucherProduct() { ProductExternalId = externalId, ProductName = productName });

            return response;
        }
        [HttpPost]
        [Route("DeleteGifteeProduct")]
        public async Task<ApiResponseViewModel> DeleteGifteeProduct(int merchantId, string productId)
        {
            var userid = User.Identity.GetUserId();
            var MerchantId = User.Claims.FirstOrDefault(x => x.Type == "MerchantId").Value;
            if (String.IsNullOrEmpty(MerchantId) || MerchantId != "1")
            {
                ApiResponseViewModel badRequestResponse = new ApiResponseViewModel();
                badRequestResponse.Successful = false;
                badRequestResponse.Message = "Invalid credentials";
                return badRequestResponse;
            }
            if (!VerifyUserAccess(merchantId))
            {
                ApiResponseViewModel badRequestResponse = new ApiResponseViewModel();
                badRequestResponse.Successful = false;
                badRequestResponse.Message = "Invalid credentials";
                return badRequestResponse;
            }
            ApiResponseViewModel response = new ApiResponseViewModel();
            response = await Mediator.Send(new DeleteGifteeVoucherProduct() { ProductId = productId });

            return response;
        }
        [HttpPost]
        [Route("UpdateGifteeProductStatus")]
        public async Task<ApiResponseViewModel> UpdateGifteeProductStatus(int merchantId, string productId, bool status)
        {
            var userid = User.Identity.GetUserId();
            var MerchantId = User.Claims.FirstOrDefault(x => x.Type == "MerchantId").Value;
            if (String.IsNullOrEmpty(MerchantId) || MerchantId != "1")
            {
                ApiResponseViewModel badRequestResponse = new ApiResponseViewModel();
                badRequestResponse.Successful = false;
                badRequestResponse.Message = "Invalid credentials";
                return badRequestResponse;
            }
            if (!VerifyUserAccess(merchantId))
            {
                ApiResponseViewModel badRequestResponse = new ApiResponseViewModel();
                badRequestResponse.Successful = false;
                badRequestResponse.Message = "Invalid credentials";
                return badRequestResponse;
            }
            ApiResponseViewModel response = new ApiResponseViewModel();
            response = await Mediator.Send(new UpdateGifteeVoucherProductStatus() { Id = productId, Status = status });

            return response;
        }
        [HttpPost]
        [Route("UpdateGifteeProduct")]
        public async Task<ApiResponseViewModel> UpdateGifteeProduct(int merchantId, string productId, string productName, string externalId)
        {
            var userid = User.Identity.GetUserId();
            var MerchantId = User.Claims.FirstOrDefault(x => x.Type == "MerchantId").Value;
            if (String.IsNullOrEmpty(MerchantId) || MerchantId != "1")
            {
                ApiResponseViewModel badRequestResponse = new ApiResponseViewModel();
                badRequestResponse.Successful = false;
                badRequestResponse.Message = "Invalid credentials";
                return badRequestResponse;
            }
            if (!VerifyUserAccess(merchantId))
            {
                ApiResponseViewModel badRequestResponse = new ApiResponseViewModel();
                badRequestResponse.Successful = false;
                badRequestResponse.Message = "Invalid credentials";
                return badRequestResponse;
            }
            ApiResponseViewModel response = new ApiResponseViewModel();
            response = await Mediator.Send(new UpdateGifteeVoucherProduct() { Id = productId, ProductExternalId = externalId, ProductName = productName });

            return response;
        }
        [HttpGet]
        [Route("GetGifteeProducts")]
        public async Task<ApiResponseViewModel> GetGifteeProducts(int merchantId)
        {
            var userid = User.Identity.GetUserId();
            var MerchantId = User.Claims.FirstOrDefault(x => x.Type == "MerchantId").Value;
            if (String.IsNullOrEmpty(MerchantId) || MerchantId != "1")
            {
                ApiResponseViewModel badRequestResponse = new ApiResponseViewModel();
                badRequestResponse.Successful = false;
                badRequestResponse.Message = "Invalid credentials";
                return badRequestResponse;
            }
            if (!VerifyUserAccess(merchantId))
            {
                ApiResponseViewModel badRequestResponse = new ApiResponseViewModel();
                badRequestResponse.Successful = false;
                badRequestResponse.Message = "Invalid credentials";
                return badRequestResponse;
            }
            ApiResponseViewModel response = new ApiResponseViewModel();
            response = await Mediator.Send(new ListGifteeProductsQuery());
            if (response.Successful)
            {
                var data = (List<ThirdPartyProducts>)response.Data;
                response.Data = data;
            }
            return response;
        }
        [HttpGet]
        [Route("GetGifteeVouchers")]
        public async Task<ApiResponseViewModel> GetGifteeVouchers(int merchantId)
        {
            var userid = User.Identity.GetUserId();
            var MerchantId = User.Claims.FirstOrDefault(x => x.Type == "MerchantId").Value;
            if (String.IsNullOrEmpty(MerchantId) || MerchantId != "1")
            {
                ApiResponseViewModel badRequestResponse = new ApiResponseViewModel();
                badRequestResponse.Successful = false;
                badRequestResponse.Message = "Invalid credentials";
                return badRequestResponse;
            }
            if (!VerifyUserAccess(merchantId))
            {
                ApiResponseViewModel badRequestResponse = new ApiResponseViewModel();
                badRequestResponse.Successful = false;
                badRequestResponse.Message = "Invalid credentials";
                return badRequestResponse;
            }
            ApiResponseViewModel response = new ApiResponseViewModel();
            response = await Mediator.Send(new ListGifteeVouchersQuery());
            if (response.Successful)
            {
                var data = (List<GifteeTokens>)response.Data;
                response.Data = data;
            }
            return response;
        }
        #endregion
        [HttpGet]
        [Route("GetsalesHistoryRedemptionSummary/{merchantId}")]
        public async Task<ApiResponseViewModel> GetsalesHistoryRedemptionSummary(int merchantId, string startDate, string endDate)
        {
            if (!VerifyUserAccess(merchantId))
            {
                ApiResponseViewModel badRequestResponse = new ApiResponseViewModel();
                badRequestResponse.Successful = false;
                badRequestResponse.Message = "Invalid credentials";
                return badRequestResponse;
            }
            ApiResponseViewModel response = new ApiResponseViewModel();
            DateTime StartDate = DateTime.Parse(startDate);
            DateTime EndDate = DateTime.Parse(endDate).AddDays(1);


            return await Mediator.Send(new SalesHistoryRedemptionSummaryWithDateQuery() { MerchantId = merchantId, StartDate = StartDate, EndDate = EndDate });
        }
        [HttpGet]
        [Route("GetInStoreRedemptionSummary/{merchantId}")]
        public async Task<ApiResponseViewModel> GetInStoreRedemptionSummary(int merchantId, string startDate, string endDate)
        {
            if (!VerifyUserAccess(merchantId))
            {
                ApiResponseViewModel badRequestResponse = new ApiResponseViewModel();
                badRequestResponse.Successful = false;
                badRequestResponse.Message = "Invalid credentials";
                return badRequestResponse;
            }
            ApiResponseViewModel response = new ApiResponseViewModel();
            DateTime StartDate = DateTime.Parse(startDate);
            DateTime EndDate = DateTime.Parse(endDate).AddHours(23).AddMinutes(59).AddSeconds(59);


            return await Mediator.Send(new InStoreRedemptionSummaryWithDateQuery() { MerchantId = merchantId, StartDate = StartDate, EndDate = EndDate });
        }

        [HttpGet]
        [Route("GetDigitalRedemptionSummary/{merchantId}")]
        public async Task<ApiResponseViewModel> GetDigitalRedemptionSummary(int merchantId, string startDate, string endDate)
        {
            if (!VerifyUserAccess(merchantId))
            {
                ApiResponseViewModel badRequestResponse = new ApiResponseViewModel();
                badRequestResponse.Successful = false;
                badRequestResponse.Message = "Invalid credentials";
                return badRequestResponse;
            }
            ApiResponseViewModel response = new ApiResponseViewModel();
            DateTime StartDate = DateTime.Parse(startDate);
            DateTime EndDate = DateTime.Parse(endDate).AddHours(23).AddMinutes(59).AddSeconds(59);
            return await Mediator.Send(new DigitalRedemptionSummaryWithDateQuery() { MerchantId = merchantId, StartDate = StartDate, EndDate = EndDate });
        }

        [HttpGet]
        [Route("GetDeliveryRedemptionSummary/{merchantId}")]
        public async Task<ApiResponseViewModel> GetDeliveryRedemptionSummary(int merchantId, string startDate, string endDate)
        {
            if (!VerifyUserAccess(merchantId))
            {
                ApiResponseViewModel badRequestResponse = new ApiResponseViewModel();
                badRequestResponse.Successful = false;
                badRequestResponse.Message = "Invalid credentials";
                return badRequestResponse;
            }
            ApiResponseViewModel response = new ApiResponseViewModel();
            DateTime StartDate = DateTime.Parse(startDate);
            DateTime EndDate = DateTime.Parse(endDate).AddHours(23).AddMinutes(59).AddSeconds(59);
            return await Mediator.Send(new DeliveryRedemptionSummaryWithDateQuery() { MerchantId = merchantId, StartDate = StartDate, EndDate = EndDate });
        }

        [HttpPost]
        [Route("Refund")]
        public async Task<ApiResponseViewModel> Refund(CreateOrderItemWithQuantityPerMerchantRefundCommand command)
        {
            ApiResponseViewModel apiResponseViewModel = new ApiResponseViewModel();
            var merchantId = int.Parse(User.Claims.FirstOrDefault(x => x.Type == "MerchantId").Value);

            if (!VerifyUserAccess(merchantId))
            {
                ApiResponseViewModel badRequestResponse = new ApiResponseViewModel();
                badRequestResponse.Successful = false;
                badRequestResponse.Message = "Invalid credentials";
                return badRequestResponse;
            }

            command.MerchantId = merchantId;
            command.IsAdminForMerchantRefund = command.NoAdminConfirmation.HasValue? (bool)command.NoAdminConfirmation : false;
            command.UserId = new Guid(User.Identity.GetUserId());

            var isMerchantHasOrderItemAccessQuery = await Mediator.Send(new IsMerchantHasOrderItemAccessQuery
            {
                MerchantId = command.MerchantId,
                OrderItemId = command.OrderItemId
            });

            if (!isMerchantHasOrderItemAccessQuery)
            {
                apiResponseViewModel.Message = "Not allowed";
                apiResponseViewModel.Successful = false;
                return apiResponseViewModel;
            }

            return await Mediator.Send(command);
        }

        [HttpPost]
        [Route("FullRefund")]
        public async Task<ApiResponseViewModel> FullRefund(CreateOrderFullRefundCommand command)
        {
            ApiResponseViewModel apiResponseViewModel = new ApiResponseViewModel();
            command.UserId = new Guid(User.Identity.GetUserId());
            command.IsExternal = false;
            return await Mediator.Send(command);
        }
        private async Task<bool> VerifyUserAccessWithDeliveryRedemptionId(int id)
        {
            int merchantId = await QueriesDeliveryRedemptionIdMerchantId(id);
            return VerifyUserAccess(merchantId);
        }

        private async Task<int> QueriesDeliveryRedemptionIdMerchantId(int id)
        {
            int merchantId = 0;
            ApiResponseViewModel response = await Mediator.Send(new DeliveryRedemptionTokensWithIdQuery() { Id = id });
            if (response.Successful)
            {
                if (response.Data != null)
                {
                    var token = (DeliveryRedemptionTokensViewModel)response.Data;
                    merchantId = token.MerchantId;
                }
            }
            return merchantId;
        }

        private async Task<bool> VerifyUserAccessWithDigitalRedemptionId(int id)
        {
            int merchantId = await QueriesDigitalRedemptionIdMerchantId(id);
            return VerifyUserAccess(merchantId);
        }

        private async Task<int> QueriesDigitalRedemptionIdMerchantId(int id)
        {
            int merchantId = 0;
            ApiResponseViewModel response = await Mediator.Send(new DigitalRedemptionTokensWithIdQuery() { Id = id });
            if (response.Successful)
            {
                if (response.Data != null)
                {
                    var token = (DigitalRedemptionTokensViewModel)response.Data;
                    merchantId = token.MerchantId;
                }
            }
            return merchantId;
        }
        private bool VerifyUserAccess(int merchantId)
        {
            bool IsMatch = false;
            try
            {
                var userid = User.Identity.GetUserId();
                if (User.IsInRole("Admin"))
                {
                    IsMatch = true;
                    return IsMatch;
                }
                var userMerchantId = User.Claims.FirstOrDefault(x => x.Type == "MerchantId").Value;
                if (Int32.Parse(userMerchantId) == merchantId)
                {
                    IsMatch = true;
                }
            }
            catch (Exception ex)
            {
                IsMatch = false;
            }
            return IsMatch;
        }
    }
}