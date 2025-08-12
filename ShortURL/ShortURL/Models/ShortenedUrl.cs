using System;
using System.ComponentModel.DataAnnotations;

namespace ShortURL
{
    public class ShortenedUrl
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(64)]
        public string Code { get; set; } = string.Empty;

        [Required]
        [MaxLength(2048)]
        public string OriginalUrl { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int ClickCount { get; set; } = 0;

        public DateTime? LastAccessedAt { get; set; }
    }
}