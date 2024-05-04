using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace CoopProj.Migrations
{
    public partial class _1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Companies",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    ContactName = table.Column<string>(nullable: true),
                    ContactNumber = table.Column<int>(nullable: false),
                    Email = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    PassWord = table.Column<string>(nullable: true),
                    IsDeleted = table.Column<bool>(nullable: true),
                    ResetToken = table.Column<string>(nullable: true),
                    RegistertionTime = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Companies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ForgotPasswordViewModel",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ForgotPasswordViewModel", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Majors",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Majors", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReportName",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportName", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ResetPasswordViewModel",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(nullable: false),
                    NewPassword = table.Column<string>(nullable: false),
                    ConfirmPassword = table.Column<string>(nullable: true),
                    Token = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResetPasswordViewModel", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Requests",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CompaniesID = table.Column<Guid>(nullable: false),
                    StartDate = table.Column<DateTime>(nullable: false),
                    EndDate = table.Column<DateTime>(nullable: false),
                    Application = table.Column<string>(nullable: true),
                    Location = table.Column<string>(nullable: true),
                    Major = table.Column<string>(nullable: true),
                    Quantity = table.Column<int>(nullable: false),
                    RegistertionTime = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Requests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Requests_Companies_CompaniesID",
                        column: x => x.CompaniesID,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(nullable: false),
                    Email = table.Column<string>(nullable: false),
                    NumberPhone = table.Column<int>(nullable: false),
                    PassWord = table.Column<string>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: true),
                    RolesID = table.Column<int>(nullable: false),
                    ResetToken = table.Column<string>(nullable: true),
                    RegistertionTime = table.Column<DateTime>(nullable: false),
                    Guid = table.Column<Guid>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_Roles_RolesID",
                        column: x => x.RolesID,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CreateReport",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    ReportNameId = table.Column<Guid>(nullable: false),
                    StartTime = table.Column<DateTime>(nullable: false),
                    EndTime = table.Column<DateTime>(nullable: false),
                    SupervisorReportID = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CreateReport", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CreateReport_ReportName_ReportNameId",
                        column: x => x.ReportNameId,
                        principalTable: "ReportName",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CreateReport_Users_SupervisorReportID",
                        column: x => x.SupervisorReportID,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Students",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    Email = table.Column<string>(nullable: false),
                    NumberPhone = table.Column<int>(nullable: false),
                    UniversityID = table.Column<int>(nullable: false),
                    PassWord = table.Column<string>(nullable: false),
                    AccessFile = table.Column<string>(nullable: true),
                    ConfirmationToken = table.Column<string>(nullable: true),
                    ConfirmationTokenPass = table.Column<string>(nullable: true),
                    IsEmailConfirmed = table.Column<bool>(nullable: false),
                    MajorId = table.Column<Guid>(nullable: false),
                    Access = table.Column<bool>(nullable: true),
                    IsDeleted = table.Column<bool>(nullable: true),
                    SupervisorID = table.Column<int>(nullable: true),
                    RegistertionTime = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Students", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Students_Majors_MajorId",
                        column: x => x.MajorId,
                        principalTable: "Majors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Students_Users_SupervisorID",
                        column: x => x.SupervisorID,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ApplyStudent",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    StudentsID = table.Column<Guid>(nullable: false),
                    RequestsID = table.Column<Guid>(nullable: false),
                    Information = table.Column<string>(nullable: true),
                    Status = table.Column<string>(nullable: true),
                    Letter = table.Column<string>(nullable: true),
                    File = table.Column<string>(nullable: true),
                    Transcript = table.Column<string>(nullable: true),
                    StartTrining = table.Column<DateTime>(nullable: false),
                    EndTrining = table.Column<DateTime>(nullable: false),
                    RequestTime = table.Column<DateTime>(nullable: false),
                    PDFINFO = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplyStudent", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApplyStudent_Requests_RequestsID",
                        column: x => x.RequestsID,
                        principalTable: "Requests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ApplyStudent_Students_StudentsID",
                        column: x => x.StudentsID,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EmailModel",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    EmailContent = table.Column<string>(nullable: true),
                    Subject = table.Column<string>(nullable: true),
                    SenderID = table.Column<int>(nullable: false),
                    EmailStudentID = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailModel", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmailModel_Students_EmailStudentID",
                        column: x => x.EmailStudentID,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EmailModel_Users_SenderID",
                        column: x => x.SenderID,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Reports",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Time = table.Column<DateTime>(nullable: false),
                    SendReports = table.Column<string>(nullable: true),
                    FeedbackReports = table.Column<string>(nullable: true),
                    Settings = table.Column<DateTime>(nullable: true),
                    Grade = table.Column<float>(nullable: true),
                    Notes = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    StudentReportID = table.Column<Guid>(nullable: false),
                    CreateReportID = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reports_CreateReport_CreateReportID",
                        column: x => x.CreateReportID,
                        principalTable: "CreateReport",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Reports_Students_StudentReportID",
                        column: x => x.StudentReportID,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApplyStudent_RequestsID",
                table: "ApplyStudent",
                column: "RequestsID");

            migrationBuilder.CreateIndex(
                name: "IX_ApplyStudent_StudentsID",
                table: "ApplyStudent",
                column: "StudentsID");

            migrationBuilder.CreateIndex(
                name: "IX_CreateReport_ReportNameId",
                table: "CreateReport",
                column: "ReportNameId");

            migrationBuilder.CreateIndex(
                name: "IX_CreateReport_SupervisorReportID",
                table: "CreateReport",
                column: "SupervisorReportID");

            migrationBuilder.CreateIndex(
                name: "IX_EmailModel_EmailStudentID",
                table: "EmailModel",
                column: "EmailStudentID");

            migrationBuilder.CreateIndex(
                name: "IX_EmailModel_SenderID",
                table: "EmailModel",
                column: "SenderID");

            migrationBuilder.CreateIndex(
                name: "IX_Reports_CreateReportID",
                table: "Reports",
                column: "CreateReportID");

            migrationBuilder.CreateIndex(
                name: "IX_Reports_StudentReportID",
                table: "Reports",
                column: "StudentReportID");

            migrationBuilder.CreateIndex(
                name: "IX_Requests_CompaniesID",
                table: "Requests",
                column: "CompaniesID");

            migrationBuilder.CreateIndex(
                name: "IX_Students_MajorId",
                table: "Students",
                column: "MajorId");

            migrationBuilder.CreateIndex(
                name: "IX_Students_SupervisorID",
                table: "Students",
                column: "SupervisorID");

            migrationBuilder.CreateIndex(
                name: "IX_Users_RolesID",
                table: "Users",
                column: "RolesID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApplyStudent");

            migrationBuilder.DropTable(
                name: "EmailModel");

            migrationBuilder.DropTable(
                name: "ForgotPasswordViewModel");

            migrationBuilder.DropTable(
                name: "Reports");

            migrationBuilder.DropTable(
                name: "ResetPasswordViewModel");

            migrationBuilder.DropTable(
                name: "Requests");

            migrationBuilder.DropTable(
                name: "CreateReport");

            migrationBuilder.DropTable(
                name: "Students");

            migrationBuilder.DropTable(
                name: "Companies");

            migrationBuilder.DropTable(
                name: "ReportName");

            migrationBuilder.DropTable(
                name: "Majors");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Roles");
        }
    }
}
