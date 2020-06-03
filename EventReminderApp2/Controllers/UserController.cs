using EventReminderApp2.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

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
                string query = $"Select UserId,UserName,Password from [dbo].[tblRegistration] where UserName='{login.UserName}' and Password='{login.Password}'";
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
                    Session["username"] = login.UserName;
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


    }
}