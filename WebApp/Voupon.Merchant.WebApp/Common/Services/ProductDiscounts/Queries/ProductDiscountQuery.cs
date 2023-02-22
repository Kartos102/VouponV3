using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Merchant.WebApp.Common.Services.Postcodes.Models;
using Voupon.Merchant.WebApp.ViewModels;

namespace Voupon.Merchant.WebApp.Common.Services.ProductDiscounts.Queries
{  
    public class ProductDiscountQuery : IRequest<ApiResponseViewModel>
    {
        public int Id { get; set; }
    }
    public class ProductDiscountQueryHandler : IRequestHandler<ProductDiscountQuery, ApiResponseViewModel>
    {
        RewardsDBContext rewardsDBContext;
        public ProductDiscountQueryHandler(RewardsDBContext rewardsDBContext)
        {
            this.rewardsDBContext = rewardsDBContext;
        }

        public async Task<ApiResponseViewModel> Handle(ProductDiscountQuery request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();
            try
            {
                var items = await rewardsDBContext.ProductDiscounts.Where(x => x.Id == request.Id).FirstOrDefaultAsync();              
                response.Successful = true;
                response.Data = items;
                response.Message = "Get Product Discount Successfully";
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
