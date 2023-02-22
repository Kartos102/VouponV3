using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Voupon.Common.Enum
{
    public enum DemographicTypeEnum 
    {
        [Description("Age")]
        Age = 1,

        [Description("Education")]
        Education = 2,

        [Description("Gender")]
        Gender = 3,

        [Description("HouseholdMinorSize")]
        HouseholdMinorSize = 4,

        [Description("House hold size")]
        HouseHoldSize = 5,

        [Description("Grocery shopper")]
        GroceryShopper = 6,

        [Description("Occupation")]
        Occupation = 7,

        [Description("Ethnicity")]
        Ethnicity = 8,

        [Description("State")]
        State = 9,

        [Description("Religion")]
        Religion = 10,

        [Description("District")]
        District = 11,

        [Description("Area")]
        Area = 12,

        [Description("Monthly Personal Income")]
        MonthlyIncome = 13,

        [Description("Marital Status")]
        MaritalStatus = 14,

        [Description("Rural/Urban")]
        RuralUrban = 15,

        [Description("Monthly Household Income")]
        MonthlyHouseholdIncome = 16

    }
}
