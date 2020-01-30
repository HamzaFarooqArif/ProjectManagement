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
            Config.save("Email", "devkomalshehzadi786@gmail.com");
            Config.save("Password", "komal1234");
            Config.save("Host", "smtp.gmail.com");
            Config.save("ApplicationName", "Inventify");

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