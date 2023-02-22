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

namespace Voupon.Merchant.WebApp.Common.Services.InStoreRedemptionTokens.Queries
{
    public class InStoreRedemptionSummary
    {
        public string OutletName { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalTransaction { get; set; }
    }
    public class InStoreRedemptionSummaryWithDateQuery : IRequest<ApiResponseViewModel>
    {
        public int MerchantId { get; set; }  
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
    public class InStoreRedemptionSummaryWithDateQueryHandler : IRequestHandler<InStoreRedemptionSummaryWithDateQuery, ApiResponseViewModel>
    {
        RewardsDBContext rewardsDBContext;
        public InStoreRedemptionSummaryWithDateQueryHandler(RewardsDBContext rewardsDBContext)
        {
            this.rewardsDBContext = rewardsDBContext;
        }

        public async Task<ApiResponseViewModel> Handle(InStoreRedemptionSummaryWithDateQuery request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();
            try
            {
                var item = await rewardsDBContext.InStoreRedemptionTokens.Where(x => x.MerchantId == request.MerchantId && x.IsRedeemed==true && x.RedeemedAt!=null && x.RedeemedAt>=request.StartDate && x.RedeemedAt <= request.EndDate).Include(x=>x.Outlet).ToListAsync();         
                if(item!=null)
                {
                    List<InStoreRedemptionSummary> summary = new List<InStoreRedemptionSummary>();
                    var grouping=item.GroupBy(x => x.OutletId);
                    foreach(var group in grouping)
                    {
                        InStoreRedemptionSummary groupSummary = new InStoreRedemptionSummary();
                        groupSummary.OutletName = group.First().Outlet.Name;
                        groupSummary.TotalTransaction = group.Count();
                        groupSummary.TotalRevenue = group.Sum(x => x.Revenue).Value;
                        summary.Add(groupSummary);
                    }
                    response.Successful = true;
                    response.Message = "Get InStore Redemption Summary Successfully";
                    response.Data = summary;
                }
                else
                {                 
                    response.Successful = false;
                    response.Message = "No InStore Redemption Summary";
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
