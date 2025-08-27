using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SportAcademy.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Infrastructure.DBContext
{
    public class ApplicationDbContext : IdentityDbContext<AppUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
            base.OnModelCreating(modelBuilder);
        }
        public DbSet<AppUser> AppUsers { get; set; }
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<Branch> Branchs { get; set; }

        public DbSet<Coach> Coachs { get; set; }

        public DbSet<Employee> Employees { get; set; }

        public DbSet<Enrollment> Enrollments { get; set; }
        public DbSet<Profile> Profiles { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Session> Sessions { get; set; }
        public DbSet<Sport> Sports { get; set; }

        public DbSet<SportBranch> SportBranchs { get; set; }
        public DbSet<SportPrice> SportPrices { get; set; }
      
        public DbSet<SportSubscriptionType> SportSubscriptionTypes { get; set; }
        public DbSet<SportTrainee> SportTrainees { get; set; }
        public DbSet<SubscriptionDetails> SubscriptionDetails { get; set; }
        public DbSet<SubscriptionType> SubscriptionTypes { get; set; }
        public DbSet<Trainee> Trainees { get; set; }







    }
}
