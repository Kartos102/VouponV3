using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Voupon.Database.Postgres.VodusEntities
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

        public virtual DbSet<AdvertisementResponseDemographic> AdvertisementResponseDemographic { get; set; } = null!;
        public virtual DbSet<AdvertisementResponses> AdvertisementResponses { get; set; } = null!;
        public virtual DbSet<Advertisements> Advertisements { get; set; } = null!;
        public virtual DbSet<AggregatorApiUrls> AggregatorApiUrls { get; set; } = null!;
        public virtual DbSet<AggregatorConfig> AggregatorConfig { get; set; } = null!;
        public virtual DbSet<AggregatorExcludeFilters> AggregatorExcludeFilters { get; set; } = null!;
        public virtual DbSet<AppRoles> AppRoles { get; set; } = null!;
        public virtual DbSet<Areas> Areas { get; set; } = null!;
        public virtual DbSet<ArticleCategories> ArticleCategories { get; set; } = null!;
        public virtual DbSet<Articles> Articles { get; set; } = null!;
        public virtual DbSet<AspNetRoles> AspNetRoles { get; set; } = null!;
        public virtual DbSet<AspNetUserClaims> AspNetUserClaims { get; set; } = null!;
        public virtual DbSet<AspNetUserLogins> AspNetUserLogins { get; set; } = null!;
        public virtual DbSet<AspNetUsers> AspNetUsers { get; set; } = null!;
        public virtual DbSet<BannedUsers> BannedUsers { get; set; } = null!;
        public virtual DbSet<Billings> Billings { get; set; } = null!;
        public virtual DbSet<BlacklistedIPAddress> BlacklistedIPAddress { get; set; } = null!;
        public virtual DbSet<BlacklistedIPs> BlacklistedIPs { get; set; } = null!;
        public virtual DbSet<BonusPoints> BonusPoints { get; set; } = null!;
        public virtual DbSet<ClientPasswordResets> ClientPasswordResets { get; set; } = null!;
        public virtual DbSet<ClientProfiles> ClientProfiles { get; set; } = null!;
        public virtual DbSet<ClientSurveyQuestionTemplates> ClientSurveyQuestionTemplates { get; set; } = null!;
        public virtual DbSet<ClientSurveyTemplates> ClientSurveyTemplates { get; set; } = null!;
        public virtual DbSet<CommercialDFPrices> CommercialDFPrices { get; set; } = null!;
        public virtual DbSet<CommercialDisplayTarget> CommercialDisplayTarget { get; set; } = null!;
        public virtual DbSet<CommercialFilters> CommercialFilters { get; set; } = null!;
        public virtual DbSet<CommercialLeadCustomFields> CommercialLeadCustomFields { get; set; } = null!;
        public virtual DbSet<CommercialLeads> CommercialLeads { get; set; } = null!;
        public virtual DbSet<CommercialPsy> CommercialPsy { get; set; } = null!;
        public virtual DbSet<CommercialPsyQuotas> CommercialPsyQuotas { get; set; } = null!;
        public virtual DbSet<CommercialRates> CommercialRates { get; set; } = null!;
        public virtual DbSet<CommercialRequests> CommercialRequests { get; set; } = null!;
        public virtual DbSet<CommercialResponseReports> CommercialResponseReports { get; set; } = null!;
        public virtual DbSet<CommercialSubgroups> CommercialSubgroups { get; set; } = null!;
        public virtual DbSet<CommercialTargetCosts> CommercialTargetCosts { get; set; } = null!;
        public virtual DbSet<CommercialTargets> CommercialTargets { get; set; } = null!;
        public virtual DbSet<CommercialTopics> CommercialTopics { get; set; } = null!;
        public virtual DbSet<CommercialWeightedScoreD> CommercialWeightedScoreD { get; set; } = null!;
        public virtual DbSet<CommercialWeightedScores> CommercialWeightedScores { get; set; } = null!;
        public virtual DbSet<Commercials> Commercials { get; set; } = null!;
        public virtual DbSet<CommercialsLanguages> CommercialsLanguages { get; set; } = null!;
        public virtual DbSet<Countries> Countries { get; set; } = null!;
        public virtual DbSet<CustomCombinationAnswers> CustomCombinationAnswers { get; set; } = null!;
        public virtual DbSet<CustomCombinationLogicFlows> CustomCombinationLogicFlows { get; set; } = null!;
        public virtual DbSet<DeletedMemberPsychographicAnswers> DeletedMemberPsychographicAnswers { get; set; } = null!;
        public virtual DbSet<DeletedMemberPsychographics> DeletedMemberPsychographics { get; set; } = null!;
        public virtual DbSet<DeletedSurveyResponseAnswers> DeletedSurveyResponseAnswers { get; set; } = null!;
        public virtual DbSet<DeletedSurveyResponses> DeletedSurveyResponses { get; set; } = null!;
        public virtual DbSet<DemographicTypes> DemographicTypes { get; set; } = null!;
        public virtual DbSet<DemographicValues> DemographicValues { get; set; } = null!;
        public virtual DbSet<Devices> Devices { get; set; } = null!;
        public virtual DbSet<Districts> Districts { get; set; } = null!;
        public virtual DbSet<FingerprintIPLogs> FingerprintIPLogs { get; set; } = null!;
        public virtual DbSet<Fingerprints> Fingerprints { get; set; } = null!;
        public virtual DbSet<HUTSurveyForms> HUTSurveyForms { get; set; } = null!;
        public virtual DbSet<HUTSurveyParticipants> HUTSurveyParticipants { get; set; } = null!;
        public virtual DbSet<HUTSurveyParticipantsStatusTypes> HUTSurveyParticipantsStatusTypes { get; set; } = null!;
        public virtual DbSet<HUTSurveyProjects> HUTSurveyProjects { get; set; } = null!;
        public virtual DbSet<HUTSurveyResponses> HUTSurveyResponses { get; set; } = null!;
        public virtual DbSet<IP2Locations> IP2Locations { get; set; } = null!;
        public virtual DbSet<IPLookups> IPLookups { get; set; } = null!;
        public virtual DbSet<InprogressR> InprogressR { get; set; } = null!;
        public virtual DbSet<InvalidDomainRequests> InvalidDomainRequests { get; set; } = null!;
        public virtual DbSet<InvoiceItems> InvoiceItems { get; set; } = null!;
        public virtual DbSet<Invoices> Invoices { get; set; } = null!;
        public virtual DbSet<Ip2Nations> Ip2Nations { get; set; } = null!;
        public virtual DbSet<Ip2nationCountries> Ip2nationCountries { get; set; } = null!;
        public virtual DbSet<Languages> Languages { get; set; } = null!;
        public virtual DbSet<LoggedOutTokens> LoggedOutTokens { get; set; } = null!;
        public virtual DbSet<MasterMemberProfiles> MasterMemberProfiles { get; set; } = null!;
        public virtual DbSet<MasterMemberShippingAddress> MasterMemberShippingAddress { get; set; } = null!;
        public virtual DbSet<MemberProfileCatalog> MemberProfileCatalog { get; set; } = null!;
        public virtual DbSet<MemberProfileChunks> MemberProfileChunks { get; set; } = null!;
        public virtual DbSet<MemberProfileDailyAllowances> MemberProfileDailyAllowances { get; set; } = null!;
        public virtual DbSet<MemberProfileEndedCommercials> MemberProfileEndedCommercials { get; set; } = null!;
        public virtual DbSet<MemberProfileExtensions> MemberProfileExtensions { get; set; } = null!;
        public virtual DbSet<MemberProfileExtensions_Old> MemberProfileExtensions_Old { get; set; } = null!;
        public virtual DbSet<MemberProfileProductAdImpression> MemberProfileProductAdImpression { get; set; } = null!;
        public virtual DbSet<MemberProfileProductAdImpressionExtensions> MemberProfileProductAdImpressionExtensions { get; set; } = null!;
        public virtual DbSet<MemberProfileSubgroups> MemberProfileSubgroups { get; set; } = null!;
        public virtual DbSet<MemberProfileVisits> MemberProfileVisits { get; set; } = null!;
        public virtual DbSet<MemberProfiles> MemberProfiles { get; set; } = null!;
        public virtual DbSet<MemberProfiles_Old> MemberProfiles_Old { get; set; } = null!;
        public virtual DbSet<MemberPsychographicAnswers> MemberPsychographicAnswers { get; set; } = null!;
        public virtual DbSet<MemberPsychographics> MemberPsychographics { get; set; } = null!;
        public virtual DbSet<MobileDevices> MobileDevices { get; set; } = null!;
        public virtual DbSet<OrderItemRedemptions> OrderItemRedemptions { get; set; } = null!;
        public virtual DbSet<OrderItems> OrderItems { get; set; } = null!;
        public virtual DbSet<OrderRedemptions> OrderRedemptions { get; set; } = null!;
        public virtual DbSet<Orders> Orders { get; set; } = null!;
        public virtual DbSet<PartitionGrouping> PartitionGrouping { get; set; } = null!;
        public virtual DbSet<PartnerProfiles> PartnerProfiles { get; set; } = null!;
        public virtual DbSet<PartnerWebsiteCommercialSubgroups> PartnerWebsiteCommercialSubgroups { get; set; } = null!;
        public virtual DbSet<PartnerWebsiteExcludedCommercials> PartnerWebsiteExcludedCommercials { get; set; } = null!;
        public virtual DbSet<PartnerWebsitePriorityCommercials> PartnerWebsitePriorityCommercials { get; set; } = null!;
        public virtual DbSet<PartnerWebsiteVisits> PartnerWebsiteVisits { get; set; } = null!;
        public virtual DbSet<PartnerWebsites> PartnerWebsites { get; set; } = null!;
        public virtual DbSet<Partners> Partners { get; set; } = null!;
        public virtual DbSet<PasswordResets> PasswordResets { get; set; } = null!;
        public virtual DbSet<PaymentStatuses> PaymentStatuses { get; set; } = null!;
        public virtual DbSet<Permissions> Permissions { get; set; } = null!;
        public virtual DbSet<ProductAdLocations> ProductAdLocations { get; set; } = null!;
        public virtual DbSet<ProductAdPartnersDomain> ProductAdPartnersDomain { get; set; } = null!;
        public virtual DbSet<ProductAdPartnersDomainWebsites> ProductAdPartnersDomainWebsites { get; set; } = null!;
        public virtual DbSet<ProductAdSubgroups> ProductAdSubgroups { get; set; } = null!;
        public virtual DbSet<ProductAds> ProductAds { get; set; } = null!;
        public virtual DbSet<ProductAdsConfig> ProductAdsConfig { get; set; } = null!;
        public virtual DbSet<Products> Products { get; set; } = null!;
        public virtual DbSet<PromoCodes> PromoCodes { get; set; } = null!;
        public virtual DbSet<PsyQuestionAnswers> PsyQuestionAnswers { get; set; } = null!;
        public virtual DbSet<PsyQuestionMultiLanguagesAnswers> PsyQuestionMultiLanguagesAnswers { get; set; } = null!;
        public virtual DbSet<PsyQuestions> PsyQuestions { get; set; } = null!;
        public virtual DbSet<QuestionDelays> QuestionDelays { get; set; } = null!;
        public virtual DbSet<QuestionTypes> QuestionTypes { get; set; } = null!;
        public virtual DbSet<RequestForDownloads> RequestForDownloads { get; set; } = null!;
        public virtual DbSet<RequestHeaderKeyValues> RequestHeaderKeyValues { get; set; } = null!;
        public virtual DbSet<RequestHeaders> RequestHeaders { get; set; } = null!;
        public virtual DbSet<RoleClaims> RoleClaims { get; set; } = null!;
        public virtual DbSet<Roles> Roles { get; set; } = null!;
        public virtual DbSet<States> States { get; set; } = null!;
        public virtual DbSet<StatusTypes> StatusTypes { get; set; } = null!;
        public virtual DbSet<SubgroupMultipliers> SubgroupMultipliers { get; set; } = null!;
        public virtual DbSet<Subgroups> Subgroups { get; set; } = null!;
        public virtual DbSet<SubgroupsV2> SubgroupsV2 { get; set; } = null!;
        public virtual DbSet<SurveyCampaigns> SurveyCampaigns { get; set; } = null!;
        public virtual DbSet<SurveyChunkings> SurveyChunkings { get; set; } = null!;
        public virtual DbSet<SurveyLogicFlows> SurveyLogicFlows { get; set; } = null!;
        public virtual DbSet<SurveyQuestionAnswers> SurveyQuestionAnswers { get; set; } = null!;
        public virtual DbSet<SurveyQuestionChucks> SurveyQuestionChucks { get; set; } = null!;
        public virtual DbSet<SurveyQuestionMultiLanguagesAnswers> SurveyQuestionMultiLanguagesAnswers { get; set; } = null!;
        public virtual DbSet<SurveyQuestionMultiLanguagesTitles> SurveyQuestionMultiLanguagesTitles { get; set; } = null!;
        public virtual DbSet<SurveyQuestionTemplates> SurveyQuestionTemplates { get; set; } = null!;
        public virtual DbSet<SurveyQuestionTiers> SurveyQuestionTiers { get; set; } = null!;
        public virtual DbSet<SurveyQuestions> SurveyQuestions { get; set; } = null!;
        public virtual DbSet<SurveyResponseAnswers> SurveyResponseAnswers { get; set; } = null!;
        public virtual DbSet<SurveyResponseDemographic> SurveyResponseDemographic { get; set; } = null!;
        public virtual DbSet<SurveyResponses> SurveyResponses { get; set; } = null!;
        public virtual DbSet<SurveyTemplates> SurveyTemplates { get; set; } = null!;
        public virtual DbSet<SystemConfigurations> SystemConfigurations { get; set; } = null!;
        public virtual DbSet<TempTokens> TempTokens { get; set; } = null!;
        public virtual DbSet<Temp_CommercialFilters> Temp_CommercialFilters { get; set; } = null!;
        public virtual DbSet<Temp_CommercialWeightedScoreDemographics> Temp_CommercialWeightedScoreDemographics { get; set; } = null!;
        public virtual DbSet<Temp_CommercialWeightedScores> Temp_CommercialWeightedScores { get; set; } = null!;
        public virtual DbSet<Topics> Topics { get; set; } = null!;
        public virtual DbSet<TreatedDataJsonFiles> TreatedDataJsonFiles { get; set; } = null!;
        public virtual DbSet<UserClaims> UserClaims { get; set; } = null!;
        public virtual DbSet<UserLogins> UserLogins { get; set; } = null!;
        public virtual DbSet<UserRoles> UserRoles { get; set; } = null!;
        public virtual DbSet<UserTokens> UserTokens { get; set; } = null!;
        public virtual DbSet<Users> Users { get; set; } = null!;
        public virtual DbSet<Variables> Variables { get; set; } = null!;
        public virtual DbSet<__MigrationHistory> __MigrationHistory { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
                optionsBuilder.UseNpgsql("Server=vodus-aurora-postgresql.cluster-c4d0ebxkvzrm.ap-southeast-1.rds.amazonaws.com;Port=5432;User Id=postgres;Password=APv,3Cvqzy,tnpma8rPrj,8QG5DKzU;Database=VodusV2_test2");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AdvertisementResponseDemographic>(entity =>
            {
                entity.ToTable("AdvertisementResponseDemographic", "dbo");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Value).HasMaxLength(1000);
            });

            modelBuilder.Entity<AdvertisementResponses>(entity =>
            {
                entity.ToTable("AdvertisementResponses", "dbo");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.Host).HasMaxLength(10);

                entity.Property(e => e.Origin).HasMaxLength(10);

                entity.Property(e => e.Referer).HasMaxLength(10);

                entity.Property(e => e.TcpIpProtocol).HasMaxLength(100);

                entity.Property(e => e.UserAgent).HasMaxLength(500);
            });

            modelBuilder.Entity<Advertisements>(entity =>
            {
                entity.ToTable("Advertisements", "dbo");

                entity.Property(e => e.Id).UseIdentityAlwaysColumn();

                entity.Property(e => e.Point).HasPrecision(10, 4);
            });

            modelBuilder.Entity<AggregatorApiUrls>(entity =>
            {
                entity.ToTable("AggregatorApiUrls", "dbo");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.LastUpdatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.Url).HasMaxLength(255);
            });

            modelBuilder.Entity<AggregatorConfig>(entity =>
            {
                entity.ToTable("AggregatorConfig", "dbo");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.LazadaCrawlerLastVerifiedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.ShopeeCrawlerLastVerifiedAt).HasColumnType("timestamp without time zone");
            });

            modelBuilder.Entity<AggregatorExcludeFilters>(entity =>
            {
                entity.ToTable("AggregatorExcludeFilters", "dbo");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Keyword).HasMaxLength(255);
            });

            modelBuilder.Entity<AppRoles>(entity =>
            {
                entity.ToTable("AppRoles", "dbo");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Name).HasMaxLength(50);

                entity.HasMany(d => d.Permission)
                    .WithMany(p => p.AppRole)
                    .UsingEntity<Dictionary<string, object>>(
                        "AppRolePermissions",
                        l => l.HasOne<Permissions>().WithMany().HasForeignKey("PermissionId").OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("fk_approlepermissions_permissions_231671873"),
                        r => r.HasOne<AppRoles>().WithMany().HasForeignKey("AppRoleId").OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("fk_approlepermissions_approles_215671816"),
                        j =>
                        {
                            j.HasKey("AppRoleId", "PermissionId").HasName("AppRolePermissions_pkey");

                            j.ToTable("AppRolePermissions", "dbo");
                        });
            });

            modelBuilder.Entity<Areas>(entity =>
            {
                entity.ToTable("Areas", "dbo");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.AreaCode).HasMaxLength(50);

                entity.Property(e => e.AreaName).HasMaxLength(200);

                entity.HasOne(d => d.District)
                    .WithMany(p => p.Areas)
                    .HasForeignKey(d => d.DistrictId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_areas_districts_247671930");
            });

            modelBuilder.Entity<ArticleCategories>(entity =>
            {
                entity.ToTable("ArticleCategories", "dbo");

                entity.Property(e => e.Id).UseIdentityAlwaysColumn();

                entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.CreatedBy).HasMaxLength(128);

                entity.Property(e => e.Name).HasMaxLength(128);
            });

            modelBuilder.Entity<Articles>(entity =>
            {
                entity.ToTable("Articles", "dbo");

                entity.Property(e => e.Id).UseIdentityAlwaysColumn();

                entity.Property(e => e.BannerImageUrl).HasMaxLength(128);

                entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.CreatedBy).HasMaxLength(128);

                entity.Property(e => e.MetaDescription).HasMaxLength(500);

                entity.Property(e => e.MetaKeywords).HasMaxLength(500);

                entity.Property(e => e.MetaTitle).HasMaxLength(255);

                entity.Property(e => e.PageSlug).HasMaxLength(500);

                entity.Property(e => e.PublishedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.ReportDownloadLink).HasMaxLength(128);

                entity.Property(e => e.StoryUrl).HasMaxLength(128);

                entity.Property(e => e.Title).HasMaxLength(255);

                entity.HasOne(d => d.Category)
                    .WithMany(p => p.Articles)
                    .HasForeignKey(d => d.CategoryId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_articles_articlecategories_263671987");
            });

            modelBuilder.Entity<AspNetRoles>(entity =>
            {
                entity.ToTable("AspNetRoles", "dbo");

                entity.Property(e => e.Id).HasMaxLength(128);

                entity.Property(e => e.Name).HasMaxLength(256);
            });

            modelBuilder.Entity<AspNetUserClaims>(entity =>
            {
                entity.ToTable("AspNetUserClaims", "dbo");

                entity.Property(e => e.Id).UseIdentityAlwaysColumn();

                entity.Property(e => e.UserId).HasMaxLength(128);

                entity.HasOne(d => d.User)
                    .WithMany(p => p.AspNetUserClaims)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK_dbo.AspNetUserClaims_dbo.AspNetUsers_UserId_279672044");
            });

            modelBuilder.Entity<AspNetUserLogins>(entity =>
            {
                entity.HasKey(e => new { e.LoginProvider, e.ProviderKey, e.UserId })
                    .HasName("AspNetUserLogins_pkey");

                entity.ToTable("AspNetUserLogins", "dbo");

                entity.Property(e => e.LoginProvider).HasMaxLength(128);

                entity.Property(e => e.ProviderKey).HasMaxLength(128);

                entity.Property(e => e.UserId).HasMaxLength(128);

                entity.HasOne(d => d.User)
                    .WithMany(p => p.AspNetUserLogins)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK_dbo.AspNetUserLogins_dbo.AspNetUsers_UserId_295672101");
            });

            modelBuilder.Entity<AspNetUsers>(entity =>
            {
                entity.ToTable("AspNetUsers", "dbo");

                entity.Property(e => e.Id).HasMaxLength(128);

                entity.Property(e => e.Email).HasMaxLength(256);

                entity.Property(e => e.LockoutEndDateUtc).HasColumnType("timestamp without time zone");

                entity.Property(e => e.UserName).HasMaxLength(256);

                entity.HasMany(d => d.Role)
                    .WithMany(p => p.User)
                    .UsingEntity<Dictionary<string, object>>(
                        "AspNetUserRoles",
                        l => l.HasOne<AspNetRoles>().WithMany().HasForeignKey("RoleId").HasConstraintName("FK_dbo.AspNetUserRoles_dbo.AspNetRoles_RoleId_311672158"),
                        r => r.HasOne<AspNetUsers>().WithMany().HasForeignKey("UserId").HasConstraintName("FK_dbo.AspNetUserRoles_dbo.AspNetUsers_UserId_327672215"),
                        j =>
                        {
                            j.HasKey("UserId", "RoleId").HasName("AspNetUserRoles_pkey");

                            j.ToTable("AspNetUserRoles", "dbo");

                            j.IndexerProperty<string>("UserId").HasMaxLength(128);

                            j.IndexerProperty<string>("RoleId").HasMaxLength(128);
                        });
            });

            modelBuilder.Entity<BannedUsers>(entity =>
            {
                entity.ToTable("BannedUsers", "dbo");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Address).HasColumnType("character varying");

                entity.Property(e => e.BanDate).HasColumnType("timestamp without time zone");

                entity.Property(e => e.Email).HasColumnType("character varying");

                entity.Property(e => e.Name).HasColumnType("character varying");

                entity.Property(e => e.Reason).HasColumnType("character varying");

                entity.Property(e => e.UserId).HasColumnType("character varying");
            });

            modelBuilder.Entity<Billings>(entity =>
            {
                entity.ToTable("Billings", "dbo");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.BillAmount).HasPrecision(19, 4);

                entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.CreatedBy).HasMaxLength(128);

                entity.Property(e => e.InvoiceNumber).HasMaxLength(20);
            });

            modelBuilder.Entity<BlacklistedIPAddress>(entity =>
            {
                entity.ToTable("BlacklistedIPAddress", "dbo");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.IpAddress).HasMaxLength(50);

                entity.Property(e => e.LastBannedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.LastCheckedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.LastUnBannedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.Remark).HasMaxLength(255);
            });

            modelBuilder.Entity<BlacklistedIPs>(entity =>
            {
                entity.ToTable("BlacklistedIPs", "dbo");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.IpAddress).HasMaxLength(50);

                entity.Property(e => e.LastUpdatedAt).HasColumnType("timestamp without time zone");
            });

            modelBuilder.Entity<BonusPoints>(entity =>
            {
                entity.ToTable("BonusPoints", "dbo");

                entity.Property(e => e.Id).UseIdentityAlwaysColumn();

                entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.CreatedBy).HasMaxLength(128);

                entity.Property(e => e.DeletedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.DeletedBy).HasMaxLength(128);

                entity.Property(e => e.LastUpdatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.LastUpdatedBy).HasMaxLength(128);

                entity.Property(e => e.Remark).HasMaxLength(500);

                entity.HasOne(d => d.MasterMemberProfile)
                    .WithMany(p => p.BonusPoints)
                    .HasForeignKey(d => d.MasterMemberProfileId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_bonuspoints_mastermemberprofiles_343672272");
            });

            modelBuilder.Entity<ClientPasswordResets>(entity =>
            {
                entity.ToTable("ClientPasswordResets", "dbo");

                entity.Property(e => e.Id).UseIdentityAlwaysColumn();

                entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.ExpireAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.ResetCode).HasMaxLength(128);

                entity.Property(e => e.Username).HasMaxLength(128);
            });

            modelBuilder.Entity<ClientProfiles>(entity =>
            {
                entity.ToTable("ClientProfiles", "dbo");

                entity.Property(e => e.Id).UseIdentityAlwaysColumn();

                entity.Property(e => e.Address1).HasMaxLength(500);

                entity.Property(e => e.Address2).HasMaxLength(500);

                entity.Property(e => e.Company).HasMaxLength(500);

                entity.Property(e => e.CompanyNumber).HasMaxLength(100);

                entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.CreatedBy).HasMaxLength(128);

                entity.Property(e => e.LineOfWork).HasMaxLength(100);

                entity.Property(e => e.Title).HasMaxLength(100);

                entity.Property(e => e.UserId).HasMaxLength(128);

                entity.HasOne(d => d.Country)
                    .WithMany(p => p.ClientProfiles)
                    .HasForeignKey(d => d.CountryId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_country_id_391672443");

                entity.HasOne(d => d.PartnerWebsite)
                    .WithMany(p => p.ClientProfiles)
                    .HasForeignKey(d => d.PartnerWebsiteId)
                    .HasConstraintName("fk_clientprofiles_partnerwebsites_375672386");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.ClientProfiles)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("fk_clientprofiles_clientprofiles_359672329");
            });

            modelBuilder.Entity<ClientSurveyQuestionTemplates>(entity =>
            {
                entity.ToTable("ClientSurveyQuestionTemplates", "dbo");

                entity.Property(e => e.Id).UseIdentityAlwaysColumn();

                entity.HasOne(d => d.ClientSurveyTemplate)
                    .WithMany(p => p.ClientSurveyQuestionTemplates)
                    .HasForeignKey(d => d.ClientSurveyTemplateId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_clientsurveyquestiontemplates_clientsurveytemplates_40767250");

                entity.HasOne(d => d.QuestionType)
                    .WithMany(p => p.ClientSurveyQuestionTemplates)
                    .HasForeignKey(d => d.QuestionTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_clientsurveyquestiontemplates_questiontypes_423672557");
            });

            modelBuilder.Entity<ClientSurveyTemplates>(entity =>
            {
                entity.ToTable("ClientSurveyTemplates", "dbo");

                entity.Property(e => e.Id).UseIdentityAlwaysColumn();

                entity.Property(e => e.CreatedBy).HasMaxLength(255);

                entity.Property(e => e.CreatedDate).HasColumnType("timestamp without time zone");

                entity.Property(e => e.CreatedIP).HasMaxLength(50);

                entity.Property(e => e.Description).HasMaxLength(255);

                entity.Property(e => e.LastUpdatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.LastUpdatedBy).HasMaxLength(255);

                entity.Property(e => e.LastUpdatedIP).HasMaxLength(50);

                entity.Property(e => e.TemplateName).HasMaxLength(100);

                entity.HasOne(d => d.ClientProfile)
                    .WithMany(p => p.ClientSurveyTemplates)
                    .HasForeignKey(d => d.ClientProfileId)
                    .HasConstraintName("fk_clientsurveytemplates_clientprofiles_439672614");
            });

            modelBuilder.Entity<CommercialDFPrices>(entity =>
            {
                entity.ToTable("CommercialDFPrices", "dbo");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.CostPrice).HasPrecision(19, 4);
            });

            modelBuilder.Entity<CommercialDisplayTarget>(entity =>
            {
                entity.ToTable("CommercialDisplayTarget", "dbo");

                entity.Property(e => e.Id).UseIdentityAlwaysColumn();

                entity.HasOne(d => d.Commercial)
                    .WithMany(p => p.CommercialDisplayTarget)
                    .HasForeignKey(d => d.CommercialId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_commercialdisplaytarget_commercials_455672671");
            });

            modelBuilder.Entity<CommercialFilters>(entity =>
            {
                entity.HasKey(e => new { e.CommercialId, e.SequenceNumber })
                    .HasName("CommercialFilters_pkey");

                entity.ToTable("CommercialFilters", "dbo");

                entity.Property(e => e.QuotaType).HasMaxLength(10);

                entity.HasOne(d => d.Commercial)
                    .WithMany(p => p.CommercialFilters)
                    .HasForeignKey(d => d.CommercialId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_commercialfilters_commercials_471672728");

                entity.HasOne(d => d.DemographicType)
                    .WithMany(p => p.CommercialFilters)
                    .HasForeignKey(d => d.DemographicTypeId)
                    .HasConstraintName("fk_commercialfilters_demographictypes_487672785");
            });

            modelBuilder.Entity<CommercialLeadCustomFields>(entity =>
            {
                entity.ToTable("CommercialLeadCustomFields", "dbo");

                entity.Property(e => e.Id).UseIdentityAlwaysColumn();

                entity.Property(e => e.CustomField).HasMaxLength(500);

                entity.Property(e => e.CustomValue).HasMaxLength(500);

                entity.HasOne(d => d.CommercialLead)
                    .WithMany(p => p.CommercialLeadCustomFields)
                    .HasForeignKey(d => d.CommercialLeadId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_commercialleadcustomfields_commercialleads_503672842");
            });

            modelBuilder.Entity<CommercialLeads>(entity =>
            {
                entity.ToTable("CommercialLeads", "dbo");

                entity.Property(e => e.Id).UseIdentityAlwaysColumn();

                entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.Email).HasMaxLength(255);

                entity.Property(e => e.LastUpdatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.RefererHostName).HasMaxLength(255);

                entity.Property(e => e.RefererUrl).HasMaxLength(255);

                entity.Property(e => e.Remark).HasMaxLength(500);

                entity.Property(e => e.VerificationCode).HasMaxLength(255);

                entity.HasOne(d => d.Commercial)
                    .WithMany(p => p.CommercialLeads)
                    .HasForeignKey(d => d.CommercialId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_commercialleads_commercials_519672899");
            });

            modelBuilder.Entity<CommercialPsy>(entity =>
            {
                entity.ToTable("CommercialPsy", "dbo");

                entity.Property(e => e.Id).UseIdentityAlwaysColumn();

                entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.LastUpdatedAt).HasColumnType("timestamp without time zone");

                entity.HasOne(d => d.Commercial)
                    .WithMany(p => p.CommercialPsy)
                    .HasForeignKey(d => d.CommercialId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_commercialpsy_commercials_535672956");

                entity.HasOne(d => d.PsyQuestion)
                    .WithMany(p => p.CommercialPsy)
                    .HasForeignKey(d => d.PsyQuestionId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_commercialpsy_psyquestions_551673013");
            });

            modelBuilder.Entity<CommercialPsyQuotas>(entity =>
            {
                entity.ToTable("CommercialPsyQuotas", "dbo");

                entity.Property(e => e.Id).UseIdentityAlwaysColumn();

                entity.Property(e => e.AnswerText).HasMaxLength(128);

                entity.HasOne(d => d.CommercialPsy)
                    .WithMany(p => p.CommercialPsyQuotas)
                    .HasForeignKey(d => d.CommercialPsyId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_commercialpsyquotas_commercialpsy_567673070");
            });

            modelBuilder.Entity<CommercialRates>(entity =>
            {
                entity.ToTable("CommercialRates", "dbo");

                entity.Property(e => e.Id).UseIdentityAlwaysColumn();

                entity.Property(e => e.Amount).HasPrecision(18, 2);

                entity.Property(e => e.CountryCode).HasMaxLength(4);

                entity.Property(e => e.LastUpdatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.LastUpdatedBy).HasMaxLength(255);

                entity.Property(e => e.Name).HasMaxLength(255);

                entity.HasOne(d => d.CountryCodeNavigation)
                    .WithMany(p => p.CommercialRates)
                    .HasForeignKey(d => d.CountryCode)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_commercialrates_ip2nationcountries_583673127");
            });

            modelBuilder.Entity<CommercialRequests>(entity =>
            {
                entity.ToTable("CommercialRequests", "dbo");

                entity.Property(e => e.Id).UseIdentityAlwaysColumn();

                entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.Host).HasMaxLength(255);

                entity.Property(e => e.IpAddress).HasMaxLength(45);

                entity.Property(e => e.Origin).HasMaxLength(255);

                entity.Property(e => e.Referer).HasMaxLength(255);

                entity.Property(e => e.UserAgent).HasMaxLength(500);
            });

            modelBuilder.Entity<CommercialResponseReports>(entity =>
            {
                entity.ToTable("CommercialResponseReports", "dbo");

                entity.Property(e => e.Id).UseIdentityAlwaysColumn();

                entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");

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

                entity.Property(e => e.LastUpdatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.Question10AnswerDisplay).HasMaxLength(1000);

                entity.Property(e => e.Question10AnswerId).HasMaxLength(255);

                entity.Property(e => e.Question10AnswerUpdatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.Question10Title).HasMaxLength(255);

                entity.Property(e => e.Question11AnswerDisplay).HasMaxLength(1000);

                entity.Property(e => e.Question11AnswerId).HasMaxLength(255);

                entity.Property(e => e.Question11AnswerUpdatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.Question11Title).HasMaxLength(255);

                entity.Property(e => e.Question12AnswerDisplay).HasMaxLength(1000);

                entity.Property(e => e.Question12AnswerId).HasMaxLength(255);

                entity.Property(e => e.Question12AnswerUpdatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.Question12Title).HasMaxLength(255);

                entity.Property(e => e.Question13AnswerDisplay).HasMaxLength(1000);

                entity.Property(e => e.Question13AnswerId).HasMaxLength(255);

                entity.Property(e => e.Question13AnswerUpdatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.Question13Title).HasMaxLength(255);

                entity.Property(e => e.Question14AnswerDisplay).HasMaxLength(1000);

                entity.Property(e => e.Question14AnswerId).HasMaxLength(255);

                entity.Property(e => e.Question14AnswerUpdatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.Question14Title).HasMaxLength(255);

                entity.Property(e => e.Question15AnswerDisplay).HasMaxLength(1000);

                entity.Property(e => e.Question15AnswerId).HasMaxLength(255);

                entity.Property(e => e.Question15AnswerUpdatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.Question15Title).HasMaxLength(255);

                entity.Property(e => e.Question16AnswerDisplay).HasMaxLength(1000);

                entity.Property(e => e.Question16AnswerId).HasMaxLength(255);

                entity.Property(e => e.Question16AnswerUpdatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.Question16Title).HasMaxLength(255);

                entity.Property(e => e.Question17AnswerDisplay).HasMaxLength(1000);

                entity.Property(e => e.Question17AnswerId).HasMaxLength(255);

                entity.Property(e => e.Question17AnswerUpdatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.Question17Title).HasMaxLength(255);

                entity.Property(e => e.Question18AnswerDisplay).HasMaxLength(1000);

                entity.Property(e => e.Question18AnswerId).HasMaxLength(255);

                entity.Property(e => e.Question18AnswerUpdatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.Question18Title).HasMaxLength(255);

                entity.Property(e => e.Question19AnswerDisplay).HasMaxLength(1000);

                entity.Property(e => e.Question19AnswerId).HasMaxLength(255);

                entity.Property(e => e.Question19AnswerUpdatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.Question19Title).HasMaxLength(255);

                entity.Property(e => e.Question1AnswerDisplay).HasMaxLength(1000);

                entity.Property(e => e.Question1AnswerId).HasMaxLength(255);

                entity.Property(e => e.Question1AnswerUpdatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.Question1Title).HasMaxLength(255);

                entity.Property(e => e.Question20AnswerDisplay).HasMaxLength(1000);

                entity.Property(e => e.Question20AnswerId).HasMaxLength(255);

                entity.Property(e => e.Question20AnswerUpdatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.Question20Title).HasMaxLength(255);

                entity.Property(e => e.Question21AnswerDisplay).HasMaxLength(1000);

                entity.Property(e => e.Question21AnswerId).HasMaxLength(255);

                entity.Property(e => e.Question21AnswerUpdatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.Question21Title).HasMaxLength(255);

                entity.Property(e => e.Question22AnswerDisplay).HasMaxLength(1000);

                entity.Property(e => e.Question22AnswerId).HasMaxLength(255);

                entity.Property(e => e.Question22AnswerUpdatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.Question22Title).HasMaxLength(255);

                entity.Property(e => e.Question23AnswerDisplay).HasMaxLength(1000);

                entity.Property(e => e.Question23AnswerId).HasMaxLength(255);

                entity.Property(e => e.Question23AnswerUpdatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.Question23Title).HasMaxLength(255);

                entity.Property(e => e.Question24AnswerDisplay).HasMaxLength(1000);

                entity.Property(e => e.Question24AnswerId).HasMaxLength(255);

                entity.Property(e => e.Question24AnswerUpdatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.Question24Title).HasMaxLength(255);

                entity.Property(e => e.Question25AnswerDisplay).HasMaxLength(1000);

                entity.Property(e => e.Question25AnswerId).HasMaxLength(255);

                entity.Property(e => e.Question25AnswerUpdatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.Question25Title).HasMaxLength(255);

                entity.Property(e => e.Question26AnswerDisplay).HasMaxLength(1000);

                entity.Property(e => e.Question26AnswerId).HasMaxLength(255);

                entity.Property(e => e.Question26AnswerUpdatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.Question26Title).HasMaxLength(255);

                entity.Property(e => e.Question27AnswerDisplay).HasMaxLength(1000);

                entity.Property(e => e.Question27AnswerId).HasMaxLength(255);

                entity.Property(e => e.Question27AnswerUpdatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.Question27Title).HasMaxLength(255);

                entity.Property(e => e.Question28AnswerDisplay).HasMaxLength(1000);

                entity.Property(e => e.Question28AnswerId).HasMaxLength(255);

                entity.Property(e => e.Question28AnswerUpdatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.Question28Title).HasMaxLength(255);

                entity.Property(e => e.Question29AnswerDisplay).HasMaxLength(1000);

                entity.Property(e => e.Question29AnswerId).HasMaxLength(255);

                entity.Property(e => e.Question29AnswerUpdatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.Question29Title).HasMaxLength(255);

                entity.Property(e => e.Question2AnswerDisplay).HasMaxLength(1000);

                entity.Property(e => e.Question2AnswerId).HasMaxLength(255);

                entity.Property(e => e.Question2AnswerUpdatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.Question2Title).HasMaxLength(255);

                entity.Property(e => e.Question30AnswerDisplay).HasMaxLength(1000);

                entity.Property(e => e.Question30AnswerId).HasMaxLength(255);

                entity.Property(e => e.Question30AnswerUpdatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.Question30Title).HasMaxLength(255);

                entity.Property(e => e.Question3AnswerDisplay).HasMaxLength(1000);

                entity.Property(e => e.Question3AnswerId).HasMaxLength(255);

                entity.Property(e => e.Question3AnswerUpdatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.Question3Title).HasMaxLength(255);

                entity.Property(e => e.Question4AnswerDisplay).HasMaxLength(1000);

                entity.Property(e => e.Question4AnswerId).HasMaxLength(255);

                entity.Property(e => e.Question4AnswerUpdatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.Question4Title).HasMaxLength(255);

                entity.Property(e => e.Question5AnswerDisplay).HasMaxLength(1000);

                entity.Property(e => e.Question5AnswerId).HasMaxLength(255);

                entity.Property(e => e.Question5AnswerUpdatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.Question5Title).HasMaxLength(255);

                entity.Property(e => e.Question6AnswerDisplay).HasMaxLength(1000);

                entity.Property(e => e.Question6AnswerId).HasMaxLength(255);

                entity.Property(e => e.Question6AnswerUpdatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.Question6Title).HasMaxLength(255);

                entity.Property(e => e.Question7AnswerDisplay).HasMaxLength(1000);

                entity.Property(e => e.Question7AnswerId).HasMaxLength(255);

                entity.Property(e => e.Question7AnswerUpdatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.Question7Title).HasMaxLength(255);

                entity.Property(e => e.Question8AnswerDisplay).HasMaxLength(1000);

                entity.Property(e => e.Question8AnswerId).HasMaxLength(255);

                entity.Property(e => e.Question8AnswerUpdatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.Question8Title).HasMaxLength(255);

                entity.Property(e => e.Question9AnswerDisplay).HasMaxLength(1000);

                entity.Property(e => e.Question9AnswerId).HasMaxLength(255);

                entity.Property(e => e.Question9AnswerUpdatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.Question9Title).HasMaxLength(255);

                entity.Property(e => e.ResponseCost).HasPrecision(18, 2);

                entity.Property(e => e.TotalResponseCost).HasPrecision(18, 2);

                entity.Property(e => e.UserId).HasMaxLength(36);
            });

            modelBuilder.Entity<CommercialSubgroups>(entity =>
            {
                entity.ToTable("CommercialSubgroups", "dbo");

                entity.Property(e => e.Id).UseIdentityAlwaysColumn();

                entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.MValue).HasPrecision(18, 2);

                entity.Property(e => e.SubgroupName).HasMaxLength(255);

                entity.HasOne(d => d.Chunk)
                    .WithMany(p => p.CommercialSubgroups)
                    .HasForeignKey(d => d.ChunkId)
                    .HasConstraintName("fk_commercialsubgroups_chunk_711673583");

                entity.HasOne(d => d.Commercial)
                    .WithMany(p => p.CommercialSubgroups)
                    .HasForeignKey(d => d.CommercialId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_commercialsubgroups_commercials_727673640");
            });

            modelBuilder.Entity<CommercialTargetCosts>(entity =>
            {
                entity.ToTable("CommercialTargetCosts", "dbo");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.CostPrice).HasPrecision(19, 4);

                entity.HasOne(d => d.CommercialIdDFPrice)
                    .WithMany(p => p.CommercialTargetCosts)
                    .HasForeignKey(d => d.CommercialIdDFPriceId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_commercialtargetcosts_commercialdfprices_743673697");

                entity.HasOne(d => d.DemographicType)
                    .WithMany(p => p.CommercialTargetCosts)
                    .HasForeignKey(d => d.DemographicTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_commercialtargetcosts_demographictypes_759673754");
            });

            modelBuilder.Entity<CommercialTargets>(entity =>
            {
                entity.ToTable("CommercialTargets", "dbo");

                entity.Property(e => e.Id).UseIdentityAlwaysColumn();

                entity.Property(e => e.DemographicRates).HasPrecision(18, 2);

                entity.Property(e => e.Value).HasMaxLength(100);

                entity.Property(e => e.Value2).HasMaxLength(100);

                entity.HasOne(d => d.Commercial)
                    .WithMany(p => p.CommercialTargets)
                    .HasForeignKey(d => d.CommercialId)
                    .HasConstraintName("fk_commercialtargets_commercials_775673811");

                entity.HasOne(d => d.CommercialTargetCost)
                    .WithMany(p => p.CommercialTargets)
                    .HasForeignKey(d => d.CommercialTargetCostId)
                    .HasConstraintName("fk_commercialtargets_commercialtargetcosts_791673868");

                entity.HasOne(d => d.DemographicType)
                    .WithMany(p => p.CommercialTargets)
                    .HasForeignKey(d => d.DemographicTypeId)
                    .HasConstraintName("fk_commercialtargets_demographictypes_807673925");

                entity.HasOne(d => d.DemographicValue)
                    .WithMany(p => p.CommercialTargets)
                    .HasForeignKey(d => d.DemographicValueId)
                    .HasConstraintName("fk_commercialtargets_demographicvalues_823673982");
            });

            modelBuilder.Entity<CommercialTopics>(entity =>
            {
                entity.HasKey(e => new { e.CommercialId, e.TopicId })
                    .HasName("CommercialTopics_pkey");

                entity.ToTable("CommercialTopics", "dbo");
            });

            modelBuilder.Entity<CommercialWeightedScoreD>(entity =>
            {
                entity.HasKey(e => new { e.CommercialId, e.SequenceNumber, e.SubSequenceNumber, e.CommercialTrgetId })
                    .HasName("CommercialWeightedScoreD_pkey");

                entity.ToTable("CommercialWeightedScoreD", "dbo");
            });

            modelBuilder.Entity<CommercialWeightedScores>(entity =>
            {
                entity.HasKey(e => new { e.CommercialId, e.SequenceNumber, e.SubSequenceNumber })
                    .HasName("CommercialWeightedScores_pkey");

                entity.ToTable("CommercialWeightedScores", "dbo");

                entity.HasOne(d => d.Commercial)
                    .WithMany(p => p.CommercialWeightedScores)
                    .HasForeignKey(d => d.CommercialId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_commercialweightedscores_commercials_839674039");
            });

            modelBuilder.Entity<Commercials>(entity =>
            {
                entity.ToTable("Commercials", "dbo");

                entity.Property(e => e.Id).UseIdentityAlwaysColumn();

                entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.CreatedBy).HasMaxLength(256);

                entity.Property(e => e.DemographicQuotaRate).HasPrecision(18, 2);

                entity.Property(e => e.Description).HasMaxLength(1000);

                entity.Property(e => e.EffectiveRate).HasPrecision(18, 2);

                entity.Property(e => e.EndDate).HasColumnType("timestamp without time zone");

                entity.Property(e => e.EstimatedCost).HasPrecision(19, 4);

                entity.Property(e => e.InUseRate).HasPrecision(18, 2);

                entity.Property(e => e.IncidentRate).HasPrecision(18, 2);

                entity.Property(e => e.Name).HasMaxLength(255);

                entity.Property(e => e.ReferralUrl).HasMaxLength(255);

                entity.Property(e => e.ScreeningQuotaM).HasPrecision(18, 2);

                entity.Property(e => e.ScreeningQuotaMAverageValue).HasPrecision(18, 2);

                entity.Property(e => e.SecretCode).HasMaxLength(70);

                entity.Property(e => e.StartDate).HasColumnType("timestamp without time zone");

                entity.Property(e => e.TargetCountryCode).HasMaxLength(4);

                entity.Property(e => e.UpdatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.UpdatedBy).HasMaxLength(256);

                entity.HasOne(d => d.ClientProfile)
                    .WithMany(p => p.Commercials)
                    .HasForeignKey(d => d.ClientProfileId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_commercials_clientprofiles_599673184");

                entity.HasOne(d => d.ClientTemplate)
                    .WithMany(p => p.Commercials)
                    .HasForeignKey(d => d.ClientTemplateId)
                    .HasConstraintName("fk_commercials_clientsurveytemplates_615673241");

                entity.HasOne(d => d.DefaultLanguageNavigation)
                    .WithMany(p => p.Commercials)
                    .HasForeignKey(d => d.DefaultLanguage)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_languages_id_663673412");

                entity.HasOne(d => d.SurveyCampaign)
                    .WithMany(p => p.Commercials)
                    .HasForeignKey(d => d.SurveyCampaignId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_commercials_surveycampaigns_631673298");

                entity.HasOne(d => d.Topic)
                    .WithMany(p => p.Commercials)
                    .HasForeignKey(d => d.TopicId)
                    .HasConstraintName("fk_commercials_topics_647673355");
            });

            modelBuilder.Entity<CommercialsLanguages>(entity =>
            {
                entity.ToTable("CommercialsLanguages", "dbo");

                entity.Property(e => e.Id).UseIdentityAlwaysColumn();

                entity.HasOne(d => d.Commercial)
                    .WithMany(p => p.CommercialsLanguages)
                    .HasForeignKey(d => d.CommercialId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_commercial_id_679673469");

                entity.HasOne(d => d.Language)
                    .WithMany(p => p.CommercialsLanguages)
                    .HasForeignKey(d => d.LanguageId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_language_id_695673526");
            });

            modelBuilder.Entity<Countries>(entity =>
            {
                entity.ToTable("Countries", "dbo");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.CodeNumber).HasMaxLength(5);

                entity.Property(e => e.ISO2).HasMaxLength(2);

                entity.Property(e => e.ISO3).HasMaxLength(4);

                entity.Property(e => e.Name).HasMaxLength(200);
            });

            modelBuilder.Entity<CustomCombinationAnswers>(entity =>
            {
                entity.ToTable("CustomCombinationAnswers", "dbo");

                entity.Property(e => e.Id).UseIdentityAlwaysColumn();

                entity.HasOne(d => d.CustomCombinationFlow)
                    .WithMany(p => p.CustomCombinationAnswers)
                    .HasForeignKey(d => d.CustomCombinationFlowId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_customcombinationanswers_customcombinationlogicflows_8556740");
            });

            modelBuilder.Entity<CustomCombinationLogicFlows>(entity =>
            {
                entity.ToTable("CustomCombinationLogicFlows", "dbo");

                entity.Property(e => e.Id).UseIdentityAlwaysColumn();

                entity.Property(e => e.Name).HasMaxLength(1000);
            });

            modelBuilder.Entity<DeletedMemberPsychographicAnswers>(entity =>
            {
                entity.ToTable("DeletedMemberPsychographicAnswers", "dbo");

                entity.Property(e => e.Id).UseIdentityAlwaysColumn();
            });

            modelBuilder.Entity<DeletedMemberPsychographics>(entity =>
            {
                entity.ToTable("DeletedMemberPsychographics", "dbo");

                entity.Property(e => e.Id).UseIdentityAlwaysColumn();

                entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");
            });

            modelBuilder.Entity<DeletedSurveyResponseAnswers>(entity =>
            {
                entity.ToTable("DeletedSurveyResponseAnswers", "dbo");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.OtherAnswer).HasMaxLength(255);
            });

            modelBuilder.Entity<DeletedSurveyResponses>(entity =>
            {
                entity.ToTable("DeletedSurveyResponses", "dbo");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.Host).HasMaxLength(255);

                entity.Property(e => e.IpAddress).HasMaxLength(45);

                entity.Property(e => e.MemberDemographic).HasMaxLength(2000);

                entity.Property(e => e.Origin).HasMaxLength(255);

                entity.Property(e => e.PartnerCode).HasMaxLength(36);

                entity.Property(e => e.Referer).HasMaxLength(255);

                entity.Property(e => e.UserAgent).HasMaxLength(500);
            });

            modelBuilder.Entity<DemographicTypes>(entity =>
            {
                entity.ToTable("DemographicTypes", "dbo");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Description).HasMaxLength(50);

                entity.Property(e => e.Name).HasMaxLength(50);

                entity.Property(e => e.ResourceString).HasMaxLength(50);

                entity.Property(e => e.TargetAverageRate).HasPrecision(18, 2);
            });

            modelBuilder.Entity<DemographicValues>(entity =>
            {
                entity.ToTable("DemographicValues", "dbo");

                entity.Property(e => e.Id).UseIdentityAlwaysColumn();

                entity.Property(e => e.CountryCode).HasMaxLength(4);

                entity.Property(e => e.DisplayValue).HasMaxLength(100);

                entity.Property(e => e.LastUpdatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.LastUpdatedBy).HasMaxLength(128);

                entity.Property(e => e.TargetRate).HasPrecision(18, 2);

                entity.Property(e => e.TargetWeight).HasPrecision(5, 2);

                entity.Property(e => e.Value1).HasMaxLength(100);

                entity.Property(e => e.Value2).HasMaxLength(100);

                entity.HasOne(d => d.DemographicType)
                    .WithMany(p => p.DemographicValues)
                    .HasForeignKey(d => d.DemographicTypeId)
                    .HasConstraintName("fk_demographicvalues_demographictypes_871674153");
            });

            modelBuilder.Entity<Devices>(entity =>
            {
                entity.ToTable("Devices", "dbo");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.CityName).HasMaxLength(128);

                entity.Property(e => e.CountryCode).HasMaxLength(2);

                entity.Property(e => e.CountryName).HasMaxLength(64);

                entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.IpAddress).HasMaxLength(39);

                entity.Property(e => e.LastUpdatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.Latitude).HasPrecision(10, 8);

                entity.Property(e => e.Longitude).HasPrecision(11, 8);

                entity.Property(e => e.RegionName).HasMaxLength(128);

                entity.Property(e => e.ZipCode).HasMaxLength(30);
            });

            modelBuilder.Entity<Districts>(entity =>
            {
                entity.ToTable("Districts", "dbo");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Code).HasMaxLength(50);

                entity.Property(e => e.Name).HasMaxLength(200);

                entity.HasOne(d => d.State)
                    .WithMany(p => p.Districts)
                    .HasForeignKey(d => d.StateId)
                    .HasConstraintName("fk_districts_states_887674210");
            });

            modelBuilder.Entity<FingerprintIPLogs>(entity =>
            {
                entity.ToTable("FingerprintIPLogs", "dbo");

                entity.Property(e => e.Id).UseIdentityAlwaysColumn();

                entity.Property(e => e.CityName).HasMaxLength(128);

                entity.Property(e => e.CountryCode).HasMaxLength(2);

                entity.Property(e => e.CountryName).HasMaxLength(64);

                entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.IpAddress).HasMaxLength(39);

                entity.Property(e => e.LastCCAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.Latitude).HasPrecision(10, 8);

                entity.Property(e => e.Longitude).HasPrecision(11, 8);

                entity.Property(e => e.RegionName).HasMaxLength(128);

                entity.Property(e => e.VisitorId).HasMaxLength(32);

                entity.Property(e => e.ZipCode).HasMaxLength(30);
            });

            modelBuilder.Entity<Fingerprints>(entity =>
            {
                entity.ToTable("Fingerprints", "dbo");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.LastCCAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.LastUpdatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.Token).HasMaxLength(255);

                entity.Property(e => e.VisitorId).HasMaxLength(32);
            });

            modelBuilder.Entity<HUTSurveyForms>(entity =>
            {
                entity.ToTable("HUTSurveyForms", "dbo");

                entity.Property(e => e.Id).UseIdentityAlwaysColumn();

                entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.CreatedBy).HasMaxLength(256);

                entity.Property(e => e.Name).HasMaxLength(256);

                entity.Property(e => e.URL).HasMaxLength(256);

                entity.Property(e => e.UpdatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.UpdatedBy).HasMaxLength(256);

                entity.HasOne(d => d.HUTSurveyProject)
                    .WithMany(p => p.HUTSurveyForms)
                    .HasForeignKey(d => d.HUTSurveyProjectId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_surveyform_hutsurveyproject_id_903674267");
            });

            modelBuilder.Entity<HUTSurveyParticipants>(entity =>
            {
                entity.ToTable("HUTSurveyParticipants", "dbo");

                entity.Property(e => e.Id).UseIdentityAlwaysColumn();

                entity.Property(e => e.City).HasMaxLength(255);

                entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.CreatedBy).HasMaxLength(256);

                entity.Property(e => e.Email).HasMaxLength(256);

                entity.Property(e => e.Name).HasMaxLength(256);

                entity.Property(e => e.PhoneNumber).HasMaxLength(20);

                entity.Property(e => e.Postcode).HasMaxLength(15);

                entity.Property(e => e.UpdatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.UpdatedBy).HasMaxLength(256);

                entity.HasOne(d => d.HUTSurveyProject)
                    .WithMany(p => p.HUTSurveyParticipants)
                    .HasForeignKey(d => d.HUTSurveyProjectId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_hutsurveyproject_id_919674324");

                entity.HasOne(d => d.StatusType)
                    .WithMany(p => p.HUTSurveyParticipants)
                    .HasForeignKey(d => d.StatusTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_statustype_id_935674381");
            });

            modelBuilder.Entity<HUTSurveyParticipantsStatusTypes>(entity =>
            {
                entity.ToTable("HUTSurveyParticipantsStatusTypes", "dbo");

                entity.Property(e => e.Id).UseIdentityAlwaysColumn();

                entity.Property(e => e.Description).HasMaxLength(50);

                entity.Property(e => e.Name).HasMaxLength(50);
            });

            modelBuilder.Entity<HUTSurveyProjects>(entity =>
            {
                entity.ToTable("HUTSurveyProjects", "dbo");

                entity.Property(e => e.Id).UseIdentityAlwaysColumn();

                entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.CreatedBy).HasMaxLength(256);

                entity.Property(e => e.EndDate).HasColumnType("timestamp without time zone");

                entity.Property(e => e.ExternalName).HasMaxLength(255);

                entity.Property(e => e.Name).HasMaxLength(255);

                entity.Property(e => e.StartDate).HasColumnType("timestamp without time zone");

                entity.Property(e => e.UpdatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.UpdatedBy).HasMaxLength(256);

                entity.Property(e => e.VPointsReward).HasMaxLength(255);

                entity.HasOne(d => d.Language)
                    .WithMany(p => p.HUTSurveyProjects)
                    .HasForeignKey(d => d.LanguageId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_hutsurveyprojects_languages_951674438");
            });

            modelBuilder.Entity<HUTSurveyResponses>(entity =>
            {
                entity.ToTable("HUTSurveyResponses", "dbo");

                entity.Property(e => e.Id).UseIdentityAlwaysColumn();

                entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");

                entity.HasOne(d => d.HUTSurveyForm)
                    .WithMany(p => p.HUTSurveyResponses)
                    .HasForeignKey(d => d.HUTSurveyFormId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_hutsurveyform_id_967674495");

                entity.HasOne(d => d.Participant)
                    .WithMany(p => p.HUTSurveyResponses)
                    .HasForeignKey(d => d.ParticipantId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_hutsurveyresponses_hutsurveyparticipants_983674552");
            });

            modelBuilder.Entity<IP2Locations>(entity =>
            {
                entity.HasNoKey();

                entity.ToTable("IP2Locations", "dbo");

                entity.Property(e => e.CountryCode).HasMaxLength(2);

                entity.Property(e => e.CountryName).HasMaxLength(100);
            });

            modelBuilder.Entity<IPLookups>(entity =>
            {
                entity.ToTable("IPLookups", "dbo");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.CityName).HasMaxLength(128);

                entity.Property(e => e.CountryCode).HasMaxLength(2);

                entity.Property(e => e.CountryName).HasMaxLength(64);

                entity.Property(e => e.Latitude).HasPrecision(10, 8);

                entity.Property(e => e.Longitude).HasPrecision(11, 8);

                entity.Property(e => e.RegionName).HasMaxLength(128);

                entity.Property(e => e.TimeZone).HasMaxLength(8);

                entity.Property(e => e.ZipCode).HasMaxLength(30);
            });

            modelBuilder.Entity<InprogressR>(entity =>
            {
                entity.ToTable("InprogressR", "dbo");

                entity.Property(e => e.Id).UseIdentityAlwaysColumn();

                entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.RecordValue).HasPrecision(18, 2);

                entity.Property(e => e.SubgroupName).HasMaxLength(255);
            });

            modelBuilder.Entity<InvalidDomainRequests>(entity =>
            {
                entity.ToTable("InvalidDomainRequests", "dbo");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.Domain).HasMaxLength(255);

                entity.Property(e => e.LastUpdatedAt).HasColumnType("timestamp without time zone");
            });

            modelBuilder.Entity<InvoiceItems>(entity =>
            {
                entity.ToTable("InvoiceItems", "dbo");

                entity.Property(e => e.Id).UseIdentityAlwaysColumn();

                entity.Property(e => e.Amount).HasPrecision(18, 2);

                entity.Property(e => e.RateAmount).HasPrecision(18, 2);

                entity.HasOne(d => d.Invoice)
                    .WithMany(p => p.InvoiceItems)
                    .HasForeignKey(d => d.InvoiceId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_invoiceitems_invoices_999674609");
            });

            modelBuilder.Entity<Invoices>(entity =>
            {
                entity.ToTable("Invoices", "dbo");

                entity.Property(e => e.Id).UseIdentityAlwaysColumn();

                entity.Property(e => e.Amount).HasPrecision(18, 2);

                entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.CreatedBy).HasMaxLength(255);

                entity.Property(e => e.CurrencyCode).HasMaxLength(3);

                entity.Property(e => e.DepositRequiredPercentage).HasPrecision(18, 2);

                entity.Property(e => e.FileUrl).HasMaxLength(255);

                entity.Property(e => e.FinalAmount).HasPrecision(18, 2);

                entity.Property(e => e.FinalPaymentDiscountPercentage).HasPrecision(18, 2);

                entity.Property(e => e.PromoCodeDescription).HasMaxLength(500);

                entity.Property(e => e.ToAddressLine1).HasMaxLength(255);

                entity.Property(e => e.ToAddressLine2).HasMaxLength(255);

                entity.Property(e => e.ToCompanyName).HasMaxLength(255);

                entity.Property(e => e.ToCountry).HasMaxLength(255);

                entity.Property(e => e.ToPostalCode).HasMaxLength(255);

                entity.Property(e => e.ToState).HasMaxLength(255);

                entity.Property(e => e.UpdatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.UpdatedBy).HasMaxLength(255);

                entity.HasOne(d => d.Commercial)
                    .WithMany(p => p.Invoices)
                    .HasForeignKey(d => d.CommercialId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_invoices_commercials_1015674666");

                entity.HasOne(d => d.PromoCode)
                    .WithMany(p => p.Invoices)
                    .HasForeignKey(d => d.PromoCodeId)
                    .HasConstraintName("fk_invoices_promocodes_1031674723");
            });

            modelBuilder.Entity<Ip2Nations>(entity =>
            {
                entity.HasKey(e => e.Ip)
                    .HasName("Ip2Nations_pkey");

                entity.ToTable("Ip2Nations", "dbo");

                entity.Property(e => e.Ip).ValueGeneratedNever();

                entity.Property(e => e.CountryCode).HasMaxLength(4);

                entity.HasOne(d => d.CountryCodeNavigation)
                    .WithMany(p => p.Ip2Nations)
                    .HasForeignKey(d => d.CountryCode)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_ip2nations_ip2nationcountries_1047674780");
            });

            modelBuilder.Entity<Ip2nationCountries>(entity =>
            {
                entity.HasKey(e => e.Code)
                    .HasName("Ip2nationCountries_pkey");

                entity.ToTable("Ip2nationCountries", "dbo");

                entity.Property(e => e.Code).HasMaxLength(4);

                entity.Property(e => e.Country).HasMaxLength(255);

                entity.Property(e => e.CurrencyCode).HasMaxLength(4);

                entity.Property(e => e.IsoCode2).HasMaxLength(2);

                entity.Property(e => e.IsoCode3).HasMaxLength(3);

                entity.Property(e => e.IsoCountry).HasMaxLength(255);
            });

            modelBuilder.Entity<Languages>(entity =>
            {
                entity.ToTable("Languages", "dbo");

                entity.Property(e => e.Id).UseIdentityAlwaysColumn();

                entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.CreatedBy).HasMaxLength(128);

                entity.Property(e => e.LanguageCode).HasMaxLength(10);

                entity.Property(e => e.LanguageDisplayName).HasMaxLength(50);
            });

            modelBuilder.Entity<LoggedOutTokens>(entity =>
            {
                entity.HasKey(e => e.Token)
                    .HasName("LoggedOutTokens_pkey");

                entity.ToTable("LoggedOutTokens", "dbo");

                entity.Property(e => e.Token).HasMaxLength(255);

                entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");
            });

            modelBuilder.Entity<MasterMemberProfiles>(entity =>
            {
                entity.ToTable("MasterMemberProfiles", "dbo");

                entity.Property(e => e.Id).UseIdentityAlwaysColumn();

                entity.Property(e => e.AddressLine1).HasMaxLength(255);

                entity.Property(e => e.AddressLine2).HasMaxLength(255);

                entity.Property(e => e.City).HasMaxLength(255);

                entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.DateOfBirth).HasColumnType("timestamp without time zone");

                entity.Property(e => e.LastGenDateTime).HasColumnType("timestamp without time zone");

                entity.Property(e => e.LastGenOtp).HasMaxLength(6);

                entity.Property(e => e.MobileCountryCode).HasMaxLength(5);

                entity.Property(e => e.MobileNumber).HasMaxLength(20);

                entity.Property(e => e.MobileVerified).HasMaxLength(3);

                entity.Property(e => e.Postcode).HasMaxLength(15);

                entity.Property(e => e.PreferLanguage).HasMaxLength(5);

                entity.Property(e => e.State).HasMaxLength(255);

                entity.Property(e => e.UserId).HasMaxLength(128);

                entity.HasOne(d => d.Country)
                    .WithMany(p => p.MasterMemberProfiles)
                    .HasForeignKey(d => d.CountryId)
                    .HasConstraintName("fk_mastermemberprofiles_countries_1063674837");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.MasterMemberProfiles)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_mastermemberprofiles_users_1079674894");
            });

            modelBuilder.Entity<MasterMemberShippingAddress>(entity =>
            {
                entity.ToTable("MasterMemberShippingAddress", "dbo");

                entity.Property(e => e.Id).UseIdentityAlwaysColumn();

                entity.Property(e => e.AddressLine1).HasMaxLength(255);

                entity.Property(e => e.AddressLine2).HasMaxLength(255);

                entity.Property(e => e.City).HasMaxLength(255);

                entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.FirstName).HasMaxLength(255);

                entity.Property(e => e.LastName).HasMaxLength(255);

                entity.Property(e => e.Postcode).HasMaxLength(15);

                entity.Property(e => e.State).HasMaxLength(255);

                entity.HasOne(d => d.MasterMemberProfile)
                    .WithMany(p => p.MasterMemberShippingAddress)
                    .HasForeignKey(d => d.MasterMemberProfileId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_mastermembershippingaddress_mastermemberprofiles_1095674951");
            });

            modelBuilder.Entity<MemberProfileCatalog>(entity =>
            {
                entity.HasKey(e => e.Guid)
                    .HasName("MemberProfileCatalog_pkey");

                entity.ToTable("MemberProfileCatalog", "dbo");

                entity.Property(e => e.Guid).HasMaxLength(128);
            });

            modelBuilder.Entity<MemberProfileChunks>(entity =>
            {
                entity.ToTable("MemberProfileChunks", "dbo");

                entity.Property(e => e.Id).UseIdentityAlwaysColumn();

                entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.LastUpdatedat).HasColumnType("timestamp without time zone");
            });

            modelBuilder.Entity<MemberProfileDailyAllowances>(entity =>
            {
                entity.ToTable("MemberProfileDailyAllowances", "dbo");

                entity.Property(e => e.Id).UseIdentityAlwaysColumn();

                entity.Property(e => e.LastUpdatedAt).HasColumnType("timestamp without time zone");
            });

            modelBuilder.Entity<MemberProfileEndedCommercials>(entity =>
            {
                entity.ToTable("MemberProfileEndedCommercials", "dbo");

                entity.Property(e => e.Id).UseIdentityAlwaysColumn();

                entity.Property(e => e.EndedAt).HasColumnType("timestamp without time zone");

                entity.HasOne(d => d.Commercial)
                    .WithMany(p => p.MemberProfileEndedCommercials)
                    .HasForeignKey(d => d.CommercialId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_memberprofileendedcommercials_commercials_1111675008");
            });

            modelBuilder.Entity<MemberProfileExtensions>(entity =>
            {
                entity.HasKey(e => new { e.MemberProfileId, e.DemographicTypeId, e.DemographicValueId })
                    .HasName("MemberProfileExtensions_pkey");

                entity.ToTable("MemberProfileExtensions", "dbo");

                entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.Value).HasMaxLength(100);

                entity.HasOne(d => d.DemographicType)
                    .WithMany(p => p.MemberProfileExtensions)
                    .HasForeignKey(d => d.DemographicTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_memberprofileextensions_demographictypes1_1127675065");

                entity.HasOne(d => d.DemographicValue)
                    .WithMany(p => p.MemberProfileExtensions)
                    .HasForeignKey(d => d.DemographicValueId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_memberprofileextensions_demographicvalues1_1143675122");
            });

            modelBuilder.Entity<MemberProfileExtensions_Old>(entity =>
            {
                entity.HasKey(e => new { e.MemberProfileId, e.DemographicTypeId, e.DemographicValueId })
                    .HasName("MemberProfileExtensions_Old_pkey");

                entity.ToTable("MemberProfileExtensions_Old", "dbo");

                entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.Value).HasMaxLength(100);

                entity.HasOne(d => d.DemographicType)
                    .WithMany(p => p.MemberProfileExtensions_Old)
                    .HasForeignKey(d => d.DemographicTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_memberprofileextensions_demographictypes_1159675179");

                entity.HasOne(d => d.DemographicValue)
                    .WithMany(p => p.MemberProfileExtensions_Old)
                    .HasForeignKey(d => d.DemographicValueId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_memberprofileextensions_demographicvalues_1175675236");
            });

            modelBuilder.Entity<MemberProfileProductAdImpression>(entity =>
            {
                entity.ToTable("MemberProfileProductAdImpression", "dbo");

                entity.Property(e => e.Id).UseIdentityAlwaysColumn();

                entity.Property(e => e.CTR).HasPrecision(18, 4);

                entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");
            });

            modelBuilder.Entity<MemberProfileProductAdImpressionExtensions>(entity =>
            {
                entity.ToTable("MemberProfileProductAdImpressionExtensions", "dbo");

                entity.Property(e => e.Id).UseIdentityAlwaysColumn();

                entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");

                entity.HasOne(d => d.MemberProfileProductAdImpression)
                    .WithMany(p => p.MemberProfileProductAdImpressionExtensions)
                    .HasForeignKey(d => d.MemberProfileProductAdImpressionId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_memberprofileproductadimpressionextensions_memberprofileprod");
            });

            modelBuilder.Entity<MemberProfileSubgroups>(entity =>
            {
                entity.HasKey(e => new { e.SubgroupId, e.MemberProfileId })
                    .HasName("MemberProfileSubgroups_pkey");

                entity.ToTable("MemberProfileSubgroups", "dbo");

                entity.HasOne(d => d.Subgroup)
                    .WithMany(p => p.MemberProfileSubgroups)
                    .HasForeignKey(d => d.SubgroupId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_memberprofilesubgroups_subgroups_1207675350");
            });

            modelBuilder.Entity<MemberProfileVisits>(entity =>
            {
                entity.ToTable("MemberProfileVisits", "dbo");

                entity.Property(e => e.Id).UseIdentityAlwaysColumn();

                entity.Property(e => e.CommercialCloseLastUpdatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.LastUpdatedAt).HasColumnType("timestamp without time zone");
            });

            modelBuilder.Entity<MemberProfiles>(entity =>
            {
                entity.ToTable("MemberProfiles", "dbo");

                entity.Property(e => e.Id)
                    .UseIdentityAlwaysColumn()
                    .HasIdentityOptions(116746509L);

                entity.Property(e => e.AddressLine1).HasMaxLength(50);

                entity.Property(e => e.AddressLine2).HasMaxLength(50);

                entity.Property(e => e.AddressLine3).HasMaxLength(50);

                entity.Property(e => e.City).HasMaxLength(50);

                entity.Property(e => e.CloseClickLastUpdatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.CreatedAtCountryCode).HasMaxLength(4);

                entity.Property(e => e.CreatedAtIP).HasMaxLength(50);

                entity.Property(e => e.DateOfBirth).HasColumnType("timestamp without time zone");

                entity.Property(e => e.DemographicLastUpdatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.Email).HasMaxLength(255);

                entity.Property(e => e.Guid).HasMaxLength(36);

                entity.Property(e => e.LastCommercialRequestedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.LastRespondedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.Postcode).HasMaxLength(10);

                entity.Property(e => e.SyncMemberProfileToken).HasMaxLength(255);
            });

            modelBuilder.Entity<MemberProfiles_Old>(entity =>
            {
                entity.ToTable("MemberProfiles_Old", "dbo");

                entity.Property(e => e.Id).UseIdentityAlwaysColumn();

                entity.Property(e => e.AddressLine1).HasMaxLength(50);

                entity.Property(e => e.AddressLine2).HasMaxLength(50);

                entity.Property(e => e.AddressLine3).HasMaxLength(50);

                entity.Property(e => e.City).HasMaxLength(50);

                entity.Property(e => e.CloseClickLastUpdatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.CreatedAtCountryCode).HasMaxLength(4);

                entity.Property(e => e.DateOfBirth).HasColumnType("timestamp without time zone");

                entity.Property(e => e.DemographicLastUpdatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.Email).HasMaxLength(255);

                entity.Property(e => e.Guid).HasMaxLength(36);

                entity.Property(e => e.LastCommercialRequestedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.LastRespondedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.Postcode).HasMaxLength(10);

                entity.Property(e => e.SyncMemberProfileToken).HasMaxLength(255);
            });

            modelBuilder.Entity<MemberPsychographicAnswers>(entity =>
            {
                entity.ToTable("MemberPsychographicAnswers", "dbo");

                entity.Property(e => e.Id)
                    .UseIdentityAlwaysColumn()
                    .HasIdentityOptions(5019L);

                entity.HasOne(d => d.MemberPsy)
                    .WithMany(p => p.MemberPsychographicAnswers)
                    .HasForeignKey(d => d.MemberPsyId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_memberpsychographicanswers_memberpsychographics_1223675407");
            });

            modelBuilder.Entity<MemberPsychographics>(entity =>
            {
                entity.ToTable("MemberPsychographics", "dbo");

                entity.Property(e => e.Id)
                    .UseIdentityAlwaysColumn()
                    .HasIdentityOptions(2867L);

                entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");

                entity.HasOne(d => d.PsyQuestion)
                    .WithMany(p => p.MemberPsychographics)
                    .HasForeignKey(d => d.PsyQuestionId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_memberpsychographics_psyquestions_1239675464");
            });

            modelBuilder.Entity<MobileDevices>(entity =>
            {
                entity.HasKey(e => e.VodusId)
                    .HasName("MobileDevices_pkey");

                entity.ToTable("MobileDevices", "dbo");

                entity.Property(e => e.VodusId).ValueGeneratedNever();

                entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.DeviceId).HasMaxLength(255);

                entity.Property(e => e.LastUpdatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.Token).HasMaxLength(255);
            });

            modelBuilder.Entity<OrderItemRedemptions>(entity =>
            {
                entity.ToTable("OrderItemRedemptions", "dbo");

                entity.Property(e => e.Id).UseIdentityAlwaysColumn();

                entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.UniqueCode).HasMaxLength(128);

                entity.HasOne(d => d.OrderItem)
                    .WithMany(p => p.OrderItemRedemptions)
                    .HasForeignKey(d => d.OrderItemId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_orderitemredemptions_orderitems_1255675521");
            });

            modelBuilder.Entity<OrderItems>(entity =>
            {
                entity.ToTable("OrderItems", "dbo");

                entity.Property(e => e.Id).UseIdentityAlwaysColumn();

                entity.Property(e => e.ProductName).HasMaxLength(255);

                entity.Property(e => e.ProductTypeName).HasMaxLength(255);

                entity.Property(e => e.ThirdPartyRedemptionUrl).HasMaxLength(255);

                entity.Property(e => e.ThirdPartyResponse).HasMaxLength(1000);

                entity.HasOne(d => d.Order)
                    .WithMany(p => p.OrderItems)
                    .HasForeignKey(d => d.OrderId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_orderitems_orders_1271675578");

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.OrderItems)
                    .HasForeignKey(d => d.ProductId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_orderitems_products_1287675635");
            });

            modelBuilder.Entity<OrderRedemptions>(entity =>
            {
                entity.ToTable("OrderRedemptions", "dbo");

                entity.Property(e => e.Id).UseIdentityAlwaysColumn();

                entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.UniqueCode).HasMaxLength(128);

                entity.HasOne(d => d.OrderItem)
                    .WithMany(p => p.OrderRedemptions)
                    .HasForeignKey(d => d.OrderItemId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_orderredemptions_orderitems_1303675692");
            });

            modelBuilder.Entity<Orders>(entity =>
            {
                entity.ToTable("Orders", "dbo");

                entity.Property(e => e.Id).UseIdentityAlwaysColumn();

                entity.Property(e => e.AddressLine1).HasMaxLength(255);

                entity.Property(e => e.AddressLine2).HasMaxLength(255);

                entity.Property(e => e.City).HasMaxLength(255);

                entity.Property(e => e.ContactEmail).HasMaxLength(255);

                entity.Property(e => e.ContactMobileCountryCode).HasMaxLength(10);

                entity.Property(e => e.ContactMobileNumber).HasMaxLength(20);

                entity.Property(e => e.ContactPersonFirstName).HasMaxLength(255);

                entity.Property(e => e.ContactPersonLastName).HasMaxLength(255);

                entity.Property(e => e.CountryName).HasMaxLength(255);

                entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.DeliveryLastUpdated).HasColumnType("timestamp without time zone");

                entity.Property(e => e.DeliveryLastUpdatedBy).HasMaxLength(128);

                entity.Property(e => e.DeliveryRemark).HasMaxLength(255);

                entity.Property(e => e.Postcode).HasMaxLength(15);

                entity.Property(e => e.State).HasMaxLength(255);

                entity.HasOne(d => d.MasterMember)
                    .WithMany(p => p.Orders)
                    .HasForeignKey(d => d.MasterMemberId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_orders_mastermemberprofiles_1319675749");
            });

            modelBuilder.Entity<PartitionGrouping>(entity =>
            {
                entity.ToTable("PartitionGrouping", "dbo");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.LastUpdatedAt).HasColumnType("timestamp without time zone");
            });

            modelBuilder.Entity<PartnerProfiles>(entity =>
            {
                entity.ToTable("PartnerProfiles", "dbo");

                entity.Property(e => e.Id).UseIdentityAlwaysColumn();

                entity.Property(e => e.UserId).HasMaxLength(128);

                entity.HasOne(d => d.User)
                    .WithMany(p => p.PartnerProfiles)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_partnerprofiles_users_1335675806");
            });

            modelBuilder.Entity<PartnerWebsiteCommercialSubgroups>(entity =>
            {
                entity.ToTable("PartnerWebsiteCommercialSubgroups", "dbo");

                entity.Property(e => e.Id).UseIdentityAlwaysColumn();

                entity.Property(e => e.CostPerResponse).HasPrecision(18, 2);

                entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.LastUpdatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.TotalCost).HasPrecision(18, 2);
            });

            modelBuilder.Entity<PartnerWebsiteExcludedCommercials>(entity =>
            {
                entity.ToTable("PartnerWebsiteExcludedCommercials", "dbo");

                entity.Property(e => e.Id).UseIdentityAlwaysColumn();

                entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.CreatedBy).HasMaxLength(128);

                entity.Property(e => e.Description).HasMaxLength(255);

                entity.HasOne(d => d.Commercial)
                    .WithMany(p => p.PartnerWebsiteExcludedCommercials)
                    .HasForeignKey(d => d.CommercialId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_partnerwebsiteexcludedcommercials_commercials_1351675863");
            });

            modelBuilder.Entity<PartnerWebsitePriorityCommercials>(entity =>
            {
                entity.ToTable("PartnerWebsitePriorityCommercials", "dbo");

                entity.Property(e => e.Id).UseIdentityAlwaysColumn();

                entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.CreatedBy).HasMaxLength(128);

                entity.Property(e => e.Description).HasMaxLength(255);

                entity.HasOne(d => d.Commercial)
                    .WithMany(p => p.PartnerWebsitePriorityCommercials)
                    .HasForeignKey(d => d.CommercialId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_partnerwebsiteprioritycommercials_commercials_1367675920");
            });

            modelBuilder.Entity<PartnerWebsiteVisits>(entity =>
            {
                entity.ToTable("PartnerWebsiteVisits", "dbo");

                entity.Property(e => e.Id).UseIdentityAlwaysColumn();

                entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.LastUpdatedAt).HasColumnType("timestamp without time zone");

                entity.HasOne(d => d.DemographicValue)
                    .WithMany(p => p.PartnerWebsiteVisits)
                    .HasForeignKey(d => d.DemographicValueId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_partnerwebsitevisits_demographicvalues_1399676034");

                entity.HasOne(d => d.PartnerWebsites)
                    .WithMany(p => p.PartnerWebsiteVisits)
                    .HasForeignKey(d => d.PartnerWebsitesId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("PartnerWebsiteVisits_PartnerWebsitesId_fkey");
            });

            modelBuilder.Entity<PartnerWebsites>(entity =>
            {
                entity.ToTable("PartnerWebsites", "dbo");

                entity.Property(e => e.Id).UseIdentityAlwaysColumn();

                entity.Property(e => e.BackgroundColor).HasMaxLength(8);

                entity.Property(e => e.ButtonColor).HasMaxLength(8);

                entity.Property(e => e.CatfishPosition).HasMaxLength(36);

                entity.Property(e => e.Code).HasMaxLength(36);

                entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.CreatedBy).HasMaxLength(128);

                entity.Property(e => e.DMPCode).HasMaxLength(10);

                entity.Property(e => e.DMPTargetCode).HasMaxLength(255);

                entity.Property(e => e.Description).HasMaxLength(255);

                entity.Property(e => e.DmpUrl).HasMaxLength(255);

                entity.Property(e => e.FontColor).HasMaxLength(8);

                entity.Property(e => e.Language).HasMaxLength(36);

                entity.Property(e => e.LogoUrl).HasMaxLength(255);

                entity.Property(e => e.Name).HasMaxLength(255);

                entity.Property(e => e.PrimaryColor).HasMaxLength(8);

                entity.Property(e => e.SecondaryColor).HasMaxLength(8);

                entity.Property(e => e.SelectedAnswerColor).HasMaxLength(8);

                entity.Property(e => e.Url).HasMaxLength(255);

                entity.HasOne(d => d.Partner)
                    .WithMany(p => p.PartnerWebsites)
                    .HasForeignKey(d => d.PartnerId)
                    .HasConstraintName("fk_partnerwebsites_partners_1383675977");
            });

            modelBuilder.Entity<Partners>(entity =>
            {
                entity.ToTable("Partners", "dbo");

                entity.Property(e => e.Id).UseIdentityAlwaysColumn();

                entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.CreatedBy).HasMaxLength(128);

                entity.Property(e => e.DataSyncAccessKey).HasMaxLength(128);

                entity.Property(e => e.DataSyncAccessSecret).HasMaxLength(128);

                entity.Property(e => e.DataSyncContainerName).HasMaxLength(128);

                entity.Property(e => e.DataSyncUrl).HasMaxLength(128);

                entity.Property(e => e.LogoUrl).HasMaxLength(128);

                entity.Property(e => e.Name).HasMaxLength(255);
            });

            modelBuilder.Entity<PasswordResets>(entity =>
            {
                entity.ToTable("PasswordResets", "dbo");

                entity.Property(e => e.Id).UseIdentityAlwaysColumn();

                entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.Email).HasMaxLength(128);

                entity.Property(e => e.ExpireAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.ResetCode).HasMaxLength(128);
            });

            modelBuilder.Entity<PaymentStatuses>(entity =>
            {
                entity.ToTable("PaymentStatuses", "dbo");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Description).HasMaxLength(50);
            });

            modelBuilder.Entity<Permissions>(entity =>
            {
                entity.ToTable("Permissions", "dbo");

                entity.Property(e => e.Id).UseIdentityAlwaysColumn();

                entity.Property(e => e.Name).HasMaxLength(50);
            });

            modelBuilder.Entity<ProductAdLocations>(entity =>
            {
                entity.ToTable("ProductAdLocations", "dbo");

                entity.Property(e => e.Id).UseIdentityAlwaysColumn();

                entity.Property(e => e.CTR).HasPrecision(18, 4);

                entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.ProvinceName).HasMaxLength(100);

                entity.HasOne(d => d.ProductAd)
                    .WithMany(p => p.ProductAdLocations)
                    .HasForeignKey(d => d.ProductAdId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_productadlocations_productadlocations_1415676091");
            });

            modelBuilder.Entity<ProductAdPartnersDomain>(entity =>
            {
                entity.ToTable("ProductAdPartnersDomain", "dbo");

                entity.Property(e => e.Id).UseIdentityAlwaysColumn();

                entity.Property(e => e.CTR).HasPrecision(18, 4);

                entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");

                entity.HasOne(d => d.Partner)
                    .WithMany(p => p.ProductAdPartnersDomain)
                    .HasForeignKey(d => d.PartnerId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_productadpartnersdomain_partners_1431676148");

                entity.HasOne(d => d.ProductAd)
                    .WithMany(p => p.ProductAdPartnersDomain)
                    .HasForeignKey(d => d.ProductAdId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_productadpartnersdomain_productads_1447676205");
            });

            modelBuilder.Entity<ProductAdPartnersDomainWebsites>(entity =>
            {
                entity.ToTable("ProductAdPartnersDomainWebsites", "dbo");

                entity.Property(e => e.Id).UseIdentityAlwaysColumn();

                entity.Property(e => e.CTR).HasPrecision(18, 4);

                entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");

                entity.HasOne(d => d.PartnerWebsite)
                    .WithMany(p => p.ProductAdPartnersDomainWebsites)
                    .HasForeignKey(d => d.PartnerWebsiteId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_productadpartnersdomainwebsites_partnerwebsites_1463676262");

                entity.HasOne(d => d.ProductAdPartnersDomain)
                    .WithMany(p => p.ProductAdPartnersDomainWebsites)
                    .HasForeignKey(d => d.ProductAdPartnersDomainId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_productadpartnersdomainwebsites_productadpartnersdomain_1479");
            });

            modelBuilder.Entity<ProductAdSubgroups>(entity =>
            {
                entity.ToTable("ProductAdSubgroups", "dbo");

                entity.Property(e => e.Id).UseIdentityAlwaysColumn();

                entity.Property(e => e.CTR).HasPrecision(18, 4);

                entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.SubgroupId).HasMaxLength(50);

                entity.HasOne(d => d.ProductAd)
                    .WithMany(p => p.ProductAdSubgroups)
                    .HasForeignKey(d => d.ProductAdId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_productadsubgroups_productads_1495676376");

                entity.HasOne(d => d.Subgroup)
                    .WithMany(p => p.ProductAdSubgroups)
                    .HasForeignKey(d => d.SubgroupId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_productadsubgroups_subgroupsv2_1511676433");
            });

            modelBuilder.Entity<ProductAds>(entity =>
            {
                entity.ToTable("ProductAds", "dbo");

                entity.Property(e => e.Id).UseIdentityAlwaysColumn();

                entity.Property(e => e.CTR).HasPrecision(18, 4);

                entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");
            });

            modelBuilder.Entity<ProductAdsConfig>(entity =>
            {
                entity.ToTable("ProductAdsConfig", "dbo");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.LastUpdatedAt).HasColumnType("timestamp without time zone");
            });

            modelBuilder.Entity<Products>(entity =>
            {
                entity.ToTable("Products", "dbo");

                entity.Property(e => e.Id).UseIdentityAlwaysColumn();

                entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.CreatedBy).HasMaxLength(255);

                entity.Property(e => e.LuckyDrawDate).HasColumnType("timestamp without time zone");

                entity.Property(e => e.Name).HasMaxLength(1000);

                entity.Property(e => e.ProductImageUrl).HasMaxLength(255);

                entity.Property(e => e.ProductSummary).HasMaxLength(1000);

                entity.Property(e => e.ProductThumbnailImageUrl).HasMaxLength(255);
            });

            modelBuilder.Entity<PromoCodes>(entity =>
            {
                entity.ToTable("PromoCodes", "dbo");

                entity.Property(e => e.Id).UseIdentityAlwaysColumn();

                entity.Property(e => e.Code).HasMaxLength(128);

                entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.CreatedBy).HasMaxLength(128);

                entity.Property(e => e.DepositRequiredPercentage).HasPrecision(18, 2);

                entity.Property(e => e.Description).HasMaxLength(500);

                entity.Property(e => e.FinalPaymentDiscountPercentage).HasPrecision(18, 2);
            });

            modelBuilder.Entity<PsyQuestionAnswers>(entity =>
            {
                entity.ToTable("PsyQuestionAnswers", "dbo");

                entity.Property(e => e.Id).UseIdentityAlwaysColumn();

                entity.Property(e => e.AnswerValue).HasMaxLength(128);

                entity.Property(e => e.TemplateUrl).HasMaxLength(200);

                entity.HasOne(d => d.PsyQuestion)
                    .WithMany(p => p.PsyQuestionAnswers)
                    .HasForeignKey(d => d.PsyQuestionId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_psyquestionanswers_psyquestions_1527676490");
            });

            modelBuilder.Entity<PsyQuestionMultiLanguagesAnswers>(entity =>
            {
                entity.ToTable("PsyQuestionMultiLanguagesAnswers", "dbo");

                entity.Property(e => e.Id).UseIdentityAlwaysColumn();

                entity.Property(e => e.AnswerValue).HasMaxLength(1000);

                entity.Property(e => e.LanguageCode)
                    .HasMaxLength(2)
                    .IsFixedLength();
            });

            modelBuilder.Entity<PsyQuestions>(entity =>
            {
                entity.ToTable("PsyQuestions", "dbo");

                entity.Property(e => e.Id).UseIdentityAlwaysColumn();

                entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.CreatedBy).HasMaxLength(128);

                entity.Property(e => e.QuestionTemplateContentMobileUrl).HasMaxLength(200);

                entity.Property(e => e.QuestionTemplateContentUrl).HasMaxLength(200);

                entity.Property(e => e.Remark).HasMaxLength(128);

                entity.Property(e => e.TemplateContentUrl).HasMaxLength(200);

                entity.Property(e => e.Title).HasMaxLength(255);
            });

            modelBuilder.Entity<QuestionDelays>(entity =>
            {
                entity.ToTable("QuestionDelays", "dbo");

                entity.Property(e => e.Id).UseIdentityAlwaysColumn();

                entity.Property(e => e.CostPrice).HasPrecision(19, 4);

                entity.Property(e => e.Point).HasPrecision(10, 4);
            });

            modelBuilder.Entity<QuestionTypes>(entity =>
            {
                entity.ToTable("QuestionTypes", "dbo");

                entity.Property(e => e.Id).UseIdentityAlwaysColumn();

                entity.Property(e => e.CostPrice).HasPrecision(19, 4);

                entity.Property(e => e.Description).HasMaxLength(50);

                entity.Property(e => e.Point).HasPrecision(10, 4);

                entity.Property(e => e.Type).HasMaxLength(50);
            });

            modelBuilder.Entity<RequestForDownloads>(entity =>
            {
                entity.ToTable("RequestForDownloads", "dbo");

                entity.Property(e => e.Id).UseIdentityAlwaysColumn();

                entity.Property(e => e.CompanyName).HasMaxLength(255);

                entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.CreatedBy).HasMaxLength(128);

                entity.Property(e => e.DownloadLink).HasMaxLength(128);

                entity.Property(e => e.Email).HasMaxLength(255);

                entity.Property(e => e.Name).HasMaxLength(255);

                entity.HasOne(d => d.Article)
                    .WithMany(p => p.RequestForDownloads)
                    .HasForeignKey(d => d.ArticleId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_requestfordownloads_articles_1543676547");
            });

            modelBuilder.Entity<RequestHeaderKeyValues>(entity =>
            {
                entity.ToTable("RequestHeaderKeyValues", "dbo");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Name).HasMaxLength(255);
            });

            modelBuilder.Entity<RequestHeaders>(entity =>
            {
                entity.ToTable("RequestHeaders", "dbo");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.CreatedAt).HasColumnType("timestamp(6) without time zone");

                entity.Property(e => e.LastUpdatedAt).HasColumnType("timestamp(6) without time zone");

                entity.Property(e => e.LastUsedAt).HasColumnType("timestamp(6) without time zone");

                entity.Property(e => e.Name).HasMaxLength(255);
            });

            modelBuilder.Entity<RoleClaims>(entity =>
            {
                entity.ToTable("RoleClaims", "dbo");

                entity.Property(e => e.Id).UseIdentityAlwaysColumn();

                entity.Property(e => e.RoleId).HasMaxLength(128);

                entity.HasOne(d => d.Role)
                    .WithMany(p => p.RoleClaims)
                    .HasForeignKey(d => d.RoleId)
                    .HasConstraintName("fk_roleclaims_roles_roleid_1575676661");
            });

            modelBuilder.Entity<Roles>(entity =>
            {
                entity.ToTable("Roles", "dbo");

                entity.Property(e => e.Id).HasMaxLength(128);

                entity.Property(e => e.ConcurrencyStamp).HasMaxLength(255);

                entity.Property(e => e.Name).HasMaxLength(256);

                entity.Property(e => e.NormalizedName).HasMaxLength(255);
            });

            modelBuilder.Entity<States>(entity =>
            {
                entity.ToTable("States", "dbo");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Code).HasMaxLength(50);

                entity.Property(e => e.Name).HasMaxLength(200);

                entity.Property(e => e.Name2).HasMaxLength(200);

                entity.HasOne(d => d.Country)
                    .WithMany(p => p.States)
                    .HasForeignKey(d => d.CountryId)
                    .HasConstraintName("fk_states_countries_1591676718");
            });

            modelBuilder.Entity<StatusTypes>(entity =>
            {
                entity.ToTable("StatusTypes", "dbo");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Description).HasMaxLength(50);

                entity.Property(e => e.Name).HasMaxLength(50);
            });

            modelBuilder.Entity<SubgroupMultipliers>(entity =>
            {
                entity.ToTable("SubgroupMultipliers", "dbo");

                entity.Property(e => e.Id).UseIdentityAlwaysColumn();

                entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.LastUpdatedAt).HasColumnType("timestamp without time zone");

                entity.HasMany(d => d.DemographicValue)
                    .WithMany(p => p.SubgroupMultiplier)
                    .UsingEntity<Dictionary<string, object>>(
                        "SubgroupMultiplierDemographics",
                        l => l.HasOne<DemographicValues>().WithMany().HasForeignKey("DemographicValueId").OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("fk_subgroupmultiplierdemographics_demographicvalues_1639676889"),
                        r => r.HasOne<SubgroupMultipliers>().WithMany().HasForeignKey("SubgroupMultiplierId").OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("fk_subgroupmultiplierdemographics_subgroupmultiplers_1655676946"),
                        j =>
                        {
                            j.HasKey("SubgroupMultiplierId", "DemographicValueId").HasName("SubgroupMultiplierDemographics_pkey");

                            j.ToTable("SubgroupMultiplierDemographics", "dbo");
                        });
            });

            modelBuilder.Entity<Subgroups>(entity =>
            {
                entity.ToTable("Subgroups", "dbo");

                entity.Property(e => e.Id).UseIdentityAlwaysColumn();

                entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.LastUpdatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.Name).HasMaxLength(64);

                entity.Property(e => e.R).HasPrecision(18, 3);

                entity.Property(e => e.RLastUpdatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.RecordValue).HasPrecision(18, 2);

                entity.HasMany(d => d.DemographicValue)
                    .WithMany(p => p.Subgroup)
                    .UsingEntity<Dictionary<string, object>>(
                        "SubgroupDemographicValues",
                        l => l.HasOne<DemographicValues>().WithMany().HasForeignKey("DemographicValueId").OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("fk_subgroupdemographicvalues_demographicvalues_1623676832"),
                        r => r.HasOne<Subgroups>().WithMany().HasForeignKey("SubgroupId").OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("fk__subgroupd__subgr__7f6bda51_1607676775"),
                        j =>
                        {
                            j.HasKey("SubgroupId", "DemographicValueId").HasName("SubgroupDemographicValues_pkey");

                            j.ToTable("SubgroupDemographicValues", "dbo");
                        });
            });

            modelBuilder.Entity<SubgroupsV2>(entity =>
            {
                entity.ToTable("SubgroupsV2", "dbo");

                entity.Property(e => e.Id).HasMaxLength(50);

                entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.LastUpdatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.R).HasPrecision(18, 3);

                entity.Property(e => e.RLastUpdatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.RecordValue).HasPrecision(18, 2);
            });

            modelBuilder.Entity<SurveyCampaigns>(entity =>
            {
                entity.ToTable("SurveyCampaigns", "dbo");

                entity.Property(e => e.Id).UseIdentityAlwaysColumn();

                entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.CreatedBy).HasMaxLength(256);

                entity.Property(e => e.Name).HasMaxLength(255);

                entity.Property(e => e.UpdatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.UpdatedBy).HasMaxLength(256);

                entity.HasOne(d => d.ClientProfile)
                    .WithMany(p => p.SurveyCampaigns)
                    .HasForeignKey(d => d.ClientProfileId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_surveycampaigns_clientprofiles_1671677003");
            });

            modelBuilder.Entity<SurveyChunkings>(entity =>
            {
                entity.ToTable("SurveyChunkings", "dbo");

                entity.Property(e => e.Id).UseIdentityAlwaysColumn();

                entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.CreatedBy).HasMaxLength(128);

                entity.Property(e => e.JsonFileUrl).HasMaxLength(255);

                entity.Property(e => e.LastUpdatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.LastUpdatedBy).HasMaxLength(128);

                entity.Property(e => e.Name).HasMaxLength(50);

                entity.Property(e => e.TotalResponseCost).HasPrecision(18, 2);

                entity.HasOne(d => d.Commercial)
                    .WithMany(p => p.SurveyChunkings)
                    .HasForeignKey(d => d.CommercialId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_surveychunkings_commercials_1687677060");
            });

            modelBuilder.Entity<SurveyLogicFlows>(entity =>
            {
                entity.HasKey(e => new { e.SurveyLogicFlowId, e.SurveyQuestionAnswerId, e.ToSurveyQuestionId })
                    .HasName("SurveyLogicFlows_pkey");

                entity.ToTable("SurveyLogicFlows", "dbo");

                entity.Property(e => e.SurveyLogicFlowId)
                    .ValueGeneratedOnAdd()
                    .UseIdentityAlwaysColumn();

                entity.HasOne(d => d.SurveyQuestionAnswer)
                    .WithMany(p => p.SurveyLogicFlows)
                    .HasForeignKey(d => d.SurveyQuestionAnswerId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_surveylogicflows_surveyquestionanswers_1703677117");

                entity.HasOne(d => d.ToSurveyQuestion)
                    .WithMany(p => p.SurveyLogicFlows)
                    .HasForeignKey(d => d.ToSurveyQuestionId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_surveylogicflows_surveyquestions_1719677174");
            });

            modelBuilder.Entity<SurveyQuestionAnswers>(entity =>
            {
                entity.ToTable("SurveyQuestionAnswers", "dbo");

                entity.Property(e => e.Id).UseIdentityAlwaysColumn();

                entity.Property(e => e.AnswerValue).HasMaxLength(1000);

                entity.Property(e => e.RedirectUrl).HasMaxLength(255);

                entity.HasOne(d => d.SurveyQuestion)
                    .WithMany(p => p.SurveyQuestionAnswers)
                    .HasForeignKey(d => d.SurveyQuestionId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_surveyquestionanswers_surveyquestions_1735677231");
            });

            modelBuilder.Entity<SurveyQuestionChucks>(entity =>
            {
                entity.ToTable("SurveyQuestionChucks", "dbo");

                entity.Property(e => e.Id).UseIdentityAlwaysColumn();

                entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.CreatedBy).HasMaxLength(128);

                entity.Property(e => e.LastUpdatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.LastUpdatedby).HasMaxLength(128);

                entity.Property(e => e.Name).HasMaxLength(128);
            });

            modelBuilder.Entity<SurveyQuestionMultiLanguagesAnswers>(entity =>
            {
                entity.ToTable("SurveyQuestionMultiLanguagesAnswers", "dbo");

                entity.Property(e => e.Id).UseIdentityAlwaysColumn();

                entity.Property(e => e.AnswerValue).HasMaxLength(1000);

                entity.Property(e => e.LanguageCode)
                    .HasMaxLength(2)
                    .IsFixedLength();

                entity.HasOne(d => d.SurveyQuestion)
                    .WithMany(p => p.SurveyQuestionMultiLanguagesAnswers)
                    .HasForeignKey(d => d.SurveyQuestionId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_surveyquestionmultilanguagesanswers_surveyquestions_17516772");
            });

            modelBuilder.Entity<SurveyQuestionMultiLanguagesTitles>(entity =>
            {
                entity.ToTable("SurveyQuestionMultiLanguagesTitles", "dbo");

                entity.Property(e => e.Id).UseIdentityAlwaysColumn();

                entity.Property(e => e.LanguageCode)
                    .HasMaxLength(2)
                    .IsFixedLength();

                entity.HasOne(d => d.SurveyQuestion)
                    .WithMany(p => p.SurveyQuestionMultiLanguagesTitles)
                    .HasForeignKey(d => d.SurveyQuestionId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_surveyquestionmultilanguagestitles_surveyquestions_176767734");
            });

            modelBuilder.Entity<SurveyQuestionTemplates>(entity =>
            {
                entity.HasKey(e => new { e.SurveyTemplateId, e.QuestionTypeId })
                    .HasName("SurveyQuestionTemplates_pkey");

                entity.ToTable("SurveyQuestionTemplates", "dbo");

                entity.HasOne(d => d.QuestionType)
                    .WithMany(p => p.SurveyQuestionTemplates)
                    .HasForeignKey(d => d.QuestionTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_surveyquestiontemplates_questiontypes_1879677744");

                entity.HasOne(d => d.SurveyTemplate)
                    .WithMany(p => p.SurveyQuestionTemplates)
                    .HasForeignKey(d => d.SurveyTemplateId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_surveyquestiontemplates_surveytemplates_1895677801");
            });

            modelBuilder.Entity<SurveyQuestionTiers>(entity =>
            {
                entity.ToTable("SurveyQuestionTiers", "dbo");

                entity.Property(e => e.Id).UseIdentityAlwaysColumn();

                entity.Property(e => e.CostPrice).HasPrecision(19, 4);
            });

            modelBuilder.Entity<SurveyQuestions>(entity =>
            {
                entity.ToTable("SurveyQuestions", "dbo");

                entity.Property(e => e.Id).UseIdentityAlwaysColumn();

                entity.Property(e => e.ChainedPrice).HasPrecision(19, 4);

                entity.Property(e => e.PipeCode).HasMaxLength(10);

                entity.Property(e => e.Point).HasPrecision(10, 4);

                entity.Property(e => e.QuestionTitle).HasMaxLength(1000);

                entity.Property(e => e.QuestionTypePrice).HasPrecision(19, 4);

                entity.HasOne(d => d.Chunk)
                    .WithMany(p => p.SurveyQuestions)
                    .HasForeignKey(d => d.ChunkId)
                    .HasConstraintName("fk_surveyquestions_chunk_1783677402");

                entity.HasOne(d => d.Commercial)
                    .WithMany(p => p.SurveyQuestions)
                    .HasForeignKey(d => d.CommercialId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_surveyquestions_commercials_1799677459");

                entity.HasOne(d => d.DemographicType)
                    .WithMany(p => p.SurveyQuestions)
                    .HasForeignKey(d => d.DemographicTypeId)
                    .HasConstraintName("fk_surveyquestions_demographictypes_1815677516");

                entity.HasOne(d => d.QuestionDelay)
                    .WithMany(p => p.SurveyQuestions)
                    .HasForeignKey(d => d.QuestionDelayId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_surveyquestions_questiondelays_1831677573");

                entity.HasOne(d => d.QuestionType)
                    .WithMany(p => p.SurveyQuestions)
                    .HasForeignKey(d => d.QuestionTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_surveyquestions_questiontypes_1847677630");

                entity.HasOne(d => d.SurveyQuestionTier)
                    .WithMany(p => p.SurveyQuestions)
                    .HasForeignKey(d => d.SurveyQuestionTierId)
                    .HasConstraintName("fk_surveyquestions_surveyquestiontiers_1863677687");
            });

            modelBuilder.Entity<SurveyResponseAnswers>(entity =>
            {
                entity.ToTable("SurveyResponseAnswers", "dbo");

                entity.Property(e => e.Id).UseIdentityAlwaysColumn();

                entity.Property(e => e.OtherAnswer).HasMaxLength(255);

                entity.HasOne(d => d.SurveyQuestionAnswer)
                    .WithMany(p => p.SurveyResponseAnswers)
                    .HasForeignKey(d => d.SurveyQuestionAnswerId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_surveyresponseanswers_surveyquestionanswers_1911677858");

                entity.HasOne(d => d.SurveyQuestion)
                    .WithMany(p => p.SurveyResponseAnswers)
                    .HasForeignKey(d => d.SurveyQuestionId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_surveyresponseanswers_surveyquestions_1927677915");

                entity.HasOne(d => d.SurveyResponse)
                    .WithMany(p => p.SurveyResponseAnswers)
                    .HasForeignKey(d => d.SurveyResponseId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_surveyresponseanswers_surveyresponses_1943677972");
            });

            modelBuilder.Entity<SurveyResponseDemographic>(entity =>
            {
                entity.ToTable("SurveyResponseDemographic", "dbo");

                entity.Property(e => e.Id).UseIdentityAlwaysColumn();

                entity.Property(e => e.Value).HasMaxLength(1000);

                entity.HasOne(d => d.DemographicType)
                    .WithMany(p => p.SurveyResponseDemographic)
                    .HasForeignKey(d => d.DemographicTypeId)
                    .HasConstraintName("fk_surveyresponsedemographic_demographictypes_1959678029");

                entity.HasOne(d => d.SurveyResponse)
                    .WithMany(p => p.SurveyResponseDemographic)
                    .HasForeignKey(d => d.SurveyResponseId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_surveyresponsedemographic_surveyresponses_1975678086");
            });

            modelBuilder.Entity<SurveyResponses>(entity =>
            {
                entity.ToTable("SurveyResponses", "dbo");

                entity.Property(e => e.Id).UseIdentityAlwaysColumn();

                entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.Host).HasMaxLength(255);

                entity.Property(e => e.IpAddress).HasMaxLength(45);

                entity.Property(e => e.MemberDemographic).HasMaxLength(500);

                entity.Property(e => e.Origin).HasMaxLength(255);

                entity.Property(e => e.PartnerCode).HasMaxLength(36);

                entity.Property(e => e.Referer).HasMaxLength(255);

                entity.Property(e => e.UserAgent).HasMaxLength(500);

                entity.HasOne(d => d.Commercial)
                    .WithMany(p => p.SurveyResponses)
                    .HasForeignKey(d => d.CommercialId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_surveyresponses_commercials_1991678143");
            });

            modelBuilder.Entity<SurveyTemplates>(entity =>
            {
                entity.ToTable("SurveyTemplates", "dbo");

                entity.Property(e => e.Id).UseIdentityAlwaysColumn();

                entity.Property(e => e.Description).HasMaxLength(255);

                entity.Property(e => e.TemplateName).HasMaxLength(100);
            });

            modelBuilder.Entity<SystemConfigurations>(entity =>
            {
                entity.HasKey(e => e.Code)
                    .HasName("SystemConfigurations_pkey");

                entity.ToTable("SystemConfigurations", "dbo");

                entity.Property(e => e.Code).HasMaxLength(50);

                entity.Property(e => e.Description).HasMaxLength(255);
            });

            modelBuilder.Entity<TempTokens>(entity =>
            {
                entity.ToTable("TempTokens", "dbo");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.LastUpdatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.Token).HasMaxLength(255);
            });

            modelBuilder.Entity<Temp_CommercialFilters>(entity =>
            {
                entity.HasKey(e => new { e.CommercialId, e.SequenceNumber })
                    .HasName("Temp_CommercialFilters_pkey");

                entity.ToTable("Temp_CommercialFilters", "dbo");

                entity.HasOne(d => d.Commercial)
                    .WithMany(p => p.Temp_CommercialFilters)
                    .HasForeignKey(d => d.CommercialId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_temp_commercialfilters_commercials_2007678200");

                entity.HasOne(d => d.DemographicType)
                    .WithMany(p => p.Temp_CommercialFilters)
                    .HasForeignKey(d => d.DemographicTypeId)
                    .HasConstraintName("fk_temp_commercialfilters_demographictypes_2023678257");
            });

            modelBuilder.Entity<Temp_CommercialWeightedScoreDemographics>(entity =>
            {
                entity.HasKey(e => new { e.CommercialId, e.SequenceNumber, e.SubSequenceNumber, e.DemographicTypeId })
                    .HasName("Temp_CommercialWeightedScoreDemographics_pkey");

                entity.ToTable("Temp_CommercialWeightedScoreDemographics", "dbo");

                entity.Property(e => e.Value).HasMaxLength(1000);

                entity.Property(e => e.Value2).HasMaxLength(1000);

                entity.HasOne(d => d.Commercial)
                    .WithMany(p => p.Temp_CommercialWeightedScoreDemographics)
                    .HasForeignKey(d => d.CommercialId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_temp_commercialweightedscoredemographics_commercials_2039678");

                entity.HasOne(d => d.DemographicType)
                    .WithMany(p => p.Temp_CommercialWeightedScoreDemographics)
                    .HasForeignKey(d => d.DemographicTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_temp_commercialweightedscoredemographics_demographictypes_20");

                entity.HasOne(d => d.DemographicValue)
                    .WithMany(p => p.Temp_CommercialWeightedScoreDemographics)
                    .HasForeignKey(d => d.DemographicValueId)
                    .HasConstraintName("fk_temp_commercialweightedscoredemographics_demographicvalues_2");
            });

            modelBuilder.Entity<Temp_CommercialWeightedScores>(entity =>
            {
                entity.HasKey(e => new { e.CommercialId, e.SequenceNumber, e.SubSequenceNumber })
                    .HasName("Temp_CommercialWeightedScores_pkey");

                entity.ToTable("Temp_CommercialWeightedScores", "dbo");

                entity.HasOne(d => d.Commercial)
                    .WithMany(p => p.Temp_CommercialWeightedScores)
                    .HasForeignKey(d => d.CommercialId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_temp_commercialweightedscores_commercials_2087678485");
            });

            modelBuilder.Entity<Topics>(entity =>
            {
                entity.ToTable("Topics", "dbo");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Name).HasMaxLength(50);
            });

            modelBuilder.Entity<TreatedDataJsonFiles>(entity =>
            {
                entity.ToTable("TreatedDataJsonFiles", "dbo");

                entity.Property(e => e.Id).UseIdentityAlwaysColumn();

                entity.Property(e => e.JsonFileName).HasMaxLength(128);

                entity.Property(e => e.JsonFileUrl).HasMaxLength(128);

                entity.Property(e => e.UploadDate).HasColumnType("timestamp without time zone");

                entity.HasOne(d => d.Commercial)
                    .WithMany(p => p.TreatedDataJsonFiles)
                    .HasForeignKey(d => d.CommercialId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_treateddatajsonfiles_commercials_2103678542");
            });

            modelBuilder.Entity<UserClaims>(entity =>
            {
                entity.ToTable("UserClaims", "dbo");

                entity.Property(e => e.Id).UseIdentityAlwaysColumn();

                entity.Property(e => e.UserId).HasMaxLength(128);

                entity.HasOne(d => d.User)
                    .WithMany(p => p.UserClaims)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_dbo.UserClaims_dbo.Users_UserId_2119678599");
            });

            modelBuilder.Entity<UserLogins>(entity =>
            {
                entity.HasKey(e => new { e.LoginProvider, e.ProviderKey, e.UserId })
                    .HasName("UserLogins_pkey");

                entity.ToTable("UserLogins", "dbo");

                entity.Property(e => e.LoginProvider).HasMaxLength(128);

                entity.Property(e => e.ProviderKey).HasMaxLength(128);

                entity.Property(e => e.UserId).HasMaxLength(128);

                entity.Property(e => e.ProviderDisplayName).HasMaxLength(255);

                entity.HasOne(d => d.User)
                    .WithMany(p => p.UserLogins)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK_dbo.UserLogins_dbo.Users_UserId_2135678656");
            });

            modelBuilder.Entity<UserRoles>(entity =>
            {
                entity.ToTable("UserRoles", "dbo");

                entity.Property(e => e.Id)
                    .HasMaxLength(128)
                    .HasDefaultValueSql("gen_random_uuid()");

                entity.Property(e => e.RoleId).HasMaxLength(128);

                entity.Property(e => e.UserId).HasMaxLength(128);

                entity.HasOne(d => d.Role)
                    .WithMany(p => p.UserRoles)
                    .HasForeignKey(d => d.RoleId)
                    .HasConstraintName("FK_dbo.UserRoles_dbo.Roles_RoleId_4195065");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.UserRoles)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK_dbo.UserRoles_dbo.Users_UserId_20195122");
            });

            modelBuilder.Entity<UserTokens>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.LoginProvider, e.Name })
                    .HasName("UserTokens_pkey");

                entity.ToTable("UserTokens", "dbo");

                entity.Property(e => e.UserId).HasMaxLength(450);

                entity.Property(e => e.LoginProvider).HasMaxLength(450);

                entity.Property(e => e.Name).HasMaxLength(450);
            });

            modelBuilder.Entity<Users>(entity =>
            {
                entity.ToTable("Users", "dbo");

                entity.Property(e => e.Id).HasMaxLength(128);

                entity.Property(e => e.ActivatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.ActivationCode).HasMaxLength(36);

                entity.Property(e => e.AuthType).HasMaxLength(20);

                entity.Property(e => e.ConcurrencyStamp).HasMaxLength(255);

                entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.Email).HasMaxLength(256);

                entity.Property(e => e.FirstName).HasMaxLength(100);

                entity.Property(e => e.LastCheckedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.LastLoggedInAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.LastName).HasMaxLength(100);

                entity.Property(e => e.LockoutEnd).HasColumnType("timestamp without time zone");

                entity.Property(e => e.LockoutEndDateUtc).HasColumnType("timestamp without time zone");

                entity.Property(e => e.NewPendingVerificationEmail).HasMaxLength(256);

                entity.Property(e => e.NormalizedEmail).HasMaxLength(255);

                entity.Property(e => e.NormalizedUserName).HasMaxLength(255);

                entity.Property(e => e.SocialId).HasMaxLength(128);

                entity.Property(e => e.UserName).HasMaxLength(256);
            });

            modelBuilder.Entity<Variables>(entity =>
            {
                entity.ToTable("Variables", "dbo");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.AddressLine1).HasMaxLength(255);

                entity.Property(e => e.AddressLine2).HasMaxLength(255);

                entity.Property(e => e.City).HasMaxLength(255);

                entity.Property(e => e.CommercialDepositRequiredPercentage).HasPrecision(18, 2);

                entity.Property(e => e.CompanyName).HasMaxLength(255);

                entity.Property(e => e.CompanyRegistrationNumber).HasMaxLength(255);

                entity.Property(e => e.ContactEmail).HasMaxLength(255);

                entity.Property(e => e.ContactTelephoneNumber).HasMaxLength(50);

                entity.Property(e => e.Country).HasMaxLength(255);

                entity.Property(e => e.DemographicAverageRate).HasPrecision(18, 2);

                entity.Property(e => e.FaxNumber).HasMaxLength(255);

                entity.Property(e => e.FingerprintDistant).HasPrecision(5, 2);

                entity.Property(e => e.PostalCode).HasMaxLength(20);

                entity.Property(e => e.ScreeningQuotaDiscountPercentage).HasPrecision(18, 2);

                entity.Property(e => e.StandardScreeningRate).HasPrecision(18, 2);

                entity.Property(e => e.State).HasMaxLength(255);
            });

            modelBuilder.Entity<__MigrationHistory>(entity =>
            {
                entity.HasKey(e => new { e.MigrationId, e.ContextKey })
                    .HasName("__MigrationHistory_pkey");

                entity.ToTable("__MigrationHistory", "dbo");

                entity.Property(e => e.MigrationId).HasMaxLength(150);

                entity.Property(e => e.ContextKey).HasMaxLength(300);

                entity.Property(e => e.ProductVersion).HasMaxLength(32);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
