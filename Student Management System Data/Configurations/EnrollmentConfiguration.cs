using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Student_Management_System_Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Student_Management_System_Data.Configurations
{
    public class EnrollmentConfiguration : IEntityTypeConfiguration<Enrollment>
    {
        public void Configure(EntityTypeBuilder<Enrollment> builder)
        {
            builder.HasKey(E => E.Id);

            builder.Property(E => E.Grade)
                   .HasPrecision(5, 2);

            builder.HasOne(E => E.Student)
                   .WithMany(S => S.Enrollments)
                   .HasForeignKey(E => E.StudentId);

            builder.HasOne(E => E.Course)
                   .WithMany(C => C.Enrollments)
                   .HasForeignKey(E => E.CourseId);
        }
    }
}
