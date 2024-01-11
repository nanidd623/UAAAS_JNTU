using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Security;
using iTextSharp.text;
using iTextSharp.text.html.simpleparser;
using iTextSharp.text.pdf;
using UAAAS.Models;
using Utilities = UAAAS.Models.Utilities;
using System.Configuration;

namespace UAAAS.Controllers
{
    //Code written by Siva
    [ErrorHandling]
    public class PharmacyreportController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();
        private string bpharmacycondition;
        private string pharmdcondition;
        private string pharmadpbcondition;
        private decimal pharmadpbrequiredfaculty;
        private decimal BpharmacyrequiredFaculty;
        private decimal PharmDRequiredFaculty;
        private decimal PharmDPBRequiredFaculty;
        private int TotalAreaRequiredFaculty = 0;
        public ActionResult Index()
        {
            return View();
        }

        #region Pharmacy Faculty DataEntry For Deficiency Code Wrritten by siva

        //Code Written By Siva
        [Authorize(Roles = "Admin,SuperAdmin,DataEntry,FacultyVerification")]
        public ActionResult PharmacyFacultyGroupingbasedonColleges(int? collegeId, int? DegreeId, int? GroupId, int? TotalRequiredFaculty, int? SpecializationWiseFaculty)
        {
            int userId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);

            //var CollegeIds = new int[] { 6, 24, 27, 30, 34, 47, 52, 54, 55, 58, 60, 65, 78, 90, 95, 97, 104, 105, 107, 110, 114, 117, 118, 120, 135, 136, 139, 140, 146, 150, 159, 169, 202, 204, 206, 213, 219, 234, 237, 253, 262, 263, 267, 283, 290, 295, 297, 298, 301, 302, 303, 313, 314, 315, 317, 318, 319, 320, 332, 353, 370, 379, 384, 389, 392, 395, 410, 428, 436, 442, 445 };
            var CollegeIds = new int[] { 448, 180, 159, 376, 428, 445, 24, 117, 202, 213, 395, 364, 27, 30, 6, 34, 55, 58, 54, 52, 9, 60, 65, 78, 150, 204, 219, 90, 97, 104, 110, 139, 105, 107, 114, 379, 118, 39, 120, 389, 127, 237, 135, 136, 47, 252, 169, 253, 262, 442, 301, 436, 370, 263, 267, 206, 140, 44, 290, 454, 42, 95, 297, 384, 75, 348, 298, 295, 303, 302, 283, 332, 410, 392, 234, 146, 315, 320, 317, 314, 318, 319, 353, 313, 235, 375 };
            var jntuhcollege = db.jntuh_college.AsNoTracking().Where(e => CollegeIds.Contains(e.id)).ToList();

            ViewBag.PharmacyCollegeList = jntuhcollege.Select(e => new
            {
                collegeId = e.id,
                collegeName = e.collegeCode + "-" + e.collegeName
            }).ToList();

            if (collegeId == null)
                ViewBag.checkcollegeId = null;
            else
                ViewBag.checkcollegeId = collegeId;

            ViewBag.AfterCollegeSelect = null;

            string StringCollegeId = collegeId == null ? "0" : collegeId.ToString();

            List<SelectListItem> DegreesFirst = new List<SelectListItem>();
            DegreesFirst.Add(new SelectListItem { Text = "---Select---", Value = "1" });
            ViewBag.Degrees = DegreesFirst;

            List<SelectListItem> GroupsFirst = new List<SelectListItem>();
            GroupsFirst.Add(new SelectListItem { Text = "---Select---", Value = "1" });
            ViewBag.Groups = GroupsFirst;

            TempData["ViewCollegeId"] = null;

            string Group1 = "1";
            string Group2 = "2";
            string Group3 = "3";
            string Group4 = "4";

            var jntuh_academic_year = db.jntuh_academic_year.ToList();
            int presentYear = jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int AY1 = jntuh_academic_year.Where(a => a.actualYear == (presentYear + 1)).Select(a => a.id).FirstOrDefault();
            int AY2 = jntuh_academic_year.Where(a => a.actualYear == (presentYear)).Select(a => a.id).FirstOrDefault();
            int AY3 = jntuh_academic_year.Where(a => a.actualYear == (presentYear - 1)).Select(a => a.id).FirstOrDefault();
            int AY4 = jntuh_academic_year.Where(a => a.actualYear == (presentYear - 2)).Select(a => a.id).FirstOrDefault();
            int AY5 = jntuh_academic_year.Where(a => a.actualYear == (presentYear - 3)).Select(a => a.id).FirstOrDefault();
            int AY6 = jntuh_academic_year.Where(a => a.actualYear == (presentYear - 4)).Select(a => a.id).FirstOrDefault();
            int AY7 = jntuh_academic_year.Where(a => a.actualYear == (presentYear - 5)).Select(a => a.id).FirstOrDefault();




