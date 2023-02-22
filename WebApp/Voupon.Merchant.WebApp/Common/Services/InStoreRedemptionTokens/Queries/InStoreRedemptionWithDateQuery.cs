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
    public class InStoreRedemptionWithDateQuery : IRequest<ApiResponseViewModel>
    {
        public int MerchantId { get; set; }  
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
    public class InStoreRedemptionWithDateQueryHandler : IRequestHandler<InStoreRedemptionWithDateQuery, ApiResponseViewModel>
    {
        RewardsDBContext rewardsDBContext;
        public InStoreRedemptionWithDateQueryHandler(RewardsDBContext rewardsDBContext)
        {
            this.rewardsDBContext = rewardsDBContext;
        }

        public async Task<ApiResponseViewModel> Handle(InStoreRedemptionWithDateQuery request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();
            try
            {
                var item = await rewardsDBContext.InStoreRedemptionTokens.Where(x => x.MerchantId == request.MerchantId && x.IsRedeemed==true && x.RedeemedAt!=null && x.RedeemedAt>=request.StartDate && x.RedeemedAt <= request.EndDate).Include(x=>x.Merchant).Include(x=>x.Outlet).ToListAsync();         
                if(item!=null)
                {                  
                    response.Successful = true;
                    response.Message = "Get InStore Redemption Successfully";
                    response.Data = item;
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
