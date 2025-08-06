using AutoMapper;
using Student_Management_System_Data.DTOs.Teacher;
using Student_Management_System_Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Student_Management_System_Data.Mapping
{
    public class TeacherProfile : Profile
    {
        public TeacherProfile()
        {
            CreateMap<Teacher, TeacherDTO>().ReverseMap();
            CreateMap<Teacher, TeacherCreateDTO>().ReverseMap();
            CreateMap<Teacher, TeacherUpdateDTO>().ReverseMap();
            CreateMap<Teacher, ImportTeacherDTO>().ReverseMap();

            // السطر ده لما اجي احول المدرس الي DTO هاخد معايا الكورسات اللي عند المدرس واملا بيهم ال dest 
            CreateMap<Teacher, TeacherWithCoursesDTO>()
                .ForMember(dest => dest.Courses, opt => opt.MapFrom(src => src.Courses));
        }

    }
}
