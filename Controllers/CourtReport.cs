using iTextSharp.text;
using iTextSharp.text.html.simpleparser;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using UAAAS.Models;
namespace UAAAS.Controllers
{
    //Written By Siva
    public class CourtReportController : Controller
    {
        //
        // GET: /CourtReport/

        //Written By Siva
        uaaasDBContext db= new uaaasDBContext();

        public ActionResult Index()
        {
            return View();
        }
     

        public ActionResult DeficiencyInactiveFacultyWordFile(string id ,string CollegeType)
        {
            if (!string.IsNullOrEmpty(id))
            {
                int collegeID = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(id.ToString(), System.Web.Configuration.WebConfigurationManager.AppSettings["CryptoKey"]));

                var integreatedIds = new[] { 9, 39, 42, 75, 140, 180, 235, 332, 364 };

                var PharmacyCollegeIds = new int[] { 448, 159, 376, 428, 445, 24, 117, 202, 213, 395, 27, 30, 6, 34, 55, 58, 54, 52, 60, 65, 78, 150, 204, 219, 90, 97, 104, 110, 139, 105, 107, 114, 379, 118, 120, 389, 127, 237, 135, 136, 47, 252, 169, 253, 262, 442, 301, 436, 370, 263, 267, 206, 44, 290, 454, 95, 297, 384, 348, 298, 295, 303, 302, 283, 410, 392, 234, 146, 315, 320, 317, 314, 318, 319, 353, 313, 375 };

                if (!PharmacyCollegeIds.Contains(collegeID) && CollegeType == "Others")
                {
                    Response.ClearContent();
                    Response.ClearHeaders();
                    Response.Buffer = true;
                    string collegeCode = db.jntuh_college.Where(c => c.isActive == true && c.id == collegeID).Select(c => c.collegeCode).FirstOrDefault();

                    Response.AddHeader("content-disposition", "attachment; filename=" + collegeCode + "-CourtReport-" + id + ".doc");
                    Response.ContentType = "application/vnd.ms-word ";
                    Response.Charset = string.Empty;
                    StringBuilder str = new StringBuilder();
                    // str.Append(Header(collegeID));
                    // str.Append("<br />");
                    str.Append(CollegeInformation(collegeID));
                    str.Append("<br />");
                    str.Append(Principal(collegeID));
                    str.Append("<br />");
                    ////str.Append(CommitteeMembers(collegeID));
                    ////str.Append("<br />");
                    str.Append(DeficienciesInactiveFaculty(collegeID));
                    str.Append("<br />");
                    ////old code
                    ////str.Append(DeficienciesInLabs(collegeID));
                    ////New code
                    //Commented by Naryana
                    str.Append(DeficiencyCollegeLabsAnnexure(collegeID));
                    str.Append("<br />");

                    ////Old Code
                    //// str.Append(CollegeLabsAnnexure(collegeID));
                    ////New Old
                    str.Append(DeficiencyPhysicalLabs(collegeID));

                    str.Append("<br />");

                    Document pdfDoc = new Document(PageSize.A4.Rotate(), 60, 50, 60, 60);

                    pdfDoc.SetPageSize(iTextSharp.text.PageSize.A4.Rotate());
                    pdfDoc.SetMargins(60, 50, 60, 60);

                    string path = Server.MapPath("~/Content/PDFReports/CourtReports/");

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
                else
                {
                    Response.ClearContent();
                    Response.ClearHeaders();
                    Response.Buffer = true;
                    string collegeCode = db.jntuh_college.Where(c => c.isActive == true && c.id == collegeID).Select(c => c.collegeCode).FirstOrDefault();

                    Response.AddHeader("content-disposition", "attachment; filename=" + collegeCode + "-CourtReport-" + id + ".doc");
                    Response.ContentType = "application/vnd.ms-word ";
                    Response.Charset = string.Empty;
                    StringBuilder str = new StringBuilder();
                    // str.Append(Header(collegeID));
                    // str.Append("<br />");
                    str.Append(CollegeInformation(collegeID));
                    str.Append("<br />");
                    str.Append(Principal(collegeID));
                    str.Append("<br />");
                    ////str.Append(CommitteeMembers(collegeID));
                    ////str.Append("<br />");
                    str.Append(PharmacyInactiveFaculty(collegeID));
                    str.Append("<br />");
                    ////old code
                    ////str.Append(DeficienciesInLabs(collegeID));
                    ////New code
                    //Commented by Naryana
                    //str.Append(DeficiencyCollegeLabsAnnexure(collegeID));
                    //str.Append("<br />");

                    ////Old Code
                    //// str.Append(CollegeLabsAnnexure(collegeID));
                    ////New Old
                    //str.Append(DeficiencyPhysicalLabs(collegeID));

                    //str.Append("<br />");

                    Document pdfDoc = new Document(PageSize.A4.Rotate(), 60, 50, 60, 60);

                    pdfDoc.SetPageSize(iTextSharp.text.PageSize.A4.Rotate());
                    pdfDoc.SetMargins(60, 50, 60, 60);

                    string path = Server.MapPath("~/Content/PDFReports/CourtReports/");

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
            }

            return View();
        }

        //Court Report for B.Tech, M.tech,MBA,MCA
        #region Court Report for B.Tech, M.tech,MBA,MCA
        public string DeficienciesInactiveFaculty(int? collegeID)
        {
            string Humanities = string.Empty;
            string Humanities1 = string.Empty;
            string facultyclosure = string.Empty;
            string twintyfivepercentbelowcurces = string.Empty;
            string DeficiencyInactiveFaculty = string.Empty;

            string faculty = string.Empty;
            string facultyAdmittedIntakeZero = string.Empty;
            string facultyAdmittedIntakeZeroTable2 = string.Empty;
            int facultycount = 0;
            int Index = 1;

            int[] shiftids = { 1, 2 };

            var jntuh_academic_year = db.jntuh_academic_year.ToList();

            int presentYear = jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int AY1 = jntuh_academic_year.Where(a => a.actualYear == (presentYear + 1)).Select(a => a.id).FirstOrDefault();
            int AY2 = jntuh_academic_year.Where(a => a.actualYear == (presentYear)).Select(a => a.id).FirstOrDefault();
            int AY3 = jntuh_academic_year.Where(a => a.actualYear == (presentYear - 1)).Select(a => a.id).FirstOrDefault();
            int AY4 = jntuh_academic_year.Where(a => a.actualYear == (presentYear - 2)).Select(a => a.id).FirstOrDefault();
            int AY5 = jntuh_academic_year.Where(a => a.actualYear == (presentYear - 3)).Select(a => a.id).FirstOrDefault();
            List<jntuh_college_intake_existing> intake = db.jntuh_college_intake_existing.Where(i => i.collegeId == collegeID).ToList();




            List<CollegeFacultyWithIntakeReport> facultyCountslist = new List<CollegeFacultyWithIntakeReport>();
            List<CollegeFacultyWithIntakeReport> facultyCounts = collegeInactiveFaculty(collegeID).Where(c => c.shiftId == 1 || c.shiftId == 2).Select(e => e).ToList();//Where(c => c.shiftId == 1)
            List<CollegeFacultyWithIntakeReport> facultyCountsmetechsecond = facultyCounts.Where(c => c.shiftId == 2).Select(e => e).ToList();
            foreach (var item in facultyCountsmetechsecond)
            {
                int id =
                    facultyCounts.Where(
                        s => s.specializationId == item.specializationId && s.shiftId == 1 && s.Degree == "M.Tech" && s.Proposedintake != 0)
                        .Select(s => s.shiftId)
                        .FirstOrDefault();
                if (id == 0)
                {
                    facultyCounts.Remove(item);
                }
            }
            List<CollegeFacultyWithIntakeReport> facultyCountper = collegeInactiveFaculty(collegeID).Where(c => (c.ispercentage == true && c.Proposedintake != 0 && c.Degree == "B.Tech") || c.Proposedintake == 0 && c.Degree == "B.Tech").Select(e => e).ToList();
            foreach (var itemmtech in facultyCountper)
            {
                List<CollegeFacultyWithIntakeReport> notshownmtech = facultyCounts.Where(
                        s => s.Department == itemmtech.Department && s.Degree == "M.Tech" && s.Proposedintake != 0)
                        .Select(s => s)
                        .ToList();
                if (notshownmtech.Count() != 0)
                {
                    // facultyCounts.Remove(itemmtech);
                    foreach (var removemtech in notshownmtech)
                    {
                        facultyCounts.Remove(removemtech);
                    }

                }
            }
            //


            var collegeStatus = db.jntuh_collegestatus.Where(e => e.CollegeId == collegeID).Select(e => e).FirstOrDefault();
            if (collegeStatus != null)
            {
                if (collegeStatus.SIStatus == true)
                {

                    facultyCounts = facultyCounts.Where(e => e.Degree == "B.Tech").Select(e => e).ToList();
                }
            }

            if (collegeStatus != null)
            {
                var CollegeSubmissionDate = Convert.ToDateTime(db.jntuh_college_edit_status.Where(e => e.collegeId == collegeID&&e.academicyearId==AY1 && e.IsCollegeEditable == false).Select(e => e.updatedOn).FirstOrDefault()).Date;
                // DateTime data = Convert.ToDateTime(CollegeSubmissionDate).Date;
                if (collegeStatus.AICTEStatus == true)
                {

                    var RegisteredFaculty = (from Reg in db.jntuh_registered_faculty
                                             join Clg in db.jntuh_college_faculty_registered on Reg.RegistrationNumber equals Clg.RegistrationNumber
                                             join ClgEdit in db.jntuh_college_edit_status on Clg.collegeId equals ClgEdit.collegeId
                                             where ClgEdit.IsCollegeEditable == false && Clg.collegeId == collegeID
                                             select Reg).ToList();

                    int Absebtfaculty = RegisteredFaculty.Count(e => e.Absent == true);
                    int TotalFaculty = RegisteredFaculty.Count();

                    var Percentage = Math.Ceiling((((double)Absebtfaculty / (double)TotalFaculty) * 100));







                    //faculty += "</table>";
                    //faculty += "<p>Sub: - Affiliation for the Academic Year: 2018-19- Reg.</p>";
                    //faculty += "<br/>";
                    //faculty += "<table><tr><td style='vertical-align:top'>Ref:</td><td>";
                    //faculty += "<ol type='1'>";
                    //faculty += "<li>Your college online application dated:" + CollegeSubmissionDate + " for grant of Affiliation for the Academic Year 2017-18.</li>";
                    //faculty += "<li>Your college FFC Inspection on  dated: ----------- </li>";
                    //faculty += "</ol></td></tr></table>";
                    //faculty += "<br/>";
                    //faculty += "<p align='center'><b>****</b></p>";

                    //faculty += "<p style='text-align:justfy'>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;As per the fact finding Committee inspection report it is noticed that " + Percentage + " of faculty members were absent against the number of faculty shown in online application for affiliation A117. It indicates that the College is not maintaining the minimum essential requirements for running the academic programs. Under these circumstances the SCA recommends <b><u>'No Admission Status'</u></b> for the A.Y. 2017-18 for the Institute. Clarification or explanation if any may be submitted within 10 days from the date of this letter</p>";

                    //faculty += "<table width='100%'  cellspacing='0'>";
                    //faculty += "<tr><td align='right' width='80%' style='font-size: 14px; font-weight: normal;'> <b>Sd /-</b></td></tr>";
                    //faculty += "<tr><td align='right' width='80%' style='font-size: 14px; font-weight: normal;'> <b>REGISTRAR</b></td></tr>";
                    //faculty += "</table>";


                }
                else if (collegeStatus.CollegeStatus == true)
                {
                    faculty += "<p>Sub: - Affiliation for the Academic Year: 2018-19- Reg.</p>";
                    faculty += "<br/>";
                    faculty += "<table><tr><td style='vertical-align:top'>Ref:</td><td>";
                    faculty += "<ol type='1'>";
                    faculty += "<li>Your college online application dated:" + CollegeSubmissionDate + " for grant of Affiliation for the Academic Year 2017-18.</li>";
                    faculty += "<li>Your college FFC Inspection on  dated: ----------- </li>";
                    faculty += "</ol></td></tr></table>";
                    faculty += "<br/>";
                    faculty += "<p align='center'><b>****</b></p>";
                }

            }

            var jntuh_departments = db.jntuh_department.Where(e => e.isActive == true).ToList();
            var jntuh_specialization = db.jntuh_specialization.Where(e => e.isActive == true).ToList();


            var jntuh_college_faculty = db.jntuh_college_faculty_registered.Where(e => e.collegeId == collegeID).Select(e => e).ToList();
            var strRegnosNew = jntuh_college_faculty.Select(e => e.RegistrationNumber).ToArray();
            var jntuh_registred_faculty = db.jntuh_registered_faculty.Where(e => strRegnosNew.Contains(e.RegistrationNumber)).Select(e => e).ToList();




            var CollegeClearedFaculty = jntuh_registred_faculty.Where(rf => rf.type != "Adjunct" && ((rf.Absent == false||rf.Absent==null) && (rf.OriginalCertificatesNotShown == false||rf.OriginalCertificatesNotShown==null) &&
                                         (rf.Xeroxcopyofcertificates == false || rf.Xeroxcopyofcertificates == null) && (rf.NotQualifiedAsperAICTE == false || rf.NotQualifiedAsperAICTE == null)
                                                        && (rf.NoSCM == false || rf.NoSCM == null) && (rf.PANNumber != null) && (rf.IncompleteCertificates == false || rf.IncompleteCertificates == null) && (rf.Blacklistfaculy == false)
                                                        && rf.NoRelevantUG != "Yes" && rf.NoRelevantPG != "Yes" && rf.NORelevantPHD != "Yes" && (rf.NotconsideredPHD == false || rf.NotconsideredPHD == null) && (rf.Genuinenessnotsubmitted == false || rf.Genuinenessnotsubmitted == null) && (rf.InvalidPANNumber == false || rf.InvalidPANNumber == null) && (rf.OriginalsVerifiedPHD != true) && (rf.OriginalsVerifiedUG != true) && (rf.Invaliddegree == false || rf.Invaliddegree == null) && rf.BAS != "Yes") && rf.InvalidAadhaar != "Yes" && (rf.DepartmentId != 61 || rf.DepartmentId != 27)).Select(rf => rf).ToList();
            var FacultyIds = jntuh_registred_faculty.Select(e => e.id).ToArray();

            var jntuh_registered_faculty_education = db.jntuh_registered_faculty_education.Where(e => FacultyIds.Contains(e.facultyId)).Select(e => e).ToList();
            var ClearedFacultyRegnos = CollegeClearedFaculty.Select(e => e.RegistrationNumber).ToArray();



            var CollegeInactiveFaculty = jntuh_registred_faculty.Where(e => !ClearedFacultyRegnos.Contains(e.RegistrationNumber)).Select(a => new
            {
                type = a.type,
                RegistrationNumber = a.RegistrationNumber.Trim(),
                FullName = a.FirstName + " " + a.MiddleName + " " + a.LastName,
                DeptId = jntuh_college_faculty.Where(c => c.RegistrationNumber.Trim() == a.RegistrationNumber.Trim()).Select(c => c.DepartmentId).FirstOrDefault(),
                SpecializationId = jntuh_college_faculty.Where(c => c.RegistrationNumber.Trim() == a.RegistrationNumber.Trim()).Select(c => c.SpecializationId).FirstOrDefault(),
                HighestDegreeID = a.jntuh_registered_faculty_education.Count() != 0 ? a.jntuh_registered_faculty_education.Select(e => e.educationId).Max() : 0,
                IsApproved = a.isApproved,
                PanNumber = a.PANNumber,
                AadhaarNumber = a.AadhaarNumber,
                DegreeId = jntuh_registered_faculty_education.Count(e => e.facultyId == a.id) > 0 ? a.jntuh_registered_faculty_education.Where(e => e.facultyId == a.id).Select(e => e.educationId).Max() : 0,

                Absent = a.Absent != null ? (bool)a.Absent : false,
                BlacklistFaculty = a.Blacklistfaculy != null ? (bool)a.Blacklistfaculy : false,
                PHDundertakingnotsubmitted = a.PHDundertakingnotsubmitted != null ? (bool)a.PHDundertakingnotsubmitted : false,
                NOTQualifiedAsPerAICTE = a.NotQualifiedAsperAICTE != null ? (bool)a.NotQualifiedAsperAICTE : false,
                InvalidPANNo = a.InvalidPANNumber != null ? (bool)a.InvalidPANNumber : false,
                InCompleteCeritificates = a.IncompleteCertificates != null ? (bool)a.IncompleteCertificates : false,
                OriginalCertificatesnotshownFlag = a.OriginalCertificatesNotShown != null ? (bool)a.OriginalCertificatesNotShown : false,
                FalsePAN = a.FalsePAN != null ? (bool)a.FalsePAN : false,
                NOForm16 = a.NoForm16 != null ? (bool)a.NoForm16 : false,
               // MultipleReginSamecoll = a.MultipleRegInSameCollege != null ? (bool)a.MultipleRegInSameCollege : false,
                XeroxcopyofcertificatesFlag = a.Xeroxcopyofcertificates != null ? (bool)a.Xeroxcopyofcertificates : false,
                NotIdentityFiedForAnyProgramFlag = a.NotIdentityfiedForanyProgram != null ? (bool)a.NotIdentityfiedForanyProgram : false,
                NOrelevantUgFlag = a.NoRelevantUG == "Yes" ? true : false,
                NOrelevantPgFlag = a.NoRelevantPG == "Yes" ? true : false,
                NOrelevantPhdFlag = a.NORelevantPHD == "Yes" ? true : false,
                //NoForm16Verification = a.Noform16Verification != null ? (bool)a.Noform16Verification : false,
                NoSCM = a.NoSCM != null ? (bool)a.NoSCM : false,
                //PhotocopyofPAN = a.PhotoCopyofPAN != null ? (bool)a.PhotoCopyofPAN : false,
                PhdUndertakingDocumentstatus = a.PhdUndertakingDocumentstatus != null ? (bool)(a.PhdUndertakingDocumentstatus) : false,
                PHDUndertakingDocumentView = a.PHDUndertakingDocument,
                PhdUndertakingDocumentText = a.PhdUndertakingDocumentText,
                //AppliedPAN = a.AppliedPAN != null ? (bool)(a.AppliedPAN) : false,
                //SamePANUsedByMultipleFaculty = a.SamePANUsedByMultipleFaculty != null ? (bool)(a.SamePANUsedByMultipleFaculty) : false,
                Basstatus = a.BAS,
                inValidAadhaar = a.InvalidAadhaar,
                NotconsideredPHD = a.NotconsideredPHD,
                Genuinenessnotsubmitted = a.Genuinenessnotsubmitted,
                AbsentforVerification = a.AbsentforVerification,
                NoPGspecialization = a.NoPGspecialization,
                Noclass = a.Noclass,
                //Basstatus Column Consider as Aadhaar Flag 
                
                Deactivedby = a.DeactivatedBy,
                DeactivedOn = a.DeactivatedOn,
                //SCMDocumentView =
                //    db.jntuh_scmupload.Where(u => u.ProfDocument.Trim() == faculty.RegistrationNumber.Trim() && u.DepartmentId != 0 && u.SpecializationId != 0)
                //        .Select(s => s.AssistDocument)
                //        .FirstOrDefault(),
                OriginalsVerifiedPHD = a.OriginalsVerifiedPHD == true ? true : false,
                OriginalsVerifiedUG = a.OriginalsVerifiedUG == true ? true : false

            }).ToList();

            var jntuh_college_basreport = db.jntuh_college_basreport.Where(e => e.collegeId == collegeID).Select(e => e).ToList();

            var count = facultyCounts.Count();
            var distDeptcount = 1;
            var deptloop = 1;
            decimal departmentWiseRequiredFaculty = 0;

            string[] departments = new string[] { "English", "Mathematics", "Physics", "Chemistry", "Others(CSE/IT)", "Others(CIVIL/MECH)", "Others(ECE/EEE)", "Others(MNGT/H&S)" };

            int[] OthersSpecIds = new int[] { 155, 156, 157, 158 };
            int totalBtechFirstYearIntake = facultyCounts.Where(d => d.Degree == "B.Tech" && !departments.Contains(d.Department)).Select(d => d.Proposedintake).Sum();
            var degrees = db.jntuh_degree.ToList();
            var firstYearRequired = Math.Ceiling((double)totalBtechFirstYearIntake / 160);
            int remainingFaculty = 0;
            int remainingPHDFaculty = 0;
            int SpecializationwisePHDFaculty = 0;
            int SpecializationwisePGFaculty = 0;
            int TotalCount = 0;
            int HumantitiesminimamRequireMet = 0;
            string HumantitiesminimamRequireMetStatus = "Yes";



            string[] ProposedIntakeZeroDeptName = facultyCounts.Where(e => e.Degree == "B.Tech" && (e.Proposedintake == 0) && !departments.Contains(e.Department)).Select(e => e.Specialization).Distinct().ToArray();
            string[] twentyfivePersentbelowDeptName = facultyCounts.Where(e => e.Degree == "B.Tech" && (e.ispercentage == true) && (e.Proposedintake != 0) && !departments.Contains(e.Department)).Select(e => e.Specialization).Distinct().ToArray();
            if (collegeStatus != null)
            {
                if (collegeStatus.CollegeStatus == false && collegeStatus.AICTEStatus == false)
                {

                    //faculty += "<table width='100%' border='0' cellpadding='5' cellspacing='0'>";
                    //faculty += "<tr>";
                    //faculty += "<td align='left'><b><u>Deficiencies in Faculty</u></b> (Department/ Specialization Wise):";
                    //faculty += "</tr>";
                    //faculty += "</table>";
                    //faculty += "<table  border='1' cellpadding='3' cellspacing='0' style='border-color: #ccc;'>";
                    //faculty += "<tr>";
                    //faculty += "<th  style='text-align: center; vertical-align: top;'>SNo</th>";
                    //faculty += "<th  style='text-align: left; vertical-align: top;' >Dept</th>";
                    //faculty += "<th  style='text-align: left; vertical-align: top;' >Degree</th>";
                    //faculty += "<th  style='text-align: left; vertical-align: top;' >Specialization</th>";                
                    //faculty += "<th style='text-align: center; vertical-align: top;' >Proposed Intake</th>";
                    //faculty += "<th style='text-align: center; vertical-align: top;'>Required <br/>Fauclty</th>";
                    //faculty += "<th style='text-align: center; vertical-align: top;'>Available <br/>Fauclty</th>";
                    //faculty += "<th  style='text-align: center; vertical-align: top;' >PG Specialization Faculty</th>";
                    //faculty += "<th style='text-align: center; vertical-align: top;'>Required <br/>Ph.D</th>";
                    //faculty += "<th style='text-align: center; vertical-align: top;'>Available <br/>Ph.D</th>";
                    //faculty += "<th  style='text-align: center; vertical-align: top;' >Deficiency</th>";
                    //faculty += "</tr>";


                    string[] admittedIntakeTwoormoreZeroDeptName1 = facultyCounts.Where(e => e.Degree == "B.Tech" && ((e.approvedIntake2 == 0 && e.approvedIntake3 == 0) || (e.approvedIntake3 == 0 && e.approvedIntake4 == 0) || (e.approvedIntake2 == 0 && e.approvedIntake4 == 0)) && !departments.Contains(e.Department)).Select(e => e.Department).Distinct().ToArray();
                    //


                    #region Intake not equal to Zero

                    // Getting M.Tech Civil Specialization ID's
                    int[] SpecializationIDS =
                        db.jntuh_specialization.Where(S => S.departmentId == 43).Select(S => S.id).ToArray();
                    int remainingFaculty2 = 0;
                    int facultyIndexnotEqualtoZeroIndex = 1;
                    int facultyIndexnotEqualtoZeroAdmittedIntakeIndex = 1;
                    //int btechcount = facultyCounts.Where(
                    //    e =>
                    //        e.Proposedintake != 0 && e.ispercentage == false && e.Degree == "B.Tech" &&
                    //        !ProposedIntakeZeroDeptName.Contains(e.Specialization) &&
                    //        !departments.Contains(e.Department)).Select(e => e).ToList().Count;
                    int btechcount = facultyCounts.Where(
                       e =>
                           e.Proposedintake != 0 && e.Degree == "B.Tech" &&
                           !ProposedIntakeZeroDeptName.Contains(e.Specialization) &&
                           !departments.Contains(e.Department)).Select(e => e).ToList().Count;
                    if (btechcount == 0)
                    {
                        foreach (var dep in departments)
                        {
                            CollegeFacultyWithIntakeReport CollegeFacultyWithIntakeReport =
                                facultyCounts.Where(d => d.Department == dep).Select(s => s).FirstOrDefault();
                            facultyCounts.Remove(CollegeFacultyWithIntakeReport);
                        }
                    }


                    foreach (
                        var item in
                            facultyCounts.Where(
                                e =>
                                    e.Proposedintake != 0 &&
                                    !ProposedIntakeZeroDeptName.Contains(e.Department)).Select(e => e).ToList())
                    {


                        //faculty += "<table width='100%' border='1' cellpadding='3' cellspacing='0' style='border-color: #ccc;'>";
                        //faculty += "<tr>";
                        //faculty += "<td align='left'><b><u>Deficiencies in Faculty</u></b> (Department/ Specialization Wise):";
                        //faculty += "</tr>";
                        //faculty += "</table>";



                        #region Old Code without calculation admitted Intake

                        if (item.Degree == "M.Tech" || item.Degree == "MCA" || item.Degree == "MBA" ||
                            item.Degree == "5-Year MBA(Integrated)")
                        {

                            if (item.Degree == "M.Tech")
                            {
                                SpecializationwisePHDFaculty = IntakeWisePhdForMtech(item.Proposedintake,
                                    item.shiftId);
                            }
                            else if (item.Degree == "MCA")
                            {
                                SpecializationwisePHDFaculty = IntakeWisePhdForMBAandMCA(item.Proposedintake);
                            }
                            else if (item.Degree == "MBA" || item.Degree == "5-Year MBA(Integrated)")
                            {
                                SpecializationwisePHDFaculty = IntakeWisePhdForMBAandMCA(item.Proposedintake);
                            }


                        }
                        else if (item.Degree == "B.Tech")
                        {
                            SpecializationwisePHDFaculty = IntakeWisePhdForBtech(item.Proposedintake);
                        }
                        TotalCount =
                            facultyCounts.Where(
                                D =>
                                    D.Department == item.Department &&
                                    (D.Degree == "M.Tech" || D.Degree == "B.Tech") && D.Proposedintake != 0)
                                .Distinct()
                                .Count();




                        int indexnow = facultyCounts.IndexOf(item);



                        if (indexnow > 0 &&
                            facultyCounts[indexnow].Department != facultyCounts[indexnow - 1].Department)
                        {
                            deptloop = 1;
                        }



                        string minimumRequirementMet = string.Empty;
                        string PhdminimumRequirementMet = string.Empty;
                        int facultyShortage = 0, tFaculty = 0;
                        int adjustedFaculty = 0;
                        int adjustedPHDFaculty = 0;
                        int othersRequiredfaculty = 0;

                        if (item.Department == "MBA" || item.Department == "MCA")
                            tFaculty = (int)Math.Ceiling(Convert.ToDecimal(item.totalFaculty)); //item.totalFaculty
                        else
                            tFaculty = (int)Math.Ceiling(Convert.ToDecimal(item.specializationWiseFaculty));
                        //item.totalFaculty
                        int rFaculty = (int)Math.Ceiling(item.requiredFaculty);

                        if (departments.Contains(item.Department))
                        {
                            if (OthersSpecIds.Contains(item.specializationId))
                            {
                                double rid = (double)(firstYearRequired / 2);
                                rFaculty = (int)(Math.Ceiling(rid));



                            }
                            else
                            {
                                rFaculty = (int)firstYearRequired;
                            }


                        }

                        var degreeType =
                            degrees.Where(d => d.degree == item.Degree)
                                .Select(d => d.jntuh_degree_type.degreeType)
                                .FirstOrDefault();

                        if (deptloop == 1)
                        {
                            if (rFaculty <= tFaculty)
                            {
                                minimumRequirementMet = "YES";
                                item.deficiency = false;
                                remainingFaculty = tFaculty - rFaculty;
                                adjustedFaculty = rFaculty; //tFaculty
                            }
                            else
                            {
                                minimumRequirementMet = "NO";
                                item.deficiency = true;
                                adjustedFaculty = tFaculty;
                                remainingFaculty = 0;
                                // facultyShortage = rFaculty - tFaculty;
                            }

                            remainingPHDFaculty = item.phdFaculty;

                            if (remainingPHDFaculty >= 2 && (degreeType.Equals("PG")) &&
                                item.SpecializationsphdFaculty > 0)
                            {
                                if (remainingPHDFaculty >= item.SpecializationsphdFaculty)
                                {
                                    if (item.shiftId == 1)
                                    {
                                        remainingPHDFaculty = remainingPHDFaculty - item.SpecializationsphdFaculty;
                                        if (item.SpecializationsphdFaculty >= SpecializationwisePHDFaculty)
                                        {
                                            adjustedPHDFaculty = SpecializationwisePHDFaculty;
                                        }
                                        else
                                        {
                                            adjustedPHDFaculty = item.SpecializationsphdFaculty;
                                        }
                                        if (adjustedPHDFaculty >= SpecializationwisePHDFaculty)
                                            PhdminimumRequirementMet = "YES";
                                        else
                                            PhdminimumRequirementMet = "NO";
                                    }
                                    else
                                    {
                                        remainingPHDFaculty = remainingPHDFaculty - SpecializationwisePHDFaculty;
                                        adjustedPHDFaculty = SpecializationwisePHDFaculty;
                                        if (adjustedPHDFaculty >= SpecializationwisePHDFaculty)
                                            PhdminimumRequirementMet = "YES";
                                        else
                                            PhdminimumRequirementMet = "NO";
                                    }
                                }
                                else
                                {
                                    remainingPHDFaculty = remainingPHDFaculty;
                                    adjustedPHDFaculty = remainingPHDFaculty;

                                    if (remainingPHDFaculty >= SpecializationwisePHDFaculty)
                                        PhdminimumRequirementMet = "YES";
                                    else
                                        PhdminimumRequirementMet = "NO";
                                }
                            }
                            else if (remainingPHDFaculty <= 1 && (degreeType.Equals("PG")) &&
                                     item.SpecializationsphdFaculty > 0)
                            {
                                //adjustedPHDFaculty = remainingPHDFaculty;
                                //remainingPHDFaculty = remainingPHDFaculty - 1;
                                //PhdminimumRequirementMet = "NO";
                                if (remainingPHDFaculty >= SpecializationwisePHDFaculty)
                                {
                                    remainingPHDFaculty = remainingPHDFaculty - SpecializationwisePHDFaculty;
                                    adjustedPHDFaculty = SpecializationwisePHDFaculty;

                                    PhdminimumRequirementMet = "YES";
                                }
                                else if (remainingPHDFaculty <= SpecializationwisePHDFaculty)
                                {

                                    adjustedPHDFaculty = remainingPHDFaculty;
                                    remainingPHDFaculty = 0;
                                    PhdminimumRequirementMet = "NO";
                                }

                            }
                            else if (SpecializationwisePHDFaculty > 0 && (degreeType.Equals("UG")) &&
                                     remainingPHDFaculty > 0)
                            {
                                if (remainingPHDFaculty >= SpecializationwisePHDFaculty)
                                {
                                    remainingPHDFaculty = remainingPHDFaculty - SpecializationwisePHDFaculty;
                                    adjustedPHDFaculty = SpecializationwisePHDFaculty;

                                    PhdminimumRequirementMet = "YES";
                                }
                                else if (remainingPHDFaculty <= SpecializationwisePHDFaculty)
                                {

                                    adjustedPHDFaculty = remainingPHDFaculty;
                                    remainingPHDFaculty = 0;
                                    PhdminimumRequirementMet = "NO";
                                }
                            }
                            else if (SpecializationwisePHDFaculty > 0 && (degreeType.Equals("UG")) &&
                                     remainingPHDFaculty <= 0)
                            {
                                // remainingPHDFaculty = SpecializationwisePHDFaculty - remainingPHDFaculty;
                                adjustedPHDFaculty = remainingPHDFaculty;
                                PhdminimumRequirementMet = "NO";

                            }
                            else if (SpecializationwisePHDFaculty == 0 && (degreeType.Equals("UG")))
                            {
                                PhdminimumRequirementMet = "NO";
                            }
                            else if (adjustedPHDFaculty <= 0 && (degreeType.Equals("PG")))
                            {
                                PhdminimumRequirementMet = "YES";
                            }
                            else if (remainingPHDFaculty >= 2 && (degreeType.Equals("Dual Degree")) &&
                                     item.SpecializationsphdFaculty > 0)
                            {
                                if (remainingPHDFaculty >= item.SpecializationsphdFaculty)
                                {
                                    remainingPHDFaculty = remainingPHDFaculty -
                                                          item.SpecializationsphdFaculty;
                                    adjustedPHDFaculty = item.SpecializationsphdFaculty;
                                    if (adjustedPHDFaculty >= SpecializationwisePHDFaculty)
                                        PhdminimumRequirementMet = "YES";
                                    else
                                        PhdminimumRequirementMet = "NO";
                                }
                                else
                                {

                                    remainingPHDFaculty = remainingPHDFaculty;
                                    adjustedPHDFaculty = remainingPHDFaculty;

                                    if (remainingPHDFaculty >= SpecializationwisePHDFaculty)
                                        PhdminimumRequirementMet = "YES";
                                    else
                                        PhdminimumRequirementMet = "NO";
                                }

                            }
                            else if (remainingPHDFaculty <= 1 && (degreeType.Equals("Dual Degree")) &&
                                     item.SpecializationsphdFaculty >= 0)
                            {
                                adjustedPHDFaculty = remainingPHDFaculty;
                                remainingPHDFaculty = remainingPHDFaculty - 1;
                                if (remainingPHDFaculty < 0)
                                {
                                    remainingPHDFaculty = 0;
                                }
                                if (adjustedPHDFaculty >= SpecializationwisePHDFaculty)
                                    PhdminimumRequirementMet = "YES";
                                else
                                    PhdminimumRequirementMet = "NO";

                            }
                            else if (SpecializationwisePHDFaculty == 0 &&
                                     (degreeType.Equals("Dual Degree")))
                            {
                                PhdminimumRequirementMet = "NO";
                            }

                        }
                        else
                        {
                            if (rFaculty <= remainingFaculty)
                            {
                                if (degreeType.Equals("PG"))
                                {
                                    minimumRequirementMet = "YES";
                                    item.deficiency = false;
                                    if (item.shiftId == 1)
                                    {
                                        if (item.specializationWiseFaculty >= rFaculty)
                                        {
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
                                            remainingFaculty = remainingFaculty - item.specializationWiseFaculty;
                                            adjustedFaculty = item.specializationWiseFaculty;
                                        }
                                    }
                                    else
                                    {
                                        if (item.shiftId == 2)
                                        {
                                            int firstshiftrequiredFaculty = (int)Math.Ceiling(facultyCounts.Where(e =>
                                         e.Proposedintake != 0 && e.ispercentage == false &&
                                         !ProposedIntakeZeroDeptName.Contains(e.Specialization) &&
                                         e.specializationId == item.specializationId && e.shiftId == 1)
                                         .Select(e => e.requiredFaculty)
                                         .FirstOrDefault());
                                            item.specializationWiseFaculty = item.specializationWiseFaculty -
                                                                                firstshiftrequiredFaculty;
                                            if (item.specializationWiseFaculty < 0)
                                            {
                                                item.SpecializationsphdFaculty = 0;
                                            }
                                        }
                                        if (item.specializationWiseFaculty >= rFaculty)
                                        {
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
                                            remainingFaculty = remainingFaculty - item.specializationWiseFaculty;
                                            adjustedFaculty = item.specializationWiseFaculty;
                                        }
                                    }
                                }
                                else
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
                            }
                            else
                            {
                                minimumRequirementMet = "NO";
                                item.deficiency = true;
                                // facultyShortage = rFaculty - remainingFaculty;
                                //taking only Specialiwise 
                                if (remainingFaculty >= item.specializationWiseFaculty)
                                {

                                    remainingFaculty = remainingFaculty - item.specializationWiseFaculty;
                                    adjustedFaculty = item.specializationWiseFaculty;
                                }
                                else
                                {
                                    adjustedFaculty = remainingFaculty;
                                    if (remainingFaculty == 0)
                                    {
                                        remainingPHDFaculty = 0;
                                    }
                                    remainingFaculty = 0;
                                }

                            }
                            //PG PHD Taking 
                            if (remainingPHDFaculty < SpecializationwisePHDFaculty && (degreeType.Equals("PG")))
                            {
                                //remainingPHDFaculty = remainingPHDFaculty - item.SpecializationsphdFaculty;
                                //adjustedPHDFaculty = remainingPHDFaculty;
                                //PhdminimumRequirementMet = "NO";
                                if (remainingPHDFaculty >= item.SpecializationsphdFaculty)
                                {
                                    if (item.shiftId == 1)
                                    {
                                        if (item.SpecializationsphdFaculty >= SpecializationwisePHDFaculty)
                                        {
                                            if (remainingPHDFaculty >= item.SpecializationsphdFaculty)
                                            {
                                                remainingPHDFaculty = remainingPHDFaculty - SpecializationwisePHDFaculty;
                                                adjustedPHDFaculty = SpecializationwisePHDFaculty;
                                                if (adjustedPHDFaculty >= SpecializationwisePHDFaculty)
                                                    PhdminimumRequirementMet = "YES";
                                                else
                                                    PhdminimumRequirementMet = "NO";
                                            }
                                            else
                                            {
                                                remainingPHDFaculty = remainingPHDFaculty - item.SpecializationsphdFaculty;
                                                adjustedPHDFaculty = item.SpecializationsphdFaculty;

                                                if (adjustedPHDFaculty >= SpecializationwisePHDFaculty)
                                                    PhdminimumRequirementMet = "YES";
                                                else
                                                    PhdminimumRequirementMet = "NO";
                                            }
                                        }
                                        else
                                        {
                                            remainingPHDFaculty = remainingPHDFaculty - item.SpecializationsphdFaculty;
                                            adjustedPHDFaculty = item.SpecializationsphdFaculty;
                                            PhdminimumRequirementMet = "NO";
                                        }

                                    }
                                    else
                                    {
                                        if (item.shiftId == 2)
                                        {
                                            int firstshiftintake = facultyCounts.Where(e =>
                                             e.approvedIntake1 != 0 && e.ispercentage == false &&
                                             !ProposedIntakeZeroDeptName.Contains(e.Specialization) &&
                                             e.specializationId == item.specializationId && e.shiftId == 1)
                                             .Select(e => e.approvedIntake1)
                                             .FirstOrDefault();
                                            int firstshiftPHDFaculty = IntakeWisePhdForMtech(firstshiftintake, 1);
                                            item.SpecializationsphdFaculty = item.SpecializationsphdFaculty -
                                                                             firstshiftPHDFaculty;
                                            if (item.SpecializationsphdFaculty < 0)
                                            {
                                                item.SpecializationsphdFaculty = 0;
                                            }
                                        }
                                        if (item.SpecializationsphdFaculty >= SpecializationwisePHDFaculty)
                                        {
                                            remainingPHDFaculty = remainingPHDFaculty - SpecializationwisePHDFaculty;
                                            adjustedPHDFaculty = SpecializationwisePHDFaculty;
                                            if (adjustedPHDFaculty >= SpecializationwisePHDFaculty)
                                                PhdminimumRequirementMet = "YES";
                                            else
                                                PhdminimumRequirementMet = "NO";
                                        }
                                        else
                                        {
                                            remainingPHDFaculty = remainingPHDFaculty - item.SpecializationsphdFaculty;
                                            adjustedPHDFaculty = item.SpecializationsphdFaculty;
                                            PhdminimumRequirementMet = "NO";
                                        }

                                    }
                                }
                                else
                                {
                                    if (item.shiftId == 2)
                                    {
                                        int firstshiftintake = facultyCounts.Where(e =>
                                            e.Proposedintake != 0 && e.ispercentage == false &&
                                            !ProposedIntakeZeroDeptName.Contains(e.Specialization) &&
                                            e.specializationId == item.specializationId && e.shiftId == 1)
                                            .Select(e => e.Proposedintake)
                                            .FirstOrDefault();
                                        int firstshiftPHDFaculty = IntakeWisePhdForMtech(firstshiftintake, 1);
                                        item.SpecializationsphdFaculty = item.SpecializationsphdFaculty -
                                                                         firstshiftPHDFaculty;
                                        if (item.SpecializationsphdFaculty < 0)
                                        {
                                            item.SpecializationsphdFaculty = 0;
                                        }
                                        if (item.SpecializationsphdFaculty >= SpecializationwisePHDFaculty)
                                        {
                                            remainingPHDFaculty = remainingPHDFaculty - SpecializationwisePHDFaculty;
                                            if (remainingPHDFaculty < 0)
                                            {
                                                remainingPHDFaculty = 0;
                                                adjustedPHDFaculty = remainingPHDFaculty;
                                                if (adjustedPHDFaculty >= SpecializationwisePHDFaculty)
                                                    PhdminimumRequirementMet = "YES";
                                                else
                                                    PhdminimumRequirementMet = "NO";
                                            }
                                            else
                                            {
                                                adjustedPHDFaculty = SpecializationwisePHDFaculty;
                                                if (adjustedPHDFaculty >= SpecializationwisePHDFaculty)
                                                    PhdminimumRequirementMet = "YES";
                                                else
                                                    PhdminimumRequirementMet = "NO";
                                            }

                                        }
                                        else
                                        {
                                            remainingPHDFaculty = remainingPHDFaculty -
                                                                  item.SpecializationsphdFaculty;
                                            if (remainingPHDFaculty < 0)
                                            {
                                                adjustedPHDFaculty = 0;
                                            }
                                            else
                                            {
                                                adjustedPHDFaculty = item.SpecializationsphdFaculty;
                                            }

                                            PhdminimumRequirementMet = "NO";
                                        }
                                    }
                                    else
                                    {
                                        remainingPHDFaculty = remainingPHDFaculty;
                                        adjustedPHDFaculty = remainingPHDFaculty;

                                        if (remainingPHDFaculty >= SpecializationwisePHDFaculty)
                                        {
                                            PhdminimumRequirementMet = "YES";
                                        }
                                        else
                                        {
                                            PhdminimumRequirementMet = "NO";
                                            remainingPHDFaculty = 0;
                                        }
                                    }
                                }

                            }
                            else if (remainingPHDFaculty >= SpecializationwisePHDFaculty && remainingPHDFaculty >= 2 && (degreeType.Equals("PG")) &&
                                 item.SpecializationsphdFaculty > 0)
                            {
                                if (remainingPHDFaculty >= item.SpecializationsphdFaculty)
                                {
                                    if (item.shiftId == 1)
                                    {
                                        if (item.SpecializationsphdFaculty >= SpecializationwisePHDFaculty)
                                        {
                                            if (remainingPHDFaculty >= item.SpecializationsphdFaculty)
                                            {
                                                remainingPHDFaculty = remainingPHDFaculty - SpecializationwisePHDFaculty;
                                                adjustedPHDFaculty = SpecializationwisePHDFaculty;
                                                if (adjustedFaculty < adjustedPHDFaculty)
                                                {
                                                    adjustedPHDFaculty = adjustedFaculty;
                                                }
                                                if (adjustedPHDFaculty >= SpecializationwisePHDFaculty)
                                                    PhdminimumRequirementMet = "YES";
                                                else
                                                    PhdminimumRequirementMet = "NO";
                                            }
                                            else
                                            {
                                                remainingPHDFaculty = remainingPHDFaculty - item.SpecializationsphdFaculty;
                                                adjustedPHDFaculty = item.SpecializationsphdFaculty;

                                                if (adjustedPHDFaculty >= SpecializationwisePHDFaculty)
                                                    PhdminimumRequirementMet = "YES";
                                                else
                                                    PhdminimumRequirementMet = "NO";
                                            }
                                        }
                                        else
                                        {
                                            remainingPHDFaculty = remainingPHDFaculty - item.SpecializationsphdFaculty;
                                            adjustedPHDFaculty = item.SpecializationsphdFaculty;
                                            PhdminimumRequirementMet = "NO";
                                        }

                                    }
                                    else
                                    {
                                        if (item.shiftId == 2)
                                        {
                                            int firstshiftintake = facultyCounts.Where(e =>
                                             e.Proposedintake != 0 && e.ispercentage == false &&
                                             !ProposedIntakeZeroDeptName.Contains(e.Specialization) &&
                                             e.specializationId == item.specializationId && e.shiftId == 1)
                                             .Select(e => e.Proposedintake)
                                             .FirstOrDefault();
                                            int firstshiftPHDFaculty = IntakeWisePhdForMtech(firstshiftintake, 1);
                                            item.SpecializationsphdFaculty = item.SpecializationsphdFaculty -
                                                                             firstshiftPHDFaculty;
                                            if (item.SpecializationsphdFaculty < 0)
                                            {
                                                item.SpecializationsphdFaculty = 0;
                                            }
                                        }
                                        if (item.SpecializationsphdFaculty >= SpecializationwisePHDFaculty)
                                        {
                                            remainingPHDFaculty = remainingPHDFaculty - SpecializationwisePHDFaculty;
                                            adjustedPHDFaculty = SpecializationwisePHDFaculty;
                                            if (adjustedPHDFaculty >= SpecializationwisePHDFaculty)
                                                PhdminimumRequirementMet = "YES";
                                            else
                                                PhdminimumRequirementMet = "NO";
                                        }
                                        else
                                        {
                                            remainingPHDFaculty = remainingPHDFaculty - item.SpecializationsphdFaculty;
                                            adjustedPHDFaculty = item.SpecializationsphdFaculty;
                                            PhdminimumRequirementMet = "NO";
                                        }

                                    }
                                }
                                else
                                {
                                    if (item.shiftId == 2)
                                    {
                                        int firstshiftintake = facultyCounts.Where(e =>
                                            e.Proposedintake != 0 && e.ispercentage == false &&
                                            !ProposedIntakeZeroDeptName.Contains(e.Specialization) &&
                                            e.specializationId == item.specializationId && e.shiftId == 1)
                                            .Select(e => e.Proposedintake)
                                            .FirstOrDefault();
                                        int firstshiftPHDFaculty = IntakeWisePhdForMtech(firstshiftintake, 1);
                                        item.SpecializationsphdFaculty = item.SpecializationsphdFaculty -
                                                                         firstshiftPHDFaculty;
                                        if (item.SpecializationsphdFaculty < 0)
                                        {
                                            item.SpecializationsphdFaculty = 0;
                                        }
                                        if (item.SpecializationsphdFaculty >= SpecializationwisePHDFaculty)
                                        {
                                            remainingPHDFaculty = remainingPHDFaculty - SpecializationwisePHDFaculty;
                                            adjustedPHDFaculty = SpecializationwisePHDFaculty;
                                            if (adjustedPHDFaculty >= SpecializationwisePHDFaculty)
                                                PhdminimumRequirementMet = "YES";
                                            else
                                                PhdminimumRequirementMet = "NO";
                                        }
                                        else
                                        {
                                            remainingPHDFaculty = remainingPHDFaculty -
                                                                  item.SpecializationsphdFaculty;
                                            adjustedPHDFaculty = item.SpecializationsphdFaculty;
                                            PhdminimumRequirementMet = "NO";
                                        }
                                    }
                                    else
                                    {
                                        if (remainingPHDFaculty == SpecializationwisePHDFaculty)
                                        {
                                            adjustedPHDFaculty = remainingPHDFaculty;
                                            remainingPHDFaculty = remainingPHDFaculty - SpecializationwisePHDFaculty;
                                            if (adjustedPHDFaculty >= SpecializationwisePHDFaculty)
                                                PhdminimumRequirementMet = "YES";
                                            else
                                                PhdminimumRequirementMet = "NO";
                                        }
                                        else
                                        {
                                            remainingPHDFaculty = remainingPHDFaculty;
                                            adjustedPHDFaculty = remainingPHDFaculty;
                                            if (remainingPHDFaculty >= SpecializationwisePHDFaculty)
                                                PhdminimumRequirementMet = "YES";
                                            else
                                                PhdminimumRequirementMet = "NO";
                                        }

                                    }
                                }
                            }
                            else if (remainingPHDFaculty >= SpecializationwisePHDFaculty && remainingPHDFaculty <= 1 && (degreeType.Equals("PG")) &&
                                     item.SpecializationsphdFaculty > 0)
                            {
                                if (item.shiftId == 1)
                                {
                                    if (item.SpecializationsphdFaculty >= SpecializationwisePHDFaculty)
                                    {
                                        if (remainingPHDFaculty >= item.SpecializationsphdFaculty)
                                        {
                                            remainingPHDFaculty = remainingPHDFaculty - SpecializationwisePHDFaculty;
                                            adjustedPHDFaculty = SpecializationwisePHDFaculty;
                                            if (adjustedPHDFaculty >= SpecializationwisePHDFaculty)
                                                PhdminimumRequirementMet = "YES";
                                            else
                                                PhdminimumRequirementMet = "NO";
                                        }
                                        else
                                        {
                                            remainingPHDFaculty = remainingPHDFaculty - item.SpecializationsphdFaculty;
                                            adjustedPHDFaculty = item.SpecializationsphdFaculty;

                                            if (adjustedPHDFaculty >= SpecializationwisePHDFaculty)
                                                PhdminimumRequirementMet = "YES";
                                            else
                                                PhdminimumRequirementMet = "NO";
                                        }
                                    }
                                    else
                                    {
                                        remainingPHDFaculty = remainingPHDFaculty - item.SpecializationsphdFaculty;
                                        adjustedPHDFaculty = item.SpecializationsphdFaculty;
                                        PhdminimumRequirementMet = "NO";
                                    }

                                }
                                else if (item.shiftId == 2)
                                {
                                    int firstshiftintake = facultyCounts.Where(e =>
                                        e.Proposedintake != 0 && e.ispercentage == false &&
                                        !ProposedIntakeZeroDeptName.Contains(e.Specialization) &&
                                        e.specializationId == item.specializationId && e.shiftId == 1)
                                        .Select(e => e.Proposedintake)
                                        .FirstOrDefault();
                                    int firstshiftPHDFaculty = IntakeWisePhdForMtech(firstshiftintake, 1);
                                    item.SpecializationsphdFaculty = item.SpecializationsphdFaculty -
                                                                     firstshiftPHDFaculty;
                                    if (item.SpecializationsphdFaculty < 0)
                                    {
                                        item.SpecializationsphdFaculty = 0;
                                    }
                                    if (item.SpecializationsphdFaculty >= SpecializationwisePHDFaculty)
                                    {
                                        remainingPHDFaculty = remainingPHDFaculty - SpecializationwisePHDFaculty;
                                        adjustedPHDFaculty = SpecializationwisePHDFaculty;
                                        if (adjustedPHDFaculty >= SpecializationwisePHDFaculty)
                                            PhdminimumRequirementMet = "YES";
                                        else
                                            PhdminimumRequirementMet = "NO";
                                    }
                                    else
                                    {
                                        remainingPHDFaculty = remainingPHDFaculty -
                                                              item.SpecializationsphdFaculty;
                                        adjustedPHDFaculty = item.SpecializationsphdFaculty;
                                        PhdminimumRequirementMet = "NO";
                                    }
                                }
                                else
                                {
                                    adjustedPHDFaculty = remainingPHDFaculty;

                                    remainingPHDFaculty = remainingPHDFaculty - 1;
                                    PhdminimumRequirementMet = "NO";
                                }

                            }
                            else if (SpecializationwisePHDFaculty > 0 && (degreeType.Equals("UG")) &&
                                     remainingPHDFaculty > 0)
                            {
                                if (item.SpecializationsphdFaculty >= SpecializationwisePHDFaculty)
                                {
                                    remainingPHDFaculty = remainingPHDFaculty -
                                                          item.SpecializationsphdFaculty;
                                    adjustedPHDFaculty = item.SpecializationsphdFaculty;
                                    PhdminimumRequirementMet = "YES";
                                }
                                else if (item.SpecializationsphdFaculty <= SpecializationwisePHDFaculty)
                                {
                                    remainingPHDFaculty = remainingPHDFaculty - SpecializationwisePHDFaculty;
                                    adjustedPHDFaculty = SpecializationwisePHDFaculty;
                                    PhdminimumRequirementMet = "YES";
                                }

                            }
                            else if (SpecializationwisePHDFaculty > 0 && (degreeType.Equals("UG")) &&
                                     remainingPHDFaculty <= 0)
                            {
                                // remainingPHDFaculty = SpecializationwisePHDFaculty - remainingPHDFaculty;
                                adjustedPHDFaculty = remainingPHDFaculty;
                                PhdminimumRequirementMet = "NO";
                            }
                            else if (SpecializationwisePHDFaculty == 0 && (degreeType.Equals("UG")))
                            {
                                PhdminimumRequirementMet = "NO";
                            }
                            else if (adjustedPHDFaculty <= 0 && (degreeType.Equals("PG")))
                            {
                                PhdminimumRequirementMet = "YES";
                            }
                            //Dual Degree
                            if (remainingPHDFaculty >= 2 && (degreeType.Equals("Dual Degree")) &&
                                item.SpecializationsphdFaculty > 0)
                            {
                                if (remainingPHDFaculty >= item.SpecializationsphdFaculty)
                                {
                                    remainingPHDFaculty = remainingPHDFaculty - item.SpecializationsphdFaculty;
                                    adjustedPHDFaculty = item.SpecializationsphdFaculty;
                                    if (adjustedPHDFaculty >= SpecializationwisePHDFaculty)
                                        PhdminimumRequirementMet = "YES";
                                    else
                                        PhdminimumRequirementMet = "NO";
                                }
                                else
                                {
                                    remainingPHDFaculty = remainingPHDFaculty;
                                    adjustedPHDFaculty = remainingPHDFaculty;

                                    if (remainingPHDFaculty >= SpecializationwisePHDFaculty)
                                        PhdminimumRequirementMet = "YES";
                                    else
                                        PhdminimumRequirementMet = "NO";
                                }
                            }
                            else if (remainingPHDFaculty <= 1 && (degreeType.Equals("Dual Degree")) &&
                                     item.SpecializationsphdFaculty > 0)
                            {
                                adjustedPHDFaculty = remainingPHDFaculty;

                                remainingPHDFaculty = remainingPHDFaculty - 1;
                                PhdminimumRequirementMet = "NO";
                            }
                            else if (adjustedPHDFaculty <= 0 && (degreeType.Equals("Dual Degree")))
                            {
                                PhdminimumRequirementMet = "YES";
                            }
                            else if (SpecializationwisePHDFaculty == 0 && (degreeType.Equals("Dual Degree")))
                            {
                                PhdminimumRequirementMet = "NO";
                            }
                        }


                        if (minimumRequirementMet == "YES" && PhdminimumRequirementMet == "YES")
                        {


                            if (rFaculty <= adjustedFaculty && SpecializationwisePHDFaculty <= adjustedPHDFaculty)
                            {
                                minimumRequirementMet = "-";
                            }
                            else if (rFaculty > adjustedFaculty && SpecializationwisePHDFaculty <= adjustedPHDFaculty)
                            {
                                minimumRequirementMet = "Deficiency In faculty";
                            }
                            else if (rFaculty <= adjustedFaculty &&
                                     SpecializationwisePHDFaculty > adjustedPHDFaculty)
                            {
                                minimumRequirementMet = "Deficiency In Ph.D";
                            }
                            else
                            {
                                minimumRequirementMet = "Deficiency In faculty and Ph.D";
                            }


                        }
                        else if (minimumRequirementMet == "NO" && PhdminimumRequirementMet == "YES")
                        {


                            if (rFaculty <= adjustedFaculty && SpecializationwisePHDFaculty <= adjustedPHDFaculty)
                            {
                                minimumRequirementMet = "-";
                            }
                            else if (rFaculty > adjustedFaculty && SpecializationwisePHDFaculty <= adjustedPHDFaculty)
                            {
                                minimumRequirementMet = "Deficiency In faculty";
                            }
                            else if (rFaculty <= adjustedFaculty &&
                                     SpecializationwisePHDFaculty > adjustedPHDFaculty)
                            {
                                minimumRequirementMet = "Deficiency In Ph.D";
                            }
                            else
                            {
                                minimumRequirementMet = "Deficiency In faculty and Ph.D";
                            }

                        }
                        else if (minimumRequirementMet == "YES" && PhdminimumRequirementMet == "NO")
                        {

                            if (rFaculty <= adjustedFaculty && SpecializationwisePHDFaculty <= adjustedPHDFaculty)
                            {
                                minimumRequirementMet = "-";
                            }
                            else if (rFaculty > adjustedFaculty && SpecializationwisePHDFaculty <= adjustedPHDFaculty)
                            {
                                minimumRequirementMet = "Deficiency In faculty";
                            }
                            else if (rFaculty <= adjustedFaculty &&
                                     SpecializationwisePHDFaculty > adjustedPHDFaculty)
                            {
                                minimumRequirementMet = "Deficiency In Ph.D";
                            }
                            else
                            {
                                minimumRequirementMet = "Deficiency In faculty and Ph.D";
                            }

                        }

                        else if (minimumRequirementMet == "NO" && PhdminimumRequirementMet == "NO")
                        {

                            if (rFaculty <= adjustedFaculty && SpecializationwisePHDFaculty <= adjustedPHDFaculty)
                            {
                                minimumRequirementMet = "-";
                            }
                            else if (rFaculty > adjustedFaculty && SpecializationwisePHDFaculty <= adjustedPHDFaculty)
                            {
                                minimumRequirementMet = "Deficiency In faculty";
                            }
                            else if (rFaculty <= adjustedFaculty &&
                                     SpecializationwisePHDFaculty > adjustedPHDFaculty)
                            {
                                minimumRequirementMet = "Deficiency In Ph.D";
                            }
                            else
                            {
                                minimumRequirementMet = "Deficiency In faculty and Ph.D";
                            }
                        }


                        if (departments.Contains(item.Department))
                        {

                            // int rowspan = 0;

                            if (item.Department == "English")
                            {
                                Humanities += "<p><b><u>Faculty Requirement for I-Year: </u></b></p><br/>";
                                Humanities += "<table width='100%' border='1' cellpadding='3' cellspacing='0' style='border-color: #ccc;'>";
                                Humanities += "<tr>";
                                Humanities += "<th>S.No</th>";
                                Humanities += "<th>Departments</th>";
                                Humanities += "<th>Total Proposed Intake</th>";
                                Humanities += "<th>Required Faculty</th>";
                                Humanities += "<th>Available Faculty</th>";
                                // Humanities += "<th>Availiable Faculty</th>";
                                Humanities += "<th>Eligible Faculty</th>";
                                //Humanities += "<th>Required Ph.D</th>";
                                //Humanities += "<th>Availiable Ph.D</th>";
                                //Humanities += "<th>Eligible Ph.D</th>";
                                Humanities += "<th>Deficiency</th>";
                                Humanities += "</tr>";
                            }


                            Humanities += "<tr>";
                            Humanities += "<td  style='text-align: center;vertical-align: top;'>" + (Index++) + "</td>";
                            Humanities += "<td  style='text-align: left;vertical-align: top;'>" + item.Department + "</td>";
                            Humanities += "<td  style='text-align: center;vertical-align: top;'>" + totalBtechFirstYearIntake + "</td>";
                            if (deptloop == 1)

                                if (OthersSpecIds.Contains(item.specializationId))
                                {
                                    Humanities += "<td style='text-align: center; vertical-align: top;'>" +
                                               Math.Ceiling(firstYearRequired / 2) + "</td>";
                                    HumantitiesminimamRequireMet += item.totalFaculty;
                                }
                                else
                                {
                                    Humanities += "<td style='text-align: center; vertical-align: top;'>" +
                                               Math.Ceiling(firstYearRequired) + "</td>";
                                    HumantitiesminimamRequireMet += item.totalFaculty;
                                }

                            //
                            if (minimumRequirementMet != "YES")
                            {
                                HumantitiesminimamRequireMetStatus = "NO";
                            }

                            // Humanities += "<td class='col2' style='text-align:center;vertical-align: top;'>" + item.requiredFaculty + "</td>";

                            int DeptWiseAvailiableFaculty = jntuh_college_faculty.Where(e => e.DepartmentId == item.DepartmentID && (e.SpecializationId == item.specializationId || e.SpecializationId == null)).Select(e => e.RegistrationNumber).Count();

                            Humanities += "<td  style='text-align: center; vertical-align: top;'>" +
                                        DeptWiseAvailiableFaculty + "</td>";
                            Humanities += "<td  style='text-align: center; vertical-align: top;'>" +
                                       adjustedFaculty + "</td>";

                            //Humanities += "<td class='col2' rowspan='" + rowspan + "' style='text-align: center; vertical-align: top;'>" +
                            //             SpecializationwisePHDFaculty + "</td>";

                            //  var DeptWiseAvailiablePHDFaculty = jntuh_college_faculty.Where(e => e.DepartmentId == item.DepartmentID && (e.SpecializationId == item.specializationId || e.SpecializationId == null)).Select(e => e.RegistrationNumber).ToArray();
                            //  var FacultyIdsnew = jntuh_registred_faculty.Where(e => DeptWiseAvailiablePHDFaculty.Contains(e.RegistrationNumber)).Select(e => e.id).ToArray();
                            //  var PHDFacuty = jntuh_registered_faculty_education.Where(r => FacultyIdsnew.Contains(r.facultyId) && r.educationId == 6).Select(e => e.facultyId).Distinct().Count();

                            //  //DeptWiseAvailiablePHDFaculty
                            //  Humanities += "<td class='col2' rowspan='" + rowspan + "' style='text-align: center; vertical-align: top;'>" +
                            //            PHDFacuty + "</td>";

                            //  //DeptWiseEligiblePHDFaculty
                            //  Humanities += "<td class='col2' rowspan='" + rowspan + "' style='text-align: center; vertical-align: top;'>" +
                            //             adjustedPHDFaculty + "</td>";
                            string Text = string.Empty;
                            if (minimumRequirementMet == "-")
                            {
                                Text = "NIL";
                                Humanities += "<td style='text-align: center;vertical-align: top;'>" + Text + "</td>";
                            }
                            else
                            {
                                Text = "Yes";
                                Humanities += "<td style='text-align: center;vertical-align: top;'>" + Text + "</td>";
                            }
                            //Humanities += "<td class='col2' rowspan='" + rowspan + "' style='text-align: center; vertical-align: top;'>" +
                            //           minimumRequirementMet + "</td>";


                            Humanities += "</tr>";
                            //var HumanitiesDepartments = facultyCounts.Where(e=>departments.Contains(e.Department)).Select(e=>e).ToList();

                            //foreach (var item in HumanitiesDepartments)
                            //{

                            //}

                            if (item.Department == "Others(MNGT/H&S)")
                            {
                                Humanities += "</table>";
                                Humanities += "<br/>";
                            }



                            #region Flags Not-Cleared Faculty

                            int CountSNo = 1;

                            int DataCount = CollegeInactiveFaculty.Where(e => e.DeptId == item.DepartmentID).Count();
                            if (DataCount == 0)
                            {
                                facultyIndexnotEqualtoZeroAdmittedIntakeIndex = 1;
                                //Humanities1 += "<p><b><u>" + item.Department + " - Inactive Faculty.</u></b></p>";
                                //Humanities1 += "<table width='100%' border='1' cellpadding='3' cellspacing='0' style='border-color: #ccc;'>";
                                //Humanities1 += "<tr>";
                                //Humanities1 += "<th>S.No</th>";
                                //Humanities1 += "<th>Registration Number</th>";
                                //Humanities1 += "<th>Full Name</th>";
                                //Humanities1 += "<th>Reason for Inactive</th>";
                                //Humanities1 += "</tr>";
                                //Humanities1 += "<tr><td colspan='7' style='text-align:center;'>NIL</td></tr>";
                                //Humanities1 += "</table>";
                                //Humanities1 += "<br/>";
                                facultyIndexnotEqualtoZeroAdmittedIntakeIndex = 1;
                            }
                            else
                            {
                                Humanities1 += "<p><b><u>" + item.Department + " - Inactive Faculty :</u></b></p>";
                                Humanities1 += "<table width='100%' border='1' cellpadding='3' cellspacing='0' style='border-color: #ccc;'>";
                                Humanities1 += "<tr>";
                                Humanities1 += "<th>S.No</th>";
                                Humanities1 += "<th>Registration Number</th>";
                                Humanities1 += "<th>Name</th>";
                                Humanities1 += "<th>BAS Joining Date</th>";
                                Humanities1 += "<th>August</th>";
                                Humanities1 += "<th>September</th>";
                                Humanities1 += "<th>October</th>";
                                Humanities1 += "<th>November</th>";
                                Humanities1 += "<th>December</th>";
                                Humanities1 += "<th>January</th>";
                                Humanities1 += "<th>February</th>";
                                Humanities1 += "<th>March</th>";
                                Humanities1 += "<th>April</th>";
                                //Humanities1 += "<th>May</th>";
                                Humanities1 += "<th>Days Present /Total Days</th>";
                                Humanities1 += "<th>Reason for Inactive</th>";
                                Humanities1 += "</tr>";

                                foreach (var NotCleared in CollegeInactiveFaculty.Where(e => e.DeptId == item.DepartmentID && (e.SpecializationId == item.specializationId || e.SpecializationId == null)).ToList())
                                {
                                    GetFacultyBASDetails BASFaculty = new GetFacultyBASDetails();

                                    var Specialization = NotCleared.SpecializationId != null ? jntuh_specialization.Where(e => e.id == NotCleared.SpecializationId).Select(e => e.specializationName).FirstOrDefault() : null;
                                    var EachFaculty = jntuh_college_basreport.Where(e => e.RegistrationNumber == NotCleared.RegistrationNumber&&e.monthId==11).Select(e => e).ToList();

                                    string date = EachFaculty.Select(e => e.joiningDate).LastOrDefault().ToString();
                                    if (EachFaculty.Count == 0)
                                    {

                                    }
                                    else
                                    {
                                        BASFaculty.BasJoiningDate = date == null ? null : Convert.ToDateTime(date).ToString("dd-MM-yyyy");
                                    }

                                    foreach (var BAS in EachFaculty)
                                    {
                                        if (BAS.month == "August")
                                        {
                                            BASFaculty.AugustTotalDays = BAS.totalworkingDays;
                                            BASFaculty.AugustPresentDays = BAS.NoofPresentDays;
                                        }

                                        else if (BAS.month == "September")
                                        {
                                            BASFaculty.SeptemberTotalDays = BAS.totalworkingDays;
                                            BASFaculty.SeptemberPresentDays = BAS.NoofPresentDays;
                                        }

                                        else if (BAS.month == "October")
                                        {
                                            BASFaculty.OctoberTotalDays = BAS.totalworkingDays;
                                            BASFaculty.OctoberPresentDays = BAS.NoofPresentDays;
                                        }

                                        else if (BAS.month == "November")
                                        {
                                            BASFaculty.NovemberTotalDays = BAS.totalworkingDays;
                                            BASFaculty.NovemberPresentDays = BAS.NoofPresentDays;
                                        }

                                        else if (BAS.month == "December")
                                        {
                                            BASFaculty.DecemberTotalDays = BAS.totalworkingDays;
                                            BASFaculty.DecemberPresentDays = BAS.NoofPresentDays;
                                        }

                                        else if (BAS.month == "January")
                                        {
                                            BASFaculty.JanuaryTotalDays = BAS.totalworkingDays;
                                            BASFaculty.JanuaryPresentDays = BAS.NoofPresentDays;
                                        }

                                        else if (BAS.month == "February")
                                        {
                                            BASFaculty.FebruaryTotalDays = BAS.totalworkingDays;
                                            BASFaculty.FebruaryPresentDays = BAS.NoofPresentDays;
                                        }

                                        else if (BAS.month == "March")
                                        {
                                            BASFaculty.MarchTotalDays = BAS.totalworkingDays;
                                            BASFaculty.MarchPresentDays = BAS.NoofPresentDays;
                                        }
                                        else if (BAS.month == "April")
                                        {
                                            BASFaculty.AprilTotalDays = BAS.totalworkingDays;
                                            BASFaculty.AprilPresentDays = BAS.NoofPresentDays;
                                        }
                                    }

                                    Humanities1 += "<tr>";
                                    Humanities1 += "<td>" + (CountSNo++) + "</td>";
                                    Humanities1 += "<td>" + NotCleared.RegistrationNumber + "</td>";
                                    if (NotCleared.DegreeId == 6)
                                        Humanities1 += "<td>" + NotCleared.FullName + "(Ph.D)" + "</td>";
                                    else
                                        Humanities1 += "<td>" + NotCleared.FullName + "</td>";
                                    Humanities1 += "<td>" + BASFaculty.BasJoiningDate + "</td>";

                                    if (BASFaculty.AugustTotalDays == 0 || BASFaculty.AugustTotalDays == null)
                                        Humanities1 += "<td>--</td>";
                                    else
                                        Humanities1 += "<td>" + BASFaculty.AugustPresentDays + "/" + BASFaculty.AugustTotalDays + "</td>";


                                    if (BASFaculty.SeptemberTotalDays == 0 || BASFaculty.SeptemberTotalDays == null)
                                        Humanities1 += "<td>--</td>";
                                    else
                                        Humanities1 += "<td>" + BASFaculty.SeptemberPresentDays + "/" + BASFaculty.SeptemberTotalDays + "</td>";
                                    if (BASFaculty.OctoberTotalDays == 0 || BASFaculty.OctoberTotalDays == null)
                                        Humanities1 += "<td>--</td>";
                                    else
                                        Humanities1 += "<td>" + BASFaculty.OctoberPresentDays + "/" + BASFaculty.OctoberTotalDays + "</td>";
                                    if (BASFaculty.NovemberTotalDays == 0 || BASFaculty.NovemberTotalDays == null)
                                        Humanities1 += "<td>--</td>";
                                    else
                                        Humanities1 += "<td>" + BASFaculty.NovemberPresentDays + "/" + BASFaculty.NovemberTotalDays + "</td>";
                                    if (BASFaculty.DecemberTotalDays == 0 || BASFaculty.DecemberTotalDays == null)
                                        Humanities1 += "<td>--</td>";
                                    else
                                        Humanities1 += "<td>" + BASFaculty.DecemberPresentDays + "/" + BASFaculty.DecemberTotalDays + "</td>";
                                    if (BASFaculty.JanuaryTotalDays == 0 || BASFaculty.JanuaryTotalDays == null)
                                        Humanities1 += "<td>--</td>";
                                    else
                                        Humanities1 += "<td>" + BASFaculty.JanuaryPresentDays + "/" + BASFaculty.JanuaryTotalDays + "</td>";
                                    if (BASFaculty.FebruaryTotalDays == 0 || BASFaculty.FebruaryTotalDays == null)
                                        Humanities1 += "<td>--</td>";
                                    else
                                        Humanities1 += "<td>" + BASFaculty.FebruaryPresentDays + "/" + BASFaculty.FebruaryTotalDays + "</td>";
                                    if (BASFaculty.MarchTotalDays == 0 || BASFaculty.MarchTotalDays == null)
                                        Humanities1 += "<td>--</td>";
                                    else
                                        Humanities1 += "<td>" + BASFaculty.MarchPresentDays + "/" + BASFaculty.MarchTotalDays + "</td>";
                                    if (BASFaculty.AprilTotalDays == 0 || BASFaculty.AprilTotalDays == null)
                                        Humanities1 += "<td>--</td>";
                                    else
                                        Humanities1 += "<td>" + BASFaculty.AprilPresentDays + "/" + BASFaculty.AprilTotalDays + "</td>";

                                    // faculty += "<td>" +  + "</td>";
                                    Humanities1 += "<td>" + EachFaculty.Select(e => e.NoofPresentDays).Sum() + "/" + EachFaculty.Select(e => e.totalworkingDays).Sum() + "</td>";
                                    //faculty += "<td>" + Specialization + "</td>";
                                    //faculty += "<td>" + NotCleared.PanNumber + "</td>";
                                    //faculty += "<td>" + NotCleared.AadhaarNumber + "</td>";
                                    string Reason = null;

                                    if (NotCleared.Absent == true)
                                        Reason += "Absent";

                                    if (NotCleared.type == "Adjunct")
                                    {
                                        if (Reason != null)
                                            Reason += ",Adjunct Faculty";
                                        else
                                            Reason += "Adjunct Faculty";
                                    }

                                    if (NotCleared.OriginalCertificatesnotshownFlag == true)
                                    {
                                        if (Reason != null)
                                            Reason += ",Original Certificates not shown";
                                        else
                                            Reason += "Original Certificates not shown";
                                    }

                                    if (NotCleared.XeroxcopyofcertificatesFlag == true)
                                    {
                                        if (Reason != null)
                                            Reason += ",Xerox copy of certificates";
                                        else
                                            Reason += "Xerox copy of certificates";
                                    }

                                    if (NotCleared.NOTQualifiedAsPerAICTE == true)
                                    {
                                        if (Reason != null)
                                            Reason += ",NOT Qualified As per AICTE/PCI";
                                        else
                                            Reason += "NOT Qualified As per AICTE/PCI";
                                    }

                                    if (NotCleared.NoSCM == true)
                                    {
                                        if (Reason != null)
                                            Reason += ",No SCM";
                                        else
                                            Reason += "No SCM";
                                    }

                                    if (NotCleared.PanNumber == null)
                                    {
                                        if (Reason != null)
                                            Reason += ",No PAN Number";
                                        else
                                            Reason += "No PAN Number";
                                    }

                                    if (NotCleared.InCompleteCeritificates == true)
                                    {
                                        if (Reason != null)
                                            Reason += ",In Complete Certificates";
                                        else
                                            Reason += "In Complete Certificates";
                                    }

                                    if (NotCleared.BlacklistFaculty == true)
                                    {
                                        if (Reason != null)
                                            Reason += ",Blacklisted Faculty";
                                        else
                                            Reason += "Blacklisted Faculty";
                                    }

                                    if (NotCleared.NOrelevantUgFlag == true)
                                    {
                                        if (Reason != null)
                                            Reason += ",NO Relevant UG";
                                        else
                                            Reason += "NO Relevant UG";
                                    }

                                    if (NotCleared.NOrelevantPgFlag == true)
                                    {
                                        if (Reason != null)
                                            Reason += ",NO Relevant PG";
                                        else
                                            Reason += "NO Relevant PG";
                                    }

                                    if (NotCleared.NOrelevantPhdFlag == true)
                                    {
                                        if (Reason != null)
                                            Reason += ",NO Relevant PHD";
                                        else
                                            Reason += "NO Relevant PHD";
                                    }

                                    if (NotCleared.InvalidPANNo == true)
                                    {
                                        if (Reason != null)
                                            Reason += ",Invalid PANNumber";
                                        else
                                            Reason += "Invalid PANNumber";
                                    }

                                    if (NotCleared.OriginalsVerifiedPHD == true)
                                    {
                                        if (Reason != null)
                                            Reason += ",No Guide Sign in PHD Thesis";
                                        else
                                            Reason += "No Guide Sign in PHD Thesis";
                                    }

                                    if (NotCleared.OriginalsVerifiedUG == true)
                                    {
                                        if (Reason != null)
                                            Reason += ",Complaint PHD Faculty";
                                        else
                                            Reason += "Complaint PHD Faculty";
                                    }

                                    if (NotCleared.Basstatus == "Yes")
                                    {
                                        if (Reason != null)
                                            Reason += ",BAS";
                                        else
                                            Reason += "BAS";
                                    }

                                    if (NotCleared.inValidAadhaar == "Yes")
                                    {
                                        if (Reason != null)
                                            Reason += ",No/Invalid Aadhaar Document";
                                        else
                                            Reason += "No/Invalid Aadhaar Document";
                                    }

                                    if (NotCleared.Noclass ==true)
                                    {
                                        if (Reason != null)
                                            Reason += ",No Class in UG/PG";
                                        else
                                            Reason += "No Class in UG/PG";
                                    }

                                    if (NotCleared.AbsentforVerification == true)
                                    {
                                        if (Reason != null)
                                            Reason += ",Absentfor Physical Verification";
                                        else
                                            Reason += "Absentfor Physical Verification";
                                    }

                                    if (NotCleared.NotconsideredPHD == true)
                                    {
                                        if (Reason != null)
                                            Reason += ",Cosidered as a faculty but not cosidered his/her P.hD";
                                        else
                                            Reason += "Cosidered as a faculty but not cosidered his/her P.hD";
                                    }

                                    if (NotCleared.NoPGspecialization == true)
                                    {
                                        if (Reason != null)
                                            Reason += ",no Specialization in PG";
                                        else
                                            Reason += "no Specialization in PG";
                                    }

                                    if (NotCleared.Genuinenessnotsubmitted == true)
                                    {
                                        if (Reason != null)
                                            Reason += ",PHD Genuinity not Submitted";
                                        else
                                            Reason += "PHD Genuinity not Submitted";
                                    }


                                    Humanities1 += "<td>" + Reason + "</td>";
                                    Humanities1 += "</tr>";
                                }
                                Humanities1 += "</table>";
                                Humanities1 += "<br/>";
                                facultyIndexnotEqualtoZeroAdmittedIntakeIndex = 1;
                            }
                            #endregion

                        }
                        else
                        {
                            if (minimumRequirementMet == "-")
                            {

                            }
                            else
                            {
                                int rowspan = 0;
                                if (item.Degree == "B.Tech")
                                    faculty += "<p><b><u>" + item.Degree + "-" + item.Department + ":</u></b></p>";
                                else { 
                                    if(item.shiftId == 2)
                                    {
                                        faculty += "<p><b><u>" + item.Degree + "-" + item.Specialization + "-2:</u></b></p>";
                                    }
                                    else
                                    {
                                        faculty += "<p><b><u>" + item.Degree + "-" + item.Specialization + ":</u></b></p>";
                                    }
                                }
                                    
                                faculty += "<table  width='100%' border='1' cellpadding='3' cellspacing='0' style='border-color: #ccc;'>";
                                faculty += "<tr>";
                                // faculty += "<th  style='text-align: center; vertical-align: top;'>S.No</th>";
                                faculty += "<th  style='text-align: left; vertical-align: top;' >Academic Year</th>";
                                if (item.Degree == "B.Tech")
                                {
                                    rowspan = 3;
                                    faculty += "<th  style='text-align: left; vertical-align: top;' >University Sanctioned Intake</th>";
                                }
                                else
                                {
                                    rowspan = 0;
                                    faculty += "<th  style='text-align: left; vertical-align: top;' >Proposed Intake</th>";
                                }

                                faculty += "<th  style='text-align: left; vertical-align: top;' >Admitted Intake</th>";
                                faculty += "<th  style='text-align: center; vertical-align: top;'>Required <br/>Faculty</th>";
                                faculty += "<th  style='text-align: center; vertical-align: top;'>Available <br/>Faculty</th>";
                                faculty += "<th  style='text-align: center; vertical-align: top;'>Eligible <br/>Faculty</th>";
                                faculty += "<th  style='text-align: center; vertical-align: top;' >Proposed Intake</th>";
                                //faculty += "<th  style='text-align: center; vertical-align: top;' >PG Specialization Faculty</th>";
                                faculty += "<th style='text-align: center; vertical-align: top;'>Required Ph.D. <br/>as per proposed Intake </th>";
                                faculty += "<th style='text-align: center; vertical-align: top;'>Available <br/>Ph.D</th>";
                                faculty += "<th  style='text-align: center; vertical-align: top;'>Eligible <br/>Ph.D</th>";
                                faculty += "<th  style='text-align: center; vertical-align: top;' >Deficiency</th>";
                                faculty += "</tr>";

                                faculty += "<tr>";
                               
                                if (departments.Contains(item.Department))
                                {
                                   
                                }
                                else
                                {
                                    if (item.Degree == "B.Tech")
                                    {

                                       
                                            faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>2018-19</td>";
                                            faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + item.SanctionIntake2 + "</td>";
                                            var AdmittedIntake = intake.Where(e => e.specializationId == item.specializationId && e.academicYearId == AY2&&e.shiftId==item.shiftId).Select(e => e.admittedIntakeasperExambranch_R).FirstOrDefault();
                                            faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + AdmittedIntake + "</td>";
                                      
                                    }
                                    else
                                    {
                                        faculty += "<td class='col2' rowspan='" + rowspan + "' style='text-align: center; vertical-align: top;'>2019-20</td>";
                                        faculty += "<td class='col2' rowspan='" + rowspan + "' style='text-align: center; vertical-align: top;'>" + item.Proposedintake + "</td>";
                                        faculty += "<td class='col2' rowspan='" + rowspan + "' style='text-align: center; vertical-align: top;'>-</td>";
                                    }

                                    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" +
                                               Math.Ceiling(item.requiredFaculty) + "</td>";

                                }
                                facultycount = facultycount + item.specializationWiseFaculty;
                                if (adjustedFaculty > 0)
                                    adjustedFaculty = adjustedFaculty;
                                else
                                    adjustedFaculty = 0;

                                if (adjustedPHDFaculty > 0)
                                    adjustedPHDFaculty = adjustedPHDFaculty;
                                else
                                    adjustedPHDFaculty = 0;

                                int DeptWiseAvailiableFaculty = jntuh_college_faculty.Where(e => e.DepartmentId == item.DepartmentID && (e.SpecializationId == item.specializationId || e.SpecializationId == null)).Select(e => e.RegistrationNumber).Count();

                                faculty += "<td class='col2' rowspan='" + rowspan + "' style='text-align: center; vertical-align: top;'>" +
                                            DeptWiseAvailiableFaculty + "</td>";
                                faculty += "<td class='col2' rowspan='" + rowspan + "' style='text-align: center; vertical-align: top;'>" +
                                           adjustedFaculty + "</td>";

                                if (String.IsNullOrEmpty(item.Note))
                                {
                                    faculty += "<td class='col2' rowspan='" + rowspan + "' style='text-align: center; vertical-align: top;'>" +
                                               item.Proposedintake + "</td>";
                                }
                                else
                                {
                                    faculty += "<td class='col2' rowspan='" + rowspan + "' style='text-align: center; vertical-align: top;'>" +
                                               item.Proposedintake + " " + item.Note + "</td>";
                                }


                                faculty += "<td class='col2' rowspan='" + rowspan + "' style='text-align: center; vertical-align: top;'>" +
                                           SpecializationwisePHDFaculty + "</td>";

                                var DeptWiseAvailiablePHDFaculty = jntuh_college_faculty.Where(e => e.DepartmentId == item.DepartmentID && (e.SpecializationId == item.specializationId || e.SpecializationId == null)).Select(e => e.RegistrationNumber).ToArray();
                                var FacultyIdsnew = jntuh_registred_faculty.Where(e => DeptWiseAvailiablePHDFaculty.Contains(e.RegistrationNumber)).Select(e => e.id).ToArray();
                                var PHDFacuty = jntuh_registered_faculty_education.Where(r => FacultyIdsnew.Contains(r.facultyId) && r.educationId == 6).Select(e => e.facultyId).Distinct().Count();

                                //DeptWiseAvailiablePHDFaculty
                                faculty += "<td class='col2' rowspan='" + rowspan + "' style='text-align: center; vertical-align: top;'>" +
                                          PHDFacuty + "</td>";

                                //DeptWiseEligiblePHDFaculty
                                faculty += "<td class='col2' rowspan='" + rowspan + "' style='text-align: center; vertical-align: top;'>" +
                                           adjustedPHDFaculty + "</td>";
                                faculty += "<td class='col2' rowspan='" + rowspan + "' style='text-align: center; vertical-align: top;'>" +
                                           minimumRequirementMet + "</td>";


                                faculty += "</tr>";

                                if (item.Degree == "B.Tech")
                                {
                                    faculty += "<tr>";
                                    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>2017-18</td>";
                                    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + item.SanctionIntake3 + "</td>";
                                    var AdmittedIntake2 = intake.Where(e => e.specializationId == item.specializationId && e.academicYearId == AY3&&e.shiftId==item.shiftId).Select(e => e.admittedIntakeasperExambranch_R).FirstOrDefault();
                                    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + AdmittedIntake2 + "</td>";
                                    faculty += "</tr>";

                                    faculty += "<tr>";
                                    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>2016-17</td>";
                                    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + item.SanctionIntake4 + "</td>";
                                    var AdmittedIntake4 = intake.Where(e => e.specializationId == item.specializationId && e.academicYearId == AY4 && e.shiftId == item.shiftId).Select(e => e.admittedIntakeasperExambranch_R).FirstOrDefault();
                                    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + AdmittedIntake4 + "</td>";
                                    faculty += "</tr>";

                                  
                                }
                                else
                                {
                                    //faculty += "<td class='col2' style='text-align: left; vertical-align: top;'>2017-18</td>";
                                    //faculty += "<td class='col2' style='text-align: left; vertical-align: top;'>" + item.approvedIntake2 + "</td>";
                                    //faculty += "<td class='col2' style='text-align: left; vertical-align: top;'></td>";
                                }

                                faculty += "</table>";
                                faculty += "<br/>";

                              

                                var SecondShiftSpecIds = facultyCountsmetechsecond.Select(e => e.specializationId).ToArray();
                                if (SecondShiftSpecIds.Contains(item.specializationId))
                                {
                                   if(item.shiftId == 2)
                                   {
                                       #region Flags Not-Cleared Faculty
                                       int CountSNo = 1;

                                       int DataCount = CollegeInactiveFaculty.Where(e => e.DeptId == item.DepartmentID).Count();



                                       if (DataCount == 0)
                                       {
                                           facultyIndexnotEqualtoZeroAdmittedIntakeIndex = 1;
                                       }
                                       else
                                       {
                                           if (item.Degree == "B.Tech")
                                               faculty += "<p><b><u>" + item.Degree + "-" + item.Department + " Inactive Faculty :</u></b></p>";
                                           else
                                           {
                                               if (item.shiftId == 2)
                                               {
                                                   faculty += "<p><b><u>" + item.Degree + "-" + item.Specialization + "-2 Inactive Faculty :</u></b></p>";
                                               }
                                               else
                                               {
                                                   faculty += "<p><b><u>" + item.Degree + "-" + item.Specialization + " Inactive Faculty :</u></b></p>";
                                               }
                                           }

                                           faculty += "<table width='100%' border='1' cellpadding='3' cellspacing='0' style='border-color: #ccc;'>";
                                           faculty += "<tr>";
                                           faculty += "<th>S.No</th>";
                                           faculty += "<th>Registration Number</th>";
                                           faculty += "<th>Name</th>";
                                           faculty += "<th>BAS Joining Date</th>";
                                           faculty += "<th>August</th>";
                                           faculty += "<th>September</th>";
                                           faculty += "<th>October</th>";
                                           faculty += "<th>November</th>";
                                           faculty += "<th>December</th>";
                                           faculty += "<th>January</th>";
                                           faculty += "<th>February</th>";
                                           faculty += "<th>March</th>";
                                           faculty += "<th>April</th>";                                          
                                           faculty += "<th>Days Present /Total Days</th>";
                                           faculty += "<th>Reason for Inactive</th>";
                                           faculty += "</tr>";

                                           foreach (var NotCleared in CollegeInactiveFaculty.Where(e => e.DeptId == item.DepartmentID && (e.SpecializationId == item.specializationId || e.SpecializationId == null)).ToList())
                                           {
                                               GetFacultyBASDetails BASFaculty = new GetFacultyBASDetails();

                                               var Specialization = NotCleared.SpecializationId != null ? jntuh_specialization.Where(e => e.id == NotCleared.SpecializationId).Select(e => e.specializationName).FirstOrDefault() : null;
                                               var EachFaculty = jntuh_college_basreport.Where(e => e.RegistrationNumber == NotCleared.RegistrationNumber&&e.monthId==11).Select(e => e).ToList();

                                               string date = EachFaculty.Select(e => e.joiningDate).LastOrDefault().ToString();
                                               if (EachFaculty.Count == 0)
                                               {

                                               }
                                               else
                                               {
                                                   BASFaculty.BasJoiningDate = date == null ? null : Convert.ToDateTime(date).ToString("dd-MM-yyyy");
                                               }

                                               foreach (var BAS in EachFaculty)
                                               {
                                                   if (BAS.month == "August")
                                                   {
                                                       BASFaculty.AugustTotalDays = BAS.totalworkingDays;
                                                       BASFaculty.AugustPresentDays = BAS.NoofPresentDays;
                                                   }

                                                   else if (BAS.month == "September")
                                                   {
                                                       BASFaculty.SeptemberTotalDays = BAS.totalworkingDays;
                                                       BASFaculty.SeptemberPresentDays = BAS.NoofPresentDays;
                                                   }

                                                   else if (BAS.month == "October")
                                                   {
                                                       BASFaculty.OctoberTotalDays = BAS.totalworkingDays;
                                                       BASFaculty.OctoberPresentDays = BAS.NoofPresentDays;
                                                   }

                                                   else if (BAS.month == "November")
                                                   {
                                                       BASFaculty.NovemberTotalDays = BAS.totalworkingDays;
                                                       BASFaculty.NovemberPresentDays = BAS.NoofPresentDays;
                                                   }

                                                   else if (BAS.month == "December")
                                                   {
                                                       BASFaculty.DecemberTotalDays = BAS.totalworkingDays;
                                                       BASFaculty.DecemberPresentDays = BAS.NoofPresentDays;
                                                   }

                                                   else if (BAS.month == "January")
                                                   {
                                                       BASFaculty.JanuaryTotalDays = BAS.totalworkingDays;
                                                       BASFaculty.JanuaryPresentDays = BAS.NoofPresentDays;
                                                   }

                                                   else if (BAS.month == "February")
                                                   {
                                                       BASFaculty.FebruaryTotalDays = BAS.totalworkingDays;
                                                       BASFaculty.FebruaryPresentDays = BAS.NoofPresentDays;
                                                   }

                                                   else if (BAS.month == "March")
                                                   {
                                                       BASFaculty.MarchTotalDays = BAS.totalworkingDays;
                                                       BASFaculty.MarchPresentDays = BAS.NoofPresentDays;
                                                   }
                                                   else if (BAS.month == "April")
                                                   {
                                                       BASFaculty.AprilTotalDays = BAS.totalworkingDays;
                                                       BASFaculty.AprilPresentDays = BAS.NoofPresentDays;
                                                   }
                                                   
                                               }

                                               faculty += "<tr>";
                                               faculty += "<td>" + (CountSNo++) + "</td>";
                                               faculty += "<td>" + NotCleared.RegistrationNumber + "</td>";
                                               if (NotCleared.DegreeId == 6)
                                                   faculty += "<td>" + NotCleared.FullName + "(Ph.D)" + "</td>";
                                               else
                                                   faculty += "<td>" + NotCleared.FullName + "</td>";
                                               faculty += "<td>" + BASFaculty.BasJoiningDate + "</td>";

                                               if (BASFaculty.AugustTotalDays == 0 || BASFaculty.AugustTotalDays == null)
                                                   faculty += "<td>--</td>";
                                               else
                                                   faculty += "<td>" + BASFaculty.AugustPresentDays + "/" + BASFaculty.AugustTotalDays + "</td>";


                                               if (BASFaculty.SeptemberTotalDays == 0 || BASFaculty.SeptemberTotalDays == null)
                                                   faculty += "<td>--</td>";
                                               else
                                                   faculty += "<td>" + BASFaculty.SeptemberPresentDays + "/" + BASFaculty.SeptemberTotalDays + "</td>";
                                               if (BASFaculty.OctoberTotalDays == 0 || BASFaculty.OctoberTotalDays == null)
                                                   faculty += "<td>--</td>";
                                               else
                                                   faculty += "<td>" + BASFaculty.OctoberPresentDays + "/" + BASFaculty.OctoberTotalDays + "</td>";
                                               if (BASFaculty.NovemberTotalDays == 0 || BASFaculty.NovemberTotalDays == null)
                                                   faculty += "<td>--</td>";
                                               else
                                                   faculty += "<td>" + BASFaculty.NovemberPresentDays + "/" + BASFaculty.NovemberTotalDays + "</td>";
                                               if (BASFaculty.DecemberTotalDays == 0 || BASFaculty.DecemberTotalDays == null)
                                                   faculty += "<td>--</td>";
                                               else
                                                   faculty += "<td>" + BASFaculty.DecemberPresentDays + "/" + BASFaculty.DecemberTotalDays + "</td>";
                                               if (BASFaculty.JanuaryTotalDays == 0 || BASFaculty.JanuaryTotalDays == null)
                                                   faculty += "<td>--</td>";
                                               else
                                                   faculty += "<td>" + BASFaculty.JanuaryPresentDays + "/" + BASFaculty.JanuaryTotalDays + "</td>";
                                               if (BASFaculty.FebruaryTotalDays == 0 || BASFaculty.FebruaryTotalDays == null)
                                                   faculty += "<td>--</td>";
                                               else
                                                   faculty += "<td>" + BASFaculty.FebruaryPresentDays + "/" + BASFaculty.FebruaryTotalDays + "</td>";
                                               if (BASFaculty.MarchTotalDays == 0 || BASFaculty.MarchTotalDays == null)
                                                   faculty += "<td>--</td>";
                                               else
                                                   faculty += "<td>" + BASFaculty.MarchPresentDays + "/" + BASFaculty.MarchTotalDays + "</td>";
                                               if (BASFaculty.AprilTotalDays == 0 || BASFaculty.AprilTotalDays == null)
                                                   faculty += "<td>--</td>";
                                               else
                                                   faculty += "<td>" + BASFaculty.AprilPresentDays + "/" + BASFaculty.AprilTotalDays + "</td>";

                                               // faculty += "<td>" +  + "</td>";
                                               faculty += "<td>" + EachFaculty.Select(e => e.NoofPresentDays).Sum() + "/" + EachFaculty.Select(e => e.totalworkingDays).Sum() + "</td>";
                                               //faculty += "<td>" + Specialization + "</td>";
                                               //faculty += "<td>" + NotCleared.PanNumber + "</td>";
                                               //faculty += "<td>" + NotCleared.AadhaarNumber + "</td>";
                                               string Reason = null;

                                               if (NotCleared.Absent == true)
                                                   Reason += "Absent";

                                               if (NotCleared.type == "Adjunct")
                                               {
                                                   if (Reason != null)
                                                       Reason += ",Adjunct Faculty";
                                                   else
                                                       Reason += "Adjunct Faculty";
                                               }

                                               if (NotCleared.OriginalCertificatesnotshownFlag == true)
                                               {
                                                   if (Reason != null)
                                                       Reason += ",Original Certificates not shown";
                                                   else
                                                       Reason += "Original Certificates not shown";
                                               }

                                               if (NotCleared.XeroxcopyofcertificatesFlag == true)
                                               {
                                                   if (Reason != null)
                                                       Reason += ",Xerox copy of certificates";
                                                   else
                                                       Reason += "Xerox copy of certificates";
                                               }

                                               if (NotCleared.NOTQualifiedAsPerAICTE == true)
                                               {
                                                   if (Reason != null)
                                                       Reason += ",NOT Qualified As per AICTE/PCI";
                                                   else
                                                       Reason += "NOT Qualified As per AICTE/PCI";
                                               }

                                               if (NotCleared.NoSCM == true)
                                               {
                                                   if (Reason != null)
                                                       Reason += ",No SCM";
                                                   else
                                                       Reason += "No SCM";
                                               }

                                               if (NotCleared.PanNumber == null)
                                               {
                                                   if (Reason != null)
                                                       Reason += ",No PAN Number";
                                                   else
                                                       Reason += "No PAN Number";
                                               }

                                               if (NotCleared.InCompleteCeritificates == true)
                                               {
                                                   if (Reason != null)
                                                       Reason += ",In Complete Certificates";
                                                   else
                                                       Reason += "In Complete Certificates";
                                               }

                                               if (NotCleared.BlacklistFaculty == true)
                                               {
                                                   if (Reason != null)
                                                       Reason += ",Blacklisted Faculty";
                                                   else
                                                       Reason += "Blacklisted Faculty";
                                               }

                                               if (NotCleared.NOrelevantUgFlag == true)
                                               {
                                                   if (Reason != null)
                                                       Reason += ",NO Relevant UG";
                                                   else
                                                       Reason += "NO Relevant UG";
                                               }

                                               if (NotCleared.NOrelevantPgFlag == true)
                                               {
                                                   if (Reason != null)
                                                       Reason += ",NO Relevant PG";
                                                   else
                                                       Reason += "NO Relevant PG";
                                               }

                                               if (NotCleared.NOrelevantPhdFlag == true)
                                               {
                                                   if (Reason != null)
                                                       Reason += ",NO Relevant PHD";
                                                   else
                                                       Reason += "NO Relevant PHD";
                                               }

                                               if (NotCleared.InvalidPANNo == true)
                                               {
                                                   if (Reason != null)
                                                       Reason += ",Invalid PANNumber";
                                                   else
                                                       Reason += "Invalid PANNumber";
                                               }

                                               if (NotCleared.OriginalsVerifiedPHD == true)
                                               {
                                                   if (Reason != null)
                                                       Reason += ",No Guide Sign in PHD Thesis";
                                                   else
                                                       Reason += "No Guide Sign in PHD Thesis";
                                               }

                                               if (NotCleared.OriginalsVerifiedUG == true)
                                               {
                                                   if (Reason != null)
                                                       Reason += ",Complaint PHD Faculty";
                                                   else
                                                       Reason += "Complaint PHD Faculty";
                                               }

                                               if (NotCleared.Basstatus == "Yes")
                                               {
                                                   if (Reason != null)
                                                       Reason += ",BAS";
                                                   else
                                                       Reason += "BAS";
                                               }

                                               if (NotCleared.inValidAadhaar == "Yes")
                                               {
                                                   if (Reason != null)
                                                       Reason += ",No/Invalid Aadhaar Document";
                                                   else
                                                       Reason += "No/Invalid Aadhaar Document";
                                               }

                                               if (NotCleared.Noclass == true)
                                               {
                                                   if (Reason != null)
                                                       Reason += ",No Class in UG/PG";
                                                   else
                                                       Reason += "No Class in UG/PG";
                                               }

                                               if (NotCleared.AbsentforVerification == true)
                                               {
                                                   if (Reason != null)
                                                       Reason += ",Absentfor Physical Verification";
                                                   else
                                                       Reason += "Absentfor Physical Verification";
                                               }

                                               if (NotCleared.NotconsideredPHD == true)
                                               {
                                                   if (Reason != null)
                                                       Reason += ",Cosidered as a faculty but not cosidered his/her P.hD";
                                                   else
                                                       Reason += "Cosidered as a faculty but not cosidered his/her P.hD";
                                               }

                                               if (NotCleared.NoPGspecialization == true)
                                               {
                                                   if (Reason != null)
                                                       Reason += ",no Specialization in PG";
                                                   else
                                                       Reason += "no Specialization in PG";
                                               }

                                               if (NotCleared.Genuinenessnotsubmitted == true)
                                               {
                                                   if (Reason != null)
                                                       Reason += ",PHD Genuinity not Submitted";
                                                   else
                                                       Reason += "PHD Genuinity not Submitted";
                                               }

                                               faculty += "<td>" + Reason + "</td>";


                                               faculty += "</tr>";
                                           }
                                           faculty += "</table>";
                                           faculty += "<br/>";
                                           facultyIndexnotEqualtoZeroAdmittedIntakeIndex = 1;
                                       }


                                       #endregion
                                   }
                                  
                                }
                                else
                                {
                                    #region Flags Not-Cleared Faculty
                                    int CountSNo = 1;

                                    int DataCount = CollegeInactiveFaculty.Where(e => e.DeptId == item.DepartmentID).Count();



                                    if (DataCount == 0)
                                    {
                                        facultyIndexnotEqualtoZeroAdmittedIntakeIndex = 1;
                                    }
                                    else
                                    {
                                        if (item.Degree == "B.Tech")
                                            faculty += "<p><b><u>" + item.Degree + "-" + item.Department + " Inactive Faculty :</u></b></p>";
                                        else
                                        {
                                            if (item.shiftId == 2)
                                            {
                                                faculty += "<p><b><u>" + item.Degree + "-" + item.Specialization + "-2 Inactive Faculty :</u></b></p>";
                                            }
                                            else
                                            {
                                                faculty += "<p><b><u>" + item.Degree + "-" + item.Specialization + " Inactive Faculty :</u></b></p>";
                                            }
                                        }

                                        faculty += "<table width='100%' border='1' cellpadding='3' cellspacing='0' style='border-color: #ccc;'>";
                                        faculty += "<tr>";
                                        faculty += "<th>S.No</th>";
                                        faculty += "<th>Registration Number</th>";
                                        faculty += "<th>Name</th>";
                                        faculty += "<th>BAS Joining Date</th>";
                                        faculty += "<th>August</th>";
                                        faculty += "<th>September</th>";
                                        faculty += "<th>October</th>";
                                        faculty += "<th>November</th>";
                                        faculty += "<th>December</th>";
                                        faculty += "<th>January</th>";
                                        faculty += "<th>February</th>";
                                        faculty += "<th>March</th>";
                                        faculty += "<th>April</th>";
                                        faculty += "<th>Days Present /Total Days</th>";
                                        faculty += "<th>Reason for Inactive</th>";
                                        faculty += "</tr>";

                                        foreach (var NotCleared in CollegeInactiveFaculty.Where(e => e.DeptId == item.DepartmentID && (e.SpecializationId == item.specializationId || e.SpecializationId == null)).ToList())
                                        {
                                            GetFacultyBASDetails BASFaculty = new GetFacultyBASDetails();

                                            var Specialization = NotCleared.SpecializationId != null ? jntuh_specialization.Where(e => e.id == NotCleared.SpecializationId).Select(e => e.specializationName).FirstOrDefault() : null;
                                            var EachFaculty = jntuh_college_basreport.Where(e => e.RegistrationNumber == NotCleared.RegistrationNumber&&e.monthId==11).Select(e => e).ToList();

                                            string date = EachFaculty.Select(e => e.joiningDate).LastOrDefault().ToString();
                                            if (EachFaculty.Count == 0)
                                            {

                                            }
                                            else
                                            {
                                                BASFaculty.BasJoiningDate = date == null ? null : Convert.ToDateTime(date).ToString("dd-MM-yyyy");
                                            }

                                            foreach (var BAS in EachFaculty)
                                            {
                                                if (BAS.month == "August")
                                                {
                                                    BASFaculty.AugustTotalDays = BAS.totalworkingDays;
                                                    BASFaculty.AugustPresentDays = BAS.NoofPresentDays;
                                                }

                                                else if (BAS.month == "September")
                                                {
                                                    BASFaculty.SeptemberTotalDays = BAS.totalworkingDays;
                                                    BASFaculty.SeptemberPresentDays = BAS.NoofPresentDays;
                                                }

                                                else if (BAS.month == "October")
                                                {
                                                    BASFaculty.OctoberTotalDays = BAS.totalworkingDays;
                                                    BASFaculty.OctoberPresentDays = BAS.NoofPresentDays;
                                                }

                                                else if (BAS.month == "November")
                                                {
                                                    BASFaculty.NovemberTotalDays = BAS.totalworkingDays;
                                                    BASFaculty.NovemberPresentDays = BAS.NoofPresentDays;
                                                }

                                                else if (BAS.month == "December")
                                                {
                                                    BASFaculty.DecemberTotalDays = BAS.totalworkingDays;
                                                    BASFaculty.DecemberPresentDays = BAS.NoofPresentDays;
                                                }

                                                else if (BAS.month == "January")
                                                {
                                                    BASFaculty.JanuaryTotalDays = BAS.totalworkingDays;
                                                    BASFaculty.JanuaryPresentDays = BAS.NoofPresentDays;
                                                }

                                                else if (BAS.month == "February")
                                                {
                                                    BASFaculty.FebruaryTotalDays = BAS.totalworkingDays;
                                                    BASFaculty.FebruaryPresentDays = BAS.NoofPresentDays;
                                                }

                                                else if (BAS.month == "March")
                                                {
                                                    BASFaculty.MarchTotalDays = BAS.totalworkingDays;
                                                    BASFaculty.MarchPresentDays = BAS.NoofPresentDays;
                                                }
                                                else if (BAS.month == "April")
                                                {
                                                    BASFaculty.AprilTotalDays = BAS.totalworkingDays;
                                                    BASFaculty.AprilPresentDays = BAS.NoofPresentDays;
                                                }
                                            }

                                            faculty += "<tr>";
                                            faculty += "<td>" + (CountSNo++) + "</td>";
                                            faculty += "<td>" + NotCleared.RegistrationNumber + "</td>";
                                            if (NotCleared.DegreeId == 6)
                                                faculty += "<td>" + NotCleared.FullName + "(Ph.D)" + "</td>";
                                            else
                                                faculty += "<td>" + NotCleared.FullName + "</td>";
                                            faculty += "<td>" + BASFaculty.BasJoiningDate + "</td>";
                                           

                                            if (BASFaculty.AugustTotalDays == 0 || BASFaculty.AugustTotalDays == null)
                                                faculty += "<td>--</td>";
                                            else
                                                faculty += "<td>" + BASFaculty.AugustPresentDays + "/" + BASFaculty.AugustTotalDays + "</td>";


                                            if (BASFaculty.SeptemberTotalDays == 0 || BASFaculty.SeptemberTotalDays == null)
                                                faculty += "<td>--</td>";
                                            else
                                                faculty += "<td>" + BASFaculty.SeptemberPresentDays + "/" + BASFaculty.SeptemberTotalDays + "</td>";
                                            if (BASFaculty.OctoberTotalDays == 0 || BASFaculty.OctoberTotalDays == null)
                                                faculty += "<td>--</td>";
                                            else
                                                faculty += "<td>" + BASFaculty.OctoberPresentDays + "/" + BASFaculty.OctoberTotalDays + "</td>";
                                            if (BASFaculty.NovemberTotalDays == 0 || BASFaculty.NovemberTotalDays == null)
                                                faculty += "<td>--</td>";
                                            else
                                                faculty += "<td>" + BASFaculty.NovemberPresentDays + "/" + BASFaculty.NovemberTotalDays + "</td>";
                                            if (BASFaculty.DecemberTotalDays == 0 || BASFaculty.DecemberTotalDays == null)
                                                faculty += "<td>--</td>";
                                            else
                                                faculty += "<td>" + BASFaculty.DecemberPresentDays + "/" + BASFaculty.DecemberTotalDays + "</td>";
                                            if (BASFaculty.JanuaryTotalDays == 0 || BASFaculty.JanuaryTotalDays == null)
                                                faculty += "<td>--</td>";
                                            else
                                                faculty += "<td>" + BASFaculty.JanuaryPresentDays + "/" + BASFaculty.JanuaryTotalDays + "</td>";
                                            if (BASFaculty.FebruaryTotalDays == 0 || BASFaculty.FebruaryTotalDays == null)
                                                faculty += "<td>--</td>";
                                            else
                                                faculty += "<td>" + BASFaculty.FebruaryPresentDays + "/" + BASFaculty.FebruaryTotalDays + "</td>";
                                            if (BASFaculty.MarchTotalDays == 0 || BASFaculty.MarchTotalDays == null)
                                                faculty += "<td>--</td>";
                                            else
                                                faculty += "<td>" + BASFaculty.MarchPresentDays + "/" + BASFaculty.MarchTotalDays + "</td>";
                                            if (BASFaculty.AprilTotalDays == 0 || BASFaculty.AprilTotalDays == null)
                                                faculty += "<td>--</td>";
                                            else
                                                faculty += "<td>" + BASFaculty.AprilPresentDays + "/" + BASFaculty.AprilTotalDays + "</td>";
                                            
                                            // faculty += "<td>" +  + "</td>";
                                            faculty += "<td>" + EachFaculty.Select(e => e.NoofPresentDays).Sum() + "/" + EachFaculty.Select(e => e.totalworkingDays).Sum() + "</td>";
                                            //faculty += "<td>" + Specialization + "</td>";
                                            //faculty += "<td>" + NotCleared.PanNumber + "</td>";
                                            //faculty += "<td>" + NotCleared.AadhaarNumber + "</td>";
                                            string Reason = null;

                                            if (NotCleared.Absent == true)
                                                Reason += "Absent";

                                            if (NotCleared.type == "Adjunct")
                                            {
                                                if (Reason != null)
                                                    Reason += ",Adjunct Faculty";
                                                else
                                                    Reason += "Adjunct Faculty";
                                            }

                                            if (NotCleared.OriginalCertificatesnotshownFlag == true)
                                            {
                                                if (Reason != null)
                                                    Reason += ",Original Certificates not shown";
                                                else
                                                    Reason += "Original Certificates not shown";
                                            }

                                            if (NotCleared.XeroxcopyofcertificatesFlag == true)
                                            {
                                                if (Reason != null)
                                                    Reason += ",Xerox copy of certificates";
                                                else
                                                    Reason += "Xerox copy of certificates";
                                            }

                                            if (NotCleared.NOTQualifiedAsPerAICTE == true)
                                            {
                                                if (Reason != null)
                                                    Reason += ",NOT Qualified As per AICTE/PCI";
                                                else
                                                    Reason += "NOT Qualified As per AICTE/PCI";
                                            }

                                            if (NotCleared.NoSCM == true)
                                            {
                                                if (Reason != null)
                                                    Reason += ",No SCM";
                                                else
                                                    Reason += "No SCM";
                                            }

                                            if (NotCleared.PanNumber == null)
                                            {
                                                if (Reason != null)
                                                    Reason += ",No PAN Number";
                                                else
                                                    Reason += "No PAN Number";
                                            }

                                            if (NotCleared.InCompleteCeritificates == true)
                                            {
                                                if (Reason != null)
                                                    Reason += ",In Complete Certificates";
                                                else
                                                    Reason += "In Complete Certificates";
                                            }

                                            if (NotCleared.BlacklistFaculty == true)
                                            {
                                                if (Reason != null)
                                                    Reason += ",Blacklisted Faculty";
                                                else
                                                    Reason += "Blacklisted Faculty";
                                            }

                                            if (NotCleared.NOrelevantUgFlag == true)
                                            {
                                                if (Reason != null)
                                                    Reason += ",NO Relevant UG";
                                                else
                                                    Reason += "NO Relevant UG";
                                            }

                                            if (NotCleared.NOrelevantPgFlag == true)
                                            {
                                                if (Reason != null)
                                                    Reason += ",NO Relevant PG";
                                                else
                                                    Reason += "NO Relevant PG";
                                            }

                                            if (NotCleared.NOrelevantPhdFlag == true)
                                            {
                                                if (Reason != null)
                                                    Reason += ",NO Relevant PHD";
                                                else
                                                    Reason += "NO Relevant PHD";
                                            }

                                            if (NotCleared.InvalidPANNo == true)
                                            {
                                                if (Reason != null)
                                                    Reason += ",Invalid PANNumber";
                                                else
                                                    Reason += "Invalid PANNumber";
                                            }

                                            if (NotCleared.OriginalsVerifiedPHD == true)
                                            {
                                                if (Reason != null)
                                                    Reason += ",No Guide Sign in PHD Thesis";
                                                else
                                                    Reason += "No Guide Sign in PHD Thesis";
                                            }

                                            if (NotCleared.OriginalsVerifiedUG == true)
                                            {
                                                if (Reason != null)
                                                    Reason += ",Complaint PHD Faculty";
                                                else
                                                    Reason += "Complaint PHD Faculty";
                                            }

                                            if (NotCleared.Basstatus == "Yes")
                                            {
                                                if (Reason != null)
                                                    Reason += ",BAS";
                                                else
                                                    Reason += "BAS";
                                            }

                                            if (NotCleared.inValidAadhaar == "Yes")
                                            {
                                                if (Reason != null)
                                                    Reason += ",No/Invalid Aadhaar Document";
                                                else
                                                    Reason += "No/Invalid Aadhaar Document";
                                            }

                                            if (NotCleared.Noclass == true)
                                            {
                                                if (Reason != null)
                                                    Reason += ",No Class in UG/PG";
                                                else
                                                    Reason += "No Class in UG/PG";
                                            }

                                            if (NotCleared.AbsentforVerification == true)
                                            {
                                                if (Reason != null)
                                                    Reason += ",Absentfor Physical Verification";
                                                else
                                                    Reason += "Absentfor Physical Verification";
                                            }

                                            if (NotCleared.NotconsideredPHD == true)
                                            {
                                                if (Reason != null)
                                                    Reason += ",Cosidered as a faculty but not cosidered his/her P.hD";
                                                else
                                                    Reason += "Cosidered as a faculty but not cosidered his/her P.hD";
                                            }

                                            if (NotCleared.NoPGspecialization == true)
                                            {
                                                if (Reason != null)
                                                    Reason += ",no Specialization in PG";
                                                else
                                                    Reason += "no Specialization in PG";
                                            }

                                            if (NotCleared.Genuinenessnotsubmitted == true)
                                            {
                                                if (Reason != null)
                                                    Reason += ",PHD Genuinity not Submitted";
                                                else
                                                    Reason += "PHD Genuinity not Submitted";
                                            }

                                            faculty += "<td>" + Reason + "</td>";


                                            faculty += "</tr>";
                                        }
                                        faculty += "</table>";
                                        faculty += "<br/>";
                                        facultyIndexnotEqualtoZeroAdmittedIntakeIndex = 1;
                                    }

                                    #endregion
                                }
                          
                            }
                        }


                        deptloop++;
                        SpecializationwisePHDFaculty = 0;
                        // facultyIndexnotEqualtoZeroIndex++;

                        #endregion
                    }

                    faculty += Humanities;
                    faculty += Humanities1;
                    #endregion



                    facultyAdmittedIntakeZeroTable2 += "</table>";
                    facultyAdmittedIntakeZero += "</table>";

                    facultyAdmittedIntakeZero += "<p>Under these circumstances the SCA recommends for <b><u>'No Admission status'</u></b> for the A.Y. 2017-18 for the following programs/courses proposed by the Institute. Clarification or explanation if any may be submitted within 10 days from the date of this letter.<br/><b> Programs / Courses for which <b><u>'No Admission status'</u></b>  recommended:</b></p>";

                    facultyAdmittedIntakeZero += facultyAdmittedIntakeZeroTable2;
                    if (admittedIntakeTwoormoreZeroDeptName1.Count() != 0 && facultyIndexnotEqualtoZeroAdmittedIntakeIndex > 1)
                    {
                        //  faculty += facultyAdmittedIntakeZero;
                    }



                    if (collegeStatus != null)
                    {
                        if (collegeStatus.SIStatus == true)
                        {
                            faculty += "<p><b>Note :In so far as P.G. programs are concerned the University has issued a separate show cause notice Dt. 15.05.2017 based on Surprise Inspection Committee Reports.</b></p>";
                            //The University has conducted surprise inspection to the Institute as a part of academic audit. As per Surprise Inspection Committee report, it is observed that no academic activity is taking place at the Institute for Post Graduate (P.G.) programs. It indicates that you have no inclination for maintaining the minimum essential requirements for running the P.G. programs.</b></p><p><b> Under the circumstances and as per clause 12.12 and point 9 of 3.27 of University affiliation regulations, SCA has recommended for suspension of affiliation to P.G. programs and <br/><b><u>'No Admission status'</u></b> for all P.G. programs of your Institute for the A.Y. 2017-18
                        }
                    }


                    #region Faculty with Proposed Intake Zero and Closure



                    facultyclosure += "<p>The Courses for which college has applied for Closure or Proposed Zero Intake for A.Y. 2018-19 are as follows.</p>";
                    facultyclosure += "<table width='100%' border='1' cellpadding='3' cellspacing='0' style='border-color: #ccc;'>";
                    facultyclosure += "<tr>";
                    facultyclosure += "<th style='text-align: center; vertical-align: top;'>SNo</th>";
                    facultyclosure += "<th style='text-align: left; vertical-align: top;' >Department</th>";
                    facultyclosure += "<th style='text-align: left; vertical-align: top;' >Degree</th>";
                    facultyclosure += "<th style='text-align: left; vertical-align: top;' >Specialization</th>";



                    facultyclosure += "</tr>";


                    int ProposedIntakeZeroIndex = 1;
                    foreach (var item in facultyCounts.Where(e => e.approvedIntake1 == 0 || ProposedIntakeZeroDeptName.Contains(e.Specialization)).Select(e => e).ToList())
                    {
                        // distDeptcount = facultyCounts.Where(d => d.Department == item.Department).Distinct().Count();
                        if (item.Degree == "M.Tech" || item.Degree == "MCA" || item.Degree == "MBA")
                        {



                            if (item.Degree == "M.Tech")
                            {
                                SpecializationwisePHDFaculty = IntakeWisePhdForMtech(item.approvedIntake1, item.shiftId);
                            }
                            else if (item.Degree == "MCA")
                            {
                                SpecializationwisePHDFaculty = IntakeWisePhdForMBAandMCA(item.approvedIntake1);
                            }
                            else if (item.Degree == "MBA")
                            {
                                SpecializationwisePHDFaculty = IntakeWisePhdForMBAandMCA(item.approvedIntake1);
                            }


                        }
                        else if (item.Degree == "B.Tech")
                        {
                            SpecializationwisePHDFaculty = IntakeWisePhdForBtech(item.approvedIntake1);
                        }
                        TotalCount = facultyCounts.Where(D => D.Department == item.Department && (D.Degree == "M.Tech" || D.Degree == "B.Tech")).Distinct().Count();

                        int indexnow = facultyCounts.IndexOf(item);



                        if (indexnow > 0 && facultyCounts[indexnow].Department != facultyCounts[indexnow - 1].Department)
                        {
                            deptloop = 1;
                        }



                        string minimumRequirementMet = string.Empty;

                        string PhdminimumRequirementMet = string.Empty;


                        int facultyShortage = 0, tFaculty = 0;
                        int adjustedFaculty = 0;
                        int adjustedPHDFaculty = 0;
                        int othersRequiredfaculty = 0;

                        if (item.Department == "MBA" || item.Department == "MCA")
                            tFaculty = (int)Math.Ceiling(Convert.ToDecimal(item.totalFaculty));//item.totalFaculty
                        else
                            tFaculty = (int)Math.Ceiling(Convert.ToDecimal(item.specializationWiseFaculty));//item.totalFaculty
                        int rFaculty = (int)Math.Ceiling(item.requiredFaculty);

                        if (departments.Contains(item.Department))
                        {
                            if (OthersSpecIds.Contains(item.specializationId))
                            {
                                rFaculty = ((int)firstYearRequired) / 2;
                                if (rFaculty <= 2)
                                {
                                    rFaculty = 2;
                                    othersRequiredfaculty = 2;
                                }
                                else
                                {
                                    othersRequiredfaculty = rFaculty;
                                }

                            }
                            else
                            {
                                rFaculty = (int)firstYearRequired;
                            }


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
                                // facultyShortage = rFaculty - tFaculty;
                            }

                            remainingPHDFaculty = item.phdFaculty;

                            if (remainingPHDFaculty > 2 && (degreeType.Equals("PG")) && item.SpecializationsphdFaculty > 0)
                            {

                                remainingPHDFaculty = remainingPHDFaculty - item.SpecializationsphdFaculty;
                                adjustedPHDFaculty = item.SpecializationsphdFaculty;
                                PhdminimumRequirementMet = "YES";
                            }
                            else if (remainingPHDFaculty <= 1 && (degreeType.Equals("PG")) && item.SpecializationsphdFaculty > 0)
                            {
                                adjustedPHDFaculty = remainingPHDFaculty;
                                remainingPHDFaculty = remainingPHDFaculty - 1;
                                PhdminimumRequirementMet = "NO";
                            }
                            else if (SpecializationwisePHDFaculty > 0 && (degreeType.Equals("UG")) && remainingPHDFaculty > 0)
                            {

                                if (item.SpecializationsphdFaculty >= SpecializationwisePHDFaculty)
                                {
                                    remainingPHDFaculty = remainingPHDFaculty - item.SpecializationsphdFaculty;
                                    adjustedPHDFaculty = item.SpecializationsphdFaculty;
                                    PhdminimumRequirementMet = "YES";
                                }
                                else if (item.SpecializationsphdFaculty <= SpecializationwisePHDFaculty)
                                {
                                    remainingPHDFaculty = remainingPHDFaculty - SpecializationwisePHDFaculty;
                                    adjustedPHDFaculty = SpecializationwisePHDFaculty;
                                    PhdminimumRequirementMet = "YES";
                                }



                            }
                            else if (SpecializationwisePHDFaculty > 0 && (degreeType.Equals("UG")) && remainingPHDFaculty <= 0)
                            {

                                adjustedPHDFaculty = remainingPHDFaculty;
                                PhdminimumRequirementMet = "NO";

                            }
                            else if (SpecializationwisePHDFaculty == 0 && (degreeType.Equals("UG")))
                            {
                                PhdminimumRequirementMet = "NO";
                            }
                            else if (adjustedPHDFaculty <= 0 && (degreeType.Equals("PG")))
                            {
                                PhdminimumRequirementMet = "YES";
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

                            if (remainingPHDFaculty >= 2 && (degreeType.Equals("PG")) && item.SpecializationsphdFaculty > 0)
                            {
                                remainingPHDFaculty = remainingPHDFaculty - item.SpecializationsphdFaculty;
                                adjustedPHDFaculty = item.SpecializationsphdFaculty;

                                PhdminimumRequirementMet = "YES";
                            }
                            else if (remainingPHDFaculty <= 1 && (degreeType.Equals("PG")) && item.SpecializationsphdFaculty > 0)
                            {

                                adjustedPHDFaculty = remainingPHDFaculty;

                                remainingPHDFaculty = remainingPHDFaculty - 1;
                                PhdminimumRequirementMet = "NO";
                            }
                            else if (SpecializationwisePHDFaculty > 0 && (degreeType.Equals("UG")) && remainingPHDFaculty > 0)
                            {

                                if (item.SpecializationsphdFaculty >= SpecializationwisePHDFaculty)
                                {
                                    remainingPHDFaculty = remainingPHDFaculty - item.SpecializationsphdFaculty;
                                    adjustedPHDFaculty = item.SpecializationsphdFaculty;
                                    PhdminimumRequirementMet = "YES";
                                }
                                else if (item.SpecializationsphdFaculty <= SpecializationwisePHDFaculty)
                                {
                                    remainingPHDFaculty = remainingPHDFaculty - SpecializationwisePHDFaculty;
                                    adjustedPHDFaculty = SpecializationwisePHDFaculty;
                                    PhdminimumRequirementMet = "YES";
                                }

                            }
                            else if (SpecializationwisePHDFaculty > 0 && (degreeType.Equals("UG")) && remainingPHDFaculty <= 0)
                            {

                                adjustedPHDFaculty = remainingPHDFaculty;
                                PhdminimumRequirementMet = "NO";
                            }
                            else if (SpecializationwisePHDFaculty == 0 && (degreeType.Equals("UG")))
                            {
                                PhdminimumRequirementMet = "NO";
                            }
                            else if (adjustedPHDFaculty <= 0 && (degreeType.Equals("PG")))
                            {
                                PhdminimumRequirementMet = "YES";
                            }
                        }




                        facultyclosure += "<tr>";
                        facultyclosure += "<td class='col2' style='text-align: center; vertical-align: top;width:10px' rowspan='1'>" + ProposedIntakeZeroIndex + "</td>";
                        facultyclosure += "<td class='col2' style='text-align: left; vertical-align: top;'>" + item.Department + "</td>";
                        facultyclosure += "<td class='col2' style='text-align: left; vertical-align: top;'>" + item.Degree + "</td>";
                        facultyclosure += "<td class='col2' style='text-align: left; vertical-align: top;'>" + item.Specialization + "</td>";


                        //DeficiencyInactiveFaculty += "<tr>";
                        //DeficiencyInactiveFaculty += "<td>";
                        //DeficiencyInactiveFaculty += "<table>";
                        //DeficiencyInactiveFaculty += "<tr>";
                        //DeficiencyInactiveFaculty += "<td>S.No</td>";
                        //DeficiencyInactiveFaculty += "<td>Department</td>";
                        //DeficiencyInactiveFaculty += "<td>Degree</td>";
                        //DeficiencyInactiveFaculty += "<td>Specialization</td>";
                        //DeficiencyInactiveFaculty += "<td>SantionedIntake</td>";
                        //DeficiencyInactiveFaculty += "<td>ProposedInatke</td>";
                        //DeficiencyInactiveFaculty += "<td>Total Department Faculty</td>";
                        //DeficiencyInactiveFaculty += "<td>Specialization Wise Faculty Required</td>";
                        //DeficiencyInactiveFaculty += "<td>Specialization Wise Faculty Available</td>";
                        //DeficiencyInactiveFaculty += "<td>Required Ph.D faculty</td>";
                        //DeficiencyInactiveFaculty += "<td>Available Ph.D faculty</td>";
                        //DeficiencyInactiveFaculty += "<td>Deficiency In faculty and Phd</td>";
                        //DeficiencyInactiveFaculty += "</tr>";
                        //DeficiencyInactiveFaculty += "</table>";
                        //DeficiencyInactiveFaculty += "</td>";
                        //DeficiencyInactiveFaculty += "</tr>";
                        //DeficiencyInactiveFaculty += "</table>";


                        if (departments.Contains(item.Department))
                        {



                            if (OthersSpecIds.Contains(item.specializationId))
                            {

                                HumantitiesminimamRequireMet += item.totalFaculty;
                            }
                            else
                            {

                                HumantitiesminimamRequireMet += item.totalFaculty;
                            }


                            if (minimumRequirementMet != "YES")
                            {
                                HumantitiesminimamRequireMetStatus = "NO";
                            }
                        }
                        else
                        {



                            if (item.Degree == "B.Tech")
                            {




                            }
                            else
                            {

                            }



                        }
                        facultycount = facultycount + item.specializationWiseFaculty;
                        if (adjustedFaculty > 0)
                            adjustedFaculty = adjustedFaculty;
                        else
                            adjustedFaculty = 0;

                        if (adjustedPHDFaculty > 0)
                            adjustedPHDFaculty = adjustedPHDFaculty;
                        else
                            adjustedPHDFaculty = 0;








                        if (minimumRequirementMet == "YES" && PhdminimumRequirementMet == "YES")
                        {
                            if (rFaculty <= adjustedFaculty)
                                minimumRequirementMet = "No Deficiency In faculty";
                            else
                                minimumRequirementMet = "Deficiency In faculty";

                            if (SpecializationwisePHDFaculty <= adjustedPHDFaculty)
                                minimumRequirementMet += " and No Deficiency In Phd";
                            else
                                minimumRequirementMet += " and Deficiency In Phd";

                        }

                        else if (minimumRequirementMet == "NO" && PhdminimumRequirementMet == "YES")
                        {
                            if (rFaculty == adjustedFaculty)
                                minimumRequirementMet = "No Deficiency In faculty";
                            else
                                minimumRequirementMet = "Deficiency In faculty";


                            if (SpecializationwisePHDFaculty <= adjustedPHDFaculty)
                                minimumRequirementMet += " and No Deficiency In Phd";
                            else
                                minimumRequirementMet += " and Deficiency In Phd";
                        }
                        else if (minimumRequirementMet == "YES" && PhdminimumRequirementMet == "NO")
                        {
                            if (rFaculty <= adjustedFaculty)
                                minimumRequirementMet = "No Deficiency In faculty";
                            else
                                minimumRequirementMet = "Deficiency In faculty";

                            if (SpecializationwisePHDFaculty == adjustedPHDFaculty)
                                minimumRequirementMet += " and No Deficiency In Phd";
                            else
                                minimumRequirementMet += " and Deficiency In Phd";

                        }

                        else if (minimumRequirementMet == "NO" && PhdminimumRequirementMet == "NO")
                        {
                            if (rFaculty == adjustedFaculty)
                                minimumRequirementMet = "No Deficiency In faculty";
                            else
                                minimumRequirementMet = "Deficiency In faculty";


                            if (SpecializationwisePHDFaculty == adjustedPHDFaculty)
                                minimumRequirementMet += " and No Deficiency In Phd";
                            else
                                minimumRequirementMet += " and Deficiency In Phd";
                        }







                        facultyclosure += "</tr>";

                        deptloop++;
                        SpecializationwisePHDFaculty = 0;
                        ProposedIntakeZeroIndex++;
                    }
                    facultyclosure += "</table>";



                    // faculty += facultyclosure;

                    #endregion

                    #region 25% Admitted Intake Cources

                    if (
                        facultyCounts.Where(
                            e =>
                                e.Degree == "B.Tech" && e.ispercentage == true &&
                                twentyfivePersentbelowDeptName.Contains(e.Specialization)).Select(e => e).ToList().Count !=
                        0)
                    {

                        twintyfivepercentbelowcurces +=
                            "<p><b>*</b> The following Courses have either Zero Admissions due to non-grant of Affiliation or Admitted intake is less than 25% of JNTUH Sanctioned intake for the previous three academic years.Hence SCA has recommend for <b>&lsquo;No Admission Status&rsquo;</b> for these courses for the A.Y.2018-19 as per clause: 3.30 of Affiliation Regulations 2018-19.However, if the admitted intake in any of the applied courses is atleast 15 and above, in any one of the previous three years,then such courses will be considered for Affiliation a minimum of 60 intake provided then meet other requirements as per norms.</p>";
                        twintyfivepercentbelowcurces +=
                            "<table width='100%' border='1' cellpadding='3' cellspacing='0' style='border-color: #ccc;'>";
                        twintyfivepercentbelowcurces += "<tr>";
                        twintyfivepercentbelowcurces += "<th style='text-align: center; vertical-align: top;'>SNo</th>";
                        twintyfivepercentbelowcurces +=
                            "<th style='text-align: left; vertical-align: top;' >Department</th>";
                        twintyfivepercentbelowcurces +=
                            "<th style='text-align: left; vertical-align: top;' >Degree</th>";
                        twintyfivepercentbelowcurces +=
                            "<th style='text-align: left; vertical-align: top;' >Specialization</th>";
                        twintyfivepercentbelowcurces +=
                            "<th style='text-align: left; vertical-align: top;' >Proposed Intake</th>";
                        facultyclosure += "<th style='text-align: center; vertical-align: top;' >Sanction Intake</th>";
                        facultyclosure += "<th style='text-align: center; vertical-align: top;' >Admitted Intake</th>";


                        facultyclosure += "<th style='text-align: center; vertical-align: top;' >Proposed Intake</th>";

                        facultyclosure += "</tr>";


                        int ProposedIntakeZeroIndex1 = 1;
                        foreach (
                            var item in
                                facultyCounts.Where(
                                    e =>
                                        e.Degree == "B.Tech" && e.ispercentage == true &&
                                        twentyfivePersentbelowDeptName.Contains(e.Specialization))
                                    .Select(e => e)
                                    .ToList())
                        {
                            // distDeptcount = facultyCounts.Where(d => d.Department == item.Department).Distinct().Count();
                            if (item.Degree == "M.Tech" || item.Degree == "MCA" || item.Degree == "MBA")
                            {



                                if (item.Degree == "M.Tech")
                                {
                                    SpecializationwisePHDFaculty = IntakeWisePhdForMtech(item.approvedIntake1,
                                        item.shiftId);
                                }
                                else if (item.Degree == "MCA")
                                {
                                    SpecializationwisePHDFaculty = IntakeWisePhdForMBAandMCA(item.approvedIntake1);
                                }
                                else if (item.Degree == "MBA")
                                {
                                    SpecializationwisePHDFaculty = IntakeWisePhdForMBAandMCA(item.approvedIntake1);
                                }


                            }
                            else if (item.Degree == "B.Tech")
                            {
                                SpecializationwisePHDFaculty = IntakeWisePhdForBtech(item.approvedIntake1);
                            }
                            TotalCount =
                                facultyCounts.Where(
                                    D =>
                                        D.Department == item.Department &&
                                        (D.Degree == "M.Tech" || D.Degree == "B.Tech")).Distinct().Count();



                            int indexnow = facultyCounts.IndexOf(item);



                            if (indexnow > 0 &&
                                facultyCounts[indexnow].Department != facultyCounts[indexnow - 1].Department)
                            {
                                deptloop = 1;
                            }



                            string minimumRequirementMet = string.Empty;

                            string PhdminimumRequirementMet = string.Empty;


                            int facultyShortage = 0, tFaculty = 0;
                            int adjustedFaculty = 0;
                            int adjustedPHDFaculty = 0;
                            int othersRequiredfaculty = 0;

                            if (item.Department == "MBA" || item.Department == "MCA")
                                tFaculty = (int)Math.Ceiling(Convert.ToDecimal(item.totalFaculty)); //item.totalFaculty
                            else
                                tFaculty = (int)Math.Ceiling(Convert.ToDecimal(item.specializationWiseFaculty));
                            //item.totalFaculty
                            int rFaculty = (int)Math.Ceiling(item.requiredFaculty);

                            if (departments.Contains(item.Department))
                            {
                                if (OthersSpecIds.Contains(item.specializationId))
                                {
                                    rFaculty = ((int)firstYearRequired) / 2;
                                    if (rFaculty <= 2)
                                    {
                                        rFaculty = 2;
                                        othersRequiredfaculty = 2;
                                    }
                                    else
                                    {
                                        othersRequiredfaculty = rFaculty;
                                    }

                                }
                                else
                                {
                                    rFaculty = (int)firstYearRequired;
                                }


                            }

                            var degreeType =
                                degrees.Where(d => d.degree == item.Degree)
                                    .Select(d => d.jntuh_degree_type.degreeType)
                                    .FirstOrDefault();

                            if (deptloop == 1)
                            {
                                if (rFaculty <= tFaculty)
                                {
                                    minimumRequirementMet = "YES";
                                    item.deficiency = false;
                                    remainingFaculty = tFaculty - rFaculty;
                                    adjustedFaculty = rFaculty; //tFaculty
                                }
                                else
                                {
                                    minimumRequirementMet = "NO";
                                    item.deficiency = true;
                                    adjustedFaculty = tFaculty;
                                    // facultyShortage = rFaculty - tFaculty;
                                }

                                remainingPHDFaculty = item.phdFaculty;

                                if (remainingPHDFaculty > 2 && (degreeType.Equals("PG")) &&
                                    item.SpecializationsphdFaculty > 0)
                                {

                                    remainingPHDFaculty = remainingPHDFaculty - item.SpecializationsphdFaculty;
                                    adjustedPHDFaculty = item.SpecializationsphdFaculty;
                                    PhdminimumRequirementMet = "YES";
                                }
                                else if (remainingPHDFaculty <= 1 && (degreeType.Equals("PG")) &&
                                         item.SpecializationsphdFaculty > 0)
                                {
                                    adjustedPHDFaculty = remainingPHDFaculty;
                                    remainingPHDFaculty = remainingPHDFaculty - 1;
                                    PhdminimumRequirementMet = "NO";
                                }
                                else if (SpecializationwisePHDFaculty > 0 && (degreeType.Equals("UG")) &&
                                         remainingPHDFaculty > 0)
                                {

                                    if (item.SpecializationsphdFaculty >= SpecializationwisePHDFaculty)
                                    {
                                        remainingPHDFaculty = remainingPHDFaculty -
                                                              item.SpecializationsphdFaculty;
                                        adjustedPHDFaculty = item.SpecializationsphdFaculty;
                                        PhdminimumRequirementMet = "YES";
                                    }
                                    else if (item.SpecializationsphdFaculty <= SpecializationwisePHDFaculty)
                                    {
                                        remainingPHDFaculty = remainingPHDFaculty - SpecializationwisePHDFaculty;
                                        adjustedPHDFaculty = SpecializationwisePHDFaculty;
                                        PhdminimumRequirementMet = "YES";
                                    }



                                }
                                else if (SpecializationwisePHDFaculty > 0 && (degreeType.Equals("UG")) &&
                                         remainingPHDFaculty <= 0)
                                {

                                    adjustedPHDFaculty = remainingPHDFaculty;
                                    PhdminimumRequirementMet = "NO";

                                }
                                else if (SpecializationwisePHDFaculty == 0 && (degreeType.Equals("UG")))
                                {
                                    PhdminimumRequirementMet = "NO";
                                }
                                else if (adjustedPHDFaculty <= 0 && (degreeType.Equals("PG")))
                                {
                                    PhdminimumRequirementMet = "YES";
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
                                //  remainingPHDFaculty = item.phdFaculty;
                                if (remainingPHDFaculty >= 2 && (degreeType.Equals("PG")) &&
                                    item.SpecializationsphdFaculty > 0)
                                {
                                    remainingPHDFaculty = remainingPHDFaculty - item.SpecializationsphdFaculty;
                                    adjustedPHDFaculty = item.SpecializationsphdFaculty;

                                    PhdminimumRequirementMet = "YES";
                                }
                                else if (remainingPHDFaculty <= 1 && (degreeType.Equals("PG")) &&
                                         item.SpecializationsphdFaculty > 0)
                                {

                                    adjustedPHDFaculty = remainingPHDFaculty;

                                    remainingPHDFaculty = remainingPHDFaculty - 1;
                                    PhdminimumRequirementMet = "NO";
                                }
                                else if (SpecializationwisePHDFaculty > 0 && (degreeType.Equals("UG")) &&
                                         remainingPHDFaculty > 0)
                                {

                                    if (item.SpecializationsphdFaculty >= SpecializationwisePHDFaculty)
                                    {
                                        remainingPHDFaculty = remainingPHDFaculty -
                                                              item.SpecializationsphdFaculty;
                                        adjustedPHDFaculty = item.SpecializationsphdFaculty;
                                        PhdminimumRequirementMet = "YES";
                                    }
                                    else if (item.SpecializationsphdFaculty <= SpecializationwisePHDFaculty)
                                    {
                                        remainingPHDFaculty = remainingPHDFaculty - SpecializationwisePHDFaculty;
                                        adjustedPHDFaculty = SpecializationwisePHDFaculty;
                                        PhdminimumRequirementMet = "YES";
                                    }

                                }
                                else if (SpecializationwisePHDFaculty > 0 && (degreeType.Equals("UG")) &&
                                         remainingPHDFaculty <= 0)
                                {
                                    // remainingPHDFaculty = SpecializationwisePHDFaculty - remainingPHDFaculty;
                                    adjustedPHDFaculty = remainingPHDFaculty;
                                    PhdminimumRequirementMet = "NO";
                                }
                                else if (SpecializationwisePHDFaculty == 0 && (degreeType.Equals("UG")))
                                {
                                    PhdminimumRequirementMet = "NO";
                                }
                                else if (adjustedPHDFaculty <= 0 && (degreeType.Equals("PG")))
                                {
                                    PhdminimumRequirementMet = "YES";
                                }
                            }




                            twintyfivepercentbelowcurces += "<tr>";
                            twintyfivepercentbelowcurces +=
                                "<td class='col2' style='text-align: center; vertical-align: top;width:10px' rowspan='1'>" +
                                ProposedIntakeZeroIndex1 + "</td>";
                            twintyfivepercentbelowcurces +=
                                "<td class='col2' style='text-align: left; vertical-align: top;'>" + item.Department +
                                "</td>";
                            twintyfivepercentbelowcurces +=
                                "<td class='col2' style='text-align: left; vertical-align: top;'>" + item.Degree +
                                "</td>";
                            twintyfivepercentbelowcurces +=
                                "<td class='col2' style='text-align: left; vertical-align: top;'>" + item.Specialization +
                                "</td>";
                            twintyfivepercentbelowcurces +=
                               "<td class='col2' style='text-align: left; vertical-align: top;'>" + item.approvedIntake1 +
                               "</td>";

                            if (departments.Contains(item.Department))
                            {

                                //facultyclosure += "<td class='col2' style='text-align: center; vertical-align: top;'> </td>";

                                //facultyclosure += "<td class='col2' style='text-align: center; vertical-align: top;'> </td>";
                                //facultyclosure += "<td class='col2' style='text-align: center; vertical-align: top;'>" + totalBtechFirstYearIntake + "</td>";


                                //if (deptloop == 1)
                                //    facultyclosure += "<td rowspan='" + TotalCount + "'  class='col2' style='text-align: center; vertical-align: top;'>" + item.totalFaculty + "</td>";

                                if (OthersSpecIds.Contains(item.specializationId))
                                {
                                    // facultyclosure += "<td class='col2' style='text-align: center; vertical-align: top;'>" + othersRequiredfaculty + "</td>";
                                    HumantitiesminimamRequireMet += item.totalFaculty;
                                }
                                else
                                {
                                    //  facultyclosure += "<td class='col2' style='text-align: center; vertical-align: top;'>" + Math.Ceiling(firstYearRequired) + "</td>";
                                    HumantitiesminimamRequireMet += item.totalFaculty;
                                }


                                if (minimumRequirementMet != "YES")
                                {
                                    HumantitiesminimamRequireMetStatus = "NO";
                                }
                            }
                            else
                            {



                                if (item.Degree == "B.Tech")
                                {



                                }
                                else
                                {


                                }



                            }
                            facultycount = facultycount + item.specializationWiseFaculty;
                            if (adjustedFaculty > 0)
                                adjustedFaculty = adjustedFaculty;
                            else
                                adjustedFaculty = 0;

                            if (adjustedPHDFaculty > 0)
                                adjustedPHDFaculty = adjustedPHDFaculty;
                            else
                                adjustedPHDFaculty = 0;







                            if (minimumRequirementMet == "YES" && PhdminimumRequirementMet == "YES")
                            {
                                if (rFaculty <= adjustedFaculty)
                                    minimumRequirementMet = "No Deficiency In faculty";
                                else
                                    minimumRequirementMet = "Deficiency In faculty";

                                if (SpecializationwisePHDFaculty <= adjustedPHDFaculty)
                                    minimumRequirementMet += " and No Deficiency In Phd";
                                else
                                    minimumRequirementMet += " and Deficiency In Phd";

                            }

                            else if (minimumRequirementMet == "NO" && PhdminimumRequirementMet == "YES")
                            {
                                if (rFaculty == adjustedFaculty)
                                    minimumRequirementMet = "No Deficiency In faculty";
                                else
                                    minimumRequirementMet = "Deficiency In faculty";


                                if (SpecializationwisePHDFaculty <= adjustedPHDFaculty)
                                    minimumRequirementMet += " and No Deficiency In Phd";
                                else
                                    minimumRequirementMet += " and Deficiency In Phd";
                            }
                            else if (minimumRequirementMet == "YES" && PhdminimumRequirementMet == "NO")
                            {
                                if (rFaculty <= adjustedFaculty)
                                    minimumRequirementMet = "No Deficiency In faculty";
                                else
                                    minimumRequirementMet = "Deficiency In faculty";

                                if (SpecializationwisePHDFaculty == adjustedPHDFaculty)
                                    minimumRequirementMet += " and No Deficiency In Phd";
                                else
                                    minimumRequirementMet += " and Deficiency In Phd";

                            }

                            else if (minimumRequirementMet == "NO" && PhdminimumRequirementMet == "NO")
                            {
                                if (rFaculty == adjustedFaculty)
                                    minimumRequirementMet = "No Deficiency In faculty";
                                else
                                    minimumRequirementMet = "Deficiency In faculty";


                                if (SpecializationwisePHDFaculty == adjustedPHDFaculty)
                                    minimumRequirementMet += " and No Deficiency In Phd";
                                else
                                    minimumRequirementMet += " and Deficiency In Phd";
                            }

                            twintyfivepercentbelowcurces += "</tr>";

                            deptloop++;
                            SpecializationwisePHDFaculty = 0;
                            ProposedIntakeZeroIndex1++;
                        }
                        twintyfivepercentbelowcurces += "</table>";



                        //faculty += twintyfivepercentbelowcurces;
                    }

                    #endregion

                }
                else
                {

                    if (collegeStatus.CollegeStatus == true)
                    {
                        faculty += "<p>As per the records the programs/courses in which there were zero admissions or no grant of affiliation by the University in the previous three academic years are shown in the following table. It is observed that for two or more than two academic years there were zero admissions in all the programs/courses either due to non grant of affiliation or unable to make admissions at your Institute. As per the records it is also observed that there is a mismatch of faculty information that you have submitted to AICTE and the University. </p>";
                        string[] admittedIntakeTwoormoreZeroDeptName1 = facultyCounts.Where(e => e.Degree == "B.Tech" && ((e.approvedIntake2 == 0 && e.approvedIntake3 == 0) || (e.approvedIntake3 == 0 && e.approvedIntake4 == 0) || (e.approvedIntake2 == 0 && e.approvedIntake4 == 0)) && !departments.Contains(e.Department)).Select(e => e.Department).Distinct().ToArray();
                        facultyAdmittedIntakeZero += "<table width='100%' border='1' cellpadding='3' cellspacing='0' style='border-color: #ccc;'>";
                        facultyAdmittedIntakeZero += "<tr>";
                        facultyAdmittedIntakeZero += "<th style='text-align: center; vertical-align: top;'>SNo</th>";
                        facultyAdmittedIntakeZero += "<th style='text-align: left; vertical-align: top;' >Department</th>";
                        facultyAdmittedIntakeZero += "<th style='text-align: left; vertical-align: top;' >Degree</th>";
                        facultyAdmittedIntakeZero += "<th style='text-align: left; vertical-align: top;' >Specialization</th>";
                        facultyAdmittedIntakeZero += "<th style='text-align: center; vertical-align: top;' >Sanction Intake</th>";
                        facultyAdmittedIntakeZero += "<th style='text-align: center; vertical-align: top;' >Admitted Intake</th>";
                        facultyAdmittedIntakeZero += "<th style='text-align: center; vertical-align: top;' >Proposed Intake</th>";
                        //facultyAdmittedIntakeZero += "<th style='text-align: center; vertical-align: top;' >Affiliation Status 2016-17</th>";
                        //facultyAdmittedIntakeZero += "<th style='text-align: center; vertical-align: top;' >Affiliation Status 2015-16</th>";
                        //facultyAdmittedIntakeZero += "<th style='text-align: center; vertical-align: top;' >Affiliation Status 2014-15</th>";
                        facultyAdmittedIntakeZero += "</tr>";

                        int facultyIndexnotEqualtoZeroAdmittedIntakeIndex = 1;
                        foreach (var item in facultyCounts.Where(e => e.Degree == "B.Tech" && e.ispercentage == false && !departments.Contains(e.Department) && e.Proposedintake != 0 && !ProposedIntakeZeroDeptName.Contains(e.Specialization)).Select(e => e).ToList())//e.approvedIntake1 != 0 && !ProposedIntakeZeroDeptName.Contains(e.Department) &&
                        {



                            // if (((item.approvedIntake2 == 0 && item.approvedIntake3 == 0) && (item.approvedIntake3 == 0 && item.approvedIntake4 == 0) && (item.approvedIntake2 == 0 && item.approvedIntake4 == 0) || admittedIntakeTwoormoreZeroDeptName1.Contains(item.Department)) && !departments.Contains(item.Department))
                            // {

                            #region Admitted Intake With Zero

                            if (item.Degree == "M.Tech" || item.Degree == "MCA" || item.Degree == "MBA")
                            {

                                if (item.Degree == "M.Tech")
                                {
                                    SpecializationwisePHDFaculty = IntakeWisePhdForMtech(item.approvedIntake1, item.shiftId);
                                }
                                else if (item.Degree == "MCA")
                                {
                                    SpecializationwisePHDFaculty = IntakeWisePhdForMBAandMCA(item.approvedIntake1);
                                }
                                else if (item.Degree == "MBA")
                                {
                                    SpecializationwisePHDFaculty = IntakeWisePhdForMBAandMCA(item.approvedIntake1);
                                }


                            }
                            else if (item.Degree == "B.Tech")
                            {
                                SpecializationwisePHDFaculty = IntakeWisePhdForBtech(item.approvedIntake1);
                            }
                            TotalCount =
                                facultyCounts.Where(
                                    D =>
                                        D.Department == item.Department &&
                                        (D.Degree == "M.Tech" || D.Degree == "B.Tech") && D.approvedIntake1 != 0)
                                    .Distinct()
                                    .Count();



                            //if (SpecializationIDS.Contains(item.specializationId))
                            //{
                            //    int SpecializationwisePGFaculty1 = facultyCounts.Count(S => S.specializationId==item.specializationId);
                            //    SpecializationwisePGFaculty = facultyCounts.Where(S => S.specializationId==item.specializationId).Select(S => S.SpecializationspgFaculty).FirstOrDefault();

                            //}
                            int indexnow = facultyCounts.IndexOf(item);



                            if (indexnow > 0 &&
                                facultyCounts[indexnow].Department != facultyCounts[indexnow - 1].Department)
                            {
                                deptloop = 1;
                            }

                            //  departmentWiseRequiredFaculty = facultyCounts.Where(d => d.Department == item.Department).Select(d => d.requiredFaculty).Sum();

                            string minimumRequirementMet = string.Empty;

                            string PhdminimumRequirementMet = string.Empty;


                            int facultyShortage = 0, tFaculty = 0;
                            int adjustedFaculty = 0;
                            int adjustedPHDFaculty = 0;
                            int othersRequiredfaculty = 0;

                            if (item.Department == "MBA" || item.Department == "MCA")
                                tFaculty = (int)Math.Ceiling(Convert.ToDecimal(item.totalFaculty)); //item.totalFaculty
                            else
                                tFaculty = (int)Math.Ceiling(Convert.ToDecimal(item.specializationWiseFaculty));
                            //item.totalFaculty
                            int rFaculty = (int)Math.Ceiling(item.requiredFaculty);

                            if (departments.Contains(item.Department))
                            {
                                if (OthersSpecIds.Contains(item.specializationId))
                                {
                                    rFaculty = ((int)firstYearRequired) / 2;
                                    if (rFaculty <= 2)
                                    {
                                        rFaculty = 2;
                                        othersRequiredfaculty = 2;
                                    }
                                    else
                                    {
                                        othersRequiredfaculty = rFaculty;
                                    }

                                }
                                else
                                {
                                    rFaculty = (int)firstYearRequired;
                                }

                                // departmentWiseRequiredFaculty = (int)firstYearRequired;
                            }

                            var degreeType =
                                degrees.Where(d => d.degree == item.Degree)
                                    .Select(d => d.jntuh_degree_type.degreeType)
                                    .FirstOrDefault();

                            if (deptloop == 1)
                            {
                                if (rFaculty <= tFaculty)
                                {
                                    minimumRequirementMet = "YES";
                                    item.deficiency = false;
                                    remainingFaculty = tFaculty - rFaculty;
                                    adjustedFaculty = rFaculty; //tFaculty
                                }
                                else
                                {
                                    minimumRequirementMet = "NO";
                                    item.deficiency = true;
                                    adjustedFaculty = tFaculty;
                                    // facultyShortage = rFaculty - tFaculty;
                                }

                                remainingPHDFaculty = item.phdFaculty;

                                if (remainingPHDFaculty >= 2 && (degreeType.Equals("PG")) &&
                                    item.SpecializationsphdFaculty > 0)
                                {

                                    remainingPHDFaculty = remainingPHDFaculty - item.SpecializationsphdFaculty;
                                    adjustedPHDFaculty = item.SpecializationsphdFaculty;
                                    PhdminimumRequirementMet = "YES";
                                }
                                else if (remainingPHDFaculty <= 1 && (degreeType.Equals("PG")) &&
                                         item.SpecializationsphdFaculty > 0)
                                {
                                    adjustedPHDFaculty = remainingPHDFaculty;
                                    remainingPHDFaculty = remainingPHDFaculty - 1;
                                    PhdminimumRequirementMet = "NO";
                                }
                                else if (SpecializationwisePHDFaculty > 0 && (degreeType.Equals("UG")) &&
                                         remainingPHDFaculty > 0)
                                {

                                    if (item.SpecializationsphdFaculty >= SpecializationwisePHDFaculty)
                                    {
                                        remainingPHDFaculty = remainingPHDFaculty -
                                                              item.SpecializationsphdFaculty;
                                        adjustedPHDFaculty = item.SpecializationsphdFaculty;
                                        PhdminimumRequirementMet = "YES";
                                    }
                                    else if (item.SpecializationsphdFaculty <= SpecializationwisePHDFaculty)
                                    {
                                        remainingPHDFaculty = remainingPHDFaculty - SpecializationwisePHDFaculty;
                                        adjustedPHDFaculty = SpecializationwisePHDFaculty;
                                        PhdminimumRequirementMet = "YES";
                                    }



                                }
                                else if (SpecializationwisePHDFaculty > 0 && (degreeType.Equals("UG")) &&
                                         remainingPHDFaculty <= 0)
                                {
                                    // remainingPHDFaculty = SpecializationwisePHDFaculty - remainingPHDFaculty;
                                    adjustedPHDFaculty = remainingPHDFaculty;
                                    PhdminimumRequirementMet = "NO";

                                }
                                else if (SpecializationwisePHDFaculty == 0 && (degreeType.Equals("UG")))
                                {
                                    PhdminimumRequirementMet = "NO";
                                }
                                else if (adjustedPHDFaculty <= 0 && (degreeType.Equals("PG")))
                                {
                                    PhdminimumRequirementMet = "YES";
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

                                if (remainingPHDFaculty >= 2 && (degreeType.Equals("PG")) &&
                                    item.SpecializationsphdFaculty > 0)
                                {
                                    remainingPHDFaculty = remainingPHDFaculty - item.SpecializationsphdFaculty;
                                    adjustedPHDFaculty = item.SpecializationsphdFaculty;
                                    PhdminimumRequirementMet = "YES";
                                }
                                else if (remainingPHDFaculty <= 1 && (degreeType.Equals("PG")) &&
                                         item.SpecializationsphdFaculty > 0)
                                {
                                    adjustedPHDFaculty = remainingPHDFaculty;
                                    remainingPHDFaculty = remainingPHDFaculty - 1;
                                    PhdminimumRequirementMet = "NO";
                                }
                                else if (SpecializationwisePHDFaculty > 0 && (degreeType.Equals("UG")) &&
                                         remainingPHDFaculty > 0)
                                {

                                    if (item.SpecializationsphdFaculty >= SpecializationwisePHDFaculty)
                                    {
                                        remainingPHDFaculty = remainingPHDFaculty -
                                                              item.SpecializationsphdFaculty;
                                        adjustedPHDFaculty = item.SpecializationsphdFaculty;
                                        PhdminimumRequirementMet = "YES";
                                    }
                                    else if (item.SpecializationsphdFaculty <= SpecializationwisePHDFaculty)
                                    {
                                        remainingPHDFaculty = remainingPHDFaculty - SpecializationwisePHDFaculty;
                                        adjustedPHDFaculty = SpecializationwisePHDFaculty;
                                        PhdminimumRequirementMet = "YES";
                                    }

                                }
                                else if (SpecializationwisePHDFaculty > 0 && (degreeType.Equals("UG")) &&
                                         remainingPHDFaculty <= 0)
                                {

                                    adjustedPHDFaculty = remainingPHDFaculty;
                                    PhdminimumRequirementMet = "NO";
                                }
                                else if (SpecializationwisePHDFaculty == 0 && (degreeType.Equals("UG")))
                                {
                                    PhdminimumRequirementMet = "NO";
                                }
                                else if (adjustedPHDFaculty <= 0 && (degreeType.Equals("PG")))
                                {
                                    PhdminimumRequirementMet = "YES";
                                }
                            }




                            facultyAdmittedIntakeZero += "<tr>";
                            facultyAdmittedIntakeZero += "<td class='col2' style='text-align: center; vertical-align: top;width:10px' rowspan='1'>" + facultyIndexnotEqualtoZeroAdmittedIntakeIndex + "</td>";
                            //(facultyCounts.Where(e => e.approvedIntake1 != 0).Select(e => e).ToList().IndexOf(item) + 1) 
                            facultyAdmittedIntakeZero += "<td class='col2' style='text-align: left; vertical-align: top;'>" + item.Department + "</td>";
                            facultyAdmittedIntakeZero += "<td class='col2' style='text-align: left; vertical-align: top;'>" + item.Degree + "</td>";
                            facultyAdmittedIntakeZero += "<td class='col2' style='text-align: left; vertical-align: top;'>" + item.Specialization + "</td>";









                            if (departments.Contains(item.Department))
                            {
                                // faculty += "<td class='col2' style='text-align: center; vertical-align: top;'> </td>";
                                facultyAdmittedIntakeZero += "<td class='col2' style='text-align: center; vertical-align: top;'> </td>";
                                //faculty += "<td class='col2' style='text-align: center; vertical-align: top;'> </td>";
                                facultyAdmittedIntakeZero += "<td class='col2' style='text-align: center; vertical-align: top;'> </td>";
                                facultyAdmittedIntakeZero += "<td class='col2' style='text-align: center; vertical-align: top;'>" + totalBtechFirstYearIntake + "</td>";


                                if (deptloop == 1)
                                    facultyAdmittedIntakeZero += "<td rowspan='" + TotalCount + "'  class='col2' style='text-align: center; vertical-align: top;'>" + item.totalFaculty + "</td>";
                                //faculty += "<td class='col2' style='text-align: left; vertical-align: top;'>" + item.A416TotalFaculty + "</td>";
                                if (OthersSpecIds.Contains(item.specializationId))
                                {
                                    facultyAdmittedIntakeZero += "<td class='col2' style='text-align: center; vertical-align: top;'>" + othersRequiredfaculty + "</td>";
                                    HumantitiesminimamRequireMet += item.totalFaculty;
                                }
                                else
                                {
                                    facultyAdmittedIntakeZero +=
                                        "<td class='col2' style='text-align: center; vertical-align: top;'>" + Math.Ceiling(firstYearRequired) + "</td>";
                                    HumantitiesminimamRequireMet += item.totalFaculty;
                                }

                                //
                                if (minimumRequirementMet != "YES")
                                {
                                    HumantitiesminimamRequireMetStatus = "NO";
                                }
                            }
                            else
                            {
                                //faculty += "<td class='col2' style='text-align: center; vertical-align: top;font-weight: bold'>" + item.AffliationStatus + "</td>";


                                if (item.Degree == "B.Tech")
                                {
                                    facultyAdmittedIntakeZero += "<td class='col2' style='text-align: center; vertical-align: top;'>" + item.totalAdmittedIntake + "</td>";

                                    facultyAdmittedIntakeZero += "<td class='col2' style='text-align: center; vertical-align: top;'>";
                                    facultyAdmittedIntakeZero += "<table width='100%' border='1'  cellspacing='0' style='border-color: #ccc;'>";
                                    if (item.approvedIntake2 == 0)
                                        facultyAdmittedIntakeZero += "<tr><td><span>" + item.approvedIntake2 + "(II Year)</span></td></tr>";
                                    if (item.approvedIntake3 == 0)
                                        facultyAdmittedIntakeZero += "<tr><td><span>" + item.approvedIntake3 + "(III Year)</span></td></tr>";
                                    if (item.approvedIntake4 == 0)
                                        facultyAdmittedIntakeZero += "<tr><td><span>" + item.approvedIntake4 + "(IV Year)</span></td></tr>";
                                    facultyAdmittedIntakeZero += "</table></td>";
                                }
                                else
                                {
                                    facultyAdmittedIntakeZero += "<td class='col2' style='text-align: center; vertical-align: top;'>" + item.totalIntake + "</td>";
                                    facultyAdmittedIntakeZero += "<td class='col2' style='text-align: center; vertical-align: top;'>" + item.totalIntake + " </td>";
                                }
                                //faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + item.totalIntake + "</td>;
                                facultyAdmittedIntakeZero += "<td class='col2' style='text-align: center; vertical-align: top;'>" + item.approvedIntake1 + "</td>";
                                // if (deptloop == 1)
                                //  facultyAdmittedIntakeZero += "<td rowspan='" + TotalCount + "'  class='col2' style='text-align: center; vertical-align: top;'>" + item.totalFaculty + "</td>";
                                //faculty += "<td class='col2' style='text-align: left; vertical-align: top;'>" + item.A416TotalFaculty + "</td>";
                                //  facultyAdmittedIntakeZero += "<td class='col2' style='text-align: center; vertical-align: top;'>" + Math.Ceiling(item.requiredFaculty) + "</td>";
                            }
                            facultycount = facultycount + item.specializationWiseFaculty;
                            if (adjustedFaculty > 0)
                                adjustedFaculty = adjustedFaculty;
                            else
                                adjustedFaculty = 0;

                            if (adjustedPHDFaculty > 0)
                                adjustedPHDFaculty = adjustedPHDFaculty;
                            else
                                adjustedPHDFaculty = 0;

                            // facultyAdmittedIntakeZero += "<td class='col2' style='text-align: center; vertical-align: top;'>" + adjustedFaculty + "</td>";


                            //if (item.Degree == "M.Tech")
                            //    facultyAdmittedIntakeZero += "<td class='col2' style='text-align: center; vertical-align: top;'>" + item.SpecializationspgFaculty + "</td>";
                            //else
                            //    facultyAdmittedIntakeZero += "<td class='col2' style='text-align: center; vertical-align: top;'>" + "" + "</td>";
                            //if (item.AffiliationStatus2 == true)
                            //  facultyAdmittedIntakeZero +="<td class='col2' style='text-align: center; vertical-align: top;'>Yes</td>";
                            //else
                            //    facultyAdmittedIntakeZero +="<td class='col2' style='text-align: center; vertical-align: top;'>" + "No" +"</td>";


                            if (minimumRequirementMet == "YES" && PhdminimumRequirementMet == "YES")
                            {
                                if (rFaculty <= adjustedFaculty)
                                    minimumRequirementMet = "No Deficiency In faculty";
                                else
                                    minimumRequirementMet = "Deficiency In faculty";

                                if (SpecializationwisePHDFaculty <= adjustedPHDFaculty)
                                    minimumRequirementMet += " and No Deficiency In Phd";
                                else
                                    minimumRequirementMet += " and Deficiency In Phd";

                            }

                            else if (minimumRequirementMet == "NO" && PhdminimumRequirementMet == "YES")
                            {
                                if (rFaculty == adjustedFaculty)
                                    minimumRequirementMet = "No Deficiency In faculty";
                                else
                                    minimumRequirementMet = "Deficiency In faculty";


                                if (SpecializationwisePHDFaculty <= adjustedPHDFaculty)
                                    minimumRequirementMet += " and No Deficiency In Phd";
                                else
                                    minimumRequirementMet += " and Deficiency In Phd";
                            }
                            else if (minimumRequirementMet == "YES" && PhdminimumRequirementMet == "NO")
                            {
                                if (rFaculty <= adjustedFaculty)
                                    minimumRequirementMet = "No Deficiency In faculty";
                                else
                                    minimumRequirementMet = "Deficiency In faculty";

                                if (SpecializationwisePHDFaculty == adjustedPHDFaculty)
                                    minimumRequirementMet += " and No Deficiency In Phd";
                                else
                                    minimumRequirementMet += " and Deficiency In Phd";

                            }

                            else if (minimumRequirementMet == "NO" && PhdminimumRequirementMet == "NO")
                            {
                                if (rFaculty == adjustedFaculty)
                                    minimumRequirementMet = "No Deficiency In faculty";
                                else
                                    minimumRequirementMet = "Deficiency In faculty";


                                if (SpecializationwisePHDFaculty == adjustedPHDFaculty)
                                    minimumRequirementMet += " and No Deficiency In Phd";
                                else
                                    minimumRequirementMet += " and Deficiency In Phd";
                            }






                            // facultyAdmittedIntakeZero += "<td class='col2' style='text-align: center; vertical-align: top;'>" + SpecializationwisePHDFaculty + "</td>";
                            //if (item.AffiliationStatus3 == true)
                            //    facultyAdmittedIntakeZero +="<td class='col2' style='text-align: center; vertical-align: top;'>Yes</td>";
                            //else
                            //    facultyAdmittedIntakeZero +="<td class='col2' style='text-align: center; vertical-align: top;'>" + "No" +"</td>";

                            // facultyAdmittedIntakeZero += "<td class='col2' style='text-align: center; vertical-align: top;'>" + adjustedPHDFaculty + "</td>";
                            //if (item.AffiliationStatus4 == true)
                            //    facultyAdmittedIntakeZero +="<td class='col2' style='text-align: center; vertical-align: top;'>Yes</td>";
                            //else
                            //    facultyAdmittedIntakeZero +="<td class='col2' style='text-align: center; vertical-align: top;'>" + "No" +"</td>";
                            //  facultyAdmittedIntakeZero += "<td class='col2' style='text-align: center; vertical-align: top;'>" + minimumRequirementMet + "</td>";


                            facultyAdmittedIntakeZero += "</tr>";
                            //  facultyAdmittedIntakeZeroTable2 += "</tr>";
                            deptloop++;
                            SpecializationwisePHDFaculty = 0;


                            #endregion

                            // }

                        }

                        facultyAdmittedIntakeZero += "</table>";
                        // faculty += facultyAdmittedIntakeZero;
                        //  facultyIndexnotEqualtoZeroAdmittedIntakeIndex++;
                    }
                }
            }




            //if (ProposedIntakeZeroDeptName.Count() != 0)
            //{

            //}

            if (collegeStatus != null)
            {
                if (collegeStatus.CollegeStatus == true)
                {
                    faculty += "<p>Under these circumstances the SCA recommends for <b><u>'No Admission status'</u></b> for the A.Y. 2017-18 for the Institute. Clarification or explanation if any may be submitted within 10 days from the date of this letter.</p>";
                    faculty += "<table width='100%'  cellspacing='0'>";
                    faculty += "<tr><td align='right' width='80%' style='font-size: 14px; font-weight: normal;'> <b>Sd /-</b></td></tr>";
                    faculty += "<tr><td align='right' width='80%' style='font-size: 14px; font-weight: normal;'> <b>REGISTRAR</b></td></tr>";
                    faculty += "</table>";
                }
            }






            var collegeFacultycount = 0;
            string[] collegefacultyRegistrationNo = db.jntuh_college_faculty_registered.AsNoTracking().Where(e => e.collegeId == collegeID).Select(e => e.RegistrationNumber).ToArray();

            var jntuh_college_faculty_registered_new = db.jntuh_college_principal_registered.Where(cf => cf.collegeId == collegeID).ToList();
            var principalRegno = jntuh_college_faculty_registered_new.Select(cf => cf.RegistrationNumber).FirstOrDefault();

            var registeredFaculty = db.jntuh_registered_faculty.Where(rf => collegefacultyRegistrationNo.Contains(rf.RegistrationNumber.Trim())).ToList();

            var jntuh_registered_faculty1 =
                    registeredFaculty.Where(rf => rf.type != "Adjunct" && ((rf.Absent == false) && (rf.OriginalCertificatesNotShown == false) && (rf.Xeroxcopyofcertificates == false || rf.Xeroxcopyofcertificates == null) && (rf.NotQualifiedAsperAICTE == false || rf.NotQualifiedAsperAICTE == null)//&& rf.Noform16Verification == false && rf.NoForm16 == false
                                                        && (rf.NoSCM == false || rf.NoSCM == null) && (rf.PANNumber != null) && (rf.IncompleteCertificates == false || rf.IncompleteCertificates == null) && (rf.Blacklistfaculy == false) && rf.NoRelevantUG != "Yes" && rf.NoRelevantPG != "Yes" && rf.NORelevantPHD != "Yes" && rf.NotIdentityfiedForanyProgram == false && (rf.InvalidPANNumber == false || rf.InvalidPANNumber == null) && (rf.Invaliddegree == false || rf.Invaliddegree == null) && (rf.OriginalsVerifiedPHD != true) && (rf.OriginalsVerifiedUG != true) && (rf.BAS != "Yes" && rf.InvalidAadhaar != "Yes")) && (rf.DepartmentId != 61 || rf.DepartmentId != 27))

                             .Select(rf => new
                             {
                                 //Departmentid = rf.DepartmentId,
                                 RegistrationNumber = rf.RegistrationNumber,
                                 // Department = rf.jntuh_department.departmentName,
                                 HighestDegreeID = rf.jntuh_registered_faculty_education.Count() != 0 ? rf.jntuh_registered_faculty_education.Select(e => e.educationId).Max() : 0,
                                 IsApproved = rf.isApproved,
                                 PanNumber = rf.PANNumber,
                                 AadhaarNumber = rf.AadhaarNumber,
                                 NoForm16 = rf.NoForm16,
                                 TotalExperience = rf.TotalExperience
                             }).ToList();
            jntuh_registered_faculty1 = jntuh_registered_faculty1.Where(e => e.HighestDegreeID >= 4).ToList();

            collegeFacultycount = jntuh_registered_faculty1.Count;



            var lastyearfacultycount = db.jntuh_notin415faculty.Where(i => i.CollegeId == collegeID).Select(i => i).FirstOrDefault();



            int count1 = 0;
            //Commented on 18-06-2018 by Narayana Reddy
            //var nodocumentsdetails = db.jntuh_deficiencyrepoprt_college_pendingdocuments.Where(i => i.CollegeId == collegeID).Select(i => i).FirstOrDefault();
            var TotalAictecount = db.jntuh_college_aictefaculty.Where(e => e.CollegeId == collegeID).ToList().Count();
            string[] regnos = db.jntuh_college_faculty_registered.Where(c => c.collegeId == collegeID).Select(s => s.RegistrationNumber).Distinct().ToArray();
            var ToatalFacultyon418 =
                db.jntuh_registered_faculty.Where(r => regnos.Contains(r.RegistrationNumber) && r.Absent == false).ToList().Distinct().Count();
            var AffiliationFee = db.jntuh_college_paymentoffee.Where(e => e.collegeId == collegeID && e.FeeTypeID == 5).Select(e => e).FirstOrDefault();
            var CommanserviceFee = db.jntuh_college_paymentoffee.Where(e => e.collegeId == collegeID && e.FeeTypeID == 3).Select(e => e).FirstOrDefault();
            if (collegeStatus != null)
            {
                if (collegeStatus.CollegeStatus == false && collegeStatus.AICTEStatus == false)
                {
                    //PENDING SUPPORTING DOCUMENTS FOR ONLINE  DATA:


                    List<int?> NoBAS = new List<int?> { 2, 59, 247, 415, 419, 239, 447 };



                    #region PENDING SUPPORTING DOCUMENTS FOR ONLINE  DATA:

                    //faculty += "<br/><table><tr><td align='left'><b><u>Pending Issues:</u></b></td>";
                    //faculty += "</tr></table>";
                    //faculty += "<ul style='list-style-type:disc'>";
                    //if (NoBAS.Contains(collegeID))
                    //{
                    //    faculty += "<li>Biometric Attendance System not implemented.</li>";
                    //}

                    //if (AffiliationFee != null)
                    //{
                    //    if (CommanserviceFee.paidAmount != null)
                    //    {
                    //        faculty += "<li>Common Service Fee Due:<b> Rs." + CommanserviceFee.paidAmount + "</b></li>";
                    //    }

                    //    if (AffiliationFee.duesAmount != null)
                    //    {
                    //        faculty += "<li>Affiliation Fee Due: <b>Rs." + AffiliationFee.duesAmount + "</b></li>";
                    //    }
                    //}



                    //faculty += "</ul>";

                    #endregion


                    #region OTHER OBSERVATIONS/  REMARKS

                    int Count2 = 0;

                    //faculty += "<table><tr><td align='left'><b><u>Other Observations/ Remarks:</u></b></td>";
                    //faculty += "</tr></table>";
                    //faculty += "<ul style='list-style-type:disc'>";

                    //if (TotalAictecount != null && TotalAictecount != 0)
                    //{

                    //    faculty += "<li>Total faculty uploaded to AICTE for Extension of Approval for AY 2018-19 &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; : " + TotalAictecount + ".</li>";//facultyCounts.Select(i => i.specializationWiseFaculty).Sum()
                    //    faculty += "<li>Total faculty available during inspection with  qualifications as prescribed by AICTE &nbsp;&nbsp;&nbsp;:<b> " + collegeFacultycount + "</b>.</li>";
                    //}
                    //else
                    //{
                    //    faculty += "<li>Total faculty uploaded to AICTE for Extension of Approval for AY 2018-19 &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; : 0.</li>";
                    //    faculty += "<li>Total faculty available during inspection with  qualifications as prescribed by AICTE &nbsp;&nbsp;&nbsp;:<b> " + collegeFacultycount + "</b>.</li>";

                    //}
                    //if (lastyearfacultycount != null)
                    //{

                    //    faculty += "<li>Number of faculty recruited after the last inspection  &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; : " + lastyearfacultycount.RegistrationNumber + ".</li>";
                    //}
                    //else
                    //{

                    //    faculty += "<li>Number of faculty recruited after the last inspection &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;   :0. </li>";
                    //    //Total Available Faculty is " + collegeFacultycount + ".
                    //}




                    //int currentyearfaculty = 0;
                    //if (nodocumentsdetails != null && nodocumentsdetails.Currentyearfaculty != 0)
                    //{
                    //    //   currentyearfaculty = (int)nodocumentsdetails.Currentyearfaculty;
                    //}





                    //faculty += "</ul>";

                    #endregion

                }
            }



            return faculty;
        }

        public List<CollegeFacultyWithIntakeReport> collegeInactiveFaculty(int? collegeId)
        {
            List<CollegeIntakeExisting> collegeIntakeExisting = new List<CollegeIntakeExisting>();
            List<CollegeFacultyWithIntakeReport> intakedetailsList = new List<CollegeFacultyWithIntakeReport>();
            var jntuh_departments = db.jntuh_department.ToList();
            int studentcount = Convert.ToInt32(ConfigurationManager.AppSettings["studentcount"]); ;
            if (collegeId != null)
            {
                var jntuh_college_faculty_deficiency = db.jntuh_college_faculty_deficiency.Where(fd => fd.CollegeId == collegeId).ToList();
                var jntuh_specialization = db.jntuh_specialization.ToList();
                string CollegeAffiliationStatus = db.jntuh_college_affiliation.Where(u => u.collegeId == collegeId && u.affiliationTypeId == 7).Select(u => u.affiliationStatus).FirstOrDefault();
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
              
                var jntuh_college = db.jntuh_college.Where(c => c.isActive == true).ToList();
                var jntuh_faculty_student_ratio_norms =
                    db.jntuh_faculty_student_ratio_norms.Where(f => f.isActive == true).ToList();
                var jntuh_degree = db.jntuh_degree.ToList();
               
                var jntuh_academic_year = db.jntuh_academic_year.ToList();

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
                var collegedepts = collegeIntakeExisting.Select(i => i.DepartmentID).Distinct().ToList();

                collegeIntakeExisting = collegeIntakeExisting.AsEnumerable().GroupBy(r => new { r.specializationId, r.shiftId, r.collegeId }).Select(r => r.First()).ToList();


                var DeptNameBasedOnSpecialization = (from a in db.jntuh_department
                                                     join b in db.jntuh_specialization on a.id equals b.departmentId
                                                     select new
                                                     {
                                                         DeptId = a.id,
                                                         DeptName = a.departmentName,
                                                         Specid = b.id
                                                     }).ToList();





                //college Reg nos
                var jntuh_college_faculty_registered = db.jntuh_college_faculty_registered.Where(cf => cf.collegeId == collegeId && cf.DepartmentId != null).ToList();
                string[] strRegnos = jntuh_college_faculty_registered.Select(cf => cf.RegistrationNumber.Trim()).ToArray();


                var jntuh_college_faculty_registered_new = db.jntuh_college_principal_registered.Where(cf => cf.collegeId == collegeId).ToList();
                var principalRegno = jntuh_college_faculty_registered_new.Select(cf => cf.RegistrationNumber).FirstOrDefault();

                //education categoryIds UG,PG,PHD...........

                int pharmacyDeptId = db.jntuh_department.Where(d => d.departmentName == "Pharmacy").Select(d => d.id).FirstOrDefault();

                var jntuh_education_category = db.jntuh_education_category.ToList();

                var registeredFaculty = db.jntuh_registered_faculty.Where(rf => strRegnos.Contains(rf.RegistrationNumber.Trim())).ToList();
                var collegeaffliations = db.jntuh_college_affliationstatus.AsNoTracking().ToList();
                var regfacultywithdepts = registeredFaculty.Where(rf => rf.DepartmentId == null).ToList();
                //02-04-2018 BASStatus Columns consider as-AadhaarFlag;BASStatusOld column Consider as -Basflag
                //var jntuh_registered_faculty1 =
                //    registeredFaculty.Where(rf => rf.type != "Adjunct" && ((rf.Absent == false) && (rf.OriginalCertificatesNotShown == false) && (rf.Xeroxcopyofcertificates == false || rf.Xeroxcopyofcertificates == null) && (rf.NotQualifiedAsperAICTE == false || rf.NotQualifiedAsperAICTE == null)//&& rf.Noform16Verification == false && rf.NoForm16 == false
                //                                        && (rf.NoSCM == false || rf.NoSCM == null) && (rf.PANNumber != null) && (rf.IncompleteCertificates == false || rf.IncompleteCertificates == null) && (rf.Blacklistfaculy == false) && rf.NoRelevantUG != "Yes" && rf.NoRelevantPG != "Yes" && rf.NORelevantPHD != "Yes" && rf.NotIdentityfiedForanyProgram == false && (rf.InvalidPANNumber == false || rf.InvalidPANNumber == null) && (rf.OriginalsVerifiedPHD != true) && (rf.OriginalsVerifiedUG != true) && (rf.Invaliddegree == false || rf.Invaliddegree == null) && rf.BAS != "Yes") && rf.InvalidAadhaar != "Yes" && (rf.DepartmentId != 61 || rf.DepartmentId != 27)).Select(rf => new
                //                                        {
                //                                            //Departmentid = rf.DepartmentId,
                //                                            RegistrationNumber = rf.RegistrationNumber.Trim(),
                //                                            // Department = rf.jntuh_department.departmentName,
                //                                            DeptId = jntuh_college_faculty_registered.Where(c => c.RegistrationNumber.Trim() == rf.RegistrationNumber.Trim()).Select(c => c.DepartmentId).FirstOrDefault(),
                //                                            SpecializationId = jntuh_college_faculty_registered.Where(c => c.RegistrationNumber.Trim() == rf.RegistrationNumber.Trim()).Select(c => c.SpecializationId).FirstOrDefault(),
                //                                            HighestDegreeID = rf.jntuh_registered_faculty_education.Count() != 0 ? rf.jntuh_registered_faculty_education.Select(e => e.educationId).Max() : 0,
                //                                            IsApproved = rf.isApproved,
                //                                            PanNumber = rf.PANNumber,
                //                                            AadhaarNumber = rf.AadhaarNumber,
                //                                            NoForm16 = rf.NoForm16,
                //                                            TotalExperience = rf.TotalExperience
                //                                        }).ToList();

                var jntuh_registered_faculty1 =
                   registeredFaculty.Where(rf => rf.type != "Adjunct" && ((rf.Absent == false || rf.Absent == null) && (rf.AbsentforVerification == false || rf.AbsentforVerification == null) && (rf.OriginalCertificatesNotShown == false || rf.OriginalCertificatesNotShown == null) && (rf.Xeroxcopyofcertificates == false || rf.Xeroxcopyofcertificates == null) && (rf.NotQualifiedAsperAICTE == false || rf.NotQualifiedAsperAICTE == null)//&& rf.Noform16Verification == false && rf.NoForm16 == false
                                                       && (rf.Noclass == false || rf.Noclass == null) && (rf.FakePHD == false || rf.FakePHD == null) && (rf.Genuinenessnotsubmitted == false || rf.Genuinenessnotsubmitted == null) && (rf.NoSCM == false || rf.NoSCM == null) && (rf.PANNumber != null) && (rf.IncompleteCertificates == false || rf.IncompleteCertificates == null) && (rf.Blacklistfaculy == false) && rf.NoRelevantUG != "Yes" && rf.NoRelevantPG != "Yes" && rf.NORelevantPHD != "Yes" && (rf.InvalidPANNumber == false || rf.InvalidPANNumber == null) && (rf.OriginalsVerifiedPHD != true) && (rf.OriginalsVerifiedUG != true) && (rf.Invaliddegree == false || rf.Invaliddegree == null) && rf.InvalidAadhaar != "Yes") && rf.BAS != "Yes" && (rf.DepartmentId != 61 || rf.DepartmentId != 27)).Select(rf => new
                                                       {
                                                           //Departmentid = rf.DepartmentId,
                                                           RegistrationNumber = rf.RegistrationNumber.Trim(),
                                                           // Department = rf.jntuh_department.departmentName,
                                                           DeptId = jntuh_college_faculty_registered.Where(c => c.RegistrationNumber.Trim() == rf.RegistrationNumber.Trim()).Select(c => c.DepartmentId).FirstOrDefault(),
                                                           SpecializationId = jntuh_college_faculty_registered.Where(c => c.RegistrationNumber.Trim() == rf.RegistrationNumber.Trim()).Select(c => c.SpecializationId).FirstOrDefault(),
                                                           NotconsideredPHD = rf.NotconsideredPHD,
                                                           // HighestDegreeID = rf.jntuh_registered_faculty_education.Count() != 0 ? rf.jntuh_registered_faculty_education.Where(e => e.educationId != 8).Select(e => e.educationId).Max() : 0,
                                                           HighestDegreeID = rf.NotconsideredPHD == true ? rf.jntuh_registered_faculty_education.Where(r => r.educationId != 8).Count() != 0 ? rf.jntuh_registered_faculty_education.Where(e => e.educationId != 8 && e.educationId != 6).Select(e => e.educationId).Max() : 0 : rf.jntuh_registered_faculty_education.Where(r => r.educationId != 8).Count() != 0 ? rf.jntuh_registered_faculty_education.Where(e => e.educationId != 8).Select(e => e.educationId).Max() : 0,
                                                           IsApproved = rf.isApproved,
                                                           Createdon = jntuh_college_faculty_registered.Where(c => c.RegistrationNumber.Trim() == rf.RegistrationNumber.Trim()).Select(c => c.createdOn).FirstOrDefault(),
                                                           PanNumber = rf.PANNumber,
                                                           AadhaarNumber = rf.AadhaarNumber,
                                                           NoForm16 = rf.NoForm16,
                                                           TotalExperience = rf.TotalExperience

                                                       }).ToList();
                //BAS Flag
                int variable = 2;

                //foreach (var bas in jntuh_registered_faculty1)
                //{
                //    List<jntuh_college_basreport> jntuh_college_basreport =
                //        db.jntuh_college_basreport.Where(a => a.RegistrationNumber == bas.RegistrationNumber && a.collegeId == collegeId)
                //            .Select(s => s)
                //            .ToList();
                //    string[] months = { "July", "August", "September", "October", "November", "December", "January","February"};
                //    foreach (var month in months)
                //    {
                //        int totalworkingdays = (int)jntuh_college_basreport.Where(m => m.month == month).Select(s=>s.totalworkingDays).FirstOrDefault();
                //        int presentdays = (int)jntuh_college_basreport.Where(m => m.month == month).Select(s => s.NoofPresentDays).FirstOrDefault();
                //        int requiedpresentdays = totalworkingdays - variable;
                //    }

                //}
                //var jntuh_registered_faculty1 =
                //    registeredFaculty.Where(rf => rf.type != "Adjunct" && ((rf.Absent == false) && (rf.NotQualifiedAsperAICTE == false)//&& rf.Noform16Verification == false && rf.NoForm16 == false
                //                                        && (rf.NoSCM17 == false) && (rf.PANNumber != null) && (rf.IncompleteCertificates == false) && (rf.Blacklistfaculy == false) && (rf.MultipleRegInSameCollege == false || rf.MultipleRegInSameCollege == null) && rf.NoRelevantUG == "No" && rf.NoRelevantPG == "No" && rf.NORelevantPHD == "No" && rf.NotIdentityfiedForanyProgram == false && (rf.InvalidPANNumber == false || rf.InvalidPANNumber == null) && (rf.PhdUndertakingDocumentstatus == true || rf.PhdUndertakingDocumentstatus == null) && (rf.AppliedPAN == false || rf.AppliedPAN == null) && (rf.SamePANUsedByMultipleFaculty == false || rf.SamePANUsedByMultipleFaculty == null) && (rf.BASStatusOld == "Y")) && (rf.DepartmentId != 61 || rf.DepartmentId != 27)).Select(rf => new
                //                                          {
                //                                              //Departmentid = rf.DepartmentId,
                //                                              RegistrationNumber = rf.RegistrationNumber.Trim(),
                //                                              // Department = rf.jntuh_department.departmentName,
                //                                              DeptId = jntuh_college_faculty_registered.Where(c => c.RegistrationNumber.Trim() == rf.RegistrationNumber.Trim()).Select(c => c.DepartmentId).FirstOrDefault(),
                //                                              SpecializationId = jntuh_college_faculty_registered.Where(c => c.RegistrationNumber.Trim() == rf.RegistrationNumber.Trim()).Select(c => c.SpecializationId).FirstOrDefault(),
                //                                              HighestDegreeID = rf.jntuh_registered_faculty_education.Count() != 0 ? rf.jntuh_registered_faculty_education.Select(e => e.educationId).Max() : 0,
                //                                              IsApproved = rf.isApproved,
                //                                              PanNumber = rf.PANNumber,
                //                                              AadhaarNumber = rf.AadhaarNumber,
                //                                              NoForm16 = rf.NoForm16,
                //                                              TotalExperience = rf.TotalExperience
                //                                          }).ToList();
                // var reg11 = registeredFaculty.Where(f => f.RegistrationNumber.Trim() == "9251-150414-062519").ToList();
                var nohighestdegree = jntuh_registered_faculty1.Where(e => e.HighestDegreeID < 4).ToList();
                jntuh_registered_faculty1 = jntuh_registered_faculty1.Where(e => e.HighestDegreeID >= 4).ToList();
                int fcount = jntuh_registered_faculty1.Count();
                var jntuh_registered_faculty = jntuh_registered_faculty1.Select(rf => new
                {
                    RegistrationNumber = rf.RegistrationNumber,
                    Department = rf.DeptId != null ? jntuh_departments.Where(e => e.id == rf.DeptId).Select(e => e.departmentName).FirstOrDefault() : DeptNameBasedOnSpecialization.Where(e => e.Specid == rf.SpecializationId).Select(e => e.DeptName).FirstOrDefault(),
                    HighestDegree = jntuh_education_category.Where(c => c.id == rf.HighestDegreeID).Select(c => c.educationCategoryName).FirstOrDefault(),
                    Recruitedfor = jntuh_college_faculty_registered.Where(c => c.RegistrationNumber.Trim() == rf.RegistrationNumber.Trim()).Select(c => c.IdentifiedFor).FirstOrDefault(),
                    rf.SpecializationId,
                    // jntuh_college_faculty_registered.Where(c => c.RegistrationNumber.Trim() == rf.RegistrationNumber.Trim()).Select(c => c.SpecializationId).FirstOrDefault(),
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
                    intakedetails.ispercentage = false;
                    intakedetails.isstarcourses = false;
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

                    //intakedetails.approvedIntake1 = GetIntake(item.collegeId, AY1, item.specializationId, item.shiftId, 1, item.degreeID);
                    //intakedetails.approvedIntake2 = GetIntake(item.collegeId, AY2, item.specializationId, item.shiftId, 1, item.degreeID);
                    //intakedetails.approvedIntake3 = GetIntake(item.collegeId, AY3, item.specializationId, item.shiftId, 1, item.degreeID);
                    //intakedetails.approvedIntake4 = GetIntake(item.collegeId, AY4, item.specializationId, item.shiftId, 1, item.degreeID);
                    //intakedetails.approvedIntake5 = GetIntake(item.collegeId, AY5, item.specializationId, item.shiftId, 1, item.degreeID);

                    //Written by Narayan reddy Admitted Intake Flag=1 and Getting AICTE Sanctioned Intake flag=2,JNTU Sanctioned Intake=0,Exam Branch Intake=3.
                    intakedetails.Proposedintake = GetIntake(item.collegeId, AY1, item.specializationId, item.shiftId, 1, item.degreeID);
                    intakedetails.admittedIntake1 = GetIntake(item.collegeId, AY1, item.specializationId, item.shiftId, 1, item.degreeID);
                    intakedetails.admittedIntake2 = GetIntake(item.collegeId, AY2, item.specializationId, item.shiftId, 1, item.degreeID);
                    intakedetails.admittedIntake3 = GetIntake(item.collegeId, AY3, item.specializationId, item.shiftId, 1, item.degreeID);
                    intakedetails.admittedIntake4 = GetIntake(item.collegeId, AY4, item.specializationId, item.shiftId, 1, item.degreeID);
                    intakedetails.admittedIntake5 = GetIntake(item.collegeId, AY5, item.specializationId, item.shiftId, 1, item.degreeID);

                    intakedetails.AICTESanctionIntake1 = GetIntake(item.collegeId, AY1, item.specializationId, item.shiftId, 2, item.degreeID);
                    intakedetails.AICTESanctionIntake2 = GetIntake(item.collegeId, AY2, item.specializationId, item.shiftId, 2, item.degreeID);
                    intakedetails.AICTESanctionIntake3 = GetIntake(item.collegeId, AY3, item.specializationId, item.shiftId, 2, item.degreeID);
                    intakedetails.AICTESanctionIntake4 = GetIntake(item.collegeId, AY4, item.specializationId, item.shiftId, 2, item.degreeID);
                    intakedetails.AICTESanctionIntake5 = GetIntake(item.collegeId, AY5, item.specializationId, item.shiftId, 2, item.degreeID);

                    //Getting exmationation Brach intake 
                    intakedetails.ExambranchIntake_R1 = GetIntake(item.collegeId, AY1, item.specializationId, item.shiftId, 3, item.degreeID);
                    intakedetails.ExambranchIntake_R2 = GetIntake(item.collegeId, AY2, item.specializationId, item.shiftId, 3, item.degreeID);
                    intakedetails.ExambranchIntake_R3 = GetIntake(item.collegeId, AY3, item.specializationId, item.shiftId, 3, item.degreeID);
                    intakedetails.ExambranchIntake_R4 = GetIntake(item.collegeId, AY4, item.specializationId, item.shiftId, 3, item.degreeID);
                    intakedetails.ExambranchIntake_R5 = GetIntake(item.collegeId, AY5, item.specializationId, item.shiftId, 3, item.degreeID);


                    intakedetails.AffiliationStatus2 = GetAcademicYear(item.collegeId, AY1, item.specializationId, item.shiftId, item.degreeID);
                    intakedetails.AffiliationStatus3 = GetAcademicYear(item.collegeId, AY2, item.specializationId, item.shiftId, item.degreeID);
                    intakedetails.AffiliationStatus4 = GetAcademicYear(item.collegeId, AY3, item.specializationId, item.shiftId, item.degreeID);

                    //Get sanction Inake for Btech
                    if (item.degreeID == 4)
                    {
                        intakedetails.SanctionIntake1 = GetIntake(item.collegeId, AY1, item.specializationId, item.shiftId, 0, item.degreeID);
                        intakedetails.SanctionIntake2 = GetIntake(item.collegeId, AY2, item.specializationId, item.shiftId, 0, item.degreeID);
                        intakedetails.SanctionIntake3 = GetIntake(item.collegeId, AY3, item.specializationId, item.shiftId, 0, item.degreeID);
                        intakedetails.SanctionIntake4 = GetIntake(item.collegeId, AY4, item.specializationId, item.shiftId, 0, item.degreeID);
                        intakedetails.SanctionIntake5 = GetIntake(item.collegeId, AY5, item.specializationId, item.shiftId, 0, item.degreeID);
                    }


                    facultystudentRatio = Convert.ToInt32(jntuh_faculty_student_ratio_norms.Where(fn => fn.degreeId == item.degreeID).Select(fn => fn.Norms).FirstOrDefault().Split(':')[1].ToString());

                    var csedept = jntuh_registered_faculty.Where(i => i.Department == item.Department).ToList();
                    intakedetails.form16count = form16Count.Count > 0 ? form16Count.Count(i => i.DepartmentId == item.DepartmentID) : 0;
                    intakedetails.aictecount = aictecount.Count > 0 ? aictecount.Count(i => i.DepartmentId == item.DepartmentID) : 0;


                    if (item.Degree == "B.Tech")
                    {
                        //25% Calculation Based on Exam Branch Intake Regular intakes from 21-04-2019 on 419
                        int senondyearpercentage = 0;
                        int thirdyearpercentage = 0;
                        int fourthyearpercentage = 0;
                        if (CollegeAffiliationStatus == "Yes")
                        {
                            intakedetails.ispercentage = false;
                        }
                        else
                        {
                            if (intakedetails.SanctionIntake2 != 0)
                            {
                                senondyearpercentage = (int)Math.Ceiling(Convert.ToDecimal((Convert.ToDecimal(intakedetails.ExambranchIntake_R2) / Convert.ToDecimal(intakedetails.SanctionIntake2)) * 100));
                            }
                            if (intakedetails.SanctionIntake3 != 0)
                            {
                                thirdyearpercentage = (int)Math.Ceiling(Convert.ToDecimal((Convert.ToDecimal(intakedetails.ExambranchIntake_R3) / Convert.ToDecimal(intakedetails.SanctionIntake3)) * 100));
                            }
                            if (intakedetails.SanctionIntake4 != 0)
                            {
                                fourthyearpercentage = (int)Math.Ceiling(Convert.ToDecimal((Convert.ToDecimal(intakedetails.ExambranchIntake_R4) / Convert.ToDecimal(intakedetails.SanctionIntake4)) * 100));
                            }

                            if (senondyearpercentage < 25 && thirdyearpercentage < 25 && fourthyearpercentage < 25)
                            {
                                intakedetails.ispercentage = true;
                                //studentcount
                                if ((intakedetails.ExambranchIntake_R2 >= studentcount || intakedetails.ExambranchIntake_R3 >= studentcount || intakedetails.ExambranchIntake_R4 >= studentcount) && intakedetails.Proposedintake != 0)
                                {
                                    intakedetails.ispercentage = false;
                                    //intakedetails.isstarcourses = true;
                                    //intakedetails.ReducedInatke = 60;
                                    //if (intakedetails.approvedIntake1 != 60)
                                    //{
                                    //    intakedetails.Note = "(Reduced from Proposed Intake of <b>";
                                    //    intakedetails.Note += intakedetails.approvedIntake1;
                                    //    intakedetails.Note += intakedetails.approvedIntake1;
                                    //    intakedetails.Note += "</b> as per 25% Clause)";
                                    //    intakedetails.approvedIntake1 = 60;
                                    //}
                                }
                            }
                        }
                        //Requried Faculty Getting based on addmitted intake and Sancanedintake and AICTE Intake
                        int intake2 = 0;
                        int intake3 = 0;
                        int intake4 = 0;
                        int totalintake = 0;
                        //intake2 = GetBtechAdmittedIntake(intakedetails.admittedIntake2);
                        //intake3 = GetBtechAdmittedIntake(intakedetails.admittedIntake3);
                        //intake4 = GetBtechAdmittedIntake(intakedetails.admittedIntake4);
                        //totalintake = (intake2) + (intake3) +
                        //                                (intake4);

                        //AfacultyRatio = Convert.ToDecimal(totalintake) /
                        //                   Convert.ToDecimal(facultystudentRatio);

                        //Getting from web.config AICTE Sanctioned-1 or JNTU Sanctioned-2 or Admitted Intake-3
                        int takecondition = Convert.ToInt32(ConfigurationManager.AppSettings["intakecondition"]);
                        if (takecondition == 1)
                        {
                            if (item.DepartmentID == 1 && collegeId == 108)
                            {
                                intakedetails.totalIntake = GetBtechAdmittedIntake(intakedetails.AICTESanctionIntake2) + GetBtechAdmittedIntake(intakedetails.AICTESanctionIntake3);
                            }
                            else
                            {
                                intakedetails.totalIntake = GetBtechAdmittedIntake(intakedetails.AICTESanctionIntake2) + GetBtechAdmittedIntake(intakedetails.AICTESanctionIntake3) +
                                                    GetBtechAdmittedIntake(intakedetails.AICTESanctionIntake4);
                            }

                        }
                        else if (takecondition == 2)
                        {
                            if (item.DepartmentID == 1 && collegeId == 108)
                            {
                                intakedetails.totalIntake = GetBtechAdmittedIntake(intakedetails.SanctionIntake2) +
                                                        GetBtechAdmittedIntake(intakedetails.SanctionIntake3);
                            }
                            else
                            {
                                intakedetails.totalIntake = GetBtechAdmittedIntake(intakedetails.SanctionIntake2) +
                                                       GetBtechAdmittedIntake(intakedetails.SanctionIntake3) +
                                                       GetBtechAdmittedIntake(intakedetails.SanctionIntake4);
                            }

                        }
                        else
                        {

                            if (item.DepartmentID == 1 && collegeId == 108)
                            {
                                intakedetails.totalIntake = GetBtechAdmittedIntake(intakedetails.admittedIntake2) +
                                                       GetBtechAdmittedIntake(intakedetails.admittedIntake3);
                            }
                            else
                            {
                                intakedetails.totalIntake = GetBtechAdmittedIntake(intakedetails.admittedIntake2) +
                                                       GetBtechAdmittedIntake(intakedetails.admittedIntake3) +
                                                       GetBtechAdmittedIntake(intakedetails.admittedIntake4);
                            }

                        }
                        facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) /
                                           Convert.ToDecimal(facultystudentRatio);

                        intake2 = GetBtechAdmittedIntake(intakedetails.SanctionIntake2);
                        intake3 = GetBtechAdmittedIntake(intakedetails.SanctionIntake3);
                        intake4 = GetBtechAdmittedIntake(intakedetails.SanctionIntake4);
                        totalintake = (intake2) + (intake3) +
                                                        (intake4);
                    }
                    else if (item.Degree == "M.Tech" || item.Degree == "MBA")
                    {
                        //intakedetails.totalIntake = (intakedetails.approvedIntake1) + (intakedetails.approvedIntake2);
                        //facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) /
                        //               Convert.ToDecimal(facultystudentRatio);
                        //New Code Written by Narayana
                        if (item.Degree == "M.Tech" && item.shiftId == 1)
                        {
                            //intakedetails.totalIntake = (intakedetails.approvedIntake1) + (intakedetails.approvedIntake2);
                            intakedetails.totalIntake = (intakedetails.Proposedintake) + (intakedetails.AICTESanctionIntake2);
                            facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) /
                                           Convert.ToDecimal(facultystudentRatio);
                            int fratio = (int)facultyRatio;
                            if (fratio < 3)
                            {
                                fratio = 3;
                                facultyRatio = Convert.ToDecimal(fratio);
                            }
                        }
                        if (item.Degree == "M.Tech" && item.shiftId == 2)
                        {
                            //intakedetails.totalIntake = (intakedetails.approvedIntake1) + (intakedetails.approvedIntake2);
                            intakedetails.totalIntake = (intakedetails.Proposedintake) + (intakedetails.AICTESanctionIntake2);
                            facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) /
                                           Convert.ToDecimal(facultystudentRatio);
                            int fratio = (int)facultyRatio;
                            if (fratio < 3)
                            {
                                fratio = 3;
                                facultyRatio = Convert.ToDecimal(fratio);
                            }
                            facultyRatio = facultyRatio / 2;
                        }
                        if (item.Degree == "MBA")
                        {
                            //intakedetails.totalIntake = (intakedetails.approvedIntake1) + (intakedetails.approvedIntake2);
                            intakedetails.totalIntake = (intakedetails.Proposedintake) + (intakedetails.AICTESanctionIntake2);
                            facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) /
                                           Convert.ToDecimal(facultystudentRatio);
                        }


                    }
                    else if (item.Degree == "MCA")
                    {
                        //intakedetails.totalIntake = (intakedetails.approvedIntake1) + (intakedetails.approvedIntake2) +
                        //                            (intakedetails.approvedIntake3);
                        intakedetails.totalIntake = (intakedetails.Proposedintake) + (intakedetails.AICTESanctionIntake2) + (intakedetails.AICTESanctionIntake3);
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
                        //intakedetails.totalIntake = (intakedetails.approvedIntake1) + (intakedetails.approvedIntake2) +
                        //                            (intakedetails.approvedIntake3) + (intakedetails.approvedIntake4) +
                        //                            (intakedetails.approvedIntake5);
                        intakedetails.totalIntake = (intakedetails.Proposedintake) + (intakedetails.AICTESanctionIntake2) +
                                                    (intakedetails.AICTESanctionIntake3) + (intakedetails.AICTESanctionIntake4) +
                                                    (intakedetails.AICTESanctionIntake5);
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
                            intakedetails.specializationWiseFaculty = jntuh_registered_faculty.Count(f => f.Department == "Pharmacy" && f.SpecializationId == item.specializationId);//&& (f.Recruitedfor == "PG" || f.Recruitedfor == "UG&PG")
                        }
                        else
                        {
                            intakedetails.specializationWiseFaculty = jntuh_registered_faculty.Count(f => f.Department == item.Department &&
                                        f.SpecializationId == item.specializationId && (f.Recruitedfor == "PG" || f.Recruitedfor == "UG&PG"));//
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
                        pgFaculty = jntuh_registered_faculty.Count(f => ("PG" == f.HighestDegree || "M.Phil / Other PG Degree" == f.HighestDegree) &&
                                    f.Department == "Pharmacy" && f.SpecializationId == item.specializationId);
                        phdFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && f.Department == "Pharmacy" && f.SpecializationId == item.specializationId);
                        //noPanOrAadhaarcount =registeredFaculty.Where(f =>f.DepartmentId == pharmacyDeptId && f.PANNumber == null && f.AadhaarNumber == null &&
                        //            (f.isApproved == null || f.isApproved == true)).Count();
                        //intakedetails.Department = "Pharmacy";
                    }
                    else if (item.Degree == "Pharm.D")
                    {
                        ugFaculty = jntuh_registered_faculty.Count(f => "UG" == f.HighestDegree && f.Department == "Pharm.D" && f.SpecializationId == item.specializationId);
                        pgFaculty = jntuh_registered_faculty.Count(f => ("PG" == f.HighestDegree || "M.Phil / Other PG Degree" == f.HighestDegree) && f.Department == "Pharm.D" && f.SpecializationId == item.specializationId);
                        phdFaculty = jntuh_registered_faculty.Count(f => "Ph.D" == f.HighestDegree && f.Department == "Pharm.D" && f.SpecializationId == item.specializationId);
                        //noPanOrAadhaarcount = registeredFaculty.Where(f => f.DepartmentId == pharmacyDeptId && f.PANNumber == null && f.AadhaarNumber == null && (f.isApproved == null || f.isApproved == true)).Count();
                        intakedetails.Department = "Pharm.D";
                    }
                    else if (item.Degree == "Pharm.D PB")
                    {
                        ugFaculty = jntuh_registered_faculty.Count(f => "UG" == f.HighestDegree && f.Department == "Pharm.D PB" && f.SpecializationId == item.specializationId);
                        pgFaculty = jntuh_registered_faculty.Count(f => ("PG" == f.HighestDegree || "M.Phil / Other PG Degree" == f.HighestDegree) && f.Department == "Pharm.D PB" && f.SpecializationId == item.specializationId);
                        phdFaculty = jntuh_registered_faculty.Count(f => "Ph.D" == f.HighestDegree && f.Department == "Pharm.D PB" && f.SpecializationId == item.specializationId);
                        //noPanOrAadhaarcount = registeredFaculty.Where(f => f.DepartmentId == pharmacyDeptId && f.PANNumber == null && f.AadhaarNumber == null && (f.isApproved == null || f.isApproved == true)).Count();
                        intakedetails.Department = "Pharm.D PB";
                    }
                    else
                    {
                        ugFaculty = jntuh_registered_faculty.Count(f => "UG" == f.HighestDegree && f.Department == item.Department);
                        pgFaculty = jntuh_registered_faculty.Count(f => ("PG" == f.HighestDegree || "M.Phil / Other PG Degree" == f.HighestDegree) && f.Department == item.Department);
                        phdFaculty = jntuh_registered_faculty.Count(f => "Ph.D" == f.HighestDegree && f.Department == item.Department);

                        var PhdReg = jntuh_registered_faculty.Where(f => "Ph.D" == f.HighestDegree && f.Department == item.Department).Select(f => f.RegistrationNumber).ToList();

                        //var phdFaculty1 = jntuh_registered_faculty.Where(f => ("Ph.D" == f.HighestDegree || "Ph.D." == f.HighestDegree || "Phd" == f.HighestDegree || "PHD" == f.HighestDegree || "Ph D" == f.HighestDegree)).ToList() ;
                        //if (item.Department == "MBA")
                        //    phdFaculty1 = phdFaculty1.Where(f => f.Department == "MBA").ToList();

                        // string REG=
                        if (item.Degree == "B.Tech")
                            SpecializationphdFaculty = jntuh_registered_faculty.Count(f => "Ph.D" == f.HighestDegree && (f.SpecializationId == item.specializationId || f.SpecializationId == null) && f.Department == item.Department);
                        else
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

                string[] strOtherDepartments = { "English", "Mathematics", "Physics", "Chemistry", "Others(CSE/IT)", "Others(CIVIL/MECH)", "Others(ECE/EEE)", "Others(MNGT/H&S)" };
                var btechdegreecount = intakedetailsList.Count(d => d.Degree == "B.Tech");
                if (btechdegreecount > 0)
                {
                    foreach (var department in strOtherDepartments)
                    {
                        var deptid = jntuh_departments.Where(i => i.departmentName == department).Select(i => i.id).FirstOrDefault();
                        var deptname = jntuh_departments.Where(i => i.departmentName == department).Select(i => i.departmentName).FirstOrDefault();
                        int speId = jntuh_specialization.Where(s => s.jntuh_department.departmentName == department).Select(s => s.id).FirstOrDefault();
                        int ugFaculty = jntuh_registered_faculty.Count(f => "UG" == f.HighestDegree && f.Department == department);
                        int pgFaculty = jntuh_registered_faculty.Count(f => ("PG" == f.HighestDegree || "M.Phil / Other PG Degree" == f.HighestDegree) && f.Department == department);
                        var pgreg = jntuh_registered_faculty.Where(f => ("PG" == f.Recruitedfor || "M.Phil / Other PG Degree" == f.HighestDegree) && f.Department == department).Select(e => e.RegistrationNumber).ToList();
                        //var testcount = jntuh_registered_faculty.Where(f => f.Department == department).ToList();
                        int phdFaculty = jntuh_registered_faculty.Count(f => "Ph.D" == f.HighestDegree && f.Department == department);

                        intakedetailsList.Add(new CollegeFacultyWithIntakeReport
                        {
                            collegeId = (int)collegeId,
                            Degree = "B.Tech",
                            DepartmentID = deptid,

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
                            specializationWiseFaculty = jntuh_registered_faculty.Count(f => f.Department == deptname),
                            Proposedintake = 1
                        });
                    }
                }
            }

            return intakedetailsList;
        }
        #endregion


        //Court Report for B.Pharmacy
        public string PharmacyInactiveFaculty(int collegeID)
        {
            string PharmacyData =string.Empty;
            bool Status = false;

            var jntuh_departments = db.jntuh_department.Where(e => e.isActive == true).ToList();
            var jntuh_specialization = db.jntuh_specialization.Where(e => e.isActive == true).ToList();

            var jntuh_academic_year = db.jntuh_academic_year.ToList();
            int presentYear = jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int AY1 = jntuh_academic_year.Where(a => a.actualYear == (presentYear + 1)).Select(a => a.id).FirstOrDefault();
            int AY2 = jntuh_academic_year.Where(a => a.actualYear == (presentYear)).Select(a => a.id).FirstOrDefault();
            int AY3 = jntuh_academic_year.Where(a => a.actualYear == (presentYear - 1)).Select(a => a.id).FirstOrDefault();
            int AY4 = jntuh_academic_year.Where(a => a.actualYear == (presentYear - 2)).Select(a => a.id).FirstOrDefault();
            int AY5 = jntuh_academic_year.Where(a => a.actualYear == (presentYear - 3)).Select(a => a.id).FirstOrDefault();
            int AY6 = jntuh_academic_year.Where(a => a.actualYear == (presentYear - 4)).Select(a => a.id).FirstOrDefault();

            var Departments = db.jntuh_department.Where(d => d.degreeId == 2 || d.degreeId == 5 || d.degreeId == 9 || d.degreeId == 10).ToList();
            int[] Departmentids = Departments.Select(d => d.id).ToArray();
            var Specializations = db.jntuh_specialization.Where(s => Departmentids.Contains(s.departmentId)).ToList();
            int[] Specializationids = Specializations.Select(S => S.id).ToArray();

            List<jntuh_college_intake_existing> intake = db.jntuh_college_intake_existing.Where(i => i.collegeId == collegeID && i.courseStatus != "Closure" && i.shiftId == 1 && Specializationids.Contains(i.specializationId)).ToList();
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

                newIntake.approvedIntake1 = PharmacyGetIntake(collegeID, AY1, item.specializationId, item.shiftId, 1, newIntake.degreeID);
                newIntake.approvedIntake2 = PharmacyGetIntake(collegeID, AY2, item.specializationId, item.shiftId, 1, newIntake.degreeID);
                newIntake.approvedIntake3 = PharmacyGetIntake(collegeID, AY3, item.specializationId, item.shiftId, 1, newIntake.degreeID);
                newIntake.approvedIntake4 = PharmacyGetIntake(collegeID, AY4, item.specializationId, item.shiftId, 1, newIntake.degreeID);
                newIntake.approvedIntake5 = PharmacyGetIntake(collegeID, AY5, item.specializationId, item.shiftId, 1, newIntake.degreeID);
                
                newIntake.admittedIntake1 = PharmacyGetIntake(collegeID, AY1, item.specializationId, item.shiftId, 0, newIntake.degreeID);
                newIntake.admittedIntake2 = PharmacyGetIntake(collegeID, AY2, item.specializationId, item.shiftId, 0, newIntake.degreeID);
                newIntake.admittedIntake3 = PharmacyGetIntake(collegeID, AY3, item.specializationId, item.shiftId, 0, newIntake.degreeID);
                newIntake.admittedIntake4 = PharmacyGetIntake(collegeID, AY4, item.specializationId, item.shiftId, 0, newIntake.degreeID);
                newIntake.admittedIntake5 = PharmacyGetIntake(collegeID, AY5, item.specializationId, item.shiftId, 0, newIntake.degreeID);


                collegeIntakeExisting.Add(newIntake);
            }
            CurrentyearcollegeIntakeExisting = collegeIntakeExisting.Where(a => a.academicYearId == 10).ToList();


            string Collegeid = collegeID.ToString();
            var jntuh_appeal_pharmacy_data = db.jntuh_appeal_pharmacydata.Where(e => e.CollegeCode == Collegeid).Select(e => e).ToList();

            var jntuh_college_faculty = db.jntuh_college_faculty_registered.Where(e => e.collegeId == collegeID).Select(e => e).ToList();
            var strRegnosNew = jntuh_college_faculty.Where(e=>e.DepartmentId == 26 ||e.DepartmentId == 36 ||e.DepartmentId == 27 ||e.DepartmentId == 39 ||e.DepartmentId == 61).Select(e => e.RegistrationNumber).ToArray();
            var jntuh_registred_faculty = db.jntuh_registered_faculty.Where(e => strRegnosNew.Contains(e.RegistrationNumber)).Select(e => e).ToList();

            var CollegeClearedFaculty = jntuh_registred_faculty.Where(rf => rf.type != "Adjunct" && ((rf.Absent == false) && (rf.OriginalCertificatesNotShown == false) && (rf.Xeroxcopyofcertificates == false || rf.Xeroxcopyofcertificates == null) && (rf.NotQualifiedAsperAICTE == false || rf.NotQualifiedAsperAICTE == null)//&& rf.Noform16Verification == false && rf.NoForm16 == false
                                                        && (rf.NoSCM == false || rf.NoSCM == null) && (rf.PANNumber != null) && (rf.IncompleteCertificates == false || rf.IncompleteCertificates == null) && (rf.Blacklistfaculy == false) && rf.NoRelevantUG != "Yes" && rf.NoRelevantPG != "Yes" && rf.NORelevantPHD != "Yes" && rf.NotIdentityfiedForanyProgram == false && (rf.InvalidPANNumber == false || rf.InvalidPANNumber == null) && (rf.OriginalsVerifiedPHD != true) && (rf.OriginalsVerifiedUG != true) && (rf.Invaliddegree == false || rf.Invaliddegree == null) && rf.InvalidAadhaar != "Yes") && rf.BAS != "Yes" && (rf.DepartmentId != 61 || rf.DepartmentId != 27)).Select(rf => rf).ToList();

            var FacultyIds = jntuh_registred_faculty.Select(e => e.id).ToArray();

            var jntuh_registered_faculty_education = db.jntuh_registered_faculty_education.Where(e => FacultyIds.Contains(e.facultyId)).Select(e => e).ToList();

            var ClearedFacultyRegnos = CollegeClearedFaculty.Select(e => e.RegistrationNumber).ToArray();

            var CollegeInactiveFaculty = jntuh_registred_faculty.Where(e => !ClearedFacultyRegnos.Contains(e.RegistrationNumber)).Select(a => new
            {
                type = a.type,
                RegistrationNumber = a.RegistrationNumber.Trim(),
                FullName = a.FirstName + " " + a.MiddleName + " " + a.LastName,
                DeptId = jntuh_college_faculty.Where(c => c.RegistrationNumber.Trim() == a.RegistrationNumber.Trim()).Select(c => c.DepartmentId).FirstOrDefault(),
                SpecializationId = jntuh_college_faculty.Where(c => c.RegistrationNumber.Trim() == a.RegistrationNumber.Trim()).Select(c => c.SpecializationId).FirstOrDefault(),
                HighestDegreeID = a.jntuh_registered_faculty_education.Count() != 0 ? a.jntuh_registered_faculty_education.Select(e => e.educationId).Max() : 0,
                IsApproved = a.isApproved,
                PanNumber = a.PANNumber,
                AadhaarNumber = a.AadhaarNumber,
                DegreeId = jntuh_registered_faculty_education.Count(e => e.facultyId == a.id) > 0 ? a.jntuh_registered_faculty_education.Where(e => e.facultyId == a.id).Select(e => e.educationId).Max() : 0,
               
                //Faculty Flags
                Absent = a.Absent != null ? (bool)a.Absent : false,
                BlacklistFaculty = a.Blacklistfaculy != null ? (bool)a.Blacklistfaculy : false,
                PHDundertakingnotsubmitted = a.PHDundertakingnotsubmitted != null ? (bool)a.PHDundertakingnotsubmitted : false,
                NOTQualifiedAsPerAICTE = a.NotQualifiedAsperAICTE != null ? (bool)a.NotQualifiedAsperAICTE : false,
                InvalidPANNo = a.InvalidPANNumber != null ? (bool)a.InvalidPANNumber : false,
                InCompleteCeritificates = a.IncompleteCertificates != null ? (bool)a.IncompleteCertificates : false,
                OriginalCertificatesnotshownFlag = a.OriginalCertificatesNotShown != null ? (bool)a.OriginalCertificatesNotShown : false,
                FalsePAN = a.FalsePAN != null ? (bool)a.FalsePAN : false,
                NOForm16 = a.NoForm16 != null ? (bool)a.NoForm16 : false,
                MultipleReginSamecoll = a.Invaliddegree != null ? (bool)a.Invaliddegree : false,
                XeroxcopyofcertificatesFlag = a.Xeroxcopyofcertificates != null ? (bool)a.Xeroxcopyofcertificates : false,
                NotIdentityFiedForAnyProgramFlag = a.NotIdentityfiedForanyProgram != null ? (bool)a.NotIdentityfiedForanyProgram : false,
                NOrelevantUgFlag = a.NoRelevantUG == "Yes" ? true : false,
                NOrelevantPgFlag = a.NoRelevantPG == "Yes" ? true : false,
                NOrelevantPhdFlag = a.NORelevantPHD == "Yes" ? true : false,
                //NoForm16Verification = a.Noform16Verification != null ? (bool)a.Noform16Verification : false,
                NoSCM = a.NoSCM != null ? (bool)a.NoSCM : false,
                //PhotocopyofPAN = a.PhotoCopyofPAN != null ? (bool)a.PhotoCopyofPAN : false,
                PhdUndertakingDocumentstatus = a.PhdUndertakingDocumentstatus != null ? (bool)(a.PhdUndertakingDocumentstatus) : false,
                PHDUndertakingDocumentView = a.PHDUndertakingDocument,
                PhdUndertakingDocumentText = a.PhdUndertakingDocumentText,
                //AppliedPAN = a.AppliedPAN != null ? (bool)(a.AppliedPAN) : false,
                //SamePANUsedByMultipleFaculty = a.SamePANUsedByMultipleFaculty != null ? (bool)(a.SamePANUsedByMultipleFaculty) : false,
                //BasstatusOld = a.BASStatusOld,
                //Basstatus Column Consider as Aadhaar Flag 
                //Basstatus = a.BASStatus,
                Deactivedby = a.DeactivatedBy,
                DeactivedOn = a.DeactivatedOn,
                //SCMDocumentView =
                //    db.jntuh_scmupload.Where(u => u.ProfDocument.Trim() == faculty.RegistrationNumber.Trim() && u.DepartmentId != 0 && u.SpecializationId != 0)
                //        .Select(s => s.AssistDocument)
                //        .FirstOrDefault(),
                OriginalsVerifiedPHD = a.OriginalsVerifiedPHD == true ? true : false,
                OriginalsVerifiedUG = a.OriginalsVerifiedUG == true ? true : false

            }).ToList();

            var jntuh_college_basreport = db.jntuh_college_basreport.Where(b => b.collegeId == collegeID).Select(e => e).ToList();

            foreach (var pharmacy in CurrentyearcollegeIntakeExisting)
	        {

                PharmacyData += "<p><b><u>"+pharmacy.Degree+ "-" + pharmacy.Specialization+"</u></b></p>";
                PharmacyData += "<table width='100%' border='1' cellpadding='3' cellspacing='0' style='border-color: #ccc;'>";
                PharmacyData += "<tr>";
                 PharmacyData += "<th  style='text-align: center; vertical-align: top;' >Degree</th>";

                PharmacyData += "<th  style='text-align: center; vertical-align: top;' >Academic Year</th>";
                if(pharmacy.Degree == "B.Pharmacy")
                {
                    PharmacyData += "<th  style='text-align: center; vertical-align: top;' >Intake</th>";
                    PharmacyData += "<th  style='text-align: center; vertical-align: top;' >Total Intake</th>";
                    PharmacyData += "<th  style='text-align: center; vertical-align: top;' >Groups</th>";
                PharmacyData += "<th  style='text-align: center; vertical-align: top;'>Required <br/>Faculty</th>";
                PharmacyData += "<th  style='text-align: center; vertical-align: top;'>Available <br/>Faculty</th>";
                PharmacyData += "<th  style='text-align: center; vertical-align: top;' >Groups Deficiency</th>";
           
                PharmacyData += "<th  style='text-align: center; vertical-align: top;'>Required</th>";
                PharmacyData += "<th  style='text-align: center; vertical-align: top;'>Available</th>";
              
                PharmacyData += "<th  style='text-align: center; vertical-align: top;' >Deficiency</th>";
                }
                else  if (pharmacy.Degree =="M.Pharmacy")
                {
                    PharmacyData += "<th  style='text-align: center; vertical-align: top;' >Proposed Intake</th>";
                  
                    PharmacyData += "<th  style='text-align: center; vertical-align: top;'>Required <br/>Faculty</th>";
                    PharmacyData += "<th  style='text-align: center; vertical-align: top;'>Available <br/>Faculty</th>";
                
                    PharmacyData += "<th  style='text-align: center; vertical-align: top;'>Ph.D</th>";
                    PharmacyData += "<th  style='text-align: center; vertical-align: top;' >Deficiency</th>";
                }
                else
                {
                    PharmacyData += "<th  style='text-align: center; vertical-align: top;' >Proposed Intake</th>";
                
                    PharmacyData += "<th  style='text-align: center; vertical-align: top;' >Deficiency</th>";

                }

                
                PharmacyData += "</tr>";

               

                if(pharmacy.Degree == "B.Pharmacy")
                {
                    var Group1RequiredFaculty = jntuh_appeal_pharmacy_data.Where(e=>e.Department=="26" && e.PharmacySpecialization == "1").Select(e=>e.SpecializationWiseRequiredFaculty).FirstOrDefault();
                    var Group2RequiredFaculty = jntuh_appeal_pharmacy_data.Where(e => e.Department == "26" && e.PharmacySpecialization == "2").Select(e => e.SpecializationWiseRequiredFaculty).FirstOrDefault();
                    var Group3RequiredFaculty = jntuh_appeal_pharmacy_data.Where(e => e.Department == "26" && e.PharmacySpecialization == "3").Select(e => e.SpecializationWiseRequiredFaculty).FirstOrDefault();
                    var Group4RequiredFaculty = jntuh_appeal_pharmacy_data.Where(e => e.Department == "26" && e.PharmacySpecialization == "4").Select(e => e.SpecializationWiseRequiredFaculty).FirstOrDefault();

                    var Group1Faculty = jntuh_appeal_pharmacy_data.Where(e => e.Department == "26" && e.PharmacySpecialization == "1" && e.Deficiency != null).Select(e => e.Deficiency).ToList();
                    var Group2Faculty = jntuh_appeal_pharmacy_data.Where(e => e.Department == "26" && e.PharmacySpecialization == "2" && e.Deficiency != null).Select(e => e.Deficiency).ToList();
                    var Group3Faculty = jntuh_appeal_pharmacy_data.Where(e => e.Department == "26" && e.PharmacySpecialization == "3" && e.Deficiency != null).Select(e => e.Deficiency).ToList();
                    var Group4Faculty = jntuh_appeal_pharmacy_data.Where(e => e.Department == "26" && e.PharmacySpecialization == "4" && e.Deficiency != null).Select(e => e.Deficiency).ToList();
                    
                    var Group1FacultyIds = jntuh_registred_faculty.Where(e=>Group1Faculty.Contains(e.RegistrationNumber)).Select(e=>e.id).ToList();
                    var Group1PHD = jntuh_registered_faculty_education.Where(e=>Group1FacultyIds.Contains(e.facultyId) && e.educationId ==6).Select(e=>e.facultyId).Distinct().Count();
                    var Group2FacultyIds = jntuh_registred_faculty.Where(e => Group2Faculty.Contains(e.RegistrationNumber)).Select(e => e.id).ToList();
                    var Group2PHD = jntuh_registered_faculty_education.Where(e=>Group2FacultyIds.Contains(e.facultyId) && e.educationId ==6).Select(e=>e.facultyId).Distinct().Count();
                    var Group3FacultyIds = jntuh_registred_faculty.Where(e=>Group3Faculty.Contains(e.RegistrationNumber)).Select(e=>e.id).ToList();
                    var Group3PHD = jntuh_registered_faculty_education.Where(e=>Group3FacultyIds.Contains(e.facultyId) && e.educationId ==6).Select(e=>e.facultyId).Distinct().Count();
                    var Group4FacultyIds = jntuh_registred_faculty.Where(e=>Group4Faculty.Contains(e.RegistrationNumber)).Select(e=>e.id).ToList();
                    var Group4PHD = jntuh_registered_faculty_education.Where(e=>Group4FacultyIds.Contains(e.facultyId) && e.educationId ==6).Select(e=>e.facultyId).Distinct().Count();
                    


                 PharmacyData += "<tr>";
                 PharmacyData += "<td rowspan='4' style='text-align:center;'>" + pharmacy.Degree + "</td>";

                 PharmacyData += "<td style='text-align:center;'>2018-19</td>";
                 PharmacyData += "<td style='text-align:center;'>" + pharmacy.approvedIntake1 + "</td>";
                 PharmacyData += "<td rowspan='4' style='text-align:center;'>" + (pharmacy.approvedIntake1 + pharmacy.approvedIntake2 + pharmacy.approvedIntake3 + pharmacy.approvedIntake4) + "</td>";
                 PharmacyData += "<td style='text-align:center;'>Group1</td>";
                 PharmacyData += "<td style='text-align:center;'>" + Group1RequiredFaculty + "</td>";
                 PharmacyData += "<td style='text-align:center;'>" + Group1Faculty.Count() + "</td>";
                 if (Group1Faculty.Count() >= Group1RequiredFaculty)
                     PharmacyData += "<td style='text-align:center;'>NIL</td>";
                 else
                     PharmacyData += "<td style='text-align:center;'>Yes</td>";
                 

                    var TotalRequiredFaculty = Group1RequiredFaculty+Group2RequiredFaculty+Group3RequiredFaculty+Group4RequiredFaculty;
                    var AvailiableFaculty =(Group1Faculty.Count()+Group2Faculty.Count()+Group3Faculty.Count()+Group4Faculty.Count());

                    PharmacyData += "<td rowspan='4' style='text-align:center;'>" + TotalRequiredFaculty + "</td>";
                    PharmacyData += "<td rowspan='4' style='text-align:center;'>" + AvailiableFaculty + "</td>";
                 
                    
                    if (AvailiableFaculty >= TotalRequiredFaculty)
                    {
                        Status = true;
                        PharmacyData += "<td rowspan='4' style='text-align:center;'>No Deficiency</td>";
                    }
                    else
                    {
                        PharmacyData += "<td rowspan='4' style='text-align:center;'>Deficiency</td>";
                    }
                
                    PharmacyData += "</tr>";

                    PharmacyData += "<tr>";
                    PharmacyData += "<td style='text-align:center;'>2017-18</td>";
                    PharmacyData += "<td style='text-align:center;'>" + pharmacy.approvedIntake2 + "</td>";
                
                    PharmacyData += "<td style='text-align:center;'>Group2</td>";
                    PharmacyData += "<td style='text-align:center;'>" + Group2RequiredFaculty + "</td>";
                 PharmacyData += "<td style='text-align:center;'>"+Group2Faculty.Count()+"</td>";
                 if (Group2Faculty.Count() >= Group2RequiredFaculty)
                     PharmacyData += "<td style='text-align:center;'>NIL</td>";
                 else
                     PharmacyData += "<td style='text-align:center;'>Yes</td>";
               
               
                    PharmacyData += "</tr>";

                    PharmacyData += "<tr>";
                    PharmacyData += "<td style='text-align:center;'>2016-17</td>";
                    PharmacyData += "<td style='text-align:center;'>" + pharmacy.approvedIntake3 + "</td>";
                
                    PharmacyData += "<td style='text-align:center;'>Group3</td>";
                    PharmacyData += "<td style='text-align:center;'>" + Group3RequiredFaculty + "</td>";
                    PharmacyData += "<td style='text-align:center;'>" + Group3Faculty.Count() + "</td>";
                 if (Group3Faculty.Count() >= Group3RequiredFaculty)
                     PharmacyData += "<td style='text-align:center;'>NIL</td>";
                 else
                     PharmacyData += "<td style='text-align:center;'>Yes</td>";
          
                
                     PharmacyData += "</tr>";

                     PharmacyData += "<tr>";
                     PharmacyData += "<td style='text-align:center;'>2015-16</td>";
                     PharmacyData += "<td style='text-align:center;'>" + pharmacy.approvedIntake4 + "</td>";
                
                     PharmacyData += "<td style='text-align:center;'>Group4</td>";
                     PharmacyData += "<td style='text-align:center;'>" + Group4RequiredFaculty + "</td>";
                     PharmacyData += "<td style='text-align:center;'>" + Group4Faculty.Count() + "</td>";
                 if (Group4Faculty.Count() >= Group4RequiredFaculty)
                     PharmacyData += "<td style='text-align:center;'>NIL</td>";
                 else
                     PharmacyData += "<td style='text-align:center;'>Yes</td>";
              
               
                     PharmacyData += "</tr>";

                }
                else if(pharmacy.Degree == "M.Pharmacy")
                {

                    var MPharmcy = jntuh_appeal_pharmacy_data.Where(e=>e.Department == pharmacy.DepartmentID.ToString() && e.Specialization == pharmacy.specializationId.ToString()).Select(e=>e).ToList();
                    var Regnos = MPharmcy.Select(e=>e.Deficiency).ToList();
                    var FacultyIDS = jntuh_registred_faculty.Where(e=>Regnos.Contains(e.RegistrationNumber)).Select(e=>e.id).ToList();
                    var PHD = jntuh_registered_faculty_education.Where(e => FacultyIDS.Contains(e.facultyId) && e.educationId == 6).Select(e => e.facultyId).Distinct().Count();
                     
                    PharmacyData += "<tr>";
                    PharmacyData += "<td style='text-align:center;'>" + pharmacy.Degree + "</td>";
                    PharmacyData += "<td style='text-align:center;'>2018-19</td>";
                    PharmacyData += "<td style='text-align:center;'>" + pharmacy.approvedIntake1 + "</td>";
                 
                    PharmacyData += "<td style='text-align:center;'>" + MPharmcy.Select(e => e.SpecializationWiseRequiredFaculty).FirstOrDefault() + "</td>";
                    PharmacyData += "<td style='text-align:center;'>" + MPharmcy.Where(e => e.Deficiency != null).Select(e => e.Deficiency).Distinct().Count() + "</td>";
                 
                    PharmacyData += "<td style='text-align:center;'>" + PHD + "</td>";

                      if(MPharmcy.Select(e=>e.SpecializationWiseRequiredFaculty).FirstOrDefault() <= MPharmcy.Where(e=>e.Deficiency != null).Select(e=>e.Deficiency).Distinct().Count())
                          PharmacyData += "<td style='text-align:center;'>No Deficiency</td>";
                     else
                          PharmacyData += "<td style='text-align:center;'>Deficiency</td>";
                      PharmacyData += "</tr>";
                }
                else
                {
                    PharmacyData += "<tr>";
                    PharmacyData += "<td style='text-align:center;'>" + pharmacy.Degree + "</td>";
                    PharmacyData += "<td style='text-align:center;'>2018-19</td>";
                    PharmacyData += "<td style='text-align:center;'>" + pharmacy.approvedIntake1 + "</td>";
                

                    if (Status == true)
                        PharmacyData += "<td style='text-align:center;'>No Deficiency</td>";
                    else
                        PharmacyData += "<td style='text-align:center;'>Deficiency</td>";
                    PharmacyData += "</tr>";
                }
                
                PharmacyData += "</table>";


              

            }

            #region Inactive Faculty
            PharmacyData += "<p><b><u>Pharmacy Inactive Faculty</u></b></p>";
            PharmacyData += "<table width='100%' border='1' cellpadding='3' cellspacing='0' style='border-color: #ccc;'>";
            PharmacyData += "<tr>";
            PharmacyData += "<th>S.No</th>";
            PharmacyData += "<th>Registration Number</th>";
            PharmacyData += "<th width='10%'>Name</th>";
            PharmacyData += "<th>Department</th>";
            PharmacyData += "<th>Specialization</th>";
            PharmacyData += "<th>BAS Joining Date</th>";
            PharmacyData += "<th>August</th>";
            PharmacyData += "<th>September</th>";
            PharmacyData += "<th>October</th>";
            PharmacyData += "<th>November</th>";
            PharmacyData += "<th>December</th>";
            PharmacyData += "<th>January</th>";
            PharmacyData += "<th>February</th>";
            PharmacyData += "<th>March</th>";
            PharmacyData += "<th>April</th>";
            PharmacyData += "<th>Days Present /Total Days</th>";
            PharmacyData += "<th>Reason for Inactive</th>";
            PharmacyData += "</tr>";
            int index = 1;
            foreach (var item in CollegeInactiveFaculty)
            {
                string Reason = null;
                var Specialization = item.SpecializationId != null ? jntuh_specialization.Where(e => e.id == item.SpecializationId).Select(e => e.specializationName).FirstOrDefault() : null;
                var Department = item.DeptId != null ? jntuh_departments.Where(e => e.id == item.DeptId).Select(e => e.departmentName).FirstOrDefault() : null;

                GetFacultyBASDetails BASFaculty = new GetFacultyBASDetails();
                var EachFaculty = jntuh_college_basreport.Where(e => e.RegistrationNumber == item.RegistrationNumber&&e.monthId==11).Select(e => e).ToList();

                string date = EachFaculty.Select(e => e.joiningDate).LastOrDefault().ToString();
                if (EachFaculty.Count == 0)
                {

                }
                else
                {
                    BASFaculty.BasJoiningDate = date == null ? null : Convert.ToDateTime(date).ToString("dd-MM-yyyy");
                }

                foreach (var BAS in EachFaculty)
                {
                    
                    if (BAS.month == "August")
                    {
                        BASFaculty.AugustTotalDays = BAS.totalworkingDays;
                        BASFaculty.AugustPresentDays = BAS.NoofPresentDays;
                    }

                    else if (BAS.month == "September")
                    {
                        BASFaculty.SeptemberTotalDays = BAS.totalworkingDays;
                        BASFaculty.SeptemberPresentDays = BAS.NoofPresentDays;
                    }

                    else if (BAS.month == "October")
                    {
                        BASFaculty.OctoberTotalDays = BAS.totalworkingDays;
                        BASFaculty.OctoberPresentDays = BAS.NoofPresentDays;
                    }

                    else if (BAS.month == "November")
                    {
                        BASFaculty.NovemberTotalDays = BAS.totalworkingDays;
                        BASFaculty.NovemberPresentDays = BAS.NoofPresentDays;
                    }

                    else if (BAS.month == "December")
                    {
                        BASFaculty.DecemberTotalDays = BAS.totalworkingDays;
                        BASFaculty.DecemberPresentDays = BAS.NoofPresentDays;
                    }

                    else if (BAS.month == "January")
                    {
                        BASFaculty.JanuaryTotalDays = BAS.totalworkingDays;
                        BASFaculty.JanuaryPresentDays = BAS.NoofPresentDays;
                    }

                    else if (BAS.month == "February")
                    {
                        BASFaculty.FebruaryTotalDays = BAS.totalworkingDays;
                        BASFaculty.FebruaryPresentDays = BAS.NoofPresentDays;
                    }

                    else if (BAS.month == "March")
                    {
                        BASFaculty.MarchTotalDays = BAS.totalworkingDays;
                        BASFaculty.MarchPresentDays = BAS.NoofPresentDays;
                    }
                    else if (BAS.month == "April")
                    {
                        BASFaculty.AprilTotalDays = BAS.totalworkingDays;
                        BASFaculty.AprilPresentDays = BAS.NoofPresentDays;
                    }
                   
                }


                PharmacyData += "<tr>";
                PharmacyData += "<td>" + (index) + "</td>";
                PharmacyData += "<td>" + item.RegistrationNumber + "</td>";
                if (item.DegreeId == 6)
                    PharmacyData += "<td>" + item.FullName + "(Ph.D)" + "</td>";
                else
                    PharmacyData += "<td>" + item.FullName + "</td>";
                PharmacyData += "<td>" + Department + "</td>";
                PharmacyData += "<td>" + Specialization + "</td>";

                PharmacyData += "<td>" + BASFaculty.BasJoiningDate + "</td>";
               
                if (BASFaculty.AugustTotalDays == 0 || BASFaculty.AugustTotalDays == null)
                    PharmacyData += "<td>--</td>";
                else
                    PharmacyData += "<td>" + BASFaculty.AugustPresentDays + "/" + BASFaculty.AugustTotalDays + "</td>";


                if (BASFaculty.SeptemberTotalDays == 0 || BASFaculty.SeptemberTotalDays == null)
                    PharmacyData += "<td>--</td>";
                else
                    PharmacyData += "<td>" + BASFaculty.SeptemberPresentDays + "/" + BASFaculty.SeptemberTotalDays + "</td>";
                if (BASFaculty.OctoberTotalDays == 0 || BASFaculty.OctoberTotalDays == null)
                    PharmacyData += "<td>--</td>";
                else
                    PharmacyData += "<td>" + BASFaculty.OctoberPresentDays + "/" + BASFaculty.OctoberTotalDays + "</td>";
                if (BASFaculty.NovemberTotalDays == 0 || BASFaculty.NovemberTotalDays == null)
                    PharmacyData += "<td>--</td>";
                else
                    PharmacyData += "<td>" + BASFaculty.NovemberPresentDays + "/" + BASFaculty.NovemberTotalDays + "</td>";
                if (BASFaculty.DecemberTotalDays == 0 || BASFaculty.DecemberTotalDays == null)
                    PharmacyData += "<td>--</td>";
                else
                    PharmacyData += "<td>" + BASFaculty.DecemberPresentDays + "/" + BASFaculty.DecemberTotalDays + "</td>";
                if (BASFaculty.JanuaryTotalDays == 0 || BASFaculty.JanuaryTotalDays == null)
                    PharmacyData += "<td>--</td>";
                else
                    PharmacyData += "<td>" + BASFaculty.JanuaryPresentDays + "/" + BASFaculty.JanuaryTotalDays + "</td>";
                if (BASFaculty.FebruaryTotalDays == 0 || BASFaculty.FebruaryTotalDays == null)
                    PharmacyData += "<td>--</td>";
                else
                    PharmacyData += "<td>" + BASFaculty.FebruaryPresentDays + "/" + BASFaculty.FebruaryTotalDays + "</td>";
                if (BASFaculty.MarchTotalDays == 0 || BASFaculty.MarchTotalDays == null)
                    PharmacyData += "<td>--</td>";
                else
                    PharmacyData += "<td>" + BASFaculty.MarchPresentDays + "/" + BASFaculty.MarchTotalDays + "</td>";
                if (BASFaculty.AprilTotalDays == 0 || BASFaculty.AprilTotalDays == null)
                    PharmacyData += "<td>--</td>";
                else
                    PharmacyData += "<td>" + BASFaculty.AprilPresentDays + "/" + BASFaculty.AprilTotalDays + "</td>";


                // faculty += "<td>" +  + "</td>";
                PharmacyData += "<td>" + EachFaculty.Select(e => e.NoofPresentDays).Sum() + "/" + EachFaculty.Select(e => e.totalworkingDays).Sum() + "</td>";


                if (item.Absent == true)
                    Reason += "Absent";

                if (item.type == "Adjunct")
                {
                    if (Reason != null)
                        Reason += ",Adjunct Faculty";
                    else
                        Reason += "Adjunct Faculty";
                }

                if (item.XeroxcopyofcertificatesFlag == true)
                {
                    if (Reason != null)
                        Reason += ",Xerox copy of certificates";
                    else
                        Reason += "Xerox copy of certificates";
                }

                if (item.NOrelevantUgFlag == true)
                {
                    if (Reason != null)
                        Reason += ",NO Relevant UG";
                    else
                        Reason += "NO Relevant UG";
                }

                if (item.NOrelevantPgFlag == true)
                {
                    if (Reason != null)
                        Reason += ",NO Relevant PG";
                    else
                        Reason += "NO Relevant PG";
                }

                if (item.NOrelevantPhdFlag == true)
                {
                    if (Reason != null)
                        Reason += ",NO Relevant PHD";
                    else
                        Reason += "NO Relevant PHD";
                }

                if (item.NOTQualifiedAsPerAICTE == true)
                {
                    if (Reason != null)
                        Reason += ",NOT Qualified As per AICTE";
                    else
                        Reason += "NOT Qualified As per AICTE";
                }

                if (item.InvalidPANNo == true)
                {
                    if (Reason != null)
                        Reason += ",Invalid PANNumber";
                    else
                        Reason += "Invalid PANNumber";
                }

                if (item.InCompleteCeritificates == true)
                {
                    if (Reason != null)
                        Reason += ",In Complete Certificates";
                    else
                        Reason += "In Complete Certificates";
                }

                if (item.NoSCM == true)
                {
                    if (Reason != null)
                        Reason += ",No SCM";
                    else
                        Reason += "No SCM";
                }

                if (item.OriginalCertificatesnotshownFlag == true)
                {
                    if (Reason != null)
                        Reason += ",Original Certificates not shown";
                    else
                        Reason += "Original Certificates not shown";
                }

                if (item.PanNumber == null)
                {
                    if (Reason != null)
                        Reason += ",No PAN Number";
                    else
                        Reason += "No PAN Number";
                }

                if (item.NotIdentityFiedForAnyProgramFlag == true)
                {
                    if (Reason != null)
                        Reason += ",Not identified for any Program";
                    else
                        Reason += "Not identified for any Program";
                }

                //if (item.SamePANUsedByMultipleFaculty == true)
                //{
                //    if (Reason != null)
                //        Reason += ",Same PAN Used by Multiple Faculty";
                //    else
                //        Reason += "Same PAN Used by Multiple Faculty";
                //}

                //if (item.Basstatus == "Yes")
                //{
                //    if (Reason != null)
                //        Reason += ",No/Invalid Aadhaar Document";
                //    else
                //        Reason += "No/Invalid Aadhaar Document";
                //}

                //if (item.BasstatusOld == "Yes")
                //{
                //    if (Reason != null)
                //        Reason += ",BAS";
                //    else
                //        Reason += "BAS";
                //}

                if (item.OriginalsVerifiedUG == true)
                {
                    if (Reason != null)
                        Reason += ",Complaint PHD Faculty";
                    else
                        Reason += "Complaint PHD Faculty";
                }

                if (item.OriginalsVerifiedPHD == true)
                {
                    if (Reason != null)
                        Reason += ",No Guide Sign in PHD Thesis";
                    else
                        Reason += "No Guide Sign in PHD Thesis";
                }

                PharmacyData += "<td>" + Reason + "</td>";
                PharmacyData += "</tr>";
                index++;
            }

            PharmacyData += "</table>";

            #endregion

            return PharmacyData;
        }

