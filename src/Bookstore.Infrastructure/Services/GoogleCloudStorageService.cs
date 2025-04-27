using Bookstore.Application.Interfaces.Services;
using Bookstore.Application.Settings;
using Google.Cloud.Storage.V1;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System.Net;

namespace Bookstore.Infrastructure.Services
{
    public class GoogleCloudStorageService : IFileStorageService
    {
        private readonly StorageClient _storageClient;
        private readonly string _bucketName;
        private readonly ILogger<GoogleCloudStorageService> _logger;

        // Định nghĩa kích thước và hậu tố
        private const int ThumbnailWidth = 150;
        private const int MediumWidth = 500;
        private const string ThumbnailSuffix = "_T";
        private const string MediumSuffix = "_M";

        public GoogleCloudStorageService(IOptions<GoogleCloudStorageSettings> gcsSettingsOptions, ILogger<GoogleCloudStorageService> logger)
        {
            _logger = logger;
            var gcsSettings = gcsSettingsOptions.Value ?? throw new ArgumentNullException(nameof(gcsSettingsOptions), "GCS Settings not configured.");
            _bucketName = gcsSettings.BucketName;
            // --- DEBUG CODE ---
            string? credPathFromEnv = Environment.GetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS");
            _logger.LogWarning("DEBUG: GOOGLE_APPLICATION_CREDENTIALS value read: '{CredPath}'", credPathFromEnv ?? "NULL"); // Log giá trị đọc được

            if (!string.IsNullOrEmpty(credPathFromEnv))
            {
                _logger.LogWarning("DEBUG: Checking if file exists at path: '{PathExists}'", File.Exists(credPathFromEnv));
                try
                {
                    Uri testUri = new Uri(credPathFromEnv);
                    _logger.LogWarning("DEBUG: Parsed Path as URI successfully: {UriPath}", testUri.LocalPath);
                }
                catch (Exception uriEx)
                {
                    _logger.LogError(uriEx, "DEBUG: Failed to parse path as URI.");
                }

                // Thử đọc file trực tiếp để xem lỗi IO khác không
                try
                {
                    string fileContent = File.ReadAllText(credPathFromEnv);
                    _logger.LogWarning("DEBUG: Successfully read content from credential file.");
                }
                catch (IOException ioEx)
                {
                    _logger.LogError(ioEx, "DEBUG: IOException when trying to read credential file directly.");
                }
                catch (Exception fileEx)
                {
                    _logger.LogError(fileEx, "DEBUG: Generic exception when trying to read credential file directly.");
                }
            }
            try
            {
                _storageClient = StorageClient.Create();
                _logger.LogInformation("Google Cloud Storage client created successfully using Application Default Credentials or Environment Variable.");
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Failed to create Google Cloud Storage client. Ensure GOOGLE_APPLICATION_CREDENTIALS environment variable is set correctly or ADC is configured.");
                throw;
            }
        }

        public async Task<string?> SaveFileAsync(
            IFormFile file,
            string subDirectory,
            string[] allowedExtensions,
            double maxFileSizeMB = 5,
            CancellationToken cancellationToken = default)
        {
            if (file == null || file.Length == 0) return null;

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
            {
                _logger.LogWarning("SaveFileAsync: Invalid file extension '{Extension}'.", extension);
                return null; // Hoặc throw ValidationException
            }
            if (file.Length > maxFileSizeMB * 1024 * 1024)
            {
                _logger.LogWarning("SaveFileAsync: File size exceeds limit.");
                return null; // Hoặc throw ValidationException
            }

            // Tạo tên gốc duy nhất (không có suffix)
            var baseObjectName = $"{Guid.NewGuid()}";
            var originalObjectNameWithExt = $"{baseObjectName}{extension}"; // vd: guid.jpg
            var objectNamePrefix = !string.IsNullOrWhiteSpace(subDirectory) ? $"{subDirectory.Replace('\\', '/')}/{baseObjectName}" : baseObjectName; // vd: images/covers/guid

            string? mediumImageUrl = null;

            try
            {
                // ---- Upload Ảnh Gốc ----
                _logger.LogInformation("Uploading original image to GCS: {ObjectName}", $"{objectNamePrefix}{extension}");
                using (var stream = file.OpenReadStream())
                {
                    await _storageClient.UploadObjectAsync(
                        _bucketName,
                        $"{objectNamePrefix}{extension}",
                        file.ContentType,
                        stream,
                        options: null,
                        cancellationToken: cancellationToken);
                }
                string originalImageUrl = $"https://storage.googleapis.com/{_bucketName}/{objectNamePrefix}{extension}";


                // ---- Resize và Upload Ảnh Medium ----
                mediumImageUrl = await ResizeAndUploadAsync(file, objectNamePrefix, MediumSuffix, MediumWidth, extension, file.ContentType, cancellationToken);
                if (mediumImageUrl == null) _logger.LogWarning("Failed to generate Medium size for {BaseObjectName}", baseObjectName);

                // ---- Resize và Upload Ảnh Thumbnail ----
                string? thumbnailUrl = await ResizeAndUploadAsync(file, objectNamePrefix, ThumbnailSuffix, ThumbnailWidth, extension, file.ContentType, cancellationToken);
                if (thumbnailUrl == null) _logger.LogWarning("Failed to generate Thumbnail size for {BaseObjectName}", baseObjectName);


                _logger.LogInformation("Image and renditions uploaded successfully for base name: {BaseObjectName}", baseObjectName);

                return mediumImageUrl ?? originalImageUrl;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file to GCS for base name: {BaseObjectName}", baseObjectName);
                return null;
            }
        }

