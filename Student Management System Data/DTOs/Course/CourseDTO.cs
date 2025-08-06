using Student_Management_System_Data.DTOs.Teacher;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Student_Management_System_Data.DTOs.Course
{
    public class CourseDTO
    {
       
        public string Title { get; set; }

        public string Description { get; set; }



    }

    public class CourseUpdateDTO : CourseDTO
    {
     
        public int Id { get; set; }
    }

    public class AssignCourseDTO
    {
        public int TeacherId { get; set; }
        public int CourseId { get; set; }
    }
   
    public class CourseWithTeacherDTO
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }

        public TeacherDTO Teacher { get; set; }
    }

    public class ImportCourseDTO
    {
        public int? Id { get; set; }

        [Required(ErrorMessage = "Title is required!!")]
        [MaxLength(50)]
        public string Title { get; set; }

        [Required(ErrorMessage = "Description is required!!")]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Description must contain only letters and spaces")]
        [MaxLength(50)]
        public string Description { get; set; }
    }
}
