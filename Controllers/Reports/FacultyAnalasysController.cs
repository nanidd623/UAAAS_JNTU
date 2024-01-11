using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using UAAAS.Models;

namespace UAAAS.Controllers.Reports
{
    [ErrorHandling]
    public class FacultyAnalasysController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();

        public ActionResult FacultyAnalasys()
        {
            return View();
        }

    }
}
