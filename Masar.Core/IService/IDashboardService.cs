using Masar.Domain.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Masar.Core.IService
{
    public interface IDashboardService
    {
        Task<CandidateDashboardDto> GetCandidateDashboardAsync(string userId);
        Task<CompanyDashboardDto> GetCompanyDashboardAsync(string userId);
    }
}
