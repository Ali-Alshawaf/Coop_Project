using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CoopProj.Models;
using System.Security.Claims;

namespace CoopProj.Controllers
{
    public class CreateReportsController : Controller
    {
        private readonly OurDB _context;

        public CreateReportsController(OurDB context)
        {
            _context = context;
        }

        // GET: CreateReports
        public async Task<IActionResult> Supervisor()
        {
            var ourDB = _context.CreateReport.Include(u => u.ReportName).Include(c => c.SupervisorReport).Where(c=>c.SupervisorReportID == int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value));
            return View(await ourDB.ToListAsync());
        }

        public async Task<IActionResult> Student()
        {
            Students studentInfo = _context.Students.SingleOrDefault(s => s.Id == Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value));

            var ourDB = _context.CreateReport.Include(u => u.ReportName)
                .Include(cr => cr.SupervisorReport)
                .Where(cr => cr.SupervisorReportID == studentInfo.SupervisorID)
                .Where(cr => !_context.Reports.Any(r => r.CreateReportID == cr.Id && r.StudentReportID == studentInfo.Id && r.SendReports != null));

            return View(await ourDB.ToListAsync());
        }



        // GET: CreateReports/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var createReport = await _context.CreateReport
                .Include(c => c.SupervisorReport)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (createReport == null)
            {
                return NotFound();
            }

            return View(createReport);
        }

        // GET: CreateReports/Create
        public IActionResult Create()
        {
            ViewData["ReportNameId"] = new SelectList(_context.ReportName, "Id", "Name");
            ViewData["SupervisorReportID"] = new SelectList(_context.Users, "Id", "Email");
            return View();
        }

        // POST: CreateReports/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ReportNameId,StartTime,EndTime,SupervisorReportID")] CreateReport createReport)
        {
            if (ModelState.IsValid)
            {
                createReport.SupervisorReportID = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                _context.Add(createReport);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Supervisor));
            }
            ViewData["ReportNameId"] = new SelectList(_context.ReportName, "Id", "Name", createReport.ReportNameId);
            ViewData["SupervisorReportID"] = new SelectList(_context.Users, "Id", "Email", createReport.SupervisorReportID);
            return View(createReport);
        }

        // GET: CreateReports/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var createReport = await _context.CreateReport.FindAsync(id);
            if (createReport == null)
            {
                return NotFound();
            }
            ViewData["ReportNameId"] = new SelectList(_context.ReportName, "Id", "Name");

            ViewData["SupervisorReportID"] = new SelectList(_context.Users, "Id", "Email", createReport.SupervisorReportID);
            return View(createReport);
        }

        // POST: CreateReports/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,ReportName,StartTime,EndTime,SupervisorReportID")] CreateReport createReport)
        {
            if (id != createReport.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingReport = await _context.CreateReport.FindAsync(id);
                    if (existingReport == null)
                    {
                        return NotFound();
                    }

                    existingReport.ReportName = createReport.ReportName;
                    existingReport.StartTime = createReport.StartTime;
                    existingReport.EndTime = createReport.EndTime;

                    _context.Update(existingReport);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CreateReportExists(createReport.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Supervisor));
            }
            ViewData["ReportNameId"] = new SelectList(_context.ReportName, "Id", "Name", createReport.ReportNameId);
            ViewData["SupervisorReportID"] = new SelectList(_context.Users, "Id", "Email", createReport.SupervisorReportID);
            return View(createReport);
        }

        // GET: CreateReports/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var createReport = await _context.CreateReport
                .Include(c => c.SupervisorReport)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (createReport == null)
            {
                return NotFound();
            }

            return View(createReport);
        }

        // POST: CreateReports/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var createReport = await _context.CreateReport.FindAsync(id);
            _context.CreateReport.Remove(createReport);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CreateReportExists(Guid id)
        {
            return _context.CreateReport.Any(e => e.Id == id);
        }
    }
}
