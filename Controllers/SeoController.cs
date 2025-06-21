using BlogManagementSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlogManagementSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SeoController : ControllerBase
    {
        private readonly ISeoService _seoService;
        private readonly IBlogPostService _blogPostService;
        private readonly ICategoryService _categoryService;
        private readonly ITagService _tagService;

        public SeoController(
            ISeoService seoService,
            IBlogPostService blogPostService,
            ICategoryService categoryService,
            ITagService tagService)
        {
            _seoService = seoService;
            _blogPostService = blogPostService;
            _categoryService = categoryService;
            _tagService = tagService;
        }

        // GET: api/Seo/sitemap
        [HttpGet("sitemap")]
        public async Task<ActionResult> GetSitemap()
        {
            var sitemap = await _seoService.GenerateSitemapAsync();
            return Content(sitemap, "application/xml");
        }

        // GET: api/Seo/robots
        [HttpGet("robots")]
        public async Task<ActionResult> GetRobotsTxt()
        {
            var robotsTxt = await _seoService.GenerateRobotsTxt();
            return Content(robotsTxt, "text/plain");
        }

        // GET: api/Seo/post/5/structured-data
        [HttpGet("post/{id}/structured-data")]
        public async Task<ActionResult<string>> GetPostStructuredData(int id)
        {
            var blogPost = await _blogPostService.GetBlogPostByIdAsync(id);
            if (blogPost == null)
            {
                return NotFound();
            }

            var structuredData = _seoService.GenerateStructuredData(blogPost);
            return Ok(structuredData);
        }

        // POST: api/Seo/generate-slug
        [HttpPost("generate-slug")]
        [Authorize(Roles = "Admin")]
        public ActionResult<string> GenerateSlug([FromBody] string title)
        {
            if (string.IsNullOrEmpty(title))
            {
                return BadRequest("Title is required");
            }

            var slug = _seoService.GenerateSlug(title);
            return Ok(new { slug });
        }

        // POST: api/Seo/generate-excerpt
        [HttpPost("generate-excerpt")]
        [Authorize(Roles = "Admin")]
        public ActionResult<string> GenerateExcerpt([FromBody] GenerateExcerptModel model)
        {
            if (string.IsNullOrEmpty(model.Content))
            {
                return BadRequest("Content is required");
            }

            var excerpt = _seoService.GenerateExcerpt(model.Content, model.MaxLength);
            return Ok(new { excerpt });
        }

        // POST: api/Seo/generate-meta-description
        [HttpPost("generate-meta-description")]
        [Authorize(Roles = "Admin")]
        public ActionResult<string> GenerateMetaDescription([FromBody] GenerateMetaDescriptionModel model)
        {
            if (string.IsNullOrEmpty(model.Content))
            {
                return BadRequest("Content is required");
            }

            var metaDescription = _seoService.GenerateMetaDescription(model.Content, model.MaxLength);
            return Ok(new { metaDescription });
        }

        // GET: api/Seo/canonical-url/{slug}
        [HttpGet("canonical-url/{slug}")]
        public ActionResult<string> GetCanonicalUrl(string slug)
        {
            var canonicalUrl = _seoService.GenerateCanonicalUrl(slug);
            return Ok(new { canonicalUrl });
        }

        // GET: api/Seo/analysis/post/{id}
        [HttpGet("analysis/post/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<SeoAnalysis>> AnalyzePost(int id)
        {
            var blogPost = await _blogPostService.GetBlogPostByIdAsync(id);
            if (blogPost == null)
            {
                return NotFound();
            }

            var analysis = await _seoService.AnalyzePost(blogPost);
            return Ok(analysis);
        }

        // POST: api/Seo/bulk-update-slugs
        [HttpPost("bulk-update-slugs")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> BulkUpdateSlugs()
        {
            var result = await _seoService.BulkUpdateSlugsAsync();
            return Ok(new { UpdatedCount = result });
        }
    }

    public class GenerateExcerptModel
    {
        public string Content { get; set; }
        public int MaxLength { get; set; } = 300;
    }

    public class GenerateMetaDescriptionModel
    {
        public string Content { get; set; }
        public int MaxLength { get; set; } = 160;
    }

    public class SeoAnalysis
    {
        public bool HasTitle { get; set; }
        public bool HasMetaDescription { get; set; }
        public bool HasFeaturedImage { get; set; }
        public bool HasExcerpt { get; set; }
        public bool HasSlug { get; set; }
        public int TitleLength { get; set; }
        public int MetaDescriptionLength { get; set; }
        public int ContentLength { get; set; }
        public int WordCount { get; set; }
        public List<string> Recommendations { get; set; }
        public int SeoScore { get; set; }
    }
}
