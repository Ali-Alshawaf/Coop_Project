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
    public class RequestsStudentController : Controller
    {
        private readonly OurDB _context;

        public RequestsStudentController(OurDB context)
        {
            _context = context;
        }

        // GET: RequestsStudent
        [Authorize(Roles = "Student")]

        public async Task<IActionResult> Index()
        {
            Guid userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            Students studentInfo = _context.Students.Include(m => m.Major).SingleOrDefault(s => s.Id == userId);

            var ourDB = _context.Requests
                .Include(cr => cr.Companies)
                .Where(cr => !_context.ApplyStudent.Any(r => r.RequestsID == cr.Id && r.StudentsID == userId && r.Status != "Rejected") && cr.Quantity != 0 && cr.EndDate > DateTime.Now && cr.Major.Contains(studentInfo.Major.Id.ToString()));

            return View(await ourDB.ToListAsync());
        }

        // GET: RequestsStudent/Details/5
        [Authorize(Roles = "Student")]
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

       
        private bool RequestsExists(Guid id)
        {
            return _context.Requests.Any(e => e.Id == id);
        }
    }
}
