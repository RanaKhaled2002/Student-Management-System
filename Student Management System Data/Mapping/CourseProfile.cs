using AutoMapper;
using Student_Management_System_Data.DTOs.Course;
using Student_Management_System_Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Student_Management_System_Data.Mapping
{
    public class CourseProfile : Profile
    {
        public CourseProfile()
        {
            CreateMap<Course, CourseDTO>().ReverseMap();
            CreateMap<Course, CourseWithTeacherDTO>().ReverseMap();
            CreateMap<Course, ImportCourseDTO>().ReverseMap();
        }
    }
}
