using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Voupon.Common.Azure.Blob;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Database.Postgres.VodusEntities;
using Voupon.API.ViewModels;

namespace Voupon.API.Functions
{
    public class GetHomepageBannersFunction
    {
        private readonly RewardsDBContext rewardsDBContext;
        private readonly VodusV2Context vodusV2Context;
        private readonly IConnectionMultiplexer connectionMultiplexer;
        private readonly IAzureBlobStorage azureBlobStorage;
        const string REDIS_FEATURED_PRODUCTS = "FEATURED_PRODUCTS";

        public GetHomepageBannersFunction(RewardsDBContext rewardsDBContext, VodusV2Context vodusV2Context, IConnectionMultiplexer connectionMultiplexer, IAzureBlobStorage azureBlobStorage)
        {
            this.rewardsDBContext = rewardsDBContext;
            this.vodusV2Context = vodusV2Context;
            this.connectionMultiplexer = connectionMultiplexer;
            this.azureBlobStorage = azureBlobStorage;
        }

        [OpenApiOperation(operationId: "Get homepage banners", tags: new[] { "Banners" }, Description = "Get homepage banners", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(HomepageBannerResponseModel), Summary = "The list of banners")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.BadRequest, Summary = "if unable to retrieve it from the server")]

        [FunctionName("GetHomepageBannersFunction")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "banners")] HttpRequest req, ILogger log)
        {
            var apiResponseViewModel = new HomepageBannerResponseModel
            {
                Data = new BannerData()
            };

            try
            {
                var baseURL = "https://vodus.my";
                //  TODO get data from cms
                var bannerList = new List<Banners>();
                var banners = new Banners
                {
                    Id = 1,
                    Title = "Vodus Promo",
                    BannerImageUrl = $"{baseURL}/images/banners/vodus-promo-banner-1.webp",
                    Description = @"<ul>
                                        <li>Get RM20 off your first purchase</li>
                                    </ul>"
                };
                bannerList.Add(banners);

                banners = new Banners
                {
                    Id = 2,
                    Title = "Win RM50 voucher when you purchase between 1st May - 20th May 2022 - Discount code to be used for next purchase",
                    BannerImageUrl = $"{baseURL}/images/banners/vodus-promo-banner-1.webp",
                    Description = @"<ul>
                                    <li>Valid for transactions on www.vodus.my between 1st May - 20th May 2022</li>
                                    <li>6 winners will be chosen at random and contacted via email within 7 days of the promotion end date.</li>
                                    <li>To participate in this promotion participants must have a registered account with www.vodus.my and have a spend of at least one transaction during the promotion period.</li>
                                    <li>The winner’s discount code is valid for 2 weeks from the voucher date.</li>
                                    <li>Discount code cannot be used in conjunction with any other vouchers/discount codes</li>
                                    <li>Discount code can be used in conjunction with VPoints discount</li>
                                    <li>Vodus Sdn.Bhd reserves the right to vary the Terms &amp; Conditions without prior notice at its own discretion.</li>
                                </ul>"
                };
                bannerList.Add(banners);


                banners = new Banners
                {
                    Id = 3,
                    Title = "Double VPoints",
                    BannerImageUrl = $"{baseURL}/images/banners/dvd-promo.webp",
                    Description = @"<ul>
                                        <li>VPoints Discount promo amount is capped at 25%</li>
                                        <li>Only applicable for purchase with pre-promo VPoints discount that is less than 25%</li>
                                        <li>Can't be used in conjunction with other vouchers/ discount codes</li>
                                    </ul>"
                };
                bannerList.Add(banners);

                apiResponseViewModel.Data.Banners = bannerList;
                return new OkObjectResult(apiResponseViewModel);
            }
            catch (Exception ex)
            {
                //  log
                return new BadRequestObjectResult(new ApiResponseViewModel
                {
                    ErrorMessage = "Fail to get banners"
                });
            }
        }

        private class HomepageBannerResponseModel : ApiResponseViewModel
        {
            public BannerData Data { get; set; }
        }

        private class BannerData
        {
            public List<Banners> Banners { get; set; }
        }

        private class Banners
        {
            public int Id { get; set; }
            public string BannerImageUrl { get; set; }
            public string Title { get; set; }
            public string Description { get; set; }
        }
    }
}
