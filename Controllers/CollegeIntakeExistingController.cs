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
    public class CollegeIntakeExistingController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult Index(string collegeId)
        {
            //if (!User.IsInRole("Admin"))
            //{
            //    return RedirectToAction("Index", "UnderConstruction");
            //}
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID =
                db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();

            if (userCollegeID == 0 && collegeId != null)
            {
                if (Roles.IsUserInRole("Admin") || Roles.IsUserInRole("DataEntry"))
                {
                    userCollegeID =
                        Convert.ToInt32(Utilities.DecryptString(collegeId,
                            WebConfigurationManager.AppSettings["CryptoKey"]));
                }
            }
            //if Test college Login we get college id from web.config file
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }

            //Checking Edit Option
            DateTime todayDate = DateTime.Now.Date;
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int prAy =
                db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
            int status = db.jntuh_college_edit_status.Where(editStatus => editStatus.academicyearId == prAy && editStatus.collegeId == userCollegeID && editStatus.academicyearId == prAy &&
                                                                                                editStatus.IsCollegeEditable == true &&
                                                                                                editStatus.editFromDate <= todayDate &&
                                                                                                editStatus.editToDate >= todayDate)
                                                                           .Select(editStatus => editStatus.id)
                                                                           .FirstOrDefault();
            if (userCollegeID == 0 && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("Create", "CollegeInformation");
            }
            if (userCollegeID == 0 && (Roles.IsUserInRole("Admin") || Roles.IsUserInRole("DataEntry")))
            {
                return RedirectToAction("Create", "CollegeInformation",
                    new
                    {
                        collegeId =
                            Utilities.EncryptString(userCollegeID.ToString(),
                                WebConfigurationManager.AppSettings["CryptoKey"]).ToString()
                    });
            }
            if (status == 0 && Roles.IsUserInRole("College"))
            {
                ViewBag.NotUpload = true;
            }
            else
            {
                ViewBag.NotUpload = false;
            }
            if (status == 0 && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("View", "CollegeIntakeExisting");
            }
            bool isPageEditable =
                db.jntuh_college_screens_assigned.Where(
                    a => a.jntuh_college_screens.ScreenCode.Equals("EP") && a.CollegeId == userCollegeID)
                    .Select(a => a.IsEditable)
                    .FirstOrDefault();

            if (!isPageEditable)
            {
                return RedirectToAction("View", "CollegeIntakeExisting");
            }

            List<jntuh_academic_year> jntuh_academic_years = db.jntuh_academic_year.AsNoTracking().ToList();
            ViewBag.collegeId =
                Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"])
                    .ToString();
            ViewBag.AcademicYear =
                jntuh_academic_years.Where(a => a.isActive == true && a.isPresentAcademicYear == true)
                    .Select(a => a.academicYear)
                    .FirstOrDefault();
            //int PresentAcademicYear = db.jntuh_college_intake_existing.Where(a => a.isActive == true).OrderByDescending(a => a.academicYearId).Select(a => a.academicYearId).FirstOrDefault();
            //int actualYear =
            //    jntuh_academic_years.Where(a => a.isActive == true && a.isPresentAcademicYear == true)
            //        .Select(a => a.actualYear)
            //        .FirstOrDefault();

            //RAMESH: ADDED to MERGE BOTH EXISTING & PROPOSED INTAKE
            ViewBag.NextYear = String.Format("{0}-{1}", (actualYear + 1).ToString(),
                (actualYear + 2).ToString().Substring(2, 2));
            int AY0 =
                jntuh_academic_years.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();


            //CHANDU: This is actual format before 19-11-2014
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
            int enclosureId =
                db.jntuh_enclosures.Where(e => e.documentName == "AICTE INTAKE APPROVAL LETTER")
                    .Select(e => e.id)
                    .FirstOrDefault();
            var AICTEApprovalLettr =
                db.jntuh_college_enclosures.Where(
                    e => e.enclosureId == enclosureId && e.collegeID == userCollegeID && e.academicyearId == AY0)
                    .OrderByDescending(a => a.id)
                    .Select(e => e.path)
                    .FirstOrDefault();
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
                newIntake.id = item.id;
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
                newIntake.AICTEApprovalLettr = AICTEApprovalLettr;
                int collegecid = jntuh_college_noc_data.Where(d => d.noctypeId == 9)
                    .Select(s => s.id)
                    .FirstOrDefault();
                if (collegecid == 0)
                {
                    int closedid =
                        jntuh_college_noc_data.Where(
                            d => d.specializationId == item.specializationId && d.shiftId == item.shiftId && d.remarks == null)
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


            var jntuh_college_intake_existing = db.jntuh_college_intake_existing.AsNoTracking().ToList();

            collegeIntakeExisting =
                collegeIntakeExisting.AsEnumerable()
                    .GroupBy(r => new { r.specializationId, r.shiftId })
                    .Select(r => r.First())
                    .ToList();
            foreach (var item in collegeIntakeExisting)
            {

                if (item.nbaFrom != null)
                    item.nbaFromDate = Utilities.MMDDYY2DDMMYY(item.nbaFrom.ToString());
                if (item.nbaTo != null)
                    item.nbaToDate = Utilities.MMDDYY2DDMMYY(item.nbaTo.ToString());
                jntuh_college_intake_existing nbadatedetails = db.jntuh_college_intake_existing
                    .Where(
                        e =>
                            e.collegeId == userCollegeID && e.academicYearId == AY1 &&
                            e.specializationId == item.specializationId && e.shiftId == item.shiftId)
                    .Select(e => e)
                    .FirstOrDefault();
                if (nbadatedetails != null)
                {
                    if (nbadatedetails.nbaFrom != null)
                        item.nbaFromDate = Utilities.MMDDYY2DDMMYY(nbadatedetails.nbaFrom.ToString());
                    if (nbadatedetails.nbaTo != null)
                        item.nbaToDate = Utilities.MMDDYY2DDMMYY(nbadatedetails.nbaTo.ToString());
                }

                //FLAG :4-Admitted Intake Lateral as per Exam Branch,3-Admitted Intake Regular as per Exam Branch,2-AICTE Approved 1 - Approved, 0 - Admitted
                jntuh_college_intake_existing details = jntuh_college_intake_existing
                    .Where(
                        e =>
                            e.collegeId == userCollegeID && e.academicYearId == AY0 &&
                            e.specializationId == item.specializationId && e.shiftId == item.shiftId)
                    .Select(e => e)
                    .FirstOrDefault();
                if (details != null)
                {
                    item.ApprovedIntake = details.approvedIntake;
                    item.letterPath = details.approvalLetter;
                    item.ProposedIntake = details.proposedIntake;
                    item.courseStatus = details.courseStatus;
                }

                //if (item.isClosed)
                //{
                //    item.ProposedIntake = 0;
                //    item.courseStatus = "Closure";
                //}

                //item.ExambranchadmittedIntakeL1 = GetIntake(userCollegeID, AY1, item.specializationId, item.shiftId, 4);
                //item.ExambranchadmittedIntakeR1 = GetIntake(userCollegeID, AY1, item.specializationId, item.shiftId,3);
                item.AICTEapprovedIntake1 = GetIntake(userCollegeID, AY1, item.specializationId, item.shiftId, 2);
                item.approvedIntake1 = GetIntake(userCollegeID, AY1, item.specializationId, item.shiftId, 1);
                item.admittedIntake1 = GetIntake(userCollegeID, AY1, item.specializationId, item.shiftId, 0);

                //item.ExambranchadmittedIntakeL2 = GetIntake(userCollegeID, AY1, item.specializationId, item.shiftId, 4);
                //item.ExambranchadmittedIntakeR2 = GetIntake(userCollegeID, AY1, item.specializationId, item.shiftId, 3);
                item.AICTEapprovedIntake2 = GetIntake(userCollegeID, AY2, item.specializationId, item.shiftId, 2);
                item.approvedIntake2 = GetIntake(userCollegeID, AY2, item.specializationId, item.shiftId, 1);
                item.admittedIntake2 = GetIntake(userCollegeID, AY2, item.specializationId, item.shiftId, 0);

                //item.ExambranchadmittedIntakeL3 = GetIntake(userCollegeID, AY1, item.specializationId, item.shiftId, 4);
                //item.ExambranchadmittedIntakeR3 = GetIntake(userCollegeID, AY1, item.specializationId, item.shiftId, 3);
                item.AICTEapprovedIntake3 = GetIntake(userCollegeID, AY3, item.specializationId, item.shiftId, 2);
                item.approvedIntake3 = GetIntake(userCollegeID, AY3, item.specializationId, item.shiftId, 1);
                item.admittedIntake3 = GetIntake(userCollegeID, AY3, item.specializationId, item.shiftId, 0);

                //item.ExambranchadmittedIntakeL4 = GetIntake(userCollegeID, AY1, item.specializationId, item.shiftId, 4);
                //item.ExambranchadmittedIntakeR4 = GetIntake(userCollegeID, AY1, item.specializationId, item.shiftId, 3);
                item.AICTEapprovedIntake4 = GetIntake(userCollegeID, AY4, item.specializationId, item.shiftId, 2);
                item.approvedIntake4 = GetIntake(userCollegeID, AY4, item.specializationId, item.shiftId, 1);
                item.admittedIntake4 = GetIntake(userCollegeID, AY4, item.specializationId, item.shiftId, 0);

                //item.ExambranchadmittedIntakeL5 = GetIntake(userCollegeID, AY1, item.specializationId, item.shiftId, 4);
                //item.ExambranchadmittedIntakeR5 = GetIntake(userCollegeID, AY1, item.specializationId, item.shiftId, 3);
                item.AICTEapprovedIntake5 = GetIntake(userCollegeID, AY5, item.specializationId, item.shiftId, 2);
                item.approvedIntake5 = GetIntake(userCollegeID, AY5, item.specializationId, item.shiftId, 1);
                item.admittedIntake5 = GetIntake(userCollegeID, AY5, item.specializationId, item.shiftId, 0);

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
            ViewBag.IsPaymentDone =
                db.jntuh_paymentresponse.Count(
                    it =>
                        it.CollegeId == clgCode && it.AcademicYearId == AY0 && it.TxnDate.Year == currentYear &&
                        it.AuthStatus == "0300" && it.PaymentTypeID == 7) > 0;
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

        [Authorize(Roles = "Admin,College")]
        public ActionResult View(string id)
        {
            //if (!User.IsInRole("Admin"))
            //{
            //    return RedirectToAction("Index", "UnderConstruction");
            //}
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID =
                db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 0)
            {
                userCollegeID =
                    userCollegeID =
                        Convert.ToInt32(Utilities.DecryptString(id, WebConfigurationManager.AppSettings["CryptoKey"]));
            }
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            DateTime todayDate = DateTime.Now.Date;
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int prAy =
                db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
            int status = db.jntuh_college_edit_status.Where(editStatus => editStatus.academicyearId == prAy && editStatus.collegeId == userCollegeID &&
                                                                          editStatus.IsCollegeEditable == true &&
                                                                          editStatus.editFromDate <= todayDate &&
                                                                          editStatus.editToDate >= todayDate)
                                                     .Select(editStatus => editStatus.id)
                                                     .FirstOrDefault();
            if (status > 0 && Roles.IsUserInRole("College"))
            {

                bool isPageEditable =
                    db.jntuh_college_screens_assigned.Where(
                        a => a.jntuh_college_screens.ScreenCode.Equals("EP") && a.CollegeId == userCollegeID)
                        .Select(a => a.IsEditable)
                        .FirstOrDefault();

                if (isPageEditable)
                {
                    return RedirectToAction("Index");
                }
            }

            var jntuh_academic_year = db.jntuh_academic_year;
            ViewBag.AcademicYear =
                jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true)
                    .Select(a => a.academicYear)
                    .FirstOrDefault();
            //int actualYear =
            //    jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true)
            //        .Select(a => a.actualYear)
            //        .FirstOrDefault();

            //RAMESH: ADDED to MERGE BOTH EXISTING & PROPOSED INTAKE
            ViewBag.NextYear = String.Format("{0}-{1}", (actualYear + 1).ToString(),
                (actualYear + 2).ToString().Substring(2, 2));
            int AY0 =
                jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();

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
                jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true)
                    .Select(a => a.actualYear)
                    .FirstOrDefault();
            int AY1 = jntuh_academic_year.Where(a => a.actualYear == (presentYear)).Select(a => a.id).FirstOrDefault();
            int AY2 =
                jntuh_academic_year.Where(a => a.actualYear == (presentYear - 1)).Select(a => a.id).FirstOrDefault();
            int AY3 =
                jntuh_academic_year.Where(a => a.actualYear == (presentYear - 2)).Select(a => a.id).FirstOrDefault();
            int AY4 =
                jntuh_academic_year.Where(a => a.actualYear == (presentYear - 3)).Select(a => a.id).FirstOrDefault();
            int AY5 =
                jntuh_academic_year.Where(a => a.actualYear == (presentYear - 4)).Select(a => a.id).FirstOrDefault();


            int enclosureId =
                db.jntuh_enclosures.Where(e => e.documentName == "AICTE INTAKE APPROVAL LETTER")
                    .Select(e => e.id)
                    .FirstOrDefault();
            var AICTEApprovalLettr =
                db.jntuh_college_enclosures.Where(e => e.enclosureId == enclosureId && e.collegeID == userCollegeID && e.academicyearId == prAy)
                    .Select(e => e.path)
                    .FirstOrDefault();
            int[] inactivespids = db.jntuh_specialization.Where(s => s.isActive == false).Select(s => s.id).ToArray();
            //int[] academicyearids = { AY1, AY2, AY3, AY4, AY5 };
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
                newIntake.id = item.id;
                newIntake.isClosed = false;
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
                newIntake.AICTEApprovalLettr = AICTEApprovalLettr;
                int collegecid = jntuh_college_noc_data.Where(d => d.noctypeId == 9)
                    .Select(s => s.id)
                    .FirstOrDefault();
                if (collegecid == 0)
                {
                    int closedid =
                        jntuh_college_noc_data.Where(
                            d => d.specializationId == item.specializationId && d.shiftId == item.shiftId && d.remarks == null)
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

            collegeIntakeExisting = collegeIntakeExisting.AsEnumerable()
                .GroupBy(r => new { r.specializationId, r.shiftId })
                .Select(r => r.First())
                .ToList();
            var jntuh_college_intake_existing = db.jntuh_college_intake_existing;

            foreach (var item in collegeIntakeExisting)
            {
                if (item.nbaFrom != null)
                    item.nbaFromDate = Utilities.MMDDYY2DDMMYY(item.nbaFrom.ToString());
                if (item.nbaTo != null)
                    item.nbaToDate = Utilities.MMDDYY2DDMMYY(item.nbaTo.ToString());

                var details = jntuh_college_intake_existing
                    .Where(
                        e =>
                            e.collegeId == userCollegeID && e.academicYearId == AY0 &&
                            e.specializationId == item.specializationId && e.shiftId == item.shiftId)
                    .Select(e => e)
                    .FirstOrDefault();
                if (details != null)
                {
                    item.ApprovedIntake = details.approvedIntake;
                    item.letterPath = details.approvalLetter;
                    item.ProposedIntake = details.proposedIntake;
                    item.courseStatus = details.courseStatus;
                }

                //if (item.isClosed)
                //{
                //    item.ProposedIntake = 0;
                //    item.courseStatus = "Closure";
                //}

                item.AICTEapprovedIntake1 = GetIntake(userCollegeID, AY1, item.specializationId, item.shiftId, 2);
                item.approvedIntake1 = GetIntake(userCollegeID, AY1, item.specializationId, item.shiftId, 1);
                item.admittedIntake1 = GetIntake(userCollegeID, AY1, item.specializationId, item.shiftId, 0);

                item.AICTEapprovedIntake2 = GetIntake(userCollegeID, AY2, item.specializationId, item.shiftId, 2);
                item.approvedIntake2 = GetIntake(userCollegeID, AY2, item.specializationId, item.shiftId, 1);
                item.admittedIntake2 = GetIntake(userCollegeID, AY2, item.specializationId, item.shiftId, 0);

                item.AICTEapprovedIntake3 = GetIntake(userCollegeID, AY3, item.specializationId, item.shiftId, 2);
                item.approvedIntake3 = GetIntake(userCollegeID, AY3, item.specializationId, item.shiftId, 1);
                item.admittedIntake3 = GetIntake(userCollegeID, AY3, item.specializationId, item.shiftId, 0);

                item.AICTEapprovedIntake4 = GetIntake(userCollegeID, AY4, item.specializationId, item.shiftId, 2);
                item.approvedIntake4 = GetIntake(userCollegeID, AY4, item.specializationId, item.shiftId, 1);
                item.admittedIntake4 = GetIntake(userCollegeID, AY4, item.specializationId, item.shiftId, 0);

                item.AICTEapprovedIntake5 = GetIntake(userCollegeID, AY5, item.specializationId, item.shiftId, 2);
                item.approvedIntake5 = GetIntake(userCollegeID, AY5, item.specializationId, item.shiftId, 1);
                item.admittedIntake5 = GetIntake(userCollegeID, AY5, item.specializationId, item.shiftId, 0);
            }
            collegeIntakeExisting =
                collegeIntakeExisting.OrderBy(ei => ei.degreeDisplayOrder)
                    .ThenBy(ei => ei.Department)
                    .ThenBy(ei => ei.Specialization)
                    .ThenBy(ei => ei.shiftId)
                    .ToList();
            ViewBag.ExistingIntake = collegeIntakeExisting;

            ViewBag.Count = collegeIntakeExisting.Count();
            return View();
        }


        //[Authorize(Roles = "Admin,College")]
        //public ActionResult View(string id)
        //{
        //    int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
        //    int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
        //    if (userCollegeID == 0)
        //    {
        //        userCollegeID = userCollegeID = Convert.ToInt32(Utilities.DecryptString(id, WebConfigurationManager.AppSettings["CryptoKey"]));
        //    }

        //    DateTime todayDate = DateTime.Now.Date;
        //    int status = db.jntuh_college_edit_status.Where(editStatus => editStatus.collegeId == userCollegeID &&
        //                                                                  editStatus.IsCollegeEditable == true &&
        //                                                                  editStatus.editFromDate <= todayDate &&
        //                                                                  editStatus.editToDate >= todayDate)
        //                                             .Select(editStatus => editStatus.id)
        //                                             .FirstOrDefault();
        //    if (status > 0 && Roles.IsUserInRole("College"))
        //    {

        //        bool isPageEditable = db.jntuh_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("EP") && a.CollegeId == userCollegeID).Select(a => a.IsEditable).FirstOrDefault();

        //        if (isPageEditable)
        //        {
        //            return RedirectToAction("Index");
        //        }
        //    }

        //    var jntuh_academic_year = db.jntuh_academic_year;
        //    ViewBag.AcademicYear = jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.academicYear).FirstOrDefault();
        //    int actualYear = jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();

        //    //RAMESH: ADDED to MERGE BOTH EXISTING & PROPOSED INTAKE
        //    ViewBag.NextYear = String.Format("{0}-{1}", (actualYear + 1).ToString(), (actualYear + 2).ToString().Substring(2, 2));
        //    int AY0 = jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();

        //    ViewBag.FirstYear = String.Format("{0}-{1}", (actualYear).ToString(), (actualYear + 1).ToString().Substring(2, 2));
        //    ViewBag.SecondYear = String.Format("{0}-{1}", (actualYear - 1).ToString(), (actualYear).ToString().Substring(2, 2));
        //    ViewBag.ThirdYear = String.Format("{0}-{1}", (actualYear - 2).ToString(), (actualYear - 1).ToString().Substring(2, 2));
        //    ViewBag.FourthYear = String.Format("{0}-{1}", (actualYear - 3).ToString(), (actualYear - 2).ToString().Substring(2, 2));
        //    ViewBag.FifthYear = String.Format("{0}-{1}", (actualYear - 4).ToString(), (actualYear - 3).ToString().Substring(2, 2));

        //    int presentYear = jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
        //    int AY1 = jntuh_academic_year.Where(a => a.actualYear == (presentYear)).Select(a => a.id).FirstOrDefault();
        //    int AY2 = jntuh_academic_year.Where(a => a.actualYear == (presentYear - 1)).Select(a => a.id).FirstOrDefault();
        //    int AY3 = jntuh_academic_year.Where(a => a.actualYear == (presentYear - 2)).Select(a => a.id).FirstOrDefault();
        //    int AY4 = jntuh_academic_year.Where(a => a.actualYear == (presentYear - 3)).Select(a => a.id).FirstOrDefault();
        //    int AY5 = jntuh_academic_year.Where(a => a.actualYear == (presentYear - 4)).Select(a => a.id).FirstOrDefault();

        //    ////RAMESH : The following code is not required becoz JNTU is not going to use the code defined for COURT

        //    //ViewBag.FirstYear = String.Format("{0}-{1}", (actualYear + 1).ToString(), (actualYear + 2).ToString().Substring(2, 2));
        //    //ViewBag.SecondYear = String.Format("{0}-{1}", (actualYear).ToString(), (actualYear + 1).ToString().Substring(2, 2));
        //    //ViewBag.ThirdYear = String.Format("{0}-{1}", (actualYear - 1).ToString(), (actualYear).ToString().Substring(2, 2));
        //    //ViewBag.FourthYear = String.Format("{0}-{1}", (actualYear - 2).ToString(), (actualYear - 1).ToString().Substring(2, 2));
        //    //ViewBag.FifthYear = String.Format("{0}-{1}", (actualYear - 3).ToString(), (actualYear - 2).ToString().Substring(2, 2));

        //    //int presentYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();

        //    //int AY1 = db.jntuh_academic_year.Where(a => a.actualYear == (presentYear + 1)).Select(a => a.id).FirstOrDefault();
        //    //int AY2 = db.jntuh_academic_year.Where(a => a.actualYear == (presentYear)).Select(a => a.id).FirstOrDefault();
        //    //int AY3 = db.jntuh_academic_year.Where(a => a.actualYear == (presentYear - 1)).Select(a => a.id).FirstOrDefault();
        //    //int AY4 = db.jntuh_academic_year.Where(a => a.actualYear == (presentYear - 2)).Select(a => a.id).FirstOrDefault();
        //    //int AY5 = db.jntuh_academic_year.Where(a => a.actualYear == (presentYear - 3)).Select(a => a.id).FirstOrDefault();
        //    int enclosureId = db.jntuh_enclosures.Where(e => e.documentName == "AICTE INTAKE APPROVAL LETTER").Select(e => e.id).FirstOrDefault();
        //    var AICTEApprovalLettr = db.jntuh_college_enclosures.Where(e => e.enclosureId == enclosureId && e.collegeID == userCollegeID).Select(e => e.path).FirstOrDefault();
        //    List<jntuh_college_intake_existing> intake = db.jntuh_college_intake_existing.Where(i => i.collegeId == userCollegeID).ToList();

        //    List<CollegeIntakeExisting> collegeIntakeExisting = new List<CollegeIntakeExisting>();
        //    var jntuh_specialization = db.jntuh_specialization;
        //    var jntuh_department = db.jntuh_department;
        //    var jntuh_degree = db.jntuh_degree;
        //    var jntuh_shift = db.jntuh_shift;

        //    foreach (var item in intake)
        //    {
        //        CollegeIntakeExisting newIntake = new CollegeIntakeExisting();
        //        newIntake.id = item.id;
        //        newIntake.collegeId = item.collegeId;
        //        newIntake.academicYearId = item.academicYearId;
        //        newIntake.shiftId = item.shiftId;
        //        newIntake.isActive = item.isActive;
        //        newIntake.nbaFrom = item.nbaFrom;
        //        newIntake.nbaTo = item.nbaTo;
        //        newIntake.specializationId = item.specializationId;
        //        newIntake.Specialization = jntuh_specialization.Where(s => s.id == item.specializationId).Select(s => s.specializationName).FirstOrDefault();
        //        newIntake.DepartmentID = jntuh_specialization.Where(s => s.id == item.specializationId).Select(s => s.departmentId).FirstOrDefault();
        //        newIntake.Department = jntuh_department.Where(d => d.id == newIntake.DepartmentID).Select(d => d.departmentName).FirstOrDefault();
        //        newIntake.degreeID = jntuh_department.Where(d => d.id == newIntake.DepartmentID).Select(d => d.degreeId).FirstOrDefault();
        //        newIntake.Degree = jntuh_degree.Where(d => d.id == newIntake.degreeID).Select(d => d.degree).FirstOrDefault();
        //        newIntake.degreeDisplayOrder = jntuh_degree.Where(d => d.id == newIntake.degreeID).Select(d => d.degreeDisplayOrder).FirstOrDefault();
        //        newIntake.shiftId = item.shiftId;
        //        newIntake.Shift = jntuh_shift.Where(s => s.id == item.shiftId).Select(s => s.shiftName).FirstOrDefault();
        //        newIntake.AICTEApprovalLettr = AICTEApprovalLettr;
        //        collegeIntakeExisting.Add(newIntake);
        //    }

        //    collegeIntakeExisting = collegeIntakeExisting.AsEnumerable()
        //                                                 .GroupBy(r => new { r.specializationId, r.shiftId })
        //                                                 .Select(r => r.First())
        //                                                 .ToList();
        //    var jntuh_college_intake_existing = db.jntuh_college_intake_existing;

        //    foreach (var item in collegeIntakeExisting)
        //    {
        //        if (item.nbaFrom != null)
        //            item.nbaFromDate = Utilities.MMDDYY2DDMMYY(item.nbaFrom.ToString());
        //        if (item.nbaTo != null)
        //            item.nbaToDate = Utilities.MMDDYY2DDMMYY(item.nbaTo.ToString());

        //        var details = jntuh_college_intake_existing
        //                                               .Where(e => e.collegeId == userCollegeID && e.academicYearId == AY0 && e.specializationId == item.specializationId && e.shiftId == item.shiftId)
        //                                               .Select(e => e)
        //                                               .FirstOrDefault();
        //        if (details != null)
        //        {
        //            item.ApprovedIntake = details.approvedIntake;
        //            item.letterPath = details.approvalLetter;
        //            item.ProposedIntake = details.proposedIntake;
        //        }

        //        item.approvedIntake1 = GetIntake(userCollegeID, AY1, item.specializationId, item.shiftId, 1);
        //        item.admittedIntake1 = GetIntake(userCollegeID, AY1, item.specializationId, item.shiftId, 0);

        //        item.approvedIntake2 = GetIntake(userCollegeID, AY2, item.specializationId, item.shiftId, 1);
        //        item.admittedIntake2 = GetIntake(userCollegeID, AY2, item.specializationId, item.shiftId, 0);

        //        item.approvedIntake3 = GetIntake(userCollegeID, AY3, item.specializationId, item.shiftId, 1);
        //        item.admittedIntake3 = GetIntake(userCollegeID, AY3, item.specializationId, item.shiftId, 0);

        //        item.approvedIntake4 = GetIntake(userCollegeID, AY4, item.specializationId, item.shiftId, 1);
        //        item.admittedIntake4 = GetIntake(userCollegeID, AY4, item.specializationId, item.shiftId, 0);

        //        item.approvedIntake5 = GetIntake(userCollegeID, AY5, item.specializationId, item.shiftId, 1);
        //        item.admittedIntake5 = GetIntake(userCollegeID, AY5, item.specializationId, item.shiftId, 0);
        //    }
        //    collegeIntakeExisting = collegeIntakeExisting.OrderBy(ei => ei.degreeDisplayOrder).ThenBy(ei => ei.Department).ThenBy(ei => ei.Specialization).ThenBy(ei => ei.shiftId).ToList();
        //    ViewBag.ExistingIntake = collegeIntakeExisting;

        //    ViewBag.Count = collegeIntakeExisting.Count();
        //    return View();
        //}

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        [HttpGet]
        public ActionResult AddEditRecord(int? id, string collegeId)
        {
            CollegeIntakeExisting collegeIntakeExisting = new CollegeIntakeExisting();
            if (Membership.GetUser() == null)
            {
                return RedirectToAction("LogOn", "Account");
            }
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
                            Convert.ToInt32(Utilities.DecryptString(collegeId,
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
                db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true)
                    .Select(a => a.academicYear)
                    .FirstOrDefault();
            int actualYear =
                db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true)
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

            ////RAMESH : The following code is not required becoz JNTU is not going to use the code defined for COURT

            //ViewBag.FirstYear = String.Format("{0}-{1}", (actualYear + 1).ToString(), (actualYear + 2).ToString().Substring(2, 2));
            //ViewBag.SecondYear = String.Format("{0}-{1}", (actualYear).ToString(), (actualYear + 1).ToString().Substring(2, 2));
            //ViewBag.ThirdYear = String.Format("{0}-{1}", (actualYear - 1).ToString(), (actualYear).ToString().Substring(2, 2));
            //ViewBag.FourthYear = String.Format("{0}-{1}", (actualYear - 2).ToString(), (actualYear - 1).ToString().Substring(2, 2));
            //ViewBag.FifthYear = String.Format("{0}-{1}", (actualYear - 3).ToString(), (actualYear - 2).ToString().Substring(2, 2));         

            if (id != null)
            {
                int presentYear =
                    db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true)
                        .Select(a => a.actualYear)
                        .FirstOrDefault();
                int AY1 =
                    db.jntuh_academic_year.Where(a => a.actualYear == (presentYear)).Select(a => a.id).FirstOrDefault();
                int AY2 =
                    db.jntuh_academic_year.Where(a => a.actualYear == (presentYear - 1))
                        .Select(a => a.id)
                        .FirstOrDefault();
                int AY3 =
                    db.jntuh_academic_year.Where(a => a.actualYear == (presentYear - 2))
                        .Select(a => a.id)
                        .FirstOrDefault();
                int AY4 =
                    db.jntuh_academic_year.Where(a => a.actualYear == (presentYear - 3))
                        .Select(a => a.id)
                        .FirstOrDefault();
                int AY5 =
                    db.jntuh_academic_year.Where(a => a.actualYear == (presentYear - 4))
                        .Select(a => a.id)
                        .FirstOrDefault();

                ////RAMESH : The following code is not required becoz JNTU is not going to use the code defined for COURT

                //int presentYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();

                //int AY1 = db.jntuh_academic_year.Where(a => a.actualYear == (presentYear + 1)).Select(a => a.id).FirstOrDefault();
                //int AY2 = db.jntuh_academic_year.Where(a => a.actualYear == (presentYear)).Select(a => a.id).FirstOrDefault();
                //int AY3 = db.jntuh_academic_year.Where(a => a.actualYear == (presentYear - 1)).Select(a => a.id).FirstOrDefault();
                //int AY4 = db.jntuh_academic_year.Where(a => a.actualYear == (presentYear - 2)).Select(a => a.id).FirstOrDefault();
                //int AY5 = db.jntuh_academic_year.Where(a => a.actualYear == (presentYear - 3)).Select(a => a.id).FirstOrDefault();

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
                        db.jntuh_college_intake_existing.Where(
                            i =>
                                i.collegeId == userCollegeID && i.specializationId == item.specializationId &&
                                i.academicYearId == AY0).Select(s => s.courseStatus).FirstOrDefault() == null
                            ? "0"
                            : db.jntuh_college_intake_existing.Where(
                                i =>
                                    i.collegeId == userCollegeID && i.specializationId == item.specializationId &&
                                    i.academicYearId == AY0).Select(s => s.courseStatus).FirstOrDefault();
                    collegeIntakeExisting.UploadNBAApproveLetter = item.NBAApproveLetter;
                    ViewBag.NBAApprovedLetter = item.NBAApproveLetter;
                    collegeIntakeExisting.courseAffiliatedStatus = item.courseAffiliatedStatus;
                    collegeIntakeExisting.Shift =
                        db.jntuh_shift.Where(s => s.id == item.shiftId).Select(s => s.shiftName).FirstOrDefault();
                }

                if (collegeIntakeExisting.nbaFrom != null)
                    collegeIntakeExisting.nbaFromDate = Utilities.MMDDYY2DDMMYY(collegeIntakeExisting.nbaFrom.ToString());
                if (collegeIntakeExisting.nbaTo != null)
                    collegeIntakeExisting.nbaToDate = Utilities.MMDDYY2DDMMYY(collegeIntakeExisting.nbaTo.ToString());

                jntuh_college_intake_existing nbadatedetails = db.jntuh_college_intake_existing
                    .Where(
                        e =>
                            e.collegeId == userCollegeID && e.academicYearId == AY1 &&
                            e.specializationId == collegeIntakeExisting.specializationId &&
                            e.shiftId == collegeIntakeExisting.shiftId)
                    .Select(e => e)
                    .FirstOrDefault();
                if (nbadatedetails != null)
                {
                    if (nbadatedetails.nbaFrom != null)
                        collegeIntakeExisting.nbaFromDate = Utilities.MMDDYY2DDMMYY(nbadatedetails.nbaFrom.ToString());
                    if (nbadatedetails.nbaTo != null)
                        collegeIntakeExisting.nbaToDate = Utilities.MMDDYY2DDMMYY(nbadatedetails.nbaTo.ToString());
                }

                //FLAG :2-AICTE Approved, 1 - Approved, 0 - Admitted
                jntuh_college_intake_existing details = db.jntuh_college_intake_existing
                    .Where(
                        e =>
                            e.collegeId == userCollegeID && e.academicYearId == AY0 &&
                            e.specializationId == collegeIntakeExisting.specializationId &&
                            e.shiftId == collegeIntakeExisting.shiftId)
                    .Select(e => e)
                    .FirstOrDefault();
                if (details != null)
                {
                    collegeIntakeExisting.ApprovedIntake = details.approvedIntake;
                    collegeIntakeExisting.letterPath = details.approvalLetter;
                    collegeIntakeExisting.ProposedIntake = details.proposedIntake;
                    collegeIntakeExisting.UploadNBAApproveLetter = details.NBAApproveLetter;
                    ViewBag.NBAApprovedLetter = collegeIntakeExisting.UploadNBAApproveLetter;

                }

                collegeIntakeExisting.ExambranchadmittedIntakeL1 = GetIntake(userCollegeID, AY1,
                    collegeIntakeExisting.specializationId, collegeIntakeExisting.shiftId, 4);
                collegeIntakeExisting.ExambranchadmittedIntakeR1 = GetIntake(userCollegeID, AY1,
                    collegeIntakeExisting.specializationId, collegeIntakeExisting.shiftId, 3);
                collegeIntakeExisting.ExambranchadmittedIntake1 = collegeIntakeExisting.ExambranchadmittedIntakeR1 + "+" +
                                                                  collegeIntakeExisting.ExambranchadmittedIntakeL1;
                collegeIntakeExisting.AICTEapprovedIntake1 = GetIntake(userCollegeID, AY1,
                    collegeIntakeExisting.specializationId, collegeIntakeExisting.shiftId, 2);
                collegeIntakeExisting.approvedIntake1 = GetIntake(userCollegeID, AY1,
                    collegeIntakeExisting.specializationId, collegeIntakeExisting.shiftId, 1);
                collegeIntakeExisting.admittedIntake1 = GetIntake(userCollegeID, AY1,
                    collegeIntakeExisting.specializationId, collegeIntakeExisting.shiftId, 0);

                collegeIntakeExisting.ExambranchadmittedIntakeL2 = GetIntake(userCollegeID, AY2,
                    collegeIntakeExisting.specializationId, collegeIntakeExisting.shiftId, 4);
                collegeIntakeExisting.ExambranchadmittedIntakeR2 = GetIntake(userCollegeID, AY2,
                    collegeIntakeExisting.specializationId, collegeIntakeExisting.shiftId, 3);
                collegeIntakeExisting.ExambranchadmittedIntake2 = collegeIntakeExisting.ExambranchadmittedIntakeR2 + "+" +
                                                                  collegeIntakeExisting.ExambranchadmittedIntakeL2;
                collegeIntakeExisting.AICTEapprovedIntake2 = GetIntake(userCollegeID, AY2,
                    collegeIntakeExisting.specializationId, collegeIntakeExisting.shiftId, 2);
                collegeIntakeExisting.approvedIntake2 = GetIntake(userCollegeID, AY2,
                    collegeIntakeExisting.specializationId, collegeIntakeExisting.shiftId, 1);
                collegeIntakeExisting.admittedIntake2 = GetIntake(userCollegeID, AY2,
                    collegeIntakeExisting.specializationId, collegeIntakeExisting.shiftId, 0);

                collegeIntakeExisting.ExambranchadmittedIntakeL3 = GetIntake(userCollegeID, AY3,
                    collegeIntakeExisting.specializationId, collegeIntakeExisting.shiftId, 4);
                collegeIntakeExisting.ExambranchadmittedIntakeR3 = GetIntake(userCollegeID, AY3,
                    collegeIntakeExisting.specializationId, collegeIntakeExisting.shiftId, 3);
                collegeIntakeExisting.ExambranchadmittedIntake3 = collegeIntakeExisting.ExambranchadmittedIntakeR3 + "+" +
                                                                  collegeIntakeExisting.ExambranchadmittedIntakeL3;
                collegeIntakeExisting.AICTEapprovedIntake3 = GetIntake(userCollegeID, AY3,
                    collegeIntakeExisting.specializationId, collegeIntakeExisting.shiftId, 2);
                collegeIntakeExisting.approvedIntake3 = GetIntake(userCollegeID, AY3,
                    collegeIntakeExisting.specializationId, collegeIntakeExisting.shiftId, 1);
                collegeIntakeExisting.admittedIntake3 = GetIntake(userCollegeID, AY3,
                    collegeIntakeExisting.specializationId, collegeIntakeExisting.shiftId, 0);

                collegeIntakeExisting.ExambranchadmittedIntakeL4 = GetIntake(userCollegeID, AY4,
                    collegeIntakeExisting.specializationId, collegeIntakeExisting.shiftId, 4);
                collegeIntakeExisting.ExambranchadmittedIntakeR4 = GetIntake(userCollegeID, AY4,
                    collegeIntakeExisting.specializationId, collegeIntakeExisting.shiftId, 3);
                collegeIntakeExisting.ExambranchadmittedIntake4 = collegeIntakeExisting.ExambranchadmittedIntakeR4 + "+" +
                                                                  collegeIntakeExisting.ExambranchadmittedIntakeL4;
                collegeIntakeExisting.AICTEapprovedIntake4 = GetIntake(userCollegeID, AY4,
                    collegeIntakeExisting.specializationId, collegeIntakeExisting.shiftId, 2);
                collegeIntakeExisting.approvedIntake4 = GetIntake(userCollegeID, AY4,
                    collegeIntakeExisting.specializationId, collegeIntakeExisting.shiftId, 1);
                collegeIntakeExisting.admittedIntake4 = GetIntake(userCollegeID, AY4,
                    collegeIntakeExisting.specializationId, collegeIntakeExisting.shiftId, 0);

                collegeIntakeExisting.ExambranchadmittedIntakeL5 = GetIntake(userCollegeID, AY5,
                    collegeIntakeExisting.specializationId, collegeIntakeExisting.shiftId, 4);
                collegeIntakeExisting.ExambranchadmittedIntakeR5 = GetIntake(userCollegeID, AY5,
                    collegeIntakeExisting.specializationId, collegeIntakeExisting.shiftId, 3);
                collegeIntakeExisting.ExambranchadmittedIntake5 = collegeIntakeExisting.ExambranchadmittedIntakeR5 + "+" +
                                                                  collegeIntakeExisting.ExambranchadmittedIntakeL5;
                collegeIntakeExisting.AICTEapprovedIntake5 = GetIntake(userCollegeID, AY5,
                    collegeIntakeExisting.specializationId, collegeIntakeExisting.shiftId, 2);
                collegeIntakeExisting.approvedIntake5 = GetIntake(userCollegeID, AY5,
                    collegeIntakeExisting.specializationId, collegeIntakeExisting.shiftId, 1);
                collegeIntakeExisting.admittedIntake5 = GetIntake(userCollegeID, AY5,
                    collegeIntakeExisting.specializationId, collegeIntakeExisting.shiftId, 0);

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
            //courseStatuslist.Add(new SelectListItem { Value = "0", Text = "-Select-" });
            courseStatuslist.Add(new SelectListItem { Value = "New", Text = "New" });
            courseStatuslist.Add(new SelectListItem { Value = "Increase", Text = "Increase" });
            courseStatuslist.Add(new SelectListItem { Value = "Decrease", Text = "Decrease" });
            courseStatuslist.Add(new SelectListItem { Value = "Nochange", Text = "No Change" });
            courseStatuslist.Add(new SelectListItem { Value = "Closure", Text = "Closure" });
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
        public ActionResult Details(int? id)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID =
                db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 0)
            {
                userCollegeID =
                    db.jntuh_college_intake_existing.Where(i => i.id == id).Select(i => i.collegeId).FirstOrDefault();
            }
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            CollegeIntakeExisting collegeIntakeExisting = new CollegeIntakeExisting();
            if (Roles.IsUserInRole("Admin") == true)
            {
                userCollegeID =
                    db.jntuh_college_intake_existing.Where(e => e.id == id).Select(e => e.collegeId).FirstOrDefault();
            }
            if (id != null)
            {

                ViewBag.IsUpdate = true;

                ViewBag.AcademicYear =
                    db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true)
                        .Select(a => a.academicYear)
                        .FirstOrDefault();
                int actualYear =
                    db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true)
                        .Select(a => a.actualYear)
                        .FirstOrDefault();

                //RAMESH: ADDED to MERGE BOTH EXISTING & PROPOSED INTAKE
                ViewBag.NextYear = String.Format("{0}-{1}", (actualYear + 1).ToString(),
                    (actualYear + 2).ToString().Substring(2, 2));
                int AY0 =
                    db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1))
                        .Select(a => a.id)
                        .FirstOrDefault();

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
                    db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true)
                        .Select(a => a.actualYear)
                        .FirstOrDefault();
                int AY1 =
                    db.jntuh_academic_year.Where(a => a.actualYear == (presentYear)).Select(a => a.id).FirstOrDefault();
                int AY2 =
                    db.jntuh_academic_year.Where(a => a.actualYear == (presentYear - 1))
                        .Select(a => a.id)
                        .FirstOrDefault();
                int AY3 =
                    db.jntuh_academic_year.Where(a => a.actualYear == (presentYear - 2))
                        .Select(a => a.id)
                        .FirstOrDefault();
                int AY4 =
                    db.jntuh_academic_year.Where(a => a.actualYear == (presentYear - 3))
                        .Select(a => a.id)
                        .FirstOrDefault();
                int AY5 =
                    db.jntuh_academic_year.Where(a => a.actualYear == (presentYear - 4))
                        .Select(a => a.id)
                        .FirstOrDefault();

                ////RAMESH : The following code is not required becoz JNTU is not going to use the code defined for COURT

                //ViewBag.FirstYear = String.Format("{0}-{1}", (actualYear + 1).ToString(), (actualYear + 2).ToString().Substring(2, 2));
                //ViewBag.SecondYear = String.Format("{0}-{1}", (actualYear).ToString(), (actualYear + 1).ToString().Substring(2, 2));
                //ViewBag.ThirdYear = String.Format("{0}-{1}", (actualYear - 1).ToString(), (actualYear).ToString().Substring(2, 2));
                //ViewBag.FourthYear = String.Format("{0}-{1}", (actualYear - 2).ToString(), (actualYear - 1).ToString().Substring(2, 2));
                //ViewBag.FifthYear = String.Format("{0}-{1}", (actualYear - 3).ToString(), (actualYear - 2).ToString().Substring(2, 2));

                //int presentYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();

                //int AY1 = db.jntuh_academic_year.Where(a => a.actualYear == (presentYear + 1)).Select(a => a.id).FirstOrDefault();
                //int AY2 = db.jntuh_academic_year.Where(a => a.actualYear == (presentYear)).Select(a => a.id).FirstOrDefault();
                //int AY3 = db.jntuh_academic_year.Where(a => a.actualYear == (presentYear - 1)).Select(a => a.id).FirstOrDefault();
                //int AY4 = db.jntuh_academic_year.Where(a => a.actualYear == (presentYear - 2)).Select(a => a.id).FirstOrDefault();
                //int AY5 = db.jntuh_academic_year.Where(a => a.actualYear == (presentYear - 3)).Select(a => a.id).FirstOrDefault();

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
                    collegeIntakeExisting.Shift =
                        db.jntuh_shift.Where(s => s.id == item.shiftId).Select(s => s.shiftName).FirstOrDefault();
                }

                if (collegeIntakeExisting.nbaFrom != null)
                    collegeIntakeExisting.nbaFromDate = Utilities.MMDDYY2DDMMYY(collegeIntakeExisting.nbaFrom.ToString());
                if (collegeIntakeExisting.nbaTo != null)
                    collegeIntakeExisting.nbaToDate = Utilities.MMDDYY2DDMMYY(collegeIntakeExisting.nbaTo.ToString());

                jntuh_college_intake_existing nbadatedetails = db.jntuh_college_intake_existing
                    .Where(
                        e =>
                            e.collegeId == userCollegeID && e.academicYearId == AY1 &&
                            e.specializationId == collegeIntakeExisting.specializationId &&
                            e.shiftId == collegeIntakeExisting.shiftId)
                    .Select(e => e)
                    .FirstOrDefault();
                if (nbadatedetails != null)
                {
                    if (nbadatedetails.nbaFrom != null)
                        collegeIntakeExisting.nbaFromDate = Utilities.MMDDYY2DDMMYY(nbadatedetails.nbaFrom.ToString());
                    if (nbadatedetails.nbaTo != null)
                        collegeIntakeExisting.nbaToDate = Utilities.MMDDYY2DDMMYY(nbadatedetails.nbaTo.ToString());
                }

                jntuh_college_intake_existing details = db.jntuh_college_intake_existing
                    .Where(
                        e =>
                            e.collegeId == userCollegeID && e.academicYearId == AY0 &&
                            e.specializationId == collegeIntakeExisting.specializationId &&
                            e.shiftId == collegeIntakeExisting.shiftId)
                    .Select(e => e)
                    .FirstOrDefault();
                if (details != null)
                {
                    collegeIntakeExisting.ApprovedIntake = details.approvedIntake;
                    collegeIntakeExisting.letterPath = details.approvalLetter;
                    collegeIntakeExisting.ProposedIntake = details.proposedIntake;
                }

                // collegeIntakeExisting.ProposedIntake = GetIntake(userCollegeID, AY0, collegeIntakeExisting.specializationId, collegeIntakeExisting.shiftId, 1);


                collegeIntakeExisting.ExambranchadmittedIntakeL1 = GetIntake(userCollegeID, AY1,
                    collegeIntakeExisting.specializationId, collegeIntakeExisting.shiftId, 4);
                collegeIntakeExisting.ExambranchadmittedIntakeR1 = GetIntake(userCollegeID, AY1,
                    collegeIntakeExisting.specializationId, collegeIntakeExisting.shiftId, 3);
                collegeIntakeExisting.ExambranchadmittedIntake1 = collegeIntakeExisting.ExambranchadmittedIntakeR1 + "+" +
                                                                  collegeIntakeExisting.ExambranchadmittedIntakeL1;
                collegeIntakeExisting.AICTEapprovedIntake1 = GetIntake(userCollegeID, AY1,
                    collegeIntakeExisting.specializationId, collegeIntakeExisting.shiftId, 2);
                collegeIntakeExisting.approvedIntake1 = GetIntake(userCollegeID, AY1,
                    collegeIntakeExisting.specializationId, collegeIntakeExisting.shiftId, 1);
                collegeIntakeExisting.admittedIntake1 = GetIntake(userCollegeID, AY1,
                    collegeIntakeExisting.specializationId, collegeIntakeExisting.shiftId, 0);

                collegeIntakeExisting.ExambranchadmittedIntakeL2 = GetIntake(userCollegeID, AY2,
                    collegeIntakeExisting.specializationId, collegeIntakeExisting.shiftId, 4);
                collegeIntakeExisting.ExambranchadmittedIntakeR2 = GetIntake(userCollegeID, AY2,
                    collegeIntakeExisting.specializationId, collegeIntakeExisting.shiftId, 3);
                collegeIntakeExisting.ExambranchadmittedIntake2 = collegeIntakeExisting.ExambranchadmittedIntakeR2 + "+" +
                                                                  collegeIntakeExisting.ExambranchadmittedIntakeL2;
                collegeIntakeExisting.AICTEapprovedIntake2 = GetIntake(userCollegeID, AY2,
                    collegeIntakeExisting.specializationId, collegeIntakeExisting.shiftId, 2);
                collegeIntakeExisting.approvedIntake2 = GetIntake(userCollegeID, AY2,
                    collegeIntakeExisting.specializationId, collegeIntakeExisting.shiftId, 1);
                collegeIntakeExisting.admittedIntake2 = GetIntake(userCollegeID, AY2,
                    collegeIntakeExisting.specializationId, collegeIntakeExisting.shiftId, 0);

                collegeIntakeExisting.ExambranchadmittedIntakeL3 = GetIntake(userCollegeID, AY3,
                    collegeIntakeExisting.specializationId, collegeIntakeExisting.shiftId, 4);
                collegeIntakeExisting.ExambranchadmittedIntakeR3 = GetIntake(userCollegeID, AY3,
                    collegeIntakeExisting.specializationId, collegeIntakeExisting.shiftId, 3);
                collegeIntakeExisting.ExambranchadmittedIntake3 = collegeIntakeExisting.ExambranchadmittedIntakeR3 + "+" +
                                                                  collegeIntakeExisting.ExambranchadmittedIntakeL3;
                collegeIntakeExisting.AICTEapprovedIntake3 = GetIntake(userCollegeID, AY3,
                    collegeIntakeExisting.specializationId, collegeIntakeExisting.shiftId, 2);
                collegeIntakeExisting.approvedIntake3 = GetIntake(userCollegeID, AY3,
                    collegeIntakeExisting.specializationId, collegeIntakeExisting.shiftId, 1);
                collegeIntakeExisting.admittedIntake3 = GetIntake(userCollegeID, AY3,
                    collegeIntakeExisting.specializationId, collegeIntakeExisting.shiftId, 0);

                collegeIntakeExisting.ExambranchadmittedIntakeL4 = GetIntake(userCollegeID, AY4,
                    collegeIntakeExisting.specializationId, collegeIntakeExisting.shiftId, 4);
                collegeIntakeExisting.ExambranchadmittedIntakeR4 = GetIntake(userCollegeID, AY4,
                    collegeIntakeExisting.specializationId, collegeIntakeExisting.shiftId, 3);
                collegeIntakeExisting.ExambranchadmittedIntake4 = collegeIntakeExisting.ExambranchadmittedIntakeR4 + "+" +
                                                                  collegeIntakeExisting.ExambranchadmittedIntakeL4;
                collegeIntakeExisting.AICTEapprovedIntake4 = GetIntake(userCollegeID, AY4,
                    collegeIntakeExisting.specializationId, collegeIntakeExisting.shiftId, 2);
                collegeIntakeExisting.approvedIntake4 = GetIntake(userCollegeID, AY4,
                    collegeIntakeExisting.specializationId, collegeIntakeExisting.shiftId, 1);
                collegeIntakeExisting.admittedIntake4 = GetIntake(userCollegeID, AY4,
                    collegeIntakeExisting.specializationId, collegeIntakeExisting.shiftId, 0);

                collegeIntakeExisting.ExambranchadmittedIntakeL5 = GetIntake(userCollegeID, AY5,
                    collegeIntakeExisting.specializationId, collegeIntakeExisting.shiftId, 4);
                collegeIntakeExisting.ExambranchadmittedIntakeR5 = GetIntake(userCollegeID, AY5,
                    collegeIntakeExisting.specializationId, collegeIntakeExisting.shiftId, 3);
                collegeIntakeExisting.ExambranchadmittedIntake5 = collegeIntakeExisting.ExambranchadmittedIntakeR5 + "+" +
                                                                  collegeIntakeExisting.ExambranchadmittedIntakeL5;
                collegeIntakeExisting.AICTEapprovedIntake5 = GetIntake(userCollegeID, AY5,
                    collegeIntakeExisting.specializationId, collegeIntakeExisting.shiftId, 2);
                collegeIntakeExisting.approvedIntake5 = GetIntake(userCollegeID, AY5,
                    collegeIntakeExisting.specializationId, collegeIntakeExisting.shiftId, 1);
                collegeIntakeExisting.admittedIntake5 = GetIntake(userCollegeID, AY5,
                    collegeIntakeExisting.specializationId, collegeIntakeExisting.shiftId, 0);

            }
            else
            {
                ViewBag.IsUpdate = false;
            }

            ViewBag.Degree =
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
            ViewBag.Department = db.jntuh_department.Where(d => d.isActive == true);
            ViewBag.Specialization = db.jntuh_specialization.Where(s => s.isActive == true);
            ViewBag.Shift = db.jntuh_shift.Where(s => s.isActive == true);

            if (Request.IsAjaxRequest())
            {
                return PartialView("Details", collegeIntakeExisting);
            }
            else
            {
                return View("Details", collegeIntakeExisting);
            }
        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult DeleteRecord(int id)
        {
            return RedirectToAction("College", "Dashboard");
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID =
                db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            if (userCollegeID == 0)
            {
                userCollegeID =
                    db.jntuh_college_intake_existing.Where(i => i.id == id).Select(i => i.collegeId).FirstOrDefault();
            }
            int specid =
                db.jntuh_college_intake_existing.Where(p => p.id == id).Select(p => p.specializationId).FirstOrDefault();
            int shiftid =
                db.jntuh_college_intake_existing.Where(p => p.id == id).Select(p => p.shiftId).FirstOrDefault();
            List<jntuh_college_intake_existing> jntuh_college_intake_existing =
                db.jntuh_college_intake_existing.Where(
                    p => p.specializationId == specid && p.shiftId == shiftid && p.collegeId == userCollegeID).ToList();
            foreach (var item in jntuh_college_intake_existing)
            {
                db.jntuh_college_intake_existing.Remove(item);
                db.SaveChanges();
                TempData["Success"] = "College Exissting Intake Deleted successfully";
            }

            return RedirectToAction("Index",
                new
                {
                    collegeId =
                        Utilities.EncryptString(userCollegeID.ToString(),
                            WebConfigurationManager.AppSettings["CryptoKey"]).ToString()
                });
        }


        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        [HttpPost]
        [AllowAnonymous]
        public ActionResult AddEditRecord(CollegeIntakeExisting collegeIntakeExisting, string cmd)
        {
            ModelState.Clear();
            if (Membership.GetUser() == null)
            {
                return RedirectToAction("LogOn", "Account");
            }
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);

            int userCollegeID =
                db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();

            if (userCollegeID == 0)
            {
                userCollegeID = collegeIntakeExisting.collegeId;
            }
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            //if (collegeIntakeExisting.degreeID != 1)
            //{
            //    if (collegeIntakeExisting.courseStatus != "New")
            //    {
            //        if ((collegeIntakeExisting.AICTEapprovedIntake1 == 0 || collegeIntakeExisting.AICTEapprovedIntake2 == 0 ||
            //         collegeIntakeExisting.AICTEapprovedIntake3 == 0) && collegeIntakeExisting.courseStatus != "Closure")
            //        {
            //            TempData["Error"] = "AICTE Sanctioned Intake Should not be Zero";
            //            return RedirectToAction("Index",
            //                new
            //                {
            //                    collegeId =
            //                        Utilities.EncryptString(userCollegeID.ToString(),
            //                            WebConfigurationManager.AppSettings["CryptoKey"]).ToString()
            //                });

            //        }
            //    }
            //}
            //else
            //{
            //    if (collegeIntakeExisting.courseStatus != "New")
            //    {
            //        if ((collegeIntakeExisting.AICTEapprovedIntake1 == 0) && collegeIntakeExisting.courseStatus != "Closure")
            //        {
            //            TempData["Error"] = "AICTE Sanctioned Intake Should not be Zero";
            //            return RedirectToAction("Index",
            //                new
            //                {
            //                    collegeId =
            //                        Utilities.EncryptString(userCollegeID.ToString(),
            //                            WebConfigurationManager.AppSettings["CryptoKey"]).ToString()
            //                });

            //        }
            //    }
            //}
            List<jntuh_college_intake_existing> jntuh_college_intake_existinglist =
                new List<jntuh_college_intake_existing>();
            ViewBag.Degree =
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
                    }).OrderBy(d => d.degree).ToList();

            ViewBag.Department = db.jntuh_department.Where(d => d.isActive == true);
            ViewBag.Specialization = db.jntuh_specialization.Where(s => s.isActive == true);
            ViewBag.Shift = db.jntuh_shift.Where(s => s.isActive == true);
            ViewBag.CourseAffiliationstatus = db.jntuh_course_affiliation_status.Where(c => c.isActive == true);
            //collegeIntakeExisting.shiftId = 1;
            if (collegeIntakeExisting.nbaFromDate != null)
                collegeIntakeExisting.nbaFrom =
                    Convert.ToDateTime(Utilities.DDMMYY2MMDDYY(collegeIntakeExisting.nbaFromDate));
            if (collegeIntakeExisting.nbaToDate != null)
                collegeIntakeExisting.nbaTo =
                    Convert.ToDateTime(Utilities.DDMMYY2MMDDYY(collegeIntakeExisting.nbaToDate));
            string nbafilename = string.Empty;
            if (ModelState.IsValid)
            {
                collegeIntakeExisting.collegeId = userCollegeID;
                var jntuhcollege = db.jntuh_college.Where(c => c.id == userCollegeID).Select(s => s).FirstOrDefault();
                //if (collegeIntakeExisting.NBAApproveLetter != null)
                //{
                //    if (!Directory.Exists(Server.MapPath("~/Content/Upload/CollegePhotos")))
                //    {
                //        Directory.CreateDirectory(Server.MapPath("~/Content/Upload/CollegePhotos"));
                //    }

                //    var ext = Path.GetExtension(collegeIntakeExisting.NBAApproveLetter.FileName);
                //    if (ext.ToUpper().Equals(".PDF") || ext.ToUpper().Equals(".pdf"))
                //    {
                //        var fileName = collegeIntakeExisting.id + "-" + collegeIntakeExisting.collegeId + "-" + DateTime.Now.ToString("yyyyMMdd-HHmmssffffff");
                //        //collegeIntakeExisting.NBAApproveLetter.FileName;
                //        collegeIntakeExisting.NBAApproveLetter.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath("~/Content/Upload/CollegePhotos"), fileName, ext));
                //        nbafilename = string.Format("{0}{1}", fileName, ext);
                //    }
                //}
                //File Saving Writen By Narayana
                if (collegeIntakeExisting.NBAApproveLetter != null)
                {
                    if (!Directory.Exists(Server.MapPath("~/Content/Upload/CollegePhotos")))
                    {
                        Directory.CreateDirectory(Server.MapPath("~/Content/Upload/CollegePhotos"));
                    }

                    var ext = Path.GetExtension(collegeIntakeExisting.NBAApproveLetter.FileName);
                    if (ext.ToUpper().Equals(".PDF") || ext.ToUpper().Equals(".pdf"))
                    {
                        var fileName = jntuhcollege.collegeCode.Trim() + "-NBA-" +
                                       DateTime.Now.ToString("yyyyMMdd-HHmmssffffff");
                        //collegeIntakeExisting.NBAApproveLetter.FileName;
                        collegeIntakeExisting.NBAApproveLetter.SaveAs(string.Format("{0}/{1}{2}",
                            Server.MapPath("~/Content/Upload/CollegePhotos"), fileName, ext));
                        nbafilename = string.Format("{0}{1}", fileName, ext);
                    }
                }
                else
                {

                    nbafilename = collegeIntakeExisting.UploadNBAApproveLetter;
                }

                // List<jntuh_college_intake_existing> intake = db.jntuh_college_intake_existing.Where(i => i.collegeId == userCollegeID).ToList();
                int presentAY =
                    db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true)
                        .Select(a => a.actualYear)
                        .FirstOrDefault();

                #region For Loop Starting

                for (int i = -1; i < 5; i++)
                //for (int i = -1; i <= -1; i++)
                {
                    int? approved = 0;
                    int? Aicteapproved = 0;
                    int admitted = 0;
                    int EadmittedR = 0;
                    int EadmittedL = 0;
                    int academicYear = 0;

                    int? proposed = null;
                    string letterPath = null;

                    if (i == -1)
                    {
                        Aicteapproved = 0;
                        approved = collegeIntakeExisting.ApprovedIntake != null
                            ? collegeIntakeExisting.ApprovedIntake
                            : 0;
                        admitted = 0;
                        EadmittedR = 0;
                        EadmittedL = 0;
                        academicYear = presentAY + 1;
                        //academicYear = presentAY + 1;

                        letterPath = collegeIntakeExisting.letterPath;
                        if (collegeIntakeExisting.ProposedIntake != null)
                            proposed = collegeIntakeExisting.ProposedIntake;
                        else if (collegeIntakeExisting.courseStatus == "Closure")
                            proposed = 0;


                    }
                    if (i == 0)
                    {
                        Aicteapproved = collegeIntakeExisting.AICTEapprovedIntake1;
                        approved = collegeIntakeExisting.approvedIntake1;
                        admitted = collegeIntakeExisting.admittedIntake1;
                        EadmittedR = collegeIntakeExisting.ExambranchadmittedIntakeR1;
                        EadmittedL = collegeIntakeExisting.ExambranchadmittedIntakeL1;
                        academicYear = presentAY - i;
                        //academicYear = presentAY + 1;
                    }
                    if (i == 1)
                    {
                        Aicteapproved = collegeIntakeExisting.AICTEapprovedIntake2;
                        approved = collegeIntakeExisting.approvedIntake2;
                        admitted = collegeIntakeExisting.admittedIntake2;
                        EadmittedR = collegeIntakeExisting.ExambranchadmittedIntakeR2;
                        EadmittedL = collegeIntakeExisting.ExambranchadmittedIntakeL2;
                        academicYear = presentAY - i;
                        //academicYear = presentAY;
                    }
                    if (i == 2)
                    {
                        Aicteapproved = collegeIntakeExisting.AICTEapprovedIntake3;
                        approved = collegeIntakeExisting.approvedIntake3;
                        admitted = collegeIntakeExisting.admittedIntake3;
                        EadmittedR = collegeIntakeExisting.ExambranchadmittedIntakeR3;
                        EadmittedL = collegeIntakeExisting.ExambranchadmittedIntakeL3;
                        academicYear = presentAY - i;
                        //academicYear = presentAY - 1;
                    }
                    if (i == 3)
                    {
                        Aicteapproved = collegeIntakeExisting.AICTEapprovedIntake4;
                        approved = collegeIntakeExisting.approvedIntake4;
                        admitted = collegeIntakeExisting.admittedIntake4;
                        EadmittedR = collegeIntakeExisting.ExambranchadmittedIntakeR4;
                        EadmittedL = collegeIntakeExisting.ExambranchadmittedIntakeL4;
                        academicYear = presentAY - i;
                        //academicYear = presentAY - 2;
                    }
                    if (i == 4)
                    {
                        Aicteapproved = collegeIntakeExisting.AICTEapprovedIntake5;
                        approved = collegeIntakeExisting.approvedIntake5;
                        admitted = collegeIntakeExisting.admittedIntake5;
                        EadmittedR = collegeIntakeExisting.ExambranchadmittedIntakeR5;
                        EadmittedL = collegeIntakeExisting.ExambranchadmittedIntakeL5;
                        academicYear = presentAY - i;
                        //academicYear = presentAY - 3;
                    }

                    jntuh_college_intake_existing jntuh_college_intake_existing = new jntuh_college_intake_existing();

                    //  jntuh_college_intake_existing_log jntuh_college_intake_existinglog = new jntuh_college_intake_existing_log();
                    jntuh_college_intake_existing.academicYearId =
                        db.jntuh_academic_year.Where(a => a.actualYear == academicYear)
                            .Select(a => a.id)
                            .FirstOrDefault();

                    var existingId =
                        db.jntuh_college_intake_existing.Where(
                            p => p.specializationId == collegeIntakeExisting.specializationId
                                 && p.shiftId == collegeIntakeExisting.shiftId
                                 && p.collegeId == collegeIntakeExisting.collegeId
                                 && p.academicYearId == jntuh_college_intake_existing.academicYearId)
                            .Select(p => p.id)
                            .FirstOrDefault();

                    //var existingIntake = db.jntuh_college_intake_existing.Find(existingId);
                    int createdByu =
                        Convert.ToInt32(
                            db.jntuh_college_intake_existing.Where(
                                a => a.collegeId == userCollegeID && a.id == existingId)
                                .Select(a => a.createdBy)
                                .FirstOrDefault());
                    DateTime createdonu =
                        Convert.ToDateTime(
                            db.jntuh_college_intake_existing.Where(
                                a => a.collegeId == userCollegeID && a.id == existingId)
                                .Select(a => a.createdOn)
                                .FirstOrDefault());

                    //if ((approved > 0 && i != -1) || (i != -1 && admitted > 0 && existingId == 0) || (existingId > 0) || (i == -1))
                    if ((approved > 0 && i != -1) || (i != -1 && existingId == 0) || (existingId > 0) || (i == -1))
                    {
                        jntuh_college_intake_existing.id = collegeIntakeExisting.id;
                        jntuh_college_intake_existing.collegeId = collegeIntakeExisting.collegeId;
                        jntuh_college_intake_existing.academicYearId =
                            db.jntuh_academic_year.Where(a => a.actualYear == academicYear)
                                .Select(a => a.id)
                                .FirstOrDefault();
                        jntuh_college_intake_existing.specializationId = collegeIntakeExisting.specializationId;
                        jntuh_college_intake_existing.shiftId = collegeIntakeExisting.shiftId;
                        jntuh_college_intake_existing.aicteApprovedIntake = (int)Aicteapproved;
                        jntuh_college_intake_existing.approvedIntake = (int)approved;
                        jntuh_college_intake_existing.admittedIntake = admitted;
                        jntuh_college_intake_existing.admittedIntakeasperExambranch_R = EadmittedR;
                        jntuh_college_intake_existing.admittedIntakeasperExambranch_L = EadmittedL;
                        jntuh_college_intake_existing.approvalLetter = letterPath; //new
                        jntuh_college_intake_existing.proposedIntake = proposed; //new
                        jntuh_college_intake_existing.nbaFrom = collegeIntakeExisting.nbaFrom;
                        jntuh_college_intake_existing.nbaTo = collegeIntakeExisting.nbaTo;
                        jntuh_college_intake_existing.isActive = true;
                        jntuh_college_intake_existing.courseStatus = collegeIntakeExisting.courseStatus != null
                            ? collegeIntakeExisting.courseStatus
                            : "New";
                        jntuh_college_intake_existing.courseAffiliatedStatus =
                            collegeIntakeExisting.courseAffiliatedStatus;
                        jntuh_college_intake_existing.NBAApproveLetter = nbafilename;
                        if (existingId == 0)
                        {
                            jntuh_college_intake_existing.createdBy = userID;
                            jntuh_college_intake_existing.createdOn = DateTime.Now;
                            db.jntuh_college_intake_existing.Add(jntuh_college_intake_existing);
                        }
                        else
                        {
                            jntuh_college_intake_existing.createdBy = createdByu;
                            jntuh_college_intake_existing.createdOn = createdonu;
                            jntuh_college_intake_existing.id = existingId;
                            jntuh_college_intake_existing.updatedBy = userID;
                            jntuh_college_intake_existing.updatedOn = DateTime.Now;
                            db.Entry(jntuh_college_intake_existing).State = EntityState.Modified;

                        }
                        db.SaveChanges();

                    }
                    jntuh_college_intake_existing CollegeIntakeLog =
                        db.jntuh_college_intake_existing.Where(
                            a =>
                                a.academicYearId == jntuh_college_intake_existing.academicYearId &&
                                a.specializationId == collegeIntakeExisting.specializationId &&
                                a.shiftId == collegeIntakeExisting.shiftId &&
                                a.collegeId == collegeIntakeExisting.collegeId).Select(s => s).FirstOrDefault();
                    jntuh_college_intake_existinglist.Add(CollegeIntakeLog);
                }

                #endregion

                //List<jntuh_college_intake_existing> jntuh_college_intake_existinglistlog = db.jntuh_college_intake_existing.Where(C => C.collegeId == userCollegeID).ToList();
                //jntuh_college_intake_existing_log jntuh_college_intake_existinglog = new jntuh_college_intake_existing_log();

                foreach (var CollegeIntakeLog in jntuh_college_intake_existinglist)
                {
                    if (CollegeIntakeLog != null)
                    {
                        jntuh_college_intake_existing_log jntuh_college_intake_existinglog =
                            new jntuh_college_intake_existing_log();
                        jntuh_college_intake_existinglog.id = CollegeIntakeLog.id;
                        jntuh_college_intake_existinglog.collegeId = CollegeIntakeLog.collegeId;
                        jntuh_college_intake_existinglog.academicYearId = CollegeIntakeLog.academicYearId;
                        jntuh_college_intake_existinglog.specializationId = CollegeIntakeLog.specializationId;
                        jntuh_college_intake_existinglog.shiftId = CollegeIntakeLog.shiftId;
                        jntuh_college_intake_existinglog.approvedIntake = CollegeIntakeLog.approvedIntake;
                        jntuh_college_intake_existinglog.admittedIntake = CollegeIntakeLog.admittedIntake;
                        jntuh_college_intake_existinglog.approvalLetter = CollegeIntakeLog.approvalLetter; //new
                        jntuh_college_intake_existinglog.proposedIntake = CollegeIntakeLog.proposedIntake; //new
                        jntuh_college_intake_existinglog.nbaFrom = CollegeIntakeLog.nbaFrom;
                        jntuh_college_intake_existinglog.nbaTo = CollegeIntakeLog.nbaTo;
                        jntuh_college_intake_existinglog.isActive = CollegeIntakeLog.isActive;
                        jntuh_college_intake_existinglog.createdBy = CollegeIntakeLog.createdBy;
                        jntuh_college_intake_existinglog.createdOn = CollegeIntakeLog.createdOn;
                        jntuh_college_intake_existinglog.updatedBy = CollegeIntakeLog.updatedBy;
                        jntuh_college_intake_existinglog.updatedOn = CollegeIntakeLog.updatedOn;
                        jntuh_college_intake_existinglog.courseStatus = CollegeIntakeLog.courseStatus;
                        jntuh_college_intake_existinglog.courseAffiliatedStatus =
                            Convert.ToBoolean(CollegeIntakeLog.courseAffiliatedStatus);
                        db.jntuh_college_intake_existing_log.Add(jntuh_college_intake_existinglog);
                        try
                        {
                            db.SaveChanges();
                        }
                        catch (Exception)
                        {

                            throw;
                        }

                    }

                }
                if (cmd == "Add")
                {
                    TempData["Success"] = "Added successfully.";
                }
                else
                {
                    TempData["Success"] = "Updated successfully.";
                }

                return RedirectToAction("Index",
                    new
                    {
                        collegeId =
                            Utilities.EncryptString(userCollegeID.ToString(),
                                WebConfigurationManager.AppSettings["CryptoKey"]).ToString()
                    });
            }
            else
            {
                return RedirectToAction("Index",
                    new
                    {
                        collegeId =
                            Utilities.EncryptString(userCollegeID.ToString(),
                                WebConfigurationManager.AppSettings["CryptoKey"]).ToString()
                    });
            }

        }

        public ActionResult UserView(string id)
        {
            int userCollegeID =
                Convert.ToInt32(Utilities.DecryptString(id, WebConfigurationManager.AppSettings["CryptoKey"]));

            ViewBag.AcademicYear =
                db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true)
                    .Select(a => a.academicYear)
                    .FirstOrDefault();
            int actualYear =
                db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true)
                    .Select(a => a.actualYear)
                    .FirstOrDefault();

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
                db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true)
                    .Select(a => a.actualYear)
                    .FirstOrDefault();
            int AY1 =
                db.jntuh_academic_year.Where(a => a.actualYear == (presentYear)).Select(a => a.id).FirstOrDefault();
            int AY2 =
                db.jntuh_academic_year.Where(a => a.actualYear == (presentYear - 1)).Select(a => a.id).FirstOrDefault();
            int AY3 =
                db.jntuh_academic_year.Where(a => a.actualYear == (presentYear - 2)).Select(a => a.id).FirstOrDefault();
            int AY4 =
                db.jntuh_academic_year.Where(a => a.actualYear == (presentYear - 3)).Select(a => a.id).FirstOrDefault();
            int AY5 =
                db.jntuh_academic_year.Where(a => a.actualYear == (presentYear - 4)).Select(a => a.id).FirstOrDefault();

            ////RAMESH : The following code is not required becoz JNTU is not going to use the code defined for COURT

            //ViewBag.FirstYear = String.Format("{0}-{1}", (actualYear + 1).ToString(), (actualYear + 2).ToString().Substring(2, 2));
            //ViewBag.SecondYear = String.Format("{0}-{1}", (actualYear).ToString(), (actualYear + 1).ToString().Substring(2, 2));
            //ViewBag.ThirdYear = String.Format("{0}-{1}", (actualYear - 1).ToString(), (actualYear).ToString().Substring(2, 2));
            //ViewBag.FourthYear = String.Format("{0}-{1}", (actualYear - 2).ToString(), (actualYear - 1).ToString().Substring(2, 2));
            //ViewBag.FifthYear = String.Format("{0}-{1}", (actualYear - 3).ToString(), (actualYear - 2).ToString().Substring(2, 2));

            //int presentYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();

            //int AY1 = db.jntuh_academic_year.Where(a => a.actualYear == (presentYear + 1)).Select(a => a.id).FirstOrDefault();
            //int AY2 = db.jntuh_academic_year.Where(a => a.actualYear == (presentYear)).Select(a => a.id).FirstOrDefault();
            //int AY3 = db.jntuh_academic_year.Where(a => a.actualYear == (presentYear - 1)).Select(a => a.id).FirstOrDefault();
            //int AY4 = db.jntuh_academic_year.Where(a => a.actualYear == (presentYear - 2)).Select(a => a.id).FirstOrDefault();
            //int AY5 = db.jntuh_academic_year.Where(a => a.actualYear == (presentYear - 3)).Select(a => a.id).FirstOrDefault();

            List<jntuh_college_intake_existing> intake =
                db.jntuh_college_intake_existing.Where(i => i.collegeId == userCollegeID).ToList();

            List<CollegeIntakeExisting> collegeIntakeExisting = new List<CollegeIntakeExisting>();

            foreach (var item in intake)
            {
                CollegeIntakeExisting newIntake = new CollegeIntakeExisting();
                newIntake.id = item.id;
                newIntake.collegeId = item.collegeId;
                newIntake.academicYearId = item.academicYearId;
                newIntake.shiftId = item.shiftId;
                newIntake.isActive = item.isActive;
                newIntake.nbaFrom = item.nbaFrom;
                newIntake.nbaTo = item.nbaTo;
                newIntake.specializationId = item.specializationId;
                newIntake.Specialization =
                    db.jntuh_specialization.Where(s => s.id == item.specializationId)
                        .Select(s => s.specializationName)
                        .FirstOrDefault();
                newIntake.DepartmentID =
                    db.jntuh_specialization.Where(s => s.id == item.specializationId)
                        .Select(s => s.departmentId)
                        .FirstOrDefault();
                newIntake.Department =
                    db.jntuh_department.Where(d => d.id == newIntake.DepartmentID)
                        .Select(d => d.departmentName)
                        .FirstOrDefault();
                newIntake.degreeID =
                    db.jntuh_department.Where(d => d.id == newIntake.DepartmentID)
                        .Select(d => d.degreeId)
                        .FirstOrDefault();
                newIntake.Degree =
                    db.jntuh_degree.Where(d => d.id == newIntake.degreeID).Select(d => d.degree).FirstOrDefault();
                newIntake.shiftId = item.shiftId;
                newIntake.Shift =
                    db.jntuh_shift.Where(s => s.id == item.shiftId).Select(s => s.shiftName).FirstOrDefault();
                collegeIntakeExisting.Add(newIntake);
            }

            collegeIntakeExisting =
                collegeIntakeExisting.AsEnumerable()
                    .GroupBy(r => new { r.specializationId, r.shiftId })
                    .Select(r => r.First())
                    .ToList();
            foreach (var item in collegeIntakeExisting)
            {
                if (item.nbaFrom != null)
                    item.nbaFromDate = Utilities.MMDDYY2DDMMYY(item.nbaFrom.ToString());
                if (item.nbaTo != null)
                    item.nbaToDate = Utilities.MMDDYY2DDMMYY(item.nbaTo.ToString());

                item.approvedIntake1 = GetIntake(userCollegeID, AY1, item.specializationId, item.shiftId, 1);
                item.admittedIntake1 = GetIntake(userCollegeID, AY1, item.specializationId, item.shiftId, 0);

                item.approvedIntake2 = GetIntake(userCollegeID, AY2, item.specializationId, item.shiftId, 1);
                item.admittedIntake2 = GetIntake(userCollegeID, AY2, item.specializationId, item.shiftId, 0);

                item.approvedIntake3 = GetIntake(userCollegeID, AY3, item.specializationId, item.shiftId, 1);
                item.admittedIntake3 = GetIntake(userCollegeID, AY3, item.specializationId, item.shiftId, 0);

                item.approvedIntake4 = GetIntake(userCollegeID, AY4, item.specializationId, item.shiftId, 1);
                item.admittedIntake4 = GetIntake(userCollegeID, AY4, item.specializationId, item.shiftId, 0);

                item.approvedIntake5 = GetIntake(userCollegeID, AY5, item.specializationId, item.shiftId, 1);
                item.admittedIntake5 = GetIntake(userCollegeID, AY5, item.specializationId, item.shiftId, 0);
            }
            ViewBag.ExistingIntake = collegeIntakeExisting;
            ViewBag.Count = collegeIntakeExisting.Count();
            return View();
        }

        public ActionResult FileUpload(HttpPostedFileBase fileUploader, string collegeId)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID =
                db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            int actualYear =
                db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true)
                    .Select(a => a.actualYear)
                    .FirstOrDefault();
            int ay0 =
                db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();
            if (userCollegeID == 0)
            {
                userCollegeID = Convert.ToInt32(collegeId);
            }
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            string fileName = string.Empty;
            int presentAY =
                db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true)
                    .Select(a => a.actualYear)
                    .FirstOrDefault();
            int academicyearId =
                db.jntuh_academic_year.Where(a => a.actualYear == (presentAY + 1))
                    .Select(a => a.actualYear)
                    .FirstOrDefault();
            int enclosureId =
                db.jntuh_enclosures.Where(e => e.documentName == "AICTE INTAKE APPROVAL LETTER")
                    .Select(e => e.id)
                    .FirstOrDefault();
            var college_enclosures =
                db.jntuh_college_enclosures.Where(
                    e => e.enclosureId == enclosureId && e.academicyearId == ay0 && e.collegeID == userCollegeID)
                    .Select(e => e)
                    .FirstOrDefault();
            jntuh_college_enclosures jntuh_college_enclosures = new jntuh_college_enclosures();
            jntuh_college_enclosures.collegeID = userCollegeID;
            jntuh_college_enclosures.academicyearId = ay0;
            jntuh_college_enclosures.enclosureId = enclosureId;
            jntuh_college_enclosures.isActive = true;
            if (fileUploader != null)
            {
                string ext = Path.GetExtension(fileUploader.FileName);
                //DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" + faculty.FirstName.Substring(0, 1) + "-" + faculty.LastName.Substring(0, 1)
                fileName =
                    db.jntuh_college.Where(c => c.id == userCollegeID).Select(c => c.collegeCode).FirstOrDefault() +
                    "_APL_" + enclosureId + "-" + DateTime.Now.ToString("yyyyMMdd-HHmmss") + ext;
                fileUploader.SaveAs(string.Format("{0}/{1}", Server.MapPath("~/Content/Upload/CollegeEnclosures/"),
                    fileName));
                jntuh_college_enclosures.path = fileName;
            }
            else if (!string.IsNullOrEmpty(college_enclosures.path))
            {
                fileName = college_enclosures.path;
                jntuh_college_enclosures.path = fileName;
            }

            if (college_enclosures == null)
            {
                jntuh_college_enclosures.createdBy = userID;
                jntuh_college_enclosures.createdOn = DateTime.Now;
                db.jntuh_college_enclosures.Add(jntuh_college_enclosures);
                db.SaveChanges();
            }
            else
            {
                college_enclosures.path = fileName;
                college_enclosures.updatedBy = userID;
                college_enclosures.updatedOn = DateTime.Now;
                db.Entry(college_enclosures).State = EntityState.Modified;
                db.SaveChanges();
            }
            return RedirectToAction("Index",
                new
                {
                    collegeId =
                        Utilities.EncryptString(userCollegeID.ToString(),
                            WebConfigurationManager.AppSettings["CryptoKey"]).ToString()
                });

        }

        [Authorize(Roles = "Admin,SuperAdmin,College")]
        public ActionResult AICTEFileUpload(HttpPostedFileBase fileUploader, string collegeId)
        {
            return RedirectToAction("College", "Dashboard");
            if (Membership.GetUser() != null)
            {
                int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
                int userCollegeID =
                    db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
                if (userCollegeID == 0)
                {
                    userCollegeID = Convert.ToInt32(collegeId);
                }
                //To Save File in jntuh_college_enclosures
                string fileName = string.Empty;
                int presentAY =
                    db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true)
                        .Select(a => a.actualYear)
                        .FirstOrDefault();
                int academicyearId =
                    db.jntuh_academic_year.Where(a => a.actualYear == (presentAY + 1))
                        .Select(a => a.actualYear)
                        .FirstOrDefault();
                int enclosureId =
                    db.jntuh_enclosures.Where(e => e.documentName == "AICTE INTAKE APPROVAL LETTER")
                        .Select(e => e.id)
                        .FirstOrDefault();
                var college_enclosures =
                    db.jntuh_college_enclosures.Where(e => e.enclosureId == enclosureId && e.collegeID == userCollegeID)
                        .Select(e => e)
                        .FirstOrDefault();
                jntuh_college_enclosures jntuh_college_enclosures = new jntuh_college_enclosures();
                jntuh_college_enclosures.collegeID = userCollegeID;
                jntuh_college_enclosures.enclosureId = enclosureId;
                jntuh_college_enclosures.isActive = true;
                if (fileUploader != null)
                {
                    string ext = Path.GetExtension(fileUploader.FileName);
                    //DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" + faculty.FirstName.Substring(0, 1) + "-" + faculty.LastName.Substring(0, 1)
                    //if (!String.IsNullOrEmpty(college_enclosures.path))
                    //{
                    //    fileName = college_enclosures.path;
                    //}
                    //else
                    //{
                    fileName =
                        db.jntuh_college.Where(c => c.id == userCollegeID).Select(c => c.collegeCode).FirstOrDefault() +
                        "_APL_" + enclosureId + "-" + DateTime.Now.ToString("yyyyMMdd-HHmmss") + ext;
                    //}               
                    fileUploader.SaveAs(string.Format("{0}/{1}", Server.MapPath("~/Content/Upload/CollegeEnclosures"),
                        fileName));
                    jntuh_college_enclosures.path = fileName;
                }
                else if (!string.IsNullOrEmpty(college_enclosures.path))
                {
                    fileName = college_enclosures.path;
                    jntuh_college_enclosures.path = fileName;
                }

                if (college_enclosures == null)
                {
                    jntuh_college_enclosures.createdBy = userID;
                    jntuh_college_enclosures.createdOn = DateTime.Now;
                    db.jntuh_college_enclosures.Add(jntuh_college_enclosures);
                    db.SaveChanges();
                }
                else
                {
                    college_enclosures.path = fileName;
                    college_enclosures.updatedBy = userID;
                    college_enclosures.updatedOn = DateTime.Now;
                    db.Entry(college_enclosures).State = EntityState.Modified;
                    db.SaveChanges();
                }
                return RedirectToAction("AicteIntakeApproval");
            }
            else
            {
                return RedirectToAction("LogOn", "Account");
            }
        }

        [Authorize(Roles = "Admin,SuperAdmin,College")]
        [HttpGet]
        public ActionResult AicteIntakeApproval()
        {
            return RedirectToAction("College", "Dashboard");
            if (Membership.GetUser() != null)
            {
                int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
                int userCollegeID =
                    db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
                if (userCollegeID == 0 || userCollegeID == null)
                {
                    return RedirectToAction("College", "Dashboard");
                }
                int enclosureId =
                    db.jntuh_enclosures.Where(e => e.documentName == "AICTE INTAKE APPROVAL LETTER")
                        .Select(e => e.id)
                        .FirstOrDefault();
                var AICTEApprovalLettr =
                    db.jntuh_college_enclosures.Where(e => e.enclosureId == enclosureId && e.collegeID == userCollegeID)
                        .OrderByDescending(a => a.id)
                        .Select(e => e.path)
                        .FirstOrDefault();
                ViewBag.AICTEApprovalLettr = AICTEApprovalLettr;
                return View();
            }
            else
            {
                return RedirectToAction("LogOn", "Account");
            }
        }

        public ActionResult FeeDetailsandPayment()
        {
            if (Membership.GetUser() == null)
            {
                return RedirectToAction("LogOn", "Account");
            }
            else
            {
                int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
                int userCollegeID =
                    db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
                if (userCollegeID == 375)
                {
                    userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
                }
                string clgCode = db.jntuh_college.SingleOrDefault(a => a.id == userCollegeID).collegeCode;

                List<CollegeIntakeExisting> collegeIntakeExisting = new List<CollegeIntakeExisting>();
                int PresentAcademicYear =
                    db.jntuh_college_intake_existing.Where(a => a.isActive == true)
                        .OrderByDescending(a => a.academicYearId)
                        .Select(a => a.academicYearId)
                        .FirstOrDefault();
                if (userCollegeID > 0 && userCollegeID != null)
                {

                    //List<jntuh_college_intake_existing> intake = db.jntuh_college_intake_existing.Where(i => i.collegeId == userCollegeID && i.academicYearId == PresentAcademicYear).ToList();
                    List<jntuh_college_intake_existing> intake =
                        db.jntuh_college_intake_existing.Where(
                            i => i.collegeId == userCollegeID && i.academicYearId == PresentAcademicYear).ToList();
                    var jntuh_specialization = db.jntuh_specialization;
                    var jntuh_department = db.jntuh_department;
                    var jntuh_degree = db.jntuh_degree;
                    var jntuh_shift = db.jntuh_shift;
                    List<int> dualdegrees = new List<int>();
                    List<int> pgdegrees = new List<int>();
                    List<int> ugdegrees = new List<int>();
                    List<int> totaldegrees = new List<int>();
                    long DualdegreeSpecializationAmmount = 0;
                    long ugSpecializationAmmount = 0;
                    long pgSpecializationAmmount = 0;
                    long applicationFee = 0;
                    int dualCount = 0;
                    int ugCount = 0;
                    int pgCount = 0;
                    if (TempData["PaymentStatus"] != null)
                        TempData["PaymentStatus1"] = TempData["PaymentStatus"];
                    else
                    {
                        int status = 0;
                        TempData["PaymentStatus1"] = "";
                    }

                    var intakeExisting =
                        intake.GroupBy(r => new { r.specializationId, r.shiftId }).Select(r => r.First()).ToList();

                    int actualYear =
                        db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true)
                            .Select(a => a.actualYear)
                            .FirstOrDefault();
                    int AY0 =
                        db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1))
                            .Select(a => a.id)
                            .FirstOrDefault();
                    foreach (var item in intakeExisting)
                    {
                        jntuh_college_intake_existing details = db.jntuh_college_intake_existing
                            .Where(
                                e =>
                                    e.collegeId == userCollegeID && e.academicYearId == AY0 &&
                                    e.specializationId == item.specializationId && e.shiftId == item.shiftId)
                            .Select(e => e)
                            .FirstOrDefault();


                        if (details != null)
                        {
                            if (item.jntuh_specialization.jntuh_department.degreeId == 5 ||
                                item.jntuh_specialization.jntuh_department.degreeId == 4)
                            {
                                if (item.proposedIntake != 0 && item.courseStatus != "Closure")
                                {
                                    ugdegrees.Add(item.jntuh_specialization.jntuh_department.degreeId);
                                    if (ugdegrees.Count != 0 && ugdegrees.Count <= 4)
                                    {
                                        ugSpecializationAmmount = 25000;
                                        totaldegrees.AddRange(ugdegrees);
                                    }
                                    else
                                    {
                                        ugCount++;
                                        ugSpecializationAmmount = 25000 + (ugCount * 4000);
                                        totaldegrees.AddRange(ugdegrees);
                                    }
                                }

                            }
                            else if (item.jntuh_specialization.jntuh_department.degreeId == 7)
                            {
                                if (item.proposedIntake != 0 && item.courseStatus != "Closure")
                                {
                                    dualdegrees.Add(item.jntuh_specialization.jntuh_department.degreeId);
                                    dualCount++;
                                    DualdegreeSpecializationAmmount = dualCount * 40000;
                                    totaldegrees.AddRange(dualdegrees);
                                }

                            }
                            else
                            {
                                if (item.proposedIntake != 0 && item.courseStatus != "Closure")
                                {
                                    pgdegrees.Add(item.jntuh_specialization.jntuh_department.degreeId);
                                    pgCount++;
                                    pgSpecializationAmmount = pgCount * 12000;
                                    totaldegrees.AddRange(pgdegrees);
                                }

                            }
                        }



                        // totaldegrees.AddRange(pgdegrees);
                        //totaldegrees.AddRange(ugdegrees);
                    }
                    if (pgdegrees.Count > 0 && ugdegrees.Count > 0)
                        applicationFee = 1000;
                    else
                        applicationFee = 750;


                    // collegeIntakeExisting = collegeIntakeExisting.AsEnumerable().GroupBy(r => new { r.specializationId, r.shiftId }).Select(r => r.First()).ToList();
                    //ugdegrees.AsEnumerable().Where(r=> new{r.co})
                    ViewBag.userCollegeID = userCollegeID;
                    ViewBag.countofUgcourse = ugdegrees.Count;
                    ViewBag.countofPgcourse = pgdegrees.Count;
                    ViewBag.countofDualcourse = dualdegrees.Count;
                    ViewBag.ugSpecializationAmmount = ugSpecializationAmmount;
                    ViewBag.pgSpecializationAmmount = pgSpecializationAmmount;
                    ViewBag.DualdegreeSpecializationAmmount = DualdegreeSpecializationAmmount;
                    ViewBag.applicationFee = applicationFee;
                    ViewBag.totalFee = ugSpecializationAmmount + pgSpecializationAmmount +
                                       DualdegreeSpecializationAmmount + applicationFee;
                    ViewBag.collegeCode = clgCode;
                    ViewBag.challnNumber = clgCode + DateTime.Now.ToString("yyyMMddHHmmss");
                    var currentYear = DateTime.Now.Year;
                    var payments =
                        db.jntuh_paymentresponse.Where(it => it.CollegeId == clgCode && it.AcademicYearId == AY0 && it.AuthStatus == "0300" && it.TxnDate.Year == currentYear && it.PaymentTypeID == 7)
                            .ToList();
                    ViewBag.Payments = payments;

                    ViewBag.IsPaymentDone =
                        db.jntuh_paymentresponse.Count(
                            it =>
                                it.CollegeId == clgCode && it.AcademicYearId == AY0 && it.TxnDate.Year == currentYear &&
                                it.AuthStatus == "0300" &&
                                it.PaymentTypeID == 7) > 0;
                    var returnUrl = WebConfigurationManager.AppSettings["ReturnUrl"];
                    var merchantId = WebConfigurationManager.AppSettings["MerchantID"];
                    var securityId = WebConfigurationManager.AppSettings["SecurityID"];
                    var typefield1 = WebConfigurationManager.AppSettings["TypeField1"];
                    var typefield2 = WebConfigurationManager.AppSettings["TypeField2"];
                    var msg = "";
                    if (userCollegeID == 375)
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

                    //Checking Edit status Is there or not.
                    DateTime todayDate = DateTime.Now.Date;
                    //int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
                    int prAy =
                        db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
                    int editstatus = db.jntuh_college_edit_status.Where(editStatus => editStatus.academicyearId == prAy && editStatus.collegeId == userCollegeID &&
                                                                          editStatus.IsCollegeEditable == true &&
                                                                          editStatus.editFromDate <= todayDate &&
                                                                          editStatus.editToDate >= todayDate)
                                                     .Select(editStatus => editStatus.id)
                                                     .FirstOrDefault();

                    if (editstatus == 0)
                    {
                        ViewBag.IsEditable = false;
                    }
                    else
                    {
                        ViewBag.IsEditable = true;
                    }
                }
            }
            return View();
        }

        public ActionResult Payment(int? id, string collegeId)
        {
            return View();
        }

        [HttpPost]
        public ActionResult SavePaymentRequest(string challanNumber, decimal txnAmount, string collegeCode)
        {

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
            req.PaymentTypeID = 7;
            db.jntuh_paymentrequests.Add(req);

            db.SaveChanges();
            return new HttpStatusCodeResult(HttpStatusCode.OK);
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

        [HttpPost]
        public ActionResult FeeDetailsandPayment(string msg)
        {
            var appSettings = WebConfigurationManager.AppSettings;
            var req = new jntuh_paymentrequests();
            //req.MerchantID = appSettings["MerchantID"];
            //req.CustomerID = appSettings["CustomerID"];
            //req.SecurityID = appSettings["SecurityID"];
            //req.CurrencyType = appSettings["CurrencyType"];
            //req.TxnDate = DateTime.Now;
            //db.jntuh_paymentrequests.Add(req);
            //db.SaveChanges();
            SaveResponse(msg, "ChallanNumber");
            return RedirectToAction("FeeDetailsandPayment");
        }

        private string CreateRequestMessage(string merchantId, string customerId, decimal amount, string currency,
            string typeField1, string securityId, string typeField2, string redirectUrl)
        {
            var format = "{0}|{1}|NA|{2}|NA|NA|NA|{3}|NA|{4}|{5}|NA|NA|{6}|NA|NA|NA|NA|NA|NA|NA|{7}";
            var msg = string.Format(format, merchantId, customerId, amount, currency, typeField1, securityId, typeField2,
                redirectUrl);
            return msg;
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

            //Response Message Saving
            temp_aeronautical temp_aeronautical = new temp_aeronautical();
            temp_aeronautical.Degree = responseMsg.Length < 255
                ? responseMsg
                : responseMsg.Substring(0, 254);
            ;
            temp_aeronautical.Department = "College&Inspectionfee";
            temp_aeronautical.Specialization = clgCode;
            temp_aeronautical.DegreeId = userCollegeID;
            temp_aeronautical.LabCode = DateTime.Now.ToString();
            db.temp_aeronautical.Add(temp_aeronautical);
            db.SaveChanges();
            int actualYear =
                db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true)
                    .Select(a => a.actualYear)
                    .FirstOrDefault();
            int prAy =
                db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
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
            dbResponse.PaymentTypeID = 7;
            db.jntuh_paymentresponse.Add(dbResponse);
            db.SaveChanges();
            //Log file paymentresponse
            //var filename = DateTime.Now.Date + ".txt";
            //string paymentpath = "~/Content/Payment/PaymentResponses";
            //FileStream fs = null;
            //if (!Directory.Exists(Server.MapPath(paymentpath)))
            //{
            //    Directory.CreateDirectory(Server.MapPath(paymentpath));
            //}
            //string filepath = Server.MapPath(paymentpath) + "\\" + @"LogFile" + DateTime.Now.ToString("CL_yyyyMMdd") + ".txt";
            //fs = new FileStream(filepath, FileMode.Append);
            //var log = new StreamWriter(fs, Encoding.UTF8);
            //log.WriteLine("Date : {0} Time : {1}", DateTime.Now.ToString("MMMM dd, yyyy"), DateTime.Now.ToString("hh:mm:ss"));
            //log.WriteLine("Message : {0} ", "College Code :- " + clgCode + ". Challan No :- " + challanno + ". MerchantID :- " + tokens[6] + ". CustomerID :- " + tokens[1] + ". Transaction Date :- " + DateTime.Now + ". Transaction Amount :- " + decimal.Parse(tokens[4]));
            //log.WriteLine("==============================================================================================================================================================");
            //log.Close();
            //fs.Close();
            //Log file paymentrequest

            //mail

            var collegename = db.jntuh_college.SingleOrDefault(a => a.id == userCollegeID).collegeName;
            var membershipmailid = db.my_aspnet_membership.Where(i => i.userId == userID).FirstOrDefault().Email;
            IUserMailer mailer = new UserMailer();
            mailer.PaymentResponse(membershipmailid, "Payment Response", dbResponse.CollegeId + " / " + collegename,
                dbResponse.CustomerID, dbResponse.TxnReferenceNo, dbResponse.BankReferenceNo, dbResponse.TxnAmount,
                dbResponse.TxnDate.ToString(), dbResponse.ErrorDescription, dbResponse.ChallanNumber, "Payment Response",
                "JNTUH-AAC-ONLINE APPLICATION PAYMENT STATUS").SendAsync();

        }

        public ActionResult Response()
        {
            return View();
        }

        [Authorize(Roles = "Admin,College")]
        public ActionResult FeeDetailsandPaymentView(string id)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID =
                db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();

            if (userCollegeID == 0)
            {
                userCollegeID =
                    Convert.ToInt32(Utilities.DecryptString(id,
                        System.Web.Configuration.WebConfigurationManager.AppSettings["CryptoKey"]));
            }


            string clgCode = db.jntuh_college.SingleOrDefault(a => a.id == userCollegeID).collegeCode;
            List<CollegeIntakeExisting> collegeIntakeExisting = new List<CollegeIntakeExisting>();
            int PresentAcademicYear =
                db.jntuh_college_intake_existing.Where(a => a.isActive == true)
                    .OrderByDescending(a => a.academicYearId)
                    .Select(a => a.academicYearId)
                    .FirstOrDefault();
            if (userCollegeID > 0 && userCollegeID != null)
            {

                //List<jntuh_college_intake_existing> intake = db.jntuh_college_intake_existing.Where(i => i.collegeId == userCollegeID && i.academicYearId == PresentAcademicYear).ToList();
                List<jntuh_college_intake_existing> intake =
                    db.jntuh_college_intake_existing.Where(
                        i => i.collegeId == userCollegeID && i.academicYearId == PresentAcademicYear).ToList();
                var jntuh_specialization = db.jntuh_specialization;
                var jntuh_department = db.jntuh_department;
                var jntuh_degree = db.jntuh_degree;
                var jntuh_shift = db.jntuh_shift;
                List<int> pgdegrees = new List<int>();
                List<int> ugdegrees = new List<int>();
                List<int> totaldegrees = new List<int>();
                long ugSpecializationAmmount = 0;
                long pgSpecializationAmmount = 0;
                long applicationFee = 0;
                int ugCount = 0;
                int pgCount = 0;
                if (TempData["PaymentStatus"] != null)
                    TempData["PaymentStatus1"] = TempData["PaymentStatus"];
                else
                {
                    int status = 0;
                    TempData["PaymentStatus1"] = "";
                }

                var intakeExisting =
                    intake.GroupBy(r => new { r.specializationId, r.shiftId }).Select(r => r.First()).ToList();

                int actualYear =
                    db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true)
                        .Select(a => a.actualYear)
                        .FirstOrDefault();
                int AY0 =
                    db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1))
                        .Select(a => a.id)
                        .FirstOrDefault();
                foreach (var item in intakeExisting)
                {
                    jntuh_college_intake_existing details = db.jntuh_college_intake_existing
                        .Where(
                            e =>
                                e.collegeId == userCollegeID && e.academicYearId == AY0 &&
                                e.specializationId == item.specializationId && e.shiftId == item.shiftId)
                        .Select(e => e)
                        .FirstOrDefault();


                    if (details != null)
                    {
                        if (item.jntuh_specialization.jntuh_department.degreeId == 5 ||
                            item.jntuh_specialization.jntuh_department.degreeId == 4)
                        {


                            if (item.proposedIntake != 0 && item.courseStatus != "Closure")
                            {
                                ugdegrees.Add(item.jntuh_specialization.jntuh_department.degreeId);
                                if (ugdegrees.Count != 0 && ugdegrees.Count <= 4)
                                {
                                    ugSpecializationAmmount = 25000;
                                    totaldegrees.AddRange(ugdegrees);
                                }
                                else
                                {
                                    ugCount++;
                                    ugSpecializationAmmount = 25000 + (ugCount * 4000);
                                    totaldegrees.AddRange(ugdegrees);
                                }
                            }

                        }
                        else
                        {
                            if (item.proposedIntake != 0 && item.courseStatus != "Closure")
                            {
                                pgdegrees.Add(item.jntuh_specialization.jntuh_department.degreeId);
                                pgCount++;
                                pgSpecializationAmmount = pgCount * 12000;
                                totaldegrees.AddRange(pgdegrees);
                            }

                        }
                    }



                    // totaldegrees.AddRange(pgdegrees);
                    //totaldegrees.AddRange(ugdegrees);
                }
                if (pgdegrees.Count > 0 && ugdegrees.Count > 0)
                    applicationFee = 1000;
                else
                    applicationFee = 750;


                // collegeIntakeExisting = collegeIntakeExisting.AsEnumerable().GroupBy(r => new { r.specializationId, r.shiftId }).Select(r => r.First()).ToList();
                //ugdegrees.AsEnumerable().Where(r=> new{r.co})
                ViewBag.userCollegeID = userCollegeID;
                ViewBag.countofUgcourse = ugdegrees.Count;
                ViewBag.countofPgcourse = pgdegrees.Count;
                ViewBag.ugSpecializationAmmount = ugSpecializationAmmount;
                ViewBag.pgSpecializationAmmount = pgSpecializationAmmount;
                ViewBag.applicationFee = applicationFee;
                ViewBag.totalFee = ugSpecializationAmmount + pgSpecializationAmmount + applicationFee;
                ViewBag.collegeCode = clgCode;
                ViewBag.challnNumber = clgCode + DateTime.Now.ToString("yyyMMddHHmmss");
                var currentYear = DateTime.Now.Year;
                var payments = db.jntuh_paymentresponse.Where(it => it.CollegeId == clgCode && it.AcademicYearId == AY0 && it.TxnDate.Year == currentYear && it.AuthStatus == "0300" && it.PaymentTypeID == 7).ToList();
                ViewBag.Payments = payments;

                ViewBag.IsPaymentDone =
                    db.jntuh_paymentresponse.Count(
                        it =>
                            it.CollegeId == clgCode && it.TxnDate.Year == currentYear && it.AuthStatus == "0300" &&
                            it.PaymentTypeID == 7) > 0;
                var returnUrl = WebConfigurationManager.AppSettings["ReturnUrl"];
                var merchantId = WebConfigurationManager.AppSettings["MerchantID"];
                var securityId = WebConfigurationManager.AppSettings["SecurityID"];
                var typefield1 = WebConfigurationManager.AppSettings["TypeField1"];
                var typefield2 = WebConfigurationManager.AppSettings["TypeField2"];
                var msg = "";
                if (userCollegeID == 375)
                {
                    msg = merchantId + "|" + ViewBag.challnNumber + "|NA|2.00|NA|NA|NA|INR|NA|" + typefield1 + "|" +
                          securityId + "|NA|NA|" + typefield2 + "|NA|NA|NA|NA|NA|NA|NA|" + returnUrl;
                    var key = WebConfigurationManager.AppSettings["ChecksumKey"];
                    msg += "|" + GetHMACSHA256(msg, key).ToUpper();
                }
                else
                {
                    msg = merchantId + "|" + ViewBag.challnNumber + "|NA|" + ViewBag.totalFee + "|NA|NA|NA|INR|NA|" +
                          typefield1 + "|" + securityId + "|NA|NA|" + typefield2 + "|NA|NA|NA|NA|NA|NA|NA|" + returnUrl;
                    var key = WebConfigurationManager.AppSettings["ChecksumKey"];
                    msg += "|" + GetHMACSHA256(msg, key).ToUpper();
                }

                ViewBag.msg = msg;

                //Checking Edit status Is there or not.
                DateTime todayDate = DateTime.Now.Date;
                //int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
                int prAy =
                    db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
                int editstatus = db.jntuh_college_edit_status.Where(editStatus => editStatus.academicyearId == prAy && editStatus.collegeId == userCollegeID &&
                                                                          editStatus.IsCollegeEditable == true &&
                                                                          editStatus.editFromDate <= todayDate &&
                                                                          editStatus.editToDate >= todayDate)
                                                     .Select(editStatus => editStatus.id)
                                                     .FirstOrDefault();

                if (editstatus == 0)
                {
                    ViewBag.IsEditable = false;
                }
                else
                {
                    ViewBag.IsEditable = true;
                }
            }
            return View();
        }


        [Authorize(Roles = "Admin,College")]
        public ActionResult UploadAICTEFileUploadIndex(string collegeId)
        {
            //if (!User.IsInRole("Admin"))
            //{
            //    return RedirectToAction("Index", "UnderConstruction");
            //}
            return RedirectToAction("CollegeDashboard", "Dashboard");
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID =
                db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();

            if (userCollegeID == 0 && collegeId != null)
            {
                if (Roles.IsUserInRole("Admin") || Roles.IsUserInRole("DataEntry"))
                {
                    userCollegeID =
                        Convert.ToInt32(Utilities.DecryptString(collegeId,
                            WebConfigurationManager.AppSettings["CryptoKey"]));
                }
            }
            //if Test college Login we get college id from web.config file
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }

            //Checking Edit Option
            DateTime todayDate = DateTime.Now.Date;
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int prAy = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();

            var lid = db.jntuh_link_screens.Where(p => p.linkName == "Upload AICTE/PCI EOA Details for A.Y. 2023-24" && p.isActive == true).Select(s => s.id).FirstOrDefault();
            var status = db.jntuh_college_links_assigned.Where(l => l.linkId == lid && l.academicyearId == prAy && l.isActive == true && l.fromdate <= todayDate && l.todate >= todayDate).Select(s => s.id).FirstOrDefault();
            //int status = db.jntuh_college_edit_status.Where(editStatus => editStatus.academicyearId == prAy && editStatus.collegeId == userCollegeID && editStatus.academicyearId == prAy &&
            //                                                                                    editStatus.IsCollegeEditable == true &&
            //                                                                                    editStatus.editFromDate <= todayDate &&
            //                                                                                    editStatus.editToDate >= todayDate)
            //                                                               .Select(editStatus => editStatus.id)
            //                                                               .FirstOrDefault();
            if (userCollegeID == 0 && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("Create", "CollegeInformation");
            }
            if (userCollegeID == 0 && (Roles.IsUserInRole("Admin") || Roles.IsUserInRole("DataEntry")))
            {
                return RedirectToAction("Create", "CollegeInformation",
                    new
                    {
                        collegeId =
                            Utilities.EncryptString(userCollegeID.ToString(),
                                WebConfigurationManager.AppSettings["CryptoKey"]).ToString()
                    });
            }
            if (status == 0 && Roles.IsUserInRole("College"))
            {
                ViewBag.NotUpload = true;
            }
            else
            {
                ViewBag.NotUpload = false;
            }
            if (status == 0 && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("College", "Dashboard");
                //return RedirectToAction("View", "CollegeIntakeExisting");
            }
            //bool isPageEditable =
            //    db.jntuh_college_screens_assigned.Where(
            //        a => a.jntuh_college_screens.ScreenCode.Equals("EP") && a.CollegeId == userCollegeID)
            //        .Select(a => a.IsEditable)
            //        .FirstOrDefault();

            //if (!isPageEditable)
            //{
            //    return RedirectToAction("View", "CollegeIntakeExisting");
            //}

            List<jntuh_academic_year> jntuh_academic_years = db.jntuh_academic_year.AsNoTracking().ToList();
            ViewBag.collegeId =
                Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"])
                    .ToString();
            ViewBag.AcademicYear =
                jntuh_academic_years.Where(a => a.isActive == true && a.isPresentAcademicYear == true)
                    .Select(a => a.academicYear)
                    .FirstOrDefault();
            //int PresentAcademicYear = db.jntuh_college_intake_existing.Where(a => a.isActive == true).OrderByDescending(a => a.academicYearId).Select(a => a.academicYearId).FirstOrDefault();
            //int actualYear =
            //    jntuh_academic_years.Where(a => a.isActive == true && a.isPresentAcademicYear == true)
            //        .Select(a => a.actualYear)
            //        .FirstOrDefault();

            //RAMESH: ADDED to MERGE BOTH EXISTING & PROPOSED INTAKE
            ViewBag.NextYear = String.Format("{0}-{1}", (actualYear + 1).ToString(),
                (actualYear + 2).ToString().Substring(2, 2));
            int AY0 =
                jntuh_academic_years.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();


            //CHANDU: This is actual format before 19-11-2014
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
            int enclosureId =
                db.jntuh_enclosures.Where(e => e.documentName == "AICTE INTAKE APPROVAL LETTER")
                    .Select(e => e.id)
                    .FirstOrDefault();
            var AICTEApprovalLettr =
                db.jntuh_college_enclosures.Where(
                    e => e.enclosureId == enclosureId && e.collegeID == userCollegeID && e.academicyearId == AY0)
                    .OrderByDescending(a => a.id)
                    .Select(e => e.path)
                    .FirstOrDefault();
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
                newIntake.id = item.id;
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
                newIntake.AICTEApprovalLettr = AICTEApprovalLettr;
                int collegecid = jntuh_college_noc_data.Where(d => d.noctypeId == 9)
                    .Select(s => s.id)
                    .FirstOrDefault();
                if (collegecid == 0)
                {
                    int closedid =
                        jntuh_college_noc_data.Where(
                            d => d.specializationId == item.specializationId && d.shiftId == item.shiftId && d.remarks == null)
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


            //var jntuh_college_intake_existing = db.jntuh_college_intake_existing.AsNoTracking().ToList();

            //collegeIntakeExisting =
            //    collegeIntakeExisting.AsEnumerable()
            //        .GroupBy(r => new { r.specializationId, r.shiftId })
            //        .Select(r => r.First())
            //        .ToList();
            //foreach (var item in collegeIntakeExisting)
            //{

            //    if (item.nbaFrom != null)
            //        item.nbaFromDate = Utilities.MMDDYY2DDMMYY(item.nbaFrom.ToString());
            //    if (item.nbaTo != null)
            //        item.nbaToDate = Utilities.MMDDYY2DDMMYY(item.nbaTo.ToString());
            //    jntuh_college_intake_existing nbadatedetails = db.jntuh_college_intake_existing
            //        .Where(
            //            e =>
            //                e.collegeId == userCollegeID && e.academicYearId == AY1 &&
            //                e.specializationId == item.specializationId && e.shiftId == item.shiftId)
            //        .Select(e => e)
            //        .FirstOrDefault();
            //    if (nbadatedetails != null)
            //    {
            //        if (nbadatedetails.nbaFrom != null)
            //            item.nbaFromDate = Utilities.MMDDYY2DDMMYY(nbadatedetails.nbaFrom.ToString());
            //        if (nbadatedetails.nbaTo != null)
            //            item.nbaToDate = Utilities.MMDDYY2DDMMYY(nbadatedetails.nbaTo.ToString());
            //    }

            //    //FLAG :4-Admitted Intake Lateral as per Exam Branch,3-Admitted Intake Regular as per Exam Branch,2-AICTE Approved 1 - Approved, 0 - Admitted
            //    jntuh_college_intake_existing details = jntuh_college_intake_existing
            //        .Where(
            //            e =>
            //                e.collegeId == userCollegeID && e.academicYearId == AY0 &&
            //                e.specializationId == item.specializationId && e.shiftId == item.shiftId)
            //        .Select(e => e)
            //        .FirstOrDefault();
            //    if (details != null)
            //    {
            //        item.ApprovedIntake = details.approvedIntake;
            //        item.letterPath = details.approvalLetter;
            //        item.ProposedIntake = details.proposedIntake;
            //        item.courseStatus = details.courseStatus;
            //    }

            //    //if (item.isClosed)
            //    //{
            //    //    item.ProposedIntake = 0;
            //    //    item.courseStatus = "Closure";
            //    //}

            //    //item.ExambranchadmittedIntakeL1 = GetIntake(userCollegeID, AY1, item.specializationId, item.shiftId, 4);
            //    //item.ExambranchadmittedIntakeR1 = GetIntake(userCollegeID, AY1, item.specializationId, item.shiftId,3);
            //    item.AICTEapprovedIntake1 = GetIntake(userCollegeID, AY1, item.specializationId, item.shiftId, 2);
            //    item.approvedIntake1 = GetIntake(userCollegeID, AY1, item.specializationId, item.shiftId, 1);
            //    item.admittedIntake1 = GetIntake(userCollegeID, AY1, item.specializationId, item.shiftId, 0);

            //    //item.ExambranchadmittedIntakeL2 = GetIntake(userCollegeID, AY1, item.specializationId, item.shiftId, 4);
            //    //item.ExambranchadmittedIntakeR2 = GetIntake(userCollegeID, AY1, item.specializationId, item.shiftId, 3);
            //    item.AICTEapprovedIntake2 = GetIntake(userCollegeID, AY2, item.specializationId, item.shiftId, 2);
            //    item.approvedIntake2 = GetIntake(userCollegeID, AY2, item.specializationId, item.shiftId, 1);
            //    item.admittedIntake2 = GetIntake(userCollegeID, AY2, item.specializationId, item.shiftId, 0);

            //    //item.ExambranchadmittedIntakeL3 = GetIntake(userCollegeID, AY1, item.specializationId, item.shiftId, 4);
            //    //item.ExambranchadmittedIntakeR3 = GetIntake(userCollegeID, AY1, item.specializationId, item.shiftId, 3);
            //    item.AICTEapprovedIntake3 = GetIntake(userCollegeID, AY3, item.specializationId, item.shiftId, 2);
            //    item.approvedIntake3 = GetIntake(userCollegeID, AY3, item.specializationId, item.shiftId, 1);
            //    item.admittedIntake3 = GetIntake(userCollegeID, AY3, item.specializationId, item.shiftId, 0);

            //    //item.ExambranchadmittedIntakeL4 = GetIntake(userCollegeID, AY1, item.specializationId, item.shiftId, 4);
            //    //item.ExambranchadmittedIntakeR4 = GetIntake(userCollegeID, AY1, item.specializationId, item.shiftId, 3);
            //    item.AICTEapprovedIntake4 = GetIntake(userCollegeID, AY4, item.specializationId, item.shiftId, 2);
            //    item.approvedIntake4 = GetIntake(userCollegeID, AY4, item.specializationId, item.shiftId, 1);
            //    item.admittedIntake4 = GetIntake(userCollegeID, AY4, item.specializationId, item.shiftId, 0);

            //    //item.ExambranchadmittedIntakeL5 = GetIntake(userCollegeID, AY1, item.specializationId, item.shiftId, 4);
            //    //item.ExambranchadmittedIntakeR5 = GetIntake(userCollegeID, AY1, item.specializationId, item.shiftId, 3);
            //    item.AICTEapprovedIntake5 = GetIntake(userCollegeID, AY5, item.specializationId, item.shiftId, 2);
            //    item.approvedIntake5 = GetIntake(userCollegeID, AY5, item.specializationId, item.shiftId, 1);
            //    item.admittedIntake5 = GetIntake(userCollegeID, AY5, item.specializationId, item.shiftId, 0);

            //}


            //collegeIntakeExisting =
            //    collegeIntakeExisting.OrderBy(ei => ei.degreeDisplayOrder)
            //        .ThenBy(ei => ei.Department)
            //        .ThenBy(ei => ei.Specialization)
            //        .ThenBy(ei => ei.shiftId)
            //        .ToList();
            //ViewBag.ExistingIntake = collegeIntakeExisting;
            //ViewBag.Count = collegeIntakeExisting.Count();
            //string clgCode = db.jntuh_college.SingleOrDefault(a => a.id == userCollegeID).collegeCode;
            //var currentYear = DateTime.Now.Year;
            //ViewBag.IsPaymentDone =
            //    db.jntuh_paymentresponse.Count(
            //        it =>
            //            it.CollegeId == clgCode && it.AcademicYearId == AY0 && it.TxnDate.Year == currentYear &&
            //            it.AuthStatus == "0300" && it.PaymentTypeID == 7) > 0;
            return View(collegeIntakeExisting);

        }

        [Authorize(Roles = "Admin,College")]
        public ActionResult FileUploadAICTEPCI(HttpPostedFileBase fileUploader, string collegeId)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID =
                db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            int actualYear =
                db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true)
                    .Select(a => a.actualYear)
                    .FirstOrDefault();
            int ay0 =
                db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();
            if (userCollegeID == 0)
            {
                userCollegeID = Convert.ToInt32(collegeId);
            }
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            string fileName = string.Empty;
            int presentAY =
                db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true)
                    .Select(a => a.actualYear)
                    .FirstOrDefault();
            int academicyearId =
                db.jntuh_academic_year.Where(a => a.actualYear == (presentAY + 1))
                    .Select(a => a.actualYear)
                    .FirstOrDefault();
            int enclosureId =
                db.jntuh_enclosures.Where(e => e.documentName == "AICTE INTAKE APPROVAL LETTER")
                    .Select(e => e.id)
                    .FirstOrDefault();
            var college_enclosures =
                db.jntuh_college_enclosures.Where(
                    e => e.enclosureId == enclosureId && e.academicyearId == ay0 && e.collegeID == userCollegeID)
                    .Select(e => e)
                    .FirstOrDefault();
            jntuh_college_enclosures jntuh_college_enclosures = new jntuh_college_enclosures();
            jntuh_college_enclosures.collegeID = userCollegeID;
            jntuh_college_enclosures.academicyearId = ay0;
            jntuh_college_enclosures.enclosureId = enclosureId;
            jntuh_college_enclosures.isActive = true;
            if (fileUploader != null)
            {
                string ext = Path.GetExtension(fileUploader.FileName);
                //DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" + faculty.FirstName.Substring(0, 1) + "-" + faculty.LastName.Substring(0, 1)
                fileName =
                    db.jntuh_college.Where(c => c.id == userCollegeID).Select(c => c.collegeCode).FirstOrDefault() +
                    "_APL_" + enclosureId + "-" + DateTime.Now.ToString("yyyyMMdd-HHmmss") + ext;
                fileUploader.SaveAs(string.Format("{0}/{1}", Server.MapPath("~/Content/Upload/CollegeEnclosures/"),
                    fileName));
                jntuh_college_enclosures.path = fileName;
            }
            else if (!string.IsNullOrEmpty(college_enclosures.path))
            {
                fileName = college_enclosures.path;
                jntuh_college_enclosures.path = fileName;
            }

            if (college_enclosures == null)
            {
                jntuh_college_enclosures.createdBy = userID;
                jntuh_college_enclosures.createdOn = DateTime.Now;
                db.jntuh_college_enclosures.Add(jntuh_college_enclosures);
                db.SaveChanges();
                TempData["Success"] = "Saved successfully.";
            }
            else
            {
                college_enclosures.path = fileName;
                college_enclosures.updatedBy = userID;
                college_enclosures.updatedOn = DateTime.Now;
                db.Entry(college_enclosures).State = EntityState.Modified;
                db.SaveChanges();
                TempData["Success"] = "Saved successfully.";
            }
            return RedirectToAction("UploadAICTEFileUploadIndex",
                new
                {
                    collegeId =
                        Utilities.EncryptString(userCollegeID.ToString(),
                            WebConfigurationManager.AppSettings["CryptoKey"]).ToString()
                });

        }
    }
}
