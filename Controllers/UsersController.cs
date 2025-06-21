using BlogManagementSystem.Models;
using BlogManagementSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BlogManagementSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        // GET: api/Users
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            var userDtos = users.Select(u => new UserDto
            {
                Id = u.Id,
                Username = u.Username,
                Email = u.Email,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Bio = u.Bio,
                ProfileImageUrl = u.ProfileImageUrl,
                CreatedDate = u.CreatedDate,
                LastLoginDate = u.LastLoginDate,
                IsAdmin = u.IsAdmin
            });
            return Ok(userDtos);
        }

        // GET: api/Users/5
        [HttpGet("{id}")]
        public async Task<ActionResult<UserDto>> GetUser(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            var userDto = new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Bio = user.Bio,
                ProfileImageUrl = user.ProfileImageUrl,
                CreatedDate = user.CreatedDate,
                LastLoginDate = user.LastLoginDate,
                IsAdmin = user.IsAdmin
            };

            return Ok(userDto);
        }

        // GET: api/Users/profile
        [HttpGet("profile")]
        [Authorize]
        public async Task<ActionResult<UserDto>> GetCurrentUserProfile()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var user = await _userService.GetUserByIdAsync(userId);

            if (user == null)
            {
                return NotFound();
            }

            var userDto = new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Bio = user.Bio,
                ProfileImageUrl = user.ProfileImageUrl,
                CreatedDate = user.CreatedDate,
                LastLoginDate = user.LastLoginDate,
                IsAdmin = user.IsAdmin
            };

            return Ok(userDto);
        }

        // PUT: api/Users/5
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateUser(int id, UpdateUserModel model)
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var isAdmin = User.IsInRole("Admin");

            // Users can only update their own profile unless they're admin
            if (currentUserId != id && !isAdmin)
            {
                return Forbid();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // Update user properties
            user.Username = model.Username;
            user.Email = model.Email;
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Bio = model.Bio;
            user.ProfileImageUrl = model.ProfileImageUrl;

            // Only admins can change admin status
            if (isAdmin && model.IsAdmin.HasValue)
            {
                user.IsAdmin = model.IsAdmin.Value;
            }

            var updatedUser = await _userService.UpdateUserAsync(user);
            if (updatedUser == null)
            {
                return NotFound();
            }

            return NoContent();
        }

        // DELETE: api/Users/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var result = await _userService.DeleteUserAsync(id);
            if (!result)
            {
                return NotFound();
            }

            return NoContent();
        }

        // POST: api/Users/change-password
        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword(ChangePasswordModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var result = await _userService.ChangePasswordAsync(userId, model.CurrentPassword, model.NewPassword);

            if (!result)
            {
                return BadRequest("Current password is incorrect");
            }

            return NoContent();
        }

        // GET: api/Users/5/posts
        [HttpGet("{id}/posts")]
        public async Task<ActionResult<IEnumerable<BlogPost>>> GetUserPosts(int id)
        {
            var posts = await _userService.GetUserPostsAsync(id);
            return Ok(posts);
        }

        // GET: api/Users/5/comments
        [HttpGet("{id}/comments")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<Comment>>> GetUserComments(int id)
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var isAdmin = User.IsInRole("Admin");

            // Users can only view their own comments unless they're admin
            if (currentUserId != id && !isAdmin)
            {
                return Forbid();
            }

            var comments = await _userService.GetUserCommentsAsync(id);
            return Ok(comments);
        }

        // POST: api/Users/5/make-admin
        [HttpPost("{id}/make-admin")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> MakeUserAdmin(int id)
        {
            var result = await _userService.MakeUserAdminAsync(id);
            if (!result)
            {
                return NotFound();
            }

            return NoContent();
        }

        // POST: api/Users/5/remove-admin
        [HttpPost("{id}/remove-admin")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RemoveUserAdmin(int id)
        {
            var result = await _userService.RemoveUserAdminAsync(id);
            if (!result)
            {
                return NotFound();
            }

            return NoContent();
        }
    }

    public class UserDto
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Bio { get; set; }
        public string ProfileImageUrl { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? LastLoginDate { get; set; }
        public bool IsAdmin { get; set; }
    }

    public class UpdateUserModel
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Bio { get; set; }
        public string ProfileImageUrl { get; set; }
        public bool? IsAdmin { get; set; }
    }

    public class ChangePasswordModel
    {
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }
    }
}
