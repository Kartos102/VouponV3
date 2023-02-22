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
using Voupon.Merchant.WebApp.Common.Services.Logger;
using Voupon.Merchant.WebApp.Common.Services.ProductShipping.Models;
using Voupon.Merchant.WebApp.ViewModels;

namespace Voupon.Merchant.WebApp.Common.Services.Products.Command
{
    public class UpdateProductShippingCostCommand : IRequest<ApiResponseViewModel>
    {
       public ProductShippingCostModel ProductShippingCostModel { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid CreatedByUserId { get; set; }
        public string UserEmail { get; set; }

    }

    public class UpdateProductShippingCostCommandHandler : IRequestHandler<UpdateProductShippingCostCommand, ApiResponseViewModel>
    {
        RewardsDBContext rewardsDBContext;
        public UpdateProductShippingCostCommandHandler(RewardsDBContext rewardsDBContext)
        {
            this.rewardsDBContext = rewardsDBContext;
        }

        public async Task<ApiResponseViewModel> Handle(UpdateProductShippingCostCommand request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();
            try
            {
                var productShippingCost = await rewardsDBContext.ProductShippingCost.Include(x => x.ShippingCost).Where(x => x.ProductId == request.ProductShippingCostModel.ProductId).FirstOrDefaultAsync();
                if (productShippingCost != null)
                {
                    productShippingCost.ShippingTypeId = request.ProductShippingCostModel.ShippingTypeId;
                    productShippingCost.ConditionalShippingCost = request.ProductShippingCostModel.ConditionalShippingCost;
                    productShippingCost.LastUpdatedAt = request.CreatedAt;
                    productShippingCost.LastUpdatedByUser = request.CreatedByUserId;

                    foreach (var shippingCost in request.ProductShippingCostModel.ShippingCosts)
                    {
                        var dbShippingCost = productShippingCost.ShippingCost.Where(x => x.ProvinceId == shippingCost.ProvinceId).FirstOrDefault();
                        if (dbShippingCost != null)
                        {
                            dbShippingCost.Cost = shippingCost.Cost;
                            dbShippingCost.LastUpdatedAt = request.CreatedAt;
                            dbShippingCost.LastUpdatedByUser = request.CreatedByUserId;
                        }
                        else
                        {
                            ShippingCost newShippingCost = new ShippingCost();
                            newShippingCost.Cost = shippingCost.Cost;
                            newShippingCost.CreatedAt = request.CreatedAt;
                            newShippingCost.CreatedByUserId = request.CreatedByUserId;
                            if(productShippingCost.ShippingCost == null)
                            {
                                productShippingCost.ShippingCost = new List<ShippingCost>();
                            }
                            productShippingCost.ShippingCost.Add(newShippingCost);
                        }
                    }
                    rewardsDBContext.ProductShippingCost.Update(productShippingCost);
                    rewardsDBContext.SaveChanges();
                    response.Successful = true;
                    response.Message = "Update Product Successfully";
                }
                else
                {
                    ProductShippingCost newProductShippingCost = new ProductShippingCost();
                    newProductShippingCost.ShippingTypeId = request.ProductShippingCostModel.ShippingTypeId;
                    newProductShippingCost.ProductId = request.ProductShippingCostModel.ProductId;
                    newProductShippingCost.ConditionalShippingCost = request.ProductShippingCostModel.ConditionalShippingCost;
                    newProductShippingCost.CreatedAt = request.CreatedAt;
                    newProductShippingCost.CreatedByUserId = request.CreatedByUserId;
                    newProductShippingCost.ShippingCost = new List<ShippingCost>();

                    foreach (var shippingCost in request.ProductShippingCostModel.ShippingCosts)
                    {
                        ShippingCost newShippingCost = new ShippingCost();
                        newShippingCost.Cost = shippingCost.Cost;
                        newShippingCost.ProvinceId = shippingCost.ProvinceId;
                        newShippingCost.CreatedAt = request.CreatedAt;
                        newShippingCost.CreatedByUserId = request.CreatedByUserId;
                        newProductShippingCost.ShippingCost.Add(newShippingCost);
                    }
                    rewardsDBContext.ProductShippingCost.Add(newProductShippingCost);
                    rewardsDBContext.SaveChanges();
                    response.Successful = true;
                    response.Message = "Update Product Successfully";
                }
            }
            catch (Exception ex)
            {
                var errorLogs = new ErrorLogs
                {
                    ActionName = "UpdateProductShippingCostCommand",
                    ActionRequest = JsonConvert.SerializeObject(request),
                    CreatedAt = DateTime.Now,
                    Errors = ex.ToString(),
                    Email = request.UserEmail,
                    TypeId = CreateErrorLogCommand.Type.Service
                };

                rewardsDBContext.ErrorLogs.Add(errorLogs);
                await rewardsDBContext.SaveChangesAsync();
                response.Successful = false;
                response.Message = "Product not found";
            }

            return response;
        }
    }
}
