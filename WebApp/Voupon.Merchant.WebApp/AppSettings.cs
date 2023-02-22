using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Voupon.Merchant.WebApp
{
    public class AppSettings
    {
        public Database Database { get; set; }
        public  Cache Cache { get; set; }
        public Identity Identity { get; set; }
        public AzureConfigurations AzureConfigurations { get; set; }
        public GoogleReCaptcha GoogleReCaptcha { get; set; }

        public Mailer Mailer { get; set; }

        public App App { get; set; }

        public ThirdPartyRedemptions ThirdPartyRedemptions { get; set; }
        public RewardAds RewardAds { get; set; }
        public ProductImage ProductImage { get; set; }

        public PaymentGateways PaymentGateways { get; set; }

        public Emails Emails { get; set; }

    }
    
    public class Cache
    {
        public string RedisConnectionString { get; set; }
    }
    public class Database
    {
        public string RewardsConnectionString { get; set; }
        public string VodusConnectionString { get; set; }
    }

    public class PaymentGateways
    {
        public RevenueMonster RevenueMonster { get; set; }
    }

    public class RevenueMonster
    {
        public string AuthUrl { get; set; }
        public string ApiUrl { get; set; }
        public string PrivateKey { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
    }

    public class App
    {
        public string BaseUrl { get; set; }
        public string VouponUrl { get; set; }
        public string ServerlessUrl { get; set; }
        public string ChatAPIUrl { get; set; }
        public string VouponAPIUrl { get; set; }
    }
    public class Mailer
    {
        public Sendgrid Sendgrid { get; set; }
    }

    public class Sendgrid
    {
        public string APIKey { get; set; }
        public Templates Templates { get; set; }
    }

    public class Templates
    {
        public string OrderConfirmation { get; set; }
        public string ThirdPartyRedemption { get; set; }
        public string AppNotification { get; set; }
        public string VerifyEmail { get; set; }
        public string LuckyDrawWin { get; set; }
        public string ResetPassword { get; set; }

        public string RefundToBuyer { get; set; }
        public string ResetPasswordMerchant { get; set; }
        public string AccountActivation { get; set; }

        public string AdminReviewResponse { get; set; }
        public string RedemptionConfirmation { get; set; }

        public string DeliveryTrackingConfirmation { get; set; }
        public string MerchantsAndRespondentGeneralEmail { get; set; }

    }
    public class GoogleReCaptcha
    {
        public string ClientKey { get; set; }
        public string Secretkey { get; set; }
    }
    public class ThirdPartyRedemptions
    {
        public Giftee Giftee { get; set; }
    }
    public class Giftee
    {
        public string Url { get; set; }

        public string CampaignName { get; set; }
        public string Distributor { get; set; }
        public string Token { get; set; }

        public bool Option { get; set; }
        public Products Products { get; set; }
    }

    public class Products
    {
        public string SushiKing { get; set; }
        public string TeaLive { get; set; }
        public string BigApple { get; set; }
        public string LaoLao { get; set; }
        public string Hokkaido { get; set; }
    }

    public class AzureConfigurations
    {
        public string StorageConnectionString { get; set; }
        public string StorageAccount { get; set; }
        public string StorageKey { get; set; }
    }
    public class Identity
    {
        public string JwtSecret { get; set; }
    }
    public class RewardAds
    {
        public string VPointCap { get; set; }
    }
    
    public class Emails
    {
        public string Support { get; set; }
        public string Noreply { get; set; }
        public string Receipt { get; set; }
    }
    public class ProductImage
    {
        public ProductImageSizes Big { get; set; }
        public ProductImageSizes Normal { get; set; }
        public ProductImageSizes Small { get; set; }
    }
    public class ProductImageSizes
    {
        public string Height { get; set; }
        public string Width { get; set; }
    }

}
