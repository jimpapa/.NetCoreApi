using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1.Models
{
    public class User
    {
        public string username { get; set; }
        public string password { get; set; }
        public string Lastname { get; set; }
        public string Firstname { get; set; }
        public string Comments { get; set; }
        public int Status { get; set; }
        public int User_Group_Id { get; set; }
        public int Attendant_Id { get; set; }
        public string Question { get; set; }
        public string Answer { get; set; }
        public int Search_Attendant_Id { get; set; }
        public DateTime pwd_expired { get; set; }
    }
}
