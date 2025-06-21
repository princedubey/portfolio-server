using BlogManagementSystem.Data;
using BlogManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace BlogManagementSystem.Services
{
    public class BlogPostService : IBlogPostService
    {
        private readonly ApplicationDbContext _context;
        private readonly ISeoService _seoService;

        public BlogPostService(ApplicationDbContext context, ISeoService seoService)
        {
            _context = context;
            _seoService = seoService;
        }

        public async Task<IEnumerable<BlogPost>> GetAllBlogPostsAsync()
        {
            return await _context.BlogPosts
                .Include(bp => bp.Category)
                .Include(bp => bp.Author)
                .Include(bp => bp.BlogPostTags)
                    .ThenInclude(bpt => bpt.Tag)
                .OrderByDescending(bp => bp.CreatedDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<BlogPost>> GetPublishedBlogPostsAsync()
        {
            return await _context.BlogPosts
                .Where(bp => bp.IsPublished && bp.PublishedDate <= DateTime.UtcNow)
                .Include(bp => bp.Category)
                .Include(bp => bp.Author)
                .Include(bp => bp.BlogPostTags)
                    .ThenInclude(bpt => bpt.Tag)
                .OrderByDescending(bp => bp.PublishedDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<BlogPost>> GetFeaturedBlogPostsAsync()
        {
            return await _context.BlogPosts
                .Where(bp => bp.IsPublished && bp.IsFeatured && bp.PublishedDate <= DateTime.UtcNow)
                .Include(bp => bp.Category)
                .Include(bp => bp.Author)
                .Include(bp => bp.BlogPostTags)
                    .ThenInclude(bpt => bpt.Tag)
                .OrderByDescending(bp => bp.PublishedDate)
                .ToListAsync();
        }

        public async Task<BlogPost> GetBlogPostByIdAsync(int id)
        {
            return await _context.BlogPosts
                .Include(bp => bp.Category)
                .Include(bp => bp.Author)
                .Include(bp => bp.Comments.Where(c => c.IsApproved))
                    .ThenInclude(c => c.User)
                .Include(bp => bp.BlogPostTags)
                    .ThenInclude(bpt => bpt.Tag)
                .FirstOrDefaultAsync(bp => bp.Id == id);
        }

        public async Task<BlogPost> GetBlogPostBySlugAsync(string slug)
        {
            return await _context.BlogPosts
                .Include(bp => bp.Category)
                .Include(bp => bp.Author)
                .Include(bp => bp.Comments.Where(c => c.IsApproved))
                    .ThenInclude(c => c.User)
                .Include(bp => bp.BlogPostTags)
                    .ThenInclude(bpt => bpt.Tag)
                .FirstOrDefaultAsync(bp => bp.Slug == slug && bp.IsPublished && bp.PublishedDate <= DateTime.UtcNow);
        }

        public async Task<IEnumerable<BlogPost>> GetBlogPostsByCategoryAsync(int categoryId)
        {
            return await _context.BlogPosts
                .Where(bp => bp.CategoryId == categoryId && bp.IsPublished && bp.PublishedDate <= DateTime.UtcNow)
                .Include(bp => bp.Category)
                .Include(bp => bp.Author)
                .Include(bp => bp.BlogPostTags)
                    .ThenInclude(bpt => bpt.Tag)
                .OrderByDescending(bp => bp.PublishedDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<BlogPost>> GetBlogPostsByTagAsync(int tagId)
        {
            return await _context.BlogPosts
                .Where(bp => bp.BlogPostTags.Any(bpt => bpt.TagId == tagId) && bp.IsPublished && bp.PublishedDate <= DateTime.UtcNow)
                .Include(bp => bp.Category)
                .Include(bp => bp.Author)
                .Include(bp => bp.BlogPostTags)
                    .ThenInclude(bpt => bpt.Tag)
                .OrderByDescending(bp => bp.PublishedDate)
                .ToListAsync();
        }

        public async Task<BlogPost> CreateBlogPostAsync(BlogPost blogPost)
        {
            // Generate SEO-friendly slug
            blogPost.Slug = _seoService.GenerateSlug(blogPost.Title);
            
            // Set created date
            blogPost.CreatedDate = DateTime.UtcNow;
            
            // Generate excerpt if not provided
            if (string.IsNullOrEmpty(blogPost.Excerpt))
            {
                blogPost.Excerpt = _seoService.GenerateExcerpt(blogPost.Content);
            }

            _context.BlogPosts.Add(blogPost);
            await _context.SaveChangesAsync();
            return blogPost;
        }

        public async Task<BlogPost> UpdateBlogPostAsync(BlogPost blogPost)
        {
            var existingPost = await _context.BlogPosts
                .Include(bp => bp.BlogPostTags)
                .FirstOrDefaultAsync(bp => bp.Id == blogPost.Id);

            if (existingPost == null)
            {
                return null;
            }

            // Update properties
            existingPost.Title = blogPost.Title;
            existingPost.Content = blogPost.Content;
            existingPost.Excerpt = blogPost.Excerpt ?? _seoService.GenerateExcerpt(blogPost.Content);
            existingPost.CategoryId = blogPost.CategoryId;
            existingPost.UpdatedDate = DateTime.UtcNow;
            existingPost.MetaDescription = blogPost.MetaDescription;
            existingPost.MetaKeywords = blogPost.MetaKeywords;
            existingPost.FeaturedImageUrl = blogPost.FeaturedImageUrl;
            existingPost.IsFeatured = blogPost.IsFeatured;

            // Update slug only if title changed
            if (existingPost.Title != blogPost.Title)
            {
                existingPost.Slug = _seoService.GenerateSlug(blogPost.Title);
            }

            _context.Entry(existingPost).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return existingPost;
        }

        public async Task<bool> DeleteBlogPostAsync(int id)
        {
            var blogPost = await _context.BlogPosts.FindAsync(id);
            if (blogPost == null)
            {
                return false;
            }

            _context.BlogPosts.Remove(blogPost);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> PublishBlogPostAsync(int id)
        {
            var blogPost = await _context.BlogPosts.FindAsync(id);
            if (blogPost == null)
            {
                return false;
            }

            blogPost.IsPublished = true;
            blogPost.PublishedDate = DateTime.UtcNow;
            _context.Entry(blogPost).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UnpublishBlogPostAsync(int id)
        {
            var blogPost = await _context.BlogPosts.FindAsync(id);
            if (blogPost == null)
            {
                return false;
            }

            blogPost.IsPublished = false;
            _context.Entry(blogPost).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<BlogPost>> SearchBlogPostsAsync(string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm))
            {
                return await GetPublishedBlogPostsAsync();
            }

            return await _context.BlogPosts
                .Where(bp => bp.IsPublished && bp.PublishedDate <= DateTime.UtcNow &&
                       (bp.Title.Contains(searchTerm) || 
                        bp.Content.Contains(searchTerm) || 
                        bp.Excerpt.Contains(searchTerm) ||
                        bp.Category.Name.Contains(searchTerm) ||
                        bp.BlogPostTags.Any(bpt => bpt.Tag.Name.Contains(searchTerm))))
                .Include(bp => bp.Category)
                .Include(bp => bp.Author)
                .Include(bp => bp.BlogPostTags)
                    .ThenInclude(bpt => bpt.Tag)
                .OrderByDescending(bp => bp.PublishedDate)
                .ToListAsync();
        }

        public async Task<int> GetTotalPostsCountAsync()
        {
            return await _context.BlogPosts.CountAsync();
        }

        public async Task<int> GetPublishedPostsCountAsync()
        {
            return await _context.BlogPosts
                .Where(p => p.Status == PostStatus.Published)
                .CountAsync();
        }

        public async Task<int> GetDraftPostsCountAsync()
        {
            return await _context.BlogPosts
                .Where(p => p.Status == PostStatus.Draft)
                .CountAsync();
        }

        public async Task<IEnumerable<BlogPost>> GetRecentPostsAsync(int count)
        {
            return await _context.BlogPosts
                .Where(p => p.Status == PostStatus.Published)
                .OrderByDescending(p => p.CreatedDate)
                .Take(count)
                .ToListAsync();
        }

        public async Task<IEnumerable<BlogPost>> GetPopularPostsAsync(int count)
        {
            return await _context.BlogPosts
                .Where(p => p.Status == PostStatus.Published)
                .OrderByDescending(p => p.ViewCount)
                .Take(count)
                .ToListAsync();
        }

        public async Task<int> GetPostsCreatedInLastDaysAsync(int days)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-days);
            return await _context.BlogPosts
                .Where(p => p.CreatedDate >= cutoffDate)
                .CountAsync();
        }

        public async Task<IEnumerable<CategoryStats>> GetPostsByCategoryStatsAsync()
        {
            return await _context.BlogPosts
                .Where(p => p.Status == PostStatus.Published)
                .GroupBy(p => p.Category.Name)
                .Select(g => new CategoryStats
                {
                    CategoryName = g.Key,
                    PostCount = g.Count()
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<MonthlyStats>> GetPostsByMonthStatsAsync()
        {
            return await _context.BlogPosts
                .Where(p => p.Status == PostStatus.Published)
                .GroupBy(p => new { p.CreatedDate.Year, p.CreatedDate.Month })
                .Select(g => new MonthlyStats
                {
                    Month = $"{g.Key.Year}-{g.Key.Month:D2}",
                    PostCount = g.Count()
                })
                .OrderByDescending(s => s.Month)
                .ToListAsync();
        }
    }
}
