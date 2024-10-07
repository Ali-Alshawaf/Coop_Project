using CoopProj.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using System.Net.Mail;
using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Http;
using System.Text.RegularExpressions;
using BCrypt.Net;
using System.Collections.Generic;

namespace CoopProj.Controllers
{
    public class StudentsController : Controller
    {
        private readonly OurDB _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public StudentsController(OurDB context, IWebHostEnvironment webHostEnvironment, IWebHostEnvironment hostingEnvironment)

        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _hostingEnvironment = hostingEnvironment;
        }
       

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            var WaitStudent = await _context.Students.Include(u => u.Supervisor).Include(u => u.Major).Where(u => u.Access == null).ToListAsync();
            var RejectStudent = await _context.Students.Include(u => u.Supervisor).Include(u => u.Major).Where(u => u.Access == false).ToListAsync();
            var AcceptStudent = await _context.Students.Include(u => u.Supervisor).Include(u => u.Major).Where(u => u.Access == true).ToListAsync();

            ViewData["WaitStudent"] = WaitStudent;
            ViewData["RejectStudent"] = RejectStudent;
            ViewData["AcceptStudent"] = AcceptStudent;
            return View();
        }


        [Authorize(Roles = "Supervisor")]
        public async Task<IActionResult> StudentForSupervisor()
        {
            var supervisorId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            var students = await _context.Students.Include(u => u.Major)
                .Include(u => u.Supervisor)
                .Where(u => u.SupervisorID == supervisorId)
                .ToListAsync();

            return View(students);

        }

        [Authorize(Roles = "Supervisor")]

        public async Task<IActionResult> DetailsS(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var students = await _context.Students.Include(u => u.Supervisor).Include(u => u.Major).FirstOrDefaultAsync(m => m.Id == id);
            if (students == null)
            {
                return NotFound();
            }
            return View(students);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult UpdateSupervisor()
        {
            // Retrieve the list of students without a supervisor from the database
            var studentsWithoutSupervisor = _context.Students.Where(s => s.SupervisorID == null).ToList();

            ViewData["SupervisorID"] = new SelectList(_context.Users.Where(u => u.Roles.Name == "Supervisor"), "Id", "Name");

            // Pass the list of students without a supervisor to the view
            return View(studentsWithoutSupervisor);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> UpdateSupervisor(List<Guid> studentIds, int supervisorId)
        {

            var students = await _context.Students
                .Where(s => studentIds.Contains(s.Id))
                .ToListAsync();

            foreach (var student in students)
            {
                student.SupervisorID = supervisorId;
            }

            ViewData["SupervisorID"] = new SelectList(_context.Users.Where(u => u.Roles.Name == "Supervisor"), "Id", "Name", supervisorId);

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Supervisor updated successfully.";


            return RedirectToAction("UpdateSupervisor");
        }



        // GET: Students/Details/5
        [Authorize(Roles = "Admin")]

        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var students = await _context.Students.Include(u => u.Supervisor).Include(u => u.Major).FirstOrDefaultAsync(m => m.Id == id);
            if (students == null)
            {
                return NotFound();
            }
            try
            {
                // Get the file paths
                string transcriptRootPath = Path.Combine(_hostingEnvironment.WebRootPath, "Transcripts");
                string transcriptPath = Path.Combine(transcriptRootPath, students.AccessFile);
                byte[] transcriptContent = await System.IO.File.ReadAllBytesAsync(transcriptPath);
                string base64Transcript = Convert.ToBase64String(transcriptContent);

                ViewData["AccessFileContent"] = base64Transcript;
            }
            catch
            {
                ViewData["filenotfound"] = "The file is not found";
            }
            return View(students);
        }

        public async Task<IActionResult> Accept(Guid id, [FromBody] string emailContent)
        {
            var students = await _context.Students.FindAsync(id);
            if (students == null)
            {
                return NotFound();
            }

            students.Access = true;
            students.RegistertionTime = DateTime.Now;
            await _context.SaveChangesAsync();

            string studentEmail = _context.Students
                .Where(s => s.Id == students.Id)
                .Select(s => s.Email)
                .FirstOrDefault();
            string name = _context.Students
                .Where(s => s.Id == students.Id)
                .Select(s => s.Name)
                .FirstOrDefault();

            await SendEmail(studentEmail, name, "Your request has been accepted", emailContent);

            return RedirectToAction("Details", new { id });
        }

        [HttpPost]
        public async Task<IActionResult> Reject(Guid id, [FromBody] string emailContent)
        {
            var students = await _context.Students.FindAsync(id);
            if (students == null)
            {
                return NotFound();
            }

            students.Access = false;
            await _context.SaveChangesAsync();

            string studentEmail = _context.Students
                .Where(s => s.Id == students.Id)
                .Select(s => s.Email)
                .FirstOrDefault();
            string name = _context.Students
                .Where(s => s.Id == students.Id)
                .Select(s => s.Name)
                .FirstOrDefault();

            await SendEmail(studentEmail, name, "Your request has been rejected", emailContent);

            return RedirectToAction("Details", new { id });
        }

        private async Task SendEmail(string recipientEmail, string recipientName, string subject, string emailContent)
        {
            // Remove HTML tags from emailContent
            string plainTextEmailContent = RemoveHtmlTags(emailContent);

            // Here is the problem
            // Get the user ID of the admin who is currently logged in
            int adminUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);


            Guid studentUserId = _context.Students
            .Where(s => s.Email == recipientEmail)
            .Select(s => s.Id)
            .FirstOrDefault();

            // Create a new EmailModel object
            var emailModel = new EmailModel
            {
                EmailContent = plainTextEmailContent,
                Subject = subject,
                SenderID = adminUserId,
                EmailStudentID = studentUserId,
            };

            // Add the EmailModel to your database context and save changes
            _context.EmailModel.Add(emailModel);
            await _context.SaveChangesAsync();


            var smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential("##Here Email##", "##Here password##"),
                EnableSsl = true,
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress("##Here Email##"),
                Subject = subject,
                Body = $"Dear student {recipientName},\n\n{plainTextEmailContent}", // Include the student's name
                IsBodyHtml = false, // Set to false to treat the content as plain text
            };

            mailMessage.To.Add(recipientEmail);

            await smtpClient.SendMailAsync(mailMessage);
        }

        private string RemoveHtmlTags(string html)
        {
            // Use regex to remove HTML tags
            return Regex.Replace(html, "<.*?>", String.Empty);
        }

        // GET: Students/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            ViewData["MajorId"] = new SelectList(_context.Majors, "Id", "Name");

            return View();
        }

        // POST: Students/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]

        public async Task<IActionResult> Create([Bind("Id,Name,Email,UniversityID,NumberPhone,PassWord,MajorId")] Students students)
        {
            if (ModelState.IsValid)
            {
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(students.PassWord);
                students.PassWord = hashedPassword;
                students.Id = Guid.NewGuid();
                _context.Add(students);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["MajorId"] = new SelectList(_context.Majors, "Id", "Name", students.MajorId);
            return View(students);
        }

        // GET: Students/Edit/5
        [Authorize(Roles = "Admin")]

        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var students = await _context.Students.FindAsync(id);
            if (students == null)
            {
                return NotFound();
            }

            ViewData["OldPassword"] = students.PassWord;
            ViewData["MajorId"] = new SelectList(_context.Majors, "Id", "Name");
            ViewData["SupervisorID"] = new SelectList(_context.Users.Where(u => u.Roles.Name == "Supervisor"), "Id", "Name");
            return View(students);
        }

        // POST: Students/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,Name,Email,NumberPhone,UniversityID,PassWord,MajorId,AccessFile,SupervisorID")] Students students, IFormFile AccessFile)
        {
            if (id != students.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Retrieve the existing student from the database
                    var existingStudent = await _context.Students.FindAsync(id);
                    if (existingStudent == null)
                    {
                        return NotFound();
                    }

                    // Update the editable fields
                    existingStudent.Name = students.Name;
                    existingStudent.Email = students.Email;
                    existingStudent.PassWord = students.PassWord;
                    existingStudent.UniversityID = students.UniversityID;
                    existingStudent.NumberPhone = students.NumberPhone;
                    existingStudent.MajorId = students.MajorId;
                    existingStudent.SupervisorID = students.SupervisorID;

                    // Update the access file if a new file is provided
                    if (AccessFile != null)
                    {
                        string rootPath = _webHostEnvironment.WebRootPath + "\\Transcripts\\";
                        string fileName = Guid.NewGuid().ToString().Replace("-", "") + Path.GetExtension(AccessFile.FileName);
                        string oldFilePath = rootPath + existingStudent.AccessFile; // Get the path of the existing file

                        // Delete the existing file, if it exists
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }

                        // Save the new file
                        using (FileStream stream = System.IO.File.Create(rootPath + fileName))
                        {
                            AccessFile.CopyTo(stream);
                            stream.Flush();
                        }
                        existingStudent.AccessFile = fileName; // Save the new file name inside the model
                    }

                    _context.Update(existingStudent);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StudentsExists(students.Id))
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

            // If the model state is not valid, return the view with the existing data
            ViewData["OldPassword"] = students.PassWord;
            ViewData["MajorId"] = new SelectList(_context.Majors, "Id", "Name", students.MajorId);
            ViewData["SupervisorID"] = new SelectList(_context.Users.Where(u => u.Roles.Name == "Supervisor"), "Id", "Name", students.SupervisorID);
            return View(students);
        }

        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _context.Students.FirstOrDefaultAsync(m => m.Id == id);
            if (student == null)
            {
                return NotFound();
            }

            ViewData["StudentId"] = id;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmDelete(Guid id)
        {
            if (id == Guid.Empty)
            {
                return NotFound();
            }

            var student = await _context.Students.FirstOrDefaultAsync(m => m.Id == id);
            if (student == null)
            {
                return NotFound();
            }

            student.IsDeleted = true;
            _context.Students.Update(student);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }




        // POST: Students/Delete/5
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Access(Guid id)
        {
            if (id == Guid.Empty)
            {
                return NotFound();
            }

            var student = await _context.Students.FirstOrDefaultAsync(m => m.Id == id);
            if (student == null)
            {
                return NotFound();
            }

            ViewData["StudentId"] = id;
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]

        public async Task<IActionResult> ConfirmAccess(Guid id)
        {
            if (id == Guid.Empty)
            {
                return NotFound();
            }

            var student = await _context.Students.FirstOrDefaultAsync(m => m.Id == id);
            if (student == null)
            {
                return NotFound();
            }

            student.IsDeleted = false;
            _context.Students.Update(student);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }


        [Authorize(Roles = "Admin")]
        private bool StudentsExists(Guid id)
        {
            return _context.Students.Any(e => e.Id == id);
        }
    }
}
