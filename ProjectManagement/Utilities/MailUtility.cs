﻿using Microsoft.AspNet.Identity;
using ProjectManagement.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Mail;
using System.Net.Mime;
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
                if(!validateEmail(mail))
                {
                    result.Add(new Tuple<string, string>(mail, "is not a valid Email"));
                }
                else if (!users.Any(u => u.Email.Equals(mail)))
                {
                    result.Add(new Tuple<string, string>(mail, "is not found"));
                }
                else if (users.Where(u => u.Email.Equals(mail)).FirstOrDefault().EmailConfirmed == false)
                {
                    result.Add(new Tuple<string, string>(mail, "is not confirmed"));
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

        public static string getIdFromEmail(string email)
        {
            ProjectManagementEntities db = new ProjectManagementEntities();
            List<AspNetUser> users = db.AspNetUsers.ToList();
            return users.Where(u => u.Email.Equals(email)).FirstOrDefault().Id;
        }

        public static bool validateEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;
            return Regex.IsMatch(email, @"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z", RegexOptions.IgnoreCase);
        }

        public static int GenerateRandomNo()
        {
            int _min = 1000;
            int _max = 9999;
            Random _rdm = new Random();
            return _rdm.Next(_min, _max);
        }

        public static void sendMail(IdentityMessage message)
        {
            MailMessage msg = new MailMessage();
            msg.From = new MailAddress(ConfigurationManager.AppSettings["Email"].ToString());
            msg.To.Add(new MailAddress(message.Destination));
            msg.Subject = message.Subject;
            msg.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(message.Body, null, MediaTypeNames.Text.Html));

            SmtpClient smtpClient = new SmtpClient(ConfigurationManager.AppSettings["Host"].ToString(), Convert.ToInt32(587));
            System.Net.NetworkCredential credentials = new System.Net.NetworkCredential(ConfigurationManager.AppSettings["Email"].ToString(), ConfigurationManager.AppSettings["Password"].ToString());
            smtpClient.Credentials = credentials;
            smtpClient.EnableSsl = true;
            smtpClient.Send(msg);
        }

        public static string getProjectAdmin(string name)
        {
            ProjectManagementEntities db = new ProjectManagementEntities();
            Project proj = db.Projects.Where(p => p.ProjectName.Equals(name)).FirstOrDefault();
            string adminId = db.ProjectUser_MTM.Where(pu => pu.ProjectId == proj.Id && pu.UserRole == db.AspNetRoles.Where(r => r.Name.Equals("Admin")).FirstOrDefault().Id).FirstOrDefault().UserId;
            string adminMail = getEmailFromId(adminId);
            return adminMail;
        }
        
        //public static List<ProjectIndexViewModel> getProjectsbyEmail(string email)
        //{
        //    ProjectManagementEntities db = new ProjectManagementEntities();
        //    List<ProjectIndexViewModel> projList = new List<ProjectIndexViewModel>();
        //    var list = db.ProjectUser_MTM.Where(pu => pu.UserId.Equals(getIdFromEmail(email))).Select(p => p.Project);
        //}
    }
}