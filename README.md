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
- [Bắt đầu](#bắt-đầu)
  - [Điều kiện Tiên quyết](#điều-kiện-tiên-quyết)
  - [Cài đặt & Thiết lập](#cài-đặt--thiết-lập)
  - [Chạy API](#chạy-api)
  - [Tài liệu API (Swagger)](#tài-liệu-api-swagger)
  - [Cấu hình](#cấu-hình)
- [Kiểm thử (Testing)](#kiểm-thử-testing)

## **Tổng quan**

**BookStoreManagement-API** cung cấp một giải pháp backend mạnh mẽ được thiết kế để đơn giản hóa và tối ưu hóa hoạt động của nhà sách. API này được xây dựng bằng ASP.NET Core, tuân theo các nguyên tắc thiết kế hiện đại, cho phép các nhà phát triển (bao gồm cả ứng dụng .NET MAUI mà chúng ta đang xây dựng) tạo ra các hệ thống quản lý hiệu quả, dễ mở rộng và bảo trì.

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

## **Bắt đầu**

### **Điều kiện Tiên quyết**

Trước khi bắt đầu, hãy đảm bảo bạn đã cài đặt:

-   **.NET SDK 8.0** (Hoặc phiên bản tương ứng với dự án) - [Tải tại đây](https://dotnet.microsoft.com/download)
-   **Microsoft SQL Server:** Phiên bản nào cũng được (SQL Server Express miễn phí là đủ) - [Tải SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads)
-   **Công cụ quản lý CSDL:**
    -   SQL Server Management Studio (SSMS) - [Tải SSMS](https://learn.microsoft.com/en-us/sql/ssms/download-sql-server-management-studio-ssms)
    -   Hoặc Azure Data Studio - [Tải Azure Data Studio](https://learn.microsoft.com/en-us/sql/azure-data-studio/download-azure-data-studio)
-   **IDE / Code Editor:**
    -   Visual Studio 2022 (Recommended) - [Tải Visual Studio](https://visualstudio.microsoft.com/vs/)
    -   Hoặc Visual Studio Code - [Tải VS Code](https://code.visualstudio.com/)
-   **Git:** - [Tải Git](https://git-scm.com/downloads)
-   **(Tùy chọn) Postman hoặc công cụ tương tự:** Để kiểm thử API endpoints.

### **Cài đặt & Thiết lập**

1.  **Clone Repository:**
    ```bash
    git clone https://github.com/hoangnguyenn26/BookStoreManagement-API.git
    cd BookStoreManagement-API
    ```

2.  **Cấu hình Secrets:**
    API này cần các thông tin cấu hình nhạy cảm như Chuỗi kết nối CSDL và Khóa bí mật JWT. **Không** lưu trữ các giá trị này trực tiếp trong `appsettings.json` khi commit lên Git. Sử dụng **User Secrets** cho môi trường Development:
    *   Mở Terminal/Command Prompt tại thư mục `src/Bookstore.Api`.
    *   Chạy lệnh `dotnet user-secrets init` (nếu chưa khởi tạo).
    *   Thiết lập Connection String (thay thế bằng thông tin của bạn):
        ```bash
        dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=YOUR_SERVER;Database=BookstoreDb;User ID=YOUR_USER;Password=YOUR_PASSWORD;TrustServerCertificate=True;MultipleActiveResultSets=true"
        # Hoặc dùng Windows Auth:
        # dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=YOUR_SERVER;Database=BookstoreDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
        ```
    *   Thiết lập JWT Settings (thay thế Key bằng chuỗi bí mật mạnh, dài của bạn):
        ```bash
        dotnet user-secrets set "JwtSettings:Key" "Your_Super_Secret_And_Long_Key_Goes_Here_Replace_This_Immediately"
        dotnet user-secrets set "JwtSettings:Issuer" "BookstoreManagementApi"
        dotnet user-secrets set "JwtSettings:Audience" "BookstoreManagementApiClient"
        dotnet user-secrets set "JwtSettings:DurationInMinutes" "60"
        ```
    *   *(Xem thêm phần [Cấu hình](#cấu-hình) bên dưới)*

3.  **Restore Dependencies:**
    Tại thư mục gốc của solution (chứa file `.sln`), chạy:
    ```bash
    dotnet restore
    ```

4.  **Áp dụng Database Migrations:**
    Đảm bảo chuỗi kết nối trong User Secrets đã đúng và SQL Server đang chạy. Sau đó chạy lệnh sau từ thư mục gốc của solution:
    ```bash
    dotnet ef database update --project src/Bookstore.Infrastructure --startup-project src/Bookstore.Api
    ```
    Lệnh này sẽ tạo CSDL `BookstoreDb` (nếu chưa có) và tất cả các bảng cần thiết dựa trên EF Core Migrations.

### **Chạy API**

1.  **Cách 1 (Dùng .NET CLI):**
    *   Mở Terminal/Command Prompt tại thư mục `src/Bookstore.Api`.
    *   Chạy lệnh:
        ```bash
        dotnet run
        ```
2.  **Cách 2 (Dùng Visual Studio):**
    *   Mở file `BookstoreManagement.sln` bằng Visual Studio 2022.
    *   Chọn project `Bookstore.Api` làm Startup Project.
    *   Nhấn `F5` hoặc nút Start Debugging.

API sẽ khởi chạy và lắng nghe trên các cổng được cấu hình trong `launchSettings.json` (ví dụ: `https://localhost:7264` và `http://localhost:5244`).

### **Tài liệu API (Swagger)**

Sau khi API khởi chạy, bạn có thể truy cập giao diện Swagger UI tương tác để xem tất cả các endpoints, mô tả, và thử nghiệm trực tiếp:

*   Mở trình duyệt và truy cập URL gốc của API (thường là địa chỉ HTTPS), ví dụ: **`https://localhost:7264`** (port có thể khác trên máy bạn).
*   Giao diện Swagger UI sẽ hiển thị. Bạn có thể chọn phiên bản API (v1), xem các Controllers, Endpoints, Schemas (DTOs) và thực hiện các request thử nghiệm. Để test các endpoint yêu cầu xác thực, hãy dùng endpoint `/api/auth/login` để lấy token, sau đó nhấn nút "Authorize" trên Swagger và nhập token theo định dạng `Bearer your_token`.

### **Cấu hình**

*   **Chuỗi Kết nối (Connection String):**
    *   **Development:** Nên được đặt trong **User Secrets** (xem phần Cài đặt).
    *   **Production:** Sử dụng biến môi trường (Environment Variables), Azure Key Vault, hoặc các cơ chế quản lý cấu hình an toàn khác. Cấu hình đọc từ `appsettings.Production.json` (nếu có) hoặc biến môi trường sẽ ghi đè `appsettings.json`.
*   **Cài đặt JWT (JwtSettings):**
    *   **Development:** `Key`, `Issuer`, `Audience`, `DurationInMinutes` nên được đặt trong **User Secrets**. Đặc biệt `Key` phải là một chuỗi dài, phức tạp và bí mật.
    *   **Production:** Tương tự Connection String, sử dụng biến môi trường hoặc Key Vault cho `Key`. `Issuer`, `Audience`, `DurationInMinutes` có thể đặt trong `appsettings.Production.json` hoặc biến môi trường.

## **Kiểm thử (Testing)**

Dự án sử dụng xUnit làm framework kiểm thử. Bạn có thể chạy các Unit Test và Integration Test bằng các cách sau:

*   **Visual Studio Test Explorer:** Mở Test Explorer (Test -> Test Explorer) và chạy các test từ đó.
*   **.NET CLI:** Tại thư mục gốc của solution, chạy lệnh:
    ```bash
    dotnet test
    ```
