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

namespace Voupon.Merchant.WebApp.Common.Services.LuckyDraws.Queries
{
    public class LuckyDrawQuery : IRequest<ApiResponseViewModel>
    {
        public int ProductId { get; set; }      
    }
    public class LuckyDrawQueryHandler : IRequestHandler<LuckyDrawQuery, ApiResponseViewModel>
    {
        RewardsDBContext rewardsDBContext;
        public LuckyDrawQueryHandler(RewardsDBContext rewardsDBContext)
        {
            this.rewardsDBContext = rewardsDBContext;
        }

        public async Task<ApiResponseViewModel> Handle(LuckyDrawQuery request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();
            try
            {
                var item = await rewardsDBContext.LuckyDraws.FirstOrDefaultAsync(x => x.ProductId == request.ProductId);         
                if(item!=null)
                {
                    response.Successful = true;
                    response.Message = "Get Product Lucky Draw Successfully";
                    response.Data = item;
                }
                else
                {                 
                    response.Successful = false;
                    response.Message = "Product Lucky Draw Haven't Created";
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
