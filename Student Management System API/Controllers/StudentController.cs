using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Student_Management_System_Data.DTOs.Enrollment;
using Student_Management_System_Data.DTOs.Student;
using Student_Management_System_Data.Models;
using Student_Management_System_Logic.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace Student_Management_System_API.Controllers
{
    [Route("api/Student")]
    [ApiController]
    [Authorize]
    public class StudentController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<StudentController> _logger;

        public StudentController(IUnitOfWork unitOfWork, IMapper mapper,ILogger<StudentController> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        [Authorize(Roles ="Admin,Student")]
        [HttpPost("AddStudent")]
        public async Task<IActionResult> AddStudent([FromBody] StudentCreateDTO studentCreateDTO, [FromServices] IValidator<StudentCreateDTO> validator)
        {
            if (studentCreateDTO == null)
            {
                _logger.LogWarning("Data is null");
                return BadRequest("Student data is null");
            }

            var validationResult = await validator.ValidateAsync(studentCreateDTO);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.Errors);

            var allStudents = await _unitOfWork.Repository<Student>().GetAllAsync();
            if (allStudents.Any(s => s.Email.ToLower() == studentCreateDTO.Email.ToLower()))
            {
                _logger.LogWarning("Email found");
                return BadRequest("Email already exists.");
            }

            var entity = _mapper.Map<Student>(studentCreateDTO);

            await _unitOfWork.Repository<Student>().AddAsync(entity);
            await _unitOfWork.CompleteAsync();

            return Ok("Student added successfully");
        }

        [HttpGet("GetAllStudents")]
        public async Task<IActionResult> GetAllStudents()
        {
            var students = await _unitOfWork.Repository<Student>()
                                             .GetAllAsync(includeProperties: "Enrollments.Course");

            if (students == null || !students.Any())
            {
                _logger.LogWarning("Students not found");
                return NotFound("No students found.");
            }

            var studentDtos = _mapper.Map<List<StudentWithCourseDTO>>(students);


            return Ok(studentDtos);
        }

        [HttpGet("GetById/{id}")]
        public async Task<IActionResult> GetStudentById(int id)
        {
            if (id <= 0)
            {
                _logger.LogWarning("Invalid Id");
                return BadRequest("Invalid ID.");
            }

            var student = await _unitOfWork.Repository<Student>()
                                           .GetById(id, includeProperties: "Enrollments.Course");

            if (student == null)
            {
                _logger.LogWarning("Student not found");
                return NotFound("Student not found.");
            }

            var studentDto = _mapper.Map<StudentWithCourseDTO>(student);

            return Ok(studentDto);
        }

        [Authorize(Roles = "Admin,Student")]
        [HttpPut("UpdateStudent")]
        public async Task<IActionResult> UpdateStudent([FromBody] StudentUpdateDTO studentUpdateDTO, [FromServices] IValidator<StudentUpdateDTO> validator)
        {
            if (studentUpdateDTO == null || studentUpdateDTO.Id <= 0)
            {
                _logger.LogWarning("Ivalid Data");
                return BadRequest("Invalid data.");
            }

            var existingStudent = await _unitOfWork.Repository<Student>().GetById(studentUpdateDTO.Id);
            if (existingStudent == null)
            {
                _logger.LogWarning("Student Not Found");
                return NotFound("Student not found.");
            }

            // Check email uniqueness (excluding current student)
            var allStudents = await _unitOfWork.Repository<Student>().GetAllAsync();
            if (allStudents.Any(s => s.Email.ToLower() == studentUpdateDTO.Email.ToLower() && s.Id != studentUpdateDTO.Id))
            {
                _logger.LogWarning("Email already exists.");
                return BadRequest("Email already exists.");
            }

            var validationResult = await validator.ValidateAsync(studentUpdateDTO);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.Errors);

            

            _mapper.Map(studentUpdateDTO, existingStudent);

            await _unitOfWork.Repository<Student>().Update(existingStudent);
            await _unitOfWork.CompleteAsync();

            return Ok("Student updated successfully.");
        }

        [Authorize(Roles ="Admin")]
        [HttpDelete("DeleteStudent/{id}")]
        public async Task<IActionResult> DeleteStudent(int id)
        {
            if (id <= 0)
                return BadRequest("Id is required.");

            var student = await _unitOfWork.Repository<Student>().GetById(id);

            if (student == null)
                return NotFound("Student not found.");

            await _unitOfWork.Repository<Student>().Delete(student);
            await _unitOfWork.CompleteAsync();

            return Ok("Student deleted successfully.");
        }

        [Authorize(Roles = "Admin,Student")]
        [HttpPost("AssignToCourse")]
        public async Task<IActionResult> AssignStudentToCourse([FromBody] EnrollmentDTO dto, [FromServices] IValidator<EnrollmentDTO> validator)
        {

            var student = await _unitOfWork.Repository<Student>().GetById(dto.StudentId);
            if (student == null)
                return NotFound("Student not found.");

            var course = await _unitOfWork.Repository<Course>().GetById(dto.CourseId);
            if (course == null)
                return NotFound("Course not found.");

            var validationResult = await validator.ValidateAsync(dto);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.Errors);


            var enrollment = _mapper.Map<Enrollment>(dto);
            await _unitOfWork.Repository<Enrollment>().AddAsync(enrollment);
            await _unitOfWork.CompleteAsync();

            return Ok("Student assigned to course successfully.");
        }


        [Authorize(Roles = "Admin,Student")]
        [HttpDelete("UnassignFromCourse")]
        public async Task<IActionResult> UnassignStudentFromCourse([FromBody] EnrollmentDTO dto)
        {
            if (dto.StudentId <= 0 || dto.CourseId <= 0)
                return BadRequest("StudentId and CourseId are required.");

            var enrollment = (await _unitOfWork.Repository<Enrollment>().GetAllAsync())
                             .FirstOrDefault(e => e.StudentId == dto.StudentId && e.CourseId == dto.CourseId);

            if (enrollment == null)
                return NotFound("Enrollment not found.");

            await _unitOfWork.Repository<Enrollment>().Delete(enrollment);
            await _unitOfWork.CompleteAsync();

            return Ok("Student unassigned from course successfully.");
        }

    }
}
