using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using UAAAS.Models;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;
using it = iTextSharp.text;
using System.Globalization;
using iTextSharp.text.html.simpleparser;
using iTextSharp.text.html;
using iTextSharp.text.xml;
using iTextSharp.text.xml.simpleparser;
using System.Net;
using System.Configuration;
using System.Web.Security;
using Ionic.Zip;
using ZipDemo.Utils.ActionResults;
using System.Threading.Tasks;
using System.Data;
using WebGrease.Css.Extensions;
using System.Text;

namespace UAAAS.Controllers
{
    public class PgDeficiencyReportLetterController : Controller
    {
        private uaaasDBContext db = new uaaasDBContext();
        int PresentYearDb = 0;
        private int TotalAreaRequiredFaculty = 0;

        public PgDeficiencyReportLetterController()
        {
            PresentYearDb = 2023;
        }

        public ActionResult Index()
        {
            return View();
        }

        [Authorize(Roles = "Admin,SuperAdmin")]
        public ActionResult Deficiencies(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                int collegeID = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(id.ToString(), System.Web.Configuration.WebConfigurationManager.AppSettings["CryptoKey"]));
                //int[] collegeIds = { 4, 7, 8, 9, 11, 12, 22, 23, 26, 29, 32, 35, 38, 39, 40, 41, 42, 43, 48, 50, 56, 68, 69, 70, 72, 75, 77, 80, 84, 85, 86, 87, 88, 100, 103, 106, 108, 109, 111, 113, 115, 116, 123, 128, 130, 132, 134, 137, 141, 143, 144, 145, 147, 148, 152, 153, 156, 157, 158, 162, 164, 165, 166, 168, 171, 173, 175, 176, 177, 178, 179, 181, 182, 183, 184, 187, 188, 193, 195, 196, 198, 203, 210, 211, 214, 218, 222, 223, 225, 228, 242, 244, 245, 250, 254, 256, 260, 261, 264, 266, 271, 273, 276, 282, 287, 293, 299, 300, 304, 306, 307, 310, 316, 322, 324, 327, 329, 334, 335, 350, 352, 360, 364, 365, 366, 367, 373, 374, 380, 385, 391, 393, 399, 400, 403, 414, 420, 423, 429, 435, 441, 455 };
                //int[] collegeIds = { 4, 7, 9, 11, 12, 20, 22, 23, 26, 32, 35, 38, 39, 40, 41, 42, 48, 50, 56, 67, 68, 69, 70, 72, 75, 77, 80, 84, 87, 88, 100, 103, 108, 109, 111, 113, 116, 119, 123, 128, 130, 132, 134, 140, 143, 144, 145, 147, 148, 152, 153, 156, 157, 158, 162, 164, 165, 168, 171, 174, 175, 177, 178, 179, 181, 182, 183, 187, 188, 193, 195, 196, 198, 203, 210, 211, 214, 218, 222, 223, 228, 242, 246, 250, 254, 256, 260, 261, 264, 266, 271, 273, 287, 293, 299, 300, 304, 306, 307, 310, 316, 322, 324, 327, 329, 335, 338, 350, 352, 360, 364, 365, 366, 367, 373, 374, 380, 385, 393, 399, 403, 411, 414, 420, 423, 429, 430, 435, 439, 441 };
                //int[] collegeIds = { 4, 9, 11, 12, 20, 22, 23, 26, 32, 35, 38, 39, 40, 41, 48, 68, 70, 72, 75, 77, 84, 88, 108, 109, 113, 130, 132, 134, 143, 144, 145, 147, 148, 152, 153, 156, 157, 162, 171, 175, 177, 178, 179, 181, 183, 187, 193, 195, 196, 198, 203, 211, 214, 218, 222, 223, 250, 254, 256, 260, 266, 287, 300, 304, 310, 327, 335, 360, 366, 367, 373, 374, 380, 385, 393, 399, 414, 435 };
                int[] collegeIds = { 171 }; //67,119,174,246,411,430
                //int collegeID = Convert.ToInt32(id);
                //string path = SaveCollegePGLetterPdf(collegeID);
                //path = path.Replace("/", "\\");
                //if (!string.IsNullOrEmpty(path))
                //{
                //    return File(path, "application/pdf", path.Substring(path.LastIndexOf('\\') + 1));
                //}
                foreach (var item in collegeIds)
                {
                    string path = SaveCollegePGLetterPdf(item);
                }

                string[] filePaths = Directory.GetFiles(Server.MapPath("~/Content/PDFReports/UGDefReport/" + PresentYearDb + "/"));

                ZipFile zip = new ZipFile();
                zip.AlternateEncodingUsage = ZipOption.AsNecessary;
                zip.AddDirectoryByName("Faculty_PGDefReport");

                foreach (var row in filePaths)
                {
                    string filePath = row;
                    zip.AddFile(filePath, "AllCollegesFaculty_PGDefReport");
                }

                Response.Clear();
                Response.BufferOutput = false;
                string zipName = String.Format("FacultyPGDefReportSProceedings_{0}.zip", DateTime.Now.ToString("yyyy-MMM-dd-HHmmss"));
                Response.ContentType = "application/zip";
                Response.AddHeader("content-disposition", "attachment; filename=" + zipName);
                zip.Save(Response.OutputStream);
                Response.End();


                //path = path.Replace("/", "\\");
                //if (!string.IsNullOrEmpty(path))
                //{
                //    return File(path, "application/pdf", path.Substring(path.LastIndexOf('\\') + 1));
                //}
                return View();
            }

            return View();
        }

        public string SaveCollegePGLetterPdf(int collegeId)
        {
            string fullPath = string.Empty;

            var collegeCode = db.jntuh_college.Where(C => C.id == collegeId && C.isActive == true).Select(C => C.collegeCode).FirstOrDefault();
            string ECollegeid = UAAAS.Models.Utilities.EncryptString(collegeId.ToString(), System.Web.Configuration.WebConfigurationManager.AppSettings["CryptoKey"]);
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int prAy = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();

            var Updateondate = db.jntuh_college_edit_status.Where(i => i.IsCollegeEditable == false && i.collegeId == collegeId && i.academicyearId == prAy).Select(I => I.updatedOn).FirstOrDefault();
            var datetime = "";
            if (Updateondate != null)
            {
                datetime = UAAAS.Models.Utilities.MMDDYY2DDMMYY(Updateondate.ToString());
            }
            else
            {
                datetime = string.Empty;
            }
            //Set page size as A4
            var pdfDoc = new Document(PageSize.A4, 40, 40, 40, 40);

            // Create a new PdfWrite object, writing the output to a MemoryStream
            //open pdf
            var output = new MemoryStream();
            var writer = PdfWriter.GetInstance(pdfDoc, output);

            //int InspectionPhaseId = db.jntuh_inspection_phase.Where(p => p.isActive == true).Select(p => p.id).SingleOrDefault();

            string path = Server.MapPath("~/Content/PDFReports/UGDefLetters/2023-24/");

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            fullPath = path + collegeCode.ToUpper() + "-" + ECollegeid + "-PGDefLetter.pdf";
            var file = collegeCode.ToUpper() + "-" + ECollegeid + "-PGDefLetter.pdf";
            var strfullPath = string.Format("{0}/{1}", "/Content/PDFReports/UGDefLetters/2023-24/", file);

            //var check = db.jntuh_college_news.Where(n => n.navigateURL == strfullPath).ToList();
            //if (check.Count == 0 || check.Count == null)
            //{
            //    jntuh_college_news jntuh_college_news = new jntuh_college_news();
            //    jntuh_college_news.collegeId = (int)collegeId;
            //    jntuh_college_news.title = "JNTUH – UAAC – ABAS- Faculty not marking biometric attendance - Notice – Issued-Reg.";
            //    jntuh_college_news.navigateURL = strfullPath;
            //    jntuh_college_news.startDate = null;
            //    jntuh_college_news.endDate = null;
            //    jntuh_college_news.isActive = true;
            //    jntuh_college_news.isLatest = true;
            //    jntuh_college_news.createdOn = DateTime.Now;
            //    jntuh_college_news.createdBy = 450;
            //    jntuh_college_news.updatedOn = null;
            //    jntuh_college_news.updatedBy = null;

            //    db.jntuh_college_news.Add(jntuh_college_news);
            //    db.SaveChanges();
            //}

            PdfWriter.GetInstance(pdfDoc, new FileStream(fullPath, FileMode.Create));

            //Open PDF Document to write data
            pdfDoc.Open();

            //Assign Html content in a string to write in PDF
            string contents = string.Empty;

            StreamReader sr;

            //Read file from server path
            sr = System.IO.File.OpenText(Server.MapPath("~/Content/PGDeficiencyLetter.html"));

            //store content in the variable
            contents = sr.ReadToEnd();
            sr.Close();
            contents = contents.Replace("##COLLEGE_CODE##", collegeCode);

            int collegeid = db.jntuh_college.Where(c => c.collegeCode == collegeCode).Select(c => c.id).FirstOrDefault();
            jntuh_address address = db.jntuh_address.Where(a => a.collegeId == collegeid && (a.addressTye.Equals("COLLEGE"))).Select(a => a).FirstOrDefault();
            jntuh_address societyAddress = db.jntuh_address.Where(ad => (ad.collegeId == collegeid) && (ad.addressTye.Equals("SOCIETY"))).Select(ad => ad).FirstOrDefault();

            string district = string.Empty;
            if (address != null)
            {
                district = db.jntuh_district.Find(address.districtId).districtName;
            }
            string scheduleCollegeAddress = string.Empty;
            string scheduleCollegeAddress1 = string.Empty;

            string collegeName = db.jntuh_college.Where(c => c.collegeCode == collegeCode && c.isActive == true).Select(c => c.collegeName).FirstOrDefault();

            if (address != null)
            {
                scheduleCollegeAddress = collegeName + " <b>(cc:" + collegeCode + ")</b>" + ",<br />" + address.address;
                scheduleCollegeAddress = scheduleCollegeAddress + ", " + address.townOrCity;
                scheduleCollegeAddress = scheduleCollegeAddress + ", " + address.mandal;
                scheduleCollegeAddress = scheduleCollegeAddress + ",<br />" + district;
                scheduleCollegeAddress = scheduleCollegeAddress + " - " + address.pincode;
            }

            if (address != null)
            {
                scheduleCollegeAddress1 = collegeName + " <b>(cc:" + collegeCode + ")</b>" + ", " + address.address;
                scheduleCollegeAddress1 = scheduleCollegeAddress1 + ", " + address.townOrCity;
                scheduleCollegeAddress1 = scheduleCollegeAddress1 + ", " + address.mandal;
                scheduleCollegeAddress1 = scheduleCollegeAddress1 + ", " + district;
                scheduleCollegeAddress1 = scheduleCollegeAddress1 + " - " + address.pincode;
            }

            string CollegeSocietyAddress = string.Empty;
            if (societyAddress != null)
            {
                CollegeSocietyAddress = collegeName + " <b>(cc:" + collegeCode + ")</b>" + ", " + societyAddress.address;
                CollegeSocietyAddress = CollegeSocietyAddress + ", " + societyAddress.townOrCity;
                CollegeSocietyAddress = CollegeSocietyAddress + ", " + societyAddress.mandal;
                CollegeSocietyAddress = CollegeSocietyAddress + ", " + district;
                CollegeSocietyAddress = CollegeSocietyAddress + " - " + societyAddress.pincode;
            }
            scheduleCollegeAddress = scheduleCollegeAddress.ToUpper().Replace(",", ", ").Replace("(", " (").Replace(")", ") ");
            CollegeSocietyAddress = CollegeSocietyAddress.ToUpper().Replace(",", ", ").Replace("(", " (").Replace(")", ") ");
            contents = contents.Replace("##SUBMITTEDDATE##", datetime);
            contents = contents.Replace("##SOCIETY_ADDRESS##", CollegeSocietyAddress);
            contents = contents.Replace("##COLLEGE_ADDRESS##", scheduleCollegeAddress);
            contents = contents.Replace("##LETTER_DATE##", DateTime.Now.ToString("dd-MM-yyy"));
            contents = contents.Replace("##Principal_DefReason##", Principal(collegeid));
            NewDeficienciesInFacultyForTotalReqFaculty(collegeid);
            //contents = contents.Replace("##BTechCOURSE_TABLE##", NewDeficienciesInFacultyBTech(collegeid));
            contents = contents.Replace("##NPTELCOURSE_TABLE##", DepartmentWiseNPTLFaculty(collegeid));
            contents = contents.Replace("##MTechCOURSE_TABLE##", NewDeficienciesInFaculty(collegeid));
            contents = contents.Replace("##MBAMCACOURSE_TABLE##", NewDeficienciesInFacultyMBA(collegeid));
            contents = contents.Replace("##LABS_TABLE##", DeficiencyCollegeLabsAnnexure(collegeid));
            contents = contents.Replace("##DUES_TABLE##", PendingDues(collegeid));
            contents = contents.Replace("##COMPLAINTS_TABLE##", CollegeComplaints(collegeid));
            contents = contents.Replace("##Administrative_Table##", AdministrativeLandDetails(collegeid));
            contents = contents.Replace("##Instructional_Table##", InstructionalAreaDetails(collegeid));
            var parsedHtmlElements = HTMLWorker.ParseToList(new StringReader(contents), null);

            //Get each array values from parsed elements and add to the PDF document
            foreach (var htmlElement in parsedHtmlElements)
            {
                if (htmlElement.Chunks.Count == 3)
                { pdfDoc.Add(Chunk.NEXTPAGE); }

                pdfDoc.Add(htmlElement as IElement);
            }

            //Close your PDF
            pdfDoc.Close();
            if (pdfDoc.IsOpen())
            {
                pdfDoc.Close();
            }

            string returnPath = string.Empty;
            returnPath = fullPath;
            return returnPath;
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
                    facultydata.MiddleName = regdata.MiddleName != null ? regdata.MiddleName : "";
                    facultydata.LastName = regdata.LastName;
                    facultydata.RegistrationNumber = regdata.RegistrationNumber;
                    if (regdata.Absent == true)
                    {
                        OriginalReason += "Absent";
                    }
                    if (regdata.BAS == "Yes")
                    {
                        OriginalReason += "Not having Sufficient Biometric Attendance ";
                    }
                    if (!string.IsNullOrEmpty(regdata.DeactivationReason) || !string.IsNullOrEmpty(OriginalReason))
                    {
                        Reason = "Dr. " + facultydata.FirstName.First().ToString().ToUpper() + facultydata.FirstName.Substring(1) + " " + facultydata.MiddleName.ToString().ToUpper() + " " + facultydata.LastName.First().ToString().ToUpper() + facultydata.LastName.Substring(1);
                        //Reason.Substring(0, Reason.Length - 1);
                        facultydata.DeactivationNew = "Yes";
                        OriginalReason += !string.IsNullOrEmpty(OriginalReason) ? "," + regdata.DeactivationReason : regdata.DeactivationReason;
                    }
                    else
                    {
                        Reason = "Dr. " + facultydata.FirstName.First().ToString().ToUpper() + facultydata.FirstName.Substring(1) + " " + facultydata.MiddleName.ToString().ToUpper() + " " + facultydata.LastName.First().ToString().ToUpper() + facultydata.LastName.Substring(1);
                        facultydata.DeactivationNew = "";
                    }
                }
            }
            else
            {
                Reason = "NOT AVAILABLE";
                facultydata.DeactivationNew = "Yes";
            }
            //principal += "<table border='1' cellpadding='3' cellspacing='0' width='100%' style='font-family: Times New Roman; font-size: 9px; font-weight: normal; background-color: #fff; margin: 0 auto;'>";
            //principal += "<tr>";
            ////principal += "<td align='left'><b>Principal: </b><img alt='' src='http://112.133.193.228:75//Content/Images/checkbox_no.png' height='16' width='16' /> Qualified &nbsp; <img alt='' src='http://112.133.193.228:75//Content/Images/checkbox_no.png' height='16' width='16' /> Ratified &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; Deficiency: <img alt='' src='http://112.133.193.228:75//Content/Images/checkbox_no.png' height='16' width='16' /> Yes <img alt='' src='http://112.133.193.228:75//Content/Images/checkbox_no.png' height='16' width='16' /> No";
            //principal += "<td align='left'><b>Principal: </b>" + Reason + "&nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;";
            //if (!string.IsNullOrEmpty(facultydata.DeactivationNew))
            //    principal += "<b> Deficiency: </b>" + facultydata.DeactivationNew + "&nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;";
            //if (!string.IsNullOrEmpty(OriginalReason))
            //    principal += "<b> Reason: </b>" + OriginalReason;
            //principal += "</td>";
            //principal += "</tr>";
            //principal += "</table>";
            if (OriginalReason == "No SCM")
            {
                OriginalReason = "Principal Not Selected / Ratified by the University";
            }
            if (!string.IsNullOrEmpty(OriginalReason))
                principal += "Yes";//principal += OriginalReason;
            else
                principal += "No Deficiency";

            return principal;
        }


        [Authorize(Roles = "Admin,SuperAdmin")]
        public string NewDeficienciesInFacultyForTotalReqFaculty(int? collegeID)
        {
            TotalAreaRequiredFaculty = 0;
            string facultyclosure = string.Empty;
            string twintyfivepercentbelowcurces = string.Empty;
            string faculty = string.Empty;
            string facultyAdmittedIntakeZero = string.Empty;
            string facultyAdmittedIntakeZeroTable2 = string.Empty;
            int facultycount = 0;
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int prAy =
                db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
            int[] mbacollegeides = { 5, 67, 119, 174, 246, 296, 343, 355, 386, 394, 411, 413, 421, 424, 430, 449, 452 };
            int cid = Convert.ToInt32(collegeID);
            int[] shiftids = { 1, 2 };
            List<CollegeFacultyWithIntakeReport> facultyCountslist = new List<CollegeFacultyWithIntakeReport>();
            List<CollegeFacultyWithIntakeReport> facultylist = collegeFacultyNew(collegeID).Select(e => e).ToList();
            List<CollegeFacultyWithIntakeReport> facultyCounts = facultylist.Where(c => c.shiftId == 1).Select(e => e).ToList();//Where(c => c.shiftId == 1)
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
            List<CollegeFacultyWithIntakeReport> facultyCountper = facultylist.Where(c => (c.ispercentage == true && c.Proposedintake != 0 && c.Degree == "B.Tech") || c.Proposedintake == 0 && c.Degree == "B.Tech").Select(e => e).ToList();
            foreach (var itemmtech in facultyCountper)
            {
                if (itemmtech.collegeId == 72 && itemmtech.Department == "IT")
                {
                }
                else if (itemmtech.collegeId == 130 && itemmtech.Department == "IT")
                {

                }
                else
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
            }
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
                var CollegeSubmissionDate = Convert.ToDateTime(db.jntuh_college_edit_status.Where(e => e.collegeId == collegeID && e.academicyearId == prAy && e.IsCollegeEditable == false).Select(e => e.updatedOn).FirstOrDefault()).Date;
                // DateTime data = Convert.ToDateTime(CollegeSubmissionDate).Date;
                if (collegeStatus.AICTEStatus == true)
                {

                    var RegisteredFaculty = (from Reg in db.jntuh_registered_faculty
                                             join Clg in db.jntuh_college_faculty_registered on Reg.RegistrationNumber equals Clg.RegistrationNumber
                                             join ClgEdit in db.jntuh_college_edit_status on Clg.collegeId equals ClgEdit.collegeId
                                             where ClgEdit.IsCollegeEditable == false && ClgEdit.academicyearId == prAy && Clg.collegeId == collegeID
                                             select Reg).ToList();

                    int Absebtfaculty = RegisteredFaculty.Count(e => e.Absent == true);
                    int TotalFaculty = RegisteredFaculty.Count();

                    var Percentage = Math.Ceiling((((double)Absebtfaculty / (double)TotalFaculty) * 100));

                    faculty += "<p>Sub: - Affiliation for the Academic Year: 2020-21- Reg.</p>";
                    faculty += "<br/>";
                    faculty += "<table><tr><td style='vertical-align:top'>Ref:</td><td>";
                    faculty += "<ol type='1'>";
                    faculty += "<li>Your college online application dated:" + CollegeSubmissionDate + " for grant of Affiliation for the Academic Year 2017-18.</li>";
                    faculty += "<li>Your college FFC Inspection on  dated: ----------- </li>";
                    faculty += "</ol></td></tr></table>";
                    faculty += "<br/>";
                    faculty += "<p align='center'><b>****</b></p>";

                    faculty += "<p style='text-align:justfy'>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;As per the fact finding Committee inspection report it is noticed that " + Percentage + " of faculty members were absent against the number of faculty shown in online application for affiliation A117. It indicates that the College is not maintaining the minimum essential requirements for running the academic programs. Under these circumstances the SCA recommends <b><u>'No Admission Status'</u></b> for the A.Y. 2017-18 for the Institute. Clarification or explanation if any may be submitted within 10 days from the date of this letter</p>";

                    faculty += "<table width='100%'  cellspacing='0'>";
                    faculty += "<tr><td align='right' width='80%' style='font-size: 14px; font-weight: normal;'> <b>Sd /-</b></td></tr>";
                    faculty += "<tr><td align='right' width='80%' style='font-size: 14px; font-weight: normal;'> <b>REGISTRAR</b></td></tr>";
                    faculty += "</table>";


                }
                else if (collegeStatus.CollegeStatus == true)
                {
                    faculty += "<p>Sub: - Affiliation for the Academic Year: 2020-21- Reg.</p>";
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
            int remainingSpecializationFaculty = 0;
            int remainingPHDFaculty = 0;
            int remainingSpecializationwisePHDFaculty = 0;
            int SpecializationwisePHDFaculty = 0;
            int SpecializationwisePGFaculty = 0;
            int TotalCount = 0;
            int HumantitiesminimamRequireMet = 0;
            string HumantitiesminimamRequireMetStatus = "Yes";



            string[] ProposedIntakeZeroDeptName = facultyCounts.Where(e => e.Degree == "B.Tech" && (e.Proposedintake == 0) && !departments.Contains(e.Department)).Select(e => e.Specialization).Distinct().ToArray();
            var ProposedIntakeZeroDeptNameSpecIDs = facultyCounts.Where(e => e.Degree == "B.Tech" && (e.Proposedintake == 0) && !departments.Contains(e.Department)).Select(e => e.specializationId).Distinct().ToArray();
            string[] twentyfivePersentbelowDeptName = facultyCounts.Where(e => e.Degree == "B.Tech" && (e.ispercentage == true) && (e.Proposedintake != 0) && !departments.Contains(e.Department)).Select(e => e.Specialization).Distinct().ToArray();
            if (collegeStatus != null)
            {
                if (collegeStatus.CollegeStatus == false && collegeStatus.AICTEStatus == false)
                {

                    faculty += "<table align='center' width='100%' border='0' cellpadding='5' cellspacing='0'>";
                    faculty += "<tr>";
                    faculty += "<td align='left'><b><u>Deficiencies in Faculty</u></b> (Department/ Specialization Wise):";
                    faculty += "</tr>";
                    faculty += "</table>";
                    faculty += "<table  border='1' cellpadding='3' cellspacing='0' style='border-color: #ccc;'>";
                    faculty += "<tr>";
                    faculty += "<th rowspan='2' style='text-align: center; vertical-align: top;'>SNo</th>";
                    faculty += "<th rowspan='2' style='text-align: left; vertical-align: top;' >Dept</th>";
                    faculty += "<th  rowspan='2' style='text-align: left; vertical-align: top;' >Degree</th>";
                    faculty += "<th rowspan='2' style='text-align: left; vertical-align: top;' >Specialization</th>";
                    //Written by Narayana
                    //faculty += "<th rowspan='2' style='text-align: left; vertical-align: top;' >Shift</th>";
                    //if (mbacollegeides.Contains(cid))
                    //{
                    //    faculty += "<th colspan='3' rowspan='2' style='text-align: center; vertical-align: top;' >Sum of Proposed &<br/> Previous Sanctioned Intake</th>";
                    //}
                    //else
                    //{
                    //    faculty += "<th colspan='3' style='text-align: center; vertical-align: top;' >Intake of Previous Three A.Y</th>";
                    //}
                    faculty += "<th colspan='3' style='text-align: center; vertical-align: top;' >Intake of Previous Three A.Y</th>";
                    //faculty += "<th rowspan='2' style='text-align: center; vertical-align: top;' >Admitted Intake</th>";

                    faculty += "<th rowspan='2' style='text-align: center; vertical-align: top;' >Proposed Intake</th>";
                    faculty += "<th rowspan='2' style='text-align: center; vertical-align: top;' >Total Department Faculty</th>";
                    faculty += "<th colspan='2' style='text-align: center; vertical-align: top;' >Specialization Wise Faculty</th>";
                    //faculty += "<th rowspan='2' style='text-align: center; vertical-align: top;' >Specialization Wise Faculty Available</th>";
                    //faculty += "<th rowspan='2' style='text-align: center; vertical-align: top;' >PG Specialization Faculty</th>";
                    faculty += "<th colspan='2' style='text-align: center; vertical-align: top;' >Faculty with Ph.D</th>";
                    //faculty += "<th rowspan='2' style='text-align: center; vertical-align: top;' >Available Ph.D faculty</th>";
                    faculty += "<th rowspan='2' style='text-align: center; vertical-align: top;' >Remarks</th>";
                    faculty += "<th rowspan='2' style='text-align: center; vertical-align: top;' >Deficiency in Faculty</th>";
                    faculty += "</tr>";
                    //if (mbacollegeides.Contains(cid))
                    //{
                    //    faculty += "<tr>";
                    //    //faculty += "<th colspan='3' style='text-align: center; vertical-align: top;'></th>";
                    //    //faculty += "<th style='text-align: center; vertical-align: top;'></th>";

                    //    //faculty += "<th style='text-align: center; vertical-align: top;'></th>";
                    //    faculty += "<th style='text-align: center; vertical-align: top;'>Required</th>";
                    //    faculty += "<th style='text-align: center; vertical-align: top;'>Available</th>";
                    //    faculty += "<th style='text-align: center; vertical-align: top;'>Required</th>";
                    //    faculty += "<th style='text-align: center; vertical-align: top;'>Available</th>";
                    //    faculty += "</tr>";
                    //}
                    //else
                    //{
                    //    faculty += "<tr>";
                    //    faculty += "<th style='text-align: center; vertical-align: top;'>JNTUH <br/>Sanctioned</th>";
                    //    faculty += "<th style='text-align: center; vertical-align: top;'>Admitted</th>";

                    //    faculty += "<th style='text-align: center; vertical-align: top;'>#Divisions</th>";
                    //    faculty += "<th style='text-align: center; vertical-align: top;'>Required</th>";
                    //    faculty += "<th style='text-align: center; vertical-align: top;'>Available</th>";
                    //    faculty += "<th style='text-align: center; vertical-align: top;'>Required</th>";
                    //    faculty += "<th style='text-align: center; vertical-align: top;'>Available</th>";
                    //    faculty += "</tr>";
                    //}

                    faculty += "<tr>";
                    faculty += "<th style='text-align: center; vertical-align: top;'>AICTE <br/>Sanctioned</th>";
                    faculty += "<th style='text-align: center; vertical-align: top;'>JNTUH <br/>Sanctioned</th>";

                    faculty += "<th style='text-align: center; vertical-align: top;'>#Divisions</th>";
                    faculty += "<th style='text-align: center; vertical-align: top;'>Required</th>";
                    faculty += "<th style='text-align: center; vertical-align: top;'>Available</th>";
                    faculty += "<th style='text-align: center; vertical-align: top;'>Required</th>";
                    faculty += "<th style='text-align: center; vertical-align: top;'>Available</th>";
                    faculty += "</tr>";

                    string[] admittedIntakeTwoormoreZeroDeptName1 = facultyCounts.Where(e => e.Degree == "B.Tech" && ((e.admittedIntake2 == 0 && e.admittedIntake3 == 0) || (e.admittedIntake3 == 0 && e.admittedIntake4 == 0) || (e.admittedIntake2 == 0 && e.admittedIntake4 == 0)) && !departments.Contains(e.Department)).Select(e => e.Department).Distinct().ToArray();

                    #region Intake not equal to Zero

                    // Getting M.Tech Civil Specialization ID's
                    int[] SpecializationIDS =
                        db.jntuh_specialization.Where(S => S.departmentId == 43).Select(S => S.id).ToArray();
                    int remainingFaculty2 = 0;
                    int facultyIndexnotEqualtoZeroIndex = 1;
                    int facultyIndexnotEqualtoZeroAdmittedIntakeIndex = 1;
                    int btechcount = facultyCounts.Where(
                        e =>
                            e.Proposedintake != 0 && e.ispercentage == false && e.Degree == "B.Tech" &&
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


                    //foreach (
                    //    var item in
                    //        facultyCounts.Where(
                    //            e =>
                    //                e.Proposedintake != 0 && e.ispercentage == false &&
                    //                !ProposedIntakeZeroDeptNameSpecIDs.Contains(e.specializationId)).Select(e => e).ToList())
                    foreach (
                        var item in
                            facultyCounts.Where(
                                e =>
                                    e.Proposedintake != 0 && e.ispercentage == false &&
                                    !ProposedIntakeZeroDeptName.Contains(e.Specialization)).Select(e => e).ToList())
                    {
                        #region Code without calculation admitted Intake

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
                            //SpecializationwisePHDFaculty = IntakeWisePhdForBtech(item.Proposedintake);
                            SpecializationwisePHDFaculty = IntakeWisePhdForBtech(item.SanctionIntake2); //SanctionIntake2 - last year approved intake
                        }
                        TotalCount =
                            facultyCounts.Where(
                                D =>
                                    D.Department == item.Department &&
                                    (D.Degree == "M.Tech" || D.Degree == "B.Tech") && D.Proposedintake != 0)
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
                        if ((item.collegeId == 72 && item.Department == "IT") || (item.collegeId == 130 && item.Department == "IT"))
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
                        //tFaculty =50;
                        //item.totalFaculty
                        int rFaculty = (int)Math.Ceiling(item.requiredFaculty);

                        if (departments.Contains(item.Department))
                        {
                            if (OthersSpecIds.Contains(item.specializationId))
                            {
                                double rid = (double)(firstYearRequired / 2);
                                rFaculty = (int)(Math.Ceiling(rid));

                                //othersRequiredfaculty = rFaculty;
                                //Commented By Narayana on 12-03-2018
                                //rFaculty = 1;
                                // othersRequiredfaculty = 1;

                                //rFaculty = ((int)firstYearRequired)/2;
                                //if (rFaculty <= 2)
                                //{
                                //    rFaculty = 2;
                                //    othersRequiredfaculty = 2;
                                //}
                                //else
                                //{
                                //    othersRequiredfaculty = rFaculty;
                                //}

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

                        faculty += "<tr>";
                        faculty +=
                            "<td class='col2' style='text-align: center; vertical-align: top;width:10px' rowspan='1'>" +
                            facultyIndexnotEqualtoZeroIndex + "</td>";
                        //(facultyCounts.Where(e=>e.approvedIntake1!=0).Select(e=>e).ToList().IndexOf(item) + 1)
                        if (item.isstarcourses == true)
                        {
                            faculty += "<td class='col2' style='text-align: left; vertical-align: top;'>" +
                                       item.Department + "#" + "</td>";
                        }
                        else
                        {
                            faculty += "<td class='col2' style='text-align: left; vertical-align: top;'>" +
                                       item.Department + "</td>";
                        }
                        faculty += "<td class='col2' style='text-align: left; vertical-align: top;'>" + item.Degree +
                                   "</td>";
                        //faculty += "<td class='col2' style='text-align: left; vertical-align: top;'>" + item.Specialization + "</td>";
                        if (item.shiftId == 1)
                        {
                            faculty += "<td class='col2' style='text-align: left; vertical-align: top;'>" +
                                       item.Specialization + "</td>";
                        }
                        else
                        {
                            faculty += "<td class='col2' style='text-align: left; vertical-align: top;'>" +
                                       item.Specialization + " -2" + "</td>";
                        }
                        //Written by Narayana
                        //faculty += "<td class='col2' style='text-align: left; vertical-align: top;'>" + item.shiftId + "</td>";
                        if (departments.Contains(item.Department))
                        {
                            // faculty += "<td class='col2' style='text-align: center; vertical-align: top;'> </td>";
                            faculty += "<td class='col2' style='text-align: center; vertical-align: top;'> </td>";
                            //faculty += "<td class='col2' style='text-align: center; vertical-align: top;'> </td>";
                            faculty +=
                                "<td colspan='2' class='col2' style='text-align: center; vertical-align: top;'> </td>";
                            faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" +
                                       totalBtechFirstYearIntake + "</td>";


                            if (deptloop == 1)
                            {
                                if (item.DeptWiseBASFlag > 0)
                                {
                                    faculty += "<td rowspan='" + TotalCount +
                                           "'  class='col2' style='text-align: center; vertical-align: top;'>" +
                                           item.totalFaculty + "+(" + item.DeptWiseBASFlag + ")</td>";
                                }
                                else
                                {
                                    faculty += "<td rowspan='" + TotalCount +
                                               "'  class='col2' style='text-align: center; vertical-align: top;'>" +
                                               item.totalFaculty + "</td>";
                                }
                            }
                            //faculty += "<td class='col2' style='text-align: left; vertical-align: top;'>" + item.A416TotalFaculty + "</td>";
                            if (OthersSpecIds.Contains(item.specializationId))
                            {
                                faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" +
                                           Math.Ceiling(firstYearRequired / 2) + "</td>";
                                HumantitiesminimamRequireMet += item.totalFaculty;
                                TotalAreaRequiredFaculty = TotalAreaRequiredFaculty + Convert.ToInt32(Math.Ceiling(firstYearRequired / 2));
                            }
                            else
                            {
                                faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" +
                                           Math.Ceiling(firstYearRequired) + "</td>";
                                HumantitiesminimamRequireMet += item.totalFaculty;
                                TotalAreaRequiredFaculty = TotalAreaRequiredFaculty + Convert.ToInt32(Math.Ceiling(firstYearRequired));
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
                                //faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + item.totalAdmittedIntake + "</td>";

                                faculty +=
                                "<td class='col2' style='text-align: center; vertical-align: top;'><table width='100%' border='1' cellpadding='3' cellspacing='0' style='border-color: #ccc;'><tr><td><span>" +
                                item.AICTESanctionIntake2 + "</span></td></tr><tr><td><span>" + item.AICTESanctionIntake3 +
                                "</span></td></tr><tr><td><span>" + item.AICTESanctionIntake4 +
                                "</span></td></tr></table></td>";
                                faculty +=
                                    "<td class='col2' style='text-align: center; vertical-align: top;'><table width='100%' border='1' cellpadding='3' cellspacing='0' style='border-color: #ccc;'><tr><td><span>" +
                                    item.SanctionIntake2 + "</span></td></tr><tr><td><span>" + item.SanctionIntake3 +
                                    "</span></td></tr><tr><td><span>" + item.SanctionIntake4 +
                                    "</span></td></tr></table></td>";
                                //faculty +=
                                //    "<td class='col2' style='text-align: center; vertical-align: top;'><table width='100%' border='1' cellpadding='3' cellspacing='0' style='border-color: #ccc;'><tr><td><span>" +
                                //    item.admittedIntake2 + "</span></td></tr><tr><td><span>" + item.admittedIntake3 +
                                //    "</span></td></tr><tr><td><span>" + item.admittedIntake4 +
                                //    "</span></td></tr></table></td>";

                                faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>";
                                faculty +=
                                    "<table width='100%' border='1' cellpadding='3' cellspacing='0' style='border-color: #ccc;'>";
                                //Written New on 03-02-2018
                                //int SanctionIntakeHigest = Max(item.admittedIntake2, item.admittedIntake3,
                                //    item.admittedIntake4);
                                //SanctionIntakeHigest = GetBtechAdmittedIntake(SanctionIntakeHigest);
                                //if (SanctionIntakeHigest >= item.Proposedintake)
                                //{
                                //    faculty += "<tr><td><span>" + GetSectionBasedonIntake(item.admittedIntake2) +
                                //               "(A)" + "</span></td>";
                                //    faculty += "</tr><tr><td><span>" + GetSectionBasedonIntake(item.admittedIntake3) +
                                //               "(A)" + "</span></td></tr>";
                                //    faculty += "<tr><td><span>" + GetSectionBasedonIntake(item.admittedIntake4) +
                                //               "(A)" + "</span></td></tr>";

                                //}
                                //else
                                //{

                                //int[] StrNewSpecs = new[] { 180, 181, 182, 183, 184, 185, 186, 187, 188, 189, 190, 191, 192, 193, 194, 195, 196 };

                                //if (StrNewSpecs.Contains(item.specializationId) && item.Proposedintake != 0 && item.SanctionIntake1 == 0 && item.SanctionIntake2 == 0 && item.SanctionIntake3 == 0)
                                //{
                                //    faculty += "<tr><td><span>0</span></td>";
                                //    faculty += "</tr><tr><td><span>0</span></td></tr>";
                                //    faculty += "<tr><td><span>0</span></td></tr>";
                                //}
                                //else
                                //{
                                //    faculty += "<tr><td><span>" + GetSectionBasedonIntake(item.SanctionIntake2) +
                                //               "</span></td>";
                                //    faculty += "</tr><tr><td><span>" + GetSectionBasedonIntake(item.SanctionIntake3) +
                                //               "</span></td></tr>";
                                //    faculty += "<tr><td><span>" + GetSectionBasedonIntake(item.SanctionIntake4) +
                                //               "</span></td></tr>";
                                //}
                                faculty += "<tr><td><span>" + GetSectionBasedonIntake(item.SanctionIntake2) +
                                               "</span></td>";
                                faculty += "</tr><tr><td><span>" + GetSectionBasedonIntake(item.SanctionIntake3) +
                                           "</span></td></tr>";
                                faculty += "<tr><td><span>" + GetSectionBasedonIntake(item.SanctionIntake4) +
                                           "</span></td></tr>";
                                faculty += "</table></td>";
                            }
                            else
                            {
                                //if (mbacollegeides.Contains(cid))
                                //{

                                //    faculty +=
                                //        "<td colspan='3' class='col2' style='text-align: center; vertical-align: top;'>" +
                                //        item.totalIntake + " </td>";
                                //}
                                //else
                                //{
                                faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" +
                                       item.AICTESanctionIntake2 + "</td>";
                                faculty +=
                                    "<td colspan='2' class='col2' style='text-align: center; vertical-align: top;'>" +
                                    item.SanctionIntake2 + " </td>";
                                //}

                            }
                            //faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + item.totalIntake + "</td>";
                            if (String.IsNullOrEmpty(item.Note))
                            {
                                faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" +
                                           item.Proposedintake + "</td>";
                            }
                            else
                            {
                                faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" +
                                           item.Proposedintake + " " + item.Note + "</td>";
                            }
                            if (deptloop == 1)
                            {
                                if (item.DeptWiseBASFlag > 0)
                                {
                                    faculty += "<td rowspan='" + TotalCount +
                                           "'  class='col2' style='text-align: center; vertical-align: top;'>" +
                                           item.totalFaculty + "+(" + item.DeptWiseBASFlag + ")</td>";
                                }
                                else
                                {
                                    faculty += "<td rowspan='" + TotalCount +
                                           "'  class='col2' style='text-align: center; vertical-align: top;'>" +
                                           item.totalFaculty + "</td>";
                                }

                            }
                            //faculty += "<td class='col2' style='text-align: left; vertical-align: top;'>" + item.A416TotalFaculty + "</td>";
                            if (item.Degree == "B.Tech")
                            {
                                //decimal AF = Math.Ceiling(item.ArequiredFaculty);
                                //decimal SF = Math.Ceiling(item.SrequiredFaculty);
                                //string requriedfaculty = Math.Ceiling(item.requiredFaculty) + "/" + Math.Ceiling(item.SrequiredFaculty);
                                faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" +
                                       item.requiredFaculty + "</td>";
                                TotalAreaRequiredFaculty = TotalAreaRequiredFaculty + Convert.ToInt32(item.requiredFaculty);
                            }
                            else
                            {
                                faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" +
                                        Math.Ceiling(item.requiredFaculty) + "</td>";
                                TotalAreaRequiredFaculty = TotalAreaRequiredFaculty + Convert.ToInt32(item.requiredFaculty);
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

                        faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" +
                                   adjustedFaculty + "</td>";
                        //    if (SpecializationIDS.Contains(item.specializationId))
                        //        faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + SpecializationwisePGFaculty + "</td>";
                        //else
                        //        faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + "" + "</td>";

                        //if (item.Degree == "M.Tech")
                        //    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" +
                        //               item.SpecializationspgFaculty + "</td>";
                        //else
                        //    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + "" +
                        //               "</td>";

                        if (minimumRequirementMet == "YES" && PhdminimumRequirementMet == "YES")
                        {
                            //if (rFaculty <= adjustedFaculty)
                            //    minimumRequirementMet = "-";//No Deficiency In faculty
                            //else
                            //    minimumRequirementMet = "Deficiency In faculty";

                            //if (SpecializationwisePHDFaculty <= adjustedPHDFaculty)
                            //    minimumRequirementMet += "-";//and No Deficiency In Phd
                            //else
                            //    minimumRequirementMet += " and Deficiency In Phd";

                            if (rFaculty <= adjustedFaculty && SpecializationwisePHDFaculty <= adjustedPHDFaculty)
                            {
                                minimumRequirementMet = "-";
                            }
                            else if (rFaculty > adjustedFaculty && SpecializationwisePHDFaculty <= adjustedPHDFaculty)
                            {
                                //if (item.Department == "AI&DS" || item.Department == "CSD" || item.Department == "AI&ML" || item.specializationId == 189)
                                //    minimumRequirementMet = "-";
                                //else
                                minimumRequirementMet = "Deficiency In faculty";
                            }
                            else if (rFaculty <= adjustedFaculty &&
                                     SpecializationwisePHDFaculty > adjustedPHDFaculty)
                            {
                                //if (item.Department == "AI&DS" || item.Department == "CSD" || item.Department == "AI&ML" || item.specializationId == 189)
                                //    minimumRequirementMet = "-";
                                //else
                                minimumRequirementMet = "Deficiency In Ph.D";
                            }
                            else
                            {
                                //if (item.Department == "AI&DS" || item.Department == "CSD" || item.Department == "AI&ML" || item.specializationId == 189)
                                //    minimumRequirementMet = "-";
                                //else
                                minimumRequirementMet = "Deficiency In faculty and Ph.D";
                            }


                        }
                        else if (minimumRequirementMet == "NO" && PhdminimumRequirementMet == "YES")
                        {
                            //if (rFaculty == adjustedFaculty)
                            //    minimumRequirementMet = "-";//No Deficiency In faculty
                            //else
                            //    minimumRequirementMet = "Deficiency In faculty";


                            //if (SpecializationwisePHDFaculty <= adjustedPHDFaculty)
                            //    minimumRequirementMet += "-";//and No Deficiency In Phd
                            //else
                            //    minimumRequirementMet += " and Deficiency In Phd";

                            if (rFaculty <= adjustedFaculty && SpecializationwisePHDFaculty <= adjustedPHDFaculty)
                            {
                                minimumRequirementMet = "-";
                            }
                            else if (rFaculty > adjustedFaculty && SpecializationwisePHDFaculty <= adjustedPHDFaculty)
                            {
                                //if (item.Department == "AI&DS" || item.Department == "CSD" || item.Department == "AI&ML" || item.specializationId == 189)
                                //    minimumRequirementMet = "-";
                                //else
                                minimumRequirementMet = "Deficiency In faculty";
                            }
                            else if (rFaculty <= adjustedFaculty &&
                                     SpecializationwisePHDFaculty > adjustedPHDFaculty)
                            {
                                //if (item.Department == "AI&DS" || item.Department == "CSD" || item.Department == "AI&ML" || item.specializationId == 189)
                                //    minimumRequirementMet = "-";
                                //else
                                minimumRequirementMet = "Deficiency In Ph.D";
                            }
                            else
                            {
                                //if (item.Department == "AI&DS" || item.Department == "CSD" || item.Department == "AI&ML" || item.specializationId == 189)
                                //    minimumRequirementMet = "-";
                                //else
                                minimumRequirementMet = "Deficiency In faculty and Ph.D";
                            }

                        }
                        else if (minimumRequirementMet == "YES" && PhdminimumRequirementMet == "NO")
                        {
                            //if (rFaculty <= adjustedFaculty)
                            //    minimumRequirementMet = "-";//No Deficiency In faculty
                            //else
                            //    minimumRequirementMet = "Deficiency In faculty";

                            //if (SpecializationwisePHDFaculty == adjustedPHDFaculty)
                            //    minimumRequirementMet += "-";// and No Deficiency In Phd
                            //else
                            //    minimumRequirementMet += " and Deficiency In Phd";
                            if (rFaculty <= adjustedFaculty && SpecializationwisePHDFaculty <= adjustedPHDFaculty)
                            {
                                minimumRequirementMet = "-";
                            }
                            else if (rFaculty > adjustedFaculty && SpecializationwisePHDFaculty <= adjustedPHDFaculty)
                            {
                                //if (item.Department == "AI&DS" || item.Department == "CSD" || item.Department == "AI&ML" || item.specializationId == 189)
                                //    minimumRequirementMet = "-";
                                //else
                                minimumRequirementMet = "Deficiency In faculty";
                            }
                            else if (rFaculty <= adjustedFaculty &&
                                     SpecializationwisePHDFaculty > adjustedPHDFaculty)
                            {
                                //if (item.Department == "AI&DS" || item.Department == "CSD" || item.Department == "AI&ML" || item.specializationId == 189)
                                //    minimumRequirementMet = "-";
                                //else
                                minimumRequirementMet = "Deficiency In Ph.D";
                            }
                            else
                            {
                                //if (item.Department == "AI&DS" || item.Department == "CSD" || item.Department == "AI&ML" || item.specializationId == 189)
                                //    minimumRequirementMet = "-";
                                //else
                                minimumRequirementMet = "Deficiency In faculty and Ph.D";
                            }

                        }

                        else if (minimumRequirementMet == "NO" && PhdminimumRequirementMet == "NO")
                        {
                            //if (rFaculty == adjustedFaculty)
                            //    minimumRequirementMet = "-";//No Deficiency In faculty
                            //else
                            //    minimumRequirementMet = "Deficiency In faculty";


                            //if (SpecializationwisePHDFaculty == adjustedPHDFaculty)
                            //    minimumRequirementMet += "-";// and No Deficiency In Phd
                            //else
                            //    minimumRequirementMet += " and Deficiency In Phd";
                            if (rFaculty <= adjustedFaculty && SpecializationwisePHDFaculty <= adjustedPHDFaculty)
                            {
                                minimumRequirementMet = "-";
                            }
                            else if (rFaculty > adjustedFaculty && SpecializationwisePHDFaculty <= adjustedPHDFaculty)
                            {
                                //if (item.Department == "AI&DS" || item.Department == "CSD" || item.Department == "AI&ML" || item.specializationId == 189)
                                //    minimumRequirementMet = "-";
                                //else
                                minimumRequirementMet = "Deficiency In faculty";
                            }
                            else if (rFaculty <= adjustedFaculty &&
                                     SpecializationwisePHDFaculty > adjustedPHDFaculty)
                            {
                                //if (item.Department == "AI&DS" || item.Department == "CSD" || item.Department == "AI&ML" || item.specializationId == 189)
                                //    minimumRequirementMet = "-";
                                //else
                                minimumRequirementMet = "Deficiency In Ph.D";
                            }
                            else
                            {
                                //if (item.Department == "AI&DS" || item.Department == "CSD" || item.Department == "AI&ML" || item.specializationId == 189)
                                //    minimumRequirementMet = "-";
                                //else
                                minimumRequirementMet = "Deficiency In faculty and Ph.D";
                            }
                        }






                        faculty += "<td class='col2' style='text-align: center; vertical-align: top;font-weight:bold;'>" +
                                   SpecializationwisePHDFaculty + "</td>";
                        //if (deptloop == 1)
                        //    faculty += "<td rowspan='" + TotalCount + "'  class='col2' style='text-align: center; vertical-align: top;'>" + SpecializationwisePHDFaculty + "</td>";

                        //faculty += "<td   class='col2' style='text-align: center; vertical-align: top;'>" + SpecializationwisePHDFaculty + "</td>";

                        //else
                        //    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + "" + "</td>";

                        faculty += "<td class='col2' style='text-align: center; vertical-align: top; font-weight: bold;'>" +
                                   adjustedPHDFaculty + "</td>";
                        faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" +
                                   minimumRequirementMet + "</td>";
                        if (((Math.Ceiling(item.requiredFaculty) - adjustedFaculty)) > 0)
                        {
                            faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" +
                                   (Math.Ceiling(item.requiredFaculty) - adjustedFaculty) + "</td>";

                        }
                        else if (OthersSpecIds.Contains(item.specializationId))
                        {
                            var d = Math.Ceiling(firstYearRequired / 2) - adjustedFaculty;
                            var str = d > 0 ? d.ToString() : "-";
                            faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" +
                                   str + "</td>";
                        }
                        else if (departments.Contains(item.Department) && ((firstYearRequired - adjustedFaculty) > 0))
                        {
                            faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" +
                                   (firstYearRequired - adjustedFaculty) + "</td>";

                        }
                        else
                        {
                            faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>-</td>";
                        }

                        faculty += "</tr>";

                        deptloop++;
                        SpecializationwisePHDFaculty = 0;
                        facultyIndexnotEqualtoZeroIndex++;

                        #endregion

                        //  }

                    }
                    faculty += "</table>";


                    #endregion

                    //

                    faculty += "<table width='100%' border='0' cellpadding='5' cellspacing='0'>";
                    //if (mbacollegeides.Contains(cid))
                    //{
                    //    faculty += "<tr>";
                    //    faculty += "<td align='left'>* I & II Year for MBA</td>";
                    //    faculty += "</tr>";
                    //}
                    //else
                    //{
                    faculty += "<tr>";
                    faculty += "<td align='left'># Every 60 or part there of is considered as one division.</td>";
                    faculty +=
                        "<td align='left'></td>";
                    faculty += "</tr>";
                    faculty += "<tr>";
                    faculty +=
                        "<td align='left' colspan='2'>* The department wise faculty requirement for II, III & IV Year for B.Tech is calculated based on JNTUH Sanctioned Intake and I & II Year for M.Tech is calculated based on AICTE Sanctioned Intake.</td>";
                    //faculty += "<td align='left'># Every 60 or part there of admitted is considered as one division.</td>";
                    faculty += "</tr>";
                    faculty += "<tr>";
                    faculty += "<td align='left'></td>";
                    faculty += "<td align='left'></td>";
                    faculty += "</tr>";
                    //}
                    faculty += "</table>";


                    if (facultyCounts.Select(e => e.Degree).Contains("B.Tech"))
                    {
                        if (Math.Ceiling(Convert.ToDecimal((totalBtechFirstYearIntake / 20))) <=
                            HumantitiesminimamRequireMet && HumantitiesminimamRequireMetStatus != "NO")
                        {
                            faculty += "<table width='100%' border='0' cellpadding='5' cellspacing='0'>";
                            faculty += "<tr>";
                            faculty += "<td align='left'><b><u>Humanities Deficiency</u></b></td>";
                            faculty += "</tr>";
                            faculty += "</table>";
                            faculty += "<table width='100%' border='1' cellpadding='5' cellspacing='0'>";
                            faculty += "<tr>";
                            faculty += "<th align='left'>S.NO</th><th align='left'>Department</th><th align='left'>1st Year Total Intake</th><th align='left'>Required</th><th align='left'>Available</th><th>Deficiency</th><th>Deficiency in Faculty</th>";
                            faculty += "</tr>";
                            faculty += "<tr>";
                            // faculty += "<td align='left'>No Deficiency in Humanities</td>";
                            if ((Math.Ceiling(Convert.ToDecimal((totalBtechFirstYearIntake / 20))) - HumantitiesminimamRequireMet) > 0)
                            {
                                faculty += "<td>1</td><td align='left'>H&S and Others</td><td>" + totalBtechFirstYearIntake + "</td><td>" + Math.Ceiling(Convert.ToDecimal((totalBtechFirstYearIntake / 20))) + "</td><td>" + HumantitiesminimamRequireMet + "</td><td align='left'>No Deficiency in H&S and Others</td><td align='center'>" + (Math.Ceiling(Convert.ToDecimal((totalBtechFirstYearIntake / 20))) - HumantitiesminimamRequireMet) + "</td>";
                            }
                            else
                            {
                                faculty += "<td>1</td><td align='left'>H&S and Others</td><td>" + totalBtechFirstYearIntake + "</td><td>" + Math.Ceiling(Convert.ToDecimal((totalBtechFirstYearIntake / 20))) + "</td><td>" + HumantitiesminimamRequireMet + "</td><td align='left'>No Deficiency in H&S and Others</td><td align='center'>-</td>";
                            }
                            faculty += "</tr>";
                            faculty += "</table>";
                        }
                        else
                        {
                            faculty += "<table width='100%' border='0' cellpadding='5' cellspacing='0'>";
                            faculty += "<tr>";
                            faculty += "<td align='left'><b><u>Humanities Deficiency</u></b></td>";
                            faculty += "</tr>";
                            faculty += "</table>";
                            faculty += "<table width='100%' border='1' cellpadding='5' cellspacing='0'>";
                            faculty += "<tr>";
                            faculty += "<th align='left'>S.NO</th><th align='left'>Department</th><th align='left'>1st Year Total Intake</th><th align='left'>Required</th><th align='left'>Available</th><th>Deficiency</th><th>Deficiency in Faculty</th>";
                            faculty += "</tr>";
                            faculty += "<tr>";
                            if (((totalBtechFirstYearIntake / 20) - HumantitiesminimamRequireMet) > 0)
                            {
                                faculty += "<td>1</td><td align='left'>H&S and Others</td><td>" + totalBtechFirstYearIntake + "</td><td>" + (totalBtechFirstYearIntake / 20) + "</td><td>" + HumantitiesminimamRequireMet + "</td><td align='left'>Overall deficiency in H&S and Others</td><td align='center'>" + (Convert.ToDecimal((totalBtechFirstYearIntake / 20)) - HumantitiesminimamRequireMet) + "</td>";
                            }
                            else
                            {
                                faculty += "<td>1</td><td align='left'>H&S and Others</td><td>" + totalBtechFirstYearIntake + "</td><td>" + (totalBtechFirstYearIntake / 20) + "</td><td>" + HumantitiesminimamRequireMet + "</td><td align='left'>Overall deficiency in H&S and Others</td><td align='center'>-</td>";
                            }
                            faculty += "</tr>";
                            faculty += "</table>";
                        }
                    }

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
                        }
                    }


                    //#region Proposed Intake Zero and Closure

                    //facultyclosure +=
                    //    "<p>The Courses for which college has applied for Closure or Proposed Zero Intake for A.Y. 2022-23 are as follows.</p>";
                    //facultyclosure +=
                    //    "<table width='100%' border='1' cellpadding='3' cellspacing='0' style='border-color: #ccc;'>";
                    //facultyclosure += "<tr>";
                    //facultyclosure += "<th style='text-align: center; vertical-align: top;'>SNo</th>";
                    //facultyclosure += "<th style='text-align: left; vertical-align: top;' >Degree</th>";
                    //facultyclosure += "<th style='text-align: left; vertical-align: top;' >Department</th>";
                    //facultyclosure += "<th style='text-align: left; vertical-align: top;' >Specialization</th>";

                    //facultyclosure += "</tr>";

                    //int ProposedIntakeZeroIndex = 1;
                    //foreach (
                    //    var item in
                    //        facultyCounts.Where(
                    //            e => e.Proposedintake == 0 || ProposedIntakeZeroDeptName.Contains(e.Specialization))
                    //            .Select(e => e)
                    //            .ToList())
                    //{
                    //    facultyclosure += "<tr>";
                    //    facultyclosure +=
                    //        "<td class='col2' style='text-align: center; vertical-align: top;width:10px' rowspan='1'>" +
                    //        ProposedIntakeZeroIndex + "</td>";
                    //    facultyclosure += "<td class='col2' style='text-align: left; vertical-align: top;'>" +
                    //                      item.Degree + "</td>";
                    //    facultyclosure += "<td class='col2' style='text-align: left; vertical-align: top;'>" +
                    //                       item.Department + "</td>";
                    //    facultyclosure += "<td class='col2' style='text-align: left; vertical-align: top;'>" +
                    //                      item.Specialization + "</td>";

                    //    facultyclosure += "</tr>";

                    //    deptloop++;
                    //    SpecializationwisePHDFaculty = 0;
                    //    ProposedIntakeZeroIndex++;
                    //}
                    //facultyclosure += "</table>";
                    //faculty += facultyclosure;
                    //#endregion

                    #region 25% Admitted Intake Cources

                    twintyfivepercentbelowcurces +=
                        "<p><b>#</b> The following Courses have either Zero Admissions due to non-grant of Affiliation or Admitted intake is less than 25% of JNTUH Sanctioned intake for the previous three academic years.Hence SCA has recommended for <b>&lsquo;No Admission Status&rsquo;</b> for these courses for the A.Y.2020-21 as per clause: 3.30 of Affiliation Regulations 2020-21.However, if the admitted intake in any of the applied courses is atleast 10 or above, in any one of the previous three years,then such courses will be considered for Affiliation with a minimum of 60 intake provided they meet other requirements as per norms.</p>";
                    twintyfivepercentbelowcurces +=
                        "<table width='100%' border='1' cellpadding='3' cellspacing='0' style='border-color: #ccc;'>";
                    twintyfivepercentbelowcurces += "<tr>";
                    twintyfivepercentbelowcurces += "<th style='text-align: center; vertical-align: top;'>SNo</th>";
                    twintyfivepercentbelowcurces +=
                        "<th style='text-align: left; vertical-align: top;' >Degree</th>";
                    twintyfivepercentbelowcurces +=
                        "<th style='text-align: left; vertical-align: top;' >Department</th>";
                    twintyfivepercentbelowcurces +=
                        "<th style='text-align: left; vertical-align: top;' >Specialization</th>";
                    twintyfivepercentbelowcurces +=
                        "<th style='text-align: left; vertical-align: top;' >Proposed Intake</th>";
                    facultyclosure += "<th style='text-align: center; vertical-align: top;' >Sanction Intake</th>";
                    facultyclosure += "<th style='text-align: center; vertical-align: top;' >Admitted Intake</th>";
                    facultyclosure += "<th style='text-align: center; vertical-align: top;' >Proposed Intake</th>";

                    facultyclosure += "</tr>";

                    int twintyfivepercentIndex = 1;
                    foreach (
                        var item in
                            facultyCounts.Where(
                                e =>
                                    e.Degree == "B.Tech" && e.ispercentage == true &&
                                    twentyfivePersentbelowDeptName.Contains(e.Specialization))
                                .Select(e => e)
                                .ToList())
                    {
                        twintyfivepercentbelowcurces += "<tr>";
                        twintyfivepercentbelowcurces +=
                            "<td class='col2' style='text-align: center; vertical-align: top;width:10px' rowspan='1'>" +
                            twintyfivepercentIndex + "</td>";
                        twintyfivepercentbelowcurces +=
                            "<td class='col2' style='text-align: left; vertical-align: top;'>" + item.Degree +
                            "</td>";
                        twintyfivepercentbelowcurces +=
                            "<td class='col2' style='text-align: left; vertical-align: top;'>" + item.Department +
                            "</td>";
                        twintyfivepercentbelowcurces +=
                            "<td class='col2' style='text-align: left; vertical-align: top;'>" + item.Specialization +
                            "</td>";
                        twintyfivepercentbelowcurces +=
                           "<td class='col2' style='text-align: left; vertical-align: top;'>" + item.Proposedintake +
                           "</td>";
                        twintyfivepercentbelowcurces += "</tr>";

                        deptloop++;
                        SpecializationwisePHDFaculty = 0;
                        twintyfivepercentIndex++;
                    }

                    //twintyfivepercentbelowcurces += "<tr>";
                    //twintyfivepercentbelowcurces +=
                    //            "<td class='col2' style='text-align: center;font-weight: bold;' colspan='5'>NIL</td>";
                    //twintyfivepercentbelowcurces += "</tr>";
                    twintyfivepercentbelowcurces += "</table>";
                    // faculty += twintyfivepercentbelowcurces;
                    #endregion
                }
                else
                {

                    if (collegeStatus.CollegeStatus == true)
                    {
                        faculty += "<p>As per the records the programs/courses in which there were zero admissions or no grant of affiliation by the University in the previous three academic years are shown in the following table. It is observed that for two or more than two academic years there were zero admissions in all the programs/courses either due to non grant of affiliation or unable to make admissions at your Institute. As per the records it is also observed that there is a mismatch of faculty information that you have submitted to AICTE and the University. </p>";
                        string[] admittedIntakeTwoormoreZeroDeptName1 = facultyCounts.Where(e => e.Degree == "B.Tech" && ((e.admittedIntake2 == 0 && e.admittedIntake3 == 0) || (e.admittedIntake3 == 0 && e.admittedIntake4 == 0) || (e.admittedIntake2 == 0 && e.admittedIntake4 == 0)) && !departments.Contains(e.Department)).Select(e => e.Department).Distinct().ToArray();
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
                                    SpecializationwisePHDFaculty = IntakeWisePhdForMtech(item.Proposedintake, item.shiftId);
                                }
                                else if (item.Degree == "MCA")
                                {
                                    SpecializationwisePHDFaculty = IntakeWisePhdForMBAandMCA(item.Proposedintake);
                                }
                                else if (item.Degree == "MBA")
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
                                    if (item.admittedIntake2 == 0)
                                        facultyAdmittedIntakeZero += "<tr><td><span>" + item.admittedIntake2 + "(II Year)</span></td></tr>";
                                    if (item.admittedIntake3 == 0)
                                        facultyAdmittedIntakeZero += "<tr><td><span>" + item.admittedIntake3 + "(III Year)</span></td></tr>";
                                    if (item.admittedIntake4 == 0)
                                        facultyAdmittedIntakeZero += "<tr><td><span>" + item.admittedIntake4 + "(IV Year)</span></td></tr>";
                                    facultyAdmittedIntakeZero += "</table></td>";
                                }
                                else
                                {
                                    facultyAdmittedIntakeZero += "<td class='col2' style='text-align: center; vertical-align: top;'>" + item.totalIntake + "</td>";
                                    facultyAdmittedIntakeZero += "<td class='col2' style='text-align: center; vertical-align: top;'>" + item.totalIntake + " </td>";
                                }
                                //faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + item.totalIntake + "</td>;
                                facultyAdmittedIntakeZero += "<td class='col2' style='text-align: center; vertical-align: top;'>" + item.Proposedintake + "</td>";
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
                            facultyIndexnotEqualtoZeroAdmittedIntakeIndex++;

                            #endregion

                            // }

                        }

                        facultyAdmittedIntakeZero += "</table>";
                        faculty += facultyAdmittedIntakeZero;
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
            //principalRegno != null ? db.jntuh_registered_faculty.Where(rf => collegefacultyRegistrationNo.Contains(rf.RegistrationNumber.Trim()) && rf.RegistrationNumber != principalRegno).ToList(): db.jntuh_registered_faculty.Where(rf => collegefacultyRegistrationNo.Contains(rf.RegistrationNumber.Trim())).ToList();
            //var jntuh_registered_faculty1 =
            //        registeredFaculty.Where(rf => rf.type != "Adjunct" && ((rf.Absent == false) && (rf.OriginalCertificatesNotShown == false) && (rf.Xeroxcopyofcertificates == false || rf.Xeroxcopyofcertificates == null) && (rf.NotQualifiedAsperAICTE == false || rf.NotQualifiedAsperAICTE == null)//&& rf.Noform16Verification == false && rf.NoForm16 == false
            //                                            && (rf.NoSCM == false || rf.NoSCM == null) && (rf.PANNumber != null) && (rf.IncompleteCertificates == false || rf.IncompleteCertificates == null) && (rf.Blacklistfaculy == false) && rf.NoRelevantUG != "Yes" && rf.NoRelevantPG != "Yes" && rf.NORelevantPHD != "Yes" && rf.NotIdentityfiedForanyProgram == false && (rf.InvalidPANNumber == false || rf.InvalidPANNumber == null) && (rf.SamePANUsedByMultipleFaculty == false || rf.SamePANUsedByMultipleFaculty == null) && (rf.OriginalsVerifiedPHD != true) && (rf.OriginalsVerifiedUG != true) && (rf.BASStatusOld != "Yes" && rf.BASStatus != "Yes")) && (rf.DepartmentId != 61 || rf.DepartmentId != 27))
            //    //  .Where(rf => rf.DepartmentId != null && ((rf.Absent != true) && (rf.NotQualifiedAsperAICTE != true)&& (rf.NoSCM != true) && (rf.PANNumber != null) && rf.Notin116 != true && rf.Blacklistfaculy != true && rf.PHDundertakingnotsubmitted != true) && (rf.DepartmentId != 61 || rf.DepartmentId != 27) && rf.type != "Adjunct")
            //                 .Select(rf => new
            //                 {
            //                     //Departmentid = rf.DepartmentId,
            //                     RegistrationNumber = rf.RegistrationNumber,
            //                     // Department = rf.jntuh_department.departmentName,
            //                     HighestDegreeID = rf.jntuh_registered_faculty_education.Count() != 0 ? rf.jntuh_registered_faculty_education.Select(e => e.educationId).Max() : 0,
            //                     IsApproved = rf.isApproved,
            //                     PanNumber = rf.PANNumber,
            //                     AadhaarNumber = rf.AadhaarNumber,
            //                     NoForm16 = rf.NoForm16,
            //                     TotalExperience = rf.TotalExperience
            //                 }).ToList();
            int totalfacultycount = registeredFaculty.Count();
            //ViewBag.Colleges =
            //       db.jntuh_college.Join(db.jntuh_college_edit_status, co => co.id, e => e.collegeId,
            //           (co, e) => new { co = co, e = e })
            //           .Where(c => c.e.IsCollegeEditable == false && assignedcollegeslist.Contains(c.e.collegeId))
            //           .Select(c => new { collegeId = c.co.id, collegeName = c.co.collegeCode + "-" + c.co.collegeName })
            //           .OrderBy(c => c.collegeName)
            //           .ToList();
            //Checking Last Year and Present Year comman Faculty
            var commanfaculty = from cf in db.jntuh_college_faculty_registered
                                from pf in db.jntuh_college_previous_academic_faculty
                                where (cf.RegistrationNumber == pf.RegistrationNumber && cf.collegeId == pf.collegeId && cf.collegeId == collegeID)
                                select new
                                {
                                    cf.RegistrationNumber,
                                    cf.collegeId,

                                };
            int commanfacultycount = commanfaculty.Count();
            int newfaculty = totalfacultycount - commanfacultycount;
            var regFacultyWothoutAbsents = registeredFaculty.Where(rf => (rf.Absent == false || rf.Absent == null)).ToList();
            var jntuh_registered_facultyBAS = registeredFaculty.Where(rf => (rf.BAS == "Yes")).Select(rf => new
            {
                FacultyId = rf.id,
                //Departmentid = rf.DepartmentId,
                RegistrationNumber = rf.RegistrationNumber.Trim(),
                // Department = rf.jntuh_department.departmentName,
                //DeptId = jntuh_college_faculty_registered.Where(c => c.RegistrationNumber.Trim() == rf.RegistrationNumber.Trim()).Select(c => c.DepartmentId).FirstOrDefault(),
                //SpecializationId = jntuh_college_faculty_registered.Where(c => c.RegistrationNumber.Trim() == rf.RegistrationNumber.Trim()).Select(c => c.SpecializationId).FirstOrDefault(),
                NotconsideredPHD = rf.NotconsideredPHD,
                // HighestDegreeID = rf.jntuh_registered_faculty_education.Count() != 0 ? rf.jntuh_registered_faculty_education.Where(e => e.educationId != 8).Select(e => e.educationId).Max() : 0,
                HighestDegreeID = rf.NotconsideredPHD == true ?
                            rf.jntuh_registered_faculty_education.Where(r => r.educationId != 8 && r.educationId != 9).Count() != 0 ?
                            rf.jntuh_registered_faculty_education.Where(e => e.educationId != 8 && e.educationId != 6 && e.educationId != 9).Select(e => e.educationId).Max() : 0
                            :
                            rf.jntuh_registered_faculty_education.Where(r => r.educationId != 8 && r.educationId != 9).Count() != 0 ?
                            rf.jntuh_registered_faculty_education.Where(e => e.educationId != 8 && e.educationId != 9).Select(e => e.educationId).Max() : 0,
                IsApproved = rf.isApproved,
                //Createdon = jntuh_college_faculty_registered.Where(c => c.RegistrationNumber.Trim() == rf.RegistrationNumber.Trim()).Select(c => c.createdOn).FirstOrDefault(),
                PanNumber = rf.PANNumber,
                AadhaarNumber = rf.AadhaarNumber,
                NoForm16 = rf.NoForm16,
                TotalExperience = rf.TotalExperience,
                CsePhDFacultyFlag = rf.PhdDeskVerification,
                BASFlag = rf.BAS
            }).ToList();
            // registeredFaculty.Where(rf => rf.type != "Adjunct" && ((rf.NoForm16 == false || rf.NoForm16 == null) && (rf.NoForm26AS == false || rf.NoForm26AS == null) && (rf.Absent == false || rf.Absent == null) && (rf.OriginalCertificatesNotShown == false || rf.OriginalCertificatesNotShown == null) && (rf.Xeroxcopyofcertificates == false || rf.Xeroxcopyofcertificates == null) && (rf.NotQualifiedAsperAICTE == false || rf.NotQualifiedAsperAICTE == null)//&& rf.Noform16Verification == false && rf.NoForm16 == false
            //&& (rf.Noclass == false || rf.Noclass == null) && (rf.FakePHD == false || rf.FakePHD == null) && (rf.Genuinenessnotsubmitted == false || rf.Genuinenessnotsubmitted == null) && (rf.NoSCM == false || rf.NoSCM == null) && (rf.PANNumber != null) && (rf.IncompleteCertificates == false || rf.IncompleteCertificates == null) && (rf.Blacklistfaculy == false) && rf.NoRelevantUG != "Yes" && rf.NoRelevantPG != "Yes" && rf.NORelevantPHD != "Yes" && (rf.InvalidPANNumber == false || rf.InvalidPANNumber == null) && (rf.OriginalsVerifiedPHD != true) && (rf.OriginalsVerifiedUG != true) && (rf.Invaliddegree == false || rf.Invaliddegree == null) && rf.InvalidAadhaar != "Yes") && (rf.DepartmentId != 61 || rf.DepartmentId != 27)).Select(rf => new
            //var jntuh_registered_faculty1 =
            //        registeredFaculty.Where(rf => rf.type != "Adjunct" && ((rf.NotQualifiedAsperAICTE == false || rf.NotQualifiedAsperAICTE == null) && (rf.Noclass == false || rf.Noclass == null) && (rf.Absent == false || rf.Absent == null) && (rf.OriginalCertificatesNotShown == false || rf.OriginalCertificatesNotShown == null) &&
            //                                             (rf.NoSCM == false || rf.NoSCM == null) && rf.NoRelevantUG != "Yes" && rf.NoRelevantPG != "Yes" && rf.NORelevantPHD != "Yes" && (rf.InvalidPANNumber == false || rf.InvalidPANNumber == null) && (rf.Blacklistfaculy == false) && (rf.AbsentforVerification == false || rf.AbsentforVerification == null) && (rf.DepartmentId != 61 || rf.DepartmentId != 27))).Select(rf => new
            //                                          {
            //                                              //Departmentid = rf.DepartmentId,
            //                                              RegistrationNumber = rf.RegistrationNumber.Trim(),
            //                                              // Department = rf.jntuh_department.departmentName,
            //                                              //DeptId = jntuh_college_faculty_registered.Where(c => c.RegistrationNumber.Trim() == rf.RegistrationNumber.Trim()).Select(c => c.DepartmentId).FirstOrDefault(),
            //                                              //SpecializationId = jntuh_college_faculty_registered.Where(c => c.RegistrationNumber.Trim() == rf.RegistrationNumber.Trim()).Select(c => c.SpecializationId).FirstOrDefault(),
            //                                              HighestDegreeID = rf.NotconsideredPHD == true ?
            //                                                             rf.jntuh_registered_faculty_education.Where(r => r.educationId != 8 && r.educationId != 9).Count() != 0 ?
            //                                                             rf.jntuh_registered_faculty_education.Where(e => e.educationId != 8 && e.educationId != 6 && e.educationId != 9).Select(e => e.educationId).Max() : 0
            //                                                             :
            //                                                             rf.jntuh_registered_faculty_education.Where(r => r.educationId != 8 && r.educationId != 9).Count() != 0 ?
            //                                                             rf.jntuh_registered_faculty_education.Where(e => e.educationId != 8 && e.educationId != 9).Select(e => e.educationId).Max() : 0,
            //                                              // HighestDegreeID = rf.NotconsideredPHD == true ? rf.jntuh_registered_faculty_education.Count() != 0 ? rf.jntuh_registered_faculty_education.Where(e => e.educationId != 8 && e.educationId != 6).Select(e => e.educationId).Max() : 0 : rf.jntuh_registered_faculty_education.Count() != 0 ? rf.jntuh_registered_faculty_education.Where(e => e.educationId != 8).Select(e => e.educationId).Max() : 0,
            //                                              IsApproved = rf.isApproved,
            //                                              //Createdon = jntuh_college_faculty_registered.Where(c => c.RegistrationNumber.Trim() == rf.RegistrationNumber.Trim()).Select(c => c.createdOn).FirstOrDefault(),
            //                                              PanNumber = rf.PANNumber,
            //                                              AadhaarNumber = rf.AadhaarNumber,
            //                                              NoForm16 = rf.NoForm16,
            //                                              TotalExperience = rf.TotalExperience
            //                                          }).ToList();
            //jntuh_registered_faculty1 = jntuh_registered_faculty1.Where(e => e.HighestDegreeID >= 4).ToList();

            //collegeFacultycount = jntuh_registered_faculty1.Count;

            var lastyearfacultycount = db.jntuh_notin415faculty.Where(i => i.CollegeId == collegeID).Select(i => i).FirstOrDefault();

            int count1 = 0;
            //var nodocumentsdetails =db.jntuh_deficiencyrepoprt_college_pendingdocuments.Where(i => i.CollegeId == collegeID).Select(i => i).FirstOrDefault();
            DateTime lasteditfromdate = new DateTime(2022, 11, 29);
            var TotalAictecount = db.jntuh_college_aictefaculty.Where(e => e.CollegeId == collegeID).ToList().Count();
            var regnos = db.jntuh_college_faculty_registered.Where(c => c.collegeId == collegeID && c.createdOn >= lasteditfromdate).Select(s => s).ToList();
            //var ToatalFacultyon418 = db.jntuh_registered_faculty.Where(r => regnos.Contains(r.RegistrationNumber) && r.Absent == false).ToList().Distinct().Count();
            var AffiliationFee2021 = db.jntuh_college_paymentoffee.Where(e => e.collegeId == collegeID && e.FeeTypeID == 5 && (e.academicyearId == (prAy - 2))).Select(e => e).FirstOrDefault();
            var CommanserviceFee2021 = db.jntuh_college_paymentoffee.Where(e => e.collegeId == collegeID && e.FeeTypeID == 3 && (e.academicyearId == (prAy - 2))).Select(e => e).FirstOrDefault();
            //var AffiliationFee1920 = db.jntuh_college_paymentoffee.Where(e => e.collegeId == collegeID && e.FeeTypeID == 5 && (e.academicyearId == (prAy - 2))).Select(e => e).FirstOrDefault();
            //var CommanserviceFee1920 = db.jntuh_college_paymentoffee.Where(e => e.collegeId == collegeID && e.FeeTypeID == 3 && (e.academicyearId == (prAy - 2))).Select(e => e).FirstOrDefault();
            var AffiliationFeeDues2022_23 = db.jntuh_college_paymentoffee.Where(e => e.collegeId == collegeID && e.FeeTypeID == 5 && e.academicyearId == (prAy - 1)).Select(e => e.duesAmount).FirstOrDefault();
            if (collegeStatus != null)
            {
                if (collegeStatus.CollegeStatus == false && collegeStatus.AICTEStatus == false)
                {
                    //PENDING SUPPORTING DOCUMENTS FOR ONLINE  DATA:

                    List<int?> NoBAS = new List<int?> { 2, 59, 247, 415, 419, 239, 447 };

                    #region PENDING SUPPORTING DOCUMENTS FOR ONLINE  DATA:

                    faculty += "<br/><table><tr><td align='left'><b><u>Pending Dues (in Rupees):</u></b></td>";
                    faculty += "</tr></table>";
                    // faculty += "<ul style='list-style-type:disc'>";
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
                    faculty +=
                       "<table width='100%' border='1' cellpadding='3' cellspacing='0' style='border-color: #ccc;'>";
                    faculty += "<tr>";
                    faculty += "<th style='text-align: left; vertical-align: top;'>Affiliation Fee 2022-23</th>";
                    //faculty += "<th style='text-align: left; vertical-align: top;' >Common Service Fee 2019-20</th>";
                    faculty += "<th style='text-align: left; vertical-align: top;' >Affiliation Fee Upto 2021-22</th>";
                    faculty += "<th style='text-align: left; vertical-align: top;' >Common Service Fee Upto 2021-22</th>";
                    faculty += "</tr>";
                    faculty += "<tr>";
                    //if (AffiliationFee1920 != null)
                    //{
                    //    faculty +=
                    //    "<td class='col2' style='text-align: left; vertical-align: top;'>" +
                    //     AffiliationFee1920.duesAmount + "</td>";
                    //}
                    //else
                    //{
                    //    faculty +=
                    //   "<td class='col2' style='text-align: left; vertical-align: top;'>" +
                    //   "0.00" + "</td>";
                    //}

                    //if (CommanserviceFee1920 != null)
                    //{
                    //    faculty += "<td class='col2' style='text-align: left; vertical-align: top;'>" +
                    //   CommanserviceFee1920.duesAmount.ToString() + "</td>";
                    //}
                    //else
                    //{
                    //    faculty += "<td class='col2' style='text-align: left; vertical-align: top;'>" +
                    //   "0.00" + "</td>";
                    //}

                    faculty += "<td class='col2' style='text-align: left; vertical-align: top; font-weight: bold'>" + AffiliationFeeDues2022_23 + "</td>";
                    if (AffiliationFee2021 != null)
                    {
                        faculty += "<td class='col2' style='text-align: left; vertical-align: top; font-weight: bold'>" +
                        AffiliationFee2021.duesAmount + "</td>";
                    }
                    else
                    {
                        faculty += "<td class='col2' style='text-align: left; vertical-align: top; font-weight: bold'>" +
                        "0.00" + "</td>";
                    }
                    if (CommanserviceFee2021 != null)
                    {
                        faculty += "<td class='col2' style='text-align: left; vertical-align: top; font-weight: bold'>" +
                        CommanserviceFee2021.duesAmount.ToString() + "</td>";
                    }
                    else
                    {
                        faculty += "<td class='col2' style='text-align: left; vertical-align: top; font-weight: bold'>" +
                        "0.00" + "</td>";
                    }
                    faculty += "</tr>";
                    faculty += "</table><br/>";
                    //if (nodocumentsdetails != null && nodocumentsdetails.Antiraging == true)
                    //{
                    //    faculty += "<li>Antiragging Committee Details.</li>";

                    //}
                    //if (nodocumentsdetails != null && nodocumentsdetails.AuditedStatement == true)
                    //{
                    //    faculty += "<li>Audited Statement.</li>";
                    //}
                    //if (nodocumentsdetails != null && nodocumentsdetails.LandUsedCertificate == true)
                    //{
                    //    faculty += "<li>Land Use Certificate.</li>";
                    //}

                    // faculty += "</ul>";

                    #endregion

                    //faculty +="<p><b>Note:-</b>Affiliattion and Common Service Fee Due Subject to Verification</p>";
                    //OTHER OBSERVATIONS/  REMARKS

                    #region OTHER OBSERVATIONS/  REMARKS

                    int Count2 = 0;

                    faculty += "<table><tr><td align='left'><b><u>Faculty Details:</u></b></td>";
                    faculty += "</tr></table>";
                    //faculty += "<ul style='list-style-type:disc'>";


                    //if (nodocumentsdetails != null && nodocumentsdetails.FFCTeamComments != "")
                    //{
                    //    faculty += "<li>" + nodocumentsdetails.FFCTeamComments + "</li>";

                    //}
                    //if (TotalAictecount != null && TotalAictecount != 0)
                    //{

                    //    faculty += "<li>Total faculty uploaded to AICTE for Extension of Approval for AY 2022-23 &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; : " + TotalAictecount + ".</li>";//facultyCounts.Select(i => i.specializationWiseFaculty).Sum()
                    //    faculty += "<li>Total faculty available during inspection with  qualifications as prescribed by AICTE &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;:<b> " + collegeFacultycount + "</b>.</li>";
                    //}
                    //else
                    //{
                    //    faculty += "<li>Total faculty uploaded to AICTE for Extension of Approval for AY 2022-23 &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; : 0.</li>";
                    //    faculty += "<li>Total faculty available during inspection with  qualifications as prescribed by AICTE &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;:<b> " + collegeFacultycount + "</b>.</li>";

                    //}
                    //if (lastyearfacultycount != null)
                    //{

                    //    faculty += "<li>Number of faculty recruited after the last inspection  &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; : " + newfaculty + ".</li>";
                    //}
                    //else
                    //{

                    //    faculty += "<li>Number of faculty recruited after the last inspection &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;   :0. </li>";
                    //    //Total Available Faculty is " + collegeFacultycount + ".
                    //}
                    //faculty += "<li>Total faculty uploaded by the college for the A.Y. 2023-24. &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; : " + totalfacultycount + ".</li>";
                    //faculty += "<li>Total faculty required by the college for the A.Y. 2023-24. &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; : " + TotalAreaRequiredFaculty + ".</li>";
                    //faculty += "<li>Total faculty available during inspection &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; : " + regFacultyWothoutAbsents.Count + ".</li>";
                    //faculty += "<li>Total faculty appointed after inspection for the A.Y. 2022-23. &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; : " + regnos.Count + ".</li>";
                    //if (nodocumentsdetails != null && nodocumentsdetails.AICTENoOfFaculty != 0)
                    //{

                    //    faculty += "<li>Total faculty uploaded to AICTE for Extension of Approval for AY 2018-19 &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; : " + nodocumentsdetails.AICTENoOfFaculty + ".</li>";//facultyCounts.Select(i => i.specializationWiseFaculty).Sum()
                    //    faculty += "<li>Total faculty available during inspection with  qualifications as prescribed by AICTE &nbsp;&nbsp;&nbsp;:<b> " + collegeFacultycount + "</b>.</li>";
                    //}
                    //else
                    //{
                    //    faculty += "<li>Total faculty uploaded to AICTE for Extension of Approval for AY 2018-19 &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; : 0.</li>";
                    //    faculty += "<li>Total faculty available during inspection with  qualifications as prescribed by AICTE &nbsp;&nbsp;&nbsp;:<b> " + collegeFacultycount + "</b>.</li>";

                    //}
                    faculty += "<table width='100%' border='1' cellpadding='5' cellspacing='0'>";
                    //faculty += "<tr>";
                    //faculty += "<th align='left'>Total faculty uploaded by the college for the A.Y. 2023-24.</th><th align='left'>Total faculty required by the college for the A.Y. 2023-24.</th><th align='left'>Total faculty available during inspection</th><th align='left'>Total faculty appointed after inspection for the A.Y. 2022-23.</th>";
                    //faculty += "</tr>";
                    faculty += "<tr>";
                    //faculty += "<td>" + totalfacultycount + "</td><td>" + TotalAreaRequiredFaculty + "</td><td>" + regFacultyWothoutAbsents.Count + "</td><td>" + regnos.Count + "</td>";
                    faculty += "<td align='left'>Total faculty required by the college for the A.Y. 2023-24.</td><td>" + TotalAreaRequiredFaculty + "</td>";
                    faculty += "<td align='left'>Total faculty uploaded by the college for the A.Y. 2023-24.</td><td>" + totalfacultycount + "</td>";
                    faculty += "</tr>";
                    faculty += "<tr>";
                    faculty += "<td align='left'>Total faculty available during inspection.</td><td>" + regFacultyWothoutAbsents.Count + "</td>";
                    faculty += "<td align='left'>Total faculty appointed after inspection for the A.Y. 2022-23.</td><td>" + regnos.Count + "</td>";
                    faculty += "</tr>";
                    faculty += "<tr>";
                    faculty += "<td align='left'>Total faculty having required BAS.</td><td>" + (totalfacultycount - jntuh_registered_facultyBAS.Count) + "</td>";
                    faculty += "<td align='left'>Total faculty not having sufficient BAS.</td><td>" + (jntuh_registered_facultyBAS.Count) + "</td>";
                    faculty += "</tr>";
                    faculty += "</table>";


                    //int currentyearfaculty = 0;
                    //if (nodocumentsdetails != null && nodocumentsdetails.Currentyearfaculty != 0)
                    //{
                    // //   currentyearfaculty = (int)nodocumentsdetails.Currentyearfaculty;
                    //}

                    //  if (nodocumentsdetails != null && nodocumentsdetails.Lastyearfaculty != 0)



                    //faculty += "</ul>";

                    #endregion

                }
            }
            return faculty;
        }

        [Authorize(Roles = "Admin,SuperAdmin")]
        public string NewDeficienciesInFacultyBTech(int? collegeID)
        {
            string facultyclosure = string.Empty;
            string twintyfivepercentbelowcurces = string.Empty;
            string faculty = string.Empty;
            string facultyAdmittedIntakeZero = string.Empty;
            string facultyAdmittedIntakeZeroTable2 = string.Empty;
            int facultycount = 0;
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int prAy =
                db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
            int[] mbacollegeides = { 5, 67, 119, 174, 246, 296, 343, 355, 386, 394, 411, 413, 421, 424, 430, 449, 452 };
            int cid = Convert.ToInt32(collegeID);
            int[] shiftids = { 1, 2 };
            List<CollegeFacultyWithIntakeReport> facultyCountslist = new List<CollegeFacultyWithIntakeReport>();
            List<CollegeFacultyWithIntakeReport> facultylist = collegeFacultyNew(collegeID).Select(e => e).ToList();
            List<CollegeFacultyWithIntakeReport> facultyCounts = facultylist.Where(c => c.shiftId == 1).Select(e => e).ToList();//Where(c => c.shiftId == 1)
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
            List<CollegeFacultyWithIntakeReport> facultyCountper = facultylist.Where(c => (c.ispercentage == true && c.Proposedintake != 0 && c.Degree == "B.Tech") || c.Proposedintake == 0 && c.Degree == "B.Tech").Select(e => e).ToList();
            foreach (var itemmtech in facultyCountper)
            {
                if (itemmtech.collegeId == 72 && itemmtech.Department == "IT")
                {
                }
                else if (itemmtech.collegeId == 130 && itemmtech.Department == "IT")
                {

                }
                else
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
            }
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
                var CollegeSubmissionDate = Convert.ToDateTime(db.jntuh_college_edit_status.Where(e => e.collegeId == collegeID && e.academicyearId == prAy && e.IsCollegeEditable == false).Select(e => e.updatedOn).FirstOrDefault()).Date;
                // DateTime data = Convert.ToDateTime(CollegeSubmissionDate).Date;
                if (collegeStatus.AICTEStatus == true)
                {

                    var RegisteredFaculty = (from Reg in db.jntuh_registered_faculty
                                             join Clg in db.jntuh_college_faculty_registered on Reg.RegistrationNumber equals Clg.RegistrationNumber
                                             join ClgEdit in db.jntuh_college_edit_status on Clg.collegeId equals ClgEdit.collegeId
                                             where ClgEdit.IsCollegeEditable == false && ClgEdit.academicyearId == prAy && Clg.collegeId == collegeID
                                             select Reg).ToList();

                    int Absebtfaculty = RegisteredFaculty.Count(e => e.Absent == true);
                    int TotalFaculty = RegisteredFaculty.Count();

                    var Percentage = Math.Ceiling((((double)Absebtfaculty / (double)TotalFaculty) * 100));

                    faculty += "<p>Sub: - Affiliation for the Academic Year: 2020-21- Reg.</p>";
                    faculty += "<br/>";
                    faculty += "<table><tr><td style='vertical-align:top'>Ref:</td><td>";
                    faculty += "<ol type='1'>";
                    faculty += "<li>Your college online application dated:" + CollegeSubmissionDate + " for grant of Affiliation for the Academic Year 2017-18.</li>";
                    faculty += "<li>Your college FFC Inspection on  dated: ----------- </li>";
                    faculty += "</ol></td></tr></table>";
                    faculty += "<br/>";
                    faculty += "<p align='center'><b>****</b></p>";

                    faculty += "<p style='text-align:justfy'>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;As per the fact finding Committee inspection report it is noticed that " + Percentage + " of faculty members were absent against the number of faculty shown in online application for affiliation A117. It indicates that the College is not maintaining the minimum essential requirements for running the academic programs. Under these circumstances the SCA recommends <b><u>'No Admission Status'</u></b> for the A.Y. 2017-18 for the Institute. Clarification or explanation if any may be submitted within 10 days from the date of this letter</p>";

                    faculty += "<table width='100%'  cellspacing='0'>";
                    faculty += "<tr><td align='right' width='80%' style='font-size: 14px; font-weight: normal;'> <b>Sd /-</b></td></tr>";
                    faculty += "<tr><td align='right' width='80%' style='font-size: 14px; font-weight: normal;'> <b>REGISTRAR</b></td></tr>";
                    faculty += "</table>";


                }
                else if (collegeStatus.CollegeStatus == true)
                {
                    faculty += "<p>Sub: - Affiliation for the Academic Year: 2020-21- Reg.</p>";
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
            int remainingSpecializationFaculty = 0;
            int remainingPHDFaculty = 0;
            int remainingSpecializationwisePHDFaculty = 0;
            int SpecializationwisePHDFaculty = 0;
            int SpecializationwisePGFaculty = 0;
            int TotalCount = 0;
            int HumantitiesminimamRequireMet = 0;
            string HumantitiesminimamRequireMetStatus = "Yes";



            string[] ProposedIntakeZeroDeptName = facultyCounts.Where(e => e.Degree == "B.Tech" && (e.Proposedintake == 0) && !departments.Contains(e.Department)).Select(e => e.Specialization).Distinct().ToArray();
            var ProposedIntakeZeroDeptNameSpecIDs = facultyCounts.Where(e => e.Degree == "B.Tech" && (e.Proposedintake == 0) && !departments.Contains(e.Department)).Select(e => e.specializationId).Distinct().ToArray();
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
                    faculty += "<table  border='1' cellpadding='3' cellspacing='0' width='100%' style='font-family: Times New Roman; font-size: 9px; font-weight: normal; background-color: #fff; margin: 0 auto;'>";
                    faculty += "<tr>";
                    //faculty += "<th rowspan='2' style='text-align: center; vertical-align: top;'>SNo</th>";
                    //faculty += "<th rowspan='2' style='text-align: left; vertical-align: top;' >Dept</th>";
                    //faculty += "<th  rowspan='2' style='text-align: left; vertical-align: top;' >Degree</th>";
                    faculty += "<th style='text-align: left; vertical-align: top;' ><b>Specialization</b></th>";
                    //Written by Narayana
                    //faculty += "<th rowspan='2' style='text-align: left; vertical-align: top;' >Shift</th>";
                    //if (mbacollegeides.Contains(cid))
                    //{
                    //    faculty += "<th colspan='3' rowspan='2' style='text-align: center; vertical-align: top;' >Sum of Proposed &<br/> Previous Sanctioned Intake</th>";
                    //}
                    //else
                    //{
                    //    faculty += "<th colspan='3' style='text-align: center; vertical-align: top;' >Intake of Previous Three A.Y</th>";
                    //}
                    //faculty += "<th colspan='3' style='text-align: center; vertical-align: top;' >Intake of Previous Three A.Y</th>";
                    //faculty += "<th rowspan='2' style='text-align: center; vertical-align: top;' >Admitted Intake</th>";

                    faculty += "<th style='text-align: center; vertical-align: top;' ><b>Proposed Intake</b></th>";
                    //faculty += "<th rowspan='2' style='text-align: center; vertical-align: top;' >Total Department Faculty</th>";
                    //faculty += "<th colspan='2' style='text-align: center; vertical-align: top;' ><b>Specialization Wise Faculty</b></th>";
                    //faculty += "<th rowspan='2' style='text-align: center; vertical-align: top;' >Specialization Wise Faculty Available</th>";
                    //faculty += "<th rowspan='2' style='text-align: center; vertical-align: top;' >PG Specialization Faculty</th>";
                    //faculty += "<th colspan='2' style='text-align: center; vertical-align: top;' ><b>Faculty with Ph.D</b></th>";
                    //faculty += "<th rowspan='2' style='text-align: center; vertical-align: top;' >Available Ph.D faculty</th>";
                    faculty += "<th style='text-align: center; vertical-align: top;'><b>Required faculty</b></th>";
                    faculty += "<th style='text-align: center; vertical-align: top;'><b>Available faculty</b></th>";
                    faculty += "<th style='text-align: center; vertical-align: top;'><b>Required Ph.D. faculty</b></th>";
                    faculty += "<th style='text-align: center; vertical-align: top;'><b>Available Ph.D. faculty</b></th>";
                    faculty += "<th style='text-align: center; vertical-align: top;' ><b>Deficiency</b></th>";
                    faculty += "</tr>";
                    //if (mbacollegeides.Contains(cid))
                    //{
                    //    faculty += "<tr>";
                    //    //faculty += "<th colspan='3' style='text-align: center; vertical-align: top;'></th>";
                    //    //faculty += "<th style='text-align: center; vertical-align: top;'></th>";

                    //    //faculty += "<th style='text-align: center; vertical-align: top;'></th>";
                    //    faculty += "<th style='text-align: center; vertical-align: top;'>Required</th>";
                    //    faculty += "<th style='text-align: center; vertical-align: top;'>Available</th>";
                    //    faculty += "<th style='text-align: center; vertical-align: top;'>Required</th>";
                    //    faculty += "<th style='text-align: center; vertical-align: top;'>Available</th>";
                    //    faculty += "</tr>";
                    //}
                    //else
                    //{
                    //    faculty += "<tr>";
                    //    faculty += "<th style='text-align: center; vertical-align: top;'>JNTUH <br/>Sanctioned</th>";
                    //    faculty += "<th style='text-align: center; vertical-align: top;'>Admitted</th>";

                    //    faculty += "<th style='text-align: center; vertical-align: top;'>#Divisions</th>";
                    //    faculty += "<th style='text-align: center; vertical-align: top;'>Required</th>";
                    //    faculty += "<th style='text-align: center; vertical-align: top;'>Available</th>";
                    //    faculty += "<th style='text-align: center; vertical-align: top;'>Required</th>";
                    //    faculty += "<th style='text-align: center; vertical-align: top;'>Available</th>";
                    //    faculty += "</tr>";
                    //}

                    //faculty += "<tr>";
                    ////faculty += "<th style='text-align: center; vertical-align: top;'>AICTE <br/>Sanctioned</th>";
                    ////faculty += "<th style='text-align: center; vertical-align: top;'>JNTUH <br/>Sanctioned</th>";

                    ////faculty += "<th style='text-align: center; vertical-align: top;'>#Divisions</th>";
                    //faculty += "<th style='text-align: center; vertical-align: top;'>Required</th>";
                    //faculty += "<th style='text-align: center; vertical-align: top;'>Available</th>";
                    //faculty += "<th style='text-align: center; vertical-align: top;'>Required</th>";
                    //faculty += "<th style='text-align: center; vertical-align: top;'>Available</th>";
                    //faculty += "</tr>";
                    //facultyAdmittedIntakeZero += "<p><b>In so for as the remaining programs/courses applied for affiliation the University has issued a separate show cause notice Dt.___________ based on the discrepancy in faculty uploaded by the College to AICTE and University and zero admissions for more than one previous A.Y. in those programs/courses. </b></p>";
                    //facultyAdmittedIntakeZero += "<p><b>The admission status in certain programs/courses in the previous three academic years in which there were zero admissions for two or more than two academic years either due to non grant of affiliation or unable to make admissions at the Institute is shown in the following table. It is also observed that there is a mismatch of faculty information that the Institute has submitted to AICTE and the University. The EOA obtained from AICTE is based on the faculty data uploaded by the Institute </b></p>";
                    //facultyAdmittedIntakeZero += "<table width='100%' border='1' cellpadding='3' cellspacing='0' style='border-color: #ccc;'>";
                    //facultyAdmittedIntakeZero += "<tr>";
                    //facultyAdmittedIntakeZero += "<th style='text-align: center; vertical-align: top;'>SNo</th>";
                    //facultyAdmittedIntakeZero += "<th style='text-align: left; vertical-align: top;' >Department</th>";
                    //facultyAdmittedIntakeZero += "<th style='text-align: left; vertical-align: top;' >Degree</th>";
                    //facultyAdmittedIntakeZero += "<th style='text-align: left; vertical-align: top;' >Specialization</th>";
                    //facultyAdmittedIntakeZero += "<th style='text-align: center; vertical-align: top;' >Sanction Intake</th>";
                    //facultyAdmittedIntakeZero += "<th style='text-align: center; vertical-align: top;' >Admitted Intake</th>";
                    //facultyAdmittedIntakeZero += "<th style='text-align: center; vertical-align: top;' >Proposed Intake</th>";
                    //facultyAdmittedIntakeZero += "<th style='text-align: center; vertical-align: top;' >Affiliation Status 2016-17</th>";
                    //facultyAdmittedIntakeZero += "<th style='text-align: center; vertical-align: top;' >Affiliation Status 2015-16</th>";
                    //facultyAdmittedIntakeZero += "<th style='text-align: center; vertical-align: top;' >Affiliation Status 2014-15</th>";
                    //facultyAdmittedIntakeZero += "</tr>";

                    //facultyAdmittedIntakeZeroTable2 += "<table width='100%' border='1' cellpadding='3' cellspacing='0' style='border-color: #ccc;'>";
                    //facultyAdmittedIntakeZeroTable2 += "<tr>";
                    //facultyAdmittedIntakeZeroTable2 += "<th style='text-align: center; vertical-align: top;'>SNo</th>";
                    //facultyAdmittedIntakeZeroTable2 += "<th style='text-align: left; vertical-align: top;' >Department</th>";
                    //facultyAdmittedIntakeZeroTable2 += "<th style='text-align: left; vertical-align: top;' >Degree</th>";
                    //facultyAdmittedIntakeZeroTable2 += "<th style='text-align: left; vertical-align: top;' >Specialization</th>";
                    //facultyAdmittedIntakeZeroTable2 += "</tr>";

                    string[] admittedIntakeTwoormoreZeroDeptName1 = facultyCounts.Where(e => e.Degree == "B.Tech" && ((e.admittedIntake2 == 0 && e.admittedIntake3 == 0) || (e.admittedIntake3 == 0 && e.admittedIntake4 == 0) || (e.admittedIntake2 == 0 && e.admittedIntake4 == 0)) && !departments.Contains(e.Department)).Select(e => e.Department).Distinct().ToArray();

                    #region Intake not equal to Zero

                    // Getting M.Tech Civil Specialization ID's
                    int[] SpecializationIDS =
                        db.jntuh_specialization.Where(S => S.departmentId == 43).Select(S => S.id).ToArray();
                    int remainingFaculty2 = 0;
                    int facultyIndexnotEqualtoZeroIndex = 1;
                    int facultyIndexnotEqualtoZeroAdmittedIntakeIndex = 1;
                    int btechcount = facultyCounts.Where(
                        e =>
                            e.Proposedintake != 0 && e.ispercentage == false && e.Degree == "B.Tech" &&
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
                                    e.Proposedintake != 0 && e.ispercentage == false &&
                                    !ProposedIntakeZeroDeptName.Contains(e.Specialization)).Select(e => e).ToList())
                    {
                        #region Code without calculation admitted Intake

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
                            //SpecializationwisePHDFaculty = IntakeWisePhdForBtech(item.Proposedintake);
                            SpecializationwisePHDFaculty = IntakeWisePhdForBtech(item.SanctionIntake2); //SanctionIntake2 - last year approved intake
                        }
                        TotalCount =
                            facultyCounts.Where(
                                D =>
                                    D.Department == item.Department &&
                                    (D.Degree == "M.Tech" || D.Degree == "B.Tech") && D.Proposedintake != 0)
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
                        if ((item.collegeId == 72 && item.Department == "IT") || (item.collegeId == 130 && item.Department == "IT"))
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
                        //tFaculty =50;
                        //item.totalFaculty
                        int rFaculty = (int)Math.Ceiling(item.requiredFaculty);

                        if (departments.Contains(item.Department))
                        {
                            if (OthersSpecIds.Contains(item.specializationId))
                            {
                                double rid = (double)(firstYearRequired / 2);
                                rFaculty = (int)(Math.Ceiling(rid));

                                //othersRequiredfaculty = rFaculty;
                                //Commented By Narayana on 12-03-2018
                                //rFaculty = 1;
                                // othersRequiredfaculty = 1;

                                //rFaculty = ((int)firstYearRequired)/2;
                                //if (rFaculty <= 2)
                                //{
                                //    rFaculty = 2;
                                //    othersRequiredfaculty = 2;
                                //}
                                //else
                                //{
                                //    othersRequiredfaculty = rFaculty;
                                //}

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

                        //faculty += "<tr>";
                        //faculty +=
                        //    "<td class='col2' style='text-align: center; vertical-align: top;width:10px' rowspan='1'>" +
                        //    facultyIndexnotEqualtoZeroIndex + "</td>";
                        ////(facultyCounts.Where(e=>e.approvedIntake1!=0).Select(e=>e).ToList().IndexOf(item) + 1)
                        //if (item.isstarcourses == true)
                        //{
                        //    faculty += "<td class='col2' style='text-align: left; vertical-align: top;'>" +
                        //               item.Department + "#" + "</td>";
                        //}
                        //else
                        //{
                        //    faculty += "<td class='col2' style='text-align: left; vertical-align: top;'>" +
                        //               item.Department + "</td>";
                        //}
                        //faculty += "<td class='col2' style='text-align: left; vertical-align: top;'>" + item.Degree +
                        //           "</td>";
                        ////faculty += "<td class='col2' style='text-align: left; vertical-align: top;'>" + item.Specialization + "</td>";
                        if (item.Degree == "B.Tech")
                        {
                            //faculty += "<td class='col2' style='text-align: left; vertical-align: top;'>" +
                            //           item.Specialization + "</td>";
                        }
                        //else
                        //{
                        //    faculty += "<td class='col2' style='text-align: left; vertical-align: top;'>" +
                        //               item.Specialization + " -2" + "</td>";
                        //}
                        //Written by Narayana
                        //faculty += "<td class='col2' style='text-align: left; vertical-align: top;'>" + item.shiftId + "</td>";
                        if (departments.Contains(item.Department))
                        {
                            // faculty += "<td class='col2' style='text-align: center; vertical-align: top;'> </td>";
                            //faculty += "<td class='col2' style='text-align: center; vertical-align: top;'> </td>";
                            ////faculty += "<td class='col2' style='text-align: center; vertical-align: top;'> </td>";
                            //faculty +=
                            //    "<td colspan='2' class='col2' style='text-align: center; vertical-align: top;'> </td>";
                            //faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" +
                            //           totalBtechFirstYearIntake + "</td>";


                            //if (deptloop == 1)
                            //    faculty += "<td rowspan='" + TotalCount +
                            //               "'  class='col2' style='text-align: center; vertical-align: top;'>" +
                            //               item.totalFaculty + "</td>";
                            ////faculty += "<td class='col2' style='text-align: left; vertical-align: top;'>" + item.A416TotalFaculty + "</td>";
                            //if (OthersSpecIds.Contains(item.specializationId))
                            //{
                            //    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" +
                            //               Math.Ceiling(firstYearRequired / 2) + "</td>";
                            //    HumantitiesminimamRequireMet += item.totalFaculty;
                            //}
                            //else
                            //{
                            //    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" +
                            //               Math.Ceiling(firstYearRequired) + "</td>";
                            //    HumantitiesminimamRequireMet += item.totalFaculty;
                            //}

                            ////
                            //if (minimumRequirementMet != "YES")
                            //{
                            //    HumantitiesminimamRequireMetStatus = "NO";
                            //}

                            //faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" +
                            //                       totalBtechFirstYearIntake + "</td>";
                            //if (OthersSpecIds.Contains(item.specializationId))
                            //{
                            //    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" +
                            //               Math.Ceiling(firstYearRequired / 2) + "</td>";
                            //    HumantitiesminimamRequireMet += item.totalFaculty;
                            //}
                            //else
                            //{
                            //    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" +
                            //               Math.Ceiling(firstYearRequired) + "</td>";
                            //    HumantitiesminimamRequireMet += item.totalFaculty;
                            //}
                        }
                        else
                        {
                            //faculty += "<td class='col2' style='text-align: center; vertical-align: top;font-weight: bold'>" + item.AffliationStatus + "</td>";
                            if (item.Degree == "B.Techh")
                            {
                                //faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + item.totalAdmittedIntake + "</td>";

                                //faculty +=
                                //"<td class='col2' style='text-align: center; vertical-align: top;'><table width='100%' border='1' cellpadding='3' cellspacing='0' style='border-color: #ccc;'><tr><td><span>" +
                                //item.AICTESanctionIntake2 + "</span></td></tr><tr><td><span>" + item.AICTESanctionIntake3 +
                                //"</span></td></tr><tr><td><span>" + item.AICTESanctionIntake4 +
                                //"</span></td></tr></table></td>";
                                //faculty +=
                                //    "<td class='col2' style='text-align: center; vertical-align: top;'><table width='100%' border='1' cellpadding='3' cellspacing='0' style='border-color: #ccc;'><tr><td><span>" +
                                //    item.SanctionIntake2 + "</span></td></tr><tr><td><span>" + item.SanctionIntake3 +
                                //    "</span></td></tr><tr><td><span>" + item.SanctionIntake4 +
                                //    "</span></td></tr></table></td>";
                                ////faculty +=
                                ////    "<td class='col2' style='text-align: center; vertical-align: top;'><table width='100%' border='1' cellpadding='3' cellspacing='0' style='border-color: #ccc;'><tr><td><span>" +
                                ////    item.admittedIntake2 + "</span></td></tr><tr><td><span>" + item.admittedIntake3 +
                                ////    "</span></td></tr><tr><td><span>" + item.admittedIntake4 +
                                ////    "</span></td></tr></table></td>";

                                //faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>";
                                //faculty +=
                                //    "<table width='100%' border='1' cellpadding='3' cellspacing='0' style='border-color: #ccc;'>";
                                ////Written New on 03-02-2018
                                ////int SanctionIntakeHigest = Max(item.admittedIntake2, item.admittedIntake3,
                                ////    item.admittedIntake4);
                                ////SanctionIntakeHigest = GetBtechAdmittedIntake(SanctionIntakeHigest);
                                ////if (SanctionIntakeHigest >= item.Proposedintake)
                                ////{
                                ////    faculty += "<tr><td><span>" + GetSectionBasedonIntake(item.admittedIntake2) +
                                ////               "(A)" + "</span></td>";
                                ////    faculty += "</tr><tr><td><span>" + GetSectionBasedonIntake(item.admittedIntake3) +
                                ////               "(A)" + "</span></td></tr>";
                                ////    faculty += "<tr><td><span>" + GetSectionBasedonIntake(item.admittedIntake4) +
                                ////               "(A)" + "</span></td></tr>";

                                ////}
                                ////else
                                ////{
                                //int[] StrNewSpecs = new[] { 180, 181, 182, 183, 184, 185, 186, 187, 188, 189, 190, 191, 192, 193, 194, 195, 196 };

                                //if (StrNewSpecs.Contains(item.specializationId) && item.Proposedintake != 0 && item.SanctionIntake1 == 0 && item.SanctionIntake2 == 0 && item.SanctionIntake3 == 0)
                                //{
                                //    faculty += "<tr><td><span>0</span></td>";
                                //    faculty += "</tr><tr><td><span>0</span></td></tr>";
                                //    faculty += "<tr><td><span>0</span></td></tr>";
                                //}
                                //else
                                //{
                                //    faculty += "<tr><td><span>" + GetSectionBasedonIntake(item.SanctionIntake2) +
                                //               "</span></td>";
                                //    faculty += "</tr><tr><td><span>" + GetSectionBasedonIntake(item.SanctionIntake3) +
                                //               "</span></td></tr>";
                                //    faculty += "<tr><td><span>" + GetSectionBasedonIntake(item.SanctionIntake4) +
                                //               "</span></td></tr>";
                                //}

                                ////}
                                //faculty += "</table></td>";
                            }
                            else
                            {
                                //if (mbacollegeides.Contains(cid))
                                //{

                                //    faculty +=
                                //        "<td colspan='3' class='col2' style='text-align: center; vertical-align: top;'>" +
                                //        item.totalIntake + " </td>";
                                //}
                                //else
                                //{
                                //faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" +
                                //       item.AICTESanctionIntake2 + "</td>";
                                //faculty +=
                                //    "<td colspan='2' class='col2' style='text-align: center; vertical-align: top;'>" +
                                //    item.SanctionIntake2 + " </td>";
                                //}

                            }
                            //faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + item.totalIntake + "</td>";
                            //if (String.IsNullOrEmpty(item.Note))
                            //{
                            //    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" +
                            //               item.Proposedintake + "</td>";
                            //}
                            //else
                            //{
                            //    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" +
                            //               item.Proposedintake + " " + item.Note + "</td>";
                            //}
                            //if (deptloop == 1)
                            //    faculty += "<td rowspan='" + TotalCount +
                            //               "'  class='col2' style='text-align: center; vertical-align: top;'>" +
                            //               item.totalFaculty + "</td>";
                            //faculty += "<td class='col2' style='text-align: left; vertical-align: top;'>" + item.A416TotalFaculty + "</td>";
                            //if (item.Degree == "B.Techh")
                            //{
                            //    //decimal AF = Math.Ceiling(item.ArequiredFaculty);
                            //    //decimal SF = Math.Ceiling(item.SrequiredFaculty);
                            //    //string requriedfaculty = Math.Ceiling(item.requiredFaculty) + "/" + Math.Ceiling(item.SrequiredFaculty);
                            //    //faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" +
                            //    //       item.requiredFaculty + "</td>";
                            //}
                            if (item.Degree == "B.Tech")
                            {
                                //faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" +
                                //                   item.Proposedintake + "</td>";
                                //faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" +
                                //        Math.Ceiling(item.requiredFaculty) + "</td>";
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

                        if (item.Degree == "B.Tech")
                        {
                            //faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" +
                            //           adjustedFaculty + "</td>";
                        }
                        //    if (SpecializationIDS.Contains(item.specializationId))
                        //        faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + SpecializationwisePGFaculty + "</td>";
                        //else
                        //        faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + "" + "</td>";

                        //if (item.Degree == "M.Tech")
                        //    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" +
                        //               item.SpecializationspgFaculty + "</td>";
                        //else
                        //    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + "" +
                        //               "</td>";

                        if (minimumRequirementMet == "YES" && PhdminimumRequirementMet == "YES")
                        {
                            //if (rFaculty <= adjustedFaculty)
                            //    minimumRequirementMet = "-";//No Deficiency In faculty
                            //else
                            //    minimumRequirementMet = "Deficiency In faculty";

                            //if (SpecializationwisePHDFaculty <= adjustedPHDFaculty)
                            //    minimumRequirementMet += "-";//and No Deficiency In Phd
                            //else
                            //    minimumRequirementMet += " and Deficiency In Phd";

                            if (rFaculty <= adjustedFaculty && SpecializationwisePHDFaculty <= adjustedPHDFaculty)
                            {
                                minimumRequirementMet = "-";
                            }
                            else if (rFaculty > adjustedFaculty && SpecializationwisePHDFaculty <= adjustedPHDFaculty)
                            {
                                //if (item.Department == "AI&DS" || item.Department == "CSD" || item.Department == "AI&ML" || item.specializationId == 189)
                                //    minimumRequirementMet = "-";
                                //else
                                minimumRequirementMet = "Deficiency In faculty";
                            }
                            else if (rFaculty <= adjustedFaculty &&
                                     SpecializationwisePHDFaculty > adjustedPHDFaculty)
                            {
                                //if (item.Department == "AI&DS" || item.Department == "CSD" || item.Department == "AI&ML" || item.specializationId == 189)
                                //    minimumRequirementMet = "-";
                                //else
                                minimumRequirementMet = "Deficiency In Ph.D";
                            }
                            else
                            {
                                //if (item.Department == "AI&DS" || item.Department == "CSD" || item.Department == "AI&ML" || item.specializationId == 189)
                                //    minimumRequirementMet = "-";
                                //else
                                minimumRequirementMet = "Deficiency In faculty and Ph.D";
                            }


                        }
                        else if (minimumRequirementMet == "NO" && PhdminimumRequirementMet == "YES")
                        {
                            //if (rFaculty == adjustedFaculty)
                            //    minimumRequirementMet = "-";//No Deficiency In faculty
                            //else
                            //    minimumRequirementMet = "Deficiency In faculty";


                            //if (SpecializationwisePHDFaculty <= adjustedPHDFaculty)
                            //    minimumRequirementMet += "-";//and No Deficiency In Phd
                            //else
                            //    minimumRequirementMet += " and Deficiency In Phd";

                            if (rFaculty <= adjustedFaculty && SpecializationwisePHDFaculty <= adjustedPHDFaculty)
                            {
                                minimumRequirementMet = "-";
                            }
                            else if (rFaculty > adjustedFaculty && SpecializationwisePHDFaculty <= adjustedPHDFaculty)
                            {
                                //if (item.Department == "AI&DS" || item.Department == "CSD" || item.Department == "AI&ML" || item.specializationId == 189)
                                //    minimumRequirementMet = "-";
                                //else
                                minimumRequirementMet = "Deficiency In faculty";
                            }
                            else if (rFaculty <= adjustedFaculty &&
                                     SpecializationwisePHDFaculty > adjustedPHDFaculty)
                            {
                                //if (item.Department == "AI&DS" || item.Department == "CSD" || item.Department == "AI&ML" || item.specializationId == 189)
                                //    minimumRequirementMet = "-";
                                //else
                                minimumRequirementMet = "Deficiency In Ph.D";
                            }
                            else
                            {
                                //if (item.Department == "AI&DS" || item.Department == "CSD" || item.Department == "AI&ML" || item.specializationId == 189)
                                //    minimumRequirementMet = "-";
                                //else
                                minimumRequirementMet = "Deficiency In faculty and Ph.D";
                            }

                        }
                        else if (minimumRequirementMet == "YES" && PhdminimumRequirementMet == "NO")
                        {
                            //if (rFaculty <= adjustedFaculty)
                            //    minimumRequirementMet = "-";//No Deficiency In faculty
                            //else
                            //    minimumRequirementMet = "Deficiency In faculty";

                            //if (SpecializationwisePHDFaculty == adjustedPHDFaculty)
                            //    minimumRequirementMet += "-";// and No Deficiency In Phd
                            //else
                            //    minimumRequirementMet += " and Deficiency In Phd";
                            if (rFaculty <= adjustedFaculty && SpecializationwisePHDFaculty <= adjustedPHDFaculty)
                            {
                                minimumRequirementMet = "-";
                            }
                            else if (rFaculty > adjustedFaculty && SpecializationwisePHDFaculty <= adjustedPHDFaculty)
                            {
                                //if (item.Department == "AI&DS" || item.Department == "CSD" || item.Department == "AI&ML" || item.specializationId == 189)
                                //    minimumRequirementMet = "-";
                                //else
                                minimumRequirementMet = "Deficiency In faculty";
                            }
                            else if (rFaculty <= adjustedFaculty &&
                                     SpecializationwisePHDFaculty > adjustedPHDFaculty)
                            {
                                //if (item.Department == "AI&DS" || item.Department == "CSD" || item.Department == "AI&ML" || item.specializationId == 189)
                                //    minimumRequirementMet = "-";
                                //else
                                minimumRequirementMet = "Deficiency In Ph.D";
                            }
                            else
                            {
                                //if (item.Department == "AI&DS" || item.Department == "CSD" || item.Department == "AI&ML" || item.specializationId == 189)
                                //    minimumRequirementMet = "-";
                                //else
                                minimumRequirementMet = "Deficiency In faculty and Ph.D";
                            }

                        }

                        else if (minimumRequirementMet == "NO" && PhdminimumRequirementMet == "NO")
                        {
                            //if (rFaculty == adjustedFaculty)
                            //    minimumRequirementMet = "-";//No Deficiency In faculty
                            //else
                            //    minimumRequirementMet = "Deficiency In faculty";


                            //if (SpecializationwisePHDFaculty == adjustedPHDFaculty)
                            //    minimumRequirementMet += "-";// and No Deficiency In Phd
                            //else
                            //    minimumRequirementMet += " and Deficiency In Phd";
                            if (rFaculty <= adjustedFaculty && SpecializationwisePHDFaculty <= adjustedPHDFaculty)
                            {
                                minimumRequirementMet = "-";
                            }
                            else if (rFaculty > adjustedFaculty && SpecializationwisePHDFaculty <= adjustedPHDFaculty)
                            {
                                //if (item.Department == "AI&DS" || item.Department == "CSD" || item.Department == "AI&ML" || item.specializationId == 189)
                                //    minimumRequirementMet = "-";
                                //else
                                minimumRequirementMet = "Deficiency In faculty";
                            }
                            else if (rFaculty <= adjustedFaculty &&
                                     SpecializationwisePHDFaculty > adjustedPHDFaculty)
                            {
                                //if (item.Department == "AI&DS" || item.Department == "CSD" || item.Department == "AI&ML" || item.specializationId == 189)
                                //    minimumRequirementMet = "-";
                                //else
                                minimumRequirementMet = "Deficiency In Ph.D";
                            }
                            else
                            {
                                //if (item.Department == "AI&DS" || item.Department == "CSD" || item.Department == "AI&ML" || item.specializationId == 189)
                                //    minimumRequirementMet = "-";
                                //else
                                minimumRequirementMet = "Deficiency In faculty and Ph.D";
                            }
                        }


                        if (item.Degree == "B.Tech")
                        {
                            //faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" +
                            //       SpecializationwisePHDFaculty + "</td>";
                            ////if (deptloop == 1)
                            ////    faculty += "<td rowspan='" + TotalCount + "'  class='col2' style='text-align: center; vertical-align: top;'>" + SpecializationwisePHDFaculty + "</td>";

                            ////faculty += "<td   class='col2' style='text-align: center; vertical-align: top;'>" + SpecializationwisePHDFaculty + "</td>";

                            ////else
                            ////    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + "" + "</td>";

                            //faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" +
                            //           adjustedPHDFaculty + "</td>";
                            //faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" +
                            //           minimumRequirementMet + "</td>";

                            if (minimumRequirementMet != "-")
                            {
                                faculty += "<tr>";
                                if (item.Degree == "B.Tech")
                                {
                                    faculty += "<td class='col2' style='text-align: left; vertical-align: top;'>" +
                                               item.Specialization + "</td>";
                                }
                                if (departments.Contains(item.Department))
                                {
                                    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + totalBtechFirstYearIntake + "</td>";
                                    if (OthersSpecIds.Contains(item.specializationId))
                                    {
                                        faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + Math.Ceiling(firstYearRequired / 2) + "</td>";
                                        HumantitiesminimamRequireMet += item.totalFaculty;
                                    }
                                    else
                                    {
                                        faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + Math.Ceiling(firstYearRequired) + "</td>";
                                        HumantitiesminimamRequireMet += item.totalFaculty;
                                    }
                                }
                                else
                                {
                                    if (item.Degree == "B.Tech")
                                    {
                                        faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + item.Proposedintake + "</td>";
                                        faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + Math.Ceiling(item.requiredFaculty) + "</td>";
                                    }
                                }
                                if (item.Degree == "B.Tech")
                                {
                                    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + adjustedFaculty + "</td>";
                                }

                                faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + SpecializationwisePHDFaculty + "</td>";
                                faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + adjustedPHDFaculty + "</td>";
                                faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + minimumRequirementMet + "</td>";
                                faculty += "</tr>";
                            }
                            //else
                            //{
                            //    faculty += "<tr><td colspan = '7' class='col2' style='text-align: center; vertical-align: top;'>NIL</td></tr>";
                            //}

                        }



                        //faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" +
                        //           SpecializationwisePHDFaculty + "</td>";
                        ////if (deptloop == 1)
                        ////    faculty += "<td rowspan='" + TotalCount + "'  class='col2' style='text-align: center; vertical-align: top;'>" + SpecializationwisePHDFaculty + "</td>";

                        ////faculty += "<td   class='col2' style='text-align: center; vertical-align: top;'>" + SpecializationwisePHDFaculty + "</td>";

                        ////else
                        ////    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + "" + "</td>";

                        //faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" +
                        //           adjustedPHDFaculty + "</td>";
                        //faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" +
                        //           minimumRequirementMet + "</td>";

                        //faculty += "</tr>";

                        deptloop++;
                        SpecializationwisePHDFaculty = 0;
                        facultyIndexnotEqualtoZeroIndex++;

                        #endregion
                    }
                    faculty += "</table>";
                    faculty += "<p style='font-family: Times New Roman; font-size: 9px; font-weight: normal; background-color: #fff; margin: 0 auto;'>The faculty members having Biometric attendance less than 40% from the date of enabling Biometric attendance are not considered.</p>";
                    #endregion
                }
            }
            if (!faculty.Contains("Deficiency In"))
            {
                faculty = string.Empty;
                faculty = "<table  border='1' cellpadding='3' cellspacing='0' width='100%' style='font-family: Times New Roman; font-size: 9px; font-weight: normal; background-color: #fff; margin: 0 auto;'><tr><th style='text-align: left; vertical-align: top;' ><b>Specialization</b></th><th style='text-align: center; vertical-align: top;' ><b>Proposed Intake</b></th><th style='text-align: center; vertical-align: top;'><b>Required faculty</b></th><th style='text-align: center; vertical-align: top;'><b>Available faculty</b></th><th style='text-align: center; vertical-align: top;'><b>Required Ph.D. faculty</b></th><th style='text-align: center; vertical-align: top;'><b>Available Ph.D. faculty</b></th><th style='text-align: center; vertical-align: top;' ><b>Deficiency</b></th></tr><tr><td colspan = '7' class='col2' style='text-align: center; vertical-align: top;'><b>NIL</b></td></tr></table>";
            }
            return faculty;
        }

        [Authorize(Roles = "Admin,SuperAdmin")]
        public string NewDeficienciesInFaculty(int? collegeID)
        {
            string facultyclosure = string.Empty;
            string twintyfivepercentbelowcurces = string.Empty;
            string faculty = string.Empty;
            string facultyAdmittedIntakeZero = string.Empty;
            string facultyAdmittedIntakeZeroTable2 = string.Empty;
            int facultycount = 0;
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int prAy =
                db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
            int[] mbacollegeides = { 5, 67, 119, 174, 246, 296, 343, 355, 386, 394, 411, 413, 421, 424, 430, 449, 452 };
            int cid = Convert.ToInt32(collegeID);
            int[] shiftids = { 1, 2 };
            List<CollegeFacultyWithIntakeReport> facultyCountslist = new List<CollegeFacultyWithIntakeReport>();
            List<CollegeFacultyWithIntakeReport> facultylist = collegeFacultyNew(collegeID).Select(e => e).ToList();
            List<CollegeFacultyWithIntakeReport> facultyCounts = facultylist.Where(c => c.shiftId == 1).Select(e => e).ToList();//Where(c => c.shiftId == 1)
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
            List<CollegeFacultyWithIntakeReport> facultyCountper = facultylist.Where(c => (c.ispercentage == true && c.Proposedintake != 0 && c.Degree == "B.Tech") || c.Proposedintake == 0 && c.Degree == "B.Tech").Select(e => e).ToList();
            foreach (var itemmtech in facultyCountper)
            {
                if (itemmtech.collegeId == 72 && itemmtech.Department == "IT")
                {
                }
                else if (itemmtech.collegeId == 130 && itemmtech.Department == "IT")
                {

                }
                else
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
            }
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
                var CollegeSubmissionDate = Convert.ToDateTime(db.jntuh_college_edit_status.Where(e => e.collegeId == collegeID && e.academicyearId == prAy && e.IsCollegeEditable == false).Select(e => e.updatedOn).FirstOrDefault()).Date;
                // DateTime data = Convert.ToDateTime(CollegeSubmissionDate).Date;
                if (collegeStatus.AICTEStatus == true)
                {

                    var RegisteredFaculty = (from Reg in db.jntuh_registered_faculty
                                             join Clg in db.jntuh_college_faculty_registered on Reg.RegistrationNumber equals Clg.RegistrationNumber
                                             join ClgEdit in db.jntuh_college_edit_status on Clg.collegeId equals ClgEdit.collegeId
                                             where ClgEdit.IsCollegeEditable == false && ClgEdit.academicyearId == prAy && Clg.collegeId == collegeID
                                             select Reg).ToList();

                    int Absebtfaculty = RegisteredFaculty.Count(e => e.Absent == true);
                    int TotalFaculty = RegisteredFaculty.Count();

                    var Percentage = Math.Ceiling((((double)Absebtfaculty / (double)TotalFaculty) * 100));

                    faculty += "<p>Sub: - Affiliation for the Academic Year: 2020-21- Reg.</p>";
                    faculty += "<br/>";
                    faculty += "<table><tr><td style='vertical-align:top'>Ref:</td><td>";
                    faculty += "<ol type='1'>";
                    faculty += "<li>Your college online application dated:" + CollegeSubmissionDate + " for grant of Affiliation for the Academic Year 2017-18.</li>";
                    faculty += "<li>Your college FFC Inspection on  dated: ----------- </li>";
                    faculty += "</ol></td></tr></table>";
                    faculty += "<br/>";
                    faculty += "<p align='center'><b>****</b></p>";

                    faculty += "<p style='text-align:justfy'>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;As per the fact finding Committee inspection report it is noticed that " + Percentage + " of faculty members were absent against the number of faculty shown in online application for affiliation A117. It indicates that the College is not maintaining the minimum essential requirements for running the academic programs. Under these circumstances the SCA recommends <b><u>'No Admission Status'</u></b> for the A.Y. 2017-18 for the Institute. Clarification or explanation if any may be submitted within 10 days from the date of this letter</p>";

                    faculty += "<table width='100%'  cellspacing='0'>";
                    faculty += "<tr><td align='right' width='80%' style='font-size: 14px; font-weight: normal;'> <b>Sd /-</b></td></tr>";
                    faculty += "<tr><td align='right' width='80%' style='font-size: 14px; font-weight: normal;'> <b>REGISTRAR</b></td></tr>";
                    faculty += "</table>";


                }
                else if (collegeStatus.CollegeStatus == true)
                {
                    faculty += "<p>Sub: - Affiliation for the Academic Year: 2020-21- Reg.</p>";
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
            int remainingSpecializationFaculty = 0;
            int remainingPHDFaculty = 0;
            int remainingSpecializationwisePHDFaculty = 0;
            int SpecializationwisePHDFaculty = 0;
            int SpecializationwisePGFaculty = 0;
            int TotalCount = 0;
            int HumantitiesminimamRequireMet = 0;
            string HumantitiesminimamRequireMetStatus = "Yes";



            string[] ProposedIntakeZeroDeptName = facultyCounts.Where(e => e.Degree == "B.Tech" && (e.Proposedintake == 0) && !departments.Contains(e.Department)).Select(e => e.Specialization).Distinct().ToArray();
            var ProposedIntakeZeroDeptNameSpecIDs = facultyCounts.Where(e => e.Degree == "B.Tech" && (e.Proposedintake == 0) && !departments.Contains(e.Department)).Select(e => e.specializationId).Distinct().ToArray();
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
                    faculty += "<table  border='1' cellpadding='3' cellspacing='0' width='100%' style='font-family: Times New Roman; font-size: 9px; font-weight: normal; background-color: #fff; margin: 0 auto;'>";
                    faculty += "<tr>";
                    //faculty += "<th rowspan='2' style='text-align: center; vertical-align: top;'>SNo</th>";
                    //faculty += "<th rowspan='2' style='text-align: left; vertical-align: top;' >Dept</th>";
                    //faculty += "<th  rowspan='2' style='text-align: left; vertical-align: top;' >Degree</th>";
                    faculty += "<th style='text-align: left; vertical-align: top;' ><b>Specialization</b></th>";
                    //Written by Narayana
                    //faculty += "<th rowspan='2' style='text-align: left; vertical-align: top;' >Shift</th>";
                    //if (mbacollegeides.Contains(cid))
                    //{
                    //    faculty += "<th colspan='3' rowspan='2' style='text-align: center; vertical-align: top;' >Sum of Proposed &<br/> Previous Sanctioned Intake</th>";
                    //}
                    //else
                    //{
                    //    faculty += "<th colspan='3' style='text-align: center; vertical-align: top;' >Intake of Previous Three A.Y</th>";
                    //}
                    //faculty += "<th colspan='3' style='text-align: center; vertical-align: top;' >Intake of Previous Three A.Y</th>";
                    //faculty += "<th rowspan='2' style='text-align: center; vertical-align: top;' >Admitted Intake</th>";

                    faculty += "<th style='text-align: center; vertical-align: top;' ><b>Proposed Intake</b></th>";
                    //faculty += "<th rowspan='2' style='text-align: center; vertical-align: top;' >Total Department Faculty</th>";
                    //faculty += "<th colspan='2' style='text-align: center; vertical-align: top;' ><b>Specialization Wise Faculty</b></th>";
                    //faculty += "<th rowspan='2' style='text-align: center; vertical-align: top;' >Specialization Wise Faculty Available</th>";
                    //faculty += "<th rowspan='2' style='text-align: center; vertical-align: top;' >PG Specialization Faculty</th>";
                    //faculty += "<th colspan='2' style='text-align: center; vertical-align: top;' ><b>Faculty with Ph.D</b></th>";
                    //faculty += "<th rowspan='2' style='text-align: center; vertical-align: top;' >Available Ph.D faculty</th>";
                    faculty += "<th style='text-align: center; vertical-align: top;'><b>Required faculty</b></th>";
                    faculty += "<th style='text-align: center; vertical-align: top;'><b>Available faculty</b></th>";
                    faculty += "<th style='text-align: center; vertical-align: top;'><b>Required Ph.D. faculty</b></th>";
                    faculty += "<th style='text-align: center; vertical-align: top;'><b>Available Ph.D. faculty</b></th>";
                    faculty += "<th style='text-align: center; vertical-align: top;' ><b>Deficiency In</b></th>";
                    faculty += "</tr>";
                    //if (mbacollegeides.Contains(cid))
                    //{
                    //    faculty += "<tr>";
                    //    //faculty += "<th colspan='3' style='text-align: center; vertical-align: top;'></th>";
                    //    //faculty += "<th style='text-align: center; vertical-align: top;'></th>";

                    //    //faculty += "<th style='text-align: center; vertical-align: top;'></th>";
                    //    faculty += "<th style='text-align: center; vertical-align: top;'>Required</th>";
                    //    faculty += "<th style='text-align: center; vertical-align: top;'>Available</th>";
                    //    faculty += "<th style='text-align: center; vertical-align: top;'>Required</th>";
                    //    faculty += "<th style='text-align: center; vertical-align: top;'>Available</th>";
                    //    faculty += "</tr>";
                    //}
                    //else
                    //{
                    //    faculty += "<tr>";
                    //    faculty += "<th style='text-align: center; vertical-align: top;'>JNTUH <br/>Sanctioned</th>";
                    //    faculty += "<th style='text-align: center; vertical-align: top;'>Admitted</th>";

                    //    faculty += "<th style='text-align: center; vertical-align: top;'>#Divisions</th>";
                    //    faculty += "<th style='text-align: center; vertical-align: top;'>Required</th>";
                    //    faculty += "<th style='text-align: center; vertical-align: top;'>Available</th>";
                    //    faculty += "<th style='text-align: center; vertical-align: top;'>Required</th>";
                    //    faculty += "<th style='text-align: center; vertical-align: top;'>Available</th>";
                    //    faculty += "</tr>";
                    //}

                    //faculty += "<tr>";
                    ////faculty += "<th style='text-align: center; vertical-align: top;'>AICTE <br/>Sanctioned</th>";
                    ////faculty += "<th style='text-align: center; vertical-align: top;'>JNTUH <br/>Sanctioned</th>";

                    ////faculty += "<th style='text-align: center; vertical-align: top;'>#Divisions</th>";
                    //faculty += "<th style='text-align: center; vertical-align: top;'>Required</th>";
                    //faculty += "<th style='text-align: center; vertical-align: top;'>Available</th>";
                    //faculty += "<th style='text-align: center; vertical-align: top;'>Required</th>";
                    //faculty += "<th style='text-align: center; vertical-align: top;'>Available</th>";
                    //faculty += "</tr>";
                    //facultyAdmittedIntakeZero += "<p><b>In so for as the remaining programs/courses applied for affiliation the University has issued a separate show cause notice Dt.___________ based on the discrepancy in faculty uploaded by the College to AICTE and University and zero admissions for more than one previous A.Y. in those programs/courses. </b></p>";
                    //facultyAdmittedIntakeZero += "<p><b>The admission status in certain programs/courses in the previous three academic years in which there were zero admissions for two or more than two academic years either due to non grant of affiliation or unable to make admissions at the Institute is shown in the following table. It is also observed that there is a mismatch of faculty information that the Institute has submitted to AICTE and the University. The EOA obtained from AICTE is based on the faculty data uploaded by the Institute </b></p>";
                    //facultyAdmittedIntakeZero += "<table width='100%' border='1' cellpadding='3' cellspacing='0' style='border-color: #ccc;'>";
                    //facultyAdmittedIntakeZero += "<tr>";
                    //facultyAdmittedIntakeZero += "<th style='text-align: center; vertical-align: top;'>SNo</th>";
                    //facultyAdmittedIntakeZero += "<th style='text-align: left; vertical-align: top;' >Department</th>";
                    //facultyAdmittedIntakeZero += "<th style='text-align: left; vertical-align: top;' >Degree</th>";
                    //facultyAdmittedIntakeZero += "<th style='text-align: left; vertical-align: top;' >Specialization</th>";
                    //facultyAdmittedIntakeZero += "<th style='text-align: center; vertical-align: top;' >Sanction Intake</th>";
                    //facultyAdmittedIntakeZero += "<th style='text-align: center; vertical-align: top;' >Admitted Intake</th>";
                    //facultyAdmittedIntakeZero += "<th style='text-align: center; vertical-align: top;' >Proposed Intake</th>";
                    //facultyAdmittedIntakeZero += "<th style='text-align: center; vertical-align: top;' >Affiliation Status 2016-17</th>";
                    //facultyAdmittedIntakeZero += "<th style='text-align: center; vertical-align: top;' >Affiliation Status 2015-16</th>";
                    //facultyAdmittedIntakeZero += "<th style='text-align: center; vertical-align: top;' >Affiliation Status 2014-15</th>";
                    //facultyAdmittedIntakeZero += "</tr>";

                    //facultyAdmittedIntakeZeroTable2 += "<table width='100%' border='1' cellpadding='3' cellspacing='0' style='border-color: #ccc;'>";
                    //facultyAdmittedIntakeZeroTable2 += "<tr>";
                    //facultyAdmittedIntakeZeroTable2 += "<th style='text-align: center; vertical-align: top;'>SNo</th>";
                    //facultyAdmittedIntakeZeroTable2 += "<th style='text-align: left; vertical-align: top;' >Department</th>";
                    //facultyAdmittedIntakeZeroTable2 += "<th style='text-align: left; vertical-align: top;' >Degree</th>";
                    //facultyAdmittedIntakeZeroTable2 += "<th style='text-align: left; vertical-align: top;' >Specialization</th>";
                    //facultyAdmittedIntakeZeroTable2 += "</tr>";

                    string[] admittedIntakeTwoormoreZeroDeptName1 = facultyCounts.Where(e => e.Degree == "B.Tech" && ((e.admittedIntake2 == 0 && e.admittedIntake3 == 0) || (e.admittedIntake3 == 0 && e.admittedIntake4 == 0) || (e.admittedIntake2 == 0 && e.admittedIntake4 == 0)) && !departments.Contains(e.Department)).Select(e => e.Department).Distinct().ToArray();

                    #region Intake not equal to Zero

                    // Getting M.Tech Civil Specialization ID's
                    int[] SpecializationIDS =
                        db.jntuh_specialization.Where(S => S.departmentId == 43).Select(S => S.id).ToArray();
                    int remainingFaculty2 = 0;
                    int facultyIndexnotEqualtoZeroIndex = 1;
                    int facultyIndexnotEqualtoZeroAdmittedIntakeIndex = 1;
                    int btechcount = facultyCounts.Where(
                        e =>
                            e.Proposedintake != 0 && e.ispercentage == false && e.Degree == "B.Tech" &&
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


                    var mtechcourses = facultyCounts.Where(
                                e => e.Proposedintake != 0 && e.ispercentage == false && e.Degree == "M.Tech" &&
                                    !ProposedIntakeZeroDeptName.Contains(e.Specialization)).Select(e => e).ToList();
                    if (mtechcourses.Count == 0)
                    {
                        faculty = string.Empty;
                        faculty += "<table border='1' cellpadding='3' cellspacing='0' width='100%' style='font-family: Times New Roman; font-size: 9px; font-weight: normal; background-color: #fff; margin: 0 auto;'>";
                        faculty += "<tr><td align='center'> <b>NIL</b></td></tr>";
                    }
                    else
                    {
                        foreach (
                            var item in
                                facultyCounts.Where(
                                    e =>
                                        e.Proposedintake != 0 && e.ispercentage == false &&
                                        !ProposedIntakeZeroDeptName.Contains(e.Specialization)).Select(e => e).ToList())
                        {
                            #region Code without calculation admitted Intake

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
                                //SpecializationwisePHDFaculty = IntakeWisePhdForBtech(item.Proposedintake);
                                SpecializationwisePHDFaculty = IntakeWisePhdForBtech(item.SanctionIntake2); //SanctionIntake2 - last year approved intake
                            }
                            TotalCount =
                                facultyCounts.Where(
                                    D =>
                                        D.Department == item.Department &&
                                        (D.Degree == "M.Tech" || D.Degree == "B.Tech") && D.Proposedintake != 0)
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
                            if ((item.collegeId == 72 && item.Department == "IT") || (item.collegeId == 130 && item.Department == "IT"))
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
                            //tFaculty =50;
                            //item.totalFaculty
                            int rFaculty = (int)Math.Ceiling(item.requiredFaculty);

                            if (departments.Contains(item.Department))
                            {
                                if (OthersSpecIds.Contains(item.specializationId))
                                {
                                    double rid = (double)(firstYearRequired / 2);
                                    rFaculty = (int)(Math.Ceiling(rid));

                                    //othersRequiredfaculty = rFaculty;
                                    //Commented By Narayana on 12-03-2018
                                    //rFaculty = 1;
                                    // othersRequiredfaculty = 1;

                                    //rFaculty = ((int)firstYearRequired)/2;
                                    //if (rFaculty <= 2)
                                    //{
                                    //    rFaculty = 2;
                                    //    othersRequiredfaculty = 2;
                                    //}
                                    //else
                                    //{
                                    //    othersRequiredfaculty = rFaculty;
                                    //}

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
                            //if (minimumRequirementMet != "YES")
                            //{
                                faculty += "<tr>";
                                //faculty +=
                                //    "<td class='col2' style='text-align: center; vertical-align: top;width:10px' rowspan='1'>" +
                                //    facultyIndexnotEqualtoZeroIndex + "</td>";
                                ////(facultyCounts.Where(e=>e.approvedIntake1!=0).Select(e=>e).ToList().IndexOf(item) + 1)
                                //if (item.isstarcourses == true)
                                //{
                                //    faculty += "<td class='col2' style='text-align: left; vertical-align: top;'>" +
                                //               item.Department + "#" + "</td>";
                                //}
                                //else
                                //{
                                //    faculty += "<td class='col2' style='text-align: left; vertical-align: top;'>" +
                                //               item.Department + "</td>";
                                //}
                                //faculty += "<td class='col2' style='text-align: left; vertical-align: top;'>" + item.Degree +
                                //           "</td>";
                                ////faculty += "<td class='col2' style='text-align: left; vertical-align: top;'>" + item.Specialization + "</td>";
                                if (item.Degree == "M.Tech")
                                {
                                    faculty += "<td class='col2' style='text-align: left; vertical-align: top;'>" +
                                               item.Specialization + "</td>";
                                }
                                //else
                                //{
                                //    faculty += "<td class='col2' style='text-align: left; vertical-align: top;'>" +
                                //               item.Specialization + " -2" + "</td>";
                                //}
                                //Written by Narayana
                                //faculty += "<td class='col2' style='text-align: left; vertical-align: top;'>" + item.shiftId + "</td>";
                                if (departments.Contains(item.Department))
                                {
                                    // faculty += "<td class='col2' style='text-align: center; vertical-align: top;'> </td>";
                                    //faculty += "<td class='col2' style='text-align: center; vertical-align: top;'> </td>";
                                    ////faculty += "<td class='col2' style='text-align: center; vertical-align: top;'> </td>";
                                    //faculty +=
                                    //    "<td colspan='2' class='col2' style='text-align: center; vertical-align: top;'> </td>";
                                    //faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" +
                                    //           totalBtechFirstYearIntake + "</td>";


                                    //if (deptloop == 1)
                                    //    faculty += "<td rowspan='" + TotalCount +
                                    //               "'  class='col2' style='text-align: center; vertical-align: top;'>" +
                                    //               item.totalFaculty + "</td>";
                                    ////faculty += "<td class='col2' style='text-align: left; vertical-align: top;'>" + item.A416TotalFaculty + "</td>";
                                    //if (OthersSpecIds.Contains(item.specializationId))
                                    //{
                                    //    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" +
                                    //               Math.Ceiling(firstYearRequired / 2) + "</td>";
                                    //    HumantitiesminimamRequireMet += item.totalFaculty;
                                    //}
                                    //else
                                    //{
                                    //    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" +
                                    //               Math.Ceiling(firstYearRequired) + "</td>";
                                    //    HumantitiesminimamRequireMet += item.totalFaculty;
                                    //}

                                    ////
                                    //if (minimumRequirementMet != "YES")
                                    //{
                                    //    HumantitiesminimamRequireMetStatus = "NO";
                                    //}
                                }
                                else
                                {
                                    //faculty += "<td class='col2' style='text-align: center; vertical-align: top;font-weight: bold'>" + item.AffliationStatus + "</td>";
                                    if (item.Degree == "B.Techh")
                                    {
                                        //faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + item.totalAdmittedIntake + "</td>";

                                        //faculty +=
                                        //"<td class='col2' style='text-align: center; vertical-align: top;'><table width='100%' border='1' cellpadding='3' cellspacing='0' style='border-color: #ccc;'><tr><td><span>" +
                                        //item.AICTESanctionIntake2 + "</span></td></tr><tr><td><span>" + item.AICTESanctionIntake3 +
                                        //"</span></td></tr><tr><td><span>" + item.AICTESanctionIntake4 +
                                        //"</span></td></tr></table></td>";
                                        //faculty +=
                                        //    "<td class='col2' style='text-align: center; vertical-align: top;'><table width='100%' border='1' cellpadding='3' cellspacing='0' style='border-color: #ccc;'><tr><td><span>" +
                                        //    item.SanctionIntake2 + "</span></td></tr><tr><td><span>" + item.SanctionIntake3 +
                                        //    "</span></td></tr><tr><td><span>" + item.SanctionIntake4 +
                                        //    "</span></td></tr></table></td>";
                                        ////faculty +=
                                        ////    "<td class='col2' style='text-align: center; vertical-align: top;'><table width='100%' border='1' cellpadding='3' cellspacing='0' style='border-color: #ccc;'><tr><td><span>" +
                                        ////    item.admittedIntake2 + "</span></td></tr><tr><td><span>" + item.admittedIntake3 +
                                        ////    "</span></td></tr><tr><td><span>" + item.admittedIntake4 +
                                        ////    "</span></td></tr></table></td>";

                                        //faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>";
                                        //faculty +=
                                        //    "<table width='100%' border='1' cellpadding='3' cellspacing='0' style='border-color: #ccc;'>";
                                        ////Written New on 03-02-2018
                                        ////int SanctionIntakeHigest = Max(item.admittedIntake2, item.admittedIntake3,
                                        ////    item.admittedIntake4);
                                        ////SanctionIntakeHigest = GetBtechAdmittedIntake(SanctionIntakeHigest);
                                        ////if (SanctionIntakeHigest >= item.Proposedintake)
                                        ////{
                                        ////    faculty += "<tr><td><span>" + GetSectionBasedonIntake(item.admittedIntake2) +
                                        ////               "(A)" + "</span></td>";
                                        ////    faculty += "</tr><tr><td><span>" + GetSectionBasedonIntake(item.admittedIntake3) +
                                        ////               "(A)" + "</span></td></tr>";
                                        ////    faculty += "<tr><td><span>" + GetSectionBasedonIntake(item.admittedIntake4) +
                                        ////               "(A)" + "</span></td></tr>";

                                        ////}
                                        ////else
                                        ////{
                                        //int[] StrNewSpecs = new[] { 180, 181, 182, 183, 184, 185, 186, 187, 188, 189, 190, 191, 192, 193, 194, 195, 196 };

                                        //if (StrNewSpecs.Contains(item.specializationId) && item.Proposedintake != 0 && item.SanctionIntake1 == 0 && item.SanctionIntake2 == 0 && item.SanctionIntake3 == 0)
                                        //{
                                        //    faculty += "<tr><td><span>0</span></td>";
                                        //    faculty += "</tr><tr><td><span>0</span></td></tr>";
                                        //    faculty += "<tr><td><span>0</span></td></tr>";
                                        //}
                                        //else
                                        //{
                                        //    faculty += "<tr><td><span>" + GetSectionBasedonIntake(item.SanctionIntake2) +
                                        //               "</span></td>";
                                        //    faculty += "</tr><tr><td><span>" + GetSectionBasedonIntake(item.SanctionIntake3) +
                                        //               "</span></td></tr>";
                                        //    faculty += "<tr><td><span>" + GetSectionBasedonIntake(item.SanctionIntake4) +
                                        //               "</span></td></tr>";
                                        //}

                                        ////}
                                        //faculty += "</table></td>";
                                    }
                                    else
                                    {
                                        //if (mbacollegeides.Contains(cid))
                                        //{

                                        //    faculty +=
                                        //        "<td colspan='3' class='col2' style='text-align: center; vertical-align: top;'>" +
                                        //        item.totalIntake + " </td>";
                                        //}
                                        //else
                                        //{
                                        //faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" +
                                        //       item.AICTESanctionIntake2 + "</td>";
                                        //faculty +=
                                        //    "<td colspan='2' class='col2' style='text-align: center; vertical-align: top;'>" +
                                        //    item.SanctionIntake2 + " </td>";
                                        //}

                                    }
                                    //faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + item.totalIntake + "</td>";
                                    //if (String.IsNullOrEmpty(item.Note))
                                    //{
                                    //    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" +
                                    //               item.Proposedintake + "</td>";
                                    //}
                                    //else
                                    //{
                                    //    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" +
                                    //               item.Proposedintake + " " + item.Note + "</td>";
                                    //}
                                    //if (deptloop == 1)
                                    //    faculty += "<td rowspan='" + TotalCount +
                                    //               "'  class='col2' style='text-align: center; vertical-align: top;'>" +
                                    //               item.totalFaculty + "</td>";
                                    //faculty += "<td class='col2' style='text-align: left; vertical-align: top;'>" + item.A416TotalFaculty + "</td>";
                                    //if (item.Degree == "B.Techh")
                                    //{
                                    //    //decimal AF = Math.Ceiling(item.ArequiredFaculty);
                                    //    //decimal SF = Math.Ceiling(item.SrequiredFaculty);
                                    //    //string requriedfaculty = Math.Ceiling(item.requiredFaculty) + "/" + Math.Ceiling(item.SrequiredFaculty);
                                    //    //faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" +
                                    //    //       item.requiredFaculty + "</td>";
                                    //}
                                    if (item.Degree == "M.Tech")
                                    {
                                        faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" +
                                                           item.Proposedintake + "</td>";
                                        faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" +
                                                Math.Ceiling(item.requiredFaculty) + "</td>";
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

                                if (item.Degree == "M.Tech")
                                {
                                    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" +
                                               adjustedFaculty + "</td>";
                                }
                                //    if (SpecializationIDS.Contains(item.specializationId))
                                //        faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + SpecializationwisePGFaculty + "</td>";
                                //else
                                //        faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + "" + "</td>";

                                //if (item.Degree == "M.Tech")
                                //    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" +
                                //               item.SpecializationspgFaculty + "</td>";
                                //else
                                //    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + "" +
                                //               "</td>";

                                if (minimumRequirementMet == "YES" && PhdminimumRequirementMet == "YES")
                                {
                                    //if (rFaculty <= adjustedFaculty)
                                    //    minimumRequirementMet = "-";//No Deficiency In faculty
                                    //else
                                    //    minimumRequirementMet = "Deficiency In faculty";

                                    //if (SpecializationwisePHDFaculty <= adjustedPHDFaculty)
                                    //    minimumRequirementMet += "-";//and No Deficiency In Phd
                                    //else
                                    //    minimumRequirementMet += " and Deficiency In Phd";

                                    if (rFaculty <= adjustedFaculty && SpecializationwisePHDFaculty <= adjustedPHDFaculty)
                                    {
                                        minimumRequirementMet = "-";
                                    }
                                    else if (rFaculty > adjustedFaculty && SpecializationwisePHDFaculty <= adjustedPHDFaculty)
                                    {
                                        //if (item.Department == "AI&DS" || item.Department == "CSD" || item.Department == "AI&ML" || item.specializationId == 189)
                                        //    minimumRequirementMet = "-";
                                        //else
                                        minimumRequirementMet = (Math.Ceiling(item.requiredFaculty) - adjustedFaculty) + " faculty";
                                    }
                                    else if (rFaculty <= adjustedFaculty &&
                                             SpecializationwisePHDFaculty > adjustedPHDFaculty)
                                    {
                                        //if (item.Department == "AI&DS" || item.Department == "CSD" || item.Department == "AI&ML" || item.specializationId == 189)
                                        //    minimumRequirementMet = "-";
                                        //else
                                        minimumRequirementMet = (SpecializationwisePHDFaculty - adjustedPHDFaculty) + " Ph.D";
                                    }
                                    else
                                    {
                                        //if (item.Department == "AI&DS" || item.Department == "CSD" || item.Department == "AI&ML" || item.specializationId == 189)
                                        //    minimumRequirementMet = "-";
                                        //else
                                        minimumRequirementMet = (Math.Ceiling(item.requiredFaculty) - adjustedFaculty) + " faculty and " + (SpecializationwisePHDFaculty - adjustedPHDFaculty) + " Ph.D";
                                    }


                                }
                                else if (minimumRequirementMet == "NO" && PhdminimumRequirementMet == "YES")
                                {
                                    //if (rFaculty == adjustedFaculty)
                                    //    minimumRequirementMet = "-";//No Deficiency In faculty
                                    //else
                                    //    minimumRequirementMet = "Deficiency In faculty";


                                    //if (SpecializationwisePHDFaculty <= adjustedPHDFaculty)
                                    //    minimumRequirementMet += "-";//and No Deficiency In Phd
                                    //else
                                    //    minimumRequirementMet += " and Deficiency In Phd";

                                    if (rFaculty <= adjustedFaculty && SpecializationwisePHDFaculty <= adjustedPHDFaculty)
                                    {
                                        minimumRequirementMet = "-";
                                    }
                                    else if (rFaculty > adjustedFaculty && SpecializationwisePHDFaculty <= adjustedPHDFaculty)
                                    {
                                        //if (item.Department == "AI&DS" || item.Department == "CSD" || item.Department == "AI&ML" || item.specializationId == 189)
                                        //    minimumRequirementMet = "-";
                                        //else
                                        minimumRequirementMet = (Math.Ceiling(item.requiredFaculty) - adjustedFaculty) + " faculty";
                                    }
                                    else if (rFaculty <= adjustedFaculty &&
                                             SpecializationwisePHDFaculty > adjustedPHDFaculty)
                                    {
                                        //if (item.Department == "AI&DS" || item.Department == "CSD" || item.Department == "AI&ML" || item.specializationId == 189)
                                        //    minimumRequirementMet = "-";
                                        //else
                                        minimumRequirementMet = (SpecializationwisePHDFaculty - adjustedPHDFaculty) + " Ph.D";
                                    }
                                    else
                                    {
                                        //if (item.Department == "AI&DS" || item.Department == "CSD" || item.Department == "AI&ML" || item.specializationId == 189)
                                        //    minimumRequirementMet = "-";
                                        //else
                                        minimumRequirementMet = (Math.Ceiling(item.requiredFaculty) - adjustedFaculty - SpecializationwisePHDFaculty) + " faculty and " + (SpecializationwisePHDFaculty - adjustedPHDFaculty) + " Ph.D";
                                    }

                                }
                                else if (minimumRequirementMet == "YES" && PhdminimumRequirementMet == "NO")
                                {
                                    //if (rFaculty <= adjustedFaculty)
                                    //    minimumRequirementMet = "-";//No Deficiency In faculty
                                    //else
                                    //    minimumRequirementMet = "Deficiency In faculty";

                                    //if (SpecializationwisePHDFaculty == adjustedPHDFaculty)
                                    //    minimumRequirementMet += "-";// and No Deficiency In Phd
                                    //else
                                    //    minimumRequirementMet += " and Deficiency In Phd";
                                    if (rFaculty <= adjustedFaculty && SpecializationwisePHDFaculty <= adjustedPHDFaculty)
                                    {
                                        minimumRequirementMet = "-";
                                    }
                                    else if (rFaculty > adjustedFaculty && SpecializationwisePHDFaculty <= adjustedPHDFaculty)
                                    {
                                        //if (item.Department == "AI&DS" || item.Department == "CSD" || item.Department == "AI&ML" || item.specializationId == 189)
                                        //    minimumRequirementMet = "-";
                                        //else
                                        minimumRequirementMet = (Math.Ceiling(item.requiredFaculty) - adjustedFaculty - SpecializationwisePHDFaculty) + " faculty";
                                    }
                                    else if (rFaculty <= adjustedFaculty &&
                                             SpecializationwisePHDFaculty > adjustedPHDFaculty)
                                    {
                                        //if (item.Department == "AI&DS" || item.Department == "CSD" || item.Department == "AI&ML" || item.specializationId == 189)
                                        //    minimumRequirementMet = "-";
                                        //else
                                        minimumRequirementMet = (SpecializationwisePHDFaculty - adjustedPHDFaculty) + " Ph.D";
                                    }
                                    else
                                    {
                                        //if (item.Department == "AI&DS" || item.Department == "CSD" || item.Department == "AI&ML" || item.specializationId == 189)
                                        //    minimumRequirementMet = "-";
                                        //else
                                        minimumRequirementMet = (Math.Ceiling(item.requiredFaculty) - adjustedFaculty - SpecializationwisePHDFaculty) + " faculty and " + (SpecializationwisePHDFaculty - adjustedPHDFaculty) + " Ph.D";
                                    }

                                }

                                else if (minimumRequirementMet == "NO" && PhdminimumRequirementMet == "NO")
                                {
                                    //if (rFaculty == adjustedFaculty)
                                    //    minimumRequirementMet = "-";//No Deficiency In faculty
                                    //else
                                    //    minimumRequirementMet = "Deficiency In faculty";


                                    //if (SpecializationwisePHDFaculty == adjustedPHDFaculty)
                                    //    minimumRequirementMet += "-";// and No Deficiency In Phd
                                    //else
                                    //    minimumRequirementMet += " and Deficiency In Phd";
                                    if (rFaculty <= adjustedFaculty && SpecializationwisePHDFaculty <= adjustedPHDFaculty)
                                    {
                                        minimumRequirementMet = "-";
                                    }
                                    else if (rFaculty > adjustedFaculty && SpecializationwisePHDFaculty <= adjustedPHDFaculty)
                                    {
                                        //if (item.Department == "AI&DS" || item.Department == "CSD" || item.Department == "AI&ML" || item.specializationId == 189)
                                        //    minimumRequirementMet = "-";
                                        //else
                                        minimumRequirementMet = (Math.Ceiling(item.requiredFaculty) - adjustedFaculty - SpecializationwisePHDFaculty) + " faculty";
                                    }
                                    else if (rFaculty <= adjustedFaculty &&
                                             SpecializationwisePHDFaculty > adjustedPHDFaculty)
                                    {
                                        //if (item.Department == "AI&DS" || item.Department == "CSD" || item.Department == "AI&ML" || item.specializationId == 189)
                                        //    minimumRequirementMet = "-";
                                        //else
                                        minimumRequirementMet = (SpecializationwisePHDFaculty - adjustedPHDFaculty) + " Ph.D";
                                    }
                                    else
                                    {
                                        //if (item.Department == "AI&DS" || item.Department == "CSD" || item.Department == "AI&ML" || item.specializationId == 189)
                                        //    minimumRequirementMet = "-";
                                        //else
                                        minimumRequirementMet = (Math.Ceiling(item.requiredFaculty) - adjustedFaculty - SpecializationwisePHDFaculty) + " faculty and " + (SpecializationwisePHDFaculty - adjustedPHDFaculty) + " Ph.D";
                                    }
                                }


                                if (item.Degree == "M.Tech")
                                {
                                    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" +
                                           SpecializationwisePHDFaculty + "</td>";
                                    //if (deptloop == 1)
                                    //    faculty += "<td rowspan='" + TotalCount + "'  class='col2' style='text-align: center; vertical-align: top;'>" + SpecializationwisePHDFaculty + "</td>";

                                    //faculty += "<td   class='col2' style='text-align: center; vertical-align: top;'>" + SpecializationwisePHDFaculty + "</td>";

                                    //else
                                    //    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + "" + "</td>";

                                    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" +
                                               adjustedPHDFaculty + "</td>";
                                    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" +
                                               minimumRequirementMet + "</td>";

                                    faculty += "</tr>";
                                }
                            //}



                            //faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" +
                            //           SpecializationwisePHDFaculty + "</td>";
                            ////if (deptloop == 1)
                            ////    faculty += "<td rowspan='" + TotalCount + "'  class='col2' style='text-align: center; vertical-align: top;'>" + SpecializationwisePHDFaculty + "</td>";

                            ////faculty += "<td   class='col2' style='text-align: center; vertical-align: top;'>" + SpecializationwisePHDFaculty + "</td>";

                            ////else
                            ////    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + "" + "</td>";

                            //faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" +
                            //           adjustedPHDFaculty + "</td>";
                            //faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" +
                            //           minimumRequirementMet + "</td>";

                            //faculty += "</tr>";

                            deptloop++;
                            SpecializationwisePHDFaculty = 0;
                            facultyIndexnotEqualtoZeroIndex++;

                            #endregion
                        }
                    }
                    faculty += "</table>";


                    #endregion
                }
            }
            if (!faculty.Contains("Deficiency In"))
            {
                faculty = string.Empty;
                faculty = "<table  border='1' cellpadding='3' cellspacing='0' width='100%' style='font-family: Times New Roman; font-size: 9px; font-weight: normal; background-color: #fff; margin: 0 auto;'><tr><th style='text-align: left; vertical-align: top;' ><b>Specialization</b></th><th style='text-align: center; vertical-align: top;' ><b>Proposed Intake</b></th><th style='text-align: center; vertical-align: top;'><b>Required faculty</b></th><th style='text-align: center; vertical-align: top;'><b>Available faculty</b></th><th style='text-align: center; vertical-align: top;'><b>Required Ph.D. faculty</b></th><th style='text-align: center; vertical-align: top;'><b>Available Ph.D. faculty</b></th><th style='text-align: center; vertical-align: top;' ><b>Deficiency</b></th></tr><tr><td colspan = '7' class='col2' style='text-align: center; vertical-align: top;'><b>NIL</b></td></tr></table>";
            }
            return faculty;
        }

        public List<CollegeFacultyWithIntakeReport> collegeFacultyNew(int? collegeId)
        {
            List<CollegeIntakeExisting> collegeIntakeExisting = new List<CollegeIntakeExisting>();
            List<CollegeFacultyWithIntakeReport> intakedetailsList = new List<CollegeFacultyWithIntakeReport>();
            var jntuh_departments = db.jntuh_department.ToList();
            string CollegeAffiliationStatus = db.jntuh_college_affiliation.Where(u => u.collegeId == collegeId && u.affiliationTypeId == 7).Select(u => u.affiliationStatus).FirstOrDefault();
            DateTime newfacultyfromdate = new DateTime(2023, 04, 04);
            int studentcount = Convert.ToInt32(ConfigurationManager.AppSettings["studentcount"]);
            if (collegeId != null)
            {
                var jntuh_college_faculty_deficiency = db.jntuh_college_faculty_deficiency.Where(fd => fd.CollegeId == collegeId).ToList();
                var jntuh_specialization = db.jntuh_specialization.ToList();

                int[] collegeIDs = null;
                int facultystudentRatio = 0;
                decimal facultyRatio = 0m;
                decimal AfacultyRatio = 0m;
                decimal SfacultyRatio = 0m;
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
                List<jntuh_college_intake_existing> intake = db.jntuh_college_intake_existing.Where(i => collegeIDs.Contains(i.collegeId) && i.isActive == true).ToList();


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
                    newIntake.courseStatus = item.courseStatus;
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
                                                         //SpecName = b.specializationName,
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
                var jntuh_phdfaculty = db.jntuh_faculty_phddetails.AsNoTracking().ToList();
                var registeredFaculty = db.jntuh_registered_faculty.Where(rf => strRegnos.Contains(rf.RegistrationNumber.Trim())).ToList();
                //principalRegno != null ? db.jntuh_registered_faculty.Where(rf => strRegnos.Contains(rf.RegistrationNumber.Trim()) && rf.RegistrationNumber != principalRegno).ToList(): db.jntuh_registered_faculty.Where(rf => strRegnos.Contains(rf.RegistrationNumber.Trim())).ToList();
                //  var reg1=registeredFaculty.Where(f => f.RegistrationNumber.Trim() == "9251-150414-062519").ToList();
                //Reg nos related online facultyIds`-- (rf.isApproved == null || rf.isApproved == true)
                var collegeaffliations = db.jntuh_college_affliationstatus.AsNoTracking().ToList();
                var regfacultywithdepts = registeredFaculty.Where(rf => rf.DepartmentId == null).ToList();
                //02-04-2018 OriginalsVerifiedPHD Columns consider as-No Guide Sign in PHD Thesis;OriginalsVerifiedUG column Consider as -Complaint PHD Faculty

                //&& rf.BAS != "Yes"  && (rf.AbsentforVerification == false || rf.AbsentforVerification == null)
                //var jntuh_registered_faculty1 =
                //    registeredFaculty.Where(rf => rf.type != "Adjunct" && ((rf.NotQualifiedAsperAICTE == false || rf.NotQualifiedAsperAICTE == null) && (rf.Noclass == false || rf.Noclass == null) && (rf.Absent == false || rf.Absent == null) && (rf.OriginalCertificatesNotShown == false || rf.OriginalCertificatesNotShown == null) &&
                //                                         (rf.NoSCM == false || rf.NoSCM == null) && rf.NoRelevantUG != "Yes" && rf.NoRelevantPG != "Yes" && rf.NORelevantPHD != "Yes" && (rf.InvalidPANNumber == false || rf.InvalidPANNumber == null) && (rf.Blacklistfaculy == false) && (rf.AbsentforVerification == false || rf.AbsentforVerification == null) && (rf.DepartmentId != 61 || rf.DepartmentId != 27))).Select(rf => new
                var jntuh_registered_faculty1 =
                    registeredFaculty.Where(rf => rf.type != "Adjunct" && (rf.BAS != "Yes") && (rf.Noclass == false || rf.Noclass == null) && (rf.Absent == false || rf.Absent == null) &&
                                                         (rf.NoSCM == false || rf.NoSCM == null) && (rf.Blacklistfaculy == false) && (rf.AbsentforVerification == false || rf.AbsentforVerification == null) && (rf.DepartmentId != 61 || rf.DepartmentId != 27)).Select(rf => new
                                                         {
                                                             FacultyId = rf.id,
                                                             //Departmentid = rf.DepartmentId,
                                                             RegistrationNumber = rf.RegistrationNumber.Trim(),
                                                             // Department = rf.jntuh_department.departmentName,
                                                             DeptId = jntuh_college_faculty_registered.Where(c => c.RegistrationNumber.Trim() == rf.RegistrationNumber.Trim()).Select(c => c.DepartmentId).FirstOrDefault(),
                                                             SpecializationId = jntuh_college_faculty_registered.Where(c => c.RegistrationNumber.Trim() == rf.RegistrationNumber.Trim()).Select(c => c.SpecializationId).FirstOrDefault(),
                                                             NotconsideredPHD = rf.NotconsideredPHD,
                                                             // HighestDegreeID = rf.jntuh_registered_faculty_education.Count() != 0 ? rf.jntuh_registered_faculty_education.Where(e => e.educationId != 8).Select(e => e.educationId).Max() : 0,
                                                             HighestDegreeID = rf.NotconsideredPHD == true ?
                                                                         rf.jntuh_registered_faculty_education.Where(r => r.educationId != 8 && r.educationId != 9).Count() != 0 ?
                                                                         rf.jntuh_registered_faculty_education.Where(e => e.educationId != 8 && e.educationId != 6 && e.educationId != 9).Select(e => e.educationId).Max() : 0
                                                                         :
                                                                         rf.jntuh_registered_faculty_education.Where(r => r.educationId != 8 && r.educationId != 9).Count() != 0 ?
                                                                         rf.jntuh_registered_faculty_education.Where(e => e.educationId != 8 && e.educationId != 9).Select(e => e.educationId).Max() : 0,
                                                             IsApproved = rf.isApproved,
                                                             Createdon = jntuh_college_faculty_registered.Where(c => c.RegistrationNumber.Trim() == rf.RegistrationNumber.Trim()).Select(c => c.createdOn).FirstOrDefault(),
                                                             PanNumber = rf.PANNumber,
                                                             AadhaarNumber = rf.AadhaarNumber,
                                                             NoForm16 = rf.NoForm16,
                                                             TotalExperience = rf.TotalExperience,
                                                             CsePhDFacultyFlag = rf.PhdDeskVerification
                                                         }).ToList();
                //BAS Flag
                int variable = 2;

                var nohighestdegree = jntuh_registered_faculty1.Where(e => e.HighestDegreeID < 4).ToList();
                jntuh_registered_faculty1 = jntuh_registered_faculty1.Where(e => e.HighestDegreeID >= 4).ToList();
                int fcount = jntuh_registered_faculty1.Count();
                var jntuh_registered_faculty = jntuh_registered_faculty1.Select(rf => new
                {
                    RegistrationNumber = rf.RegistrationNumber,
                    Department = rf.DeptId != null ? jntuh_departments.Where(e => e.id == rf.DeptId).Select(e => e.departmentName).FirstOrDefault() : DeptNameBasedOnSpecialization.Where(e => e.Specid == rf.SpecializationId).Select(e => e.DeptName).FirstOrDefault(),
                    //Specialization = rf.DeptId != null ? jntuh_specialization.Where(e => e.departmentId == rf.DeptId).Select(e => e.specializationName).FirstOrDefault() : DeptNameBasedOnSpecialization.Where(e => e.Specid == rf.SpecializationId).Select(e => e.SpecName).FirstOrDefault(),
                    NotconsideredPHD = rf.NotconsideredPHD,
                    HighestDegree = jntuh_education_category.Where(c => c.id == rf.HighestDegreeID).Select(c => c.educationCategoryName).FirstOrDefault(),
                    Recruitedfor = jntuh_college_faculty_registered.Where(c => c.RegistrationNumber.Trim() == rf.RegistrationNumber.Trim()).Select(c => c.IdentifiedFor).FirstOrDefault(),
                    rf.SpecializationId,
                    PanNumber = rf.PanNumber,
                    AadhaarNumber = rf.AadhaarNumber,
                    noform16 = rf.NoForm16,
                    Createdon = rf.Createdon,
                    TotalExperience = rf.TotalExperience,
                    Phd2pages = jntuh_phdfaculty.Where(i => i.Facultyid == rf.FacultyId).Count() > 0 ? true : false,
                    CsePhDFacultyFlag = rf.CsePhDFacultyFlag
                }).Where(e => e.Department != null).ToList();

                var jntuh_registered_facultyBAS = registeredFaculty.Where(rf => rf.BAS == "Yes").Select(rf => new
                {
                    FacultyId = rf.id,
                    //Departmentid = rf.DepartmentId,
                    RegistrationNumber = rf.RegistrationNumber.Trim(),
                    // Department = rf.jntuh_department.departmentName,
                    DeptId = jntuh_college_faculty_registered.Where(c => c.RegistrationNumber.Trim() == rf.RegistrationNumber.Trim()).Select(c => c.DepartmentId).FirstOrDefault(),
                    SpecializationId = jntuh_college_faculty_registered.Where(c => c.RegistrationNumber.Trim() == rf.RegistrationNumber.Trim()).Select(c => c.SpecializationId).FirstOrDefault(),
                    NotconsideredPHD = rf.NotconsideredPHD,
                    // HighestDegreeID = rf.jntuh_registered_faculty_education.Count() != 0 ? rf.jntuh_registered_faculty_education.Where(e => e.educationId != 8).Select(e => e.educationId).Max() : 0,
                    HighestDegreeID = rf.NotconsideredPHD == true ?
                                rf.jntuh_registered_faculty_education.Where(r => r.educationId != 8 && r.educationId != 9).Count() != 0 ?
                                rf.jntuh_registered_faculty_education.Where(e => e.educationId != 8 && e.educationId != 6 && e.educationId != 9).Select(e => e.educationId).Max() : 0
                                :
                                rf.jntuh_registered_faculty_education.Where(r => r.educationId != 8 && r.educationId != 9).Count() != 0 ?
                                rf.jntuh_registered_faculty_education.Where(e => e.educationId != 8 && e.educationId != 9).Select(e => e.educationId).Max() : 0,
                    IsApproved = rf.isApproved,
                    Createdon = jntuh_college_faculty_registered.Where(c => c.RegistrationNumber.Trim() == rf.RegistrationNumber.Trim()).Select(c => c.createdOn).FirstOrDefault(),
                    PanNumber = rf.PANNumber,
                    AadhaarNumber = rf.AadhaarNumber,
                    NoForm16 = rf.NoForm16,
                    TotalExperience = rf.TotalExperience,
                    CsePhDFacultyFlag = rf.PhdDeskVerification,
                    BASFlag = rf.BAS
                }).ToList();

                var jntuh_registered_facultyBASFlag = jntuh_registered_facultyBAS.Select(rf => new
                {
                    RegistrationNumber = rf.RegistrationNumber,
                    Department = rf.DeptId != null ? jntuh_departments.Where(e => e.id == rf.DeptId).Select(e => e.departmentName).FirstOrDefault() : DeptNameBasedOnSpecialization.Where(e => e.Specid == rf.SpecializationId).Select(e => e.DeptName).FirstOrDefault(),
                    //Specialization = rf.DeptId != null ? jntuh_specialization.Where(e => e.departmentId == rf.DeptId).Select(e => e.specializationName).FirstOrDefault() : DeptNameBasedOnSpecialization.Where(e => e.Specid == rf.SpecializationId).Select(e => e.SpecName).FirstOrDefault(),
                    NotconsideredPHD = rf.NotconsideredPHD,
                    HighestDegree = jntuh_education_category.Where(c => c.id == rf.HighestDegreeID).Select(c => c.educationCategoryName).FirstOrDefault(),
                    Recruitedfor = jntuh_college_faculty_registered.Where(c => c.RegistrationNumber.Trim() == rf.RegistrationNumber.Trim()).Select(c => c.IdentifiedFor).FirstOrDefault(),
                    rf.SpecializationId,
                    PanNumber = rf.PanNumber,
                    AadhaarNumber = rf.AadhaarNumber,
                    noform16 = rf.NoForm16,
                    Createdon = rf.Createdon,
                    TotalExperience = rf.TotalExperience,
                    Phd2pages = jntuh_phdfaculty.Where(i => i.Facultyid == rf.FacultyId).Count() > 0 ? true : false,
                    CsePhDFacultyFlag = rf.CsePhDFacultyFlag,
                    BASFlag = rf.BASFlag
                }).Where(e => e.Department != null).ToList();

                var form16Count = registeredFaculty.Where(i => i.NoForm16 == true).ToList();
                var aictecount = registeredFaculty.Where(i => i.NotQualifiedAsperAICTE == true).ToList();
                int[] StrPharmacy = new[] { 26, 27, 36, 39 };
                foreach (var item in collegeIntakeExisting.Where(D => !StrPharmacy.Contains(D.DepartmentID)).ToList())
                {
                    var intakedetails = new CollegeFacultyWithIntakeReport();
                    intakedetails.ispercentage = false;
                    intakedetails.isstarcourses = false;
                    int phdFaculty; int oldphdFaculty = 0; int newphdFaculty = 0; int pgFaculty; int oldpgFaculty = 0; int newpgFaculty = 0; int ugFaculty; int oldugFaculty = 0; int newugFaculty = 0; int SpecializationphdFaculty = 0; int SpecializationpgFaculty = 0;

                    intakedetails.collegeId = item.collegeId;
                    intakedetails.collegeCode = jntuh_college.Where(c => c.id == item.collegeId).Select(c => c.collegeCode).FirstOrDefault();
                    intakedetails.collegeName = jntuh_college.Where(c => c.id == item.collegeId).Select(c => c.collegeName).FirstOrDefault();
                    intakedetails.Degree = item.Degree;
                    intakedetails.Department = item.Department;
                    intakedetails.Specialization = item.Specialization;
                    item.courseStatus = db.jntuh_college_intake_existing.Where(
                        e =>
                            e.specializationId == item.specializationId && e.shiftId == item.shiftId &&
                            e.academicYearId == 15 &&
                            e.collegeId == item.collegeId && e.isActive == true).Select(s => s.courseStatus).FirstOrDefault();
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

                    //intakedetails.approvedIntake1 = GetIntake(item.collegeId, AY1, item.specializationId, item.shiftId, 1, item.degreeID);
                    //intakedetails.approvedIntake2 = GetIntake(item.collegeId, AY2, item.specializationId, item.shiftId, 1, item.degreeID);
                    //intakedetails.approvedIntake3 = GetIntake(item.collegeId, AY3, item.specializationId, item.shiftId, 1, item.degreeID);
                    //intakedetails.approvedIntake4 = GetIntake(item.collegeId, AY4, item.specializationId, item.shiftId, 1, item.degreeID);
                    //intakedetails.approvedIntake5 = GetIntake(item.collegeId, AY5, item.specializationId, item.shiftId, 1, item.degreeID);

                    intakedetails.AffiliationStatus2 = GetAcademicYear(item.collegeId, AY1, item.specializationId, item.shiftId, item.degreeID);
                    intakedetails.AffiliationStatus3 = GetAcademicYear(item.collegeId, AY2, item.specializationId, item.shiftId, item.degreeID);
                    intakedetails.AffiliationStatus4 = GetAcademicYear(item.collegeId, AY3, item.specializationId, item.shiftId, item.degreeID);

                    //Get sanction Inake for Btech
                    //if (item.degreeID == 4)
                    //{
                    //    intakedetails.SanctionIntake1 = GetIntake(item.collegeId, AY1, item.specializationId, item.shiftId, 0, item.degreeID);
                    //    intakedetails.SanctionIntake2 = GetIntake(item.collegeId, AY2, item.specializationId, item.shiftId, 0, item.degreeID);
                    //    intakedetails.SanctionIntake3 = GetIntake(item.collegeId, AY3, item.specializationId, item.shiftId, 0, item.degreeID);
                    //    intakedetails.SanctionIntake4 = GetIntake(item.collegeId, AY4, item.specializationId, item.shiftId, 0, item.degreeID);
                    //    intakedetails.SanctionIntake5 = GetIntake(item.collegeId, AY5, item.specializationId, item.shiftId, 0, item.degreeID);
                    //}
                    intakedetails.SanctionIntake1 = GetIntake(item.collegeId, AY1, item.specializationId, item.shiftId, 0, item.degreeID);
                    intakedetails.SanctionIntake2 = GetIntake(item.collegeId, AY2, item.specializationId, item.shiftId, 0, item.degreeID);
                    intakedetails.SanctionIntake3 = GetIntake(item.collegeId, AY3, item.specializationId, item.shiftId, 0, item.degreeID);
                    intakedetails.SanctionIntake4 = GetIntake(item.collegeId, AY4, item.specializationId, item.shiftId, 0, item.degreeID);
                    intakedetails.SanctionIntake5 = GetIntake(item.collegeId, AY5, item.specializationId, item.shiftId, 0, item.degreeID);
                    facultystudentRatio = Convert.ToInt32(jntuh_faculty_student_ratio_norms.Where(fn => fn.degreeId == item.degreeID).Select(fn => fn.Norms).FirstOrDefault().Split(':')[1].ToString());

                    var csedept = jntuh_registered_faculty.Where(i => i.Department == item.Department).ToList();
                    intakedetails.form16count = form16Count.Count > 0 ? form16Count.Count(i => i.DepartmentId == item.DepartmentID) : 0;
                    intakedetails.aictecount = aictecount.Count > 0 ? aictecount.Count(i => i.DepartmentId == item.DepartmentID) : 0;


                    if (item.Degree == "B.Tech")
                    {
                        //25% Calculation Based on Admitted Intake intakes on 418
                        //Take Higest of 3 Years Of Admitated Intake
                        //Propasedintake or admittedIntake1  means Proposed Intake of Present Year
                        //int SanctionIntakeHigest = Max(intakedetails.admittedIntake2, intakedetails.admittedIntake3, intakedetails.admittedIntake4);
                        //SanctionIntakeHigest = GetBtechAdmittedIntake(SanctionIntakeHigest);
                        //int senondyearpercentage = 0;
                        //int thirdyearpercentage = 0;
                        //int fourthyearpercentage = 0;
                        ////Comment on 26-07--2018 Due Count
                        ////Comment on 02-05-2018 Due Count
                        //if (intakedetails.SanctionIntake2 != 0)
                        //{
                        //    senondyearpercentage = (int)Math.Ceiling(Convert.ToDecimal((Convert.ToDecimal(intakedetails.admittedIntake2) / Convert.ToDecimal(intakedetails.SanctionIntake2)) * 100));
                        //}
                        //if (intakedetails.SanctionIntake3 != 0)
                        //{
                        //    thirdyearpercentage = (int)Math.Ceiling(Convert.ToDecimal((Convert.ToDecimal(intakedetails.admittedIntake3) / Convert.ToDecimal(intakedetails.SanctionIntake3)) * 100));
                        //}
                        //if (intakedetails.SanctionIntake4 != 0)
                        //{
                        //    fourthyearpercentage = (int)Math.Ceiling(Convert.ToDecimal((Convert.ToDecimal(intakedetails.admittedIntake4) / Convert.ToDecimal(intakedetails.SanctionIntake4)) * 100));
                        //}

                        //if (senondyearpercentage < 25 && thirdyearpercentage < 25 && fourthyearpercentage < 25)
                        //{
                        //    intakedetails.ispercentage = true;
                        //    //studentcount
                        //    if ((intakedetails.admittedIntake2 >= studentcount || intakedetails.admittedIntake3 >= studentcount || intakedetails.admittedIntake3 >= studentcount) && intakedetails.Proposedintake != 0)
                        //    {
                        //        intakedetails.ispercentage = false;
                        //        intakedetails.isstarcourses = true;
                        //        //intakedetails.ReducedInatke = 60;
                        //        //if (intakedetails.approvedIntake1 != 60)
                        //        //{
                        //        //    intakedetails.Note = "(Reduced from Proposed Intake of <b>";
                        //        //    intakedetails.Note += intakedetails.approvedIntake1;
                        //        //    intakedetails.Note += intakedetails.approvedIntake1;
                        //        //    intakedetails.Note += "</b> as per 25% Clause)";
                        //        //    intakedetails.approvedIntake1 = 60;
                        //        //}
                        //    }
                        //}
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
                                //intakedetails.ispercentage = true;
                                intakedetails.ispercentage = false;
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
                        #region This Is Commented on 16-04-2019 due to Calculate on AICTE/JNTU Sanctioned Intakes only
                        //if (intakedetails.ispercentage == false)
                        //{
                        //    if (item.courseStatus == "New")
                        //    {
                        //        intakedetails.approvedIntake2 = GetBtechAdmittedIntake(intakedetails.SanctionIntake2);
                        //        intakedetails.approvedIntake3 = 0;
                        //        intakedetails.approvedIntake4 = 0;
                        //    }
                        //    else if (SanctionIntakeHigest >= intakedetails.Proposedintake)
                        //    {
                        //        //New Code 
                        //        intakedetails.approvedIntake2 = GetBtechAdmittedIntake(intakedetails.admittedIntake2);
                        //        intakedetails.approvedIntake3 = GetBtechAdmittedIntake(intakedetails.admittedIntake3);
                        //        intakedetails.approvedIntake4 = GetBtechAdmittedIntake(intakedetails.admittedIntake4);


                        //    }
                        //    else
                        //    {
                        //        intakedetails.approvedIntake2 = GetBtechAdmittedIntake(intakedetails.SanctionIntake2);
                        //        intakedetails.approvedIntake3 = GetBtechAdmittedIntake(intakedetails.SanctionIntake3);
                        //        intakedetails.approvedIntake4 = GetBtechAdmittedIntake(intakedetails.SanctionIntake4);
                        //    }

                        //    intakedetails.totalIntake = (intakedetails.approvedIntake2) + (intakedetails.approvedIntake3) +
                        //                                (intakedetails.approvedIntake4);

                        //    facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) /
                        //                       Convert.ToDecimal(facultystudentRatio);

                        //    //intakedetails.totalAdmittedIntake = (intakedetails.admittedIntake2) + (intakedetails.admittedIntake3) + (intakedetails.admittedIntake4);
                        //    //AICTE Intake
                        //    intakedetails.totalAdmittedIntake = (intakedetails.AICTESanctionIntake2) + (intakedetails.AICTESanctionIntake3) + (intakedetails.AICTESanctionIntake4);
                        //    intakedetails.totalSanctionIntake = (intakedetails.SanctionIntake2) + (intakedetails.SanctionIntake3) + (intakedetails.SanctionIntake4);

                        //}
                        #endregion
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
                        //int[] StrNewSpecs = new[] { 180, 181, 182, 183, 184, 185, 186, 187, 188, 189, 190, 191, 192, 193, 194, 195, 196 };
                        int takecondition = Convert.ToInt32(ConfigurationManager.AppSettings["intakecondition"]);
                        if (takecondition == 1)
                        {
                            //if (StrNewSpecs.Contains(item.specializationId) && intakedetails.Proposedintake != 0 && intakedetails.SanctionIntake1 == 0 && intakedetails.SanctionIntake2 == 0 && intakedetails.SanctionIntake3 == 0)
                            //{
                            //    intakedetails.totalIntake = 0;
                            //}
                            //else if (item.Department == "CSE(SE)" || item.Department == "CSE(CS)" || item.Department == "CSE(AI&ML)" || item.Department == "CSE(DS)" || item.Department == "CSE(IoT)" || item.Department == "CSE(Networks)" || item.DepartmentID == 22)
                            //{
                            //    intakedetails.totalIntake = GetBtechAdmittedIntake(intakedetails.SanctionIntake2) + GetBtechAdmittedIntake(intakedetails.AICTESanctionIntake3);
                            //}
                            //else
                            //{
                            //    intakedetails.totalIntake = GetBtechAdmittedIntake(intakedetails.AICTESanctionIntake2) + GetBtechAdmittedIntake(intakedetails.AICTESanctionIntake3) +
                            //                        GetBtechAdmittedIntake(intakedetails.AICTESanctionIntake4);
                            //}
                            intakedetails.totalIntake = GetBtechAdmittedIntake(intakedetails.AICTESanctionIntake2) + GetBtechAdmittedIntake(intakedetails.AICTESanctionIntake3) +
                                                    GetBtechAdmittedIntake(intakedetails.AICTESanctionIntake4);
                        }
                        else if (takecondition == 2)
                        {
                            //if (StrNewSpecs.Contains(item.specializationId) && intakedetails.Proposedintake != 0 && intakedetails.SanctionIntake1 == 0 && intakedetails.SanctionIntake2 == 0 && intakedetails.SanctionIntake3 == 0)
                            //{
                            //    intakedetails.totalIntake = 0;
                            //}
                            //else if (item.Department == "CSE(SE)" || item.Department == "CSE(CS)" || item.Department == "CSE(AI&ML)" || item.Department == "CSE(DS)" || item.Department == "CSE(IoT)" || item.Department == "CSE(Networks)" || item.DepartmentID == 22)
                            //{
                            //    intakedetails.totalIntake = GetBtechAdmittedIntake(intakedetails.SanctionIntake2) + GetBtechAdmittedIntake(intakedetails.AICTESanctionIntake3);
                            //}
                            //else
                            //{
                            //    intakedetails.totalIntake = GetBtechAdmittedIntake(intakedetails.SanctionIntake2) +
                            //                           GetBtechAdmittedIntake(intakedetails.SanctionIntake3) +
                            //                           GetBtechAdmittedIntake(intakedetails.SanctionIntake4);
                            //}
                            intakedetails.totalIntake = GetBtechAdmittedIntake(intakedetails.SanctionIntake2) +
                                                       GetBtechAdmittedIntake(intakedetails.SanctionIntake3) +
                                                       GetBtechAdmittedIntake(intakedetails.SanctionIntake4);
                        }
                        else
                        {
                            //if (StrNewSpecs.Contains(item.specializationId) && intakedetails.Proposedintake != 0 && intakedetails.SanctionIntake1 == 0 && intakedetails.SanctionIntake2 == 0 && intakedetails.SanctionIntake3 == 0)
                            //{
                            //    intakedetails.totalIntake = 0;
                            //}
                            //else if (item.Department == "CSE(SE)" || item.Department == "CSE(CS)" || item.Department == "CSE(AI&ML)" || item.Department == "CSE(DS)" || item.Department == "CSE(IoT)" || item.Department == "CSE(Networks)" || item.DepartmentID == 22)
                            //{
                            //    intakedetails.totalIntake = GetBtechAdmittedIntake(intakedetails.SanctionIntake2) + GetBtechAdmittedIntake(intakedetails.AICTESanctionIntake3);
                            //}
                            //else
                            //{
                            //    intakedetails.totalIntake = GetBtechAdmittedIntake(intakedetails.admittedIntake2) +
                            //                           GetBtechAdmittedIntake(intakedetails.admittedIntake3) +
                            //                           GetBtechAdmittedIntake(intakedetails.admittedIntake4);
                            //}
                            intakedetails.totalIntake = GetBtechAdmittedIntake(intakedetails.admittedIntake2) +
                                                       GetBtechAdmittedIntake(intakedetails.admittedIntake3) +
                                                       GetBtechAdmittedIntake(intakedetails.admittedIntake4);
                        }
                        facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) /
                                           Convert.ToDecimal(facultystudentRatio);

                        intake2 = GetBtechAdmittedIntake(intakedetails.SanctionIntake2);
                        intake3 = GetBtechAdmittedIntake(intakedetails.SanctionIntake3);
                        intake4 = GetBtechAdmittedIntake(intakedetails.SanctionIntake4);
                        totalintake = (intake2) + (intake3) +
                                                        (intake4);

                        SfacultyRatio = Convert.ToDecimal(totalintake) /
                                           Convert.ToDecimal(facultystudentRatio);

                        //else
                        //{
                        //    if (SanctionIntakeHigest >= intakedetails.approvedIntake1)
                        //    {
                        //        //New Code 
                        //        intakedetails.admittedIntake2 = GetBtechAdmittedIntake(intakedetails.approvedIntake2);
                        //        intakedetails.admittedIntake3 = GetBtechAdmittedIntake(intakedetails.approvedIntake3);
                        //        intakedetails.admittedIntake4 = GetBtechAdmittedIntake(intakedetails.approvedIntake4);


                        //    }
                        //    else
                        //    {
                        //        intakedetails.admittedIntake2 = GetBtechAdmittedIntake(intakedetails.SanctionIntake2);
                        //        intakedetails.admittedIntake3 = GetBtechAdmittedIntake(intakedetails.SanctionIntake3);
                        //        intakedetails.admittedIntake4 = GetBtechAdmittedIntake(intakedetails.SanctionIntake4);
                        //    }

                        //    intakedetails.totalIntake = (intakedetails.admittedIntake2) + (intakedetails.admittedIntake3) +
                        //                                (intakedetails.admittedIntake4);
                        //    facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) /
                        //                   Convert.ToDecimal(facultystudentRatio);


                        //    intakedetails.totalAdmittedIntake = (intakedetails.SanctionIntake2) + (intakedetails.SanctionIntake3) + (intakedetails.SanctionIntake4);
                        //}

                        //intakedetails.A416TotalFaculty = registeredFaculty.Where(i => i.DepartmentId == item.DepartmentID && (i.jntuh_department.jntuh_degree.degree == "B.Tech" || i.jntuh_department.jntuh_degree.degree == "B.Pharmacy")).ToList().Count;
                    }
                    else if (item.Degree == "M.Tech" || item.Degree == "MBA")
                    {
                        //intakedetails.totalIntake = (intakedetails.approvedIntake1) + (intakedetails.approvedIntake2);
                        //facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) /
                        //               Convert.ToDecimal(facultystudentRatio);
                        //New Code Written by Narayana
                        if (item.Degree == "M.Tech" && item.shiftId == 1)
                        {
                            //intakedetails.totalIntake = (intakedetails.admittedIntake1) + (intakedetails.admittedIntake2);
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
                            //intakedetails.totalIntake = (intakedetails.admittedIntake1) + (intakedetails.admittedIntake2);
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
                            //intakedetails.totalIntake = (intakedetails.admittedIntake1) + (intakedetails.admittedIntake2);
                            intakedetails.totalIntake = (intakedetails.Proposedintake) + (intakedetails.AICTESanctionIntake2);
                            facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) /
                                           Convert.ToDecimal(facultystudentRatio);
                        }
                    }
                    else if (item.Degree == "MCA")
                    {
                        //intakedetails.totalIntake = (intakedetails.admittedIntake1) + (intakedetails.admittedIntake2) +(intakedetails.admittedIntake3);

                        intakedetails.totalIntake = (intakedetails.Proposedintake) + (intakedetails.AICTESanctionIntake2);//+ (intakedetails.AICTESanctionIntake3)

                        facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) /
                                       Convert.ToDecimal(facultystudentRatio);
                    }
                    else if (item.Degree == "B.Pharmacy")
                    {
                        intakedetails.totalIntake = (intakedetails.admittedIntake1) + (intakedetails.admittedIntake2) +
                                                    (intakedetails.admittedIntake3) + (intakedetails.admittedIntake4);
                        facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) / Convert.ToDecimal(facultystudentRatio);
                    }
                    else if (item.Degree == "M.Pharmacy")
                    {
                        intakedetails.totalIntake = (intakedetails.admittedIntake1) + (intakedetails.admittedIntake2);
                        facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) / 12;

                    }
                    else if (item.Degree == "Pharm.D")
                    {
                        intakedetails.totalIntake = (intakedetails.admittedIntake1) + (intakedetails.admittedIntake2) +
                                                    (intakedetails.admittedIntake3) + (intakedetails.admittedIntake4) +
                                                    (intakedetails.admittedIntake5);
                        facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) / 12;
                    }
                    else if (item.Degree == "Pharm.D PB")
                    {
                        intakedetails.totalIntake = (intakedetails.admittedIntake1) + (intakedetails.admittedIntake2);
                        facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) / 10;
                    }
                    else //MAM MTM
                    {
                        //intakedetails.totalIntake = (intakedetails.admittedIntake1) + (intakedetails.admittedIntake2) +
                        //                            (intakedetails.admittedIntake3) + (intakedetails.admittedIntake4) +
                        //                            (intakedetails.admittedIntake5);
                        intakedetails.totalIntake = (intakedetails.Proposedintake) + (intakedetails.AICTESanctionIntake2) +
                                                    (intakedetails.AICTESanctionIntake3) + (intakedetails.AICTESanctionIntake4) +
                                                    (intakedetails.AICTESanctionIntake5);
                        facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) / Convert.ToDecimal(facultystudentRatio);
                    }

                    intakedetails.requiredFaculty = Math.Round(facultyRatio, 2);
                    //intakedetails.ArequiredFaculty = Math.Round(AfacultyRatio, 2);
                    intakedetails.SrequiredFaculty = Math.Round(SfacultyRatio, 2);
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

                            //var regno = jntuh_registered_faculty.Where(f => f.Department == item.Department).Select(f => f.RegistrationNumber);//&& (f.Recruitedfor == "UG" || f.Recruitedfor == null)
                            //intakedetails.specializationWiseFaculty = jntuh_registered_faculty.Count(f => f.Department == item.Department);//&& (f.Recruitedfor == "UG" || f.Recruitedfor == null)
                            intakedetails.specializationWiseFaculty = jntuh_registered_faculty.Count(f => f.Department == item.Department && f.CsePhDFacultyFlag != true);
                            //var testing = jntuh_registered_faculty.Where(f => f.Department == item.Department).ToList();
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
                                        f.SpecializationId == item.specializationId && (f.Recruitedfor == "PG" || f.Recruitedfor == "UG&PG"));//f.Specialization == item.Specialization
                        }
                    }
                    if (strdegreetype == "Dual Degree")
                    {
                        intakedetails.specializationWiseFaculty = jntuh_registered_faculty.Count(f => f.SpecializationId == item.specializationId);//(f.Recruitedfor == "PG" || f.Recruitedfor == "UG&PG") &&
                    }
                    int noPanOrAadhaarcount = 0;

                    if (item.Degree == "B.Pharmacy")
                    {
                        ugFaculty = jntuh_registered_faculty.Count(f => "UG" == f.HighestDegree && f.SpecializationId == item.specializationId);
                        pgFaculty = jntuh_registered_faculty.Count(f => "PG" == f.HighestDegree && f.SpecializationId == item.specializationId);
                        phdFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && f.CsePhDFacultyFlag != true && f.SpecializationId == item.specializationId);
                        //intakedetails.Department = "Pharmacy";
                    }
                    if (item.Degree == "M.Pharmacy")
                    {
                        ugFaculty = jntuh_registered_faculty.Count(f => "UG" == f.HighestDegree && f.Department == "Pharmacy" && f.SpecializationId == item.specializationId);
                        pgFaculty = jntuh_registered_faculty.Count(f => ("PG" == f.HighestDegree || "M.Phil / Other PG Degree" == f.HighestDegree) &&
                                    f.Department == "Pharmacy" && f.SpecializationId == item.specializationId);
                        phdFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && f.CsePhDFacultyFlag != true && f.Department == "Pharmacy" && f.SpecializationId == item.specializationId);
                        //noPanOrAadhaarcount =registeredFaculty.Where(f =>f.DepartmentId == pharmacyDeptId && f.PANNumber == null && f.AadhaarNumber == null &&
                        //            (f.isApproved == null || f.isApproved == true)).Count();
                        //intakedetails.Department = "Pharmacy";
                    }
                    else if (item.Degree == "Pharm.D")
                    {
                        ugFaculty = jntuh_registered_faculty.Count(f => "UG" == f.HighestDegree && f.Department == "Pharm.D" && f.SpecializationId == item.specializationId);
                        pgFaculty = jntuh_registered_faculty.Count(f => ("PG" == f.HighestDegree || "M.Phil / Other PG Degree" == f.HighestDegree) && f.Department == "Pharm.D" && f.SpecializationId == item.specializationId);
                        phdFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && f.CsePhDFacultyFlag != true && f.Department == "Pharm.D" && f.SpecializationId == item.specializationId);
                        //noPanOrAadhaarcount = registeredFaculty.Where(f => f.DepartmentId == pharmacyDeptId && f.PANNumber == null && f.AadhaarNumber == null && (f.isApproved == null || f.isApproved == true)).Count();
                        intakedetails.Department = "Pharm.D";
                    }
                    else if (item.Degree == "Pharm.D PB")
                    {
                        ugFaculty = jntuh_registered_faculty.Count(f => "UG" == f.HighestDegree && f.Department == "Pharm.D PB" && f.SpecializationId == item.specializationId);
                        pgFaculty = jntuh_registered_faculty.Count(f => ("PG" == f.HighestDegree || "M.Phil / Other PG Degree" == f.HighestDegree) && f.Department == "Pharm.D PB" && f.SpecializationId == item.specializationId);
                        phdFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && f.CsePhDFacultyFlag != true && f.Department == "Pharm.D PB" && f.SpecializationId == item.specializationId);
                        //noPanOrAadhaarcount = registeredFaculty.Where(f => f.DepartmentId == pharmacyDeptId && f.PANNumber == null && f.AadhaarNumber == null && (f.isApproved == null || f.isApproved == true)).Count();
                        intakedetails.Department = "Pharm.D PB";
                    }
                    else
                    {
                        ugFaculty = jntuh_registered_faculty.Count(f => "UG" == f.HighestDegree && f.Department == item.Department);
                        pgFaculty = jntuh_registered_faculty.Count(f => ("PG" == f.HighestDegree || "M.Phil / Other PG Degree" == f.HighestDegree) && f.Department == item.Department);
                        phdFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && f.CsePhDFacultyFlag != true && f.Department == item.Department);

                        //Written by Narayana Reddy on 02-03-2019.
                        oldugFaculty = jntuh_registered_faculty.Count(f => "UG" == f.HighestDegree && f.Department == item.Department && f.Createdon <= newfacultyfromdate);
                        newugFaculty = jntuh_registered_faculty.Count(f => "UG" == f.HighestDegree && f.Department == item.Department && f.Createdon >= newfacultyfromdate);
                        oldpgFaculty = jntuh_registered_faculty.Count(f => ("PG" == f.HighestDegree || "M.Phil / Other PG Degree" == f.HighestDegree) && f.Department == item.Department && f.Createdon <= newfacultyfromdate);
                        newpgFaculty = jntuh_registered_faculty.Count(f => ("PG" == f.HighestDegree || "M.Phil / Other PG Degree" == f.HighestDegree) && f.Department == item.Department && f.Createdon >= newfacultyfromdate);
                        oldphdFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && f.CsePhDFacultyFlag != true && f.Department == item.Department && f.Createdon <= newfacultyfromdate);
                        newphdFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && f.CsePhDFacultyFlag != true && f.Department == item.Department && f.Createdon >= newfacultyfromdate);

                        var PhdReg = jntuh_registered_faculty.Where(f => ("Ph.D" == f.HighestDegree) && f.CsePhDFacultyFlag != true && f.Department == item.Department).Select(f => f.RegistrationNumber).ToList();

                        //var phdFaculty1 = jntuh_registered_faculty.Where(f => ("Ph.D" == f.HighestDegree || "Ph.D." == f.HighestDegree || "Phd" == f.HighestDegree || "PHD" == f.HighestDegree || "Ph D" == f.HighestDegree)).ToList() ;
                        //if (item.Department == "MBA")
                        //    phdFaculty1 = phdFaculty1.Where(f => f.Department == "MBA").ToList();

                        if (item.Degree == "B.Tech")
                            SpecializationphdFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && f.CsePhDFacultyFlag != true && (f.SpecializationId == item.specializationId || f.SpecializationId == null) && f.Department == item.Department);
                        else
                            SpecializationphdFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && f.CsePhDFacultyFlag != true && f.SpecializationId == item.specializationId);
                        var reg = jntuh_registered_faculty.Where(f => f.SpecializationId == item.specializationId && (f.Recruitedfor == "PG" || f.Recruitedfor == "UG&PG")).Select(S => S.RegistrationNumber).ToList();
                        SpecializationpgFaculty = jntuh_registered_faculty.Count(f => f.SpecializationId == item.specializationId && (f.Recruitedfor == "PG" || f.Recruitedfor == "UG&PG"));
                    }

                    intakedetails.phdFaculty = phdFaculty;
                    intakedetails.SpecializationsphdFaculty = SpecializationphdFaculty;
                    intakedetails.SpecializationspgFaculty = SpecializationpgFaculty;
                    intakedetails.pgFaculty = pgFaculty;
                    intakedetails.ugFaculty = ugFaculty;
                    intakedetails.totalFaculty = (ugFaculty + pgFaculty + phdFaculty);
                    intakedetails.DeptWiseBASFlag = jntuh_registered_facultyBAS.Count(f => "Yes" == f.BASFlag && f.DeptId == item.DepartmentID);
                    intakedetails.oldtotalFaculty = (oldugFaculty + oldpgFaculty + oldphdFaculty);
                    intakedetails.newtotalFaculty = (newugFaculty + newpgFaculty + newphdFaculty);
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
                        //oldugFaculty = jntuh_registered_faculty.Count(f => "UG" == f.HighestDegree && f.Department == item.Department && f.Createdon <= newfacultyfromdate);
                        //newugFaculty = jntuh_registered_faculty.Count(f => "UG" == f.HighestDegree && f.Department == item.Department && f.Createdon >= newfacultyfromdate);

                        var deptid = jntuh_departments.Where(i => i.departmentName == department).Select(i => i.id).FirstOrDefault();
                        var deptname = jntuh_departments.Where(i => i.departmentName == department).Select(i => i.departmentName).FirstOrDefault();
                        int speId = jntuh_specialization.Where(s => s.jntuh_department.departmentName == department).Select(s => s.id).FirstOrDefault();
                        int ugFaculty = jntuh_registered_faculty.Count(f => "UG" == f.HighestDegree && f.Department == department);
                        int oldugFaculty = jntuh_registered_faculty.Count(f => "UG" == f.HighestDegree && f.Department == department && f.Createdon <= newfacultyfromdate);
                        int newugFaculty = jntuh_registered_faculty.Count(f => "UG" == f.HighestDegree && f.Department == department && f.Createdon >= newfacultyfromdate);

                        int pgFaculty = jntuh_registered_faculty.Count(f => ("PG" == f.HighestDegree || "M.Phil / Other PG Degree" == f.HighestDegree) && f.Department == department);
                        int oldpgFaculty = jntuh_registered_faculty.Count(f => ("PG" == f.HighestDegree || "M.Phil / Other PG Degree" == f.HighestDegree) && f.Department == department && f.Createdon <= newfacultyfromdate);
                        int newpgFaculty = jntuh_registered_faculty.Count(f => ("PG" == f.HighestDegree || "M.Phil / Other PG Degree" == f.HighestDegree) && f.Department == department && f.Createdon >= newfacultyfromdate);
                        var pgreg = jntuh_registered_faculty.Where(f => ("PG" == f.Recruitedfor || "M.Phil / Other PG Degree" == f.HighestDegree) && f.Department == department).Select(e => e.RegistrationNumber).ToList();
                        //var testcount = jntuh_registered_faculty.Where(f => f.Department == department).ToList();
                        int phdFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && f.CsePhDFacultyFlag != true && f.Department == department);
                        int oldphdFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && f.CsePhDFacultyFlag != true && f.Department == department && f.Createdon <= newfacultyfromdate);
                        int newphdFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && f.CsePhDFacultyFlag != true && f.Department == department && f.Createdon >= newfacultyfromdate);
                        int DeptWiseBASCounts = jntuh_registered_facultyBAS.Count(f => "Yes" == f.BASFlag && f.DeptId == deptid);
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
                            DeptWiseBASFlag = DeptWiseBASCounts,
                            oldtotalFaculty = oldugFaculty + oldpgFaculty + oldphdFaculty,
                            newtotalFaculty = newugFaculty + newpgFaculty + newphdFaculty,
                            specializationId = speId,
                            shiftId = 1,
                            form16count = form16Count.Count > 0 ? form16Count.Count(i => i.DepartmentId == deptid) : 0,
                            aictecount = aictecount.Count > 0 ? aictecount.Count(i => i.DepartmentId == deptid) : 0,
                            A416TotalFaculty = registeredFaculty.Where(i => i.DepartmentId == deptid).ToList().Count,
                            specializationWiseFaculty = jntuh_registered_faculty.Count(f => f.Department == deptname),
                            //approvedIntake1 = 1
                            Proposedintake = 1
                        });
                    }
                }
            }
            return intakedetailsList;
        }

        [Authorize(Roles = "Admin,SuperAdmin")]
        public string NewDeficienciesInFacultyMBA(int? collegeID)
        {
            string facultyclosure = string.Empty;
            string twintyfivepercentbelowcurces = string.Empty;
            string faculty = string.Empty;
            string facultyAdmittedIntakeZero = string.Empty;
            string facultyAdmittedIntakeZeroTable2 = string.Empty;
            int facultycount = 0;
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int prAy =
                db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
            int[] mbacollegeides = { 5, 67, 119, 174, 246, 296, 343, 355, 386, 394, 411, 413, 421, 424, 430, 449, 452 };
            int cid = Convert.ToInt32(collegeID);
            int[] shiftids = { 1, 2 };
            List<CollegeFacultyWithIntakeReport> facultyCountslist = new List<CollegeFacultyWithIntakeReport>();
            List<CollegeFacultyWithIntakeReport> facultylist = collegeFacultyNew(collegeID).Select(e => e).ToList();
            List<CollegeFacultyWithIntakeReport> facultyCounts = facultylist.Where(c => c.shiftId == 1 || c.shiftId == 2).Select(e => e).ToList();//Where(c => c.shiftId == 1)
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
            List<CollegeFacultyWithIntakeReport> facultyCountper = facultylist.Where(c => (c.ispercentage == true && c.Proposedintake != 0 && c.Degree == "B.Tech") || c.Proposedintake == 0 && c.Degree == "B.Tech").Select(e => e).ToList();
            foreach (var itemmtech in facultyCountper)
            {
                if (itemmtech.collegeId == 72 && itemmtech.Department == "IT")
                {
                }
                else if (itemmtech.collegeId == 130 && itemmtech.Department == "IT")
                {

                }
                else
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
            }
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
                var CollegeSubmissionDate = Convert.ToDateTime(db.jntuh_college_edit_status.Where(e => e.collegeId == collegeID && e.academicyearId == prAy && e.IsCollegeEditable == false).Select(e => e.updatedOn).FirstOrDefault()).Date;
                // DateTime data = Convert.ToDateTime(CollegeSubmissionDate).Date;
                if (collegeStatus.AICTEStatus == true)
                {

                    var RegisteredFaculty = (from Reg in db.jntuh_registered_faculty
                                             join Clg in db.jntuh_college_faculty_registered on Reg.RegistrationNumber equals Clg.RegistrationNumber
                                             join ClgEdit in db.jntuh_college_edit_status on Clg.collegeId equals ClgEdit.collegeId
                                             where ClgEdit.IsCollegeEditable == false && ClgEdit.academicyearId == prAy && Clg.collegeId == collegeID
                                             select Reg).ToList();

                    int Absebtfaculty = RegisteredFaculty.Count(e => e.Absent == true);
                    int TotalFaculty = RegisteredFaculty.Count();

                    var Percentage = Math.Ceiling((((double)Absebtfaculty / (double)TotalFaculty) * 100));

                    faculty += "<p>Sub: - Affiliation for the Academic Year: 2020-21- Reg.</p>";
                    faculty += "<br/>";
                    faculty += "<table><tr><td style='vertical-align:top'>Ref:</td><td>";
                    faculty += "<ol type='1'>";
                    faculty += "<li>Your college online application dated:" + CollegeSubmissionDate + " for grant of Affiliation for the Academic Year 2017-18.</li>";
                    faculty += "<li>Your college FFC Inspection on  dated: ----------- </li>";
                    faculty += "</ol></td></tr></table>";
                    faculty += "<br/>";
                    faculty += "<p align='center'><b>****</b></p>";

                    faculty += "<p style='text-align:justfy'>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;As per the fact finding Committee inspection report it is noticed that " + Percentage + " of faculty members were absent against the number of faculty shown in online application for affiliation A117. It indicates that the College is not maintaining the minimum essential requirements for running the academic programs. Under these circumstances the SCA recommends <b><u>'No Admission Status'</u></b> for the A.Y. 2017-18 for the Institute. Clarification or explanation if any may be submitted within 10 days from the date of this letter</p>";

                    faculty += "<table width='100%'  cellspacing='0'>";
                    faculty += "<tr><td align='right' width='80%' style='font-size: 14px; font-weight: normal;'> <b>Sd /-</b></td></tr>";
                    faculty += "<tr><td align='right' width='80%' style='font-size: 14px; font-weight: normal;'> <b>REGISTRAR</b></td></tr>";
                    faculty += "</table>";


                }
                else if (collegeStatus.CollegeStatus == true)
                {
                    faculty += "<p>Sub: - Affiliation for the Academic Year: 2020-21- Reg.</p>";
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
            int remainingSpecializationFaculty = 0;
            int remainingPHDFaculty = 0;
            int remainingSpecializationwisePHDFaculty = 0;
            int SpecializationwisePHDFaculty = 0;
            int SpecializationwisePGFaculty = 0;
            int TotalCount = 0;
            int HumantitiesminimamRequireMet = 0;
            string HumantitiesminimamRequireMetStatus = "Yes";



            string[] ProposedIntakeZeroDeptName = facultyCounts.Where(e => e.Degree == "B.Tech" && (e.Proposedintake == 0) && !departments.Contains(e.Department)).Select(e => e.Specialization).Distinct().ToArray();
            var ProposedIntakeZeroDeptNameSpecIDs = facultyCounts.Where(e => e.Degree == "B.Tech" && (e.Proposedintake == 0) && !departments.Contains(e.Department)).Select(e => e.specializationId).Distinct().ToArray();
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
                    faculty += "<table  border='1' cellpadding='3' cellspacing='0' width='100%' style='font-family: Times New Roman; font-size: 9px; font-weight: normal; background-color: #fff; margin: 0 auto;'>";
                    faculty += "<tr>";
                    //faculty += "<th rowspan='2' style='text-align: center; vertical-align: top;'>SNo</th>";
                    //faculty += "<th rowspan='2' style='text-align: left; vertical-align: top;' >Dept</th>";
                    //faculty += "<th  rowspan='2' style='text-align: left; vertical-align: top;' >Degree</th>";
                    faculty += "<th style='text-align: left; vertical-align: top;' ><b>Specialization</b></th>";
                    //Written by Narayana
                    //faculty += "<th rowspan='2' style='text-align: left; vertical-align: top;' >Shift</th>";
                    //if (mbacollegeides.Contains(cid))
                    //{
                    //    faculty += "<th colspan='3' rowspan='2' style='text-align: center; vertical-align: top;' >Sum of Proposed &<br/> Previous Sanctioned Intake</th>";
                    //}
                    //else
                    //{
                    //    faculty += "<th colspan='3' style='text-align: center; vertical-align: top;' >Intake of Previous Three A.Y</th>";
                    //}
                    //faculty += "<th colspan='3' style='text-align: center; vertical-align: top;' >Intake of Previous Three A.Y</th>";
                    //faculty += "<th rowspan='2' style='text-align: center; vertical-align: top;' >Admitted Intake</th>";

                    faculty += "<th style='text-align: center; vertical-align: top;' ><b>Proposed Intake</b></th>";
                    //faculty += "<th rowspan='2' style='text-align: center; vertical-align: top;' >Total Department Faculty</th>";
                    //faculty += "<th colspan='2' style='text-align: center; vertical-align: top;' ><b>Specialization Wise Faculty</b></th>";
                    //faculty += "<th rowspan='2' style='text-align: center; vertical-align: top;' >Specialization Wise Faculty Available</th>";
                    //faculty += "<th rowspan='2' style='text-align: center; vertical-align: top;' >PG Specialization Faculty</th>";
                    //faculty += "<th colspan='2' style='text-align: center; vertical-align: top;' ><b>Faculty with Ph.D</b></th>";
                    //faculty += "<th rowspan='2' style='text-align: center; vertical-align: top;' >Available Ph.D faculty</th>";
                    faculty += "<th style='text-align: center; vertical-align: top;'><b>Required faculty</b></th>";
                    faculty += "<th style='text-align: center; vertical-align: top;'><b>Available faculty</b></th>";
                    faculty += "<th style='text-align: center; vertical-align: top;'><b>Required Ph.D. faculty</b></th>";
                    faculty += "<th style='text-align: center; vertical-align: top;'><b>Available Ph.D. faculty</b></th>";
                    faculty += "<th style='text-align: center; vertical-align: top;' ><b>Deficiency</b></th>";
                    faculty += "</tr>";
                    //if (mbacollegeides.Contains(cid))
                    //{
                    //    faculty += "<tr>";
                    //    //faculty += "<th colspan='3' style='text-align: center; vertical-align: top;'></th>";
                    //    //faculty += "<th style='text-align: center; vertical-align: top;'></th>";

                    //    //faculty += "<th style='text-align: center; vertical-align: top;'></th>";
                    //    faculty += "<th style='text-align: center; vertical-align: top;'>Required</th>";
                    //    faculty += "<th style='text-align: center; vertical-align: top;'>Available</th>";
                    //    faculty += "<th style='text-align: center; vertical-align: top;'>Required</th>";
                    //    faculty += "<th style='text-align: center; vertical-align: top;'>Available</th>";
                    //    faculty += "</tr>";
                    //}
                    //else
                    //{
                    //    faculty += "<tr>";
                    //    faculty += "<th style='text-align: center; vertical-align: top;'>JNTUH <br/>Sanctioned</th>";
                    //    faculty += "<th style='text-align: center; vertical-align: top;'>Admitted</th>";

                    //    faculty += "<th style='text-align: center; vertical-align: top;'>#Divisions</th>";
                    //    faculty += "<th style='text-align: center; vertical-align: top;'>Required</th>";
                    //    faculty += "<th style='text-align: center; vertical-align: top;'>Available</th>";
                    //    faculty += "<th style='text-align: center; vertical-align: top;'>Required</th>";
                    //    faculty += "<th style='text-align: center; vertical-align: top;'>Available</th>";
                    //    faculty += "</tr>";
                    //}

                    //faculty += "<tr>";
                    ////faculty += "<th style='text-align: center; vertical-align: top;'>AICTE <br/>Sanctioned</th>";
                    ////faculty += "<th style='text-align: center; vertical-align: top;'>JNTUH <br/>Sanctioned</th>";

                    ////faculty += "<th style='text-align: center; vertical-align: top;'>#Divisions</th>";
                    //faculty += "<th style='text-align: center; vertical-align: top;'>Required faculty</th>";
                    //faculty += "<th style='text-align: center; vertical-align: top;'>Available available</th>";
                    //faculty += "<th style='text-align: center; vertical-align: top;'>Required Ph.D. faculty</th>";
                    //faculty += "<th style='text-align: center; vertical-align: top;'>Available Ph.D. faculty</th>";
                    //faculty += "</tr>";
                    //facultyAdmittedIntakeZero += "<p><b>In so for as the remaining programs/courses applied for affiliation the University has issued a separate show cause notice Dt.___________ based on the discrepancy in faculty uploaded by the College to AICTE and University and zero admissions for more than one previous A.Y. in those programs/courses. </b></p>";
                    //facultyAdmittedIntakeZero += "<p><b>The admission status in certain programs/courses in the previous three academic years in which there were zero admissions for two or more than two academic years either due to non grant of affiliation or unable to make admissions at the Institute is shown in the following table. It is also observed that there is a mismatch of faculty information that the Institute has submitted to AICTE and the University. The EOA obtained from AICTE is based on the faculty data uploaded by the Institute </b></p>";
                    //facultyAdmittedIntakeZero += "<table width='100%' border='1' cellpadding='3' cellspacing='0' style='border-color: #ccc;'>";
                    //facultyAdmittedIntakeZero += "<tr>";
                    //facultyAdmittedIntakeZero += "<th style='text-align: center; vertical-align: top;'>SNo</th>";
                    //facultyAdmittedIntakeZero += "<th style='text-align: left; vertical-align: top;' >Department</th>";
                    //facultyAdmittedIntakeZero += "<th style='text-align: left; vertical-align: top;' >Degree</th>";
                    //facultyAdmittedIntakeZero += "<th style='text-align: left; vertical-align: top;' >Specialization</th>";
                    //facultyAdmittedIntakeZero += "<th style='text-align: center; vertical-align: top;' >Sanction Intake</th>";
                    //facultyAdmittedIntakeZero += "<th style='text-align: center; vertical-align: top;' >Admitted Intake</th>";
                    //facultyAdmittedIntakeZero += "<th style='text-align: center; vertical-align: top;' >Proposed Intake</th>";
                    //facultyAdmittedIntakeZero += "<th style='text-align: center; vertical-align: top;' >Affiliation Status 2016-17</th>";
                    //facultyAdmittedIntakeZero += "<th style='text-align: center; vertical-align: top;' >Affiliation Status 2015-16</th>";
                    //facultyAdmittedIntakeZero += "<th style='text-align: center; vertical-align: top;' >Affiliation Status 2014-15</th>";
                    //facultyAdmittedIntakeZero += "</tr>";

                    //facultyAdmittedIntakeZeroTable2 += "<table width='100%' border='1' cellpadding='3' cellspacing='0' style='border-color: #ccc;'>";
                    //facultyAdmittedIntakeZeroTable2 += "<tr>";
                    //facultyAdmittedIntakeZeroTable2 += "<th style='text-align: center; vertical-align: top;'>SNo</th>";
                    //facultyAdmittedIntakeZeroTable2 += "<th style='text-align: left; vertical-align: top;' >Department</th>";
                    //facultyAdmittedIntakeZeroTable2 += "<th style='text-align: left; vertical-align: top;' >Degree</th>";
                    //facultyAdmittedIntakeZeroTable2 += "<th style='text-align: left; vertical-align: top;' >Specialization</th>";
                    //facultyAdmittedIntakeZeroTable2 += "</tr>";

                    string[] admittedIntakeTwoormoreZeroDeptName1 = facultyCounts.Where(e => e.Degree == "B.Tech" && ((e.admittedIntake2 == 0 && e.admittedIntake3 == 0) || (e.admittedIntake3 == 0 && e.admittedIntake4 == 0) || (e.admittedIntake2 == 0 && e.admittedIntake4 == 0)) && !departments.Contains(e.Department)).Select(e => e.Department).Distinct().ToArray();

                    #region Intake not equal to Zero

                    // Getting M.Tech Civil Specialization ID's
                    int[] SpecializationIDS =
                        db.jntuh_specialization.Where(S => S.departmentId == 43).Select(S => S.id).ToArray();
                    int remainingFaculty2 = 0;
                    int facultyIndexnotEqualtoZeroIndex = 1;
                    int facultyIndexnotEqualtoZeroAdmittedIntakeIndex = 1;
                    int btechcount = facultyCounts.Where(
                        e =>
                            e.Proposedintake != 0 && e.ispercentage == false && e.Degree == "B.Tech" &&
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


                    var pgcourses = facultyCounts.Where(
                                e => e.Proposedintake != 0 && e.ispercentage == false && (e.Degree == "MBA" || e.Degree == "MCA") &&
                                    !ProposedIntakeZeroDeptName.Contains(e.Specialization)).Select(e => e).ToList();
                    if (pgcourses.Count == 0)
                    {
                        faculty = string.Empty;
                        faculty += "<table border='1' cellpadding='3' cellspacing='0' width='100%' style='font-family: Times New Roman; font-size: 9px; font-weight: normal; background-color: #fff; margin: 0 auto;'>";
                        faculty += "<tr><td align='center'> <b>NIL</b></td></tr>";
                    }
                    else
                    {
                        foreach (
                            var item in
                                facultyCounts.Where(
                                    e =>
                                        e.Proposedintake != 0 && e.ispercentage == false &&
                                        !ProposedIntakeZeroDeptName.Contains(e.Specialization)).Select(e => e).ToList())
                        {
                            #region Code without calculation admitted Intake

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
                                //SpecializationwisePHDFaculty = IntakeWisePhdForBtech(item.Proposedintake);
                                SpecializationwisePHDFaculty = IntakeWisePhdForBtech(item.SanctionIntake2); //SanctionIntake2 - last year approved intake
                            }
                            TotalCount =
                                facultyCounts.Where(
                                    D =>
                                        D.Department == item.Department &&
                                        (D.Degree == "M.Tech" || D.Degree == "B.Tech") && D.Proposedintake != 0)
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
                            if ((item.collegeId == 72 && item.Department == "IT") || (item.collegeId == 130 && item.Department == "IT"))
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
                            //tFaculty =50;
                            //item.totalFaculty
                            int rFaculty = (int)Math.Ceiling(item.requiredFaculty);

                            if (departments.Contains(item.Department))
                            {
                                if (OthersSpecIds.Contains(item.specializationId))
                                {
                                    double rid = (double)(firstYearRequired / 2);
                                    rFaculty = (int)(Math.Ceiling(rid));

                                    //othersRequiredfaculty = rFaculty;
                                    //Commented By Narayana on 12-03-2018
                                    //rFaculty = 1;
                                    // othersRequiredfaculty = 1;

                                    //rFaculty = ((int)firstYearRequired)/2;
                                    //if (rFaculty <= 2)
                                    //{
                                    //    rFaculty = 2;
                                    //    othersRequiredfaculty = 2;
                                    //}
                                    //else
                                    //{
                                    //    othersRequiredfaculty = rFaculty;
                                    //}

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
                            //if (minimumRequirementMet != "YES" || PhdminimumRequirementMet != "YES")
                            //{
                                faculty += "<tr>";
                                //faculty +=
                                //    "<td class='col2' style='text-align: center; vertical-align: top;width:10px' rowspan='1'>" +
                                //    facultyIndexnotEqualtoZeroIndex + "</td>";
                                ////(facultyCounts.Where(e=>e.approvedIntake1!=0).Select(e=>e).ToList().IndexOf(item) + 1)
                                //if (item.isstarcourses == true)
                                //{
                                //    faculty += "<td class='col2' style='text-align: left; vertical-align: top;'>" +
                                //               item.Department + "#" + "</td>";
                                //}
                                //else
                                //{
                                //    faculty += "<td class='col2' style='text-align: left; vertical-align: top;'>" +
                                //               item.Department + "</td>";
                                //}
                                //faculty += "<td class='col2' style='text-align: left; vertical-align: top;'>" + item.Degree +
                                //           "</td>";
                                ////faculty += "<td class='col2' style='text-align: left; vertical-align: top;'>" + item.Specialization + "</td>";
                                if (item.Degree == "MBA" || item.Degree == "MCA")
                                {
                                    faculty += "<td class='col2' style='text-align: left; vertical-align: top;'>" +
                                               item.Specialization + "</td>";
                                }
                                //else
                                //{
                                //    faculty += "<td class='col2' style='text-align: left; vertical-align: top;'>" +
                                //               item.Specialization + " -2" + "</td>";
                                //}
                                //Written by Narayana
                                //faculty += "<td class='col2' style='text-align: left; vertical-align: top;'>" + item.shiftId + "</td>";
                                if (departments.Contains(item.Department))
                                {
                                    // faculty += "<td class='col2' style='text-align: center; vertical-align: top;'> </td>";
                                    //faculty += "<td class='col2' style='text-align: center; vertical-align: top;'> </td>";
                                    ////faculty += "<td class='col2' style='text-align: center; vertical-align: top;'> </td>";
                                    //faculty +=
                                    //    "<td colspan='2' class='col2' style='text-align: center; vertical-align: top;'> </td>";
                                    //faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" +
                                    //           totalBtechFirstYearIntake + "</td>";


                                    //if (deptloop == 1)
                                    //    faculty += "<td rowspan='" + TotalCount +
                                    //               "'  class='col2' style='text-align: center; vertical-align: top;'>" +
                                    //               item.totalFaculty + "</td>";
                                    ////faculty += "<td class='col2' style='text-align: left; vertical-align: top;'>" + item.A416TotalFaculty + "</td>";
                                    //if (OthersSpecIds.Contains(item.specializationId))
                                    //{
                                    //    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" +
                                    //               Math.Ceiling(firstYearRequired / 2) + "</td>";
                                    //    HumantitiesminimamRequireMet += item.totalFaculty;
                                    //}
                                    //else
                                    //{
                                    //    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" +
                                    //               Math.Ceiling(firstYearRequired) + "</td>";
                                    //    HumantitiesminimamRequireMet += item.totalFaculty;
                                    //}

                                    ////
                                    //if (minimumRequirementMet != "YES")
                                    //{
                                    //    HumantitiesminimamRequireMetStatus = "NO";
                                    //}
                                }
                                else
                                {
                                    //faculty += "<td class='col2' style='text-align: center; vertical-align: top;font-weight: bold'>" + item.AffliationStatus + "</td>";
                                    if (item.Degree == "B.Techh")
                                    {
                                        //faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + item.totalAdmittedIntake + "</td>";

                                        //faculty +=
                                        //"<td class='col2' style='text-align: center; vertical-align: top;'><table width='100%' border='1' cellpadding='3' cellspacing='0' style='border-color: #ccc;'><tr><td><span>" +
                                        //item.AICTESanctionIntake2 + "</span></td></tr><tr><td><span>" + item.AICTESanctionIntake3 +
                                        //"</span></td></tr><tr><td><span>" + item.AICTESanctionIntake4 +
                                        //"</span></td></tr></table></td>";
                                        //faculty +=
                                        //    "<td class='col2' style='text-align: center; vertical-align: top;'><table width='100%' border='1' cellpadding='3' cellspacing='0' style='border-color: #ccc;'><tr><td><span>" +
                                        //    item.SanctionIntake2 + "</span></td></tr><tr><td><span>" + item.SanctionIntake3 +
                                        //    "</span></td></tr><tr><td><span>" + item.SanctionIntake4 +
                                        //    "</span></td></tr></table></td>";
                                        ////faculty +=
                                        ////    "<td class='col2' style='text-align: center; vertical-align: top;'><table width='100%' border='1' cellpadding='3' cellspacing='0' style='border-color: #ccc;'><tr><td><span>" +
                                        ////    item.admittedIntake2 + "</span></td></tr><tr><td><span>" + item.admittedIntake3 +
                                        ////    "</span></td></tr><tr><td><span>" + item.admittedIntake4 +
                                        ////    "</span></td></tr></table></td>";

                                        //faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>";
                                        //faculty +=
                                        //    "<table width='100%' border='1' cellpadding='3' cellspacing='0' style='border-color: #ccc;'>";
                                        ////Written New on 03-02-2018
                                        ////int SanctionIntakeHigest = Max(item.admittedIntake2, item.admittedIntake3,
                                        ////    item.admittedIntake4);
                                        ////SanctionIntakeHigest = GetBtechAdmittedIntake(SanctionIntakeHigest);
                                        ////if (SanctionIntakeHigest >= item.Proposedintake)
                                        ////{
                                        ////    faculty += "<tr><td><span>" + GetSectionBasedonIntake(item.admittedIntake2) +
                                        ////               "(A)" + "</span></td>";
                                        ////    faculty += "</tr><tr><td><span>" + GetSectionBasedonIntake(item.admittedIntake3) +
                                        ////               "(A)" + "</span></td></tr>";
                                        ////    faculty += "<tr><td><span>" + GetSectionBasedonIntake(item.admittedIntake4) +
                                        ////               "(A)" + "</span></td></tr>";

                                        ////}
                                        ////else
                                        ////{
                                        //int[] StrNewSpecs = new[] { 180, 181, 182, 183, 184, 185, 186, 187, 188, 189, 190, 191, 192, 193, 194, 195, 196 };

                                        //if (StrNewSpecs.Contains(item.specializationId) && item.Proposedintake != 0 && item.SanctionIntake1 == 0 && item.SanctionIntake2 == 0 && item.SanctionIntake3 == 0)
                                        //{
                                        //    faculty += "<tr><td><span>0</span></td>";
                                        //    faculty += "</tr><tr><td><span>0</span></td></tr>";
                                        //    faculty += "<tr><td><span>0</span></td></tr>";
                                        //}
                                        //else
                                        //{
                                        //    faculty += "<tr><td><span>" + GetSectionBasedonIntake(item.SanctionIntake2) +
                                        //               "</span></td>";
                                        //    faculty += "</tr><tr><td><span>" + GetSectionBasedonIntake(item.SanctionIntake3) +
                                        //               "</span></td></tr>";
                                        //    faculty += "<tr><td><span>" + GetSectionBasedonIntake(item.SanctionIntake4) +
                                        //               "</span></td></tr>";
                                        //}

                                        ////}
                                        //faculty += "</table></td>";
                                    }
                                    else
                                    {
                                        //if (mbacollegeides.Contains(cid))
                                        //{

                                        //    faculty +=
                                        //        "<td colspan='3' class='col2' style='text-align: center; vertical-align: top;'>" +
                                        //        item.totalIntake + " </td>";
                                        //}
                                        //else
                                        //{
                                        //faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" +
                                        //       item.AICTESanctionIntake2 + "</td>";
                                        //faculty +=
                                        //    "<td colspan='2' class='col2' style='text-align: center; vertical-align: top;'>" +
                                        //    item.SanctionIntake2 + " </td>";
                                        //}

                                    }
                                    //faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + item.totalIntake + "</td>";
                                    //if (String.IsNullOrEmpty(item.Note))
                                    //{
                                    //    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" +
                                    //               item.Proposedintake + "</td>";
                                    //}
                                    //else
                                    //{
                                    //    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" +
                                    //               item.Proposedintake + " " + item.Note + "</td>";
                                    //}
                                    //if (deptloop == 1)
                                    //    faculty += "<td rowspan='" + TotalCount +
                                    //               "'  class='col2' style='text-align: center; vertical-align: top;'>" +
                                    //               item.totalFaculty + "</td>";
                                    //faculty += "<td class='col2' style='text-align: left; vertical-align: top;'>" + item.A416TotalFaculty + "</td>";
                                    //if (item.Degree == "B.Techh")
                                    //{
                                    //    //decimal AF = Math.Ceiling(item.ArequiredFaculty);
                                    //    //decimal SF = Math.Ceiling(item.SrequiredFaculty);
                                    //    //string requriedfaculty = Math.Ceiling(item.requiredFaculty) + "/" + Math.Ceiling(item.SrequiredFaculty);
                                    //    //faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" +
                                    //    //       item.requiredFaculty + "</td>";
                                    //}
                                    if (item.Degree == "MBA" || item.Degree == "MCA")
                                    {
                                        faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" +
                                                          item.Proposedintake + "</td>";
                                        faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" +
                                                Math.Ceiling(item.requiredFaculty) + "</td>";
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

                                if (item.Degree == "MBA" || item.Degree == "MCA")
                                {
                                    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" +
                                               adjustedFaculty + "</td>";
                                }
                                //    if (SpecializationIDS.Contains(item.specializationId))
                                //        faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + SpecializationwisePGFaculty + "</td>";
                                //else
                                //        faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + "" + "</td>";

                                //if (item.Degree == "MBA")
                                //    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" +
                                //               item.SpecializationspgFaculty + "</td>";
                                //else
                                //    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + "" +
                                //               "</td>";

                                if (minimumRequirementMet == "YES" && PhdminimumRequirementMet == "YES")
                                {
                                    //if (rFaculty <= adjustedFaculty)
                                    //    minimumRequirementMet = "-";//No Deficiency In faculty
                                    //else
                                    //    minimumRequirementMet = "Deficiency In faculty";

                                    //if (SpecializationwisePHDFaculty <= adjustedPHDFaculty)
                                    //    minimumRequirementMet += "-";//and No Deficiency In Phd
                                    //else
                                    //    minimumRequirementMet += " and Deficiency In Phd";

                                    if (rFaculty <= adjustedFaculty && SpecializationwisePHDFaculty <= adjustedPHDFaculty)
                                    {
                                        minimumRequirementMet = "-";
                                    }
                                    else if (rFaculty > adjustedFaculty && SpecializationwisePHDFaculty <= adjustedPHDFaculty)
                                    {
                                        //if (item.Department == "AI&DS" || item.Department == "CSD" || item.Department == "AI&ML" || item.specializationId == 189)
                                        //    minimumRequirementMet = "-";
                                        //else
                                        minimumRequirementMet = "Deficiency In faculty";
                                    }
                                    else if (rFaculty <= adjustedFaculty &&
                                             SpecializationwisePHDFaculty > adjustedPHDFaculty)
                                    {
                                        //if (item.Department == "AI&DS" || item.Department == "CSD" || item.Department == "AI&ML" || item.specializationId == 189)
                                        //    minimumRequirementMet = "-";
                                        //else
                                        minimumRequirementMet = "Deficiency In Ph.D";
                                    }
                                    else
                                    {
                                        //if (item.Department == "AI&DS" || item.Department == "CSD" || item.Department == "AI&ML" || item.specializationId == 189)
                                        //    minimumRequirementMet = "-";
                                        //else
                                        minimumRequirementMet = "Deficiency In faculty and Ph.D";
                                    }


                                }
                                else if (minimumRequirementMet == "NO" && PhdminimumRequirementMet == "YES")
                                {
                                    //if (rFaculty == adjustedFaculty)
                                    //    minimumRequirementMet = "-";//No Deficiency In faculty
                                    //else
                                    //    minimumRequirementMet = "Deficiency In faculty";


                                    //if (SpecializationwisePHDFaculty <= adjustedPHDFaculty)
                                    //    minimumRequirementMet += "-";//and No Deficiency In Phd
                                    //else
                                    //    minimumRequirementMet += " and Deficiency In Phd";

                                    if (rFaculty <= adjustedFaculty && SpecializationwisePHDFaculty <= adjustedPHDFaculty)
                                    {
                                        minimumRequirementMet = "-";
                                    }
                                    else if (rFaculty > adjustedFaculty && SpecializationwisePHDFaculty <= adjustedPHDFaculty)
                                    {
                                        //if (item.Department == "AI&DS" || item.Department == "CSD" || item.Department == "AI&ML" || item.specializationId == 189)
                                        //    minimumRequirementMet = "-";
                                        //else
                                        minimumRequirementMet = "Deficiency In faculty";
                                    }
                                    else if (rFaculty <= adjustedFaculty &&
                                             SpecializationwisePHDFaculty > adjustedPHDFaculty)
                                    {
                                        //if (item.Department == "AI&DS" || item.Department == "CSD" || item.Department == "AI&ML" || item.specializationId == 189)
                                        //    minimumRequirementMet = "-";
                                        //else
                                        minimumRequirementMet = "Deficiency In Ph.D";
                                    }
                                    else
                                    {
                                        //if (item.Department == "AI&DS" || item.Department == "CSD" || item.Department == "AI&ML" || item.specializationId == 189)
                                        //    minimumRequirementMet = "-";
                                        //else
                                        minimumRequirementMet = "Deficiency In faculty and Ph.D";
                                    }

                                }
                                else if (minimumRequirementMet == "YES" && PhdminimumRequirementMet == "NO")
                                {
                                    //if (rFaculty <= adjustedFaculty)
                                    //    minimumRequirementMet = "-";//No Deficiency In faculty
                                    //else
                                    //    minimumRequirementMet = "Deficiency In faculty";

                                    //if (SpecializationwisePHDFaculty == adjustedPHDFaculty)
                                    //    minimumRequirementMet += "-";// and No Deficiency In Phd
                                    //else
                                    //    minimumRequirementMet += " and Deficiency In Phd";
                                    if (rFaculty <= adjustedFaculty && SpecializationwisePHDFaculty <= adjustedPHDFaculty)
                                    {
                                        minimumRequirementMet = "-";
                                    }
                                    else if (rFaculty > adjustedFaculty && SpecializationwisePHDFaculty <= adjustedPHDFaculty)
                                    {
                                        //if (item.Department == "AI&DS" || item.Department == "CSD" || item.Department == "AI&ML" || item.specializationId == 189)
                                        //    minimumRequirementMet = "-";
                                        //else
                                        minimumRequirementMet = "Deficiency In faculty";
                                    }
                                    else if (rFaculty <= adjustedFaculty &&
                                             SpecializationwisePHDFaculty > adjustedPHDFaculty)
                                    {
                                        //if (item.Department == "AI&DS" || item.Department == "CSD" || item.Department == "AI&ML" || item.specializationId == 189)
                                        //    minimumRequirementMet = "-";
                                        //else
                                        minimumRequirementMet = "Deficiency In Ph.D";
                                    }
                                    else
                                    {
                                        //if (item.Department == "AI&DS" || item.Department == "CSD" || item.Department == "AI&ML" || item.specializationId == 189)
                                        //    minimumRequirementMet = "-";
                                        //else
                                        minimumRequirementMet = "Deficiency In faculty and Ph.D";
                                    }

                                }

                                else if (minimumRequirementMet == "NO" && PhdminimumRequirementMet == "NO")
                                {
                                    //if (rFaculty == adjustedFaculty)
                                    //    minimumRequirementMet = "-";//No Deficiency In faculty
                                    //else
                                    //    minimumRequirementMet = "Deficiency In faculty";


                                    //if (SpecializationwisePHDFaculty == adjustedPHDFaculty)
                                    //    minimumRequirementMet += "-";// and No Deficiency In Phd
                                    //else
                                    //    minimumRequirementMet += " and Deficiency In Phd";
                                    if (rFaculty <= adjustedFaculty && SpecializationwisePHDFaculty <= adjustedPHDFaculty)
                                    {
                                        minimumRequirementMet = "-";
                                    }
                                    else if (rFaculty > adjustedFaculty && SpecializationwisePHDFaculty <= adjustedPHDFaculty)
                                    {
                                        //if (item.Department == "AI&DS" || item.Department == "CSD" || item.Department == "AI&ML" || item.specializationId == 189)
                                        //    minimumRequirementMet = "-";
                                        //else
                                        minimumRequirementMet = "Deficiency In faculty";
                                    }
                                    else if (rFaculty <= adjustedFaculty &&
                                             SpecializationwisePHDFaculty > adjustedPHDFaculty)
                                    {
                                        //if (item.Department == "AI&DS" || item.Department == "CSD" || item.Department == "AI&ML" || item.specializationId == 189)
                                        //    minimumRequirementMet = "-";
                                        //else
                                        minimumRequirementMet = "Deficiency In Ph.D";
                                    }
                                    else
                                    {
                                        //if (item.Department == "AI&DS" || item.Department == "CSD" || item.Department == "AI&ML" || item.specializationId == 189)
                                        //    minimumRequirementMet = "-";
                                        //else
                                        minimumRequirementMet = "Deficiency In faculty and Ph.D";
                                    }
                                }


                                if (item.Degree == "MBA" || item.Degree == "MCA")
                                {
                                    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" +
                                           SpecializationwisePHDFaculty + "</td>";
                                    //if (deptloop == 1)
                                    //    faculty += "<td rowspan='" + TotalCount + "'  class='col2' style='text-align: center; vertical-align: top;'>" + SpecializationwisePHDFaculty + "</td>";

                                    //faculty += "<td   class='col2' style='text-align: center; vertical-align: top;'>" + SpecializationwisePHDFaculty + "</td>";

                                    //else
                                    //    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + "" + "</td>";

                                    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" +
                                               adjustedPHDFaculty + "</td>";
                                    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" +
                                               minimumRequirementMet + "</td>";

                                    faculty += "</tr>";
                                }
                            //}


                            //faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" +
                            //           SpecializationwisePHDFaculty + "</td>";
                            ////if (deptloop == 1)
                            ////    faculty += "<td rowspan='" + TotalCount + "'  class='col2' style='text-align: center; vertical-align: top;'>" + SpecializationwisePHDFaculty + "</td>";

                            ////faculty += "<td   class='col2' style='text-align: center; vertical-align: top;'>" + SpecializationwisePHDFaculty + "</td>";

                            ////else
                            ////    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + "" + "</td>";

                            //faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" +
                            //           adjustedPHDFaculty + "</td>";
                            //faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" +
                            //           minimumRequirementMet + "</td>";

                            //faculty += "</tr>";

                            deptloop++;
                            SpecializationwisePHDFaculty = 0;
                            facultyIndexnotEqualtoZeroIndex++;

                            #endregion

                        }
                    }
                    faculty += "</table>";


                    #endregion
                }
            }
            if (!faculty.Contains("Deficiency In"))
            {
                faculty = string.Empty;
                faculty = "<table  border='1' cellpadding='3' cellspacing='0' width='100%' style='font-family: Times New Roman; font-size: 9px; font-weight: normal; background-color: #fff; margin: 0 auto;'><tr><th style='text-align: left; vertical-align: top;' ><b>Specialization</b></th><th style='text-align: center; vertical-align: top;' ><b>Proposed Intake</b></th><th style='text-align: center; vertical-align: top;'><b>Required faculty</b></th><th style='text-align: center; vertical-align: top;'><b>Available faculty</b></th><th style='text-align: center; vertical-align: top;'><b>Required Ph.D. faculty</b></th><th style='text-align: center; vertical-align: top;'><b>Available Ph.D. faculty</b></th><th style='text-align: center; vertical-align: top;' ><b>Deficiency</b></th></tr><tr><td colspan = '7' class='col2' style='text-align: center; vertical-align: top;'><b>NIL</b></td></tr></table>";
            }
            return faculty;
        }

        public string PendingDues(int? collegeID)
        {
            var jntuh_academic_year = db.jntuh_academic_year.ToList();
            int presentYear = jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();

            int AY0 = jntuh_academic_year.Where(a => a.actualYear == (presentYear + 1)).Select(a => a.id).FirstOrDefault();
            var duesstr = string.Empty;
            #region Pending Dues
            //var AffiliationFee = db.jntuh_college_paymentoffee.Where(e => e.collegeId == collegeID && e.FeeTypeID == 5).Select(e => e).FirstOrDefault();
            //var CommanserviceFee = db.jntuh_college_paymentoffee.Where(e => e.collegeId == collegeID && e.FeeTypeID == 3).Select(e => e).FirstOrDefault();
            var AffiliationFee2021 = db.jntuh_college_paymentoffee.Where(e => e.collegeId == collegeID && e.FeeTypeID == 5 && (e.academicyearId == (AY0 - 2))).Select(e => e).FirstOrDefault();
            var CommanserviceFee2021 = db.jntuh_college_paymentoffee.Where(e => e.collegeId == collegeID && e.FeeTypeID == 3 && (e.academicyearId == (AY0 - 2))).Select(e => e).FirstOrDefault();
            //var AffiliationFee1920 = db.jntuh_college_paymentoffee.Where(e => e.collegeId == collegeID && e.FeeTypeID == 5 && (e.academicyearId == (AY0 - 2))).Select(e => e).FirstOrDefault();
            //var CommanserviceFee1920 = db.jntuh_college_paymentoffee.Where(e => e.collegeId == collegeID && e.FeeTypeID == 3 && (e.academicyearId == (AY0 - 2))).Select(e => e).FirstOrDefault();
            var AffiliationFeeDues2022_23 = db.jntuh_college_paymentoffee.Where(e => e.collegeId == collegeID && e.FeeTypeID == 5 && e.academicyearId == (AY0 - 1)).Select(e => e.duesAmount).FirstOrDefault();
            duesstr += "<table border='1' cellpadding='3' cellspacing='0' width='100%' style='font-family: Times New Roman; font-size: 9px; font-weight: normal; background-color: #fff; margin: 0 auto;'>";
            duesstr += "<tr>";
            //duesstr += "<th style='text-align: left; vertical-align: top;'>Affiliation Fee 2019-20</th>";
            //duesstr += "<th style='text-align: left; vertical-align: top;' >Common Service Fee 2019-20</th>";
            duesstr += "<th style='text-align: left; vertical-align: top;'><b>Affiliation Fee 2022-23</b></th>";
            duesstr += "<th style='text-align: left; vertical-align: top;' ><b>Affiliation Fee Upto 2021-22</b></th>";
            duesstr += "<th style='text-align: left; vertical-align: top;' ><b>Common Service Fee Upto 2021-22</b></th>";
            duesstr += "</tr>";
            duesstr += "<tr>";
            duesstr += "<td class='col2' style='text-align: left; vertical-align: top;'>" + AffiliationFeeDues2022_23 + "</td>";
            if (AffiliationFee2021 != null)
            {
                duesstr += "<td class='col2' style='text-align: left; vertical-align: top;'>" +
                AffiliationFee2021.duesAmount + "</td>";
            }
            else
            {
                duesstr += "<td class='col2' style='text-align: left; vertical-align: top;'>" +
                "0.00" + "</td>";
            }
            if (CommanserviceFee2021 != null)
            {
                duesstr += "<td class='col2' style='text-align: left; vertical-align: top;'>" +
                CommanserviceFee2021.duesAmount.ToString() + "</td>";
            }
            else
            {
                duesstr += "<td class='col2' style='text-align: left; vertical-align: top;'>" +
                "0.00" + "</td>";
            }
            duesstr += "</tr>";
            duesstr += "</table><br/>";

            #endregion

            return duesstr;
        }

        public string DeficiencyCollegeLabsAnnexure(int? collegeID)
        {
            string annexure = string.Empty;

            var collegeStatus = db.jntuh_collegestatus.Where(e => e.CollegeId == collegeID).Select(e => e).FirstOrDefault();
            if (collegeStatus != null)
            {
                int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
                int prAy =
                    db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
                List<FacultyVerificationController.AnonymousLabclass> collegeLabAnonymousLabclass = new List<FacultyVerificationController.AnonymousLabclass>();
                //Commented by Narayana
                //List<int> specializationIds =db.jntuh_college_intake_existing.Where(e => e.collegeId == collegeID && e.courseStatus != "Closure" && e.academicYearId == 9 && e.proposedIntake!=0).Select(e => e.specializationId).Distinct().ToList();
                //int[] degreeids = { 1, 3, 6, 2, 5, 9, 10 };
                int[] degreeids = { 1, 3, 6 };
                List<int> specializationIds = db.jntuh_college_intake_existing.Where(e => e.collegeId == collegeID && e.courseStatus != "Closure" && e.academicYearId == prAy && e.proposedIntake != 0 && e.isActive == true).Select(e => e.specializationId).Distinct().ToList();
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
                    collegeLabAnonymousLabclass = db.jntuh_lab_master.AsNoTracking().Where(l => l.CollegeId == collegeID && specializationIds.Contains(l.SpecializationID) && !Equipmentsids.Contains(l.id) && degreeids.Contains(l.DegreeID) && !l.EquipmentName.Contains("Desirable") && !l.EquipmentName.Contains("optional")).Select(l => new FacultyVerificationController.AnonymousLabclass
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
                                        l.CollegeId == null && degreeids.Contains(l.DegreeID) && !l.EquipmentName.Contains("Desirable") && !l.EquipmentName.Contains("optional"))
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
                        collegeLabAnonymousLabclass = db.jntuh_lab_master.AsNoTracking().Where(l => specializationIds.Contains(l.SpecializationID) && !Equipmentsids.Contains(l.id) && l.CollegeId == null && degreeids.Contains(l.DegreeID) && l.Labcode != "PH105BS" && !l.EquipmentName.Contains("Desirable") && !l.EquipmentName.Contains("optional")).Select(l => new FacultyVerificationController.AnonymousLabclass
                        {
                            //  id = db.jntuh_appeal_college_laboratories.Where(l1 => l1.EquipmentID == l.id && l1.EquipmentNo == 1 && l1.CollegeID == collegeID).Select(l1 => l1.id).FirstOrDefault(),
                            EquipmentID = l.id,
                            LabName = l.LabName.Trim(),
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

                var list1 = collegeLabAnonymousLabclass.OrderBy(c => c.LabName).ThenBy(c => c.EquipmentName).Distinct().ToList();

                #region this code written by suresh

                int?[] labequipmentIds = list1.Select(i => i.EquipmentID).ToArray();

                int[] clgequipmentIDs = db.jntuh_college_laboratories.Where(l => l.CollegeID == collegeID && labequipmentIds.Contains(l.EquipmentID)).Select(i => i.EquipmentID).ToArray();

                list1 = list1.Where(l => !clgequipmentIDs.Contains((int)l.EquipmentID)).ToList();


                #endregion

                int[] noDefcollegeIds = { 22, 23, 75, 85, 123, 144, 152, 176, 178, 203, 225, 250, 256, 324, 365, 391, 41, 244 };

                int[] SpecializationIDs;
                if (DegreeIds.Contains(4))
                    SpecializationIDs = (from a in collegeLabAnonymousLabclass orderby a.Department select a.specializationId).Distinct().ToArray();
                ////labs.Select(l => l.specializationId).Distinct().ToArray();
                else
                    SpecializationIDs = (from a in collegeLabAnonymousLabclass where a.specializationId != 39 orderby a.Department select a.specializationId).Distinct().ToArray();

                if (list1.Count() > 0 && !noDefcollegeIds.Contains((int)collegeID))
                {
                    var specializations = db.jntuh_specialization.Where(it => SpecializationIDs.Contains(it.id)).Select(s => new
                    {
                        s.id,
                        specialization = s.specializationName,
                        department = s.jntuh_department.departmentName,
                        degree = s.jntuh_department.jntuh_degree.degree,
                        deptId = s.jntuh_department.id,

                    }).OrderBy(e => e.deptId).ToList();

                    foreach (var speclializationId in SpecializationIDs)
                    {
                        string LabNmae = "", EquipmentName = "", DepartmentName = "";
                        var specializationDetails = specializations.FirstOrDefault(s => s.id == speclializationId);
                        DepartmentName = list1.Where(l => l.specializationId == speclializationId).Select(l => l.Department).FirstOrDefault();
                        annexure += "<table border='1' cellpadding='3' cellspacing='0' width='100%' style='font-family: Times New Roman; font-size: 9px; font-weight: normal; background-color: #fff; margin: 0 auto;'>";
                        annexure += "<tr>";
                        annexure += "<th align='left' colspan='2'><b> " + specializationDetails.degree + " -" + specializationDetails.department + "-" + specializationDetails.specialization + "</b></th>";
                        annexure += "</tr>";
                        annexure += "<tr>";
                        //annexure += "<th align='left'>S.No</th><th align='left'>Lab Name</th><th align='left'>Equipment Name</th>";
                        annexure += "<th style='text-align: left; width: 10%;'><b>Lab Name</b></th>";
                        annexure += "<th style='text-align: left; width: 10%;'><b>Equipment Name</b></th>";
                        annexure += "</tr>";
                        int LabsCount = 0;
                        int EquipmentsCount = 0;

                        var labs = list1.Where(l => l.specializationId == speclializationId).OrderBy(e => e.year).ThenBy(e => e.Semester).ToList();
                        var labsgroup = list1.Where(l => l.specializationId == speclializationId).GroupBy(e => e.LabName).ToList();
                        int indexnow = 1;
                        foreach (var lb in labsgroup)
                        {
                            var selctdLab = labs.Where(i => i.LabName == lb.Key.Trim()).ToList();
                            var labslst = labs.Where(i => i.LabName == lb.Key.Trim()).Select(i => i.EquipmentName).ToArray();
                            var EqpName = string.Join("<br>", labslst);
                            var labname = lb.Key.Trim() != null ? selctdLab.FirstOrDefault().year + "-" + selctdLab.FirstOrDefault().Semester + "-" + selctdLab.FirstOrDefault().LabName : null;
                            annexure += "<tr>";
                            //annexure += "<td style='text-align: left; width: 1%;'>" + indexnow + "</td>";
                            annexure += "<td style=''>" + labname + "</td>";
                            annexure += "<td style=''>" + EqpName + "</td>";
                            //annexure += "<td align='left'>" + indexnow + "</td><td  align='left'>" + labname + "</td><td  align='left'>" + EqpName + "</td>";
                            annexure += "</tr>";
                            if (!string.IsNullOrEmpty(lb.Key))
                                LabsCount = 0;
                            if (!string.IsNullOrEmpty(selctdLab.FirstOrDefault().EquipmentName))
                                EquipmentsCount = 0;
                            indexnow++;
                        }
                        //foreach (var item in labs.ToList())
                        //{

                        //    LabNmae = item.LabName.Trim() != null ? item.year + "-" + item.Semester + "-" + item.LabName : null;
                        //    EquipmentName = item.EquipmentName;
                        //    annexure += "<tr>";
                        //    annexure += "<td align='left'>" + indexnow + "</td><td  align='left'>" + LabNmae + "</td><td  align='left'>" + EquipmentName + "</td>";
                        //    annexure += "</tr>";

                        //    if (!string.IsNullOrEmpty(item.LabName))
                        //        LabsCount = 0;
                        //    if (!string.IsNullOrEmpty(item.EquipmentName))
                        //        EquipmentsCount = 0;
                        //    indexnow++;
                        //}

                        annexure += "</table>";
                        annexure += "<br/>";
                    }
                }
                else
                {
                    annexure += "<table border='1' cellpadding='3' cellspacing='0' width='100%' style='font-family: Times New Roman; font-size: 9px; font-weight: normal; background-color: #fff; margin: 0 auto;'>";
                    annexure += "<tr><td align='center'> <b>NIL</b></td></tr>";
                    annexure += "</table>";
                }
            }

            return annexure;
        }

        public List<Lab> collegeLabs(int? collegeID)
        {
            List<Lab> lstlaboratories = new List<Lab>();
            List<Lab> collegeLabMaster = new List<Lab>();
            var jntuh_college_laboratories_deficiency = db.jntuh_college_laboratories_deficiency.Where(c => c.CollegeId == collegeID).ToList();

            string strcollegecode = db.jntuh_college_randamcodes.Where(r => r.CollegeId == collegeID).Select(r => r.RandamCode).FirstOrDefault();
            int[] specializationIds = db.jntuh_college_intake_existing.Where(e => e.collegeId == collegeID && e.proposedIntake != 0 && e.courseStatus != "Closure" && e.isActive == true).Select(e => e.specializationId).Distinct().ToArray();

            string CollegeAffiliationStatus = db.jntuh_college_affiliation.Where(u => u.collegeId == collegeID && u.affiliationTypeId == 7).Select(u => u.affiliationStatus).FirstOrDefault();


            if (CollegeAffiliationStatus == "Yes")
            {
                collegeLabMaster = db.jntuh_lab_master.AsNoTracking()
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
            }
            else if (CollegeAffiliationStatus == "No" || CollegeAffiliationStatus == null)
            {
                collegeLabMaster = db.jntuh_lab_master.AsNoTracking()
                                                           .Where(l => specializationIds.Contains(l.SpecializationID) && l.Labcode != "TMP-CL")
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
            }



            var jntuh_college_laboratories = db.jntuh_college_laboratories_dataentry2.AsNoTracking().Where(l => l.CollegeID == collegeID).ToList();

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
                    lstlabs.deficiency = jntuh_college_laboratories_deficiency.Where(ld => ld.LabCode == item.Labcode && ld.Year == item.year && ld.Semister == item.Semester && ld.CollegeId == collegeID).Select(ld => ld.Deficiency).FirstOrDefault();
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

        public string CollegeComplaints(int? collegeID)
        {
            string Complaints = string.Empty;
            var presentyear = DateTime.Now.Year - 1;
            List<jntuh_college_complaints> Collegecomplaints =
                db.jntuh_college_complaints.Where(c => c.college_faculty_Id == collegeID && c.roleId == 4 && c.complaintOn != "Closed" && c.complaintDate.Value.Year >= presentyear).OrderByDescending(o => o.complaintDate).ToList();
            //Complaints += "<table border='1' cellpadding='3' cellspacing='0' width='100%' style='font-family: Times New Roman; font-size: 9px; font-weight: normal; background-color: #fff; margin: 0 auto;'>";
            //Complaints += "<tr>";
            //Complaints += "<td align='left'><b><u>College Complaints</u></b></td>";
            //Complaints += "</tr>";
            //Complaints += "</table>";
            Complaints += "<table border='1' cellpadding='3' cellspacing='0' width='100%' style='font-family: Times New Roman; font-size: 9px; font-weight: normal; background-color: #fff; margin: 0 auto;'>";

            Complaints += "<tr>";
            Complaints +=
                "<th align='left'><b>Complaint</b></th><th align='left'><b>Complaint Date</b></th><th align='left'><b>Complaint Givenby</b></th>";
            Complaints += "</tr>";
            int indexnow = 1;
            if (Collegecomplaints.Count != 0)
            {
                foreach (var item in Collegecomplaints)
                {
                    var complaint =
                        db.jntuh_complaints.Where(c => c.id == item.complaintId)
                            .Select(s => s.complaintType)
                            .FirstOrDefault();
                    string complaintdate = string.Empty;
                    if (item.complaintDate != null)
                        complaintdate = UAAAS.Models.Utilities.MMDDYY2DDMMYY(item.complaintDate.ToString());

                    Complaints += "<tr>";
                    Complaints += "<td  align='left'>" + complaint +
                                "</td><td align=''left'>" + complaintdate + "</td><td align=''left'>" + item.givenBy + "</td>";
                    Complaints += "</tr>";
                    indexnow++;
                }
            }
            else
            {
                Complaints += "<tr>";
                Complaints += "<td colspan='6' align='center'><b>NIL</b></td>";
                Complaints += "</tr>";
            }
            Complaints += "</table>";
            return Complaints;
        }

        private int GetIntake(int collegeId, int academicYearId, int specializationId, int shiftId, int flag, int DegreeId)
        {
            int intake = 0;

            //Degree B.Tech  
            if (DegreeId == 4)
            {
                //admitted
                if (flag == 1 && (academicYearId == 14 || academicYearId == 13 || academicYearId == 12 || academicYearId == 11 || academicYearId == 10 || academicYearId == 9 || academicYearId == 8 || academicYearId == 3))
                {
                    intake = db.jntuh_college_intake_existing.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == shiftId && i.isActive == true).Select(i => i.admittedIntake).FirstOrDefault();

                }
                else if (flag == 2 && (academicYearId == 14 || academicYearId == 13 || academicYearId == 12 || academicYearId == 11 || academicYearId == 10 || academicYearId == 9 || academicYearId == 8 || academicYearId == 3))
                {
                    intake = db.jntuh_college_intake_existing.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == shiftId && i.isActive == true).Select(i => i.aicteApprovedIntake).FirstOrDefault();

                }
                else if (flag == 1 && academicYearId == 15)
                {
                    var inta = db.jntuh_college_intake_existing.FirstOrDefault(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == shiftId && i.isActive == true);
                    if (inta != null)
                    {
                        intake = Convert.ToInt32(inta.proposedIntake);
                    }

                }
                else if (flag == 3)//Exam Branch Regular Intake
                {
                    var inta = db.jntuh_college_intake_existing.FirstOrDefault(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == shiftId && i.isActive == true);
                    if (inta != null)
                    {
                        intake = Convert.ToInt32(inta.admittedIntakeasperExambranch_R);
                    }

                }
                else   //approved
                {
                    intake = db.jntuh_college_intake_existing.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == shiftId && i.isActive == true).Select(i => i.approvedIntake).FirstOrDefault();
                }
            }
            else
            {
                //admitted
                if (flag == 1 && academicYearId != 15)
                {
                    intake = db.jntuh_college_intake_existing.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == shiftId && i.isActive == true).Select(i => i.admittedIntake).FirstOrDefault();

                }
                else if (flag == 1 && academicYearId == 15)
                {
                    var inta = db.jntuh_college_intake_existing.FirstOrDefault(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == shiftId && i.isActive == true);
                    if (inta != null)
                    {
                        intake = Convert.ToInt32(inta.proposedIntake);
                    }

                }
                else if (flag == 2) //AICTE
                {
                    if (academicYearId == 15)
                    {
                        intake = 0;
                    }
                    else
                    {
                        intake = db.jntuh_college_intake_existing.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == shiftId && i.isActive == true).Select(i => i.aicteApprovedIntake).FirstOrDefault();
                    }
                }
                else //JNTU Approved
                {
                    intake = db.jntuh_college_intake_existing.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == shiftId && i.isActive == true).Select(i => i.approvedIntake).FirstOrDefault();
                }
            }
            return intake;
        }

        private int GetSectionBasedonIntake(int Intake)
        {
            int Section = 0;

            if (Intake > 0 && Intake <= 60)
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

        private bool GetAcademicYear(int collegeId, int academicYearId, int specializationId, int shiftId, int DegreeId)
        {
            var firstOrDefault = db.jntuh_college_intake_existing.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == shiftId && i.isActive == true).Select(i => i.courseAffiliatedStatus).FirstOrDefault();
            return firstOrDefault ?? false;
        }

        private int GetBtechAdmittedIntake(int Intake)
        {
            int BtechAdmittedIntake = 0;
            if (Intake > 0 && Intake <= 60)
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

        public string AdministrativeLandDetails(int collegeId)
        {
            var contents = string.Empty;
            using (var db = new UAAAS.Models.uaaasDBContext())
            {
                string strAdministrativeLandDetails = string.Empty;
                decimal totalArea = 0;
                //strAdministrativeLandDetails += "<table width='100%' border='0' cellpadding='5' cellspacing='0'>";
                //strAdministrativeLandDetails += "<tr>";
                //strAdministrativeLandDetails += "<td align='left'><b><u>Administrative Area</u></b></td>";
                //strAdministrativeLandDetails += "</tr>";
                //strAdministrativeLandDetails += "</table>";
                strAdministrativeLandDetails += "<table border='1' cellpadding='3' cellspacing='0' width='100%' style='font-family: Times New Roman; font-size: 9px; font-weight: normal; background-color: #fff; margin: 0 auto;'><tbody>";
                strAdministrativeLandDetails += "<tr>";
                strAdministrativeLandDetails += "<td width='35%'><p><b>Type</b></p></td>";
                //strAdministrativeLandDetails += "<td width='18%'><p align='left'><b>Program</b></p></td>";
                strAdministrativeLandDetails += "<td width='9%'><p align='center'><b>Required Rooms</b></p></td>";
                strAdministrativeLandDetails += "<td width='9%'><p align='center'><b>Uploaded Rooms</b></p></td>";
                strAdministrativeLandDetails += "<td width='5%'><p align='center'><b>CF</b></p></td>";
                strAdministrativeLandDetails += "<td width='9%'><p align='center'><b>Deficiency</b></p></td>";
                strAdministrativeLandDetails += "<td width='9%'><p align='center'><b>Required Area (Sq.m)</b></p></td>";
                strAdministrativeLandDetails += "<td width='9%'><p align='center'><b>Uploaded Area (Sq.m)</b></p></td>";
                strAdministrativeLandDetails += "<td width='5%'><p align='center'><b>CF</b></p></td>";
                strAdministrativeLandDetails += "<td width='9%'><p align='center'><b>Deficiency</b></p></td>";
                strAdministrativeLandDetails += "</tr>";
                IQueryable<jntuh_college_area> jntuh_college_area = db.jntuh_college_area.Where(s => s.collegeId == collegeId).Select(e => e);
                IQueryable<jntuh_program_type> jntuh_program_type = db.jntuh_program_type.Select(e => e);
                int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
                int prAy = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();

                //List<int> collegeDegrees = db.jntuh_college_degree
                //                         .Join(db.jntuh_degree, cd => cd.degreeId, d => d.id, (cd, d) => new { cd, d })
                //                         .Where(s => s.d.isActive == true && s.cd.collegeId == collegeId && s.cd.isActive == true)
                //                         .OrderBy(s => s.d.degreeDisplayOrder)
                //                         .Select(s => s.d.id).ToList();

                List<int> collegeDegrees = (from ie in db.jntuh_college_intake_existing
                                            join s in db.jntuh_specialization on ie.specializationId equals s.id
                                            join d in db.jntuh_department on s.departmentId equals d.id
                                            join de in db.jntuh_degree on d.degreeId equals de.id
                                            //where ie.academicYearId == (prAy - 1) && (ie.aicteApprovedIntake != 0 || ie.approvedIntake != 0) && ie.collegeId == userCollegeID
                                            where ie.academicYearId == (prAy) && (ie.proposedIntake != 0) && ie.collegeId == collegeId
                                            orderby de.degreeDisplayOrder
                                            select de.id
                ).Distinct().ToList();

                var collegeSpecs = (from ie in db.jntuh_college_intake_existing
                                    join s in db.jntuh_specialization on ie.specializationId equals s.id
                                    join d in db.jntuh_department on s.departmentId equals d.id
                                    join de in db.jntuh_degree on d.degreeId equals de.id
                                    where ie.isActive == true && ie.academicYearId == (prAy) && (ie.proposedIntake != 0) && ie.collegeId == collegeId && de.id == 4
                                    orderby de.degreeDisplayOrder
                                    select d.id
                ).Distinct().ToList();
                var rrCount = 0;
                if (collegeDegrees.Contains(4))
                {
                    rrCount = collegeSpecs.Count + 4;
                }
                if (collegeDegrees.Contains(6) && collegeDegrees.Contains(3))
                {
                    rrCount = rrCount + 2;
                }
                else if (collegeDegrees.Contains(6) || collegeDegrees.Contains(3))
                {
                    rrCount = rrCount + 1;
                }

                var collegeFacultyCount = db.jntuh_college_faculty_registered.AsNoTracking().Where(i => i.collegeId == collegeId).Count();

                List<AdminLand> land = db.jntuh_area_requirement.Where(r => r.isActive == true && r.areaType == "ADMINISTRATIVE" && !r.requirementType.ToLower().Contains("desirable")).OrderBy(r => r.areaTypeDisplayOrder)
                                        .Select(r => new AdminLand
                                        {
                                            id = r.id,
                                            requirementType = r.requirementType,
                                            programId = r.programId,
                                            requiredRooms = r.requiredRooms,
                                            requiredRoomsCalculation = r.requiredRoomsCalculation,
                                            requiredArea = r.requiredArea,
                                            requiredAreaCalculation = r.requiredAreaCalculation,
                                            areaTypeDescription = r.areaTypeDescription,
                                            areaTypeDisplayOrder = r.areaTypeDisplayOrder,
                                            jntuh_program_type = jntuh_program_type.Where(p => p.id == r.programId).FirstOrDefault(),
                                            availableRooms = jntuh_college_area.Where(a => a.areaRequirementId == r.id).Select(a => a.availableRooms).FirstOrDefault(),
                                            availableArea = jntuh_college_area.Where(a => a.areaRequirementId == r.id).Select(a => a.availableArea).FirstOrDefault(),
                                            cfRooms = jntuh_college_area.Where(a => a.areaRequirementId == r.id).Select(a => a.cfrooms).FirstOrDefault(),
                                            cfArea = jntuh_college_area.Where(a => a.areaRequirementId == r.id).Select(a => a.cfarea).FirstOrDefault(),
                                        }).Where(g => g.availableRooms != null && g.availableRooms != 0).ToList();
                if (land != null)
                {
                    var indexCount = 0;
                    foreach (var item in land)
                    {
                        string programType = jntuh_program_type.Where(p => p.id == item.programId).Select(p => p.programType).FirstOrDefault();
                        if (programType == null)
                        {
                            programType = string.Empty;
                        }

                        var requiredRooms = string.Empty;
                        var requiredArea = string.Empty;
                        if (programType != "B.Pharmacy")
                        {
                            if (item.id == 6) // FacultyRooms
                            {
                                //requiredRooms = collegeFacultyCount.ToString();
                                //requiredArea = (collegeFacultyCount * 5).ToString();
                                requiredRooms = TotalAreaRequiredFaculty.ToString();
                                requiredArea = (TotalAreaRequiredFaculty * 5).ToString();
                            }
                            else if (item.id == 7) // Cabin for HOD
                            {
                                requiredRooms = rrCount.ToString();
                                requiredArea = (rrCount * 20).ToString();
                            }
                            else
                            {
                                requiredRooms = Convert.ToInt16(item.requiredRooms).ToString();
                                requiredArea = Convert.ToDecimal(item.requiredArea).ToString();
                            }
                        }
                        else if (programType == "B.Pharmacy")
                        {
                            if (item.id == 6) // FacultyRooms
                            {
                                requiredRooms = collegeFacultyCount.ToString();
                                requiredArea = (collegeFacultyCount * 10).ToString();
                            }
                            else if (item.id == 7) // Cabin for HOD
                            {
                                var bpharmrrCount = 1;
                                requiredRooms = bpharmrrCount.ToString();
                                requiredArea = (bpharmrrCount * 20).ToString();
                            }
                            else if (item.id == 1) // Cabin for HOD
                            {
                                var bpharmrrCount = 1;
                                requiredRooms = bpharmrrCount.ToString();
                                requiredArea = (bpharmrrCount * 20).ToString();
                            }
                            else if (item.id == 20) // Boy’s Common room
                            {
                                var bpharmrrCount = 1;
                                requiredRooms = bpharmrrCount.ToString();
                                requiredArea = (bpharmrrCount * 10).ToString();
                            }
                            else if (item.id == 22) // Girl’s Common room
                            {
                                var bpharmrrCount = 1;
                                requiredRooms = bpharmrrCount.ToString();
                                requiredArea = (bpharmrrCount * 20).ToString();
                            }
                            else if (item.id == 13) // Examination Control Office
                            {
                                var bpharmrrCount = 1;
                                requiredRooms = bpharmrrCount.ToString();
                                requiredArea = (bpharmrrCount * 80).ToString();
                            }
                            else if (item.id == 8) // Central Stores
                            {
                                var bpharmrrCount = 1;
                                requiredRooms = bpharmrrCount.ToString();
                                requiredArea = (bpharmrrCount * 120).ToString();
                            }
                            else if (item.id == 4) // Office All Inclusive
                            {
                                var bpharmrrCount = 1;
                                requiredRooms = bpharmrrCount.ToString();
                                requiredArea = (bpharmrrCount * 75).ToString();
                            }
                            else if (item.id == 14) // Placement Office
                            {
                                var bpharmrrCount = 1;
                                requiredRooms = bpharmrrCount.ToString();
                                requiredArea = (bpharmrrCount * 100).ToString();
                            }
                            else
                            {
                                requiredRooms = Convert.ToInt16(item.requiredRooms).ToString();
                                requiredArea = Convert.ToDecimal(item.requiredArea).ToString();
                            }
                        }
                        var defTxtRooms = string.Empty;
                        var defTxtArea = string.Empty;

                        if (item.cfRooms != null && item.cfRooms > 0)
                        {
                            var defCond = Convert.ToDecimal(requiredRooms) - Convert.ToDecimal(item.cfRooms);
                            if (defCond > 0)
                            {
                                defTxtRooms = "Yes";
                            }
                            else
                            {
                                defTxtRooms = "-";
                            }
                        }
                        else
                        {
                            var defCond = Convert.ToDecimal(requiredRooms) - Convert.ToDecimal(item.availableRooms);
                            if (defCond > 0)
                            {
                                defTxtRooms = "Yes";
                            }
                            else
                            {
                                defTxtRooms = "-";
                            }
                        }
                        if (item.cfArea != null && item.cfArea > 0)
                        {
                            var defCond = Convert.ToDecimal(requiredArea) - Convert.ToDecimal(item.cfArea);
                            if (defCond > 0)
                            {
                                defTxtArea = "Yes";
                            }
                            else
                            {
                                defTxtArea = "-";
                            }
                        }
                        else
                        {
                            var defCond = Convert.ToDecimal(requiredArea) - Convert.ToDecimal(item.availableArea);
                            if (defCond > 0)
                            {
                                defTxtArea = "Yes";
                            }
                            else
                            {
                                defTxtArea = "-";
                            }
                        }

                        if (defTxtRooms == "Yes" || defTxtArea == "Yes")
                        {
                            strAdministrativeLandDetails += "<tr>";
                            if (programType == "B.Pharmacy" && item.requirementType == "Examination Control Office")
                            {
                                strAdministrativeLandDetails += "<td width='35%'><p>Confidential Room</p></td>";
                            }
                            else if (programType == "B.Pharmacy" && item.requirementType == "Central Stores")
                            {
                                strAdministrativeLandDetails += "<td width='35%'><p>Store Room 1&2</p></td>";
                            }
                            else if (programType == "B.Pharmacy" && item.requirementType == "Office All Inclusive")
                            {
                                strAdministrativeLandDetails += "<td width='35%'><p>Office-1 Establishment</p></td>";
                            }
                            else if (programType == "B.Pharmacy" && item.requirementType == "Placement Office")
                            {
                                strAdministrativeLandDetails += "<td width='35%'><p>Office-2 Academics</p></td>";
                            }
                            else
                            {
                                strAdministrativeLandDetails += "<td width='35%'><p>" + item.requirementType + "</p></td>";
                            }

                            //strAdministrativeLandDetails += "<td width='18%'>" + programType + "</td>";
                            strAdministrativeLandDetails += "<td width='9%' align='center'>" + Convert.ToInt32(requiredRooms) + "</td>";
                            if (item.availableRooms != null)
                            {
                                strAdministrativeLandDetails += "<td width='9%' align='center'>" + (int)item.availableRooms + "</td>";
                            }
                            else
                            {
                                strAdministrativeLandDetails += "<td width='9%' align='center'>" + item.availableRooms + "</td>";
                            }
                            strAdministrativeLandDetails += "<td width='5%' align='center'>" + Convert.ToInt32(item.cfRooms) + "</td>";
                            strAdministrativeLandDetails += "<td width='9%' align='center' style='font-weight: bold'>" + defTxtRooms + "</td>";
                            strAdministrativeLandDetails += "<td width='9%' align='center'>" + requiredArea + "</td>";
                            strAdministrativeLandDetails += "<td width='9%' align='center'>" + item.availableArea + "</td>";
                            strAdministrativeLandDetails += "<td width='5%' align='center'>" + item.cfArea + "</td>";

                            strAdministrativeLandDetails += "<td width='9%' align='center' style='font-weight: bold'>" + defTxtArea + "</td>";
                            strAdministrativeLandDetails += "</tr>";
                            indexCount++;
                        }
                        //if (item.availableArea != null)
                        //{
                        //    totalArea += (decimal)item.availableArea;
                        //}
                    }
                    if (indexCount == 0)
                    {
                        strAdministrativeLandDetails += "<tr><td colspan='9' align='center'><b>NIL</b></td></tr>";
                    }
                }
                //strAdministrativeLandDetails += "<tr>";
                //strAdministrativeLandDetails += "<td colspan='3' align='right'><b>Total</b></td>";
                //strAdministrativeLandDetails += "<td width='9%' align='right'>" + totalArea + "</td>";
                //strAdministrativeLandDetails += "</tr>";
                strAdministrativeLandDetails += "</tbody></table>";
                contents = strAdministrativeLandDetails;
                // List<IElement> parsedHtmlElements = HTMLWorker.ParseToList(new StringReader(contents), null);
                return contents;
            }
        }

        public string InstructionalAreaDetails(int collegeId)
        {
            var contents = string.Empty;
            double TotalCollegeIntake = 0;
            using (var db = new UAAAS.Models.uaaasDBContext())
            {
                string strInstructionalAreaDetails = string.Empty;
                decimal totalArea = 0;
                IQueryable<jntuh_college_area> jntuh_college_area = db.jntuh_college_area.Where(s => s.collegeId == collegeId).Select(e => e);
                //strInstructionalAreaDetails += "<table width='100%' border='0' cellpadding='5' cellspacing='0'>";
                //strInstructionalAreaDetails += "<tr>";
                //strInstructionalAreaDetails += "<td align='left'><b><u>Instructional Area</u></b></td>";
                //strInstructionalAreaDetails += "</tr>";
                //strInstructionalAreaDetails += "</table>";
                strInstructionalAreaDetails += "<table border='1' cellpadding='3' cellspacing='0' width='100%' style='font-family: Times New Roman; font-size: 9px; font-weight: normal; background-color: #fff; margin: 0 auto;'><tbody>";
                strInstructionalAreaDetails += "<tr><td width='35%'><p><b>Requirement Type</b></p></td><td width='9%'><p align='center'><b>Required Rooms</b></p></td><td width='9%'><p align='center'><b>Uploaded Rooms</b></p></td><td width='5%'><p align='center'><b>CF</b></p></td><td width='9%'><p align='center'><b>Deficiency</b></p></td><td width='9%'><p align='center'><b>Required Area (Sq.m)</b></p></td><td width='9%'><p align='center'><b>Uploaded Area (Sq.m)</b></p></td><td width='5%'><p align='center'><b>CF</b></p></td><td width='9%'><p align='center'><b>Deficiency</b></p></td></tr>";
                int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
                int prAy = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();

                var collegeSpecs = (from ie in db.jntuh_college_intake_existing
                                    join s in db.jntuh_specialization on ie.specializationId equals s.id
                                    join d in db.jntuh_department on s.departmentId equals d.id
                                    join de in db.jntuh_degree on d.degreeId equals de.id
                                    where ie.isActive == true && ie.academicYearId == (prAy) && (ie.proposedIntake != 0) && ie.collegeId == collegeId && de.id == 4
                                    orderby de.degreeDisplayOrder
                                    select s.id
                ).Distinct().ToList();

                var collegeSpecApprovedSum = (from ie in db.jntuh_approvedadmitted_intake
                                              join s in db.jntuh_specialization on ie.SpecializationId equals s.id
                                              join d in db.jntuh_department on s.departmentId equals d.id
                                              join de in db.jntuh_degree on d.degreeId equals de.id
                                              where (ie.AcademicYearId == (prAy - 1) || ie.AcademicYearId == (prAy - 2) || ie.AcademicYearId == (prAy - 3)) && (ie.ApprovedIntake != 0) && ie.collegeId == collegeId && de.id == 4
                                              orderby de.degreeDisplayOrder
                                              select ie.ApprovedIntake
                ).ToList();

                var collegeSpecSum = (from ie in db.jntuh_college_intake_existing
                                      join s in db.jntuh_specialization on ie.specializationId equals s.id
                                      join d in db.jntuh_department on s.departmentId equals d.id
                                      join de in db.jntuh_degree on d.degreeId equals de.id
                                      where ie.isActive == true && ie.academicYearId == (prAy) && (ie.proposedIntake != 0) && ie.collegeId == collegeId && de.id == 4
                                      orderby de.degreeDisplayOrder
                                      select ie.proposedIntake
                ).Sum();

                var MtechcollegeSpecs = (from ie in db.jntuh_college_intake_existing
                                         join s in db.jntuh_specialization on ie.specializationId equals s.id
                                         join d in db.jntuh_department on s.departmentId equals d.id
                                         join de in db.jntuh_degree on d.degreeId equals de.id
                                         where ie.isActive == true && ie.academicYearId == (prAy) && (ie.proposedIntake != 0) && ie.collegeId == collegeId && de.id == 1
                                         orderby de.degreeDisplayOrder
                                         select s.id
                ).Distinct().ToList();

                var MtechcollegeSpecSum = (from ie in db.jntuh_college_intake_existing
                                           join s in db.jntuh_specialization on ie.specializationId equals s.id
                                           join d in db.jntuh_department on s.departmentId equals d.id
                                           join de in db.jntuh_degree on d.degreeId equals de.id
                                           where ie.isActive == true && ie.academicYearId == (prAy) && (ie.proposedIntake != 0) && ie.collegeId == collegeId && de.id == 1
                                           orderby de.degreeDisplayOrder
                                           select ie.proposedIntake
                ).Sum();

                var MtechcollegeSpecApprovedSum = (from ie in db.jntuh_approvedadmitted_intake
                                                   join s in db.jntuh_specialization on ie.SpecializationId equals s.id
                                                   join d in db.jntuh_department on s.departmentId equals d.id
                                                   join de in db.jntuh_degree on d.degreeId equals de.id
                                                   where (ie.AcademicYearId == (prAy - 1)) && (ie.ApprovedIntake != 0) && ie.collegeId == collegeId && de.id == 1
                                                   orderby de.degreeDisplayOrder
                                                   select ie.ApprovedIntake
                ).ToList();

                var MBAProposedSpecSum = (from ie in db.jntuh_college_intake_existing
                                          join s in db.jntuh_specialization on ie.specializationId equals s.id
                                          join d in db.jntuh_department on s.departmentId equals d.id
                                          join de in db.jntuh_degree on d.degreeId equals de.id
                                          where ie.isActive == true && ie.academicYearId == (prAy) && (ie.proposedIntake != 0) && ie.collegeId == collegeId && de.id == 6
                                          orderby de.degreeDisplayOrder
                                          select ie.proposedIntake
                ).Sum();

                var MBAApprovedSum = (from ie in db.jntuh_approvedadmitted_intake
                                      join s in db.jntuh_specialization on ie.SpecializationId equals s.id
                                      join d in db.jntuh_department on s.departmentId equals d.id
                                      join de in db.jntuh_degree on d.degreeId equals de.id
                                      where (ie.AcademicYearId == (prAy - 1)) && (ie.ApprovedIntake != 0) && ie.collegeId == collegeId && de.id == 6
                                      orderby de.degreeDisplayOrder
                                      select ie.ApprovedIntake
                ).ToList();

                var MCAProposedSpecSum = (from ie in db.jntuh_college_intake_existing
                                          join s in db.jntuh_specialization on ie.specializationId equals s.id
                                          join d in db.jntuh_department on s.departmentId equals d.id
                                          join de in db.jntuh_degree on d.degreeId equals de.id
                                          where ie.isActive == true && ie.academicYearId == (prAy) && (ie.proposedIntake != 0) && ie.collegeId == collegeId && de.id == 3
                                          orderby de.degreeDisplayOrder
                                          select ie.proposedIntake
                ).Sum();

                var MCAApprovedSum = (from ie in db.jntuh_approvedadmitted_intake
                                      join s in db.jntuh_specialization on ie.SpecializationId equals s.id
                                      join d in db.jntuh_department on s.departmentId equals d.id
                                      join de in db.jntuh_degree on d.degreeId equals de.id
                                      where (ie.AcademicYearId == (prAy - 1)) && (ie.ApprovedIntake != 0) && ie.collegeId == collegeId && de.id == 3
                                      orderby de.degreeDisplayOrder
                                      select ie.ApprovedIntake
                ).ToList();

                var mPharmacySpecSum = (from ie in db.jntuh_college_intake_existing
                                        join s in db.jntuh_specialization on ie.specializationId equals s.id
                                        join d in db.jntuh_department on s.departmentId equals d.id
                                        join de in db.jntuh_degree on d.degreeId equals de.id
                                        where ie.isActive == true && ie.academicYearId == (prAy) && (ie.proposedIntake != 0) && ie.collegeId == collegeId && de.id == 2
                                        orderby de.degreeDisplayOrder
                                        select s.id
                ).Distinct().ToList();

                //List<string> collegeDegrees = db.jntuh_college_degree
                //                         .Join(db.jntuh_degree, cd => cd.degreeId, d => d.id, (cd, d) => new { cd, d })
                //                         .Where(s => s.d.isActive == true && s.cd.collegeId == collegeId && s.cd.isActive == true)
                //                         .OrderBy(s => s.d.degreeDisplayOrder)
                //                         .Select(s => s.d.degree).ToList();
                List<string> collegeDegrees = (from ie in db.jntuh_college_intake_existing
                                               join s in db.jntuh_specialization on ie.specializationId equals s.id
                                               join d in db.jntuh_department on s.departmentId equals d.id
                                               join de in db.jntuh_degree on d.degreeId equals de.id
                                               //where ie.academicYearId == (prAy - 1) && (ie.aicteApprovedIntake != 0 || ie.approvedIntake != 0) && ie.collegeId == userCollegeID
                                               where ie.isActive == true && ie.academicYearId == (prAy) && (ie.proposedIntake != 0) && ie.collegeId == collegeId
                                               orderby de.degreeDisplayOrder
                                               select de.degree
                ).Distinct().ToList();

                var strpharmacyDegs = new string[] { "M.Pharmacy", "B.Pharmacy", "Pharm.D", "Pharm.D PB" };
                collegeDegrees = collegeDegrees.Where(i => !strpharmacyDegs.Contains(i)).ToList();
                foreach (string degree in collegeDegrees)
                {
                    var programTypeDegree = db.jntuh_program_type.Where(p => p.programType == degree).FirstOrDefault();

                    int programId = (int)programTypeDegree.id;
                    //string programType = programtypes.Where(it => it.id == item.programId).Select(d => d.programType).FirstOrDefault();
                    List<AdminLand> land = db.jntuh_area_requirement.Where(r => r.isActive == true && r.areaType == "INSTRUCTIONAL" && r.programId == programId).OrderBy(r => r.areaTypeDisplayOrder)
                                 .Select(r => new AdminLand
                                 {
                                     id = r.id,
                                     requirementType = r.requirementType,
                                     programId = r.programId,
                                     requiredRooms = r.requiredRooms,
                                     requiredRoomsCalculation = r.requiredRoomsCalculation,
                                     requiredArea = r.requiredArea,
                                     requiredAreaCalculation = r.requiredAreaCalculation,
                                     areaTypeDescription = r.areaTypeDescription,
                                     areaTypeDisplayOrder = r.areaTypeDisplayOrder,
                                     jntuh_program_type = db.jntuh_program_type.Where(p => p.id == r.programId).FirstOrDefault(),
                                     availableRooms = jntuh_college_area.Where(a => a.areaRequirementId == r.id).Select(a => a.availableRooms).FirstOrDefault(),
                                     availableArea = jntuh_college_area.Where(a => a.areaRequirementId == r.id).Select(a => a.availableArea).FirstOrDefault(),
                                     cfRooms = jntuh_college_area.Where(a => a.areaRequirementId == r.id).Select(a => a.cfrooms).FirstOrDefault(),
                                     cfArea = jntuh_college_area.Where(a => a.areaRequirementId == r.id).Select(a => a.cfarea).FirstOrDefault()
                                 }).ToList();
                    var indexcount = 0;
                    strInstructionalAreaDetails += "<tr>";
                    strInstructionalAreaDetails += "<td colspan='9' style='width: 200%'><p><b>" + programTypeDegree.programType + "</b></p></td>";
                    strInstructionalAreaDetails += "</tr>";
                    foreach (var i in land)
                    {
                        var requiredRooms = string.Empty;
                        var requiredArea = string.Empty;

                        if (i.programId == 4)
                        {
                            if (i.id == 27) // ClassRooms - B.Tech
                            {
                                var clgspecapprovedSum = collegeSpecApprovedSum.Sum(j => j);
                                TotalCollegeIntake = ((int)collegeSpecSum + clgspecapprovedSum);
                                var calc = (((int)collegeSpecSum + clgspecapprovedSum) / 60) * 0.5;
                                var rrooms = (int)Math.Ceiling(Convert.ToDouble(calc));
                                var rarea = rrooms * 66;
                                requiredRooms = rrooms.ToString();
                                requiredArea = rarea.ToString();
                            }
                            else if (i.id == 28) // TutorialRooms - B.Tech
                            {
                                var clgspecapprovedSum = collegeSpecApprovedSum.Sum(e => e);
                                var calc = ((collegeSpecSum + clgspecapprovedSum) / 60) * 0.5;
                                var rrooms = (int)Math.Ceiling(Convert.ToDouble(calc)) * 0.25;
                                var rarea = rrooms * 33;
                                requiredRooms = Math.Ceiling(rrooms).ToString();
                                requiredArea = Math.Ceiling(rarea).ToString();
                            }
                            else if (i.id == 30) // Workshop (all courses) - B.Tech
                            {
                                if (collegeSpecSum <= 600)
                                {
                                    requiredRooms = Convert.ToInt16(i.requiredRooms).ToString();
                                    requiredArea = Convert.ToDecimal(i.requiredArea).ToString();
                                }
                                else if (collegeSpecSum > 600 && collegeSpecSum <= 1200)
                                {
                                    requiredRooms = "2";
                                    requiredArea = "400";
                                }
                                else if (collegeSpecSum > 1200 && collegeSpecSum <= 1800)
                                {
                                    requiredRooms = "3";
                                    requiredArea = "600";
                                }
                                else
                                {
                                    requiredRooms = "4";
                                    requiredArea = "800";
                                }
                            }
                            else if (i.id == 32) // Computer Center - B.Tech
                            {
                                if (collegeSpecSum <= 600)
                                {
                                    requiredRooms = Convert.ToInt16(i.requiredRooms).ToString();
                                    requiredArea = Convert.ToDecimal(i.requiredArea).ToString();
                                }
                                else if (collegeSpecSum > 600 && collegeSpecSum <= 1200)
                                {
                                    requiredRooms = "2";
                                    requiredArea = "300";
                                }
                                else if (collegeSpecSum > 1200 && collegeSpecSum <= 1800)
                                {
                                    requiredRooms = "3";
                                    requiredArea = "450";
                                }
                                else
                                {
                                    requiredRooms = "4";
                                    requiredArea = "600";
                                }
                            }
                            else if (i.id == 33) // Drawing Hall/CAD Centre - B.Tech
                            {
                                if (collegeSpecSum <= 600)
                                {
                                    requiredRooms = Convert.ToInt16(i.requiredRooms).ToString();
                                    requiredArea = Convert.ToDecimal(i.requiredArea).ToString();
                                }
                                else if (collegeSpecSum > 600 && collegeSpecSum <= 1200)
                                {
                                    requiredRooms = "2";
                                    requiredArea = "264";
                                }
                                else if (collegeSpecSum > 1200 && collegeSpecSum <= 1800)
                                {
                                    requiredRooms = "3";
                                    requiredArea = "396";
                                }
                                else
                                {
                                    requiredRooms = "4";
                                    requiredArea = "528";
                                }
                            }
                            else if (i.id == 34) // Library - B.Tech
                            {
                                requiredRooms = Convert.ToInt16(i.requiredRooms).ToString();
                                var clgspecProposedSum = collegeSpecSum;
                                var calc = ((clgspecProposedSum - 420) / 60) * 50;
                                requiredArea = calc.ToString();
                            }
                            else if (i.id == 94) // Language Laboratory - B.Tech
                            {
                                var rrooms = collegeSpecSum / 300;
                                var rarea = rrooms * 33;
                                requiredRooms = Math.Ceiling(Convert.ToDecimal(rrooms)).ToString();
                                requiredArea = rarea.ToString();
                            }
                            else if (i.id == 29) // Laboratory other than first Year - B.Tech
                            {
                                var rrooms = (collegeSpecs.Count * 6) + 4;
                                var rarea = rrooms * 66;
                                requiredRooms = rrooms.ToString();
                                requiredArea = rarea.ToString();
                            }
                            else
                            {
                                requiredRooms = Convert.ToInt16(i.requiredRooms).ToString();
                                requiredArea = Convert.ToDecimal(i.requiredArea).ToString();
                            }
                        }
                        else if (i.programId == 5)
                        {
                            if (i.id == 36) // ClassRooms - M.Tech
                            {
                                var calc = MtechcollegeSpecs.Count * 2;
                                var rrooms = (int)Math.Ceiling(Convert.ToDouble(calc));
                                var rarea = rrooms * 33;
                                requiredRooms = rrooms.ToString();
                                requiredArea = rarea.ToString();
                            }
                            else if (i.id == 37) // Laboratory - M.Tech
                            {
                                var rrooms = (MtechcollegeSpecs.Count);
                                var rarea = rrooms * 66;
                                requiredRooms = rrooms.ToString();
                                requiredArea = rarea.ToString();
                            }
                            //else if (i.id == 38) // Research Laboratory - M.Tech
                            //{
                            //    var rrooms = (MtechcollegeSpecs.Count);
                            //    var rarea = rrooms * 66;
                            //    requiredRooms = rrooms.ToString();
                            //    requiredArea = rarea.ToString();
                            //}
                            else
                            {
                                requiredRooms = Convert.ToInt16(i.requiredRooms).ToString();
                                requiredArea = Convert.ToDecimal(i.requiredArea).ToString();
                            }
                        }
                        else if (i.programId == 8)
                        {
                            if (i.id == 58) // ClassRooms - MBA
                            {
                                var calc = ((MBAProposedSpecSum * 2) / 60);
                                var rrooms = (int)Math.Ceiling(Convert.ToDouble(calc));
                                var rarea = rrooms * 66;
                                requiredRooms = rrooms.ToString();
                                requiredArea = rarea.ToString();
                            }
                            else if (i.id == 59) // TutorialRooms - MBA
                            {
                                var calc = ((MBAProposedSpecSum * 2) / 60);
                                var rrooms = (int)Math.Ceiling(Convert.ToDouble(calc)) * 0.25;
                                var rarea = rrooms * 33;
                                requiredRooms = Math.Ceiling(rrooms).ToString();
                                requiredArea = Math.Ceiling(rarea).ToString();
                            }
                            else
                            {
                                requiredRooms = Convert.ToInt16(i.requiredRooms).ToString();
                                requiredArea = Convert.ToDecimal(i.requiredArea).ToString();
                            }
                        }
                        else if (i.programId == 9)
                        {
                            if (i.id == 63) // ClassRooms - MCA
                            {
                                var calc = ((MCAProposedSpecSum * 2) / 60);
                                var rrooms = (int)Math.Ceiling(Convert.ToDouble(calc));
                                var rarea = rrooms * 66;
                                requiredRooms = rrooms.ToString();
                                requiredArea = rarea.ToString();
                            }
                            else if (i.id == 64) // TutorialRooms - MCA
                            {
                                var calc = ((MCAProposedSpecSum * 2) / 60);
                                var rrooms = (int)Math.Ceiling(Convert.ToDouble(calc)) * 0.25;
                                var rarea = rrooms * 33;
                                requiredRooms = Math.Ceiling(rrooms).ToString();
                                requiredArea = Math.Ceiling(rarea).ToString();
                            }
                            else
                            {
                                requiredRooms = Convert.ToInt16(i.requiredRooms).ToString();
                                requiredArea = Convert.ToDecimal(i.requiredArea).ToString();
                            }
                        }
                        else if (i.programId == 6)
                        {
                            requiredRooms = Convert.ToInt16(i.requiredRooms).ToString();
                            requiredArea = Convert.ToDecimal(i.requiredArea).ToString();
                        }
                        else if (i.programId == 7)
                        {
                            if (i.id == 51) // ClassRooms - M.Pharmacy
                            {
                                var rrooms = mPharmacySpecSum.Count * 2;
                                var rarea = rrooms * 36;
                                requiredRooms = rrooms.ToString();
                                requiredArea = rarea.ToString();
                            }
                            else if (i.id == 52) // Laboratory - M.Pharmacy
                            {
                                var rrooms = mPharmacySpecSum.Count * 2;
                                var rarea = (rrooms * 75) + 10;
                                requiredRooms = rrooms.ToString();
                                requiredArea = rarea.ToString();
                            }
                            else
                            {
                                requiredRooms = Convert.ToInt16(i.requiredRooms).ToString();
                                requiredArea = Convert.ToDecimal(i.requiredArea).ToString();
                            }
                        }
                        if (requiredRooms == "" || requiredArea == "")
                        {
                            requiredRooms = "0";
                            requiredArea = "0";
                        }
                        var defTxtRooms = string.Empty;
                        var defTxtArea = string.Empty;
                        if (i.cfRooms != null && i.cfRooms > 0)
                        {
                            var defCond = Convert.ToDecimal(requiredRooms) - Convert.ToDecimal(i.cfRooms);
                            if (defCond > 0)
                            {
                                defTxtRooms = "Yes";
                            }
                            else
                            {
                                defTxtRooms = "-";
                            }
                        }
                        else
                        {
                            var defCond = Convert.ToDecimal(requiredRooms) - Convert.ToDecimal(i.availableRooms);
                            if (defCond > 0)
                            {
                                defTxtRooms = "Yes";
                            }
                            else
                            {
                                defTxtRooms = "-";
                            }
                        }
                        if (i.cfArea != null && i.cfArea > 0)
                        {
                            var defCond = Convert.ToDecimal(requiredArea) - Convert.ToDecimal(i.cfArea);
                            if (defCond > 0)
                            {
                                defTxtArea = "Yes";
                            }
                            else
                            {
                                defTxtArea = "-";
                            }
                        }
                        else
                        {
                            var defCond = Convert.ToDecimal(requiredArea) - Convert.ToDecimal(i.availableArea);
                            if (defCond > 0)
                            {
                                defTxtArea = "Yes";
                            }
                            else
                            {
                                defTxtArea = "-";
                            }
                        }
                        if (defTxtRooms == "Yes" || defTxtArea == "Yes")
                        {
                            //indexcount += indexcount;
                            strInstructionalAreaDetails += "<tr>";
                            strInstructionalAreaDetails += "<td width='35%'><p>" + i.requirementType + "</p></td>";
                            strInstructionalAreaDetails += "<td width='9%' align='center'> " + Convert.ToInt32(requiredRooms) + " </td>";
                            if (i.availableRooms != null)
                            {
                                strInstructionalAreaDetails += "<td width='9%' align='center'>" + (int)i.availableRooms + "</td>";
                            }
                            else
                            {
                                strInstructionalAreaDetails += "<td width='9%' align='center'>" + i.availableRooms + "</td>";
                            }
                            strInstructionalAreaDetails += "<td width='5%' align='center'>" + Convert.ToInt32(i.cfRooms) + "</td>";
                            strInstructionalAreaDetails += "<td width='9%' align='center' style='font-weight: bold'>" + defTxtRooms + "</td>";
                            strInstructionalAreaDetails += "<td width='9%' align='center'> " + requiredArea + " </td>";
                            strInstructionalAreaDetails += "<td width='9%' align='center'>" + i.availableArea + "</td>";
                            strInstructionalAreaDetails += "<td width='5%' align='center'>" + i.cfArea + "</td>";
                            strInstructionalAreaDetails += "<td width='9%' align='center' style='font-weight: bold'>" + defTxtArea + "</td>";
                            strInstructionalAreaDetails += "</tr>";
                            indexcount++;
                        }
                        //if (i.availableArea != null)
                        //{
                        //    totalArea += (int)i.availableArea;
                        //}
                    }
                    if (indexcount == 0)
                    {
                        strInstructionalAreaDetails += "<tr><td colspan='9' align='center'><b>NIL</b></td></tr>";
                    }
                    //}
                }
                //strInstructionalAreaDetails += "<tr>";
                //strInstructionalAreaDetails += "<td colspan='2' align='right'>Total</td>";
                //strInstructionalAreaDetails += "<td width='10%' align='right'>" + totalArea + "</td>";
                //strInstructionalAreaDetails += "</tr>";

                strInstructionalAreaDetails += "</tbody>";
                strInstructionalAreaDetails += "</table><br/>";

                //var proposedDegreeContents = string.Empty;
                //proposedDegreeContents += "<table width='100%' border='0' cellpadding='5' cellspacing='0'>";
                //proposedDegreeContents += "<tr>";
                //proposedDegreeContents += "<td align='left'><b><u>Intake Details</u></b></td>";
                //proposedDegreeContents += "</tr>";
                //proposedDegreeContents += "</table>";
                //proposedDegreeContents += "<table width='100%' border='1' cellpadding='7' cellspacing='0'>";

                //proposedDegreeContents += "<tr>";
                //proposedDegreeContents +=
                //    "<th align='left'>Degree/Program</th><th align='left'>No.of Courses/Specializations</th><th align='left'>Total Intake</th><th align='left'>Total Divisions</th>";
                //proposedDegreeContents += "</tr>";

                //foreach (var deg in collegeDegrees)
                //{
                //    if (deg == "B.Tech")
                //    {
                //        var totalcollegebtechdivisions = Math.Ceiling(TotalCollegeIntake / 60);
                //        proposedDegreeContents += "<tr>";
                //        proposedDegreeContents += "<td align=''left'>" + deg.ToString() + "</td><td  align='left'>" + collegeSpecs.Count +
                //                "</td><td align=''left'>" + TotalCollegeIntake.ToString() + "</td><td align=''left'>" + totalcollegebtechdivisions.ToString() + "</td>";
                //        proposedDegreeContents += "</tr>";
                //    }
                //    else if (deg == "M.Tech")
                //    {
                //        var mtechclgspecApprovesSum = MtechcollegeSpecApprovedSum.Sum(i => i);
                //        var totalMtechIntake = (int)MtechcollegeSpecSum + mtechclgspecApprovesSum;
                //        var totalcollegemtechdivisions = Math.Ceiling(Convert.ToDouble(totalMtechIntake) / 30);
                //        proposedDegreeContents += "<tr>";
                //        proposedDegreeContents += "<td align=''left'>" + deg.ToString() + "</td><td  align='left'>" + MtechcollegeSpecs.Count +
                //                "</td><td align=''left'>" + totalMtechIntake.ToString() + "</td><td align=''left'>" + totalcollegemtechdivisions.ToString() + "</td>";
                //    }
                //    else if (deg == "MBA")
                //    {
                //        var mbaspecs = 1;
                //        var clgMBAapprovedSum = MBAApprovedSum.Sum(i => i);
                //        var totalMBAIntake = (int)MBAProposedSpecSum + clgMBAapprovedSum;
                //        var totalcollegembadivisions = Math.Ceiling(Convert.ToDouble(totalMBAIntake) / 60);
                //        proposedDegreeContents += "<tr>";
                //        proposedDegreeContents += "<td align=''left'>" + deg.ToString() + "</td><td  align='left'>" + mbaspecs.ToString() +
                //                "</td><td align=''left'>" + totalMBAIntake.ToString() + "</td><td align=''left'>" + totalcollegembadivisions.ToString() + "</td>";
                //    }
                //    else if (deg == "MCA")
                //    {
                //        var mcaspecs = 1;
                //        var MCAApprovesTotal = 0;
                //        if (MCAApprovedSum.Count > 0)
                //        {
                //            MCAApprovesTotal = MCAApprovedSum.Sum(i => i);
                //        }
                //        var totalMCAIntake = (int)MCAProposedSpecSum + MCAApprovesTotal;
                //        var totalcollegemcadivisions = Math.Ceiling(Convert.ToDouble(totalMCAIntake) / 60);
                //        proposedDegreeContents += "<tr>";
                //        proposedDegreeContents += "<td align=''left'>" + deg.ToString() + "</td><td  align='left'>" + mcaspecs.ToString() +
                //                "</td><td align=''left'>" + totalMCAIntake.ToString() + "</td><td align=''left'>" + totalcollegemcadivisions.ToString() + "</td>";
                //    }
                //    else
                //    {
                //        proposedDegreeContents += "<tr>";
                //        proposedDegreeContents += "<td colspan='6' align='center'><b>NIL</b></td>";
                //        proposedDegreeContents += "</tr>";
                //    }
                //}
                //proposedDegreeContents += "</table>";
                //strInstructionalAreaDetails += proposedDegreeContents;
                //strInstructionalAreaDetails += "<li>Total Intake : <b>" + TotalCollegeIntake.ToString() + "</b>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; Total Divisions : <b>" + totalcollegedivisions.ToString() + "</b>.</li>";
                contents = strInstructionalAreaDetails;
                //List<IElement> parsedHtmlElements = HTMLWorker.ParseToList(new StringReader(contents), null);
                return contents;
            }
        }

        public string DepartmentWiseNPTLFaculty(int collegeId)
        {
            int[] NptlcollegeIds = { 42, 68, 72, 88, 147, 148, 187, 223, 254, 276, 334, 374, 441 };
            if (!NptlcollegeIds.Contains(collegeId))
            {
                string NPTLFaculty = string.Empty;
                NPTLFaculty += "<table border='1' cellpadding='3' cellspacing='0' width='100%' style='font-family: Times New Roman; font-size: 9px; font-weight: normal; background-color: #fff; margin: 0 auto;'>";
                NPTLFaculty += "<tr>";
                NPTLFaculty += "<td colspan='8' align='center'><b>NIL</b></td>";
                NPTLFaculty += "</tr>";
                NPTLFaculty += "</table>";
                return NPTLFaculty;
            }
            else
            {
                string NPTLFaculty = string.Empty;
                int[] collegeIDs = null;
                List<CollegeIntakeExisting> collegeIntakeExisting = new List<CollegeIntakeExisting>();
                List<CollegeFacultyWithIntakeReport> intakedetailsList = new List<CollegeFacultyWithIntakeReport>();
                var jntuh_departments = db.jntuh_department.ToList();
                string CollegeAffiliationStatus = db.jntuh_college_affiliation.Where(u => u.collegeId == collegeId && u.affiliationTypeId == 7).Select(u => u.affiliationStatus).FirstOrDefault();
                DateTime newfacultyfromdate = new DateTime(2023, 04, 04);
                int studentcount = Convert.ToInt32(ConfigurationManager.AppSettings["studentcount"]);
                if (collegeId != null)
                {
                    if (collegeId != 0)
                    {
                        collegeIDs = db.jntuh_college.Where(c => c.isActive == true && c.id == collegeId).Select(c => c.id).ToArray();
                    }
                    else
                    {
                        collegeIDs = db.jntuh_college.Where(c => c.isActive == true).Select(c => c.id).ToArray();
                    }
                    int facultystudentRatio = 0;
                    decimal facultyRatio = 0m;
                    decimal SfacultyRatio = 0m;
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
                    List<jntuh_college_intake_existing> intake = db.jntuh_college_intake_existing.Where(i => collegeIDs.Contains(i.collegeId) && i.isActive == true).ToList();

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
                        newIntake.courseStatus = item.courseStatus;
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
                                                             //SpecName = b.specializationName,
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
                    var jntuh_phdfaculty = db.jntuh_faculty_phddetails.AsNoTracking().ToList();
                    var registeredFaculty = db.jntuh_registered_faculty.Where(rf => strRegnos.Contains(rf.RegistrationNumber.Trim())).ToList();
                    //principalRegno != null ? db.jntuh_registered_faculty.Where(rf => strRegnos.Contains(rf.RegistrationNumber.Trim()) && rf.RegistrationNumber != principalRegno).ToList(): db.jntuh_registered_faculty.Where(rf => strRegnos.Contains(rf.RegistrationNumber.Trim())).ToList();
                    //  var reg1=registeredFaculty.Where(f => f.RegistrationNumber.Trim() == "9251-150414-062519").ToList();
                    //Reg nos related online facultyIds`-- (rf.isApproved == null || rf.isApproved == true)
                    // var collegeaffliations = db.jntuh_college_affliationstatus.AsNoTracking().ToList();
                    //var regfacultywithdepts = registeredFaculty.Where(rf => rf.DepartmentId == null).ToList();
                    //02-04-2018 OriginalsVerifiedPHD Columns consider as-No Guide Sign in PHD Thesis;OriginalsVerifiedUG column Consider as -Complaint PHD Faculty

                    //&& rf.BAS != "Yes"  && (rf.AbsentforVerification == false || rf.AbsentforVerification == null)
                    //var jntuh_registered_faculty1 =
                    //    registeredFaculty.Where(rf => rf.type != "Adjunct" && ((rf.NotQualifiedAsperAICTE == false || rf.NotQualifiedAsperAICTE == null) && (rf.Noclass == false || rf.Noclass == null) && (rf.Absent == false || rf.Absent == null) && (rf.OriginalCertificatesNotShown == false || rf.OriginalCertificatesNotShown == null) &&
                    //                                         (rf.NoSCM == false || rf.NoSCM == null) && rf.NoRelevantUG != "Yes" && rf.NoRelevantPG != "Yes" && rf.NORelevantPHD != "Yes" && (rf.InvalidPANNumber == false || rf.InvalidPANNumber == null) && (rf.Blacklistfaculy == false) && (rf.AbsentforVerification == false || rf.AbsentforVerification == null) && (rf.DepartmentId != 61 || rf.DepartmentId != 27))).Select(rf => new

                    var jntuh_registered_faculty1 =
                        registeredFaculty.Where(rf => rf.type != "Adjunct" && (rf.BAS != "Yes") && (rf.Noclass == false || rf.Noclass == null) && (rf.Absent == false || rf.Absent == null) &&
                                                             (rf.NoSCM == false || rf.NoSCM == null) && (rf.Blacklistfaculy == false) && (rf.AbsentforVerification == false || rf.AbsentforVerification == null) && (rf.DepartmentId != 61 || rf.DepartmentId != 27)).Select(rf => new
                                                             {
                                                                 FacultyId = rf.id,
                                                                 //Departmentid = rf.DepartmentId,
                                                                 RegistrationNumber = rf.RegistrationNumber.Trim(),
                                                                 // Department = rf.jntuh_department.departmentName,
                                                                 DeptId = jntuh_college_faculty_registered.Where(c => c.RegistrationNumber.Trim() == rf.RegistrationNumber.Trim()).Select(c => c.DepartmentId).FirstOrDefault(),
                                                                 SpecializationId = jntuh_college_faculty_registered.Where(c => c.RegistrationNumber.Trim() == rf.RegistrationNumber.Trim()).Select(c => c.SpecializationId).FirstOrDefault(),
                                                                 NotconsideredPHD = rf.NotconsideredPHD,
                                                                 // HighestDegreeID = rf.jntuh_registered_faculty_education.Count() != 0 ? rf.jntuh_registered_faculty_education.Where(e => e.educationId != 8).Select(e => e.educationId).Max() : 0,
                                                                 HighestDegreeID = rf.NotconsideredPHD == true ?
                                                                             rf.jntuh_registered_faculty_education.Where(r => r.educationId != 8 && r.educationId != 9).Count() != 0 ?
                                                                             rf.jntuh_registered_faculty_education.Where(e => e.educationId != 8 && e.educationId != 6 && e.educationId != 9).Select(e => e.educationId).Max() : 0
                                                                             :
                                                                             rf.jntuh_registered_faculty_education.Where(r => r.educationId != 8 && r.educationId != 9).Count() != 0 ?
                                                                             rf.jntuh_registered_faculty_education.Where(e => e.educationId != 8 && e.educationId != 9).Select(e => e.educationId).Max() : 0,
                                                                 IsApproved = rf.isApproved,
                                                                 Createdon = jntuh_college_faculty_registered.Where(c => c.RegistrationNumber.Trim() == rf.RegistrationNumber.Trim()).Select(c => c.createdOn).FirstOrDefault(),
                                                                 PanNumber = rf.PANNumber,
                                                                 AadhaarNumber = rf.AadhaarNumber,
                                                                 NoForm16 = rf.NoForm16,
                                                                 TotalExperience = rf.TotalExperience,
                                                                 CsePhDFacultyFlag = rf.PhdDeskVerification,
                                                             }).ToList();
                    //BAS Flag
                    int variable = 2;

                    var nohighestdegree = jntuh_registered_faculty1.Where(e => e.HighestDegreeID < 4).ToList();
                    jntuh_registered_faculty1 = jntuh_registered_faculty1.Where(e => e.HighestDegreeID >= 4).ToList();
                    int fcount = jntuh_registered_faculty1.Count();
                    var jntuh_registered_faculty = jntuh_registered_faculty1.Select(rf => new
                    {
                        RegistrationNumber = rf.RegistrationNumber,
                        Department = rf.DeptId != null ? jntuh_departments.Where(e => e.id == rf.DeptId).Select(e => e.departmentName).FirstOrDefault() : DeptNameBasedOnSpecialization.Where(e => e.Specid == rf.SpecializationId).Select(e => e.DeptName).FirstOrDefault(),
                        //Specialization = rf.DeptId != null ? jntuh_specialization.Where(e => e.departmentId == rf.DeptId).Select(e => e.specializationName).FirstOrDefault() : DeptNameBasedOnSpecialization.Where(e => e.Specid == rf.SpecializationId).Select(e => e.SpecName).FirstOrDefault(),
                        NotconsideredPHD = rf.NotconsideredPHD,
                        HighestDegree = jntuh_education_category.Where(c => c.id == rf.HighestDegreeID).Select(c => c.educationCategoryName).FirstOrDefault(),
                        Recruitedfor = jntuh_college_faculty_registered.Where(c => c.RegistrationNumber.Trim() == rf.RegistrationNumber.Trim()).Select(c => c.IdentifiedFor).FirstOrDefault(),
                        rf.SpecializationId,
                        PanNumber = rf.PanNumber,
                        AadhaarNumber = rf.AadhaarNumber,
                        noform16 = rf.NoForm16,
                        Createdon = rf.Createdon,
                        TotalExperience = rf.TotalExperience,
                        Phd2pages = jntuh_phdfaculty.Where(i => i.Facultyid == rf.FacultyId).Count() > 0 ? true : false,
                        CsePhDFacultyFlag = rf.CsePhDFacultyFlag
                    }).Where(e => e.Department != null).ToList();

                    var jntuh_registered_facultyntpl =
                        registeredFaculty.Where(rf => (rf.NotIdentityfiedForanyProgram == true)).Select(rf => new
                        {
                            FacultyId = rf.id,
                            //Departmentid = rf.DepartmentId,
                            RegistrationNumber = rf.RegistrationNumber.Trim(),
                            // Department = rf.jntuh_department.departmentName,
                            DeptId = jntuh_college_faculty_registered.Where(c => c.RegistrationNumber.Trim() == rf.RegistrationNumber.Trim()).Select(c => c.DepartmentId).FirstOrDefault(),
                            SpecializationId = jntuh_college_faculty_registered.Where(c => c.RegistrationNumber.Trim() == rf.RegistrationNumber.Trim()).Select(c => c.SpecializationId).FirstOrDefault(),
                            NotconsideredPHD = rf.NotconsideredPHD,
                            // HighestDegreeID = rf.jntuh_registered_faculty_education.Count() != 0 ? rf.jntuh_registered_faculty_education.Where(e => e.educationId != 8).Select(e => e.educationId).Max() : 0,
                            HighestDegreeID = rf.NotconsideredPHD == true ?
                                        rf.jntuh_registered_faculty_education.Where(r => r.educationId != 8 && r.educationId != 9).Count() != 0 ?
                                        rf.jntuh_registered_faculty_education.Where(e => e.educationId != 8 && e.educationId != 6 && e.educationId != 9).Select(e => e.educationId).Max() : 0
                                        :
                                        rf.jntuh_registered_faculty_education.Where(r => r.educationId != 8 && r.educationId != 9).Count() != 0 ?
                                        rf.jntuh_registered_faculty_education.Where(e => e.educationId != 8 && e.educationId != 9).Select(e => e.educationId).Max() : 0,
                            IsApproved = rf.isApproved,
                            Createdon = jntuh_college_faculty_registered.Where(c => c.RegistrationNumber.Trim() == rf.RegistrationNumber.Trim()).Select(c => c.createdOn).FirstOrDefault(),
                            PanNumber = rf.PANNumber,
                            AadhaarNumber = rf.AadhaarNumber,
                            NoForm16 = rf.NoForm16,
                            TotalExperience = rf.TotalExperience,
                            CsePhDFacultyFlag = rf.PhdDeskVerification,
                        }).ToList();
                    //BAS Flag
                    //int variable = 2;

                    //var nohighestdegree = jntuh_registered_faculty1.Where(e => e.HighestDegreeID < 4).ToList();
                    //jntuh_registered_faculty1 = jntuh_registered_faculty1.Where(e => e.HighestDegreeID >= 4).ToList();
                    //int fcount = jntuh_registered_faculty1.Count();
                    var jntuh_registered_facultyNTPL = jntuh_registered_facultyntpl.Select(rf => new
                    {
                        RegistrationNumber = rf.RegistrationNumber,
                        DeptId = rf.DeptId,
                        Department = rf.DeptId != null ? jntuh_departments.Where(e => e.id == rf.DeptId).Select(e => e.departmentName).FirstOrDefault() : DeptNameBasedOnSpecialization.Where(e => e.Specid == rf.SpecializationId).Select(e => e.DeptName).FirstOrDefault(),
                        //Specialization = rf.DeptId != null ? jntuh_specialization.Where(e => e.departmentId == rf.DeptId).Select(e => e.specializationName).FirstOrDefault() : DeptNameBasedOnSpecialization.Where(e => e.Specid == rf.SpecializationId).Select(e => e.SpecName).FirstOrDefault(),
                        NotconsideredPHD = rf.NotconsideredPHD,
                        HighestDegree = jntuh_education_category.Where(c => c.id == rf.HighestDegreeID).Select(c => c.educationCategoryName).FirstOrDefault(),
                        Recruitedfor = jntuh_college_faculty_registered.Where(c => c.RegistrationNumber.Trim() == rf.RegistrationNumber.Trim()).Select(c => c.IdentifiedFor).FirstOrDefault(),
                        rf.SpecializationId,
                        PanNumber = rf.PanNumber,
                        AadhaarNumber = rf.AadhaarNumber,
                        noform16 = rf.NoForm16,
                        Createdon = rf.Createdon,
                        TotalExperience = rf.TotalExperience,
                        Phd2pages = jntuh_phdfaculty.Where(i => i.Facultyid == rf.FacultyId).Count() > 0 ? true : false,
                        CsePhDFacultyFlag = rf.CsePhDFacultyFlag
                    }).Where(e => e.Department != null).ToList();
                    int[] StrPharmacy = new[] { 26, 27, 36, 39 };
                    foreach (var item in collegeIntakeExisting.Where(D => !StrPharmacy.Contains(D.DepartmentID)).ToList())
                    {
                        var intakedetails = new CollegeFacultyWithIntakeReport();
                        intakedetails.ispercentage = false;
                        intakedetails.isstarcourses = false;
                        int phdFaculty; int oldphdFaculty = 0; int newphdFaculty = 0; int pgFaculty; int oldpgFaculty = 0; int newpgFaculty = 0; int ugFaculty; int oldugFaculty = 0; int newugFaculty = 0; int SpecializationphdFaculty = 0; int SpecializationpgFaculty = 0;

                        intakedetails.collegeId = item.collegeId;
                        intakedetails.collegeCode = jntuh_college.Where(c => c.id == item.collegeId).Select(c => c.collegeCode).FirstOrDefault();
                        intakedetails.collegeName = jntuh_college.Where(c => c.id == item.collegeId).Select(c => c.collegeName).FirstOrDefault();
                        intakedetails.Degree = item.Degree;
                        intakedetails.Department = item.Department;
                        intakedetails.Specialization = item.Specialization;
                        intakedetails.shiftId = item.shiftId;
                        item.courseStatus = db.jntuh_college_intake_existing.Where(
                            e =>
                                e.specializationId == item.specializationId && e.shiftId == item.shiftId &&
                                e.academicYearId == 15 &&
                                e.collegeId == item.collegeId && e.isActive == true).Select(s => s.courseStatus).FirstOrDefault();
                        if (item.Specialization == "Industrial Pharmacy")
                        {

                        }
                        intakedetails.specializationId = item.specializationId;
                        intakedetails.DepartmentID = item.DepartmentID;
                        //var status = collegeaffliations.Where(i => i.DegreeID == item.degreeID && i.SpecializationId == item.specializationId && i.CollegeId == item.collegeId).ToList();
                        //if (status.Count > 0)
                        //{
                        //    intakedetails.AffliationStatus = "A";
                        //}
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

                        //intakedetails.approvedIntake1 = GetIntake(item.collegeId, AY1, item.specializationId, item.shiftId, 1, item.degreeID);
                        //intakedetails.approvedIntake2 = GetIntake(item.collegeId, AY2, item.specializationId, item.shiftId, 1, item.degreeID);
                        //intakedetails.approvedIntake3 = GetIntake(item.collegeId, AY3, item.specializationId, item.shiftId, 1, item.degreeID);
                        //intakedetails.approvedIntake4 = GetIntake(item.collegeId, AY4, item.specializationId, item.shiftId, 1, item.degreeID);
                        //intakedetails.approvedIntake5 = GetIntake(item.collegeId, AY5, item.specializationId, item.shiftId, 1, item.degreeID);

                        intakedetails.AffiliationStatus2 = GetAcademicYear(item.collegeId, AY1, item.specializationId, item.shiftId, item.degreeID);
                        intakedetails.AffiliationStatus3 = GetAcademicYear(item.collegeId, AY2, item.specializationId, item.shiftId, item.degreeID);
                        intakedetails.AffiliationStatus4 = GetAcademicYear(item.collegeId, AY3, item.specializationId, item.shiftId, item.degreeID);

                        //Get sanction Inake for Btech
                        //if (item.degreeID == 4)
                        //{
                        //    intakedetails.SanctionIntake1 = GetIntake(item.collegeId, AY1, item.specializationId, item.shiftId, 0, item.degreeID);
                        //    intakedetails.SanctionIntake2 = GetIntake(item.collegeId, AY2, item.specializationId, item.shiftId, 0, item.degreeID);
                        //    intakedetails.SanctionIntake3 = GetIntake(item.collegeId, AY3, item.specializationId, item.shiftId, 0, item.degreeID);
                        //    intakedetails.SanctionIntake4 = GetIntake(item.collegeId, AY4, item.specializationId, item.shiftId, 0, item.degreeID);
                        //    intakedetails.SanctionIntake5 = GetIntake(item.collegeId, AY5, item.specializationId, item.shiftId, 0, item.degreeID);
                        //}
                        intakedetails.SanctionIntake1 = GetIntake(item.collegeId, AY1, item.specializationId, item.shiftId, 0, item.degreeID);
                        intakedetails.SanctionIntake2 = GetIntake(item.collegeId, AY2, item.specializationId, item.shiftId, 0, item.degreeID);
                        intakedetails.SanctionIntake3 = GetIntake(item.collegeId, AY3, item.specializationId, item.shiftId, 0, item.degreeID);
                        intakedetails.SanctionIntake4 = GetIntake(item.collegeId, AY4, item.specializationId, item.shiftId, 0, item.degreeID);
                        intakedetails.SanctionIntake5 = GetIntake(item.collegeId, AY5, item.specializationId, item.shiftId, 0, item.degreeID);
                        facultystudentRatio = Convert.ToInt32(jntuh_faculty_student_ratio_norms.Where(fn => fn.degreeId == item.degreeID).Select(fn => fn.Norms).FirstOrDefault().Split(':')[1].ToString());

                        var csedept = jntuh_registered_faculty.Where(i => i.Department == item.Department).ToList();
                        //intakedetails.form16count = form16Count.Count > 0 ? form16Count.Count(i => i.DepartmentId == item.DepartmentID) : 0;
                        //intakedetails.aictecount = aictecount.Count > 0 ? aictecount.Count(i => i.DepartmentId == item.DepartmentID) : 0;


                        if (item.Degree == "B.Tech")
                        {
                            //25% Calculation Based on Admitted Intake intakes on 418
                            //Take Higest of 3 Years Of Admitated Intake
                            //Propasedintake or admittedIntake1  means Proposed Intake of Present Year
                            //int SanctionIntakeHigest = Max(intakedetails.admittedIntake2, intakedetails.admittedIntake3, intakedetails.admittedIntake4);
                            //SanctionIntakeHigest = GetBtechAdmittedIntake(SanctionIntakeHigest);
                            //int senondyearpercentage = 0;
                            //int thirdyearpercentage = 0;
                            //int fourthyearpercentage = 0;
                            ////Comment on 26-07--2018 Due Count
                            ////Comment on 02-05-2018 Due Count
                            //if (intakedetails.SanctionIntake2 != 0)
                            //{
                            //    senondyearpercentage = (int)Math.Ceiling(Convert.ToDecimal((Convert.ToDecimal(intakedetails.admittedIntake2) / Convert.ToDecimal(intakedetails.SanctionIntake2)) * 100));
                            //}
                            //if (intakedetails.SanctionIntake3 != 0)
                            //{
                            //    thirdyearpercentage = (int)Math.Ceiling(Convert.ToDecimal((Convert.ToDecimal(intakedetails.admittedIntake3) / Convert.ToDecimal(intakedetails.SanctionIntake3)) * 100));
                            //}
                            //if (intakedetails.SanctionIntake4 != 0)
                            //{
                            //    fourthyearpercentage = (int)Math.Ceiling(Convert.ToDecimal((Convert.ToDecimal(intakedetails.admittedIntake4) / Convert.ToDecimal(intakedetails.SanctionIntake4)) * 100));
                            //}

                            //if (senondyearpercentage < 25 && thirdyearpercentage < 25 && fourthyearpercentage < 25)
                            //{
                            //    intakedetails.ispercentage = true;
                            //    //studentcount
                            //    if ((intakedetails.admittedIntake2 >= studentcount || intakedetails.admittedIntake3 >= studentcount || intakedetails.admittedIntake3 >= studentcount) && intakedetails.Proposedintake != 0)
                            //    {
                            //        intakedetails.ispercentage = false;
                            //        intakedetails.isstarcourses = true;
                            //        //intakedetails.ReducedInatke = 60;
                            //        //if (intakedetails.approvedIntake1 != 60)
                            //        //{
                            //        //    intakedetails.Note = "(Reduced from Proposed Intake of <b>";
                            //        //    intakedetails.Note += intakedetails.approvedIntake1;
                            //        //    intakedetails.Note += intakedetails.approvedIntake1;
                            //        //    intakedetails.Note += "</b> as per 25% Clause)";
                            //        //    intakedetails.approvedIntake1 = 60;
                            //        //}
                            //    }
                            //}
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
                                    //intakedetails.ispercentage = true;
                                    intakedetails.ispercentage = false;
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
                            #region This Is Commented on 16-04-2019 due to Calculate on AICTE/JNTU Sanctioned Intakes only
                            //if (intakedetails.ispercentage == false)
                            //{
                            //    if (item.courseStatus == "New")
                            //    {
                            //        intakedetails.approvedIntake2 = GetBtechAdmittedIntake(intakedetails.SanctionIntake2);
                            //        intakedetails.approvedIntake3 = 0;
                            //        intakedetails.approvedIntake4 = 0;
                            //    }
                            //    else if (SanctionIntakeHigest >= intakedetails.Proposedintake)
                            //    {
                            //        //New Code 
                            //        intakedetails.approvedIntake2 = GetBtechAdmittedIntake(intakedetails.admittedIntake2);
                            //        intakedetails.approvedIntake3 = GetBtechAdmittedIntake(intakedetails.admittedIntake3);
                            //        intakedetails.approvedIntake4 = GetBtechAdmittedIntake(intakedetails.admittedIntake4);


                            //    }
                            //    else
                            //    {
                            //        intakedetails.approvedIntake2 = GetBtechAdmittedIntake(intakedetails.SanctionIntake2);
                            //        intakedetails.approvedIntake3 = GetBtechAdmittedIntake(intakedetails.SanctionIntake3);
                            //        intakedetails.approvedIntake4 = GetBtechAdmittedIntake(intakedetails.SanctionIntake4);
                            //    }

                            //    intakedetails.totalIntake = (intakedetails.approvedIntake2) + (intakedetails.approvedIntake3) +
                            //                                (intakedetails.approvedIntake4);

                            //    facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) /
                            //                       Convert.ToDecimal(facultystudentRatio);

                            //    //intakedetails.totalAdmittedIntake = (intakedetails.admittedIntake2) + (intakedetails.admittedIntake3) + (intakedetails.admittedIntake4);
                            //    //AICTE Intake
                            //    intakedetails.totalAdmittedIntake = (intakedetails.AICTESanctionIntake2) + (intakedetails.AICTESanctionIntake3) + (intakedetails.AICTESanctionIntake4);
                            //    intakedetails.totalSanctionIntake = (intakedetails.SanctionIntake2) + (intakedetails.SanctionIntake3) + (intakedetails.SanctionIntake4);

                            //}
                            #endregion
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
                            //int[] StrNewSpecs = new[] { 180, 181, 182, 183, 184, 185, 186, 187, 188, 189, 190, 191, 192, 193, 194, 195, 196 };
                            int takecondition = Convert.ToInt32(ConfigurationManager.AppSettings["intakecondition"]);
                            if (takecondition == 1)
                            {
                                //if (StrNewSpecs.Contains(item.specializationId) && intakedetails.Proposedintake != 0 && intakedetails.SanctionIntake1 == 0 && intakedetails.SanctionIntake2 == 0 && intakedetails.SanctionIntake3 == 0)
                                //{
                                //    intakedetails.totalIntake = 0;
                                //}
                                //else if (item.Department == "CSE(SE)" || item.Department == "CSE(CS)" || item.Department == "CSE(AI&ML)" || item.Department == "CSE(DS)" || item.Department == "CSE(IoT)" || item.Department == "CSE(Networks)" || item.DepartmentID == 22)
                                //{
                                //    intakedetails.totalIntake = GetBtechAdmittedIntake(intakedetails.SanctionIntake2) + GetBtechAdmittedIntake(intakedetails.AICTESanctionIntake3);
                                //}
                                //else
                                //{
                                //    intakedetails.totalIntake = GetBtechAdmittedIntake(intakedetails.AICTESanctionIntake2) + GetBtechAdmittedIntake(intakedetails.AICTESanctionIntake3) +
                                //                        GetBtechAdmittedIntake(intakedetails.AICTESanctionIntake4);
                                //}
                                intakedetails.totalIntake = GetBtechAdmittedIntake(intakedetails.AICTESanctionIntake2) + GetBtechAdmittedIntake(intakedetails.AICTESanctionIntake3) +
                                                        GetBtechAdmittedIntake(intakedetails.AICTESanctionIntake4);
                            }
                            else if (takecondition == 2)
                            {
                                //if (StrNewSpecs.Contains(item.specializationId) && intakedetails.Proposedintake != 0 && intakedetails.SanctionIntake1 == 0 && intakedetails.SanctionIntake2 == 0 && intakedetails.SanctionIntake3 == 0)
                                //{
                                //    intakedetails.totalIntake = 0;
                                //}
                                //else if (item.Department == "CSE(SE)" || item.Department == "CSE(CS)" || item.Department == "CSE(AI&ML)" || item.Department == "CSE(DS)" || item.Department == "CSE(IoT)" || item.Department == "CSE(Networks)" || item.DepartmentID == 22)
                                //{
                                //    intakedetails.totalIntake = GetBtechAdmittedIntake(intakedetails.SanctionIntake2) + GetBtechAdmittedIntake(intakedetails.AICTESanctionIntake3);
                                //}
                                //else
                                //{
                                //    intakedetails.totalIntake = GetBtechAdmittedIntake(intakedetails.SanctionIntake2) +
                                //                           GetBtechAdmittedIntake(intakedetails.SanctionIntake3) +
                                //                           GetBtechAdmittedIntake(intakedetails.SanctionIntake4);
                                //}
                                intakedetails.totalIntake = GetBtechAdmittedIntake(intakedetails.SanctionIntake2) +
                                                           GetBtechAdmittedIntake(intakedetails.SanctionIntake3) +
                                                           GetBtechAdmittedIntake(intakedetails.SanctionIntake4);
                            }
                            else
                            {
                                //if (StrNewSpecs.Contains(item.specializationId) && intakedetails.Proposedintake != 0 && intakedetails.SanctionIntake1 == 0 && intakedetails.SanctionIntake2 == 0 && intakedetails.SanctionIntake3 == 0)
                                //{
                                //    intakedetails.totalIntake = 0;
                                //}
                                //else if (item.Department == "CSE(SE)" || item.Department == "CSE(CS)" || item.Department == "CSE(AI&ML)" || item.Department == "CSE(DS)" || item.Department == "CSE(IoT)" || item.Department == "CSE(Networks)" || item.DepartmentID == 22)
                                //{
                                //    intakedetails.totalIntake = GetBtechAdmittedIntake(intakedetails.SanctionIntake2) + GetBtechAdmittedIntake(intakedetails.AICTESanctionIntake3);
                                //}
                                //else
                                //{
                                //    intakedetails.totalIntake = GetBtechAdmittedIntake(intakedetails.admittedIntake2) +
                                //                           GetBtechAdmittedIntake(intakedetails.admittedIntake3) +
                                //                           GetBtechAdmittedIntake(intakedetails.admittedIntake4);
                                //}
                                intakedetails.totalIntake = GetBtechAdmittedIntake(intakedetails.admittedIntake2) +
                                                           GetBtechAdmittedIntake(intakedetails.admittedIntake3) +
                                                           GetBtechAdmittedIntake(intakedetails.admittedIntake4);
                            }
                            facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) /
                                               Convert.ToDecimal(facultystudentRatio);

                            intake2 = GetBtechAdmittedIntake(intakedetails.SanctionIntake2);
                            intake3 = GetBtechAdmittedIntake(intakedetails.SanctionIntake3);
                            intake4 = GetBtechAdmittedIntake(intakedetails.SanctionIntake4);
                            totalintake = (intake2) + (intake3) +
                                                            (intake4);

                            SfacultyRatio = Convert.ToDecimal(totalintake) /
                                               Convert.ToDecimal(facultystudentRatio);

                            //else
                            //{
                            //    if (SanctionIntakeHigest >= intakedetails.approvedIntake1)
                            //    {
                            //        //New Code 
                            //        intakedetails.admittedIntake2 = GetBtechAdmittedIntake(intakedetails.approvedIntake2);
                            //        intakedetails.admittedIntake3 = GetBtechAdmittedIntake(intakedetails.approvedIntake3);
                            //        intakedetails.admittedIntake4 = GetBtechAdmittedIntake(intakedetails.approvedIntake4);


                            //    }
                            //    else
                            //    {
                            //        intakedetails.admittedIntake2 = GetBtechAdmittedIntake(intakedetails.SanctionIntake2);
                            //        intakedetails.admittedIntake3 = GetBtechAdmittedIntake(intakedetails.SanctionIntake3);
                            //        intakedetails.admittedIntake4 = GetBtechAdmittedIntake(intakedetails.SanctionIntake4);
                            //    }

                            //    intakedetails.totalIntake = (intakedetails.admittedIntake2) + (intakedetails.admittedIntake3) +
                            //                                (intakedetails.admittedIntake4);
                            //    facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) /
                            //                   Convert.ToDecimal(facultystudentRatio);


                            //    intakedetails.totalAdmittedIntake = (intakedetails.SanctionIntake2) + (intakedetails.SanctionIntake3) + (intakedetails.SanctionIntake4);
                            //}

                            //intakedetails.A416TotalFaculty = registeredFaculty.Where(i => i.DepartmentId == item.DepartmentID && (i.jntuh_department.jntuh_degree.degree == "B.Tech" || i.jntuh_department.jntuh_degree.degree == "B.Pharmacy")).ToList().Count;
                        }
                        else if (item.Degree == "M.Tech" || item.Degree == "MBA")
                        {
                            //intakedetails.totalIntake = (intakedetails.approvedIntake1) + (intakedetails.approvedIntake2);
                            //facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) /
                            //               Convert.ToDecimal(facultystudentRatio);
                            //New Code Written by Narayana
                            if (item.Degree == "M.Tech" && item.shiftId == 1)
                            {
                                //intakedetails.totalIntake = (intakedetails.admittedIntake1) + (intakedetails.admittedIntake2);
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
                                //intakedetails.totalIntake = (intakedetails.admittedIntake1) + (intakedetails.admittedIntake2);
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
                                //intakedetails.totalIntake = (intakedetails.admittedIntake1) + (intakedetails.admittedIntake2);
                                intakedetails.totalIntake = (intakedetails.Proposedintake) + (intakedetails.AICTESanctionIntake2);
                                facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) /
                                               Convert.ToDecimal(facultystudentRatio);
                            }
                        }
                        else if (item.Degree == "MCA")
                        {
                            //intakedetails.totalIntake = (intakedetails.admittedIntake1) + (intakedetails.admittedIntake2) +(intakedetails.admittedIntake3);

                            intakedetails.totalIntake = (intakedetails.Proposedintake) + (intakedetails.AICTESanctionIntake2);// + (intakedetails.AICTESanctionIntake3)

                            facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) /
                                           Convert.ToDecimal(facultystudentRatio);
                        }
                        else if (item.Degree == "B.Pharmacy")
                        {
                            intakedetails.totalIntake = (intakedetails.admittedIntake1) + (intakedetails.admittedIntake2) +
                                                        (intakedetails.admittedIntake3) + (intakedetails.admittedIntake4);
                            facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) / Convert.ToDecimal(facultystudentRatio);
                        }
                        else if (item.Degree == "M.Pharmacy")
                        {
                            intakedetails.totalIntake = (intakedetails.admittedIntake1) + (intakedetails.admittedIntake2);
                            facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) / 12;

                        }
                        else if (item.Degree == "Pharm.D")
                        {
                            intakedetails.totalIntake = (intakedetails.admittedIntake1) + (intakedetails.admittedIntake2) +
                                                        (intakedetails.admittedIntake3) + (intakedetails.admittedIntake4) +
                                                        (intakedetails.admittedIntake5);
                            facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) / 12;
                        }
                        else if (item.Degree == "Pharm.D PB")
                        {
                            intakedetails.totalIntake = (intakedetails.admittedIntake1) + (intakedetails.admittedIntake2);
                            facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) / 10;
                        }
                        else //MAM MTM
                        {
                            //intakedetails.totalIntake = (intakedetails.admittedIntake1) + (intakedetails.admittedIntake2) +
                            //                            (intakedetails.admittedIntake3) + (intakedetails.admittedIntake4) +
                            //                            (intakedetails.admittedIntake5);
                            intakedetails.totalIntake = (intakedetails.Proposedintake) + (intakedetails.AICTESanctionIntake2) +
                                                        (intakedetails.AICTESanctionIntake3) + (intakedetails.AICTESanctionIntake4) +
                                                        (intakedetails.AICTESanctionIntake5);
                            facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) / Convert.ToDecimal(facultystudentRatio);
                        }

                        intakedetails.requiredFaculty = Math.Round(facultyRatio, 2);
                        intakedetails.SrequiredFaculty = Math.Round(SfacultyRatio, 2);
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

                                //var regno = jntuh_registered_faculty.Where(f => f.Department == item.Department).Select(f => f.RegistrationNumber);//&& (f.Recruitedfor == "UG" || f.Recruitedfor == null)
                                intakedetails.specializationWiseFaculty = jntuh_registered_faculty.Count(f => f.Department == item.Department);//&& (f.Recruitedfor == "UG" || f.Recruitedfor == null)
                                //var testing = jntuh_registered_faculty.Where(f => f.Department == item.Department).ToList();
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
                                            f.SpecializationId == item.specializationId && (f.Recruitedfor == "PG" || f.Recruitedfor == "UG&PG"));//f.Specialization == item.Specialization
                            }
                        }
                        if (strdegreetype == "Dual Degree")
                        {
                            intakedetails.specializationWiseFaculty = jntuh_registered_faculty.Count(f => f.SpecializationId == item.specializationId);//(f.Recruitedfor == "PG" || f.Recruitedfor == "UG&PG") &&
                        }
                        int noPanOrAadhaarcount = 0;

                        if (item.Degree == "B.Pharmacy")
                        {
                            ugFaculty = jntuh_registered_faculty.Count(f => "UG" == f.HighestDegree && f.SpecializationId == item.specializationId);
                            pgFaculty = jntuh_registered_faculty.Count(f => "PG" == f.HighestDegree && f.SpecializationId == item.specializationId);
                            phdFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && f.CsePhDFacultyFlag != true && f.SpecializationId == item.specializationId);
                            //intakedetails.Department = "Pharmacy";
                        }
                        if (item.Degree == "M.Pharmacy")
                        {
                            ugFaculty = jntuh_registered_faculty.Count(f => "UG" == f.HighestDegree && f.Department == "Pharmacy" && f.SpecializationId == item.specializationId);
                            pgFaculty = jntuh_registered_faculty.Count(f => ("PG" == f.HighestDegree || "M.Phil / Other PG Degree" == f.HighestDegree) &&
                                        f.Department == "Pharmacy" && f.SpecializationId == item.specializationId);
                            phdFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && f.CsePhDFacultyFlag != true && f.Department == "Pharmacy" && f.SpecializationId == item.specializationId);
                            //noPanOrAadhaarcount =registeredFaculty.Where(f =>f.DepartmentId == pharmacyDeptId && f.PANNumber == null && f.AadhaarNumber == null &&
                            //            (f.isApproved == null || f.isApproved == true)).Count();
                            //intakedetails.Department = "Pharmacy";
                        }
                        else if (item.Degree == "Pharm.D")
                        {
                            ugFaculty = jntuh_registered_faculty.Count(f => "UG" == f.HighestDegree && f.Department == "Pharm.D" && f.SpecializationId == item.specializationId);
                            pgFaculty = jntuh_registered_faculty.Count(f => ("PG" == f.HighestDegree || "M.Phil / Other PG Degree" == f.HighestDegree) && f.Department == "Pharm.D" && f.SpecializationId == item.specializationId);
                            phdFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && f.CsePhDFacultyFlag != true && f.Department == "Pharm.D" && f.SpecializationId == item.specializationId);
                            //noPanOrAadhaarcount = registeredFaculty.Where(f => f.DepartmentId == pharmacyDeptId && f.PANNumber == null && f.AadhaarNumber == null && (f.isApproved == null || f.isApproved == true)).Count();
                            intakedetails.Department = "Pharm.D";
                        }
                        else if (item.Degree == "Pharm.D PB")
                        {
                            ugFaculty = jntuh_registered_faculty.Count(f => "UG" == f.HighestDegree && f.Department == "Pharm.D PB" && f.SpecializationId == item.specializationId);
                            pgFaculty = jntuh_registered_faculty.Count(f => ("PG" == f.HighestDegree || "M.Phil / Other PG Degree" == f.HighestDegree) && f.Department == "Pharm.D PB" && f.SpecializationId == item.specializationId);
                            phdFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && f.CsePhDFacultyFlag != true && f.Department == "Pharm.D PB" && f.SpecializationId == item.specializationId);
                            //noPanOrAadhaarcount = registeredFaculty.Where(f => f.DepartmentId == pharmacyDeptId && f.PANNumber == null && f.AadhaarNumber == null && (f.isApproved == null || f.isApproved == true)).Count();
                            intakedetails.Department = "Pharm.D PB";
                        }
                        else
                        {
                            ugFaculty = jntuh_registered_faculty.Count(f => "UG" == f.HighestDegree && f.Department == item.Department);
                            pgFaculty = jntuh_registered_faculty.Count(f => ("PG" == f.HighestDegree || "M.Phil / Other PG Degree" == f.HighestDegree) && f.Department == item.Department);
                            phdFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && f.CsePhDFacultyFlag != true && f.Department == item.Department);

                            //Written by Narayana Reddy on 02-03-2019.
                            oldugFaculty = jntuh_registered_faculty.Count(f => "UG" == f.HighestDegree && f.Department == item.Department && f.Createdon <= newfacultyfromdate);
                            newugFaculty = jntuh_registered_faculty.Count(f => "UG" == f.HighestDegree && f.Department == item.Department && f.Createdon >= newfacultyfromdate);
                            oldpgFaculty = jntuh_registered_faculty.Count(f => ("PG" == f.HighestDegree || "M.Phil / Other PG Degree" == f.HighestDegree) && f.Department == item.Department && f.Createdon <= newfacultyfromdate);
                            newpgFaculty = jntuh_registered_faculty.Count(f => ("PG" == f.HighestDegree || "M.Phil / Other PG Degree" == f.HighestDegree) && f.Department == item.Department && f.Createdon >= newfacultyfromdate);
                            oldphdFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && f.CsePhDFacultyFlag != true && f.Department == item.Department && f.Createdon <= newfacultyfromdate);
                            newphdFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && f.CsePhDFacultyFlag != true && f.Department == item.Department && f.Createdon >= newfacultyfromdate);

                            var PhdReg = jntuh_registered_faculty.Where(f => ("Ph.D" == f.HighestDegree) && f.CsePhDFacultyFlag != true && f.Department == item.Department).Select(f => f.RegistrationNumber).ToList();

                            //var phdFaculty1 = jntuh_registered_faculty.Where(f => ("Ph.D" == f.HighestDegree || "Ph.D." == f.HighestDegree || "Phd" == f.HighestDegree || "PHD" == f.HighestDegree || "Ph D" == f.HighestDegree)).ToList() ;
                            //if (item.Department == "MBA")
                            //    phdFaculty1 = phdFaculty1.Where(f => f.Department == "MBA").ToList();

                            if (item.Degree == "B.Tech")
                                SpecializationphdFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && f.CsePhDFacultyFlag != true && (f.SpecializationId == item.specializationId || f.SpecializationId == null) && f.Department == item.Department);
                            else
                                SpecializationphdFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && f.CsePhDFacultyFlag != true && f.SpecializationId == item.specializationId);
                            var reg = jntuh_registered_faculty.Where(f => f.SpecializationId == item.specializationId && (f.Recruitedfor == "PG" || f.Recruitedfor == "UG&PG")).Select(S => S.RegistrationNumber).ToList();
                            SpecializationpgFaculty = jntuh_registered_faculty.Count(f => f.SpecializationId == item.specializationId && (f.Recruitedfor == "PG" || f.Recruitedfor == "UG&PG"));
                        }

                        intakedetails.phdFaculty = phdFaculty;
                        intakedetails.SpecializationsphdFaculty = SpecializationphdFaculty;
                        intakedetails.SpecializationspgFaculty = SpecializationpgFaculty;
                        intakedetails.pgFaculty = pgFaculty;
                        intakedetails.ugFaculty = ugFaculty;
                        intakedetails.totalFaculty = (ugFaculty + pgFaculty + phdFaculty);
                        //intakedetails.DeptWiseBASFlag = jntuh_registered_facultyBAS.Count(f => "Yes" == f.BASFlag && f.DeptId == item.DepartmentID);
                        intakedetails.oldtotalFaculty = (oldugFaculty + oldpgFaculty + oldphdFaculty);
                        intakedetails.newtotalFaculty = (newugFaculty + newpgFaculty + newphdFaculty);
                        intakedetails.A416TotalFaculty = registeredFaculty.Where(i => i.DepartmentId == item.DepartmentID && (i.jntuh_department.jntuh_degree.degree == item.Degree ||
                                     i.jntuh_department.jntuh_degree.degree == item.Degree)).ToList().Count;
                        intakedetails.facultyWithoutPANAndAadhaar = noPanOrAadhaarcount;
                        intakedetails.specializationWiseNTPLFaculty = jntuh_registered_facultyNTPL.Count(f => f.DeptId == item.DepartmentID);

                        intakedetailsList.Add(intakedetails);
                    }
                    List<CollegeFacultyWithIntakeReport> facultyCounts = intakedetailsList.Where(c => c.shiftId == 1).Select(e => e).ToList();
                    string faculty = string.Empty;
                    var distDeptcount = 1;
                    var deptloop = 1;
                    decimal departmentWiseRequiredFaculty = 0;
                    string[] departments = new string[] { "English", "Mathematics", "Physics", "Chemistry", "Others(CSE/IT)", "Others(CIVIL/MECH)", "Others(ECE/EEE)", "Others(MNGT/H&S)" };
                    int[] OthersSpecIds = new int[] { 155, 156, 157, 158 };
                    int TotalCount = 0;
                    int totalBtechFirstYearIntake = intakedetailsList.Where(d => d.Degree == "B.Tech" && !departments.Contains(d.Department)).Select(d => d.Proposedintake).Sum();
                    var degrees = db.jntuh_degree.ToList();
                    var firstYearRequired = Math.Ceiling((double)totalBtechFirstYearIntake / 160);
                    int SpecializationwisePHDFaculty = 0;
                    int SpecializationwisePGFaculty = 0;
                    int remainingPHDFaculty = 0;
                    int remainingSpecializationwisePHDFaculty = 0;
                    int remainingFaculty = 0;
                    int facultyIndexnotEqualtoZeroIndex = 1;
                    int facultyIndexnotEqualtoZeroAdmittedIntakeIndex = 1;
                    int HumantitiesminimamRequireMet = 0;
                    string HumantitiesminimamRequireMetStatus = "Yes";
                    int facultycount = 0;
                    string[] ProposedIntakeZeroDeptName = intakedetailsList.Where(e => e.Degree == "B.Tech" && (e.Proposedintake == 0) && !departments.Contains(e.Department)).Select(e => e.Specialization).Distinct().ToArray();
                    foreach (var item in intakedetailsList.Where(e => e.Proposedintake != 0 && !ProposedIntakeZeroDeptName.Contains(e.Specialization)).Select(e => e).ToList())
                    {
                        #region Code without calculation admitted Intake

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
                            SpecializationwisePHDFaculty = IntakeWisePhdForBtech(item.SanctionIntake2); //SanctionIntake2 - last year approved intake
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
                        if ((item.collegeId == 72 && item.Department == "IT") || (item.collegeId == 130 && item.Department == "IT"))
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

                        faculty += "<tr>";
                        faculty +=
                            "<td class='col2' style='text-align: center; vertical-align: top;width:10px' rowspan='1'>" +
                            facultyIndexnotEqualtoZeroIndex + "</td>";
                        if (item.isstarcourses == true)
                        {
                            faculty += "<td class='col2' style='text-align: left; vertical-align: top;'>" +
                                       item.Department + "#" + "</td>";
                        }
                        else
                        {
                            faculty += "<td class='col2' style='text-align: left; vertical-align: top;'>" +
                                       item.Department + "</td>";
                        }
                        faculty += "<td class='col2' style='text-align: left; vertical-align: top;'>" + item.Degree +
                                   "</td>";
                        if (item.shiftId == 1)
                        {
                            faculty += "<td class='col2' style='text-align: left; vertical-align: top;'>" +
                                       item.Specialization + "</td>";
                        }
                        else
                        {
                            faculty += "<td class='col2' style='text-align: left; vertical-align: top;'>" +
                                       item.Specialization + " -2" + "</td>";
                        }
                        if (departments.Contains(item.Department))
                        {
                            faculty += "<td class='col2' style='text-align: center; vertical-align: top;'> </td>";
                            faculty +=
                                "<td colspan='2' class='col2' style='text-align: center; vertical-align: top;'> </td>";
                            faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" +
                                       totalBtechFirstYearIntake + "</td>";


                            if (deptloop == 1)
                            {
                                if (item.DeptWiseBASFlag > 0)
                                {
                                    faculty += "<td rowspan='" + TotalCount +
                                           "'  class='col2' style='text-align: center; vertical-align: top;'>" +
                                           item.totalFaculty + "(" + item.DeptWiseBASFlag + ")</td>";
                                }
                                else
                                {
                                    faculty += "<td rowspan='" + TotalCount +
                                               "'  class='col2' style='text-align: center; vertical-align: top;'>" +
                                               item.totalFaculty + "</td>";
                                }
                            }
                            if (OthersSpecIds.Contains(item.specializationId))
                            {
                                faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" +
                                           Math.Ceiling(firstYearRequired / 2) + "</td>";
                                HumantitiesminimamRequireMet += item.totalFaculty;
                                //TotalAreaRequiredFaculty = TotalAreaRequiredFaculty + Convert.ToInt32(Math.Ceiling(firstYearRequired / 2));
                            }
                            else
                            {
                                faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" +
                                           Math.Ceiling(firstYearRequired) + "</td>";
                                HumantitiesminimamRequireMet += item.totalFaculty;
                                //TotalAreaRequiredFaculty = TotalAreaRequiredFaculty + Convert.ToInt32(Math.Ceiling(firstYearRequired));
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
                                faculty +=
                                "<td class='col2' style='text-align: center; vertical-align: top;'><table width='100%' border='1' cellpadding='3' cellspacing='0' style='border-color: #ccc;'><tr><td><span>" +
                                item.AICTESanctionIntake2 + "</span></td></tr><tr><td><span>" + item.AICTESanctionIntake3 +
                                "</span></td></tr><tr><td><span>" + item.AICTESanctionIntake4 +
                                "</span></td></tr></table></td>";
                                faculty +=
                                    "<td class='col2' style='text-align: center; vertical-align: top;'><table width='100%' border='1' cellpadding='3' cellspacing='0' style='border-color: #ccc;'><tr><td><span>" +
                                    item.SanctionIntake2 + "</span></td></tr><tr><td><span>" + item.SanctionIntake3 +
                                    "</span></td></tr><tr><td><span>" + item.SanctionIntake4 +
                                    "</span></td></tr></table></td>";

                                faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>";
                                faculty +=
                                    "<table width='100%' border='1' cellpadding='3' cellspacing='0' style='border-color: #ccc;'>";

                                faculty += "<tr><td><span>" + GetSectionBasedonIntake(item.SanctionIntake2) +
                                               "</span></td>";
                                faculty += "</tr><tr><td><span>" + GetSectionBasedonIntake(item.SanctionIntake3) +
                                           "</span></td></tr>";
                                faculty += "<tr><td><span>" + GetSectionBasedonIntake(item.SanctionIntake4) +
                                           "</span></td></tr>";
                                faculty += "</table></td>";
                            }
                            else
                            {
                                faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" +
                                       item.AICTESanctionIntake2 + "</td>";
                                faculty +=
                                    "<td colspan='2' class='col2' style='text-align: center; vertical-align: top;'>" +
                                    item.SanctionIntake2 + " </td>";
                            }
                            if (String.IsNullOrEmpty(item.Note))
                            {
                                faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" +
                                           item.Proposedintake + "</td>";
                            }
                            else
                            {
                                faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" +
                                           item.Proposedintake + " " + item.Note + "</td>";
                            }
                            if (deptloop == 1)
                            {
                                if (item.DeptWiseBASFlag > 0)
                                {
                                    faculty += "<td rowspan='" + TotalCount +
                                           "'  class='col2' style='text-align: center; vertical-align: top;'>" +
                                           item.totalFaculty + "(" + item.DeptWiseBASFlag + ")</td>";
                                }
                                else
                                {
                                    faculty += "<td rowspan='" + TotalCount +
                                           "'  class='col2' style='text-align: center; vertical-align: top;'>" +
                                           item.totalFaculty + "</td>";
                                }

                            }
                            if (item.Degree == "B.Tech")
                            {
                                faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" +
                                       item.requiredFaculty + "</td>";
                                //TotalAreaRequiredFaculty = TotalAreaRequiredFaculty + Convert.ToInt32(item.requiredFaculty);
                            }
                            else
                            {
                                faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" +
                                        Math.Ceiling(item.requiredFaculty) + "</td>";
                                //TotalAreaRequiredFaculty = TotalAreaRequiredFaculty + Convert.ToInt32(item.requiredFaculty);
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

                        faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" +
                                   adjustedFaculty + "</td>";
                        item.AvailableFaculty = item.totalFaculty;
                        if (item.Degree == "M.Tech")
                            faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" +
                                       item.SpecializationspgFaculty + "</td>";
                        else
                            faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + "" +
                                       "</td>";

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

                        faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" +
                                   SpecializationwisePHDFaculty + "</td>";
                        faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" +
                                   adjustedPHDFaculty + "</td>";
                        faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" +
                                   minimumRequirementMet + "</td>";
                        faculty += "</tr>";
                        deptloop++;
                        SpecializationwisePHDFaculty = 0;
                        facultyIndexnotEqualtoZeroIndex++;

                        #endregion
                    }

                    //NPTLFaculty += "<table border='1' cellpadding='3' cellspacing='0' width='100%' style='font-family: Times New Roman; font-size: 9px; font-weight: normal; background-color: #fff; margin: 0 auto;'>";
                    //NPTLFaculty += "<tr>";
                    //NPTLFaculty += "<td align='left'><b><u>Interdisciplinary faculty:</u></b></td>";
                    //NPTLFaculty += "</tr>";
                    //NPTLFaculty += "<tr>";
                    //NPTLFaculty += "<td align='left'>Faculty members specializing in interdisciplinary branches, exceeding 10% of the total department faculty, are considered a deficiency. The excess faculty will be added to the existing deficiency in the corresponding branch.</td>";
                    //NPTLFaculty += "</tr>";
                    //NPTLFaculty += "</table>";
                    NPTLFaculty += "<table border='1' cellpadding='3' cellspacing='0' width='100%' style='font-family: Times New Roman; font-size: 9px; font-weight: normal; background-color: #fff; margin: 0 auto;'>";
                    NPTLFaculty += "<tr>";
                    NPTLFaculty += "<th align='left'>Department</th><th align='left'>Required Faculty</th><th align='left'>NPTEL Eligible Faculty</th><th align='left'>NPTEL Available Faculty</th><th align='left'>Excess Faculty</th><th align='left'>Total Department Faculty</th><th align='left'>Total Eligible Faculty</th><th align='left'>Remarks</th>";
                    NPTLFaculty += "</tr>";
                    int indexnowntpl = 1;
                    if (intakedetailsList.Where(i => i.Proposedintake != 0 && !ProposedIntakeZeroDeptName.Contains(i.Specialization) && i.specializationWiseNTPLFaculty > 0).ToList().Count > 0)
                    {
                        foreach (var item in intakedetailsList.Where(i => i.Proposedintake != 0 && !ProposedIntakeZeroDeptName.Contains(i.Specialization) && i.specializationWiseNTPLFaculty > 0).ToList())
                        {
                            var percentage = Math.Round((item.requiredFaculty * 10) / 100);
                            var excessFacultyStr = string.Empty;
                            var eligibleFacultyStr = string.Empty;
                            var eligibleFaculty = 0m;
                            var excessFaculty = item.specializationWiseNTPLFaculty - percentage;
                            excessFacultyStr = excessFaculty < 0 ? "0" : excessFaculty.ToString();
                            if (excessFaculty > 0)
                            {
                                eligibleFaculty = (int)item.AvailableFaculty - excessFaculty;
                                eligibleFacultyStr = eligibleFaculty.ToString();
                            }
                            eligibleFacultyStr = !string.IsNullOrEmpty(eligibleFacultyStr) ? eligibleFacultyStr : "-";
                            //var deficiencyStr = (eligibleFaculty > 0 && eligibleFaculty > item.AvailableFaculty) ? "Deficiency In faculty" : "-";
                            var coond = item.requiredFaculty - eligibleFaculty;
                            var deficiencyStr = (excessFaculty > 0 && coond >= 1) ? "Deficiency In faculty" : "-";
                            NPTLFaculty += "<tr>";
                            NPTLFaculty += "<td  align='left'>" + item.Department + "</td><td align='left'>" + item.requiredFaculty + "</td><td align='left'>" + percentage + "</td><td align='left'>" + item.specializationWiseNTPLFaculty + "</td><td align='left'>" + excessFacultyStr + "</td><td align='left'>" + item.AvailableFaculty + "</td><td align='left'>" + eligibleFacultyStr + "</td><td align='left'>" + deficiencyStr + "</td>";
                            NPTLFaculty += "</tr>";
                            indexnowntpl++;
                        }
                    }
                    else
                    {
                        NPTLFaculty += "<tr>";
                        NPTLFaculty += "<td colspan='9' align='center'><b>NIL</b></td>";
                        NPTLFaculty += "</tr>";
                    }
                    NPTLFaculty += "</table>";
                    NPTLFaculty += "&nbsp;&nbsp;&nbsp;";
                    NPTLFaculty += "<p style='font-family: Times New Roman; font-size: 9px; font-weight: normal; background-color: #fff; margin: 0 auto;'>The College is permitted to engage faculty members with specialization in interdisciplinary branches not exceeding 10% of the total faculty strength in the concerned discipline.</p><br />";
                    NPTLFaculty += "<p style='font-family: Times New Roman; font-size: 9px; font-weight: normal; background-color: #fff; margin: 0 auto;'>Faculty members engaged with specialization in interdisciplinary branches exceeding 10% of the total faculty strength in the concerned discipline are not considered and it is taken as deficiency. This deficiency is additional to the existing faculty deficiency in the concerned discipline.</p>";
                }
                return NPTLFaculty;
            }
        }
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
        public int DeptWiseBASFlag { get; set; }
        public int oldtotalFaculty { get; set; }
        public int newtotalFaculty { get; set; }
        public int specializationWiseFaculty { get; set; }
        public int specializationWiseNTPLFaculty { get; set; }
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

        //public IList<PharmacySpecilaizationList> PharmacySpecilaizationList { get; set; }

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
}
