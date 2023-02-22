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
using Voupon.Merchant.WebApp.Common.Services.ProductVariations.Models;
using Voupon.Merchant.WebApp.ViewModels;

namespace Voupon.Merchant.WebApp.Common.Services.ProductVariations.Command
{
    public class UpdateProductVariationStatusCommand : IRequest<ApiResponseViewModel>
    {
        public int ProductId { get; set; }
        public bool Status { get; set; }
        public bool IsCallFromMerchant { get; set; }
    }
    public class UpdateProductVariationStatusCommandHandler : IRequestHandler<UpdateProductVariationStatusCommand, ApiResponseViewModel>
    {
        RewardsDBContext rewardsDBContext;
        public UpdateProductVariationStatusCommandHandler(RewardsDBContext rewardsDBContext)
        {
            this.rewardsDBContext = rewardsDBContext;
        }

        public async Task<ApiResponseViewModel> Handle(UpdateProductVariationStatusCommand request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();
            try
            {
                if (request.Status == false)
                {
                    var oldVariations = await rewardsDBContext.Variations.Include(x => x.VariationOptions).ThenInclude(x => x.VariationCombination).ThenInclude(x => x.ProductVariation).Where(x => x.ProductId == request.ProductId).ToListAsync();
                    List<int> ProductVariationIds = new List<int>();
                    foreach (var variations in oldVariations)
                        foreach (var variationOptions in variations.VariationOptions)
                            foreach (var variationCombination in variationOptions.VariationCombination)
                            {
                                //ProductVariationIds.Add(variationCombination.ProductVariation.Id);
                            }
                    var cartDeletedProducts = await rewardsDBContext.CartProducts.Where(x => ProductVariationIds.Contains(x.VariationId.Value)).ToListAsync();
                    foreach (var cartProduct in cartDeletedProducts)
                    {
                        cartProduct.VariationId = null;
                        cartProduct.CartProductType = 3;
                        cartProduct.UpdatedAt = DateTime.Now;
                    }
                    rewardsDBContext.CartProducts.UpdateRange(cartDeletedProducts);
                    rewardsDBContext.Variations.RemoveRange(oldVariations);
                }
                var product = await rewardsDBContext.Products.Where(x => x.Id == request.ProductId).FirstOrDefaultAsync();
                if(product != null)
                {
                    if (request.IsCallFromMerchant)
                    {
                        var jsonString = "";
                        if (String.IsNullOrEmpty(product.PendingChanges))
                        {
                            jsonString = JsonConvert.SerializeObject(product);
                            var newItem = JsonConvert.DeserializeObject<Voupon.Database.Postgres.RewardsEntities.Products>(jsonString);
                            newItem.IsProductVariationEnabled = request.Status;

                            product.PendingChanges = JsonConvert.SerializeObject(newItem);
                        }
                        else
                        {
                            jsonString = product.PendingChanges;
                            var newItem = JsonConvert.DeserializeObject<Voupon.Database.Postgres.RewardsEntities.Products>(jsonString);
                            newItem.IsProductVariationEnabled = request.Status;

                            product.PendingChanges = JsonConvert.SerializeObject(newItem);
                        }
                    }
                    else
                    {
                        product.IsProductVariationEnabled = request.Status;
                    }
                }
                rewardsDBContext.SaveChanges();


                response.Successful = true;
                response.Message = "Update Product variations Successfully";
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }

            return response;
        }
    }
}
