
using Bookstore.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection; // Cần cho Assembly

namespace Bookstore.Infrastructure.Persistence
{
    public class ApplicationDbContext : DbContext
    {
        private static readonly Guid AdminRoleId = new Guid("E1F3E5D4-1111-4F6F-9C5C-9B8D3A5B2A01"); // Thay bằng Guid bạn tự tạo
        private static readonly Guid UserRoleId = new Guid("A2E4F6A8-2222-4D8E-8A4B-8A7C2B4E1F02"); // Thay bằng Guid bạn tự tạo
        // Constructor cần thiết để DI hoạt động
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Khai báo các DbSet cho các Entities cốt lõi đã tạo ở Ngày 1
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Role> Roles { get; set; } = null!;
        public DbSet<Book> Books { get; set; } = null!;
        public DbSet<Category> Categories { get; set; } = null!;
        public DbSet<Author> Authors { get; set; } = null!;
        public DbSet<UserRole> UserRoles { get; set; } = null!;
        public DbSet<Address> Addresses { get; set; } = null!;
        public DbSet<Order> Orders { get; set; } = null!;
        public DbSet<OrderDetail> OrderDetails { get; set; } = null!;
        // Thêm các DbSet khác khi bạn tạo Entities mới (ví dụ: Orders, Addresses...)

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder); // Gọi base trước
            // ----- Cấu hình UserRole (Many-to-Many) -----
            builder.Entity<UserRole>(entity =>
            {
                // Khóa chính kết hợp
                entity.HasKey(ur => new { ur.UserId, ur.RoleId });

                // Mối quan hệ với User
                entity.HasOne(ur => ur.User)
                      .WithMany() // Nếu không cần nav prop trong User thì để trống WithMany()
                                  // .WithMany(u => u.UserRoles) // Nếu có nav prop ICollection<UserRole> UserRoles trong User
                      .HasForeignKey(ur => ur.UserId)
                      .IsRequired()
                      .OnDelete(DeleteBehavior.Cascade); // Xóa UserRole nếu User bị xóa

                // Mối quan hệ với Role
                entity.HasOne(ur => ur.Role)
                      .WithMany() // Tương tự, nếu không cần nav prop trong Role
                                  // .WithMany(r => r.UserRoles) // Nếu có nav prop ICollection<UserRole> UserRoles trong Role
                      .HasForeignKey(ur => ur.RoleId)
                      .IsRequired()
                      .OnDelete(DeleteBehavior.Cascade); // Xóa UserRole nếu Role bị xóa
            });

            // ----- Cấu hình Address (One-to-Many với User) -----
            builder.Entity<Address>(entity =>
            {
                entity.HasOne(a => a.User)
                      .WithMany(u => u.Addresses) // Giả sử có ICollection<Address> Addresses trong User
                      .HasForeignKey(a => a.UserId)
                      .IsRequired()
                      .OnDelete(DeleteBehavior.Cascade); // Xóa địa chỉ nếu User bị xóa
            });

            // ----- Cấu hình Order (One-to-Many với User, One-to-Many với OrderDetail) -----
            builder.Entity<Order>(entity =>
            {
                entity.Property(e => e.TotalAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.RowVersion).IsRowVersion();
                entity.Property(e => e.Status).HasConversion<byte>(); // Lưu Enum dưới dạng byte

                entity.HasOne(o => o.User)
                      .WithMany(u => u.Orders) // Giả sử có ICollection<Order> Orders trong User
                      .HasForeignKey(o => o.UserId)
                      .IsRequired()
                      .OnDelete(DeleteBehavior.Restrict); // Ngăn xóa User nếu họ có Orders (hành vi phổ biến)
                // Không cần cấu hình HasMany cho OrderDetails ở đây nếu đã cấu hình trong OrderDetail
            });

