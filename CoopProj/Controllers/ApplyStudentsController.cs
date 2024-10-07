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
using Microsoft.AspNetCore.Hosting;
using System.Net.Mail;
using System.Net;
using System.Text;
using Microsoft.VisualBasic;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Data;
using System.Linq.Expressions;
using OpenHtmlToPdf;
using iTextSharp.text;
using static System.Net.WebRequestMethods;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Drawing;
using iTextSharp.tool.xml.html;

namespace CoopProj.Controllers
{
    public class ApplyStudentsController : Controller
    {
        private readonly OurDB _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IWebHostEnvironment _hostingEnvironment;


        public ApplyStudentsController(OurDB context, IWebHostEnvironment webHostEnvironment, IWebHostEnvironment hostingEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _hostingEnvironment = hostingEnvironment;
        }

        // GET: ApplyStudents
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> Index()
        {
            var ourDB = _context.ApplyStudent.Include(a => a.Requests.Companies).Include(a => a.Students).Where(c => c.StudentsID == Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value));
            return View(await ourDB.ToListAsync());
        }

        [HttpPost]
        public async Task<IActionResult> FinalAccept(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            ApplyStudent applyStudent = await _context.ApplyStudent
                .Include(R => R.Requests)
                .ThenInclude(C => C.Companies)
                .SingleOrDefaultAsync(S => S.Id == id);

            if (applyStudent == null)
            {
                return NotFound();
            }

            try
            {
                applyStudent.Status = "Final Accepted";
                _context.Update(applyStudent);
                await _context.SaveChangesAsync();
            }
            catch
            {
                ViewData["filenotfound"] = "The file is not found";
                return View(applyStudent);
            }

            return RedirectToAction("Index", new { id });
        }

        // GET: ApplyStudents/Details/5
        [Authorize(Roles = "Student")]
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




            try { 

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

        // GET: ApplyStudents/Create
        [Authorize(Roles = "Student")]
        public IActionResult Create()
        {
            ViewData["RequestsID"] = new SelectList(_context.Requests, "Id", "Id");
            ViewData["StudentsID"] = new SelectList(_context.Students, "Id", "Email");
            return View();
        }

        // POST: ApplyStudents/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> Create([Bind("Information, Letter, File, Transcript, StartTrining, EndTrining, GPA")] ApplyStudent applyStudent, Guid? Id, IFormFile File, IFormFile Letter, IFormFile Transcript)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var existingRequest = _context.ApplyStudent
                 .FirstOrDefault(a => a.RequestsID == Id && a.StudentsID == Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value));

