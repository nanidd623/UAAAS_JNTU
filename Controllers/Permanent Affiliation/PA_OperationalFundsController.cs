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

namespace UAAAS.Controllers.Permanent_Affiliation
{
    [ErrorHandling]
    public class PA_OperationalFundsController : BaseController
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
            ViewBag.collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString();
            if (userCollegeID == 0 && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("Create", "CollegeInformation");
            }
            if (userCollegeID == 0 && (Roles.IsUserInRole("Admin") || Roles.IsUserInRole("DataEntry")))
            {
                return RedirectToAction("Create", "CollegeInformation", new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
            }



            List<OperationalFunds> operationalFunds = db.jntuh_college_funds.Where(a => a.collegeId == userCollegeID).Select(a =>
                                              new OperationalFunds
                                              {
                                                  id = a.id,
                                                  //rowNumber = index + 1,
                                                  collegeId = a.collegeId,
                                                  bankName = a.bankName,
                                                  bankBranch = a.bankBranch,
                                                  bankAddress = a.bankAddress,
                                                  bankAccountNumber = a.bankAccountNumber,
                                                  FDR = a.FDR,
                                                  FDRReceiptview = a.fdrreceipt,
                                                  cashBalance = a.cashBalance,
                                                  total = a.cashBalance + a.FDR,
                                                  createdBy = a.createdBy,
                                                  createdOn = a.createdOn,
                                                  my_aspnet_users = a.my_aspnet_users,
                                                  my_aspnet_users1 = a.my_aspnet_users1
                                              }).ToList();
            ViewBag.OperationalFunds = operationalFunds;
            ViewBag.Count = operationalFunds.Count();
            DateTime todayDate = DateTime.Now.Date;
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int prAy =
                db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();

            var status = GetPageEditableStatus(userCollegeID);

            if (operationalFunds.Count() == 0 && status == 0 && Roles.IsUserInRole("College"))
            {
                ViewBag.NotUpload = true;
            }
            else
            {
                ViewBag.NotUpload = false;
            }
            if (status == 0 && operationalFunds.Count() > 0 && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("View", "PA_OperationalFunds");
            }
            bool isPageEditable = db.jntuh_pa_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("POF") && a.CollegeId == userCollegeID).Select(a => a.IsEditable).FirstOrDefault();

