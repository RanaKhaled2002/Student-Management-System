using Student_Management_System_Data.DTOs.Course;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Student_Management_System_Data.DTOs.Student
{
    public class StudentDTO
    {
       
        public string FullName { get; set; }

       
        public string Email { get; set; }

        public DateTime DOB { get; set; }
    }

    public class StudentCreateDTO : StudentDTO
    {
    }

    public class StudentUpdateDTO : StudentDTO
    {
        public int Id { get; set; }
    }

    public class StudentWithCourseDTO
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public DateTime DOB { get; set; }

        public List<StudentCourseDTO> Courses { get; set; }
    }


    public class StudentCourseDTO
    {
        public int CourseId { get; set; }
        public string CourseTitle { get; set; }
        public decimal? Grade { get; set; }
    }

}
