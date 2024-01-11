using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using UAAAS.Models;

namespace UAAAS.Controllers.Reports
{
    public class HumanitiesDeficiencyController : Controller
    {
        //
        // GET: /HumanitiesDeficiency/
        private uaaasDBContext db = new uaaasDBContext();
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult PhysicalLabDeficiency()
        {
            int collegeId =8;

            return View();
        }

        [Authorize(Roles = "Admin")]
        public ActionResult HumanitiesDeficiency()
        {
            List<HumanitiesDeficiencyList> HumanitiesDeficiencyfinal = new List<HumanitiesDeficiencyList>();
            List<HumanitiesDeficiencyList> HumanitiesDeficiency = new List<HumanitiesDeficiencyList>();
            int[] collegeidsarry =db.jntuh_college_edit_status.Where(e=>e.IsCollegeEditable==false).Select(s=>s.collegeId).ToArray();
            List<jntuh_college> jntuh_college = db.jntuh_college.Select(s => s).ToList();
            foreach (int id in collegeidsarry)
            {

                int collegeId = id;
                List<CollegeFacultyWithIntakeReport> intakedetailsList = new List<CollegeFacultyWithIntakeReport>();
                int[] btechdegreesdept = db.jntuh_department.Where(d => d.degreeId == 4).Select(s => s.id).ToArray();
                var jntuh_specialization = db.jntuh_specialization.ToList();
                int[] btechdegreesspec =
                    db.jntuh_specialization.Where(s => btechdegreesdept.Contains(s.departmentId))
                        .Select(s => s.id)
                        .ToArray();
                List<jntuh_college_intake_existing> intake =
                    db.jntuh_college_intake_existing.Where(
                        i =>
                            i.collegeId == collegeId && i.academicYearId == 10 &&
                            btechdegreesspec.Contains(i.specializationId) && i.proposedIntake != 0).ToList();
                var jntuh_college_faculty_registered =
                    db.jntuh_college_faculty_registered.Where(cf => cf.collegeId == collegeId && cf.DepartmentId != null)
                        .ToList();
                string[] strRegnos =
                    jntuh_college_faculty_registered.Select(cf => cf.RegistrationNumber.Trim()).ToArray();
                var jntuh_college_faculty_registered_new =
                    db.jntuh_college_principal_registered.Where(cf => cf.collegeId == collegeId).ToList();
                var principalRegno =
                    jntuh_college_faculty_registered_new.Select(cf => cf.RegistrationNumber).FirstOrDefault();
                var registeredFaculty =
                    db.jntuh_registered_faculty.Where(rf => strRegnos.Contains(rf.RegistrationNumber.Trim())).ToList();
                int TotalIntake = 0;
                double rfaculty = 0;
                foreach (var item in intake)
                {
                    TotalIntake = TotalIntake + (int) item.proposedIntake;
                }
                rfaculty = Math.Ceiling((double) TotalIntake/160);
                var jntuh_registered_faculty1 =
                   registeredFaculty.Where(rf => rf.type != "Adjunct" && ((rf.Absent == false) && (rf.OriginalCertificatesNotShown == false) && (rf.Xeroxcopyofcertificates == false || rf.Xeroxcopyofcertificates == null) && (rf.NotQualifiedAsperAICTE == false || rf.NotQualifiedAsperAICTE == null)//&& rf.Noform16Verification == false && rf.NoForm16 == false
                                                       && (rf.NoSCM == false || rf.NoSCM == null) && (rf.PANNumber != null) && (rf.IncompleteCertificates == false || rf.IncompleteCertificates == null) && (rf.Blacklistfaculy == false) && rf.NoRelevantUG != "Yes" && rf.NoRelevantPG == "Yes" && rf.NORelevantPHD == "Yes" && rf.NotIdentityfiedForanyProgram == false && (rf.InvalidPANNumber == false || rf.InvalidPANNumber == null) && (rf.Invaliddegree == false || rf.Invaliddegree == null) && rf.BAS != "Yes") && rf.InvalidAadhaar != "Yes" && (rf.DepartmentId != 61 || rf.DepartmentId != 27)).Select(rf => new
                                                       {
                                                           //Departmentid = rf.DepartmentId,
                                                           RegistrationNumber = rf.RegistrationNumber.Trim(),
                                                           // Department = rf.jntuh_department.departmentName,
                                                           DeptId = jntuh_college_faculty_registered.Where(c => c.RegistrationNumber.Trim() == rf.RegistrationNumber.Trim()).Select(c => c.DepartmentId).FirstOrDefault(),
                                                           SpecializationId = jntuh_college_faculty_registered.Where(c => c.RegistrationNumber.Trim() == rf.RegistrationNumber.Trim()).Select(c => c.SpecializationId).FirstOrDefault(),
                                                           HighestDegreeID = rf.jntuh_registered_faculty_education.Count() != 0 ? rf.jntuh_registered_faculty_education.Select(e => e.educationId).Max() : 0,
                                                           IsApproved = rf.isApproved,
                                                           PanNumber = rf.PANNumber,
                                                           AadhaarNumber = rf.AadhaarNumber,
                                                           NoForm16 = rf.NoForm16,
                                                           TotalExperience = rf.TotalExperience
                                                       }).ToList();
                var jntuh_departments = db.jntuh_department.ToList();

                var jntuh_education_category = db.jntuh_education_category.ToList();
                var DeptNameBasedOnSpecialization = (from a in db.jntuh_department
                    join b in db.jntuh_specialization on a.id equals b.departmentId
                    select new
                    {
                        DeptId = a.id,
                        DeptName = a.departmentName,
                        Specid = b.id
                    }).ToList();
                jntuh_registered_faculty1 = jntuh_registered_faculty1.Where(e => e.HighestDegreeID >= 4).ToList();
                var jntuh_registered_faculty = jntuh_registered_faculty1.Select(rf => new
                {
                    RegistrationNumber = rf.RegistrationNumber,
                    Department =
                        rf.DeptId != null
                            ? jntuh_departments.Where(e => e.id == rf.DeptId)
                                .Select(e => e.departmentName)
                                .FirstOrDefault()
                            : DeptNameBasedOnSpecialization.Where(e => e.Specid == rf.SpecializationId)
                                .Select(e => e.DeptName)
                                .FirstOrDefault(),
                    HighestDegree =
                        jntuh_education_category.Where(c => c.id == rf.HighestDegreeID)
                            .Select(c => c.educationCategoryName)
                            .FirstOrDefault(),
                    Recruitedfor =
                        jntuh_college_faculty_registered.Where(
                            c => c.RegistrationNumber.Trim() == rf.RegistrationNumber.Trim())
                            .Select(c => c.IdentifiedFor)
                            .FirstOrDefault(),
                    rf.SpecializationId,
                    // jntuh_college_faculty_registered.Where(c => c.RegistrationNumber.Trim() == rf.RegistrationNumber.Trim()).Select(c => c.SpecializationId).FirstOrDefault(),
                    PanNumber = rf.PanNumber,
                    AadhaarNumber = rf.AadhaarNumber,
                    noform16 = rf.NoForm16,
                    TotalExperience = rf.TotalExperience
                }).Where(e => e.Department != null).ToList();

                string[] strOtherDepartments =
                {
                    "English", "Mathematics", "Physics", "Chemistry", "Others(CSE/IT)",
                    "Others(CIVIL/MECH)", "Others(ECE/EEE)", "Others(MNGT/H&S)"
                };
                var btechdegreecount = intakedetailsList.Count(d => d.Degree == "B.Tech");
                //if (btechdegreecount > 0)
                //{
                if (TotalIntake!=0)
                {
                    foreach (var department in strOtherDepartments)
                    {
                        var deptid =
                            jntuh_departments.Where(i => i.departmentName == department)
                                .Select(i => i.id)
                                .FirstOrDefault();
                        var deptname =
                            jntuh_departments.Where(i => i.departmentName == department)
                                .Select(i => i.departmentName)
                                .FirstOrDefault();
                        int speId =
                            jntuh_specialization.Where(s => s.jntuh_department.departmentName == department)
                                .Select(s => s.id)
                                .FirstOrDefault();
                        int ugFaculty =
                            jntuh_registered_faculty.Count(f => "UG" == f.HighestDegree && f.Department == department);
                        int pgFaculty =
                            jntuh_registered_faculty.Count(
                                f =>
                                    ("PG" == f.HighestDegree || "M.Phil / Other PG Degree" == f.HighestDegree) &&
                                    f.Department == department);
                        var pgreg =
                            jntuh_registered_faculty.Where(
                                f =>
                                    ("PG" == f.Recruitedfor || "M.Phil / Other PG Degree" == f.HighestDegree) &&
                                    f.Department == department).Select(e => e.RegistrationNumber).ToList();
                        //var testcount = jntuh_registered_faculty.Where(f => f.Department == department).ToList();
                        int phdFaculty =
                            jntuh_registered_faculty.Count(f => "Ph.D" == f.HighestDegree && f.Department == department);

                        intakedetailsList.Add(new CollegeFacultyWithIntakeReport
                        {
                            collegeId = (int)collegeId,
                            Degree = "B.Tech",
                            DepartmentID = deptid,
                            totalIntake = TotalIntake,
                            Department = department,
                            Specialization = department,
                            ugFaculty = ugFaculty,
                            pgFaculty = pgFaculty,
                            phdFaculty = phdFaculty,
                            totalFaculty = ugFaculty + pgFaculty + phdFaculty,
                            specializationId = speId,
                            shiftId = 1,
                            //form16count = form16Count.Count > 0 ? form16Count.Count(i => i.DepartmentId == deptid) : 0,
                            //aictecount = aictecount.Count > 0 ? aictecount.Count(i => i.DepartmentId == deptid) : 0,
                            A416TotalFaculty = registeredFaculty.Where(i => i.DepartmentId == deptid).ToList().Count,
                            specializationWiseFaculty = jntuh_registered_faculty.Count(f => f.Department == deptname),
                            approvedIntake1 = 1

                        });
                    }

                }
                if (intakedetailsList.Count!=0)
                {
                    foreach (var item1 in intakedetailsList)
                    {
                        HumanitiesDeficiencyList Humanities = new HumanitiesDeficiencyList();
                        Humanities.collegeCode =
                            jntuh_college.Where(c => c.id == item1.collegeId)
                                .Select(s => s.collegeCode)
                                .FirstOrDefault();
                        Humanities.collegeName =
                            jntuh_college.Where(c => c.id == item1.collegeId)
                                .Select(s => s.collegeName)
                                .FirstOrDefault();
                        Humanities.Totalintake = item1.totalIntake;
                        Humanities.avialablefaculty = item1.totalFaculty;
                        Humanities.requeriedfaculty = (int)rfaculty;
                        Humanities.Degree = item1.Degree;
                        Humanities.Department = item1.Department;
                        Humanities.Specialization = item1.Specialization;

                        HumanitiesDeficiency.Add(Humanities);
                    }
                }                
            }
            HumanitiesDeficiencyfinal.AddRange(HumanitiesDeficiency);
            HumanitiesDeficiencyfinal = HumanitiesDeficiencyfinal.OrderBy(e => e.collegeName).ToList();
                string ReportHeader = null;
                ReportHeader = "HumanitiesDeficiency.xls";
                Response.ClearContent();
                Response.Buffer = true;
                Response.AddHeader("content-disposition", "attachment; filename=" + ReportHeader);
                Response.ContentType = "application/vnd.ms-excel";
                return PartialView(HumanitiesDeficiency);
    }
    }
    public class HumanitiesDeficiencyList
    {
        public string collegeCode { get; set; }
        public string collegeName { get; set; }
        public string Degree { get; set; }
        public string Department { get; set; }
        public string Specialization { get; set; }
        public int Totalintake { get; set; }
        public int avialablefaculty { get; set; }
        public int requeriedfaculty { get; set; }
    }
    public class CollegeFacultyWithIntakeReport
    {
        public int id { get; set; }
        public int collegeId { get; set; }
        public int academicYearId { get; set; }
        public string collegeCode { get; set; }
        public string collegeName { get; set; }
        public string Degree { get; set; }
        public string Department { get; set; }
        public string Specialization { get; set; }
        public int shiftId { get; set; }
        public string Shift { get; set; }
        public int specializationId { get; set; }
        public int DepartmentID { get; set; }
        public int? degreeDisplayOrder { get; set; }
        public int? AvailableFaculty { get; set; }
        public int approvedIntake1 { get; set; }
        public int approvedIntake2 { get; set; }
        public int approvedIntake3 { get; set; }
        public int approvedIntake4 { get; set; }
        public int approvedIntake5 { get; set; }
        public int totalIntake { get; set; }
        public int totalBtechFirstYearIntake { get; set; }
        public int dollercourseintake { get; set; }
        public int firstYearRequired { get; set; }
        public decimal requiredFaculty { get; set; }
        public decimal ArequiredFaculty { get; set; }
        public decimal SrequiredFaculty { get; set; }
        public int phdFaculty { get; set; }
        public int SpecializationsphdFaculty { get; set; }
        public int SpecializationspgFaculty { get; set; }
        public int pgFaculty { get; set; }
        public int ugFaculty { get; set; }
        public int totalFaculty { get; set; }
        public int oldtotalFaculty { get; set; }
        public int newtotalFaculty { get; set; }
        public int specializationWiseFaculty { get; set; }
        public int facultyWithoutPANAndAadhaar { get; set; }
        public int A416TotalFaculty { get; set; }
        public int form16count { get; set; }
        public int aictecount { get; set; }
        public int adjustedFaculty { get; set; }
        public string minimumRequirementMet { get; set; }

