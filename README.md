<div align="center">

# **BookStoreManagement-API**

API Backend Mạnh mẽ cho Hệ thống Quản lý Nhà sách
*(Empowering Bookstores, Simplifying Management Efforts)*

[![phiên bản .NET](https://img.shields.io/badge/.NET-8.0-blueviolet)](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
[![last commit](https://img.shields.io/github/last-commit/hoangnguyenn26/BookStoreManagement-API)](https://github.com/hoangnguyenn26/BookStoreManagement-API/commits/main)
[![languages](https://img.shields.io/github/languages/count/hoangnguyenn26/BookStoreManagement-API)](https://github.com/hoangnguyenn26/BookStoreManagement-API)
[![license](https://img.shields.io/github/license/hoangnguyenn26/BookStoreManagement-API)](https://github.com/hoangnguyenn26/BookStoreManagement-API/blob/main/LICENSE) <!-- Thêm License nếu có -->

</div>

## **Mục lục**

- [Tổng quan](#tổng-quan)
- [Kiến trúc](#kiến-trúc)
- [Tính năng Chính](#tính-năng-chính)
- [Công nghệ Sử dụng](#công-nghệ-sử-dụng)
- [Kiểm thử (Testing)](#kiểm-thử-testing)

## **Tổng quan**

**BookStoreManagement-API** cung cấp một giải pháp backend mạnh mẽ được thiết kế để đơn giản hóa và tối ưu hóa hoạt động của nhà sách. API này được xây dựng bằng ASP.NET Core, tuân theo các nguyên tắc thiết kế hiện đại, cho phép các nhà phát triển tạo ra các hệ thống quản lý hiệu quả, dễ mở rộng và bảo trì.

## **Kiến trúc**

Dự án áp dụng **Layered Architecture (Kiến trúc Phân lớp)** để đảm bảo sự phân tách rõ ràng các mối quan tâm (Separation of Concerns):

-   **Domain:** Chứa các Entities, Enums, Interfaces (Repositories, Domain Services), logic nghiệp vụ cốt lõi.
-   **Application:** Chứa logic ứng dụng (Use Cases), DTOs, Interfaces (Application Services, Infrastructure Services), Validators, Mappers.
-   **Infrastructure:** Chứa các triển khai cụ thể liên quan đến kỹ thuật bên ngoài: Truy cập CSDL (EF Core DbContext, Repositories, UnitOfWork), triển khai Services bên ngoài (Email, Token...).
-   **Api:** Lớp trình bày (Presentation Layer), chứa Controllers, Middleware, cấu hình DI, Authentication, Authorization, Swagger...

## **Tính năng Chính**

-   📦 **Kiến trúc Module Phân lớp:** Tăng cường khả năng bảo trì, mở rộng và kiểm thử.
-   👤 **Quản lý Người dùng Toàn diện:** Xác thực (JWT), Phân quyền theo Vai trò (User, Admin, Staff), Quản lý Hồ sơ và Địa chỉ.
-   📚 **Quản lý Danh mục Sản phẩm:** CRUD cho Sách, Danh mục (hỗ trợ phân cấp), Tác giả.
-   🛒 **Xử lý Giỏ hàng & Đơn hàng:** Quản lý giỏ hàng phía server, quy trình Checkout, tạo đơn hàng online và tại cửa hàng (In-Store), quản lý trạng thái đơn hàng, snapshot địa chỉ/giá.
*   **Quản lý Nhà Cung cấp & Nhập kho:** Theo dõi Nhà cung cấp, ghi nhận Phiếu nhập kho và cập nhật tồn kho tự động.
*   📊 **Quản lý Tồn kho & Nhật ký:** Theo dõi `StockQuantity`, ghi `InventoryLogs` chi tiết cho các thay đổi.
*   🏷️ **Quản lý Khuyến mãi:** Tạo và áp dụng mã khuyến mãi, theo dõi lượt sử dụng.
*   ⭐ **Quản lý Đánh giá:** Cho phép User đánh giá sách và Admin kiểm duyệt.
*   📈 **Báo cáo & Dashboard:** Cung cấp thông tin tổng quan (Admin/User Dashboard), báo cáo Doanh thu, Sách bán chạy, Tồn kho thấp.
*   🔬 **Kiểm thử (Testing):** Hỗ trợ Unit Test (xUnit, Moq) và Integration Test (`Microsoft.AspNetCore.Mvc.Testing`).
*   ⚠️ **Middleware Xử lý lỗi:** Cơ chế bắt lỗi tập trung, trả về lỗi chuẩn hóa và logging chi tiết.
*   📄 **Tài liệu API Tự động (Swagger):** Tích hợp Swagger (OpenAPI) để dễ dàng khám phá và kiểm thử endpoints.
*   🔐 **Bảo mật:** JWT Authentication, Password Hashing (BCrypt), Authorization theo Role.
*   🚀 **API Versioning:** Hỗ trợ quản lý các phiên bản API.

## **Công nghệ Sử dụng**

-   **Framework:** ASP.NET Core 8.0 (Hoặc phiên bản cụ thể bạn dùng)
-   **Ngôn ngữ:** C# 12 (Hoặc phiên bản cụ thể bạn dùng)
-   **Database:** Microsoft SQL Server (Có thể dùng bản Express)
-   **ORM:** Entity Framework Core 8.0 (Code-First)
-   **API Documentation:** Swashbuckle.AspNetCore (Swagger UI)
-   **Authentication:** JWT Bearer Tokens (`Microsoft.AspNetCore.Authentication.JwtBearer`)
-   **Mapping:** AutoMapper (`AutoMapper.Extensions.Microsoft.DependencyInjection`)
-   **Validation:** FluentValidation (`FluentValidation.AspNetCore`)
-   **Password Hashing:** BCrypt.Net (`BCrypt.Net-Next`)
-   **Logging:** Serilog (với Sinks Console, File...)
-   **Testing:** xUnit, Moq, `Microsoft.AspNetCore.Mvc.Testing`
-   **API Versioning:** `Microsoft.AspNetCore.Mvc.Versioning`

## **Kiểm thử (Testing)**

Dự án sử dụng xUnit làm framework kiểm thử. Bạn có thể chạy các Unit Test và Integration Test bằng các cách sau:

*   **Visual Studio Test Explorer:** Mở Test Explorer (Test -> Test Explorer) và chạy các test từ đó.
*   **.NET CLI:** Tại thư mục gốc của solution, chạy lệnh:
    ```bash
    dotnet test
    ```
- Kiểm tra đánh giá chất lượng API (by K6 - javascripts)
  ![image](https://github.com/user-attachments/assets/78a1fb47-1878-4ca2-a59f-ec6807960a9c)
