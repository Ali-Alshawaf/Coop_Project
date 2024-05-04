using System;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace CoopProj.Models
{
    public class Users
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "Full Name")]
        [Required(ErrorMessage = "Write your Name!")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Write your Email!")]
       
        public string Email { get; set; }

        [Required(ErrorMessage = "Write your Number!")]
        public int NumberPhone { get; set; }

        [Required(ErrorMessage = "Write your Password!")]
        [DataType(DataType.Password)]
        public string PassWord { get; set; }

        public bool? IsDeleted { get; set; }

        public Roles Roles { get; set; }
        public int RolesID { get; set; }
        public string? ResetToken { get; set; }

        public DateTime RegistertionTime { get; set; }

        public Guid? Guid { get; set; }

    }
}
