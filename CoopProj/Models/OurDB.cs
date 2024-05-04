using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using CoopProj.Models;

namespace CoopProj.Models
{
    public class OurDB : DbContext
    {
        public OurDB(DbContextOptions<OurDB> options)
            : base(options)
        {
        }
            
        public virtual DbSet<Users> Users { get; set; }
        public virtual DbSet<Companies> Companies { get; set; }
        public virtual DbSet<Requests> Requests { get; set; }
        public virtual DbSet<ApplyStudent> ApplyStudent { get; set; }
        public virtual DbSet<Students> Students { get; set; }
        public virtual DbSet<Majors> Majors { get; set; }

        public virtual DbSet<Roles> Roles { get; set; }

        public virtual DbSet<Reports> Reports { get; set; }

        public virtual DbSet<EmailModel> EmailModel { get; set; }

        public virtual DbSet<CreateReport> CreateReport { get; set; }
        public virtual DbSet<ReportName> ReportName { get; set; }
        public virtual DbSet<ResetPasswordViewModel> ResetPasswordViewModel { get; set; }
        public virtual DbSet<ForgotPasswordViewModel> ForgotPasswordViewModel { get; set; }


        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    optionsBuilder.EnableSensitiveDataLogging();
        //}

    }
}