            // ----- Cấu hình OrderDetail (Many-to-One với Order, Many-to-One với Book) -----
            builder.Entity<OrderDetail>(entity =>
            {
                // entity.HasKey(od => new { od.OrderId, od.BookId }); // Nếu dùng khóa chính kết hợp
                entity.Property(e => e.UnitPrice).HasColumnType("decimal(18,2)");

                entity.HasOne(od => od.Order)
                      .WithMany(o => o.OrderDetails) // Cần ICollection<OrderDetail> OrderDetails trong Order
                      .HasForeignKey(od => od.OrderId)
                      .IsRequired()
                      .OnDelete(DeleteBehavior.Cascade); // Xóa OrderDetail nếu Order bị xóa

                entity.HasOne(od => od.Book)
                      .WithMany() // Không cần nav prop ngược lại từ Book về OrderDetail thường là không cần thiết
                                  // .WithMany(b => b.OrderDetails) // Nếu cần
                      .HasForeignKey(od => od.BookId)
                      .IsRequired()
                      .OnDelete(DeleteBehavior.Restrict); // Ngăn xóa Book nếu nó nằm trong OrderDetail (trừ khi dùng Soft Delete)
            });


            // ----- Cấu hình lại Book & Category nếu cần -----
            builder.Entity<Book>(entity =>
            {
                entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
                entity.Property(e => e.RowVersion).IsRowVersion();

                // Cấu hình Soft Delete Filter (Rất quan trọng!)
                entity.HasQueryFilter(b => !b.IsDeleted);

                entity.HasOne(d => d.Category)
                      .WithMany(p => p.Books) // Giả sử có ICollection<Book> Books trong Category
                      .HasForeignKey(d => d.CategoryId)
                      .IsRequired()
                      .OnDelete(DeleteBehavior.Restrict); // Ngăn xóa Category nếu có Book thuộc về nó (trừ khi dùng Soft Delete)

                entity.HasOne(d => d.Author)
                      .WithMany(p => p.Books) // Giả sử có ICollection<Book> Books trong Author
                      .HasForeignKey(d => d.AuthorId)
                      .OnDelete(DeleteBehavior.SetNull); // Nếu Author bị xóa, AuthorId trong Book thành NULL
            });

            builder.Entity<Category>(entity =>
            {
                // Cấu hình Soft Delete Filter
                entity.HasQueryFilter(c => !c.IsDeleted);

                // Cấu hình mối quan hệ tự tham chiếu cho ParentCategory (nếu có)
                entity.HasOne(c => c.ParentCategory)
                      .WithMany(c => c.SubCategories) // Giả sử có ICollection<Category> SubCategories trong Category
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
                // Không cần cấu hình các HasMany ở đây nếu đã cấu hình ở phía 'Many' (Address, Order...)
            });


            // ----- (Thêm cấu hình cho các entity khác nếu bạn đã tạo: CartItem, WishlistItem, Promotion, Review, InventoryLog) -----


            // Áp dụng cấu hình từ các lớp riêng (Cách làm tốt hơn sau này)
            builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            SeedData(builder); // Gọi SeedData (đảm bảo nó vẫn dùng giá trị tĩnh)
        }


        // (Tùy chọn) Ghi đè SaveChangesAsync để cập nhật UpdatedAtUtc tự động
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
                    // Đảm bảo Id được tạo nếu chưa có (Guid.NewGuid() đã làm điều này trong BaseEntity)
                    // if (((BaseEntity)entityEntry.Entity).Id == Guid.Empty)
                    // {
                    //     ((BaseEntity)entityEntry.Entity).Id = Guid.NewGuid();
                    // }
                }
            }

            return await base.SaveChangesAsync(cancellationToken);
        }

        private static void SeedData(ModelBuilder builder)
        {
            // --- Định nghĩa một DateTime tĩnh ---
            var seedDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc); // Hoặc một ngày cố định khác

            // Seed Roles sử dụng Guid và DateTime tĩnh
            builder.Entity<Role>().HasData(
                new Role { Id = AdminRoleId, Name = "Admin", CreatedAtUtc = seedDate, UpdatedAtUtc = seedDate },
                new Role { Id = UserRoleId, Name = "User", CreatedAtUtc = seedDate, UpdatedAtUtc = seedDate }
            );

            // Nếu bạn seed User Admin, cũng phải dùng Guid tĩnh và DateTime tĩnh
        }
    }
}