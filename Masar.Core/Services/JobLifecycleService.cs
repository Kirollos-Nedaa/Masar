using Masar.Core.IService;
using Masar.Domain.Helpers;
using Masar.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Masar.Core.Services
{
    public class JobLifecycleService : IJobLifecycleService
    {
        private readonly AppDbContext _context;

        public JobLifecycleService(AppDbContext context)
        {
            _context = context;
        }

        public async Task CloseExpiredJobsAsync()
        {
            await _context.Jobs
                .Where(j => j.IsActive && j.ApplicationDeadline <= AppTime.Now)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(j => j.IsActive, false));
        }
    }
}
