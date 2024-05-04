using System;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace CoopProj.Models
{
    public class Students
    {
        [Key]
        public Guid Id { get; set; }

        [Display(Name = "Full Name")]
        [Required(ErrorMessage = "Write your Name!")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Write your Email!")]
        //[RegularExpression(@"^\d{9}@student\.kfu\.edu\.sa$", ErrorMessage = "Invalid email format")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Write your Number!")]
        [Display(Name = "Phone number")]
        //[RegularExpression(@"^\d{10}$", ErrorMessage = "Invalid number format")]

        public int NumberPhone { get; set; }

        [Required(ErrorMessage = "Write your University ID!")]
        [Display(Name = "University ID")]
        [RegularExpression(@"^\d{9}$", ErrorMessage = "Invalid number format")]

        public int UniversityID { get; set; }

        [Required(ErrorMessage = "Write your Password!")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string PassWord { get; set; }

        [Display(Name = "Transcript")]
        public string AccessFile { get; set; }

        public string? ConfirmationToken { get; set; }
        public string? ConfirmationTokenPass { get; set; }

        public bool IsEmailConfirmed { get; internal set; }

        public Majors Major { get; set; }

        [Display(Name = "Major")]
        public Guid MajorId { get; set; }

        public bool? Access { get; set; }
        public bool? IsDeleted { get; set; }

        public string? Image { get; set; }

        public Users Supervisor { get; set; }
        public int? SupervisorID { get; set; }

        public DateTime RegistertionTime { get; set; }


    }
}
