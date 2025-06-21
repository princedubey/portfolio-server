using BlogManagementSystem.Models;
using BlogManagementSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlogManagementSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BlogPostsController : ControllerBase
    {
        private readonly IBlogPostService _blogPostService;
        private readonly ISeoService _seoService;

        public BlogPostsController(IBlogPostService blogPostService, ISeoService seoService)
        {
            _blogPostService = blogPostService;
            _seoService = seoService;
        }

        // GET: api/BlogPosts/health
        [HttpGet("health")]
        public ActionResult<string> Health()
        {
            return Ok("Blog Posts API is running!");
        }

        // GET: api/BlogPosts
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BlogPost>>> GetBlogPosts()
        {
            var blogPosts = await _blogPostService.GetPublishedBlogPostsAsync();
            return Ok(blogPosts);
        }

        // GET: api/BlogPosts/all
        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<BlogPost>>> GetAllBlogPosts()
        {
            var blogPosts = await _blogPostService.GetAllBlogPostsAsync();
            return Ok(blogPosts);
        }

        // GET: api/BlogPosts/featured
        [HttpGet("featured")]
        public async Task<ActionResult<IEnumerable<BlogPost>>> GetFeaturedBlogPosts()
        {
            var blogPosts = await _blogPostService.GetFeaturedBlogPostsAsync();
            return Ok(blogPosts);
        }

        // GET: api/BlogPosts/5
        [HttpGet("{id}")]
        public async Task<ActionResult<BlogPost>> GetBlogPost(int id)
        {
            var blogPost = await _blogPostService.GetBlogPostByIdAsync(id);

            if (blogPost == null)
            {
                return NotFound();
            }

            return Ok(blogPost);
        }

        // GET: api/BlogPosts/slug/my-blog-post
        [HttpGet("slug/{slug}")]
        public async Task<ActionResult<BlogPost>> GetBlogPostBySlug(string slug)
        {
            var blogPost = await _blogPostService.GetBlogPostBySlugAsync(slug);

            if (blogPost == null)
            {
                return NotFound();
            }

            return Ok(blogPost);
        }

        // GET: api/BlogPosts/category/5
        [HttpGet("category/{categoryId}")]
        public async Task<ActionResult<IEnumerable<BlogPost>>> GetBlogPostsByCategory(int categoryId)
        {
            var blogPosts = await _blogPostService.GetBlogPostsByCategoryAsync(categoryId);
            return Ok(blogPosts);
        }

        // GET: api/BlogPosts/tag/5
        [HttpGet("tag/{tagId}")]
        public async Task<ActionResult<IEnumerable<BlogPost>>> GetBlogPostsByTag(int tagId)
        {
            var blogPosts = await _blogPostService.GetBlogPostsByTagAsync(tagId);
            return Ok(blogPosts);
        }

        // GET: api/BlogPosts/search?q=keyword
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<BlogPost>>> SearchBlogPosts([FromQuery] string q)
        {
            var blogPosts = await _blogPostService.SearchBlogPostsAsync(q);
            return Ok(blogPosts);
        }

        // POST: api/BlogPosts
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<BlogPost>> CreateBlogPost(BlogPost blogPost)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var createdBlogPost = await _blogPostService.CreateBlogPostAsync(blogPost);
            return CreatedAtAction(nameof(GetBlogPost), new { id = createdBlogPost.Id }, createdBlogPost);
        }

        // PUT: api/BlogPosts/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateBlogPost(int id, BlogPost blogPost)
        {
            if (id != blogPost.Id)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var updatedBlogPost = await _blogPostService.UpdateBlogPostAsync(blogPost);
            if (updatedBlogPost == null)
            {
                return NotFound();
            }

            return NoContent();
        }

        // DELETE: api/BlogPosts/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteBlogPost(int id)
        {
            var result = await _blogPostService.DeleteBlogPostAsync(id);
            if (!result)
            {
                return NotFound();
            }

            return NoContent();
        }

        // POST: api/BlogPosts/5/publish
        [HttpPost("{id}/publish")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PublishBlogPost(int id)
        {
            var result = await _blogPostService.PublishBlogPostAsync(id);
            if (!result)
            {
                return NotFound();
            }

            return NoContent();
        }

        // POST: api/BlogPosts/5/unpublish
        [HttpPost("{id}/unpublish")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UnpublishBlogPost(int id)
        {
            var result = await _blogPostService.UnpublishBlogPostAsync(id);
            if (!result)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}
