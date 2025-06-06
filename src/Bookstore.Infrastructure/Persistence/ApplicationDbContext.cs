
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
            // --- 1. ĐỊNH NGHĨA CÁC GIÁ TRỊ CỐ ĐỊNH ---
            // GUIDs
            Guid adminRoleId = new Guid("E1F3E5D4-1111-4F6F-9C5C-9B8D3A5B2A01");
            Guid staffRoleId = new Guid("C3D5E7B9-3333-4A9A-9B5C-7B6D1A3E0F03");
            Guid userRoleId = new Guid("A2E4F6A8-2222-4D8E-8A4B-8A7C2B4E1F02");

            Guid adminId = new Guid("F54527EB-F806-40DB-BF76-C7B0E5FA6D39");
            Guid staffId = new Guid("D8A8E7C6-9F0A-4B1C-8A5B-9C7E6D5B4A02");
            Guid customerId1 = new Guid("C7B6A5E4-8E1D-4C9C-9F2E-8B1D0564F01B");
            Guid customerId2 = new Guid("A6E5D4C3-7D2C-4B8B-8A3F-7C3F5A7B9D2C");

            Guid catFictionId = new Guid("DA2A34A1-F94B-4610-818A-8B1D0564F01B");
            Guid catSciFiId = new Guid("B8D7B4E0-5A3E-4C9C-8E1D-9A1C3F5B7E0A");
            Guid catNonFictionId = new Guid("C9E8D5F1-6B4F-4D8D-9F2E-8B2E4F6A8C1B");
            Guid catTechId = new Guid("E0F9E6A2-7C5A-4E7E-8A3F-7C3F5A7B9D2C");

            Guid authorAsimovId = new Guid("B2C3D4E5-F6A1-4B2C-9D3E-8F1A2B3C4D5E");
            Guid authorHarariId = new Guid("C3D4E5F6-A1B2-4C3D-8E4F-7A2B3C4D5E6F");
            Guid authorHuntId = new Guid("E5F6A1B2-C3D4-4E5F-8A6B-5C4D5E6F7A8B");

            Guid supplierFahasaId = new Guid("F1A2B3C4-D5E6-4A1B-8C2D-9E0F1A2B3C4D");
            Guid supplierTikiId = new Guid("A2B3C4D5-E6F1-4B2C-9D3E-8F1A2B3C4D5E");

            Guid bookFoundationId = new Guid("B1A2C3D4-E5F6-4A1B-8C2D-9E0F1A2B3C4D");
            Guid bookSapiensId = new Guid("B2C3D4E5-F6A1-4B2C-9D3E-8F1A2B3C4D5E");
            Guid bookPragmaticId = new Guid("C3D4E5F6-A1B2-4C3D-8E4F-7A2B3C4D5E6F");
            Guid bookOutOfStockId = new Guid("D4E5F6A1-B2C3-4D4E-9F5A-6B3C4D5E6F7A");
            Guid bookLowStockId = new Guid("E5F6A1B2-C3D4-4E5F-8A6B-5C4D5E6F7A8B");

            // Password Hash cho "Password123!"
            string passwordHash = "$2a$11$hBqPh187JFDfmgUxrk4ZJeq7IyuBoLtOPugz.Di9Mc6weUeSDLYcy"; // Hash bạn đã cung cấp

            // Ngày cố định
            var seedDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            // --- 2. SEED ROLES ---
            builder.Entity<Role>().HasData(
                new Role { Id = adminRoleId, Name = "Admin", CreatedAtUtc = seedDate, UpdatedAtUtc = seedDate },
                new Role { Id = staffRoleId, Name = "Staff", CreatedAtUtc = seedDate, UpdatedAtUtc = seedDate },
                new Role { Id = userRoleId, Name = "User", CreatedAtUtc = seedDate, UpdatedAtUtc = seedDate }
            );

            // --- 3. SEED USERS ---
            builder.Entity<User>().HasData(
                new User { Id = adminId, UserName = "adminuser", Email = "admin@example.com", PasswordHash = passwordHash, FirstName = "Admin", LastName = "User", PhoneNumber = "0123456780", IsActive = true, CreatedAtUtc = seedDate, UpdatedAtUtc = seedDate },
                new User { Id = staffId, UserName = "staffuser", Email = "staff@example.com", PasswordHash = passwordHash, FirstName = "Staff", LastName = "Member", PhoneNumber = "0123456781", IsActive = true, CreatedAtUtc = seedDate, UpdatedAtUtc = seedDate },
                new User { Id = customerId1, UserName = "customer1", Email = "customer1@example.com", PasswordHash = passwordHash, FirstName = "An", LastName = "Nguyễn", PhoneNumber = "0901234567", IsActive = true, CreatedAtUtc = seedDate, UpdatedAtUtc = seedDate },
                new User { Id = customerId2, UserName = "customer2", Email = "customer2@example.com", PasswordHash = passwordHash, FirstName = "Bình", LastName = "Trần", PhoneNumber = "0907654321", IsActive = true, CreatedAtUtc = seedDate, UpdatedAtUtc = seedDate }
            );

            // --- 4. SEED USERROLES ---
            builder.Entity<UserRole>().HasData(
                new UserRole { UserId = adminId, RoleId = adminRoleId },
                new UserRole { UserId = staffId, RoleId = staffRoleId },
                new UserRole { UserId = customerId1, RoleId = userRoleId },
                new UserRole { UserId = customerId2, RoleId = userRoleId }
            );

            // --- 5. SEED CATEGORIES, AUTHORS, SUPPLIERS ---
            builder.Entity<Category>().HasData(
                new { Id = catFictionId, Name = "Tiểu thuyết", Description = "Các tác phẩm hư cấu.", ParentCategoryId = (Guid?)null, IsDeleted = false, CreatedAtUtc = seedDate, UpdatedAtUtc = seedDate },
                new { Id = catSciFiId, Name = "Khoa học Viễn tưởng", Description = "Tiểu thuyết dựa trên các tiến bộ khoa học công nghệ.", ParentCategoryId = catFictionId, IsDeleted = false, CreatedAtUtc = seedDate, UpdatedAtUtc = seedDate },
                new { Id = catNonFictionId, Name = "Phi hư cấu", Description = "Dựa trên sự thật và sự kiện có thật.", ParentCategoryId = (Guid?)null, IsDeleted = false, CreatedAtUtc = seedDate, UpdatedAtUtc = seedDate },
                new { Id = catTechId, Name = "Công nghệ", Description = "Sách về lập trình và phần mềm.", ParentCategoryId = catNonFictionId, IsDeleted = false, CreatedAtUtc = seedDate, UpdatedAtUtc = seedDate }
            );
            builder.Entity<Author>().HasData(
                new { Id = authorAsimovId, Name = "Isaac Asimov", Biography = "Nhà văn và giáo sư hóa sinh người Mỹ.", CreatedAtUtc = seedDate, UpdatedAtUtc = seedDate },
                new { Id = authorHarariId, Name = "Yuval Noah Harari", Biography = "Nhà sử học và giáo sư người Israel.", CreatedAtUtc = seedDate, UpdatedAtUtc = seedDate },
                new { Id = authorHuntId, Name = "Andrew Hunt", Biography = "Đồng tác giả cuốn The Pragmatic Programmer.", CreatedAtUtc = seedDate, UpdatedAtUtc = seedDate }
            );
            builder.Entity<Supplier>().HasData(
                new { Id = supplierFahasaId, Name = "FAHASA", ContactPerson = "Mr. An", Email = "contact@fahasa.com.vn", Phone = "1900636467", Address = "TP.HCM", CreatedAtUtc = seedDate, UpdatedAtUtc = seedDate },
                new { Id = supplierTikiId, Name = "Tiki Trading", ContactPerson = "Ms. Bình", Email = "trading@tiki.vn", Phone = "19006035", Address = "TP.HCM", CreatedAtUtc = seedDate, UpdatedAtUtc = seedDate }
            );

            // --- 6. SEED BOOKS ---
            builder.Entity<Book>().HasData(
                new { Id = bookFoundationId, Title = "Foundation", Description = "Cuốn tiểu thuyết đầu tiên trong series Foundation.", ISBN = "978-0553293357", AuthorId = authorAsimovId, Publisher = "Spectra", PublicationYear = 1951, CoverImageUrl = "https://images-na.ssl-images-amazon.com/images/S/compressed.photo.goodreads.com/books/1417942793l/29579.jpg", Price = 150000.00m, StockQuantity = 50, CategoryId = catSciFiId, IsDeleted = false, CreatedAtUtc = seedDate, UpdatedAtUtc = seedDate },
                new { Id = bookSapiensId, Title = "Sapiens: Lược sử loài người", Description = "Khám phá lịch sử loài người từ thời Đồ đá.", ISBN = "978-0062316097", AuthorId = authorHarariId, Publisher = "NXB Tri Thức", PublicationYear = 2015, CoverImageUrl = "https://images-na.ssl-images-amazon.com/images/S/compressed.photo.goodreads.com/books/1420585954l/23692271.jpg", Price = 250000.00m, StockQuantity = 100, CategoryId = catNonFictionId, IsDeleted = false, CreatedAtUtc = seedDate, UpdatedAtUtc = seedDate },
                new { Id = bookPragmaticId, Title = "The Pragmatic Programmer", Description = "Hành trình trở thành lập trình viên bậc thầy.", ISBN = "978-0135957059", AuthorId = authorHuntId, Publisher = "Addison-Wesley", PublicationYear = 2019, CoverImageUrl = "https://images-na.ssl-images-amazon.com/images/S/compressed.photo.goodreads.com/books/1584551142l/4099.jpg", Price = 350000.00m, StockQuantity = 75, CategoryId = catTechId, IsDeleted = false, CreatedAtUtc = seedDate, UpdatedAtUtc = seedDate },
                new { Id = bookOutOfStockId, Title = "I, Robot", Description = "Tuyển tập truyện ngắn khoa học viễn tưởng.", ISBN = "978-0553382563", AuthorId = authorAsimovId, Publisher = "Spectra", PublicationYear = 1950, CoverImageUrl = "https://images-na.ssl-images-amazon.com/images/S/compressed.photo.goodreads.com/books/1550997314l/41804.jpg", Price = 120000.00m, StockQuantity = 0, CategoryId = catSciFiId, IsDeleted = false, CreatedAtUtc = seedDate, UpdatedAtUtc = seedDate },
                new { Id = bookLowStockId, Title = "Homo Deus: Lược sử Tương lai", Description = "Khám phá tương lai của loài người.", ISBN = "978-0062464316", AuthorId = authorHarariId, Publisher = "NXB Tri Thức", PublicationYear = 2017, CoverImageUrl = "https://images-na.ssl-images-amazon.com/images/S/compressed.photo.goodreads.com/books/1473453322l/31138556.jpg", Price = 260000.00m, StockQuantity = 3, CategoryId = catNonFictionId, IsDeleted = false, CreatedAtUtc = seedDate, UpdatedAtUtc = seedDate }
            );
        }
    }
}