using BlogManagementSystem.Models;
using BlogManagementSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BlogManagementSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImagesController : ControllerBase
    {
        private readonly IImageService _imageService;

        public ImagesController(IImageService imageService)
        {
            _imageService = imageService;
        }

        // GET: api/Images/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Image>> GetImage(int id)
        {
            var image = await _imageService.GetImageByIdAsync(id);
            if (image == null)
            {
                return NotFound();
            }

            return Ok(image);
        }

        // GET: api/Images/user/5
        [HttpGet("user/{userId}")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<Image>>> GetImagesByUser(int userId)
        {
            // Check if the user is requesting their own images or is an admin
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var isAdmin = User.IsInRole("Admin");

            if (currentUserId != userId && !isAdmin)
            {
                return Forbid();
            }

            var images = await _imageService.GetImagesByUserAsync(userId);
            return Ok(images);
        }

        // POST: api/Images
        [HttpPost]
        [Authorize]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<ActionResult<Image>> UploadImage([FromForm] IFormFile file, [FromForm] string altText)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file was uploaded.");
            }

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var image = await _imageService.UploadImageAsync(file, userId, altText);

            return CreatedAtAction(nameof(GetImage), new { id = image.Id }, image);
        }

        // DELETE: api/Images/5
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteImage(int id)
        {
            var image = await _imageService.GetImageByIdAsync(id);
            if (image == null)
            {
                return NotFound();
            }

            // Check if the user owns the image or is an admin
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var isAdmin = User.IsInRole("Admin");

            if (image.UploadedById != currentUserId && !isAdmin)
            {
                return Forbid();
            }

            var result = await _imageService.DeleteImageAsync(id);
            if (!result)
            {
                return NotFound();
            }

            return NoContent();
        }

        // GET: api/Images
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<Image>>> GetAllImages()
        {
            var images = await _imageService.GetAllImagesAsync();
            return Ok(images);
        }

        // PUT: api/Images/5/metadata
        [HttpPut("{id}/metadata")]
        [Authorize]
        public async Task<IActionResult> UpdateImageMetadata(int id, [FromBody] UpdateImageMetadataModel model)
        {
            var image = await _imageService.GetImageByIdAsync(id);
            if (image == null)
            {
                return NotFound();
            }

            // Check if the user owns the image or is an admin
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var isAdmin = User.IsInRole("Admin");

            if (image.UploadedById != currentUserId && !isAdmin)
            {
                return Forbid();
            }

            var result = await _imageService.UpdateImageMetadataAsync(id, model.AltText, model.FileName);
            if (!result)
            {
                return NotFound();
            }

            return NoContent();
        }
    }

    public class UpdateImageMetadataModel
    {
        public string AltText { get; set; }
        public string FileName { get; set; }
    }
}
