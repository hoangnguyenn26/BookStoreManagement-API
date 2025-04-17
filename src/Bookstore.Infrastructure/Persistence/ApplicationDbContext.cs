
using Bookstore.Domain.Entities;
using Bookstore.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System.Reflection; // Cần cho Assembly

namespace Bookstore.Infrastructure.Persistence
{
    public class ApplicationDbContext : DbContext
    {
        private static readonly Guid AdminRoleId = new Guid("E1F3E5D4-1111-4F6F-9C5C-9B8D3A5B2A01");
        private static readonly Guid UserRoleId = new Guid("A2E4F6A8-2222-4D8E-8A4B-8A7C2B4E1F02");

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Khai báo các DbSet cho các Entities
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Role> Roles { get; set; } = null!;
        public DbSet<Book> Books { get; set; } = null!;
        public DbSet<Category> Categories { get; set; } = null!;
        public DbSet<Author> Authors { get; set; } = null!;
        public DbSet<UserRole> UserRoles { get; set; } = null!;
        public DbSet<Address> Addresses { get; set; } = null!;
        public DbSet<Order> Orders { get; set; } = null!;
        public DbSet<OrderDetail> OrderDetails { get; set; } = null!;
        public DbSet<InventoryLog> InventoryLogs { get; set; } = null!;
        public DbSet<WishlistItem> WishlistItems { get; set; } = null!;
        public DbSet<CartItem> CartItems { get; set; } = null!;
        public DbSet<Supplier> Suppliers { get; set; } = null!;
        public DbSet<StockReceipt> StockReceipts { get; set; } = null!;
        public DbSet<StockReceiptDetail> StockReceiptDetails { get; set; } = null!;
        public DbSet<OrderShippingAddress> OrderShippingAddresses { get; set; } = null!;
        public DbSet<Promotion> Promotions { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // ----- Cấu hình UserRole (Many-to-Many) -----
            builder.Entity<UserRole>(entity =>
            {
                // Khóa chính kết hợp
                entity.HasKey(ur => new { ur.UserId, ur.RoleId });

                // Mối quan hệ với User
                entity.HasOne(ur => ur.User)
                      .WithMany(u => u.UserRoles)
                      .HasForeignKey(ur => ur.UserId)
                      .IsRequired()
                      .OnDelete(DeleteBehavior.Cascade);

                // Mối quan hệ với Role
                entity.HasOne(ur => ur.Role)
                      .WithMany(r => r.UserRoles)
                      .HasForeignKey(ur => ur.RoleId)
                      .IsRequired()
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ----- Cấu hình Address (One-to-Many với User) -----
            builder.Entity<Address>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Street).IsRequired().HasMaxLength(256);
                entity.Property(e => e.Village).IsRequired().HasMaxLength(100);
                entity.Property(e => e.District).IsRequired().HasMaxLength(100);
                entity.Property(e => e.City).IsRequired().HasMaxLength(100);
                entity.Property(e => e.IsDefault).IsRequired();

                // Mối quan hệ với User
                entity.HasOne(a => a.User)
                      .WithMany(u => u.Addresses)
                      .HasForeignKey(a => a.UserId)
                      .IsRequired()
                      .OnDelete(DeleteBehavior.Cascade); // Xóa địa chỉ nếu User bị xóa

            });

