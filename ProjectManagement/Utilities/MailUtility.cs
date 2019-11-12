using ProjectManagement.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace ProjectManagement.Utilities
{
    public class MailUtility
    {
        public static List<Tuple<string, string>> verifyEmailList(List<string> mails)
        {
            ProjectManagementEntities db = new ProjectManagementEntities();
            List<AspNetUser> users = db.AspNetUsers.ToList();
            var result = new List<Tuple<string, string>>();
            foreach (string mail in mails)
            {
                if (!users.Any(u => u.Email.Equals(mail)))
                {
                    result.Add(new Tuple<string, string>(mail, "Email not found"));
                }
                else if (users.Where(u => u.Email.Equals(mail)).FirstOrDefault().EmailConfirmed == false)
                {
                    result.Add(new Tuple<string, string>(mail, "Email not confirmed"));
                }
            }
            return result;
        }
        public static string getEmailFromId(string userId)
        {
            ProjectManagementEntities db = new ProjectManagementEntities();
            List<AspNetUser> users = db.AspNetUsers.ToList();
            return users.Where(u => u.Id.Equals(userId)).FirstOrDefault().Email;
        }

        public static bool validateEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;
            return Regex.IsMatch(email, @"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z", RegexOptions.IgnoreCase);
        }
    }
}