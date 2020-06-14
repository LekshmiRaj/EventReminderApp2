using EventReminderApp2.Models;
using Facebook;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace EventReminderApp2.Controllers
{
    public class UserController : Controller
    {
        string ConnectionString = @"Data Source=DESKTOP-3VSFCTT\LEKSHMISQL; Initial Catalog=EventReminderDB; Integrated Security=True";
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
            using (SqlConnection con = new SqlConnection(ConnectionString))
            {
                con.Open();
                string query = "Insert Into tblRegistration Values(@UserName,@Email,@Password)";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@UserName", registration.UserName);
                cmd.Parameters.AddWithValue("@Email", registration.Email);
                cmd.Parameters.AddWithValue("@Password", registration.Password);
                cmd.ExecuteNonQuery();
                status = true;
            }
            return new JsonResult { Data = new { status = status } };
        }


        [HttpPost]
        public JsonResult SignIn(Registration login)
        {
            using (SqlConnection con = new SqlConnection(ConnectionString))
            {
                con.Open();
                string query = $"Select UserId,Email,Password from [dbo].[tblRegistration] where Email='{login.Email}' and Password='{login.Password}'";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.CommandType = CommandType.Text;                

                SqlDataAdapter sda = new SqlDataAdapter(cmd);
                DataTable datatable = new DataTable();
                sda.Fill(datatable);
                if (datatable.Rows.Count == 1)
                {
                    DataRow row = datatable.Rows[0];
                    login.UserId = Convert.ToInt32(row["Userid"]);
                    string uid = row["Userid"].ToString();
                    Session["userid"] = uid;
                    Session["Email"] = login.Email;
                    var status = true;
                    con.Close();
                    return new JsonResult { Data = new { status = status } };
                }
                else
                {
                    var status = false;
                    con.Close();
                    return new JsonResult { Data = new { status = status } };
                }                                
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
            var status = false;
            using (SqlConnection con = new SqlConnection(ConnectionString))
            {
                string qry;               
                con.Open();
                string query = $"Select UserId,Email from [dbo].[tblRegistration] where Email='{email}' ";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.CommandType = CommandType.Text;

                SqlDataAdapter sda = new SqlDataAdapter(cmd);
                DataTable datatable = new DataTable();
                sda.Fill(datatable);
                if (datatable.Rows.Count == 1)
                {
                    DataRow row = datatable.Rows[0];                   
                    string uid = row["UserId"].ToString();
                    string mail = row["Email"].ToString();
                    Session["userid"] = uid;
                    Session["email"] = mail;

                    status = true;
                }
                else
                {
                    qry = "insert into tblRegistration(UserName,Email)" +
                    " values('" + name + "','" + email + "')";
                    eventRepository.AddUpdateDeleteSQL(qry);
                    status = true;
                }
            }
            return new JsonResult { Data = new { status = status } };
        }
               


    }
}