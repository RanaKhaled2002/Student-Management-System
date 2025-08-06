using Microsoft.EntityFrameworkCore;
using Student_Management_System_Data.Data;
using Student_Management_System_Data.Models;
using Student_Management_System_Logic.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Student_Management_System_Logic.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : BaseClass
    {
        private readonly StudentDbContext _studentDbContext;

        public GenericRepository(StudentDbContext studentDbContext)
        {
            _studentDbContext = studentDbContext;
        }

        public async Task<IEnumerable<T>> GetAllAsync(string includeProperties = null)
        {
            IQueryable<T> query = _studentDbContext.Set<T>();

            if (!string.IsNullOrWhiteSpace(includeProperties))
            {
                foreach (var includeProp in includeProperties.Split(',', StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProp.Trim());
                }
            }

            return await query.ToListAsync();
        }


        public async Task<T> GetById(int id, string includeProperties = null)
        {
            IQueryable<T> query = _studentDbContext.Set<T>();

            if (!string.IsNullOrWhiteSpace(includeProperties))
            {
                foreach (var includeProp in includeProperties.Split(',', StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProp.Trim());
                }
            }

            return await query.FirstOrDefaultAsync(e => e.Id == id);
        }


        public async Task AddAsync(T entity)
        {
            await _studentDbContext.AddAsync(entity);
        }

        public async Task Update(T entity)
        {
             _studentDbContext.Update(entity);
        }

        public async Task Delete(T entity)
        {
            _studentDbContext.Remove(entity);
        }

    }
}
