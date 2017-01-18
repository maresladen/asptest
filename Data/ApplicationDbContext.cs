using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebApplication.Models;

namespace WebApplication.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser,ApplicationRole,string>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Employee> Employees { get; set; }
        public DbSet<Project> Projects{get;set;}
        public DbSet<ProjectDepend> ProjectDepends{get;set;}
        public DbSet<Features> Features{get;set;}
        public DbSet<FeaturesDepend> FeaturesDepends{get;set;}
        public DbSet<FeaIgnoreProDepend> FeaIgnoreProDepends{get;set;}

        protected override void OnModelCreating(ModelBuilder builder)
        {
             
            builder.Entity<Employee>().ToTable("Employees");
            builder.Entity<Project>().ToTable("Project");
            builder.Entity<ProjectDepend>().ToTable("ProjectDepend");
            builder.Entity<Features>().ToTable("Features");
            builder.Entity<FeaturesDepend>().ToTable("FeaturesDepend");
            builder.Entity<FeaIgnoreProDepend>().ToTable("FeaIgnoreProDepend");


            builder.Entity<Project>().HasMany(p => p.projectDepends).WithOne(l => l.project);

            base.OnModelCreating(builder);
            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);
        }
    }
}
