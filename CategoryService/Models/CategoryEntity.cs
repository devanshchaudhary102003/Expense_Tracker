using System.ComponentModel.DataAnnotations;

namespace CategoryService.Models
{
    public class CategoryEntity
    {
        [Key] 
        public int CategoryId { get; set; }
        public int? UserId { get; set; } // null for system defaults

        [Required, MaxLength(100)] 
        public string Name { get; set; } = string.Empty;
        [MaxLength(50)]
        public string? Icon  { get; set; }
        [MaxLength(10)]  
        public string? Color { get; set; }

        [MaxLength(10)]
        public string Type { get; set; } = "EXPENSE"; // EXPENSE | INCOME

        public bool IsDefault { get; set; }
        public bool IsActive  { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}