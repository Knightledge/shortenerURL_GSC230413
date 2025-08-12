using System.ComponentModel.DataAnnotations;

namespace ShortURL.Controllers
{
    public class ShortenUrlRequest
    {
        [Required]
        [Url]
        public string LongUrl { get; set; } = string.Empty;

        [MaxLength(10)]
        [RegularExpression(@"^[a-zA-Z0-9]+$", ErrorMessage = "Custom code must contain only letters and numbers")]
        public string? CustomCode { get; set; }
    }

    public class UpdateUrlRequest
    {
        [Url]
        public string? OriginalUrl { get; set; }

        [MaxLength(64)]
        [RegularExpression(@"^[a-zA-Z0-9\-]+$", ErrorMessage = "Code must contain only letters, numbers, or hyphen")]
        public string? NewCode { get; set; }
    }
}
