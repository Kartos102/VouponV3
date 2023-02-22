using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Voupon.Merchant.WebApp.Areas.App.Services.Queries.List;
using Voupon.Merchant.WebApp.Common.Services.BankAccounts.Models;
using Voupon.Merchant.WebApp.Common.Services.BankAccounts.Queries;
using Voupon.Merchant.WebApp.Common.Services.DeliveryRedemptionTokens.Queries;
using Voupon.Merchant.WebApp.Common.Services.DigitalRedemptionTokens.Queries;
using Voupon.Merchant.WebApp.Common.Services.InStoreRedemptionTokens.Queries;
using Voupon.Merchant.WebApp.Common.Services.Merchants.Models;
using Voupon.Merchant.WebApp.Common.Services.Merchants.Queries;
using Voupon.Merchant.WebApp.Common.Services.PersonInCharges.Models;
using Voupon.Merchant.WebApp.Common.Services.PersonInCharges.Queries;
using Voupon.Merchant.WebApp.Common.Services.Products.Queries;
using Voupon.Merchant.WebApp.Infrastructure.Enums;
using Voupon.Merchant.WebApp.ViewModels;

namespace Voupon.Merchant.WebApp.Areas.App.Controllers
{
    public class DashboardViewModel
    {
        public string MerchantCode { get; set; }

        public bool StorePublished { get; set; }
        public string MerchantStatus { get; set; }
        public int MerchantStatusId { get; set; }
        public string BankAccountStatus { get; set; }
        public int BankAccountStatusId { get; set; }
        public string PersonInChargeStatus { get; set; }
        public int PersonInChargeStatusId { get; set; }
        public int ProductPendingRevision { get; set; }
        public int ProductPendingReview { get; set; }
        public int ProductPublished { get; set; }
        public int ProductTotal { get; set; }

        public PendingProducts PendingProducts { get; set; }
        public SoldProducts SoldProductsLastOneWeek { get; set; }



        [Required(ErrorMessage = "Please input product title")]
        [Display(Name = "Title")]
        public string Title { get; set; }

        public int MerchantId { get; set; }
    }

    public class SoldProducts
    {
        public int NumberOfSoldProductsInlastWeek { get; set; }
        public decimal RevenueOfSoldProductsInOneWeek { get; set; }
    }
    public class PendingProducts
    {
        public int NumberOfPerndingProducts { get; set; }
        public decimal RevenueOfPerndingProducts { get; set; }
    }

