using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StudyAssistant.Web.Core.Domain;
using StudyAssistant.Web.Models;

namespace StudyAssistant.Web.Data
{
    public class ApplicationDbContext : IdentityDbContext<User, Role, int>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Course> Courses { get; set; }
        public virtual DbSet<StudySession> StudySessions { get; set; }
        public virtual DbSet<Assignment> Assignments { get; set; }
        public virtual DbSet<StudySessionType> StudySessionTypes { get; set; }

        /// <summary>
        /// Gets a single course by Id
        /// </summary>
        /// <param name="id">The Id of the course to get</param>
        /// <returns>A single course or null if the specified course doesn't exist</returns>
        public async Task<Course> GetCourseById(int? id)
        {
            return await Courses.FirstOrDefaultAsync(c => c.Id == id);
        }

        /// <summary>
        /// Gets a list of courses belonging to the specified user with pagination data.
        /// </summary>
        /// <param name="userId">The id of the user who owns the courses</param>
        /// <param name="searchString">The search string for the course title search</param>
        /// <param name="sortOrder">Specifies which column to sort by</param>
        /// <param name="descending">True returns a descending list, false returns an ascending list</param>
        /// <param name="onlyActiveCourses">True returns only active courses, false returns all courses</param>
        /// <returns></returns>
        public IQueryable<Course> GetCoursesPagedList(int userId, string searchString, string sortOrder, bool descending, bool onlyActiveCourses = false)
        {
            IQueryable<Course> courses = Courses.Where(c => c.UserId == userId);
           
            if (!string.IsNullOrEmpty(searchString))
            {
                courses = courses.Where(c => c.Title.Contains(searchString));
            }

            if (onlyActiveCourses)
            {
                courses = courses.Where(c => DateTime.Now.CompareTo(c.DateTo) <= 0);
            }

            courses = descending ? courses.OrderByDescending(e => EF.Property<object>(e, sortOrder)) : courses.OrderBy(e => EF.Property<object>(e, sortOrder));
            
            return courses;
        }

        /// <summary>
        /// Gets a list of assignments belonging to the specified user with pagination data
        /// </summary>
        /// <param name="userId">The is of the user who owns the assignments</param>
        /// <param name="searchString">The search string for the assignment title search</param>
        /// <param name="sortOrder">Specifies which column to sort by</param>
        /// <param name="descending">True returns a descending list, false returns an ascending list</param>
        /// <param name="onlyActiveAssignments">True returns only active assignments, false returns all assignments</param>
        /// <returns></returns>
        public IQueryable<Assignment> GetAssignmentsPagedList(int userId, string searchString, string sortOrder, bool descending, bool onlyActiveAssignments = false)
        {
            IQueryable<Assignment> assignments = Assignments.Where(c => c.Course.UserId == userId)
                .Include(a => a.Course);
           
            if (!string.IsNullOrEmpty(searchString))
            {
                assignments = assignments.Where(c => c.Title.Contains(searchString));
            }

            if (onlyActiveAssignments)
            {
                assignments = assignments.Where(c => c.DateCompleted == null);
            }

            assignments = descending ? assignments.OrderByDescending(e => EF.Property<object>(e, sortOrder)) : assignments.OrderBy(e => EF.Property<object>(e, sortOrder));
            
            return assignments;
        }

        /// <summary>
        /// Gets a list of study sessions belonging to the specified user with pagination data
        /// </summary>
        /// <param name="userId">The id of the user who owns the study sessions</param>
        /// <param name="searchString">The search string for the study session search</param>
        /// <param name="sortOrder">Specifies which column to order by</param>
        /// <param name="descending">True returns a descending list, false returns an ascending list</param>
        /// <param name="onlyActiveStudySessions">True returns only planned (or active) study sessions, false returns all study sessions</param>
        /// <returns></returns>
        public IQueryable<StudySession> GetStudySessionsPagedList(int userId, string searchString, string sortOrder, bool descending, bool onlyActiveStudySessions = false)
        {
            IQueryable<StudySession> studySessions = GetStudySessionsByUser(userId, false)
                .Include(s => s.Course)
                .Include(s => s.StudySessionType);
           
            if (!string.IsNullOrEmpty(searchString))
            {
                studySessions = studySessions.Where(s => s.Title.Contains(searchString));
            }

            if (onlyActiveStudySessions)
            {
                
                studySessions = studySessions.Where(s => DateTime.Now.CompareTo(s.GetStudySessionStart().Add(s.Duration)) <= 0 && s.IsCompleted != true);
            }

            studySessions = descending ? studySessions.OrderByDescending(e => EF.Property<object>(e, sortOrder)) : studySessions.OrderBy(e => EF.Property<object>(e, sortOrder));
            
            return studySessions;
        }

