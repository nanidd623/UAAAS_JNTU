using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace UAAAS.Controllers
{
    public class PaymentController : BaseController
    {
        //
        // GET: /Payment/

        public ActionResult TermsandConditions()
        {
            return View();
        }
        public ActionResult Disclaimer()
        {
            return View();
        }
        public ActionResult PrivacyPolicy()
        {
            return View();
        }
        public ActionResult RefundandCancellationPolicy()
        {
            return View();
        }

    }
}
