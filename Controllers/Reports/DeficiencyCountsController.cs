using iTextSharp.text;
using iTextSharp.text.html.simpleparser;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using UAAAS.Models;

namespace UAAAS.Controllers.Reports
{
    public class DeficiencyCountsController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();

        public ActionResult Deficiencies()
        {
            StringBuilder str = new StringBuilder();

            //&& (s.collegeId == 19 || s.collegeId == 10)
            List<int> collegeIds = db.jntuh_college_edit_status.Where(s => s.IsCollegeEditable == false).Select(s => s.collegeId).OrderBy(s => s).ToList();

            string table = "<table width='100%' border='1' cellpadding='3' cellspacing='0' style='border-color: #ccc;'>";
            table += "<tr><th>S.No</th><th>Code</th><th>College with all B.Tech courses approved</th></tr>";

            int count = 1;

            string faculty = string.Empty;
            faculty += "<table width='100%' border='1' cellpadding='3' cellspacing='0' style='border-color: #ccc;'>";
            faculty += "<tr>";
            faculty += "<th style='text-align: center; vertical-align: top;'>S.No</th>";
            faculty += "<th style='text-align: left; vertical-align: top;' >Code</th>";
            faculty += "<th style='text-align: left; vertical-align: top;' >College Name</th>";
            faculty += "<th style='text-align: left; vertical-align: top;' >Department</th>";
            faculty += "<th style='text-align: left; vertical-align: top;' >Degree</th>";
            faculty += "<th style='text-align: left; vertical-align: top;' >Specialization</th>";
            faculty += "<th style='text-align: center; vertical-align: top;' >Intake</th>";
            faculty += "</tr>";

            string labs = string.Empty;
            labs += "<table width='100%' border='1' cellpadding='3' cellspacing='0' style='border-color: #ccc;'>";
            labs += "<tr>";
            labs += "<th style='text-align: center; width: 5%;'>S.No</th>";
            labs += "<th style='text-align: left; width: 10%;'>Code</th>";
            labs += "<th style='text-align: left; width: 10%;'>College Name</th>";
            labs += "<th style='text-align: left; width: 10%;'>Degree</th>";
            labs += "<th style='text-align: left; width: 10%;'>Department</th>";
            labs += "<th style='text-align: left; width: 20%;'>Specialization</th>";
            labs += "<th style='text-align: center; '>Names of the Labs with Deficiency</th>";
            labs += "</tr>";

            foreach (var collegeID in collegeIds)
            {
                //faculty
                string[] fStatus = DeficienciesInFaculty(collegeID).Split('$');

                string fFlag = fStatus[0];
                string facultyList = fStatus[1];

                //labs
                string[] lStatus = DeficienciesInLabs(collegeID).Split('$');

                string lFlag = lStatus[0];
                string labsList = lStatus[1];

                if (fFlag == "YES" && lFlag == "YES")
                {
                    var college = db.jntuh_college.Where(c => c.isActive == true && c.id == collegeID).Select(c => c).FirstOrDefault();
                    table += "<tr><td>" + count + "</td><td>" + college.collegeCode + "</td><td>" + college.collegeName + "</td></tr>";
                    count++;

                    faculty += facultyList;
                    labs += labsList;
                }
            }

            faculty += "</table>";
            labs += "</table>";

            table += "</table>";

            Response.ClearContent();
            Response.ClearHeaders();
            Response.Buffer = true;
            Response.AddHeader("content-disposition", "attachment; filename=Deficiency in all courses of B.Tech - Course Wise Intake" + ".doc");
            Response.ContentType = "application/vnd.ms-word";
            Response.Charset = string.Empty;
            Response.Output.Write(table + "<br />" + faculty + "<br />" + labs);
            Response.Flush();
            Response.End();

            return View();
        }

        public ActionResult NoDeficiencyCourses(int collegeid)
        {
            List<int> collegeIds = db.jntuh_college_edit_status.Where(s => s.IsCollegeEditable == false && s.collegeId == collegeid).Select(s => s.collegeId).OrderBy(s => s).ToList();

            string[] departments = new string[] { "English", "Mathematics", "Physics", "Chemistry", "Others", "Library Science", "Humanities" };

            var allCourses = db.jntuh_specialization.Where(s => s.jntuh_department.jntuh_degree.degree == "B.Tech" && !departments.Contains(s.jntuh_department.departmentName))
                               .Select(s => new { id = s.id, name = s.specializationName }).ToList();

            string table = "<table width='100%' border='1' cellpadding='3' cellspacing='0' style='border-color: #ccc;'>";
            table += "<tr>";
            table += "<th>S.No</th>";
            table += "<th>Code</th>";
            table += "<th>College Name</th>";

            foreach (var course in allCourses)
            {
                table += "<th>" + course.name + "</th>";
            }

            table += "</tr>";

            int count = 1;

            foreach (var collegeID in collegeIds)
            {
                var college = db.jntuh_college.Where(c => c.isActive == true && c.id == collegeID).Select(c => c).FirstOrDefault();
                table += "<tr>";
                table += "<td>" + count + "</td>";
                table += "<td>" + college.collegeCode + "</td>";
                table += "<td>" + college.collegeName + "</td>";

                foreach (var course in allCourses)
                {
                    //faculty
                    string[] fStatus = NoFacultyDeficiencyCourse(collegeID, course.id).Split('$');


                    string fFlag = fStatus[0];
                    string intake = fStatus[1];

                    //labs
                    string lStatus = NoLabDeficiencyCourse(collegeID, course.id);

                    string lFlag = lStatus;

                    if (fFlag == "YES" && lFlag == "YES")
                    {
                        table += "<td>" + intake + "</td>";
                    }
                    else
                    {
                        table += "<td></td>";
                    }
                }

                table += "</tr>";

                count++;
            }

            table += "</table>";

            Response.ClearContent();
            Response.ClearHeaders();
            Response.Buffer = true;
            Response.AddHeader("content-disposition", "attachment; filename=Deficiency in all courses of B.Tech - Course Wise Intake" + ".xls");
            Response.ContentType = "application/vnd.ms-excel";
            Response.Charset = string.Empty;
            Response.Output.Write(table);
            Response.Flush();
            Response.End();

            return View();
        }

