using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace LMS.Models.LMSModels
{
    public partial class Team9LMSContext : DbContext
    {
        public Team9LMSContext()
        {
        }

        public Team9LMSContext(DbContextOptions<Team9LMSContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Administrators> Administrators { get; set; }
        public virtual DbSet<AssignmentCategories> AssignmentCategories { get; set; }
        public virtual DbSet<Assignments> Assignments { get; set; }
        public virtual DbSet<Classes> Classes { get; set; }
        public virtual DbSet<Courses> Courses { get; set; }
        public virtual DbSet<Departments> Departments { get; set; }
        public virtual DbSet<Enrolled> Enrolled { get; set; }
        public virtual DbSet<Professors> Professors { get; set; }
        public virtual DbSet<Students> Students { get; set; }
        public virtual DbSet<Submission> Submission { get; set; }
        public virtual DbSet<Users> Users { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
                optionsBuilder.UseMySql("Server=atr.eng.utah.edu;User Id=u0513119;Password=kim123456789;Database=Team9LMS");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Administrators>(entity =>
            {
                entity.HasKey(e => e.UId)
                    .HasName("PRIMARY");

                entity.Property(e => e.UId)
                    .HasColumnName("uID")
                    .HasColumnType("char(8)");

                entity.HasOne(d => d.U)
                    .WithOne(p => p.Administrators)
                    .HasForeignKey<Administrators>(d => d.UId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("Admin_fk_1");
            });

            modelBuilder.Entity<AssignmentCategories>(entity =>
            {
                entity.HasKey(e => e.AcId)
                    .HasName("PRIMARY");

                entity.HasIndex(e => e.ClassId)
                    .HasName("ClassID");

                entity.HasIndex(e => new { e.Name, e.ClassId })
                    .HasName("Name")
                    .IsUnique();

                entity.Property(e => e.AcId).HasColumnName("acID");

                entity.Property(e => e.ClassId).HasColumnName("ClassID");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnType("varchar(100)");

                entity.HasOne(d => d.Class)
                    .WithMany(p => p.AssignmentCategories)
                    .HasForeignKey(d => d.ClassId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("AssignmentCategories_ibfk_1");
            });

            modelBuilder.Entity<Assignments>(entity =>
            {
                entity.HasKey(e => e.AId)
                    .HasName("PRIMARY");

                entity.HasIndex(e => e.AcId)
                    .HasName("acID");

                entity.Property(e => e.AId).HasColumnName("aID");

                entity.Property(e => e.AcId).HasColumnName("acID");

                entity.Property(e => e.Contents).HasColumnType("varchar(8192)");

                entity.Property(e => e.Due).HasColumnType("datetime");

                entity.Property(e => e.Name).HasColumnType("varchar(100)");

                entity.HasOne(d => d.Ac)
                    .WithMany(p => p.Assignments)
                    .HasForeignKey(d => d.AcId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("Assignments_ibfk_1");
            });

            modelBuilder.Entity<Classes>(entity =>
            {
                entity.HasKey(e => e.ClassId)
                    .HasName("PRIMARY");

                entity.HasIndex(e => e.CatalogId)
                    .HasName("CatalogID");

                entity.HasIndex(e => e.UId)
                    .HasName("uID");

                entity.HasIndex(e => new { e.Semester, e.CatalogId })
                    .HasName("Semester")
                    .IsUnique();

                entity.Property(e => e.ClassId).HasColumnName("ClassID");

                entity.Property(e => e.CatalogId).HasColumnName("CatalogID");

                entity.Property(e => e.EndTime).HasColumnType("time");

                entity.Property(e => e.Location).HasColumnType("varchar(100)");

                entity.Property(e => e.Semester)
                    .IsRequired()
                    .HasColumnType("varchar(16)");

                entity.Property(e => e.StartTime).HasColumnType("time");

                entity.Property(e => e.UId)
                    .IsRequired()
                    .HasColumnName("uID")
                    .HasColumnType("char(8)");

                entity.HasOne(d => d.Catalog)
                    .WithMany(p => p.Classes)
                    .HasForeignKey(d => d.CatalogId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("Classes_ibfk_1");

                entity.HasOne(d => d.U)
                    .WithMany(p => p.Classes)
                    .HasForeignKey(d => d.UId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("Classes_ibfk_2");
            });

            modelBuilder.Entity<Courses>(entity =>
            {
                entity.HasKey(e => e.CatalogId)
                    .HasName("PRIMARY");

                entity.HasIndex(e => e.Subject)
                    .HasName("Subject");

                entity.Property(e => e.CatalogId).HasColumnName("CatalogID");

                entity.Property(e => e.Name).HasColumnType("varchar(100)");

                entity.Property(e => e.Subject)
                    .IsRequired()
                    .HasColumnType("varchar(4)");

                entity.HasOne(d => d.SubjectNavigation)
                    .WithMany(p => p.Courses)
                    .HasForeignKey(d => d.Subject)
                    .HasConstraintName("Courses_ibfk_1");
            });

            modelBuilder.Entity<Departments>(entity =>
            {
                entity.HasKey(e => e.Subject)
                    .HasName("PRIMARY");

                entity.Property(e => e.Subject).HasColumnType("varchar(4)");

                entity.Property(e => e.DepartmentName).HasColumnType("varchar(100)");
            });

            modelBuilder.Entity<Enrolled>(entity =>
            {
                entity.HasKey(e => new { e.ClassId, e.UId })
                    .HasName("PRIMARY");

                entity.HasIndex(e => e.UId)
                    .HasName("uID");

                entity.Property(e => e.ClassId).HasColumnName("ClassID");

                entity.Property(e => e.UId)
                    .HasColumnName("uID")
                    .HasColumnType("char(8)");

                entity.Property(e => e.Grade).HasColumnType("varchar(2)");

                entity.HasOne(d => d.Class)
                    .WithMany(p => p.Enrolled)
                    .HasForeignKey(d => d.ClassId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("Enrolled_ibfk_1");

                entity.HasOne(d => d.U)
                    .WithMany(p => p.Enrolled)
                    .HasForeignKey(d => d.UId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("Enrolled_ibfk_2");
            });

            modelBuilder.Entity<Professors>(entity =>
            {
                entity.HasKey(e => e.UId)
                    .HasName("PRIMARY");

                entity.HasIndex(e => e.Subject)
                    .HasName("Subject");

                entity.Property(e => e.UId)
                    .HasColumnName("uID")
                    .HasColumnType("char(8)");

                entity.Property(e => e.Subject)
                    .IsRequired()
                    .HasColumnType("varchar(4)");

                entity.HasOne(d => d.SubjectNavigation)
                    .WithMany(p => p.Professors)
                    .HasForeignKey(d => d.Subject)
                    .HasConstraintName("Professors_ibfk_2");

                entity.HasOne(d => d.U)
                    .WithOne(p => p.Professors)
                    .HasForeignKey<Professors>(d => d.UId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("Professors_ibfk_1");
            });

            modelBuilder.Entity<Students>(entity =>
            {
                entity.HasKey(e => e.UId)
                    .HasName("PRIMARY");

                entity.HasIndex(e => e.Subject)
                    .HasName("Subject");

                entity.Property(e => e.UId)
                    .HasColumnName("uID")
                    .HasColumnType("char(8)");

                entity.Property(e => e.Subject)
                    .IsRequired()
                    .HasColumnType("varchar(4)");

                entity.HasOne(d => d.SubjectNavigation)
                    .WithMany(p => p.Students)
                    .HasForeignKey(d => d.Subject)
                    .HasConstraintName("Students_ibfk_2");

                entity.HasOne(d => d.U)
                    .WithOne(p => p.Students)
                    .HasForeignKey<Students>(d => d.UId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("Students_ibfk_1");
            });

            modelBuilder.Entity<Submission>(entity =>
            {
                entity.HasKey(e => new { e.AId, e.UId })
                    .HasName("PRIMARY");

                entity.HasIndex(e => e.UId)
                    .HasName("uID");

                entity.Property(e => e.AId).HasColumnName("aID");

                entity.Property(e => e.UId)
                    .HasColumnName("uID")
                    .HasColumnType("char(8)");

                entity.Property(e => e.Contents).HasColumnType("varchar(8192)");

                entity.Property(e => e.Time).HasColumnType("datetime");

                entity.HasOne(d => d.A)
                    .WithMany(p => p.Submission)
                    .HasForeignKey(d => d.AId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("Submission_ibfk_1");

                entity.HasOne(d => d.U)
                    .WithMany(p => p.Submission)
                    .HasForeignKey(d => d.UId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("Submission_ibfk_2");
            });

            modelBuilder.Entity<Users>(entity =>
            {
                entity.HasKey(e => e.UId)
                    .HasName("PRIMARY");

                entity.Property(e => e.UId)
                    .HasColumnName("uID")
                    .HasColumnType("char(8)");

                entity.Property(e => e.Dob)
                    .HasColumnName("DOB")
                    .HasColumnType("date");

                entity.Property(e => e.FirstName).HasColumnType("varchar(100)");

                entity.Property(e => e.LastName).HasColumnType("varchar(100)");
            });
        }
    }
}