        public string Header(int collegeID)
        {
            string header = string.Empty;
            var collegeStatus = db.jntuh_collegestatus.Where(e => e.CollegeId == collegeID).Select(e => e).FirstOrDefault();
            if (collegeStatus != null)
            {
                //Code Commented By Narayana on 28-03-2018
                //if (collegeStatus.CollegeStatus == true || collegeStatus.AICTEStatus == true)
                //{
                //    header += "<table width='100%'>";
                //   // header += "<tr><td align='right' width='80%' style='font-size: 16px; font-weight: normal;' colspan='2'><b><u> Date : " + DateTime.Now.ToString("G") + "</u></b></td></tr></br>";
                ////http://112.133.193.228:76/Content/Images/new_logo.jpg
                //    header += "<tr>";
                //    header += "<td>Web &nbsp;&nbsp; : www.jntuh.ac.in<br/>Email &nbsp;: pa2registrar@gmail.com</td>";
                //    header += "<td align='center'><img src='http://10.10.10.5:75/Content/Images/new_logo.jpg' height='70' width='70' style='text-align: center' align='middle' /></td>";
                //    header += "<td align='right'>Phone: Off: +91-40-32422256<br/>Res: +91-40-32517275<br/>Fax: +91-40-23158665</td>";
                //    header += "</tr>";
                //    header += "<tr>";
                //    header += "<td colspan='3' align='center' width='80%' style='font-size: 14px; font-weight: normal;color: Blue'><b>JAWAHARLAL NEHRU TECHNOLOGICAL UNIVERSITY HYDERABAD</b></td>";
                //    header += "</tr>";
                //    header += "<tr>";
                //    header += "<td colspan='3' align='center' style='font-size: 11px; font-weight: normal;'> (Established by JNTU Act, 2008)</td>";
                //    header += "</tr>";
                //    header += "<tr>";
                //    header += "<td colspan='3' align='center' style='font-size: 12px; font-weight: normal;color: Purple'>Kukatpally, Hyderabad - 500 085, Telangana, India</td>";
                //    header += "</tr>";
                //    header += "</table>";
                //    header += "<p><span style='color: Purple'><b>Dr. N. YADAIAH</b></span><br/><span style='color: blue;font-size: 10px;'>B.E (OUCE), M.Tech, (IIT KGP), Ph.D (JNTU)</span><br/><span style='color: blue;font-size: 10px;'>SMIEEE, FIE, FIETE, MSSI, MISTE</span><br/><span style='color: blue;font-size: 10px;'>Professor of EEE &</span><br/><b>REGISTRAR</b>";
                //    header += "</p>";
                //    header += "<p align='center'><b><u>Speaking Order</u></b></p>";
                //    header += "<p align='right'>Date: " + DateTime.Now.ToString("dd/M/yyyy") + "</p>";
                //    header += "<p>To</p>";
                //    header += "<p>The Principal / Secretary / Chairman</p>";




                //    //header += "<table width='100%'>";
                //    //header += "<tr>";
                //    //header += "<td rowspan='4' align='center' width='20%'><img src='http://112.133.193.232:72/Content/Images/new_logo.jpg' height='70' width='70' style='text-align: center' align='middle' /></td>";
                //    //header += "<td align='center' width='80%' style='font-size: 14px; font-weight: normal;'><b>JAWAHARLAL NEHRU TECHNOLOGICAL UNIVERSITY HYDERABAD</b></td>";
                //    //header += "</tr>";
                //    //header += "<tr>";
                //    //header += "<td align='center' style='font-size: 11px; font-weight: normal;'><b>KUKATPALLY, HYDERABAD, TELANGANA, INDIA - 500 085</b></td>";
                //    //header += "</tr>";
                //    //header += "<tr>";
                //    //header += "<td  align='center' style='font-weight: normal;'><u><b>DEFICIENCY REPORT AS PER FORM 417</b></u></td>";
                //    //header += "</tr>";
                //    //header += "<tr>";
                //    //header += "<td  align='center' style='font-weight: normal;'><u><b>(for Academic Year 2017-2018)</b></u></td>";
                //    //header += "</tr>";
                //    //header += "</table>";
                //}
                //else
                //{
                header += "<table width='100%'>";
                header += "<tr><td align='right' width='80%' style='font-size: 16px; font-weight: normal;' colspan='2'><b><u> Date : " + DateTime.Now.ToString("G") + "</u></b></td></tr></br>";
                header += "<tr></tr>";
                header += "</table>";
                header += "<table width='100%'>";
                header += "<tr>";
                header += "<td rowspan='4' align='center' width='20%'><img src='http://10.10.10.5:75/Content/Images/new_logo.jpg' height='70' width='70' style='text-align: center' align='middle' /></td>";
                header += "<td align='center' width='80%' style='font-size: 14px; font-weight: normal;'><b>JAWAHARLAL NEHRU TECHNOLOGICAL UNIVERSITY HYDERABAD</b></td>";
                header += "</tr>";
                header += "<tr>";
                header += "<td align='center' style='font-size: 11px; font-weight: normal;'><b>KUKATPALLY, HYDERABAD, TELANGANA, INDIA - 500 085</b></td>";
                header += "</tr>";
                header += "<tr>";
                header += "<td  align='center' style='font-weight: normal;'><u><b>DEFICIENCY REPORT AS PER FORM 418</b></u></td>";
                header += "</tr>";
                header += "<tr>";
                header += "<td  align='center' style='font-weight: normal;'><u><b>(for Academic Year 2019-2020)</b></u></td>";
                header += "</tr>";
                header += "</table>";
                //}
            }



            return header;
        }

