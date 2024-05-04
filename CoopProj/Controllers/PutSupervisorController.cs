using CoopProj.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoopProj.Controllers
{
    public class PutSupervisorController : Controller
    {
        private readonly OurDB _context;

        public PutSupervisorController(OurDB context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<ActionResult> UpdateSupervisorAsync(List<Students> students)
        {
            if (ModelState.IsValid)
            {
                foreach (var student in students)
                {
                    var existingStudent = await _context.Students.FindAsync(student.Id);
                    existingStudent.SupervisorID = student.SupervisorID;
                    _context.Entry(existingStudent).State = EntityState.Modified;
                }

                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }



    }

}
