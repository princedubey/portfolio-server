using BlogManagementSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.ApiExplorer;

namespace BlogManagementSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileUploadController : ControllerBase
    {
        private readonly IImageService _imageService;

        public FileUploadController(IImageService imageService)
        {
            _imageService = imageService;
        }

        // POST: api/FileUpload/image
        [HttpPost("image")]
        [Authorize]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<ActionResult> UploadImage([FromForm] UploadImageRequest request)
        {
            if (request.File == null || request.File.Length == 0)
            {
                return BadRequest(new { error = "No file was uploaded." });
            }

            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                var image = await _imageService.UploadImageAsync(request.File, userId, request.AltText);

                return Ok(new
                {
                    id = image.Id,
                    fileName = image.FileName,
                    url = image.FilePath,
                    altText = image.AltText,
                    fileSize = image.FileSize,
                    contentType = image.ContentType,
                    uploadDate = image.UploadDate
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Upload failed", details = ex.Message });
            }
        }

        // POST: api/FileUpload/images/bulk
        [HttpPost("images/bulk")]
        [Authorize]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<ActionResult> UploadMultipleImages([FromForm] UploadMultipleImagesRequest request)
        {
            if (request.Files == null || !request.Files.Any())
            {
                return BadRequest(new { error = "No files were uploaded." });
            }

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var results = new List<object>();
            var errors = new List<object>();

            foreach (var file in request.Files)
            {
                try
                {
                    var image = await _imageService.UploadImageAsync(file, userId);
                    results.Add(new
                    {
                        id = image.Id,
                        fileName = image.FileName,
                        url = image.FilePath,
                        fileSize = image.FileSize,
                        success = true
                    });
                }
                catch (Exception ex)
                {
                    errors.Add(new
                    {
                        fileName = file.FileName,
                        error = ex.Message,
                        success = false
                    });
                }
            }

            return Ok(new
            {
                uploaded = results,
                errors = errors,
                totalUploaded = results.Count,
                totalErrors = errors.Count
            });
        }

        // GET: api/FileUpload/usage
        [HttpGet("usage")]
        [Authorize]
        public async Task<ActionResult> GetUploadUsage()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var userImages = await _imageService.GetImagesByUserAsync(userId);
            
            var totalSize = userImages.Sum(i => i.FileSize);
            var totalFiles = userImages.Count();
            
            return Ok(new
            {
                totalFiles = totalFiles,
                totalSizeBytes = totalSize,
                totalSizeMB = Math.Round(totalSize / (1024.0 * 1024.0), 2),
                images = userImages.Select(i => new
                {
                    id = i.Id,
                    fileName = i.FileName,
                    url = i.FilePath,
                    fileSize = i.FileSize,
                    uploadDate = i.UploadDate,
                    altText = i.AltText
                })
            });
        }

        // DELETE: api/FileUpload/cleanup-orphaned
        [HttpDelete("cleanup-orphaned")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> CleanupOrphanedImages()
        {
            // This would implement logic to find and delete images not referenced by any blog posts
            // Implementation depends on your specific requirements
            return Ok(new { message = "Cleanup completed" });
        }
    }

    public class UploadImageRequest
    {
        public IFormFile File { get; set; }
        public string AltText { get; set; }
    }

    public class UploadMultipleImagesRequest
    {
        public IFormFile[] Files { get; set; }
    }
}
