using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Security;
using DotNetOpenAuth.Messaging;
using UAAAS.Models;
using System.Configuration;

namespace UAAAS.Controllers
{
    [ErrorHandling]
    public class CollegeFacultyController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();

        public ActionResult Index()
        {
            return View();
        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult Create(string fid, string type, string collegeId)
        {
            TempData["success"] = null;
            TempData["Error"] = null;

            int fID = 0;
            int fType = 0;

            if (fid != null)
            {
                fID = Convert.ToInt32(Utilities.DecryptString(fid, WebConfigurationManager.AppSettings["CryptoKey"]));
            }

            if (type != null)
            {
                fType = Convert.ToInt32(Utilities.DecryptString(type, WebConfigurationManager.AppSettings["CryptoKey"]));
                ViewBag.FacultyType = fType;
            }

            ViewBag.Type = type;
            ViewBag.Id = fid;

            ViewBag.FacultyType = fType;
            ViewBag.FacultyID = fID;
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int ay0 = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(collegeUser => collegeUser.userID == userID).Select(collegeUser => collegeUser.collegeID).FirstOrDefault();
            if (userCollegeID == 0)
            {
                if (Roles.IsUserInRole("Admin") || Roles.IsUserInRole("DataEntry"))
                {
                    if (collegeId != null)
                    {
                        userCollegeID = Convert.ToInt32(Utilities.DecryptString(collegeId, WebConfigurationManager.AppSettings["CryptoKey"]));
                    }
                    else if (fID > 0)
                    {
                        userCollegeID = db.jntuh_college_faculty.Where(i => i.id == fID).Select(i => i.collegeId).FirstOrDefault();
                    }
                }
            }
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            DateTime todayDate = DateTime.Now.Date;
            int status = db.jntuh_college_edit_status.Where(editStatus => editStatus.collegeId == userCollegeID &&
                                                                                     editStatus.IsCollegeEditable == false &&
                                                                                     editStatus.editFromDate <= todayDate &&
                                                                                     editStatus.editToDate >= todayDate)
                                                                .Select(editStatus => editStatus.id)
                                                                .FirstOrDefault();
            if (status > 0 && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("View");
            }

            var cSpcIds =
               db.jntuh_college_intake_existing.Where(i => i.collegeId == userCollegeID && (i.academicYearId == ay0 - 1 || i.academicYearId == ay0 - 2 || i.academicYearId == ay0 - 3) && (i.approvedIntake != 0 && i.approvedIntake != null))
                //.GroupBy(r => new { r.specializationId })
                   .Select(s => s.specializationId)
                   .ToList();

            var DepartmentsData = db.jntuh_specialization.Where(e => e.isActive == true && cSpcIds.Contains(e.id)).Select(e => e.departmentId).Distinct().ToArray();


            var DegreeIds = db.jntuh_department.Where(e => e.isActive == true && DepartmentsData.Contains(e.id)).Select(e => e.degreeId).Distinct().ToList();

            var degrees = DegreeIds.Join(db.jntuh_degree, degreeId => degreeId, degree => degree.id,
                                                                 (degreeId, degree) => new
                                                                 {
                                                                     degreeId,
                                                                     //collegeDegree.collegeId,
                                                                     //collegeDegree.isActive,
                                                                     degree.degree,
                                                                     degree.degreeDisplayOrder
                                                                 })
                                                             .Select(collegeDegree => new
                                                             {
                                                                 collegeDegree.degreeId,
                                                                 collegeDegree.degree,
                                                                 collegeDegree.degreeDisplayOrder
                                                             }).OrderBy(d => d.degreeDisplayOrder).ToList();

            //var degrees = db.jntuh_college_degree.Join(db.jntuh_degree, collegeDegree => collegeDegree.degreeId, degree => degree.id,
            //                                                     (collegeDegree, degree) => new
            //                                                     {
            //                                                         collegeDegree.degreeId,
            //                                                         collegeDegree.collegeId,
            //                                                         collegeDegree.isActive,
            //                                                         degree.degree,
            //                                                         degree.degreeDisplayOrder
            //                                                     })
            //                                                 .Where(collegeDegree => collegeDegree.collegeId == userCollegeID)
            //                                                 .Select(collegeDegree => new
            //                                                 {
            //                                                     collegeDegree.degreeId,
            //                                                     collegeDegree.degree,
            //                                                     collegeDegree.degreeDisplayOrder
            //                                                 }).OrderBy(d => d.degreeDisplayOrder).ToList();
            ViewBag.degree = degrees;
            ViewBag.DegreeCount = degrees.Count();
            ViewBag.shift = db.jntuh_shift.Where(s => s.isActive == true).OrderBy(s => s.shiftName).ToList();
            List<DistinctDepartment> depts = new List<DistinctDepartment>();
            string existingDepts = string.Empty;
            var desigIds = new int[] { 1, 2, 3, 4 };
            foreach (var item in db.jntuh_department.Where(s => s.isActive == true && DepartmentsData.Contains(s.id)).OrderBy(s => s.departmentName))
            {
                if (!existingDepts.Split(',').Contains(item.departmentName))
                {
                    depts.Add(new DistinctDepartment { id = item.id, departmentName = item.departmentName });
                    existingDepts = existingDepts + "," + item.departmentName;
                }
            }
            if (fType == 3 || fType == 2) // Add others deparment to technical,non-teaching faculty
            {
                depts.Add(new DistinctDepartment { id = 60, departmentName = "Others" });
            }

            ViewBag.department = depts;
            ViewBag.category = db.jntuh_faculty_category.Where(c => c.isActive == true).ToList();
            ViewBag.designation = db.jntuh_designation.Where(c => c.isActive == true && desigIds.Contains(c.id)).ToList();

            List<SelectListItem> ratifiedDuration = new List<SelectListItem>();
            for (int i = 1; i <= 10; i++)
            {
                ratifiedDuration.Add(new SelectListItem { Text = i.ToString(), Value = i.ToString() });
            }
            ViewBag.duration = ratifiedDuration;

            List<SelectListItem> prevExperience = new List<SelectListItem>();
            for (int i = 0; i <= 100; i++)
            {
                prevExperience.Add(new SelectListItem { Text = i.ToString(), Value = i.ToString() });
            }
            ViewBag.prevExperience = prevExperience;

            List<SelectListItem> years = new List<SelectListItem>();
            for (int i = 1940; i <= DateTime.Now.Year; i++)
            {
                years.Add(new SelectListItem { Text = i.ToString(), Value = i.ToString() });
            }
            ViewBag.years = years;

            List<SelectListItem> division = new List<SelectListItem>();
            for (int i = 1; i <= 5; i++)
            {
                division.Add(new SelectListItem { Text = i.ToString(), Value = i.ToString() });
            }
            ViewBag.division = division;

            CollegeFaculty collegeFaculty = new CollegeFaculty();
            collegeFaculty.collegeId = userCollegeID;
            var edcatIds = new int[] { 1, 2, 3, 4 };
            if (fID == 0)
            {
                collegeFaculty.FacultyEducation = db.jntuh_education_category.Where(e => e.isActive == true && edcatIds.Contains(e.id))
                                                            .Select(e => new FacultyEducation
                                                            {
                                                                educationId = e.id,
                                                                educationName = e.educationCategoryName,
                                                                studiedEducation = string.Empty,
                                                                passedYear = 0,
                                                                percentage = 0,
                                                                division = 0,
                                                                university = string.Empty,
                                                                place = string.Empty,
                                                            }).ToList();
            }
            else
            {
                collegeFaculty.FacultyEducation = db.jntuh_education_category.Where(e => e.isActive == true && edcatIds.Contains(e.id))
                                                             .Select(e => new FacultyEducation
                                                             {
                                                                 educationId = e.id,
                                                                 educationName = e.educationCategoryName,
                                                                 studiedEducation = db.jntuh_faculty_education.Where(fe => fe.educationId == e.id && fe.facultyId == fID).Select(fe => fe.courseStudied).FirstOrDefault(),
                                                                 passedYear = db.jntuh_faculty_education.Where(fe => fe.educationId == e.id && fe.facultyId == fID).Select(fe => fe.passedYear).FirstOrDefault(),
                                                                 percentage = db.jntuh_faculty_education.Where(fe => fe.educationId == e.id && fe.facultyId == fID).Select(fe => fe.marksPercentage).FirstOrDefault(),
                                                                 division = db.jntuh_faculty_education.Where(fe => fe.educationId == e.id && fe.facultyId == fID).Select(fe => fe.division).FirstOrDefault(),
                                                                 university = db.jntuh_faculty_education.Where(fe => fe.educationId == e.id && fe.facultyId == fID).Select(fe => fe.boardOrUniversity).FirstOrDefault(),
                                                                 place = db.jntuh_faculty_education.Where(fe => fe.educationId == e.id && fe.facultyId == fID).Select(fe => fe.placeOfEducation).FirstOrDefault(),
                                                             }).ToList();

                foreach (var item in collegeFaculty.FacultyEducation)
                {
                    if (item.division == null)
                        item.division = 0;
                }
            }
            List<FacultySubject> FacultySubject = new List<FacultySubject>();

            if (fID == 0)
            {
                for (int i = 1; i <= 4; i++)
                {
                    FacultySubject subject = new FacultySubject();
                    subject.degreeId = 0;
                    subject.departmentId = 0;
                    subject.specializationId = 0;
                    subject.shiftId = 1;
                    subject.subject = string.Empty;
                    subject.duration = 0;
                    FacultySubject.Add(subject);
                }
            }
            else
            {
                var list = db.jntuh_faculty_subjects.Where(s => s.isActive == true && s.facultyId == fID).ToList();
                foreach (var item in list)
                {
                    int dept = db.jntuh_specialization.Find(item.specializationId).departmentId;
                    int deg = db.jntuh_department.Find(dept).degreeId;

                    //select departmentId from jntuh_specialization where id=15;
                    //select degreeid from jntuh_department where id=35;
                    //select degree from jntuh_degree where id=3
                    var ShiftName = db.jntuh_shift.Find(item.shiftId).shiftName;
                    var SpecializationName = db.jntuh_specialization.Find(item.specializationId).specializationName;
                    var departmentId = db.jntuh_specialization.Find(item.specializationId).departmentId;
                    var departmentName = db.jntuh_department.Find(departmentId).departmentName;
                    var degreeid = db.jntuh_department.Find(departmentId).degreeId;
                    var degreeName = db.jntuh_degree.Find(degreeid).degree;


                    FacultySubject subject = new FacultySubject();
                    subject.degreeId = deg;
                    subject.degreeName = degreeName;
                    subject.departmentId = dept;
                    subject.departmentName = departmentName;
                    subject.specializationId = item.specializationId;
                    subject.specializationName = SpecializationName;
                    subject.shiftId = item.shiftId;
                    subject.shiftName = ShiftName;
                    subject.subject = item.subjectOrLab;
                    subject.duration = item.classDuration;
                    FacultySubject.Add(subject);
                }

                for (int i = 1; i <= 4 - list.Count(); i++)
                {
                    FacultySubject subject = new FacultySubject();
                    subject.degreeId = 0;
                    subject.departmentId = 0;
                    subject.specializationId = 0;
                    subject.shiftId = 1;
                    subject.subject = string.Empty;
                    subject.duration = 0;
                    FacultySubject.Add(subject);
                }
            }

            collegeFaculty.FacultySubject = FacultySubject;
            collegeFaculty.facultyGenderId = null;
            collegeFaculty.isFacultyRatifiedByJNTU = null;
            collegeFaculty.isRelatedToExamBranch = null;
            collegeFaculty.isRelatedToPlacementCell = null;
            collegeFaculty.facultySalary = null;
            collegeFaculty.facultyTypeId = fType;
            collegeFaculty.isFacultyRatifiedByJNTU = false;

            if (fID > 0)
            {
                jntuh_college_faculty faculty = db.jntuh_college_faculty.Find(fID);
                collegeFaculty.id = fID;
                collegeFaculty.collegeId = faculty.collegeId;
                collegeFaculty.facultyTypeId = faculty.facultyTypeId;
                collegeFaculty.facultyFirstName = faculty.facultyFirstName;
                collegeFaculty.facultyLastName = faculty.facultyLastName;
                collegeFaculty.facultySurname = faculty.facultySurname;
                collegeFaculty.facultyGenderId = faculty.facultyGenderId;
                collegeFaculty.facultyFatherName = faculty.facultyFatherName;
                collegeFaculty.facultyCategoryId = faculty.facultyCategoryId;
                collegeFaculty.photo = faculty.facultyPhoto;

                if (faculty.facultyDateOfBirth != null)
                    collegeFaculty.dateOfBirth = Utilities.MMDDYY2DDMMYY(faculty.facultyDateOfBirth.ToString(CultureInfo.InvariantCulture));

                collegeFaculty.facultyDesignationId = faculty.facultyDesignationId;
                collegeFaculty.facultyOtherDesignation = faculty.facultyOtherDesignation;
                collegeFaculty.facultyDepartmentId = faculty.facultyDepartmentId;

                if (faculty.facultyDateOfAppointment != null)
                    collegeFaculty.dateOfAppointment = Utilities.MMDDYY2DDMMYY(faculty.facultyDateOfAppointment.ToString(CultureInfo.InvariantCulture)); // Convert.ToDateTime(faculty.facultyDateOfAppointment).ToString("dd/MM/yyyy").Split(' ')[0];

                if (faculty.facultyDateOfResignation != null)
                    collegeFaculty.dateOfResignation = Utilities.MMDDYY2DDMMYY(faculty.facultyDateOfResignation.ToString()); // Convert.ToDateTime(faculty.facultyDateOfResignation).ToString("dd/MM/yyyy").Split(' ')[0];

                collegeFaculty.isFacultyRatifiedByJNTU = faculty.isFacultyRatifiedByJNTU;

                if (faculty.facultyDateOfRatification != null)
                    collegeFaculty.dateOfRatification = Utilities.MMDDYY2DDMMYY(faculty.facultyDateOfRatification.ToString()); // Convert.ToDateTime(faculty.facultyDateOfRatification).ToString("dd/MM/yyyy").Split(' ')[0];

                collegeFaculty.facultyDurationOfRatification = faculty.facultyDurationOfRatification;
                collegeFaculty.facultySalary = faculty.facultySalary;
                collegeFaculty.facultyPreviousExperience = Convert.ToInt32(faculty.facultyPreviousExperience);
                collegeFaculty.facultyPayScale = faculty.facultyPayScale;
                collegeFaculty.salaryAccountNumber = faculty.salaryAccountNumber;
                collegeFaculty.salaryBankName = faculty.salaryBankName;
                collegeFaculty.salaryBranchName = faculty.salaryBranchName;
                collegeFaculty.facultyRecruitedFor = faculty.facultyRecruitedFor;
                collegeFaculty.grossSalary = faculty.grossSalary;
                collegeFaculty.netSalary = faculty.netSalary;
                collegeFaculty.facultyEmail = faculty.facultyEmail;
                collegeFaculty.facultyMobile = faculty.facultyMobile;
                collegeFaculty.facultyPANNumber = faculty.facultyPANNumber;
                collegeFaculty.facultyAadhaarNumber = faculty.facultyAadhaarNumber;
                collegeFaculty.isRelatedToExamBranch = faculty.isRelatedToExamBranch;
                collegeFaculty.isRelatedToPlacementCell = faculty.isRelatedToPlacementCell;
                collegeFaculty.facultyAchievements1 = faculty.facultyAchievements1;
                collegeFaculty.facultyAchievements2 = faculty.facultyAchievements2;
                collegeFaculty.facultyAchievements3 = faculty.facultyAchievements3;

                ViewBag.FacultyDepartmentName = db.jntuh_department.Where(d => d.id == collegeFaculty.facultyDepartmentId).Select(d => d.departmentName).FirstOrDefault();
                ViewBag.FacultyCategoryName = db.jntuh_faculty_category.Where(c => c.id == collegeFaculty.facultyCategoryId).Select(c => c.facultyCategory).FirstOrDefault();
                ViewBag.FacultyDesignation = db.jntuh_designation.Where(d => d.id == collegeFaculty.facultyDesignationId).Select(d => d.designation).FirstOrDefault();


            }
            collegeFaculty.collegeId = userCollegeID;
            collegeFaculty.dateOfResignation = collegeFaculty.dateOfResignation == "1/1/0001" ? string.Empty : collegeFaculty.dateOfResignation;
            return View(collegeFaculty);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult Create(CollegeFaculty faculty, string fid, string type)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(collegeUser => collegeUser.userID == userID).Select(collegeUser => collegeUser.collegeID).FirstOrDefault();
            if (userCollegeID == 0)
            {
                userCollegeID = faculty.collegeId;
            }
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int ay0 = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();
            var cSpcIds =
               db.jntuh_college_intake_existing.Where(i => i.collegeId == userCollegeID && (i.academicYearId == ay0 - 1 || i.academicYearId == ay0 - 2 || i.academicYearId == ay0 - 3) && (i.approvedIntake != 0 && i.approvedIntake != null))
                //.GroupBy(r => new { r.specializationId })
                   .Select(s => s.specializationId)
                   .ToList();
            var DepartmentsData = db.jntuh_specialization.Where(e => e.isActive == true && cSpcIds.Contains(e.id)).Select(e => e.departmentId).Distinct().ToArray();
            var DegreeIds = db.jntuh_department.Where(e => e.isActive == true && DepartmentsData.Contains(e.id)).Select(e => e.degreeId).Distinct().ToList();
            var degrees = DegreeIds.Join(db.jntuh_degree, degreeId => degreeId, degree => degree.id,
                                                                 (degreeId, degree) => new
                                                                 {
                                                                     degreeId,
                                                                     //collegeDegree.collegeId,
                                                                     //collegeDegree.isActive,
                                                                     degree.degree,
                                                                     degree.degreeDisplayOrder
                                                                 })
                                                             .Select(collegeDegree => new
                                                             {
                                                                 collegeDegree.degreeId,
                                                                 collegeDegree.degree,
                                                                 collegeDegree.degreeDisplayOrder
                                                             }).OrderBy(d => d.degreeDisplayOrder).ToList();
            ViewBag.degree = degrees;
            //ViewBag.degree = db.jntuh_college_degree.Join(db.jntuh_degree, collegeDegree => collegeDegree.degreeId, degree => degree.id,
            //                                                     (collegeDegree, degree) => new
            //                                                     {
            //                                                         collegeDegree.degreeId,
            //                                                         collegeDegree.collegeId,
            //                                                         collegeDegree.isActive,
            //                                                         degree.degree
            //                                                     })
            //                                                 .Where(collegeDegree => collegeDegree.collegeId == userCollegeID)
            //                                                 .Select(collegeDegree => new
            //                                                 {
            //                                                     collegeDegree.degreeId,
            //                                                     collegeDegree.degree
            //                                                 }).OrderBy(d => d.degree).ToList();
            ViewBag.shift = db.jntuh_shift.Where(s => s.isActive == true).OrderBy(s => s.shiftName).ToList();
            List<DistinctDepartment> depts = new List<DistinctDepartment>();
            string existingDepts = string.Empty;
            var desigIds = new int[] { 1, 2, 3, 4 };
            foreach (var item in db.jntuh_department.Where(s => s.isActive == true && DepartmentsData.Contains(s.id)).OrderBy(s => s.departmentName))
            {
                if (!existingDepts.Contains(item.departmentName))
                {
                    depts.Add(new DistinctDepartment { id = item.id, departmentName = item.departmentName });
                    existingDepts = existingDepts + "," + item.departmentName;
                }
            }

            ViewBag.department = depts;
            ViewBag.category = db.jntuh_faculty_category.Where(c => c.isActive == true).ToList();
            ViewBag.designation = db.jntuh_designation.Where(c => c.isActive == true && desigIds.Contains(c.id)).ToList();

            List<SelectListItem> ratifiedDuration = new List<SelectListItem>();
            for (int i = 1; i <= 10; i++)
            {
                ratifiedDuration.Add(new SelectListItem { Text = i.ToString(), Value = i.ToString() });
            }
            ViewBag.duration = ratifiedDuration;

            List<SelectListItem> prevExperience = new List<SelectListItem>();
            for (int i = 0; i <= 100; i++)
            {
                prevExperience.Add(new SelectListItem { Text = i.ToString(), Value = i.ToString() });
            }
            ViewBag.prevExperience = prevExperience;

            List<SelectListItem> years = new List<SelectListItem>();
            for (int i = 1940; i <= DateTime.Now.Year; i++)
            {
                years.Add(new SelectListItem { Text = i.ToString(), Value = i.ToString() });
            }
            ViewBag.years = years;

            List<SelectListItem> division = new List<SelectListItem>();
            for (int i = 1; i <= 5; i++)
            {
                division.Add(new SelectListItem { Text = i.ToString(), Value = i.ToString() });
            }
            ViewBag.division = division;

            int fID = 0;
            int fType = 0;

            if (fid != null)
            {
                fID = Convert.ToInt32(Utilities.DecryptString(fid, WebConfigurationManager.AppSettings["CryptoKey"]));
            }

            if (type != null)
            {
                fType = Convert.ToInt32(Utilities.DecryptString(type, WebConfigurationManager.AppSettings["CryptoKey"]));
            }

            ViewBag.FacultyID = fID;

            //faculty.id = fID;
            //faculty.dateOfBirth = Utilities.MMDDYY2DDMMYY(faculty.dateOfBirth);
            //faculty.dateOfAppointment = Utilities.MMDDYY2DDMMYY(faculty.dateOfAppointment);
            //if (faculty.dateOfResignation != null)
            //    faculty.dateOfResignation = Utilities.MMDDYY2DDMMYY(faculty.dateOfResignation);
            //if (faculty.dateOfRatification != null)
            //    faculty.dateOfRatification = Utilities.MMDDYY2DDMMYY(faculty.dateOfRatification);

            //if (ModelState.IsValid)
            //{
            jntuh_college_faculty collegeFaculty = new jntuh_college_faculty();
            collegeFaculty.collegeId = userCollegeID;
            if (fType != 0)
            {
                collegeFaculty.facultyTypeId = fType;
            }
            else
            {
                collegeFaculty.facultyTypeId = faculty.facultyTypeId;
            }

            ViewBag.FacultyType = collegeFaculty.facultyTypeId;

            collegeFaculty.facultyFirstName = faculty.facultyFirstName;
            collegeFaculty.facultyLastName = faculty.facultyLastName;
            collegeFaculty.facultySurname = faculty.facultySurname;
            collegeFaculty.facultyGenderId = faculty.facultyGenderId == null ? 0 : (int)faculty.facultyGenderId;
            collegeFaculty.facultyFatherName = faculty.facultyFatherName;
            collegeFaculty.facultyCategoryId = faculty.facultyCategoryId;
            collegeFaculty.facultyDateOfBirth =
                Utilities.DDMMYY2MMDDYY(faculty.dateOfBirth.ToString(CultureInfo.InvariantCulture));// Convert.ToDateTime(faculty.dateOfBirth);
            collegeFaculty.facultyDesignationId = (int)faculty.facultyDesignationId;
            collegeFaculty.facultyOtherDesignation = faculty.facultyOtherDesignation;
            collegeFaculty.facultyDepartmentId = faculty.facultyDepartmentId;
            collegeFaculty.facultyDateOfAppointment = Utilities.DDMMYY2MMDDYY(faculty.dateOfAppointment.ToString(CultureInfo.InvariantCulture)); //Convert.ToDateTime(faculty.dateOfAppointment);
            //collegeFaculty.facultyDateOfResignation = Utilities.DDMMYY2MMDDYY(faculty.dateOfResignation.ToString(CultureInfo.InvariantCulture)); //Convert.ToDateTime(faculty.dateOfResignation);
            collegeFaculty.isFacultyRatifiedByJNTU = faculty.isFacultyRatifiedByJNTU == null ? false : (bool)faculty.isFacultyRatifiedByJNTU;
            //collegeFaculty.facultyDateOfRatification = Utilities.DDMMYY2MMDDYY(faculty.dateOfRatification.ToString(CultureInfo.InvariantCulture)); //Convert.ToDateTime(faculty.dateOfRatification);
            collegeFaculty.facultyDurationOfRatification = faculty.facultyDurationOfRatification;
            collegeFaculty.facultySalary = faculty.facultySalary == null ? "0" : faculty.facultySalary;
            collegeFaculty.facultyPreviousExperience = faculty.facultyPreviousExperience;
            collegeFaculty.facultyPayScale = faculty.facultyPayScale;
            collegeFaculty.salaryAccountNumber = faculty.salaryAccountNumber;
            collegeFaculty.salaryBankName = faculty.salaryBankName;
            collegeFaculty.salaryBranchName = faculty.salaryBranchName;
            collegeFaculty.facultyRecruitedFor = faculty.facultyRecruitedFor;
            collegeFaculty.grossSalary = faculty.grossSalary;
            collegeFaculty.netSalary = faculty.netSalary;
            collegeFaculty.facultyEmail = faculty.facultyEmail;
            collegeFaculty.facultyMobile = faculty.facultyMobile;
            collegeFaculty.facultyPANNumber = faculty.facultyPANNumber;
            collegeFaculty.facultyAadhaarNumber = faculty.facultyAadhaarNumber;
            collegeFaculty.isRelatedToExamBranch = faculty.isRelatedToExamBranch == null ? false : (bool)faculty.isRelatedToExamBranch;
            collegeFaculty.isRelatedToPlacementCell = faculty.isRelatedToPlacementCell == null ? false : (bool)faculty.isRelatedToPlacementCell;
            collegeFaculty.facultyAchievements1 = faculty.facultyAchievements1;
            collegeFaculty.facultyAchievements2 = faculty.facultyAchievements2;
            collegeFaculty.facultyAchievements3 = faculty.facultyAchievements3;

            int count = 0;
            if (faculty.FacultySubject != null)
            {
                foreach (var item in faculty.FacultySubject)
                {
                    if (item.degreeId != null && item.departmentId != null && item.specializationId != null && item.shiftId != null)
                        count += 1;
                }
            }
            if ((collegeFaculty.facultyTypeId == 1 || collegeFaculty.facultyTypeId == 2) && count == 0)
            {
                faculty.dateOfBirth = Utilities.MMDDYY2DDMMYY(faculty.dateOfBirth);
                faculty.dateOfAppointment = Utilities.MMDDYY2DDMMYY(faculty.dateOfAppointment);
                if (faculty.dateOfResignation != null)
                    faculty.dateOfResignation = Utilities.MMDDYY2DDMMYY(faculty.dateOfResignation);

                if (collegeFaculty.facultyTypeId == 1)
                    TempData["Error"] = "Specify atleast one subject beging taught by the faculty";
                if (collegeFaculty.facultyTypeId == 2)
                    TempData["Error"] = "Specify atleast one lab being conduct by the faculty";

                foreach (var item in faculty.FacultyEducation)
                {
                    item.division = 0;
                    item.passedYear = 0;
                }

                foreach (var subject in faculty.FacultySubject)
                {
                    subject.degreeId = 0;
                    subject.departmentId = 0;
                    subject.specializationId = 0;
                    subject.shiftId = 1;
                    subject.subject = string.Empty;
                    subject.duration = 0;
                }
                return View(faculty);
            }

            if (faculty.facultyPhoto != null)
            {
                if (!Directory.Exists(Server.MapPath("~/Content/Upload/Photos")))
                {
                    Directory.CreateDirectory(Server.MapPath("~/Content/Upload/Photos"));
                }

                var ext = Path.GetExtension(faculty.facultyPhoto.FileName);

                if (ext.ToUpper().Equals(".GIF") || ext.ToUpper().Equals(".BMP") || ext.ToUpper().Equals(".JPEG") || ext.ToUpper().Equals(".JPG") || ext.ToUpper().Equals(".PNG"))
                {
                    string fileName = db.jntuh_college.Where(c => c.id == userCollegeID).Select(c => c.collegeCode).FirstOrDefault() + "-" +
                                      db.jntuh_faculty_type.Where(f => f.id == collegeFaculty.facultyTypeId).Select(f => f.facultyType).FirstOrDefault() + "-" +
                                      faculty.facultyFirstName + "-" + faculty.facultySurname;
                    faculty.facultyPhoto.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath("~/Content/Upload/Photos"), fileName, ext));
                    collegeFaculty.facultyPhoto = string.Format("{0}/{1}{2}", "~/Content/Upload/Photos", fileName, ext);
                }
            }
            else if (faculty.photo != null)
            {
                collegeFaculty.facultyPhoto = faculty.photo;
            }

