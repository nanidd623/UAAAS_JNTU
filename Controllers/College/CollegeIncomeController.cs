using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Security;
using UAAAS.Models;

namespace UAAAS.Controllers
{
    [ErrorHandling]
    public class CollegeIncomeController : BaseController
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
            int collegeIncomeId = db.jntuh_college_income.Where(a => a.collegeId == userCollegeID).Select(a => a.id).FirstOrDefault();

            DateTime todayDate = DateTime.Now.Date;
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int prAy = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
            int status = db.jntuh_college_edit_status.Where(editStatus => editStatus.academicyearId == prAy && editStatus.collegeId == userCollegeID &&
                                                                                      editStatus.IsCollegeEditable == true &&
                                                                                      editStatus.editFromDate <= todayDate &&
                                                                                      editStatus.editToDate >= todayDate)
                                                                 .Select(editStatus => editStatus.id)
                                                                 .FirstOrDefault();

            if (userCollegeID > 0 && collegeIncomeId > 0 && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("View", "CollegeIncome");
            }
            if (userCollegeID > 0 && collegeIncomeId > 0 && (Roles.IsUserInRole("Admin") || Roles.IsUserInRole("DataEntry")))
            {
                return RedirectToAction("Edit", "CollegeIncome", new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
            }

            if (collegeIncomeId == 0 && status == 0 && Roles.IsUserInRole("College"))
            {
                ViewBag.NotUpload = true;
            }
            else
            {
                ViewBag.NotUpload = false;
            }

            bool isPageEditable = db.jntuh_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("IN") && a.CollegeId == userCollegeID).Select(a => a.IsEditable).FirstOrDefault();

            if (isPageEditable)
            {
                ViewBag.IsEditable = true;
            }
            else
            {
                ViewBag.IsEditable = false;
                return RedirectToAction("View", "CollegeIncome");
            }