    [Area("App")]
    [Route("App/[controller]")]
    [Authorize(Roles = "Merchant,Staff")]
    public class DashboardController : BaseAppController
    {
        public async Task<IActionResult> Index()
        {

            var merchantId = int.Parse(User.Claims.FirstOrDefault(x => x.Type == "MerchantId").Value);
            if (merchantId == 0)
            {
                return View(ErrorPageEnum.NOT_ALLOWED_PAGE);
            }

            var merchantResponse = await Mediator.Send(new MerchantQuery() { MerchantId = merchantId });
            var merchantResponse1 = await Mediator.Send(new MerchantPendingChangesQuery() { MerchantId = merchantId });
            var personInChargeResponse = await Mediator.Send(new MerchantPersonInChargePendingChangesQuery() { MerchantId = merchantId });
            var bankAccountResponse = await Mediator.Send(new MerchantBankAccountPendingChangesQuery() { MerchantId = merchantId });
            var merchant = (MerchantModel)merchantResponse.Data;
            var merchant1 = (MerchantModel)merchantResponse1.Data;
            var personInCharge = (PersonInChargeModel)personInChargeResponse.Data;
            var bankAccount = (BankAccountModel)bankAccountResponse.Data;

            var model = new DashboardViewModel();
            model.MerchantId = merchantId;
            model.Title = "";
            model.MerchantCode = merchant1.Code;
            model.MerchantStatus = merchant1.Status;
            model.MerchantStatusId = merchant1.StatusTypeId;
            model.StorePublished = merchant.IsPublished;
            model.PersonInChargeStatus = personInCharge.Status;
            model.PersonInChargeStatusId = personInCharge.StatusTypeId;
            model.BankAccountStatus = bankAccount.Status;
            model.BankAccountStatusId = bankAccount.StatusTypeId;
            model.ProductPendingReview = 0;
            model.ProductPendingRevision = 0;
            model.ProductPublished = 0;
            model.ProductTotal = 0;

            model.SoldProductsLastOneWeek = new SoldProducts();
            model.PendingProducts = new PendingProducts();

            ApiResponseViewModel ProductPendingReviewResponse = await Mediator.Send(new ProductListByStatusQueryAndMerchantId() { Status = Voupon.Common.Enum.StatusTypeEnum.PENDING_REVIEW, MerchantId = merchantId });
            if (ProductPendingReviewResponse.Successful)
            {
                if (ProductPendingReviewResponse.Data != null)
                {
                    var productlist = (List<Common.Services.Products.Models.ProductModel>)ProductPendingReviewResponse.Data;
                    model.ProductPendingReview = productlist.Count;
                }
            }
            ApiResponseViewModel ProductPendingRevisionResponse = await Mediator.Send(new ProductListByStatusQueryAndMerchantId() { Status = Voupon.Common.Enum.StatusTypeEnum.PENDING_REVISION, MerchantId = merchantId });
            if (ProductPendingRevisionResponse.Successful)
            {
                if (ProductPendingRevisionResponse.Data != null)
                {
                    var productlist = (List<Common.Services.Products.Models.ProductModel>)ProductPendingRevisionResponse.Data;
                    model.ProductPendingRevision = productlist.Count;
                }
            }

            ApiResponseViewModel ProductAprrovedResponse = await Mediator.Send(new ProductListByStatusQueryAndMerchantId() { Status = Voupon.Common.Enum.StatusTypeEnum.APPROVED, MerchantId = merchantId });
            if (ProductAprrovedResponse.Successful)
            {
                if (ProductAprrovedResponse.Data != null)
                {
                    var productlist = (List<Common.Services.Products.Models.ProductModel>)ProductAprrovedResponse.Data;
                    model.ProductPublished = productlist.Where(x => x.IsPublished && x.IsActivated).Count();
                }
            }

            ApiResponseViewModel TotalProductResponse = await Mediator.Send(new MerchantProductListQuery() { MerchantId = merchantId });
            if (TotalProductResponse.Successful)
            {
                if (TotalProductResponse.Data != null)
                {
                    var productlist = (List<Common.Services.Products.Models.ProductModel>)TotalProductResponse.Data;
                    model.ProductTotal = productlist.Count();
                }
            }

            //model.TodayRevenue = await GetRevenueByDays(DateTime.Now, merchantId);
            model.SoldProductsLastOneWeek = await GetRevenueAndnumberOfSoldProductsByDays(DateTime.Now.AddDays(-7), merchantId);
            model.PendingProducts = await GetRevenueAndnumberOfPendingProducts(merchantId);
            return View(model);
        }

        private async Task<SoldProducts> GetRevenueAndnumberOfSoldProductsByDays(DateTime date, int MerchantId)
        {
            SoldProducts soldProductsModel = new SoldProducts();
            DateTime startDate = new DateTime(date.Year, date.Month, date.Day, 0, 0, 0);
            DateTime endDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 23, 59, 59);
            //DateTime endDate = new DateTime(date.Year, date.Month, date.Day, 23, 59, 59);
            decimal revenue = 0;
            int numberOfProducts = 0;
            ApiResponseViewModel InStoreResponse = await Mediator.Send(new Common.Services.InStoreRedemptionTokens.Queries.InStoreRedemptionSummaryWithDateQuery()
            {
                StartDate = startDate,
                EndDate = endDate,
                MerchantId = MerchantId
            });
            if (InStoreResponse.Successful)
            {
                var data = (List<Common.Services.InStoreRedemptionTokens.Queries.InStoreRedemptionSummary>)InStoreResponse.Data;
                revenue = revenue + data.Sum(x => x.TotalRevenue);
                numberOfProducts = numberOfProducts + data.Count();
            }