        /// <summary>
        /// Gets a list of courses belonging to the specified user
        /// </summary>
        /// <param name="userId">The id of the user who owns the courses</param>
        /// <param name="onlyActiveCourses">True returns only active courses, false returns *all* courses</param>
        /// <returns></returns>
        public IQueryable<Course> GetCourses(int userId, bool onlyActiveCourses = false)
        {
            IQueryable<Course> courses = Courses.Where(c => c.UserId == userId);
           
            if (onlyActiveCourses)
            {
                courses = courses.Where(c => DateTime.Now.CompareTo(c.DateTo) <= 0);
            }
           
            return courses;
        }

        /// <summary>
        /// Gets a specified assignment by Id
        /// </summary>
        /// <param name="id">The Id of the course to get</param>
        /// <returns>A single assignment or null if the specified assignment doesn't exist</returns>
        public async Task<Assignment> GetAssignmentById(int? id)
        {
            return await Assignments
                .Include(a => a.Course)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        /// <summary>
        /// Gets a list of assignments belonging to the specified user
        /// </summary>
        /// <param name="userId">The id of the user who owns the assignments</param>
        /// <param name="courseId">The id of the course the assignments are given in</param>
        /// <param name="isActive">True returns only active assignments, false returns all assignments</param>
        /// <param name="timeInterval">The time period for the search, i.e. 7 returns assignments with deadline in the next 7 days</param>
        /// <returns></returns>
        public IQueryable<Assignment> GetAssignments(int userId, int? courseId, bool isActive, double timeInterval = 0)
        {
            IQueryable<Assignment> assignments = Assignments.Include(a => a.Course)
                                                    .Where(a => a.Course.UserId == userId);
            if (courseId != null)
            {
                assignments = assignments.Where(a => a.CourseId == courseId);
            }
            if (isActive)
            {
                assignments = assignments.Where(a => a.DateCompleted == null);
            }
            if (timeInterval > 0)
            {
                DateTime endDate = DateTime.Now.AddDays(timeInterval);
                assignments = assignments.Where(a =>
                    a.Deadline.CompareTo(DateTime.Now) >= 0 && a.Deadline.CompareTo(endDate) <= 0);
            }
            return assignments;
        }

        /// <summary>
        /// Gets a specified study session by Id
        /// </summary>
        /// <param name="id">The Id of the study session to get</param>
        /// <returns>A single study session or null if the specified assignment doesn't exist</returns>
        public async Task<StudySession> GetStudySessionById(int? id)
        {
            return await StudySessions
                .Include(s => s.Course)
                .Include(s => s.StudySessionType)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public IQueryable<StudySession> GetStudySessions(bool onlyActive)
        {
            IQueryable<StudySession> studySessions = StudySessions
                .Include(sa => sa.StudySessionType)
                .Include(sa => sa.Course);
            if (onlyActive)
            {
                studySessions = studySessions.Where(sa => DateTime.Now.CompareTo(sa.GetStudySessionEnd()) < 0 && sa.IsCompleted == false);
                
            }
            return studySessions;
        }

        /// <summary>
        /// Gets a user's study sessions
        /// </summary>
        /// <param name="userId">The id of the user who owns the study sessions</param>
        /// <param name="onlyActive">Returns all study sessions if false, only active sessions if true</param>
        /// <returns></returns>
        public IQueryable<StudySession> GetStudySessionsByUser(int userId, bool onlyActive)
        {
            IQueryable<StudySession> studySessions = GetStudySessions(onlyActive)
                                                        .Where(sa => sa.Course.UserId == userId);
            return studySessions;
        }

        public IQueryable<StudySession> GetStudySessionsByCourse(int? courseId, bool onlyActive)
        {
            return GetStudySessions(onlyActive).Where(c => c.CourseId == courseId);
        }

        /// <summary>
        /// Sets a study session's status to completed
        /// </summary>
        /// <param name="id">Id of the particular study session</param>
        /// <returns></returns>
        public async Task<int> FinishStudySessionAsync(int id)
        {
            var studySession = await GetStudySessionById(id);
            studySession.IsCompleted = true;

            StudySessions.Update(studySession);
            return await SaveChangesAsync();
        }

        /// <summary>
        /// Finishing an assignment by setting its completion date to todays date
        /// </summary>
        /// <param name="id">The id of the assignment to finish</param>
        /// <returns></returns>
        public async Task<int> FinishAssignmentAsync(int id)
        {
            var assignment = await GetAssignmentById(id);
            assignment.DateCompleted = DateTime.Now;

            Assignments.Update(assignment);
            return await SaveChangesAsync();

        }

        /// <summary>
        /// For populating a select input with course data.
        /// </summary>
        /// <param name="userId">The id of the user who owns the courses</param>
        /// <returns></returns>
        public async Task<List<SelectListItem>> PopulateCourseData(int userId)
        {
            return await GetCourses(userId, true)
                .OrderBy(c => c.Title)
                .Select(c => new SelectListItem() {Text = c.Title, Value = c.Id.ToString()})
                .ToListAsync();
        }
        
        public async Task<double> GetStudyTimeByCourse(int? courseId)
        {
            double sumStudyTime = 0;

            if (courseId == 0)
            {
                return 0;
            }
            var studySessions = await GetStudySessionsByCourse(courseId, false)
                .Where(s => s.IsCompleted == true)
                .Select(s => new { durationMinutes = (s.Duration.Hours + (s.Duration.Minutes/60))})
                .ToListAsync();
            foreach (var t in studySessions)
            {
                sumStudyTime += t.durationMinutes;
            }

            return sumStudyTime;
        }

        public async Task<List<StudySessionDuration>> GetCompletedStudySessionDurationsByCourse(int courseId)
        {
            var studySessions = await GetStudySessionsByCourse(courseId, false)
                .Where(s => s.IsCompleted == true)
                .OrderBy(s => s.StartDate)
                .ThenBy(s => s.StartTime)
                .Select(s => new StudySessionDuration() { StartDate = s.StartDate, Duration = (s.Duration.Hours + (s.Duration.Minutes/60))})
                .ToListAsync();
            return studySessions;
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);


            builder.Entity<Assignment>(e =>
            {
                e.ToTable("Assignment");
                
                e.HasKey(k => k.Id);

                e.Property(p => p.DateCreated)
                    .HasDefaultValueSql("getdate()");
                e.Property(p => p.Title)
                    .IsRequired();
                e.HasOne(c => c.Course)
                    .WithMany(a => a.Assignments)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<Course>(e =>
            {
                e.ToTable("Course");

                e.Property(p => p.DateCreated)
                    .HasDefaultValueSql("getdate()");
                e.Property(p => p.Title)
                    .IsRequired();
                e.Property(p => p.Credits)
                    .IsRequired();
                e.Property(p => p.DateFrom)
                    .HasColumnType("date");
                e.Property(p => p.DateTo)
                    .HasColumnType("date");
            });

            
            builder.Entity<StudySession>(e =>
            {
                e.ToTable("StudySession");

                e.Property(p => p.DateCreated)
                    .HasDefaultValueSql("getdate()");
                e.Property(p => p.Title)
                    .IsRequired();
                e.Property(p => p.StartDate)
                    .HasColumnType("date")
                    .IsRequired();
                e.Property(p => p.StartTime)
                    .HasColumnType("time")
                    .IsRequired();
                e.Property(p => p.Duration)
                    .IsRequired();
                e.HasOne(c => c.Course)
                    .WithMany(s => s.StudySessions)
                    .OnDelete(DeleteBehavior.Cascade);
            });
            
            builder.Entity<StudySessionType>(e =>
                {
                    e.ToTable("StudySessionType");
                    e.Property(p => p.Title)
                        .IsRequired();
                });

            builder.Entity<User>(e =>
            {
                e.ToTable("User");
            });

            builder.Entity<IdentityRole<int>>()
                .ToTable("Role");

            builder.Entity<IdentityRoleClaim<int>>()
                .ToTable("RoleClaim");
            builder.Entity<IdentityUserClaim<int>>()
                .ToTable("UserClaim");
            builder.Entity<IdentityUserLogin<int>>()
                .ToTable("UserLogin");
            builder.Entity<IdentityUserToken<int>>()
                .ToTable("UserToken");
            builder.Entity<IdentityUserRole<int>>()
                .ToTable("UserRole");



            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);

            // Singularize Identity table names


        }
    }
}