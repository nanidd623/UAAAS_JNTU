using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Security;
using UAAAS.Models;

namespace UAAAS.Controllers.College
{
    [ErrorHandling]
    public class CollegeNBAAccreditedController : BaseController
    {
        //
        // GET: /CollegeNBAAccredited/
        uaaasDBContext db = new uaaasDBContext();

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult Index(string collegeId)
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
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            var jntuh_academic_year = db.jntuh_academic_year.Select(s => s).ToList();
            int actualYear = jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int ay0 = jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();
            //int lid =
            //    db.jntuh_link_screens.Where(p => p.linkName == "College Course Add and Intake Increase" && p.isActive == true).Select(s => s.id).FirstOrDefault();
            //jntuh_college_links_assigned scmphase =
            //    db.jntuh_college_links_assigned.Where(l => l.linkId == lid && l.academicyearId == ay0 && l.isActive == true && l.fromdate <= todayDate && l.todate >= todayDate).Select(s => s).FirstOrDefault();

            //if (scmphase == null)
            //{
            //    return RedirectToAction("College", "Dashboard");
            //}

            int status = db.jntuh_college_edit_status.Where(editStatus => editStatus.academicyearId == ay0 && editStatus.collegeId == userCollegeID &&
                                                                                                editStatus.IsCollegeEditable == true &&
                                                                                                editStatus.editFromDate <= todayDate &&
                                                                                                editStatus.editToDate >= todayDate)
                                                                           .Select(editStatus => editStatus.id)
                                                                           .FirstOrDefault();
            //if (status == 0)
            ////if (false)
            //{
            //    return RedirectToAction("View", "CollegeNBAAccredited");
            //}
            List<jntuh_academic_year> jntuh_academic_years = db.jntuh_academic_year.AsNoTracking().ToList();
            //ViewBag.collegeId =
            //    UAAAS.Models.Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"])
            //        .ToString();
            //ViewBag.AcademicYear =
            //    jntuh_academic_years.Where(a => a.isActive == true && a.isPresentAcademicYear == true)
            //        .Select(a => a.academicYear)
            //        .FirstOrDefault();

            //ViewBag.NextYear = String.Format("{0}-{1}", (actualYear + 1).ToString(),
            //    (actualYear + 2).ToString().Substring(2, 2));
            int AY0 =
                jntuh_academic_years.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();



            //ViewBag.FirstYear = String.Format("{0}-{1}", (actualYear).ToString(),
            //    (actualYear + 1).ToString().Substring(2, 2));
            //ViewBag.SecondYear = String.Format("{0}-{1}", (actualYear - 1).ToString(),
            //    (actualYear).ToString().Substring(2, 2));
            //ViewBag.ThirdYear = String.Format("{0}-{1}", (actualYear - 2).ToString(),
            //    (actualYear - 1).ToString().Substring(2, 2));
            //ViewBag.FourthYear = String.Format("{0}-{1}", (actualYear - 3).ToString(),
            //    (actualYear - 2).ToString().Substring(2, 2));
            //ViewBag.FifthYear = String.Format("{0}-{1}", (actualYear - 4).ToString(),
            //    (actualYear - 3).ToString().Substring(2, 2));

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
                    i => i.collegeId == userCollegeID && !inactivespids.Contains(i.specializationId) && (i.academicYearId == ay0 || i.academicYearId == AY1 || i.academicYearId == AY2 || i.academicYearId == AY3) && (i.approvedIntake != 0 || i.aicteApprovedIntake != 0)).ToList();
            var nbaaccrdata = db.jntuh_college_nbaaccreditationdata.AsNoTracking().Where(i => i.collegeid == userCollegeID).ToList();
            List<CollegeIntakeExisting> collegeIntakeExisting = new List<CollegeIntakeExisting>();

