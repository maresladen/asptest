using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace WebApplication.Data.Migrations
{
    public partial class MyMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FeaIgnoreProDepend",
                columns: table => new
                {
                    freaturesDependId = table.Column<int>(nullable: false)
                        .Annotation("MySQL:AutoIncrement", true),
                    featuresId = table.Column<int>(nullable: false),
                    projectDependid = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeaIgnoreProDepend", x => x.freaturesDependId);
                });

            migrationBuilder.CreateTable(
                name: "Features",
                columns: table => new
                {
                    featuresId = table.Column<int>(nullable: false)
                        .Annotation("MySQL:AutoIncrement", true),
                    featuresCss = table.Column<string>(nullable: true),
                    featuresHtml = table.Column<string>(nullable: true),
                    featuresMardDown = table.Column<string>(nullable: true),
                    featuresName = table.Column<string>(nullable: true),
                    featuresScript = table.Column<string>(nullable: true),
                    projectId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Features", x => x.featuresId);
                });

            migrationBuilder.CreateTable(
                name: "FeaturesDepend",
                columns: table => new
                {
                    featuresDependsId = table.Column<int>(nullable: false)
                        .Annotation("MySQL:AutoIncrement", true),
                    featuresId = table.Column<int>(nullable: false),
                    fileMarkdown = table.Column<string>(nullable: true),
                    fileName = table.Column<string>(nullable: true),
                    filePath = table.Column<string>(nullable: true),
                    fileType = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeaturesDepend", x => x.featuresDependsId);
                });

            migrationBuilder.CreateTable(
                name: "Project",
                columns: table => new
                {
                    projectId = table.Column<int>(nullable: false)
                        .Annotation("MySQL:AutoIncrement", true),
                    projectMarkDown = table.Column<string>(nullable: true),
                    projectName = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Project", x => x.projectId);
                });

            migrationBuilder.CreateTable(
                name: "ProjectDepend",
                columns: table => new
                {
                    dependId = table.Column<int>(nullable: false)
                        .Annotation("MySQL:AutoIncrement", true),
                    fileMarkdown = table.Column<string>(nullable: true),
                    fileName = table.Column<string>(nullable: true),
                    filePath = table.Column<string>(nullable: true),
                    fileType = table.Column<string>(nullable: true),
                    projectId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectDepend", x => x.dependId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FeaIgnoreProDepend");

            migrationBuilder.DropTable(
                name: "Features");

            migrationBuilder.DropTable(
                name: "FeaturesDepend");

            migrationBuilder.DropTable(
                name: "Project");

            migrationBuilder.DropTable(
                name: "ProjectDepend");
        }
    }
}
