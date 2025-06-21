using BlogManagementSystem.Models;

namespace BlogManagementSystem.Services
{
    public interface ICommentService
    {
        Task<IEnumerable<Comment>> GetAllCommentsAsync();
        Task<IEnumerable<Comment>> GetApprovedCommentsByPostAsync(int blogPostId);
        Task<IEnumerable<Comment>> GetPendingCommentsAsync();
        Task<Comment> GetCommentByIdAsync(int id);
        Task<Comment> CreateCommentAsync(Comment comment);
        Task<Comment> UpdateCommentAsync(Comment comment);
        Task<bool> DeleteCommentAsync(int id);
        Task<bool> ApproveCommentAsync(int id);
        Task<bool> RejectCommentAsync(int id);
        Task<int> BulkApproveCommentsAsync(int[] commentIds);
        Task<int> BulkRejectCommentsAsync(int[] commentIds);
        Task<int> GetTotalCommentsCountAsync();
        Task<int> GetPendingCommentsCountAsync();
        Task<int> GetApprovedCommentsCountAsync();
        Task<IEnumerable<Comment>> GetRecentCommentsAsync(int count);
        Task<int> GetCommentsInLastDaysAsync(int days);
    }
}
