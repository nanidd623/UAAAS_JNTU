using iTextSharp.text;
using iTextSharp.text.html.simpleparser;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using UAAAS.Models;
using System.Web.Security;
using System.Data;
using System.Web.Configuration;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using System.Net;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.Configuration;

namespace UAAAS.Controllers
{
    [ErrorHandling]
    public class CollegeNOCController : BaseController
    {
        uaaasDBContext db = new uaaasDBContext();
        public ActionResult Index()
        {
            int userId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(collegeUser => collegeUser.userID == userId).Select(collegeUser => collegeUser.collegeID).FirstOrDefault();
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            ViewBag.collegeId = userCollegeID;
            return View();
        }

        [Authorize(Roles = "Admin,College")]
        public ActionResult College_NocAction(string collegeId, int? noc_typeId)
        {
            int userId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(collegeUser => collegeUser.userID == userId).Select(collegeUser => collegeUser.collegeID).FirstOrDefault();

            var todayDate = DateTime.Now;
            var jntuh_academivYear = db.jntuh_academic_year.ToList();
            int actualYear = jntuh_academivYear.Where(s => s.isActive == true && s.isPresentAcademicYear == true).Select(a => a.id).FirstOrDefault();
            int PresentYear = jntuh_academivYear.Where(s => s.id == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
            int lid = db.jntuh_link_screens.Where(p => p.linkName == "College NOC Request" && p.isActive == true).Select(s => s.id).FirstOrDefault();
            jntuh_college_links_assigned NOCPhase =
                db.jntuh_college_links_assigned.Where(l => l.linkId == lid && l.academicyearId == PresentYear && l.isActive == true && l.fromdate <= todayDate && l.todate >= todayDate).Select(s => s).FirstOrDefault();

            if (NOCPhase == null || userCollegeID != 375)
            {
                return RedirectToAction("College", "Dashboard");
            }

            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            if (userCollegeID == 0 && collegeId != null)
            {
                if (Roles.IsUserInRole("Admin"))
                {
                    userCollegeID = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(collegeId, WebConfigurationManager.AppSettings["CryptoKey"]));
                }
            }



            var NOCTypes = db.jntuh_college_paymentoffee_type.Where(a => (a.FeeType == "NOC For Closure of College" || a.FeeType == "NOC For Closure of Course") && a.isActive == true).Select(s => new { TypeId = s.id, Type = s.FeeType }).ToList();
            ViewBag.NocTypes = NOCTypes.Select(e => new { e.TypeId, e.Type }).ToList();
            var NOCIDs = NOCTypes.Select(e => e.TypeId).ToList();

            var ClosureCollegesIds = new int[] { 330, 122, 189, 186, 197, 336, 46, 138, 78 };
            if (ClosureCollegesIds.Contains(userCollegeID))
                return RedirectToAction("CollegeDashboard", "Dashboard");

            var college = db.jntuh_college.Where(s => s.id == userCollegeID).Select(a => a).FirstOrDefault();
            College_Noc noc = new College_Noc();
            noc.CollegeId = college.id;
            noc.CollegeCode = college.collegeCode;
            noc.CollegeName = college.collegeName;
            if (noc_typeId == null && TempData["NOCID"] != null)
            {
                noc.NocTypeId = Convert.ToInt32(TempData["NOCID"]);
                noc_typeId = noc.NocTypeId;
            }
            else
            {
                noc.NocTypeId = Convert.ToInt32(noc_typeId);
            }

            if (noc_typeId != 0 && noc_typeId != null)
            {
                noc.CollegeNocId = NOCTypes.Where(e => e.Type == "NOC For Closure of College").Select(a => a.TypeId).FirstOrDefault();
                noc.CoursesNocId = NOCTypes.Where(e => e.Type == "NOC For Closure of Course").Select(a => a.TypeId).FirstOrDefault();

                var jntuh_college_payments = db.jntuh_paymentresponse.Where(s => s.AcademicYearId == PresentYear && s.CollegeId == noc.CollegeCode && s.AuthStatus == "0300" && NOCIDs.Contains(s.PaymentTypeID)).Select(a => a).ToList();

                if (noc_typeId == noc.CoursesNocId)
                {
                    noc.PaymentAmount = jntuh_college_payments.Where(a => a.PaymentTypeID == noc.CoursesNocId).Select(s => s.TxnAmount).Sum();
                    int NOCID = db.jntuh_college_noc_data.Where(a => a.academicyearId == PresentYear && a.collegeId == userCollegeID && a.noctypeId == noc.CollegeNocId).Select(w => w.id).FirstOrDefault();

                    if (NOCID != 0 && NOCID != null)
                    {
                        TempData["ERROR"] = "Applied NOC for Closure College";
                        return RedirectToAction("College_NocAction", new { collegeId = UAAAS.Models.Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]) });
                    }

                    var jntuh_college_noc_data = db.jntuh_college_noc_data.Where(a => a.academicyearId == PresentYear && a.collegeId == userCollegeID && a.noctypeId == noc.CoursesNocId).Select(w => w).ToList();
                    var jntuh_college_intake_existing = db.jntuh_college_intake_existing.Where(a => a.collegeId == userCollegeID).Select(w => w).ToList();
                    List<IntakeExisting> ExistingIntake = (from e in jntuh_college_intake_existing
                                                           join s in db.jntuh_specialization on e.specializationId equals s.id
                                                           join d in db.jntuh_department on s.departmentId equals d.id
                                                           join de in db.jntuh_degree on d.degreeId equals de.id
                                                           join c in db.jntuh_college on e.collegeId equals c.id
                                                           // join n in db.jntuh_college_noc_data on e.specializationId equals n.specializationId
                                                           where e.collegeId == userCollegeID
                                                           select new IntakeExisting
                                                           {
                                                               DegreeId = de.id,
                                                               Degree = de.degree,
                                                               DepartmentId = d.id,
                                                               Department = d.departmentName,
                                                               SpecializationId = s.id,
                                                               Specialization = s.specializationName,
                                                               shiftId = e.shiftId,
                                                               ApprovedIntake = jntuh_college_intake_existing.Where(sp => sp.academicYearId == PresentYear && sp.specializationId == s.id && sp.shiftId == e.shiftId).Select(a => a.proposedIntake).FirstOrDefault(),
                                                               Noc_SpecId = jntuh_college_noc_data.Where(n => n.specializationId == e.specializationId && n.shiftId == e.shiftId).Select(n => n.id).FirstOrDefault()
                                                           }).ToList();

                    ExistingIntake = ExistingIntake.AsEnumerable().GroupBy(s => new { s.SpecializationId, s.shiftId }).Select(q => q.First()).ToList();
                    //Already Closure Courses Removing.........
                    var AlreadyClosureCoursesSpecIds = db.jntuh_college_noc_data.Where(w => w.collegeId == userCollegeID).Select(r => r.specializationId).ToList();
                    noc.IntakeDetails = ExistingIntake.Where(r => !AlreadyClosureCoursesSpecIds.Contains(r.SpecializationId)).Select(t => t).ToList();
                }
                else
                {
                    noc.PaymentAmount = jntuh_college_payments.Where(a => a.PaymentTypeID == noc.CollegeNocId).Select(s => s.TxnAmount).FirstOrDefault();
                    noc.Noc_DataId = db.jntuh_college_noc_data.Where(a => a.academicyearId == PresentYear && a.collegeId == noc.CollegeId && a.noctypeId == noc.CollegeNocId && a.departmentId == 0 && a.specializationId == 0).Select(a => a.id).FirstOrDefault();
                    noc.PaymentStatus = noc.PaymentAmount == 45000 ? true : false;
                    noc.Noc_Status = db.jntuh_college_noc_data.Where(a => a.academicyearId == PresentYear && a.collegeId == noc.CollegeId && a.noctypeId == noc.CollegeNocId && a.departmentId == 0 && a.specializationId == 0).Select(a => a.isClosure).FirstOrDefault() == true ? "Submitted" : "Not-Submitted";
                }
            }
            return View(noc);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,College")]
        public ActionResult College_NocAction(CourseClosure course, string CourseStatus, string collegeId)
        {
            //return RedirectToAction("CollegeDashboard", "Dashboard");
            int userId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(collegeUser => collegeUser.userID == userId).Select(collegeUser => collegeUser.collegeID).FirstOrDefault();

            var todayDate = DateTime.Now;
            var jntuh_academivYear = db.jntuh_academic_year.ToList();
            int actualYear = jntuh_academivYear.Where(s => s.isActive == true && s.isPresentAcademicYear == true).Select(a => a.id).FirstOrDefault();
            int PresentYear = jntuh_academivYear.Where(s => s.id == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
            int lid = db.jntuh_link_screens.Where(p => p.linkName == "College NOC Request" && p.isActive == true).Select(s => s.id).FirstOrDefault();
            jntuh_college_links_assigned NOCPhase =
                db.jntuh_college_links_assigned.Where(l => l.linkId == lid && l.academicyearId == PresentYear && l.isActive == true && l.fromdate <= todayDate && l.todate >= todayDate).Select(s => s).FirstOrDefault();

            if (NOCPhase == null || userCollegeID != 375)
            {
                return RedirectToAction("College", "Dashboard");
            }


            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            if (userCollegeID == 0 && collegeId != null)
            {
                if (Roles.IsUserInRole("Admin"))
                {
                    userCollegeID = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(collegeId, WebConfigurationManager.AppSettings["CryptoKey"]));
                }
            }

            var NOCTypes = db.jntuh_college_paymentoffee_type.Where(a => (a.FeeType == "NOC For Closure of College" || a.FeeType == "NOC For Closure of Course") && a.isActive == true).Select(s => new { TypeId = s.id, Type = s.FeeType }).ToList();
            int CollegeNocId = NOCTypes.Where(e => e.Type == "NOC For Closure of College").Select(a => a.TypeId).FirstOrDefault();
            int CoursesNocId = NOCTypes.Where(e => e.Type == "NOC For Closure of Course").Select(a => a.TypeId).FirstOrDefault();

            if (course.departmentId == null && course.specializationId == null && course.NocTypeId == CoursesNocId)
            {
                TempData["ERROR"] = "Something Went wrong, Please try again";
                return RedirectToAction("College_NocAction", new { collegeId = UAAAS.Models.Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]), noc_typeId = course.NocTypeId });
            }


            if (course.departmentId == null && course.specializationId == null && course.NocTypeId == CollegeNocId)
            {
                jntuh_college_noc_data Obj = new jntuh_college_noc_data();
                Obj.academicyearId = PresentYear;
                Obj.collegeId = userCollegeID;
                Obj.noctypeId = Convert.ToInt32(course.NocTypeId);
                Obj.departmentId = 0;
                Obj.specializationId = 0;
                Obj.approvedIntake = 0;
                Obj.shiftId = 0;
                Obj.remarks = null;
                Obj.isClosure = false;
                Obj.isActive = true;
                Obj.createdOn = DateTime.Now;
                Obj.createdBy = userId;
                Obj.updatedOn = null;
                Obj.updatedBy = null;
                db.jntuh_college_noc_data.Add(Obj);
                db.SaveChanges();
                TempData["SUCCESS"] = "Your College Closure Request Is Processing....";
                TempData["NOCID"] = course.NocTypeId;
                var jntuh_courses_delete = db.jntuh_college_noc_data.Where(s => s.academicyearId == PresentYear && s.collegeId == userCollegeID && s.noctypeId == CoursesNocId).Select(s => s).ToList();
                foreach (var item in jntuh_courses_delete)
                {
                    db.jntuh_college_noc_data.Remove(item);
                    db.SaveChanges();
                }
                //string clgcode = db.jntuh_college.Where(a => a.id == userCollegeID).Select(a => a.collegeCode).FirstOrDefault();
                //var jntuh_requestPayment = db.jntuh_paymentrequests.Where(s => s.AcademicYearId == PresentYear && s.CollegeCode == clgcode && s.PaymentTypeID == CollegeNocId).Select(s => s).FirstOrDefault();
                //if (jntuh_requestPayment != null)
                //{
                //    db.jntuh_paymentrequests.Remove(jntuh_requestPayment);
                //    db.SaveChanges();
                //}
                return RedirectToAction("College_NocAction", new { collegeId = UAAAS.Models.Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]), noc_typeId = course.NocTypeId });
            }
            else if ((course.departmentId != null || course.departmentId != 0) && (course.specializationId != null || course.specializationId != 0))
            {
                var IDCheck = db.jntuh_college_noc_data.Where(e => e.academicyearId == PresentYear && e.collegeId == userCollegeID && e.departmentId == course.departmentId && e.specializationId == course.specializationId).Select(e => e.id).FirstOrDefault();
                if (IDCheck != 0)
                {
                    TempData["ERROR"] = "This Closure Request Is already Exists....";
                    return RedirectToAction("College_NocAction", new { collegeId = UAAAS.Models.Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]), noc_typeId = course.NocTypeId });
                }
                else
                {
                    jntuh_college_noc_data Obj = new jntuh_college_noc_data();
                    Obj.academicyearId = PresentYear;
                    Obj.collegeId = userCollegeID;
                    Obj.noctypeId = Convert.ToInt32(course.NocTypeId);
                    Obj.departmentId = Convert.ToInt32(course.departmentId);
                    Obj.specializationId = Convert.ToInt32(course.specializationId);
                    Obj.approvedIntake = course.ApprovedIntake;
                    Obj.shiftId = course.ShiftId;
                    Obj.remarks = null;
                    Obj.isClosure = false;
                    Obj.isActive = true;
                    Obj.createdOn = DateTime.Now;
                    Obj.createdBy = userId;
                    Obj.updatedOn = null;
                    Obj.updatedBy = null;
                    db.jntuh_college_noc_data.Add(Obj);
                    db.SaveChanges();
                    TempData["SUCCESS"] = "Your Closure Request Is Processing....";
                    TempData["NOCID"] = course.NocTypeId;

                    return Json(new { noc_typeId = course.NocTypeId }, JsonRequestBehavior.AllowGet);
                }

            }
            return RedirectToAction("College_NocAction");
        }

        // [Authorize(Roles = "Admin,College")]
        public ActionResult FeeDetailsandPayment(string CollegeId, int? nocTypeId)
        {
            int userId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            // int userCollegeID = db.jntuh_college_users.Where(collegeUser => collegeUser.userID == userId).Select(collegeUser => collegeUser.collegeID).FirstOrDefault();
            int userCollegeID = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(CollegeId, WebConfigurationManager.AppSettings["CryptoKey"]));
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            if (userCollegeID == 0 && CollegeId != null)
            {
                if (Roles.IsUserInRole("Admin"))
                {
                    userCollegeID = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(CollegeId, WebConfigurationManager.AppSettings["CryptoKey"]));
                }
            }

            var todayDate = DateTime.Now;
            var jntuh_academivYear = db.jntuh_academic_year.ToList();
            int actualYear = jntuh_academivYear.Where(s => s.isActive == true && s.isPresentAcademicYear == true).Select(a => a.id).FirstOrDefault();
            int PresentYear = jntuh_academivYear.Where(s => s.id == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
            int lid = db.jntuh_link_screens.Where(p => p.linkName == "College NOC Request" && p.isActive == true).Select(s => s.id).FirstOrDefault();
            jntuh_college_links_assigned NOCPhase =
                db.jntuh_college_links_assigned.Where(l => l.linkId == lid && l.academicyearId == PresentYear && l.isActive == true && l.fromdate <= todayDate && l.todate >= todayDate).Select(s => s).FirstOrDefault();

            if (NOCPhase == null)
            {
                return RedirectToAction("College", "Dashboard");
            }


            var NOCTypes = db.jntuh_college_paymentoffee_type.Where(a => (a.FeeType == "NOC For Closure of College" || a.FeeType == "NOC For Closure of Course") && a.isActive == true).Select(s => new { TypeId = s.id, Type = s.FeeType }).ToList();
            int CollegeNocId = NOCTypes.Where(e => e.Type == "NOC For Closure of College").Select(a => a.TypeId).FirstOrDefault();
            int CoursesNocId = NOCTypes.Where(e => e.Type == "NOC For Closure of Course").Select(a => a.TypeId).FirstOrDefault();

            if (nocTypeId == CollegeNocId)
            {
                var jntuh_noc_data = db.jntuh_college_noc_data.Where(s => s.academicyearId == PresentYear && s.collegeId == userCollegeID && s.noctypeId == nocTypeId).Select(a => a).FirstOrDefault();

                var CoursesDetails = (from n in db.jntuh_college_intake_existing
                                      join s in db.jntuh_specialization on n.specializationId equals s.id
                                      join d in db.jntuh_department on s.departmentId equals d.id
                                      join de in db.jntuh_degree on d.degreeId equals de.id
                                      where n.collegeId == userCollegeID
                                      select new IntakeExisting
                                      {
                                          DegreeId = de.id,
                                          Degree = de.degree,
                                          DepartmentId = d.id,
                                          Department = d.departmentName,
                                          SpecializationId = s.id,
                                          Specialization = s.specializationName,
                                          shiftId = n.shiftId,
                                          ApprovedIntake = n.approvedIntake,
                                          CourseStatus = jntuh_noc_data.isClosure == true ? "Submitted" : "Not-Submitted"
                                      }).ToList();

                CoursesDetails = CoursesDetails.AsEnumerable().GroupBy(a => new { a.SpecializationId, a.shiftId }).Select(s => s.First()).ToList();

                ViewBag.Courses = CoursesDetails;
                if (CoursesDetails.Count == 0)
                    TempData["CoursesCount"] = "false";
                else
                    TempData["CoursesCount"] = "true";

                var returnUrl = WebConfigurationManager.AppSettings["NocReturnUrl"];
                var merchantId = WebConfigurationManager.AppSettings["MerchantID"];
                var securityId = WebConfigurationManager.AppSettings["SecurityID"];
                var typefield1 = WebConfigurationManager.AppSettings["TypeField1"];
                var typefield2 = WebConfigurationManager.AppSettings["TypeField2"];

                string clgCode = db.jntuh_college.Where(a => a.id == userCollegeID).Select(a => a.collegeCode).FirstOrDefault();
                ViewBag.totalFee = 45000;
                ViewBag.collegeCode = clgCode;
                ViewBag.challnNumber = clgCode + DateTime.Now.ToString("yyyMMddHHmmss");
                ViewBag.noctype = CollegeNocId;
                // ViewBag.IsPaymentDone = db.jntuh_paymentresponse.Where(s => s.AcademicYearId == PresentYear && s.CollegeId == clgCode && s.PaymentTypeID == 10 && s.noctypeId == nocTypeId && s.AuthStatus == "0300").Select(q => q.TxnAmount).Sum();

                ViewBag.IsPaymentDone = db.jntuh_paymentresponse.Count(s => s.AcademicYearId == PresentYear && s.CollegeId == clgCode && s.PaymentTypeID == CollegeNocId && s.AuthStatus == "0300") > 0 ? true : false;

                var msg = "";
                if (userCollegeID == 375)
                {
                    msg = merchantId + "|" + ViewBag.challnNumber + "|NA|2.00|NA|NA|NA|INR|NA|" + typefield1 + "|" + securityId + "|NA|NA|" + typefield2 + "|NA|NA|NA|NA|NA|NA|NA|" + returnUrl;
                    var key = WebConfigurationManager.AppSettings["ChecksumKey"];
                    msg += "|" + GetHMACSHA256(msg, key).ToUpper();
                }
                else
                {
                    msg = merchantId + "|" + ViewBag.challnNumber + "|NA|" + ViewBag.totalFee + "|NA|NA|NA|INR|NA|" + typefield1 + "|" + securityId + "|NA|NA|" + typefield2 + "|NA|NA|NA|NA|NA|NA|NA|" + returnUrl;
                    var key = WebConfigurationManager.AppSettings["ChecksumKey"];
                    msg += "|" + GetHMACSHA256(msg, key).ToUpper();
                }
                ViewBag.msg = msg;
            }
            else if (nocTypeId == CoursesNocId)
            {
                var jntuh_noc_data = db.jntuh_college_noc_data.Where(s => s.academicyearId == PresentYear && s.collegeId == userCollegeID && s.noctypeId == nocTypeId).Select(a => a).ToList();
                int CourseCount = jntuh_noc_data.Select(d => d.specializationId).Count();

                var CoursesDetails = (from n in jntuh_noc_data
                                      join s in db.jntuh_specialization on n.specializationId equals s.id
                                      join d in db.jntuh_department on n.departmentId equals d.id
                                      join de in db.jntuh_degree on d.degreeId equals de.id
                                      select new IntakeExisting
                                      {
                                          nocid = n.id,
                                          DegreeId = de.id,
                                          Degree = de.degree,
                                          DepartmentId = d.id,
                                          Department = d.departmentName,
                                          SpecializationId = s.id,
                                          Specialization = s.specializationName,
                                          shiftId = n.shiftId,
                                          ApprovedIntake = n.approvedIntake,
                                          CourseStatus = n.isClosure == true ? "Submitted" : "Not-Submitted"
                                      }).ToList();

                ViewBag.Courses = CoursesDetails;
                if (CoursesDetails.Count == 0)
                    TempData["CoursesCount"] = "false";
                else
                    TempData["CoursesCount"] = "true";

                var returnUrl = WebConfigurationManager.AppSettings["NocReturnUrl"];
                var merchantId = WebConfigurationManager.AppSettings["MerchantID"];
                var securityId = WebConfigurationManager.AppSettings["SecurityID"];
                var typefield1 = WebConfigurationManager.AppSettings["TypeField1"];
                var typefield2 = WebConfigurationManager.AppSettings["TypeField2"];

                string clgCode = db.jntuh_college.Where(a => a.id == userCollegeID).Select(a => a.collegeCode).FirstOrDefault();

                var payment = db.jntuh_paymentresponse.Where(s => s.AcademicYearId == PresentYear && s.CollegeId == clgCode && s.PaymentTypeID == CoursesNocId && s.AuthStatus == "0300").Select(q => q).ToList();

                ViewBag.totalFee = payment.Count() > 0 ? ((CourseCount * 25000) - payment.Select(a => a.TxnAmount).Sum()) : (CourseCount * 25000);
                ViewBag.collegeCode = clgCode;
                ViewBag.challnNumber = clgCode + DateTime.Now.ToString("yyyMMddHHmmss");
                ViewBag.noctype = CoursesNocId;
                ViewBag.IsPaymentDone = false;
                if ((CourseCount * 25000) == payment.Select(a => a.TxnAmount).Sum())
                {
                    ViewBag.IsPaymentDone = true;
                }
                //ViewBag.IsPaymentDone = payment.Select(a => a.TxnAmount).Sum();

                //ViewBag.PaymentStatus = null;
                //if (payment != null || payment != 0)
                // ViewBag.PaymentStatus = true;
                // ViewBag.IsPaymentDone = db.jntuh_paymentresponse.Count(s => s.AcademicYearId == PresentYear && s.CollegeId == clgCode && s.PaymentTypeID == 10 && s.noctypeId == nocTypeId && s.AuthStatus == "0300") > 0;

                var msg = "";
                if (userCollegeID == 375)
                {
                    msg = merchantId + "|" + ViewBag.challnNumber + "|NA|2.00|NA|NA|NA|INR|NA|" + typefield1 + "|" + securityId + "|NA|NA|" + typefield2 + "|NA|NA|NA|NA|NA|NA|NA|" + returnUrl;
                    var key = WebConfigurationManager.AppSettings["ChecksumKey"];
                    msg += "|" + GetHMACSHA256(msg, key).ToUpper();
                }
                else
                {
                    msg = merchantId + "|" + ViewBag.challnNumber + "|NA|" + ViewBag.totalFee + "|NA|NA|NA|INR|NA|" + typefield1 + "|" + securityId + "|NA|NA|" + typefield2 + "|NA|NA|NA|NA|NA|NA|NA|" + returnUrl;
                    var key = WebConfigurationManager.AppSettings["ChecksumKey"];
                    msg += "|" + GetHMACSHA256(msg, key).ToUpper();
                }
                ViewBag.msg = msg;
            }
            return View();
        }

        [HttpPost]
        // [Authorize(Roles = "Admin,College")]
        public ActionResult FeeDetailsandPayment(string msg)
        {
            //return RedirectToAction("CollegeDashboard", "Dashboard");
            var appSettings = WebConfigurationManager.AppSettings;
            var req = new jntuh_paymentrequests();
            SaveResponse(msg, "ChallanNumber");
            return RedirectToAction("College_NocAction");
        }

        public ActionResult Delete(int? Id)
        {
            // return RedirectToAction("CollegeDashboard", "Dashboard");
            int userId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(collegeUser => collegeUser.userID == userId).Select(collegeUser => collegeUser.collegeID).FirstOrDefault();
            int TypeId = 0;

            var NOCTypes = db.jntuh_college_paymentoffee_type.Where(a => (a.FeeType == "NOC For Closure of College" || a.FeeType == "NOC For Closure of Course") && a.isActive == true).Select(s => new { TypeId = s.id, Type = s.FeeType }).ToList();
            int CollegeNocId = NOCTypes.Where(e => e.Type == "NOC For Closure of College").Select(a => a.TypeId).FirstOrDefault();
            int CoursesNocId = NOCTypes.Where(e => e.Type == "NOC For Closure of Course").Select(a => a.TypeId).FirstOrDefault();

            if (Id != null)
            {
                var courseObject = db.jntuh_college_noc_data.Where(a => a.id == Id).Select(s => s).FirstOrDefault();
                TypeId = courseObject.noctypeId;
                db.jntuh_college_noc_data.Remove(courseObject);
                db.SaveChanges();
                if (TypeId == CoursesNocId)
                    TempData["SUCCESS"] = "Closure Course Request is Deleted Successfully.";
                else
                    TempData["SUCCESS"] = "Closure College Request is Deleted Successfully.";
            }

            if (TypeId == CoursesNocId)
                return RedirectToAction("FeeDetailsandPayment", new { CollegeId = UAAAS.Models.Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]), nocTypeId = CoursesNocId });
            else
                return RedirectToAction("College_NocAction", new { CollegeId = UAAAS.Models.Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]) });
        }

        public ActionResult DownloadAcknowlegement(int? noctypeid)
        {
            int userId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(collegeUser => collegeUser.userID == userId).Select(collegeUser => collegeUser.collegeID).FirstOrDefault();

            string path = DownloadPath(userCollegeID, noctypeid);
            path = path.Replace("/", "\\");
            if (!string.IsNullOrEmpty(path))
            {
                return File(path, "application/pdf", path.Substring(path.LastIndexOf('\\') + 1));
            }

            return View();
        }


        public string DownloadPath(int? CollegeId, int? nocid)
        {
            jntuh_college CollegeData = db.jntuh_college.Where(c => c.id == CollegeId).Select(c => c).FirstOrDefault();
            Document pdfDoc = new Document(PageSize.A4, 60, 50, 90, 50);

            // Create a new PdfWrite object, writing the output to a MemoryStream
            //open pdf
            var output = new MemoryStream();
            var writer = PdfWriter.GetInstance(pdfDoc, output);

            string path = Server.MapPath("~/Content/PDFReports/NOC/");

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            string fullPath = path + CollegeData.collegeCode.ToUpper() + "_NOC_Acknowlegement.pdf";
            PdfWriter pdfWriter = PdfWriter.GetInstance(pdfDoc, new FileStream(fullPath, FileMode.Create));

            var file = CollegeData.collegeCode.ToUpper() + "_NOC_Acknowlegement.pdf";

            fullPath = string.Format("{0}/{1}", "/Content/PDFReports/NOC/", file);

            //Open PDF Document to write data
            pdfDoc.Open();

            //Assign Html content in a string to write in PDF
            string contents = string.Empty;

            StreamReader sr;

            //Read file from server path
            sr = System.IO.File.OpenText(Server.MapPath("~/Content/NOCFile.html"));

            //store content in the variable
            contents = sr.ReadToEnd();
            sr.Close();

            jntuh_address address = db.jntuh_address.Where(a => a.collegeId == CollegeData.id && (a.addressTye.Equals("COLLEGE"))).Select(a => a).FirstOrDefault();
            jntuh_address societyAddress = db.jntuh_address.Where(ad => (ad.collegeId == CollegeData.id) && (ad.addressTye.Equals("SOCIETY"))).Select(ad => ad).FirstOrDefault();

            string district = string.Empty;
            if (address != null)
            {
                district = db.jntuh_district.Find(address.districtId).districtName;
            }
            string scheduleCollegeAddress = string.Empty;
            string scheduleCollegeAddress1 = string.Empty;

            if (address != null)
            {
                scheduleCollegeAddress = CollegeData.collegeName + ",<br />" + address.address;
                scheduleCollegeAddress = scheduleCollegeAddress + ", " + address.townOrCity;
                scheduleCollegeAddress = scheduleCollegeAddress + ", " + address.mandal;
                scheduleCollegeAddress = scheduleCollegeAddress + ",<br />" + district;
                scheduleCollegeAddress = scheduleCollegeAddress + " - " + address.pincode;
            }

            if (address != null)
            {
                scheduleCollegeAddress1 = CollegeData.collegeName + ", " + address.address;
                scheduleCollegeAddress1 = scheduleCollegeAddress1 + ", " + address.townOrCity;
                scheduleCollegeAddress1 = scheduleCollegeAddress1 + ", " + address.mandal;
                scheduleCollegeAddress1 = scheduleCollegeAddress1 + ", " + district;
                scheduleCollegeAddress1 = scheduleCollegeAddress1 + " - " + address.pincode;
            }

            string CollegeSocietyAddress = string.Empty;
            if (societyAddress != null)
            {
                CollegeSocietyAddress = CollegeData.collegeName + ", " + societyAddress.address;
                CollegeSocietyAddress = CollegeSocietyAddress + ", " + societyAddress.townOrCity;
                CollegeSocietyAddress = CollegeSocietyAddress + ", " + societyAddress.mandal;
                CollegeSocietyAddress = CollegeSocietyAddress + ", " + district;
                CollegeSocietyAddress = CollegeSocietyAddress + " - " + societyAddress.pincode;
            }
            scheduleCollegeAddress = scheduleCollegeAddress.ToUpper().Replace(",", ", ").Replace("(", " (").Replace(")", ") ");
            CollegeSocietyAddress = CollegeSocietyAddress.ToUpper().Replace(",", ", ").Replace("(", " (").Replace(")", ") ");
            contents = contents.Replace("##SOCIETY_ADDRESS##", CollegeSocietyAddress);
            contents = contents.Replace("##COLLEGE_ADDRESS##", scheduleCollegeAddress);
            contents = contents.Replace("##COLLEGE_ADDRESS1##", scheduleCollegeAddress1);

            var College = CollegeData.collegeName + "(" + CollegeData.collegeCode + ")";

            //contents = contents.Replace("##College##", College);
            contents = contents.Replace("##CurrentDate##", DateTime.Now.ToString("dd-MM-yyy"));

            var NOCTypes = db.jntuh_college_paymentoffee_type.Where(a => (a.FeeType == "NOC For Closure of College" || a.FeeType == "NOC For Closure of Course") && a.isActive == true).Select(s => new { TypeId = s.id, Type = s.FeeType }).ToList();
            int CollegeNocId = NOCTypes.Where(e => e.Type == "NOC For Closure of College").Select(a => a.TypeId).FirstOrDefault();
            int CoursesNocId = NOCTypes.Where(e => e.Type == "NOC For Closure of Course").Select(a => a.TypeId).FirstOrDefault();


            if (nocid == CollegeNocId)
            {
                contents = contents.Replace("##ClosureCase##", "Closure of College from the Academic Year 2020-21-Req.-Reg.");
                contents = contents.Replace("##NOCTYPE##", "Closure of the College");
                contents = contents.Replace("##NocType##", "<p style=''><b>CHAIRMAN /SECRETARY</b></p>");
            }
            else
            {
                contents = contents.Replace("##ClosureCase##", "Closure of Courses / Programs from the Academic Year 2020-21-Req.-Reg.");
                contents = contents.Replace("##NOCTYPE##", "Closure of the following Courses /Programs");
                contents = contents.Replace("##NocType##", "<p style='float:right;'><b>PRINCIPAL</b></p>");
            }

            contents = contents.Replace("##ClosureActivity##", Closure(CollegeId));
            contents = contents.Replace("##PaymentDetails##", PaymentDetails(CollegeId));
            // contents = contents.Replace("##CollegeName##", "TEST COLLEGE");
            contents = contents.Replace("##CollegeName##", "<p style='float:right;'><b>" + College.ToString() + "</b></p>");
            contents = contents.Replace("##Enclosures##", enclosures(nocid));



            var parsedHtmlElements = HTMLWorker.ParseToList(new StringReader(contents), null);

            bool pageRotated = false;

            //Get each array values from parsed elements and add to the PDF document
            foreach (var htmlElement in parsedHtmlElements)
            {
                try
                {
                    if (htmlElement.Equals("<textarea>"))
                    {
                        pdfDoc.NewPage();
                    }

                    if (htmlElement.Chunks.Count >= 3)
                    {
                        if (htmlElement.Chunks.Count == 4)
                        {
                            pdfDoc.SetPageSize(iTextSharp.text.PageSize.A4.Rotate());
                            pdfDoc.SetMargins(50, 50, 90, 50);
                            pageRotated = true;
                        }
                        else
                        {
                            if (pageRotated == true)
                            {
                                pdfDoc.SetPageSize(iTextSharp.text.PageSize.A4);
                                pdfDoc.SetMargins(50, 50, 90, 50);
                                pageRotated = false;
                            }
                        }
                        pdfDoc.NewPage();
                    }
                    else
                    {
                        pdfDoc.Add((IElement)htmlElement);
                    }
                }
                catch (Exception ex)
                {
                    continue;

                }
            }

            //Close your PDF
            pdfDoc.Close();
            if (pdfDoc.IsOpen())
            {
                pdfDoc.Close();
            }
            string returnpath = string.Empty;
            returnpath = fullPath;
            return returnpath;
        }

        public string Closure(int? CollegeId)
        {
            string contents = string.Empty;
            int count = 1;
            int index = 1;
            int sno = 1;
            if (CollegeId != null)
            {
                var jntuh_academivYear = db.jntuh_academic_year.ToList();
                int actualYear = jntuh_academivYear.Where(s => s.isActive == true && s.isPresentAcademicYear == true).Select(a => a.id).FirstOrDefault();
                int PresentYear = jntuh_academivYear.Where(s => s.id == (actualYear + 1)).Select(s => s.id).FirstOrDefault();

                var college = db.jntuh_college.Where(s => s.id == CollegeId).Select(a => a).FirstOrDefault();
                var jntuh_noc_data = db.jntuh_college_noc_data.Where(s => s.academicyearId == PresentYear && s.collegeId == CollegeId).Select(a => a).ToList();

                var CollegeClosure = jntuh_noc_data.Where(s => s.departmentId == 0 && s.specializationId == 0).Select(a => a).FirstOrDefault();
                var CourseClosure = jntuh_noc_data.Where(s => s.departmentId != 0 && s.specializationId != 0).Select(a => a).ToList();
                var jntuh_college_intake_existing = db.jntuh_college_intake_existing.Where(a => a.collegeId == CollegeId).Select(w => w).ToList();
                if (CollegeClosure != null)
                {
                    List<IntakeExisting> ExistingIntake = (from e in jntuh_college_intake_existing
                                                           join s in db.jntuh_specialization on e.specializationId equals s.id
                                                           join d in db.jntuh_department on s.departmentId equals d.id
                                                           join de in db.jntuh_degree on d.degreeId equals de.id
                                                           join c in db.jntuh_college on e.collegeId equals c.id
                                                           where e.collegeId == CollegeId
                                                           select new IntakeExisting
                                                           {
                                                               academicyearid = e.academicYearId,
                                                               DegreeId = de.id,
                                                               Degree = de.degree,
                                                               DepartmentId = d.id,
                                                               Department = d.departmentName,
                                                               SpecializationId = s.id,
                                                               Specialization = s.specializationName,
                                                               shiftId = e.shiftId,
                                                               ApprovedIntake = jntuh_college_intake_existing.Where(sp => sp.academicYearId == PresentYear && sp.specializationId == s.id).Select(a => a.proposedIntake).FirstOrDefault(),

                                                           }).ToList();

                    ExistingIntake = ExistingIntake.AsEnumerable().GroupBy(s => new { s.SpecializationId, s.shiftId }).Select(q => q.First()).ToList();

                    contents += "<p style='text-align:left;font-size:10px;'><b><u>a) NOC required for: Closure of College</b></u></p><br/>";
                    contents += "<table border='1' cellspacing='0' cellpadding='4' style='font-size:10px;'>";
                    contents += "<tr style='font-weight:bold;'>";
                    contents += "<th width='10%' style='text-align:center;'>S.No</th>";
                    contents += "<th width='55%' style='text-align:left;'>Course(s) recommended for Closure</th>";
                    contents += "<th width='15%' style='text-align:left;'>Current Intake</th>";
                    contents += "<th width='20%' style='text-align:left;'>Intake after Reduction/Closure</th>";
                    contents += "</tr>";

                    foreach (var item in ExistingIntake.OrderBy(a => a.Degree).ThenBy(s => s.Specialization).ToList())
                    {
                        contents += "<tr style='font-size:10px;'>";
                        contents += "<td width='10%' style='text-align:center;'>" + (index++) + "</td>";
                        contents += "<td width='55%' style='text-align:left;'>" + item.Degree + "-" + item.Specialization + "</td>";
                        int id = jntuh_college_intake_existing.Where(a => a.academicYearId == PresentYear && a.specializationId == item.SpecializationId).Select(s => s.id).FirstOrDefault();
                        if (id != 0)
                        {
                            var inkatenew = jntuh_college_intake_existing.Where(a => a.academicYearId == PresentYear && a.specializationId == item.SpecializationId).Select(s => s.proposedIntake).FirstOrDefault();
                            contents += "<td width='15%' style='text-align:left;'>" + inkatenew + "</td>";
                        }
                        else
                        {
                            int appintake = jntuh_college_intake_existing.Where(a => a.specializationId == item.SpecializationId).Max(s => s.aicteApprovedIntake);
                            contents += "<td width='15%' style='text-align:left;'>" + appintake + "</td>";
                        }
                        contents += "<td width='20%' style='text-align:left;'>0</td>";
                        contents += "</tr>";
                    }
                    contents += "</table>";
                }

                if (CourseClosure.Count() != 0 && CourseClosure.Count() != null && CollegeClosure == null)
                {
                    contents += "<p style='text-align:left;font-size:10px;'><b><u>a) NOC required for: Closure of Courses</b></u></p><br/>";
                    contents += "<table border='1' cellspacing='0' cellpadding='4' style='font-size:10px;'>";
                    contents += "<tr style='font-weight:bold;'>";
                    contents += "<th width='10%' style='text-align:center;'>S.No</th>";
                    contents += "<th width='55%' style='text-align:left;'>Course(s) recommended for closure</th>";
                    contents += "<th width='15%' style='text-align:left;'>Current Intake</th>";
                    contents += "<th width='20%' style='text-align:left;'>Intake after Reduction/Closure</th>";
                    contents += "</tr>";

                    var CoursesDetails = (from n in CourseClosure
                                          join s in db.jntuh_specialization on n.specializationId equals s.id
                                          join d in db.jntuh_department on n.departmentId equals d.id
                                          join de in db.jntuh_degree on d.degreeId equals de.id
                                          select new IntakeExisting
                                          {
                                              DegreeId = de.id,
                                              Degree = de.degree,
                                              DepartmentId = d.id,
                                              Department = d.departmentName,
                                              SpecializationId = s.id,
                                              Specialization = s.specializationName,
                                              CourseStatus = n.isClosure == true ? "Submitted" : "Active"
                                          }).ToList();

                    foreach (var item in CoursesDetails)
                    {
                        contents += "<tr style='font-size:10px;'>";
                        contents += "<td width='10%' style='text-align:center;'>" + (index++) + "</td>";
                        contents += "<td width='55%' style='text-align:left;'>" + item.Degree + "-" + item.Specialization + "</td>";
                        int id = jntuh_college_intake_existing.Where(a => a.academicYearId == PresentYear && a.specializationId == item.SpecializationId).Select(s => s.id).FirstOrDefault();
                        if (id != 0)
                        {
                            var inkatenew = jntuh_college_intake_existing.Where(a => a.academicYearId == PresentYear && a.specializationId == item.SpecializationId).Select(s => s.proposedIntake).FirstOrDefault();
                            contents += "<td width='15%' style='text-align:left;'>" + inkatenew + "</td>";
                        }
                        else
                        {
                            int appintake = jntuh_college_intake_existing.Where(a => a.specializationId == item.SpecializationId).Max(s => s.aicteApprovedIntake);
                            contents += "<td width='15%' style='text-align:left;'>" + appintake + "</td>";
                        }
                        contents += "<td width='20%' style='text-align:left;'>0</td>";
                        contents += "</tr>";
                    }
                    contents += "</table>";
                }
                return contents;
            }
            return contents;
        }

        public string PaymentDetails(int? CollegeId)
        {
            string contents = string.Empty;
            int count = 1;
            int index = 1;
            int sno = 1;
            if (CollegeId != null)
            {
                var college = db.jntuh_college.Where(s => s.id == CollegeId).Select(a => a).FirstOrDefault();
                var jntuh_academivYear = db.jntuh_academic_year.ToList();

                int actualYear = jntuh_academivYear.Where(s => s.isActive == true && s.isPresentAcademicYear == true).Select(a => a.id).FirstOrDefault();
                int PresentYear = jntuh_academivYear.Where(s => s.id == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
                var jntuh_noc_data = db.jntuh_college_noc_data.Where(s => s.academicyearId == PresentYear && s.collegeId == CollegeId).Select(a => a).ToList();

                var CollegeClosure = jntuh_noc_data.Where(s => s.departmentId == 0 && s.specializationId == 0).Select(a => a).FirstOrDefault();
                var CourseClosure = jntuh_noc_data.Where(s => s.departmentId != 0 && s.specializationId != 0).Select(a => a).ToList();

                var jntuh_college_intake_existing = db.jntuh_college_intake_existing.Where(a => a.collegeId == CollegeId).Select(w => w).ToList();

                var NOCTypes = db.jntuh_college_paymentoffee_type.Where(a => (a.FeeType == "NOC For Closure of College" || a.FeeType == "NOC For Closure of Course") && a.isActive == true).Select(s => new { TypeId = s.id, Type = s.FeeType }).ToList();
                int CollegeNocId = NOCTypes.Where(e => e.Type == "NOC For Closure of College").Select(a => a.TypeId).FirstOrDefault();
                int CoursesNocId = NOCTypes.Where(e => e.Type == "NOC For Closure of Course").Select(a => a.TypeId).FirstOrDefault();

                if (CollegeClosure != null)
                {
                    var jntuh_payments = db.jntuh_paymentresponse.Where(s => s.AcademicYearId == PresentYear && s.CollegeId == college.collegeCode && s.AuthStatus == "0300" && s.PaymentTypeID == CollegeNocId).FirstOrDefault();

                    contents += "<p style='text-align:left;font-size:10px;'><b><u>b) Details of payment:</b></u></p><br/>";
                    contents += "<table border='1' cellspacing='0' cellpadding='4' style='font-size:10px;'>";
                    contents += "<tr style='font-weight:bold;'>";
                    contents += "<th width='10%' style='text-align:center;'>S.No</th>";
                    //contents += "<th width='55%' style='text-align:left;'>Course(s) recommended for closure</th>";
                    contents += "<th width='55%' style='text-align:left;'>Payment Date</th>";
                    contents += "<th width='20%' style='text-align:left;'>Reference Number</th>";
                    contents += "<th width='15%' style='text-align:left;'>Paid Amount</th>";

                    contents += "</tr>";

                    contents += "<tr style='font-size:10px;'>";
                    contents += "<td width='10%' style='text-align:center;'>1</td>";
                    // contents += "<td width='55%' style='text-align:left;'>" + college.collegeName + "(" + college.collegeCode +")"+ "</td>";
                    contents += "<td width='55%' style='text-align:left;'>" + jntuh_payments.TxnDate.ToString("dd-MM-yyyy") + "</td>";
                    contents += "<td width='20%' style='text-align:left;'>" + jntuh_payments.TxnReferenceNo + "</td>";
                    contents += "<td width='15%' style='text-align:left;'>" + jntuh_payments.TxnAmount + "</td>";

                    contents += "</tr>";
                    contents += "</table>";

                    contents += "<br/>";
                }
                else if (CourseClosure.Count() != 0 && CourseClosure.Count() != null && CollegeClosure == null)
                {
                    var jntuh_payments = db.jntuh_paymentresponse.Where(s => s.AcademicYearId == PresentYear && s.CollegeId == college.collegeCode && s.AuthStatus == "0300" && s.PaymentTypeID == CoursesNocId).ToList();

                    var CoursesDetails = (from n in CourseClosure
                                          join s in db.jntuh_specialization on n.specializationId equals s.id
                                          join d in db.jntuh_department on n.departmentId equals d.id
                                          join de in db.jntuh_degree on d.degreeId equals de.id
                                          select new IntakeExisting
                                          {
                                              DegreeId = de.id,
                                              Degree = de.degree,
                                              DepartmentId = d.id,
                                              Department = d.departmentName,
                                              SpecializationId = s.id,
                                              Specialization = s.specializationName,
                                              CourseStatus = n.isClosure == true ? "Submitted" : "Active"
                                          }).ToList();

                    var coursescount = CoursesDetails.Select(a => a.SpecializationId).Count();
                    contents += "<br/>";
                    contents += "<p style='text-align:left;font-size:10px;'><b><u>b) Details of Payment :</b></u></p><br/>";
                    contents += "<table border='1' cellspacing='0' cellpadding='3' style='font-size:10px;'>";
                    contents += "<thead>";
                    contents += "<tr style='font-weight:bold;'>";
                    contents += "<th width='10%' style='text-align:center;'>S.No</th>";
                    contents += "<th width='30%' style='text-align:left;'>Payment Date</th>";
                    contents += "<th width='40%' style='text-align:left;'>Reference Number</th>";
                    contents += "<th width='20%' style='text-align:left;'>Paid Amount</th>";
                    contents += "</thead>";
                    contents += "</tr>";

                    contents += "<tbody>";
                    foreach (var item1 in jntuh_payments)
                    {
                        contents += "<tr>";
                        contents += "<td width='10%' style='text-align:center;'>" + (count++) + "</td>";
                        contents += "<td width='30%' style='text-align:left;'>" + item1.TxnDate.ToString("dd-MM-yyyy") + "</td>";
                        contents += "<td width='40%' style='text-align:left;'>" + item1.TxnReferenceNo + "</td>";
                        contents += "<td width='20%' style='text-align:left;'>" + item1.TxnAmount + "</td>";
                        contents += "</tr>";
                    }
                    contents += "</tbody>";
                    contents += "</table>";
                }
                return contents;
            }
            return contents;
        }

        public string enclosures(int? nocTypeid)
        {
            string contents = string.Empty;
            int index = 1;
            contents += "<p style='font-size:10px;'><b>Enclosures:</b></p>";
            var noc_enclosures = db.jntuh_college_noc_enclosures.Where(s => s.isActive == true && s.noctypeid == nocTypeid).Select(s => s).OrderBy(a => a.id).ToList();

            contents += "<table border='0' cellspacing='0' cellpadding='4'  width='100%' style='font-size:10px;'>";
            foreach (var item in noc_enclosures)
            {
                contents += "<tr>";
                contents += "<td width='10%' style='text-align:center;'>" + (index++) + "</td>";
                contents += "<td width='90%' style='text-align:left;'>" + item.formName + "</td>";
                contents += "</tr>";
            }
            contents += "</table>";
            return contents;
        }

        [HttpPost]
        public ActionResult SavePaymentRequest(string challanNumber, decimal txnAmount, string collegeCode, int? noctype)
        {
            var jntuh_academivYear = db.jntuh_academic_year.ToList();
            int actualYear = jntuh_academivYear.Where(s => s.isActive == true && s.isPresentAcademicYear == true).Select(a => a.id).FirstOrDefault();
            int PresentYear = jntuh_academivYear.Where(s => s.id == (actualYear + 1)).Select(s => s.id).FirstOrDefault();

            var appSettings = WebConfigurationManager.AppSettings;
            var req = new jntuh_paymentrequests();
            req.AcademicYearId = PresentYear;
            req.TxnAmount = txnAmount;
            req.CollegeCode = collegeCode;

            req.ChallanNumber = challanNumber;
            req.MerchantID = appSettings["MerchantID"];
            req.CustomerID = appSettings["CustomerID"];
            req.SecurityID = appSettings["SecurityID"];
            req.CurrencyType = appSettings["CurrencyType"];
            req.TxnDate = DateTime.Now;
            req.PaymentTypeID = Convert.ToInt32(noctype); ;
            db.jntuh_paymentrequests.Add(req);
            db.SaveChanges();
            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }

        private void SaveResponse(string responseMsg, string challanno)
        {
            var tokens = responseMsg.Split('|');
            int userID = 0;
            int userCollegeID = 0;
            string clgCode = string.Empty;
            if (Membership.GetUser() == null)
            {
                //return RedirectToAction("LogOn", "Account");
                string cid = tokens[1];
                clgCode = cid.Substring(0, 2);
                userCollegeID =
                    db.jntuh_college.Where(c => c.collegeCode == clgCode.Trim()).Select(s => s.id).FirstOrDefault();
                userID = db.jntuh_college_users.Where(u => u.collegeID == userCollegeID).Select(u => u.userID).FirstOrDefault();
            }
            else
            {
                userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
                userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
                clgCode = db.jntuh_college.SingleOrDefault(a => a.id == userCollegeID).collegeCode;
            }

            //Response Message Saving
            temp_aeronautical temp_aeronautical = new temp_aeronautical();
            temp_aeronautical.Degree = responseMsg.Length < 255
                ? responseMsg
                : responseMsg.Substring(0, 254); ;
            temp_aeronautical.Department = "NOC";
            temp_aeronautical.Specialization = clgCode;
            temp_aeronautical.DegreeId = userCollegeID;
            temp_aeronautical.LabCode = DateTime.Now.ToString();
            db.temp_aeronautical.Add(temp_aeronautical);
            db.SaveChanges();

            var jntuh_academivYear = db.jntuh_academic_year.ToList();
            int actualYear = jntuh_academivYear.Where(s => s.isActive == true && s.isPresentAcademicYear == true).Select(a => a.id).FirstOrDefault();
            int PresentYear = jntuh_academivYear.Where(s => s.id == (actualYear + 1)).Select(s => s.id).FirstOrDefault();

            var dbResponse = new UAAAS.Models.jntuh_paymentresponse();
            dbResponse.AcademicYearId = PresentYear;
            dbResponse.MerchantID = tokens[0];
            dbResponse.CustomerID = tokens[1];
            dbResponse.TxnReferenceNo = tokens[2];
            dbResponse.BankReferenceNo = tokens[3];
            dbResponse.TxnAmount = decimal.Parse(tokens[4]);
            dbResponse.BankID = tokens[5];
            dbResponse.BankMerchantID = tokens[6];
            dbResponse.TxnType = tokens[7];
            dbResponse.CurrencyName = tokens[8];
            dbResponse.TxnDate = DateTime.Now;
            dbResponse.AuthStatus = tokens[14];
            dbResponse.SettlementType = tokens[15];
            dbResponse.ErrorStatus = tokens[23];
            dbResponse.ErrorDescription = tokens[24];
            dbResponse.CollegeId = clgCode;
            dbResponse.ChallanNumber = challanno;
            dbResponse.PaymentTypeID = Convert.ToInt32(db.jntuh_paymentrequests.Where(a => a.CollegeCode == clgCode && a.ChallanNumber == dbResponse.CustomerID).Select(a => a.PaymentTypeID).FirstOrDefault());
            // dbResponse.noctypeId = db.jntuh_paymentrequests.Where(a => a.CollegeCode == clgCode && a.ChallanNumber == dbResponse.CustomerID).Select(a => a.noctypeId).FirstOrDefault();
            db.jntuh_paymentresponse.Add(dbResponse);
            db.SaveChanges();
            TempData["SUCCESS"] = "Your Payment is Done";


            string collegeid = db.jntuh_paymentresponse.Where(a => a.CollegeId == clgCode && a.ChallanNumber == dbResponse.CustomerID && a.PaymentTypeID == dbResponse.PaymentTypeID && a.AuthStatus == "0300").Select(a => a.CollegeId).FirstOrDefault();
            if (!string.IsNullOrEmpty(collegeid))
            {
                var jntuh_college_noc_data = db.jntuh_college_noc_data.Where(q => q.academicyearId == PresentYear && q.collegeId == userCollegeID && q.noctypeId == dbResponse.PaymentTypeID && q.isClosure == false).Select(a => a).ToList();
                foreach (var item in jntuh_college_noc_data)
                {
                    item.isClosure = true;
                    db.Entry(item).State = System.Data.EntityState.Modified;
                    db.SaveChanges();
                }
            }

            //mail
            var collegename = db.jntuh_college.SingleOrDefault(a => a.id == userCollegeID).collegeName;
            var membershipmailid = db.my_aspnet_membership.Where(i => i.userId == userID).FirstOrDefault().Email;
            IUserMailer mailer = new UserMailer();
            mailer.PaymentResponse(membershipmailid, "Payment Response", dbResponse.CollegeId + " / " + collegename, dbResponse.CustomerID, dbResponse.TxnReferenceNo, dbResponse.BankReferenceNo, dbResponse.TxnAmount, dbResponse.TxnDate.ToString(), dbResponse.ErrorDescription, dbResponse.ChallanNumber, "Payment Response", "JNTUH-AAC-ONLINE APPLICATION PAYMENT STATUS").SendAsync();
        }

        public static string GetHMACSHA256(string text, string key)
        {
            UTF8Encoding encoder = new UTF8Encoding();

            byte[] hashValue;
            byte[] keybyt = encoder.GetBytes(key);
            byte[] message = encoder.GetBytes(text);

            HMACSHA256 hashString = new HMACSHA256(keybyt);
            string hex = "";

            hashValue = hashString.ComputeHash(message);
            foreach (byte x in hashValue)
            {
                hex += String.Format("{0:x2}", x);
            }
            return hex;
        }

        #region Adding New Course and Increase Intake
        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult Intakechange()
        {
            if (Membership.GetUser() == null)
            {
                return RedirectToAction("LogOn", "Account");
            }
            DateTime todayDate = DateTime.Now;
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID =
                db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            //if Test college Login we get college id from web.config file
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            var jntuh_academic_year = db.jntuh_academic_year.Select(s => s).ToList();
            int actualYear = jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int ay0 = jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();
            int lid =
                db.jntuh_link_screens.Where(p => p.linkName == "College Course Add and Intake Increase" && p.isActive == true).Select(s => s.id).FirstOrDefault();
            jntuh_college_links_assigned scmphase =
                db.jntuh_college_links_assigned.Where(l => l.linkId == lid && l.academicyearId == ay0 && l.isActive == true && l.fromdate <= todayDate && l.todate >= todayDate).Select(s => s).FirstOrDefault();

            if (scmphase == null)
            {
                return RedirectToAction("College", "Dashboard");
            }

            List<jntuh_academic_year> jntuh_academic_years = db.jntuh_academic_year.AsNoTracking().ToList();
            ViewBag.collegeId =
                UAAAS.Models.Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"])
                    .ToString();
            ViewBag.AcademicYear =
                jntuh_academic_years.Where(a => a.isActive == true && a.isPresentAcademicYear == true)
                    .Select(a => a.academicYear)
                    .FirstOrDefault();

            ViewBag.NextYear = String.Format("{0}-{1}", (actualYear + 1).ToString(),
                (actualYear + 2).ToString().Substring(2, 2));
            int AY0 =
                jntuh_academic_years.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();



            ViewBag.FirstYear = String.Format("{0}-{1}", (actualYear).ToString(),
                (actualYear + 1).ToString().Substring(2, 2));
            ViewBag.SecondYear = String.Format("{0}-{1}", (actualYear - 1).ToString(),
                (actualYear).ToString().Substring(2, 2));
            ViewBag.ThirdYear = String.Format("{0}-{1}", (actualYear - 2).ToString(),
                (actualYear - 1).ToString().Substring(2, 2));
            ViewBag.FourthYear = String.Format("{0}-{1}", (actualYear - 3).ToString(),
                (actualYear - 2).ToString().Substring(2, 2));
            ViewBag.FifthYear = String.Format("{0}-{1}", (actualYear - 4).ToString(),
                (actualYear - 3).ToString().Substring(2, 2));

            int presentYear =
                jntuh_academic_years.Where(a => a.isActive == true && a.isPresentAcademicYear == true)
                    .Select(a => a.actualYear)
                    .FirstOrDefault();
            int AY1 = jntuh_academic_years.Where(a => a.actualYear == (presentYear)).Select(a => a.id).FirstOrDefault();
            int AY2 =
                jntuh_academic_years.Where(a => a.actualYear == (presentYear - 1)).Select(a => a.id).FirstOrDefault();
            int AY3 =
                jntuh_academic_years.Where(a => a.actualYear == (presentYear - 2)).Select(a => a.id).FirstOrDefault();
            int AY4 =
                jntuh_academic_years.Where(a => a.actualYear == (presentYear - 3)).Select(a => a.id).FirstOrDefault();
            int AY5 =
                jntuh_academic_years.Where(a => a.actualYear == (presentYear - 4)).Select(a => a.id).FirstOrDefault();

            //List<jntuh_college_intake_existing> intake = db.jntuh_college_intake_existing.Where(i => i.collegeId == userCollegeID && i.academicYearId == PresentAcademicYear).ToList();
            int[] inactivespids = db.jntuh_specialization.Where(s => s.isActive == false).Select(s => s.id).ToArray();
            //int[] academicyearids = { AY0,AY1, AY2, AY3, AY4 };

            List<jntuh_college_intake_existing> intake =
                db.jntuh_college_intake_existing.Where(
                    i => i.collegeId == userCollegeID && !inactivespids.Contains(i.specializationId)).ToList();

            List<CollegeIntakeExisting> collegeIntakeExisting = new List<CollegeIntakeExisting>();

            var jntuh_specialization = db.jntuh_specialization;
            var jntuh_department = db.jntuh_department;
            var jntuh_degree = db.jntuh_degree;
            var jntuh_shift = db.jntuh_shift;
            var jntuh_college_noc_data =
                db.jntuh_college_noc_data.Where(n => n.collegeId == userCollegeID && n.isClosure == true)
                    .Select(s => s)
                    .ToList();
            foreach (var item in intake)
            {
                CollegeIntakeExisting newIntake = new CollegeIntakeExisting();
                newIntake.isClosed = false;
                int nocdataid =
               db.jntuh_college_noc_data.Where(
                   r =>
                       r.collegeId == userCollegeID && r.academicyearId == AY0 &&
                       (r.noctypeId == 18) && r.specializationId == item.specializationId).Select(s => s.id).FirstOrDefault();
                newIntake.id = item.id;
                newIntake.nocid = nocdataid;
                newIntake.collegeId = item.collegeId;
                newIntake.academicYearId = item.academicYearId;
                newIntake.shiftId = item.shiftId;
                newIntake.isActive = item.isActive;
                newIntake.nbaFrom = item.nbaFrom;
                newIntake.nbaTo = item.nbaTo;
                newIntake.specializationId = item.specializationId;
                newIntake.Specialization =
                    jntuh_specialization.Where(s => s.id == item.specializationId)
                        .Select(s => s.specializationName)
                        .FirstOrDefault();
                newIntake.DepartmentID =
                    jntuh_specialization.Where(s => s.id == item.specializationId)
                        .Select(s => s.departmentId)
                        .FirstOrDefault();
                newIntake.Department =
                    jntuh_department.Where(d => d.id == newIntake.DepartmentID)
                        .Select(d => d.departmentName)
                        .FirstOrDefault();
                newIntake.degreeID =
                    jntuh_department.Where(d => d.id == newIntake.DepartmentID).Select(d => d.degreeId).FirstOrDefault();
                newIntake.Degree =
                    jntuh_degree.Where(d => d.id == newIntake.degreeID).Select(d => d.degree).FirstOrDefault();
                newIntake.degreeDisplayOrder =
                    jntuh_degree.Where(d => d.id == newIntake.degreeID)
                        .Select(d => d.degreeDisplayOrder)
                        .FirstOrDefault();
                newIntake.shiftId = item.shiftId;
                newIntake.Shift = jntuh_shift.Where(s => s.id == item.shiftId).Select(s => s.shiftName).FirstOrDefault();

                int collegecid = jntuh_college_noc_data.Where(d => d.noctypeId == 15)
                    .Select(s => s.id)
                    .FirstOrDefault();
                if (collegecid == 0)
                {
                    int closedid =
                        jntuh_college_noc_data.Where(
                            d => d.specializationId == item.specializationId && d.shiftId == item.shiftId && d.noctypeId == 16)
                            .Select(s => s.id)
                            .FirstOrDefault();
                    if (closedid != 0)
                    {
                        newIntake.isClosed = true;
                    }
                }
                else
                {
                    newIntake.isClosed = true;
                }
                collegeIntakeExisting.Add(newIntake);
            }

            //From Get The Data NOC Data
            var nocdata =
                db.jntuh_college_noc_data.Where(
                    r =>
                        r.collegeId == userCollegeID && r.academicyearId == AY0 &&
                        (r.noctypeId == 17)).Select(s => s).ToList();
            foreach (var item in nocdata)
            {
                CollegeIntakeExisting newIntake = new CollegeIntakeExisting();
                newIntake.isClosed = false;
                newIntake.nocid = item.id;
                newIntake.collegeId = item.collegeId;
                newIntake.academicYearId = item.academicyearId;
                newIntake.shiftId = item.shiftId != null ? (int)item.shiftId : 0;
                newIntake.isActive = item.isActive != null ? (bool)item.isActive : false;

                newIntake.specializationId = item.specializationId;
                newIntake.Specialization =
                    jntuh_specialization.Where(s => s.id == item.specializationId)
                        .Select(s => s.specializationName)
                        .FirstOrDefault();
                newIntake.DepartmentID =
                    jntuh_specialization.Where(s => s.id == item.specializationId)
                        .Select(s => s.departmentId)
                        .FirstOrDefault();
                newIntake.Department =
                    jntuh_department.Where(d => d.id == newIntake.DepartmentID)
                        .Select(d => d.departmentName)
                        .FirstOrDefault();
                newIntake.degreeID =
                    jntuh_department.Where(d => d.id == newIntake.DepartmentID).Select(d => d.degreeId).FirstOrDefault();
                newIntake.Degree =
                    jntuh_degree.Where(d => d.id == newIntake.degreeID).Select(d => d.degree).FirstOrDefault();
                newIntake.degreeDisplayOrder =
                    jntuh_degree.Where(d => d.id == newIntake.degreeID)
                        .Select(d => d.degreeDisplayOrder)
                        .FirstOrDefault();

                newIntake.Shift = jntuh_shift.Where(s => s.id == item.shiftId).Select(s => s.shiftName).FirstOrDefault();

                int collegecid = jntuh_college_noc_data.Where(d => d.noctypeId == 15)
                    .Select(s => s.id)
                    .FirstOrDefault();
                if (collegecid == 0)
                {
                    int closedid =
                        jntuh_college_noc_data.Where(
                            d => d.specializationId == item.specializationId && d.shiftId == item.shiftId && d.noctypeId == 16)
                            .Select(s => s.id)
                            .FirstOrDefault();
                    if (closedid != 0)
                    {
                        newIntake.isClosed = true;
                    }
                }
                else
                {
                    newIntake.isClosed = true;
                }
                collegeIntakeExisting.Add(newIntake);
            }


            var jntuh_college_intake_existing = db.jntuh_college_intake_existing.Where(r => r.collegeId == userCollegeID).Select(s => s).ToList();

            collegeIntakeExisting =
                collegeIntakeExisting.AsEnumerable()
                    .GroupBy(r => new { r.specializationId, r.shiftId })
                    .Select(r => r.First())
                    .ToList();
            foreach (var item in collegeIntakeExisting)
            {

                if (item.specializationId == 25)
                {

                }

                //FLAG :4-Admitted Intake Lateral as per Exam Branch,3-Admitted Intake Regular as per Exam Branch,2-AICTE Approved 1 - Approved, 0 - Admitted
                jntuh_college_intake_existing details = jntuh_college_intake_existing
                    .Where(
                        e =>
                            e.collegeId == userCollegeID && e.academicYearId == AY1 &&
                            e.specializationId == item.specializationId && e.shiftId == item.shiftId)
                    .Select(e => e)
                    .FirstOrDefault();
                if (details != null)
                {
                    item.ApprovedIntake = details.approvedIntake;
                }


                jntuh_college_noc_data nocdetails = db.jntuh_college_noc_data
                    .Where(
                        e =>
                            e.collegeId == userCollegeID && e.academicyearId == ay0 &&
                            e.specializationId == item.specializationId && e.shiftId == item.shiftId)
                    .Select(e => e)
                    .FirstOrDefault();
                if (nocdetails != null)
                {
                    item.ProposedIntake = nocdetails.approvedIntake;
                }

            }


            collegeIntakeExisting =
                collegeIntakeExisting.OrderBy(ei => ei.degreeDisplayOrder)
                    .ThenBy(ei => ei.Department)
                    .ThenBy(ei => ei.Specialization)
                    .ThenBy(ei => ei.shiftId)
                    .ToList();
            ViewBag.ExistingIntake = collegeIntakeExisting;
            ViewBag.Count = collegeIntakeExisting.Count();
            string clgCode = db.jntuh_college.SingleOrDefault(a => a.id == userCollegeID).collegeCode;
            var currentYear = DateTime.Now.Year;
            //ViewBag.IsPaymentDone =
            //    db.jntuh_paymentresponse.Count(
            //        it =>
            //            it.CollegeId == clgCode && it.AcademicYearId == AY0 && it.TxnDate.Year == currentYear &&
            //            it.AuthStatus == "0300") > 0;
            return View(collegeIntakeExisting);

        }
        private int GetIntake(int collegeId, int academicYearId, int specializationId, int shiftId, int flag)
        {
            int intake = 0;
            //int presentYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            //int AYID = db.jntuh_academic_year.Where(a => a.actualYear == (presentYear + 1)).Select(a => a.id).FirstOrDefault();

            //approved
            if (flag == 1)
            {
                intake =
                    db.jntuh_college_intake_existing.Where(
                        i =>
                            i.collegeId == collegeId && i.academicYearId == academicYearId &&
                            i.specializationId == specializationId && i.shiftId == shiftId)
                        .Select(i => i.approvedIntake)
                        .FirstOrDefault();

                //RAMESH: NOT REQUIRED AS PROPOSED INTAKE IS COMING FROM EXISTING TABLE ONLY

                ////to get proposedIntake vale for AY-2014-15
                //if (intake == 0 && academicYearId == AYID)
                //{
                //    intake = db.jntuh_college_intake_proposed.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == shiftId).Select(i => i.proposedIntake).FirstOrDefault();
                //}
            }
            else if (flag == 2)
            {
                intake =
                    db.jntuh_college_intake_existing.Where(
                        i =>
                            i.collegeId == collegeId && i.academicYearId == academicYearId &&
                            i.specializationId == specializationId && i.shiftId == shiftId)
                        .Select(i => i.aicteApprovedIntake)
                        .FirstOrDefault();
            }
            else if (flag == 3) //Narayana Reddy- Regular admitted Intake as per Exam Branch data
            {
                intake =
                    db.jntuh_college_intake_existing.Where(
                        i =>
                            i.collegeId == collegeId && i.academicYearId == academicYearId &&
                            i.specializationId == specializationId && i.shiftId == shiftId)
                        .Select(i => i.admittedIntakeasperExambranch_R)
                        .FirstOrDefault();
            }
            else if (flag == 4) //Narayana Reddy-lateral admitted Intake as per Exam Branch data
            {
                intake =
                    db.jntuh_college_intake_existing.Where(
                        i =>
                            i.collegeId == collegeId && i.academicYearId == academicYearId &&
                            i.specializationId == specializationId && i.shiftId == shiftId)
                        .Select(i => i.admittedIntakeasperExambranch_L)
                        .FirstOrDefault();
            }
            else //admitted
            {
                intake =
                    db.jntuh_college_intake_existing.Where(
                        i =>
                            i.collegeId == collegeId && i.academicYearId == academicYearId &&
                            i.specializationId == specializationId && i.shiftId == shiftId)
                        .Select(i => i.admittedIntake)
                        .FirstOrDefault();
            }

            return intake;
        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        [HttpGet]
        public ActionResult AddEditRecord(int? id, int? nocid, string collegeId)
        {
            CollegeIntakeExisting collegeIntakeExisting = new CollegeIntakeExisting();
            if (Membership.GetUser() == null)
            {
                return RedirectToAction("LogOn", "Account");
            }
            int typecourseid = 0;
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID =
                db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 0)
            {
                if (Roles.IsUserInRole("Admin") || Roles.IsUserInRole("DataEntry"))
                {
                    if (collegeId != null)
                    {
                        userCollegeID =
                            Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(collegeId,
                                WebConfigurationManager.AppSettings["CryptoKey"]));
                    }
                    else if (id != null)
                    {
                        userCollegeID =
                            db.jntuh_college_intake_existing.Where(i => i.id == id)
                                .Select(i => i.collegeId)
                                .FirstOrDefault();
                    }
                }
            }
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            var jntuh_academic_year = db.jntuh_academic_year.Select(s => s).ToList();
            ViewBag.IsUpdate = true;
            int enclosureId =
                db.jntuh_enclosures.Where(e => e.documentName == "AICTE INTAKE APPROVAL LETTER")
                    .Select(e => e.id)
                    .FirstOrDefault();
            var AICTEApprovalLettr =
                db.jntuh_college_enclosures.Where(e => e.enclosureId == enclosureId && e.collegeID == userCollegeID)
                    .Select(e => e.path)
                    .FirstOrDefault();
            collegeIntakeExisting.collegeId = userCollegeID;
            collegeIntakeExisting.AICTEApprovalLettr = AICTEApprovalLettr;

            ViewBag.AcademicYear =
                jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true)
                    .Select(a => a.academicYear)
                    .FirstOrDefault();
            int actualYear =
                jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true)
                    .Select(a => a.actualYear)
                    .FirstOrDefault();

            //RAMESH: ADDED to MERGE BOTH EXISTING & PROPOSED INTAKE
            ViewBag.NextYear = String.Format("{0}-{1}", (actualYear + 1).ToString(),
                (actualYear + 2).ToString().Substring(2, 2));
            ViewBag.PrevYear = String.Format("{0}-{1}", (actualYear).ToString(),
                (actualYear + 1).ToString().Substring(2, 2));
            int AY0 =
                db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();

            ViewBag.FirstYear = String.Format("{0}-{1}", (actualYear).ToString(),
                (actualYear + 1).ToString().Substring(2, 2));
            ViewBag.SecondYear = String.Format("{0}-{1}", (actualYear - 1).ToString(),
                (actualYear).ToString().Substring(2, 2));
            ViewBag.ThirdYear = String.Format("{0}-{1}", (actualYear - 2).ToString(),
                (actualYear - 1).ToString().Substring(2, 2));
            ViewBag.FourthYear = String.Format("{0}-{1}", (actualYear - 3).ToString(),
                (actualYear - 2).ToString().Substring(2, 2));
            ViewBag.FifthYear = String.Format("{0}-{1}", (actualYear - 4).ToString(),
                (actualYear - 3).ToString().Substring(2, 2));
            if (id != null)
                typecourseid = db.jntuh_college_paymentoffee_type.Where(r => r.FeeType == "New Course").Select(s => s.id).FirstOrDefault();
            else
                typecourseid =
                    db.jntuh_college_paymentoffee_type.Where(r => r.FeeType == "New Course")
                        .Select(s => s.id)
                        .FirstOrDefault();


            if (id != null)
            {
                int presentYear =
                    jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true)
                        .Select(a => a.actualYear)
                        .FirstOrDefault();
                int AY1 =
                    jntuh_academic_year.Where(a => a.actualYear == (presentYear)).Select(a => a.id).FirstOrDefault();
                int AY2 =
                    jntuh_academic_year.Where(a => a.actualYear == (presentYear - 1))
                        .Select(a => a.id)
                        .FirstOrDefault();
                int AY3 =
                    jntuh_academic_year.Where(a => a.actualYear == (presentYear - 2))
                        .Select(a => a.id)
                        .FirstOrDefault();
                int AY4 =
                    jntuh_academic_year.Where(a => a.actualYear == (presentYear - 3))
                        .Select(a => a.id)
                        .FirstOrDefault();
                int AY5 =
                    jntuh_academic_year.Where(a => a.actualYear == (presentYear - 4))
                        .Select(a => a.id)
                        .FirstOrDefault();
                var nocdata =
                    db.jntuh_college_noc_data.Where(r => r.collegeId == userCollegeID && r.academicyearId == AY0 && r.id == nocid)
                        .Select(s => s)
                        .FirstOrDefault();
                if (nocdata != null)
                {
                    collegeIntakeExisting.id = nocdata.id;
                    collegeIntakeExisting.collegeId = nocdata.collegeId;
                    collegeIntakeExisting.academicYearId = nocdata.academicyearId;
                    collegeIntakeExisting.shiftId = nocdata.shiftId != null ? (int)nocdata.shiftId : 0;
                    collegeIntakeExisting.isActive = nocdata.isActive != null ? (bool)nocdata.isActive : false;

                    collegeIntakeExisting.specializationId = nocdata.specializationId;
                    collegeIntakeExisting.Specialization =
                        db.jntuh_specialization.Where(s => s.id == nocdata.specializationId)
                            .Select(s => s.specializationName)
                            .FirstOrDefault();
                    collegeIntakeExisting.DepartmentID =
                        db.jntuh_specialization.Where(s => s.id == nocdata.specializationId)
                            .Select(s => s.departmentId)
                            .FirstOrDefault();
                    collegeIntakeExisting.Department =
                        db.jntuh_department.Where(d => d.id == collegeIntakeExisting.DepartmentID)
                            .Select(d => d.departmentName)
                            .FirstOrDefault();
                    collegeIntakeExisting.degreeID =
                        db.jntuh_department.Where(d => d.id == collegeIntakeExisting.DepartmentID)
                            .Select(d => d.degreeId)
                            .FirstOrDefault();
                    collegeIntakeExisting.Degree =
                        db.jntuh_degree.Where(d => d.id == collegeIntakeExisting.degreeID)
                            .Select(d => d.degree)
                            .FirstOrDefault();
                    collegeIntakeExisting.ProposedIntake = nocdata.approvedIntake;
                    collegeIntakeExisting.courseStatus = nocdata.remarks;
                    ViewBag.IsUpdate = true;
                }
                else
                {
                    List<jntuh_college_intake_existing> intake =
                  db.jntuh_college_intake_existing.Where(i => i.collegeId == userCollegeID && i.id == id).ToList();

                    foreach (var item in intake)
                    {
                        collegeIntakeExisting.id = item.id;
                        collegeIntakeExisting.collegeId = item.collegeId;
                        collegeIntakeExisting.academicYearId = item.academicYearId;
                        collegeIntakeExisting.shiftId = item.shiftId;
                        collegeIntakeExisting.isActive = item.isActive;
                        collegeIntakeExisting.nbaFrom = item.nbaFrom;
                        collegeIntakeExisting.nbaTo = item.nbaTo;
                        collegeIntakeExisting.specializationId = item.specializationId;
                        collegeIntakeExisting.Specialization =
                            db.jntuh_specialization.Where(s => s.id == item.specializationId)
                                .Select(s => s.specializationName)
                                .FirstOrDefault();
                        collegeIntakeExisting.DepartmentID =
                            db.jntuh_specialization.Where(s => s.id == item.specializationId)
                                .Select(s => s.departmentId)
                                .FirstOrDefault();
                        collegeIntakeExisting.Department =
                            db.jntuh_department.Where(d => d.id == collegeIntakeExisting.DepartmentID)
                                .Select(d => d.departmentName)
                                .FirstOrDefault();
                        collegeIntakeExisting.degreeID =
                            db.jntuh_department.Where(d => d.id == collegeIntakeExisting.DepartmentID)
                                .Select(d => d.degreeId)
                                .FirstOrDefault();
                        collegeIntakeExisting.Degree =
                            db.jntuh_degree.Where(d => d.id == collegeIntakeExisting.degreeID)
                                .Select(d => d.degree)
                                .FirstOrDefault();
                        collegeIntakeExisting.shiftId = item.shiftId;
                        //collegeIntakeExisting.courseStatus = item.courseStatus;
                        collegeIntakeExisting.courseStatus =
                            intake.Where(
                                i =>
                                    i.collegeId == userCollegeID && i.specializationId == item.specializationId &&
                                    i.academicYearId == AY0).Select(s => s.courseStatus).FirstOrDefault() == null
                                ? "0"
                                : intake.Where(
                                    i =>
                                        i.collegeId == userCollegeID && i.specializationId == item.specializationId &&
                                        i.academicYearId == AY0).Select(s => s.courseStatus).FirstOrDefault();
                        collegeIntakeExisting.UploadNBAApproveLetter = item.NBAApproveLetter;
                        ViewBag.NBAApprovedLetter = item.NBAApproveLetter;
                        collegeIntakeExisting.courseAffiliatedStatus = item.courseAffiliatedStatus;
                        collegeIntakeExisting.Shift =
                            db.jntuh_shift.Where(s => s.id == item.shiftId).Select(s => s.shiftName).FirstOrDefault();
                    }
                }
            }
            else
            {
                ViewBag.IsUpdate = false;
            }

            var degrees =
                db.jntuh_college_degree.Join(db.jntuh_degree, collegeDegree => collegeDegree.degreeId,
                    degree => degree.id,
                    (collegeDegree, degree) => new
                    {
                        collegeDegree.degreeId,
                        collegeDegree.collegeId,
                        collegeDegree.isActive,
                        degree.degree
                    })
                    .Where(collegeDegree => collegeDegree.collegeId == userCollegeID && collegeDegree.isActive == true)
                    .Select(collegeDegree => new
                    {
                        collegeDegree.degreeId,
                        collegeDegree.degree
                    }).ToList();
            ViewBag.Degree = degrees.OrderBy(d => d.degree);
            ViewBag.Department = db.jntuh_department.Where(d => d.isActive == true);
            ViewBag.Specialization = db.jntuh_specialization.Where(s => s.isActive == true);
            ViewBag.Shift = db.jntuh_shift.Where(s => s.isActive == true);
            ViewBag.Count = degrees.Count();

            List<SelectListItem> courseStatuslist = new List<SelectListItem>();
            courseStatuslist.Add(new SelectListItem { Value = "New", Text = "New" });
            courseStatuslist.Add(new SelectListItem { Value = "Increase", Text = "Increase" });

            ViewBag.courseStatusdata = courseStatuslist;
            if (Request.IsAjaxRequest())
            {
                return PartialView("_Create", collegeIntakeExisting);
            }
            else
            {
                return View("_Create", collegeIntakeExisting);
            }
        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        [HttpPost]
        [AllowAnonymous]
        public ActionResult AddEditRecord(CollegeIntakeExisting collegeIntakeExisting, string cmd)
        {
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int ay0 = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();
            ModelState.Clear();
            if (Membership.GetUser() == null)
            {
                return RedirectToAction("LogOn", "Account");
            }
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID =
                db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            int typecourseid = 0;
            if (userCollegeID == 0)
            {
                userCollegeID = collegeIntakeExisting.collegeId;
            }
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            if (cmd == "Add")
            {
                typecourseid = db.jntuh_college_paymentoffee_type.Where(r => r.FeeType == "New Course(s)/Program(s)").Select(s => s.id).FirstOrDefault();
                if (collegeIntakeExisting.id != 0)
                {
                    var updatedata =
                        db.jntuh_college_noc_data.Where(r => r.collegeId == userCollegeID && r.academicyearId == ay0)
                            .FirstOrDefault();
                    updatedata.academicyearId = ay0;


                    updatedata.specializationId = collegeIntakeExisting.specializationId;
                    updatedata.noctypeId = typecourseid;
                    updatedata.shiftId = collegeIntakeExisting.shiftId;
                    updatedata.approvedIntake = collegeIntakeExisting.ProposedIntake;
                    updatedata.isClosure = false;
                    updatedata.isActive = true;
                    //updatedata.remarks = "New";
                    updatedata.createdBy = null;
                    updatedata.createdOn = null;
                    updatedata.updatedBy = userID;
                    updatedata.updatedOn = DateTime.Now;
                    db.Entry(updatedata).State = EntityState.Modified;
                    db.SaveChanges();

                }
                else
                {
                    jntuh_college_noc_data nocdata = new jntuh_college_noc_data();
                    nocdata.academicyearId = ay0;
                    nocdata.collegeId = userCollegeID;
                    nocdata.departmentId = collegeIntakeExisting.DepartmentID;
                    nocdata.specializationId = collegeIntakeExisting.specializationId;
                    nocdata.noctypeId = typecourseid;
                    nocdata.shiftId = collegeIntakeExisting.shiftId;
                    nocdata.approvedIntake = collegeIntakeExisting.ProposedIntake;
                    nocdata.isClosure = false;
                    nocdata.isActive = true;
                    nocdata.remarks = "New";
                    nocdata.createdBy = userID;
                    nocdata.createdOn = DateTime.Now;
                    nocdata.updatedBy = null;
                    nocdata.updatedOn = null;
                    db.jntuh_college_noc_data.Add(nocdata);
                    db.SaveChanges();
                    TempData["Success"] = "Added successfully.";
                }

            }
            else
            {
                typecourseid = db.jntuh_college_paymentoffee_type.Where(r => r.FeeType == "Increased in Intake(s)").Select(s => s.id).FirstOrDefault();
                var updatedata = db.jntuh_college_noc_data.Where(r => r.collegeId == userCollegeID && r.academicyearId == ay0 && r.id == collegeIntakeExisting.nocid)
                            .Select(s => s).FirstOrDefault();

                if (updatedata != null)
                {
                    updatedata.academicyearId = ay0;
                    updatedata.specializationId = collegeIntakeExisting.specializationId;
                    //updatedata.noctypeId = typecourseid;
                    updatedata.shiftId = collegeIntakeExisting.shiftId;
                    updatedata.approvedIntake = collegeIntakeExisting.ProposedIntake;
                    updatedata.isClosure = false;
                    updatedata.isActive = true;
                    //updatedata.remarks = null;
                    //updatedata.createdBy = null;
                    //updatedata.createdOn = null;
                    updatedata.updatedBy = userID;
                    updatedata.updatedOn = DateTime.Now;
                    db.Entry(updatedata).State = EntityState.Modified;
                    db.SaveChanges();

                }
                else
                {
                    jntuh_college_noc_data nocdata = new jntuh_college_noc_data();
                    nocdata.academicyearId = ay0;
                    nocdata.collegeId = userCollegeID;
                    nocdata.departmentId = collegeIntakeExisting.DepartmentID;
                    nocdata.specializationId = collegeIntakeExisting.specializationId;
                    nocdata.noctypeId = typecourseid;
                    nocdata.shiftId = collegeIntakeExisting.shiftId;
                    nocdata.approvedIntake = collegeIntakeExisting.ProposedIntake;
                    nocdata.isClosure = false;
                    nocdata.isActive = true;
                    nocdata.remarks = "Increase";
                    nocdata.createdBy = userID;
                    nocdata.createdOn = DateTime.Now;
                    nocdata.updatedBy = null;
                    nocdata.updatedOn = null;
                    db.jntuh_college_noc_data.Add(nocdata);
                    db.SaveChanges();
                    TempData["Success"] = "Added successfully.";
                }

            }
            return RedirectToAction("Intakechange",
                    new
                    {
                        collegeId =
                            UAAAS.Models.Utilities.EncryptString(userCollegeID.ToString(),
                                WebConfigurationManager.AppSettings["CryptoKey"]).ToString()
                    });
        }

        public ActionResult IntakeFreeDetails(string CollegeId, int? nocTypeId)
        {
            int userId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(collegeUser => collegeUser.userID == userId).Select(collegeUser => collegeUser.collegeID).FirstOrDefault();
            if (userCollegeID == 0)
            {
                userCollegeID = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(CollegeId, WebConfigurationManager.AppSettings["CryptoKey"]));
            }

            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            if (userCollegeID == 0 && CollegeId != null)
            {
                if (Roles.IsUserInRole("Admin"))
                {
                    userCollegeID = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(CollegeId, WebConfigurationManager.AppSettings["CryptoKey"]));
                }
            }

            var todayDate = DateTime.Now;
            var jntuh_academivYear = db.jntuh_academic_year.ToList();
            int actualYear = jntuh_academivYear.Where(s => s.isActive == true && s.isPresentAcademicYear == true).Select(a => a.id).FirstOrDefault();
            int PresentYear = jntuh_academivYear.Where(s => s.id == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
            int lid = db.jntuh_link_screens.Where(p => p.linkName == "College Course Add and Intake Increase" && p.isActive == true).Select(s => s.id).FirstOrDefault();
            jntuh_college_links_assigned NOCPhase =
                db.jntuh_college_links_assigned.Where(l => l.linkId == lid && l.academicyearId == PresentYear && l.isActive == true && l.fromdate <= todayDate && l.todate >= todayDate).Select(s => s).FirstOrDefault();

            if (NOCPhase == null)
            {
                return RedirectToAction("College", "Dashboard");
            }


            var NOCTypes = db.jntuh_college_paymentoffee_type.Where(a => (a.FeeType == "New Course(s)/Program(s)" || a.FeeType == "Increased in Intake(s)") && a.isActive == true).Select(s => s.id).ToArray();
            var newcourseid = db.jntuh_college_paymentoffee_type.Where(a => (a.FeeType == "New Course(s)/Program(s)") && a.isActive == true).Select(s => s.id).FirstOrDefault();
            var Incresaecourseid = db.jntuh_college_paymentoffee_type.Where(a => (a.FeeType == "Increased in Intake(s)") && a.isActive == true).Select(s => s.id).FirstOrDefault();

            //if (nocTypeId == CollegeNocId)
            //{
            var jntuh_noc_data = db.jntuh_college_noc_data.Where(s => s.academicyearId == PresentYear && s.collegeId == userCollegeID && NOCTypes.Contains(s.noctypeId)).Select(a => a).ToList();
            var jntuh_degree = db.jntuh_degree.AsNoTracking().ToList();
            var jntuh_department = db.jntuh_department.AsNoTracking().ToList();
            var jntuh_specialization = db.jntuh_specialization.AsNoTracking().ToList();
            decimal amount = 0;
            ViewBag.IsCloser = false;
            List<IntakechangeaddDetails> listofIntakechanges = new List<IntakechangeaddDetails>();
            foreach (var item in jntuh_noc_data)
            {
                IntakechangeaddDetails intakedetails = new IntakechangeaddDetails();
                intakedetails.Id = item.id;
                intakedetails.DepartmentName =
                    jntuh_department.Where(d => d.id == item.departmentId).Select(s => s.departmentName).FirstOrDefault();
                intakedetails.SpecializationName =
                    jntuh_specialization.Where(s => s.id == item.specializationId)
                        .Select(s => s.specializationName)
                        .FirstOrDefault();
                intakedetails.ApprovedIntake = item.approvedIntake != null ? (int)item.approvedIntake : 0;
                if (newcourseid == item.noctypeId)
                {
                    intakedetails.Amount = 25000;
                    intakedetails.CourseStatus = "New";
                }
                else
                {
                    intakedetails.Amount = 25000;
                    intakedetails.CourseStatus = "Increase";
                }
                intakedetails.TotalAmount = intakedetails.TotalAmount + intakedetails.Amount;
                intakedetails.Isclouser = item.isClosure != null ? (bool)item.isClosure : false;
                ViewBag.IsCloser = intakedetails.Isclouser;
                amount = amount + intakedetails.Amount;
                listofIntakechanges.Add(intakedetails);
            }
            ViewBag.Intakechanges = listofIntakechanges;
            var returnUrl = WebConfigurationManager.AppSettings["IntakeReturnUrl"];
            var merchantId = WebConfigurationManager.AppSettings["MerchantID"];
            var securityId = WebConfigurationManager.AppSettings["SecurityID"];
            var typefield1 = WebConfigurationManager.AppSettings["TypeField1"];
            var typefield2 = WebConfigurationManager.AppSettings["TypeField2"];
            ViewBag.totalFee = amount;
            string clgCode = db.jntuh_college.Where(a => a.id == userCollegeID).Select(a => a.collegeCode).FirstOrDefault();
            ViewBag.collegeCode = clgCode;
            ViewBag.challnNumber = clgCode + DateTime.Now.ToString("yyyMMddHHmmss");
            ViewBag.paymenttypeid = newcourseid;
            decimal payedamount = db.jntuh_paymentresponse.Where(s => s.AcademicYearId == PresentYear && s.CollegeId == clgCode && NOCTypes.Contains(s.PaymentTypeID) && s.AuthStatus == "0300").Select(q => q.TxnAmount).Count() != 0 ? db.jntuh_paymentresponse.Where(s => s.AcademicYearId == PresentYear && s.CollegeId == clgCode && NOCTypes.Contains(s.PaymentTypeID) && s.AuthStatus == "0300").Select(q => q.TxnAmount).Sum() : 0;
            ViewBag.IsPaymentDone = false;
            if (payedamount < amount)
            {
                ViewBag.IsPaymentDone = false;
                ViewBag.totalFee = amount - payedamount;

            }
            else
            {
                ViewBag.IsPaymentDone = true;
            }
            var msg = "";
            if (userCollegeID == 375)
            {
                msg = merchantId + "|" + ViewBag.challnNumber + "|NA|2.00|NA|NA|NA|INR|NA|" + typefield1 + "|" + securityId + "|NA|NA|" + typefield2 + "|NA|NA|NA|NA|NA|NA|NA|" + returnUrl;
                var key = WebConfigurationManager.AppSettings["ChecksumKey"];
                msg += "|" + GetHMACSHA256(msg, key).ToUpper();
            }
            else
            {
                msg = merchantId + "|" + ViewBag.challnNumber + "|NA|" + ViewBag.totalFee + "|NA|NA|NA|INR|NA|" + typefield1 + "|" + securityId + "|NA|NA|" + typefield2 + "|NA|NA|NA|NA|NA|NA|NA|" + returnUrl;
                var key = WebConfigurationManager.AppSettings["ChecksumKey"];
                msg += "|" + GetHMACSHA256(msg, key).ToUpper();
            }
            ViewBag.msg = msg;
            return View();
        }

        public ActionResult DeleteIntakeChange(int? Id)
        {
            // return RedirectToAction("CollegeDashboard", "Dashboard");
            int userId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(collegeUser => collegeUser.userID == userId).Select(collegeUser => collegeUser.collegeID).FirstOrDefault();
            int TypeId = 0;

            var NOCTypes = db.jntuh_college_paymentoffee_type.Where(a => (a.FeeType == "New Course(s)/Program(s)" || a.FeeType == "Increased in Intake(s)") && a.isActive == true).Select(s => s.id).ToArray();
            int CoursesNewId = db.jntuh_college_paymentoffee_type.Where(a => (a.FeeType == "New Course(s)/Program(s)") && a.isActive == true).Select(s => s.id).FirstOrDefault();
            int CoursesIncId = db.jntuh_college_paymentoffee_type.Where(a => (a.FeeType == "Increased in Intake(s)") && a.isActive == true).Select(s => s.id).FirstOrDefault();
            if (Id != null)
            {
                var courseObject = db.jntuh_college_noc_data.Where(a => a.id == Id && NOCTypes.Contains(a.noctypeId) && a.isClosure == false).Select(s => s).FirstOrDefault();
                if (courseObject != null)
                {
                    TypeId = courseObject.noctypeId;
                    db.jntuh_college_noc_data.Remove(courseObject);
                    db.SaveChanges();
                    if (TypeId == CoursesNewId)
                        TempData["SUCCESS"] = "New Course(s)/Program(s) Request is Deleted Successfully.";
                    else
                        TempData["SUCCESS"] = "Increased in Intake(s) Request is Deleted Successfully.";
                }
                else
                {
                    TempData["ERROR"] = "You can not Delete this Request after Payment.";
                }
            }
            return RedirectToAction("IntakeFreeDetails", new { CollegeId = UAAAS.Models.Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]) });
        }

        [HttpPost]
        // [Authorize(Roles = "Admin,College")]
        public ActionResult IntakeFreeDetails(string msg)
        {
            //return RedirectToAction("CollegeDashboard", "Dashboard");
            var appSettings = WebConfigurationManager.AppSettings;
            var req = new jntuh_paymentrequests();
            SaveIntakeFreeResponse(msg, "ChallanNumber");
            return RedirectToAction("IntakeFreeDetails");
        }

        private void SaveIntakeFreeResponse(string responseMsg, string challanno)
        {
            var tokens = responseMsg.Split('|');
            int userID = 0;
            int userCollegeID = 0;
            string clgCode = string.Empty;
            if (Membership.GetUser() == null)
            {
                //return RedirectToAction("LogOn", "Account");
                string cid = tokens[1];
                clgCode = cid.Substring(0, 2);
                userCollegeID =
                    db.jntuh_college.Where(c => c.collegeCode == clgCode.Trim()).Select(s => s.id).FirstOrDefault();
                userID = db.jntuh_college_users.Where(u => u.collegeID == userCollegeID).Select(u => u.userID).FirstOrDefault();
            }
            else
            {
                userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
                userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
                clgCode = db.jntuh_college.SingleOrDefault(a => a.id == userCollegeID).collegeCode;
            }

            //Response Message Saving
            temp_aeronautical temp_aeronautical = new temp_aeronautical();
            temp_aeronautical.Degree = responseMsg.Length < 255
                ? responseMsg
                : responseMsg.Substring(0, 254); ;
            temp_aeronautical.Department = "Intake";
            temp_aeronautical.Specialization = clgCode;
            temp_aeronautical.DegreeId = userCollegeID;
            temp_aeronautical.LabCode = DateTime.Now.ToString();
            db.temp_aeronautical.Add(temp_aeronautical);
            db.SaveChanges();

            var jntuh_academivYear = db.jntuh_academic_year.ToList();
            int actualYear = jntuh_academivYear.Where(s => s.isActive == true && s.isPresentAcademicYear == true).Select(a => a.id).FirstOrDefault();
            int PresentYear = jntuh_academivYear.Where(s => s.id == (actualYear + 1)).Select(s => s.id).FirstOrDefault();

            var dbResponse = new UAAAS.Models.jntuh_paymentresponse();
            dbResponse.AcademicYearId = PresentYear;
            dbResponse.MerchantID = tokens[0];
            dbResponse.CustomerID = tokens[1];
            dbResponse.TxnReferenceNo = tokens[2];
            dbResponse.BankReferenceNo = tokens[3];
            dbResponse.TxnAmount = decimal.Parse(tokens[4]);
            dbResponse.BankID = tokens[5];
            dbResponse.BankMerchantID = tokens[6];
            dbResponse.TxnType = tokens[7];
            dbResponse.CurrencyName = tokens[8];
            dbResponse.TxnDate = DateTime.Now;
            dbResponse.AuthStatus = tokens[14];
            dbResponse.SettlementType = tokens[15];
            dbResponse.ErrorStatus = tokens[23];
            dbResponse.ErrorDescription = tokens[24];
            dbResponse.CollegeId = clgCode;
            dbResponse.ChallanNumber = challanno;
            dbResponse.PaymentTypeID = Convert.ToInt32(db.jntuh_paymentrequests.Where(a => a.CollegeCode == clgCode && a.ChallanNumber == dbResponse.CustomerID).Select(a => a.PaymentTypeID).FirstOrDefault());
            // dbResponse.noctypeId = db.jntuh_paymentrequests.Where(a => a.CollegeCode == clgCode && a.ChallanNumber == dbResponse.CustomerID).Select(a => a.noctypeId).FirstOrDefault();
            db.jntuh_paymentresponse.Add(dbResponse);
            db.SaveChanges();
            TempData["SUCCESS"] = "Your Payment is Done";

            var NOCTypes = db.jntuh_college_paymentoffee_type.Where(a => (a.FeeType == "New Course(s)/Program(s)" || a.FeeType == "Increased in Intake(s)") && a.isActive == true).Select(s => s.id).ToArray();
            string collegeid = db.jntuh_paymentresponse.Where(a => a.CollegeId == clgCode && a.ChallanNumber == dbResponse.CustomerID && a.PaymentTypeID == dbResponse.PaymentTypeID && a.AuthStatus == "0300" && NOCTypes.Contains(a.PaymentTypeID)).Select(a => a.CollegeId).FirstOrDefault();
            if (!string.IsNullOrEmpty(collegeid))
            {
                var jntuh_college_noc_data = db.jntuh_college_noc_data.Where(q => q.academicyearId == PresentYear && q.collegeId == userCollegeID && q.noctypeId == dbResponse.PaymentTypeID && q.isClosure == false && NOCTypes.Contains(q.noctypeId)).Select(a => a).ToList();
                foreach (var item in jntuh_college_noc_data)
                {
                    item.isClosure = true;
                    item.updatedBy = userID;
                    item.updatedOn = DateTime.Now;
                    db.Entry(item).State = System.Data.EntityState.Modified;
                    db.SaveChanges();
                }
            }

            //mail
            var collegename = db.jntuh_college.SingleOrDefault(a => a.id == userCollegeID).collegeName;
            var membershipmailid = db.my_aspnet_membership.Where(i => i.userId == userID).FirstOrDefault().Email;
            IUserMailer mailer = new UserMailer();
            mailer.PaymentResponse(membershipmailid, "Payment Response", dbResponse.CollegeId + " / " + collegename, dbResponse.CustomerID, dbResponse.TxnReferenceNo, dbResponse.BankReferenceNo, dbResponse.TxnAmount, dbResponse.TxnDate.ToString(), dbResponse.ErrorDescription, dbResponse.ChallanNumber, "Payment Response", "JNTUH-AAC-ONLINE APPLICATION PAYMENT STATUS").SendAsync();
        }


        [HttpPost]
        public ActionResult SaveIntakePaymentRequest(string challanNumber, decimal txnAmount, string collegeCode, int? noctype)
        {
            var jntuh_academivYear = db.jntuh_academic_year.ToList();
            int actualYear = jntuh_academivYear.Where(s => s.isActive == true && s.isPresentAcademicYear == true).Select(a => a.id).FirstOrDefault();
            int PresentYear = jntuh_academivYear.Where(s => s.id == (actualYear + 1)).Select(s => s.id).FirstOrDefault();

            var appSettings = WebConfigurationManager.AppSettings;
            var req = new jntuh_paymentrequests();
            req.AcademicYearId = PresentYear;
            req.TxnAmount = txnAmount;
            req.CollegeCode = collegeCode;

            req.ChallanNumber = challanNumber;
            req.MerchantID = appSettings["MerchantID"];
            req.CustomerID = appSettings["CustomerID"];
            req.SecurityID = appSettings["SecurityID"];
            req.CurrencyType = appSettings["CurrencyType"];
            req.TxnDate = DateTime.Now;
            req.PaymentTypeID = Convert.ToInt32(noctype); ;
            db.jntuh_paymentrequests.Add(req);
            db.SaveChanges();
            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }


        public ActionResult DownloadIntakeAcknowlegement()
        {
            int userId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(collegeUser => collegeUser.userID == userId).Select(collegeUser => collegeUser.collegeID).FirstOrDefault();

            string path = DownloadIntakePath(userCollegeID);
            path = path.Replace("/", "\\");
            if (!string.IsNullOrEmpty(path))
            {
                return File(path, "application/pdf", path.Substring(path.LastIndexOf('\\') + 1));
            }

            return View();
        }


        public string DownloadIntakePath(int? CollegeId)
        {
            jntuh_college CollegeData = db.jntuh_college.Where(c => c.id == CollegeId).Select(c => c).FirstOrDefault();
            Document pdfDoc = new Document(PageSize.A4, 60, 50, 90, 50);

            // Create a new PdfWrite object, writing the output to a MemoryStream
            //open pdf
            var output = new MemoryStream();
            var writer = PdfWriter.GetInstance(pdfDoc, output);

            string path = Server.MapPath("~/Content/PDFReports/NOC/");

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            string fullPath = path + CollegeData.collegeCode.ToUpper() + "_Intake_Acknowlegement.pdf";
            PdfWriter pdfWriter = PdfWriter.GetInstance(pdfDoc, new FileStream(fullPath, FileMode.Create));

            var file = CollegeData.collegeCode.ToUpper() + "_Intake_Acknowlegement.pdf";

            fullPath = string.Format("{0}/{1}", "/Content/PDFReports/NOC/", file);

            //Open PDF Document to write data
            pdfDoc.Open();

            //Assign Html content in a string to write in PDF
            string contents = string.Empty;

            StreamReader sr;

            //Read file from server path
            sr = System.IO.File.OpenText(Server.MapPath("~/Content/IntakeNOCFile.html"));

            //store content in the variable
            contents = sr.ReadToEnd();
            sr.Close();

            jntuh_address address = db.jntuh_address.Where(a => a.collegeId == CollegeData.id && (a.addressTye.Equals("COLLEGE"))).Select(a => a).FirstOrDefault();
            jntuh_address societyAddress = db.jntuh_address.Where(ad => (ad.collegeId == CollegeData.id) && (ad.addressTye.Equals("SOCIETY"))).Select(ad => ad).FirstOrDefault();

            string district = string.Empty;
            if (address != null)
            {
                district = db.jntuh_district.Find(address.districtId).districtName;
            }
            string scheduleCollegeAddress = string.Empty;
            string scheduleCollegeAddress1 = string.Empty;

            if (address != null)
            {
                scheduleCollegeAddress = CollegeData.collegeName + ",<br />" + address.address;
                scheduleCollegeAddress = scheduleCollegeAddress + ", " + address.townOrCity;
                scheduleCollegeAddress = scheduleCollegeAddress + ", " + address.mandal;
                scheduleCollegeAddress = scheduleCollegeAddress + ",<br />" + district;
                scheduleCollegeAddress = scheduleCollegeAddress + " - " + address.pincode;
            }

            if (address != null)
            {
                scheduleCollegeAddress1 = CollegeData.collegeName + ", " + address.address;
                scheduleCollegeAddress1 = scheduleCollegeAddress1 + ", " + address.townOrCity;
                scheduleCollegeAddress1 = scheduleCollegeAddress1 + ", " + address.mandal;
                scheduleCollegeAddress1 = scheduleCollegeAddress1 + ", " + district;
                scheduleCollegeAddress1 = scheduleCollegeAddress1 + " - " + address.pincode;
            }

            string CollegeSocietyAddress = string.Empty;
            if (societyAddress != null)
            {
                CollegeSocietyAddress = CollegeData.collegeName + ", " + societyAddress.address;
                CollegeSocietyAddress = CollegeSocietyAddress + ", " + societyAddress.townOrCity;
                CollegeSocietyAddress = CollegeSocietyAddress + ", " + societyAddress.mandal;
                CollegeSocietyAddress = CollegeSocietyAddress + ", " + district;
                CollegeSocietyAddress = CollegeSocietyAddress + " - " + societyAddress.pincode;
            }
            scheduleCollegeAddress = scheduleCollegeAddress.ToUpper().Replace(",", ", ").Replace("(", " (").Replace(")", ") ");
            CollegeSocietyAddress = CollegeSocietyAddress.ToUpper().Replace(",", ", ").Replace("(", " (").Replace(")", ") ");
            contents = contents.Replace("##SOCIETY_ADDRESS##", CollegeSocietyAddress);
            contents = contents.Replace("##COLLEGE_ADDRESS##", scheduleCollegeAddress);
            contents = contents.Replace("##COLLEGE_ADDRESS1##", scheduleCollegeAddress1);

            var College = CollegeData.collegeName + "(" + CollegeData.collegeCode + ")";

            //contents = contents.Replace("##College##", College);
            contents = contents.Replace("##CurrentDate##", DateTime.Now.ToString("dd-MM-yyy"));

            var nocintakeids = db.jntuh_college_paymentoffee_type.Where(a => (a.FeeType == "New Course(s)/Program(s)" || a.FeeType == "Increased in Intake(s)") && a.isActive == true).Select(s => s.id).ToArray();

            var NOCTypes = db.jntuh_college_paymentoffee_type.Where(a => (a.FeeType == "New Course(s)/Program(s)" || a.FeeType == "Increased in Intake(s)") && a.isActive == true).Select(s => new { TypeId = s.id, Type = s.FeeType }).ToList();
            int CoursenewId = NOCTypes.Where(e => e.Type == "New Course(s)/Program(s)").Select(a => a.TypeId).FirstOrDefault();
            int CoursesIncreseId = NOCTypes.Where(e => e.Type == "Increased in Intake(s)").Select(a => a.TypeId).FirstOrDefault();

            var jntuh_academivYear = db.jntuh_academic_year.ToList();
            int actualYear = jntuh_academivYear.Where(s => s.isActive == true && s.isPresentAcademicYear == true).Select(a => a.id).FirstOrDefault();
            int PresentYear = jntuh_academivYear.Where(s => s.id == (actualYear + 1)).Select(s => s.id).FirstOrDefault();

            var college = db.jntuh_college.Where(s => s.id == CollegeId).Select(a => a).FirstOrDefault();
            var jntuh_noc_data = db.jntuh_college_noc_data.Where(s => s.academicyearId == PresentYear && s.collegeId == CollegeId && nocintakeids.Contains(s.noctypeId)).Select(a => a.noctypeId).ToArray();

            if (jntuh_noc_data.Contains(CoursenewId) && jntuh_noc_data.Contains(CoursesIncreseId))
            {
                contents = contents.Replace("##ClosureCase##", "New Course(s)/Program(s) / Increased in Intake(s)  from the Academic Year 2020-21-Req.-Reg.");
                contents = contents.Replace("##NOCTYPE##", "Closure of the College");
                contents = contents.Replace("##NocType##", "<p style=''><b>CHAIRMAN /SECRETARY</b></p>");
            }
            else if (jntuh_noc_data.Contains(CoursenewId) && !jntuh_noc_data.Contains(CoursesIncreseId))
            {
                contents = contents.Replace("##ClosureCase##", "New Course(s)/Program(s)  from the Academic Year 2020-21-Req.-Reg.");
                contents = contents.Replace("##NOCTYPE##", "Closure of the following Courses /Programs");
                contents = contents.Replace("##NocType##", "<p style='float:right;'><b>PRINCIPAL</b></p>");
            }
            else if (jntuh_noc_data.Contains(CoursesIncreseId) && !jntuh_noc_data.Contains(CoursenewId))
            {
                contents = contents.Replace("##ClosureCase##", "Increased in Intake(s) from the Academic Year 2020-21-Req.-Reg.");
                contents = contents.Replace("##NOCTYPE##", "Closure of the following Courses /Programs");
                contents = contents.Replace("##NocType##", "<p style='float:right;'><b>PRINCIPAL</b></p>");
            }

            contents = contents.Replace("##ClosureActivity##", CourseIntakeChange(CollegeId));
            contents = contents.Replace("##PaymentDetails##", IntakePaymentDetails(CollegeId));
            // contents = contents.Replace("##CollegeName##", "TEST COLLEGE");
            contents = contents.Replace("##CollegeName##", "<p style='float:right;'><b>" + College.ToString() + "</b></p>");
            contents = contents.Replace("##Enclosures##", enclosures(15));



            var parsedHtmlElements = HTMLWorker.ParseToList(new StringReader(contents), null);

            bool pageRotated = false;

            //Get each array values from parsed elements and add to the PDF document
            foreach (var htmlElement in parsedHtmlElements)
            {
                try
                {
                    if (htmlElement.Equals("<textarea>"))
                    {
                        pdfDoc.NewPage();
                    }

                    if (htmlElement.Chunks.Count >= 3)
                    {
                        if (htmlElement.Chunks.Count == 4)
                        {
                            pdfDoc.SetPageSize(iTextSharp.text.PageSize.A4.Rotate());
                            pdfDoc.SetMargins(50, 50, 90, 50);
                            pageRotated = true;
                        }
                        else
                        {
                            if (pageRotated == true)
                            {
                                pdfDoc.SetPageSize(iTextSharp.text.PageSize.A4);
                                pdfDoc.SetMargins(50, 50, 90, 50);
                                pageRotated = false;
                            }
                        }
                        pdfDoc.NewPage();
                    }
                    else
                    {
                        pdfDoc.Add((IElement)htmlElement);
                    }
                }
                catch (Exception ex)
                {
                    continue;

                }
            }

            //Close your PDF
            pdfDoc.Close();
            if (pdfDoc.IsOpen())
            {
                pdfDoc.Close();
            }
            string returnpath = string.Empty;
            returnpath = fullPath;
            return returnpath;
        }

        public string CourseIntakeChange(int? CollegeId)
        {
            string contents = string.Empty;
            int count = 1;
            int index = 1;
            int sno = 1;
            if (CollegeId != null)
            {
                var jntuh_academivYear = db.jntuh_academic_year.ToList();
                int actualYear = jntuh_academivYear.Where(s => s.isActive == true && s.isPresentAcademicYear == true).Select(a => a.id).FirstOrDefault();
                int PresentYear = jntuh_academivYear.Where(s => s.id == (actualYear + 1)).Select(s => s.id).FirstOrDefault();

                var college = db.jntuh_college.Where(s => s.id == CollegeId).Select(a => a).FirstOrDefault();
                var nocintakeids = db.jntuh_college_paymentoffee_type.Where(a => (a.FeeType == "New Course(s)/Program(s)" || a.FeeType == "Increased in Intake(s)") && a.isActive == true).Select(s => s.id).ToArray();
                int CoursenewId = db.jntuh_college_paymentoffee_type.Where(e => e.FeeType == "New Course(s)/Program(s)").Select(a => a.id).FirstOrDefault();
                int CoursesIncreseId = db.jntuh_college_paymentoffee_type.Where(e => e.FeeType == "Increased in Intake(s)").Select(a => a.id).FirstOrDefault();

                var jntuh_noc_array = db.jntuh_college_noc_data.Where(s => s.academicyearId == PresentYear && s.collegeId == CollegeId && s.isClosure == true && nocintakeids.Contains(s.noctypeId)).Select(a => a.noctypeId).ToArray();

                var jntuh_noc_data = db.jntuh_college_noc_data.Where(s => s.academicyearId == PresentYear && s.collegeId == CollegeId && s.isClosure == true && nocintakeids.Contains(s.noctypeId)).Select(a => a).ToList();

                var CollegeClosure = jntuh_noc_data.Where(s => s.departmentId == 0 && s.specializationId == 0).Select(a => a).FirstOrDefault();
                var CourseClosure = jntuh_noc_data.Where(s => s.departmentId != 0 && s.specializationId != 0).Select(a => a).ToList();
                var jntuh_college_intake_existing = db.jntuh_college_intake_existing.Where(a => a.collegeId == CollegeId).Select(w => w).ToList();
                //if (CollegeClosure != null)
                //{
                //    List<IntakeExisting> ExistingIntake = (from e in jntuh_college_intake_existing
                //                                           join s in db.jntuh_specialization on e.specializationId equals s.id
                //                                           join d in db.jntuh_department on s.departmentId equals d.id
                //                                           join de in db.jntuh_degree on d.degreeId equals de.id
                //                                           join c in db.jntuh_college on e.collegeId equals c.id
                //                                           where e.collegeId == CollegeId
                //                                           select new IntakeExisting
                //                                           {
                //                                               academicyearid = e.academicYearId,
                //                                               DegreeId = de.id,
                //                                               Degree = de.degree,
                //                                               DepartmentId = d.id,
                //                                               Department = d.departmentName,
                //                                               SpecializationId = s.id,
                //                                               Specialization = s.specializationName,
                //                                               shiftId = e.shiftId,
                //                                               ApprovedIntake = jntuh_college_intake_existing.Where(sp => sp.academicYearId == PresentYear && sp.specializationId == s.id).Select(a => a.proposedIntake).FirstOrDefault(),

                //                                           }).ToList();

                //    ExistingIntake = ExistingIntake.AsEnumerable().GroupBy(s => new { s.SpecializationId, s.shiftId }).Select(q => q.First()).ToList();

                //    contents += "<p style='text-align:left;font-size:10px;'><b><u>a) NOC required for: Closure of College</b></u></p><br/>";
                //    contents += "<table border='1' cellspacing='0' cellpadding='4' style='font-size:10px;'>";
                //    contents += "<tr style='font-weight:bold;'>";
                //    contents += "<th width='10%' style='text-align:center;'>S.No</th>";
                //    contents += "<th width='55%' style='text-align:left;'>Course(s) recommended for Closure</th>";
                //    contents += "<th width='15%' style='text-align:left;'>Current Intake</th>";
                //    contents += "<th width='20%' style='text-align:left;'>Intake after Reduction/Closure</th>";
                //    contents += "</tr>";

                //    foreach (var item in ExistingIntake.OrderBy(a => a.Degree).ThenBy(s => s.Specialization).ToList())
                //    {
                //        contents += "<tr style='font-size:10px;'>";
                //        contents += "<td width='10%' style='text-align:center;'>" + (index++) + "</td>";
                //        contents += "<td width='55%' style='text-align:left;'>" + item.Degree + "-" + item.Specialization + "</td>";
                //        int id = jntuh_college_intake_existing.Where(a => a.academicYearId == PresentYear && a.specializationId == item.SpecializationId).Select(s => s.id).FirstOrDefault();
                //        if (id != 0)
                //        {
                //            var inkatenew = jntuh_college_intake_existing.Where(a => a.academicYearId == PresentYear && a.specializationId == item.SpecializationId).Select(s => s.proposedIntake).FirstOrDefault();
                //            contents += "<td width='15%' style='text-align:left;'>" + inkatenew + "</td>";
                //        }
                //        else
                //        {
                //            int appintake = jntuh_college_intake_existing.Where(a => a.specializationId == item.SpecializationId).Max(s => s.aicteApprovedIntake);
                //            contents += "<td width='15%' style='text-align:left;'>" + appintake + "</td>";
                //        }
                //        contents += "<td width='20%' style='text-align:left;'>0</td>";
                //        contents += "</tr>";
                //    }
                //    contents += "</table>";
                //}

                //if (jntuh_noc_array.Contains(CoursenewId) && jntuh_noc_array.Contains(CoursesIncreseId))
                //{
                contents += "<p style='text-align:left;font-size:10px;'><b><u>a) NOC required for: New Course(s)/Program(s) / Increased in Intake(s)</b></u></p><br/>";
                contents += "<table border='1' cellspacing='0' cellpadding='4' style='font-size:10px;'>";
                contents += "<tr style='font-weight:bold;'>";
                contents += "<th width='10%' style='text-align:center;'>S.No</th>";
                contents += "<th width='55%' style='text-align:left;'>Course(s) recommended for closure</th>";
                contents += "<th width='15%' style='text-align:left;'>Current Intake</th>";
                contents += "<th width='20%' style='text-align:left;'>Proposed Intake</th>";
                contents += "<th width='20%' style='text-align:left;'>Status</th>";
                contents += "</tr>";

                var CoursesDetails = (from n in CourseClosure
                                      join s in db.jntuh_specialization on n.specializationId equals s.id
                                      join d in db.jntuh_department on n.departmentId equals d.id
                                      join de in db.jntuh_degree on d.degreeId equals de.id
                                      select new IntakeExisting
                                      {
                                          DegreeId = de.id,
                                          Degree = de.degree,
                                          DepartmentId = d.id,
                                          Department = d.departmentName,
                                          SpecializationId = s.id,
                                          Specialization = s.specializationName,
                                          CourseStatus = n.isClosure == true ? "Submitted" : "Active"
                                      }).ToList();

                foreach (var item in CoursesDetails)
                {
                    contents += "<tr style='font-size:10px;'>";
                    contents += "<td width='10%' style='text-align:center;'>" + (index++) + "</td>";
                    contents += "<td width='55%' style='text-align:left;'>" + item.Degree + "-" + item.Specialization + "</td>";
                    int id = jntuh_college_intake_existing.Where(a => a.academicYearId == PresentYear && a.specializationId == item.SpecializationId).Select(s => s.id).FirstOrDefault();
                    if (id != 0)
                    {
                        var inkatenew = jntuh_college_intake_existing.Where(a => a.academicYearId == (PresentYear - 1) && a.specializationId == item.SpecializationId).Select(s => s.aicteApprovedIntake).FirstOrDefault();
                        contents += "<td width='15%' style='text-align:left;'>" + inkatenew + "</td>";
                    }
                    else
                    {
                        //int appintake = jntuh_college_intake_existing.Where(a => a.specializationId == item.SpecializationId).Max(s => s.aicteApprovedIntake);
                        contents += "<td width='15%' style='text-align:left;'>0</td>";
                    }
                    int cid = CourseClosure.Where(a => a.academicyearId == PresentYear && a.specializationId == item.SpecializationId).Select(s => s.id).FirstOrDefault();
                    if (cid != 0)
                    {
                        var inkatenew = CourseClosure.Where(a => a.academicyearId == PresentYear && a.specializationId == item.SpecializationId).Select(s => s).FirstOrDefault();
                        contents += "<td width='15%' style='text-align:left;'>" + inkatenew.approvedIntake + "</td>";
                        contents += "<td width='20%' style='text-align:left;'>" + inkatenew.remarks + "</td>";
                    }

                    contents += "</tr>";
                }
                contents += "</table>";
                //}
                return contents;
            }
            return contents;
        }

        public string IntakePaymentDetails(int? CollegeId)
        {
            string contents = string.Empty;
            int count = 1;
            int index = 1;
            int sno = 1;
            if (CollegeId != null)
            {
                var college = db.jntuh_college.Where(s => s.id == CollegeId).Select(a => a).FirstOrDefault();
                var jntuh_academivYear = db.jntuh_academic_year.ToList();

                int actualYear = jntuh_academivYear.Where(s => s.isActive == true && s.isPresentAcademicYear == true).Select(a => a.id).FirstOrDefault();
                int PresentYear = jntuh_academivYear.Where(s => s.id == (actualYear + 1)).Select(s => s.id).FirstOrDefault();

                var NOCTypes = db.jntuh_college_paymentoffee_type.Where(a => (a.FeeType == "New Course(s)/Program(s)" || a.FeeType == "Increased in Intake(s)") && a.isActive == true).Select(s => s.id).ToArray();
                //int CoursesNewId = NOCTypes.Where(e => e.Type == "NOC For Closure of College").Select(a => a.TypeId).FirstOrDefault();
                //int CoursesIncreId = NOCTypes.Where(e => e.Type == "NOC For Closure of Course").Select(a => a.TypeId).FirstOrDefault();
                var jntuh_noc_data = db.jntuh_college_noc_data.Where(s => s.academicyearId == PresentYear && s.collegeId == CollegeId && s.isClosure == true && NOCTypes.Contains(s.noctypeId)).Select(a => a).ToList();

                var CollegeClosure = jntuh_noc_data.Where(s => s.departmentId == 0 && s.specializationId == 0).Select(a => a).FirstOrDefault();
                var CourseClosure = jntuh_noc_data.Where(s => s.departmentId != 0 && s.specializationId != 0).Select(a => a).ToList();

                var jntuh_college_intake_existing = db.jntuh_college_intake_existing.Where(a => a.collegeId == CollegeId).Select(w => w).ToList();




                if (jntuh_noc_data != null)
                {
                    var jntuh_payments = db.jntuh_paymentresponse.Where(s => s.AcademicYearId == PresentYear && s.CollegeId == college.collegeCode && s.AuthStatus == "0300" && NOCTypes.Contains(s.PaymentTypeID)).ToList();

                    var CoursesDetails = (from n in CourseClosure
                                          join s in db.jntuh_specialization on n.specializationId equals s.id
                                          join d in db.jntuh_department on n.departmentId equals d.id
                                          join de in db.jntuh_degree on d.degreeId equals de.id
                                          select new IntakeExisting
                                          {
                                              DegreeId = de.id,
                                              Degree = de.degree,
                                              DepartmentId = d.id,
                                              Department = d.departmentName,
                                              SpecializationId = s.id,
                                              Specialization = s.specializationName,
                                              CourseStatus = n.isClosure == true ? "Submitted" : "Active"
                                          }).ToList();

                    var coursescount = CoursesDetails.Select(a => a.SpecializationId).Count();
                    contents += "<br/>";
                    contents += "<p style='text-align:left;font-size:10px;'><b><u>b) Details of Payment :</b></u></p><br/>";
                    contents += "<table border='1' cellspacing='0' cellpadding='3' style='font-size:10px;'>";
                    contents += "<thead>";
                    contents += "<tr style='font-weight:bold;'>";
                    contents += "<th width='10%' style='text-align:center;'>S.No</th>";
                    contents += "<th width='30%' style='text-align:left;'>Payment Date</th>";
                    contents += "<th width='40%' style='text-align:left;'>Reference Number</th>";
                    contents += "<th width='20%' style='text-align:left;'>Paid Amount</th>";
                    contents += "</thead>";
                    contents += "</tr>";

                    contents += "<tbody>";
                    foreach (var item1 in jntuh_payments)
                    {
                        contents += "<tr>";
                        contents += "<td width='10%' style='text-align:center;'>" + (count++) + "</td>";
                        contents += "<td width='30%' style='text-align:left;'>" + item1.TxnDate.ToString("dd-MM-yyyy") + "</td>";
                        contents += "<td width='40%' style='text-align:left;'>" + item1.TxnReferenceNo + "</td>";
                        contents += "<td width='20%' style='text-align:left;'>" + item1.TxnAmount + "</td>";
                        contents += "</tr>";
                    }
                    contents += "</tbody>";
                    contents += "</table>";
                }
                return contents;
            }
            return contents;
        }

        //public string enclosures(int? nocTypeid)
        //{
        //    string contents = string.Empty;
        //    int index = 1;
        //    contents += "<p style='font-size:10px;'><b>Enclosures:</b></p>";
        //    var noc_enclosures = db.jntuh_college_noc_enclosures.Where(s => s.isActive == true && s.noctypeid == nocTypeid).Select(s => s).OrderBy(a => a.id).ToList();

        //    contents += "<table border='0' cellspacing='0' cellpadding='4'  width='100%' style='font-size:10px;'>";
        //    foreach (var item in noc_enclosures)
        //    {
        //        contents += "<tr>";
        //        contents += "<td width='10%' style='text-align:center;'>" + (index++) + "</td>";
        //        contents += "<td width='90%' style='text-align:left;'>" + item.formName + "</td>";
        //        contents += "</tr>";
        //    }
        //    contents += "</table>";
        //    return contents;
        //}

        #endregion

        #region college and socity address change
        [Authorize(Roles = "Admin,College")]
        public ActionResult Collegeaddress()
        {
            int userId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(collegeUser => collegeUser.userID == userId).Select(collegeUser => collegeUser.collegeID).FirstOrDefault();

            var todayDate = DateTime.Now;
            var jntuh_academivYear = db.jntuh_academic_year.ToList();
            int actualYear = jntuh_academivYear.Where(s => s.isActive == true && s.isPresentAcademicYear == true).Select(a => a.id).FirstOrDefault();
            int PresentYear = jntuh_academivYear.Where(s => s.id == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
            int lid = db.jntuh_link_screens.Where(p => p.linkName == "Change Of Location" && p.isActive == true).Select(s => s.id).FirstOrDefault();
            jntuh_college_links_assigned NOCPhase =
                db.jntuh_college_links_assigned.Where(l => l.linkId == lid && l.academicyearId == PresentYear && l.isActive == true && l.fromdate <= todayDate && l.todate >= todayDate).Select(s => s).FirstOrDefault();

            if (NOCPhase == null || userCollegeID != 375)
            {
                return RedirectToAction("College", "Dashboard");
            }

            var jntudistricte = db.jntuh_district.Where(r => r.isActive == true).Select(s => new { disctrictID = s.id, disctrictName = s.districtName }).ToList();
            ViewBag.Disctricts = jntudistricte;
            var collegedata = db.jntuh_address.Where(e => e.addressTye == "COLLEGE").Select(r => r).FirstOrDefault();
            address obj = new address();
            obj.clgaddress = collegedata.address;
            obj.clgmandal = collegedata.mandal;
            obj.clgtown = collegedata.townOrCity;
            obj.clgpincode = collegedata.pincode;
            obj.clgdistrict = db.jntuh_district.Where(r => r.id == collegedata.districtId).Select(r => r.districtName).FirstOrDefault();

            var collegedatanew = db.jntuh_address_log.Where(e => e.collegeId == userCollegeID && e.addressTye == "COLLEGE").Select(r => r).FirstOrDefault();
            TempData["Status"] = "false";
            if (collegedatanew != null)
            {
                TempData["Status"] = "true";
                obj.Id = Convert.ToInt32(collegedatanew.id);
                obj.caddress = collegedatanew.address;
                obj.mandal = collegedatanew.mandal;
                obj.townOrcity = collegedatanew.townOrCity;
                obj.pincode = collegedatanew.pincode;
                obj.district = db.jntuh_district.Where(r => r.id == collegedatanew.districtId).Select(r => r.districtName).FirstOrDefault();
            }
            #region Payment
            var returnUrl = WebConfigurationManager.AppSettings["collegesocityReturnUrl"];
            var merchantId = WebConfigurationManager.AppSettings["MerchantID"];
            var securityId = WebConfigurationManager.AppSettings["SecurityID"];
            var typefield1 = WebConfigurationManager.AppSettings["TypeField1"];
            var typefield2 = WebConfigurationManager.AppSettings["TypeField2"];

            string clgCode = db.jntuh_college.Where(a => a.id == userCollegeID).Select(a => a.collegeCode).FirstOrDefault();
            var NOCTypes = db.jntuh_college_paymentoffee_type.Where(a => (a.FeeType == "Change Of Location") && a.isActive == true).Select(s => new { TypeId = s.id, Type = s.FeeType }).ToList();
            int CoursesNocId = NOCTypes.Where(e => e.Type == "Change Of Location").Select(a => a.TypeId).FirstOrDefault();
            var payment = db.jntuh_paymentresponse.Where(s => s.AcademicYearId == PresentYear && s.CollegeId == clgCode && s.PaymentTypeID == CoursesNocId && s.AuthStatus == "0300").Select(q => q).ToList();

            ViewBag.totalFee = "75000";
            ViewBag.collegeCode = clgCode;
            ViewBag.challnNumber = clgCode + DateTime.Now.ToString("yyyMMddHHmmss");
            ViewBag.noctype = CoursesNocId;
            ViewBag.IsPaymentDone = false;
            decimal val = 75000;
            if (75000 == payment.Select(a => a.TxnAmount).Sum())
            {
                ViewBag.IsPaymentDone = true;
            }

            var msg = "";
            if (userCollegeID == 375)
            {
                msg = merchantId + "|" + ViewBag.challnNumber + "|NA|2.00|NA|NA|NA|INR|NA|" + typefield1 + "|" + securityId + "|NA|NA|" + typefield2 + "|NA|NA|NA|NA|NA|NA|NA|" + returnUrl;
                var key = WebConfigurationManager.AppSettings["ChecksumKey"];
                msg += "|" + GetHMACSHA256(msg, key).ToUpper();
            }
            else
            {
                msg = merchantId + "|" + ViewBag.challnNumber + "|NA|75000|NA|NA|NA|INR|NA|" + typefield1 + "|" + securityId + "|NA|NA|" + typefield2 + "|NA|NA|NA|NA|NA|NA|NA|" + returnUrl;
                var key = WebConfigurationManager.AppSettings["ChecksumKey"];
                msg += "|" + GetHMACSHA256(msg, key).ToUpper();
            }
            ViewBag.msg = msg;
            #endregion
            return View(obj);
        }
        [HttpPost]
        [Authorize(Roles = "Admin,College")]
        public ActionResult Collegeaddress(address obj)
        {
            int userId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(collegeUser => collegeUser.userID == userId).Select(collegeUser => collegeUser.collegeID).FirstOrDefault();

            if (obj != null)
            {
                var todayDate = DateTime.Now;
                var jntuh_academivYear = db.jntuh_academic_year.ToList();
                int actualYear = jntuh_academivYear.Where(s => s.isActive == true && s.isPresentAcademicYear == true).Select(a => a.id).FirstOrDefault();
                int PresentYear = jntuh_academivYear.Where(s => s.id == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
                int lid = db.jntuh_link_screens.Where(p => p.linkName == "Change Of Location" && p.isActive == true).Select(s => s.id).FirstOrDefault();
                jntuh_college_links_assigned NOCPhase =
                    db.jntuh_college_links_assigned.Where(l => l.linkId == lid && l.academicyearId == PresentYear && l.isActive == true && l.fromdate <= todayDate && l.todate >= todayDate).Select(s => s).FirstOrDefault();

                if (NOCPhase == null || userCollegeID != 375)
                {
                    return RedirectToAction("College", "Dashboard");
                }

                jntuh_address_log log = new jntuh_address_log();
                log.academicYearid = PresentYear;
                log.collegeId = userCollegeID;
                log.addressTye = "COLLEGE";
                log.address = obj.caddress;
                log.townOrCity = obj.townOrcity;
                log.mandal = obj.mandal;
                log.districtId = Convert.ToInt32(obj.districtId);
                log.stateId = 1;
                log.pincode = obj.pincode;
                log.fax = null;
                log.landline = null;
                log.mobile = null;
                log.website = null;
                log.email = null;
                log.typename = null;
                log.isUpdated = false;
                log.createdBy = userId;
                log.createdOn = DateTime.Now;
                log.updatedBy = null;
                log.updatedOn = null;
                db.jntuh_address_log.Add(log);
                db.SaveChanges();
                TempData["SUCCESS"] = "Address is Saved Successfully.";
                var jntudistricte = db.jntuh_district.Where(r => r.isActive == true).Select(s => new { disctrictID = s.id, disctrictName = s.districtName }).ToList();
                ViewBag.Disctricts = jntudistricte;
                //return View();
            }
            else
            {
                TempData["ERROR"] = "Some thing Went Wrong.Please Try Again.";

            }
            return RedirectToAction("Collegeaddress");
        }

        [Authorize(Roles = "Admin,College")]
        public ActionResult EditCollegeaddress(int id)
        {
            int userId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(collegeUser => collegeUser.userID == userId).Select(collegeUser => collegeUser.collegeID).FirstOrDefault();

            var todayDate = DateTime.Now;
            var jntuh_academivYear = db.jntuh_academic_year.ToList();
            int actualYear = jntuh_academivYear.Where(s => s.isActive == true && s.isPresentAcademicYear == true).Select(a => a.id).FirstOrDefault();
            int PresentYear = jntuh_academivYear.Where(s => s.id == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
            int lid = db.jntuh_link_screens.Where(p => p.linkName == "Change Of Location" && p.isActive == true).Select(s => s.id).FirstOrDefault();
            jntuh_college_links_assigned NOCPhase =
                db.jntuh_college_links_assigned.Where(l => l.linkId == lid && l.academicyearId == PresentYear && l.isActive == true && l.fromdate <= todayDate && l.todate >= todayDate).Select(s => s).FirstOrDefault();

            if (NOCPhase == null || userCollegeID != 375)
            {
                return RedirectToAction("College", "Dashboard");
            }

            var collegedata = db.jntuh_address.Where(e => e.addressTye == "COLLEGE").Select(r => r).FirstOrDefault();
            address obj = new address();
            obj.clgaddress = collegedata.address;
            obj.clgmandal = collegedata.mandal;
            obj.clgtown = collegedata.townOrCity;
            obj.clgpincode = collegedata.pincode;
            obj.clgdistrict = db.jntuh_district.Where(r => r.id == collegedata.districtId).Select(r => r.districtName).FirstOrDefault();

            var addressdata = db.jntuh_address_log.Where(r => r.id == id).Select(t => t).FirstOrDefault();
            obj.Id = id;
            obj.caddress = addressdata.address;
            obj.townOrcity = addressdata.townOrCity;
            obj.mandal = addressdata.mandal;
            obj.districtId = addressdata.districtId;
            obj.pincode = addressdata.pincode;

            var jntudistricte = db.jntuh_district.Where(r => r.isActive == true).Select(s => new { disctrictID = s.id, disctrictName = s.districtName }).ToList();
            ViewBag.Disctricts = jntudistricte;


            return View(obj);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,College")]
        public ActionResult EditCollegeaddress(address obj)
        {
            int userId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(collegeUser => collegeUser.userID == userId).Select(collegeUser => collegeUser.collegeID).FirstOrDefault();

            if (obj != null)
            {
                var todayDate = DateTime.Now;
                var jntuh_academivYear = db.jntuh_academic_year.ToList();
                int actualYear = jntuh_academivYear.Where(s => s.isActive == true && s.isPresentAcademicYear == true).Select(a => a.id).FirstOrDefault();
                int PresentYear = jntuh_academivYear.Where(s => s.id == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
                int lid = db.jntuh_link_screens.Where(p => p.linkName == "Change Of Location" && p.isActive == true).Select(s => s.id).FirstOrDefault();
                jntuh_college_links_assigned NOCPhase =
                    db.jntuh_college_links_assigned.Where(l => l.linkId == lid && l.academicyearId == PresentYear && l.isActive == true && l.fromdate <= todayDate && l.todate >= todayDate).Select(s => s).FirstOrDefault();

                if (NOCPhase == null || userCollegeID != 375)
                {
                    return RedirectToAction("College", "Dashboard");
                }

                var addressdata = db.jntuh_address_log.Where(r => r.id == obj.Id && r.addressTye == "COLLEGE").Select(t => t).FirstOrDefault();
                addressdata.address = obj.caddress;
                addressdata.townOrCity = obj.townOrcity;
                addressdata.mandal = obj.mandal;
                addressdata.districtId = Convert.ToInt32(obj.districtId);
                addressdata.stateId = 1;
                addressdata.pincode = obj.pincode;
                addressdata.updatedBy = userId;
                addressdata.updatedOn = DateTime.Now;
                db.Entry(addressdata).State = EntityState.Modified;
                db.SaveChanges();
                TempData["SUCCESS"] = "Address is Updated Successfully.";
                //return View();
            }
            else
            {
                TempData["ERROR"] = "Some thing Went Wrong.Please Try Again.";

            }
            return RedirectToAction("Collegeaddress");
        }

        [Authorize(Roles = "Admin,College")]
        public ActionResult Societyaddress()
        {
            int userId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(collegeUser => collegeUser.userID == userId).Select(collegeUser => collegeUser.collegeID).FirstOrDefault();

            var todayDate = DateTime.Now;
            var jntuh_academivYear = db.jntuh_academic_year.ToList();
            int actualYear = jntuh_academivYear.Where(s => s.isActive == true && s.isPresentAcademicYear == true).Select(a => a.id).FirstOrDefault();
            int PresentYear = jntuh_academivYear.Where(s => s.id == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
            int lid = db.jntuh_link_screens.Where(p => p.linkName == "Change of Name of the Society" && p.isActive == true).Select(s => s.id).FirstOrDefault();
            jntuh_college_links_assigned NOCPhase =
                db.jntuh_college_links_assigned.Where(l => l.linkId == lid && l.academicyearId == PresentYear && l.isActive == true && l.fromdate <= todayDate && l.todate >= todayDate).Select(s => s).FirstOrDefault();

            if (NOCPhase == null || userCollegeID != 375)
            {
                return RedirectToAction("College", "Dashboard");
            }
            var jntudistricte = db.jntuh_district.Where(r => r.isActive == true).Select(s => new { disctrictID = s.id, disctrictName = s.districtName }).ToList();
            ViewBag.Disctricts = jntudistricte;
            var sociery = db.jntuh_college_establishment.Where(r => r.collegeId == userCollegeID).Select(r => r).FirstOrDefault();
            var collegedata = db.jntuh_address.Where(e => e.addressTye == "COLLEGE").Select(r => r).FirstOrDefault();
            address obj = new address();
            obj.clgsociety = sociery.societyName;
            obj.clgaddress = collegedata.address;
            obj.clgmandal = collegedata.mandal;
            obj.clgtown = collegedata.townOrCity;
            obj.clgpincode = collegedata.pincode;
            obj.NOCtype = lid;
            obj.clgdistrict = db.jntuh_district.Where(r => r.id == collegedata.districtId).Select(r => r.districtName).FirstOrDefault();
            var collegedatanew = db.jntuh_address_log.Where(e => e.collegeId == userCollegeID && e.addressTye == "SOCIETY").Select(r => r).FirstOrDefault();
            TempData["Status"] = "false";
            if (collegedatanew != null)
            {
                TempData["Status"] = "true";
                obj.Id = Convert.ToInt32(collegedatanew.id);
                obj.typename = collegedatanew.typename;

                obj.Id = Convert.ToInt32(collegedatanew.id);
                obj.caddress = collegedatanew.address;
                obj.mandal = collegedatanew.mandal;
                obj.townOrcity = collegedatanew.townOrCity;
                obj.pincode = collegedatanew.pincode;
                obj.district = db.jntuh_district.Where(r => r.id == collegedatanew.districtId).Select(r => r.districtName).FirstOrDefault();
            }
            #region Payment
            var returnUrl = WebConfigurationManager.AppSettings["collegesocityReturnUrl"];
            var merchantId = WebConfigurationManager.AppSettings["MerchantID"];
            var securityId = WebConfigurationManager.AppSettings["SecurityID"];
            var typefield1 = WebConfigurationManager.AppSettings["TypeField1"];
            var typefield2 = WebConfigurationManager.AppSettings["TypeField2"];

            string clgCode = db.jntuh_college.Where(a => a.id == userCollegeID).Select(a => a.collegeCode).FirstOrDefault();
            var NOCTypes = db.jntuh_college_paymentoffee_type.Where(a => (a.FeeType == "Change of Name of the Society") && a.isActive == true).Select(s => new { TypeId = s.id, Type = s.FeeType }).ToList();
            int CoursesNocId = NOCTypes.Where(e => e.Type == "Change of Name of the Society").Select(a => a.TypeId).FirstOrDefault();
            var payment = db.jntuh_paymentresponse.Where(s => s.AcademicYearId == PresentYear && s.CollegeId == clgCode && s.PaymentTypeID == CoursesNocId && s.AuthStatus == "0300").Select(q => q).ToList();

            ViewBag.totalFee = "75000";
            ViewBag.collegeCode = clgCode;
            ViewBag.challnNumber = clgCode + DateTime.Now.ToString("yyyMMddHHmmss");
            ViewBag.noctype = CoursesNocId;
            ViewBag.IsPaymentDone = false;
            decimal val = 75000;
            if (75000 == payment.Select(a => a.TxnAmount).Sum())
            {
                ViewBag.IsPaymentDone = true;
            }

            var msg = "";
            if (userCollegeID == 375)
            {
                msg = merchantId + "|" + ViewBag.challnNumber + "|NA|2.00|NA|NA|NA|INR|NA|" + typefield1 + "|" + securityId + "|NA|NA|" + typefield2 + "|NA|NA|NA|NA|NA|NA|NA|" + returnUrl;
                var key = WebConfigurationManager.AppSettings["ChecksumKey"];
                msg += "|" + GetHMACSHA256(msg, key).ToUpper();
            }
            else
            {
                msg = merchantId + "|" + ViewBag.challnNumber + "|NA|75000|NA|NA|NA|INR|NA|" + typefield1 + "|" + securityId + "|NA|NA|" + typefield2 + "|NA|NA|NA|NA|NA|NA|NA|" + returnUrl;
                var key = WebConfigurationManager.AppSettings["ChecksumKey"];
                msg += "|" + GetHMACSHA256(msg, key).ToUpper();
            }
            ViewBag.msg = msg;
            #endregion
            return View(obj);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,College")]
        public ActionResult Societyaddress(address obj)
        {
            int userId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(collegeUser => collegeUser.userID == userId).Select(collegeUser => collegeUser.collegeID).FirstOrDefault();

            if (obj != null)
            {
                var todayDate = DateTime.Now;
                var jntuh_academivYear = db.jntuh_academic_year.ToList();
                int actualYear = jntuh_academivYear.Where(s => s.isActive == true && s.isPresentAcademicYear == true).Select(a => a.id).FirstOrDefault();
                int PresentYear = jntuh_academivYear.Where(s => s.id == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
                int lid = db.jntuh_link_screens.Where(p => p.linkName == "Change of Name of the Society" && p.isActive == true).Select(s => s.id).FirstOrDefault();
                jntuh_college_links_assigned NOCPhase =
                    db.jntuh_college_links_assigned.Where(l => l.linkId == lid && l.academicyearId == PresentYear && l.isActive == true && l.fromdate <= todayDate && l.todate >= todayDate).Select(s => s).FirstOrDefault();

                if (NOCPhase == null || userCollegeID != 375)
                {
                    return RedirectToAction("College", "Dashboard");
                }
                jntuh_address_log log = new jntuh_address_log();
                log.academicYearid = PresentYear;
                log.collegeId = userCollegeID;
                log.addressTye = "SOCIETY";
                log.address = obj.caddress;
                log.townOrCity = obj.townOrcity;
                log.mandal = obj.mandal;
                log.districtId = Convert.ToInt32(obj.districtId);
                log.stateId = 1;
                log.pincode = obj.pincode;
                log.fax = null;
                log.landline = null;
                log.mobile = null;
                log.website = null;
                log.email = null;
                log.typename = obj.typename;
                log.isUpdated = false;
                log.createdBy = userId;
                log.createdOn = DateTime.Now;
                log.updatedBy = null;
                log.updatedOn = null;
                db.jntuh_address_log.Add(log);
                db.SaveChanges();
                TempData["SUCCESS"] = "Society is Saved Successfully.";
                var jntudistricte = db.jntuh_district.Where(r => r.isActive == true).Select(s => new { disctrictID = s.id, disctrictName = s.districtName }).ToList();
                ViewBag.Disctricts = jntudistricte;
                //return View();
            }
            else
            {
                TempData["ERROR"] = "Some thing Went Wrong.Please Try Again.";

            }
            return RedirectToAction("Societyaddress");
        }

        [Authorize(Roles = "Admin,College")]
        public ActionResult Editsocietyname(int id)
        {
            int userId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(collegeUser => collegeUser.userID == userId).Select(collegeUser => collegeUser.collegeID).FirstOrDefault();

            var todayDate = DateTime.Now;
            var jntuh_academivYear = db.jntuh_academic_year.ToList();
            int actualYear = jntuh_academivYear.Where(s => s.isActive == true && s.isPresentAcademicYear == true).Select(a => a.id).FirstOrDefault();
            int PresentYear = jntuh_academivYear.Where(s => s.id == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
            int lid = db.jntuh_link_screens.Where(p => p.linkName == "Change of Name of the Society" && p.isActive == true).Select(s => s.id).FirstOrDefault();
            jntuh_college_links_assigned NOCPhase =
                db.jntuh_college_links_assigned.Where(l => l.linkId == lid && l.academicyearId == PresentYear && l.isActive == true && l.fromdate <= todayDate && l.todate >= todayDate).Select(s => s).FirstOrDefault();

            if (NOCPhase == null)
            {
                return RedirectToAction("College", "Dashboard");
            }

            var sociery = db.jntuh_college_establishment.Where(r => r.collegeId == userCollegeID).Select(r => r).FirstOrDefault();
            society obj = new society();
            obj.clgsociety = sociery.societyName;

            var collegedatanew = db.jntuh_address_log.Where(e => e.collegeId == userCollegeID && e.addressTye == "SOCIETY").Select(r => r).FirstOrDefault();

            obj.Id = Convert.ToInt32(collegedatanew.id);
            obj.societyname = collegedatanew.typename;

            return View(obj);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,College")]
        public ActionResult Editsocietyname(society obj)
        {
            int userId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(collegeUser => collegeUser.userID == userId).Select(collegeUser => collegeUser.collegeID).FirstOrDefault();

            if (obj != null)
            {
                var todayDate = DateTime.Now;
                var jntuh_academivYear = db.jntuh_academic_year.ToList();
                int actualYear = jntuh_academivYear.Where(s => s.isActive == true && s.isPresentAcademicYear == true).Select(a => a.id).FirstOrDefault();
                int PresentYear = jntuh_academivYear.Where(s => s.id == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
                int lid = db.jntuh_link_screens.Where(p => p.linkName == "Change of Name of the Society" && p.isActive == true).Select(s => s.id).FirstOrDefault();
                jntuh_college_links_assigned NOCPhase =
                    db.jntuh_college_links_assigned.Where(l => l.linkId == lid && l.academicyearId == PresentYear && l.isActive == true && l.fromdate <= todayDate && l.todate >= todayDate).Select(s => s).FirstOrDefault();

                if (NOCPhase == null || userCollegeID != 375)
                {
                    return RedirectToAction("College", "Dashboard");
                }

                var addressdata = db.jntuh_address_log.Where(r => r.id == obj.Id && r.addressTye == "SOCIETY").Select(t => t).FirstOrDefault();
                addressdata.typename = obj.societyname;
                addressdata.updatedBy = userId;
                addressdata.updatedOn = DateTime.Now;
                db.Entry(addressdata).State = EntityState.Modified;
                db.SaveChanges();
                TempData["SUCCESS"] = "Society is Updated Successfully.";
                //return View();
            }
            else
            {
                TempData["ERROR"] = "Some thing Went Wrong.Please Try Again.";

            }
            return RedirectToAction("Societyaddress");
        }

        [Authorize(Roles = "Admin,College")]
        public ActionResult Collegename()
        {
            int userId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(collegeUser => collegeUser.userID == userId).Select(collegeUser => collegeUser.collegeID).FirstOrDefault();

            var todayDate = DateTime.Now;
            var jntuh_academivYear = db.jntuh_academic_year.ToList();
            int actualYear = jntuh_academivYear.Where(s => s.isActive == true && s.isPresentAcademicYear == true).Select(a => a.id).FirstOrDefault();
            int PresentYear = jntuh_academivYear.Where(s => s.id == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
            int lid = db.jntuh_link_screens.Where(p => p.linkName == "Change of Name of the College" && p.isActive == true).Select(s => s.id).FirstOrDefault();
            jntuh_college_links_assigned NOCPhase =
                db.jntuh_college_links_assigned.Where(l => l.linkId == lid && l.academicyearId == PresentYear && l.isActive == true && l.fromdate <= todayDate && l.todate >= todayDate).Select(s => s).FirstOrDefault();

            if (NOCPhase == null || userCollegeID != 375)
            {
                return RedirectToAction("College", "Dashboard");
            }

            //var sociery = db.jntuh_college_establishment.Where(r => r.collegeId == userCollegeID).Select(r => r).FirstOrDefault();
            var college = db.jntuh_college.Where(r => r.id == userCollegeID).Select(r => r).FirstOrDefault();
            society obj = new society();
            obj.clgsociety = college.collegeName;

            var collegedatanew = db.jntuh_address_log.Where(e => e.collegeId == userCollegeID && e.addressTye == "COLEGENAME").Select(r => r).FirstOrDefault();
            TempData["Status"] = "false";
            if (collegedatanew != null)
            {
                TempData["Status"] = "true";
                obj.Id = Convert.ToInt32(collegedatanew.id);
                obj.societyname = collegedatanew.typename;
            }
            #region Payment
            var returnUrl = WebConfigurationManager.AppSettings["collegesocityReturnUrl"];
            var merchantId = WebConfigurationManager.AppSettings["MerchantID"];
            var securityId = WebConfigurationManager.AppSettings["SecurityID"];
            var typefield1 = WebConfigurationManager.AppSettings["TypeField1"];
            var typefield2 = WebConfigurationManager.AppSettings["TypeField2"];

            string clgCode = db.jntuh_college.Where(a => a.id == userCollegeID).Select(a => a.collegeCode).FirstOrDefault();
            var NOCTypes = db.jntuh_college_paymentoffee_type.Where(a => (a.FeeType == "Change of Name of the College") && a.isActive == true).Select(s => new { TypeId = s.id, Type = s.FeeType }).ToList();
            int CoursesNocId = NOCTypes.Where(e => e.Type == "Change of Name of the College").Select(a => a.TypeId).FirstOrDefault();
            var payment = db.jntuh_paymentresponse.Where(s => s.AcademicYearId == PresentYear && s.CollegeId == clgCode && s.PaymentTypeID == CoursesNocId && s.AuthStatus == "0300").Select(q => q).ToList();

            ViewBag.totalFee = "75000";
            ViewBag.collegeCode = clgCode;
            ViewBag.challnNumber = clgCode + DateTime.Now.ToString("yyyMMddHHmmss");
            ViewBag.noctype = CoursesNocId;
            ViewBag.IsPaymentDone = false;
            decimal val = 75000;
            if (75000 == payment.Select(a => a.TxnAmount).Sum())
            {
                ViewBag.IsPaymentDone = true;
            }

            var msg = "";
            if (userCollegeID == 375)
            {
                msg = merchantId + "|" + ViewBag.challnNumber + "|NA|2.00|NA|NA|NA|INR|NA|" + typefield1 + "|" + securityId + "|NA|NA|" + typefield2 + "|NA|NA|NA|NA|NA|NA|NA|" + returnUrl;
                var key = WebConfigurationManager.AppSettings["ChecksumKey"];
                msg += "|" + GetHMACSHA256(msg, key).ToUpper();
            }
            else
            {
                msg = merchantId + "|" + ViewBag.challnNumber + "|NA|7500|NA|NA|NA|INR|NA|" + typefield1 + "|" + securityId + "|NA|NA|" + typefield2 + "|NA|NA|NA|NA|NA|NA|NA|" + returnUrl;
                var key = WebConfigurationManager.AppSettings["ChecksumKey"];
                msg += "|" + GetHMACSHA256(msg, key).ToUpper();
            }
            ViewBag.msg = msg;
            #endregion
            return View(obj);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,College")]
        public ActionResult Collegename(society obj)
        {
            int userId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(collegeUser => collegeUser.userID == userId).Select(collegeUser => collegeUser.collegeID).FirstOrDefault();

            if (obj != null)
            {
                var todayDate = DateTime.Now;
                var jntuh_academivYear = db.jntuh_academic_year.ToList();
                int actualYear = jntuh_academivYear.Where(s => s.isActive == true && s.isPresentAcademicYear == true).Select(a => a.id).FirstOrDefault();
                int PresentYear = jntuh_academivYear.Where(s => s.id == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
                int lid = db.jntuh_link_screens.Where(p => p.linkName == "Change of Name of the College" && p.isActive == true).Select(s => s.id).FirstOrDefault();
                jntuh_college_links_assigned NOCPhase =
                    db.jntuh_college_links_assigned.Where(l => l.linkId == lid && l.academicyearId == PresentYear && l.isActive == true && l.fromdate <= todayDate && l.todate >= todayDate).Select(s => s).FirstOrDefault();

                if (NOCPhase == null || userCollegeID != 375)
                {
                    return RedirectToAction("College", "Dashboard");
                }


                jntuh_address_log log = new jntuh_address_log();
                log.academicYearid = PresentYear;
                log.collegeId = userCollegeID;
                log.addressTye = "COLEGENAME";
                log.address = "College Name";
                log.townOrCity = "College Name";
                log.mandal = "College Name";
                log.districtId = 100;
                log.stateId = 1;
                log.pincode = 999999;
                log.fax = null;
                log.landline = null;
                log.mobile = null;
                log.website = null;
                log.email = null;
                log.typename = obj.societyname;
                log.isUpdated = false;
                log.createdBy = userId;
                log.createdOn = DateTime.Now;
                log.updatedBy = null;
                log.updatedOn = null;
                db.jntuh_address_log.Add(log);
                db.SaveChanges();
                TempData["SUCCESS"] = "College name is Saved Successfully.";

                //return View();
            }
            else
            {
                TempData["ERROR"] = "Some thing Went Wrong.Please Try Again.";

            }
            return RedirectToAction("Collegename");
        }

        [Authorize(Roles = "Admin,College")]
        public ActionResult EditCollegename(int id)
        {
            int userId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(collegeUser => collegeUser.userID == userId).Select(collegeUser => collegeUser.collegeID).FirstOrDefault();

            var todayDate = DateTime.Now;
            var jntuh_academivYear = db.jntuh_academic_year.ToList();
            int actualYear = jntuh_academivYear.Where(s => s.isActive == true && s.isPresentAcademicYear == true).Select(a => a.id).FirstOrDefault();
            int PresentYear = jntuh_academivYear.Where(s => s.id == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
            int lid = db.jntuh_link_screens.Where(p => p.linkName == "Change of Name of the College" && p.isActive == true).Select(s => s.id).FirstOrDefault();
            jntuh_college_links_assigned NOCPhase =
                db.jntuh_college_links_assigned.Where(l => l.linkId == lid && l.academicyearId == PresentYear && l.isActive == true && l.fromdate <= todayDate && l.todate >= todayDate).Select(s => s).FirstOrDefault();

            if (NOCPhase == null)
            {
                return RedirectToAction("College", "Dashboard");
            }

            var sociery = db.jntuh_college_establishment.Where(r => r.collegeId == userCollegeID).Select(r => r).FirstOrDefault();
            society obj = new society();
            obj.clgsociety = sociery.societyName;

            var collegedatanew = db.jntuh_address_log.Where(e => e.collegeId == userCollegeID && e.addressTye == "SOCIETY").Select(r => r).FirstOrDefault();

            obj.Id = Convert.ToInt32(collegedatanew.id);
            obj.societyname = collegedatanew.typename;

            return View(obj);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,College")]
        public ActionResult EditCollegename(society obj)
        {
            int userId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(collegeUser => collegeUser.userID == userId).Select(collegeUser => collegeUser.collegeID).FirstOrDefault();

            if (obj != null)
            {
                var todayDate = DateTime.Now;
                var jntuh_academivYear = db.jntuh_academic_year.ToList();
                int actualYear = jntuh_academivYear.Where(s => s.isActive == true && s.isPresentAcademicYear == true).Select(a => a.id).FirstOrDefault();
                int PresentYear = jntuh_academivYear.Where(s => s.id == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
                int lid = db.jntuh_link_screens.Where(p => p.linkName == "Change of Name of the College" && p.isActive == true).Select(s => s.id).FirstOrDefault();
                jntuh_college_links_assigned NOCPhase =
                    db.jntuh_college_links_assigned.Where(l => l.linkId == lid && l.academicyearId == PresentYear && l.isActive == true && l.fromdate <= todayDate && l.todate >= todayDate).Select(s => s).FirstOrDefault();

                if (NOCPhase == null || userCollegeID != 375)
                {
                    return RedirectToAction("College", "Dashboard");
                }

                var addressdata = db.jntuh_address_log.Where(r => r.id == obj.Id && r.addressTye == "SOCIETY").Select(t => t).FirstOrDefault();
                addressdata.typename = obj.societyname;
                addressdata.updatedBy = userId;
                addressdata.updatedOn = DateTime.Now;
                db.Entry(addressdata).State = EntityState.Modified;
                db.SaveChanges();
                TempData["SUCCESS"] = "Society is Updated Successfully.";
                //return View();
            }
            else
            {
                TempData["ERROR"] = "Some thing Went Wrong.Please Try Again.";

            }
            return RedirectToAction("Societyaddress");
        }

        public string SocityAddressChange(int? CollegeId)
        {
            string contents = string.Empty;
            int count = 1;
            int index = 1;
            int sno = 1;
            if (CollegeId != null)
            {
                var jntuh_academivYear = db.jntuh_academic_year.ToList();
                int actualYear = jntuh_academivYear.Where(s => s.isActive == true && s.isPresentAcademicYear == true).Select(a => a.id).FirstOrDefault();
                int PresentYear = jntuh_academivYear.Where(s => s.id == (actualYear + 1)).Select(s => s.id).FirstOrDefault();

                var college = db.jntuh_college.Where(s => s.id == CollegeId).Select(a => a).FirstOrDefault();
                var jntuh_noc_data = db.jntuh_college_noc_data.Where(s => s.academicyearId == PresentYear && s.collegeId == CollegeId).Select(a => a).ToList();

                var CollegeClosure = jntuh_noc_data.Where(s => s.departmentId == 0 && s.specializationId == 0).Select(a => a).FirstOrDefault();
                var CourseClosure = jntuh_noc_data.Where(s => s.departmentId != 0 && s.specializationId != 0).Select(a => a).ToList();

                contents += "<p style='text-align:left;font-size:10px;'><b><u>a) NOC required for: Change of Name of the Society</b></u></p><br/>";
                contents += "<table border='1' cellspacing='0' cellpadding='4' style='font-size:10px;'>";
                contents += "<tr style='font-weight:bold;'>";
                contents += "<th width='5%' style='text-align:center;'>S.No</th>";
                contents += "<th width='32%' style='text-align:left;'> Existing Society Name</th>";
                contents += "<th width='33%' style='text-align:left;'> Proposed Society Name</th>";

                contents += "</tr>";

                var oldCollegeAddress = db.jntuh_college_establishment.Where(c => c.collegeId == CollegeId).Select(c => c).FirstOrDefault();
                var NewCollegeAddress = db.jntuh_address_log.Where(c => c.collegeId == CollegeId && c.addressTye == "SOCIETY").Select(c => c).FirstOrDefault();
                string NewDistrictName = db.jntuh_district.Where(d => d.id == NewCollegeAddress.districtId).Select(D => D.districtName).FirstOrDefault().ToString();

                contents += "<tr style='font-size:10px;'>";
                contents += "<td width='5%' style='text-align:center;'> 1 </td>";
                contents += "<td width='32%' style='text-align:left;'>" + oldCollegeAddress.societyName + "</td>";

                if (NewCollegeAddress.collegeId != 0)
                {

                    contents += "<td width='33%' style='text-align:left;'>" + NewCollegeAddress.typename + "</td>";
                }
                else
                {

                    contents += "<td width='33%' style='text-align:left;'>---</td>";
                }

                contents += "</tr>";


                contents += "</table>";

                return contents;
            }
            return contents;
        }
        public string CollegeAddressChange(int? CollegeId)
        {
            string contents = string.Empty;
            int count = 1;
            int index = 1;
            int sno = 1;
            if (CollegeId != null)
            {
                var jntuh_academivYear = db.jntuh_academic_year.ToList();
                int actualYear = jntuh_academivYear.Where(s => s.isActive == true && s.isPresentAcademicYear == true).Select(a => a.id).FirstOrDefault();
                int PresentYear = jntuh_academivYear.Where(s => s.id == (actualYear + 1)).Select(s => s.id).FirstOrDefault();

                var college = db.jntuh_college.Where(s => s.id == CollegeId).Select(a => a).FirstOrDefault();
                var jntuh_noc_data = db.jntuh_college_noc_data.Where(s => s.academicyearId == PresentYear && s.collegeId == CollegeId).Select(a => a).ToList();

                var CollegeClosure = jntuh_noc_data.Where(s => s.departmentId == 0 && s.specializationId == 0).Select(a => a).FirstOrDefault();
                var CourseClosure = jntuh_noc_data.Where(s => s.departmentId != 0 && s.specializationId != 0).Select(a => a).ToList();

                contents += "<p style='text-align:left;font-size:10px;'><b><u>a) NOC required for: Change of Location of the College / Institute</b></u></p><br/>";
                contents += "<table border='1' cellspacing='0' cellpadding='4' style='font-size:10px;'>";
                contents += "<tr style='font-weight:bold;'>";
                contents += "<th width='5%' style='text-align:center;'>S.No</th>";
                contents += "<th width='32%' style='text-align:left;'> Current Location</th>";
                contents += "<th width='33%' style='text-align:left;'> New Location</th>";

                contents += "</tr>";

                var oldCollegeAddress = db.jntuh_address.Where(c => c.collegeId == CollegeId && c.addressTye == "COLLEGE").Select(c => c).FirstOrDefault();
                var NewCollegeAddress = db.jntuh_address_log.Where(c => c.collegeId == CollegeId && c.addressTye == "COLLEGE").Select(c => c).FirstOrDefault();
                string oldDistrictName = db.jntuh_district.Where(d => d.id == oldCollegeAddress.districtId).Select(D => D.districtName).FirstOrDefault().ToString();
                string NewDistrictName = db.jntuh_district.Where(d => d.id == NewCollegeAddress.districtId).Select(D => D.districtName).FirstOrDefault().ToString();

                contents += "<tr style='font-size:10px;'>";
                contents += "<td width='5%' style='text-align:center;'> 1 </td>";
                contents += "<td width='32%' style='text-align:left;'>" + oldCollegeAddress.address + " , " + oldCollegeAddress.townOrCity + " , " + oldCollegeAddress.mandal + " , " + oldDistrictName.ToString() + "</td>";

                if (NewCollegeAddress.collegeId != 0)
                {

                    contents += "<td width='33%' style='text-align:left;'>" + NewCollegeAddress.address + " , " + NewCollegeAddress.townOrCity + " , " + NewCollegeAddress.mandal + " , " + NewDistrictName.ToString() + "</td>";
                }
                else
                {

                    contents += "<td width='33%' style='text-align:left;'>---</td>";
                }

                contents += "</tr>";


                contents += "</table>";
                return contents;
            }
            return contents;
        }

        public ActionResult DownloadSocityAddressChangeAcknowlegement(int? noctypeid)
        {
            int userId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(collegeUser => collegeUser.userID == userId).Select(collegeUser => collegeUser.collegeID).FirstOrDefault();

            string path = SocityAddressChangeDownloadPath(userCollegeID, noctypeid);
            path = path.Replace("/", "\\");
            if (!string.IsNullOrEmpty(path))
            {
                return File(path, "application/pdf", path.Substring(path.LastIndexOf('\\') + 1));
            }

            return View();
        }
        public string SocityAddressChangeDownloadPath(int? CollegeId, int? nocid)
        {
            jntuh_college CollegeData = db.jntuh_college.Where(c => c.id == CollegeId).Select(c => c).FirstOrDefault();
            Document pdfDoc = new Document(PageSize.A4, 60, 50, 90, 50);

            // Create a new PdfWrite object, writing the output to a MemoryStream
            //open pdf
            var output = new MemoryStream();
            var writer = PdfWriter.GetInstance(pdfDoc, output);

            string path = Server.MapPath("~/Content/PDFReports/NOC/SocityAddressChange/");

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            string fullPath = path + CollegeData.collegeCode.ToUpper() + "_NOC_SocityAddressChangeAcknowlegement.pdf";
            PdfWriter pdfWriter = PdfWriter.GetInstance(pdfDoc, new FileStream(fullPath, FileMode.Create));

            var file = CollegeData.collegeCode.ToUpper() + "_NOC_SocityAddressChangeAcknowlegement.pdf";

            fullPath = string.Format("{0}/{1}", "/Content/PDFReports/NOC/SocityAddressChange/", file);

            //Open PDF Document to write data
            pdfDoc.Open();

            //Assign Html content in a string to write in PDF
            string contents = string.Empty;

            StreamReader sr;

            //Read file from server path
            sr = System.IO.File.OpenText(Server.MapPath("~/Content/ChangeSocityAddress.html"));

            //store content in the variable
            contents = sr.ReadToEnd();
            sr.Close();

            jntuh_address address = db.jntuh_address.Where(a => a.collegeId == CollegeData.id && (a.addressTye.Equals("COLLEGE"))).Select(a => a).FirstOrDefault();
            jntuh_address societyAddress = db.jntuh_address.Where(ad => (ad.collegeId == CollegeData.id) && (ad.addressTye.Equals("SOCIETY"))).Select(ad => ad).FirstOrDefault();

            string district = string.Empty;
            if (address != null)
            {
                district = db.jntuh_district.Find(address.districtId).districtName;
            }
            string scheduleCollegeAddress = string.Empty;
            string scheduleCollegeAddress1 = string.Empty;

            if (address != null)
            {
                scheduleCollegeAddress = CollegeData.collegeName + ",<br />" + address.address;
                scheduleCollegeAddress = scheduleCollegeAddress + ", " + address.townOrCity;
                scheduleCollegeAddress = scheduleCollegeAddress + ", " + address.mandal;
                scheduleCollegeAddress = scheduleCollegeAddress + ",<br />" + district;
                scheduleCollegeAddress = scheduleCollegeAddress + " - " + address.pincode;
            }

            if (address != null)
            {
                scheduleCollegeAddress1 = CollegeData.collegeName + ", " + address.address;
                scheduleCollegeAddress1 = scheduleCollegeAddress1 + ", " + address.townOrCity;
                scheduleCollegeAddress1 = scheduleCollegeAddress1 + ", " + address.mandal;
                scheduleCollegeAddress1 = scheduleCollegeAddress1 + ", " + district;
                scheduleCollegeAddress1 = scheduleCollegeAddress1 + " - " + address.pincode;
            }

            string CollegeSocietyAddress = string.Empty;
            if (societyAddress != null)
            {
                CollegeSocietyAddress = CollegeData.collegeName + ", " + societyAddress.address;
                CollegeSocietyAddress = CollegeSocietyAddress + ", " + societyAddress.townOrCity;
                CollegeSocietyAddress = CollegeSocietyAddress + ", " + societyAddress.mandal;
                CollegeSocietyAddress = CollegeSocietyAddress + ", " + district;
                CollegeSocietyAddress = CollegeSocietyAddress + " - " + societyAddress.pincode;
            }
            scheduleCollegeAddress = scheduleCollegeAddress.ToUpper().Replace(",", ", ").Replace("(", " (").Replace(")", ") ");
            CollegeSocietyAddress = CollegeSocietyAddress.ToUpper().Replace(",", ", ").Replace("(", " (").Replace(")", ") ");
            contents = contents.Replace("##SOCIETY_ADDRESS##", CollegeSocietyAddress);
            contents = contents.Replace("##COLLEGE_ADDRESS##", scheduleCollegeAddress);
            contents = contents.Replace("##COLLEGE_ADDRESS1##", scheduleCollegeAddress1);

            var College = CollegeData.collegeName + "(" + CollegeData.collegeCode + ")";

            //contents = contents.Replace("##College##", College);
            contents = contents.Replace("##CurrentDate##", DateTime.Now.ToString("dd-MM-yyy"));

            var NOCTypes = db.jntuh_college_paymentoffee_type.Where(a => (a.FeeType == "NOC For Closure of College" || a.FeeType == "NOC For Closure of Course") && a.isActive == true).Select(s => new { TypeId = s.id, Type = s.FeeType }).ToList();
            int CollegeNocId = NOCTypes.Where(e => e.Type == "NOC For Closure of College").Select(a => a.TypeId).FirstOrDefault();
            int CoursesNocId = NOCTypes.Where(e => e.Type == "NOC For Closure of Course").Select(a => a.TypeId).FirstOrDefault();

            // contents = contents.Replace("##CollegeAddressChange##", CollegeAddressChange(CollegeId));
            contents = contents.Replace("##SocityAddressChange##", SocityAddressChange(CollegeId));
            contents = contents.Replace("##PaymentDetails##", PaymentDetails(CollegeId));
            // contents = contents.Replace("##CollegeName##", "TEST COLLEGE");
            contents = contents.Replace("##CollegeName##", "<p style='float:right;'><b>" + College.ToString() + "</b></p>");
            contents = contents.Replace("##Enclosures##", enclosures(nocid));



            var parsedHtmlElements = HTMLWorker.ParseToList(new StringReader(contents), null);

            bool pageRotated = false;

            //Get each array values from parsed elements and add to the PDF document
            foreach (var htmlElement in parsedHtmlElements)
            {
                try
                {
                    if (htmlElement.Equals("<textarea>"))
                    {
                        pdfDoc.NewPage();
                    }

                    if (htmlElement.Chunks.Count >= 3)
                    {
                        if (htmlElement.Chunks.Count == 4)
                        {
                            pdfDoc.SetPageSize(iTextSharp.text.PageSize.A4.Rotate());
                            pdfDoc.SetMargins(50, 50, 90, 50);
                            pageRotated = true;
                        }
                        else
                        {
                            if (pageRotated == true)
                            {
                                pdfDoc.SetPageSize(iTextSharp.text.PageSize.A4);
                                pdfDoc.SetMargins(50, 50, 90, 50);
                                pageRotated = false;
                            }
                        }
                        pdfDoc.NewPage();
                    }
                    else
                    {
                        pdfDoc.Add((IElement)htmlElement);
                    }
                }
                catch (Exception ex)
                {
                    continue;

                }
            }

            //Close your PDF
            pdfDoc.Close();
            if (pdfDoc.IsOpen())
            {
                pdfDoc.Close();
            }
            string returnpath = string.Empty;
            returnpath = fullPath;
            return returnpath;
        }

        public ActionResult DownloadCollegeAddressChangeAcknowlegement(int? noctypeid)
        {
            int userId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(collegeUser => collegeUser.userID == userId).Select(collegeUser => collegeUser.collegeID).FirstOrDefault();

            string path = CollegeAddressChangeDownloadPath(userCollegeID, noctypeid);
            path = path.Replace("/", "\\");
            if (!string.IsNullOrEmpty(path))
            {
                return File(path, "application/pdf", path.Substring(path.LastIndexOf('\\') + 1));
            }

            return View();
        }
        public string CollegeAddressChangeDownloadPath(int? CollegeId, int? nocid)
        {
            jntuh_college CollegeData = db.jntuh_college.Where(c => c.id == CollegeId).Select(c => c).FirstOrDefault();
            Document pdfDoc = new Document(PageSize.A4, 60, 50, 90, 50);

            // Create a new PdfWrite object, writing the output to a MemoryStream
            //open pdf
            var output = new MemoryStream();
            var writer = PdfWriter.GetInstance(pdfDoc, output);

            string path = Server.MapPath("~/Content/PDFReports/NOC/CollegeAddressChange/");

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            string fullPath = path + CollegeData.collegeCode.ToUpper() + "_NOC_CollegeAddressChangeAcknowlegement.pdf";
            PdfWriter pdfWriter = PdfWriter.GetInstance(pdfDoc, new FileStream(fullPath, FileMode.Create));

            var file = CollegeData.collegeCode.ToUpper() + "_NOC_CollegeAddressChangeAcknowlegement.pdf";

            fullPath = string.Format("{0}/{1}", "/Content/PDFReports/NOC/CollegeAddressChange/", file);

            //Open PDF Document to write data
            pdfDoc.Open();

            //Assign Html content in a string to write in PDF
            string contents = string.Empty;

            StreamReader sr;

            //Read file from server path
            sr = System.IO.File.OpenText(Server.MapPath("~/Content/ChangeCollegeAddress.html"));

            //store content in the variable
            contents = sr.ReadToEnd();
            sr.Close();

            jntuh_address address = db.jntuh_address.Where(a => a.collegeId == CollegeData.id && (a.addressTye.Equals("COLLEGE"))).Select(a => a).FirstOrDefault();
            jntuh_address societyAddress = db.jntuh_address.Where(ad => (ad.collegeId == CollegeData.id) && (ad.addressTye.Equals("SOCIETY"))).Select(ad => ad).FirstOrDefault();

            string district = string.Empty;
            if (address != null)
            {
                district = db.jntuh_district.Find(address.districtId).districtName;
            }
            string scheduleCollegeAddress = string.Empty;
            string scheduleCollegeAddress1 = string.Empty;

            if (address != null)
            {
                scheduleCollegeAddress = CollegeData.collegeName + ",<br />" + address.address;
                scheduleCollegeAddress = scheduleCollegeAddress + ", " + address.townOrCity;
                scheduleCollegeAddress = scheduleCollegeAddress + ", " + address.mandal;
                scheduleCollegeAddress = scheduleCollegeAddress + ",<br />" + district;
                scheduleCollegeAddress = scheduleCollegeAddress + " - " + address.pincode;
            }

            if (address != null)
            {
                scheduleCollegeAddress1 = CollegeData.collegeName + ", " + address.address;
                scheduleCollegeAddress1 = scheduleCollegeAddress1 + ", " + address.townOrCity;
                scheduleCollegeAddress1 = scheduleCollegeAddress1 + ", " + address.mandal;
                scheduleCollegeAddress1 = scheduleCollegeAddress1 + ", " + district;
                scheduleCollegeAddress1 = scheduleCollegeAddress1 + " - " + address.pincode;
            }

            string CollegeSocietyAddress = string.Empty;
            if (societyAddress != null)
            {
                CollegeSocietyAddress = CollegeData.collegeName + ", " + societyAddress.address;
                CollegeSocietyAddress = CollegeSocietyAddress + ", " + societyAddress.townOrCity;
                CollegeSocietyAddress = CollegeSocietyAddress + ", " + societyAddress.mandal;
                CollegeSocietyAddress = CollegeSocietyAddress + ", " + district;
                CollegeSocietyAddress = CollegeSocietyAddress + " - " + societyAddress.pincode;
            }
            scheduleCollegeAddress = scheduleCollegeAddress.ToUpper().Replace(",", ", ").Replace("(", " (").Replace(")", ") ");
            CollegeSocietyAddress = CollegeSocietyAddress.ToUpper().Replace(",", ", ").Replace("(", " (").Replace(")", ") ");
            contents = contents.Replace("##SOCIETY_ADDRESS##", CollegeSocietyAddress);
            contents = contents.Replace("##COLLEGE_ADDRESS##", scheduleCollegeAddress);
            contents = contents.Replace("##COLLEGE_ADDRESS1##", scheduleCollegeAddress1);

            var College = CollegeData.collegeName + "(" + CollegeData.collegeCode + ")";

            //contents = contents.Replace("##College##", College);
            contents = contents.Replace("##CurrentDate##", DateTime.Now.ToString("dd-MM-yyy"));

            var NOCTypes = db.jntuh_college_paymentoffee_type.Where(a => (a.FeeType == "NOC For Closure of College" || a.FeeType == "NOC For Closure of Course") && a.isActive == true).Select(s => new { TypeId = s.id, Type = s.FeeType }).ToList();
            int CollegeNocId = NOCTypes.Where(e => e.Type == "NOC For Closure of College").Select(a => a.TypeId).FirstOrDefault();
            int CoursesNocId = NOCTypes.Where(e => e.Type == "NOC For Closure of Course").Select(a => a.TypeId).FirstOrDefault();

            contents = contents.Replace("##CollegeAddressChange##", CollegeAddressChange(CollegeId));
            contents = contents.Replace("##PaymentDetails##", PaymentDetails(CollegeId));
            contents = contents.Replace("##CollegeName##", "<p style='float:right;'><b>" + College.ToString() + "</b></p>");
            contents = contents.Replace("##Enclosures##", enclosures(nocid));



            var parsedHtmlElements = HTMLWorker.ParseToList(new StringReader(contents), null);

            bool pageRotated = false;

            //Get each array values from parsed elements and add to the PDF document
            foreach (var htmlElement in parsedHtmlElements)
            {
                try
                {
                    if (htmlElement.Equals("<textarea>"))
                    {
                        pdfDoc.NewPage();
                    }

                    if (htmlElement.Chunks.Count >= 3)
                    {
                        if (htmlElement.Chunks.Count == 4)
                        {
                            pdfDoc.SetPageSize(iTextSharp.text.PageSize.A4.Rotate());
                            pdfDoc.SetMargins(50, 50, 90, 50);
                            pageRotated = true;
                        }
                        else
                        {
                            if (pageRotated == true)
                            {
                                pdfDoc.SetPageSize(iTextSharp.text.PageSize.A4);
                                pdfDoc.SetMargins(50, 50, 90, 50);
                                pageRotated = false;
                            }
                        }
                        pdfDoc.NewPage();
                    }
                    else
                    {
                        pdfDoc.Add((IElement)htmlElement);
                    }
                }
                catch (Exception ex)
                {
                    continue;

                }
            }

            //Close your PDF
            pdfDoc.Close();
            if (pdfDoc.IsOpen())
            {
                pdfDoc.Close();
            }
            string returnpath = string.Empty;
            returnpath = fullPath;
            return returnpath;
        }
        public ActionResult CollegeaddressFeeDetailsandPayment(string CollegeId, int? nocTypeId)
        {
            int userId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            // int userCollegeID = db.jntuh_college_users.Where(collegeUser => collegeUser.userID == userId).Select(collegeUser => collegeUser.collegeID).FirstOrDefault();
            int userCollegeID = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(CollegeId, WebConfigurationManager.AppSettings["CryptoKey"]));
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            if (userCollegeID == 0 && CollegeId != null)
            {
                if (Roles.IsUserInRole("Admin"))
                {
                    userCollegeID = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(CollegeId, WebConfigurationManager.AppSettings["CryptoKey"]));
                }
            }

            var todayDate = DateTime.Now;
            var jntuh_academivYear = db.jntuh_academic_year.ToList();
            int actualYear = jntuh_academivYear.Where(s => s.isActive == true && s.isPresentAcademicYear == true).Select(a => a.id).FirstOrDefault();
            int PresentYear = jntuh_academivYear.Where(s => s.id == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
            int lid = db.jntuh_link_screens.Where(p => p.linkName == "College Address Change" && p.isActive == true).Select(s => s.id).FirstOrDefault();
            jntuh_college_links_assigned NOCPhase =
                db.jntuh_college_links_assigned.Where(l => l.linkId == lid && l.academicyearId == PresentYear && l.isActive == true && l.fromdate <= todayDate && l.todate >= todayDate).Select(s => s).FirstOrDefault();

            if (NOCPhase == null)
            {
                return RedirectToAction("College", "Dashboard");
            }
            var returnUrl = WebConfigurationManager.AppSettings["NocReturnUrl"];
            var merchantId = WebConfigurationManager.AppSettings["MerchantID"];
            var securityId = WebConfigurationManager.AppSettings["SecurityID"];
            var typefield1 = WebConfigurationManager.AppSettings["TypeField1"];
            var typefield2 = WebConfigurationManager.AppSettings["TypeField2"];

            string clgCode = db.jntuh_college.Where(a => a.id == userCollegeID).Select(a => a.collegeCode).FirstOrDefault();
            var NOCTypes = db.jntuh_college_paymentoffee_type.Where(a => (a.FeeType == "Change Of Location") && a.isActive == true).Select(s => new { TypeId = s.id, Type = s.FeeType }).ToList();
            int CoursesNocId = NOCTypes.Where(e => e.Type == "Change Of Location").Select(a => a.TypeId).FirstOrDefault();
            var payment = db.jntuh_paymentresponse.Where(s => s.AcademicYearId == PresentYear && s.CollegeId == clgCode && s.PaymentTypeID == CoursesNocId && s.AuthStatus == "0300").Select(q => q).ToList();

            ViewBag.totalFee = "75000";
            ViewBag.collegeCode = clgCode;
            ViewBag.challnNumber = clgCode + DateTime.Now.ToString("yyyMMddHHmmss");
            ViewBag.noctype = CoursesNocId;
            ViewBag.IsPaymentDone = false;
            decimal val = 75000;
            if (75000 == payment.Select(a => a.TxnAmount).Sum())
            {
                ViewBag.IsPaymentDone = true;
            }

            var msg = "";
            if (userCollegeID == 375)
            {
                msg = merchantId + "|" + ViewBag.challnNumber + "|NA|2.00|NA|NA|NA|INR|NA|" + typefield1 + "|" + securityId + "|NA|NA|" + typefield2 + "|NA|NA|NA|NA|NA|NA|NA|" + returnUrl;
                var key = WebConfigurationManager.AppSettings["ChecksumKey"];
                msg += "|" + GetHMACSHA256(msg, key).ToUpper();
            }
            else
            {
                msg = merchantId + "|" + ViewBag.challnNumber + "|NA|7500|NA|NA|NA|INR|NA|" + typefield1 + "|" + securityId + "|NA|NA|" + typefield2 + "|NA|NA|NA|NA|NA|NA|NA|" + returnUrl;
                var key = WebConfigurationManager.AppSettings["ChecksumKey"];
                msg += "|" + GetHMACSHA256(msg, key).ToUpper();
            }
            ViewBag.msg = msg;
            return View();
        }

        [HttpPost]
        // [Authorize(Roles = "Admin,College")]
        public ActionResult CollegesocityFeeDetailsandPayment(string msg)
        {
            //return RedirectToAction("CollegeDashboard", "Dashboard");
            var appSettings = WebConfigurationManager.AppSettings;
            var req = new jntuh_paymentrequests();
            CollegesocitySaveResponse(msg, "ChallanNumber");
            return RedirectToAction("College_NocAction");
        }

        [HttpPost]
        public ActionResult CollegeaddressSavePaymentRequest(string challanNumber, decimal txnAmount, string collegeCode, int? noctype)
        {
            var jntuh_academivYear = db.jntuh_academic_year.ToList();
            int actualYear = jntuh_academivYear.Where(s => s.isActive == true && s.isPresentAcademicYear == true).Select(a => a.id).FirstOrDefault();
            int PresentYear = jntuh_academivYear.Where(s => s.id == (actualYear + 1)).Select(s => s.id).FirstOrDefault();

            var appSettings = WebConfigurationManager.AppSettings;
            var req = new jntuh_paymentrequests();
            req.AcademicYearId = PresentYear;
            req.TxnAmount = txnAmount;
            req.CollegeCode = collegeCode;

            req.ChallanNumber = challanNumber;
            req.MerchantID = appSettings["MerchantID"];
            req.CustomerID = appSettings["CustomerID"];
            req.SecurityID = appSettings["SecurityID"];
            req.CurrencyType = appSettings["CurrencyType"];
            req.TxnDate = DateTime.Now;
            req.PaymentTypeID = Convert.ToInt32(noctype); ;
            db.jntuh_paymentrequests.Add(req);
            db.SaveChanges();
            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }
        //[HttpPost]
        //public ActionResult SocityaddressSavePaymentRequest(string challanNumber, decimal txnAmount, string collegeCode, int? noctype)
        //{
        //    var jntuh_academivYear = db.jntuh_academic_year.ToList();
        //    int actualYear = jntuh_academivYear.Where(s => s.isActive == true && s.isPresentAcademicYear == true).Select(a => a.id).FirstOrDefault();
        //    int PresentYear = jntuh_academivYear.Where(s => s.id == (actualYear + 1)).Select(s => s.id).FirstOrDefault();

        //    var appSettings = WebConfigurationManager.AppSettings;
        //    var req = new jntuh_paymentrequests();
        //    req.AcademicYearId = PresentYear;
        //    req.TxnAmount = txnAmount;
        //    req.CollegeCode = collegeCode;

        //    req.ChallanNumber = challanNumber;
        //    req.MerchantID = appSettings["MerchantID"];
        //    req.CustomerID = appSettings["CustomerID"];
        //    req.SecurityID = appSettings["SecurityID"];
        //    req.CurrencyType = appSettings["CurrencyType"];
        //    req.TxnDate = DateTime.Now;
        //    req.PaymentTypeID = Convert.ToInt32(noctype); ;
        //    db.jntuh_paymentrequests.Add(req);
        //    db.SaveChanges();
        //    return new HttpStatusCodeResult(HttpStatusCode.OK);
        //}

        private void CollegesocitySaveResponse(string responseMsg, string challanno)
        {
            var tokens = responseMsg.Split('|');
            int userID = 0;
            int userCollegeID = 0;
            string clgCode = string.Empty;
            if (Membership.GetUser() == null)
            {
                //return RedirectToAction("LogOn", "Account");
                string cid = tokens[1];
                clgCode = cid.Substring(0, 2);
                userCollegeID =
                    db.jntuh_college.Where(c => c.collegeCode == clgCode.Trim()).Select(s => s.id).FirstOrDefault();
                userID = db.jntuh_college_users.Where(u => u.collegeID == userCollegeID).Select(u => u.userID).FirstOrDefault();
            }
            else
            {
                userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
                userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
                clgCode = db.jntuh_college.SingleOrDefault(a => a.id == userCollegeID).collegeCode;
            }

            //Response Message Saving
            temp_aeronautical temp_aeronautical = new temp_aeronautical();
            temp_aeronautical.Degree = responseMsg.Length < 255
                ? responseMsg
                : responseMsg.Substring(0, 254); ;
            temp_aeronautical.Department = "NOC";
            temp_aeronautical.Specialization = clgCode;
            temp_aeronautical.DegreeId = userCollegeID;
            temp_aeronautical.LabCode = DateTime.Now.ToString();
            db.temp_aeronautical.Add(temp_aeronautical);
            db.SaveChanges();

            var jntuh_academivYear = db.jntuh_academic_year.ToList();
            int actualYear = jntuh_academivYear.Where(s => s.isActive == true && s.isPresentAcademicYear == true).Select(a => a.id).FirstOrDefault();
            int PresentYear = jntuh_academivYear.Where(s => s.id == (actualYear + 1)).Select(s => s.id).FirstOrDefault();

            var dbResponse = new UAAAS.Models.jntuh_paymentresponse();
            dbResponse.AcademicYearId = PresentYear;
            dbResponse.MerchantID = tokens[0];
            dbResponse.CustomerID = tokens[1];
            dbResponse.TxnReferenceNo = tokens[2];
            dbResponse.BankReferenceNo = tokens[3];
            dbResponse.TxnAmount = decimal.Parse(tokens[4]);
            dbResponse.BankID = tokens[5];
            dbResponse.BankMerchantID = tokens[6];
            dbResponse.TxnType = tokens[7];
            dbResponse.CurrencyName = tokens[8];
            dbResponse.TxnDate = DateTime.Now;
            dbResponse.AuthStatus = tokens[14];
            dbResponse.SettlementType = tokens[15];
            dbResponse.ErrorStatus = tokens[23];
            dbResponse.ErrorDescription = tokens[24];
            dbResponse.CollegeId = clgCode;
            dbResponse.ChallanNumber = challanno;
            dbResponse.PaymentTypeID = Convert.ToInt32(db.jntuh_paymentrequests.Where(a => a.CollegeCode == clgCode && a.ChallanNumber == dbResponse.CustomerID).Select(a => a.PaymentTypeID).FirstOrDefault());
            // dbResponse.noctypeId = db.jntuh_paymentrequests.Where(a => a.CollegeCode == clgCode && a.ChallanNumber == dbResponse.CustomerID).Select(a => a.noctypeId).FirstOrDefault();
            db.jntuh_paymentresponse.Add(dbResponse);
            db.SaveChanges();
            TempData["SUCCESS"] = "Your Payment is Done";


            string collegeid = db.jntuh_paymentresponse.Where(a => a.CollegeId == clgCode && a.ChallanNumber == dbResponse.CustomerID && a.PaymentTypeID == dbResponse.PaymentTypeID && a.AuthStatus == "0300").Select(a => a.CollegeId).FirstOrDefault();
            if (!string.IsNullOrEmpty(collegeid))
            {
                var jntuh_college_noc_data = db.jntuh_college_noc_data.Where(q => q.academicyearId == PresentYear && q.collegeId == userCollegeID && q.noctypeId == dbResponse.PaymentTypeID && q.isClosure == false).Select(a => a).ToList();
                foreach (var item in jntuh_college_noc_data)
                {
                    item.isClosure = true;
                    db.Entry(item).State = System.Data.EntityState.Modified;
                    db.SaveChanges();
                }
            }

            //mail
            var collegename = db.jntuh_college.SingleOrDefault(a => a.id == userCollegeID).collegeName;
            var membershipmailid = db.my_aspnet_membership.Where(i => i.userId == userID).FirstOrDefault().Email;
            IUserMailer mailer = new UserMailer();
            mailer.PaymentResponse(membershipmailid, "Payment Response", dbResponse.CollegeId + " / " + collegename, dbResponse.CustomerID, dbResponse.TxnReferenceNo, dbResponse.BankReferenceNo, dbResponse.TxnAmount, dbResponse.TxnDate.ToString(), dbResponse.ErrorDescription, dbResponse.ChallanNumber, "Payment Response", "JNTUH-AAC-ONLINE APPLICATION PAYMENT STATUS").SendAsync();
        }
        #endregion
    }

    public class College_Noc
    {
        public int CollegeId { get; set; }
        public string EncrytCollegeid { get; set; }
        public string CollegeCode { get; set; }
        public string CollegeName { get; set; }
        // [Required]
        public int NocTypeId { get; set; }
        public string NocType { get; set; }
        public string EncrytNocType { get; set; }
        public int DepartmentId { get; set; }
        public string Department { get; set; }
        public int SpecializationId { get; set; }
        public string Specialization { get; set; }
        public string College_Status { get; set; }
        public string Noc_Status { get; set; }
        // [Required]
        public string Remarks { get; set; }
        public int PaymentId { get; set; }
        public string PaymentDate { get; set; }
        public decimal? PaymentAmount { get; set; }
        public decimal? PaymentDivision { get; set; }
        public bool PaymentStatus { get; set; }
        public int Noc_DataId { get; set; }
        public List<IntakeExisting> IntakeDetails { get; set; }

        public int CollegeNocId { get; set; }
        public int CoursesNocId { get; set; }
    }

    public class IntakeExisting
    {
        public int nocid { get; set; }
        public int academicyearid { get; set; }
        public int DegreeId { get; set; }
        public string Degree { get; set; }
        public int DepartmentId { get; set; }
        public string Department { get; set; }
        public int SpecializationId { get; set; }
        public string Specialization { get; set; }
        public int? ApprovedIntake { get; set; }
        public string CourseStatus { get; set; }
        public int? shiftId { get; set; }
        public int Noc_SpecId { get; set; }

        public int NOCTypeID { get; set; }
    }

    public class CourseClosure
    {
        public int? DegreeId { get; set; }
        public string Degree { get; set; }
        public int? departmentId { get; set; }
        public string Department { get; set; }
        public int? specializationId { get; set; }
        public string Specialization { get; set; }
        public int? ApprovedIntake { get; set; }
        // public string CourseStatus { get; set; }
        public int? ShiftId { get; set; }
        public int? NocTypeId { get; set; }
    }

    public class address
    {
        public string clgaddress { get; set; }
        public string clgtown { get; set; }
        public string clgmandal { get; set; }
        public string clgdistrict { get; set; }
        public int clgpincode { get; set; }

        public int Id { get; set; }
        public int academicyearId { get; set; }
        public int collegeId { get; set; }
        public string addresstype { get; set; }
        [Required(ErrorMessage = "*")]
        public string caddress { get; set; }
        [Required(ErrorMessage = "*")]
        public string townOrcity { get; set; }
        [Required(ErrorMessage = "*")]
        public string mandal { get; set; }
        [Required(ErrorMessage = "*")]
        public int? districtId { get; set; }
        public string district { get; set; }
        [Required(ErrorMessage = "*")]
        //[StringLength(6, MinimumLength = 6, ErrorMessage = "Maximum 6 characters")]
        // [RegularExpression(@"\d{6}", ErrorMessage = "Invalid mobile")]
        public int pincode { get; set; }
        public string typename { get; set; }
        public string clgsociety { get; set; }
        public int NOCtype { get; set; }

    }

    public class society
    {
        public string clgsociety { get; set; }
        public int Id { get; set; }
        public int NOCtype { get; set; }
        [Required(ErrorMessage = "*")]
        public string societyname { get; set; }
    }

    public class IntakechangeaddDetails
    {
        public int Id { get; set; }
        public string DepartmentName { get; set; }
        public string DegreeName { get; set; }
        public string SpecializationName { get; set; }
        public int Intake { get; set; }
        public decimal Amount { get; set; }
        public int shiftId { get; set; }
        public decimal TotalAmount { get; set; }
        public int ApprovedIntake { get; set; }
        public string CourseStatus { get; set; }
        public bool Isclouser { get; set; }
    }
}
