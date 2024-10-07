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
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;
using System.Net.Mail;
using System.Net;
using OfficeOpenXml;

namespace CoopProj.Controllers
{
    public class ApplyStudentsAdminController : Controller
    {
        private readonly OurDB _context;
        private readonly IWebHostEnvironment _hostingEnvironment;


        public ApplyStudentsAdminController(OurDB context, IWebHostEnvironment hostingEnvironment)
        {
            _context = context;
            _hostingEnvironment = hostingEnvironment;
        }

        // GET: ApplyStudentsAdmin
        [Authorize(Roles = "Admin")]

        public async Task<IActionResult> Index()
        {
            try
            {
                string wait = "Waiting";
                var ourDB = _context.ApplyStudent.Include(a => a.Requests.Companies).Include(a => a.Students).Where(a => a.Status == wait);
                return View(await ourDB.ToListAsync());
            }
            catch (Exception ex)
            {
                // Handle the exception, log or display an error message
                return View("Error");
            }
        }

       

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AcceptStudent()
        {
            try
            {
                string accept = "Accepted";
                var ourDB = _context.ApplyStudent.Include(a => a.Requests.Companies).Include(a => a.Students).Where(a => a.Status == accept);
                return View(await ourDB.ToListAsync());
            }
            catch (Exception ex)
            {
                // Handle the exception, log or display an error message
                return View("Error");
            }
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RejectStudent()
        {
            try
            {
                string reject = "Rejected";
                var ourDB = _context.ApplyStudent.Include(a => a.Requests.Companies).Include(a => a.Students).Where(a => a.Status == reject);
                return View(await ourDB.ToListAsync());
            }
            catch (Exception ex)
            {
                // Handle the exception, log or display an error message
                return View("Error");
            }
        }

        // GET: ApplyStudentsAdmin/Details/5

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Details(Guid? id)
        {
           
                if (id == null)
            {
                return NotFound();
            }

            var applyStudent = await _context.ApplyStudent
                .Include(a => a.Requests.Companies)
                .Include(a => a.Students)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (applyStudent == null)
            {
                return NotFound();
            }
               
                    try
                    {
                        // Get the file paths
                        string fileRootPath = Path.Combine(_hostingEnvironment.WebRootPath, "CVS");
                        string filePath = Path.Combine(fileRootPath, applyStudent.File);
                        byte[] fileContent = await System.IO.File.ReadAllBytesAsync(filePath);
                        string base64File = Convert.ToBase64String(fileContent);
                        ViewData["FileContent"] = base64File;
                    }
                    catch
                    {
                        ViewData["Filenotfound"] = "The CV does not exist";
                    }




                    try
                    {

                        string letterRootPath = Path.Combine(_hostingEnvironment.WebRootPath, "Letters");
                        string letterPath = Path.Combine(letterRootPath, applyStudent.Letter);
                        byte[] letterContent = await System.IO.File.ReadAllBytesAsync(letterPath);
                        string base64Letter = Convert.ToBase64String(letterContent);
                        ViewData["LetterContent"] = base64Letter;

                    }
                    catch
                    {
                        ViewData["Letternotfound"] = "The Letter does not exist";
                    }




                    try
                    {

                        string transcriptRootPath = Path.Combine(_hostingEnvironment.WebRootPath, "Transcripts");
                        string transcriptPath = Path.Combine(transcriptRootPath, applyStudent.Transcript);
                        byte[] transcriptContent = await System.IO.File.ReadAllBytesAsync(transcriptPath);
                        string base64Transcript = Convert.ToBase64String(transcriptContent);
                        ViewData["TranscriptContent"] = base64Transcript;
                    }
                    catch
                    {
                        ViewData["Transcriptnotfound"] = "The Transcript does not exist";
                    }

               
                return View(applyStudent);
            }
           
        
        [HttpPost]
        public async Task<IActionResult> Accept(Guid id)
        {
            try
            {
                var applyStudent = await _context.ApplyStudent.FindAsync(id);
                if (applyStudent == null)
                {
                    return NotFound();
                }

                applyStudent.Status = "Accepted";
                await _context.SaveChangesAsync();

                // Send email to the student
                string studentEmail = _context.Students
                    .Where(s => s.Id == applyStudent.StudentsID)
                    .Select(s => s.Email)
                    .FirstOrDefault();
                string name = _context.Students
                    .Where(s => s.Id == applyStudent.StudentsID)
                    .Select(s => s.Name)
                    .FirstOrDefault();

                var smtpClient = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential("##Here Email##", "vsuepgdfexnjbeih"),
                    EnableSsl = true,
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress("##Here Email##"),
                    Subject = "Your request has been accepted ",
                    Body = $"Dear student {name}\n\nyour request has been accepted.",
                    IsBodyHtml = false,
                };

                mailMessage.To.Add(studentEmail);

                smtpClient.Send(mailMessage);

                return RedirectToAction("Details", new { id });
            }
            catch (Exception ex)
            {
                // Handle the exception, log or display an error message
                return View("Something is error");
    }
}

