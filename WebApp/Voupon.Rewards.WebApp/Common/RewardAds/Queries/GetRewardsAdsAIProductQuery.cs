using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Common.Azure.Blob;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Database.Postgres.VodusEntities;
using Voupon.Rewards.WebApp.ViewModels;

namespace Voupon.Rewards.WebApp.Common.Redis.Queries
{
    public class RewardsAdsAIProduct
    {
        public int PointRequired { get; set; }
        public string ImageUrl { get; set; }
        public string ProductUrl { get; set; }
        public string ProductTitle { get; set; }
        public string MerchantName { get; set; }
        public string DiscountedPrice { get; set; }
        public string Price { get; set; }
        public string TotalDiscount { get; set; }
    }
    public class GetRewardsAdsAIProductQuery : IRequest<ApiResponseViewModel>
    {
        public int ProvinceId { get; set; }
    }

    public class GetRewardsAdsAIProductQueryHandler : IRequestHandler<GetRewardsAdsAIProductQuery, ApiResponseViewModel>
    {
        RewardsDBContext rewardsDBContext;
        VodusV2Context vodusV2Context;
        IConnectionMultiplexer connectionMultiplexer;
        private readonly IAzureBlobStorage azureBlobStorage;
        private IOptions<AppSettings> appSettings;
        public GetRewardsAdsAIProductQueryHandler(RewardsDBContext rewardsDBContext, IConnectionMultiplexer connectionMultiplexer, IAzureBlobStorage azureBlobStorage, IOptions<AppSettings> appSettings, VodusV2Context vodusV2Context)
        {
            this.rewardsDBContext = rewardsDBContext;
            this.vodusV2Context = vodusV2Context;
            this.connectionMultiplexer = connectionMultiplexer;
            this.azureBlobStorage = azureBlobStorage;
            this.appSettings = appSettings;
        }


        public async Task<ApiResponseViewModel> Handle(GetRewardsAdsAIProductQuery request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();
            List<RewardsAdsAIProduct> productlist = new List<RewardsAdsAIProduct>();
            try
            {
                int VPointCap = Int16.Parse(appSettings.Value.RewardAds.VPointCap);

              
                int ImpressionCountIdentifier = 1000;
                
                var result = await vodusV2Context.ProductAdsConfig.FirstOrDefaultAsync();
                if (result != null)
                {
                    ImpressionCountIdentifier = result.ImpressionCountIdentifier;
                }
                var newProductAds = await vodusV2Context.ProductAds.Include(x => x.ProductAdLocations).Where(x => x.AdImpressionCount < ImpressionCountIdentifier && x.IsActive == true ).ToListAsync();

                for (int i = 0; i < newProductAds.Count; i++)
                {

                    bool locationPass = false;
                    if (newProductAds[i].ProductAdLocations.Count > 0)
                    {
                        foreach (var ProductAdLocation in newProductAds[i].ProductAdLocations)
                        {
                            if (ProductAdLocation.ProvinceId == request.ProvinceId)
                            {
                                locationPass = true;
                            }
                        }
                    }
                    else
                    {
                        locationPass = true;
                    }
                    if (!locationPass)
                    {
                        newProductAds.RemoveAt(i);
                        i--;
                    }
                    else if (!PassDiscountVpointCap(newProductAds[i].ProductId, VPointCap))
                    {
                        newProductAds.RemoveAt(i);
                        i--;
                    }

                }

                if (newProductAds.Count > 12)
                {
                    response.Successful = true;
                    response.Message = "Get Reward Ad Successfully";

                    response.Data = newProductAds.OrderBy(x => Guid.NewGuid()).Take(12).ToList(); ;
                }
                else
                {
                    var productAds = vodusV2Context.ProductAds.Include(x => x.ProductAdLocations).Where(x => x.AdImpressionCount >= ImpressionCountIdentifier && x.IsActive == true).ToList();

                    for (int i = 0; i < productAds.Count; i++)
                    {

                        bool locationPass = false;
                        if (productAds[i].ProductAdLocations.Count > 0)
                        {
                            foreach (var ProductAdLocation in productAds[i].ProductAdLocations)
                            {
                                if (ProductAdLocation.ProvinceId == request.ProvinceId)
                                {
                                    locationPass = true;
                                }
                            }
                        }
                        else
                        {
                            locationPass = true;
                        }
                        if (!locationPass)
                        {
                            productAds.RemoveAt(i);
                            i--;
                        }
                        else if (!PassDiscountVpointCap(productAds[i].ProductId, VPointCap))
                        {
                            productAds.RemoveAt(i);
                            i--;
                        }

                    }
                    if(productAds.Count > 12)
                    {
                        response.Successful = true;
                        response.Message = "Get Reward Ad Successfully";

                        response.Data = productAds.OrderByDescending(x => x.CTR).Take(12).ToList(); ;
                    }
                }

                response.Successful = false;
                response.Message = "Fail to get from Ads AI";
            }
            catch (Exception ex)
            {
                response.Successful = false;

                response.Message = ex.Message;
            }

            return response;
        }
        private bool PassDiscountVpointCap(int productId, int VpointCap)
        {
            var discounts = rewardsDBContext.ProductDiscounts.Where(x => x.ProductId == productId).ToList();
            if (discounts != null && discounts.Count > 0 && (discounts.Where(x => x.PointRequired <= VpointCap).ToList().Count) > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
