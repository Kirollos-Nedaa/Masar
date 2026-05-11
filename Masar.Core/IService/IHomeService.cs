using Masar.Domain.ViewModels.HomeDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Masar.Core.IService
{
    public interface IHomeService
    {
        Task<HomePageDto> GetHomePageDataAsync();
    }
}
