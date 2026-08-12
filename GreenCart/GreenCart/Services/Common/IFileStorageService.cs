using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace GreenCart.Services.Common
{
    public interface IFileStorageService
    {
        /// <summary>
        /// Saves uploaded file to wwwroot/images/products/{productId}/
        /// Validates extension (.jpg, .jpeg, .png, .webp) and max size (5MB).
        /// Returns relative path (e.g. /images/products/1/file.jpg)
        /// </summary>
        Task<string> SaveProductImageAsync(IFormFile file, int productId);

        /// <summary>
        /// Deletes file from local storage given relative path.
        /// </summary>
        Task<bool> DeleteFileAsync(string relativePath);
    }
}
