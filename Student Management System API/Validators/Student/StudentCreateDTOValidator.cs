using FluentValidation;
using Student_Management_System_Data.DTOs.Student;

namespace Student_Management_System_API.Validators.Student
{
    public class StudentCreateDTOValidator : AbstractValidator<StudentCreateDTO>
    {
        public StudentCreateDTOValidator()
        {
            RuleFor(S => S.FullName)
                .NotEmpty().WithMessage("FullName is required")
                .Matches(@"^[a-zA-Z\s]+$").WithMessage("FullName must contain only letters and spaces")
                .MaximumLength(50).WithMessage("MaxLenght Is 50");

            RuleFor(S => S.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Invalid email format")
                .MaximumLength(100).WithMessage("MaxLenght Is 100");

            RuleFor(S => S.DOB)
                .NotEmpty().WithMessage("Date of Birth is required");
        }
    }
}
