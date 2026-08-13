using System;
using GreenCart.Dtos.Requests.Categories;
using GreenCart.Dtos.Responses.Categories;
using GreenCart.Entities;
using GreenCart.Repositories;

namespace GreenCart.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly IUnitOfWork _unitOfWork;

        public CategoryService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<CategoryResponse>> GetCategoriesAsync()
        {
            var allCategories = (await _unitOfWork.Categories.FindAsync(c => !c.IsDeleted)).ToList();
            
            var rootCategories = allCategories.Where(c => c.ParentCategoryId == null).ToList();
            
            return rootCategories.Select(c => MapToCategoryResponse(c, allCategories)).ToList();
        }

        public async Task<CategoryResponse> GetCategoryByIdAsync(int id)
        {
            var category = await _unitOfWork.Categories.GetByIdAsync(id);
            if (category == null || category.IsDeleted)
            {
                throw new KeyNotFoundException($"Category with ID {id} not found.");
            }

            var allCategories = (await _unitOfWork.Categories.FindAsync(c => !c.IsDeleted)).ToList();
            return MapToCategoryResponse(category, allCategories);
        }

        public async Task<CategoryResponse> CreateCategoryAsync(CreateCategoryRequest request)
        {
            if (request.ParentCategoryId.HasValue)
            {
                var parent = await _unitOfWork.Categories.GetByIdAsync(request.ParentCategoryId.Value);
                if (parent == null || parent.IsDeleted)
                {
                    throw new InvalidOperationException("Parent category does not exist.");
                }
            }

            var category = new Category
            {
                Name = request.Name.Trim(),
                Slug = GenerateSlug(request.Name),
                Description = request.Description?.Trim(),
                ParentCategoryId = request.ParentCategoryId
            };

            await _unitOfWork.Categories.AddAsync(category);
            await _unitOfWork.SaveChangesAsync();

            var allCategories = (await _unitOfWork.Categories.FindAsync(c => !c.IsDeleted)).ToList();
            return MapToCategoryResponse(category, allCategories);
        }

        public async Task<CategoryResponse> UpdateCategoryAsync(int id, UpdateCategoryRequest request)
        {
            var category = await _unitOfWork.Categories.GetByIdAsync(id);
            if (category == null || category.IsDeleted)
            {
                throw new KeyNotFoundException($"Category with ID {id} not found.");
            }

            if (request.ParentCategoryId.HasValue && request.ParentCategoryId.Value != category.ParentCategoryId)
            {
                if (request.ParentCategoryId.Value == id)
                {
                    throw new InvalidOperationException("A category cannot be its own parent.");
                }

                var parent = await _unitOfWork.Categories.GetByIdAsync(request.ParentCategoryId.Value);
                if (parent == null || parent.IsDeleted)
                {
                    throw new InvalidOperationException("Parent category does not exist.");
                }
            }

            category.Name = request.Name.Trim();
            category.Slug = GenerateSlug(request.Name);
            category.Description = request.Description?.Trim();
            category.ParentCategoryId = request.ParentCategoryId;

            _unitOfWork.Categories.Update(category);
            await _unitOfWork.SaveChangesAsync();

            var allCategories = (await _unitOfWork.Categories.FindAsync(c => !c.IsDeleted)).ToList();
            return MapToCategoryResponse(category, allCategories);
        }

        public async Task DeleteCategoryAsync(int id, bool force = false)
        {
            var category = await _unitOfWork.Categories.GetByIdAsync(id);
            if (category == null || category.IsDeleted)
            {
                throw new KeyNotFoundException($"Category with ID {id} not found.");
            }

            var childCategories = await _unitOfWork.Categories.FindAsync(c => c.ParentCategoryId == id && !c.IsDeleted);
            if (childCategories.Any() && !force)
            {
                throw new InvalidOperationException("Cannot delete category with existing subcategories. Use force=true to override.");
            }

            if (force && childCategories.Any())
            {
                foreach (var child in childCategories)
                {
                    child.IsDeleted = true;
                    _unitOfWork.Categories.Update(child);
                }
            }

            category.IsDeleted = true;
            _unitOfWork.Categories.Update(category);
            await _unitOfWork.SaveChangesAsync();
        }

        private static CategoryResponse MapToCategoryResponse(Category category, List<Category> allCategories)
        {
            var subCategories = allCategories
                .Where(c => c.ParentCategoryId == category.Id)
                .Select(c => MapToCategoryResponse(c, allCategories))
                .ToList();

            return new CategoryResponse
            {
                Id = category.Id,
                Name = category.Name,
                Slug = category.Slug,
                Description = category.Description,
                ParentCategoryId = category.ParentCategoryId,
                SubCategories = subCategories
            };
        }

        private static string GenerateSlug(string text)
        {
            var slug = text.ToLowerInvariant().Trim();
            slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[^a-z0-9\s-]", "");
            slug = System.Text.RegularExpressions.Regex.Replace(slug, @"\s+", " ").Trim();
            slug = System.Text.RegularExpressions.Regex.Replace(slug, @"\s", "-");
            return $"{slug}-{Random.Shared.Next(1000, 9999)}";
        }
    }
}
