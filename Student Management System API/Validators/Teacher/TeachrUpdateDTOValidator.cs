using FluentValidation;
using Student_Management_System_Data.DTOs.Teacher;

namespace Student_Management_System_API.Validators.Teacher
{
    public class TeachrUpdateDTOValidator : AbstractValidator<TeacherUpdateDTO>
    {
        public TeachrUpdateDTOValidator()
        {
            RuleFor(T => T.FullName)
            .NotEmpty().WithMessage("Full Name is required!!")
            .Matches(@"^[a-zA-Z\s]+$").WithMessage("FullName must contain only letters and spaces")
            .MaximumLength(50);

            RuleFor(T => T.Department)
                .NotEmpty().WithMessage("Department is required!!")
                .Matches(@"^[a-zA-Z\s]+$").WithMessage("Department must contain only letters and spaces")
                .MaximumLength(50);

            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("Id is required");
        }
    }
}
