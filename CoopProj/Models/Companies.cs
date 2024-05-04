using System;
using System.ComponentModel.DataAnnotations;

namespace CoopProj.Models
{
    public class Companies
    {
        [Key]
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string ContactName { get; set; }
        public int ContactNumber { get; set; }
        public string Email { get; set; }
        public string Description { get; set; }
        public string PassWord { get; set; }
        public bool? IsDeleted { get; set; }
        public string? ResetToken { get; set; }
        public string? Image { get; set; }

        public DateTime RegistertionTime { get; set; }

    }
}
