using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Student_Management_System_Data.DTOs.Course;
using Student_Management_System_Data.DTOs.Enrollment;
using Student_Management_System_Data.DTOs.Student;
using Student_Management_System_Data.DTOs.Teacher;
using Student_Management_System_Data.Models;
using Student_Management_System_Logic.Interfaces;

namespace Student_Management_System_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CourseController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CourseController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        [Authorize(Roles ="Admin")]
        [HttpPost("AddCourse")]
        public async Task<IActionResult> AddCourse([FromBody] CourseDTO courseDTO, [FromServices] IValidator<CourseDTO> validator)
        {
            if (courseDTO is null)
                return BadRequest("Course Data is null");

            var validationResult = await validator.ValidateAsync(courseDTO);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.Errors);

            var course = _mapper.Map<Course>(courseDTO);


            await _unitOfWork.Repository<Course>().AddAsync(course);
            await _unitOfWork.CompleteAsync();

            return Ok("Course added successfully.");
        }

        [HttpGet("GetAllCourses")]
        public async Task<IActionResult> GetAllCourses()
        {
            var courses = await _unitOfWork.Repository<Course>()
                                   .GetAllAsync(includeProperties: "Teacher");

            if (courses == null || !courses.Any())
                return NotFound("No courses found.");

            var courseDtos = _mapper.Map<List<CourseWithTeacherDTO>>(courses);

            return Ok(courseDtos);
        }

        [HttpGet("GetById/{id}")]
        public async Task<IActionResult> GetCourseById(int id)
        {
            if (id <= 0)
                return BadRequest("Enter correct ID.");

            var course = await _unitOfWork.Repository<Course>()
                                           .GetById(id, includeProperties: "Teacher");

            if (course == null)
                return NotFound("Course not found.");

            var courseDto = _mapper.Map<CourseWithTeacherDTO>(course);

            return Ok(courseDto);
        }

        [Authorize(Roles ="Admin")]
        [HttpPut("UpdateCourse")]
        public async Task<IActionResult> UpdateCourse([FromBody] CourseUpdateDTO courseUpdateDto, [FromServices] IValidator<CourseUpdateDTO> validator)
        {
            if (courseUpdateDto == null || courseUpdateDto.Id <= 0)
                return BadRequest("Invalid Data");

            var validationResult = await validator.ValidateAsync(courseUpdateDto);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.Errors);

            var exisitingCourse = await _unitOfWork.Repository<Course>().GetById(courseUpdateDto.Id);

            if (exisitingCourse == null)
                return NotFound("Course not found");

           
            _mapper.Map(courseUpdateDto, exisitingCourse);

            await _unitOfWork.Repository<Course>().Update(exisitingCourse);
            await _unitOfWork.CompleteAsync();

            return Ok("Course updated successfully.");
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("DeleteCourse/{id}")]
        public async Task<IActionResult> DeleteCourse(int id)
        {
            if (id <= 0)
                return BadRequest("Id is required.");

            var course = await _unitOfWork.Repository<Course>().GetById(id);

            if (course == null)
                return NotFound("Course not found.");

            await _unitOfWork.Repository<Course>().Delete(course);
            await _unitOfWork.CompleteAsync();

            return Ok("Course deleted successfully.");
        }

        [Authorize(Roles ="Admin,Teacher")]
        [HttpGet("ViewStudents/{courseId}")]
        public async Task<IActionResult> ViewStudentAssignedToCourse(int courseId)
        {
            if (courseId <= 0) return BadRequest("Enter Valid Id");

            var course = await _unitOfWork.Repository<Course>().GetById(courseId,includeProperties:"Enrollments.Student");

            if (course == null) return NotFound("Course not found");

            var students = course.Enrollments.Select(e => e.Student).ToList();

            if (students == null || !students.Any())
                return BadRequest("No students assigned to this course.");

            var studentDtos = _mapper.Map<List<StudentDTO>>(students);

            return Ok(studentDtos);

        }

        [HttpGet("ViewTeacher/{courseId}")]
        public async Task<IActionResult> ViewTeacherAssignedToCourse(int courseId)
        {
            if (courseId <= 0)
                return BadRequest("Enter valid course ID.");

            var course = await _unitOfWork.Repository<Course>()
                                           .GetById(courseId, includeProperties: "Teacher");

            if (course == null)
                return NotFound("Course not found.");

            if (course.Teacher == null)
                return BadRequest("No teacher assigned to this course.");

            var teacherDto = _mapper.Map<TeacherDTO>(course.Teacher);

            return Ok(teacherDto);
        }

    }
}
