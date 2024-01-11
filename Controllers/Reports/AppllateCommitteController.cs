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
    public class AppllateCommitteController : BaseController
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
                Response.AddHeader("content-disposition", "attachment; filename=" + collegeCode + "-AppellateFormat Report" + id + ".doc");
                Response.ContentType = "application/vnd.ms-word ";
                Response.Charset = string.Empty;
                StringBuilder str = new StringBuilder();
                str.Append(Header());
                str.Append("<br />");
                str.Append(CollegeInformation(collegeID));
                str.Append("<br />");
                str.Append(Principal(collegeID));
                str.Append("<br />");

                var integreatedIds = new[] { 9, 18, 39, 42, 75, 140, 180, 332, 364, 375 };

                var pharmacyids = new[]{24,27,30,33,34,44,47,52,55,65,78,83,95,97,104,105,107,110,114,117,118,127,135,136,139,146,150,159,169,202,204,213,219,234,239,253,
                262,263,267,275,283,284,290,295,297,298,301,303,314,317,320,348,353,362,376,377,379,384,392,395,410,427,428,436,442,445,448,454,457,458,6,
                45,54,58,90,120,206,237,302,313,318,319,389,60,66,252,255,315,370};



                if (pharmacyids.Contains(collegeID))
                {
                    str.Append(PharmacyDeficienciesInFaculty(collegeID));
                }
                else if (integreatedIds.Contains(collegeID))
                {
                    str.Append(PharmacyDeficienciesInFaculty(collegeID));
                    str.Append("<br />");
                    str.Append(DeficienciesInFaculty(collegeID));
                }
                else
                {
                    str.Append(DeficienciesInFaculty(collegeID));
                }
                
                str.Append("<br />");
                

                Document pdfDoc = new Document(PageSize.A4.Rotate(), 60, 50, 60, 60);

                pdfDoc.SetPageSize(iTextSharp.text.PageSize.A4.Rotate());
                pdfDoc.SetMargins(60, 50, 60, 60);

                string path = Server.MapPath("~/Content/PDFReports/AppellateReports/");

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
            header += "<tr><td align='right' width='80%' style='font-size: 16px; font-weight: normal;' colspan='2'></td></tr></br>";
            header += "<tr></tr>";
            header += "</table>";
            header += "<table width='100%'>";
            header += "<tr>";
            header += "<td rowspan='4' align='center' width='20%'><img src='http://112.133.193.228:75/Content/Images/new_logo.jpg' height='70' width='70' style='text-align: center' align='middle' /></td>";
            header += "<td align='center' width='80%' style='font-size: 14px; font-weight: normal;'><b>JAWAHARLAL NEHRU TECHNOLOGICAL UNIVERSITY HYDERABAD</b></td>";
            header += "</tr>";
            header += "<tr>";
            header += "<td align='center' style='font-size: 11px; font-weight: normal;'><b>KUKATPALLY, HYDERABAD, TELANGANA, INDIA - 500 085</b></td>";
            header += "</tr>";
            header += "<tr>";
            header += "<td  align='center' style='font-weight: normal;'><u><b>RECOMMENDATIONS OF APPELLATE COMMITTEE</b></u></td>";
            header += "</tr>";
            header += "<tr>";
            header += "<td  align='center' style='font-weight: normal;'><u><b>(for Academic Year 2016-2017)</b></u></td>";
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
                        Reason = "NOT AVAILABLE" + ",";
                    }
                    if (regdata.NotQualifiedAsperAICTE == true)
                    {
                        Reason += "NOT QUALIFIED " + ",";
                    }
                    if (regdata.InvalidPANNumber == true)
                    {
                        Reason += "NO PAN" + ",";
                    }
                    if (regdata.FalsePAN == true)
                    {
                        Reason += "FALSE PAN" + ",";
                    }
                    if (regdata.NoSCM == true)
                    {
                        Reason += "NO SCM/RATIFICATION" + ",";
                    }
                    if (regdata.IncompleteCertificates == true)
                    {
                        Reason += "Incomplete Certificates" + ",";
                    }
                    if (regdata.PHDundertakingnotsubmitted == true)
                    {
                        Reason += "No Undertaking" + ",";
                    }
                    if (regdata.Blacklistfaculy == true)
                    {
                        Reason += "Blacklisted" + ",";
                    }
                    if (Reason != "")
                    {
                        Reason = Reason.Substring(0, Reason.Length - 1);
                        facultydata.DeactivationNew = "Principal Deficiency.";
                    }
                    else
                    {
                        Reason = "Dr. " + facultydata.FirstName.First().ToString().ToUpper() + facultydata.FirstName.Substring(1) + " " + facultydata.LastName.First().ToString().ToUpper() + facultydata.LastName.Substring(1);
                        facultydata.DeactivationNew = "Principal No Deficiency";
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
            principal += "<td align='left'><b>Principal: </b>";
            principal += "</tr>";

            principal += "</table>";

            return principal;
        }
        public string DeficienciesInFaculty(int? collegeID)
        {
            string faculty = string.Empty;
            int facultycount = 0;

            List<CollegeFacultyWithIntakeReport> facultyCounts = collegeFaculty(collegeID).Where(c => c.shiftId == 1).ToList();//Where(c => c.shiftId == 1)

            var count = facultyCounts.Count();
            var distDeptcount = 1;
            var deptloop = 1;
            decimal departmentWiseRequiredFaculty = 0;

            string[] departments = new string[] { "English", "Mathematics", "Physics", "Chemistry", "Others" };

            int totalBtechFirstYearIntake = facultyCounts.Where(d => d.Degree == "B.Tech").Select(d => d.approvedIntake1).Sum();
            var degrees = db.jntuh_degree.ToList();
            var firstYearRequired = Math.Ceiling((double)totalBtechFirstYearIntake / 120);
            int remainingFaculty = 0;
            int remainingPHDFaculty = 0;
            int SpecializationwisePHDFaculty = 0;
            int SpecializationwisePGFaculty = 0;
            int TotalCount = 0;



            faculty += "<table width='100%' border='1' cellpadding='3' cellspacing='0' style='border-color: #ccc;'>";
            faculty += "<tr>";
            faculty += "<th style='text-align: center; vertical-align: top;width:10px'>SNo</th>";
            faculty += "<th style='text-align: left; vertical-align: top;width:15px' >Department</th>";
            faculty += "<th style='text-align: left; vertical-align: top;width:15px' >Degree</th>";
            faculty += "<th style='text-align: left; vertical-align: top;width:20px' >Specialization</th>";
            faculty += "<th style='text-align: center; vertical-align: top;width:500px' >Details</th>";
            faculty += "<th style='text-align: center; vertical-align: top;width:100px' >Recommendations</th>";

            faculty += "</tr>";

            // Getting M.Tech Civil Specialization ID's
            int[] SpecializationIDS = db.jntuh_specialization.Where(S => S.departmentId == 43).Select(S => S.id).ToArray();
            int remainingFaculty2 = 0;
            foreach (var item in facultyCounts)
            {
                distDeptcount = facultyCounts.Where(d => d.Department == item.Department).Distinct().Count();
                if (item.Degree == "M.Tech" || item.Degree == "B.Tech")
                    SpecializationwisePHDFaculty = facultyCounts.Where(D => D.Department == item.Department && D.Degree == "M.Tech").Distinct().Count();
                else if (item.Degree == "MCA")
                    SpecializationwisePHDFaculty = facultyCounts.Where(D => D.Department == item.Department && D.Degree == "MCA").Distinct().Count();
                else if (item.Degree == "MBA")
                    SpecializationwisePHDFaculty = facultyCounts.Where(D => D.Department == item.Department && D.Degree == "MBA").Distinct().Count();
                TotalCount = facultyCounts.Where(D => D.Department == item.Department && (D.Degree == "M.Tech" || D.Degree == "B.Tech")).Distinct().Count();
                SpecializationwisePHDFaculty = SpecializationwisePHDFaculty * 2;


                if (SpecializationIDS.Contains(item.specializationId))
                {
                    int SpecializationwisePGFaculty1 = facultyCounts.Where(S => S.specializationId == item.specializationId).Count();
                    SpecializationwisePGFaculty = facultyCounts.Where(S => S.specializationId == item.specializationId).Select(S => S.SpecializationspgFaculty).FirstOrDefault();

                }
                int indexnow = facultyCounts.IndexOf(item);



                if (indexnow > 0 && facultyCounts[indexnow].Department != facultyCounts[indexnow - 1].Department)
                {
                    deptloop = 1;
                }

                departmentWiseRequiredFaculty = facultyCounts.Where(d => d.Department == item.Department).Select(d => d.requiredFaculty).Sum();

                string minimumRequirementMet = string.Empty;
                int facultyShortage = 0, tFaculty = 0;
                int adjustedFaculty = 0;
                int adjustedPHDFaculty = 0;
                if (item.Department == "MBA" || item.Department == "MCA")
                    tFaculty = (int)Math.Ceiling(Convert.ToDecimal(item.totalFaculty));//item.totalFaculty
                else
                    tFaculty = (int)Math.Ceiling(Convert.ToDecimal(item.specializationWiseFaculty));//item.totalFaculty
                int rFaculty = (int)Math.Ceiling(item.requiredFaculty);

                if (departments.Contains(item.Department))
                {
                    rFaculty = (int)firstYearRequired;
                    departmentWiseRequiredFaculty = (int)firstYearRequired;
                }

                var degreeType = degrees.Where(d => d.degree == item.Degree).Select(d => d.jntuh_degree_type.degreeType).FirstOrDefault();

                if (deptloop == 1)
                {
                    if (rFaculty <= tFaculty)
                    {
                        minimumRequirementMet = "YES";
                        item.deficiency = false;
                        remainingFaculty = tFaculty - rFaculty;
                        adjustedFaculty = rFaculty;//tFaculty
                    }
                    else
                    {
                        minimumRequirementMet = "NO";
                        item.deficiency = true;
                        adjustedFaculty = tFaculty;
                        facultyShortage = rFaculty - tFaculty;
                    }

                    remainingPHDFaculty = item.phdFaculty;

                    if (remainingPHDFaculty > 2 && (degreeType.Equals("PG")))
                    {
                        adjustedPHDFaculty = remainingPHDFaculty;
                        remainingPHDFaculty = remainingPHDFaculty - 1;
                    }
                    else if (remainingPHDFaculty <= 0 && (degreeType.Equals("PG")))
                    {
                        adjustedPHDFaculty = remainingPHDFaculty;
                        remainingPHDFaculty = remainingPHDFaculty - 1;
                    }
                }
                else
                {
                    if (rFaculty <= remainingFaculty)
                    {
                        minimumRequirementMet = "YES";
                        item.deficiency = false;
                        if (rFaculty <= item.specializationWiseFaculty)
                        {
                            remainingFaculty = remainingFaculty - rFaculty;
                            adjustedFaculty = rFaculty;
                        }

                        else if (rFaculty >= item.specializationWiseFaculty)
                        {
                            remainingFaculty = remainingFaculty - item.specializationWiseFaculty;
                            adjustedFaculty = item.specializationWiseFaculty;
                        }

                    }
                    else
                    {
                        minimumRequirementMet = "NO";
                        item.deficiency = true;
                        adjustedFaculty = remainingFaculty;
                        facultyShortage = rFaculty - remainingFaculty;
                        remainingFaculty = 0;
                    }
                    remainingPHDFaculty = item.phdFaculty;
                    if (remainingPHDFaculty > 2 && (degreeType.Equals("PG")))
                    {

                        adjustedPHDFaculty = remainingPHDFaculty;
                        remainingPHDFaculty = remainingPHDFaculty - 1;
                    }
                    else if (remainingPHDFaculty <= 0 && (degreeType.Equals("PG")))
                    {

                        adjustedPHDFaculty = remainingPHDFaculty;

                        remainingPHDFaculty = remainingPHDFaculty - 1;
                    }
                }
                faculty += "<tr>";
                faculty += "<td  style='text-align: center; vertical-align: top;width:10px' rowspan='1'>" + (facultyCounts.IndexOf(item) + 1) + "</td>";
                faculty += "<td  style='text-align: left; vertical-align: top;width:15px'>" + item.Department + "</td>";
                faculty += "<td  style='text-align: left; vertical-align: top;width:15px'>" + item.Degree + "</td>";
                faculty += "<td  style='text-align: left; vertical-align: top;width:20px'>" + item.Specialization + "</td>";
                faculty += "<td  style='text-align: left; vertical-align: top;width:500px;'></td>";
                faculty += "<td  style='text-align: left; vertical-align: top;width:200px;'></td>";
                faculty += "</tr>";

                deptloop++;
                SpecializationwisePHDFaculty = 0;
            }

            faculty += "</table><br/><br/>";

            faculty += "<p><strong><u>Equipment:</u></strong></p><table width='100%' border='1' cellpadding='90' style='width:100%;height:200px;'><tr><td style='height:200px'></td></tr></table>";
            faculty += "<br/><br/>";

            faculty += "<p><strong>Signatures of Appellate Committee</p></strong>";

            return faculty;
        }
        public List<CollegeFacultyWithIntakeReport> collegeFaculty(int? collegeId)
        {
            List<CollegeIntakeExisting> collegeIntakeExisting = new List<CollegeIntakeExisting>();
            List<CollegeFacultyWithIntakeReport> intakedetailsList = new List<CollegeFacultyWithIntakeReport>();
            var jntuh_departments = db.jntuh_department.ToList();
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
                var jntuh_faculty_student_ratio_norms =
                    db.jntuh_faculty_student_ratio_norms.Where(f => f.isActive == true).ToList();
                var jntuh_degree = db.jntuh_degree.ToList();

                int presentYear = jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
                int AY1 = jntuh_academic_year.Where(a => a.actualYear == (presentYear + 1)).Select(a => a.id).FirstOrDefault();
                int AY2 = jntuh_academic_year.Where(a => a.actualYear == (presentYear)).Select(a => a.id).FirstOrDefault();
                int AY3 = jntuh_academic_year.Where(a => a.actualYear == (presentYear - 1)).Select(a => a.id).FirstOrDefault();
                int AY4 = jntuh_academic_year.Where(a => a.actualYear == (presentYear - 2)).Select(a => a.id).FirstOrDefault();
                int AY5 = jntuh_academic_year.Where(a => a.actualYear == (presentYear - 3)).Select(a => a.id).FirstOrDefault();
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
                    newIntake.degreeDisplayOrder =
                        item.jntuh_specialization.jntuh_department.jntuh_degree.degreeDisplayOrder;
                    newIntake.shiftId = item.shiftId;
                    newIntake.Shift = item.jntuh_shift.shiftName;
                    collegeIntakeExisting.Add(newIntake);
                }
                var collegedepts = collegeIntakeExisting.Select(i => i.DepartmentID).Distinct().ToList();
                collegeIntakeExisting = collegeIntakeExisting.AsEnumerable()
                        .GroupBy(r => new { r.specializationId, r.shiftId, r.collegeId })
                        .Select(r => r.First())
                        .ToList();

                //college Reg nos
                var jntuh_college_faculty_registered = db.jntuh_college_faculty_registered.Where(cf => cf.collegeId == collegeId).ToList();
                string[] strRegnos = jntuh_college_faculty_registered.Select(cf => cf.RegistrationNumber.Trim()).ToArray();


                var jntuh_college_faculty_registered_new = db.jntuh_college_principal_registered.Where(cf => cf.collegeId == collegeId).ToList();
                var principalRegno = jntuh_college_faculty_registered_new.Select(cf => cf.RegistrationNumber).FirstOrDefault();

                //education categoryIds UG,PG,PHD...........

                int pharmacyDeptId = db.jntuh_department.Where(d => d.departmentName == "Pharmacy").Select(d => d.id).FirstOrDefault();

                var jntuh_education_category = db.jntuh_education_category.ToList();

                var registeredFaculty = principalRegno != null ? db.jntuh_registered_faculty.Where(rf => strRegnos.Contains(rf.RegistrationNumber.Trim()) && rf.RegistrationNumber != principalRegno).ToList()
                    : db.jntuh_registered_faculty.Where(rf => strRegnos.Contains(rf.RegistrationNumber.Trim())).ToList();
                var reg1 = registeredFaculty.Where(f => f.RegistrationNumber.Trim() == "9251-150414-062519").ToList();
                //Reg nos related online facultyIds`-- (rf.isApproved == null || rf.isApproved == true)
                var collegeaffliations = db.jntuh_college_affliationstatus.AsNoTracking().ToList();
                var regfacultywithdepts = registeredFaculty.Where(rf => rf.DepartmentId == null).ToList();

                var jntuh_registered_faculty1 =
                    registeredFaculty.Where(rf => rf.DepartmentId != null && ((rf.Absent != true) && (rf.NotQualifiedAsperAICTE != true)
                                                        && (rf.NoSCM != true) && (rf.PANNumber != null) && rf.Notin116 != true && rf.Blacklistfaculy != true && rf.PHDundertakingnotsubmitted != true) && (rf.DepartmentId != 61 || rf.DepartmentId != 27) && rf.type != "Adjunct").Select(rf => new
                                                        {
                                                            //Departmentid = rf.DepartmentId,
                                                            RegistrationNumber = rf.RegistrationNumber.Trim(),
                                                            Department = rf.jntuh_department.departmentName,
                                                            HighestDegreeID = rf.jntuh_registered_faculty_education.Count() != 0 ? rf.jntuh_registered_faculty_education.Select(e => e.educationId).Max() : 0,
                                                            IsApproved = rf.isApproved,
                                                            PanNumber = rf.PANNumber,
                                                            AadhaarNumber = rf.AadhaarNumber,
                                                            NoForm16 = rf.NoForm16,
                                                            TotalExperience = rf.TotalExperience
                                                        }).ToList();
                // var reg11 = registeredFaculty.Where(f => f.RegistrationNumber.Trim() == "9251-150414-062519").ToList();
                var nohighestdegree = jntuh_registered_faculty1.Where(e => e.HighestDegreeID < 4).ToList();
                jntuh_registered_faculty1 = jntuh_registered_faculty1.Where(e => e.HighestDegreeID >= 4).ToList();

                var jntuh_registered_faculty = jntuh_registered_faculty1.Select(rf => new
                {
                    RegistrationNumber = rf.RegistrationNumber,
                    Department = rf.Department,
                    HighestDegree = jntuh_education_category.Where(c => c.id == rf.HighestDegreeID).Select(c => c.educationCategoryName).FirstOrDefault(),
                    Recruitedfor = jntuh_college_faculty_registered.Where(c => c.RegistrationNumber.Trim() == rf.RegistrationNumber.Trim()).Select(c => c.IdentifiedFor).FirstOrDefault(),
                    SpecializationId = jntuh_college_faculty_registered.Where(c => c.RegistrationNumber.Trim() == rf.RegistrationNumber.Trim()).Select(c => c.SpecializationId).FirstOrDefault(),
                    PanNumber = rf.PanNumber,
                    AadhaarNumber = rf.AadhaarNumber,
                    noform16 = rf.NoForm16,
                    TotalExperience = rf.TotalExperience
                }).Where(e => e.Department != null).ToList();
                var form16Count = registeredFaculty.Where(i => i.NoForm16 == true).ToList();
                var aictecount = registeredFaculty.Where(i => i.NotQualifiedAsperAICTE == true).ToList();
                int[] StrPharmacy = new[] { 26, 27, 36, 39 };
                foreach (var item in collegeIntakeExisting.Where(D => !StrPharmacy.Contains(D.DepartmentID)).ToList())
                {
                    var intakedetails = new CollegeFacultyWithIntakeReport();
                    int phdFaculty; int pgFaculty; int ugFaculty; int SpecializationphdFaculty = 0; int SpecializationpgFaculty = 0;

                    intakedetails.collegeId = item.collegeId;
                    intakedetails.collegeCode = jntuh_college.Where(c => c.id == item.collegeId).Select(c => c.collegeCode).FirstOrDefault();
                    intakedetails.collegeName = jntuh_college.Where(c => c.id == item.collegeId).Select(c => c.collegeName).FirstOrDefault();
                    intakedetails.Degree = item.Degree;
                    intakedetails.Department = item.Department;
                    intakedetails.Specialization = item.Specialization;
                    if (item.Specialization == "Industrial Pharmacy")
                    {

                    }
                    intakedetails.specializationId = item.specializationId;
                    intakedetails.DepartmentID = item.DepartmentID;
                    intakedetails.shiftId = item.shiftId;

                    var status = collegeaffliations.Where(i => i.DegreeID == item.degreeID && i.SpecializationId == item.specializationId && i.CollegeId == item.collegeId).ToList();
                    if (status.Count > 0)
                    {
                        intakedetails.AffliationStatus = "A";
                    }

                    intakedetails.approvedIntake1 = GetIntake(item.collegeId, AY1, item.specializationId, item.shiftId, 1);
                    intakedetails.approvedIntake2 = GetIntake(item.collegeId, AY2, item.specializationId, item.shiftId, 1);
                    intakedetails.approvedIntake3 = GetIntake(item.collegeId, AY3, item.specializationId, item.shiftId, 1);
                    intakedetails.approvedIntake4 = GetIntake(item.collegeId, AY4, item.specializationId, item.shiftId, 1);
                    intakedetails.approvedIntake5 = GetIntake(item.collegeId, AY5, item.specializationId, item.shiftId, 1);
                    facultystudentRatio = Convert.ToInt32(jntuh_faculty_student_ratio_norms.Where(fn => fn.degreeId == item.degreeID).Select(fn => fn.Norms).FirstOrDefault().Split(':')[1].ToString());

                    var csedept = jntuh_registered_faculty.Where(i => i.Department == item.Department).ToList();
                    intakedetails.form16count = form16Count.Count > 0 ? form16Count.Count(i => i.DepartmentId == item.DepartmentID) : 0;
                    intakedetails.aictecount = aictecount.Count > 0 ? aictecount.Count(i => i.DepartmentId == item.DepartmentID) : 0;


                    if (item.Degree == "B.Tech")
                    {
                        intakedetails.totalIntake = (intakedetails.approvedIntake2) + (intakedetails.approvedIntake3) +
                                                    (intakedetails.approvedIntake4);
                        facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) /
                                       Convert.ToDecimal(facultystudentRatio);
                        //intakedetails.A416TotalFaculty = registeredFaculty.Where(i => i.DepartmentId == item.DepartmentID && (i.jntuh_department.jntuh_degree.degree == "B.Tech" || i.jntuh_department.jntuh_degree.degree == "B.Pharmacy")).ToList().Count;
                    }
                    else if (item.Degree == "M.Tech" || item.Degree == "MBA")
                    {
                        intakedetails.totalIntake = (intakedetails.approvedIntake1) + (intakedetails.approvedIntake2);
                        facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) /
                                       Convert.ToDecimal(facultystudentRatio);

                    }
                    else if (item.Degree == "MCA")
                    {
                        intakedetails.totalIntake = (intakedetails.approvedIntake1) + (intakedetails.approvedIntake2) +
                                                    (intakedetails.approvedIntake3);
                        facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) /
                                       Convert.ToDecimal(facultystudentRatio);
                    }
                    else if (item.Degree == "B.Pharmacy")
                    {
                        intakedetails.totalIntake = (intakedetails.approvedIntake1) + (intakedetails.approvedIntake2) +
                                                    (intakedetails.approvedIntake3) + (intakedetails.approvedIntake4);
                        facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) / Convert.ToDecimal(facultystudentRatio);
                    }
                    else if (item.Degree == "M.Pharmacy")
                    {
                        intakedetails.totalIntake = (intakedetails.approvedIntake1) + (intakedetails.approvedIntake2);
                        facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) / 12;

                    }
                    else if (item.Degree == "Pharm.D")
                    {
                        intakedetails.totalIntake = (intakedetails.approvedIntake1) + (intakedetails.approvedIntake2) +
                                                    (intakedetails.approvedIntake3) + (intakedetails.approvedIntake4) +
                                                    (intakedetails.approvedIntake5);
                        facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) / 12;
                    }
                    else if (item.Degree == "Pharm.D PB")
                    {
                        intakedetails.totalIntake = (intakedetails.approvedIntake1) + (intakedetails.approvedIntake2);
                        facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) / 10;
                    }
                    else //MAM MTM
                    {
                        intakedetails.totalIntake = (intakedetails.approvedIntake1) + (intakedetails.approvedIntake2) +
                                                    (intakedetails.approvedIntake3) + (intakedetails.approvedIntake4) +
                                                    (intakedetails.approvedIntake5);
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
                            intakedetails.specializationWiseFaculty = jntuh_registered_faculty.Count(f => f.Department == "Pharmacy" && f.Recruitedfor == "UG");
                        }
                        else if (item.Degree == "Pharm.D")
                        {
                            intakedetails.specializationWiseFaculty = jntuh_registered_faculty.Count(f => f.Department == "Pharm.D" && f.Recruitedfor == "UG");
                        }
                        else if (item.Degree == "Pharm.D PB")
                        {
                            intakedetails.specializationWiseFaculty = jntuh_registered_faculty.Count(f => f.Department == "Pharm.D PB" && f.Recruitedfor == "UG");
                        }
                        else
                        {

                            var regno = jntuh_registered_faculty.Where(f => f.Department == item.Department).Select(f => f.RegistrationNumber);//&& (f.Recruitedfor == "UG" || f.Recruitedfor == null)
                            intakedetails.specializationWiseFaculty = jntuh_registered_faculty.Count(f => f.Department == item.Department);//&& (f.Recruitedfor == "UG" || f.Recruitedfor == null)
                        }
                    }

                    if (strdegreetype == "PG")
                    {
                        if (item.Degree == "M.Pharmacy")
                        {
                            intakedetails.specializationWiseFaculty = jntuh_registered_faculty.Count(f => f.Department == "Pharmacy" &&
                                f.SpecializationId == item.specializationId);//&& (f.Recruitedfor == "PG" || f.Recruitedfor == "UG&PG")
                        }
                        else
                        {
                            intakedetails.specializationWiseFaculty = jntuh_registered_faculty.Count(f => f.Department == item.Department &&
                                        f.SpecializationId == item.specializationId);//&& (f.Recruitedfor == "PG" || f.Recruitedfor == "UG&PG")
                        }
                    }
                    if (strdegreetype == "Dual Degree")
                    {
                        intakedetails.specializationWiseFaculty = jntuh_registered_faculty.Count(f => f.SpecializationId == item.specializationId);//(f.Recruitedfor == "PG" || f.Recruitedfor == "UG&PG") &&
                    }
                    //intakedetails.id =
                    //    jntuh_college_faculty_deficiency.Where(
                    //        fd =>
                    //            fd.CollegeId == collegeId && fd.SpecializationId == item.specializationId &&
                    //            fd.ShiftId == item.shiftId).Select(fd => fd.Id).FirstOrDefault();

                    //if (intakedetails.id > 0)
                    //{
                    //    int? swf =
                    //        jntuh_college_faculty_deficiency.Where(fd => fd.Id == intakedetails.id)
                    //            .Select(fd => fd.SpecializationWiseFaculty)
                    //            .FirstOrDefault();
                    //    if (swf != null)
                    //    {
                    //        intakedetails.specializationWiseFaculty = (int) swf;
                    //    }
                    //    intakedetails.deficiency =
                    //        jntuh_college_faculty_deficiency.Where(fd => fd.Id == intakedetails.id)
                    //            .Select(fd => fd.Deficiency)
                    //            .FirstOrDefault();
                    //    intakedetails.shortage =
                    //        jntuh_college_faculty_deficiency.Where(fd => fd.Id == intakedetails.id)
                    //            .Select(fd => fd.Shortage)
                    //            .FirstOrDefault();
                    //}

                    //============================================

                    int noPanOrAadhaarcount = 0;

                    if (item.Degree == "B.Pharmacy")
                    {
                        ugFaculty = jntuh_registered_faculty.Count(f => "UG" == f.HighestDegree && f.SpecializationId == item.specializationId);
                        pgFaculty = jntuh_registered_faculty.Count(f => "PG" == f.HighestDegree && f.SpecializationId == item.specializationId);
                        phdFaculty = jntuh_registered_faculty.Count(f => "Ph.D" == f.HighestDegree && f.SpecializationId == item.specializationId);
                        //intakedetails.Department = "Pharmacy";
                    }
                    if (item.Degree == "M.Pharmacy")
                    {
                        ugFaculty = jntuh_registered_faculty.Count(f => "UG" == f.HighestDegree && f.Department == "Pharmacy" && f.SpecializationId == item.specializationId);
                        pgFaculty = jntuh_registered_faculty.Count(f => ("PG" == f.HighestDegree || "M.Phil" == f.HighestDegree) &&
                                    f.Department == "Pharmacy" && f.SpecializationId == item.specializationId);
                        phdFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && f.Department == "Pharmacy" && f.SpecializationId == item.specializationId);
                        //noPanOrAadhaarcount =registeredFaculty.Where(f =>f.DepartmentId == pharmacyDeptId && f.PANNumber == null && f.AadhaarNumber == null &&
                        //            (f.isApproved == null || f.isApproved == true)).Count();
                        //intakedetails.Department = "Pharmacy";
                    }
                    else if (item.Degree == "Pharm.D")
                    {
                        ugFaculty = jntuh_registered_faculty.Count(f => "UG" == f.HighestDegree && f.Department == "Pharm.D" && f.SpecializationId == item.specializationId);
                        pgFaculty = jntuh_registered_faculty.Count(f => ("PG" == f.HighestDegree || "M.Phil" == f.HighestDegree) && f.Department == "Pharm.D" && f.SpecializationId == item.specializationId);
                        phdFaculty = jntuh_registered_faculty.Count(f => "Ph.D" == f.HighestDegree && f.Department == "Pharm.D" && f.SpecializationId == item.specializationId);
                        //noPanOrAadhaarcount = registeredFaculty.Where(f => f.DepartmentId == pharmacyDeptId && f.PANNumber == null && f.AadhaarNumber == null && (f.isApproved == null || f.isApproved == true)).Count();
                        intakedetails.Department = "Pharm.D";
                    }
                    else if (item.Degree == "Pharm.D PB")
                    {
                        ugFaculty = jntuh_registered_faculty.Count(f => "UG" == f.HighestDegree && f.Department == "Pharm.D PB" && f.SpecializationId == item.specializationId);
                        pgFaculty = jntuh_registered_faculty.Count(f => ("PG" == f.HighestDegree || "M.Phil" == f.HighestDegree) && f.Department == "Pharm.D PB" && f.SpecializationId == item.specializationId);
                        phdFaculty = jntuh_registered_faculty.Count(f => "Ph.D" == f.HighestDegree && f.Department == "Pharm.D PB" && f.SpecializationId == item.specializationId);
                        //noPanOrAadhaarcount = registeredFaculty.Where(f => f.DepartmentId == pharmacyDeptId && f.PANNumber == null && f.AadhaarNumber == null && (f.isApproved == null || f.isApproved == true)).Count();
                        intakedetails.Department = "Pharm.D PB";
                    }
                    else
                    {
                        ugFaculty = jntuh_registered_faculty.Count(f => "UG" == f.HighestDegree && f.Department == item.Department);
                        pgFaculty = jntuh_registered_faculty.Count(f => ("PG" == f.HighestDegree || "M.Phil" == f.HighestDegree) && f.Department == item.Department);
                        phdFaculty = jntuh_registered_faculty.Count(f => "Ph.D" == f.HighestDegree && f.Department == item.Department);


                        //var phdFaculty1 = jntuh_registered_faculty.Where(f => ("Ph.D" == f.HighestDegree || "Ph.D." == f.HighestDegree || "Phd" == f.HighestDegree || "PHD" == f.HighestDegree || "Ph D" == f.HighestDegree)).ToList() ;
                        //if (item.Department == "MBA")
                        //    phdFaculty1 = phdFaculty1.Where(f => f.Department == "MBA").ToList();

                        // string REG=
                        SpecializationphdFaculty = jntuh_registered_faculty.Count(f => "Ph.D" == f.HighestDegree && f.SpecializationId == item.specializationId);
                        var reg = jntuh_registered_faculty.Where(f => f.SpecializationId == item.specializationId && (f.Recruitedfor == "PG" || f.Recruitedfor == "UG&PG")).Select(S => S.RegistrationNumber).ToList();
                        SpecializationpgFaculty = jntuh_registered_faculty.Count(f => f.SpecializationId == item.specializationId && (f.Recruitedfor == "PG" || f.Recruitedfor == "UG&PG"));

                    }

                    intakedetails.phdFaculty = phdFaculty;
                    intakedetails.SpecializationsphdFaculty = SpecializationphdFaculty;
                    intakedetails.SpecializationspgFaculty = SpecializationpgFaculty;
                    intakedetails.pgFaculty = pgFaculty;
                    intakedetails.ugFaculty = ugFaculty;
                    intakedetails.totalFaculty = (ugFaculty + pgFaculty + phdFaculty);
                    intakedetails.A416TotalFaculty = registeredFaculty.Where(i => i.DepartmentId == item.DepartmentID && (i.jntuh_department.jntuh_degree.degree == item.Degree ||
                                 i.jntuh_department.jntuh_degree.degree == item.Degree)).ToList().Count;
                    intakedetails.facultyWithoutPANAndAadhaar = noPanOrAadhaarcount;
                    //=============//

                    intakedetailsList.Add(intakedetails);
                }

                intakedetailsList = intakedetailsList.OrderBy(ei => ei.Department).ThenBy(ei => ei.degreeDisplayOrder).ThenBy(ei => ei.Specialization).ThenBy(ei => ei.shiftId).ToList();

                string[] strOtherDepartments = { "English", "Mathematics", "Physics", "Chemistry", "Others" };
                var btechdegreecount = intakedetailsList.Count(d => d.Degree == "B.Tech");
                if (btechdegreecount > 0)
                {
                    foreach (var department in strOtherDepartments)
                    {
                        var deptid = jntuh_departments.Where(i => i.departmentName == department).Select(i => i.id).FirstOrDefault();
                        var deptname = jntuh_departments.Where(i => i.departmentName == department).Select(i => i.departmentName).FirstOrDefault();
                        int speId = jntuh_specialization.Where(s => s.jntuh_department.departmentName == department).Select(s => s.id).FirstOrDefault();
                        int ugFaculty = jntuh_registered_faculty.Count(f => "UG" == f.HighestDegree && f.Department == department);
                        int pgFaculty = jntuh_registered_faculty.Count(f => ("PG" == f.HighestDegree || "M.Phil" == f.HighestDegree) && f.Department == department);
                        int phdFaculty = jntuh_registered_faculty.Count(f => "Ph.D" == f.HighestDegree && f.Department == department);

                        intakedetailsList.Add(new CollegeFacultyWithIntakeReport
                        {
                            collegeId = (int)collegeId,
                            Degree = "B.Tech",
                            Department = department,
                            Specialization = department,
                            ugFaculty = ugFaculty,
                            pgFaculty = pgFaculty,
                            phdFaculty = phdFaculty,
                            totalFaculty = ugFaculty + pgFaculty + phdFaculty,
                            specializationId = speId,
                            shiftId = 1,
                            form16count = form16Count.Count > 0 ? form16Count.Count(i => i.DepartmentId == deptid) : 0,
                            aictecount = aictecount.Count > 0 ? aictecount.Count(i => i.DepartmentId == deptid) : 0,
                            A416TotalFaculty = registeredFaculty.Where(i => i.DepartmentId == deptid).ToList().Count,
                            specializationWiseFaculty = jntuh_registered_faculty.Count(f => f.Department == deptname)
                        });
                    }
                }
            }

            return intakedetailsList;
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

        #region Class
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
            public int approvedIntake5 { get; set; }

            public int totalIntake { get; set; }
            public decimal requiredFaculty { get; set; }
            public int phdFaculty { get; set; }
            public int SpecializationsphdFaculty { get; set; }
            public int SpecializationspgFaculty { get; set; }
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
            public int form16count { get; set; }
            public int aictecount { get; set; }
            public int A416TotalFaculty { get; set; }

            public int NOSCM { get; set; }
            public int DeactivationReasionsCount { get; set; }
            public int FacultyAbsentCount { get; set; }
            public int InvalidPanCount { get; set; }
            public int totalBtechFirstYearIntake { get; set; }
            public int incompletecerificatesCount { get; set; }
            public int firstYearRequired { get; set; }
            public string minimumRequirementMet { get; set; }
            public string PHDminimumRequirementMet { get; set; }
            public int adjustedFaculty { get; set; }
            public int adjointfacultycount { get; set; }

            public string AffliationStatus { get; set; }
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


            public string PharmacySpec1 { get; set; }
            public string PharmacySpec2 { get; set; }

            public IList<PharmacySpecilaizationList> PharmacySpecilaizationList { get; set; }
        }
        public class PharmacySpecilaizationList
        {
            public string PharmacyspecName { get; set; }
            public string Specialization { get; set; }
            public int PharmacyspecWiseCount { get; set; }
        }
        #endregion

        #region For Pharmacy
        public List<CollegeFacultyWithIntakeReport> PharmacyCollegeFaculty(int? collegeId)
        {
            var colleges = db.jntuh_college.Where(c => c.isActive == true).Select(c => new
            {
                collegeId = c.id,
                collegeName = c.collegeCode + "-" + c.collegeName
            }).OrderBy(c => c.collegeName).ToList();

            //colleges.Add(new { collegeId = 0, collegeName = "00-ALL Colleges" });

            ViewBag.Colleges = colleges.OrderBy(c => c.collegeId).ThenBy(c => c.collegeName).ToList();



            var randomcode = "";
            if (collegeId != null)
            {
                randomcode = db.jntuh_college_randamcodes.FirstOrDefault(i => i.CollegeId == collegeId).RandamCode;
            }
            var pharmadTotalintake = 0;
            var pharmadPBTotalintake = 0;

            #region PharmacyCode
            List<CollegeIntakeExisting> collegeIntakeExisting = new List<CollegeIntakeExisting>();
            List<CollegeFacultyWithIntakeReport> intakedetailsList = new List<CollegeFacultyWithIntakeReport>();
            if (collegeId != null)
            {
                var jntuh_Bpharmacy_faculty_deficiency = db.jntuh_bpharmacy_faculty_deficiency.Where(fd => fd.CollegeId == collegeId).ToList();
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
                int AY5 = jntuh_academic_year.Where(a => a.actualYear == (presentYear - 3)).Select(a => a.id).FirstOrDefault();
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

                var bpharmacyintake = 0;
                decimal BpharcyrequiredFaculty = 0;
                decimal PharmDrequiredFaculty = 0;
                decimal PharmDPBrequiredFaculty = 0;
                var pharmacydeptids = new[] { 26, 27, 36, 39 };
                collegeIntakeExisting = collegeIntakeExisting.Where(i => pharmacydeptids.Contains(i.DepartmentID)).ToList();
                foreach (var item in collegeIntakeExisting)
                {
                    var intakedetails = new CollegeFacultyWithIntakeReport();
                    var phdFaculty = 0;
                    var pgFaculty = 0;
                    var ugFaculty = 0;

                    intakedetails.collegeId = item.collegeId;
                    intakedetails.collegeCode = jntuh_college.Where(c => c.id == item.collegeId).Select(c => c.collegeCode).FirstOrDefault();
                    intakedetails.collegeName = jntuh_college.Where(c => c.id == item.collegeId).Select(c => c.collegeName).FirstOrDefault();
                    intakedetails.Degree = item.Degree;
                    intakedetails.Department = item.Department;
                    intakedetails.Specialization = item.Specialization;
                    intakedetails.specializationId = item.specializationId;
                    intakedetails.DepartmentID = item.DepartmentID;
                    intakedetails.shiftId = item.shiftId;

                    switch (item.Degree)
                    {
                        case "B.Pharmacy":
                            intakedetails.SortId = 1;
                            intakedetails.Department = "Pharmacy";
                            break;
                        case "M.Pharmacy":
                            intakedetails.SortId = 4;
                            intakedetails.Department = "Pharmacy";
                            break;
                        case "Pharm.D":
                            intakedetails.SortId = 2;
                            break;
                        case "Pharm.D PB":
                            intakedetails.SortId = 3;
                            break;
                    }
                    
                    
                    intakedetailsList.Add(intakedetails);
                }
            #endregion

                var pharmdspeclist = new List<PharmacySpecilaizationList>
                {
                    new PharmacySpecilaizationList()
                    {
                        PharmacyspecName = "Group1 (Pharmacy Practice, Pharm D)",
                        Specialization = "Pharm.D"
                    }
                };
                var pharmdpbspeclist = new List<PharmacySpecilaizationList>
                {
                    new PharmacySpecilaizationList()
                    {
                        PharmacyspecName = "Group1 (Pharmacy Practice, Pharm D)",
                        Specialization = "Pharm.D PB"
                    }
                };

                var pharmacyspeclist = new List<PharmacySpecilaizationList>
                {
                    new PharmacySpecilaizationList()
                    {
                        PharmacyspecName = "Group1 (Pharmaceutics)",
                        Specialization = "B.Pharmacy"
                    },
                    new PharmacySpecilaizationList()
                    {
                        PharmacyspecName = "Group2 (Pharmaceutical Chemistry,Pharmacy Analysis, PAQA)",
                        Specialization = "B.Pharmacy"
                    },
                    
                    new PharmacySpecilaizationList()
                    {
                        PharmacyspecName = "Group3 (Pharmacology)",
                        Specialization = "B.Pharmacy"
                    },
                    new PharmacySpecilaizationList()
                    {
                        PharmacyspecName = "Group4 (Pharmacognosy)",
                        Specialization = "B.Pharmacy"
                    },
                    
                };



                #region All B.Pharmacy Specializations

                var group1Subcount = 0; var group2Subcount = 0; var group3Subcount = 0; var group4Subcount = 0; var group5Subcount = 0; var group6Subcount = 0;
                var pharmadgroup1Subcount = 0; var pharmadPBgroup1Subcount = 0;
                string conditionbpharm = null;
                string conditionpharmd = null;
                string conditionphardpb = null;
                foreach (var list in pharmacyspeclist)
                {
                    var bpharmacylist = new CollegeFacultyWithIntakeReport();
                    bpharmacylist.Specialization = list.Specialization;
                    bpharmacylist.PharmacySpec1 = list.PharmacyspecName;
                    bpharmacylist.collegeId = (int)collegeId;
                    bpharmacylist.collegeCode = jntuh_college.Where(c => c.id == collegeId).Select(c => c.collegeCode).FirstOrDefault();
                    bpharmacylist.collegeName = jntuh_college.Where(c => c.id == collegeId).Select(c => c.collegeName).FirstOrDefault();
                    bpharmacylist.shiftId = 1;
                    bpharmacylist.Degree = "B.Pharmacy";
                    bpharmacylist.Department = "Pharmacy";
                    bpharmacylist.PharmacyGroup1 = "Group1";
                    bpharmacylist.PharmacySpecilaizationList = pharmacyspeclist;
                    bpharmacylist.BphramacyrequiredFaculty = BpharcyrequiredFaculty;
                    bpharmacylist.SortId = 1;

                    if (list.PharmacyspecName == "Group1 (Pharmaceutics)" || list.PharmacyspecName == "Pharmaceutics" || list.PharmacyspecName == "Industrial Pharmacy" || list.PharmacyspecName == "Pharmacy BioTechnology" || list.PharmacyspecName == "Pharmaceutical Technology")
                    {
                        bpharmacylist.BPharmacySubGroup1Count = group1Subcount;
                        bpharmacylist.BPharmacySubGroupRequired = bpharmacyintake >= 100 ? 6 : 4;
                        bpharmacylist.PharmacySubGroup1 = "Group1 (Pharmaceutics)";
                    }

                    else if (list.PharmacyspecName == "Group2 (Pharmaceutical Chemistry,Pharmacy Analysis, PAQA)" || list.PharmacyspecName == "Pharmaceutical Chemistry")
                    {
                        bpharmacylist.BPharmacySubGroup1Count = group2Subcount;
                        bpharmacylist.BPharmacySubGroupRequired = bpharmacyintake >= 100 ? 6 : 5;
                        bpharmacylist.PharmacySubGroup1 = "Group2 (Pharmaceutical Chemistry,Pharmacy Analysis, PAQA)";
                    }
                    

                    else if (list.PharmacyspecName == "Group3 (Pharmacology)" || list.PharmacyspecName == "Pharmacology" || list.PharmacyspecName == "Pharma D" || list.PharmacyspecName == "Pharma.D" || list.PharmacyspecName == "Pharm D" || list.PharmacyspecName == "Pharm.D")
                    {
                        bpharmacylist.BPharmacySubGroup1Count = group3Subcount;
                        bpharmacylist.BPharmacySubGroupRequired = bpharmacyintake >= 100 ? 5 : 4;
                        bpharmacylist.PharmacySubGroup1 = "Group3 (Pharmacology)";
                    }

                    else if (list.PharmacyspecName == "Group4 (Pharmacognosy)" || list.PharmacyspecName == "Pharmacognosy")
                    {
                       
                        bpharmacylist.BPharmacySubGroup1Count = group4Subcount;
                        bpharmacylist.BPharmacySubGroupRequired = 3;
                        bpharmacylist.PharmacySubGroup1 = "Group4 (Pharmacognosy)";
                    }

                  
                    var id = jntuh_Bpharmacy_faculty_deficiency.Where(fd => fd.CollegeId == collegeId && fd.BpharmacySpecialization == list.PharmacyspecName && fd.ShiftId == 1).Select(fd => fd.Id).FirstOrDefault();
                    if (id > 0)
                    {
                        var swf = jntuh_Bpharmacy_faculty_deficiency.Where(fd => fd.Id == id).Select(fd => fd.SpecializationWiseFaculty).FirstOrDefault();
                        if (swf != null)
                        {
                            bpharmacylist.specializationWiseFaculty = (int)swf;
                        }
                        bpharmacylist.id = id;
                        bpharmacylist.deficiency = jntuh_Bpharmacy_faculty_deficiency.Where(fd => fd.Id == id).Select(fd => fd.Deficiency).FirstOrDefault();
                        bpharmacylist.shortage = jntuh_Bpharmacy_faculty_deficiency.Where(fd => fd.Id == id).Select(fd => fd.Shortage).FirstOrDefault();
                    }

                    intakedetailsList.Add(bpharmacylist);
                }

                //for pharma D specializations
                var pharmaD = collegeIntakeExisting.Where(i => i.specializationId == 18).ToList();
                if (pharmaD.Count > 0)
                {
                    foreach (var list in pharmdspeclist)
                    {
                        int phd;
                        int pg;
                        int ug;
                        var bpharmacylist = new CollegeFacultyWithIntakeReport();
                        bpharmacylist.Specialization = list.Specialization;
                        bpharmacylist.PharmacySpec1 = list.PharmacyspecName;
                        bpharmacylist.collegeId = (int)collegeId;
                        bpharmacylist.collegeCode = jntuh_college.Where(c => c.id == collegeId).Select(c => c.collegeCode).FirstOrDefault();
                        bpharmacylist.collegeName = jntuh_college.Where(c => c.id == collegeId).Select(c => c.collegeName).FirstOrDefault();
                        bpharmacylist.shiftId = 1;
                        bpharmacylist.Degree = "Pharm.D";
                        bpharmacylist.Department = "Pharm.D";
                        bpharmacylist.PharmacyGroup1 = "Group1";
                        //bpharmacylist.requiredFaculty = BpharcyrequiredFaculty;
                        bpharmacylist.PharmacySpecilaizationList = pharmacyspeclist;
                        bpharmacylist.pharmadrequiredfaculty = Math.Ceiling(PharmDrequiredFaculty);
                        bpharmacylist.SortId = 2;
                        


                        if (list.PharmacyspecName == "Group1 (Pharmacy Practice, Pharm D)" || list.PharmacyspecName == "Pharmacy Practice" || list.PharmacyspecName == "Pharm D" || list.PharmacyspecName == "Pharm.D")
                        {
                            bpharmacylist.BPharmacySubGroup1Count = pharmadgroup1Subcount;
                            bpharmacylist.BPharmacySubGroupRequired = pharmadTotalintake / 30;
                            bpharmacylist.PharmacySubGroup1 = "Group1 (Pharmacy Practice, Pharm D)";
                        }


                        var id = jntuh_Bpharmacy_faculty_deficiency.Where(fd => fd.CollegeId == collegeId && fd.BpharmacySpecialization == list.PharmacyspecName && fd.ShiftId == 1).Select(fd => fd.Id).FirstOrDefault();
                        if (id > 0)
                        {
                            var swf = jntuh_Bpharmacy_faculty_deficiency.Where(fd => fd.Id == id).Select(fd => fd.SpecializationWiseFaculty).FirstOrDefault();
                            if (swf != null)
                            {
                                bpharmacylist.specializationWiseFaculty = (int)swf;
                            }
                            bpharmacylist.id = id;
                            bpharmacylist.deficiency = jntuh_Bpharmacy_faculty_deficiency.Where(fd => fd.Id == id).Select(fd => fd.Deficiency).FirstOrDefault();
                            bpharmacylist.shortage = jntuh_Bpharmacy_faculty_deficiency.Where(fd => fd.Id == id).Select(fd => fd.Shortage).FirstOrDefault();
                        }

                        intakedetailsList.Add(bpharmacylist);
                    }
                }


                //for pharma.D PB specializations
                var pharmaDPB = collegeIntakeExisting.Where(i => i.specializationId == 19).ToList();
                if (pharmaDPB.Count > 0)
                {
                    foreach (var list in pharmdpbspeclist)
                    {
                        int phd;
                        int pg;
                        int ug;
                        var bpharmacylist = new CollegeFacultyWithIntakeReport();
                        bpharmacylist.Specialization = list.Specialization;
                        bpharmacylist.PharmacySpec1 = list.PharmacyspecName;
                        bpharmacylist.collegeId = (int)collegeId;
                        bpharmacylist.collegeCode = jntuh_college.Where(c => c.id == collegeId).Select(c => c.collegeCode).FirstOrDefault();
                        bpharmacylist.collegeName = jntuh_college.Where(c => c.id == collegeId).Select(c => c.collegeName).FirstOrDefault();
                        bpharmacylist.shiftId = 1;
                        bpharmacylist.Degree = "Pharm.D PB";
                        bpharmacylist.Department = "Pharm.D PB";
                        bpharmacylist.PharmacyGroup1 = "Group1";
                        bpharmacylist.pharmadPBrequiredfaculty = Math.Ceiling(PharmDPBrequiredFaculty);
                        bpharmacylist.PharmacySpecilaizationList = pharmacyspeclist;
                        bpharmacylist.SortId = 3;
                        
                        if (list.PharmacyspecName == "Group1 (Pharmacy Practice, Pharm D)" || list.PharmacyspecName == "Pharmacy Practice" || list.PharmacyspecName == "Pharm D" || list.PharmacyspecName == "Pharm.D")
                        {
                            
                            bpharmacylist.BPharmacySubGroup1Count = pharmadPBgroup1Subcount;
                            bpharmacylist.BPharmacySubGroupRequired = pharmadPBTotalintake / 10;
                            bpharmacylist.PharmacySubGroup1 = "Group1 (Pharmacy Practice, Pharm D)";
                        }


                        var id = jntuh_Bpharmacy_faculty_deficiency.Where(fd => fd.CollegeId == collegeId && fd.BpharmacySpecialization == list.PharmacyspecName && fd.ShiftId == 1).Select(fd => fd.Id).FirstOrDefault();
                        if (id > 0)
                        {
                            var swf = jntuh_Bpharmacy_faculty_deficiency.Where(fd => fd.Id == id).Select(fd => fd.SpecializationWiseFaculty).FirstOrDefault();
                            if (swf != null)
                            {
                                bpharmacylist.specializationWiseFaculty = (int)swf;
                            }
                            bpharmacylist.id = id;
                            bpharmacylist.deficiency = jntuh_Bpharmacy_faculty_deficiency.Where(fd => fd.Id == id).Select(fd => fd.Deficiency).FirstOrDefault();
                            bpharmacylist.shortage = jntuh_Bpharmacy_faculty_deficiency.Where(fd => fd.Id == id).Select(fd => fd.Shortage).FirstOrDefault();
                        }

                        intakedetailsList.Add(bpharmacylist);
                    }
                }

                #endregion

                intakedetailsList = intakedetailsList.OrderBy(ei => ei.Department).ThenBy(ei => ei.degreeDisplayOrder).ThenBy(ei => ei.Specialization).ThenBy(ei => ei.shiftId).ToList();
            }
            return intakedetailsList;
        }

        public string PharmacyDeficienciesInFaculty(int? collegeID)
        {
            var faculty = string.Empty;

            var facultyCounts = PharmacyCollegeFaculty(collegeID).Where(c => c.shiftId == 1).OrderBy(i=>i.SortId).ToList();//Where(c => c.shiftId == 1)
            var deptloop = 1;
            var specloop = 1;

            faculty += "<table width='100%' border='1' cellpadding='3' cellspacing='0' style='border-color: #ccc;'>";
            faculty += "<tr>";
            faculty += "<th style='text-align: center; vertical-align: top;width:5%'>SNo</th>";
            faculty += "<th style='text-align: center; vertical-align: top;width:10%' >Department</th>";
            faculty += "<th style='text-align: center; vertical-align: top;width:10%' >Degree</th>";
            faculty += "<th style='text-align: center; vertical-align: top;width:15%' >Specialization</th>";

            faculty += "<th style='text-align: center; vertical-align: top;width:45%' >Details</th>";
            faculty += "<th style='text-align: center; vertical-align: top;width:15%' >Recommendations</th>";
            faculty += "</tr>";

            foreach (var item in facultyCounts)
            {
                var distSpeccount = facultyCounts.Where(d => d.Specialization == item.Specialization && d.Degree == item.Degree).Distinct().Count();
                var indexnow = facultyCounts.IndexOf(item);

                if (indexnow > 0 && facultyCounts[indexnow].Department != facultyCounts[indexnow - 1].Department)
                {
                    deptloop = 1;
                }
                if (indexnow > 0 && facultyCounts[indexnow].Specialization != facultyCounts[indexnow - 1].Specialization)
                {
                    specloop = 1;
                }
                

                faculty += "<tr>";
                faculty += "<td class='col2' style='text-align: center; vertical-align: top;width:10px' rowspan='1'>" + (facultyCounts.IndexOf(item) + 1) + "</td>";

                faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + item.Department + "</td>";
                if (specloop == 1)
                    faculty += "<td rowspan='" + distSpeccount + "'  class='col2' style='text-align: center; vertical-align: center;'>" + item.Degree + "</td>";
                if (item.Degree != "M.Pharmacy")
                    faculty += "<td class='col2' style='text-align: center; vertical-align: center;'>" + item.PharmacySubGroup1 + "</td>";
                else if (item.Degree == "M.Pharmacy")
                    faculty += "<td class='col2' style='text-align: center; vertical-align: center;'>" + item.Specialization + "</td>";
                faculty += "<td class='col2' style='text-align: center; vertical-align: top;' rowspan='1'></td>";
                faculty += "<td class='col2' style='text-align: center; vertical-align: top;' rowspan='1'></td>";
                

                faculty += "</tr>";
                deptloop++;
                specloop++;
            }

            faculty += "</table>";

            var integreatedIds = new[] { 9, 18, 39, 42, 75, 140, 180, 332, 364, 375 };

            if (collegeID != null && !integreatedIds.Contains((int)collegeID))
            {
                faculty += "<p><strong><u>Equipment:</u></p></strong><table border='1' cellpadding='90' style='width:100%;height:200px;'><tr><td style='height:200px'></td></tr></table>";
                faculty += "<br/><br/>";

                faculty += "<p><strong>Signatures of Appellate Committee</p></strong>";
            }

            return faculty;
        }
        #endregion

    }
}
