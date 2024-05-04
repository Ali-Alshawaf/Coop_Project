using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CoopProj.Models;
using Microsoft.AspNetCore.Authorization;
using System.Data;
using System.Security.Claims;
using OfficeOpenXml;
using System.IO;
using System.Net.Mail;
using System.Net;
using Microsoft.AspNetCore.Hosting;

namespace CoopProj.Controllers
{
    public class Requests1Controller : Controller
    {
        private readonly OurDB _context;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public Requests1Controller(OurDB context, IWebHostEnvironment hostingEnvironment, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _hostingEnvironment = hostingEnvironment;
            _webHostEnvironment = webHostEnvironment;
        }

      
            // GET: Requests
            [Authorize(Roles = "Company")]
        public async Task<IActionResult> Index()
        {
            var requests = await _context.Requests
                .Include(r => r.Companies)
                .Where(r => r.CompaniesID == Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                .ToListAsync();

            // Fetch the major names for each request
            foreach (var request in requests)
            {
                if (!string.IsNullOrEmpty(request.Major))
                {
                    var majorGuids = request.Major.Split(',');

                    var majorNames = await _context.Majors
                        .Where(m => majorGuids.Contains(m.Id.ToString()))
                        .Select(m => m.Name)
                        .ToListAsync();

                    request.Major = string.Join(",", majorNames); 
                }
            }

            return View(requests);
        }

        public async Task<IActionResult> Requests(Guid? id)
        {
            var ourDB = _context.ApplyStudent
                .Include(a => a.Requests.Companies)
                .Include(a => a.Students)
                .Where(c => c.Requests.CompaniesID == Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value) && c.Requests.Id == id)
                .OrderByDescending(a => a.GPA); 

            return View(await ourDB.ToListAsync());
        }
        public IActionResult DownloadExcel(Guid? id)
        {
            var dataRows = _context.ApplyStudent
                .Include(a => a.Requests.Companies)
                .Include(a => a.Students)
                .Where(c => c.Requests.CompaniesID == Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value) && c.Requests.Id == id)
                .ToList();

            // Create an Excel package using a library like EPPlus
            var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Students");


            var headers = new string[] { "Application", "Name", "Email", "Status", "GPA" }; // Add "Application" as the first header
            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cells[1, i + 1].Value = headers[i];
                worksheet.Cells[1, i + 1].Style.Font.Bold = true;
            }

            for (int i = 0; i < dataRows.Count; i++)
            {
                var row = dataRows[i];

                worksheet.Cells[i + 2, 1].Value = row.Requests.Application; // Set the "Application" value in the first column
                worksheet.Cells[i + 2, 2].Value = row.Students.Name;
                worksheet.Cells[i + 2, 3].Value = row.Students.Email;
                worksheet.Cells[i + 2, 4].Value = row.Status;
                worksheet.Cells[i + 2, 5].Value = row.GPA;

                // Set the column width based on the length of the name
                worksheet.Column(1).Width = row.Requests.Application.Length; // Adjust the first column width
                worksheet.Column(2).Width = row.Students.Name.Length;
                worksheet.Column(3).Width = row.Students.Email.Length;
                worksheet.Column(4).Width = row.Status.Length;
            }

            // Generate a unique filename
            var filename = $"Students_{DateTime.Now:yyyyMMddHHmmss}.xlsx";

            // Set the response headers to trigger the file download
            Response.Headers.Add("Content-Disposition", "attachment; filename=" + filename);
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

            // Write the Excel package to the response stream
            using (var stream = new MemoryStream())
            {
                package.SaveAs(stream);
                stream.Position = 0;
                stream.CopyTo(Response.Body);
            }

