using Student_Management_System_Data.DTOs.Course;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Student_Management_System_Data.DTOs.Teacher
{
    public class TeacherDTO
    {

        public string FullName { get; set; }

        public string Department { get; set; }
    }

    public class TeacherCreateDTO : TeacherDTO { }
    public class TeacherUpdateDTO : TeacherDTO 
    {
        public int Id { get; set; } 
    }

    public class TeacherWithCoursesDTO
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Department { get; set; }
        public List<CourseDTO> Courses { get; set; }
    }

    public class UnassignTeacherDTO
    {
        public int CourseId { get; set; }
        public int TeacherId { get; set; }
    }

    public class ImportTeacherDTO
    {
        public int? Id { get; set; }

        [Required(ErrorMessage = "Full Name is required!!")]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "FullName must contain only letters and spaces")]
        [MaxLength(50)]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Department is required!!")]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Department must contain only letters and spaces")]
        [MaxLength(50)]
        public string Department { get; set; }
    }

}
