using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Security;
using UAAAS.Models;

namespace UAAAS.Controllers
{
    [ErrorHandling]
    public class CollegePaymentController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult Index(string collegeId)
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
            IEnumerable<CollegePayment> collegePayment = db.jntuh_college_payment.Where(a => a.collegeId == userCollegeID)
                .Select(a => new CollegePayment
                {
                    id = a.id,
                    collegeId = userCollegeID,
                    paymentDate = a.paymentDate,
                    paymentType = a.paymentType,
                    paymentNumber = a.paymentNumber,
                    paymentStatus = a.paymentStatus,
                    paymentAmount = a.paymentAmount,
                    paymentBranch = a.paymentBranch,
                    paymentLocation = a.paymentLocation
                }).OrderBy(a => a.paymentDate).ToList();
            ViewBag.CollegePayment = collegePayment;
            ViewBag.Count = collegePayment.Count();
            DateTime todayDate = DateTime.Now.Date;
            int status = db.jntuh_college_edit_status.Where(editStatus => editStatus.collegeId == userCollegeID &&
                                                                                     editStatus.IsCollegeEditable == true &&
                                                                                     editStatus.editFromDate <= todayDate &&
                                                                                     editStatus.editToDate >= todayDate)
                                                                .Select(editStatus => editStatus.id)
                                                                .FirstOrDefault();

