using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Masar.Infrastructure
{
    public interface IRepository<T> where T : class
    {
        Task<T> GetByIdAsync(int id);
    }
}