        public string NoFacultyDeficiencyCourse(int? collegeID, int speciazliationID)
        {
            string faculty = string.Empty; string flag = string.Empty + "$" + string.Empty;
            string[] departments = new string[] { "English", "Mathematics", "Physics", "Chemistry", "Others" };

            List<CollegeFacultyWithIntakeReport> facultyCounts = collegeFaculty(collegeID).Where(c => c.shiftId == 1 && c.Degree == "B.Tech" && c.specializationId == speciazliationID && !departments.Contains(c.Department)).ToList();

            var count = facultyCounts.Count();
            var distDeptcount = 1;
            var deptloop = 1;
            decimal departmentWiseRequiredFaculty = 0;

            int totalBtechFirstYearIntake = facultyCounts.Where(d => d.Degree == "B.Tech").Select(d => d.approvedIntake1).Sum();
            var firstYearRequired = Math.Ceiling((double)totalBtechFirstYearIntake / 120);
            int remainingFaculty = 0;
            int remainingPHDFaculty = 0;

            int totalSpecializations = facultyCounts.Count();
            int totalDeficiencySpecializations = 0;
            int totalApprovedSpecializations = 0;
            int courseApproved = 0;

            foreach (var item in facultyCounts)
            {
                courseApproved = 0;

                //if (item.approvedIntake1 > 0)
                //{
                distDeptcount = facultyCounts.Where(d => d.Department == item.Department).Distinct().Count();

                int indexnow = facultyCounts.IndexOf(item);

                if (indexnow > 0 && facultyCounts[indexnow].Department != facultyCounts[indexnow - 1].Department)
                {
                    deptloop = 1;
                }

                departmentWiseRequiredFaculty = facultyCounts.Where(d => d.Department == item.Department).Select(d => d.requiredFaculty).Sum();

                string minimumRequirementMet = string.Empty;
                int facultyShortage = 0;
                int adjustedFaculty = 0;
                int adjustedPHDFaculty = 0;

                int tFaculty = (int)Math.Ceiling(Convert.ToDecimal(item.totalFaculty));
                int rFaculty = (int)Math.Ceiling(item.requiredFaculty);

                if (departments.Contains(item.Department))
                {
                    rFaculty = (int)firstYearRequired;
                    departmentWiseRequiredFaculty = (int)firstYearRequired;
                }

                var degreeType = db.jntuh_degree.Where(d => d.degree == item.Degree).Select(d => d.jntuh_degree_type.degreeType).FirstOrDefault();

                if (deptloop == 1)
                {
                    if (rFaculty <= tFaculty)
                    {
                        minimumRequirementMet = "NO"; totalApprovedSpecializations += 1; courseApproved = 1;
                        remainingFaculty = tFaculty - rFaculty;
                        adjustedFaculty = rFaculty;
                    }
                    else
                    {
                        minimumRequirementMet = "YES"; totalDeficiencySpecializations += 1;
                        adjustedFaculty = tFaculty;
                        facultyShortage = rFaculty - tFaculty;
                    }

                    remainingPHDFaculty = item.phdFaculty;

                    if (remainingPHDFaculty > 0 && degreeType.Equals("PG"))
                    {
                        adjustedPHDFaculty = 1;
                        remainingPHDFaculty = remainingPHDFaculty - 1;
                    }
                }
                else
                {
                    if (rFaculty <= remainingFaculty)
                    {
                        minimumRequirementMet = "NO"; totalApprovedSpecializations += 1; courseApproved = 1;
                        remainingFaculty = remainingFaculty - rFaculty;
                        adjustedFaculty = rFaculty;
                    }
                    else
                    {
                        minimumRequirementMet = "YES"; totalDeficiencySpecializations += 1;
                        adjustedFaculty = remainingFaculty;
                        facultyShortage = rFaculty - remainingFaculty;
                        remainingFaculty = 0;
                    }

                    if (remainingPHDFaculty > 0 && degreeType.Equals("PG"))
                    {
                        adjustedPHDFaculty = 1;
                        remainingPHDFaculty = remainingPHDFaculty - 1;
                    }
                }

                //if (courseApproved == 1)
                //{
                faculty += "<tr>";
                faculty += "<td class='col2' style='text-align: center; vertical-align: top;width:10px' rowspan='1'>" + (facultyCounts.IndexOf(item) + 1) + "</td>";
                faculty += "<td class='col2' style='text-align: left; vertical-align: top;'>" + item.collegeCode + "</td>";
                faculty += "<td class='col2' style='text-align: left; vertical-align: top;'>" + item.collegeName + "</td>";
                faculty += "<td class='col2' style='text-align: left; vertical-align: top;'>" + item.Department + "</td>";
                faculty += "<td class='col2' style='text-align: left; vertical-align: top;'>" + item.Degree + "</td>";
                faculty += "<td class='col2' style='text-align: left; vertical-align: top;'>" + item.Specialization + "</td>";
                faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + item.approvedIntake1 + "</td>";
                faculty += "</tr>";
                //}

                deptloop++;

                //}

                if (totalSpecializations > 0)
                {
                    //flag = (totalSpecializations == totalDeficiencySpecializations) ? "YES" : "NO";
                    flag = (totalSpecializations == totalApprovedSpecializations) ? "YES" : "NO";
                    //flag = (totalApprovedSpecializations > 0) ? "NO" : "YES";
                }

                flag = flag + "$" + item.approvedIntake1;
            }

            return flag;
        }

        public string NoLabDeficiencyCourse(int? collegeID, int speciazliationID)
        {
            string labs = string.Empty; string flag = string.Empty;
            string[] departments = new string[] { "English", "Mathematics", "Physics", "Chemistry", "Others" };

            List<Lab> labsCount = collegeLabs(collegeID);
            List<Lab> labsCount1 = labsCount.Where(c => c.degree == "B.Tech" && c.specializationId == speciazliationID && !departments.Contains(c.department)).ToList();

            var deficiencies = labsCount1.GroupBy(l => new { l.degree, l.department, l.specializationName })
                                         .Select(l => new { degree = l.Key.degree, department = l.Key.department, specializationName = l.Key.specializationName, deficiencies = string.Empty })
                                         .ToList();

            int totalSpecializations = deficiencies.Count();
            int totalDeficiencySpecializations = 0;
            int totalApprovedSpecializations = 0;
            int courseApproved = 0;

            List<jntuh_college> colleges = db.jntuh_college.ToList();

            foreach (var item in deficiencies)
            {
                courseApproved = 0;
                string degreeType = db.jntuh_degree.Where(d => d.degree == item.degree).Select(d => d.jntuh_degree_type.degreeType).FirstOrDefault();

                var labsWithDeficiency = labsCount.Where(l => l.degree == item.degree && l.department == item.department && l.specializationName == item.specializationName && l.deficiency == true)
                    .Select(l => new { Deficiency = degreeType.Equals("PG") ? "No Equipement Uploaded" : l.year + "-" + l.Semester + "-" + l.LabName }).Select(l => l.Deficiency).ToList();

                //if (courseApproved == 1)
                //{
                labs += "<tr>";
                labs += "<td style='text-align: center; width: 5%; '>" + (deficiencies.IndexOf(item) + 1) + "</td>";
                labs += "<td style=''>" + colleges.Where(c => c.id == collegeID).Select(c => c.collegeCode).FirstOrDefault() + "</td>";
                labs += "<td style=''>" + colleges.Where(c => c.id == collegeID).Select(c => c.collegeName).FirstOrDefault() + "</td>";
                labs += "<td style=''>" + item.degree + "</td>";
                labs += "<td style=''>" + item.department + "</td>";
                labs += "<td style=''>" + item.specializationName + "</td>";
                labs += "<td style='; text-align: center'>" + (labsWithDeficiency.Count() == 0 ? "NIL" : String.Join(", ", labsWithDeficiency)) + "</td>";
                labs += "</tr>";
                //}

                if (labsWithDeficiency.Count() == 0)
                {
                    totalApprovedSpecializations += 1;
                    courseApproved = 1;
                }

                if (labsWithDeficiency.Count() > 0)
                {
                    totalDeficiencySpecializations += 1;
                }

                if (totalSpecializations > 0)
                {
                    //flag = (totalSpecializations == totalDeficiencySpecializations) ? "YES" : "NO";
                    flag = (totalSpecializations == totalApprovedSpecializations) ? "YES" : "NO";
                    //flag = (totalApprovedSpecializations > 0) ? "NO" : "YES";
                }
            }

            return flag;
        }

