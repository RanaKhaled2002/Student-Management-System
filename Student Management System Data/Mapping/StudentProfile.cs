using AutoMapper;
using Microsoft.Extensions.Configuration;
using Student_Management_System_Data.DTOs.Student;
using Student_Management_System_Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Student_Management_System_Data.Mapping
{
    public class StudentProfile : Profile
    {
        public StudentProfile()
        {
            CreateMap<Student, StudentDTO>().ReverseMap();
            CreateMap<Student, StudentCreateDTO>().ReverseMap();
            CreateMap<Student, StudentUpdateDTO>().ReverseMap();


            CreateMap<Student, StudentWithCourseDTO>()
                    .ForMember(dest => dest.Courses, opt => opt.MapFrom(src =>
                        src.Enrollments.Select(e => new StudentCourseDTO
                        {
                            CourseId = e.CourseId,
                            CourseTitle = e.Course.Title,
                            Grade = e.Grade  
                        }).ToList()
                    ));

        }
    }
}
