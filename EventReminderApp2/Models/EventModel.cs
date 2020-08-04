using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EventReminderApp2.Models
{
    public class EventModel
    {
        public int EventId { get; set; }
        public int UserId { get; set; }
        public string EventName { get; set; }
        public string Description { get; set; }       
        public DateTime StartDate { get; set; }        
        public DateTime EndDate { get; set; }
        public string StartDateStr { get { return StartDate.ToString(); } }
        public string EndDateStr { get { return EndDate.ToString(); } }

        public string StartDateString { get { return StartDate.ToString("yyyy-MM-dd HH:mm"); } }       

        public string Email { get; set; }
        public string MailSend { get; set; }
    }
}