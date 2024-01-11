using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using UAAAS.Models;

namespace UAAAS.Controllers
{
    [ErrorHandling]
    public class UGAllDeficiencyController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();
        private int academicYearId = 0;
        private int BoysHostelId = 0;
        private int GirlsHostelId = 0;
        private int[] CollegeIds;
        private int[] DegreeId;
        private int DegreeTypeId = 0;
        private int AcademicYear = 0;
        private int nextAcademicYearId = 0;
        private int ClosureCourseId = 0;
        private void AcademicYearId()
        {
            //Get Colsure Course Id
            ClosureCourseId = db.jntuh_course_affiliation_status
                                .Where(c => c.courseAffiliationStatusCode.Trim() == "C")
                                .Select(c => c.id)
                                .FirstOrDefault();
            //Get present academic Year Id
            academicYearId = db.jntuh_academic_year
                                   .Where(year => year.isActive == true &&
                                                  year.isPresentAcademicYear == true)
                                   .Select(year => year.id)
                                   .FirstOrDefault();
            //Get Present academic Year
            AcademicYear = db.jntuh_academic_year.Where(year => year.isActive == true &&
                                                                year.id == academicYearId)
                                                 .Select(year => year.actualYear)
                                                 .FirstOrDefault();

            //Get next academic year
            nextAcademicYearId = db.jntuh_academic_year
                                   .Where(year => year.actualYear == (AcademicYear + 1))
                                   .Select(year => year.id)
                                   .FirstOrDefault();
            //Get Ug Degree Type Id
            DegreeTypeId = db.jntuh_degree_type
                             .Where(d => d.degreeType == "UG")
                             .Select(d => d.id)
                             .FirstOrDefault();
            //get Ug Degree Ids
            DegreeId = db.jntuh_degree
                         .Where(d => d.degreeTypeId == DegreeTypeId)
                         .Select(d => d.id)
                         .ToArray();
            //get all Ug degree Ids
            CollegeIds = db.jntuh_college_degree
                          .Where(d => DegreeId.Contains(d.degreeId) &&
                                      d.isActive == true)
                          .Select(d => d.collegeId)
                          .Distinct()
                          .ToArray();
            ViewBag.Colleges = db.jntuh_college
                                 .Where(college => college.isActive == true &&
                                                   CollegeIds.Contains(college.id))
                                 .Select(college => new
                                 {
                                     ID = college.id,
                                     CollegeName = college.collegeCode + " - " + college.collegeName
                                 }).ToList();
            BoysHostelId = db.jntuh_desirable_requirement_type
                             .Where(d => d.isHostelRequirement == false &&
                                         d.requirementType.Trim() == "Boys Hostel" &&
                                         d.isActive== true)
                             .Select(r => r.id)
                             .FirstOrDefault();
            GirlsHostelId = db.jntuh_desirable_requirement_type
                              .Where(d => d.isHostelRequirement == false &&
                                          d.requirementType.Trim() == "Girls Hostel" &&
                                          d.isActive == true)
                              .Select(r => r.id)
                              .FirstOrDefault();


        }

