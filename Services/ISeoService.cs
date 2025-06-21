using BlogManagementSystem.Models;

namespace BlogManagementSystem.Services
{
    public interface ISeoService
    {
        string GenerateSlug(string title);
        string GenerateExcerpt(string content, int maxLength = 300);
        string GenerateMetaDescription(string content, int maxLength = 160);
        string GenerateCanonicalUrl(string slug);
        string GenerateStructuredData(Models.BlogPost blogPost);
        Task<string> GenerateSlugAsync(string title);
        Task<string> GenerateMetaDescriptionAsync(string content);
        Task<string[]> GenerateMetaKeywordsAsync(string content);
        Task<SeoAnalysis> AnalyzePost(BlogPost post);
        Task<string> GenerateSitemapAsync();
        Task<string> GenerateRobotsTxt();
        Task<bool> BulkUpdateSlugsAsync();
    }

    public class SeoAnalysis
    {
        public string Content { get; set; }
        public List<string> Recommendations { get; set; }
    }
}
