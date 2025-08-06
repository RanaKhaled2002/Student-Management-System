using FluentValidation;
using Student_Management_System_Data.DTOs.Enrollment;
using Student_Management_System_Data.Models;
using Student_Management_System_Logic.Interfaces;

namespace Student_Management_System_API.Validators.Enrollment
{
    public class EnrollmentDTOValidator : AbstractValidator<EnrollmentDTO>
    {
        private readonly IUnitOfWork _unitOfWork;

        public EnrollmentDTOValidator(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;

            RuleFor(E => E.StudentId).GreaterThan(0).WithMessage("StudentId is required.");
            RuleFor(E => E.CourseId).GreaterThan(0).WithMessage("CourseId is required.");

            RuleFor(E => E)
                .MustAsync(NotAlreadyEnrolled)
                .WithMessage("Student is already enrolled in this course.");
        }

        private async Task<bool> NotAlreadyEnrolled(EnrollmentDTO enrollmentDTO, CancellationToken cancellationToken)
        {
            var enrollments = await _unitOfWork.Repository<Student_Management_System_Data.Models.Enrollment>().GetAllAsync();

            return !enrollments.Any(E => E.CourseId == enrollmentDTO.CourseId && E.StudentId == enrollmentDTO.StudentId);
        }
    }
}
