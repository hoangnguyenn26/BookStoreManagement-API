
using Bookstore.Domain.Entities;
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
                entity.Property(e => e.RowVersion).IsRowVersion();
                entity.Property(e => e.Status).HasConversion<byte>(); 

                entity.HasOne(o => o.User)
                      .WithMany(u => u.Orders)
                      .HasForeignKey(o => o.UserId)
                      .IsRequired()
                      .OnDelete(DeleteBehavior.Restrict); // Ngăn xóa User nếu họ có Orders (hành vi phổ biến)
            });

            // ----- Cấu hình OrderDetail (Many-to-One với Order, Many-to-One với Book) -----
            builder.Entity<OrderDetail>(entity =>
            {
                // entity.HasKey(od => new { od.OrderId, od.BookId }); // Nếu dùng khóa chính kết hợp
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
            });
            builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            SeedData(builder); 
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
        }
    }
}