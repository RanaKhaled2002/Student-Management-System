using Student_Management_System_Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Student_Management_System_Data.Configurations
{
    public class StudentConfiguration : IEntityTypeConfiguration<Student>
    {
        public void Configure(EntityTypeBuilder<Student> builder)
        {
            builder.HasKey(S => S.Id);

            builder.Property(S => S.FullName)
                   .IsRequired()
                   .HasMaxLength(50);

            builder.Property(S => S.Email)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.HasIndex(S => S.Email)
                   .IsUnique();

            builder.Property(S => S.DOB)
                   .IsRequired();

            builder.HasMany(S => S.Enrollments)
                   .WithOne(E => E.Student)
                   .HasForeignKey(E => E.StudentId);
        }
    }
}
