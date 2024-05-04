using System;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace CoopProj.Models
{
    public class Reports
    {
        [Key]
        public Guid Id { get; set; }

        public DateTime Time { get; set; }


        [Display(Name = "Upload report")]
        public string? SendReports { get; set; }

        [Display(Name = "Upload feedback report")]
        public string? FeedbackReports { get; set; }

        public DateTime? Settings { get; set; }

        public float? Grade { get; set; }

        public string Notes { get; set; }

        public string? Description { get; set; }


        public Students StudentReport { get; set; }
        public Guid StudentReportID { get; set; }

        public CreateReport CreateReport { get; set; }
        public Guid CreateReportID { get; set; }



    }
}
