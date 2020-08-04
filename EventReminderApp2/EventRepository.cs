using EventReminderApp2.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace EventReminderApp2
{
    public class EventRepository
    {

        const string ConnectionString = @"Data Source=DESKTOP-3VSFCTT\LEKSHMISQL; Initial Catalog=EventReminderDB; Integrated Security=True";
        SqlConnection con = new SqlConnection(ConnectionString);

        public void AddEditEvent(EventModel eventModel, string userid)
        {
            string qry = string.Empty;
            if (eventModel.EventId > 0)
            {
                var startDate = eventModel.StartDate.ToString("yyyy-MM-dd HH:mm");
                var endDate = eventModel.EndDate.ToString("yyyy-MM-dd HH:mm");

                qry = "update tblEvents set UserId = '" + userid +
                    "', EventName= '" + eventModel.EventName + "',Description= '" + eventModel.Description + "', StartDate='" + startDate + "', EndDate='" + endDate + "', MailSend='" + " " + "' where EventId=" + eventModel.EventId;
            }
            else
            {
                var startDate = eventModel.StartDate.ToString("yyyy-MM-dd HH:mm");
                var endDate = eventModel.EndDate.ToString("yyyy-MM-dd HH:mm");
                
                qry = "insert into tblEvents(UserId,EventName,Description,StartDate,EndDate)" +
                    " values('" + userid + "','" + eventModel.EventName + "','" + eventModel.Description + "','" + startDate + "','" + endDate + "')";
            }
            this.AddUpdateDeleteSQL(qry);
        }

        public int AddUpdateDeleteSQL(string qry)
        {
            con.Open();
            int count = new SqlCommand(qry, con).ExecuteNonQuery();
            con.Close();
            return count;
        }

        public EventModel GetEventById(int eventId)
        {
            string qry = "select * from tblEvents where EventId=" + eventId;
            DataRow row = GetSQLList(qry).Rows[0];
          
            return new EventModel
            {
                EventId = Convert.ToInt32(row.ItemArray[0]),
                UserId = Convert.ToInt32(row.ItemArray[1]),
                EventName = row.ItemArray[2].ToString(),
                Description = row.ItemArray[3].ToString(),
                StartDate = Convert.ToDateTime(row.ItemArray[4]),
                EndDate = Convert.ToDateTime(row.ItemArray[5]),
            };
        }

        private DataTable GetSQLList(string qry)
        {
            con.Open();
            SqlCommand cmd = new SqlCommand(qry, con);
            SqlDataReader sdr = cmd.ExecuteReader();
            System.Data.DataTable datatable = new System.Data.DataTable();
            datatable.Load(sdr);
            con.Close();
            return datatable;            
        }

        public int DeleteEvent(int eventId)
        {
            string qry = "delete from tblEvents where EventId=" + eventId;
            return this.AddUpdateDeleteSQL(qry);
        }

        public List<EventModel> ListEvents(string userid)
        {
            string query = "Select * from [dbo].[tblEvents] where UserId=" + userid;
            DataTable datatable = GetSQLList(query);

            List<EventModel> eventList = new List<EventModel>();

            foreach (DataRow row in datatable.Rows)
            {
                EventModel eventModel = new EventModel();
                eventModel.EventId = Convert.ToInt32(row.ItemArray[0]);
                eventModel.UserId = Convert.ToInt32(row.ItemArray[1]);
                eventModel.EventName = row.ItemArray[2].ToString();
                eventModel.Description = row.ItemArray[3].ToString();
                eventModel.StartDate = Convert.ToDateTime(row.ItemArray[4]);
                eventModel.EndDate = Convert.ToDateTime(row.ItemArray[5]);
                eventList.Add(eventModel);
            }
            return eventList;
        }

        public List<string> GetUserLoginDetails(string qry)
        {
            List<string> sessionVariables = new List<string>();
            con.Open();
            SqlCommand cmd = new SqlCommand(qry, con);
            cmd.CommandType = CommandType.Text;
            SqlDataAdapter sda = new SqlDataAdapter(cmd);
            DataTable datatable = new DataTable();
            sda.Fill(datatable);

            if (datatable.Rows.Count == 1)
            {
                DataRow row = datatable.Rows[0];
                string uid = row["UserId"].ToString();
                string mail = row["Email"].ToString();
                string name = row["UserName"].ToString();
                sessionVariables.Add(uid);
                sessionVariables.Add(mail);
                sessionVariables.Add(name);
            }
            con.Close();
            return sessionVariables;
        }

        public List<EventModel> GetMailDetails(string qry)
        {
            List<EventModel> mailDetails = new List<EventModel>();
            con.Open();
            SqlCommand cmd = new SqlCommand(qry, con);
            cmd.CommandType = CommandType.Text;
            SqlDataAdapter sda = new SqlDataAdapter(cmd);
            DataTable datatable = new DataTable();
            sda.Fill(datatable);

            if (datatable.Rows.Count != 0)
            {
                foreach (DataRow row in datatable.Rows)
                {
                    EventModel eventModel = new EventModel();
                    eventModel.Email= row["Email"].ToString();
                    eventModel.StartDate= Convert.ToDateTime(row["StartDate"]);
                    eventModel.EventName= row["EventName"].ToString();
                    eventModel.Description= row["Description"].ToString(); 
                    eventModel.EventId = Convert.ToInt32(row["EventId"]);
                    eventModel.MailSend = row["MailSend"].ToString();
                    mailDetails.Add(eventModel);
                }
            }                
            con.Close();
            return mailDetails;
        }

        public bool verifyEmail(string qry)
        {
            DataTable datatable = GetSQLList(qry);
            if (datatable.Rows.Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public string getPassword(string qry)
        {
            string pass = "";
            DataTable dataTable = GetSQLList(qry);

            if(dataTable != null)
            {
              DataRow row = GetSQLList(qry).Rows[0];
                if(row != null)
                {
                   pass = row["Password"].ToString();
                }
            }
            return pass;
        }
        
        public Registration Get_User(string qry)
        {            
            DataRow row = GetSQLList(qry).Rows[0];
            Registration registration = new Registration();
                           
            registration.UserId = Convert.ToInt32(row["UserId"]);
            registration.UserName = row["UserName"].ToString();
            registration.Email = row["Email"].ToString();
            registration.Password = row["Password"].ToString();
            registration.ResetPasswordCode = row["ResetPasswordCode"].ToString();
            registration.DOB = Convert.ToDateTime(string.IsNullOrEmpty(row["DOB"].ToString()) ? "01-01-1111" : row["DOB"].ToString());
            //registration.DOB = Convert.ToDateTime(string.IsNullOrEmpty(row["DOB"].ToString())? DateTime.Now.ToString(): row["DOB"].ToString());
            registration.Phone = ((row["Phone"]) ?? "").ToString();
            
            return registration;
        }
    }
}