        public int DegreeID { get; set; }
        public bool isActive { get; set; }
        public DateTime? nbaFrom { get; set; }
        public DateTime? nbaTo { get; set; }
        public int NOSCM { get; set; }
        public string PHDminimumRequirementMet { get; set; }
        public int DeactivationReasionsCount { get; set; }
        public int FacultyAbsentCount { get; set; }
        public int InvalidPanCount { get; set; }
        public int incompletecerificatesCount { get; set; }
        public int adjointfacultycount { get; set; }

        public bool? deficiency { get; set; }
        public int shortage { get; set; }
        public string Note { get; set; }
        public string AffliationStatus { get; set; }
        public string collegeRandomCode { get; set; }
        public decimal BphramacyrequiredFaculty { get; set; }
        public decimal pharmadrequiredfaculty { get; set; }
        public decimal pharmadPBrequiredfaculty { get; set; }
        public int totalcollegefaculty { get; set; }
        public int SortId { get; set; }

        public IList<CollegeFacultyWithIntakeReport> FacultyWithIntakeReports { get; set; }
        public int BtechAdjustedFaculty { get; set; }
        public int specializationWiseFacultyPHDFaculty { get; set; }

        public string PharmacyGroup1 { get; set; }


        public string PharmacySubGroup1 { get; set; }
        public int BPharmacySubGroup1Count { get; set; }
        public int BPharmacySubGroupRequired { get; set; }
        public string BPharmacySubGroupMet { get; set; }
        public int PharmacyspecializationWiseFaculty { get; set; }

