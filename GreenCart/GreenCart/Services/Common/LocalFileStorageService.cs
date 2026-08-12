using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace GreenCart.Services.Common
{
    public class LocalFileStorageService : IFileStorageService
    {
        private readonly IWebHostEnvironment _environment;
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
        private const long MaxFileSizeInBytes = 5 * 1024 * 1024; // 5MB

        public LocalFileStorageService(IWebHostEnvironment environment)
        {
            _environment = environment ?? throw new ArgumentNullException(nameof(environment));
        }

        public async Task<string> SaveProductImageAsync(IFormFile file, int productId)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("Uploaded file is empty or null.", nameof(file));
            }

            if (file.Length > MaxFileSizeInBytes)
            {
                throw new ArgumentException($"File size exceeds maximum allowed limit of {MaxFileSizeInBytes / (1024 * 1024)}MB.");
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (string.IsNullOrEmpty(extension) || !AllowedExtensions.Contains(extension))
            {
                throw new ArgumentException($"Invalid file extension '{extension}'. Allowed extensions are: {string.Join(", ", AllowedExtensions)}");
            }

            // Determine target directory: wwwroot/images/products/{productId}/
            string webRootPath = _environment.WebRootPath;
            if (string.IsNullOrEmpty(webRootPath))
            {
                webRootPath = Path.Combine(_environment.ContentRootPath, "wwwroot");
            }

            string relativeFolder = Path.Combine("images", "products", productId.ToString());
            string absoluteFolder = Path.Combine(webRootPath, relativeFolder);

            if (!Directory.Exists(absoluteFolder))
            {
                Directory.CreateDirectory(absoluteFolder);
            }

            // Generate unique filename
            string uniqueFileName = $"{Guid.NewGuid()}{extension}";
            string absoluteFilePath = Path.Combine(absoluteFolder, uniqueFileName);

            using (var stream = new FileStream(absoluteFilePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Return relative URL with forward slashes: e.g., /images/products/1/abc.jpg
            string relativeUrl = $"/images/products/{productId}/{uniqueFileName}";
            return relativeUrl;
        }

        public Task<bool> DeleteFileAsync(string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
            {
                return Task.FromResult(false);
            }

            string webRootPath = _environment.WebRootPath;
            if (string.IsNullOrEmpty(webRootPath))
            {
                webRootPath = Path.Combine(_environment.ContentRootPath, "wwwroot");
            }

            // Remove leading slash or tilde if present
            string cleanRelativePath = relativePath.TrimStart('~', '/').Replace('/', Path.DirectorySeparatorChar);
            string fullPath = Path.Combine(webRootPath, cleanRelativePath);

            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
                return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }
    }
}
