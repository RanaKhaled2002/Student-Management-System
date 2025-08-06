using FluentValidation;
using Student_Management_System_Data.DTOs.Course;

namespace Student_Management_System_API.Validators.Course
{
    public class CourseUpdateDTOValidator : AbstractValidator<CourseUpdateDTO>
    {
        public CourseUpdateDTOValidator()
        {
            RuleFor(C => C.Title)
                  .NotEmpty().WithMessage("Title is required!!")
                  .Matches(@"^[a-zA-Z\s]+$").WithMessage("Title must contain only letters and spaces")
                  .MaximumLength(50);

            RuleFor(C => C.Description)
                .NotEmpty().WithMessage("Description is required!!")
                .Matches(@"^[a-zA-Z\s]+$").WithMessage("Description must contain only letters and spaces")
                .MaximumLength(50);

            RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Id is Required");
        }
    }
}
