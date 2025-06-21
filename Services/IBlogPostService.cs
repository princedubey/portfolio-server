using BlogManagementSystem.Models;

namespace BlogManagementSystem.Services
{
    public interface IBlogPostService
    {
        Task<IEnumerable<BlogPost>> GetAllBlogPostsAsync();
        Task<IEnumerable<BlogPost>> GetPublishedBlogPostsAsync();
        Task<IEnumerable<BlogPost>> GetFeaturedBlogPostsAsync();
        Task<BlogPost> GetBlogPostByIdAsync(int id);
        Task<BlogPost> GetBlogPostBySlugAsync(string slug);
        Task<IEnumerable<BlogPost>> GetBlogPostsByCategoryAsync(int categoryId);
        Task<IEnumerable<BlogPost>> GetBlogPostsByTagAsync(int tagId);
        Task<BlogPost> CreateBlogPostAsync(BlogPost blogPost);
        Task<BlogPost> UpdateBlogPostAsync(BlogPost blogPost);
        Task<bool> DeleteBlogPostAsync(int id);
        Task<bool> PublishBlogPostAsync(int id);
        Task<bool> UnpublishBlogPostAsync(int id);
        Task<IEnumerable<BlogPost>> SearchBlogPostsAsync(string searchTerm);
        Task<int> GetTotalPostsCountAsync();
        Task<int> GetPublishedPostsCountAsync();
        Task<int> GetDraftPostsCountAsync();
        Task<IEnumerable<BlogPost>> GetRecentPostsAsync(int count);
        Task<IEnumerable<BlogPost>> GetPopularPostsAsync(int count);
        Task<int> GetPostsCreatedInLastDaysAsync(int days);
        Task<IEnumerable<CategoryStats>> GetPostsByCategoryStatsAsync();
        Task<IEnumerable<MonthlyStats>> GetPostsByMonthStatsAsync();
    }

    public class CategoryStats
    {
        public string CategoryName { get; set; }
        public int PostCount { get; set; }
    }

    public class MonthlyStats
    {
        public string Month { get; set; }
        public int PostCount { get; set; }
    }
}
