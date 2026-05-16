using Masar.Domain.ViewModels.AdminDtos;
using Masar.Domain.ViewModels.CandidateDtos;
using Masar.Domain.ViewModels.CompanyDtos;

namespace Masar.Core.IService
{
    public interface IDashboardService
    {
        Task<CandidateDashboardDto> GetCandidateDashboardAsync(string userId);
        Task<CompanyDashboardDto> GetCompanyDashboardAsync(string userId);
        Task<AdminDashboardDto> GetAdminDashboardAsync();
    }
}
