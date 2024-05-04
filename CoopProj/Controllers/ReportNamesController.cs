using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CoopProj.Models;

namespace CoopProj.Controllers
{
    public class ReportNamesController : Controller
    {
        private readonly OurDB _context;

        public ReportNamesController(OurDB context)
        {
            _context = context;
        }

        // GET: ReportNames
        public async Task<IActionResult> Index()
        {
            return View(await _context.ReportName.ToListAsync());
        }

       

        // GET: ReportNames/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: ReportNames/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name")] ReportName reportName)
        {
            if (ModelState.IsValid)
            {
                reportName.Id = Guid.NewGuid();
                _context.Add(reportName);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(reportName);
        }

        // GET: ReportNames/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reportName = await _context.ReportName.FindAsync(id);
            if (reportName == null)
            {
                return NotFound();
            }
            return View(reportName);
        }

        // POST: ReportNames/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,Name")] ReportName reportName)
        {
            if (id != reportName.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(reportName);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ReportNameExists(reportName.Id))
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
            return View(reportName);
        }

        // GET: ReportNames/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reportName = await _context.ReportName
                .FirstOrDefaultAsync(m => m.Id == id);
            if (reportName == null)
            {
                return NotFound();
            }

            return View(reportName);
        }

        // POST: ReportNames/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var reportName = await _context.ReportName.FindAsync(id);
            _context.ReportName.Remove(reportName);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ReportNameExists(Guid id)
        {
            return _context.ReportName.Any(e => e.Id == id);
        }
    }
}
