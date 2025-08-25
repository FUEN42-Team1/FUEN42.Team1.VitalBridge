using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Team1.VitalBridge.Frontend.Models.DTOs.ECShop;
using Team1.VitalBridge.Frontend.Models.EFModels;

namespace Team1.VitalBridge.Frontend.Models.Services
{
    public class CategoryService
    {
        private readonly AppDbContext _context;

        public CategoryService(AppDbContext context)
        {
            _context=context;
        }

        /// <summary>
        /// 取得類別完整路徑（只處理啟用的類別）
        /// </summary>
        /// <param name="categoryId">類別ID</param>
        /// <returns>類別路徑回應 DTO</returns>
        public async Task<CategoryPathResponseDto> GetCategoryPathAsync(int categoryId)
        {
            var category = await _context.Categories
                .Include(c => c.Father)
                .FirstOrDefaultAsync(c => c.Id == categoryId && c.IsActive);

            if (category == null)
            {
                return null;
            }

            var path = await BuildCategoryPathAsync(category);

            return new CategoryPathResponseDto
            {
                CategoryId = categoryId,
                CategoryPath = path
            };
        }
        /// <summary>
        /// 取得類別階層資訊（只處理啟用的類別）
        /// </summary>
        /// <param name="categoryId">類別ID</param>
        /// <returns>類別階層回應 DTO</returns>
        public async Task<CategoryHierarchyResponseDto> GetCategoryHierarchyAsync(int categoryId)
        {
            var category = await _context.Categories
                .Include(c => c.Father)
                .Include(c => c.InverseFather)
                .FirstOrDefaultAsync(c => c.Id == categoryId && c.IsActive);

            if (category == null)
            {
                return null;
            }

            var path = await BuildCategoryPathAsync(category);

            // 只取得啟用的子類別
            var children = category.InverseFather
                .Where(c => c.IsActive)
                .Select(c => MapToCategoryDto(c, path.Count))
                .OrderBy(c => c.Name)
                .ToList();

            return new CategoryHierarchyResponseDto
            {
                Category = MapToCategoryDto(category, path.Count - 1),
                CategoryPath = path,
                ParentCategory = category.Father != null && category.Father.IsActive
                    ? MapToCategoryDto(category.Father, path.Count - 2)
                    : null,
                Children = children
            };
        }

        /// <summary>
        /// 取得所有啟用的類別
        /// </summary>
        /// <returns>類別列表</returns>
        public async Task<List<CategoryDto>> GetEnabledCategoriesAsync()
        {
            var categories = await _context.Categories
                .Where(c => c.IsActive)
                .Include(c => c.Father)
                .OrderBy(c => c.Name)
                .ToListAsync();

            return categories.Select(c => MapToCategoryDto(c)).ToList();
        }
        /// <summary>
        /// 取得啟用的根類別列表
        /// </summary>
        /// <returns>根類別列表</returns>
        public async Task<List<CategoryTreeDto>> GetRootCategoriesAsync()
        {
            var rootCategories = await _context.Categories
                .Where(c => c.FatherId == null && c.IsActive)
                .OrderBy(c => c.Name)
                .ToListAsync();

            return rootCategories.Select(c => new CategoryTreeDto
            {
                Id = c.Id,
                Name = c.Name,
                FatherId = c.FatherId,
                IsActive = true, // 確定是啟用的
                HasChildren = c.InverseFather.Any(child => child.IsActive)
            }).ToList();
        }
        /// <summary>
        /// 取得指定類別的啟用子類別
        /// </summary>
        /// <param name="parentId">父類別ID</param>
        /// <returns>子類別列表</returns>
        public async Task<List<CategoryTreeDto>> GetChildCategoriesAsync(int parentId)
        {
            var parentCategory = await _context.Categories
                .Include(c => c.InverseFather)
                .FirstOrDefaultAsync(c => c.Id == parentId && c.IsActive);

            if (parentCategory == null)
            {
                return new List<CategoryTreeDto>();
            }

            return parentCategory.InverseFather
                .Where(c => c.IsActive)
                .Select(c => new CategoryTreeDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    FatherId = c.FatherId,
                    IsActive = true, // 確定是啟用的
                    HasChildren = c.InverseFather.Any(child => child.IsActive)
                })
                .OrderBy(c => c.Name)
                .ToList();
        }
        /// <summary>
        /// 根據類別 ID 取得類別詳情
        /// </summary>
        /// <param name="categoryId">類別ID</param>
        /// <returns>類別 DTO</returns>
        public async Task<CategoryDto> GetCategoryByIdAsync(int categoryId)
        {
            var category = await _context.Categories
                .Include(c => c.Father)
                .FirstOrDefaultAsync(c => c.Id == categoryId && c.IsActive);

            return category != null ? MapToCategoryDto(category) : null;
        }

        /// <summary>
        /// 建立類別完整路徑（只包含啟用的類別）
        /// </summary>
        /// <param name="category">目標類別</param>
        /// <returns>類別路徑列表</returns>
        private async Task<List<CategoryDto>> BuildCategoryPathAsync(Category category)
        {
            var path = new List<Category>();
            var current = category;

            // 往上遞迴找啟用的父類別
            while (current != null && current.IsActive)
            {
                path.Insert(0, current); // 插入到開頭，保持從根到葉的順序

                if (current.FatherId.HasValue)
                {
                    current = await _context.Categories
                        .Include(c => c.Father)
                        .FirstOrDefaultAsync(c => c.Id == current.FatherId.Value && c.IsActive);
                }
                else
                {
                    current = null;
                }
            }

            // 轉換為 CategoryDto 格式
            return path.Select((c, index) => MapToCategoryDto(c, index)).ToList();
        }

        /// <summary>
        /// 將 Category Entity 轉換為 CategoryDto
        /// </summary>
        /// <param name="category">類別實體</param>
        /// <param name="level">階層等級</param>
        /// <returns>類別 DTO</returns>
        private CategoryDto MapToCategoryDto(Category category, int level = 0)
        {
            if (category == null) return null;

            return new CategoryDto
            {
                // 原有屬性
                Id = category.Id,
                Name = category.Name,

                // 新增屬性（使用正確的命名）
                FatherId = category.FatherId,      // 對應 EF Model 的 FatherId
                FatherName = category.Father?.Name, // 對應 EF Model 的 Father.Name
                IsActive = true, // 前台顯示的一定是啟用的
                Level = level
            };
        }






    }
}