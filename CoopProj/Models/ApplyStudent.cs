using System;
using System.ComponentModel.DataAnnotations;

namespace CoopProj.Models
{
    public class ApplyStudent
    {
        [Key]
        public Guid Id { get; set; }
        public Students Students { get; set; }
        public Guid StudentsID { get; set; }
        public Requests Requests { get; set; }
        public Guid RequestsID { get; set; }

        [Display(Name = "Description")]
        public string Information { get; set; }
        public string Status { get; set; }

        [RegularExpression(@"\.pdf$",
    ErrorMessage = "Please select a valid PDF file")]

        public string Letter { get; set; }

        [Display(Name = "Phone number")]
        [RegularExpression(@"\.pdf$",
    ErrorMessage = "Please select a valid PDF file")]
        public string File { get; set; }

        [RegularExpression(@"\.pdf$",
    ErrorMessage = "Please select a valid PDF file")]
        public string Transcript { get; set; }

        public DateTime StartTrining { get; set; }
        public DateTime EndTrining { get; set; }

        [Display(Name = "Rquest time")]

        public DateTime RequestTime { get; set; } // New property for creation time
        public string PDFINFO { get; set; }


        [RegularExpression(@"^(?:[1-4](?:\.[0-9]{2})?|5(?:\.00)?)$",
    ErrorMessage = "The GPA must be between 1 and 5.")]
        public float GPA { get; set; }

    }
}
