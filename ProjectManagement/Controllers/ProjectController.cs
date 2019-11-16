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
            return View();
        }
        public ActionResult Index()
        {
            return View();
        }

        // GET: Project/Details/5
        public ActionResult Details(int id)
        {
            return View();
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

            string a = Request.RawUrl;


            p_mtm.Confirmation = "0";

            db.ProjectUser_MTM.Add(p_mtm);
            db.SaveChanges();

            foreach (string e in emails)
            {
                ProjectUser_MTM p_mtm_ = new ProjectUser_MTM();
                p_mtm_.ProjectId = db.Projects.Where(p => p.ProjectName.Equals(model.name)).FirstOrDefault().Id;
                p_mtm_.UserId = db.AspNetUsers.Where(u => u.Email.Equals(e)).FirstOrDefault().Id;
                p_mtm_.UserRole = db.AspNetRoles.Where(r => r.Name.Equals("User")).FirstOrDefault().Id;
                p_mtm_.Confirmation = "0";
                db.ProjectUser_MTM.Add(p_mtm_);
                db.SaveChanges();
            }
            return RedirectToAction("Index", "Home");
        }

        // GET: Project/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: Project/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: Project/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Project/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
    }
}
