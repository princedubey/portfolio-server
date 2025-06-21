using BlogManagementSystem.Models;
using BlogManagementSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlogManagementSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TagsController : ControllerBase
    {
        private readonly ITagService _tagService;

        public TagsController(ITagService tagService)
        {
            _tagService = tagService;
        }

        // GET: api/Tags
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Tag>>> GetTags()
        {
            var tags = await _tagService.GetAllTagsAsync();
            return Ok(tags);
        }

        // GET: api/Tags/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Tag>> GetTag(int id)
        {
            var tag = await _tagService.GetTagByIdAsync(id);

            if (tag == null)
            {
                return NotFound();
            }

            return Ok(tag);
        }

        // GET: api/Tags/slug/csharp
        [HttpGet("slug/{slug}")]
        public async Task<ActionResult<Tag>> GetTagBySlug(string slug)
        {
            var tag = await _tagService.GetTagBySlugAsync(slug);

            if (tag == null)
            {
                return NotFound();
            }

            return Ok(tag);
        }

        // GET: api/Tags/5/posts
        [HttpGet("{id}/posts")]
        public async Task<ActionResult<IEnumerable<BlogPost>>> GetTagPosts(int id)
        {
            var posts = await _tagService.GetTagPostsAsync(id);
            return Ok(posts);
        }

        // GET: api/Tags/popular
        [HttpGet("popular")]
        public async Task<ActionResult<IEnumerable<Tag>>> GetPopularTags([FromQuery] int count = 10)
        {
            var tags = await _tagService.GetPopularTagsAsync(count);
            return Ok(tags);
        }

        // POST: api/Tags
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Tag>> CreateTag(Tag tag)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var createdTag = await _tagService.CreateTagAsync(tag);
            return CreatedAtAction(nameof(GetTag), new { id = createdTag.Id }, createdTag);
        }

        // PUT: api/Tags/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateTag(int id, Tag tag)
        {
            if (id != tag.Id)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var updatedTag = await _tagService.UpdateTagAsync(tag);
            if (updatedTag == null)
            {
                return NotFound();
            }

            return NoContent();
        }

        // DELETE: api/Tags/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteTag(int id)
        {
            var result = await _tagService.DeleteTagAsync(id);
            if (!result)
            {
                return NotFound();
            }

            return NoContent();
        }

        // POST: api/Tags/bulk
        [HttpPost("bulk")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<Tag>>> CreateTagsBulk([FromBody] string[] tagNames)
        {
            if (tagNames == null || tagNames.Length == 0)
            {
                return BadRequest("Tag names are required");
            }

            var createdTags = await _tagService.CreateTagsBulkAsync(tagNames);
            return Ok(createdTags);
        }
    }
}
