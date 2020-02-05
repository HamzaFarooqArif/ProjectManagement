using ProjectManagement.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using ProjectManagement.Utilities;

namespace ProjectManagement.Controllers
{
    [Authorize]
    public class ProjectController : Controller
    {
        public JsonResult verifyUserEmail(string item)
        {
            ProjectManagementEntities db = new ProjectManagementEntities();
            List<AspNetUser> users = db.AspNetUsers.ToList();
            string currentEmail = users.Where(u => u.Id.Equals(User.Identity.GetUserId())).FirstOrDefault().Email;
            if(!MailUtility.validateEmail(item))
            {
                var data = new { id = 1, message = "Enter a valid email" };
                return Json(data, JsonRequestBehavior.AllowGet);
            }
            if (item.Equals(currentEmail))
            {
                var data = new { id = 2, message = "Your email is not needed explicitly" };
                return Json(data, JsonRequestBehavior.AllowGet);
            }
            if (!users.Any(u => u.Email.Equals(item)))
            {
                var data = new { id = 3, message = "Email not found" };
                return Json(data, JsonRequestBehavior.AllowGet);
            }
            if (users.Where(u => u.Email.Equals(item)).FirstOrDefault().EmailConfirmed == false)
            {
                var data = new { id = 4, message = "Email not confirmed" };
                return Json(data, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var data = new { id = 0, message = "" };
                return Json(data, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult confrimJoinProject(string email, string project, string code)
        {
            ProjectManagementEntities db = new ProjectManagementEntities();
            string userId = MailUtility.getIdFromEmail(email);
            int projectId = db.Projects.Where(p => p.ProjectName.Equals(project)).FirstOrDefault().Id;
            string confirmation = db.ProjectUser_MTM.Where(pu => pu.UserId.Equals(userId) && pu.ProjectId == projectId).FirstOrDefault().Confirmation;
            if (code.Equals(confirmation))
            {
                db.ProjectUser_MTM.Where(pu => pu.UserId.Equals(userId) && pu.ProjectId == projectId).FirstOrDefault().Confirmation = "0";
                db.SaveChanges();
                return Content("You joined project " + project + " successfully");
            }
            else if(confirmation.Equals("0"))
            {
                return Content("You already joined project " + project);
            }
            else
            {
                return Content("Failed to join project " + project);
            }
        }
        public void sendProjConfirmation(string projectName, string email)
        {
            ProjectManagementEntities db = new ProjectManagementEntities();

            string userId = MailUtility.getIdFromEmail(email);
            int projectId = db.Projects.Where(p => p.ProjectName.Equals(projectName)).FirstOrDefault().Id;
            string confirmation = db.ProjectUser_MTM.Where(pu => pu.UserId.Equals(userId) && pu.ProjectId == projectId).FirstOrDefault().Confirmation;
            string callbackUrl = Request.Url.AbsoluteUri.Substring(0, Request.Url.AbsoluteUri.Length - Request.Url.AbsolutePath.Length) + "/Project/confrimJoinProject/?email=" + email + "&project=" + projectName + "&code=" + confirmation;

            IdentityMessage message = new IdentityMessage();
            message.Destination = email;
            message.Subject = "Join Project";
            message.Body = callbackUrl;

            MailUtility.sendMail(message);
        }
        public ActionResult Index()
        {
            List<ProjectIndexViewModel> projModelList = MailUtility.getProjectsbyEmail(MailUtility.getEmailFromId(User.Identity.GetUserId()));

            return View(projModelList);
        }

        // GET: Project/Details/5
        public ActionResult Details(int id)
        {
            ProjectManagementEntities db = new ProjectManagementEntities();
            ProjectDetailsViewModel model = new ProjectDetailsViewModel();
            Project proj = db.Projects.Where(p => p.Id == id).FirstOrDefault();

            model.id = proj.Id;
            model.name = proj.ProjectName;
            model.admin = MailUtility.getProjectAdmin(proj.ProjectName);
            model.editable = model.admin.Equals(MailUtility.getEmailFromId(User.Identity.GetUserId()));
            model.users = MailUtility.getEmailsByProjectId(id);
            
            return View(model);
        }

        // GET: Project/Create
        public ActionResult Create()
        {
            ViewBag.alertVisibility = "d-none";
            ViewBag.alertMessage = "";
            ViewBag.alertType = "danger";

            string selectListEmails = "<select multiple data-role='tagsinput'></select>";
            ViewBag.selectedEmails = selectListEmails;

            return View();
        }
        
        // POST: Project/Create
        [HttpPost]
        public ActionResult Create(ProjectCreateViewModel model)
        {
            
            ViewBag.alertVisibility = "d-none";
            ViewBag.alertMessage = "";
            ViewBag.alertType = "danger";

            
            ProjectManagementEntities db = new ProjectManagementEntities();
            List<string> emails = new List<string>();
            string selectListEmails = "<select multiple data-role='tagsinput'></select>";
            if (model.emails != null)
            {
                emails = model.emails.Split(',').ToList().Distinct().ToList();
                selectListEmails = "<select multiple data-role='tagsinput'>";
                foreach (string email in emails)
                {
                    selectListEmails += "<option selected='' value="+email+">"+email+"</option>";
                }
                selectListEmails += "</select>";
            }
            if (db.Projects.Any(u => u.ProjectName.Equals(model.name)))
            {
                ViewBag.alertVisibility = "";
                ViewBag.alertMessage = "Project name already exists";
                ViewBag.selectedEmails = selectListEmails;
                return View(model);
            }
            if (string.IsNullOrWhiteSpace(model.name))
            {
                ViewBag.alertVisibility = "";
                ViewBag.alertMessage = "Project Name is required";
                ViewBag.selectedEmails = selectListEmails;
                return View(model);
            }
            if (emails.Count > 0)
            {
                if (emails.Any(e => e.Equals(MailUtility.getEmailFromId(User.Identity.GetUserId()))))
                {
                    emails.Remove(MailUtility.getEmailFromId(User.Identity.GetUserId()));
                }
                var badMails = MailUtility.verifyEmailList(emails);
                if (badMails.Count > 0)
                {
                    ViewBag.alertVisibility = "";
                    string errorList = "";
                    foreach (Tuple<string, string> m in badMails)
                    {
                        errorList += m.Item1 + " " + m.Item2 + "<br>";
                    }
                    ViewBag.alertMessage = errorList;
                    ViewBag.selectedEmails = selectListEmails;
                    return View(model);
                }
            }

            db.Projects.Add(new Project { ProjectName = model.name });
            db.SaveChanges();

            ProjectUser_MTM p_mtm = new ProjectUser_MTM();
            p_mtm.ProjectId = db.Projects.Where(p => p.ProjectName.Equals(model.name)).FirstOrDefault().Id;
            p_mtm.UserId = User.Identity.GetUserId();
            p_mtm.UserRole = db.AspNetRoles.Where(r => r.Name.Equals("Admin")).FirstOrDefault().Id;
            p_mtm.Confirmation = "0";

            db.ProjectUser_MTM.Add(p_mtm);
            db.SaveChanges();

            foreach (string e in emails)
            {
                ProjectUser_MTM p_mtm_ = new ProjectUser_MTM();
                p_mtm_.ProjectId = db.Projects.Where(p => p.ProjectName.Equals(model.name)).FirstOrDefault().Id;
                p_mtm_.UserId = db.AspNetUsers.Where(u => u.Email.Equals(e)).FirstOrDefault().Id;
                p_mtm_.UserRole = db.AspNetRoles.Where(r => r.Name.Equals("User")).FirstOrDefault().Id;
                p_mtm_.Confirmation = MailUtility.GenerateRandomNo().ToString(); ;
                db.ProjectUser_MTM.Add(p_mtm_);
                db.SaveChanges();
            }

            foreach (string e in emails)
            {
                sendProjConfirmation(model.name, e);
            }
            return RedirectToAction("Index", "Project");
        }

        // GET: Project/Edit/5
        public ActionResult Edit(int id)
        {
            ViewBag.alertVisibility = "d-none";
            ViewBag.alertMessage = "";
            ViewBag.alertType = "danger";

            string selectListEmails = "<select multiple data-role='tagsinput'></select>";
            ViewBag.selectedEmails = selectListEmails;

            ProjectManagementEntities db = new ProjectManagementEntities();

            if (!MailUtility.getProjectAdmin(db.Projects.Where(p => p.Id == id).FirstOrDefault().ProjectName).Equals(MailUtility.getEmailFromId(User.Identity.GetUserId())))
            {
                return Content("You are not allowed to edit project " + db.Projects.Where(p=>p.Id == id).FirstOrDefault().ProjectName);
            }

            ProjectCreateViewModel model = new ProjectCreateViewModel();

            Project proj = db.Projects.Where(p => p.Id == id).FirstOrDefault();
            List<string> emailsList = proj.ProjectUser_MTM.Select(u => u.AspNetUser).Select(e => e.Email).ToList();
            string emails = "";
            
            for(int i = 0; i < emailsList.Count; i++)
            {
                if (i == 0) emails += emailsList[i];
                else emails += ","+emailsList[i];
            }

            model.id = id;
            model.name = proj.ProjectName;
            model.emails = emails;

            if(emailsList.Count > 0)
            {
                selectListEmails = "<select multiple data-role='tagsinput'>";
                foreach (string e in emailsList)
                {
                    selectListEmails += "<option selected='' value=" + e + ">" + e + "</option>";
                }
                selectListEmails += "</select>";
                ViewBag.selectedEmails = selectListEmails;
            }
            
            return View(model);
        }

        // POST: Project/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, ProjectCreateViewModel model)
        {
            ViewBag.alertVisibility = "d-none";
            ViewBag.alertMessage = "";
            ViewBag.alertType = "danger";

            ProjectManagementEntities db = new ProjectManagementEntities();

            if(!MailUtility.getProjectAdmin(db.Projects.Where(p => p.Id == id).FirstOrDefault().ProjectName).Equals(MailUtility.getEmailFromId(User.Identity.GetUserId())))
            {
                return Content("You are not allowed to edit project "+model.name);
            }

            List<string> newEmails = new List<string>();
            List<string> oldEmails = db.Projects.Where(p => p.Id == id).FirstOrDefault().ProjectUser_MTM.Select(u => u.AspNetUser).ToList().Select(e => e.Email).ToList();
            List<string> emailsToAdd = new List<string>();
            List<string> emailsToRemove = new List<string>();

            string selectListEmails = "<select multiple data-role='tagsinput'></select>";
            if (model.emails != null)
            {
                newEmails = model.emails.Split(',').ToList().Distinct().ToList();
                selectListEmails = "<select multiple data-role='tagsinput'>";
                foreach (string email in newEmails)
                {
                    selectListEmails += "<option selected='' value=" + email + ">" + email + "</option>";
                }
                selectListEmails += "</select>";
            }
            if (string.IsNullOrWhiteSpace(model.name))
            {
                ViewBag.alertVisibility = "";
                ViewBag.alertMessage = "Project Name is required";
                ViewBag.selectedEmails = selectListEmails;
                return View(model);
            }
            if (db.Projects.Any(p => p.ProjectName.Equals(model.name)) && db.Projects.Where(p => p.ProjectName.Equals(model.name)).FirstOrDefault().Id != id)
            {
                ViewBag.alertVisibility = "";
                ViewBag.alertMessage = "Project Name already exists";
                ViewBag.selectedEmails = selectListEmails;
                return View(model);
            }
            if (newEmails.Count > 0)
            {
                if (newEmails.Any(e => e.Equals(MailUtility.getEmailFromId(User.Identity.GetUserId()))))
                {
                    newEmails.Remove(MailUtility.getEmailFromId(User.Identity.GetUserId()));
                }
                var badMails = MailUtility.verifyEmailList(newEmails);
                if (badMails.Count > 0)
                {
                    ViewBag.alertVisibility = "";
                    string errorList = "";
                    foreach (Tuple<string, string> m in badMails)
                    {
                        errorList += m.Item1 + " " + m.Item2 + "<br>";
                    }
                    ViewBag.alertMessage = errorList;
                    ViewBag.selectedEmails = selectListEmails;
                    return View(model);
                }
            }

            foreach (string mail in newEmails)
            {
                if(!oldEmails.Exists(e=>e.EndsWith(mail)))
                {
                    emailsToAdd.Add(mail);
                }
            }

            foreach (string mail in oldEmails)
            {
                if (!newEmails.Exists(e => e.EndsWith(mail)) && !MailUtility.getEmailFromId(User.Identity.GetUserId()).Equals(mail))
                {
                    emailsToRemove.Add(mail);
                }
            }
            
            db.Projects.Where(p => p.Id == id).FirstOrDefault().ProjectName = model.name;
            db.SaveChanges();

            foreach(string mail in emailsToRemove)
            {
                string uid = MailUtility.getIdFromEmail(mail);
                db.ProjectUser_MTM.Remove(db.ProjectUser_MTM.Where(pu => pu.UserId.Equals(uid) && pu.ProjectId == id).FirstOrDefault());
                db.SaveChanges();
            }

            foreach(string e in emailsToAdd)
            {
                ProjectUser_MTM p_mtm_ = new ProjectUser_MTM();
                p_mtm_.ProjectId = db.Projects.Where(p => p.ProjectName.Equals(model.name)).FirstOrDefault().Id;
                p_mtm_.UserId = db.AspNetUsers.Where(u => u.Email.Equals(e)).FirstOrDefault().Id;
                p_mtm_.UserRole = db.AspNetRoles.Where(r => r.Name.Equals("User")).FirstOrDefault().Id;
                p_mtm_.Confirmation = MailUtility.GenerateRandomNo().ToString(); ;
                db.ProjectUser_MTM.Add(p_mtm_);
                db.SaveChanges();
            }

            foreach (string e in newEmails)
            {
                sendProjConfirmation(model.name, e);
            }

            return RedirectToAction("Index", "Project");
        }

        // GET: Project/Delete/5
        public ActionResult Delete(int id)
        {
            ProjectManagementEntities db = new ProjectManagementEntities();

            if (!MailUtility.getProjectAdmin(db.Projects.Where(p => p.Id == id).FirstOrDefault().ProjectName).Equals(MailUtility.getEmailFromId(User.Identity.GetUserId())))
            {
                return Content("You are not allowed to delete project " + db.Projects.Where(p => p.Id == id).FirstOrDefault().ProjectName);
            }

            ProjectDetailsViewModel model = new ProjectDetailsViewModel();
            Project proj = db.Projects.Where(p => p.Id == id).FirstOrDefault();

            model.id = proj.Id;
            model.name = proj.ProjectName;
            model.admin = MailUtility.getProjectAdmin(proj.ProjectName);
            model.editable = model.admin.Equals(MailUtility.getEmailFromId(User.Identity.GetUserId()));
            model.users = MailUtility.getEmailsByProjectId(id);

            return View(model);
        }

        // POST: Project/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, ProjectDetailsViewModel model)
        {
            ProjectManagementEntities db = new ProjectManagementEntities();

            if (!MailUtility.getProjectAdmin(db.Projects.Where(p => p.Id == id).FirstOrDefault().ProjectName).Equals(MailUtility.getEmailFromId(User.Identity.GetUserId())))
            {
                return Content("You are not allowed to delete project " + db.Projects.Where(p => p.Id == id).FirstOrDefault().ProjectName);
            }

            db.Projects.Remove(db.Projects.Where(p => p.Id == id).FirstOrDefault());
            db.SaveChanges();

            return RedirectToAction("Index", "Project");
        }

        public ActionResult viewProfile(string email)
        {
            try
            {
                List<ProjectIndexViewModel> projModelList = MailUtility.getProjectsbyEmail(MailUtility.getEmailFromId(User.Identity.GetUserId()));

                AspNetUser user = MailUtility.getUserFromEmail(email);
                ViewBag.Username = user.UserName;
                ViewBag.Email = user.Email;
                ViewBag.ProjectsCount = projModelList.Count;
                return View(projModelList);
            }
            catch(Exception ex)
            {
                HandleErrorInfo error = new HandleErrorInfo(ex, "Project", "viewProfile");
                return View("Error", error);
            }
        }
    }

}
