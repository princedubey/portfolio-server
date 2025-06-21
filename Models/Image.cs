using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlogManagementSystem.Models
{
    public class Image
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string FileName { get; set; }

        [Required]
        [MaxLength(500)] // Increased for UploadThing URLs
        public string FilePath { get; set; }

        [Required]
        [MaxLength(100)]
        public string ContentType { get; set; }

        public long FileSize { get; set; }

        [Required]
        public DateTime UploadDate { get; set; }

        [MaxLength(255)]
        public string Description { get; set; }

        [MaxLength(255)]
        public string AltText { get; set; }

        [ForeignKey("User")]
        public int UploadedById { get; set; }
        public User UploadedBy { get; set; }

        // UploadThing specific fields
        [MaxLength(255)]
        public string UploadThingKey { get; set; } // For deletion purposes

        [MaxLength(500)]
        public string ThumbnailUrl { get; set; } // UploadThing can provide thumbnails

        public int? Width { get; set; }
        public int? Height { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int? BlogPostId { get; set; }
        public int? UserId { get; set; }
        public int ViewCount { get; set; }
    }
}
