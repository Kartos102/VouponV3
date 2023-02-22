using System;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Voupon.Database.MSSQL.VodusEntities
{
    public partial class VodusV2Context : IdentityDbContext<Users, Roles, string, UserClaims, UserRoles, UserLogins, RoleClaims, UserTokens>
    {
        public VodusV2Context()
        {
        }

        public VodusV2Context(DbContextOptions<VodusV2Context> options)
            : base(options)
        {
        }

        public virtual DbSet<AdvertisementResponseDemographic> AdvertisementResponseDemographic { get; set; }
        public virtual DbSet<AdvertisementResponses> AdvertisementResponses { get; set; }
        public virtual DbSet<Advertisements> Advertisements { get; set; }
        public virtual DbSet<AggregatorApiUrls> AggregatorApiUrls { get; set; }
        public virtual DbSet<AggregatorConfig> AggregatorConfig { get; set; }
        public virtual DbSet<AggregatorExcludeFilters> AggregatorExcludeFilters { get; set; }
        public virtual DbSet<AppRolePermissions> AppRolePermissions { get; set; }
        public virtual DbSet<AppRoles> AppRoles { get; set; }
        public virtual DbSet<Areas> Areas { get; set; }
        public virtual DbSet<ArticleCategories> ArticleCategories { get; set; }
        public virtual DbSet<Articles> Articles { get; set; }
        public virtual DbSet<AspNetRoles> AspNetRoles { get; set; }
        public virtual DbSet<AspNetUserClaims> AspNetUserClaims { get; set; }
        public virtual DbSet<AspNetUserLogins> AspNetUserLogins { get; set; }
        public virtual DbSet<AspNetUserRoles> AspNetUserRoles { get; set; }
        public virtual DbSet<AspNetUsers> AspNetUsers { get; set; }
        public virtual DbSet<Billings> Billings { get; set; }
        public virtual DbSet<BlacklistedIPAddress> BlacklistedIPAddress { get; set; }
        public virtual DbSet<BonusPoints> BonusPoints { get; set; }
        public virtual DbSet<ClientPasswordResets> ClientPasswordResets { get; set; }
        public virtual DbSet<ClientProfiles> ClientProfiles { get; set; }
        public virtual DbSet<ClientSurveyQuestionTemplates> ClientSurveyQuestionTemplates { get; set; }
        public virtual DbSet<ClientSurveyTemplates> ClientSurveyTemplates { get; set; }
        public virtual DbSet<CommercialDFPrices> CommercialDFPrices { get; set; }
        public virtual DbSet<CommercialDisplayTarget> CommercialDisplayTarget { get; set; }
        public virtual DbSet<CommercialFilters> CommercialFilters { get; set; }
        public virtual DbSet<CommercialLeadCustomFields> CommercialLeadCustomFields { get; set; }
        public virtual DbSet<CommercialLeads> CommercialLeads { get; set; }
        public virtual DbSet<CommercialPsy> CommercialPsy { get; set; }
        public virtual DbSet<CommercialPsyQuotas> CommercialPsyQuotas { get; set; }
        public virtual DbSet<CommercialRates> CommercialRates { get; set; }
        public virtual DbSet<CommercialRequests> CommercialRequests { get; set; }
        public virtual DbSet<CommercialResponseReports> CommercialResponseReports { get; set; }
        public virtual DbSet<CommercialSubgroups> CommercialSubgroups { get; set; }
        public virtual DbSet<CommercialTargetCosts> CommercialTargetCosts { get; set; }
        public virtual DbSet<CommercialTargets> CommercialTargets { get; set; }
        public virtual DbSet<CommercialTopics> CommercialTopics { get; set; }
        public virtual DbSet<CommercialWeightedScoreD> CommercialWeightedScoreD { get; set; }
        public virtual DbSet<CommercialWeightedScores> CommercialWeightedScores { get; set; }
        public virtual DbSet<Commercials> Commercials { get; set; }
        public virtual DbSet<CommercialsLanguages> CommercialsLanguages { get; set; }
        public virtual DbSet<Countries> Countries { get; set; }
        public virtual DbSet<CustomCombinationAnswers> CustomCombinationAnswers { get; set; }
        public virtual DbSet<CustomCombinationLogicFlows> CustomCombinationLogicFlows { get; set; }
        public virtual DbSet<DeletedMemberPsychographicAnswers> DeletedMemberPsychographicAnswers { get; set; }
        public virtual DbSet<DeletedMemberPsychographics> DeletedMemberPsychographics { get; set; }
        public virtual DbSet<DeletedSurveyResponseAnswers> DeletedSurveyResponseAnswers { get; set; }
        public virtual DbSet<DeletedSurveyResponses> DeletedSurveyResponses { get; set; }
        public virtual DbSet<DemographicTypes> DemographicTypes { get; set; }
        public virtual DbSet<DemographicValues> DemographicValues { get; set; }
        public virtual DbSet<Devices> Devices { get; set; }
        public virtual DbSet<Districts> Districts { get; set; }
        public virtual DbSet<ErrorLogs> ErrorLogs { get; set; }
        public virtual DbSet<FingerPrintTests> FingerPrintTests { get; set; }
        public virtual DbSet<FingerprintIPLogs> FingerprintIPLogs { get; set; }
        public virtual DbSet<Fingerprints> Fingerprints { get; set; }
        public virtual DbSet<HUTSurveyForms> HUTSurveyForms { get; set; }
        public virtual DbSet<HUTSurveyParticipants> HUTSurveyParticipants { get; set; }
        public virtual DbSet<HUTSurveyParticipantsStatusTypes> HUTSurveyParticipantsStatusTypes { get; set; }
        public virtual DbSet<HUTSurveyProjects> HUTSurveyProjects { get; set; }
        public virtual DbSet<HUTSurveyResponses> HUTSurveyResponses { get; set; }
        public virtual DbSet<IP2Locations> IP2Locations { get; set; }
        public virtual DbSet<IPLookups> IPLookups { get; set; }
        public virtual DbSet<InprogressR> InprogressR { get; set; }
        public virtual DbSet<InvalidDomainRequests> InvalidDomainRequests { get; set; }
        public virtual DbSet<InvoiceItems> InvoiceItems { get; set; }
        public virtual DbSet<Invoices> Invoices { get; set; }
        public virtual DbSet<Ip2Nations> Ip2Nations { get; set; }
        public virtual DbSet<Ip2nationCountries> Ip2nationCountries { get; set; }
        public virtual DbSet<Languages> Languages { get; set; }
        public virtual DbSet<LoggedOutTokens> LoggedOutTokens { get; set; }
        public virtual DbSet<MasterMemberProfiles> MasterMemberProfiles { get; set; }
        public virtual DbSet<MasterMemberShippingAddress> MasterMemberShippingAddress { get; set; }
        public virtual DbSet<MemberProfileCatalog> MemberProfileCatalog { get; set; }
        public virtual DbSet<MemberProfileChunks> MemberProfileChunks { get; set; }
        public virtual DbSet<MemberProfileDailyAllowances> MemberProfileDailyAllowances { get; set; }
        public virtual DbSet<MemberProfileEndedCommercials> MemberProfileEndedCommercials { get; set; }
        public virtual DbSet<MemberProfileExtensions> MemberProfileExtensions { get; set; }
        public virtual DbSet<MemberProfileProductAdImpression> MemberProfileProductAdImpression { get; set; }
        public virtual DbSet<MemberProfileProductAdImpressionExtensions> MemberProfileProductAdImpressionExtensions { get; set; }
        public virtual DbSet<MemberProfileSubgroups> MemberProfileSubgroups { get; set; }
        public virtual DbSet<MemberProfileVisits> MemberProfileVisits { get; set; }
        public virtual DbSet<MemberProfiles> MemberProfiles { get; set; }
        public virtual DbSet<MemberProfilesOld> MemberProfilesOld { get; set; }
        public virtual DbSet<MemberProfilesTest> MemberProfilesTest { get; set; }
        public virtual DbSet<MemberProfilesV2> MemberProfilesV2 { get; set; }
        public virtual DbSet<MemberPsychographicAnswers> MemberPsychographicAnswers { get; set; }
        public virtual DbSet<MemberPsychographics> MemberPsychographics { get; set; }
        public virtual DbSet<MobileDevices> MobileDevices { get; set; }
        public virtual DbSet<OrderItemRedemptions> OrderItemRedemptions { get; set; }
        public virtual DbSet<OrderItems> OrderItems { get; set; }
        public virtual DbSet<OrderRedemptions> OrderRedemptions { get; set; }
        public virtual DbSet<Orders> Orders { get; set; }
        public virtual DbSet<PartnerProfiles> PartnerProfiles { get; set; }
        public virtual DbSet<PartnerWebsiteExcludedCommercials> PartnerWebsiteExcludedCommercials { get; set; }
        public virtual DbSet<PartnerWebsitePriorityCommercials> PartnerWebsitePriorityCommercials { get; set; }
        public virtual DbSet<PartnerWebsiteVisits> PartnerWebsiteVisits { get; set; }
        public virtual DbSet<PartnerWebsites> PartnerWebsites { get; set; }
        public virtual DbSet<Partners> Partners { get; set; }
        public virtual DbSet<PasswordResets> PasswordResets { get; set; }
        public virtual DbSet<PaymentStatuses> PaymentStatuses { get; set; }
        public virtual DbSet<Permissions> Permissions { get; set; }
        public virtual DbSet<ProductAdLocations> ProductAdLocations { get; set; }
        public virtual DbSet<ProductAdPartnersDomain> ProductAdPartnersDomain { get; set; }
        public virtual DbSet<ProductAdPartnersDomainWebsites> ProductAdPartnersDomainWebsites { get; set; }
        public virtual DbSet<ProductAdSubgroups> ProductAdSubgroups { get; set; }
        public virtual DbSet<ProductAds> ProductAds { get; set; }
        public virtual DbSet<ProductAdsConfig> ProductAdsConfig { get; set; }
        public virtual DbSet<Products> Products { get; set; }
        public virtual DbSet<PromoCodes> PromoCodes { get; set; }
        public virtual DbSet<PsyQuestionAnswers> PsyQuestionAnswers { get; set; }
        public virtual DbSet<PsyQuestionMultiLanguagesAnswers> PsyQuestionMultiLanguagesAnswers { get; set; }
        public virtual DbSet<PsyQuestions> PsyQuestions { get; set; }
        public virtual DbSet<QuestionDelays> QuestionDelays { get; set; }
        public virtual DbSet<QuestionTypes> QuestionTypes { get; set; }
        public virtual DbSet<RequestForDownloads> RequestForDownloads { get; set; }
        public virtual DbSet<RoleClaims> RoleClaims { get; set; }
        public virtual DbSet<Roles> Roles { get; set; }
        public virtual DbSet<States> States { get; set; }
        public virtual DbSet<StatusTypes> StatusTypes { get; set; }
        public virtual DbSet<SubgroupDemographicValues> SubgroupDemographicValues { get; set; }
        public virtual DbSet<SubgroupMultiplierDemographics> SubgroupMultiplierDemographics { get; set; }
        public virtual DbSet<SubgroupMultipliers> SubgroupMultipliers { get; set; }
        public virtual DbSet<Subgroups> Subgroups { get; set; }
        public virtual DbSet<SubgroupsV2> SubgroupsV2 { get; set; }
        public virtual DbSet<SurveyCampaigns> SurveyCampaigns { get; set; }
        public virtual DbSet<SurveyChunkings> SurveyChunkings { get; set; }
        public virtual DbSet<SurveyLogicFlows> SurveyLogicFlows { get; set; }
        public virtual DbSet<SurveyQuestionAnswers> SurveyQuestionAnswers { get; set; }
        public virtual DbSet<SurveyQuestionChucks> SurveyQuestionChucks { get; set; }
        public virtual DbSet<SurveyQuestionMultiLanguagesAnswers> SurveyQuestionMultiLanguagesAnswers { get; set; }
        public virtual DbSet<SurveyQuestionMultiLanguagesTitles> SurveyQuestionMultiLanguagesTitles { get; set; }
        public virtual DbSet<SurveyQuestionTemplates> SurveyQuestionTemplates { get; set; }
        public virtual DbSet<SurveyQuestionTiers> SurveyQuestionTiers { get; set; }
        public virtual DbSet<SurveyQuestions> SurveyQuestions { get; set; }
        public virtual DbSet<SurveyResponseAnswers> SurveyResponseAnswers { get; set; }
        public virtual DbSet<SurveyResponseDemographic> SurveyResponseDemographic { get; set; }
        public virtual DbSet<SurveyResponses> SurveyResponses { get; set; }
        public virtual DbSet<SurveyTemplates> SurveyTemplates { get; set; }
        public virtual DbSet<SystemConfigurations> SystemConfigurations { get; set; }
        public virtual DbSet<TempTokens> TempTokens { get; set; }
        public virtual DbSet<Temp_CommercialFilters> Temp_CommercialFilters { get; set; }
        public virtual DbSet<Temp_CommercialWeightedScoreDemographics> Temp_CommercialWeightedScoreDemographics { get; set; }
        public virtual DbSet<Temp_CommercialWeightedScores> Temp_CommercialWeightedScores { get; set; }
        public virtual DbSet<Topics> Topics { get; set; }
        public virtual DbSet<TreatedDataJsonFiles> TreatedDataJsonFiles { get; set; }
        public virtual DbSet<UserClaims> UserClaims { get; set; }
        public virtual DbSet<UserLogins> UserLogins { get; set; }
        public virtual DbSet<UserRoles> UserRoles { get; set; }
        public virtual DbSet<UserTokens> UserTokens { get; set; }
        public virtual DbSet<Users> Users { get; set; }
        public virtual DbSet<Variables> Variables { get; set; }
        public virtual DbSet<__MigrationHistory> __MigrationHistory { get; set; }
        public virtual DbSet<vw_questionnaire> vw_questionnaire { get; set; }
        public virtual DbSet<RequestHeaders> RequestHeaders { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
                optionsBuilder.UseSqlServer("Server=tcp:vodus-live.database.windows.net,1433;Database=VodusV2;Trusted_Connection=false;Encrypt=True;TrustServerCertificate=False;User id=Vodus;password=Pleasechangethis2017");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<RequestHeaders>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(255);
            });

            modelBuilder.Entity<AdvertisementResponseDemographic>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Value).HasMaxLength(1000);
            });

            modelBuilder.Entity<AdvertisementResponses>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.Host)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.Origin)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.Referer)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.TcpIpProtocol)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.UserAgent)
                    .HasMaxLength(500)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<Advertisements>(entity =>
            {
                entity.Property(e => e.Point).HasColumnType("decimal(10, 4)");

                entity.Property(e => e.Template).IsRequired();
            });

            modelBuilder.Entity<AggregatorApiUrls>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.LastUpdatedAt).HasColumnType("datetime");

                entity.Property(e => e.Url)
                    .IsRequired()
                    .HasMaxLength(255)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<AggregatorConfig>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.LazadaCrawlerEndpoint)
                    .IsRequired()
                    .IsUnicode(false);

                entity.Property(e => e.LazadaCrawlerLastVerifiedAt).HasColumnType("datetime");

                entity.Property(e => e.ShopeeCrawlerEndpoint)
                    .IsRequired()
                    .IsUnicode(false);

                entity.Property(e => e.ShopeeCrawlerLastVerifiedAt).HasColumnType("datetime");
            });

            modelBuilder.Entity<AggregatorExcludeFilters>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Keyword)
                    .IsRequired()
                    .HasMaxLength(255);
            });

            modelBuilder.Entity<AppRolePermissions>(entity =>
            {
                entity.HasKey(e => new { e.AppRoleId, e.PermissionId });

                entity.HasOne(d => d.AppRole)
                    .WithMany(p => p.AppRolePermissions)
                    .HasForeignKey(d => d.AppRoleId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_AppRolePermissions_AppRoles");

                entity.HasOne(d => d.Permission)
                    .WithMany(p => p.AppRolePermissions)
                    .HasForeignKey(d => d.PermissionId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_AppRolePermissions_Permissions");
            });

            modelBuilder.Entity<AppRoles>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<Areas>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.AreaCode).HasMaxLength(50);

                entity.Property(e => e.AreaName)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.HasOne(d => d.District)
                    .WithMany(p => p.Areas)
                    .HasForeignKey(d => d.DistrictId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Areas_Districts");
            });

            modelBuilder.Entity<ArticleCategories>(entity =>
            {
                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.CreatedBy)
                    .IsRequired()
                    .HasMaxLength(128)
                    .IsUnicode(false);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(128);
            });

            modelBuilder.Entity<Articles>(entity =>
            {
                entity.Property(e => e.BannerImageUrl)
                    .HasMaxLength(128)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.CreatedBy)
                    .IsRequired()
                    .HasMaxLength(128)
                    .IsUnicode(false);

                entity.Property(e => e.MetaDescription).HasMaxLength(500);

                entity.Property(e => e.MetaKeywords).HasMaxLength(500);

                entity.Property(e => e.MetaTitle).HasMaxLength(255);

                entity.Property(e => e.PageSlug).HasMaxLength(500);

                entity.Property(e => e.PublishedAt)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.ReportDownloadLink)
                    .HasMaxLength(128)
                    .IsUnicode(false);

                entity.Property(e => e.StoryUrl)
                    .HasMaxLength(128)
                    .IsUnicode(false);

                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.HasOne(d => d.Category)
                    .WithMany(p => p.Articles)
                    .HasForeignKey(d => d.CategoryId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Articles_ArticleCategories");
            });

            modelBuilder.Entity<AspNetRoles>(entity =>
            {
                entity.HasIndex(e => e.Name)
                    .HasName("RoleNameIndex")
                    .IsUnique();

                entity.Property(e => e.Id).HasMaxLength(128);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(256);
            });

            modelBuilder.Entity<AspNetUserClaims>(entity =>
            {
                entity.HasIndex(e => e.UserId)
                    .HasName("IX_UserId");

                entity.Property(e => e.UserId)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.HasOne(d => d.User)
                    .WithMany(p => p.AspNetUserClaims)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK_dbo.AspNetUserClaims_dbo.AspNetUsers_UserId");
            });

            modelBuilder.Entity<AspNetUserLogins>(entity =>
            {
                entity.HasKey(e => new { e.LoginProvider, e.ProviderKey, e.UserId })
                    .HasName("PK_dbo.AspNetUserLogins");

                entity.HasIndex(e => e.UserId)
                    .HasName("IX_UserId");

                entity.Property(e => e.LoginProvider).HasMaxLength(128);

                entity.Property(e => e.ProviderKey).HasMaxLength(128);

                entity.Property(e => e.UserId).HasMaxLength(128);

                entity.HasOne(d => d.User)
                    .WithMany(p => p.AspNetUserLogins)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK_dbo.AspNetUserLogins_dbo.AspNetUsers_UserId");
            });

            modelBuilder.Entity<AspNetUserRoles>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.RoleId })
                    .HasName("PK_dbo.AspNetUserRoles");

                entity.HasIndex(e => e.RoleId)
                    .HasName("IX_RoleId");

                entity.HasIndex(e => e.UserId)
                    .HasName("IX_UserId");

                entity.Property(e => e.UserId).HasMaxLength(128);

                entity.Property(e => e.RoleId).HasMaxLength(128);

                entity.HasOne(d => d.Role)
                    .WithMany(p => p.AspNetUserRoles)
                    .HasForeignKey(d => d.RoleId)
                    .HasConstraintName("FK_dbo.AspNetUserRoles_dbo.AspNetRoles_RoleId");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.AspNetUserRoles)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK_dbo.AspNetUserRoles_dbo.AspNetUsers_UserId");
            });

            modelBuilder.Entity<AspNetUsers>(entity =>
            {
                entity.HasIndex(e => e.UserName)
                    .HasName("UserNameIndex")
                    .IsUnique();

                entity.Property(e => e.Id).HasMaxLength(128);

                entity.Property(e => e.Email).HasMaxLength(256);

                entity.Property(e => e.LockoutEndDateUtc).HasColumnType("datetime");

                entity.Property(e => e.UserName)
                    .IsRequired()
                    .HasMaxLength(256);
            });

            modelBuilder.Entity<Billings>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.BillAmount).HasColumnType("money");

                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.CreatedBy)
                    .IsRequired()
                    .HasMaxLength(128)
                    .IsUnicode(false);

                entity.Property(e => e.InvoiceNumber)
                    .IsRequired()
                    .HasMaxLength(20)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<BlacklistedIPAddress>(entity =>
            {
                entity.HasIndex(e => e.IpAddress)
                    .HasName("IX_BlacklistedIPAddress_IPAddress");

                entity.HasIndex(e => e.PartnerWebsiteId);

                entity.HasIndex(e => e.StatusId);

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.IpAddress)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.LastBannedAt).HasColumnType("datetime");

                entity.Property(e => e.LastCheckedAt).HasColumnType("datetime");

                entity.Property(e => e.LastUnBannedAt).HasColumnType("datetime");

                entity.Property(e => e.Remark).HasMaxLength(255);
            });

            modelBuilder.Entity<BonusPoints>(entity =>
            {
                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.CreatedBy)
                    .IsRequired()
                    .HasMaxLength(128)
                    .IsUnicode(false);

                entity.Property(e => e.DeletedAt).HasColumnType("datetime");

                entity.Property(e => e.DeletedBy)
                    .HasMaxLength(128)
                    .IsUnicode(false);

                entity.Property(e => e.IsActive)
                    .IsRequired()
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.LastUpdatedAt).HasColumnType("datetime");

                entity.Property(e => e.LastUpdatedBy)
                    .HasMaxLength(128)
                    .IsUnicode(false);

                entity.Property(e => e.Remark)
                    .IsRequired()
                    .HasMaxLength(500)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<ClientPasswordResets>(entity =>
            {
                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.ExpireAt).HasColumnType("datetime");

                entity.Property(e => e.ResetCode)
                    .IsRequired()
                    .HasMaxLength(128)
                    .IsUnicode(false);

                entity.Property(e => e.Username)
                    .IsRequired()
                    .HasMaxLength(128)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<ClientProfiles>(entity =>
            {
                entity.Property(e => e.Address1)
                    .HasMaxLength(500)
                    .IsUnicode(false);

                entity.Property(e => e.Address2)
                    .HasMaxLength(500)
                    .IsUnicode(false);

                entity.Property(e => e.Company)
                    .HasMaxLength(500)
                    .IsUnicode(false);

                entity.Property(e => e.CompanyNumber)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.CountryId).HasDefaultValueSql("((160))");

                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.CreatedBy)
                    .HasMaxLength(128)
                    .IsUnicode(false);

                entity.Property(e => e.LineOfWork)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.Title)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.UserId).HasMaxLength(128);

                entity.HasOne(d => d.Country)
                    .WithMany(p => p.ClientProfiles)
                    .HasForeignKey(d => d.CountryId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Country_Id");

                entity.HasOne(d => d.PartnerWebsite)
                    .WithMany(p => p.ClientProfiles)
                    .HasForeignKey(d => d.PartnerWebsiteId)
                    .HasConstraintName("FK_ClientProfiles_PartnerWebsites");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.ClientProfiles)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK_ClientProfiles_ClientProfiles");
            });

            modelBuilder.Entity<ClientSurveyQuestionTemplates>(entity =>
            {
                entity.Property(e => e.QuestionTemplateContent).IsRequired();

                entity.Property(e => e.TemplateContent).IsRequired();

                entity.HasOne(d => d.ClientSurveyTemplate)
                    .WithMany(p => p.ClientSurveyQuestionTemplates)
                    .HasForeignKey(d => d.ClientSurveyTemplateId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ClientSurveyQuestionTemplates_ClientSurveyTemplates");

                entity.HasOne(d => d.QuestionType)
                    .WithMany(p => p.ClientSurveyQuestionTemplates)
                    .HasForeignKey(d => d.QuestionTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ClientSurveyQuestionTemplates_QuestionTypes");
            });

            modelBuilder.Entity<ClientSurveyTemplates>(entity =>
            {
                entity.Property(e => e.CreatedBy).HasMaxLength(255);

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.CreatedIP)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Description)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.LastUpdatedAt).HasColumnType("datetime");

                entity.Property(e => e.LastUpdatedBy).HasMaxLength(255);

                entity.Property(e => e.LastUpdatedIP)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.TemplateName)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.HasOne(d => d.ClientProfile)
                    .WithMany(p => p.ClientSurveyTemplates)
                    .HasForeignKey(d => d.ClientProfileId)
                    .HasConstraintName("FK_ClientSurveyTemplates_ClientProfiles");
            });

            modelBuilder.Entity<CommercialDFPrices>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.CostPrice).HasColumnType("money");
            });

            modelBuilder.Entity<CommercialDisplayTarget>(entity =>
            {
                entity.Property(e => e.IsEnabled)
                    .IsRequired()
                    .HasDefaultValueSql("((1))");

                entity.HasOne(d => d.Commercial)
                    .WithMany(p => p.CommercialDisplayTarget)
                    .HasForeignKey(d => d.CommercialId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_CommercialDisplayTarget_Commercials");
            });

            modelBuilder.Entity<CommercialFilters>(entity =>
            {
                entity.HasKey(e => new { e.CommercialId, e.SequenceNumber });

                entity.Property(e => e.QuotaType).HasMaxLength(10);

                entity.HasOne(d => d.Commercial)
                    .WithMany(p => p.CommercialFilters)
                    .HasForeignKey(d => d.CommercialId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_CommercialFilters_Commercials");

                entity.HasOne(d => d.DemographicType)
                    .WithMany(p => p.CommercialFilters)
                    .HasForeignKey(d => d.DemographicTypeId)
                    .HasConstraintName("FK_CommercialFilters_DemographicTypes");
            });

            modelBuilder.Entity<CommercialLeadCustomFields>(entity =>
            {
                entity.Property(e => e.CustomField)
                    .IsRequired()
                    .HasMaxLength(500)
                    .IsUnicode(false);

                entity.Property(e => e.CustomValue)
                    .IsRequired()
                    .HasMaxLength(500)
                    .IsUnicode(false);

                entity.HasOne(d => d.CommercialLead)
                    .WithMany(p => p.CommercialLeadCustomFields)
                    .HasForeignKey(d => d.CommercialLeadId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_CommercialLeadCustomFields_CommercialLeads");
            });

            modelBuilder.Entity<CommercialLeads>(entity =>
            {
                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.LastUpdatedAt).HasColumnType("datetime");

                entity.Property(e => e.RefererHostName)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.RefererUrl)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.Remark).HasMaxLength(500);

                entity.Property(e => e.VerificationCode)
                    .IsRequired()
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.HasOne(d => d.Commercial)
                    .WithMany(p => p.CommercialLeads)
                    .HasForeignKey(d => d.CommercialId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_CommercialLeads_Commercials");
            });

            modelBuilder.Entity<CommercialPsy>(entity =>
            {
                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.LastUpdatedAt).HasColumnType("datetime");

                entity.Property(e => e.QuotaType).HasDefaultValueSql("((1))");

                entity.HasOne(d => d.Commercial)
                    .WithMany(p => p.CommercialPsy)
                    .HasForeignKey(d => d.CommercialId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_CommercialPsy_Commercials");

                entity.HasOne(d => d.PsyQuestion)
                    .WithMany(p => p.CommercialPsy)
                    .HasForeignKey(d => d.PsyQuestionId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_CommercialPsy_PsyQuestions");
            });

            modelBuilder.Entity<CommercialPsyQuotas>(entity =>
            {
                entity.Property(e => e.AnswerText)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.HasOne(d => d.CommercialPsy)
                    .WithMany(p => p.CommercialPsyQuotas)
                    .HasForeignKey(d => d.CommercialPsyId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_CommercialPsyQuotas_CommercialPsy");
            });

            modelBuilder.Entity<CommercialRates>(entity =>
            {
                entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.CountryCode)
                    .IsRequired()
                    .HasMaxLength(4)
                    .IsUnicode(false);

                entity.Property(e => e.LastUpdatedAt).HasColumnType("datetime");

                entity.Property(e => e.LastUpdatedBy)
                    .IsRequired()
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.HasOne(d => d.CountryCodeNavigation)
                    .WithMany(p => p.CommercialRates)
                    .HasForeignKey(d => d.CountryCode)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_CommercialRates_Ip2nationCountries");
            });

            modelBuilder.Entity<CommercialRequests>(entity =>
            {
                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.Host)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.IpAddress)
                    .HasMaxLength(45)
                    .IsUnicode(false);

                entity.Property(e => e.Origin)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.Referer)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.UserAgent)
                    .HasMaxLength(500)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<CommercialResponseReports>(entity =>
            {
                entity.HasIndex(e => e.CommercialId);

                entity.HasIndex(e => e.MemberProfileId);

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.DemographicAgeDisplay).HasMaxLength(255);

                entity.Property(e => e.DemographicEducationDisplay).HasMaxLength(255);

                entity.Property(e => e.DemographicEthnicityDisplay).HasMaxLength(255);

                entity.Property(e => e.DemographicGenderDisplay).HasMaxLength(255);

                entity.Property(e => e.DemographicMaritalStatusDisplay).HasMaxLength(255);

                entity.Property(e => e.DemographicMonthlyHouseHoldIncomeDisplay).HasMaxLength(255);

                entity.Property(e => e.DemographicMonthlyIncomeDisplay).HasMaxLength(255);

                entity.Property(e => e.DemographicOccupationDisplay).HasMaxLength(255);

                entity.Property(e => e.DemographicRuralUrbanDisplay).HasMaxLength(255);

                entity.Property(e => e.DemographicStateDisplay).HasMaxLength(255);

                entity.Property(e => e.IsCompleted).HasDefaultValueSql("((0))");

                entity.Property(e => e.IsFailedScreening).HasDefaultValueSql("((0))");

                entity.Property(e => e.LastUpdatedAt).HasColumnType("datetime");

                entity.Property(e => e.NumberOfQuestionCompleted).HasDefaultValueSql("((1))");

                entity.Property(e => e.NumberOfTiers).HasDefaultValueSql("((1))");

                entity.Property(e => e.Question10AnswerDisplay).HasMaxLength(1000);

                entity.Property(e => e.Question10AnswerId)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.Question10AnswerUpdatedAt).HasColumnType("datetime");

                entity.Property(e => e.Question10Title).HasMaxLength(1000);

                entity.Property(e => e.Question11AnswerDisplay).HasMaxLength(1000);

                entity.Property(e => e.Question11AnswerId)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.Question11AnswerUpdatedAt).HasColumnType("datetime");

                entity.Property(e => e.Question11Title).HasMaxLength(1000);

                entity.Property(e => e.Question12AnswerDisplay).HasMaxLength(1000);

                entity.Property(e => e.Question12AnswerId)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.Question12AnswerUpdatedAt).HasColumnType("datetime");

                entity.Property(e => e.Question12Title).HasMaxLength(1000);

                entity.Property(e => e.Question13AnswerDisplay).HasMaxLength(1000);

                entity.Property(e => e.Question13AnswerId)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.Question13AnswerUpdatedAt).HasColumnType("datetime");

                entity.Property(e => e.Question13Title).HasMaxLength(1000);

                entity.Property(e => e.Question14AnswerDisplay).HasMaxLength(1000);

                entity.Property(e => e.Question14AnswerId)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.Question14AnswerUpdatedAt).HasColumnType("datetime");

                entity.Property(e => e.Question14Title).HasMaxLength(1000);

                entity.Property(e => e.Question15AnswerDisplay).HasMaxLength(1000);

                entity.Property(e => e.Question15AnswerId)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.Question15AnswerUpdatedAt).HasColumnType("datetime");

                entity.Property(e => e.Question15Title).HasMaxLength(1000);

                entity.Property(e => e.Question16AnswerDisplay).HasMaxLength(1000);

                entity.Property(e => e.Question16AnswerId)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.Question16AnswerUpdatedAt).HasColumnType("datetime");

                entity.Property(e => e.Question16Title).HasMaxLength(1000);

                entity.Property(e => e.Question17AnswerDisplay).HasMaxLength(1000);

                entity.Property(e => e.Question17AnswerId)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.Question17AnswerUpdatedAt).HasColumnType("datetime");

                entity.Property(e => e.Question17Title).HasMaxLength(1000);

                entity.Property(e => e.Question18AnswerDisplay).HasMaxLength(1000);

                entity.Property(e => e.Question18AnswerId)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.Question18AnswerUpdatedAt).HasColumnType("datetime");

                entity.Property(e => e.Question18Title).HasMaxLength(1000);

                entity.Property(e => e.Question19AnswerDisplay).HasMaxLength(1000);

                entity.Property(e => e.Question19AnswerId)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.Question19AnswerUpdatedAt).HasColumnType("datetime");

                entity.Property(e => e.Question19Title).HasMaxLength(1000);

                entity.Property(e => e.Question1AnswerDisplay).HasMaxLength(1000);

                entity.Property(e => e.Question1AnswerId)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.Question1AnswerUpdatedAt).HasColumnType("datetime");

                entity.Property(e => e.Question1Title).HasMaxLength(1000);

                entity.Property(e => e.Question20AnswerDisplay).HasMaxLength(1000);

                entity.Property(e => e.Question20AnswerId)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.Question20AnswerUpdatedAt).HasColumnType("datetime");

                entity.Property(e => e.Question20Title).HasMaxLength(1000);

                entity.Property(e => e.Question21AnswerDisplay).HasMaxLength(1000);

                entity.Property(e => e.Question21AnswerId)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.Question21AnswerUpdatedAt).HasColumnType("datetime");

                entity.Property(e => e.Question21Title).HasMaxLength(1000);

                entity.Property(e => e.Question22AnswerDisplay).HasMaxLength(1000);

                entity.Property(e => e.Question22AnswerId)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.Question22AnswerUpdatedAt).HasColumnType("datetime");

                entity.Property(e => e.Question22Title).HasMaxLength(1000);

                entity.Property(e => e.Question23AnswerDisplay).HasMaxLength(1000);

                entity.Property(e => e.Question23AnswerId)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.Question23AnswerUpdatedAt).HasColumnType("datetime");

                entity.Property(e => e.Question23Title).HasMaxLength(1000);

                entity.Property(e => e.Question24AnswerDisplay).HasMaxLength(1000);

                entity.Property(e => e.Question24AnswerId)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.Question24AnswerUpdatedAt).HasColumnType("datetime");

                entity.Property(e => e.Question24Title).HasMaxLength(1000);

                entity.Property(e => e.Question25AnswerDisplay).HasMaxLength(1000);

                entity.Property(e => e.Question25AnswerId)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.Question25AnswerUpdatedAt).HasColumnType("datetime");

                entity.Property(e => e.Question25Title).HasMaxLength(1000);

                entity.Property(e => e.Question26AnswerDisplay).HasMaxLength(1000);

                entity.Property(e => e.Question26AnswerId)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.Question26AnswerUpdatedAt).HasColumnType("datetime");

                entity.Property(e => e.Question26Title).HasMaxLength(1000);

                entity.Property(e => e.Question27AnswerDisplay).HasMaxLength(1000);

                entity.Property(e => e.Question27AnswerId)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.Question27AnswerUpdatedAt).HasColumnType("datetime");

                entity.Property(e => e.Question27Title).HasMaxLength(1000);

                entity.Property(e => e.Question28AnswerDisplay).HasMaxLength(1000);

                entity.Property(e => e.Question28AnswerId)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.Question28AnswerUpdatedAt).HasColumnType("datetime");

                entity.Property(e => e.Question28Title).HasMaxLength(1000);

                entity.Property(e => e.Question29AnswerDisplay).HasMaxLength(1000);

                entity.Property(e => e.Question29AnswerId)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.Question29AnswerUpdatedAt).HasColumnType("datetime");

                entity.Property(e => e.Question29Title).HasMaxLength(1000);

                entity.Property(e => e.Question2AnswerDisplay).HasMaxLength(1000);

                entity.Property(e => e.Question2AnswerId)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.Question2AnswerUpdatedAt).HasColumnType("datetime");

                entity.Property(e => e.Question2Title).HasMaxLength(1000);

                entity.Property(e => e.Question30AnswerDisplay).HasMaxLength(1000);

                entity.Property(e => e.Question30AnswerId)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.Question30AnswerUpdatedAt).HasColumnType("datetime");

                entity.Property(e => e.Question30Title).HasMaxLength(1000);

                entity.Property(e => e.Question3AnswerDisplay).HasMaxLength(1000);

                entity.Property(e => e.Question3AnswerId)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.Question3AnswerUpdatedAt).HasColumnType("datetime");

                entity.Property(e => e.Question3Title).HasMaxLength(1000);

                entity.Property(e => e.Question4AnswerDisplay).HasMaxLength(1000);

                entity.Property(e => e.Question4AnswerId)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.Question4AnswerUpdatedAt).HasColumnType("datetime");

                entity.Property(e => e.Question4Title).HasMaxLength(1000);

                entity.Property(e => e.Question5AnswerDisplay).HasMaxLength(1000);

                entity.Property(e => e.Question5AnswerId)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.Question5AnswerUpdatedAt).HasColumnType("datetime");

                entity.Property(e => e.Question5Title).HasMaxLength(1000);

                entity.Property(e => e.Question6AnswerDisplay).HasMaxLength(1000);

                entity.Property(e => e.Question6AnswerId)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.Question6AnswerUpdatedAt).HasColumnType("datetime");

                entity.Property(e => e.Question6Title).HasMaxLength(1000);

                entity.Property(e => e.Question7AnswerDisplay).HasMaxLength(1000);

                entity.Property(e => e.Question7AnswerId)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.Question7AnswerUpdatedAt).HasColumnType("datetime");

                entity.Property(e => e.Question7Title).HasMaxLength(1000);

                entity.Property(e => e.Question8AnswerDisplay).HasMaxLength(1000);

                entity.Property(e => e.Question8AnswerId)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.Question8AnswerUpdatedAt).HasColumnType("datetime");

                entity.Property(e => e.Question8Title).HasMaxLength(1000);

                entity.Property(e => e.Question9AnswerDisplay).HasMaxLength(1000);

                entity.Property(e => e.Question9AnswerId)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.Question9AnswerUpdatedAt).HasColumnType("datetime");

                entity.Property(e => e.Question9Title).HasMaxLength(1000);

                entity.Property(e => e.ResponseCost).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.TotalResponseCost).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.UserId)
                    .HasMaxLength(36)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<CommercialSubgroups>(entity =>
            {
                entity.HasIndex(e => new { e.CommercialId, e.IsTemplate })
                    .HasName("nci_wi_CommercialSubgroups_5D77DA7D3D1F0EF8AEE803134CCA66ED");

                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.IsTemplate)
                    .IsRequired()
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.MValue).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.QuotaMultiplierCompletedCount).HasDefaultValueSql("((0))");

                entity.Property(e => e.QuotaMultiplierIsCompleted).HasDefaultValueSql("((0))");

                entity.Property(e => e.QuotaMultiplierVolume).HasDefaultValueSql("((0))");

                entity.Property(e => e.QuotaVolumeCompletedCount).HasDefaultValueSql("((0))");

                entity.Property(e => e.QuotaVolumeIsCompleted).HasDefaultValueSql("((0))");

                entity.Property(e => e.SubgroupName)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.HasOne(d => d.Chunk)
                    .WithMany(p => p.CommercialSubgroups)
                    .HasForeignKey(d => d.ChunkId)
                    .HasConstraintName("FK_CommercialSubgroups_Chunk");

                entity.HasOne(d => d.Commercial)
                    .WithMany(p => p.CommercialSubgroups)
                    .HasForeignKey(d => d.CommercialId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_CommercialSubgroups_Commercials");
            });

            modelBuilder.Entity<CommercialTargetCosts>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.CostPrice).HasColumnType("money");

                entity.HasOne(d => d.CommercialIdDFPrice)
                    .WithMany(p => p.CommercialTargetCosts)
                    .HasForeignKey(d => d.CommercialIdDFPriceId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_CommercialTargetCosts_CommercialDFPrices");

                entity.HasOne(d => d.DemographicType)
                    .WithMany(p => p.CommercialTargetCosts)
                    .HasForeignKey(d => d.DemographicTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_CommercialTargetCosts_DemographicTypes");
            });

            modelBuilder.Entity<CommercialTargets>(entity =>
            {
                entity.Property(e => e.DemographicRates).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.Value).HasMaxLength(100);

                entity.Property(e => e.Value2).HasMaxLength(100);

                entity.HasOne(d => d.Commercial)
                    .WithMany(p => p.CommercialTargets)
                    .HasForeignKey(d => d.CommercialId)
                    .HasConstraintName("FK_CommercialTargets_Commercials");

                entity.HasOne(d => d.CommercialTargetCost)
                    .WithMany(p => p.CommercialTargets)
                    .HasForeignKey(d => d.CommercialTargetCostId)
                    .HasConstraintName("FK_CommercialTargets_CommercialTargetCosts");

                entity.HasOne(d => d.DemographicType)
                    .WithMany(p => p.CommercialTargets)
                    .HasForeignKey(d => d.DemographicTypeId)
                    .HasConstraintName("FK_CommercialTargets_DemographicTypes");

                entity.HasOne(d => d.DemographicValue)
                    .WithMany(p => p.CommercialTargets)
                    .HasForeignKey(d => d.DemographicValueId)
                    .HasConstraintName("FK_CommercialTargets_DemographicValues");
            });

            modelBuilder.Entity<CommercialTopics>(entity =>
            {
                entity.HasKey(e => new { e.CommercialId, e.TopicId });
            });

            modelBuilder.Entity<CommercialWeightedScoreD>(entity =>
            {
                entity.HasKey(e => new { e.CommercialId, e.SequenceNumber, e.SubSequenceNumber, e.CommercialTrgetId });
            });

            modelBuilder.Entity<CommercialWeightedScores>(entity =>
            {
                entity.HasKey(e => new { e.CommercialId, e.SequenceNumber, e.SubSequenceNumber });

                entity.HasOne(d => d.Commercial)
                    .WithMany(p => p.CommercialWeightedScores)
                    .HasForeignKey(d => d.CommercialId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_CommercialWeightedScores_Commercials");
            });

            modelBuilder.Entity<Commercials>(entity =>
            {
                entity.HasIndex(e => new { e.CommercialType, e.PriorityLevel, e.ScreeningQuotaIsCompleted, e.Status })
                    .HasName("nci_wi_Commercials_D93EB5B3773D076F6FB3A91F01FECBB3");

                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.CreatedBy)
                    .IsRequired()
                    .HasMaxLength(256);

                entity.Property(e => e.DefaultLanguage).HasDefaultValueSql("((1))");

                entity.Property(e => e.DemographicQuotaRate).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.Description)
                    .IsRequired()
                    .HasMaxLength(1000);

                entity.Property(e => e.EndDate).HasColumnType("datetime");

                entity.Property(e => e.EstimatedCost).HasColumnType("money");

                entity.Property(e => e.IncidentRate)
                    .HasColumnType("decimal(18, 2)")
                    .HasDefaultValueSql("((0.00))");

                entity.Property(e => e.IsShowPartnerDashboard)
                    .IsRequired()
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(e => e.ReferralUrl)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.ScreeningQuotaM).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.ScreeningQuotaMAverageValue).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.SecretCode)
                    .HasMaxLength(70)
                    .IsUnicode(false);

                entity.Property(e => e.StartDate).HasColumnType("datetime");

                entity.Property(e => e.TargetCountryCode)
                    .IsRequired()
                    .HasMaxLength(4)
                    .IsUnicode(false);

                entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

                entity.Property(e => e.UpdatedBy).HasMaxLength(256);

                entity.HasOne(d => d.ClientProfile)
                    .WithMany(p => p.Commercials)
                    .HasForeignKey(d => d.ClientProfileId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Commercials_ClientProfiles");

                entity.HasOne(d => d.ClientTemplate)
                    .WithMany(p => p.Commercials)
                    .HasForeignKey(d => d.ClientTemplateId)
                    .HasConstraintName("FK_Commercials_ClientSurveyTemplates");

                entity.HasOne(d => d.DefaultLanguageNavigation)
                    .WithMany(p => p.Commercials)
                    .HasForeignKey(d => d.DefaultLanguage)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Languages_Id");

                entity.HasOne(d => d.SurveyCampaign)
                    .WithMany(p => p.Commercials)
                    .HasForeignKey(d => d.SurveyCampaignId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Commercials_SurveyCampaigns");

                entity.HasOne(d => d.Topic)
                    .WithMany(p => p.Commercials)
                    .HasForeignKey(d => d.TopicId)
                    .HasConstraintName("FK_Commercials_Topics");
            });

            modelBuilder.Entity<CommercialsLanguages>(entity =>
            {
                entity.HasOne(d => d.Commercial)
                    .WithMany(p => p.CommercialsLanguages)
                    .HasForeignKey(d => d.CommercialId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Commercial_Id");

                entity.HasOne(d => d.Language)
                    .WithMany(p => p.CommercialsLanguages)
                    .HasForeignKey(d => d.LanguageId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Language_Id");
            });

            modelBuilder.Entity<Countries>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.CodeNumber)
                    .IsRequired()
                    .HasMaxLength(5)
                    .IsUnicode(false);

                entity.Property(e => e.ISO2)
                    .IsRequired()
                    .HasMaxLength(2)
                    .IsUnicode(false);

                entity.Property(e => e.ISO3)
                    .IsRequired()
                    .HasMaxLength(4)
                    .IsUnicode(false);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(200)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<CustomCombinationAnswers>(entity =>
            {
                entity.HasOne(d => d.CustomCombinationFlow)
                    .WithMany(p => p.CustomCombinationAnswers)
                    .HasForeignKey(d => d.CustomCombinationFlowId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_CustomCombinationAnswers_CustomCombinationLogicFlows");
            });

            modelBuilder.Entity<CustomCombinationLogicFlows>(entity =>
            {
                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(1000);
            });

            modelBuilder.Entity<DeletedMemberPsychographics>(entity =>
            {
                entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            });

            modelBuilder.Entity<DeletedSurveyResponseAnswers>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.OtherAnswer).HasMaxLength(255);
            });

            modelBuilder.Entity<DeletedSurveyResponses>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.Host)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.IpAddress)
                    .HasMaxLength(45)
                    .IsUnicode(false);

                entity.Property(e => e.MemberDemographic)
                    .HasMaxLength(2000)
                    .IsUnicode(false);

                entity.Property(e => e.Origin)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.PartnerCode).HasMaxLength(36);

                entity.Property(e => e.PointsCollected).HasDefaultValueSql("((1))");

                entity.Property(e => e.Referer)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.TierNo).HasDefaultValueSql("((1))");

                entity.Property(e => e.UserAgent)
                    .HasMaxLength(500)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<DemographicTypes>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Description)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Name)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ResourceString)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.TargetAverageRate).HasColumnType("decimal(18, 2)");
            });

            modelBuilder.Entity<DemographicValues>(entity =>
            {
                entity.Property(e => e.CountryCode).HasMaxLength(4);

                entity.Property(e => e.DisplayValue).HasMaxLength(100);

                entity.Property(e => e.LastUpdatedAt).HasColumnType("datetime");

                entity.Property(e => e.LastUpdatedBy)
                    .HasMaxLength(128)
                    .IsUnicode(false);

                entity.Property(e => e.TargetRate).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.TargetWeight).HasColumnType("decimal(5, 2)");

                entity.Property(e => e.Value1).HasMaxLength(100);

                entity.Property(e => e.Value2).HasMaxLength(100);

                entity.HasOne(d => d.DemographicType)
                    .WithMany(p => p.DemographicValues)
                    .HasForeignKey(d => d.DemographicTypeId)
                    .HasConstraintName("FK_DemographicValues_DemographicTypes");
            });

            modelBuilder.Entity<Devices>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.CityName)
                    .IsRequired()
                    .HasMaxLength(128)
                    .IsUnicode(false);

                entity.Property(e => e.CountryCode)
                    .IsRequired()
                    .HasMaxLength(2)
                    .IsUnicode(false);

                entity.Property(e => e.CountryName)
                    .IsRequired()
                    .HasMaxLength(64)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.IpAddress)
                    .HasMaxLength(39)
                    .IsUnicode(false);

                entity.Property(e => e.LastUpdatedAt).HasColumnType("datetime");

                entity.Property(e => e.Latitude).HasColumnType("decimal(10, 8)");

                entity.Property(e => e.Longitude).HasColumnType("decimal(11, 8)");

                entity.Property(e => e.RegionName)
                    .IsRequired()
                    .HasMaxLength(128)
                    .IsUnicode(false);

                entity.Property(e => e.UserAgent).IsUnicode(false);

                entity.Property(e => e.ZipCode)
                    .IsRequired()
                    .HasMaxLength(30)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<Districts>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Code)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Name).HasMaxLength(200);

                entity.HasOne(d => d.State)
                    .WithMany(p => p.Districts)
                    .HasForeignKey(d => d.StateId)
                    .HasConstraintName("FK_Districts_States");
            });

            modelBuilder.Entity<ErrorLogs>(entity =>
            {
                entity.HasIndex(e => e.CreatedAt);

                entity.HasIndex(e => e.TypeId);

                entity.Property(e => e.ActionName)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.Email)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.Errors).IsRequired();

                entity.Property(e => e.Token)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.UserName)
                    .HasMaxLength(255)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<FingerPrintTests>(entity =>
            {
                entity.Property(e => e.CityName)
                    .IsRequired()
                    .HasMaxLength(128)
                    .IsUnicode(false);

                entity.Property(e => e.ComponentsJSON).IsUnicode(false);

                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.FingerPrintId)
                    .HasMaxLength(64)
                    .IsUnicode(false);

                entity.Property(e => e.IpAddress)
                    .IsRequired()
                    .HasMaxLength(64)
                    .IsUnicode(false);

                entity.Property(e => e.LastCCAt).HasColumnType("datetime");

                entity.Property(e => e.LastUpdatedAt).HasColumnType("datetime");

                entity.Property(e => e.Latitude).HasColumnType("decimal(10, 8)");

                entity.Property(e => e.Longitude).HasColumnType("decimal(11, 8)");

                entity.Property(e => e.Postcode)
                    .IsRequired()
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.RegionName)
                    .IsRequired()
                    .HasMaxLength(128)
                    .IsUnicode(false);

                entity.Property(e => e.Token)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.UserAgent).IsUnicode(false);
            });

            modelBuilder.Entity<FingerprintIPLogs>(entity =>
            {
                entity.HasIndex(e => e.DeviceId);

                entity.HasIndex(e => e.VisitorId);

                entity.Property(e => e.CityName)
                    .IsRequired()
                    .HasMaxLength(128)
                    .IsUnicode(false);

                entity.Property(e => e.CountryCode)
                    .IsRequired()
                    .HasMaxLength(2)
                    .IsUnicode(false);

                entity.Property(e => e.CountryName)
                    .IsRequired()
                    .HasMaxLength(64)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.IpAddress)
                    .IsRequired()
                    .HasMaxLength(39)
                    .IsUnicode(false);

                entity.Property(e => e.LastCCAt).HasColumnType("datetime");

                entity.Property(e => e.Latitude).HasColumnType("decimal(10, 8)");

                entity.Property(e => e.Longitude).HasColumnType("decimal(11, 8)");

                entity.Property(e => e.RegionName)
                    .IsRequired()
                    .HasMaxLength(128)
                    .IsUnicode(false);

                entity.Property(e => e.UserAgent).IsUnicode(false);

                entity.Property(e => e.VisitorId)
                    .IsRequired()
                    .HasMaxLength(32)
                    .IsUnicode(false);

                entity.Property(e => e.ZipCode)
                    .IsRequired()
                    .HasMaxLength(30)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<Fingerprints>(entity =>
            {
                entity.HasIndex(e => e.DeviceId);

                entity.HasIndex(e => e.MemberProfileId);

                entity.HasIndex(e => e.VisitorId);

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.ComponentsJSON).IsUnicode(false);

                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.LastCCAt).HasColumnType("datetime");

                entity.Property(e => e.LastUpdatedAt).HasColumnType("datetime");

                entity.Property(e => e.Token)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.VisitorId)
                    .IsRequired()
                    .HasMaxLength(32)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<HUTSurveyForms>(entity =>
            {
                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.CreatedBy).HasMaxLength(256);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(256);

                entity.Property(e => e.URL)
                    .IsRequired()
                    .HasMaxLength(256);

                entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

                entity.Property(e => e.UpdatedBy).HasMaxLength(256);

                entity.HasOne(d => d.HUTSurveyProject)
                    .WithMany(p => p.HUTSurveyForms)
                    .HasForeignKey(d => d.HUTSurveyProjectId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_SurveyForm_HUTSurveyProject_Id");
            });

            modelBuilder.Entity<HUTSurveyParticipants>(entity =>
            {
                entity.Property(e => e.Address).IsRequired();

                entity.Property(e => e.City)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.CreatedBy).HasMaxLength(256);

                entity.Property(e => e.Demographics).IsRequired();

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(256);

                entity.Property(e => e.IsQualified)
                    .IsRequired()
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.ListTypeId).HasDefaultValueSql("((1))");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(256);

                entity.Property(e => e.PhoneNumber)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.Postcode)
                    .HasMaxLength(15)
                    .IsUnicode(false);

                entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

                entity.Property(e => e.UpdatedBy).HasMaxLength(256);

                entity.HasOne(d => d.HUTSurveyProject)
                    .WithMany(p => p.HUTSurveyParticipants)
                    .HasForeignKey(d => d.HUTSurveyProjectId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_HUTSurveyProject_Id");

                entity.HasOne(d => d.StatusType)
                    .WithMany(p => p.HUTSurveyParticipants)
                    .HasForeignKey(d => d.StatusTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_StatusType_Id");
            });

            modelBuilder.Entity<HUTSurveyParticipantsStatusTypes>(entity =>
            {
                entity.Property(e => e.Description).HasMaxLength(50);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<HUTSurveyProjects>(entity =>
            {
                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.CreatedBy).HasMaxLength(256);

                entity.Property(e => e.EndDate).HasColumnType("datetime");

                entity.Property(e => e.ExternalName).HasMaxLength(255);

                entity.Property(e => e.IsRandomized)
                    .IsRequired()
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.LanguageId).HasDefaultValueSql("((1))");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(e => e.StartDate).HasColumnType("datetime");

                entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

                entity.Property(e => e.UpdatedBy).HasMaxLength(256);

                entity.Property(e => e.VPointsReward)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.HasOne(d => d.Language)
                    .WithMany(p => p.HUTSurveyProjects)
                    .HasForeignKey(d => d.LanguageId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_HUTSurveyProjects_Languages");
            });

            modelBuilder.Entity<HUTSurveyResponses>(entity =>
            {
                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.HasOne(d => d.HUTSurveyForm)
                    .WithMany(p => p.HUTSurveyResponses)
                    .HasForeignKey(d => d.HUTSurveyFormId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_HUTSurveyForm_Id");

                entity.HasOne(d => d.Participant)
                    .WithMany(p => p.HUTSurveyResponses)
                    .HasForeignKey(d => d.ParticipantId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_HUTSurveyResponses_HUTSurveyParticipants");
            });

            modelBuilder.Entity<IP2Locations>(entity =>
            {
                entity.HasKey(e => e.IPFrom);

                entity.HasIndex(e => e.IPFrom)
                    .HasName("IX_IP2Locations");

                entity.Property(e => e.IPFrom).ValueGeneratedNever();

                entity.Property(e => e.CountryCode)
                    .IsRequired()
                    .HasMaxLength(2)
                    .IsUnicode(false);

                entity.Property(e => e.CountryName)
                    .IsRequired()
                    .HasMaxLength(100);
            });

            modelBuilder.Entity<IPLookups>(entity =>
            {
                entity.HasIndex(e => e.IPFrom);

                entity.HasIndex(e => e.IPTo);

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.CityName)
                    .IsRequired()
                    .HasMaxLength(128)
                    .IsUnicode(false);

                entity.Property(e => e.CountryCode)
                    .IsRequired()
                    .HasMaxLength(2)
                    .IsUnicode(false);

                entity.Property(e => e.CountryName)
                    .IsRequired()
                    .HasMaxLength(64)
                    .IsUnicode(false);

                entity.Property(e => e.Latitude).HasColumnType("decimal(10, 8)");

                entity.Property(e => e.Longitude).HasColumnType("decimal(11, 8)");

                entity.Property(e => e.RegionName)
                    .IsRequired()
                    .HasMaxLength(128)
                    .IsUnicode(false);

                entity.Property(e => e.TimeZone)
                    .IsRequired()
                    .HasMaxLength(8)
                    .IsUnicode(false);

                entity.Property(e => e.ZipCode)
                    .IsRequired()
                    .HasMaxLength(30)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<InprogressR>(entity =>
            {
                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.RecordValue).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.SubgroupName)
                    .IsRequired()
                    .HasMaxLength(255)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<InvalidDomainRequests>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.Domain)
                    .IsRequired()
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.LastUpdatedAt).HasColumnType("datetime");
            });

            modelBuilder.Entity<InvoiceItems>(entity =>
            {
                entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.ItemDescription).IsRequired();

                entity.Property(e => e.RateAmount).HasColumnType("decimal(18, 2)");

                entity.HasOne(d => d.Invoice)
                    .WithMany(p => p.InvoiceItems)
                    .HasForeignKey(d => d.InvoiceId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_InvoiceItems_Invoices");
            });

            modelBuilder.Entity<Invoices>(entity =>
            {
                entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.CreatedBy)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(e => e.CurrencyCode)
                    .IsRequired()
                    .HasMaxLength(3)
                    .IsUnicode(false);

                entity.Property(e => e.DepositRequiredPercentage).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.FileUrl).HasMaxLength(255);

                entity.Property(e => e.FinalAmount).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.FinalPaymentDiscountPercentage).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.PromoCodeDescription)
                    .HasMaxLength(500)
                    .IsUnicode(false);

                entity.Property(e => e.ToAddressLine1)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(e => e.ToAddressLine2).HasMaxLength(255);

                entity.Property(e => e.ToCompanyName)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(e => e.ToCountry)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(e => e.ToPostalCode)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(e => e.ToState)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

                entity.Property(e => e.UpdatedBy).HasMaxLength(255);

                entity.HasOne(d => d.Commercial)
                    .WithMany(p => p.Invoices)
                    .HasForeignKey(d => d.CommercialId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Invoices_Commercials");

                entity.HasOne(d => d.PromoCode)
                    .WithMany(p => p.Invoices)
                    .HasForeignKey(d => d.PromoCodeId)
                    .HasConstraintName("FK_Invoices_PromoCodes");
            });

            modelBuilder.Entity<Ip2Nations>(entity =>
            {
                entity.HasKey(e => e.Ip);

                entity.Property(e => e.Ip).ValueGeneratedNever();

                entity.Property(e => e.CountryCode)
                    .IsRequired()
                    .HasMaxLength(4)
                    .IsUnicode(false);

                entity.HasOne(d => d.CountryCodeNavigation)
                    .WithMany(p => p.Ip2Nations)
                    .HasForeignKey(d => d.CountryCode)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Ip2Nations_Ip2nationCountries");
            });

            modelBuilder.Entity<Ip2nationCountries>(entity =>
            {
                entity.HasKey(e => e.Code);

                entity.Property(e => e.Code)
                    .HasMaxLength(4)
                    .IsUnicode(false);

                entity.Property(e => e.Country)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(e => e.CurrencyCode)
                    .HasMaxLength(4)
                    .IsUnicode(false);

                entity.Property(e => e.IsoCode2)
                    .IsRequired()
                    .HasMaxLength(2)
                    .IsUnicode(false);

                entity.Property(e => e.IsoCode3)
                    .IsRequired()
                    .HasMaxLength(3)
                    .IsUnicode(false);

                entity.Property(e => e.IsoCountry)
                    .IsRequired()
                    .HasMaxLength(255);
            });

            modelBuilder.Entity<Languages>(entity =>
            {
                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.CreatedBy)
                    .IsRequired()
                    .HasMaxLength(128)
                    .IsUnicode(false);

                entity.Property(e => e.IsActive)
                    .IsRequired()
                    .HasDefaultValueSql("('0')");

                entity.Property(e => e.IsDefault)
                    .IsRequired()
                    .HasDefaultValueSql("('0')");

                entity.Property(e => e.LanguageCode)
                    .IsRequired()
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.LanguageDisplayName)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<LoggedOutTokens>(entity =>
            {
                entity.HasKey(e => e.Token);

                entity.Property(e => e.Token)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            });

            modelBuilder.Entity<MasterMemberProfiles>(entity =>
            {
                entity.HasIndex(e => e.MemberProfileId)
                    .HasName("nci_wi_MasterMemberProfiles_B59AE24CA117687C1D093B77C5B9534C");

                entity.Property(e => e.AddressLine1)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.AddressLine2)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.City)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.DateOfBirth).HasColumnType("datetime");

                entity.Property(e => e.MobileCountryCode)
                    .HasMaxLength(5)
                    .IsUnicode(false);

                entity.Property(e => e.MobileNumber)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                //Mobile Number not verified - 1754 
                entity.Property(e => e.MobileVerified)
                          .HasMaxLength(50)
                          .HasDefaultValue('N');

                //Last generated OTP
                entity.Property(e => e.LastGenOtp)
                   .HasMaxLength(50)
                   .HasDefaultValue('N');

                //OTP Generated date

                entity.Property(e => e.LastGenDateTime)
               .HasMaxLength(50);
           

                entity.Property(e => e.Postcode)
                    .HasMaxLength(15)
                    .IsUnicode(false);

                entity.Property(e => e.PreferLanguage).HasMaxLength(5);

                entity.Property(e => e.State)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.UserId)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.HasOne(d => d.Country)
                    .WithMany(p => p.MasterMemberProfiles)
                    .HasForeignKey(d => d.CountryId)
                    .HasConstraintName("FK_MasterMemberProfiles_Countries");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.MasterMemberProfiles)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_MasterMemberProfiles_Users");
            });

            modelBuilder.Entity<MasterMemberShippingAddress>(entity =>
            {
                entity.Property(e => e.AddressLine1)
                    .IsRequired()
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.AddressLine2)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.City)
                    .IsRequired()
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.FirstName)
                    .IsRequired()
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.LastName)
                    .IsRequired()
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.Postcode)
                    .IsRequired()
                    .HasMaxLength(15)
                    .IsUnicode(false);

                entity.Property(e => e.State)
                    .IsRequired()
                    .HasMaxLength(255)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<MemberProfileCatalog>(entity =>
            {
                entity.HasKey(e => e.Guid);

                entity.Property(e => e.Guid).HasMaxLength(128);
            });

            modelBuilder.Entity<MemberProfileChunks>(entity =>
            {
                entity.HasIndex(e => e.MemberProfileId)
                    .HasName("nci_wi_MemberProfileChunks_B59AE24CA117687C1D093B77C5B9534C");

                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.LastUpdatedat).HasColumnType("datetime");
            });

            modelBuilder.Entity<MemberProfileDailyAllowances>(entity =>
            {
                entity.HasIndex(e => e.DayOfTheYear)
                    .HasName("IX_MemberProfileDailyAllowances");

                entity.HasIndex(e => new { e.DayOfTheYear, e.MemberProfileId, e.PartnerWebsiteId })
                    .HasName("nci_wi_MemberProfileDailyAllowances_5FB2601B33E9A25CB2D404916C59C505");

                entity.Property(e => e.LastUpdatedAt).HasColumnType("datetime");

                entity.Property(e => e.VisitDate).HasColumnType("date");
            });

            modelBuilder.Entity<MemberProfileEndedCommercials>(entity =>
            {
                entity.HasIndex(e => e.CommercialId)
                    .HasName("IX_MemberProfileEndedCommercials_1");

                entity.HasIndex(e => e.MemberProfileId)
                    .HasName("IX_MemberProfileEndedCommercials_MemberprofileId");

                entity.Property(e => e.EndedAt).HasColumnType("datetime");

                entity.HasOne(d => d.Commercial)
                    .WithMany(p => p.MemberProfileEndedCommercials)
                    .HasForeignKey(d => d.CommercialId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_MemberProfileEndedCommercials_Commercials");
            });

            modelBuilder.Entity<MemberProfileExtensions>(entity =>
            {
                entity.HasKey(e => new { e.MemberProfileId, e.DemographicTypeId, e.DemographicValueId });

                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.Value)
                    .IsRequired()
                    .HasMaxLength(100);
            });

            modelBuilder.Entity<MemberProfileProductAdImpression>(entity =>
            {
                entity.HasIndex(e => e.MemberProfileId);

                entity.HasIndex(e => e.ProductId)
                    .HasName("IX_MemberProfileProductAdImpression_ProducId");

                entity.Property(e => e.CTR).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            });

            modelBuilder.Entity<MemberProfileProductAdImpressionExtensions>(entity =>
            {
                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.HasOne(d => d.MemberProfileProductAdImpression)
                    .WithMany(p => p.MemberProfileProductAdImpressionExtensions)
                    .HasForeignKey(d => d.MemberProfileProductAdImpressionId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_MemberProfileProductAdImpressionExtensions_MemberProfileProductAdImpression");
            });

            modelBuilder.Entity<MemberProfileSubgroups>(entity =>
            {
                entity.HasKey(e => new { e.SubgroupId, e.MemberProfileId });

                entity.HasOne(d => d.Subgroup)
                    .WithMany(p => p.MemberProfileSubgroups)
                    .HasForeignKey(d => d.SubgroupId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_MemberProfileSubgroups_Subgroups");
            });

            modelBuilder.Entity<MemberProfileVisits>(entity =>
            {
                entity.HasIndex(e => new { e.MemberProfileId, e.PartnerWebsiteId })
                    .HasName("nci_wi_MemberProfileVisits_906E434E29ECEC58B6DE1D1561F14E39");

                entity.Property(e => e.CommercialCloseLastUpdatedAt).HasColumnType("datetime");

                entity.Property(e => e.LastUpdatedAt).HasColumnType("datetime");
            });

            modelBuilder.Entity<MemberProfiles>(entity =>
            {
                entity.HasIndex(e => e.MasterMemberProfileId)
                    .HasName("IX_MemberProfiles_MasterMemberProfiles");

                entity.Property(e => e.AddressLine1).HasMaxLength(50);

                entity.Property(e => e.AddressLine2).HasMaxLength(50);

                entity.Property(e => e.AddressLine3).HasMaxLength(50);

                entity.Property(e => e.City).HasMaxLength(50);



                entity.Property(e => e.CloseClickLastUpdatedAt).HasColumnType("datetime");

                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.CreatedAtCountryCode)
                    .HasMaxLength(4)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedAtIP)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.DateOfBirth).HasColumnType("datetime");

                entity.Property(e => e.DemographicLastUpdatedAt).HasColumnType("datetime");

                entity.Property(e => e.Email).HasMaxLength(255);

                entity.Property(e => e.Guid)
                    .IsRequired()
                    .HasMaxLength(36);

                entity.Property(e => e.IsMasterProfile)
                    .IsRequired()
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.LastCommercialRequestedAt).HasColumnType("datetime");

                entity.Property(e => e.LastRespondedAt).HasColumnType("datetime");

                entity.Property(e => e.Postcode).HasMaxLength(10);

                entity.Property(e => e.ResponseCount).HasDefaultValueSql("((1))");

                entity.Property(e => e.SyncMemberProfileToken)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.HasOne(d => d.Area)
                    .WithMany(p => p.MemberProfiles)
                    .HasForeignKey(d => d.AreaId)
                    .HasConstraintName("FK_MemberProfilesV3_Areas");

                entity.HasOne(d => d.Country)
                    .WithMany(p => p.MemberProfiles)
                    .HasForeignKey(d => d.CountryId)
                    .HasConstraintName("FK_MemberProfilesV3_Countries");

                entity.HasOne(d => d.District)
                    .WithMany(p => p.MemberProfiles)
                    .HasForeignKey(d => d.DistrictId)
                    .HasConstraintName("FK_MemberProfilesV3_Districts");

                entity.HasOne(d => d.State)
                    .WithMany(p => p.MemberProfiles)
                    .HasForeignKey(d => d.StateId)
                    .HasConstraintName("FK_MemberProfilesV3_States");
            });

            modelBuilder.Entity<MemberProfilesOld>(entity =>
            {
                entity.HasIndex(e => e.DemographicLastUpdatedAt)
                    .HasName("IX_MemberProfiles");

                entity.HasIndex(e => e.DemographicMonthlyHouseHoldIncomeId)
                    .HasName("IX_MemberProfiles_DemographicHouseholdIncomeId");

                entity.HasIndex(e => e.IsMasterProfile)
                    .HasName("IX_MemberProfiles_IsMasterProfile");

                entity.HasIndex(e => e.LastCommercialRequestedAt)
                    .HasName("IX_MemberProfiles_LastCommercialRequestedAt");

                entity.HasIndex(e => new { e.DemographicAgeId, e.LastCommercialRequestedAt })
                    .HasName("nci_wi_MemberProfiles_C779E79055CDBFA50686B3A5E8A2AB22");

                entity.HasIndex(e => new { e.DemographicGenderId, e.LastCommercialRequestedAt })
                    .HasName("nci_wi_MemberProfiles_68BE437485292C451A11E1B106A8EEBC");

                entity.HasIndex(e => new { e.DemographicStateId, e.LastCommercialRequestedAt })
                    .HasName("nci_wi_MemberProfiles_2BE7F48DA8480F4008069FEB316A0CF6");

                entity.HasIndex(e => new { e.DemographicEducationId, e.DemographicGenderId, e.LastCommercialRequestedAt })
                    .HasName("nci_wi_MemberProfiles_0558336EC832534D7B84CB2ACA933910");

                entity.HasIndex(e => new { e.DemographicAgeId, e.DemographicOccupationId, e.DemographicStateId, e.LastRespondedAt })
                    .HasName("nci_wi_MemberProfiles_639316BE7840124B8F61191E8EC09CBA");

                entity.HasIndex(e => new { e.DemographicEthnicityId, e.DemographicGenderId, e.DemographicOccupationId, e.LastRespondedAt })
                    .HasName("nci_wi_MemberProfiles_684BB1833679B4382B49BF1A0D8E0028");

                entity.HasIndex(e => new { e.DemographicMaritalStatusId, e.DemographicOccupationId, e.DemographicStateId, e.LastRespondedAt })
                    .HasName("nci_wi_MemberProfiles_386816D9B0B07AFE838D4B805DAEB072");

                entity.HasIndex(e => new { e.DemographicMonthlyIncomeId, e.DemographicOccupationId, e.DemographicStateId, e.LastRespondedAt })
                    .HasName("nci_wi_MemberProfiles_F7961EB4EA996FED83130878030FA199");

                entity.HasIndex(e => new { e.AddressLine1, e.AddressLine2, e.AddressLine3, e.AreaId, e.AvailablePoints, e.City, e.CountryId, e.CreatedAt, e.CreatedAtCountryCode, e.CreatedAtPartnerWebsiteId, e.DateOfBirth, e.DemographicAgeId, e.DemographicEducationId, e.DemographicEthnicityId, e.DemographicGenderId, e.DemographicLastUpdatedAt, e.DemographicMaritalStatusId, e.DemographicMonthlyIncomeId, e.DemographicOccupationId, e.DemographicPoints, e.DemographicReligionId, e.DemographicStateId, e.DistrictId, e.Email, e.Guid, e.IsMasterProfile, e.LastRespondedAt, e.Postcode, e.StateId, e.MasterMemberProfileId })
                    .HasName("nci_wi_MemberProfiles_A121FFC6804837EBAE3D926AD4F6F56F");

                entity.Property(e => e.AddressLine1).HasMaxLength(50);

                entity.Property(e => e.AddressLine2).HasMaxLength(50);

                entity.Property(e => e.AddressLine3).HasMaxLength(50);

                entity.Property(e => e.City).HasMaxLength(50);

                entity.Property(e => e.CloseClickLastUpdatedAt).HasColumnType("datetime");

                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.CreatedAtCountryCode)
                    .HasMaxLength(4)
                    .IsUnicode(false);

                entity.Property(e => e.DateOfBirth).HasColumnType("datetime");

                entity.Property(e => e.DemographicLastUpdatedAt).HasColumnType("datetime");

                entity.Property(e => e.Email).HasMaxLength(255);

                entity.Property(e => e.Guid)
                    .IsRequired()
                    .HasMaxLength(36);

                entity.Property(e => e.IsMasterProfile)
                    .IsRequired()
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.LastCommercialRequestedAt).HasColumnType("datetime");

                entity.Property(e => e.LastRespondedAt).HasColumnType("datetime");

                entity.Property(e => e.NewIdToUse).HasDefaultValueSql("((0))");

                entity.Property(e => e.Postcode).HasMaxLength(10);

                entity.Property(e => e.ResponseCount).HasDefaultValueSql("((1))");

                entity.Property(e => e.SyncMemberProfileToken)
                    .HasMaxLength(255)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<MemberProfilesTest>(entity =>
            {
                entity.Property(e => e.AddressLine1).HasMaxLength(50);

                entity.Property(e => e.AddressLine2).HasMaxLength(50);

                entity.Property(e => e.AddressLine3).HasMaxLength(50);

                entity.Property(e => e.City).HasMaxLength(50);

                entity.Property(e => e.CloseClickLastUpdatedAt).HasColumnType("datetime");

                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.CreatedAtCountryCode)
                    .HasMaxLength(4)
                    .IsUnicode(false);

                entity.Property(e => e.DateOfBirth).HasColumnType("datetime");

                entity.Property(e => e.DemographicLastUpdatedAt).HasColumnType("datetime");

                entity.Property(e => e.Email).HasMaxLength(255);

                entity.Property(e => e.Guid)
                    .IsRequired()
                    .HasMaxLength(36);

                entity.Property(e => e.LastCommercialRequestedAt).HasColumnType("datetime");

                entity.Property(e => e.LastRespondedAt).HasColumnType("datetime");

                entity.Property(e => e.Postcode).HasMaxLength(10);

                entity.Property(e => e.SyncMemberProfileToken)
                    .HasMaxLength(255)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<MemberProfilesV2>(entity =>
            {
                entity.Property(e => e.AddressLine1).HasMaxLength(50);

                entity.Property(e => e.AddressLine2).HasMaxLength(50);

                entity.Property(e => e.AddressLine3).HasMaxLength(50);

                entity.Property(e => e.City).HasMaxLength(50);

                entity.Property(e => e.CloseClickLastUpdatedAt).HasColumnType("datetime");

                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.CreatedAtCountryCode)
                    .HasMaxLength(4)
                    .IsUnicode(false);

                entity.Property(e => e.DateOfBirth).HasColumnType("datetime");

                entity.Property(e => e.DemographicLastUpdatedAt).HasColumnType("datetime");

                entity.Property(e => e.Email).HasMaxLength(255);

                entity.Property(e => e.Guid)
                    .IsRequired()
                    .HasMaxLength(36);

                entity.Property(e => e.LastCommercialRequestedAt).HasColumnType("datetime");

                entity.Property(e => e.LastRespondedAt).HasColumnType("datetime");

                entity.Property(e => e.Postcode).HasMaxLength(10);

                entity.Property(e => e.SyncMemberProfileToken)
                    .HasMaxLength(255)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<MemberPsychographicAnswers>(entity =>
            {
                entity.HasIndex(e => new { e.AnswerId, e.IsActive, e.MemberPsyId })
                    .HasName("nci_wi_MemberPsychographicAnswers_B30A06F343AAB4245F9D4CDA516BED3B");

                entity.HasOne(d => d.MemberPsy)
                    .WithMany(p => p.MemberPsychographicAnswers)
                    .HasForeignKey(d => d.MemberPsyId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_MemberPsychographicAnswers_MemberPsychographics");
            });

            modelBuilder.Entity<MemberPsychographics>(entity =>
            {
                entity.HasIndex(e => new { e.MemberProfileId, e.PsyQuestionId })
                    .HasName("nci_wi_MemberPsychographics_E524A844C0A7219A1475FE3ACC91E4A1");

                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.HasOne(d => d.PsyQuestion)
                    .WithMany(p => p.MemberPsychographics)
                    .HasForeignKey(d => d.PsyQuestionId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_MemberPsychographics_PsyQuestions");
            });

            modelBuilder.Entity<MobileDevices>(entity =>
            {
                entity.HasKey(e => e.VodusId);

                entity.HasIndex(e => e.DeviceId);

                entity.HasIndex(e => e.MemberProfileId);

                entity.HasIndex(e => e.OS);

                entity.Property(e => e.VodusId).ValueGeneratedNever();

                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.DeviceId)
                    .IsRequired()
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.LastUpdatedAt).HasColumnType("datetime");

                entity.Property(e => e.Token)
                    .IsRequired()
                    .HasMaxLength(255)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<OrderItemRedemptions>(entity =>
            {
                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.UniqueCode)
                    .HasMaxLength(128)
                    .IsUnicode(false);

                entity.HasOne(d => d.OrderItem)
                    .WithMany(p => p.OrderItemRedemptions)
                    .HasForeignKey(d => d.OrderItemId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_OrderItemRedemptions_OrderItems");
            });

            modelBuilder.Entity<OrderItems>(entity =>
            {
                entity.Property(e => e.ProductName)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(e => e.ProductTypeName)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(e => e.ThirdPartyRedemptionUrl)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.ThirdPartyResponse).HasMaxLength(1000);

                entity.HasOne(d => d.Order)
                    .WithMany(p => p.OrderItems)
                    .HasForeignKey(d => d.OrderId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_OrderItems_Orders");

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.OrderItems)
                    .HasForeignKey(d => d.ProductId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_OrderItems_Products");
            });

            modelBuilder.Entity<OrderRedemptions>(entity =>
            {
                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.UniqueCode)
                    .HasMaxLength(128)
                    .IsUnicode(false);

                entity.HasOne(d => d.OrderItem)
                    .WithMany(p => p.OrderRedemptions)
                    .HasForeignKey(d => d.OrderItemId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_OrderRedemptions_OrderItems");
            });

            modelBuilder.Entity<Orders>(entity =>
            {
                entity.Property(e => e.AddressLine1)
                    .IsRequired()
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.AddressLine2)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.City)
                    .IsRequired()
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.ContactEmail)
                    .IsRequired()
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.ContactMobileCountryCode)
                    .IsRequired()
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.ContactMobileNumber)
                    .IsRequired()
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.ContactPersonFirstName)
                    .IsRequired()
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.ContactPersonLastName)
                    .IsRequired()
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.CountryName)
                    .IsRequired()
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.DeliveryLastUpdated).HasColumnType("datetime");

                entity.Property(e => e.DeliveryLastUpdatedBy)
                    .HasMaxLength(128)
                    .IsUnicode(false);

                entity.Property(e => e.DeliveryRemark)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.Postcode)
                    .IsRequired()
                    .HasMaxLength(15)
                    .IsUnicode(false);

                entity.Property(e => e.State)
                    .IsRequired()
                    .HasMaxLength(255)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<PartnerProfiles>(entity =>
            {
                entity.Property(e => e.UserId)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.HasOne(d => d.User)
                    .WithMany(p => p.PartnerProfiles)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_PartnerProfiles_Users");
            });

            modelBuilder.Entity<PartnerWebsiteExcludedCommercials>(entity =>
            {
                entity.HasIndex(e => new { e.PartnerWebsiteId, e.IsActive, e.CommercialId })
                    .HasName("nci_wi_PartnerWebsiteExcludedCommercials_6DFB617C00AC93AC50BB87F98EB3BAF8");

                entity.HasIndex(e => new { e.CommercialId, e.CreatedAt, e.CreatedBy, e.Description, e.PartnerWebsiteId, e.IsActive })
                    .HasName("nci_wi_PartnerWebsiteExcludedCommercials_5755506325A69BBE6BDAF05AE95465B5");

                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.CreatedBy)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.Description).HasMaxLength(255);

                entity.Property(e => e.IsActive)
                    .IsRequired()
                    .HasDefaultValueSql("((1))");

                entity.HasOne(d => d.Commercial)
                    .WithMany(p => p.PartnerWebsiteExcludedCommercials)
                    .HasForeignKey(d => d.CommercialId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_PartnerWebsiteExcludedCommercials_Commercials");

                entity.HasOne(d => d.PartnerWebsite)
                    .WithMany(p => p.PartnerWebsiteExcludedCommercials)
                    .HasForeignKey(d => d.PartnerWebsiteId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_PartnerWebsiteExcludedCommercials_PartnerWebsites");
            });

            modelBuilder.Entity<PartnerWebsitePriorityCommercials>(entity =>
            {
                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.CreatedBy)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.Description).HasMaxLength(255);

                entity.Property(e => e.IsActive)
                    .IsRequired()
                    .HasDefaultValueSql("((1))");

                entity.HasOne(d => d.Commercial)
                    .WithMany(p => p.PartnerWebsitePriorityCommercials)
                    .HasForeignKey(d => d.CommercialId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_PartnerWebsitePriorityCommercials_Commercials");

                entity.HasOne(d => d.PartnerWebsite)
                    .WithMany(p => p.PartnerWebsitePriorityCommercials)
                    .HasForeignKey(d => d.PartnerWebsiteId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_PartnerWebsitePriorityCommercials_PartnerWebsites");
            });

            modelBuilder.Entity<PartnerWebsiteVisits>(entity =>
            {
                entity.HasIndex(e => new { e.CreatedAt, e.LastUpdatedAt, e.PartnerWebsitesId, e.Type1CloseCount, e.Type1ResponseCount, e.Type2CloseCount, e.Type2ResponseCount, e.VisitCount, e.DemographicValueId })
                    .HasName("nci_wi_PartnerWebsiteVisits_ADD1F2AF42DA1CB5CB130273468D777B");

                entity.HasIndex(e => new { e.LastUpdatedAt, e.PartnerWebsitesId, e.Type1CCCount, e.Type1CloseCount, e.Type1ResponseCount, e.Type2CCCount, e.Type2CloseCount, e.Type2ResponseCount, e.VisitCount, e.DemographicValueId, e.CreatedAt })
                    .HasName("nci_wi_PartnerWebsiteVisits_A6C94DC0FB80D37246BE17D5C014E2F8");

                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.LastUpdatedAt).HasColumnType("datetime");

                entity.HasOne(d => d.DemographicValue)
                    .WithMany(p => p.PartnerWebsiteVisits)
                    .HasForeignKey(d => d.DemographicValueId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_PartnerWebsiteVisits_DemographicValues");

                entity.HasOne(d => d.PartnerWebsites)
                    .WithMany(p => p.PartnerWebsiteVisits)
                    .HasForeignKey(d => d.PartnerWebsitesId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_PartnerWebsiteVisits_PartnerWebsites");
            });

            modelBuilder.Entity<PartnerWebsites>(entity =>
            {
                entity.Property(e => e.BackgroundColor)
                    .HasMaxLength(8)
                    .IsUnicode(false);

                entity.Property(e => e.ButtonColor)
                    .HasMaxLength(8)
                    .IsUnicode(false);

                entity.Property(e => e.CCType).HasDefaultValueSql("((3))");

                entity.Property(e => e.CTCInterval).HasDefaultValueSql("((15))");

                entity.Property(e => e.CTCTimer).HasDefaultValueSql("((7))");

                entity.Property(e => e.CatfishPosition)
                    .IsRequired()
                    .HasMaxLength(36)
                    .HasDefaultValueSql("('bottom-right')");

                entity.Property(e => e.Code).HasMaxLength(36);

                entity.Property(e => e.CommercialEnabled)
                    .IsRequired()
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.CreatedBy)
                    .HasMaxLength(128)
                    .IsUnicode(false);

                entity.Property(e => e.DMPCode)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.DMPTargetAudience).IsUnicode(false);

                entity.Property(e => e.DMPTargetCode)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.DMPUrl)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.DemographicCCType).HasDefaultValueSql("((3))");

                entity.Property(e => e.Description).HasMaxLength(255);

                entity.Property(e => e.FontColor)
                    .HasMaxLength(8)
                    .IsUnicode(false);

                entity.Property(e => e.Interval).HasDefaultValueSql("((360))");

                entity.Property(e => e.Language)
                    .IsRequired()
                    .HasMaxLength(36)
                    .HasDefaultValueSql("('en')");

                entity.Property(e => e.LogoUrl)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.ModalClosable)
                    .IsRequired()
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.Name).HasMaxLength(255);

                entity.Property(e => e.NoDemo).HasDefaultValueSql("((1))");

                entity.Property(e => e.PrimaryColor)
                    .HasMaxLength(8)
                    .IsUnicode(false);

                entity.Property(e => e.STOAfterTotalNoResponse).HasDefaultValueSql("((999))");

                entity.Property(e => e.SecondaryColor)
                    .HasMaxLength(8)
                    .IsUnicode(false);

                entity.Property(e => e.SelectedAnswerColor)
                    .HasMaxLength(8)
                    .IsUnicode(false);

                entity.Property(e => e.Url).HasMaxLength(255);

                entity.Property(e => e.VodusLogoType).HasDefaultValueSql("((1))");

                entity.HasOne(d => d.Partner)
                    .WithMany(p => p.PartnerWebsites)
                    .HasForeignKey(d => d.PartnerId)
                    .HasConstraintName("FK_PartnerWebsites_Partners");
            });

            modelBuilder.Entity<Partners>(entity =>
            {
                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.CreatedBy)
                    .HasMaxLength(128)
                    .IsUnicode(false);

                entity.Property(e => e.DataSyncAccessKey)
                    .HasMaxLength(128)
                    .IsUnicode(false);

                entity.Property(e => e.DataSyncAccessSecret)
                    .HasMaxLength(128)
                    .IsUnicode(false);

                entity.Property(e => e.DataSyncContainerName)
                    .HasMaxLength(128)
                    .IsUnicode(false);

                entity.Property(e => e.DataSyncUrl)
                    .HasMaxLength(128)
                    .IsUnicode(false);

                entity.Property(e => e.LogoUrl)
                    .HasMaxLength(128)
                    .IsUnicode(false);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(255);
            });

            modelBuilder.Entity<PasswordResets>(entity =>
            {
                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(128)
                    .IsUnicode(false);

                entity.Property(e => e.ExpireAt).HasColumnType("datetime");

                entity.Property(e => e.ResetCode)
                    .IsRequired()
                    .HasMaxLength(128)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<PaymentStatuses>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Description).HasMaxLength(50);
            });

            modelBuilder.Entity<Permissions>(entity =>
            {
                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<ProductAdLocations>(entity =>
            {
                entity.Property(e => e.CTR).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.ProvinceName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.HasOne(d => d.ProductAd)
                    .WithMany(p => p.ProductAdLocations)
                    .HasForeignKey(d => d.ProductAdId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ProductAdLocations_ProductAdLocations");
            });

            modelBuilder.Entity<ProductAdPartnersDomain>(entity =>
            {
                entity.Property(e => e.CTR).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.HasOne(d => d.Partner)
                    .WithMany(p => p.ProductAdPartnersDomain)
                    .HasForeignKey(d => d.PartnerId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ProductAdPartnersDomain_Partners");

                entity.HasOne(d => d.ProductAd)
                    .WithMany(p => p.ProductAdPartnersDomain)
                    .HasForeignKey(d => d.ProductAdId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ProductAdPartnersDomain_ProductAds");
            });

            modelBuilder.Entity<ProductAdPartnersDomainWebsites>(entity =>
            {
                entity.Property(e => e.CTR).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.HasOne(d => d.PartnerWebsite)
                    .WithMany(p => p.ProductAdPartnersDomainWebsites)
                    .HasForeignKey(d => d.PartnerWebsiteId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ProductAdPartnersDomainWebsites_PartnerWebsites");

                entity.HasOne(d => d.ProductAdPartnersDomain)
                    .WithMany(p => p.ProductAdPartnersDomainWebsites)
                    .HasForeignKey(d => d.ProductAdPartnersDomainId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ProductAdPartnersDomainWebsites_ProductAdPartnersDomain");
            });

            modelBuilder.Entity<ProductAdSubgroups>(entity =>
            {
                entity.HasIndex(e => e.SubgroupId);

                entity.HasIndex(e => new { e.AdClickCount, e.AdImpressionCount, e.CreatedAt, e.CTR, e.ProductAdId, e.SubgroupId })
                    .HasName("nci_wi_ProductAdSubgroups_4F339C1FC02C8C64537110C65F559026");

                entity.Property(e => e.CTR).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.SubgroupId)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.HasOne(d => d.ProductAd)
                    .WithMany(p => p.ProductAdSubgroups)
                    .HasForeignKey(d => d.ProductAdId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ProductAdSubgroups_ProductAds");

                entity.HasOne(d => d.Subgroup)
                    .WithMany(p => p.ProductAdSubgroups)
                    .HasForeignKey(d => d.SubgroupId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ProductAdSubgroups_SubgroupsV2");
            });

            modelBuilder.Entity<ProductAds>(entity =>
            {
                entity.HasIndex(e => e.IsActive);

                entity.HasIndex(e => e.ProductId);

                entity.Property(e => e.CTR).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            });

            modelBuilder.Entity<ProductAdsConfig>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.LastUpdatedAt).HasColumnType("datetime");

                entity.Property(e => e.VPointsCap).HasDefaultValueSql("((500))");
            });

            modelBuilder.Entity<Products>(entity =>
            {
                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.CreatedBy)
                    .IsRequired()
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.Description).IsRequired();

                entity.Property(e => e.IsActive)
                    .IsRequired()
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.LuckyDrawDate).HasColumnType("datetime");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(1000);

                entity.Property(e => e.ProductImageUrl)
                    .IsRequired()
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.ProductSummary).HasMaxLength(1000);

                entity.Property(e => e.ProductThumbnailImageUrl)
                    .IsRequired()
                    .HasMaxLength(255)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<PromoCodes>(entity =>
            {
                entity.Property(e => e.Code)
                    .IsRequired()
                    .HasMaxLength(128)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.CreatedBy)
                    .IsRequired()
                    .HasMaxLength(128)
                    .IsUnicode(false);

                entity.Property(e => e.DepositRequiredPercentage).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.Description)
                    .IsRequired()
                    .HasMaxLength(500)
                    .IsUnicode(false);

                entity.Property(e => e.EndDate).HasColumnType("date");

                entity.Property(e => e.FinalPaymentDiscountPercentage).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.StartDate).HasColumnType("date");
            });

            modelBuilder.Entity<PsyQuestionAnswers>(entity =>
            {
                entity.Property(e => e.AnswerValue)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.IsActive)
                    .IsRequired()
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.TemplateUrl)
                    .HasMaxLength(200)
                    .IsUnicode(false);

                entity.HasOne(d => d.PsyQuestion)
                    .WithMany(p => p.PsyQuestionAnswers)
                    .HasForeignKey(d => d.PsyQuestionId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_PsyQuestionAnswers_PsyQuestions");
            });

            modelBuilder.Entity<PsyQuestionMultiLanguagesAnswers>(entity =>
            {
                entity.Property(e => e.AnswerValue)
                    .IsRequired()
                    .HasMaxLength(1000);

                entity.Property(e => e.LanguageCode)
                    .IsRequired()
                    .HasMaxLength(2)
                    .IsUnicode(false)
                    .IsFixedLength();
            });

            modelBuilder.Entity<PsyQuestions>(entity =>
            {
                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.CreatedBy)
                    .IsRequired()
                    .HasMaxLength(128)
                    .IsUnicode(false);

                entity.Property(e => e.QuestionTemplateContentMobileUrl).IsUnicode(false);

                entity.Property(e => e.QuestionTemplateContentUrl).IsUnicode(false);

                entity.Property(e => e.Remark).HasMaxLength(128);

                entity.Property(e => e.TemplateContentUrl).IsUnicode(false);

                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasMaxLength(255);
            });

            modelBuilder.Entity<QuestionDelays>(entity =>
            {
                entity.Property(e => e.CostPrice).HasColumnType("money");

                entity.Property(e => e.Point).HasColumnType("decimal(10, 4)");
            });

            modelBuilder.Entity<QuestionTypes>(entity =>
            {
                entity.Property(e => e.CostPrice).HasColumnType("money");

                entity.Property(e => e.Description)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Point).HasColumnType("decimal(10, 4)");

                entity.Property(e => e.Type)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<RequestForDownloads>(entity =>
            {
                entity.Property(e => e.CompanyName)
                    .IsRequired()
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.CreatedBy)
                    .IsRequired()
                    .HasMaxLength(128)
                    .IsUnicode(false);

                entity.Property(e => e.DownloadLink)
                    .IsRequired()
                    .HasMaxLength(128)
                    .IsUnicode(false);

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.HasOne(d => d.Article)
                    .WithMany(p => p.RequestForDownloads)
                    .HasForeignKey(d => d.ArticleId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_RequestForDownloads_Articles");
            });

            modelBuilder.Entity<RoleClaims>(entity =>
            {
                entity.HasIndex(e => e.RoleId);

                entity.Property(e => e.RoleId)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.HasOne(d => d.Role)
                    .WithMany(p => p.RoleClaims)
                    .HasForeignKey(d => d.RoleId);
            });

            modelBuilder.Entity<Roles>(entity =>
            {
                entity.Property(e => e.Id).HasMaxLength(128);

                entity.Property(e => e.ConcurrencyStamp)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(256);

                entity.Property(e => e.NormalizedName)
                    .HasMaxLength(255)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<States>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Code)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Name)
                    .HasMaxLength(200)
                    .IsUnicode(false);

                entity.Property(e => e.Name2)
                    .HasMaxLength(200)
                    .IsUnicode(false);

                entity.HasOne(d => d.Country)
                    .WithMany(p => p.States)
                    .HasForeignKey(d => d.CountryId)
                    .HasConstraintName("FK_States_Countries");
            });

            modelBuilder.Entity<StatusTypes>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Description)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<SubgroupDemographicValues>(entity =>
            {
                entity.HasKey(e => new { e.SubgroupId, e.DemographicValueId })
                    .HasName("PK_SubgroupDemographicValue");

                entity.HasIndex(e => e.DemographicValueId)
                    .HasName("nci_wi_SubgroupDemographicValues_C1A8DE3DCEC88ED6E79F3C2262291011");

                entity.HasOne(d => d.DemographicValue)
                    .WithMany(p => p.SubgroupDemographicValues)
                    .HasForeignKey(d => d.DemographicValueId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_SubgroupDemographicValues_DemographicValues");

                entity.HasOne(d => d.Subgroup)
                    .WithMany(p => p.SubgroupDemographicValues)
                    .HasForeignKey(d => d.SubgroupId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__SubgroupD__Subgr__7F6BDA51");
            });

            modelBuilder.Entity<SubgroupMultiplierDemographics>(entity =>
            {
                entity.HasKey(e => new { e.SubgroupMultiplierId, e.DemographicValueId });

                entity.HasOne(d => d.DemographicValue)
                    .WithMany(p => p.SubgroupMultiplierDemographics)
                    .HasForeignKey(d => d.DemographicValueId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_SubgroupMultiplierDemographics_DemographicValues");

                entity.HasOne(d => d.SubgroupMultiplier)
                    .WithMany(p => p.SubgroupMultiplierDemographics)
                    .HasForeignKey(d => d.SubgroupMultiplierId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_SubgroupMultiplierDemographics_SubgroupMultiplers");
            });

            modelBuilder.Entity<SubgroupMultipliers>(entity =>
            {
                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.LastUpdatedAt).HasColumnType("datetime");
            });

            modelBuilder.Entity<Subgroups>(entity =>
            {
                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.LastUpdatedAt).HasColumnType("datetime");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(64);

                entity.Property(e => e.R).HasColumnType("decimal(18, 3)");

                entity.Property(e => e.RLastUpdatedAt).HasColumnType("datetime");

                entity.Property(e => e.RecordValue).HasColumnType("decimal(18, 2)");
            });

            modelBuilder.Entity<SubgroupsV2>(entity =>
            {
                entity.Property(e => e.Id)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.LastUpdatedAt).HasColumnType("datetime");

                entity.Property(e => e.Name).IsRequired();

                entity.Property(e => e.R).HasColumnType("decimal(18, 3)");

                entity.Property(e => e.RLastUpdatedAt).HasColumnType("datetime");

                entity.Property(e => e.RecordValue).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.SubgroupMembers).HasColumnType("text");
            });

            modelBuilder.Entity<SurveyCampaigns>(entity =>
            {
                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.CreatedBy)
                    .IsRequired()
                    .HasMaxLength(256);

                entity.Property(e => e.IsDeleted).HasDefaultValueSql("((0))");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

                entity.Property(e => e.UpdatedBy).HasMaxLength(256);

                entity.HasOne(d => d.ClientProfile)
                    .WithMany(p => p.SurveyCampaigns)
                    .HasForeignKey(d => d.ClientProfileId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_SurveyCampaigns_ClientProfiles");
            });

            modelBuilder.Entity<SurveyChunkings>(entity =>
            {
                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.CreatedBy)
                    .IsRequired()
                    .HasMaxLength(128)
                    .IsUnicode(false);

                entity.Property(e => e.IsActive)
                    .IsRequired()
                    .HasDefaultValueSql("('0')");

                entity.Property(e => e.IsIntroChunk)
                    .IsRequired()
                    .HasDefaultValueSql("('0')");

                entity.Property(e => e.JsonFileUrl)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.LastUpdatedAt).HasColumnType("datetime");

                entity.Property(e => e.LastUpdatedBy)
                    .IsRequired()
                    .HasMaxLength(128)
                    .IsUnicode(false);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.TotalResponseCost).HasColumnType("decimal(18, 2)");

                entity.HasOne(d => d.Commercial)
                    .WithMany(p => p.SurveyChunkings)
                    .HasForeignKey(d => d.CommercialId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_SurveyChunkings_Commercials");
            });

            modelBuilder.Entity<SurveyLogicFlows>(entity =>
            {
                entity.HasKey(e => new { e.SurveyLogicFlowId, e.SurveyQuestionAnswerId, e.ToSurveyQuestionId })
                    .HasName("PK_SurveyLogicFlows_1");

                entity.Property(e => e.SurveyLogicFlowId).ValueGeneratedOnAdd();

                entity.HasOne(d => d.SurveyQuestionAnswer)
                    .WithMany(p => p.SurveyLogicFlows)
                    .HasForeignKey(d => d.SurveyQuestionAnswerId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_SurveyLogicFlows_SurveyQuestionAnswers");

                entity.HasOne(d => d.ToSurveyQuestion)
                    .WithMany(p => p.SurveyLogicFlows)
                    .HasForeignKey(d => d.ToSurveyQuestionId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_SurveyLogicFlows_SurveyQuestions");
            });

            modelBuilder.Entity<SurveyQuestionAnswers>(entity =>
            {
                entity.HasIndex(e => new { e.AnswerSequenceNumber, e.AnswerValue, e.IsActive, e.Template, e.SurveyQuestionId })
                    .HasName("nci_wi_SurveyQuestionAnswers_178447552413B5125A4B864F013AEB9A");

                entity.Property(e => e.AnswerValue)
                    .IsRequired()
                    .HasMaxLength(1000);

                entity.Property(e => e.RedirectUrl)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.Template)
                    .IsRequired()
                    .HasColumnType("nvarchar(max)");

                entity.HasOne(d => d.SurveyQuestion)
                    .WithMany(p => p.SurveyQuestionAnswers)
                    .HasForeignKey(d => d.SurveyQuestionId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_SurveyQuestionAnswers_SurveyQuestions");
            });

            modelBuilder.Entity<SurveyQuestionChucks>(entity =>
            {
                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.CreatedBy)
                    .IsRequired()
                    .HasMaxLength(128)
                    .IsUnicode(false);

                entity.Property(e => e.IsActive)
                    .IsRequired()
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.LastUpdatedAt).HasColumnType("datetime");

                entity.Property(e => e.LastUpdatedby)
                    .HasMaxLength(128)
                    .IsUnicode(false);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(128);
            });

            modelBuilder.Entity<SurveyQuestionMultiLanguagesAnswers>(entity =>
            {
                entity.Property(e => e.AnswerValue)
                    .IsRequired()
                    .HasMaxLength(1000);

                entity.Property(e => e.LanguageCode)
                    .IsRequired()
                    .HasMaxLength(2)
                    .IsUnicode(false)
                    .IsFixedLength();
            });

            modelBuilder.Entity<SurveyQuestionMultiLanguagesTitles>(entity =>
            {
                entity.Property(e => e.LanguageCode)
                    .IsRequired()
                    .HasMaxLength(2)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.TitleText).IsRequired();

                entity.HasOne(d => d.SurveyQuestion)
                    .WithMany(p => p.SurveyQuestionMultiLanguagesTitles)
                    .HasForeignKey(d => d.SurveyQuestionId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_SurveyQuestionMultiLanguagesTitles_SurveyQuestions");
            });

            modelBuilder.Entity<SurveyQuestionTemplates>(entity =>
            {
                entity.HasKey(e => new { e.SurveyTemplateId, e.QuestionTypeId });

                entity.HasOne(d => d.QuestionType)
                    .WithMany(p => p.SurveyQuestionTemplates)
                    .HasForeignKey(d => d.QuestionTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_SurveyQuestionTemplates_QuestionTypes");

                entity.HasOne(d => d.SurveyTemplate)
                    .WithMany(p => p.SurveyQuestionTemplates)
                    .HasForeignKey(d => d.SurveyTemplateId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_SurveyQuestionTemplates_SurveyTemplates");
            });

            modelBuilder.Entity<SurveyQuestionTiers>(entity =>
            {
                entity.HasIndex(e => e.CommercialId)
                    .HasName("nci_wi_SurveyQuestionTiers_1DF7EA914ED8932317B14098846257ED");

                entity.Property(e => e.CostPrice).HasColumnType("money");
            });

            modelBuilder.Entity<SurveyQuestions>(entity =>
            {
                entity.HasIndex(e => e.CommercialId);

                entity.HasIndex(e => new { e.ChainedPrice, e.CommercialId, e.DemographicTypeId, e.IsChained, e.IsSequence, e.PipeCode, e.PipeFrom, e.Point, e.QuestionDelayId, e.QuestionTemplateContent, e.QuestionTemplateContentMobile, e.QuestionTitle, e.QuestionTypeId, e.QuestionTypePrice, e.TemplateContent, e.ChunkId, e.SurveyQuestionTierId })
                    .HasName("nci_wi_SurveyQuestions_F227B35C2900C89C7E9481DC80AEB132");

                entity.Property(e => e.ChainedPrice).HasColumnType("money");

                entity.Property(e => e.PipeCode)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.Point).HasColumnType("decimal(10, 4)");

                entity.Property(e => e.QuestionTemplateContent)
                    .IsRequired()
                    .HasColumnType("nvarchar(max)");

                entity.Property(e => e.QuestionTemplateContentMobile).HasColumnType("nvarchar(max)");

                entity.Property(e => e.QuestionTitle).HasMaxLength(1000);

                entity.Property(e => e.QuestionTypePrice).HasColumnType("money");

                entity.Property(e => e.TemplateContent)
                    .IsRequired()
                    .HasColumnType("nvarchar(max)");

                entity.HasOne(d => d.Chunk)
                    .WithMany(p => p.SurveyQuestions)
                    .HasForeignKey(d => d.ChunkId)
                    .HasConstraintName("FK_SurveyQuestions_Chunk");

                entity.HasOne(d => d.Commercial)
                    .WithMany(p => p.SurveyQuestions)
                    .HasForeignKey(d => d.CommercialId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_SurveyQuestions_Commercials");

                entity.HasOne(d => d.DemographicType)
                    .WithMany(p => p.SurveyQuestions)
                    .HasForeignKey(d => d.DemographicTypeId)
                    .HasConstraintName("FK_SurveyQuestions_DemographicTypes");

                entity.HasOne(d => d.QuestionDelay)
                    .WithMany(p => p.SurveyQuestions)
                    .HasForeignKey(d => d.QuestionDelayId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_SurveyQuestions_QuestionDelays");

                entity.HasOne(d => d.QuestionType)
                    .WithMany(p => p.SurveyQuestions)
                    .HasForeignKey(d => d.QuestionTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_SurveyQuestions_QuestionTypes");

                entity.HasOne(d => d.SurveyQuestionTier)
                    .WithMany(p => p.SurveyQuestions)
                    .HasForeignKey(d => d.SurveyQuestionTierId)
                    .HasConstraintName("FK_SurveyQuestions_SurveyQuestionTiers");
            });

            modelBuilder.Entity<SurveyResponseAnswers>(entity =>
            {
                entity.HasIndex(e => new { e.BillingId, e.IsEndSurvey, e.Order, e.OtherAnswer, e.SurveyQuestionAnswerId, e.SurveyQuestionId, e.SurveyResponseId })
                    .HasName("nci_wi_SurveyResponseAnswers_56CA1D54B8A03B2064A0AD8246EB8C3B");

                entity.Property(e => e.OtherAnswer).HasMaxLength(255);

                entity.HasOne(d => d.SurveyQuestionAnswer)
                    .WithMany(p => p.SurveyResponseAnswers)
                    .HasForeignKey(d => d.SurveyQuestionAnswerId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_SurveyResponseAnswers_SurveyQuestionAnswers");

                entity.HasOne(d => d.SurveyQuestion)
                    .WithMany(p => p.SurveyResponseAnswers)
                    .HasForeignKey(d => d.SurveyQuestionId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_SurveyResponseAnswers_SurveyQuestions");

                entity.HasOne(d => d.SurveyResponse)
                    .WithMany(p => p.SurveyResponseAnswers)
                    .HasForeignKey(d => d.SurveyResponseId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_SurveyResponseAnswers_SurveyResponses");
            });

            modelBuilder.Entity<SurveyResponseDemographic>(entity =>
            {
                entity.Property(e => e.Value).HasMaxLength(1000);

                entity.HasOne(d => d.DemographicType)
                    .WithMany(p => p.SurveyResponseDemographic)
                    .HasForeignKey(d => d.DemographicTypeId)
                    .HasConstraintName("FK_SurveyResponseDemographic_DemographicTypes");

                entity.HasOne(d => d.SurveyResponse)
                    .WithMany(p => p.SurveyResponseDemographic)
                    .HasForeignKey(d => d.SurveyResponseId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_SurveyResponseDemographic_SurveyResponses");
            });

            modelBuilder.Entity<SurveyResponses>(entity =>
            {
                entity.HasIndex(e => e.CreatedAt);

                entity.HasIndex(e => new { e.CommercialId, e.CreatedAt, e.Host, e.IpAddress, e.IsExceedQuotaResponse, e.SubgroupId, e.SurveyQuestionId, e.TierNo, e.UserAgent, e.IsFailedScreening, e.Origin, e.PartnerCode, e.PointsCollected, e.Referer, e.RespondTimeInSeconds, e.MemberProfileId })
                    .HasName("nci_wi_SurveyResponses_4C0816D67AFCDA879CFA74117112E154");

                entity.HasIndex(e => new { e.CreatedAt, e.Host, e.IpAddress, e.IsExceedQuotaResponse, e.SubgroupId, e.SurveyQuestionId, e.TierNo, e.UserAgent, e.IsFailedScreening, e.Origin, e.PartnerCode, e.PointsCollected, e.Referer, e.RespondTimeInSeconds, e.CommercialId, e.MemberProfileId })
                    .HasName("nci_wi_SurveyResponses_144CD50FDC78EEC2581B1AA69101F068");

                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.Host)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.IpAddress)
                    .HasMaxLength(45)
                    .IsUnicode(false);

                entity.Property(e => e.MemberDemographic)
                    .HasMaxLength(2000)
                    .IsUnicode(false);

                entity.Property(e => e.Origin)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.PartnerCode).HasMaxLength(36);

                entity.Property(e => e.PointsCollected).HasDefaultValueSql("((1))");

                entity.Property(e => e.Referer)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.TierNo).HasDefaultValueSql("((1))");

                entity.Property(e => e.UserAgent)
                    .HasMaxLength(500)
                    .IsUnicode(false);

                entity.HasOne(d => d.Commercial)
                    .WithMany(p => p.SurveyResponses)
                    .HasForeignKey(d => d.CommercialId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_SurveyResponses_Commercials");
            });

            modelBuilder.Entity<SurveyTemplates>(entity =>
            {
                entity.Property(e => e.Description)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.TemplateName)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<SystemConfigurations>(entity =>
            {
                entity.HasKey(e => e.Code);

                entity.Property(e => e.Code)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Description)
                    .HasMaxLength(255)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<TempTokens>(entity =>
            {
                entity.HasIndex(e => e.MemberProfileId);

                entity.HasIndex(e => e.Token);

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.LastUpdatedAt).HasColumnType("datetime");

                entity.Property(e => e.Token)
                    .HasMaxLength(255)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<Temp_CommercialFilters>(entity =>
            {
                entity.HasKey(e => new { e.CommercialId, e.SequenceNumber });

                entity.HasOne(d => d.Commercial)
                    .WithMany(p => p.Temp_CommercialFilters)
                    .HasForeignKey(d => d.CommercialId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Temp_CommercialFilters_Commercials");

                entity.HasOne(d => d.DemographicType)
                    .WithMany(p => p.Temp_CommercialFilters)
                    .HasForeignKey(d => d.DemographicTypeId)
                    .HasConstraintName("FK_Temp_CommercialFilters_DemographicTypes");
            });

            modelBuilder.Entity<Temp_CommercialWeightedScoreDemographics>(entity =>
            {
                entity.HasKey(e => new { e.CommercialId, e.SequenceNumber, e.SubSequenceNumber, e.DemographicTypeId });

                entity.Property(e => e.Value).HasMaxLength(1000);

                entity.Property(e => e.Value2).HasMaxLength(1000);

                entity.HasOne(d => d.Commercial)
                    .WithMany(p => p.Temp_CommercialWeightedScoreDemographics)
                    .HasForeignKey(d => d.CommercialId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Temp_CommercialWeightedScoreDemographics_Commercials");

                entity.HasOne(d => d.DemographicType)
                    .WithMany(p => p.Temp_CommercialWeightedScoreDemographics)
                    .HasForeignKey(d => d.DemographicTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Temp_CommercialWeightedScoreDemographics_DemographicTypes");

                entity.HasOne(d => d.DemographicValue)
                    .WithMany(p => p.Temp_CommercialWeightedScoreDemographics)
                    .HasForeignKey(d => d.DemographicValueId)
                    .HasConstraintName("FK_Temp_CommercialWeightedScoreDemographics_DemographicValues");
            });

            modelBuilder.Entity<Temp_CommercialWeightedScores>(entity =>
            {
                entity.HasKey(e => new { e.CommercialId, e.SequenceNumber, e.SubSequenceNumber });

                entity.Property(e => e.QuotaMultiplierCompletedCount).HasDefaultValueSql("((0))");

                entity.Property(e => e.QuotaMultiplierIsCompleted).HasDefaultValueSql("((0))");

                entity.Property(e => e.QuotaMultiplierVolume).HasDefaultValueSql("((0))");

                entity.Property(e => e.QuotaVolumeCompletedCount).HasDefaultValueSql("((0))");

                entity.Property(e => e.QuotaVolumeIsCompleted).HasDefaultValueSql("((0))");

                entity.HasOne(d => d.Commercial)
                    .WithMany(p => p.Temp_CommercialWeightedScores)
                    .HasForeignKey(d => d.CommercialId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Temp_CommercialWeightedScores_Commercials");
            });

            modelBuilder.Entity<Topics>(entity =>
            {
                entity.Property(e => e.Name).HasMaxLength(50);
            });

            modelBuilder.Entity<TreatedDataJsonFiles>(entity =>
            {
                entity.Property(e => e.JsonFileName)
                    .HasMaxLength(128)
                    .IsUnicode(false);

                entity.Property(e => e.JsonFileUrl)
                    .HasMaxLength(128)
                    .IsUnicode(false);

                entity.Property(e => e.UploadDate).HasColumnType("datetime");

                entity.HasOne(d => d.Commercial)
                    .WithMany(p => p.TreatedDataJsonFiles)
                    .HasForeignKey(d => d.CommercialId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_TreatedDataJsonFiles_Commercials");
            });

            modelBuilder.Entity<UserClaims>(entity =>
            {
                entity.Property(e => e.UserId)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.HasOne(d => d.User)
                    .WithMany(p => p.UserClaims)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK_dbo.UserClaims_dbo.Users_UserId");
            });

            modelBuilder.Entity<UserLogins>(entity =>
            {
                entity.HasKey(e => new { e.LoginProvider, e.ProviderKey, e.UserId })
                    .HasName("PK_dbo.UserLogins");

                entity.Property(e => e.LoginProvider).HasMaxLength(128);

                entity.Property(e => e.ProviderKey).HasMaxLength(128);

                entity.Property(e => e.UserId).HasMaxLength(128);

                entity.Property(e => e.ProviderDisplayName)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.HasOne(d => d.User)
                    .WithMany(p => p.UserLogins)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK_dbo.UserLogins_dbo.Users_UserId");
            });

            modelBuilder.Entity<UserRoles>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.RoleId })
                    .HasName("PK_dbo.UserRoles");

                entity.Property(e => e.UserId).HasMaxLength(128);

                entity.Property(e => e.RoleId).HasMaxLength(128);

                entity.HasOne(d => d.Role)
                    .WithMany(p => p.UserRoles)
                    .HasForeignKey(d => d.RoleId)
                    .HasConstraintName("FK_dbo.UserRoles_dbo.Roles_RoleId");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.UserRoles)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK_dbo.UserRoles_dbo.Users_UserId");
            });

            modelBuilder.Entity<UserTokens>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.LoginProvider, e.Name });
            });

            modelBuilder.Entity<Users>(entity =>
            {
                entity.HasIndex(e => e.Email)
                    .HasName("nci_wi_Users_FAD9A4DCB2C2226FB45CF38DA1713AD0");

                entity.HasIndex(e => e.UserName)
                    .HasName("nci_wi_Users_D3BEA568FB285FE4954473DEC1A7D1C1");

                entity.Property(e => e.Id).HasMaxLength(128);

                entity.Property(e => e.ActivatedAt).HasColumnType("datetime");

                entity.Property(e => e.ActivationCode).HasMaxLength(36);

                entity.Property(e => e.AuthType).HasMaxLength(20);

                entity.Property(e => e.ConcurrencyStamp)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(256);

                entity.Property(e => e.FirstName).HasMaxLength(100);

                entity.Property(e => e.LastCheckedAt).HasColumnType("datetime");

                entity.Property(e => e.LastLoggedInAt).HasColumnType("datetime");

                entity.Property(e => e.LastName).HasMaxLength(100);

                entity.Property(e => e.LockoutEnd).HasColumnType("datetime");

                entity.Property(e => e.LockoutEndDateUtc).HasColumnType("datetime");

                entity.Property(e => e.NewPendingVerificationEmail)
                    .HasMaxLength(256)
                    .IsUnicode(false);

                entity.Property(e => e.NormalizedEmail)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.NormalizedUserName)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.SocialId).HasMaxLength(128);

                entity.Property(e => e.UserName)
                    .IsRequired()
                    .HasMaxLength(256);
            });

            modelBuilder.Entity<Variables>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.AddressLine1).HasMaxLength(255);

                entity.Property(e => e.AddressLine2).HasMaxLength(255);

                entity.Property(e => e.CCSortingType).HasDefaultValueSql("((1))");

                entity.Property(e => e.CSVFilterEnabled)
                    .IsRequired()
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.City).HasMaxLength(255);

                entity.Property(e => e.CommercialCallingIsActive)
                    .IsRequired()
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.CommercialCallingMaximumIntervalMinutes).HasDefaultValueSql("((60))");

                entity.Property(e => e.CommercialDepositRequiredPercentage).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.CommercialPendingToLiveDays).HasDefaultValueSql("((1))");

                entity.Property(e => e.CompanyName).HasMaxLength(255);

                entity.Property(e => e.CompanyRegistrationNumber).HasMaxLength(255);

                entity.Property(e => e.ContactEmail).HasMaxLength(255);

                entity.Property(e => e.ContactTelephoneNumber).HasMaxLength(50);

                entity.Property(e => e.Country).HasMaxLength(255);

                entity.Property(e => e.DemographicAverageRate).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.FaxNumber).HasMaxLength(255);

                entity.Property(e => e.FingerprintDistant)
                    .HasColumnType("decimal(5, 2)")
                    .HasDefaultValueSql("((10.00))");

                entity.Property(e => e.GeoBlockEnabled)
                    .IsRequired()
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.NoDemographicInterval).HasDefaultValueSql("((12))");

                entity.Property(e => e.PostalCode)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.QuotaThreeQuarterRate).HasDefaultValueSql("((2000))");

                entity.Property(e => e.ScreeningQuotaDiscountPercentage).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.StandardScreeningRate)
                    .HasColumnType("decimal(18, 2)")
                    .HasDefaultValueSql("((0.4))");

                entity.Property(e => e.State).HasMaxLength(255);

                entity.Property(e => e.TRPM).HasDefaultValueSql("((30))");
            });

            modelBuilder.Entity<__MigrationHistory>(entity =>
            {
                entity.HasKey(e => new { e.MigrationId, e.ContextKey })
                    .HasName("PK_dbo.__MigrationHistory");

                entity.Property(e => e.MigrationId).HasMaxLength(150);

                entity.Property(e => e.ContextKey).HasMaxLength(300);

                entity.Property(e => e.Model).IsRequired();

                entity.Property(e => e.ProductVersion)
                    .IsRequired()
                    .HasMaxLength(32);
            });

            modelBuilder.Entity<vw_questionnaire>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("vw_questionnaire");

                entity.Property(e => e.AnswerValue)
                    .IsRequired()
                    .HasMaxLength(1000);

                entity.Property(e => e.QuestionTemplateContent).IsRequired();

                entity.Property(e => e.QuestionTitle).HasMaxLength(255);

                entity.Property(e => e.Template).IsRequired();

                entity.Property(e => e.TemplateContent).IsRequired();
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
