using FluentValidation;
using Student_Management_System_Data.DTOs.Course;
using Student_Management_System_Logic.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Student_Management_System_Data.DTOs.Enrollment
{
    public class AssignCourseDTOValidator : AbstractValidator<AssignCourseDTO>
    {
        private readonly IUnitOfWork _unitOfWork;

        public AssignCourseDTOValidator(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;


            RuleFor(A => A.TeacherId).GreaterThan(0).WithMessage("TeacherId is required.");
            RuleFor(A => A.CourseId).GreaterThan(0).WithMessage("CourseId is required.");

            RuleFor(x => x)
                .MustAsync(TeacherCourses)
                .WithMessage("Teacher is already assigned to 5 courses.");
        }

        private async Task<bool> TeacherCourses(AssignCourseDTO assignCourseDTO,CancellationToken token)
        {
            var teacher = await _unitOfWork.Repository<Student_Management_System_Data.Models.Teacher>()
                               .GetById(assignCourseDTO.TeacherId, includeProperties: "Courses");

            return teacher != null && (teacher.Courses?.Count < 5);
        }
    }
}
