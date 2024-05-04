
using CoopProj.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

public class AllSheets : Controller
    {

    private readonly OurDB _context;


    public AllSheets(OurDB context)
    {
        _context = context;
    }
    public FileResult DownloadStatistics()
    {
        using (var package = new ExcelPackage())
        {
            // Add worksheet for Companies
            var companiesWorksheet = package.Workbook.Worksheets.Add("Companies");
            SetHeaders(companiesWorksheet, "Company Name", "Email", "Contact Name", "Contact Number", "Description");

            // Populate Companies data
            var companiesData = _context.Companies.ToList();
            PopulateData(companiesWorksheet, companiesData, c => c.Name, c => c.Email, c => c.ContactName, c => c.ContactNumber, c => c.Description);

            // Add worksheet for Students
            var studentsWorksheet = package.Workbook.Worksheets.Add("Students");
            SetHeaders(studentsWorksheet, "Student Name", "Email", "Supervisor");

            // Populate Students data
            var studentsData = _context.Students.Include(u => u.Major).Include(s => s.Supervisor).ToList();
            PopulateData(studentsWorksheet, studentsData, s => s.Name, s => s.Email, s => s.Supervisor?.Name);

            // Add worksheet for Supervisors
            var supervisorsWorksheet = package.Workbook.Worksheets.Add("Supervisors");
            SetHeaders(supervisorsWorksheet, "Supervisor Name", "Email", "Student Name", "Student Email");

            var supervisorsData = _context.Users.Include(u => u.Roles).Where(u => u.Roles.Name == "Supervisor").ToList();
            PopulateSupervisorData(supervisorsWorksheet, supervisorsData, _context.Students);

            // Add worksheet for Opportunities
            var opportunitiesWorksheet = package.Workbook.Worksheets.Add("Opportunities");
            SetHeaders(opportunitiesWorksheet, "Company ", "Application");

            // Populate Opportunities data
            var opportunitiesData = _context.Requests.Include(o => o.Companies).ToList();
            PopulateData(opportunitiesWorksheet, opportunitiesData,  o => o.Companies.Name,o => o.Application);

            // Add worksheet for Requests
            var requestsWorksheet = package.Workbook.Worksheets.Add("Requests");
            SetHeaders(requestsWorksheet, "Company","Opportunity", "Student", "Status");

            // Populate Requests data
            var requestsData = _context.ApplyStudent.Include(u => u.Students).Include(u => u.Requests).ToList();
            PopulateData(requestsWorksheet, requestsData, r => r.Requests.Companies.Name , r => r.Requests.Application, r => r.Students.Email, r => r.Status);

            // Generate a unique filename for the Excel file
            string fileName = "data_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xlsx";

            // Return the Excel file as a FileResult
            byte[] fileBytes = package.GetAsByteArray();
            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
    }
    private void SetHeaders(ExcelWorksheet worksheet, params string[] headers)
    {
        for (int i = 0; i < headers.Length; i++)
        {
            worksheet.Cells[1, i + 1].Value = headers[i];
            worksheet.Cells[1, i + 1].Style.Font.Bold = true;
            worksheet.Column(i + 1).AutoFit(); // Adjust column width to fit the header content
        }

    }

    private void PopulateData<T>(ExcelWorksheet worksheet, List<T> dataRows, params Func<T, object>[] columnSelectors)
    {
        for (int i = 0; i < dataRows.Count; i++)
        {
            var row = dataRows[i];

            for (int j = 0; j < columnSelectors.Length; j++)
            {
                var value = columnSelectors[j](row);
                worksheet.Cells[i + 2, j + 1].Value = value?.ToString() ?? string.Empty;
                worksheet.Columns[j + 1].AutoFit(); // Adjust the width of the column
            }
        }
    }
    private void PopulateSupervisorData(ExcelWorksheet worksheet, List<Users> supervisors, DbSet<Students> students)
    {
        int currentRow = 2;

        foreach (var supervisor in supervisors)
        {
            worksheet.Cells[currentRow, 1].Value = supervisor.Name;
            worksheet.Cells[currentRow, 2].Value = supervisor.Email;

            var assignedStudents = students.Where(s => s.SupervisorID == supervisor.Id).ToList();
            foreach (var student in assignedStudents)
            {
                worksheet.Cells[currentRow, 3].Value = student.Name;
                worksheet.Cells[currentRow, 4].Value = student.Email;

                currentRow++;
            }

            currentRow++;
        }
    }
}