            return new EmptyResult();
        }

        // GET: Requests1/Details/5
        [Authorize(Roles = "Company")]

        public async Task<IActionResult> RejectStudent()
        {
            string reject = "Rejected";
            var ourDB = _context.ApplyStudent.Include(a => a.Requests.Companies).Include(a => a.Students).Where(c => c.Requests.CompaniesID == Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value) && c.Status == reject); ;
            return View(await ourDB.ToListAsync());
        }

        // GET: ApplyStudentsCompany/Details/5
        [Authorize(Roles = "Company")]
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var applyStudent = await _context.ApplyStudent
                .Include(a => a.Requests)
                .Include(a => a.Students)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (applyStudent == null)
            {
                return NotFound();
            }

            try
            {
                // Get the file paths
                string fileRootPath = Path.Combine(_hostingEnvironment.WebRootPath, "CVS");
                string filePath = Path.Combine(fileRootPath, applyStudent.File);
                byte[] fileContent = await System.IO.File.ReadAllBytesAsync(filePath);
                string base64File = Convert.ToBase64String(fileContent);
                ViewData["FileContent"] = base64File;
            }
            catch
            {
                ViewData["Filenotfound"] = "The CV does not exist";
            }




            try
            {

                string letterRootPath = Path.Combine(_hostingEnvironment.WebRootPath, "Letters");
                string letterPath = Path.Combine(letterRootPath, applyStudent.Letter);
                byte[] letterContent = await System.IO.File.ReadAllBytesAsync(letterPath);
                string base64Letter = Convert.ToBase64String(letterContent);
                ViewData["LetterContent"] = base64Letter;

            }
            catch
            {
                ViewData["Letternotfound"] = "The Letter does not exist";
            }




            try
            {

                string transcriptRootPath = Path.Combine(_hostingEnvironment.WebRootPath, "Transcripts");
                string transcriptPath = Path.Combine(transcriptRootPath, applyStudent.Transcript);
                byte[] transcriptContent = await System.IO.File.ReadAllBytesAsync(transcriptPath);
                string base64Transcript = Convert.ToBase64String(transcriptContent);
                ViewData["TranscriptContent"] = base64Transcript;
            }
            catch
            {
                ViewData["Transcriptnotfound"] = "The Transcript does not exist";
            }

            return View(applyStudent);
        }


        [HttpPost]
        public async Task<IActionResult> Accept(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            ApplyStudent applyStudent = await _context.ApplyStudent.Include(R => R.Requests).ThenInclude(C => C.Companies).SingleOrDefaultAsync(S => S.Id == id);

            if (applyStudent == null)
            {
                return NotFound();
            }

            try
            {
                applyStudent.Status = "Accepted";
                Requests request = _context.Requests.FirstOrDefault(r => r.Id == applyStudent.RequestsID);
                if (request.Quantity == 0)
                {
                    ViewData["filenotfound"] = "Failed";
                    return View(applyStudent);
                }
                request.Quantity = request.Quantity - 1;
                _context.Update(applyStudent);
                _context.Update(request);
                _context.SaveChanges();

            }
            catch
            {
                ViewData["filenotfound"] = "The file is not found";
                return View(applyStudent);
            }
            return RedirectToAction("Details", new { id });

        }







        [HttpPost]
        public async Task<IActionResult> Reject(Guid id)
        {
            var applyStudent = await _context.ApplyStudent.FindAsync(id);
            if (applyStudent == null)
            {
                return NotFound();
            }

            applyStudent.Status = "Rejected";
            await _context.SaveChangesAsync();

            string studentEmail = _context.Students
                .Where(s => s.Id == applyStudent.StudentsID)
                .Select(s => s.Email)
                .FirstOrDefault();
            string name = _context.Students
                .Where(s => s.Id == applyStudent.StudentsID)
                .Select(s => s.Name)
                .FirstOrDefault();

            string company = _context.Requests
    .Where(r => r.Id == applyStudent.RequestsID)
    .Select(r => r.Companies.Name)
    .FirstOrDefault();

            string Opp = _context.Requests
    .Where(r => r.Id == applyStudent.RequestsID)
    .Select(r => r.Application)
    .FirstOrDefault();

            var smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential("good10manx@gmail.com", "vsuepgdfexnjbeih"),
                EnableSsl = true,
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress("good10manx@gmail.com"),
                Subject = "Your request has been rejected",
                Body = $"Dear student {name}\n\nyour request has been rejected in {company} : at {Opp} Application.",
                IsBodyHtml = false,
            };

            mailMessage.To.Add(studentEmail);

            smtpClient.Send(mailMessage);

            return RedirectToAction("Details", new { id });
        }


        // GET: Requests1/Create
        [Authorize(Roles = "Company")]

        public IActionResult Create()
        {
            ViewData["MajorId"] = new SelectList(_context.Majors, "Id", "Name");
            ViewData["CompaniesID"] = new SelectList(_context.Companies, "Id", "Id");
            return View();
        }

        // POST: Requests1/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Company")]
        public async Task<IActionResult> Create([Bind("Id,StartDate,EndDate,Application,Location,Quantity,Major")] Requests requests, Guid[] Major)
        {
            if (ModelState.IsValid)
            {
                if (requests.EndDate < requests.StartDate)
                {
                    ModelState.AddModelError("EndDate", "End date cannot be lower than the start date.");
                    // Re-populate the select lists
                    ViewData["MajorId"] = new SelectList(_context.Majors, "Id", "Name");
                    ViewData["CompaniesID"] = new SelectList(_context.Companies, "Id", "Id", requests.CompaniesID);

                    return View(requests);
                }

                if (Major != null && Major.Length > 0)
                {
                    requests.Major = string.Join(",", Major.Select(g => g.ToString()));
                }
                else
                {
                    requests.Major = Guid.Empty.ToString();
                }

                requests.CompaniesID = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                requests.RegistertionTime = DateTime.Now;
                requests.Id = Guid.NewGuid();

                _context.Add(requests);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            ViewData["MajorId"] = new SelectList(_context.Majors, "Id", "Name");
            ViewData["CompaniesID"] = new SelectList(_context.Companies, "Id", "Id", requests.CompaniesID);

            return View(requests);
        }

        // GET: Requests1/Edit/5
        [Authorize(Roles = "Company")]

        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var requests = await _context.Requests.FindAsync(id);
            if (requests == null)
            {
                return NotFound();
            }
            ViewData["MajorId"] = new SelectList(_context.Majors, "Id", "Name");
            ViewData["CompaniesID"] = new SelectList(_context.Companies, "Id", "Id", requests.CompaniesID);
            return View(requests);
        }

        // POST: Requests1/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Company")]

        public async Task<IActionResult> Edit(Guid id, [Bind("Id,CompaniesID,StartDate,EndDate,Application,Location,Quantity,Major")] Requests requests, Guid[] Major)
        {
            if (id != requests.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (Major != null && Major.Length > 0)
                    {
                        requests.Major = string.Join(",", Major.Select(g => g.ToString()));
                    }
                    else
                    {
                        requests.Major = Guid.Empty.ToString();
                    }
                    _context.Update(requests);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RequestsExists(requests.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["MajorId"] = new SelectList(_context.Majors, "Id", "Name");
            ViewData["CompaniesID"] = new SelectList(_context.Companies, "Id", "Id", requests.CompaniesID);
            return View(requests);
        }

        // GET: Requests1/Delete/5
        [Authorize(Roles = "Company")]

        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var requests = await _context.Requests
                .Include(r => r.Companies)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (requests == null)
            {
                return NotFound();
            }

            return View(requests);
        }



        // POST: Requests1/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Company")]

        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var requests = await _context.Requests.FindAsync(id);
            _context.Requests.Remove(requests);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RequestsExists(Guid id)
        {
            return _context.Requests.Any(e => e.Id == id);
        }
        //public FileResult CompanyStatistics(Guid id)
        //{
        //    // Retrieve the ApplyStudent data from the database
        //    var applyStudents = _context.ApplyStudent
        //        .Include(a => a.Requests.Companies)
        //        .Include(a => a.Students)
        //        .Where(c => c.Id == id)
        //        .ToList();

        //    // Create a new Excel package
        //    using (var package = new ExcelPackage())
        //    {
        //        // Create a worksheet in the Excel file
        //        var worksheet = package.Workbook.Worksheets.Add("ApplyStudent");

        //        // Define the column headers
        //        worksheet.Cells[1, 1].Value = "Name";
        //        worksheet.Cells[1, 2].Value = "Email";
        //        // ...

        //        for (int i = 0; i < applyStudents.Count; i++)
        //        {
        //            // Fill in the data for each row
        //            worksheet.Cells[i + 2, 1].Value = applyStudents[i].Students.Name;
        //            worksheet.Cells[i + 2, 2].Value = applyStudents[i].Students.Email;
        //            // ...
        //        }

        //        // Auto-fit the columns for better visibility
        //        worksheet.Cells.AutoFitColumns();

        //        // Convert the Excel package to a byte array
        //        var fileBytes = package.GetAsByteArray();

        //        // Return the Excel file to the client for download
        //        return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "ApplyStudent.xlsx");
        //    }
        //}
    }
}



