using System;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace CoopProj.Models
{
    public class Majors
    {
        [Key]
        public Guid Id { get; set; }
        
        [Display(Name="Major")]
        public string Name { get; set; }

    }
}
