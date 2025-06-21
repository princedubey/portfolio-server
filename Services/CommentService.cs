using BlogManagementSystem.Data;
using BlogManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace BlogManagementSystem.Services
{
    public class CommentService : ICommentService
    {
        private readonly ApplicationDbContext _context;

        public CommentService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Comment>> GetAllCommentsAsync()
        {
            return await _context.Comments.ToListAsync();
        }

        public async Task<IEnumerable<Comment>> GetApprovedCommentsByPostAsync(int blogPostId)
        {
            return await _context.Comments.Where(c => c.BlogPostId == blogPostId && c.IsApproved).ToListAsync();
        }

        public async Task<IEnumerable<Comment>> GetPendingCommentsAsync()
        {
            return await _context.Comments.Where(c => !c.IsApproved).ToListAsync();
        }

        public async Task<Comment> GetCommentByIdAsync(int id)
        {
            return await _context.Comments.FindAsync(id);
        }

        public async Task<Comment> CreateCommentAsync(Comment comment)
        {
            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();
            return comment;
        }

        public async Task<Comment> UpdateCommentAsync(Comment comment)
        {
            _context.Entry(comment).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return comment;
        }

        public async Task<bool> DeleteCommentAsync(int id)
        {
            var comment = await _context.Comments.FindAsync(id);
            if (comment == null) return false;
            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ApproveCommentAsync(int id)
        {
            var comment = await _context.Comments.FindAsync(id);
            if (comment == null) return false;
            comment.IsApproved = true;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RejectCommentAsync(int id)
        {
            var comment = await _context.Comments.FindAsync(id);
            if (comment == null) return false;
            comment.IsApproved = false;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> BulkApproveCommentsAsync(int[] commentIds)
        {
            var comments = await _context.Comments.Where(c => commentIds.Contains(c.Id)).ToListAsync();
            foreach (var comment in comments) comment.IsApproved = true;
            await _context.SaveChangesAsync();
            return comments.Count;
        }

        public async Task<int> BulkRejectCommentsAsync(int[] commentIds)
        {
            var comments = await _context.Comments.Where(c => commentIds.Contains(c.Id)).ToListAsync();
            foreach (var comment in comments) comment.IsApproved = false;
            await _context.SaveChangesAsync();
            return comments.Count;
        }

        public async Task<int> GetTotalCommentsCountAsync()
        {
            return await _context.Comments.CountAsync();
        }

        public async Task<int> GetPendingCommentsCountAsync()
        {
            return await _context.Comments.CountAsync(c => !c.IsApproved);
        }

        public async Task<int> GetApprovedCommentsCountAsync()
        {
            return await _context.Comments.CountAsync(c => c.IsApproved);
        }

        public async Task<IEnumerable<Comment>> GetRecentCommentsAsync(int count)
        {
            return await _context.Comments.OrderByDescending(c => c.CreatedDate).Take(count).ToListAsync();
        }

        public async Task<int> GetCommentsInLastDaysAsync(int days)
        {
            var cutoff = DateTime.UtcNow.AddDays(-days);
            return await _context.Comments.CountAsync(c => c.CreatedDate >= cutoff);
        }
    }
} 