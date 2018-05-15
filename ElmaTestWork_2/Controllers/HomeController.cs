using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ElmaTestWork_2.Controllers
{
    /* Проект источник
     * https://techbrij.com/crud-file-upload-asp-net-mvc-ef-multiple
     */

    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            //return View();
            if (!Request.IsAuthenticated)
                return Redirect("~/Account/Login");
            return Redirect("~/Document");
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }
    }
}