            List<CollegeIncome> income = db.jntuh_college_income_type.Where(i => i.isActive == true)
                                                                     .Select(s => new CollegeIncome
                                                                     {
                                                                         collegeId = userCollegeID,
                                                                         incomeTypeID = s.id,
                                                                         incomeType = s.sourceOfIncome
                                                                     }).ToList();
            ViewBag.Count = income.Count();
            return View(income);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult Create(ICollection<CollegeIncome> collegeIncome)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 0)
            {
                foreach (var item in collegeIncome)
                {
                    userCollegeID = item.collegeId;
                }
            }
            SaveArea(collegeIncome);
            TempData["Success"] = "Added successfully";

            List<CollegeIncome> incomeType = db.jntuh_college_income_type.Where(income => income.isActive == true)
                                                                         .Select(income => new CollegeIncome
                                                                         {
                                                                             incomeTypeID = income.id,
                                                                             incomeType = income.sourceOfIncome,
                                                                             incomeAmount = 0,
                                                                             collegeId = userCollegeID
                                                                         }).ToList();
            foreach (var item in incomeType)
            {
                item.incomeAmount = db.jntuh_college_income.Where(collegeIncomeType => collegeIncomeType.collegeId == userCollegeID &&
                                                                collegeIncomeType.incomeTypeID == item.incomeTypeID)
                                                         .Select(collegeIncomeType => collegeIncomeType.incomeAmount).FirstOrDefault();
                if (item.incomeAmount == 0)
                {
                    ViewBag.incomeAmount = true;
                }

            }
            ViewBag.Count = incomeType.Count();
            return View(incomeType);
        }


        private void SaveArea(ICollection<CollegeIncome> collgeIncome)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 0)
            {
                foreach (var item in collgeIncome)
                {
                    userCollegeID = item.collegeId;
                }
            }
            if (ModelState.IsValid)
            {
                foreach (CollegeIncome item in collgeIncome)
                {
                    jntuh_college_income income = new jntuh_college_income();
                    income.collegeId = userCollegeID;
                    income.incomeTypeID = item.incomeTypeID;
                    income.incomeAmount = item.incomeAmount;

                    int incomeID = db.jntuh_college_income.Where(a => a.collegeId == userCollegeID && a.incomeTypeID == item.incomeTypeID).Select(a => a.id).FirstOrDefault();
                    if (incomeID == 0)
                    {
                        income.createdBy = userID;
                        income.createdOn = DateTime.Now;

                        if ((item.incomeTypeID != 0))
                        {
                            db.jntuh_college_income.Add(income);
                            db.SaveChanges();
                        }
                    }
                    else
                    {
                        income.id = incomeID;
                        income.createdBy = db.jntuh_college_income.Where(incomeTypeId => incomeTypeId.id == incomeID).Select(incomeTypeId => incomeTypeId.createdBy).FirstOrDefault();
                        income.createdOn = db.jntuh_college_income.Where(incomeTypeId => incomeTypeId.id == incomeID).Select(incomeTypeId => incomeTypeId.createdOn).FirstOrDefault();
                        income.updatedBy = userID;
                        income.updatedOn = DateTime.Now;
                        db.Entry(income).State = EntityState.Modified;
                        db.SaveChanges();
                    }
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
            int collegeIncomeId = db.jntuh_college_income.Where(a => a.collegeId == userCollegeID).Select(a => a.id).FirstOrDefault();

            if (userCollegeID == 0 && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("Create", "CollegeInformation");
            }

            if (collegeIncomeId == 0 && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("Create", "CollegeIncome");
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
                return RedirectToAction("View", "CollegeIncome");
            }
            else
            {
                bool isPageEditable = db.jntuh_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("IN") && a.CollegeId == userCollegeID).Select(a => a.IsEditable).FirstOrDefault();

                if (isPageEditable)
                {
                    ViewBag.IsEditable = true;
                }
                else
                {
                    ViewBag.IsEditable = false;
                    return RedirectToAction("View", "CollegeIncome");
                }
               
            }

            List<CollegeIncome> incomeType = db.jntuh_college_income_type.Where(income => income.isActive == true)
                                                                         .Select(income => new CollegeIncome
                                                                         {
                                                                             incomeTypeID = income.id,
                                                                             incomeType = income.sourceOfIncome,
                                                                             incomeAmount = 0,
                                                                             collegeId = userCollegeID
                                                                         }).ToList();
            foreach (var item in incomeType)
            {
                item.incomeAmount = db.jntuh_college_income.Where(collegeIncome => collegeIncome.collegeId == userCollegeID &&
                                                                collegeIncome.incomeTypeID == item.incomeTypeID)
                                                         .Select(collegeIncome => collegeIncome.incomeAmount).FirstOrDefault();
                if (item.incomeAmount == 0)
                {
                    ViewBag.incomeAmount = true;
                }

            }
            ViewBag.Count = incomeType.Count();
            return View("Create", incomeType);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult Edit(ICollection<CollegeIncome> collgeIncome)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 0)
            {
                foreach (var item in collgeIncome)
                {
                    userCollegeID = item.collegeId;
                }
            }
            SaveArea(collgeIncome);
            TempData["Success"] = "Updated successfully";

            List<CollegeIncome> incomeType = db.jntuh_college_income_type.Where(income => income.isActive == true)
                                                                         .Select(income => new CollegeIncome
                                                                         {
                                                                             incomeTypeID = income.id,
                                                                             incomeType = income.sourceOfIncome,
                                                                             incomeAmount = 0,
                                                                             collegeId = userCollegeID
                                                                         }).ToList();
            foreach (var item in incomeType)
            {
                item.incomeAmount = db.jntuh_college_income.Where(collegeIncome => collegeIncome.collegeId == userCollegeID &&
                                                                collegeIncome.incomeTypeID == item.incomeTypeID)
                                                         .Select(collegeIncome => collegeIncome.incomeAmount).FirstOrDefault();
                if (item.incomeAmount == 0)
                {
                    ViewBag.incomeAmount = true;
                }

            }
            ViewBag.Count = incomeType.Count();
            
            // changed by Naushad Khan 
            return View("View");
            //return View("Create", incomeType);

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
            int collegeIncomeId = db.jntuh_college_income.Where(a => a.collegeId == userCollegeID).Select(a => a.id).FirstOrDefault();



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
                bool isPageEditable = db.jntuh_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("IN") && a.CollegeId == userCollegeID).Select(a => a.IsEditable).FirstOrDefault();

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

            List<CollegeIncome> incomeType = db.jntuh_college_income_type.Where(income => income.isActive == true)
                                                                             .Select(income => new CollegeIncome
                                                                             {
                                                                                 incomeTypeID = income.id,
                                                                                 incomeType = income.sourceOfIncome,
                                                                                 incomeAmount = 0,
                                                                             }).ToList();
            if (collegeIncomeId == 0)
            {
                ViewBag.Norecords = true;
            }
            else
            {
                foreach (var item in incomeType)
                {
                    item.incomeAmount = db.jntuh_college_income.Where(collegeIncome => collegeIncome.collegeId == userCollegeID &&
                                                                    collegeIncome.incomeTypeID == item.incomeTypeID)
                                                             .Select(collegeIncome => collegeIncome.incomeAmount).FirstOrDefault();
                    if (item.incomeAmount == 0)
                    {
                        ViewBag.incomeAmount = true;
                    }

                }
            }
            ViewBag.Count = incomeType.Count();
            return View("View", incomeType);
        }


    }
}