            ApiResponseViewModel DigitalResponse = await Mediator.Send(new Common.Services.DigitalRedemptionTokens.Queries.DigitalRedemptionSummaryWithDateQuery()
            {
                StartDate = startDate,
                EndDate = endDate,
                MerchantId = MerchantId
            });
            if (DigitalResponse.Successful)
            {
                var data = (List<Common.Services.DigitalRedemptionTokens.Queries.DigitalRedemptionSummary>)DigitalResponse.Data;
                revenue = revenue + data.Sum(x => x.TotalRevenue);
                numberOfProducts = numberOfProducts + data.Count();

            }

            ApiResponseViewModel DeliveryResponse = await Mediator.Send(new Common.Services.DeliveryRedemptionTokens.Queries.DeliveryRedemptionSummaryWithDateQuery()
            {
                StartDate = startDate,
                EndDate = endDate,
                MerchantId = MerchantId
            });
            if (DeliveryResponse.Successful)
            {
                var data = (List<Common.Services.DeliveryRedemptionTokens.Queries.DeliveryRedemptionSummary>)DeliveryResponse.Data;
                revenue = revenue + data.Sum(x => x.TotalRevenue);
                numberOfProducts = numberOfProducts + data.Count();

            }
            soldProductsModel.NumberOfSoldProductsInlastWeek = numberOfProducts;
            soldProductsModel.RevenueOfSoldProductsInOneWeek = revenue;
            return soldProductsModel;
        }

        private async Task<PendingProducts> GetRevenueAndnumberOfPendingProducts(int merchantId)
        {
            PendingProducts pendingProductsModel = new PendingProducts();

            decimal revenue = 0;
            int numberOfProducts = 0;

            ApiResponseViewModel response = await Mediator.Send(new InStoreRedemptionTokensWithMerchantIdQuery() { MerchantId = merchantId });
            if (response.Successful)
            {
                var data = (List<InStoreRedemptionTokensViewModel>)response.Data;
                var filteredData = data.Where(x => DateTime.Now.Subtract(x.CreatedAt).TotalDays <= 14).Where(x => x.IsRedeemed == false).ToList();
                revenue = revenue + filteredData.Sum(x => x.Revenue.Value);
                numberOfProducts = numberOfProducts + filteredData.Count();
            }


            response = await Mediator.Send(new DigitalRedemptionTokensWithMerchantIdQuery() { MerchantId = merchantId });
            if (response.Successful)
            {
                var data = (List<DigitalRedemptionTokensViewModel>)response.Data;
                var filteredData = data.Where(x => DateTime.Now.Subtract(x.CreatedAt).TotalDays <= 14).Where(x => x.IsRedeemed == false).ToList();

                revenue = revenue + filteredData.Sum(x => x.Revenue.Value);
                numberOfProducts = numberOfProducts + filteredData.Count();
            }

            response = await Mediator.Send(new DeliveryRedemptionTokensWithMerchantIdQuery() { MerchantId = merchantId });
            if (response.Successful)
            {
                var data = (List<DeliveryRedemptionTokensViewModel>)response.Data;
                var filteredData = data.Where(x => DateTime.Now.Subtract(x.CreatedAt).TotalDays <= 14).Where(x => x.IsRedeemed == false).ToList();

                revenue = revenue + filteredData.Sum(x => x.Revenue.Value);
                numberOfProducts = numberOfProducts + filteredData.Count();
            }

            pendingProductsModel.NumberOfPerndingProducts = numberOfProducts;
            pendingProductsModel.RevenueOfPerndingProducts = revenue;
            return pendingProductsModel;
        }

    }



}