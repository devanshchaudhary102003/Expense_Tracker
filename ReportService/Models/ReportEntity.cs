using System.ComponentModel.DataAnnotations;

namespace ReportService.Models
{
    public class ReportEntity
    {
        [Key]
        public int ReportId { get; set; }
        public int UserId { get; set; }

        [MaxLength(30)]
        public string ReportType { get; set; } = "MONTHLY"; // MONTHLY | CATEGORY | TREND | YEARLY | CUSTOM

        [MaxLength(200)] 
        public string Title { get; set; } = string.Empty;
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

        [MaxLength(1000)] 
        public string? FilePath { get; set; } // Blob URL or local path
        public string? Parameters { get; set; } // JSON

        [MaxLength(20)]
        public string Status { get; set; } = "GENERATED"; // GENERATED | FAILED
    }
}