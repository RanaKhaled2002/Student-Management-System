using Student_Management_System_Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Student_Management_System_Logic.Interfaces
{
    public interface IGenericRepository<T> where T : BaseClass
    {
        Task<IEnumerable<T>> GetAllAsync(string includeProperties = null);
        Task<T> GetById(int id, string includeProperties = null);
        Task AddAsync(T entity);
        Task Update(T entity);
        Task Delete(T entity);
    }
}
