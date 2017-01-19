using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace WebApplication.Data.Migrations
{
    public partial class MyMigration2017 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "projectMdText",
                table: "Project",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "featuresMdText",
                table: "Features",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectDepend_projectId",
                table: "ProjectDepend",
                column: "projectId");

            migrationBuilder.CreateIndex(
                name: "IX_FeaturesDepend_featuresId",
                table: "FeaturesDepend",
                column: "featuresId");

            migrationBuilder.AddForeignKey(
                name: "FK_FeaturesDepend_Features_featuresId",
                table: "FeaturesDepend",
                column: "featuresId",
                principalTable: "Features",
                principalColumn: "featuresId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectDepend_Project_projectId",
                table: "ProjectDepend",
                column: "projectId",
                principalTable: "Project",
                principalColumn: "projectId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FeaturesDepend_Features_featuresId",
                table: "FeaturesDepend");

            migrationBuilder.DropForeignKey(
                name: "FK_ProjectDepend_Project_projectId",
                table: "ProjectDepend");

            migrationBuilder.DropIndex(
                name: "IX_ProjectDepend_projectId",
                table: "ProjectDepend");

            migrationBuilder.DropIndex(
                name: "IX_FeaturesDepend_featuresId",
                table: "FeaturesDepend");

            migrationBuilder.DropColumn(
                name: "projectMdText",
                table: "Project");

            migrationBuilder.DropColumn(
                name: "featuresMdText",
                table: "Features");
        }
    }
}
