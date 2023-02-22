using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Voupon.Rewards.WebApp
{
    public class AppSettings
    {
        public Database Database { get; set; }
        public  Cache Cache { get; set; }
        public Identity Identity { get; set; }
        public AzureConfigurations AzureConfigurations { get; set; }
        public GoogleReCaptcha GoogleReCaptcha { get; set; }

        public App App { get; set; }

        public PaymentGateways PaymentGateways { get; set; }

        public ThirdPartyRedemptions ThirdPartyRedemptions { get; set; }

        public Mailer Mailer { get; set; }
        public RewardAds RewardAds { get; set; }
        public Emails Emails { get; set; }
        public ProductSearchSettings ProductSearchSettings { get; set; }

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
        public string AccountActivation { get; set; }
        public string InStoreRedemption { get; set; }
        public string DigitalRedemption { get; set; }
        public string CancelPendingOrderByScheduler { get; set; }
        public string InStoreMerchantSalesNotificationEmail { get; set; }
        public string DigitalMerchantSalesNotificationEmail { get; set; }
        public string DeliveryMerchantSalesNotificationEmail { get; set; }
        public string FirstTimeUserPromoEmail { get; set; }
    }

    public class GoogleReCaptcha
    {
        public string ClientKey { get; set; }
        public string Secretkey { get; set; }
    }

    public class PaymentGateways
    {
        public Ipay88 Ipay88 { get; set; }
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


    public class Ipay88
    {
        public string MerchantCode { get; set; }
        public string MerchantKey { get; set; }
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
    public class Emails
    {
        public string Support { get; set; }
        public string Noreply { get; set; }
        public string Receipt { get; set; }
    }
    public class App
    {
        public bool UseLocalAggregator { get; set; }
        public string VouponMerchantAppBaseUrl { get; set; }
        public string APIUrl { get; set; }
        public string ServerlessUrl { get; set; }
        public string BaseUrl { get; set; }
        public string ChatAPIUrl { get; set; }
        public string CDNUrl { get; set; }
        public string CookieName { get; set; }
        public string CookieDomain { get; set; }
        public string ApiCookieDomain { get; set; }
        public bool IsDebugModeEnabled { get; set; }
        public bool IsSecure { get; set; }
        public string TemplateCacheBusterCode { get; set; }
        public decimal AdditionalShippingDiscount { get; set; }

        public string GhostMemberProfileId { get; set; }

        public string CCJSFileUrl { get; set; }
    }
    public class Identity
    {
        public string JwtSecret { get; set; }
    }
    public class RewardAds
    {
        public string VPointCap { get; set; }
    }

    public class ProductSearchSettings
    {
        public int PerMerchant { get; set; }
        public int PerPage { get; set; }
    }

}