        [HttpPost]
        public async Task<IActionResult> Reject(Guid id)
        {
            try { 
            var applyStudent = await _context.ApplyStudent.FindAsync(id);
            if (applyStudent == null)
            {
                return NotFound();
            }

            applyStudent.Status = "Rejected";
            await _context.SaveChangesAsync();

            string studentEmail = _context.Students
                .Where(s => s.Id == applyStudent.StudentsID)
                .Select(s => s.Email)
                .FirstOrDefault();
            string name = _context.Students
                .Where(s => s.Id == applyStudent.StudentsID)
                .Select(s => s.Name)
                .FirstOrDefault();

            var smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential("##Here Email##", "##Here password##"),
                EnableSsl = true,
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress("##Here Email##"),
                Subject = "Your request has been rejected",
                Body = $"Dear student {name}\n\nyour request has been rejected.",
                IsBodyHtml = false,
            };

            mailMessage.To.Add(studentEmail);

            smtpClient.Send(mailMessage);

            return RedirectToAction("Details", new { id });
            }
            catch (Exception ex)
            {
                // Handle the exception, log or display an error message
                return View("Something is error");
            }
        }

        // GET: ApplyStudentsAdmin/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(Guid? id)
        {

            if (id == null)
            {
                return NotFound();
            }

            var applyStudent = await _context.ApplyStudent.FindAsync(id);
            if (applyStudent == null)
            {
                return NotFound();
            }
            ViewData["RequestsID"] = new SelectList(_context.Requests, "Id", "Id", applyStudent.RequestsID);
            ViewData["StudentsID"] = new SelectList(_context.Students, "Id", "Email", applyStudent.StudentsID);
            return View(applyStudent);
        }

        // POST: ApplyStudentsAdmin/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,StudentsID,RequestsID,Information,Status,Letter,File,GPA")] ApplyStudent applyStudent)
        {
            if (id != applyStudent.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(applyStudent);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ApplyStudentExists(applyStudent.Id))
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
            ViewData["RequestsID"] = new SelectList(_context.Requests, "Id", "Id", applyStudent.RequestsID);
            ViewData["StudentsID"] = new SelectList(_context.Students, "Id", "Email", applyStudent.StudentsID);
            return View(applyStudent);
        }

        // GET: ApplyStudentsAdmin/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var applyStudent = await _context.ApplyStudent
                .Include(a => a.Requests)
                .Include(a => a.Students)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (applyStudent == null)
            {
                return NotFound();
            }

            return View(applyStudent);
        }

        // POST: ApplyStudentsAdmin/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var applyStudent = await _context.ApplyStudent.FindAsync(id);
            _context.ApplyStudent.Remove(applyStudent);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ApplyStudentExists(Guid id)
        {
            return _context.ApplyStudent.Any(e => e.Id == id);
        }



    }
}
