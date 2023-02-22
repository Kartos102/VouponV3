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

namespace Voupon.Merchant.WebApp.Common.Services.DigitalRedemptionTokens.Queries
{
    public class DigitalRedemptionSummary
    {
        public string ProductName { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalTransaction { get; set; }
    }
    public class DigitalRedemptionSummaryWithDateQuery : IRequest<ApiResponseViewModel>
    {
        public int MerchantId { get; set; }  
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
    public class DigitalRedemptionSummaryWithDateQueryHandler : IRequestHandler<DigitalRedemptionSummaryWithDateQuery, ApiResponseViewModel>
    {
        RewardsDBContext rewardsDBContext;
        public DigitalRedemptionSummaryWithDateQueryHandler(RewardsDBContext rewardsDBContext)
        {
            this.rewardsDBContext = rewardsDBContext;
        }

        public async Task<ApiResponseViewModel> Handle(DigitalRedemptionSummaryWithDateQuery request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();
            try
            {
                var item = await rewardsDBContext.DigitalRedemptionTokens.Where(x => x.MerchantId == request.MerchantId && x.IsRedeemed==true && x.RedeemedAt!=null && x.RedeemedAt>=request.StartDate && x.RedeemedAt <= request.EndDate).ToListAsync();         
                if(item!=null)
                {
                    List<DigitalRedemptionSummary> summary = new List<DigitalRedemptionSummary>();
                    var grouping=item.GroupBy(x => x.ProductTitle);
                    foreach(var group in grouping)
                    {
                        DigitalRedemptionSummary groupSummary = new DigitalRedemptionSummary();
                        groupSummary.ProductName = group.Key;
                        groupSummary.TotalTransaction = group.Count();
                        groupSummary.TotalRevenue = group.Sum(x => x.Revenue).Value;
                        summary.Add(groupSummary);
                    }
                    response.Successful = true;
                    response.Message = "Get Digital Redemption Summary Successfully";
                    response.Data = summary;
                }
                else
                {                 
                    response.Successful = false;
                    response.Message = "No Digital Redemption Summary";
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