        public string PharmacySpec1 { get; set; }
        public string PharmacySpec2 { get; set; }

        public IList<PharmacySpecilaizationList> PharmacySpecilaizationList { get; set; }

        public bool AffiliationStatus2 { get; set; }
        public bool AffiliationStatus3 { get; set; }
        public bool AffiliationStatus4 { get; set; }

        //Added this in 25-04-2017
        public int admittedIntake1 { get; set; }
        public int admittedIntake2 { get; set; }
        public int admittedIntake3 { get; set; }
        public int admittedIntake4 { get; set; }
        public int admittedIntake5 { get; set; }

        //Added 02-03-2019
        public int Proposedintake { get; set; }
        public int AICTESanctionIntake1 { get; set; }
        public int AICTESanctionIntake2 { get; set; }
        public int AICTESanctionIntake3 { get; set; }
        public int AICTESanctionIntake4 { get; set; }
        public int AICTESanctionIntake5 { get; set; }

        public int ExambranchIntake_R1 { get; set; }
        public int ExambranchIntake_R2 { get; set; }
        public int ExambranchIntake_R3 { get; set; }
        public int ExambranchIntake_R4 { get; set; }
        public int ExambranchIntake_R5 { get; set; }

        public int ExambranchIntake_L1 { get; set; }
        public int ExambranchIntake_L2 { get; set; }
        public int ExambranchIntake_L3 { get; set; }
        public int ExambranchIntake_L4 { get; set; }
        public int ExambranchIntake_L5 { get; set; }

        public int SanctionIntake1 { get; set; }
        public int SanctionIntake2 { get; set; }
        public int SanctionIntake3 { get; set; }
        public int SanctionIntake4 { get; set; }
        public int SanctionIntake5 { get; set; }
        public bool ispercentage { get; set; }
        public bool ishashcourses { get; set; }
        public bool isstarcourses { get; set; }

        public int totalAdmittedIntake { get; set; }
    }
    public class PharmacySpecilaizationList
    {
        public string PharmacyspecName { get; set; }
        public string Specialization { get; set; }
        public int PharmacyspecWiseCount { get; set; }
    }
}

