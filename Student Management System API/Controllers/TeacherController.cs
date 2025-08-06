using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Student_Management_System_Data.Data;
using Student_Management_System_Data.DTOs.Course;
using Student_Management_System_Data.DTOs.Enrollment;
using Student_Management_System_Data.DTOs.Student;
using Student_Management_System_Data.DTOs.Teacher;
using Student_Management_System_Data.Models;
using Student_Management_System_Logic.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace Student_Management_System_API.Controllers
{
    [Route("api/Teacher")]
    [ApiController]
    [Authorize]
    public class TeacherController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;


        public TeacherController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        [Authorize(Roles ="Admin,Teacher")]
        [HttpPost("AddTeacher")]
        public async Task<IActionResult> AddTeacher([FromBody]TeacherCreateDTO teacherCreateDTO, [FromServices] IValidator<TeacherCreateDTO> validator)
        {
            if (teacherCreateDTO is null) return BadRequest("Teacher Data is null");

            var validationResult = await validator.ValidateAsync(teacherCreateDTO);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.Errors);

            var entity = _mapper.Map<Teacher>(teacherCreateDTO);

            await _unitOfWork.Repository<Teacher>().AddAsync(entity);
            await _unitOfWork.CompleteAsync();

            return Ok("Teacher Added Successfully");
        }

        [HttpGet("GetAllTeachers")]
        public async Task<IActionResult> GetAllTeachers()
        {
            var teachers = await _unitOfWork.Repository<Teacher>()
                                             .GetAllAsync(includeProperties: "Courses");

            if (teachers == null || !teachers.Any())
                return NotFound("No teachers found.");

            var teacherDtos = _mapper.Map<List<TeacherWithCoursesDTO>>(teachers);

            return Ok(teacherDtos);
        }

        [HttpGet("GetById/{id}")]
        public async Task<IActionResult> GetTeacherById(int id)
        {
            if (id <= 0) return BadRequest("Enter correct id.");

            var teacher = await _unitOfWork.Repository<Teacher>()
                                            .GetById(id, includeProperties: "Courses");

            if (teacher == null)
                return NotFound("Teacher not found");

            var teacherDto = _mapper.Map<TeacherWithCoursesDTO>(teacher);

            return Ok(teacherDto);
        }

        [Authorize(Roles ="Admin,Teacher")]
        [HttpPut("UpdateTeacher")]
        public async Task<IActionResult> UpdateTeacher([FromBody]TeacherUpdateDTO teacherUpdateDto, [FromServices] IValidator<TeacherUpdateDTO> validator)
        {
            if (teacherUpdateDto == null || teacherUpdateDto.Id <= 0) return BadRequest("Invalid Data");


            var exisitingTeacher = await _unitOfWork.Repository<Teacher>().GetById(teacherUpdateDto.Id);

            if (exisitingTeacher == null) return NotFound("Teacher Not found");

            var validationResult = await validator.ValidateAsync(teacherUpdateDto);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.Errors);


            _mapper.Map(teacherUpdateDto, exisitingTeacher);

            await _unitOfWork.Repository<Teacher>().Update(exisitingTeacher);
            await _unitOfWork.CompleteAsync();

            return Ok("Teacher updated successfully.");
        }

        [Authorize(Roles ="Admin")]
        [HttpDelete("DeleteTeacher/{id}")]
        public async Task<IActionResult> DeleteTeacher(int id)
        {
            if (id <= 0)
                return BadRequest("Id is required.");

            var teacher = await _unitOfWork.Repository<Teacher>().GetById(id);

            if (teacher == null)
                return NotFound("teacher not found.");

            await _unitOfWork.Repository<Teacher>().Delete(teacher);
            await _unitOfWork.CompleteAsync();

            return Ok("Teacher deleted successfully.");
        }

        [Authorize(Roles ="Admin,Teacher")]
        [HttpPost("AssignCourse")]
        public async Task<IActionResult> AssignCourse([FromBody] AssignCourseDTO assignCourseDTO, [FromServices] IValidator<AssignCourseDTO> validator)
        {
            if(assignCourseDTO.CourseId <=0 || assignCourseDTO.TeacherId <=0)
            { return BadRequest("Please Assign Correct Id"); }

            var teacher = await _unitOfWork.Repository<Teacher>().GetById(assignCourseDTO.TeacherId);

            if (teacher == null) return NotFound("Teacher not Found");

            var course = await _unitOfWork.Repository<Course>().GetById(assignCourseDTO.CourseId);

            if (course == null) return NotFound("Course Not Found");

            var validationResult = await validator.ValidateAsync(assignCourseDTO);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.Errors);



            course.TeacherId = teacher.Id;

            await _unitOfWork.Repository<Course>().Update(course);
            await _unitOfWork.CompleteAsync();

            return Ok($"Course '{course.Title}' assigned to teacher '{teacher.FullName}' successfully.");
        }

        [Authorize(Roles ="Admin")]
        [HttpPost("UnassignTeacherFromCourse")]
        public async Task<IActionResult> UnassignTeacherFromCourse([FromBody] UnassignTeacherDTO dto)
        {
            if (dto.CourseId <= 0 || dto.TeacherId <= 0)
                return BadRequest("CourseId and TeacherId are required.");

            var course = await _unitOfWork.Repository<Course>().GetById(dto.CourseId);

            if (course == null)
                return NotFound("Course not found.");

            if (course.TeacherId != dto.TeacherId)
                return BadRequest("This course is not assigned to the specified teacher.");

            course.TeacherId = null;

            await _unitOfWork.Repository<Course>().Update(course);
            await _unitOfWork.CompleteAsync();

            return Ok($"Teacher unassigned from course '{course.Title}' successfully.");
        }

    }
}
