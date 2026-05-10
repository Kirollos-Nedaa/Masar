using Masar.Core.IService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;

namespace Masar.Core.Services
{
    public class FileService : IFileService
    {
        private readonly IWebHostEnvironment _env;

        public FileService(IWebHostEnvironment env)
        {
            _env = env;
        }

        // ── Public API ────────────────────────────────────────────

        public async Task<string> SaveResumeAsync(IFormFile file, string userId)
        {
            ValidateFile(file, AllowedResumeExtensions, AllowedResumeContentTypes, MaxResumeBytes, "Only PDF and DOCX files are allowed for resumes.");

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var fileName = $"{userId}_{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}{extension}";

            return await SaveFileAsync(file, ResumeFolder, fileName);
        }

        public async Task<string> SaveAvatarAsync(IFormFile file, string userId)
        {
            ValidateFile(file, AllowedImageExtensions, AllowedImageContentTypes, MaxAvatarBytes, "Only JPG, PNG, and WebP images are allowed for profile pictures.");

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var fileName = $"{userId}_{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}{extension}";

            return await SaveFileAsync(file, AvatarFolder, fileName);
        }

        public async Task<string> SaveLogoAsync(IFormFile file, string companyId)
        {
            ValidateFile(file, AllowedImageExtensions, AllowedImageContentTypes,
                MaxLogoBytes, "Only JPG, PNG, and WebP images are allowed for company logos.");

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var fileName = $"{companyId}_{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}{extension}";

            return await SaveFileAsync(file, LogoFolder, fileName);
        }

        public void DeleteFile(string? relativeUrl)
        {
            if (string.IsNullOrWhiteSpace(relativeUrl)) return;

            // relativeUrl looks like "/uploads/resumes/abc_123.pdf"
            var relativePath = relativeUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            var fullPath = Path.Combine(_env.WebRootPath, relativePath);

            if (File.Exists(fullPath))
                File.Delete(fullPath);
        }

        // ── Private helpers ───────────────────────────────────────

        private async Task<string> SaveFileAsync(IFormFile file, string folder, string fileName)
        {
            // Ensure directory exists
            var fullFolder = Path.Combine(_env.WebRootPath, folder);
            Directory.CreateDirectory(fullFolder);

            var fullPath = Path.Combine(fullFolder, fileName);

            await using var stream = new FileStream(fullPath, FileMode.Create);
            await file.CopyToAsync(stream);

            // Return a web-accessible relative URL
            return $"/{folder.Replace(Path.DirectorySeparatorChar, '/')}/{fileName}";
        }

        private static void ValidateFile(IFormFile file, HashSet<string> allowedExtensions, HashSet<string> allowedContentTypes, long maxBytes, string typeErrorMessage)
        {
            if (file == null || file.Length == 0)
                throw new InvalidOperationException("No file was provided or the file is empty.");

            var extension = Path.GetExtension(file.FileName);

            if (!allowedExtensions.Contains(extension))
                throw new InvalidOperationException(typeErrorMessage);

            if (!allowedContentTypes.Contains(file.ContentType))
                throw new InvalidOperationException(typeErrorMessage);

            if (file.Length > maxBytes)
            {
                var limitMb = maxBytes / (1024 * 1024);
                throw new InvalidOperationException($"File size must not exceed {limitMb} MB.");
            }
        }

        // ── Allowed types ─────────────────────────────────────────
        private static readonly HashSet<string> AllowedResumeExtensions = new(StringComparer.OrdinalIgnoreCase) { ".pdf", ".docx" };
        private static readonly HashSet<string> AllowedImageExtensions = new(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png", ".webp" };

        // ── Size limits ───────────────────────────────────────────
        private const long MaxResumeBytes = 5 * 1024 * 1024; // 5 MB
        private const long MaxAvatarBytes = 2 * 1024 * 1024; // 2 MB
        private const long MaxLogoBytes = 2 * 1024 * 1024; // 2 MB

        // ── Upload sub-folders (under wwwroot/uploads/) ───────────
        private const string ResumeFolder = "uploads/resumes";
        private const string AvatarFolder = "uploads/avatars";
        private const string LogoFolder = "uploads/logos";

        private static readonly HashSet<string> AllowedResumeContentTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "application/pdf",
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
        };

        private static readonly HashSet<string> AllowedImageContentTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "image/jpeg",
            "image/png",
            "image/webp"
        };
    }
}