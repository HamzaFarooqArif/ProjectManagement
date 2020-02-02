using Microsoft.AspNet.Identity;
using ProjectManagement.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Mail;
using System.Net.Mime;
using System.Security.Principal;
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

        public static List<ProjectIndexViewModel> getProjectsbyEmail(string email)
        {
            ProjectManagementEntities db = new ProjectManagementEntities();
            string uid = getIdFromEmail(email);
            List<Project> projList = db.ProjectUser_MTM.Where(pu => pu.UserId.Equals(uid)).Select(p => p.Project).ToList();
            List<ProjectIndexViewModel> projModelList = new List<ProjectIndexViewModel>();

            foreach (Project p in projList)
            {
                ProjectIndexViewModel projmodel = new ProjectIndexViewModel();
                projmodel.id = p.Id;
                projmodel.name = p.ProjectName;
                projmodel.admin = MailUtility.getProjectAdmin(p.ProjectName);
                if (projmodel.admin.Equals(email))
                {
                    projmodel.editable = true;
                }
                else
                {
                    projmodel.editable = false;
                }
                projModelList.Add(projmodel);
            }
            return projModelList;
        }
        public static List<ProjectUserViewModel> getEmailsByProjectName(string projectName)
        {
            ProjectManagementEntities db = new ProjectManagementEntities();
            Project proj = db.Projects.Where(p => p.ProjectName.Equals(projectName)).FirstOrDefault();
            List<AspNetUser> users = proj.ProjectUser_MTM.Select(u=>u.AspNetUser).ToList();
            List<ProjectUserViewModel> result = new List<ProjectUserViewModel>();
            foreach (AspNetUser user in users)
            {
                ProjectUserViewModel model = new ProjectUserViewModel();
                model.id = user.Id;
                model.email = user.Email;
                model.isAdmin = getProjectAdmin(projectName).Equals(user.Email);
                model.role = getUserRole(user.Email, projectName);
                if(getConfirmation(user.Email, projectName).Equals("0"))
                {
                    model.confirmed = true;
                }
                else model.confirmed = false;
                result.Add(model);
            }
            return result;
        }
        public static string getUserRole(string email, string projectName)
        {
            ProjectManagementEntities db = new ProjectManagementEntities();
            return db.AspNetUsers.Where(u => u.Email.Equals(email)).FirstOrDefault().ProjectUser_MTM.ToList().Where(p => p.Project.ProjectName.Equals(projectName)).FirstOrDefault().AspNetRole.Name;
        }
        public static string getConfirmation(string email, string projectName)
        {
            ProjectManagementEntities db = new ProjectManagementEntities();
            return db.AspNetUsers.Where(u => u.Email.Equals(email)).FirstOrDefault().ProjectUser_MTM.ToList().Where(p => p.Project.ProjectName.Equals(projectName)).FirstOrDefault().Confirmation;
        }
        public static List<ProjectUserViewModel> getEmailsByProjectId(int id)
        {
            ProjectManagementEntities db = new ProjectManagementEntities();
            Project proj = db.Projects.Where(p => p.Id == id).FirstOrDefault();
            return getEmailsByProjectName(proj.ProjectName);
        }
        public static string getCurrentEmail()
        {
            return getEmailFromId(HttpContext.Current.User.Identity.GetUserId());
        }
    }
}