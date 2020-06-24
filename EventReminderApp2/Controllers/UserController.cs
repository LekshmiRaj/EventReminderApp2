using EventReminderApp2.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Web.Mvc;
using System.Timers;
using System.IO;

namespace EventReminderApp2.Controllers
{
    public class UserController : Controller
    {        
        EventRepository eventRepository = new EventRepository();       

        string logFormat = string.Empty;
        string logPath = string.Empty;

        public UserController()
        {
            Timer myTimer = new Timer();
            myTimer.Interval = 60000;
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
            if (Session["userid"] != null)
            {
                ViewBag.USERID = Session["userid"];
                ViewBag.EMAIL = Session["email"];
                ViewBag.USERNAME = Session["username"];
                return View();             
            }            
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
            
            string query = $"Select UserId,Email,Password,UserName from [dbo].[tblRegistration] where Email='{login.Email}' and Password='{login.Password}'";
            List<string> sessionVariables = eventRepository.GetUserLoginDetails(query);

            string uId = sessionVariables[0];
            string uEmail = sessionVariables[1];
            string uname = sessionVariables[2];

            if (sessionVariables != null)
            {
                Session["userid"] = uId;
                Session["email"] = uEmail;
                Session["username"] = uname;
                status = true;
                return new JsonResult { Data = new { status = status,username= uname } };
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
            string query = $"Select UserId,Email,UserName from [dbo].[tblRegistration] where Email='{email}' ";

            List<string> sessionVariables = eventRepository.GetUserLoginDetails(query);

            string uId = sessionVariables[0];
            string uEmail = sessionVariables[1];
            string uname = sessionVariables[2];

            if (sessionVariables != null)
            {
                Session["userid"] = uId;
                Session["email"] = uEmail;
                Session["username"] = uname;
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
            string query = $"Select UserId,Email,UserName from [dbo].[tblRegistration] where Email='{email}' ";

            List<string> sessionVariables = eventRepository.GetUserLoginDetails(query);

            string uId = sessionVariables[0];
            string uEmail = sessionVariables[1];
            string uname = sessionVariables[2];

            if (sessionVariables != null)
            {
                Session["userid"] = uId;
                Session["email"] = uEmail;
                Session["username"] = uname;
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
                string e = ex.ToString();
                writeLogFile(toEmail,e);
                return false;
            }
        }
        
        public void writeLogFile(string toEmail, string e)
        {
            logPath = @"D:\EmailLogFile";
            logFormat = DateTime.Now.ToLongDateString().ToString() + " - " +
                DateTime.Now.ToLongTimeString().ToString() + " ==> ";

            System.IO.File.AppendAllText(logPath + "\\" + "LogFile.txt", logFormat +" " +toEmail + "Reason- " + e+ Environment.NewLine);
                  
            return;
        }

        [HttpPost]
        public JsonResult ResetPassword(string email)
        {
            string pass;
            string qry;
            var status = false;
            string query = $"Select Email from [dbo].[tblRegistration] where Email='{email}' ";
            bool verify = eventRepository.verifyEmail(query);
            if (verify)
            {
                qry= $"Select Password from [dbo].[tblRegistration] where Email='{email}' ";
                pass = eventRepository.getPassword(qry);
                //send mail
                string ebody = "<p>Hi,<br />Your password is-<br /> "+pass+ "</ p > ";
                SendEmail(email, "Password", ebody);
                status = true;
                return new JsonResult { Data = new { status = status } };
            }
            else
            {
                return new JsonResult { Data = new { status = status } };
            }
                                              
        }
        
    }
}