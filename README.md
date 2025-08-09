# 🎓 Student Management System (Backend Only)

A backend-only Student Management System built with **.NET 8**, following a clean 3-tier architecture. This system provides core functionalities for managing students, teachers, courses, grades, and includes features such as identity with role-based access, data import/export (CSV/Excel), and logging.

---

## 🧩 Tech Stack

- **Framework:** ASP.NET Core Web API (.NET 8)
- **Language:** C#
- **ORM:** Entity Framework Core
- **Database:** SQL Server / SQLite
- **Mapping Tool:** AutoMapper
- **Validation:** FluentValidation
- **Authentication:** ASP.NET Core Identity
- **Authorization Roles:** `Admin`, `Teacher`, `Student`
- **Logging:** Serilog
- **Import/Export:** EPPlus / CsvHelper
- **Testing:** xUnit (Unit Testing)
- **IDE:** Visual Studio / VS Code

---


---

## 📚 Features

### 🧑‍🎓 Student Management
- Add, update, delete student
- Get all students or by ID
- Assign student to course
- Unassign student from course

### 👨‍🏫 Teacher Management
- Add, update, delete teacher
- Get all teachers or by ID
- Assign teacher to course
- Unassign teacher from course

### 📘 Course Management
- Create, update, delete course
- Get all courses or by ID
- View students and teacher assigned to course

### 📝 Grades & Performance
- Add/edit grades per student per course
- Get all grades
- View grades by student or course
- delete grade from specific coure and specific student

### 📁 Data Export
- Export students, teachers, courses, and grades to CSV

### 📂 Data Import
- Import students, teachers, courses, and grades from CSVl with validation

---

## 🔐 Authentication & Authorization

- Integrated **ASP.NET Identity**
- Roles:
  - `Admin`: Full access to all features
  - `Teacher`: Access to (Add teacher, update teacher, Assign teacher to course, View student assigned to course, Enrollment controller)
  - `Student`: Access to (Add student, update student, Assign/unassign student to course, view student assigned to course )

---

## ✅ Validation & Testing

- **FluentValidation** used for validating input DTOs.
- **xUnit** used for unit testing service and repository layers.

---

## 📊 Logging

- **Serilog** for structured and centralized logging

---

## 🧪 Sample Import/Export Files

Sample CSV/Excel templates are included for:

- Students
- Teachers
- Courses
- Grades

---

## 🛠️ Setup Instructions

1. Clone the repository
2. Update connection string in `appsettings.json`
3. Run migrations:


## Project Documention
- https://www.postman.com/navigation-geoscientist-26997479/projects/collection/dmgmtae/student-management-system?action=share&creator=31992113


