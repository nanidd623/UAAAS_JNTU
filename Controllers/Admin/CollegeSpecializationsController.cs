using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using UAAAS.Models;

namespace UAAAS.Controllers
{
    [ErrorHandling]
    public class CollegeSpecializationsController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();
        //
        // GET: /CollegeSpecializations/

        [Authorize(Roles = "Admin")]
        public ActionResult CollegeSpecializations()
        {

            ViewBag.Districts = db.jntuh_district.Where(district => district.isActive == true)
                                                 .ToList();
            int[] collegeIs = { 216, 7, 180, 206, 315, 9, 268 };
            List<CollegeSpecializations> collegeSpecializations = db.jntuh_college
                .Where(c => collegeIs.Contains(c.id))
                .Select(college => new CollegeSpecializations
                                                                   {
                                                                       collegeId = college.id,
                                                                       collegeCode = college.collegeCode,
                                                                       collegeName = college.collegeName
                                                                   }).OrderBy(college => college.collegeId)
                                                                   .Take(10)
                                                                     .ToList();
            foreach (var colleges in collegeSpecializations)
            {
                colleges.collegeSpecializations = GetAdminSpecialization(colleges.collegeId);
            }
            ViewBag.CollegeSpecializations = collegeSpecializations;
            ViewBag.Count = collegeSpecializations.Count();
            return View("~/Views/Admin/CollegeSpecializations.cshtml");
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public ActionResult CollegeSpecializations(CollegeSpecializations collegeSpecializationsDetails)
        {
            List<CollegeSpecializations> collegeSpecializations = new List<CollegeSpecializations>();
            if (collegeSpecializationsDetails.districtId == 0)
            {
                return RedirectToAction("CollegeSpecializations", "CollegeSpecializations");
            }
            else
            {
                int[] collegesId = db.jntuh_address.Where(address => address.addressTye == "COLLEGE" && address.districtId == collegeSpecializationsDetails.districtId)
                                                       .Select(address => address.collegeId)
                                                       .ToArray();
                    ViewBag.Districts = db.jntuh_district.Where(district => district.isActive == true)
                                                     .ToList();

                    collegeSpecializations = db.jntuh_college.Where(college => collegesId.Contains(college.id)).Select(college => new CollegeSpecializations
                    {
                        collegeId = college.id,
                        collegeCode = college.collegeCode,
                        collegeName = college.collegeName
                    }).OrderBy(college => college.collegeId)
                    .ToList();
                    foreach (var colleges in collegeSpecializations)
                    {
                        colleges.collegeSpecializations = GetAdminSpecialization(colleges.collegeId);
                    }               
            }
            ViewBag.CollegeSpecializations = collegeSpecializations;
            ViewBag.Count = collegeSpecializations.Count();
            return View("~/Views/Admin/CollegeSpecializations.cshtml");
        }
        private List<AdminSpecialization> GetAdminSpecialization(int collegeId)
        {
            int academicYearId = db.jntuh_academic_year.Where(year => year.isActive == true &&
                                                                            year.isPresentAcademicYear == true)
                                                     .Select(year => year.id)
                                                     .FirstOrDefault();
            int AcademicYear = db.jntuh_academic_year.Where(year => year.isActive == true && year.id == academicYearId)
                                                     .Select(year => year.actualYear).FirstOrDefault();
            int nextAcademicYearId = db.jntuh_academic_year.Where(year => year.actualYear == (AcademicYear + 1)).Select(year => year.id).FirstOrDefault();
            List<AdminSpecialization> adminSpecializationDetails = new List<AdminSpecialization>();
            List<AdminCollegeSpecialization> adminCollegeSpecializationDetails = db.jntuh_college_intake_proposed.Where(proposedIntake => proposedIntake.collegeId == collegeId &&
                                                                                       proposedIntake.academicYearId == nextAcademicYearId)
                                                                                       .OrderBy(proposedIntake => proposedIntake.specializationId)
                                                                                       .Select(proposedIntake => new AdminCollegeSpecialization
                                                                                       {
                                                                                           collegeId = collegeId,
                                                                                           specializationId = proposedIntake.specializationId,
                                                                                           shiftId = proposedIntake.shiftId,
                                                                                           proposedIntake = proposedIntake.proposedIntake,
                                                                                           courseAffiliationStatusCodeId = proposedIntake.courseAffiliationStatusCodeId
                                                                                       }).ToList();
            string adminMBA = string.Empty;
            foreach (var item in adminCollegeSpecializationDetails)
            {

                AdminSpecialization adminSpecialization = new AdminSpecialization();
                int[] departmentIds = db.jntuh_specialization.Where(specialization => specialization.id == item.specializationId).Select(specialization => specialization.departmentId).ToArray();
                foreach (var departmentId in departmentIds)
                {
                    int[] degreesId = db.jntuh_department.Where(department => department.id == departmentId).OrderBy(department => department.degreeId)
                        .Select(department => department.degreeId).ToArray();
                    foreach (var degrees in degreesId)
                    {
                        adminSpecialization.intake = GetIntake(item.collegeId, item.specializationId, item.shiftId, item.courseAffiliationStatusCodeId, item.proposedIntake);
                        item.degreeTypeId = db.jntuh_degree.Where(degree => degree.id == degrees).Select(degree => degree.degreeTypeId).FirstOrDefault();
                        string degreeType = db.jntuh_degree_type.Where(degree => degree.id == item.degreeTypeId).Select(degree => degree.degreeType).FirstOrDefault();
                        string degreeName = db.jntuh_degree.Where(degree => degree.id == degrees && degree.isActive == true).Select(degree => degree.degree).FirstOrDefault();
                        string specializationName = db.jntuh_specialization.Where(specialization => specialization.id == item.specializationId)
                                                              .Select(specialization => specialization.specializationName).FirstOrDefault();
                        if (degreeType != null)
                        {
                            if (degreeType.ToUpper().Trim() == "PG")
                            {
                                if (degreeName.ToUpper().Trim() == "MBA" && adminMBA != "MBA")
                                {
                                    adminMBA = "MBA";
                                    adminSpecialization.specialization = degreeName;
                                    adminSpecialization.intake = GetMBAIntake(item.collegeId, degrees);
                                }
                                if (degreeName.ToUpper().Trim() == "M.TECH" || degreeName.ToUpper().Trim() == "M.PHARMACY")
                                {
                                    if (item.shiftId == 1)
                                    {
                                        adminSpecialization.specialization = degreeName + "-" + specializationName;
                                    }
                                    else
                                    {
                                        adminSpecialization.specialization = degreeName + "-" + specializationName + "-" + item.shiftId.ToString();
                                    }
                                }
                                if (degreeName.ToUpper().Trim() != "M.TECH" && degreeName.ToUpper().Trim() == "M.PHARMACY" && degreeName.ToUpper().Trim() == "MBA")
                                {
                                    if (item.shiftId == 1)
                                    {
                                        adminSpecialization.specialization = specializationName;
                                    }
                                    else
                                    {
                                        adminSpecialization.specialization = specializationName + "-" + item.shiftId.ToString();
                                    }
                                }
                            }
                            else
                            {
                                if (item.shiftId == 1)
                                {
                                    adminSpecialization.specialization = specializationName;
                                }
                                else
                                {
                                    adminSpecialization.specialization = specializationName + "-" + item.shiftId.ToString();
                                }
                            }
                            adminSpecializationDetails.Add(adminSpecialization);
                        }
                    }
                }
            }
            return adminSpecializationDetails;
        }

        private string GetMBAIntake(int collegeId, int degreeId)
        {
            int existingIntake = 0;
            int proposedIntake = 0;
            int academicYearId = db.jntuh_academic_year.Where(year => year.isActive == true &&
                                                                            year.isPresentAcademicYear == true)
                                                     .Select(year => year.id)
                                                     .FirstOrDefault();
            int AcademicYear = db.jntuh_academic_year.Where(year => year.isActive == true && year.id == academicYearId)
                                                     .Select(year => year.actualYear).FirstOrDefault();
            int nextAcademicYearId = db.jntuh_academic_year.Where(year => year.actualYear == (AcademicYear + 1)).Select(year => year.id).FirstOrDefault();
            int[] specializationId = (from department in db.jntuh_department
                                      join specialization in db.jntuh_specialization on department.id equals specialization.departmentId
                                      where department.degreeId == degreeId
                                      select specialization.id).ToArray();
            foreach (var item in specializationId)
            {
                int[] shift = db.jntuh_shift.Where(s => s.isActive == true).Select(s => s.id).ToArray();
                foreach (var shiftId in shift)
                {                    
                    int id = db.jntuh_college_intake_proposed.Where(ei => ei.collegeId == collegeId && ei.specializationId == item &&
                         ei.academicYearId == nextAcademicYearId && ei.shiftId == shiftId)
                        .Select(ei => ei.id).FirstOrDefault();
                    if (id > 0)
                    {
                        proposedIntake += db.jntuh_college_intake_proposed.Where(ei => ei.collegeId == collegeId && ei.specializationId == item &&
                         ei.academicYearId == nextAcademicYearId && ei.shiftId == shiftId)
                        .Select(ei => ei.proposedIntake).FirstOrDefault();
                        existingIntake += db.jntuh_college_intake_existing.Where(ei => ei.collegeId == collegeId && ei.specializationId == item &&
                             ei.academicYearId == academicYearId && ei.shiftId == shiftId)
                            .Select(ei => ei.approvedIntake).FirstOrDefault();
                    }
                    
                }
            }
            string intake = string.Empty;
            if (existingIntake == proposedIntake)
            {
                intake = (existingIntake.ToString() + "#");
            }
            if (existingIntake > proposedIntake)
            {
                intake = (existingIntake.ToString() + "@" + (existingIntake - proposedIntake).ToString());
            }
            if (existingIntake < proposedIntake)
            {
                intake = (existingIntake.ToString() + "%" + (proposedIntake - existingIntake).ToString());
            }
            if (proposedIntake == 0)
            {
                intake = (existingIntake.ToString() + "?");
            }
            if (existingIntake == 0)
            {
                intake = (proposedIntake.ToString() + "^");
            }
            return intake;
        }
        private string GetIntake(int collegeId, int specializationId, int shiftId, int courseAffiliationStatusCodeId, int proposedIntake)
        {
            int academicYearId = db.jntuh_academic_year.Where(year => year.isActive == true &&
                                                                            year.isPresentAcademicYear == true)
                                                     .Select(year => year.id)
                                                     .FirstOrDefault();

            string intake = string.Empty;
            string courseAffiliationStatusCode = string.Empty;
            string existingIntake = string.Empty;
            string adminProposedIntake = string.Empty;
            courseAffiliationStatusCode = (db.jntuh_course_affiliation_status.Where(status => status.id == courseAffiliationStatusCodeId).Select(status => status.courseAffiliationStatusCode).FirstOrDefault()).ToUpper().Trim();
            if (courseAffiliationStatusCode == "S")
            {
                existingIntake = string.Empty;
                adminProposedIntake = proposedIntake.ToString();
                intake = adminProposedIntake + "$";
            }
            else if (courseAffiliationStatusCode == "N")
            {
                existingIntake = string.Empty;
                adminProposedIntake = proposedIntake.ToString();
                intake = adminProposedIntake + "*";
            }
            else if (courseAffiliationStatusCode == "I")
            {
                int eIntake = db.jntuh_college_intake_existing.Where(ei => ei.collegeId == collegeId && ei.specializationId == specializationId &&
                     ei.academicYearId == academicYearId && ei.shiftId == shiftId)
                    .Select(ei => ei.approvedIntake).FirstOrDefault();
                existingIntake = eIntake.ToString();
                adminProposedIntake = (proposedIntake - eIntake).ToString();
                intake = existingIntake + "+" + adminProposedIntake;
            }
            else if (courseAffiliationStatusCode == "R")
            {
                int eIntake = db.jntuh_college_intake_existing.Where(ei => ei.collegeId == collegeId && ei.specializationId == specializationId &&
                     ei.academicYearId == academicYearId && ei.shiftId == shiftId)
                    .Select(ei => ei.approvedIntake).FirstOrDefault();
                existingIntake = eIntake.ToString();
                adminProposedIntake = (eIntake - proposedIntake).ToString();
                intake = existingIntake + "&" + adminProposedIntake;
            }
            else if (courseAffiliationStatusCode == "C")
            {
                int eIntake = db.jntuh_college_intake_existing.Where(ei => ei.collegeId == collegeId && ei.specializationId == specializationId &&
                     ei.academicYearId == academicYearId && ei.shiftId == shiftId)
                    .Select(ei => ei.approvedIntake).FirstOrDefault();
                adminProposedIntake = string.Empty;
                intake = eIntake.ToString() + "(0)";
            }
            return intake;
        }
    }
}
