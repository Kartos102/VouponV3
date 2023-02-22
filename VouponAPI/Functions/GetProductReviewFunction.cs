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
using Voupon.API.Util;

namespace Voupon.API.Functions
{
    public class GetProductReviewFunction
    {
        private readonly RewardsDBContext rewardsDBContext;
        private readonly VodusV2Context vodusV2Context;
        private readonly IConnectionMultiplexer connectionMultiplexer;
        private readonly IAzureBlobStorage azureBlobStorage;

        public GetProductReviewFunction(RewardsDBContext rewardsDBContext, VodusV2Context vodusV2Context, IConnectionMultiplexer connectionMultiplexer, IAzureBlobStorage azureBlobStorage)
        {
            this.rewardsDBContext = rewardsDBContext;
            this.vodusV2Context = vodusV2Context;
            this.connectionMultiplexer = connectionMultiplexer;
            this.azureBlobStorage = azureBlobStorage;
        }

        [OpenApiOperation(operationId: "Get products reviews", tags: new[] { "Products" }, Description = "Get products reviews", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "id", In = ParameterLocation.Query, Required = false, Type = typeof(int), Description = "Product id", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "i", In = ParameterLocation.Query, Required = false, Type = typeof(string), Description = "External item id", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "s", In = ParameterLocation.Query, Required = false, Type = typeof(string), Description = "External shop id", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "t", In = ParameterLocation.Query, Required = false, Type = typeof(string), Description = "External type id", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "type", In = ParameterLocation.Query, Required = false, Type = typeof(byte), Description = "Rating type (0 = all items,1 = 1 star rating, same goes for 2-5", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "limit", In = ParameterLocation.Query, Required = false, Type = typeof(int), Description = "Limit of items to show", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "offset", In = ParameterLocation.Query, Required = false, Type = typeof(int), Description = "Offset if items", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(ProductReviewResponseModel), Summary = "The paginated result of reviews")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.BadRequest, Summary = "If no id or external id is supplied")]


