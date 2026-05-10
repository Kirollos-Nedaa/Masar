namespace Masar.Core.IService
{
    public interface IJobLifecycleService
    {
        Task CloseExpiredJobsAsync();
    }
}
