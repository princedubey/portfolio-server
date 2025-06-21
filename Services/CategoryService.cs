using BlogManagementSystem.Data;
using BlogManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace BlogManagementSystem.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ApplicationDbContext _context;
        private readonly ISeoService _seoService;

        public CategoryService(ApplicationDbContext context, ISeoService seoService)
        {
            _context = context;
            _seoService = seoService;
        }

        public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
        {
            return await _context.Categories
                .Include(c => c.BlogPosts)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<Category> GetCategoryByIdAsync(int id)
        {
            return await _context.Categories
                .Include(c => c.BlogPosts.Where(bp => bp.IsPublished))
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Category> GetCategoryBySlugAsync(string slug)
        {
            return await _context.Categories
                .Include(c => c.BlogPosts.Where(bp => bp.IsPublished))
                .FirstOrDefaultAsync(c => c.Slug == slug);
        }

        public async Task<IEnumerable<BlogPost>> GetCategoryPostsAsync(int categoryId)
        {
            return await _context.BlogPosts
                .Where(bp => bp.CategoryId == categoryId && bp.IsPublished)
                .Include(bp => bp.Author)
                .Include(bp => bp.BlogPostTags)
                    .ThenInclude(bpt => bpt.Tag)
                .OrderByDescending(bp => bp.PublishedDate)
                .ToListAsync();
        }

        public async Task<Category> CreateCategoryAsync(Category category)
        {
            category.Slug = _seoService.GenerateSlug(category.Name);
            
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            return category;
        }

        public async Task<Category> UpdateCategoryAsync(Category category)
        {
            var existingCategory = await _context.Categories.FindAsync(category.Id);
            if (existingCategory == null)
            {
                return null;
            }

            existingCategory.Name = category.Name;
            existingCategory.Description = category.Description;
            existingCategory.MetaDescription = category.MetaDescription;
            
            // Update slug if name changed
            if (existingCategory.Name != category.Name)
            {
                existingCategory.Slug = _seoService.GenerateSlug(category.Name);
            }

            _context.Entry(existingCategory).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return existingCategory;
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return false;
            }

            // Check if category has posts
            var hasPosts = await _context.BlogPosts.AnyAsync(bp => bp.CategoryId == id);
            if (hasPosts)
            {
                throw new InvalidOperationException("Cannot delete category that contains blog posts");
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> GetTotalCategoriesCountAsync()
        {
            return await _context.Categories.CountAsync();
        }
    }
}
