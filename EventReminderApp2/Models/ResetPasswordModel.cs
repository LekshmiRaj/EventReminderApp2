using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EventReminderApp2.Models
{
    public class ResetPasswordModel
    {
        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set; }
        public string ResetCode { get; set; }
    }
}