        #region Not in use

        public string NoFacultyDeficiencyCourseNew(int? collegeID, int speciazliationID)
        {
            string faculty = string.Empty; string flag = string.Empty + "$" + string.Empty;
            string[] departments = new string[] { "English", "Mathematics", "Physics", "Chemistry", "Others" };

            List<CollegeFacultyWithIntakeReport> facultyCounts = collegeFaculty(collegeID).Where(c => c.shiftId == 1 && c.Degree == "B.Tech" && c.specializationId == speciazliationID && !departments.Contains(c.Department)).ToList();

            var count = facultyCounts.Count();
            var distDeptcount = 1;
            var deptloop = 1;
            decimal departmentWiseRequiredFaculty = 0;

            int totalBtechFirstYearIntake = facultyCounts.Where(d => d.Degree == "B.Tech").Select(d => d.approvedIntake1).Sum();
            var firstYearRequired = Math.Ceiling((double)totalBtechFirstYearIntake / 120);
            int remainingFaculty = 0;
            int remainingPHDFaculty = 0;

            int totalSpecializations = facultyCounts.Count();
            int totalDeficiencySpecializations = 0;
            int totalApprovedSpecializations = 0;
            int courseApproved = 0;

            foreach (var item in facultyCounts)
            {
                distDeptcount = facultyCounts.Where(d => d.Department == item.Department).Distinct().Count();

                int indexnow = facultyCounts.IndexOf(item);

                if (indexnow > 0 && facultyCounts[indexnow].Department != facultyCounts[indexnow - 1].Department)
                {
                    deptloop = 1;
                }

                departmentWiseRequiredFaculty = facultyCounts.Where(d => d.Department == item.Department).Select(d => d.requiredFaculty).Sum();

                string minimumRequirementMet = string.Empty;
                int facultyShortage = 0;
                int adjustedFaculty = 0;
                int adjustedPHDFaculty = 0;

                int tFaculty = (int)Math.Ceiling(Convert.ToDecimal(item.totalFaculty));
                int rFaculty = (int)Math.Ceiling(item.requiredFaculty);

                if (departments.Contains(item.Department))
                {
                    rFaculty = (int)firstYearRequired;
                    departmentWiseRequiredFaculty = (int)firstYearRequired;
                }

                var degreeType = db.jntuh_degree.Where(d => d.degree == item.Degree).Select(d => d.jntuh_degree_type.degreeType).FirstOrDefault();

                if (deptloop == 1)
                {
                    if (rFaculty <= tFaculty)
                    {
                        minimumRequirementMet = "NO";
                        remainingFaculty = tFaculty - rFaculty;
                        adjustedFaculty = rFaculty;
                    }
                    else
                    {
                        minimumRequirementMet = "YES";
                        adjustedFaculty = tFaculty;
                        facultyShortage = rFaculty - tFaculty;
                    }

                    remainingPHDFaculty = item.phdFaculty;

                    if (remainingPHDFaculty > 0 && degreeType.Equals("PG"))
                    {
                        adjustedPHDFaculty = 1;
                        remainingPHDFaculty = remainingPHDFaculty - 1;
                    }
                }
                else
                {
                    if (rFaculty <= remainingFaculty)
                    {
                        minimumRequirementMet = "NO";
                        remainingFaculty = remainingFaculty - rFaculty;
                        adjustedFaculty = rFaculty;
                    }
                    else
                    {
                        minimumRequirementMet = "YES";
                        adjustedFaculty = remainingFaculty;
                        facultyShortage = rFaculty - remainingFaculty;
                        remainingFaculty = 0;
                    }

                    if (remainingPHDFaculty > 0 && degreeType.Equals("PG"))
                    {
                        adjustedPHDFaculty = 1;
                        remainingPHDFaculty = remainingPHDFaculty - 1;
                    }
                }
                if (totalSpecializations > 0)
                {
                    if (minimumRequirementMet == "YES")
                    {
                        flag = "NO";
                    }
                    else
                    {
                        flag = "YES";
                    }
                }

                flag = flag + "$" + item.approvedIntake1;

                deptloop++;
            }
            return flag;
        }

