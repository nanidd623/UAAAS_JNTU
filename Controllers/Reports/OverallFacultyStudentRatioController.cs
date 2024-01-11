using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using UAAAS.Models;


namespace UAAAS.Controllers
{
    [ErrorHandling]
    public class OverallFacultyStudentRatioController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult OverallFacultyStudentRatio()
        {
            int presentYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int AY1 = db.jntuh_academic_year.Where(a => a.actualYear == (presentYear + 1)).Select(a => a.id).FirstOrDefault();
            int AY2 = db.jntuh_academic_year.Where(a => a.actualYear == (presentYear)).Select(a => a.id).FirstOrDefault();
            int AY3 = db.jntuh_academic_year.Where(a => a.actualYear == (presentYear - 1)).Select(a => a.id).FirstOrDefault();
            int AY4 = db.jntuh_academic_year.Where(a => a.actualYear == (presentYear - 2)).Select(a => a.id).FirstOrDefault();
            int AY5 = db.jntuh_academic_year.Where(a => a.actualYear == (presentYear - 3)).Select(a => a.id).FirstOrDefault();
            int AY6 = db.jntuh_academic_year.Where(a => a.actualYear == (presentYear - 4)).Select(a => a.id).FirstOrDefault();
            List<jntuh_college_overall_faculty_studentratio> intake = db.jntuh_college_overall_faculty_studentratio.Where(i => i.collegeId == 403).ToList();

            List<OverallStudentRatio> overallStudentRatio = new List<OverallStudentRatio>();

            foreach (var item in intake)
            {
                OverallStudentRatio newIntake = new OverallStudentRatio();
                newIntake.id = item.id;
                newIntake.collegeId = item.collegeId;
                newIntake.academicYearId = item.academicYearId;
                newIntake.shiftId = item.shiftId;               
                newIntake.totalFaculty = item.totalFaculty;
                newIntake.ratifiedFaculty = item.ratifiedFaculty;
                newIntake.specializationId = item.specializationId;
                newIntake.Specialization = db.jntuh_specialization.Where(s => s.id == item.specializationId).Select(s => s.specializationName).FirstOrDefault();
                newIntake.DepartmentID = db.jntuh_specialization.Where(s => s.id == item.specializationId).Select(s => s.departmentId).FirstOrDefault();
                newIntake.Department = db.jntuh_department.Where(d => d.id == newIntake.DepartmentID).Select(d => d.departmentName).FirstOrDefault();
                newIntake.degreeID = db.jntuh_department.Where(d => d.id == newIntake.DepartmentID).Select(d => d.degreeId).FirstOrDefault();
                newIntake.degree = db.jntuh_degree.Where(d => d.id == newIntake.degreeID).Select(d => d.degree).FirstOrDefault();
                newIntake.degreeDisplayOrder = db.jntuh_degree.Where(d => d.id == newIntake.degreeID).Select(d => d.degreeDisplayOrder).FirstOrDefault();
                newIntake.shiftId = item.shiftId;              
                overallStudentRatio.Add(newIntake);
            }
            
            overallStudentRatio = overallStudentRatio.AsEnumerable().GroupBy(r => new { r.specializationId, r.shiftId }).Select(r => r.First()).ToList();
            foreach (var item in overallStudentRatio)
            {

                item.totalFaculty = item.totalFaculty;
                item.ratifiedFaculty = item.ratifiedFaculty;

                item.approvedIntake1 = GetIntake(403, AY1, item.specializationId, item.shiftId);

                item.approvedIntake2 = GetIntake(403, AY2, item.specializationId, item.shiftId);

                item.approvedIntake3 = GetIntake(403, AY3, item.specializationId, item.shiftId);

                item.approvedIntake4 = GetIntake(403, AY4, item.specializationId, item.shiftId);

                item.approvedIntake5 = GetIntake(403, AY5, item.specializationId, item.shiftId);

                item.approvedIntake6 = GetIntake(403, AY6, item.specializationId, item.shiftId);
            }
            overallStudentRatio = overallStudentRatio.OrderBy(ei => ei.degreeDisplayOrder).ThenBy(ei => ei.Department).ThenBy(ei => ei.Specialization).ThenBy(ei => ei.shiftId).ToList();


            var degree = overallStudentRatio.Select(d => new { degree = d.degree, degreeId = d.degreeID }).Distinct().ToList();
         

            return View("~/Views/Reports/OverallFacultyStudentRatio.cshtml", overallStudentRatio);
        }
        private int GetIntake(int collegeId, int academicYearId, int specializationId, int shiftId)
        {
            int intake = 0;
                intake = db.jntuh_college_overall_faculty_studentratio.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == shiftId).Select(i => i.sanctionedIntake).FirstOrDefault();
            return intake;
        }

        
    }
}