                    if (existingRequest != null)
                    {
                        ModelState.AddModelError("", "You have already made a request for this ID.");
                    }
                    else
                    {
                        applyStudent.Status = "Waiting";
                        applyStudent.RequestsID = Id.Value;
                        applyStudent.RequestTime = DateTime.Now;
                        applyStudent.StudentsID = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                        applyStudent.RequestTime = DateTime.Now;
                        if (File != null)
                        {
                            string root_path = _webHostEnvironment.WebRootPath + "\\CVS\\";
                            string file_name = Guid.NewGuid().ToString().Replace("-", "") + Path.GetExtension(File.FileName);
                            using (FileStream stream = System.IO.File.Create(root_path + file_name))
                            {
                                File.CopyTo(stream);
                                stream.Flush();
                            }
                            applyStudent.File = file_name; // Save Image name Inside model
                        }
                        if (Letter != null)
                        {
                            string root_path = _webHostEnvironment.WebRootPath + "\\Letters\\";
                            string file_name = Guid.NewGuid().ToString().Replace("-", "") + Path.GetExtension(Letter.FileName);
                            using (FileStream stream = System.IO.File.Create(root_path + file_name))
                            {
                                Letter.CopyTo(stream);
                                stream.Flush();
                            }
                            applyStudent.Letter = file_name; // Save Image name Inside model
                        }
                        if (Transcript != null)
                        {
                            string root_path = _webHostEnvironment.WebRootPath + "\\Transcripts\\";
                            string file_name = Guid.NewGuid().ToString().Replace("-", "") + Path.GetExtension(Transcript.FileName);
                            using (FileStream stream = System.IO.File.Create(root_path + file_name))
                            {
                                Transcript.CopyTo(stream);
                                stream.Flush();
                            }
                            applyStudent.Transcript = file_name; // Save Image name Inside model
                        }
                        applyStudent.Id = Guid.NewGuid();

                        _context.Add(applyStudent);
                        await _context.SaveChangesAsync();

                        var studentEmail = _context.Students
                            .Where(s => s.Id == applyStudent.StudentsID)
                            .Select(s => s.Email)
                            .FirstOrDefault();

                        var time = DateTime.Now;

                        var nameCompany = _context.Requests
                            .Where(s => s.Id == applyStudent.StudentsID)
                            .Select(s => s.Companies.Name)
                            .FirstOrDefault();

                        // Send thank you email to student
                        var smtpClient = new SmtpClient("smtp.gmail.com")
                        {
                            Port = 587,
                            Credentials = new NetworkCredential("##Here Email##", "##Here password##"),
                            EnableSsl = true,
                        };

                        var mailMessage = new MailMessage
                        {
                            From = new MailAddress("##Here Email##"),
                            Subject = "Thank you for your request",
                            Body = $"Dear Student,\n\nWe have received your request and we will process it as soon as possible.\n\nThank you for choosing our service {nameCompany}.\n\nSincerely,\nYour Service Provider\n\n {time} ",
                            IsBodyHtml = false,
                        };

                        mailMessage.To.Add(studentEmail);

                        smtpClient.Send(mailMessage);

                        return RedirectToAction(nameof(Index));
                    }
                }
            }
            catch
            {
                return NotFound();
            }

            ViewData["RequestsID"] = new SelectList(_context.Requests, "Id", "Id", applyStudent.RequestsID);
            ViewData["StudentsID"] = new SelectList(_context.Students, "Id", "Email", applyStudent.StudentsID);
            return View(applyStudent);
        }
        // GET: ApplyStudents/Edit/5
        [Authorize(Roles = "Student")]
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

        // POST: ApplyStudents/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,StudentsID,RequestsID,RequestTime,Information,Status,Letter,File,Transcript,StartTrining,EndTrining,GPA")] ApplyStudent applyStudent)
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
        [HttpGet]
        public async Task<IActionResult> Print(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            ApplyStudent applyStudent = await _context.ApplyStudent.Include(S => S.Students).ThenInclude(M => M.Major).Include(R => R.Requests).ThenInclude(C => C.Companies).SingleOrDefaultAsync(S => S.Id == id);

            if (applyStudent == null)
            {
                return NotFound();
            }

            try
            {
                TimeSpan duration = applyStudent.EndTrining - applyStudent.StartTrining;
                int numberOfWeeks = (int)(duration.TotalDays / 7);


                byte[] IconArray = System.IO.File.ReadAllBytes(_hostingEnvironment.WebRootPath + @"/assets/images/R.png");
                string Icon = Convert.ToBase64String(IconArray);
                Icon = "data:image/png;base64," + Icon;

                byte[] IconArray1 = System.IO.File.ReadAllBytes(_hostingEnvironment.WebRootPath + @"/assets/images/IMGKFU.png");
                string Icon1 = Convert.ToBase64String(IconArray1);
                Icon1 = "data:image/png;base64," + Icon1;


                // Perform the PDF conversion
                string htmlContent = "<!DOCTYPE html>" +
                "<html>" +
                    "<head>" +
                    "<meta charset='UTF-8'>" +
                    "<meta name='viewport' content='width=device-width'>" +
                    "<meta name='viewport' content='width=device-width, initial-scale=1.0'>" +
                    "<meta name='viewport' content='width=device-width, initial-scale=1'>" +
                    "<link rel='stylesheet' href='https://maxcdn.bootstrapcdn.com/bootstrap/3.4.1/css/bootstrap.min.css'>" +
                    "<style>" +
                    "body{" +
                    "font-family:Arial,sans-serif;" +
                    "padding:70px;" +
                    "}" +
                    ".header{" +
                    "display:flex;" +
                    "justify-content:space-between;" +
                    "align-items:enter;" +
                    "padding:20px;" +
                    "display:inline;" +
                    "}" +
                    "hr{" +
                    "border-top:2px solid black;" +
                    "margin:10px 0;" +
                    "}" +
                    "table{" +
                    "width:100%;" +
                    "border-collapse:collapse;" +
                    "}" +
                    "th,td{" +
                    "padding:10px;" +
                    "text-align:left;" +
                    "border:1px solid #ddd;" +
                    "height:3px;" +
                    "}" +
                    "th{" +
                    "text-align:center;" +
                    "background-color:#f2f2f2;" +
                    "width:200px;" +
                    "}" +
                    "th:first-child," +
                    "td:first-child{" +
                    "text-align:center;" +
                    "}" +
                    ".Footer{" +
                    "left: 0;" +
                    "bottom: 0;" +
                    "width:100%;" +
                    "margin-top:150px;" +
                    "padding-top:50px;" +
                    "text-align:center;" +
                    "}" +
                    ".centere{" +
                    "text-align:center;" +
                    "}" +
                    ".par{" +
                    "text-align:right;" +
                    "line-height:1.5;" +
                    "}" +
                    ".thank{" +
                    "display:flex;" +
                    "justify-content:center;" +
                    "align-items:center;" +
                    "margin-left:35%;" +
                    "margin-top:150px;" +
                    "}" +
                    ".image img{" +
                    "max-width:60%;" +
                    "height:auto;" +
                    "}" +
                    ".university-name," +
                    ".mobile," +
                    ".coop," +
                    ".KSA," +
                    ".image1," +
                    ".image{" +
                    "display:inline-block;" +
                    "vertical-align:top;" +
                    "width:33%;" +
                    "}" +
                    ".KFU{" +
                    "display:flex;" +
                    "justify-content:space-between;" +
                    "margin-top:150px" +
                    "}" +
                    ".KFU1{" +
                    "display:flex;" +
                    "justify-content:space-between;" +
                    "}" +
                    "</style>" +
                    "</head>" +
                    "<body style='font-size:3.5em;'>" +
                    "<div class='main'>" +
                    "<div class='KFU1'>" +
                    "<div class='KSA'>" +
                    "<div class='centere'>" +
                    "<h1>KINGDOM OF SAUDI ARABIA</h1>" +
                    "<em>" +
                    "<h1>Ministri of Education</h1>" +
                    "<h1>KING FAISAL UNIVERSITY</h1>" +
                    "<h1>(037)</h1>" +
                    "</em>" +
                    "</div>" +
                    "</div>" +
                    "<div class='image'>" +
                    "<div class='centere'>" +
                    "<img src=" + Icon + @" />" +
                    "</div>" +
                    "</div>" +
                    "<div class='university-name'>" +
                    "<div class='centere'>" +
                    "<h1>المملكة العربية السعودية</h1>" +
                    "<div class='centere'>" +
                    "<h1>وزارة التعليم</h1>" +
                    "<h1>جامعة الملك فيصل</h1>" +
                    "<h1>(037)</h1>" +
                    "</div>" +
                    "</div>" +
                    "</div>" +
                    "<hr/>" +
                    "<br/>" +
                    "<div class='centere'>" +
                    "<h1><strong>(خطاب توجيه لتدريب طالب جامعي)</strong></h1>" +
                    "</div>" +
                    "<br/>" +
                    "<table>" +
                    "<tr>" +
                    "<td>" + applyStudent.Students.Name + "</td>" +
                    "<th>:الأسم</th>" +
                    "</tr>" +
                    "<tr>" +
                    "<td>" + applyStudent.Students.UniversityID + "</td>" +
                    "<th>:الرقم الأكاديمي</th>" +
                    "</tr>" +
                    "<tr>" +
                    "<td>" + applyStudent.Students.Major.Name + "</td>" +
                    "<th>:التخصص</th>" +
                    "</tr>" +
                    "<tr>" +
                    "<td>(" + numberOfWeeks.ToString() + ")أسابيع</td>" +
                    "<th>:فترة التدريب</th>" +
                    "</tr>" +
                    "</table>" +
                    "<div class='par'>" +
                    "<br/>" +
                    "<br/>" +
                    "<h1><strong>" + applyStudent.Requests.Companies.Name + " : سعادة / شركة</strong></h1>" +
                    "<h1><strong>: السلام عليكم ورحمة الله وبركاته, تحيه طيبه</strong></h1>" +
                    "<h1>تتقدم كلية علوم الحاسب وتقنية المعلومات بجامعة الملك فيصل لكم بخالص الشكر والتقدير على موافقتكم الكريمة على</h1>" +
                    "<h1>.منح الفرصة التدريبية للطالب المذكور/ة أعلاه</h1>" +
                    "<h1>.عليه، نفيدكم بأن الكلية ترغب في تدريب الطالب المذكور/ة بياناته/ا أعلاه لدى جهتكم الموقرة</h1>" +
                    "</div>" +
                    "<div class='thank'>" +
                    "<h1>,,,هذا وتقبلوا خالص التحية والتقدير</h1>" +
                    "</div>" +
                    "<div class='KFU'>" +
                    "<div class='mobile'>" +
                    "<div class='centere'>" +
                    "<h2>هاتف: 0135898114</h2>" +
                    "<h2>فاكس: 0135899237</h2>" +
                    "<h2>البريد الألكتروني: coop.ccsit@kfu.edu.sa</h2>" +
                    "</div>" +
                    "</div>" +
                    "<div class='coop'>" +
                    "<div class='centere'>" +
                    "<h2>لجنة التدريب التعاوني</h2>" +
                    "<h2>بكلية علوم الحاسب وتقنية المعلومات</h2>" +
                    "</div>" +
                    "</div>" +
                    "<div class='image'>" +
                    "<div class='centere'>" +
                    "<h2><strong>: الختم</strong></h2>" +
                    "<img src='"+Icon1+@"'/>" +
                    "</div>" +
                    "</div>" +
                    "</div>" +
                    "<div class='Footer'>" +
                    "<hr>" +
                    "<div class='centere'>" +
                    "<h1>الأحساء - المملكة العربية السعودية</h1>" +
                    "<h1>____________________________WWW.kfu.edu.sa____________________________</h1>" +
                    "</div>" +
                    "</div>" +
                    "</div>" +
                    "</div>" +
                    "</body>" +
                    "</html>";
            

                string NameOFFile = applyStudent.Id.ToString();
                return File(Pdf.From(htmlContent).OfSize(PaperSize.A4).WithMargins(0.Centimeters()).Portrait().Comressed().Content(), System.Net.Mime.MediaTypeNames.Application.Pdf, "" + NameOFFile + ".pdf");

            }
            catch(Exception ex)
            {
                ViewData["filenotfound"] = "The file is not found";
                return View(applyStudent);
            }

        }
        //private byte[] ConvertHtmlToPdf(string htmlContent)
        //{
        //    // Use your preferred method/library to convert HTML to PDF
        //    // Here's an example using the SelectPdf library
        //    var converter = new SelectPdf.HtmlToPdf();
        //    var doc = converter.ConvertHtmlString(htmlContent);
        //    var pdfBytes = doc.Save();
        //    doc.Close();

        //    return pdfBytes;
        //}

        
    

    // GET: ApplyStudents/Delete/5
    [Authorize(Roles = "Student")]
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

        // POST: ApplyStudents/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Student")]
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