            if (collegePayment.Count() == 0 && status == 0 && Roles.IsUserInRole("College"))
            {
                ViewBag.NotUpload = true;
            }
            else
            {
                ViewBag.NotUpload = false;
            }
            if (status == 0 && collegePayment.Count() > 0 && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("View", "CollegePayment");
            }
            bool isPageEditable = db.jntuh_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("PY") && a.CollegeId == userCollegeID).Select(a => a.IsEditable).FirstOrDefault();

            if (!isPageEditable)
            {
                return RedirectToAction("View", "CollegePayment");

            }
            return View();
        }

        public ModeOfPaymentModel[] paymentMode = new[]
            {
                new ModeOfPaymentModel { id = "1", Name = "DD" },
            };

        public List<ModeOfPaymentModel> paymentsModes = new List<ModeOfPaymentModel>();

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        [HttpGet]
        public ActionResult AddEditRecord(int? id, string collegeId)
        {
            List<SelectListItem> list = new List<SelectListItem>();
            foreach (var type in paymentMode)
            {
                list.Add(new SelectListItem { Text = type.Name, Value = type.id });
            }

            ViewBag.Modes = list;

            if (Request.IsAjaxRequest())
            {
                if (id != null)
                {
                    ViewBag.IsUpdate = true;
                    CollegePayment collegePayment = db.jntuh_college_payment.Where(p => p.id == id).Select(p =>
                                              new CollegePayment
                                              {
                                                  id = p.id,
                                                  collegeId = p.collegeId,
                                                  paymentDate = p.paymentDate,
                                                  paymentType = p.paymentType,
                                                  paymentNumber = p.paymentNumber,
                                                  paymentStatus = p.paymentStatus,
                                                  paymentAmount = p.paymentAmount,
                                                  paymentBranch = p.paymentBranch,
                                                  paymentLocation = p.paymentLocation,
                                                  createdBy = p.createdBy,
                                                  createdOn = p.createdOn,
                                                  updatedBy = p.updatedBy,
                                                  updatedOn = p.updatedOn
                                              }).FirstOrDefault();
                    collegePayment.date = Utilities.MMDDYY2DDMMYY(collegePayment.paymentDate.ToString());
                    return PartialView("_Create", collegePayment);
                }
                else
                {
                    CollegePayment collegePayment = new CollegePayment();
                    if (collegeId != null)
                    {
                        int userCollegeID = Convert.ToInt32(Utilities.DecryptString(collegeId, WebConfigurationManager.AppSettings["CryptoKey"]));
                        collegePayment.collegeId = userCollegeID;
                    }
                    ViewBag.IsUpdate = false;
                    return PartialView("_Create", collegePayment);
                }
            }
            else
            {
                if (id != null)
                {
                    ViewBag.IsUpdate = true;
                    CollegePayment collegePayment = db.jntuh_college_payment.Where(p => p.id == id).Select(p =>
                                              new CollegePayment
                                              {
                                                  id = p.id,
                                                  collegeId = p.collegeId,
                                                  paymentDate = p.paymentDate,
                                                  paymentType = p.paymentType,
                                                  paymentNumber = p.paymentNumber,
                                                  paymentStatus = p.paymentStatus,
                                                  paymentAmount = p.paymentAmount,
                                                  paymentBranch = p.paymentBranch,
                                                  paymentLocation = p.paymentLocation,
                                                  createdBy = p.createdBy,
                                                  createdOn = p.createdOn,
                                                  updatedBy = p.updatedBy,
                                                  updatedOn = p.updatedOn
                                              }).FirstOrDefault();
                    collegePayment.date = Utilities.MMDDYY2DDMMYY(collegePayment.paymentDate.ToString());
                    return View("Create", collegePayment);
                }
                else
                {
                    CollegePayment collegePayment = new CollegePayment();
                    if (collegeId != null)
                    {
                        int userCollegeID = Convert.ToInt32(Utilities.DecryptString(collegeId, WebConfigurationManager.AppSettings["CryptoKey"]));
                        collegePayment.collegeId = userCollegeID;
                    }
                    ViewBag.IsUpdate = false;
                    return View("Create", collegePayment);
                }
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult AddEditRecord(CollegePayment collegePayment, string cmd)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 0)
            {
                userCollegeID = collegePayment.collegeId;
            }
            if (ModelState.IsValid)
            {

                if (cmd == "Save")
                {
                    try
                    {
                        jntuh_college_payment jntuh_college_payment = new jntuh_college_payment();
                        jntuh_college_payment.id = collegePayment.id;
                        jntuh_college_payment.collegeId = userCollegeID;
                        jntuh_college_payment.paymentDate = Utilities.DDMMYY2MMDDYY(collegePayment.date);
                        jntuh_college_payment.paymentAmount = collegePayment.paymentAmount;
                        jntuh_college_payment.paymentType = collegePayment.paymentType;
                        jntuh_college_payment.paymentNumber = collegePayment.paymentNumber;
                        jntuh_college_payment.paymentBranch = collegePayment.paymentBranch;
                        jntuh_college_payment.paymentLocation = collegePayment.paymentLocation;
                        jntuh_college_payment.paymentStatus = 0;

                        int existingId = db.jntuh_college_payment.Where(p => p.paymentNumber == collegePayment.paymentNumber).Select(p => p.id).FirstOrDefault();
                        if (existingId > 0)
                        {
                            jntuh_college_payment.id = existingId;
                            jntuh_college_payment.createdBy = db.jntuh_college_payment.Where(d => d.id == existingId).Select(d => d.createdBy).FirstOrDefault();
                            jntuh_college_payment.createdOn = db.jntuh_college_payment.Where(d => d.id == existingId).Select(d => d.createdOn).FirstOrDefault();
                            jntuh_college_payment.updatedBy = userID;
                            jntuh_college_payment.updatedOn = DateTime.Now;
                            db.Entry(jntuh_college_payment).State = EntityState.Modified;
                            db.SaveChanges();
                            TempData["CollegesSuccess"] = "Updated Successfully.";
                        }
                        else
                        {
                            jntuh_college_payment.createdBy = userID;
                            jntuh_college_payment.createdOn = DateTime.Now;

                            db.jntuh_college_payment.Add(jntuh_college_payment);
                            db.SaveChanges();
                            TempData["CollegesSuccess"] = "Added Successfully.";
                        }
                        return RedirectToAction("Index", new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
                    }
                    catch { }
                }
                else
                {
                    try
                    {
                        jntuh_college_payment jntuh_college_payment = new jntuh_college_payment();
                        jntuh_college_payment.id = collegePayment.id;
                        jntuh_college_payment.collegeId = userCollegeID;
                        jntuh_college_payment.paymentDate = Utilities.DDMMYY2MMDDYY(collegePayment.date);
                        jntuh_college_payment.paymentAmount = collegePayment.paymentAmount;
                        jntuh_college_payment.paymentType = collegePayment.paymentType;
                        jntuh_college_payment.paymentNumber = collegePayment.paymentNumber;
                        jntuh_college_payment.paymentBranch = collegePayment.paymentBranch;
                        jntuh_college_payment.paymentLocation = collegePayment.paymentLocation;
                        jntuh_college_payment.paymentStatus = 0;

                        jntuh_college_payment.createdBy = collegePayment.createdBy;
                        jntuh_college_payment.createdOn = collegePayment.createdOn;
                        jntuh_college_payment.updatedBy = userID;
                        jntuh_college_payment.updatedOn = DateTime.Now;
                        db.Entry(jntuh_college_payment).State = EntityState.Modified;
                        db.SaveChanges();
                        TempData["CollegesSuccess"] = "Updated Successfully.";

                        return RedirectToAction("Index", new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
                    }
                    catch { }
                }
            }

            if (Request.IsAjaxRequest())
            {
                return PartialView("_Create", collegePayment);
            }
            else
            {
                return View("Create", collegePayment);
            }
        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult DeleteRecord(int id)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 0)
            {
                userCollegeID = db.jntuh_college_payment.Where(oc => oc.id == id).Select(oc => oc.collegeId).FirstOrDefault();
            }
            jntuh_college_payment payment = db.jntuh_college_payment.Where(oc => oc.id == id && oc.collegeId == userCollegeID).FirstOrDefault();
            if (payment != null)
            {
                try
                {
                    db.jntuh_college_payment.Remove(payment);
                    db.SaveChanges();
                    TempData["CollegesSuccess"] = "Deleted Successfully.";
                }
                catch { }
            }
            return RedirectToAction("Index", new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
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
            DateTime todayDate = DateTime.Now.Date;
            int status = db.jntuh_college_edit_status.Where(editStatus => editStatus.collegeId == userCollegeID &&
                                                                          editStatus.IsCollegeEditable == true &&
                                                                          editStatus.editFromDate <= todayDate &&
                                                                          editStatus.editToDate >= todayDate)
                                                     .Select(editStatus => editStatus.id)
                                                     .FirstOrDefault();
            if (status > 0 && Roles.IsUserInRole("College"))
            {
                bool isPageEditable = db.jntuh_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("PY") && a.CollegeId == userCollegeID).Select(a => a.IsEditable).FirstOrDefault();

                if (isPageEditable)
                {
                    return RedirectToAction("Index");
                }
            }

            IEnumerable<CollegePayment> collegePayment = db.jntuh_college_payment.Where(a => a.collegeId == userCollegeID)
                .Select(a => new CollegePayment
                {
                    id = a.id,
                    collegeId = userCollegeID,
                    paymentDate = a.paymentDate,
                    paymentType = a.paymentType,
                    paymentNumber = a.paymentNumber,
                    paymentStatus = a.paymentStatus,
                    paymentAmount = a.paymentAmount,
                    paymentBranch = a.paymentBranch,
                    paymentLocation = a.paymentLocation
                }).OrderBy(a => a.paymentDate).ToList();
            ViewBag.CollegePayment = collegePayment;
            ViewBag.Count = collegePayment.Count();
            return View("View", collegePayment);
        }
    }
}
