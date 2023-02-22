using System.ComponentModel;

namespace Voupon.Merchant.WebApp.Infrastructure.Enums
{
    public enum UserType : int
    {
            [Description("User")]
            User = 1,
            [Description("Admin")]
            Admin = 2,
            [Description("Merchat Aggregator")]
            Merchat = 3,

    }

}
