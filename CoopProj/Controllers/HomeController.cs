using CoopProj.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Net;
using Microsoft.EntityFrameworkCore;
using System.Net.NetworkInformation;
using Microsoft.CodeAnalysis;
using System.Net.Mail;
using System.Xml.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.AspNetCore.Mvc.Rendering;
using BCrypt.Net;


namespace CoopProj.Controllers
{
    public class HomeController : Controller
    {
        private readonly OurDB _context;
        private readonly ILogger<HomeController> _logger; 
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public HomeController(ILogger<HomeController> logger, OurDB context, IWebHostEnvironment webHostEnvironment, IWebHostEnvironment hostingEnvironment)
        {
            _logger = logger;
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _hostingEnvironment = hostingEnvironment;
        }



        // GET: Users/Create
        [AllowAnonymous]
        public IActionResult Signup()
        {
            ViewData["MajorId"] = new SelectList(_context.Majors, "Id", "Name");
            return View();
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Signup([Bind("Id,Name,Email,UniversityID,NumberPhone,PassWord,AccessFile,IsDeleted,Access,MajorId")] Students students, IFormFile AccessFile)
        {
            ViewData["MajorId"] = new SelectList(_context.Majors, "Id", "Name", students.MajorId);

            if (_context.Students.Any(s => s.Email == students.Email) || _context.Users.Any(u => u.Email == students.Email) || _context.Companies.Any(u => u.Email == students.Email))
            {
                ModelState.AddModelError("Email", "This email already exists!");
                return View(students);
            }
            if (ModelState.IsValid)
            {
                if (AccessFile != null)
                {
                    string root_path = _webHostEnvironment.WebRootPath + "\\Transcripts\\";
                    string file_name = Guid.NewGuid().ToString().Replace("-", "") + Path.GetExtension(AccessFile.FileName);
                    using (FileStream stream = System.IO.File.Create(root_path + file_name))
                    {
                        AccessFile.CopyTo(stream);
                        stream.Flush();
                    }
                    students.AccessFile = file_name; // Save Image name Inside model
                }
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(students.PassWord);
                students.PassWord = hashedPassword;
                students.Id = Guid.NewGuid();
                _context.Add(students);
                await _context.SaveChangesAsync();

                // Generate a confirmation token
                var token = GenerateConfirmationToken(students.Email);

                // Create the confirmation link
                var confirmationLink = Url.Action("ConfirmEmail", "Home", new { email = students.Email, token }, Request.Scheme);

                var name = students.Name;

                // Send confirmation email to the student
                await SendConfirmationEmail(students.Email, confirmationLink, name);
                ViewBag.ConfirmEmailMessage = "confirm your email!";
                return View(nameof(Login));
            }
            return View(students);
        }

        private string GenerateConfirmationToken(string email)
        {
            var token = Guid.NewGuid().ToString();

            // Save the token in the database for later verification
            var student = _context.Students.FirstOrDefault(s => s.Email == email);
            if (student != null)
            {
                student.ConfirmationToken = token;
                _context.SaveChanges();
            }

            return token;
        }

        private async Task SendConfirmationEmail(string email, string confirmationLink, string name)
        {
            // Send the confirmation email to the student
            var smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential("##Here Email##", "##Here password##"),
                EnableSsl = true,
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress("##Here Email##"),
                Subject = "Confirm Your Email",
                Body = $"Dear Student {name},\n\nPlease confirm your email address by clicking the link below:\n\n{confirmationLink}\n\nThank you for signing up.\n\nSincerely,\nYour Service Provider",
                IsBodyHtml = false,
            };

            mailMessage.To.Add(email);

            await smtpClient.SendMailAsync(mailMessage);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(string email, string token)
        {
            if (email == null || token == null)
            {
                // Invalid confirmation link
                return RedirectToAction(nameof(Index));
            }

            var student = await _context.Students.FirstOrDefaultAsync(s => s.Email == email && s.ConfirmationToken == token);

            if (student == null)
            {
                // Invalid email or token
                return RedirectToAction(nameof(Index));
            }

            // Pass the email and token to the view
            ViewBag.Email = email;
            ViewBag.Token = token;

            // Return the view with the confirmation button
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmailAction(string email, string token)
        {
            var student = await _context.Students.FirstOrDefaultAsync(s => s.Email == email && s.ConfirmationToken == token);

            if (student == null)
            {
                // Invalid email or token
                return RedirectToAction(nameof(Index));
            }

            // Mark the email as confirmed
            student.IsEmailConfirmed = true;
            student.ConfirmationToken = null;
            await _context.SaveChangesAsync();
            ViewBag.Confirm = "Your email address has been successfully confirmed. Thank you!";
            // Perform additional actions if necessary, e.g., sign the user in
            // ...
            ViewBag.success = "your email has been successfully confirmed";
            return View(nameof(Login));
        }



        [AllowAnonymous]
        public IActionResult SignupC()
        {
            return View();
        }

        // POST: Companies/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignupC([Bind("Id,Name,ContactName,ContactNumber,Email,Description,PassWord")] Companies companies)
        {
            if (_context.Students.Any(s => s.Email == companies.Email) || _context.Users.Any(u => u.Email == companies.Email) || _context.Companies.Any(u => u.Email == companies.Email))
            {
                ModelState.AddModelError("Email", "This email already exists!");
                return View(companies);
            }
            if (ModelState.IsValid)
            {
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(companies.PassWord);
                companies.RegistertionTime = DateTime.Now;
                companies.PassWord = hashedPassword;
                companies.Id = Guid.NewGuid();
                _context.Add(companies);
                await _context.SaveChangesAsync();
                ViewBag.ConfirmEmailMessage = "confirm your email!";

                return View(nameof(Login));
            }
            return View(companies);
        }



        #region Login/Logout
        [HttpGet]
        public IActionResult Login()
        {
            try
            {
                if (User.Identity.IsAuthenticated)
                {
                    string UserName = User.FindFirst(ClaimTypes.Name).Value;

                    // Assuming you have a role claim for the user
                    string UserRole = User.FindFirst(ClaimTypes.Role).Value;

                    if (UserRole == "Student")
                    {
                        return RedirectToAction("StudentHome", "Home");
                    }
                    else if (UserRole == "Company")
                    {
                        return RedirectToAction("CompanyHome", "Home");
                    }
                    else if (UserRole == "Supervisor")
                    {
                        return RedirectToAction("SupervisorHome", "Home");
                    }
                    else if (UserRole == "Admin")
                    {
                        return RedirectToAction("AdminHome", "Home");
                    }
                }

                return View();
                }
            catch
            {
                return View();
            }

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string Email, string Password)
        {
            try
            {
                Users check = _context.Users.Include(u => u.Roles).Where(u => u.Email == Email).SingleOrDefault();
                Students checkS = _context.Students.Where(u => u.Email == Email).SingleOrDefault();
                Companies checkC = _context.Companies.Where(u => u.Email == Email).SingleOrDefault();

                if (check != null && BCrypt.Net.BCrypt.Verify(Password, check.PassWord))
                {
                  
                    var identity = new ClaimsIdentity(new[]
                    {
                    new Claim(ClaimTypes.Name, check.Email),
                    new Claim(ClaimTypes.Role,check.Roles.Name),
                    new Claim(ClaimTypes.NameIdentifier, check.Id.ToString()),
                    new Claim(ClaimTypes.GivenName, check.Name)

                }, CookieAuthenticationDefaults.AuthenticationScheme);

                    var principal = new ClaimsPrincipal(identity);

                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        principal);

                    if (check.Roles.Name == "Admin")
                    {
                        return RedirectToAction("AdminHome", "Home");
                    }
                    else if (check.Roles.Name == "Supervisor")
                    {
                        return RedirectToAction("SupervisorHome", "Home");
                    }
                    else 
                    {
                        return RedirectToAction("Index", "Home");
                    }
                }
                else if (checkS != null && BCrypt.Net.BCrypt.Verify(Password, checkS.PassWord) && checkS.IsDeleted != true )
                {
                    if (checkS.IsEmailConfirmed == false)
                    {
                        ViewBag.ConfirmEmailMessage = "confirm your email!";
                        return View();
                    }
                    else if (checkS.Access == null)
                    {
                        ViewBag.NameMassage = checkS.Name;
                        ViewBag.ProMessage = "Your process is being processed";

                        return View();
                    }
                    else if (checkS.IsDeleted == true)
                    {
                        ViewBag.NameMassage = checkS.Name;
                        ViewBag.DeleteMessage = "Your account is deleted";

                        return View();
                    }
                    else if (checkS.Access == false)
                    {
                        ViewBag.ErrorMessage = "The status of the request is rejected"; 

                        return View();
                    }

                    var identity = new ClaimsIdentity(new[]
                    {
                    new Claim(ClaimTypes.Name, checkS.Email),
                    new Claim(ClaimTypes.Role, "Student"),
                    new Claim(ClaimTypes.NameIdentifier, checkS.Id.ToString()),
                    new Claim(ClaimTypes.GivenName, checkS.Name)

                }, CookieAuthenticationDefaults.AuthenticationScheme);

                    var principal = new ClaimsPrincipal(identity);

                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        principal);

                    return RedirectToAction("StudentHome", "Home");

                }
                else if (checkC != null && BCrypt.Net.BCrypt.Verify(Password, checkC.PassWord))
                {
                     if (checkC.IsDeleted == true)
                    {
                        ViewBag.NameMassage = checkC.Name;
                        ViewBag.DeleteMessage = "Your account is deleted";
                        return View();
                    }
                    var identity = new ClaimsIdentity(new[]
                    {
                    new Claim(ClaimTypes.Name, checkC.Email),
                    new Claim(ClaimTypes.Role, "Company"),
                    new Claim(ClaimTypes.NameIdentifier, checkC.Id.ToString()),
                    new Claim(ClaimTypes.GivenName, checkC.Name),

                }, CookieAuthenticationDefaults.AuthenticationScheme);

                    var principal = new ClaimsPrincipal(identity);

                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        principal);

                    return RedirectToAction("CompanyHome", "Home");

                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Error: Incorrect username or password!");
                    return View();
                }
            }
            catch
            {
                ModelState.AddModelError(string.Empty, "Error: Incorrect username or password!");
                return View();
            }

        }


        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(Login));
        }


        #endregion Login/Logout


        public IActionResult Index()
        {
            
            return View();
        }


      

        [Authorize(Roles ="Admin")]
        public IActionResult AdminHome()
        {
            string Accept = "Accepted";
            string reject = "rejected";
            string wait = "waiting";

            int student = _context.Students.Count();
            ViewBag.Student = student;

            int company = _context.Companies.Count();
            ViewBag.Company = company;

            int User = _context.Users.Count();
            ViewBag.User = User;

            int request = _context.Requests.Count();
            ViewBag.Request = request;

            int apply = _context.ApplyStudent.Count();
            ViewBag.Apply = apply;

            int statusAccept = _context.ApplyStudent.Include(u => u.Requests.Companies).Include(r => r.Students).Where(r => r.Status == Accept).Count();
            ViewBag.StatusAccept = statusAccept;

            int statusReject = _context.ApplyStudent.Include(u => u.Requests.Companies).Include(r => r.Students).Where(r => r.Status == reject).Count();
            ViewBag.StatusReject = statusReject;

            int statuswaiting = _context.ApplyStudent.Include(u => u.Requests.Companies).Include(r => r.Students).Where(r =>r.Status == wait).Count();
            ViewBag.Statuswaiting = statuswaiting;


            return View();
        }

        [Authorize(Roles = "Company")]
        public IActionResult CompanyHome()
        {
            Guid companyId = Guid.Parse(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value);

            string Accept = "Accepted";
            string reject = "rejected";
            string wait = "waiting";

            int opportinity = _context.Requests.Include(r => r.Companies).Where(r => r.CompaniesID == companyId).Count();
            ViewBag.Opportinity = opportinity;

            int studentRequest = _context.ApplyStudent.Include(u => u.Requests.Companies).Include(r => r.Students).Where(r => r.Requests.CompaniesID == companyId).Count();
            ViewBag.StudentRequest = studentRequest;

            int studentAccept = _context.ApplyStudent.Include(u => u.Requests.Companies).Include(r => r.Students).Where(r => r.Requests.CompaniesID == companyId && r.Status == Accept).Count();
            ViewBag.StudentAccept = studentAccept;

            int studentReject = _context.ApplyStudent.Include(u => u.Requests.Companies).Include(r => r.Students).Where(r => r.Requests.CompaniesID == companyId && r.Status == reject).Count();
            ViewBag.StudentReject = studentReject;

            int studentwaiting = _context.ApplyStudent.Include(u => u.Requests.Companies).Include(r => r.Students).Where(r => r.Requests.CompaniesID == companyId && r.Status == wait).Count();
            ViewBag.Studentwaiting = studentwaiting;

            return View();
        }
        [Authorize(Roles = "Supervisor")]

        public IActionResult SupervisorHome()
        {
            int Users = int.Parse(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value);

            var students = _context.Students.Include(u => u.Major).Include(u => u.Supervisor).Where(u => u.SupervisorID == Users).Count();
            ViewBag.StudentsForSupervisor = students;

            var report = _context.Reports.Include(u => u.CreateReport).Include(u => u.StudentReport).Where(u => u.StudentReport.SupervisorID == Users).Count();
            ViewBag.StudentsReports = report;

            return View();
        }



        [Authorize(Roles = "Student")]

        public IActionResult StudentHome()
        {
            Guid Users = Guid.Parse(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value);
            string Accept = "Accepted";
            string reject = "rejected";
            string wait = "waiting";

            int numStudentRequest = _context.ApplyStudent.Include(u => u.Requests.Companies).Include(r => r.Students).Where(r => r.StudentsID == Users).Count();
            ViewBag.NumberRequest = numStudentRequest;

            int statusAccept = _context.ApplyStudent.Include(u => u.Requests.Companies).Include(r => r.Students).Where(r => r.StudentsID == Users && r.Status == Accept ).Count();
            ViewBag.StatusAccept = statusAccept;

            int statusReject = _context.ApplyStudent.Include(u => u.Requests.Companies).Include(r => r.Students).Where(r => r.StudentsID == Users && r.Status == reject).Count();
            ViewBag.StatusReject = statusReject;

            int statuswaiting = _context.ApplyStudent.Include(u => u.Requests.Companies).Include(r => r.Students).Where(r => r.StudentsID == Users && r.Status == wait).Count();
            ViewBag.Statuswaiting = statuswaiting;

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }






        // Forgot password View
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return View();
        }
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            // Check if the email exists in the database
            var student = await _context.Students.FirstOrDefaultAsync(s => s.Email == model.Email);
            var company = await _context.Companies.FirstOrDefaultAsync(u => u.Email == model.Email);
            var users = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);

            if (student == null && company == null && users == null)
            {
                // Email doesn't exist in the database, show an error message
                ModelState.AddModelError(string.Empty, "Email not found.");
                return View(model);
            }

            // Generate a password reset token
            var token = Guid.NewGuid().ToString();

            if (student != null)
            {
                if (student.IsEmailConfirmed == true) {
                    student.ConfirmationTokenPass = token;
                    await _context.SaveChangesAsync();

                    // Create the password reset link
                    var resetLink = Url.Action("ResetPassword", "Home", new { email = model.Email, token }, Request.Scheme);

                    // Send the password reset email
                    await SendPasswordResetEmail(model.Email, resetLink, student.Name);
                    ViewData["CheckEmail"] = "Please Check your Eamil Inbox";
                    return View("ForgotPassword");
                }
                else
                {
                    ViewBag.confirmToReset = "Confirm your email before reset your password";
                    return View("Login");
                }
            }
            else if (company != null)
            {
                company.ResetToken = token;
                await _context.SaveChangesAsync();

                // Create the password reset link
                var resetLink = Url.Action("ResetPassword", "Home", new { email = model.Email, token }, Request.Scheme);

                // Send the password reset email
                await SendPasswordResetEmail(model.Email, resetLink, company.Name);
                ViewData["CheckEmail"] = "Please Check your Eamil Inbox";
                return View("ForgotPassword");
            }
            else if (users != null)
            {
                users.ResetToken = token;
                await _context.SaveChangesAsync();

                // Create the password reset link
                var resetLink = Url.Action("ResetPassword", "Home", new { email = model.Email, token }, Request.Scheme);

                // Send the password reset email
                await SendPasswordResetEmail(model.Email, resetLink, users.Name);
                ViewData["CheckEmail"] = "Please Check your Eamil Inbox";
                return View("ForgotPassword");
            }

            return View("Login");
        }


        private async Task SendPasswordResetEmail(string email, string resetLink, string name)
        {
            // Use the SMTP settings defined within your HomeController
            string smtpServer = "smtp.gmail.com";
            int smtpPort = 587;
            string smtpUsername = "##Here Email##";
            string smtpPassword = "vsuepgdfexnjbeih";
            string senderEmail = "##Here Email##";

            try
            {
                using (var smtpClient = new SmtpClient(smtpServer))
                {
                    smtpClient.Port = smtpPort;
                    smtpClient.Credentials = new NetworkCredential(smtpUsername, smtpPassword);
                    smtpClient.EnableSsl = true;

                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress(senderEmail), // Use the sender email defined in the controller
                        Subject = "Password Reset",
                        Body = $"Dear {name},\n\nYou have requested a password reset. Click the link below to reset your password:\n\n{resetLink}\n\nIf you didn't request this reset, please ignore this email.\n\nSincerely,\nYour Service Provider",
                        IsBodyHtml = false,
                    };
                    mailMessage.To.Add(email);

                    await smtpClient.SendMailAsync(mailMessage);
                }
            }
            catch (Exception ex)
            {
                // Handle email sending errors (log, display error message, etc.)
                // You can add your error handling logic here
                _logger.LogError($"Error sending password reset email: {ex.Message}");
                throw;
            }
        }



        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword(string email, string token)
        {
            var model = new ResetPasswordViewModel
            {
                Email = email,
                Token = token
            };

            return View(model);

        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Check if the token and email are valid for students or companies
                var student = await _context.Students.FirstOrDefaultAsync(s => s.Email == model.Email);
                var company = await _context.Companies.FirstOrDefaultAsync(c => c.Email == model.Email);
                var users = await _context.Users.FirstOrDefaultAsync(c => c.Email == model.Email);

                if (student != null && IsValidToken(student, model.Token))
                {
                    string hashedPassword = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
                    await UpdatePasswordAndClearToken(student, model.NewPassword = hashedPassword);
                    ViewData["NewPassword"] = "Password has been reset successfully";
                    return View("Login");
                }
                else if (company != null && IsValidToken(company, model.Token))
                {
                    string hashedPassword = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
                    await UpdatePasswordAndClearToken(company, model.NewPassword = hashedPassword);
                    ViewData["NewPassword"] = "Password has been reset successfully";
                    return View("Login");
                }
                else if (users != null && IsValidToken(users, model.Token))
                {
                    string hashedPassword = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
                    await UpdatePasswordAndClearToken(users, model.NewPassword = hashedPassword);
                    ViewData["NewPassword"] = "Password has been reset successfully";
                    return View("Login");
                }
                else
                {
                    // Token or email is not valid, handle the error
                    ModelState.AddModelError(string.Empty, "Password reset failed. Invalid token or email.");
                }
            }

            // If there are any other errors, return the view with the model to display error messages
            return View(model);
        }

        private bool IsValidToken(Students student, string token)
        {
            // Implement your token validation logic for students
            // Return true if the token is valid, otherwise return false
            return student.ConfirmationTokenPass == token;
        }

        private bool IsValidToken(Companies company, string token)
        {
            // Implement your token validation logic for companies
            // Return true if the token is valid, otherwise return false
            return company.ResetToken == token;
        }

        private bool IsValidToken(Users users, string token)
        {
            // Implement your token validation logic for companies
            // Return true if the token is valid, otherwise return false
            return users.ResetToken == token;
        }

        private async Task UpdatePasswordAndClearToken(Students student, string newPassword)
        {
            // Update the student's password and clear the token
            student.PassWord = newPassword; // Update the password
            student.ConfirmationTokenPass = null; // Clear the token
            await _context.SaveChangesAsync();
        }

        private async Task UpdatePasswordAndClearToken(Companies company, string newPassword)
        {
            // Update the company's password and clear the token
            company.PassWord = newPassword; // Update the password
            company.ResetToken = null; // Clear the token
            await _context.SaveChangesAsync();
        }
        private async Task UpdatePasswordAndClearToken(Users users, string newPassword)
        {
            // Update the company's password and clear the token
            users.PassWord = newPassword; // Update the password
            users.ResetToken = null; // Clear the token
            await _context.SaveChangesAsync();
        }

    }
}
