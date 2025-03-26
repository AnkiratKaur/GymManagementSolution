﻿// <auto-generated />
using System;
using GymManagement.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace GymManagement.Data.GymMigrations
{
    [DbContext(typeof(GymContext))]
    [Migration("20250310215311_Initial")]
    partial class Initial
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "8.0.11");

            modelBuilder.Entity("GymManagement.Models.ClassTime", b =>
                {
                    b.Property<int>("ID")
                        .HasColumnType("INTEGER");

                    b.Property<string>("StartTime")
                        .IsRequired()
                        .HasMaxLength(8)
                        .HasColumnType("TEXT");

                    b.HasKey("ID");

                    b.ToTable("ClassTimes");
                });

            modelBuilder.Entity("GymManagement.Models.Client", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("CreatedBy")
                        .HasMaxLength(256)
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("CreatedOn")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("DOB")
                        .HasColumnType("TEXT");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("TEXT");

                    b.Property<bool>("FeePaid")
                        .HasColumnType("INTEGER");

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("TEXT");

                    b.Property<string>("HealthCondition")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("TEXT");

                    b.Property<string>("LastName")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("MembershipEndDate")
                        .HasColumnType("TEXT");

                    b.Property<double>("MembershipFee")
                        .HasColumnType("REAL");

                    b.Property<int>("MembershipNumber")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("MembershipStartDate")
                        .HasColumnType("TEXT");

                    b.Property<int>("MembershipTypeID")
                        .HasColumnType("INTEGER");

                    b.Property<string>("MiddleName")
                        .HasMaxLength(50)
                        .HasColumnType("TEXT");

                    b.Property<string>("Notes")
                        .IsRequired()
                        .HasMaxLength(2000)
                        .HasColumnType("TEXT");

                    b.Property<string>("Phone")
                        .IsRequired()
                        .HasMaxLength(10)
                        .HasColumnType("TEXT");

                    b.Property<string>("PostalCode")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<byte[]>("RowVersion")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("BLOB");

                    b.Property<string>("UpdatedBy")
                        .HasMaxLength(256)
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("UpdatedOn")
                        .HasColumnType("TEXT");

                    b.HasKey("ID");

                    b.HasIndex("MembershipNumber")
                        .IsUnique();

                    b.HasIndex("MembershipTypeID");

                    b.ToTable("Clients");
                });

            modelBuilder.Entity("GymManagement.Models.ClientPhoto", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("ClientID")
                        .HasColumnType("INTEGER");

                    b.Property<byte[]>("Content")
                        .HasColumnType("BLOB");

                    b.Property<string>("MimeType")
                        .HasMaxLength(255)
                        .HasColumnType("TEXT");

                    b.HasKey("ID");

                    b.HasIndex("ClientID")
                        .IsUnique();

                    b.ToTable("ClientPhotos");
                });

            modelBuilder.Entity("GymManagement.Models.ClientThumbnail", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("ClientID")
                        .HasColumnType("INTEGER");

                    b.Property<byte[]>("Content")
                        .HasColumnType("BLOB");

                    b.Property<string>("MimeType")
                        .HasMaxLength(255)
                        .HasColumnType("TEXT");

                    b.HasKey("ID");

                    b.HasIndex("ClientID")
                        .IsUnique();

                    b.ToTable("ClientThumbnails");
                });

            modelBuilder.Entity("GymManagement.Models.Enrollment", b =>
                {
                    b.Property<int>("ClientID")
                        .HasColumnType("INTEGER");

                    b.Property<int>("GroupClassID")
                        .HasColumnType("INTEGER");

                    b.HasKey("ClientID", "GroupClassID");

                    b.HasIndex("GroupClassID");

                    b.ToTable("Enrollments");
                });

            modelBuilder.Entity("GymManagement.Models.Exercise", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("TEXT");

                    b.HasKey("ID");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.ToTable("Exercises");
                });

            modelBuilder.Entity("GymManagement.Models.ExerciseCategory", b =>
                {
                    b.Property<int>("FitnessCategoryID")
                        .HasColumnType("INTEGER");

                    b.Property<int>("ExerciseID")
                        .HasColumnType("INTEGER");

                    b.HasKey("FitnessCategoryID", "ExerciseID");

                    b.HasIndex("ExerciseID");

                    b.ToTable("ExerciseCategories");
                });

            modelBuilder.Entity("GymManagement.Models.FileContent", b =>
                {
                    b.Property<int>("FileContentID")
                        .HasColumnType("INTEGER");

                    b.Property<byte[]>("Content")
                        .HasColumnType("BLOB");

                    b.HasKey("FileContentID");

                    b.ToTable("FileContent");
                });

            modelBuilder.Entity("GymManagement.Models.FitnessCategory", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Category")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("TEXT");

                    b.Property<string>("CreatedBy")
                        .HasMaxLength(256)
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("CreatedOn")
                        .HasColumnType("TEXT");

                    b.Property<string>("UpdatedBy")
                        .HasMaxLength(256)
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("UpdatedOn")
                        .HasColumnType("TEXT");

                    b.HasKey("ID");

                    b.HasIndex("Category")
                        .IsUnique();

                    b.ToTable("FitnessCategories");
                });

            modelBuilder.Entity("GymManagement.Models.GroupClass", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("ClassTimeID")
                        .HasColumnType("INTEGER");

                    b.Property<string>("CreatedBy")
                        .HasMaxLength(256)
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("CreatedOn")
                        .HasColumnType("TEXT");

                    b.Property<int>("DOW")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("TEXT");

                    b.Property<int>("FitnessCategoryID")
                        .HasColumnType("INTEGER");

                    b.Property<int>("InstructorID")
                        .HasColumnType("INTEGER");

                    b.Property<byte[]>("RowVersion")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("BLOB");

                    b.Property<string>("UpdatedBy")
                        .HasMaxLength(256)
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("UpdatedOn")
                        .HasColumnType("TEXT");

                    b.HasKey("ID");

                    b.HasIndex("ClassTimeID");

                    b.HasIndex("FitnessCategoryID");

                    b.HasIndex("InstructorID", "DOW", "ClassTimeID")
                        .IsUnique();

                    b.ToTable("GroupClasses");
                });

            modelBuilder.Entity("GymManagement.Models.Instructor", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("CreatedBy")
                        .HasMaxLength(256)
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("CreatedOn")
                        .HasColumnType("TEXT");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("TEXT");

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("HireDate")
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsActive")
                        .HasColumnType("INTEGER");

                    b.Property<string>("LastName")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("TEXT");

                    b.Property<string>("MiddleName")
                        .HasMaxLength(50)
                        .HasColumnType("TEXT");

                    b.Property<string>("Phone")
                        .IsRequired()
                        .HasMaxLength(10)
                        .HasColumnType("TEXT");

                    b.Property<string>("UpdatedBy")
                        .HasMaxLength(256)
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("UpdatedOn")
                        .HasColumnType("TEXT");

                    b.HasKey("ID");

                    b.HasIndex("Email")
                        .IsUnique();

                    b.ToTable("Instructors");
                });

            modelBuilder.Entity("GymManagement.Models.MembershipType", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<double>("StandardFee")
                        .HasColumnType("REAL");

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("TEXT");

                    b.HasKey("ID");

                    b.ToTable("MembershipTypes");
                });

            modelBuilder.Entity("GymManagement.Models.UploadedFile", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Description")
                        .HasMaxLength(255)
                        .HasColumnType("TEXT");

                    b.Property<string>("Discriminator")
                        .IsRequired()
                        .HasMaxLength(21)
                        .HasColumnType("TEXT");

                    b.Property<string>("FileName")
                        .HasMaxLength(255)
                        .HasColumnType("TEXT");

                    b.Property<string>("MimeType")
                        .HasMaxLength(255)
                        .HasColumnType("TEXT");

                    b.HasKey("ID");

                    b.ToTable("UploadedFiles");

                    b.HasDiscriminator().HasValue("UploadedFile");

                    b.UseTphMappingStrategy();
                });

            modelBuilder.Entity("GymManagement.Models.Workout", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("ClientID")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime?>("EndTime")
                        .HasColumnType("TEXT");

                    b.Property<int?>("InstructorID")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Notes")
                        .HasMaxLength(2000)
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("StartTime")
                        .HasColumnType("TEXT");

                    b.HasKey("ID");

                    b.HasIndex("ClientID");

                    b.HasIndex("InstructorID");

                    b.ToTable("Workouts");
                });

            modelBuilder.Entity("GymManagement.Models.WorkoutExercise", b =>
                {
                    b.Property<int>("WorkoutID")
                        .HasColumnType("INTEGER");

                    b.Property<int>("ExerciseID")
                        .HasColumnType("INTEGER");

                    b.HasKey("WorkoutID", "ExerciseID");

                    b.HasIndex("ExerciseID");

                    b.ToTable("WorkoutExercises");
                });

            modelBuilder.Entity("GymManagement.Models.InstructorDocument", b =>
                {
                    b.HasBaseType("GymManagement.Models.UploadedFile");

                    b.Property<int>("InstructorID")
                        .HasColumnType("INTEGER");

                    b.HasIndex("InstructorID");

                    b.HasDiscriminator().HasValue("InstructorDocument");
                });

            modelBuilder.Entity("GymManagement.Models.Client", b =>
                {
                    b.HasOne("GymManagement.Models.MembershipType", "MembershipType")
                        .WithMany("Clients")
                        .HasForeignKey("MembershipTypeID")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("MembershipType");
                });

            modelBuilder.Entity("GymManagement.Models.ClientPhoto", b =>
                {
                    b.HasOne("GymManagement.Models.Client", "Client")
                        .WithOne("ClientPhoto")
                        .HasForeignKey("GymManagement.Models.ClientPhoto", "ClientID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Client");
                });

            modelBuilder.Entity("GymManagement.Models.ClientThumbnail", b =>
                {
                    b.HasOne("GymManagement.Models.Client", "Client")
                        .WithOne("ClientThumbnail")
                        .HasForeignKey("GymManagement.Models.ClientThumbnail", "ClientID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Client");
                });

            modelBuilder.Entity("GymManagement.Models.Enrollment", b =>
                {
                    b.HasOne("GymManagement.Models.Client", "Client")
                        .WithMany("Enrollments")
                        .HasForeignKey("ClientID")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("GymManagement.Models.GroupClass", "GroupClass")
                        .WithMany("Enrollments")
                        .HasForeignKey("GroupClassID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Client");

                    b.Navigation("GroupClass");
                });

            modelBuilder.Entity("GymManagement.Models.ExerciseCategory", b =>
                {
                    b.HasOne("GymManagement.Models.Exercise", "Exercise")
                        .WithMany("ExerciseCategories")
                        .HasForeignKey("ExerciseID")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("GymManagement.Models.FitnessCategory", "FitnessCategory")
                        .WithMany("ExerciseCategories")
                        .HasForeignKey("FitnessCategoryID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Exercise");

                    b.Navigation("FitnessCategory");
                });

            modelBuilder.Entity("GymManagement.Models.FileContent", b =>
                {
                    b.HasOne("GymManagement.Models.UploadedFile", "UploadedFile")
                        .WithOne("FileContent")
                        .HasForeignKey("GymManagement.Models.FileContent", "FileContentID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("UploadedFile");
                });

            modelBuilder.Entity("GymManagement.Models.GroupClass", b =>
                {
                    b.HasOne("GymManagement.Models.ClassTime", "ClassTime")
                        .WithMany("GroupClasses")
                        .HasForeignKey("ClassTimeID")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("GymManagement.Models.FitnessCategory", "FitnessCategory")
                        .WithMany("GroupClasses")
                        .HasForeignKey("FitnessCategoryID")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("GymManagement.Models.Instructor", "Instructor")
                        .WithMany("GroupClasses")
                        .HasForeignKey("InstructorID")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("ClassTime");

                    b.Navigation("FitnessCategory");

                    b.Navigation("Instructor");
                });

            modelBuilder.Entity("GymManagement.Models.Workout", b =>
                {
                    b.HasOne("GymManagement.Models.Client", "Client")
                        .WithMany("Workouts")
                        .HasForeignKey("ClientID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("GymManagement.Models.Instructor", "Instructor")
                        .WithMany("Workouts")
                        .HasForeignKey("InstructorID");

                    b.Navigation("Client");

                    b.Navigation("Instructor");
                });

            modelBuilder.Entity("GymManagement.Models.WorkoutExercise", b =>
                {
                    b.HasOne("GymManagement.Models.Exercise", "Exercise")
                        .WithMany("WorkoutExercises")
                        .HasForeignKey("ExerciseID")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("GymManagement.Models.Workout", "Workout")
                        .WithMany("WorkoutExercises")
                        .HasForeignKey("WorkoutID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Exercise");

                    b.Navigation("Workout");
                });

            modelBuilder.Entity("GymManagement.Models.InstructorDocument", b =>
                {
                    b.HasOne("GymManagement.Models.Instructor", "Instructor")
                        .WithMany("InstructorDocuments")
                        .HasForeignKey("InstructorID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Instructor");
                });

            modelBuilder.Entity("GymManagement.Models.ClassTime", b =>
                {
                    b.Navigation("GroupClasses");
                });

            modelBuilder.Entity("GymManagement.Models.Client", b =>
                {
                    b.Navigation("ClientPhoto");

                    b.Navigation("ClientThumbnail");

                    b.Navigation("Enrollments");

                    b.Navigation("Workouts");
                });

            modelBuilder.Entity("GymManagement.Models.Exercise", b =>
                {
                    b.Navigation("ExerciseCategories");

                    b.Navigation("WorkoutExercises");
                });

            modelBuilder.Entity("GymManagement.Models.FitnessCategory", b =>
                {
                    b.Navigation("ExerciseCategories");

                    b.Navigation("GroupClasses");
                });

            modelBuilder.Entity("GymManagement.Models.GroupClass", b =>
                {
                    b.Navigation("Enrollments");
                });

            modelBuilder.Entity("GymManagement.Models.Instructor", b =>
                {
                    b.Navigation("GroupClasses");

                    b.Navigation("InstructorDocuments");

                    b.Navigation("Workouts");
                });

            modelBuilder.Entity("GymManagement.Models.MembershipType", b =>
                {
                    b.Navigation("Clients");
                });

            modelBuilder.Entity("GymManagement.Models.UploadedFile", b =>
                {
                    b.Navigation("FileContent");
                });

            modelBuilder.Entity("GymManagement.Models.Workout", b =>
                {
                    b.Navigation("WorkoutExercises");
                });
#pragma warning restore 612, 618
        }
    }
}
