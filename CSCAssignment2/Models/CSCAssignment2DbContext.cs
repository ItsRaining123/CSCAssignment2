using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using CSCAssignment2.Models;
using CSCAssignment2.Services;
using CSCAssignment2.Helpers;


namespace ExamScriptTS.Models
{
    public partial class CSCAssignment2DbContext : DbContext
    {
        public CSCAssignment2DbContext()
        {
        }

        public CSCAssignment2DbContext(DbContextOptions<CSCAssignment2DbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Approles> Approles { get; set; }
        public virtual DbSet<Users> Users { get; set; }
        
        public virtual DbSet<Talents> Talents { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
                optionsBuilder.UseMySQL("Server=database-cscassignment2.cwj35gkrhbyl.us-east-1.rds.amazonaws.com; Database=thelifetimetalents; Uid=csc2020sem1; Pwd=cscassignment2;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Approles>(entity =>
            {
                entity.ToTable("approles");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.RoleName)
                    .IsRequired()
                    .HasMaxLength(45)
                    .IsUnicode(false);
            });


            modelBuilder.Entity<Users>(entity =>
            {
                entity.ToTable("Users");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.FullName)
                    .IsRequired()
                    .HasMaxLength(45)
                    .IsUnicode(false);

                entity.Property(e => e.PasswordHash)
                    .IsRequired()
                    .HasColumnType("mediumblob");

                entity.Property(e => e.PasswordSalt)
                    .IsRequired()
                    .HasColumnType("mediumblob");

                entity.Property(e => e.Username)
                    .IsRequired()
                    .HasColumnName("Username")
                    .HasMaxLength(45)
                    .IsUnicode(false);
            });

            modelBuilder.Entity <Talents>(entity =>
            {
                entity.ToTable("talents");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.TalentName)
                    .IsRequired()
                    .HasColumnName("talentname")
                    .HasMaxLength(45)
                    .IsUnicode(false);

                entity.Property(e => e.Reknown)
                    .IsRequired()
                    .HasColumnName("reknown")
                    .HasMaxLength(45)
                    .IsUnicode(false);

                entity.Property(e => e.Shortname)
                    .IsRequired()
                    .HasColumnName("shortname")
                    .HasMaxLength(45)
                    .IsUnicode(false);

                entity.Property(e => e.Bio)
                    .IsRequired()
                    .HasColumnName("bio")
                    .HasMaxLength(45)
                    .IsUnicode(false);

                entity.Property(e => e.TalentImageURL)
                    .IsRequired()
                    .HasColumnName("talentimageurl")
                    .HasMaxLength(100)
                    .IsUnicode(false);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
