using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Student_Management_System_Data.Data;
using Student_Management_System_Data.DTOs.Enrollment;
using Student_Management_System_Data.DTOs.Student;
using Student_Management_System_Data.Models;
using Student_Management_System_Logic.Interfaces;

namespace Student_Management_System_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles ="Admin,Teacher")]
    public class EnrollmentController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly StudentDbContext _studentDbContext;

        public EnrollmentController(IUnitOfWork unitOfWork, IMapper mapper,StudentDbContext studentDbContext)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _studentDbContext = studentDbContext;
        }

        [HttpGet("GetAllGrades")]
        public async Task<IActionResult> GetAllGrades()
        {
            var grades = await _unitOfWork.Repository<Enrollment>().GetAllAsync();

            if (grades == null || !grades.Any())
                return NotFound("No Grades found.");

            var gradeDtos = _mapper.Map<List<AddOrUpdateGradeDTO>>(grades);


            return Ok(gradeDtos);
        }

        [HttpPut("AddOrUpdateGrade")]
        public async Task<IActionResult> AddOrUpdateGrade([FromBody] AddOrUpdateGradeDTO dto)
        {
            if (dto.StudentId <= 0 || dto.CourseId <= 0)
                return BadRequest("StudentId and CourseId are required.");

            var enrollments = await _unitOfWork.Repository<Enrollment>().GetAllAsync();

            var enrollment = enrollments.FirstOrDefault(e =>
                e.StudentId == dto.StudentId && e.CourseId == dto.CourseId);

            if (enrollment == null)
                return NotFound("Enrollment not found. Assign the student to the course first.");

            enrollment.Grade = dto.Grade;

            await _unitOfWork.Repository<Enrollment>().Update(enrollment);
            await _unitOfWork.CompleteAsync();

            return Ok("Grade added/updated successfully.");
        }

        [HttpDelete("DeleteGrade/{studentId}/{courseId}")]
        public async Task<IActionResult> DeleteStudent(int studentId, int courseId)
        {
            if (studentId <= 0 || courseId <= 0)
                return BadRequest("Invalid student or course ID.");

            var enrollment = await _studentDbContext.Enrollments
                .FirstOrDefaultAsync(e => e.StudentId == studentId && e.CourseId == courseId);

            if (enrollment == null)
                return NotFound("Grade not found for this student and course.");

            _studentDbContext.Enrollments.Remove(enrollment);
            await _unitOfWork.CompleteAsync();

            return Ok("Grade deleted successfully.");
        }

        [HttpGet("GetGradesByStudent/{studentId}")]
        public async Task<IActionResult> GetGradesByStudent(int studentId)
        {
            if (studentId <= 0)
                return BadRequest("Enter valid student ID.");

            var enrollments = await _unitOfWork.Repository<Enrollment>()
                .GetAllAsync("Course");

            var studentGrades = enrollments
                .Where(e => e.StudentId == studentId)
                .Select(e => new
                {
                    CourseId = e.CourseId,
                    CourseTitle = e.Course.Title,
                    Grade = e.Grade
                }).ToList();

            if (!studentGrades.Any())
                return BadRequest("No grades found for this student.");

            return Ok(studentGrades);
        }

        [HttpGet("GetGradesByCourse/{courseId}")]
        public async Task<IActionResult> GetGradesByCourse(int courseId)
        {
            if (courseId <= 0)
                return BadRequest("Enter valid course ID.");

            var enrollments = await _unitOfWork.Repository<Enrollment>()
                .GetAllAsync("Student");

            var courseGrades = enrollments
                .Where(e => e.CourseId == courseId)
                .Select(e => new
                {
                    StudentId = e.StudentId,
                    StudentName = e.Student.FullName,
                    Grade = e.Grade
                }).ToList();

            if (!courseGrades.Any())
                return BadRequest("No students or grades found for this course.");

            return Ok(courseGrades);
        }

    }
}
