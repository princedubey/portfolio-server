using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlogManagementSystem.Models
{
    public class BlogPost
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; }

        [Required]
        [MaxLength(200)]
        public string Slug { get; set; }

        [Required]
        public string Content { get; set; }

        [MaxLength(500)]
        public string Excerpt { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }

        public DateTime? UpdatedDate { get; set; }

        public DateTime? PublishedDate { get; set; }

        public bool IsPublished { get; set; }

        public bool IsFeatured { get; set; }

        [ForeignKey("Category")]
        public int CategoryId { get; set; }
        public Category Category { get; set; }

        [ForeignKey("User")]
        public int AuthorId { get; set; }
        public User Author { get; set; }

        // SEO Properties
        [MaxLength(160)]
        public string MetaDescription { get; set; }

        [MaxLength(100)]
        public string MetaKeywords { get; set; }

        public string FeaturedImageUrl { get; set; }

        // Navigation properties
        public ICollection<Comment> Comments { get; set; }
        public ICollection<BlogPostTag> BlogPostTags { get; set; }

        public PostStatus Status { get; set; }

        public int ViewCount { get; set; }
    }

    public enum PostStatus
    {
        Draft,
        Published,
        Archived
    }
}
