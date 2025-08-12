using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace ShortURL.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UrlsController : ControllerBase
    {
        private readonly URLDbContext _context;

        public UrlsController(URLDbContext context)
        {
            _context = context;
        }

        [HttpPost("shorten")]
        public async Task<IActionResult> ShortenUrl([FromBody] ShortenUrlRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.LongUrl) || !Uri.TryCreate(request.LongUrl, UriKind.Absolute, out var validatedUri))
            {
                return BadRequest(new { error = "Invalid or empty URL provided." });
            }

            string code;
            
            // Use custom code if provided, otherwise generate random code
            if (!string.IsNullOrWhiteSpace(request.CustomCode))
            {
                code = request.CustomCode.Trim();
                
                // Check if custom code already exists
                if (await _context.ShortenedUrls.AnyAsync(u => u.Code == code))
                {
                    return BadRequest(new { error = "Custom code already exists. Please choose a different code." });
                }
            }
            else
            {
                // Generate code as: <domainLabel>-<random>
                // Extract the label between "www." and the next '.' from the host
                var host = validatedUri.Host.ToLowerInvariant();
                if (host.StartsWith("www."))
                {
                    host = host[4..];
                }

                var firstLabel = host.Split('.')[0];
                // Keep only letters and digits from the label
                var alphanumericLabelChars = firstLabel.Where(char.IsLetterOrDigit).ToArray();
                var baseLabel = new string(alphanumericLabelChars);
                if (string.IsNullOrWhiteSpace(baseLabel))
                {
                    baseLabel = "url";
                }

                string randomSuffix;
                do
                {
                    randomSuffix = Guid.NewGuid().ToString("N")[..6]; // 6 hex chars
                    code = $"{baseLabel}-{randomSuffix}";
                } while (await _context.ShortenedUrls.AnyAsync(u => u.Code == code));
            }

            var shortenedUrl = new ShortURL.ShortenedUrl
            {
                Code = code,
                OriginalUrl = validatedUri.ToString()
            };

            _context.ShortenedUrls.Add(shortenedUrl);
            await _context.SaveChangesAsync();

            var shortUrl = $"{Request.Scheme}://{Request.Host}/r/{shortenedUrl.Code}";
            return CreatedAtAction(nameof(RedirectToUrl), new { code = shortenedUrl.Code }, new { shortUrl });
        }

        [HttpGet("{code}")]
        public async Task<IActionResult> RedirectToUrl(string code)
        {
            var url = await _context.ShortenedUrls.FirstOrDefaultAsync(u => u.Code == code);
            if (url == null)
            {
                return NotFound("Shortened URL not found.");
            }

            url.ClickCount += 1;
            url.LastAccessedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Redirect(url.OriginalUrl);
        }

        // GET api/urls/{code} -> returns metadata for a shortened URL
        [HttpGet("metadata/{code}")]
        public async Task<IActionResult> GetMetadata(string code)
        {
            var url = await _context.ShortenedUrls
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Code == code);

            if (url == null)
            {
                return NotFound(new { error = "Shortened URL not found." });
            }

            return Ok(new
            {
                code = url.Code,
                originalUrl = url.OriginalUrl,
                createdAt = url.CreatedAt,
                clickCount = url.ClickCount,
                lastAccessedAt = url.LastAccessedAt
            });
        }

        // GET api/urls -> returns all shortened URLs
        [HttpGet]
        public async Task<IActionResult> GetAllUrls()
        {
            var urls = await _context.ShortenedUrls
                .AsNoTracking()
                .OrderByDescending(u => u.CreatedAt)
                .Select(u => new
                {
                    code = u.Code,
                    originalUrl = u.OriginalUrl,
                    shortUrl = $"{Request.Scheme}://{Request.Host}/r/{u.Code}",
                    createdAt = u.CreatedAt,
                    clickCount = u.ClickCount,
                    lastAccessedAt = u.LastAccessedAt
                })
                .ToListAsync();

            return Ok(urls);
        }

        // PUT api/urls/{code} -> update original URL and/or code
        [HttpPut("{code}")]
        public async Task<IActionResult> UpdateUrl(string code, [FromBody] UpdateUrlRequest request)
        {
            var url = await _context.ShortenedUrls.FirstOrDefaultAsync(u => u.Code == code);
            if (url == null)
            {
                return NotFound(new { error = "Shortened URL not found." });
            }

            if (!string.IsNullOrWhiteSpace(request.OriginalUrl))
            {
                if (!Uri.TryCreate(request.OriginalUrl, UriKind.Absolute, out var validatedUri))
                {
                    return BadRequest(new { error = "Invalid URL provided." });
                }
                url.OriginalUrl = validatedUri.ToString();
            }

            if (!string.IsNullOrWhiteSpace(request.NewCode) && request.NewCode != code)
            {
                var newCode = request.NewCode.Trim();
                if (await _context.ShortenedUrls.AnyAsync(u => u.Code == newCode))
                {
                    return Conflict(new { error = "Code already exists. Choose a different code." });
                }
                url.Code = newCode;
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                code = url.Code,
                originalUrl = url.OriginalUrl,
                shortUrl = $"{Request.Scheme}://{Request.Host}/r/{url.Code}",
                createdAt = url.CreatedAt,
                clickCount = url.ClickCount,
                lastAccessedAt = url.LastAccessedAt
            });
        }

        // DELETE api/urls/{code}
        [HttpDelete("{code}")]
        public async Task<IActionResult> DeleteUrl(string code)
        {
            var url = await _context.ShortenedUrls.FirstOrDefaultAsync(u => u.Code == code);
            if (url == null)
            {
                return NotFound(new { error = "Shortened URL not found." });
            }

            _context.ShortenedUrls.Remove(url);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}