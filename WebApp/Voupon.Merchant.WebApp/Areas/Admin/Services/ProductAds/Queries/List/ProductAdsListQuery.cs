using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Voupon.Merchant.WebApp.ViewModels;
using Voupon.Database.Postgres.VodusEntities;
using Voupon.Database.Postgres.RewardsEntities;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using Voupon.Merchant.WebApp.Areas.Admin.ViewModels.ProductAds;
using Voupon.Merchant.WebApp.Common.Services.ProductDemographicsTarget.Models;

namespace Voupon.Merchant.WebApp.Areas.Admin.Services.ProductAds.Queries
{
    public class ProductAdsListQuery : IRequest<ApiResponseViewModel>
    {
        public int AdImpressionCountType { get; set; }  //  1 (productlist): >= , 2 (new product ads): <
        public class ProductAdsListQueryHandler : IRequestHandler<ProductAdsListQuery, ApiResponseViewModel>
        {
            VodusV2Context vodusV2Context;
            RewardsDBContext rewardsDBContext;
            public ProductAdsListQueryHandler(VodusV2Context vodusV2Context, RewardsDBContext rewardsDBContext)
            {
                this.vodusV2Context = vodusV2Context;
                this.rewardsDBContext = rewardsDBContext;
            }
            public async Task<ApiResponseViewModel> Handle(ProductAdsListQuery request, CancellationToken cancellationToken)
            {
                var apiResponseViewModel = new ApiResponseViewModel();
                try
                {
                    List<int> selectedDemographicsTargets = new List<int>() { 1, 3, 8 };
                    var demographics = await vodusV2Context.DemographicTypes.Select(y => new { y.Id, y.Name, DemographicValues = y.DemographicValues.Where(z => z.IsActive == true) }).Where(x => selectedDemographicsTargets.Contains(x.Id)).ToListAsync();
                    var productAdsConfig = await vodusV2Context.ProductAdsConfig.FirstOrDefaultAsync();

                    if(productAdsConfig == null)
                    {
                        apiResponseViewModel.Successful = false;
                        return apiResponseViewModel;
                    }
                    var productAds = new List<Voupon.Database.Postgres.VodusEntities.ProductAds>();
                    var rewardsProducts = await rewardsDBContext.Products.Include(x => x.Merchant).Where(x => x.Merchant.IsTestAccount == false).ToListAsync();
                    if(request.AdImpressionCountType == 1)
                    {
                        productAds = await vodusV2Context.ProductAds.Include(x => x.ProductAdSubgroups).Include(x => x.ProductAdLocations).Where(x => x.AdImpressionCount >= productAdsConfig.ImpressionCountIdentifier).ToListAsync();
                    }

                    else if (request.AdImpressionCountType == 2)
                    {
                        productAds = await vodusV2Context.ProductAds.Include(x => x.ProductAdSubgroups).Include(x => x.ProductAdLocations).Where(x => x.AdImpressionCount < productAdsConfig.ImpressionCountIdentifier).ToListAsync();
                    }
                    else
                    {
                        apiResponseViewModel.Successful = false;
                        return apiResponseViewModel;
                    }

                    var productAdsList = new List<ProductAdsList>();

                    foreach (var item in productAds)
                    {
                        var product = rewardsProducts.Where(x => x.Id == item.ProductId).FirstOrDefault();

                        if (product == null)
                        {
                            continue;
                        }
                        var productDemographicTargets = await rewardsDBContext.ProductDemographicsTarget.Where(x => x.ProductId == item.ProductId && selectedDemographicsTargets.Contains(x.DemographicTypeId)).ToListAsync();
                        var productAd = new ProductAdsList
                        {
                            Id = item.Id,
                            ProductId = item.ProductId,
                            AdImpressionCount = item.AdImpressionCount,
                            AdClickCount = item.AdClickCount,
                            CreatedAt = item.CreatedAt,
                            CTR = (item.CTR.HasValue ? item.CTR.Value : 0),
                            IsActive = item.IsActive,
                            ProductAdLocations = new List<string>(),
                            ProductDemographicTargets = new List<Common.Services.ProductDemographicsTarget.Models.ProductDemographicTargets>(),
                            ProductTargetsText = ""
                        };

                        foreach (var location in item.ProductAdLocations.Where(x => x.IsActive == true))
                        {
                            productAd.ProductAdLocations.Add(location.ProvinceName);
                        }
                        int counter = 0;
                        int skipDemo = 0;
                        foreach (var productDemographicTarget in productDemographicTargets)
                        {
                            ProductDemographicTargets productDemographicTargetsModel = new ProductDemographicTargets();
                            productDemographicTargetsModel.Id = productDemographicTarget.Id;
                            productDemographicTargetsModel.DemographicTypeId = productDemographicTarget.DemographicTypeId;
                            productDemographicTargetsModel.DemographicValue = productDemographicTarget.DemographicValue;
                            productAd.ProductDemographicTargets.Add(productDemographicTargetsModel);
                            if (skipDemo == productDemographicTarget.DemographicTypeId)
                            {
                                continue;
                            }
                            if (productDemographicTargets.Where(x=> x.DemographicTypeId == productDemographicTarget.DemographicTypeId).Count() == demographics.Where(x=> x.Id == productDemographicTarget.DemographicTypeId).Select(x => x.DemographicValues.Count()).ToArray().Sum())
                            {
                                skipDemo = productDemographicTarget.DemographicTypeId;
                                if (counter != 0)
                                {
                                    productAd.ProductTargetsText += ", ";
                                }
                                if (productDemographicTarget.DemographicTypeId == 1)
                                    productAd.ProductTargetsText += "All Ages";
                                else if(productDemographicTarget.DemographicTypeId == 3)
                                    productAd.ProductTargetsText += "Both Genders";
                                else if (productDemographicTarget.DemographicTypeId == 8)
                                    productAd.ProductTargetsText += "All Races";

                                counter += productDemographicTargets.Where(x => x.DemographicTypeId == productDemographicTarget.DemographicTypeId).Count();
                                continue;
                            }
                            
                            if (counter != 0)
                            {
                                productAd.ProductTargetsText += ", ";
                            }
                            productAd.ProductTargetsText += demographics.Where(x => x.Id == productDemographicTarget.DemographicTypeId).FirstOrDefault().DemographicValues.Where(x => x.Id == productDemographicTarget.DemographicValue).FirstOrDefault().DisplayValue;
                            counter++;
                           
                        }
                        if (productDemographicTargets.Count() == demographics.Select(x => x.DemographicValues.Count()).ToArray().Sum())
                        {
                            productAd.ProductTargetsText = "";
                        }
                        productAd.ActualPriceForVpoints = product.ActualPriceForVpoints;
                        productAd.MerchantId = product.MerchantId;
                        if (product.Merchant != null)
                        {
                            productAd.MerchantName = product.Merchant.DisplayName;
                        }
                        productAd.PointsRequired = product.PointsRequired;
                        productAd.Price = product.Price;
                        productAd.Title = product.Title;
                        productAdsList.Add(productAd);
                    }

                    apiResponseViewModel.Successful = true;
                    apiResponseViewModel.Message = "Get Product Ads List Successfully";
                    apiResponseViewModel.Data = productAdsList;
                }
                catch (Exception ex)
                {
                    apiResponseViewModel.Message = ex.Message;
                }
                return apiResponseViewModel;
            }
        }
    }

}
