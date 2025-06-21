using BlogManagementSystem.Models;
using Microsoft.AspNetCore.Http;

namespace BlogManagementSystem.Services
{
    public interface IImageService
    {
        Task<Image?> GetImageByIdAsync(int id);
        Task<IEnumerable<Image>> GetAllImagesAsync();
        Task<Image> CreateImageAsync(Image image);
        Task<Image?> UpdateImageAsync(int id, Image image);
        Task<bool> DeleteImageAsync(int id);
        Task<string?> GetImageUrlAsync(int id);
        Task<bool> ImageExistsAsync(int id);
        Task<IEnumerable<Image>> GetImagesByPostIdAsync(int postId);
        Task<int> GetTotalImagesCountAsync();
        Task<int> GetImagesUploadedInLastDaysAsync(int days);
        Task<IEnumerable<Image>> GetRecentImagesAsync(int count);
        Task<IEnumerable<Image>> GetPopularImagesAsync(int count);
        Task<Image> UploadImageAsync(Microsoft.AspNetCore.Http.IFormFile file, int userId, string? altText = null);
        Task<IEnumerable<Image>> GetImagesByUserAsync(int userId);
        Task<bool> UpdateImageMetadataAsync(int id, string altText, string? fileName = null);
    }
}
