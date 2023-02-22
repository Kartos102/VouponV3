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

namespace Voupon.Merchant.WebApp.Common.Services.DeliveryRedemptionTokens.Queries
{
    public class DeliveryRedemptionSummary
    {
        public string ProductName { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalTransaction { get; set; }
    }
    public class DeliveryRedemptionSummaryWithDateQuery : IRequest<ApiResponseViewModel>
    {
        public int MerchantId { get; set; }  
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
    public class DeliveryRedemptionSummaryWithDateQueryHandler : IRequestHandler<DeliveryRedemptionSummaryWithDateQuery, ApiResponseViewModel>
    {
        RewardsDBContext rewardsDBContext;
        public DeliveryRedemptionSummaryWithDateQueryHandler(RewardsDBContext rewardsDBContext)
        {
            this.rewardsDBContext = rewardsDBContext;
        }

        public async Task<ApiResponseViewModel> Handle(DeliveryRedemptionSummaryWithDateQuery request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();
            try
            {
                var item = await rewardsDBContext.DeliveryRedemptionTokens.Where(x => x.MerchantId == request.MerchantId && x.IsRedeemed==true && x.RedeemedAt!=null && x.RedeemedAt>=request.StartDate && x.RedeemedAt <= request.EndDate).ToListAsync();         
                if(item!=null)
                {
                    List<DeliveryRedemptionSummary> summary = new List<DeliveryRedemptionSummary>();
                    var grouping=item.GroupBy(x => x.ProductTitle);
                    foreach(var group in grouping)
                    {
                        DeliveryRedemptionSummary groupSummary = new DeliveryRedemptionSummary();
                        groupSummary.ProductName = group.Key;
                        groupSummary.TotalTransaction = group.Count();
                        groupSummary.TotalRevenue = group.Sum(x => x.Revenue).Value;
                        summary.Add(groupSummary);
                    }
                    response.Successful = true;
                    response.Message = "Get Delivery Redemption Summary Successfully";
                    response.Data = summary;
                }
                else
                {                 
                    response.Successful = false;
                    response.Message = "No Delivery Redemption Summary Successfully";
                    response.Data = null;
                }               
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }

            return response;
        }
    }
}
