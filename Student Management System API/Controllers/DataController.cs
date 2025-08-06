using AutoMapper;
using CsvHelper;
using CsvHelper.TypeConversion;
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
using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace Student_Management_System_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles ="Admin")]
    public class DataController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly StudentDbContext _studentDbContxet;

        public DataController(IUnitOfWork unitOfWork, IMapper mapper,StudentDbContext studentDbContxet)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _studentDbContxet = studentDbContxet;
        }

        [HttpGet("ExportStudents")]
        public async Task<IActionResult> ExportStudentsToCSV()
        {
            var students = await _unitOfWork.Repository<Student>().GetAllAsync();
            if (students == null || !students.Any())
                return NotFound("No students found.");

            var studentDto = _mapper.Map<List<StudentDTO>>(students);


            // مكان مؤقت في الذاكرة عشان نكتب فيه ملف csv
            using var memoryStream = new MemoryStream();

            // بكتب النص داخل memoryStream
            using var writer = new StreamWriter(memoryStream, Encoding.UTF8);

            // كتابة البيانات بصيغة CSV
            using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

            // نكتب كل بيانات الطلاب (كائنات Student كاملة)
            csv.WriteRecords(studentDto);

            // حفظ البيانات
            writer.Flush();

            // نرجع الملف المطلوب
            return File(memoryStream.ToArray(), "text/csv", "students.csv");
        }

        [HttpPost("ImportStudents")]
        public async Task<IActionResult> ImportStudentFromCSV(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Please upload a valid CSV file.");

            using var stream = new StreamReader(file.OpenReadStream());
            using var csv = new CsvReader(stream, CultureInfo.InvariantCulture);

            List<StudentDTO> studentDto;
            try
            {
                studentDto = csv.GetRecords<StudentDTO>().ToList();
            }
            catch
            {
                return BadRequest("All fields are required & make sure DOB is in format dd/MM/yyyy t:tt.");
            }

            // لو فيه اي ايميلات متكرره 
            var duplicateEmailsInCSV = studentDto
                .GroupBy(s => s.Email.ToLower()) // بجمع كل الطلبه
                .Where(g => g.Count() > 1) // جروب اللي فيه اكتر من طالب بنفس الايميل
                .Select(g => g.Key) // بياخد قيمه الايميل المتكرر
                .ToList();

            if (duplicateEmailsInCSV.Any())
            {
                var message = string.Join(" | ", duplicateEmailsInCSV.Select(e => $"Duplicate email in CSV: {e}"));
                return BadRequest(message);
            }

            var invalidRecords = new List<string>();

            foreach (var student in studentDto)
            {
                var context = new ValidationContext(student);
                var validationResults = new List<ValidationResult>();
                bool isValid = Validator.TryValidateObject(student, context, validationResults, true);

                if (!isValid)
                {
                    var errors = string.Join(" | ", validationResults.Select(e => e.ErrorMessage));
                    invalidRecords.Add($" {student.FullName} → {errors}");
                }
            }

            if (invalidRecords.Any())
            {
                return BadRequest(string.Join("\n", invalidRecords));
            }

            foreach (var student in studentDto)
            {
                var existingStudent = await _studentDbContxet.Students
                    .FirstOrDefaultAsync(s => s.Email.ToLower() == student.Email.ToLower());

                if (existingStudent != null)
                {
                    if (existingStudent.FullName != student.FullName || existingStudent.DOB != student.DOB)
                    {
                        existingStudent.FullName = student.FullName;
                        existingStudent.DOB = student.DOB;
                        await _unitOfWork.Repository<Student>().Update(existingStudent);
                    }
                }
                else
                {
                    var studentData = _mapper.Map<Student>(student);
                    await _unitOfWork.Repository<Student>().AddAsync(studentData);
                }
            }

            await _unitOfWork.CompleteAsync();
            return Ok("Students imported and updated successfully.");
        }

        [HttpGet("ExportTeachers")]
        public async Task<IActionResult> ExportTeacherToCSV()
        {
            var teachers = await _unitOfWork.Repository<Teacher>().GetAllAsync();
            if (teachers == null || !teachers.Any())
                return NotFound("No teachers found.");



            // مكان مؤقت في الذاكرة عشان نكتب فيه ملف csv
            using var memoryStream = new MemoryStream();

            // بكتب النص داخل memoryStream
            using var writer = new StreamWriter(memoryStream, Encoding.UTF8);

            // كتابة البيانات بصيغة CSV
            using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

            // نكتب كل بيانات الطلاب (كائنات Teacher كاملة)
            csv.WriteRecords(teachers);

            // حفظ البيانات
            writer.Flush();

            // نرجع الملف المطلوب
            return File(memoryStream.ToArray(), "text/csv", "teachers.csv");
        }

        [HttpPost("ImportTeachers")]
        public async Task<IActionResult> ImportTeacherFromCSV(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Please upload a valid CSV file.");

            using var stream = new StreamReader(file.OpenReadStream());
            using var csv = new CsvReader(stream, CultureInfo.InvariantCulture);

            List<ImportTeacherDTO> teachers;
            try
            {
                teachers = csv.GetRecords<ImportTeacherDTO>().ToList();
            }
            catch
            {
                return BadRequest("All fields are required except id");
            }

            var invalidRecords = new List<string>();

            foreach (var teacher in teachers)
            {
                var context = new ValidationContext(teacher);
                var validationResults = new List<ValidationResult>();
                bool isValid = Validator.TryValidateObject(teacher, context, validationResults, true);

                if (!isValid)
                {
                    var errors = string.Join(" | ", validationResults.Select(e => e.ErrorMessage));
                    invalidRecords.Add($" {teacher.FullName} → {errors}");
                }
            }

            if (invalidRecords.Any())
            {
                return BadRequest(string.Join("\n", invalidRecords));
            }

            foreach (var teacher in teachers)
            {
                var existingTeacher = await _studentDbContxet.Teachers
                    .FirstOrDefaultAsync(s => s.Id == teacher.Id);

                if (existingTeacher != null)
                {
                    if (existingTeacher.FullName != teacher.FullName || existingTeacher.Department != teacher.Department)
                    {
                        existingTeacher.FullName = teacher.FullName;
                        existingTeacher.Department = teacher.Department;
                        await _unitOfWork.Repository<Teacher>().Update(existingTeacher);
                    }
                }
                else
                {
                    var teacherData = _mapper.Map<Teacher>(teacher);

                    await _unitOfWork.Repository<Teacher>().AddAsync(teacherData);
                }
            }

            await _unitOfWork.CompleteAsync();
            return Ok("Teachers imported and updated successfully.");
        }

        [HttpGet("ExportCourses")]
        public async Task<IActionResult> ExportCoursesToCSV()
        {
            var courses = await _unitOfWork.Repository<Course>().GetAllAsync();
            if (courses == null || !courses.Any())
                return NotFound("No Courses found.");

            var CourseDto = _mapper.Map<List<ImportCourseDTO>>(courses);

            // مكان مؤقت في الذاكرة عشان نكتب فيه ملف csv
            using var memoryStream = new MemoryStream();

            // بكتب النص داخل memoryStream
            using var writer = new StreamWriter(memoryStream, Encoding.UTF8);

            // كتابة البيانات بصيغة CSV
            using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

            // نكتب كل بيانات الطلاب (كائنات Course كاملة)
            csv.WriteRecords(CourseDto);

            // حفظ البيانات
            writer.Flush();

            // نرجع الملف المطلوب
            return File(memoryStream.ToArray(), "text/csv", "courses.csv");
        }

        [HttpPost("ImportCourses")]
        public async Task<IActionResult> ImportCourseFromCSV(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Please upload a valid CSV file.");

            using var stream = new StreamReader(file.OpenReadStream());
            using var csv = new CsvReader(stream, CultureInfo.InvariantCulture);

            List<ImportCourseDTO> courses;
            try
            {
                courses = csv.GetRecords<ImportCourseDTO>().ToList();
            }
            catch
            {
                return BadRequest("All fields are required except id");
            }

            var invalidRecords = new List<string>();

            foreach (var course in courses)
            {
                var context = new ValidationContext(course);
                var validationResults = new List<ValidationResult>();
                bool isValid = Validator.TryValidateObject(course, context, validationResults, true);

                if (!isValid)
                {
                    var errors = string.Join(" | ", validationResults.Select(e => e.ErrorMessage));
                    invalidRecords.Add($" {course.Title} → {errors}");
                }
            }

            if (invalidRecords.Any())
            {
                return BadRequest(string.Join("\n", invalidRecords));
            }

            foreach (var course in courses)
            {
                var existingCourse = await _studentDbContxet.Courses
                    .FirstOrDefaultAsync(s => s.Id == course.Id);

                if (existingCourse != null)
                {
                    if (existingCourse.Title != course.Title || existingCourse.Description != course.Description)
                    {
                        existingCourse.Title = course.Title;
                        existingCourse.Description = course.Description;
                        await _unitOfWork.Repository<Course>().Update(existingCourse);
                    }
                }
                else
                {
                    var CourseData = _mapper.Map<Course>(course);

                    await _unitOfWork.Repository<Course>().AddAsync(CourseData);
                }
            }

            await _unitOfWork.CompleteAsync();
            return Ok("Courses imported and updated successfully.");
        }

        [HttpGet("ExportGrades")]
        public async Task<IActionResult> ExportGradesToCSV()
        {
            var grades = await _unitOfWork.Repository<Enrollment>().GetAllAsync();
            if (grades == null || !grades.Any())
                return NotFound("No Enrollment found.");

            var gradeDto = _mapper.Map<List<ImportGradeDTO>>(grades);

            // مكان مؤقت في الذاكرة عشان نكتب فيه ملف csv
            using var memoryStream = new MemoryStream();

            // بكتب النص داخل memoryStream
            using var writer = new StreamWriter(memoryStream, Encoding.UTF8);

            // كتابة البيانات بصيغة CSV
            using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

            // نكتب كل بيانات الطلاب (كائنات Grades كاملة)
            csv.WriteRecords(gradeDto);

            // حفظ البيانات
            writer.Flush();

            // نرجع الملف المطلوب
            return File(memoryStream.ToArray(), "text/csv", "grades.csv");
        }

        [HttpPost("ImportGrades")]
        public async Task<IActionResult> ImportGradesFromCSV(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Please upload a valid CSV file.");

            using var stream = new StreamReader(file.OpenReadStream());
            using var csv = new CsvReader(stream, CultureInfo.InvariantCulture);

            List<ImportGradeDTO> gradeDto;
            try
            {
                gradeDto = csv.GetRecords<ImportGradeDTO>().ToList();
            }
            catch
            {
                return BadRequest("All fields are required.");
            }

            var invalidRecords = new List<string>();

            foreach (var grade in gradeDto)
            {
                var context = new ValidationContext(grade);
                var validationResults = new List<ValidationResult>();
                bool isValid = Validator.TryValidateObject(grade, context, validationResults, true);

                if (!isValid)
                {
                    var errors = string.Join(" | ", validationResults.Select(e => e.ErrorMessage));
                    invalidRecords.Add($"StudentId: {grade.StudentId}, CourseId: {grade.CourseId} → {errors}");
                    continue;
                }

                bool studentExists = await _studentDbContxet.Students.AnyAsync(s => s.Id == grade.StudentId);
                bool courseExists = await _studentDbContxet.Courses.AnyAsync(c => c.Id == grade.CourseId);

                if (!studentExists || !courseExists)
                {
                    invalidRecords.Add($"StudentId: {grade.StudentId}, CourseId: {grade.CourseId} → Student or Course not found.");
                }
            }

            if (invalidRecords.Any())
                return BadRequest(string.Join("\n", invalidRecords));

            foreach (var grade in gradeDto)
            {
                var existing = await _studentDbContxet.Enrollments
                    .FirstOrDefaultAsync(t => t.StudentId == grade.StudentId && t.CourseId == grade.CourseId);

                if (existing != null)
                {
                    if (existing.Grade != grade.Grade)
                    {
                        existing.Grade = grade.Grade;
                        await _unitOfWork.Repository<Enrollment>().Update(existing);
                    }
                }
                else
                {
                    var gradeData = _mapper.Map<Enrollment>(grade);
                    await _unitOfWork.Repository<Enrollment>().AddAsync(gradeData);
                }
            }

            await _unitOfWork.CompleteAsync();
            return Ok("Grades imported and updated successfully.");
        }

    }
}
