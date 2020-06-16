using EventReminderApp2.Models;
using System.Collections.Generic;
using System.Web.Mvc;


namespace EventReminderApp2.Controllers
{
    public class UserController : Controller
    {        
        EventRepository eventRepository = new EventRepository();

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

    }
}