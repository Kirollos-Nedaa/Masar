using Microsoft.AspNetCore.Http;

namespace Masar.Core.IService
{
    public interface IFileService
    {
        Task<string> SaveResumeAsync(IFormFile file, string userId);
        Task<string> SaveAvatarAsync(IFormFile file, string userId);
        Task<string> SaveLogoAsync(IFormFile file, string companyId);
        void DeleteFile(string? relativeUrl);
    }
}