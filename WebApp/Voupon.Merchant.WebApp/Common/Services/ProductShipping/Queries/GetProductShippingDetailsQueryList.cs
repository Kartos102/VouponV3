using MediatR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Merchant.WebApp.Common.Services.Logger;
using Voupon.Merchant.WebApp.Common.Services.ProductShipping.Models;
using Voupon.Merchant.WebApp.ViewModels;

namespace Voupon.Merchant.WebApp.Common.Services.ProductShipping.Queries
{  
    public class GetProductShippingDetailsQueryList : IRequest<ApiResponseViewModel>
    {
        public int ProductId { get; set; }
        public string UserEmail { get; set; }

    }
    public class GetProductShippingDetailsQueryListHandler : IRequestHandler<GetProductShippingDetailsQueryList, ApiResponseViewModel>
    {
        RewardsDBContext rewardsDBContext;
        public GetProductShippingDetailsQueryListHandler(RewardsDBContext rewardsDBContext)
        {
            this.rewardsDBContext = rewardsDBContext;
        }

        public async Task<ApiResponseViewModel> Handle(GetProductShippingDetailsQueryList request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();
            try
            {
                var items = await rewardsDBContext.ProductShippingCost.Include(x=> x.ShippingCost).ThenInclude(x=> x.Province).AsNoTracking().Where(x => x.ProductId == request.ProductId).FirstOrDefaultAsync();
                var shippingTypeList = await rewardsDBContext.ShippingTypes.AsNoTracking().Where(x=> x.IsActivated == true).ToListAsync();
                ProductShippingCostModel productShippingCostModel = new ProductShippingCostModel ();
                if (items != null)
                {
                    productShippingCostModel.Id = items.Id;
                    productShippingCostModel.ProductId = items.ProductId;
                    productShippingCostModel.ShippingTypeId = items.ShippingTypeId;
                    productShippingCostModel.ConditionalShippingCost = items.ConditionalShippingCost;
                    productShippingCostModel.ShippingCosts = new List<ShippingCostModel>();

                    foreach (var shippingCostDetail in items.ShippingCost)
                    {
                        ShippingCostModel shippingCostModel  = new ShippingCostModel();
                        shippingCostModel.Id = shippingCostDetail.Id;
                        shippingCostModel.ProductShippingId = shippingCostDetail.ProductShippingId;
                        shippingCostModel.ProvinceId = shippingCostDetail.ProvinceId;
                        shippingCostModel.ProvinceName = shippingCostDetail.Province.Name;
                        shippingCostModel.Cost = shippingCostDetail.Cost;
                        productShippingCostModel.ShippingCosts.Add(shippingCostModel);
                    }
                }
                GetProductShippingDetailsModel productShippingDetailsModel = new GetProductShippingDetailsModel
                {
                    productShippingCost = productShippingCostModel,
                    ShippingTypes = shippingTypeList
                };
                response.Successful = true;
                response.Message = "Get Product Shipping cost details Successfully";
                response.Data = productShippingDetailsModel;
            }
            catch (Exception ex)
            {
                var errorLogs = new ErrorLogs
                {
                    ActionName = "GetProductShippingDetailsQueryList",
                    ActionRequest = JsonConvert.SerializeObject(request),
                    CreatedAt = DateTime.Now,
                    Errors = ex.ToString(),
                    Email = request.UserEmail,
                    TypeId = CreateErrorLogCommand.Type.Service
                };

                rewardsDBContext.ErrorLogs.Add(errorLogs);
                await rewardsDBContext.SaveChangesAsync();
                response.Successful = false;
                response.Message = "Fail to get Product Shipping cost details";
            }

            return response;
        }
    }
    public class GetProductShippingDetailsModel
    {
        public ProductShippingCostModel productShippingCost { get; set; }
        public List<ShippingTypes> ShippingTypes { get; set; }
    }
}
