using Bookstore.Application.Interfaces;
using Bookstore.Application.Interfaces.Services;
using Bookstore.Application.Services;
using Bookstore.Application.Settings; // Namespace chứa JwtSettings
using Bookstore.Application.Validators.Categories;
using Bookstore.Domain.Interfaces.Repositories; // Namespace chứa Interfaces Repository
using Bookstore.Domain.Interfaces.Services;
using Bookstore.Infrastructure.Persistence; // Namespace chứa DbContext
using Bookstore.Infrastructure.Repositories; // Namespace chứa Implementations Repository
using Bookstore.Infrastructure.Services;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.ApiExplorer; // Cần cho Swagger + Versioning
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Reflection; // Cần cho Assembly
using System.Text;

var builder = WebApplication.CreateBuilder(args);


// Đọc cấu hình JWT từ User Secrets hoặc appsettings
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
// Lấy instance của JwtSettings để sử dụng trong cấu hình Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>()
                  ?? throw new InvalidOperationException("JwtSettings is not configured correctly in user secrets or appsettings.");



// Add services to the container.

// ----- Configure DbContext -----
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlServerOptionsAction: sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null);
        }));


// ----- Register UnitOfWork -----
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>(); 
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IBookService, BookService>();

// ----- Configure Authentication -----
// Cấu hình hệ thống xác thực sử dụng JWT Bearer tokens
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,

        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key))
    };
});

// ----- Configure Authorization -----
builder.Services.AddAuthorization();


// ----- Configure API Versioning -----
builder.Services.AddApiVersioning(options =>
{
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.DefaultApiVersion = new Microsoft.AspNetCore.Mvc.ApiVersion(1, 0);
    options.ReportApiVersions = true;
});
builder.Services.AddVersionedApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});


// ----- Configure CORS -----
var swaggerUiOrigin = builder.Configuration["SwaggerUIOrigin"] ?? "https://localhost:5244"; // Lấy URL Swagger từ config hoặc mặc định
var developmentOrigins = new string[] { swaggerUiOrigin }; // Có thể thêm các origin khác nếu cần

if (builder.Environment.IsDevelopment())
{
    // Cho phép origin của Swagger UI trong môi trường Development
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowSwagger", policy =>
        {
            // Lấy các URL từ launchSettings.json để đảm bảo CORS hoạt động với cả HTTP và HTTPS
            var launchSettingsPath = Path.Combine(Directory.GetCurrentDirectory(), "Properties", "launchSettings.json");
            if (File.Exists(launchSettingsPath))
            {
                try
                {
                    var launchSettingsJson = File.ReadAllText(launchSettingsPath);
                    var launchSettings = System.Text.Json.JsonDocument.Parse(launchSettingsJson);
                    var profiles = launchSettings.RootElement.GetProperty("profiles");
                    var urls = profiles.EnumerateObject()
                                       .SelectMany(p => p.Value.TryGetProperty("applicationUrl", out var appUrl) ? appUrl.GetString()?.Split(';') ?? Array.Empty<string>() : Array.Empty<string>())
                                       .Where(url => !string.IsNullOrWhiteSpace(url))
                                       .Select(url => url.TrimEnd('/')) // Loại bỏ dấu / cuối URL nếu có
                                       .Distinct()
                                       .ToArray();
                    if (urls.Any())
                    {
                        developmentOrigins = urls;
                    }
                }
                catch
                {
                    // Bỏ qua nếu không đọc được launchSettings
                }
            }

            policy.WithOrigins(developmentOrigins) // Cho phép các origin này
                  .AllowAnyHeader() // Cho phép mọi header
                  .AllowAnyMethod(); // Cho phép mọi phương thức HTTP
        });
    });
}
else
{

}

// ----- Configure Controllers -----
builder.Services.AddControllers();

// ----- Configure FluentValidation -----
// Tự động tìm và đăng ký tất cả validators trong Assembly chứa CreateCategoryDtoValidator
builder.Services.AddValidatorsFromAssemblyContaining<CreateCategoryDtoValidator>();

// ----- Configure Swagger/OpenAPI -----
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // Cấu hình thông tin chung cho tài liệu API v1
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1.0",
        Title = "Bookstore Management API",
        Description = "API for managing the Bookstore application",
    });
});


// Build aplication host
var app = builder.Build();


// Configure the HTTP request pipeline (Middleware). 
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        // Lấy danh sách các phiên bản API để tạo endpoint trong Swagger UI
        var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
        foreach (var description in provider.ApiVersionDescriptions.Reverse())
        {
            options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json",
                description.GroupName.ToUpperInvariant());
        }
    });
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

// ----- Áp dụng chính sách CORS -----
if (app.Environment.IsDevelopment())
{
    app.UseCors("AllowSwagger");
}
else
{
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Chạy ứng dụng
app.Run();