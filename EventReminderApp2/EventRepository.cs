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
                DateTime start = eventModel.StartDate;
                DateTime end = eventModel.EndDate;

                string startDate = start.ToString("yyyy-MM-dd");
                string endDate = end.ToString("yyyy-MM-dd");

                qry = "update tblEvents set UserId = '" + userid +
                    "', EventName= '" + eventModel.EventName + "',Description= '" + eventModel.Description + "', StartDate='" + startDate + "', EndDate='" + endDate + "' where EventId=" + eventModel.EventId;
            }
            else
            {
                DateTime start = eventModel.StartDate;
                DateTime end = eventModel.EndDate;

                string startDate = start.ToString("yyyy-MM-dd");
                string endDate = end.ToString("yyyy-MM-dd");

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

            DateTime start = Convert.ToDateTime(row.ItemArray[4]);
            DateTime end = Convert.ToDateTime(row.ItemArray[5]);

            return new EventModel
            {
                EventId = Convert.ToInt32(row.ItemArray[0]),
                UserId = Convert.ToInt32(row.ItemArray[1]),
                EventName = row.ItemArray[2].ToString(),
                Description = row.ItemArray[3].ToString(),
                StartDate = Convert.ToDateTime(row.ItemArray[4]),
                EndDate = Convert.ToDateTime(row.ItemArray[5]),

                //StartDate = Convert.ToDateTime(start.ToString("yyyy-MM-dd")),
                //EndDate = Convert.ToDateTime(end.ToString("yyyy-MM-dd")),



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

    }
}