        private List<UGAllDeficiency> DeficiencyList(int? CollegeId)
        {
            //int[] collegeIs = { 216, 7, 180, 206, 315, 9, 268 };
            List<UGAllDeficiency> UGAllDeficiencyList = new List<UGAllDeficiency>();
            if (CollegeId == null)
            {
                UGAllDeficiencyList = db.jntuh_college
                                        .Where(college => college.isActive == true &&
                                            CollegeIds.Contains(college.id))
                                        .Select(college => new UGAllDeficiency
                                        {
                                            CollegeId = college.id,
                                            CollegeCode = college.collegeCode,
                                            CollegeName = college.collegeName,
                                        }).ToList();
            }
            else
            {
                UGAllDeficiencyList = db.jntuh_college
                                         .Where(college => college.isActive == true &&
                                                           college.id == CollegeId)
                                         .Select(college => new UGAllDeficiency
                                         {
                                             CollegeId = college.id,
                                             CollegeCode = college.collegeCode,
                                             CollegeName = college.collegeName,
                                         }).ToList();
            }
            foreach (var item in UGAllDeficiencyList)
            {
                //Binding collegeCode CollegeName and specializationDetails Data to table
                item.CollegeSpecializations = GetUGWithDeficiencyCollegeSpecializations(item.CollegeId);
                item.Labs = Labs(item.CollegeId);
                item.Computers = Computers(item.CollegeId);
                item.Strength = Strength(item.CollegeId);
                item.Faculty = Faculty(item.CollegeId);
                item.FacultyNumber = GetFacultyNumber(item.CollegeId);
                item.FacultyPercentage = GetFacultyPercentage(item.CollegeId);
                item.Library = Library(item.CollegeId);
                item.BoysHostel = BoysHostel(item.CollegeId);
                item.GirlsHostel = GirlsHostel(item.CollegeId);
                item.PlacementAndTraining = PlacementAndTraining(item.CollegeId);
                item.CollegeAddress = CollegeAddress(item.CollegeId);
            }
            ViewBag.CollegeSpecializations = UGAllDeficiencyList;
            ViewBag.Count = UGAllDeficiencyList.Count();
            return UGAllDeficiencyList;
        }

