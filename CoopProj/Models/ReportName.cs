using System;
using System.ComponentModel.DataAnnotations;

namespace CoopProj.Models
{
    public class ReportName
    {
        [Key]
        public Guid Id { get; set; }
        public string Name { get; set; }

    }
}
