using AutoMapper;
using FakeItEasy;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Student_Management_System_API.Controllers;
using Student_Management_System_Data.Data;
using Student_Management_System_Data.DTOs.Course;
using Student_Management_System_Data.DTOs.Student;
using Student_Management_System_Data.DTOs.Teacher;
using Student_Management_System_Data.Models;
using Student_Management_System_Logic.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Student_Management_System_Test
{
    public class TeacherControllerTests
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IValidator<AssignCourseDTO> _validator;
        private readonly TeacherController _teacherController;

        public TeacherControllerTests()
        {
            _unitOfWork = A.Fake<IUnitOfWork>();
            _mapper = A.Fake<IMapper>();
            _validator = A.Fake<IValidator<AssignCourseDTO>>();
            _teacherController = new TeacherController(_unitOfWork, _mapper);
        }

        #region AddTeacherTests

        private IValidator<TeacherCreateDTO> CreateFakeTeacherValidatorWithErrors(params (string PropertyName, string ErrorMessage)[] errors)
        {
            var fakeValidator = A.Fake<IValidator<TeacherCreateDTO>>();
            var failures = errors.Select(e => new FluentValidation.Results.ValidationFailure(e.PropertyName, e.ErrorMessage)).ToList();
            var validationResult = new FluentValidation.Results.ValidationResult(failures);

            A.CallTo(() => fakeValidator.ValidateAsync(A<TeacherCreateDTO>.Ignored, default))
                .Returns(Task.FromResult(validationResult));

            return fakeValidator;
        }

        [Fact]
        public async Task AddTeacher_ReturnBadRequest_WhenDataIsNull()
        {
            var fakeValidator = CreateFakeTeacherValidatorWithErrors();

            var result = await _teacherController.AddTeacher(null, fakeValidator);

            result.Should().BeOfType<BadRequestObjectResult>()
                           .Which.Value.Should().Be("Teacher Data is null");
        }

        [Fact]
        public async Task AddTeacher_ReturnBadRequest_WhenFullNameIsMissing()
        {
            var dto = new TeacherCreateDTO
            {
                FullName = "",
                Department = "HR"
            };

            var fakeValidator = CreateFakeTeacherValidatorWithErrors(("FullName", "FullName is required"));

            var result = await _teacherController.AddTeacher(dto, fakeValidator);

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task AddTeacher_ReturnBadRequest_WhenDepartmentIsMissing()
        {
            var dto = new TeacherCreateDTO
            {
                FullName = "Ahmed",
                Department = ""
            };

            var fakeValidator = CreateFakeTeacherValidatorWithErrors(("Department", "Department is required"));

            var result = await _teacherController.AddTeacher(dto, fakeValidator);

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task AddTeacher_ReturnsBadRequest_WhenFullNameExceedsMaxLength()
        {
            var dto = new TeacherCreateDTO
            {
                FullName = new string('A', 51),
                Department = "HR"
            };

            var fakeValidator = CreateFakeTeacherValidatorWithErrors(("FullName", "FullName max length is 50"));

            var result = await _teacherController.AddTeacher(dto, fakeValidator);

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task AddTeacher_ReturnsBadRequest_WhenDepartmentExceedsMaxLength()
        {
            var dto = new TeacherCreateDTO
            {
                FullName = "Ahmed",
                Department = new string('A', 51),
            };

            var fakeValidator = CreateFakeTeacherValidatorWithErrors(("Department", "Department max length is 50"));

            var result = await _teacherController.AddTeacher(dto, fakeValidator);

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task AddTeacher_ReturnsOk_WhenDataIsValid()
        {
            var teacherDto = new TeacherCreateDTO
            {
                FullName = "Rana",
                Department = "HR"
            };

            var repo = A.Fake<IGenericRepository<Teacher>>();
            A.CallTo(() => _unitOfWork.Repository<Teacher>()).Returns(repo);
            A.CallTo(() => repo.GetAllAsync(null)).Returns(Task.FromResult((IEnumerable<Teacher>)new List<Teacher>()));
            A.CallTo(() => repo.AddAsync(A<Teacher>.Ignored)).Returns(Task.CompletedTask);
            A.CallTo(() => _unitOfWork.CompleteAsync()).Returns(Task.FromResult(1));
            A.CallTo(() => _mapper.Map<Teacher>(teacherDto)).Returns(new Teacher
            {
                FullName = teacherDto.FullName,
                Department = teacherDto.Department,
            });

            var fakeValidator = CreateFakeTeacherValidatorWithErrors(); // بدون أخطاء

            var result = await _teacherController.AddTeacher(teacherDto, fakeValidator);

            result.Should().BeOfType<OkObjectResult>()
                           .Which.Value.Should().Be("Teacher Added Successfully");
        }

        #endregion

        #region GetAllTeachersTests
        [Fact]
        public async Task GetAllTeachers_ReturnsNotFound_WhenNoTeacherExist()
        {
            var repo = A.Fake<IGenericRepository<Teacher>>();
            A.CallTo(() => _unitOfWork.Repository<Teacher>()).Returns(repo);
            A.CallTo(() => repo.GetAllAsync("Courses"))
                             .Returns(Task.FromResult((IEnumerable<Teacher>)new List<Teacher>()));

            var result = await _teacherController.GetAllTeachers();

            result.Should().BeOfType<NotFoundObjectResult>()
                           .Which.Value.Should().Be("No teachers found.");

        }

        [Fact]
        public async Task GetAllTeachers_ReturnOk_WithMappedTeachers()
        {
            var repo = A.Fake<IGenericRepository<Teacher>>();
            A.CallTo(() => _unitOfWork.Repository<Teacher>()).Returns(repo);

            var teachers = new List<Teacher>
            {
                new Teacher { Id = 1, FullName = "Rana", Department = "HR" }
            };

            A.CallTo(() => repo.GetAllAsync("Courses"))
                .Returns(Task.FromResult((IEnumerable<Teacher>)teachers));

            var mapped = new List<TeacherWithCoursesDTO>
            {
                new TeacherWithCoursesDTO
                {
                    Id = 1,
                    FullName = "Rana",
                    Department = "HR",
                    Courses = new List<CourseDTO>()
                }
            };

            A.CallTo(() => _mapper.Map<List<TeacherWithCoursesDTO>>(teachers)).Returns(mapped);

            var result = await _teacherController.GetAllTeachers();

            result.Should().BeOfType<OkObjectResult>()
                          .Which.Value.Should().BeEquivalentTo(mapped);
        }
        #endregion

        #region GetTeacherByIdTests
        [Fact]
        public async Task GetTeacherById_ReturnsBadRequest_InvalidId()
        {

            // Act
            var result = await _teacherController.GetTeacherById(0);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>()
                  .Which.Value.Should().Be("Enter correct id.");


            // Act
            var res = await _teacherController.GetTeacherById(-1);

            // Assert
            res.Should().BeOfType<BadRequestObjectResult>()
                  .Which.Value.Should().Be("Enter correct id.");
        }

        [Fact]
        public async Task GetTeacherById_ReturnsNotFound_TeacherNotFound()
        {
            // Arrange
            int teacherId = 1;
            var repo = A.Fake<IGenericRepository<Teacher>>();
            A.CallTo(() => _unitOfWork.Repository<Teacher>()).Returns(repo);
            A.CallTo(() => repo.GetById(teacherId, "Courses")).Returns(Task.FromResult<Teacher>(null));

            // Act
            var result = await _teacherController.GetTeacherById(teacherId);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>()
                  .Which.Value.Should().Be("Teacher not found");
        }

        [Fact]
        public async Task GetTeacherById_ReturnsOkWithMappedTeacher_TeacherFound()
        {
            // Arrange
            int teacherId = 1;
            var repo = A.Fake<IGenericRepository<Teacher>>();
            A.CallTo(() => _unitOfWork.Repository<Teacher>()).Returns(repo);

            var teacher = new Teacher
            {
                Id = teacherId,
                FullName = "Rana",
                Department = "HR",
                Courses = new List<Course>()
            };

            A.CallTo(() => repo.GetById(teacherId, "Courses")).Returns(Task.FromResult(teacher));

            var mappedTeacherDto = new TeacherWithCoursesDTO
            {
                Id = teacherId,
                FullName = "Rana",
                Department = "HR",
                Courses = new List<CourseDTO>()
            };

            A.CallTo(() => _mapper.Map<TeacherWithCoursesDTO>(teacher)).Returns(mappedTeacherDto);

            // Act
            var result = await _teacherController.GetTeacherById(teacherId);

            // Assert
            result.Should().BeOfType<OkObjectResult>()
                  .Which.Value.Should().BeEquivalentTo(mappedTeacherDto);
        }
        #endregion

        #region UpdateTeacherTests

        private IValidator<TeacherUpdateDTO> CreateFakeTeacherUpdateValidatorWithErrors(params (string PropertyName, string ErrorMessage)[] errors)
        {
            var fakeValidator = A.Fake<IValidator<TeacherUpdateDTO>>();
            var failures = errors.Select(e => new FluentValidation.Results.ValidationFailure(e.PropertyName, e.ErrorMessage)).ToList();
            var validationResult = new FluentValidation.Results.ValidationResult(failures);

            A.CallTo(() => fakeValidator.ValidateAsync(A<TeacherUpdateDTO>.Ignored, default))
                .Returns(Task.FromResult(validationResult));

            return fakeValidator;
        }

        [Fact]
        public async Task UpdateTeacher_ReturnsBadRequest_NullDtoOrZeroId()
        {
            // Arrange
            TeacherUpdateDTO nullDto = null;
            var dtoWithZeroId = new TeacherUpdateDTO { Id = 0 };

            var fakeValidator = CreateFakeTeacherUpdateValidatorWithErrors();

            // Act
            var result1 = await _teacherController.UpdateTeacher(nullDto, fakeValidator);
            var result2 = await _teacherController.UpdateTeacher(dtoWithZeroId, fakeValidator);

            // Assert
            result1.Should().BeOfType<BadRequestObjectResult>()
                   .Which.Value.Should().Be("Invalid Data");

            result2.Should().BeOfType<BadRequestObjectResult>()
                   .Which.Value.Should().Be("Invalid Data");
        }

        [Fact]
        public async Task UpdateTeacher_ReturnsBadRequest_WhenValidationFails()
        {
            // Arrange
            var invalidDto = new TeacherUpdateDTO { Id = 1, FullName = "", Department = "" };
            var fakeValidator = CreateFakeTeacherUpdateValidatorWithErrors(
                ("FullName", "FullName is required"),
                ("Department", "Department is required")
            );

            // Act
            var result = await _teacherController.UpdateTeacher(invalidDto, fakeValidator);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>()
                  .Which.Value.Should().BeAssignableTo<List<FluentValidation.Results.ValidationFailure>>()
                  .Subject.Should().Contain(v => v.PropertyName == "FullName" && v.ErrorMessage == "FullName is required")
                  .And.Contain(v => v.PropertyName == "Department" && v.ErrorMessage == "Department is required");
        }


        [Fact]
        public async Task UpdateTeacher_ReturnsNotFound_WhenTeacherNotFound()
        {
            // Arrange
            var dto = new TeacherUpdateDTO { Id = 1, FullName = "Rana", Department = "HR" };
            var fakeValidator = CreateFakeTeacherUpdateValidatorWithErrors();

            var repo = A.Fake<IGenericRepository<Teacher>>();
            A.CallTo(() => _unitOfWork.Repository<Teacher>()).Returns(repo);
            A.CallTo(() => repo.GetById(dto.Id, null)).Returns(Task.FromResult<Teacher>(null));

            // Act
            var result = await _teacherController.UpdateTeacher(dto, fakeValidator);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>()
                  .Which.Value.Should().Be("Teacher Not found");
        }

        [Fact]
        public async Task UpdateTeacher_UpdatesTeacherAndReturnsOk_WhenDataIsValid()
        {
            // Arrange
            var dto = new TeacherUpdateDTO { Id = 1, FullName = "Rana", Department = "HR" };
            var fakeValidator = CreateFakeTeacherUpdateValidatorWithErrors();

            var repo = A.Fake<IGenericRepository<Teacher>>();
            var existingTeacher = new Teacher { Id = 1, FullName = "OldName", Department = "OldDept" };

            A.CallTo(() => _unitOfWork.Repository<Teacher>()).Returns(repo);
            A.CallTo(() => repo.GetById(dto.Id, null)).Returns(Task.FromResult(existingTeacher));
            A.CallTo(() => repo.Update(existingTeacher)).Returns(Task.CompletedTask);
            A.CallTo(() => _unitOfWork.CompleteAsync()).Returns(Task.FromResult(1));
            A.CallTo(() => _mapper.Map(dto, existingTeacher)).Returns(existingTeacher);

            // Act
            var result = await _teacherController.UpdateTeacher(dto, fakeValidator);

            // Assert
            result.Should().BeOfType<OkObjectResult>()
                  .Which.Value.Should().Be("Teacher updated successfully.");

            A.CallTo(() => repo.Update(existingTeacher)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _unitOfWork.CompleteAsync()).MustHaveHappenedOnceExactly();
        }

        #endregion

        #region DeleteTeacherTests
        [Fact]
        public async Task DeleteTeacher_ReturnsBadRequest_IdIsZero()
        {
            // Act
            var result = await _teacherController.DeleteTeacher(0);
            var res = await _teacherController.DeleteTeacher(-1);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>()
                  .Which.Value.Should().Be("Id is required.");

            res.Should().BeOfType<BadRequestObjectResult>()
                  .Which.Value.Should().Be("Id is required.");
        }

        [Fact]
        public async Task DeleteTeacher_ReturnsNotFound_TeacherNotFound()
        {
            // Arrange
            var repo = A.Fake<IGenericRepository<Teacher>>();
            A.CallTo(() => _unitOfWork.Repository<Teacher>()).Returns(repo);
            A.CallTo(() => repo.GetById(1,null)).Returns(Task.FromResult<Teacher>(null));

            // Act
            var result = await _teacherController.DeleteTeacher(1);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>()
                  .Which.Value.Should().Be("teacher not found.");
        }

        [Fact]
        public async Task DeleteTeacher_DeletesAndReturnsOk_TeacherFound()
        {
            // Arrange
            var repo = A.Fake<IGenericRepository<Teacher>>();
            var teacher = new Teacher { Id = 1, FullName = "Rana", Department = "HR" };

            A.CallTo(() => _unitOfWork.Repository<Teacher>()).Returns(repo);
            A.CallTo(() => repo.GetById(1,null)).Returns(Task.FromResult(teacher));
            A.CallTo(() => repo.Delete(teacher)).Returns(Task.CompletedTask);
            A.CallTo(() => _unitOfWork.CompleteAsync()).Returns(Task.FromResult(1));

            // Act
            var result = await _teacherController.DeleteTeacher(1);

            // Assert
            result.Should().BeOfType<OkObjectResult>()
                  .Which.Value.Should().Be("Teacher deleted successfully.");

            A.CallTo(() => repo.Delete(teacher)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _unitOfWork.CompleteAsync()).MustHaveHappenedOnceExactly();
        }
        #endregion

        #region AssignCourseToTeacherTests
        [Fact]
        public async Task AssignCourse_ReturnsBadRequest_WhenIdsAreInvalid()
        {
            var dto = new AssignCourseDTO { CourseId = 0, TeacherId = 0 };

            var result = await _teacherController.AssignCourse(dto, _validator);

            result.Should().BeOfType<BadRequestObjectResult>()
                  .Which.Value.Should().Be("Please Assign Correct Id");
        }

        [Fact]
        public async Task AssignCourse_ReturnsBadRequest_WhenValidationFails()
        {
            var dto = new AssignCourseDTO { CourseId = 1, TeacherId = 1 };

            A.CallTo(() => _validator.ValidateAsync(dto, CancellationToken.None))
                .Returns(Task.FromResult(new FluentValidation.Results.ValidationResult(new List<ValidationFailure>
                {
                new ValidationFailure("TeacherId", "Validation failed")
                })));

            var result = await _teacherController.AssignCourse(dto, _validator);

            result.Should().BeOfType<BadRequestObjectResult>()
                  .Which.Value.Should().BeOfType<List<ValidationFailure>>()
                  .And.Subject.As<List<ValidationFailure>>()
                  .Should().ContainSingle(f => f.PropertyName == "TeacherId" && f.ErrorMessage == "Validation failed");
        }

        [Fact]
        public async Task AssignCourse_ReturnsNotFound_WhenTeacherNotFound()
        {
            var dto = new AssignCourseDTO { CourseId = 1, TeacherId = 1 };

            A.CallTo(() => _validator.ValidateAsync(dto, CancellationToken.None))
                .Returns(Task.FromResult(new FluentValidation.Results.ValidationResult()));

            var teacherRepo = A.Fake<IGenericRepository<Teacher>>();
            A.CallTo(() => _unitOfWork.Repository<Teacher>()).Returns(teacherRepo);
            A.CallTo(() => teacherRepo.GetById(dto.TeacherId, null)).Returns(Task.FromResult<Teacher>(null));

            var result = await _teacherController.AssignCourse(dto, _validator);

            result.Should().BeOfType<NotFoundObjectResult>()
                  .Which.Value.Should().Be("Teacher not Found");
        }

        [Fact]
        public async Task AssignCourse_ReturnsNotFound_WhenCourseNotFound()
        {
            var dto = new AssignCourseDTO { CourseId = 1, TeacherId = 1 };

            A.CallTo(() => _validator.ValidateAsync(dto, CancellationToken.None))
                .Returns(Task.FromResult(new FluentValidation.Results.ValidationResult()));

            var teacher = new Teacher { Id = 1, FullName = "Rana" };

            var teacherRepo = A.Fake<IGenericRepository<Teacher>>();
            var courseRepo = A.Fake<IGenericRepository<Course>>();
            A.CallTo(() => _unitOfWork.Repository<Teacher>()).Returns(teacherRepo);
            A.CallTo(() => _unitOfWork.Repository<Course>()).Returns(courseRepo);

            A.CallTo(() => teacherRepo.GetById(dto.TeacherId, null)).Returns(Task.FromResult(teacher));
            A.CallTo(() => courseRepo.GetById(dto.CourseId, null)).Returns(Task.FromResult<Course>(null));

            var result = await _teacherController.AssignCourse(dto, _validator);

            result.Should().BeOfType<NotFoundObjectResult>()
                  .Which.Value.Should().Be("Course Not Found");
        }

        [Fact]
        public async Task AssignCourse_AssignsCourseAndReturnsOk_WhenDataIsValid()
        {
            var dto = new AssignCourseDTO { CourseId = 1, TeacherId = 1 };
            var teacher = new Teacher { Id = 1, FullName = "Rana" };
            var course = new Course { Id = 1, Title = "Math" };

            A.CallTo(() => _validator.ValidateAsync(dto, CancellationToken.None))
                .Returns(Task.FromResult(new FluentValidation.Results.ValidationResult()));

            var teacherRepo = A.Fake<IGenericRepository<Teacher>>();
            var courseRepo = A.Fake<IGenericRepository<Course>>();
            A.CallTo(() => _unitOfWork.Repository<Teacher>()).Returns(teacherRepo);
            A.CallTo(() => _unitOfWork.Repository<Course>()).Returns(courseRepo);

            A.CallTo(() => teacherRepo.GetById(dto.TeacherId, null)).Returns(Task.FromResult(teacher));
            A.CallTo(() => courseRepo.GetById(dto.CourseId, null)).Returns(Task.FromResult(course));

            A.CallTo(() => courseRepo.Update(course)).Returns(Task.CompletedTask);
            A.CallTo(() => _unitOfWork.CompleteAsync()).Returns(Task.FromResult(1));

            var result = await _teacherController.AssignCourse(dto, _validator);

            result.Should().BeOfType<OkObjectResult>()
                  .Which.Value.Should().Be($"Course '{course.Title}' assigned to teacher '{teacher.FullName}' successfully.");

            course.TeacherId.Should().Be(teacher.Id);

            A.CallTo(() => courseRepo.Update(course)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _unitOfWork.CompleteAsync()).MustHaveHappenedOnceExactly();
        } 
        #endregion

        #region UnassignTeacherFromCourseTests
        [Fact]
        public async Task UnassignTeacherFromCourse_ReturnsBadRequest_WhenInvalidIds()
        {
            var dto = new UnassignTeacherDTO { CourseId = 0, TeacherId = 0 };

            var result = await _teacherController.UnassignTeacherFromCourse(dto);

            result.Should().BeOfType<BadRequestObjectResult>()
                  .Which.Value.Should().Be("CourseId and TeacherId are required.");
        }

        [Fact]
        public async Task UnassignTeacherFromCourse_ReturnsNotFound_WhenCourseNotFound()
        {
            var dto = new UnassignTeacherDTO { CourseId = 1, TeacherId = 1 };

            var courseRepo = A.Fake<IGenericRepository<Course>>();
            A.CallTo(() => _unitOfWork.Repository<Course>()).Returns(courseRepo);
            A.CallTo(() => courseRepo.GetById(dto.CourseId,null)).Returns(Task.FromResult<Course>(null));

            var result = await _teacherController.UnassignTeacherFromCourse(dto);

            result.Should().BeOfType<NotFoundObjectResult>()
                  .Which.Value.Should().Be("Course not found.");
        }

        [Fact]
        public async Task UnassignTeacherFromCourse_ReturnsBadRequest_WhenTeacherIdDoesNotMatch()
        {
            var dto = new UnassignTeacherDTO { CourseId = 1, TeacherId = 2 };
            var course = new Course { Id = 1, Title = "Math", TeacherId = 3 };

            var courseRepo = A.Fake<IGenericRepository<Course>>();
            A.CallTo(() => _unitOfWork.Repository<Course>()).Returns(courseRepo);
            A.CallTo(() => courseRepo.GetById(dto.CourseId,null)).Returns(Task.FromResult(course));

            var result = await _teacherController.UnassignTeacherFromCourse(dto);

            result.Should().BeOfType<BadRequestObjectResult>()
                  .Which.Value.Should().Be("This course is not assigned to the specified teacher.");
        }

        [Fact]
        public async Task UnassignTeacherFromCourse_SuccessfullyUnassignsTeacher()
        {
            var dto = new UnassignTeacherDTO { CourseId = 1, TeacherId = 2 };
            var course = new Course { Id = 1, Title = "Math", TeacherId = 2 };

            var courseRepo = A.Fake<IGenericRepository<Course>>();
            A.CallTo(() => _unitOfWork.Repository<Course>()).Returns(courseRepo);
            A.CallTo(() => courseRepo.GetById(dto.CourseId,null)).Returns(Task.FromResult(course));

            A.CallTo(() => courseRepo.Update(course)).Returns(Task.CompletedTask);
            A.CallTo(() => _unitOfWork.CompleteAsync()).Returns(Task.FromResult(1));

            var result = await _teacherController.UnassignTeacherFromCourse(dto);

            result.Should().BeOfType<OkObjectResult>()
                  .Which.Value.Should().Be($"Teacher unassigned from course '{course.Title}' successfully.");

            course.TeacherId.Should().BeNull();

            A.CallTo(() => courseRepo.Update(course)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _unitOfWork.CompleteAsync()).MustHaveHappenedOnceExactly();
        }
        #endregion

    }
}