        public string CollegeInformation(int? collegeID)
        {
            string collegeInformation = string.Empty;

            jntuh_college college = db.jntuh_college.Where(c => c.isActive == true && c.id == collegeID).Select(c => c).FirstOrDefault();
            jntuh_college_establishment society = db.jntuh_college_establishment.Where(e => e.collegeId == collegeID).Select(e => e).FirstOrDefault();
            var collegeStatus = db.jntuh_collegestatus.Where(e => e.CollegeId == collegeID).Select(e => e).FirstOrDefault();
            if (collegeStatus != null)
            {
                //if (collegeStatus.CollegeStatus == true || collegeStatus.AICTEStatus == true)
                //{
                //    var address = db.jntuh_address.Where(e => e.collegeId == collegeID && e.addressTye=="COLLEGE").Select(e => e).FirstOrDefault();
                //    collegeInformation += "<table>";
                //    collegeInformation += "<tr><td>" + college.collegeName + "</td></tr>";
                //    collegeInformation += "<tr><td>" + address.address + "," + address.townOrCity + "," + address.mandal + "</td></tr>";
                //    collegeInformation += "<tr><td>" + db.jntuh_district.FirstOrDefault(e=>e.id==address.districtId).districtName + " - " + address.pincode + "</td></tr>";
                //    collegeInformation += "</table>";
                //}
                //else
                //{

                collegeInformation += "<table width='100%' border='0' cellpadding='5' cellspacing='0'>";
                collegeInformation += "<tr>";
                collegeInformation += "<td align='left' width='80%'><b>W.P.No: </b></td></tr>";
                collegeInformation += "<tr><td align='left' width='80%'><b>College Name: </b><u>" + college.collegeName + " (" + college.collegeCode + ")"+"</u></td>";
               // collegeInformation += "<td align='left' width='25%'><b>CC:  </b><u>" + college.collegeCode + "</u></td>";
                collegeInformation += "</tr>";
                collegeInformation += "<tr><td align='left' width='75%'><b>Society Name: </b><u>" + society.societyName + "</u></td></tr>";
                collegeInformation += "<tr><td align='left' width='75%'><b>College Establishment Year: </b><u>" + society.instituteEstablishedYear + "</u></td></tr>";

                collegeInformation += "</table>";
                //}
            }

            return collegeInformation;
        }
        public string CollegeRandomCode(int? collegeID)
        {
            string collegeInformation = string.Empty;

            var college = db.jntuh_college_randamcodes.Where(c => c.IsActive == true && c.CollegeId == collegeID).Select(c => c).FirstOrDefault();

            collegeInformation += "<table width='100%' border='0' cellpadding='5' cellspacing='0'>";
            collegeInformation += "<tr>";
            collegeInformation += "<td align='left' width='75%'><b>College Name: --- </b>";
            collegeInformation += "<td align='left' width='25%'><b>CC:  </b><u>" + college.RandamCode + "</u></td>";
            collegeInformation += "</tr>";

            collegeInformation += "</table>";

            return collegeInformation;
        }

