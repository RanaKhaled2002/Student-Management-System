using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Student_Management_System_Data.DTOs.Enrollment
{
    public class EnrollmentDTO
    {
        public int StudentId { get; set; }
        public int CourseId { get; set; }
    }

    public class AddOrUpdateGradeDTO : EnrollmentDTO
    {
        public decimal Grade { get; set; }
    }

    public class ImportGradeDTO
    {
        [Required]
        public int StudentId { get; set; }

        [Required]
        public int CourseId { get; set; }

        [Required]
        public decimal Grade { get; set; }
    }
}
