using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EventReminderApp2.Models
{
    public class Registration
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Repassword { get; set; }
       
        public string ResetPasswordCode { get; set; }

        public DateTime DOB { get; set; }
        public string DobString { get { return DOB.ToString("dd-MM-yyyy"); } }
        public string Phone { get; set; }
    }
}