            if (collegeId != null && GroupId == null)
            {
                TempData["ViewCollegeId"] = collegeId;
                string CollegeidNew1 = collegeId.ToString();

                var jntuh_specialization = db.jntuh_specialization.Where(e => e.isActive == true).ToList();

                var jntuh_college_intake_existing = db.jntuh_college_intake_existing.Where(e => e.collegeId == collegeId).Select(e => e).ToList();
                var jntuh_college_intake_existing_Data = jntuh_college_intake_existing.Where(e => e.academicYearId == AY1 && e.proposedIntake != 0 && e.courseStatus != "Closure").Select(w => w).ToList();
                var SpecializationIds = jntuh_college_intake_existing_Data.Select(e => e.specializationId).Distinct().ToArray();
                var Degrees = (from e in jntuh_college_intake_existing_Data
                               join s in db.jntuh_specialization on e.specializationId equals s.id
                               join d in db.jntuh_department on s.departmentId equals d.id
                               join de in db.jntuh_degree on d.degreeId equals de.id
                               where SpecializationIds.Contains(s.id) && (de.id == 2 || de.id == 5 || de.id == 9 || de.id == 10)
                               select new
                               {
                                   DegreeId = de.id,
                                   Degreename = de.degree
                               }).Distinct().ToList();

                ViewBag.Degrees = Degrees;

                List<SelectListItem> Groups = new List<SelectListItem>();
                Groups.Add(new SelectListItem { Text = "---Select---", Value = "1" });
                ViewBag.Groups = Groups;

                //BpharmacyIntake
                int? BpharmacyProposedintake = jntuh_college_intake_existing.Where(e => e.academicYearId == AY1 && e.specializationId == 12).Select(e => e.proposedIntake).FirstOrDefault();
                int? Bpharmacyintake1 = jntuh_college_intake_existing.Where(e => e.academicYearId == AY2 && e.specializationId == 12).Select(e => e.approvedIntake).FirstOrDefault();
                int? Bpharmacyintake2 = jntuh_college_intake_existing.Where(e => e.academicYearId == AY3 && e.specializationId == 12).Select(e => e.approvedIntake).FirstOrDefault();
                int? Bpharmacyintake3 = jntuh_college_intake_existing.Where(e => e.academicYearId == AY4 && e.specializationId == 12).Select(e => e.approvedIntake).FirstOrDefault();
                int? Bpharmacyintake4 = jntuh_college_intake_existing.Where(e => e.academicYearId == AY5 && e.specializationId == 12).Select(e => e.approvedIntake).FirstOrDefault();
                int? Bpharmacyintake5 = jntuh_college_intake_existing.Where(e => e.academicYearId == AY6 && e.specializationId == 12).Select(e => e.approvedIntake).FirstOrDefault();
                int? Bpharmacyintake6 = jntuh_college_intake_existing.Where(e => e.academicYearId == AY7 && e.specializationId == 12).Select(e => e.approvedIntake).FirstOrDefault();

                ViewBag.BpharmacyProposedInatke = BpharmacyProposedintake;
                ViewBag.BpharmcySecondInatke = (Bpharmacyintake1);
                ViewBag.BpharmcythirdInatke = (Bpharmacyintake2);
                ViewBag.BpharmcyfouthInatke = (Bpharmacyintake3);
                ViewBag.BpharmcyfifthInatke = (Bpharmacyintake4);
                ViewBag.BpharmcySixthInatke = (Bpharmacyintake5);
                ViewBag.BpharmcySeventhInatke = (Bpharmacyintake6);

                //Pharm D Intake
                int? PharmDProposedintake = jntuh_college_intake_existing.Where(e => e.academicYearId == AY1 && e.specializationId == 18).Select(e => e.proposedIntake).FirstOrDefault();
                int? PharmDintake1 = jntuh_college_intake_existing.Where(e => e.academicYearId == AY2 && e.specializationId == 18).Select(e => e.approvedIntake).FirstOrDefault();
                int? PharmDintake2 = jntuh_college_intake_existing.Where(e => e.academicYearId == AY3 && e.specializationId == 18).Select(e => e.approvedIntake).FirstOrDefault();
                int? PharmDintake3 = jntuh_college_intake_existing.Where(e => e.academicYearId == AY4 && e.specializationId == 18).Select(e => e.approvedIntake).FirstOrDefault();
                int? PharmDintake4 = jntuh_college_intake_existing.Where(e => e.academicYearId == AY5 && e.specializationId == 18).Select(e => e.approvedIntake).FirstOrDefault();
                int? PharmDintake5 = jntuh_college_intake_existing.Where(e => e.academicYearId == AY6 && e.specializationId == 18).Select(e => e.approvedIntake).FirstOrDefault();
                int? PharmDintake6 = jntuh_college_intake_existing.Where(e => e.academicYearId == AY7 && e.specializationId == 18).Select(e => e.approvedIntake).FirstOrDefault();

                ViewBag.PharmDProposedInatke = PharmDProposedintake;
                ViewBag.PharmDySecondInatke = (PharmDintake1);
                ViewBag.PharmDthirdInatke = (PharmDintake2);
                ViewBag.PharmDfouthInatke = (PharmDintake3);
                ViewBag.PharmDfifthInatke = (PharmDintake4);
                ViewBag.PharmDSixthInatke = (PharmDintake5);
                ViewBag.PharmDSeventhInatke = (PharmDintake6);

                //Pharm D.PB Intake
                int? PharmDPBProposedintake = jntuh_college_intake_existing.Where(e => e.academicYearId == AY1 && e.specializationId == 19).Select(e => e.proposedIntake).FirstOrDefault();
                int? PharmDPBintake1 = jntuh_college_intake_existing.Where(e => e.academicYearId == AY2 && e.specializationId == 19).Select(e => e.approvedIntake).FirstOrDefault();
                int? PharmDPBintake2 = jntuh_college_intake_existing.Where(e => e.academicYearId == AY3 && e.specializationId == 19).Select(e => e.approvedIntake).FirstOrDefault();
                int? PharmDPBintake3 = jntuh_college_intake_existing.Where(e => e.academicYearId == AY4 && e.specializationId == 19).Select(e => e.approvedIntake).FirstOrDefault();
                int? PharmDPBintake4 = jntuh_college_intake_existing.Where(e => e.academicYearId == AY5 && e.specializationId == 19).Select(e => e.approvedIntake).FirstOrDefault();
                int? PharmDPBintake5 = jntuh_college_intake_existing.Where(e => e.academicYearId == AY6 && e.specializationId == 19).Select(e => e.approvedIntake).FirstOrDefault();
                int? PharmDPBintake6 = jntuh_college_intake_existing.Where(e => e.academicYearId == AY7 && e.specializationId == 19).Select(e => e.approvedIntake).FirstOrDefault();

                ViewBag.PharmDPBProposedInatke = PharmDPBProposedintake;
                ViewBag.PharmDPBySecondInatke = (PharmDPBintake1);
                ViewBag.PharmDPBthirdInatke = (PharmDPBintake2);
                ViewBag.PharmDPBfouthInatke = (PharmDPBintake3);
                ViewBag.PharmDPBfifthInatke = (PharmDPBintake4);
                ViewBag.PharmDPBSixthInatke = (PharmDPBintake5);
                ViewBag.PharmDPBSeventhInatke = (PharmDPBintake5);

                var jntuh_college = jntuhcollege.Where(e => e.id == collegeId).Select(e => e).FirstOrDefault();
                var teachingFaculty = new List<FacultyRegistration>();
                var jntuh_college_faculty_registered = db.jntuh_college_faculty_registered.Where(e => e.collegeId == collegeId && (e.DepartmentId == 26 || e.DepartmentId == 36 || e.DepartmentId == 27 || e.DepartmentId == 39 || e.DepartmentId == 61)).Select(e => e).ToList();
                var jntuh_college_faculty_registered_new = jntuh_college_faculty_registered;
                var jntuh_college_prinicipal_registered = db.jntuh_college_principal_registered.Where(c => c.collegeId == collegeId).Select(e => e.RegistrationNumber).FirstOrDefault();

                ViewBag.Prinicipal = jntuh_college_prinicipal_registered;

                var strRegnos = jntuh_college_faculty_registered.Select(e => e.RegistrationNumber).ToList();

                var jntuh_registered_faculty_New = db.jntuh_registered_faculty.Where(e => strRegnos.Contains(e.RegistrationNumber.Trim())).Select(e => e).ToList();

                //var jntuh_registered_faculty = jntuh_registered_faculty_New.Where(rf => rf.type != "Adjunct" && ((rf.NoForm16 == false || rf.NoForm16 == null) && (rf.NoForm26AS == false || rf.NoForm26AS == null) && (rf.Absent == false || rf.Absent == null) && (rf.OriginalCertificatesNotShown == false || rf.OriginalCertificatesNotShown == null) && (rf.Xeroxcopyofcertificates == false || rf.Xeroxcopyofcertificates == null) && (rf.NotQualifiedAsperAICTE == false || rf.NotQualifiedAsperAICTE == null)
                //                                       && (rf.NoSCM == false || rf.NoSCM == null) && (rf.PANNumber != null) && (rf.IncompleteCertificates == false || rf.IncompleteCertificates == null) && (rf.Blacklistfaculy == false) && rf.NoRelevantUG != "Yes" && rf.NoRelevantPG != "Yes" && rf.NORelevantPHD != "Yes"
                //                                       && (rf.InvalidPANNumber == false || rf.InvalidPANNumber == null) && (rf.OriginalsVerifiedPHD != true) && (rf.OriginalsVerifiedUG != true) && (rf.Noclass == false || rf.Noclass == null) && (rf.FakePHD == false || rf.FakePHD == null) && (rf.Genuinenessnotsubmitted == false || rf.Genuinenessnotsubmitted == null) && (rf.NoPGspecialization == false || rf.NoPGspecialization == null) && (rf.Invaliddegree == false || rf.Invaliddegree == null) && rf.BAS != "Yes" && (rf.AbsentforVerification == false || rf.AbsentforVerification == null)) && rf.InvalidAadhaar != "Yes").Select(rf => rf).ToList();
                //var jntuh_registered_faculty = jntuh_registered_faculty_New.Where(rf => rf.type != "Adjunct" && ((rf.NotQualifiedAsperAICTE == false || rf.NotQualifiedAsperAICTE == null) && (rf.Noclass == false || rf.Noclass == null) && (rf.Absent == false || rf.Absent == null) && (rf.OriginalCertificatesNotShown == false || rf.OriginalCertificatesNotShown == null) &&
                //                                         (rf.NoSCM == false || rf.NoSCM == null) && rf.NoRelevantUG != "Yes" && rf.NoRelevantPG != "Yes" && rf.NORelevantPHD != "Yes" && (rf.InvalidPANNumber == false || rf.InvalidPANNumber == null) && (rf.Blacklistfaculy == false) && (rf.AbsentforVerification == false || rf.AbsentforVerification == null))).Select(rf => rf).ToList();
                var jntuh_registered_faculty = jntuh_registered_faculty_New.Where(rf => rf.type != "Adjunct" && (rf.BAS != "Yes") && (rf.Noclass == false || rf.Noclass == null) && (rf.Absent == false || rf.Absent == null) &&
                                                            (rf.NoSCM == false || rf.NoSCM == null) && (rf.Blacklistfaculy == false) && (rf.AbsentforVerification == false || rf.AbsentforVerification == null)).Select(rf => rf).ToList();
                var jntuh_registered_facultyBAS = jntuh_registered_faculty_New.Where(rf => (rf.BAS == "Yes")).Select(rf => new
                {
                    FacultyId = rf.id,
                    RegistrationNumber = rf.RegistrationNumber.Trim(),
                    BASFlag = rf.BAS
                }).ToList();
                var BASRegNos = jntuh_registered_facultyBAS.Select(rf => rf.RegistrationNumber).ToArray();
                string CollegeidNew = collegeId.ToString();
                var jntuh_appeal_pharmacydata = db.jntuh_appeal_pharmacydata.Where(e => e.CollegeCode == CollegeidNew && (e.Deficiency == null || !BASRegNos.Contains(e.Deficiency))).Select(e => e).ToList();
                var RegNumbers = jntuh_appeal_pharmacydata.Select(e => e.Deficiency).ToList();

                jntuh_college_faculty_registered_new = jntuh_college_faculty_registered_new.Where(e => !RegNumbers.Contains(e.RegistrationNumber)).Select(e => e).ToList();

                jntuh_registered_faculty = jntuh_registered_faculty.Where(e => !RegNumbers.Contains(e.RegistrationNumber)).Select(e => e).ToList();

                //var jntuh_appeal_pharmacy_data = db.jntuh_appeal_pharmacydata.Where(e => e.CollegeCode == StringCollegeId).Select(e => e).ToList();

                ViewBag.Group1Required = jntuh_appeal_pharmacydata.Where(e => e.PharmacySpecialization == Group1).Select(e => e.SpecializationWiseRequiredFaculty).FirstOrDefault() == null ? 0 : jntuh_appeal_pharmacydata.Where(e => e.PharmacySpecialization == Group1).Select(e => e.SpecializationWiseRequiredFaculty).LastOrDefault();
                ViewBag.Group2Required = jntuh_appeal_pharmacydata.Where(e => e.PharmacySpecialization == Group2).Select(e => e.SpecializationWiseRequiredFaculty).FirstOrDefault() == null ? 0 : jntuh_appeal_pharmacydata.Where(e => e.PharmacySpecialization == Group2).Select(e => e.SpecializationWiseRequiredFaculty).LastOrDefault(); ;
                ViewBag.Group3Required = jntuh_appeal_pharmacydata.Where(e => e.PharmacySpecialization == Group3).Select(e => e.SpecializationWiseRequiredFaculty).FirstOrDefault() == null ? 0 : jntuh_appeal_pharmacydata.Where(e => e.PharmacySpecialization == Group3).Select(e => e.SpecializationWiseRequiredFaculty).LastOrDefault(); ;
                ViewBag.Group4Required = jntuh_appeal_pharmacydata.Where(e => e.PharmacySpecialization == Group4).Select(e => e.SpecializationWiseRequiredFaculty).FirstOrDefault() == null ? 0 : jntuh_appeal_pharmacydata.Where(e => e.PharmacySpecialization == Group4).Select(e => e.SpecializationWiseRequiredFaculty).LastOrDefault(); ;

                var Group1Available = jntuh_appeal_pharmacydata.Where(e => e.PharmacySpecialization == Group1).Select(e => e).ToList();
                var Group2Available = jntuh_appeal_pharmacydata.Where(e => e.PharmacySpecialization == Group2).Select(e => e).ToList();
                var Group3Available = jntuh_appeal_pharmacydata.Where(e => e.PharmacySpecialization == Group3).Select(e => e).ToList();
                var Group4Available = jntuh_appeal_pharmacydata.Where(e => e.PharmacySpecialization == Group4).Select(e => e).ToList();

                ViewBag.Group1Available = Group1Available.Where(e => e.Deficiency != null).Select(e => e.Deficiency).Count();
                ViewBag.Group2Available = Group2Available.Where(e => e.Deficiency != null).Select(e => e.Deficiency).Count();
                ViewBag.Group3Available = Group3Available.Where(e => e.Deficiency != null).Select(e => e.Deficiency).Count();
                ViewBag.Group4Available = Group4Available.Where(e => e.Deficiency != null).Select(e => e.Deficiency).Count();

                //var Group1RemainingRegnos = jntuh_college_faculty_registered_new.Where(e => e.FacultySpecializationId == 115 || e.FacultySpecializationId == 119 || e.FacultySpecializationId == 172 || e.FacultySpecializationId == 120).Select(e => e.RegistrationNumber).ToList();
                //ViewBag.Group1RemainingFaculty = jntuh_registered_faculty.Where(e => Group1RemainingRegnos.Contains(e.RegistrationNumber)).Select(e => e.RegistrationNumber).Count();

                //var Group2RemainingRegnos = jntuh_college_faculty_registered_new.Where(e => e.FacultySpecializationId == 116 || e.FacultySpecializationId == 167 || e.FacultySpecializationId == 123 || e.FacultySpecializationId == 170 || e.FacultySpecializationId == 117).Select(e => e.RegistrationNumber).ToList();
                //ViewBag.Group2RemainingFaculty = jntuh_registered_faculty.Where(e => Group2RemainingRegnos.Contains(e.RegistrationNumber)).Select(e => e.RegistrationNumber).Count();

                //var Group3RemainingRegnos = jntuh_college_faculty_registered_new.Where(e => e.FacultySpecializationId == 169 || e.FacultySpecializationId == 18 || e.FacultySpecializationId == 19 || e.FacultySpecializationId == 114 || e.FacultySpecializationId == 122).Select(e => e.RegistrationNumber).ToList();
                //ViewBag.Group3RemainingFaculty = jntuh_registered_faculty.Where(e => Group3RemainingRegnos.Contains(e.RegistrationNumber)).Select(e => e.RegistrationNumber).Count();

                //var Group4RemainingRegnos = jntuh_college_faculty_registered_new.Where(e => e.FacultySpecializationId == 172 || e.FacultySpecializationId == 117 || e.FacultySpecializationId == 121).Select(e => e.RegistrationNumber).ToList();
                //ViewBag.Group4RemainingFaculty = jntuh_registered_faculty.Where(e => Group4RemainingRegnos.Contains(e.RegistrationNumber)).Select(e => e.RegistrationNumber).Count();

                var Group1RemainingRegnos = jntuh_registered_faculty.Where(e => e.Jntu_PGSpecializationId == 115 || e.Jntu_PGSpecializationId == 119 || e.Jntu_PGSpecializationId == 172 || e.Jntu_PGSpecializationId == 120 || e.Jntu_PGSpecializationId == 118).Select(e => e.RegistrationNumber).ToList();
                ViewBag.Group1RemainingFaculty = jntuh_registered_faculty.Where(e => Group1RemainingRegnos.Contains(e.RegistrationNumber)).Select(e => e.RegistrationNumber).Count();

                var Group2RemainingRegnos = jntuh_registered_faculty.Where(e => e.Jntu_PGSpecializationId == 116 || e.Jntu_PGSpecializationId == 167 || e.Jntu_PGSpecializationId == 123 || e.Jntu_PGSpecializationId == 170 || e.Jntu_PGSpecializationId == 117 || e.Jntu_PGSpecializationId == 124).Select(e => e.RegistrationNumber).ToList();
                ViewBag.Group2RemainingFaculty = jntuh_registered_faculty.Where(e => Group2RemainingRegnos.Contains(e.RegistrationNumber)).Select(e => e.RegistrationNumber).Count();

                var Group3RemainingRegnos = jntuh_registered_faculty.Where(e => e.Jntu_PGSpecializationId == 169 || e.Jntu_PGSpecializationId == 18 || e.Jntu_PGSpecializationId == 19 || e.Jntu_PGSpecializationId == 114 || e.Jntu_PGSpecializationId == 122).Select(e => e.RegistrationNumber).ToList();
                ViewBag.Group3RemainingFaculty = jntuh_registered_faculty.Where(e => Group3RemainingRegnos.Contains(e.RegistrationNumber)).Select(e => e.RegistrationNumber).Count();

                var Group4RemainingRegnos = jntuh_registered_faculty.Where(e => e.Jntu_PGSpecializationId == 172 || e.Jntu_PGSpecializationId == 117 || e.Jntu_PGSpecializationId == 121).Select(e => e.RegistrationNumber).ToList();
                ViewBag.Group4RemainingFaculty = jntuh_registered_faculty.Where(e => Group4RemainingRegnos.Contains(e.RegistrationNumber)).Select(e => e.RegistrationNumber).Count();



                var FacultyIds = jntuh_registered_faculty.Select(e => e.id).ToList();

                var jntuh_registered_faculty_education = db.jntuh_registered_faculty_education.Where(e => FacultyIds.Contains(e.facultyId)).Select(e => e).ToList();

                List<FacultyRegistration> data = new List<FacultyRegistration>();
                foreach (var item in jntuh_registered_faculty)
                {
                    string Reason = null;
                    FacultyRegistration Faculty = new FacultyRegistration();
                    Faculty.id = item.id;
                    Faculty.Type = item.type;
                    Faculty.RegistrationNumber = item.RegistrationNumber;
                    Faculty.UniqueID = item.UniqueID;
                    Faculty.FirstName = item.FirstName;
                    Faculty.MiddleName = item.MiddleName;
                    Faculty.LastName = item.LastName;
                    Faculty.GenderId = item.GenderId;
                    Faculty.Email = item.Email;
                    Faculty.facultyPhoto = item.Photo;
                    Faculty.Mobile = item.Mobile;
                    Faculty.PANNumber = item.PANNumber;
                    Faculty.AadhaarNumber = item.AadhaarNumber;
                    Faculty.DegreeId = jntuh_registered_faculty_education.Count(e => e.facultyId == item.id) > 0 ? item.jntuh_registered_faculty_education.Where(e => e.facultyId == item.id).Select(e => e.educationId).Max() : 0;
                    Faculty.PANNumber = item.PANNumber;

                    Faculty.Absent = item.Absent != null ? (bool)item.Absent : false;
                    Faculty.XeroxcopyofcertificatesFlag = item.Xeroxcopyofcertificates != null ? (bool)item.Xeroxcopyofcertificates : false;
                    Faculty.NOrelevantUgFlag = item.NoRelevantUG == "Yes" ? true : false;
                    Faculty.NOrelevantPgFlag = item.NoRelevantPG == "Yes" ? true : false;
                    Faculty.NOrelevantPhdFlag = item.NORelevantPHD == "Yes" ? true : false;
                    Faculty.NOTQualifiedAsPerAICTE = item.NotQualifiedAsperAICTE != null ? (bool)item.NotQualifiedAsperAICTE : false;
                    Faculty.InvalidPANNo = item.InvalidPANNumber != null ? (bool)item.InvalidPANNumber : false;
                    Faculty.InCompleteCeritificates = item.IncompleteCertificates == true ? true : false;
                    Faculty.NoSCM = item.NoSCM != null ? (bool)item.NoSCM : false;
                    Faculty.OriginalCertificatesnotshownFlag = item.OriginalCertificatesNotShown != null ? (bool)item.OriginalCertificatesNotShown : false;
                    Faculty.NotIdentityFiedForAnyProgramFlag = item.NotIdentityfiedForanyProgram != null ? (bool)item.NotIdentityfiedForanyProgram : false;
                    Faculty.Basstatus = item.InvalidAadhaar;
                    Faculty.BasstatusOld = item.BAS;
                    Faculty.OriginalsVerifiedUG = item.OriginalsVerifiedUG == true ? true : false;
                    Faculty.OriginalsVerifiedPHD = item.OriginalsVerifiedPHD == true ? true : false;
                    Faculty.BlacklistFaculty = item.Blacklistfaculy == true ? true : false;
                    Faculty.VerificationStatus = item.AbsentforVerification == true ? true : false;

                    //New Flags 
                    Faculty.InvalidDegree = item.Invaliddegree == true ? true : false;
                    Faculty.NoClass = item.Noclass == true ? true : false;
                    Faculty.GenuinenessnotSubmitted = item.Genuinenessnotsubmitted == true ? true : false;
                    Faculty.FakePhd = item.FakePHD == true ? true : false;
                    Faculty.NoPgSpecialization = item.NoPGspecialization == true ? true : false;
                    Faculty.NotconsiderPhd = item.NotconsideredPHD == true ? true : false;

                    Faculty.NoSCM17Flag = item.NoSCM17 != null ? (bool)item.NoSCM17 : false;
                    Faculty.PhdUndertakingDocumentstatus = item.PhdUndertakingDocumentstatus != null ? (bool)(item.PhdUndertakingDocumentstatus) : false;
                    Faculty.PHDUndertakingDocumentView = item.PHDUndertakingDocument;
                    Faculty.PhdUndertakingDocumentText = item.PhdUndertakingDocumentText;

                    Faculty.Deactivedby = item.DeactivatedBy;
                    Faculty.DeactivedOn = item.DeactivatedOn;

                    Faculty.DegreeId = jntuh_registered_faculty_education.Count(E => E.facultyId == item.id) > 0 ? jntuh_registered_faculty_education.Where(E => E.facultyId == item.id).Select(E => E.educationId).Max() : 0;

                    if (Faculty.Absent == true)
                        Reason += "Absent";

                    if (Faculty.Type == "Adjunct")
                    {
                        if (Reason != null)
                            Reason += ",Adjunct Faculty";
                        else
                            Reason += "Adjunct Faculty";
                    }

                    if (Faculty.NoClass == true)
                    {
                        if (Reason != null)
                            Reason += ",No Class";
                        else
                            Reason += "No Class";
                    }

                    //if (Faculty.XeroxcopyofcertificatesFlag == true)
                    //{
                    //    if (Reason != null)
                    //        Reason += ",Xerox copyof certificates";
                    //    else
                    //        Reason += "Xerox copyof certificates";
                    //}

                    //if (Faculty.NOrelevantUgFlag == true)
                    //{
                    //    if (Reason != null)
                    //        Reason += ",NO Relevant UG";
                    //    else
                    //        Reason += "NO Relevant UG";
                    //}

                    //if (Faculty.NOrelevantPgFlag == true)
                    //{
                    //    if (Reason != null)
                    //        Reason += ",NO Relevant PG";
                    //    else
                    //        Reason += "NO Relevant PG";
                    //}

                    //if (Faculty.NOrelevantPhdFlag == true)
                    //{
                    //    if (Reason != null)
                    //        Reason += ",NO Relevant PHD";
                    //    else
                    //        Reason += "NO Relevant PHD";
                    //}

                    //if (Faculty.NOTQualifiedAsPerAICTE == true)
                    //{
                    //    if (Reason != null)
                    //        Reason += ",NOT Qualified AsPerAICTE";
                    //    else
                    //        Reason += "NOT Qualified AsPerAICTE";
                    //}

                    //if (Faculty.InvalidPANNo == true)
                    //{
                    //    if (Reason != null)
                    //        Reason += ",InvalidPANNumber";
                    //    else
                    //        Reason += "InvalidPANNumber";
                    //}

                    //if (Faculty.InCompleteCeritificates == true)
                    //{
                    //    if (Reason != null)
                    //        Reason += ",InComplete Ceritificates";
                    //    else
                    //        Reason += "InComplete Ceritificates";
                    //}

                    if (Faculty.NoSCM == true)
                    {
                        if (Reason != null)
                            Reason += ",NoSCM";
                        else
                            Reason += "NoSCM";
                    }

                    //if (Faculty.OriginalCertificatesnotshownFlag == true)
                    //{
                    //    if (Reason != null)
                    //        Reason += ",Original Certificates notshown";
                    //    else
                    //        Reason += "Original Certificates notshown";
                    //}

                    //if (Faculty.PANNumber == null)
                    //{
                    //    if (Reason != null)
                    //        Reason += ",No PANNumber";
                    //    else
                    //        Reason += "No PANNumber";
                    //}

                    //if (Faculty.NotIdentityFiedForAnyProgramFlag == true)
                    //{
                    //    if (Reason != null)
                    //        Reason += ",NotIdentityFied ForAnyProgram";
                    //    else
                    //        Reason += "NotIdentityFied ForAnyProgram";
                    //}

                    //if (Faculty.SamePANUsedByMultipleFaculty == true)
                    //{
                    //    if (Reason != null)
                    //        Reason += ",SamePANUsedByMultipleFaculty";
                    //    else
                    //        Reason += "SamePANUsedByMultipleFaculty";
                    //}

                    //if (Faculty.MultipleReginSamecoll == true)
                    //{
                    //    if (Reason != null)
                    //        Reason += ",No Class in UG/PG";
                    //    else
                    //        Reason += "No Class in UG/PG";
                    //}

                    //if (Faculty.Basstatus == "Yes")
                    //{
                    //    if (Reason != null)
                    //        Reason += ",No/Invalid Aadhaar Document";
                    //    else
                    //        Reason += "No/Invalid Aadhaar Document";
                    //}

                    //if (Faculty.BasstatusOld == "Yes")
                    //{
                    //    if (Reason != null)
                    //        Reason += ",BAS Flag";
                    //    else
                    //        Reason += "BAS Flag";
                    //}

                    //if (Faculty.OriginalsVerifiedUG == true)
                    //{
                    //    if (Reason != null)
                    //        Reason += ",Complaint PHD Faculty";
                    //    else
                    //        Reason += "Complaint PHD Faculty";
                    //}

                    //if (Faculty.OriginalsVerifiedPHD == true)
                    //{
                    //    if (Reason != null)
                    //        Reason += ",No Guide Sign in PHD Thesis";
                    //    else
                    //        Reason += "No Guide Sign in PHD Thesis";
                    //}

                    if (Faculty.BlacklistFaculty == true)
                    {
                        if (Reason != null)
                            Reason += ",BlackList";
                        else
                            Reason += "BlackList";
                    }

                    if (Faculty.VerificationStatus == true)
                    {
                        if (Reason != null)
                            Reason += ",Not Attend For Physical Verification";
                        else
                            Reason += "Not Attend For Physical Verification";
                    }

                    Faculty.DeactivationReason = Reason == null ? null : Reason;

                    Faculty.SpecializationId = jntuh_college_faculty_registered.Where(e => e.RegistrationNumber == Faculty.RegistrationNumber).Select(e => e.SpecializationId).FirstOrDefault();
                    Faculty.SpecializationName = Faculty.SpecializationId != null ? jntuh_specialization.Where(e => e.id == Faculty.SpecializationId).Select(e => e.specializationName).FirstOrDefault() : null;
                    Faculty.IdentfiedFor = jntuh_college_faculty_registered.Where(e => e.RegistrationNumber == Faculty.RegistrationNumber).Select(e => e.IdentifiedFor).FirstOrDefault();
                    Faculty.PGSpecialization = item.Jntu_PGSpecializationId;
                    Faculty.PGSpecializationName = Faculty.PGSpecialization != null ? jntuh_specialization.Where(e => e.id == Faculty.PGSpecialization).Select(e => e.specializationName).FirstOrDefault() : null;

                    Faculty.PHD = jntuh_registered_faculty_education.Where(E => E.educationId == 6 && E.facultyId == item.id).Select(E => E.courseStudied).FirstOrDefault();
                    Faculty.CollegeId = collegeId;
                    Faculty.CollegeName = jntuh_college.collegeName;
                    Faculty.isVerified = false;

                    teachingFaculty.Add(Faculty);

                }
                return View(teachingFaculty.Where(e => e.DeactivationReason == null).ToList());
            }
            else if (collegeId != null && DegreeId != null && GroupId != null)
            {
                TempData["ViewCollegeId"] = collegeId;
                string CollegeidNew1 = collegeId.ToString();

                int?[] Group_Specializationids = new int?[] { };
                if (GroupId == 1)
                {
                    Group_Specializationids = new int?[] { 115, 119, 172, 120, 118 };
                }
                else if (GroupId == 2)
                {
                    Group_Specializationids = new int?[] { 116, 167, 123, 170, 117, 124 };
                }
                else if (GroupId == 3)
                {
                    Group_Specializationids = new int?[] { 18, 19, 114, 169, 122 };
                }
                else if (GroupId == 4)
                {
                    Group_Specializationids = new int?[] { 172, 117, 121 };
                }

                var jntuh_specialization = db.jntuh_specialization.Where(e => e.isActive == true).ToList();

                var jntuh_college_intake_existing = db.jntuh_college_intake_existing.Where(e => e.collegeId == collegeId).Select(e => e).ToList();
                var jntuh_college_intake_existing_Data = jntuh_college_intake_existing.Where(e => e.academicYearId == AY1 && e.proposedIntake != 0 && e.courseStatus != "Closure").Select(w => w).ToList();
                var SpecializationIds = jntuh_college_intake_existing_Data.Select(e => e.specializationId).Distinct().ToArray();
                var Degrees = (from e in jntuh_college_intake_existing_Data
                               join s in db.jntuh_specialization on e.specializationId equals s.id
                               join d in db.jntuh_department on s.departmentId equals d.id
                               join de in db.jntuh_degree on d.degreeId equals de.id
                               where SpecializationIds.Contains(s.id) && (de.id == 2 || de.id == 5 || de.id == 9 || de.id == 10)
                               select new
                               {
                                   DegreeId = de.id,
                                   Degreename = de.degree
                               }).Distinct().ToList();

                ViewBag.Degrees = Degrees;

                //BpharmacyIntake
                int? BpharmacyProposedintake = jntuh_college_intake_existing.Where(e => e.academicYearId == AY1 && e.specializationId == 12).Select(e => e.proposedIntake).FirstOrDefault();
                int? Bpharmacyintake1 = jntuh_college_intake_existing.Where(e => e.academicYearId == AY2 && e.specializationId == 12).Select(e => e.approvedIntake).FirstOrDefault();
                int? Bpharmacyintake2 = jntuh_college_intake_existing.Where(e => e.academicYearId == AY3 && e.specializationId == 12).Select(e => e.approvedIntake).FirstOrDefault();
                int? Bpharmacyintake3 = jntuh_college_intake_existing.Where(e => e.academicYearId == AY4 && e.specializationId == 12).Select(e => e.approvedIntake).FirstOrDefault();
                int? Bpharmacyintake4 = jntuh_college_intake_existing.Where(e => e.academicYearId == AY5 && e.specializationId == 12).Select(e => e.approvedIntake).FirstOrDefault();
                int? Bpharmacyintake5 = jntuh_college_intake_existing.Where(e => e.academicYearId == AY6 && e.specializationId == 12).Select(e => e.approvedIntake).FirstOrDefault();

                ViewBag.BpharmacyProposedInatke = BpharmacyProposedintake;
                ViewBag.BpharmcySecondInatke = (Bpharmacyintake1);
                ViewBag.BpharmcythirdInatke = (Bpharmacyintake2);
                ViewBag.BpharmcyfouthInatke = (Bpharmacyintake3);
                ViewBag.BpharmcyfifthInatke = (Bpharmacyintake4);
                ViewBag.BpharmcySixthInatke = (Bpharmacyintake5);

                //Pharm D Intake
                int? PharmDProposedintake = jntuh_college_intake_existing.Where(e => e.academicYearId == AY1 && e.specializationId == 18).Select(e => e.proposedIntake).FirstOrDefault();
                int? PharmDintake1 = jntuh_college_intake_existing.Where(e => e.academicYearId == AY2 && e.specializationId == 18).Select(e => e.approvedIntake).FirstOrDefault();
                int? PharmDintake2 = jntuh_college_intake_existing.Where(e => e.academicYearId == AY3 && e.specializationId == 18).Select(e => e.approvedIntake).FirstOrDefault();
                int? PharmDintake3 = jntuh_college_intake_existing.Where(e => e.academicYearId == AY4 && e.specializationId == 18).Select(e => e.approvedIntake).FirstOrDefault();
                int? PharmDintake4 = jntuh_college_intake_existing.Where(e => e.academicYearId == AY5 && e.specializationId == 18).Select(e => e.approvedIntake).FirstOrDefault();
                int? PharmDintake5 = jntuh_college_intake_existing.Where(e => e.academicYearId == AY6 && e.specializationId == 18).Select(e => e.approvedIntake).FirstOrDefault();

                ViewBag.PharmDProposedInatke = PharmDProposedintake;
                ViewBag.PharmDySecondInatke = (PharmDintake1);
                ViewBag.PharmDthirdInatke = (PharmDintake2);
                ViewBag.PharmDfouthInatke = (PharmDintake3);
                ViewBag.PharmDfifthInatke = (PharmDintake4);
                ViewBag.PharmDSixthInatke = (PharmDintake5);

                //Pharm D.PB Intake
                int? PharmDPBProposedintake = jntuh_college_intake_existing.Where(e => e.academicYearId == AY1 && e.specializationId == 19).Select(e => e.proposedIntake).FirstOrDefault();
                int? PharmDPBintake1 = jntuh_college_intake_existing.Where(e => e.academicYearId == AY2 && e.specializationId == 19).Select(e => e.approvedIntake).FirstOrDefault();
                int? PharmDPBintake2 = jntuh_college_intake_existing.Where(e => e.academicYearId == AY3 && e.specializationId == 19).Select(e => e.approvedIntake).FirstOrDefault();
                int? PharmDPBintake3 = jntuh_college_intake_existing.Where(e => e.academicYearId == AY4 && e.specializationId == 19).Select(e => e.approvedIntake).FirstOrDefault();
                int? PharmDPBintake4 = jntuh_college_intake_existing.Where(e => e.academicYearId == AY5 && e.specializationId == 19).Select(e => e.approvedIntake).FirstOrDefault();
                int? PharmDPBintake5 = jntuh_college_intake_existing.Where(e => e.academicYearId == AY6 && e.specializationId == 19).Select(e => e.approvedIntake).FirstOrDefault();

                ViewBag.PharmDPBProposedInatke = PharmDPBProposedintake;
                ViewBag.PharmDPBySecondInatke = (PharmDPBintake1);
                ViewBag.PharmDPBthirdInatke = (PharmDPBintake2);
                ViewBag.PharmDPBfouthInatke = (PharmDPBintake3);
                ViewBag.PharmDPBfifthInatke = (PharmDPBintake4);
                ViewBag.PharmDPBSixthInatke = (PharmDPBintake5);


                var jntuh_college = jntuhcollege.Where(e => e.id == collegeId).Select(e => e).FirstOrDefault();
                var teachingFaculty = new List<FacultyRegistration>();
                var jntuh_college_faculty_registered = db.jntuh_college_faculty_registered.Where(e => e.collegeId == collegeId && (e.DepartmentId == 26 || e.DepartmentId == 36 || e.DepartmentId == 27 || e.DepartmentId == 39 || e.DepartmentId == 61)).Select(e => e).ToList();
                var jntuh_college_faculty_registered_new = jntuh_college_faculty_registered;
                var jntuh_college_prinicipal_registered = db.jntuh_college_principal_registered.Where(c => c.collegeId == collegeId).Select(e => e.RegistrationNumber).FirstOrDefault();

                ViewBag.Prinicipal = jntuh_college_prinicipal_registered;
                if (DegreeId == 5)
                {

                    //jntuh_college_faculty_registered = jntuh_college_faculty_registered.Where(e => Group_Specializationids.Contains(e.FacultySpecializationId)).Select(e => e).ToList();

                    List<SelectListItem> Groups = new List<SelectListItem>();
                    Groups.Add(new SelectListItem { Text = "Group1", Value = "1" });
                    Groups.Add(new SelectListItem { Text = "Group2", Value = "2" });
                    Groups.Add(new SelectListItem { Text = "Group3", Value = "3" });
                    Groups.Add(new SelectListItem { Text = "Group4", Value = "4" });
                    ViewBag.Groups = Groups;
                    if (TotalRequiredFaculty != null)
                        TempData["RequiredFaculty"] = TotalRequiredFaculty;
                    else
                        TempData["RequiredFaculty"] = null;

                    // if (SpecializationWiseFaculty != null)
                    TempData["SpecializationFaculty"] = null;
                    //else
                    //    TempData["SpecializationFaculty"] = null;
                }
                else
                {
                    //jntuh_college_faculty_registered = jntuh_college_faculty_registered.Select(e => e).ToList();

                    var jntuh_college_intake_existing_new = db.jntuh_college_intake_existing.Where(e => e.collegeId == collegeId && e.academicYearId == AY1 && e.proposedIntake != 0 && e.courseStatus != "Closure").Select(e => e).ToList();
                    int[] SpecializationIdsNew = jntuh_college_intake_existing_new.Select(e => e.specializationId).Distinct().ToArray();
                    var jntuh_degree = db.jntuh_degree.Where(e => e.isActive == true).ToList();
                    var jntuh_department = db.jntuh_department.Where(e => e.isActive == true).ToList();
                    var jntuh_specialization_New = db.jntuh_specialization.Where(e => e.isActive == true).ToList();

                    var Spec = (from e in jntuh_college_intake_existing_new
                                join s in jntuh_specialization on e.specializationId equals s.id
                                join d in jntuh_department on s.departmentId equals d.id
                                join de in jntuh_degree on d.degreeId equals de.id
                                where SpecializationIdsNew.Contains(s.id) && de.id == DegreeId
                                select new
                                {
                                    GroupId = s.id,
                                    GroupName = s.specializationName
                                }).Distinct().ToList();
                    ViewBag.Groups = Spec;

                    TempData["RequiredFaculty"] = 2;

                    TempData["SpecializationFaculty"] = 2;

                    ViewBag.AfterCollegeSelect = GroupId;
                }

                var strRegnos = jntuh_college_faculty_registered.Select(e => e.RegistrationNumber).ToList();

                var jntuh_registered_faculty_New = db.jntuh_registered_faculty.Where(e => strRegnos.Contains(e.RegistrationNumber.Trim())).Select(e => e).ToList();

                //var jntuh_registered_faculty = jntuh_registered_faculty_New.Where(rf => rf.type != "Adjunct" && ((rf.NoForm16 == false || rf.NoForm16 == null) && (rf.NoForm26AS == false || rf.NoForm26AS == null) && (rf.Absent == false || rf.Absent == null) && (rf.OriginalCertificatesNotShown == false || rf.OriginalCertificatesNotShown == null) && (rf.Xeroxcopyofcertificates == false || rf.Xeroxcopyofcertificates == null) && (rf.NotQualifiedAsperAICTE == false || rf.NotQualifiedAsperAICTE == null)
                //                                       && (rf.NoSCM == false || rf.NoSCM == null) && (rf.PANNumber != null) && (rf.IncompleteCertificates == false || rf.IncompleteCertificates == null) && (rf.Blacklistfaculy == false) && rf.NoRelevantUG != "Yes" && rf.NoRelevantPG != "Yes" && rf.NORelevantPHD != "Yes"
                //                                       && (rf.InvalidPANNumber == false || rf.InvalidPANNumber == null) && (rf.OriginalsVerifiedPHD != true) && (rf.OriginalsVerifiedUG != true) && (rf.Noclass == false || rf.Noclass == null) && (rf.FakePHD == false || rf.FakePHD == null) && (rf.Genuinenessnotsubmitted == false || rf.Genuinenessnotsubmitted == null) && (rf.NoPGspecialization == false || rf.NoPGspecialization == null) && (rf.Invaliddegree == false || rf.Invaliddegree == null) && rf.BAS != "Yes" && (rf.AbsentforVerification == false || rf.AbsentforVerification == null)) && rf.InvalidAadhaar != "Yes").Select(rf => rf).ToList();
                //var jntuh_registered_faculty = jntuh_registered_faculty_New.Where(rf => rf.type != "Adjunct" && ((rf.NotQualifiedAsperAICTE == false || rf.NotQualifiedAsperAICTE == null) && (rf.Noclass == false || rf.Noclass == null) && (rf.Absent == false || rf.Absent == null) && (rf.OriginalCertificatesNotShown == false || rf.OriginalCertificatesNotShown == null) &&
                //                                         (rf.NoSCM == false || rf.NoSCM == null) && rf.NoRelevantUG != "Yes" && rf.NoRelevantPG != "Yes" && rf.NORelevantPHD != "Yes" && (rf.InvalidPANNumber == false || rf.InvalidPANNumber == null) && (rf.Blacklistfaculy == false) && (rf.AbsentforVerification == false || rf.AbsentforVerification == null))).Select(rf => rf).ToList();
                var jntuh_registered_faculty = jntuh_registered_faculty_New.Where(rf => rf.type != "Adjunct" && (rf.BAS != "Yes") && (rf.Noclass == false || rf.Noclass == null) && (rf.Absent == false || rf.Absent == null) &&
                                                            (rf.NoSCM == false || rf.NoSCM == null) && (rf.Blacklistfaculy == false) && (rf.AbsentforVerification == false || rf.AbsentforVerification == null)).Select(rf => rf).ToList();
                var jntuh_registered_facultyBAS = jntuh_registered_faculty_New.Where(rf => (rf.BAS == "Yes")).Select(rf => new
                {
                    FacultyId = rf.id,
                    RegistrationNumber = rf.RegistrationNumber.Trim(),
                    BASFlag = rf.BAS
                }).ToList();
                var BASRegNos = jntuh_registered_facultyBAS.Select(rf => rf.RegistrationNumber).ToArray();

                string CollegeidNew = collegeId.ToString();
                var jntuh_appeal_pharmacydata = db.jntuh_appeal_pharmacydata.Where(e => e.CollegeCode == CollegeidNew && (e.Deficiency == null || !BASRegNos.Contains(e.Deficiency))).Select(e => e).ToList();
                var RegNumbers = jntuh_appeal_pharmacydata.Select(e => e.Deficiency).ToList();

                jntuh_college_faculty_registered_new = jntuh_college_faculty_registered_new.Where(e => !RegNumbers.Contains(e.RegistrationNumber)).Select(e => e).ToList();

                if (DegreeId == 5)
                {
                    jntuh_registered_faculty = jntuh_registered_faculty.Where(q => Group_Specializationids.Contains(q.Jntu_PGSpecializationId)).Select(a => a).ToList();
                }
                jntuh_registered_faculty = jntuh_registered_faculty.Where(e => !RegNumbers.Contains(e.RegistrationNumber)).Select(e => e).ToList();

                //var jntuh_appeal_pharmacy_data = db.jntuh_appeal_pharmacydata.Where(e => e.CollegeCode == StringCollegeId).Select(e => e).ToList();

                ViewBag.Group1Required = jntuh_appeal_pharmacydata.Where(e => e.PharmacySpecialization == Group1).Select(e => e.SpecializationWiseRequiredFaculty).FirstOrDefault() == null ? 0 : jntuh_appeal_pharmacydata.Where(e => e.PharmacySpecialization == Group1).Select(e => e.SpecializationWiseRequiredFaculty).LastOrDefault();
                ViewBag.Group2Required = jntuh_appeal_pharmacydata.Where(e => e.PharmacySpecialization == Group2).Select(e => e.SpecializationWiseRequiredFaculty).FirstOrDefault() == null ? 0 : jntuh_appeal_pharmacydata.Where(e => e.PharmacySpecialization == Group2).Select(e => e.SpecializationWiseRequiredFaculty).LastOrDefault(); ;
                ViewBag.Group3Required = jntuh_appeal_pharmacydata.Where(e => e.PharmacySpecialization == Group3).Select(e => e.SpecializationWiseRequiredFaculty).FirstOrDefault() == null ? 0 : jntuh_appeal_pharmacydata.Where(e => e.PharmacySpecialization == Group3).Select(e => e.SpecializationWiseRequiredFaculty).LastOrDefault(); ;
                ViewBag.Group4Required = jntuh_appeal_pharmacydata.Where(e => e.PharmacySpecialization == Group4).Select(e => e.SpecializationWiseRequiredFaculty).FirstOrDefault() == null ? 0 : jntuh_appeal_pharmacydata.Where(e => e.PharmacySpecialization == Group4).Select(e => e.SpecializationWiseRequiredFaculty).LastOrDefault(); ;

                var Group1Available = jntuh_appeal_pharmacydata.Where(e => e.PharmacySpecialization == Group1).Select(e => e).ToList();
                var Group2Available = jntuh_appeal_pharmacydata.Where(e => e.PharmacySpecialization == Group2).Select(e => e).ToList();
                var Group3Available = jntuh_appeal_pharmacydata.Where(e => e.PharmacySpecialization == Group3).Select(e => e).ToList();
                var Group4Available = jntuh_appeal_pharmacydata.Where(e => e.PharmacySpecialization == Group4).Select(e => e).ToList();

                ViewBag.Group1Available = Group1Available.Where(e => e.Deficiency != null).Select(e => e.Deficiency).Count();
                ViewBag.Group2Available = Group2Available.Where(e => e.Deficiency != null).Select(e => e.Deficiency).Count();
                ViewBag.Group3Available = Group3Available.Where(e => e.Deficiency != null).Select(e => e.Deficiency).Count();
                ViewBag.Group4Available = Group4Available.Where(e => e.Deficiency != null).Select(e => e.Deficiency).Count();

                //var Group1RemainingRegnos = jntuh_college_faculty_registered_new.Where(e => e.FacultySpecializationId == 115 || e.FacultySpecializationId == 119 || e.FacultySpecializationId == 172 || e.FacultySpecializationId == 120).Select(e => e.RegistrationNumber).ToList();
                //ViewBag.Group1RemainingFaculty = jntuh_registered_faculty.Where(e => Group1RemainingRegnos.Contains(e.RegistrationNumber)).Select(e => e.RegistrationNumber).Count();

                //var Group2RemainingRegnos = jntuh_college_faculty_registered_new.Where(e => e.FacultySpecializationId == 116 || e.FacultySpecializationId == 167 || e.FacultySpecializationId == 123 || e.FacultySpecializationId == 170 || e.FacultySpecializationId == 117).Select(e => e.RegistrationNumber).ToList();
                //ViewBag.Group2RemainingFaculty = jntuh_registered_faculty.Where(e => Group2RemainingRegnos.Contains(e.RegistrationNumber)).Select(e => e.RegistrationNumber).Count();

                //var Group3RemainingRegnos = jntuh_college_faculty_registered_new.Where(e => e.FacultySpecializationId == 169 || e.FacultySpecializationId == 18 || e.FacultySpecializationId == 19 || e.FacultySpecializationId == 114 || e.FacultySpecializationId == 122).Select(e => e.RegistrationNumber).ToList();
                //ViewBag.Group3RemainingFaculty = jntuh_registered_faculty.Where(e => Group3RemainingRegnos.Contains(e.RegistrationNumber)).Select(e => e.RegistrationNumber).Count();

                //var Group4RemainingRegnos = jntuh_college_faculty_registered_new.Where(e => e.FacultySpecializationId == 172 || e.FacultySpecializationId == 117 || e.FacultySpecializationId == 121).Select(e => e.RegistrationNumber).ToList();
                //ViewBag.Group4RemainingFaculty = jntuh_registered_faculty.Where(e => Group4RemainingRegnos.Contains(e.RegistrationNumber)).Select(e => e.RegistrationNumber).Count();

                var Group1RemainingRegnos = jntuh_registered_faculty.Where(e => e.Jntu_PGSpecializationId == 115 || e.Jntu_PGSpecializationId == 119 || e.Jntu_PGSpecializationId == 172 || e.Jntu_PGSpecializationId == 120 || e.Jntu_PGSpecializationId == 118).Select(e => e.RegistrationNumber).ToList();
                ViewBag.Group1RemainingFaculty = jntuh_registered_faculty.Where(e => Group1RemainingRegnos.Contains(e.RegistrationNumber)).Select(e => e.RegistrationNumber).Count();

                var Group2RemainingRegnos = jntuh_registered_faculty.Where(e => e.Jntu_PGSpecializationId == 116 || e.Jntu_PGSpecializationId == 167 || e.Jntu_PGSpecializationId == 123 || e.Jntu_PGSpecializationId == 170 || e.Jntu_PGSpecializationId == 117 || e.Jntu_PGSpecializationId == 124).Select(e => e.RegistrationNumber).ToList();
                ViewBag.Group2RemainingFaculty = jntuh_registered_faculty.Where(e => Group2RemainingRegnos.Contains(e.RegistrationNumber)).Select(e => e.RegistrationNumber).Count();

                var Group3RemainingRegnos = jntuh_registered_faculty.Where(e => e.Jntu_PGSpecializationId == 169 || e.Jntu_PGSpecializationId == 18 || e.Jntu_PGSpecializationId == 19 || e.Jntu_PGSpecializationId == 114 || e.Jntu_PGSpecializationId == 122).Select(e => e.RegistrationNumber).ToList();
                ViewBag.Group3RemainingFaculty = jntuh_registered_faculty.Where(e => Group3RemainingRegnos.Contains(e.RegistrationNumber)).Select(e => e.RegistrationNumber).Count();

                var Group4RemainingRegnos = jntuh_registered_faculty.Where(e => e.Jntu_PGSpecializationId == 172 || e.Jntu_PGSpecializationId == 117 || e.Jntu_PGSpecializationId == 121).Select(e => e.RegistrationNumber).ToList();
                ViewBag.Group4RemainingFaculty = jntuh_registered_faculty.Where(e => Group4RemainingRegnos.Contains(e.RegistrationNumber)).Select(e => e.RegistrationNumber).Count();



                var FacultyIds = jntuh_registered_faculty.Select(e => e.id).ToList();

                var jntuh_registered_faculty_education = db.jntuh_registered_faculty_education.Where(e => FacultyIds.Contains(e.facultyId)).Select(e => e).ToList();
                List<FacultyRegistration> data = new List<FacultyRegistration>();
                foreach (var item in jntuh_registered_faculty)
                {
                    string Reason = null;
                    FacultyRegistration Faculty = new FacultyRegistration();
                    Faculty.id = item.id;
                    Faculty.Type = item.type;
                    Faculty.RegistrationNumber = item.RegistrationNumber;
                    Faculty.UniqueID = item.UniqueID;
                    Faculty.FirstName = item.FirstName;
                    Faculty.MiddleName = item.MiddleName;
                    Faculty.LastName = item.LastName;
                    Faculty.GenderId = item.GenderId;
                    Faculty.Email = item.Email;
                    Faculty.facultyPhoto = item.Photo;
                    Faculty.Mobile = item.Mobile;
                    Faculty.PANNumber = item.PANNumber;
                    Faculty.AadhaarNumber = item.AadhaarNumber;
                    Faculty.DegreeId = jntuh_registered_faculty_education.Count(e => e.facultyId == item.id) > 0 ? item.jntuh_registered_faculty_education.Where(e => e.facultyId == item.id).Select(e => e.educationId).Max() : 0;
                    Faculty.PANNumber = item.PANNumber;

                    Faculty.Absent = item.Absent != null ? (bool)item.Absent : false;
                    Faculty.XeroxcopyofcertificatesFlag = item.Xeroxcopyofcertificates != null ? (bool)item.Xeroxcopyofcertificates : false;
                    Faculty.NOrelevantUgFlag = item.NoRelevantUG == "Yes" ? true : false;
                    Faculty.NOrelevantPgFlag = item.NoRelevantPG == "Yes" ? true : false;
                    Faculty.NOrelevantPhdFlag = item.NORelevantPHD == "Yes" ? true : false;
                    Faculty.NOTQualifiedAsPerAICTE = item.NotQualifiedAsperAICTE != null ? (bool)item.NotQualifiedAsperAICTE : false;
                    Faculty.InvalidPANNo = item.InvalidPANNumber != null ? (bool)item.InvalidPANNumber : false;
                    Faculty.InCompleteCeritificates = item.IncompleteCertificates == true ? true : false;
                    Faculty.NoSCM = item.NoSCM != null ? (bool)item.NoSCM : false;
                    Faculty.OriginalCertificatesnotshownFlag = item.OriginalCertificatesNotShown != null ? (bool)item.OriginalCertificatesNotShown : false;
                    Faculty.NotIdentityFiedForAnyProgramFlag = item.NotIdentityfiedForanyProgram != null ? (bool)item.NotIdentityfiedForanyProgram : false;
                    Faculty.Basstatus = item.InvalidAadhaar;
                    Faculty.BasstatusOld = item.BAS;
                    Faculty.OriginalsVerifiedUG = item.OriginalsVerifiedUG == true ? true : false;
                    Faculty.OriginalsVerifiedPHD = item.OriginalsVerifiedPHD == true ? true : false;
                    Faculty.BlacklistFaculty = item.Blacklistfaculy == true ? true : false;
                    Faculty.VerificationStatus = item.AbsentforVerification == true ? true : false;

                    //New Flags 
                    Faculty.InvalidDegree = item.Invaliddegree == true ? true : false;
                    Faculty.NoClass = item.Noclass == true ? true : false;
                    Faculty.GenuinenessnotSubmitted = item.Genuinenessnotsubmitted == true ? true : false;
                    Faculty.FakePhd = item.FakePHD == true ? true : false;
                    Faculty.NoPgSpecialization = item.NoPGspecialization == true ? true : false;
                    Faculty.NotconsiderPhd = item.NotconsideredPHD == true ? true : false;

                    Faculty.NoSCM17Flag = item.NoSCM17 != null ? (bool)item.NoSCM17 : false;
                    Faculty.PhdUndertakingDocumentstatus = item.PhdUndertakingDocumentstatus != null ? (bool)(item.PhdUndertakingDocumentstatus) : false;
                    Faculty.PHDUndertakingDocumentView = item.PHDUndertakingDocument;
                    Faculty.PhdUndertakingDocumentText = item.PhdUndertakingDocumentText;

                    Faculty.Deactivedby = item.DeactivatedBy;
                    Faculty.DeactivedOn = item.DeactivatedOn;

                    Faculty.DegreeId = jntuh_registered_faculty_education.Count(e => e.facultyId == item.id) > 0 ? item.jntuh_registered_faculty_education.Where(e => e.facultyId == item.id).Select(e => e.educationId).Max() : 0;

                    if (Faculty.Absent == true)
                        Reason += "Absent";

                    if (Faculty.Type == "Adjunct")
                    {
                        if (Reason != null)
                            Reason += ",Adjunct Faculty";
                        else
                            Reason += "Adjunct Faculty";
                    }

                    if (Faculty.NoClass == true)
                    {
                        if (Reason != null)
                            Reason += ",No Class";
                        else
                            Reason += "No Class";
                    }

                    //if (Faculty.XeroxcopyofcertificatesFlag == true)
                    //{
                    //    if (Reason != null)
                    //        Reason += ",Xerox copyof certificates";
                    //    else
                    //        Reason += "Xerox copyof certificates";
                    //}

                    //if (Faculty.NOrelevantUgFlag == true)
                    //{
                    //    if (Reason != null)
                    //        Reason += ",NO Relevant UG";
                    //    else
                    //        Reason += "NO Relevant UG";
                    //}

                    //if (Faculty.NOrelevantPgFlag == true)
                    //{
                    //    if (Reason != null)
                    //        Reason += ",NO Relevant PG";
                    //    else
                    //        Reason += "NO Relevant PG";
                    //}

                    //if (Faculty.NOrelevantPhdFlag == true)
                    //{
                    //    if (Reason != null)
                    //        Reason += ",NO Relevant PHD";
                    //    else
                    //        Reason += "NO Relevant PHD";
                    //}

                    //if (Faculty.NOTQualifiedAsPerAICTE == true)
                    //{
                    //    if (Reason != null)
                    //        Reason += ",NOT Qualified AsPerAICTE";
                    //    else
                    //        Reason += "NOT Qualified AsPerAICTE";
                    //}

                    //if (Faculty.InvalidPANNo == true)
                    //{
                    //    if (Reason != null)
                    //        Reason += ",InvalidPANNumber";
                    //    else
                    //        Reason += "InvalidPANNumber";
                    //}

                    //if (Faculty.InCompleteCeritificates == true)
                    //{
                    //    if (Reason != null)
                    //        Reason += ",InComplete Ceritificates";
                    //    else
                    //        Reason += "InComplete Ceritificates";
                    //}

                    if (Faculty.NoSCM == true)
                    {
                        if (Reason != null)
                            Reason += ",NoSCM";
                        else
                            Reason += "NoSCM";
                    }

                    //if (Faculty.OriginalCertificatesnotshownFlag == true)
                    //{
                    //    if (Reason != null)
                    //        Reason += ",Original Certificates notshown";
                    //    else
                    //        Reason += "Original Certificates notshown";
                    //}

                    //if (Faculty.PANNumber == null)
                    //{
                    //    if (Reason != null)
                    //        Reason += ",No PANNumber";
                    //    else
                    //        Reason += "No PANNumber";
                    //}

                    //if (Faculty.NotIdentityFiedForAnyProgramFlag == true)
                    //{
                    //    if (Reason != null)
                    //        Reason += ",NotIdentityFied ForAnyProgram";
                    //    else
                    //        Reason += "NotIdentityFied ForAnyProgram";
                    //}

                    //if (Faculty.SamePANUsedByMultipleFaculty == true)
                    //{
                    //    if (Reason != null)
                    //        Reason += ",SamePANUsedByMultipleFaculty";
                    //    else
                    //        Reason += "SamePANUsedByMultipleFaculty";
                    //}

                    //if (Faculty.MultipleReginSamecoll == true)
                    //{
                    //    if (Reason != null)
                    //        Reason += ",No Class in UG/PG";
                    //    else
                    //        Reason += "No Class in UG/PG";
                    //}

                    //if (Faculty.Basstatus == "Yes")
                    //{
                    //    if (Reason != null)
                    //        Reason += ",No/Invalid Aadhaar Document";
                    //    else
                    //        Reason += "No/Invalid Aadhaar Document";
                    //}

                    //if (Faculty.BasstatusOld == "Yes")
                    //{
                    //    if (Reason != null)
                    //        Reason += ",BAS Flag";
                    //    else
                    //        Reason += "BAS Flag";
                    //}

                    //if (Faculty.OriginalsVerifiedUG == true)
                    //{
                    //    if (Reason != null)
                    //        Reason += ",Complaint PHD Faculty";
                    //    else
                    //        Reason += "Complaint PHD Faculty";
                    //}

                    //if (Faculty.OriginalsVerifiedPHD == true)
                    //{
                    //    if (Reason != null)
                    //        Reason += ",No Guide Sign in PHD Thesis";
                    //    else
                    //        Reason += "No Guide Sign in PHD Thesis";
                    //}

                    if (Faculty.BlacklistFaculty == true)
                    {
                        if (Reason != null)
                            Reason += ",BlackList";
                        else
                            Reason += "BlackList";
                    }

                    if (Faculty.VerificationStatus == true)
                    {
                        if (Reason != null)
                            Reason += ",Not Attend For Physical Verification";
                        else
                            Reason += "Not Attend For Physical Verification";
                    }

                    Faculty.DeactivationReason = Reason;

                    Faculty.SpecializationId = jntuh_college_faculty_registered.Where(e => e.RegistrationNumber == Faculty.RegistrationNumber).Select(e => e.SpecializationId).FirstOrDefault();
                    Faculty.SpecializationName = Faculty.SpecializationId != null ? jntuh_specialization.Where(e => e.id == Faculty.SpecializationId).Select(e => e.specializationName).FirstOrDefault() : null;
                    Faculty.IdentfiedFor = jntuh_college_faculty_registered.Where(e => e.RegistrationNumber == Faculty.RegistrationNumber).Select(e => e.IdentifiedFor).FirstOrDefault();
                    Faculty.PGSpecialization = item.Jntu_PGSpecializationId;
                    Faculty.PGSpecializationName = Faculty.PGSpecialization != null ? jntuh_specialization.Where(e => e.id == Faculty.PGSpecialization).Select(e => e.specializationName).FirstOrDefault() : null;

                    Faculty.PHD = jntuh_registered_faculty_education.Where(E => E.educationId == 6 && E.facultyId == item.id).Select(E => E.courseStudied).FirstOrDefault();
                    Faculty.CollegeId = collegeId;
                    Faculty.CollegeName = jntuh_college.collegeName;
                    Faculty.isVerified = false;

                    teachingFaculty.Add(Faculty);
                }
                return View(teachingFaculty.Where(e => e.DeactivationReason == null).ToList());
            }
            else
            {
                return View();
            }
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin,SuperAdmin,DataEntry,FacultyVerification")]
        public ActionResult PharmacyFacultyGroupingbasedonColleges(ICollection<FacultyRegistration> Faculty, int? collegeId, int? DegreeId, int? GroupId, int? TotalRequiredFaculty, int? SpecializationWiseFaculty)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            // int? CollegeID = collegeId;
            var CollegeId = collegeId;
            if (DegreeId != null && GroupId != null && TotalRequiredFaculty != null && SpecializationWiseFaculty != null)
            {
                TempData["RequiredFaculty"] = TotalRequiredFaculty;
                var FacultyData = new List<FacultyRegistration>();
                if (Faculty == null)
                {

                }
                else
                {
                    FacultyData = Faculty.Where(e => e.isVerified == true).Select(e => e).ToList();
                }

                var jntuh_department = db.jntuh_department.Where(e => e.isActive == true).Select(e => e).ToList();
                var jntuh_specialization = db.jntuh_specialization.Where(e => e.isActive == true).Select(e => e).ToList();
                var jntuh_college_intake_existing = db.jntuh_college_intake_existing.Where(e => e.collegeId == CollegeId && e.proposedIntake != 0 && e.courseStatus != "Closure").Select(e => e).ToList();

                int BPharmacyDepartmentId = 26;
                int BPharmacySpecializationId = 12;
                int? Proposedintake = 0;
                int? intake1 = 0;
                int? intake2 = 0;
                int? intake3 = 0;
                int? intake4 = 0;
                int? intake5 = 0;
                int? intake6 = 0;
                var jntuh_academic_year = db.jntuh_academic_year.ToList();

                int presentYear = jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
                int AY1 = jntuh_academic_year.Where(a => a.actualYear == (presentYear + 1)).Select(a => a.id).FirstOrDefault();
                int AY2 = jntuh_academic_year.Where(a => a.actualYear == (presentYear)).Select(a => a.id).FirstOrDefault();
                int AY3 = jntuh_academic_year.Where(a => a.actualYear == (presentYear - 1)).Select(a => a.id).FirstOrDefault();
                int AY4 = jntuh_academic_year.Where(a => a.actualYear == (presentYear - 2)).Select(a => a.id).FirstOrDefault();
                int AY5 = jntuh_academic_year.Where(a => a.actualYear == (presentYear - 3)).Select(a => a.id).FirstOrDefault();
                int AY6 = jntuh_academic_year.Where(a => a.actualYear == (presentYear - 4)).Select(a => a.id).FirstOrDefault();

                if (DegreeId == 5)
                {
                    Proposedintake = jntuh_college_intake_existing.Where(e => e.academicYearId == AY1 && e.specializationId == 12).Select(e => e.proposedIntake).FirstOrDefault();
                    intake1 = jntuh_college_intake_existing.Where(e => e.academicYearId == AY2 && e.specializationId == 12).Select(e => e.approvedIntake).FirstOrDefault();
                    intake2 = jntuh_college_intake_existing.Where(e => e.academicYearId == AY3 && e.specializationId == 12).Select(e => e.approvedIntake).FirstOrDefault();
                    intake3 = jntuh_college_intake_existing.Where(e => e.academicYearId == AY4 && e.specializationId == 12).Select(e => e.approvedIntake).FirstOrDefault();
                    intake4 = 0;
                    intake5 = 0;
                    intake6 = 0;
                }
                else
                {
                    if (GroupId == 18)
                    {
                        Proposedintake = jntuh_college_intake_existing.Where(e => e.academicYearId == AY1 && e.specializationId == GroupId).Select(e => e.proposedIntake).FirstOrDefault();
                        intake1 = jntuh_college_intake_existing.Where(e => e.academicYearId == AY2 && e.specializationId == GroupId).Select(e => e.approvedIntake).FirstOrDefault();
                        intake2 = jntuh_college_intake_existing.Where(e => e.academicYearId == AY3 && e.specializationId == GroupId).Select(e => e.approvedIntake).FirstOrDefault();
                        intake3 = jntuh_college_intake_existing.Where(e => e.academicYearId == AY4 && e.specializationId == GroupId).Select(e => e.approvedIntake).FirstOrDefault();
                        intake4 = jntuh_college_intake_existing.Where(e => e.academicYearId == AY5 && e.specializationId == GroupId).Select(e => e.approvedIntake).FirstOrDefault();
                        intake5 = 0;
                        intake6 = 0;
                    }
                    else if (GroupId == 19)
                    {
                        Proposedintake = jntuh_college_intake_existing.Where(e => e.academicYearId == AY1 && e.specializationId == GroupId).Select(e => e.proposedIntake).FirstOrDefault();
                        intake1 = jntuh_college_intake_existing.Where(e => e.academicYearId == AY2 && e.specializationId == GroupId).Select(e => e.approvedIntake).FirstOrDefault();
                        intake2 = jntuh_college_intake_existing.Where(e => e.academicYearId == AY3 && e.specializationId == GroupId).Select(e => e.approvedIntake).FirstOrDefault();
                        intake3 = jntuh_college_intake_existing.Where(e => e.academicYearId == AY4 && e.specializationId == GroupId).Select(e => e.approvedIntake).FirstOrDefault();
                        intake4 = jntuh_college_intake_existing.Where(e => e.academicYearId == AY5 && e.specializationId == GroupId).Select(e => e.approvedIntake).FirstOrDefault();
                        intake5 = jntuh_college_intake_existing.Where(e => e.academicYearId == AY6 && e.specializationId == GroupId).Select(e => e.approvedIntake).FirstOrDefault();
                        intake6 = 0;
                    }
                    else
                    {
                        Proposedintake = jntuh_college_intake_existing.Where(e => e.academicYearId == AY1 && e.specializationId == GroupId).Select(e => e.proposedIntake).FirstOrDefault();
                        intake1 = jntuh_college_intake_existing.Where(e => e.academicYearId == AY2 && e.specializationId == GroupId).Select(e => e.approvedIntake).FirstOrDefault();
                        intake2 = 0;
                        intake3 = 0;
                        intake4 = 0;
                        intake5 = 0;
                        intake6 = 0;
                    }


                }

                if (FacultyData.Count() == 0 || FacultyData == null)
                {
                    jntuh_appeal_pharmacydata pharmacy = new jntuh_appeal_pharmacydata();
                    pharmacy.CollegeCode = CollegeId.ToString();
                    pharmacy.Department = jntuh_department.Where(e => e.degreeId == DegreeId).Select(e => e.id).FirstOrDefault().ToString();
                    pharmacy.NoOfFacultyRequired = TotalRequiredFaculty;
                    pharmacy.SpecializationWiseRequiredFaculty = TotalRequiredFaculty;
                    if (DegreeId == 5)
                    {
                        pharmacy.PharmacySpecialization = GroupId.ToString();
                        pharmacy.Specialization = BPharmacySpecializationId.ToString();
                        pharmacy.TotalIntake = (Proposedintake + intake1 + intake2 + intake3);
                    }
                    else
                    {
                        if (GroupId == 18)
                        {
                            pharmacy.TotalIntake = (Proposedintake + intake1 + intake2 + intake3 + intake4);
                        }
                        else if (GroupId == 19)
                        {
                            pharmacy.TotalIntake = (Proposedintake + intake1 + intake2 + intake3 + intake4 + intake5);
                        }
                        else
                        {
                            pharmacy.TotalIntake = (Proposedintake + intake1);
                        }
                        pharmacy.PharmacySpecialization = null;
                        pharmacy.Specialization = GroupId.ToString();

                    }
                    pharmacy.ProposedIntake = Proposedintake;
                    //pharmacy.TotalIntake = (Proposedintake + intake1 + intake2 + intake3 + intake4 + intake5 + intake6);
                    pharmacy.Deficiency = null;
                    pharmacy.CreatedOn = DateTime.Now;
                    pharmacy.IsActive = true;
                    db.jntuh_appeal_pharmacydata.Add(pharmacy);
                    db.SaveChanges();
                    TempData["Success"] = "Course Details Are Added Without Faculty";
                }
                else
                {
                    foreach (var item in FacultyData)
                    {

                        jntuh_appeal_pharmacydata pharmacy = new jntuh_appeal_pharmacydata();
                        pharmacy.CollegeCode = CollegeId.ToString();
                        pharmacy.Department = jntuh_department.Where(e => e.degreeId == DegreeId).Select(e => e.id).FirstOrDefault().ToString();
                        pharmacy.NoOfFacultyRequired = TotalRequiredFaculty;
                        pharmacy.SpecializationWiseRequiredFaculty = TotalRequiredFaculty;
                        if (DegreeId == 5)
                        {
                            pharmacy.PharmacySpecialization = GroupId.ToString();
                            pharmacy.Specialization = BPharmacySpecializationId.ToString();
                            pharmacy.TotalIntake = (Proposedintake + intake1 + intake2 + intake3);
                        }
                        else
                        {
                            if (GroupId == 18)
                            {
                                pharmacy.TotalIntake = (Proposedintake + intake1 + intake2 + intake3 + intake4);
                            }
                            else if (GroupId == 19)
                            {
                                pharmacy.TotalIntake = (Proposedintake + intake1 + intake2 + intake3 + intake4 + intake5);
                            }
                            else
                            {
                                pharmacy.TotalIntake = (Proposedintake + intake1);
                            }
                            pharmacy.PharmacySpecialization = null;
                            pharmacy.Specialization = GroupId.ToString();

                        }
                        pharmacy.ProposedIntake = Proposedintake;
                        //  pharmacy.TotalIntake = (intake1 + intake2 + intake3 + intake4 + intake5 + intake6);
                        pharmacy.Deficiency = item.RegistrationNumber;
                        pharmacy.CreatedOn = DateTime.Now;
                        pharmacy.IsActive = true;
                        db.jntuh_appeal_pharmacydata.Add(pharmacy);
                        db.SaveChanges();
                        TempData["Success"] = "Registration Details Are Added";
                    }
                }

                //if (FacultyData.Count() == 0)
                //{
                //    TempData["Error"] = "Please Select The Faculty";
                //}
                return RedirectToAction("PharmacyFacultyGroupingbasedonColleges", new { collegeId = CollegeId, DegreeId = DegreeId, GroupId = GroupId, totalRequiredFaculty = TotalRequiredFaculty });
            }

