using System;
using System.Collections.Generic;
using System.Text;

namespace Voupon.Common.Azure.Blob
{
    public static class FilePathEnum
    {
        public static readonly string Temporary = "temporary";
        public static readonly string Merchants_BusinessInfo = "businessInfo";
        public static readonly string Merchants_Logo = "businessInfo/logo";
        public static readonly string Merchants_BusinessInfo_Documents = "businessInfo/documents";
        public static readonly string Merchants_PersonInCharge = "person-in-charge";
        public static readonly string Merchants_PersonInCharge_Documents = "person-in-charge/documents";
        public static readonly string Merchants_BankAccount = "bank-account";
        public static readonly string Merchants_BankAccount_Documents = "bank-account/documents";
        public static readonly string Merchants_Outlets = "outlets";        
        public static readonly string Products = "products";
        public static readonly string Products_Images = "images";
        public static readonly string PRODUCT_DESCRIPTION_IMAGES = "description/images";
        public static readonly string Products_Temporary_Images = "temporary/images";
        public static readonly string Products_Variation_Images = "variation/images";
        public static readonly string Outlets_Images = "images";
    }
}
