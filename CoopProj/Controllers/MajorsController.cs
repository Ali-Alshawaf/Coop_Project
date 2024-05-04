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

namespace CoopProj.Controllers
{
    public class MajorsController : Controller
    {
        private readonly OurDB _context;

        public MajorsController(OurDB context)
        {
            _context = context;
        }

        // GET: Majors
        public async Task<IActionResult> Index()
        {
           return View(await _context.Majors.ToListAsync());
        }

       

        // GET: Majors/Create
        public IActionResult Create()
        {
                return View();
        }

        // POST: Majors/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name")] Majors majors)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    majors.Id = Guid.NewGuid();
                    _context.Add(majors);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                return View(majors);
            }
            catch 
            {
                return View("Error");
            }
        }

        // GET: Majors/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
           
                if (id == null)
                {
                    return NotFound();
                }

            var major = await _context.Majors.FindAsync(id);
            if (major == null)
                {
                    return NotFound();
                }

                return View(major);
            
            
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,Name")] Majors updatedMajor)
        {
            
                if (id != updatedMajor.Id)
                {
                    return NotFound();
                }

                if (ModelState.IsValid)
                {
                    try
                    {
                    var existingMajor = await _context.Majors.FirstOrDefaultAsync(m => m.Id == id);
                    if (existingMajor == null)
                        {
                            return NotFound();
                        }

                        existingMajor.Name = updatedMajor.Name; // Update the name

                        _context.Update(existingMajor);
                        await _context.SaveChangesAsync();
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!MajorsExists(updatedMajor.Id))
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
                return View(updatedMajor);
           
        }

        // GET: Majors/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            try
            {
                if (id == null)
                {
                    return NotFound();
                }

                var majors = await _context.Majors
                    .FirstOrDefaultAsync(m => m.Id == id);
                if (majors == null)
                {
                    return NotFound();
                }

                return View(majors);
            }
            catch
            {
                return View("Error");
            }
        }
        // POST: ApplyStudents/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            try
            {
                var major = await _context.Majors.FindAsync(id);
                _context.Majors.Remove(major);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }catch
            {
                return View("Error");
            }
        }


        private bool MajorsExists(Guid id)
        {
            return _context.Majors.Any(e => e.Id == id);
        }
    }
}
