using iTextSharp.text;
using iTextSharp.text.html.simpleparser;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Security;
using DotNetOpenAuth.Messaging;
using UAAAS.Controllers.Admin;
using UAAAS.Models;
using System.Configuration;
using System.Text;
using System.Security.Cryptography;
using System.Net;
using System.Drawing.Imaging;
using System.Threading;

namespace UAAAS.Controllers.College
{
    [ErrorHandling]
    public class CollegeSCMProceedingsRequestController : BaseController
    {
        //
        // GET: /CollegeSCMProceedingsRequest/
        private uaaasDBContext db = new uaaasDBContext();
        private string serverURL;
        private string barcodetext;

        [Authorize(Roles = "College")]
        [HttpGet]
        public ActionResult CollegeScmProceedingsRequest()
        {
            DateTime todayDate = DateTime.Now;
            var userId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            var userCollegeId = db.jntuh_college_users.Where(u => u.userID == userId).Select(u => u.collegeID).FirstOrDefault();
            int status = db.jntuh_college_edit_status.Where(editStatus => editStatus.collegeId == userCollegeId &&
                                                                                     editStatus.IsCollegeEditable == true &&
                                                                                     editStatus.editFromDate <= todayDate &&
                                                                                     editStatus.editToDate >= todayDate)
                                                                .Select(editStatus => editStatus.id)
                                                                .FirstOrDefault();

            if (userCollegeId == 375)
            {
                userCollegeId = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int ay0 = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();
            int lid =
                db.jntuh_link_screens.Where(p => p.linkName == "Faculty SCM Request" && p.isActive == true).Select(s => s.id).FirstOrDefault();
            jntuh_college_links_assigned scmphase =
                db.jntuh_college_links_assigned.Where(l => l.linkId == lid && l.academicyearId == ay0 && l.isActive == true && l.fromdate <= todayDate && l.todate >= todayDate).Select(s => s).FirstOrDefault();
            if (scmphase == null && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("College", "Dashboard");
            }

            #region scm Submission Date End when will stract uncommentted this
            //SCM Appeal Starts From 10-04-2018
            //DateTime SCMStartDate = new DateTime(2018, 08,10);
            //DateTime SCMStartDate1 = new DateTime(2019, 02, 11);
            DateTime SCMStartDate = Convert.ToDateTime(scmphase.fromdate);
            ScmProceedingsRequest scmProceedings = new ScmProceedingsRequest();
            string clgCode;

            var firstOrDefault = db.jntuh_college.FirstOrDefault(a => a.id == userCollegeId);
            var specs = new List<DistinctSpecializations>();
            var depts = new List<DistinctDepartments>();
            var degrees = db.jntuh_degree.AsNoTracking().ToList();
            var specializations = db.jntuh_specialization.AsNoTracking().ToList();
            var departments = db.jntuh_department.AsNoTracking().ToList();
            List<int> collegespecs = new List<int>();
            //collegespecs.AddRange(db.jntuh_approvedadmitted_intake.Where(i => i.collegeId == userCollegeId).Select(i => i.SpecializationId).Distinct().ToArray());
            collegespecs.AddRange(db.jntuh_college_intake_existing.Where(i => i.collegeId == userCollegeId && (i.academicYearId == (ay0 - 1) || (i.academicYearId == (ay0 - 2)))).Select(i => i.specializationId).Distinct().ToArray());

            int[] degreeIds = (from a in specializations
                               join b in departments on a.departmentId equals b.id
                               join c in degrees on b.degreeId equals c.id
                               where collegespecs.Contains(a.id)
                               select c.id).Distinct().ToArray();
            if (degreeIds.Contains(4))
            {
                var humanitesSpeci = new[] { 37, 48, 42, 31, 154 };
                collegespecs.AddRange(humanitesSpeci);
            }

            //Others Departments
            if (collegespecs.Contains(154))
            {
                var othersSpeci = new[] { 155, 156, 157, 158 };
                collegespecs.AddRange(othersSpeci);
            }

            collegespecs.Remove(154);
            int[] degreeids = { 4, 3, 5, 6, 7 };
            foreach (var s in collegespecs)
            {
                var specid = specializations.FirstOrDefault(i => i.id == s);

                if (specid != null)
                {
                    var deptment = departments.FirstOrDefault(i => i.id == specid.departmentId && degreeids.Contains(i.degreeId));
                    var degree = degrees.FirstOrDefault(i => i.id == (deptment != null ? deptment.degreeId : 0));
                    if (degree != null)
                        specs.Add(new DistinctSpecializations { SpecializationId = specid.id, SpecializationName = deptment.departmentDescription, DepartmentId = specid.departmentId });
                }
            }

            ViewBag.departments = specs.OrderBy(i => i.DepartmentId);

            //var collegescmrequestslist = db.jntuh_scmproceedingsrequests.AsNoTracking().Where(i => i.CollegeId == userCollegeId && SCMStartDate < i.CreatedOn && i.RequestSubmittedDate == null && i.PaymentTypeId == null).ToList();
            var collegescmrequestslist = db.jntuh_scmproceedingsrequests.AsNoTracking().Where(i => i.CollegeId == userCollegeId && SCMStartDate < i.CreatedOn).ToList();
            var collegescmaddedFaculty = db.jntuh_scmproceedingrequest_addfaculty.AsNoTracking().Where(i => i.CollegeId == userCollegeId).ToList();

            var proceedingsRequests = new List<ScmProceedingsRequest>();
            scmProceedings.scmtotalrequests = new List<ScmProceedingsRequest>();
            foreach (var s in collegescmrequestslist)
            {

                var specid = specializations.FirstOrDefault(i => i.id == s.SpecializationId);

                if (specid != null && firstOrDefault != null)
                {
                    var deptment = departments.FirstOrDefault(i => i.id == specid.departmentId);
                    var degree = degrees.FirstOrDefault(i => i.id == (deptment != null ? deptment.degreeId : 0));
                    if (degree != null)
                        // if (s.RequiredProfessor != null)
                        proceedingsRequests.Add(new ScmProceedingsRequest
                            {
                                CollegeName = firstOrDefault.collegeCode + " - " + firstOrDefault.collegeName,
                                CollegeCode = firstOrDefault.collegeCode,
                                ProfessorVacancies = s.ProfessorsCount.ToString(),
                                AssociateProfessorVacancies = s.AssociateProfessorsCount.ToString(),
                                AssistantProfessorVacancies = s.AssistantProfessorsCount.ToString(),
                                DepartmentName = deptment.departmentDescription,
                                SpecializationId = specid.id,
                                CollegeId = firstOrDefault.id,
                                DepartmentId = specid.departmentId,
                                ScmNotificationpath = s.SCMNotification,
                                Id = s.ID,
                                RequiredProfessorVacancies = s.RequiredProfessor.ToString(),
                                RequiredAssistantProfessorVacancies = s.RequiredAssistantProfessor.ToString(),
                                RequiredAssociateProfessorVacancies = s.RequiredAssociateProfessor.ToString(),
                                CreatedDate = s.CreatedOn,
                                Checked = false,
                                isscmatUniversity = s.isscmatUniversity,
                                RequestSubmittedDate = s.RequestSubmittedDate,
                                paymentId = s.PaymentTypeId,
                                TotalFacultyRequired = collegescmaddedFaculty.Where(a => s.CollegeId == a.CollegeId && s.ID == a.ScmProceedingId).Count()
                            });
                }
            }
            scmProceedings.scmtotalrequests.AddRange(proceedingsRequests.OrderByDescending(e => e.CreatedDate).Select(e => e).ToList());
            //scmProceedings.isscmatUniversity = null;
            ViewBag.collegescmrequestslist = scmProceedings.scmtotalrequests.Where(r => r.RequestSubmittedDate == null && r.paymentId == null).Select(s => s).ToList();
            //scmProceedings.ScmProceedingsRequestslist = scmProceedings.scmtotalrequests.Where(r => r.RequestSubmittedDate == null && r.paymentId == null).Select(s => s).ToList();
            scmProceedings.ScmProceedingsRequestslist = scmProceedings.scmtotalrequests.Where(r => r.isscmatUniversity == true).Select(s => s).ToList();
            ViewBag.collegescmrequestslist = scmProceedings.ScmProceedingsRequestslist;
            //Selection Process at University

            scmProceedings.collegeatuniversity = scmProceedings.scmtotalrequests.Where(
                    r => r.isscmatUniversity == false).Select(s => s).ToList();
            ViewBag.sellectionatcollege = scmProceedings.collegeatuniversity;
            //Payment Details
            string payclgcode = db.jntuh_college.Where(c => c.id == userCollegeId && c.isActive == true).Select(s => s.collegeCode).FirstOrDefault();
            var paymenthistory =
               db.jntuh_paymentresponse.Where(
                   it =>
                       it.CollegeId == payclgcode && it.AcademicYearId == ay0 && it.AuthStatus == "0300" &&
                       it.TxnDate.Year == todayDate.Year && it.PaymentTypeID == 9)
                   .Select(s => s).ToList();
            ViewBag.Payments = paymenthistory;

            ViewBag.editable = false;
            #endregion
            return View(scmProceedings);
        }

        [HttpPost]
        [Authorize(Roles = "College")]
        public ActionResult CollegeScmProceedingsRequest(ScmProceedingsRequest scmrequest)
        {
            var todayDate = DateTime.Now;
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int ay0 = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();
            int lid = db.jntuh_link_screens.Where(p => p.linkName == "Faculty SCM Request" && p.isActive == true).Select(s => s.id).FirstOrDefault();
            jntuh_college_links_assigned scmphase =
                db.jntuh_college_links_assigned.Where(l => l.linkId == lid && l.academicyearId == ay0 && l.isActive == true && l.fromdate <= todayDate && l.todate >= todayDate).Select(s => s).FirstOrDefault();

            if (scmphase == null)
            {
                return RedirectToAction("College", "Dashboard");
            }
            var userId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            var userCollegeId = db.jntuh_college_users.Where(u => u.userID == userId).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeId == 375)
            {
                userCollegeId = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            if (userCollegeId == 0)
            {
                return RedirectToAction("CollegeScmProceedingsRequest");
            }

            if (ModelState.IsValid)
            {
                int SCmOrder = 1;
                var fileName = string.Empty;
                var filepath = string.Empty;
                var collegescmrequests = new jntuh_scmproceedingsrequests();

                const string scmnotificationpath = "~/Content/Upload/SCMPROCEEDINGSREQUEST/ScmNotificationDocuments";

                //Checking For Unic Request
                collegescmrequests.CollegeId = userCollegeId;
                collegescmrequests.academicyearId = scmphase.academicyearId;
                collegescmrequests.SpecializationId = scmrequest.SpecializationId;
                var specialization =
                    db.jntuh_specialization.AsNoTracking()
                        .FirstOrDefault(i => i.id == scmrequest.SpecializationId);
                var department =
                    db.jntuh_department.AsNoTracking().FirstOrDefault(i => i.id == specialization.departmentId);
                collegescmrequests.DEpartmentId = specialization != null ? specialization.departmentId : 0;
                var ishaveScmRequest =
                    db.jntuh_scmproceedingsrequests.Where(
                        s =>
                            s.CollegeId == userCollegeId && s.academicyearId == ay0 &&
                            s.DEpartmentId == collegescmrequests.DEpartmentId &&
                            s.SpecializationId == collegescmrequests.SpecializationId && s.phaseId == scmphase.phaseId && s.RequestSubmittedDate == null)
                        .Select(s => s).FirstOrDefault();
                if (ishaveScmRequest != null)
                {
                    TempData["Error"] = "you can have make only one request for one Department.";
                    return RedirectToAction("CollegeScmProceedingsRequest");
                }

                if (scmrequest.ScmNotificationSupportDoc != null)
                {
                    if (!Directory.Exists(Server.MapPath(scmnotificationpath)))
                    {
                        Directory.CreateDirectory(Server.MapPath(scmnotificationpath));
                    }

                    var ext = Path.GetExtension(scmrequest.ScmNotificationSupportDoc.FileName);
                    if (ext != null && ext.ToUpper().Equals(".PDF"))
                    {
                        var scmfileName = db.jntuh_college.Where(c => c.id == userCollegeId).Select(c => c.id).FirstOrDefault() + "_" + "ScmNotofication" + "_" + DateTime.Now.ToString("yyyMMddHHmmss");
                        scmrequest.ScmNotificationSupportDoc.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(scmnotificationpath), scmfileName, ext));
                        collegescmrequests.SCMNotification = scmfileName + ext;
                    }
                    IUserMailer mailer = new UserMailer();

                    collegescmrequests.phaseId = scmphase.phaseId;

                    collegescmrequests.DegreeId = department != null ? department.degreeId : 0;
                    collegescmrequests.ProfessorsCount = Convert.ToInt16(scmrequest.ProfessorVacancies);
                    collegescmrequests.AssociateProfessorsCount = Convert.ToInt16(scmrequest.AssociateProfessorVacancies);
                    collegescmrequests.AssistantProfessorsCount = Convert.ToInt16(scmrequest.AssistantProfessorVacancies);
                    collegescmrequests.RequiredProfessor = Convert.ToInt16(scmrequest.RequiredProfessorVacancies);
                    collegescmrequests.RequiredAssistantProfessor = Convert.ToInt16(scmrequest.RequiredAssistantProfessorVacancies);
                    collegescmrequests.RequiredAssociateProfessor = Convert.ToInt16(scmrequest.RequiredAssociateProfessorVacancies);
                    if (scmrequest.NotificationDate != null)
                        collegescmrequests.Notificationdate = UAAAS.Models.Utilities.DDMMYY2MMDDYY(scmrequest.NotificationDate);
                    //collegescmrequests.isscmatUniversity = scmrequest.isscmatUniversity; // for assistant professor it will be either selection at University /College
                    collegescmrequests.isscmatUniversity = true; // for professor default selection at University 
                    collegescmrequests.Remarks = scmrequest.Remarks;
                    collegescmrequests.CreatedBy = userId;
                    collegescmrequests.CreatedOn = DateTime.Now;
                    collegescmrequests.ISActive = true;
                    collegescmrequests.TotalNoofFacultyRequired = Convert.ToInt16(scmrequest.TotalFacultyRequired);
                    //Checking SCM Order Id
                    var scmdata = db.jntuh_scmproceedingsrequests.Where(e => e.ISActive == true && e.CollegeId == scmrequest.CollegeId && e.SpecializationId == collegescmrequests.SpecializationId &&
                                e.DEpartmentId == collegescmrequests.DEpartmentId && e.DegreeId == collegescmrequests.DegreeId).OrderByDescending(e => e.ID).Select(e => e).FirstOrDefault();

                    if (scmdata != null)
                    {
                        var assigneddata = db.jntuh_auditor_assigned.Where(e => e.ScmId == scmdata.ID).Select(e => e.Id).FirstOrDefault();
                        if (assigneddata != 0)
                        {
                            SCmOrder = scmdata.SCMOrder + 1;
                        }
                        else
                        {
                            SCmOrder = scmdata.SCMOrder;
                        }
                    }
                    collegescmrequests.SCMOrder = SCmOrder;
                    db.jntuh_scmproceedingsrequests.Add(collegescmrequests);
                    try
                    {
                        db.SaveChanges();

                        var attachments = new List<Attachment>();
                        if (scmrequest.ScmNotificationSupportDoc != null)
                        {

                            fileName = Path.GetFileName(scmrequest.ScmNotificationSupportDoc.FileName);
                            if (!Directory.Exists(Server.MapPath("~/Content/Attachments")))
                            {
                                Directory.CreateDirectory(Server.MapPath("~/Content/Attachments"));
                            }
                            filepath = Path.Combine(Server.MapPath("~/Content/Attachments"), fileName);
                            scmrequest.ScmNotificationSupportDoc.SaveAs(filepath);
                            attachments.Add(new Attachment(filepath));
                            //  mailer.SendAttachmentToAllColleges("sureshpalsa1@gmail.com", "", "",
                            //   "SCM PROCEEDINGS REQUEST", "Scm Requests", attachments).SendAsync();
                            TempData["Success"] = "Your request has been proccessed successfully..";
                        }

                    }
                    catch (DbEntityValidationException dbEx)
                    {
                        foreach (var validationErrors in dbEx.EntityValidationErrors)
                        {
                            foreach (var validationError in validationErrors.ValidationErrors)
                            {
                                Trace.TraceInformation("Property: {0} Error: {1}",
                                    validationError.PropertyName,
                                    validationError.ErrorMessage);
                            }
                        }
                    }
                }
                else
                {
                    TempData["Error"] = "Please Fill All Mandatory Fields..";
                }
            }


            return RedirectToAction("CollegeScmProceedingsRequest");
        }

        [Authorize(Roles = "Admin")]
        public ActionResult CollegeScmProceedingsRequestView(string id)
        {
            var userCollegeId = 0;
            if (Roles.IsUserInRole("Admin"))
            {
                if (id != null)
                {
                    userCollegeId = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(id, WebConfigurationManager.AppSettings["CryptoKey"]));
                }
            }
            var firstOrDefault = db.jntuh_college.FirstOrDefault(a => a.id == userCollegeId);
            var specs = new List<DistinctSpecializations>();
            var degrees = db.jntuh_degree.AsNoTracking().ToList();
            var specializations = db.jntuh_specialization.AsNoTracking().ToList();
            var departments = db.jntuh_department.AsNoTracking().ToList();
            var collegespecs = db.jntuh_college_intake_existing.Where(i => i.collegeId == userCollegeId).Select(i => i.specializationId).Distinct().ToArray();
            foreach (var s in collegespecs)
            {
                var specid = specializations.FirstOrDefault(i => i.id == s);

                if (specid != null)
                {
                    var deptment = departments.FirstOrDefault(i => i.id == specid.departmentId);
                    var degree = degrees.FirstOrDefault(i => i.id == (deptment != null ? deptment.degreeId : 0));
                    if (degree != null)
                        specs.Add(new DistinctSpecializations { SpecializationId = specid.id, SpecializationName = degree.degree + " - " + specid.specializationName, DepartmentId = specid.departmentId });
                }
            }
            ViewBag.departments = specs.OrderBy(i => i.DepartmentId);
            var collegescmrequestslist = db.jntuh_scmproceedingsrequests.AsNoTracking().Where(i => i.CollegeId == userCollegeId).ToList();
            var proceedingsRequests = new List<ScmProceedingsRequest>();
            foreach (var s in collegescmrequestslist)
            {
                var specid = specializations.FirstOrDefault(i => i.id == s.SpecializationId);

                if (specid != null && firstOrDefault != null)
                {
                    var deptment = departments.FirstOrDefault(i => i.id == specid.departmentId);
                    var degree = degrees.FirstOrDefault(i => i.id == (deptment != null ? deptment.degreeId : 0));
                    if (degree != null)
                        proceedingsRequests.Add(new ScmProceedingsRequest
                        {
                            CollegeName = firstOrDefault.collegeCode + " - " + firstOrDefault.collegeName,
                            CollegeId = firstOrDefault.id,
                            SpecializationId = s.SpecializationId,
                            ProfessorVacancies = s.ProfessorsCount.ToString(),
                            AssociateProfessorVacancies = s.AssociateProfessorsCount.ToString(),
                            AssistantProfessorVacancies = s.AssistantProfessorsCount.ToString(),
                            SpecializationName = degree.degree + " - " + specid.specializationName,
                            DepartmentId = specid.departmentId,
                            CreatedDate = s.CreatedOn,
                            ScmNotificationpath = s.SCMNotification,
                            RequiredProfessorVacancies = s.RequiredProfessor.ToString(),
                            RequiredAssociateProfessorVacancies = s.RequiredAssociateProfessor.ToString(),
                            RequiredAssistantProfessorVacancies = s.RequiredAssistantProfessor.ToString(),
                            Id = s.ID
                        });
                }
            }
            ViewBag.collegescmrequestslist = proceedingsRequests.OrderByDescending(e => e.CreatedDate).Select(e => e).ToList();
            return View();
        }



        [Authorize(Roles = "Admin")]
        [HttpGet]
        public ActionResult CollegeScmProceedingsRequestScreen()
        {
            var colleges = db.jntuh_college.AsNoTracking().ToList();
            var collegescmrequestslist = db.jntuh_scmproceedingsrequests.AsNoTracking().ToList();
            var proceedingsRequests = new List<ScmProceedingsRequest>();
            foreach (var s in collegescmrequestslist.GroupBy(i => i.CollegeId))
            {
                var colldetails = colleges.FirstOrDefault(i => i.id == s.Key);
                proceedingsRequests.Add(new ScmProceedingsRequest
                {
                    CollegeId = s.Key,
                    CollegeName = colldetails != null ? colldetails.collegeCode + " - " + colldetails.collegeName : "",
                });
            }
            ViewBag.collegescmrequestslist = proceedingsRequests;
            return View();
        }

        //This Print Faculty for College Select Option for Selection Process at College
        public ActionResult sellectioncommetteatcollegeprint()
        {
            //if (paymentid != 0)
            //{
            DateTime todayDate = DateTime.Now;
            var userId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            var userCollegeId = db.jntuh_college_users.Where(u => u.userID == userId).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeId == 375)
            {
                userCollegeId = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int ay0 = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();
            int lid =
                db.jntuh_link_screens.Where(p => p.linkName == "Faculty SCM Request" && p.isActive == true).Select(s => s.id).FirstOrDefault();
            jntuh_college_links_assigned scmphase =
                db.jntuh_college_links_assigned.Where(l => l.linkId == lid && l.academicyearId == ay0 && l.isActive == true && l.fromdate <= todayDate && l.todate >= todayDate).Select(s => s).FirstOrDefault();
            var college = db.jntuh_college.Where(r => r.id == userCollegeId && r.isActive == true).Select(s => s).FirstOrDefault();
            if (scmphase == null && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("College", "Dashboard");
            }

            serverURL = Request.Url.Scheme + System.Uri.SchemeDelimiter + Request.Url.Host + (Request.Url.IsDefaultPort ? "" : ":" + Request.Url.Port);
            //var checkedscmlistdata = scmlist.ScmProceedingsRequestslist.Where(e => e.Checked == true).ToList();
            var checkedscmlistdata = db.jntuh_scmproceedingsrequests.Where(r => r.PaymentTypeId == null && r.RequestSubmittedDate != null && r.CollegeId == userCollegeId && r.phaseId == scmphase.phaseId && r.CreatedOn >= scmphase.fromdate).Select(s => s).ToList();
            if (checkedscmlistdata.Count() != 0)
            {
                var pdfPath = string.Empty;
                int preview = 0;
                if (preview == 0)
                {
                    pdfPath = SaveFacultyDataPdfatcollege(preview, checkedscmlistdata);
                    pdfPath = pdfPath.Replace("/", "\\");
                }
                return File(pdfPath, "application/pdf", college.collegeCode + "- Scm Request File - " + DateTime.Now.ToString("yyyMMddHHmmss") + ".pdf");
            }
            else
            {
                TempData["Error"] = "Please select any one checkbox for the print";
                return RedirectToAction("CollegeScmProceedingsRequest");
            }
            //}
            return RedirectToAction("CollegeScmProceedingsRequest");
        }

        public string SaveFacultyDataPdfatcollege(int preview, List<jntuh_scmproceedingsrequests> scmProceedings)
        {
            string fullPath = string.Empty;
            //Set page size as A4
            var userId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            var userCollegeId = db.jntuh_college_users.Where(u => u.userID == userId).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeId == 375)
            {
                userCollegeId = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            Document pdfDoc = new Document(PageSize.A4, 60, 50, 60, 60);
            string path = Server.MapPath("~/Content/PDFReports/SCMRequestDownload");
            if (!Directory.Exists(Server.MapPath("~/Content/PDFReports/SCMRequestDownload")))
            {
                Directory.CreateDirectory(Server.MapPath("~/Content/PDFReports/SCMRequestDownload"));
            }
            var college = db.jntuh_college.Where(r => r.id == userCollegeId && r.isActive == true).Select(s => s).FirstOrDefault();
            if (preview == 0 && college != null)
            {
                fullPath = path + "/" + college.collegeCode + "- SCM Request Download -" + DateTime.Now.ToString("yyyMMddHHmmss") + ".pdf";//
                PdfWriter pdfWriter = PdfWriter.GetInstance(pdfDoc, new FileStream(fullPath, FileMode.Create));
                ITextEvents iTextEvents = new ITextEvents();
                //iTextEvents.CollegeCode = scmProceedings[0].CollegeCode;
                //iTextEvents.CollegeName = scmProceedings[0].CollegeName;

                iTextEvents.CollegeCode = college.collegeCode;
                iTextEvents.CollegeName = college.collegeName;
                iTextEvents.formType = "Scm Request Download";
                pdfWriter.PageEvent = iTextEvents;
            }
            //Open PDF Document to write data
            pdfDoc.Open();

            //Assign Html content in a string to write in PDF
            string contents = string.Empty;

            StreamReader sr;

            //Read file from server path
            sr = System.IO.File.OpenText(Server.MapPath("~/Content/SCMRequestDownloadnonPayment.html"));
            //store content in the variable
            contents = sr.ReadToEnd();
            sr.Close();

            contents = contents.Replace("##SERVERURL##", serverURL);

            //contents = PaymentBillDetails(userCollegeId, contents);
            //contents = barcodegenerator(userCollegeId, contents);
            //contents += "<br/><p style='text-align:center;font-size:12px'><b> STAFF SELECTION COMMITTEE REQUEST </b></p><br/>";
            //contents += "<table border='0'cellspacing='0' cellpadding='4' width='100%'>";
            //contents += "<tr><td style='text-align:left'><b>College Name : " + college.collegeName + "</b></td>";
            //contents += "<td style='text-align:right'><b>College Code : " + college.collegeCode + "</b></td></tr>";
            //contents += "</table>";
            contents = GetSCMRequestDataatCollege(scmProceedings, contents);
            //  contents = affiliationType(collegeId, contents);

            //Read string contents using stream reader and convert html to parsed conent
            var parsedHtmlElements = HTMLWorker.ParseToList(new StringReader(contents), null);
            //Get each array values from parsed elements and add to the PDF document
            bool pageRotated = false;
            int count = 0;
            foreach (var htmlElement in parsedHtmlElements)
            {
                count++;
                if (count == 100)
                {

                }
                if (htmlElement.Equals("<textarea>"))
                {
                    pdfDoc.NewPage();
                }

                if (htmlElement.Chunks.Count >= 3)
                {
                    if (htmlElement.Chunks.Count == 4)
                    {
                        pdfDoc.SetPageSize(iTextSharp.text.PageSize.A4.Rotate());
                        pdfDoc.SetMargins(60, 50, 60, 60);
                        pageRotated = true;
                    }
                    else
                    {
                        if (pageRotated == true)
                        {
                            pdfDoc.SetPageSize(iTextSharp.text.PageSize.A4);
                            pdfDoc.SetMargins(60, 50, 60, 60);
                            pageRotated = false;
                        }
                    }

                    pdfDoc.NewPage();

                }
                else
                {
                    pdfDoc.Add(htmlElement as IElement);
                }
            }

            //Close your PDF
            pdfDoc.Close();
            if (pdfDoc.IsOpen())
            {
                pdfDoc.Close();
            }

            string returnPath = string.Empty;
            returnPath = fullPath;
            return returnPath;
        }

        public string GetSCMRequestDataatCollege(List<jntuh_scmproceedingsrequests> scmProceedings, string contents)
        {
            string contentdata = string.Empty;
            DateTime scmstartdate = new DateTime(2019, 04, 29);
            int[] scmrequestIds = scmProceedings.Select(e => e.ID).ToArray();
            var jntuh_scmproceeding_add_faculty = db.jntuh_scmproceedingrequest_addfaculty.Where(e => scmrequestIds.Contains(e.ScmProceedingId)).Select(e => e.ScmProceedingId).Distinct().ToArray();
            var scmdetails = (from a in db.jntuh_scmproceedingsrequests
                              join b in db.jntuh_college on a.CollegeId equals b.id
                              join c in db.jntuh_specialization on a.SpecializationId equals c.id
                              join d in db.jntuh_department on a.DEpartmentId equals d.id
                              join e in db.jntuh_degree on a.DegreeId equals e.id
                              where scmrequestIds.Contains(a.ID) && a.CreatedOn > scmstartdate
                              select new
                              {

                                  CollegeCode = b.collegeCode,
                                  CollegeName = b.collegeName,
                                  SpecializationId = c.id,
                                  SpecializationName = c.specializationName,
                                  //DepartmentId = abcd.id,
                                  //DepartmentName = abcd.departmentName,
                                  DegreeId = e.id,
                                  DegreeName = e.degree,
                                  Department = d.departmentDescription,
                                  Professors = a.ProfessorsCount,
                                  AssociateProfessors = a.AssociateProfessorsCount,
                                  AssistantProfessors = a.AssistantProfessorsCount,
                                  RequiredProfessors = a.RequiredProfessor,
                                  RequiredAssociateProfessors = a.RequiredAssociateProfessor,
                                  RequiredAssistantProfessors = a.RequiredAssistantProfessor,
                                  RequestSubmittedDate = a.RequestSubmittedDate
                              }).ToList();

            contentdata += "<br/><p style='text-align:center;font-size:12px'><b> STAFF SELECTION COMMITTEE REQUEST </b></p><br/>";
            contentdata += "<table border='0'cellspacing='0' cellpadding='4' width='100%'>";
            contentdata += "<tr><td style='text-align:left'><b>College Name : " + scmdetails[0].CollegeName + "</b></td>";
            contentdata += "<td style='text-align:right'><b>College Code : " + scmdetails[0].CollegeCode + "</b></td></tr>";
            contentdata += "</table>";

            contentdata += "<br/><table border='1'cellspacing='0' cellpadding='4' width='100%'>";
            contentdata += "<tr><td style='text-align:left' width='5%'>S.No</td><td width='25%'>Department</td><td width='8%'>Available Prof</td><td width='10%'>Available Assoc.Prof</td><td width='10%'>Available Asst.Prof</td><td width='10%'>Required Prof</td><td width='10%'>Required  Assoc.Prof</td><td width='10%'>Required  Asst.Prof</td><td width='12%'>Req. Submitted Date</td></tr>";

            for (int i = 0; i < scmdetails.Count(); i++)
            {
                string Requestdate = string.Empty;
                if (scmdetails[i].RequestSubmittedDate != null)
                {
                    Requestdate = UAAAS.Models.Utilities.MMDDYY2DDMMYY(scmdetails[i].RequestSubmittedDate.ToString());
                }

                contentdata += "<tr>";
                contentdata += "<td width='5%'>" + (i + 1) + "</td>";
                //contentdata += "<td width='25%'>" + scmdetails[i].DegreeName + "-" + scmdetails[i].SpecializationName + "</td>";
                contentdata += "<td width='25%'>" + scmdetails[i].Department + "</td>";
                contentdata += "<td width='8%'>" + scmdetails[i].Professors + "</td>";
                contentdata += "<td width='10%'>" + scmdetails[i].AssociateProfessors + "</td>";
                contentdata += "<td width='10%'>" + scmdetails[i].AssistantProfessors + "</td>";
                contentdata += "<td width='10%'>" + scmdetails[i].RequiredProfessors + "</td>";
                contentdata += "<td width='10%'>" + scmdetails[i].RequiredAssociateProfessors + "</td>";
                contentdata += "<td width='10%'>" + scmdetails[i].RequiredAssistantProfessors + "</td>";
                contentdata += "<td width='12%'>" + Requestdate + "</td>";
                contentdata += "</tr>";
            }
            contentdata += "</table>";

            contents = contents.Replace("##SCMDOWLOAD##", contentdata);
            //******* Display the Added Faculty Details *********//

            string FacultyData = string.Empty;
            List<ScmProceedingsRequestAddReg> scmaddedfaculty = new List<ScmProceedingsRequestAddReg>();
            scmaddedfaculty = (from SPR in db.jntuh_scmproceedingsrequests
                               join SPRF in db.jntuh_scmproceedingrequest_addfaculty on SPR.ID equals SPRF.ScmProceedingId
                               join RF in db.jntuh_registered_faculty on SPRF.RegistrationNumber equals RF.RegistrationNumber
                               join D in db.jntuh_department on SPR.DEpartmentId equals D.id
                               join S in db.jntuh_specialization on SPR.SpecializationId equals S.id
                               join DG in db.jntuh_degree on SPR.DegreeId equals DG.id
                               where scmrequestIds.Contains(SPR.ID)
                               select new ScmProceedingsRequestAddReg
                               {
                                   Id = (int)SPRF.Id,
                                   SpecializationId = SPR.SpecializationId,
                                   DepartmentName = D.departmentDescription,
                                   SpecializationName = S.specializationName,//abcde.degree + "-" + abcd.specializationName,
                                   Regno = RF.RegistrationNumber,
                                   RegName = RF.FirstName + " " + RF.LastName,
                                   DegreeName = DG.degree,
                                   ScmId = SPRF.ScmProceedingId
                               }).ToList();


            var specializationIds = scmaddedfaculty.Select(e => e.ScmId).Distinct().ToArray();
            if (specializationIds.Count() > 0)
            {
                foreach (var speid in specializationIds)
                {
                    FacultyData += PrintingDepartmentwiseFacultyatCollege(scmaddedfaculty.Where(e => e.ScmId == speid).ToList());
                }
            }


            contents = contents.Replace("##FACULTYDATA##", FacultyData);
            //contents = contents.Replace("##Currentdate##",DateTime.Now.ToString());

            return contents;
        }

        private string PrintingDepartmentwiseFacultyatCollege(List<ScmProceedingsRequestAddReg> scmfacultylist)
        {
            int count = 1;
            string contentdata = string.Empty;
            if (scmfacultylist != null)
            {
                contentdata += "<strong><u>" + scmfacultylist[0].DepartmentName + "</u></strong> <br /> <br />";
                contentdata += "<table border='1' cellspacing='0' cellpadding='5' style='font-size: 9px;' width='100%'>";
                contentdata += "<tbody>";
                contentdata += "<tr>";
                contentdata += "<td width='8%'><p align='left'>SNo</p></td>";
                contentdata += "<td width='20%'><p align='left'>Registration No</p></td>";
                contentdata += "<td width='27%'><p align='left'>Name</p></td>";
                //contentdata += "<td width='15%'><p align='left'>Degree</p></td>";
                //contentdata += "<td width='30%'><p align='left'>Department</p></td>";
                contentdata += "</tr>";
                foreach (var item in scmfacultylist)
                {
                    contentdata += "<tr>";
                    contentdata += "<td width='8%'><p align='left'>" + count + "</p></td>";
                    contentdata += "<td width='20%'><p align='left'>" + item.Regno + "</p></td>";
                    contentdata += "<td width='27%'><p align='left'>" + item.RegName + "</p></td>";
                    //contentdata += "<td width='15%'><p align='left'>" + item.DegreeName + "</p></td>";
                    //contentdata += "<td width='30%'><p align='left'>" + item.DepartmentName + "</p></td>";
                    contentdata += "</tr>";
                    count++;
                }
                contentdata += "</tbody></table>";
                return contentdata;
            }

            return contentdata;
        }

        //----------------------------------------
        public ActionResult CollegeScmPrint(int paymentid)
        {
            if (paymentid != 0)
            {
                var userId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
                var userCollegeId = db.jntuh_college_users.Where(u => u.userID == userId).Select(u => u.collegeID).FirstOrDefault();
                if (userCollegeId == 375)
                {
                    userCollegeId = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
                }
                serverURL = Request.Url.Scheme + System.Uri.SchemeDelimiter + Request.Url.Host + (Request.Url.IsDefaultPort ? "" : ":" + Request.Url.Port);
                //var checkedscmlistdata = scmlist.ScmProceedingsRequestslist.Where(e => e.Checked == true).ToList();
                var checkedscmlistdata = db.jntuh_scmproceedingsrequests.Where(r => r.PaymentTypeId == paymentid && r.RequestSubmittedDate != null && r.CollegeId == userCollegeId).Select(s => s).ToList();

                if (checkedscmlistdata.Count() != 0)
                {
                    var pdfPath = string.Empty;
                    int preview = 0;
                    if (preview == 0)
                    {
                        pdfPath = SaveFacultyDataPdf(preview, checkedscmlistdata, paymentid);
                        pdfPath = pdfPath.Replace("/", "\\");
                    }
                    var college = db.jntuh_college.Where(r => r.id == userCollegeId && r.isActive == true).Select(s => s).FirstOrDefault();
                    return File(pdfPath, "application/pdf", college.collegeCode + "- Scm Request Payment - " + DateTime.Now.ToString("yyyMMddHHmmss") + ".pdf");

                }
                else
                {
                    TempData["Error"] = "Please select any one checkbox for the print";
                    return RedirectToAction("CollegeScmProceedingsRequest");
                }
            }
            return RedirectToAction("CollegeScmProceedingsRequest");
        }

        //public ActionResult CollegeScmPrint(ScmProceedingsRequest scmlist)
        //{
        //    if (scmlist.ScmProceedingsRequestslist.Count() != 0)
        //    {
        //        serverURL = Request.Url.Scheme + System.Uri.SchemeDelimiter + Request.Url.Host + (Request.Url.IsDefaultPort ? "" : ":" + Request.Url.Port);
        //        var checkedscmlistdata = scmlist.ScmProceedingsRequestslist.Where(e => e.Checked == true).ToList();
        //        if (checkedscmlistdata.Count() != 0)
        //        {
        //            var pdfPath = string.Empty;
        //            int preview = 0;
        //            if (preview == 0)
        //            {
        //                pdfPath = SaveFacultyDataPdf(preview, checkedscmlistdata);
        //                pdfPath = pdfPath.Replace("/", "\\");
        //            }
        //            return File(pdfPath, "application/pdf", scmlist.ScmProceedingsRequestslist[0].CollegeCode + "- Scm Request File - " + DateTime.Now.ToString("yyyMMddHHmmss") + ".pdf");

        //        }
        //        else
        //        {
        //            TempData["Error"] = "Please select any one checkbox for the print";
        //            return RedirectToAction("CollegeScmProceedingsRequest");
        //        }
        //    }
        //    return RedirectToAction("CollegeScmProceedingsRequest");
        //}

        public string SaveFacultyDataPdf(int preview, List<jntuh_scmproceedingsrequests> scmProceedings, int paymentid)
        {
            string fullPath = string.Empty;
            //Set page size as A4
            var userId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            var userCollegeId = db.jntuh_college_users.Where(u => u.userID == userId).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeId == 375)
            {
                userCollegeId = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            Document pdfDoc = new Document(PageSize.A4, 60, 50, 60, 60);
            string path = Server.MapPath("~/Content/PDFReports/SCMRequestDownload");
            if (!Directory.Exists(Server.MapPath("~/Content/PDFReports/SCMRequestDownload")))
            {
                Directory.CreateDirectory(Server.MapPath("~/Content/PDFReports/SCMRequestDownload"));
            }
            var college = db.jntuh_college.Where(r => r.id == userCollegeId && r.isActive == true).Select(s => s).FirstOrDefault();
            if (preview == 0)
            {
                fullPath = path + "/" + college.collegeCode + "- SCM Request Payment Download -" + DateTime.Now.ToString("yyyMMddHHmmss") + ".pdf";//
                PdfWriter pdfWriter = PdfWriter.GetInstance(pdfDoc, new FileStream(fullPath, FileMode.Create));
                ITextEvents iTextEvents = new ITextEvents();
                //iTextEvents.CollegeCode = scmProceedings[0].CollegeCode;
                //iTextEvents.CollegeName = scmProceedings[0].CollegeName;

                iTextEvents.CollegeCode = college.collegeCode;
                iTextEvents.CollegeName = college.collegeName;
                iTextEvents.formType = "Scm Request Download";
                pdfWriter.PageEvent = iTextEvents;
            }
            //Open PDF Document to write data
            pdfDoc.Open();

            //Assign Html content in a string to write in PDF
            string contents = string.Empty;

            StreamReader sr;

            //Read file from server path
            sr = System.IO.File.OpenText(Server.MapPath("~/Content/SCMRequestDownload.html"));
            //store content in the variable
            contents = sr.ReadToEnd();
            sr.Close();

            contents = contents.Replace("##SERVERURL##", serverURL);

            contents = PaymentBillDetails(userCollegeId, contents, paymentid);
            //contents = barcodegenerator(userCollegeId, contents);
            contents = GetSCMRequestData(scmProceedings, contents, paymentid);
            //  contents = affiliationType(collegeId, contents);

            //Read string contents using stream reader and convert html to parsed conent
            var parsedHtmlElements = HTMLWorker.ParseToList(new StringReader(contents), null);
            //Get each array values from parsed elements and add to the PDF document
            bool pageRotated = false;
            int count = 0;
            foreach (var htmlElement in parsedHtmlElements)
            {
                count++;
                if (count == 100)
                {

                }
                if (htmlElement.Equals("<textarea>"))
                {
                    pdfDoc.NewPage();
                }

                if (htmlElement.Chunks.Count >= 3)
                {
                    if (htmlElement.Chunks.Count == 4)
                    {
                        pdfDoc.SetPageSize(iTextSharp.text.PageSize.A4.Rotate());
                        pdfDoc.SetMargins(60, 50, 60, 60);
                        pageRotated = true;
                    }
                    else
                    {
                        if (pageRotated == true)
                        {
                            pdfDoc.SetPageSize(iTextSharp.text.PageSize.A4);
                            pdfDoc.SetMargins(60, 50, 60, 60);
                            pageRotated = false;
                        }
                    }

                    pdfDoc.NewPage();

                }
                else
                {
                    pdfDoc.Add(htmlElement as IElement);
                }
            }

            //Close your PDF
            pdfDoc.Close();
            if (pdfDoc.IsOpen())
            {
                pdfDoc.Close();
            }

            string returnPath = string.Empty;
            returnPath = fullPath;
            return returnPath;
        }

        public string GetSCMRequestData(List<jntuh_scmproceedingsrequests> scmProceedings, string contents, int paymentid)
        {
            string contentdata = string.Empty;
            DateTime todayDate = DateTime.Now;
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int ay0 = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();
            int lid =
                db.jntuh_link_screens.Where(p => p.linkName == "Faculty SCM Request" && p.isActive == true).Select(s => s.id).FirstOrDefault();
            jntuh_college_links_assigned scmphase =
                db.jntuh_college_links_assigned.Where(l => l.linkId == lid && l.academicyearId == ay0 && l.isActive == true && l.fromdate <= todayDate && l.todate >= todayDate).Select(s => s).FirstOrDefault();
            int[] scmrequestIds = scmProceedings.Select(e => e.ID).ToArray();
            var jntuh_scmproceeding_add_faculty = db.jntuh_scmproceedingrequest_addfaculty.Where(e => scmrequestIds.Contains(e.ScmProceedingId)).Select(e => e.ScmProceedingId).Distinct().ToArray();
            var scmdetails = (from a in db.jntuh_scmproceedingsrequests
                              join b in db.jntuh_college on a.CollegeId equals b.id
                              join c in db.jntuh_specialization on a.SpecializationId equals c.id
                              join d in db.jntuh_department on a.DEpartmentId equals d.id
                              join e in db.jntuh_degree on a.DegreeId equals e.id
                              where scmrequestIds.Contains(a.ID) && a.CreatedOn > scmphase.fromdate && a.PaymentTypeId == paymentid
                              select new
                              {

                                  CollegeCode = b.collegeCode,
                                  CollegeName = b.collegeName,
                                  SpecializationId = c.id,
                                  SpecializationName = c.specializationName,
                                  //DepartmentId = abcd.id,
                                  //DepartmentName = abcd.departmentName,
                                  DegreeId = e.id,
                                  DegreeName = e.degree,
                                  Department = d.departmentDescription,
                                  Professors = a.ProfessorsCount,
                                  AssociateProfessors = a.AssociateProfessorsCount,
                                  AssistantProfessors = a.AssistantProfessorsCount,
                                  RequiredProfessors = a.RequiredProfessor,
                                  RequiredAssociateProfessors = a.RequiredAssociateProfessor,
                                  RequiredAssistantProfessors = a.RequiredAssistantProfessor,
                                  RequestSubmittedDate = a.RequestSubmittedDate
                              }).ToList();

            //contentdata += "<br/><p style='text-align:center;font-size:12px'><b> STAFF SELECTION COMMITTEE REQUEST </b></p><br/>";
            //contentdata += "<table border='0'cellspacing='0' cellpadding='4' width='100%'>";
            //contentdata += "<tr><td style='text-align:left'><b>College Name : " + scmdetails[0].CollegeName + "</b></td>";
            //contentdata += "<td style='text-align:right'><b>College Code : " + scmdetails[0].CollegeCode + "</b></td></tr>";
            //contentdata += "</table>";
            contentdata += "<br/><table border='1'cellspacing='0' cellpadding='4' width='100%'>";
            contentdata += "<tr><td style='text-align:left' width='5%'>S.No</td><td width='25%'>Department</td><td width='8%'>Available Prof</td><td width='10%'>Available Assoc.Prof</td><td width='10%'>Available Asst.Prof</td><td width='10%'>Required Prof</td><td width='10%'>Required  Assoc.Prof</td><td width='10%'>Required  Asst.Prof</td><td width='12%'>Req. Submitted Date</td></tr>";

            for (int i = 0; i < scmdetails.Count(); i++)
            {
                string Requestdate = string.Empty;
                if (scmdetails[i].RequestSubmittedDate != null)
                {
                    Requestdate = UAAAS.Models.Utilities.MMDDYY2DDMMYY(scmdetails[i].RequestSubmittedDate.ToString());
                }

                contentdata += "<tr>";
                contentdata += "<td width='5%'>" + (i + 1) + "</td>";
                //contentdata += "<td width='25%'>" + scmdetails[i].DegreeName + "-" + scmdetails[i].SpecializationName + "</td>";
                contentdata += "<td width='25%'>" + scmdetails[i].Department + "</td>";
                contentdata += "<td width='8%'>" + scmdetails[i].Professors + "</td>";
                contentdata += "<td width='10%'>" + scmdetails[i].AssociateProfessors + "</td>";
                contentdata += "<td width='10%'>" + scmdetails[i].AssistantProfessors + "</td>";
                contentdata += "<td width='10%'>" + scmdetails[i].RequiredProfessors + "</td>";
                contentdata += "<td width='10%'>" + scmdetails[i].RequiredAssociateProfessors + "</td>";
                contentdata += "<td width='10%'>" + scmdetails[i].RequiredAssistantProfessors + "</td>";
                contentdata += "<td width='12%'>" + Requestdate + "</td>";
                contentdata += "</tr>";
            }
            contentdata += "</table>";

            contents = contents.Replace("##SCMDOWLOAD##", contentdata);
            //******* Display the Added Faculty Details *********//

            string FacultyData = string.Empty;
            List<ScmProceedingsRequestAddReg> scmaddedfaculty = new List<ScmProceedingsRequestAddReg>();
            scmaddedfaculty = (from SPR in db.jntuh_scmproceedingsrequests
                               join SPRF in db.jntuh_scmproceedingrequest_addfaculty on SPR.ID equals SPRF.ScmProceedingId
                               join RF in db.jntuh_registered_faculty on SPRF.RegistrationNumber equals RF.RegistrationNumber
                               join D in db.jntuh_department on SPR.DEpartmentId equals D.id
                               join S in db.jntuh_specialization on SPR.SpecializationId equals S.id
                               join DG in db.jntuh_degree on SPR.DegreeId equals DG.id
                               where scmrequestIds.Contains(SPR.ID)
                               select new ScmProceedingsRequestAddReg
                               {
                                   Id = (int)SPRF.Id,
                                   SpecializationId = SPR.SpecializationId,
                                   DepartmentName = D.departmentDescription,
                                   SpecializationName = S.specializationName,//abcde.degree + "-" + abcd.specializationName,
                                   Regno = RF.RegistrationNumber,
                                   RegName = RF.FirstName + " " + RF.LastName,
                                   DegreeName = DG.degree,
                                   ScmId = SPRF.ScmProceedingId
                               }).ToList();


            var specializationIds = scmaddedfaculty.Select(e => e.ScmId).Distinct().ToArray();
            if (specializationIds.Count() > 0)
            {
                foreach (var speid in specializationIds)
                {
                    FacultyData += PrintingDepartmentwiseFaculty(scmaddedfaculty.Where(e => e.ScmId == speid).ToList());
                }
            }


            contents = contents.Replace("##FACULTYDATA##", FacultyData);
            //contents = contents.Replace("##Currentdate##",DateTime.Now.ToString());

            return contents;
        }

        private string PrintingDepartmentwiseFaculty(List<ScmProceedingsRequestAddReg> scmfacultylist)
        {
            int count = 1;
            string contentdata = string.Empty;
            if (scmfacultylist != null)
            {
                contentdata += "<strong><u>" + scmfacultylist[0].DepartmentName + "</u></strong> <br /> <br />";
                contentdata += "<table border='1' cellspacing='0' cellpadding='5' style='font-size: 9px;' width='100%'>";
                contentdata += "<tbody>";
                contentdata += "<tr>";
                contentdata += "<td width='8%'><p align='left'>SNo</p></td>";
                contentdata += "<td width='20%'><p align='left'>Registration No</p></td>";
                contentdata += "<td width='27%'><p align='left'>Name</p></td>";
                //contentdata += "<td width='15%'><p align='left'>Degree</p></td>";
                //contentdata += "<td width='30%'><p align='left'>Department</p></td>";
                contentdata += "</tr>";
                foreach (var item in scmfacultylist)
                {
                    contentdata += "<tr>";
                    contentdata += "<td width='8%'><p align='left'>" + count + "</p></td>";
                    contentdata += "<td width='20%'><p align='left'>" + item.Regno + "</p></td>";
                    contentdata += "<td width='27%'><p align='left'>" + item.RegName + "</p></td>";
                    //contentdata += "<td width='15%'><p align='left'>" + item.DegreeName + "</p></td>";
                    //contentdata += "<td width='30%'><p align='left'>" + item.DepartmentName + "</p></td>";
                    contentdata += "</tr>";
                    count++;
                }
                contentdata += "</tbody></table>";
                return contentdata;
            }

            return contentdata;
        }

        #region Principal SCM Print After Submission and Payment
        public ActionResult CollegePrincipalScmPrint(int paymentid)
        {
            if (paymentid != 0)
            {
                var userId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
                var userCollegeId = db.jntuh_college_users.Where(u => u.userID == userId).Select(u => u.collegeID).FirstOrDefault();
                if (userCollegeId == 375)
                {
                    userCollegeId = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
                }
                serverURL = Request.Url.Scheme + System.Uri.SchemeDelimiter + Request.Url.Host + (Request.Url.IsDefaultPort ? "" : ":" + Request.Url.Port);
                //var checkedscmlistdata = scmlist.ScmProceedingsRequestslist.Where(e => e.Checked == true).ToList();
                var checkedscmlistdata = db.jntuh_scmproceedingsrequests.Where(r => r.PaymentTypeId == paymentid && r.RequestSubmittedDate != null && r.CollegeId == userCollegeId && r.DEpartmentId == 0).Select(s => s).ToList();

                if (checkedscmlistdata.Count() != 0)
                {
                    var pdfPath = string.Empty;
                    int preview = 0;
                    if (preview == 0)
                    {
                        pdfPath = SavePrincipalDataPdf(preview, checkedscmlistdata, paymentid);
                        pdfPath = pdfPath.Replace("/", "\\");
                    }
                    var college = db.jntuh_college.Where(r => r.id == userCollegeId && r.isActive == true).Select(s => s).FirstOrDefault();
                    return File(pdfPath, "application/pdf", college.collegeCode + "- Scm Request Payment - " + DateTime.Now.ToString("yyyMMddHHmmss") + ".pdf");

                }
                else
                {
                    TempData["Error"] = "Please try again.";
                    return RedirectToAction("CollegeScmProceedingsPrincipalRequest");
                }
            }
            return RedirectToAction("CollegeScmProceedingsPrincipalRequest");
        }

        public string SavePrincipalDataPdf(int preview, List<jntuh_scmproceedingsrequests> scmProceedings, int paymentid)
        {
            string fullPath = string.Empty;
            //Set page size as A4
            var userId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            var userCollegeId = db.jntuh_college_users.Where(u => u.userID == userId).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeId == 375)
            {
                userCollegeId = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            Document pdfDoc = new Document(PageSize.A4, 60, 50, 60, 60);
            string path = Server.MapPath("~/Content/PDFReports/SCMRequestDownload");
            if (!Directory.Exists(Server.MapPath("~/Content/PDFReports/SCMRequestDownload")))
            {
                Directory.CreateDirectory(Server.MapPath("~/Content/PDFReports/SCMRequestDownload"));
            }
            var college = db.jntuh_college.Where(r => r.id == userCollegeId && r.isActive == true).Select(s => s).FirstOrDefault();
            if (preview == 0)
            {
                fullPath = path + "/" + college.collegeCode + "- SCM Principal Payment -" + DateTime.Now.ToString("yyyMMddHHmmss") + ".pdf";//
                PdfWriter pdfWriter = PdfWriter.GetInstance(pdfDoc, new FileStream(fullPath, FileMode.Create));
                ITextEvents iTextEvents = new ITextEvents();
                //iTextEvents.CollegeCode = scmProceedings[0].CollegeCode;
                //iTextEvents.CollegeName = scmProceedings[0].CollegeName;

                iTextEvents.CollegeCode = college.collegeCode;
                iTextEvents.CollegeName = college.collegeName;
                iTextEvents.formType = "Scm Request Download";
                pdfWriter.PageEvent = iTextEvents;
            }
            //Open PDF Document to write data
            pdfDoc.Open();

            //Assign Html content in a string to write in PDF
            string contents = string.Empty;

            StreamReader sr;

            //Read file from server path
            sr = System.IO.File.OpenText(Server.MapPath("~/Content/SCMPrincipalRequestPaymet.html"));
            //store content in the variable
            contents = sr.ReadToEnd();
            sr.Close();

            contents = contents.Replace("##SERVERURL##", serverURL);

            contents = PaymentBillDetails(userCollegeId, contents, paymentid);
            //contents = barcodegenerator(userCollegeId, contents);
            contents = GetPrincipalSCMRequestData(scmProceedings, contents);
            //  contents = affiliationType(collegeId, contents);

            //Read string contents using stream reader and convert html to parsed conent
            var parsedHtmlElements = HTMLWorker.ParseToList(new StringReader(contents), null);
            //Get each array values from parsed elements and add to the PDF document
            bool pageRotated = false;
            int count = 0;
            foreach (var htmlElement in parsedHtmlElements)
            {
                count++;
                if (count == 100)
                {

                }
                if (htmlElement.Equals("<textarea>"))
                {
                    pdfDoc.NewPage();
                }

                if (htmlElement.Chunks.Count >= 3)
                {
                    if (htmlElement.Chunks.Count == 4)
                    {
                        pdfDoc.SetPageSize(iTextSharp.text.PageSize.A4.Rotate());
                        pdfDoc.SetMargins(60, 50, 60, 60);
                        pageRotated = true;
                    }
                    else
                    {
                        if (pageRotated == true)
                        {
                            pdfDoc.SetPageSize(iTextSharp.text.PageSize.A4);
                            pdfDoc.SetMargins(60, 50, 60, 60);
                            pageRotated = false;
                        }
                    }

                    pdfDoc.NewPage();

                }
                else
                {
                    pdfDoc.Add(htmlElement as IElement);
                }
            }

            //Close your PDF
            pdfDoc.Close();
            if (pdfDoc.IsOpen())
            {
                pdfDoc.Close();
            }

            string returnPath = string.Empty;
            returnPath = fullPath;
            return returnPath;
        }

        public string GetPrincipalSCMRequestData(List<jntuh_scmproceedingsrequests> scmProceedings, string contents)
        {
            string contentdata = string.Empty;
            DateTime todayDate = DateTime.Now;
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int ay0 = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();
            int lid =
                db.jntuh_link_screens.Where(p => p.linkName == "Principal SCM Request" && p.isActive == true).Select(s => s.id).FirstOrDefault();
            jntuh_college_links_assigned scmphase =
                db.jntuh_college_links_assigned.Where(l => l.linkId == lid && l.academicyearId == ay0 && l.isActive == true && l.fromdate <= todayDate && l.todate >= todayDate).Select(s => s).FirstOrDefault();
            int count = 1;
            int[] scmrequestIds = scmProceedings.Select(e => e.ID).ToArray();
            var jntuh_scmproceeding_add_faculty = db.jntuh_scmproceedingrequest_addfaculty.Where(e => scmrequestIds.Contains(e.ScmProceedingId)).Select(e => e.ScmProceedingId).Distinct().ToArray();
            var scmdetails = (from a in db.jntuh_scmproceedingsrequests
                              join b in db.jntuh_college on a.CollegeId equals b.id
                              join ad in db.jntuh_scmproceedingrequest_addfaculty on a.ID equals ad.ScmProceedingId
                              join reg in db.jntuh_registered_faculty on ad.RegistrationNumber equals reg.RegistrationNumber
                              where scmrequestIds.Contains(a.ID) && a.CreatedOn > scmphase.fromdate
                              select new
                              {

                                  CollegeCode = b.collegeCode,
                                  CollegeName = b.collegeName,
                                  RequestSubmittedDate = a.RequestSubmittedDate,
                                  RegistrationNumber = ad.RegistrationNumber,
                                  Firstname = reg.FirstName,
                                  Middlename = reg.MiddleName,
                                  Lastname = reg.LastName

                              }).ToList();
            contentdata += "<br/><p align='left'><strong><u>Principal Details</u></strong></p><br />";
            contentdata += "<table border='1' cellspacing='0' cellpadding='5' style='font-size: 9px;' width='100%'>";
            contentdata += "<tbody>";
            contentdata += "<tr>";
            contentdata += "<td width='8%'><p align='left'>SNo</p></td>";
            contentdata += "<td width='20%'><p align='left'>Registration No</p></td>";
            contentdata += "<td width='27%'><p align='left'>Name</p></td>";
            //contentdata += "<td width='15%'><p align='left'>Degree</p></td>";
            //contentdata += "<td width='30%'><p align='left'>Department</p></td>";
            contentdata += "</tr>";
            foreach (var item in scmdetails)
            {

                contentdata += "<tr>";
                contentdata += "<td width='8%'><p align='left'>" + count + "</p></td>";
                contentdata += "<td width='20%'><p align='left'>" + item.RegistrationNumber + "</p></td>";
                contentdata += "<td width='27%'><p align='left'>" + item.Firstname + " " + item.Middlename + " " + item.Lastname + "</p></td>";

                contentdata += "</tr>";
                count++;
            }
            contentdata += "</tbody></table>";

            contents = contents.Replace("##SCMDOWLOAD##", contentdata);
            return contents;
        }
        #endregion

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public ActionResult ScmFacultyVerfication(int? collegeId)
        {
            var collegeIds = db.jntuh_scmproceedingsrequests.Where(e => e.ISActive == true).Select(e => e.CollegeId).Distinct().ToArray();
            ViewBag.Colleges = db.jntuh_college.Where(e => e.isActive == true && collegeIds.Contains(e.id)).Select(e => new { collegeId = e.id, collegeName = e.collegeCode + "-" + e.collegeName }).OrderBy(e => e.collegeName).ToList();
            var jntuh_department = db.jntuh_department.ToList();
            List<FacultyRegistrationDetails> teachingFaculty = new List<FacultyRegistrationDetails>();

            if (collegeId != null)
            {
                List<jntuh_college_faculty_registered> jntuh_college_faculty_registered = db.jntuh_college_faculty_registered.Where(cf => cf.collegeId == collegeId).Select(cf => cf).ToList();
                string[] strRegNoS = (from a in db.jntuh_scmproceedingsrequests
                                      join b in db.jntuh_scmproceedingrequest_addfaculty on a.ID equals b.ScmProceedingId
                                      where a.CollegeId == collegeId
                                      select b.RegistrationNumber).Distinct().ToArray();
                string[] PrincipalstrRegNoS = db.jntuh_college_principal_registered.Where(P => P.collegeId == collegeId).Select(P => P.RegistrationNumber.Trim()).ToArray();
                List<jntuh_registered_faculty> jntuh_registered_faculty = new List<jntuh_registered_faculty>();
                jntuh_registered_faculty = db.jntuh_registered_faculty
                                             .Where(rf => strRegNoS.Contains(rf.RegistrationNumber) && rf.Notin116 != true)  //&& (rf.collegeId == null || rf.collegeId == collegeid)
                                             .ToList();

                var jntuhScmproceedingrequestAddfaculty = db.jntuh_scmproceedingrequest_addfaculty.Where(e => strRegNoS.Contains(e.RegistrationNumber.Trim())).Select(e => e).ToList();


                var Specializations = db.jntuh_specialization.ToList();
                string RegNumber = "";
                int? Specializationid = 0;
                foreach (var a in jntuh_registered_faculty)
                {
                    string Reason = String.Empty;
                    Specializationid = jntuh_college_faculty_registered.Where(C => C.RegistrationNumber.Trim() == a.RegistrationNumber.Trim()).Select(C => C.SpecializationId).FirstOrDefault();
                    var faculty = new FacultyRegistrationDetails();
                    faculty.id = a.id;
                    faculty.Type = a.type;
                    faculty.CollegeId = collegeId;
                    faculty.RegistrationNumber = a.RegistrationNumber;
                    faculty.UniqueID = a.UniqueID;
                    faculty.FirstName = a.FirstName;
                    faculty.MiddleName = a.MiddleName;
                    faculty.LastName = a.LastName;
                    if (PrincipalstrRegNoS.Contains(a.RegistrationNumber.Trim()))
                        faculty.Principal = "Principal";
                    else
                        faculty.Principal = "";
                    faculty.GenderId = a.GenderId;
                    faculty.Email = a.Email;
                    faculty.facultyPhoto = a.Photo;
                    faculty.Mobile = a.Mobile;
                    faculty.PANNumber = a.PANNumber;
                    faculty.AadhaarNumber = a.AadhaarNumber;
                    faculty.isActive = a.isActive;
                    faculty.FacultyAddId = jntuhScmproceedingrequestAddfaculty.Where(e => e.RegistrationNumber.Trim() == a.RegistrationNumber.Trim()).Select(e => e.Id).FirstOrDefault();
                    faculty.isApproved = jntuhScmproceedingrequestAddfaculty.Where(e => e.RegistrationNumber.Trim() == a.RegistrationNumber.Trim()).Select(e => e.IsApproved).FirstOrDefault();
                    faculty.department = jntuh_department.Where(d => d.id == a.DepartmentId).Select(d => d.departmentName).FirstOrDefault();
                    faculty.SamePANNumberCount = jntuh_registered_faculty.Where(f => f.PANNumber == a.PANNumber).ToList().Count;
                    faculty.SameAadhaarNumberCount = jntuh_registered_faculty.Where(f => f.AadhaarNumber == a.AadhaarNumber).ToList().Count;
                    faculty.SpecializationIdentfiedFor = Specializationid > 0 ? Specializations.Where(S => S.id == Specializationid).Select(S => S.specializationName).FirstOrDefault() : "";
                    faculty.IdentfiedFor = jntuh_college_faculty_registered.Where(f => f.RegistrationNumber.Trim() == a.RegistrationNumber.Trim()).Select(f => f.IdentifiedFor).FirstOrDefault();
                    faculty.jntuh_registered_faculty_education = a.jntuh_registered_faculty_education;
                    teachingFaculty.Add(faculty);
                }
                teachingFaculty = teachingFaculty.Where(m => m.isActive == true).OrderBy(f => f.department).ToList();

                return View(teachingFaculty);
            }

            return View(teachingFaculty);
        }

        [HttpGet]
        [Authorize(Roles = "College")]
        public ActionResult AddRegistrationNumber(int id)
        {
            DateTime todayDate = DateTime.Now;
            var userId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            var userCollegeId = db.jntuh_college_users.Where(u => u.userID == userId).Select(u => u.collegeID).FirstOrDefault();
            int status = db.jntuh_college_edit_status.Where(editStatus => editStatus.collegeId == userCollegeId &&
                                                                                     editStatus.IsCollegeEditable == true &&
                                                                                     editStatus.editFromDate <= todayDate &&
                                                                                     editStatus.editToDate >= todayDate)
                                                                .Select(editStatus => editStatus.id)
                                                                .FirstOrDefault();

            if (userCollegeId == 375)
            {
                userCollegeId = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int ay0 = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();
            int lid =
                db.jntuh_link_screens.Where(p => p.linkName == "Faculty SCM Request" && p.isActive == true).Select(s => s.id).FirstOrDefault();
            jntuh_college_links_assigned scmphase =
                db.jntuh_college_links_assigned.Where(l => l.linkId == lid && l.academicyearId == ay0 && l.isActive == true && l.fromdate <= todayDate && l.todate >= todayDate).Select(s => s).FirstOrDefault();
            if (scmphase == null && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("College", "Dashboard");
            }

            ScmProceedingsRequestAddReg scmDetails = new ScmProceedingsRequestAddReg();
            scmDetails = (from a in db.jntuh_scmproceedingsrequests
                          join b in db.jntuh_college on a.CollegeId equals b.id into abdata
                          from ab in abdata.DefaultIfEmpty()
                          join c in db.jntuh_specialization on a.SpecializationId equals c.id into abcdata
                          from abc in abcdata.DefaultIfEmpty()
                          join d in db.jntuh_department on a.DEpartmentId equals d.id into abcddata
                          from abcd in abcdata.DefaultIfEmpty()
                          join e in db.jntuh_degree on a.DegreeId equals e.id into abcdedata
                          from abcde in abcdedata.DefaultIfEmpty()
                          where a.ID == id && scmphase.fromdate < a.CreatedOn
                          select new ScmProceedingsRequestAddReg
                           {
                               CollegeCode = ab.collegeCode,
                               CollegeName = ab.collegeName,

                               SpecializationId = abc.id,
                               SpecializationName = abc.specializationName,
                               DepartmentId = abc.departmentId,
                               DepartmentName = abc.jntuh_department.departmentDescription,
                               DegreeId = abcde.id,
                               DegreeName = abcde.degree,
                               Professors = (int)a.ProfessorsCount,
                               AssociateProfessors = (int)a.AssociateProfessorsCount,
                               AssistantProfessors = (int)a.AssistantProfessorsCount,
                               Id = a.ID,
                               FacultyId = 3
                           }).FirstOrDefault();

            //  var jntuh_college = db.jntuh_college.Where(e => e.isActive == true).Select(e => e).ToList();

            // jntuh_college.Add(new jntuh_college() { id = 0, collegeCode = "Not Working" });

            //  ViewBag.Colleges = jntuh_college.Select(e => new { collegeId = e.id, collegeName = e.collegeCode + "-" + e.collegeName }).OrderBy(e => e.collegeId).ToList();

            //var data = jntuh_college1.Select(e => new SelectListItem() { Value = e.id.ToString(), Text = e.collegeCode + "-" + e.collegeName }).ToList();
            //data.Insert(00, new SelectListItem() { Value = "00", Text = "Not Working", Selected = true });
            //data.Insert(0, new SelectListItem() {  Text = "---Select---",Selected = false});
            //ViewBag.Collegess = data;

            //ViewBag.Designations = db.jntuh_designation.Where(e => e.isActive == true && (e.id == 3)).Select(e => new { Id = e.id, Name = e.designation }).OrderBy(e => e.Id).ToList();


            return PartialView("_AddRegistrationNumber", scmDetails);
        }

        [HttpPost]
        [Authorize(Roles = "College")]
        public ActionResult AddRegistrationNumber(ScmProceedingsRequestAddReg reg)
        {
            TempData["Error"] = null;
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(collegeUser => collegeUser.userID == userID).Select(collegeUser => collegeUser.collegeID).FirstOrDefault();
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }

            DateTime todayDate = DateTime.Now;
            int status = db.jntuh_college_edit_status.Where(editStatus => editStatus.collegeId == userCollegeID &&
                                                                                     editStatus.IsCollegeEditable == true &&
                                                                                     editStatus.editFromDate <= todayDate &&
                                                                                     editStatus.editToDate >= todayDate)
                                                                .Select(editStatus => editStatus.id)
                                                                .FirstOrDefault();

            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int ay0 = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();
            int lid =
                db.jntuh_link_screens.Where(p => p.linkName == "Faculty SCM Request" && p.isActive == true).Select(s => s.id).FirstOrDefault();
            jntuh_college_links_assigned scmphase =
                db.jntuh_college_links_assigned.Where(l => l.linkId == lid && l.academicyearId == ay0 && l.isActive == true && l.fromdate <= todayDate && l.todate >= todayDate).Select(s => s).FirstOrDefault();
            if (scmphase == null && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("College", "Dashboard");
            }

            if (ModelState.IsValid)
            {
                if (reg != null)
                {
                    //DateTime SCMStartDate = new DateTime(2018, 08,10);
                    //DateTime SCMStartDate =scmphase.fromdate;
                    //This Code Writen By Narayana removing Duplicateswithsame Designation
                    //int deptId =
                    //    db.jntuh_scmproceedingsrequests.Where(s => s.ID == reg.ScmId)
                    //        .Select(e => e.DEpartmentId)
                    //        .FirstOrDefault();
                    jntuh_registered_faculty jntuh_registered_faculty =
                        db.jntuh_registered_faculty.Where(rf => rf.RegistrationNumber.Trim() == reg.RegistrationNo.Trim())
                            .Select(s => s)
                            .FirstOrDefault();
                    if (jntuh_registered_faculty == null)
                    {
                        TempData["Error"] = "This Registration Number doesn't Exist.";
                        return RedirectToAction("CollegeScmProceedingsRequest", "CollegeSCMProceedingsRequest");
                    }
                    if (jntuh_registered_faculty.Blacklistfaculy == true || jntuh_registered_faculty.AbsentforVerification == true)
                    {
                        TempData["Error"] = "Faculty Registration Number is in Blacklist.";
                        return RedirectToAction("CollegeScmProceedingsRequest", "CollegeSCMProceedingsRequest");
                    }
                    int[] scmids =
                       db.jntuh_scmproceedingsrequests.Where(s => s.CollegeId == userCollegeID && scmphase.fromdate < s.CreatedOn && s.DEpartmentId == reg.DepartmentId)
                           .Select(e => e.ID)
                           .ToArray();
                    foreach (var ids in scmids)
                    {
                        int id =
                        db.jntuh_scmproceedingrequest_addfaculty.Where(
                            s => s.RegistrationNumber == reg.RegistrationNo.Trim() && s.FacultyType == reg.FacultyId && s.ScmProceedingId == ids)
                            .Select(s => s.Id)
                            .FirstOrDefault();
                        if (id != 0)
                        {
                            TempData["Error1"] = "Faculty Registration Number Exists.";
                            return RedirectToAction("CollegeScmProceedingsRequest", "CollegeSCMProceedingsRequest");
                        }
                    }

                    jntuh_scmproceedingrequest_addfaculty addfaculty = new jntuh_scmproceedingrequest_addfaculty();
                    const string facultyexperiancepath = "~/Content/Upload/SCMPROCEEDINGSREQUEST/FacultyExperianceDocuments";
                    if (reg.FacultyId == 1 || reg.FacultyId == 2)
                    {
                        if (reg.ExperianceDocument != null)
                        {
                            if (!Directory.Exists(Server.MapPath(facultyexperiancepath)))
                            {
                                Directory.CreateDirectory(Server.MapPath(facultyexperiancepath));
                            }

                            var ext = Path.GetExtension(reg.ExperianceDocument.FileName);
                            if (ext != null && ext.ToUpper().Equals(".PDF"))
                            {
                                var ExpfileName = db.jntuh_college.Where(c => c.id == userCollegeID).Select(c => c.id).FirstOrDefault() + "_" + "FacultyExperiance" + "_" + DateTime.Now.ToString("yyyMMddHHmmss");
                                reg.ExperianceDocument.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(facultyexperiancepath), ExpfileName, ext));
                                reg.ExperianceDocumentView = ExpfileName + ext;
                            }
                        }
                        else
                        {
                            TempData["Error1"] = "Faculty Experiance Document Requried.";
                            return RedirectToAction("CollegeScmProceedingsRequest", "CollegeSCMProceedingsRequest");
                        }

                    }
                    addfaculty.ScmProceedingId = reg.Id;
                    addfaculty.RegistrationNumber = reg.RegistrationNo.Trim();
                    addfaculty.academicyearId = ay0;
                    addfaculty.phaseId = scmphase.phaseId;
                    addfaculty.FacultyType = 3; // 3 - assistant professor , 1- professor , 2- assosiate professor
                    addfaculty.PreviousCollegeId = reg.PreviousCollegeId.ToString();
                    addfaculty.CollegeId = userCollegeID;
                    addfaculty.Createdby = userID;
                    addfaculty.CreatedOn = DateTime.Now;
                    addfaculty.Isactive = true;
                    addfaculty.FacultyExperianceDocument = reg.ExperianceDocumentView;
                    db.jntuh_scmproceedingrequest_addfaculty.Add(addfaculty);
                    db.SaveChanges();
                    TempData["Success"] = "Faculty Add Successfully";
                    return RedirectToAction("CollegeScmProceedingsRequest", "CollegeSCMProceedingsRequest");
                }
            }
            return RedirectToAction("CollegeScmProceedingsRequest", "CollegeSCMProceedingsRequest");
        }
        [HttpGet]
        public ActionResult SCMRequestSubmission(int id)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            if (ModelState.IsValid)
            {
                if (id != null)
                {
                    int facultyCount = db.jntuh_scmproceedingrequest_addfaculty.Where(e => e.ScmProceedingId == id).Select(e => e.Id).Count();
                    if (facultyCount >= 1)
                    {
                        var Requests = db.jntuh_scmproceedingsrequests.Where(S => S.ID == id && S.isscmatUniversity == false).Select(S => S).FirstOrDefault();
                        if (Requests != null)
                        {
                            Requests.RequestSubmittedDate = DateTime.Now;
                            db.Entry(Requests).State = EntityState.Modified;
                            db.SaveChanges();
                            TempData["Success1"] = "Submission Successfully.";
                        }
                        else
                        {
                            TempData["Success1"] = "Submission Failed due to SCMs at University.";
                        }
                    }
                    else
                    {
                        TempData["Error1"] = "Please add Faculty Registration Number(s).";
                    }

                }

            }
            return RedirectToAction("CollegeScmProceedingsRequest", "CollegeScmProceedingsRequest");
        }
        //[HttpPost]
        //public JsonResult CheckRegistrationNumber(string RegistrationNo)
        //{
        //    string CheckingReg = db.jntuh_registered_faculty.Where(f => f.RegistrationNumber.Trim() == RegistrationNo.Trim() && (f.Blacklistfaculy == false || f.Blacklistfaculy == null)).Select(f => f.RegistrationNumber).FirstOrDefault();
        //    if (!string.IsNullOrEmpty(CheckingReg))
        //    {
        //        if (CheckingReg.Trim() == RegistrationNo.Trim())
        //            return Json(true);
        //        else
        //            return Json("This Registration Number doesn't Exist", JsonRequestBehavior.AllowGet);
        //    }
        //    else
        //        return Json("This Registration Number doesn't Exist", JsonRequestBehavior.AllowGet);

        //}

        [HttpPost]
        public JsonResult CheckRegistrationNumber(string RegistrationNo)
        {
            jntuh_registered_faculty CheckingReg = db.jntuh_registered_faculty.Where(f => f.RegistrationNumber.Trim() == RegistrationNo.Trim()).Select(f => f).FirstOrDefault();
            if (CheckingReg != null)
            {
                if (CheckingReg.Blacklistfaculy == true || CheckingReg.AbsentforVerification == true)
                    return Json("This Registration Number is in Blacklist", JsonRequestBehavior.AllowGet);
                else
                    return Json(true);
            }
            else
                return Json("This Registration Number doesn't Exist", JsonRequestBehavior.AllowGet);

        }

        public ActionResult ViewFaculty(int scmid)
        {
            DateTime scmstartdate = new DateTime(2022, 06, 25);
            if (scmid != 0)
            {
                List<ScmProceedingsRequestAddReg> addFacultyDetails = new List<ScmProceedingsRequestAddReg>();

                addFacultyDetails = (from a in db.jntuh_scmproceedingsrequests
                                     join b in db.jntuh_scmproceedingrequest_addfaculty on a.ID equals b.ScmProceedingId
                                     join c in db.jntuh_registered_faculty on b.RegistrationNumber.Trim() equals c.RegistrationNumber.Trim()
                                     join d in db.jntuh_specialization on a.SpecializationId equals d.id
                                     join e in db.jntuh_degree on a.DegreeId equals e.id
                                     join f in db.jntuh_designation on b.FacultyType equals f.id
                                     join g in db.jntuh_department on d.departmentId equals g.id
                                     where b.ScmProceedingId == scmid && a.CreatedOn > scmstartdate
                                     select new ScmProceedingsRequestAddReg
                                     {
                                         Id = b.Id,
                                         SpecializationId = a.SpecializationId,
                                         SpecializationName = e.degree + "-" + d.specializationName,
                                         DepartmentName = g.departmentDescription,
                                         Regno = b.RegistrationNumber,
                                         RegName = c.FirstName + " " + c.LastName,
                                         ScmId = b.ScmProceedingId,
                                         FacultyId = c.id,
                                         Designation = f.designation,
                                         ExperianceDocumentView = b.FacultyExperianceDocument,
                                         RequestSubmissionDate = a.RequestSubmittedDate

                                     }).ToList();
                return View(addFacultyDetails);
            }
            return RedirectToAction("CollegeScmProceedingsRequest");
        }


        public ActionResult DeleteRegistrationNumber(int id, int scmId)
        {
            if (id != 0 && scmId != 0)
            {
                var faculydata = db.jntuh_scmproceedingrequest_addfaculty.Where(e => e.Id == id).Select(e => e).FirstOrDefault();
                if (faculydata != null)
                {
                    db.jntuh_scmproceedingrequest_addfaculty.Remove(faculydata);
                    db.SaveChanges();
                    TempData["Success"] = "Faculty Deleted Successfully";
                    return RedirectToAction("ViewFaculty", "CollegeSCMProceedingsRequest", new { scmid = scmId });
                }
            }
            return RedirectToAction("CollegeScmProceedingsRequest");
        }

        [Authorize(Roles = "Admin")]
        public ActionResult SCMRequestsList()
        {
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int ay0 = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();
            int lid =
                db.jntuh_link_screens.Where(p => p.linkCode == "FSCM" && p.isActive == true).Select(s => s.id).FirstOrDefault();
            jntuh_college_links_assigned scmphase =
                db.jntuh_college_links_assigned.Where(l => l.linkId == lid && l.academicyearId == ay0 && l.isActive == true).Select(s => s).FirstOrDefault();

            DateTime scmfacultystartdate = Convert.ToDateTime(scmphase.fromdate);
            if (User.Identity.IsAuthenticated)
            {
                var jntuh_scmproceedingsrequests =
                    db.jntuh_scmproceedingsrequests.Where(e => e.ISActive == true && e.CreatedOn > scmfacultystartdate).Select(e => e).ToList();
                var AddFacultyScmIds =
                    db.jntuh_scmproceedingrequest_addfaculty.Where(e => e.CreatedOn > scmfacultystartdate).Select(e => e.ScmProceedingId).Distinct().ToArray();


                var jntuh_auditor_assign =
                    db.jntuh_auditor_assigned.Where(e => e.IsActive == true && e.CreatedOn > scmfacultystartdate).Select(e => e).ToList();


                //SCM Ids with Requests
                var SCMRequests =
                    jntuh_scmproceedingsrequests.Where(
                        e => e.ISActive == true && e.CreatedOn > scmfacultystartdate && e.SpecializationId != 0 && e.DEpartmentId != 0 && e.DegreeId != 0)
                        .Select(e => e.ID)
                        .Distinct()
                        .ToArray();
                var AddFacultySCMRequests =
                    AddFacultyScmIds.Where(e => SCMRequests.Contains(e)).Select(e => e).Distinct().ToArray();

                var collegeIds =
                    jntuh_scmproceedingsrequests.Where(
                        e =>
                            e.ISActive == true && e.SpecializationId != 0 && e.DEpartmentId != 0 && e.DegreeId != 0 && e.RequestSubmittedDate != null
                           ).Select(e => e.CollegeId).Distinct().ToArray();


                List<jntuh_college> colleges =
                    db.jntuh_college.Where(e => collegeIds.Contains(e.id)).Select(e => e).Distinct().ToList();


                ScmRequestList scmdata = new ScmRequestList();
                scmdata.SCmRequestList = new List<ScmRequestList>();
                List<ScmRequestList> scmRequestLists = new List<ScmRequestList>();
                foreach (var item in colleges)
                {
                    bool Ismatch = false;
                    var scmrequestIds =
                        jntuh_scmproceedingsrequests.Where(
                            e =>
                                e.CollegeId == item.id && e.CreatedOn > scmfacultystartdate && e.SpecializationId != 0 && e.DEpartmentId != 0 &&
                                e.DegreeId != 0 && e.RequestSubmittedDate != null)
                            .Select(e => e.ID)
                            .Distinct()
                            .ToArray();
                    var scmrequestsaddfaculty =
                        AddFacultyScmIds.Where(e => scmrequestIds.Contains(e)).Select(e => e).Distinct().ToArray();
                    var scmrequestassigncount =
                        jntuh_auditor_assign.Where(e => scmrequestsaddfaculty.Contains(e.ScmId))
                            .Select(e => e.ScmId)
                            .Distinct()
                            .Count();
                    if (scmrequestassigncount == scmrequestsaddfaculty.Count() && scmrequestassigncount != 0 && scmrequestsaddfaculty.Count() != 0)
                    {
                        Ismatch = true;
                    }
                    scmRequestLists.Add(new ScmRequestList()
                    {
                        Id = item.id,
                        CollegeCode = item.collegeCode,
                        CollegeName = item.collegeName,
                        IsAuditorAssigned = Ismatch,
                        Checked = false
                    });
                }
                scmdata.SCmRequestList.AddRange(scmRequestLists.OrderBy(e => e.CollegeName).Select(e => e).ToList());
                return View(scmdata);
            }
            else
            {
                FormsAuthentication.SignOut();
                return RedirectToAction("Login", "Admin");
            }
        }

        [Authorize(Roles = "Admin")]
        public ActionResult CollegeScmRequestFacultysView(int id)
        {
            //return RedirectToAction("ComingSoon", "Labs");
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int ay0 = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();
            int lid =
                db.jntuh_link_screens.Where(p => p.linkCode == "FSCM" && p.isActive == true).Select(s => s.id).FirstOrDefault();
            jntuh_college_links_assigned scmphase =
                db.jntuh_college_links_assigned.Where(l => l.linkId == lid && l.academicyearId == ay0 && l.isActive == true).Select(s => s).FirstOrDefault();
            if (scmphase == null)
            {
                return RedirectToAction("SCMRequestsList");
            }
            DateTime scmstartdate = Convert.ToDateTime(scmphase.fromdate);
            if (User.Identity.IsAuthenticated)
            {
                if (id != 0)
                {
                    var firstOrDefault = db.jntuh_college.FirstOrDefault(a => a.id == id);
                    var specs = new List<DistinctSpecializations>();
                    var degrees = db.jntuh_degree.AsNoTracking().ToList();
                    var specializations = db.jntuh_specialization.AsNoTracking().ToList();
                    var departments = db.jntuh_department.AsNoTracking().ToList();
                    var collegespecs =
                        db.jntuh_college_intake_existing.Where(i => i.collegeId == id)
                            .Select(i => i.specializationId)
                            .Distinct()
                            .ToArray();
                    foreach (var s in collegespecs)
                    {
                        var specid = specializations.FirstOrDefault(i => i.id == s);

                        if (specid != null)
                        {
                            var deptment = departments.FirstOrDefault(i => i.id == specid.departmentId);
                            var degree = degrees.FirstOrDefault(i => i.id == (deptment != null ? deptment.degreeId : 0));
                            if (degree != null)
                                specs.Add(new DistinctSpecializations
                                {
                                    SpecializationId = specid.id,
                                    SpecializationName = degree.degree + " - " + specid.specializationName,
                                    DepartmentId = specid.departmentId
                                });
                        }
                    }
                    ViewBag.departments = specs.OrderBy(i => i.DepartmentId);
                    var collegescmrequestslist = db.jntuh_scmproceedingsrequests.AsNoTracking().Where(i => i.CollegeId == id && i.RequestSubmittedDate != null && i.CreatedOn > scmstartdate).ToList();

                    var nomineeAssignedScmIds = (from SCM in db.jntuh_scmproceedingsrequests
                                                 join AUDA in db.jntuh_auditor_assigned on SCM.ID equals AUDA.ScmId
                                                 where
                                                     SCM.CollegeId == id && SCM.CreatedOn > scmstartdate && SCM.SpecializationId != 0 && SCM.DEpartmentId != 0 &&
                                                     SCM.DegreeId != 0
                                                 select SCM.ID).Distinct().ToArray();


                    var jntuh_auditor_assigned = db.jntuh_auditor_assigned.AsNoTracking().ToList();
                    var jntuh_auditors_dataentry = db.jntuh_auditors_dataentry.AsNoTracking().ToList();
                    var proceedingsRequests = new List<ScmProceedingsRequest>();



                    var SplittedSCMIds = db.jntuh_scmproceedingsrequests.Where(e => e.ISActive == true && e.OldSCMId != null).Select(e => e.OldSCMId).ToArray();




                    var AddFacultySCMIds = db.jntuh_scmproceedingrequest_addfaculty.Where(e => e.FacultyType != 0 && e.FacultyType != 1 && e.FacultyType != 2 && e.CreatedOn > scmstartdate).Select(e => e.ScmProceedingId).Distinct().ToArray();
                    foreach (var s in collegescmrequestslist.Where(e => !nomineeAssignedScmIds.Contains(e.ID)).Select(e => e).ToList())
                    {
                        if (AddFacultySCMIds.Contains(s.ID))
                        {

                            bool Isauditor = false;
                            bool IsSplited = false;
                            bool localIsAuditorVerified = false;
                            var specid = specializations.FirstOrDefault(i => i.id == s.SpecializationId);

                            if (specid != null && firstOrDefault != null)
                            {
                                var deptment = departments.FirstOrDefault(i => i.id == specid.departmentId);
                                var degree = degrees.FirstOrDefault(i => i.id == (deptment != null ? deptment.degreeId : 0));
                                var AuditorAssigneddata = jntuh_auditor_assigned.Where(e => e.ScmId == s.ID).Select(e => e.Id).FirstOrDefault();
                                var Auditorverifieddata = jntuh_auditors_dataentry.Where(e => e.ScmProceedingId == s.ID).Select(e => e.Id).FirstOrDefault();
                                if (AuditorAssigneddata != 0)
                                {
                                    Isauditor = true;
                                }
                                if (Auditorverifieddata != 0)
                                {
                                    localIsAuditorVerified = true;
                                }
                                if (SplittedSCMIds.Contains(s.ID))
                                {
                                    IsSplited = true;
                                }

                                if (degree != null)
                                    proceedingsRequests.Add(new ScmProceedingsRequest
                                    {
                                        CollegeName = firstOrDefault.collegeCode + " - " + firstOrDefault.collegeName,
                                        CollegeId = firstOrDefault.id,
                                        SpecializationId = s.SpecializationId,
                                        ProfessorVacancies = s.ProfessorsCount.ToString(),
                                        AssociateProfessorVacancies = s.AssociateProfessorsCount.ToString(),
                                        AssistantProfessorVacancies = s.AssistantProfessorsCount.ToString(),
                                        SpecializationName = degree.degree + " - " + specid.specializationName,
                                        DepartmentId = specid.departmentId,
                                        DepartmentName = deptment.departmentDescription,
                                        CreatedDate = (DateTime)s.RequestSubmittedDate,
                                        ScmNotificationpath = s.SCMNotification,
                                        RequiredProfessorVacancies = s.RequiredProfessor.ToString(),
                                        RequiredAssociateProfessorVacancies = s.RequiredAssociateProfessor.ToString(),
                                        RequiredAssistantProfessorVacancies = s.RequiredAssistantProfessor.ToString(),
                                        Id = s.ID,
                                        IsSplited = IsSplited,
                                        IsAuditorAssigned = Isauditor,
                                        IsAuditorVerified = localIsAuditorVerified
                                    });
                            }
                        }
                    }
                    ViewBag.collegescmrequestslist = proceedingsRequests.OrderByDescending(e => e.CreatedDate).Select(e => e).ToList();
                    return View(proceedingsRequests.OrderBy(e => e.DepartmentName).Select(e => e).ToList());
                }
                else
                {
                    return RedirectToAction("SCMRequestsList");
                }
            }
            else
            {
                FormsAuthentication.SignOut();
                return RedirectToAction("Login", "Admin");
            }

        }

        //SCM Upload For Faculty
        [Authorize(Roles = "College")]
        public ActionResult SCMUpload()
        {
            var isedit = isFacultySCMupload();
            if (!isedit)
            {
                return RedirectToAction("College", "Dashboard");
            }
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int ay0 = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();
            var userId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            var userCollegeId = db.jntuh_college_users.Where(u => u.userID == userId).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeId == 375)
            {
                userCollegeId = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            var firstOrDefault = db.jntuh_college.FirstOrDefault(a => a.id == userCollegeId);
            var specs = new List<DistinctSpecializations>();
            var depts = new List<DistinctDepartments>();
            var degrees = db.jntuh_degree.AsNoTracking().ToList();
            var specializations = db.jntuh_specialization.AsNoTracking().ToList();
            var departments = db.jntuh_department.AsNoTracking().ToList();
            //int[] collegespecs = new int[];
            //DateTime scmuploadstartdate This means SCM UPload Table is changed
            DateTime scmuploadstartdate = new DateTime(2018, 02, 10);
            //   var humanitesSpeci1 = new[] { 37, 48, 42, 31, 154 };
            List<int> collegespecs = new List<int>();
            collegespecs.AddRange(db.jntuh_college_intake_existing.Where(i => i.collegeId == userCollegeId).Select(i => i.specializationId).Distinct().ToArray());

            int[] degreeIds = (from a in specializations
                               join b in departments on a.departmentId equals b.id
                               join c in degrees on b.degreeId equals c.id
                               where collegespecs.Contains(a.id)
                               select c.id).Distinct().ToArray();
            if (degreeIds.Contains(4))
            {
                var humanitesSpeci = new[] { 37, 48, 42, 31, 154 };
                collegespecs.AddRange(humanitesSpeci);
            }

            //Others Departments

            if (collegespecs.Contains(154))
            {
                var othersSpeci = new[] { 155, 156, 157, 158 };
                collegespecs.AddRange(othersSpeci);
            }

            collegespecs.Remove(154);

            int[] degreeids = { 4, 3, 5, 6, 7 };
            foreach (var s in collegespecs)
            {
                var specid = specializations.FirstOrDefault(i => i.id == s);

                if (specid != null)
                {
                    var deptment = departments.FirstOrDefault(i => i.id == specid.departmentId && degreeids.Contains(i.degreeId));
                    var degree = degrees.FirstOrDefault(i => i.id == (deptment != null ? deptment.degreeId : 0));
                    if (degree != null)
                        specs.Add(new DistinctSpecializations { SpecializationId = specid.id, SpecializationName = deptment.departmentDescription, DepartmentId = specid.departmentId });
                }
            }

            //specs.Add(new DistinctSpecializations { SpecializationId = specid.id, SpecializationName = degree.degree + " - " + specid.specializationName, DepartmentId = specid.departmentId });
            ViewBag.departments = specs.OrderBy(i => i.DepartmentId);

            List<jntuh_scmupload> jntuh_scmupload = db.jntuh_scmupload.Where(SCMUPL => SCMUPL.IsActive == true && SCMUPL.CreatedOn >= scmuploadstartdate && SCMUPL.DepartmentId != 0 && SCMUPL.DegreeId != 0).Select(s => s).ToList();
            List<Scmuploads> scmuploadeddata = new List<Scmuploads>();
            ViewBag.designation =
                db.jntuh_designation.Where(d => d.isActive == true && (d.id == 1 || d.id == 2 || d.id == 3)).Select(e => e).ToList();
            var collegefaculty =
                db.jntuh_college_faculty_registered.Where(c => c.collegeId == userCollegeId).Select(s => s).ToList();
            int nooffacultycount = collegefaculty.Count();
            int scmuploadcount = 0;
            foreach (var data in collegefaculty)
            {
                Scmuploads scmup = new Scmuploads();
                scmup.RegistrationNo = data.RegistrationNumber;

                if (data.SpecializationId != null)
                {
                    int SpecId = (int)data.SpecializationId;
                    scmup.SpecializationId = SpecId;
                    scmup.SpecializationName = specializations.Where(e => e.id == SpecId).Select(e => e.specializationName).FirstOrDefault();
                }
                if (data.DepartmentId != null)
                {
                    int deptId = (int)data.DepartmentId;
                    scmup.DepartmentId = deptId;
                    scmup.DegreeId = departments.Where(e => e.id == deptId).Select(e => e.degreeId).FirstOrDefault();
                    scmup.Degree = degrees.Where(d => d.id == scmup.DegreeId).Select(s => s.degree).FirstOrDefault();
                    scmup.Department = departments.Where(e => e.id == deptId).Select(e => e.departmentDescription).FirstOrDefault();
                    if (data.SpecializationId != null)
                    {
                        scmup.SpecializationId = (int)data.SpecializationId;
                    }
                }
                jntuh_scmupload scm =
                    jntuh_scmupload.Where(u => u.RegistrationNumber.Trim() == data.RegistrationNumber.Trim())
                        .Select(s => s)
                        .FirstOrDefault();
                if (scm != null)
                {
                    scmup.Id = scm.Id;
                    if (scm.CollegeId == data.collegeId)
                    {
                        scmup.DesignationName = scm.Designation;
                        scmup.AssistantProfessorDocumentView = scm.SCMDocument;
                        scmup.ScmDateView = scm.SCMdate;
                        scmuploadcount++;
                    }
                    else
                    {

                        //scmup.ScmDateView = null;
                    }
                }
                else
                {
                    scmup.DesignationName = null;
                    scmup.AssistantProfessorDocumentView = null;
                }

                scmuploadeddata.Add(scmup);
            }
            //List<Scmuploads> scmuploadeddata = (from TCF in db.jntuh_college_faculty_registered
            //                                    join SCMUPL in db.jntuh_scmupload on TCF.RegistrationNumber equals SCMUPL.RegistrationNumber
            //                                    join SPE in db.jntuh_specialization on SCMUPL.SpecializationId equals SPE.id
            //                                    join DEPT in db.jntuh_department on SCMUPL.DepartmentId equals DEPT.id
            //                                    join DEG in db.jntuh_degree on SCMUPL.DegreeId equals DEG.id
            //                                    where SCMUPL.IsActive == true && SCMUPL.CreatedOn >= scmuploadstartdate && SCMUPL.CollegeId == userCollegeId && SCMUPL.SpecializationId != 0 && SCMUPL.DepartmentId != 0 && SCMUPL.DegreeId != 0
            //                                    select new Scmuploads()
            //                                    {
            //                                        Id = SCMUPL.Id,
            //                                        CollegeId = SCMUPL.CollegeId,
            //                                        SpecializationId = SCMUPL.SpecializationId,
            //                                        DegreeId = SCMUPL.DegreeId,
            //                                        DepartmentId = SCMUPL.DepartmentId,
            //                                        SpecializationName = DEG.degree + "-" + SPE.specializationName,
            //                                        Department = DEPT.departmentDescription,
            //                                        Degree = DEG.degree,
            //                                        ScmDateView = SCMUPL.SCMdate,
            //                                        DesignationName = SCMUPL.Designation,
            //                                        RegistrationNo = TCF.RegistrationNumber,                                                   
            //                                        AssistantProfessorDocumentView = SCMUPL.SCMDocument
            //                                    }).ToList();

            ViewBag.SCMUPLOADEDDATA = scmuploadeddata;
            ViewBag.TotalCollegeFaculty = nooffacultycount;
            ViewBag.TotalCollegeFacultyscmupload = scmuploadcount;
            ViewBag.TotalCollegeFacultyscmnotupload = nooffacultycount - scmuploadcount;
            return View();
        }


        [Authorize(Roles = "College")]
        [HttpPost]
        public ActionResult SCMUpload(Scmuploads scmdata)
        {
            return RedirectToAction("College", "Dashboard");
            var isedit = isFacultySCMupload();
            if (!isedit)
            {
                return RedirectToAction("College", "Dashboard");
            }
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int ay0 = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();
            DateTime scmuploadstartdate = new DateTime(2018, 02, 10);
            var userId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            var userCollegeId = db.jntuh_college_users.Where(u => u.userID == userId).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeId == 375)
            {
                userCollegeId = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            var isRegistrationnumbervalid =
               db.jntuh_registered_faculty.Where(s => s.RegistrationNumber == scmdata.RegistrationNo.Trim())
                   .Select(r => r)
                   .FirstOrDefault();

            if (isRegistrationnumbervalid == null)
            {
                TempData["Error"] = "Invalid Registration Number";
                return RedirectToAction("SCMUpload");
            }
            if (isRegistrationnumbervalid.Blacklistfaculy == true || isRegistrationnumbervalid.FacultyVerificationStatus == true)
            {
                TempData["Error"] = "Blacklist Registration Number";
                return RedirectToAction("SCMUpload");
            }
            string Registrationnumber =
                db.jntuh_college_faculty_registered.Where(s => s.RegistrationNumber.Trim() == scmdata.RegistrationNo.Trim() && s.collegeId == userCollegeId)
                    .Select(r => r.RegistrationNumber)
                    .FirstOrDefault();
            if (String.IsNullOrEmpty(Registrationnumber))
            {
                string ARegistrationnumber =
                db.jntuh_college_faculty_registered.Where(s => s.RegistrationNumber.Trim() == scmdata.RegistrationNo.Trim() && s.collegeId == userCollegeId)
                    .Select(r => r.RegistrationNumber)
                    .FirstOrDefault();
                if (String.IsNullOrEmpty(ARegistrationnumber))
                {
                    TempData["Error"] = "First Add the Faculty Registration Number in College Portal";
                    return RedirectToAction("SCMUpload");
                }

            }
            int designationid = Convert.ToInt32(scmdata.DesignationId);
            var designation =
                db.jntuh_designation.Where(d => d.id == designationid).Select(s => s.designation).FirstOrDefault();
            int ScmUploadId =
                db.jntuh_scmupload.Where(
                    s => s.RegistrationNumber.Trim() == scmdata.RegistrationNo.Trim() && s.CreatedOn >= scmuploadstartdate && s.CollegeId == userCollegeId && s.Designation == designation && s.DegreeId != scmdata.DegreeId && s.DepartmentId != 0 && s.SpecializationId != 0)
                    .Select(s => s.Id)
                    .FirstOrDefault();
            if (ScmUploadId != 0)
            {
                TempData["Error"] = "SCM already Uploaded for this Registration Number";
                return RedirectToAction("SCMUpload");
            }
            if (ModelState.IsValid)
            {
                var jntuh_college = db.jntuh_college.Where(e => e.isActive == true).Select(e => e).ToList();
                var jntuh_specialization = db.jntuh_specialization.AsNoTracking().ToList();
                var jntuh_department = db.jntuh_department.AsNoTracking().ToList();

                const string scmnotificationpath = "~/Content/Upload/SCMUploads";
                var jntuh_scmupload = new jntuh_scmupload();

                #region Saving Pdf data if Upload SCM File

                //Professor's Document Saving
                //if (scmdata.ProfessorDocument != null)
                //{
                //    if (!Directory.Exists(Server.MapPath(scmnotificationpath)))
                //    {
                //        Directory.CreateDirectory(Server.MapPath(scmnotificationpath));
                //    }

                //    var ext = Path.GetExtension(scmdata.ProfessorDocument.FileName);
                //    if (ext != null && ext.ToUpper().Equals(".PDF"))
                //    {
                //        var professorScmFileName =
                //            jntuh_college.Where(c => c.id == userCollegeId).Select(c => c.collegeCode).FirstOrDefault() +
                //            "_" + "Professors" + "_" + DateTime.Now.ToString("yyyMMddHHmmss");
                //        scmdata.ProfessorDocument.SaveAs(string.Format("{0}/{1}{2}",
                //            Server.MapPath(scmnotificationpath), professorScmFileName, ext));
                //        scmdata.ProfessorDocumentView = professorScmFileName + ext;
                //    }
                //}

                ////Associate Professor's Document Saving
                //if (scmdata.AssociateProfessorDocument != null)
                //{
                //    if (!Directory.Exists(Server.MapPath(scmnotificationpath)))
                //    {
                //        Directory.CreateDirectory(Server.MapPath(scmnotificationpath));
                //    }

                //    var ext = Path.GetExtension(scmdata.AssociateProfessorDocument.FileName);
                //    if (ext != null && ext.ToUpper().Equals(".PDF"))
                //    {
                //        var associateprofessorScmFileName =
                //            jntuh_college.Where(c => c.id == userCollegeId).Select(c => c.collegeCode).FirstOrDefault() +
                //            "_" + "AssociateProfessors" + "_" + DateTime.Now.ToString("yyyMMddHHmmss");
                //        scmdata.AssociateProfessorDocument.SaveAs(string.Format("{0}/{1}{2}",
                //            Server.MapPath(scmnotificationpath), associateprofessorScmFileName, ext));
                //        scmdata.AssociateProfessorDocumentView = associateprofessorScmFileName + ext;
                //    }
                //}

                //Assistant Professor's Document Saving
                if (scmdata.AssistantProfessorDocument != null)
                {
                    if (!Directory.Exists(Server.MapPath(scmnotificationpath)))
                    {
                        Directory.CreateDirectory(Server.MapPath(scmnotificationpath));
                    }

                    var ext = Path.GetExtension(scmdata.AssistantProfessorDocument.FileName);
                    if (ext != null && ext.ToUpper().Equals(".PDF"))
                    {
                        var assistantprofessorScmFileName =
                            jntuh_college.Where(c => c.id == userCollegeId).Select(c => c.collegeCode).FirstOrDefault() +
                            "_" + scmdata.RegistrationNo.Trim() + DateTime.Now.ToString("yyyMMddHHmmss");
                        scmdata.AssistantProfessorDocument.SaveAs(string.Format("{0}/{1}{2}",
                            Server.MapPath(scmnotificationpath), assistantprofessorScmFileName, ext));
                        scmdata.AssistantProfessorDocumentView = assistantprofessorScmFileName + ext;
                    }
                }

                #endregion

                jntuh_scmupload.CollegeId = userCollegeId;
                jntuh_scmupload.SpecializationId = scmdata.SpecializationId;
                var specialization = jntuh_specialization.FirstOrDefault(i => i.id == scmdata.SpecializationId);
                var department = jntuh_department.FirstOrDefault(i => i.id == specialization.departmentId);
                jntuh_scmupload.DepartmentId = specialization != null ? specialization.departmentId : 0;
                jntuh_scmupload.DegreeId = department != null ? department.degreeId : 0;
                //ProfDocument is Considered as Registation Number from 03-02-2018
                //jntuh_scmupload.ProfDocument = scmdata.ProfessorDocumentView ?? "";
                jntuh_scmupload.RegistrationNumber = scmdata.RegistrationNo.Trim();
                //AssocDocument Is Considered as Faculty Designation Id
                //jntuh_scmupload.AssocDocument = scmdata.AssociateProfessorDocumentView ?? "";

                jntuh_scmupload.Designation = db.jntuh_designation.Where(d => d.id == designationid).Select(e => e.designation).FirstOrDefault().Trim();
                //jntuh_scmupload.AssistDocument is Considered as SCM Upload Document by college
                jntuh_scmupload.SCMDocument = scmdata.AssistantProfessorDocumentView ?? "";
                if (scmdata.ScmDate != null)
                    jntuh_scmupload.SCMdate = UAAAS.Models.Utilities.DDMMYY2MMDDYY(scmdata.ScmDate);
                jntuh_scmupload.IsActive = true;
                jntuh_scmupload.CreatedBy = userId;
                jntuh_scmupload.CreatedOn = DateTime.Now;
                db.jntuh_scmupload.Add(jntuh_scmupload);
                db.SaveChanges();
                TempData["Success"] = "SCM Uploaded Successfully......";
            }
            else
            {
                TempData["Error"] = "Enter Data Mandatory Fields";
            }
            return RedirectToAction("SCMUpload");
        }

        /// <summary>
        /// Faculty SCM Update
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Roles = "College")]
        public ActionResult FacultySCMEdit(int id, string registrationnumber)
        {
            var isedit = isFacultySCMupload();
            if (!isedit)
            {
                return RedirectToAction("College", "Dashboard");
            }
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int ay0 = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();
            DateTime scmuploadstartdate = new DateTime(2018, 02, 10);
            var userId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            var userCollegeId = db.jntuh_college_users.Where(u => u.userID == userId).Select(u => u.collegeID).FirstOrDefault();

            var firstOrDefault = db.jntuh_college.FirstOrDefault(a => a.id == userCollegeId);
            var specs = new List<DistinctSpecializations>();
            var depts = new List<DistinctDepartments>();
            var degrees = db.jntuh_degree.AsNoTracking().ToList();
            var specializations = db.jntuh_specialization.AsNoTracking().ToList();
            var departments = db.jntuh_department.AsNoTracking().ToList();
            List<int> collegespecs = new List<int>();
            collegespecs.AddRange(db.jntuh_college_intake_existing.Where(i => i.collegeId == userCollegeId).Select(i => i.specializationId).Distinct().ToArray());


            int[] degreeIds = (from a in specializations
                               join b in departments on a.departmentId equals b.id
                               join c in degrees on b.degreeId equals c.id
                               where collegespecs.Contains(a.id)
                               select c.id).Distinct().ToArray();
            if (degreeIds.Contains(4))
            {
                var humanitesSpeci = new[] { 37, 48, 42, 31, 154 };
                collegespecs.AddRange(humanitesSpeci);
            }

            //Others Departments

            if (collegespecs.Contains(154))
            {
                var othersSpeci = new[] { 155, 156, 157, 158 };
                collegespecs.AddRange(othersSpeci);
            }

            collegespecs.Remove(154);

            int[] degreeids = { 4, 3, 5, 6, 7 };
            foreach (var s in collegespecs)
            {
                var specid = specializations.FirstOrDefault(i => i.id == s);

                if (specid != null)
                {
                    var deptment = departments.FirstOrDefault(i => i.id == specid.departmentId && degreeids.Contains(i.degreeId));
                    var degree = degrees.FirstOrDefault(i => i.id == (deptment != null ? deptment.degreeId : 0));
                    if (degree != null)
                        specs.Add(new DistinctSpecializations { SpecializationId = specid.id, SpecializationName = deptment.departmentDescription, DepartmentName = deptment.departmentDescription, DepartmentId = specid.departmentId });
                }
            }
            //specs.Add(new DistinctSpecializations { SpecializationId = specid.id, SpecializationName = degree.degree + " - " + specid.specializationName, DepartmentId = specid.departmentId });
            ViewBag.departments = specs.OrderBy(i => i.DepartmentId);

            ViewBag.designation =
                db.jntuh_designation.Where(d => d.isActive == true && (d.id == 1 || d.id == 2 || d.id == 3)).Select(e => e).ToList();
            Scmuploads newScmuploads = new Scmuploads();
            jntuh_scmupload scmupload = db.jntuh_scmupload.Where(u => u.Id == id).Select(s => s).FirstOrDefault();
            //&&scmupload.CollegeId==userCollegeId
            if (scmupload != null)
            {
                newScmuploads.Id = scmupload.Id;
                newScmuploads.RegistrationNo = scmupload.RegistrationNumber.Trim();
                newScmuploads.DegreeId = scmupload.DegreeId;
                newScmuploads.DepartmentId = scmupload.DepartmentId;
                newScmuploads.SpecializationId = scmupload.SpecializationId;
                int did =
                    db.jntuh_designation.Where(d => d.designation == scmupload.Designation)
                        .Select(s => s.id)
                        .FirstOrDefault();
                newScmuploads.DesignationId = did;
                newScmuploads.Department =
                    specs.Where(d => d.DepartmentId == newScmuploads.DepartmentId)
                        .Select(s => s.DepartmentName)
                        .FirstOrDefault();
                //DateTime date = scmupload.SCMdate.Date;
                //newScmuploads.ScmDate = scmupload.SCMdate.Date.ToShortDateString();
                if (scmupload.CollegeId == userCollegeId)
                {
                    newScmuploads.NewScmDate = UAAAS.Models.Utilities.MMDDYY2DDMMYY(scmupload.SCMdate.ToString());
                    newScmuploads.AssistantProfessorDocumentView = scmupload.SCMDocument;
                }

            }
            else
            {
                jntuh_college_faculty_registered collegeFacultyRegistered =
                    db.jntuh_college_faculty_registered.Where(
                        r => r.RegistrationNumber.Trim() == registrationnumber.Trim()).FirstOrDefault();
                newScmuploads.CollegeId = collegeFacultyRegistered.collegeId;
                newScmuploads.RegistrationNo = collegeFacultyRegistered.RegistrationNumber.Trim();
                //newScmuploads.DegreeId = scmupload.DegreeId;
                newScmuploads.DepartmentId = (int)collegeFacultyRegistered.DepartmentId;
                if (collegeFacultyRegistered.SpecializationId != null)
                    newScmuploads.SpecializationId = (int)collegeFacultyRegistered.SpecializationId;
                newScmuploads.Department =
                    specs.Where(d => d.DepartmentId == newScmuploads.DepartmentId)
                        .Select(s => s.DepartmentName)
                        .FirstOrDefault();
                newScmuploads.Id = 0;

            }
            return PartialView("FacultySCMEdit", newScmuploads);
        }


        /// <summary>
        /// Faculty SCM Update
        /// </summary>
        /// <param name="scmedit"></param>
        /// <returns></returns>
        [Authorize(Roles = "College")]
        [HttpPost]
        public ActionResult FacultySCMEdit(Scmuploads scmedit)
        {
            var isedit = isFacultySCMupload();
            if (!isedit)
            {
                return RedirectToAction("College", "Dashboard");
            }
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int ay0 = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();
            DateTime scmuploadstartdate = new DateTime(2018, 02, 10);
            var userId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            var userCollegeId = db.jntuh_college_users.Where(u => u.userID == userId).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeId == 375)
            {
                userCollegeId = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            var jntuh_specialization = db.jntuh_specialization.AsNoTracking().ToList();
            var jntuh_department = db.jntuh_department.AsNoTracking().ToList();
            const string scmnotificationpath = "~/Content/Upload/SCMUploads";
            //var jntuh_scmupload = new jntuh_scmupload();
            var jntuh_college = db.jntuh_college.Where(e => e.isActive == true).Select(e => e).ToList();
            #region Saving Pdf data if Upload SCM File
            //SCM Document Saving
            if (scmedit.SCMDocument != null)
            {
                if (!Directory.Exists(Server.MapPath(scmnotificationpath)))
                {
                    Directory.CreateDirectory(Server.MapPath(scmnotificationpath));
                }

                var ext = Path.GetExtension(scmedit.SCMDocument.FileName);
                if (ext != null && ext.ToUpper().Equals(".PDF"))
                {
                    var ScmFileName =
                    jntuh_college.Where(c => c.id == userCollegeId).Select(c => c.collegeCode).FirstOrDefault() +
                    "_" + scmedit.RegistrationNo.Trim() + DateTime.Now.ToString("yyyMMddHHmmss");
                    scmedit.SCMDocument.SaveAs(string.Format("{0}/{1}{2}",
                   Server.MapPath(scmnotificationpath), ScmFileName, ext));
                    scmedit.SCMDocumentView = ScmFileName + ext;

                }
            }
            #endregion

            jntuh_scmupload scmupdate =
                db.jntuh_scmupload.Where(
                    s => s.Id == scmedit.Id && s.RegistrationNumber.Trim() == scmedit.RegistrationNo.Trim() && s.DepartmentId != 0)
                    .Select(s => s)
                    .FirstOrDefault();
            //Update Coding
            if (scmupdate != null)
            {
                scmupdate.CollegeId = userCollegeId;
                scmupdate.DepartmentId = scmedit.DepartmentId;
                var specialization = jntuh_specialization.FirstOrDefault(i => i.departmentId == scmedit.DepartmentId);
                var department = jntuh_department.FirstOrDefault(i => i.id == scmupdate.DepartmentId);
                scmupdate.DegreeId = department != null ? department.degreeId : 0;
                if (scmedit.SpecializationId != null)
                    scmupdate.SpecializationId = scmedit.SpecializationId;
                scmupdate.RegistrationNumber = scmupdate.RegistrationNumber;
                scmupdate.Designation = db.jntuh_designation.Where(d => d.id == scmedit.DesignationId).Select(s => s.designation).FirstOrDefault();
                if (scmedit.SCMDocumentView != null)
                {
                    scmupdate.SCMDocument = scmedit.SCMDocumentView;
                }

                //scmupdate.SCMdate =Convert.ToDateTime(scmedit.NewScmDate);
                if (scmedit.NewScmDate != null)
                    scmupdate.SCMdate = UAAAS.Models.Utilities.DDMMYY2MMDDYY(scmedit.NewScmDate);
                scmupdate.CreatedOn = scmupdate.CreatedOn;
                scmupdate.CreatedBy = scmupdate.CreatedBy;
                scmupdate.UpdatedOn = DateTime.Now;
                scmupdate.UpdatedBy = userId;
                db.Entry(scmupdate).State = EntityState.Modified;
                db.SaveChanges();
                TempData["Success"] = "SCM Update Successfully......";
            }
            else
            {
                //Save Coding
                jntuh_scmupload newscm = new jntuh_scmupload();
                var checkscm =
                    db.jntuh_scmupload.Where(
                        u => u.RegistrationNumber == scmedit.RegistrationNo.Trim() && u.DepartmentId != 0)
                        .Select(s => s)
                        .FirstOrDefault();
                if (checkscm != null)
                {
                    TempData["Success"] = "SCM alredy Uploaded......";
                    return RedirectToAction("SCMUpload");
                }
                db.jntuh_scmupload.Where(
                    s => s.Id == scmedit.Id && s.RegistrationNumber.Trim() == scmedit.RegistrationNo.Trim())
                    .Select(s => s)
                    .FirstOrDefault();
                newscm.academicYearId = ay0;
                newscm.CollegeId = userCollegeId;
                newscm.DepartmentId = scmedit.DepartmentId;
                var specialization = jntuh_specialization.FirstOrDefault(i => i.departmentId == scmedit.DepartmentId);
                var department = jntuh_department.FirstOrDefault(i => i.id == scmedit.DepartmentId);
                newscm.DegreeId = department != null ? department.degreeId : 0;
                if (scmedit.SpecializationId != null)
                    newscm.SpecializationId = scmedit.SpecializationId;
                newscm.RegistrationNumber = scmedit.RegistrationNo.Trim();
                newscm.Designation = db.jntuh_designation.Where(d => d.id == scmedit.DesignationId).Select(s => s.designation).FirstOrDefault();
                if (scmedit.SCMDocumentView != null)
                {
                    newscm.SCMDocument = scmedit.SCMDocumentView;
                }
                //newscm.SCMdate = Convert.ToDateTime(scmedit.NewScmDate);
                if (scmedit.NewScmDate != null)
                    newscm.SCMdate = UAAAS.Models.Utilities.DDMMYY2MMDDYY(scmedit.NewScmDate);
                newscm.CreatedOn = DateTime.Now;
                newscm.CreatedBy = userId;
                newscm.IsActive = true;
                newscm.UpdatedOn = null;
                newscm.UpdatedBy = null;
                db.jntuh_scmupload.Add(newscm);
                db.SaveChanges();
                TempData["Success"] = "SCM Uploaded Successfully......";
            }
            return RedirectToAction("SCMUpload");
        }

        //SCM Delete For Faculty
        [Authorize(Roles = "College")]
        public ActionResult DeleteSCMUploadForFaculty(int id)
        {
            return RedirectToAction("College", "Dashboard");
            var isedit = isFacultySCMupload();
            if (!isedit)
            {
                return RedirectToAction("College", "Dashboard");
            }
            DateTime scmuploadstartdate = new DateTime(2018, 02, 10);
            var userId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            var userCollegeId = db.jntuh_college_users.Where(u => u.userID == userId).Select(u => u.collegeID).FirstOrDefault();
            if (id != 0)
            {
                jntuh_scmupload deletejntuh_scmupload =
                    db.jntuh_scmupload.Where(s => s.CollegeId == userCollegeId && s.Id == id && s.DepartmentId != 0)
                        .Select(s => s)
                        .FirstOrDefault();
                if (deletejntuh_scmupload != null)
                {
                    db.jntuh_scmupload.Remove(deletejntuh_scmupload);
                    db.SaveChanges();
                    TempData["Success"] = deletejntuh_scmupload.RegistrationNumber + " SCM Deleted Successfully......";
                }
            }
            return RedirectToAction("SCMUpload");
        }

        //SCM Upload for Princiapl
        [Authorize(Roles = "College")]
        [HttpGet]
        public ActionResult SCMUploadForPrincipal()
        {
            var isedit = isPrincipalSCMupload();
            if (!isedit)
            {
                return RedirectToAction("College", "Dashboard");
            }
            //Note:In DB (ProfDocument) column considered as Faculty Registration Number,(AssocDocument) column considered as Faculty Designatin,(AssistDocument) as Considered as Faculty SCM Document
            //DateTime scmuploadstartdate = new DateTime(2018, 02, 10);
            DateTime scmuploadstartdate = new DateTime(2018, 02, 10);
            var userId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            var userCollegeId = db.jntuh_college_users.Where(u => u.userID == userId).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeId == 375)
            {
                userCollegeId = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            List<SCMUploadForPrincipal> scmuploadeddata = (from SCMUPL in db.jntuh_scmupload
                                                           where SCMUPL.IsActive == true && SCMUPL.CreatedOn > scmuploadstartdate && SCMUPL.CollegeId == userCollegeId && SCMUPL.SpecializationId == 0 && SCMUPL.DepartmentId == 0 && SCMUPL.DegreeId == 0
                                                           select new SCMUploadForPrincipal()
                                                            {
                                                                Id = SCMUPL.Id,
                                                                CollegeId = SCMUPL.CollegeId,
                                                                SpecializationId = SCMUPL.SpecializationId,
                                                                DegreeId = SCMUPL.DegreeId,
                                                                DepartmentId = SCMUPL.DepartmentId,
                                                                RegistrationNo = SCMUPL.RegistrationNumber,
                                                                DesignationName = SCMUPL.Designation,
                                                                ScmDateView = SCMUPL.SCMdate,
                                                                PrincipalDocumentView = SCMUPL.SCMDocument

                                                            }).ToList();

            ViewBag.SCMUPLOADEDDATA = scmuploadeddata;

            return View();
        }

        [Authorize(Roles = "College")]
        [HttpPost]
        public ActionResult SCMUploadForPrincipal(SCMUploadForPrincipal scmdataforPrincipal)
        {
            var isedit = isPrincipalSCMupload();
            if (!isedit)
            {
                return RedirectToAction("College", "Dashboard");
            }
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int ay0 = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();
            //Note:In DB (ProfDocument) column considered as Faculty Registration Number,(AssocDocument) column considered as Faculty Designatin,(AssistDocument) as Considered as Faculty SCM Document
            DateTime scmuploadstartdate = new DateTime(2018, 02, 10);
            var userId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            var userCollegeId = db.jntuh_college_users.Where(u => u.userID == userId).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeId == 375)
            {
                userCollegeId = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            string faRegistrationnumber =
               db.jntuh_registered_faculty.Where(s => s.RegistrationNumber == scmdataforPrincipal.RegistrationNo.Trim())
                   .Select(r => r.RegistrationNumber)
                   .FirstOrDefault();
            if (String.IsNullOrEmpty(faRegistrationnumber))
            {
                TempData["Error"] = "Invalid Registration Number";
                return RedirectToAction("SCMUploadForPrincipal");
            }
            string CRegistrationnumber =
                db.jntuh_college_faculty_registered.Where(s => s.RegistrationNumber == scmdataforPrincipal.RegistrationNo.Trim() && s.collegeId == userCollegeId)
                    .Select(r => r.RegistrationNumber)
                    .FirstOrDefault();
            string PRegistrationnumber =
              db.jntuh_college_principal_registered.Where(s => s.RegistrationNumber == scmdataforPrincipal.RegistrationNo.Trim() && s.collegeId == userCollegeId)
                  .Select(r => r.RegistrationNumber)
                  .FirstOrDefault();
            if (String.IsNullOrEmpty(PRegistrationnumber))
            {
                TempData["Error"] = "First Add the Faculty Registration Number in College Portal as Principal";
                return RedirectToAction("SCMUploadForPrincipal");
            }

            int ScmUploadId =
               db.jntuh_scmupload.Where(
                   s => s.RegistrationNumber.Trim() == scmdataforPrincipal.RegistrationNo.Trim() && s.CollegeId == userCollegeId && s.DegreeId == 0 && s.DepartmentId == 0 && s.SpecializationId == 0 && s.CreatedOn >= scmuploadstartdate)
                   .Select(s => s.Id)
                   .FirstOrDefault();
            if (ScmUploadId != 0)
            {
                TempData["Error"] = "SCM already Uploaded for this Registration Number";
                return RedirectToAction("SCMUploadForPrincipal");
            }
            if (scmdataforPrincipal.PrincipalDocument == null || scmdataforPrincipal.ScmDate == null || String.IsNullOrEmpty(scmdataforPrincipal.RegistrationNo))
            {
                TempData["Error"] = "Enter Valid Fields";
                return RedirectToAction("SCMUploadForPrincipal");
            }

            jntuh_scmupload jntuh_scmuploadupdate =
              db.jntuh_scmupload.Where(
                  s => s.RegistrationNumber.Trim() == scmdataforPrincipal.RegistrationNo.Trim() && s.CollegeId == userCollegeId && s.DegreeId == 0 && s.DepartmentId == 0 && s.SpecializationId == 0 && s.CreatedOn >= scmuploadstartdate)
                  .Select(s => s)
                  .FirstOrDefault();

            if (ModelState.IsValid)
            {
                var jntuh_college = db.jntuh_college.Where(e => e.isActive == true).Select(e => e).ToList();
                const string scmnotificationpath = "~/Content/Upload/SCMUploads";

                if (jntuh_scmuploadupdate == null)
                {
                    //Professor's Document Saving
                    if (scmdataforPrincipal.PrincipalDocument != null)
                    {
                        if (!Directory.Exists(Server.MapPath(scmnotificationpath)))
                        {
                            Directory.CreateDirectory(Server.MapPath(scmnotificationpath));
                        }

                        var ext = Path.GetExtension(scmdataforPrincipal.PrincipalDocument.FileName);
                        if (ext != null && ext.ToUpper().Equals(".PDF"))
                        {
                            var professorScmFileName = jntuh_college.Where(c => c.id == userCollegeId).Select(c => c.collegeCode).FirstOrDefault() + "_" + scmdataforPrincipal.RegistrationNo.Trim() + "_" + DateTime.Now.ToString("yyyMMddHHmmss");
                            scmdataforPrincipal.PrincipalDocument.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(scmnotificationpath), professorScmFileName, ext));
                            scmdataforPrincipal.PrincipalDocumentView = professorScmFileName + ext;
                        }
                    }
                    var jntuh_scmupload = new jntuh_scmupload();

                    jntuh_scmupload.CollegeId = userCollegeId;
                    jntuh_scmupload.academicYearId = ay0;
                    jntuh_scmupload.SpecializationId = 0;
                    jntuh_scmupload.DepartmentId = 0;
                    jntuh_scmupload.DegreeId = 0;
                    jntuh_scmupload.RegistrationNumber = scmdataforPrincipal.RegistrationNo.Trim();
                    jntuh_scmupload.Designation = "Principal";
                    jntuh_scmupload.SCMDocument = scmdataforPrincipal.PrincipalDocumentView ?? "";
                    if (scmdataforPrincipal.ScmDate != null)
                        jntuh_scmupload.SCMdate = UAAAS.Models.Utilities.DDMMYY2MMDDYY(scmdataforPrincipal.ScmDate);
                    jntuh_scmupload.IsActive = true;
                    jntuh_scmupload.CreatedBy = userId;
                    jntuh_scmupload.CreatedOn = DateTime.Now;
                    db.jntuh_scmupload.Add(jntuh_scmupload);
                    db.SaveChanges();
                    TempData["Success"] = "Principal SCM Uploaded Successfully......";
                }
                else
                {
                    if (scmdataforPrincipal.PrincipalDocument != null)
                    {
                        if (!Directory.Exists(Server.MapPath(scmnotificationpath)))
                        {
                            Directory.CreateDirectory(Server.MapPath(scmnotificationpath));
                        }

                        var ext = Path.GetExtension(scmdataforPrincipal.PrincipalDocument.FileName);
                        if (ext != null && ext.ToUpper().Equals(".PDF"))
                        {
                            var professorScmFileName = jntuh_college.Where(c => c.id == userCollegeId).Select(c => c.collegeCode).FirstOrDefault() + "_" + scmdataforPrincipal.RegistrationNo.Trim() + "_" + DateTime.Now.ToString("yyyMMddHHmmss");
                            scmdataforPrincipal.PrincipalDocument.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(scmnotificationpath), professorScmFileName, ext));
                            scmdataforPrincipal.PrincipalDocumentView = professorScmFileName + ext;
                        }
                        jntuh_scmuploadupdate.SCMDocument = scmdataforPrincipal.PrincipalDocumentView ?? "";
                        //jntuh_scmuploadupdate.CreatedOn = DateTime.Now;
                        //jntuh_scmuploadupdate.
                    }
                }
            }
            else
            {
                TempData["Error"] = "Enter Data Mandatory Fields";
            }
            return RedirectToAction("SCMUploadForPrincipal");
        }

        //SCM Principal Delete
        [Authorize(Roles = "College")]
        public ActionResult DeleteSCMUploadForPrincipal(int id)
        {
            // return RedirectToAction("College", "Dashboard");
            var isedit = isPrincipalSCMupload();
            if (!isedit)
            {
                return RedirectToAction("College", "Dashboard");
            }
            var userId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            var userCollegeId = db.jntuh_college_users.Where(u => u.userID == userId).Select(u => u.collegeID).FirstOrDefault();
            if (id != 0)
            {
                jntuh_scmupload deletejntuh_scmupload =
                    db.jntuh_scmupload.Where(s => s.CollegeId == userCollegeId && s.Id == id && s.DepartmentId == 0)
                        .Select(s => s)
                        .FirstOrDefault();
                if (deletejntuh_scmupload != null)
                {
                    db.jntuh_scmupload.Remove(deletejntuh_scmupload);
                    db.SaveChanges();
                    TempData["Success"] = deletejntuh_scmupload.RegistrationNumber + " SCM Deleted Successfully......";
                }
            }
            return RedirectToAction("SCMUploadForPrincipal");
        }

        //Checking for Principal SCM Upload have Live 
        private bool isPrincipalSCMupload()
        {
            bool islive = false;
            DateTime todayDate = DateTime.Now;
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int ay0 = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();
            int lid =
                db.jntuh_link_screens.Where(p => p.linkCode == "PSU" && p.isActive == true).Select(s => s.id).FirstOrDefault();
            jntuh_college_links_assigned scmphase =
                db.jntuh_college_links_assigned.Where(l => l.linkId == lid && l.academicyearId == ay0 && l.isActive == true && l.fromdate <= todayDate && l.todate >= todayDate).Select(s => s).FirstOrDefault();
            if (scmphase != null && Roles.IsUserInRole("College"))
                islive = true;

            return islive;
        }

        //Checking for Principal SCM Upload have Live 
        private bool isFacultySCMupload()
        {
            bool islive = false;
            DateTime todayDate = DateTime.Now;
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int ay0 = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();
            int lid =
                db.jntuh_link_screens.Where(p => p.linkCode == "FSU" && p.isActive == true).Select(s => s.id).FirstOrDefault();
            jntuh_college_links_assigned scmphase =
                db.jntuh_college_links_assigned.Where(l => l.linkId == lid && l.academicyearId == ay0 && l.isActive == true && l.fromdate <= todayDate && l.todate >= todayDate).Select(s => s).FirstOrDefault();
            if (scmphase != null && Roles.IsUserInRole("College"))
                islive = true;

            return islive;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public ActionResult SCMUploadView(int? collegeId, int? departmentId, string SCMdate, int? SCMdateId)
        {
            var collegeIds = db.jntuh_scmupload.Where(e => e.IsActive == true).Select(e => e.CollegeId).Distinct().ToArray();
            ViewBag.Colleges = db.jntuh_college.Where(e => e.isActive == true && collegeIds.Contains(e.id)).Select(e => new { collegeId = e.id, collegeName = e.collegeCode + "-" + e.collegeName }).OrderBy(e => e.collegeName).ToList();
            //Get the Deprtments Based on the CollegeId
            if (collegeId != null)
            {
                var specs = new List<DistinctSpecializations>();
                var degrees = db.jntuh_degree.AsNoTracking().ToList();
                var specializations = db.jntuh_specialization.AsNoTracking().ToList();
                var departments = db.jntuh_department.AsNoTracking().ToList();

                List<int> collegespecs = new List<int>();
                collegespecs.AddRange(
                    db.jntuh_college_intake_existing.Where(i => i.collegeId == collegeId)
                        .Select(i => i.specializationId)
                        .Distinct()
                        .ToArray());

                foreach (var s in collegespecs)
                {
                    var specid = specializations.FirstOrDefault(i => i.id == s);

                    if (specid != null)
                    {
                        var deptment = departments.FirstOrDefault(i => i.id == specid.departmentId);
                        var degree = degrees.FirstOrDefault(i => i.id == (deptment != null ? deptment.degreeId : 0));
                        if (degree != null)
                            specs.Add(new DistinctSpecializations { SpecializationId = specid.id, SpecializationName = degree.degree + " - " + specid.specializationName, DepartmentId = specid.departmentId, DepartmentName = specid.jntuh_department.departmentName });
                    }
                }

                ViewBag.departments = specs.OrderBy(i => i.DepartmentName);
            }


            if (collegeId != null && departmentId != null)
            {
                List<Scmdates> SCMdatesList = new List<Scmdates>();
                SCMdatesList.AddRange(db.jntuh_scmupload.Where(e => e.IsActive == true && e.CollegeId == collegeId && e.SpecializationId == departmentId).Select(e => new Scmdates() { SCMDATE = e.SCMdate, SCMDateId = e.Id }).ToList());

                ViewBag.SCMDates = SCMdatesList.Select(e => new Scmdates() { SCMDATEview = UAAAS.Models.Utilities.MMDDYY2DDMMYY(e.SCMDATE.ToString()), SCMDateId = e.SCMDateId }).ToList();

                var specialization = db.jntuh_specialization.Where(e => e.isActive == true && e.id == departmentId).Select(e => e).ToList();

                ViewBag.MenuData = specialization.Select(e => new jntuh_specialization()
                            {
                                specializationName = e.jntuh_department.jntuh_degree.degree + "-" + e.specializationName,
                                specializationDescription = e.jntuh_department.departmentName

                            })
                        .FirstOrDefault();
            }
            if (collegeId != null && departmentId != null && SCMdate != null && SCMdateId != null)
            {
                DateTime SCMdateFormat = new DateTime();
                SCMdateFormat = UAAAS.Models.Utilities.DDMMYY2MMDDYY(SCMdate);
                var scmuloaddata = db.jntuh_scmupload.Where(e => e.IsActive == true && e.CollegeId == collegeId && e.SpecializationId == departmentId && e.SCMdate == SCMdateFormat && e.Id == SCMdateId)
                        .Select(e => e)
                        .ToList();

                ViewBag.SCMData = scmuloaddata;
            }

            return View();
        }



        [Authorize(Roles = "Admin")]
        [HttpGet]
        public ActionResult TempFacultyVerification()
        {

            List<FacultyRegistration> teachingFaculty = new List<FacultyRegistration>();
            // List<jntuh_registered_faculty> jntuh_registered_faculty = new List<jntuh_registered_faculty>();
            List<jntuh_registered_faculty_log> jntuh_registered_faculty_log = new List<jntuh_registered_faculty_log>();

            //string[] FacultyRegNumbers = db.jntuh_college_faculty_registered.Where(C => C.collegeId == collegeid).Select(C => C.RegistrationNumber).ToArray();
            //if (collegeid != null && collegeid != 0)
            jntuh_registered_faculty_log = db.jntuh_registered_faculty_log.Where(c => c.isActive == true && (c.FacultyApprovedStatus == 0 || c.FacultyApprovedStatus == 3)).ToList();//c => FacultyRegNumbers.Contains(c.RegistrationNumber)
            //else
            //    jntuh_registered_faculty_log = db.jntuh_registered_faculty_log.Where(c => c.isActive == true && c.FacultyApprovedStatus == 0).Take(50).ToList();
            var data = jntuh_registered_faculty_log.Select(a => new FacultyRegistration
            {

                id = db.jntuh_registered_faculty.Where(f => f.UserId == a.UserId).Select(f => f.id).FirstOrDefault(),
                Type = a.type,
                RegistrationNumber = a.RegistrationNumber,
                UniqueID = a.UniqueID,
                FirstName = a.FirstName,
                MiddleName = a.MiddleName,
                LastName = a.LastName,
                GenderId = a.GenderId,
                Email = a.Email,
                facultyPhoto = a.Photo,
                Mobile = a.Mobile,
                PANNumber = a.PANNumber,
                AadhaarNumber = a.AadhaarNumber,
                isActive = a.isActive,
                isApproved = a.isApproved,
                SamePANNumberCount = 1,
                SameAadhaarNumberCount = 2,
                FIsApproved = a.FacultyApprovedStatus


            });
            teachingFaculty.AddRange(data);
            return View(teachingFaculty);
        }

        [Authorize(Roles = "Admin")]
        public ActionResult ApprovedFaculty(int facultyAddId, int collegeId)
        {
            var userId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            if (facultyAddId != 0 && collegeId != 0)
            {
                var data = db.jntuh_scmproceedingrequest_addfaculty.Where(e => e.Id == facultyAddId).Select(e => e).FirstOrDefault();
                if (data != null)
                {
                    data.IsApproved = true;
                    data.Updatedby = userId;
                    data.UpdatedOn = DateTime.Now;
                    data.Isactive = true;
                    db.Entry(data).State = EntityState.Modified;
                    db.SaveChanges();
                }
                TempData["Success"] = "Faculty Approved Successfully";
                return RedirectToAction("ScmFacultyVerfication", "CollegeSCMProceedingsRequest", new { collegeId = collegeId });
            }
            return RedirectToAction("ScmFacultyVerfication", "CollegeSCMProceedingsRequest");
        }


        [Authorize(Roles = "Admin")]
        public ActionResult NotApproveFaculty(int facultyAddId, int collegeId)
        {
            Facultynotapproved data = new Facultynotapproved();
            if (facultyAddId != 0 && collegeId != 0)
            {
                data.FacultyAddId = facultyAddId;
                data.CollegeId = collegeId;
                // data.DeactivationReason = "";
            }
            return PartialView("_NotApproveFaculty", data);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public ActionResult NotApproveFaculty(Facultynotapproved facultynotapproved)
        {
            TempData["Error"] = null;
            int userId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            if (ModelState.IsValid)
            {
                if (facultynotapproved != null)
                {
                    jntuh_scmproceedingrequest_addfaculty addfaculty = db.jntuh_scmproceedingrequest_addfaculty.Where(e => e.Id == facultynotapproved.FacultyAddId).Select(e => e).FirstOrDefault();
                    if (addfaculty != null)
                    {
                        addfaculty.DeactiviationReason = facultynotapproved.DeactivationReason;
                        addfaculty.Updatedby = userId;
                        addfaculty.UpdatedOn = DateTime.Now;
                        addfaculty.Isactive = true;
                        addfaculty.IsApproved = false;
                        db.Entry(addfaculty).State = EntityState.Modified;
                        db.SaveChanges();
                        TempData["Success"] = "Faculty Add Successfully";
                    }
                    return RedirectToAction("ScmFacultyVerfication", "CollegeSCMProceedingsRequest", new { collegeId = facultynotapproved.CollegeId });
                }
            }
            return RedirectToAction("ScmFacultyVerfication", "CollegeSCMProceedingsRequest");
        }

        [Authorize(Roles = "College")]
        [HttpGet]
        public ActionResult CollegeScmProceedingsPrincipalRequest()
        {
            var todayDate = DateTime.Now;
            var userId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            var userCollegeId = db.jntuh_college_users.Where(u => u.userID == userId).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeId == 375)
            {
                userCollegeId = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int ay0 = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();
            int lid =
                db.jntuh_link_screens.Where(p => p.linkName == "Principal SCM Request" && p.isActive == true).Select(s => s.id).FirstOrDefault();
            jntuh_college_links_assigned scmphase =
                db.jntuh_college_links_assigned.Where(l => l.linkId == lid && l.academicyearId == ay0 && l.isActive == true && l.fromdate <= todayDate && l.todate >= todayDate).Select(s => s).FirstOrDefault();
            if (scmphase == null && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("College", "Dashboard");
            }
            //principal SCM Request open from 13-10-2017
            //principal SCM Request open from 19-12-2017
            //principal SCM Request open from 29-01-2018
            //Appeal Principal SCM Starts From 10-04-2018
            //DateTime SCMStartDate = new DateTime(2018, 08,09);
            DateTime SCMStartDate = Convert.ToDateTime(scmphase.fromdate);

            var jntuh_college = db.jntuh_college.Where(e => e.isActive == true).Select(e => e).ToList();

            jntuh_college.Add(new jntuh_college() { id = 0, collegeCode = "Not Working" });
            jntuh_college.Add(new jntuh_college() { id = -1, collegeCode = "Others" });

            ViewBag.Colleges = jntuh_college.Select(e => new { collegeId = e.id, collegeName = e.collegeCode + "-" + e.collegeName }).OrderBy(e => e.collegeId).ToList();

            List<SCMPrincipal> scmPrincipaldata = new List<SCMPrincipal>();
            scmPrincipaldata = (from SCMREQ in db.jntuh_scmproceedingsrequests
                                join SCMADDFLY in db.jntuh_scmproceedingrequest_addfaculty on SCMREQ.ID equals SCMADDFLY.ScmProceedingId
                                join REGFLY in db.jntuh_registered_faculty on SCMADDFLY.RegistrationNumber equals REGFLY.RegistrationNumber
                                where SCMREQ.CollegeId == userCollegeId && SCMREQ.SpecializationId == 0 && SCMREQ.DegreeId == 0 && SCMREQ.DEpartmentId == 0 && SCMADDFLY.FacultyType == 0 && SCMREQ.CreatedOn >= SCMStartDate
                                select new SCMPrincipal
                                {
                                    RegistrationNo = SCMADDFLY.RegistrationNumber,
                                    FirstName = REGFLY.FirstName + " " + REGFLY.LastName + " " + REGFLY.LastName,
                                    scmnotificationdocview = SCMREQ.SCMNotification,
                                    createdDate = (DateTime)SCMREQ.RequestSubmittedDate,
                                    FacultyId = REGFLY.id,
                                    SCMId = SCMREQ.ID,
                                    ScmAddfacultyId = SCMADDFLY.Id,
                                    paymentid = SCMREQ.PaymentTypeId,
                                    PrincipalDetailsFound = false
                                }).OrderByDescending(e => e.createdDate).ToList();
            foreach (var item in scmPrincipaldata)
            {
                var principaldetails = db.jntuh_principal_details_affiliated_colleges.Where(i => i.registrationnumber == item.RegistrationNo && i.collegeid == userCollegeId).ToList();
                if (principaldetails.Count > 0)
                {
                    item.PrincipalDetailsFound = true;
                }
            }
            var scmproceedingsprincipalregnos = scmPrincipaldata.Select(e => e.RegistrationNo).ToArray();
            var scmprincipaldetailsregnos = db.jntuh_principal_details_affiliated_colleges.Where(i => scmproceedingsprincipalregnos.Contains(i.registrationnumber) && i.collegeid == userCollegeId).Count();
            if (scmproceedingsprincipalregnos.Count() == scmprincipaldetailsregnos)
            {
                ViewBag.principaldetailsfound = true;
            }
            else
            {
                ViewBag.principaldetailsfound = false;
            }
            ViewBag.SCMPrincipal = scmPrincipaldata;
            ViewBag.SCMPrincipalcount = scmPrincipaldata.Count();
            int paymentid =
              db.jntuh_college_paymentoffee_type.Where(p => p.FeeType == "SCM Principal Payment")
                  .Select(s => s.id)
                  .FirstOrDefault();
            //Payment Details
            string payclgcode = db.jntuh_college.Where(c => c.id == userCollegeId && c.isActive == true).Select(s => s.collegeCode).FirstOrDefault();
            var paymenthistory =
               db.jntuh_paymentresponse.Where(
                   it =>
                       it.CollegeId == payclgcode && it.AcademicYearId == ay0 && it.AuthStatus == "0300" &&
                       it.TxnDate.Year == todayDate.Year && it.PaymentTypeID == paymentid && it.TxnDate >= SCMStartDate)
                   .Select(s => s).ToList();
            ViewBag.Payments = paymenthistory;
            var scmproceedings = db.jntuh_scmproceedingsrequests.Where(e => e.CollegeId == userCollegeId && e.SpecializationId == 0 && e.DEpartmentId == 0 && e.DegreeId == 0 && e.RequestSubmittedDate == null && e.CreatedOn >= SCMStartDate).Select(e => e);
            ViewBag.VisiableRequestSumissionButton = scmproceedings.Count();
            return View();
        }

        [Authorize(Roles = "College")]
        [HttpPost]
        public ActionResult CollegeScmProceedingsPrincipalRequest(SCMPrincipal scmprincipaldata)
        {
            var todayDate = DateTime.Now;
            var userId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            var userCollegeId = db.jntuh_college_users.Where(u => u.userID == userId).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeId == 375)
            {
                userCollegeId = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int ay0 = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();
            int lid =
                db.jntuh_link_screens.Where(p => p.linkName == "Principal SCM Request" && p.isActive == true).Select(s => s.id).FirstOrDefault();
            jntuh_college_links_assigned scmphase =
                db.jntuh_college_links_assigned.Where(l => l.linkId == lid && l.academicyearId == ay0 && l.isActive == true && l.fromdate <= todayDate && l.todate >= todayDate).Select(s => s).FirstOrDefault();
            jntuh_registered_faculty jntuh_registered_faculty =
                       db.jntuh_registered_faculty.Where(rf => rf.RegistrationNumber.Trim() == scmprincipaldata.RegistrationNo.Trim())
                           .Select(s => s)
                           .FirstOrDefault();
            if (scmphase == null && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("College", "Dashboard");
            }
            if (jntuh_registered_faculty == null)
            {
                TempData["Error"] = "This Registration Number doesn't Exist.";
                return RedirectToAction("CollegeScmProceedingsPrincipalRequest", "CollegeSCMProceedingsRequest");
            }
            if (jntuh_registered_faculty.Blacklistfaculy == true || jntuh_registered_faculty.AbsentforVerification == true)
            {
                TempData["Error"] = "Faculty Registration Number is in Blacklist.";
                return RedirectToAction("CollegeScmProceedingsPrincipalRequest", "CollegeSCMProceedingsRequest");
            }
            //principal SCM Request open from 13-10-2017
            //principal SCM Request open from 19-12-2017
            //principal SCM Request open from 29-01-2018
            //DateTime SCMStartDate = new DateTime(2018, 08,19);

            DateTime SCMStartDate = Convert.ToDateTime(scmphase.fromdate);

            if (ModelState.IsValid)
            {
                var jntuh_college = db.jntuh_college.Where(e => e.isActive == true).Select(e => e).ToList();
                int SCmOrder = 1;
                var fileName = string.Empty;
                var filepath = string.Empty;
                var collegescmrequests = new jntuh_scmproceedingsrequests();
                const string scmnotificationpath = "~/Content/Upload/SCMPROCEEDINGSREQUEST/ScmNotificationDocuments";
                if (scmprincipaldata.ScmNotificationSupportDoc != null)
                {

                    var DataExisting = (from scm in db.jntuh_scmproceedingsrequests
                                        join scmadd in db.jntuh_scmproceedingrequest_addfaculty on scm.ID equals scmadd.ScmProceedingId
                                        where
                                            scm.CollegeId == userCollegeId && scm.SpecializationId == 0 && scm.DEpartmentId == 0 &&
                                            scm.DegreeId == 0 && scmadd.FacultyType == 0 &&
                                            scmadd.RegistrationNumber == scmprincipaldata.RegistrationNo && scm.CreatedOn >= SCMStartDate
                                        select scm.ID).FirstOrDefault();


                    if (DataExisting == 0)
                    {


                        if (!Directory.Exists(Server.MapPath(scmnotificationpath)))
                        {
                            Directory.CreateDirectory(Server.MapPath(scmnotificationpath));
                        }

                        var ext = Path.GetExtension(scmprincipaldata.ScmNotificationSupportDoc.FileName);
                        if (ext != null && ext.ToUpper().Equals(".PDF"))
                        {
                            var scmfileName =
                                db.jntuh_college.Where(c => c.id == userCollegeId).Select(c => c.id).FirstOrDefault() +
                                "_" +
                                "ScmNotofication" + "_" + DateTime.Now.ToString("yyyMMddHHmmss");
                            scmprincipaldata.ScmNotificationSupportDoc.SaveAs(string.Format("{0}/{1}{2}",
                                Server.MapPath(scmnotificationpath), scmfileName, ext));
                            collegescmrequests.SCMNotification = scmfileName + ext;
                        }
                        IUserMailer mailer = new UserMailer();
                        collegescmrequests.CollegeId = userCollegeId;
                        collegescmrequests.academicyearId = scmphase.academicyearId;
                        collegescmrequests.phaseId = scmphase.phaseId;
                        collegescmrequests.isscmatUniversity = true;
                        collegescmrequests.SpecializationId = 0;
                        collegescmrequests.DEpartmentId = 0;
                        collegescmrequests.DegreeId = 0;
                        collegescmrequests.ProfessorsCount = 0;
                        collegescmrequests.AssociateProfessorsCount = 0;
                        collegescmrequests.AssistantProfessorsCount = 0;
                        collegescmrequests.RequiredProfessor = 0;
                        collegescmrequests.RequiredAssistantProfessor = 0;
                        collegescmrequests.RequiredAssociateProfessor = 0;
                        if (scmprincipaldata.NotificationDate != null)
                            collegescmrequests.Notificationdate =
                                UAAAS.Models.Utilities.DDMMYY2MMDDYY(scmprincipaldata.NotificationDate);
                        collegescmrequests.CreatedBy = userId;
                        collegescmrequests.CreatedOn = DateTime.Now;
                        //  collegescmrequests.RequestSubmittedDate = DateTime.Now;
                        collegescmrequests.ISActive = true;

                        //Checking SCM Order Id
                        var scmdata =
                            db.jntuh_scmproceedingsrequests.Where(
                                e => e.ISActive == true && e.CollegeId == userCollegeId && e.SpecializationId == 0 &&
                                     e.DEpartmentId == 0 && e.DegreeId == 0)
                                .OrderByDescending(e => e.ID)
                                .Select(e => e)
                                .FirstOrDefault();
                        if (scmdata != null)
                        {
                            var assigneddata =
                                db.jntuh_auditor_assigned.Where(e => e.ScmId == scmdata.ID)
                                    .Select(e => e.Id)
                                    .FirstOrDefault();
                            if (assigneddata != 0)
                            {
                                SCmOrder = scmdata.SCMOrder + 1;
                            }
                            else
                            {
                                SCmOrder = scmdata.SCMOrder;
                            }
                        }
                        collegescmrequests.SCMOrder = SCmOrder;
                        collegescmrequests.TotalNoofFacultyRequired = 0;
                        db.jntuh_scmproceedingsrequests.Add(collegescmrequests);
                        try
                        {
                            db.SaveChanges();
                            int scmId = collegescmrequests.ID;

                            if (scmId != 0)
                            {
                                jntuh_scmproceedingrequest_addfaculty addfaculty =
                                    new jntuh_scmproceedingrequest_addfaculty();
                                addfaculty.ScmProceedingId = scmId;
                                addfaculty.academicyearId = scmphase.academicyearId;
                                addfaculty.phaseId = scmphase.phaseId;
                                addfaculty.RegistrationNumber = scmprincipaldata.RegistrationNo;
                                addfaculty.PreviousCollegeId = scmprincipaldata.PreviousCollegeId.ToString() == "-1"
                                    ? scmprincipaldata.PreviousCollegeName
                                    : scmprincipaldata.PreviousCollegeId != 0
                                        ? jntuh_college.Where(e => e.id == scmprincipaldata.PreviousCollegeId)
                                            .Select(e => e.collegeName)
                                            .FirstOrDefault()
                                        : scmprincipaldata.PreviousCollegeId.ToString();
                                //  scmprincipaldata.PreviousCollegeId.ToString();
                                addfaculty.FacultyType = 0;
                                addfaculty.Createdby = userId;
                                addfaculty.CreatedOn = DateTime.Now;
                                addfaculty.Isactive = true;
                                addfaculty.CollegeId = userCollegeId;
                                db.jntuh_scmproceedingrequest_addfaculty.Add(addfaculty);
                                db.SaveChanges();
                            }

                            //var attachments = new List<Attachment>();
                            //if (scmprincipaldata.ScmNotificationSupportDoc != null)
                            //{

                            //    fileName = Path.GetFileName(scmprincipaldata.ScmNotificationSupportDoc.FileName);
                            //    filepath = Path.Combine(Server.MapPath("~/Content/Attachments"), fileName);
                            //    scmprincipaldata.ScmNotificationSupportDoc.SaveAs(filepath);
                            //    attachments.Add(new Attachment(filepath));
                            //    mailer.SendAttachmentToAllColleges("sureshpalsa1@gmail.com", "", "",
                            //        "SCM PROCEEDINGS REQUEST", "Scm Requests", attachments).SendAsync();
                            //    TempData["Success"] = "Your request has been proccessed successfully..";
                            //}
                            TempData["Success"] = "Your request has been proccessed successfully..";
                        }
                        catch (DbEntityValidationException dbEx)
                        {
                            foreach (var validationErrors in dbEx.EntityValidationErrors)
                            {
                                foreach (var validationError in validationErrors.ValidationErrors)
                                {
                                    Trace.TraceInformation("Property: {0} Error: {1}",
                                        validationError.PropertyName,
                                        validationError.ErrorMessage);
                                }
                            }
                        }

                    }
                    else
                    {
                        TempData["Error"] = "This Registration Number already applied for SCM from your college.";
                    }
                }
                else
                {
                    TempData["Error"] = "Please Fill All Mandatory Fields..";
                }
            }
            return RedirectToAction("CollegeScmProceedingsPrincipalRequest");
        }

        [Authorize(Roles = "College")]
        [HttpGet]
        public ActionResult SCMPrincipalRequestSubmission()
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            var userCollegeId = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            //principal SCM Request open from 13-10-2017
            //principal SCM Request open from 19-12-2017
            //DateTime SCMStartDate = new DateTime(2017, 12, 19);
            DateTime todayDate = DateTime.Now;
            if (userCollegeId == 375)
            {
                userCollegeId = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int ay0 = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();
            int lid =
                db.jntuh_link_screens.Where(p => p.linkName == "Principal SCM Request" && p.isActive == true).Select(s => s.id).FirstOrDefault();
            jntuh_college_links_assigned scmphase =
                db.jntuh_college_links_assigned.Where(l => l.linkId == lid && l.academicyearId == ay0 && l.isActive == true && l.fromdate <= todayDate && l.todate >= todayDate).Select(s => s).FirstOrDefault();

            if (scmphase == null && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("College", "Dashboard");
            }
            if (userCollegeId != null)
            {
                List<jntuh_scmproceedingsrequests> ScmPrincipalList = db.jntuh_scmproceedingsrequests.Where(e => e.CollegeId == userCollegeId && e.SpecializationId == 0 && e.DEpartmentId == 0 && e.DegreeId == 0 && e.RequestSubmittedDate == null && e.CreatedOn >= scmphase.fromdate && e.phaseId == scmphase.phaseId).Select(e => e).ToList();
                if (ScmPrincipalList.Count() != 0)
                {
                    foreach (var data in ScmPrincipalList)
                    {
                        if (data.ID != 0)
                        {
                            var scmPrincipalrequestdata = db.jntuh_scmproceedingsrequests.Where(e => e.ID == data.ID).Select(e => e).FirstOrDefault();
                            if (scmPrincipalrequestdata != null)
                            {
                                scmPrincipalrequestdata.RequestSubmittedDate = DateTime.Now;
                                db.Entry(scmPrincipalrequestdata).State = EntityState.Modified;
                            }
                            db.SaveChanges();
                            TempData["Success"] = "Submission Successfully.";
                        }
                    }
                }
                else
                {
                    TempData["Error"] = "Please Add Principal SCM Request.";
                }
            }

            return RedirectToAction("CollegeScmProceedingsPrincipalRequest", "CollegeScmProceedingsRequest");
        }


        [Authorize(Roles = "Admin,SuperAdmin,College")]
        public ActionResult ViewFacultyDetails(string fid)
        {
            FacultyRegistration regFaculty = new FacultyRegistration();
            int fID = 0;

            if (!string.IsNullOrEmpty(fid))
            {
                regFaculty.GenderId = null;
                regFaculty.isFacultyRatifiedByJNTU = null;
                fID = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(fid, WebConfigurationManager.AppSettings["CryptoKey"]));
                // Above code commented by Naushad Khan Anbd added the below line.
                //fID = Convert.ToInt32(fid);
                jntuh_registered_faculty faculty = db.jntuh_registered_faculty.Find(fID);
                if (faculty != null)
                {
                    regFaculty.id = fID;
                    regFaculty.Type = faculty.type;
                    regFaculty.RegistrationNumber = faculty.RegistrationNumber;
                    regFaculty.UserName =
                        db.my_aspnet_users.Where(u => u.id == faculty.UserId).Select(u => u.name).FirstOrDefault();
                    regFaculty.Email = faculty.Email;
                    regFaculty.UniqueID = faculty.UniqueID;
                    regFaculty.FirstName = faculty.FirstName;
                    regFaculty.MiddleName = faculty.MiddleName;
                    regFaculty.LastName = faculty.LastName;
                    regFaculty.FatherOrhusbandName = faculty.FatherOrHusbandName;
                    regFaculty.MotherName = faculty.MotherName;
                    regFaculty.GenderId = faculty.GenderId;
                    if (faculty.DateOfBirth != null)
                    {
                        regFaculty.facultyDateOfBirth =
                            UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.DateOfBirth.ToString());
                    }
                    regFaculty.Mobile = faculty.Mobile;
                    regFaculty.facultyPhoto = faculty.Photo;
                    regFaculty.PANNumber = faculty.PANNumber;
                    regFaculty.facultyPANCardDocument = faculty.PANDocument;
                    regFaculty.AadhaarNumber = faculty.AadhaarNumber;
                    regFaculty.facultyAadhaarCardDocument = faculty.AadhaarDocument;
                    regFaculty.WorkingStatus = faculty.WorkingStatus;
                    regFaculty.TotalExperience = faculty.TotalExperience;
                    regFaculty.OrganizationName = faculty.OrganizationName;
                    regFaculty.CollegeId = db.jntuh_college_faculty_registered.Where(f => f.RegistrationNumber == regFaculty.RegistrationNumber).Select(s => s.collegeId).FirstOrDefault();
                    if (regFaculty.CollegeId != 0)
                    {
                        regFaculty.CollegeName = db.jntuh_college.Find(regFaculty.CollegeId).collegeName;
                    }

                    if (faculty.DepartmentId != null)
                    {
                        regFaculty.department = db.jntuh_department.Find(faculty.DepartmentId).departmentName;
                    }
                    regFaculty.DepartmentId = faculty.DepartmentId;
                    regFaculty.OtherDepartment = faculty.OtherDepartment;

                    if (faculty.DesignationId != null)
                    {
                        regFaculty.designation = db.jntuh_designation.Find(faculty.DesignationId).designation;
                    }
                    regFaculty.DesignationId = faculty.DesignationId;
                    regFaculty.OtherDesignation = faculty.OtherDesignation;

                    if (faculty.DateOfAppointment != null)
                    {
                        regFaculty.facultyDateOfAppointment =
                            UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.DateOfAppointment.ToString());
                    }
                    regFaculty.TotalExperiencePresentCollege = faculty.TotalExperiencePresentCollege;
                    regFaculty.isFacultyRatifiedByJNTU = faculty.isFacultyRatifiedByJNTU;
                    if (faculty.DateOfRatification != null)
                    {
                        regFaculty.facultyDateOfRatification =
                            UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.DateOfRatification.ToString());
                    }
                    regFaculty.ProceedingsNo = faculty.ProceedingsNumber;
                    regFaculty.SelectionCommitteeProcedings = faculty.ProceedingDocument;
                    regFaculty.AICTEFacultyId = faculty.AICTEFacultyId;
                    regFaculty.GrossSalary = faculty.grosssalary;
                    regFaculty.National = faculty.National;
                    regFaculty.InterNational = faculty.InterNational;
                    regFaculty.Citation = faculty.Citation;
                    regFaculty.Awards = faculty.Awards;
                    regFaculty.isActive = faculty.isActive;
                    regFaculty.isApproved = faculty.isApproved;
                    regFaculty.isView = true;
                    regFaculty.DeactivationReason = faculty.DeactivationReason;


                    regFaculty.FacultyEducation = db.jntuh_education_category.Where(e => e.isActive == true && (e.id == 1 || e.id == 3 || e.id == 4 || e.id == 5 || e.id == 6))
                            .Select(e => new RegisteredFacultyEducation
                            {
                                educationId = e.id,
                                educationName = e.educationCategoryName,
                                studiedEducation = db.jntuh_registered_faculty_education.Where(fe => fe.educationId == e.id && fe.facultyId == fID).Select(fe => fe.courseStudied).FirstOrDefault(),
                                specialization = db.jntuh_registered_faculty_education.Where(fe => fe.educationId == e.id && fe.facultyId == fID).Select(fe => fe.specialization).FirstOrDefault(),
                                passedYear = db.jntuh_registered_faculty_education.Where(fe => fe.educationId == e.id && fe.facultyId == fID).Select(fe => fe.passedYear).FirstOrDefault(),
                                percentage = db.jntuh_registered_faculty_education.Where(fe => fe.educationId == e.id && fe.facultyId == fID).Select(fe => fe.marksPercentage).FirstOrDefault(),
                                division = db.jntuh_registered_faculty_education.Where(fe => fe.educationId == e.id && fe.facultyId == fID).Select(fe => fe.division).FirstOrDefault(),
                                university = db.jntuh_registered_faculty_education.Where(fe => fe.educationId == e.id && fe.facultyId == fID).Select(fe => fe.boardOrUniversity).FirstOrDefault(),
                                place = db.jntuh_registered_faculty_education.Where(fe => fe.educationId == e.id && fe.facultyId == fID).Select(fe => fe.placeOfEducation).FirstOrDefault(),
                                facultyCertificate = db.jntuh_registered_faculty_education.Where(fe => fe.educationId == e.id && fe.facultyId == fID).Select(fe => fe.certificate).FirstOrDefault(),
                            }).ToList();

                    foreach (var item in regFaculty.FacultyEducation)
                    {
                        if (item.division == null)
                            item.division = 0;
                    }

                    string registrationNumber =
                        db.jntuh_registered_faculty.Where(of => of.id == fID)
                            .Select(of => of.RegistrationNumber)
                            .FirstOrDefault();
                    int facultyId =
                        db.jntuh_college_faculty_registered.Where(of => of.RegistrationNumber == registrationNumber)
                            .Select(of => of.id)
                            .FirstOrDefault();
                    //int[] verificationOfficers =
                    //    db.jntuh_college_faculty_verified.Where(v => v.FacultyId == facultyId)
                    //        .Select(v => v.VerificationOfficer)
                    //        .Distinct()
                    //        .ToArray();
                    int userId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);

                    //bool isValid = ShowHideLink(fID);

                    //ViewBag.HideVerifyLink = isValid;

                    //if (verificationOfficers.Contains(userId))
                    //{
                    //    if (isValid)
                    //    {
                    //        ViewBag.HideVerifyLink = true;
                    //    }
                    //    else
                    //    {
                    //        ViewBag.HideVerifyLink = false;
                    //    }
                    //}

                    //if (verificationOfficers.Count() == 3)
                    //{
                    //    ViewBag.HideVerifyLink = true;
                    //}

                    ViewBag.HideVerifyLink = regFaculty.isApproved != null ? true : false;
                    return View(regFaculty);
                }
                else
                {

                    return RedirectToAction("CollegeScmProceedingsRequest", "CollegeSCMProceedingsRequest");
                }
            }
            else
            {
                return RedirectToAction("CollegeScmProceedingsRequest", "CollegeSCMProceedingsRequest");
            }
        }

        [Authorize(Roles = "College")]
        public ActionResult DeletePrincipalSCMReg(string AddfacultyId)
        {
            MembershipUser userdata = Membership.GetUser(User.Identity.Name);
            if (userdata == null)
            {
                return RedirectToAction("LogOn", "Account");
            }
            int userId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            if (AddfacultyId != null)
            {
                var Id = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(AddfacultyId, System.Web.Configuration.WebConfigurationManager.AppSettings["CryptoKey"]));
                var jntuh_scmproceedingrequest_addfaculty = db.jntuh_scmproceedingrequest_addfaculty.Where(s => s.Id == Id).Select(a => a).FirstOrDefault();
                if (jntuh_scmproceedingrequest_addfaculty != null)
                {
                    db.jntuh_scmproceedingrequest_addfaculty.Remove(jntuh_scmproceedingrequest_addfaculty);
                    db.SaveChanges();
                    TempData["Success"] = "Registration Number Deleted Successfully.";
                }
            }
            return RedirectToAction("CollegeScmProceedingsPrincipalRequest");
        }

        //CollegeScmProceedingsRequestNomineeAssignedview
        [Authorize(Roles = "Admin,College")]
        public ActionResult CollegeScmProceedingsRequestNomineeAssignedview()
        {
            return RedirectToAction("College", "Dashboard");
            if (Membership.GetUser() == null)
            {
                return RedirectToAction("LogOn", "Account");
            }
            var userId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            var userCollegeId = db.jntuh_college_users.Where(u => u.userID == userId).Select(u => u.collegeID).FirstOrDefault();
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int ay0 = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();
            int lid =
                db.jntuh_link_screens.Where(p => p.linkCode == "FSCM" && p.isActive == true).Select(s => s.id).FirstOrDefault();
            jntuh_college_links_assigned scmphase =
                db.jntuh_college_links_assigned.Where(l => l.linkId == lid && l.academicyearId == ay0 && l.isActive == true && l.isAssigned == true).Select(s => s).FirstOrDefault();

            DateTime scmstartdate = Convert.ToDateTime(scmphase.fromdate);
            if (userCollegeId == 375)
            {
                userCollegeId = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            if (userCollegeId != 0)
            {
                var nomineeAssignedScmIds = (from SCM in db.jntuh_scmproceedingsrequests
                                             join AUDA in db.jntuh_auditor_assigned on SCM.ID equals AUDA.ScmId
                                             where
                                                 SCM.CollegeId == userCollegeId && SCM.SpecializationId != 0 && SCM.DEpartmentId != 0 &&
                                                 SCM.DegreeId != 0 && SCM.CreatedOn >= scmstartdate && SCM.isscmatUniversity == false
                                             select SCM.ID).Distinct().ToArray();
                var nomineeassignRequest = (from SCM in db.jntuh_scmproceedingsrequests
                                            join AUDA in db.jntuh_auditor_assigned on SCM.ID equals AUDA.ScmId
                                            join Nominee in db.jntuh_ffc_auditor on AUDA.AuditorId equals Nominee.id
                                            join SPEC in db.jntuh_specialization on SCM.SpecializationId equals SPEC.id
                                            join DEPT in db.jntuh_department on SCM.DEpartmentId equals DEPT.id
                                            join DEG in db.jntuh_degree on SCM.DegreeId equals DEG.id
                                            join CLG in db.jntuh_college on SCM.CollegeId equals CLG.id
                                            where nomineeAssignedScmIds.Contains(SCM.ID) && SCM.isscmatUniversity == false
                                            select new NomineeAssignSCMRequests
                                            {
                                                SCMId = SCM.ID,
                                                CollegeCode = CLG.collegeCode,
                                                CollegeName = CLG.collegeName,
                                                //Department = DEG.degree + " " + SPEC.specializationName,
                                                Department = DEPT.departmentDescription,
                                                AuditorEmail = Nominee.auditorEmail1,
                                                AuditorMobile = Nominee.auditorMobile1,
                                                AuditorName = Nominee.auditorName,
                                                AuditorAssignDate = AUDA.CreatedOn,
                                                ScmRequestDate = (DateTime)SCM.RequestSubmittedDate,
                                            }).OrderByDescending(e => e.AuditorAssignDate).ToList();

                return View(nomineeassignRequest);
            }
            return View();
        }

        //Proffeser and Associate Professor Inactive SCM Request Faculty on 20-08-2018
        [Authorize(Roles = "Admin,College")]
        public ActionResult CollegeSCMRequestFaculty()
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            var jntuh_designation = db.jntuh_designation.AsNoTracking().ToList();
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            DateTime scmstartdate = new DateTime(2018, 08, 19);
            List<ScmProceedingsRequestAddReg> ScmProceedingsRequestAddReglist = new List<ScmProceedingsRequestAddReg>();

            if (userCollegeID == null || userCollegeID == 0)
            {
                return RedirectToAction("College", "Dashboard");
            }
            else
            {
                //var scmdata =
                //    db.jntuh_scmproceedingsrequests.Where(s => s.CollegeId == userCollegeID && s.CreatedOn >= scmstartdate)
                //        .Select(s => s)
                //        .ToList();


                //foreach (var scm in scmdata)
                //{
                //    var departmetname =
                //        db.jntuh_department.Where(d => d.id == scm.DEpartmentId)
                //            .Select(s => s.departmentDescription)
                //            .FirstOrDefault();
                //    List<jntuh_scmproceedingrequest_addfaculty> jntuh_scmproceedingrequest_addfaculty =
                //    db.jntuh_scmproceedingrequest_addfaculty.Where(
                //        a => a.ScmProceedingId == scm.ID && a.FacultyType != 3 && a.IsApproved == false).Select(s => s).ToList();
                //    foreach (var item in jntuh_scmproceedingrequest_addfaculty)
                //    {
                //        ScmProceedingsRequestAddReg newreg = new ScmProceedingsRequestAddReg();
                //        newreg.RegistrationNo = item.RegistrationNumber.Trim();
                //        var regdata =
                //            db.jntuh_registered_faculty.Where(r => r.RegistrationNumber == item.RegistrationNumber)
                //                .Select(s => s)
                //                .FirstOrDefault();
                //        newreg.Name = regdata.FirstName+" " + regdata.MiddleName +" "+ regdata.LastName;
                //        newreg.DepartmentName = departmetname;
                //        newreg.Designation = jntuh_designation.Where(d=>d.id==item.FacultyType).Select(s=>s.designation).FirstOrDefault();
                //        newreg.CollegeId = (int)item.CollegeId;
                //        newreg.ExperianceDocumentView = item.FacultyExperianceDocument;
                //        newreg.ScmId = item.ScmProceedingId;
                //        newreg.DeactiviationReason = item.DeactiviationReason.Trim();
                //        newreg.IsApprove = item.IsApproved;
                //        newreg.IsActive = item.Isactive;
                //        newreg.FacultyId = item.Id;
                //        newreg.FacultyType = (int)item.FacultyType;
                //        ScmProceedingsRequestAddReglist.Add(newreg);
                //    }

                //}
                ScmProceedingsRequestAddReglist = (from scrq in db.jntuh_scmproceedingsrequests
                                                   join adfa in db.jntuh_scmproceedingrequest_addfaculty on scrq.ID equals adfa.ScmProceedingId
                                                   join regdata in db.jntuh_registered_faculty on adfa.RegistrationNumber.Trim() equals regdata.RegistrationNumber.Trim()
                                                   join dpt in db.jntuh_department on scrq.DEpartmentId equals dpt.id
                                                   join des in db.jntuh_designation on adfa.FacultyType equals des.id
                                                   where
                                                       scrq.CreatedOn >= scmstartdate && adfa.FacultyType != 3 && adfa.IsApproved == false &&
                                                       scrq.CollegeId == userCollegeID
                                                   select new ScmProceedingsRequestAddReg
                                                   {
                                                       RegistrationNo = adfa.RegistrationNumber,
                                                       Firstname = regdata.FirstName,
                                                       Middlename = regdata.MiddleName,
                                                       Lastname = regdata.LastName,
                                                       DepartmentName = dpt.departmentDescription,
                                                       Designation = des.designation,
                                                       CollegeId = scrq.CollegeId,
                                                       ExperianceDocumentView = adfa.FacultyExperianceDocument,
                                                       ScmId = adfa.ScmProceedingId,
                                                       DeactiviationReason = adfa.DeactiviationReason,
                                                       IsApprove = adfa.IsApproved,
                                                       IsActive = adfa.Isactive,
                                                       FacultyId = adfa.Id,
                                                       FacultyType = (int)adfa.FacultyType

                                                   }).ToList();
            }
            //ScmProceedingsRequestAddReg
            return View(ScmProceedingsRequestAddReglist.OrderBy(d => d.DepartmentName).ToList());
        }

        //SCM Faculty Experiance Document Edit
        [Authorize(Roles = "Admin,College")]
        public ActionResult FacultyExperianceDocumentEdit(string regno, string scmId, string facultyType, string Id, string deptName, string expdoc)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int Collegeid = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            ScmProceedingsRequestAddReg facultydata = new ScmProceedingsRequestAddReg();
            int fId = 0;
            if (regno != null && Id != null)
            {
                facultydata.RegistrationNo = UAAAS.Models.Utilities.DecryptString(regno,
                    System.Web.Configuration.WebConfigurationManager.AppSettings["CryptoKey"]);
                facultydata.FacultyId = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(Id,
                    System.Web.Configuration.WebConfigurationManager.AppSettings["CryptoKey"]));
                facultydata.FacultyType = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(facultyType,
                    System.Web.Configuration.WebConfigurationManager.AppSettings["CryptoKey"]));
                facultydata.ScmId = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(scmId,
                    System.Web.Configuration.WebConfigurationManager.AppSettings["CryptoKey"]));
                facultydata.DepartmentName = deptName;
                facultydata.ExperianceDocumentView = expdoc;
            }
            //jntuh_scmproceedingrequest_addfaculty addfaculty =
            //    db.jntuh_scmproceedingrequest_addfaculty.Where(a => a.Id == fId).Select(s => s).FirstOrDefault();
            //if ()
            //{

            //}
            return PartialView("FacultyExperianceDocumentEdit", facultydata);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,College")]
        public ActionResult FacultyExperianceDocumentEdit(ScmProceedingsRequestAddReg editexpdocument)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            ScmProceedingsRequestAddReg facultydata = new ScmProceedingsRequestAddReg();
            int fId = 0;
            const string facultyexperiancepath = "~/Content/Upload/SCMPROCEEDINGSREQUEST/FacultyExperianceDocuments";
            //if (editexpdocument.FacultyType == 1 || editexpdocument.FacultyType == 2)
            //{
            if (editexpdocument.ExperianceDocument != null)
            {
                if (!Directory.Exists(Server.MapPath(facultyexperiancepath)))
                {
                    Directory.CreateDirectory(Server.MapPath(facultyexperiancepath));
                }

                var ext = Path.GetExtension(editexpdocument.ExperianceDocument.FileName);
                if (ext != null && ext.ToUpper().Equals(".PDF"))
                {
                    var ExpfileName = db.jntuh_college.Where(c => c.id == userCollegeID).Select(c => c.collegeCode).FirstOrDefault() + "_" + "FacultyExperiance" + "_" + DateTime.Now.ToString("yyyMMddHHmmss");
                    editexpdocument.ExperianceDocument.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(facultyexperiancepath), ExpfileName, ext));
                    editexpdocument.ExperianceDocumentView = ExpfileName + ext;
                    jntuh_scmproceedingrequest_addfaculty updatescmaddfaculty =
                        db.jntuh_scmproceedingrequest_addfaculty.Where(
                            s =>
                                s.RegistrationNumber.Trim() == editexpdocument.RegistrationNo.Trim() &&
                                s.Id == editexpdocument.FacultyId)
                            .FirstOrDefault();
                    if (updatescmaddfaculty != null)
                    {
                        updatescmaddfaculty.FacultyExperianceDocument = editexpdocument.ExperianceDocumentView;
                        updatescmaddfaculty.UpdatedOn = DateTime.Now;
                        updatescmaddfaculty.Updatedby = userID;
                        db.Entry(updatescmaddfaculty).State = EntityState.Modified;
                        db.SaveChanges();
                        TempData["Success"] = "Faculty Experiance Update Successfully....";
                    }
                    else
                    {
                        TempData["Error1"] = "Faculty Experiance Update Failed.";
                        return RedirectToAction("CollegeSCMRequestFaculty", "CollegeSCMProceedingsRequest");
                    }
                }
                else
                {
                    TempData["Error1"] = "Faculty Experiance Document can upload only PDF.";
                    return RedirectToAction("CollegeSCMRequestFaculty", "CollegeSCMProceedingsRequest");
                }
            }
            else
            {
                TempData["Error1"] = "Faculty Experiance Document Requried.";
                return RedirectToAction("CollegeSCMRequestFaculty", "CollegeSCMProceedingsRequest");
            }

            //}
            return RedirectToAction("CollegeSCMRequestFaculty", "CollegeSCMProceedingsRequest");
        }

        #region SCM Fee Calculation

        public ActionResult scmfeeDetails()
        {
            //Note: 9 Means SCM Payment ID it is in Payment Master
            if (Membership.GetUser() == null)
            {
                return RedirectToAction("LogOn", "Account");
            }
            DateTime todayDate = DateTime.Now;
            var userId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            var userCollegeId = db.jntuh_college_users.Where(u => u.userID == userId).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeId == 375)
            {
                userCollegeId = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int ay0 = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();
            int lid =
                db.jntuh_link_screens.Where(p => p.linkName == "Faculty SCM Request" && p.isActive == true).Select(s => s.id).FirstOrDefault();
            jntuh_college_links_assigned scmphase =
                db.jntuh_college_links_assigned.Where(l => l.linkId == lid && l.academicyearId == ay0 && l.isActive == true && l.fromdate <= todayDate && l.todate >= todayDate).Select(s => s).FirstOrDefault();
            ViewBag.IsEditable = false;
            if (scmphase == null && Roles.IsUserInRole("College"))
            {

                return RedirectToAction("College", "Dashboard");
            }
            else
            {
                ViewBag.IsEditable = true;
            }

            List<jntuh_scmproceedingsrequests> scmrequests =
                db.jntuh_scmproceedingsrequests.Where(
                    r =>
                        r.CollegeId == userCollegeId && r.phaseId == scmphase.phaseId && r.CreatedOn >= scmphase.fromdate && r.DEpartmentId != 0 && r.academicyearId == ay0 &&
                         r.isscmatUniversity == true).Select(s => s).ToList();
            var requesteddepartments = scmrequests.Select(s => s.DEpartmentId).GroupBy(d => d).ToList();
            //var requesteddepartments = depts.GroupBy().t;    r.RequestSubmittedDate == null &&
            List<scmpaymentsdetals> paymentdetailslist = new List<scmpaymentsdetals>();
            long totalamount = 0;
            foreach (var departments in requesteddepartments)
            {
                long scmamount = 0;
                long RequestsubmittedAmount = 0;
                long RequestnotsubmittedAmount = 0;
                int did = Convert.ToInt32(departments.Key);

                //var scmids = scmrequests.Where(r => r.DEpartmentId == did && r.RequestSubmittedDate != null).Select(s => s.ID).ToArray();
                //List<jntuh_scmproceedingrequest_addfaculty> addedfaculty =
                //    db.jntuh_scmproceedingrequest_addfaculty.Where(a => scmids.Contains(a.ScmProceedingId))
                //        .Select(s => s)
                //        .ToList();
                //scmpaymentsdetals paymentdetails = new scmpaymentsdetals();
                //paymentdetails.departmentId = did;
                //paymentdetails.department =
                //    db.jntuh_department.Where(d => d.id == did).Select(s => s.departmentDescription).FirstOrDefault();
                //paymentdetails.addedfacultycount = addedfaculty.Count();
                //int facultycount = paymentdetails.addedfacultycount;
                //while (facultycount > 0)
                //{
                //    scmamount = scmamount + 11000;
                //    facultycount = facultycount - 10;
                //}
                //paymentdetails.amount = scmamount;
                #region This Code is Commented by Narayana Reddy due to don't take Extra Amount for the SCM New Request before Payed amount less than or equal to need to Pay Amount.
                //Request Submitted Amount
                var scmids = scmrequests.Where(r => r.DEpartmentId == did && r.RequestSubmittedDate != null).Select(s => s.ID).ToArray();
                List<jntuh_scmproceedingrequest_addfaculty> addedfaculty =
                    db.jntuh_scmproceedingrequest_addfaculty.Where(a => scmids.Contains(a.ScmProceedingId))
                        .Select(s => s)
                        .ToList();
                scmpaymentsdetals paymentdetails = new scmpaymentsdetals();
                paymentdetails.department =
                    db.jntuh_department.Where(d => d.id == did).Select(s => s.departmentDescription).FirstOrDefault();
                paymentdetails.addedfacultycount = addedfaculty.Count();
                int facultycount = paymentdetails.addedfacultycount;
                if (facultycount == 0)
                {
                    paymentdetails.IssubmissionStatus = true;
                }
                while (facultycount > 0)
                {
                    //RequestsubmittedAmount = RequestsubmittedAmount + 11000; // for assistant professor  
                    //facultycount = facultycount - 10; // for assistant professor  
                    RequestsubmittedAmount = RequestsubmittedAmount + 22000; // for professor  
                    facultycount = facultycount - 10; // for professor 
                    paymentdetails.IssubmissionStatus = scmrequests.Where(r => r.DEpartmentId == did && r.RequestSubmittedDate != null).Select(s => s.ID).SingleOrDefault() != 0 ? true : false;
                    paymentdetails.amount = RequestsubmittedAmount;
                }

                //Request Not Sumbitted Amount            
                var notsubscmids = scmrequests.Where(r => r.DEpartmentId == did && r.RequestSubmittedDate == null).Select(s => s.ID).ToArray();
                List<jntuh_scmproceedingrequest_addfaculty> reqnotsubaddedfaculty =
                    db.jntuh_scmproceedingrequest_addfaculty.Where(a => notsubscmids.Contains(a.ScmProceedingId))
                        .Select(s => s)
                        .ToList();
                paymentdetails.department =
                    db.jntuh_department.Where(d => d.id == did).Select(s => s.departmentDescription).FirstOrDefault();
                paymentdetails.addedfacultycount = reqnotsubaddedfaculty.Count();
                int notsubfacultycount = paymentdetails.addedfacultycount;
                if (notsubfacultycount == 0)
                {
                    paymentdetails.IssubmissionStatus = true;
                }
                while (notsubfacultycount > 0)
                {
                    //RequestnotsubmittedAmount = RequestnotsubmittedAmount + 11000; // for assistant professor 
                    //notsubfacultycount = notsubfacultycount - 10; // for assistant professor 
                    RequestnotsubmittedAmount = RequestnotsubmittedAmount + 22000; // for  professor 
                    notsubfacultycount = notsubfacultycount - 10; // for professor 
                    paymentdetails.IssubmissionStatus = false;
                    paymentdetails.amount = RequestnotsubmittedAmount;
                }
                totalamount = totalamount + RequestsubmittedAmount + RequestnotsubmittedAmount;
                #endregion

                //totalamount = totalamount + scmamount;
                //totalamount = totalamount + RequestsubmittedAmount + RequestnotsubmittedAmount;
                //paymentdetails.IssubmissionStatus = scmrequests.Where(r => r.DEpartmentId == did && r.RequestSubmittedDate != null).Select(s => s.ID).SingleOrDefault() != 0 ? true :false;
                paymentdetailslist.Add(paymentdetails);
            }
            string clgCode = db.jntuh_college.SingleOrDefault(a => a.id == userCollegeId).collegeCode;

            ViewBag.collegeCode = clgCode;
            ViewBag.challnNumber = clgCode + DateTime.Now.ToString("yyyMMddHHmmss");
            var currentYear = DateTime.Now.Year;
            //var professorSCMDate = Convert.ToDateTime(scmphase.fromdate).Date;//Convert.ToDateTime("2023-06-30").Date;
            var payments = db.jntuh_paymentresponse.Where(it => it.CollegeId == clgCode && it.AcademicYearId == ay0 && it.AuthStatus == "0300" && it.TxnDate.Year == currentYear && it.PaymentTypeID == 9 && it.TxnDate >= scmphase.fromdate)
                    .Select(s => s.TxnAmount).ToList().Count() > 0 ?
                db.jntuh_paymentresponse.Where(it => it.CollegeId == clgCode && it.AcademicYearId == ay0 && it.AuthStatus == "0300" && it.TxnDate.Year == currentYear && it.PaymentTypeID == 9 && it.TxnDate >= scmphase.fromdate)
                .Select(s => s.TxnAmount).Sum() : 0;
            var paymenthistory =
                db.jntuh_paymentresponse.Where(
                    it =>
                        it.CollegeId == clgCode && it.AcademicYearId == ay0 && it.AuthStatus == "0300" &&
                        it.TxnDate.Year == currentYear && it.PaymentTypeID == 9 && it.TxnDate >= scmphase.fromdate)
                    .Select(s => s).ToList();
            ViewBag.Payments = paymenthistory;
            ViewBag.totalFee = totalamount;
            decimal remaingamount = totalamount - payments;
            ViewBag.IsPaymentDone = false;
            //if (payments >= totalamount)
            //{
            //    ViewBag.IsPaymentDone = true;
            //    ViewBag.totalFee = payments;
            //}
            if (remaingamount <= 0)
            {
                ViewBag.IsPaymentDone = true;
                ViewBag.totalFee = totalamount;

                //New Code If the Payment is Zero Respective College SCM Requests Submitted with the Departments
                //var departmentids = paymentdetailslist.Select(d => d.departmentId).GroupBy(d => d).ToArray();
                //foreach (var department in departmentids)
                //{
                //    int did = Convert.ToInt32(department.Key);
                //    var paymentid= db.jntuh_scmproceedingsrequests.Where(
                // r =>
                //     r.CollegeId == userCollegeId && r.phaseId == scmphase.phaseId && r.CreatedOn >= scmphase.fromdate && r.DEpartmentId != 0 && r.academicyearId == ay0 &&
                //     r.RequestSubmittedDate != null&&r.PaymentTypeId!=null && r.isscmatUniversity == true).Select(s => s.PaymentTypeId).FirstOrDefault();
                //    if (paymentid!=null)
                //    {
                //        List<jntuh_scmproceedingsrequests> scmrequestsupdate =
                // db.jntuh_scmproceedingsrequests.Where(
                // r =>
                //     r.CollegeId == userCollegeId&&r.DEpartmentId==did && r.phaseId == scmphase.phaseId && r.CreatedOn >= scmphase.fromdate && r.DEpartmentId != 0 && r.academicyearId == ay0 &&
                //     r.RequestSubmittedDate == null && r.isscmatUniversity == true).Select(s => s).ToList();
                //foreach (var scmrequest in scmrequestsupdate)
                //{
                //    scmrequest.RequestSubmittedDate = DateTime.Now;
                //    scmrequest.PaymentTypeId = paymentid;
                //    db.Entry(scmrequest).State = EntityState.Modified;
                //    db.SaveChanges();
                //}
                //    }
                //}
                //TempData["Success"] = "You No Need to Pay Amount.Your Request Submitted Successfully.";
                //return RedirectToAction("CollegeScmProceedingsRequest");
            }
            else
            {
                ViewBag.totalFee = remaingamount;
            }
            var returnUrl = WebConfigurationManager.AppSettings["SCMReturnUrl"];
            var merchantId = WebConfigurationManager.AppSettings["MerchantID"];
            var securityId = WebConfigurationManager.AppSettings["SecurityID"];
            var typefield1 = WebConfigurationManager.AppSettings["TypeField1"];
            var typefield2 = WebConfigurationManager.AppSettings["TypeField2"];
            var msg = "";
            if (userCollegeId == 375)
            {
                msg = merchantId + "|" + ViewBag.challnNumber + "|NA|2.00|NA|NA|NA|INR|NA|" + typefield1 + "|" +
                      securityId + "|NA|NA|" + typefield2 + "|NA|NA|NA|NA|NA|NA|NA|" + returnUrl;
                var key = WebConfigurationManager.AppSettings["ChecksumKey"];
                msg += "|" + GetHMACSHA256(msg, key).ToUpper();
            }
            else
            {
                msg = merchantId + "|" + ViewBag.challnNumber + "|NA|" + ViewBag.totalFee + "|NA|NA|NA|INR|NA|" +
                      typefield1 + "|" + securityId + "|NA|NA|" + typefield2 + "|NA|NA|NA|NA|NA|NA|NA|" +
                      returnUrl;
                var key = WebConfigurationManager.AppSettings["ChecksumKey"];
                msg += "|" + GetHMACSHA256(msg, key).ToUpper();
            }

            ViewBag.msg = msg;

            return View(paymentdetailslist);
        }

        [HttpPost]
        public ActionResult scmfeeDetails(string msg)
        {
            //if (Membership.GetUser() == null)
            //{
            //    return RedirectToAction("LogOn", "Account");
            //}
            var appSettings = WebConfigurationManager.AppSettings;
            var req = new jntuh_paymentrequests();
            SavePaymentResponse(msg, "ChallanNumber");
            return RedirectToAction("CollegeScmProceedingsRequest");
        }

        private void SavePaymentResponse(string responseMsg, string challanno)
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
                userID =
                    db.jntuh_college_users.Where(u => u.collegeID == userCollegeID)
                        .Select(u => u.userID)
                        .FirstOrDefault();
            }
            else
            {
                userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
                userCollegeID =
                    db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
                clgCode = db.jntuh_college.SingleOrDefault(a => a.id == userCollegeID).collegeCode;
            }
            DateTime todayDate = DateTime.Now;
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int ay0 =
               db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
            int lid =
                db.jntuh_link_screens.Where(p => p.linkName == "Faculty SCM Request" && p.isActive == true).Select(s => s.id).FirstOrDefault();
            jntuh_college_links_assigned scmphase =
                db.jntuh_college_links_assigned.Where(l => l.linkId == lid && l.academicyearId == ay0 && l.isActive == true && l.fromdate <= todayDate && l.todate >= todayDate).Select(s => s).FirstOrDefault();

            //Response Message Saving
            temp_aeronautical temp_aeronautical = new temp_aeronautical();
            temp_aeronautical.Degree = responseMsg.Length < 255
                ? responseMsg
                : responseMsg.Substring(0, 254);
            ;
            temp_aeronautical.Department = "SCM fee";
            temp_aeronautical.Specialization = clgCode;
            temp_aeronautical.DegreeId = userCollegeID;
            temp_aeronautical.LabCode = DateTime.Now.ToString();
            db.temp_aeronautical.Add(temp_aeronautical);
            db.SaveChanges();


            var dbResponse = new UAAAS.Models.jntuh_paymentresponse();
            dbResponse.AcademicYearId = ay0;
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
            dbResponse.PaymentTypeID = 9;
            db.jntuh_paymentresponse.Add(dbResponse);
            db.SaveChanges();
            //var professorSCMDate = Convert.ToDateTime(scmphase.fromdate).Date;//Convert.ToDateTime("2023-06-30").Date;
            //Payment Done Details
            var paymentdonedetails =
                db.jntuh_paymentresponse.Where(
                    r =>
                        r.CollegeId == dbResponse.CollegeId && r.AcademicYearId == ay0 && r.AuthStatus == "0300" &&
                        r.PaymentTypeID == 9 && r.TxnReferenceNo == dbResponse.TxnReferenceNo &&
                        r.CustomerID == dbResponse.CustomerID && r.BankReferenceNo == dbResponse.BankReferenceNo && r.TxnDate >= scmphase.fromdate)
                    .FirstOrDefault();


            #region Check Payment is Match or Not
            List<jntuh_scmproceedingsrequests> scmrequests =
             db.jntuh_scmproceedingsrequests.Where(
                 r =>
                     r.CollegeId == userCollegeID && r.phaseId == scmphase.phaseId && r.CreatedOn >= scmphase.fromdate && r.DEpartmentId != 0 && r.academicyearId == ay0 &&
                     r.RequestSubmittedDate == null && r.isscmatUniversity == true).Select(s => s).ToList();
            var requesteddepartments = scmrequests.Select(s => s.DEpartmentId).GroupBy(d => d).ToList();
            //var requesteddepartments = depts.GroupBy().t;
            List<scmpaymentsdetals> paymentdetailslist = new List<scmpaymentsdetals>();
            long totalamount = 0;
            foreach (var departments in requesteddepartments)
            {
                long scmamount = 0;
                long RequestsubmittedAmount = 0;
                long RequestnotsubmittedAmount = 0;
                int did = Convert.ToInt32(departments.Key);
                //var scmids = scmrequests.Where(r => r.DEpartmentId == did).Select(s => s.ID).ToArray();
                //List<jntuh_scmproceedingrequest_addfaculty> addedfaculty =
                //    db.jntuh_scmproceedingrequest_addfaculty.Where(a => scmids.Contains(a.ScmProceedingId))
                //        .Select(s => s)
                //        .ToList();
                //scmpaymentsdetals paymentdetails = new scmpaymentsdetals();
                //paymentdetails.department =
                //    db.jntuh_department.Where(d => d.id == did).Select(s => s.departmentDescription).FirstOrDefault();
                //paymentdetails.addedfacultycount = addedfaculty.Count();
                //int facultycount = paymentdetails.addedfacultycount;
                //while (facultycount > 0)
                //{
                //    scmamount = scmamount + 11000;
                //    facultycount = facultycount - 10;
                //}
                //paymentdetails.amount = scmamount;
                //totalamount = totalamount + scmamount;
                //Request Submitted Amount
                var scmids = scmrequests.Where(r => r.DEpartmentId == did && r.RequestSubmittedDate != null).Select(s => s.ID).ToArray();
                List<jntuh_scmproceedingrequest_addfaculty> addedfaculty =
                    db.jntuh_scmproceedingrequest_addfaculty.Where(a => scmids.Contains(a.ScmProceedingId))
                        .Select(s => s)
                        .ToList();
                scmpaymentsdetals paymentdetails = new scmpaymentsdetals();
                paymentdetails.department =
                    db.jntuh_department.Where(d => d.id == did).Select(s => s.departmentDescription).FirstOrDefault();
                paymentdetails.addedfacultycount = addedfaculty.Count();
                int facultycount = paymentdetails.addedfacultycount;
                while (facultycount > 0)
                {
                    //RequestsubmittedAmount = RequestsubmittedAmount + 11000; // for assistant professor  
                    //facultycount = facultycount - 10; // for assistant professor  
                    RequestsubmittedAmount = RequestsubmittedAmount + 22000; // for professor  
                    facultycount = facultycount - 10; // for professor  
                    paymentdetails.IssubmissionStatus = scmrequests.Where(r => r.DEpartmentId == did && r.RequestSubmittedDate != null).Select(s => s.ID).SingleOrDefault() != 0 ? true : false;
                    paymentdetails.amount = RequestsubmittedAmount;
                }

                //Request Not Sumbitted Amount

                var notsubscmids = scmrequests.Where(r => r.DEpartmentId == did && r.RequestSubmittedDate == null).Select(s => s.ID).ToArray();
                List<jntuh_scmproceedingrequest_addfaculty> reqnotsubaddedfaculty =
                    db.jntuh_scmproceedingrequest_addfaculty.Where(a => notsubscmids.Contains(a.ScmProceedingId))
                        .Select(s => s)
                        .ToList();
                //scmpaymentsdetals paymentdetails = new scmpaymentsdetals();
                paymentdetails.department =
                    db.jntuh_department.Where(d => d.id == did).Select(s => s.departmentDescription).FirstOrDefault();
                paymentdetails.addedfacultycount = reqnotsubaddedfaculty.Count();
                int notsubfacultycount = paymentdetails.addedfacultycount;
                while (notsubfacultycount > 0)
                {
                    //RequestnotsubmittedAmount = RequestnotsubmittedAmount + 11000; // for assistant professor  
                    //notsubfacultycount = notsubfacultycount - 10; // for assistant professor  
                    RequestnotsubmittedAmount = RequestnotsubmittedAmount + 22000; // for professor  
                    notsubfacultycount = notsubfacultycount - 10; // for professor  
                    paymentdetails.IssubmissionStatus = false;
                    paymentdetails.amount = RequestnotsubmittedAmount;
                }

                //paymentdetails.amount = scmamount;
                totalamount = totalamount + RequestsubmittedAmount + RequestnotsubmittedAmount;

                paymentdetailslist.Add(paymentdetails);
            }
            string clgCode1 = db.jntuh_college.SingleOrDefault(a => a.id == userCollegeID).collegeCode;

            ViewBag.collegeCode = clgCode;
            ViewBag.challnNumber = clgCode + DateTime.Now.ToString("yyyMMddHHmmss");
            var currentYear = DateTime.Now.Year;
            var payments = db.jntuh_paymentresponse.Where(it => it.CollegeId == clgCode && it.AcademicYearId == ay0 && it.AuthStatus == "0300" && it.TxnDate.Year == currentYear && it.PaymentTypeID == 9 && it.TxnDate >= scmphase.fromdate)
                    .Select(s => s.TxnAmount).ToList().Count() > 0 ?
                db.jntuh_paymentresponse.Where(it => it.CollegeId == clgCode && it.AcademicYearId == ay0 && it.AuthStatus == "0300" && it.TxnDate.Year == currentYear && it.PaymentTypeID == 9 && it.TxnDate >= scmphase.fromdate)
                .Select(s => s.TxnAmount).Sum() : 0;
            var paymenthistory =
                db.jntuh_paymentresponse.Where(
                    it =>
                        it.CollegeId == clgCode && it.AcademicYearId == ay0 && it.AuthStatus == "0300" &&
                        it.TxnDate.Year == currentYear && it.PaymentTypeID == 9 && it.TxnDate >= scmphase.fromdate)
                    .Select(s => s).ToList();
            ViewBag.Payments = paymenthistory;
            ViewBag.totalFee = totalamount;
            decimal remaingamount = totalamount - payments;
            #endregion

            if (paymentdonedetails != null && remaingamount <= 0)
            {
                List<jntuh_scmproceedingsrequests> scmrequestsupdate =
              db.jntuh_scmproceedingsrequests.Where(
                  r =>
                      r.CollegeId == userCollegeID && r.phaseId == scmphase.phaseId && r.CreatedOn >= scmphase.fromdate && r.DEpartmentId != 0 && r.academicyearId == ay0 &&
                      r.RequestSubmittedDate == null && r.isscmatUniversity == true).Select(s => s).ToList();
                foreach (var scmrequest in scmrequestsupdate)
                {
                    var facultyCount = db.jntuh_scmproceedingrequest_addfaculty.Where(a => a.ScmProceedingId == scmrequest.ID).ToList();
                    if (facultyCount.Count > 0)
                    {
                        scmrequest.RequestSubmittedDate = DateTime.Now;
                        scmrequest.PaymentTypeId = Convert.ToInt32(paymentdonedetails.Id);
                        db.Entry(scmrequest).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                }
            }
            else if (remaingamount > 0)
            {
                TempData["Error"] = "You Need to Pay Remaing Amount " + remaingamount;
            }
            else
            {
                TempData["Error"] = "Payment is failed.";
            }
            var collegename = db.jntuh_college.SingleOrDefault(a => a.id == userCollegeID).collegeName;
            var membershipmailid = db.my_aspnet_membership.Where(i => i.userId == userID).FirstOrDefault().Email;
            IUserMailer mailer = new UserMailer();
            mailer.PaymentResponse(membershipmailid, "SCM Faculty Payment Response", dbResponse.CollegeId + " / " + collegename,
                dbResponse.CustomerID, dbResponse.TxnReferenceNo, dbResponse.BankReferenceNo, dbResponse.TxnAmount,
                dbResponse.TxnDate.ToString(), dbResponse.ErrorDescription, dbResponse.ChallanNumber, "SCM Faculty Payment Response",
                "JNTUH-AAC-SCM Faculty PAYMENT STATUS").SendAsync();

        }

        [HttpPost]
        public ActionResult SavePaymentRequest(string challanNumber, decimal txnAmount, string collegeCode)
        {
            //Note: 9 Means SCM Payment ID it is in Payment Master
            var appSettings = WebConfigurationManager.AppSettings;
            var req = new jntuh_paymentrequests();
            int actualYear =
                db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true)
                    .Select(a => a.actualYear)
                    .FirstOrDefault();
            int prAy =
                db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
            req.TxnAmount = txnAmount;
            req.AcademicYearId = prAy;
            req.CollegeCode = collegeCode;
            req.ChallanNumber = challanNumber;
            req.MerchantID = appSettings["MerchantID"];
            req.CustomerID = appSettings["CustomerID"];
            req.SecurityID = appSettings["SecurityID"];
            req.CurrencyType = appSettings["CurrencyType"];
            req.TxnDate = DateTime.Now;
            req.PaymentTypeID = 9;

            db.jntuh_paymentrequests.Add(req);

            db.SaveChanges();
            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }

        //SCM Principal Payment Screens
        public ActionResult scmprincipalpayment()
        {
            //Note: 9 Means SCM Payment ID it is in Payment Master
            DateTime todayDate = DateTime.Now;
            var userId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            var userCollegeId = db.jntuh_college_users.Where(u => u.userID == userId).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeId == 375)
            {
                userCollegeId = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int ay0 = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();
            int lid =
                db.jntuh_link_screens.Where(p => p.linkName == "Principal SCM Request" && p.isActive == true).Select(s => s.id).FirstOrDefault();
            jntuh_college_links_assigned scmphase =
                db.jntuh_college_links_assigned.Where(l => l.linkId == lid && l.academicyearId == ay0 && l.isActive == true && l.fromdate <= todayDate && l.todate >= todayDate).Select(s => s).FirstOrDefault();
            ViewBag.IsEditable = false;
            if (scmphase == null && Roles.IsUserInRole("College"))
            {

                return RedirectToAction("College", "Dashboard");
            }
            else
            {
                ViewBag.IsEditable = true;
            }


            //var requesteddepartments = depts.GroupBy().t;
            scmpaymentsdetals paymentdetailslist = new scmpaymentsdetals();
            long totalamount = 0;



            long scmamount = 0;
            long reqsubamount = 0;
            long reqnotsubamount = 0;
            //int did = Convert.ToInt32(departments.Key);
            //var scmids = scmrequests.Where(r => r.DEpartmentId == did).Select(s => s.ID).ToArray();

            scmpaymentsdetals paymentdetails = new scmpaymentsdetals();
            paymentdetails.department = "Principal";

            //paymentdetails.addedfacultycount = addedfaculty.Count();
            //int facultycount = paymentdetails.addedfacultycount;
            //while (facultycount > 0)
            //{
            //    scmamount = scmamount + 11000;
            //    facultycount = facultycount - 4;
            //}
            //paymentdetails.amount = scmamount;
            //Requested Submitted Amount
            List<jntuh_scmproceedingsrequests> scmrequests =
               db.jntuh_scmproceedingsrequests.Where(
                   r =>
                       r.CollegeId == userCollegeId && r.phaseId == scmphase.phaseId && r.CreatedOn >= scmphase.fromdate && r.DEpartmentId == 0 && r.SpecializationId == 0 && r.academicyearId == ay0 &&
                        r.isscmatUniversity == true && r.RequestSubmittedDate != null).Select(s => s).ToList();

            var requestedsmids = scmrequests.Select(s => s.ID).ToArray();
            List<jntuh_scmproceedingrequest_addfaculty> RequestSubmittedaddedfaculty =
                   db.jntuh_scmproceedingrequest_addfaculty.Where(a => requestedsmids.Contains(a.ScmProceedingId))
                       .Select(s => s)
                       .ToList();
            paymentdetails.addedfacultycount = RequestSubmittedaddedfaculty.Count();
            int reqsubfacultycount = paymentdetails.addedfacultycount;
            while (reqsubfacultycount > 0)
            {
                reqsubamount = reqsubfacultycount * 22000;
                reqsubfacultycount = reqsubfacultycount - 1;
                paymentdetails.amount = scmamount;
            }
            //Requested Not Submitted Amount
            List<jntuh_scmproceedingsrequests> reqnotsubscmrequests =
                  db.jntuh_scmproceedingsrequests.Where(
                      r =>
                          r.CollegeId == userCollegeId && r.phaseId == scmphase.phaseId && r.CreatedOn >= scmphase.fromdate && r.DEpartmentId == 0 && r.SpecializationId == 0 && r.academicyearId == ay0 &&
                           r.isscmatUniversity == true && r.RequestSubmittedDate == null).Select(s => s).ToList();

            var reqnotsubsmids = reqnotsubscmrequests.Select(s => s.ID).ToArray();
            List<jntuh_scmproceedingrequest_addfaculty> RequestnotSubmittedaddedfaculty =
                   db.jntuh_scmproceedingrequest_addfaculty.Where(a => reqnotsubsmids.Contains(a.ScmProceedingId))
                       .Select(s => s)
                       .ToList();
            paymentdetails.addedfacultycount = RequestnotSubmittedaddedfaculty.Count();
            int reqnotsubfacultycount = paymentdetails.addedfacultycount;
            while (reqnotsubfacultycount > 0)
            {
                reqnotsubamount = reqnotsubamount + 22000;
                reqnotsubfacultycount = reqnotsubfacultycount - 1;
                paymentdetails.amount = reqnotsubamount;
            }
            //paymentdetails.amount = scmamount;

            totalamount = totalamount + reqnotsubamount + reqsubamount;
            string clgCode = db.jntuh_college.SingleOrDefault(a => a.id == userCollegeId).collegeCode;

            ViewBag.collegeCode = clgCode;
            ViewBag.challnNumber = clgCode + DateTime.Now.ToString("yyyMMddHHmmss");
            var currentYear = DateTime.Now.Year;
            int paymentid =
              db.jntuh_college_paymentoffee_type.Where(p => p.FeeType == "SCM Principal Payment")
                  .Select(s => s.id)
                  .FirstOrDefault();
            var payments = db.jntuh_paymentresponse.Where(it => it.CollegeId == clgCode && it.AcademicYearId == ay0 && it.AuthStatus == "0300" && it.TxnDate.Year == currentYear && it.PaymentTypeID == paymentid && it.TxnDate >= scmphase.fromdate)
                    .Select(s => s.TxnAmount).ToList().Count() > 0 ?
                db.jntuh_paymentresponse.Where(it => it.CollegeId == clgCode && it.AcademicYearId == ay0 && it.AuthStatus == "0300" && it.TxnDate.Year == currentYear && it.PaymentTypeID == paymentid && it.TxnDate >= scmphase.fromdate)
                .Select(s => s.TxnAmount).Sum() : 0;
            var paymenthistory =
                db.jntuh_paymentresponse.Where(
                    it =>
                        it.CollegeId == clgCode && it.AcademicYearId == ay0 && it.AuthStatus == "0300" &&
                        it.TxnDate.Year == currentYear && it.PaymentTypeID == paymentid && it.TxnDate >= scmphase.fromdate)
                    .Select(s => s).ToList();
            ViewBag.Payments = paymenthistory;
            ViewBag.totalFee = totalamount;
            decimal remaingamount = totalamount - payments;
            ViewBag.IsPaymentDone = false;
            //if (payments >= totalamount)
            //{
            //    ViewBag.IsPaymentDone = true;
            //    ViewBag.totalFee = payments;
            //}
            if (remaingamount < 0)
            {
                ViewBag.IsPaymentDone = true;
                ViewBag.totalFee = totalamount;
            }
            else
            {
                ViewBag.totalFee = remaingamount;
            }
            var returnUrl = WebConfigurationManager.AppSettings["SCMPrincialReturnUrl"];
            var merchantId = WebConfigurationManager.AppSettings["MerchantID"];
            var securityId = WebConfigurationManager.AppSettings["SecurityID"];
            var typefield1 = WebConfigurationManager.AppSettings["TypeField1"];
            var typefield2 = WebConfigurationManager.AppSettings["TypeField2"];
            var msg = "";
            if (userCollegeId == 375)
            {
                msg = merchantId + "|" + ViewBag.challnNumber + "|NA|2.00|NA|NA|NA|INR|NA|" + typefield1 + "|" +
                      securityId + "|NA|NA|" + typefield2 + "|NA|NA|NA|NA|NA|NA|NA|" + returnUrl;
                var key = WebConfigurationManager.AppSettings["ChecksumKey"];
                msg += "|" + GetHMACSHA256(msg, key).ToUpper();
            }
            else
            {
                msg = merchantId + "|" + ViewBag.challnNumber + "|NA|" + ViewBag.totalFee + "|NA|NA|NA|INR|NA|" +
                      typefield1 + "|" + securityId + "|NA|NA|" + typefield2 + "|NA|NA|NA|NA|NA|NA|NA|" +
                      returnUrl;
                var key = WebConfigurationManager.AppSettings["ChecksumKey"];
                msg += "|" + GetHMACSHA256(msg, key).ToUpper();
            }

            ViewBag.msg = msg;

            return View(paymentdetails);
        }

        [HttpPost]
        public ActionResult scmprincipalpayment(string msg)
        {
            var appSettings = WebConfigurationManager.AppSettings;
            var req = new jntuh_paymentrequests();
            SavePrincipalPaymentResponse(msg, "ChallanNumber");
            return RedirectToAction("CollegeScmProceedingsPrincipalRequest");
        }

        private void SavePrincipalPaymentResponse(string responseMsg, string challanno)
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
                userID =
                    db.jntuh_college_users.Where(u => u.collegeID == userCollegeID)
                        .Select(u => u.userID)
                        .FirstOrDefault();
            }
            else
            {
                userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
                userCollegeID =
                    db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
                clgCode = db.jntuh_college.SingleOrDefault(a => a.id == userCollegeID).collegeCode;
            }
            DateTime todayDate = DateTime.Now;
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int prAy =
               db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
            int lid =
                db.jntuh_link_screens.Where(p => p.linkName == "Principal SCM Request" && p.isActive == true).Select(s => s.id).FirstOrDefault();
            jntuh_college_links_assigned scmphase =
                db.jntuh_college_links_assigned.Where(l => l.linkId == lid && l.academicyearId == prAy && l.isActive == true && l.fromdate <= todayDate && l.todate >= todayDate).Select(s => s).FirstOrDefault();
            int paymentid =
                db.jntuh_college_paymentoffee_type.Where(p => p.FeeType == "SCM Principal Payment")
                    .Select(s => s.id)
                    .FirstOrDefault();
            //Response Message Saving
            temp_aeronautical temp_aeronautical = new temp_aeronautical();
            temp_aeronautical.Degree = responseMsg.Length < 255
                ? responseMsg
                : responseMsg.Substring(0, 254);
            ;
            temp_aeronautical.Department = "PSCM fee";
            temp_aeronautical.Specialization = clgCode;
            temp_aeronautical.DegreeId = userCollegeID;
            temp_aeronautical.LabCode = DateTime.Now.ToString();
            db.temp_aeronautical.Add(temp_aeronautical);
            db.SaveChanges();


            var dbResponse = new UAAAS.Models.jntuh_paymentresponse();
            dbResponse.AcademicYearId = prAy;
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

            dbResponse.PaymentTypeID = paymentid;
            db.jntuh_paymentresponse.Add(dbResponse);
            db.SaveChanges();

            //Payment Done Details
            var paymentdonedetails =
                db.jntuh_paymentresponse.Where(
                    r =>
                        r.CollegeId == dbResponse.CollegeId && r.AcademicYearId == prAy && r.AuthStatus == "0300" &&
                        r.PaymentTypeID == paymentid && r.TxnReferenceNo == dbResponse.TxnReferenceNo &&
                        r.CustomerID == dbResponse.CustomerID && r.BankReferenceNo == dbResponse.BankReferenceNo && r.TxnDate >= scmphase.fromdate)
                    .FirstOrDefault();
            #region
            //Payment Checking
            //List<jntuh_scmproceedingsrequests> scmrequests =
            //   db.jntuh_scmproceedingsrequests.Where(
            //       r =>
            //           r.CollegeId == userCollegeID && r.phaseId == scmphase.phaseId && r.CreatedOn >= scmphase.fromdate && r.DEpartmentId == 0 && r.SpecializationId == 0 && r.academicyearId == prAy &&
            //           r.RequestSubmittedDate == null && r.isscmatUniversity == true).Select(s => s).ToList();

            //var requestedsmids = scmrequests.Select(s => s.ID).ToArray();
            //var requesteddepartments = depts.GroupBy().t;
            scmpaymentsdetals paymentdetailslist = new scmpaymentsdetals();
            long totalamount = 0;
            long reqsubamount = 0;
            long reqnotsubamount = 0;
            long scmamount = 0;
            //int did = Convert.ToInt32(departments.Key);
            //var scmids = scmrequests.Where(r => r.DEpartmentId == did).Select(s => s.ID).ToArray();

            //List<jntuh_scmproceedingrequest_addfaculty> addedfaculty =
            //    db.jntuh_scmproceedingrequest_addfaculty.Where(a => requestedsmids.Contains(a.ScmProceedingId))
            //        .Select(s => s)
            //        .ToList();
            //scmpaymentsdetals paymentdetails = new scmpaymentsdetals();
            //paymentdetails.department = "Principal";
            //paymentdetails.addedfacultycount = addedfaculty.Count();
            //int facultycount = paymentdetails.addedfacultycount;
            //while (facultycount > 0)
            //{
            //    scmamount = scmamount + 11000;
            //    facultycount = facultycount - 4;
            //}
            //paymentdetails.amount = scmamount;

            //totalamount = totalamount + scmamount;
            //Requested Submitted Amount
            List<jntuh_scmproceedingsrequests> scmrequests =
               db.jntuh_scmproceedingsrequests.Where(
                   r =>
                       r.CollegeId == userCollegeID && r.phaseId == scmphase.phaseId && r.CreatedOn >= scmphase.fromdate && r.DEpartmentId == 0 && r.SpecializationId == 0 && r.academicyearId == prAy &&
                        r.isscmatUniversity == true && r.RequestSubmittedDate != null).Select(s => s).ToList();

            var requestedsmids = scmrequests.Select(s => s.ID).ToArray();
            List<jntuh_scmproceedingrequest_addfaculty> RequestSubmittedaddedfaculty =
                   db.jntuh_scmproceedingrequest_addfaculty.Where(a => requestedsmids.Contains(a.ScmProceedingId))
                       .Select(s => s)
                       .ToList();
            paymentdetailslist.addedfacultycount = RequestSubmittedaddedfaculty.Count();
            int reqsubfacultycount = paymentdetailslist.addedfacultycount;
            while (reqsubfacultycount > 0)
            {
                reqsubamount = reqsubamount + 22000;
                reqsubfacultycount = reqsubfacultycount - 1;
                paymentdetailslist.amount = scmamount;
            }
            //Requested Not Submitted Amount
            List<jntuh_scmproceedingsrequests> reqnotsubscmrequests =
                  db.jntuh_scmproceedingsrequests.Where(
                      r =>
                          r.CollegeId == userCollegeID && r.phaseId == scmphase.phaseId && r.CreatedOn >= scmphase.fromdate && r.DEpartmentId == 0 && r.SpecializationId == 0 && r.academicyearId == prAy &&
                           r.isscmatUniversity == true && r.RequestSubmittedDate == null).Select(s => s).ToList();

            var reqnotsubsmids = reqnotsubscmrequests.Select(s => s.ID).ToArray();
            List<jntuh_scmproceedingrequest_addfaculty> RequestnotSubmittedaddedfaculty =
                   db.jntuh_scmproceedingrequest_addfaculty.Where(a => reqnotsubsmids.Contains(a.ScmProceedingId))
                       .Select(s => s)
                       .ToList();
            paymentdetailslist.addedfacultycount = RequestnotSubmittedaddedfaculty.Count();
            int reqnotsubfacultycount = paymentdetailslist.addedfacultycount;
            while (reqnotsubfacultycount > 0)
            {
                reqnotsubamount = reqnotsubamount + 22000;
                reqnotsubfacultycount = reqnotsubfacultycount - 1;
                paymentdetailslist.amount = reqnotsubamount;
            }
            //paymentdetails.amount = scmamount;

            totalamount = totalamount + reqnotsubamount + reqsubamount;
            #endregion
            ViewBag.collegeCode = clgCode;
            ViewBag.challnNumber = clgCode + DateTime.Now.ToString("yyyMMddHHmmss");
            var currentYear = DateTime.Now.Year;
            var payments = db.jntuh_paymentresponse.Where(it => it.CollegeId == clgCode && it.AcademicYearId == prAy && it.AuthStatus == "0300" && it.TxnDate.Year == currentYear && it.PaymentTypeID == paymentid)
                    .Select(s => s.TxnAmount).ToList().Count() > 0 ?
                db.jntuh_paymentresponse.Where(it => it.CollegeId == clgCode && it.AcademicYearId == prAy && it.AuthStatus == "0300" && it.TxnDate.Year == currentYear && it.PaymentTypeID == paymentid)
                .Select(s => s.TxnAmount).Sum() : 0;

            decimal remaingamount = totalamount - payments;

            if (paymentdonedetails != null && remaingamount <= 0)
            {
                List<jntuh_scmproceedingsrequests> scmrequestsupdate =
              db.jntuh_scmproceedingsrequests.Where(
                  r =>
                      r.CollegeId == userCollegeID && r.phaseId == scmphase.phaseId && r.CreatedOn >= scmphase.fromdate && r.DEpartmentId == 0 && r.academicyearId == prAy &&
                      r.RequestSubmittedDate == null && r.isscmatUniversity == true).Select(s => s).ToList();
                foreach (var scmrequest in scmrequestsupdate)
                {
                    scmrequest.RequestSubmittedDate = DateTime.Now;
                    scmrequest.PaymentTypeId = Convert.ToInt32(paymentdonedetails.Id);
                    db.Entry(scmrequest).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }
            else if (remaingamount > 0)
            {
                TempData["Error"] = "SCM Requset Not Yet Submitted.you need to Pay Remaing Amount " + remaingamount;
            }
            else
            {
                TempData["Error"] = "Payment is failed.";
            }
            var collegename = db.jntuh_college.SingleOrDefault(a => a.id == userCollegeID).collegeName;
            var membershipmailid = db.my_aspnet_membership.Where(i => i.userId == userID).FirstOrDefault().Email;
            IUserMailer mailer = new UserMailer();
            mailer.PaymentResponse(membershipmailid, "SCM Principal Payment Response", dbResponse.CollegeId + " / " + collegename,
                dbResponse.CustomerID, dbResponse.TxnReferenceNo, dbResponse.BankReferenceNo, dbResponse.TxnAmount,
                dbResponse.TxnDate.ToString(), dbResponse.ErrorDescription, dbResponse.ChallanNumber, "SCM Principal Payment Response",
                "JNTUH-AAC-SCM Principal PAYMENT STATUS").SendAsync();

        }

        [HttpPost]
        public ActionResult SavePrincipalPaymentRequest(string challanNumber, decimal txnAmount, string collegeCode)
        {
            //Note: 14 Means SCM Payment ID it is in Payment Master
            var appSettings = WebConfigurationManager.AppSettings;
            var req = new jntuh_paymentrequests();
            int actualYear =
                db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true)
                    .Select(a => a.actualYear)
                    .FirstOrDefault();
            int prAy =
                db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
            int paymentid =
               db.jntuh_college_paymentoffee_type.Where(p => p.FeeType == "SCM Principal Payment")
                   .Select(s => s.id)
                   .FirstOrDefault();
            req.TxnAmount = txnAmount;
            req.AcademicYearId = prAy;
            req.CollegeCode = collegeCode;
            req.ChallanNumber = challanNumber;
            req.MerchantID = appSettings["MerchantID"];
            req.CustomerID = appSettings["CustomerID"];
            req.SecurityID = appSettings["SecurityID"];
            req.CurrencyType = appSettings["CurrencyType"];
            req.TxnDate = DateTime.Now;
            req.PaymentTypeID = paymentid;

            db.jntuh_paymentrequests.Add(req);

            db.SaveChanges();
            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }


        private string PaymentBillDetails(int collegeId, string contents, int paymentid)
        {
            string Paymentbilldetails = string.Empty;
            string strPaymentDate = string.Empty;
            Paymentbilldetails = "";

            List<IElement> parsedHtmlElements3 = HTMLWorker.ParseToList(new StringReader(contents), null);
            var collegedetails = db.jntuh_college.Where(e => e.id == collegeId && e.isActive == true).Select(e => e).FirstOrDefault();
            string collegecode = collegedetails.collegeCode;
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int ay0 = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();
            List<jntuh_paymentresponse> payment = db.jntuh_paymentresponse.Where(a => a.AcademicYearId == ay0 && a.CollegeId == collegecode && a.AuthStatus == "0300" && a.Id == paymentid).ToList();


            if (payment != null && payment.Count() != 0)
            {
                Paymentbilldetails += "<br/><p style='text-align:center;font-size:12px'><b> STAFF SELECTION COMMITTEE REQUEST </b></p><br/>";
                Paymentbilldetails += "<table border='0'cellspacing='0' cellpadding='4' width='100%'>";
                Paymentbilldetails += "<tr><td style='text-align:left'><b>College Name : " + collegedetails.collegeName + "</b></td>";
                Paymentbilldetails += "<td style='text-align:right'><b>College Code : " + collegedetails.collegeCode + "</b></td></tr>";
                Paymentbilldetails += "</table>";
                Paymentbilldetails += "<br/><p align='left'><strong><u>Payment Details</u></strong></p><br />";
                Paymentbilldetails += "<table width='100%' border='1' cellspacing='0' cellpadding='5' style='font-size: 9px;'>";
                Paymentbilldetails += "<tbody>";
                foreach (var item in payment.Take(1))
                {
                    if (item.TxnDate != null)
                    {
                        strPaymentDate = item.TxnDate.ToString() == null ? string.Empty : UAAAS.Models.Utilities.MMDDYY2DDMMYY(item.TxnDate.ToString()).ToString();
                    }
                    Paymentbilldetails += "<tr>";
                    Paymentbilldetails += "<td width='35%'><p align='left'>Customer Id</p></td>";
                    Paymentbilldetails += "<td width='5%'><p align='center'>:</p></td>";
                    Paymentbilldetails += "<td width='60%'><p align='left'>" + item.CustomerID + "</p></td>";
                    Paymentbilldetails += "</tr>";
                    Paymentbilldetails += "<tr>";
                    Paymentbilldetails += "<td width='35%'><p align='left'>Transaction Ref.no</p></td>";
                    Paymentbilldetails += "<td width='5%'><p align='center'>:</p></td>";
                    Paymentbilldetails += "<td width='60%'><p align='left'>" + item.TxnReferenceNo + "</p></td>";
                    Paymentbilldetails += "</tr>";
                    Paymentbilldetails += "<tr>";
                    Paymentbilldetails += "<td width='35%'><p align='left'>Bank Refno</p></td>";
                    Paymentbilldetails += "<td width='5%'><p align='center'>:</p></td>";
                    Paymentbilldetails += "<td width='60%'><p align='left'>" + item.BankReferenceNo + "</p></td>";
                    Paymentbilldetails += "</tr>";
                    Paymentbilldetails += "<tr>";
                    Paymentbilldetails += "<td width='35%'><p align='left'>Transaction Amount</p></td>";
                    Paymentbilldetails += "<td width='5%'><p align='center'>:</p></td>";
                    Paymentbilldetails += "<td width='60%'><p align='left'>" + item.TxnAmount + "</p></td>";
                    Paymentbilldetails += "</tr>";
                    Paymentbilldetails += "<tr>";
                    Paymentbilldetails += "<td width='35%'><p align='left'>Transaction Date</p></td>";
                    Paymentbilldetails += "<td width='5%'><p align='center'>:</p></td>";
                    Paymentbilldetails += "<td width='60%'><p align='left'>" + strPaymentDate + "</p></td>";
                    Paymentbilldetails += "</tr>";
                    Paymentbilldetails += "<tr>";
                    Paymentbilldetails += "<td width='35%'><p align='left'>Payment Description</p></td>";
                    Paymentbilldetails += "<td width='5%'><p align='center'>:</p></td>";
                    Paymentbilldetails += "<td width='60%'><p align='left'>" + item.ErrorDescription + "</p></td>";
                    Paymentbilldetails += "</tr>";

                    //Paymentbilldetails += "<tr>";
                    //Paymentbilldetails += "<td colspan='4'><p align='left'>" + strPaymentDate + "</p></td>";
                    //Paymentbilldetails += "<td  colspan='3' align='left'>" + item.TxnReferenceNo + "</td>";
                    //Paymentbilldetails += "<td  colspan='3' align='left'>" + item.TxnAmount + "</td>";
                    //Paymentbilldetails += "</tr>";
                    barcodetext += ";Payment Date:" + strPaymentDate + ";Customer Id:" + item.CustomerID;
                }
                Paymentbilldetails += "</tbody></table>";
            }
            else
            {
                Paymentbilldetails += "<table border='1' cellspacing='0' cellpadding='5' style='font-size: 9px;'>";
                Paymentbilldetails += "<tbody>";
                Paymentbilldetails += "<tr>";
                Paymentbilldetails += "<td colspan='4'><p align='left'>Payment Date</p></td>";
                Paymentbilldetails += "<td colspan='3'><p align='left'>Reference Number</p></td>";
                Paymentbilldetails += "<td colspan='3'><p align='left'>Transaction Amount</p></td>";
                Paymentbilldetails += "</tr>";
                Paymentbilldetails += "<tr>";
                Paymentbilldetails += "<td colspan='10'><p align='center'>NIL</p></td>";
                Paymentbilldetails += "</tr>";
                Paymentbilldetails += "</tbody></table>";

            }
            //  Paymentbilldetails += "</tbody></table>";
            contents = contents.Replace("##PaymentDetails##", Paymentbilldetails);
            List<IElement> parsedHtmlElements = HTMLWorker.ParseToList(new StringReader(contents), null);
            var Paymentdate = payment.Select(e => e.TxnDate).FirstOrDefault();
            string paymentdatecurrentformat = string.Empty;
            if (Paymentdate != null && payment.Count() != 0)
            {
                paymentdatecurrentformat = UAAAS.Models.Utilities.MMDDYY2DDMMYY(Paymentdate.ToString());
            }
            else
            {
                paymentdatecurrentformat = string.Empty;
            }
            contents = contents.Replace("##PAYMENTDATE##", paymentdatecurrentformat);
            List<IElement> parsedHtmlElements2 = HTMLWorker.ParseToList(new StringReader(contents), null);

            barcodetext += ";SCM Payment Date:" + strPaymentDate;
            return contents;
        }
        public string barcodegenerator(int collegeId, string contents)
        {

            string str = string.Empty;
            string strDataModifications = string.Empty;
            string strimagedetails = string.Empty;
            string strimagebarcodedetails = string.Empty;
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int ay0 = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();
            var collegeCode = db.jntuh_college.Where(college => college.isActive == true && college.id == collegeId).Select(college => college.collegeCode).FirstOrDefault();
            var challanaNO = db.jntuh_paymentresponse.Where(college => college.CollegeId == collegeCode && college.AuthStatus == "0300" && college.AcademicYearId == ay0 && college.PaymentTypeID == 9).Select(college => college.CustomerID).FirstOrDefault();

            if (challanaNO != null)
            {

                /////QR Code GEneration Code
                Gma.QrCodeNet.Encoding.QrEncoder qrEncoder = new Gma.QrCodeNet.Encoding.QrEncoder(Gma.QrCodeNet.Encoding.ErrorCorrectionLevel.H);
                Gma.QrCodeNet.Encoding.QrCode qrCode = new Gma.QrCodeNet.Encoding.QrCode();
                qrEncoder.TryEncode(barcodetext, out qrCode);
                Gma.QrCodeNet.Encoding.Windows.Render.GraphicsRenderer renderer = new Gma.QrCodeNet.Encoding.Windows.Render.GraphicsRenderer(new Gma.QrCodeNet.Encoding.Windows.Render.FixedModuleSize(4, Gma.QrCodeNet.Encoding.Windows.Render.QuietZoneModules.Four), System.Drawing.Brushes.Black, System.Drawing.Brushes.White);

                Stream memoryStream = new MemoryStream();
                renderer.WriteToStream(qrCode.Matrix, ImageFormat.Png, memoryStream);
                // very important to reset memory stream to a starting position, otherwise you would get 0 bytes returned
                memoryStream.Position = 0;

                var resultStream = new FileStreamResult(memoryStream, "image/png");
                resultStream.FileDownloadName = String.Format("{0}.png", collegeCode);


                System.Drawing.Image v = System.Drawing.Image.FromStream(memoryStream);
                if (!Directory.Exists(Server.MapPath("~/Content/Upload/Barcodes")))
                {
                    Directory.CreateDirectory(Server.MapPath("~/Content/Upload/Barcodes"));
                }
                var ext = resultStream.ContentType;
                var Filename = resultStream.FileDownloadName;

                System.Drawing.Image img = v;
                img.Save(string.Format("{0}/{1}", Server.MapPath("~/Content/Upload/Barcodes"), Filename));

                if (Filename != null)
                {
                    strimagedetails = "/Content/Upload/Barcodes/" + Filename;
                }
                else
                {
                    strimagedetails = string.Empty;
                }
                string path = @"~" + strimagedetails;
                path = System.Web.HttpContext.Current.Server.MapPath(path);


                if (challanaNO != null)
                {
                    strimagebarcodedetails = "/Content/Upload/Barcodes/" + challanaNO + ".png";
                }
                else
                {
                    strimagebarcodedetails = string.Empty;
                }
                string path1 = @"~" + strimagebarcodedetails;
                path1 = System.Web.HttpContext.Current.Server.MapPath(path1);

                strDataModifications += "<table><tr>";

                if (System.IO.File.Exists(path))
                {
                    strDataModifications += "<td><img src='" + path + "' align='left'  width='100' height='100' /></td>";
                    // strDataModifications += "<td><img src=" + serverURL + "" + strimagedetails + " align='left'  width='100' height='100' /></td>";
                }
                else
                {
                    strDataModifications += "<td width='100' style='vertical-align:top' align='left' colspan='4'><p align='center'></p></td>";
                }
                strDataModifications += "</tr></table>";
            }

            contents = contents.Replace("##QRcode##", strDataModifications);
            return contents;
        }
        public class scmpaymentsdetals
        {
            public int departmentId { get; set; }
            public string department { get; set; }
            public int addedfacultycount { get; set; }
            public string registrationnumber { get; set; }
            public decimal amount { get; set; }
            public bool IssubmissionStatus { get; set; }
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
        #endregion

        //Total Nominees Assigned With colleges for Admin
        //[Authorize(Roles = "Admin")]
        //public ActionResult NomineeAssignedAdminView()
        //{
        //    var userId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
        //    //var userCollegeId =
        //    //    db.jntuh_college_users.Where(u => u.userID == userId).Select(u => u.collegeID).FirstOrDefault();
        //    //userCollegeId = 158;
        //    int[] collegeIds = { 2, 4, 7, 8, 9, 11, 12, 17, 20, 22, 23, 26, 29, 32, 34, 38, 40, 41, 46, 48, 56, 59, 68, 69, 70, 72, 74, 77, 79, 80, 81, 84, 85, 86, 87, 88, 100, 102, 103, 104, 106, 108, 109, 111, 113, 115, 116, 119, 121, 122, 123, 124, 125, 128, 129, 130, 132, 134, 137, 138, 141, 143, 144, 145, 147, 148, 151, 152, 153, 155, 156, 157, 158, 159, 161, 162, 163, 164, 165, 166, 168, 170, 171, 172, 173, 175, 176, 177, 178, 179, 181, 182, 183, 184, 185, 186, 187, 188, 189, 192, 193, 195, 196, 197, 198, 201, 203, 207, 210, 211, 214, 215, 218, 222, 225, 227, 228, 229, 236, 238, 241, 242, 243, 244, 245, 247, 249, 250, 254, 256, 259, 260, 261, 264, 269, 271, 273, 276, 282, 283, 286, 287, 291, 292, 293, 299, 300, 304, 305, 306, 307, 308, 309, 310, 315, 316, 321, 322, 324, 326, 327, 329, 330, 334, 335, 336, 342, 349, 350, 352, 360, 365, 366, 367, 368, 369, 371, 373, 374, 376, 380, 382, 385, 391, 393, 394, 395, 399, 400, 401, 402, 403, 414, 415, 416, 419, 420, 422, 423, 424, 428, 429, 430, 6, 24, 27, 30, 44, 45, 47, 52, 54, 55, 58, 60, 65, 66, 78, 90, 95, 97, 105, 107, 110, 114, 117, 118, 120, 127, 135, 136, 139, 146, 150, 169, 202, 204, 206, 213, 219, 234, 237, 239, 252, 253, 262, 263, 267, 290, 295, 297, 298, 301, 302, 303, 313, 314, 317, 318, 319, 320, 348, 353, 362, 370, 379, 384, 389, 392, 410, 427, 5, 67, 246, 279, 296, 325, 343, 355, 386, 411, 421, 39, 42, 43, 75, 140, 180, 194, 217, 223, 230, 235, 266, 332, 364, 35, 50, 91, 174, 435, 436, 439, 441, 442, 443, 445, 447, 448, 452, 454, 455 ,449};
        //    DateTime scmstartdate = new DateTime(2018, 01, 22);
        //    if (userId != 0)
        //    {
        //        var nomineeAssignedScmIds = (from SCM in db.jntuh_scmproceedingsrequests
        //            join AUDA in db.jntuh_auditor_assigned on SCM.ID equals AUDA.ScmId
        //            where
        //                collegeIds.Contains(SCM.CollegeId) && SCM.SpecializationId != 0 && SCM.DEpartmentId != 0 &&
        //                SCM.DegreeId != 0 && SCM.CreatedOn > scmstartdate
        //            select SCM.ID).Distinct().ToArray();
        //        var nomineeassignRequest = (from SCM in db.jntuh_scmproceedingsrequests
        //            join AUDA in db.jntuh_auditor_assigned on SCM.ID equals AUDA.ScmId
        //            join Nominee in db.jntuh_ffc_auditor on AUDA.AuditorId equals Nominee.id
        //            join SPEC in db.jntuh_specialization on SCM.SpecializationId equals SPEC.id
        //            join DEPT in db.jntuh_department on SCM.DEpartmentId equals DEPT.id
        //            join DEG in db.jntuh_degree on SCM.DegreeId equals DEG.id
        //            join CLG in db.jntuh_college on SCM.CollegeId equals CLG.id
        //            where nomineeAssignedScmIds.Contains(SCM.ID)
        //            select new NomineeAssignSCMRequests
        //            {
        //                SCMId = SCM.ID,
        //                CollegeCode = CLG.collegeCode,
        //                CollegeName = CLG.collegeName,
        //                //Department = DEG.degree + " " + SPEC.specializationName,
        //                Department = DEPT.departmentDescription,
        //                AuditorEmail = Nominee.auditorEmail1,
        //                AuditorMobile = Nominee.auditorMobile1,
        //                AuditorName = Nominee.auditorName,
        //                AuditorAssignDate = AUDA.CreatedOn,
        //                ScmRequestDate = (DateTime) SCM.RequestSubmittedDate,
        //            }).OrderByDescending(e => e.AuditorAssignDate).ToList();

        //        return View(nomineeassignRequest.OrderBy(s=>s.CollegeName).ToList());
        //    }
        //    return View();
        //}

        #region Appeal SCM Procedding Request Screens
        [Authorize(Roles = "College")]
        [HttpGet]
        public ActionResult AppealCollegeScmProceedingsRequest()
        {


            var pharmacyids = new[]{24,27,30,33,34,44,47,52,55,65,78,83,95,97,104,105,107,110,114,117,118,127,135,136,139,146,150,159,169,202,204,213,219,234,239,253,
                262,263,267,275,283,284,290,295,297,298,301,303,314,317,320,348,353,362,376,377,379,384,392,395,410,427,428,436,442,445,448,454,457,458,6,
                45,54,58,90,120,206,237,302,313,318,319,389,60,66,252,255,315,370,9, 18, 39, 42, 75, 140, 180, 332, 364,375};







            ScmProceedingsRequest scmProceedings = new ScmProceedingsRequest();
            string clgCode;
            var userId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            var userCollegeId = db.jntuh_college_users.Where(u => u.userID == userId).Select(u => u.collegeID).FirstOrDefault();




            if (!pharmacyids.Contains(userCollegeId))
            {
                return RedirectToAction("College", "Dashboard");
            }

            var firstOrDefault = db.jntuh_college.FirstOrDefault(a => a.id == userCollegeId);
            var specs = new List<DistinctSpecializations>();
            var depts = new List<DistinctDepartments>();
            var degrees = db.jntuh_degree.AsNoTracking().ToList();
            var specializations = db.jntuh_specialization.AsNoTracking().ToList();
            var departments = db.jntuh_department.AsNoTracking().ToList();
            //int[] collegespecs = new int[];
            List<int> collegespecs = new List<int>();

            List<int> phrmacyDegreeIds = new List<int>() { 2, 5, 9, 10 };


            collegespecs.AddRange(db.jntuh_college_intake_existing.Where(i => i.collegeId == userCollegeId).Select(i => i.specializationId).Distinct().ToArray());

            int[] degreeIds = (from a in specializations
                               join b in departments on a.departmentId equals b.id
                               join c in degrees on b.degreeId equals c.id
                               where collegespecs.Contains(a.id)
                               select c.id).Distinct().ToArray();
            if (degreeIds.Contains(4))
            {
                var humanitesSpeci = new[] { 37, 48, 42, 31, 154 };
                collegespecs.AddRange(humanitesSpeci);
            }

            //Others Departments

            if (collegespecs.Contains(154))
            {
                var othersSpeci = new[] { 155, 156, 157, 158 };
                collegespecs.AddRange(othersSpeci);
            }

            collegespecs.Remove(154);

            foreach (var s in collegespecs)
            {
                var specid = specializations.FirstOrDefault(i => i.id == s);

                if (specid != null)
                {
                    var deptment = departments.FirstOrDefault(i => i.id == specid.departmentId);
                    var degree = degrees.FirstOrDefault(i => i.id == (deptment != null ? deptment.degreeId : 0));
                    if (degree != null)
                    {
                        if (phrmacyDegreeIds.Contains(degree.id))
                            specs.Add(new DistinctSpecializations { SpecializationId = specid.id, SpecializationName = degree.degree + " - " + specid.specializationName, DepartmentId = specid.departmentId });

                    }

                }
            }




            ViewBag.departments = specs.OrderBy(i => i.DepartmentId);

            var collegescmrequestslist = db.jntuh_appeal_scmproceedingsrequests.AsNoTracking().Where(i => i.CollegeId == userCollegeId).ToList();

            var proceedingsRequests = new List<ScmProceedingsRequest>();
            scmProceedings.ScmProceedingsRequestslist = new List<ScmProceedingsRequest>();
            foreach (var s in collegescmrequestslist)
            {

                var specid = specializations.FirstOrDefault(i => i.id == s.SpecializationId);

                if (specid != null && firstOrDefault != null)
                {
                    var deptment = departments.FirstOrDefault(i => i.id == specid.departmentId);
                    var degree = degrees.FirstOrDefault(i => i.id == (deptment != null ? deptment.degreeId : 0));
                    if (degree != null)

                        proceedingsRequests.Add(new ScmProceedingsRequest
                        {
                            CollegeName = firstOrDefault.collegeCode + " - " + firstOrDefault.collegeName,
                            CollegeCode = firstOrDefault.collegeCode,
                            ProfessorVacancies = s.ProfessorsCount.ToString(),
                            AssociateProfessorVacancies = s.AssociateProfessorsCount.ToString(),
                            AssistantProfessorVacancies = s.AssistantProfessorsCount.ToString(),
                            SpecializationName = degree.degree + " - " + specid.specializationName,
                            SpecializationId = specid.id,
                            DegreeId = s.DegreeId,
                            CollegeId = firstOrDefault.id,
                            DepartmentId = specid.departmentId,
                            ScmNotificationpath = s.SCMNotification,
                            Id = s.ID,
                            RequiredProfessorVacancies = s.RequiredProfessor.ToString(),
                            RequiredAssistantProfessorVacancies = s.RequiredAssistantProfessor.ToString(),
                            RequiredAssociateProfessorVacancies = s.RequiredAssociateProfessor.ToString(),
                            CreatedDate = s.CreatedOn,
                            Checked = false,
                            RequestSubmittedDate = s.RequestSubmittedDate
                        });
                }
            }
            scmProceedings.ScmProceedingsRequestslist.AddRange(proceedingsRequests.OrderByDescending(e => e.CreatedDate).Select(e => e).ToList());

            ViewBag.collegescmrequestslist = scmProceedings.ScmProceedingsRequestslist;
            ViewBag.CollegeId = userCollegeId;
            ViewBag.editable = false;
            ViewBag.RequestSubmittedDatecount = collegescmrequestslist.Where(C => C.RequestSubmittedDate != null).Select(C => C.RequestSubmittedDate).Count();
            ViewBag.Requestcount = collegescmrequestslist.Select(C => C.RequestSubmittedDate).Count();

            int Requestcount = collegescmrequestslist.Select(C => C.RequestSubmittedDate).Count();
            int RequestSubmittedCount = collegescmrequestslist.Where(C => C.RequestSubmittedDate != null).Select(C => C.RequestSubmittedDate).Count();
            ViewBag.OneRequest = false;
            if (Requestcount == 0 && RequestSubmittedCount == 0)
            {
                ViewBag.OneRequest = false;
            }
            else if (Requestcount != 0 && RequestSubmittedCount == 0)
            {
                ViewBag.OneRequest = false;
            }
            else if (Requestcount == RequestSubmittedCount)
            {
                ViewBag.OneRequest = true;
            }

            var currentDate = DateTime.Now;
            if (currentDate > new DateTime(2017, 6, 02, 23, 59, 59))
            {
                ViewBag.editable = true;
            }





            // scmProceedings.ScmProceedingsRequestslist.AddRange(proceedingsRequests);
            return View(scmProceedings);
        }

        [Authorize(Roles = "College")]
        [HttpPost]
        public ActionResult AppealCollegeScmProceedingsRequest(ScmProceedingsRequest scmrequest)
        {
            var userId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            var userCollegeId = db.jntuh_college_users.Where(u => u.userID == userId).Select(u => u.collegeID).FirstOrDefault();


            if (ModelState.IsValid)
            {
                int SCmOrder = 1;
                var fileName = string.Empty;
                var filepath = string.Empty;
                var collegescmrequests = new jntuh_appeal_scmproceedingsrequests();
                const string scmnotificationpath = "~/Content/Upload/SCMPROCEEDINGSREQUEST/ScmNotificationDocuments";
                if (scmrequest.ScmNotificationSupportDoc != null)
                {
                    var checkScmApplied = db.jntuh_appeal_scmproceedingsrequests.AsNoTracking().FirstOrDefault(e => e.CollegeId == userCollegeId && e.SpecializationId == scmrequest.SpecializationId);

                    if (checkScmApplied != null)
                    {
                        TempData["Error"] = "Selection Committe request already applied for this course.";
                    }
                    else
                    {

                        if (!Directory.Exists(Server.MapPath(scmnotificationpath)))
                        {
                            Directory.CreateDirectory(Server.MapPath(scmnotificationpath));
                        }

                        var ext = Path.GetExtension(scmrequest.ScmNotificationSupportDoc.FileName);
                        if (ext != null && ext.ToUpper().Equals(".PDF"))
                        {
                            var scmfileName = db.jntuh_college.Where(c => c.id == userCollegeId).Select(c => c.id).FirstOrDefault() + "_" + "ScmNotofication" + "_" + DateTime.Now.ToString("yyyMMddHHmmss");
                            scmrequest.ScmNotificationSupportDoc.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(scmnotificationpath), scmfileName, ext));
                            collegescmrequests.SCMNotification = scmfileName + ext;
                        }
                        // IUserMailer mailer = new UserMailer();
                        collegescmrequests.CollegeId = userCollegeId;
                        collegescmrequests.SpecializationId = scmrequest.SpecializationId;
                        var specialization = db.jntuh_specialization.AsNoTracking().FirstOrDefault(i => i.id == scmrequest.SpecializationId);
                        var department = db.jntuh_department.AsNoTracking().FirstOrDefault(i => i.id == specialization.departmentId);
                        collegescmrequests.DEpartmentId = specialization != null ? specialization.departmentId : 0;
                        collegescmrequests.DegreeId = department != null ? department.degreeId : 0;
                        collegescmrequests.ProfessorsCount = Convert.ToInt16(scmrequest.ProfessorVacancies);
                        collegescmrequests.AssociateProfessorsCount = Convert.ToInt16(scmrequest.AssociateProfessorVacancies);
                        collegescmrequests.AssistantProfessorsCount = Convert.ToInt16(scmrequest.AssistantProfessorVacancies);
                        collegescmrequests.RequiredProfessor = Convert.ToInt16(scmrequest.RequiredProfessorVacancies);
                        collegescmrequests.RequiredAssistantProfessor = Convert.ToInt16(scmrequest.RequiredAssistantProfessorVacancies);
                        collegescmrequests.RequiredAssociateProfessor = Convert.ToInt16(scmrequest.RequiredAssociateProfessorVacancies);
                        if (scmrequest.NotificationDate != null)
                            collegescmrequests.Notificationdate = UAAAS.Models.Utilities.DDMMYY2MMDDYY(scmrequest.NotificationDate);
                        collegescmrequests.Remarks = scmrequest.Remarks;
                        collegescmrequests.CreatedBy = userId;
                        collegescmrequests.CreatedOn = DateTime.Now;
                        collegescmrequests.ISActive = true;
                        collegescmrequests.TotalNoofFacultyRequired = Convert.ToInt16(scmrequest.TotalFacultyRequired);
                        db.jntuh_appeal_scmproceedingsrequests.Add(collegescmrequests);
                        try
                        {
                            db.SaveChanges();

                            // var attachments = new List<Attachment>();
                            //if (scmrequest.ScmNotificationSupportDoc != null)
                            //{

                            //fileName = Path.GetFileName(scmrequest.ScmNotificationSupportDoc.FileName);
                            //filepath = Path.Combine(Server.MapPath("~/Content/Attachments"), fileName);
                            //scmrequest.ScmNotificationSupportDoc.SaveAs(filepath);
                            //attachments.Add(new Attachment(filepath));
                            //  mailer.SendAttachmentToAllColleges("sureshpalsa1@gmail.com", "", "",
                            //   "SCM PROCEEDINGS REQUEST", "Scm Requests", attachments).SendAsync();
                            TempData["Success"] = "Your request has been proccessed successfully..";
                            //}

                        }
                        catch (DbEntityValidationException dbEx)
                        {
                            foreach (var validationErrors in dbEx.EntityValidationErrors)
                            {
                                foreach (var validationError in validationErrors.ValidationErrors)
                                {
                                    Trace.TraceInformation("Property: {0} Error: {1}", validationError.PropertyName,
                                        validationError.ErrorMessage);
                                }
                            }
                        }
                    }
                }
                else
                {
                    TempData["Error"] = "Please Fill All Mandatory Fields..";
                }
            }
            return RedirectToAction("AppealCollegeScmProceedingsRequest");
        }

        [Authorize(Roles = "College")]
        [HttpGet]
        public ActionResult AppealAddRegistrationNumber(int id)
        {
            ScmProceedingsRequestAddReg scmDetails = new ScmProceedingsRequestAddReg();
            scmDetails = (from a in db.jntuh_appeal_scmproceedingsrequests
                          join b in db.jntuh_college on a.CollegeId equals b.id
                          join c in db.jntuh_specialization on a.SpecializationId equals c.id
                          join d in db.jntuh_department on a.DEpartmentId equals d.id
                          join e in db.jntuh_degree on a.DegreeId equals e.id
                          where a.ID == id
                          select new ScmProceedingsRequestAddReg
                          {
                              CollegeCode = b.collegeCode,
                              CollegeName = b.collegeName,
                              SpecializationId = c.id,
                              SpecializationName = c.specializationName,
                              DepartmentId = d.id,
                              DepartmentName = c.jntuh_department.departmentName,
                              DegreeId = e.id,
                              DegreeName = e.degree,
                              Professors = (int)a.ProfessorsCount,
                              AssociateProfessors = (int)a.AssociateProfessorsCount,
                              AssistantProfessors = (int)a.AssistantProfessorsCount,
                              Id = a.ID
                          }).FirstOrDefault();

            //  var jntuh_college = db.jntuh_college.Where(e => e.isActive == true).Select(e => e).ToList();

            // jntuh_college.Add(new jntuh_college() { id = 0, collegeCode = "Not Working" });

            //  ViewBag.Colleges = jntuh_college.Select(e => new { collegeId = e.id, collegeName = e.collegeCode + "-" + e.collegeName }).OrderBy(e => e.collegeId).ToList();

            //var data = jntuh_college1.Select(e => new SelectListItem() { Value = e.id.ToString(), Text = e.collegeCode + "-" + e.collegeName }).ToList();
            //data.Insert(00, new SelectListItem() { Value = "00", Text = "Not Working", Selected = true });
            //data.Insert(0, new SelectListItem() {  Text = "---Select---",Selected = false});
            //ViewBag.Collegess = data;

            ViewBag.Designations = db.jntuh_designation.Where(e => e.isActive == true && (e.id == 3)).Select(e => new { Id = e.id, Name = e.designation }).OrderBy(e => e.Id).ToList();
            //e.id == 1 || e.id == 2 ||

            return PartialView("_AppealAddRegistrationNumber", scmDetails);
        }

        [Authorize(Roles = "College")]
        [HttpPost]
        public ActionResult AppealAddRegistrationNumber(ScmProceedingsRequestAddReg reg)
        {
            TempData["Error"] = null;
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(collegeUser => collegeUser.userID == userID).Select(collegeUser => collegeUser.collegeID).FirstOrDefault();

            var FindReg = db.jntuh_appeal_scmproceedingsrequests.Join(db.jntuh_appeal_scmproceedingrequest_addfaculty, S => S.ID, A => A.ScmProceedingId, (S, A) => new { S = S, A = A }).Where(e => e.A.RegistrationNumber == reg.RegistrationNo.Trim() && e.S.CollegeId == userCollegeID && e.S.SpecializationId != 0).Select(e => e.A.Id).FirstOrDefault();
            var FacultyCount = db.jntuh_appeal_scmproceedingrequest_addfaculty.Count(e => e.ScmProceedingId == reg.Id);
            //One Course Have only one Request.
            if (FindReg != 0)
            {
                TempData["Error"] = "Already requested for SCM for this faculty in your college.";
            }
            else
            {

                //Checking Maximum Number of Faculty is 5
                //if (FacultyCount < 3)
                //{
                if (ModelState.IsValid)
                {
                    if (reg != null)
                    {
                        jntuh_appeal_scmproceedingrequest_addfaculty addfaculty =
                            new jntuh_appeal_scmproceedingrequest_addfaculty();
                        addfaculty.ScmProceedingId = reg.Id;
                        addfaculty.RegistrationNumber = reg.RegistrationNo.Trim();
                        addfaculty.FacultyType = reg.FacultyId;
                        addfaculty.PreviousCollegeId = reg.PreviousCollegeId.ToString();
                        addfaculty.Createdby = userID;
                        addfaculty.CreatedOn = DateTime.Now;
                        addfaculty.Isactive = true;
                        db.jntuh_appeal_scmproceedingrequest_addfaculty.Add(addfaculty);
                        db.SaveChanges();
                        TempData["Success"] = "Faculty Add Successfully";
                        return RedirectToAction("AppealCollegeScmProceedingsRequest", "CollegeSCMProceedingsRequest");
                    }
                }
                //}
                // else
                //{
                TempData["Error"] = "SCM Request have maximum 5 members only.";
                //}
            }
            return RedirectToAction("AppealCollegeScmProceedingsRequest", "CollegeSCMProceedingsRequest");
        }


        [Authorize(Roles = "College")]
        public ActionResult AppealViewFaculty(int scmid)
        {
            if (scmid != 0)
            {
                List<ScmProceedingsRequestAddReg> addFacultyDetails = new List<ScmProceedingsRequestAddReg>();

                addFacultyDetails = (from a in db.jntuh_appeal_scmproceedingsrequests
                                     join b in db.jntuh_appeal_scmproceedingrequest_addfaculty on a.ID equals b.ScmProceedingId
                                     join c in db.jntuh_registered_faculty on b.RegistrationNumber.Trim() equals c.RegistrationNumber.Trim()
                                     join d in db.jntuh_specialization on a.SpecializationId equals d.id
                                     join e in db.jntuh_degree on a.DegreeId equals e.id
                                     join f in db.jntuh_designation on b.FacultyType equals f.id
                                     where b.ScmProceedingId == scmid
                                     select new ScmProceedingsRequestAddReg
                                     {
                                         Id = b.Id,
                                         SpecializationId = a.SpecializationId,
                                         SpecializationName = e.degree + "-" + d.specializationName,
                                         Regno = b.RegistrationNumber,
                                         RegName = c.FirstName + " " + c.LastName,
                                         ScmId = b.ScmProceedingId,
                                         FacultyId = c.id,
                                         Designation = f.designation,
                                         RequestSubmissionDate = a.RequestSubmittedDate

                                     }).ToList();
                return View(addFacultyDetails);
            }
            return RedirectToAction("AppealCollegeScmProceedingsRequest");
        }


        public ActionResult AppealDeleteRegistrationNumber(int id, int scmId)
        {
            if (id != 0 && scmId != 0)
            {
                var faculydata = db.jntuh_appeal_scmproceedingrequest_addfaculty.Where(e => e.Id == id).Select(e => e).FirstOrDefault();
                if (faculydata != null)
                {
                    db.jntuh_appeal_scmproceedingrequest_addfaculty.Remove(faculydata);
                    db.SaveChanges();
                    TempData["Success"] = "Faculty Deleted Successfully";
                    return RedirectToAction("AppealViewFaculty", "CollegeSCMProceedingsRequest", new { scmid = scmId });
                }
            }
            return RedirectToAction("AppealCollegeScmProceedingsRequest");
        }

        [Authorize(Roles = "College")]
        public ActionResult AllSCMRequestSubmission()
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(collegeUser => collegeUser.userID == userID).Select(collegeUser => collegeUser.collegeID).FirstOrDefault();

            if (userCollegeID != 0)
            {
                var scmRequests = db.jntuh_appeal_scmproceedingsrequests.Where(e => e.CollegeId == userCollegeID && e.SpecializationId != 0 && e.RequestSubmittedDate == null).Select(e => e).ToList();
                foreach (var item in scmRequests)
                {
                    item.RequestSubmittedDate = DateTime.Now;
                    db.Entry(item).State = EntityState.Modified;
                }
                try
                {
                    db.SaveChanges();
                    TempData["Success"] = "Your SCM requests Submisson successfully..";
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "Your SCM requests Submisson Failed..";
                }
            }
            else
            {
                TempData["Error"] = "Your SCM requests Submisson Failed..";
            }

            return RedirectToAction("AppealCollegeScmProceedingsRequest");
        }

        [Authorize(Roles = "College")]
        public ActionResult AppealCollegeScmPrint(ScmProceedingsRequest scmlist)
        {
            if (scmlist.ScmProceedingsRequestslist.Count() != 0)
            {
                serverURL = Request.Url.Scheme + System.Uri.SchemeDelimiter + Request.Url.Host + (Request.Url.IsDefaultPort ? "" : ":" + Request.Url.Port);
                var checkedscmlistdata = scmlist.ScmProceedingsRequestslist.Where(e => e.Checked == true).ToList();
                if (checkedscmlistdata.Count() != 0)
                {
                    var pdfPath = string.Empty;
                    int preview = 0;
                    if (preview == 0)
                    {
                        pdfPath = AppealSaveFacultyDataPdf(preview, checkedscmlistdata);
                        pdfPath = pdfPath.Replace("/", "\\");
                    }
                    return File(pdfPath, "application/pdf", scmlist.ScmProceedingsRequestslist[0].CollegeCode + "- Scm Request File - " + DateTime.Now.ToString("yyyMMddHHmmss") + ".pdf");

                }
                else
                {
                    TempData["Error"] = "Please select any one checkbox for the print";
                    return RedirectToAction("AppealCollegeScmProceedingsRequest");
                }
            }
            return RedirectToAction("AppealCollegeScmProceedingsRequest");
        }

        public string AppealSaveFacultyDataPdf(int preview, List<ScmProceedingsRequest> scmProceedings)
        {
            string fullPath = string.Empty;
            //Set page size as A4
            Document pdfDoc = new Document(PageSize.A4, 60, 50, 60, 60);
            string path = Server.MapPath("~/Content/PDFReports/SCMRequestDownload");
            if (!Directory.Exists(Server.MapPath("~/Content/PDFReports/SCMRequestDownload")))
            {
                Directory.CreateDirectory(Server.MapPath("~/Content/PDFReports/SCMRequestDownload"));
            }

            if (preview == 0)
            {
                fullPath = path + "/" + scmProceedings[0].CollegeCode + "- SCM Request Download -" + DateTime.Now.ToString("yyyMMddHHmmss") + ".pdf";//
                PdfWriter pdfWriter = PdfWriter.GetInstance(pdfDoc, new FileStream(fullPath, FileMode.Create));
                ITextEvents iTextEvents = new ITextEvents();
                iTextEvents.CollegeCode = scmProceedings[0].CollegeCode;
                iTextEvents.CollegeName = scmProceedings[0].CollegeName;
                iTextEvents.formType = "Scm Request Download";
                pdfWriter.PageEvent = iTextEvents;
            }
            //Open PDF Document to write data
            pdfDoc.Open();

            //Assign Html content in a string to write in PDF
            string contents = string.Empty;

            StreamReader sr;

            //Read file from server path
            sr = System.IO.File.OpenText(Server.MapPath("~/Content/SCMRequestDownload.html"));
            //store content in the variable
            contents = sr.ReadToEnd();
            sr.Close();

            contents = contents.Replace("##SERVERURL##", serverURL);


            contents = AppealGetSCMRequestData(scmProceedings, contents);
            //  contents = affiliationType(collegeId, contents);

            //Read string contents using stream reader and convert html to parsed conent
            var parsedHtmlElements = HTMLWorker.ParseToList(new StringReader(contents), null);
            //Get each array values from parsed elements and add to the PDF document
            bool pageRotated = false;
            int count = 0;
            foreach (var htmlElement in parsedHtmlElements)
            {
                count++;
                if (count == 100)
                {

                }
                if (htmlElement.Equals("<textarea>"))
                {
                    pdfDoc.NewPage();
                }

                if (htmlElement.Chunks.Count >= 3)
                {
                    if (htmlElement.Chunks.Count == 4)
                    {
                        pdfDoc.SetPageSize(iTextSharp.text.PageSize.A4.Rotate());
                        pdfDoc.SetMargins(60, 50, 60, 60);
                        pageRotated = true;
                    }
                    else
                    {
                        if (pageRotated == true)
                        {
                            pdfDoc.SetPageSize(iTextSharp.text.PageSize.A4);
                            pdfDoc.SetMargins(60, 50, 60, 60);
                            pageRotated = false;
                        }
                    }

                    pdfDoc.NewPage();

                }
                else
                {
                    pdfDoc.Add(htmlElement as IElement);
                }
            }

            //Close your PDF
            pdfDoc.Close();
            if (pdfDoc.IsOpen())
            {
                pdfDoc.Close();
            }

            string returnPath = string.Empty;
            returnPath = fullPath;
            return returnPath;
        }

        public string AppealGetSCMRequestData(List<ScmProceedingsRequest> scmProceedings, string contents)
        {
            string contentdata = string.Empty;
            int[] scmrequestIds = scmProceedings.Select(e => e.Id).ToArray();
            var jntuh_scmproceeding_add_faculty = db.jntuh_appeal_scmproceedingrequest_addfaculty.Where(e => scmrequestIds.Contains(e.ScmProceedingId)).Select(e => e.ScmProceedingId).Distinct().ToArray();
            var scmdetails = (from a in db.jntuh_appeal_scmproceedingsrequests
                              join b in db.jntuh_college on a.CollegeId equals b.id
                              join c in db.jntuh_specialization on a.SpecializationId equals c.id
                              join d in db.jntuh_department on a.DEpartmentId equals d.id
                              join e in db.jntuh_degree on a.DegreeId equals e.id
                              where scmrequestIds.Contains(a.ID)
                              select new
                              {

                                  CollegeCode = b.collegeCode,
                                  CollegeName = b.collegeName,
                                  SpecializationId = c.id,
                                  SpecializationName = c.specializationName,
                                  //DepartmentId = abcd.id,
                                  //DepartmentName = abcd.departmentName,
                                  DegreeId = e.id,
                                  DegreeName = e.degree,
                                  Professors = a.ProfessorsCount,
                                  AssociateProfessors = a.AssociateProfessorsCount,
                                  AssistantProfessors = a.AssistantProfessorsCount,
                                  RequiredProfessors = a.RequiredProfessor,
                                  RequiredAssociateProfessors = a.RequiredAssociateProfessor,
                                  RequiredAssistantProfessors = a.RequiredAssistantProfessor,
                                  RequestSubmittedDate = a.RequestSubmittedDate
                              }).ToList();

            contentdata += "<br/><p style='text-align:center;font-size:12px'><b> Staff Selection Committee Request </b></p><br/>";
            contentdata += "<table border='0'cellspacing='0' cellpadding='4' width='100%'>";
            contentdata += "<tr><td style='text-align:left'><b>College Name : " + scmdetails[0].CollegeName + "</b></td>";
            contentdata += "<td style='text-align:left'><b>College Code : " + scmdetails[0].CollegeCode + "</b></td></tr>";
            contentdata += "</table>";
            contentdata += "<br/><table border='1'cellspacing='0' cellpadding='4' width='100%'>";
            contentdata += "<tr><td style='text-align:left' width='5%'>S.No</td><td width='25%'>Specialization</td><td width='8%'>Available Prof</td><td width='10%'>Available Assoc.Prof</td><td width='10%'>Available Asst.Prof</td><td width='10%'>Required Prof</td><td width='10%'>Required  Assoc.Prof</td><td width='10%'>Required  Asst.Prof</td><td width='12%'>Req. Submitted Date</td></tr>";

            for (int i = 0; i < scmdetails.Count(); i++)
            {
                string Requestdate = string.Empty;
                if (scmdetails[i].RequestSubmittedDate != null)
                {
                    Requestdate = UAAAS.Models.Utilities.MMDDYY2DDMMYY(scmdetails[i].RequestSubmittedDate.ToString());
                }

                contentdata += "<tr>";
                contentdata += "<td width='5%'>" + (i + 1) + "</td>";
                contentdata += "<td width='25%'>" + scmdetails[i].DegreeName + "-" + scmdetails[i].SpecializationName + "</td>";
                contentdata += "<td width='8%'>" + scmdetails[i].Professors + "</td>";
                contentdata += "<td width='10%'>" + scmdetails[i].AssociateProfessors + "</td>";
                contentdata += "<td width='10%'>" + scmdetails[i].AssistantProfessors + "</td>";
                contentdata += "<td width='10%'>" + scmdetails[i].RequiredProfessors + "</td>";
                contentdata += "<td width='10%'>" + scmdetails[i].RequiredAssociateProfessors + "</td>";
                contentdata += "<td width='10%'>" + scmdetails[i].RequiredAssistantProfessors + "</td>";
                contentdata += "<td width='12%'>" + Requestdate + "</td>";
                contentdata += "</tr>";
            }
            contentdata += "</table>";

            contents = contents.Replace("##SCMDOWLOAD##", contentdata);
            //******* Display the Added Faculty Details *********//

            string FacultyData = string.Empty;
            List<ScmProceedingsRequestAddReg> scmaddedfaculty = new List<ScmProceedingsRequestAddReg>();
            scmaddedfaculty = (from SPR in db.jntuh_appeal_scmproceedingsrequests
                               join SPRF in db.jntuh_appeal_scmproceedingrequest_addfaculty on SPR.ID equals SPRF.ScmProceedingId
                               join RF in db.jntuh_registered_faculty on SPRF.RegistrationNumber equals RF.RegistrationNumber
                               join D in db.jntuh_department on SPR.DEpartmentId equals D.id
                               join S in db.jntuh_specialization on SPR.SpecializationId equals S.id
                               join DG in db.jntuh_degree on SPR.DegreeId equals DG.id
                               where scmrequestIds.Contains(SPR.ID)
                               select new ScmProceedingsRequestAddReg
                               {
                                   Id = (int)SPRF.Id,
                                   SpecializationId = SPR.SpecializationId,
                                   SpecializationName = S.specializationName,//abcde.degree + "-" + abcd.specializationName,
                                   Regno = RF.RegistrationNumber,
                                   RegName = RF.FirstName + " " + RF.LastName,
                                   DegreeName = DG.degree,
                                   ScmId = SPRF.ScmProceedingId
                               }).ToList();


            var specializationIds = scmaddedfaculty.Select(e => e.ScmId).Distinct().ToArray();
            if (specializationIds.Count() > 0)
            {
                foreach (var speid in specializationIds)
                {
                    FacultyData += PrintingDepartmentwiseFaculty(scmaddedfaculty.Where(e => e.ScmId == speid).ToList());
                }
            }


            contents = contents.Replace("##FACULTYDATA##", FacultyData);

            return contents;
        }


        //Principal SCM Request
        [Authorize(Roles = "College")]
        [HttpGet]
        public ActionResult AppealCollegeScmProceedingsPrincipalRequest()
        {
            var pharmacyids = new[]{24,27,30,33,34,44,47,52,55,65,78,83,95,97,104,105,107,110,114,117,118,127,135,136,139,146,150,159,169,202,204,213,219,234,239,253,
                262,263,267,275,283,284,290,295,297,298,301,303,314,317,320,348,353,362,376,377,379,384,392,395,410,427,428,436,442,445,448,454,457,458,6,
                45,54,58,90,120,206,237,302,313,318,319,389,60,66,252,255,315,370,9, 18, 39, 42, 75, 140, 180, 332, 364,375};


            #region SCM Submission Last Date End
            var userId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            var userCollegeId = db.jntuh_college_users.Where(u => u.userID == userId).Select(u => u.collegeID).FirstOrDefault();
            //  ViewBag.Colleges = db.jntuh_college.Where(e => e.isActive == true).Select(e => new { collegeId = e.id, collegeName = e.collegeCode + "-" + e.collegeName }).OrderBy(e => e.collegeName).ToList();
            if (!pharmacyids.Contains(userCollegeId))
            {
                return RedirectToAction("College", "Dashboard");
            }

            var jntuh_college = db.jntuh_college.Where(e => e.isActive == true).Select(e => e).ToList();

            jntuh_college.Add(new jntuh_college() { id = 0, collegeCode = "Not Working" });
            jntuh_college.Add(new jntuh_college() { id = -1, collegeCode = "Others" });

            ViewBag.Colleges = jntuh_college.Select(e => new { collegeId = e.id, collegeName = e.collegeCode + "-" + e.collegeName }).OrderBy(e => e.collegeId).ToList();



            List<SCMPrincipal> scmPrincipaldata = new List<SCMPrincipal>();
            scmPrincipaldata = (from SCMREQ in db.jntuh_appeal_scmproceedingsrequests
                                join SCMADDFLY in db.jntuh_appeal_scmproceedingrequest_addfaculty on SCMREQ.ID equals SCMADDFLY.ScmProceedingId
                                join REGFLY in db.jntuh_registered_faculty on SCMADDFLY.RegistrationNumber equals REGFLY.RegistrationNumber
                                where SCMREQ.CollegeId == userCollegeId && SCMREQ.SpecializationId == 0 && SCMREQ.DegreeId == 0 && SCMREQ.DEpartmentId == 0 && SCMADDFLY.FacultyType == 0
                                select new SCMPrincipal
                                {
                                    RegistrationNo = SCMADDFLY.RegistrationNumber,
                                    FirstName = REGFLY.FirstName + "-" + REGFLY.LastName,
                                    scmnotificationdocview = SCMREQ.SCMNotification,
                                    createdDate = (DateTime)SCMREQ.RequestSubmittedDate,
                                    FacultyId = REGFLY.id,
                                    SCMId = SCMREQ.ID

                                }).OrderByDescending(e => e.createdDate).ToList();
            ViewBag.SCMPrincipal = scmPrincipaldata;
            ViewBag.SCMPrincipalcount = scmPrincipaldata.Count();


            ViewBag.VisiableRequestSumissionButton = db.jntuh_appeal_scmproceedingsrequests.Where(e => e.CollegeId == userCollegeId && e.SpecializationId == 0 && e.DEpartmentId == 0 && e.DegreeId == 0 && e.RequestSubmittedDate == null).Select(e => e).Count();
            #endregion
            ViewBag.editable = false;
            var currentDate = DateTime.Now;
            if (currentDate > new DateTime(2017, 6, 02, 23, 59, 59))//&&    currentDate >= new DateTime(2017, 2, 12, 00, 00, 00)
            {
                ViewBag.editable = true;
            }
            return View();
        }

        [Authorize(Roles = "College")]
        [HttpPost]
        public ActionResult AppealCollegeScmProceedingsPrincipalRequest(SCMPrincipal scmprincipaldata)
        {



            var userId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            var userCollegeId = db.jntuh_college_users.Where(u => u.userID == userId).Select(u => u.collegeID).FirstOrDefault();





            if (ModelState.IsValid)
            {

                var FindReg = db.jntuh_appeal_scmproceedingsrequests.Join(db.jntuh_appeal_scmproceedingrequest_addfaculty, S => S.ID, A => A.ScmProceedingId, (S, A) => new { S = S, A = A }).Where(e => e.A.RegistrationNumber == scmprincipaldata.RegistrationNo.Trim() && e.S.CollegeId == userCollegeId && e.S.SpecializationId == 0).Select(e => e.A.Id).FirstOrDefault();
                if (FindReg != 0)
                {
                    TempData["Error"] = "Faculty already request for SCM in your college.";
                }
                else
                {
                    var jntuh_college = db.jntuh_college.Where(e => e.isActive == true).Select(e => e).ToList();
                    int SCmOrder = 1;
                    var fileName = string.Empty;
                    var filepath = string.Empty;
                    var collegescmrequests = new jntuh_appeal_scmproceedingsrequests();
                    const string scmnotificationpath =
                        "~/Content/Upload/SCMPROCEEDINGSREQUEST/ScmNotificationDocuments";
                    if (scmprincipaldata.ScmNotificationSupportDoc != null)
                    {
                        if (!Directory.Exists(Server.MapPath(scmnotificationpath)))
                        {
                            Directory.CreateDirectory(Server.MapPath(scmnotificationpath));
                        }

                        var ext = Path.GetExtension(scmprincipaldata.ScmNotificationSupportDoc.FileName);
                        if (ext != null && ext.ToUpper().Equals(".PDF"))
                        {
                            var scmfileName =
                                db.jntuh_college.Where(c => c.id == userCollegeId).Select(c => c.id).FirstOrDefault() +
                                "_" + "ScmNotofication" + "_" + DateTime.Now.ToString("yyyMMddHHmmss");
                            scmprincipaldata.ScmNotificationSupportDoc.SaveAs(string.Format("{0}/{1}{2}",
                                Server.MapPath(scmnotificationpath), scmfileName, ext));
                            collegescmrequests.SCMNotification = scmfileName + ext;
                        }
                        IUserMailer mailer = new UserMailer();
                        collegescmrequests.CollegeId = userCollegeId;
                        collegescmrequests.SpecializationId = 0;
                        collegescmrequests.DEpartmentId = 0;
                        collegescmrequests.DegreeId = 0;
                        collegescmrequests.ProfessorsCount = 0;
                        collegescmrequests.AssociateProfessorsCount = 0;
                        collegescmrequests.AssistantProfessorsCount = 0;
                        collegescmrequests.RequiredProfessor = 0;
                        collegescmrequests.RequiredAssistantProfessor = 0;
                        collegescmrequests.RequiredAssociateProfessor = 0;
                        if (scmprincipaldata.NotificationDate != null)
                            collegescmrequests.Notificationdate =
                                UAAAS.Models.Utilities.DDMMYY2MMDDYY(scmprincipaldata.NotificationDate);
                        collegescmrequests.CreatedBy = userId;
                        collegescmrequests.CreatedOn = DateTime.Now;
                        collegescmrequests.ISActive = true;

                        //Checking SCM Order Id
                        var scmdata =
                            db.jntuh_appeal_scmproceedingsrequests.Where(
                                e =>
                                    e.ISActive == true && e.CollegeId == userCollegeId && e.SpecializationId == 0 &&
                                    e.DEpartmentId == 0 && e.DegreeId == 0)
                                .OrderByDescending(e => e.ID)
                                .Select(e => e)
                                .FirstOrDefault();
                        if (scmdata != null)
                        {
                            //Commented on 18-06-2018 by Narayana Reddy
                            //var assigneddata =
                            //    db.jntuh_appeal_auditor_assigned.Where(e => e.ScmId == scmdata.ID)
                            //        .Select(e => e.Id)
                            //        .FirstOrDefault();
                            //if (assigneddata != 0)
                            //{
                            //    SCmOrder = (int) (scmdata.SCMOrder + 1);
                            //}
                            //else
                            //{
                            //    SCmOrder = (int) scmdata.SCMOrder;
                            //}
                        }
                        collegescmrequests.SCMOrder = SCmOrder;
                        collegescmrequests.TotalNoofFacultyRequired = 0;
                        db.jntuh_appeal_scmproceedingsrequests.Add(collegescmrequests);
                        try
                        {
                            db.SaveChanges();
                            int scmId = collegescmrequests.ID;

                            if (scmId != 0)
                            {
                                jntuh_appeal_scmproceedingrequest_addfaculty addfaculty =
                                    new jntuh_appeal_scmproceedingrequest_addfaculty();
                                addfaculty.ScmProceedingId = scmId;
                                addfaculty.RegistrationNumber = scmprincipaldata.RegistrationNo;
                                addfaculty.PreviousCollegeId = scmprincipaldata.PreviousCollegeId.ToString() == "-1"
                                    ? scmprincipaldata.PreviousCollegeName
                                    : scmprincipaldata.PreviousCollegeId != 0
                                        ? jntuh_college.Where(e => e.id == scmprincipaldata.PreviousCollegeId)
                                            .Select(e => e.collegeName)
                                            .FirstOrDefault()
                                        : scmprincipaldata.PreviousCollegeId.ToString();
                                //  scmprincipaldata.PreviousCollegeId.ToString();
                                addfaculty.FacultyType = 0;
                                addfaculty.Createdby = userId;
                                addfaculty.CreatedOn = DateTime.Now;
                                addfaculty.Isactive = true;
                                db.jntuh_appeal_scmproceedingrequest_addfaculty.Add(addfaculty);
                                db.SaveChanges();
                            }




                            //var attachments = new List<Attachment>();
                            //if (scmprincipaldata.ScmNotificationSupportDoc != null)
                            //{

                            //    fileName = Path.GetFileName(scmprincipaldata.ScmNotificationSupportDoc.FileName);
                            //    filepath = Path.Combine(Server.MapPath("~/Content/Attachments"), fileName);
                            //    scmprincipaldata.ScmNotificationSupportDoc.SaveAs(filepath);
                            //    attachments.Add(new Attachment(filepath));
                            //    mailer.SendAttachmentToAllColleges("sureshpalsa1@gmail.com", "", "",
                            //        "SCM PROCEEDINGS REQUEST", "Scm Requests", attachments).SendAsync();
                            //    TempData["Success"] = "Your request has been proccessed successfully..";
                            //}
                            TempData["Success"] = "Your request has been proccessed successfully..";
                        }
                        catch (DbEntityValidationException dbEx)
                        {
                            foreach (var validationErrors in dbEx.EntityValidationErrors)
                            {
                                foreach (var validationError in validationErrors.ValidationErrors)
                                {
                                    Trace.TraceInformation("Property: {0} Error: {1}",
                                        validationError.PropertyName,
                                        validationError.ErrorMessage);
                                }
                            }
                        }
                    }
                    else
                    {
                        TempData["Error"] = "Please Fill All Mandatory Fields..";
                    }
                }
            }
            return RedirectToAction("AppealCollegeScmProceedingsPrincipalRequest");
        }

        [Authorize(Roles = "College")]
        [HttpGet]
        public ActionResult AppealSCMPrincipalRequestSubmission()
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            var userCollegeId = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeId != null)
            {
                List<jntuh_appeal_scmproceedingsrequests> ScmPrincipalList = db.jntuh_appeal_scmproceedingsrequests.Where(e => e.CollegeId == userCollegeId && e.SpecializationId == 0 && e.DEpartmentId == 0 && e.DegreeId == 0 && e.RequestSubmittedDate == null).Select(e => e).ToList();
                if (ScmPrincipalList.Count() != 0)
                {
                    foreach (var data in ScmPrincipalList)
                    {
                        if (data.ID != 0)
                        {
                            var scmPrincipalrequestdata = db.jntuh_appeal_scmproceedingsrequests.Where(e => e.ID == data.ID).Select(e => e).FirstOrDefault();
                            if (scmPrincipalrequestdata != null)
                            {
                                scmPrincipalrequestdata.RequestSubmittedDate = DateTime.Now;
                                db.Entry(scmPrincipalrequestdata).State = EntityState.Modified;
                            }
                            db.SaveChanges();
                            TempData["Success"] = "Submission Successfully.";
                        }
                    }
                }
                else
                {
                    TempData["Error"] = "Please Add Principal SCM Request.";
                }
            }

            return RedirectToAction("AppealCollegeScmProceedingsPrincipalRequest", "CollegeScmProceedingsRequest");
        }




        #endregion

        [Authorize(Roles = "College")]
        [HttpGet]
        public ActionResult CollegePrincipalDetailsAffColl(string RegId)
        {
            var todayDate = DateTime.Now;
            var userId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            var userCollegeId = db.jntuh_college_users.Where(u => u.userID == userId).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeId == 375)
            {
                userCollegeId = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int ay0 = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();
            int lid =
                db.jntuh_link_screens.Where(p => p.linkName == "Principal SCM Request" && p.isActive == true).Select(s => s.id).FirstOrDefault();
            jntuh_college_links_assigned scmphase =
                db.jntuh_college_links_assigned.Where(l => l.linkId == lid && l.academicyearId == ay0 && l.isActive == true && l.fromdate <= todayDate && l.todate >= todayDate).Select(s => s).FirstOrDefault();
            if (scmphase == null && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("College", "Dashboard");
            }
            var regNo = UAAAS.Models.Utilities.DecryptString(RegId, WebConfigurationManager.AppSettings["CryptoKey"]);
            var principaldetails = db.jntuh_principal_details_affiliated_colleges.FirstOrDefault(i => i.registrationnumber == regNo && i.collegeid == userCollegeId);
            if (principaldetails != null)
            {
                return RedirectToAction("CollegeScmProceedingsPrincipalRequest", "CollegeSCMProceedingsRequest");
            }
            var PrincipalAppliedDetails = new PrincipalAppliedDetails();
            if (!string.IsNullOrEmpty(regNo))
            {
                var collegeData = db.jntuh_college.Where(i => i.id == userCollegeId).FirstOrDefault();
                var regFaculty = db.jntuh_registered_faculty.Where(i => i.RegistrationNumber == regNo).FirstOrDefault();
                PrincipalAppliedDetails.RegistrationNumber = regNo;
                PrincipalAppliedDetails.FacultyName = regFaculty.FirstName + " " + regFaculty.MiddleName + " " + regFaculty.LastName;
                PrincipalAppliedDetails.FacultyName = PrincipalAppliedDetails.FacultyName.ToUpper();
                PrincipalAppliedDetails.WorkingCollege = collegeData.collegeName + "(" + collegeData.collegeCode + ")";
            }
            return View(PrincipalAppliedDetails);
        }

        [Authorize(Roles = "College")]
        [HttpPost]
        public ActionResult CollegePrincipalDetailsAffColl(PrincipalAppliedDetails principalDetails)
        {
            var todayDate = DateTime.Now;
            var userId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            var userCollegeId = db.jntuh_college_users.Where(u => u.userID == userId).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeId == 375)
            {
                userCollegeId = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int ay0 = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();
            int lid =
                db.jntuh_link_screens.Where(p => p.linkName == "Principal SCM Request" && p.isActive == true).Select(s => s.id).FirstOrDefault();
            jntuh_college_links_assigned scmphase =
                db.jntuh_college_links_assigned.Where(l => l.linkId == lid && l.academicyearId == ay0 && l.isActive == true && l.fromdate <= todayDate && l.todate >= todayDate).Select(s => s).FirstOrDefault();
            if (scmphase == null && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("College", "Dashboard");
            }
            var principaldetailsalreadyfilled = db.jntuh_principal_details_affiliated_colleges.Where(i => i.registrationnumber == principalDetails.RegistrationNumber && i.collegeid == userCollegeId).Count();
            if (principaldetailsalreadyfilled > 0)
            {
                TempData["Error"] = "This Registration Number already applied for Principal details from your college.";
            }
            try
            {
                //if (ModelState.IsValid)
                if (!string.IsNullOrEmpty(principalDetails.RegistrationNumber))
                {
                    var collegeCode = db.jntuh_college.Where(c => c.id == userCollegeId).Select(c => c.collegeCode).FirstOrDefault();
                    var collegePrincipaldetails = new jntuh_principal_details_affiliated_colleges()
                    {
                        academicyearid = ay0,
                        collegeid = userCollegeId,
                        registrationnumber = principalDetails.RegistrationNumber,
                        collegetype = principalDetails.CollegeType == "1" ? "Engineering" : "Pharmacy",
                        ugdivision = !string.IsNullOrEmpty(principalDetails.UgDivision) ? Convert.ToInt16(principalDetails.UgDivision) : 0,
                        ugpassedyear = !string.IsNullOrEmpty(principalDetails.UgPassingYear) ? Convert.ToInt16(principalDetails.UgPassingYear) : 0,
                        ugodproof = principalDetails.UgODProofName,
                        pgdivision = !string.IsNullOrEmpty(principalDetails.PgDivision) ? Convert.ToInt16(principalDetails.PgDivision) : 0,
                        pgpassedyear = !string.IsNullOrEmpty(principalDetails.PgPassingYear) ? Convert.ToInt16(principalDetails.PgPassingYear) : 0,
                        pgodproof = principalDetails.PgODProofName,
                        phddivision = !string.IsNullOrEmpty(principalDetails.PhdDivision) ? Convert.ToInt16(principalDetails.PhdDivision) : 0,
                        phdpassedyear = !string.IsNullOrEmpty(principalDetails.PhdPassingYear) ? Convert.ToInt16(principalDetails.PhdPassingYear) : 0,
                        phdodproof = principalDetails.PhdODProofName,
                        ugdegree = principalDetails.UgDegree.ToUpper(),
                        pgdegree = principalDetails.PgDegree.ToUpper(),
                        phddegree = principalDetails.PhDArea.ToUpper(),

                        lecturernoofyears = !string.IsNullOrEmpty(principalDetails.LectYears) ? Convert.ToDecimal(principalDetails.LectYears) : 0,
                        lecturersupportingdocument = principalDetails.LectSuppDocName,
                        associateprofnoofyears = !string.IsNullOrEmpty(principalDetails.AssProfYears) ? Convert.ToDecimal(principalDetails.AssProfYears) : 0,
                        associateprofsupportingdoc = principalDetails.AssProfSuppDocName,
                        professornoofyears = !string.IsNullOrEmpty(principalDetails.ProfYears) ? Convert.ToDecimal(principalDetails.ProfYears) : 0,
                        professorsupportingdoc = principalDetails.ProfSuppDocName,
                        principalnoofyears = !string.IsNullOrEmpty(principalDetails.PrincipalYears) ? Convert.ToDecimal(principalDetails.PrincipalYears) : 0,
                        principalsupportingdoc = principalDetails.PrincipalSuppDocName,
                        researchnoofyears = !string.IsNullOrEmpty(principalDetails.ResearchYears) ? Convert.ToDecimal(principalDetails.ResearchYears) : 0,
                        researchsupportingdoc = principalDetails.ResearchSuppDocName,
                        industrynoofyears = !string.IsNullOrEmpty(principalDetails.IndustryYears) ? Convert.ToDecimal(principalDetails.IndustryYears) : 0,
                        industrysupportingdoc = principalDetails.IndustrySuppDocName,
                        totalexperience = !string.IsNullOrEmpty(principalDetails.TotalExperience) ? Convert.ToDecimal(principalDetails.TotalExperience) : 0,

                        internationaljournals = !string.IsNullOrEmpty(principalDetails.InterJornals) ? Convert.ToDecimal(principalDetails.InterJornals) : 0,
                        internationaljournalsdoc = principalDetails.InterJornalsSuppDocName,
                        internationalconferences = !string.IsNullOrEmpty(principalDetails.InterConfs) ? Convert.ToDecimal(principalDetails.InterConfs) : 0,
                        internationalconferencesdoc = principalDetails.InterConfsSuppDocName,
                        nationaljournals = !string.IsNullOrEmpty(principalDetails.NationalJournals) ? Convert.ToDecimal(principalDetails.NationalJournals) : 0,
                        nationaljournalsdoc = principalDetails.NationalJournalsSuppDocName,
                        nationalconferences = !string.IsNullOrEmpty(principalDetails.NationalConfs) ? Convert.ToDecimal(principalDetails.NationalConfs) : 0,
                        nationalconferencesdoc = principalDetails.NationalConfsSuppDocName,

                        supervisorphdguided = !string.IsNullOrEmpty(principalDetails.Supervisors) ? Convert.ToDecimal(principalDetails.Supervisors) : 0,
                        supervisorphdguideddoc = principalDetails.SupervisorsSuppDocName,
                        supervisorguideduniversity = principalDetails.SupervisorsGuidedUniversity,
                        cosupervisorphdguided = !string.IsNullOrEmpty(principalDetails.CoSupervisors) ? Convert.ToDecimal(principalDetails.CoSupervisors) : 0,
                        cosupervisorphdguideddoc = principalDetails.CoSupervisorsSuppDocName,
                        totalpublications = !string.IsNullOrEmpty(principalDetails.TotalPublications) ? Convert.ToDecimal(principalDetails.TotalPublications) : 0,
                        totalphdsguided = !string.IsNullOrEmpty(principalDetails.TotalPhDsGuided) ? Convert.ToDecimal(principalDetails.TotalPhDsGuided) : 0,
                        cosupervisorguideduniversity = principalDetails.CoSupervisorsGuidedUniversity,

                        assistantprofratactivitystatus = principalDetails.LectRatactivitystatus == true ? "Yes" : "No",
                        assistantprofratifieddate = !string.IsNullOrEmpty(principalDetails.LectRatDate) ? Convert.ToDateTime(UAAAS.Models.Utilities.MMDDYY2DDMMYY(principalDetails.LectRatDate)) : Convert.ToDateTime("01/01/1900"),
                        assistantprofratnoofyears = !string.IsNullOrEmpty(principalDetails.LectRatYears) ? Convert.ToDecimal(principalDetails.LectRatYears) : 0,
                        assistantprofratsuptdoc = principalDetails.LectRatSuppDocName,

                        associateprofratactivitystatus = principalDetails.AssRatactivitystatus == true ? "Yes" : "No",
                        associateprofratifieddate = !string.IsNullOrEmpty(principalDetails.AssRatDate) ? Convert.ToDateTime(UAAAS.Models.Utilities.MMDDYY2DDMMYY(principalDetails.AssRatDate)) : Convert.ToDateTime("01/01/1900"),
                        associateprofratnoofyears = !string.IsNullOrEmpty(principalDetails.AssRatYears) ? Convert.ToDecimal(principalDetails.AssRatYears) : 0,
                        associateprofratsuptdoc = principalDetails.AssRatProfSuppDocName,

                        professorratactivitystatus = principalDetails.ProfRatactivitystatus == true ? "Yes" : "No",
                        professorratifieddate = !string.IsNullOrEmpty(principalDetails.ProfRatDate) ? Convert.ToDateTime(UAAAS.Models.Utilities.MMDDYY2DDMMYY(principalDetails.ProfRatDate)) : Convert.ToDateTime("01/01/1900"),
                        professorratnoofyears = !string.IsNullOrEmpty(principalDetails.ProfRatYears) ? Convert.ToDecimal(principalDetails.ProfRatYears) : 0,
                        professorratsuptdoc = principalDetails.ProfRatSuppDocName,

                        principalratactivitystatus = principalDetails.PrincipalRatactivitystatus == true ? "Yes" : "No",
                        principalratifieddate = !string.IsNullOrEmpty(principalDetails.PrincipalRatDate) ? Convert.ToDateTime(UAAAS.Models.Utilities.MMDDYY2DDMMYY(principalDetails.PrincipalRatDate)) : Convert.ToDateTime("01/01/1900"),
                        principalratnoofyears = !string.IsNullOrEmpty(principalDetails.PrincipalRatYears) ? Convert.ToDecimal(principalDetails.PrincipalRatYears) : 0,
                        principalratsuptdoc = principalDetails.PrincipalRatSuppDocName,
                        totalratifiedservice = !string.IsNullOrEmpty(principalDetails.TotalRatExperience) ? Convert.ToDecimal(principalDetails.TotalRatExperience) : 0,

                        createdon = DateTime.Now,
                        createdby = userId
                    };

                    if (principalDetails.UgODProof != null)
                    {
                        var affiliationfile = "~/Content/Upload/SCMPROCEEDINGSREQUEST/Principal_Details/";
                        if (!Directory.Exists(Server.MapPath(affiliationfile)))
                        {
                            Directory.CreateDirectory(Server.MapPath(affiliationfile));
                        }
                        var ext = Path.GetExtension(principalDetails.UgODProof.FileName);
                        if (ext.ToUpper().Equals(".PDF") || ext.ToUpper().Equals(".PDF"))
                        {
                            string fileName = collegeCode + "_UGODProof_" + DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "_" + principalDetails.RegistrationNumber;
                            principalDetails.UgODProof.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(affiliationfile), fileName, ext));
                            principalDetails.UgODProofName = string.Format("{0}{1}", fileName, ext);
                            collegePrincipaldetails.ugodproof = principalDetails.UgODProofName;
                        }
                    }

                    if (principalDetails.PgODProof != null)
                    {
                        var affiliationfile = "~/Content/Upload/SCMPROCEEDINGSREQUEST/Principal_Details/";
                        if (!Directory.Exists(Server.MapPath(affiliationfile)))
                        {
                            Directory.CreateDirectory(Server.MapPath(affiliationfile));
                        }
                        var ext = Path.GetExtension(principalDetails.PgODProof.FileName);
                        if (ext.ToUpper().Equals(".PDF") || ext.ToUpper().Equals(".PDF"))
                        {
                            string fileName = collegeCode + "_PGODProof_" + DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "_" + principalDetails.RegistrationNumber;
                            principalDetails.PgODProof.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(affiliationfile), fileName, ext));
                            principalDetails.PgODProofName = string.Format("{0}{1}", fileName, ext);
                            collegePrincipaldetails.pgodproof = principalDetails.PgODProofName;
                        }
                    }

                    if (principalDetails.PhDODProof != null)
                    {
                        var affiliationfile = "~/Content/Upload/SCMPROCEEDINGSREQUEST/Principal_Details/";
                        if (!Directory.Exists(Server.MapPath(affiliationfile)))
                        {
                            Directory.CreateDirectory(Server.MapPath(affiliationfile));
                        }
                        var ext = Path.GetExtension(principalDetails.PhDODProof.FileName);
                        if (ext.ToUpper().Equals(".PDF") || ext.ToUpper().Equals(".PDF"))
                        {
                            string fileName = collegeCode + "_PhDODProof_" + DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "_" + principalDetails.RegistrationNumber;
                            principalDetails.PhDODProof.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(affiliationfile), fileName, ext));
                            principalDetails.PhdODProofName = string.Format("{0}{1}", fileName, ext);
                            collegePrincipaldetails.phdodproof = principalDetails.PhdODProofName;
                        }
                    }

                    if (principalDetails.LectSuppDoc != null)
                    {
                        var affiliationfile = "~/Content/Upload/SCMPROCEEDINGSREQUEST/Principal_Details/";
                        if (!Directory.Exists(Server.MapPath(affiliationfile)))
                        {
                            Directory.CreateDirectory(Server.MapPath(affiliationfile));
                        }
                        var ext = Path.GetExtension(principalDetails.LectSuppDoc.FileName);
                        if (ext.ToUpper().Equals(".PDF") || ext.ToUpper().Equals(".PDF"))
                        {
                            string fileName = collegeCode + "_LecturerExp_" + DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "_" + principalDetails.RegistrationNumber;
                            principalDetails.LectSuppDoc.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(affiliationfile), fileName, ext));
                            principalDetails.LectSuppDocName = string.Format("{0}{1}", fileName, ext);
                            collegePrincipaldetails.lecturersupportingdocument = principalDetails.LectSuppDocName;
                        }
                    }
                    if (principalDetails.AssProfSuppDoc != null)
                    {
                        var affiliationfile = "~/Content/Upload/SCMPROCEEDINGSREQUEST/Principal_Details/";
                        if (!Directory.Exists(Server.MapPath(affiliationfile)))
                        {
                            Directory.CreateDirectory(Server.MapPath(affiliationfile));
                        }
                        var ext = Path.GetExtension(principalDetails.AssProfSuppDoc.FileName);
                        if (ext.ToUpper().Equals(".PDF") || ext.ToUpper().Equals(".PDF"))
                        {
                            string fileName = collegeCode + "_AssociateProfExp_" + DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "_" + principalDetails.RegistrationNumber;
                            principalDetails.AssProfSuppDoc.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(affiliationfile), fileName, ext));
                            principalDetails.AssProfSuppDocName = string.Format("{0}{1}", fileName, ext);
                            collegePrincipaldetails.associateprofsupportingdoc = principalDetails.AssProfSuppDocName;
                        }
                    }
                    if (principalDetails.ProfSuppDoc != null)
                    {
                        var affiliationfile = "~/Content/Upload/SCMPROCEEDINGSREQUEST/Principal_Details/";
                        if (!Directory.Exists(Server.MapPath(affiliationfile)))
                        {
                            Directory.CreateDirectory(Server.MapPath(affiliationfile));
                        }
                        var ext = Path.GetExtension(principalDetails.ProfSuppDoc.FileName);
                        if (ext.ToUpper().Equals(".PDF") || ext.ToUpper().Equals(".PDF"))
                        {
                            string fileName = collegeCode + "_ProfessorExp_" + DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "_" + principalDetails.RegistrationNumber;
                            principalDetails.ProfSuppDoc.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(affiliationfile), fileName, ext));
                            principalDetails.ProfSuppDocName = string.Format("{0}{1}", fileName, ext);
                            collegePrincipaldetails.professorsupportingdoc = principalDetails.ProfSuppDocName;
                        }
                    }
                    if (principalDetails.PrincipalSuppDoc != null)
                    {
                        var affiliationfile = "~/Content/Upload/SCMPROCEEDINGSREQUEST/Principal_Details/";
                        if (!Directory.Exists(Server.MapPath(affiliationfile)))
                        {
                            Directory.CreateDirectory(Server.MapPath(affiliationfile));
                        }
                        var ext = Path.GetExtension(principalDetails.PrincipalSuppDoc.FileName);
                        if (ext.ToUpper().Equals(".PDF") || ext.ToUpper().Equals(".PDF"))
                        {
                            string fileName = collegeCode + "_PrincipalExp_" + DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "_" + principalDetails.RegistrationNumber;
                            principalDetails.PrincipalSuppDoc.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(affiliationfile), fileName, ext));
                            principalDetails.PrincipalSuppDocName = string.Format("{0}{1}", fileName, ext);
                            collegePrincipaldetails.principalsupportingdoc = principalDetails.PrincipalSuppDocName;
                        }
                    }
                    if (principalDetails.ResearchSuppDoc != null)
                    {
                        var affiliationfile = "~/Content/Upload/SCMPROCEEDINGSREQUEST/Principal_Details/";
                        if (!Directory.Exists(Server.MapPath(affiliationfile)))
                        {
                            Directory.CreateDirectory(Server.MapPath(affiliationfile));
                        }
                        var ext = Path.GetExtension(principalDetails.ResearchSuppDoc.FileName);
                        if (ext.ToUpper().Equals(".PDF") || ext.ToUpper().Equals(".PDF"))
                        {
                            string fileName = collegeCode + "_ReasearchExp_" + DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "_" + principalDetails.RegistrationNumber;
                            principalDetails.ResearchSuppDoc.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(affiliationfile), fileName, ext));
                            principalDetails.ResearchSuppDocName = string.Format("{0}{1}", fileName, ext);
                            collegePrincipaldetails.researchsupportingdoc = principalDetails.ResearchSuppDocName;
                        }
                    }
                    if (principalDetails.IndustrySuppDoc != null)
                    {
                        var affiliationfile = "~/Content/Upload/SCMPROCEEDINGSREQUEST/Principal_Details/";
                        if (!Directory.Exists(Server.MapPath(affiliationfile)))
                        {
                            Directory.CreateDirectory(Server.MapPath(affiliationfile));
                        }
                        var ext = Path.GetExtension(principalDetails.IndustrySuppDoc.FileName);
                        if (ext.ToUpper().Equals(".PDF") || ext.ToUpper().Equals(".PDF"))
                        {
                            string fileName = collegeCode + "_IndustryExp_" + DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "_" + principalDetails.RegistrationNumber;
                            principalDetails.IndustrySuppDoc.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(affiliationfile), fileName, ext));
                            principalDetails.IndustrySuppDocName = string.Format("{0}{1}", fileName, ext);
                            collegePrincipaldetails.industrysupportingdoc = principalDetails.IndustrySuppDocName;
                        }
                    }

                    if (principalDetails.InterConfsSuppDoc != null)
                    {
                        var affiliationfile = "~/Content/Upload/SCMPROCEEDINGSREQUEST/Principal_Details/";
                        if (!Directory.Exists(Server.MapPath(affiliationfile)))
                        {
                            Directory.CreateDirectory(Server.MapPath(affiliationfile));
                        }
                        var ext = Path.GetExtension(principalDetails.InterConfsSuppDoc.FileName);
                        if (ext.ToUpper().Equals(".PDF") || ext.ToUpper().Equals(".PDF"))
                        {
                            string fileName = collegeCode + "_InternationalConf_" + DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "_" + principalDetails.RegistrationNumber;
                            principalDetails.InterConfsSuppDoc.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(affiliationfile), fileName, ext));
                            principalDetails.InterConfsSuppDocName = string.Format("{0}{1}", fileName, ext);
                            collegePrincipaldetails.internationalconferencesdoc = principalDetails.InterConfsSuppDocName;
                        }
                    }
                    if (principalDetails.InterJornalsSuppDoc != null)
                    {
                        var affiliationfile = "~/Content/Upload/SCMPROCEEDINGSREQUEST/Principal_Details/";
                        if (!Directory.Exists(Server.MapPath(affiliationfile)))
                        {
                            Directory.CreateDirectory(Server.MapPath(affiliationfile));
                        }
                        var ext = Path.GetExtension(principalDetails.InterJornalsSuppDoc.FileName);
                        if (ext.ToUpper().Equals(".PDF") || ext.ToUpper().Equals(".PDF"))
                        {
                            string fileName = collegeCode + "_InternationalJourn_" + DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "_" + principalDetails.RegistrationNumber;
                            principalDetails.InterJornalsSuppDoc.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(affiliationfile), fileName, ext));
                            principalDetails.InterJornalsSuppDocName = string.Format("{0}{1}", fileName, ext);
                            collegePrincipaldetails.internationaljournalsdoc = principalDetails.InterJornalsSuppDocName;
                        }
                    }
                    if (principalDetails.NationalConfsSuppDoc != null)
                    {
                        var affiliationfile = "~/Content/Upload/SCMPROCEEDINGSREQUEST/Principal_Details/";
                        if (!Directory.Exists(Server.MapPath(affiliationfile)))
                        {
                            Directory.CreateDirectory(Server.MapPath(affiliationfile));
                        }
                        var ext = Path.GetExtension(principalDetails.NationalConfsSuppDoc.FileName);
                        if (ext.ToUpper().Equals(".PDF") || ext.ToUpper().Equals(".PDF"))
                        {
                            string fileName = collegeCode + "_NationalConf_" + DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "_" + principalDetails.RegistrationNumber;
                            principalDetails.NationalConfsSuppDoc.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(affiliationfile), fileName, ext));
                            principalDetails.NationalConfsSuppDocName = string.Format("{0}{1}", fileName, ext);
                            collegePrincipaldetails.nationalconferencesdoc = principalDetails.NationalConfsSuppDocName;
                        }
                    }
                    if (principalDetails.NationalJournalsSuppDoc != null)
                    {
                        var affiliationfile = "~/Content/Upload/SCMPROCEEDINGSREQUEST/Principal_Details/";
                        if (!Directory.Exists(Server.MapPath(affiliationfile)))
                        {
                            Directory.CreateDirectory(Server.MapPath(affiliationfile));
                        }
                        var ext = Path.GetExtension(principalDetails.NationalJournalsSuppDoc.FileName);
                        if (ext.ToUpper().Equals(".PDF") || ext.ToUpper().Equals(".PDF"))
                        {
                            string fileName = collegeCode + "_NationalJourn_" + DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "_" + principalDetails.RegistrationNumber;
                            principalDetails.NationalJournalsSuppDoc.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(affiliationfile), fileName, ext));
                            principalDetails.NationalJournalsSuppDocName = string.Format("{0}{1}", fileName, ext);
                            collegePrincipaldetails.nationaljournalsdoc = principalDetails.NationalJournalsSuppDocName;
                        }
                    }
                    if (principalDetails.SupervisorsSuppDoc != null)
                    {
                        var affiliationfile = "~/Content/Upload/SCMPROCEEDINGSREQUEST/Principal_Details/";
                        if (!Directory.Exists(Server.MapPath(affiliationfile)))
                        {
                            Directory.CreateDirectory(Server.MapPath(affiliationfile));
                        }
                        var ext = Path.GetExtension(principalDetails.SupervisorsSuppDoc.FileName);
                        if (ext.ToUpper().Equals(".PDF") || ext.ToUpper().Equals(".PDF"))
                        {
                            string fileName = collegeCode + "_SupervisorsPhd_" + DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "_" + principalDetails.RegistrationNumber;
                            principalDetails.SupervisorsSuppDoc.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(affiliationfile), fileName, ext));
                            principalDetails.SupervisorsSuppDocName = string.Format("{0}{1}", fileName, ext);
                            collegePrincipaldetails.supervisorphdguideddoc = principalDetails.SupervisorsSuppDocName;
                        }
                    }
                    if (principalDetails.CoSupervisorsSuppDoc != null)
                    {
                        var affiliationfile = "~/Content/Upload/SCMPROCEEDINGSREQUEST/Principal_Details/";
                        if (!Directory.Exists(Server.MapPath(affiliationfile)))
                        {
                            Directory.CreateDirectory(Server.MapPath(affiliationfile));
                        }
                        var ext = Path.GetExtension(principalDetails.CoSupervisorsSuppDoc.FileName);
                        if (ext.ToUpper().Equals(".PDF") || ext.ToUpper().Equals(".PDF"))
                        {
                            string fileName = collegeCode + "_CoSupervisorsPhd_" + DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "_" + principalDetails.RegistrationNumber;
                            principalDetails.CoSupervisorsSuppDoc.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(affiliationfile), fileName, ext));
                            principalDetails.CoSupervisorsSuppDocName = string.Format("{0}{1}", fileName, ext);
                            collegePrincipaldetails.cosupervisorphdguideddoc = principalDetails.CoSupervisorsSuppDocName;
                        }
                    }
                    if (principalDetails.LectRatSuppDoc != null)
                    {
                        var affiliationfile = "~/Content/Upload/SCMPROCEEDINGSREQUEST/Principal_Details/";
                        if (!Directory.Exists(Server.MapPath(affiliationfile)))
                        {
                            Directory.CreateDirectory(Server.MapPath(affiliationfile));
                        }
                        var ext = Path.GetExtension(principalDetails.LectRatSuppDoc.FileName);
                        if (ext.ToUpper().Equals(".PDF") || ext.ToUpper().Equals(".PDF"))
                        {
                            string fileName = collegeCode + "_AssistantRatified_" + DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "_" + principalDetails.RegistrationNumber;
                            principalDetails.LectRatSuppDoc.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(affiliationfile), fileName, ext));
                            principalDetails.LectRatSuppDocName = string.Format("{0}{1}", fileName, ext);
                            collegePrincipaldetails.assistantprofratsuptdoc = principalDetails.LectRatSuppDocName;
                        }
                    }

                    if (principalDetails.AssRatProfSuppDoc != null)
                    {
                        var affiliationfile = "~/Content/Upload/SCMPROCEEDINGSREQUEST/Principal_Details/";
                        if (!Directory.Exists(Server.MapPath(affiliationfile)))
                        {
                            Directory.CreateDirectory(Server.MapPath(affiliationfile));
                        }
                        var ext = Path.GetExtension(principalDetails.AssRatProfSuppDoc.FileName);
                        if (ext.ToUpper().Equals(".PDF") || ext.ToUpper().Equals(".PDF"))
                        {
                            string fileName = collegeCode + "_AssociateRatified_" + DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "_" + principalDetails.RegistrationNumber;
                            principalDetails.AssRatProfSuppDoc.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(affiliationfile), fileName, ext));
                            principalDetails.AssRatProfSuppDocName = string.Format("{0}{1}", fileName, ext);
                            collegePrincipaldetails.associateprofratsuptdoc = principalDetails.AssRatProfSuppDocName;
                        }
                    }

                    if (principalDetails.ProfRatSuppDoc != null)
                    {
                        var affiliationfile = "~/Content/Upload/SCMPROCEEDINGSREQUEST/Principal_Details/";
                        if (!Directory.Exists(Server.MapPath(affiliationfile)))
                        {
                            Directory.CreateDirectory(Server.MapPath(affiliationfile));
                        }
                        var ext = Path.GetExtension(principalDetails.ProfRatSuppDoc.FileName);
                        if (ext.ToUpper().Equals(".PDF") || ext.ToUpper().Equals(".PDF"))
                        {
                            string fileName = collegeCode + "_ProfessorRatified_" + DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "_" + principalDetails.RegistrationNumber;
                            principalDetails.ProfRatSuppDoc.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(affiliationfile), fileName, ext));
                            principalDetails.ProfRatSuppDocName = string.Format("{0}{1}", fileName, ext);
                            collegePrincipaldetails.professorratsuptdoc = principalDetails.ProfRatSuppDocName;
                        }
                    }

                    if (principalDetails.PrincipalRatSuppDoc != null)
                    {
                        var affiliationfile = "~/Content/Upload/SCMPROCEEDINGSREQUEST/Principal_Details/";
                        if (!Directory.Exists(Server.MapPath(affiliationfile)))
                        {
                            Directory.CreateDirectory(Server.MapPath(affiliationfile));
                        }
                        var ext = Path.GetExtension(principalDetails.PrincipalRatSuppDoc.FileName);
                        if (ext.ToUpper().Equals(".PDF") || ext.ToUpper().Equals(".PDF"))
                        {
                            string fileName = collegeCode + "_ProfessorRatified_" + DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "_" + principalDetails.RegistrationNumber;
                            principalDetails.PrincipalRatSuppDoc.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(affiliationfile), fileName, ext));
                            principalDetails.PrincipalRatSuppDocName = string.Format("{0}{1}", fileName, ext);
                            collegePrincipaldetails.principalratsuptdoc = principalDetails.PrincipalRatSuppDocName;
                        }
                    }

                    db.jntuh_principal_details_affiliated_colleges.Add(collegePrincipaldetails);
                    db.SaveChanges();
                    if (true)
                    {
                        TempData["Success"] = "Your request has been proccessed successfully..";
                    }
                }
                else
                {
                    var query = ModelState.Values.SelectMany(x => x.Errors).ToArray();
                    //var errors = query.ToArray();
                }
            }
            catch (DbEntityValidationException dbEx)
            {
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        Trace.TraceInformation("Property: {0} Error: {1}",
                            validationError.PropertyName,
                            validationError.ErrorMessage);
                    }
                }
            }
            //return RedirectToAction("CollegePrincipalDetailsAffColl", new { RegId = UAAAS.Models.Utilities.EncryptString(principalDetails.RegistrationNumber, System.Web.Configuration.WebConfigurationManager.AppSettings["CryptoKey"]) });
            return RedirectToAction("CollegeScmProceedingsPrincipalRequest", "CollegeSCMProceedingsRequest");
        }

        #region Principal details print pdf

        public ActionResult PrincipalDetails(string strfacultyId, string reg)
        {
            if (!string.IsNullOrEmpty(strfacultyId))
            {
                int fid =
                    Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(strfacultyId,
                        WebConfigurationManager.AppSettings["CryptoKey"]));
                // fid = 125662;
                string pdfPath = string.Empty;
                if (fid != 0 && reg != null)
                {
                    pdfPath = SavePrincipalDataPdf(fid, reg);
                    pdfPath = pdfPath.Replace("/", "\\");
                }
                return File(pdfPath, "application/pdf", reg + ".pdf");
            }
            else
            {
                return RedirectToAction("LogOn", "Account");
            }
        }

        public string SavePrincipalDataPdf(int fid, string reg)
        {
            string fullPath = string.Empty;
            //Set page size as A4
            Document pdfDoc = new Document(PageSize.A4, 50, 50, 40, 40);
            string path = Server.MapPath("~/Content/PDFReports/temp/PrincipalDetailsPrint");
            if (!Directory.Exists(Server.MapPath("~/Content/PDFReports/temp/PrincipalDetailsPrint")))
            {
                Directory.CreateDirectory(Server.MapPath("~/Content/PDFReports/temp/PrincipalDetailsPrint"));
            }
            const int DelayOnRetry = 3000;
            try
            {
                //if (preview == 0)
                //{
                //fullPath = path + "/" + "" + ".pdf"; //
                //PdfWriter pdfWriter = PdfWriter.GetInstance(pdfDoc, new FileStream(fullPath, FileMode.Create));
                //ITextEvents iTextEvents = new ITextEvents();
                //iTextEvents.CollegeCode = "";
                //iTextEvents.CollegeName = "";
                //iTextEvents.formType = "Faculty Information";
                //  pdfWriter.PageEvent = "";
                //}


                fullPath = path + "/" + "Principal_Details_Print_" + reg + DateTime.Now.ToString("yyyMMddHHmmss") + ".pdf";
                //   fullPath = path + "/" + "Faculty_Data_Print_" + "" + DateTime.Now.ToString("yyyMMddHHmmss") + ".pdf"; //
                PdfWriter pdfWriter = PdfWriter.GetInstance(pdfDoc, new FileStream(fullPath, FileMode.Create));
                ITextEvents iTextEvents = new ITextEvents();
                //   var objtest = iTextEvents.OnEndPage;
                //TextLayout iTextLayout=new TextLayout();
                //pdfWriter.PageNumber = "1";
                iTextEvents.formType = "Faculty Information";
                //  pdfWriter.PageEvent = "";



            }
            catch (IOException e)
            {
                Thread.Sleep(DelayOnRetry);
            }

            //Open PDF Document to write data
            pdfDoc.Open();

            //Assign Html content in a string to write in PDF
            string contents = string.Empty;

            StreamReader sr;

            //Read file from server path
            sr = System.IO.File.OpenText(Server.MapPath("~/Content/FacultyData.html"));
            //store content in the variable
            contents = sr.ReadToEnd();
            sr.Close();
            contents = GetPhdFacultyData(fid, reg, contents);
            //  contents = affiliationType(collegeId, contents);

            //Read string contents using stream reader and convert html to parsed conent
            var parsedHtmlElements = HTMLWorker.ParseToList(new StringReader(contents), null);
            //Get each array values from parsed elements and add to the PDF document
            bool pageRotated = false;
            int count = 0;
            foreach (var htmlElement in parsedHtmlElements)
            {
                count++;
                if (count == 100)
                {
                }
                if (htmlElement.Equals("<textarea>"))
                {
                    pdfDoc.NewPage();
                }

                if (htmlElement.Chunks.Count >= 3)
                {
                    if (htmlElement.Chunks.Count == 4)
                    {
                        pdfDoc.SetPageSize(iTextSharp.text.PageSize.A4.Rotate());
                        pdfDoc.SetMargins(60, 50, 50, 50);
                        pageRotated = true;
                    }
                    else
                    {
                        if (pageRotated == true)
                        {
                            pdfDoc.SetPageSize(iTextSharp.text.PageSize.A4);
                            pdfDoc.SetMargins(60, 50, 50, 50);
                            pageRotated = false;
                        }
                    }
                    pdfDoc.NewPage();
                }
                else
                {
                    pdfDoc.Add(htmlElement as IElement);
                }
            }

            //Close your PDF
            pdfDoc.Close();
            if (pdfDoc.IsOpen())
            {
                pdfDoc.Close();
            }

            string returnPath = string.Empty;
            returnPath = fullPath;
            return returnPath;
        }

        public string GetPhdFacultyData(int fid, string regno, string contents)
        {
            string contentdata = string.Empty;
            var facultydata = db.jntuh_principal_details_affiliated_colleges.Where(i => i.registrationnumber == regno).ToList();
            if (facultydata.Count > 0)
            {
                var jntuhregfaculty = db.jntuh_registered_faculty.Find(fid);
                var collegeid = facultydata[0].collegeid;
                var jntuhcollege = db.jntuh_college.AsNoTracking().FirstOrDefault(i => i.id == collegeid);
                var facultyName = jntuhregfaculty.FirstName + " " + jntuhregfaculty.MiddleName + " " + jntuhregfaculty.LastName;
                var facultyDob = jntuhregfaculty.DateOfBirth.ToString("dd-MM-yyyy");
                var facultyEducation = db.jntuh_registered_faculty_education.AsNoTracking().Where(e => e.facultyId == fid).ToList();
                var facultyPhdDetails = db.jntuh_faculty_phddetails.AsNoTracking().Where(i => i.Facultyid == fid).FirstOrDefault();
                var facultyUGbranch = facultyEducation.Where(i => i.educationId == 3).FirstOrDefault();
                var facultyPGbranch = facultyEducation.Where(i => i.educationId == 4).FirstOrDefault();
                var facultyphDEducation = facultyEducation.Where(i => i.educationId == 6).FirstOrDefault();
                var facultyPhDbranch = facultyPhdDetails != null ? facultyPhdDetails.University : "";
                //contentdata += "<div>";
                //contentdata += "<p style='color:darkblue;font-family:inherit;text-align:center;font-size:13px;font-family:Times New Roman'>Jawaharlal Nehru Technological University Hyderabad</p>";
                //contentdata += "</div>";
                //contentdata += "<div><p style='color:darkblue;font-family:inherit;text-align:center;font-size:13px;font-family:Times New Roman'>DIRECTORATE OF AFFILIATIONS & ACADEMIC AUDIT </p></div>";
                //contentdata += "<div><p style='color:darkblue;font-family:inherit;text-align:center;font-size:13px;font-family:Times New Roman'>Kukatpally, Hyderabad – 500 085, Telangana, India</p></div>";
                //contentdata += "<br/>";
                contentdata += "<p style='text-align:center;font-size:10px;'><b><u>DETAILS OF THE CANDIDATES APPLIED FOR THE POST OF PRINCIPAL IN AFFILIATED COLLEGES</b></u></p>";
                contentdata += "<br/><table border='0'cellspacing='0' cellpadding='4'  width='100%' class='auto' style='font-size:10px;border-collapse:collapse'>"; //cellspacing='0' cellpadding='5'
                contentdata += "<tr><td style='text-align:left' width='20%'><b>Faculty Name&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;:</b></td><td  style='text-align:left'  width='45%'>" +
                facultyName.ToUpper() + "</td>";
                contentdata += "</tr>";
                contentdata += "<tr><td style='text-align:left' width='20%'><b>Registration Number&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;:</b></td><td  style='text-align:left'  width='45%'>" +
                facultydata[0].registrationnumber + "</td>";
                contentdata += "</tr>";
                contentdata += "<tr><td style='text-align:left' width='20%'><b>College where working&nbsp;&nbsp;&nbsp;:</b></td><td  style='text-align:left'  width='45%'>" +
                jntuhcollege.collegeName + " (" + jntuhcollege.collegeCode + ")" + "</td>";
                contentdata += "</tr>";
                contentdata += "<tr><td style='text-align:left' width='20%'><b>Date of Birth&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;:</b></td><td  style='text-align:left'  width='45%'>" +
               facultyDob + "</td>";
                // contentdata += "<tr><td style='text-align:left' width='20%'><b>UG Branch&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;:</b></td><td  style='text-align:left'  width='45%'>" +
                //     (facultyUGbranch != null ? facultyUGbranch.specialization : "") + "</td>";
                // contentdata += "<tr><td style='text-align:left' width='20%'><b>PG Branch&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;:</b></td><td  style='text-align:left'  width='45%'>" +
                //(facultyPGbranch != null ? facultyPGbranch.specialization : "") + "</td>";
                contentdata += "<tr><td style='text-align:left' width='20%'><b>Ph.D obtained University&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;:</b></td><td  style='text-align:left'  width='45%'>" +
               facultyPhDbranch + "</td>";
                contentdata += "</table>";

                contentdata += "<table border='1'cellspacing='0' cellpadding='4'  width='100%' class='auto' style='font-size:9px;border-collapse:collapse'>";
                contentdata += "<tr>";
                contentdata += "<td width='12%' style='text-align:left'><b>Educational <br/> Qualifications</b></td>";
                contentdata += "<td width='10%' style='text-align:left'><b>Degree-Branch</b></td>";
                contentdata += "<td width='10%' style='text-align:left'><b>Division</b></td>";
                contentdata += "<td width='9%' style='text-align:left'><b>Year of Passing</b></td>";
                contentdata += "<td width='6%' style='text-align:left'><b>Certificate</b></td>";
                contentdata += "</tr>";
                string UgCerificateStatus = facultydata[0].ugodproof == null ? "No" : "Yes";
                string PgCerificateStatus = facultydata[0].pgodproof == null ? "No" : "Yes";
                string PhdCerificateStatus = facultydata[0].phdodproof == null ? "No" : "Yes";
                contentdata += "<tr>";
                contentdata += "<td width='12%' style='text-align:left'>UG</td>";
                contentdata += "<td width='10%' style='text-align:left'>" + (facultyUGbranch != null ? facultydata[0].ugdegree + "-" + facultyUGbranch.specialization : facultydata[0].ugdegree + "-" + "") + "</td>";
                contentdata += "<td width='10%' style='text-align:left'>" + facultydata[0].ugdivision + "</td>";
                contentdata += "<td width='14%' style='text-align:left'>" + facultydata[0].ugpassedyear + "</td>";
                contentdata += "<td width='9%' style='text-align:left'>" + UgCerificateStatus + "</td>";
                contentdata += "</tr>";
                contentdata += "<tr>";
                contentdata += "<td width='12%' style='text-align:left'>PG</td>";
                contentdata += "<td width='10%' style='text-align:left'>" + (facultyPGbranch != null ? facultydata[0].pgdegree + "-" + facultyPGbranch.specialization : facultydata[0].pgdegree + "-" + "") + "</td>";
                contentdata += "<td width='10%' style='text-align:left'>" + facultydata[0].pgdivision + "</td>";
                contentdata += "<td width='14%' style='text-align:left'>" + facultydata[0].pgpassedyear + "</td>";
                contentdata += "<td width='9%' style='text-align:left'>" + PgCerificateStatus + "</td>";
                contentdata += "</tr>";
                contentdata += "<tr>";
                contentdata += "<td width='12%' style='text-align:left'>Ph.D.</td>";
                contentdata += "<td width='10%' style='text-align:left'>" + (facultyphDEducation != null ? facultydata[0].phddegree + "-" + facultyphDEducation.specialization : facultydata[0].phddegree + "-" + "") + "</td>";
                contentdata += "<td width='10%' style='text-align:left'>" + facultydata[0].phddivision + "</td>";
                contentdata += "<td width='14%' style='text-align:left'>" + facultydata[0].phdpassedyear + "</td>";
                contentdata += "<td width='9%' style='text-align:left'>" + PhdCerificateStatus + "</td>";
                contentdata += "</tr>";
                contentdata += "</table>";

                contentdata += "<br/><table border='1'cellspacing='0' cellpadding='4'  width='100%' class='auto' style='font-size:9px;border-collapse:collapse'>";
                contentdata += "<tr>";
                contentdata += "<td width='20%' style='text-align:left'><b>Experience in Teaching/<br/>Research/Industry</b></td>";
                contentdata += "<td width='12%' style='text-align:left'><b>Number of Years</b></td>";
                contentdata += "<td width='7%' style='text-align:left'><b>Total Experience</b></td>";
                contentdata += "<td width='6%' style='text-align:left'><b>Document</b></td>";
                contentdata += "</tr>";
                string LectCerificateStatus = facultydata[0].lecturersupportingdocument == null ? "No" : "Yes";
                string AssProfCerificateStatus = facultydata[0].associateprofsupportingdoc == null ? "No" : "Yes";
                string ProfCerificateStatus = facultydata[0].professorsupportingdoc == null ? "No" : "Yes";
                string PrinCerificateStatus = facultydata[0].principalsupportingdoc == null ? "No" : "Yes";
                string RsrProfCerificateStatus = facultydata[0].researchsupportingdoc == null ? "No" : "Yes";
                string IndCerificateStatus = facultydata[0].industrysupportingdoc == null ? "No" : "Yes";
                contentdata += "<tr>";
                contentdata += "<td width='20%' style='text-align:left'>Lecturer / Asst.Professor</td>";
                contentdata += "<td width='12%' style='text-align:left'>" + facultydata[0].lecturernoofyears + "</td>";
                contentdata += "<td width='7%' rowspan='5' style='text-align:center;'>" + facultydata[0].totalexperience + "</td>";
                contentdata += "<td width='6%' style='text-align:left'>" + LectCerificateStatus + "</td>";
                contentdata += "</tr>";
                contentdata += "<tr>";
                contentdata += "<td width='20%' style='text-align:left'>Associate Professor</td>";
                contentdata += "<td width='12%' style='text-align:left'>" + facultydata[0].associateprofnoofyears + "</td>";
                //contentdata += "<td width='14%' style='text-align:left'>" + facultydata[0].pgpassedyear + "</td>";
                contentdata += "<td width='6%' style='text-align:left'>" + AssProfCerificateStatus + "</td>";
                contentdata += "</tr>";
                contentdata += "<tr>";
                contentdata += "<td width='20%' style='text-align:left'>Professor</td>";
                contentdata += "<td width='12%' style='text-align:left'>" + facultydata[0].professornoofyears + "</td>";
                //contentdata += "<td width='14%' style='text-align:left'>" + facultydata[0].phdpassedyear + "</td>";
                contentdata += "<td width='6%' style='text-align:left'>" + ProfCerificateStatus + "</td>";
                contentdata += "</tr>";

                contentdata += "<tr>";
                contentdata += "<td width='20%' style='text-align:left'>Research</td>";
                contentdata += "<td width='12%' style='text-align:left'>" + facultydata[0].researchnoofyears + "</td>";
                //contentdata += "<td width='14%' style='text-align:left'>" + facultydata[0].pgpassedyear + "</td>";
                contentdata += "<td width='6%' style='text-align:left'>" + RsrProfCerificateStatus + "</td>";
                contentdata += "</tr>";
                contentdata += "<tr>";
                contentdata += "<td width='20%' style='text-align:left'>Industry</td>";
                contentdata += "<td width='12%' style='text-align:left'>" + facultydata[0].industrynoofyears + "</td>";
                //contentdata += "<td width='14%' style='text-align:left'>" + facultydata[0].phdpassedyear + "</td>";
                contentdata += "<td width='6%' style='text-align:left'>" + IndCerificateStatus + "</td>";
                contentdata += "</tr>";
                contentdata += "<tr>";
                contentdata += "<td width='20%' style='text-align:left'>Principal</td>";
                contentdata += "<td width='12%' colspan='2' style='text-align:center'>" + facultydata[0].principalnoofyears + "</td>";
                //contentdata += "<td width='7%' colspan='2' style='text-align:center;'>" + facultydata[0].totalexperience + "</td>";
                contentdata += "<td width='9%' style='text-align:left'>" + PrinCerificateStatus + "</td>";
                contentdata += "</tr>";
                contentdata += "</table>";




                contentdata += "<br/><table border='1'cellspacing='0' cellpadding='4'  width='100%' class='auto' style='font-size:9px;border-collapse:collapse'>";
                contentdata += "<tr>";
                contentdata += "<td width='23%' style='text-align:left'><b>Ratified / Selected as</b></td>";
                contentdata += "<td width='20%' style='text-align:left'><b>Ratification/Selected Date</b></td>";
                contentdata += "<td width='12%' style='text-align:left'><b>Number of Years</b></td>";
                contentdata += "<td width='7%' style='text-align:left'><b>Total Ratified / Selected Service</b></td>";
                contentdata += "<td width='6%' style='text-align:left'><b>Document</b></td>";
                contentdata += "</tr>";
                string LectRatCerificateStatus = facultydata[0].assistantprofratsuptdoc == null ? "No" : "Yes";
                string AssProfRatCerificateStatus = facultydata[0].associateprofratsuptdoc == null ? "No" : "Yes";
                string ProfRatCerificateStatus = facultydata[0].professorratsuptdoc == null ? "No" : "Yes";
                string PrinRatCerificateStatus = facultydata[0].principalratsuptdoc == null ? "No" : "Yes";
                contentdata += "<tr>";
                contentdata += "<td width='20%' style='text-align:left'>Asst.Professor</td>";
                if (facultydata[0].assistantprofratactivitystatus == "Yes")
                {
                    contentdata += "<td width='12%' style='text-align:left'>" + Convert.ToDateTime(facultydata[0].assistantprofratifieddate).ToString("dd-MM-yyyy") + "</td>";
                    contentdata += "<td width='12%' style='text-align:left'>" + facultydata[0].assistantprofratnoofyears + "</td>";
                    contentdata += "<td width='7%' rowspan='3' style='text-align:center;'>" + facultydata[0].totalratifiedservice + "</td>";
                    contentdata += "<td width='6%' style='text-align:left'>" + LectRatCerificateStatus + "</td>";
                    contentdata += "</tr>";
                }
                else
                {
                    contentdata += "<td width='12%' style='text-align:left'>NA</td>";
                    contentdata += "<td width='12%' style='text-align:left'>NA</td>";
                    contentdata += "<td width='7%' rowspan='3' style='text-align:center;'>" + facultydata[0].totalratifiedservice + "</td>";
                    contentdata += "<td width='6%' style='text-align:left'>NA</td>";
                    contentdata += "</tr>";
                }
                if (facultydata[0].associateprofratactivitystatus == "Yes")
                {
                    contentdata += "<tr>";
                    contentdata += "<td width='20%' style='text-align:left'>Associate Professor</td>";
                    contentdata += "<td width='12%' style='text-align:left'>" + Convert.ToDateTime(facultydata[0].associateprofratifieddate).ToString("dd-MM-yyyy") + "</td>";
                    contentdata += "<td width='12%' style='text-align:left'>" + facultydata[0].associateprofratnoofyears + "</td>";
                    contentdata += "<td width='6%' style='text-align:left'>" + AssProfRatCerificateStatus + "</td>";
                    contentdata += "</tr>";
                }
                else
                {
                    contentdata += "<tr>";
                    contentdata += "<td width='20%' style='text-align:left'>Associate Professor</td>";
                    contentdata += "<td width='12%' style='text-align:left'>NA</td>";
                    contentdata += "<td width='12%' style='text-align:left'>NA</td>";
                    contentdata += "<td width='6%' style='text-align:left'>NA</td>";
                    contentdata += "</tr>";
                }
                if (facultydata[0].professorratactivitystatus == "Yes")
                {
                    contentdata += "<tr>";
                    contentdata += "<td width='20%' style='text-align:left'>Professor</td>";
                    contentdata += "<td width='12%' style='text-align:left'>" + Convert.ToDateTime(facultydata[0].professorratifieddate).ToString("dd-MM-yyyy") + "</td>";
                    contentdata += "<td width='12%' style='text-align:left'>" + facultydata[0].professorratnoofyears + "</td>";
                    contentdata += "<td width='6%' style='text-align:left'>" + ProfRatCerificateStatus + "</td>";
                    contentdata += "</tr>";
                }
                else
                {
                    contentdata += "<tr>";
                    contentdata += "<td width='20%' style='text-align:left'>Professor</td>";
                    contentdata += "<td width='12%' style='text-align:left'>NA</td>";
                    contentdata += "<td width='12%' style='text-align:left'>NA</td>";
                    contentdata += "<td width='6%' style='text-align:left'>NA</td>";
                    contentdata += "</tr>";
                }
                if (facultydata[0].principalratactivitystatus == "Yes")
                {
                    contentdata += "<tr>";
                    contentdata += "<td width='20%' style='text-align:left'>Principal</td>";
                    contentdata += "<td width='12%' style='text-align:left'>" + Convert.ToDateTime(facultydata[0].principalratifieddate).ToString("dd-MM-yyyy") + "</td>";
                    contentdata += "<td width='12%' colspan='2' style='text-align:center'>" + facultydata[0].principalratnoofyears + "</td>";
                    contentdata += "<td width='9%' style='text-align:left'>" + PrinRatCerificateStatus + "</td>";
                    contentdata += "</tr>";
                    contentdata += "</table>";
                }
                else
                {
                    contentdata += "<tr>";
                    contentdata += "<td width='20%' style='text-align:left'>Principal</td>";
                    contentdata += "<td width='12%' style='text-align:left'>NA</td>";
                    contentdata += "<td width='12%' colspan='2' style='text-align:center' style='text-align:center'>NA</td>";
                    contentdata += "<td width='6%' style='text-align:left'>NA</td>";
                    contentdata += "</tr>";
                    contentdata += "</table>";
                }

                if (facultydata[0].collegetype != "Pharmacy")
                {
                    contentdata += "<br/><table border='1'cellspacing='0' cellpadding='4'  width='100%' class='auto' style='font-size:9px;border-collapse:collapse'>";
                    contentdata += "<tr>";
                    contentdata += "<td width='18%' style='text-align:left'><b>Number of Publications</b></td>";
                    contentdata += "<td width='12%' style='text-align:left'><b>Number</b></td>";
                    contentdata += "<td width='7%' style='text-align:left'><b>Total Publications</b></td>";
                    contentdata += "<td width='6%' style='text-align:left'><b>Document</b></td>";
                    contentdata += "</tr>";
                    string IntJounCerificateStatus = facultydata[0].internationaljournalsdoc == null ? "No" : "Yes";
                    string IntConfCerificateStatus = facultydata[0].internationalconferencesdoc == null ? "No" : "Yes";
                    string NatJounCerificateStatus = facultydata[0].nationaljournalsdoc == null ? "No" : "Yes";
                    string NatConfCerificateStatus = facultydata[0].nationalconferencesdoc == null ? "No" : "Yes";

                    contentdata += "<tr>";
                    contentdata += "<td width='18%' style='text-align:left'>International Journals</td>";
                    contentdata += "<td width='12%' style='text-align:left'>" + facultydata[0].internationaljournals + "</td>";
                    contentdata += "<td width='7%' rowspan='4' style='text-align:center;'>" + facultydata[0].totalpublications + "</td>";
                    contentdata += "<td width='6%' style='text-align:left'>" + IntJounCerificateStatus + "</td>";
                    contentdata += "</tr>";
                    contentdata += "<tr>";
                    contentdata += "<td width='18%' style='text-align:left'>International Conferences</td>";
                    contentdata += "<td width='12%' style='text-align:left'>" + facultydata[0].internationalconferences + "</td>";
                    //contentdata += "<td width='14%' style='text-align:left'>" + facultydata[0].pgpassedyear + "</td>";
                    contentdata += "<td width='6%' style='text-align:left'>" + IntConfCerificateStatus + "</td>";
                    contentdata += "</tr>";
                    contentdata += "<tr>";
                    contentdata += "<td width='18%' style='text-align:left'>National Journals</td>";
                    contentdata += "<td width='12%' style='text-align:left'>" + facultydata[0].nationaljournals + "</td>";
                    //contentdata += "<td width='14%' style='text-align:left'>" + facultydata[0].phdpassedyear + "</td>";
                    contentdata += "<td width='6%' style='text-align:left'>" + NatJounCerificateStatus + "</td>";
                    contentdata += "</tr>";
                    contentdata += "<tr>";
                    contentdata += "<td width='18%' style='text-align:left'>National Conferences</td>";
                    contentdata += "<td width='12%' style='text-align:left'>" + facultydata[0].nationalconferences + "</td>";
                    //contentdata += "<td width='14%' style='text-align:left'>" + facultydata[0].ugpassedyear + "</td>";
                    contentdata += "<td width='9%' style='text-align:left'>" + NatConfCerificateStatus + "</td>";
                    contentdata += "</tr>";
                    contentdata += "</table>";

                    //contentdata += "<br/><table border='1'cellspacing='0' cellpadding='4'  width='100%' class='auto' style='font-size:9px;border-collapse:collapse'>";
                    //contentdata += "<tr>";
                    //contentdata += "<td width='18%' style='text-align:left'><b>Number of Ph.Ds guided</b></td>";
                    //contentdata += "<td width='50%' style='text-align:left'><b>Guided University</b></td>";
                    //contentdata += "<td width='2%' style='text-align:left'><b>Number</b></td>";
                    //contentdata += "<td width='4%' style='text-align:left'><b>Total Number</b></td>";
                    //contentdata += "<td width='5%' style='text-align:left'><b>Document</b></td>";
                    //contentdata += "</tr>";
                    //string SuperCerificateStatus = facultydata[0].supervisorphdguideddoc == null ? "No" : "Yes";
                    //string CoSuperCerificateStatus = facultydata[0].cosupervisorphdguideddoc == null ? "No" : "Yes";

                    //contentdata += "<tr>";
                    //contentdata += "<td width='18%' style='text-align:left'>Supervisor</td>";
                    //contentdata += "<td width='50%' style='text-align:left;'>" + facultydata[0].supervisorguideduniversity + "</td>";
                    //contentdata += "<td width='2%' style='text-align:left'>" + facultydata[0].supervisorphdguided + "</td>";
                    //contentdata += "<td width='4%' rowspan='2' style='text-align:center;'>" + facultydata[0].totalphdsguided + "</td>";
                    //contentdata += "<td width='5%' style='text-align:left'>" + SuperCerificateStatus + "</td>";
                    //contentdata += "</tr>";
                    //contentdata += "<tr>";
                    //contentdata += "<td width='18%' style='text-align:left'>Co-Supervisor</td>";
                    //contentdata += "<td width='50%' style='text-align:left;'>" + facultydata[0].cosupervisorguideduniversity + "</td>";
                    //contentdata += "<td width='2%' style='text-align:left'>" + facultydata[0].cosupervisorphdguided + "</td>";
                    //contentdata += "<td width='5%' style='text-align:left'>" + CoSuperCerificateStatus + "</td>";
                    //contentdata += "</tr>";
                    //contentdata += "</table>";


                    contentdata += "<br/><table border='1'cellspacing='0' cellpadding='4'  width='100%' class='auto' style='font-size:9px;border-collapse:collapse'>";
                    contentdata += "<tr>";
                    contentdata += "<td style='text-align:left'><b>Number of Ph.Ds guided</b></td>";
                    contentdata += "<td style='text-align:left'><b>Guided University</b></td>";
                    contentdata += "<td style='text-align:left'><b>Number</b></td>";
                    contentdata += "<td style='text-align:left'><b>Total Number</b></td>";
                    contentdata += "<td style='text-align:left'><b>Document</b></td>";
                    contentdata += "</tr>";
                    string SuperCerificateStatus = facultydata[0].supervisorphdguideddoc == null ? "No" : "Yes";
                    string CoSuperCerificateStatus = facultydata[0].cosupervisorphdguideddoc == null ? "No" : "Yes";

                    contentdata += "<tr>";
                    contentdata += "<td style='text-align:left'>Supervisor</td>";
                    contentdata += "<td style='text-align:left;'>" + facultydata[0].supervisorguideduniversity + "</td>";
                    contentdata += "<td style='text-align:left'>" + facultydata[0].supervisorphdguided + "</td>";
                    contentdata += "<td rowspan='2' style='text-align:center;'>" + facultydata[0].totalphdsguided + "</td>";
                    contentdata += "<td style='text-align:left'>" + SuperCerificateStatus + "</td>";
                    contentdata += "</tr>";
                    contentdata += "<tr>";
                    contentdata += "<td style='text-align:left'>Co-Supervisor</td>";
                    contentdata += "<td style='text-align:left;'>" + facultydata[0].cosupervisorguideduniversity + "</td>";
                    contentdata += "<td style='text-align:left'>" + facultydata[0].cosupervisorphdguided + "</td>";
                    contentdata += "<td style='text-align:left'>" + CoSuperCerificateStatus + "</td>";
                    contentdata += "</tr>";
                    contentdata += "</table>";
                }
                contentdata += "<p style='text-align:left;font-weight:8px;' width='19%'><b>Date &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;:</b></p>";
                contentdata += "<p style='text-align:left;font-weight:8px;' width='19%'><b>Place &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;:</b></p>";
                contentdata += "<p style='text-align:right;font-weight:8px;'><b>Signature of the Candidate</b></p>";
                contents = contents.Replace("##COLLEGE_RANDOMCODE##", contentdata);
            }
            return contents;
        }

        #endregion
    }

    #region Model classes
    public class ScmRequestList
    {
        public int Id { get; set; }
        public string CollegeCode { get; set; }
        public string CollegeName { get; set; }
        public bool IsAuditorAssigned { get; set; }
        public bool Checked { get; set; }

        public List<ScmRequestList> SCmRequestList { get; set; }
    }
    public class DistinctSpecializations
    {
        public int DepartmentId { get; set; }
        public int SpecializationId { get; set; }
        public string SpecializationName { get; set; }
        public string DepartmentName { get; set; }
    }

    public class NomineeAssignSCMRequests
    {
        public int SCMId { get; set; }
        public string CollegeName { get; set; }
        public string CollegeCode { get; set; }
        public string Department { get; set; }
        public DateTime? ScmRequestDate { get; set; }
        public string AuditorName { get; set; }
        public string AuditorEmail { get; set; }
        public string AuditorMobile { get; set; }
        public DateTime AuditorAssignDate { get; set; }

    }

    public class DistinctDepartments
    {
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; }
    }

    public class ScmProceedingsRequest
    {
        public string CollegeName { get; set; }
        public string CollegeCode { get; set; }
        public int CollegeId { get; set; }
        public int DepartmentId { get; set; }
        public int SpecializationId { get; set; }

        //[Required(ErrorMessage = "*")]
        public bool? isscmatUniversity { get; set; }

        public int Professors { get; set; }
        public int AssociateProfessors { get; set; }
        public int AssistantProfessors { get; set; }
        public DateTime? RequestSubmittedDate { get; set; }
        public int? paymentId { get; set; }


        [Required(ErrorMessage = "*")]
        [StringLength(2, ErrorMessage = "Max 2 characters")]
        [RegularExpression("^[0-9]*$", ErrorMessage = "Invalid Number")]
        [Display(Name = "Professors")]
        public string ProfessorVacancies { get; set; }
        [Required(ErrorMessage = "*")]
        [StringLength(2, ErrorMessage = "Max 2 characters")]
        [RegularExpression("^[0-9]*$", ErrorMessage = "Invalid Number")]
        [Display(Name = "Associate Professors")]
        public string AssociateProfessorVacancies { get; set; }
        [Required(ErrorMessage = "*")]
        [StringLength(2, ErrorMessage = "Max 2 characters")]
        [RegularExpression("^[0-9]*$", ErrorMessage = "Invalid Number")]
        [Display(Name = "Assistant Professors")]
        public string AssistantProfessorVacancies { get; set; }
        public string Remarks { get; set; }
        public HttpPostedFileBase ScmNotificationSupportDoc { get; set; }
        public string ScmNotificationpath { get; set; }
        public string SpecializationName { get; set; }

        public string DepartmentName { get; set; }
        public int DegreeId { get; set; }
        public string DegreeName { get; set; }

        public bool Checked { get; set; }
        public int Id { get; set; }

        public string NotificationDate { get; set; }
        public DateTime? NotificationDateView { get; set; }

        [Required(ErrorMessage = "*")]
        [StringLength(2, ErrorMessage = "Max 2 characters")]
        [RegularExpression("^[0-9]*$", ErrorMessage = "Invalid Number")]
        [Display(Name = "Professors")]
        public string RequiredProfessorVacancies { get; set; }
        [Required(ErrorMessage = "*")]
        [StringLength(2, ErrorMessage = "Max 2 characters")]
        [RegularExpression("^[0-9]*$", ErrorMessage = "Invalid Number")]
        [Display(Name = "Associate Professors")]
        public string RequiredAssociateProfessorVacancies { get; set; }
        [Required(ErrorMessage = "*")]
        [StringLength(2, ErrorMessage = "Max 2 characters")]
        [RegularExpression("^[0-9]*$", ErrorMessage = "Invalid Number")]
        [Display(Name = "Assistant Professors")]
        public string RequiredAssistantProfessorVacancies { get; set; }

        public DateTime CreatedDate { get; set; }


        public bool IsEdit { get; set; }
        public bool IsAuditorAssigned { get; set; }
        public bool IsAuditorVerified { get; set; }
        public bool IsSplited { get; set; }
        [Required(ErrorMessage = "*")]
        public int? TotalFacultyRequired { get; set; }
        public List<ScmProceedingsRequest> scmtotalrequests { get; set; }
        public List<ScmProceedingsRequest> ScmProceedingsRequestslist { get; set; }
        public List<ScmProceedingsRequest> collegeatuniversity { get; set; }
    }


    public class ScmProceedingsRequestAddReg
    {
        public int Id { get; set; }
        public int ScmId { get; set; }
        public string CollegeName { get; set; }
        public string CollegeCode { get; set; }

        public string Name { get; set; }
        public string Firstname { get; set; }
        public string Middlename { get; set; }
        public string Lastname { get; set; }

        public int CollegeId { get; set; }
        public int DepartmentId { get; set; }
        public int SpecializationId { get; set; }

        public string SpecializationName { get; set; }

        public string DepartmentName { get; set; }
        public int DegreeId { get; set; }
        public string DegreeName { get; set; }


        public int Professors { get; set; }
        public int AssociateProfessors { get; set; }
        public int AssistantProfessors { get; set; }
        [Required(ErrorMessage = "Registration Number is Required")]
        [Remote("CheckRegistrationNumber", "CollegeSCMProceedingsRequest", HttpMethod = "POST", ErrorMessage = "Registration Number doesn't Exist")]
        public string RegistrationNo { get; set; }

        public string ExperianceDocumentView { get; set; }
        public HttpPostedFileBase ExperianceDocument { get; set; }

        public string DeactiviationReason { get; set; }
        public string Regno { get; set; }
        public string RegName { get; set; }

        public int FacultyId { get; set; }
        [Required]
        public int PreviousCollegeId { get; set; }
        public int FacultyType { get; set; }
        public string Designation { get; set; }

        public bool? IsApprove { get; set; }
        public bool? IsActive { get; set; }

        public DateTime? RequestSubmissionDate { get; set; }
    }

    public class Scmdates
    {
        public DateTime SCMDATE { get; set; }
        public string SCMDATEview { get; set; }
        public int SCMDateId { get; set; }
    }

    public class Scmuploads
    {
        public int Id { get; set; }
        public string CollegeCode { get; set; }
        public string CollegeName { get; set; }
        public int CollegeId { get; set; }
        [Required(ErrorMessage = "*")]
        [Remote("CheckRegistrationNumber", "CollegeSCMProceedingsRequest", HttpMethod = "POST", ErrorMessage = "Registration Number doesn't Exist")]
        public string RegistrationNo { get; set; }
        [Required(ErrorMessage = "*")]
        public int SpecializationId { get; set; }
        public string SpecializationName { get; set; }
        [Required(ErrorMessage = "*")]
        public int DesignationId { get; set; }
        public string DesignationName { get; set; }
        public int DegreeId { get; set; }
        public string Degree { get; set; }
        public int DepartmentId { get; set; }
        public string Department { get; set; }
        [Required(ErrorMessage = "*")]
        public string ScmDate { get; set; }

        public string NewScmDate { get; set; }


        public HttpPostedFileBase ProfessorDocument { get; set; }
        public HttpPostedFileBase AssociateProfessorDocument { get; set; }
        [Required(ErrorMessage = "*")]
        public HttpPostedFileBase AssistantProfessorDocument { get; set; }

        public string ProfessorDocumentView { get; set; }
        public string AssociateProfessorDocumentView { get; set; }
        public string AssistantProfessorDocumentView { get; set; }

        public string SCMDocumentView { get; set; }
        public HttpPostedFileBase SCMDocument { get; set; }
        public DateTime ScmDateView { get; set; }

    }

    public class FacultyRegistrationDetails
    {
        public FacultyRegistrationDetails()
        {
            this.jntuh_registered_faculty_education = new HashSet<jntuh_registered_faculty_education>();
            //this.jntuh_registered_faculty_education_log=new HashSet<jntuh_registered_faculty_education_log>();
        }

        public int id { get; set; }
        public string Type { get; set; }
        public int? CollegeId { get; set; }
        public string CollegeName { get; set; }
        public string CollegeCode { get; set; }
        public string RegistrationNumber { get; set; }
        public string UniqueID { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public int? GenderId { get; set; }
        public string FatherOrhusbandName { get; set; }
        public string MotherName { get; set; }
        public string Principal { get; set; }
        public string Email { get; set; }
        public string facultyPhoto { get; set; }
        public string Mobile { get; set; }
        public string PANNumber { get; set; }
        public string AadhaarNumber { get; set; }
        public bool isActive { get; set; }
        public bool? isApproved { get; set; }
        public string department { get; set; }
        public int? SamePANNumberCount { get; set; }
        public int? SameAadhaarNumberCount { get; set; }
        public string SpecializationIdentfiedFor { get; set; }
        public string DeactivationReason { get; set; }
        public string IdentfiedFor { get; set; }
        public int FacultyAddId { get; set; }







        //public int DegreeId { get; set; }
        //public int Eid { get; set; }
        //public string PanVerificationStatus { get; set; }
        //public string PanDeactivationReasion { get; set; }
        //public string PanStatusAfterDE { get; set; }
        //public int? FIsApproved { get; set; }
        ////[Required(ErrorMessage = "*")]
        //public string UserName { get; set; }
        //public string AdjunctDesignation { get; set; }
        //public string AdjunctDepartment { get; set; }

        //public DateTime? DateOfBirth { get; set; }
        //public string facultyDateOfBirth { get; set; }
        //public string OrganizationName { get; set; }
        //public int? DepartmentId { get; set; }
        //public int? DesignationId { get; set; }
        //public string designation { get; set; }
        //public DateTime? DateOfAppointment { get; set; }
        //public string facultyDateOfAppointment { get; set; }
        //public string ProceedingsNo { get; set; }
        //public string SelectionCommitteeProcedings { get; set; }
        //public string AICTEFacultyId { get; set; }
        //public string GrossSalary { get; set; }
        //public int? TotalExperience { get; set; }
        //public int? TotalExperiencePresentCollege { get; set; }
        //public string EditPANNumber { get; set; }
        //public HttpPostedFileBase PANCardDocument { get; set; }
        //public string facultyPANCardDocument { get; set; }
        //public string EditEmail { get; set; }
        //public string National { get; set; }
        //public string InterNational { get; set; }
        //public string Citation { get; set; }
        //public string Awards { get; set; }
        //public HttpPostedFileBase Photo { get; set; }
        //public string facultyAadhaarCardDocument { get; set; }
        //public bool? isView { get; set; }
        //public Nullable<System.DateTime> createdOn { get; set; }
        //public Nullable<int> createdBy { get; set; }
        //public Nullable<System.DateTime> updatedOn { get; set; }
        //public Nullable<int> updatedBy { get; set; }
        //public bool? isFacultyRatifiedByJNTU { get; set; }
        //public DateTime? DateOfRatification { get; set; }
        //public string facultyDateOfRatification { get; set; }
        //public bool? WorkingStatus { get; set; }
        //public string OtherDepartment { get; set; }
        //public string OtherDesignation { get; set; }
        //public bool isVerified { get; set; }
        //public bool isValid { get; set; }
        //public virtual my_aspnet_users my_aspnet_users { get; set; }
        //public virtual my_aspnet_users my_aspnet_users1 { get; set; }
        //public virtual jntuh_designation jntuh_designation { get; set; }
        //public virtual jntuh_department jntuh_department { get; set; }
        public virtual ICollection<jntuh_registered_faculty_education> jntuh_registered_faculty_education { get; set; }
        //public virtual ICollection<jntuh_reinspection_registered_faculty_education> jntuh_reinspection_registered_faculty_education { get; set; }
        //// public virtual ICollection<jntuh_registered_faculty_education_log> jntuh_registered_faculty_education_log { get; set; }
        //public List<RegisteredFacultyEducation> FacultyEducation { get; set; }
        //public List<RegisteredfacultyExperience> RFExperience { get; set; }
        //public int? SpecializationId { get; set; }
        //public string SpecializationName { get; set; }

    }

    public class Facultynotapproved
    {
        [Required]
        public int FacultyAddId { get; set; }
        [Required]
        public int CollegeId { get; set; }
        [Required(ErrorMessage = "Please Enter Reason")]
        public string DeactivationReason { get; set; }
    }

    public class CollegeData
    {
        public int collegeId { get; set; }
        public string collegeName { get; set; }
    }

    public class SCMPrincipal
    {
        public int SCMId { get; set; }
        [Required(ErrorMessage = "Registration Number is Required")]
        [Remote("CheckRegistrationNumber", "CollegeSCMProceedingsRequest", HttpMethod = "POST", ErrorMessage = "Registration Number doesn't Exist")]
        public string RegistrationNo { get; set; }
        [Required]
        public string NotificationDate { get; set; }
        [Required]
        public HttpPostedFileBase ScmNotificationSupportDoc { get; set; }
        [Required(ErrorMessage = "Select Previous Working College")]
        public int PreviousCollegeId { get; set; }
        public int? paymentid { get; set; }
        public string PreviousCollegeName { get; set; }
        public int ScmAddfacultyId { get; set; }
        public string FirstName { get; set; }
        public string scmnotificationdocview { get; set; }
        public DateTime? createdDate { get; set; }
        public int FacultyId { get; set; }
        public bool PrincipalDetailsFound { get; set; }
    }

    public class SCMUploadForPrincipal
    {
        public int Id { get; set; }
        public string CollegeCode { get; set; }
        public string CollegeName { get; set; }
        public int CollegeId { get; set; }
        public int SpecializationId { get; set; }
        public string SpecializationName { get; set; }
        public int DegreeId { get; set; }
        public string Degree { get; set; }
        public int DepartmentId { get; set; }
        public string Department { get; set; }
        [Required(ErrorMessage = "*")]
        public string ScmDate { get; set; }
        [Required(ErrorMessage = "*")]
        [Remote("CheckRegistrationNumber", "CollegeSCMProceedingsRequest", HttpMethod = "POST", ErrorMessage = "Registration Number doesn't Exist")]
        public string RegistrationNo { get; set; }
        public int DesignationId { get; set; }
        public string DesignationName { get; set; }

        public HttpPostedFileBase PrincipalDocument { get; set; }
        public string PrincipalDocumentView { get; set; }
        public DateTime ScmDateView { get; set; }
    }

    public class PrincipalAppliedDetails
    {
        public string RegistrationNumber { get; set; }
        public string FacultyName { get; set; }
        public string WorkingCollege { get; set; }
        [Required(ErrorMessage = "Required")]
        public string CollegeType { get; set; }
        [Required(ErrorMessage = "*")]
        [RegularExpression(@"\d{1}", ErrorMessage = "Invalid UG Division")]
        public string UgDivision { get; set; }
        [Required(ErrorMessage = "*")]
        [RegularExpression(@"\d{1}", ErrorMessage = "Invalid PG Division")]
        public string PgDivision { get; set; }
        [Required(ErrorMessage = "*")]
        [RegularExpression(@"\d{1}", ErrorMessage = "Invalid Ph.D Division")]
        public string PhdDivision { get; set; }
        [Required(ErrorMessage = "*")]
        [RegularExpression(@"\d{4}", ErrorMessage = "Invalid UG Year of passing")]
        public string UgPassingYear { get; set; }
        [Required(ErrorMessage = "*")]
        [RegularExpression(@"\d{4}", ErrorMessage = "Invalid PG Year of passing")]
        public string PgPassingYear { get; set; }
        [Required(ErrorMessage = "*")]
        [RegularExpression(@"\d{4}", ErrorMessage = "Invalid Ph.D. Year of passing")]
        public string PhdPassingYear { get; set; }
        public HttpPostedFileBase UgODProof { get; set; }
        public string UgODProofName { get; set; }
        public HttpPostedFileBase PgODProof { get; set; }
        public string PgODProofName { get; set; }
        public HttpPostedFileBase PhDODProof { get; set; }
        public string PhdODProofName { get; set; }
        [Required(ErrorMessage = "*")]
        public string UgDegree { get; set; }
        [Required(ErrorMessage = "*")]
        public string PgDegree { get; set; }
        [Required(ErrorMessage = "*")]
        public string PhDArea { get; set; }

        //[RegularExpression(@"\d", ErrorMessage = "Invalid")]
        [Required(ErrorMessage = "*")]
        [StringLength(4, ErrorMessage = "Max 4 digits")]
        public string LectYears { get; set; }
        public HttpPostedFileBase LectSuppDoc { get; set; }
        public string LectSuppDocName { get; set; }

        [Required(ErrorMessage = "*")]
        [StringLength(4, ErrorMessage = "Max 4 digits")]
        public string AssProfYears { get; set; }
        public HttpPostedFileBase AssProfSuppDoc { get; set; }
        public string AssProfSuppDocName { get; set; }

        [Required(ErrorMessage = "*")]
        [StringLength(4, ErrorMessage = "Max 4 digits")]
        public string ProfYears { get; set; }
        public HttpPostedFileBase ProfSuppDoc { get; set; }
        public string ProfSuppDocName { get; set; }

        [Required(ErrorMessage = "*")]
        [StringLength(4, ErrorMessage = "Max 4 digits")]
        public string PrincipalYears { get; set; }
        public HttpPostedFileBase PrincipalSuppDoc { get; set; }
        public string PrincipalSuppDocName { get; set; }

        [Required(ErrorMessage = "*")]
        [StringLength(4, ErrorMessage = "Max 4 digits")]
        public string ResearchYears { get; set; }
        public HttpPostedFileBase ResearchSuppDoc { get; set; }
        public string ResearchSuppDocName { get; set; }

        [Required(ErrorMessage = "*")]
        [StringLength(4, ErrorMessage = "Max 4 digits")]
        public string IndustryYears { get; set; }
        public HttpPostedFileBase IndustrySuppDoc { get; set; }
        public string IndustrySuppDocName { get; set; }

        public string TotalExperience { get; set; }

        [Required(ErrorMessage = "*")]
        [StringLength(3, ErrorMessage = "Max 3 digits")]
        public string InterJornals { get; set; }
        public HttpPostedFileBase InterJornalsSuppDoc { get; set; }
        public string InterJornalsSuppDocName { get; set; }

        [Required(ErrorMessage = "*")]
        [StringLength(3, ErrorMessage = "Max 3 digits")]
        public string InterConfs { get; set; }
        public HttpPostedFileBase InterConfsSuppDoc { get; set; }
        public string InterConfsSuppDocName { get; set; }

        [Required(ErrorMessage = "*")]
        [StringLength(3, ErrorMessage = "Max 3 digits")]
        public string NationalJournals { get; set; }
        public HttpPostedFileBase NationalJournalsSuppDoc { get; set; }
        public string NationalJournalsSuppDocName { get; set; }

        [Required(ErrorMessage = "*")]
        [StringLength(3, ErrorMessage = "Max 3 digits")]
        public string NationalConfs { get; set; }
        public HttpPostedFileBase NationalConfsSuppDoc { get; set; }
        public string NationalConfsSuppDocName { get; set; }

        public string TotalPublications { get; set; }

        [Required(ErrorMessage = "*")]
        [StringLength(3, ErrorMessage = "Max 3 digits")]
        public string Supervisors { get; set; }
        public HttpPostedFileBase SupervisorsSuppDoc { get; set; }
        public string SupervisorsSuppDocName { get; set; }
        public string SupervisorsGuidedUniversity { get; set; }

        [Required(ErrorMessage = "*")]
        [StringLength(3, ErrorMessage = "Max 3 digits")]
        public string CoSupervisors { get; set; }
        public HttpPostedFileBase CoSupervisorsSuppDoc { get; set; }
        public string CoSupervisorsSuppDocName { get; set; }
        public string CoSupervisorsGuidedUniversity { get; set; }

        public string TotalPhDsGuided { get; set; }


        [Required(ErrorMessage = "*")]
        [StringLength(4, ErrorMessage = "Max 4 digits")]
        public string LectRatYears { get; set; }
        public bool LectRatactivitystatus { get; set; }
        public string LectRatDate { get; set; }
        public HttpPostedFileBase LectRatSuppDoc { get; set; }
        public string LectRatSuppDocName { get; set; }


        [Required(ErrorMessage = "*")]
        [StringLength(4, ErrorMessage = "Max 4 digits")]
        public string AssRatYears { get; set; }
        public bool AssRatactivitystatus { get; set; }
        public string AssRatDate { get; set; }
        public HttpPostedFileBase AssRatProfSuppDoc { get; set; }
        public string AssRatProfSuppDocName { get; set; }

        [Required(ErrorMessage = "*")]
        [StringLength(2, ErrorMessage = "Max 4 digits")]
        public string ProfRatYears { get; set; }
        public bool ProfRatactivitystatus { get; set; }
        public string ProfRatDate { get; set; }
        public HttpPostedFileBase ProfRatSuppDoc { get; set; }
        public string ProfRatSuppDocName { get; set; }

        [Required(ErrorMessage = "*")]
        [StringLength(4, ErrorMessage = "Max 4 digits")]
        public string PrincipalRatYears { get; set; }
        public bool PrincipalRatactivitystatus { get; set; }
        public string PrincipalRatDate { get; set; }
        public HttpPostedFileBase PrincipalRatSuppDoc { get; set; }
        public string PrincipalRatSuppDocName { get; set; }

        public string TotalRatExperience { get; set; }
    }
    #endregion
}
