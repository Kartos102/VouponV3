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
using Voupon.Merchant.WebApp.Common.Services.Postcodes.Models;
using Voupon.Merchant.WebApp.Common.Services.ProductVariations.Models;
using Voupon.Merchant.WebApp.ViewModels;

namespace Voupon.Merchant.WebApp.Common.Services.ProductVariations.Queries
{  
    public class ProductVariationListQuery : IRequest<ApiResponseViewModel>
    {
        public int ProductId { get; set; }
        public string UserEmail { get; set; }
    }
    public class ProductVariationListQueryHandler : IRequestHandler<ProductVariationListQuery, ApiResponseViewModel>
    {
        RewardsDBContext rewardsDBContext;
        public ProductVariationListQueryHandler(RewardsDBContext rewardsDBContext)
        {
            this.rewardsDBContext = rewardsDBContext;
        }

        public async Task<ApiResponseViewModel> Handle(ProductVariationListQuery request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();
            try
            {
                var items = await rewardsDBContext.Variations.Include(x => x.VariationOptions).ThenInclude(x => x.VariationCombination).ThenInclude(x => x.ProductVariation).Where(x => x.ProductId == request.ProductId).ToListAsync();

                VariationModel variationListModel = new VariationModel();
                variationListModel.ProductId = request.ProductId;
                variationListModel.VariationList = new List<VariationList>();
                variationListModel.ProductVariationDetailsList = new List<ProductVariationDetailsList>();

                foreach (var variation in items)
                {
                    VariationList variationModel = new VariationList();
                    variationModel.IsFirstVariation = variation.IsFirstVariation;
                    variationModel.Name = variation.Name;
                    variationModel.ProductId = variation.ProductId;
                    variationModel.Id = variation.Id;
                    variationModel.VariationOptions = new List<Models.VariationOptions>();
                    foreach (var variationOption in variation.VariationOptions.OrderBy(x=> x.Order))
                    {
                        /*
                        Models.VariationOptions variationOptionModel = new Models.VariationOptions();
                        variationOptionModel.Name = variationOption.Name;
                        variationOptionModel.Order = variationOption.Order;
                        variationOptionModel.Id = variationOption.Id;
                        variationModel.VariationOptions.Add(variationOptionModel);
                        foreach(var productVariation in variationOption.VariationCombination.OrderBy(x=> x.ProductVariation.VariationCombination.OptionTwoId))
                        {
                            ProductVariationDetailsList productVariationDetails = new ProductVariationDetailsList();
                            productVariationDetails.AvailableQuantity = productVariation.ProductVariation.AvailableQuantity;
                            productVariationDetails.DiscountedPrice = productVariation.ProductVariation.DiscountedPrice.Value;
                            productVariationDetails.Price = productVariation.ProductVariation.Price;
                            productVariationDetails.Id = productVariation.ProductVariation.Id;
                            productVariationDetails.IsDiscountedPriceEnabled = productVariation.ProductVariation.IsDiscountedPriceEnabled;
                            productVariationDetails.Sku = productVariation.ProductVariation.SKU;
                            variationListModel.ProductVariationDetailsList.Add(productVariationDetails);
                        }
                        */
                    }
                    variationListModel.VariationList.Add(variationModel);
                }
                response.Successful = true;
                response.Data = variationListModel;
                response.Message = "Get Product Variation List Successfully";
            }
            catch (Exception ex)
            {
                var errorLogs = new ErrorLogs
                {
                    ActionName = "GetProductVariationList",
                    ActionRequest = JsonConvert.SerializeObject(request),
                    CreatedAt = DateTime.Now,
                    Errors = ex.ToString(),
                    Email = request.UserEmail,
                    TypeId = CreateErrorLogCommand.Type.Service
                };

                rewardsDBContext.ErrorLogs.Add(errorLogs);
                await rewardsDBContext.SaveChangesAsync();
                response.Successful = false;
                response.Message = "Fail to get Product Variation List";
            }
            return response;
        }
    }
}
