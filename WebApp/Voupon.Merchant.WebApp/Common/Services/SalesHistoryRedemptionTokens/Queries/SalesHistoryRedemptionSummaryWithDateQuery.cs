using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Merchant.WebApp.Common.Services.Products.Models;
using Voupon.Merchant.WebApp.ViewModels;

namespace Voupon.Merchant.WebApp.Common.Services.SalesHistoryRedemptionTokens.Queries
{
    public class SalesHistoryRedemptionSummary
    {
        public string ProductName { get; set; }
        public string Type { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalTransaction { get; set; }
    }
    public class SalesHistoryRedemptionSummaryWithDateQuery : IRequest<ApiResponseViewModel>
    {
        public int MerchantId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
    public class SalesHistoryRedemptionSummaryWithDateQueryHandler : IRequestHandler<SalesHistoryRedemptionSummaryWithDateQuery, ApiResponseViewModel>
    {
        RewardsDBContext rewardsDBContext;
        public SalesHistoryRedemptionSummaryWithDateQueryHandler(RewardsDBContext rewardsDBContext)
        {
            this.rewardsDBContext = rewardsDBContext;
        }

        public async Task<ApiResponseViewModel> Handle(SalesHistoryRedemptionSummaryWithDateQuery request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();
            try
            {
                var inStoreItems = await rewardsDBContext.InStoreRedemptionTokens.Where(x => x.MerchantId == request.MerchantId && x.IsRedeemed == true && x.RedeemedAt != null && x.RedeemedAt >= request.StartDate && x.RedeemedAt <= request.EndDate).Include(x => x.Outlet).ToListAsync();

                var deliveryItems = await rewardsDBContext.DeliveryRedemptionTokens.Where(x => x.MerchantId == request.MerchantId && x.IsRedeemed == true && x.RedeemedAt != null && x.RedeemedAt >= request.StartDate && x.RedeemedAt <= request.EndDate).ToListAsync();

                var digitalItem = await rewardsDBContext.DigitalRedemptionTokens.Where(x => x.MerchantId == request.MerchantId && x.IsRedeemed == true && x.RedeemedAt != null && x.RedeemedAt >= request.StartDate && x.RedeemedAt <= request.EndDate).ToListAsync();

                List<SalesHistoryRedemptionSummary> summary = new List<SalesHistoryRedemptionSummary>();

                if (inStoreItems != null)
                {
                    var grouping = inStoreItems.GroupBy(x => x.ProductId);
                    foreach (var group in grouping)
                    {
                        SalesHistoryRedemptionSummary groupSummary = new SalesHistoryRedemptionSummary();
                        groupSummary.ProductName = group.First().ProductTitle;
                        groupSummary.TotalTransaction = group.Count();
                        groupSummary.TotalRevenue = group.Sum(x => x.Revenue).Value;
                        groupSummary.Type = "Outlet";
                        summary.Add(groupSummary);
                    }

                }
                else
                {
                    response.Successful = false;
                    response.Message = "No InStore Redemption Summary";
                    response.Data = null;
                }

                if (deliveryItems != null)
                {
                    var grouping = deliveryItems.GroupBy(x => x.ProductTitle);
                    foreach (var group in grouping)
                    {
                        SalesHistoryRedemptionSummary groupSummary = new SalesHistoryRedemptionSummary();
                        groupSummary.ProductName = group.Key;
                        groupSummary.TotalTransaction = group.Count();
                        groupSummary.TotalRevenue = group.Sum(x => x.Revenue).Value;
                        groupSummary.Type = "Ecommerce";

                        summary.Add(groupSummary);
                    }
                }
                else
                {
                    response.Successful = false;
                    response.Message = "No Delivery Redemption Summary Successfully";
                    response.Data = null;
                }

                if (digitalItem != null)
                {
                    var grouping = digitalItem.GroupBy(x => x.ProductTitle);
                    foreach (var group in grouping)
                    {
                        SalesHistoryRedemptionSummary groupSummary = new SalesHistoryRedemptionSummary();
                        groupSummary.ProductName = group.Key;
                        groupSummary.TotalTransaction = group.Count();
                        groupSummary.TotalRevenue = group.Sum(x => x.Revenue).Value;
                        groupSummary.Type = "E-voucher";

                        summary.Add(groupSummary);
                    }
                }
                else
                {
                    response.Successful = false;
                    response.Message = "No Digital Redemption Summary";
                    response.Data = null;
                }

                response.Successful = true;
                response.Message = "Get InStore Redemption Summary Successfully";
                response.Data = summary;
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }

            return response;
        }
    }
}
