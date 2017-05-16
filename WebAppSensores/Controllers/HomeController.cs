using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebAppSensores.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Aplicación de visualización de datos de sensores.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "ETSISI";

            return View();
        }
    }
}