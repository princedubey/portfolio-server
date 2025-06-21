using BlogManagementSystem.Data;
using BlogManagementSystem.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Net.Http.Headers;

namespace BlogManagementSystem.Services
{
    public class UploadThingImageService : IImageService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _apiUrl;

        public UploadThingImageService(ApplicationDbContext context, IConfiguration configuration, HttpClient httpClient)
        {
            _context = context;
            _configuration = configuration;
            _httpClient = httpClient;
            _apiKey = _configuration["UploadThing:ApiKey"] ?? throw new ArgumentNullException("UploadThing:ApiKey");
            _apiUrl = _configuration["UploadThing:ApiUrl"] ?? throw new ArgumentNullException("UploadThing:ApiUrl");
        }

        public async Task<Image?> GetImageByIdAsync(int id)
        {
            return await _context.Images.FindAsync(id);
        }

        public async Task<IEnumerable<Image>> GetAllImagesAsync()
        {
            return await _context.Images.ToListAsync();
        }

        public async Task<Image> CreateImageAsync(Image image)
        {
            _context.Images.Add(image);
            await _context.SaveChangesAsync();
            return image;
        }

        public async Task<Image?> UpdateImageAsync(int id, Image image)
        {
            var existingImage = await _context.Images.FindAsync(id);
            if (existingImage == null)
                return null;

            existingImage.FileName = image.FileName;
            existingImage.FilePath = image.FilePath;
            existingImage.AltText = image.AltText;
            existingImage.Description = image.Description;
            existingImage.ContentType = image.ContentType;
            existingImage.FileSize = image.FileSize;
            existingImage.Width = image.Width;
            existingImage.Height = image.Height;
            existingImage.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return existingImage;
        }

        public async Task<bool> DeleteImageAsync(int id)
        {
            var image = await _context.Images.FindAsync(id);
            if (image == null)
                return false;

            // Delete from UploadThing
            var deleteUrl = $"{_apiUrl}/files/{image.FileName}";
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
            var response = await _httpClient.DeleteAsync(deleteUrl);

            if (!response.IsSuccessStatusCode)
                return false;

            // Delete from database
            _context.Images.Remove(image);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<string?> GetImageUrlAsync(int id)
        {
            var image = await _context.Images.FindAsync(id);
            return image?.FilePath;
        }

        public async Task<bool> ImageExistsAsync(int id)
        {
            return await _context.Images.AnyAsync(i => i.Id == id);
        }

        public async Task<IEnumerable<Image>> GetImagesByPostIdAsync(int postId)
        {
            return await _context.Images
                .Where(i => i.BlogPostId == postId)
                .OrderByDescending(i => i.CreatedAt)
                .ToListAsync();
        }

        public async Task<int> GetTotalImagesCountAsync()
        {
            return await _context.Images.CountAsync();
        }

        public async Task<int> GetImagesUploadedInLastDaysAsync(int days)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-days);
            return await _context.Images
                .CountAsync(i => i.CreatedAt >= cutoffDate);
        }

        public async Task<IEnumerable<Image>> GetRecentImagesAsync(int count)
        {
            return await _context.Images
                .OrderByDescending(i => i.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task<IEnumerable<Image>> GetPopularImagesAsync(int count)
        {
            return await _context.Images
                .OrderByDescending(i => i.ViewCount)
                .Take(count)
                .ToListAsync();
        }

        public async Task<Image> UploadImageAsync(IFormFile file, int userId, string? altText = null)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File is empty", nameof(file));

            // Upload to UploadThing
            var uploadUrl = $"{_apiUrl}/upload";
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

            using var content = new MultipartFormDataContent();
            using var fileStream = file.OpenReadStream();
            using var streamContent = new StreamContent(fileStream);
            streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
            content.Add(streamContent, "file", file.FileName);

            var response = await _httpClient.PostAsync(uploadUrl, content);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var uploadResponse = JsonSerializer.Deserialize<UploadThingResponse>(responseContent);

            if (uploadResponse == null || string.IsNullOrEmpty(uploadResponse.Url))
                throw new Exception("Failed to upload image to UploadThing");

            // Create image record
            var image = new Image
            {
                FileName = file.FileName,
                FilePath = uploadResponse.Url,
                AltText = altText,
                ContentType = file.ContentType,
                FileSize = file.Length,
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Images.Add(image);
            await _context.SaveChangesAsync();

            return image;
        }

        public async Task<IEnumerable<Image>> GetImagesByUserAsync(int userId)
        {
            return await _context.Images.Where(i => i.UserId == userId).OrderByDescending(i => i.CreatedAt).ToListAsync();
        }

        public async Task<bool> UpdateImageMetadataAsync(int id, string altText, string? fileName = null)
        {
            var image = await _context.Images.FindAsync(id);
            if (image == null) return false;
            image.AltText = altText;
            if (!string.IsNullOrEmpty(fileName))
                image.FileName = fileName;
            image.UpdatedAt = DateTime.UtcNow;
            _context.Entry(image).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            await _context.SaveChangesAsync();
            return true;
        }

        private class UploadThingResponse
        {
            public string? Url { get; set; }
        }
    }
}
