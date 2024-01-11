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
    public class DeficiencyReportWordNewController : Controller
    {
        private uaaasDBContext db = new uaaasDBContext();

        public ActionResult Deficiencies(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                int collegeID = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(id.ToString(), System.Web.Configuration.WebConfigurationManager.AppSettings["CryptoKey"]));

                Response.ClearContent();
                Response.ClearHeaders();
                Response.Buffer = true;
                string collegeCode = db.jntuh_college.Where(c => c.isActive == true && c.id == collegeID).Select(c => c.collegeCode).FirstOrDefault();
                Response.AddHeader("content-disposition", "attachment; filename=" + collegeCode + "-EvaluationReport" + ".doc");
                Response.ContentType = "application/vnd.ms-word "; 
                Response.Charset = string.Empty;
                StringBuilder str = new StringBuilder();
                str.Append(Header());
                str.Append("<br />");
                str.Append(CollegeInformation(collegeID));
                str.Append("<br />");
                str.Append(Principal(collegeID));
                str.Append("<br />");
                //str.Append(CommitteeMembers(collegeID));
                //str.Append("<br />");
                str.Append(DeficienciesInFaculty(collegeID));
                str.Append("<br />");
                str.Append(DeficienciesInLabs(collegeID));
                str.Append("<br />");
                //str.Append(CollegeLabsAnnexure(collegeID));
                //str.Append("<br />");
                //string[] courses = CollegeCoursesAllClear(collegeID).Split('$');
                //str.Append(courses[1]);

                Document pdfDoc = new Document(PageSize.A4, 60, 50, 60, 60);

                pdfDoc.SetPageSize(iTextSharp.text.PageSize.A4.Rotate());
                pdfDoc.SetMargins(60, 50, 60, 60);

                string path = Server.MapPath("~/Content/PDFReports/EvaluationReports/");

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
            header += "<tr>";
            header += "<td rowspan='3' align='center' width='20%'><img src='http://jntuhaac.in/Content/Images/new_logo.jpg' height='70' width='70' style='text-align: center' align='middle' /></td>";
            header += "<td align='center' width='80%' style='font-size: 14px; font-weight: normal;'><b>JAWAHARLAL NEHRU TECHNOLOGICAL UNIVERSITY HYDERABAD</b></td>";
            header += "</tr>";
            header += "<tr>";
            header += "<td align='center' style='font-size: 11px; font-weight: normal;'><b>KUKATPALLY, HYDERABAD, TELANGANA, INDIA - 500 085</b></td>";
            header += "</tr>";
            header += "<tr>";
            header += "<td  align='center' style='font-weight: normal;'><u><b>EVALUATION REPORT</b></u></td>";
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
            string principal = string.Empty;

            jntuh_college college = db.jntuh_college.Where(c => c.isActive == true && c.id == collegeID).Select(c => c).FirstOrDefault();

            principal += "<table width='100%' border='0' cellpadding='5' cellspacing='0'>";
            principal += "<tr>";
            principal += "<td align='left'><b>Principal: </b><img alt='' src='http://jntuhaac.in/Content/Images/checkbox_no.png' height='16' width='16' /> Qualified &nbsp; <img alt='' src='http://112.133.193.228:75//Content/Images/checkbox_no.png' height='16' width='16' /> Ratified &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; Deficiency: <img alt='' src='http://112.133.193.228:75//Content/Images/checkbox_no.png' height='16' width='16' /> Yes <img alt='' src='http://112.133.193.228:75//Content/Images/checkbox_no.png' height='16' width='16' /> No  &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; CD Received: <img alt='' src='http://112.133.193.228:75//Content/Images/checkbox_no.png' height='16' width='16' /> Yes <img alt='' src='http://112.133.193.228:75//Content/Images/checkbox_no.png' height='16' width='16' /> No";
            principal += "</tr>";

            principal += "</table>";

            return principal;
        }

        //public string CommitteeMembers(int? collegeID)
        //{
        //    string members = string.Empty;

        //    members += "<table width='100%' border='0' cellpadding='5' cellspacing='0'>";
        //    members += "<tr>";
        //    members += "<td align='left'><b>Members of FFC Team: </b>";
        //    members += "</tr>";

        //    string collegeCode = db.jntuh_college.Find(collegeID).collegeCode;
        //    committeemembers2015 committee = db.committeemembers2015.Where(c => c.CC == collegeCode).Select(c => c).FirstOrDefault();

        //    if (committee != null)
        //    {
        //        string strCommittee = string.Empty;

        //        if (!string.IsNullOrEmpty(committee.TeamMember1))
        //        {
        //            strCommittee += "1. " + committee.TeamMember1 + "<br />";
        //        }
        //        if (!string.IsNullOrEmpty(committee.TeamMember2))
        //        {
        //            strCommittee += "2. " + committee.TeamMember2 + "<br />";
        //        }
        //        if (!string.IsNullOrEmpty(committee.TeamMember3))
        //        {
        //            strCommittee += "3. " + committee.TeamMember3 + "<br />";
        //        }
        //        if (!string.IsNullOrEmpty(committee.TeamMember4))
        //        {
        //            strCommittee += "4. " + committee.TeamMember4 + "<br />";
        //        }

        //        members += "<tr>";
        //        members += "<td align='left'>" + strCommittee + "</td>";
        //        members += "</tr>";
        //    }

        //    members += "</table>";

        //    return members;
        //}

        public string DeficienciesInFaculty(int? collegeID)
        {
            string faculty = string.Empty;

            faculty += "<table width='100%' border='0' cellpadding='5' cellspacing='0'>";
            faculty += "<tr>";
            faculty += "<td align='left'><b><u>Deficiencies in Faculty</u></b> (Department/ Specialization Wise):";
            faculty += "</tr>";
            faculty += "</table>";

            List<CollegeFacultyWithIntakeReport> facultyCounts = collegeFaculty(collegeID).Where(c => c.shiftId == 1).ToList();

            var count = facultyCounts.Count();
            var distDeptcount = 1;
            var deptloop = 1;
            decimal departmentWiseRequiredFaculty = 0;

            string[] departments = new string[] { "English", "Mathematics", "Physics", "Chemistry", "Others" };

            int totalBtechFirstYearIntake = facultyCounts.Where(d => d.Degree == "B.Tech").Select(d => d.approvedIntake1).Sum();
            var firstYearRequired = Math.Ceiling((double)totalBtechFirstYearIntake / 120);
            int remainingFaculty = 0;
            int remainingPHDFaculty = 0;

            faculty += "<table width='100%' border='1' cellpadding='3' cellspacing='0' style='border-color: #ccc;'>";
            faculty += "<tr>";
            faculty += "<th style='text-align: center; vertical-align: top;'>SNo</th>";
            faculty += "<th style='text-align: left; vertical-align: top;' >Department</th>";
            faculty += "<th style='text-align: left; vertical-align: top;' >Degree</th>";
            faculty += "<th style='text-align: left; vertical-align: top;' >Specialization</th>";
            faculty += "<th style='text-align: center; vertical-align: top;' >Total Intake *</th>";
            faculty += "<th style='text-align: center; vertical-align: top;' >Specialization Wise Faculty Required</th>";
            faculty += "<th style='text-align: center; vertical-align: top;' >Specialization Wise Faculty Available</th>";
            faculty += "<th style='text-align: center; vertical-align: top;' >Deficiency</th>";
            faculty += "<th style='text-align: center; vertical-align: top;' >Deficiency of Ph.D faculty</th>";
            faculty += "</tr>";

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
                        minimumRequirementMet = "YES/NO";
                        remainingFaculty = tFaculty - rFaculty;
                        adjustedFaculty = rFaculty;
                    }
                    else
                    {
                        minimumRequirementMet = "YES/NO";
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
                        minimumRequirementMet = "YES/NO";
                        remainingFaculty = remainingFaculty - rFaculty;
                        adjustedFaculty = rFaculty;
                    }
                    else
                    {
                        minimumRequirementMet = "YES/NO";
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

                faculty += "<tr>";
                faculty += "<td class='col2' style='text-align: center; vertical-align: top;width:10px' rowspan='1'>" + (facultyCounts.IndexOf(item) + 1) + "</td>";
                faculty += "<td class='col2' style='text-align: left; vertical-align: top;'>" + item.Department + "</td>";
                faculty += "<td class='col2' style='text-align: left; vertical-align: top;'>" + item.Degree + "</td>";
                faculty += "<td class='col2' style='text-align: left; vertical-align: top;'>" + item.Specialization + "</td>";

                if (departments.Contains(item.Department))
                {
                    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + totalBtechFirstYearIntake + "</td>";
                    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + Math.Ceiling(firstYearRequired) + "</td>";
                }
                else
                {
                    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + item.totalIntake + "</td>";
                    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + Math.Ceiling(item.requiredFaculty) + "</td>";
                }

                faculty += "<td class='col2' style='text-align: center; vertical-align: top;'></td>";
                faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + minimumRequirementMet + "</td>";
               

                if (adjustedPHDFaculty > 0)
                {
                    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>YES/NO</td>";
                }
                else if (degreeType.Equals("PG"))
                {
                    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>YES/NO</td>";
                }
                else
                {
                    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>YES/NO</td>";
                }

                faculty += "</tr>";

                deptloop++;
            }

            faculty += "</table>";

            faculty += "<table width='100%' border='0' cellpadding='5' cellspacing='0'>";
            faculty += "<tr>";
            faculty += "<td align='left'>* II, III & IV Year for B.Tech; I & II Year for M.Tech";
            faculty += "</tr>";
            faculty += "</table>";

            return faculty;
        }

        public string DeficienciesInLabs(int? collegeID)
        {
            string labs = string.Empty;

            labs += "<table width='100%' border='0' cellpadding='5' cellspacing='0'>";
            labs += "<tr>";
            labs += "<td align='left'><b><u>Deficiencies in Laboratory</u></b> (Department/ Specialization Wise):";
            labs += "</tr>";
            labs += "</table>";

            List<Lab> labsCount = collegeLabs(collegeID);

            var deficiencies = labsCount.GroupBy(l => new { l.degree, l.department, l.specializationName })
                                        .Select(l => new { degree = l.Key.degree, department = l.Key.department, specializationName = l.Key.specializationName, deficiencies = string.Empty })
                                        .ToList();

            labs += "<table width='100%' border='1' cellpadding='3' cellspacing='0' style='border-color: #ccc;'>";
            labs += "<tr>";
            labs += "<th style='text-align: center; width: 5%;'>S.No</th>";
            labs += "<th style='text-align: left; width: 10%;'>Degree</th>";
            labs += "<th style='text-align: left; width: 10%;'>Department</th>";
            labs += "<th style='text-align: left; width: 20%;'>Specialization</th>";
            labs += "<th style='text-align: center; '>Names of the Labs with Deficiency </th>";
            labs += "</tr>";

            foreach (var item in deficiencies)
            {

                labs += "<tr>";
                labs += "<td style='text-align: center; width: 5%; '>" + (deficiencies.IndexOf(item) + 1) + "</td>";
                labs += "<td style=''>" + item.degree + "</td>";
                labs += "<td style=''>" + item.department + "</td>";
                labs += "<td style=''>" + item.specializationName + "</td>";

                string degreeType = db.jntuh_degree.Where(d => d.degree == item.degree).Select(d => d.jntuh_degree_type.degreeType).FirstOrDefault();

                var labsWithDeficiency = labsCount.Where(l => l.degree == item.degree && l.department == item.department && l.specializationName == item.specializationName && l.deficiency == true)
                    .Select(l => new { Deficiency = degreeType.Equals("PG") ? "No Equipement Uploaded" : l.year + "-" + l.Semester + "-" + l.LabName }).Select(l => l.Deficiency).ToList();

                labs += "<td style='; text-align: center'>&nbsp;<br />&nbsp;<br />&nbsp;<br /></td>";
               //// labs += "<td style='; text-align: center'>" + (labsWithDeficiency.Count() == 0 ? "NIL" : String.Join(", ", labsWithDeficiency)) + "</td>";
                labs += "</tr>";
            }

            labs += "</table>";

            return labs;
        }

        //FACULTY
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
                var jntuh_college_faculty_registered = db.jntuh_college_faculty_registered.Where(cf => cf.collegeId == collegeId).ToList();
                string[] strRegnos = jntuh_college_faculty_registered.Select(cf => cf.RegistrationNumber).ToArray();

                //education categoryIds UG,PG,PHD...........

                int pharmacyDeptId = db.jntuh_department.Where(d => d.departmentName == "Pharmacy").Select(d => d.id).FirstOrDefault();

                var jntuh_education_category = db.jntuh_education_category.ToList();

                var registeredFaculty = db.jntuh_registered_faculty.Where(rf => strRegnos.Contains(rf.RegistrationNumber) && (rf.collegeId == null || rf.collegeId == collegeId)).ToList();
                //Reg nos related online facultyIds
                var jntuh_registered_faculty1 = registeredFaculty.Where(rf => rf.DepartmentId != null && (rf.Absent == false && rf.NoSCM == false && rf.NoForm16 == false
                    && rf.InvalidPANNumber == false && rf.NotQualifiedAsperAICTE == false && rf.IncompleteCertificates == false)
                    && (rf.PANNumber != null || rf.AadhaarNumber != null))
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
            if (flag == 1 && academicYearId != 8)
            {
                intake = db.jntuh_college_intake_existing.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == shiftId).Select(i => i.approvedIntake).FirstOrDefault();

            }
            else if (flag == 1 && academicYearId == 8)
            {
                var inta = db.jntuh_college_intake_existing.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == shiftId).FirstOrDefault();
                if (inta != null)
                {
                    intake = (int)inta.proposedIntake;
                }

            }
            else //admitted
            {
                intake = db.jntuh_college_intake_existing.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == shiftId).Select(i => i.admittedIntake).FirstOrDefault();
            }

            return intake;
        }

        //LABS
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

        //public string CollegeLabsAnnexure(int? collegeID)
        //{
        //    string annexure = string.Empty;

        //    int[] specializationIds = db.jntuh_college_intake_existing.Where(e => e.collegeId == collegeID).Select(e => e.specializationId).Distinct().ToArray();
        //    List<Lab> collegeLabMaster = db.jntuh_lab_master.AsNoTracking()
        //                                                .Where(l => specializationIds.Contains(l.SpecializationID))
        //                                                .Select(l => new Lab
        //                                                {
        //                                                    EquipmentID = l.id,
        //                                                    degreeId = l.DegreeID,
        //                                                    degree = l.jntuh_degree.degree,
        //                                                    degreeDisplayOrder = l.jntuh_degree.degreeDisplayOrder,
        //                                                    departmentId = l.DepartmentID,
        //                                                    department = l.jntuh_department.departmentName,
        //                                                    specializationId = l.SpecializationID,
        //                                                    specializationName = l.jntuh_specialization.specializationName,
        //                                                    year = l.Year,
        //                                                    Semester = l.Semester,
        //                                                    Labcode = l.Labcode,
        //                                                    LabName = l.LabName,
        //                                                    EquipmentName = l.EquipmentName
        //                                                })
        //                                                .OrderBy(l => l.degreeDisplayOrder)
        //                                                .ThenBy(l => l.department)
        //                                                .ThenBy(l => l.specializationName)
        //                                                .ThenBy(l => l.year).ThenBy(l => l.Semester)
        //                                                .ToList();

        //    var collegeEquipments = db.jntuh_college_laboratories.Where(l => l.CollegeID == collegeID).Select(l => l.EquipmentID).Distinct().ToArray();

        //    var list = collegeLabMaster.Where(c => !collegeEquipments.Contains(c.EquipmentID)).Select(c => new { LabCode = c.Labcode, LabName = c.LabName, EquipmentName = c.EquipmentName })
        //                               .OrderBy(c => c.LabName).ThenBy(c => c.EquipmentName).Distinct().ToList();

        //    var labDeficiencies = db.jntuh_college_laboratories_deficiency.Where(ld => ld.CollegeId == collegeID && ld.Deficiency == true).Select(ld => ld.LabCode).ToArray();

        //    list = list.Where(l => labDeficiencies.Contains(l.LabCode)).ToList();

        //    //if (list.Count() > 0)
        //    //{
        //    //    annexure += "<table width='100%' border='1' cellpadding='5' cellspacing='0'>";
        //    //    annexure += "<tr>";
        //    //    annexure += "<th align='center' colspan='3'>LIST OF EQUIPMENT NOT AVAILABLE</th>";
        //    //    annexure += "</tr>";
        //    //    annexure += "<tr>";
        //    //    annexure += "<th align='left'>S.No</th><th align='left'>Lab Name</th><th align='left'>Equipment Name</th>";
        //    //    annexure += "</tr>";

        //    //    foreach (var item in list.Where(l => labDeficiencies.Contains(l.LabCode)).ToList())
        //    //    {
        //    //        annexure += "<tr>";
        //    //        annexure += "<td align='left'>" + (list.IndexOf(item) + 1) + "</td><td align='left'>" + item.LabName + "</td><td align='left'>" + item.EquipmentName + "</td>";
        //    //        //annexure += "<td align='left'>" + (list.IndexOf(item) + 1) + "</td><td align='left'>" + item.LabCode + "</td><td align='left'>" + item.LabName + "</td><td align='left'>" + item.EquipmentName + "</td>";
        //    //        annexure += "</tr>";
        //    //    }

        //    //    annexure += "</table>";
        //    //}

        //    return annexure;
        //}

        //========================//
        public ActionResult DeficienciesNew(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                int collegeID = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(id.ToString(), System.Web.Configuration.WebConfigurationManager.AppSettings["CryptoKey"]));

                Response.ClearContent();
                Response.ClearHeaders();
                Response.Buffer = true;
                string collegeCode = db.jntuh_college.Where(c => c.isActive == true && c.id == collegeID).Select(c => c.collegeCode).FirstOrDefault();
                Response.AddHeader("content-disposition", "attachment; filename=" + collegeCode + "- DeficiencyCorrections" + ".doc");
                Response.ContentType = "application/vnd.ms-word ";
                Response.Charset = string.Empty;
                StringBuilder str = new StringBuilder();
                str.Append(Header());
                str.Append("<br />");
                str.Append(CollegeInformation(collegeID));
                str.Append("<br />");
                str.Append(Principal(collegeID));
                str.Append("<br />");
                //str.Append(CommitteeMembers(collegeID));
                //str.Append("<br />");
                str.Append(DeficienciesInFacultyNew(collegeID));
                str.Append("<br />");
                str.Append(DeficienciesInLabsNew(collegeID));
                str.Append("<br />");
                str.Append(CollegeLabsAnnexureNew(collegeID));
                str.Append("<br />");

                Document pdfDoc = new Document(PageSize.A4, 60, 50, 60, 60);

                pdfDoc.SetPageSize(iTextSharp.text.PageSize.A4.Rotate());
                pdfDoc.SetMargins(60, 50, 60, 60);

                string path = Server.MapPath("~/Content/PDFReports/EvaluationReports/");

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

        public string DeficienciesInFacultyNew(int? collegeID)
        {
            string faculty = string.Empty;

            faculty += "<table width='100%' border='0' cellpadding='5' cellspacing='0'>";
            faculty += "<tr>";
            faculty += "<td align='left'><b><u>Deficiencies in Faculty</u></b> (Department/ Specialization Wise):";
            faculty += "</tr>";
            faculty += "</table>";

            List<CollegeFacultyWithIntakeReport> facultyCounts = collegeFaculty(collegeID).Where(c => c.shiftId == 1).ToList();

            var count = facultyCounts.Count();
            var distDeptcount = 1;
            var deptloop = 1;
            decimal departmentWiseRequiredFaculty = 0;

            string[] departments = new string[] { "English", "Mathematics", "Physics", "Chemistry", "Others" };

            int totalBtechFirstYearIntake = facultyCounts.Where(d => d.Degree == "B.Tech").Select(d => d.approvedIntake1).Sum();
            var firstYearRequired = Math.Ceiling((double)totalBtechFirstYearIntake / 120);
            int remainingFaculty = 0;
            int remainingPHDFaculty = 0;

            faculty += "<table width='100%' border='1' cellpadding='3' cellspacing='0' style='border-color: #ccc;'>";
            faculty += "<tr>";
            faculty += "<th style='text-align: center; vertical-align: top;'>SNo</th>";
            faculty += "<th style='text-align: left; vertical-align: top;' >Department</th>";
            faculty += "<th style='text-align: left; vertical-align: top;' >Degree</th>";
            faculty += "<th style='text-align: left; vertical-align: top;' >Specialization</th>";
            faculty += "<th style='text-align: center; vertical-align: top;' >Total Intake *</th>";
            faculty += "<th style='text-align: center; vertical-align: top;' >Total Intake Corrections</th>";
            faculty += "<th style='text-align: center; vertical-align: top;' >Specialization Wise Faculty Required</th>";
            faculty += "<th style='text-align: center; vertical-align: top;' >Required Faculty Corrections</th>";
            faculty += "<th style='text-align: center; vertical-align: top;' >Specialization Wise Faculty Available</th>";
            faculty += "<th style='text-align: center; vertical-align: top;' >Available Faculty Corrections</th>";
            faculty += "<th style='text-align: center; vertical-align: top;' >Deficiency</th>";
            faculty += "<th style='text-align: center; vertical-align: top;' >Deficiency Corrections</th>";
            faculty += "<th style='text-align: center; vertical-align: top;' >Deficiency of Ph.D faculty</th>";
            faculty += "<th style='text-align: center; vertical-align: top;' >Remarks</th>";
            faculty += "</tr>";

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
                        minimumRequirementMet = "YES/NO";
                        remainingFaculty = tFaculty - rFaculty;
                        adjustedFaculty = rFaculty;
                    }
                    //else
                    //{
                    //    minimumRequirementMet = "YES";
                    //    adjustedFaculty = tFaculty;
                    //    facultyShortage = rFaculty - tFaculty;
                    //}

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
                        minimumRequirementMet = "YES/NO";
                        remainingFaculty = remainingFaculty - rFaculty;
                        adjustedFaculty = rFaculty;
                    }
                    //else
                    //{
                    //    minimumRequirementMet = "YES";
                    //    adjustedFaculty = remainingFaculty;
                    //    facultyShortage = rFaculty - remainingFaculty;
                    //    remainingFaculty = 0;
                    //}

                    if (remainingPHDFaculty > 0 && degreeType.Equals("PG"))
                    {
                        adjustedPHDFaculty = 1;
                        remainingPHDFaculty = remainingPHDFaculty - 1;
                    }
                }

                faculty += "<tr>";
                faculty += "<td class='col2' style='text-align: center; vertical-align: top;width:10px' rowspan='1'>" + (facultyCounts.IndexOf(item) + 1) + "</td>";
                faculty += "<td class='col2' style='text-align: left; vertical-align: top;'>" + item.Department + "</td>";
                faculty += "<td class='col2' style='text-align: left; vertical-align: top;'>" + item.Degree + "</td>";
                faculty += "<td class='col2' style='text-align: left; vertical-align: top;'>" + item.Specialization + "</td>";

                if (departments.Contains(item.Department))
                {
                    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + totalBtechFirstYearIntake + "</td>";
                    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'></td>";
                    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + Math.Ceiling(firstYearRequired) + "</td>";
                    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'></td>";
                }
                else
                {
                    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + item.totalIntake + "</td>";
                    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'></td>";
                    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + Math.Ceiling(item.requiredFaculty) + "</td>";
                    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'></td>";
                }

                faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + adjustedFaculty + "</td>";
                faculty += "<td class='col2' style='text-align: center; vertical-align: top;'></td>";
                faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + minimumRequirementMet + "</td>";
                faculty += "<td class='col2' style='text-align: center; vertical-align: top;'></td>";

                if (adjustedPHDFaculty > 0)
                {
                    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>NO</td>";
                }
                else if (degreeType.Equals("PG"))
                {
                    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>YES</td>";
                }
                else
                {
                    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>-</td>";
                }
                faculty += "<td class='col2' style='text-align: center; vertical-align: top;'></td>";

                faculty += "</tr>";

                deptloop++;
            }

            faculty += "</table>";

            faculty += "<table width='100%' border='0' cellpadding='5' cellspacing='0'>";
            faculty += "<tr>";
            faculty += "<td align='left'>* II, III & IV Year for B.Tech; I & II Year for M.Tech";
            faculty += "</tr>";
            faculty += "</table>";

            return faculty;
        }

        public string DeficienciesInLabsNew(int? collegeID)
        {
            string labs = string.Empty;

            labs += "<table width='100%' border='0' cellpadding='5' cellspacing='0'>";
            labs += "<tr>";
            labs += "<td align='left'><b><u>Deficiencies in Laboratory</u></b> (Department/ Specialization Wise):";
            labs += "</tr>";
            labs += "</table>";

            List<Lab> labsCount = collegeLabs(collegeID);

            var deficiencies = labsCount.GroupBy(l => new { l.degree, l.department, l.specializationName })
                                        .Select(l => new { degree = l.Key.degree, department = l.Key.department, specializationName = l.Key.specializationName, deficiencies = string.Empty })
                                        .ToList();

            labs += "<table width='100%' border='1' cellpadding='3' cellspacing='0' style='border-color: #ccc;'>";
            labs += "<tr>";
            labs += "<th style='text-align: center; width: 5%;'>S.No</th>";
            labs += "<th style='text-align: left; width: 10%;'>Degree</th>";
            labs += "<th style='text-align: left; width: 10%;'>Department</th>";
            labs += "<th style='text-align: left; width: 20%;'>Specialization</th>";
            labs += "<th style='text-align: center; '>Names of the Labs with Deficiency (Details Annexed)</th>";
            labs += "<th style='text-align: center; vertical-align: top;' colspan='3'>Remarks</th>";
            labs += "</tr>";

            foreach (var item in deficiencies)
            {

                labs += "<tr>";
                labs += "<td style='text-align: center; width: 5%; '>" + (deficiencies.IndexOf(item) + 1) + "</td>";
                labs += "<td style=''>" + item.degree + "</td>";
                labs += "<td style=''>" + item.department + "</td>";
                labs += "<td style=''>" + item.specializationName + "</td>";

                string degreeType = db.jntuh_degree.Where(d => d.degree == item.degree).Select(d => d.jntuh_degree_type.degreeType).FirstOrDefault();

                var labsWithDeficiency = labsCount.Where(l => l.degree == item.degree && l.department == item.department && l.specializationName == item.specializationName && l.deficiency == true)
                    .Select(l => new { Deficiency = degreeType.Equals("PG") ? "No Equipement Uploaded" : l.year + "-" + l.Semester + "-" + l.LabName }).Select(l => l.Deficiency).ToList();

                labs += "<td style='; text-align: center'>" + (labsWithDeficiency.Count() == 0 ? "NIL" : String.Join(", ", labsWithDeficiency)) + "</td>";
                labs += "<td style='; text-align: center' colspan='3'></td>";
                labs += "</tr>";
            }

            labs += "</table>";

            return labs;
        }
        //public string MainlyFacultyLabs(int? collegeID)
        //{
        //    string labs = string.Empty;

        //    labs += "<table width='100%' border='0' cellpadding='5' cellspacing='0'>";
        //    labs += "<tr>";
        //    labs += "<td align='left'><b>Course wise assessment of essential requirement(Mainly Faculty & Labs) as per University norms & regulations.</b> :";
        //    labs += "</tr>";
        //    labs += "</table>";

        //    List<Lab> labsCount = collegeLabs(collegeID);

        //    var deficiencies = labsCount.GroupBy(l => new { l.degree, l.department, l.specializationName })
        //                                .Select(l => new { degree = l.Key.degree, department = l.Key.department, specializationName = l.Key.specializationName, deficiencies = string.Empty })
        //                                .ToList();

        //    labs += "<table width='100%' border='1' cellpadding='3' cellspacing='0' style='border-color: #ccc;'>";
        //    labs += "<tr>";
        //    labs += "<th style='text-align: center; width: 5%;' colspan='1'>S.No</th>";
        //    labs += "<th style='text-align: left; width: 10%;'  colspan='2' rowspan='2'>Course</th>";
        //    //labs += "<th style='text-align: left; width: 10%;'>Department</th>";
        //    //labs += "<th style='text-align: left; width: 20%;'>Specialization</th>";
        //    labs += "<th style='text-align: center; colspan='1' rowspan='2' >Faculty Shortage</th>";
        //    labs += "<th style='text-align: center; colspan='1'  >No</th>";
        //    labs += "<th style='text-align: center; colspan='1'  >Deficiency of Doctorates</th>";
        //    labs += "<th style='text-align: center; vertical-align: top;' colspan='2' >Labs Shortage</th>";
        //    labs += "<th style='text-align: center; vertical-align: top;' colspan='2' >Labs Shortage</th>";
        //    labs += "<th style='text-align: center; vertical-align: top;' colspan='2' >Labs Shortage</th>";
        //    labs += "</tr>";

        //    foreach (var item in deficiencies)
        //    {

        //        labs += "<tr>";
        //        labs += "<td style='text-align: center; width: 5%; '>" + (deficiencies.IndexOf(item) + 1) + "</td>";
        //        labs += "<td style=''>" + item.degree + '-' + item.specializationName+ "</td>";
        //       ///// labs += "<td style=''>" + item.department + "</td>";
        //        /////labs += "<td style=''>" + item.specializationName + "</td>";

        //        string degreeType = db.jntuh_degree.Where(d => d.degree == item.degree).Select(d => d.jntuh_degree_type.degreeType).FirstOrDefault();

        //        var labsWithDeficiency = labsCount.Where(l => l.degree == item.degree && l.department == item.department && l.specializationName == item.specializationName && l.deficiency == true)
        //            .Select(l => new { Deficiency = degreeType.Equals("PG") ? "No Equipement Uploaded" : l.year + "-" + l.Semester + "-" + l.LabName }).Select(l => l.Deficiency).ToList();

        //        labs += "<td style='; text-align: center'>" + (labsWithDeficiency.Count() == 0 ? "NIL" : String.Join(", ", labsWithDeficiency)) + "</td>";
        //        labs += "<td style='; text-align: center' colspan='3'></td>";
        //        labs += "</tr>";
        //    }

        //    labs += "</table>";

        //    return labs;
        //}
        public string CollegeLabsAnnexureNew(int? collegeID)
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

            //var labDeficiencies = db.jntuh_college_laboratories_deficiency.Where(ld => ld.CollegeId == collegeID && ld.Deficiency == true).Select(ld => ld.LabCode).ToArray();

            //list = list.Where(l => labDeficiencies.Contains(l.LabCode)).ToList();

            if (list.Count() > 0)
            {
                annexure += "<table width='100%' border='1' cellpadding='5' cellspacing='0'>";
                annexure += "<tr>";
                annexure += "<th align='center' colspan='12'>LIST OF EQUIPMENT NOT AVAILABLE</th>";
                annexure += "</tr>";
                annexure += "<tr>";
                annexure += "<th align='left' colspan='1'>S.No</th>";
                annexure += "<th align='left' colspan='3'>Lab Name</th>";
                annexure += "<th align='left' colspan='3'>Equipment Name</th>";
                annexure += "<th align='left' colspan='5'>Remarks</th>";

                annexure += "</tr>";

                //foreach (var item in list.Where(l => labDeficiencies.Contains(l.LabCode)).ToList())
                //{
                //    annexure += "<tr>";
                //    annexure += "<td align='left' colspan='1'>" + (list.IndexOf(item) + 1) + "</td>";
                //    annexure += "<td align='left' colspan='3'>" + item.LabName + "</td>";
                //    annexure += "<td align='left' colspan='3'>" + item.EquipmentName + "</td>";
                //    annexure += "<td colspan='5'></td>";
                //    annexure += "</tr>";
                //}

                annexure += "</table>";
            }

            return annexure;
        }

        //========================//

        public class CollegeFacultyLabs
        {
            public int SNo { get; set; }
            public int DegreeDisplayOrder { get; set; }
            public string DegreeType { get; set; }
            public string Degree { get; set; }
            public string Department { get; set; }
            public string Specialization { get; set; }
            public int SpecializationId { get; set; }
            public string Shift { get; set; }
            public int TotalIntake { get; set; }
            public int Required { get; set; }
            public int Available { get; set; }
            public string Deficiency { get; set; }
            public string PhdDeficiency { get; set; }
            public string LabsDeficiency { get; set; }
        }

        public List<CollegeFacultyLabs> DeficienciesInFaculty2(int? collegeID)
        {
            string faculty = string.Empty;

            faculty += "<table width='100%' border='0' cellpadding='5' cellspacing='0'>";
            faculty += "<tr>";
            faculty += "<td align='left'><b><u>Deficiencies in Faculty</u></b> (Department/ Specialization Wise):";
            faculty += "</tr>";
            faculty += "</table>";

            List<CollegeFacultyWithIntakeReport> facultyCounts = collegeFaculty(collegeID).Where(c => c.shiftId == 1).ToList();

            var count = facultyCounts.Count();
            var distDeptcount = 1;
            var deptloop = 1;
            decimal departmentWiseRequiredFaculty = 0;

            string[] departments = new string[] { "English", "Mathematics", "Physics", "Chemistry", "Others" };

            int totalBtechFirstYearIntake = facultyCounts.Where(d => d.Degree == "B.Tech").Select(d => d.approvedIntake1).Sum();
            var firstYearRequired = Math.Ceiling((double)totalBtechFirstYearIntake / 120);
            int remainingFaculty = 0;
            int remainingPHDFaculty = 0;

            faculty += "<table width='100%' border='1' cellpadding='3' cellspacing='0' style='border-color: #ccc;'>";
            faculty += "<tr>";
            faculty += "<th style='text-align: center; vertical-align: top;'>SNo</th>";
            faculty += "<th style='text-align: left; vertical-align: top;' >Department</th>";
            faculty += "<th style='text-align: left; vertical-align: top;' >Degree</th>";
            faculty += "<th style='text-align: left; vertical-align: top;' >Specialization</th>";
            faculty += "<th style='text-align: center; vertical-align: top;' >Total Intake *</th>";
            faculty += "<th style='text-align: center; vertical-align: top;' >Specialization Wise Faculty Required</th>";
            faculty += "<th style='text-align: center; vertical-align: top;' >Specialization Wise Faculty Available</th>";
            faculty += "<th style='text-align: center; vertical-align: top;' >Deficiency</th>";
            faculty += "<th style='text-align: center; vertical-align: top;' >Deficiency of Ph.D faculty</th>";
            faculty += "</tr>";

            var degreeTypes = db.jntuh_degree_type.Select(t => t).ToList();
            var degrees = db.jntuh_degree.Select(t => t).ToList();

            List<CollegeFacultyLabs> lstFaculty = new List<CollegeFacultyLabs>();

            foreach (var item in facultyCounts)
            {
                //item.requiredFaculty = (item.requiredFaculty - item.requiredFaculty * 10 / 100);
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

                faculty += "<tr>";
                faculty += "<td class='col2' style='text-align: center; vertical-align: top;width:10px' rowspan='1'>" + (facultyCounts.IndexOf(item) + 1) + "</td>";
                faculty += "<td class='col2' style='text-align: left; vertical-align: top;'>" + item.Department + "</td>";
                faculty += "<td class='col2' style='text-align: left; vertical-align: top;'>" + item.Degree + "</td>";
                faculty += "<td class='col2' style='text-align: left; vertical-align: top;'>" + item.Specialization + "</td>";

                if (departments.Contains(item.Department))
                {
                    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + totalBtechFirstYearIntake + "</td>";
                    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + Math.Ceiling(firstYearRequired) + "</td>";
                }
                else
                {
                    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + item.totalIntake + "</td>";
                    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + Math.Ceiling(item.requiredFaculty) + "</td>";
                }

                faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + adjustedFaculty + "</td>";
                faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + minimumRequirementMet + "</td>";

                if (adjustedPHDFaculty > 0)
                {
                    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>NO</td>";
                }
                else if (degreeType.Equals("PG"))
                {
                    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>YES</td>";
                }
                else
                {
                    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>-</td>";
                }

                faculty += "</tr>";

                CollegeFacultyLabs newFaculty = new CollegeFacultyLabs();
                int degreeTypeId = degrees.Where(d => d.degree == item.Degree).Select(d => d.degreeTypeId).FirstOrDefault();
                newFaculty.DegreeType = degreeTypes.Where(t => t.id == degreeTypeId).Select(t => t.degreeType).FirstOrDefault();
                newFaculty.DegreeDisplayOrder = (int)degrees.Where(d => d.degree == item.Degree).Select(d => d.degreeDisplayOrder).FirstOrDefault();
                newFaculty.Degree = item.Degree;
                newFaculty.Department = item.Department;
                newFaculty.Specialization = item.Specialization;
                newFaculty.SpecializationId = item.specializationId;
                newFaculty.TotalIntake = item.approvedIntake1;

                if (departments.Contains(item.Department))
                {
                    //newFaculty.TotalIntake = totalBtechFirstYearIntake;
                    newFaculty.Required = (int)Math.Ceiling(firstYearRequired);
                    newFaculty.Available = item.totalFaculty;
                }
                else
                {
                    //newFaculty.TotalIntake = item.totalIntake;
                    newFaculty.Required = (int)Math.Ceiling(item.requiredFaculty);
                    newFaculty.Available = adjustedFaculty;
                }

                newFaculty.Deficiency = minimumRequirementMet;

                if (adjustedPHDFaculty > 0)
                {
                    newFaculty.PhdDeficiency = "NO";
                }
                else if (degreeType.Equals("PG"))
                {
                    newFaculty.PhdDeficiency = "YES";
                }
                else
                {
                    newFaculty.PhdDeficiency = "-";
                }

                lstFaculty.Add(newFaculty);
                deptloop++;
            }

            faculty += "</table>";

            faculty += "<table width='100%' border='0' cellpadding='5' cellspacing='0'>";
            faculty += "<tr>";
            faculty += "<td align='left'>* II, III & IV Year for B.Tech; I & II Year for M.Tech";
            faculty += "</tr>";
            faculty += "</table>";

            return lstFaculty;
        }

        //public List<CollegeFacultyLabs> DeficienciesInLabs2(int? collegeID)
        //{
        //    string labs = string.Empty;

        //    labs += "<table width='100%' border='0' cellpadding='5' cellspacing='0'>";
        //    labs += "<tr>";
        //    labs += "<td align='left'><b><u>Deficiencies in Laboratory</u></b> (Department/ Specialization Wise):";
        //    labs += "</tr>";
        //    labs += "</table>";

        //    List<Lab> labsCount = collegeLabs(collegeID);

        //    var deficiencies = labsCount.GroupBy(l => new { l.degree, l.department, l.specializationName, l.specializationId })
        //                                .Select(l => new { degree = l.Key.degree, department = l.Key.department, specializationid = l.Key.specializationId, specializationName = l.Key.specializationName, deficiencies = string.Empty })
        //                                .ToList();

        //    labs += "<table width='100%' border='1' cellpadding='3' cellspacing='0' style='border-color: #ccc;'>";
        //    labs += "<tr>";
        //    labs += "<th style='text-align: center; width: 5%;'>S.No</th>";
        //    labs += "<th style='text-align: left; width: 10%;'>Degree</th>";
        //    labs += "<th style='text-align: left; width: 10%;'>Department</th>";
        //    labs += "<th style='text-align: left; width: 20%;'>Specialization</th>";
        //    labs += "<th style='text-align: center; '>Names of the Labs with Deficiency (Details Annexed)</th>";
        //    labs += "</tr>";

        //    var labMaster = db.jntuh_lab_master.ToList();
        //    var collegeLabMaster = db.jntuh_college_laboratories_dataentry2.Where(l => l.CollegeID == collegeID).ToList();

        //    List<CollegeFacultyLabs> lstFaculty = new List<CollegeFacultyLabs>();

        //    foreach (var item in deficiencies)
        //    {

        //        labs += "<tr>";
        //        labs += "<td style='text-align: center; width: 5%; '>" + (deficiencies.IndexOf(item) + 1) + "</td>";
        //        labs += "<td style=''>" + item.degree + "</td>";
        //        labs += "<td style=''>" + item.department + "</td>";
        //        labs += "<td style=''>" + item.specializationName + "</td>";

        //        string degreeType = db.jntuh_degree.Where(d => d.degree == item.degree).Select(d => d.jntuh_degree_type.degreeType).FirstOrDefault();

        //        var labsWithDeficiency = labsCount.Where(l => l.degree == item.degree && l.department == item.department && l.specializationName == item.specializationName && l.deficiency == true)
        //            .Select(l => new { Deficiency = degreeType.Equals("PG") ? "No Equipement Uploaded" : l.year + "-" + l.Semester + "-" + l.Labcode.Replace("-", "$") + "-" + l.specializationId }).Select(l => l.Deficiency).ToList();

        //        List<string> defs = new List<string>();
        //        labsWithDeficiency.ForEach(l =>
        //        {
        //            if (l.Equals("No Equipement Uploaded"))
        //            {
        //                defs.Add(l);
        //            }
        //            else
        //            {
        //                string[] strLab = l.Split('-');

        //                int specializationid = Convert.ToInt32(strLab[3]);
        //                int year = Convert.ToInt32(strLab[0]);
        //                int semester = Convert.ToInt32(strLab[1]);
        //                string labCode = strLab[2].Replace("$", "-");

        //                var requiredLabs = labMaster.Where(m => m.SpecializationID == specializationid && m.Year == year && m.Semester == semester && m.Labcode == labCode).Select(m => m.id).ToList();
        //                int requiredCount = requiredLabs.Count();
        //                int availableCount = collegeLabMaster.Where(m => requiredLabs.Contains(m.EquipmentID)).Count();

        //                if (requiredCount > availableCount)
        //                {
        //                    string labName = labMaster.Where(m => m.SpecializationID == specializationid && m.Year == year && m.Semester == semester && m.Labcode == labCode).Select(m => m.LabName).FirstOrDefault();
        //                    defs.Add(year + "-" + semester + "-" + labName);
        //                }
        //            }
        //        });

        //        labs += "<td style='; text-align: center'>" + (labsWithDeficiency.Count() == 0 ? "NIL" : (defs.Count() == 0 ? "NIL" : String.Join(", ", defs))) + "</td>";
        //        labs += "</tr>";

        //        CollegeFacultyLabs newFaculty = new CollegeFacultyLabs();
        //        newFaculty.Degree = item.degree;
        //        newFaculty.Department = item.department;
        //        newFaculty.Specialization = item.specializationName;
        //        newFaculty.SpecializationId = item.specializationid;
        //        newFaculty.LabsDeficiency = (labsWithDeficiency.Count() == 0 ? "NIL" : (defs.Count() == 0 ? "NIL" : String.Join(", ", defs)));

        //        lstFaculty.Add(newFaculty);
        //    }

        //    labs += "</table>";

        //    return lstFaculty;
        //}

        //public string CollegeCoursesAllClear(int collegeId)
        //{
        //    string courses = string.Empty;
        //    string assessments = string.Empty;

        //    List<CollegeIntakeExisting> collegeIntakeExisting = new List<CollegeIntakeExisting>();

        //    //int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);

        //    courses += "<table border='1' cellpadding='3' cellspacing='0' width='100%' style='font-family: Times New Roman; font-size: 9px; font-weight: normal; background-color: #fff; margin: 0 auto;'>";
        //    courses += "<tr>";
        //    courses += "<th colspan='1' align='center'><b>S.No</b></th>";
        //    courses += "<th colspan='10'><b>Name of the Course</b></th>";
        //    courses += "<th colspan='2' align='center'><b>Intake</b></th>";
        //    courses += "</tr>";

        //    assessments += "<table border='0' cellpadding='3' cellspacing='0' width='100%'>";
        //    assessments += "<tr>";
        //    assessments += "<th align='left'><b>Course wise assessment of essential requirement (Mainly Faculty & Labs) as per University norms & regulations.</b></th>";
        //    assessments += "</tr></table><br />";

        //    assessments += "<table border='1' cellpadding='3' cellspacing='0' width='100%'>";
        //    assessments += "<tr>";
        //    assessments += "<th rowspan='2' colspan='1' align='center'><b>S.No</b></th>";
        //    assessments += "<th rowspan='2' colspan='5'><b>Course</b></th>";
        //    assessments += "<th colspan='4' align='center'><b>Faculty Shortage *</b></th>";
        //    assessments += "<th colspan='5' align='center'><b>Lab Shortage</b></th>";
        //    assessments += "</tr>";
        //    assessments += "<tr>";
        //    assessments += "<th colspan='2' align='center'><b>No</b></th>";
        //    assessments += "<th colspan='2' align='center'><b>Deficiency of Doctorates</b></th>";
        //    assessments += "<th colspan='5' align='center'><b>Name of the Lab(s)</b></th>";
        //    assessments += "</tr>";

        //    int rowCount = 0; int assessmentCount = 0;

        //    List<CollegeFacultyLabs> faculty = DeficienciesInFaculty2(collegeId);//DeficienciesInFaculty2(collegeId)
        //    List<CollegeFacultyLabs> labs = DeficienciesInLabs2(collegeId);//DeficienciesInLabs2(collegeId)

        //    List<CollegeFacultyLabs> clearedCourses = faculty.Join(labs, f => f.SpecializationId, l => l.SpecializationId, (f, l) => new { f, l })
        //                                .Where(a => a.f.Deficiency == "NO" && a.f.PhdDeficiency != "YES" && a.l.LabsDeficiency == "NIL")
        //                                .Select(a => new CollegeFacultyLabs
        //                                {
        //                                    DegreeType = a.f.DegreeType,
        //                                    DegreeDisplayOrder = a.f.DegreeDisplayOrder,
        //                                    Degree = a.f.Degree,
        //                                    Department = a.f.Department,
        //                                    SpecializationId = a.f.SpecializationId,
        //                                    Specialization = a.f.Specialization,
        //                                    TotalIntake = a.f.TotalIntake,
        //                                    Required = a.f.Required,
        //                                    Available = a.f.Available,
        //                                    Deficiency = a.f.Deficiency,
        //                                    PhdDeficiency = a.f.PhdDeficiency,
        //                                    LabsDeficiency = a.l.LabsDeficiency
        //                                })
        //                                .OrderBy(a => a.DegreeDisplayOrder).ThenBy(a => a.Department).ThenBy(a => a.Specialization)
        //                                .ToList();

        //    List<CollegeFacultyLabs> deficiencyCourses = faculty.Join(labs, f => f.SpecializationId, l => l.SpecializationId, (f, l) => new { f, l })
        //                                .Where(a => (a.f.Deficiency == "YES" || a.f.PhdDeficiency == "YES") || a.l.LabsDeficiency != "NIL")
        //                                .Select(a => new CollegeFacultyLabs
        //                                {
        //                                    DegreeType = a.f.DegreeType,
        //                                    DegreeDisplayOrder = a.f.DegreeDisplayOrder,
        //                                    Degree = a.f.Degree,
        //                                    Department = a.f.Department,
        //                                    SpecializationId = a.f.SpecializationId,
        //                                    Specialization = a.f.Specialization,
        //                                    TotalIntake = a.f.TotalIntake,
        //                                    Required = a.f.Required,
        //                                    Available = a.f.Available,
        //                                    Deficiency = a.f.Deficiency,
        //                                    PhdDeficiency = a.f.PhdDeficiency,
        //                                    LabsDeficiency = a.l.LabsDeficiency
        //                                })
        //                                .OrderBy(a => a.DegreeDisplayOrder).ThenBy(a => a.Department).ThenBy(a => a.Specialization)
        //                                .ToList();

        //    //string[] departments = new string[] { "English", "Mathematics", "Physics", "Chemistry" };

        //    //var humanities = faculty.Where(f => departments.Contains(f.Department))
        //    //                        .Select(f => new
        //    //                        {
        //    //                            totalIntake = f.TotalIntake,
        //    //                            required = f.Required,
        //    //                            available = f.Available
        //    //                        }).ToList();

        //    //var humanitiesTotalIntake = humanities.Select(h => h.totalIntake).Sum();
        //    //var humanitiesTotalRequired = humanities.Select(h => h.required).Sum();
        //    //var humanitiesTotalAvailable = humanities.Select(h => h.available).Sum();

        //    //decimal hPercentage = 0;
        //    //hPercentage = (humanitiesTotalRequired * 10) / 100;

        //    //var hDeficiency = false;

        //    //if ((humanitiesTotalRequired - humanitiesTotalAvailable) <= hPercentage)
        //    //{
        //    //    hDeficiency = false;
        //    //}
        //    //else
        //    //{
        //    //    hDeficiency = true;
        //    //}

        //    int affiliatedCount = 0; int defiencyRows = 0;

        //    List<string> deficiencyDepartments = new List<string>();

        //    foreach (var course in deficiencyCourses)
        //    {
        //        if (!deficiencyDepartments.Contains(course.Department))
        //        {
        //            deficiencyDepartments.Add(course.Department);
        //        }

        //        //FIVE percent relaxation for faculty only
        //        decimal percentage = 0;

        //        if (course.LabsDeficiency == "NIL")
        //        {
        //            percentage = (course.Required * 10) / 100;
        //        }

        //        int presentYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
        //        int AY1 = db.jntuh_academic_year.Where(a => a.actualYear == (presentYear + 1)).Select(a => a.id).FirstOrDefault();
        //        bool isCourseAffiliated = false;

        //        var affiliation = db.jntuh_college_intake_existing_datentry2.Where(d => d.collegeId == collegeId && d.specializationId == course.SpecializationId && d.isAffiliated == true).Select(d => d).FirstOrDefault();

        //        if (affiliation != null)
        //        {
        //            if (affiliation.isAffiliated == true)
        //            {
        //                isCourseAffiliated = true;
        //            }
        //        }

        //        if (((course.Required - course.Available) <= percentage && course.PhdDeficiency != "YES" && course.LabsDeficiency == "NIL") || isCourseAffiliated != false)
        //        {
        //            if (course.TotalIntake != 0)
        //            {
        //                affiliatedCount++;

        //                courses += "<tr>";
        //                courses += "<td colspan='1' align='center'>" + (rowCount + 1) + ".</td>";
        //                course.Shift = "1";

        //                string test = "/ " + course.Deficiency + "/ " + course.LabsDeficiency + "/ " + course.Required + "/" + course.Available + "/ " + (course.Required - course.Available) + "/ " + percentage;

        //                if (course.Shift == "1")
        //                {
        //                    courses += "<td colspan='10'>" + course.Degree + " - " + course.Specialization + "</td>";
        //                }

        //                courses += "<td colspan='2' align='center'>" + course.TotalIntake + "</td>";
        //                courses += "</tr>";

        //                rowCount++;


        //                var secondshift = db.jntuh_college_intake_existing.Where(e => e.collegeId == collegeId && e.specializationId == course.SpecializationId && e.shiftId == 2).Select(e => e).FirstOrDefault();

        //                if (secondshift != null)
        //                {
        //                    int approvedIntake = GetIntake(collegeId, AY1, course.SpecializationId, 2, 1);

        //                    if (approvedIntake != 0)
        //                    {
        //                        courses += "<tr>";
        //                        courses += "<td colspan='1' align='center'>" + (rowCount + 1) + ".</td>";
        //                        course.Shift = "2";

        //                        if (course.Shift == "2")
        //                        {
        //                            courses += "<td colspan='10'>" + course.Degree + " - " + course.Specialization + " - 2</td>";
        //                        }

        //                        courses += "<td colspan='2' align='center'>" + GetIntake(collegeId, AY1, course.SpecializationId, 2, 1) + "</td>";
        //                        courses += "</tr>";

        //                        rowCount++;
        //                    }
        //                }
        //            }
        //        }
        //        else if (isCourseAffiliated == false)
        //        {
        //            defiencyRows++;

        //            assessments += "<tr>";
        //            assessments += "<td colspan='1' align='center'>" + (assessmentCount + 1) + "</td>";

        //            course.Shift = "1";
        //            if (course.Shift == "1")
        //            {
        //                assessments += "<td colspan='5' >" + course.Degree + " - " + course.Specialization + "</td>";
        //            }

        //            assessments += "<td colspan='2' align='center'>" + "" + "</td>";
        //            assessments += "<td colspan='2' align='center'>" + "" + "</td>";
        //            assessments += "<td colspan='5' align='center'>" + "" + "</td>";
        //            assessments += "</tr>";

        //            assessmentCount++;

        //            var secondshift = db.jntuh_college_intake_existing.Where(e => e.collegeId == collegeId && e.specializationId == course.SpecializationId && e.shiftId == 2).Select(e => e).FirstOrDefault();

        //            if (secondshift != null)
        //            {
        //                assessments += "<tr>";
        //                assessments += "<td colspan='1' align='center'>" + (assessmentCount + 1) + "</td>";

        //                course.Shift = "2";
        //                if (course.Shift == "2")
        //                {
        //                    assessments += "<td colspan='5' >" + course.Degree + " - " + course.Specialization + " - 2</td>";
        //                }

        //                assessments += "<td colspan='2' align='center'>" + "" + "</td>";
        //                assessments += "<td colspan='2' align='center'>" + "" + "</td>";
        //                assessments += "<td colspan='5' align='center'>" + "" + "</td>";
        //                assessments += "</tr>";

        //                assessmentCount++;
        //            }
        //        }

        //    }

        //    foreach (var course in clearedCourses)
        //    {
        //        string degreeType = db.jntuh_degree.Join(db.jntuh_degree_type, d => d.degreeTypeId, t => t.id, (d, t) => new { d, t })
        //                                           .Where(a => a.d.degree == course.Degree).Select(a => a.t.degreeType).FirstOrDefault();

        //        List<string> clearedDepartments = clearedCourses.Where(a => a.DegreeType == "UG").Select(a => a.Department).ToList();
        //        //degreeType != "UG" && 
        //        if (deficiencyDepartments.Contains(course.Department) && !clearedDepartments.Contains(course.Department))
        //        {
        //            defiencyRows++;
        //            assessments += "<tr>";
        //            assessments += "<td colspan='1' align='center'>" + (assessmentCount + 1) + "</td>";

        //            course.Shift = "1";
        //            if (course.Shift == "1")
        //            {
        //                assessments += "<td colspan='5' >" + course.Degree + " - " + course.Specialization + "</td>";
        //            }

        //            assessments += "<td colspan='2' align='center'>" + "" + "</td>";
        //            assessments += "<td colspan='2' align='center'>" + "" + "</td>";
        //            assessments += "<td colspan='5' align='center'>" + "" + "</td>";
        //            assessments += "</tr>";

        //            assessmentCount++;

        //            var secondshift = db.jntuh_college_intake_existing.Where(e => e.collegeId == collegeId && e.specializationId == course.SpecializationId && e.shiftId == 2).Select(e => e).FirstOrDefault();

        //            if (secondshift != null)
        //            {
        //                assessments += "<tr>";
        //                assessments += "<td colspan='1' align='center'>" + (assessmentCount + 1) + "</td>";

        //                course.Shift = "2";
        //                if (course.Shift == "2")
        //                {
        //                    assessments += "<td colspan='5' >" + course.Degree + " - " + course.Specialization + " - 2</td>";
        //                }

        //                assessments += "<td colspan='2' align='center'>" + "" + "</td>";
        //                assessments += "<td colspan='2' align='center'>" + "" + "</td>";
        //                assessments += "<td colspan='5' align='center'>" + "" + "</td>";
        //                assessments += "</tr>";

        //                assessmentCount++;
        //            }

        //        }
        //        else
        //        {
        //            if (course.TotalIntake != 0)
        //            {
        //                affiliatedCount++;
        //                string test = "/ " + course.Deficiency + "/ " + course.LabsDeficiency + "/ " + course.Required + "/" + course.Available + "/ " + (course.Required - course.Available) + "/ ";

        //                courses += "<tr>";
        //                courses += "<td colspan='1' align='center'>" + (rowCount + 1) + ".</td>";
        //                course.Shift = "1";

        //                if (course.Shift == "1")
        //                {
        //                    courses += "<td colspan='10'>" + course.Degree + " - " + course.Specialization + "</td>";
        //                }

        //                courses += "<td colspan='2' align='center'>" + course.TotalIntake + "</td>";
        //                courses += "</tr>";

        //                rowCount++;


        //                var secondshift = db.jntuh_college_intake_existing.Where(e => e.collegeId == collegeId && e.specializationId == course.SpecializationId && e.shiftId == 2).Select(e => e).FirstOrDefault();

        //                if (secondshift != null)
        //                {
        //                    int presentYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
        //                    int AY1 = db.jntuh_academic_year.Where(a => a.actualYear == (presentYear + 1)).Select(a => a.id).FirstOrDefault();

        //                    int approvedIntake = GetIntake(collegeId, AY1, course.SpecializationId, 2, 1);

        //                    if (approvedIntake != 0)
        //                    {
        //                        courses += "<tr>";
        //                        courses += "<td colspan='1' align='center'>" + (rowCount + 1) + ".</td>";
        //                        course.Shift = "2";

        //                        if (course.Shift == "2")
        //                        {
        //                            courses += "<td colspan='10'>" + course.Degree + " - " + course.Specialization + " - 2</td>";
        //                        }

        //                        courses += "<td colspan='2' align='center'>" + GetIntake(collegeId, AY1, course.SpecializationId, 2, 1) + "</td>";
        //                        courses += "</tr>";

        //                        rowCount++;
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    int totalCleared = clearedCourses.Count();
        //    int totalZeroIntake = clearedCourses.Count();//Where(c => c.TotalIntake == 0).

        //    if (affiliatedCount == 0)
        //    {
        //        courses += "<tr>";
        //        courses += "<td colspan='13' align='center'>NIL</td>";
        //        courses += "</tr>";
        //    }

        //    assessments += "</table>";
        //    courses += "</table>";

        //    return courses + "$" + assessments + "$" + defiencyRows;
        //}
    }
}
