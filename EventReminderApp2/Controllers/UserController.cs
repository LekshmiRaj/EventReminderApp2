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

        public void SendResetPasswordLinkEmail(string toEmail, string activationCode)
        {
            var verifyUrl = "/User/ResetPassword/" + activationCode;
            var link = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, verifyUrl);
            var subject = "Reset Password";
            string ebody = "<p>Hi,<br /><br/>We got request for reset your account password. Please click on the below link to reset your password" +
                    "<br/><br/><a href=" + link + ">Reset password link</a>";

            string senderEmail = System.Configuration.ConfigurationManager.AppSettings["SenderEmail"].ToString();
                string senderPassword = System.Configuration.ConfigurationManager.AppSettings["SenderPassword"].ToString(); ;

                SmtpClient client = new SmtpClient("smtp.gmail.com", 587);
                client.EnableSsl = true;
                client.Timeout = 100000;
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential(senderEmail, senderPassword);
                MailMessage mailMessage = new MailMessage(senderEmail, toEmail, subject, ebody);
                mailMessage.IsBodyHtml = true;
                mailMessage.BodyEncoding = UTF8Encoding.UTF8;
                client.Send(mailMessage);
                return;
        }

        [HttpPost]
        public JsonResult ForgotPassword(string email)
        {            
            string qry;
            var status = false;
            string query = $"Select Email from [dbo].[tblRegistration] where Email='{email}' ";
            bool verify = eventRepository.verifyEmail(query);
            if (verify)
            {                                
                string resetCode = Guid.NewGuid().ToString();
                SendResetPasswordLinkEmail(email, resetCode);
                qry=$"update tblRegistration set ResetPasswordCode='{resetCode}' where Email='{email}'";
                eventRepository.AddUpdateDeleteSQL(qry);
                status = true;
                return new JsonResult { Data = new { status = status } };
            }
            else
            {
                return new JsonResult { Data = new { status = status } };
            }
                                              
        }

        public ActionResult ResetPassword(string id)
        {
            string qry= $"Select * from [dbo].[tblRegistration] where ResetPasswordCode='{id}' ";
            var user = eventRepository.Get_User(qry);
            if(user != null)
            {
                ResetPasswordModel resetPasswordModel = new ResetPasswordModel();
                resetPasswordModel.ResetCode = id;
                return View(resetPasswordModel);
            }
            else
            {
                return HttpNotFound();
            }            
        }

        [HttpPost]
        public ActionResult ResetPassword(ResetPasswordModel resetPasswordModel)
        {
            string qry = $"Select * from [dbo].[tblRegistration] where ResetPasswordCode='{resetPasswordModel.ResetCode}' ";
            var user = eventRepository.Get_User(qry);
            if(user != null)
            {
                string query= "update tblRegistration set Password = '" + resetPasswordModel.NewPassword + "' where ResetPasswordCode ='" + resetPasswordModel.ResetCode+"'";
                eventRepository.AddUpdateDeleteSQL(query);
                string query2= "update tblRegistration set ResetPasswordCode = '" + "" + "' where ResetPasswordCode ='" + resetPasswordModel.ResetCode+"'";
                ViewBag.Message = "New password updated successfully";
            }
            else
            {
                ViewBag.Message = "Something invalid";
            }
            return View(resetPasswordModel);
        }

        [HttpPost]
        public JsonResult GetUserDetails()
        {
            string userid="";
            if (Session["userid"] != null)
            {
                userid = Session["userid"].ToString();
            }
            string qry = $"Select * from [dbo].[tblRegistration] where UserId='{userid}' ";
            Registration regUser = eventRepository.Get_User(qry);
            return new JsonResult { Data = regUser, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        [HttpPost]
        public ActionResult UpdateUserDetails(Registration registration)
        {
            var status = false;
            string query = "update tblRegistration set UserName = '" + registration.UserName +
                    "', Email= '" + registration.Email + "' where UserId=" + registration.UserId;
            int count = eventRepository.AddUpdateDeleteSQL(query);

            if (count == 1)
            {
                status = true;
                Session["email"] = registration.Email;
                Session["username"] = registration.UserName;
            }
            return new JsonResult { Data = new { status = status, username = registration.UserName } };
        }
    }
}