using System;
using System.Collections.Generic;

namespace Voupon.Database.Postgres.VodusEntities
{
    public partial class PartnerWebsites
    {
        public PartnerWebsites()
        {
            ClientProfiles = new HashSet<ClientProfiles>();
            PartnerWebsiteVisits = new HashSet<PartnerWebsiteVisits>();
            ProductAdPartnersDomainWebsites = new HashSet<ProductAdPartnersDomainWebsites>();
        }

        public int Id { get; set; }
        public int? PartnerId { get; set; }
        public string? Code { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Url { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public bool? CommercialEnabled { get; set; }
        public string? LogoUrl { get; set; }
        public string? BackgroundColor { get; set; }
        public string? FontColor { get; set; }
        public short VodusLogoType { get; set; }
        public string? ButtonColor { get; set; }
        public string? SelectedAnswerColor { get; set; }
        public string? PrimaryColor { get; set; }
        public string? SecondaryColor { get; set; }
        public string? DMPTargetAudience { get; set; }
        public string? DMPTargetCode { get; set; }
        public short? DmpType { get; set; }
        public string? DmpUrl { get; set; }
        public string? DMPCode { get; set; }
        public bool IsJSConsoleLogEnabled { get; set; }
        public int Interval { get; set; }
        public int Delay { get; set; }
        public int CTCTimer { get; set; }
        public int CTCInterval { get; set; }
        public string Language { get; set; } = null!;
        public bool? ModalClosable { get; set; }
        public int MinSessionCount { get; set; }
        public int CCType { get; set; }
        public int NoDemo { get; set; }
        public int DailyAllowance { get; set; }
        public int DemographicCCType { get; set; }
        public int DemographicCTCTimer { get; set; }
        public int DemographicInterval { get; set; }
        public int CCScrollTrigger { get; set; }
        public string CatfishPosition { get; set; } = null!;
        public int ChainQuota { get; set; }
        public int BannerMode { get; set; }
        public int IntervalBannerMode { get; set; }
        public bool IsAdminCCControl { get; set; }
        public int STOAfterTotalNoResponse { get; set; }

        public virtual Partners? Partner { get; set; }
        public virtual ICollection<ClientProfiles> ClientProfiles { get; set; }
        public virtual ICollection<PartnerWebsiteVisits> PartnerWebsiteVisits { get; set; }
        public virtual ICollection<ProductAdPartnersDomainWebsites> ProductAdPartnersDomainWebsites { get; set; }
    }
}