            var jntuh_specialization = db.jntuh_specialization;
            var jntuh_department = db.jntuh_department;
            var jntuh_degree = db.jntuh_degree;
            //var jntuh_shift = db.jntuh_shift;
            //var jntuh_college_noc_data =
            //    db.jntuh_college_noc_data.Where(n => n.collegeId == userCollegeID && n.isClosure == true)
            //        .Select(s => s)
            //        .ToList();
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
                newIntake.enccollegeid = UAAAS.Models.Utilities.EncryptString(item.collegeId.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString();
                newIntake.academicYearId = item.academicYearId;
                //newIntake.shiftId = item.shiftId;
                newIntake.isActive = item.isActive;
                //newIntake.nbaFrom = item.nbaFrom;
                //newIntake.nbaTo = item.nbaTo;
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
                //newIntake.shiftId = item.shiftId;

                var nbadt = nbaaccrdata.Where(i => i.specealizationid == item.specializationId && i.accademicyear == AY0).FirstOrDefault();
                if (nbadt != null)
                {
                    newIntake.nbaid = UAAAS.Models.Utilities.EncryptString(nbadt.sno.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString();
                    if (nbadt.nbafrom != null)
                        //item.nbaFromDate = Utilities.MMDDYY2DDMMYY(nbadt.nbafrom.ToString());
                        newIntake.nbaFromDate = nbadt.nbafrom.ToString("dd/MM/yyyy");
                    if (nbadt.nbato != null)
                        //item.nbaToDate = Utilities.MMDDYY2DDMMYY(nbadt.nbato.ToString());
                        newIntake.nbaToDate = nbadt.nbato.ToString("dd/MM/yyyy");
                    newIntake.UploadNBAApproveLetter = nbadt.nbaapprovalletter;
                }
                //newIntake.Shift = jntuh_shift.Where(s => s.id == item.shiftId).Select(s => s.shiftName).FirstOrDefault();
                //newIntake.UploadNBAApproveLetter = item.NBAApproveLetter;
                //int collegecid = jntuh_college_noc_data.Where(d => d.noctypeId == 15)
                //    .Select(s => s.id)
                //    .FirstOrDefault();
                //if (collegecid == 0)
                //{
                //    int closedid =
                //        jntuh_college_noc_data.Where(
                //            d => d.specializationId == item.specializationId && d.shiftId == item.shiftId && d.noctypeId == 16)
                //            .Select(s => s.id)
                //            .FirstOrDefault();
                //    if (closedid != 0)
                //    {
                //        newIntake.isClosed = true;
                //    }
                //}
                //else
                //{
                //    newIntake.isClosed = true;
                //}

                collegeIntakeExisting.Add(newIntake);
            }
            //var jntuh_college_intake_existing = db.jntuh_college_intake_existing.Where(r => r.collegeId == userCollegeID).Select(s => s).ToList();

            //collegeIntakeExisting =
            //    collegeIntakeExisting.AsEnumerable()
            //        .GroupBy(r => new { r.specializationId, r.shiftId })
            //        .Select(r => r.First())
            //        .ToList();
            //foreach (var item in collegeIntakeExisting)
            //{
            //    var nbadt = nbaaccrdata.Where(i => i.specealizationid == item.specializationId && i.accademicyear == item.academicYearId).FirstOrDefault();
            //    if (nbadt != null)
            //    {
            //        if (nbadt.nbafrom != null)
            //            //item.nbaFromDate = Utilities.MMDDYY2DDMMYY(nbadt.nbafrom.ToString());
            //            item.nbaFromDate = nbadt.nbafrom.ToString("dd/MM/yyyy");
            //        if (nbadt.nbato != null)
            //            //item.nbaToDate = Utilities.MMDDYY2DDMMYY(nbadt.nbato.ToString());
            //            item.nbaToDate = nbadt.nbato.ToString("dd/MM/yyyy");
            //        item.UploadNBAApproveLetter = nbadt.nbaapprovalletter;
            //    }

            //    //jntuh_college_intake_existing nbadatedetails = db.jntuh_college_intake_existing
            //    //    .Where(
            //    //        e =>
            //    //            e.collegeId == userCollegeID && e.academicYearId == AY1 &&
            //    //            e.specializationId == item.specializationId && e.shiftId == item.shiftId)
            //    //    .Select(e => e)
            //    //    .FirstOrDefault();
            //    //if (nbadatedetails != null)
            //    //{
            //    //    if (nbadatedetails.nbaFrom != null)
            //    //        item.nbaFromDate = Utilities.MMDDYY2DDMMYY(nbadatedetails.nbaFrom.ToString());
            //    //    if (nbadatedetails.nbaTo != null)
            //    //        item.nbaToDate = Utilities.MMDDYY2DDMMYY(nbadatedetails.nbaTo.ToString());
            //    //}
            //}

            collegeIntakeExisting =
                collegeIntakeExisting.AsEnumerable()
                //.GroupBy(r => new { r.specializationId, r.shiftId })
                    .GroupBy(r => new { r.specializationId })
                    .Select(r => r.First())
                    .ToList();

            collegeIntakeExisting =
                collegeIntakeExisting.Where(i => i.degreeID != 7 && i.degreeID != 8 && i.degreeID != 9 && i.degreeID != 10).OrderBy(ei => ei.degreeDisplayOrder)
                    .ThenBy(ei => ei.Department)
                    .ThenBy(ei => ei.Specialization)
                //.ThenBy(ei => ei.shiftId)
                    .ToList();
            ViewBag.ExistingIntake = collegeIntakeExisting;
            ViewBag.Count = collegeIntakeExisting.Count();
            string clgCode = db.jntuh_college.SingleOrDefault(a => a.id == userCollegeID).collegeCode;
            var currentYear = DateTime.Now.Year;
            ViewBag.IsPaymentDone =
                db.jntuh_paymentresponse.Count(
                    it =>
                        it.CollegeId == clgCode && it.AcademicYearId == AY0 && it.TxnDate.Year == currentYear &&
                        it.AuthStatus == "0300") > 0;
            return View(collegeIntakeExisting);
        }

        [Authorize(Roles = "Admin,College")]
        public ActionResult View(string id)
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
            if (userCollegeID == 0)
            {
                userCollegeID = Convert.ToInt32(Utilities.DecryptString(id, System.Web.Configuration.WebConfigurationManager.AppSettings["CryptoKey"]));
            }
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            var jntuh_academic_year = db.jntuh_academic_year.Select(s => s).ToList();
            int actualYear = jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int ay0 = jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();

            //int status = db.jntuh_college_edit_status.Where(editStatus => editStatus.academicyearId == ay0 && editStatus.collegeId == userCollegeID &&
            //                                                                                    editStatus.IsCollegeEditable == true &&
            //                                                                                    editStatus.editFromDate <= todayDate &&
            //                                                                                    editStatus.editToDate >= todayDate)
            //                                                               .Select(editStatus => editStatus.id)
            //                                                               .FirstOrDefault();

            List<jntuh_academic_year> jntuh_academic_years = db.jntuh_academic_year.AsNoTracking().ToList();
            //ViewBag.collegeId =
            //    UAAAS.Models.Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"])
            //        .ToString();
            //ViewBag.AcademicYear =
            //    jntuh_academic_years.Where(a => a.isActive == true && a.isPresentAcademicYear == true)
            //        .Select(a => a.academicYear)
            //        .FirstOrDefault();

            //ViewBag.NextYear = String.Format("{0}-{1}", (actualYear + 1).ToString(),
            //    (actualYear + 2).ToString().Substring(2, 2));
            int AY0 =
                jntuh_academic_years.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();



            //ViewBag.FirstYear = String.Format("{0}-{1}", (actualYear).ToString(),
            //    (actualYear + 1).ToString().Substring(2, 2));
            //ViewBag.SecondYear = String.Format("{0}-{1}", (actualYear - 1).ToString(),
            //    (actualYear).ToString().Substring(2, 2));
            //ViewBag.ThirdYear = String.Format("{0}-{1}", (actualYear - 2).ToString(),
            //    (actualYear - 1).ToString().Substring(2, 2));
            //ViewBag.FourthYear = String.Format("{0}-{1}", (actualYear - 3).ToString(),
            //    (actualYear - 2).ToString().Substring(2, 2));
            //ViewBag.FifthYear = String.Format("{0}-{1}", (actualYear - 4).ToString(),
            //    (actualYear - 3).ToString().Substring(2, 2));

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

            int[] inactivespids = db.jntuh_specialization.Where(s => s.isActive == false).Select(s => s.id).ToArray();

            List<jntuh_college_intake_existing> intake =
                db.jntuh_college_intake_existing.Where(
                    i => i.collegeId == userCollegeID && !inactivespids.Contains(i.specializationId) && (i.academicYearId == ay0 || i.academicYearId == AY1 || i.academicYearId == AY2 || i.academicYearId == AY3) && (i.approvedIntake != 0 || i.aicteApprovedIntake != 0)).ToList();
            var nbaaccrdata = db.jntuh_college_nbaaccreditationdata.AsNoTracking().Where(i => i.collegeid == userCollegeID).ToList();