        #region Special Case Deficiency
        public string DeficiencyHeader()
        {
            string header = string.Empty;
            header += "<table width='100%'>";
            header += "<tr><td align='right' width='80%' style='font-size: 16px; font-weight: normal;' colspan='2'><b><u> Date : " + DateTime.Now.ToString("G") + "</u></b></td></tr></br>";
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
            header += "<td  align='center' style='font-weight: normal;'><u><b>RECOMMENDATION OF THE SCA BASED ON THE FFCA REPORT</b></u></td>";
            header += "</tr>";
            header += "<tr>";
            header += "<td  align='center' style='font-weight: normal;'><u><b>(for Academic Year 2016-2017)</b></u></td>";
            header += "</tr>";
            header += "</table>";
            return header;
        }
        public ActionResult SpecialCaseDeficiencies(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                int collegeID = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(id.ToString(), System.Web.Configuration.WebConfigurationManager.AppSettings["CryptoKey"]));

                Response.ClearContent();
                Response.ClearHeaders();
                Response.Buffer = true;
                string collegeCode = db.jntuh_college.Where(c => c.isActive == true && c.id == collegeID).Select(c => c.collegeCode).FirstOrDefault();
                Response.AddHeader("content-disposition", "attachment; filename=" + collegeCode + "-SpecialCaseDeficiency Report" + id + ".doc");
                Response.ContentType = "application/vnd.ms-word ";
                Response.Charset = string.Empty;
                StringBuilder str = new StringBuilder();
                str.Append(DeficiencyHeader());
                str.Append("<br />");
                str.Append(CollegeInformation(collegeID));
                str.Append("<br />");
                str.Append(DeficiencyData());
                str.Append("<br />");
                str.Append(CollegeIntake(collegeID));
                str.Append("<br />");
                str.Append(DeficiencyData1());
                str.Append("<br />");
                //str.Append(DeficienciesInLabs(collegeID));
                //str.Append("<br />");
                //str.Append(CollegeLabsAnnexure(collegeID));
                str.Append("<br />");

