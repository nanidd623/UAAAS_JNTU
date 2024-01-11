using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Security;
using UAAAS.Models;

namespace UAAAS.Controllers
{

    [ErrorHandling]
    public class PrintAllScreensController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();
        //
        // GET: /PrintAllScreens/

        [Authorize(Roles = "Admin,College")]
        public ActionResult Index(string id)
        {
       
       
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 0)
            {
                userCollegeID = userCollegeID = Convert.ToInt32(Utilities.DecryptString(id, WebConfigurationManager.AppSettings["CryptoKey"]));          
            }
            int[] degree = db.jntuh_college_degree.Where(Degree => Degree.collegeId == userCollegeID && Degree.isActive == true).Select(Degree => Degree.degreeId).ToArray();
            int ExistId = db.jntuh_college_computer_lab_printers.Where(a => a.collegeId == userCollegeID && degree.Contains(a.degreeId)).Select(a => a.id).FirstOrDefault();
            DateTime todayDate = DateTime.Now.Date;
            int status = db.jntuh_college_edit_status.Where(editStatus => editStatus.collegeId == userCollegeID &&
                                                                          editStatus.IsCollegeEditable == true &&
                                                                          editStatus.editFromDate <= todayDate &&
                                                                          editStatus.editToDate >= todayDate)
                                                     .Select(editStatus => editStatus.id)
                                                     .FirstOrDefault();
            if (status > 0 && Roles.IsUserInRole("College"))
            {
                ViewBag.IsEditable = true;
            }
            else
            {
                ViewBag.IsEditable = false;
            }

            List<CollegePrinterDetails> PrinterDetails = (from CollegeDegree in db.jntuh_college_degree
                                                          join Degree in db.jntuh_degree on CollegeDegree.degreeId equals Degree.id
                                                          orderby Degree.degree
                                                          where (CollegeDegree.collegeId == userCollegeID && CollegeDegree.isActive == true)
                                                          select new CollegePrinterDetails
                                                          {
                                                              degreeId = CollegeDegree.degreeId,
                                                              degree = Degree.degree,
                                                              availableComputers = 0,
                                                              availablePrinters = 0
                                                          }).ToList();
            foreach (var item in PrinterDetails)
            {
                item.availableComputers = db.jntuh_college_computer_student_ratio.Where(availableComputers => availableComputers.collegeId == userCollegeID &&
                                                                                        availableComputers.degreeId == item.degreeId)
                                                                                 .Select(availableComputers => availableComputers.availableComputers)
                                                                                 .FirstOrDefault();
                item.availablePrinters = db.jntuh_college_computer_lab_printers.Where(availablePrinters => availablePrinters.collegeId == userCollegeID &&
                                                                                      availablePrinters.degreeId == item.degreeId)
                                                                               .Select(availablePrinters => availablePrinters.availablePrinters)
                                                                               .FirstOrDefault();
            }
            if (ExistId == 0)
            {
                ViewBag.Norecords = true;
            }
            else
            {
                ViewBag.Update = true;
                ViewBag.Count = PrinterDetails.Count();
            }
            // return View("View", PrinterDetails);  return RedirectToAction("View", "CollegeInformation");

            return View("Index", PrinterDetails);
        }

    }
}
