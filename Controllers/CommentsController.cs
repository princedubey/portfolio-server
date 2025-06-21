using BlogManagementSystem.Models;
using BlogManagementSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BlogManagementSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommentsController : ControllerBase
    {
        private readonly ICommentService _commentService;

        public CommentsController(ICommentService commentService)
        {
            _commentService = commentService;
        }

        // GET: api/Comments/post/5
        [HttpGet("post/{blogPostId}")]
        public async Task<ActionResult<IEnumerable<Comment>>> GetCommentsByPost(int blogPostId)
        {
            var comments = await _commentService.GetApprovedCommentsByPostAsync(blogPostId);
            return Ok(comments);
        }

        // GET: api/Comments/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Comment>> GetComment(int id)
        {
            var comment = await _commentService.GetCommentByIdAsync(id);

            if (comment == null)
            {
                return NotFound();
            }

            return Ok(comment);
        }

        // GET: api/Comments/pending
        [HttpGet("pending")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<Comment>>> GetPendingComments()
        {
            var comments = await _commentService.GetPendingCommentsAsync();
            return Ok(comments);
        }

        // GET: api/Comments/all
        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<Comment>>> GetAllComments()
        {
            var comments = await _commentService.GetAllCommentsAsync();
            return Ok(comments);
        }

        // POST: api/Comments
        [HttpPost]
        public async Task<ActionResult<Comment>> CreateComment(CreateCommentModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var comment = new Comment
            {
                Content = model.Content,
                BlogPostId = model.BlogPostId,
                CreatedDate = DateTime.UtcNow,
                IsApproved = false // Comments need approval by default
            };

            // Check if user is authenticated
            if (User.Identity.IsAuthenticated)
            {
                comment.UserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            }
            else
            {
                // Guest comment
                if (string.IsNullOrEmpty(model.GuestName) || string.IsNullOrEmpty(model.GuestEmail))
                {
                    return BadRequest("Guest name and email are required for anonymous comments");
                }
                comment.GuestName = model.GuestName;
                comment.GuestEmail = model.GuestEmail;
            }

            var createdComment = await _commentService.CreateCommentAsync(comment);
            return CreatedAtAction(nameof(GetComment), new { id = createdComment.Id }, createdComment);
        }

        // PUT: api/Comments/5
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateComment(int id, UpdateCommentModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var comment = await _commentService.GetCommentByIdAsync(id);
            if (comment == null)
            {
                return NotFound();
            }

            // Check if user owns the comment or is admin
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var isAdmin = User.IsInRole("Admin");

            if (comment.UserId != currentUserId && !isAdmin)
            {
                return Forbid();
            }

            comment.Content = model.Content;
            comment.UpdatedDate = DateTime.UtcNow;

            var updatedComment = await _commentService.UpdateCommentAsync(comment);
            if (updatedComment == null)
            {
                return NotFound();
            }

            return NoContent();
        }

        // DELETE: api/Comments/5
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteComment(int id)
        {
            var comment = await _commentService.GetCommentByIdAsync(id);
            if (comment == null)
            {
                return NotFound();
            }

            // Check if user owns the comment or is admin
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var isAdmin = User.IsInRole("Admin");

            if (comment.UserId != currentUserId && !isAdmin)
            {
                return Forbid();
            }

            var result = await _commentService.DeleteCommentAsync(id);
            if (!result)
            {
                return NotFound();
            }

            return NoContent();
        }

        // POST: api/Comments/5/approve
        [HttpPost("{id}/approve")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ApproveComment(int id)
        {
            var result = await _commentService.ApproveCommentAsync(id);
            if (!result)
            {
                return NotFound();
            }

            return NoContent();
        }

        // POST: api/Comments/5/reject
        [HttpPost("{id}/reject")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RejectComment(int id)
        {
            var result = await _commentService.RejectCommentAsync(id);
            if (!result)
            {
                return NotFound();
            }

            return NoContent();
        }

        // POST: api/Comments/bulk-approve
        [HttpPost("bulk-approve")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> BulkApproveComments([FromBody] int[] commentIds)
        {
            if (commentIds == null || commentIds.Length == 0)
            {
                return BadRequest("Comment IDs are required");
            }

            var result = await _commentService.BulkApproveCommentsAsync(commentIds);
            return Ok(new { ApprovedCount = result });
        }

        // POST: api/Comments/bulk-reject
        [HttpPost("bulk-reject")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> BulkRejectComments([FromBody] int[] commentIds)
        {
            if (commentIds == null || commentIds.Length == 0)
            {
                return BadRequest("Comment IDs are required");
            }

            var result = await _commentService.BulkRejectCommentsAsync(commentIds);
            return Ok(new { RejectedCount = result });
        }
    }

    public class CreateCommentModel
    {
        public string Content { get; set; }
        public int BlogPostId { get; set; }
        public string GuestName { get; set; }
        public string GuestEmail { get; set; }
    }

    public class UpdateCommentModel
    {
        public string Content { get; set; }
    }
}
