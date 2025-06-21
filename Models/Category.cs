using System.ComponentModel.DataAnnotations;

namespace BlogManagementSystem.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [Required]
        [MaxLength(100)]
        public string Slug { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }

        // SEO Properties
        [MaxLength(160)]
        public string MetaDescription { get; set; }

        // Navigation property
        public ICollection<BlogPost> BlogPosts { get; set; }
    }
}