            List<CollegeIntakeExisting> collegeIntakeExisting = new List<CollegeIntakeExisting>();

            var jntuh_specialization = db.jntuh_specialization;
            var jntuh_department = db.jntuh_department;
            var jntuh_degree = db.jntuh_degree;
            //var jntuh_shift = db.jntuh_shift;
            //var jntuh_college_noc_data =
            //    db.jntuh_college_noc_data.Where(n => n.collegeId == userCollegeID && n.isClosure == true)
            //        .Select(s => s)
            //        .ToList();
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
                newIntake.enccollegeid = UAAAS.Models.Utilities.EncryptString(item.collegeId.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString();
                newIntake.academicYearId = item.academicYearId;
                //newIntake.shiftId = item.shiftId;
                newIntake.isActive = item.isActive;
                //newIntake.nbaFrom = item.nbaFrom;
                ///newIntake.nbaTo = item.nbaTo;
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
                //newIntake.shiftId = item.shiftId;

                var nbadt = nbaaccrdata.Where(i => i.specealizationid == item.specializationId && i.accademicyear == AY0).FirstOrDefault();

                if (nbadt != null)
                {
                    newIntake.nbaid = UAAAS.Models.Utilities.EncryptString(nbadt.sno.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString();
                    if (nbadt.nbafrom != null)
                        //item.nbaFromDate = Utilities.MMDDYY2DDMMYY(nbadt.nbafrom.ToString("dd/MM/yyyy"));
                        newIntake.nbaFromDate = nbadt.nbafrom.ToString("dd/MM/yyyy");
                    if (nbadt.nbato != null)
                        //item.nbaToDate = Utilities.MMDDYY2DDMMYY(nbadt.nbato.ToString("dd/MM/yyyy"));
                        newIntake.nbaToDate = nbadt.nbato.ToString("dd/MM/yyyy");
                    newIntake.UploadNBAApproveLetter = nbadt.nbaapprovalletter;
                }
                //newIntake.Shift = jntuh_shift.Where(s => s.id == item.shiftId).Select(s => s.shiftName).FirstOrDefault();
                ////newIntake.UploadNBAApproveLetter = item.NBAApproveLetter;
                //int collegecid = jntuh_college_noc_data.Where(d => d.noctypeId == 15)
                //    .Select(s => s.id)
                //    .FirstOrDefault();
                //if (collegecid == 0)
                //{
                //    int closedid =
                //        jntuh_college_noc_data.Where(
                //            d => d.specializationId == item.specializationId && d.shiftId == item.shiftId && d.noctypeId == 16)
                //            .Select(s => s.id)
                //            .FirstOrDefault();
                //    if (closedid != 0)
                //    {
                //        newIntake.isClosed = true;
                //    }
                //}
                //else
                //{
                //    newIntake.isClosed = true;
                //}
                //if (newIntake.isClosed == false)
                //{
                //    collegeIntakeExisting.Add(newIntake);
                //}

                collegeIntakeExisting.Add(newIntake);
            }
            //var jntuh_college_intake_existing = db.jntuh_college_intake_existing.Where(r => r.collegeId == userCollegeID).Select(s => s).ToList();

            //collegeIntakeExisting =
            //    collegeIntakeExisting.AsEnumerable()
            //        .GroupBy(r => new { r.specializationId, r.shiftId })
            //        .Select(r => r.First())
            //        .ToList();
            //foreach (var item in collegeIntakeExisting)
            //{
            //    var nbadt = nbaaccrdata.Where(i => i.specealizationid == item.specializationId && i.accademicyear == item.academicYearId).FirstOrDefault();

            //    if (nbadt != null)
            //    {
            //        if (nbadt.nbafrom != null)
            //            //item.nbaFromDate = Utilities.MMDDYY2DDMMYY(nbadt.nbafrom.ToString("dd/MM/yyyy"));
            //            item.nbaFromDate = nbadt.nbafrom.ToString("dd/MM/yyyy");
            //        if (nbadt.nbato != null)
            //            //item.nbaToDate = Utilities.MMDDYY2DDMMYY(nbadt.nbato.ToString("dd/MM/yyyy"));
            //            item.nbaToDate = nbadt.nbato.ToString("dd/MM/yyyy");
            //        item.UploadNBAApproveLetter = nbadt.nbaapprovalletter;
            //    }

            //    //jntuh_college_intake_existing nbadatedetails = db.jntuh_college_intake_existing
            //    //    .Where(
            //    //        e =>
            //    //            e.collegeId == userCollegeID && e.academicYearId == AY1 &&
            //    //            e.specializationId == item.specializationId && e.shiftId == item.shiftId)
            //    //    .Select(e => e)
            //    //    .FirstOrDefault();
            //    //if (nbadatedetails != null)
            //    //{
            //    //    if (nbadatedetails.nbaFrom != null)
            //    //        item.nbaFromDate = Utilities.MMDDYY2DDMMYY(nbadatedetails.nbaFrom.ToString());
            //    //    if (nbadatedetails.nbaTo != null)
            //    //        item.nbaToDate = Utilities.MMDDYY2DDMMYY(nbadatedetails.nbaTo.ToString());
            //    //}
            //}

            collegeIntakeExisting =
                collegeIntakeExisting.AsEnumerable()
                //.GroupBy(r => new { r.specializationId, r.shiftId })
                    .GroupBy(r => new { r.specializationId })
                    .Select(r => r.First())
                    .ToList();

            collegeIntakeExisting =
                collegeIntakeExisting.Where(i => i.degreeID != 7 && i.degreeID != 8 && i.degreeID != 9 && i.degreeID != 10).OrderBy(ei => ei.degreeDisplayOrder)
                    .ThenBy(ei => ei.Department)
                    .ThenBy(ei => ei.Specialization)
                //.ThenBy(ei => ei.shiftId)
                    .ToList();
            ViewBag.ExistingIntake = collegeIntakeExisting;
            ViewBag.Count = collegeIntakeExisting.Count();
            string clgCode = db.jntuh_college.SingleOrDefault(a => a.id == userCollegeID).collegeCode;
            var currentYear = DateTime.Now.Year;
            ViewBag.IsPaymentDone =
                db.jntuh_paymentresponse.Count(
                    it =>
                        it.CollegeId == clgCode && it.AcademicYearId == AY0 && it.TxnDate.Year == currentYear &&
                        it.AuthStatus == "0300") > 0;
            return View(collegeIntakeExisting);
        }

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

