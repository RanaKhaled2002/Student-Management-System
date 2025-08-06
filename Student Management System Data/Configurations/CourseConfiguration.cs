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
    public class CourseConfiguration : IEntityTypeConfiguration<Course>
    {
        public void Configure(EntityTypeBuilder<Course> builder)
        {
            builder.HasKey(C => C.Id);

            builder.Property(C => C.Title)
                   .IsRequired()
                   .HasMaxLength(50);

            builder.Property(C => C.Description)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.HasMany(C => C.Enrollments)
                   .WithOne(E => E.Course)
                   .HasForeignKey(E => E.CourseId);

            builder.HasOne(C => C.Teacher)
                   .WithMany(T => T.Courses)
                   .HasForeignKey(C => C.TeacherId)
                   .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
