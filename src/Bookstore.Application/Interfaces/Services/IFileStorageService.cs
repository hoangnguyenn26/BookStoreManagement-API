using Microsoft.AspNetCore.Http;

namespace Bookstore.Application.Interfaces.Services
{
    public interface IFileStorageService
    {
        Task<string?> SaveFileAsync(
            IFormFile file,
            string subDirectory,
            string[] allowedExtensions,
            double maxFileSizeMB = 5.0, // Mặc định 5MB
            CancellationToken cancellationToken = default);
        void DeleteFile(string? relativePath);
    }
}