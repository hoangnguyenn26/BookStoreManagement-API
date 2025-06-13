using Bookstore.Api.Middleware;
using Bookstore.Application.Interfaces;
using Bookstore.Application.Interfaces.Services;
using Bookstore.Application.Services;
using Bookstore.Application.Settings;
using Bookstore.Domain.Interfaces.Services;
using Bookstore.Infrastructure.Persistence;
using Bookstore.Infrastructure.Services;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Reflection;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// 1. Cấu hình Serilog
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext());

// 2. Thêm các Services vào DI Container
ConfigureServices(builder.Services, builder.Configuration);

var app = builder.Build();

// 3. Cấu hình HTTP Request Pipeline (Middleware)
ConfigurePipeline(app, app.Environment, app.Services);

app.Run();


// ====================================================================
// ==================== CÁC PHƯƠNG THỨC CẤU HÌNH ======================
// ====================================================================

void ConfigureServices(IServiceCollection services, IConfiguration configuration)
{
    // Cấu hình Controllers
    services.AddControllers();

    // Cấu hình cho API Versioning và Swagger
    services.AddEndpointsApiExplorer();
    services.AddApiVersioning(options =>
    {
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.DefaultApiVersion = new ApiVersion(1, 0);
        options.ReportApiVersions = true;
    });
    services.AddVersionedApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
    });

    // Cấu hình Swagger
    services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Version = "v1.0",
            Title = "Bookstore Management API",
            Description = "API for managing the Bookstore application",
        });

        var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);
        if (File.Exists(xmlPath))
        {
            options.IncludeXmlComments(xmlPath);
        }

        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            In = ParameterLocation.Header,
            Description = "Please enter JWT with Bearer into field (e.g., Bearer {token})",
            Name = "Authorization",
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer"
        });

        options.AddSecurityRequirement(new OpenApiSecurityRequirement {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header,
            },
            new List<string>()
        }});
    });

    // Cấu hình DbContext
    services.AddDbContext<ApplicationDbContext>(options =>
    {
        options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
        if (builder.Environment.IsDevelopment())
        {
            options.EnableSensitiveDataLogging();
        }
    });

    // Cấu hình và bind các Settings từ appsettings.json
    services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));
    services.Configure<GoogleCloudStorageSettings>(configuration.GetSection("GoogleCloudStorageSettings"));

    // Cấu hình Authentication với JWT
    var jwtSettings = configuration.GetSection("JwtSettings").Get<JwtSettings>()!;
    services.AddAuthentication(options =>
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

    // Cấu hình Authorization
    services.AddAuthorization();

    // Cấu hình CORS
    services.AddCors(options =>
    {
        options.AddPolicy("AllowAll", policy =>
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader());
    });


    // Đăng ký các Services, Repositories (thông qua UnitOfWork), Validators, Mappers...
    services.AddScoped<IUnitOfWork, UnitOfWork>();
    services.AddScoped<IUserService, UserService>();
    services.AddScoped<ITokenService, TokenService>();
    services.AddScoped<ICategoryService, CategoryService>();
    services.AddScoped<IBookService, BookService>();
    services.AddScoped<IWishlistService, WishlistService>();
    services.AddScoped<ICartService, CartService>();
    services.AddScoped<IAddressService, AddressService>();
    services.AddScoped<IOrderService, OrderService>();
    services.AddScoped<IPromotionService, PromotionService>();
    services.AddScoped<IReviewService, ReviewService>();
    services.AddScoped<IDashboardService, DashboardService>();
    services.AddScoped<IReportService, ReportService>();
    services.AddScoped<ISupplierService, SupplierService>();
    services.AddScoped<IStockReceiptService, StockReceiptService>();
    services.AddScoped<IInventoryService, InventoryService>();
    services.AddScoped<IAuthorService, AuthorService>();
    services.AddSingleton<IFileStorageService, GoogleCloudStorageService>();

    // Đăng ký AutoMapper
    services.AddAutoMapper(typeof(Bookstore.Application.Mappings.MappingProfile).Assembly);

    // Đăng ký FluentValidation
    services.AddValidatorsFromAssembly(typeof(Bookstore.Application.Validators.Books.CreateBookDtoValidator).Assembly);
}


void ConfigurePipeline(WebApplication app, IWebHostEnvironment env, IServiceProvider services)
{
    // Sử dụng Middleware xử lý lỗi tập trung
    app.UseMiddleware<ErrorHandlingMiddleware>();

    if (env.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            var provider = services.GetRequiredService<IApiVersionDescriptionProvider>();
            foreach (var description in provider.ApiVersionDescriptions.Reverse())
            {
                options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json",
                    description.GroupName.ToUpperInvariant());
            }
            options.RoutePrefix = string.Empty; // Hiển thị Swagger tại root
        });
    }

    // Luôn sử dụng HSTS và HTTPS Redirection cho Production
    if (!env.IsDevelopment())
    {
        app.UseHsts();
    }
    //app.UseHttpsRedirection();

    app.UseStaticFiles();
    app.UseRouting();

    // Áp dụng chính sách CORS
    app.UseCors("AllowAll"); // Đơn giản hóa, có thể cấu hình chi tiết hơn cho Production

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();
}