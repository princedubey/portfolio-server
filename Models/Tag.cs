using System.ComponentModel.DataAnnotations;

namespace BlogManagementSystem.Models
{
    public class Tag
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; }

        [Required]
        [MaxLength(50)]
        public string Slug { get; set; }

        // Navigation property
        public ICollection<BlogPostTag> BlogPostTags { get; set; }
    }
}
