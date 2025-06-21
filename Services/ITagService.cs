using BlogManagementSystem.Models;

namespace BlogManagementSystem.Services
{
    public interface ITagService
    {
        Task<IEnumerable<Tag>> GetAllTagsAsync();
        Task<Tag> GetTagByIdAsync(int id);
        Task<Tag> GetTagBySlugAsync(string slug);
        Task<IEnumerable<BlogPost>> GetTagPostsAsync(int tagId);
        Task<IEnumerable<Tag>> GetPopularTagsAsync(int count);
        Task<Tag> CreateTagAsync(Tag tag);
        Task<IEnumerable<Tag>> CreateTagsBulkAsync(string[] tagNames);
        Task<Tag> UpdateTagAsync(Tag tag);
        Task<bool> DeleteTagAsync(int id);
        Task<int> GetTotalTagsCountAsync();
    }
}
