using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace UAAAS.Controllers
{
    public class ContentBuilderController : BaseController
    {
        //
        // GET: /ContentBuilder/

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult DisplayPage(string page)
        {
            // TODO: Validate
            return Content(String.Concat("<html><body><p>", page, "</p></body></html>"), System.Net.Mime.MediaTypeNames.Text.Html);
        }
    }
}
