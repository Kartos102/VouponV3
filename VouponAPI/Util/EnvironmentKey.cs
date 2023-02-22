using System;
using System.Collections.Generic;
using System.Text;

namespace Voupon.API.Util
{
    public static class EnvironmentKey
    {
        public static readonly string ADMIN_USER_ID = "ADMIN_USER_ID";
        public static readonly string JWT_SECRET = "JWT_SECRET";
        public static readonly string REDIS_CONNECTION_STRING = "REDIS_CONNECTION_STRING";
        public static readonly string SQL_REWARDS_CONNECTION_STRING = "SQL_REWARDS_CONNECTION_STRING";
        public static readonly string SQL_CONNECTION_STRING = "SQL_CONNECTION_STRING";
        public static readonly string BASE_URL = "BASE_URL";
        public static readonly string VOUPON_URL = "VOUPON_URL";
        public static readonly string ENV = "ENV";
        public static readonly string USE_LOCAL_AGGREGATOR = "USE_LOCAL_AGGREGATOR";
        public static readonly string SMS_API_KEY = "SMS_API_KEY";
        public static readonly string SMS_EMAIL = "SMS_EMAIL";

        public static class SENDGRID
        {
            public static readonly string API_KEY = "SENDGRID_APIKEY";
            public static class TEMPLATES
            {
                public static readonly string RESET_PASSWORD = "SENDGRID_TEMPLATES_RESET_PASSWORD";
                public static readonly string ORDER_CONFIRMATION = "SENDGRID_TEMPLATE_ORDER_CONFIRMATION";
                public static readonly string IN_STORE_MERCHANT_SALES_NOTIFICATION_EMAIL = "SENDGRID_TEMPLATE_IN_STORE_MERCHANT_SALES_NOTIFICATION_EMAIL";
                public static readonly string IN_STORE_REDEMPTION = "SENDGRID_TEMPLATE_IN_STORE_REDEMPTION";
                public static readonly string DIGITAL_MERCHANT_SALES_NOTIFICATION_EMAIL = "SENDGRID_TEMPLATE_DIGITAL_MERCHANT_SALES_NOTIFICATION_EMAIL";
                public static readonly string DELIVERY_MERCHANT_SALES_NOTIFICATION_EMAIL = "SENDGRID_TEMPLATE_DELIVERY_MERCHANT_SALES_NOTIFICATION_EMAIL";
                public static readonly string VERIFY_EMAIL = "SENDGRID_TEMPLATE_VERIFY_EMAIL";
            }
        }

        public static class EMAIL
        {
            public static class FROM
            {
                public static readonly string NO_REPLY = "EMAIL_FROM_NO_REPLY";
                public static readonly string SUPPORT = "EMAIL_FROM_SUPPORT";
                public static readonly string RECEIPT = "EMAIL_FROM_RECEIPT";
            }
        }

        public static class AZURECONFIGURATION
        {
            public static readonly string STORAGE_ACCOUNT = "AZURE_CONFIGURATION_STORAGE_ACCOUNT";
            public static readonly string STORAGE_KEY = "AZURE_CONFIGURATION_STORAGE_KEY";
        }

        public static class PRODUCT_IMAGE
        {
            public static class SIZE
            {
                public static class SMALL
                {
                    public static readonly string HEIGHT = "PRODUCT_IMAGE_SIZE_SMALL_HEIGHT";
                    public static readonly string WIDTH = "PRODUCT_IMAGE_SIZE_SMALL_WIDTH";
                }
                public static class NORMAL
                {
                    public static readonly string HEIGHT = "PRODUCT_IMAGE_SIZE_NORMAL_HEIGHT";
                    public static readonly string WIDTH = "PRODUCT_IMAGE_SIZE_NORMAL_WIDTH";
                }
                public static class BIG
                {
                    public static readonly string HEIGHT = "PRODUCT_IMAGE_SIZE_BIG_HEIGHT";
                    public static readonly string WIDTH = "PRODUCT_IMAGE_SIZE_BIG_WIDTH";
                }
            }

        }

        public static class SMS
        {
            public static readonly string SMS_API_KEY = "SMS_API_KEY";
            public static readonly string SMS_EMAIL = "SMS_EMAIL";

        }


    }
}
