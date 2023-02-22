using MediatR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Common.Enum;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Merchant.WebApp.Common.Services.Products.Models;
using Voupon.Merchant.WebApp.ViewModels;

namespace Voupon.Merchant.WebApp.Common.Services.Products.Command
{
    public class UpdateAllProductsWithVpointsByCommercialRateCommand : IRequest<ApiResponseViewModel>
    {

    }

    public class UpdateAllProductsWithVpointsByCommercialRateCommandHandler : IRequestHandler<UpdateAllProductsWithVpointsByCommercialRateCommand, ApiResponseViewModel>
    {
        RewardsDBContext rewardsDBContext;
        public UpdateAllProductsWithVpointsByCommercialRateCommandHandler(RewardsDBContext rewardsDBContext)
        {
            this.rewardsDBContext = rewardsDBContext;
        }

        public async Task<ApiResponseViewModel> Handle(UpdateAllProductsWithVpointsByCommercialRateCommand request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();
            response.Message = "Update Product discounts Successfully";
            response.Successful = true;
            var appConfige = await rewardsDBContext.AppConfig.FirstOrDefaultAsync();
            var commercialRate = appConfige.RinggitPerVpoints;
            var products = await rewardsDBContext.Products.Include(x=> x.ProductDiscounts).ToListAsync();
            if (products != null || products.Count > 0)
            {
                foreach (var product in products)
                {
                    foreach(var productDiscount in product.ProductDiscounts)
                    {
                        if(productDiscount.DiscountTypeId == 4)
                        {
                            productDiscount.PointRequired = (int)Math.Round(productDiscount.PriceValue / commercialRate);
                            productDiscount.Name = "RM" + (int)productDiscount.PriceValue + " DISCOUNT (" + productDiscount.PointRequired + " VPoints)";
                        }
                    }
                   
                    rewardsDBContext.SaveChanges();
                }
                response.Successful = true;
                response.Message = "Update Product discounts Successfully";
            }
            else
            {
                response.Message = "Fail to update product discounts";
            }
            return response;
        }
    }
}
