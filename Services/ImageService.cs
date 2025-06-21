using BlogManagementSystem.Data;
using BlogManagementSystem.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace BlogManagementSystem.Services
{
    public class ImageService : IImageService
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly IConfiguration _configuration;

        public ImageService(ApplicationDbContext context, IWebHostEnvironment environment, IConfiguration configuration)
        {
            _context = context;
            _environment = environment;
            _configuration = configuration;
        }

        public async Task<Image> UploadImageAsync(IFormFile file, int userId, string altText = null)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("File is empty or null", nameof(file));
            }

            // Generate a unique filename
            var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
            
            // Create directory if it doesn't exist
            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "images");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            // Save the file
            var filePath = Path.Combine(uploadsFolder, fileName);
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            // Create image record
            var image = new Image
            {
                FileName = fileName,
                FilePath = $"/uploads/images/{fileName}",
                ContentType = file.ContentType,
                FileSize = file.Length,
                UploadDate = DateTime.UtcNow,
                AltText = altText,
                UploadedById = userId
            };

            _context.Images.Add(image);
            await _context.SaveChangesAsync();
            return image;
        }

        public async Task<Image?> GetImageByIdAsync(int id)
        {
            return await _context.Images.FindAsync(id);
        }

        public async Task<IEnumerable<Image>> GetImagesByUserAsync(int userId)
        {
            return await _context.Images
                .Where(i => i.UploadedById == userId)
                .OrderByDescending(i => i.UploadDate)
                .ToListAsync();
        }

        public async Task<bool> DeleteImageAsync(int id)
        {
            var image = await _context.Images.FindAsync(id);
            if (image == null)
            {
                return false;
            }

            // Delete the physical file
            var filePath = Path.Combine(_environment.WebRootPath, image.FilePath.TrimStart('/'));
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            // Delete the database record
            _context.Images.Remove(image);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<string?> GetImageUrlAsync(int id)
        {
            var image = await _context.Images.FindAsync(id);
            return image?.FilePath;
        }

        public async Task<IEnumerable<Image>> GetAllImagesAsync()
        {
            return await _context.Images.ToListAsync();
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
    }
}
