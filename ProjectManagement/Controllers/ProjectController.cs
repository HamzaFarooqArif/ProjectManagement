using ProjectManagement.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;

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
            if(item.Equals(currentEmail))
            {
                var data = new { id = 1, message = "Your email is not needed explicitly" };
                return Json(data, JsonRequestBehavior.AllowGet);
            }
            if (!users.Any(u => u.Email.Equals(item)))
            {
                var data = new { id = 2, message = "Email not found" };
                return Json(data, JsonRequestBehavior.AllowGet);
            }
            if (users.Where(u => u.Email.Equals(item)).FirstOrDefault().EmailConfirmed == false)
            {
                var data = new { id = 3, message = "Email not confirmed" };
                return Json(data, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var data = new { id = 0, message = "" };
                return Json(data, JsonRequestBehavior.AllowGet);
            }

            //ProjectManagementEntities db = new ProjectManagementEntities();
            //if(!db.AspNetUsers.Any(u=>u.Email.Equals(item)))
            //{
            //    return Json(res.Select(x => new
            //    {
            //        ID = "0",
            //        Name = "name"
            //    }));
            //}
            ////IEnumerable<GetProducts_Result> res = db.GetProducts();
            //List<string> res = new List<string>();
            //return Json(res.Select(x => new
            //{
            //    ID = "id",
            //    Name = "name"
            //}));
        }
        // GET: Project
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
            ViewBag.alertVisibility = "hidden";
            ViewBag.alertMessage = "";
            ViewBag.alertType = "danger";
            return View();
        }

        // POST: Project/Create
        [HttpPost]
        public ActionResult Create(ProjectCreateViewModel model)
        {
            ViewBag.alertVisibility = "hidden";
            ViewBag.alertMessage = "";
            ViewBag.alertType = "danger";
            try
            {
                ProjectManagementEntities db = new ProjectManagementEntities();
                List<string> emails = model.emails.Split(',').ToList();

                if (db.Projects.Any(u=>u.ProjectName.Equals(model.name)))
                {
                    ViewBag.alertVisibility = "";
                    ViewBag.alertMessage = "Project name already exists";
                    return View(model);
                }
                //db.Projects.Add(new Project { ProjectName = model.name });
                //db.SaveChanges();

                //ProjectUser_MTM p_mtm = new ProjectUser_MTM();
                //p_mtm.ProjectId = db.Projects.Where(p => p.ProjectName.Equals(model.name)).FirstOrDefault().Id;
                //p_mtm.UserId = db.AspNetUsers.Where(u => u.Id.Equals(User.Identity.GetUserId())).FirstOrDefault().Id;
                //p_mtm.UserRole = db.AspNetRoles.Where(r => r.Name.Equals("Admin")).FirstOrDefault().Id;
                //p_mtm.Confirmation = "0";
                //db.ProjectUser_MTM.Add(p_mtm);
                //db.SaveChanges();

                //foreach (string e in emails)
                //{
                //    ProjectUser_MTM p_mtm_ = new ProjectUser_MTM();
                //    p_mtm_.ProjectId = db.Projects.Where(p => p.ProjectName.Equals(model.name)).FirstOrDefault().Id;
                //    p_mtm_.UserId = db.AspNetUsers.Where(u=>u.Email.Equals(e)).FirstOrDefault().Id;
                //    p_mtm_.UserRole = db.AspNetRoles.Where(r => r.Name.Equals("User")).FirstOrDefault().Id;
                //    p_mtm_.Confirmation = "0";
                //    db.ProjectUser_MTM.Add(p_mtm_);
                //    db.SaveChanges();
                //}

                
                
                return RedirectToAction("Index","Home");
            }
            catch
            {
                return View();
            }
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