        [Authorize(Roles = "Admin")]
        public ActionResult UGAllDeficiency()
        {
            AcademicYearId();
            List<UGAllDeficiency> UGAllDeficiencies = DeficiencyList(null);
            return View("~/Views/Reports/UGAllDeficiency.cshtml");
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult UGAllDeficiency(UGAllDeficiency collegeSpecializationsDetails,string cmd)
        {
            List<UGAllDeficiency> UGAllDeficiencyList = new List<UGAllDeficiency>();
            int CollegeId = collegeSpecializationsDetails.CollegeId;
            if (CollegeId == 0)
            {
                AcademicYearId();
                UGAllDeficiencyList = DeficiencyList(null);
            }
            else
            {
                AcademicYearId();
                UGAllDeficiencyList = DeficiencyList(CollegeId);
            }
            ViewBag.CollegeSpecializations = UGAllDeficiencyList;
            ViewBag.Count = UGAllDeficiencyList.Count();
            int Count = UGAllDeficiencyList.Count();
            if (cmd == "Export To Excel" && Count != 0)
            {
                Response.ClearContent();
                Response.Buffer = true;
                Response.AddHeader("content-disposition", "attachment; filename=UGAllDeficiency.xls");
                Response.ContentType = "application/vnd.ms-excel";
                return PartialView("~/Views/Reports/_UGAllDeficiency.cshtml");
            }
            else
            {                
                return View("~/Views/Reports/UGAllDeficiency.cshtml");
            }              
        }

        private List<UGWithDeficiencyCollegeSpecializations> GetUGWithDeficiencyCollegeSpecializations(int CollegeId)
        {

            List<UGWithDeficiencyCollegeSpecializations> UGWithDeficiencyCollegeSpecializations = new List<UGWithDeficiencyCollegeSpecializations>();
            List<UGWithDeficiencySpecializations> SpecializationList = db.jntuh_college_intake_proposed
                                                                         .Where(proposedIntake => proposedIntake.collegeId == CollegeId &&
                                                                                                  proposedIntake.academicYearId == nextAcademicYearId &&
                                                                                                  proposedIntake.courseAffiliationStatusCodeId != ClosureCourseId)
                                                                         .OrderBy(proposedIntake => proposedIntake.specializationId)
                                                                         .Select(proposedIntake => new UGWithDeficiencySpecializations
                                                                         {
                                                                             CollegeId = CollegeId,
                                                                             SpecializationId = proposedIntake.specializationId,
                                                                             ShiftId = proposedIntake.shiftId,
                                                                             ProposedIntake = proposedIntake.proposedIntake,
                                                                             CourseAffiliationStatusCodeId = proposedIntake.courseAffiliationStatusCodeId
                                                                         }).ToList();
            string adminMBA = string.Empty;
            foreach (var item in SpecializationList)
            {
                string NewSpecialization = string.Empty;
                string NewIntake = string.Empty;
                UGWithDeficiencyCollegeSpecializations UGWithDeficiencySpecializations = new UGWithDeficiencyCollegeSpecializations();
                int departmentId = db.jntuh_specialization
                                      .Where(specialization => specialization.id == item.SpecializationId)
                                      .Select(specialization => specialization.departmentId)
                                      .FirstOrDefault();
                int degreeId = db.jntuh_department
                                  .Where(department => department.id == departmentId)
                                  .Select(department => department.degreeId)
                                  .FirstOrDefault();
                item.DegreeTypeId = db.jntuh_degree.Where(degree => degree.id == degreeId).Select(degree => degree.degreeTypeId).FirstOrDefault();
                string degreeType = db.jntuh_degree_type.Where(degree => degree.id == item.DegreeTypeId).Select(degree => degree.degreeType).FirstOrDefault();
                string degreeName = db.jntuh_degree.Where(degree => degree.id == degreeId && degree.isActive == true).Select(degree => degree.degree).FirstOrDefault();
                string specializationName = db.jntuh_specialization.Where(specialization => specialization.id == item.SpecializationId)
                                                      .Select(specialization => specialization.specializationName).FirstOrDefault();
                if (degreeType != null)
                {
                    if (degreeType.ToUpper().Trim() == "PG")
                    {
                        if (degreeName.ToUpper().Trim() == "MBA" && adminMBA != "MBA")
                        {
                            adminMBA = "MBA";
                            NewSpecialization = degreeName;
                            NewIntake = GetMBAIntake(item.CollegeId, degreeId);
                        }
                        if (degreeName.ToUpper().Trim() == "M.TECH" || degreeName.ToUpper().Trim() == "M.PHARMACY")
                        {
                            NewIntake = GetIntake(item.CollegeId, item.SpecializationId, item.ShiftId, item.CourseAffiliationStatusCodeId, item.ProposedIntake);
                            NewSpecialization = item.ShiftId == 1 ? degreeName + "-" + specializationName : degreeName + "-" + specializationName + "-" + "(II Shift)";
                        }
                        if (degreeName.ToUpper().Trim() != "M.TECH" && degreeName.ToUpper().Trim() == "M.PHARMACY" && degreeName.ToUpper().Trim() == "MBA")
                        {
                            NewIntake = GetIntake(item.CollegeId, item.SpecializationId, item.ShiftId, item.CourseAffiliationStatusCodeId, item.ProposedIntake);
                            NewSpecialization = item.ShiftId == 1 ? specializationName : specializationName + "-" + "(II Shift)";
                        }
                    }
                    else
                    {
                        NewIntake = GetIntake(item.CollegeId, item.SpecializationId, item.ShiftId, item.CourseAffiliationStatusCodeId, item.ProposedIntake);
                        NewSpecialization = item.ShiftId == 1 ? specializationName : specializationName + "-" + "(II Shift)";

                    }
                    UGWithDeficiencySpecializations.Specialization = NewSpecialization;
                    UGWithDeficiencySpecializations.Intake = NewIntake;
                    UGWithDeficiencyCollegeSpecializations.Add(UGWithDeficiencySpecializations);
                }
            }
            return UGWithDeficiencyCollegeSpecializations;
        }

        private string GetMBAIntake(int collegeId, int degreeId)
        {
            int existingIntake = 0;
            int proposedIntake = 0;
            string intake = string.Empty;
            int[] specializationId = (from department in db.jntuh_department
                                      join specialization in db.jntuh_specialization on department.id equals specialization.departmentId
                                      where department.degreeId == degreeId
                                      select specialization.id).ToArray();
            foreach (var item in specializationId)
            {
                int[] shift = db.jntuh_shift.Where(s => s.isActive == true).Select(s => s.id).ToArray();
                foreach (var shiftId in shift)
                {
                    int id = db.jntuh_college_intake_proposed.Where(ei => ei.collegeId == collegeId &&
                                                                          ei.specializationId == item &&
                                                                          ei.academicYearId == nextAcademicYearId &&
                                                                          ei.shiftId == shiftId &&
                                                                          ei.courseAffiliationStatusCodeId != ClosureCourseId)
                                                             .Select(ei => ei.id)
                                                             .FirstOrDefault();
                    if (id > 0)
                    {
                        proposedIntake += db.jntuh_college_intake_proposed
                                            .Where(ei => ei.id == id)
                                            .Select(ei => ei.proposedIntake).FirstOrDefault();
                        existingIntake += db.jntuh_college_intake_existing
                                            .Where(ei => ei.collegeId == collegeId &&
                                                         ei.specializationId == item &&
                                                         ei.academicYearId == academicYearId &&
                                                         ei.shiftId == shiftId)
                                           .Select(ei => ei.approvedIntake)
                                           .FirstOrDefault();
                    }

                }
            }
            if (proposedIntake != 0)
            {
                if (existingIntake == proposedIntake)
                {
                    intake = proposedIntake.ToString() + "$";
                }
                else if (existingIntake == 0)
                {
                    intake = proposedIntake.ToString() + "#";
                }
                else
                {
                    intake = proposedIntake.ToString() + "*";
                }
            }
            return intake;
        }

        private string GetIntake(int collegeId, int specializationId, int shiftId, int courseAffiliationStatusCodeId, int proposedIntake)
        {
            string intake = string.Empty;
            string courseAffiliationStatusCode = string.Empty;
            courseAffiliationStatusCode = (db.jntuh_course_affiliation_status.Where(status => status.id == courseAffiliationStatusCodeId).Select(status => status.courseAffiliationStatusCode).FirstOrDefault()).ToUpper().Trim();
            if (proposedIntake != 0)
            {
                if (courseAffiliationStatusCode == "S")
                {
                    intake = proposedIntake.ToString() + "$";
                }
                else if (courseAffiliationStatusCode == "N")
                {
                    intake = proposedIntake.ToString() + "#";
                }
                else
                {
                    intake = proposedIntake.ToString() + "*";
                }
            }
            return intake;
        }

        private string GetFacultyNumber(int CollegeId)
        {
            string FacultyNum;
            int facultyRatifiedCount = db.jntuh_college_faculty
                                        .Where(f => f.collegeId == CollegeId &&
                                                    f.isFacultyRatifiedByJNTU == true)
                                        .Select(f => f.id)
                                        .Count();
            int facultyCount = db.jntuh_college_faculty
                                        .Where(f => f.collegeId == CollegeId)
                                        .Select(f => f.id)
                                        .Count();
            if (facultyRatifiedCount != 0 && facultyCount != 0)
            {
                FacultyNum = Convert.ToString(facultyRatifiedCount) + "/" + Convert.ToString(facultyCount)+".";
            }
            else
            {
                FacultyNum = "0" + "/" + string.Empty + Convert.ToString(facultyCount) + ".";
            }
            return FacultyNum;
        }

        private decimal GetFacultyPercentage(int CollegeId)
        {
            decimal Percentage = 0;
            decimal facultyRatifiedCount = db.jntuh_college_faculty
                                        .Where(f => f.collegeId == CollegeId &&
                                                    f.isFacultyRatifiedByJNTU == true)
                                        .Select(f => f.id)
                                        .Count();
            decimal facultyCount = db.jntuh_college_faculty
                                        .Where(f => f.collegeId == CollegeId)
                                        .Select(f => f.id)
                                        .Count();
            if (facultyRatifiedCount != 0 && facultyCount != 0)
            {
                Percentage = (facultyRatifiedCount / facultyCount) * 100;
                Percentage = Math.Round(Percentage, 2);
            }
            return Percentage;
        }

        private string Labs(int CollegeId)
        {
            string Lab = string.Empty;
            int[] LabCount = db.jntuh_college_lab
                             .Where(c => c.collegeId == CollegeId)
                             .Select(c => c.id)
                             .ToArray();
            if (LabCount.Count() > 0)
            {
                Lab = "YES";
            }
            else
            {
                Lab = "NO";
            }
            return Lab;
        }

        private int Computers(int CollegeId)
        {
            int CollegeComputers = 0;
            int[] Computers = db.jntuh_college_computer_student_ratio
                                .Where(c => c.collegeId == CollegeId)
                                .Select(c => c.availableComputers)
                                .ToArray();
            if (Computers.Count() > 0)
            {
                CollegeComputers = Computers.Sum();
            }
            return CollegeComputers;
        }

        private int Strength(int CollegeId)
        {
            int Strength = 0;
            int[] StrengthCount = db.jntuh_college_faculty_student_ratio
                                   .Where(c => c.collegeId == CollegeId)
                                   .Select(c => c.totalIntake)
                                   .ToArray();
            if (StrengthCount.Count() > 0)
            {
                Strength = StrengthCount.Sum();
            }
            return Strength;
        }

        private int Faculty(int CollegeId)
        {
            int CollegeFaculty=0;
            int[] FacultyCount = db.jntuh_college_faculty
                                   .Where(f => f.collegeId == CollegeId)
                                   .Select(f => f.id)
                                   .ToArray();
            CollegeFaculty = FacultyCount.Count();
            return CollegeFaculty;
        }

        private string Library(int CollegeId)
        {
            string CollegeLibrary = string.Empty;
            int[] LibraryCount = db.jntuh_college_library
                                   .Where(l => l.collegeId == CollegeId)
                                   .Select(l => l.id)
                                   .ToArray();
            if (LibraryCount.Count() > 0)
            {
                CollegeLibrary = "YES";
            }
            else
            {
                CollegeLibrary = "NO";
            }
            return CollegeLibrary;
        }

        private string BoysHostel(int CollegeId)
        {
            string CollegeBoysHostel = string.Empty;
            int ExistBoysHostelId = db.jntuh_college_desirable_requirement
                                      .Where(h => h.isAvaiable == true &&
                                                  h.requirementTypeID == BoysHostelId &&
                                                  h.collegeId==CollegeId)
                                      .Select(h => h.id)
                                      .FirstOrDefault();
            if (ExistBoysHostelId > 0)
            {
                CollegeBoysHostel = "YES";
            }
            else
            {
                CollegeBoysHostel = "NO";
            }
            return CollegeBoysHostel;
        }

        private string GirlsHostel(int CollegeId)
        {
            string CollegeGirlsHostel = string.Empty;
            int ExistGirlsHostelId = db.jntuh_college_desirable_requirement
                                 .Where(h => h.isAvaiable == true &&
                                             h.requirementTypeID == GirlsHostelId &&
                                             h.collegeId==CollegeId)
                                 .Select(h => h.id)
                                 .FirstOrDefault();
            if (ExistGirlsHostelId > 0)
            {
                CollegeGirlsHostel = "YES";
            }
            else
            {
                CollegeGirlsHostel = "NO";
            }
            return CollegeGirlsHostel;
        }

        private string PlacementAndTraining(int CollegeId)
        {
            string CollegePlacementAndTraining = string.Empty;
            int[] PlacementAndTrainingCount = db.jntuh_college_placement
                                              .Where(p => p.collegeId == CollegeId)
                                              .Select(h => h.id)
                                              .ToArray();
            if (PlacementAndTrainingCount.Count() > 0)
            {
                CollegePlacementAndTraining = "YES";
            }
            else
            {
                CollegePlacementAndTraining = "NO";
            }
            return CollegePlacementAndTraining;
        }

        private string CollegeAddress(int CollegeId)
        {
            string Address = string.Empty;
            string District = string.Empty;
            jntuh_address CollegeAddress = db.jntuh_address
                                           .Where(a => a.addressTye == "COLLEGE" &&
                                                       a.collegeId == CollegeId)
                                           .FirstOrDefault();
            if (CollegeAddress.townOrCity != null || CollegeAddress.townOrCity != string.Empty)
            {
                Address += CollegeAddress.townOrCity+",";
            }
            if (CollegeAddress.mandal != null || CollegeAddress.mandal != string.Empty)
            {
                Address += CollegeAddress.mandal+",";
            }
            if (CollegeAddress.districtId != 0 || CollegeAddress.districtId != null)
            {
                District = db.jntuh_district
                             .Where(d => d.id == CollegeAddress.districtId)
                             .Select(d => d.districtName)
                             .FirstOrDefault();
                Address += District + ",";
            }
            if (CollegeAddress.pincode != 0 || CollegeAddress.pincode != null)
            {
                Address += CollegeAddress.pincode.ToString();
            }
            return Address;
        }
    }
}