        private async Task<string?> ResizeAndUploadAsync(IFormFile originalFile, string objectNamePrefix, string sizeSuffix, int targetWidth, string extension, string? contentType, CancellationToken cancellationToken)
        {
            try
            {
                using var stream = originalFile.OpenReadStream();
                using var image = await Image.LoadAsync(stream, cancellationToken);

                var options = new ResizeOptions
                {
                    Size = new Size(targetWidth, 0),
                    Mode = ResizeMode.Max
                };
                image.Mutate(x => x.Resize(options));

                using var outputStream = new MemoryStream();
                await image.SaveAsync(outputStream, GetImageEncoder(extension), cancellationToken);
                outputStream.Position = 0;

                var resizedObjectName = $"{objectNamePrefix}{sizeSuffix}{extension}";

                _logger.LogInformation("Uploading resized image to GCS: {ObjectName}", resizedObjectName);
                await _storageClient.UploadObjectAsync(
                     _bucketName,
                     resizedObjectName,
                     contentType,
                     outputStream,
                     options: null,
                     cancellationToken: cancellationToken);

                return $"https://storage.googleapis.com/{_bucketName}/{resizedObjectName}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resizing/uploading image with suffix {SizeSuffix} for base name {BaseObjectName}", sizeSuffix, objectNamePrefix);
                return null;
            }
        }

        // Helper lấy Encoder cho ImageSharp dựa trên đuôi file
        private SixLabors.ImageSharp.Formats.IImageEncoder GetImageEncoder(string extension)
        {
            return extension.ToLowerInvariant() switch
            {
                ".png" => new SixLabors.ImageSharp.Formats.Png.PngEncoder(),
                ".gif" => new SixLabors.ImageSharp.Formats.Gif.GifEncoder(),
                ".jpg" or ".jpeg" or _ => new SixLabors.ImageSharp.Formats.Jpeg.JpegEncoder { Quality = 80 },
            };
        }


        public void DeleteFile(string? relativeOrAbsoluteUrl)
        {
            if (string.IsNullOrWhiteSpace(relativeOrAbsoluteUrl) || !relativeOrAbsoluteUrl.Contains(_bucketName)) return;

            try
            {
                // Trích xuất object name từ URL
                // URL: https://storage.googleapis.com/yourbookstore-covers/images/covers/guid_M.jpg
                var uri = new Uri(relativeOrAbsoluteUrl);
                var objectNameWithPrefix = uri.AbsolutePath.TrimStart('/'); // Bỏ dấu '/' đầu
                objectNameWithPrefix = objectNameWithPrefix.Substring(objectNameWithPrefix.IndexOf('/') + 1); // Bỏ tên bucket

                // Suy ra tên gốc và các tên khác
                var extension = Path.GetExtension(objectNameWithPrefix); // .jpg
                var nameWithoutExt = objectNameWithPrefix.Substring(0, objectNameWithPrefix.Length - extension.Length); // images/covers/guid_M
                var baseObjectName = nameWithoutExt;
                if (baseObjectName.EndsWith(MediumSuffix)) baseObjectName = baseObjectName.Substring(0, baseObjectName.Length - MediumSuffix.Length);
                else if (baseObjectName.EndsWith(ThumbnailSuffix)) baseObjectName = baseObjectName.Substring(0, baseObjectName.Length - ThumbnailSuffix.Length);

                _logger.LogInformation("Attempting to delete GCS objects for base name: {BaseObjectName}", baseObjectName);

                // Xóa các phiên bản ảnh
                DeleteObjectIfExistsAsync($"{baseObjectName}{extension}", CancellationToken.None).ConfigureAwait(false); // Original
                DeleteObjectIfExistsAsync($"{baseObjectName}{MediumSuffix}{extension}", CancellationToken.None).ConfigureAwait(false); // Medium
                DeleteObjectIfExistsAsync($"{baseObjectName}{ThumbnailSuffix}{extension}", CancellationToken.None).ConfigureAwait(false); // Thumbnail

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting file from GCS: {FileUrl}", relativeOrAbsoluteUrl);
            }
        }

        // Helper xóa object, bỏ qua lỗi nếu không tồn tại
        private async Task DeleteObjectIfExistsAsync(string objectName, CancellationToken cancellationToken)
        {
            try
            {
                await _storageClient.DeleteObjectAsync(_bucketName, objectName, cancellationToken: cancellationToken);
                _logger.LogInformation("Deleted GCS object: {ObjectName}", objectName);
            }
            catch (Google.GoogleApiException e) when (e.HttpStatusCode == HttpStatusCode.NotFound)
            {
                _logger.LogWarning("GCS object not found for deletion (ignored): {ObjectName}", objectName);
                // Bỏ qua lỗi không tìm thấy
            }
            // Các lỗi khác sẽ được ném ra
        }
    }
}