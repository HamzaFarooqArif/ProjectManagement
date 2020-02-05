using ProjectManagement.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProjectManagement.Utilities
{
    public class Notifications
    {
        public static bool addToNotifications(string email, string subject, string body)
        {
            ProjectManagementEntities db = new ProjectManagementEntities();
            AspNetUser user = MailUtility.getUserFromEmail(email);
            if(user != null)
            {
                Notification notif = new Notification();
                notif.UserId = user.Id;
                notif.Time = DateTime.Now;
                notif.IsRead = false;
                notif.Subject = subject;
                notif.Body = body;

                db.Notifications.Add(notif);
                db.SaveChanges();
                return true;
            }
            else
            {
                return false;
            }   
        }
        public static List<Notification> GetNotifications(string email)
        {
            List<Notification> result = new List<Notification>();
            if(email != null)
            {
                ProjectManagementEntities db = new ProjectManagementEntities();
                AspNetUser user = MailUtility.getUserFromEmail(email);
                result = db.Notifications.Where(n => n.UserId.Equals(user.Id)).ToList();
                result = result.OrderByDescending(o => o.Time).ToList();
            }
            return result;
        }
    }
}