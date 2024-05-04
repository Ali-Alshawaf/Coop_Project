using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CoopProj.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using System.Data;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using System.Net.Mail;
using System.Net;
using SelectPdf;
using Microsoft.AspNetCore.Http;
using OpenHtmlToPdf;

namespace CoopProj.Controllers
{
    public class ApplyStudentsCompanyController : Controller
    {
        private readonly OurDB _context;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly IWebHostEnvironment _webHostEnvironment;



        public ApplyStudentsCompanyController(OurDB context, IWebHostEnvironment hostingEnvironment, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _hostingEnvironment = hostingEnvironment;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: ApplyStudentsCompany
        [Authorize(Roles = "Company")]
        public async Task<IActionResult> Index()
        {
            string wait = "Waiting";
            var ourDB = _context.ApplyStudent.Include(a => a.Requests.Companies).Include(a => a.Students).Where(c => c.Requests.CompaniesID == Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value) && c.Status == wait); ;
            return View(await ourDB.ToListAsync());

        }

        [Authorize(Roles = "Company")]

        public async Task<IActionResult> AcceptStudent()
        {
            string accept = "Accepted";
            var ourDB = _context.ApplyStudent.Include(a => a.Requests.Companies).Include(a => a.Students).Where(c => c.Requests.CompaniesID == Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value) && c.Status == accept); ;
            return View(await ourDB.ToListAsync());

        }

        [Authorize(Roles = "Company")]

        public async Task<IActionResult> RejectStudent()
        {
            string reject = "Rejected";
            var ourDB = _context.ApplyStudent.Include(a => a.Requests.Companies).Include(a => a.Students).Where(c => c.Requests.CompaniesID == Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value) && c.Status == reject); ;
            return View(await ourDB.ToListAsync());
        }

        // GET: ApplyStudentsCompany/Details/5
        [Authorize(Roles = "Company")]
        public async Task<IActionResult> Details(Guid? id)
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
        public async Task<IActionResult> Accept(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            ApplyStudent applyStudent = await _context.ApplyStudent.Include(R => R.Requests).ThenInclude(C => C.Companies).SingleOrDefaultAsync(S => S.Id == id);

            if (applyStudent == null)
            {
                return NotFound();
            }

            try
            {
                applyStudent.Status = "Accepted";
                Requests request = _context.Requests.FirstOrDefault(r => r.Id == applyStudent.RequestsID);
                if (request.Quantity == 0) {
                    ViewData["filenotfound"] = "Failed";
                    return View(applyStudent);
                }
                request.Quantity = request.Quantity - 1;
                _context.Update(applyStudent);
                _context.Update(request);
                _context.SaveChanges();
                
            }
            catch
            {
                ViewData["filenotfound"] = "The file is not found";
                return View(applyStudent);
            }
            return RedirectToAction("Details", new { id });

        }







        [HttpPost]
        public async Task<IActionResult> Reject(Guid id)
        {
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

            string company = _context.Requests
    .Where(r => r.Id == applyStudent.RequestsID)
    .Select(r => r.Companies.Name)
    .FirstOrDefault();

            string Opp = _context.Requests
    .Where(r => r.Id == applyStudent.RequestsID)
    .Select(r => r.Application)
    .FirstOrDefault();

            var smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential("good10manx@gmail.com", "vsuepgdfexnjbeih"),
                EnableSsl = true,
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress("good10manx@gmail.com"),
                Subject = "Your request has been rejected",
                Body = $"Dear student {name}\n\nyour request has been rejected in {company} : at {Opp} Application.",
                IsBodyHtml = false,
            };

            mailMessage.To.Add(studentEmail);

            smtpClient.Send(mailMessage);

            return RedirectToAction("Details", new { id });
        }


        // GET: ApplyStudentsCompany/Create
        [Authorize(Roles = "Company")]
        public IActionResult Create()
        {
            ViewData["RequestsID"] = new SelectList(_context.Requests, "Id", "Id");
            ViewData["StudentsID"] = new SelectList(_context.Students, "Id", "Email");
            return View();
        }

        // POST: ApplyStudentsCompany/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Company")]
        public async Task<IActionResult> Create([Bind("Id,StudentsID,RequestsID,RequestTime,Information,Status,Letter,File,Transcript,GPA")] ApplyStudent applyStudent)
        {
            if (ModelState.IsValid)
            {
                applyStudent.Id = Guid.NewGuid();
                _context.Add(applyStudent);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["RequestsID"] = new SelectList(_context.Requests, "Id", "Id", applyStudent.RequestsID);
            ViewData["StudentsID"] = new SelectList(_context.Students, "Id", "Email", applyStudent.StudentsID);
            return View(applyStudent);
        }

        // GET: ApplyStudentsCompany/Edit/5
        [Authorize(Roles = "Company")]
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

        // POST: ApplyStudentsCompany/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Company")]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,StudentsID,RequestsID,Information,Status,Letter,File")] ApplyStudent applyStudent)
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



        private bool ApplyStudentExists(Guid id)
        {
            return _context.ApplyStudent.Any(e => e.Id == id);
        }
    }
}

