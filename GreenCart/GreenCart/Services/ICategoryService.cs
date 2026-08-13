using System.Collections.Generic;
using System.Threading.Tasks;
using GreenCart.Dtos.Requests.Categories;
using GreenCart.Dtos.Responses.Categories;

namespace GreenCart.Services
{
    public interface ICategoryService
    {
        Task<IEnumerable<CategoryResponse>> GetCategoriesAsync();
        Task<CategoryResponse> GetCategoryByIdAsync(int id);
        Task<CategoryResponse> CreateCategoryAsync(CreateCategoryRequest request);
        Task<CategoryResponse> UpdateCategoryAsync(int id, UpdateCategoryRequest request);
        Task DeleteCategoryAsync(int id, bool force = false);
    }
}
