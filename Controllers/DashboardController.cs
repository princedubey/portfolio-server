using BlogManagementSystem.Services;
using BlogManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlogManagementSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class DashboardController : ControllerBase
    {
        private readonly IBlogPostService _blogPostService;
        private readonly ICommentService _commentService;
        private readonly IUserService _userService;
        private readonly ICategoryService _categoryService;
        private readonly ITagService _tagService;

        public DashboardController(
            IBlogPostService blogPostService,
            ICommentService commentService,
            IUserService userService,
            ICategoryService categoryService,
            ITagService tagService)
        {
            _blogPostService = blogPostService;
            _commentService = commentService;
            _userService = userService;
            _categoryService = categoryService;
            _tagService = tagService;
        }

        // GET: api/Dashboard/stats
        [HttpGet("stats")]
        public async Task<ActionResult<DashboardStats>> GetDashboardStats()
        {
            var stats = new DashboardStats
            {
                TotalPosts = await _blogPostService.GetTotalPostsCountAsync(),
                PublishedPosts = await _blogPostService.GetPublishedPostsCountAsync(),
                DraftPosts = await _blogPostService.GetDraftPostsCountAsync(),
                TotalComments = await _commentService.GetTotalCommentsCountAsync(),
                PendingComments = await _commentService.GetPendingCommentsCountAsync(),
                ApprovedComments = await _commentService.GetApprovedCommentsCountAsync(),
                TotalUsers = await _userService.GetTotalUsersCountAsync(),
                TotalCategories = await _categoryService.GetTotalCategoriesCountAsync(),
                TotalTags = await _tagService.GetTotalTagsCountAsync()
            };

            return Ok(stats);
        }

        // GET: api/Dashboard/recent-posts
        [HttpGet("recent-posts")]
        public async Task<ActionResult<IEnumerable<BlogPost>>> GetRecentPosts([FromQuery] int count = 5)
        {
            var posts = await _blogPostService.GetRecentPostsAsync(count);
            return Ok(posts);
        }

        // GET: api/Dashboard/recent-comments
        [HttpGet("recent-comments")]
        public async Task<ActionResult<IEnumerable<Comment>>> GetRecentComments([FromQuery] int count = 5)
        {
            var comments = await _commentService.GetRecentCommentsAsync(count);
            return Ok(comments);
        }

        // GET: api/Dashboard/popular-posts
        [HttpGet("popular-posts")]
        public async Task<ActionResult<IEnumerable<BlogPost>>> GetPopularPosts([FromQuery] int count = 5)
        {
            var posts = await _blogPostService.GetPopularPostsAsync(count);
            return Ok(posts);
        }

        // GET: api/Dashboard/analytics
        [HttpGet("analytics")]
        public async Task<ActionResult<AnalyticsData>> GetAnalytics([FromQuery] int days = 30)
        {
            var analytics = new AnalyticsData
            {
                PostsCreatedInPeriod = await _blogPostService.GetPostsCreatedInLastDaysAsync(days),
                CommentsInPeriod = await _commentService.GetCommentsInLastDaysAsync(days),
                UsersRegisteredInPeriod = await _userService.GetUsersRegisteredInLastDaysAsync(days),
                PostsByCategory = (await _blogPostService.GetPostsByCategoryStatsAsync()).ToDictionary(x => x.CategoryName, x => x.PostCount),
                PostsByMonth = (await _blogPostService.GetPostsByMonthStatsAsync()).ToDictionary(x => x.Month, x => x.PostCount)
            };

            return Ok(analytics);
        }

        // GET: api/Dashboard/system-info
        [HttpGet("system-info")]
        public ActionResult<SystemInfo> GetSystemInfo()
        {
            var systemInfo = new SystemInfo
            {
                ServerTime = DateTime.UtcNow,
                Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production",
                Version = "1.0.0",
                DatabaseProvider = "PostgreSQL"
            };

            return Ok(systemInfo);
        }
    }

    public class DashboardStats
    {
        public int TotalPosts { get; set; }
        public int PublishedPosts { get; set; }
        public int DraftPosts { get; set; }
        public int TotalComments { get; set; }
        public int PendingComments { get; set; }
        public int ApprovedComments { get; set; }
        public int TotalUsers { get; set; }
        public int TotalCategories { get; set; }
        public int TotalTags { get; set; }
    }

    public class AnalyticsData
    {
        public int PostsCreatedInPeriod { get; set; }
        public int CommentsInPeriod { get; set; }
        public int UsersRegisteredInPeriod { get; set; }
        public Dictionary<string, int> PostsByCategory { get; set; }
        public Dictionary<string, int> PostsByMonth { get; set; }
    }

    public class SystemInfo
    {
        public DateTime ServerTime { get; set; }
        public string Environment { get; set; }
        public string Version { get; set; }
        public string DatabaseProvider { get; set; }
    }
}
