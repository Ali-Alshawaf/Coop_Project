using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;


namespace CoopProj.Models
{
    public class Requests
    {
        [Key]
        public Guid Id { get; set; }
        public Companies Companies { get; set; }
        public Guid CompaniesID { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Application { get; set; }
        public string Location { get; set; }
        public string Major { get; set; }
        public int Quantity { get; set; }
        public DateTime RegistertionTime { get; set; }




    }

}
