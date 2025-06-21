using BlogManagementSystem.Data;
using BlogManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace BlogManagementSystem.Services
{
    public class TagService : ITagService
    {
        private readonly ApplicationDbContext _context;
        private readonly ISeoService _seoService;

        public TagService(ApplicationDbContext context, ISeoService seoService)
        {
            _context = context;
            _seoService = seoService;
        }

        public async Task<IEnumerable<Tag>> GetAllTagsAsync()
        {
            return await _context.Tags
                .Include(t => t.BlogPostTags)
                .OrderBy(t => t.Name)
                .ToListAsync();
        }

        public async Task<Tag> GetTagByIdAsync(int id)
        {
            return await _context.Tags
                .Include(t => t.BlogPostTags)
                    .ThenInclude(bpt => bpt.BlogPost)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<Tag> GetTagBySlugAsync(string slug)
        {
            return await _context.Tags
                .Include(t => t.BlogPostTags)
                    .ThenInclude(bpt => bpt.BlogPost)
                .FirstOrDefaultAsync(t => t.Slug == slug);
        }

        public async Task<IEnumerable<BlogPost>> GetTagPostsAsync(int tagId)
        {
            return await _context.BlogPosts
                .Where(bp => bp.BlogPostTags.Any(bpt => bpt.TagId == tagId) && bp.IsPublished)
                .Include(bp => bp.Author)
                .Include(bp => bp.Category)
                .Include(bp => bp.BlogPostTags)
                    .ThenInclude(bpt => bpt.Tag)
                .OrderByDescending(bp => bp.PublishedDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Tag>> GetPopularTagsAsync(int count)
        {
            return await _context.Tags
                .Include(t => t.BlogPostTags)
                .OrderByDescending(t => t.BlogPostTags.Count(bpt => bpt.BlogPost.IsPublished))
                .Take(count)
                .ToListAsync();
        }

        public async Task<Tag> CreateTagAsync(Tag tag)
        {
            tag.Slug = _seoService.GenerateSlug(tag.Name);
            
            _context.Tags.Add(tag);
            await _context.SaveChangesAsync();
            return tag;
        }

        public async Task<IEnumerable<Tag>> CreateTagsBulkAsync(string[] tagNames)
        {
            var createdTags = new List<Tag>();
            
            foreach (var tagName in tagNames)
            {
                var existingTag = await _context.Tags.FirstOrDefaultAsync(t => t.Name == tagName);
                if (existingTag == null)
                {
                    var tag = new Tag
                    {
                        Name = tagName,
                        Slug = _seoService.GenerateSlug(tagName)
                    };
                    _context.Tags.Add(tag);
                    createdTags.Add(tag);
                }
                else
                {
                    createdTags.Add(existingTag);
                }
            }
            
            await _context.SaveChangesAsync();
            return createdTags;
        }

        public async Task<Tag> UpdateTagAsync(Tag tag)
        {
            var existingTag = await _context.Tags.FindAsync(tag.Id);
            if (existingTag == null)
            {
                return null;
            }

            existingTag.Name = tag.Name;
            
            // Update slug if name changed
            if (existingTag.Name != tag.Name)
            {
                existingTag.Slug = _seoService.GenerateSlug(tag.Name);
            }

            _context.Entry(existingTag).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return existingTag;
        }

        public async Task<bool> DeleteTagAsync(int id)
        {
            var tag = await _context.Tags.FindAsync(id);
            if (tag == null)
            {
                return false;
            }

            _context.Tags.Remove(tag);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> GetTotalTagsCountAsync()
        {
            return await _context.Tags.CountAsync();
        }
    }
}