            if (isPageEditable)
            {
                ViewBag.IsEditable = true;
            }
            else
            {
                ViewBag.IsEditable = false;
                return RedirectToAction("View", "PA_OperationalFunds");
            }
            return View();
        }

        [HttpGet]
        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult AddEditRecord(int? id, string collegeId)
        {

            if (Request.IsAjaxRequest())
            {
                if (id != null)
                {
                    ViewBag.IsUpdate = true;
                    OperationalFunds operationalFunds = db.jntuh_college_funds.Where(oc => oc.id == id).Select(a =>
                                              new OperationalFunds
                                              {
                                                  id = a.id,
                                                  collegeId = a.collegeId,
                                                  bankName = a.bankName,
                                                  bankBranch = a.bankBranch,
                                                  bankAddress = a.bankAddress,
                                                  bankAccountNumber = a.bankAccountNumber,
                                                  FDR = a.FDR,
                                                  FDRReceiptview = a.fdrreceipt,
                                                  cashBalance = a.cashBalance,

                                                  createdBy = a.createdBy,
                                                  createdOn = a.createdOn,
                                                  updatedBy = a.updatedBy,
                                                  updatedOn = a.updatedOn,


                                                  my_aspnet_users = a.my_aspnet_users,
                                                  my_aspnet_users1 = a.my_aspnet_users1
                                              }).FirstOrDefault();
                    return PartialView("_OperationalFundsData", operationalFunds);
                }
                else
                {
                    OperationalFunds operationalFunds = new OperationalFunds();
                    if (collegeId != null)
                    {
                        int usercollegeId = Convert.ToInt32(Utilities.DecryptString(collegeId, WebConfigurationManager.AppSettings["CryptoKey"]));
                        operationalFunds.collegeId = usercollegeId;
                    }
                    ViewBag.IsUpdate = false;
                    return PartialView("_OperationalFundsData", operationalFunds);
                }
            }
            else
            {
                if (id != null)
                {
                    ViewBag.IsUpdate = true;
                    OperationalFunds operationalFunds = db.jntuh_college_funds.Where(oc => oc.id == id).Select(a =>
                                               new OperationalFunds
                                               {
                                                   id = a.id,
                                                   collegeId = a.collegeId,
                                                   bankName = a.bankName,
                                                   bankBranch = a.bankBranch,
                                                   bankAddress = a.bankAddress,
                                                   bankAccountNumber = a.bankAccountNumber,
                                                   FDR = a.FDR,
                                                   cashBalance = a.cashBalance,
                                                   createdBy = a.createdBy,
                                                   createdOn = a.createdOn,
                                                   updatedBy = a.updatedBy,
                                                   updatedOn = a.updatedOn,
                                                   jntuh_college = a.jntuh_college,

                                                   my_aspnet_users = a.my_aspnet_users,
                                                   my_aspnet_users1 = a.my_aspnet_users1
                                               }).FirstOrDefault();
                    return View("OperationalFundsData", operationalFunds);
                }
                else
                {
                    OperationalFunds operationalFunds = new OperationalFunds();
                    if (collegeId != null)
                    {
                        int usercollegeId = Convert.ToInt32(Utilities.DecryptString(collegeId, WebConfigurationManager.AppSettings["CryptoKey"]));
                        operationalFunds.collegeId = usercollegeId;
                    }
                    ViewBag.IsUpdate = false;
                    return View("OperationalFundsData", operationalFunds);
                }
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult AddEditRecord(OperationalFunds operationalFunds, string cmd)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 0)
            {
                userCollegeID = operationalFunds.collegeId;
            }
            if (ModelState.IsValid)
            {

                if (cmd == "Save")
                {
                    try
                    {
                        jntuh_college_funds jntuh_college_funds = new jntuh_college_funds();
                        jntuh_college_funds.id = operationalFunds.id;
                        jntuh_college_funds.collegeId = userCollegeID;
                        jntuh_college_funds.bankName = operationalFunds.bankName;
                        jntuh_college_funds.bankBranch = operationalFunds.bankBranch;
                        jntuh_college_funds.bankAddress = operationalFunds.bankAddress;
                        jntuh_college_funds.bankAccountNumber = operationalFunds.bankAccountNumber;
                        jntuh_college_funds.FDR = operationalFunds.FDR;
                        string photoPath = "~/Content/Upload/College/operationalFunds";
                        if (operationalFunds.FDRReceipt != null)
                        {
                            if (!Directory.Exists(Server.MapPath(photoPath)))
                            {
                                Directory.CreateDirectory(Server.MapPath(photoPath));
                            }

                            var ext = Path.GetExtension(operationalFunds.FDRReceipt.FileName);

                            if (ext.ToUpper().Equals(".PDF"))
                            {
                                string fileName = "FDR-" + userCollegeID + "-" + DateTime.Now.ToString("yyyyMMdd-HHmmssffffff");
                                operationalFunds.FDRReceipt.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(photoPath), fileName, ext));
                                jntuh_college_funds.fdrreceipt = string.Format("{0}{1}", fileName, ext);
                            }
                        }

                        jntuh_college_funds.cashBalance = operationalFunds.cashBalance;
                        jntuh_college_funds.createdBy = userID;
                        jntuh_college_funds.createdOn = DateTime.Now;

                        db.jntuh_college_funds.Add(jntuh_college_funds);
                        db.SaveChanges();
                        TempData["Success"] = "Operational funds Details are Added Successfully.";
                        return RedirectToAction("Index", new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
                    }
                    catch { }
                }
                else
                {
                    try
                    {
                        jntuh_college_funds funds = new jntuh_college_funds();

                        if (funds != null)
                        {
                            funds.id = operationalFunds.id;
                            funds.collegeId = userCollegeID;
                            funds.bankName = operationalFunds.bankName;
                            funds.bankBranch = operationalFunds.bankBranch;
                            funds.bankAddress = operationalFunds.bankAddress;
                            funds.bankAccountNumber = operationalFunds.bankAccountNumber;
                            funds.FDR = operationalFunds.FDR;
                            string photoPath = "~/Content/Upload/College/operationalFunds";
                            if (operationalFunds.FDRReceipt != null)
                            {
                                if (!Directory.Exists(Server.MapPath(photoPath)))
                                {
                                    Directory.CreateDirectory(Server.MapPath(photoPath));
                                }

                                var ext = Path.GetExtension(operationalFunds.FDRReceipt.FileName);

                                if (ext.ToUpper().Equals(".PDF"))
                                {
                                    string fileName = "FDR-" + DateTime.Now.ToString("yyyyMMdd-HHmmssffffff");
                                    operationalFunds.FDRReceipt.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(photoPath), fileName, ext));
                                    funds.fdrreceipt = string.Format("{0}{1}", fileName, ext);
                                }
                            }

                            funds.cashBalance = operationalFunds.cashBalance;
                            funds.createdBy = operationalFunds.createdBy;
                            funds.createdOn = operationalFunds.createdOn;
                            funds.updatedBy = userID;
                            funds.updatedOn = DateTime.Now;
                            db.Entry(funds).State = EntityState.Modified;
                            db.SaveChanges();
                            TempData["Success"] = "Operational funds Details are Updated Successfully.";
                        }
                        return RedirectToAction("Index", new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
                    }
                    catch { }
                }
            }

            if (Request.IsAjaxRequest())
            {
                return PartialView("_OperationalFundsData", operationalFunds);
            }
            else
            {
                return View("_OperationalFundsData", operationalFunds);
            }
        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult DeleteRecord(int id)
        {
            jntuh_college_funds funds = db.jntuh_college_funds.Where(oc => oc.id == id).FirstOrDefault();
            if (funds != null)
            {
                try
                {
                    db.jntuh_college_funds.Remove(funds);
                    db.SaveChanges();
                }
                catch { }
            }
            return RedirectToAction("Index", new { collegeId = Utilities.EncryptString(funds.collegeId.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult Details(int id)
        {
            OperationalFunds funds = db.jntuh_college_funds.Where(oc => oc.id == id).Select(a =>
                                              new OperationalFunds
                                              {
                                                  id = a.id,
                                                  collegeId = a.collegeId,
                                                  bankName = a.bankName,
                                                  bankBranch = a.bankBranch,
                                                  bankAddress = a.bankAddress,
                                                  bankAccountNumber = a.bankAccountNumber,
                                                  FDR = a.FDR,
                                                  cashBalance = a.cashBalance,
                                                  total = a.cashBalance + a.FDR,
                                                  my_aspnet_users = a.my_aspnet_users,
                                                  my_aspnet_users1 = a.my_aspnet_users1
                                              }).FirstOrDefault();
            if (funds != null)
            {
                if (Request.IsAjaxRequest())
                {
                    return PartialView("_OperationalFundsDetails", funds);
                }
                else
                {
                    return View("OperationalFundsDetails", funds);
                }
            }
            return View("Index", new { collegeId = Utilities.EncryptString(funds.collegeId.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
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
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int prAy = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
            var status = GetPageEditableStatus(userCollegeID);

            if (status > 0 && Roles.IsUserInRole("College"))
            {
                bool isPageEditable = db.jntuh_pa_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("POF") && a.CollegeId == userCollegeID).Select(a => a.IsEditable).FirstOrDefault();

                if (isPageEditable)
                {
                    ViewBag.IsEditable = true;
                    return RedirectToAction("Index");
                }
                else
                {
                    ViewBag.IsEditable = false;
                }

            }

            List<OperationalFunds> operationalFunds = db.jntuh_college_funds.Where(a => a.collegeId == userCollegeID).Select(a =>
                                              new OperationalFunds
                                              {
                                                  id = a.id,
                                                  //rowNumber = index + 1,
                                                  collegeId = a.collegeId,
                                                  bankName = a.bankName,
                                                  bankBranch = a.bankBranch,
                                                  bankAddress = a.bankAddress,
                                                  bankAccountNumber = a.bankAccountNumber,
                                                  FDR = a.FDR,
                                                  cashBalance = a.cashBalance,
                                                  total = a.cashBalance + a.FDR,
                                                  createdBy = a.createdBy,
                                                  createdOn = a.createdOn,
                                                  my_aspnet_users = a.my_aspnet_users,
                                                  my_aspnet_users1 = a.my_aspnet_users1
                                              }).ToList();
            ViewBag.OperationalFunds = operationalFunds;
            ViewBag.Count = operationalFunds.Count();
            return View("View", operationalFunds);
        }

        public ActionResult UserView(string id)
        {
            int userCollegeID = Convert.ToInt32(Utilities.DecryptString(id, WebConfigurationManager.AppSettings["CryptoKey"]));
            List<OperationalFunds> operationalFunds = db.jntuh_college_funds.Where(a => a.collegeId == userCollegeID).Select(a =>
                                              new OperationalFunds
                                              {
                                                  id = a.id,
                                                  collegeId = a.collegeId,
                                                  bankName = a.bankName,
                                                  bankBranch = a.bankBranch,
                                                  bankAddress = a.bankAddress,
                                                  bankAccountNumber = a.bankAccountNumber,
                                                  FDR = a.FDR,
                                                  cashBalance = a.cashBalance,
                                                  total = a.cashBalance + a.FDR,
                                                  createdBy = a.createdBy,
                                                  createdOn = a.createdOn,
                                                  my_aspnet_users = a.my_aspnet_users,
                                                  my_aspnet_users1 = a.my_aspnet_users1
                                              }).ToList();
            ViewBag.OperationalFunds = operationalFunds;
            ViewBag.Count = operationalFunds.Count();
            return View("UserView", operationalFunds);
        }
    }
}
