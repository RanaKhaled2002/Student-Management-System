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
    public class TeacherConfiguration : IEntityTypeConfiguration<Teacher>
    {
        public void Configure(EntityTypeBuilder<Teacher> builder)
        {
            builder.HasKey(T => T.Id);

            builder.Property(T => T.FullName)
                   .IsRequired()
                   .HasMaxLength(50);

            builder.Property(T => T.Department)
                   .IsRequired()
                   .HasMaxLength(50);

            builder.HasMany(T => T.Courses)
                   .WithOne(C => C.Teacher)
                   .HasForeignKey(C => C.TeacherId)
                   .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
