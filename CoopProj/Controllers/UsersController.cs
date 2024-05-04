using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CoopProj.Models;
using Microsoft.AspNetCore.Authorization;
using BCrypt.Net;


namespace CoopProj.Controllers
{
    public class UsersController : Controller
    {
        private readonly OurDB _context;

        public UsersController(OurDB context)
        {
            _context = context;
        }

        // GET: Users
        [Authorize(Roles = "Admin")]

        public async Task<IActionResult> Index()
        {
            var admins = await _context.Users.Include(u => u.Roles).Where(u => u.Roles.Name == "admin").ToListAsync();
            return View(admins);
        }
        public async Task<IActionResult> Supervisor()
        {
            var supervisors = await _context.Users.Include(u => u.Roles).Where(u => u.Roles.Name == "supervisor").ToListAsync();
            //var coops = await _context.Users.Include(u => u.Roles).Where(u => u.Roles.Name == "Coop").ToListAsync();

            return View(supervisors);
        }



        // GET: Users/Details/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var users = await _context.Users
                .FirstOrDefaultAsync(m => m.Id == id);
            if (users == null)
            {
                return NotFound();
            }
            var relatedStudents = await _context.Students.Include(u => u.Major)
        .Where(s => s.SupervisorID == id)
        .ToListAsync();

            ViewData["RelatedStudents"] = relatedStudents;

            return View(users);
        }

        // GET: Users/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            ViewData["RolesID"] = new SelectList(_context.Roles, "Id", "Name");

            return View();
        }

        // POST: Users/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("Id,Name,Email,NumberPhone,PassWord")] Users users)
        {
            if (ModelState.IsValid)
            {
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(users.PassWord);
                users.RegistertionTime = DateTime.Now;
                users.PassWord = hashedPassword;
                users.RolesID = 1;
                users.Guid = Guid.NewGuid();
                _context.Add(users);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["RolesID"] = new SelectList(_context.Roles, "Id", "Name", users.RolesID);

            return View(users);
        }




        public IActionResult CreateSupervisor()
        {
            ViewData["RolesID"] = new SelectList(_context.Roles, "Id", "Name");

            return View();
        }

        // POST: Users/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateSupervisor([Bind("Id,Name,Email,NumberPhone,PassWord,RolesID,RegistertionTime")] Users users)
        {
            if (ModelState.IsValid)
            {
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(users.PassWord);
                users.RegistertionTime = DateTime.Now;
                users.PassWord = hashedPassword;
                users.RolesID = 2;
                users.Guid = Guid.NewGuid();
                _context.Add(users);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Supervisor));
            }
            ViewData["RolesID"] = new SelectList(_context.Roles, "Id", "Name", users.RolesID);
            return View(users);
        }




        // GET: Users/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            ViewData["OldPasswordHash"] = user.PassWord;
            ViewData["OldRole"] = user.RolesID;
            return View(user);
        }

        // POST: Users/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Email,NumberPhone,PassWord,Permissions")] Users user, string OldPasswordHash)
        {
            if (id != user.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                if (user.PassWord != OldPasswordHash)
                {
                    // Password has changed, so hash the new password
                    user.PassWord = BCrypt.Net.BCrypt.HashPassword(user.PassWord);
                }

                try
                {
                    _context.Update(user);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UsersExists(user.Id))
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

            ViewData["OldRole"] = user.RolesID;
            return View(user);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users.FirstOrDefaultAsync(m => m.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            ViewData["UserId"] = id;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmDelete(int id)
        {
            if (id == 0)
            {
                return NotFound();
            }

            var user = await _context.Users.FirstOrDefaultAsync(m => m.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            user.IsDeleted = true;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Supervisor));
        }




        // POST: Students/Delete/5
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Access(int id)
        {
            if (id == 0)
            {
                return NotFound();
            }

            var user = await _context.Users.FirstOrDefaultAsync(m => m.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            ViewData["UserId"] = id;
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]

        public async Task<IActionResult> ConfirmAccess(int id)
        {
            if (id == 0)
            {
                return NotFound();
            }

            var user = await _context.Users.FirstOrDefaultAsync(m => m.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            user.IsDeleted = false;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Supervisor));
        }

        private bool UsersExists(int id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
    }
}
