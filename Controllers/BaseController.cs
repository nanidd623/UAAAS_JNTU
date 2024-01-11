using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using UAAAS.Models;

//using ReportManagement;

namespace UAAAS.Controllers
{
    public class BaseController : Controller //PdfViewController
    {
        private readonly uaaasDBContext _db = new uaaasDBContext();

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var siteMenuManager = new SiteMenuManager();
            ViewBag.SiteLinks = siteMenuManager.GetSitemMenuItems().ToList();
            base.OnActionExecuting(filterContext);

            //var rolename = Roles.GetRolesForUser(User.Identity.Name);
            //if (filterContext.ActionDescriptor.ActionName.Contains("LogOff"))
            //{
            //    var siteMenuManager = new SiteMenuManager();
            //    ViewBag.SiteLinks = siteMenuManager.GetSitemMenuItems().ToList();
            //    base.OnActionExecuting(filterContext);
            //}
            //else if (rolename.Contains("College"))
            //{



            //        var siteMenuManager = new SiteMenuManager();
            //        var result = new ViewResult();
            //        result.ViewBag.SiteLinks = siteMenuManager.GetSitemMenuItems().ToList();
            //        filterContext.Result = result;

            //        filterContext.RequestContext.RouteData.Values["controller"] = "CollegeSCMProceedingsRequest";
            //        filterContext.RequestContext.RouteData.Values["action"] = "CollegeScmProceedingsPrincipalRequest";

            //        //filterContext.RequestContext.RouteData.Values["controller"] = "OnlineRegistration";
            //        //filterContext.RequestContext.RouteData.Values["action"] = "College";
            //        base.OnActionExecuting(filterContext);









            //}
            //else if (User.Identity.Name == "cstl")
            //{
            //    var siteMenuManager = new SiteMenuManager();
            //    ViewBag.SiteLinks = siteMenuManager.GetSitemMenuItems().ToList();
            //    base.OnActionExecuting(filterContext);
            //}
            //else
            //{
            //    var siteMenuManager = new SiteMenuManager();
            //    ViewBag.SiteLinks = siteMenuManager.GetSitemMenuItems().ToList();
            //    base.OnActionExecuting(filterContext);
            //}





        }

        protected override void OnResultExecuting(ResultExecutingContext filterContext)
        {
            var siteMenuManager = new SiteMenuManager();
            //  ViewBag.SiteLinks = siteMenuManager.GetSitemMenuItems().ToList();
            base.OnResultExecuting(filterContext);
        }

        public int GetPageEditableStatus(int userCollegeId)
        {
            var actualYear = _db.jntuh_academic_year.Where(a => a.isActive && a.isPresentAcademicYear).Select(a => a.actualYear).FirstOrDefault();
            var prAy = _db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
            var todayDate = DateTime.Now.Date;
            var status = _db.jntuh_pa_college_edit_status.Where(editStatus => editStatus.academicyearId == prAy && editStatus.collegeId == userCollegeId && editStatus.IsCollegeEditable &&
                                  editStatus.editFromDate <= todayDate && editStatus.editToDate >= todayDate).Select(editStatus => editStatus.id).FirstOrDefault();
            return status;
        }

        public int GetOnlineAppPageEditableStatus(int userCollegeId)
        {
            var actualYear = _db.jntuh_academic_year.Where(a => a.isActive && a.isPresentAcademicYear).Select(a => a.actualYear).FirstOrDefault();
            var prAy = _db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
            var todayDate = DateTime.Now.Date;
            var status = _db.jntuh_college_edit_status.Where(editStatus => editStatus.academicyearId == prAy && editStatus.collegeId == userCollegeId && editStatus.IsCollegeEditable &&
                                  editStatus.editFromDate <= todayDate && editStatus.editToDate >= todayDate).Select(editStatus => editStatus.id).FirstOrDefault();
            return status;
        }
    }
}