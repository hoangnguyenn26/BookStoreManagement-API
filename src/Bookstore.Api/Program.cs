using Bookstore.Api.Middleware;
using Bookstore.Application.Interfaces;
using Bookstore.Application.Interfaces.Services;
using Bookstore.Application.Services;
using Bookstore.Application.Settings;
using Bookstore.Application.Validators.Categories;
using Bookstore.Domain.Interfaces.Services;
using Bookstore.Infrastructure.Persistence;
using Bookstore.Infrastructure.Services;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Reflection;
using System.Text;

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
        .AddEnvironmentVariables()
        .Build())
    .Enrich.FromLogContext()
    .CreateBootstrapLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext());

// --- Bind JwtSettings from configuration ---
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>()
                  ?? throw new InvalidOperationException("JwtSettings is not configured correctly in user secrets or appsettings.");



// ----- Configure DbContext -----
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ----- Register UnitOfWork & Services -----
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IBookService, BookService>();
builder.Services.AddScoped<IWishlistService, WishlistService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IAddressService, AddressService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IPromotionService, PromotionService>();
builder.Services.AddScoped<IReviewService, ReviewService>();

// ----- Register AutoMapper -----
builder.Services.AddAutoMapper(typeof(Bookstore.Application.Mappings.MappingProfile).Assembly);

// ----- Configure Authentication -----
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
string corsPolicyDevelopment = "AllowSwaggerAndDevClients";
string corsPolicyProduction = "AllowAppClients";

builder.Services.AddCors(options =>
{
    options.AddPolicy(corsPolicyDevelopment, policy =>
    {
        var developmentOrigins = new List<string>();
        var launchSettingsPath = Path.Combine(Directory.GetCurrentDirectory(), "Properties", "launchSettings.json");
        if (File.Exists(launchSettingsPath))
        {
            try
            {
                var launchSettingsJson = File.ReadAllText(launchSettingsPath);
                using var launchSettings = System.Text.Json.JsonDocument.Parse(launchSettingsJson);
                var profiles = launchSettings.RootElement.GetProperty("profiles");
                var urls = profiles.EnumerateObject()
                                   .SelectMany(p => p.Value.TryGetProperty("applicationUrl", out var appUrl) ? appUrl.GetString()?.Split(';') ?? Array.Empty<string>() : Array.Empty<string>())
                                   .Where(url => !string.IsNullOrWhiteSpace(url) && (url.StartsWith("http://") || url.StartsWith("https://")))
                                   .Select(url => url.TrimEnd('/'))
                                   .Distinct()
                                   .ToList();
                if (urls.Any())
                {
                    developmentOrigins.AddRange(urls);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Could not read launchSettings.json for CORS origins. Using defaults. Error: {ex.Message}");
                developmentOrigins.Add("https://localhost:7001");
                developmentOrigins.Add("http://localhost:7000");
            }
        }
        else
        {
            developmentOrigins.Add("https://localhost:7001");
            developmentOrigins.Add("http://localhost:7000");
        }

        Console.WriteLine("Development CORS Origins: " + string.Join(", ", developmentOrigins)); // Log để kiểm tra
        policy.WithOrigins(developmentOrigins.ToArray())
              .AllowAnyHeader()
              .AllowAnyMethod();
    });

    options.AddPolicy(corsPolicyProduction, policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});


// ----- Configure Controllers -----
builder.Services.AddControllers();

// ----- Configure FluentValidation -----
builder.Services.AddValidatorsFromAssemblyContaining<CreateCategoryDtoValidator>();


// ----- Configure Swagger/OpenAPI -----
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1.0",
        Title = "Bookstore Management API",
        Description = "API for managing the Bookstore application",
    });

    // --- Cấu hình để Swagger hiểu JWT Authentication ---
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
            Name = "Bearer",
            In = ParameterLocation.Header,
        },
        new List<string>()
        }
    });

    // --- Tích hợp XML Comments ---
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});


// Build application host
var app = builder.Build();


// Configure the HTTP request pipeline (Middleware).

app.UseMiddleware<ErrorHandlingMiddleware>();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
        foreach (var description in provider.ApiVersionDescriptions.Reverse())
        {
            options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json",
                description.GroupName.ToUpperInvariant());
        }
        options.RoutePrefix = string.Empty;
    });
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler(appBuilder =>
    {
        appBuilder.Run(async context =>
        {
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new { Message = "An unexpected server error occurred. Please try again later." });
        });
    });
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

// ----- Áp dụng chính sách CORS -----
if (app.Environment.IsDevelopment())
{
    app.UseCors(corsPolicyDevelopment);
}
else
{
    app.UseCors(corsPolicyProduction);
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();