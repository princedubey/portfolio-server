using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using BlogManagementSystem.Models;
using Microsoft.Extensions.Configuration;
using BlogManagementSystem.Data;
using Microsoft.EntityFrameworkCore;
using System.Xml;

namespace BlogManagementSystem.Services
{
    public class SeoService : ISeoService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public SeoService(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public string GenerateSlug(string title)
        {
            if (string.IsNullOrEmpty(title))
                return string.Empty;

            // Remove diacritics (accents)
            string normalizedString = title.Normalize(NormalizationForm.FormD);
            StringBuilder stringBuilder = new StringBuilder();

            foreach (char c in normalizedString)
            {
                UnicodeCategory unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            // Convert to lowercase and replace spaces with hyphens
            string slug = stringBuilder.ToString().ToLowerInvariant()
                .Replace(" ", "-")
                .Replace("'", "")
                .Replace("\"", "")
                .Replace("&", "and");

            // Remove invalid characters
            slug = Regex.Replace(slug, @"[^a-z0-9\-]", "");

            // Remove multiple hyphens
            slug = Regex.Replace(slug, @"-{2,}", "-");

            // Trim hyphens from start and end
            slug = slug.Trim('-');

            return slug;
        }

        public string GenerateExcerpt(string content, int maxLength = 300)
        {
            if (string.IsNullOrEmpty(content))
                return string.Empty;

            // Remove HTML tags
            string plainText = Regex.Replace(content, @"<[^>]+>", "");

            // Trim whitespace
            plainText = plainText.Trim();

            // Truncate to maxLength
            if (plainText.Length <= maxLength)
                return plainText;

            // Find the last space before maxLength
            int lastSpace = plainText.LastIndexOf(' ', maxLength);
            if (lastSpace > 0)
                return plainText.Substring(0, lastSpace) + "...";

            return plainText.Substring(0, maxLength) + "...";
        }

        public string GenerateMetaDescription(string content, int maxLength = 160)
        {
            return GenerateExcerpt(content, maxLength);
        }

        public string GenerateCanonicalUrl(string slug)
        {
            string baseUrl = _configuration["SiteSettings:BaseUrl"];
            return $"{baseUrl}/blog/{slug}";
        }

        public string GenerateStructuredData(BlogPost blogPost)
        {
            var structuredData = new
            {
                @context = "https://schema.org",
                @type = "BlogPosting",
                mainEntityOfPage = new
                {
                    @type = "WebPage",
                    @id = GenerateCanonicalUrl(blogPost.Slug)
                },
                headline = blogPost.Title,
                description = blogPost.MetaDescription,
                image = blogPost.FeaturedImageUrl,
                author = new
                {
                    @type = "Person",
                    name = $"{blogPost.Author.FirstName} {blogPost.Author.LastName}".Trim()
                },
                publisher = new
                {
                    @type = "Organization",
                    name = _configuration["SiteSettings:SiteName"],
                    logo = new
                    {
                        @type = "ImageObject",
                        url = _configuration["SiteSettings:LogoUrl"]
                    }
                },
                datePublished = blogPost.PublishedDate?.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                dateModified = blogPost.UpdatedDate?.ToString("yyyy-MM-ddTHH:mm:ssZ")
            };

            return JsonSerializer.Serialize(structuredData);
        }

        public async Task<string> GenerateSitemapAsync()
        {
            var baseUrl = _configuration["BaseUrl"];
            var posts = await _context.BlogPosts
                .Where(p => p.Status == PostStatus.Published)
                .ToListAsync();

            var xml = new StringBuilder();
            xml.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            xml.AppendLine("<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">");

            // Add homepage
            xml.AppendLine("  <url>");
            xml.AppendLine($"    <loc>{baseUrl}</loc>");
            xml.AppendLine("    <changefreq>daily</changefreq>");
            xml.AppendLine("    <priority>1.0</priority>");
            xml.AppendLine("  </url>");

            // Add blog posts
            foreach (var post in posts)
            {
                xml.AppendLine("  <url>");
                xml.AppendLine($"    <loc>{baseUrl}/blog/{post.Slug}</loc>");
                xml.AppendLine($"    <lastmod>{post.UpdatedDate:yyyy-MM-dd}</lastmod>");
                xml.AppendLine("    <changefreq>weekly</changefreq>");
                xml.AppendLine("    <priority>0.8</priority>");
                xml.AppendLine("  </url>");
            }

            xml.AppendLine("</urlset>");
            return xml.ToString();
        }

        public async Task<string> GenerateRobotsTxt()
        {
            var baseUrl = _configuration["BaseUrl"];
            var sb = new StringBuilder();

            sb.AppendLine("User-agent: *");
            sb.AppendLine("Allow: /");
            sb.AppendLine($"Sitemap: {baseUrl}/sitemap.xml");

            return sb.ToString();
        }

        public async Task<SeoAnalysis> AnalyzePost(BlogPost post)
        {
            var analysis = new SeoAnalysis
            {
                Content = post.Content,
                Recommendations = new List<string>()
            };

            // Check title length
            if (post.Title.Length < 30 || post.Title.Length > 60)
            {
                analysis.Recommendations.Add("Title should be between 30 and 60 characters");
            }

            // Check meta description length
            if (string.IsNullOrEmpty(post.MetaDescription) || post.MetaDescription.Length < 120 || post.MetaDescription.Length > 160)
            {
                analysis.Recommendations.Add("Meta description should be between 120 and 160 characters");
            }

            // Check content length
            if (post.Content.Length < 300)
            {
                analysis.Recommendations.Add("Content should be at least 300 characters long");
            }

            // Check for images
            if (!post.Content.Contains("<img"))
            {
                analysis.Recommendations.Add("Consider adding images to improve engagement");
            }

            // Check for headings
            if (!post.Content.Contains("<h1") || !post.Content.Contains("<h2"))
            {
                analysis.Recommendations.Add("Use proper heading hierarchy (H1, H2, etc.)");
            }

            return analysis;
        }

        public async Task<bool> BulkUpdateSlugsAsync()
        {
            var posts = await _context.BlogPosts.ToListAsync();
            var updated = false;

            foreach (var post in posts)
            {
                var newSlug = await GenerateSlugAsync(post.Title);
                if (newSlug != post.Slug)
                {
                    post.Slug = newSlug;
                    updated = true;
                }
            }

            if (updated)
            {
                await _context.SaveChangesAsync();
            }

            return updated;
        }

        public async Task<string> GenerateSlugAsync(string title)
        {
            if (string.IsNullOrEmpty(title))
                return string.Empty;

            // Remove diacritics
            string normalizedString = title.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (char c in normalizedString)
            {
                if (System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c) != System.Globalization.UnicodeCategory.NonSpacingMark)
                    stringBuilder.Append(c);
            }

            string slug = stringBuilder.ToString().Normalize(NormalizationForm.FormC);

            // Convert to lowercase
            slug = slug.ToLowerInvariant();

            // Replace spaces with hyphens
            slug = Regex.Replace(slug, @"\s+", "-");

            // Remove invalid chars
            slug = Regex.Replace(slug, @"[^a-z0-9\s-]", "");

            // Remove multiple hyphens
            slug = Regex.Replace(slug, @"-+", "-");

            // Trim hyphens from start and end
            slug = slug.Trim('-');

            return slug;
        }

        public async Task<string> GenerateMetaDescriptionAsync(string content)
        {
            if (string.IsNullOrEmpty(content))
                return string.Empty;

            // Remove HTML tags
            string plainText = Regex.Replace(content, "<.*?>", string.Empty);

            // Remove extra whitespace
            plainText = Regex.Replace(plainText, @"\s+", " ").Trim();

            // Take first 160 characters
            if (plainText.Length > 160)
            {
                plainText = plainText.Substring(0, 157) + "...";
            }

            return plainText;
        }

        public async Task<string[]> GenerateMetaKeywordsAsync(string content)
        {
            if (string.IsNullOrEmpty(content))
                return Array.Empty<string>();

            // Remove HTML tags
            string plainText = Regex.Replace(content, "<.*?>", string.Empty);

            // Split into words
            var words = plainText.Split(new[] { ' ', '\t', '\n', '\r', '.', ',', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);

            // Count word frequency
            var wordFrequency = words
                .Where(w => w.Length > 3) // Only consider words longer than 3 characters
                .GroupBy(w => w.ToLowerInvariant())
                .Select(g => new { Word = g.Key, Count = g.Count() })
                .OrderByDescending(w => w.Count)
                .Take(10) // Take top 10 most frequent words
                .Select(w => w.Word)
                .ToArray();

            return wordFrequency;
        }
    }
}
