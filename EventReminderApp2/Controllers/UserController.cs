using EventReminderApp2.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Web.Mvc;
using System.Timers;


namespace EventReminderApp2.Controllers
{
    public class UserController : Controller
    {        
        EventRepository eventRepository = new EventRepository();

        public UserController()
        {
            Timer myTimer = new Timer();
            myTimer.Interval = 60000; //3600000;
            myTimer.AutoReset = true;
            myTimer.Elapsed += new ElapsedEventHandler(SendMailToUser);
            myTimer.Enabled = true;
        }

        // GET: User
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult UserHome()
        {            
            return View();
        }


        [HttpPost]
        public ActionResult SignUp(Registration registration)
        {
            var status = false;
            string query = "insert into tblRegistration(UserName,Email,Password)" +
                    " values('" + registration.UserName + "','" + registration.Email + "','" + registration.Password + "')";
            int count = eventRepository.AddUpdateDeleteSQL(query);
            
            if(count == 1)
            {                
                status = true;
            }
            return new JsonResult { Data = new { status = status } };
        }


        [HttpPost]
        public JsonResult SignIn(Registration login)
        {
            var status = false;
            string query = $"Select UserId,Email,Password from [dbo].[tblRegistration] where Email='{login.Email}' and Password='{login.Password}'";
            List<string> sessionVariables = eventRepository.GetUserLoginDetails(query);

            string uId = sessionVariables[0];
            string uEmail = sessionVariables[1];

            if (sessionVariables != null)
            {
                Session["userid"] = uId;
                Session["email"] = uEmail;
                status = true;
                return new JsonResult { Data = new { status = status } };
            }
            else
            {
                status = false;                   
                return new JsonResult { Data = new { status = status } };
            }                                            
        }


        [HttpPost]
        public JsonResult SaveEvent(EventModel eventModel)
        {           
            string userid = Session["userid"].ToString();
            eventRepository.AddEditEvent(eventModel, userid);            
            var status = true;            
            return new JsonResult { Data = new { status = status } };
        }


        public JsonResult GetEvents()
        {
            string userid=null;
            if (Session["userid"] != null)
            {
                userid = Session["userid"].ToString();
            }
            List<EventModel> eventList = eventRepository.ListEvents(userid);

            return new JsonResult { Data = eventList, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        [HttpPost]
        public JsonResult DeleteEvent(int eventID)
        {
            var status = false;
            var d = eventRepository.DeleteEvent(eventID);
            if (d != 0)
            {
                status = true;
            }
            return new JsonResult { Data = new { status = status } };
        }
        
        public ActionResult ClearSessions()
        {
            Session.Clear();                        
            return RedirectToAction("UserHome");
        }

        [HttpPost]
        public JsonResult Edit(int id)
        {
            EventModel eventModel = eventRepository.GetEventById(id);
            return new JsonResult { Data = eventModel, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        [HttpPost]
        public JsonResult GoogleLogin(string email, string name, string gender, string lastname, string location)
        {
            string qry;
            var status = false;
            string query = $"Select UserId,Email from [dbo].[tblRegistration] where Email='{email}' ";

            List<string> sessionVariables = eventRepository.GetUserLoginDetails(query);

            string uId = sessionVariables[0];
            string uEmail = sessionVariables[1];

            if (sessionVariables != null)
            {
                Session["userid"] = uId;
                Session["email"] = uEmail;
                status = true;
            }
            else
            {
                qry = "insert into tblRegistration(UserName,Email)" +
                    " values('" + name + "','" + email + "')";
                eventRepository.AddUpdateDeleteSQL(qry);
                status = true;
            }
           
            return new JsonResult { Data = new { status = status } };
        }

        [HttpPost]
        public JsonResult FacebookLogin(string email, string name)
        {
            string qry;
            var status = false;
            string query = $"Select UserId,Email from [dbo].[tblRegistration] where Email='{email}' ";

            List<string> sessionVariables = eventRepository.GetUserLoginDetails(query);

            string uId = sessionVariables[0];
            string uEmail = sessionVariables[1];

            if (sessionVariables != null)
            {
                Session["userid"] = uId;
                Session["email"] = uEmail;
                status = true;
            }
            else
            {
                qry = "insert into tblRegistration(UserName,Email)" +
                      " values('" + name + "','" + email + "')";
                eventRepository.AddUpdateDeleteSQL(qry);
                status = true;
            }
            
            return new JsonResult { Data = new { status = status } };
        }

        //mail  notification        
        public void SendMailToUser(object sender, EventArgs e)
        {
            bool status = false;     
            
            var currentDate = DateTime.Now;            
            var eventDate= currentDate.AddMinutes(+5).ToString("yyyy-MM-dd HH:mm");             
            string qry = $"Select Email,StartDate,EventName,Description from tblRegistration join tblEvents on (tblRegistration.UserId=tblEvents.UserId) where StartDate='{eventDate}' ";
            List<EventModel> mailDetails= eventRepository.GetMailDetails(qry);
            foreach (EventModel item in mailDetails)
            {
                string ebody = "<p>Hi,<br />This is a reminder of the following event-<br />Event:"+item.EventName+ "<br />"+"Description:" + item.Description + "<br />" +"Time:" + item.StartDate + "</ p > ";
                status = SendEmail(item.Email, "EventReminder", ebody);
            }              

        }

        public bool SendEmail(string toEmail, string subject, string emailBody)
        {
            try
            {
                string senderEmail = System.Configuration.ConfigurationManager.AppSettings["SenderEmail"].ToString();
                string senderPassword = System.Configuration.ConfigurationManager.AppSettings["SenderPassword"].ToString(); ;

                SmtpClient client = new SmtpClient("smtp.gmail.com",587);
                client.EnableSsl = true;
                client.Timeout = 100000;
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential(senderEmail, senderPassword);
                MailMessage mailMessage = new MailMessage(senderEmail, toEmail, subject, emailBody);
                mailMessage.IsBodyHtml = true;
                mailMessage.BodyEncoding = UTF8Encoding.UTF8;
                client.Send(mailMessage);

                return true;
            }
            catch(Exception ex)
            {
                Exception e = ex;               
                return false;
            }

        }

    }
}