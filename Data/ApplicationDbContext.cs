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
        public DbSet<MarkDown> MarkDowns{get;set;}

        protected override void OnModelCreating(ModelBuilder builder)
        {
             
            builder.Entity<Employee>().ToTable("Employees");
            builder.Entity<Project>().ToTable("Project");
            builder.Entity<ProjectDepend>().ToTable("ProjectDepend");
            builder.Entity<Features>().ToTable("Features");
            builder.Entity<FeaturesDepend>().ToTable("FeaturesDepend");
            builder.Entity<FeaIgnoreProDepend>().ToTable("FeaIgnoreProDepend");
            builder.Entity<MarkDown>().ToTable("MarkDown");


            // builder.Entity<Project>().HasMany(p => p.projectDepends).WithOne(l => l.project);
            this.initTable(builder);

            base.OnModelCreating(builder);
            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);
        }
           private void initTable(ModelBuilder builder){
            builder.Entity<FeaIgnoreProDepend>(entity =>
                       {
                           entity.HasKey(e => e.freaturesDependId)
                               .HasName("PK_FeaIgnoreProDepend");

                           entity.Property(e => e.freaturesDependId)
                               .HasColumnName("freaturesDependId")
                               .HasColumnType("int(11)");

                           entity.Property(e => e.featuresId)
                               .HasColumnName("featuresId")
                               .HasColumnType("int(11)");

                           entity.Property(e => e.projectDependid)
                               .HasColumnName("projectDependid")
                               .HasColumnType("int(11)");
                       });

            builder.Entity<Features>(entity =>
            {
                entity.Property(e => e.featuresId)
                    .HasColumnName("featuresId")
                    .HasColumnType("int(11)");

           

                entity.Property(e => e.featuresCss)
                    .HasColumnName("featuresCss")
                    .HasColumnType("varchar(6000)");

                entity.Property(e => e.featuresHtml)
                    .HasColumnName("featuresHtml")
                    .HasColumnType("varchar(6000)");

                entity.Property(e => e.featuresMardDown)
                    .HasColumnName("featuresMardDown")
                    .HasColumnType("varchar(mediumtext)");

                entity.Property(e => e.featuresMdText)
                    .HasColumnName("featuresMdText")
                    .HasColumnType("text");

                entity.Property(e => e.featuresName)
                    .HasColumnName("featuresName")
                    .HasColumnType("varchar(text)");

                entity.Property(e => e.featuresScript)
                    .HasColumnName("featuresScript")
                    .HasColumnType("varchar(6000)");


                entity.Property(e => e.projectId)
                    .HasColumnName("projectId")
                    .HasColumnType("int(11)");

                 entity.HasMany(e => e.featuresDepends)
                    .WithOne(p => p.features);
            });

            builder.Entity<FeaturesDepend>(entity =>
            {
                entity.HasKey(e => e.featuresDependsId)
                    .HasName("PK_FeaturesDepend");

                entity.HasIndex(e => e.featuresId)
                    .HasName("IX_FeaturesDepend_featuresId");

                entity.Property(e => e.featuresDependsId)
                    .HasColumnName("featuresDependsId")
                    .HasColumnType("int(11)");

                entity.Property(e => e.featuresId)
                    .HasColumnName("featuresId")
                    .HasColumnType("int(11)");

                entity.Property(e => e.fileMarkdown)
                    .HasColumnName("fileMarkdown")
                    .HasColumnType("varchar(255)");

                entity.Property(e => e.fileName)
                    .HasColumnName("fileName")
                    .HasColumnType("varchar(255)");

                entity.Property(e => e.filePath)
                    .HasColumnName("filePath")
                    .HasColumnType("varchar(255)");

                entity.Property(e => e.fileType)
                    .HasColumnName("fileType")
                    .HasColumnType("varchar(255)");

            });

            builder.Entity<Project>(entity =>
            {
                entity.Property(e => e.projectId)
                    .HasColumnName("projectId")
                    .HasColumnType("int(11)");


                entity.Property(e => e.projectMarkDown)
                    .HasColumnName("projectMarkDown")
                    .HasColumnType("mediumtext");

                entity.Property(e => e.projectMdText)
                    .HasColumnName("projectMdText")
                    .HasColumnType("text");

                entity.Property(e => e.projectName)
                    .HasColumnName("projectName")
                    .HasColumnType("varchar(255)");

                entity.HasMany(d => d.projectDepends)
                    .WithOne(p => p.project);
            });

            builder.Entity<ProjectDepend>(entity =>
            {
                entity.HasKey(e => e.dependId)
                    .HasName("PK_ProjectDepend");

                entity.HasIndex(e => e.projectId)
                    .HasName("IX_ProjectDepend_projectId");

                entity.Property(e => e.dependId)
                    .HasColumnName("dependId")
                    .HasColumnType("int(11)");

                entity.Property(e => e.fileMarkdown)
                    .HasColumnName("fileMarkdown")
                    .HasColumnType("text");

                entity.Property(e => e.fileName)
                    .HasColumnName("fileName")
                    .HasColumnType("varchar(255)");

                entity.Property(e => e.filePath)
                    .HasColumnName("filePath")
                    .HasColumnType("varchar(255)");

                entity.Property(e => e.fileType)
                    .HasColumnName("fileType")
                    .HasColumnType("varchar(255)");

                entity.Property(e => e.projectId)
                    .HasColumnName("projectId")
                    .HasColumnType("int(11)");

            });
        }
    }
}
