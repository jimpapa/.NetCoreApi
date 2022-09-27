using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1.Models
{
    public class Patient
    {
        public int Id { get; set; }
        public string Lastname { get; set; }
        public string Firstname { get; set; }
        public string Husband_Lastname { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string Area { get; set; }
        public string Zip { get; set; }
        public string Home_Phone { get; set; }
        public string Work_Phone { get; set; }
        public string Cell_Phone { get; set; }
        public string Email1 { get; set; }
        public string Provident { get; set; }
        public string Fathername { get; set; }
        public DateTime Birthdate { get; set; }
        public string Referee_Lastname { get; set; }
        public int Attendant_Id { get; set; }
    }
}
