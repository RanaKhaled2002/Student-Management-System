using Student_Management_System_Data.Data;
using Student_Management_System_Data.Models;
using Student_Management_System_Logic.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Student_Management_System_Logic.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly StudentDbContext _studentDbContext;
        private Hashtable _repositories; // استخدمته عشام مفيش داعي ان كل مره اعمل نسخه جديده

        public UnitOfWork(StudentDbContext studentDbContext)
        {
            _studentDbContext = studentDbContext;
            _repositories = new Hashtable();
        }

        public async Task<int> CompleteAsync()
        {
           return await _studentDbContext.SaveChangesAsync();
        }

        // بترجع generic repository للنوع اللي اطلبه 
        public IGenericRepository<T> Repository<T>() where T : BaseClass
        {
            // Ex: type = student
            var type = typeof(T).Name;

            if(!_repositories.ContainsKey(type))
            {
                // بينشئ نسخه جديده ويحقنه بال dbContext
                var repository = new GenericRepository<T>(_studentDbContext);

                _repositories.Add(type, repository);
            }

            return _repositories[type] as IGenericRepository<T>;
        }
    }
}
