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
    public class PaymentOfFeesController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();

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
            int collegePaymentId = db.jntuh_college_paymentoffee.Where(p => p.collegeId == userCollegeID).Select(p => p.id).FirstOrDefault();

            DateTime todayDate = DateTime.Now.Date;
            int status = db.jntuh_college_edit_status.Where(editStatus => editStatus.collegeId == userCollegeID &&
                                                                                     editStatus.IsCollegeEditable == true &&
                                                                                     editStatus.editFromDate <= todayDate &&
                                                                                     editStatus.editToDate >= todayDate)
                                                                .Select(editStatus => editStatus.id)
                                                                .FirstOrDefault();

            if (userCollegeID > 0 && collegePaymentId > 0 && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("View", "PaymentOfFees");
            }
            if (userCollegeID > 0 && collegePaymentId > 0 && (Roles.IsUserInRole("Admin") || Roles.IsUserInRole("DataEntry")))
            {
                return RedirectToAction("Edit", "PaymentOfFees", new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
            }

            if (collegePaymentId == 0 && status == 0 && Roles.IsUserInRole("College"))
            {
                ViewBag.NotUpload = true;
            }
            else
            {
                ViewBag.NotUpload = false;
            }

            bool isPageEditable = db.jntuh_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("PF") && a.CollegeId == userCollegeID).Select(a => a.IsEditable).FirstOrDefault();

            if (isPageEditable)
            {
                ViewBag.IsEditable = true;
            }
            else
            {
                ViewBag.IsEditable = false;
                return RedirectToAction("View", "PaymentOfFees");
            }

            List<PaymentOfFees> collegePaymentOfFees = db.jntuh_college_paymentoffee_type.Where(p => p.isActive == true).Select(p => new PaymentOfFees
            {
                FeeTypeID = p.id,
                FeeType = p.FeeType,
                paidAmount=0,
                duesAmount="",
                collegeId = userCollegeID
            }).ToList();
            ViewBag.Count = collegePaymentOfFees.Count();
            return View(collegePaymentOfFees);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult Create(ICollection<PaymentOfFees> collegePaymentOfFees)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(collegeUser => collegeUser.userID == userID).Select(collegeUser => collegeUser.collegeID).FirstOrDefault();
            if (userCollegeID == 0)
            {
                foreach (var item in collegePaymentOfFees)
                {
                    userCollegeID = item.collegeId;
                }
            }
            SaveCollegePaymentOfFees(collegePaymentOfFees);

            List<PaymentOfFees> collegePaymentOfFeesDetails = db.jntuh_college_paymentoffee_type.Where(p => p.isActive == true).Select(p => new PaymentOfFees
            {
                FeeTypeID = p.id,
                FeeType = p.FeeType,
                paidAmount = 0,
                duesAmount = "",
                collegeId = userCollegeID
            }).ToList();
            foreach (var item in collegePaymentOfFeesDetails)
            {
                item.paidAmount = db.jntuh_college_paymentoffee.Where(e => e.collegeId == userCollegeID && e.FeeTypeID == item.FeeTypeID).Select(e => e.paidAmount).FirstOrDefault();
                item.duesAmount = db.jntuh_college_paymentoffee.Where(e => e.collegeId == userCollegeID && e.FeeTypeID == item.FeeTypeID).Select(e => e.duesAmount).FirstOrDefault();
            }
            ViewBag.Count = collegePaymentOfFeesDetails.Count();
            return View(collegePaymentOfFeesDetails);
        }

        private void SaveCollegePaymentOfFees(ICollection<PaymentOfFees> collegePaymentOfFees)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(collegeUser => collegeUser.userID == userID).Select(collegeUser => collegeUser.collegeID).FirstOrDefault();
            if (userCollegeID == 0)
            {
                foreach (var item in collegePaymentOfFees)
                {
                    userCollegeID = item.collegeId;
                }
            }
            var errorMessage = string.Empty;
            if (ModelState.IsValid)
            {
                foreach (PaymentOfFees item in collegePaymentOfFees)
                {
                    jntuh_college_paymentoffee collegePaymentOfFee = new jntuh_college_paymentoffee();
                    collegePaymentOfFee.FeeTypeID = item.FeeTypeID;
                    collegePaymentOfFee.paidAmount = item.paidAmount;
                    collegePaymentOfFee.duesAmount = item.duesAmount;
                    collegePaymentOfFee.collegeId = userCollegeID;

                    int feeId = db.jntuh_college_paymentoffee.Where(p => p.collegeId == userCollegeID && p.FeeTypeID == item.FeeTypeID).Select(p => p.id).FirstOrDefault();
                    if (feeId == 0)
                    {

                        if ((item.FeeTypeID != 0))
                        {
                            collegePaymentOfFee.createdBy = userID;
                            collegePaymentOfFee.createdOn = DateTime.Now;
                            db.jntuh_college_paymentoffee.Add(collegePaymentOfFee);
                            db.SaveChanges();
                            errorMessage = "Save";
                        }
                    }
                    else
                    {
                        collegePaymentOfFee.id = feeId;
                        collegePaymentOfFee.createdBy = db.jntuh_college_paymentoffee.Where(e => e.id == feeId).Select(e => e.createdBy).FirstOrDefault();
                        collegePaymentOfFee.createdOn = db.jntuh_college_paymentoffee.Where(e => e.id == feeId).Select(e => e.createdOn).FirstOrDefault();
                        collegePaymentOfFee.updatedBy = userID;
                        collegePaymentOfFee.updatedOn = DateTime.Now;
                        db.Entry(collegePaymentOfFee).State = EntityState.Modified;
                        db.SaveChanges();
                        errorMessage = "Update";
                    }
                }
            }
            if (errorMessage == "Update")
            {
                TempData["Success"] = "College Payment Of Fee details are Updated Successfully.";
            }
            else
            {
                TempData["Success"] = "College Payment Of Fee details are Added Successfully.";
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
            int FeeId = db.jntuh_college_paymentoffee.Where(p => p.collegeId == userCollegeID).Select(p => p.id).FirstOrDefault();

            if (userCollegeID == 0 && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("Create", "CollegeInformation");
            }
            if (userCollegeID == 0 && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("Create", "CollegeInformation", new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
            }           
            if (FeeId == 0 && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("Create", "PaymentOfFees");
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
                return RedirectToAction("View", "PaymentOfFees");
            }
            else
            {
                bool isPageEditable = db.jntuh_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("PF") && a.CollegeId == userCollegeID).Select(a => a.IsEditable).FirstOrDefault();

                if (isPageEditable)
                {
                    ViewBag.IsEditable = true;
                }
                else
                {
                    ViewBag.IsEditable = false;
                    return RedirectToAction("View", "PaymentOfFees");
                }
            }

            List<PaymentOfFees> collegePaymentOfFeesDetails = db.jntuh_college_paymentoffee_type.Where(p => p.isActive == true).Select(p => new PaymentOfFees
            {
                FeeTypeID = p.id,
                FeeType = p.FeeType,
                paidAmount = 0,
                duesAmount = "",
                collegeId = userCollegeID
            }).ToList();
            foreach (var item in collegePaymentOfFeesDetails)
            {
                item.paidAmount = db.jntuh_college_paymentoffee.Where(e => e.collegeId == userCollegeID && e.FeeTypeID == item.FeeTypeID).Select(e => e.paidAmount).FirstOrDefault();
                item.duesAmount = db.jntuh_college_paymentoffee.Where(e => e.collegeId == userCollegeID && e.FeeTypeID == item.FeeTypeID).Select(e => e.duesAmount).FirstOrDefault();
            }
            ViewBag.Count = collegePaymentOfFeesDetails.Count();
            return View("Create", collegePaymentOfFeesDetails);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult Edit(ICollection<PaymentOfFees> collegePaymentOfFees)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(collegeUser => collegeUser.userID == userID).Select(collegeUser => collegeUser.collegeID).FirstOrDefault();
            if (userCollegeID == 0)
            {
                foreach (var item in collegePaymentOfFees)
                {
                    userCollegeID = item.collegeId;
                }
            }
            SaveCollegePaymentOfFees(collegePaymentOfFees);

            List<PaymentOfFees> collegePaymentOfFeesDetails = db.jntuh_college_paymentoffee_type.Where(p => p.isActive == true).Select(p => new PaymentOfFees
            {
                FeeTypeID = p.id,
                FeeType = p.FeeType,
                paidAmount = 0,
                duesAmount = "",
                collegeId = userCollegeID
            }).ToList();
            foreach (var item in collegePaymentOfFeesDetails)
            {
                item.paidAmount = db.jntuh_college_paymentoffee.Where(e => e.collegeId == userCollegeID && e.FeeTypeID == item.FeeTypeID).Select(e => e.paidAmount).FirstOrDefault();
                item.duesAmount = db.jntuh_college_paymentoffee.Where(e => e.collegeId == userCollegeID && e.FeeTypeID == item.FeeTypeID).Select(e => e.duesAmount).FirstOrDefault();
            }
            ViewBag.Count = collegePaymentOfFeesDetails.Count();
            return View("Create", collegePaymentOfFeesDetails);
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
            int FeeId = db.jntuh_college_paymentoffee.Where(p => p.collegeId == userCollegeID).Select(p => p.id).FirstOrDefault();



            DateTime todayDate = DateTime.Now.Date;
            int status = db.jntuh_college_edit_status.Where(editStatus => editStatus.collegeId == userCollegeID &&
                                                                          editStatus.IsCollegeEditable == true &&
                                                                          editStatus.editFromDate <= todayDate &&
                                                                          editStatus.editToDate >= todayDate)
                                                     .Select(editStatus => editStatus.id)
                                                     .FirstOrDefault();
            if (status > 0 && Roles.IsUserInRole("College"))
            {
                bool isPageEditable = db.jntuh_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("PF") && a.CollegeId == userCollegeID).Select(a => a.IsEditable).FirstOrDefault();

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

            List<PaymentOfFees> collegecollegePaymentOfFees = db.jntuh_college_paymentoffee_type.Where(p => p.isActive == true).Select(p => new PaymentOfFees
            {
                FeeTypeID = p.id,
                FeeType=p.FeeType,
                paidAmount=0,
                duesAmount="",
            }).ToList();

            if (FeeId == 0)
            {
                ViewBag.Norecords = true;
            }
            else
            {
                foreach (var item in collegecollegePaymentOfFees)
                {
                    item.paidAmount = db.jntuh_college_paymentoffee.Where(e => e.collegeId == userCollegeID && e.FeeTypeID == item.FeeTypeID).Select(e => e.paidAmount).FirstOrDefault();
                item.duesAmount = db.jntuh_college_paymentoffee.Where(e => e.collegeId == userCollegeID && e.FeeTypeID == item.FeeTypeID).Select(e => e.duesAmount).FirstOrDefault();
                }
                ViewBag.Update = true;
            }
            ViewBag.Count = collegecollegePaymentOfFees.Count();
            return View("View", collegecollegePaymentOfFees);
        }
    }
}
