using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Voupon.Database.Postgres.RewardsEntities
{
    public partial class RewardsDBContext : IdentityDbContext<Users, Roles, Guid, UserClaims, UserRoles, UserLogins, RoleClaims, UserTokens>
    {
        public RewardsDBContext()
        {
        }

        public RewardsDBContext(DbContextOptions<RewardsDBContext> options)
            : base(options)
        {
        }

        public virtual DbSet<ActivityLogs> ActivityLogs { get; set; } = null!;
        public virtual DbSet<ActivityLogs_bak> ActivityLogs_bak { get; set; } = null!;
        public virtual DbSet<AdditionalDiscounts> AdditionalDiscounts { get; set; } = null!;
        public virtual DbSet<AggregatorApiUrls> AggregatorApiUrls { get; set; } = null!;
        public virtual DbSet<AggregatorExcludeMerchants> AggregatorExcludeMerchants { get; set; } = null!;
        public virtual DbSet<AggregatorExcludeProducts> AggregatorExcludeProducts { get; set; } = null!;
        public virtual DbSet<AggregatorKeywordFilters> AggregatorKeywordFilters { get; set; } = null!;
        public virtual DbSet<AggregatorMaxQuantityFilters> AggregatorMaxQuantityFilters { get; set; } = null!;
        public virtual DbSet<AppConfig> AppConfig { get; set; } = null!;
        public virtual DbSet<BankAccounts> BankAccounts { get; set; } = null!;
        public virtual DbSet<Banks> Banks { get; set; } = null!;
        public virtual DbSet<BusinessTypes> BusinessTypes { get; set; } = null!;
        public virtual DbSet<CartProductExternal> CartProductExternal { get; set; } = null!;
        public virtual DbSet<CartProductTypes> CartProductTypes { get; set; } = null!;
        public virtual DbSet<CartProducts> CartProducts { get; set; } = null!;
        public virtual DbSet<ChatGroup> ChatGroup { get; set; } = null!;
        public virtual DbSet<ChatGroupUsers> ChatGroupUsers { get; set; } = null!;
        public virtual DbSet<ChatMessages> ChatMessages { get; set; } = null!;
        public virtual DbSet<ConsoleMerchantJSON> ConsoleMerchantJSON { get; set; } = null!;
        public virtual DbSet<ConsoleMerchantToCrawl> ConsoleMerchantToCrawl { get; set; } = null!;
        public virtual DbSet<ConsoleProductJSON> ConsoleProductJSON { get; set; } = null!;
        public virtual DbSet<Countries> Countries { get; set; } = null!;
        public virtual DbSet<DealExpirations> DealExpirations { get; set; } = null!;
        public virtual DbSet<DealTypes> DealTypes { get; set; } = null!;
        public virtual DbSet<DeliveryRedemptionTokens> DeliveryRedemptionTokens { get; set; } = null!;
        public virtual DbSet<DigitalRedemptionTokens> DigitalRedemptionTokens { get; set; } = null!;
        public virtual DbSet<DiscountTypes> DiscountTypes { get; set; } = null!;
        public virtual DbSet<Districts> Districts { get; set; } = null!;
        public virtual DbSet<ErrorLogs> ErrorLogs { get; set; } = null!;
        public virtual DbSet<ExpirationTypes> ExpirationTypes { get; set; } = null!;
        public virtual DbSet<FinanceSummary> FinanceSummary { get; set; } = null!;
        public virtual DbSet<FinanceTransaction> FinanceTransaction { get; set; } = null!;
        public virtual DbSet<GifteeTokens> GifteeTokens { get; set; } = null!;
        public virtual DbSet<GoogleMerchantKeywords> GoogleMerchantKeywords { get; set; } = null!;
        public virtual DbSet<InStoreRedemptionTokens> InStoreRedemptionTokens { get; set; } = null!;
        public virtual DbSet<LuckyDraws> LuckyDraws { get; set; } = null!;
        public virtual DbSet<MailingLists> MailingLists { get; set; } = null!;
        public virtual DbSet<MerchantCarousel> MerchantCarousel { get; set; } = null!;
        public virtual DbSet<MerchantFinance> MerchantFinance { get; set; } = null!;
        public virtual DbSet<MerchantUserChat> MerchantUserChat { get; set; } = null!;
        public virtual DbSet<Merchants> Merchants { get; set; } = null!;
        public virtual DbSet<OrderItemExternal> OrderItemExternal { get; set; } = null!;
        public virtual DbSet<OrderItems> OrderItems { get; set; } = null!;
        public virtual DbSet<OrderItemsExternalStatusTypes> OrderItemsExternalStatusTypes { get; set; } = null!;
        public virtual DbSet<OrderPayments> OrderPayments { get; set; } = null!;
        public virtual DbSet<OrderShopExternal> OrderShopExternal { get; set; } = null!;
        public virtual DbSet<OrderShopsExternalStatusTypes> OrderShopsExternalStatusTypes { get; set; } = null!;
        public virtual DbSet<Orders> Orders { get; set; } = null!;
        public virtual DbSet<Outlets> Outlets { get; set; } = null!;
        public virtual DbSet<PasswordResets> PasswordResets { get; set; } = null!;
        public virtual DbSet<PersonInCharges> PersonInCharges { get; set; } = null!;
        public virtual DbSet<PostCodes> PostCodes { get; set; } = null!;
        public virtual DbSet<ProductCarousel> ProductCarousel { get; set; } = null!;
        public virtual DbSet<ProductCategories> ProductCategories { get; set; } = null!;
        public virtual DbSet<ProductDemographicsTarget> ProductDemographicsTarget { get; set; } = null!;
        public virtual DbSet<ProductDiscounts> ProductDiscounts { get; set; } = null!;
        public virtual DbSet<ProductOutlets> ProductOutlets { get; set; } = null!;
        public virtual DbSet<ProductReview> ProductReview { get; set; } = null!;
        public virtual DbSet<ProductReviewReply> ProductReviewReply { get; set; } = null!;
        public virtual DbSet<ProductReviewUploads> ProductReviewUploads { get; set; } = null!;
        public virtual DbSet<ProductShippingCost> ProductShippingCost { get; set; } = null!;
        public virtual DbSet<ProductSubCategories> ProductSubCategories { get; set; } = null!;
        public virtual DbSet<ProductVariation> ProductVariation { get; set; } = null!;
        public virtual DbSet<Products> Products { get; set; } = null!;
        public virtual DbSet<PromoCodeSelectedUsers> PromoCodeSelectedUsers { get; set; } = null!;
        public virtual DbSet<PromoCodes> PromoCodes { get; set; } = null!;
        public virtual DbSet<Provinces> Provinces { get; set; } = null!;
        public virtual DbSet<RefundPayments> RefundPayments { get; set; } = null!;
        public virtual DbSet<Refunds> Refunds { get; set; } = null!;
        public virtual DbSet<RefundsExternalOrderItems> RefundsExternalOrderItems { get; set; } = null!;
        public virtual DbSet<RoleClaims> RoleClaims { get; set; } = null!;
        public virtual DbSet<Roles> Roles { get; set; } = null!;
        public virtual DbSet<ShippingCost> ShippingCost { get; set; } = null!;
        public virtual DbSet<ShippingTypes> ShippingTypes { get; set; } = null!;
        public virtual DbSet<StatusTypes> StatusTypes { get; set; } = null!;
        public virtual DbSet<TempUsers> TempUsers { get; set; } = null!;
        public virtual DbSet<ThirdPartyProducts> ThirdPartyProducts { get; set; } = null!;
        public virtual DbSet<ThirdPartyTypes> ThirdPartyTypes { get; set; } = null!;
        public virtual DbSet<UserClaims> UserClaims { get; set; } = null!;
        public virtual DbSet<UserLogins> UserLogins { get; set; } = null!;
        public virtual DbSet<UserMobileTAC> UserMobileTAC { get; set; } = null!;
        public virtual DbSet<UserRoles> UserRoles { get; set; } = null!;
        public virtual DbSet<UserTokens> UserTokens { get; set; } = null!;
        public virtual DbSet<Users> Users { get; set; } = null!;
        public virtual DbSet<VariationCombination> VariationCombination { get; set; } = null!;
        public virtual DbSet<VariationOptions> VariationOptions { get; set; } = null!;
        public virtual DbSet<Variations> Variations { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
                optionsBuilder.UseNpgsql("Server=vodus-aurora-postgresql.cluster-c4d0ebxkvzrm.ap-southeast-1.rds.amazonaws.com;Port=5432;User Id=postgres;Password=APv,3Cvqzy,tnpma8rPrj,8QG5DKzU;Database=RewardsDB_test2");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasPostgresExtension("dbo", "uuid-ossp");

            modelBuilder.Entity<ActivityLogs>(entity =>
            {
                entity.ToTable("ActivityLogs", "dbo");

                entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");

                entity.Property(e => e.ActionId).HasMaxLength(50);

                entity.Property(e => e.ActionName).HasMaxLength(255);

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("timestamp(6) without time zone")
                    .HasDefaultValueSql("now()");

                entity.Property(e => e.CreatedAt2).HasMaxLength(37);

                entity.Property(e => e.TriggerBy).HasMaxLength(128);

                entity.Property(e => e.TriggerFor).HasMaxLength(128);
            });

            modelBuilder.Entity<ActivityLogs_bak>(entity =>
            {
                entity.HasNoKey();

                entity.ToTable("ActivityLogs_bak", "dbo");

                entity.Property(e => e.ActionId).HasMaxLength(50);

                entity.Property(e => e.ActionName).HasMaxLength(255);

                entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.CreatedAt2).HasMaxLength(37);

                entity.Property(e => e.TriggerBy).HasMaxLength(128);

                entity.Property(e => e.TriggerFor).HasMaxLength(128);
            });

            modelBuilder.Entity<AdditionalDiscounts>(entity =>
            {
                entity.ToTable("AdditionalDiscounts", "dbo");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.DiscountPercentage).HasPrecision(18, 2);

                entity.Property(e => e.MaxPrice).HasPrecision(18, 2);
            });

            modelBuilder.Entity<AggregatorApiUrls>(entity =>
            {
                entity.ToTable("AggregatorApiUrls", "dbo");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.LastUpdatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.Url).HasMaxLength(255);
            });

            modelBuilder.Entity<AggregatorExcludeMerchants>(entity =>
            {
                entity.ToTable("AggregatorExcludeMerchants", "dbo");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.MerchantId).HasMaxLength(255);

                entity.Property(e => e.MerchantUsername).HasMaxLength(255);
            });

            modelBuilder.Entity<AggregatorExcludeProducts>(entity =>
            {
                entity.ToTable("AggregatorExcludeProducts", "dbo");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.MerchantId).HasMaxLength(255);

                entity.Property(e => e.ProductId).HasMaxLength(255);

                entity.Property(e => e.ProductUrl).HasMaxLength(1000);
            });

            modelBuilder.Entity<AggregatorKeywordFilters>(entity =>
            {
                entity.ToTable("AggregatorKeywordFilters", "dbo");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.Keyword).HasMaxLength(255);
            });

            modelBuilder.Entity<AggregatorMaxQuantityFilters>(entity =>
            {
                entity.ToTable("AggregatorMaxQuantityFilters", "dbo");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.Keyword).HasMaxLength(255);
            });

            modelBuilder.Entity<AppConfig>(entity =>
            {
                entity.ToTable("AppConfig", "dbo");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.AggregatorErrorLastEmailedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.DefaultCommission).HasPrecision(5, 2);

                entity.Property(e => e.LastUpdatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.RinggitPerVpoints).HasPrecision(18, 2);

                entity.Property(e => e.VPointsMultiplier).HasPrecision(5, 2);

                entity.Property(e => e.VPointsMultiplierCap).HasPrecision(5, 2);
            });

            modelBuilder.Entity<BankAccounts>(entity =>
            {
                entity.ToTable("BankAccounts", "dbo");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.AccountNumber).HasMaxLength(50);

                entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.LastUpdatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.Name).HasMaxLength(255);

                entity.HasOne(d => d.Bank)
                    .WithMany(p => p.BankAccounts)
                    .HasForeignKey(d => d.BankId)
                    .HasConstraintName("fk_bankaccounts_banks_750625717");

                entity.HasOne(d => d.Merchant)
                    .WithMany(p => p.BankAccounts)
                    .HasForeignKey(d => d.MerchantId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_bankaccounts_merchants_766625774");

                entity.HasOne(d => d.StatusType)
                    .WithMany(p => p.BankAccounts)
                    .HasForeignKey(d => d.StatusTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_bankaccounts_statustypes_782625831");
            });

            modelBuilder.Entity<Banks>(entity =>
            {
                entity.ToTable("Banks", "dbo");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.BankCode).HasMaxLength(100);

                entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.LastUpdatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.Name).HasMaxLength(100);

                entity.Property(e => e.SWIFTCode).HasMaxLength(100);
            });

            modelBuilder.Entity<BusinessTypes>(entity =>
            {
                entity.ToTable("BusinessTypes", "dbo");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.LastUpdatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.Name).HasMaxLength(100);
            });

            modelBuilder.Entity<CartProductExternal>(entity =>
            {
                entity.ToTable("CartProductExternal", "dbo");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.AdditionalDiscountName).HasMaxLength(255);

                entity.Property(e => e.AdditionalDiscountPriceValue).HasPrecision(18, 2);

                entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.ExternalItemId).HasMaxLength(64);

                entity.Property(e => e.ExternalShopId).HasMaxLength(64);

                entity.Property(e => e.ExternalUrl).HasMaxLength(255);

                entity.Property(e => e.ProductDiscountedPrice).HasPrecision(18, 2);

                entity.Property(e => e.ProductPrice).HasPrecision(18, 2);

                entity.Property(e => e.SubTotal).HasPrecision(18, 2);

                entity.Property(e => e.TotalPrice).HasPrecision(18, 2);

                entity.Property(e => e.UpdatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.VPointsMultiplier).HasPrecision(5, 2);

                entity.Property(e => e.VPointsMultiplierCap).HasPrecision(5, 2);

                entity.Property(e => e.VariationText).HasMaxLength(255);
            });

            modelBuilder.Entity<CartProductTypes>(entity =>
            {
                entity.ToTable("CartProductTypes", "dbo");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Name).HasMaxLength(255);

                entity.HasOne(d => d.IdNavigation)
                    .WithOne(p => p.InverseIdNavigation)
                    .HasForeignKey<CartProductTypes>(d => d.Id)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_cartproducttypes_cartproducttypes_878626173");
            });

            modelBuilder.Entity<CartProducts>(entity =>
            {
                entity.ToTable("CartProducts", "dbo");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.SubTotal).HasPrecision(18, 2);

                entity.Property(e => e.TotalPrice).HasPrecision(18, 2);

                entity.Property(e => e.UpdatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.VariationText).HasMaxLength(255);

                entity.HasOne(d => d.AdditionalDiscount)
                    .WithMany(p => p.CartProducts)
                    .HasForeignKey(d => d.AdditionalDiscountId)
                    .HasConstraintName("fk_cartproducts_productdiscounts1_830626002");

                entity.HasOne(d => d.DealExpiration)
                    .WithMany(p => p.CartProducts)
                    .HasForeignKey(d => d.DealExpirationId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_cartproducts_dealexpirations1_798625888");

                entity.HasOne(d => d.Merchant)
                    .WithMany(p => p.CartProducts)
                    .HasForeignKey(d => d.MerchantId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_cartproducts_merchants1_814625945");

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.CartProducts)
                    .HasForeignKey(d => d.ProductId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_cartproducts_products1_846626059");

                entity.HasOne(d => d.Variation)
                    .WithMany(p => p.CartProducts)
                    .HasForeignKey(d => d.VariationId)
                    .HasConstraintName("fk_cartproducts_productvariation_862626116");
            });

            modelBuilder.Entity<ChatGroup>(entity =>
            {
                entity.ToTable("ChatGroup", "dbo");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.LastUpdatedAt).HasColumnType("timestamp without time zone");
            });

            modelBuilder.Entity<ChatGroupUsers>(entity =>
            {
                entity.ToTable("ChatGroupUsers", "dbo");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");

                entity.HasOne(d => d.ChatGroup)
                    .WithMany(p => p.ChatGroupUsers)
                    .HasForeignKey(d => d.ChatGroupId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("ChatGroupUsers_ChatGroupId_fkey");
            });

            modelBuilder.Entity<ChatMessages>(entity =>
            {
                entity.ToTable("ChatMessages", "dbo");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.AttachmentsUrl).HasMaxLength(250);

                entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");

                entity.HasOne(d => d.ToGroup)
                    .WithMany(p => p.ChatMessages)
                    .HasForeignKey(d => d.ToGroupId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("ChatMessages_ToGroupId_fkey");
            });

            modelBuilder.Entity<ConsoleMerchantJSON>(entity =>
            {
                entity.ToTable("ConsoleMerchantJSON", "dbo");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.ExternalId).HasMaxLength(20);

                entity.Property(e => e.LastUpdatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.MerchantUsername).HasMaxLength(128);

                entity.Property(e => e.PageUrl).HasMaxLength(255);
            });

            modelBuilder.Entity<ConsoleMerchantToCrawl>(entity =>
            {
                entity.ToTable("ConsoleMerchantToCrawl", "dbo");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.LastUpdatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.MerchantName).HasMaxLength(255);

                entity.Property(e => e.Url).HasMaxLength(50);
            });

            modelBuilder.Entity<ConsoleProductJSON>(entity =>
            {
                entity.ToTable("ConsoleProductJSON", "dbo");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.ExternalId).HasMaxLength(20);

                entity.Property(e => e.ExternalMerchantId).HasMaxLength(20);

                entity.Property(e => e.LastUpdatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.PageUrl).HasMaxLength(255);
            });

            modelBuilder.Entity<Countries>(entity =>
            {
                entity.ToTable("Countries", "dbo");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.LastUpdatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.Name).HasMaxLength(100);
            });

            modelBuilder.Entity<DealExpirations>(entity =>
            {
                entity.ToTable("DealExpirations", "dbo");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.ExpiredDate).HasColumnType("timestamp without time zone");

                entity.Property(e => e.LastUpdatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.StartDate).HasColumnType("timestamp without time zone");

                entity.HasOne(d => d.ExpirationType)
                    .WithMany(p => p.DealExpirations)
                    .HasForeignKey(d => d.ExpirationTypeId)
                    .HasConstraintName("fk_dealexpirations_expirationtypes_926626344");

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.DealExpirations)
                    .HasForeignKey(d => d.ProductId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_dealexpirations_products_942626401");
            });

            modelBuilder.Entity<DealTypes>(entity =>
            {
                entity.ToTable("DealTypes", "dbo");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.LastUpdatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.Name).HasMaxLength(100);
            });

            modelBuilder.Entity<DeliveryRedemptionTokens>(entity =>
            {
                entity.ToTable("DeliveryRedemptionTokens", "dbo");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.ExpiredDate).HasColumnType("timestamp without time zone");

                entity.Property(e => e.RedeemedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.Revenue).HasPrecision(18, 2);

                entity.Property(e => e.StartDate).HasColumnType("timestamp without time zone");

                entity.Property(e => e.UpdateTokenAt).HasColumnType("timestamp without time zone");

                entity.HasOne(d => d.Merchant)
                    .WithMany(p => p.DeliveryRedemptionTokens)
                    .HasForeignKey(d => d.MerchantId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_deliveryredemptiontokens_merchants_958626458");

                entity.HasOne(d => d.OrderItem)
                    .WithMany(p => p.DeliveryRedemptionTokens)
                    .HasForeignKey(d => d.OrderItemId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("DeliveryRedemptionTokens_OrderItemId_fkey");

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.DeliveryRedemptionTokens)
                    .HasForeignKey(d => d.ProductId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_deliveryredemptiontokens_products_990626572");
            });

            modelBuilder.Entity<DigitalRedemptionTokens>(entity =>
            {
                entity.ToTable("DigitalRedemptionTokens", "dbo");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.ExpiredDate).HasColumnType("timestamp without time zone");

                entity.Property(e => e.RedeemedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.Revenue).HasPrecision(18, 2);

                entity.Property(e => e.StartDate).HasColumnType("timestamp without time zone");

                entity.Property(e => e.UpdateTokenAt).HasColumnType("timestamp without time zone");

                entity.HasOne(d => d.Merchant)
                    .WithMany(p => p.DigitalRedemptionTokens)
                    .HasForeignKey(d => d.MerchantId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_digitalredemptiontokens_merchants_1006626629");

                entity.HasOne(d => d.OrderItem)
                    .WithMany(p => p.DigitalRedemptionTokens)
                    .HasForeignKey(d => d.OrderItemId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("DigitalRedemptionTokens_OrderItemId_fkey");

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.DigitalRedemptionTokens)
                    .HasForeignKey(d => d.ProductId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_digitalredemptiontokens_products_1038626743");
            });

            modelBuilder.Entity<DiscountTypes>(entity =>
            {
                entity.ToTable("DiscountTypes", "dbo");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.LastUpdatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.Name).HasMaxLength(100);
            });

            modelBuilder.Entity<Districts>(entity =>
            {
                entity.ToTable("Districts", "dbo");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.LastUpdatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.Name).HasMaxLength(100);

                entity.HasOne(d => d.Province)
                    .WithMany(p => p.Districts)
                    .HasForeignKey(d => d.ProvinceId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_districts_provinces_1054626800");
            });

            modelBuilder.Entity<ErrorLogs>(entity =>
            {
                entity.ToTable("ErrorLogs", "dbo");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.ActionName).HasMaxLength(500);

                entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.Email).HasMaxLength(255);

                entity.Property(e => e.Token).HasMaxLength(255);

                entity.Property(e => e.UserName).HasMaxLength(255);
            });

            modelBuilder.Entity<ExpirationTypes>(entity =>
            {
                entity.ToTable("ExpirationTypes", "dbo");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.Description).HasMaxLength(500);

                entity.Property(e => e.LastUpdatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.Name).HasMaxLength(100);
            });

            modelBuilder.Entity<FinanceSummary>(entity =>
            {
                entity.ToTable("FinanceSummary", "dbo");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.TotalPayout).HasPrecision(18, 2);
            });

            modelBuilder.Entity<FinanceTransaction>(entity =>
            {
                entity.ToTable("FinanceTransaction", "dbo");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.AdminProfit).HasPrecision(18, 2);

                entity.Property(e => e.DefaultCommission).HasPrecision(18, 2);

                entity.Property(e => e.MerchantProfit).HasPrecision(18, 2);

                entity.Property(e => e.TotalPrice).HasPrecision(18, 2);

                entity.HasOne(d => d.MerchantFinance)
                    .WithMany(p => p.FinanceTransaction)
                    .HasForeignKey(d => d.MerchantFinanceId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_financetransaction_merchantfinance_1070626857");

                entity.HasOne(d => d.OrderItem)
                    .WithMany(p => p.FinanceTransaction)
                    .HasForeignKey(d => d.OrderItemId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FinanceTransaction_OrderItemId_fkey");

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.FinanceTransaction)
                    .HasForeignKey(d => d.ProductId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_financetransaction_products_1102626971");
            });

            modelBuilder.Entity<GifteeTokens>(entity =>
            {
                entity.ToTable("GifteeTokens", "dbo");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.IssuedDate).HasColumnType("timestamp without time zone");
            });

            modelBuilder.Entity<GoogleMerchantKeywords>(entity =>
            {
                entity.ToTable("GoogleMerchantKeywords", "dbo");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Keyword).HasMaxLength(255);

                entity.Property(e => e.Language).HasMaxLength(2);

                entity.Property(e => e.SortBy).HasMaxLength(10);
            });

            modelBuilder.Entity<InStoreRedemptionTokens>(entity =>
            {
                entity.ToTable("InStoreRedemptionTokens", "dbo");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.ExpiredDate).HasColumnType("timestamp without time zone");

                entity.Property(e => e.RedeemedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.Revenue).HasPrecision(18, 2);

                entity.Property(e => e.StartDate).HasColumnType("timestamp without time zone");

                entity.HasOne(d => d.Merchant)
                    .WithMany(p => p.InStoreRedemptionTokens)
                    .HasForeignKey(d => d.MerchantId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_instoreredemptiontokens_merchants_1118627028");

                entity.HasOne(d => d.OrderItem)
                    .WithMany(p => p.InStoreRedemptionTokens)
                    .HasForeignKey(d => d.OrderItemId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("InStoreRedemptionTokens_OrderItemId_fkey");

                entity.HasOne(d => d.Outlet)
                    .WithMany(p => p.InStoreRedemptionTokens)
                    .HasForeignKey(d => d.OutletId)
                    .HasConstraintName("fk_instoreredemptiontokens_outlets_1150627142");

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.InStoreRedemptionTokens)
                    .HasForeignKey(d => d.ProductId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_instoreredemptiontokens_products_1166627199");
            });

            modelBuilder.Entity<LuckyDraws>(entity =>
            {
                entity.ToTable("LuckyDraws", "dbo");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.LastUpdatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.LuckyDrawDate).HasColumnType("timestamp without time zone");

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.LuckyDraws)
                    .HasForeignKey(d => d.ProductId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_luckydraws_products_1182627256");

                entity.HasOne(d => d.StatusType)
                    .WithMany(p => p.LuckyDraws)
                    .HasForeignKey(d => d.StatusTypeId)
                    .HasConstraintName("fk_luckydraws_statustypes_1198627313");
            });

            modelBuilder.Entity<MailingLists>(entity =>
            {
                entity.HasKey(e => e.Email)
                    .HasName("MailingLists_pkey");

                entity.ToTable("MailingLists", "dbo");

                entity.Property(e => e.Email).HasMaxLength(255);

                entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.LastEmailedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.UnSubscribeAt).HasColumnType("timestamp without time zone");
            });

            modelBuilder.Entity<MerchantCarousel>(entity =>
            {
                entity.ToTable("MerchantCarousel", "dbo");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.ImageUrl).HasMaxLength(255);

                entity.HasOne(d => d.Merchant)
                    .WithMany(p => p.MerchantCarousel)
                    .HasForeignKey(d => d.MerchantId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_merchantcarousel_merchants_1214627370");
            });

            modelBuilder.Entity<MerchantFinance>(entity =>
            {
                entity.ToTable("MerchantFinance", "dbo");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.TotalPayout).HasPrecision(18, 2);

                entity.HasOne(d => d.FinanceSummary)
                    .WithMany(p => p.MerchantFinance)
                    .HasForeignKey(d => d.FinanceSummaryId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_merchantfinance_financesummary_1230627427");

                entity.HasOne(d => d.Merchant)
                    .WithMany(p => p.MerchantFinance)
                    .HasForeignKey(d => d.MerchantId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_merchantfinance_merchants_1246627484");
            });

            modelBuilder.Entity<MerchantUserChat>(entity =>
            {
                entity.ToTable("MerchantUserChat", "dbo");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");
            });

            modelBuilder.Entity<Merchants>(entity =>
            {
                entity.ToTable("Merchants", "dbo");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Address_1).HasMaxLength(255);

                entity.Property(e => e.Address_2).HasMaxLength(255);

                entity.Property(e => e.Code).HasMaxLength(255);

                entity.Property(e => e.CompanyContact).HasMaxLength(255);

                entity.Property(e => e.CompanyName).HasMaxLength(255);

                entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.DefaultCommission).HasPrecision(4, 2);

                entity.Property(e => e.DisplayName).HasMaxLength(255);

                entity.Property(e => e.ExternalId).HasMaxLength(50);

                entity.Property(e => e.ExternalUrl).HasMaxLength(255);

                entity.Property(e => e.LastUpdatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.Rating).HasPrecision(4, 2);

                entity.Property(e => e.RegistrationNumber).HasMaxLength(100);

                entity.Property(e => e.WebsiteSiteUrl).HasMaxLength(255);

                entity.HasOne(d => d.BusinessType)
                    .WithMany(p => p.Merchants)
                    .HasForeignKey(d => d.BusinessTypeId)
                    .HasConstraintName("fk_merchants_businesstypes_1262627541");

                entity.HasOne(d => d.Country)
                    .WithMany(p => p.Merchants)
                    .HasForeignKey(d => d.CountryId)
                    .HasConstraintName("fk_merchants_countries_1278627598");

                entity.HasOne(d => d.Distrit)
                    .WithMany(p => p.Merchants)
                    .HasForeignKey(d => d.DistritId)
                    .HasConstraintName("fk_merchants_districts_1294627655");

                entity.HasOne(d => d.PostCode)
                    .WithMany(p => p.Merchants)
                    .HasForeignKey(d => d.PostCodeId)
                    .HasConstraintName("fk_merchants_postcodes_1310627712");

                entity.HasOne(d => d.Province)
                    .WithMany(p => p.Merchants)
                    .HasForeignKey(d => d.ProvinceId)
                    .HasConstraintName("fk_merchants_provinces_1326627769");

                entity.HasOne(d => d.StatusType)
                    .WithMany(p => p.Merchants)
                    .HasForeignKey(d => d.StatusTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_merchants_statustypes_1342627826");
            });

            modelBuilder.Entity<OrderItemExternal>(entity =>
            {
                entity.ToTable("OrderItemExternal", "dbo");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.DiscountName).HasMaxLength(255);

                entity.Property(e => e.DiscountPriceValue).HasPrecision(18, 2);

                entity.Property(e => e.DiscountedAmount).HasPrecision(18, 2);

                entity.Property(e => e.ExternalItemId).HasMaxLength(64);

                entity.Property(e => e.LastUpdatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.LastUpdatedByUser).HasMaxLength(255);

                entity.Property(e => e.OriginalPrice).HasPrecision(18, 2);

                entity.Property(e => e.Price).HasPrecision(18, 2);

                entity.Property(e => e.ShortId).HasMaxLength(10);

                entity.Property(e => e.SubTotal).HasPrecision(18, 2);

                entity.Property(e => e.SubtotalPrice).HasPrecision(18, 2);

                entity.Property(e => e.TotalPrice).HasPrecision(18, 2);

                entity.Property(e => e.VPointsMultiplier).HasPrecision(5, 2);

                entity.Property(e => e.VPointsMultiplierCap).HasPrecision(5, 2);

                entity.Property(e => e.VariationText).HasMaxLength(255);

                entity.HasOne(d => d.OrderShopExternal)
                    .WithMany(p => p.OrderItemExternal)
                    .HasForeignKey(d => d.OrderShopExternalId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("OrderItemExternal_OrderShopExternalId_fkey");
            });

            modelBuilder.Entity<OrderItems>(entity =>
            {
                entity.ToTable("OrderItems", "dbo");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Commision).HasPrecision(3, 2);

                entity.Property(e => e.DiscountedAmount).HasPrecision(18, 2);

                entity.Property(e => e.MerchantDisplayName).HasMaxLength(255);

                entity.Property(e => e.Price).HasPrecision(18, 2);

                entity.Property(e => e.ProductImageFolderUrl).HasMaxLength(500);

                entity.Property(e => e.ProductTitle).HasMaxLength(255);

                entity.Property(e => e.ShippingCost).HasPrecision(18, 2);

                entity.Property(e => e.ShippingCostDiscount).HasPrecision(18, 2);

                entity.Property(e => e.ShippingCostSubTotal).HasPrecision(18, 2);

                entity.Property(e => e.ShortId).HasMaxLength(10);

                entity.Property(e => e.SubtotalPrice).HasPrecision(18, 2);

                entity.Property(e => e.VPointsMultiplier).HasPrecision(5, 2);

                entity.Property(e => e.VPointsMultiplierCap).HasPrecision(5, 2);

                entity.Property(e => e.VariationText).HasMaxLength(255);

                entity.HasOne(d => d.Order)
                    .WithMany(p => p.OrderItems)
                    .HasForeignKey(d => d.OrderId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("OrderItems_OrderId_fkey");

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.OrderItems)
                    .HasForeignKey(d => d.ProductId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_orderitems_products_1374627940");
            });

            modelBuilder.Entity<OrderItemsExternalStatusTypes>(entity =>
            {
                entity.ToTable("OrderItemsExternalStatusTypes", "dbo");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.Description).HasMaxLength(500);

                entity.Property(e => e.Name).HasMaxLength(100);
            });

            modelBuilder.Entity<OrderPayments>(entity =>
            {
                entity.ToTable("OrderPayments", "dbo");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Amount).HasPrecision(18, 2);

                entity.Property(e => e.AuthCode).HasMaxLength(40);

                entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.CreditCardBankName).HasMaxLength(100);

                entity.Property(e => e.CreditCardIssuingCountry).HasMaxLength(100);

                entity.Property(e => e.CreditCardName).HasMaxLength(200);

                entity.Property(e => e.CreditCardNumber).HasMaxLength(16);

                entity.Property(e => e.Currency).HasMaxLength(5);

                entity.Property(e => e.ErrorDescription).HasMaxLength(200);

                entity.Property(e => e.MerchantCode).HasMaxLength(20);

                entity.Property(e => e.Remark).HasMaxLength(500);

                entity.Property(e => e.Signature).HasMaxLength(100);

                entity.Property(e => e.Status).HasMaxLength(50);

                entity.Property(e => e.TransactionId).HasMaxLength(128);

                entity.HasOne(d => d.RefNoNavigation)
                    .WithMany(p => p.OrderPayments)
                    .HasForeignKey(d => d.RefNo)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("OrderPayments_RefNo_fkey");
            });

            modelBuilder.Entity<OrderShopExternal>(entity =>
            {
                entity.ToTable("OrderShopExternal", "dbo");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.ExternalOrderId).HasMaxLength(255);

                entity.Property(e => e.ExternalShopId).HasMaxLength(64);

                entity.Property(e => e.ExternalShopUrl).HasMaxLength(255);

                entity.Property(e => e.LastUpdatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.LastUpdatedByUser).HasMaxLength(255);

                entity.Property(e => e.ShippingCost).HasPrecision(18, 2);

                entity.Property(e => e.ShippingCostDiscount).HasPrecision(18, 2);

                entity.Property(e => e.ShippingCostSubTotal).HasPrecision(18, 2);

                entity.Property(e => e.ShippingLatestStatus).HasMaxLength(255);

                entity.Property(e => e.TotalPrice).HasPrecision(18, 2);

                entity.HasOne(d => d.Order)
                    .WithMany(p => p.OrderShopExternal)
                    .HasForeignKey(d => d.OrderId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("OrderShopExternal_OrderId_fkey");
            });

            modelBuilder.Entity<OrderShopsExternalStatusTypes>(entity =>
            {
                entity.ToTable("OrderShopsExternalStatusTypes", "dbo");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.Description).HasMaxLength(500);

                entity.Property(e => e.Name).HasMaxLength(100);
            });

            modelBuilder.Entity<Orders>(entity =>
            {
                entity.ToTable("Orders", "dbo");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.BillingAddressLine1).HasMaxLength(255);

                entity.Property(e => e.BillingAddressLine2).HasMaxLength(255);

                entity.Property(e => e.BillingCity).HasMaxLength(255);

                entity.Property(e => e.BillingCountry).HasMaxLength(255);

                entity.Property(e => e.BillingEmail).HasMaxLength(255);

                entity.Property(e => e.BillingMobileCountryCode).HasMaxLength(10);

                entity.Property(e => e.BillingMobileNumber).HasMaxLength(20);

                entity.Property(e => e.BillingPersonFirstName).HasMaxLength(255);

                entity.Property(e => e.BillingPersonLastName).HasMaxLength(255);

                entity.Property(e => e.BillingPostcode).HasMaxLength(15);

                entity.Property(e => e.BillingState).HasMaxLength(255);

                entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.Email).HasMaxLength(255);

                entity.Property(e => e.PromoCodeDiscountValue).HasPrecision(18, 2);

                entity.Property(e => e.PromoCodeExpireOn).HasColumnType("timestamp without time zone");

                entity.Property(e => e.PromoCodeMaxDiscountValue).HasPrecision(18, 2);

                entity.Property(e => e.PromoCodeMinSpend).HasPrecision(18, 2);

                entity.Property(e => e.PromoCodeValue).HasMaxLength(255);

                entity.Property(e => e.ShippingAddressLine1).HasMaxLength(255);

                entity.Property(e => e.ShippingAddressLine2).HasMaxLength(255);

                entity.Property(e => e.ShippingCity).HasMaxLength(255);

                entity.Property(e => e.ShippingCostDiscount).HasPrecision(18, 2);

                entity.Property(e => e.ShippingCostSubTotal).HasPrecision(18, 2);

                entity.Property(e => e.ShippingCountry).HasMaxLength(255);

                entity.Property(e => e.ShippingEmail).HasMaxLength(255);

                entity.Property(e => e.ShippingMobileCountryCode).HasMaxLength(10);

                entity.Property(e => e.ShippingMobileNumber).HasMaxLength(20);

                entity.Property(e => e.ShippingPersonFirstName).HasMaxLength(255);

                entity.Property(e => e.ShippingPersonLastName).HasMaxLength(255);

                entity.Property(e => e.ShippingPostcode).HasMaxLength(15);

                entity.Property(e => e.ShippingState).HasMaxLength(255);

                entity.Property(e => e.ShortId).HasMaxLength(10);

                entity.Property(e => e.TotalPrice).HasPrecision(18, 2);

                entity.Property(e => e.TotalPriceBeforePromoCodeDiscount).HasPrecision(18, 2);

                entity.Property(e => e.TotalPromoCodeDiscount).HasPrecision(18, 2);

                entity.Property(e => e.TotalShippingCost).HasPrecision(18, 2);
            });

            modelBuilder.Entity<Outlets>(entity =>
            {
                entity.ToTable("Outlets", "dbo");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Address_1).HasMaxLength(255);

                entity.Property(e => e.Address_2).HasMaxLength(255);

                entity.Property(e => e.Contact).HasMaxLength(255);

                entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.ImgUrl).HasMaxLength(255);

                entity.Property(e => e.LastUpdatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.Name).HasMaxLength(255);

                entity.Property(e => e.OpeningHour_1).HasMaxLength(255);

                entity.Property(e => e.OpeningHour_2).HasMaxLength(255);

                entity.Property(e => e.OpeningHour_3).HasMaxLength(255);

                entity.Property(e => e.OpeningHour_4).HasMaxLength(255);

                entity.Property(e => e.OpeningHour_5).HasMaxLength(255);

                entity.Property(e => e.OpeningHour_6).HasMaxLength(255);

                entity.Property(e => e.OpeningHour_7).HasMaxLength(255);

                entity.Property(e => e.OpeningHour_8).HasMaxLength(255);

                entity.Property(e => e.StreetName).HasMaxLength(255);

                entity.HasOne(d => d.Country)
                    .WithMany(p => p.Outlets)
                    .HasForeignKey(d => d.CountryId)
                    .HasConstraintName("fk_outlets_countries_1390627997");

                entity.HasOne(d => d.District)
                    .WithMany(p => p.Outlets)
                    .HasForeignKey(d => d.DistrictId)
                    .HasConstraintName("fk_outlets_districts_1406628054");

                entity.HasOne(d => d.Merchant)
                    .WithMany(p => p.Outlets)
                    .HasForeignKey(d => d.MerchantId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_outlets_merchants_1422628111");

                entity.HasOne(d => d.PostCode)
                    .WithMany(p => p.Outlets)
                    .HasForeignKey(d => d.PostCodeId)
                    .HasConstraintName("fk_outlets_postcodes_1438628168");

                entity.HasOne(d => d.Province)
                    .WithMany(p => p.Outlets)
                    .HasForeignKey(d => d.ProvinceId)
                    .HasConstraintName("fk_outlets_provinces_1454628225");
            });

            modelBuilder.Entity<PasswordResets>(entity =>
            {
                entity.ToTable("PasswordResets", "dbo");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.Email).HasMaxLength(128);

                entity.Property(e => e.ExpireAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.ResetCode).HasMaxLength(128);
            });

            modelBuilder.Entity<PersonInCharges>(entity =>
            {
                entity.ToTable("PersonInCharges", "dbo");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Contact).HasMaxLength(255);

                entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.IdentityNumber).HasMaxLength(255);

                entity.Property(e => e.LastUpdatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.Name).HasMaxLength(255);

                entity.Property(e => e.Position).HasMaxLength(255);

                entity.HasOne(d => d.Merchant)
                    .WithMany(p => p.PersonInCharges)
                    .HasForeignKey(d => d.MerchantId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_personincharges_merchants_1470628282");

                entity.HasOne(d => d.StatusType)
                    .WithMany(p => p.PersonInCharges)
                    .HasForeignKey(d => d.StatusTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_personincharges_statustypes_1486628339");
            });

            modelBuilder.Entity<PostCodes>(entity =>
            {
                entity.ToTable("PostCodes", "dbo");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.LastUpdatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.Name).HasMaxLength(100);

                entity.HasOne(d => d.District)
                    .WithMany(p => p.PostCodes)
                    .HasForeignKey(d => d.DistrictId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_postcodes_districts_1502628396");
            });

            modelBuilder.Entity<ProductCarousel>(entity =>
            {
                entity.ToTable("ProductCarousel", "dbo");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.ImageUrl).HasMaxLength(255);

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.ProductCarousel)
                    .HasForeignKey(d => d.ProductId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_productcarousel_products_1518628453");
            });

            modelBuilder.Entity<ProductCategories>(entity =>
            {
                entity.ToTable("ProductCategories", "dbo");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.LastUpdatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.Name).HasMaxLength(100);
            });

            modelBuilder.Entity<ProductDemographicsTarget>(entity =>
            {
                entity.ToTable("ProductDemographicsTarget", "dbo");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.ProductDemographicsTarget)
                    .HasForeignKey(d => d.ProductId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_productdemographicstarget_products_1534628510");
            });

            modelBuilder.Entity<ProductDiscounts>(entity =>
            {
                entity.ToTable("ProductDiscounts", "dbo");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.EndDate).HasColumnType("timestamp without time zone");

                entity.Property(e => e.LastUpdatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.Name).HasMaxLength(255);

                entity.Property(e => e.PercentageValue).HasPrecision(18, 2);

                entity.Property(e => e.PriceValue).HasPrecision(18, 2);

                entity.Property(e => e.StartDate).HasColumnType("timestamp without time zone");

                entity.HasOne(d => d.DiscountType)
                    .WithMany(p => p.ProductDiscounts)
                    .HasForeignKey(d => d.DiscountTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_productdiscounts_discounttypes_1550628567");

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.ProductDiscounts)
                    .HasForeignKey(d => d.ProductId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_productdiscounts_products_1566628624");
            });

            modelBuilder.Entity<ProductOutlets>(entity =>
            {
                entity.HasKey(e => new { e.ProductId, e.OutletId })
                    .HasName("ProductOutlets_pkey");

                entity.ToTable("ProductOutlets", "dbo");

                entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.LastUpdatedAt).HasColumnType("timestamp without time zone");

                entity.HasOne(d => d.Outlet)
                    .WithMany(p => p.ProductOutlets)
                    .HasForeignKey(d => d.OutletId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_productoutlets_outlets_1582628681");

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.ProductOutlets)
                    .HasForeignKey(d => d.ProductId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_productoutlets_products_1598628738");
            });

            modelBuilder.Entity<ProductReview>(entity =>
            {
                entity.ToTable("ProductReview", "dbo");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.MemberName).HasMaxLength(255);

                entity.Property(e => e.Rating).HasPrecision(4, 2);

                entity.HasOne(d => d.Merchant)
                    .WithMany(p => p.ProductReview)
                    .HasForeignKey(d => d.MerchantId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_productreview_merchants_1614628795");

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.ProductReview)
                    .HasForeignKey(d => d.ProductId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_productreview_products_1646628909");
            });

            modelBuilder.Entity<ProductReviewReply>(entity =>
            {
                entity.ToTable("ProductReviewReply", "dbo");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.CreatedBy).HasMaxLength(255);

                entity.HasOne(d => d.ProductReview)
                    .WithMany(p => p.ProductReviewReply)
                    .HasForeignKey(d => d.ProductReviewId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_productreviewreply_productreview_1662628966");
            });

            modelBuilder.Entity<ProductReviewUploads>(entity =>
            {
                entity.ToTable("ProductReviewUploads", "dbo");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.FileUrl).HasMaxLength(255);

                entity.Property(e => e.MimeType).HasMaxLength(50);
            });

            modelBuilder.Entity<ProductShippingCost>(entity =>
            {
                entity.ToTable("ProductShippingCost", "dbo");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.ConditionalShippingCost).HasPrecision(18, 2);

                entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.LastUpdatedAt).HasColumnType("timestamp without time zone");

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.ProductShippingCost)
                    .HasForeignKey(d => d.ProductId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_productshippingcost_products_1806629479");
            });

            modelBuilder.Entity<ProductSubCategories>(entity =>
            {
                entity.ToTable("ProductSubCategories", "dbo");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.LastUpdatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.Name).HasMaxLength(100);

                entity.HasOne(d => d.Category)
                    .WithMany(p => p.ProductSubCategories)
                    .HasForeignKey(d => d.CategoryId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_productsubcategories_productcategories_1822629536");
            });

            modelBuilder.Entity<ProductVariation>(entity =>
            {
                entity.ToTable("ProductVariation", "dbo");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.DiscountedPrice).HasPrecision(18, 2);

                entity.Property(e => e.LastUpdatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.Price).HasPrecision(18, 2);

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.ProductVariation)
                    .HasForeignKey(d => d.ProductId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_productvariation_products1_1838629593");

                entity.HasOne(d => d.VariationCombination)
                    .WithMany(p => p.ProductVariation)
                    .HasForeignKey(d => d.VariationCombinationId)
                    .HasConstraintName("fk_productvariation_variationcombination1_1854629650");
            });

            modelBuilder.Entity<Products>(entity =>
            {
                entity.ToTable("Products", "dbo");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.ActualPriceForVpoints).HasPrecision(18, 2);

                entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.DefaultCommission).HasPrecision(4, 2);

                entity.Property(e => e.DiscountedPrice).HasPrecision(18, 2);

                entity.Property(e => e.EndDate).HasColumnType("timestamp without time zone");

                entity.Property(e => e.ExternalId).HasMaxLength(50);

                entity.Property(e => e.ExternalMerchantId).HasMaxLength(50);

                entity.Property(e => e.ExternalUrl).HasMaxLength(500);

                entity.Property(e => e.LastUpdatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.Price).HasPrecision(18, 2);

                entity.Property(e => e.Rating).HasPrecision(4, 2);

                entity.Property(e => e.StartDate).HasColumnType("timestamp without time zone");

                entity.Property(e => e.Subtitle).HasMaxLength(255);

                entity.Property(e => e.Title).HasMaxLength(255);

                entity.HasOne(d => d.DealExpiration)
                    .WithMany(p => p.Products)
                    .HasForeignKey(d => d.DealExpirationId)
                    .HasConstraintName("fk_products_dealexpirations_1694629080");

                entity.HasOne(d => d.DealType)
                    .WithMany(p => p.Products)
                    .HasForeignKey(d => d.DealTypeId)
                    .HasConstraintName("fk_products_dealtypes_1710629137");

                entity.HasOne(d => d.LuckyDraw)
                    .WithMany(p => p.Products)
                    .HasForeignKey(d => d.LuckyDrawId)
                    .HasConstraintName("fk_products_luckydraws_1726629194");

                entity.HasOne(d => d.Merchant)
                    .WithMany(p => p.Products)
                    .HasForeignKey(d => d.MerchantId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_products_merchants_1742629251");

                entity.HasOne(d => d.ProductCategory)
                    .WithMany(p => p.Products)
                    .HasForeignKey(d => d.ProductCategoryId)
                    .HasConstraintName("fk_products_productcategories_1758629308");

                entity.HasOne(d => d.ProductSubCategory)
                    .WithMany(p => p.Products)
                    .HasForeignKey(d => d.ProductSubCategoryId)
                    .HasConstraintName("fk_products_productsubcategories_1774629365");

                entity.HasOne(d => d.StatusType)
                    .WithMany(p => p.Products)
                    .HasForeignKey(d => d.StatusTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_products_statustypes_1790629422");
            });

            modelBuilder.Entity<PromoCodeSelectedUsers>(entity =>
            {
                entity.ToTable("PromoCodeSelectedUsers", "dbo");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.Email).HasMaxLength(255);

                entity.Property(e => e.RedeemedAt).HasColumnType("timestamp without time zone");

                entity.HasOne(d => d.PromoCode)
                    .WithMany(p => p.PromoCodeSelectedUsers)
                    .HasForeignKey(d => d.PromoCodeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_promocodeselectedusers_promocodes_1870629707");
            });

            modelBuilder.Entity<PromoCodes>(entity =>
            {
                entity.ToTable("PromoCodes", "dbo");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.Description).HasMaxLength(500);

                entity.Property(e => e.DiscountValue).HasPrecision(18, 2);

                entity.Property(e => e.ExpireOn).HasColumnType("timestamp without time zone");

                entity.Property(e => e.LastUpdatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.MaxDiscountValue).HasPrecision(18, 2);

                entity.Property(e => e.MinSpend).HasPrecision(18, 2);

                entity.Property(e => e.Name).HasMaxLength(255);

                entity.Property(e => e.PromoCode).HasMaxLength(50);
            });

            modelBuilder.Entity<Provinces>(entity =>
            {
                entity.ToTable("Provinces", "dbo");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.LastUpdatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.Name).HasMaxLength(100);

                entity.HasOne(d => d.Country)
                    .WithMany(p => p.Provinces)
                    .HasForeignKey(d => d.CountryId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_provinces_countries_1886629764");
            });

            modelBuilder.Entity<RefundPayments>(entity =>
            {
                entity.ToTable("RefundPayments", "dbo");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Amount).HasPrecision(18, 2);

                entity.Property(e => e.AuthCode).HasMaxLength(40);

                entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.CreditCardBankName).HasMaxLength(100);

                entity.Property(e => e.CreditCardIssuingCountry).HasMaxLength(100);

                entity.Property(e => e.CreditCardName).HasMaxLength(200);

                entity.Property(e => e.CreditCardNumber).HasMaxLength(16);

                entity.Property(e => e.Currency).HasMaxLength(5);

                entity.Property(e => e.ErrorDescription).HasMaxLength(200);

                entity.Property(e => e.MerchantCode).HasMaxLength(20);

                entity.Property(e => e.Remark).HasMaxLength(500);

                entity.Property(e => e.Signature).HasMaxLength(100);

                entity.Property(e => e.Status).HasMaxLength(50);

                entity.Property(e => e.TransactionId).HasMaxLength(128);

                entity.HasOne(d => d.RefNoNavigation)
                    .WithMany(p => p.RefundPayments)
                    .HasForeignKey(d => d.RefNo)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_refundpayments_refunds_1902629821");
            });

            modelBuilder.Entity<Refunds>(entity =>
            {
                entity.ToTable("Refunds", "dbo");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.MoneyRefunded).HasPrecision(18, 2);

                entity.Property(e => e.RefundTransactionId).HasMaxLength(255);

                entity.Property(e => e.Remark).HasMaxLength(1000);

                entity.Property(e => e.ShortId).HasMaxLength(10);

                entity.HasOne(d => d.OrderItem)
                    .WithMany(p => p.Refunds)
                    .HasForeignKey(d => d.OrderItemId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("Refunds_OrderItemId_fkey");
            });

            modelBuilder.Entity<RefundsExternalOrderItems>(entity =>
            {
                entity.ToTable("RefundsExternalOrderItems", "dbo");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.MoneyRefunded).HasPrecision(18, 2);

                entity.Property(e => e.RefundTransactionId).HasMaxLength(255);

                entity.Property(e => e.Remark).HasMaxLength(1000);

                entity.Property(e => e.ShortId).HasMaxLength(10);
            });

            modelBuilder.Entity<RoleClaims>(entity =>
            {
                entity.ToTable("RoleClaims", "dbo");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.HasOne(d => d.Role)
                    .WithMany(p => p.RoleClaims)
                    .HasForeignKey(d => d.RoleId)
                    .HasConstraintName("fk_aspnetroleclaims_aspnetroles_roleid_1950629992");
            });

            modelBuilder.Entity<Roles>(entity =>
            {
                entity.ToTable("Roles", "dbo");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Name).HasMaxLength(256);

                entity.Property(e => e.NormalizedName).HasMaxLength(256);
            });

            modelBuilder.Entity<ShippingCost>(entity =>
            {
                entity.ToTable("ShippingCost", "dbo");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Cost).HasPrecision(18, 2);

                entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.LastUpdatedAt).HasColumnType("timestamp without time zone");

                entity.HasOne(d => d.ProductShipping)
                    .WithMany(p => p.ShippingCost)
                    .HasForeignKey(d => d.ProductShippingId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_shippingcost_productshippingcost_1966630049");

                entity.HasOne(d => d.Province)
                    .WithMany(p => p.ShippingCost)
                    .HasForeignKey(d => d.ProvinceId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_shippingcost_provinces_1982630106");
            });

            modelBuilder.Entity<ShippingTypes>(entity =>
            {
                entity.ToTable("ShippingTypes", "dbo");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.LastUpdatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.Name).HasMaxLength(100);
            });

            modelBuilder.Entity<StatusTypes>(entity =>
            {
                entity.ToTable("StatusTypes", "dbo");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.Description).HasMaxLength(500);

                entity.Property(e => e.LastUpdatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.Name).HasMaxLength(100);
            });

            modelBuilder.Entity<TempUsers>(entity =>
            {
                entity.ToTable("TempUsers", "dbo");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.BusinessName).HasMaxLength(255);

                entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.Email).HasMaxLength(128);

                entity.Property(e => e.MobileNumber).HasMaxLength(12);

                entity.Property(e => e.TAC).HasMaxLength(6);

                entity.Property(e => e.TACRequestedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.TACVerifiedAt).HasColumnType("timestamp without time zone");
            });

            modelBuilder.Entity<ThirdPartyProducts>(entity =>
            {
                entity.ToTable("ThirdPartyProducts", "dbo");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.ExternalId).HasMaxLength(50);

                entity.Property(e => e.Name).HasMaxLength(255);

                entity.HasOne(d => d.ThirdPartyType)
                    .WithMany(p => p.ThirdPartyProducts)
                    .HasForeignKey(d => d.ThirdPartyTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("ThirdPartyProducts_ThirdPartyTypeId_fkey");
            });

            modelBuilder.Entity<ThirdPartyTypes>(entity =>
            {
                entity.ToTable("ThirdPartyTypes", "dbo");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Name).HasMaxLength(255);
            });

            modelBuilder.Entity<UserClaims>(entity =>
            {
                entity.ToTable("UserClaims", "dbo");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.HasOne(d => d.User)
                    .WithMany(p => p.UserClaims)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("UserClaims_UserId_fkey");
            });

            modelBuilder.Entity<UserLogins>(entity =>
            {
                entity.HasKey(e => new { e.LoginProvider, e.ProviderKey })
                    .HasName("UserLogins_pkey");

                entity.ToTable("UserLogins", "dbo");

                entity.Property(e => e.LoginProvider).HasMaxLength(128);

                entity.Property(e => e.ProviderKey).HasMaxLength(128);

                entity.HasOne(d => d.User)
                    .WithMany(p => p.UserLogins)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("fk_aspnetuserlogins_aspnetusers_userid_2030630277");
            });

            modelBuilder.Entity<UserMobileTAC>(entity =>
            {
                entity.ToTable("UserMobileTAC", "dbo");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.ExpiredAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.MobileNumber).HasMaxLength(12);

                entity.Property(e => e.TAC).HasMaxLength(6);

                entity.Property(e => e.VerifiedAt).HasColumnType("timestamp without time zone");
            });

            modelBuilder.Entity<UserRoles>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.RoleId })
                    .HasName("UserRoles_pkey");

                entity.ToTable("UserRoles", "dbo");

                entity.HasOne(d => d.Merchant)
                    .WithMany(p => p.UserRoles)
                    .HasForeignKey(d => d.MerchantId)
                    .HasConstraintName("fk_userroles_merchants_2078630448");

                entity.HasOne(d => d.Role)
                    .WithMany(p => p.UserRoles)
                    .HasForeignKey(d => d.RoleId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("UserRoles_RoleId_fkey");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.UserRoles)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("UserRoles_UserId_fkey");
            });

            modelBuilder.Entity<UserTokens>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.LoginProvider, e.Name })
                    .HasName("UserTokens_pkey");

                entity.ToTable("UserTokens", "dbo");

                entity.Property(e => e.LoginProvider).HasMaxLength(128);

                entity.Property(e => e.Name).HasMaxLength(128);

                entity.HasOne(d => d.User)
                    .WithMany(p => p.UserTokens)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("fk_aspnetusertokens_aspnetusers_userid_2094630505");
            });

            modelBuilder.Entity<Users>(entity =>
            {
                entity.ToTable("Users", "dbo");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.Email).HasMaxLength(256);

                entity.Property(e => e.NormalizedEmail).HasMaxLength(256);

                entity.Property(e => e.NormalizedUserName).HasMaxLength(256);

                entity.Property(e => e.UserName).HasMaxLength(256);
            });

            modelBuilder.Entity<VariationCombination>(entity =>
            {
                entity.ToTable("VariationCombination", "dbo");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.LastUpdatedAt).HasColumnType("timestamp without time zone");

                entity.HasOne(d => d.OptionOne)
                    .WithMany(p => p.VariationCombination)
                    .HasForeignKey(d => d.OptionOneId)
                    .HasConstraintName("fk_variationcombination_variationoptions1_2110630562");
            });

            modelBuilder.Entity<VariationOptions>(entity =>
            {
                entity.ToTable("VariationOptions", "dbo");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.LastUpdatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.Name).HasMaxLength(255);

                entity.HasOne(d => d.Variation)
                    .WithMany(p => p.VariationOptions)
                    .HasForeignKey(d => d.VariationId)
                    .HasConstraintName("fk_variationoptions_variations1_2126630619");
            });

            modelBuilder.Entity<Variations>(entity =>
            {
                entity.ToTable("Variations", "dbo");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.LastUpdatedAt).HasColumnType("timestamp without time zone");

                entity.Property(e => e.Name).HasMaxLength(255);

                entity.HasOne(d => d.IdNavigation)
                    .WithOne(p => p.InverseIdNavigation)
                    .HasForeignKey<Variations>(d => d.Id)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_variations_variations_11147085");

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.Variations)
                    .HasForeignKey(d => d.ProductId)
                    .HasConstraintName("fk_variations_products_2142630676");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
