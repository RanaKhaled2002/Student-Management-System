using FluentValidation;
using Student_Management_System_Data.DTOs.Course;

namespace Student_Management_System_API.Validators.Course
{
    public class CourseCreateDTOValidator : AbstractValidator<CourseDTO>
    {
        public CourseCreateDTOValidator()
        {
            RuleFor(C => C.Title)
           .NotEmpty().WithMessage("Title is required!!")
           .Matches(@"^[a-zA-Z\s]+$").WithMessage("Title must contain only letters and spaces")
           .MaximumLength(50);

            RuleFor(C => C.Description)
                .NotEmpty().WithMessage("Description is required!!")
                .Matches(@"^[a-zA-Z\s]+$").WithMessage("Description must contain only letters and spaces")
                .MaximumLength(50);
        }
    }
}
