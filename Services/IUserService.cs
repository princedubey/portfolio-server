using BlogManagementSystem.Models;

namespace BlogManagementSystem.Services
{
    public interface IUserService
    {
        Task<User> GetUserByIdAsync(int id);
        Task<User> GetUserByEmailAsync(string email);
        Task<User> GetUserByUsernameAsync(string username);
        Task<User> CreateUserAsync(User user, string password);
        Task<User> UpdateUserAsync(User user);
        Task<bool> DeleteUserAsync(int id);
        Task<User> AuthenticateAsync(string email, string password);
        Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword);
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<IEnumerable<BlogPost>> GetUserPostsAsync(int userId);
        Task<IEnumerable<Comment>> GetUserCommentsAsync(int userId);
        Task<bool> MakeUserAdminAsync(int userId);
        Task<bool> RemoveUserAdminAsync(int userId);
        Task<int> GetTotalUsersCountAsync();
        Task<int> GetUsersRegisteredInLastDaysAsync(int days);
    }
}
