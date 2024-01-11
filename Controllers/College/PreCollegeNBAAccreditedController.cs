using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using UAAAS.Models;

namespace UAAAS.Controllers.College
{
    [ErrorHandling]
    public class PreCollegeNBAAccreditedController : BaseController
    {
        uaaasDBContext db = new uaaasDBContext();

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult Index()
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
                    i => i.collegeId == userCollegeID && !inactivespids.Contains(i.specializationId) && (i.academicYearId == AY1 || i.academicYearId == AY2 || i.academicYearId == AY3) && (i.approvedIntake != 0 || i.aicteApprovedIntake != 0)).ToList();

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
                       r.collegeId == userCollegeID && r.academicyearId == (AY1) &&
                       (r.noctypeId == 18) && r.specializationId == item.specializationId).Select(s => s.id).FirstOrDefault();
                newIntake.id = item.id;
                newIntake.nocid = nocdataid;
                newIntake.collegeId = item.collegeId;
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

                var nbadt = nbaaccrdata.Where(i => i.specealizationid == item.specializationId && i.accademicyear == AY1).FirstOrDefault();

                if (nbadt != null)
                {
                    if (nbadt.nbafrom != null)
                        //item.nbaFromDate = Utilities.MMDDYY2DDMMYY(nbadt.nbafrom.ToString("dd/MM/yyyy"));
                        newIntake.nbaFromDate = nbadt.nbafrom.ToString("dd/MM/yyyy");
                    if (nbadt.nbato != null)
                        //item.nbaToDate = Utilities.MMDDYY2DDMMYY(nbadt.nbato.ToString("dd/MM/yyyy"));
                        newIntake.nbaToDate = nbadt.nbato.ToString("dd/MM/yyyy");
                    newIntake.UploadNBAApproveLetter = nbadt.nbaapprovalletter;
                    ViewBag.ButtonVisible = true;
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
                        it.CollegeId == clgCode && it.AcademicYearId == AY1 && it.TxnDate.Year == currentYear &&
                        it.AuthStatus == "0300") > 0;
            //ViewBag.Success = false;
            return View(collegeIntakeExisting);
        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult PostPreCollegeNBAData()
        {
            if (Membership.GetUser() == null)
            {
                return RedirectToAction("LogOn", "Account");
            }
            DateTime todayDate = DateTime.Now;
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID =
                db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();

            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            var jntuh_academic_year = db.jntuh_academic_year.Select(s => s).ToList();
            int actualYear = jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();

            List<jntuh_academic_year> jntuh_academic_years = db.jntuh_academic_year.AsNoTracking().ToList();

            int AY0 =
                jntuh_academic_years.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();

            int presentYear =
                jntuh_academic_years.Where(a => a.isActive == true && a.isPresentAcademicYear == true)
                    .Select(a => a.actualYear)
                    .FirstOrDefault();

            int AY1 = jntuh_academic_years.Where(a => a.actualYear == (presentYear)).Select(a => a.id).FirstOrDefault();

            int[] inactivespids = db.jntuh_specialization.Where(s => s.isActive == false).Select(s => s.id).ToArray();

            List<jntuh_college_nbaaccreditationdata> previousYearIntake =
                db.jntuh_college_nbaaccreditationdata.Where(
                    i => i.collegeid == userCollegeID && !inactivespids.Contains(i.specealizationid) && (i.accademicyear == AY1)).ToList();

            var nbaaccrdata = db.jntuh_college_nbaaccreditationdata.AsNoTracking().Where(i => i.collegeid == userCollegeID).ToList();

            previousYearIntake =
                previousYearIntake.AsEnumerable()
                    .GroupBy(r => new { r.specealizationid })
                    .Select(r => r.First())
                    .ToList();

            int count = 0;

            foreach (var item in previousYearIntake)
            {
                var nbadt = nbaaccrdata.Where(i => i.specealizationid == item.specealizationid && i.accademicyear == AY0).FirstOrDefault();

                if (nbadt == null)
                {
                    var PYintake = nbaaccrdata.Where(i => i.specealizationid == item.specealizationid && i.accademicyear == AY1).FirstOrDefault();

                    var objnbaaccreditation = new jntuh_college_nbaaccreditationdata()
                    {
                        collegeid = item.collegeid,
                        accademicyear = AY0,
                        specealizationid = item.specealizationid,
                        nbafrom = PYintake.nbafrom,
                        nbato = PYintake.nbato,
                        nbaapprovalletter = item.nbaapprovalletter,
                        createdby = userID,
                        createdon = DateTime.Now,
                        isactive = true
                    };
                    db.jntuh_college_nbaaccreditationdata.Add(objnbaaccreditation);
                    db.SaveChanges();
                    count += 1;
                }
            }
            if (count != 0)
            {
                TempData["Success"] = "Added Successfully!";
                return RedirectToAction("Index", "CollegeNBAAccredited");
            }
            else
            {
                TempData["Error"] = "Nothing to change";
                return RedirectToAction("Index");
            }
        }
    }
}
