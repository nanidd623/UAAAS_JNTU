using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Data;
using UAAAS.Models;
using System.Web.Configuration;

namespace UAAAS.Controllers
{
    [ErrorHandling]
    public class CollegeExpenditureController : BaseController
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
            int userCollegeID = db.jntuh_college_users.Where(collegeUser => collegeUser.userID == userID).Select(collegeUser => collegeUser.collegeID).FirstOrDefault();
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
            int collegeExpenditureId = db.jntuh_college_expenditure.Where(expenditure => expenditure.collegeId == userCollegeID).Select(expenditure => expenditure.id).FirstOrDefault();

            DateTime todayDate = DateTime.Now.Date;
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int prAy = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
            int status = db.jntuh_college_edit_status.Where(editStatus => editStatus.academicyearId == prAy && editStatus.collegeId == userCollegeID &&
                                                                                      editStatus.IsCollegeEditable == true &&
                                                                                      editStatus.editFromDate <= todayDate &&
                                                                                      editStatus.editToDate >= todayDate)
                                                                 .Select(editStatus => editStatus.id)
                                                                 .FirstOrDefault();

            if (userCollegeID > 0 && collegeExpenditureId > 0 && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("View", "CollegeExpenditure");
            }
            if (userCollegeID > 0 && collegeExpenditureId > 0 && (Roles.IsUserInRole("Admin") || Roles.IsUserInRole("DataEntry")))
            {
                return RedirectToAction("Edit", "CollegeExpenditure", new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
            }

            if (collegeExpenditureId == 0 && status == 0 && Roles.IsUserInRole("College"))
            {
                ViewBag.NotUpload = true;
            }
            else
            {
                ViewBag.NotUpload = false;
            }

            bool isPageEditable = db.jntuh_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("EX") && a.CollegeId == userCollegeID).Select(a => a.IsEditable).FirstOrDefault();

            if (isPageEditable)
            {
                ViewBag.IsEditable = true;
            }
            else
            {
                ViewBag.IsEditable = false;
                return RedirectToAction("View", "CollegeExpenditure");
            }

            List<CollegeExpenditure> collegeExpenditure = db.jntuh_college_expenditure_type.Where(expenditure => expenditure.isActive == true).Select(expenditure => new CollegeExpenditure
            {
                expenditureTypeID = expenditure.id,
                expenditure = expenditure.expenditure,
                expenditureAmount = 0,
                collegeId = userCollegeID
            }).ToList();
            ViewBag.Count = collegeExpenditure.Count();
            ViewBag.Update = false;
            return View(collegeExpenditure);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult Create(ICollection<CollegeExpenditure> collegeExpenditure)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(collegeUser => collegeUser.userID == userID).Select(collegeUser => collegeUser.collegeID).FirstOrDefault();
            if (userCollegeID == 0)
            {
                foreach (var item in collegeExpenditure)
                {
                    userCollegeID = item.collegeId;
                }
            }
            SavecollegeExpenditure(collegeExpenditure);

