using System.ComponentModel.DataAnnotations;

namespace CoopProj.Models
{
    public class ForgotPasswordViewModel
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
