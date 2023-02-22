using System;
using System.Collections.Generic;

namespace Voupon.Database.Postgres.RewardsEntities
{
    public partial class Outlets
    {
        public Outlets()
        {
            InStoreRedemptionTokens = new HashSet<InStoreRedemptionTokens>();
            ProductOutlets = new HashSet<ProductOutlets>();
        }

        public int Id { get; set; }
        public int MerchantId { get; set; }
        public string? Name { get; set; }
        public string? StreetName { get; set; }
        public string? Address_1 { get; set; }
        public string? Address_2 { get; set; }
        public int? CountryId { get; set; }
        public int? ProvinceId { get; set; }
        public int? DistrictId { get; set; }
        public int? PostCodeId { get; set; }
        public string? Contact { get; set; }
        public string? ImgUrl { get; set; }
        public string? OpeningHour_1 { get; set; }
        public string? OpeningHour_2 { get; set; }
        public string? OpeningHour_3 { get; set; }
        public string? OpeningHour_4 { get; set; }
        public string? OpeningHour_5 { get; set; }
        public string? OpeningHour_6 { get; set; }
        public string? OpeningHour_7 { get; set; }
        public string? OpeningHour_8 { get; set; }
        public bool IsActivated { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid CreatedByUserId { get; set; }
        public DateTime? LastUpdatedAt { get; set; }
        public Guid? LastUpdatedByUserId { get; set; }
        public bool IsDeleted { get; set; }

        public virtual Countries? Country { get; set; }
        public virtual Districts? District { get; set; }
        public virtual Merchants Merchant { get; set; } = null!;
        public virtual PostCodes? PostCode { get; set; }
        public virtual Provinces? Province { get; set; }
        public virtual ICollection<InStoreRedemptionTokens> InStoreRedemptionTokens { get; set; }
        public virtual ICollection<ProductOutlets> ProductOutlets { get; set; }
    }
}