            return RedirectToAction("PharmacyFacultyGroupingbasedonColleges", new { collegeId = CollegeId });
        }

        [Authorize(Roles = "Admin,SuperAdmin,DataEntry,FacultyVerification")]
        public ActionResult ViewAddedFaculty(int? collegeId)
        {

            int userId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            var CollegeIds = new int[] { 448, 180, 159, 376, 428, 445, 24, 117, 202, 213, 395, 364, 27, 30, 6, 34, 55, 58, 54, 52, 9, 60, 65, 78, 150, 204, 219, 90, 97, 104, 110, 139, 105, 107, 114, 379, 118, 39, 120, 389, 127, 237, 135, 136, 47, 252, 169, 253, 262, 442, 301, 436, 370, 263, 267, 206, 140, 44, 290, 454, 42, 95, 297, 384, 75, 348, 298, 295, 303, 302, 283, 332, 410, 392, 234, 146, 315, 320, 317, 314, 318, 319, 353, 313, 235, 375 };
            var jntuhcollege = db.jntuh_college.AsNoTracking().Where(e => CollegeIds.Contains(e.id)).ToList();
            ViewBag.PharmacyCollegeList = jntuhcollege.Select(e => new
            {
                collegeId = e.id,
                collegeName = e.collegeCode + "-" + e.collegeName
            }).ToList();
            List<FacultyRegistration> Faculty = new List<FacultyRegistration>();
            if (collegeId != null)
            {
                string collegeid = collegeId.ToString();
                var jntuh_appeal_pharmacydata = db.jntuh_appeal_pharmacydata.Where(e => e.CollegeCode == collegeid).Select(e => e).ToList();
                var StrRegNos = jntuh_appeal_pharmacydata.Where(e => e.Deficiency != null).Select(e => e.Deficiency.Trim()).ToList();

                var jntuh_college_prinicipal_registered = db.jntuh_college_principal_registered.Where(e => e.collegeId == collegeId).Select(e => e.RegistrationNumber).FirstOrDefault();
                ViewBag.Prinicipal = jntuh_college_prinicipal_registered;

                var jntuh_college_faculty_registered = db.jntuh_college_faculty_registered.Where(e => StrRegNos.Contains(e.RegistrationNumber.Trim()) && e.collegeId == collegeId).Select(e => e).ToList();
                var jntuh_registered_faculty = db.jntuh_registered_faculty.Where(e => StrRegNos.Contains(e.RegistrationNumber.Trim())).Select(e => e).ToList();

                var jntuh_college_faculty_registered_all = db.jntuh_college_faculty_registered.Where(e => e.collegeId == collegeId).Select(e => e).ToList();
                var clgstrRegnos = jntuh_college_faculty_registered_all.Select(e => e.RegistrationNumber).ToList();
                var jntuh_registered_faculty_New = db.jntuh_registered_faculty.Where(e => clgstrRegnos.Contains(e.RegistrationNumber.Trim())).Select(e => e).ToList();
                //jntuh_registered_faculty = jntuh_registered_faculty.Where(rf => rf.type != "Adjunct" && ((rf.NotQualifiedAsperAICTE == false || rf.NotQualifiedAsperAICTE == null) && (rf.Noclass == false || rf.Noclass == null) && (rf.Absent == false || rf.Absent == null) && (rf.OriginalCertificatesNotShown == false || rf.OriginalCertificatesNotShown == null) &&
                //                                         (rf.NoSCM == false || rf.NoSCM == null) && rf.NoRelevantUG != "Yes" && rf.NoRelevantPG != "Yes" && rf.NORelevantPHD != "Yes" && (rf.InvalidPANNumber == false || rf.InvalidPANNumber == null) && (rf.Blacklistfaculy == false) && (rf.AbsentforVerification == false || rf.AbsentforVerification == null))).Select(rf => rf).ToList();
                jntuh_registered_faculty = jntuh_registered_faculty.Where(rf => rf.type != "Adjunct" && (rf.BAS != "Yes") && (rf.Noclass == false || rf.Noclass == null) && (rf.Absent == false || rf.Absent == null) &&
                                                            (rf.NoSCM == false || rf.NoSCM == null) && (rf.Blacklistfaculy == false) && (rf.AbsentforVerification == false || rf.AbsentforVerification == null)).Select(rf => rf).ToList();
                var jntuh_registered_facultyBAS = jntuh_registered_faculty_New.Where(rf => (rf.BAS == "Yes")).Select(rf => new
                {
                    FacultyId = rf.id,
                    RegistrationNumber = rf.RegistrationNumber.Trim(),
                    BASFlag = rf.BAS
                }).ToList();
                var BASRegNos = jntuh_registered_facultyBAS.Select(rf => rf.RegistrationNumber).ToArray();
                var FacultyStrRegNos = jntuh_registered_faculty.Select(e => e.RegistrationNumber).ToList();
                var FacultyIds = jntuh_registered_faculty.Select(e => e.id).ToList();
                var jntuh_faculty_registered_education = db.jntuh_registered_faculty_education.Where(e => FacultyIds.Contains(e.facultyId)).Select(e => e).ToList();

                var jntuh_specialization = db.jntuh_specialization.Where(e => e.isActive == true).ToList();
                var jntuh_department = db.jntuh_department.Where(e => e.isActive == true).ToList();

                jntuh_appeal_pharmacydata = jntuh_appeal_pharmacydata.Where(s => FacultyStrRegNos.Contains(s.Deficiency) && !BASRegNos.Contains(s.Deficiency)).ToList();
                foreach (var item in jntuh_appeal_pharmacydata)
                {
                    FacultyRegistration eachfaculty = new FacultyRegistration();
                    if (item.Deficiency == null)
                    {
                        eachfaculty.id = item.Sno;
                        eachfaculty.RegistrationNumber = item.Deficiency; ;
                        eachfaculty.FirstName = null;
                        eachfaculty.MiddleName = null;
                        eachfaculty.LastName = null;

                        eachfaculty.SpecializationId = null;
                        eachfaculty.SpecializationName = null;
                        eachfaculty.IdentfiedFor = null;
                        eachfaculty.PGSpecialization = null;
                        eachfaculty.PGSpecializationName = null;

                        eachfaculty.DepartmentId = Convert.ToInt32(item.Department);
                        eachfaculty.department = eachfaculty.DepartmentId != null ? jntuh_department.Where(e => e.id == eachfaculty.DepartmentId).Select(e => e.departmentName).FirstOrDefault() : null;
                        eachfaculty.DesignationId = Convert.ToInt32(item.Specialization);
                        eachfaculty.designation = eachfaculty.DesignationId == null ? null : jntuh_specialization.Where(e => e.id == eachfaculty.DesignationId).Select(e => e.specializationName).FirstOrDefault();
                        eachfaculty.Eid = item.PharmacySpecialization == null ? 0 : Convert.ToInt32(item.PharmacySpecialization);

                        eachfaculty.PHD = null;
                    }
                    else
                    {
                        //FacultyRegistration eachfaculty = new FacultyRegistration();
                        eachfaculty.id = item.Sno;
                        eachfaculty.RegistrationNumber = item.Deficiency.Trim();
                        eachfaculty.FirstName = jntuh_registered_faculty.Where(e => e.RegistrationNumber == item.Deficiency.Trim()).Select(e => e.FirstName).FirstOrDefault();
                        eachfaculty.MiddleName = jntuh_registered_faculty.Where(e => e.RegistrationNumber == item.Deficiency.Trim()).Select(e => e.MiddleName).FirstOrDefault();
                        eachfaculty.LastName = jntuh_registered_faculty.Where(e => e.RegistrationNumber == item.Deficiency.Trim()).Select(e => e.LastName).FirstOrDefault();

                        eachfaculty.SpecializationId = jntuh_college_faculty_registered.Where(e => e.RegistrationNumber.Trim() == item.Deficiency.Trim()).Select(e => e.SpecializationId).FirstOrDefault();
                        eachfaculty.SpecializationName = eachfaculty.SpecializationId == null ? null : jntuh_specialization.Where(e => e.id == eachfaculty.SpecializationId).Select(e => e.specializationName).FirstOrDefault();
                        eachfaculty.IdentfiedFor = jntuh_college_faculty_registered.Where(e => e.RegistrationNumber.Trim() == item.Deficiency.Trim()).Select(e => e.IdentifiedFor).FirstOrDefault();
                        eachfaculty.PGSpecialization = jntuh_registered_faculty.Where(e => e.RegistrationNumber.Trim() == item.Deficiency.Trim()).Select(e => e.Jntu_PGSpecializationId).FirstOrDefault();
                        eachfaculty.PGSpecializationName = eachfaculty.PGSpecialization == null ? null : jntuh_specialization.Where(e => e.id == eachfaculty.PGSpecialization).Select(e => e.specializationName).FirstOrDefault();

                        eachfaculty.DepartmentId = Convert.ToInt32(item.Department);
                        eachfaculty.department = eachfaculty.DepartmentId == null ? null : jntuh_department.Where(e => e.id == eachfaculty.DepartmentId).Select(e => e.departmentName).FirstOrDefault();
                        eachfaculty.DesignationId = Convert.ToInt32(item.Specialization);
                        eachfaculty.designation = eachfaculty.DesignationId == null ? null : jntuh_specialization.Where(e => e.id == eachfaculty.DesignationId).Select(e => e.specializationName).FirstOrDefault();
                        eachfaculty.Eid = item.PharmacySpecialization == null ? 0 : Convert.ToInt32(item.PharmacySpecialization);

                        var Notconsiderphd = jntuh_registered_faculty.Where(e => e.RegistrationNumber.Trim() == item.Deficiency.Trim()).Select(e => e.NotconsideredPHD).FirstOrDefault();
                        eachfaculty.NotconsiderPhd = Notconsiderphd == true ? true : false;
                        eachfaculty.PHD = jntuh_faculty_registered_education.Where(e => e.educationId == 6 && e.facultyId == jntuh_registered_faculty.Where(s => s.RegistrationNumber == item.Deficiency).Select(q => q.id).FirstOrDefault()).Select(w => w.courseStudied).FirstOrDefault();
                    }

                    Faculty.Add(eachfaculty);
                }
            }
            else
            {
                return View();
            }
            return View(Faculty.Where(e => e.RegistrationNumber != null).ToList());
        }

        [Authorize(Roles = "Admin,SuperAdmin,DataEntry,FacultyVerification")]
        public ActionResult EditPharmacyIntake(int? collegeId, int? DegreeId, int? GroupId)
        {
            var CollegeIds = new int[] { 448, 180, 159, 376, 428, 445, 24, 117, 202, 213, 395, 364, 27, 30, 6, 34, 55, 58, 54, 52, 9, 60, 65, 78, 150, 204, 219, 90, 97, 104, 110, 139, 105, 107, 114, 379, 118, 39, 120, 389, 127, 237, 135, 136, 47, 252, 169, 253, 262, 442, 301, 436, 370, 263, 267, 206, 140, 44, 290, 454, 42, 95, 297, 384, 75, 348, 298, 295, 303, 302, 283, 332, 410, 392, 234, 146, 315, 320, 317, 314, 318, 319, 353, 313, 235, 375 };
            var jntuhcollege = db.jntuh_college.AsNoTracking().Where(e => CollegeIds.Contains(e.id)).ToList();

            ViewBag.PharmacyCollegeList = jntuhcollege.Select(e => new
            {
                collegeId = e.id,
                collegeName = e.collegeCode + "-" + e.collegeName
            }).ToList();

            List<SelectListItem> DegreesFirst = new List<SelectListItem>();
            DegreesFirst.Add(new SelectListItem { Text = "---Select---", Value = "1" });
            ViewBag.Degrees = DegreesFirst.Select(w => new
            {
                DegreeId = w.Value,
                Degreename = w.Text
            }).ToList();

            List<SelectListItem> GroupsFirst = new List<SelectListItem>();
            GroupsFirst.Add(new SelectListItem { Text = "---Select---", Value = "1" });
            ViewBag.Groups = GroupsFirst.Select(e => new
            {
                GroupId = e.Value,
                GroupName = e.Text
            }).ToList();

            int presentYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int AY1 = db.jntuh_academic_year.Where(a => a.actualYear == (presentYear + 1)).Select(a => a.id).FirstOrDefault();

            var jntuh_department = db.jntuh_department.Where(e => e.isActive == true).Select(e => e).ToList();
            var jntuh_specialization = db.jntuh_specialization.Where(e => e.isActive == true).Select(e => e).ToList();
            if (collegeId != null && DegreeId == null && GroupId == null)
            {
                ViewBag.AfterCollegeSelect = null;

                var jntuh_college_intake_existing = db.jntuh_college_intake_existing.Where(e => e.collegeId == collegeId).Select(e => e).ToList();
                var jntuh_college_intake_existing_Data = jntuh_college_intake_existing.Where(e => e.academicYearId == AY1 && e.courseStatus != "Closure").Select(w => w).ToList();
                var SpecializationIds = jntuh_college_intake_existing_Data.Select(e => e.specializationId).Distinct().ToArray();
                var Degrees = (from e in jntuh_college_intake_existing_Data
                               join s in jntuh_specialization on e.specializationId equals s.id
                               join d in jntuh_department on s.departmentId equals d.id
                               join de in db.jntuh_degree on d.degreeId equals de.id
                               where SpecializationIds.Contains(s.id) && (de.id == 2 || de.id == 5 || de.id == 9 || de.id == 10)
                               select new
                               {
                                   DegreeId = de.id,
                                   Degreename = de.degree
                               }).Distinct().ToList();

                ViewBag.Degrees = Degrees;

                List<SelectListItem> Groups = new List<SelectListItem>();
                Groups.Add(new SelectListItem { Text = "Group1", Value = "1" });
                Groups.Add(new SelectListItem { Text = "Group2", Value = "2" });
                Groups.Add(new SelectListItem { Text = "Group3", Value = "3" });
                Groups.Add(new SelectListItem { Text = "Group4", Value = "4" });
                ViewBag.Groups = Groups.Select(e => new
                {
                    GroupId = e.Value,
                    GroupName = e.Text
                }).ToList();

                string CollegeID = collegeId.ToString();
                var jntuh_college_faculty_registered_all = db.jntuh_college_faculty_registered.Where(e => e.collegeId == collegeId).Select(e => e).ToList();
                var clgstrRegnos = jntuh_college_faculty_registered_all.Select(e => e.RegistrationNumber).ToList();
                var jntuh_registered_faculty_New = db.jntuh_registered_faculty.Where(e => clgstrRegnos.Contains(e.RegistrationNumber.Trim())).Select(e => e).ToList();
                var jntuh_registered_facultyBAS = jntuh_registered_faculty_New.Where(rf => (rf.BAS == "Yes")).Select(rf => new
                {
                    FacultyId = rf.id,
                    RegistrationNumber = rf.RegistrationNumber.Trim(),
                    BASFlag = rf.BAS
                }).ToList();
                var BASRegNos = jntuh_registered_facultyBAS.Select(rf => rf.RegistrationNumber).ToArray();
                var jntuh_appeal_pharmacyData = db.jntuh_appeal_pharmacydata.Where(e => e.CollegeCode == CollegeID && (e.Deficiency == null || !BASRegNos.Contains(e.Deficiency))).Select(e => e).ToList();

                List<PharmacyIntakeFaculty> PharmacyList = new List<PharmacyIntakeFaculty>();

                if (jntuh_appeal_pharmacyData.Count() == 0 || jntuh_appeal_pharmacyData.Count() == null)
                {
                    return View(PharmacyList);
                }
                else
                {
                    var jntuh_appeal_pharmacyData_group = jntuh_appeal_pharmacyData.GroupBy(e => new { e.Department, e.Specialization, e.PharmacySpecialization }).Select(e => e.First()).ToList();
                    foreach (var item in jntuh_appeal_pharmacyData_group)
                    {
                        PharmacyIntakeFaculty Pharmacy = new PharmacyIntakeFaculty();
                        Pharmacy.collegeId = Convert.ToInt32(item.CollegeCode);
                        Pharmacy.DegreeId = Convert.ToInt32(item.Department);
                        Pharmacy.Department = jntuh_department.Where(e => e.id == Pharmacy.DegreeId).Select(e => e.departmentName).FirstOrDefault();
                        Pharmacy.SpecializationId = Convert.ToInt32(item.Specialization);
                        Pharmacy.Specialization = jntuh_specialization.Where(e => e.id == Pharmacy.SpecializationId).Select(e => e.specializationName).FirstOrDefault();
                        Pharmacy.GroupId = item.PharmacySpecialization;
                        // Pharmacy.TotalIntake = jntuh_college_intake_existing_Data.Where(e => e.specializationId == Pharmacy.SpecializationId).Select(e => e.proposedIntake).FirstOrDefault();
                        Pharmacy.TotalIntake = item.TotalIntake;
                        Pharmacy.ProposedIntake = item.ProposedIntake;
                        Pharmacy.TotalRequiredFaculty = item.NoOfFacultyRequired;
                        Pharmacy.SpecializationWiseFaculty = item.SpecializationWiseRequiredFaculty;
                        Pharmacy.FacultyCount = jntuh_appeal_pharmacyData.Where(e => e.Department == item.Department && e.Specialization == item.Specialization && e.PharmacySpecialization == item.PharmacySpecialization && e.Deficiency != null).Select(e => e.Deficiency).Count();
                        PharmacyList.Add(Pharmacy);
                    }
                }
                return View(PharmacyList);
            }
            else if (collegeId != null && DegreeId != null && GroupId == null)
            {
                ViewBag.AfterCollegeSelect = GroupId;

                var jntuh_college_intake_existing = db.jntuh_college_intake_existing.Where(e => e.collegeId == collegeId).Select(e => e).ToList();
                var jntuh_college_intake_existing_Data = jntuh_college_intake_existing.Where(e => e.academicYearId == AY1 && e.courseStatus != "Closure").Select(w => w).ToList();
                var SpecializationIds = jntuh_college_intake_existing_Data.Select(e => e.specializationId).Distinct().ToArray();
                var Degrees = (from e in jntuh_college_intake_existing_Data
                               join s in jntuh_specialization on e.specializationId equals s.id
                               join d in jntuh_department on s.departmentId equals d.id
                               join de in db.jntuh_degree on d.degreeId equals de.id
                               where SpecializationIds.Contains(s.id) && (de.id == 2 || de.id == 5 || de.id == 9 || de.id == 10)
                               select new
                               {
                                   DegreeId = de.id,
                                   Degreename = de.degree
                               }).Distinct().ToList();

                ViewBag.Degrees = Degrees;

                string Departmentid = jntuh_department.Where(e => e.degreeId == DegreeId).Select(e => e.id).FirstOrDefault().ToString();
                string collegeID = collegeId.ToString();
                string DegreeID = DegreeId.ToString();
                string GroupID = GroupId.ToString();
                var pharmacyDatabasedonGroup = new jntuh_appeal_pharmacydata();
                var jntuh_college_faculty_registered_all = db.jntuh_college_faculty_registered.Where(e => e.collegeId == collegeId).Select(e => e).ToList();
                var clgstrRegnos = jntuh_college_faculty_registered_all.Select(e => e.RegistrationNumber).ToList();
                var jntuh_registered_faculty_New = db.jntuh_registered_faculty.Where(e => clgstrRegnos.Contains(e.RegistrationNumber.Trim())).Select(e => e).ToList();
                var jntuh_registered_facultyBAS = jntuh_registered_faculty_New.Where(rf => (rf.BAS == "Yes")).Select(rf => new
                {
                    FacultyId = rf.id,
                    RegistrationNumber = rf.RegistrationNumber.Trim(),
                    BASFlag = rf.BAS
                }).ToList();
                var BASRegNos = jntuh_registered_facultyBAS.Select(rf => rf.RegistrationNumber).ToArray();
                if (DegreeId == 5)
                {
                    pharmacyDatabasedonGroup = db.jntuh_appeal_pharmacydata.Where(e => e.CollegeCode == collegeID && e.Department == Departmentid && !BASRegNos.Contains(e.Deficiency)).Select(e => e).FirstOrDefault();
                    List<SelectListItem> Groups = new List<SelectListItem>();
                    Groups.Add(new SelectListItem { Text = "Group1", Value = "1" });
                    Groups.Add(new SelectListItem { Text = "Group2", Value = "2" });
                    Groups.Add(new SelectListItem { Text = "Group3", Value = "3" });
                    Groups.Add(new SelectListItem { Text = "Group4", Value = "4" });
                    ViewBag.Groups = Groups.Select(e => new
                    {
                        GroupId = e.Value,
                        GroupName = e.Text
                    }).ToList();
                }
                if (pharmacyDatabasedonGroup == null)
                {
                    TempData["TotalRequiredFaculty"] = null;
                    TempData["SpecializationWiseFaculty"] = null;
                }
                else
                {
                    TempData["TotalRequiredFaculty"] = pharmacyDatabasedonGroup.NoOfFacultyRequired;
                    int specializationID = Convert.ToInt32(pharmacyDatabasedonGroup.Specialization);
                    TempData["ProposedIntake"] = jntuh_college_intake_existing_Data.Where(e => e.specializationId == specializationID).Select(e => e.proposedIntake).FirstOrDefault();
                    //TempData["SpecializationWiseFaculty"] = pharmacyDatabasedonGroup.SpecializationWiseRequiredFaculty;
                }

                string CollegeID = collegeId.ToString();

                var jntuh_appeal_pharmacyData = db.jntuh_appeal_pharmacydata.Where(e => e.CollegeCode == CollegeID && !BASRegNos.Contains(e.Deficiency)).Select(e => e).ToList();

                List<PharmacyIntakeFaculty> PharmacyList = new List<PharmacyIntakeFaculty>();

                if (jntuh_appeal_pharmacyData.Count() == 0 || jntuh_appeal_pharmacyData.Count() == null)
                {
                    return View(PharmacyList);
                }
                else
                {
                    var jntuh_appeal_pharmacyData_group = jntuh_appeal_pharmacyData.GroupBy(e => new { e.Department, e.Specialization, e.PharmacySpecialization }).Select(e => e.First()).ToList();
                    foreach (var item in jntuh_appeal_pharmacyData_group)
                    {
                        PharmacyIntakeFaculty Pharmacy = new PharmacyIntakeFaculty();
                        Pharmacy.collegeId = Convert.ToInt32(item.CollegeCode);
                        Pharmacy.DegreeId = Convert.ToInt32(item.Department);
                        Pharmacy.Department = jntuh_department.Where(e => e.id == Pharmacy.DegreeId).Select(e => e.departmentName).FirstOrDefault();
                        Pharmacy.SpecializationId = Convert.ToInt32(item.Specialization);
                        Pharmacy.Specialization = jntuh_specialization.Where(e => e.id == Pharmacy.SpecializationId).Select(e => e.specializationName).FirstOrDefault();
                        Pharmacy.GroupId = item.PharmacySpecialization;
                        // Pharmacy.TotalIntake = jntuh_college_intake_existing_Data.Where(e => e.specializationId == Pharmacy.SpecializationId).Select(e => e.proposedIntake).FirstOrDefault();
                        Pharmacy.TotalIntake = item.TotalIntake;
                        Pharmacy.ProposedIntake = item.ProposedIntake;
                        Pharmacy.TotalRequiredFaculty = item.NoOfFacultyRequired;
                        Pharmacy.SpecializationWiseFaculty = item.SpecializationWiseRequiredFaculty;
                        Pharmacy.FacultyCount = jntuh_appeal_pharmacyData.Where(e => e.Department == item.Department && e.Specialization == item.Specialization && e.PharmacySpecialization == item.PharmacySpecialization && e.Deficiency != null).Select(e => e.Deficiency).Count();

                        PharmacyList.Add(Pharmacy);
                    }
                }
                return View(PharmacyList);
            }
            else if (collegeId != null && DegreeId != null && GroupId != null)
            {

                ViewBag.AfterCollegeSelect = GroupId;

                var jntuh_college_intake_existing = db.jntuh_college_intake_existing.Where(e => e.collegeId == collegeId).Select(e => e).ToList();
                var jntuh_college_intake_existing_Data = jntuh_college_intake_existing.Where(e => e.academicYearId == AY1 && e.courseStatus != "Closure").Select(w => w).ToList();
                var SpecializationIds = jntuh_college_intake_existing_Data.Select(e => e.specializationId).Distinct().ToArray();
                var Degrees = (from e in jntuh_college_intake_existing_Data
                               join s in jntuh_specialization on e.specializationId equals s.id
                               join d in jntuh_department on s.departmentId equals d.id
                               join de in db.jntuh_degree on d.degreeId equals de.id
                               where SpecializationIds.Contains(s.id) && (de.id == 2 || de.id == 5 || de.id == 9 || de.id == 10)
                               select new
                               {
                                   DegreeId = de.id,
                                   Degreename = de.degree
                               }).Distinct().ToList();

                ViewBag.Degrees = Degrees;

                string Departmentid = jntuh_department.Where(e => e.degreeId == DegreeId).Select(e => e.id).FirstOrDefault().ToString();
                string collegeID = collegeId.ToString();
                string DegreeID = DegreeId.ToString();
                string GroupID = GroupId.ToString();
                var pharmacyDatabasedonGroup = new jntuh_appeal_pharmacydata();
                var jntuh_college_faculty_registered_all = db.jntuh_college_faculty_registered.Where(e => e.collegeId == collegeId).Select(e => e).ToList();
                var clgstrRegnos = jntuh_college_faculty_registered_all.Select(e => e.RegistrationNumber).ToList();
                var jntuh_registered_faculty_New = db.jntuh_registered_faculty.Where(e => clgstrRegnos.Contains(e.RegistrationNumber.Trim())).Select(e => e).ToList();
                var jntuh_registered_facultyBAS = jntuh_registered_faculty_New.Where(rf => (rf.BAS == "Yes")).Select(rf => new
                {
                    FacultyId = rf.id,
                    RegistrationNumber = rf.RegistrationNumber.Trim(),
                    BASFlag = rf.BAS
                }).ToList();
                var BASRegNos = jntuh_registered_facultyBAS.Select(rf => rf.RegistrationNumber).ToArray();
                if (DegreeId == 5)
                {
                    pharmacyDatabasedonGroup = db.jntuh_appeal_pharmacydata.Where(e => e.CollegeCode == collegeID && e.Department == Departmentid && e.PharmacySpecialization == GroupID && !BASRegNos.Contains(e.Deficiency)).Select(e => e).FirstOrDefault();
                    List<SelectListItem> Groups = new List<SelectListItem>();
                    Groups.Add(new SelectListItem { Text = "Group1", Value = "1" });
                    Groups.Add(new SelectListItem { Text = "Group2", Value = "2" });
                    Groups.Add(new SelectListItem { Text = "Group3", Value = "3" });
                    Groups.Add(new SelectListItem { Text = "Group4", Value = "4" });
                    ViewBag.Groups = Groups.Select(e => new
                    {
                        GroupId = e.Value,
                        GroupName = e.Text
                    }).ToList();
                }
                else
                {
                    pharmacyDatabasedonGroup = db.jntuh_appeal_pharmacydata.Where(e => e.CollegeCode == collegeID && e.Department == Departmentid && e.Specialization == GroupID && !BASRegNos.Contains(e.Deficiency)).Select(e => e).FirstOrDefault();

                    var Spec = (from e in jntuh_college_intake_existing_Data
                                join s in jntuh_specialization on e.specializationId equals s.id
                                join d in jntuh_department on s.departmentId equals d.id
                                join de in db.jntuh_degree on d.degreeId equals de.id
                                where SpecializationIds.Contains(s.id) && de.id == DegreeId
                                select new
                                {
                                    GroupId = s.id,
                                    GroupName = s.specializationName
                                }).Distinct().ToList();
                    ViewBag.Groups = Spec;
                }

                if (pharmacyDatabasedonGroup == null)
                {
                    TempData["TotalRequiredFaculty"] = null;
                    TempData["SpecializationWiseFaculty"] = null;
                }
                else
                {
                    TempData["TotalRequiredFaculty"] = pharmacyDatabasedonGroup.NoOfFacultyRequired;
                    TempData["SpecializationWiseFaculty"] = pharmacyDatabasedonGroup.SpecializationWiseRequiredFaculty;
                    int specializationID = Convert.ToInt32(pharmacyDatabasedonGroup.Specialization);
                    TempData["ProposedIntake"] = jntuh_college_intake_existing_Data.Where(e => e.specializationId == specializationID).Select(e => e.proposedIntake).FirstOrDefault();
                }

                string CollegeID = collegeId.ToString();

                var jntuh_appeal_pharmacyData = db.jntuh_appeal_pharmacydata.Where(e => e.CollegeCode == CollegeID && !BASRegNos.Contains(e.Deficiency)).Select(e => e).ToList();

                List<PharmacyIntakeFaculty> PharmacyList = new List<PharmacyIntakeFaculty>();

                if (jntuh_appeal_pharmacyData.Count() == 0 || jntuh_appeal_pharmacyData.Count() == null)
                {
                    return View(PharmacyList);
                }
                else
                {
                    var jntuh_appeal_pharmacyData_group = jntuh_appeal_pharmacyData.GroupBy(e => new { e.Department, e.Specialization, e.PharmacySpecialization }).Select(e => e.First()).ToList();
                    foreach (var item in jntuh_appeal_pharmacyData_group)
                    {
                        PharmacyIntakeFaculty Pharmacy = new PharmacyIntakeFaculty();
                        Pharmacy.collegeId = Convert.ToInt32(item.CollegeCode);
                        Pharmacy.DegreeId = Convert.ToInt32(item.Department);
                        Pharmacy.Department = jntuh_department.Where(e => e.id == Pharmacy.DegreeId).Select(e => e.departmentName).FirstOrDefault();
                        Pharmacy.SpecializationId = Convert.ToInt32(item.Specialization);
                        Pharmacy.Specialization = jntuh_specialization.Where(e => e.id == Pharmacy.SpecializationId).Select(e => e.specializationName).FirstOrDefault();
                        Pharmacy.GroupId = item.PharmacySpecialization;
                        // Pharmacy.TotalIntake = jntuh_college_intake_existing_Data.Where(e => e.specializationId == Pharmacy.SpecializationId).Select(e => e.proposedIntake).FirstOrDefault();
                        Pharmacy.TotalIntake = item.TotalIntake;
                        Pharmacy.ProposedIntake = item.ProposedIntake;
                        Pharmacy.TotalRequiredFaculty = item.NoOfFacultyRequired;
                        Pharmacy.SpecializationWiseFaculty = item.SpecializationWiseRequiredFaculty;
                        Pharmacy.FacultyCount = jntuh_appeal_pharmacyData.Where(e => e.Department == item.Department && e.Specialization == item.Specialization && e.PharmacySpecialization == item.PharmacySpecialization && e.Deficiency != null).Select(e => e.Deficiency).Count();

                        PharmacyList.Add(Pharmacy);
                    }
                }
                return View(PharmacyList);
            }
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin,SuperAdmin,DataEntry,FacultyVerification")]
        public ActionResult EditPharmacyIntake(int? collegeId, int? DegreeId, int? GroupId, int? TotalRequiredFaculty, int? SpecializationWiseFaculty, int? ProposedIntake)
        {
            var CollegeIds = new int[] { 448, 180, 159, 376, 428, 445, 24, 117, 202, 213, 395, 364, 27, 30, 6, 34, 55, 58, 54, 52, 9, 60, 65, 78, 150, 204, 219, 90, 97, 104, 110, 139, 105, 107, 114, 379, 118, 39, 120, 389, 127, 237, 135, 136, 47, 252, 169, 253, 262, 442, 301, 436, 370, 263, 267, 206, 140, 44, 290, 454, 42, 95, 297, 384, 75, 348, 298, 295, 303, 302, 283, 332, 410, 392, 234, 146, 315, 320, 317, 314, 318, 319, 353, 313, 235, 375 };
            var jntuhcollege = db.jntuh_college.AsNoTracking().Where(e => CollegeIds.Contains(e.id)).ToList();

            ViewBag.PharmacyCollegeList = jntuhcollege.Select(e => new
            {
                collegeId = e.id,
                collegeName = e.collegeCode + "-" + e.collegeName
            }).ToList();
            var jntuh_college_faculty_registered_all = db.jntuh_college_faculty_registered.Where(e => e.collegeId == collegeId).Select(e => e).ToList();
            var clgstrRegnos = jntuh_college_faculty_registered_all.Select(e => e.RegistrationNumber).ToList();
            var jntuh_registered_faculty_New = db.jntuh_registered_faculty.Where(e => clgstrRegnos.Contains(e.RegistrationNumber.Trim())).Select(e => e).ToList();
            var jntuh_registered_facultyBAS = jntuh_registered_faculty_New.Where(rf => (rf.BAS == "Yes")).Select(rf => new
            {
                FacultyId = rf.id,
                RegistrationNumber = rf.RegistrationNumber.Trim(),
                BASFlag = rf.BAS
            }).ToList();
            var BASRegNos = jntuh_registered_facultyBAS.Select(rf => rf.RegistrationNumber).ToArray();
            if (collegeId != null && DegreeId != null && GroupId != null && TotalRequiredFaculty != null && SpecializationWiseFaculty != null)
            {
                string collegeID = collegeId.ToString();
                string DegreeID = DegreeId.ToString();
                string GroupID = GroupId.ToString();
                var PharmacySpecializationData = new List<jntuh_appeal_pharmacydata>();

                if (DegreeId == 5)
                {
                    string DepartmentId = db.jntuh_department.Where(e => e.degreeId == DegreeId).Select(e => e.id).FirstOrDefault().ToString();
                    PharmacySpecializationData = db.jntuh_appeal_pharmacydata.Where(e => e.CollegeCode == collegeID && e.Department == DepartmentId && e.PharmacySpecialization == GroupID && !BASRegNos.Contains(e.Deficiency)).Select(e => e).ToList();
                }
                else
                {
                    string DepartmentId = db.jntuh_department.Where(e => e.degreeId == DegreeId).Select(e => e.id).FirstOrDefault().ToString();
                    PharmacySpecializationData = db.jntuh_appeal_pharmacydata.Where(e => e.CollegeCode == collegeID && e.Department == DepartmentId && e.Specialization == GroupID && !BASRegNos.Contains(e.Deficiency)).Select(e => e).ToList();
                }

                foreach (var item in PharmacySpecializationData)
                {
                    var data = item;
                    jntuh_appeal_pharmacydata PharmacyData = new jntuh_appeal_pharmacydata();
                    PharmacyData.Sno = data.Sno;
                    PharmacyData.CollegeCode = data.CollegeCode;
                    PharmacyData.Department = data.Department;
                    PharmacyData.Specialization = data.Specialization;
                    PharmacyData.TotalIntake = data.TotalIntake;
                    PharmacyData.ProposedIntake = ProposedIntake;
                    // PharmacyData.ProposedIntake = data.ProposedIntake;
                    PharmacyData.NoOfFacultyRequired = TotalRequiredFaculty;
                    PharmacyData.NoOfFacultyAvilabli = data.NoOfFacultyAvilabli;
                    PharmacyData.SpecializationWiseRequiredFaculty = SpecializationWiseFaculty;
                    PharmacyData.SpecializationWiseAvilableFaculty = data.SpecializationWiseAvilableFaculty;
                    PharmacyData.PharmacySpecialization = data.PharmacySpecialization;
                    PharmacyData.Deficiency = data.Deficiency;
                    PharmacyData.PhDFaculty = data.PhDFaculty;
                    PharmacyData.CreatedOn = DateTime.Now;
                    PharmacyData.IsActive = data.IsActive;
                    db.Entry(data).CurrentValues.SetValues(PharmacyData);
                    db.SaveChanges();

                }
                TempData["Success"] = "Data is Updated Successfully";
                return RedirectToAction("EditPharmacyIntake", new { CollegeId = collegeId, DegreeID = DegreeId, GroupID = GroupId });
            }
            else if (collegeId != null && DegreeId != null && GroupId == null && TotalRequiredFaculty != null)
            {
                string collegeID = collegeId.ToString();
                string DegreeID = DegreeId.ToString();
                string GroupID = GroupId.ToString();
                var PharmacySpecializationData = new List<jntuh_appeal_pharmacydata>();

                string DepartmentId = db.jntuh_department.Where(e => e.degreeId == DegreeId).Select(e => e.id).FirstOrDefault().ToString();
                PharmacySpecializationData = db.jntuh_appeal_pharmacydata.Where(e => e.CollegeCode == collegeID && e.Department == DepartmentId && !BASRegNos.Contains(e.Deficiency)).Select(e => e).ToList();

                foreach (var item in PharmacySpecializationData)
                {
                    var data = item;
                    jntuh_appeal_pharmacydata PharmacyData = new jntuh_appeal_pharmacydata();
                    PharmacyData.Sno = data.Sno;
                    PharmacyData.CollegeCode = data.CollegeCode;
                    PharmacyData.Department = data.Department;
                    PharmacyData.Specialization = data.Specialization;
                    PharmacyData.TotalIntake = data.TotalIntake;
                    PharmacyData.ProposedIntake = ProposedIntake;
                    // PharmacyData.ProposedIntake = data.ProposedIntake;
                    PharmacyData.NoOfFacultyRequired = TotalRequiredFaculty;
                    PharmacyData.NoOfFacultyAvilabli = data.NoOfFacultyAvilabli;
                    PharmacyData.SpecializationWiseRequiredFaculty = data.SpecializationWiseRequiredFaculty;
                    PharmacyData.SpecializationWiseAvilableFaculty = data.SpecializationWiseAvilableFaculty;
                    PharmacyData.PharmacySpecialization = data.PharmacySpecialization;
                    PharmacyData.Deficiency = data.Deficiency;
                    PharmacyData.PhDFaculty = data.PhDFaculty;
                    PharmacyData.CreatedOn = DateTime.Now;
                    PharmacyData.IsActive = data.IsActive;
                    db.Entry(data).CurrentValues.SetValues(PharmacyData);
                    db.SaveChanges();

                }
                TempData["Success"] = "Data is Updated Successfully";
                return RedirectToAction("EditPharmacyIntake", new { CollegeId = collegeId, DegreeID = DegreeId, GroupID = GroupId });
            }
            else
            {
                return RedirectToAction("EditPharmacyIntake");
            }

            //return View();
        }

        [Authorize(Roles = "Admin,SuperAdmin,DataEntry,FacultyVerification")]
        public ActionResult DeleteAddedFaculty(int? Id)
        {
            if (Id != null)
            {
                var PharmacyFaculty = db.jntuh_appeal_pharmacydata.Where(e => e.Sno == Id).Select(e => e).FirstOrDefault();
                int collegeid = Convert.ToInt32(PharmacyFaculty.CollegeCode);
                if (PharmacyFaculty != null)
                {
                    db.jntuh_appeal_pharmacydata.Remove(PharmacyFaculty);
                    db.SaveChanges();
                    TempData["Success"] = PharmacyFaculty.Deficiency + "is Deleted Successfully";
                }
                return RedirectToAction("ViewAddedFaculty", new { collegeId = collegeid });
            }
            else
            {
                TempData["Error"] = "Data is Not Found";
                return RedirectToAction("ViewAddedFaculty");
            }
            return View();
        }

        [Authorize(Roles = "Admin,SuperAdmin,DataEntry,FacultyVerification")]
        public ActionResult DeleteAllAddedFaculty(int? collegeId, string Status)
        {
            if (Status == "DeleteAll")
            {
                int collegeID = Convert.ToInt32(collegeId);
                string collegeid = collegeId.ToString();
                var PharmacyFaculty = db.jntuh_appeal_pharmacydata.Where(e => e.CollegeCode == collegeid).Select(e => e).ToList();
                if (PharmacyFaculty.Count() > 0)
                {
                    db.jntuh_appeal_pharmacydata.Where(d => d.CollegeCode == collegeid).ToList().ForEach(d => db.jntuh_appeal_pharmacydata.Remove(d));
                    db.SaveChanges();
                }
                else
                {
                    TempData["Error"] = "Data is Not Found";
                    return RedirectToAction("ViewAddedFaculty", new { collegeId = collegeID });
                }
                TempData["Success"] = "Data is Deleted Successfully";
                return RedirectToAction("ViewAddedFaculty", new { collegeId = collegeID });
            }
            else
            {
                TempData["Error"] = "Data is Not Found";
                return RedirectToAction("ViewAddedFaculty");
            }
            return View();
        }

        public ActionResult GetGroups(int? DegreeId)
        {
            List<SelectListItem> Groups = new List<SelectListItem>();
            if (DegreeId != null)
            {

                Groups.Add(new SelectListItem { Text = "Group1", Value = "1" });
                Groups.Add(new SelectListItem { Text = "Group2", Value = "2" });
                Groups.Add(new SelectListItem { Text = "Group3", Value = "3" });
                Groups.Add(new SelectListItem { Text = "Group4", Value = "4" });
                ViewBag.Groups = Groups;
                return Json(new { data = Groups }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { data = Groups }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetSpecializations(int? DegreeId, int? collegeId)
        {

            var jntuh_college_intake_existing = db.jntuh_college_intake_existing.Where(e => e.collegeId == collegeId && e.academicYearId == 10 && e.courseStatus != "Closure").Select(e => e).ToList();
            int[] SpecializationIds = jntuh_college_intake_existing.Select(e => e.specializationId).Distinct().ToArray();
            var jntuh_degree = db.jntuh_degree.Where(e => e.isActive == true).ToList();
            var jntuh_department = db.jntuh_department.Where(e => e.isActive == true).ToList();
            var jntuh_specialization = db.jntuh_specialization.Where(e => e.isActive == true).ToList();

            var data = (from e in jntuh_college_intake_existing
                        join s in jntuh_specialization on e.specializationId equals s.id
                        join d in jntuh_department on s.departmentId equals d.id
                        join de in jntuh_degree on d.degreeId equals de.id
                        where SpecializationIds.Contains(s.id) && de.id == DegreeId
                        select new
                        {
                            id = s.id,
                            Specname = s.specializationName
                        }).Distinct().ToList();
            TempData["Required"] = "Empty";
            return Json(new { data = data }, JsonRequestBehavior.AllowGet);
        }



        #endregion

        #region Pharmacy Deficiency Report Code Wrritten by siva

        [Authorize(Roles = "Admin,SuperAdmin")]
        public ActionResult Deficiencies(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                int collegeID = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(id.ToString(), System.Web.Configuration.WebConfigurationManager.AppSettings["CryptoKey"]));
                //int collegeID = Convert.ToInt32(id);
                var randomid = UAAAS.Models.Utilities.EncryptString(id.ToString(), System.Web.Configuration.WebConfigurationManager.AppSettings["CryptoKey"]);
                Response.ClearContent();
                Response.ClearHeaders();
                Response.Buffer = true;
                string collegeCode = db.jntuh_college.Where(c => c.isActive == true && c.id == collegeID).Select(c => c.collegeCode).FirstOrDefault();
                Response.AddHeader("content-disposition", "attachment; filename=" + collegeCode + "-Deficiency-Report -" + randomid + ".doc");
                Response.ContentType = "application/vnd.ms-word ";
                Response.Charset = string.Empty;
                StringBuilder str = new StringBuilder();
                str.Append("<meta http-equiv='Content-Type' content='text/html; charset=UTF-8' />");
                str.Append(Header());
                str.Append("<br />");
                str.Append(CollegeInformation(collegeID));
                str.Append("<br />");
                str.Append(Principal(collegeID));
                str.Append("<br />");
                str.Append(DeficienciesInFaculty(collegeID));
                str.Append("<br />");
                str.Append(DeficiencyCollegeLabsAnnexure(collegeID));
                str.Append("<br />");
                str.Append(CollegeComplaintsController(collegeID));
                str.Append("<br />");
                str.Append(AdministrativeLandDetails(collegeID));
                str.Append("<br />");
                str.Append(InstructionalAreaDetails(collegeID));
                str.Append("<br />");
                str.Append(CommitteeMembers(collegeID));
                str.Append("<br />");
                str.Append(Annexure());
                str.Append("<br />");
                Document pdfDoc = new Document(PageSize.LEDGER.Rotate(), 20, 10, 20, 20);

                pdfDoc.SetPageSize(iTextSharp.text.PageSize.LEDGER.Rotate());
                pdfDoc.SetMargins(20, 10, 20, 20);


                string path = Server.MapPath("~/Content/PDFReports/PharmacyDeficiencyReports/");

                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                path = path + collegeCode + "-" + DateTime.Now.ToString("yyyMMddHHmmss") + ".pdf";
                PdfWriter pdfWriter = PdfWriter.GetInstance(pdfDoc, new FileStream(path, FileMode.Create));

                pdfDoc.Open();

                List<IElement> parsedHtmlElements = HTMLWorker.ParseToList(new StringReader(str.ToString()), null);

                foreach (var htmlElement in parsedHtmlElements)
                {
                    pdfDoc.Add((IElement)htmlElement);
                }

                pdfDoc.Close();

                Response.Output.Write(str.ToString());
                Response.Flush();
                Response.End();
            }

            return View();
        }

        public string Header()
        {
            string header = string.Empty;
            header += "<table width='100%'>";
            // header += "<tr><td align='center' width='80%' style='font-size: 16px; font-weight: normal;' colspan='2'><b><u> CORRIGENDUM: IN MODIFICATION TO THE DEFICIENCY REPORTS " +
            //"ISSUED ON 18:05:2017 AND 19:05:2017, THE INSTITUTIONS ARE HEREBY ISSUED THE FOLLOWING REVISED DEFICIENCY REPORTS AS PER REVISED PCI NORMS.</u></b></td></tr></br>";

            header += "<tr><td align='right' width='80%' style='font-size: 16px; font-weight: normal;' colspan='2'><b><u> Date : " + DateTime.Now.ToString("dd-MM-yyyy") + "</u></b></td></tr></br>";
            header += "<tr><td  colspan='2'>&nbsp;</td></tr>";
            header += "</table>";
            header += "<table width='100%'>";
            header += "<tr>";
            header += "<td rowspan='4' align='center' width='20%'><img src='http://jntuhaac.in/Content/Images/new_logo.jpg' height='70' width='70' style='text-align: center' align='middle' /></td>";
            header += "<td align='center' width='80%' style='font-size: 14px; font-weight: normal;'><b>JAWAHARLAL NEHRU TECHNOLOGICAL UNIVERSITY HYDERABAD</b></td>";
            header += "</tr>";
            header += "<tr>";
            header += "<td align='center' style='font-size: 11px; font-weight: normal;'><b>KUKATPALLY, HYDERABAD, TELANGANA, INDIA - 500 085</b></td>";
            header += "</tr>";
            header += "<tr>";
            header += "<td  align='center' style='font-weight: normal;'><u><b>DEFICIENCY REPORT AS PER FORM 423</b></u></td>";
            header += "</tr>";
            header += "<tr>";
            header += "<td  align='center' style='font-weight: normal;'><u><b>(for Academic Year 2023-2024)</b></u></td>";
            header += "</tr>";
            header += "</table>";
            return header;
        }

        public string CollegeInformation(int? collegeID)
        {
            string collegeInformation = string.Empty;

            jntuh_college college = db.jntuh_college.Where(c => c.isActive == true && c.id == collegeID).Select(c => c).FirstOrDefault();

            collegeInformation += "<table width='100%' border='0' cellpadding='5' cellspacing='0'>";
            collegeInformation += "<tr>";
            collegeInformation += "<td align='left' width='75%'><b>College Name: </b><u>" + college.collegeName + "</u>";
            collegeInformation += "<td align='left' width='25%'><b>CC:  </b><u>" + college.collegeCode + "</u></td>";
            collegeInformation += "</tr>";

            collegeInformation += "</table>";

            return collegeInformation;
        }

        public string Principal(int? collegeID)
        {
            var principal = string.Empty;
            var Reason = string.Empty;
            var OriginalReason = string.Empty;
            //var college = db.jntuh_college.Where(c => c.isActive == true && c.id == collegeID).Select(c => c).FirstOrDefault();
            var facultydata = new FacultyRegistration();
            var principaldata = db.jntuh_college_principal_registered.FirstOrDefault(i => i.collegeId == collegeID);

            if (principaldata != null)
            {
                var regdata = db.jntuh_registered_faculty.FirstOrDefault(i => i.RegistrationNumber == principaldata.RegistrationNumber);

                if (regdata != null)
                {
                    facultydata.FirstName = regdata.FirstName;
                    facultydata.LastName = regdata.LastName;
                    facultydata.RegistrationNumber = regdata.RegistrationNumber;
                    if (regdata.Absent == true)
                    {
                        OriginalReason += "Absent" + ", ";
                    }
                    if (regdata.BAS == "Yes")
                    {
                        OriginalReason += "Not having Sufficient Biometric Attendance ";
                    }
                    if (!string.IsNullOrEmpty(regdata.DeactivationReason) || !string.IsNullOrEmpty(OriginalReason))
                    {
                        Reason = "Dr. " + facultydata.FirstName.First().ToString().ToUpper() + facultydata.FirstName.Substring(1) + " " + facultydata.LastName.First().ToString().ToUpper() + facultydata.LastName.Substring(1);
                        //Reason.Substring(0, Reason.Length - 1);
                        facultydata.DeactivationNew = "Yes";
                        OriginalReason += !string.IsNullOrEmpty(OriginalReason) ? "," + regdata.DeactivationReason : regdata.DeactivationReason;
                    }
                    else
                    {
                        Reason = "Dr. " + facultydata.FirstName.First().ToString().ToUpper() + facultydata.FirstName.Substring(1) + " " + facultydata.LastName.First().ToString().ToUpper() + facultydata.LastName.Substring(1);
                        facultydata.DeactivationNew = "";

                    }

                }
            }

            else
            {
                Reason = "NOT AVAILABLE";
                facultydata.DeactivationNew = "Yes";
            }

            principal += "<table width='100%' border='0' cellpadding='5' cellspacing='0'>";
            principal += "<tr>";
            //principal += "<td align='left'><b>Principal: </b><img alt='' src='http://112.133.193.228:75//Content/Images/checkbox_no.png' height='16' width='16' /> Qualified &nbsp; <img alt='' src='http://112.133.193.228:75//Content/Images/checkbox_no.png' height='16' width='16' /> Ratified &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; Deficiency: <img alt='' src='http://112.133.193.228:75//Content/Images/checkbox_no.png' height='16' width='16' /> Yes <img alt='' src='http://112.133.193.228:75//Content/Images/checkbox_no.png' height='16' width='16' /> No";
            principal += "<td align='left'><b>Principal: </b>" + Reason + "&nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;";
            if (!string.IsNullOrEmpty(facultydata.DeactivationNew))
                principal += "<b> Deficiency: </b>" + facultydata.DeactivationNew + "&nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;";
            if (!string.IsNullOrEmpty(OriginalReason))
                principal += "<b> Reason: </b>" + OriginalReason;
            principal += "</td>";
            principal += "</tr>";
            principal += "</table>";

            return principal;
        }

        public string DeficienciesInFaculty(int? collegeID)
        {
            string faculty = string.Empty;
            int? AddingFacultyCount = 0;
            int? TotalcollegeFaculty = 0;
            int? AdditionalFaculty = 0;
            int SecondYerintake = 0;
            int ThirdYerintake = 0;
            int FourthYerintake = 0;
            int PharmDFirstYerintake = 0;
            int PharmDSecondYerintake = 0;
            int PharmDThirdYerintake = 0;
            int PharmDFourthYerintake = 0;
            int PharmDFifthhYerintake = 0;

            List<PharmacyReportsClass> PharmacyAppealFaculty = new List<PharmacyReportsClass>();
            string facultyAdmittedIntakeZero = string.Empty;
            faculty += "<table width='100%' border='0' cellpadding='5' cellspacing='0'>";
            faculty += "<tr>";
            faculty += "<td align='left'><b><u>Deficiencies in Faculty</u></b> (Department/ Specialization Wise):</td>";
            faculty += "</tr>";
            faculty += "</table>";

            faculty += "<table width='100%' border='1' cellpadding='3' cellspacing='0' style='border-color: #ccc;font-size:13px'>";
            // faculty += "<table width='100%' border='1' cellpadding='3' cellspacing='0' style='border-color: #ccc;font-size:13px;border-collapse:collapse;border: 1px;' rules='all'>";
            faculty += "<tr>";
            faculty += "<th style='text-align: center; vertical-align: top;'>SNo</th>";
            faculty += "<th style='text-align: left; vertical-align: top;' >Department</th>";
            faculty += "<th style='text-align: left; vertical-align: top;' >Degree</th>";
            faculty += "<th style='text-align: left; vertical-align: top;' >Specialization</th>";
            faculty += "<th style='text-align: center; vertical-align: top;' >Total Intake *</th>";
            faculty += "<th style='text-align: center; vertical-align: top;' >Proposed Intake</th>";
            faculty += "<th style='text-align: center; vertical-align: top;' >No.of Faculty Required *</th>";
            faculty += "<th style='text-align: center; vertical-align: top;' >No.of Faculty Available</th>";
            faculty += "<th style='text-align: center; vertical-align: top;' >Pharmacy Specializations *</th>";
            faculty += "<th style='text-align: center; vertical-align: top;' >Specialization Wise Faculty Required</th>";
            faculty += "<th style='text-align: center; vertical-align: top;' >Specialization Wise Faculty Available</th>";
            faculty += "<th style='text-align: center; vertical-align: top;' >Deficiency</th>";
            faculty += "<th style='text-align: center; vertical-align: top;' >No. of Ph.D faculty Available</th>";
            faculty += "<th style='text-align: center; vertical-align: top;' >Deficiency in Faculty</th>";
            faculty += "</tr>";

            var jntuh_academic_year = db.jntuh_academic_year.ToList();
            int presentYear = jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();

            int AY0 = jntuh_academic_year.Where(a => a.actualYear == (presentYear + 1)).Select(a => a.id).FirstOrDefault();
            int AY1 = jntuh_academic_year.Where(a => a.actualYear == presentYear).Select(a => a.id).FirstOrDefault();
            int AY2 = jntuh_academic_year.Where(a => a.actualYear == (presentYear - 1)).Select(a => a.id).FirstOrDefault();
            int AY3 = jntuh_academic_year.Where(a => a.actualYear == (presentYear - 2)).Select(a => a.id).FirstOrDefault();
            int AY4 = jntuh_academic_year.Where(a => a.actualYear == (presentYear - 3)).Select(a => a.id).FirstOrDefault();
            int AY5 = jntuh_academic_year.Where(a => a.actualYear == (presentYear - 4)).Select(a => a.id).FirstOrDefault();

            List<jntuh_college_intake_existing> intake = db.jntuh_college_intake_existing.Where(i => i.collegeId == collegeID && i.academicYearId == AY0 && i.proposedIntake != 0 && i.courseStatus != "Closure").ToList();
            List<CollegeIntakeExisting> collegeIntakeExisting = new List<CollegeIntakeExisting>();
            //List<CollegeIntakeReport> collegeIntakeExistingList = new List<CollegeIntakeReport>();

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
                newIntake.Specialization = item.jntuh_specialization.specializationName;
                newIntake.DepartmentID = item.jntuh_specialization.jntuh_department.id;
                newIntake.Department = item.jntuh_specialization.jntuh_department.departmentName;
                newIntake.degreeID = item.jntuh_specialization.jntuh_department.jntuh_degree.id;
                newIntake.Degree = item.jntuh_specialization.jntuh_department.jntuh_degree.degree;
                newIntake.degreeDisplayOrder = item.jntuh_specialization.jntuh_department.jntuh_degree.degreeDisplayOrder;
                newIntake.shiftId = item.shiftId;
                newIntake.Shift = item.jntuh_shift.shiftName;
                collegeIntakeExisting.Add(newIntake);
            }

            collegeIntakeExisting = collegeIntakeExisting.GroupBy(a => new { a.specializationId, a.shiftId }).Select(a => a.First()).ToList();

            var jntuh_college = db.jntuh_college.Where(a => a.isActive == true).Select(q => q).ToList();

            string cid = collegeID.ToString();
            var PharmacyDepartmens = new int[] { 26, 36, 27, 39, 61 };
            string[] collegefacultyRegistrationNo = db.jntuh_college_faculty_registered.AsNoTracking().Where(e => e.collegeId == collegeID).Select(e => e.RegistrationNumber).ToArray();
            var registeredFacultyNew = db.jntuh_registered_faculty.Where(rf => collegefacultyRegistrationNo.Contains(rf.RegistrationNumber.Trim())).ToList();
            var jntuh_registered_facultyBAS = registeredFacultyNew.Where(rf => (rf.BAS == "Yes")).Select(rf => new
            {
                FacultyId = rf.id,
                //Departmentid = rf.DepartmentId,
                RegistrationNumber = rf.RegistrationNumber.Trim(),
                // Department = rf.jntuh_department.departmentName,
                //DeptId = jntuh_college_faculty_registered.Where(c => c.RegistrationNumber.Trim() == rf.RegistrationNumber.Trim()).Select(c => c.DepartmentId).FirstOrDefault(),
                //SpecializationId = jntuh_college_faculty_registered.Where(c => c.RegistrationNumber.Trim() == rf.RegistrationNumber.Trim()).Select(c => c.SpecializationId).FirstOrDefault(),
                NotconsideredPHD = rf.NotconsideredPHD,
                // HighestDegreeID = rf.jntuh_registered_faculty_education.Count() != 0 ? rf.jntuh_registered_faculty_education.Where(e => e.educationId != 8).Select(e => e.educationId).Max() : 0,
                HighestDegreeID = rf.NotconsideredPHD == true ?
                            rf.jntuh_registered_faculty_education.Where(r => r.educationId != 8 && r.educationId != 9).Count() != 0 ?
                            rf.jntuh_registered_faculty_education.Where(e => e.educationId != 8 && e.educationId != 6 && e.educationId != 9).Select(e => e.educationId).Max() : 0
                            :
                            rf.jntuh_registered_faculty_education.Where(r => r.educationId != 8 && r.educationId != 9).Count() != 0 ?
                            rf.jntuh_registered_faculty_education.Where(e => e.educationId != 8 && e.educationId != 9).Select(e => e.educationId).Max() : 0,
                IsApproved = rf.isApproved,
                //Createdon = jntuh_college_faculty_registered.Where(c => c.RegistrationNumber.Trim() == rf.RegistrationNumber.Trim()).Select(c => c.createdOn).FirstOrDefault(),
                PanNumber = rf.PANNumber,
                AadhaarNumber = rf.AadhaarNumber,
                NoForm16 = rf.NoForm16,
                TotalExperience = rf.TotalExperience,
                CsePhDFacultyFlag = rf.PhdDeskVerification,
                BASFlag = rf.BAS
            }).ToList();
            var BASRegNos = jntuh_registered_facultyBAS.Select(rf => rf.RegistrationNumber).ToArray();
            var FacultyData = db.jntuh_appeal_pharmacydata.Where(p => p.CollegeCode == cid && (p.Deficiency == null || !BASRegNos.Contains(p.Deficiency))).ToList();
            string[] AssignedFaculty = FacultyData.Select(p => p.Deficiency).ToArray();
            var registeredFaculty = db.jntuh_registered_faculty.Where(rf => AssignedFaculty.Contains(rf.RegistrationNumber.Trim())).ToList();
            AdditionalFaculty = FacultyData.Where(a => a.CollegeCode == cid && a.Deficiency != null).Select(z => z.Deficiency).Count();

            int Count = 1;
            var totalReqFaculty = 0;
            foreach (var item in collegeIntakeExisting.OrderBy(s => s.specializationId).ThenBy(a => a.shiftId).ToList())
            {
                if (item.Degree == "B.Pharmacy")
                {
                    for (int i = 1; i <= 4; i++)
                    {
                        PharmacyReportsClass Pharmacy = new PharmacyReportsClass();
                        Pharmacy.Collegeid = item.collegeId;
                        Pharmacy.Degree = item.Degree;
                        Pharmacy.DepartmentId = item.DepartmentID;
                        Pharmacy.Department = item.Department;
                        Pharmacy.SpecializationId = item.specializationId;
                        Pharmacy.Specialization = item.Specialization;
                        Pharmacy.ShiftId = item.shiftId;
                        switch (i)
                        {
                            case 1:
                                Pharmacy.PharmacySpecialization = "Group1 (Pharmaceutics , Industrial Pharmacy , Pharmaceutical Technology , 	Pharmaceutical Biotechnology , RA)";
                                Pharmacy.GroupId = "1";
                                break;
                            case 2:
                                Pharmacy.PharmacySpecialization = "Group2 (Pharmaceutical Chemistry,Pharmaceutical Analysis , PAQA , QA , QAPRA , NIPER Medicinal Chemistry)";
                                Pharmacy.GroupId = "2";
                                break;
                            case 3:
                                Pharmacy.PharmacySpecialization = "Group3 (Pharmacology, Pharm-D, Pharm-DPB, Pharmacy Practice , Hospital Pharmacy , Clinical Pharmacy,  Hospital and Clinical Pharmacy)";
                                Pharmacy.GroupId = "3";
                                break;
                            default:
                                Pharmacy.PharmacySpecialization = "Group4 (Pharmacognosy, Pharmaceutical Chemistry , Phytopharmacy & Phytomedicine , NIPER  Natural Products , Pharmaceutical Biotechnology)";
                                Pharmacy.GroupId = "4";
                                break;
                        }

                        Pharmacy.ProposedIntake = GetPharmacyIntake(item.collegeId, AY0, item.specializationId, item.shiftId, 0);
                        Pharmacy.TotalIntake = FacultyData.Where(f => f.CollegeCode == cid && f.Specialization == item.specializationId.ToString()).Select(f => f.TotalIntake).FirstOrDefault();
                        Pharmacy.NoOfFacultyRequired = FacultyData.Where(f => f.CollegeCode == cid && f.Specialization == item.specializationId.ToString()).Select(f => f.NoOfFacultyRequired).FirstOrDefault();
                        Pharmacy.NoOfAvilableFaculty = FacultyData.Where(f => f.CollegeCode == cid && f.Specialization == item.specializationId.ToString() && f.Deficiency != null).Select(a => a.Deficiency).Distinct().Count();
                        Pharmacy.SpecializationwiseRequiredFaculty = FacultyData.Where(f => f.CollegeCode == cid && f.Specialization == item.specializationId.ToString() && f.PharmacySpecialization == Pharmacy.GroupId).Select(f => f.SpecializationWiseRequiredFaculty).LastOrDefault();
                        Pharmacy.SpecializationwiseAvilableFaculty = FacultyData.Where(f => f.CollegeCode == cid && f.Specialization == item.specializationId.ToString() && f.PharmacySpecialization == Pharmacy.GroupId && f.Deficiency != null).Select(a => a.Deficiency).Distinct().Count();
                        Pharmacy.SpecializationwiseRequiredFaculty = Pharmacy.SpecializationwiseRequiredFaculty == null ? 0 : (int)Pharmacy.SpecializationwiseRequiredFaculty;
                        totalReqFaculty = totalReqFaculty + (int)Pharmacy.SpecializationwiseRequiredFaculty;
                        if (Pharmacy.SpecializationwiseAvilableFaculty >= Pharmacy.SpecializationwiseRequiredFaculty)
                        {
                            Pharmacy.Deficiency = "No Deficiency";
                        }
                        else
                        {
                            Pharmacy.Deficiency = "Deficiency";
                        }

                        var AvailiableFacultyRegNos = FacultyData.Where(f => f.CollegeCode == cid && f.Specialization == item.specializationId.ToString() && f.PharmacySpecialization == Pharmacy.GroupId && f.Deficiency != null).Select(q => q.Deficiency).Distinct().ToList();

                        var PHDFacultyList = db.jntuh_registered_faculty.Where(q => AvailiableFacultyRegNos.Contains(q.RegistrationNumber)).Select(a => a).ToList();

                        //var PHDFacultyCleared = PHDFacultyList.Where(rf => rf.type != "Adjunct" && ((rf.NoForm16 == false || rf.NoForm16 == null) && (rf.NoForm26AS == false || rf.NoForm26AS == null) && (rf.Absent == false || rf.Absent == null) && (rf.AbsentforVerification == false || rf.AbsentforVerification == null) && (rf.OriginalCertificatesNotShown == false || rf.OriginalCertificatesNotShown == null) && (rf.Xeroxcopyofcertificates == false || rf.Xeroxcopyofcertificates == null) && (rf.NotQualifiedAsperAICTE == false || rf.NotQualifiedAsperAICTE == null)
                        //                                 && (rf.Noclass == false || rf.Noclass == null) && (rf.FakePHD == false || rf.FakePHD == null) && (rf.Genuinenessnotsubmitted == false || rf.Genuinenessnotsubmitted == null) && (rf.NotconsideredPHD == false || rf.NotconsideredPHD == null) && (rf.NoPGspecialization == false || rf.NoPGspecialization == null) && (rf.NoSCM == false || rf.NoSCM == null) && (rf.PANNumber != null) && (rf.IncompleteCertificates == false || rf.IncompleteCertificates == null) && (rf.Blacklistfaculy == false) && rf.NoRelevantUG != "Yes" && rf.NoRelevantPG != "Yes" && rf.NORelevantPHD != "Yes" && (rf.InvalidPANNumber == false || rf.InvalidPANNumber == null) && (rf.OriginalsVerifiedPHD != true) && (rf.OriginalsVerifiedUG != true) && (rf.Invaliddegree == false || rf.Invaliddegree == null) && rf.InvalidAadhaar != "Yes")).Select(rf => rf).ToList();
                        //var PHDFacultyCleared = PHDFacultyList.Where(rf => rf.type != "Adjunct" && ((rf.NotQualifiedAsperAICTE == false || rf.NotQualifiedAsperAICTE == null) && (rf.Noclass == false || rf.Noclass == null) && (rf.Absent == false || rf.Absent == null) && (rf.OriginalCertificatesNotShown == false || rf.OriginalCertificatesNotShown == null) &&
                        //                                 (rf.NoSCM == false || rf.NoSCM == null) && rf.NoRelevantUG != "Yes" && rf.NoRelevantPG != "Yes" && rf.NORelevantPHD != "Yes" && (rf.InvalidPANNumber == false || rf.InvalidPANNumber == null) && (rf.Blacklistfaculy == false) && (rf.AbsentforVerification == false || rf.AbsentforVerification == null))).Select(rf => rf).ToList();
                        var PHDFacultyCleared = PHDFacultyList.Where(rf => rf.type != "Adjunct" && (rf.BAS != "Yes") && (rf.Noclass == false || rf.Noclass == null) && (rf.Absent == false || rf.Absent == null) &&
                                                            (rf.NoSCM == false || rf.NoSCM == null) && (rf.Blacklistfaculy == false) && (rf.AbsentforVerification == false || rf.AbsentforVerification == null)).Select(rf => rf).ToList();
                        var PHDFaculty = PHDFacultyCleared.Select(q => q.id).ToList();
                        int PhdRegNOCount = db.jntuh_registered_faculty_education.Count(q => PHDFaculty.Contains(q.facultyId) && q.educationId == 6);

                        Pharmacy.PHdFaculty = PhdRegNOCount;

                        if (i == 1)
                        {
                            SecondYerintake = GetPharmacyIntake(Pharmacy.Collegeid, AY1, Pharmacy.SpecializationId, item.shiftId, 1);
                            ThirdYerintake = GetPharmacyIntake(Pharmacy.Collegeid, AY2, Pharmacy.SpecializationId, item.shiftId, 1);
                            FourthYerintake = GetPharmacyIntake(Pharmacy.Collegeid, AY3, Pharmacy.SpecializationId, item.shiftId, 1);

                            faculty += "<tr>";
                            faculty += "<td class='col2' style='text-align: center; vertical-align: top;width:10px' rowspan='5'>" + (Count++) + "</td>";
                            faculty += "<td class='col2' style='text-align: center; vertical-align: top;' rowspan='4'>Pharmacy</td>";
                            faculty += "<td class='col2' style='text-align: center; vertical-align: top;'  rowspan='4'>" + item.Department + "</td>";
                            faculty += "<td class='col2' style='text-align: center; vertical-align: top;' rowspan='4'>" + item.Specialization + "</td>";
                            faculty += "<td class='col2' style='text-align: center; vertical-align: top;' rowspan='4' >";
                            faculty += "<table width='100%' border='1' cellpadding='3' cellspacing='0' style='border-color: #ccc;font-size:13px'>";
                            faculty += "<tr>";
                            faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>II</td>";
                            faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>III</td>";
                            faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>IV</td>";
                            faculty += "</tr>";
                            faculty += "<tr>";
                            faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + SecondYerintake + "</td>";
                            faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + ThirdYerintake + "</td>";
                            faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + FourthYerintake + "</td>";
                            faculty += "</tr>";
                            faculty += "</table>";

                            var PharmD = collegeIntakeExisting.Where(a => a.DepartmentID == 27).Select(a => a.Department).FirstOrDefault();
                            if (!String.IsNullOrEmpty(PharmD))
                            {
                                PharmDFirstYerintake = GetPharmacyIntake(Pharmacy.Collegeid, AY1, 18, item.shiftId, 1);
                                PharmDSecondYerintake = GetPharmacyIntake(Pharmacy.Collegeid, AY2, 18, item.shiftId, 1);
                                PharmDThirdYerintake = GetPharmacyIntake(Pharmacy.Collegeid, AY3, 18, item.shiftId, 1);
                                PharmDFourthYerintake = GetPharmacyIntake(Pharmacy.Collegeid, AY4, 18, item.shiftId, 1);
                                PharmDFifthhYerintake = GetPharmacyIntake(Pharmacy.Collegeid, AY5, 18, item.shiftId, 1);

                                faculty += "<br/><table width='100%' border='1' cellpadding='3' cellspacing='0' style='border-color: #ccc;font-size:13px'>";
                                faculty += "<tr>";
                                faculty += "<td class='col2' style='text-align: center; vertical-align: top;'  colspan='5'><b>Pharm.D</b></td>";
                                faculty += "</tr>";
                                faculty += "<tr>";
                                faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>I</td>";
                                faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>II</td>";
                                faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>III</td>";
                                faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>IV</td>";
                                faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>V</td>";
                                faculty += "</tr>";
                                faculty += "<tr>";
                                faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + PharmDFirstYerintake + "</td>";
                                faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + PharmDSecondYerintake + "</td>";
                                faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + PharmDThirdYerintake + "</td>";
                                faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + PharmDFourthYerintake + "</td>";
                                faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + PharmDFifthhYerintake + "</td>";
                                faculty += "</tr>";
                                faculty += "</table>";
                            }
                            //var PharmD = collegeIntakeExisting.Where(a => a.DepartmentID == ).Select(a => a.Department).FirstOrDefault();
                            faculty += "</td>";

                        }

                        if (i > 1)
                        {
                            faculty += "<tr>";
                        }
                        var grpdefCount = (Pharmacy.SpecializationwiseRequiredFaculty - Pharmacy.SpecializationwiseAvilableFaculty);
                        var grpdeficientyInFcultyStr = grpdefCount == 0 ? "-" : grpdefCount.ToString();

                        faculty += "<td class='col2' style='text-align: center; vertical-align: top;'></td>";
                        faculty += "<td class='col2' style='text-align: center; vertical-align: top;'></td>";
                        faculty += "<td class='col2' style='text-align: center; vertical-align: top;'></td>";
                        faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + Pharmacy.PharmacySpecialization + "</td>";
                        faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + Pharmacy.SpecializationwiseRequiredFaculty + "</td>";
                        faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + Pharmacy.SpecializationwiseAvilableFaculty + "</td>";
                        faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + Pharmacy.Deficiency + "</td>";
                        faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + Pharmacy.PHdFaculty + "</td>";
                        faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + grpdeficientyInFcultyStr + "</td>";
                        faculty += "</tr>";

                        PharmacyAppealFaculty.Add(Pharmacy);
                    }

                    PharmacyReportsClass BPharmacyObj = new PharmacyReportsClass();
                    BPharmacyObj.Collegeid = PharmacyAppealFaculty.Select(z => z.Collegeid).FirstOrDefault();
                    BPharmacyObj.Degree = "B.Pharmacy";
                    BPharmacyObj.DepartmentId = 26;
                    BPharmacyObj.Department = "B.Pharmacy";
                    BPharmacyObj.SpecializationId = 12;
                    BPharmacyObj.Specialization = "B.Pharmacy";
                    BPharmacyObj.ShiftId = PharmacyAppealFaculty.Select(z => z.ShiftId).FirstOrDefault();
                    BPharmacyObj.TotalIntake = PharmacyAppealFaculty.Where(a => a.DepartmentId == 26).Select(z => z.TotalIntake).FirstOrDefault();
                    BPharmacyObj.ProposedIntake = PharmacyAppealFaculty.Where(a => a.DepartmentId == 26).Select(z => z.ProposedIntake).FirstOrDefault();
                    BPharmacyObj.NoOfFacultyRequired = PharmacyAppealFaculty.Where(a => a.DepartmentId == 26).Select(z => z.NoOfFacultyRequired).FirstOrDefault();
                    BPharmacyObj.NoOfAvilableFaculty = PharmacyAppealFaculty.Where(a => a.DepartmentId == 26).Select(z => z.NoOfAvilableFaculty).FirstOrDefault();
                    BPharmacyObj.Deficiency = PharmacyAppealFaculty.Count(a => a.DepartmentId == 26 && a.Deficiency == "Deficiency") > 0 ? "Deficiency" : "No Deficiency";
                    BPharmacyObj.PHdFaculty = PharmacyAppealFaculty.Where(a => a.DepartmentId == 26).Select(q => q.PHdFaculty).Sum();
                    BPharmacyObj.IsActive = true;
                    PharmacyAppealFaculty.Add(BPharmacyObj);
                    TotalAreaRequiredFaculty = TotalAreaRequiredFaculty + totalReqFaculty;
                    var defCount = (totalReqFaculty - BPharmacyObj.NoOfAvilableFaculty);
                    var deficientyInFcultyStr = defCount == 0 ? "-" : defCount.ToString();
                    faculty += "<tr>";
                    //  faculty += "<td class='col2' style='text-align: center; vertical-align: top;'></td>";
                    faculty += "<td class='col2' style='text-align: center; vertical-align: top; font-weight: bold;'>" + BPharmacyObj.Degree + "</td>";
                    faculty += "<td class='col2' style='text-align: center; vertical-align: top; font-weight: bold;'>" + BPharmacyObj.Department + "</td>";
                    faculty += "<td class='col2' style='text-align: center; vertical-align: top; font-weight: bold;'>" + BPharmacyObj.Specialization + "</td>";
                    faculty += "<td class='col2' style='text-align: center; vertical-align: top; font-weight: bold;'>" + BPharmacyObj.TotalIntake + "</td>";
                    if (BPharmacyObj.ProposedIntake > 100)
                        faculty += "<td class='col2' style='text-align: center; vertical-align: top; font-weight: bold;'>100</td>";
                    else
                        faculty += "<td class='col2' style='text-align: center; vertical-align: top; font-weight: bold;'>" + BPharmacyObj.ProposedIntake + "</td>";

                    faculty += "<td class='col2' style='text-align: center; vertical-align: top; font-weight: bold;'>" + totalReqFaculty + "</td>";
                    faculty += "<td class='col2' style='text-align: center; vertical-align: top; font-weight: bold;'>" + BPharmacyObj.NoOfAvilableFaculty + "</td>";
                    faculty += "<td class='col2' style='text-align: center; vertical-align: top; font-weight: bold;'></td>";
                    faculty += "<td class='col2' style='text-align: center; vertical-align: top; font-weight: bold;'>" + totalReqFaculty + "</td>";
                    faculty += "<td class='col2' style='text-align: center; vertical-align: top; font-weight: bold;'>" + BPharmacyObj.NoOfAvilableFaculty + "</td>";
                    faculty += "<td class='col2' style='text-align: center; vertical-align: top; font-weight: bold;'>" + BPharmacyObj.Deficiency + "</td>";
                    faculty += "<td class='col2' style='text-align: center; vertical-align: top; font-weight: bold;'>" + BPharmacyObj.PHdFaculty + "</td>";
                    faculty += "<td class='col2' style='text-align: center; vertical-align: top; font-weight: bold;'>" + deficientyInFcultyStr + "</td>";
                    faculty += "</tr>";

                    if (BPharmacyObj.Deficiency == "Deficiency")
                        faculty += "<tr><td colspan='14' style='text-align:center;'><b>Note :B.Pharmacy Deficiency Exists & Hence other Degrees will not be considered.</b></td></tr>";
                }
                else if (item.Degree == "Pharm.D" || item.Degree == "Pharm.D PB")
                {
                    PharmacyReportsClass Pharmacy = new PharmacyReportsClass();
                    Pharmacy.Collegeid = item.collegeId;
                    Pharmacy.Degree = item.Degree;
                    Pharmacy.DepartmentId = item.DepartmentID;
                    Pharmacy.Department = item.Department;
                    Pharmacy.SpecializationId = item.specializationId;
                    Pharmacy.Specialization = item.Specialization;
                    Pharmacy.TotalIntake = (PharmDFirstYerintake + PharmDSecondYerintake + PharmDThirdYerintake + PharmDFourthYerintake + PharmDFifthhYerintake);
                    Pharmacy.ProposedIntake = GetPharmacyIntake(item.collegeId, AY0, item.specializationId, item.shiftId, 0);
                    if (PharmacyAppealFaculty.Count(a => a.DepartmentId == 26 && a.Deficiency == "Deficiency") > 0)
                        Pharmacy.Deficiency = "Deficiency";
                    else
                        Pharmacy.Deficiency = "No Deficiency";

                    PharmacyAppealFaculty.Add(Pharmacy);

                    faculty += "<tr>";
                    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + (Count++) + "</td>";
                    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + Pharmacy.Degree + "</td>";
                    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + Pharmacy.Department + "</td>";
                    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + Pharmacy.Specialization + "</td>";
                    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'></td>";
                    //faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + Pharmacy.TotalIntake + "</td>";
                    if (item.Degree == "Pharm.D")
                    {
                        faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>30</td>";
                    }
                    else if (item.Degree == "Pharm.D PB")
                    {
                        faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>10</td>";
                    }
                    //faculty += "<td class='col2' style='text-align: center; vertical-align: top;'></td>";
                    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'></td>";
                    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'></td>";
                    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'></td>";
                    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'></td>";
                    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'></td>";
                    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + Pharmacy.Deficiency + "</td>";
                    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + Pharmacy.PHdFaculty + "</td>";
                    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'></td>";
                    faculty += "</tr>";
                }
                else if (item.Degree == "M.Pharmacy")
                {
                    PharmacyReportsClass Pharmacy = new PharmacyReportsClass();
                    Pharmacy.Collegeid = item.collegeId;
                    Pharmacy.Degree = item.Degree;
                    Pharmacy.DepartmentId = item.DepartmentID;
                    Pharmacy.Department = item.Department;
                    Pharmacy.SpecializationId = item.specializationId;
                    Pharmacy.Specialization = item.Specialization;
                    Pharmacy.ShiftId = item.shiftId;
                    //Pharmacy.ProposedIntake = FacultyData.Where(f => f.CollegeCode == cid && f.Specialization == item.specializationId.ToString()).Select(f => f.ProposedIntake).FirstOrDefault();
                    //Pharmacy.TotalIntake = FacultyData.Where(f => f.CollegeCode == cid && f.Specialization == item.specializationId.ToString()).Select(f => f.TotalIntake).FirstOrDefault();
                    Pharmacy.ProposedIntake = GetPharmacyIntake(item.collegeId, AY0, item.specializationId, item.shiftId, 0);
                    var approveIntake1 = GetPharmacyIntake(item.collegeId, AY1, item.specializationId, item.shiftId, 1);
                    var approveIntake2 = GetPharmacyIntake(item.collegeId, AY2, item.specializationId, item.shiftId, 1);
                    Pharmacy.TotalIntake = (approveIntake1 != null ? approveIntake1 : 0) + (approveIntake2 != null ? approveIntake2 : 0);
                    Pharmacy.NoOfFacultyRequired = FacultyData.Where(f => f.CollegeCode == cid && f.Specialization == item.specializationId.ToString()).Select(f => f.NoOfFacultyRequired).LastOrDefault();
                    Pharmacy.NoOfAvilableFaculty = FacultyData.Count(f => f.CollegeCode == cid && f.Specialization == item.specializationId.ToString() && f.Deficiency != null);
                    Pharmacy.SpecializationwiseRequiredFaculty = FacultyData.Where(f => f.CollegeCode == cid && f.Specialization == item.specializationId.ToString()).Select(f => f.SpecializationWiseRequiredFaculty).LastOrDefault();
                    Pharmacy.SpecializationwiseAvilableFaculty = FacultyData.Where(f => f.CollegeCode == cid && f.Specialization == item.specializationId.ToString() && f.Deficiency != null).Select(a => a.Deficiency).Distinct().Count();

                    var AvailiableFacultyRegNos = FacultyData.Where(f => f.CollegeCode == cid && f.Specialization == item.specializationId.ToString() && f.Deficiency != null).Select(q => q.Deficiency).Distinct().ToList();

                    var PHDFacultyList = db.jntuh_registered_faculty.Where(q => AvailiableFacultyRegNos.Contains(q.RegistrationNumber)).Select(a => a).ToList();

                    //var PHDFacultyCleared = PHDFacultyList.Where(rf => rf.type != "Adjunct" && ((rf.NoForm16 == false || rf.NoForm16 == null) && (rf.NoForm26AS == false || rf.NoForm26AS == null) && (rf.Absent == false || rf.Absent == null) && (rf.AbsentforVerification == false || rf.AbsentforVerification == null) && (rf.OriginalCertificatesNotShown == false || rf.OriginalCertificatesNotShown == null) && (rf.Xeroxcopyofcertificates == false || rf.Xeroxcopyofcertificates == null) && (rf.NotQualifiedAsperAICTE == false || rf.NotQualifiedAsperAICTE == null)
                    //                                 && (rf.Noclass == false || rf.Noclass == null) && (rf.FakePHD == false || rf.FakePHD == null) && (rf.Genuinenessnotsubmitted == false || rf.Genuinenessnotsubmitted == null) && (rf.NotconsideredPHD == false || rf.NotconsideredPHD == null) && (rf.NoPGspecialization == false || rf.NoPGspecialization == null) && (rf.NoSCM == false || rf.NoSCM == null) && (rf.PANNumber != null) && (rf.IncompleteCertificates == false || rf.IncompleteCertificates == null) && (rf.Blacklistfaculy == false) && rf.NoRelevantUG != "Yes" && rf.NoRelevantPG != "Yes" && rf.NORelevantPHD != "Yes" && (rf.InvalidPANNumber == false || rf.InvalidPANNumber == null) && (rf.OriginalsVerifiedPHD != true) && (rf.OriginalsVerifiedUG != true) && (rf.Invaliddegree == false || rf.Invaliddegree == null) && rf.InvalidAadhaar != "Yes") && rf.BAS != "Yes").Select(rf => rf).ToList();
                    //var PHDFacultyCleared = PHDFacultyList.Where(rf => rf.type != "Adjunct" && ((rf.NotQualifiedAsperAICTE == false || rf.NotQualifiedAsperAICTE == null) && (rf.Noclass == false || rf.Noclass == null) && (rf.Absent == false || rf.Absent == null) && (rf.OriginalCertificatesNotShown == false || rf.OriginalCertificatesNotShown == null) &&
                    //                                     (rf.NoSCM == false || rf.NoSCM == null) && rf.NoRelevantUG != "Yes" && rf.NoRelevantPG != "Yes" && rf.NORelevantPHD != "Yes" && (rf.InvalidPANNumber == false || rf.InvalidPANNumber == null) && (rf.Blacklistfaculy == false) && (rf.AbsentforVerification == false || rf.AbsentforVerification == null))).Select(rf => rf).ToList();
                    var PHDFacultyCleared = PHDFacultyList.Where(rf => rf.type != "Adjunct" && (rf.BAS != "Yes") && (rf.Noclass == false || rf.Noclass == null) && (rf.Absent == false || rf.Absent == null) &&
                                                            (rf.NoSCM == false || rf.NoSCM == null) && (rf.Blacklistfaculy == false) && (rf.AbsentforVerification == false || rf.AbsentforVerification == null)).Select(rf => rf).ToList();

                    var PHDFaculty = PHDFacultyCleared.Select(q => q.id).ToList();
                    int PhdRegNOCount = db.jntuh_registered_faculty_education.Count(q => PHDFaculty.Contains(q.facultyId) && q.educationId == 6);

                    Pharmacy.PHdFaculty = PhdRegNOCount;

                    if (Pharmacy.SpecializationwiseAvilableFaculty >= Pharmacy.SpecializationwiseRequiredFaculty)
                    {
                        if (PharmacyAppealFaculty.Count(a => a.DepartmentId == 26 && a.Deficiency == "Deficiency") > 0)
                            Pharmacy.Deficiency = "Deficiency";
                        else
                            Pharmacy.Deficiency = "No Deficiency";
                    }
                    else
                    {
                        Pharmacy.Deficiency = "Deficiency";
                    }

                    if (Pharmacy.ShiftId == 1)
                    {
                        var mpdefCount = (Pharmacy.NoOfFacultyRequired - Pharmacy.NoOfAvilableFaculty);
                        var mpdeficientyInFcultyStr = mpdefCount == 0 ? "-" : mpdefCount.ToString();
                        faculty += "<tr>";
                        faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + (Count++) + "</td>";
                        faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + Pharmacy.Degree + "</td>";
                        faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + Pharmacy.Department + "</td>";
                        faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + Pharmacy.Specialization + "</td>";
                        faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + Pharmacy.TotalIntake + "</td>";
                        if (Pharmacy.ProposedIntake > 15)
                            faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>15</td>";
                        else
                            faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + Pharmacy.ProposedIntake + "</td>";
                        //faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + Pharmacy.ProposedIntake + "</td>";
                        faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + Pharmacy.SpecializationwiseRequiredFaculty + "</td>";
                        faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + Pharmacy.SpecializationwiseAvilableFaculty + "</td>";
                        faculty += "<td class='col2' style='text-align: center; vertical-align: top;'></td>";
                        faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + Pharmacy.NoOfFacultyRequired + "</td>";
                        faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + Pharmacy.NoOfAvilableFaculty + "</td>";
                        faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + Pharmacy.Deficiency + "</td>";
                        faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + Pharmacy.PHdFaculty + "</td>";
                        faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + mpdeficientyInFcultyStr + "</td>";
                        faculty += "</tr>";
                    }
                    TotalAreaRequiredFaculty = TotalAreaRequiredFaculty + (Pharmacy.SpecializationwiseRequiredFaculty != null ? (int)Pharmacy.SpecializationwiseRequiredFaculty : 0);
                    PharmacyAppealFaculty.Add(Pharmacy);
                }
            }
            DateTime lasteditfromdate = new DateTime(2022, 11, 29);
            var regnos = db.jntuh_college_faculty_registered.Where(c => c.collegeId == collegeID && c.createdOn >= lasteditfromdate).Select(s => s).ToList();

            var commanfaculty = from cf in db.jntuh_college_faculty_registered
                                from pf in db.jntuh_college_previous_academic_faculty
                                where (cf.RegistrationNumber == pf.RegistrationNumber && cf.collegeId == pf.collegeId && cf.collegeId == collegeID)
                                select new
                                {
                                    cf.RegistrationNumber,
                                    cf.collegeId,
                                };
            int commanfacultycount = commanfaculty.Count();
            int newfaculty = registeredFacultyNew.Count() - commanfacultycount;
            var regFacultyWothoutAbsents = registeredFacultyNew.Where(rf => (rf.Absent == false || rf.Absent == null)).ToList();

            //var jntuh_registered_faculty2 =
            //      registeredFacultyNew.Where(rf => rf.type != "Adjunct" && ((rf.NoForm16 == false || rf.NoForm16 == null) && (rf.NoForm26AS == false || rf.NoForm26AS == null) && (rf.Absent == false || rf.Absent == null) && (rf.AbsentforVerification == false || rf.AbsentforVerification == null) && (rf.OriginalCertificatesNotShown == false || rf.OriginalCertificatesNotShown == null) && (rf.Xeroxcopyofcertificates == false || rf.Xeroxcopyofcertificates == null) && (rf.NotQualifiedAsperAICTE == false || rf.NotQualifiedAsperAICTE == null)
            //                                          && (rf.Noclass == false || rf.Noclass == null) && (rf.FakePHD == false || rf.FakePHD == null) && (rf.Genuinenessnotsubmitted == false || rf.Genuinenessnotsubmitted == null) && (rf.NotconsideredPHD == false || rf.NotconsideredPHD == null) && (rf.NoPGspecialization == false || rf.NoPGspecialization == null) && (rf.NoSCM == false || rf.NoSCM == null) && (rf.PANNumber != null) && (rf.IncompleteCertificates == false || rf.IncompleteCertificates == null) && (rf.Blacklistfaculy == false) && rf.NoRelevantUG != "Yes" && rf.NoRelevantPG != "Yes" && rf.NORelevantPHD != "Yes" && (rf.InvalidPANNumber == false || rf.InvalidPANNumber == null) && (rf.OriginalsVerifiedPHD != true) && (rf.OriginalsVerifiedUG != true) && (rf.Invaliddegree == false || rf.Invaliddegree == null) && rf.InvalidAadhaar != "Yes") && rf.BAS != "Yes").Select(rf => rf.RegistrationNumber).ToList();
            //var jntuh_registered_faculty2 =
            //      registeredFacultyNew.Where(rf => rf.type != "Adjunct" && ((rf.NotQualifiedAsperAICTE == false || rf.NotQualifiedAsperAICTE == null) && (rf.Noclass == false || rf.Noclass == null) && (rf.Absent == false || rf.Absent == null) && (rf.OriginalCertificatesNotShown == false || rf.OriginalCertificatesNotShown == null) &&
            //                                             (rf.NoSCM == false || rf.NoSCM == null) && rf.NoRelevantUG != "Yes" && rf.NoRelevantPG != "Yes" && rf.NORelevantPHD != "Yes" && (rf.InvalidPANNumber == false || rf.InvalidPANNumber == null) && (rf.Blacklistfaculy == false) && (rf.AbsentforVerification == false || rf.AbsentforVerification == null))).Select(rf => rf.RegistrationNumber).ToList();
            var jntuh_registered_faculty2 =
                  registeredFacultyNew.Where(rf => rf.type != "Adjunct" && (rf.BAS != "Yes") && (rf.Noclass == false || rf.Noclass == null) && (rf.Absent == false || rf.Absent == null) &&
                                                            (rf.NoSCM == false || rf.NoSCM == null) && (rf.Blacklistfaculy == false) && (rf.AbsentforVerification == false || rf.AbsentforVerification == null)).Select(rf => rf.RegistrationNumber).ToList();

            faculty += "<tr><td align='center' colspan='14' style='font-size: 14px; font-weight: normal;'><b> Additional Faculty  :" + (jntuh_registered_faculty2.Count() - AdditionalFaculty) + "</b></td></tr>";
            faculty += "<tr><td align='center' colspan='14' style='font-size: 14px; font-weight: normal;'><b> Total Faculty : " + jntuh_registered_faculty2.Count() + " </b></td></tr>";
            faculty += "</table>";

            faculty += "<table width='100%' border='0' cellpadding='5' cellspacing='0'>";
            faculty += "<tr>";
            faculty += "<td align='left'>* II, III & IV Year for B.Pharmacy</td>";
            faculty += "<td align='left'>* I, II Year for M.Pharmacy</td>";
            faculty += "<td align='left'>* I, II, III , IV & V Year for Pharm.D</td>";
            faculty += "<td align='left'>* IV, V Year for Pharm.D PB</td>";
            faculty += "</tr>";
            faculty += "</table>";

            #region Pending Issues
            //var AffiliationFee = db.jntuh_college_paymentoffee.Where(e => e.collegeId == collegeID && e.FeeTypeID == 5).Select(e => e).FirstOrDefault();
            //var CommanserviceFee = db.jntuh_college_paymentoffee.Where(e => e.collegeId == collegeID && e.FeeTypeID == 3).Select(e => e).FirstOrDefault();
            var AffiliationFee2021 = db.jntuh_college_paymentoffee.Where(e => e.collegeId == collegeID && e.FeeTypeID == 5 && (e.academicyearId == (AY0 - 2))).Select(e => e).FirstOrDefault();
            var CommanserviceFee2021 = db.jntuh_college_paymentoffee.Where(e => e.collegeId == collegeID && e.FeeTypeID == 3 && (e.academicyearId == (AY0 - 2))).Select(e => e).FirstOrDefault();
            //var AffiliationFee1920 = db.jntuh_college_paymentoffee.Where(e => e.collegeId == collegeID && e.FeeTypeID == 5 && (e.academicyearId == (AY0 - 2))).Select(e => e).FirstOrDefault();
            //var CommanserviceFee1920 = db.jntuh_college_paymentoffee.Where(e => e.collegeId == collegeID && e.FeeTypeID == 3 && (e.academicyearId == (AY0 - 2))).Select(e => e).FirstOrDefault();
            var AffiliationFeeDues2022_23 = db.jntuh_college_paymentoffee.Where(e => e.collegeId == collegeID && e.FeeTypeID == 5 && e.academicyearId == (AY0 - 1)).Select(e => e.duesAmount).FirstOrDefault();
            faculty += "<br/><table><tr><td align='left'><b><u>Pending Dues (in Rupees):</u></b></td>";
            faculty += "</tr></table>";
            //faculty += "<ul style='list-style-type:disc'>";
            //if (CommanserviceFee.paidAmount != null)
            //{
            //    faculty += "<li>Common Service Fee Due:<b> Rs." + CommanserviceFee.paidAmount + "</b></li>";
            //}

            //if (AffiliationFee.duesAmount != null)
            //{
            //    faculty += "<li>Affiliation Fee Due: <b>Rs." + AffiliationFee.duesAmount + "</b></li>";
            //}
            faculty += "<table width='100%' border='1' cellpadding='3' cellspacing='0' style='border-color: #ccc;'>";
            faculty += "<tr>";
            faculty += "<th style='text-align: left; vertical-align: top;'>Affiliation Fee 2022-23</th>";
            //faculty += "<th style='text-align: left; vertical-align: top;' >Common Service Fee 2019-20</th>";
            faculty += "<th style='text-align: left; vertical-align: top;' >Affiliation Fee 2021-22</th>";
            faculty += "<th style='text-align: left; vertical-align: top;' >Common Service Fee 2021-22</th>";
            faculty += "</tr>";
            faculty += "<tr>";
            //if (AffiliationFee1920 != null)
            //{
            //    faculty +=
            //    "<td class='col2' style='text-align: left; vertical-align: top;'>" +
            //     AffiliationFee1920.duesAmount + "</td>";
            //}
            //else
            //{
            //    faculty +=
            //   "<td class='col2' style='text-align: left; vertical-align: top;'>" +
            //   "0.00" + "</td>";
            //}

            //if (CommanserviceFee1920 != null)
            //{
            //    faculty += "<td class='col2' style='text-align: left; vertical-align: top;'>" +
            //   CommanserviceFee1920.duesAmount.ToString() + "</td>";
            //}
            //else
            //{
            //    faculty += "<td class='col2' style='text-align: left; vertical-align: top;'>" +
            //   "0.00" + "</td>";
            //}

            AffiliationFeeDues2022_23 = (AffiliationFeeDues2022_23 == null) ? "0" : AffiliationFeeDues2022_23;
            faculty += "<td class='col2' style='text-align: left; vertical-align: top; font-weight: bold'>" + AffiliationFeeDues2022_23 + "</td>";
            if (AffiliationFee2021 != null)
            {
                faculty += "<td class='col2' style='text-align: left; vertical-align: top; font-weight: bold'>" +
                AffiliationFee2021.duesAmount + "</td>";
            }
            else
            {
                faculty += "<td class='col2' style='text-align: left; vertical-align: top; font-weight: bold'>" +
                "0.00" + "</td>";
            }
            if (CommanserviceFee2021 != null)
            {
                faculty += "<td class='col2' style='text-align: left; vertical-align: top; font-weight: bold'>" +
                CommanserviceFee2021.duesAmount.ToString() + "</td>";
            }
            else
            {
                faculty += "<td class='col2' style='text-align: left; vertical-align: top; font-weight: bold'>" +
                "0.00" + "</td>";
            }
            faculty += "</tr>";
            faculty += "</table><br/>";

            #endregion

            #region OTHER OBSERVATIONS/  REMARKS

            var lastyearfacultycount = db.jntuh_notin415faculty.Where(i => i.CollegeId == collegeID).Select(i => i).FirstOrDefault();
            var AICTEFaculty = db.jntuh_college_aictefaculty.Where(a => a.CollegeId == collegeID).ToList();

            int AICTEFacultyCount = AICTEFaculty.Count();

            faculty += "<table><tr><td align='left'><b><u>Faculty Details:</u></b></td>";
            faculty += "</tr></table>";
            //faculty += "<ul style='list-style-type:disc'>";
            //faculty += "<li>Total faculty uploaded to AICTE for Extension of Approval for AY 2022-23 &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; : " + AICTEFacultyCount + ".</li>";
            //faculty += "<li>Total faculty available during inspection with  qualifications as prescribed by AICTE &nbsp;&nbsp;&nbsp;:<b> " + jntuh_registered_faculty2.Count() + "</b>.</li>";
            //if (newfaculty != null && newfaculty != 0)
            //{
            //    faculty += "<li>Number of faculty recruited after the last inspection  &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; : " + newfaculty + ".</li>";
            //}
            //else
            //{
            //    faculty += "<li>Number of faculty recruited after the last inspection &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;   :0. </li>";
            //}
            //faculty += "<li>Total faculty uploaded by the college for A.Y. 2023-24 &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; : " + registeredFacultyNew.Count() + ".</li>";
            //faculty += "<li>Total faculty available during inspection &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; : " + regFacultyWothoutAbsents.Count + ".</li>";
            //faculty += "<li>Total faculty appointed after inspection for the A.Y. 2022-23. &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; : " + regnos.Count + ".</li>";

            //faculty += "</ul>";

            faculty += "<table width='100%' border='1' cellpadding='5' cellspacing='0'>";
            faculty += "<tr>";
            faculty += "<td align='left'>Total faculty required by the college for the A.Y. 2023-24.</td><td>" + TotalAreaRequiredFaculty + "</td>";
            faculty += "<td align='left'>Total faculty uploaded by the college for A.Y. 2023-24.</td><td>" + registeredFacultyNew.Count() + "</td>";
            faculty += "</tr>";
            faculty += "<tr>";
            faculty += "<td align='left'>Total faculty available during inspection.</td><td>" + regFacultyWothoutAbsents.Count + "</td>";
            faculty += "<td align='left'>Total faculty appointed after inspection for the A.Y. 2022-23.</td><td>" + regnos.Count + "</td>";
            faculty += "</tr>";
            faculty += "<tr>";
            faculty += "<td align='left'>Total faculty having required BAS.</td><td>" + (registeredFacultyNew.Count() - jntuh_registered_facultyBAS.Count) + "</td>";
            faculty += "<td align='left'>Total faculty not having sufficient BAS.</td><td>" + (jntuh_registered_facultyBAS.Count) + "</td>";
            faculty += "</tr>";
            faculty += "</table>";
            #endregion

            //faculty += "</ul>";

            return faculty;
        }

        private int GetIntake(int collegeId, int academicYearId, int specializationId, int shiftId, int flag)
        {
            int intake = 0;

            //approved
            if (flag == 1 && academicYearId != 10)
            {
                intake = db.jntuh_college_intake_existing.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == 1).Select(i => i.approvedIntake).FirstOrDefault();

            }
            else if (flag == 1 && academicYearId == 10)
            {
                var inta = db.jntuh_college_intake_existing.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == 1).FirstOrDefault();
                if (inta != null)
                {
                    intake = (int)inta.proposedIntake;
                }

            }
            else //admitted
            {
                intake = db.jntuh_college_intake_existing.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == 1).Select(i => i.admittedIntake).FirstOrDefault();
            }

            return intake;
        }

        public string DeficiencyCollegeLabsAnnexure(int? collegeID)
        {
            var jntuh_academic_year = db.jntuh_academic_year.ToList();
            int presentYear = jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int AY1 = jntuh_academic_year.Where(a => a.actualYear == (presentYear + 1)).Select(a => a.id).FirstOrDefault();
            string annexure = string.Empty;
            var Departments = db.jntuh_department.Where(d => d.degreeId == 2 || d.degreeId == 5 || d.degreeId == 9 || d.degreeId == 10).ToList();
            int[] Departmentids = Departments.Select(d => d.id).ToArray();
            var Specializations = db.jntuh_specialization.Where(s => Departmentids.Contains(s.departmentId)).ToList();
            List<int> specializationIds1 = Specializations.Select(s => s.id).ToList();

            List<FacultyVerificationController.AnonymousLabclass> collegeLabAnonymousLabclass = new List<FacultyVerificationController.AnonymousLabclass>();
            List<int> specializationIds = db.jntuh_college_intake_existing.Where(e => e.collegeId == collegeID && e.academicYearId == AY1 && e.courseStatus != "Closure" && e.proposedIntake != 0).Select(e => e.specializationId).Distinct().ToList();

            int[] Equipmentsids = db.jntuh_college_laboratories.Where(C => C.CollegeID == collegeID).Select(C => C.EquipmentID).ToArray();

            var DepartmentsData = db.jntuh_specialization.Where(e => e.isActive == true && specializationIds.Contains(e.id)).Select(e => e.departmentId).Distinct().ToArray();

            var DegreeIds = db.jntuh_department.Where(e => e.isActive == true && DepartmentsData.Contains(e.id)).Select(e => e.degreeId).ToArray();

            if (DegreeIds.Contains(4))
            {
                specializationIds.Add(39);
            }
            string CollegeAffiliationStatus = db.jntuh_college_affiliation.Where(u => u.collegeId == collegeID && u.affiliationTypeId == 7).Select(u => u.affiliationStatus).FirstOrDefault();

            if (CollegeAffiliationStatus == "Yes")
            {
                collegeLabAnonymousLabclass = db.jntuh_lab_master.AsNoTracking()
                                                           .Where(l => l.CollegeId == collegeID && !Equipmentsids.Contains(l.id) && !l.EquipmentName.Contains("desirable"))
                                                           .Select(l => new FacultyVerificationController.AnonymousLabclass
                                                           {
                                                               // id = db.jntuh_appeal_college_laboratories.Where(l1 => l1.EquipmentID == l.id && l1.EquipmentNo == 1 && l1.CollegeID == collegeID).Select(l1 => l1.id).FirstOrDefault(),
                                                               EquipmentID = l.id,
                                                               LabName = l.LabName,
                                                               EquipmentName = l.EquipmentName,
                                                               Department = l.jntuh_department.departmentName,
                                                               LabCode = l.Labcode,
                                                               year = l.Year,
                                                               Semester = l.Semester,
                                                               specializationId = l.SpecializationID
                                                           })
                                                           .OrderBy(l => l.LabName)
                                                           .ThenBy(l => l.EquipmentName)
                                                           .ToList();

            }
            else if (CollegeAffiliationStatus == "No" || CollegeAffiliationStatus == null)
            {
                collegeLabAnonymousLabclass = db.jntuh_lab_master.AsNoTracking()
                                                              .Where(l => specializationIds.Contains(l.SpecializationID) && !Equipmentsids.Contains(l.id) && l.CollegeId == null && !l.EquipmentName.Contains("desirable"))
                                                              .Select(l => new FacultyVerificationController.AnonymousLabclass
                                                              {
                                                                  //  id = db.jntuh_appeal_college_laboratories.Where(l1 => l1.EquipmentID == l.id && l1.EquipmentNo == 1 && l1.CollegeID == collegeID).Select(l1 => l1.id).FirstOrDefault(),
                                                                  EquipmentID = l.id,
                                                                  LabName = l.LabName,
                                                                  EquipmentName = l.EquipmentName,
                                                                  Department = l.jntuh_department.departmentName,
                                                                  LabCode = l.Labcode,
                                                                  year = l.Year,
                                                                  Semester = l.Semester,
                                                                  specializationId = l.SpecializationID
                                                              })
                                                              .OrderBy(l => l.LabName)
                                                              .ThenBy(l => l.EquipmentName)
                                                              .ToList();
            }

            var list1 = collegeLabAnonymousLabclass.OrderBy(c => c.LabName).ThenBy(c => c.EquipmentName).Distinct().ToList();


            int?[] labequipmentIds = list1.Select(i => i.EquipmentID).ToArray();

            int[] clgequipmentIDs =
                db.jntuh_college_laboratories.Where(
                    l => l.CollegeID == collegeID && labequipmentIds.Contains(l.EquipmentID))
                    .Select(i => i.EquipmentID)
                    .ToArray();

            list1 = list1.Where(l => !clgequipmentIDs.Contains((int)l.EquipmentID))
                    .ToList();

            int[] SpecializationIDs;
            if (DegreeIds.Contains(4))
                SpecializationIDs = (from a in collegeLabAnonymousLabclass orderby a.Department select a.specializationId).Distinct().ToArray();
            else
                SpecializationIDs = (from a in collegeLabAnonymousLabclass where a.specializationId != 39 orderby a.Department select a.specializationId).Distinct().ToArray();

            if (list1.Count() > 0)
            {
                var specializations = db.jntuh_specialization.Where(it => SpecializationIDs.Contains(it.id)).Select(s => new
                {
                    s.id,
                    specialization = s.specializationName,
                    department = s.jntuh_department.departmentName,
                    degree = s.jntuh_department.jntuh_degree.degree,
                    deptId = s.jntuh_department.id,

                }).OrderBy(e => e.deptId).ToList();

                annexure += "<table width='100%' border='0' cellpadding='5' cellspacing='0'>";
                annexure += "<tr>";
                annexure += "<td align='left'><b><u>Deficiencies in Laboratory  </u></b></td>";
                annexure += "</tr>";
                annexure += "</table>";
                foreach (var speclializationId in SpecializationIDs)
                {
                    string LabNmae = "", EquipmentName = "", DepartmentName = "";
                    var specializationDetails = specializations.FirstOrDefault(s => s.id == speclializationId);
                    DepartmentName = list1.Where(l => l.specializationId == speclializationId).Select(l => l.Department).FirstOrDefault();
                    annexure += "<table width='100%' border='1' cellpadding='5' cellspacing='0'>";
                    annexure += "<tr>";
                    annexure += "<th align='left' colspan='3'> " + specializationDetails.degree + " -" + specializationDetails.department + "-" + specializationDetails.specialization + "</th>";
                    annexure += "</tr>";
                    annexure += "<tr>";
                    annexure += "<th align='left'>S.No</th><th align='left'>Lab Name</th><th align='left'>Equipment Name</th>";
                    annexure += "</tr>";
                    int LabsCount = 0;
                    int EquipmentsCount = 0;

                    var labs = list1.Where(l => l.specializationId == speclializationId).OrderBy(e => e.year).ThenBy(e => e.Semester).ToList();
                    int indexnow = 1;
                    foreach (var item in labs.ToList())
                    {
                        LabNmae = item.LabName.Trim() != null ? item.year + "-" + item.Semester + "-" + item.LabName : null;
                        EquipmentName = item.EquipmentName;
                        annexure += "<tr>";
                        annexure += "<td align='left'>" + indexnow + "</td><td  align='left'>" + LabNmae + "</td><td  align='left'>" + EquipmentName + "</td>";
                        annexure += "</tr>";
                        if (!string.IsNullOrEmpty(item.LabName))
                            LabsCount = 0;
                        if (!string.IsNullOrEmpty(item.EquipmentName))
                            EquipmentsCount = 0;
                        indexnow++;
                    }
                    annexure += "</table>";
                    annexure += "<br/>";
                }
            }
            else
            {
                annexure += "<table width='100%' border='0' cellpadding='5' cellspacing='0'>";
                annexure += "<tr>";
                annexure += "<td align='left'><b><u>Deficiencies in Laboratory  </u></b></td>";
                annexure += "</tr>";
                annexure += "</table>";
                annexure += "<table width='100%' border='1'  cellspacing='0'>";
                annexure += "<tr><td align='center'> <b>NIL</b></td></tr>";
                annexure += "</table>";
            }

            annexure += "</br><p> <b>NOTE:</b> The Physical Verification of the faculty and their presence at the time of Inspection by the FFC, automatically does not mean that the college is entitled for Affiliation based on numbers. Those of the faculty who are having the requisite qualifications,biometric and credentials are verified and found correct will be taken into account for the purpose of granting affiliation.</p>";
            //annexure += "</br><table width='100%'  cellspacing='0'></br>";
            //annexure += "<tr><td align='right' width='80%' style='font-size: 14px; font-weight: normal;'> <b>Sd /-</b></td></tr>";
            //annexure += "<tr><td align='right' width='80%' style='font-size: 14px; font-weight: normal;'> <b>REGISTRAR</b></td></tr></br></br>";
            //annexure += "</br><tr><td align='center' width='80%' style='font-size: 14px; font-weight: normal;' colspan='2'><b>The College shall submit Appeal, if any through Online mode only in the format " +
            //           "prescribed within 10 Days from the date of this letter." + "</b></td></tr></br>";
            //annexure += "<tr><td></td></tr>"; annexure += "</table>";

            return annexure;
        }

        ////New College complaints Added in Deficiency Reports 419(26-03-2019) by Narayana Reddy
        public string CollegeComplaintsController(int? collegeID)
        {
            string Complaints = string.Empty;
            var presentyear = DateTime.Now.Year - 1;
            List<jntuh_college_complaints> Collegecomplaints =
                db.jntuh_college_complaints.Where(c => c.college_faculty_Id == collegeID && c.roleId == 4 && c.complaintOn != "Closed" && c.complaintDate.Value.Year >= presentyear).OrderByDescending(o => o.complaintDate).ToList();
            Complaints += "<table width='100%' border='0' cellpadding='5' cellspacing='0'>";
            Complaints += "<tr>";
            Complaints += "<td align='left'><b><u>College Complaints</u></b></td>";
            Complaints += "</tr>";
            Complaints += "</table>";
            Complaints += "<table width='100%' border='1' cellpadding='7' cellspacing='0'>";

            Complaints += "<tr>";
            Complaints +=
                "<th align='left'>S.No</th><th align='left'>Complaint</th><th align='left'>Complaint Date</th><th align='left'>Complaint Givenby</th>";
            Complaints += "</tr>";
            int indexnow = 1;
            if (Collegecomplaints.Count != 0)
            {
                foreach (var item in Collegecomplaints)
                {
                    var complaint =
                        db.jntuh_complaints.Where(c => c.id == item.complaintId)
                            .Select(s => s.complaintType)
                            .FirstOrDefault();
                    string complaintdate = string.Empty;
                    if (item.complaintDate != null)
                        complaintdate = UAAAS.Models.Utilities.MMDDYY2DDMMYY(item.complaintDate.ToString());

                    Complaints += "<tr>";
                    Complaints += "<td align='left'>" + indexnow + "</td><td  align='left'>" + complaint +
                                "</td><td align=''left'>" + complaintdate + "</td><td align=''left'>" + item.givenBy + "</td>";
                    Complaints += "</tr>";
                    indexnow++;
                }
            }
            else
            {
                Complaints += "<tr>";
                Complaints += "<td colspan='6' align='center'><b>NIL</b></td>";
                Complaints += "</tr>";
            }
            Complaints += "</table>";
            return Complaints;
        }

        public string Annexure()
        {
            string annexure = string.Empty;
            annexure += "<table width='100%'  cellspacing='0'></br>";
            annexure +=
                "<tr><td align='right' width='80%' style='font-size: 14px; font-weight: normal;'> <b>Sd /-</b></td></tr>";
            annexure +=
                "<tr><td align='right' width='80%' style='font-size: 14px; font-weight: normal;'> <b>REGISTRAR</b></td></tr>";
            //annexure +=
            //    "</br><tr><td align='center' width='80%' style='font-size: 14px; font-weight: normal;' colspan='2'><b>The College shall submit Appeal, if any through Online mode only in the format " +
            //    "prescribed within 10 Days from the date of this letter." + "</b></td></tr></br>";
            //  annexure += "<tr><td></td></tr>";
            annexure += "</table>";
            return annexure;
        }

        #endregion

        #region Pharmacy Counselling Report Code Written by siva
        //Code written by Siva
        public ActionResult GroupWiseAddedFaculty(int? CollegeID)
        {
            var userId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            var collegeId = db.jntuh_college_users.Where(u => u.userID == userId).Select(u => u.collegeID).FirstOrDefault();
            if (collegeId == 375)
            {
                collegeId = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }

            var CollegeIds = new int[] { 448, 180, 159, 376, 428, 445, 24, 117, 202, 213, 395, 364, 27, 30, 6, 34, 55, 58, 54, 52, 9, 60, 65, 78, 150, 204, 219, 90, 97, 104, 110, 139, 105, 107, 114, 379, 118, 39, 120, 389, 127, 237, 135, 136, 47, 252, 169, 253, 262, 442, 301, 436, 370, 263, 267, 206, 140, 44, 290, 454, 42, 95, 297, 384, 75, 348, 298, 295, 303, 302, 283, 332, 410, 392, 234, 146, 315, 320, 317, 314, 318, 319, 353, 313, 235, 375 };
            var SubmittedCollegeIds = db.jntuh_college_edit_status.Where(a => CollegeIds.Contains(a.collegeId) && a.IsCollegeEditable == false).Select(a => a.collegeId).ToList();
            //var integreatedIds = db.jntuh_college.Where(z => SubmittedCollegeIds.Contains(z.id)).Select(q => q.id).ToList();


            var pharmacyids = db.jntuh_college.Where(z => SubmittedCollegeIds.Contains(z.id) && z.isActive == true).Select(q => q.id).ToList();
            List<PharmacyReportsClass> PharmacyGroupsReport = new List<PharmacyReportsClass>();

            var jntuh_address = db.jntuh_address.Where(a => a.addressTye == "COLLEGE").Select(z => z).ToList();
            var jntuh_district = db.jntuh_district.Select(z => z).ToList();
            foreach (var item in pharmacyids)
            {
                var Data = EachCollegeCounsellingData(item);
                if (Data != null && Data.Count != 0)
                {
                    int i = 0;
                    var GroupsData = Data.Where(a => a.SpecializationId == 12 && a.IsActive == false).Select(a => a).ToList();
                    foreach (var Group in GroupsData)
                    {
                        if (i == 0)
                        {
                            PharmacyReportsClass Pharmacy = new PharmacyReportsClass();
                            Pharmacy.Collegeid = Group.Collegeid;
                            Pharmacy.CollegeCode = Group.CollegeCode;
                            Pharmacy.CollegeName = Group.CollegeName;
                            Pharmacy.Degree = Group.Degree;
                            Pharmacy.DepartmentId = Group.DepartmentId;
                            Pharmacy.Department = Group.Department;
                            Pharmacy.SpecializationId = Group.SpecializationId;
                            Pharmacy.Specialization = Group.Specialization;
                            Pharmacy.ShiftId = Group.ShiftId;
                            Pharmacy.ProposedIntake = Group.ProposedIntake;
                            Pharmacy.Group1RequiredFaculty = GroupsData.Where(s => s.GroupId == "1").Select(q => q.SpecializationwiseRequiredFaculty).LastOrDefault();
                            Pharmacy.Group1AvilableFaculty = GroupsData.Where(s => s.GroupId == "1").Select(q => q.SpecializationwiseAvilableFaculty).LastOrDefault();
                            Pharmacy.Group2RequiredFaculty = GroupsData.Where(s => s.GroupId == "2").Select(q => q.SpecializationwiseRequiredFaculty).LastOrDefault();
                            Pharmacy.Group2AvilableFaculty = GroupsData.Where(s => s.GroupId == "2").Select(q => q.SpecializationwiseAvilableFaculty).LastOrDefault();
                            Pharmacy.Group3RequiredFaculty = GroupsData.Where(s => s.GroupId == "3").Select(q => q.SpecializationwiseRequiredFaculty).LastOrDefault();
                            Pharmacy.Group3AvilableFaculty = GroupsData.Where(s => s.GroupId == "3").Select(q => q.SpecializationwiseAvilableFaculty).LastOrDefault();
                            Pharmacy.Group4RequiredFaculty = GroupsData.Where(s => s.GroupId == "4").Select(q => q.SpecializationwiseRequiredFaculty).LastOrDefault();
                            Pharmacy.Group4AvilableFaculty = GroupsData.Where(s => s.GroupId == "4").Select(q => q.SpecializationwiseAvilableFaculty).LastOrDefault();
                            Pharmacy.NoOfFacultyRequired = Group.NoOfFacultyRequired;
                            Pharmacy.NoOfAvilableFaculty = Group.NoOfAvilableFaculty;
                            Pharmacy.TotalPHdFaculty = GroupsData.Select(q => q.PHdFaculty).Sum();
                            PharmacyGroupsReport.Add(Pharmacy);
                            i++;
                        }
                    }
                }
            }
            Response.ClearContent();
            Response.Buffer = true;
            Response.AddHeader("content-disposition", "attachment; filename=All Colleges Group Wise Faculty Count.XLS");
            Response.ContentType = "application/vnd.ms-excel";
            return PartialView("~/Views/Pharmacyreport/_GroupWiseFacultyCount.cshtml", PharmacyGroupsReport.OrderBy(a => a.CollegeName).ToList());
        }

        public ActionResult BPharmacyCounsellingReport(int? CollegeID)
        {
            var userId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            var collegeId = db.jntuh_college_users.Where(u => u.userID == userId).Select(u => u.collegeID).FirstOrDefault();
            if (collegeId == 375)
            {
                collegeId = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            var CollegeIds = new int[] { 448, 180, 159, 376, 428, 445, 24, 117, 202, 213, 395, 364, 27, 30, 6, 34, 55, 58, 54, 52, 9, 60, 65, 78, 150, 204, 219, 90, 97, 104, 110, 139, 105, 107, 114, 379, 118, 39, 120, 389, 127, 237, 135, 136, 47, 252, 169, 253, 262, 442, 301, 436, 370, 263, 267, 206, 140, 44, 290, 454, 42, 95, 297, 384, 75, 348, 298, 295, 303, 302, 283, 332, 410, 392, 234, 146, 315, 320, 317, 314, 318, 319, 353, 313, 235, 375 };
            //var CollegeIds = new int[] { 6, 24, 27, 30, 34, 47, 52, 54, 55, 58, 60, 65, 78, 90, 95, 97, 104, 105, 107, 110, 114, 117, 118, 120, 135, 136, 139, 140, 146, 150, 159, 169, 202, 204, 206, 213, 219, 234, 237, 253, 262, 263, 267, 283, 290, 295, 297, 298, 301, 302, 303, 313, 314, 315, 317, 318, 319, 320, 332, 353, 370, 379, 384, 389, 392, 395, 410, 428, 436, 442, 445 };
            var SubmittedCollegeIds = db.jntuh_college_edit_status.Where(a => CollegeIds.Contains(a.collegeId) && a.IsCollegeEditable == false).Select(a => a.collegeId).ToList();
            //var integreatedIds = db.jntuh_college.Where(z => SubmittedCollegeIds.Contains(z.id)).Select(q => q.id).ToList();


            var pharmacyids = db.jntuh_college.Where(z => SubmittedCollegeIds.Contains(z.id) && z.isActive == true).Select(q => q.id).ToList();
            List<PharmacyReportsClass> PharmacyCounselling = new List<PharmacyReportsClass>();

            var jntuh_address = db.jntuh_address.Where(a => a.addressTye == "COLLEGE").Select(z => z).ToList();
            var jntuh_district = db.jntuh_district.Select(z => z).ToList();
            foreach (var item in pharmacyids)
            {
                var Data = EachCollegeCounsellingData(item);
                if (Data != null && Data.Count != 0)
                {
                    var specIds = Data.Where(a => a.SpecializationId != 0 && a.DepartmentId != 36).Select(a => a.SpecializationId).Distinct().ToList();
                    foreach (var SpecId in specIds)
                    {
                        PharmacyReportsClass pharmacy = new PharmacyReportsClass();
                        PharmacyReportsClass pharmacyData = new PharmacyReportsClass();
                        if (SpecId == 12)
                            pharmacyData = Data.Where(s => s.SpecializationId == SpecId && s.IsActive == true).Select(s => s).FirstOrDefault();
                        else if (SpecId == 18)
                            pharmacyData = Data.Where(s => s.SpecializationId == SpecId).Select(s => s).FirstOrDefault();
                        else if (SpecId == 19)
                            pharmacyData = Data.Where(s => s.SpecializationId == SpecId).Select(s => s).FirstOrDefault();


                        pharmacy.Collegeid = pharmacyData.Collegeid;
                        pharmacy.CollegeCode = pharmacyData.CollegeCode;
                        pharmacy.CollegeName = pharmacyData.CollegeName;
                        pharmacy.districtId = jntuh_address.Where(a => a.collegeId == pharmacy.Collegeid).Select(a => a.districtId).FirstOrDefault();
                        pharmacy.district = jntuh_district.Where(a => a.id == pharmacy.districtId).Select(a => a.districtName).FirstOrDefault();
                        pharmacy.address = jntuh_address.Where(a => a.collegeId == pharmacy.Collegeid).Select(z => z.townOrCity + "," + z.mandal + "," + pharmacy.district + "," + z.pincode).FirstOrDefault();
                        pharmacy.Degree = pharmacyData.Degree;
                        pharmacy.Department = pharmacyData.Department;
                        pharmacy.DepartmentId = pharmacyData.DepartmentId;
                        pharmacy.Specialization = pharmacyData.Specialization;
                        pharmacy.SpecializationId = pharmacyData.SpecializationId;
                        pharmacy.ShiftId = pharmacyData.ShiftId;
                        pharmacy.ProposedIntake = pharmacyData.ProposedIntake;
                        if (SpecId == 12)
                        {
                            if (pharmacy.ProposedIntake > 100)
                                pharmacy.JntuhApprovedIntake = 100;
                            else
                                pharmacy.JntuhApprovedIntake = pharmacy.ProposedIntake;
                        }
                        else if (SpecId == 18)
                        {
                            if (pharmacy.ProposedIntake > 30)
                                pharmacy.JntuhApprovedIntake = 30;
                            else
                                pharmacy.JntuhApprovedIntake = pharmacy.ProposedIntake;
                        }
                        else if (SpecId == 19)
                        {
                            if (pharmacy.ProposedIntake > 10)
                                pharmacy.JntuhApprovedIntake = 10;
                            else
                                pharmacy.JntuhApprovedIntake = pharmacy.ProposedIntake;
                        }
                        pharmacy.NoOfFacultyRequired = pharmacyData.NoOfFacultyRequired;
                        pharmacy.NoOfAvilableFaculty = pharmacyData.NoOfAvilableFaculty;
                        pharmacy.Deficiency = pharmacyData.Deficiency;
                        pharmacy.PHdFaculty = pharmacyData.PHdFaculty;
                        PharmacyCounselling.Add(pharmacy);
                    }
                }
            }

            Response.ClearContent();
            Response.Buffer = true;
            Response.AddHeader("content-disposition", "attachment; filename=BPharmacy Conselling Report.XLS");
            Response.ContentType = "application/vnd.ms-excel";
            return PartialView("~/Views/Pharmacyreport/_PharmacyCounsellingReport.cshtml", PharmacyCounselling.Where(a => a.Deficiency != "Deficiency").OrderBy(a => a.CollegeName).ThenBy(q => q.SpecializationId).ToList());
        }

        public ActionResult MPharmacyCounsellingReport(string Type)
        {
            var userId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            var collegeId = db.jntuh_college_users.Where(u => u.userID == userId).Select(u => u.collegeID).FirstOrDefault();
            if (collegeId == 375)
            {
                collegeId = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            var CollegeIds = new int[] { 448, 180, 159, 376, 428, 445, 24, 117, 202, 213, 395, 364, 27, 30, 6, 34, 55, 58, 54, 52, 9, 60, 65, 78, 150, 204, 219, 90, 97, 104, 110, 139, 105, 107, 114, 379, 118, 39, 120, 389, 127, 237, 135, 136, 47, 252, 169, 253, 262, 442, 301, 436, 370, 263, 267, 206, 140, 44, 290, 454, 42, 95, 297, 384, 75, 348, 298, 295, 303, 302, 283, 332, 410, 392, 234, 146, 315, 320, 317, 314, 318, 319, 353, 313, 235, 375 };
            //var CollegeIds = new int[] { 39 };
            var SubmittedCollegeIds = db.jntuh_college_edit_status.Where(a => CollegeIds.Contains(a.collegeId) && a.IsCollegeEditable == false).Select(a => a.collegeId).ToList();
            //var integreatedIds = db.jntuh_college.Where(z => SubmittedCollegeIds.Contains(z.id)).Select(q => q.id).ToList();

            var pharmacyids = db.jntuh_college.Where(z => SubmittedCollegeIds.Contains(z.id) && z.isActive == true).Select(q => q.id).ToList();
            List<PharmacyReportsClass> PharmacyCounselling = new List<PharmacyReportsClass>();

            var jntuh_address = db.jntuh_address.Where(a => a.addressTye == "COLLEGE").Select(z => z).ToList();
            var jntuh_district = db.jntuh_district.Select(z => z).ToList();
            foreach (var item in pharmacyids)
            {
                var Data = EachCollegeCounsellingData(item);
                if (Data != null && Data.Count != 0)
                {
                    var specIds = Data.Where(a => a.SpecializationId != 0 && a.DepartmentId == 36).Select(a => a.SpecializationId).Distinct().ToList();
                    foreach (var SpecId in specIds)
                    {
                        PharmacyReportsClass pharmacy = new PharmacyReportsClass();
                        PharmacyReportsClass pharmacyData = new PharmacyReportsClass();

                        pharmacyData = Data.Where(s => s.SpecializationId == SpecId).Select(s => s).FirstOrDefault();

                        pharmacy.Collegeid = pharmacyData.Collegeid;
                        pharmacy.CollegeCode = pharmacyData.CollegeCode;
                        pharmacy.CollegeName = pharmacyData.CollegeName;
                        pharmacy.districtId = jntuh_address.Where(a => a.collegeId == pharmacy.Collegeid).Select(a => a.districtId).FirstOrDefault();
                        pharmacy.district = jntuh_district.Where(a => a.id == pharmacy.districtId).Select(a => a.districtName).FirstOrDefault();
                        pharmacy.address = jntuh_address.Where(a => a.collegeId == pharmacy.Collegeid).Select(z => z.townOrCity + "," + z.mandal + "," + pharmacy.district + "," + z.pincode).FirstOrDefault();
                        pharmacy.Degree = pharmacyData.Degree;
                        pharmacy.Department = pharmacyData.Department;
                        pharmacy.DepartmentId = pharmacyData.DepartmentId;
                        pharmacy.Specialization = pharmacyData.Specialization;
                        pharmacy.SpecializationId = pharmacyData.SpecializationId;
                        pharmacy.ShiftId = pharmacyData.ShiftId;
                        pharmacy.ProposedIntake = pharmacyData.ProposedIntake;

                        if (pharmacy.ProposedIntake > 15)
                            pharmacy.JntuhApprovedIntake = 15;
                        else
                            pharmacy.JntuhApprovedIntake = pharmacy.ProposedIntake;

                        pharmacy.NoOfFacultyRequired = pharmacyData.NoOfFacultyRequired;
                        pharmacy.NoOfAvilableFaculty = pharmacyData.NoOfAvilableFaculty;
                        pharmacy.PHdFaculty = pharmacyData.PHdFaculty;
                        pharmacy.Deficiency = pharmacyData.Deficiency;
                        PharmacyCounselling.Add(pharmacy);
                    }
                }
            }

            Response.ClearContent();
            Response.Buffer = true;
            if (Type == "AllData")
            {
                Response.AddHeader("content-disposition", "attachment; filename=MPharmacy Added Faculty.XLS");
                Response.ContentType = "application/vnd.ms-excel";
                return PartialView("~/Views/Pharmacyreport/_PharmacyCounsellingReport.cshtml", PharmacyCounselling.OrderBy(a => a.CollegeName).ThenBy(q => q.SpecializationId).ToList());
            }
            else if (Type == "NoDeficiency")
            {
                Response.AddHeader("content-disposition", "attachment; filename=MPharmacy Conselling Report.XLS");
                Response.ContentType = "application/vnd.ms-excel";
                return PartialView("~/Views/Pharmacyreport/_PharmacyCounsellingReport.cshtml", PharmacyCounselling.Where(a => a.Deficiency != "Deficiency").OrderBy(a => a.CollegeName).ThenBy(q => q.SpecializationId).ToList());
            }
            else
            {
                List<PharmacyReportsClass> AllCollegesCoursesWise = new List<PharmacyReportsClass>();
                var Collegeids = PharmacyCounselling.Select(q => q.Collegeid).Distinct().ToList();
                foreach (var item in Collegeids)
                {
                    PharmacyReportsClass CoursesWise = new PharmacyReportsClass();
                    CoursesWise.Collegeid = item;
                    CoursesWise.CollegeCode = PharmacyCounselling.Where(q => q.Collegeid == item).Select(w => w.CollegeCode).FirstOrDefault();
                    CoursesWise.CollegeName = PharmacyCounselling.Where(q => q.Collegeid == item).Select(w => w.CollegeName).FirstOrDefault();
                    CoursesWise.address = PharmacyCounselling.Where(q => q.Collegeid == item).Select(w => w.address).FirstOrDefault();
                    CoursesWise.district = PharmacyCounselling.Where(q => q.Collegeid == item).Select(w => w.district).FirstOrDefault();
                    CoursesWise.CoursesCount = PharmacyCounselling.Where(q => q.Collegeid == item).Select(w => w.SpecializationId).Count();
                    CoursesWise.ProposedIntake = PharmacyCounselling.Where(q => q.Collegeid == item).Select(w => w.ProposedIntake).Sum();
                    CoursesWise.JntuhApprovedIntake = PharmacyCounselling.Where(q => q.Collegeid == item).Select(w => w.JntuhApprovedIntake).Sum();
                    CoursesWise.NoOfFacultyRequired = PharmacyCounselling.Where(q => q.Collegeid == item).Select(w => w.NoOfFacultyRequired).Sum();
                    CoursesWise.NoOfAvilableFaculty = PharmacyCounselling.Where(q => q.Collegeid == item).Select(w => w.NoOfAvilableFaculty).Sum();
                    CoursesWise.PHdFaculty = PharmacyCounselling.Where(q => q.Collegeid == item).Select(w => w.PHdFaculty).Sum();
                    AllCollegesCoursesWise.Add(CoursesWise);
                }
                Response.AddHeader("content-disposition", "attachment; filename=College Wise MPharmacy Courses Count Report.XLS");
                Response.ContentType = "application/vnd.ms-excel";
                return PartialView("~/Views/Pharmacyreport/_MpharmacyCousesCountwithFaculty.cshtml", AllCollegesCoursesWise.OrderBy(a => a.CollegeName).ThenBy(q => q.SpecializationId).ToList());
            }

        }

        public List<PharmacyReportsClass> EachCollegeCounsellingData(int? collegeID)
        {
            string faculty = string.Empty;
            int? AddingFacultyCount = 0;
            int? TotalcollegeFaculty = 0;
            int? AdditionalFaculty = 0;

            List<PharmacyReportsClass> PharmacyAppealFaculty = new List<PharmacyReportsClass>();
            string facultyAdmittedIntakeZero = string.Empty;

            var jntuh_academic_year = db.jntuh_academic_year.ToList();
            int presentYear = jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();

            int AY0 = jntuh_academic_year.Where(a => a.actualYear == (presentYear + 1)).Select(a => a.id).FirstOrDefault();
            int AY1 = jntuh_academic_year.Where(a => a.actualYear == presentYear).Select(a => a.id).FirstOrDefault();
            int AY2 = jntuh_academic_year.Where(a => a.actualYear == (presentYear - 1)).Select(a => a.id).FirstOrDefault();
            int AY3 = jntuh_academic_year.Where(a => a.actualYear == (presentYear - 2)).Select(a => a.id).FirstOrDefault();
            int AY4 = jntuh_academic_year.Where(a => a.actualYear == (presentYear - 3)).Select(a => a.id).FirstOrDefault();
            int AY5 = jntuh_academic_year.Where(a => a.actualYear == (presentYear - 4)).Select(a => a.id).FirstOrDefault();

            List<jntuh_college_intake_existing> intake = db.jntuh_college_intake_existing.Where(i => i.collegeId == collegeID && i.academicYearId == AY0 && i.proposedIntake != 0 && i.courseStatus != "Closure").ToList();
            List<CollegeIntakeExisting> collegeIntakeExisting = new List<CollegeIntakeExisting>();
            //List<CollegeIntakeReport> collegeIntakeExistingList = new List<CollegeIntakeReport>();

            foreach (var item in intake)
            {
                CollegeIntakeExisting newIntake = new CollegeIntakeExisting();
                newIntake.id = item.id;
                newIntake.collegeId = item.collegeId;
                newIntake.academicYearId = item.academicYearId;
                newIntake.isActive = item.isActive;
                newIntake.nbaFrom = item.nbaFrom;
                newIntake.nbaTo = item.nbaTo;
                newIntake.specializationId = item.specializationId;
                newIntake.Specialization = item.jntuh_specialization.specializationName;
                newIntake.DepartmentID = item.jntuh_specialization.jntuh_department.id;
                newIntake.Department = item.jntuh_specialization.jntuh_department.departmentName;
                newIntake.degreeID = item.jntuh_specialization.jntuh_department.jntuh_degree.id;
                newIntake.Degree = item.jntuh_specialization.jntuh_department.jntuh_degree.degree;
                newIntake.degreeDisplayOrder = item.jntuh_specialization.jntuh_department.jntuh_degree.degreeDisplayOrder;
                newIntake.shiftId = item.shiftId;
                newIntake.Shift = item.jntuh_shift.shiftName;
                collegeIntakeExisting.Add(newIntake);
            }

            collegeIntakeExisting = collegeIntakeExisting.GroupBy(a => new { a.specializationId, a.shiftId }).Select(a => a.First()).ToList();

            var jntuh_college = db.jntuh_college.Where(a => a.isActive == true && a.id == collegeID).Select(q => q).FirstOrDefault();

            string cid = collegeID.ToString();
            var PharmacyDepartmens = new int[] { 26, 36, 27, 39, 61 };
            var jntuh_college_faculty_registered_all = db.jntuh_college_faculty_registered.Where(e => e.collegeId == collegeID).Select(e => e).ToList();
            var clgstrRegnos = jntuh_college_faculty_registered_all.Select(e => e.RegistrationNumber).ToList();
            var jntuh_registered_faculty_New = db.jntuh_registered_faculty.Where(e => clgstrRegnos.Contains(e.RegistrationNumber.Trim())).Select(e => e).ToList();
            var jntuh_registered_facultyBAS = jntuh_registered_faculty_New.Where(rf => (rf.BAS == "Yes")).Select(rf => new
            {
                FacultyId = rf.id,
                RegistrationNumber = rf.RegistrationNumber.Trim(),
                BASFlag = rf.BAS
            }).ToList();
            var BASRegNos = jntuh_registered_facultyBAS.Select(rf => rf.RegistrationNumber).ToArray();
            var FacultyData = db.jntuh_appeal_pharmacydata.Where(p => p.CollegeCode == cid && (p.Deficiency == null || !BASRegNos.Contains(p.Deficiency))).ToList();
            string[] AssignedFaculty = FacultyData.Select(p => p.Deficiency).ToArray();
            var registeredFaculty = db.jntuh_registered_faculty.Where(rf => AssignedFaculty.Contains(rf.RegistrationNumber.Trim())).ToList();
            AdditionalFaculty = FacultyData.Where(a => a.CollegeCode == cid && a.Deficiency != null).Select(z => z.Deficiency).Count();

            int Count = 1;
            foreach (var item in collegeIntakeExisting.OrderBy(s => s.specializationId).ThenBy(a => a.shiftId).ToList())
            {
                if (item.Degree == "B.Pharmacy")
                {
                    for (int i = 1; i <= 4; i++)
                    {
                        PharmacyReportsClass Pharmacy = new PharmacyReportsClass();
                        Pharmacy.Collegeid = item.collegeId;
                        Pharmacy.CollegeCode = jntuh_college.collegeCode;
                        Pharmacy.CollegeName = jntuh_college.collegeName;
                        Pharmacy.Degree = item.Degree;
                        Pharmacy.DepartmentId = item.DepartmentID;
                        Pharmacy.Department = item.Department;
                        Pharmacy.SpecializationId = item.specializationId;
                        Pharmacy.Specialization = item.Specialization;
                        Pharmacy.ShiftId = item.shiftId;
                        switch (i)
                        {
                            case 1:
                                Pharmacy.PharmacySpecialization = "Group1 (Pharmaceutics , Industrial Pharmacy , Pharmaceutical Technology , 	Pharmaceutical Biotechnology , RA)";
                                Pharmacy.GroupId = "1";
                                break;
                            case 2:
                                Pharmacy.PharmacySpecialization = "Group2 (Pharmaceutical Chemistry,Pharmaceutical Analysis , PAQA , QA , QAPRA , NIPER Medicinal Chemistry)";
                                Pharmacy.GroupId = "2";
                                break;
                            case 3:
                                Pharmacy.PharmacySpecialization = "Group3 (Pharmacology, Pharm-D, Pharm-DPB, Pharmacy Practice , Hospital Pharmacy , Clinical Pharmacy,  Hospital and Clinical Pharmacy)";
                                Pharmacy.GroupId = "3";
                                break;
                            default:
                                Pharmacy.PharmacySpecialization = "Group4 (Pharmacognosy, Pharmaceutical Chemistry , Phytopharmacy & Phytomedicine , NIPER  Natural Products , Pharmaceutical Biotechnology";
                                Pharmacy.GroupId = "4";
                                break;
                        }

                        Pharmacy.ProposedIntake = GetPharmacyIntake(item.collegeId, AY0, item.specializationId, item.shiftId, 0);

                        Pharmacy.TotalIntake = FacultyData.Where(f => f.CollegeCode == cid && f.Specialization == item.specializationId.ToString()).Select(f => f.TotalIntake).FirstOrDefault();
                        Pharmacy.NoOfFacultyRequired = FacultyData.Where(f => f.CollegeCode == cid && f.Specialization == item.specializationId.ToString()).Select(f => f.NoOfFacultyRequired).FirstOrDefault();
                        Pharmacy.NoOfAvilableFaculty = FacultyData.Where(f => f.CollegeCode == cid && f.Specialization == item.specializationId.ToString() && f.Deficiency != null).Select(a => a.Deficiency).Distinct().Count();
                        Pharmacy.SpecializationwiseRequiredFaculty = FacultyData.Where(f => f.CollegeCode == cid && f.Specialization == item.specializationId.ToString() && f.PharmacySpecialization == Pharmacy.GroupId).Select(f => f.SpecializationWiseRequiredFaculty).LastOrDefault();
                        Pharmacy.SpecializationwiseAvilableFaculty = FacultyData.Where(f => f.CollegeCode == cid && f.Specialization == item.specializationId.ToString() && f.PharmacySpecialization == Pharmacy.GroupId && f.Deficiency != null).Select(a => a.Deficiency).Distinct().Count();
                        if (Pharmacy.SpecializationwiseAvilableFaculty >= Pharmacy.SpecializationwiseRequiredFaculty)
                        {
                            Pharmacy.Deficiency = "No Deficiency";
                        }
                        else
                        {
                            Pharmacy.Deficiency = "Deficiency";
                        }

                        var AvailiableFacultyRegNos = FacultyData.Where(f => f.CollegeCode == cid && f.Specialization == item.specializationId.ToString() && f.PharmacySpecialization == Pharmacy.GroupId && f.Deficiency != null).Select(q => q.Deficiency).Distinct().ToList();

                        var PHDFacultyList = db.jntuh_registered_faculty.Where(q => AvailiableFacultyRegNos.Contains(q.RegistrationNumber)).Select(a => a).ToList();

                        //var PHDFacultyCleared = PHDFacultyList.Where(rf => rf.type != "Adjunct" && ((rf.NoForm16 == false || rf.NoForm16 == null) && (rf.NoForm26AS == false || rf.NoForm26AS == null) && (rf.Absent == false || rf.Absent == null) && (rf.AbsentforVerification == false || rf.AbsentforVerification == null) && (rf.OriginalCertificatesNotShown == false || rf.OriginalCertificatesNotShown == null) && (rf.Xeroxcopyofcertificates == false || rf.Xeroxcopyofcertificates == null) && (rf.NotQualifiedAsperAICTE == false || rf.NotQualifiedAsperAICTE == null)
                        //                                 && (rf.Noclass == false || rf.Noclass == null) && (rf.FakePHD == false || rf.FakePHD == null) && (rf.Genuinenessnotsubmitted == false || rf.Genuinenessnotsubmitted == null) && (rf.NotconsideredPHD == false || rf.NotconsideredPHD == null) && (rf.NoPGspecialization == false || rf.NoPGspecialization == null) && (rf.NoSCM == false || rf.NoSCM == null) && (rf.PANNumber != null) && (rf.IncompleteCertificates == false || rf.IncompleteCertificates == null) && (rf.Blacklistfaculy == false) && rf.NoRelevantUG != "Yes" && rf.NoRelevantPG != "Yes" && rf.NORelevantPHD != "Yes" && (rf.InvalidPANNumber == false || rf.InvalidPANNumber == null) && (rf.OriginalsVerifiedPHD != true) && (rf.OriginalsVerifiedUG != true) && (rf.Invaliddegree == false || rf.Invaliddegree == null) && rf.InvalidAadhaar != "Yes") && rf.BAS != "Yes").Select(rf => rf).ToList();
                        //var PHDFacultyCleared = PHDFacultyList.Where(rf => rf.type != "Adjunct" && ((rf.NotQualifiedAsperAICTE == false || rf.NotQualifiedAsperAICTE == null) && (rf.Noclass == false || rf.Noclass == null) && (rf.Absent == false || rf.Absent == null) && (rf.OriginalCertificatesNotShown == false || rf.OriginalCertificatesNotShown == null) &&
                        //                                 (rf.NoSCM == false || rf.NoSCM == null) && rf.NoRelevantUG != "Yes" && rf.NoRelevantPG != "Yes" && rf.NORelevantPHD != "Yes" && (rf.InvalidPANNumber == false || rf.InvalidPANNumber == null) && (rf.Blacklistfaculy == false) && (rf.AbsentforVerification == false || rf.AbsentforVerification == null))).Select(rf => rf).ToList();
                        var PHDFacultyCleared = PHDFacultyList.Where(rf => rf.type != "Adjunct" && (rf.BAS != "Yes") && (rf.Noclass == false || rf.Noclass == null) && (rf.Absent == false || rf.Absent == null) &&
                                                            (rf.NoSCM == false || rf.NoSCM == null) && (rf.Blacklistfaculy == false) && (rf.AbsentforVerification == false || rf.AbsentforVerification == null)).Select(rf => rf).ToList();


                        var PHDFaculty = PHDFacultyCleared.Select(q => q.id).ToList();
                        int PhdRegNOCount = db.jntuh_registered_faculty_education.Count(q => PHDFaculty.Contains(q.facultyId) && q.educationId == 6);

                        Pharmacy.PHdFaculty = PhdRegNOCount;

                        Pharmacy.IsActive = false;
                        PharmacyAppealFaculty.Add(Pharmacy);
                    }

                    PharmacyReportsClass BPharmacyObj = new PharmacyReportsClass();
                    BPharmacyObj.Collegeid = PharmacyAppealFaculty.Select(z => z.Collegeid).FirstOrDefault();
                    BPharmacyObj.CollegeCode = PharmacyAppealFaculty.Select(z => z.CollegeCode).FirstOrDefault();
                    BPharmacyObj.CollegeName = PharmacyAppealFaculty.Select(z => z.CollegeName).FirstOrDefault();
                    BPharmacyObj.Degree = PharmacyAppealFaculty.Select(z => z.Degree).FirstOrDefault();
                    BPharmacyObj.DepartmentId = PharmacyAppealFaculty.Select(z => z.DepartmentId).FirstOrDefault();
                    BPharmacyObj.Department = PharmacyAppealFaculty.Select(z => z.Department).FirstOrDefault();
                    BPharmacyObj.SpecializationId = PharmacyAppealFaculty.Select(z => z.SpecializationId).FirstOrDefault();
                    BPharmacyObj.Specialization = PharmacyAppealFaculty.Select(z => z.Specialization).FirstOrDefault();
                    BPharmacyObj.ShiftId = PharmacyAppealFaculty.Select(z => z.ShiftId).FirstOrDefault();
                    BPharmacyObj.TotalIntake = PharmacyAppealFaculty.Where(a => a.DepartmentId == 26).Select(z => z.TotalIntake).FirstOrDefault();
                    BPharmacyObj.ProposedIntake = PharmacyAppealFaculty.Where(a => a.DepartmentId == 26).Select(z => z.ProposedIntake).FirstOrDefault();
                    BPharmacyObj.NoOfFacultyRequired = PharmacyAppealFaculty.Where(a => a.DepartmentId == 26).Select(z => z.NoOfFacultyRequired).FirstOrDefault();
                    BPharmacyObj.NoOfAvilableFaculty = PharmacyAppealFaculty.Where(a => a.DepartmentId == 26).Select(z => z.NoOfAvilableFaculty).FirstOrDefault();
                    BPharmacyObj.Deficiency = PharmacyAppealFaculty.Count(a => a.DepartmentId == 26 && a.Deficiency == "Deficiency") > 0 ? "Deficiency" : "No Deficiency";
                    BPharmacyObj.PHdFaculty = PharmacyAppealFaculty.Where(a => a.DepartmentId == 26).Select(q => q.PHdFaculty).Sum();
                    BPharmacyObj.IsActive = true;
                    PharmacyAppealFaculty.Add(BPharmacyObj);
                }
                else if (item.Degree == "Pharm.D" || item.Degree == "Pharm.D PB")
                {
                    PharmacyReportsClass Pharmacy = new PharmacyReportsClass();
                    Pharmacy.Collegeid = item.collegeId;
                    Pharmacy.CollegeCode = jntuh_college.collegeCode;
                    Pharmacy.CollegeName = jntuh_college.collegeName;
                    Pharmacy.Degree = item.Degree;
                    Pharmacy.DepartmentId = item.DepartmentID;
                    Pharmacy.Department = item.Department;
                    Pharmacy.SpecializationId = item.specializationId;
                    Pharmacy.Specialization = item.Specialization;
                    Pharmacy.ShiftId = item.shiftId;
                    //Pharmacy.TotalIntake = (PharmDFirstYerintake + PharmDSecondYerintake + PharmDThirdYerintake + PharmDFourthYerintake + PharmDFifthhYerintake);
                    Pharmacy.ProposedIntake = GetPharmacyIntake(item.collegeId, AY0, item.specializationId, item.shiftId, 0);
                    if (PharmacyAppealFaculty.Count(a => a.DepartmentId == 26 && a.Deficiency == "Deficiency") > 0)
                        Pharmacy.Deficiency = "Deficiency";
                    else
                        Pharmacy.Deficiency = "No Deficiency";

                    PharmacyAppealFaculty.Add(Pharmacy);
                }
                else if (item.Degree == "M.Pharmacy")
                {
                    PharmacyReportsClass Pharmacy = new PharmacyReportsClass();
                    Pharmacy.Collegeid = item.collegeId;
                    Pharmacy.CollegeCode = jntuh_college.collegeCode;
                    Pharmacy.CollegeName = jntuh_college.collegeName;
                    Pharmacy.Degree = item.Degree;
                    Pharmacy.DepartmentId = item.DepartmentID;
                    Pharmacy.Department = item.Department;
                    Pharmacy.SpecializationId = item.specializationId;
                    Pharmacy.Specialization = item.Specialization;
                    Pharmacy.ShiftId = item.shiftId;
                    Pharmacy.ProposedIntake = FacultyData.Where(f => f.CollegeCode == cid && f.Specialization == item.specializationId.ToString()).Select(f => f.ProposedIntake).FirstOrDefault();
                    Pharmacy.TotalIntake = FacultyData.Where(f => f.CollegeCode == cid && f.Specialization == item.specializationId.ToString()).Select(f => f.TotalIntake).FirstOrDefault();
                    Pharmacy.NoOfFacultyRequired = FacultyData.Where(f => f.CollegeCode == cid && f.Specialization == item.specializationId.ToString()).Select(f => f.NoOfFacultyRequired).FirstOrDefault();
                    Pharmacy.NoOfAvilableFaculty = FacultyData.Count(f => f.CollegeCode == cid && f.Specialization == item.specializationId.ToString() && f.Deficiency != null);
                    Pharmacy.SpecializationwiseRequiredFaculty = FacultyData.Where(f => f.CollegeCode == cid && f.Specialization == item.specializationId.ToString()).Select(f => f.SpecializationWiseRequiredFaculty).FirstOrDefault();
                    Pharmacy.SpecializationwiseAvilableFaculty = FacultyData.Where(f => f.CollegeCode == cid && f.Specialization == item.specializationId.ToString() && f.Deficiency != null).Select(a => a.Deficiency).Distinct().Count();

                    var AvailiableFacultyRegNos = FacultyData.Where(f => f.CollegeCode == cid && f.Specialization == item.specializationId.ToString() && f.Deficiency != null).Select(q => q.Deficiency).Distinct().ToList();

                    var PHDFacultyList = db.jntuh_registered_faculty.Where(q => AvailiableFacultyRegNos.Contains(q.RegistrationNumber)).Select(a => a).ToList();

                    //var PHDFacultyCleared = PHDFacultyList.Where(rf => rf.type != "Adjunct" && ((rf.NoForm16 == false || rf.NoForm16 == null) && (rf.NoForm26AS == false || rf.NoForm26AS == null) && (rf.Absent == false || rf.Absent == null) && (rf.AbsentforVerification == false || rf.AbsentforVerification == null) && (rf.OriginalCertificatesNotShown == false || rf.OriginalCertificatesNotShown == null) && (rf.Xeroxcopyofcertificates == false || rf.Xeroxcopyofcertificates == null) && (rf.NotQualifiedAsperAICTE == false || rf.NotQualifiedAsperAICTE == null)
                    //                                 && (rf.Noclass == false || rf.Noclass == null) && (rf.FakePHD == false || rf.FakePHD == null) && (rf.Genuinenessnotsubmitted == false || rf.Genuinenessnotsubmitted == null) && (rf.NotconsideredPHD == false || rf.NotconsideredPHD == null) && (rf.NoPGspecialization == false || rf.NoPGspecialization == null) && (rf.NoSCM == false || rf.NoSCM == null) && (rf.PANNumber != null) && (rf.IncompleteCertificates == false || rf.IncompleteCertificates == null) && (rf.Blacklistfaculy == false) && rf.NoRelevantUG != "Yes" && rf.NoRelevantPG != "Yes" && rf.NORelevantPHD != "Yes" && (rf.InvalidPANNumber == false || rf.InvalidPANNumber == null) && (rf.OriginalsVerifiedPHD != true) && (rf.OriginalsVerifiedUG != true) && (rf.Invaliddegree == false || rf.Invaliddegree == null) && rf.InvalidAadhaar != "Yes") && rf.BAS != "Yes").Select(rf => rf).ToList();
                    //var PHDFacultyCleared = PHDFacultyList.Where(rf => rf.type != "Adjunct" && ((rf.NotQualifiedAsperAICTE == false || rf.NotQualifiedAsperAICTE == null) && (rf.Noclass == false || rf.Noclass == null) && (rf.Absent == false || rf.Absent == null) && (rf.OriginalCertificatesNotShown == false || rf.OriginalCertificatesNotShown == null) &&
                    //                                     (rf.NoSCM == false || rf.NoSCM == null) && rf.NoRelevantUG != "Yes" && rf.NoRelevantPG != "Yes" && rf.NORelevantPHD != "Yes" && (rf.InvalidPANNumber == false || rf.InvalidPANNumber == null) && (rf.Blacklistfaculy == false) && (rf.AbsentforVerification == false || rf.AbsentforVerification == null))).Select(rf => rf).ToList();
                    var PHDFacultyCleared = PHDFacultyList.Where(rf => rf.type != "Adjunct" && (rf.BAS != "Yes") && (rf.Noclass == false || rf.Noclass == null) && (rf.Absent == false || rf.Absent == null) &&
                                                            (rf.NoSCM == false || rf.NoSCM == null) && (rf.Blacklistfaculy == false) && (rf.AbsentforVerification == false || rf.AbsentforVerification == null)).Select(rf => rf).ToList();

                    var PHDFaculty = PHDFacultyCleared.Select(q => q.id).ToList();
                    int PhdRegNOCount = db.jntuh_registered_faculty_education.Count(q => PHDFaculty.Contains(q.facultyId) && q.educationId == 6);

                    Pharmacy.PHdFaculty = PhdRegNOCount;

                    if (Pharmacy.SpecializationwiseAvilableFaculty >= Pharmacy.SpecializationwiseRequiredFaculty)
                    {
                        if (PharmacyAppealFaculty.Count(a => a.DepartmentId == 26 && a.Deficiency == "Deficiency") > 0)
                            Pharmacy.Deficiency = "Deficiency";
                        else
                            Pharmacy.Deficiency = "No Deficiency";
                    }
                    else
                    {
                        Pharmacy.Deficiency = "Deficiency";
                    }
                    if (Pharmacy.ShiftId == 1)
                        PharmacyAppealFaculty.Add(Pharmacy);
                }
            }

            return PharmacyAppealFaculty;
        }

        private int GetPharmacyIntake(int collegeId, int academicYearId, int specializationId, int shiftId, int flag)
        {
            int intake = 0;
            if (flag == 0) //Proposed
            {
                var inta = db.jntuh_college_intake_existing.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == shiftId).FirstOrDefault();
                if (inta != null && inta.proposedIntake != null)
                {
                    intake = (int)inta.proposedIntake;
                }
            }
            else if (flag == 1) //Approved
            {
                intake = db.jntuh_college_intake_existing.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == 1).Select(i => i.approvedIntake).FirstOrDefault();
            }
            else if (flag == 2)//Admitted
            {
                intake = db.jntuh_college_intake_existing.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == 1).Select(i => i.admittedIntake).FirstOrDefault();
            }
            return intake;
        }

        #endregion

        public string AdministrativeLandDetails(int collegeId)
        {
            var contents = string.Empty;
            using (var db = new UAAAS.Models.uaaasDBContext())
            {
                string strAdministrativeLandDetails = string.Empty;
                decimal totalArea = 0;
                strAdministrativeLandDetails += "<table width='100%' border='0' cellpadding='5' cellspacing='0'>";
                strAdministrativeLandDetails += "<tr>";
                strAdministrativeLandDetails += "<td align='left'><b><u>Administrative Area</u></b></td>";
                strAdministrativeLandDetails += "</tr>";
                strAdministrativeLandDetails += "</table>";
                strAdministrativeLandDetails += "<table width='100%' border='1' cellpadding='5' cellspacing='0'><tbody>";
                strAdministrativeLandDetails += "<tr>";
                strAdministrativeLandDetails += "<td width='35%'><p><b>Type</b></p></td>";
                //strAdministrativeLandDetails += "<td width='18%'><p align='left'><b>Program</b></p></td>";
                strAdministrativeLandDetails += "<td width='9%'><p align='center'><b>Required Rooms</b></p></td>";
                strAdministrativeLandDetails += "<td width='9%'><p align='center'><b>Uploaded Rooms</b></p></td>";
                strAdministrativeLandDetails += "<td width='5%'><p align='center'><b>CF</b></p></td>";
                strAdministrativeLandDetails += "<td width='9%'><p align='center'><b>Deficiency</b></p></td>";
                strAdministrativeLandDetails += "<td width='9%'><p align='center'><b>Required Area (Sq.m)</b></p></td>";
                strAdministrativeLandDetails += "<td width='9%'><p align='center'><b>Uploaded Area (Sq.m)</b></p></td>";
                strAdministrativeLandDetails += "<td width='5%'><p align='center'><b>CF</b></p></td>";
                strAdministrativeLandDetails += "<td width='9%'><p align='center'><b>Deficiency</b></p></td>";
                strAdministrativeLandDetails += "</tr>";
                IQueryable<jntuh_college_area> jntuh_college_area = db.jntuh_college_area.Where(s => s.collegeId == collegeId).Select(e => e);
                IQueryable<jntuh_program_type> jntuh_program_type = db.jntuh_program_type.Select(e => e);
                int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
                int prAy = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();

                //List<int> collegeDegrees = db.jntuh_college_degree
                //                         .Join(db.jntuh_degree, cd => cd.degreeId, d => d.id, (cd, d) => new { cd, d })
                //                         .Where(s => s.d.isActive == true && s.cd.collegeId == collegeId && s.cd.isActive == true)
                //                         .OrderBy(s => s.d.degreeDisplayOrder)
                //                         .Select(s => s.d.id).ToList();

                List<int> collegeDegrees = (from ie in db.jntuh_college_intake_existing
                                            join s in db.jntuh_specialization on ie.specializationId equals s.id
                                            join d in db.jntuh_department on s.departmentId equals d.id
                                            join de in db.jntuh_degree on d.degreeId equals de.id
                                            //where ie.academicYearId == (prAy - 1) && (ie.aicteApprovedIntake != 0 || ie.approvedIntake != 0) && ie.collegeId == userCollegeID
                                            where ie.academicYearId == (prAy) && (ie.proposedIntake != 0) && ie.collegeId == collegeId
                                            orderby de.degreeDisplayOrder
                                            select de.id
                ).Distinct().ToList();

                var collegeSpecs = (from ie in db.jntuh_college_intake_existing
                                    join s in db.jntuh_specialization on ie.specializationId equals s.id
                                    join d in db.jntuh_department on s.departmentId equals d.id
                                    join de in db.jntuh_degree on d.degreeId equals de.id
                                    where ie.isActive == true && ie.academicYearId == (prAy) && (ie.proposedIntake != 0) && ie.collegeId == collegeId && de.id == 4
                                    orderby de.degreeDisplayOrder
                                    select s.id
                ).Distinct().ToList();
                var rrCount = 0;
                if (collegeDegrees.Contains(4))
                {
                    rrCount = collegeSpecs.Count + 4;
                }
                if (collegeDegrees.Contains(6) && collegeDegrees.Contains(3))
                {
                    rrCount = rrCount + 2;
                }
                else if (collegeDegrees.Contains(6) || collegeDegrees.Contains(3))
                {
                    rrCount = rrCount + 1;
                }

                var collegeFacultyCount = db.jntuh_college_faculty_registered.AsNoTracking().Where(i => i.collegeId == collegeId).Count();

                List<AdminLand> land = db.jntuh_area_requirement.Where(r => r.isActive == true && r.areaType == "ADMINISTRATIVE" && !r.requirementType.ToLower().Contains("desirable")).OrderBy(r => r.areaTypeDisplayOrder)
                                        .Select(r => new AdminLand
                                        {
                                            id = r.id,
                                            requirementType = r.requirementType,
                                            programId = r.programId,
                                            requiredRooms = r.requiredRooms,
                                            requiredRoomsCalculation = r.requiredRoomsCalculation,
                                            requiredArea = r.requiredArea,
                                            requiredAreaCalculation = r.requiredAreaCalculation,
                                            areaTypeDescription = r.areaTypeDescription,
                                            areaTypeDisplayOrder = r.areaTypeDisplayOrder,
                                            jntuh_program_type = jntuh_program_type.Where(p => p.id == r.programId).FirstOrDefault(),
                                            availableRooms = jntuh_college_area.Where(a => a.areaRequirementId == r.id).Select(a => a.availableRooms).FirstOrDefault(),
                                            availableArea = jntuh_college_area.Where(a => a.areaRequirementId == r.id).Select(a => a.availableArea).FirstOrDefault(),
                                            cfRooms = jntuh_college_area.Where(a => a.areaRequirementId == r.id).Select(a => a.cfrooms).FirstOrDefault(),
                                            cfArea = jntuh_college_area.Where(a => a.areaRequirementId == r.id).Select(a => a.cfarea).FirstOrDefault(),
                                        }).Where(g => g.availableRooms != null && g.availableRooms != 0).ToList();
                if (land != null)
                {
                    foreach (var item in land)
                    {
                        string programType = jntuh_program_type.Where(p => p.id == item.programId).Select(p => p.programType).FirstOrDefault();
                        if (programType == null)
                        {
                            programType = string.Empty;
                        }

                        var requiredRooms = string.Empty;
                        var requiredArea = string.Empty;
                        //if (programType == "B.Pharmacy")
                        //{
                        if (item.id == 6) // FacultyRooms
                        {
                            requiredRooms = TotalAreaRequiredFaculty.ToString();
                            requiredArea = (TotalAreaRequiredFaculty * 10).ToString();
                        }
                        else if (item.id == 7) // Cabin for HOD
                        {
                            var bpharmrrCount = 1;
                            requiredRooms = bpharmrrCount.ToString();
                            requiredArea = (bpharmrrCount * 20).ToString();
                        }
                        else if (item.id == 1) // Cabin for HOD
                        {
                            var bpharmrrCount = 1;
                            requiredRooms = bpharmrrCount.ToString();
                            requiredArea = "75".ToString();
                        }
                        else if (item.id == 20) // Boy’s Common room
                        {
                            var bpharmrrCount = 1;
                            requiredRooms = bpharmrrCount.ToString();
                            requiredArea = (bpharmrrCount * 10).ToString();
                        }
                        else if (item.id == 22) // Girl’s Common room
                        {
                            var bpharmrrCount = 1;
                            requiredRooms = bpharmrrCount.ToString();
                            requiredArea = (bpharmrrCount * 20).ToString();
                        }
                        else if (item.id == 13) // Examination Control Office
                        {
                            var bpharmrrCount = 1;
                            requiredRooms = bpharmrrCount.ToString();
                            requiredArea = (bpharmrrCount * 80).ToString();
                        }
                        else if (item.id == 8) // Central Stores
                        {
                            var bpharmrrCount = 1;
                            requiredRooms = bpharmrrCount.ToString();
                            requiredArea = (bpharmrrCount * 120).ToString();
                        }
                        else if (item.id == 4) // Office All Inclusive
                        {
                            var bpharmrrCount = 1;
                            requiredRooms = bpharmrrCount.ToString();
                            requiredArea = (bpharmrrCount * 75).ToString();
                        }
                        else if (item.id == 14) // Placement Office
                        {
                            var bpharmrrCount = 1;
                            requiredRooms = bpharmrrCount.ToString();
                            requiredArea = (bpharmrrCount * 100).ToString();
                        }
                        else
                        {
                            requiredRooms = Convert.ToInt16(item.requiredRooms).ToString();
                            requiredArea = Convert.ToDecimal(item.requiredArea).ToString();
                        }
                        //}
                        var defTxtRooms = string.Empty;
                        var defTxtArea = string.Empty;
                        strAdministrativeLandDetails += "<tr>";
                        if (item.requirementType == "Examination Control Office")
                        {
                            strAdministrativeLandDetails += "<td width='35%'><p>Confidential Room</p></td>";
                        }
                        else if (item.requirementType == "Central Stores")
                        {
                            strAdministrativeLandDetails += "<td width='35%'><p>Store Room 1&2</p></td>";
                        }
                        else if (item.requirementType == "Office All Inclusive")
                        {
                            strAdministrativeLandDetails += "<td width='35%'><p>Office-1 Establishment</p></td>";
                        }
                        else if (item.requirementType == "Placement Office")
                        {
                            strAdministrativeLandDetails += "<td width='35%'><p>Office-2 Academics</p></td>";
                        }
                        else
                        {
                            strAdministrativeLandDetails += "<td width='35%'><p>" + item.requirementType + "</p></td>";
                        }

                        //strAdministrativeLandDetails += "<td width='18%'>" + programType + "</td>";
                        strAdministrativeLandDetails += "<td width='9%' align='center'>" + Convert.ToInt32(requiredRooms) + "</td>";
                        if (item.availableRooms != null)
                        {
                            strAdministrativeLandDetails += "<td width='9%' align='center'>" + (int)item.availableRooms + "</td>";
                        }
                        else
                        {
                            strAdministrativeLandDetails += "<td width='9%' align='center'>" + item.availableRooms + "</td>";
                        }
                        strAdministrativeLandDetails += "<td width='5%' valign='top' align='center'>" + Convert.ToInt32(item.cfRooms) + "</td>";
                        if (item.cfRooms != null && item.cfRooms > 0)
                        {
                            var defCond = Convert.ToDecimal(requiredRooms) - Convert.ToDecimal(item.cfRooms);
                            if (defCond > 0)
                            {
                                defTxtRooms = "Yes";
                            }
                            else
                            {
                                defTxtRooms = "-";
                            }
                        }
                        else
                        {
                            var defCond = Convert.ToDecimal(requiredRooms) - Convert.ToDecimal(item.availableRooms);
                            if (defCond > 0)
                            {
                                defTxtRooms = "Yes";
                            }
                            else
                            {
                                defTxtRooms = "-";
                            }
                        }
                        strAdministrativeLandDetails += "<td width='9%' align='center' style='font-weight: bold'>" + defTxtRooms + "</td>";
                        strAdministrativeLandDetails += "<td width='9%' align='center'>" + requiredArea + "</td>";
                        strAdministrativeLandDetails += "<td width='9%' align='center'>" + item.availableArea + "</td>";
                        strAdministrativeLandDetails += "<td width='5%' valign='top' align='center'>" + item.cfArea + "</td>";
                        if (item.cfArea != null && item.cfArea > 0)
                        {
                            var defCond = Convert.ToDecimal(requiredArea) - Convert.ToDecimal(item.cfArea);
                            if (defCond > 0)
                            {
                                defTxtArea = "Yes";
                            }
                            else
                            {
                                defTxtArea = "-";
                            }
                        }
                        else
                        {
                            var defCond = Convert.ToDecimal(requiredArea) - Convert.ToDecimal(item.availableArea);
                            if (defCond > 0)
                            {
                                defTxtArea = "Yes";
                            }
                            else
                            {
                                defTxtArea = "-";
                            }
                        }
                        strAdministrativeLandDetails += "<td width='9%' align='center' style='font-weight: bold'>" + defTxtArea + "</td>";
                        strAdministrativeLandDetails += "</tr>";
                        //if (item.availableArea != null)
                        //{
                        //    totalArea += (decimal)item.availableArea;
                        //}
                    }
                }
                //strAdministrativeLandDetails += "<tr>";
                //strAdministrativeLandDetails += "<td colspan='3' align='right'><b>Total</b></td>";
                //strAdministrativeLandDetails += "<td width='9%' align='right'>" + totalArea + "</td>";
                //strAdministrativeLandDetails += "</tr>";
                strAdministrativeLandDetails += "</tbody></table>";
                contents = strAdministrativeLandDetails;
                // List<IElement> parsedHtmlElements = HTMLWorker.ParseToList(new StringReader(contents), null);
                return contents;
            }
        }

        public string InstructionalAreaDetails(int collegeId)
        {
            var contents = string.Empty;
            double TotalCollegeIntake = 0;
            using (var db = new UAAAS.Models.uaaasDBContext())
            {
                string strInstructionalAreaDetails = string.Empty;
                decimal totalArea = 0;
                IQueryable<jntuh_college_area> jntuh_college_area = db.jntuh_college_area.Where(s => s.collegeId == collegeId).Select(e => e);
                strInstructionalAreaDetails += "<table width='100%' border='0' cellpadding='5' cellspacing='0'>";
                strInstructionalAreaDetails += "<tr>";
                strInstructionalAreaDetails += "<td align='left'><b><u>Instructional Area</u></b></td>";

                strInstructionalAreaDetails += "</tr>";
                strInstructionalAreaDetails += "</table>";
                strInstructionalAreaDetails += "<table width='100%' border='1' cellpadding='5' cellspacing='0'><tbody>";
                strInstructionalAreaDetails += "<tr><td width='35%'><p><b>Requirement Type</b></p></td><td width='9%'><p align='center'><b>Required Rooms</b></p></td><td width='9%'><p align='center'><b>Uploaded Rooms</b></p></td><td width='5%'><p align='center'><b>CF</b></p></td><td width='9%'><p align='center'><b>Deficiency</b></p></td><td width='9%'><p align='center'><b>Required Area(Sq.m)</b></p></td><td width='9%'><p align='center'><b>Uploaded Area(Sq.m)</b></p></td><td width='5%'><p align='center'><b>CF</b></p></td><td width='9%'><p align='center'><b>Deficiency</b></p></td></tr>";
                int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
                int prAy = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();

                var mPharmacySpecSum = (from ie in db.jntuh_college_intake_existing
                                        join s in db.jntuh_specialization on ie.specializationId equals s.id
                                        join d in db.jntuh_department on s.departmentId equals d.id
                                        join de in db.jntuh_degree on d.degreeId equals de.id
                                        where ie.isActive == true && ie.academicYearId == (prAy) && (ie.proposedIntake != 0) && ie.collegeId == collegeId && de.id == 2
                                        orderby de.degreeDisplayOrder
                                        select s.id
                ).Distinct().ToList();

                //List<string> collegeDegrees = db.jntuh_college_degree
                //                         .Join(db.jntuh_degree, cd => cd.degreeId, d => d.id, (cd, d) => new { cd, d })
                //                         .Where(s => s.d.isActive == true && s.cd.collegeId == collegeId && s.cd.isActive == true)
                //                         .OrderBy(s => s.d.degreeDisplayOrder)
                //                         .Select(s => s.d.degree).ToList();
                List<string> collegeDegrees = (from ie in db.jntuh_college_intake_existing
                                               join s in db.jntuh_specialization on ie.specializationId equals s.id
                                               join d in db.jntuh_department on s.departmentId equals d.id
                                               join de in db.jntuh_degree on d.degreeId equals de.id
                                               //where ie.academicYearId == (prAy - 1) && (ie.aicteApprovedIntake != 0 || ie.approvedIntake != 0) && ie.collegeId == userCollegeID
                                               where ie.academicYearId == (prAy) && (ie.proposedIntake != 0) && ie.collegeId == collegeId
                                               orderby de.degreeDisplayOrder
                                               select de.degree
                ).Distinct().ToList();

                foreach (string degree in collegeDegrees)
                {
                    var programTypeDegree = db.jntuh_program_type.Where(p => p.programType == degree).FirstOrDefault();

                    int programId = (int)programTypeDegree.id;
                    //string programType = programtypes.Where(it => it.id == item.programId).Select(d => d.programType).FirstOrDefault();
                    List<AdminLand> land = db.jntuh_area_requirement.Where(r => r.isActive == true && r.areaType == "INSTRUCTIONAL" && r.programId == programId).OrderBy(r => r.areaTypeDisplayOrder)
                                 .Select(r => new AdminLand
                                 {
                                     id = r.id,
                                     requirementType = r.requirementType,
                                     programId = r.programId,
                                     requiredRooms = r.requiredRooms,
                                     requiredRoomsCalculation = r.requiredRoomsCalculation,
                                     requiredArea = r.requiredArea,
                                     requiredAreaCalculation = r.requiredAreaCalculation,
                                     areaTypeDescription = r.areaTypeDescription,
                                     areaTypeDisplayOrder = r.areaTypeDisplayOrder,
                                     jntuh_program_type = db.jntuh_program_type.Where(p => p.id == r.programId).FirstOrDefault(),
                                     availableRooms = jntuh_college_area.Where(a => a.areaRequirementId == r.id).Select(a => a.availableRooms).FirstOrDefault(),
                                     availableArea = jntuh_college_area.Where(a => a.areaRequirementId == r.id).Select(a => a.availableArea).FirstOrDefault(),
                                     cfRooms = jntuh_college_area.Where(a => a.areaRequirementId == r.id).Select(a => a.cfrooms).FirstOrDefault(),
                                     cfArea = jntuh_college_area.Where(a => a.areaRequirementId == r.id).Select(a => a.cfarea).FirstOrDefault()
                                 }).ToList();

                    strInstructionalAreaDetails += "<tr>";
                    strInstructionalAreaDetails += "<td colspan='9' style='width: 200%'><p><b>" + programTypeDegree.programType + "</b></p></td>";
                    strInstructionalAreaDetails += "</tr>";
                    foreach (var i in land)
                    {
                        var requiredRooms = string.Empty;
                        var requiredArea = string.Empty;

                        if (i.programId == 6)
                        {
                            requiredRooms = Convert.ToInt16(i.requiredRooms).ToString();
                            requiredArea = Convert.ToDecimal(i.requiredArea).ToString();
                        }
                        else if (i.programId == 7)
                        {
                            if (i.id == 51) // ClassRooms - M.Pharmacy
                            {
                                var rrooms = mPharmacySpecSum.Count * 2;
                                var rarea = rrooms * 36;
                                requiredRooms = rrooms.ToString();
                                requiredArea = rarea.ToString();
                            }
                            else if (i.id == 52) // Laboratory - M.Pharmacy
                            {
                                var rrooms = mPharmacySpecSum.Count * 2;
                                var rarea = (rrooms * 75) + 10;
                                requiredRooms = rrooms.ToString();
                                requiredArea = rarea.ToString();
                            }
                            else
                            {
                                requiredRooms = Convert.ToInt16(i.requiredRooms).ToString();
                                requiredArea = Convert.ToDecimal(i.requiredArea).ToString();
                            }
                        }
                        else
                        {
                            requiredRooms = Convert.ToInt16(i.requiredRooms).ToString();
                            requiredArea = Convert.ToDecimal(i.requiredArea).ToString();
                        }
                        if (requiredRooms == "" || requiredArea == "")
                        {
                            requiredRooms = "";
                            requiredArea = "";
                        }
                        var defTxtRooms = string.Empty;
                        var defTxtArea = string.Empty;
                        strInstructionalAreaDetails += "<tr>";
                        strInstructionalAreaDetails += "<td width='35%'><p>" + i.requirementType + "</p></td>";
                        strInstructionalAreaDetails += "<td width='9%' align='center'> " + Convert.ToInt32(requiredRooms).ToString() + " </td>";
                        if (i.availableRooms != null)
                        {
                            strInstructionalAreaDetails += "<td width='9%' align='center'>" + (int)i.availableRooms + "</td>";
                        }
                        else
                        {
                            strInstructionalAreaDetails += "<td width='9%' align='center'>" + i.availableRooms + "</td>";
                        }
                        strInstructionalAreaDetails += "<td width='5%' valign='top' align='center'>" + Convert.ToInt32(i.cfRooms) + "</td>";
                        if (i.cfRooms != null && i.cfRooms > 0)
                        {
                            var defCond = Convert.ToDecimal(requiredRooms) - Convert.ToDecimal(i.cfRooms);
                            if (defCond > 0)
                            {
                                defTxtRooms = "Yes";
                            }
                            else
                            {
                                defTxtRooms = "-";
                            }
                        }
                        else
                        {
                            var defCond = Convert.ToDecimal(requiredRooms) - Convert.ToDecimal(i.availableRooms);
                            if (defCond > 0)
                            {
                                defTxtRooms = "Yes";
                            }
                            else
                            {
                                defTxtRooms = "-";
                            }
                        }
                        strInstructionalAreaDetails += "<td width='9%' align='center' style='font-weight: bold'>" + defTxtRooms + "</td>";
                        strInstructionalAreaDetails += "<td width='9%' align='center'> " + requiredArea + " </td>";
                        strInstructionalAreaDetails += "<td width='9%' align='center'>" + i.availableArea + "</td>";
                        strInstructionalAreaDetails += "<td width='5%' valign='top' align='center'>" + i.cfArea + "</td>";
                        if (i.cfArea != null && i.cfArea > 0)
                        {
                            var defCond = Convert.ToDecimal(requiredArea) - Convert.ToDecimal(i.cfArea);
                            if (defCond > 0)
                            {
                                defTxtArea = "Yes";
                            }
                            else
                            {
                                defTxtArea = "-";
                            }
                        }
                        else
                        {
                            var defCond = Convert.ToDecimal(requiredArea) - Convert.ToDecimal(i.availableArea);
                            if (defCond > 0)
                            {
                                defTxtArea = "Yes";
                            }
                            else
                            {
                                defTxtArea = "-";
                            }
                        }
                        strInstructionalAreaDetails += "<td width='9%' align='center' style='font-weight: bold'>" + defTxtArea + "</td>";
                        strInstructionalAreaDetails += "</tr>";
                        //if (i.availableArea != null)
                        //{
                        //    totalArea += (int)i.availableArea;
                        //}
                    }
                    //}
                }
                //strInstructionalAreaDetails += "<tr>";
                //strInstructionalAreaDetails += "<td colspan='2' align='right'>Total</td>";
                //strInstructionalAreaDetails += "<td width='10%' align='right'>" + totalArea + "</td>";
                //strInstructionalAreaDetails += "</tr>";
                var totalcollegedivisions = Math.Ceiling(TotalCollegeIntake / 60);
                strInstructionalAreaDetails += "</tbody>";
                strInstructionalAreaDetails += "</table>";
                //strInstructionalAreaDetails += "<li>Total Intake : <b>" + TotalCollegeIntake.ToString() + "</b>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; Total Divisions : <b>" + totalcollegedivisions.ToString() + "</b>.</li>";
                contents = strInstructionalAreaDetails;
                //List<IElement> parsedHtmlElements = HTMLWorker.ParseToList(new StringReader(contents), null);
                return contents;
            }
        }

        public string CommitteeMembers(int? collegeID)
        {
            string collegeCode = db.jntuh_college.Find(collegeID).collegeCode;
            var committee = db.committeemembers2023_24.Where(c => c.CC == collegeCode).Select(c => c).FirstOrDefault();
            var members = string.Empty;
            members += "<table width='100%' border='0' cellpadding='5' cellspacing='0'>";
            members += "<tr>";
            members += "<td align='left'><b><u>Members of FFC Team:</u></b></td>";
            members += "</tr>";
            members += "</table>";
            members += "<table width='100%' border='1' cellpadding='7' cellspacing='0'>";
            members += "<tr>";
            members += "<th align='left'>Team Member 1</th><th align='left'>Team Member 2</th><th align='left'>Team Member 3</th><th align='left'>Inspected On</th>";
            members += "</tr>";
            members += "<tr>";
            members += "<td align=''left'>" + committee.TeamMember1 + "</td><td  align='left'>" + committee.TeamMember2 + "</td><td align=''left'>" + committee.TeamMember3 + "</td><td align=''left'>" + committee.Date + "</td>";
            members += "</tr>";
            members += "</table>";
            return members;
        }
    }


}
