using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using UAAAS.Models;
using System.Web.Security;
using System.Data;
using System.Web.Configuration;

namespace UAAAS.Controllers
{
    [ErrorHandling]
    public class CollegeComputerLabController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();
       

        public ActionResult Index()
        {
            return View();
        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult Create(string collegeId)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 0)
            {
                if (Roles.IsUserInRole("Admin") || Roles.IsUserInRole("DataEntry"))
                {
                    if (collegeId != null)
                    {
                        userCollegeID = Convert.ToInt32(Utilities.DecryptString(collegeId, WebConfigurationManager.AppSettings["CryptoKey"]));
                    }
                }
            }

            if (userCollegeID == 0 && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("Create", "CollegeInformation");
            }
            if (userCollegeID == 0 && (Roles.IsUserInRole("Admin") || Roles.IsUserInRole("DataEntry")))
            {
                return RedirectToAction("Create", "CollegeInformation", new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
            }
            ViewBag.collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString();
            int computerLabId = db.jntuh_college_computer_lab.Where(c => c.collegeId == userCollegeID).Select(c => c.id).FirstOrDefault();

            DateTime todayDate = DateTime.Now.Date;
            int status = db.jntuh_college_edit_status.Where(editStatus => editStatus.collegeId == userCollegeID &&
                                                                                     editStatus.IsCollegeEditable == true &&
                                                                                     editStatus.editFromDate <= todayDate &&
                                                                                     editStatus.editToDate >= todayDate)
                                                                .Select(editStatus => editStatus.id)
                                                                .FirstOrDefault();

            if (computerLabId == 0 && status == 0 && Roles.IsUserInRole("College"))
            {
                ViewBag.NotUpload = true;
            }
            else
            {
                ViewBag.NotUpload = false;
            }

            if (computerLabId > 0 && userCollegeID > 0 && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("View", "CollegeComputerLab");
            }
            if (computerLabId > 0 && userCollegeID > 0 && (Roles.IsUserInRole("Admin") || Roles.IsUserInRole("DataEntry")))
            {
                return RedirectToAction("Edit", "CollegeComputerLab", new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
            }
            
            CollegeComputerLab collegeComputerLab = new CollegeComputerLab();
            collegeComputerLab.collegeId = userCollegeID;
            return View("Create", collegeComputerLab);
        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        [HttpPost]
        public ActionResult Create(CollegeComputerLab collegeComputerLab)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 0)
            {
                userCollegeID = collegeComputerLab.collegeId;
            }
            SaveCollegeComputerlab(collegeComputerLab);
            return View(collegeComputerLab);
        }

