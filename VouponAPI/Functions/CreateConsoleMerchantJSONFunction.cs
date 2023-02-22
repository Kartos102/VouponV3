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
using Newtonsoft.Json.Converters;
using System.Globalization;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Identity;

namespace Voupon.API.Functions.Blog
{
    public class CreateConsoleMerchantJSONFunction
    {
        private readonly RewardsDBContext _rewardsDBContext;
        private Microsoft.AspNetCore.Identity.UserManager<Voupon.Database.Postgres.VodusEntities.Users> _userManager;

        public CreateConsoleMerchantJSONFunction(IHttpClientFactory httpClientFactory, RewardsDBContext rewardsDBContext, Microsoft.AspNetCore.Identity.UserManager<Voupon.Database.Postgres.VodusEntities.Users> userManager)
        {
            _rewardsDBContext = rewardsDBContext;
            _userManager = userManager;
        }

        [FunctionName("CreateConsoleMerchantFunction")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "console/merchant-json")] HttpRequest req, ILogger log)
        {
            var response = new ApiResponseViewModel
            {
            };

            try
            {
                var request = HttpRequestHelper.DeserializeModel<MerchantRequestModel>(req);

                //  Update merchant last crawled date
                var merchantToCrawl = await _rewardsDBContext.ConsoleMerchantToCrawl.Where(x => x.Url == request.PageUrl).FirstOrDefaultAsync();
                if (merchantToCrawl != null)
                {
                    merchantToCrawl.LastCrawledAt = DateTime.Now;
                    _rewardsDBContext.ConsoleMerchantToCrawl.Update(merchantToCrawl);
                }

                var existingMerchantJSON = await _rewardsDBContext.ConsoleMerchantJSON.Where(x => x.URL == request.URL).FirstOrDefaultAsync();
                if (existingMerchantJSON != null)
                {
                    existingMerchantJSON.JSON = request.JSON;
                    existingMerchantJSON.LastUpdatedAt = DateTime.Now;
                    _rewardsDBContext.ConsoleMerchantJSON.Update(existingMerchantJSON);
                }
                else
                {
                    _rewardsDBContext.ConsoleMerchantJSON.Add(new ConsoleMerchantJSON
                    {
                        Id = Guid.NewGuid(),
                        ExternalId = request.ExternalId,
                        ExternalTypeId = request.ExternalTypeId,
                        MerchantUsername = request.MerchantName,
                        PageUrl = request.PageUrl,
                        StatusId = 2,
                        URL = request.URL,
                        JSON = request.JSON,
                        CreatedAt = DateTime.Now
                    });
                }

                await _rewardsDBContext.SaveChangesAsync();

                //  Create merchant
                var shopResult = JsonConvert.DeserializeObject<ShopeeShopDetailModel>(request.JSON);

                var existingMerchant = await _rewardsDBContext.Merchants.Where(x => x.ExternalId == shopResult.Data.Shopid.ToString() && x.IsExternal == true).FirstOrDefaultAsync();

                var createdByUserId = Guid.Parse(Environment.GetEnvironmentVariable("ADMIN_USER_ID"));


                if (existingMerchant == null)
                {
                    var merchant = new Voupon.Database.Postgres.RewardsEntities.Merchants
                    {
                        Description = shopResult.Data.Description,
                        DisplayName = shopResult.Data.Name,
                        CompanyName = shopResult.Data.Name,
                        Code = shopResult.Data.Account.Portrait,
                        CountryId = 1,
                        Rating = 5,
                        IsBrandShownInHomePage = false,
                        IsTestAccount = false,
                        TotalRatingCount = 0,
                        ExternalId = shopResult.Data.Shopid.ToString(),
                        ExternalTypeId = 1,
                        ExternalUrl = $"https://shopee.com.my/{shopResult.Data.Account.Username}",
                        CreatedAt = DateTime.Now,
                        CreatedByUserId = createdByUserId,
                        LogoUrl = $"https://cf.shopee.com.my/file/{shopResult.Data.Account.Portrait}_tn",
                        StatusTypeId = 1,
                        IsExternal = true,
                        IsPublished = true
                    };

                    await _rewardsDBContext.Merchants.AddAsync(merchant);
                    await _rewardsDBContext.SaveChangesAsync();


                    //  Create user
                    var id = Guid.NewGuid();
                    var email = $"{shopResult.Data.Account.Username}@vodus.my";
                    var merchantRole = await _rewardsDBContext.Roles.Where(x => x.NormalizedName == "MERCHANT").FirstOrDefaultAsync();
                    var tempUser = new Voupon.Database.Postgres.VodusEntities.Users
                    {
                        CreatedAt = DateTime.Now,
                        Email = email,
                        NormalizedEmail = email.ToUpper(),
                        EmailConfirmed = true,
                        UserName = email,
                        NormalizedUserName = email.ToUpper(),
                        Id = id.ToString(),
                        LockoutEnabled = false,
                        TwoFactorEnabled = false,
                        AccessFailedCount = 0
                    };

                    tempUser.PasswordHash = _userManager.PasswordHasher.HashPassword(tempUser, $"{shopResult.Data.Account.Username}!234*");

                    var user = new Voupon.Database.Postgres.RewardsEntities.Users
                    {
                        CreatedAt = DateTime.Now,
                        Email = email,
                        NormalizedEmail = email.ToUpper(),
                        EmailConfirmed = true,
                        UserName = email,
                        NormalizedUserName = email.ToUpper(),
                        Id = id,
                        PasswordHash = tempUser.PasswordHash,
                        LockoutEnabled = false,
                        TwoFactorEnabled = false,
                        AccessFailedCount = 0,
                        SecurityStamp = Guid.NewGuid().ToString()
                    };
                    await _rewardsDBContext.Users.AddAsync(user);
                    await _rewardsDBContext.SaveChangesAsync();

                    var userClaims = new Voupon.Database.Postgres.RewardsEntities.UserClaims
                    {
                        UserId = id,
                        ClaimType = "MerchantId",
                        ClaimValue = merchant.Id.ToString()
                    };

                    await _rewardsDBContext.UserClaims.AddAsync(userClaims);

                    var userRole = new Voupon.Database.Postgres.RewardsEntities.UserRoles
                    {
                        UserId = id,
                        RoleId = merchantRole.Id,
                        MerchantId = merchant.Id
                    };

                    await _rewardsDBContext.UserRoles.AddAsync(userRole);
                    await _rewardsDBContext.SaveChangesAsync();
                }
                else
                {
                    existingMerchant.Description = shopResult.Data.Description;
                    existingMerchant.CompanyName = shopResult.Data.Name;
                    existingMerchant.DisplayName = shopResult.Data.Name;
                    existingMerchant.LogoUrl = $"https://cf.shopee.com.my/file/{shopResult.Data.Account.Portrait}_tn";

                    _rewardsDBContext.Merchants.Update(existingMerchant);
                    await _rewardsDBContext.SaveChangesAsync();
                }

                response.Code = 0;

                return new OkObjectResult(response);
            }
            catch (Exception ex)
            {
                //  log
                response.ErrorMessage = "Fail to create merchant json" + ex.ToString();
                return new BadRequestObjectResult(response);

            }
        }

        private class MerchantRequestModel
        {
            public Guid Id { get; set; }
            public string URL { get; set; }
            public string PageUrl { get; set; }
            public string MerchantName { get; set; }
            public byte ExternalTypeId { get; set; }
            public string ExternalId { get; set; }
            public byte StatusId { get; set; }
            public string JSON { get; set; }
            public string Remark { get; set; }
            public DateTime CreatedAt { get; set; }

        }

        public partial class ShopeeShopDetailModel
        {
            [JsonProperty("data", NullValueHandling = NullValueHandling.Ignore)]
            public Data Data { get; set; }

            [JsonProperty("error", NullValueHandling = NullValueHandling.Ignore)]
            public long? Error { get; set; }

            [JsonProperty("error_msg", NullValueHandling = NullValueHandling.Ignore)]
            public string ErrorMsg { get; set; }

            [JsonProperty("version", NullValueHandling = NullValueHandling.Ignore)]
            public string Version { get; set; }
        }

        public partial class Data
        {
            [JsonProperty("place")]
            public object Place { get; set; }

            [JsonProperty("ctime", NullValueHandling = NullValueHandling.Ignore)]
            public long? Ctime { get; set; }

            [JsonProperty("mtime", NullValueHandling = NullValueHandling.Ignore)]
            public long? Mtime { get; set; }

            [JsonProperty("country", NullValueHandling = NullValueHandling.Ignore)]
            public string Country { get; set; }

            [JsonProperty("last_active_time", NullValueHandling = NullValueHandling.Ignore)]
            public long? LastActiveTime { get; set; }

            [JsonProperty("is_shopee_verified", NullValueHandling = NullValueHandling.Ignore)]
            public bool? IsShopeeVerified { get; set; }

            [JsonProperty("is_official_shop", NullValueHandling = NullValueHandling.Ignore)]
            public bool? IsOfficialShop { get; set; }

            [JsonProperty("chat_disabled", NullValueHandling = NullValueHandling.Ignore)]
            public bool? ChatDisabled { get; set; }

            [JsonProperty("disable_make_offer", NullValueHandling = NullValueHandling.Ignore)]
            public long? DisableMakeOffer { get; set; }

            [JsonProperty("enable_display_unitno", NullValueHandling = NullValueHandling.Ignore)]
            public bool? EnableDisplayUnitno { get; set; }

            [JsonProperty("cover", NullValueHandling = NullValueHandling.Ignore)]
            public string Cover { get; set; }

            [JsonProperty("rating_normal", NullValueHandling = NullValueHandling.Ignore)]
            public long? RatingNormal { get; set; }

            [JsonProperty("rating_bad", NullValueHandling = NullValueHandling.Ignore)]
            public long? RatingBad { get; set; }

            [JsonProperty("rating_good", NullValueHandling = NullValueHandling.Ignore)]
            public long? RatingGood { get; set; }

            [JsonProperty("shop_covers")]
            public object ShopCovers { get; set; }

            [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
            public string Description { get; set; }

            [JsonProperty("is_semi_inactive", NullValueHandling = NullValueHandling.Ignore)]
            public bool? IsSemiInactive { get; set; }

            [JsonProperty("is_blocking_owner")]
            public object IsBlockingOwner { get; set; }

            [JsonProperty("preparation_time", NullValueHandling = NullValueHandling.Ignore)]
            public long? PreparationTime { get; set; }

            [JsonProperty("cancellation_rate", NullValueHandling = NullValueHandling.Ignore)]
            public long? CancellationRate { get; set; }

            [JsonProperty("followed")]
            public object Followed { get; set; }

            [JsonProperty("buyer_rating", NullValueHandling = NullValueHandling.Ignore)]
            public BuyerRating BuyerRating { get; set; }

            [JsonProperty("vacation", NullValueHandling = NullValueHandling.Ignore)]
            public bool? Vacation { get; set; }

            [JsonProperty("show_low_fulfillment_warning", NullValueHandling = NullValueHandling.Ignore)]
            public bool? ShowLowFulfillmentWarning { get; set; }

            [JsonProperty("show_official_shop_label", NullValueHandling = NullValueHandling.Ignore)]
            public bool? ShowOfficialShopLabel { get; set; }

            [JsonProperty("show_official_shop_label_in_title", NullValueHandling = NullValueHandling.Ignore)]
            public bool? ShowOfficialShopLabelInTitle { get; set; }

            [JsonProperty("show_shopee_verified_label", NullValueHandling = NullValueHandling.Ignore)]
            public bool? ShowShopeeVerifiedLabel { get; set; }

            [JsonProperty("show_official_shop_label_in_normal_position")]
            public object ShowOfficialShopLabelInNormalPosition { get; set; }

            [JsonProperty("real_url_if_matching_custom_url")]
            public object RealUrlIfMatchingCustomUrl { get; set; }

            [JsonProperty("status", NullValueHandling = NullValueHandling.Ignore)]
            public long? Status { get; set; }

            [JsonProperty("cb_option", NullValueHandling = NullValueHandling.Ignore)]
            public long? CbOption { get; set; }

            [JsonProperty("campaign_id")]
            public object CampaignId { get; set; }

            [JsonProperty("has_decoration", NullValueHandling = NullValueHandling.Ignore)]
            public bool? HasDecoration { get; set; }

            [JsonProperty("shop_location", NullValueHandling = NullValueHandling.Ignore)]
            public string ShopLocation { get; set; }

            [JsonProperty("rating_star", NullValueHandling = NullValueHandling.Ignore)]
            public double? RatingStar { get; set; }

            [JsonProperty("is_ab_test")]
            public object IsAbTest { get; set; }

            [JsonProperty("show_live_tab", NullValueHandling = NullValueHandling.Ignore)]
            public bool? ShowLiveTab { get; set; }

            [JsonProperty("has_flash_sale", NullValueHandling = NullValueHandling.Ignore)]
            public bool? HasFlashSale { get; set; }

            [JsonProperty("userid", NullValueHandling = NullValueHandling.Ignore)]
            public long? Userid { get; set; }

            [JsonProperty("shopid", NullValueHandling = NullValueHandling.Ignore)]
            public long? Shopid { get; set; }

            [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
            public string Name { get; set; }

            [JsonProperty("item_count", NullValueHandling = NullValueHandling.Ignore)]
            public long? ItemCount { get; set; }

            [JsonProperty("follower_count", NullValueHandling = NullValueHandling.Ignore)]
            public long? FollowerCount { get; set; }

            [JsonProperty("response_rate", NullValueHandling = NullValueHandling.Ignore)]
            public long? ResponseRate { get; set; }

            [JsonProperty("response_time", NullValueHandling = NullValueHandling.Ignore)]
            public long? ResponseTime { get; set; }

            [JsonProperty("account", NullValueHandling = NullValueHandling.Ignore)]
            public Account Account { get; set; }

            [JsonProperty("campaign_config")]
            public object CampaignConfig { get; set; }

            [JsonProperty("has_shopee_flash_sale", NullValueHandling = NullValueHandling.Ignore)]
            public bool? HasShopeeFlashSale { get; set; }

            [JsonProperty("has_in_shop_flash_sale", NullValueHandling = NullValueHandling.Ignore)]
            public bool? HasInShopFlashSale { get; set; }

            [JsonProperty("has_brand_sale", NullValueHandling = NullValueHandling.Ignore)]
            public bool? HasBrandSale { get; set; }

            [JsonProperty("is_preferred_plus_seller", NullValueHandling = NullValueHandling.Ignore)]
            public bool? IsPreferredPlusSeller { get; set; }

            [JsonProperty("show_new_arrival_items", NullValueHandling = NullValueHandling.Ignore)]
            public bool? ShowNewArrivalItems { get; set; }

            [JsonProperty("new_arrival_items_start_ts", NullValueHandling = NullValueHandling.Ignore)]
            public long? NewArrivalItemsStartTs { get; set; }
        }

        public partial class Account
        {
            [JsonProperty("username", NullValueHandling = NullValueHandling.Ignore)]
            public string Username { get; set; }

            [JsonProperty("following_count", NullValueHandling = NullValueHandling.Ignore)]
            public long? FollowingCount { get; set; }

            [JsonProperty("portrait", NullValueHandling = NullValueHandling.Ignore)]
            public string Portrait { get; set; }

            [JsonProperty("is_seller", NullValueHandling = NullValueHandling.Ignore)]
            public bool? IsSeller { get; set; }

            [JsonProperty("phone_verified", NullValueHandling = NullValueHandling.Ignore)]
            public bool? PhoneVerified { get; set; }

            [JsonProperty("email_verified", NullValueHandling = NullValueHandling.Ignore)]
            public bool? EmailVerified { get; set; }

            [JsonProperty("fbid", NullValueHandling = NullValueHandling.Ignore)]
            public string Fbid { get; set; }

            [JsonProperty("total_avg_star", NullValueHandling = NullValueHandling.Ignore)]
            public double? TotalAvgStar { get; set; }

            [JsonProperty("hide_likes", NullValueHandling = NullValueHandling.Ignore)]
            public long? HideLikes { get; set; }

            [JsonProperty("feed_account_info", NullValueHandling = NullValueHandling.Ignore)]
            public FeedAccountInfo FeedAccountInfo { get; set; }

            [JsonProperty("status", NullValueHandling = NullValueHandling.Ignore)]
            public long? Status { get; set; }
        }

        public partial class FeedAccountInfo
        {
            [JsonProperty("is_kol")]
            public object IsKol { get; set; }

            [JsonProperty("can_post_feed", NullValueHandling = NullValueHandling.Ignore)]
            public bool? CanPostFeed { get; set; }
        }

        public partial class BuyerRating
        {
            [JsonProperty("rating_count")]
            public object RatingCount { get; set; }

            [JsonProperty("rating_star")]
            public object RatingStar { get; set; }
        }

        public partial class ShopeeShopDetailModel
        {
            public static ShopeeShopDetailModel FromJson(string json) => JsonConvert.DeserializeObject<ShopeeShopDetailModel>(json, Converter.Settings);
        }

        public static class Serialize
        {
            // public static string ToJson(this ShopeeShopDetailModel self) => JsonConvert.SerializeObject(self, Converter.Settings);
        }

        internal static class Converter
        {
            public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
            {
                MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
                DateParseHandling = DateParseHandling.None,
                Converters =
            {
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
            };
        }

    }
}
