using System;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Voupon.Database.MSSQL.RewardsEntities
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

        public virtual DbSet<AdditionalDiscounts> AdditionalDiscounts { get; set; }
        public virtual DbSet<AggregatorApiUrls> AggregatorApiUrls { get; set; }
        public virtual DbSet<AggregatorExcludeMerchants> AggregatorExcludeMerchants { get; set; }
        public virtual DbSet<AggregatorExcludeProducts> AggregatorExcludeProducts { get; set; }
        public virtual DbSet<AggregatorKeywordFilters> AggregatorKeywordFilters { get; set; }
        public virtual DbSet<AggregatorMaxQuantityFilters> AggregatorMaxQuantityFilters { get; set; }
        public virtual DbSet<AppConfig> AppConfig { get; set; }
        public virtual DbSet<BankAccounts> BankAccounts { get; set; }
        public virtual DbSet<Banks> Banks { get; set; }
        public virtual DbSet<BusinessTypes> BusinessTypes { get; set; }
        public virtual DbSet<CartProductExternal> CartProductExternal { get; set; }
        public virtual DbSet<CartProductTypes> CartProductTypes { get; set; }
        public virtual DbSet<CartProducts> CartProducts { get; set; }
        public virtual DbSet<ChatGroup> ChatGroup { get; set; }
        public virtual DbSet<ChatGroupUsers> ChatGroupUsers { get; set; }
        public virtual DbSet<ChatMessages> ChatMessages { get; set; }
        public virtual DbSet<Countries> Countries { get; set; }
        public virtual DbSet<DealExpirations> DealExpirations { get; set; }
        public virtual DbSet<DealTypes> DealTypes { get; set; }
        public virtual DbSet<DeliveryRedemptionTokens> DeliveryRedemptionTokens { get; set; }
        public virtual DbSet<DigitalRedemptionTokens> DigitalRedemptionTokens { get; set; }
        public virtual DbSet<DiscountTypes> DiscountTypes { get; set; }
        public virtual DbSet<Districts> Districts { get; set; }
        public virtual DbSet<ErrorLogs> ErrorLogs { get; set; }
        public virtual DbSet<ExpirationTypes> ExpirationTypes { get; set; }
        public virtual DbSet<FinanceSummary> FinanceSummary { get; set; }
        public virtual DbSet<FinanceTransaction> FinanceTransaction { get; set; }
        public virtual DbSet<GifteeTokens> GifteeTokens { get; set; }
        public virtual DbSet<GoogleMerchantKeywords> GoogleMerchantKeywords { get; set; }
        public virtual DbSet<InStoreRedemptionTokens> InStoreRedemptionTokens { get; set; }
        public virtual DbSet<LuckyDraws> LuckyDraws { get; set; }
        public virtual DbSet<MailingLists> MailingLists { get; set; }
        public virtual DbSet<MerchantFinance> MerchantFinance { get; set; }
        public virtual DbSet<Merchants> Merchants { get; set; }
        public virtual DbSet<OrderItemExternal> OrderItemExternal { get; set; }
        public virtual DbSet<OrderItems> OrderItems { get; set; }
        public virtual DbSet<OrderItemsExternalStatusTypes> OrderItemsExternalStatusTypes { get; set; }
        public virtual DbSet<OrderPayments> OrderPayments { get; set; }
        public virtual DbSet<OrderShopExternal> OrderShopExternal { get; set; }
        public virtual DbSet<Orders> Orders { get; set; }
        public virtual DbSet<Outlets> Outlets { get; set; }
        public virtual DbSet<PasswordResets> PasswordResets { get; set; }
        public virtual DbSet<PersonInCharges> PersonInCharges { get; set; }
        public virtual DbSet<PostCodes> PostCodes { get; set; }
        public virtual DbSet<ProductCategories> ProductCategories { get; set; }
        public virtual DbSet<ProductDemographicsTarget> ProductDemographicsTarget { get; set; }
        public virtual DbSet<ProductDiscounts> ProductDiscounts { get; set; }
        public virtual DbSet<ProductOutlets> ProductOutlets { get; set; }
        public virtual DbSet<ProductReview> ProductReview { get; set; }
        public virtual DbSet<ProductReviewReply> ProductReviewReply { get; set; }
        public virtual DbSet<ProductReviewUploads> ProductReviewUploads { get; set; }
        public virtual DbSet<ProductShippingCost> ProductShippingCost { get; set; }
        public virtual DbSet<ProductSubCategories> ProductSubCategories { get; set; }
        public virtual DbSet<ProductVariation> ProductVariation { get; set; }
        public virtual DbSet<Products> Products { get; set; }
        public virtual DbSet<PromoCodeSelectedUsers> PromoCodeSelectedUsers { get; set; }
        public virtual DbSet<PromoCodes> PromoCodes { get; set; }
        public virtual DbSet<Provinces> Provinces { get; set; }
        public virtual DbSet<RefundPayments> RefundPayments { get; set; }
        public virtual DbSet<Refunds> Refunds { get; set; }
        public virtual DbSet<RefundsExternalOrderItems> RefundsExternalOrderItems { get; set; }
        public virtual DbSet<RoleClaims> RoleClaims { get; set; }
        public virtual DbSet<Roles> Roles { get; set; }
        public virtual DbSet<ShippingCost> ShippingCost { get; set; }
        public virtual DbSet<ShippingTypes> ShippingTypes { get; set; }
        public virtual DbSet<StatusTypes> StatusTypes { get; set; }
        public virtual DbSet<TempUsers> TempUsers { get; set; }
        public virtual DbSet<ThirdPartyProducts> ThirdPartyProducts { get; set; }
        public virtual DbSet<ThirdPartyTypes> ThirdPartyTypes { get; set; }
        public virtual DbSet<UserClaims> UserClaims { get; set; }
        public virtual DbSet<UserLogins> UserLogins { get; set; }
        public virtual DbSet<UserMobileTAC> UserMobileTAC { get; set; }
        public virtual DbSet<UserRoles> UserRoles { get; set; }
        public virtual DbSet<UserTokens> UserTokens { get; set; }
        public virtual DbSet<Users> Users { get; set; }
        public virtual DbSet<VariationCombination> VariationCombination { get; set; }
        public virtual DbSet<VariationOptions> VariationOptions { get; set; }
        public virtual DbSet<Variations> Variations { get; set; }
        public virtual DbSet<ActivityLogs> ActivityLogs { get; set; }
        public virtual DbSet<ConsoleMerchantToCrawl> ConsoleMerchantToCrawl { get; set; }
        public virtual DbSet<ConsoleMerchantJSON> ConsoleMerchantJSON { get; set; }
        public virtual DbSet<ConsoleProductJSON> ConsoleProductJSON { get; set; }
        public virtual DbSet<ProductCarousel> ProductCarousel { get; set; }
        public virtual DbSet<MerchantCarousel> MerchantCarousel { get; set; }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                //#warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
                //optionsBuilder.UseSqlServer("Server=tcp:voupon.database.windows.net,1433;Database=RewardsDB;Trusted_Connection=false;Encrypt=True;TrustServerCertificate=False;User id=Voupon;password=Vodus!234##");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MerchantCarousel>(entity =>
            {
                entity.Property(e => e.ImageUrl)
                    .IsRequired()
                    .HasMaxLength(255)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<ProductCarousel>(entity =>
            {
                entity.Property(e => e.ImageUrl)
                    .IsRequired()
                    .HasMaxLength(255)
                    .IsUnicode(false);
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

            modelBuilder.Entity<AdditionalDiscounts>(entity =>
            {
                entity.Property(e => e.DiscountPercentage).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.MaxPrice).HasColumnType("decimal(18, 2)");
            });

            modelBuilder.Entity<AggregatorExcludeMerchants>(entity =>
            {
                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.MerchantId)
                    .IsRequired()
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.MerchantUsername)
                    .IsRequired()
                    .HasMaxLength(255)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<AggregatorExcludeProducts>(entity =>
            {
                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.MerchantId)
                    .IsRequired()
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.ProductId)
                    .IsRequired()
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.ProductUrl)
                    .IsRequired()
                    .HasMaxLength(1000)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<AggregatorKeywordFilters>(entity =>
            {
                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.Keyword)
                    .IsRequired()
                    .HasMaxLength(255);
            });

            modelBuilder.Entity<AggregatorMaxQuantityFilters>(entity =>
            {
                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.Keyword)
                    .IsRequired()
                    .HasMaxLength(255);
            });

            modelBuilder.Entity<AppConfig>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.DefaultCommission)
                    .HasColumnType("decimal(5, 2)")
                    .HasDefaultValueSql("((3.0))");

                entity.Property(e => e.IsCheckoutEnabled)
                    .IsRequired()
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.LastUpdatedAt).HasColumnType("datetime");

                entity.Property(e => e.MaxOrderFilter).HasDefaultValueSql("((50))");

                entity.Property(e => e.RinggitPerVpoints)
                    .HasColumnType("decimal(18, 2)")
                    .HasDefaultValueSql("((0.07))");

                entity.Property(e => e.VPointsMultiplier)
                    .HasColumnType("decimal(5, 2)")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.VPointsMultiplierCap)
                    .HasColumnType("decimal(5, 2)")
                    .HasDefaultValueSql("((25))");
            });

            modelBuilder.Entity<BankAccounts>(entity =>
            {
                entity.Property(e => e.AccountNumber).HasMaxLength(50);

                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.LastUpdatedAt).HasColumnType("datetime");

                entity.Property(e => e.Name).HasMaxLength(255);

                entity.HasOne(d => d.Bank)
                    .WithMany(p => p.BankAccounts)
                    .HasForeignKey(d => d.BankId)
                    .HasConstraintName("FK_BankAccounts_Banks");

                entity.HasOne(d => d.Merchant)
                    .WithMany(p => p.BankAccounts)
                    .HasForeignKey(d => d.MerchantId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_BankAccounts_Merchants");

                entity.HasOne(d => d.StatusType)
                    .WithMany(p => p.BankAccounts)
                    .HasForeignKey(d => d.StatusTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_BankAccounts_StatusTypes");
            });

            modelBuilder.Entity<Banks>(entity =>
            {
                entity.Property(e => e.BankCode).HasMaxLength(100);

                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.LastUpdatedAt).HasColumnType("datetime");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.SWIFTCode).HasMaxLength(100);
            });

            modelBuilder.Entity<BusinessTypes>(entity =>
            {
                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.LastUpdatedAt).HasColumnType("datetime");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);
            });

            modelBuilder.Entity<CartProductExternal>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.AdditionalDiscountName)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.AdditionalDiscountPriceValue).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.CartProductType).HasDefaultValueSql("((1))");

                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.ExternalItemId)
                    .IsRequired()
                    .HasMaxLength(64)
                    .IsUnicode(false);

                entity.Property(e => e.ExternalShopId)
                    .IsRequired()
                    .HasMaxLength(64)
                    .IsUnicode(false);

                entity.Property(e => e.ExternalShopName).IsRequired();

                entity.Property(e => e.ExternalUrl)
                    .IsRequired()
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.JsonData).IsUnicode(false);

                entity.Property(e => e.ProductCartPreviewSmallImageURL)
                    .IsRequired()
                    .IsUnicode(false);

                entity.Property(e => e.ProductDiscountedPrice).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.ProductPrice).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.ProductTitle).IsRequired();

                entity.Property(e => e.ProductVariation).IsUnicode(false);

                entity.Property(e => e.SubTotal).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.TotalPrice).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

                entity.Property(e => e.VPointsMultiplier).HasColumnType("decimal(5, 2)");

                entity.Property(e => e.VPointsMultiplierCap).HasColumnType("decimal(5, 2)");

                entity.Property(e => e.VariationText).HasMaxLength(255);
            });

            modelBuilder.Entity<CartProductTypes>(entity =>
            {
                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(255);
            });

            modelBuilder.Entity<CartProducts>(entity =>
            {
                entity.HasIndex(e => e.MasterMemberProfileId);

                entity.Property(e => e.CartProductType).HasDefaultValueSql("((1))");

                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.ProductCartPreviewSmallImageURL).IsRequired();

                entity.Property(e => e.SubTotal).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.TotalPrice).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

                entity.Property(e => e.VariationText).HasMaxLength(255);

                entity.HasOne(d => d.AdditionalDiscount)
                    .WithMany(p => p.CartProducts)
                    .HasForeignKey(d => d.AdditionalDiscountId)
                    .HasConstraintName("FK_CartProducts_ProductDiscounts1");

                entity.HasOne(d => d.DealExpiration)
                    .WithMany(p => p.CartProducts)
                    .HasForeignKey(d => d.DealExpirationId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_CartProducts_DealExpirations1");

                entity.HasOne(d => d.Merchant)
                    .WithMany(p => p.CartProducts)
                    .HasForeignKey(d => d.MerchantId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_CartProducts_Merchants1");

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.CartProducts)
                    .HasForeignKey(d => d.ProductId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_CartProducts_Products1");

                entity.HasOne(d => d.Variation)
                    .WithMany(p => p.CartProducts)
                    .HasForeignKey(d => d.VariationId)
                    .HasConstraintName("FK_CartProducts_ProductVariation");
            });

            modelBuilder.Entity<ChatGroup>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.IsActive)
                    .IsRequired()
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.LastUpdatedAt).HasColumnType("datetime");
            });

            modelBuilder.Entity<ChatGroupUsers>(entity =>
            {
                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.IsActive)
                    .IsRequired()
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.UserId).IsRequired();

                entity.Property(e => e.UserName).IsRequired();

                entity.Property(e => e.UserProfileImageUrl).IsRequired();

                entity.HasOne(d => d.ChatGroup)
                    .WithMany(p => p.ChatGroupUsers)
                    .HasForeignKey(d => d.ChatGroupId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ChatGroupUsers_ChatGroup");
            });

            modelBuilder.Entity<ChatMessages>(entity =>
            {
                entity.Property(e => e.AttachmentsUrl).HasMaxLength(250);

                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.CreatedByUserId).IsRequired();

                entity.Property(e => e.Message).IsRequired();

                entity.HasOne(d => d.ToGroup)
                    .WithMany(p => p.ChatMessages)
                    .HasForeignKey(d => d.ToGroupId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ChatMessages_ChatGroup");
            });

            modelBuilder.Entity<Countries>(entity =>
            {
                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.LastUpdatedAt).HasColumnType("datetime");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);
            });

            modelBuilder.Entity<DealExpirations>(entity =>
            {
                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.ExpiredDate).HasColumnType("datetime");

                entity.Property(e => e.LastUpdatedAt).HasColumnType("datetime");

                entity.Property(e => e.StartDate).HasColumnType("datetime");

                entity.HasOne(d => d.ExpirationType)
                    .WithMany(p => p.DealExpirations)
                    .HasForeignKey(d => d.ExpirationTypeId)
                    .HasConstraintName("FK_DealExpirations_ExpirationTypes");

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.DealExpirations)
                    .HasForeignKey(d => d.ProductId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_DealExpirations_Products");
            });

            modelBuilder.Entity<DealTypes>(entity =>
            {
                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.LastUpdatedAt).HasColumnType("datetime");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);
            });

            modelBuilder.Entity<DeliveryRedemptionTokens>(entity =>
            {
                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.ExpiredDate).HasColumnType("datetime");

                entity.Property(e => e.RedeemedAt).HasColumnType("datetime");

                entity.Property(e => e.Revenue).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.StartDate).HasColumnType("datetime");

                entity.Property(e => e.UpdateTokenAt).HasColumnType("datetime");

                entity.HasOne(d => d.Merchant)
                    .WithMany(p => p.DeliveryRedemptionTokens)
                    .HasForeignKey(d => d.MerchantId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_DeliveryRedemptionTokens_Merchants");

                entity.HasOne(d => d.OrderItem)
                    .WithMany(p => p.DeliveryRedemptionTokens)
                    .HasForeignKey(d => d.OrderItemId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_DeliveryRedemptionTokens_OrderItems");

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.DeliveryRedemptionTokens)
                    .HasForeignKey(d => d.ProductId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_DeliveryRedemptionTokens_Products");
            });

            modelBuilder.Entity<DigitalRedemptionTokens>(entity =>
            {
                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.ExpiredDate).HasColumnType("datetime");

                entity.Property(e => e.RedeemedAt).HasColumnType("datetime");

                entity.Property(e => e.Revenue).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.StartDate).HasColumnType("datetime");

                entity.Property(e => e.TokenType).HasDefaultValueSql("((1))");

                entity.Property(e => e.UpdateTokenAt).HasColumnType("datetime");

                entity.HasOne(d => d.Merchant)
                    .WithMany(p => p.DigitalRedemptionTokens)
                    .HasForeignKey(d => d.MerchantId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_DigitalRedemptionTokens_Merchants");

                entity.HasOne(d => d.OrderItem)
                    .WithMany(p => p.DigitalRedemptionTokens)
                    .HasForeignKey(d => d.OrderItemId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_DigitalRedemptionTokens_OrderItems");

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.DigitalRedemptionTokens)
                    .HasForeignKey(d => d.ProductId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_DigitalRedemptionTokens_Products");
            });

            modelBuilder.Entity<DiscountTypes>(entity =>
            {
                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.Description).IsRequired();

                entity.Property(e => e.LastUpdatedAt).HasColumnType("datetime");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);
            });

            modelBuilder.Entity<Districts>(entity =>
            {
                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.LastUpdatedAt).HasColumnType("datetime");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.HasOne(d => d.Province)
                    .WithMany(p => p.Districts)
                    .HasForeignKey(d => d.ProvinceId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Districts_Provinces");
            });

            modelBuilder.Entity<ErrorLogs>(entity =>
            {
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

            modelBuilder.Entity<ActivityLogs>(entity =>
            {
                entity.Property(e => e.ActionName)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.Property(e => e.CreatedAt).HasColumnType("datetime");    

                entity.Property(e => e.ActionName).IsRequired();
                entity.Property(e => e.Message).IsRequired();

            });


            modelBuilder.Entity<ExpirationTypes>(entity =>
                {
                    entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                    entity.Property(e => e.Description).HasMaxLength(500);

                    entity.Property(e => e.LastUpdatedAt).HasColumnType("datetime");

                    entity.Property(e => e.Name)
                        .IsRequired()
                        .HasMaxLength(100);
                });

            modelBuilder.Entity<FinanceSummary>(entity =>
            {
                entity.Property(e => e.EndDate).HasColumnType("date");

                entity.Property(e => e.PayoutDate).HasColumnType("date");

                entity.Property(e => e.StartDate).HasColumnType("date");

                entity.Property(e => e.TotalPayout)
                    .HasColumnType("decimal(18, 2)")
                    .HasDefaultValueSql("((0.00))");
            });

            modelBuilder.Entity<FinanceTransaction>(entity =>
            {
                entity.Property(e => e.AdminProfit)
                    .HasColumnType("decimal(18, 2)")
                    .HasDefaultValueSql("((0.00))");

                entity.Property(e => e.DefaultCommission)
                    .HasColumnType("numeric(18, 2)")
                    .HasDefaultValueSql("((0.00))");

                entity.Property(e => e.MerchantProfit)
                    .HasColumnType("decimal(18, 2)")
                    .HasDefaultValueSql("((0.00))");

                entity.Property(e => e.ProductTitle)
                    .IsRequired()
                    .HasColumnType("ntext");

                entity.Property(e => e.TotalPrice)
                    .HasColumnType("decimal(18, 2)")
                    .HasDefaultValueSql("((0.00))");

                entity.HasOne(d => d.MerchantFinance)
                    .WithMany(p => p.FinanceTransaction)
                    .HasForeignKey(d => d.MerchantFinanceId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_FinanceTransaction_MerchantFinance");

                entity.HasOne(d => d.OrderItem)
                    .WithMany(p => p.FinanceTransaction)
                    .HasForeignKey(d => d.OrderItemId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_FinanceTransaction_OrderItems");

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.FinanceTransaction)
                    .HasForeignKey(d => d.ProductId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_FinanceTransaction_Products");
            });

            modelBuilder.Entity<GifteeTokens>(entity =>
            {
                entity.Property(e => e.IssuedDate).HasColumnType("datetime");

                entity.Property(e => e.Token).IsRequired();

                entity.Property(e => e.VoucherName).IsRequired();
            });

            modelBuilder.Entity<GoogleMerchantKeywords>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Keyword)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(e => e.Language)
                    .IsRequired()
                    .HasMaxLength(2)
                    .IsUnicode(false);

                entity.Property(e => e.SortBy)
                    .IsRequired()
                    .HasMaxLength(10)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<InStoreRedemptionTokens>(entity =>
            {
                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.ExpiredDate).HasColumnType("datetime");

                entity.Property(e => e.RedeemedAt).HasColumnType("datetime");

                entity.Property(e => e.Revenue).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.StartDate).HasColumnType("datetime");

                entity.Property(e => e.TokenType).HasDefaultValueSql("((1))");

                entity.HasOne(d => d.Merchant)
                    .WithMany(p => p.InStoreRedemptionTokens)
                    .HasForeignKey(d => d.MerchantId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_InStoreRedemptionTokens_Merchants");

                entity.HasOne(d => d.OrderItem)
                    .WithMany(p => p.InStoreRedemptionTokens)
                    .HasForeignKey(d => d.OrderItemId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_InStoreRedemptionTokens_OrderItems");

                entity.HasOne(d => d.Outlet)
                    .WithMany(p => p.InStoreRedemptionTokens)
                    .HasForeignKey(d => d.OutletId)
                    .HasConstraintName("FK_InStoreRedemptionTokens_Outlets");

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.InStoreRedemptionTokens)
                    .HasForeignKey(d => d.ProductId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_InStoreRedemptionTokens_Products");
            });

            modelBuilder.Entity<LuckyDraws>(entity =>
            {
                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.LastUpdatedAt).HasColumnType("datetime");

                entity.Property(e => e.LuckyDrawDate).HasColumnType("datetime");

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.LuckyDraws)
                    .HasForeignKey(d => d.ProductId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_LuckyDraws_Products");

                entity.HasOne(d => d.StatusType)
                    .WithMany(p => p.LuckyDraws)
                    .HasForeignKey(d => d.StatusTypeId)
                    .HasConstraintName("FK_LuckyDraws_StatusTypes");
            });

            modelBuilder.Entity<MailingLists>(entity =>
            {
                entity.HasKey(e => e.Email);

                entity.Property(e => e.Email)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.IsSubscribe)
                    .IsRequired()
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.LastEmailedAt).HasColumnType("datetime");

                entity.Property(e => e.UnSubscribeAt).HasColumnType("datetime");
            });

            modelBuilder.Entity<MerchantFinance>(entity =>
            {
                entity.Property(e => e.Bank).HasColumnType("ntext");

                entity.Property(e => e.BankAccount).HasColumnType("ntext");

                entity.Property(e => e.MerchantDisplayName)
                    .IsRequired()
                    .HasColumnType("ntext");

                entity.Property(e => e.PayoutDate).HasColumnType("date");

                entity.Property(e => e.Remarks).HasColumnType("ntext");

                entity.Property(e => e.StatementOfAccountUrl).HasColumnType("ntext");

                entity.Property(e => e.TotalPayout)
                    .HasColumnType("decimal(18, 2)")
                    .HasDefaultValueSql("((0.00))");

                entity.HasOne(d => d.FinanceSummary)
                    .WithMany(p => p.MerchantFinance)
                    .HasForeignKey(d => d.FinanceSummaryId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_MerchantFinance_FinanceSummary");

                entity.HasOne(d => d.Merchant)
                    .WithMany(p => p.MerchantFinance)
                    .HasForeignKey(d => d.MerchantId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_MerchantFinance_Merchants");
            });

            modelBuilder.Entity<Merchants>(entity =>
            {
                entity.Property(e => e.Address_1).HasMaxLength(255);

                entity.Property(e => e.Address_2).HasMaxLength(255);

                entity.Property(e => e.Code).HasMaxLength(255);

                entity.Property(e => e.CompanyContact).HasMaxLength(255);

                entity.Property(e => e.CompanyName).HasMaxLength(255);

                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.DefaultCommission).HasColumnType("decimal(4, 2)");

                entity.Property(e => e.Description).HasColumnType("ntext");

                entity.Property(e => e.DisplayName).HasMaxLength(255);

                entity.Property(e => e.LastUpdatedAt).HasColumnType("datetime");

                entity.Property(e => e.Rating)
                    .HasColumnType("decimal(4, 2)")
                    .HasDefaultValueSql("((0.00))");

                entity.Property(e => e.RegistrationNumber).HasMaxLength(100);

                entity.Property(e => e.WebsiteSiteUrl).HasMaxLength(255);

                entity.HasOne(d => d.BusinessType)
                    .WithMany(p => p.Merchants)
                    .HasForeignKey(d => d.BusinessTypeId)
                    .HasConstraintName("FK_Merchants_BusinessTypes");

                entity.HasOne(d => d.Country)
                    .WithMany(p => p.Merchants)
                    .HasForeignKey(d => d.CountryId)
                    .HasConstraintName("FK_Merchants_Countries");

                entity.HasOne(d => d.Distrit)
                    .WithMany(p => p.Merchants)
                    .HasForeignKey(d => d.DistritId)
                    .HasConstraintName("FK_Merchants_Districts");

                entity.HasOne(d => d.PostCode)
                    .WithMany(p => p.Merchants)
                    .HasForeignKey(d => d.PostCodeId)
                    .HasConstraintName("FK_Merchants_PostCodes");

                entity.HasOne(d => d.Province)
                    .WithMany(p => p.Merchants)
                    .HasForeignKey(d => d.ProvinceId)
                    .HasConstraintName("FK_Merchants_Provinces");

                entity.HasOne(d => d.StatusType)
                    .WithMany(p => p.Merchants)
                    .HasForeignKey(d => d.StatusTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Merchants_StatusTypes");
            });

            modelBuilder.Entity<OrderItemExternal>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.DiscountName)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.DiscountPriceValue).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.DiscountedAmount).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.ExternalItemId)
                    .IsRequired()
                    .HasMaxLength(64)
                    .IsUnicode(false);

                entity.Property(e => e.ExternalUrl).IsRequired();

                entity.Property(e => e.JsonData).IsUnicode(false);

                entity.Property(e => e.LastUpdatedAt).HasColumnType("datetime");

                entity.Property(e => e.LastUpdatedByUser).HasMaxLength(255);

                entity.Property(e => e.OriginalPrice).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.ProductCartPreviewSmallImageURL)
                    .IsRequired()
                    .IsUnicode(false);

                entity.Property(e => e.ProductTitle).IsRequired();

                entity.Property(e => e.ProductVariation).IsUnicode(false);

                entity.Property(e => e.ShortId)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.SubTotal).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.SubtotalPrice).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.TotalPrice).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.VPointsMultiplier).HasColumnType("decimal(5, 2)");

                entity.Property(e => e.VPointsMultiplierCap).HasColumnType("decimal(5, 2)");

                entity.Property(e => e.VariationText).HasMaxLength(255);

                entity.HasOne(d => d.OrderShopExternal)
                    .WithMany(p => p.OrderItemExternal)
                    .HasForeignKey(d => d.OrderShopExternalId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_OrderItemExternal_OrderShopExternal");
            });

            modelBuilder.Entity<OrderItems>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Commision).HasColumnType("decimal(3, 2)");

                entity.Property(e => e.DiscountedAmount).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.MerchantDisplayName)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.ProductDetail).IsRequired();

                entity.Property(e => e.ProductImageFolderUrl)
                    .IsRequired()
                    .HasMaxLength(500)
                    .IsUnicode(false);

                entity.Property(e => e.ProductTitle)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(e => e.ShippingCost).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.ShippingCostDiscount).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.ShippingCostSubTotal).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.ShortId)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.SubtotalPrice).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.VPointsMultiplier).HasColumnType("decimal(5, 2)");

                entity.Property(e => e.VPointsMultiplierCap).HasColumnType("decimal(5, 2)");

                entity.Property(e => e.VariationText).HasMaxLength(255);

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

            modelBuilder.Entity<OrderItemsExternalStatusTypes>(entity =>
            {
                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.Description).HasMaxLength(500);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);
            });

            modelBuilder.Entity<OrderPayments>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.AuthCode)
                    .HasMaxLength(40)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.CreditCardBankName)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.CreditCardIssuingCountry)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.CreditCardName)
                    .HasMaxLength(200)
                    .IsUnicode(false);

                entity.Property(e => e.CreditCardNumber)
                    .HasMaxLength(16)
                    .IsUnicode(false);

                entity.Property(e => e.Currency)
                    .IsRequired()
                    .HasMaxLength(5)
                    .IsUnicode(false);

                entity.Property(e => e.ErrorDescription)
                    .HasMaxLength(200)
                    .IsUnicode(false);

                entity.Property(e => e.JsonResponse)
                    .IsRequired()
                    .IsUnicode(false);

                entity.Property(e => e.MerchantCode)
                    .IsRequired()
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.Remark).HasMaxLength(500);

                entity.Property(e => e.Signature)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.Status)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.TransactionId)
                    .IsRequired()
                    .HasMaxLength(128)
                    .IsUnicode(false);

                entity.HasOne(d => d.RefNoNavigation)
                    .WithMany(p => p.OrderPayments)
                    .HasForeignKey(d => d.RefNo)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_OrderPayments_Orders");
            });

            modelBuilder.Entity<OrderShopExternal>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.ExternalOrderId)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.ExternalShopId)
                    .IsRequired()
                    .HasMaxLength(64)
                    .IsUnicode(false);

                entity.Property(e => e.ExternalShopName).IsRequired();

                entity.Property(e => e.ExternalShopUrl)
                    .IsRequired()
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.LastUpdatedAt).HasColumnType("datetime");

                entity.Property(e => e.LastUpdatedByUser).HasMaxLength(255);

                entity.Property(e => e.ShippingCost).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.ShippingCostDiscount).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.ShippingCostSubTotal).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.ShippingLatestStatus)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.TotalPrice).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.TrackingNo).IsUnicode(false);

                entity.HasOne(d => d.Order)
                    .WithMany(p => p.OrderShopExternal)
                    .HasForeignKey(d => d.OrderId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_OrderShopExternal_Orders");
            });

            modelBuilder.Entity<Orders>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.BillingAddressLine1)
                    .IsRequired()
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.BillingAddressLine2)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.BillingCity)
                    .IsRequired()
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.BillingCountry)
                    .IsRequired()
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.BillingEmail)
                    .IsRequired()
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.BillingMobileCountryCode)
                    .IsRequired()
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.BillingMobileNumber)
                    .IsRequired()
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.BillingPersonFirstName)
                    .IsRequired()
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.BillingPersonLastName)
                    .IsRequired()
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.BillingPostcode)
                    .IsRequired()
                    .HasMaxLength(15)
                    .IsUnicode(false);

                entity.Property(e => e.BillingState)
                    .IsRequired()
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.PromoCodeDiscountValue).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.PromoCodeExpireOn).HasColumnType("datetime");

                entity.Property(e => e.PromoCodeMaxDiscountValue).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.PromoCodeMinSpend).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.PromoCodeValue)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.ShippingAddressLine1)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.ShippingAddressLine2)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.ShippingCity)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.ShippingCostDiscount).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.ShippingCostSubTotal).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.ShippingCountry)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.ShippingEmail)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.ShippingMobileCountryCode)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.ShippingMobileNumber)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.ShippingPersonFirstName)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.ShippingPersonLastName)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.ShippingPostcode)
                    .HasMaxLength(15)
                    .IsUnicode(false);

                entity.Property(e => e.ShippingState)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.ShortId)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.TotalPrice).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.TotalPriceBeforePromoCodeDiscount).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.TotalPromoCodeDiscount).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.TotalShippingCost).HasColumnType("decimal(18, 2)");
            });

            modelBuilder.Entity<Outlets>(entity =>
            {
                entity.Property(e => e.Address_1).HasMaxLength(255);

                entity.Property(e => e.Address_2).HasMaxLength(255);

                entity.Property(e => e.Contact).HasMaxLength(255);

                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.ImgUrl).HasMaxLength(255);

                entity.Property(e => e.LastUpdatedAt).HasColumnType("datetime");

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
                    .HasConstraintName("FK_Outlets_Countries");

                entity.HasOne(d => d.District)
                    .WithMany(p => p.Outlets)
                    .HasForeignKey(d => d.DistrictId)
                    .HasConstraintName("FK_Outlets_Districts");

                entity.HasOne(d => d.Merchant)
                    .WithMany(p => p.Outlets)
                    .HasForeignKey(d => d.MerchantId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Outlets_Merchants");

                entity.HasOne(d => d.PostCode)
                    .WithMany(p => p.Outlets)
                    .HasForeignKey(d => d.PostCodeId)
                    .HasConstraintName("FK_Outlets_PostCodes");

                entity.HasOne(d => d.Province)
                    .WithMany(p => p.Outlets)
                    .HasForeignKey(d => d.ProvinceId)
                    .HasConstraintName("FK_Outlets_Provinces");
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

            modelBuilder.Entity<PersonInCharges>(entity =>
            {
                entity.Property(e => e.Contact).HasMaxLength(255);

                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.IdentityNumber).HasMaxLength(255);

                entity.Property(e => e.LastUpdatedAt).HasColumnType("datetime");

                entity.Property(e => e.Name).HasMaxLength(255);

                entity.Property(e => e.Position).HasMaxLength(255);

                entity.HasOne(d => d.Merchant)
                    .WithMany(p => p.PersonInCharges)
                    .HasForeignKey(d => d.MerchantId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_PersonInCharges_Merchants");

                entity.HasOne(d => d.StatusType)
                    .WithMany(p => p.PersonInCharges)
                    .HasForeignKey(d => d.StatusTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_PersonInCharges_StatusTypes");
            });

            modelBuilder.Entity<PostCodes>(entity =>
            {
                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.LastUpdatedAt).HasColumnType("datetime");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.HasOne(d => d.District)
                    .WithMany(p => p.PostCodes)
                    .HasForeignKey(d => d.DistrictId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_PostCodes_Districts");
            });

            modelBuilder.Entity<ProductCategories>(entity =>
            {
                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.LastUpdatedAt).HasColumnType("datetime");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);
            });

            modelBuilder.Entity<ProductDemographicsTarget>(entity =>
            {
                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.ProductDemographicsTarget)
                    .HasForeignKey(d => d.ProductId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ProductDemographicsTarget_Products");
            });

            modelBuilder.Entity<ProductDiscounts>(entity =>
            {
                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.EndDate).HasColumnType("datetime");

                entity.Property(e => e.LastUpdatedAt).HasColumnType("datetime");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(e => e.PercentageValue)
                    .HasColumnType("decimal(18, 2)")
                    .HasDefaultValueSql("((0.00))");

                entity.Property(e => e.PriceValue).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.StartDate).HasColumnType("datetime");

                entity.HasOne(d => d.DiscountType)
                    .WithMany(p => p.ProductDiscounts)
                    .HasForeignKey(d => d.DiscountTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ProductDiscounts_DiscountTypes");

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.ProductDiscounts)
                    .HasForeignKey(d => d.ProductId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ProductDiscounts_Products");
            });

            modelBuilder.Entity<ProductOutlets>(entity =>
            {
                entity.HasKey(e => new { e.ProductId, e.OutletId });

                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.LastUpdatedAt).HasColumnType("datetime");

                entity.HasOne(d => d.Outlet)
                    .WithMany(p => p.ProductOutlets)
                    .HasForeignKey(d => d.OutletId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ProductOutlets_Outlets");

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.ProductOutlets)
                    .HasForeignKey(d => d.ProductId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ProductOutlets_Products");
            });

            modelBuilder.Entity<ProductReview>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Comment).IsRequired();

                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.MemberName)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(e => e.ProductTitle).IsRequired();

                entity.Property(e => e.Rating).HasColumnType("decimal(4, 2)");

                entity.HasOne(d => d.Merchant)
                    .WithMany(p => p.ProductReview)
                    .HasForeignKey(d => d.MerchantId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ProductReview_Merchants");

                entity.HasOne(d => d.OrderItem)
                    .WithMany(p => p.ProductReview)
                    .HasForeignKey(d => d.OrderItemId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ProductReview_OrderItems");

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.ProductReview)
                    .HasForeignKey(d => d.ProductId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ProductReview_Products");
            });

            modelBuilder.Entity<ProductReviewReply>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Comment).IsRequired();

                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.CreatedBy)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.HasOne(d => d.ProductReview)
                    .WithMany(p => p.ProductReviewReply)
                    .HasForeignKey(d => d.ProductReviewId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ProductReviewReply_ProductReview");
            });

            modelBuilder.Entity<ProductReviewUploads>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.FileUrl)
                    .IsRequired()
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.MimeType)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.HasOne(d => d.ProductReview)
                    .WithMany(p => p.ProductReviewUploads)
                    .HasForeignKey(d => d.ProductReviewId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ProductReviewUploads_ProductReview");
            });

            modelBuilder.Entity<ProductShippingCost>(entity =>
            {
                entity.Property(e => e.ConditionalShippingCost).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.LastUpdatedAt).HasColumnType("datetime");

                entity.Property(e => e.ShippingTypeId).HasDefaultValueSql("((1))");

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.ProductShippingCost)
                    .HasForeignKey(d => d.ProductId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ProductShippingCost_Products");
            });

            modelBuilder.Entity<ProductSubCategories>(entity =>
            {
                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.LastUpdatedAt).HasColumnType("datetime");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.HasOne(d => d.Category)
                    .WithMany(p => p.ProductSubCategories)
                    .HasForeignKey(d => d.CategoryId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ProductSubCategories_ProductCategories");
            });

            modelBuilder.Entity<ProductVariation>(entity =>
            {
                entity.HasIndex(e => e.VariationCombinationId)
                    .HasName("Unique_VariationCombinationId")
                    .IsUnique();

                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.DiscountedPrice).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.LastUpdatedAt).HasColumnType("datetime");

                entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.SKU).IsRequired();

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.ProductVariation)
                    .HasForeignKey(d => d.ProductId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ProductVariation_Products1");

                entity.HasOne(d => d.VariationCombination)
                    .WithOne(p => p.ProductVariation)
                    .HasForeignKey<ProductVariation>(d => d.VariationCombinationId)
                    .HasConstraintName("FK_ProductVariation_VariationCombination1");
            });

            modelBuilder.Entity<Products>(entity =>
            {
                entity.Property(e => e.ActualPriceForVpoints)
                    .HasColumnType("decimal(18, 2)")
                    .HasDefaultValueSql("((0.00))");

                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.DefaultCommission).HasColumnType("decimal(4, 2)");

                entity.Property(e => e.DiscountedPrice).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.EndDate).HasColumnType("datetime");

                entity.Property(e => e.LastUpdatedAt).HasColumnType("datetime");

                entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.Rating)
                    .HasColumnType("decimal(4, 2)")
                    .HasDefaultValueSql("((0.00))");

                entity.Property(e => e.StartDate).HasColumnType("datetime");

                entity.Property(e => e.Subtitle).HasMaxLength(255);

                entity.Property(e => e.Title).HasMaxLength(255);

                entity.Property(e => e.TotalBought).HasDefaultValueSql("((0))");

                entity.HasOne(d => d.DealExpiration)
                    .WithMany(p => p.Products)
                    .HasForeignKey(d => d.DealExpirationId)
                    .HasConstraintName("FK_Products_DealExpirations");

                entity.HasOne(d => d.DealType)
                    .WithMany(p => p.Products)
                    .HasForeignKey(d => d.DealTypeId)
                    .HasConstraintName("FK_Products_DealTypes");

                entity.HasOne(d => d.LuckyDraw)
                    .WithMany(p => p.Products)
                    .HasForeignKey(d => d.LuckyDrawId)
                    .HasConstraintName("FK_Products_LuckyDraws");

                entity.HasOne(d => d.Merchant)
                    .WithMany(p => p.Products)
                    .HasForeignKey(d => d.MerchantId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Products_Merchants");

                entity.HasOne(d => d.ProductCategory)
                    .WithMany(p => p.Products)
                    .HasForeignKey(d => d.ProductCategoryId)
                    .HasConstraintName("FK_Products_ProductCategories");

                entity.HasOne(d => d.ProductSubCategory)
                    .WithMany(p => p.Products)
                    .HasForeignKey(d => d.ProductSubCategoryId)
                    .HasConstraintName("FK_Products_ProductSubCategories");

                entity.HasOne(d => d.StatusType)
                    .WithMany(p => p.Products)
                    .HasForeignKey(d => d.StatusTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Products_StatusTypes");
            });

            modelBuilder.Entity<PromoCodeSelectedUsers>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.RedeemedAt).HasColumnType("datetime");

                entity.HasOne(d => d.PromoCode)
                    .WithMany(p => p.PromoCodeSelectedUsers)
                    .HasForeignKey(d => d.PromoCodeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_PromoCodeSelectedUsers_PromoCodes");
            });

            modelBuilder.Entity<PromoCodes>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.Description)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.Property(e => e.DiscountValue).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.ExpireOn).HasColumnType("datetime");

                entity.Property(e => e.LastUpdatedAt).HasColumnType("datetime");

                entity.Property(e => e.MaxDiscountValue).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.MinSpend).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.PromoCode)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<Provinces>(entity =>
            {
                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.LastUpdatedAt).HasColumnType("datetime");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.HasOne(d => d.Country)
                    .WithMany(p => p.Provinces)
                    .HasForeignKey(d => d.CountryId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Provinces_Countries");
            });

            modelBuilder.Entity<RefundPayments>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.AuthCode)
                    .IsRequired()
                    .HasMaxLength(40)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.CreditCardBankName)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.CreditCardIssuingCountry)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.CreditCardName)
                    .HasMaxLength(200)
                    .IsUnicode(false);

                entity.Property(e => e.CreditCardNumber)
                    .HasMaxLength(16)
                    .IsUnicode(false);

                entity.Property(e => e.Currency)
                    .IsRequired()
                    .HasMaxLength(5)
                    .IsUnicode(false);

                entity.Property(e => e.ErrorDescription)
                    .HasMaxLength(200)
                    .IsUnicode(false);

                entity.Property(e => e.JsonResponse)
                    .IsRequired()
                    .IsUnicode(false);

                entity.Property(e => e.MerchantCode)
                    .IsRequired()
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.Remark).HasMaxLength(500);

                entity.Property(e => e.Signature)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.Status)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.TransactionId)
                    .IsRequired()
                    .HasMaxLength(128)
                    .IsUnicode(false);

                entity.HasOne(d => d.RefNoNavigation)
                    .WithMany(p => p.RefundPayments)
                    .HasForeignKey(d => d.RefNo)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_RefundPayments_Refunds");
            });

            modelBuilder.Entity<Refunds>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.MoneyRefunded).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.RefundJsonResponse).IsUnicode(false);

                entity.Property(e => e.RefundMethod).HasDefaultValueSql("((1))");

                entity.Property(e => e.RefundTransactionId)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.Remark).HasMaxLength(1000);

                entity.Property(e => e.ShortId)
                    .IsRequired()
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.HasOne(d => d.OrderItem)
                    .WithMany(p => p.Refunds)
                    .HasForeignKey(d => d.OrderItemId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Refunds_OrderItems");
            });

            modelBuilder.Entity<RefundsExternalOrderItems>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.MoneyRefunded).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.RefundJsonResponse).IsUnicode(false);

                entity.Property(e => e.RefundMethod).HasDefaultValueSql("((1))");

                entity.Property(e => e.RefundTransactionId)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.Remark).HasMaxLength(1000);

                entity.Property(e => e.ShortId)
                    .IsRequired()
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.HasOne(d => d.OrderItem)
                    .WithMany(p => p.RefundsExternalOrderItems)
                    .HasForeignKey(d => d.OrderItemId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_RefundsExternalOrderItems_OrderExternalItems");
            });

            modelBuilder.Entity<RoleClaims>(entity =>
            {
                entity.HasIndex(e => e.RoleId)
                    .HasName("IX_AspNetRoleClaims_RoleId");

                entity.HasOne(d => d.Role)
                    .WithMany(p => p.RoleClaims)
                    .HasForeignKey(d => d.RoleId)
                    .HasConstraintName("FK_AspNetRoleClaims_AspNetRoles_RoleId");
            });

            modelBuilder.Entity<Roles>(entity =>
            {
                entity.HasIndex(e => e.NormalizedName)
                    .HasName("RoleNameIndex")
                    .IsUnique()
                    .HasFilter("([NormalizedName] IS NOT NULL)");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Name).HasMaxLength(256);

                entity.Property(e => e.NormalizedName).HasMaxLength(256);
            });

            modelBuilder.Entity<ShippingCost>(entity =>
            {
                entity.Property(e => e.Cost).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.LastUpdatedAt).HasColumnType("datetime");

                entity.HasOne(d => d.ProductShipping)
                    .WithMany(p => p.ShippingCost)
                    .HasForeignKey(d => d.ProductShippingId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ShippingCost_ProductShippingCost");

                entity.HasOne(d => d.Province)
                    .WithMany(p => p.ShippingCost)
                    .HasForeignKey(d => d.ProvinceId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ShippingCost_Provinces");
            });

            modelBuilder.Entity<ShippingTypes>(entity =>
            {
                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.LastUpdatedAt).HasColumnType("datetime");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);
            });

            modelBuilder.Entity<StatusTypes>(entity =>
            {
                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.Description).HasMaxLength(500);

                entity.Property(e => e.LastUpdatedAt).HasColumnType("datetime");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);
            });

            modelBuilder.Entity<TempUsers>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.BusinessName)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(128)
                    .IsUnicode(false);

                entity.Property(e => e.MobileNumber)
                    .IsRequired()
                    .HasMaxLength(12)
                    .IsUnicode(false);

                entity.Property(e => e.TAC)
                    .IsRequired()
                    .HasMaxLength(6)
                    .IsUnicode(false);

                entity.Property(e => e.TACRequestedAt).HasColumnType("datetime");

                entity.Property(e => e.TACVerifiedAt).HasColumnType("datetime");
            });

            modelBuilder.Entity<ThirdPartyProducts>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.ExternalId)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.HasOne(d => d.ThirdPartyType)
                    .WithMany(p => p.ThirdPartyProducts)
                    .HasForeignKey(d => d.ThirdPartyTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ThirdPartyProducts_ThirdPartyTypes");
            });

            modelBuilder.Entity<ThirdPartyTypes>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(255);
            });

            modelBuilder.Entity<UserClaims>(entity =>
            {
                entity.HasIndex(e => e.UserId)
                    .HasName("IX_AspNetUserClaims_UserId");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.UserClaims)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK_AspNetUserClaims_AspNetUsers_UserId");
            });

            modelBuilder.Entity<UserLogins>(entity =>
            {
                entity.HasKey(e => new { e.LoginProvider, e.ProviderKey })
                    .HasName("PK_AspNetUserLogins");

                entity.HasIndex(e => e.UserId)
                    .HasName("IX_AspNetUserLogins_UserId");

                entity.Property(e => e.LoginProvider).HasMaxLength(128);

                entity.Property(e => e.ProviderKey).HasMaxLength(128);

                entity.HasOne(d => d.User)
                    .WithMany(p => p.UserLogins)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK_AspNetUserLogins_AspNetUsers_UserId");
            });

            modelBuilder.Entity<UserMobileTAC>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.ExpiredAt).HasColumnType("datetime");

                entity.Property(e => e.MobileNumber)
                    .IsRequired()
                    .HasMaxLength(12)
                    .IsUnicode(false);

                entity.Property(e => e.TAC)
                    .IsRequired()
                    .HasMaxLength(6)
                    .IsUnicode(false);

                entity.Property(e => e.VerifiedAt).HasColumnType("datetime");
            });

            modelBuilder.Entity<UserRoles>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.RoleId })
                    .HasName("PK_UserRoles_1");

                entity.HasIndex(e => e.RoleId)
                    .HasName("IX_AspNetUserRoles_RoleId");

                entity.HasOne(d => d.Merchant)
                    .WithMany(p => p.UserRoles)
                    .HasForeignKey(d => d.MerchantId)
                    .HasConstraintName("FK_UserRoles_Merchants");

                entity.HasOne(d => d.Role)
                    .WithMany(p => p.UserRoles)
                    .HasForeignKey(d => d.RoleId)
                    .HasConstraintName("FK_AspNetUserRoles_AspNetRoles_RoleId");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.UserRoles)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK_AspNetUserRoles_AspNetUsers_UserId");
            });

            modelBuilder.Entity<UserTokens>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.LoginProvider, e.Name })
                    .HasName("PK_AspNetUserTokens");

                entity.Property(e => e.LoginProvider).HasMaxLength(128);

                entity.Property(e => e.Name).HasMaxLength(128);

                entity.HasOne(d => d.User)
                    .WithMany(p => p.UserTokens)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK_AspNetUserTokens_AspNetUsers_UserId");
            });

            modelBuilder.Entity<Users>(entity =>
            {
                entity.HasIndex(e => e.NormalizedEmail)
                    .HasName("EmailIndex");

                entity.HasIndex(e => e.NormalizedUserName)
                    .HasName("UserNameIndex")
                    .IsUnique()
                    .HasFilter("([NormalizedUserName] IS NOT NULL)");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.Email).HasMaxLength(256);

                entity.Property(e => e.NormalizedEmail).HasMaxLength(256);

                entity.Property(e => e.NormalizedUserName).HasMaxLength(256);

                entity.Property(e => e.UserName).HasMaxLength(256);
            });

            modelBuilder.Entity<VariationCombination>(entity =>
            {
                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.LastUpdatedAt).HasColumnType("datetime");

                entity.HasOne(d => d.OptionOne)
                    .WithMany(p => p.VariationCombination)
                    .HasForeignKey(d => d.OptionOneId)
                    .HasConstraintName("FK_VariationCombination_VariationOptions1");
            });

            modelBuilder.Entity<VariationOptions>(entity =>
            {
                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.LastUpdatedAt).HasColumnType("datetime");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.HasOne(d => d.Variation)
                    .WithMany(p => p.VariationOptions)
                    .HasForeignKey(d => d.VariationId)
                    .HasConstraintName("FK_VariationOptions_Variations1");
            });

            modelBuilder.Entity<Variations>(entity =>
            {
                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.LastUpdatedAt).HasColumnType("datetime");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.Variations)
                    .HasForeignKey(d => d.ProductId)
                    .HasConstraintName("FK_Variations_Products");
            });

            modelBuilder.Entity<ConsoleMerchantToCrawl>(entity =>
            {
              
            });

            modelBuilder.Entity<ConsoleMerchantJSON>(entity =>
            {

            });

            modelBuilder.Entity<ConsoleProductJSON>(entity =>
            {

            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