        private void SaveCollegeComputerlab(CollegeComputerLab collegeComputerLab)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 0)
            {
                userCollegeID = collegeComputerLab.collegeId;
            }
            if (ModelState.IsValid)
            {
                jntuh_college_computer_lab labDetails = new jntuh_college_computer_lab();
                labDetails.id = collegeComputerLab.id;
                labDetails.collegeId = userCollegeID;
                labDetails.printersAvailability = collegeComputerLab.printersAvailability;
                labDetails.labWorkingHoursFrom = TimeSpan.Parse(collegeComputerLab.workingHoursofComputerLabFrom);
                labDetails.labWorkingHoursTo = TimeSpan.Parse(collegeComputerLab.workingHoursofComputerLabTo);
                labDetails.internetAccessibilityFrom = TimeSpan.Parse(collegeComputerLab.internetAccessibilityFrom);
                labDetails.internetAccessibilityTo = TimeSpan.Parse(collegeComputerLab.internetAccessibilityTo);

                int labId = db.jntuh_college_computer_lab.Where(c => c.collegeId == userCollegeID).Select(c => c.id).FirstOrDefault();
                if (labId == 0)
                {
                    labDetails.createdBy = userID;
                    labDetails.createdOn = DateTime.Now;
                    db.jntuh_college_computer_lab.Add(labDetails);
                    db.SaveChanges();
                    TempData["Success"] = "College Computer lab Details are Added successfully";
                }
                else
                {
                    labDetails.id = labId;
                    labDetails.createdOn = db.jntuh_college_computer_lab.Where(c => c.id == labId).Select(c => c.createdOn).FirstOrDefault();
                    labDetails.createdBy = db.jntuh_college_computer_lab.Where(c => c.id == labId).Select(c => c.createdBy).FirstOrDefault();
                    labDetails.updatedBy = userID;
                    labDetails.updatedOn = DateTime.Now;
                    db.Entry(labDetails).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["Success"] = "College Computer lab Details are Updated successfully";
                }
            }
        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult Edit(string collegeId)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 0)
            {
                if (Roles.IsUserInRole("Admin") || Roles.IsUserInRole("DataEntry"))
                {
                    if (collegeId != null)
                    {
                        userCollegeID = Convert.ToInt32(Utilities.DecryptString(collegeId, WebConfigurationManager.AppSettings["CryptoKey"]));
                    }
                }
            }
            int labId = db.jntuh_college_computer_lab.Where(c => c.collegeId == userCollegeID).Select(c => c.id).FirstOrDefault();
            if (userCollegeID == 0 && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("Create", "CollegeInformation");
            }
            if (labId == 0 && Roles.IsUserInRole("College"))            
            {
                return RedirectToAction("Create", "CollegeComputerLab");
            }

            DateTime todayDate = DateTime.Now.Date;
            int status = db.jntuh_college_edit_status.Where(editStatus => editStatus.collegeId == userCollegeID &&
                                                                                     editStatus.IsCollegeEditable == true &&
                                                                                     editStatus.editFromDate <= todayDate &&
                                                                                     editStatus.editToDate >= todayDate)
                                                                .Select(editStatus => editStatus.id)
                                                                .FirstOrDefault();
            if (status == 0 && Roles.IsUserInRole("College"))           
            {
                ViewBag.IsEditable = false;
                return RedirectToAction("View", "CollegeComputerLab");
            }
            else
            {
                ViewBag.IsEditable = true;
            }

            CollegeComputerLab collegeComputerLab = new CollegeComputerLab();
            collegeComputerLab.collegeId = userCollegeID;
            jntuh_college_computer_lab labDetails = db.jntuh_college_computer_lab.Find(labId);
            if (labDetails != null)
            {
                collegeComputerLab.id = labDetails.id;
                collegeComputerLab.collegeId = labDetails.collegeId;
                collegeComputerLab.printersAvailability = labDetails.printersAvailability;
                collegeComputerLab.workingHoursofComputerLabFrom = labDetails.labWorkingHoursFrom.ToString(@"hh\:mm");
                collegeComputerLab.workingHoursofComputerLabTo = labDetails.labWorkingHoursTo.ToString(@"hh\:mm");
                collegeComputerLab.internetAccessibilityFrom = labDetails.internetAccessibilityFrom.ToString(@"hh\:mm");
                collegeComputerLab.internetAccessibilityTo = labDetails.internetAccessibilityTo.ToString(@"hh\:mm");
                collegeComputerLab.createdBy = labDetails.createdBy;
                collegeComputerLab.createdOn = labDetails.createdOn;
                collegeComputerLab.updatedBy = labDetails.updatedBy;
                collegeComputerLab.updatedOn = labDetails.updatedOn;
            }

            return View("Create",collegeComputerLab);
        }


        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        [HttpPost]
        public ActionResult Edit(CollegeComputerLab collegeComputerLab)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 0)
            {
                userCollegeID = collegeComputerLab.collegeId;
            }
            SaveCollegeComputerlab(collegeComputerLab);
            return View("Create",collegeComputerLab);
        }
        [Authorize(Roles = "Admin,College")]
        public ActionResult View(string id)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 0)
            {
                userCollegeID = userCollegeID = Convert.ToInt32(Utilities.DecryptString(id, WebConfigurationManager.AppSettings["CryptoKey"]));
            }
            int collegeLabId = db.jntuh_college_computer_lab.Where(l => l.collegeId == userCollegeID).Select(l => l.id).FirstOrDefault();

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
            CollegeComputerLab collegeComputerLab = new CollegeComputerLab();
            if (collegeLabId == 0)
            {
                ViewBag.Norecords = true;
            }
            else
            {
                ViewBag.Norecords = false;
                jntuh_college_computer_lab labDetails = db.jntuh_college_computer_lab.Find(collegeLabId);
                if (labDetails != null)
                {
                    collegeComputerLab.id = labDetails.id;
                    collegeComputerLab.collegeId = labDetails.collegeId;
                    collegeComputerLab.printersAvailability = labDetails.printersAvailability;
                    collegeComputerLab.workingHoursofComputerLabFrom = labDetails.labWorkingHoursFrom.ToString(@"hh\:mm");
                    collegeComputerLab.workingHoursofComputerLabTo = labDetails.labWorkingHoursTo.ToString(@"hh\:mm");
                    collegeComputerLab.internetAccessibilityFrom = labDetails.internetAccessibilityFrom.ToString(@"hh\:mm");
                    collegeComputerLab.internetAccessibilityTo = labDetails.internetAccessibilityTo.ToString(@"hh\:mm");
                }
            }
            return View("View", collegeComputerLab);
        }        
    }
}