            List<CollegeExpenditure> expenditure = db.jntuh_college_expenditure_type.Where(expenditureType => expenditureType.isActive == true).Select(expenditureType => new CollegeExpenditure
            {
                expenditureTypeID = expenditureType.id,
                expenditure = expenditureType.expenditure,
                expenditureAmount = 0,
                collegeId = userCollegeID
            }).ToList();
            foreach (var item in expenditure)
            {
                item.expenditureAmount = db.jntuh_college_expenditure.Where(e => e.collegeId == userCollegeID && e.expenditureTypeID == item.expenditureTypeID).Select(e => e.expenditureAmount).FirstOrDefault();
            }
            ViewBag.Count = expenditure.Count();
            return View("View");
            //return View(expenditure);
        }


        private void SavecollegeExpenditure(ICollection<CollegeExpenditure> collegeExpenditure)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(collegeUser => collegeUser.userID == userID).Select(collegeUser => collegeUser.collegeID).FirstOrDefault();
            if (userCollegeID == 0)
            {
                foreach (var item in collegeExpenditure)
                {
                    userCollegeID = item.collegeId;
                }
            }
            var errorMessage = string.Empty;
            if (ModelState.IsValid)
            {
                foreach (CollegeExpenditure item in collegeExpenditure)
                {
                    var expenditureDetails = new jntuh_college_expenditure
                    {
                        expenditureTypeID = item.expenditureTypeID,
                        expenditureAmount = item.expenditureAmount,
                        collegeId = userCollegeID
                    };

                    if (item.expenditureTypeID == 8)
                    {
                        expenditureDetails.expenditureAmount = (item.expenditureStatus
                            ? (item.payScaleStatus ? 6 : 7)
                            : 0);
                        //if (item.expenditureStatus)
                        //{
                        //    expenditureDetails.expenditureAmount = item.payScaleStatus ? 6 : 7;
                        //}
                        //else
                        //{
                        //    expenditureDetails.expenditureAmount = 0;
                        //}
                    }



                    var expenditureId = db.jntuh_college_expenditure.Where(expenditure => expenditure.collegeId == userCollegeID && expenditure.expenditureTypeID == item.expenditureTypeID).Select(expenditure => expenditure.id).FirstOrDefault();
                    if (expenditureId == 0)
                    {

                        if ((item.expenditureTypeID != 0))
                        {
                            expenditureDetails.createdBy = userID;
                            expenditureDetails.createdOn = DateTime.Now;
                            db.jntuh_college_expenditure.Add(expenditureDetails);
                            db.SaveChanges();
                            errorMessage = "Save";
                        }
                    }
                    else
                    {
                        expenditureDetails.id = expenditureId;
                        expenditureDetails.createdBy = db.jntuh_college_expenditure.Where(e => e.id == expenditureId).Select(e => e.createdBy).FirstOrDefault();
                        expenditureDetails.createdOn = db.jntuh_college_expenditure.Where(e => e.id == expenditureId).Select(e => e.createdOn).FirstOrDefault();
                        expenditureDetails.updatedBy = userID;
                        expenditureDetails.updatedOn = DateTime.Now;
                        db.Entry(expenditureDetails).State = EntityState.Modified;
                        db.SaveChanges();
                        errorMessage = "Update";
                    }
                }
            }
            if (errorMessage == "Update")
            {
                TempData["Success"] = "College Expenditure details are Updated Successfully.";
            }
            else
            {
                TempData["Success"] = "College Expenditure details are Added Successfully.";
            }
        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult Edit(string collegeId)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(collegeUser => collegeUser.userID == userID).Select(collegeUser => collegeUser.collegeID).FirstOrDefault();
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
            int collegeExpenditureId = db.jntuh_college_expenditure.Where(expenditure => expenditure.collegeId == userCollegeID).Select(expenditure => expenditure.id).FirstOrDefault();

            if (userCollegeID == 0 && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("Create", "CollegeInformation");
            }

            if (collegeExpenditureId == 0 && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("Create", "CollegeExpenditure");
            }
            DateTime todayDate = DateTime.Now.Date;
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int prAy = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
            int status = db.jntuh_college_edit_status.Where(editStatus => editStatus.academicyearId == prAy && editStatus.collegeId == userCollegeID &&
                                                                                      editStatus.IsCollegeEditable == true &&
                                                                                      editStatus.editFromDate <= todayDate &&
                                                                                      editStatus.editToDate >= todayDate)
                                                                 .Select(editStatus => editStatus.id)
                                                                 .FirstOrDefault();
            if (status == 0 && Roles.IsUserInRole("College"))
            {
                ViewBag.IsEditable = false;
                return RedirectToAction("View", "CollegeExpenditure");
            }
            else
            {
                bool isPageEditable = db.jntuh_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("EX") && a.CollegeId == userCollegeID).Select(a => a.IsEditable).FirstOrDefault();

                if (isPageEditable)
                {
                    ViewBag.IsEditable = true;
                }
                else
                {
                    ViewBag.IsEditable = false;
                    return RedirectToAction("View", "CollegeExpenditure");
                }
            }

            List<CollegeExpenditure> collegeExpenditure = db.jntuh_college_expenditure_type.Where(expenditureType => expenditureType.isActive == true).Select(expenditureType => new CollegeExpenditure
            {
                expenditureTypeID = expenditureType.id,
                expenditure = expenditureType.expenditure,
                expenditureAmount = 0,
                collegeId = userCollegeID,
            }).ToList();
            foreach (var item in collegeExpenditure)
            {
                item.expenditureAmount = db.jntuh_college_expenditure.Where(e => e.collegeId == userCollegeID && e.expenditureTypeID == item.expenditureTypeID).Select(e => e.expenditureAmount).FirstOrDefault();
                if (item.expenditureTypeID == 8 && item.expenditureAmount > 0)
                {
                    item.expenditureStatus = true;
                    item.payScaleStatus = item.expenditureAmount == 6;
                }
                else if (item.expenditureTypeID == 8 && item.expenditureAmount == 0)
                {
                    item.expenditureStatus = false;
                    item.payScaleStatus = false;
                }
            }
            ViewBag.Update = true;
            ViewBag.Count = collegeExpenditure.Count();
            return View("Create", collegeExpenditure);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult Edit(ICollection<CollegeExpenditure> collegeExpenditure)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(collegeUser => collegeUser.userID == userID).Select(collegeUser => collegeUser.collegeID).FirstOrDefault();
            if (userCollegeID == 0)
            {
                foreach (var item in collegeExpenditure)
                {
                    userCollegeID = item.collegeId;
                }
            }
            SavecollegeExpenditure(collegeExpenditure);

            List<CollegeExpenditure> expenditure = db.jntuh_college_expenditure_type.Where(expenditureType => expenditureType.isActive == true).Select(expenditureType => new CollegeExpenditure
            {
                expenditureTypeID = expenditureType.id,
                expenditure = expenditureType.expenditure,
                expenditureAmount = 0,
                collegeId = userCollegeID
            }).ToList();
            foreach (var item in expenditure)
            {
                item.expenditureAmount = db.jntuh_college_expenditure.Where(e => e.collegeId == userCollegeID && e.expenditureTypeID == item.expenditureTypeID).Select(e => e.expenditureAmount).FirstOrDefault();
            }
            ViewBag.Count = expenditure.Count();
            ViewBag.Update = true;

            // Changed by Naushad Khan
            return View("View");
            //return View("Create", expenditure);
        }

        [Authorize(Roles = "Admin,College")]
        public ActionResult View(string id)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(collegeUser => collegeUser.userID == userID).Select(collegeUser => collegeUser.collegeID).FirstOrDefault();
            if (userCollegeID == 0)
            {
                userCollegeID = userCollegeID = Convert.ToInt32(Utilities.DecryptString(id, WebConfigurationManager.AppSettings["CryptoKey"]));
            }
            int collegeExpenditureId = db.jntuh_college_expenditure.Where(expenditure => expenditure.collegeId == userCollegeID).Select(expenditure => expenditure.id).FirstOrDefault();



            DateTime todayDate = DateTime.Now.Date;
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int prAy = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
            int status = db.jntuh_college_edit_status.Where(editStatus => editStatus.academicyearId == prAy && editStatus.collegeId == userCollegeID &&
                                                                                      editStatus.IsCollegeEditable == true &&
                                                                                      editStatus.editFromDate <= todayDate &&
                                                                                      editStatus.editToDate >= todayDate)
                                                                 .Select(editStatus => editStatus.id)
                                                                 .FirstOrDefault();
            if (status > 0 && Roles.IsUserInRole("College"))
            {
                bool isPageEditable = db.jntuh_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("EX") && a.CollegeId == userCollegeID).Select(a => a.IsEditable).FirstOrDefault();

                if (isPageEditable)
                {
                    ViewBag.IsEditable = true;
                }
                else
                {
                    ViewBag.IsEditable = false;
                }
            }
            else
            {
                ViewBag.IsEditable = false;
            }

            List<CollegeExpenditure> collegeExpenditure = db.jntuh_college_expenditure_type.Where(expenditureType => expenditureType.isActive == true).Select(expenditureType => new CollegeExpenditure
           {
               expenditureTypeID = expenditureType.id,
               expenditure = expenditureType.expenditure,
               expenditureAmount = 0
           }).ToList();

            if (collegeExpenditureId == 0)
            {
                ViewBag.Norecords = true;
            }
            else
            {

                foreach (var item in collegeExpenditure)
                {
                    item.expenditureAmount = db.jntuh_college_expenditure.Where(e => e.collegeId == userCollegeID && e.expenditureTypeID == item.expenditureTypeID).Select(e => e.expenditureAmount).FirstOrDefault();
                }
                ViewBag.Update = true;
            }
            ViewBag.Count = collegeExpenditure.Count();
            return View("View", collegeExpenditure);
        }
    }
}
