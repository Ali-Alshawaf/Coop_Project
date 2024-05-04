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

namespace CoopProj.Controllers
{
    public class RequestsController : Controller
    {
        private readonly OurDB _context;

        public RequestsController(OurDB context)
        {
            _context = context;
        }

        // GET: Requests

        [Authorize(Roles = "Admin")] 
        public ActionResult Index(Guid? CompaniesID)
        {

            if (CompaniesID.HasValue)
            {
                var requests = _context.Requests.Include(u => u.Companies).Where(r => r.CompaniesID == CompaniesID.Value).ToList();
                return View(requests);

            }
            else
            {
                var requests = _context.Requests.Include(u => u.Companies).ToList();
                return View(requests);
            }
        }

        // GET: Requests/Details/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Details(Guid? id)
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

        // GET: Requests/Create
        [Authorize(Roles = "Admin")]

        public IActionResult Create()
        {
            ViewData["CompaniesID"] = new SelectList(_context.Companies, "Id", "Id");
            return View();
        }

        // POST: Requests/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]

        public async Task<IActionResult> Create([Bind("Id,CompaniesID,StartDate,EndDate,Application,Location,RegistertionTime")] Requests requests)
        {
            if (ModelState.IsValid)
            {
                requests.Id = Guid.NewGuid();
                requests.RegistertionTime = DateTime.Now;
                _context.Add(requests);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CompaniesID"] = new SelectList(_context.Companies, "Id", "Id", requests.CompaniesID);
            return View(requests);
        }


        // GET: Requests/Edit/5
        [Authorize(Roles = "Admin")]

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
            ViewData["CompaniesID"] = new SelectList(_context.Companies, "Id", "Id", requests.CompaniesID);
            return View(requests);
        }

        // POST: Requests/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]

        public async Task<IActionResult> Edit(Guid id, [Bind("Id,CompaniesID,StartDate,EndDate,Application,Location")] Requests requests)
        {
            if (id != requests.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
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
            ViewData["CompaniesID"] = new SelectList(_context.Companies, "Id", "Id", requests.CompaniesID);
            return View(requests);
        }

        // GET: Requests/Delete/5
        [Authorize(Roles = "Admin")]

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

        // POST: Requests/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]

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
    }
}
