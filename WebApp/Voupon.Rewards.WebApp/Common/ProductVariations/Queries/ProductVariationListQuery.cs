using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Rewards.WebApp.Common.Services.ProductVariations.Models;
using Voupon.Rewards.WebApp.ViewModels;

namespace Voupon.Rewards.WebApp.Common.Services.ProductVariations.Queries
{
    public class ProductVariationListQuery : IRequest<ApiResponseViewModel>
    {
        public int ProductId { get; set; }
        public string ExternalId { get; set; }
        public string ExternalMerchantId { get; set; }
        public byte ExternalTypeId { get; set; }
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
                if (request.ProductId == 0)
                {
                    var product = await rewardsDBContext.Products.Where(x => x.ExternalId == request.ExternalId && x.ExternalMerchantId == request.ExternalMerchantId && x.ExternalTypeId == request.ExternalTypeId).FirstOrDefaultAsync();

                    if (product == null)
                    {
                        response.Data = null;
                        response.Message = "Get Product Variation List Successfully";
                        return response;
                    }

                    request.ProductId = product.Id;

                    var items = await rewardsDBContext.Variations.Include(x => x.VariationOptions).ThenInclude(x => x.VariationCombination).ThenInclude(x => x.ProductVariation).Where(x => x.ProductId == product.Id).ToListAsync();

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
                        variationModel.VariationOptions = new List<Models.VariationOptions>();
                        foreach (var variationOption in variation.VariationOptions)
                        {
                            Models.VariationOptions variationOptionModel = new Models.VariationOptions();
                            variationOptionModel.Name = variationOption.Name;
                            variationOptionModel.Order = variationOption.Order;
                            variationModel.VariationOptions.Add(variationOptionModel);
                            int variation2Order = 0;
                            /*
                            foreach (var productVariation in variationOption.VariationCombination.OrderBy(x => x.ProductVariation.VariationCombination.OptionTwoId))
                            {
                                ProductVariationDetailsList productVariationDetails = new ProductVariationDetailsList();
                                productVariationDetails.AvailableQuantity = productVariation.ProductVariation.AvailableQuantity;
                                productVariationDetails.Id = productVariation.ProductVariation.Id;
                                productVariationDetails.DiscountedPrice = productVariation.ProductVariation.DiscountedPrice.Value;
                                productVariationDetails.Price = productVariation.ProductVariation.Price;
                                productVariationDetails.Order = variationOption.Order + "," + variation2Order;
                                productVariationDetails.IsDiscountedPriceEnabled = productVariation.ProductVariation.IsDiscountedPriceEnabled;
                                productVariationDetails.Sku = productVariation.ProductVariation.SKU;
                                variationListModel.ProductVariationDetailsList.Add(productVariationDetails);
                                variation2Order++;
                            }
                            */
                        }
                        variationListModel.VariationList.Add(variationModel);
                    }
                    response.Successful = true;
                    //response.Data = items;
                    response.Data = variationListModel;
                    response.Message = "Get Product Variation List Successfully";
                }
                else
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
                        variationModel.VariationOptions = new List<Models.VariationOptions>();
                        foreach (var variationOption in variation.VariationOptions)
                        {
                            Models.VariationOptions variationOptionModel = new Models.VariationOptions();
                            variationOptionModel.Name = variationOption.Name;
                            variationOptionModel.Order = variationOption.Order;
                            variationModel.VariationOptions.Add(variationOptionModel);
                            int variation2Order = 0;
                            /*
                            foreach (var productVariation in variationOption.VariationCombination.OrderBy(x => x.ProductVariation.VariationCombination.OptionTwoId))
                            {
                                ProductVariationDetailsList productVariationDetails = new ProductVariationDetailsList();
                                productVariationDetails.AvailableQuantity = productVariation.ProductVariation.AvailableQuantity;
                                productVariationDetails.Id = productVariation.ProductVariation.Id;
                                productVariationDetails.DiscountedPrice = productVariation.ProductVariation.DiscountedPrice.Value;
                                productVariationDetails.Price = productVariation.ProductVariation.Price;
                                productVariationDetails.Order = variationOption.Order + "," + variation2Order;
                                productVariationDetails.IsDiscountedPriceEnabled = productVariation.ProductVariation.IsDiscountedPriceEnabled;
                                productVariationDetails.Sku = productVariation.ProductVariation.SKU;
                                variationListModel.ProductVariationDetailsList.Add(productVariationDetails);
                                variation2Order++;
                            }
                            */
                        }
                        variationListModel.VariationList.Add(variationModel);
                    }
                    response.Successful = true;
                    //response.Data = items;
                    response.Data = variationListModel;
                    response.Message = "Get Product Variation List Successfully";
                }

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
