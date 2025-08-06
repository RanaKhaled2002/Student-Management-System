using AutoMapper;
using Student_Management_System_Data.DTOs.Enrollment;
using Student_Management_System_Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Student_Management_System_Data.Mapping
{
    public class EnrollmentProfile : Profile
    {
        public EnrollmentProfile()
        {
            CreateMap<Enrollment, EnrollmentDTO>().ReverseMap();
            CreateMap<Enrollment, ImportGradeDTO>().ReverseMap();
            CreateMap<Enrollment, AddOrUpdateGradeDTO>().ReverseMap();

        }
    }
}
