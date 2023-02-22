using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Voupon.Merchant.WebApp.Entities
{
    public partial class RewardsDBContext : DbContext
    {
        public RewardsDBContext()
        {
        }

        public RewardsDBContext(DbContextOptions<RewardsDBContext> options)
            : base(options)
        {
        }

        public virtual DbSet<BankAccounts> BankAccounts { get; set; }
        public virtual DbSet<Banks> Banks { get; set; }
        public virtual DbSet<BusinessTypes> BusinessTypes { get; set; }
        public virtual DbSet<Countries> Countries { get; set; }
        public virtual DbSet<DealExpirations> DealExpirations { get; set; }
        public virtual DbSet<DealTypes> DealTypes { get; set; }
        public virtual DbSet<Districts> Districts { get; set; }
        public virtual DbSet<ExpirationTypes> ExpirationTypes { get; set; }
        public virtual DbSet<LuckyDraws> LuckyDraws { get; set; }
        public virtual DbSet<Merchants> Merchants { get; set; }
        public virtual DbSet<Outlets> Outlets { get; set; }
        public virtual DbSet<PersonInCharges> PersonInCharges { get; set; }
        public virtual DbSet<PostCodes> PostCodes { get; set; }
        public virtual DbSet<ProductCategories> ProductCategories { get; set; }
        public virtual DbSet<ProductOutlets> ProductOutlets { get; set; }
        public virtual DbSet<ProductSubCategories> ProductSubCategories { get; set; }
        public virtual DbSet<Products> Products { get; set; }
        public virtual DbSet<Provinces> Provinces { get; set; }
        public virtual DbSet<RoleClaims> RoleClaims { get; set; }
        public virtual DbSet<Roles> Roles { get; set; }
        public virtual DbSet<StatusTypes> StatusTypes { get; set; }
        public virtual DbSet<TempUsers> TempUsers { get; set; }
        public virtual DbSet<UserClaims> UserClaims { get; set; }
        public virtual DbSet<UserLogins> UserLogins { get; set; }
        public virtual DbSet<UserMobileTAC> UserMobileTAC { get; set; }
        public virtual DbSet<UserRoles> UserRoles { get; set; }
        public virtual DbSet<UserTokens> UserTokens { get; set; }
        public virtual DbSet<Users> Users { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
                optionsBuilder.UseNpgsql("Server=tcp:vodus-live.database.windows.net,1433;Database=RewardsDB;Trusted_Connection=false;Encrypt=True;TrustServerCertificate=False;User id=Vodus;password=Pleasechangethis2017");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BankAccounts>(entity =>
            {
                entity.Property(e => e.AccountNumber).HasMaxLength(50);

                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.DocumentUrl).HasMaxLength(255);

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

            modelBuilder.Entity<ExpirationTypes>(entity =>
            {
                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.Description).HasMaxLength(500);

                entity.Property(e => e.LastUpdatedAt).HasColumnType("datetime");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);
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

            modelBuilder.Entity<Merchants>(entity =>
            {
                entity.Property(e => e.Address_1).HasMaxLength(255);

                entity.Property(e => e.Address_2).HasMaxLength(255);

                entity.Property(e => e.BIDDocumentUrl).HasMaxLength(255);

                entity.Property(e => e.Code).HasMaxLength(255);

                entity.Property(e => e.CompanyContact).HasMaxLength(255);

                entity.Property(e => e.CompanyName).HasMaxLength(255);

                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.Description).HasColumnType("ntext");

                entity.Property(e => e.DisplayName).HasMaxLength(255);

                entity.Property(e => e.LastUpdatedAt).HasColumnType("datetime");

                entity.Property(e => e.LogoUrl).HasMaxLength(255);

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

            modelBuilder.Entity<PersonInCharges>(entity =>
            {
                entity.Property(e => e.Contact).HasMaxLength(255);

                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.DocumentUrl).HasMaxLength(255);

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

            modelBuilder.Entity<Products>(entity =>
            {
                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.DiscountedPrice).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.EndDate).HasColumnType("datetime");

                entity.Property(e => e.ImageUrl)
                    .HasMaxLength(10)
                    .IsFixedLength();

                entity.Property(e => e.LastUpdatedAt).HasColumnType("datetime");

                entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");

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

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
