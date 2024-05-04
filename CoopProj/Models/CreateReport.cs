using System;
using System.ComponentModel.DataAnnotations;

namespace CoopProj.Models
{
    public class CreateReport
    {
        [Key]
        public Guid Id { get; set; }

        public ReportName ReportName { get; set; }
        public Guid ReportNameId { get; set; }

        [Display(Name = "StartDate")]
        public DateTime StartTime { get; set; }

        [Display(Name = "EndDate")]

        public DateTime EndTime { get; set; }

        public Users SupervisorReport { get; set; }
        public int SupervisorReportID { get; set; }




    }
}