            //ViewBag.AcademicYear =
            //    db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true)
            //        .Select(a => a.academicYear)
            //        .FirstOrDefault();
            int actualYear =
                db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true)
                    .Select(a => a.actualYear)
                    .FirstOrDefault();

            //RAMESH: ADDED to MERGE BOTH EXISTING & PROPOSED INTAKE
            //ViewBag.NextYear = String.Format("{0}-{1}", (actualYear + 1).ToString(),
            //    (actualYear + 2).ToString().Substring(2, 2));
            //ViewBag.PrevYear = String.Format("{0}-{1}", (actualYear).ToString(),
            //    (actualYear + 1).ToString().Substring(2, 2));
            int AY0 =
                db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();

            //ViewBag.FirstYear = String.Format("{0}-{1}", (actualYear).ToString(),
            //    (actualYear + 1).ToString().Substring(2, 2));
            //ViewBag.SecondYear = String.Format("{0}-{1}", (actualYear - 1).ToString(),
            //    (actualYear).ToString().Substring(2, 2));
            //ViewBag.ThirdYear = String.Format("{0}-{1}", (actualYear - 2).ToString(),
            //    (actualYear - 1).ToString().Substring(2, 2));
            //ViewBag.FourthYear = String.Format("{0}-{1}", (actualYear - 3).ToString(),
            //    (actualYear - 2).ToString().Substring(2, 2));
            //ViewBag.FifthYear = String.Format("{0}-{1}", (actualYear - 4).ToString(),
            //    (actualYear - 3).ToString().Substring(2, 2));

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
                    collegeIntakeExisting.academicYearId = AY0;
                    //collegeIntakeExisting.shiftId = item.shiftId;
                    collegeIntakeExisting.isActive = item.isActive;
                    //collegeIntakeExisting.nbaFrom = item.nbaFrom;
                    //collegeIntakeExisting.nbaTo = item.nbaTo;
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
                    //collegeIntakeExisting.shiftId = item.shiftId;
                    //collegeIntakeExisting.courseStatus = item.courseStatus;
                    //collegeIntakeExisting.courseStatus =
                    //    db.jntuh_college_intake_existing.Where(
                    //        i =>
                    //            i.collegeId == userCollegeID && i.specializationId == item.specializationId &&
                    //            i.academicYearId == AY0).Select(s => s.courseStatus).FirstOrDefault() == null
                    //        ? "0"
                    //        : db.jntuh_college_intake_existing.Where(
                    //            i =>
                    //                i.collegeId == userCollegeID && i.specializationId == item.specializationId &&
                    //                i.academicYearId == AY0).Select(s => s.courseStatus).FirstOrDefault();


                    //collegeIntakeExisting.courseAffiliatedStatus = item.courseAffiliatedStatus;
                    //collegeIntakeExisting.Shift =
                    //    db.jntuh_shift.Where(s => s.id == item.shiftId).Select(s => s.shiftName).FirstOrDefault();
                    ViewBag.NBAApprovedLetter = string.Empty;
                    //var nbaaccrdata = db.jntuh_college_nbaaccreditationdata.AsNoTracking().Where(i => i.collegeid == userCollegeID && i.specealizationid == item.specializationId && i.accademicyear == AY0).ToList();
                    var nbaaccrdata = db.jntuh_college_nbaaccreditationdata.AsNoTracking().Where(i => i.collegeid == item.collegeId).ToList();
                    var nbadt = nbaaccrdata.Where(i => i.specealizationid == item.specializationId && i.accademicyear == AY0).FirstOrDefault();
                    if (nbadt != null)
                    {
                        if (nbadt.nbafrom != null)
                            //collegeIntakeExisting.nbaFromDate = Utilities.MMDDYY2DDMMYY(nbadt.nbafrom.ToString());
                            collegeIntakeExisting.nbaFromDate = nbadt.nbafrom.ToString("dd/MM/yyyy");
                        if (nbadt.nbato != null)
                            //collegeIntakeExisting.nbaToDate = Utilities.MMDDYY2DDMMYY(nbadt.nbato.ToString());
                            collegeIntakeExisting.nbaToDate = nbadt.nbato.ToString("dd/MM/yyyy");
                        collegeIntakeExisting.UploadNBAApproveLetter = nbadt.nbaapprovalletter;
                        ViewBag.NBAApprovedLetter = nbadt.nbaapprovalletter;
                    }
                }

                //if (collegeIntakeExisting.nbaFrom != null)
                //    collegeIntakeExisting.nbaFromDate = Utilities.MMDDYY2DDMMYY(collegeIntakeExisting.nbaFrom.ToString());
                //if (collegeIntakeExisting.nbaTo != null)
                //    collegeIntakeExisting.nbaToDate = Utilities.MMDDYY2DDMMYY(collegeIntakeExisting.nbaTo.ToString());

                //jntuh_college_intake_existing nbadatedetails = db.jntuh_college_intake_existing
                //    .Where(
                //        e =>
                //            e.collegeId == userCollegeID && e.academicYearId == AY1 &&
                //            e.specializationId == collegeIntakeExisting.specializationId &&
                //            e.shiftId == collegeIntakeExisting.shiftId)
                //    .Select(e => e)
                //    .FirstOrDefault();
                //if (nbadatedetails != null)
                //{
                //    if (nbadatedetails.nbaFrom != null)
                //        collegeIntakeExisting.nbaFromDate = Utilities.MMDDYY2DDMMYY(nbadatedetails.nbaFrom.ToString());
                //    if (nbadatedetails.nbaTo != null)
                //        collegeIntakeExisting.nbaToDate = Utilities.MMDDYY2DDMMYY(nbadatedetails.nbaTo.ToString());
                //}
            }
            else
            {
                ViewBag.IsUpdate = false;
            }

            //List<SelectListItem> courseStatuslist = new List<SelectListItem>();
            ////courseStatuslist.Add(new SelectListItem { Value = "0", Text = "-Select-" });
            //courseStatuslist.Add(new SelectListItem { Value = "Decrease", Text = "Decrease" });
            //courseStatuslist.Add(new SelectListItem { Value = "Nochange", Text = "No Change" });
            //courseStatuslist.Add(new SelectListItem { Value = "Closure", Text = "Closure" });
            //ViewBag.courseStatusdata = courseStatuslist;
            return PartialView("_Create", collegeIntakeExisting);
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
            //    if ((collegeIntakeExisting.AICTEapprovedIntake1 == 0 || collegeIntakeExisting.AICTEapprovedIntake2 == 0 ||
            //         collegeIntakeExisting.AICTEapprovedIntake3 == 0) && collegeIntakeExisting.courseStatus != "Closure")
            //    {
            //        TempData["Error"] = "AICTE Sanctioned Intake Should not be Zero";
            //        return RedirectToAction("Index",
            //            new
            //            {
            //                collegeId =
            //                    Utilities.EncryptString(userCollegeID.ToString(),
            //                        WebConfigurationManager.AppSettings["CryptoKey"]).ToString()
            //            });

            //    }
            //}
            //else
            //{
            //    if ((collegeIntakeExisting.AICTEapprovedIntake1 == 0) && collegeIntakeExisting.courseStatus != "Closure")
            //    {
            //        TempData["Error"] = "AICTE Sanctioned Intake Should not be Zero";
            //        return RedirectToAction("Index",
            //            new
            //            {
            //                collegeId =
            //                    Utilities.EncryptString(userCollegeID.ToString(),
            //                        WebConfigurationManager.AppSettings["CryptoKey"]).ToString()
            //            });

            //    }
            //}
            List<jntuh_college_intake_existing> jntuh_college_intake_existinglist =
                new List<jntuh_college_intake_existing>();
            //ViewBag.Degree =
            //    db.jntuh_college_degree.Join(db.jntuh_degree, collegeDegree => collegeDegree.degreeId,
            //        degree => degree.id,
            //        (collegeDegree, degree) => new
            //        {
            //            collegeDegree.degreeId,
            //            collegeDegree.collegeId,
            //            collegeDegree.isActive,
            //            degree.degree
            //        })
            //        .Where(collegeDegree => collegeDegree.collegeId == userCollegeID && collegeDegree.isActive == true)
            //        .Select(collegeDegree => new
            //        {
            //            collegeDegree.degreeId,
            //            collegeDegree.degree
            //        }).OrderBy(d => d.degree).ToList();

            //ViewBag.Department = db.jntuh_department.Where(d => d.isActive == true);
            //ViewBag.Specialization = db.jntuh_specialization.Where(s => s.isActive == true);
            //ViewBag.Shift = db.jntuh_shift.Where(s => s.isActive == true);
            //ViewBag.CourseAffiliationstatus = db.jntuh_course_affiliation_status.Where(c => c.isActive == true);

            //if (collegeIntakeExisting.nbaFromDate != null)
            //    collegeIntakeExisting.nbaFrom =
            //        Convert.ToDateTime(Utilities.DDMMYY2MMDDYY(collegeIntakeExisting.nbaFromDate));
            //if (collegeIntakeExisting.nbaToDate != null)
            //    collegeIntakeExisting.nbaTo =
            //        Convert.ToDateTime(Utilities.DDMMYY2MMDDYY(collegeIntakeExisting.nbaToDate));
            string nbafilename = string.Empty;
            if (ModelState.IsValid)
            {
                collegeIntakeExisting.collegeId = userCollegeID;
                var jntuhcollege = db.jntuh_college.Where(c => c.id == userCollegeID).Select(s => s).FirstOrDefault();
                var jntuh_academic_year = db.jntuh_academic_year.Select(s => s).ToList();

                int actualYear = jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();

                int AY0 =
                jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();
                //File Saving Writen By Narayana
                if (collegeIntakeExisting.NBAApproveLetter != null)
                {
                    if (!Directory.Exists(Server.MapPath("~/Content/Upload/College/NBAAccredited_Latest")))
                    {
                        Directory.CreateDirectory(Server.MapPath("~/Content/Upload/College/NBAAccredited_Latest"));
                    }

                    var ext = Path.GetExtension(collegeIntakeExisting.NBAApproveLetter.FileName);
                    if (ext.ToUpper().Equals(".PDF") || ext.ToUpper().Equals(".pdf"))
                    {
                        var fileName = jntuhcollege.collegeCode.Trim() + "-NBA-" +
                                       DateTime.Now.ToString("yyyyMMdd-HHmmssffffff");
                        //collegeIntakeExisting.NBAApproveLetter.FileName;
                        collegeIntakeExisting.NBAApproveLetter.SaveAs(string.Format("{0}/{1}{2}",
                            Server.MapPath("~/Content/Upload/College/NBAAccredited_Latest"), fileName, ext));
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

                //List<jntuh_college_intake_existing> jntuh_college_intake_existinglistlog = db.jntuh_college_intake_existing.Where(C => C.collegeId == userCollegeID).ToList();
                //jntuh_college_intake_existing_log jntuh_college_intake_existinglog = new jntuh_college_intake_existing_log();

                if (collegeIntakeExisting != null)
                {
                    //var jntuh_college_intake_existing = db.jntuh_college_intake_existing.Where(i => i.id == collegeIntakeExisting.id).FirstOrDefault();
                    //jntuh_college_intake_existing.nbaFrom = collegeIntakeExisting.nbaFrom;
                    //jntuh_college_intake_existing.nbaTo = collegeIntakeExisting.nbaTo;
                    //jntuh_college_intake_existing.updatedBy = collegeIntakeExisting.updatedBy;
                    //jntuh_college_intake_existing.updatedOn = collegeIntakeExisting.updatedOn;
                    //jntuh_college_intake_existing.NBAApproveLetter = nbafilename;
                    //db.Entry(jntuh_college_intake_existing).State = EntityState.Modified;

                    var jntuh_college_nbaaccreditationdata = db.jntuh_college_nbaaccreditationdata.Where(i => i.collegeid == collegeIntakeExisting.collegeId &&
                        i.accademicyear == collegeIntakeExisting.academicYearId && i.specealizationid == collegeIntakeExisting.specializationId).FirstOrDefault();

                    if (jntuh_college_nbaaccreditationdata == null)// insert
                    {
                        var objnbaaccreditation = new jntuh_college_nbaaccreditationdata()
                        {
                            collegeid = collegeIntakeExisting.collegeId,
                            accademicyear = AY0,
                            specealizationid = collegeIntakeExisting.specializationId,
                            nbafrom = Convert.ToDateTime(DateTime.ParseExact(collegeIntakeExisting.nbaFromDate, "dd/MM/yyyy", CultureInfo.InvariantCulture)),
                            nbato = Convert.ToDateTime(DateTime.ParseExact(collegeIntakeExisting.nbaToDate, "dd/MM/yyyy", CultureInfo.InvariantCulture)),
                            nbaapprovalletter = nbafilename,
                            createdby = userID,
                            createdon = DateTime.Now,
                            isactive = true
                        };
                        db.jntuh_college_nbaaccreditationdata.Add(objnbaaccreditation);
                        //db.Entry(jntuh_college_nbaaccreditationdata).State = EntityState.Detached;
                        cmd = "Add";
                    }
                    else //update
                    {
                        jntuh_college_nbaaccreditationdata.nbafrom = Convert.ToDateTime(DateTime.ParseExact(collegeIntakeExisting.nbaFromDate, "dd/MM/yyyy", CultureInfo.InvariantCulture));
                        jntuh_college_nbaaccreditationdata.nbato = Convert.ToDateTime(DateTime.ParseExact(collegeIntakeExisting.nbaToDate, "dd/MM/yyyy", CultureInfo.InvariantCulture));
                        jntuh_college_nbaaccreditationdata.nbaapprovalletter = nbafilename;
                        jntuh_college_nbaaccreditationdata.updatedon = DateTime.Now;
                        jntuh_college_nbaaccreditationdata.updatedby = userID;
                        db.Entry(jntuh_college_nbaaccreditationdata).State = EntityState.Modified;
                        cmd = "Update";
                    }
                    try
                    {
                        db.SaveChanges();
                    }
                    catch (Exception)
                    {
                        throw;
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

        //[Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        //public ActionResult Details(int? id)
        //{
        //    int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
        //    int userCollegeID =
        //        db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
        //    if (userCollegeID == 0)
        //    {
        //        userCollegeID =
        //            db.jntuh_college_intake_existing.Where(i => i.id == id).Select(i => i.collegeId).FirstOrDefault();
        //    }
        //    if (userCollegeID == 375)
        //    {
        //        userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
        //    }
        //    CollegeIntakeExisting collegeIntakeExisting = new CollegeIntakeExisting();
        //    if (Roles.IsUserInRole("Admin") == true)
        //    {
        //        userCollegeID =
        //            db.jntuh_college_intake_existing.Where(e => e.id == id).Select(e => e.collegeId).FirstOrDefault();
        //    }
        //    if (id != null)
        //    {

        //        ViewBag.IsUpdate = true;

        //        ViewBag.AcademicYear =
        //            db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true)
        //                .Select(a => a.academicYear)
        //                .FirstOrDefault();
        //        int actualYear =
        //            db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true)
        //                .Select(a => a.actualYear)
        //                .FirstOrDefault();

        //        //RAMESH: ADDED to MERGE BOTH EXISTING & PROPOSED INTAKE
        //        ViewBag.NextYear = String.Format("{0}-{1}", (actualYear + 1).ToString(),
        //            (actualYear + 2).ToString().Substring(2, 2));
        //        int AY0 =
        //            db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1))
        //                .Select(a => a.id)
        //                .FirstOrDefault();

        //        ViewBag.FirstYear = String.Format("{0}-{1}", (actualYear).ToString(),
        //            (actualYear + 1).ToString().Substring(2, 2));
        //        ViewBag.SecondYear = String.Format("{0}-{1}", (actualYear - 1).ToString(),
        //            (actualYear).ToString().Substring(2, 2));
        //        ViewBag.ThirdYear = String.Format("{0}-{1}", (actualYear - 2).ToString(),
        //            (actualYear - 1).ToString().Substring(2, 2));
        //        ViewBag.FourthYear = String.Format("{0}-{1}", (actualYear - 3).ToString(),
        //            (actualYear - 2).ToString().Substring(2, 2));
        //        ViewBag.FifthYear = String.Format("{0}-{1}", (actualYear - 4).ToString(),
        //            (actualYear - 3).ToString().Substring(2, 2));

        //        int presentYear =
        //            db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true)
        //                .Select(a => a.actualYear)
        //                .FirstOrDefault();
        //        int AY1 =
        //            db.jntuh_academic_year.Where(a => a.actualYear == (presentYear)).Select(a => a.id).FirstOrDefault();
        //        int AY2 =
        //            db.jntuh_academic_year.Where(a => a.actualYear == (presentYear - 1))
        //                .Select(a => a.id)
        //                .FirstOrDefault();
        //        int AY3 =
        //            db.jntuh_academic_year.Where(a => a.actualYear == (presentYear - 2))
        //                .Select(a => a.id)
        //                .FirstOrDefault();
        //        int AY4 =
        //            db.jntuh_academic_year.Where(a => a.actualYear == (presentYear - 3))
        //                .Select(a => a.id)
        //                .FirstOrDefault();
        //        int AY5 =
        //            db.jntuh_academic_year.Where(a => a.actualYear == (presentYear - 4))
        //                .Select(a => a.id)
        //                .FirstOrDefault();

        //        ////RAMESH : The following code is not required becoz JNTU is not going to use the code defined for COURT

        //        //ViewBag.FirstYear = String.Format("{0}-{1}", (actualYear + 1).ToString(), (actualYear + 2).ToString().Substring(2, 2));
        //        //ViewBag.SecondYear = String.Format("{0}-{1}", (actualYear).ToString(), (actualYear + 1).ToString().Substring(2, 2));
        //        //ViewBag.ThirdYear = String.Format("{0}-{1}", (actualYear - 1).ToString(), (actualYear).ToString().Substring(2, 2));
        //        //ViewBag.FourthYear = String.Format("{0}-{1}", (actualYear - 2).ToString(), (actualYear - 1).ToString().Substring(2, 2));
        //        //ViewBag.FifthYear = String.Format("{0}-{1}", (actualYear - 3).ToString(), (actualYear - 2).ToString().Substring(2, 2));

        //        //int presentYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();

        //        //int AY1 = db.jntuh_academic_year.Where(a => a.actualYear == (presentYear + 1)).Select(a => a.id).FirstOrDefault();
        //        //int AY2 = db.jntuh_academic_year.Where(a => a.actualYear == (presentYear)).Select(a => a.id).FirstOrDefault();
        //        //int AY3 = db.jntuh_academic_year.Where(a => a.actualYear == (presentYear - 1)).Select(a => a.id).FirstOrDefault();
        //        //int AY4 = db.jntuh_academic_year.Where(a => a.actualYear == (presentYear - 2)).Select(a => a.id).FirstOrDefault();
        //        //int AY5 = db.jntuh_academic_year.Where(a => a.actualYear == (presentYear - 3)).Select(a => a.id).FirstOrDefault();

        //        List<jntuh_college_intake_existing> intake =
        //            db.jntuh_college_intake_existing.Where(i => i.collegeId == userCollegeID && i.id == id).ToList();

        //        foreach (var item in intake)
        //        {
        //            collegeIntakeExisting.id = item.id;
        //            collegeIntakeExisting.collegeId = item.collegeId;
        //            collegeIntakeExisting.academicYearId = item.academicYearId;
        //            collegeIntakeExisting.shiftId = item.shiftId;
        //            collegeIntakeExisting.isActive = item.isActive;
        //            collegeIntakeExisting.nbaFrom = item.nbaFrom;
        //            collegeIntakeExisting.nbaTo = item.nbaTo;
        //            collegeIntakeExisting.specializationId = item.specializationId;
        //            collegeIntakeExisting.Specialization =
        //                db.jntuh_specialization.Where(s => s.id == item.specializationId)
        //                    .Select(s => s.specializationName)
        //                    .FirstOrDefault();
        //            collegeIntakeExisting.DepartmentID =
        //                db.jntuh_specialization.Where(s => s.id == item.specializationId)
        //                    .Select(s => s.departmentId)
        //                    .FirstOrDefault();
        //            collegeIntakeExisting.Department =
        //                db.jntuh_department.Where(d => d.id == collegeIntakeExisting.DepartmentID)
        //                    .Select(d => d.departmentName)
        //                    .FirstOrDefault();
        //            collegeIntakeExisting.degreeID =
        //                db.jntuh_department.Where(d => d.id == collegeIntakeExisting.DepartmentID)
        //                    .Select(d => d.degreeId)
        //                    .FirstOrDefault();
        //            collegeIntakeExisting.Degree =
        //                db.jntuh_degree.Where(d => d.id == collegeIntakeExisting.degreeID)
        //                    .Select(d => d.degree)
        //                    .FirstOrDefault();
        //            collegeIntakeExisting.shiftId = item.shiftId;
        //            collegeIntakeExisting.Shift =
        //                db.jntuh_shift.Where(s => s.id == item.shiftId).Select(s => s.shiftName).FirstOrDefault();
        //            collegeIntakeExisting.UploadNBAApproveLetter = item.NBAApproveLetter;
        //        }

        //        if (collegeIntakeExisting.nbaFrom != null)
        //            collegeIntakeExisting.nbaFromDate = Utilities.MMDDYY2DDMMYY(collegeIntakeExisting.nbaFrom.ToString());
        //        if (collegeIntakeExisting.nbaTo != null)
        //            collegeIntakeExisting.nbaToDate = Utilities.MMDDYY2DDMMYY(collegeIntakeExisting.nbaTo.ToString());

        //        jntuh_college_intake_existing nbadatedetails = db.jntuh_college_intake_existing
        //            .Where(
        //                e =>
        //                    e.collegeId == userCollegeID && e.academicYearId == AY1 &&
        //                    e.specializationId == collegeIntakeExisting.specializationId &&
        //                    e.shiftId == collegeIntakeExisting.shiftId)
        //            .Select(e => e)
        //            .FirstOrDefault();
        //        if (nbadatedetails != null)
        //        {
        //            if (nbadatedetails.nbaFrom != null)
        //                collegeIntakeExisting.nbaFromDate = Utilities.MMDDYY2DDMMYY(nbadatedetails.nbaFrom.ToString());
        //            if (nbadatedetails.nbaTo != null)
        //                collegeIntakeExisting.nbaToDate = Utilities.MMDDYY2DDMMYY(nbadatedetails.nbaTo.ToString());
        //        }

        //        jntuh_college_intake_existing details = db.jntuh_college_intake_existing
        //            .Where(
        //                e =>
        //                    e.collegeId == userCollegeID && e.academicYearId == AY0 &&
        //                    e.specializationId == collegeIntakeExisting.specializationId &&
        //                    e.shiftId == collegeIntakeExisting.shiftId)
        //            .Select(e => e)
        //            .FirstOrDefault();
        //        if (details != null)
        //        {
        //            collegeIntakeExisting.ApprovedIntake = details.approvedIntake;
        //            collegeIntakeExisting.letterPath = details.approvalLetter;
        //            collegeIntakeExisting.ProposedIntake = details.proposedIntake;
        //        }

        //        // collegeIntakeExisting.ProposedIntake = GetIntake(userCollegeID, AY0, collegeIntakeExisting.specializationId, collegeIntakeExisting.shiftId, 1);


        //        collegeIntakeExisting.ExambranchadmittedIntakeL1 = GetIntake(userCollegeID, AY1,
        //            collegeIntakeExisting.specializationId, collegeIntakeExisting.shiftId, 4);
        //        collegeIntakeExisting.ExambranchadmittedIntakeR1 = GetIntake(userCollegeID, AY1,
        //            collegeIntakeExisting.specializationId, collegeIntakeExisting.shiftId, 3);
        //        collegeIntakeExisting.ExambranchadmittedIntake1 = collegeIntakeExisting.ExambranchadmittedIntakeR1 + "+" +
        //                                                          collegeIntakeExisting.ExambranchadmittedIntakeL1;
        //        collegeIntakeExisting.AICTEapprovedIntake1 = GetIntake(userCollegeID, AY1,
        //            collegeIntakeExisting.specializationId, collegeIntakeExisting.shiftId, 2);
        //        collegeIntakeExisting.approvedIntake1 = GetIntake(userCollegeID, AY1,
        //            collegeIntakeExisting.specializationId, collegeIntakeExisting.shiftId, 1);
        //        collegeIntakeExisting.admittedIntake1 = GetIntake(userCollegeID, AY1,
        //            collegeIntakeExisting.specializationId, collegeIntakeExisting.shiftId, 0);

        //        collegeIntakeExisting.ExambranchadmittedIntakeL2 = GetIntake(userCollegeID, AY2,
        //            collegeIntakeExisting.specializationId, collegeIntakeExisting.shiftId, 4);
        //        collegeIntakeExisting.ExambranchadmittedIntakeR2 = GetIntake(userCollegeID, AY2,
        //            collegeIntakeExisting.specializationId, collegeIntakeExisting.shiftId, 3);
        //        collegeIntakeExisting.ExambranchadmittedIntake2 = collegeIntakeExisting.ExambranchadmittedIntakeR2 + "+" +
        //                                                          collegeIntakeExisting.ExambranchadmittedIntakeL2;
        //        collegeIntakeExisting.AICTEapprovedIntake2 = GetIntake(userCollegeID, AY2,
        //            collegeIntakeExisting.specializationId, collegeIntakeExisting.shiftId, 2);
        //        collegeIntakeExisting.approvedIntake2 = GetIntake(userCollegeID, AY2,
        //            collegeIntakeExisting.specializationId, collegeIntakeExisting.shiftId, 1);
        //        collegeIntakeExisting.admittedIntake2 = GetIntake(userCollegeID, AY2,
        //            collegeIntakeExisting.specializationId, collegeIntakeExisting.shiftId, 0);

        //        collegeIntakeExisting.ExambranchadmittedIntakeL3 = GetIntake(userCollegeID, AY3,
        //            collegeIntakeExisting.specializationId, collegeIntakeExisting.shiftId, 4);
        //        collegeIntakeExisting.ExambranchadmittedIntakeR3 = GetIntake(userCollegeID, AY3,
        //            collegeIntakeExisting.specializationId, collegeIntakeExisting.shiftId, 3);
        //        collegeIntakeExisting.ExambranchadmittedIntake3 = collegeIntakeExisting.ExambranchadmittedIntakeR3 + "+" +
        //                                                          collegeIntakeExisting.ExambranchadmittedIntakeL3;
        //        collegeIntakeExisting.AICTEapprovedIntake3 = GetIntake(userCollegeID, AY3,
        //            collegeIntakeExisting.specializationId, collegeIntakeExisting.shiftId, 2);
        //        collegeIntakeExisting.approvedIntake3 = GetIntake(userCollegeID, AY3,
        //            collegeIntakeExisting.specializationId, collegeIntakeExisting.shiftId, 1);
        //        collegeIntakeExisting.admittedIntake3 = GetIntake(userCollegeID, AY3,
        //            collegeIntakeExisting.specializationId, collegeIntakeExisting.shiftId, 0);

        //        collegeIntakeExisting.ExambranchadmittedIntakeL4 = GetIntake(userCollegeID, AY4,
        //            collegeIntakeExisting.specializationId, collegeIntakeExisting.shiftId, 4);
        //        collegeIntakeExisting.ExambranchadmittedIntakeR4 = GetIntake(userCollegeID, AY4,
        //            collegeIntakeExisting.specializationId, collegeIntakeExisting.shiftId, 3);
        //        collegeIntakeExisting.ExambranchadmittedIntake4 = collegeIntakeExisting.ExambranchadmittedIntakeR4 + "+" +
        //                                                          collegeIntakeExisting.ExambranchadmittedIntakeL4;
        //        collegeIntakeExisting.AICTEapprovedIntake4 = GetIntake(userCollegeID, AY4,
        //            collegeIntakeExisting.specializationId, collegeIntakeExisting.shiftId, 2);
        //        collegeIntakeExisting.approvedIntake4 = GetIntake(userCollegeID, AY4,
        //            collegeIntakeExisting.specializationId, collegeIntakeExisting.shiftId, 1);
        //        collegeIntakeExisting.admittedIntake4 = GetIntake(userCollegeID, AY4,
        //            collegeIntakeExisting.specializationId, collegeIntakeExisting.shiftId, 0);

        //        collegeIntakeExisting.ExambranchadmittedIntakeL5 = GetIntake(userCollegeID, AY5,
        //            collegeIntakeExisting.specializationId, collegeIntakeExisting.shiftId, 4);
        //        collegeIntakeExisting.ExambranchadmittedIntakeR5 = GetIntake(userCollegeID, AY5,
        //            collegeIntakeExisting.specializationId, collegeIntakeExisting.shiftId, 3);
        //        collegeIntakeExisting.ExambranchadmittedIntake5 = collegeIntakeExisting.ExambranchadmittedIntakeR5 + "+" +
        //                                                          collegeIntakeExisting.ExambranchadmittedIntakeL5;
        //        collegeIntakeExisting.AICTEapprovedIntake5 = GetIntake(userCollegeID, AY5,
        //            collegeIntakeExisting.specializationId, collegeIntakeExisting.shiftId, 2);
        //        collegeIntakeExisting.approvedIntake5 = GetIntake(userCollegeID, AY5,
        //            collegeIntakeExisting.specializationId, collegeIntakeExisting.shiftId, 1);
        //        collegeIntakeExisting.admittedIntake5 = GetIntake(userCollegeID, AY5,
        //            collegeIntakeExisting.specializationId, collegeIntakeExisting.shiftId, 0);

        //    }
        //    else
        //    {
        //        ViewBag.IsUpdate = false;
        //    }

        //    ViewBag.Degree =
        //        db.jntuh_college_degree.Join(db.jntuh_degree, collegeDegree => collegeDegree.degreeId,
        //            degree => degree.id,
        //            (collegeDegree, degree) => new
        //            {
        //                collegeDegree.degreeId,
        //                collegeDegree.collegeId,
        //                collegeDegree.isActive,
        //                degree.degree
        //            })
        //            .Where(collegeDegree => collegeDegree.collegeId == userCollegeID && collegeDegree.isActive == true)
        //            .Select(collegeDegree => new
        //            {
        //                collegeDegree.degreeId,
        //                collegeDegree.degree
        //            }).ToList();
        //    ViewBag.Department = db.jntuh_department.Where(d => d.isActive == true);
        //    ViewBag.Specialization = db.jntuh_specialization.Where(s => s.isActive == true);
        //    ViewBag.Shift = db.jntuh_shift.Where(s => s.isActive == true);

        //    if (Request.IsAjaxRequest())
        //    {
        //        return PartialView("Details", collegeIntakeExisting);
        //    }
        //    else
        //    {
        //        return View("Details", collegeIntakeExisting);
        //    }
        //}

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

        [Authorize(Roles = "College,Admin,SuperAdmin")]
        public ActionResult DeleteNBAAccredited(string nbaid, string enccollegeid)
        {
            var userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            var activityId = 0;
            if (!string.IsNullOrEmpty(nbaid))
            {
                activityId = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(nbaid.ToString(), System.Web.Configuration.WebConfigurationManager.AppSettings["CryptoKey"]));
            }
            try
            {
                if (activityId > 0)
                {
                    var data = db.jntuh_college_nbaaccreditationdata.Find(activityId);
                    db.jntuh_college_nbaaccreditationdata.Remove(data);
                    db.SaveChanges();
                    if (true)
                    {
                        TempData["Success"] = "NBA Accreditation Record Deleted successfully.";
                    }
                }
            }
            catch (Exception)
            {
                TempData["Error"] = "Error while deleting..";
            }

            return RedirectToAction("Index", new { collegeId = enccollegeid });
        }
    }
}
