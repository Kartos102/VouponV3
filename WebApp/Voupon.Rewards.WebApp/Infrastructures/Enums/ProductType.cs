using System;
using System.ComponentModel;

namespace Voupon.Rewards.WebApp.Infrastructures.Enums
{
    public enum ProductType : byte
    {
            [Description("Defult")]
            Default = 0,
            [Description("Women's fashion")]
            WomenFashion = 1,

            [Description("Men's fashion")]
            MenFashion = 2,

            [Description("Gadgets and Accessories")]
            GadgetsAndAccessories = 3,

            [Description("Home and Family")]
            HomeAndFamily = 4,

            [Description("Leisure and Entertainment")]
            LeisureAndEntertainment = 5,

            [Description("Beauty, Health and Groceries")]
            BeautyHealthAndGroceries = 6,

            [Description("Sports and Outdoor")]
            SportsAndOutdoor = 7,

            [Description("Vouchers and Others")]
            VouchersAndOthers = 8,

    }
    public static class ProductTypExtension
    {
        public static string GetEnumDescription(this Enum enumValue)
        {
            var field = enumValue.GetType().GetField(enumValue.ToString());
            if (Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) is DescriptionAttribute attribute)
            {
                return attribute.Description;
            }
            throw new ArgumentException("Item not found.", nameof(enumValue));
        }

        public static ProductType GetEnumValueByDescription(this string description)
        {
            var enums = Enum.GetValues(typeof(ProductType));
            foreach (Enum enumItem in enums)
            {
                if (enumItem.GetEnumDescription().ToUpper() == description)
                {
                    return (ProductType)enumItem;
                }
            }
            return ProductType.Default;
            //throw new ArgumentException("Not found.", nameof(description));
        }
    }
    
}
