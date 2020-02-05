using ProjectManagement.Models;
using ProjectManagement.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ProjectManagement.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            //Notifications.addToNotifications(MailUtility.getCurrentEmail(), "Confirm Join", "Link to confirm join");
            //List<Notification> list = Notifications.GetNotifications(MailUtility.getCurrentEmail());
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}