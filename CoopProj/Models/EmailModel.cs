using System;
using System.ComponentModel.DataAnnotations;

namespace CoopProj.Models
{
    public class EmailModel
    {
        [Key]
        public Guid Id { get; set; }
        [DataType(DataType.MultilineText)]
        public string EmailContent { get; set; }
        public string Subject { get; set; }

        public Users Sender { get; set; }
        public int SenderID { get; set; }

        public Students EmailStudent { get; set; }
        public Guid EmailStudentID { get; set; }

    }
}