        [FunctionName("GetProductReviewFunction")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "products/reviews")] HttpRequest req, ILogger log)
        {
            var response = new ProductReviewResponseModel
            {
                Data = new ReviewData()
            };

            try
            {
                var productId = req.Query["id"];
                var externalItemId = req.Query["i"];
                var externalShopId = req.Query["s"];
                var externalTypeId = req.Query["t"];
                var tempType = req.Query["type"];

                var tempLimit = req.Query["limit"];
                var tempOffset = req.Query["offset"];

                var limit = 0;
                int.TryParse(tempLimit, out limit);

                var offset = 0;
                int.TryParse(tempOffset, out offset);

                byte type = 0;
                byte.TryParse(tempType, out type);

                int id = 0;
                bool success = int.TryParse(productId, out id);
                if (id > 0)
                {
                    response.Data = await GetInternalProductReview(id);
                }
                else
                {
                    if (string.IsNullOrEmpty(externalItemId) || string.IsNullOrEmpty(externalShopId) || string.IsNullOrEmpty(externalTypeId))
                    {
                        return new BadRequestObjectResult(new ApiResponseViewModel
                        {
                            Code = -1,
                            ErrorMessage = "Fail to get data [001]"
                        });
                    }
                    byte typeId = 0;
                    if (!byte.TryParse(externalTypeId, out typeId))
                    {
                        return new BadRequestObjectResult(new ApiResponseViewModel
                        {
                            Code = -1,
                            ErrorMessage = "Fail to get data [002]"
                        });
                    };
                    response.Data = await GetExternalProductReview(externalItemId, externalShopId, typeId, type, limit, offset);
                }
                return new OkObjectResult(response);
            }
            catch (Exception ex)
            {
                //  log
                return new BadRequestObjectResult(new ApiResponseViewModel
                {
                    Code = -1,
                    ErrorMessage = "Fail to get data [099]"
                });

            }
        }

        private async Task<ReviewData> GetExternalProductReview(string itemId, string shopId, byte typeId, byte type, int limit, int offset)
        {
            var _aggregatorUrl = "";
            var aggregatorUrl = await vodusV2Context.AggregatorApiUrls.AsNoTracking().ToListAsync();
            if (bool.Parse(Environment.GetEnvironmentVariable(EnvironmentKey.USE_LOCAL_AGGREGATOR)) == true)
            {
                _aggregatorUrl = aggregatorUrl.Where(x => x.IsLocalAggregator == true && x.Status == 1).FirstOrDefault().Url;
            }
            else
            {
                _aggregatorUrl = aggregatorUrl.Where(x => x.IsLocalAggregator == false && x.Status == 1).OrderBy(x => x.LastUpdatedAt).First().Url;
            }

            StringContent httpContent = new StringContent(JsonConvert.SerializeObject(new AggRequest()
            {
                ExternalItemId = itemId,
                ExternalShopId = shopId,
                ExternalTypeId = typeId,
                Type = type

            }), System.Text.Encoding.UTF8, "application/json");
            var httpClient = new HttpClient();
            var result = await httpClient.GetAsync($"{_aggregatorUrl}/v1/product/reviews?externalItemId={itemId}&externalShopId={shopId}&externalTypeId={typeId}&type={type}&limit={limit}&offset={offset}");
            var resultString = await result.Content.ReadAsStringAsync();
            var crawlerResult = JsonConvert.DeserializeObject<LegacyApiResponseViewModel>(resultString);
            if (!crawlerResult.Successful)
            {
                return null;
            }

            var reviewData = JsonConvert.DeserializeObject<ReviewData>(crawlerResult.Data.ToString());
            if (!string.IsNullOrEmpty(reviewData.NextPageUrl))
            {
                reviewData.NextPageUrl = Environment.GetEnvironmentVariable(EnvironmentKey.BASE_URL) + "/v1/products/reviews" + reviewData.NextPageUrl;
            }
            return reviewData;
        }

        private async Task<ReviewData> GetInternalProductReview(int id)
        {
            var reviews = await rewardsDBContext.ProductReview.Where(x => x.ProductId == id).OrderByDescending(x => x.CreatedAt).ToListAsync();

            return new ReviewData
            {
                OneStarRatingCount = 0,
                TwoStarRatingCount = 0,
                ThreeStarRatingCount = 0,
                FourStarRatingCount = 0,
                FiveStarRatingCount = 0,
                TotalReviewCount = reviews.Count(),
                AverageRating = 5,//(double)(reviews.Sum(x => x.Rating) / reviews.Count()),
                NextPageUrl = "",
                Items = reviews.Select(x => new Reviews
                {
                    Comment = x.Comment,
                    CreatedAt = x.CreatedAt,
                    MemberName = x.MemberName,
                    Rating = x.Rating,
                    ProductTitle = x.ProductTitle
                }).ToList()
            };
        }
    }


    public class ReviewData
    {
        public int OneStarRatingCount { get; set; }
        public int TwoStarRatingCount { get; set; }
        public int ThreeStarRatingCount { get; set; }
        public int FourStarRatingCount { get; set; }
        public int FiveStarRatingCount { get; set; }
        public double AverageRating { get; set; }
        public int TotalReviewCount { get; set; }
        public string NextPageUrl { get; set; }
        public List<Reviews> Items { get; set; }
    }

    public partial class Reviews
    {
        public int ProductId { get; set; }
        public decimal Rating { get; set; }
        public string Comment { get; set; }
        public string MemberName { get; set; }
        public string ProductTitle { get; set; }
        public DateTime CreatedAt { get; set; }
        public string VariationText { get; set; }
        public List<string> ImageList { get; set; }
        public List<Video> VideoList { get; set; }
        public RatingReply RatingReply { get; set; }
    }

    public class RatingReply
    {
        public string Comment { get; set; }
    }


    public partial class Video
    {
        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public string Id { get; set; }

        [JsonProperty("cover", NullValueHandling = NullValueHandling.Ignore)]
        public Uri Cover { get; set; }

        [JsonProperty("url", NullValueHandling = NullValueHandling.Ignore)]
        public Uri Url { get; set; }

        [JsonProperty("duration", NullValueHandling = NullValueHandling.Ignore)]
        public long? Duration { get; set; }

        [JsonProperty("upload_time")]
        public object UploadTime { get; set; }
    }

    public class AggRequest
    {
        public string ExternalItemId { get; set; }
        public string ExternalShopId { get; set; }
        public short ExternalTypeId { get; set; }
        public short Type { get; set; }
    }

    public class AggregatorSearchByKeywordQuery
    {
        public string SearchQuery { get; set; }
        public List<int> PriceFilter { get; set; }
        public List<int> LocationFilter { get; set; }

        public int PageNumber { get; set; }
    }

    public class ProductReviewResponseModel : ApiResponseViewModel
    {
        public ReviewData Data { get; set; }
    }

}
