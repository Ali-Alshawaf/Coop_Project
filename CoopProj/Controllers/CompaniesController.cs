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
using BCrypt.Net;


namespace CoopProj.Controllers
{
    public class CompaniesController : Controller
    {
        private readonly OurDB _context;

        public CompaniesController(OurDB context)
        {
            _context = context;
        }

        [Authorize(Roles = "Admin")]
        // GET: Companies
        public async Task<IActionResult> Index(string searchTerm)
        {
            var companies = await _context.Companies.ToListAsync();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                var searchTermLower = searchTerm.ToLower(); // Convert search term to lowercase

                companies = companies.Where(c => c.Name.ToLower().Contains(searchTermLower)).ToList();
            }
            ViewData["SearchTerm"] = searchTerm; // Store the search term in ViewData

            return View(companies);
        }

        // GET: Companies/Details/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var companies = await _context.Companies
                .FirstOrDefaultAsync(m => m.Id == id);
            if (companies == null)
            {
                return NotFound();
            }
            return View(companies);
        }

        // GET: Companies/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Companies/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]

        public async Task<IActionResult> Create([Bind("Id,Name,ContactName,ContactNumber,Email,Description,PassWord")] Companies companies)
        {
            if (ModelState.IsValid)
            {
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(companies.PassWord);
                companies.PassWord = hashedPassword;
                companies.Id = Guid.NewGuid();
                _context.Add(companies);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(companies);
        }

        // GET: Companies/Edit/5
        [Authorize(Roles = "Admin")]

        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var companies = await _context.Companies.FindAsync(id);
            if (companies == null)
            {
                return NotFound();
            }
            return View(companies);
        }

        // POST: Companies/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]

        public async Task<IActionResult> Edit(Guid id, [Bind("Id,Name,ContactName,ContactNumber,Email,Description,PassWord")] Companies companies)
        {
            if (id != companies.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(companies);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CompaniesExists(companies.Id))
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
            return View(companies);
        }

        // GET: Companies/Delete/5
        [Authorize(Roles = "Admin")]

        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var companies = await _context.Companies
                .FirstOrDefaultAsync(m => m.Id == id);
            if (companies == null)
            {
                return NotFound();
            }

            return View(companies);
        }

        // POST: Companies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]

        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var companies = await _context.Companies.FindAsync(id);
            _context.Companies.Remove(companies);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CompaniesExists(Guid id)
        {
            return _context.Companies.Any(e => e.Id == id);
        }
    }
}