                Document pdfDoc = new Document(PageSize.A4.Rotate(), 60, 50, 60, 60);

                pdfDoc.SetPageSize(iTextSharp.text.PageSize.A4.Rotate());
                pdfDoc.SetMargins(60, 50, 60, 60);

                string path = Server.MapPath("~/Content/PDFReports/SpecialcaseDeficiencyReports/");

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
        public string DeficiencyData()
        {
            string collegeInformation = string.Empty;

            // jntuh_college college = db.jntuh_college.Where(c => c.isActive == true && c.id == collegeID).Select(c => c).FirstOrDefault();

            collegeInformation += "<table width='100%' border='0' cellpadding='5' cellspacing='0'>";
            collegeInformation += "<tr> <td align='left' width='75%'><b>Based on the FFC report it has been observed that: </b></td></tr>";
            collegeInformation += "<tr> <td align='left' width='75%'>i). More than ----- of the faculty are newly appointed.</td></tr>";
            collegeInformation += "<tr> <td align='left' width='75%'>ii). As per the records the College is running the following approved programs in the previous academic years</td></tr>";
            collegeInformation += "</table>";

            return collegeInformation;
        }
        public string DeficiencyData1()
        {
            string DeficiencyData = string.Empty;

            // jntuh_college college = db.jntuh_college.Where(c => c.isActive == true && c.id == collegeID).Select(c => c).FirstOrDefault();

            DeficiencyData += "<table width='100%' border='0' cellpadding='5' cellspacing='2' line-height='1'>";
            DeficiencyData += "<tr> <td align='left' width='75%'>It appears that the faculty have been brought for the purpose of present FFC inspection only, which is an indication that minimum academic standards have not been maintained during the previous academic year.   Hence, the SCA recommends No Admission Status  for the academic year 2016-17 for your Institution. You are further directed to provide the clear explanation within 10 days regarding the availability of essential requirements for the previous batches for running the respective academic programs.</td></tr>";
            DeficiencyData += "</table>";

            DeficiencyData += "</br><table width='100%'  cellspacing='0'></br>";
            DeficiencyData += "<tr><td align='right' width='80%' style='font-size: 14px; font-weight: normal;'> <b>Sd /-</b></td></tr>";
            DeficiencyData += "<tr><td align='right' width='80%' style='font-size: 14px; font-weight: normal;'> <b>REGISTRAR</b></td></tr></br></br>";
            DeficiencyData += "<tr><td></td></tr>"; DeficiencyData += "</table>";

            return DeficiencyData;
        }
        public string CollegeIntake(int collegeId)
        {
            string Intake = string.Empty;
            var colleges = db.jntuh_college.Where(c => c.isActive == true).Select(c => new
            {
                collegeId = c.id,
                collegeName = c.collegeCode + "-" + c.collegeName
            }).OrderBy(c => c.collegeName).ToList();
            ViewBag.Colleges = colleges.OrderBy(c => c.collegeId).ThenBy(c => c.collegeName).ToList();
            //collegeId = 375;
            if (collegeId != null)
            {
                ViewBag.Status = true;
                int userCollegeID = (int)collegeId;

                ViewBag.collegeId = collegeId;
                var jntuh_academic_year = db.jntuh_academic_year.ToList();

                ViewBag.AcademicYear = jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.academicYear).FirstOrDefault();
                int actualYear = jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();

                //RAMESH: ADDED to MERGE BOTH EXISTING & PROPOSED INTAKE
                ViewBag.NextYear = String.Format("{0}-{1}", (actualYear + 1).ToString(), (actualYear + 2).ToString().Substring(2, 2));
                int AY0 = jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();


                ViewBag.FirstYear = String.Format("{0}-{1}", (actualYear).ToString(), (actualYear + 1).ToString().Substring(2, 2));
                ViewBag.SecondYear = String.Format("{0}-{1}", (actualYear - 1).ToString(), (actualYear).ToString().Substring(2, 2));
                ViewBag.ThirdYear = String.Format("{0}-{1}", (actualYear - 2).ToString(), (actualYear - 1).ToString().Substring(2, 2));
                ViewBag.FourthYear = String.Format("{0}-{1}", (actualYear - 3).ToString(), (actualYear - 2).ToString().Substring(2, 2));
                ViewBag.FifthYear = String.Format("{0}-{1}", (actualYear - 4).ToString(), (actualYear - 3).ToString().Substring(2, 2));

                int presentYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
                int AY1 = jntuh_academic_year.Where(a => a.actualYear == (presentYear)).Select(a => a.id).FirstOrDefault();
                int AY2 = jntuh_academic_year.Where(a => a.actualYear == (presentYear - 1)).Select(a => a.id).FirstOrDefault();
                int AY3 = jntuh_academic_year.Where(a => a.actualYear == (presentYear - 2)).Select(a => a.id).FirstOrDefault();
                int AY4 = jntuh_academic_year.Where(a => a.actualYear == (presentYear - 3)).Select(a => a.id).FirstOrDefault();
                int AY5 = jntuh_academic_year.Where(a => a.actualYear == (presentYear - 4)).Select(a => a.id).FirstOrDefault();

                List<jntuh_college_intake_existing> intake = db.jntuh_college_intake_existing.Where(i => i.collegeId == userCollegeID).ToList();

                int enclosureId = db.jntuh_enclosures.Where(e => e.documentName == "AICTE INTAKE APPROVAL LETTER").Select(e => e.id).FirstOrDefault();
                var AICTEApprovalLettr = db.jntuh_college_enclosures.Where(e => e.enclosureId == enclosureId && e.collegeID == userCollegeID).Select(e => e.path).FirstOrDefault();
                var jntuh_specialization = db.jntuh_specialization;
                var jntuh_department = db.jntuh_department;
                var jntuh_degree = db.jntuh_degree;
                var jntuh_shift = db.jntuh_shift;

                Intake += "<table width='100%' border='1' cellpadding='3' cellspacing='0' style='border-color: #ccc;'>";
                Intake += "<tr>";
                Intake += "<th style='text-align: left; vertical-align: top;' >S.no</th>";
                Intake += "<th style='text-align: left; vertical-align: top;' >Degree</th>";
                Intake += "<th style='text-align: left; vertical-align: top;' >Specialization</th>";
                Intake += "</tr>";
                int DegreeId = 0, DepartMentId = 0;
                string Degree = "", Specialization = "";
                foreach (var item in intake.AsEnumerable().GroupBy(r => new { r.specializationId, r.shiftId }).Select(r => r.First()).ToList())
                {
                    DepartMentId = jntuh_specialization.Where(s => s.id == item.specializationId).Select(s => s.departmentId).FirstOrDefault();
                    DegreeId = jntuh_department.Where(d => d.id == DepartMentId).Select(d => d.degreeId).FirstOrDefault();
                    Specialization = jntuh_specialization.Where(s => s.id == item.specializationId).Select(s => s.specializationName).FirstOrDefault();
                    Degree = jntuh_degree.Where(d => d.id == DegreeId).Select(d => d.degree).FirstOrDefault();
                    Intake += "<tr>";
                    Intake += "<td class='col2' style='text-align: center; vertical-align: top;width:10px' rowspan='1'>" + (intake.IndexOf(item) + 1) + "</td>";
                    Intake += "<td class='col2' style='text-align: center; vertical-align: top;width:10px' rowspan='1'>" + Degree + "</td>";
                    Intake += "<td class='col2' style='text-align: center; vertical-align: top;width:10px' rowspan='1'>" + Specialization + "</td>";
                    Intake += "</tr>";
                    //CollegeIntakeExisting newIntake = new CollegeIntakeExisting();
                    //newIntake.id = item.id;
                    //newIntake.collegeId = item.collegeId;
                    //newIntake.academicYearId = item.academicYearId;
                    //newIntake.shiftId = item.shiftId;
                    //newIntake.isActive = item.isActive;
                    //newIntake.nbaFrom = item.nbaFrom;
                    //newIntake.nbaTo = item.nbaTo;
                    //newIntake.specializationId = item.specializationId;
                    //newIntake.Specialization = jntuh_specialization.Where(s => s.id == item.specializationId).Select(s => s.specializationName).FirstOrDefault();
                    //newIntake.DepartmentID = jntuh_specialization.Where(s => s.id == item.specializationId).Select(s => s.departmentId).FirstOrDefault();
                    //newIntake.Department = jntuh_department.Where(d => d.id == newIntake.DepartmentID).Select(d => d.departmentName).FirstOrDefault();
                    //newIntake.degreeID = jntuh_department.Where(d => d.id == newIntake.DepartmentID).Select(d => d.degreeId).FirstOrDefault();
                    //newIntake.Degree = jntuh_degree.Where(d => d.id == newIntake.degreeID).Select(d => d.degree).FirstOrDefault();
                    //newIntake.degreeDisplayOrder = jntuh_degree.Where(d => d.id == newIntake.degreeID).Select(d => d.degreeDisplayOrder).FirstOrDefault();
                    //newIntake.shiftId = item.shiftId;
                    //newIntake.Shift = jntuh_shift.Where(s => s.id == item.shiftId).Select(s => s.shiftName).FirstOrDefault();
                    //newIntake.AICTEApprovalLettr = AICTEApprovalLettr;
                    //collegeIntakeExisting.Add(newIntake);
                }
                Intake += "</table>";


            }
            return Intake;

        }
        #endregion

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
                    //if (regdata.Absent == true)
                    //{
                    //    Reason = "NOT AVAILABLE" + ",";
                    //}
                    //if (regdata.NotQualifiedAsperAICTE == true)
                    //{
                    //    Reason += "NOT QUALIFIED " + ",";
                    //}
                    //if (regdata.InvalidPANNumber == true)
                    //{
                    //    Reason += "NO PAN" + ",";
                    //}
                    //if (regdata.FalsePAN == true)
                    //{
                    //    Reason += "FALSE PAN" + ",";
                    //}
                    //if (regdata.NoSCM == true)
                    //{
                    //    Reason += "NO SCM/RATIFICATION" + ",";
                    //}
                    //if (regdata.IncompleteCertificates == true)
                    //{
                    //    Reason += "Incomplete Certificates" + ",";
                    //}
                    //if (regdata.PHDundertakingnotsubmitted == true)
                    //{
                    //    Reason += "No Undertaking" + ",";
                    //}
                    //if (regdata.Blacklistfaculy == true)
                    //{
                    //    Reason += "Blacklisted" + ",";
                    //}
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
                        // facultydata.DeactivationNew = "Principal No Deficiency";

                    }

                }
            }

            else
            {
                Reason = "NOT AVAILABLE";
                facultydata.DeactivationNew = "Yes";
            }


            var collegeStatus = db.jntuh_collegestatus.Where(e => e.CollegeId == collegeID).Select(e => e).FirstOrDefault();
            if (collegeStatus != null)
            {
                //if (collegeStatus.CollegeStatus == true || collegeStatus.AICTEStatus == true)
                //{


                //}
                //else
                //{
                principal += "<table width='100%' border='0' cellpadding='5' cellspacing='0'>";
                principal += "<tr>";
                //principal += "<td align='left'><b>Principal: </b><img alt='' src='http://112.133.193.228:75//Content/Images/checkbox_no.png' height='16' width='16' /> Qualified &nbsp; <img alt='' src='http://112.133.193.228:75//Content/Images/checkbox_no.png' height='16' width='16' /> Ratified &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; Deficiency: <img alt='' src='http://112.133.193.228:75//Content/Images/checkbox_no.png' height='16' width='16' /> Yes <img alt='' src='http://112.133.193.228:75//Content/Images/checkbox_no.png' height='16' width='16' /> No";
                principal += "<td align='left'><b>Principal: </b>" + Reason +
                             "&nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;";
                //<b> Reason: </b>" + OriginalReason
                if (!string.IsNullOrEmpty(facultydata.DeactivationNew))
                    principal += "<b> Deficiency: </b>" + facultydata.DeactivationNew +
                                 "&nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;";


                if (!string.IsNullOrEmpty(OriginalReason))
                    principal += "<b> Reason: </b>" + OriginalReason;
                principal += "</td>";
                principal += "</tr>";

                principal += "</table>";
                //}
            }
            return principal;
        }


        private int IntakeWisePhdForBtech(int Intake)
        {
            int Phdcount = 0;
            if (Intake > 0 && Intake <= 60)
            {
                Phdcount = 0;
            }
            else if (Intake > 60 && Intake <= 120)
            {
                Phdcount = 1;
            }
            else if (Intake > 120 && Intake <= 180)
            {
                Phdcount = 2;
            }
            else if (Intake > 180 && Intake <= 240)
            {
                Phdcount = 3;
            }
            else if (Intake > 240 && Intake <= 300)
            {
                Phdcount = 4;
            }
            else if (Intake > 300 && Intake <= 360)
            {
                Phdcount = 5;
            }
            else if (Intake > 360 && Intake <= 420)
            {
                Phdcount = 6;
            }
            return Phdcount;
        }

        private int IntakeWisePhdForMtech(int Intake, int shiftid)
        {
            int Phdcount = 0;
            if (shiftid == 1)
            {
                if (Intake > 0 && Intake <= 30)
                {
                    Phdcount = 2;
                }
                else if (Intake > 30 && Intake <= 60)
                {
                    Phdcount = 4;
                }
            }
            else
            {
                if (Intake > 0 && Intake <= 30)
                {
                    Phdcount = 1;
                }
                else if (Intake > 30 && Intake <= 60)
                {
                    Phdcount = 2;
                }
            }

            return Phdcount;
        }


        private int GetSectionBasedonIntake(int Intake)
        {
            int Section = 0;

            if (Intake >= 0 && Intake <= 60)
            {
                Section = 1;
            }
            else if (Intake > 60 && Intake <= 120)
            {
                Section = 2;
            }
            else if (Intake > 120 && Intake <= 180)
            {
                Section = 3;
            }
            else if (Intake > 180 && Intake <= 240)
            {
                Section = 4;
            }
            else if (Intake > 240 && Intake <= 300)
            {
                Section = 5;
            }
            else if (Intake > 300 && Intake <= 360)
            {
                Section = 6;
            }
            else if (Intake > 360 && Intake <= 420)
            {
                Section = 7;
            }
            return Section;
        }

        private int IntakeWisePhdForMBAandMCA(int Intake)
        {
            int Phdcount = 0;
            if (Intake > 0 && Intake <= 60)
            {
                Phdcount = 1;
            }
            else if (Intake > 60 && Intake <= 120)
            {
                Phdcount = 2;
            }
            else if (Intake > 120 && Intake <= 180)
            {
                Phdcount = 3;
            }
            else if (Intake > 180 && Intake <= 240)
            {
                Phdcount = 4;
            }
            else if (Intake > 240 && Intake <= 300)
            {
                Phdcount = 5;
            }
            else if (Intake > 300 && Intake <= 360)
            {
                Phdcount = 6;
            }
            else if (Intake > 360 && Intake <= 420)
            {
                Phdcount = 7;
            }

            return Phdcount;
        }

        private int Max(int AdmittedIntake2, int AdmittedIntake3, int AdmittedIntake4)
        {
            return Math.Max(AdmittedIntake2, Math.Max(AdmittedIntake3, AdmittedIntake4));
        }

        private int GetBtechAdmittedIntake(int Intake)
        {
            int BtechAdmittedIntake = Intake;
            if (Intake >= 0 && Intake <= 60)
            {
                BtechAdmittedIntake = 60;
            }
            else if (Intake > 60 && Intake <= 120)
            {
                BtechAdmittedIntake = 120;
            }
            else if (Intake > 120 && Intake <= 180)
            {
                BtechAdmittedIntake = 180;
            }
            else if (Intake > 180 && Intake <= 240)
            {
                BtechAdmittedIntake = 240;
            }
            else if (Intake > 240 && Intake <= 300)
            {
                BtechAdmittedIntake = 300;
            }
            else if (Intake > 300 && Intake <= 360)
            {
                BtechAdmittedIntake = 360;
            }
            else if (Intake > 360 && Intake <= 420)
            {
                BtechAdmittedIntake = 420;
            }
            return BtechAdmittedIntake;
        }

        private int GetIntake(int collegeId, int academicYearId, int specializationId, int shiftId, int flag, int DegreeId)
        {
            int intake = 0;
            //Degree B.Tech  
            if (DegreeId == 4)
            {
                //admitted
                if (flag == 1 && (academicYearId == 10 || academicYearId == 9 || academicYearId == 8 || academicYearId == 3))
                {
                    intake = db.jntuh_college_intake_existing.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == shiftId).Select(i => i.admittedIntake).FirstOrDefault();

                }
                else if (flag == 2 && (academicYearId == 10 || academicYearId == 9 || academicYearId == 8 || academicYearId == 3))
                {
                    intake = db.jntuh_college_intake_existing.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == shiftId).Select(i => i.aicteApprovedIntake).FirstOrDefault();

                }
                else if (flag == 1 && academicYearId == 11)
                {
                    var inta = db.jntuh_college_intake_existing.FirstOrDefault(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == shiftId);
                    if (inta != null)
                    {
                        intake = Convert.ToInt32(inta.proposedIntake);
                    }

                }
                else if (flag == 3)//Exam Branch Regular Intake
                {
                    var inta = db.jntuh_college_intake_existing.FirstOrDefault(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == shiftId);
                    if (inta != null)
                    {
                        intake = Convert.ToInt32(inta.admittedIntakeasperExambranch_R);
                    }

                }
                else   //approved
                {
                    intake = db.jntuh_college_intake_existing.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == shiftId).Select(i => i.approvedIntake).FirstOrDefault();
                }
            }
            else
            {
                //admitted
                if (flag == 1 && academicYearId != 11)
                {
                    intake = db.jntuh_college_intake_existing.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == shiftId).Select(i => i.admittedIntake).FirstOrDefault();

                }
                else if (flag == 1 && academicYearId == 11)
                {
                    var inta = db.jntuh_college_intake_existing.FirstOrDefault(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == shiftId);
                    if (inta != null)
                    {
                        intake = Convert.ToInt32(inta.proposedIntake);
                    }

                }
                else if (flag == 2) //AICTE
                {
                    if (academicYearId == 11)
                    {
                        intake = 0;
                    }
                    else
                    {
                        intake = db.jntuh_college_intake_existing.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == shiftId).Select(i => i.aicteApprovedIntake).FirstOrDefault();
                    }
                }
                else //JNTU Approved
                {
                    intake = db.jntuh_college_intake_existing.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == shiftId).Select(i => i.approvedIntake).FirstOrDefault();
                }
            }
            return intake;
        }

        private int PharmacyGetIntake(int collegeId, int academicYearId, int specializationId, int shiftId, int flag, int DegreeId)
        {
            int intake = 0;

            //Degree B.Tech  
            if (DegreeId == 4)
            {
                //admitted
                if (flag == 1 && (academicYearId == 10 || academicYearId == 9 || academicYearId == 8))
                {
                    intake = db.jntuh_college_intake_existing.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == shiftId).Select(i => i.admittedIntake).FirstOrDefault();

                }
                else if (flag == 1 && academicYearId == 11)
                {
                    var inta = db.jntuh_college_intake_existing.FirstOrDefault(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == shiftId);
                    if (inta != null)
                    {
                        intake = Convert.ToInt32(inta.proposedIntake);
                    }

                }
                else   //approved
                {
                    intake = db.jntuh_college_intake_existing.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == shiftId).Select(i => i.approvedIntake).FirstOrDefault();
                }
            }
            else
            {
                //approved
                if (flag == 1 && academicYearId != 11)
                {
                    intake = db.jntuh_college_intake_existing.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == shiftId).Select(i => i.approvedIntake).FirstOrDefault();

                }
                else if (flag == 1 && academicYearId == 11)
                {
                    var inta = db.jntuh_college_intake_existing.FirstOrDefault(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == shiftId);
                    if (inta != null)
                    {
                        intake = Convert.ToInt32(inta.proposedIntake);
                    }

                }
                else //admitted
                {
                    intake = db.jntuh_college_intake_existing.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == shiftId).Select(i => i.admittedIntake).FirstOrDefault();
                }
            }
            return intake;
        }

        private bool GetAcademicYear(int collegeId, int academicYearId, int specializationId, int shiftId, int DegreeId)
        {
            var firstOrDefault = db.jntuh_college_intake_existing.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == shiftId).Select(i => i.courseAffiliatedStatus).FirstOrDefault();
            return firstOrDefault ?? false;
        }

        public string DeficiencyCollegeLabsAnnexure(int? collegeID)
        {
            string annexure = string.Empty;
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int prAy =
                db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
            var collegeStatus = db.jntuh_collegestatus.Where(e => e.CollegeId == collegeID).Select(e => e).FirstOrDefault();
            if (collegeStatus != null)
            {
                //if (collegeStatus.CollegeStatus == false && collegeStatus.AICTEStatus == false)
                //{
                List<FacultyVerificationController.AnonymousLabclass> collegeLabAnonymousLabclass = new List<FacultyVerificationController.AnonymousLabclass>();
                //Commented by Narayana
                //List<int> specializationIds =db.jntuh_college_intake_existing.Where(e => e.collegeId == collegeID && e.courseStatus != "Closure" && e.academicYearId == 9 && e.proposedIntake!=0).Select(e => e.specializationId).Distinct().ToList();
                List<int> specializationIds = db.jntuh_college_intake_existing.Where(e => e.collegeId == collegeID && e.courseStatus != "Closure" && e.academicYearId == prAy && e.proposedIntake != 0).Select(e => e.specializationId).Distinct().ToList();
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
                    collegeLabAnonymousLabclass = db.jntuh_lab_master.AsNoTracking().Where(l => l.CollegeId == collegeID && specializationIds.Contains(l.SpecializationID) && !Equipmentsids.Contains(l.id)).Select(l => new FacultyVerificationController.AnonymousLabclass
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
                    if (specializationIds.Contains(33) || specializationIds.Contains(43))
                    {
                        collegeLabAnonymousLabclass =
                            db.jntuh_lab_master.AsNoTracking()
                                .Where(
                                    l =>
                                        specializationIds.Contains(l.SpecializationID) && !Equipmentsids.Contains(l.id) &&
                                        l.CollegeId == null && !l.EquipmentName.Contains("Desirable") && !l.EquipmentName.Contains("optional"))
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
                    else
                    {
                        collegeLabAnonymousLabclass =
                              db.jntuh_lab_master.AsNoTracking()
                                  .Where(
                                      l =>
                                          specializationIds.Contains(l.SpecializationID) && !Equipmentsids.Contains(l.id) &&
                                          l.CollegeId == null && l.Labcode != "PH105BS" && !l.EquipmentName.Contains("Desirable") && !l.EquipmentName.Contains("optional"))
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
                }


                //var collegeEquipments = db.jntuh_college_laboratories_dataentry2.Where(l => l.CollegeID == collegeID).Select(l => l.EquipmentID).Distinct().ToArray();

                //var list = collegeLabMaster.Where(c => !collegeEquipments.Contains(c.EquipmentID)).Select(c => new { EquipmentID = c.id, LabCode = c.Labcode, LabName = c.LabName, EquipmentName = c.EquipmentName })
                //                           .OrderBy(c => c.LabName).ThenBy(c => c.EquipmentName).Distinct().ToList();

                //var labDeficiencies = db.jntuh_college_laboratories_deficiency.Where(ld => ld.CollegeId == collegeID && ld.Deficiency == true).Select(ld => ld.LabCode).ToArray();

                var list1 = collegeLabAnonymousLabclass.OrderBy(c => c.LabName).ThenBy(c => c.EquipmentName).Distinct().ToList();


                // list = list.Where(l => labDeficiencies.Contains(l.LabCode)).ToList();
                //list1 = list1.Where(l => labDeficiencies.Contains(l.LabCode)).ToList();

                #region this code written by suresh

                int?[] labequipmentIds = list1.Select(i => i.EquipmentID).ToArray();

                int[] clgequipmentIDs = db.jntuh_college_laboratories.Where(l => l.CollegeID == collegeID && labequipmentIds.Contains(l.EquipmentID)).Select(i => i.EquipmentID).ToArray();

                list1 = list1.Where(l => !clgequipmentIDs.Contains((int)l.EquipmentID)).ToList();


                #endregion

                int[] SpecializationIDs;
                if (DegreeIds.Contains(4))
                    SpecializationIDs = (from a in collegeLabAnonymousLabclass orderby a.Department select a.specializationId).Distinct().ToArray();
                //labs.Select(l => l.specializationId).Distinct().ToArray();
                else
                    SpecializationIDs = (from a in collegeLabAnonymousLabclass where a.specializationId != 39 orderby a.Department select a.specializationId).Distinct().ToArray();


                //list

                int CheckLabs = 0;
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
                            // int indexnow = list1.IndexOf(item);



                            //if (indexnow > 0 && list1[indexnow].LabName == list1[indexnow - 1].LabName)

                            //    LabsCount++;

                            //else if (indexnow == 0 && (list1[indexnow].LabName == null || list1[indexnow].LabName == ""))
                            //    LabsCount++;

                            //if (indexnow > 0 && list1[indexnow].EquipmentName == list1[indexnow - 1].EquipmentName)

                            //    EquipmentsCount++;

                            //else if (indexnow == 0 && (list1[indexnow].EquipmentName == null || list1[indexnow].EquipmentName == ""))
                            //    EquipmentsCount++;

                            //if (string.IsNullOrEmpty(item.LabName.Trim()) && LabsCount > 0)
                            //{
                            //    //if (indexnow > 0 && (item.LabName.Trim() == null ||item.LabName.Trim() == ""))
                            //    //    LabNmae = "No Labs Uploaded";
                            //}
                            //else
                            //{
                            //    LabNmae = item.LabName.Trim() != null ? item.year + "-" + item.Semester + "-" + item.LabName : null;
                            //}
                            //if (string.IsNullOrEmpty(item.EquipmentName) && EquipmentsCount > 0)
                            //{
                            //    //if (indexnow > 0 && (item.EquipmentName == null || item.EquipmentName == ""))
                            //    //    LabNmae = "No Equipments Uploaded";
                            //}
                            //else
                            //{
                            //    EquipmentName = item.EquipmentName;
                            //}
                            //if (string.IsNullOrEmpty(item.Department))
                            //{
                            //    DepartmentName = item.Department;
                            //}
                            //else
                            //{
                            //    DepartmentName = "";
                            //}


                            annexure += "<tr>";
                            annexure += "<td align='left'>" + indexnow + "</td><td  align='left'>" + LabNmae + "</td><td  align='left'>" + EquipmentName + "</td>";
                            annexure += "</tr>";



                            //if (string.IsNullOrEmpty(item.LabName))
                            //    LabsCount++;
                            //if (string.IsNullOrEmpty(item.EquipmentName))
                            //    EquipmentsCount++;

                            // annexure += "<td align='left'>" + (list1.IndexOf(item) + 1) + "</td><td rowspan='" + LabsCount + "' align='left'>" + LabNmae + "</td><td rowspan='" + EquipmentsCount + "' align='left'>" + EquipmentName + "</td>";\

                            #region code

                            //if (indexnow != list1.Count() - 1)
                            //{
                            //    annexure += "<tr>";
                            //    annexure += "<td align='left'>" + (list1.IndexOf(item) + 1) + "</td>";
                            //    if (list1[indexnow].LabName != list1[indexnow + 1].LabName)
                            //    {
                            //        // annexure += "<tr>";
                            //        //<td align='left'>" + (list1.IndexOf(item) + 1) + "</td>
                            //        annexure += "<td rowspan='" + LabsCount + "' align='left'>" + LabNmae + "</td><td rowspan='" + EquipmentsCount + "' align='left'>" + EquipmentName + "</td>";
                            //        // annexure += "</tr>";
                            //    }
                            //    annexure += "</tr>";
                            //}
                            //else
                            //{
                            //    if (list1[indexnow].LabName != list1[indexnow].LabName)
                            //    {
                            //        //annexure += "<tr>";
                            //        //annexure += "<td align='left'>" + (list1.IndexOf(item) + 1) + "</td><td rowspan='" + LabsCount + "' align='left'>" + LabNmae + "</td><td rowspan='" + EquipmentsCount + "' align='left'>" + EquipmentName + "</td>";
                            //        //annexure += "</tr>";
                            //        annexure += "<tr>";
                            //        annexure += "<td align='left'>" + (list1.IndexOf(item) + 1) + "</td>";
                            //        if (list1[indexnow].LabName != list1[indexnow + 1].LabName)
                            //        {
                            //            // annexure += "<tr>";
                            //            //<td align='left'>" + (list1.IndexOf(item) + 1) + "</td>
                            //            annexure += "<td rowspan='" + LabsCount + "' align='left'>" + LabNmae + "</td><td rowspan='" + EquipmentsCount + "' align='left'>" + EquipmentName + "</td>";
                            //            // annexure += "</tr>";
                            //        }
                            //        annexure += "</tr>";
                            //    }
                            //}

                            #endregion



                            // annexure += "<td align='left'>" + (list1.IndexOf(item) + 1) + "</td><td  align='left'>" + LabNmae + "</td><td  align='left'>" + EquipmentName + "</td>";
                            //annexure += "<td align='left'>" + (list.IndexOf(item) + 1) + "</td><td align='left'>" + item.LabCode + "</td><td align='left'>" + item.LabName + "</td><td align='left'>" + item.EquipmentName + "</td>";

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

                    CheckLabs = 1;

                }

                if(CheckLabs == 1)
                {
                    annexure = string.Empty;
                }

                //Deficiencies Labs Video Verification is commented by Narayana on 31/03/2018

                //var CollegelaboratoriesInActive = (from a in db.jntuh_college_laboratories
                //    join b in db.jntuh_lab_master on a.EquipmentID equals b.id
                //    where a.CollegeID == collegeID && a.isActive == false
                //    select new
                //    {
                //        LabName = b.LabName,
                //        SpecId = b.SpecializationID,
                //        EquipmentNAme = a.EquipmentName,
                //        Year = b.Year,
                //        Semester = b.Semester
                //    }).ToList();
                //if (CollegelaboratoriesInActive.Count != 0)
                //{
                //    annexure += "<table width='100%' border='0' cellpadding='5' cellspacing='0'>";
                //    annexure += "<tr>";
                //    annexure += "<td align='left'><b><u>Deficiencies Labs Video Verification</u></b></td>";
                //    annexure += "</tr>";
                //    annexure += "</table>";
                //    var SpecIds = CollegelaboratoriesInActive.Select(e => e.SpecId).Distinct().ToArray();
                //    var specializations =
                //        db.jntuh_specialization.Where(it => SpecIds.Contains(it.id)).Select(s => new
                //        {
                //            s.id,
                //            specialization = s.specializationName,
                //            department = s.jntuh_department.departmentName,
                //            degree = s.jntuh_department.jntuh_degree.degree,
                //            deptId = s.jntuh_department.id,

                //        }).OrderBy(e => e.deptId).ToList();


                //    foreach (var Id in SpecIds)
                //    {
                //        var specializationDetails =
                //            specializations.Where(e => e.id == Id).Select(e => e).FirstOrDefault();
                //        annexure += "<table width='100%' border='1' cellpadding='5' cellspacing='0'>";
                //        annexure += "<tr>";
                //        annexure += "<th align='left' colspan='3'> " + specializationDetails.degree + " -" +
                //                    specializationDetails.department + "-" + specializationDetails.specialization +
                //                    "</th>";
                //        annexure += "</tr>";
                //        annexure += "<tr>";
                //        annexure +=
                //            "<th align='left'>S.No</th><th align='left'>Lab Name</th><th align='left'>Equipment Name</th>";
                //        annexure += "</tr>";
                //        var CollegeInactiveLabsSpecializationWise =
                //            CollegelaboratoriesInActive.Where(e => e.SpecId == Id).Select(e => e).ToList();
                //        int count = 1;
                //        foreach (var data in CollegeInactiveLabsSpecializationWise)
                //        {
                //            annexure += "<tr>";
                //            annexure += "<td align='left'>" + count + "</td>";
                //            annexure += "<td align='left'>" + data.Year + "-" + data.Semester + "-" + data.LabName +
                //                        "</td>";
                //            annexure += "<td align='left'>" + data.EquipmentNAme + "</td>";
                //            // annexure += "<td align='left'>" + "" + "</td>";
                //            annexure += "</tr>";
                //            count++;
                //        }
                //        annexure += "</table>";
                //    }
                //}
                //else
                //{
                //    annexure += "<table width='100%' border='0' cellpadding='5' cellspacing='0'>";
                //    annexure += "<tr>";
                //    annexure += "<td align='left'><b><u>Deficiencies In Labs Video Verification</u></b></td>";
                //    annexure += "</tr>";
                //    annexure += "</table>";
                //    annexure += "<table width='100%' border='1'  cellspacing='0'>";
                //    annexure += "<tr><td align='center'> <b>NIL</b></td></tr>";
                //    annexure += "</table>";
                //}








                //annexure += "</br><table width='100%'  cellspacing='0'></br>";
                //annexure += "<tr><td align='right' width='80%' style='font-size: 14px; font-weight: normal;'> <b>Sd /-</b></td></tr>";
                //annexure += "<tr><td align='right' width='80%' style='font-size: 14px; font-weight: normal;'> <b>REGISTRAR</b></td></tr></br></br>";
                //annexure += "</br><tr><td align='center' width='80%' style='font-size: 14px; font-weight: normal;' colspan='2'><b>The College shall submit Appeal, if any through Online mode only in the format " +
                //           "prescribed within 10 Days from the date of this letter." + "</b></td></tr></br>";
                //annexure += "<tr><td></td></tr>"; annexure += "</table>";
            }
            //}
            return annexure;
        }

        public string DeficiencyPhysicalLabs(int? collegeID)
        {
            string annexure = string.Empty;


            var collegeStatus = db.jntuh_collegestatus.Where(e => e.CollegeId == collegeID).Select(e => e).FirstOrDefault();
            if (collegeStatus != null)
            {
                if (collegeStatus.CollegeStatus == false && collegeStatus.AICTEStatus == false)
                {

                    List<physicalLabs> LabMaster = new List<physicalLabs>();
                    List<physicalLabs> CollegePhysicalLabMaster = new List<physicalLabs>();
                    //var jntuh_college_intake_existings = db.jntuh_college_intake_existing.Where(e => e.collegeId == collegeID && e.courseStatus != "Closure" && e.academicYearId == 9).Select(e => e).ToArray();

                    //int[] specializationIds = jntuh_college_intake_existings.Where(e => e.collegeId == collegeID && e.courseStatus != "Closure" && e.academicYearId == 9).Select(e => e.specializationId).ToArray();
                    //string CollegeAffiliationStatus = db.jntuh_college_affiliation.Where(u => u.collegeId == collegeID && u.affiliationTypeId == 7).Select(u => u.affiliationStatus).FirstOrDefault();
                    //var jntuh_lab_masters = db.jntuh_lab_master.Where(L => L.DegreeID == 4 && specializationIds.Contains(L.SpecializationID)).GroupBy(L => new { L.Labcode, L.DepartmentID }).Select(L => L.FirstOrDefault()).ToList();

                    //if (CollegeAffiliationStatus == "Yes")
                    //    jntuh_lab_masters = jntuh_lab_masters.Where(L => L.CollegeId == collegeID).Select(L => L).ToList();
                    //else
                    //    jntuh_lab_masters = jntuh_lab_masters.Where(L => specializationIds.Contains(L.SpecializationID) && L.CollegeId == null).Select(L => L).ToList();
                    //string[] LabMaserLabCodes = jntuh_lab_masters.Select(L => L.Labcode).Distinct().ToArray();
                    //var jntuh_physical_labmasters = db.jntuh_physical_labmaster.Where(P => P.Collegeid == collegeID).GroupBy(P => new { P.Labcode, P.DepartmentId }).Select(P => P.FirstOrDefault()).ToList();
                    //var jntuh_degrees = db.jntuh_degree.Where(D => D.isActive == true).Select(D => D).ToList();
                    //LabMaster = jntuh_lab_masters.Select(L => new physicalLabs
                    //{
                    //    LabCode = L.Labcode,
                    //    Labname = L.LabName,
                    //    specializationid = L.SpecializationID,
                    //    departmentid = L.DepartmentID,
                    //    degreeid = L.DegreeID,
                    //    collegeId = L.CollegeId
                    //}).ToList();
                    ////.GroupBy(L => new { L.LabCode, L.department })
                    //foreach (var item in LabMaster)
                    //{
                    //    physicalLabs CollegephysicalLabs = new physicalLabs();
                    //    CollegephysicalLabs.Intake = jntuh_college_intake_existings.Where(E => E.specializationId == item.specializationid).Select(E => E.proposedIntake).FirstOrDefault();
                    //    if (CollegephysicalLabs.Intake > 120)
                    //    {
                    //        CollegephysicalLabs.NoOfAvailabeLabs = jntuh_physical_labmasters.Where(P => P.DepartmentId == item.departmentid && P.Numberofavilablelabs > 1).Sum(P => P.Numberofavilablelabs);
                    //        if (CollegephysicalLabs.NoOfAvailabeLabs < 4)
                    //        {
                    //            CollegephysicalLabs.LabCode = item.LabCode;
                    //            CollegephysicalLabs.Labname = item.Labname;
                    //            CollegephysicalLabs.departmentid = item.departmentid;
                    //            CollegephysicalLabs.specializationid = item.specializationid;
                    //            CollegephysicalLabs.NoOfRequiredLabs = 4;
                    //            CollegephysicalLabs.NoOfAvailabeLabs = CollegephysicalLabs.NoOfAvailabeLabs;//item.NoOfAvailabeLabs;
                    //            CollegephysicalLabs.degree = jntuh_degrees.Where(D => D.id == item.degreeid).Select(D => D.degree).FirstOrDefault();
                    //            CollegePhysicalLabMaster.Add(CollegephysicalLabs);
                    //        }

                    //    }


                    //}
                    //int?[] specializationIDS = CollegePhysicalLabMaster.Select(L => L.specializationid).Distinct().ToArray();
                    // var CollegePhysicalLabMasters=CollegePhysicalLabMaster.ToList();
                    int indexnow = 1;
                    string Department = "", Degree = "", Specialization = "";

                    //var jntuh_department = db.jntuh_department.Where(e => e.isActive == true).Select(e => e).ToList();
                    //var jntuh_specialization = db.jntuh_specialization.Where(e => e.isActive == true).Select(e => e).ToList();

                    //Old Code Comment on 29-03-2018
                    //CollegePhysicalLabMaster =
                    //    db.jntuh_college_laboratories_deficiency.Where(e => e.CollegeId == collegeID)
                    //        .Select(e => new physicalLabs
                    //        {
                    //            department = e.LabCode,
                    //            NoOfAvailabeLabs = e.Semister
                    //        }).ToList();

                    CollegePhysicalLabMaster =
                        db.jntuh_physical_labmaster_copy.Where(e => e.Collegeid == collegeID && e.Numberofrequiredlabs != null)
                            .Select(e => new physicalLabs
                            {
                                department = db.jntuh_department.Where(d => d.id == e.DepartmentId).Select(s => s.departmentName).FirstOrDefault(),
                                NoOfRequiredLabs = e.Numberofrequiredlabs,
                                Labname = e.LabName,
                                year = e.Year,
                                semister = e.Semister,
                                LabCode = e.Labcode,
                                NoOfAvailabeLabs = e.Numberofavilablelabs == null ? 0 : e.Numberofavilablelabs
                            }).ToList();
                    //CollegePhysicalLabMaster =
                    //    db.jntuh_physical_labmaster_copy.Where(e => e.Collegeid == collegeID&&e.Numberofrequiredlabs!=null)
                    //        .Select(e => new physicalLabs
                    //        {
                    //            department =db.jntuh_department.Where(d=>d.id==e.DepartmentId).Select(s=>s.departmentName).FirstOrDefault(),
                    //            NoOfRequiredLabs = e.Numberofrequiredlabs,
                    //            Labname = e.LabName,
                    //            year = e.Year,
                    //            semister = e.Semister,
                    //            LabCode = e.Labcode,
                    //            NoOfAvailabeLabs = db.jntuh_physical_labmaster_copy.Where(a => a.Collegeid == collegeID &&  a.Semister == e.Semister && a.Year == e.Year && a.Labcode == e.Labcode).Select(s => s.Numberofavilablelabs).FirstOrDefault() == null ? 0 : db.jntuh_physical_labmaster_copy.Where(a => a.Collegeid == collegeID && a.DepartmentId == e.DepartmentId && a.Semister == e.Semister && a.Year == e.Year && a.Labcode == e.Labcode).Select(s => s.Numberofavilablelabs).FirstOrDefault()
                    //        }).ToList();



                    annexure += "<table width='100%' border='0' cellpadding='5' cellspacing='0'>";
                    annexure += "<tr>";
                    annexure += "<td align='left'><b><u>Deficiencies in Physical Labs</u></b></td>";
                    annexure += "</tr>";
                    annexure += "</table>";
                    annexure += "<table width='100%' border='1' cellpadding='7' cellspacing='0'>";

                    annexure += "<tr>";
                    annexure +=
                        "<th align='left'>S.No</th><th align='left'>Department</th><th align='left'>Lab Name</th><th align='left'>Required Extra labs</th><th align='left'>Available Extra Labs</th><th align='left'>Deficiency</th>";
                    annexure += "</tr>";

                    int CheckPhysicalLabs = 0;
                    if (CollegePhysicalLabMaster.Count != 0)
                    {
                        foreach (var item in CollegePhysicalLabMaster)
                        {
                            if (item.NoOfAvailabeLabs < item.NoOfRequiredLabs)
                            {
                                annexure += "<tr>";
                                annexure += "<td align='left'>" + indexnow + "</td><td  align='left'>" + item.department +
                                            "</td><td  align='left'>" + item.year + "-" + item.semister + "--" + item.Labname +
                                            "</td><td  align='left'>" + item.NoOfRequiredLabs + "</td><td  align='left'>" +
                                            item.NoOfAvailabeLabs + "</td><td>Yes</td>";
                                annexure += "</tr>";
                                indexnow++;
                            }

                        }
                        if (indexnow == 1)
                        {
                            annexure += "<tr>";
                            annexure += "<td colspan='6' align='center'><b>NIL</b></td>";
                            annexure += "</tr>";
                            CheckPhysicalLabs = 1;
                        }
                    }
                    else
                    {
                        annexure += "<tr>";
                        annexure += "<td colspan='6' align='center'><b>NIL</b></td>";
                        annexure += "</tr>";
                        CheckPhysicalLabs = 1;
                    }


                    annexure += "</table>";

                    if(CheckPhysicalLabs == 1)
                    {
                        annexure = string.Empty;
                    }


                    //foreach (var speclializationId in specializationIDS)
                    //{
                    //    if (speclializationId != null)
                    //    {
                    //        var CollegePhysicalLabMasters = CollegePhysicalLabMaster.Where(L => L.specializationid == speclializationId).Select(L => L).FirstOrDefault();
                    //        Department = jntuh_department.Where(e => e.id == CollegePhysicalLabMasters.departmentid).Select(e => e.departmentName).FirstOrDefault();
                    //        Specialization = jntuh_specialization.Where(e => e.id == CollegePhysicalLabMasters.specializationid).Select(e => e.specializationName).FirstOrDefault();
                    //        Degree = CollegePhysicalLabMasters.degree;
                    //        //annexure += "<table width='100%' border='1' cellpadding='7' cellspacing='0'>";
                    //        //annexure += "<tr>";
                    //        //annexure += "<th align='center' colspan='5'>LIST OF EQUIPMENT NOT AVAILABLE IN " + Degree + " -" + Department + "-" + Specialization + "</th>";
                    //        //annexure += "</tr>";
                    //        //annexure += "<tr>";
                    //        //annexure += "<th align='left'>S.No</th><th align='left'>Lab Code</th><th align='left'>Required</th><th align='left'>Avilable</th><th align='left'>Lab Name</th>";
                    //        //annexure += "</tr>";
                    //        //foreach (var item in CollegePhysicalLabMaster.ToList())
                    //        //{
                    //        //    annexure += "<tr>";
                    //        //    annexure += "<td align='left'>" + indexnow + "</td><td  align='left'>" + item.department + "</td><td  align='left'>" + item.LabCode + "</td><td  align='left'>" + item.NoOfRequiredLabs + "</td><td  align='left'>" + item.NoOfAvailabeLabs + "</td>";
                    //        //    annexure += "</tr>";
                    //        //    indexnow++;
                    //        //}
                    //        //annexure += "</table>";
                    //    }
                    //    else
                    //        annexure = " No Deficiency PhysicalLabs ";
                    //}


                    //annexure +=
                    //    "<p><b>NOTE:</b> The Physical Verification of the faculty and their presence at the time of Inspection by the FFC, automatically does not mean that the college is entitled for Affiliation based on numbers. Those of the faculty who are having the requisite qualifications, Biometric attendance and credentials are verified and found correct will be taken into account for the purpose of granting affiliation.</p>";

                    //annexure += "</br><table width='100%'  cellspacing='0'></br>";
                    //annexure +=
                    //    "<tr><td align='right' width='80%' style='font-size: 14px; font-weight: normal;'> <b>Sd /-</b></td></tr>";
                    //annexure +=
                    //    "<tr><td align='right' width='80%' style='font-size: 14px; font-weight: normal;'> <b>REGISTRAR</b></td></tr></br></br>";
                    //annexure +=
                    //    "</br><tr><td align='center' width='80%' style='font-size: 14px; font-weight: normal;' colspan='2'><b>The College shall submit Appeal, if any through Online mode only in the format " +
                    //    "prescribed within 10 Days from the date of this letter." + "</b></td></tr></br>";
                    //annexure += "<tr><td></td></tr>";
                    //annexure += "</table>";
                }
            }
            return annexure;
        }

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
        public class GetFacultyBASDetails
        {
            public string RegistarationNumber { get; set; }
            public string BasJoiningDate { get; set; }
            public int? JulyTotalDays { get; set; }
            public int? AugustTotalDays { get; set; }
            public int? SeptemberTotalDays { get; set; }
            public int? OctoberTotalDays { get; set; }
            public int? NovemberTotalDays { get; set; }
            public int? DecemberTotalDays { get; set; }
            public int? JanuaryTotalDays { get; set; }
            public int? FebruaryTotalDays { get; set; }
            public int? MarchTotalDays { get; set; }
            public int? AprilTotalDays { get; set; }
            public int? MayTotalDays { get; set; }
            public int? JulyPresentDays { get; set; }
            public int? AugustPresentDays { get; set; }
            public int? SeptemberPresentDays { get; set; }
            public int? OctoberPresentDays { get; set; }
            public int? NovemberPresentDays { get; set; }
            public int? DecemberPresentDays { get; set; }
            public int? JanuaryPresentDays { get; set; }
            public int? FebruaryPresentDays { get; set; }
            public int? MarchPresentDays { get; set; }
            public int? AprilPresentDays { get; set; }
            public int? MayPresentDays { get; set; }
            public int? TotalWorkingDays { get; set; }
            public int? TotalPresentDays { get; set; }
        }

        public class physicalLabs
        {
            public int? id { get; set; }
            public int Labid { get; set; }
            public int? collegeId { get; set; }
            public int? degreeid { get; set; }
            public int departmentid { get; set; }
            public int? specializationid { get; set; }
            public string degree { get; set; }
            // public string degree { get; set; }
            public string department { get; set; }
            public string specialization { get; set; }
            public int? year { get; set; }
            public int? semister { get; set; }
            public string Labname { get; set; }
            public string LabCode { get; set; }
            public string Remarks { get; set; }
            public int? NoOfRequiredLabs { get; set; }
            public int? NoOfAvailabeLabs { get; set; }
            public string PhysicalLab { get; set; }
            public int physicalId { get; set; }
            public int? Intake { get; set; }


        }
    }
}
