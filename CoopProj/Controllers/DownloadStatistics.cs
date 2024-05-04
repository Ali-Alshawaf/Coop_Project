using CoopProj.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System;
using System.Data;
using System.Linq;

namespace CoopProj.Controllers
{
    public class DownloadStatistics : Controller
    {
        private readonly OurDB _context;


        public DownloadStatistics(OurDB context)
        {
            _context = context;
        }


        [HttpGet]
        [Authorize(Roles = "Admin")]

        public IActionResult Statistics()
        {
            return View();
        }


        [HttpPost]
        public FileResult CompanyStatistics(int selectedYear)
        {
            // Create a new Excel package
            using (var package = new ExcelPackage())
            {
                // Add a new worksheet to the Excel package
                var worksheet = package.Workbook.Worksheets.Add("Companies");

                // Set the column headers
                var headers = new string[] { "Company Name", "Email", "Contact Name", "Contact Number", "Description" }; // Replace with your actual column names
                for (int i = 0; i < headers.Length; i++)
                {
                    worksheet.Cells[1, i + 1].Value = headers[i];
                    worksheet.Cells[1, i + 1].Style.Font.Bold = true;
                }

                // Populate the data rows from the database
                var dataRows = _context.Companies.ToList(); // Replace "YourTable" with the name of your database table

                if (selectedYear != 0)
                {
                    dataRows = dataRows.Where(u => u.RegistertionTime.Year == selectedYear).ToList();
                }

                for (int i = 0; i < dataRows.Count; i++)
                {
                    var row = dataRows[i];

                    worksheet.Cells[i + 2, 1].Value = row.Name;
                    worksheet.Cells[i + 2, 2].Value = row.Email;
                    worksheet.Cells[i + 2, 3].Value = row.ContactName;
                    worksheet.Cells[i + 2, 4].Value = row.ContactNumber;
                    worksheet.Cells[i + 2, 5].Value = row.Description;
                    // Repeat the above line for each column in your table
                }

                // Auto-fit the columns
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                // Generate a unique filename for the Excel file
                string fileName = "data_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xlsx";

                // Return the Excel file as a FileResult
                byte[] fileBytes = package.GetAsByteArray();
                return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
        }

        [HttpPost]
        public FileResult SudentStatistics(int selectedYear)
        {
            // Create a new Excel package
            using (var package = new ExcelPackage())
            {
                // Add a new worksheet to the Excel package
                var worksheet = package.Workbook.Worksheets.Add("Students");

                // Set the column headers
                var headers = new string[] { "Sudent Name", "Email", "Supervisor" }; // Replace with your actual column names
                for (int i = 0; i < headers.Length; i++)
                {
                    worksheet.Cells[1, i + 1].Value = headers[i];
                    worksheet.Cells[1, i + 1].Style.Font.Bold = true;
                }

                // Populate the data rows from the database
                var dataRows = _context.Students.Include(u => u.Major).Include(s => s.Supervisor).ToList(); // Replace "YourTable" with the name of your database table

                if (selectedYear != 0)
                {
                    dataRows = dataRows.Where(u => u.RegistertionTime.Year == selectedYear).ToList();
                }

                for (int i = 0; i < dataRows.Count; i++)
                {
                    var row = dataRows[i];

                    worksheet.Cells[i + 2, 1].Value = row.Name;
                    worksheet.Cells[i + 2, 2].Value = row.Email;
                    worksheet.Cells[i + 2, 3].Value = row.Supervisor?.Name;
                    // Repeat the above line for each column in your table
                }

                // Auto-fit the columns
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                // Generate a unique filename for the Excel file
                string fileName = "data_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xlsx";

                // Return the Excel file as a FileResult
                byte[] fileBytes = package.GetAsByteArray();
                return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
        }


        [HttpPost]
        public FileResult SupervisorStatistics(int selectedYear)
        {
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Supervisors");

                var headers = new string[] { "Supervisor Name", "Email", "Number Phone", "Student Name", "Student Email" }; // Add student-related headers
                for (int i = 0; i < headers.Length; i++)
                {
                    worksheet.Cells[1, i + 1].Value = headers[i];
                    worksheet.Cells[1, i + 1].Style.Font.Bold = true;
                }

                var supervisors = _context.Users.Include(u => u.Roles).Where(u => u.Roles.Name == "Supervisor").ToList();

                if (selectedYear != 0)
                {
                    supervisors = _context.Users.Include(u => u.Roles).Where(u => u.Roles.Name == "Supervisor" && u.RegistertionTime.Year == selectedYear).ToList();
                }

                int currentRow = 2; // Start from the second row to avoid overwriting supervisor data

                foreach (var supervisor in supervisors)
                {
                    worksheet.Cells[currentRow, 1].Value = supervisor.Name;
                    worksheet.Cells[currentRow, 2].Value = supervisor.Email;
                    worksheet.Cells[currentRow, 3].Value = supervisor.NumberPhone;

                    var students = _context.Students.Where(s => s.SupervisorID == supervisor.Id).ToList();

                  

                    foreach (var student in students)
                    {
                        worksheet.Cells[currentRow, 4].Value = student.Name; // Place student-related data in the next columns
                        worksheet.Cells[currentRow, 5].Value = student.Email;

                        // Repeat the above two lines for each additional column related to students

                        currentRow++; // Move to the next row for the next student
                    }

                    currentRow++; // Move to the next row for the next supervisor
                }

                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                string fileName = "data_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xlsx";

                byte[] fileBytes = package.GetAsByteArray();
                return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
        }

        public FileResult OpportunityStatistics(int selectedYear)
        {
            // Create a new Excel package
            using (var package = new ExcelPackage())
            {
                // Add a new worksheet to the Excel package
                var worksheet = package.Workbook.Worksheets.Add("Opportunity");

                // Set the column headers
                var headers = new string[] { "Company Name", "Application" ,"Email" }; // Replace with your actual column names
                for (int i = 0; i < headers.Length; i++)
                {
                    worksheet.Cells[1, i + 1].Value = headers[i];
                    worksheet.Cells[1, i + 1].Style.Font.Bold = true;
                }

                // Populate the data rows from the database
                var dataRows = _context.Requests.Include(u => u.Companies).ToList(); // Replace "YourTable" with the name of your database table

                if (selectedYear != 0)
                {
                    dataRows = dataRows.Where(u => u.RegistertionTime.Year == selectedYear).ToList();
                }
                for (int i = 0; i < dataRows.Count; i++)
                {
                    var row = dataRows[i];

                    worksheet.Cells[i + 2, 1].Value = row.Companies.Name;
                    worksheet.Cells[i + 2, 2].Value = row.Application;
                    worksheet.Cells[i + 2, 3].Value = row.Companies.Email;

                    // Repeat the above line for each column in your table
                }

                // Auto-fit the columns
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                // Generate a unique filename for the Excel file
                string fileName = "data_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xlsx";

                // Return the Excel file as a FileResult
                byte[] fileBytes = package.GetAsByteArray();
                return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
        }

        public FileResult RequestStatistics(int selectedYear)
        {
            // Create a new Excel package
            using (var package = new ExcelPackage())
            {
                // Add a new worksheet to the Excel package
                var worksheet = package.Workbook.Worksheets.Add("Request");

                // Set the column headers
                var headers = new string[] { "Student Name", "University ID", "Companies Name", "Application", "Status" }; // Replace with your actual column names
                for (int i = 0; i < headers.Length; i++)
                {
                    worksheet.Cells[1, i + 1].Value = headers[i];
                    worksheet.Cells[1, i + 1].Style.Font.Bold = true;
                }

                // Get the list of companies
                var companies = _context.Companies.ToList();

                // Populate the data rows
                int rowIndex = 2;

                // Get the related ApplyStudent data based on the selected year
                var applyStudentData = _context.ApplyStudent
                    .Include(u => u.Students)
                    .Include(u => u.Requests)
                    .ToList();

                if (selectedYear != 0)
                {
                    applyStudentData = applyStudentData.Where(u => u.RequestTime.Year == selectedYear).ToList();
                }

                foreach (var applyStudent in applyStudentData)
                {
                    worksheet.Cells[rowIndex, 1].Value = applyStudent.Students.Name;
                    worksheet.Cells[rowIndex, 2].Value = applyStudent.Students.UniversityID;
                    worksheet.Cells[rowIndex, 3].Value = applyStudent.Requests.Companies.Name;
                    worksheet.Cells[rowIndex, 4].Value = applyStudent.Requests.Application;
                    worksheet.Cells[rowIndex, 5].Value = applyStudent.Status;
                    // Repeat the above line for each column in your table

                    rowIndex++;
                }

                // Auto-fit the columns
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                // Generate a unique filename for the Excel file
                string fileName = "data_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xlsx";

                // Return the Excel file as a FileResult
                byte[] fileBytes = package.GetAsByteArray();
                return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
        }


    }
}
