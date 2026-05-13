using Masar.Domain.ViewModels.HomeDtos;

namespace Masar.Core.IService
{
    public interface IHomeService
    {
        Task<HomePageDto> GetHomePageDataAsync();
    }
}
