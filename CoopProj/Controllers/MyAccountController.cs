using CoopProj.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;
using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.IO;
using System.Linq;

namespace CoopProj.Controllers
{
    public class MyAccountController : Controller
    {
        private readonly OurDB _context;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public MyAccountController(OurDB context, IWebHostEnvironment hostingEnvironment)
        {
            _context = context;
            _hostingEnvironment = hostingEnvironment;
        }

        [Authorize(Roles = "Admin,Supervisor")]
        public async Task<IActionResult> AccountUser()
        {
            var id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users.Include(u => u.Roles)
                .FirstOrDefaultAsync(m => m.Id.Equals(id));

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        [Authorize(Roles = "Student")]
        public async Task<IActionResult> AccountStudent()
        {
            var id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (id == null)
            {
                return NotFound();
            }
            var student = await _context.Students
                .Include(u => u.Supervisor)
                .Include(u => u.Major)
                .FirstOrDefaultAsync(m => m.Id == Guid.Parse(id));
            if (student == null)
            {
                return NotFound();
            }
            return View(student);
        }

        [Authorize(Roles = "Company")]
        public async Task<IActionResult> AccountCompany()
        {
            var id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (id == null)
            {
                return NotFound();
            }
            var company = await _context.Companies.FirstOrDefaultAsync(m => m.Id == Guid.Parse(id));

            if (company == null)
            {
                return NotFound();
            }
            return View(company);
        }

        //GET: Students/Edit/5
        [HttpGet]
        public async Task<IActionResult> EditForStudent(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _context.Students.FindAsync(id);
            if (student == null)
            {
                return NotFound();
            }

            ViewData["OldPassword"] = student.PassWord;
            ViewData["MajorId"] = new SelectList(_context.Majors, "Id", "Name", student.MajorId);
            ViewData["SupervisorID"] = new SelectList(_context.Users.Where(u => u.Roles.Name == "Supervisor"), "Id", "Name", student.SupervisorID);
            return View(student);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> EditForStudent(Guid id, [Bind("Id,Name,Email,NumberPhone,UniversityID,PassWord,MajorId,AccessFile,SupervisorID,Image,Access,IsEmailConfirmed,RegistertionTime")] Students student, IFormFile Image)
        {
            if (id != student.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingStudent = await _context.Students.FindAsync(id);
                    if (existingStudent == null)
                    {
                        return NotFound();
                    }

                    if (!string.IsNullOrEmpty(student.PassWord))
                    {
                        existingStudent.PassWord = student.PassWord;
                    }

                    if (Image != null && Image.Length > 0)
                    {
                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + Image.FileName;
                        string imagePath = Path.Combine(_hostingEnvironment.WebRootPath, "images", uniqueFileName);
                        using (var fileStream = new FileStream(imagePath, FileMode.Create))
                        {
                            await Image.CopyToAsync(fileStream);
                        }
                        existingStudent.Image = "/images/" + uniqueFileName;
                    }

                    existingStudent.Name = student.Name;
                    existingStudent.Email = student.Email;
                    existingStudent.NumberPhone = student.NumberPhone;
                    existingStudent.UniversityID = student.UniversityID;
                    existingStudent.MajorId = student.MajorId;
                    existingStudent.SupervisorID = student.SupervisorID;
                    existingStudent.AccessFile = student.AccessFile;
                    existingStudent.Access = student.Access;
                    existingStudent.IsEmailConfirmed = student.IsEmailConfirmed;
                    existingStudent.RegistertionTime = student.RegistertionTime;

                    _context.Update(existingStudent);
                    await _context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    if (!StudentsExists(student.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(AccountStudent));
            }

            ViewData["OldPassword"] = student.PassWord;
            ViewData["MajorId"] = new SelectList(_context.Majors, "Id", "Name", student.MajorId);
            ViewData["SupervisorID"] = new SelectList(_context.Users.Where(u => u.Roles.Name == "Supervisor"), "Id", "Name", student.SupervisorID);
            return View(student);
        }



        public async Task<IActionResult> EditForCompanies(Guid? id)
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
        [Authorize(Roles = "Company")]
        public async Task<IActionResult> EditForCompanies(Guid id, [Bind("Id,Name,ContactName,ContactNumber,Email,Description,PassWord,Image")] Companies companies, IFormFile Image)
        {
            if (id != companies.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingCompany = await _context.Companies.FindAsync(id);
                    if (existingCompany == null)
                    {
                        return NotFound();
                    }

                    // Update the password only if it is provided
                    if (!string.IsNullOrEmpty(companies.PassWord))
                    {
                        existingCompany.PassWord = companies.PassWord;
                    }

                    // Rest of the code remains the same
                    if (Image != null && Image.Length > 0)
                    {
                        // Generate a unique filename for the image
                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + Image.FileName;
                        // Set the path where the image will be saved
                        string imagePath = Path.Combine(_hostingEnvironment.WebRootPath, "images", uniqueFileName);
                        // Save the image file to the specified path
                        using (var fileStream = new FileStream(imagePath, FileMode.Create))
                        {
                            await Image.CopyToAsync(fileStream);
                        }
                        // Save the new file path to the database
                        existingCompany.Image = "/images/" + uniqueFileName;
                    }

                    existingCompany.Email = companies.Email;
                    existingCompany.ContactName = companies.ContactName;
                    existingCompany.ContactNumber = companies.ContactNumber;
                    existingCompany.Name = companies.Name;
                    existingCompany.Description = companies.Description;

                    _context.Update(existingCompany);
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
                return RedirectToAction(nameof(AccountCompany));
            }
            return View(companies);
        }

        private bool CompaniesExists(Guid id)
        {
            return _context.Companies.Any(e => e.Id == id);
        }
        private bool StudentsExists(Guid id)
        {
            return _context.Students.Any(e => e.Id == id);
        }


    }

}


