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
using Voupon.Database.Postgres.VodusEntities;
using Voupon.Merchant.WebApp.Common.Services.Logger;

namespace Voupon.Merchant.WebApp.Common.Services.ProductVariations.Command
{
    public class CreateProductVariationCommand : IRequest<ApiResponseViewModel>
    {
        public int ProductId { get; set; }
        public bool IsCallFromMerchant { get; set; }
        public string UserEmail { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid CreatedByUserId { get; set; }
        public List<VariationList> VariationList { get; set; }
        public List<ProductVariationDetailsList> ProductVariationDetailsList { get; set; }
        public List<string> ImagesList { get; set; }
    }
    public class CreateProductVariationCommandHandler : IRequestHandler<CreateProductVariationCommand, ApiResponseViewModel>
    {
        RewardsDBContext rewardsDBContext;
        VodusV2Context vodusV2Context;
        public CreateProductVariationCommandHandler(RewardsDBContext rewardsDBContext, VodusV2Context vodusV2Context)
        {
            this.rewardsDBContext = rewardsDBContext;
            this.vodusV2Context = vodusV2Context;
        }

        public async Task<ApiResponseViewModel> Handle(CreateProductVariationCommand request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();
            try
            {
                var oldVariations = await rewardsDBContext.Variations.Include(x => x.VariationOptions).ThenInclude(x => x.VariationCombination).ThenInclude(x=> x.ProductVariation).Where(x => x.ProductId == request.ProductId).ToListAsync();
                List<int> productVariationOldIds = new List<int>();
                List<int> variationOldIds = new List<int>();
                List<int> variationOptionsOldIds = new List<int>();
                foreach (var variations in oldVariations) { 
                    foreach (var variationOptions in variations.VariationOptions) { 
                        foreach (var variationCombination in variationOptions.VariationCombination)
                        {
                            //productVariationOldIds.Add(variationCombination.ProductVariation.Id);
                        }
                        variationOptionsOldIds.Add(variationOptions.Id);
                    }
                    variationOldIds.Add(variations.Id);
                }
                foreach(var productVariationDetails in request.ProductVariationDetailsList)
                {
                    if(productVariationDetails.DiscountedPrice == 0)
                    {
                        productVariationDetails.DiscountedPrice = productVariationDetails.Price;
                    }
                }

                List<int> productVariationNewIds = new List<int>();
                List<int> variationNewIds = new List<int>();
                List<int> variationOptionsNewIds = new List<int>();
                foreach (var variations in request.VariationList)
                {
                    foreach (var variationOptions in variations.VariationOptions)
                    {
                        variationOptionsNewIds.Add(variationOptions.Id);
                    }
                    variationNewIds.Add(variations.Id);
                }

                foreach (var ProductVariation in request.ProductVariationDetailsList)
                {
                    productVariationNewIds.Add(ProductVariation.Id);
                }

                List<int> removedProductVariationOldIds = new List<int>();
                List<int> removedVariationOldIds = new List<int>();
                List<int> removedVariationOptionsOldIds = new List<int>();

                foreach (var variationOldId in variationOldIds)
                {
                    if (!variationNewIds.Contains(variationOldId))
                    {
                        removedVariationOldIds.Add(variationOldId);
                    }
                }
                
                foreach (var variationOptionsOldId in variationOptionsOldIds)
                {
                    if (!variationOptionsNewIds.Contains(variationOptionsOldId))
                    {
                        removedVariationOptionsOldIds.Add(variationOptionsOldId);
                    }
                }
                
                foreach (var productVariationOldId in productVariationOldIds)
                {
                    if (!productVariationNewIds.Contains(productVariationOldId))
                    {
                        removedProductVariationOldIds.Add(productVariationOldId);
                    }
                }
                var removedOldVariations = rewardsDBContext.Variations.Include(x => x.VariationOptions).ThenInclude(x => x.VariationCombination).ThenInclude(x => x.ProductVariation).Where(x => removedVariationOldIds.Contains(x.Id) && x.ProductId == request.ProductId).ToList();
                var removedOldVariationsOptions = rewardsDBContext.VariationOptions.Include(x => x.VariationCombination).ThenInclude(x=> x.ProductVariation).Where(x => removedVariationOptionsOldIds.Contains(x.Id)).ToList();
                var removedOldProductVariations = rewardsDBContext.ProductVariation.Where(x => removedProductVariationOldIds.Contains(x.Id)).ToList();
                var removedVariationCombination = rewardsDBContext.VariationCombination.Where(x => removedVariationOptionsOldIds.Contains(x.OptionTwoId.Value)).ToList();
                var toUpdateCartProducts = rewardsDBContext.CartProducts.Where(x => removedProductVariationOldIds.Contains(x.VariationId.Value)).ToList();
                rewardsDBContext.Variations.RemoveRange(removedOldVariations);
                rewardsDBContext.VariationOptions.RemoveRange(removedOldVariationsOptions);
                rewardsDBContext.VariationCombination.RemoveRange(removedVariationCombination);
                foreach(var cartProduct in toUpdateCartProducts)
                {
                    cartProduct.VariationId = null;
                    cartProduct.CartProductType = 3;
                    cartProduct.UpdatedAt = request.CreatedAt;
                }
                rewardsDBContext.CartProducts.UpdateRange(toUpdateCartProducts);
                rewardsDBContext.ProductVariation.RemoveRange(removedOldProductVariations);
                rewardsDBContext.SaveChanges();
                var product = await rewardsDBContext.Products.Where(x => x.Id == request.ProductId).FirstOrDefaultAsync();
                if (product != null)
                {
                    if (request.IsCallFromMerchant)
                    {
                        var jsonString = "";
                        if (String.IsNullOrEmpty(product.PendingChanges))
                        {
                            //jsonString = JsonConvert.SerializeObject(product);
                        }
                        else
                        {
                            jsonString = product.PendingChanges;
                            var newItem = JsonConvert.DeserializeObject<Voupon.Database.Postgres.RewardsEntities.Products>(jsonString);

                            newItem.Price = request.ProductVariationDetailsList.Where(x => x.Price > 0).Min(x => x.Price);
                            if (request.ProductVariationDetailsList.Where(x => x.DiscountedPrice > 0).Count() > 0)
                                newItem.DiscountedPrice = request.ProductVariationDetailsList.Where(x => x.DiscountedPrice > 0).Min(x => x.DiscountedPrice);
                            else
                            {
                                newItem.DiscountedPrice = newItem.Price;
                            }

                            if (newItem.DiscountedPrice < newItem.Price && newItem.DiscountedPrice != 0)
                            {
                                newItem.IsDiscountedPriceEnabled = true;
                            }
                            if (request.ProductVariationDetailsList.Count() > 0)
                            {
                                newItem.IsProductVariationEnabled = true;
                            }

                            product.PendingChanges = JsonConvert.SerializeObject(newItem);
                        }
                    }
                    else
                    {
                        product.Price = request.ProductVariationDetailsList.Where(x => x.Price > 0).Min(x => x.Price);
                        if (request.ProductVariationDetailsList.Where(x => x.DiscountedPrice > 0).Count() > 0)
                            product.DiscountedPrice = request.ProductVariationDetailsList.Where(x => x.DiscountedPrice > 0).Min(x => x.DiscountedPrice);
                        else
                        {
                            product.DiscountedPrice = product.Price;
                        }

                        if (product.DiscountedPrice < product.Price && product.DiscountedPrice != 0)
                        {
                            product.IsDiscountedPriceEnabled = true;
                        }
                        if (request.ProductVariationDetailsList.Count() > 0)
                        {
                            product.IsProductVariationEnabled = true;
                        }
                    }
                }
                rewardsDBContext.SaveChanges();

                foreach(var variation in request.VariationList)
                {
                    if (!variationOldIds.Contains(variation.Id))
                    {
                        Variations newVariation = new Variations();
                        newVariation.IsFirstVariation = variation.IsFirstVariation;
                        newVariation.Name = variation.Name;
                        newVariation.ProductId = variation.ProductId;
                        newVariation.CreatedAt = request.CreatedAt;
                        newVariation.CreatedByUserId = request.CreatedByUserId;

                        foreach (var variationOption in variation.VariationOptions)
                        {
                            Voupon.Database.Postgres.RewardsEntities.VariationOptions newVariationOption = new Voupon.Database.Postgres.RewardsEntities.VariationOptions();
                            newVariationOption.Name = variationOption.Name;
                            newVariationOption.Order = variationOption.Order;
                            newVariationOption.CreatedAt = request.CreatedAt;
                            newVariationOption.CreatedBy = request.CreatedByUserId;
                            if (variation.IsFirstVariation)
                            {
                                newVariationOption.ImageUrl = request.ImagesList[variationOption.Order];
                            }
                            newVariation.VariationOptions.Add(newVariationOption);
                        }
                        var variationToDelete = rewardsDBContext.Variations.Where(x=> x.ProductId == request.ProductId && x.IsFirstVariation == variation.IsFirstVariation).FirstOrDefault();
                        if(variationToDelete != null)
                        rewardsDBContext.Variations.Remove(variationToDelete);
                        rewardsDBContext.Variations.Add(newVariation);
                    }
                    else
                    {
                        var oldVariationToUpdate = oldVariations.Where(x => x.Id == variation.Id).FirstOrDefault();
                        oldVariationToUpdate.IsFirstVariation = variation.IsFirstVariation;
                        oldVariationToUpdate.Name = variation.Name;
                        oldVariationToUpdate.ProductId = variation.ProductId;
                        oldVariationToUpdate.LastUpdatedAt = request.CreatedAt;
                        oldVariationToUpdate.LastUpdateByUserId = request.CreatedByUserId;
                        foreach (var variationOption in variation.VariationOptions)
                        {
                            if (!variationOptionsOldIds.Contains(variationOption.Id))
                            {
                                Voupon.Database.Postgres.RewardsEntities.VariationOptions newVariationOption = new Voupon.Database.Postgres.RewardsEntities.VariationOptions();
                                newVariationOption.Name = variationOption.Name;
                                newVariationOption.Order = variationOption.Order;
                                newVariationOption.CreatedAt = request.CreatedAt;
                                newVariationOption.CreatedBy = request.CreatedByUserId;
                                if (variation.IsFirstVariation)
                                {
                                    newVariationOption.ImageUrl = request.ImagesList[variationOption.Order];
                                }
                                oldVariationToUpdate.VariationOptions.Add(newVariationOption);
                            }
                            else
                            {
                                var oldVariationOptions = oldVariations.Where(x => x.Id == variation.Id).FirstOrDefault().VariationOptions.Where(y=> y.Id == variationOption.Id).FirstOrDefault();
                                oldVariationOptions.Name = variationOption.Name;
                                oldVariationOptions.Order = variationOption.Order;
                                oldVariationOptions.LastUpdatedAt = request.CreatedAt;
                                oldVariationOptions.LastUpdateByUserId = request.CreatedByUserId;
                                if (variation.IsFirstVariation)
                                {
                                    oldVariationOptions.ImageUrl = request.ImagesList[variationOption.Order];
                                }
                                rewardsDBContext.VariationOptions.Update(oldVariationOptions);

                            }
                        }
                        rewardsDBContext.Variations.Update(oldVariationToUpdate);
                    }
                }
                rewardsDBContext.SaveChanges();

                var newFirstAddedVariations = await rewardsDBContext.Variations.Include(x => x.VariationOptions).Where(x => x.ProductId == request.ProductId && x.IsFirstVariation).FirstOrDefaultAsync();
                var newSecondAddedVariations = await rewardsDBContext.Variations.Include(x => x.VariationOptions).Where(x => x.ProductId == request.ProductId && !x.IsFirstVariation).FirstOrDefaultAsync();
                foreach(var firstVariationOption in newFirstAddedVariations.VariationOptions)
                {
                    /*
                    if (newSecondAddedVariations != null && newSecondAddedVariations.VariationOptions.Count > 0)
                    {
                        foreach (var secondVariationOption in newSecondAddedVariations.VariationOptions)
                        {
                            if (!variationOptionsOldIds.Contains(firstVariationOption.Id) || !variationOptionsOldIds.Contains(secondVariationOption.Id))
                            {
                                VariationCombination variationCombination = new VariationCombination()
                                {
                                    OptionOneId = firstVariationOption.Id,
                                    OptionTwoId = secondVariationOption.Id,
                                    CreatedAt = request.CreatedAt,
                                    CreatedByUserId = request.CreatedByUserId,
                                    ProductVariation = new ProductVariation()
                                };
                                var productVariationDetails = request.ProductVariationDetailsList.Where(x => Int16.Parse(x.Order.Split(',').ToList()[0]) == firstVariationOption.Order && Int16.Parse(x.Order.Split(',').ToList()[1]) == secondVariationOption.Order).FirstOrDefault();

                                if (productVariationDetails != null)
                                {
                                    variationCombination.ProductVariation.AvailableQuantity = productVariationDetails.AvailableQuantity;
                                    variationCombination.ProductVariation.Price = productVariationDetails.Price;
                                    variationCombination.ProductVariation.IsDiscountedPriceEnabled = productVariationDetails.IsDiscountedPriceEnabled;
                                    if (productVariationDetails.IsDiscountedPriceEnabled)
                                        variationCombination.ProductVariation.DiscountedPrice = productVariationDetails.DiscountedPrice;
                                    else
                                        variationCombination.ProductVariation.DiscountedPrice = productVariationDetails.Price;
                                    variationCombination.ProductVariation.SKU = productVariationDetails.Sku;
                                    variationCombination.ProductVariation.ProductId = request.ProductId;
                                    variationCombination.ProductVariation.CreatedAt = request.CreatedAt;
                                    variationCombination.ProductVariation.CreatedByUserId = request.CreatedByUserId;
                                }
                                rewardsDBContext.VariationCombination.Add(variationCombination);
                            }
                            else
                            {
                                VariationCombination variationCombination = await rewardsDBContext.VariationCombination.Where(x => x.OptionOneId == firstVariationOption.Id && x.OptionTwoId == secondVariationOption.Id).FirstOrDefaultAsync();
                                var productVariationDetails = request.ProductVariationDetailsList.Where(x => Int16.Parse(x.Order.Split(',').ToList()[0]) == firstVariationOption.Order && Int16.Parse(x.Order.Split(',').ToList()[1]) == secondVariationOption.Order).FirstOrDefault();

                                if (productVariationDetails != null)
                                {
                                    variationCombination.ProductVariation.AvailableQuantity = productVariationDetails.AvailableQuantity;
                                    variationCombination.ProductVariation.Price = productVariationDetails.Price;
                                    variationCombination.ProductVariation.IsDiscountedPriceEnabled = productVariationDetails.IsDiscountedPriceEnabled;
                                    if (productVariationDetails.IsDiscountedPriceEnabled)
                                        variationCombination.ProductVariation.DiscountedPrice = productVariationDetails.DiscountedPrice;
                                    else
                                        variationCombination.ProductVariation.DiscountedPrice = productVariationDetails.Price;
                                    variationCombination.ProductVariation.SKU = productVariationDetails.Sku;
                                    variationCombination.ProductVariation.ProductId = request.ProductId;
                                    variationCombination.ProductVariation.LastUpdatedAt = request.CreatedAt;
                                    variationCombination.ProductVariation.LastUpdateByUserId = request.CreatedByUserId;
                                }
                                rewardsDBContext.VariationCombination.Update(variationCombination);

                            }
                        }
                    }
                    else
                    {
                        var VariationCombinationList = await rewardsDBContext.VariationCombination.Where(x => x.OptionOneId == firstVariationOption.Id && x.OptionTwoId == null).FirstOrDefaultAsync();
                        if (!variationOptionsOldIds.Contains(firstVariationOption.Id) ||( variationOptionsOldIds.Contains(firstVariationOption.Id) && VariationCombinationList == null))
                        {
                            VariationCombination variationCombination = new VariationCombination()
                            {
                                OptionOneId = firstVariationOption.Id,
                                OptionTwoId = null,
                                CreatedAt = request.CreatedAt,
                                CreatedByUserId = request.CreatedByUserId,
                                ProductVariation = new ProductVariation()
                            };
                            var productVariationDetails = request.ProductVariationDetailsList.Where(x => Int16.Parse(x.Order.Split(',').ToList()[0]) == firstVariationOption.Order).FirstOrDefault();

                            if (productVariationDetails != null)
                            {
                                variationCombination.ProductVariation.AvailableQuantity = productVariationDetails.AvailableQuantity;
                                variationCombination.ProductVariation.Price = productVariationDetails.Price;
                                variationCombination.ProductVariation.IsDiscountedPriceEnabled = productVariationDetails.IsDiscountedPriceEnabled;
                                if (productVariationDetails.IsDiscountedPriceEnabled)
                                    variationCombination.ProductVariation.DiscountedPrice = productVariationDetails.DiscountedPrice;
                                else
                                    variationCombination.ProductVariation.DiscountedPrice = 0;
                                variationCombination.ProductVariation.SKU = productVariationDetails.Sku;
                                variationCombination.ProductVariation.ProductId = request.ProductId;
                                variationCombination.ProductVariation.CreatedAt = request.CreatedAt;
                                variationCombination.ProductVariation.CreatedByUserId = request.CreatedByUserId;
                            }
                            rewardsDBContext.VariationCombination.Add(variationCombination);
                        }
                        else
                        {
                            VariationCombination variationCombination = await rewardsDBContext.VariationCombination.Where(x => x.OptionOneId == firstVariationOption.Id && x.OptionTwoId == null).FirstOrDefaultAsync();
                            var productVariationDetails = request.ProductVariationDetailsList.Where(x => Int16.Parse(x.Order.Split(',').ToList()[0]) == firstVariationOption.Order).FirstOrDefault();


                            if (productVariationDetails != null)
                            {
                                variationCombination.ProductVariation.AvailableQuantity = productVariationDetails.AvailableQuantity;
                                variationCombination.ProductVariation.Price = productVariationDetails.Price;
                                variationCombination.ProductVariation.IsDiscountedPriceEnabled = productVariationDetails.IsDiscountedPriceEnabled;
                                if (productVariationDetails.IsDiscountedPriceEnabled)
                                    variationCombination.ProductVariation.DiscountedPrice = productVariationDetails.DiscountedPrice;
                                else
                                    variationCombination.ProductVariation.DiscountedPrice = productVariationDetails.Price;
                                variationCombination.ProductVariation.SKU = productVariationDetails.Sku;
                                variationCombination.ProductVariation.ProductId = request.ProductId;
                                variationCombination.ProductVariation.LastUpdatedAt = request.CreatedAt;
                                variationCombination.ProductVariation.LastUpdateByUserId = request.CreatedByUserId;
                            }
                            rewardsDBContext.VariationCombination.Update(variationCombination);

                        }
                    }
                    rewardsDBContext.SaveChanges();
                    */
                }
            
                response.Successful = true;
                response.Message = "Update Product variations Successfully";
                //response.Data = item;
            }
            catch (Exception ex)
            {
                var errorLogs = new ErrorLogs
                {
                    ActionName = "CreateProductVariationCommand",
                    ActionRequest = JsonConvert.SerializeObject(request),
                    CreatedAt = DateTime.Now,
                    Errors = ex.ToString(),
                    Email = request.UserEmail,
                    TypeId = CreateErrorLogCommand.Type.Service
                };

                rewardsDBContext.ErrorLogs.Add(errorLogs);
                await rewardsDBContext.SaveChangesAsync();

                response.Message = "Fail to Update Variation Info";
            }

            return response;
        }
    }
}
