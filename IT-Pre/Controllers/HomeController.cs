using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace IT_Pre.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            //System.IO.DirectoryInfo di = System.IO.Directory.CreateDirectory(@"C:\Users\Ihor\Documents\Visual Studio 2015\Projects\it-pre\IT-Pre\Files\UploadImages\DELETE");
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