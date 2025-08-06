using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Student_Management_System_Data.Models
{
    public class Student : BaseClass
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public DateTime DOB { get; set; }

        public ICollection<Enrollment> Enrollments { get; set; }
    }
}
