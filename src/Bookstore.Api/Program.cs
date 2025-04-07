using Bookstore.Domain.Interfaces.Repositories;
using Bookstore.Infrastructure.Persistence;
using Bookstore.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddOpenApi();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
        // (Optional) Configure SQL Server specific options like retry logic
        sqlServerOptionsAction: sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null);
        }));
// ----- Register Repositories -----
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>)); 
builder.Services.AddScoped<IUserRepository, UserRepository>(); 
builder.Services.AddScoped<IBookRepository, BookRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>(); 
builder.Services.AddScoped<IAuthorRepository, AuthorRepository>();  

builder.Services.AddControllers();
// ----- Configure API Versioning -----
builder.Services.AddApiVersioning(options =>
{
    options.AssumeDefaultVersionWhenUnspecified = true; 
    options.DefaultApiVersion = new Microsoft.AspNetCore.Mvc.ApiVersion(1, 0); 
    options.ReportApiVersions = true; // Trả về header 'api-supported-versions' trong response
});

builder.Services.AddVersionedApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});
// ----- End Configure API Versioning -----


// ----- Configure Swagger/OpenAPI -----
builder.Services.AddEndpointsApiExplorer(); // Cần thiết cho Minimal APIs và Swagger
builder.Services.AddSwaggerGen(options =>
{
    // --- Cấu hình cơ bản cho Swagger Doc (ví dụ cho v1.0) ---
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1.0",
        Title = "Bookstore Management API",
        Description = "API for managing the Bookstore application",
    });

});
// ----- End Configure Swagger/OpenAPI -----

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        // Tạo một endpoint cho mỗi phiên bản API được khám phá
        // Cần IApiVersionDescriptionProvider để lấy danh sách các version
        var provider = app.Services.GetRequiredService<Microsoft.AspNetCore.Mvc.ApiExplorer.IApiVersionDescriptionProvider>();
        foreach (var description in provider.ApiVersionDescriptions.Reverse()) // Reverse để v1 hiện trước
        {
            options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json",
                description.GroupName.ToUpperInvariant());
        }
        options.RoutePrefix = string.Empty; 
    });
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseRouting(); 
app.UseAuthorization(); 
app.MapControllers();
app.Run();