            int facultyId = db.jntuh_college_faculty.AsNoTracking()
                              .Where(f => f.collegeId == userCollegeID && f.facultyTypeId == faculty.facultyTypeId && f.id == faculty.id)
                              .Select(f => f.id).FirstOrDefault();

            if (facultyId == 0)
            {
                collegeFaculty.createdBy = userID;
                collegeFaculty.createdOn = DateTime.Now;
                db.jntuh_college_faculty.Add(collegeFaculty);
                db.SaveChanges();
            }
            else
            {
                collegeFaculty.id = facultyId;
                collegeFaculty.createdBy = db.jntuh_college_faculty.Where(f => f.id == facultyId).Select(f => f.createdBy).FirstOrDefault();
                collegeFaculty.createdOn = db.jntuh_college_faculty.Where(f => f.id == facultyId).Select(f => f.createdOn).FirstOrDefault();
                collegeFaculty.updatedBy = userID;
                collegeFaculty.updatedOn = DateTime.Now;
                db.Entry(collegeFaculty).State = EntityState.Modified;
                db.SaveChanges();
            }

            if (collegeFaculty.id > 0)
            {
                List<jntuh_faculty_education> jntuh_faculty_education = db.jntuh_faculty_education.Where(f => f.facultyId == collegeFaculty.id).ToList();
                foreach (var item in jntuh_faculty_education)
                {
                    db.jntuh_faculty_education.Remove(item);
                    db.SaveChanges();
                }

                foreach (var item in faculty.FacultyEducation)
                {
                    if (item.studiedEducation != null)
                    {
                        jntuh_faculty_education education = new jntuh_faculty_education();
                        education.facultyId = collegeFaculty.id;
                        education.educationId = item.educationId;
                        education.courseStudied = item.studiedEducation;
                        education.passedYear = item.passedYear == null ? 0 : (int)item.passedYear;
                        education.marksPercentage = item.percentage == null ? 0 : (decimal)item.percentage;
                        education.division = item.division == null ? 0 : (int)item.division;
                        education.boardOrUniversity = item.university == null ? string.Empty : item.university;
                        education.placeOfEducation = item.place;

                        int eid = db.jntuh_faculty_education.AsNoTracking().Where(e => e.educationId == item.educationId && e.facultyId == collegeFaculty.id).Select(e => e.id).FirstOrDefault();

                        if (eid == 0)
                        {
                            education.createdBy = userID;
                            education.createdOn = DateTime.Now;
                            db.jntuh_faculty_education.Add(education);
                            db.SaveChanges();
                        }
                        else
                        {
                            education.id = eid;
                            education.createdBy = db.jntuh_faculty_education.Where(e => e.id == eid).Select(e => e.createdBy).FirstOrDefault();
                            education.createdOn = db.jntuh_faculty_education.Where(e => e.id == eid).Select(e => e.createdOn).FirstOrDefault();
                            education.updatedBy = userID;
                            education.updatedOn = DateTime.Now;
                            db.Entry(education).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                    }
                }

                if (faculty.FacultySubject != null)
                {
                    List<jntuh_faculty_subjects> jntuh_faculty_subjects = db.jntuh_faculty_subjects.Where(f => f.facultyId == collegeFaculty.id).ToList();
                    foreach (var item in jntuh_faculty_subjects)
                    {
                        db.jntuh_faculty_subjects.Remove(item);
                        db.SaveChanges();
                    }

                    foreach (var item in faculty.FacultySubject)
                    {
                        if (item.degreeId != null)
                        {
                            jntuh_faculty_subjects subject = new jntuh_faculty_subjects();
                            subject.facultyId = collegeFaculty.id;
                            subject.specializationId = item.specializationId == null ? 0 : (int)item.specializationId;
                            subject.shiftId = item.shiftId;
                            subject.subjectOrLab = item.subject == null ? string.Empty : item.subject;
                            subject.classDuration = item.duration == null ? 0 : (int)item.duration;
                            subject.isActive = true;

                            int sid = db.jntuh_faculty_subjects.AsNoTracking().Where(s => s.specializationId == item.specializationId && s.shiftId == item.shiftId && s.facultyId == collegeFaculty.id && s.subjectOrLab == item.subject).Select(f => f.id).FirstOrDefault();

                            if (sid == 0)
                            {
                                subject.createdBy = userID;
                                subject.createdOn = DateTime.Now;
                                db.jntuh_faculty_subjects.Add(subject);
                                db.SaveChanges();
                            }
                            //else
                            //{
                            //    subject.id = sid;
                            //    subject.createdBy = db.jntuh_faculty_subjects.Where(e => e.id == sid).Select(e => e.createdBy).FirstOrDefault();
                            //    subject.createdOn = db.jntuh_faculty_subjects.Where(e => e.id == sid).Select(e => e.createdOn).FirstOrDefault();
                            //    subject.updatedBy = userID;
                            //    subject.updatedOn = DateTime.Now;
                            //    db.Entry(subject).State = EntityState.Modified;
                            //    db.SaveChanges();
                            //}
                        }
                    }
                }
            }

            faculty.dateOfBirth = Utilities.MMDDYY2DDMMYY(faculty.dateOfBirth);
            faculty.dateOfAppointment = Utilities.MMDDYY2DDMMYY(faculty.dateOfAppointment);
            if (faculty.dateOfResignation != null)
                faculty.dateOfResignation = Utilities.MMDDYY2DDMMYY(faculty.dateOfResignation);

            string rtnAction = string.Empty;
            if (collegeFaculty.facultyTypeId == 1)
                rtnAction = "Teaching";
            else if (collegeFaculty.facultyTypeId == 2)
                rtnAction = "Technical";
            else if (collegeFaculty.facultyTypeId == 3)
                rtnAction = "NonTeaching";

            return RedirectToAction(rtnAction, "Faculty", new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
        }

        [HttpPost]
        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult Delete(string fid, string type)
        {
            int fID = 0;
            int fType = 0;

            if (fid != null)
            {
                fID = Convert.ToInt32(Utilities.DecryptString(fid, WebConfigurationManager.AppSettings["CryptoKey"]));
            }

            if (type != null)
            {
                fType = Convert.ToInt32(Utilities.DecryptString(type, WebConfigurationManager.AppSettings["CryptoKey"]));
            }

            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 0)
            {
                userCollegeID = db.jntuh_college_faculty.Where(f => f.id == fID && f.facultyTypeId == fType).Select(f => f.collegeId).FirstOrDefault();
            }
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            var list = db.jntuh_college_faculty.Where(f => f.id == fID && f.collegeId == userCollegeID && f.facultyTypeId == fType).Select(f => f).FirstOrDefault();
            if (list != null)
            {
                jntuh_college_faculty rowToDelete = new jntuh_college_faculty();
                rowToDelete = db.jntuh_college_faculty.Where(i => i.id == list.id).Select(i => i).FirstOrDefault();
                db.jntuh_college_faculty.Remove(rowToDelete);
                db.SaveChanges();
                TempData["success"] = "deleted successfully";
            }

            string rtnAction = string.Empty;
            if (fType == 1)
                rtnAction = "Teaching";
            else if (fType == 2)
                rtnAction = "Technical";
            else if (fType == 3)
                rtnAction = "NonTeaching";

            return RedirectToAction(rtnAction, "Faculty", new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
        }

        [Authorize(Roles = "Admin,College")]
        public ActionResult View(string fid, string type, string id)
        {
            TempData["success"] = null;
            TempData["Error"] = null;

            int fID = 0;
            int fType = 0;

            if (fid != null)
            {
                fID = Convert.ToInt32(Utilities.DecryptString(fid, WebConfigurationManager.AppSettings["CryptoKey"]));
            }

            if (type != null)
            {
                fType = Convert.ToInt32(Utilities.DecryptString(type, WebConfigurationManager.AppSettings["CryptoKey"]));

                ViewBag.FacultyType = fType;
            }

            ViewBag.Type = type;
            ViewBag.Id = fid;

            ViewBag.FacultyType = fType;
            ViewBag.FacultyID = fID;

            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(collegeUser => collegeUser.userID == userID).Select(collegeUser => collegeUser.collegeID).FirstOrDefault();
            if (userCollegeID == 0)
            {
                // userCollegeID  = Convert.ToInt32(Utilities.DecryptString(id, WebConfigurationManager.AppSettings["CryptoKey"]));   
                userCollegeID = db.jntuh_college_faculty.Where(f => f.id == fID && f.facultyTypeId == fType).Select(f => f.collegeId).FirstOrDefault();
            }
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            DateTime todayDate = DateTime.Now.Date;
            int status = db.jntuh_college_edit_status.Where(editStatus => editStatus.collegeId == userCollegeID &&
                                                                          editStatus.IsCollegeEditable == true &&
                                                                          editStatus.editFromDate <= todayDate &&
                                                                          editStatus.editToDate >= todayDate)
                                                     .Select(editStatus => editStatus.id)
                                                     .FirstOrDefault();
            if (status > 0 && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("Create");
            }

            //var degrees = db.jntuh_college_degree.Join(db.jntuh_degree, collegeDegree => collegeDegree.degreeId, degree => degree.id,
            //                                                     (collegeDegree, degree) => new
            //                                                     {
            //                                                         collegeDegree.degreeId,
            //                                                         collegeDegree.collegeId,
            //                                                         collegeDegree.isActive,
            //                                                         degree.degree,
            //                                                         degree.degreeDisplayOrder
            //                                                     })
            //                                                 .Where(collegeDegree => collegeDegree.collegeId == userCollegeID)
            //                                                 .Select(collegeDegree => new
            //                                                 {
            //                                                     collegeDegree.degreeId,
            //                                                     collegeDegree.degree,
            //                                                     collegeDegree.degreeDisplayOrder
            //                                                 }).OrderBy(d => d.degreeDisplayOrder).ToList();
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int ay0 = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();
            var cSpcIds =
               db.jntuh_college_intake_existing.Where(i => i.collegeId == userCollegeID && (i.academicYearId == ay0 - 1 || i.academicYearId == ay0 - 2 || i.academicYearId == ay0 - 3) && (i.approvedIntake != 0 && i.approvedIntake != null))
                //.GroupBy(r => new { r.specializationId })
                   .Select(s => s.specializationId)
                   .ToList();

            var DepartmentsData = db.jntuh_specialization.Where(e => e.isActive == true && cSpcIds.Contains(e.id)).Select(e => e.departmentId).Distinct().ToArray();

            var DegreeIds = db.jntuh_department.Where(e => e.isActive == true && DepartmentsData.Contains(e.id)).Select(e => e.degreeId).Distinct().ToList();

            var degrees = DegreeIds.Join(db.jntuh_degree, degreeId => degreeId, degree => degree.id,
                                                                 (degreeId, degree) => new
                                                                 {
                                                                     degreeId,
                                                                     //collegeDegree.collegeId,
                                                                     //collegeDegree.isActive,
                                                                     degree.degree,
                                                                     degree.degreeDisplayOrder
                                                                 })
                                                             .Select(collegeDegree => new
                                                             {
                                                                 collegeDegree.degreeId,
                                                                 collegeDegree.degree,
                                                                 collegeDegree.degreeDisplayOrder
                                                             }).OrderBy(d => d.degreeDisplayOrder).ToList();
            ViewBag.degree = degrees;
            ViewBag.DegreeCount = degrees.Count();
            ViewBag.shift = db.jntuh_shift.Where(s => s.isActive == true).OrderBy(s => s.shiftName).ToList();
            List<DistinctDepartment> depts = new List<DistinctDepartment>();
            string existingDepts = string.Empty;
            var desigIds = new int[] { 1, 2, 3, 4 };
            foreach (var item in db.jntuh_department.Where(s => s.isActive == true && DepartmentsData.Contains(s.id)).OrderBy(s => s.departmentName))
            {
                if (!existingDepts.Split(',').Contains(item.departmentName))
                {
                    depts.Add(new DistinctDepartment { id = item.id, departmentName = item.departmentName });
                    existingDepts = existingDepts + "," + item.departmentName;
                }
            }

            ViewBag.department = depts;
            ViewBag.category = db.jntuh_faculty_category.Where(c => c.isActive == true).ToList();
            ViewBag.designation = db.jntuh_designation.Where(c => c.isActive == true && desigIds.Contains(c.id)).ToList();

            List<SelectListItem> ratifiedDuration = new List<SelectListItem>();
            for (int i = 1; i <= 10; i++)
            {
                ratifiedDuration.Add(new SelectListItem { Text = i.ToString(), Value = i.ToString() });
            }
            ViewBag.duration = ratifiedDuration;

            List<SelectListItem> prevExperience = new List<SelectListItem>();
            for (int i = 0; i <= 100; i++)
            {
                prevExperience.Add(new SelectListItem { Text = i.ToString(), Value = i.ToString() });
            }
            ViewBag.prevExperience = prevExperience;

            List<SelectListItem> years = new List<SelectListItem>();
            for (int i = 1940; i <= DateTime.Now.Year; i++)
            {
                years.Add(new SelectListItem { Text = i.ToString(), Value = i.ToString() });
            }
            ViewBag.years = years;

            List<SelectListItem> division = new List<SelectListItem>();
            for (int i = 1; i <= 5; i++)
            {
                division.Add(new SelectListItem { Text = i.ToString(), Value = i.ToString() });
            }
            ViewBag.division = division;

            CollegeFaculty collegeFaculty = new CollegeFaculty();
            var edcatIds = new int[] { 1, 2, 3, 4 };
            if (fID == 0)
            {
                collegeFaculty.FacultyEducation = db.jntuh_education_category.Where(e => e.isActive == true && edcatIds.Contains(e.id))
                                                            .Select(e => new FacultyEducation
                                                            {
                                                                educationId = e.id,
                                                                educationName = e.educationCategoryName,
                                                                studiedEducation = string.Empty,
                                                                passedYear = 0,
                                                                percentage = 0,
                                                                division = 0,
                                                                university = string.Empty,
                                                                place = string.Empty,
                                                            }).ToList();
            }
            else
            {
                collegeFaculty.FacultyEducation = db.jntuh_education_category.Where(e => e.isActive == true && edcatIds.Contains(e.id))
                                                             .Select(e => new FacultyEducation
                                                             {
                                                                 educationId = e.id,
                                                                 educationName = e.educationCategoryName,
                                                                 studiedEducation = db.jntuh_faculty_education.Where(fe => fe.educationId == e.id && fe.facultyId == fID).Select(fe => fe.courseStudied).FirstOrDefault(),
                                                                 passedYear = db.jntuh_faculty_education.Where(fe => fe.educationId == e.id && fe.facultyId == fID).Select(fe => fe.passedYear).FirstOrDefault(),
                                                                 percentage = db.jntuh_faculty_education.Where(fe => fe.educationId == e.id && fe.facultyId == fID).Select(fe => fe.marksPercentage).FirstOrDefault(),
                                                                 division = db.jntuh_faculty_education.Where(fe => fe.educationId == e.id && fe.facultyId == fID).Select(fe => fe.division).FirstOrDefault(),
                                                                 university = db.jntuh_faculty_education.Where(fe => fe.educationId == e.id && fe.facultyId == fID).Select(fe => fe.boardOrUniversity).FirstOrDefault(),
                                                                 place = db.jntuh_faculty_education.Where(fe => fe.educationId == e.id && fe.facultyId == fID).Select(fe => fe.placeOfEducation).FirstOrDefault(),
                                                             }).ToList();

                foreach (var item in collegeFaculty.FacultyEducation)
                {
                    if (item.division == null)
                        item.division = 0;
                }
            }
            List<FacultySubject> FacultySubject = new List<FacultySubject>();

            if (fID == 0)
            {
                for (int i = 1; i <= 4; i++)
                {
                    FacultySubject subject = new FacultySubject();
                    subject.degreeId = 0;
                    subject.departmentId = 0;
                    subject.specializationId = 0;
                    subject.shiftId = 1;
                    subject.subject = string.Empty;
                    subject.duration = 0;
                    FacultySubject.Add(subject);
                }
            }
            else
            {
                var list = db.jntuh_faculty_subjects.Where(s => s.isActive == true && s.facultyId == fID).ToList();
                foreach (var item in list)
                {
                    int dept = db.jntuh_specialization.Find(item.specializationId).departmentId;
                    int deg = db.jntuh_department.Find(dept).degreeId;

                    //select departmentId from jntuh_specialization where id=15;
                    //select degreeid from jntuh_department where id=35;
                    //select degree from jntuh_degree where id=3
                    var ShiftName = db.jntuh_shift.Find(item.shiftId).shiftName;
                    var SpecializationName = db.jntuh_specialization.Find(item.specializationId).specializationName;
                    var departmentId = db.jntuh_specialization.Find(item.specializationId).departmentId;
                    var departmentName = db.jntuh_department.Find(departmentId).departmentName;
                    var degreeid = db.jntuh_department.Find(departmentId).degreeId;
                    var degreeName = db.jntuh_degree.Find(degreeid).degree;


                    FacultySubject subject = new FacultySubject();
                    subject.degreeId = deg;
                    subject.degreeName = degreeName;
                    subject.departmentId = dept;
                    subject.departmentName = departmentName;
                    subject.specializationId = item.specializationId;
                    subject.specializationName = SpecializationName;
                    subject.shiftId = item.shiftId;
                    subject.shiftName = ShiftName;
                    subject.subject = item.subjectOrLab;
                    subject.duration = item.classDuration;
                    FacultySubject.Add(subject);
                }

                for (int i = 1; i <= 4 - list.Count(); i++)
                {
                    FacultySubject subject = new FacultySubject();
                    subject.degreeId = 0;
                    subject.departmentId = 0;
                    subject.specializationId = 0;
                    subject.shiftId = 1;
                    subject.subject = string.Empty;
                    subject.duration = 0;
                    FacultySubject.Add(subject);
                }
            }

            collegeFaculty.FacultySubject = FacultySubject;
            collegeFaculty.facultyGenderId = null;
            collegeFaculty.isFacultyRatifiedByJNTU = null;
            collegeFaculty.isRelatedToExamBranch = null;
            collegeFaculty.isRelatedToPlacementCell = null;
            collegeFaculty.facultySalary = null;
            collegeFaculty.facultyTypeId = fType;
            collegeFaculty.isFacultyRatifiedByJNTU = false;

            if (fID > 0)
            {
                jntuh_college_faculty faculty = db.jntuh_college_faculty.Find(fID);
                collegeFaculty.collegeId = faculty.collegeId;
                collegeFaculty.facultyTypeId = faculty.facultyTypeId;
                collegeFaculty.facultyFirstName = faculty.facultyFirstName;
                collegeFaculty.facultyLastName = faculty.facultyLastName;
                collegeFaculty.facultySurname = faculty.facultySurname;
                collegeFaculty.facultyGenderId = faculty.facultyGenderId;
                collegeFaculty.facultyFatherName = faculty.facultyFatherName;
                collegeFaculty.facultyCategoryId = faculty.facultyCategoryId;
                collegeFaculty.photo = faculty.facultyPhoto;

                if (faculty.facultyDateOfBirth != null)
                    collegeFaculty.dateOfBirth = Utilities.MMDDYY2DDMMYY(faculty.facultyDateOfBirth.ToString(CultureInfo.InvariantCulture)); // Convert.ToDateTime(faculty.facultyDateOfBirth).ToString("dd/MM/yyyy").Split(' ')[0];

                collegeFaculty.facultyDesignationId = faculty.facultyDesignationId;
                collegeFaculty.facultyOtherDesignation = faculty.facultyOtherDesignation;
                collegeFaculty.facultyDepartmentId = faculty.facultyDepartmentId;

                if (faculty.facultyDateOfAppointment != null)
                    collegeFaculty.dateOfAppointment = Utilities.MMDDYY2DDMMYY(faculty.facultyDateOfAppointment.ToString(CultureInfo.InvariantCulture)); // Convert.ToDateTime(faculty.facultyDateOfAppointment).ToString("dd/MM/yyyy").Split(' ')[0];

                if (faculty.facultyDateOfResignation != null)
                    collegeFaculty.dateOfResignation = Utilities.MMDDYY2DDMMYY(faculty.facultyDateOfResignation.ToString()); // Convert.ToDateTime(faculty.facultyDateOfResignation).ToString("dd/MM/yyyy").Split(' ')[0];

                collegeFaculty.isFacultyRatifiedByJNTU = faculty.isFacultyRatifiedByJNTU;

                if (faculty.facultyDateOfRatification != null)
                    collegeFaculty.dateOfRatification = Utilities.MMDDYY2DDMMYY(faculty.facultyDateOfRatification.ToString()); // Convert.ToDateTime(faculty.facultyDateOfRatification).ToString("dd/MM/yyyy").Split(' ')[0];

                collegeFaculty.facultyDurationOfRatification = faculty.facultyDurationOfRatification;
                collegeFaculty.facultySalary = faculty.facultySalary;
                collegeFaculty.facultyPreviousExperience = Convert.ToInt32(faculty.facultyPreviousExperience);
                collegeFaculty.facultyPayScale = faculty.facultyPayScale;
                collegeFaculty.salaryAccountNumber = faculty.salaryAccountNumber;
                collegeFaculty.salaryBankName = faculty.salaryBankName;
                collegeFaculty.salaryBranchName = faculty.salaryBranchName;
                collegeFaculty.facultyRecruitedFor = faculty.facultyRecruitedFor;
                collegeFaculty.grossSalary = faculty.grossSalary;
                collegeFaculty.netSalary = faculty.netSalary;
                collegeFaculty.facultyEmail = faculty.facultyEmail;
                collegeFaculty.facultyMobile = faculty.facultyMobile;
                collegeFaculty.facultyPANNumber = faculty.facultyPANNumber;
                collegeFaculty.facultyAadhaarNumber = faculty.facultyAadhaarNumber;
                collegeFaculty.isRelatedToExamBranch = faculty.isRelatedToExamBranch;
                collegeFaculty.isRelatedToPlacementCell = faculty.isRelatedToPlacementCell;
                collegeFaculty.facultyAchievements1 = faculty.facultyAchievements1;
                collegeFaculty.facultyAchievements2 = faculty.facultyAchievements2;
                collegeFaculty.facultyAchievements3 = faculty.facultyAchievements3;

                ViewBag.FacultyDepartmentName = db.jntuh_department.Where(d => d.id == collegeFaculty.facultyDepartmentId).Select(d => d.departmentName).FirstOrDefault();
                ViewBag.FacultyCategoryName = db.jntuh_faculty_category.Where(c => c.id == collegeFaculty.facultyCategoryId).Select(c => c.facultyCategory).FirstOrDefault();
                ViewBag.FacultyDesignation = db.jntuh_designation.Where(d => d.id == collegeFaculty.facultyDesignationId).Select(d => d.designation).FirstOrDefault();


            }
            collegeFaculty.dateOfResignation = collegeFaculty.dateOfResignation == "1/1/0001" ? string.Empty : collegeFaculty.dateOfResignation;
            return View("View", collegeFaculty);
        }

        public ActionResult UserView(string fid, string type, string id)
        {
            TempData["success"] = null;
            TempData["Error"] = null;

            int fID = 0;
            int fType = 0;

            if (fid != null)
            {
                fID = Convert.ToInt32(Utilities.DecryptString(fid, WebConfigurationManager.AppSettings["CryptoKey"]));
            }

            if (type != null)
            {
                fType = Convert.ToInt32(Utilities.DecryptString(type, WebConfigurationManager.AppSettings["CryptoKey"]));

                ViewBag.FacultyType = fType;
            }

            ViewBag.Type = type;
            ViewBag.Id = fid;

            ViewBag.FacultyType = fType;
            ViewBag.FacultyID = fID;

            int userCollegeID = db.jntuh_college_faculty.Where(f => f.id == fID && f.facultyTypeId == fType).Select(f => f.collegeId).FirstOrDefault();
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            CollegeFaculty collegeFaculty = new CollegeFaculty();
            var edcatIds = new int[] { 1, 2, 3, 4 };
            if (fID == 0)
            {
                collegeFaculty.FacultyEducation = db.jntuh_education_category.Where(e => e.isActive == true && edcatIds.Contains(e.id))
                                                            .Select(e => new FacultyEducation
                                                            {
                                                                educationId = e.id,
                                                                educationName = e.educationCategoryName,
                                                                studiedEducation = string.Empty,
                                                                passedYear = 0,
                                                                percentage = 0,
                                                                division = 0,
                                                                university = string.Empty,
                                                                place = string.Empty,
                                                            }).ToList();
            }
            else
            {
                collegeFaculty.FacultyEducation = db.jntuh_education_category.Where(e => e.isActive == true && edcatIds.Contains(e.id))
                                                             .Select(e => new FacultyEducation
                                                             {
                                                                 educationId = e.id,
                                                                 educationName = e.educationCategoryName,
                                                                 studiedEducation = db.jntuh_faculty_education.Where(fe => fe.educationId == e.id && fe.facultyId == fID).Select(fe => fe.courseStudied).FirstOrDefault(),
                                                                 passedYear = db.jntuh_faculty_education.Where(fe => fe.educationId == e.id && fe.facultyId == fID).Select(fe => fe.passedYear).FirstOrDefault(),
                                                                 percentage = db.jntuh_faculty_education.Where(fe => fe.educationId == e.id && fe.facultyId == fID).Select(fe => fe.marksPercentage).FirstOrDefault(),
                                                                 division = db.jntuh_faculty_education.Where(fe => fe.educationId == e.id && fe.facultyId == fID).Select(fe => fe.division).FirstOrDefault(),
                                                                 university = db.jntuh_faculty_education.Where(fe => fe.educationId == e.id && fe.facultyId == fID).Select(fe => fe.boardOrUniversity).FirstOrDefault(),
                                                                 place = db.jntuh_faculty_education.Where(fe => fe.educationId == e.id && fe.facultyId == fID).Select(fe => fe.placeOfEducation).FirstOrDefault(),
                                                             }).ToList();

                foreach (var item in collegeFaculty.FacultyEducation)
                {
                    if (item.division == null)
                        item.division = 0;
                }
            }
            List<FacultySubject> FacultySubject = new List<FacultySubject>();

            if (fID == 0)
            {
                for (int i = 1; i <= 4; i++)
                {
                    FacultySubject subject = new FacultySubject();
                    subject.degreeId = 0;
                    subject.departmentId = 0;
                    subject.specializationId = 0;
                    subject.shiftId = 1;
                    subject.subject = string.Empty;
                    subject.duration = 0;
                    FacultySubject.Add(subject);
                }
            }
            else
            {
                var list = db.jntuh_faculty_subjects.Where(s => s.isActive == true && s.facultyId == fID).ToList();
                foreach (var item in list)
                {
                    int dept = db.jntuh_specialization.Find(item.specializationId).departmentId;
                    int deg = db.jntuh_department.Find(dept).degreeId;

                    //select departmentId from jntuh_specialization where id=15;
                    //select degreeid from jntuh_department where id=35;
                    //select degree from jntuh_degree where id=3
                    var ShiftName = db.jntuh_shift.Find(item.shiftId).shiftName;
                    var SpecializationName = db.jntuh_specialization.Find(item.specializationId).specializationName;
                    var departmentId = db.jntuh_specialization.Find(item.specializationId).departmentId;
                    var departmentName = db.jntuh_department.Find(departmentId).departmentName;
                    var degreeid = db.jntuh_department.Find(departmentId).degreeId;
                    var degreeName = db.jntuh_degree.Find(degreeid).degree;


                    FacultySubject subject = new FacultySubject();
                    subject.degreeId = deg;
                    subject.degreeName = degreeName;
                    subject.departmentId = dept;
                    subject.departmentName = departmentName;
                    subject.specializationId = item.specializationId;
                    subject.specializationName = SpecializationName;
                    subject.shiftId = item.shiftId;
                    subject.shiftName = ShiftName;
                    subject.subject = item.subjectOrLab;
                    subject.duration = item.classDuration;
                    FacultySubject.Add(subject);
                }

                for (int i = 1; i <= 4 - list.Count(); i++)
                {
                    FacultySubject subject = new FacultySubject();
                    subject.degreeId = 0;
                    subject.departmentId = 0;
                    subject.specializationId = 0;
                    subject.shiftId = 1;
                    subject.subject = string.Empty;
                    subject.duration = 0;
                    FacultySubject.Add(subject);
                }
            }

            collegeFaculty.FacultySubject = FacultySubject;
            collegeFaculty.facultyGenderId = null;
            collegeFaculty.isFacultyRatifiedByJNTU = null;
            collegeFaculty.isRelatedToExamBranch = null;
            collegeFaculty.isRelatedToPlacementCell = null;
            collegeFaculty.facultySalary = null;
            collegeFaculty.facultyTypeId = fType;
            collegeFaculty.isFacultyRatifiedByJNTU = false;

            if (fID > 0)
            {
                jntuh_college_faculty faculty = db.jntuh_college_faculty.Find(fID);
                collegeFaculty.collegeId = faculty.collegeId;
                collegeFaculty.facultyTypeId = faculty.facultyTypeId;
                collegeFaculty.facultyFirstName = faculty.facultyFirstName;
                collegeFaculty.facultyLastName = faculty.facultyLastName;
                collegeFaculty.facultySurname = faculty.facultySurname;
                collegeFaculty.facultyGenderId = faculty.facultyGenderId;
                collegeFaculty.facultyFatherName = faculty.facultyFatherName;
                collegeFaculty.facultyCategoryId = faculty.facultyCategoryId;
                collegeFaculty.photo = faculty.facultyPhoto;

                if (faculty.facultyDateOfBirth != null)
                    collegeFaculty.dateOfBirth = Utilities.MMDDYY2DDMMYY(faculty.facultyDateOfBirth.ToString()); // Convert.ToDateTime(faculty.facultyDateOfBirth).ToString("dd/MM/yyyy").Split(' ')[0];

                collegeFaculty.facultyDesignationId = faculty.facultyDesignationId;
                collegeFaculty.facultyOtherDesignation = faculty.facultyOtherDesignation;
                collegeFaculty.facultyDepartmentId = faculty.facultyDepartmentId;

                if (faculty.facultyDateOfAppointment != null)
                    collegeFaculty.dateOfAppointment = Utilities.MMDDYY2DDMMYY(faculty.facultyDateOfAppointment.ToString()); // Convert.ToDateTime(faculty.facultyDateOfAppointment).ToString("dd/MM/yyyy").Split(' ')[0];

                if (faculty.facultyDateOfResignation != null)
                    collegeFaculty.dateOfResignation = Utilities.MMDDYY2DDMMYY(faculty.facultyDateOfResignation.ToString()); // Convert.ToDateTime(faculty.facultyDateOfResignation).ToString("dd/MM/yyyy").Split(' ')[0];

                collegeFaculty.isFacultyRatifiedByJNTU = faculty.isFacultyRatifiedByJNTU;

                if (faculty.facultyDateOfRatification != null)
                    collegeFaculty.dateOfRatification = Utilities.MMDDYY2DDMMYY(faculty.facultyDateOfRatification.ToString()); // Convert.ToDateTime(faculty.facultyDateOfRatification).ToString("dd/MM/yyyy").Split(' ')[0];

                collegeFaculty.facultyDurationOfRatification = faculty.facultyDurationOfRatification;
                collegeFaculty.facultySalary = faculty.facultySalary;
                collegeFaculty.facultyPreviousExperience = Convert.ToInt32(faculty.facultyPreviousExperience);
                collegeFaculty.facultyPayScale = faculty.facultyPayScale;
                collegeFaculty.salaryAccountNumber = faculty.salaryAccountNumber;
                collegeFaculty.salaryBankName = faculty.salaryBankName;
                collegeFaculty.salaryBranchName = faculty.salaryBranchName;
                collegeFaculty.facultyRecruitedFor = faculty.facultyRecruitedFor;
                collegeFaculty.grossSalary = faculty.grossSalary;
                collegeFaculty.netSalary = faculty.netSalary;
                collegeFaculty.facultyEmail = faculty.facultyEmail;
                collegeFaculty.facultyMobile = faculty.facultyMobile;
                collegeFaculty.facultyPANNumber = faculty.facultyPANNumber;
                collegeFaculty.facultyAadhaarNumber = faculty.facultyAadhaarNumber;
                collegeFaculty.isRelatedToExamBranch = faculty.isRelatedToExamBranch;
                collegeFaculty.isRelatedToPlacementCell = faculty.isRelatedToPlacementCell;
                collegeFaculty.facultyAchievements1 = faculty.facultyAchievements1;
                collegeFaculty.facultyAchievements2 = faculty.facultyAchievements2;
                collegeFaculty.facultyAchievements3 = faculty.facultyAchievements3;

                ViewBag.FacultyDepartmentName = db.jntuh_department.Where(d => d.id == collegeFaculty.facultyDepartmentId).Select(d => d.departmentName).FirstOrDefault();
                ViewBag.FacultyCategoryName = db.jntuh_faculty_category.Where(c => c.id == collegeFaculty.facultyCategoryId).Select(c => c.facultyCategory).FirstOrDefault();
                ViewBag.FacultyDesignation = db.jntuh_designation.Where(d => d.id == collegeFaculty.facultyDesignationId).Select(d => d.designation).FirstOrDefault();


            }
            collegeFaculty.dateOfResignation = collegeFaculty.dateOfResignation == "1/1/0001" ? string.Empty : collegeFaculty.dateOfResignation;
            return View("UserView", collegeFaculty);
        }
    }
}