        public string NoLabDeficiencyCourseNew(int? collegeID, int speciazliationID)
        {
            string labs = string.Empty; string flag = string.Empty;
            string[] departments = new string[] { "English", "Mathematics", "Physics", "Chemistry", "Others" };

            List<Lab> labsCount = collegeLabs(collegeID);
            List<Lab> labsCount1 = labsCount.Where(c => c.degree == "B.Tech" && c.specializationId == speciazliationID && !departments.Contains(c.department)).ToList();

            var deficiencies = labsCount1.GroupBy(l => new { l.degree, l.department, l.specializationName })
                                         .Select(l => new { degree = l.Key.degree, department = l.Key.department, specializationName = l.Key.specializationName, deficiencies = string.Empty })
                                         .ToList();

            int totalSpecializations = deficiencies.Count();
            int totalDeficiencySpecializations = 0;
            int totalApprovedSpecializations = 0;
            int courseApproved = 0;

            List<jntuh_college> colleges = db.jntuh_college.ToList();

            foreach (var item in deficiencies)
            {
                string degreeType = db.jntuh_degree.Where(d => d.degree == item.degree).Select(d => d.jntuh_degree_type.degreeType).FirstOrDefault();

                var labsWithDeficiency = labsCount.Where(l => l.degree == item.degree && l.department == item.department && l.specializationName == item.specializationName && l.deficiency == true)
                    .Select(l => new { Deficiency = degreeType.Equals("PG") ? "No Equipement Uploaded" : l.year + "-" + l.Semester + "-" + l.LabName }).Select(l => l.Deficiency).ToList();

                labs += "<td style='; text-align: center'>" + (labsWithDeficiency.Count() == 0 ? "NIL" : String.Join(", ", labsWithDeficiency)) + "</td>";
                labs += "<td style='; text-align: center' colspan='3'></td>";
                labs += "</tr>";
                if (labsWithDeficiency.Count() == 0)
                {
                    if (totalSpecializations > 0)
                    {
                        flag = "YES";
                    }
                    else
                    {
                        flag = "NO";
                    }
                }
            }

            return flag;
        }
        #endregion

