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
    public class DigitalRedemptionWithDateQuery : IRequest<ApiResponseViewModel>
    {
        public int MerchantId { get; set; }  
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
    public class DigitalRedemptionWithDateQueryHandler : IRequestHandler<DigitalRedemptionWithDateQuery, ApiResponseViewModel>
    {
        RewardsDBContext rewardsDBContext;
        public DigitalRedemptionWithDateQueryHandler(RewardsDBContext rewardsDBContext)
        {
            this.rewardsDBContext = rewardsDBContext;
        }

        public async Task<ApiResponseViewModel> Handle(DigitalRedemptionWithDateQuery request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();
            try
            {
                var item = await rewardsDBContext.DigitalRedemptionTokens.Where(x => x.MerchantId == request.MerchantId && x.IsRedeemed==true && x.RedeemedAt!=null && x.RedeemedAt>=request.StartDate && x.RedeemedAt <= request.EndDate).ToListAsync();         
                if(item!=null)
                {                 
                    response.Successful = true;
                    response.Message = "Get Digital Redemption Successfully";
                    response.Data = item;
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
