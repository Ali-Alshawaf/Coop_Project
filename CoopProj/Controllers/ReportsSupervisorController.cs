using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CoopProj.Models;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using System.Security.AccessControl;

namespace CoopProj.Controllers
{
    public class ReportsSupervisorController : Controller
    {
        private readonly OurDB _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IWebHostEnvironment _hostingEnvironment;


        public ReportsSupervisorController(OurDB context , IWebHostEnvironment webHostEnvironment, IWebHostEnvironment hostingEnvironmen)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _hostingEnvironment = hostingEnvironmen;
        }

        // GET: ReportsSupervisor
        public async Task<IActionResult> Index(Guid? Id)
        {
            var ourDB = await _context.Reports
                .Include(r => r.CreateReport.ReportName)
                .Include(r => r.StudentReport)
                .Where(u => u.CreateReport.Id == Id)
                .ToListAsync();

            return View(ourDB);
        }
       
        public async Task<IActionResult> Grade(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reports = await _context.Reports
                .Include(r => r.CreateReport.ReportName)
                .Include(r => r.StudentReport).Include(r => r.CreateReport.SupervisorReport)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (reports == null)
            {
                return NotFound();
            }


            try
            {
                string fileRootPath = Path.Combine(_hostingEnvironment.WebRootPath, "Reports");

                string filePath = Path.Combine(fileRootPath, reports.FeedbackReports);


                // Read the file content
                byte[] reportContent = await System.IO.File.ReadAllBytesAsync(filePath);


                // Convert file content to base64 string
                string base64File = Convert.ToBase64String(reportContent);


                // Pass the base64 strings to the view
                ViewData["FeedbackContent"] = base64File;
            }
            catch
            {
                ViewData["notfound"] = " Report not found";

            }


            return View(reports);
        }


        // GET: ReportsSupervisor/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reports = await _context.Reports
                .Include(r => r.CreateReport.ReportName)
                .Include(r => r.StudentReport)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (reports == null)
            {
                return NotFound();
            }
            try
            {
                string fileRootPath = Path.Combine(_hostingEnvironment.WebRootPath, "Reports");

                string filePath = Path.Combine(fileRootPath, reports.SendReports);

                // Read the file content
                byte[] reportContent = await System.IO.File.ReadAllBytesAsync(filePath);

                // Convert file content to base64 string
                string base64File = Convert.ToBase64String(reportContent);

                // Pass the base64 strings to the view
                ViewData["ReportContent"] = base64File;
            }
            catch
            {
                ViewData["NoFile"] = " There is no file";
            }


            return View(reports);
        }



        // GET: Reports/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reports = await _context.Reports.FindAsync(id);
            if (reports == null)
            {
                return NotFound();
            }

            return View(reports);
        }

        // POST: Reports/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,FeedbackReports,Grade,Description,Settings")] Reports reports, IFormFile FeedbackReports)
        {
            if (id != reports.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (FeedbackReports != null)
                    {
                        string root_path = _webHostEnvironment.WebRootPath + "\\Reports\\";
                        string file_name = Guid.NewGuid().ToString().Replace("-", "") + Path.GetExtension(FeedbackReports.FileName);
                        using (FileStream stream = System.IO.File.Create(root_path + file_name))
                        {
                            FeedbackReports.CopyTo(stream);
                            stream.Flush();
                        }
                        reports.FeedbackReports = file_name; // Save Image name Inside model
                    }
                    var existingReports = await _context.Reports.FindAsync(id);
                    if (existingReports == null)
                    {
                        return NotFound();
                    }

                    existingReports.FeedbackReports = reports.FeedbackReports;
                    existingReports.Grade = reports.Grade;
                    existingReports.Description = reports.Description;
                    existingReports.Settings = DateTime.Now;

                    

                    _context.Update(existingReports);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ReportsExists(reports.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Grade), new { id = reports.Id });
            }

            return View(reports);
        }

        // GET: ReportsSupervisor/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reports = await _context.Reports
                .Include(r => r.CreateReport)
                .Include(r => r.StudentReport)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (reports == null)
            {
                return NotFound();
            }

            return View(reports);
        }

        // POST: ReportsSupervisor/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var reports = await _context.Reports.FindAsync(id);
            _context.Reports.Remove(reports);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ReportsExists(Guid id)
        {
            return _context.Reports.Any(e => e.Id == id);
        }
    }
}
