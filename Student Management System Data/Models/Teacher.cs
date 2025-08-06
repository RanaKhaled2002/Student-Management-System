using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Student_Management_System_Data.Models
{
    public class Teacher : BaseClass
    {
        public string FullName { get; set; }
        public string Department { get; set; }

        public ICollection<Course> Courses { get; set; }
    }
}
