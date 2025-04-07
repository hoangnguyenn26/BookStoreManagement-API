
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
        // Thêm các DbSet khác khi bạn tạo Entities mới (ví dụ: Orders, Addresses...)

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder); // Gọi base trước

            // Cấu hình cho User
            builder.Entity<User>(entity =>
            {
                entity.HasIndex(e => e.UserName).IsUnique(); // Đảm bảo UserName là duy nhất
                entity.HasIndex(e => e.Email).IsUnique();    // Đảm bảo Email là duy nhất
                entity.Property(e => e.FirstName).HasMaxLength(100);
                entity.Property(e => e.LastName).HasMaxLength(100);
            });

            // Cấu hình cho Book
            builder.Entity<Book>(entity =>
            {
                entity.Property(e => e.Price).HasColumnType("decimal(18,2)"); // Kiểu dữ liệu chính xác cho giá
                entity.Property(e => e.RowVersion).IsRowVersion(); // Cấu hình RowVersion cho concurrency
                                                                   // Cấu hình mối quan hệ với Category (Ví dụ)
                                                                   // entity.HasOne(d => d.Category)
                                                                   //       .WithMany(p => p.Books)
                                                                   //       .HasForeignKey(d => d.CategoryId)
                                                                   //       .OnDelete(DeleteBehavior.Restrict); // Hoặc .SetNull, .Cascade tùy logic

                // Cấu hình mối quan hệ với Author (Ví dụ)
                // entity.HasOne(d => d.Author)
                //      .WithMany(p => p.Books)
                //      .HasForeignKey(d => d.AuthorId)
                //      .OnDelete(DeleteBehavior.SetNull); // Nếu Author bị xóa, set AuthorId trong Book thành NULL
            });

            base.OnModelCreating(builder);
            builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly()); // Nên giữ dòng này
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