        //FACULTY
        public string DeficienciesInFaculty(int? collegeID)
        {
            string faculty = string.Empty; string flag = string.Empty;
            string[] departments = new string[] { "English", "Mathematics", "Physics", "Chemistry", "Others" };

            List<CollegeFacultyWithIntakeReport> facultyCounts = collegeFaculty(collegeID).Where(c => c.shiftId == 1 && c.Degree == "B.Tech" && !departments.Contains(c.Department)).ToList();

            var count = facultyCounts.Count();
            var distDeptcount = 1;
            var deptloop = 1;
            decimal departmentWiseRequiredFaculty = 0;

            int totalBtechFirstYearIntake = facultyCounts.Where(d => d.Degree == "B.Tech").Select(d => d.approvedIntake1).Sum();
            var firstYearRequired = Math.Ceiling((double)totalBtechFirstYearIntake / 120);
            int remainingFaculty = 0;
            int remainingPHDFaculty = 0;

            int totalSpecializations = facultyCounts.Count();
            int totalDeficiencySpecializations = 0;
            int totalApprovedSpecializations = 0;
            int courseApproved = 0;

            foreach (var item in facultyCounts)
            {
                courseApproved = 0;

                //if (item.approvedIntake1 > 0)
                //{
                distDeptcount = facultyCounts.Where(d => d.Department == item.Department).Distinct().Count();

                int indexnow = facultyCounts.IndexOf(item);

                if (indexnow > 0 && facultyCounts[indexnow].Department != facultyCounts[indexnow - 1].Department)
                {
                    deptloop = 1;
                }

                departmentWiseRequiredFaculty = facultyCounts.Where(d => d.Department == item.Department).Select(d => d.requiredFaculty).Sum();

                string minimumRequirementMet = string.Empty;
                int facultyShortage = 0;
                int adjustedFaculty = 0;
                int adjustedPHDFaculty = 0;

                int tFaculty = (int)Math.Ceiling(Convert.ToDecimal(item.totalFaculty));
                int rFaculty = (int)Math.Ceiling(item.requiredFaculty);

                if (departments.Contains(item.Department))
                {
                    rFaculty = (int)firstYearRequired;
                    departmentWiseRequiredFaculty = (int)firstYearRequired;
                }

                var degreeType = db.jntuh_degree.Where(d => d.degree == item.Degree).Select(d => d.jntuh_degree_type.degreeType).FirstOrDefault();

                if (deptloop == 1)
                {
                    if (rFaculty <= tFaculty)
                    {
                        minimumRequirementMet = "NO"; totalApprovedSpecializations += 1; courseApproved = 1;
                        remainingFaculty = tFaculty - rFaculty;
                        adjustedFaculty = rFaculty;
                    }
                    else
                    {
                        minimumRequirementMet = "YES"; totalDeficiencySpecializations += 1;
                        adjustedFaculty = tFaculty;
                        facultyShortage = rFaculty - tFaculty;
                    }

                    remainingPHDFaculty = item.phdFaculty;

                    if (remainingPHDFaculty > 0 && degreeType.Equals("PG"))
                    {
                        adjustedPHDFaculty = 1;
                        remainingPHDFaculty = remainingPHDFaculty - 1;
                    }
                }
                else
                {
                    if (rFaculty <= remainingFaculty)
                    {
                        minimumRequirementMet = "NO"; totalApprovedSpecializations += 1; courseApproved = 1;
                        remainingFaculty = remainingFaculty - rFaculty;
                        adjustedFaculty = rFaculty;
                    }
                    else
                    {
                        minimumRequirementMet = "YES"; totalDeficiencySpecializations += 1;
                        adjustedFaculty = remainingFaculty;
                        facultyShortage = rFaculty - remainingFaculty;
                        remainingFaculty = 0;
                    }

                    if (remainingPHDFaculty > 0 && degreeType.Equals("PG"))
                    {
                        adjustedPHDFaculty = 1;
                        remainingPHDFaculty = remainingPHDFaculty - 1;
                    }
                }

                //if (courseApproved == 1)
                //{
                faculty += "<tr>";
                faculty += "<td class='col2' style='text-align: center; vertical-align: top;width:10px' rowspan='1'>" + (facultyCounts.IndexOf(item) + 1) + "</td>";
                faculty += "<td class='col2' style='text-align: left; vertical-align: top;'>" + item.collegeCode + "</td>";
                faculty += "<td class='col2' style='text-align: left; vertical-align: top;'>" + item.collegeName + "</td>";
                faculty += "<td class='col2' style='text-align: left; vertical-align: top;'>" + item.Department + "</td>";
                faculty += "<td class='col2' style='text-align: left; vertical-align: top;'>" + item.Degree + "</td>";
                faculty += "<td class='col2' style='text-align: left; vertical-align: top;'>" + item.Specialization + "</td>";
                faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + item.approvedIntake1 + "</td>";
                faculty += "</tr>";
                //}

                deptloop++;

                //}
            }

            if (totalSpecializations > 0)
            {
                flag = (totalSpecializations == totalDeficiencySpecializations) ? "YES" : "NO";
                //flag = (totalSpecializations == totalApprovedSpecializations) ? "YES" : "NO";
                //flag = (totalApprovedSpecializations > 0) ? "NO" : "YES";
            }

            return flag + "$" + faculty;
        }
        public List<CollegeFacultyWithIntakeReport> collegeFaculty(int? collegeId)
        {
            List<CollegeIntakeExisting> collegeIntakeExisting = new List<CollegeIntakeExisting>();
            List<CollegeFacultyWithIntakeReport> intakedetailsList = new List<CollegeFacultyWithIntakeReport>();

            if (collegeId != null)
            {
                var jntuh_college_faculty_deficiency = db.jntuh_college_faculty_deficiency.Where(fd => fd.CollegeId == collegeId).ToList();
                var jntuh_specialization = db.jntuh_specialization.ToList();

                int[] collegeIDs = null;
                int facultystudentRatio = 0;
                decimal facultyRatio = 0m;
                if (collegeId != 0)
                {
                    collegeIDs = db.jntuh_college.Where(c => c.isActive == true && c.id == collegeId).Select(c => c.id).ToArray();
                }
                else
                {
                    collegeIDs = db.jntuh_college.Where(c => c.isActive == true).Select(c => c.id).ToArray();
                }
                var jntuh_academic_year = db.jntuh_academic_year.ToList();
                var jntuh_college = db.jntuh_college.Where(c => c.isActive == true).ToList();
                var jntuh_faculty_student_ratio_norms = db.jntuh_faculty_student_ratio_norms.Where(f => f.isActive == true).ToList();
                var jntuh_degree = db.jntuh_degree.ToList();

                int presentYear = jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
                int AY1 = jntuh_academic_year.Where(a => a.actualYear == (presentYear + 1)).Select(a => a.id).FirstOrDefault();
                int AY2 = jntuh_academic_year.Where(a => a.actualYear == (presentYear)).Select(a => a.id).FirstOrDefault();
                int AY3 = jntuh_academic_year.Where(a => a.actualYear == (presentYear - 1)).Select(a => a.id).FirstOrDefault();
                int AY4 = jntuh_academic_year.Where(a => a.actualYear == (presentYear - 2)).Select(a => a.id).FirstOrDefault();

                List<jntuh_college_intake_existing> intake = db.jntuh_college_intake_existing.Where(i => collegeIDs.Contains(i.collegeId)).ToList();
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
                collegeIntakeExisting = collegeIntakeExisting.AsEnumerable().GroupBy(r => new { r.specializationId, r.shiftId, r.collegeId }).Select(r => r.First()).ToList();

                //college Reg nos
                var jntuh_college_faculty_registered = db.jntuh_college_faculty_registered.Where(cf => cf.collegeId == collegeId && cf.createdBy != 63809).ToList();
                string[] strRegnos = jntuh_college_faculty_registered.Select(cf => cf.RegistrationNumber).ToArray();

                //education categoryIds UG,PG,PHD...........

                int pharmacyDeptId = db.jntuh_department.Where(d => d.departmentName == "Pharmacy").Select(d => d.id).FirstOrDefault();

                var jntuh_education_category = db.jntuh_education_category.ToList();

                var registeredFaculty = db.jntuh_registered_faculty.Where(rf => strRegnos.Contains(rf.RegistrationNumber) && (rf.collegeId == null || rf.collegeId == collegeId)).ToList();
                //Reg nos related online facultyIds
                var jntuh_registered_faculty1 = registeredFaculty.Where(rf => rf.DepartmentId != null && (rf.isApproved == null || rf.isApproved == true) && (rf.PANNumber != null || rf.AadhaarNumber != null))
                                                 .Select(rf => new
                                                 {
                                                     RegistrationNumber = rf.RegistrationNumber,
                                                     Department = rf.jntuh_department.departmentName,
                                                     HighestDegreeID = rf.jntuh_registered_faculty_education.Count() != 0 ? rf.jntuh_registered_faculty_education.Select(e => e.educationId).Max() : 0,
                                                     IsApproved = rf.isApproved,
                                                     PanNumber = rf.PANNumber,
                                                     AadhaarNumber = rf.AadhaarNumber
                                                 }).ToList();
                jntuh_registered_faculty1 = jntuh_registered_faculty1.Where(e => e.HighestDegreeID != 0).ToList();
                var jntuh_registered_faculty = jntuh_registered_faculty1.Select(rf => new
                {
                    RegistrationNumber = rf.RegistrationNumber,
                    Department = rf.Department,
                    HighestDegree = jntuh_education_category.Where(c => c.id == rf.HighestDegreeID).Select(c => c.educationCategoryName).FirstOrDefault(),
                    Recruitedfor = jntuh_college_faculty_registered.Where(c => c.RegistrationNumber == rf.RegistrationNumber).Select(c => c.IdentifiedFor).FirstOrDefault(),
                    SpecializationId = jntuh_college_faculty_registered.Where(c => c.RegistrationNumber == rf.RegistrationNumber).Select(c => c.SpecializationId).FirstOrDefault(),
                    PanNumber = rf.PanNumber,
                    AadhaarNumber = rf.AadhaarNumber
                }).Where(e => e.Department != null)
                                                 .ToList();

                foreach (var item in collegeIntakeExisting)
                {
                    CollegeFacultyWithIntakeReport intakedetails = new CollegeFacultyWithIntakeReport();
                    int phdFaculty = 0;
                    int pgFaculty = 0;
                    int ugFaculty = 0;

                    intakedetails.collegeId = item.collegeId;
                    intakedetails.collegeCode = jntuh_college.Where(c => c.id == item.collegeId).Select(c => c.collegeCode).FirstOrDefault();
                    intakedetails.collegeName = jntuh_college.Where(c => c.id == item.collegeId).Select(c => c.collegeName).FirstOrDefault();
                    intakedetails.Degree = item.Degree;
                    intakedetails.Department = item.Department;
                    intakedetails.Specialization = item.Specialization;
                    intakedetails.specializationId = item.specializationId;
                    intakedetails.DepartmentID = item.DepartmentID;
                    intakedetails.shiftId = item.shiftId;

                    intakedetails.approvedIntake1 = GetIntake(item.collegeId, AY1, item.specializationId, item.shiftId, 1);
                    intakedetails.approvedIntake2 = GetIntake(item.collegeId, AY2, item.specializationId, item.shiftId, 1);
                    intakedetails.approvedIntake3 = GetIntake(item.collegeId, AY3, item.specializationId, item.shiftId, 1);
                    intakedetails.approvedIntake4 = GetIntake(item.collegeId, AY4, item.specializationId, item.shiftId, 1);
                    facultystudentRatio = Convert.ToInt32(jntuh_faculty_student_ratio_norms.Where(fn => fn.degreeId == item.degreeID).Select(fn => fn.Norms).FirstOrDefault().Split(':')[1].ToString());

                    if (item.Degree == "B.Tech" || item.Degree == "B.Pharmacy")
                    {
                        intakedetails.totalIntake = (intakedetails.approvedIntake2) + (intakedetails.approvedIntake3) + (intakedetails.approvedIntake4);
                        facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) / Convert.ToDecimal(facultystudentRatio);
                    }
                    else if (item.Degree == "M.Tech" || item.Degree == "M.Pharmacy" || item.Degree == "MBA")
                    {
                        intakedetails.totalIntake = (intakedetails.approvedIntake1) + (intakedetails.approvedIntake2);
                        facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) / Convert.ToDecimal(facultystudentRatio);

                    }
                    else if (item.Degree == "MCA")
                    {
                        intakedetails.totalIntake = (intakedetails.approvedIntake1) + (intakedetails.approvedIntake2) + (intakedetails.approvedIntake3);
                        facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) / Convert.ToDecimal(facultystudentRatio);
                    }
                    else //MAM MTM Pharm.D Pharm.D PB
                    {
                        intakedetails.totalIntake = (intakedetails.approvedIntake1) + (intakedetails.approvedIntake2) + (intakedetails.approvedIntake3) + (intakedetails.approvedIntake4);
                        facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) / Convert.ToDecimal(facultystudentRatio);
                    }

                    intakedetails.requiredFaculty = Math.Round(facultyRatio, 2);
                    intakedetails.degreeDisplayOrder = item.degreeDisplayOrder;

                    //====================================
                    // intakedetails.specializationWiseFaculty = jntuh_registered_faculty.Where(s => s.SpecializationId == item.specializationId).Count();

                    string strdegreetype = jntuh_degree.Where(d => d.degree == item.Degree).Select(d => d.jntuh_degree_type.degreeType).FirstOrDefault();
                    if (strdegreetype == "UG")
                    {
                        if (item.Degree == "B.Pharmacy")
                        {
                            intakedetails.specializationWiseFaculty = jntuh_registered_faculty.Where(f => f.Department == "Pharmacy" && f.Recruitedfor == "UG").Count();
                        }
                        else
                        {
                            intakedetails.specializationWiseFaculty = jntuh_registered_faculty.Where(f => f.Department == item.Department && f.Recruitedfor == "UG").Count();
                        }
                    }

                    if (strdegreetype == "PG")
                    {
                        if (item.Degree == "M.Pharmacy")
                        {
                            intakedetails.specializationWiseFaculty = jntuh_registered_faculty.Where(f => f.Department == "Pharmacy" && (f.Recruitedfor == "PG" || f.Recruitedfor == "UG&PG") && f.SpecializationId == item.specializationId).Count();
                        }
                        else
                        {
                            intakedetails.specializationWiseFaculty = jntuh_registered_faculty.Where(f => f.Department == item.Department && (f.Recruitedfor == "PG" || f.Recruitedfor == "UG&PG") && f.SpecializationId == item.specializationId).Count();
                        }
                    }
                    intakedetails.id = jntuh_college_faculty_deficiency.Where(fd => fd.CollegeId == collegeId && fd.SpecializationId == item.specializationId && fd.ShiftId == item.shiftId).Select(fd => fd.Id).FirstOrDefault();

                    if (intakedetails.id > 0)
                    {
                        int? swf = jntuh_college_faculty_deficiency.Where(fd => fd.Id == intakedetails.id).Select(fd => fd.SpecializationWiseFaculty).FirstOrDefault();
                        if (swf != null)
                        {
                            intakedetails.specializationWiseFaculty = (int)swf;
                        }
                        intakedetails.deficiency = jntuh_college_faculty_deficiency.Where(fd => fd.Id == intakedetails.id).Select(fd => fd.Deficiency).FirstOrDefault();
                        intakedetails.shortage = jntuh_college_faculty_deficiency.Where(fd => fd.Id == intakedetails.id).Select(fd => fd.Shortage).FirstOrDefault();
                    }

                    //============================================

                    int noPanOrAadhaarcount = 0;

                    if (item.Degree == "B.Pharmacy" || item.Degree == "M.Pharmacy")
                    {
                        ugFaculty = jntuh_registered_faculty.Where(f => "UG" == f.HighestDegree && f.Department == "Pharmacy").Count();
                        pgFaculty = jntuh_registered_faculty.Where(f => ("PG" == f.HighestDegree || "M.Phil" == f.HighestDegree) && f.Department == "Pharmacy").Count();
                        phdFaculty = jntuh_registered_faculty.Where(f => "Ph.D" == f.HighestDegree && f.Department == "Pharmacy").Count();
                        noPanOrAadhaarcount = registeredFaculty.Where(f => f.DepartmentId == pharmacyDeptId && f.PANNumber == null && f.AadhaarNumber == null && (f.isApproved == null || f.isApproved == true)).Count();
                        intakedetails.Department = "Pharmacy";
                    }
                    else
                    {
                        ugFaculty = jntuh_registered_faculty.Where(f => "UG" == f.HighestDegree && f.Department == item.Department).Count();
                        pgFaculty = jntuh_registered_faculty.Where(f => ("PG" == f.HighestDegree || "M.Phil" == f.HighestDegree) && f.Department == item.Department).Count();
                        phdFaculty = jntuh_registered_faculty.Where(f => "Ph.D" == f.HighestDegree && f.Department == item.Department).Count();
                        noPanOrAadhaarcount = registeredFaculty.Where(f => f.DepartmentId == item.DepartmentID && f.PANNumber == null && f.AadhaarNumber == null && (f.isApproved == null || f.isApproved == true)).Count();
                    }

                    intakedetails.phdFaculty = phdFaculty;
                    intakedetails.pgFaculty = pgFaculty;
                    intakedetails.ugFaculty = ugFaculty;
                    intakedetails.totalFaculty = (ugFaculty + pgFaculty + phdFaculty);
                    intakedetails.facultyWithoutPANAndAadhaar = noPanOrAadhaarcount;
                    //=============//

                    intakedetailsList.Add(intakedetails);
                }

                intakedetailsList = intakedetailsList.OrderBy(ei => ei.Department).ThenBy(ei => ei.degreeDisplayOrder).ThenBy(ei => ei.Specialization).ThenBy(ei => ei.shiftId).ToList();

                string[] strOtherDepartments = { "English", "Mathematics", "Physics", "Chemistry", "Others" };
                int btechdegreecount = intakedetailsList.Where(d => d.Degree == "B.Tech").Count();
                if (btechdegreecount != 0)
                {
                    foreach (var department in strOtherDepartments)
                    {
                        int speId = jntuh_specialization.Where(s => s.jntuh_department.departmentName == department).Select(s => s.id).FirstOrDefault();
                        int ugFaculty = jntuh_registered_faculty.Where(f => "UG" == f.HighestDegree && f.Department == department).Count();
                        int pgFaculty = jntuh_registered_faculty.Where(f => ("PG" == f.HighestDegree || "M.Phil" == f.HighestDegree) && f.Department == department).Count();
                        int phdFaculty = jntuh_registered_faculty.Where(f => "Ph.D" == f.HighestDegree && f.Department == department).Count();

                        int facultydeficiencyId = jntuh_college_faculty_deficiency.Where(fd => fd.CollegeId == collegeId && fd.SpecializationId == speId && fd.ShiftId == 1).Select(fd => fd.Id).FirstOrDefault();
                        if (facultydeficiencyId == 0)
                        {
                            intakedetailsList.Add(new CollegeFacultyWithIntakeReport { collegeId = (int)collegeId, Degree = "B.Tech", Department = department, Specialization = department, ugFaculty = ugFaculty, pgFaculty = pgFaculty, phdFaculty = phdFaculty, totalFaculty = ugFaculty + pgFaculty + phdFaculty, specializationId = speId, shiftId = 1 });
                        }
                        else
                        {
                            int? swf = jntuh_college_faculty_deficiency.Where(fd => fd.Id == facultydeficiencyId).Select(fd => fd.SpecializationWiseFaculty).FirstOrDefault();
                            bool deficiency = jntuh_college_faculty_deficiency.Where(fd => fd.Id == facultydeficiencyId).Select(fd => fd.Deficiency).FirstOrDefault();
                            int shortage = jntuh_college_faculty_deficiency.Where(fd => fd.Id == facultydeficiencyId).Select(fd => fd.Shortage).FirstOrDefault();
                            intakedetailsList.Add(new CollegeFacultyWithIntakeReport { id = facultydeficiencyId, collegeId = (int)collegeId, Degree = "B.Tech", Department = department, Specialization = department, ugFaculty = ugFaculty, pgFaculty = pgFaculty, phdFaculty = phdFaculty, totalFaculty = ugFaculty + pgFaculty + phdFaculty, specializationId = speId, shiftId = 1, specializationWiseFaculty = (int)swf, deficiency = deficiency, shortage = shortage });
                        }
                    }
                }
            }

            return intakedetailsList;
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

            public int approvedIntake1 { get; set; }
            public int approvedIntake2 { get; set; }
            public int approvedIntake3 { get; set; }
            public int approvedIntake4 { get; set; }
            public int totalIntake { get; set; }
            public decimal requiredFaculty { get; set; }
            public int phdFaculty { get; set; }
            public int pgFaculty { get; set; }
            public int ugFaculty { get; set; }
            public int totalFaculty { get; set; }
            public int specializationWiseFaculty { get; set; }
            public int facultyWithoutPANAndAadhaar { get; set; }

            public bool isActive { get; set; }
            public DateTime? nbaFrom { get; set; }
            public DateTime? nbaTo { get; set; }

            public bool? deficiency { get; set; }
            public int shortage { get; set; }
        }
        private int GetIntake(int collegeId, int academicYearId, int specializationId, int shiftId, int flag)
        {
            int intake = 0;

            //approved
            if (flag == 1)
            {
                intake = db.jntuh_college_intake_existing.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == shiftId).Select(i => i.approvedIntake).FirstOrDefault();

            }
            else //admitted
            {
                intake = db.jntuh_college_intake_existing.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == shiftId).Select(i => i.admittedIntake).FirstOrDefault();
            }

            return intake;
        }

        //LABS
        public string DeficienciesInLabs(int? collegeID)
        {
            string labs = string.Empty; string flag = string.Empty;
            string[] departments = new string[] { "English", "Mathematics", "Physics", "Chemistry", "Others" };

            List<Lab> labsCount = collegeLabs(collegeID);
            List<Lab> labsCount1 = labsCount.Where(c => c.degree == "B.Tech" && !departments.Contains(c.department)).ToList();

            var deficiencies = labsCount1.GroupBy(l => new { l.degree, l.department, l.specializationName })
                                         .Select(l => new { degree = l.Key.degree, department = l.Key.department, specializationName = l.Key.specializationName, deficiencies = string.Empty })
                                         .ToList();

            int totalSpecializations = deficiencies.Count();
            int totalDeficiencySpecializations = 0;
            int totalApprovedSpecializations = 0;
            int courseApproved = 0;

            List<jntuh_college> colleges = db.jntuh_college.ToList();

            foreach (var item in deficiencies)
            {
                courseApproved = 0;
                string degreeType = db.jntuh_degree.Where(d => d.degree == item.degree).Select(d => d.jntuh_degree_type.degreeType).FirstOrDefault();

                var labsWithDeficiency = labsCount.Where(l => l.degree == item.degree && l.department == item.department && l.specializationName == item.specializationName && l.deficiency == true)
                    .Select(l => new { Deficiency = degreeType.Equals("PG") ? "No Equipement Uploaded" : l.year + "-" + l.Semester + "-" + l.LabName }).Select(l => l.Deficiency).ToList();

                //if (courseApproved == 1)
                //{
                labs += "<tr>";
                labs += "<td style='text-align: center; width: 5%; '>" + (deficiencies.IndexOf(item) + 1) + "</td>";
                labs += "<td style=''>" + colleges.Where(c => c.id == collegeID).Select(c => c.collegeCode).FirstOrDefault() + "</td>";
                labs += "<td style=''>" + colleges.Where(c => c.id == collegeID).Select(c => c.collegeName).FirstOrDefault() + "</td>";
                labs += "<td style=''>" + item.degree + "</td>";
                labs += "<td style=''>" + item.department + "</td>";
                labs += "<td style=''>" + item.specializationName + "</td>";
                labs += "<td style='; text-align: center'>" + (labsWithDeficiency.Count() == 0 ? "NIL" : String.Join(", ", labsWithDeficiency)) + "</td>";
                labs += "</tr>";
                //}

                if (labsWithDeficiency.Count() == 0)
                {
                    totalApprovedSpecializations += 1;
                    courseApproved = 1;
                }

                if (labsWithDeficiency.Count() > 0)
                {
                    totalDeficiencySpecializations += 1;
                }
            }

            if (totalSpecializations > 0)
            {
                flag = (totalSpecializations == totalDeficiencySpecializations) ? "YES" : "NO";
                //flag = (totalSpecializations == totalApprovedSpecializations) ? "YES" : "NO";
                //flag = (totalApprovedSpecializations > 0) ? "NO" : "YES";
            }

            return flag + "$" + labs;
        }
        public List<Lab> collegeLabs(int? collegeID)
        {
            List<Lab> lstlaboratories = new List<Lab>();

            var jntuh_college_laboratories_deficiency = db.jntuh_college_laboratories_deficiency.Where(c => c.CollegeId == collegeID).ToList();

            string strcollegecode = db.jntuh_college_randamcodes.Where(r => r.CollegeId == collegeID).Select(r => r.RandamCode).FirstOrDefault();
            int[] specializationIds = db.jntuh_college_intake_existing.Where(e => e.collegeId == collegeID).Select(e => e.specializationId).Distinct().ToArray();
            List<Lab> collegeLabMaster = db.jntuh_lab_master.AsNoTracking()
                                                        .Where(l => specializationIds.Contains(l.SpecializationID))
                                                        .Select(l => new Lab
                                                        {
                                                            ////// EquipmentID=l.id,                                                               
                                                            degreeId = l.DegreeID,
                                                            degree = l.jntuh_degree.degree,
                                                            degreeDisplayOrder = l.jntuh_degree.degreeDisplayOrder,
                                                            departmentId = l.DepartmentID,
                                                            department = l.jntuh_department.departmentName,
                                                            specializationId = l.SpecializationID,
                                                            specializationName = l.jntuh_specialization.specializationName,
                                                            year = l.Year,
                                                            Semester = l.Semester,
                                                            Labcode = l.Labcode,
                                                            LabName = l.LabName
                                                        })
                                                        .OrderBy(l => l.degreeDisplayOrder)
                                                        .ThenBy(l => l.department)
                                                        .ThenBy(l => l.specializationName)
                                                        .ThenBy(l => l.year).ThenBy(l => l.Semester).Distinct()
                                                        .ToList();

            var jntuh_college_laboratories = db.jntuh_college_laboratories.AsNoTracking().Where(l => l.CollegeID == collegeID).ToList();

            foreach (var item in collegeLabMaster)
            {
                Lab lstlabs = new Lab();
                lstlabs.collegeId = (int)collegeID;
                lstlabs.EquipmentID = item.EquipmentID;
                lstlabs.degree = item.degree;
                lstlabs.department = item.department;
                lstlabs.specializationName = item.specializationName;
                lstlabs.specializationId = item.specializationId;
                lstlabs.Semester = item.Semester;
                lstlabs.year = item.year;
                lstlabs.Labcode = item.Labcode;
                //lstlabs.RandomId = (int)rid;
                lstlabs.LabName = item.LabName;
                lstlabs.EquipmentNo = 1;
                lstlabs.RandomCode = strcollegecode;
                lstlabs.degreeDisplayOrder = item.degreeDisplayOrder;
                if (jntuh_college_laboratories_deficiency.Count() != 0)
                {
                    lstlabs.deficiency = jntuh_college_laboratories_deficiency.Where(ld => ld.LabCode == item.Labcode && ld.CollegeId == collegeID).Select(ld => ld.Deficiency).FirstOrDefault();
                    lstlabs.id = jntuh_college_laboratories_deficiency.Where(ld => ld.LabCode == item.Labcode && ld.CollegeId == collegeID).Select(ld => ld.Id).FirstOrDefault();
                }
                else
                {
                    lstlabs.deficiency = null;
                    lstlabs.id = 0;
                }
                lstlaboratories.Add(lstlabs);
            }

            lstlaboratories = lstlaboratories.OrderBy(l => l.degreeDisplayOrder).ThenBy(l => l.department).ThenBy(l => l.specializationName)
                                             .ThenBy(l => l.year).ThenBy(l => l.Semester).Distinct().ToList();

            return lstlaboratories;
        }
        public string CollegeLabsAnnexure(int? collegeID)
        {
            string annexure = string.Empty;

            int[] specializationIds = db.jntuh_college_intake_existing.Where(e => e.collegeId == collegeID).Select(e => e.specializationId).Distinct().ToArray();
            List<Lab> collegeLabMaster = db.jntuh_lab_master.AsNoTracking()
                                                        .Where(l => specializationIds.Contains(l.SpecializationID))
                                                        .Select(l => new Lab
                                                        {
                                                            EquipmentID = l.id,
                                                            degreeId = l.DegreeID,
                                                            degree = l.jntuh_degree.degree,
                                                            degreeDisplayOrder = l.jntuh_degree.degreeDisplayOrder,
                                                            departmentId = l.DepartmentID,
                                                            department = l.jntuh_department.departmentName,
                                                            specializationId = l.SpecializationID,
                                                            specializationName = l.jntuh_specialization.specializationName,
                                                            year = l.Year,
                                                            Semester = l.Semester,
                                                            Labcode = l.Labcode,
                                                            LabName = l.LabName,
                                                            EquipmentName = l.EquipmentName
                                                        })
                                                        .OrderBy(l => l.degreeDisplayOrder)
                                                        .ThenBy(l => l.department)
                                                        .ThenBy(l => l.specializationName)
                                                        .ThenBy(l => l.year).ThenBy(l => l.Semester)
                                                        .ToList();

            var collegeEquipments = db.jntuh_college_laboratories.Where(l => l.CollegeID == collegeID).Select(l => l.EquipmentID).Distinct().ToArray();

            var list = collegeLabMaster.Where(c => !collegeEquipments.Contains(c.EquipmentID)).Select(c => new { LabCode = c.Labcode, LabName = c.LabName, EquipmentName = c.EquipmentName })
                                       .OrderBy(c => c.LabName).ThenBy(c => c.EquipmentName).Distinct().ToList();

            var labDeficiencies = db.jntuh_college_laboratories_deficiency.Where(ld => ld.CollegeId == collegeID && ld.Deficiency == true).Select(ld => ld.LabCode).ToArray();

            list = list.Where(l => labDeficiencies.Contains(l.LabCode)).ToList();

            if (list.Count() > 0)
            {
                annexure += "<table width='100%' border='1' cellpadding='5' cellspacing='0'>";
                annexure += "<tr>";
                annexure += "<th align='center' colspan='3'>LIST OF EQUIPMENT NOT AVAILABLE</th>";
                annexure += "</tr>";
                annexure += "<tr>";
                annexure += "<th align='left'>S.No</th><th align='left'>Lab Name</th><th align='left'>Equipment Name</th>";
                annexure += "</tr>";

                foreach (var item in list.Where(l => labDeficiencies.Contains(l.LabCode)).ToList())
                {
                    annexure += "<tr>";
                    annexure += "<td align='left'>" + (list.IndexOf(item) + 1) + "</td><td align='left'>" + item.LabName + "</td><td align='left'>" + item.EquipmentName + "</td>";
                    annexure += "</tr>";
                }

                annexure += "</table>";
            }

            return annexure;
        }



    }
}
