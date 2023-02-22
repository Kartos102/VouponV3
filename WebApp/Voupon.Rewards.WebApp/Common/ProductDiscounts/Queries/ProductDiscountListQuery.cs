using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Rewards.WebApp.ViewModels;

namespace Voupon.Rewards.WebApp.Common.ProductDiscounts.Queries
{  
    public class ProductDiscountListQuery : IRequest<ApiResponseViewModel>
    {
        public int ProductId { get; set; }
    }
    public class ProductDiscountListQueryHandler : IRequestHandler<ProductDiscountListQuery, ApiResponseViewModel>
    {
        RewardsDBContext rewardsDBContext;
        public ProductDiscountListQueryHandler(RewardsDBContext rewardsDBContext)
        {
            this.rewardsDBContext = rewardsDBContext;
        }

        public async Task<ApiResponseViewModel> Handle(ProductDiscountListQuery request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();
            try
            {
                var items = await rewardsDBContext.ProductDiscounts.Where(x => x.ProductId == request.ProductId).ToListAsync();              
                response.Successful = true;
                response.Data = items;
                response.Message = "Get Product Discount List Successfully";
            }
            catch (Exception ex)
            {
                response.Successful = false;
                response.Message = ex.Message;
            }
            return response;
        }
    }
}
