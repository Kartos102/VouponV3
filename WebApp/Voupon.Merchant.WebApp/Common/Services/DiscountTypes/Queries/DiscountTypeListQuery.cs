using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Merchant.WebApp.Common.Services.DealTypes.Models;
using Voupon.Merchant.WebApp.ViewModels;

namespace Voupon.Merchant.WebApp.Common.Services.DiscountTypes.Queries
{   
    public class DiscountTypeListQuery : IRequest<ApiResponseViewModel>
    {
    }
    public class DiscountTypeListQueryHandler : IRequestHandler<DiscountTypeListQuery, ApiResponseViewModel>
    {
        RewardsDBContext rewardsDBContext;
        public DiscountTypeListQueryHandler(RewardsDBContext rewardsDBContext)
        {
            this.rewardsDBContext = rewardsDBContext;
        }

        public async Task<ApiResponseViewModel> Handle(DiscountTypeListQuery request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();
            try
            {
                var items = await rewardsDBContext.DiscountTypes.ToListAsync();               
                response.Successful = true;
                response.Message = "Get Discount Type List Successfully";
                response.Data = items;
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }

            return response;
        }
    }
}