            // ----- Cấu hình Order (One-to-Many với User, One-to-Many với OrderDetail) -----
            builder.Entity<Order>(entity =>
            {

                entity.Property(e => e.TotalAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Status).HasConversion<byte>();
                entity.Property(e => e.OrderType).HasConversion<byte>();
                entity.Property(e => e.PaymentMethod).HasConversion<byte>();
                entity.Property(e => e.PaymentStatus).HasConversion<string>().HasMaxLength(50).HasDefaultValue(PaymentStatus.Pending);
                entity.Property(e => e.DeliveryMethod).HasConversion<byte>();
                entity.Property(e => e.InvoiceNumber).HasMaxLength(50);
                entity.HasIndex(e => e.InvoiceNumber).IsUnique().HasFilter("[InvoiceNumber] IS NOT NULL"); // Index unique nếu không null

                entity.HasOne(o => o.User)
                      .WithMany(u => u.Orders)
                      .HasForeignKey(o => o.UserId)
                      .IsRequired(false)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(o => o.OrderShippingAddress)
                      .WithMany()
                      .HasForeignKey(o => o.OrderShippingAddressId)
                      .IsRequired(false)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // ----- Cấu hình OrderDetail (Many-to-One với Order, Many-to-One với Book) -----
            builder.Entity<OrderDetail>(entity =>
            {
                entity.Property(e => e.UnitPrice).HasColumnType("decimal(18,2)");

                entity.HasOne(od => od.Order)
                      .WithMany(o => o.OrderDetails)
                      .HasForeignKey(od => od.OrderId)
                      .IsRequired()
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(od => od.Book)
                      .WithMany()
                      .HasForeignKey(od => od.BookId)
                      .IsRequired()
                      .OnDelete(DeleteBehavior.Restrict); // Ngăn xóa Book nếu nó nằm trong OrderDetail
            });


            // ----- Cấu hình lại Book & Category nếu cần -----
            builder.Entity<Book>(entity =>
            {
                entity.Property(e => e.Price).HasColumnType("decimal(18,2)");

                // Cấu hình Soft Delete Filter (Rất quan trọng!)
                entity.HasQueryFilter(b => !b.IsDeleted);

                entity.HasOne(d => d.Category)
                      .WithMany(p => p.Books)
                      .HasForeignKey(d => d.CategoryId)
                      .IsRequired()
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(d => d.Author)
                      .WithMany(p => p.Books)
                      .HasForeignKey(d => d.AuthorId)
                      .OnDelete(DeleteBehavior.SetNull); // Nếu Author bị xóa, AuthorId trong Book thành NULL
            });

            builder.Entity<Category>(entity =>
            {
                // Cấu hình Soft Delete Filter
                entity.HasQueryFilter(c => !c.IsDeleted);

                // Cấu hình mối quan hệ tự tham chiếu cho ParentCategory 
                entity.HasOne(c => c.ParentCategory)
                      .WithMany(c => c.SubCategories)
                      .HasForeignKey(c => c.ParentCategoryId)
                      .OnDelete(DeleteBehavior.Restrict); // Ngăn xóa danh mục cha nếu có danh mục con
            });

            // ----- Cập nhật cấu hình User -----
            builder.Entity<User>(entity =>
            {
                entity.HasIndex(e => e.UserName).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.FirstName).HasMaxLength(100);
                entity.Property(e => e.LastName).HasMaxLength(100);
            });
            // ----- Cấu hình InventoryLog -----
            builder.Entity<InventoryLog>(entity =>
            {
                entity.Property(e => e.Reason).HasConversion<byte>();
                entity.HasOne(il => il.Book)
                      .WithMany()
                      .HasForeignKey(il => il.BookId)
                      .IsRequired()
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(il => il.Order)
                      .WithMany()
                      .HasForeignKey(il => il.OrderId)
                      .IsRequired(false)
                      .OnDelete(DeleteBehavior.SetNull);
                entity.HasOne(il => il.User)
                      .WithMany()
                      .HasForeignKey(il => il.UserId)
                      .IsRequired(false)
                      .OnDelete(DeleteBehavior.SetNull); // Nếu User bị xóa, set UserId trong log thành NULL
                entity.HasOne(il => il.StockReceipt)
                      .WithMany()
                      .HasForeignKey(il => il.StockReceiptId)
                      .IsRequired(false)
                      .OnDelete(DeleteBehavior.SetNull); // Nếu phiếu nhập bị xóa, chỉ set null trong log
            });

            // ----- Cấu hình WishlistItem -----
            builder.Entity<WishlistItem>(entity =>
            {
                entity.HasIndex(wi => new { wi.UserId, wi.BookId }).IsUnique();

                entity.HasOne(wi => wi.User)
                      .WithMany()
                      .HasForeignKey(wi => wi.UserId)
                      .IsRequired()
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(wi => wi.Book)
                      .WithMany()
                      .HasForeignKey(wi => wi.BookId)
                      .IsRequired()
                      .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<CartItem>(entity =>
            {
                entity.HasKey(ci => new { ci.UserId, ci.BookId });
                entity.Property(ci => ci.Quantity).IsRequired();
                entity.ToTable(t => t.HasCheckConstraint("CK_CartItems_Quantity", "[Quantity] > 0"));

                entity.HasOne(ci => ci.User)
                      .WithMany()
                      .HasForeignKey(ci => ci.UserId)
                      .IsRequired()
                      .OnDelete(DeleteBehavior.Cascade);

                // Mối quan hệ với Book
                entity.HasOne(ci => ci.Book)
                      .WithMany()
                      .HasForeignKey(ci => ci.BookId)
                      .IsRequired()
                      .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<Supplier>(entity =>
            {
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.HasIndex(e => e.Name);
                entity.Property(e => e.Email).HasMaxLength(256);
                entity.HasIndex(e => e.Email).IsUnique().HasFilter("[Email] IS NOT NULL");
                entity.Property(e => e.Phone).HasMaxLength(50);
                entity.Property(e => e.Address).HasMaxLength(500);
            });

            builder.Entity<StockReceipt>(entity =>
            {
                entity.HasOne(sr => sr.Supplier)
                      .WithMany()
                      .HasForeignKey(sr => sr.SupplierId)
                      .IsRequired(false)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            builder.Entity<StockReceiptDetail>(entity =>
            {
                entity.Property(e => e.PurchasePrice).HasColumnType("decimal(18,2)");
                entity.ToTable(t => t.HasCheckConstraint("CK_StockReceiptDetails_QuantityReceived", "[QuantityReceived] > 0"));

                entity.HasOne(srd => srd.StockReceipt)
                      .WithMany(sr => sr.StockReceiptDetails)
                      .HasForeignKey(srd => srd.StockReceiptId)
                      .IsRequired()
                      .OnDelete(DeleteBehavior.Cascade); // Xóa chi tiết nếu phiếu nhập bị xóa

                entity.HasOne(srd => srd.Book)
                      .WithMany()
                      .HasForeignKey(srd => srd.BookId)
                      .IsRequired()
                      .OnDelete(DeleteBehavior.Restrict); // Không cho xóa sách nếu đã có lịch sử nhập
            });

            builder.Entity<OrderShippingAddress>(entity =>
            {
                entity.HasKey(e => e.Id); // Đảm bảo có khóa chính
                entity.Property(e => e.Street).IsRequired().HasMaxLength(256);
                entity.Property(e => e.Village).HasMaxLength(100);
                entity.Property(e => e.District).IsRequired().HasMaxLength(100);
                entity.Property(e => e.City).IsRequired().HasMaxLength(100);
                entity.Property(e => e.RecipientName).HasMaxLength(200);
                entity.Property(e => e.PhoneNumber).HasMaxLength(20);
            });

            // ----- Cấu hình Promotion -----
            builder.Entity<Promotion>(entity =>
            {
                // Khóa chính Id đã được kế thừa từ BaseEntity và tự cấu hình

                entity.Property(e => e.Code)
                      .IsRequired()
                      .HasMaxLength(50);

                entity.HasIndex(e => e.Code)
                      .IsUnique();

                entity.Property(e => e.Description).HasMaxLength(256);

                entity.Property(e => e.DiscountPercentage)
                      .HasColumnType("decimal(5, 2)");

                entity.Property(e => e.DiscountAmount)
                      .HasColumnType("decimal(18, 2)");

                entity.Property(e => e.StartDate).IsRequired();

                entity.Property(e => e.CurrentUsage).IsRequired().HasDefaultValue(0);

                entity.Property(e => e.IsActive).IsRequired().HasDefaultValue(true);
                entity.ToTable(t => t.HasCheckConstraint("CK_Promotions_DiscountType",
                    "([DiscountPercentage] IS NOT NULL AND [DiscountAmount] IS NULL) OR ([DiscountPercentage] IS NULL AND [DiscountAmount] IS NOT NULL) OR ([DiscountPercentage] IS NULL AND [DiscountAmount] IS NULL)"));
                entity.ToTable(t => t.HasCheckConstraint("CK_Promotions_Percentage",
                    "[DiscountPercentage] IS NULL OR ([DiscountPercentage] > 0 AND [DiscountPercentage] <= 100)"));
                entity.ToTable(t => t.HasCheckConstraint("CK_Promotions_Amount",
                    "[DiscountAmount] IS NULL OR [DiscountAmount] > 0"));
                entity.ToTable(t => t.HasCheckConstraint("CK_Promotions_Usage",
                    "[CurrentUsage] >= 0 AND ([MaxUsage] IS NULL OR [CurrentUsage] <= [MaxUsage])"));
                entity.ToTable(t => t.HasCheckConstraint("CK_Promotions_Dates",
                    "[EndDate] IS NULL OR [EndDate] >= [StartDate]"));
            });

            // ----- Cấu hình Review -----
            builder.Entity<Review>(entity =>
            {
                // Ràng buộc UNIQUE để mỗi User chỉ đánh giá 1 Book 1 lần
                entity.HasIndex(r => new { r.UserId, r.BookId }).IsUnique();

                entity.Property(r => r.Comment).HasMaxLength(1000);

                entity.ToTable(t => t.HasCheckConstraint("CK_Reviews_Rating", "[Rating] >= 1 AND [Rating] <= 5"));
                // Mối quan hệ với Book
                entity.HasOne(r => r.Book)
                      .WithMany()
                      .HasForeignKey(r => r.BookId)
                      .IsRequired()
                      .OnDelete(DeleteBehavior.Cascade);

                // Mối quan hệ với User
                entity.HasOne(r => r.User)
                      .WithMany()
                      .HasForeignKey(r => r.UserId)
                      .IsRequired()
                      .OnDelete(DeleteBehavior.Cascade); // Xóa Review nếu User bị xóa
            });

            builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            SeedData(builder);
        }


        // Ghi đè SaveChangesAsync để cập nhật UpdatedAtUtc tự động
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var entries = ChangeTracker
                .Entries()
                .Where(e => e.Entity is BaseEntity && (
                        e.State == EntityState.Added
                        || e.State == EntityState.Modified));

            foreach (var entityEntry in entries)
            {
                ((BaseEntity)entityEntry.Entity).UpdatedAtUtc = DateTime.UtcNow;

                if (entityEntry.State == EntityState.Added)
                {
                    ((BaseEntity)entityEntry.Entity).CreatedAtUtc = DateTime.UtcNow;
                }
            }

            return await base.SaveChangesAsync(cancellationToken);
        }

        private static void SeedData(ModelBuilder builder)
        {
            // --- Định nghĩa các Guid tĩnh cho Seed Data ---
            Guid adminRoleId = new Guid("E1F3E5D4-1111-4F6F-9C5C-9B8D3A5B2A01");
            Guid userRoleId = new Guid("A2E4F6A8-2222-4D8E-8A4B-8A7C2B4E1F02");
            Guid adminUserId = new Guid("F54527EB-F806-40DB-BF76-C7B0E5FA6D39");

            // --- Định nghĩa một DateTime tĩnh ---
            var seedDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            // Seed Roles 
            builder.Entity<Role>().HasData(
                new Role { Id = adminRoleId, Name = "Admin", CreatedAtUtc = seedDate, UpdatedAtUtc = seedDate },
                new Role { Id = userRoleId, Name = "User", CreatedAtUtc = seedDate, UpdatedAtUtc = seedDate }
            );

            // --- Seed Admin User ---
            builder.Entity<User>().HasData(
                new User
                {
                    Id = adminUserId,
                    UserName = "admin",
                    Email = "admin@bookstore.com",
                    PasswordHash = "$2a$12$PCb6JuQsMqxNkxzSLh1EaOaQBbtDy0wwOdu5xkSu7nbJ31KB8yRAe",
                    FirstName = "Admin",
                    LastName = "User",
                    IsActive = true,
                    CreatedAtUtc = seedDate,
                    UpdatedAtUtc = seedDate
                }
            );

            // --- Seed UserRole cho Admin User ---
            builder.Entity<UserRole>().HasData(
                new UserRole { UserId = adminUserId, RoleId = adminRoleId }
            );
        }
    }
}