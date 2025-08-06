using AutoMapper;
using Castle.Core.Logging;
using FakeItEasy;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Student_Management_System_API.Controllers;
using Student_Management_System_Data.DTOs.Enrollment;
using Student_Management_System_Data.DTOs.Student;
using Student_Management_System_Data.Models;
using Student_Management_System_Logic.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace Student_Management_System_Test
{
    public class StudentControllerTests
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly StudentController _studentController;
        private readonly ILogger<StudentController> _logger;

        public StudentControllerTests()
        {
            _unitOfWork = A.Fake<IUnitOfWork>();
            _mapper = A.Fake<IMapper>();
            _logger = A.Fake<ILogger<StudentController>>();
            _studentController = new StudentController(_unitOfWork, _mapper,_logger);
        }

        #region AddStudentTests

        private IValidator<StudentCreateDTO> CreateFakeValidatorWithErrors(params (string PropertyName, string ErrorMessage)[] errors)
        {
            var fakeValidator = A.Fake<IValidator<StudentCreateDTO>>();
            var failures = errors.Select(e => new FluentValidation.Results.ValidationFailure(e.PropertyName, e.ErrorMessage)).ToList();
            var validationResult = new FluentValidation.Results.ValidationResult(failures);

            A.CallTo(() => fakeValidator.ValidateAsync(A<StudentCreateDTO>.Ignored, default))
                .Returns(Task.FromResult(validationResult));

            return fakeValidator;
        }

        [Fact]
        public async Task AddStudent_ReturnsBadRequest_WhenStudentIsNull()
        {
            var fakeValidator = A.Fake<IValidator<StudentCreateDTO>>();

            var result = await _studentController.AddStudent(null, fakeValidator);

            result.Should().BeOfType<BadRequestObjectResult>()
                  .Which.Value.Should().Be("Student data is null");
        }

        [Fact]
        public async Task AddStudent_ReturnsBadRequest_WhenFullNameIsMissing()
        {
            var dto = new StudentCreateDTO
            {
                FullName = "",
                Email = "test@example.com",
                DOB = DateTime.Now.AddYears(-20)
            };

            var fakeValidator = CreateFakeValidatorWithErrors(("FullName", "FullName is required"));

            var result = await _studentController.AddStudent(dto, fakeValidator);

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task AddStudent_ReturnsBadRequest_WhenFullNameIsInvalidFormat()
        {
            var dto = new StudentCreateDTO
            {
                FullName = "Rana123",
                Email = "test@example.com",
                DOB = DateTime.Now.AddYears(-20)
            };

            var fakeValidator = CreateFakeValidatorWithErrors(("FullName", "FullName must contain only letters and spaces"));

            var result = await _studentController.AddStudent(dto, fakeValidator);

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task AddStudent_ReturnsBadRequest_WhenFullNameExceedsMaxLength()
        {
            var dto = new StudentCreateDTO
            {
                FullName = new string('A', 51),
                Email = "test@example.com",
                DOB = DateTime.Now.AddYears(-20)
            };

            var fakeValidator = CreateFakeValidatorWithErrors(("FullName", "FullName max length is 50"));

            var result = await _studentController.AddStudent(dto, fakeValidator);

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task AddStudent_ReturnsBadRequest_WhenEmailIsMissing()
        {
            var dto = new StudentCreateDTO
            {
                FullName = "Rana",
                Email = "",
                DOB = DateTime.Now.AddYears(-20)
            };

            var fakeValidator = CreateFakeValidatorWithErrors(("Email", "Email is required"));

            var result = await _studentController.AddStudent(dto, fakeValidator);

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task AddStudent_ReturnsBadRequest_WhenEmailIsInvalidFormat()
        {
            var dto = new StudentCreateDTO
            {
                FullName = "Rana",
                Email = "invalid-email",
                DOB = DateTime.Now.AddYears(-20)
            };

            var fakeValidator = CreateFakeValidatorWithErrors(("Email", "Invalid email format"));

            var result = await _studentController.AddStudent(dto, fakeValidator);

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task AddStudent_ReturnsBadRequest_WhenEmailExceedsMaxLength()
        {
            var dto = new StudentCreateDTO
            {
                FullName = "Rana",
                Email = new string('a', 101) + "@example.com",
                DOB = DateTime.Now.AddYears(-20)
            };

            var fakeValidator = CreateFakeValidatorWithErrors(("Email", "Email max length is 100"));

            var result = await _studentController.AddStudent(dto, fakeValidator);

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task AddStudent_ReturnsBadRequest_WhenDOBIsMissing()
        {
            var dto = new StudentCreateDTO
            {
                FullName = "Rana",
                Email = "test@example.com",
                DOB = default
            };

            var fakeValidator = CreateFakeValidatorWithErrors(("DOB", "Date of Birth is required"));

            var result = await _studentController.AddStudent(dto, fakeValidator);

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task AddStudent_ReturnsBadRequest_WhenEmailAlreadyExist()
        {
            var studentDto = new StudentCreateDTO
            {
                FullName = "Rana",
                Email = "rana@gmail.com",
                DOB = DateTime.Now.AddYears(-20)
            };

            var existingStudent = new List<Student>
    { new Student { Email = "rana@gmail.com" } };

            var repo = A.Fake<IGenericRepository<Student>>();
            A.CallTo(() => _unitOfWork.Repository<Student>()).Returns(repo);
            A.CallTo(() => repo.GetAllAsync(null)).Returns(Task.FromResult((IEnumerable<Student>)existingStudent));

            var fakeValidator = CreateFakeValidatorWithErrors(); // ·« √Œÿ«¡ Â‰«

            var result = await _studentController.AddStudent(studentDto, fakeValidator);

            result.Should().BeOfType<BadRequestObjectResult>()
                  .Which.Value.Should().Be("Email already exists.");
        }

        [Fact]
        public async Task AddStudent_ReturnsOk_WhenDataIsValid()
        {
            var studentDto = new StudentCreateDTO
            {
                FullName = "Rana",
                Email = "rana@gmail.com",
                DOB = DateTime.Now.AddYears(-20)
            };

            var fakeValidator = CreateFakeValidatorWithErrors(); // ·« √Œÿ«¡

            var repo = A.Fake<IGenericRepository<Student>>();
            A.CallTo(() => _unitOfWork.Repository<Student>()).Returns(repo);
            A.CallTo(() => repo.GetAllAsync(null)).Returns(Task.FromResult((IEnumerable<Student>)new List<Student>()));
            A.CallTo(() => repo.AddAsync(A<Student>.Ignored)).Returns(Task.CompletedTask);
            A.CallTo(() => _unitOfWork.CompleteAsync()).Returns(Task.FromResult(1));
            A.CallTo(() => _mapper.Map<Student>(studentDto)).Returns(new Student
            {
                FullName = studentDto.FullName,
                Email = studentDto.Email,
                DOB = studentDto.DOB
            });

            var result = await _studentController.AddStudent(studentDto, fakeValidator);

            result.Should().BeOfType<OkObjectResult>()
                  .Which.Value.Should().Be("Student added successfully");
        }

        #endregion

        #region GetAllStudentTests
        [Fact]
        public async Task GetAllStudents_ReturnsNotFound_WhenNoStudentExist()
        {
            var repo = A.Fake<IGenericRepository<Student>>();
            A.CallTo(() => _unitOfWork.Repository<Student>()).Returns(repo);
            A.CallTo(() => repo.GetAllAsync("Enrollments.Course"))
                             .Returns(Task.FromResult((IEnumerable<Student>)new List<Student>()));

            var result = await _studentController.GetAllStudents();

            result.Should().BeOfType<NotFoundObjectResult>()
                           .Which.Value.Should().Be("No students found.");

        }

        [Fact]
        public async Task GetAllStudents_ReturnOk_WithMappedStudents()
        {
            var repo = A.Fake<IGenericRepository<Student>>();
            A.CallTo(() => _unitOfWork.Repository<Student>()).Returns(repo);

            var students = new List<Student>
            {
                new Student { Id = 1, FullName = "Rana", DOB = DateTime.Now.AddYears(-20)}
            };

            A.CallTo(() => repo.GetAllAsync("Enrollments.Course"))
                              .Returns(Task.FromResult((IEnumerable<Student>)students));

            var mapped = new List<StudentWithCourseDTO>
            {
                new StudentWithCourseDTO {Id = 1, FullName = "Rana", Email = "rana@gmail.com", DOB = System.DateTime.Now.AddYears(-20), Courses = new List<StudentCourseDTO>()}
            };

            A.CallTo(() => _mapper.Map<List<StudentWithCourseDTO>>(students)).Returns(mapped);

            var result = await _studentController.GetAllStudents();

            result.Should().BeOfType<OkObjectResult>()
                           .Which.Value.Should().BeEquivalentTo(mapped);



        }
        #endregion

        #region GetStudentByIdTests
        [Fact]
        public async Task GetStudentById_ReturnBadRequest_WhenIdIsZero()
        {
            var result = await _studentController.GetStudentById(0);

            result.Should().BeOfType<BadRequestObjectResult>()
                           .Which.Value.Should().Be("Invalid ID.");
        }

        [Fact]
        public async Task GetStudentById_ReturnBadRequest_WhenIdIsLessThanZero()
        {
            var result = await _studentController.GetStudentById(-1);

            result.Should().BeOfType<BadRequestObjectResult>()
                           .Which.Value.Should().Be("Invalid ID.");
        }

        [Fact]
        public async Task GetStudentById_ReturnNotfound_WhenNoStudentExist()
        {
            var repo = A.Fake<IGenericRepository<Student>>();
            A.CallTo(() => _unitOfWork.Repository<Student>()).Returns(repo);

            A.CallTo(() => repo.GetById(1, "Enrollments.Course")).Returns((Student)null);

            var result = await _studentController.GetStudentById(1);

            result.Should().BeOfType<NotFoundObjectResult>()
                           .Which.Value.Should().Be("Student not found.");
        }

        [Fact]
        public async Task GetStudentById_ReturnOk_WithMappedStudents()
        {
            var repo = A.Fake<IGenericRepository<Student>>();
            A.CallTo(() => _unitOfWork.Repository<Student>()).Returns(repo);

            var student = new Student
            {
                Id = 2,
                FullName = "Rana",
                Email = "rana@gmail.com",
                DOB = DateTime.Now.AddYears(-20),
                Enrollments = new List<Enrollment>()
            };

            A.CallTo(() => repo.GetById(2, "Enrollments.Course")).Returns(student);

            var mapped = new StudentWithCourseDTO
            {
                Id = 2,
                FullName = "Rana",
                Email = "rana@gmail.com",
                DOB = student.DOB,
                Courses = new List<StudentCourseDTO>()
            };

            A.CallTo(() => _mapper.Map<StudentWithCourseDTO>(student)).Returns(mapped);

            var result = await _studentController.GetStudentById(2);

            result.Should().BeOfType<OkObjectResult>()
                           .Which.Value.Should().BeEquivalentTo(mapped);
        }
        #endregion

        #region UpdateStudentTests

        private IValidator<StudentUpdateDTO> CreateFakeValidatorWithError(params (string PropertyName, string ErrorMessage)[] errors)
        {
            var fakeValidator = A.Fake<IValidator<StudentUpdateDTO>>();
            var failures = errors.Select(e => new FluentValidation.Results.ValidationFailure(e.PropertyName, e.ErrorMessage)).ToList();
            var validationResult = new FluentValidation.Results.ValidationResult(failures);

            A.CallTo(() => fakeValidator.ValidateAsync(A<StudentUpdateDTO>.Ignored, default))
                .Returns(Task.FromResult(validationResult));

            return fakeValidator;
        }

        [Fact]
        public async Task UpdateStudent_ReturnBadRequest_WhenDtoIsNullOrIdIsInvalid()
        {
            var fakeValidator = CreateFakeValidatorWithError();

            var result = await _studentController.UpdateStudent(null, fakeValidator);
            result.Should().BeOfType<BadRequestObjectResult>()
                           .Which.Value.Should().Be("Invalid data.");

            var dto = new StudentUpdateDTO
            {
                Id = 0,
                FullName = "Rana",
                Email = "rana@gmail.com",
                DOB = DateTime.Now.AddYears(-20)
            };

            var res = await _studentController.UpdateStudent(dto, fakeValidator);
            res.Should().BeOfType<BadRequestObjectResult>()
                          .Which.Value.Should().Be("Invalid data.");
        }

        [Fact]
        public async Task UpdateStudent_ReturnsBadRequest_WhenFullNameIsMissing()
        {
            var dto = new StudentUpdateDTO
            {
                Id = 1,
                FullName = "",
                Email = "test@example.com",
                DOB = DateTime.Now.AddYears(-20)
            };

            var fakeValidator = CreateFakeValidatorWithError(("FullName", "FullName is required"));

            var result = await _studentController.UpdateStudent(dto, fakeValidator);

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task UpdateStudent_ReturnsBadRequest_WhenFullNameIsInvalidFormat()
        {
            var dto = new StudentUpdateDTO
            {
                Id = 1,
                FullName = "Rana123",
                Email = "test@example.com",
                DOB = DateTime.Now.AddYears(-20)
            };

            var fakeValidator = CreateFakeValidatorWithError(("FullName", "FullName must contain only letters and spaces"));

            var result = await _studentController.UpdateStudent(dto, fakeValidator);

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task UpdateStudent_ReturnsBadRequest_WhenFullNameExceedsMaxLength()
        {
            var dto = new StudentUpdateDTO
            {
                Id = 1,
                FullName = new string('A', 51),
                Email = "test@example.com",
                DOB = DateTime.Now.AddYears(-20)
            };

            var fakeValidator = CreateFakeValidatorWithError(("FullName", "FullName max length is 50"));

            var result = await _studentController.UpdateStudent(dto, fakeValidator);

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task UpdateStudent_ReturnsBadRequest_WhenEmailIsMissing()
        {
            var dto = new StudentUpdateDTO
            {
                Id = 1,
                FullName = "Rana",
                Email = "",
                DOB = DateTime.Now.AddYears(-20)
            };

            var fakeValidator = CreateFakeValidatorWithError(("Email", "Email is required"));

            var result = await _studentController.UpdateStudent(dto, fakeValidator);

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task UpdateStudent_ReturnsBadRequest_WhenEmailIsInvalidFormat()
        {
            var dto = new StudentUpdateDTO
            {
                Id = 1,
                FullName = "Rana",
                Email = "invalid-email",
                DOB = DateTime.Now.AddYears(-20)
            };

            var fakeValidator = CreateFakeValidatorWithError(("Email", "Invalid email format"));

            var result = await _studentController.UpdateStudent(dto, fakeValidator);

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task UpdateStudent_ReturnsBadRequest_WhenEmailExceedsMaxLength()
        {
            var dto = new StudentUpdateDTO
            {
                Id = 1,
                FullName = "Rana",
                Email = new string('a', 101) + "@example.com",
                DOB = DateTime.Now.AddYears(-20)
            };

            var fakeValidator = CreateFakeValidatorWithError(("Email", "Email max length is 100"));

            var result = await _studentController.UpdateStudent(dto, fakeValidator);

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task UpdateStudent_ReturnsBadRequest_WhenDOBIsMissing()
        {
            var dto = new StudentUpdateDTO
            {
                Id = 1,
                FullName = "Rana",
                Email = "test@example.com",
                DOB = default
            };

            var fakeValidator = CreateFakeValidatorWithError(("DOB", "Date of Birth is required"));

            var result = await _studentController.UpdateStudent(dto, fakeValidator);

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task UpdateStudent_ReturnsNotFound_WhenStudentDoesNotExist()
        {
            var dto = new StudentUpdateDTO
            {
                Id = 2,
                FullName = "Rana",
                Email = "rana@gmail.com",
                DOB = DateTime.Now.AddYears(-20)
            };

            var fakeValidator = CreateFakeValidatorWithError();

            var repo = A.Fake<IGenericRepository<Student>>();
            A.CallTo(() => _unitOfWork.Repository<Student>()).Returns(repo);
            A.CallTo(() => repo.GetById(dto.Id, null)).Returns((Student)null);

            var result = await _studentController.UpdateStudent(dto, fakeValidator);

            result.Should().BeOfType<NotFoundObjectResult>()
                   .Which.Value.Should().Be("Student not found.");
        }

        [Fact]
        public async Task UpdateStudent_ReturnsBadRequest_WhenEmailAlreadyExist()
        {
            var dto = new StudentUpdateDTO
            {
                Id = 1,
                FullName = "Rana",
                Email = "rana@gmail.com",
                DOB = DateTime.Now.AddYears(-20)
            };

            var existingStudent = new Student { Id = 1, Email = "rana@gmail.com" };

            var otherStudents = new List<Student>
            {
                new Student { Id = 2, Email = "rana@gmail.com" } // duplicate
            };

            var fakeValidator = CreateFakeValidatorWithError();

            var repo = A.Fake<IGenericRepository<Student>>();
            A.CallTo(() => _unitOfWork.Repository<Student>()).Returns(repo);
            A.CallTo(() => repo.GetById(dto.Id, null)).Returns(existingStudent);
            A.CallTo(() => repo.GetAllAsync(null)).Returns(otherStudents);

            var result = await _studentController.UpdateStudent(dto, fakeValidator);

            result.Should().BeOfType<BadRequestObjectResult>()
                   .Which.Value.Should().Be("Email already exists.");
        }

        [Fact]
        public async Task UpdateStudent_ReturnsOk_WhenUpdateIsSuccessful()
        {
            var dto = new StudentUpdateDTO
            {
                Id = 1,
                FullName = "Rana",
                Email = "rana@gmail.com",
                DOB = DateTime.Now.AddYears(-20)
            };

            var student = new Student { Id = 1, Email = "old@mail.com", FullName = "Old Name", DOB = DateTime.Now.AddYears(-25) };

            var fakeValidator = CreateFakeValidatorWithError();

            var repo = A.Fake<IGenericRepository<Student>>();
            A.CallTo(() => _unitOfWork.Repository<Student>()).Returns(repo);
            A.CallTo(() => repo.GetById(dto.Id, null)).Returns(student);
            A.CallTo(() => repo.GetAllAsync(null)).Returns(new List<Student> { student });

            A.CallTo(() => _mapper.Map(dto, student)).Returns(student);
            A.CallTo(() => repo.Update(student)).Returns(Task.CompletedTask);
            A.CallTo(() => _unitOfWork.CompleteAsync()).Returns(Task.FromResult(1));

            var result = await _studentController.UpdateStudent(dto, fakeValidator);

            result.Should().BeOfType<OkObjectResult>()
                   .Which.Value.Should().Be("Student updated successfully.");
        }

        #endregion

        #region DeleteStudentTests
        [Fact]
        public async Task DeleteStudent_ReturnBadRequest_WhenIdIsZeroOrNegative()
        {
            var result = await _studentController.DeleteStudent(0);
            result.Should().BeOfType<BadRequestObjectResult>()
                           .Which.Value.Should().Be("Id is required.");

            var res = await _studentController.DeleteStudent(-1);
            res.Should().BeOfType<BadRequestObjectResult>()
                           .Which.Value.Should().Be("Id is required.");
        }

        [Fact]
        public async Task DeleteStudent_ReturnNotFound_WhenStudentNotExist()
        {
            var repo = A.Fake<IGenericRepository<Student>>();
            A.CallTo(() => _unitOfWork.Repository<Student>()).Returns(repo);
            A.CallTo(() => repo.GetById(1, null)).Returns((Student)null);

            var result = await _studentController.DeleteStudent(1);

            result.Should().BeOfType<NotFoundObjectResult>()
                           .Which.Value.Should().Be("Student not found.");
        }

        [Fact]
        public async Task DeleteStudent_ReturnOk_WhenStudentIsDeleted()
        {
            var student = new Student { Id = 1, FullName = "Rana" };

            var repo = A.Fake<IGenericRepository<Student>>();
            A.CallTo(() => _unitOfWork.Repository<Student>()).Returns(repo);
            A.CallTo(() => repo.GetById(1, null)).Returns(student);
            A.CallTo(() => repo.Delete(student)).Returns(Task.CompletedTask);
            A.CallTo(() => _unitOfWork.CompleteAsync()).Returns(Task.FromResult(1));

            var result = await _studentController.DeleteStudent(1);
            result.Should().BeOfType<OkObjectResult>()
                           .Which.Value.Should().Be("Student deleted successfully.");
        }
        #endregion

        #region AssignStudentToCourseTests
        [Fact]
        public async Task AssignStudentToCourse_ReturnsBadRequest_WhenValidationFails()
        {
            var dto = new EnrollmentDTO { StudentId = 1, CourseId = 1 };

            var fakeValidator = A.Fake<IValidator<EnrollmentDTO>>();
            var validationResult = new FluentValidation.Results.ValidationResult(new List<ValidationFailure>
            {
                new ValidationFailure("StudentId", "StudentId is required.")
            });

            A.CallTo(() => fakeValidator.ValidateAsync(dto, default))
                .Returns(Task.FromResult(validationResult));

            var result = await _studentController.AssignStudentToCourse(dto, fakeValidator);

            result.Should().BeOfType<BadRequestObjectResult>()
                .Which.Value.Should().BeEquivalentTo(validationResult.Errors);
        }

        [Fact]
        public async Task AssignStudentToCourse_ReturnsNotFound_WhenStudentDoesNotExist()
        {
            var dto = new EnrollmentDTO { StudentId = 1, CourseId = 1 };

            var fakeValidator = A.Fake<IValidator<EnrollmentDTO>>();
            A.CallTo(() => fakeValidator.ValidateAsync(dto, default))
                .Returns(Task.FromResult(new FluentValidation.Results.ValidationResult()));

            var studentRepo = A.Fake<IGenericRepository<Student>>();
            A.CallTo(() => _unitOfWork.Repository<Student>()).Returns(studentRepo);
            A.CallTo(() => studentRepo.GetById(dto.StudentId, null)).Returns(Task.FromResult<Student>(null));

            var result = await _studentController.AssignStudentToCourse(dto, fakeValidator);

            result.Should().BeOfType<NotFoundObjectResult>()
                .Which.Value.Should().Be("Student not found.");
        }

        [Fact]
        public async Task AssignStudentToCourse_ReturnsNotFound_WhenCourseDoesNotExist()
        {
            var dto = new EnrollmentDTO { StudentId = 1, CourseId = 1 };

            var fakeValidator = A.Fake<IValidator<EnrollmentDTO>>();
            A.CallTo(() => fakeValidator.ValidateAsync(dto, default))
                .Returns(Task.FromResult(new FluentValidation.Results.ValidationResult()));

            var student = new Student { Id = dto.StudentId };
            var studentRepo = A.Fake<IGenericRepository<Student>>();
            A.CallTo(() => _unitOfWork.Repository<Student>()).Returns(studentRepo);
            A.CallTo(() => studentRepo.GetById(dto.StudentId, null)).Returns(Task.FromResult(student));

            var courseRepo = A.Fake<IGenericRepository<Course>>();
            A.CallTo(() => _unitOfWork.Repository<Course>()).Returns(courseRepo);
            A.CallTo(() => courseRepo.GetById(dto.CourseId, null)).Returns(Task.FromResult<Course>(null));

            var result = await _studentController.AssignStudentToCourse(dto, fakeValidator);

            result.Should().BeOfType<NotFoundObjectResult>()
                .Which.Value.Should().Be("Course not found.");
        }

        [Fact]
        public async Task AssignStudentToCourse_ReturnsOk_WhenEnrollmentIsSuccessful()
        {
            var dto = new EnrollmentDTO { StudentId = 1, CourseId = 1 };

            var fakeValidator = A.Fake<IValidator<EnrollmentDTO>>();
            A.CallTo(() => fakeValidator.ValidateAsync(dto, default))
                .Returns(Task.FromResult(new FluentValidation.Results.ValidationResult()));

            var student = new Student { Id = dto.StudentId };
            var studentRepo = A.Fake<IGenericRepository<Student>>();
            A.CallTo(() => _unitOfWork.Repository<Student>()).Returns(studentRepo);
            A.CallTo(() => studentRepo.GetById(dto.StudentId, null)).Returns(Task.FromResult(student));

            var course = new Course { Id = dto.CourseId };
            var courseRepo = A.Fake<IGenericRepository<Course>>();
            A.CallTo(() => _unitOfWork.Repository<Course>()).Returns(courseRepo);
            A.CallTo(() => courseRepo.GetById(dto.CourseId, null)).Returns(Task.FromResult(course));

            var enrollment = new Enrollment { StudentId = dto.StudentId, CourseId = dto.CourseId };
            A.CallTo(() => _mapper.Map<Enrollment>(dto)).Returns(enrollment);

            var enrollmentRepo = A.Fake<IGenericRepository<Enrollment>>();
            A.CallTo(() => _unitOfWork.Repository<Enrollment>()).Returns(enrollmentRepo);
            A.CallTo(() => enrollmentRepo.AddAsync(enrollment)).Returns(Task.CompletedTask);

            A.CallTo(() => _unitOfWork.CompleteAsync()).Returns(Task.FromResult(1));

            var result = await _studentController.AssignStudentToCourse(dto, fakeValidator);

            result.Should().BeOfType<OkObjectResult>()
                .Which.Value.Should().Be("Student assigned to course successfully.");
        } 
        #endregion

        #region UsassignStudentFromCourseTests
        [Fact]
        public async Task UnassignStudentFromCourse_ReturnsBadRequest_WhenIdsAreInvalid()
        {
            var dto = new EnrollmentDTO { StudentId = 0, CourseId = -1 };

            var result = await _studentController.UnassignStudentFromCourse(dto);

            result.Should().BeOfType<BadRequestObjectResult>()
                  .Which.Value.Should().Be("StudentId and CourseId are required.");
        }

        [Fact]
        public async Task UnassignStudentFromCourse_ReturnsNotFound_WhenEnrollmentDoesNotExist()
        {
            var dto = new EnrollmentDTO { StudentId = 1, CourseId = 2 };

            var repo = A.Fake<IGenericRepository<Enrollment>>();
            A.CallTo(() => _unitOfWork.Repository<Enrollment>()).Returns(repo);
            A.CallTo(() => repo.GetAllAsync(null)).Returns(new List<Enrollment>());

            var result = await _studentController.UnassignStudentFromCourse(dto);

            result.Should().BeOfType<NotFoundObjectResult>()
                  .Which.Value.Should().Be("Enrollment not found.");
        }

        [Fact]
        public async Task UnassignStudentFromCourse_ReturnsOk_WhenEnrollmentIsDeleted()
        {
            var dto = new EnrollmentDTO { StudentId = 1, CourseId = 2 };
            var enrollment = new Enrollment { StudentId = 1, CourseId = 2 };

            var repo = A.Fake<IGenericRepository<Enrollment>>();
            A.CallTo(() => _unitOfWork.Repository<Enrollment>()).Returns(repo);
            A.CallTo(() => repo.GetAllAsync(null)).Returns(new List<Enrollment> { enrollment });
            A.CallTo(() => repo.Delete(enrollment)).Returns(Task.CompletedTask);
            A.CallTo(() => _unitOfWork.CompleteAsync()).Returns(1);

            var result = await _studentController.UnassignStudentFromCourse(dto);

            result.Should().BeOfType<OkObjectResult>()
                  .Which.Value.Should().Be("Student unassigned from course successfully.");
        } 
        #endregion
    }
}