using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CoopProj.Models;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting.Internal;

namespace CoopProj.Controllers
{
    public class ReportsController : Controller
    {
        private readonly OurDB _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public ReportsController(OurDB context, IWebHostEnvironment webHostEnvironment, IWebHostEnvironment hostingEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _hostingEnvironment = hostingEnvironment;
        }

        // GET: Reports
        public async Task<IActionResult> Index()
        {
            var ourDB = _context.Reports.Include(r => r.CreateReport.ReportName).Include(r => r.StudentReport).Where(u=>u.StudentReportID == Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value));
            return View(await ourDB.ToListAsync());
        }

        // GET: Reports/Details/5
        public async Task<IActionResult> Details(Guid? id)
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
                ViewData["notfound"] = " Report not found";
            }
            return View(reports);
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
            }catch
            {
                ViewData["notfound"] = " Report not found";
            
            }


            return View(reports);
        }


        // GET: Reports/Create
        public IActionResult Create(Guid? Id)
        {

            ViewBag.ReportId = Id;
            ViewData["CreateReportID"] = new SelectList(_context.CreateReport, "Id", "Id");
            ViewData["StudentReportID"] = new SelectList(_context.Students, "Id", "Email");
            return View();
        }

        // POST: Reports/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Guid ReportId, [Bind("Notes")] Reports reports, IFormFile SendReports)
        {
            if (ModelState.IsValid)
            {
                // Check if the student has already created a report with the same information
                bool isDuplicateReport = await _context.Reports
                    .AnyAsync(r => r.CreateReportID == ReportId && r.StudentReportID == Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value));

                if (isDuplicateReport)
                {
                    ModelState.AddModelError("", "You have already created a report with the same information.");
                }
                else
                {
                    if (SendReports != null)
                    {
                        string root_path = _webHostEnvironment.WebRootPath + "\\Reports\\";
                        string file_name = Guid.NewGuid().ToString().Replace("-", "") + Path.GetExtension(SendReports.FileName);
                        using (FileStream stream = System.IO.File.Create(root_path + file_name))
                        {
                            SendReports.CopyTo(stream);
                            stream.Flush();
                        }
                        reports.SendReports = file_name;
                    }

                    reports.CreateReportID = ReportId;
                    reports.StudentReportID = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                    reports.Time = DateTime.Now;
                    reports.Id = Guid.NewGuid();

                    _context.Add(reports);
                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));
                }
            }

            ViewData["CreateReportID"] = new SelectList(_context.CreateReport, "Id", "Id", reports.CreateReportID);
            ViewData["StudentReportID"] = new SelectList(_context.Students, "Id", "Email", reports.StudentReportID);
            return View(reports);
        }

        // GET: Reports/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
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
            ViewData["CreateReportID"] = new SelectList(_context.CreateReport, "Id", "Id", reports.CreateReportID);
            ViewData["StudentReportID"] = new SelectList(_context.Students, "Id", "Email", reports.StudentReportID);
            return View(reports);
        }

        // POST: Reports/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,Time,SendReports,FeedbackReports,Settings,Grade,Notes,Guid,StudentReportID,CreateReportID,Description")] Reports reports)
        {
            if (id != reports.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(reports);
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
                return RedirectToAction(nameof(Index));
            }
            ViewData["CreateReportID"] = new SelectList(_context.CreateReport, "Id", "Id", reports.CreateReportID);
            ViewData["StudentReportID"] = new SelectList(_context.Students, "Id", "Email", reports.StudentReportID);
            return View(reports);
        }

        // GET: Reports/Delete/5
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

        // POST: Reports/Delete/5
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
