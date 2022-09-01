using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Script.Serialization;

namespace FYP_Customer_Behavior_.Models
{
    public class NotificationServices
    {

        static FYPEntities _db = new FYPEntities();
        internal static SqlDependency dependency = null;
        internal static SqlCommand command = null;
        static int s_id;

        public static string GetNotification(int s_idd)
        {
            s_id = s_idd;
            try
            {
                var messages = new List<Notification>();
                using (var connection = new SqlConnection(_db.Database.Connection.ConnectionString))
                {
                    connection.Open();
                    // int s_id = ((SellerDto)Session["sellerSession"]).Id;

                    using (command = new SqlCommand(@"SELECT [NotificationId],[SellerId],[CustomerId],[Message] , [Status] , [Date] FROM [dbo].[Notification] where Status = 'unread' and SellerId ='" + s_idd + "'", connection))
                    {

                        command.Notification = null;
                        if (dependency == null)
                        {

                            dependency = new SqlDependency(command);
                            dependency.OnChange += new OnChangeEventHandler(dependency_OnChange);
                        }
                        if (connection.State == System.Data.ConnectionState.Closed)
                        {
                            connection.Open();
                        }
                        var reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            messages.Add(item: new Notification
                            {
                                NotificationId = (int)reader["NotificationId"],
                                Status = reader["Status"] != DBNull.Value ? (string)reader["Status"] : "",
                                Message = reader["Message"] != DBNull.Value ? (string)reader["Message"] : "",

                                //  ExtraColumn = reader["ExtraColumn"] != DBNull.Value ? (string)reader["ExtraColumn"] : ""
                            });
                        }
                        var jsonSerializer = new JavaScriptSerializer();
                        var json = jsonSerializer.Serialize(messages);
                        return json;
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        private static void dependency_OnChange(object sender, SqlNotificationEventArgs e)
        {
            if (dependency != null)
            {
                dependency.OnChange -= dependency_OnChange;
                dependency = null;
            }
            if (e.Type == SqlNotificationType.Change)
            {
                var ress = _db.Notifications.FirstOrDefault(p => p.NotificationId == (from nn in _db.Notifications select nn.NotificationId).Max());
                Hubs.MyHub.Send(ress.SellerId);
            }
        }




    }
}