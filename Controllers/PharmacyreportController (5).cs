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

namespace UAAAS.Controllers
{
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
        //private int TotalcollegeFaculty;
        //private int Group1PharmacyFaculty;
        //private int Group2PharmacyFaculty;
        //private int Group3PharmacyFaculty;
        //private int Group4PharmacyFaculty;
        //private int Group5PharmacyFaculty;
        //private int Group6PharmacyFaculty;
        //private int Allgroupscount;
        //private string PharmacyandPharmDMeet = "";

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

                Document pdfDoc = new Document(PageSize.A4.Rotate(), 60, 50, 60, 60);

                pdfDoc.SetPageSize(iTextSharp.text.PageSize.A4.Rotate());
                pdfDoc.SetMargins(60, 50, 60, 60);
                
               
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
            header += "<tr></tr>";
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
            header += "<td  align='center' style='font-weight: normal;'><u><b>DEFICIENCY REPORT AS PER FORM 418</b></u></td>";
            header += "</tr>";
            header += "<tr>";
            header += "<td  align='center' style='font-weight: normal;'><u><b>(for Academic Year 2018-2019)</b></u></td>";
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
                  
                    if (!string.IsNullOrEmpty(regdata.DeactivationReason))
                    {
                        Reason = "Dr. " + facultydata.FirstName.First().ToString().ToUpper() + facultydata.FirstName.Substring(1) + " " + facultydata.LastName.First().ToString().ToUpper() + facultydata.LastName.Substring(1);
                        //Reason.Substring(0, Reason.Length - 1);
                        facultydata.DeactivationNew = "Yes";
                        OriginalReason = regdata.DeactivationReason;
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
            string facultyAdmittedIntakeZero = string.Empty;
            faculty += "<table width='100%' border='0' cellpadding='5' cellspacing='0'>";
            faculty += "<tr>";
            faculty += "<td align='left'><b><u>Deficiencies in Faculty</u></b> (Department/ Specialization Wise):</td>";
            faculty += "</tr>";
            faculty += "</table>";

            faculty += "<table width='100%' border='1' cellpadding='3' cellspacing='0' style='border-color: #ccc;font-size:13px'>";
            faculty += "<tr>";
            faculty += "<th style='text-align: center; vertical-align: top;'>SNo</th>";
            faculty += "<th style='text-align: left; vertical-align: top;' >Department</th>";
            faculty += "<th style='text-align: left; vertical-align: top;' >Degree</th>";
            faculty += "<th style='text-align: left; vertical-align: top;' >Specialization</th>";
            //faculty += "<th style='text-align: left; vertical-align: top;' >Status</th>";
            faculty += "<th style='text-align: center; vertical-align: top;' >Total Intake *</th>";
            faculty += "<th style='text-align: center; vertical-align: top;' >Proposed Intake</th>";
            faculty += "<th style='text-align: center; vertical-align: top;' >No.of Faculty Required *</th>";
            faculty += "<th style='text-align: center; vertical-align: top;' >No.of Faculty Available</th>";
            faculty += "<th style='text-align: center; vertical-align: top;' >Pharmacy Specializations *</th>";
            faculty += "<th style='text-align: center; vertical-align: top;' >Specialization Wise Faculty Required</th>";
            faculty += "<th style='text-align: center; vertical-align: top;' >Specialization Wise Faculty Available</th>";
            //faculty += "<th style='text-align: center; vertical-align: top;' >Adjusted faculty</th>";
            //faculty += "<th style='text-align: center; vertical-align: top;' >Not Qualified as per AICTE faculty</th>";
            faculty += "<th style='text-align: center; vertical-align: top;' >Deficiency</th>";
            faculty += "<th style='text-align: center; vertical-align: top;' >No. of Ph.D faculty</th>";
            faculty += "</tr>";

            var jntuh_academic_year = db.jntuh_academic_year.ToList();
            int presentYear = jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int AY1 = jntuh_academic_year.Where(a => a.actualYear == (presentYear + 1)).Select(a => a.id).FirstOrDefault();
            int AY2 = jntuh_academic_year.Where(a => a.actualYear == (presentYear)).Select(a => a.id).FirstOrDefault();
            int AY3 = jntuh_academic_year.Where(a => a.actualYear == (presentYear - 1)).Select(a => a.id).FirstOrDefault();
            int AY4 = jntuh_academic_year.Where(a => a.actualYear == (presentYear - 2)).Select(a => a.id).FirstOrDefault();
            int AY5 = jntuh_academic_year.Where(a => a.actualYear == (presentYear - 3)).Select(a => a.id).FirstOrDefault();

            var Departments = db.jntuh_department.Where(d => d.degreeId == 2 || d.degreeId == 5 || d.degreeId == 9 || d.degreeId == 10).ToList();
            int[] Departmentids = Departments.Select(d => d.id).ToArray();
            var Specializations = db.jntuh_specialization.Where(s => Departmentids.Contains(s.departmentId)).ToList();
            int[] Specializationids = Specializations.Select(s => s.id).ToArray();

            List<jntuh_college_intake_existing> intake = db.jntuh_college_intake_existing.Where(i => i.collegeId == collegeID && Specializationids.Contains(i.specializationId) && i.courseStatus != "Closure").ToList();
            List<CollegeIntakeExisting> collegeIntakeExisting = new List<CollegeIntakeExisting>();
              List<CollegeIntakeExisting> CurrentyearcollegeIntakeExisting = new List<CollegeIntakeExisting>();
              
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
            CurrentyearcollegeIntakeExisting = collegeIntakeExisting.Where(a => a.academicYearId == 10).OrderBy(a=>a.DepartmentID).ToList();
             string cid=collegeID.ToString();
             var FacultyData=db.jntuh_appeal_pharmacydata.Where(p=>p.CollegeCode==cid).ToList();
             string[] AssignedFaculty = FacultyData.Select(p => p.Deficiency).ToArray();
             var registeredFaculty = db.jntuh_registered_faculty.Where(rf => AssignedFaculty.Contains(rf.RegistrationNumber.Trim())).ToList();

             var jntuh_registered_faculty1 = registeredFaculty
                                                     .Select(rf => new
                                                     {
                                                         RegistrationNumber = rf.RegistrationNumber,
                                                         HighestDegreeID = rf.jntuh_registered_faculty_education.Count() != 0 ? rf.jntuh_registered_faculty_education.Select(e => e.educationId).Max() : 0,
                                                     }).Where(e=>e.HighestDegreeID==6).ToList();
             string[] PhdRegNO = jntuh_registered_faculty1.Select(e => e.RegistrationNumber).ToArray();

             string[] Collegefaculty = db.jntuh_college_faculty_registered.Where(CF => CF.collegeId == collegeID).Select(Cf => Cf.RegistrationNumber).ToArray();
             var collegeFacultycount1 = db.jntuh_registered_faculty.Where(rf => Collegefaculty.Contains(rf.RegistrationNumber)).ToList();
             var collegeFacultycount = collegeFacultycount1.Where(rf => rf.type != "Adjunct" && ((rf.Absent == false) && (rf.OriginalCertificatesNotShown == false) && (rf.Xeroxcopyofcertificates == false || rf.Xeroxcopyofcertificates == null) && (rf.NotQualifiedAsperAICTE == false || rf.NotQualifiedAsperAICTE == null)//&& rf.Noform16Verification == false && rf.NoForm16 == false
                                                        && (rf.NoSCM == false || rf.NoSCM == null) && (rf.PANNumber != null) && (rf.IncompleteCertificates == false || rf.IncompleteCertificates == null) && (rf.Blacklistfaculy == false) && rf.NoRelevantUG != "Yes" && rf.NoRelevantPG != "Yes" && rf.NORelevantPHD != "Yes" && rf.NotIdentityfiedForanyProgram == false && (rf.InvalidPANNumber == false || rf.InvalidPANNumber == null) && (rf.OriginalsVerifiedPHD != true) && (rf.OriginalsVerifiedUG != true) && (rf.SamePANUsedByMultipleFaculty == false || rf.SamePANUsedByMultipleFaculty == null) && rf.BASStatusOld != "Yes") && rf.BASStatus != "Yes").Select(r => r.RegistrationNumber).ToList();
             TotalcollegeFaculty = collegeFacultycount.Count();
             int? Required=0;
             int? Avilable=0;
             int? PhDAvilable=0;
             //int? TotalRequired = 0;
             //int? TotalAvilable = 0;
             int? TotalIntake = 0;
             int? PraposedIntake = 0;
            int Sno = 1;
             string strgroup="";
             string PharmacyStatus="";
             string specialization="";
             int SecondYerintake = 0;
             int ThirdYerintake = 0;
             int FourthYerintake = 0;
             int PharmDFirstYerintake = 0;
             int PharmDSecondYerintake = 0;
             int PharmDThirdYerintake = 0;
             int PharmDFourthYerintake = 0;
             int PharmDFifthhYerintake = 0;
             string PharmD = "";
            foreach (var item in CurrentyearcollegeIntakeExisting)
            {
                if (item.Degree == "B.Pharmacy")
                {
                    for (int i = 1; i <= 4; i++ )
                    {
                        faculty += "<tr>";
                       
                       
                        if(i==1)
                        {
                             PharmD=CurrentyearcollegeIntakeExisting.Where(d => d.Degree == "Pharm.D").Select(d=>d.Degree).FirstOrDefault();
                            if (PharmD!="" && PharmD!=null)
                            {
                                PharmDFirstYerintake = intake.Where(a => a.specializationId == 18 && a.academicYearId == 9).Select(a => a.approvedIntake).FirstOrDefault();
                                PharmDSecondYerintake = intake.Where(a => a.specializationId == 18 && a.academicYearId == 8).Select(a => a.approvedIntake).FirstOrDefault();
                                PharmDThirdYerintake = intake.Where(a => a.specializationId == 18 && a.academicYearId == 3).Select(a => a.approvedIntake).FirstOrDefault();
                                PharmDFourthYerintake = intake.Where(a => a.specializationId == 18 && a.academicYearId == 2).Select(a => a.approvedIntake).FirstOrDefault();
                                PharmDFifthhYerintake = intake.Where(a => a.specializationId == 18 && a.academicYearId == 1).Select(a => a.approvedIntake).FirstOrDefault();
                            }
                            SecondYerintake = intake.Where(a => a.specializationId == 12 && a.academicYearId == 9).Select(a => a.approvedIntake).FirstOrDefault();
                            ThirdYerintake = intake.Where(a => a.specializationId == 12 && a.academicYearId ==8).Select(a => a.approvedIntake).FirstOrDefault();
                            FourthYerintake = intake.Where(a => a.specializationId == 12 && a.academicYearId == 3).Select(a => a.approvedIntake).FirstOrDefault();
                            faculty += "<td class='col2' style='text-align: center; vertical-align: top;width:10px' rowspan='5'>1</td>";
                            faculty += "<td class='col2' style='text-align: center; vertical-align: top;' rowspan='4'>Pharmacy</td>";
                            faculty += "<td class='col2' style='text-align: center; vertical-align: top;'  rowspan='4'>" + item.Department + "</td>";

                            faculty += "<td class='col2' style='text-align: center; vertical-align: top;' rowspan='4'>" + item.Specialization + "</td>";

                            faculty += "<td class='col2' style='text-align: center; vertical-align: top;' rowspan='4' >";
                            faculty += "<table width='100%' border='1' cellpadding='3' cellspacing='0' style='border-color: #ccc;font-size:13px'>";
                            //faculty += "<tr>";
                            //faculty += "<td class='col2' style='text-align: center; vertical-align: top;'  colspan='3'><b>B.Pharmacy.</b></td>";
                            //faculty += "</tr>";
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
                            if (PharmD != "" && PharmD != null)
                            {
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

                            faculty += "</td>";
                        }
                       
                        
                           
                        faculty += "<td class='col2' style='text-align: center; vertical-align: top;'></td>";
                        faculty += "<td class='col2' style='text-align: center; vertical-align: top;'></td>";
                        faculty += "<td class='col2' style='text-align: center; vertical-align: top;'></td>";
                        switch(i)
                        {
                            case 1:
                                faculty += "<td class='col2' style='text-align: left; vertical-align: top;'>Group1 (Pharmaceutics , Industrial Pharmacy , Pharmaceutical Technology , Pharmaceutical Biotechnology)</td>";
                                strgroup = "1";
                                break;
                            case 2:
                                faculty += "<td class='col2' style='text-align: left; vertical-align: top;'>Group2 (Pharmaceutical Chemistry , Pharmaceutical Analysis , PAQA , QA , QAPRA , NIPER Medicinal Chemistry)</td>";
                                strgroup = "2";
                                break;
                            case 3:
                                faculty += "<td class='col2' style='text-align: left; vertical-align: top;'>Group3 (Pharmacology , Pharm-D, Pharm-DPB , Pharmacy Practice , Hospital Pharmacy , Clinical Pharmacy, Hospital and Clinical Pharmacy)</td>";
                                strgroup = "3";
                                break;
                            default:
                                faculty += "<td class='col2' style='text-align: left; vertical-align: top;'>Group4 (Pharmacognosy , Pharmaceutical Chemistry , Phytopharmacy & Phytomedicine , NIPER  Natural Products , Pharmaceutical Biotechnology)</td>";
                                strgroup = "4";
                                break;
                        }
                           
                       Required=FacultyData.Where(p=>p.PharmacySpecialization==strgroup && p.CollegeCode==cid).Select(p=>p.SpecializationWiseRequiredFaculty).FirstOrDefault();
                       Avilable = FacultyData.Count(p => p.PharmacySpecialization == strgroup && p.CollegeCode==cid && p.Deficiency!=null);
                        faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>"+Required+"</td>";
                        faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + Avilable + "</td>";
                        if (Required <= Avilable)
                        {
                            faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>No Deficiency</td>";
                            PharmacyStatus="No Deficiency";
                        }
                        else
                        {
                             faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>Deficiency</td>";
                            PharmacyStatus="Deficiency";
                        }
                           

                        faculty += "<td class='col2' style='text-align: center; vertical-align: top;'></td>";
                        faculty += "</tr>";
                        Sno=1;
                        if (AddingFacultyCount == 0)
                            AddingFacultyCount = Avilable;
                        else
                            AddingFacultyCount += Avilable;

                        
                       
                    }
                        
                }
                specialization=item.specializationId.ToString();
                TotalIntake = FacultyData.Where(p => p.CollegeCode == cid && p.Specialization == specialization).Select(p => p.TotalIntake).FirstOrDefault();
                PraposedIntake = FacultyData.Where(p => p.CollegeCode == cid && p.Specialization == specialization).Select(p => p.ProposedIntake).FirstOrDefault();
                Required = FacultyData.Where(p => p.CollegeCode == cid && p.Specialization == specialization).Select(p => p.NoOfFacultyRequired).FirstOrDefault();
                Avilable = FacultyData.Count(p => p.CollegeCode == cid && p.Specialization == specialization && p.Deficiency!=null );

                    faculty += "<tr>";
                    if (item.Degree != "B.Pharmacy")
                    faculty += "<td class='col2' style='text-align: center; vertical-align: top;width:10px' rowspan='1'>" + Sno + "</td>";
                    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + item.Department + "</td>";
                    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + item.Degree + "</td>";
                    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + item.Specialization + "</td>";

                    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + TotalIntake + "</td>";
                    if (item.Degree == "B.Pharmacy")
                    {
                        if (PraposedIntake>100)
                        faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>100</td>";
                        else
                            faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + PraposedIntake + "</td>";
                    }
                    else if (item.Degree == "M.Pharmacy")
                    {
                        faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>15</td>";
                    }
                    else
                    {
                        faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + PraposedIntake + "</td>";
                    }
                    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + Required + "</td>";
                    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + Avilable + "</td>";
                    faculty += "<td class='col2' style='text-align: left; vertical-align: top;'></td>";
                    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'></td>";
                    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + Avilable + "</td>";
                 if (item.Degree == "B.Pharmacy")
                    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>"+PharmacyStatus+"</td>";
                 else
                 {
                     PhDAvilable = FacultyData.Count(f => PhdRegNO.Contains(f.Deficiency) && f.Specialization == specialization);
                     if (PharmacyStatus != "Deficiency")
                     {
                         if (Required <= Avilable)
                         {
                             if (item.Degree == "M.Pharmacy" && PhDAvilable>0)
                             {
                                 faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>No Deficiency</td>";
                             }
                             else
                             {
                                 if ((item.Degree == "Pharm.D" || item.Degree == "Pharm.D PB") && (PharmacyStatus != "Deficiency"))
                                 {
                                     faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>No Deficiency</td>";
                                 }
                                 else
                                 {
                                     faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>Deficiency</td>";
                                 }
                                 
                             }
                             
                         }
                         else
                         {
                             if ((item.Degree == "Pharm.D" || item.Degree == "Pharm.D PB") && (PharmacyStatus != "Deficiency"))
                                 {
                                     faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>No Deficiency</td>";
                                 }
                                 else
                                 {
                                     faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>Deficiency</td>";
                                 }
                         }
                     }
                     else
                     {
                         if ((item.Degree == "Pharm.D" || item.Degree == "Pharm.D PB") && (PharmacyStatus != "Deficiency"))
                                 {
                                     faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>No Deficiency</td>";
                                 }
                                 else
                                 {
                                     faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>Deficiency</td>";
                                 }
                     }
                     
                 }
                 if (item.Degree == "M.Pharmacy")
                 {
                     PhDAvilable = FacultyData.Count(f => PhdRegNO.Contains(f.Deficiency) && f.Specialization == specialization);
                     faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + PhDAvilable + "</td>";
                 }
                else
                 {
                     faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>0</td>";
                 }
                    
                    faculty += "</tr>";
                    Sno++;
                    if (item.Degree != "B.Pharmacy")
                    {
                        if (AddingFacultyCount == 0)
                            AddingFacultyCount = Avilable;
                        else
                            AddingFacultyCount += Avilable;
                    }
                   
                    if (PharmacyStatus == "Deficiency" && item.Degree == "B.Pharmacy")
                    {
                        faculty += "<tr>";
                        faculty += "<td class='col2' style='text-align: center; vertical-align: top;'  colspan='13'><b>Note :B.Pharmacy Deficiency Exists & Hence other Degrees will not be considered.</b></td>";
                        faculty += "</tr>";
                    }
               
                
            }
            int? Remaning = (TotalcollegeFaculty - AddingFacultyCount);
            faculty += "<tr><td align='center' colspan='13' style='font-size: 14px; font-weight: normal;'><b> Additional Faculty  : " + Remaning + "</b></td></tr>";
            faculty += "<tr><td align='center' colspan='13' style='font-size: 14px; font-weight: normal;'><b> Total Faculty : " + TotalcollegeFaculty + "</b></td></tr>";
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
            var AffiliationFee = db.jntuh_college_paymentoffee.Where(e => e.collegeId == collegeID && e.FeeTypeID == 5).Select(e => e).FirstOrDefault();
            var CommanserviceFee = db.jntuh_college_paymentoffee.Where(e => e.collegeId == collegeID && e.FeeTypeID == 3).Select(e => e).FirstOrDefault();
            faculty += "<br/><table><tr><td align='left'><b><u>Pending Issues:</u></b></td>";
            faculty += "</tr></table>";
            faculty += "<ul style='list-style-type:disc'>";


            if (CommanserviceFee.paidAmount != null)
                {
                    faculty += "<li>Common Service Fee Due:<b> Rs." + CommanserviceFee.paidAmount + "</b></li>";
                }

            if (AffiliationFee.duesAmount != null)
                {
                    faculty += "<li>Affiliation Fee Due: <b>Rs." + AffiliationFee.duesAmount + "</b></li>";
                }
            
            #endregion
            

            #region OTHER OBSERVATIONS/  REMARKS
            var nodocumentsdetails = db.jntuh_deficiencyrepoprt_college_pendingdocuments.Where(i => i.CollegeId == collegeID).Select(i => i).FirstOrDefault();
            var lastyearfacultycount = db.jntuh_notin415faculty.Where(i => i.CollegeId == collegeID).Select(i => i).FirstOrDefault();
            var AICTEFaculty = db.jntuh_college_aictefaculty.Where(a=>a.CollegeId==collegeID).ToList();

            int AICTEFacultyCount = AICTEFaculty.Count();

            faculty += "<table><tr><td align='left'><b><u>Other Observations/ Remarks:</u></b></td>";
            faculty += "</tr></table>";
            faculty += "<ul style='list-style-type:disc'>";


            if (nodocumentsdetails != null && nodocumentsdetails.FFCTeamComments != "" && nodocumentsdetails.FFCTeamComments != null)
            {
                faculty += "<li>" + nodocumentsdetails.FFCTeamComments + "</li>";

            }


            if (nodocumentsdetails != null && nodocumentsdetails.AICTENoOfFaculty != 0)
            {

                faculty += "<li>Total faculty uploaded to AICTE for Extension of Approval for AY 2018-19 &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; : " + AICTEFacultyCount + ".</li>";//facultyCounts.Select(i => i.specializationWiseFaculty).Sum()
                faculty += "<li>Total faculty available during inspection with  qualifications as prescribed by AICTE &nbsp;&nbsp;&nbsp;:<b> " + TotalcollegeFaculty + "</b>.</li>";
            }
            else
            {
                faculty += "<li>Total faculty uploaded to AICTE for Extension of Approval for AY 2017-18 &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; " + AICTEFacultyCount + ".</li>";
                faculty += "<li>Total faculty available during inspection with  qualifications as prescribed by AICTE &nbsp;&nbsp;&nbsp;:<b> " + TotalcollegeFaculty + "</b>.</li>";

            }


           
            
            if (lastyearfacultycount != null)
            {

                faculty += "<li>Number of faculty recruited after the last inspection  &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; : " + lastyearfacultycount.RegistrationNumber + ".</li>";//". Total Available Faculty is " + collegeFacultycount + 
            }
            else
            {

                faculty += "<li>Number of faculty recruited after the last inspection &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;   :0. </li>";//Total Available Faculty is " + collegeFacultycount + ".
            }


            faculty += "</ul>";
            #endregion


            faculty += "</ul>";

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
            string annexure = string.Empty;
            var Departments = db.jntuh_department.Where(d => d.degreeId == 2 || d.degreeId == 5 || d.degreeId == 9 || d.degreeId == 10).ToList();
            int[] Departmentids = Departments.Select(d => d.id).ToArray();
            var Specializations = db.jntuh_specialization.Where(s => Departmentids.Contains(s.departmentId)).ToList();
            List<int> specializationIds1 = Specializations.Select(s => s.id).ToList();

            List<FacultyVerificationController.AnonymousLabclass> collegeLabAnonymousLabclass = new List<FacultyVerificationController.AnonymousLabclass>();
            List<int> specializationIds = db.jntuh_college_intake_existing.Where(e => e.collegeId == collegeID && e.courseStatus != "Closure" && e.academicYearId == 10 && specializationIds1.Contains(e.specializationId)).Select(e => e.specializationId).Distinct().ToList();


           

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
                                                              .Where(l => specializationIds.Contains(l.SpecializationID) && !Equipmentsids.Contains(l.id) && l.CollegeId == null &&  !l.EquipmentName.Contains("desirable"))
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
                    annexure += "<th align='left' colspan='4'> " + specializationDetails.degree + " -" + specializationDetails.department + "-" + specializationDetails.specialization + "</th>";
                    annexure += "</tr>";
                    annexure += "<tr>";
                    annexure += "<th align='left'>S.No</th><th align='left'>Lab Name</th><th align='left'>Equipment Name</th><th align='left'>Deficiency(Yes/No)<br/>If No Reason</th>";
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
                        annexure += "<td align='left'>" + indexnow + "</td><td  align='left'>" + LabNmae + "</td><td  align='left'>" + EquipmentName + "</td> <td  align='left'></td>";
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
           
            annexure += "</br><p> <b>NOTE:</b> The Physical Verification of the faculty and their presence at the time of Inspection by the FFC, automatically does not mean that the college is entitled for Affiliation based on numbers. Those of the faculty who are having the requisite qualifications and credentials are verified and found correct will be taken into account for the purpose of granting affiliation.</p>";
            annexure += "</br><table width='100%'  cellspacing='0'></br>";
            annexure += "<tr><td align='right' width='80%' style='font-size: 14px; font-weight: normal;'> <b>Sd /-</b></td></tr>";
            annexure += "<tr><td align='right' width='80%' style='font-size: 14px; font-weight: normal;'> <b>REGISTRAR</b></td></tr></br></br>";
            annexure += "</br><tr><td align='center' width='80%' style='font-size: 14px; font-weight: normal;' colspan='2'><b>The College shall submit Appeal, if any through Online mode only in the format " +
                       "prescribed within 10 Days from the date of this letter." + "</b></td></tr></br>";
            annexure += "<tr><td></td></tr>"; annexure += "</table>";


            return annexure;
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
            public int DegreeId { get; set; }
            public int? degreeDisplayOrder { get; set; }
            public string collegeRandomCode { get; set; }
            public int ProposedIntake { get; set; }
            public int approvedIntake1 { get; set; }
            public int approvedIntake2 { get; set; }
            public int approvedIntake3 { get; set; }
            public int approvedIntake4 { get; set; }
            public int approvedIntake5 { get; set; }




            //Added this in 25-04-2017
            public int admittedIntake1 { get; set; }
            public int admittedIntake2 { get; set; }
            public int admittedIntake3 { get; set; }
            public int admittedIntake4 { get; set; }
            public int admittedIntake5 { get; set; }

            public int SanctionIntake1 { get; set; }
            public int SanctionIntake2 { get; set; }
            public int SanctionIntake3 { get; set; }
            public int SanctionIntake4 { get; set; }
            public int SanctionIntake5 { get; set; }

            public int totalAdmittedIntake { get; set; }
            //

            public bool AffiliationStatus2 { get; set; }
            public bool AffiliationStatus3 { get; set; }
            public bool AffiliationStatus4 { get; set; }

            public int division1 { get; set; }
            public int division2 { get; set; }
            public int division3 { get; set; }




            public int totalIntake { get; set; }
            public decimal requiredFaculty { get; set; }
            public int phdFaculty { get; set; }
            public int SpecializationsphdFaculty { get; set; }
            public int SpecializationspgFaculty { get; set; }
            public int pgFaculty { get; set; }
            public int ugFaculty { get; set; }
            public int totalFaculty { get; set; }
            public int specializationWiseFaculty { get; set; }
            public int PharmacyspecializationWiseFaculty { get; set; }
            public int facultyWithoutPANAndAadhaar { get; set; }
            public int newlyAddedFaculty { get; set; }

            public bool isActive { get; set; }
            public DateTime? nbaFrom { get; set; }
            public DateTime? nbaTo { get; set; }

            public bool? deficiency { get; set; }
            public bool? PHDdeficiency { get; set; }
            public bool? PHDBtechdeficiency { get; set; }
            public int shortage { get; set; }
            public IList<Lab> LabsListDefs { get; set; }
           // public List<AnonymousLabclass> LabsListDefs1 { get; set; }
            //public List<AnonymousMBAMACclass> MBAMACDetails { get; set; }
            public bool deficiencystatus { get; set; }
            public string RegistrationNumber { get; set; }
            //=====18-06-2015=====//
            public int FalseNameFaculty { get; set; }
            public int FalsePhotoFaculty { get; set; }
            public int FalsePANNumberFaculty { get; set; }
            public int FalseAadhaarNumberFaculty { get; set; }
            public int CertificatesIncompleteFaculty { get; set; }
            public int AbsentFaculty { get; set; }
            public int AvailableFaculty { get; set; }
            public int AvailablePHDFaculty { get; set; }

           

            //For collegeintake

            public List<CollegeIntakeExisting> CollegeIntakeExistings { get; set; }

            public string AffliationStatus { get; set; }
            public decimal BphramacyrequiredFaculty { get; set; }
            public decimal pharmadrequiredfaculty { get; set; }
            public decimal pharmadPBrequiredfaculty { get; set; }
            public int totalcollegefaculty { get; set; }
            public int SortId { get; set; }

            public IList<CollegeFacultyWithIntakeReport> FacultyWithIntakeReports { get; set; }
            public int BtechAdjustedFaculty { get; set; }
            public int specializationWiseFacultyPHDFaculty { get; set; }
            public IList<PhysicalLabMaster> PhysicalLabs { get; set; }
        }

        

    }

}
