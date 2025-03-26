using GymManagement.Models;
using Microsoft.EntityFrameworkCore;
using GymManagement.ViewModels;

namespace GymManagement.Data
{
    public class GymContext : DbContext
    {
        //To give access to IHttpContextAccessor for Audit Data with IAuditable
        private readonly IHttpContextAccessor _httpContextAccessor;

        //Property to hold the UserName value
        public string UserName
        {
            get; private set;
        }

        public GymContext(DbContextOptions<GymContext> options, IHttpContextAccessor httpContextAccessor)
             : base(options)
        {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            if (_httpContextAccessor.HttpContext != null)
            {
                //We have a HttpContext, but there might not be anyone Authenticated
                UserName = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Unknown";
            }
            else
            {
                //No HttpContext so seeding data
                UserName = "Seed Data";
            }
        }

        public GymContext(DbContextOptions<GymContext> options)
            : base(options)
        {
            _httpContextAccessor = null!;
            UserName = "Seed Data";
        }

        public DbSet<FitnessCategory> FitnessCategories { get; set; }
        public DbSet<Instructor> Instructors { get; set; }
        public DbSet<ClassTime> ClassTimes { get; set; }
        public DbSet<GroupClass> GroupClasses { get; set; }
        public DbSet<MembershipType> MembershipTypes { get; set; }
        public DbSet<Client> Clients { get; set; }
        public DbSet<Enrollment> Enrollments { get; set; }
        public DbSet<Workout> Workouts { get; set; }
        public DbSet<Exercise> Exercises { get; set; }
        public DbSet<WorkoutExercise> WorkoutExercises { get; set; }
        public DbSet<ExerciseCategory> ExerciseCategories { get; set; }
        public DbSet<ClientPhoto> ClientPhotos { get; set; }
        public DbSet<ClientThumbnail> ClientThumbnails { get; set; }
        public DbSet<InstructorDocument> InstructorDocuments { get; set; }
        public DbSet<UploadedFile> UploadedFiles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //Prevent Cascade Delete from FitnessCategory to GroupClass
            //so we are prevented from deleting a FitnessCategory with
            //GroupClasses scheduled
            modelBuilder.Entity<FitnessCategory>()
                .HasMany<GroupClass>(d => d.GroupClasses)
                .WithOne(p => p.FitnessCategory)
                .HasForeignKey(p => p.FitnessCategoryID)
                .OnDelete(DeleteBehavior.Restrict);

            //Same for Instructor
            modelBuilder.Entity<Instructor>()
                .HasMany<GroupClass>(d => d.GroupClasses)
                .WithOne(p => p.Instructor)
                .HasForeignKey(p => p.InstructorID)
                .OnDelete(DeleteBehavior.Restrict);

            //Same for ClassTime
            modelBuilder.Entity<ClassTime>()
                .HasMany<GroupClass>(d => d.GroupClasses)
                .WithOne(p => p.ClassTime)
                .HasForeignKey(p => p.ClassTimeID)
                .OnDelete(DeleteBehavior.Restrict);


            //Similar for MembershipType and Client
            modelBuilder.Entity<MembershipType>()
                .HasMany<Client>(d => d.Clients)
                .WithOne(p => p.MembershipType)
                .HasForeignKey(p => p.MembershipTypeID)
                .OnDelete(DeleteBehavior.Restrict);

            //Prevent Cascade Delete from Client to Enrollment
            //so we are prevented from deleting a Client who is
            //Enrolled in any Group Classes.
            //NOTE: we will allow cascade delete from GroupClass
            //to Enrollment so if a class is deleted, the related
            //enrollment records are cleaned up as well.
            //Also Note: We will allow cascade delete from Client
            //to Workout
            modelBuilder.Entity<Client>()
                .HasMany<Enrollment>(d => d.Enrollments)
                .WithOne(p => p.Client)
                .HasForeignKey(p => p.ClientID)
                .OnDelete(DeleteBehavior.Restrict);

            //3 New restrictions for Part 3A
            modelBuilder.Entity<Exercise>()
                .HasMany<ExerciseCategory>(d => d.ExerciseCategories)
                .WithOne(p => p.Exercise)
                .HasForeignKey(p => p.ExerciseID)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Exercise>()
                .HasMany<WorkoutExercise>(d => d.WorkoutExercises)
                .WithOne(p => p.Exercise)
                .HasForeignKey(p => p.ExerciseID)
                .OnDelete(DeleteBehavior.Restrict);

            //Many to Many Intersection
            modelBuilder.Entity<Enrollment>()
            .HasKey(e => new { e.ClientID, e.GroupClassID });

            //2 New Many to Many Intersections for Part 3A
            modelBuilder.Entity<ExerciseCategory>()
            .HasKey(e => new { e.FitnessCategoryID, e.ExerciseID });
            //
            modelBuilder.Entity<WorkoutExercise>()
            .HasKey(e => new { e.WorkoutID, e.ExerciseID });

            //Add a unique index to the Instructor Email 
            modelBuilder.Entity<Instructor>()
            .HasIndex(p => p.Email)
            .IsUnique();

            //Add a unique index to the Client MembershipNumber 
            modelBuilder.Entity<Client>()
            .HasIndex(p => p.MembershipNumber)
            .IsUnique();

            //Add a unique composite index to the GroupClass 
            modelBuilder.Entity<GroupClass>()
            .HasIndex(p => new { p.InstructorID, p.DOW, p.ClassTimeID })
            .IsUnique();

            //Many to Many Intersection
            modelBuilder.Entity<Enrollment>()
            .HasKey(e => new { e.ClientID, e.GroupClassID });

            //Add a unique index to the FitnessCategory Category
            modelBuilder.Entity<FitnessCategory>()
             .HasIndex(fc => fc.Category)
             .IsUnique();

            //Add a unique index to the Exercise Name
            modelBuilder.Entity<Exercise>()
                .HasIndex(e => e.Name)
                .IsUnique();
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            OnBeforeSaving();
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default(CancellationToken))
        {
            OnBeforeSaving();
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        private void OnBeforeSaving()
        {
            var entries = ChangeTracker.Entries();
            foreach (var entry in entries)
            {
                if (entry.Entity is IAuditable trackable)
                {
                    var now = DateTime.UtcNow;
                    switch (entry.State)
                    {
                        case EntityState.Modified:
                            trackable.UpdatedOn = now;
                            trackable.UpdatedBy = UserName;
                            break;

                        case EntityState.Added:
                            trackable.CreatedOn = now;
                            trackable.CreatedBy = UserName;
                            trackable.UpdatedOn = now;
                            trackable.UpdatedBy = UserName;
                            break;
                    }
                }
            }
        }
    }
}
