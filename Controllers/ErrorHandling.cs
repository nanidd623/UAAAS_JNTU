using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using UAAAS.Models;

namespace UAAAS.Controllers
{
    public class ErrorHandlingAttribute : HandleErrorAttribute
    {
        private uaaasDBContext db = new uaaasDBContext();

        public override void OnException(ExceptionContext context)
        {
            base.OnException(context);
            LogException(context);
        }

        private void LogException(ExceptionContext context)
        {
            jntuh_error_log jntuh_error_log = new jntuh_error_log();
            jntuh_error_log.exception = context.Exception.Message;
            jntuh_error_log.controller = context.RouteData.Values["controller"].ToString();
            jntuh_error_log.action = context.RouteData.Values["action"].ToString();
            jntuh_error_log.StackTrace = context.Exception.StackTrace;
            if (context.Exception.InnerException != null)
            {
                jntuh_error_log.innerException = context.Exception.InnerException.Message;
            }
            jntuh_error_log.createdOn = DateTime.Now;
            if (!HttpContext.Current.User.Identity.Name.Equals(string.Empty))
                jntuh_error_log.createdBy = Convert.ToInt32(Membership.GetUser(HttpContext.Current.User.Identity.Name).ProviderUserKey);
            db.jntuh_error_log.Add(jntuh_error_log);
            db.SaveChanges();
            //throw new NotImplementedException();
        }
    }
}