using iTextSharp.text;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Security;
using iTextSharp.text.html.simpleparser;
using iTextSharp.text.pdf;
using UAAAS.Models;

namespace UAAAS.Controllers.Permanent_Affiliation
{
    [ErrorHandling]
    public class PA_SubmitDataController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();
        private string serverURL;

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult Affidavit(string collegeId)
        {
            int actualYear =
                db.jntuh_academic_year.Where(a => a.isPresentAcademicYear == true && a.isActive == true)
                    .Select(s => s.actualYear)
                    .FirstOrDefault();
            ViewBag.NextacademicYear = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.academicYear).FirstOrDefault();
            return View();
        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult Create(string collegeId)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(collegeUser => collegeUser.userID == userID).Select(collegeUser => collegeUser.collegeID).FirstOrDefault();
            int actualYear =
                       db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true)
                           .Select(a => a.actualYear)
                           .FirstOrDefault();
            int AY0 =
                db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1))
                    .Select(a => a.id)
                    .FirstOrDefault();
            var presentacademicyear = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1))
                    .Select(a => a.academicYear)
                    .FirstOrDefault();
            if (userCollegeID == 0 && collegeId != null)
            {
                if (Roles.IsUserInRole("Admin") || Roles.IsUserInRole("DataEntry"))
                {
                    userCollegeID = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(collegeId, WebConfigurationManager.AppSettings["CryptoKey"]));
                }
            }
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            string CollegeCode = db.jntuh_college.Where(C => C.id == userCollegeID).Select(C => C.collegeCode).FirstOrDefault();
            DateTime todayDate = DateTime.Now.Date;
            //int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int prAy = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();

            var status = GetPageEditableStatus(userCollegeID);

            int submitcollegeid = db.jntuh_pa_college_edit_status.Where(editStatus => editStatus.academicyearId == prAy && editStatus.collegeId == userCollegeID &&
                                                                                     editStatus.IsCollegeEditable == false)
                                                                .Select(editStatus => editStatus.id)
                                                                .FirstOrDefault();
            var Encls = db.jntuh_enclosures.Where(a => a.isActive == true && a.id != 39 && a.id != 40).ToList();
            var objEnclosures = Encls.Select(enc => new CollegeEnclosures
            {
                id = enc.id,
                collegeId = userCollegeID,
                documentName = enc.documentName,
            }).ToList();

            ViewBag.AllObjEnclosures = objEnclosures;

            var colgEnclsIds =
                db.jntuh_college_enclosures.Where(
                    a => a.collegeID == userCollegeID && a.isActive == false && a.academicyearId == AY0).Select(a => a.enclosureId).ToList();

            if (colgEnclsIds.Count > 0)
            {
                objEnclosures = objEnclosures.Where(i => !colgEnclsIds.Contains(i.id)).ToList();
                ViewBag.AllObjEnclosures = objEnclosures;
            }

            var cSpcIds =
              db.jntuh_college_intake_existing.Where(i => i.collegeId == userCollegeID && (i.academicYearId == prAy) && (i.proposedIntake != 0 && i.proposedIntake != null))
                //.GroupBy(r => new { r.specializationId })
                  .Select(s => s.specializationId)
                  .ToList();

            var StrPharmDEnclousres = db.jntuh_college_enclosures.Where(E => E.collegeID == userCollegeID && E.enclosureId == 39 && E.academicyearId == prAy).Select(E => E.path).FirstOrDefault();
            var StrPharPracticeEnclousres = db.jntuh_college_enclosures.Where(E => E.collegeID == userCollegeID && E.enclosureId == 40 && E.academicyearId == prAy).Select(E => E.path).FirstOrDefault();

            if (StrPharmDEnclousres == null && cSpcIds.Contains(18)) // pharm.d specialization
            {
                TempData["PharmDEnclousres"] = " Upload Hospital MoU Certificate Enclosure";
                var pharmdenc = new CollegeEnclosures()
                {
                    id = 39,
                    collegeId = userCollegeID,
                    documentName = "Hospital MoU Certificate Enclosure",
                };
                objEnclosures.Add(pharmdenc);
                ViewBag.AllObjEnclosures = objEnclosures;
            }

            if (StrPharPracticeEnclousres == null && cSpcIds.Contains(114))
            {
                TempData["PharPracticeEnclousres"] = " Upload Bed strength certificate from competent authority Enclosure";
                var pharmcypracticeenc = new CollegeEnclosures()
                {
                    id = 40,
                    collegeId = userCollegeID,
                    documentName = "Bed strength certificate from competent authority Enclosure",
                };
                objEnclosures.Add(pharmcypracticeenc);
                ViewBag.AllObjEnclosures = objEnclosures;
            }

            var currentYear = DateTime.Now.Year;
            int macCollegeId = db.jntuh_college_macaddress.Where(E => E.collegeId == userCollegeID).Select(E => E.collegeId).FirstOrDefault();
            string StrPartA = db.jntuh_college_enclosures.Where(E => E.collegeID == userCollegeID && E.enclosureId == 20 && E.academicyearId == AY0).Select(E => E.path).FirstOrDefault();
            string StrPartB = db.jntuh_college_enclosures.Where(E => E.collegeID == userCollegeID && E.enclosureId == 21 && E.academicyearId == AY0).Select(E => E.path).FirstOrDefault();
            string StrPartAffidavit = db.jntuh_college_enclosures.Where(E => E.collegeID == userCollegeID && E.enclosureId == 18 && E.academicyearId == AY0).Select(E => E.path).FirstOrDefault();
            string StrAllEnclousres = db.jntuh_college_enclosures.Where(E => E.collegeID == userCollegeID && E.enclosureId == 22 && E.academicyearId == AY0).Select(E => E.path).FirstOrDefault();
            string StrAffidavit = db.jntuh_college_enclosures.Where(E => E.collegeID == userCollegeID && E.enclosureId == 24 && E.academicyearId == AY0).Select(E => E.path).FirstOrDefault();
            string StrCollegeFee = db.jntuh_paymentresponse.Where(E => E.CollegeId == CollegeCode && E.AuthStatus == "0300" && E.PaymentTypeID == 7 && E.AcademicYearId == AY0 && E.TxnDate.Year == currentYear).Select(E => E.ErrorDescription).FirstOrDefault();

            string clgCode = db.jntuh_college.Where(C => C.id == userCollegeID).Select(C => C.collegeCode).FirstOrDefault();
            var isLateFeePaid = db.jntuh_paymentresponse.Count(it => it.CollegeId == clgCode && it.TxnDate.Year == currentYear && it.AuthStatus == "0300" && it.PaymentTypeID == 8 && it.AcademicYearId == AY0) > 0;

            //Checking the Condition of Latefee ,For Example: College Paid 25% but College try to submit in 50% Date then that Time Ask to Pay 50% Late Fee
            #region LateFee Condiation Checking



            #region Cal
            //List<jntuh_college_intake_existing> intake = db.jntuh_college_intake_existing.Where(i => i.collegeId == userCollegeID).ToList();
            //var intakeExisting = intake.GroupBy(r => new { r.specializationId, r.shiftId }).Select(r => r.First()).ToList();
            //List<int> pgdegrees = new List<int>();
            //List<int> ugdegrees = new List<int>();
            //List<int> totaldegrees = new List<int>();
            //long ugSpecializationAmmount = 0;
            //long pgSpecializationAmmount = 0;
            //long applicationFee = 0;
            //int ugCount = 0;
            //int pgCount = 0;
            //int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            //int AY0 = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();
            //foreach (var item in intakeExisting)
            //{
            //    jntuh_college_intake_existing details = db.jntuh_college_intake_existing.Where(e => e.collegeId == userCollegeID && e.academicYearId == AY0 && e.specializationId == item.specializationId && e.shiftId == item.shiftId).Select(e => e).FirstOrDefault();
            //    if (details != null)
            //    {
            //        if (item.jntuh_specialization.jntuh_department.degreeId == 5 || item.jntuh_specialization.jntuh_department.degreeId == 4)
            //        {
            //            if (item.proposedIntake != 0 && item.courseStatus != "Closure")
            //            {
            //                ugdegrees.Add(item.jntuh_specialization.jntuh_department.degreeId);
            //                if (ugdegrees.Count != 0 && ugdegrees.Count <= 4)
            //                {
            //                    ugSpecializationAmmount = 25000;
            //                    totaldegrees.AddRange(ugdegrees);
            //                }
            //                else
            //                {
            //                    ugCount++;
            //                    ugSpecializationAmmount = 25000 + (ugCount * 4000);
            //                    totaldegrees.AddRange(ugdegrees);
            //                }
            //            }

            //        }
            //        else
            //        {
            //            if (item.proposedIntake != 0 && item.courseStatus != "Closure")
            //            {
            //                pgdegrees.Add(item.jntuh_specialization.jntuh_department.degreeId);
            //                pgCount++;
            //                pgSpecializationAmmount = pgCount * 12000;
            //                totaldegrees.AddRange(pgdegrees);
            //            }

            //        }
            //    }
            //}
            //if (pgdegrees.Count > 0 && ugdegrees.Count > 0)
            //    applicationFee = 1000;
            //else
            //    applicationFee = 750;
            #endregion
            // var currentYear = DateTime.Now.Year;
            // var isPaid = db.jntuh_paymentresponse.Count(it => it.CollegeId == clgCode && it.TxnDate.Year == currentYear && it.AuthStatus == "0300" && it.PaymentType == "Latefee") > 0;
            // ViewBag.IsLatePaymentDone = isPaid;

            decimal paidamount = db.jntuh_paymentresponse.Where(E => E.CollegeId == CollegeCode && E.AuthStatus == "0300" && E.PaymentTypeID == 7 && E.AcademicYearId == AY0).Select(E => E.TxnAmount).FirstOrDefault();
            var totalLateFee = 0.0;
            var currentDate = DateTime.Now;
            if (!isLateFeePaid)
            {
                var amount = (long)paidamount;

                if (currentDate >= new DateTime(2020, 3, 17, 00, 00, 00) && currentDate <= new DateTime(2020, 3, 18, 23, 59, 59))
                {
                    totalLateFee = amount / 4.0;
                }
                if (currentDate >= new DateTime(2020, 3, 19) && currentDate <= new DateTime(2020, 3, 20, 23, 59, 59))
                {
                    totalLateFee = amount / 2.0;
                }
                if (currentDate >= new DateTime(2020, 3, 21) && currentDate <= new DateTime(2020, 3, 23, 16, 59, 59))
                {
                    totalLateFee = amount;
                }
            }
            else //Once Late Fee Paid Checking 
            {
                var LateFeePaid = 0.0;
                var lateFeeData = db.jntuh_paymentresponse.Where(it => it.CollegeId == clgCode && it.TxnDate.Year == currentYear && it.AuthStatus == "0300" && it.PaymentTypeID == 8 && it.AcademicYearId == AY0).Select(it => it.TxnAmount).ToList();
                foreach (var late in lateFeeData)
                {
                    if (late != null)
                        LateFeePaid += (double)late;
                }
                var amount = (long)paidamount;

                if (currentDate >= new DateTime(2020, 3, 17, 00, 00, 00) && currentDate <= new DateTime(2020, 3, 18, 23, 59, 59))
                {
                    totalLateFee = amount / 4.0;
                }
                if (currentDate >= new DateTime(2020, 3, 19) && currentDate <= new DateTime(2020, 3, 20, 23, 59, 59))
                {
                    totalLateFee = amount / 2.0;
                }
                if (currentDate >= new DateTime(2020, 3, 21) && currentDate <= new DateTime(2020, 3, 23, 16, 59, 59))
                {
                    totalLateFee = amount;
                }

                //if (currentDate >= new DateTime(2018, 2, 22, 00, 00, 00) && currentDate <= new DateTime(2018, 2, 23, 23, 59, 59))
                //{
                //    totalLateFee = amount / 4.0;
                //}
                //if (currentDate >= new DateTime(2018, 2, 24) && currentDate <= new DateTime(2018, 2, 25, 23, 59, 59))
                //{
                //    totalLateFee = amount / 2.0;
                //}
                //if (currentDate >= new DateTime(2018, 2, 26) && currentDate <= new DateTime(2018, 2, 27, 23, 59, 59))
                //{
                //    totalLateFee = amount;
                //}

                totalLateFee = totalLateFee - LateFeePaid;
                if (totalLateFee < 30)
                    totalLateFee = 0;

            }

            // //Get All LateFee Amount 
            #endregion

            var macSpcIds =
                db.jntuh_college_intake_existing.Where(i => i.collegeId == userCollegeID && (i.academicYearId == prAy - 1 || i.academicYearId == prAy - 2 || i.academicYearId == prAy - 3) && (i.approvedIntake != 0 || i.aicteApprovedIntake != 0))
                    .Select(s => s.specializationId)
                    .ToList();

            var currentDate1 = DateTime.Now;
            if (currentDate1 <= new DateTime(2016, 3, 15))
            {
            }
            ViewBag.IsLatePaymentDone = isLateFeePaid;
            if ((isLateFeePaid == true && totalLateFee == 0.0) || totalLateFee < 0)
            {

                //Please Pay The Late Fee. Colleges are requested to pay the Fee from Online Payment Portal. Please logout from Application here and login into online payment portal for Payments.
            }
            else
            {
                if (currentDate >= new DateTime(2020, 03, 17, 00, 00, 00))
                    TempData["LateFee"] = "You are hereby  informed to pay the Application + Inspection fees along with the late fee charges as per the circular published at UAAC portal dated 11-03-2020.";
            }

            if (macCollegeId == 0 && macSpcIds.Count > 0)
            {
                TempData["MAC"] =
                    "The MAC addresses of the Computers available in the College have to be given in a “single excel sheet” room-wise (as per the excel format provided).";
            }

            if (StrPartA == null || StrPartA == "")
            {

                TempData["PartA"] = "Upload AICTE Part A File";
            }
            if (StrPartB == null || StrPartB == "")
            {

                TempData["PartB"] = "Upload AICTE Part B File";
            }
            if (StrPartAffidavit == null || StrPartAffidavit == "")
            {

                TempData["Affidavit"] = "Upload  Affidavit File";
            }
            if (StrCollegeFee == null || StrCollegeFee == "")
            {
                TempData["CollegeFee"] = "College is informed to pay the requiste Application Processing Fee + Inspection Fee as per University norms for the Affiliation process for AY " + presentacademicyear + ".";
                //" Pay The Application and Inspection Fee";
            }
            if (StrAllEnclousres == null || StrAllEnclousres == "")
            {
                TempData["AllEnclousres"] = " Upload AICTE Part A & Part B Enclosures";
            }
            if (StrAffidavit == null || StrAffidavit == "")
            {

                TempData["Declaration"] = "Upload DECLARATION & DATA SUBMISSION";


            }
            ViewBag.Issubmit = false;
            if (submitcollegeid != 0)
            {
                ViewBag.Issubmit = true;
            }
            if (status == 0 && Roles.IsUserInRole("College"))
            {
                ViewBag.NotUpload = true;
            }
            else
            {
                ViewBag.NotUpload = false;
            }

            int InspectionPhaseId = db.jntuh_inspection_phase.Where(p => p.isActive == true && p.inspectionPhase == "Data Entry").Select(p => p.id).SingleOrDefault();

            if (Roles.IsUserInRole("DataEntry"))
            {
                ViewBag.IsCompleted = true;
                var isCompleted = db.jntuh_dataentry_allotment.Where(d => d.collegeID == userCollegeID && d.isActive == true && d.InspectionPhaseId == InspectionPhaseId).Select(d => d.isCompleted).FirstOrDefault();
                if (isCompleted == true)
                {
                    ViewBag.IsCompleted = false;
                }
            }

            if (Roles.IsUserInRole("Admin"))
            {
                ViewBag.IsVerified = true;
                var isVerified = db.jntuh_dataentry_allotment.Where(d => d.collegeID == userCollegeID && d.isActive == true && d.InspectionPhaseId == InspectionPhaseId).Select(d => d.isVerified).FirstOrDefault();
                if (isVerified == true)
                {
                    ViewBag.IsVerified = false;
                }
            }

            SubmitData submitData = new SubmitData();
            submitData.collegeId = userCollegeID;
            ViewBag.CollegeId = UAAAS.Models.Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString();

            return View(submitData);
        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        [HttpPost]
        public ActionResult Create(SubmitData submitData, string cmd)
        {
            if (Membership.GetUser() == null)
            {
                return RedirectToAction("LogOn", "Account");
            }
            int InspectionPhaseId = db.jntuh_inspection_phase.Where(p => p.isActive == true).Select(p => p.id).SingleOrDefault();

            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);

            int userCollegeID = db.jntuh_college_users.Where(collegeUser => collegeUser.userID == userID).Select(collegeUser => collegeUser.collegeID).FirstOrDefault();
            //int userCollegeID=177;
            if (userCollegeID == 0)
            {
                userCollegeID = submitData.collegeId;
                if (userCollegeID == 0)
                {
                    return RedirectToAction("Create", "CollegeInformation");
                }
            }
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            jntuh_dataentry_allotment jntuh_dataentry_allotment = db.jntuh_dataentry_allotment.Where(d => d.collegeID == userCollegeID && d.isActive == true && d.InspectionPhaseId == InspectionPhaseId).Select(d => d).FirstOrDefault();


            DateTime todayDate = DateTime.Now.Date;
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int prAy = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
            int status = db.jntuh_college_edit_status.Where(editStatus => editStatus.academicyearId == prAy && editStatus.collegeId == userCollegeID &&
                                                                                      editStatus.IsCollegeEditable == true &&
                                                                                      editStatus.editFromDate <= todayDate &&
                                                                                      editStatus.editToDate >= todayDate)
                                                                 .Select(editStatus => editStatus.id)
                                                                 .FirstOrDefault();


            if (cmd == "Submit")
            {
                //Once PAid Fee before Submission Date and try to click on the Submit button Data Not Saved into Database.
                if (status != 0 && Roles.IsUserInRole("College"))
                {
                    SaveSubmitData(submitData);
                }

            }

            if (cmd == "Data Entry Completed")
            {
                if (jntuh_dataentry_allotment != null)
                {
                    jntuh_dataentry_allotment.id = jntuh_dataentry_allotment.id;
                    jntuh_dataentry_allotment.userID = jntuh_dataentry_allotment.userID;
                    jntuh_dataentry_allotment.collegeID = jntuh_dataentry_allotment.collegeID;
                    jntuh_dataentry_allotment.isActive = jntuh_dataentry_allotment.isActive;
                    jntuh_dataentry_allotment.isCompleted = true;
                    jntuh_dataentry_allotment.isVerified = jntuh_dataentry_allotment.isVerified;
                    jntuh_dataentry_allotment.createdBy = jntuh_dataentry_allotment.createdBy;
                    jntuh_dataentry_allotment.createdOn = jntuh_dataentry_allotment.createdOn;
                    jntuh_dataentry_allotment.updatedBy = userID;
                    jntuh_dataentry_allotment.updatedOn = DateTime.Now;
                    db.Entry(jntuh_dataentry_allotment).State = EntityState.Modified;
                    db.SaveChanges();
                    //return RedirectToAction("Create", new { collegeId = UAAAS.Models.Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
                    return RedirectToAction("DataEntryAssignedColleges", "DataEntryAssignedColleges");

                }
            }

            if (cmd == "Data Entry Verified")
            {
                if (jntuh_dataentry_allotment != null)
                {
                    jntuh_dataentry_allotment.id = jntuh_dataentry_allotment.id;
                    jntuh_dataentry_allotment.userID = jntuh_dataentry_allotment.userID;
                    jntuh_dataentry_allotment.collegeID = jntuh_dataentry_allotment.collegeID;
                    jntuh_dataentry_allotment.isActive = jntuh_dataentry_allotment.isActive;
                    jntuh_dataentry_allotment.isCompleted = jntuh_dataentry_allotment.isCompleted;
                    jntuh_dataentry_allotment.isVerified = true;
                    jntuh_dataentry_allotment.createdBy = jntuh_dataentry_allotment.createdBy;
                    jntuh_dataentry_allotment.createdOn = jntuh_dataentry_allotment.createdOn;
                    jntuh_dataentry_allotment.updatedBy = userID;
                    jntuh_dataentry_allotment.updatedOn = DateTime.Now;
                    db.Entry(jntuh_dataentry_allotment).State = EntityState.Modified;
                    db.SaveChanges();
                    //return RedirectToAction("Create", new { collegeId = UAAAS.Models.Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
                    return RedirectToAction("DataEntryVerifiedColleges", "DataEntryVerifiedColleges");
                }
            }

            //ViewBag.CollegeId = userCollegeID;
            ViewBag.CollegeId = UAAAS.Models.Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString();
            return View(submitData);
        }

        private void SaveSubmitData(SubmitData submitData)
        {
            if (ModelState.IsValid)
            {
                int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
                int userCollegeID = db.jntuh_college_users.Where(collegeUser => collegeUser.userID == userID).Select(collegeUser => collegeUser.collegeID).FirstOrDefault();
                if (userCollegeID == 375)
                {
                    userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
                }
                int actualYear =
                       db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true)
                           .Select(a => a.actualYear)
                           .FirstOrDefault();
                int AY0 =
                    db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1))
                        .Select(a => a.id)
                        .FirstOrDefault();
                string clgCode = db.jntuh_college.Where(C => C.id == userCollegeID).Select(C => C.collegeCode).FirstOrDefault();
                var currentYear = DateTime.Now.Year;
                var isPaid = db.jntuh_paymentresponse.Count(it => it.CollegeId == clgCode && it.TxnDate.Year == currentYear && it.AuthStatus == "0300" && it.PaymentTypeID == 7 && it.AcademicYearId == AY0) > 0;

                ViewBag.IsLatePaymentDone = isPaid;
                if (isPaid)
                {

                    //jntuh_college_edit_status collegeEditStatus = new jntuh_college_edit_status();
                    jntuh_college_edit_status collegeEditStatus = db.jntuh_college_edit_status.AsNoTracking().Where(editStatus => editStatus.collegeId == userCollegeID && editStatus.academicyearId == AY0)
                                                              .Select(editStatus => editStatus)
                                                              .FirstOrDefault();
                    if (collegeEditStatus != null)
                    {
                        //collegeEditStatus.id = existEditStatusId;
                        //collegeEditStatus.academicyearId = AY0;
                        //collegeEditStatus.collegeId = userCollegeID;
                        //collegeEditStatus.editFromDate = db.jntuh_college_edit_status.AsNoTracking().Where(editStatus => editStatus.collegeId == userCollegeID)
                        //                                      .Select(editStatus => editStatus.editFromDate)
                        //                                      .FirstOrDefault();
                        //collegeEditStatus.editToDate = db.jntuh_college_edit_status.AsNoTracking().Where(editStatus => editStatus.collegeId == userCollegeID)
                        //                                      .Select(editStatus => editStatus.editToDate)
                        //                                      .FirstOrDefault();
                        //collegeEditStatus.createdBy = db.jntuh_college_edit_status.AsNoTracking().Where(editStatus => editStatus.collegeId == userCollegeID)
                        //                                      .Select(editStatus => editStatus.createdBy)
                        //                                      .FirstOrDefault();
                        //collegeEditStatus.createdOn = db.jntuh_college_edit_status.AsNoTracking().Where(editStatus => editStatus.collegeId == userCollegeID)
                        //                                      .Select(editStatus => editStatus.createdOn)
                        //                                      .FirstOrDefault();
                        collegeEditStatus.updatedOn = DateTime.Now;
                        collegeEditStatus.updatedBy = userID;
                        if (submitData.IsCollegeEditable == true)
                        {
                            collegeEditStatus.IsCollegeEditable = false;
                            //collegeEditStatus.isSubmitted = true;
                        }
                        else
                        {
                            collegeEditStatus.IsCollegeEditable = true;
                        }
                        db.Entry(collegeEditStatus).State = EntityState.Modified;
                        db.SaveChanges();

                        if (submitData.collegeEditRemarks != null)
                        {
                            jntuh_college_edit_remarks collegeEditRemarks = new jntuh_college_edit_remarks();

                            int existEditRemarksId = db.jntuh_college_edit_remarks.AsNoTracking().Where(editRemarks => editRemarks.collegeId == userCollegeID)
                                                                                      .Select(editRemarks => editRemarks.id)
                                                                                      .FirstOrDefault();
                            if (existEditRemarksId == 0)
                            {
                                collegeEditRemarks.collegeId = userCollegeID;
                                collegeEditRemarks.collegeEditRemarks = submitData.collegeEditRemarks;
                                collegeEditRemarks.isCollegeRemarks = true;
                                collegeEditRemarks.createdBy = userID;
                                collegeEditRemarks.createdOn = DateTime.Now;
                                db.jntuh_college_edit_remarks.Add(collegeEditRemarks);
                                db.SaveChanges();
                            }
                        }

                        TempData["Success"] = "Data Submitted SuccessFully.";
                    }
                }
                else
                {
                    TempData["Error"] = "Please Pay The Late Fee. Colleges are requested to pay the Fee from Online Payment Portal. Please logout from Application here and login into online payment portal for Payments.";
                }

            }
        }

        //[Authorize(Roles = "College,Admin")]
        //public ActionResult CollegeData2(int preview, string strcollegeId)
        //{
        //    serverURL = Request.Url.Scheme + System.Uri.SchemeDelimiter + Request.Url.Host + (Request.Url.IsDefaultPort ? "" : ":" + Request.Url.Port);
        //    int collegeId = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(strcollegeId, WebConfigurationManager.AppSettings["CryptoKey"]));
        //    string pdfPath = string.Empty;
        //    if (collegeId == 375)
        //    {
        //        collegeId = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
        //    }
        //    string collegeCode = db.jntuh_college.Find(collegeId).collegeCode;

        //    if (preview == 0)
        //    {
        //        pdfPath = SaveCollegeDataPdf2(preview, collegeId);
        //        pdfPath = pdfPath.Replace("/", "\\");

        //    }
        //    else
        //    {
        //        int[] collegeIDs = db.jntuh_college.Where(c => c.isActive == true).Select(c => c.id).ToArray();
        //        foreach (var cid in collegeIDs)
        //        {
        //            collegeId = cid;
        //            pdfPath = SaveCollegeDataPdf(preview, collegeId);
        //            collegeCode = db.jntuh_college.Find(collegeId).collegeCode;
        //            pdfPath = pdfPath.Replace("/", "\\");
        //        }

        //    }
        //    //return File(pdfPath, "application/pdf", "A-114-" + collegeCode + ".pdf");
        //    return File(pdfPath, "application/pdf", "Acknowledgement-" + collegeCode + ".pdf");


        //}

        //private string SaveCollegeDataPdf(int preview, int collegeId)
        //{
        //    string fullPath = string.Empty;
        //    //Set page size as A4
        //    Document pdfDoc = new Document(PageSize.A4, 60, 50, 60, 60);
        //    string path = Server.MapPath("~/Content/PDFReports");
        //    var CollegeRecord = db.jntuh_college.Find(collegeId);
        //    string collegeCode = CollegeRecord.collegeCode;
        //    string collegeName = CollegeRecord.collegeName;

        //    if (preview == 0)
        //    {
        //        fullPath = path + "/temp/A-119_" + collegeCode + "_" + DateTime.Now.ToString("yyyMMddHHmmss") + ".pdf";
        //        PdfWriter pdfWriter = PdfWriter.GetInstance(pdfDoc, new FileStream(fullPath, FileMode.Create));
        //        ITextEvents iTextEvents = new ITextEvents();
        //        iTextEvents.CollegeName = collegeName;
        //        iTextEvents.CollegeCode = collegeCode;
        //        iTextEvents.formType = "A-119";
        //        pdfWriter.PageEvent = iTextEvents;

        //    }
        //    else if (preview == 1)
        //    {
        //        string path1 = path + "/CollegeData/A-119/" + DateTime.Now.ToString("dd-MM-yyyy");
        //        if (!Directory.Exists(path1))
        //        {
        //            Directory.CreateDirectory(path1);
        //        }
        //        fullPath = path1 + "/A-119_" + collegeCode + ".pdf";
        //        PdfWriter pdfWriter = PdfWriter.GetInstance(pdfDoc, new FileStream(fullPath, FileMode.Create));
        //        ITextEvents iTextEvents = new ITextEvents();
        //        iTextEvents.CollegeName = collegeName;
        //        iTextEvents.CollegeCode = collegeCode;
        //        iTextEvents.formType = "A-119";
        //        pdfWriter.PageEvent = iTextEvents;
        //    }
        //    else
        //    {
        //        fullPath = path + "/temp/A-119_" + collegeCode + "_" + DateTime.Now.ToString("yyyMMddHHmmss") + ".pdf";
        //        PdfWriter pdfWriter = PdfWriter.GetInstance(pdfDoc, new FileStream(fullPath, FileMode.Create));
        //        ITextEvents iTextEvents = new ITextEvents();
        //        iTextEvents.CollegeName = collegeName;
        //        iTextEvents.CollegeCode = collegeCode;
        //        iTextEvents.formType = "A-119";
        //        pdfWriter.PageEvent = iTextEvents;
        //    }

        //    // Create a new PdfWrite object, writing the output to a MemoryStream
        //    //open pdf
        //    var output = new MemoryStream();
        //    var writer = PdfWriter.GetInstance(pdfDoc, output);

        //    //Open PDF Document to write data
        //    pdfDoc.Open();

        //    //Assign Html content in a string to write in PDF
        //    string contents = string.Empty;

        //    StreamReader sr;

        //    //Read file from server path
        //    //sr = System.IO.File.OpenText(Server.MapPath("~/Content/A-114.html"));
        //    sr = System.IO.File.OpenText(Server.MapPath("~/Content/A-118.html"));
        //    //store content in the variable
        //    contents = sr.ReadToEnd();
        //    sr.Close();

        //    contents = contents.Replace("##SERVERURL##", serverURL);
        //    contents = affiliationType(collegeId, contents);
        //    contents = collegeInformation(collegeId, contents);
        //    contents = EducationalSocietyDetails(collegeId, contents);
        //    contents = PrincipalDirectorDetails(collegeId, contents);
        //    contents = ChairpersonDetails(collegeId, contents);
        //    contents = OthercollegesandOtherCoursesDetails(collegeId, contents);
        //    contents = LandInformationDetails(collegeId, contents);
        //    contents = AdministrativeLandDetails(collegeId, contents);
        //    contents = InstructionalAreaDetails(collegeId, contents);
        //    contents = ExistingIntakeDetails(collegeId, contents);
        //    contents = AcademicPerformanceDetails(collegeId, contents);

        //    contents = collegeTachingFacultyMembers(collegeId, contents);
        //    contents = collegeNonTachingFacultyMembers(collegeId, contents);
        //    contents = collegeTechnicalFacultyMembers(collegeId, contents);

        //    contents = LaboratoriesDetails(collegeId, contents);
        //    ////In place of LibraryDetails - LibraryInformation LibraryBooks Computers
        //    contents = LibraryDetails(collegeId, contents);

        //    ////ComputerStudentRatio comes in LibraryDetails

        //    contents = InternetBandwidthDetails(collegeId, contents);
        //    contents = LegalSystemSoftwareDetails(collegeId, contents);
        //    contents = PrintersDetails(collegeId, contents);
        //    contents = ExaminationBranchDetails(collegeId, contents);
        //    ////EDEP comes in  ExaminationBranchDetails

        //    ////Grievance Redressal comes in DesirableRequirementsDetails
        //    contents = DesirableRequirementsDetails(collegeId, contents);
        //    contents = AntiRaggingCommitteeDetails(collegeId, contents);
        //    contents = WomenProtectionCellDetails(collegeId, contents);
        //    contents = RTIDetails(collegeId, contents);

        //    ////sports comes and Desirable Requirements in OtherDesirablesDetails
        //    contents = OtherDesirablesDetails(collegeId, contents);
        //    contents = CampusHostelMaintenanceDetails(collegeId, contents);
        //    contents = OperationalFundsDetails(collegeId, contents);
        //    contents = IncomeDetails(collegeId, contents);
        //    contents = ExpenditureDetails(collegeId, contents);
        //    contents = StudentsPlacementDetails(collegeId, contents);
        //    contents = CollegePhotosDetails(collegeId, contents);
        //    contents = PaymentDetails(collegeId, contents);
        //    contents = PaymentOfFee(collegeId, contents);
        //    contents = collegeEnclosures(collegeId, contents);
        //    // contents = ExperimentDetails(collegeId, contents);
        //    contents = NOofPhysicalLabs(collegeId, contents);

        //    #region Getting Error Commented by Srinivas
        //    contents = PaymentBillDetails(collegeId, contents);
        //    //contents = barcodegenerator(collegeId, contents);
        //    contents = LateFeePaymentBillDetails(collegeId, contents);
        //    //contents = LateFeebarcodegenerator(collegeId, contents);
        //    #endregion

        //    ////contents = CollegeOverallFacultyStudentRatio(collegeId, contents);
        //    contents = DataModifications(collegeId, contents);

        //    // contents = AffidavitDetails(collegeId, contents);
        //    //Read string contents using stream reader and convert html to parsed conent
        //    List<IElement> parsedHtmlElements = HTMLWorker.ParseToList(new StringReader(contents), null);

        //    //Get each array values from parsed elements and add to the PDF document
        //    bool pageRotated = false;
        //    int count = 0;
        //    foreach (var htmlElement in parsedHtmlElements)
        //    {
        //        count++;
        //        if (count == 100)
        //        {

        //        }
        //        if (htmlElement.Equals("<textarea>"))
        //        {
        //            pdfDoc.NewPage();
        //        }

        //        if (htmlElement.Chunks.Count >= 3)
        //        {
        //            if (htmlElement.Chunks.Count == 4)
        //            {
        //                pdfDoc.SetPageSize(iTextSharp.text.PageSize.A4.Rotate());
        //                pdfDoc.SetMargins(60, 50, 60, 60);
        //                pageRotated = true;
        //            }
        //            else
        //            {
        //                if (pageRotated == true)
        //                {
        //                    pdfDoc.SetPageSize(iTextSharp.text.PageSize.A4);
        //                    pdfDoc.SetMargins(60, 50, 60, 60);
        //                    pageRotated = false;
        //                }
        //            }

        //            pdfDoc.NewPage();

        //        }
        //        else
        //        {
        //            pdfDoc.Add(htmlElement as IElement);
        //        }
        //    }

        //    //Close your PDF
        //    pdfDoc.Close();
        //    if (pdfDoc.IsOpen())
        //    {
        //        pdfDoc.Close();
        //    }

        //    string returnPath = string.Empty;
        //    returnPath = fullPath;
        //    return returnPath;
        //}

        //private string SaveCollegeDataPdf2(int preview, int collegeId)
        //{
        //    string fullPath = string.Empty;
        //    //Set page size as A4
        //    Document pdfDoc = new Document(PageSize.A4, 60, 50, 60, 60);
        //    string path = Server.MapPath("~/Content/PDFReports/Acknowledgement/");

        //    string collegeCode = db.jntuh_college.Find(collegeId).collegeCode;
        //    string collegeName = db.jntuh_college.Find(collegeId).collegeName;
        //    int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
        //    int prAy = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
        //    jntuh_college_edit_status editstatus =
        //        db.jntuh_college_edit_status.Where(e => e.academicyearId == prAy && e.collegeId == collegeId).Select(s => s).FirstOrDefault();
        //    //if (preview == 0)
        //    //{
        //    if (!Directory.Exists(path))
        //    {
        //        Directory.CreateDirectory(path);
        //    }
        //    //fullPath = path + "/temp/A-114_" + collegeCode + "_" + DateTime.Now.ToString("yyyMMddHHmmss") + ".pdf";
        //    fullPath = path + collegeCode + "_" + collegeName.Substring(0, 3) + "_" + DateTime.Now.ToString("yyyMMddHHmmss") + ".pdf";

        //    PdfWriter pdfWriter = PdfWriter.GetInstance(pdfDoc, new FileStream(fullPath, FileMode.Create));
        //    ITextEvents iTextEvents = new ITextEvents();
        //    iTextEvents.CollegeName = collegeName;
        //    iTextEvents.CollegeCode = collegeCode;
        //    //iTextEvents.formType = "A-114";
        //    iTextEvents.formType = "Acknowledgement";
        //    pdfWriter.PageEvent = iTextEvents;

        //    //}

        //    //else if (preview == 1)
        //    //{
        //    //    //string path1 = path + "/CollegeData/A-114/" + DateTime.Now.ToString("dd-MM-yyyy");
        //    //    string path1 = path + "/CollegeData/Acknowledgement/" + DateTime.Now.ToString("dd-MM-yyyy");
        //    //    if (!Directory.Exists(path1))
        //    //    {
        //    //        Directory.CreateDirectory(path1);
        //    //    }
        //    //    //fullPath = path1 + "/A-114_" + collegeCode + ".pdf";
        //    //    fullPath = path1 + "/A-550_" + collegeCode + ".pdf";
        //    //    PdfWriter pdfWriter = PdfWriter.GetInstance(pdfDoc, new FileStream(fullPath, FileMode.Create));
        //    //    ITextEvents iTextEvents = new ITextEvents();
        //    //    iTextEvents.CollegeName = collegeName;
        //    //    iTextEvents.CollegeCode = collegeCode;
        //    //    //iTextEvents.formType = "A-114";
        //    //    iTextEvents.formType = "Acknowledgement";
        //    //    pdfWriter.PageEvent = iTextEvents;
        //    //}
        //    //else
        //    //{
        //    //    //fullPath = path + "/temp/A-114_" + collegeCode + "_" + DateTime.Now.ToString("yyyMMddHHmmss") + ".pdf";
        //    //    fullPath = path + "/temp/A-550_" + collegeCode + "_" + DateTime.Now.ToString("yyyMMddHHmmss") + ".pdf";
        //    //    PdfWriter pdfWriter = PdfWriter.GetInstance(pdfDoc, new FileStream(fullPath, FileMode.Create));
        //    //    ITextEvents iTextEvents = new ITextEvents();
        //    //    iTextEvents.CollegeName = collegeName;
        //    //    iTextEvents.CollegeCode = collegeCode;
        //    //    //iTextEvents.formType = "A-114";
        //    //    iTextEvents.formType = "Acknowledgement";
        //    //    pdfWriter.PageEvent = iTextEvents;
        //    //}

        //    // Create a new PdfWrite object, writing the output to a MemoryStream
        //    //open pdf
        //    //var output = new MemoryStream();
        //    //var writer = PdfWriter.GetInstance(pdfDoc, output);

        //    //Open PDF Document to write data
        //    pdfDoc.Open();

        //    //Assign Html content in a string to write in PDF
        //    string contents = string.Empty;

        //    StreamReader sr;

        //    //Read file from server path
        //    //sr = System.IO.File.OpenText(Server.MapPath("~/Content/A-114.html"));
        //    sr = System.IO.File.OpenText(Server.MapPath("~/Content/Acknowledgement.html"));
        //    //store content in the variable
        //    contents = sr.ReadToEnd();
        //    sr.Close();

        //    contents = contents.Replace("##SERVERURL##", serverURL);
        //    contents = contents.Replace("##CURRENTDATE##", editstatus.updatedOn.ToString());
        //    contents = affiliationType(collegeId, contents);
        //    contents = collegeInformation(collegeId, contents);
        //    //payment details
        //    contents = PaymentBillDetails(collegeId, contents);
        //    contents = barcodegenerator(collegeId, contents);


        //    //Read string contents using stream reader and convert html to parsed conent
        //    var parsedHtmlElements = HTMLWorker.ParseToList(new StringReader(contents), null);

        //    //Get each array values from parsed elements and add to the PDF document
        //    bool pageRotated = false;

        //    foreach (var htmlElement in parsedHtmlElements)
        //    {
        //        if (htmlElement.Equals("<textarea>"))
        //        {
        //            pdfDoc.NewPage();
        //        }

        //        if (htmlElement.Chunks.Count >= 3)
        //        {
        //            if (htmlElement.Chunks.Count == 4)
        //            {
        //                pdfDoc.SetPageSize(iTextSharp.text.PageSize.A4.Rotate());
        //                pdfDoc.SetMargins(60, 50, 60, 60);
        //                pageRotated = true;
        //            }
        //            else
        //            {
        //                if (pageRotated == true)
        //                {
        //                    pdfDoc.SetPageSize(iTextSharp.text.PageSize.A4);
        //                    pdfDoc.SetMargins(60, 50, 60, 60);
        //                    pageRotated = false;
        //                }
        //            }

        //            pdfDoc.NewPage();

        //        }
        //        else
        //        {
        //            pdfDoc.Add(htmlElement as IElement);
        //        }
        //    }

        //    //Close your PDF
        //    pdfDoc.Close();
        //    if (pdfDoc.IsOpen())
        //    {
        //        pdfDoc.Close();
        //    }

        //    string returnPath = string.Empty;
        //    returnPath = fullPath;
        //    return returnPath;
        //}
    }
}
