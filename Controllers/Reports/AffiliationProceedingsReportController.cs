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

namespace UAAAS.Controllers.Reports
{
    [ErrorHandling]
    public class AffiliationProceedingsReportController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();
        private string bpharmacycondition;
        private string pharmdcondition;
        private string pharmadpbcondition;
        private decimal pharmadpbrequiredfaculty;
        private decimal BpharmacyrequiredFaculty;
        private decimal PharmDRequiredFaculty;
        private decimal PharmDPBRequiredFaculty;
        private int TotalcollegeFaculty;
        private int Group1PharmacyFaculty;
        private int Group2PharmacyFaculty;
        private int Group3PharmacyFaculty;
        private int Group4PharmacyFaculty;
        private int Group5PharmacyFaculty;
        private int Group6PharmacyFaculty;
        private int Allgroupscount;
        private int ApprovedIntake;
        private int specializationId;
        private int PharmaDApprovedIntake;
        private int PharmaDspecializationId;
        private int PharmaDPBApprovedIntake;
        private int PharmaDPBspecializationId;
        private string PharmacyandPharmDMeet = "";
        private int BpharmcyAvilableFaculty = 0;
        public string DeficiencyInPharmacy = "";



        public ActionResult Index()
        {
            return View();
        }

        //deficiency letter formats 

        public ActionResult AffiliationLetter(int collegeId, string type)
        {
            var collegeIds = new[] { 4, 6, 7, 8, 9, 11, 12, 22, 23, 24, 26, 27, 29, 30, 32, 34, 35, 38, 39, 40, 41, 42, 43, 47, 48, 50, 52, 54, 55, 56, 58, 60, 65, 67, 68, 69, 70, 72, 75, 77, 78, 80, 84, 85, 86, 87, 88, 90, 95, 100, 103, 104, 105, 106, 107, 108, 109, 110, 111, 113, 114, 115, 116, 117, 118, 120, 123, 128, 130, 132, 134, 135, 136, 137, 139, 140, 141, 144, 145, 146, 147, 148, 150, 152, 153, 156, 157, 158, 159, 162, 164, 165, 166, 168, 169, 171, 173, 174, 175, 176, 177, 178, 179, 181, 182, 183, 184, 187, 188, 193, 195, 196, 198, 202, 203, 204, 206, 210, 211, 213, 214, 218, 219, 222, 223, 228, 234, 237, 242, 244, 245, 246, 250, 253, 254, 256, 260, 261, 262, 263, 264, 266, 267, 271, 273, 276, 282, 283, 287, 290, 293, 295, 297, 298, 299, 300, 301, 302, 303, 304, 306, 307, 310, 313, 314, 315, 316, 317, 318, 319, 320, 322, 324, 327, 329, 332, 334, 335, 350, 352, 353, 360, 364, 365, 366, 367, 373, 374, 379, 380, 384, 385, 389, 391, 392, 393, 395, 399, 403, 410, 411, 414, 420, 423, 428, 429, 430, 435, 436, 441, 442, 445, 455, 461, 462 };
            //var collegeIds = new[] { 370 };
            string code = db.jntuh_college.Where(c => c.id == collegeId && c.isActive == true).Select(c => c.collegeCode).FirstOrDefault().ToUpper();
            //var collegeCodes = db.jntuh_college.Where(c => collegeIds.Contains(c.id) && c.isActive == true).Select(c => c.collegeCode).ToList();
            //ZipFile zip = new ZipFile();
            //zip.AlternateEncodingUsage = ZipOption.AsNecessary;
            //zip.AddDirectoryByName("Faculty_" + type + "_AffiliationLettesReport");
            //foreach (var item in collegeCodes)
            //{
            //    string path = SaveCollegeDefficiencyLetterPdf(item, type);

            //}
            //string[] filePaths = Directory.GetFiles(Server.MapPath("~/Content/PDFReports/AffiliationProceedings/2023-24/"));

            //foreach (var row in filePaths)
            //{
            //    string filePath = row;
            //    zip.AddFile(filePath, "AffiliationLettesReport");
            //}

            //Response.Clear();
            //Response.BufferOutput = false;
            //string zipName = String.Format(type + "AffiliationLettesReport_{0}.zip", DateTime.Now.ToString("yyyy-MMM-dd-HHmmss"));
            //Response.ContentType = "application/zip";
            //Response.AddHeader("content-disposition", "attachment; filename=" + zipName);
            //zip.Save(Response.OutputStream);
            //Response.End();
            string path = SaveCollegeDefficiencyLetterPdf(code, type);
            path = path.Replace("/", "\\");
            if (!string.IsNullOrEmpty(path))
            {
                return File(path, "application/pdf", path.Substring(path.LastIndexOf('\\') + 1));
            }
            return View();
        }

        private string SaveCollegeDefficiencyLetterPdf(string collegeCode, string type)
        {
            string fullPath = string.Empty;

            int Collegeid = db.jntuh_college.Where(C => C.collegeCode == collegeCode && C.isActive == true).Select(C => C.id).FirstOrDefault();
            string ECollegeid = UAAAS.Models.Utilities.EncryptString(Collegeid.ToString(), System.Web.Configuration.WebConfigurationManager.AppSettings["CryptoKey"]);
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int prAy =
                db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();

            //Set page size as A4
            var pdfDoc = new Document(PageSize.A4, 40, 40, 40, 40);

            // Create a new PdfWrite object, writing the output to a MemoryStream
            //open pdf
            var output = new MemoryStream();
            var writer = PdfWriter.GetInstance(pdfDoc, output);

            //int InspectionPhaseId = db.jntuh_inspection_phase.Where(p => p.isActive == true).Select(p => p.id).SingleOrDefault();

            string path = Server.MapPath("~/Content/PDFReports/AffiliationProceedings/2023-24/");

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            fullPath = path + collegeCode + "-" + ECollegeid + "-AffiliationProceedings.pdf";

            //var check = db.jntuh_college_news.Where(n => n.navigateURL == fullPath).ToList();
            //if (check.Count == 0 || check.Count == null)
            //{
            //    jntuh_college_news jntuh_college_news = new jntuh_college_news();
            //    jntuh_college_news.collegeId = (int)Collegeid;
            //    jntuh_college_news.title = "JNTUH- Communication of grant of affiliation for the Academic Year 2023-24-Reg.";
            //    jntuh_college_news.navigateURL = "/Content/PDFReports/AffiliationProceedings/2023-24/" + collegeCode + "-" + ECollegeid + "-AffiliationProceedings.pdf";
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
            sr = System.IO.File.OpenText(Server.MapPath("~/Content/AffiliationProceedings.html"));

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
            contents = contents.Replace("##SOCIETY_ADDRESS##", CollegeSocietyAddress);
            contents = contents.Replace("##COLLEGE_ADDRESS##", scheduleCollegeAddress);
            contents = contents.Replace("##COLLEGE_ADDRESS1##", scheduleCollegeAddress1);
            contents = contents.Replace("##LETTER_DATE##", DateTime.Now.ToString("dd-MM-yyy"));

            //var dates = db.jntuh_dates.Where(d => d.collegecode == collegeCode).Select(d => d).FirstOrDefault();
            //contents = contents.Replace("##APPLICATION_DATE##", ((DateTime)dates.applicationDate).ToString("dd-MM-yyy"));
            //contents = contents.Replace("##DEFICIENCY_REPORT_DATE##", ((DateTime)dates.deficiencyLetterDate).ToString("dd-MM-yyy"));
            //contents = contents.Replace("##HEARING_DATE##", dates.hearingDate != null ? ((DateTime)dates.hearingDate).ToString("dd-MM-yyy") : "");

            //var deficiencydates = db.jntuh_dates.Where(d => d.collegecode == collegeCode).Select(d => d).FirstOrDefault();
            var deficiencydates = db.jntuh_college_news.Where(d => d.collegeId == collegeid && d.title == "JNTUH - Deficiency Report for the Academic Year 2019-20-Reg.").Select(d => d).FirstOrDefault();
            var dates = db.jntuh_college_edit_status.Where(d => d.collegeId == collegeid && d.academicyearId == prAy && d.IsCollegeEditable == false).Select(d => d).FirstOrDefault();
            var Appeal = db.jntuh_appeal_college_edit_status.Where(d => d.collegeId == collegeid && d.academicyearid == prAy && d.IsCollegeEditable == false).Select(d => d).FirstOrDefault();
            if (Appeal != null)
            {
                contents = contents.Replace("##APPLICATION_DATE##", ((DateTime)Appeal.updatedOn).ToString("dd-MM-yyy") + ",");
            }
            else
            {
                contents = contents.Replace("##APPLICATION_DATE##", "NIL");
            }

            if (dates != null)
            {
                contents = contents.Replace("##116Submitted_DATE##", ((DateTime)dates.updatedOn).ToString("dd-MM-yyy") + ",");
            }
            else
            {
            }

            if (deficiencydates != null)
            {
                contents = contents.Replace("##DEFICIENCY_REPORT_DATE##", "");
            }
            else
            {
                contents = contents.Replace("##DEFICIENCY_REPORT_DATE##", "NIL");
            }

            string AppealSubmittedMessage = "Pursuant to the communication of deficiencies you have filed an appeal for reconsideration and the University reviewed the same or re inspection was conducted. Based on the above the University has accorded affiliation to the following courses.";
            string AppealNotSubmittedMessage = "Based on the above the University has accorded affiliation to the following courses.";
            contents = contents.Replace("##HEARING_DATE##", Appeal != null ? ((DateTime)Appeal.updatedOn).ToString("dd-MM-yyy") : "NIL");
            if (Appeal != null)
                contents = contents.Replace("##College_Message##", AppealSubmittedMessage);
            else
                contents = contents.Replace("##College_Message##", AppealNotSubmittedMessage);


            if (type == "All")
            {
                contents = contents.Replace("##COURSE_TABLE##", CollegeCoursesAll(collegeid));
            }
            //else if (type == "AllClear")
            //{
            //    string[] courses = CollegeCoursesAllClear(collegeid).Split('$');
            //    string defiencyRows = string.Empty;
            //    var pharmacyids = new[] { 6, 24, 27, 30, 34, 44, 47, 52, 54, 55, 58, 60, 65, 78, 90, 95, 97, 104, 105, 107, 110, 114, 117, 118, 120, 127, 135, 136, 139, 146, 150, 159, 169, 202, 204, 206, 213, 219, 234, 237, 253, 262, 263, 267, 283, 290, 295, 297, 298, 301, 302, 303, 313, 314, 315, 317, 318, 319, 320, 348, 353, 370, 376, 379, 384, 389, 392, 395, 410, 428, 436, 442, 445 };
            //    if (pharmacyids.Contains(collegeid))
            //    {
            //        contents = contents.Replace("##COURSE_TABLE##", courses[0]);
            //        contents = contents.Replace("##ASSESSMNET_TABLE##", courses[1]);
            //        contents = contents.Replace("##TwentyFive_TABLE##", string.Empty);
            //        contents = contents.Replace("##SI_TABLE##", string.Empty);
            //        defiencyRows = courses[2];
            //    }
            //    else
            //    {
            //        contents = contents.Replace("##COURSE_TABLE##", courses[0]);
            //        contents = contents.Replace("##ASSESSMNET_TABLE##", courses[1]);
            //        contents = contents.Replace("##TwentyFive_TABLE##", courses[2]);
            //        defiencyRows = courses[4];
            //    }

            //    if (defiencyRows != "0")
            //    {
            //        contents = contents.Replace("##HIDE_START##", "<!--");
            //        contents = contents.Replace("##HIDE_END##", "-->");
            //    }
            //    else
            //    {
            //        //contents = contents.Replace("##HIDE_START##", "<table border='1' style=''width:100%;text-align:center;padding-top:20px'><tr><td style='text-align:center'>NIL</td></tr></table>");
            //        //contents = contents.Replace("##HIDE_START##", string.Empty);
            //    }
            //}
            //
            //var pharmacyids1 = new[] { 6, 24, 27, 30, 34, 44, 47, 52, 54, 55, 58, 60, 65, 78, 90, 95, 97, 104, 105, 107, 110, 114, 117, 118, 120, 127, 135, 136, 139, 146, 150, 159, 169, 202, 204, 206, 213, 219, 234, 237, 253, 262, 263, 267, 283, 290, 295, 297, 298, 301, 302, 303, 313, 314, 315, 317, 318, 319, 320, 348, 353, 370, 376, 379, 384, 389, 392, 395, 410, 428, 436, 442, 445 };
            //if (pharmacyids1.Contains(collegeid))
            //{
            //    contents = contents.Replace("##PHDSHOEARTAGE##", string.Empty);
            //    var PharmacyPrincipalDefcollegeids = new[] { 54, 58, 120, 253, 263, 370 };
            //    if (PharmacyPrincipalDefcollegeids.Contains(collegeid))
            //        contents = contents.Replace("##PRINCIPALDEF##", "# Due to non-availability of Principal for the majority period during the Academic year 2018-19, the intake is reduced from 100 to 90 or 60 to 50 accordingly.");
            //    else
            //        contents = contents.Replace("##PRINCIPALDEF##", string.Empty);
            //    contents = contents.Replace("##THREESTARCON##", string.Empty);
            //    contents = contents.Replace("##THREEHASCON##", string.Empty);
            //    contents = contents.Replace("##END##", "<br/><b>R</b>- Required Faculty,<b>A</b>-Available Faculty,<b>R.Ph.D</b>-Required Ph.D Faculty,<b>A.Ph.D</b>-Available Ph.D Faculty. <br/>* Any data discrepancies may be brought to the notice of the University within two days.<br/> The faculty requirement for Pharm.D / Pharm.D(PB) is included in B.Pharmacy course.");
            //    contents = contents.Replace("##ENDTWO##", string.Empty);
            //    contents = contents.Replace("##HASCOURSESNOTE##", string.Empty);
            //    //contents = contents.Replace("##DOLLERCOURSESNOTE##",string.Empty);

            //}
            //else
            //{
            //    List<jntuh_college_intake_existing> jntuh_college =
            //        db.jntuh_college_intake_existing.Where(e => e.collegeId == collegeid)
            //            .Select(s => s)
            //            .ToList();
            //    var OldReduce =
            //        jntuh_college.Where(e => e.academicYearId == 12 && e.courseStatus == "OldReduce")
            //            .Select(s => s.approvedIntake)
            //            .FirstOrDefault();
            //    var NewReduce =
            //        jntuh_college.Where(e => e.academicYearId == 13 && e.courseStatus == "OldReduce")
            //            .Select(s => s.proposedIntake)
            //            .FirstOrDefault();

            //    var OldIncrease =
            //       jntuh_college.Where(e => e.academicYearId == 12 && e.courseStatus == "OldIncrease")
            //           .Select(s => s.approvedIntake)
            //           .FirstOrDefault();
            //    var NewIncrease =
            //        jntuh_college.Where(e => e.academicYearId == 13 && e.courseStatus == "OldIncrease")
            //            .Select(s => s.proposedIntake)
            //            .FirstOrDefault();

            //    var sOldReduce =
            //       jntuh_college.Where(e => e.academicYearId == 12 && e.courseStatus == "NewReduce")
            //           .Select(s => s.approvedIntake)
            //           .FirstOrDefault();
            //    var sNewReduce =
            //        jntuh_college.Where(e => e.academicYearId == 13 && e.courseStatus == "NewReduce")
            //            .Select(s => s.proposedIntake)
            //            .FirstOrDefault();

            //    var sOldIncrease =
            //       jntuh_college.Where(e => e.academicYearId == 12 && e.courseStatus == "NewIncrease")
            //           .Select(s => s.approvedIntake)
            //           .FirstOrDefault();
            //    var sNewIncrease =
            //        jntuh_college.Where(e => e.academicYearId == 13 && e.courseStatus == "NewIncrease")
            //            .Select(s => s.proposedIntake)
            //            .FirstOrDefault();

            //    contents = contents.Replace("##PHDSHOEARTAGE##", "* In case, Ph.D qualified faculty members requirement is not met for the proposed intake, then the proposed intake is reduced proportionately as per the Ph.D possessing faculty members.");
            //    contents = contents.Replace("##PRINCIPALDEF##", "<br />## Due to non- availability of Principal for the majority period during the Academic year 2018-19, the intake is reduced to 50% of the normal JNTUH sanctioned intake (subject to a maximum of 60) in the course having highest sanctioned intake and also having least admissions during previous Academic Years.");
            //    contents = contents.Replace("##HASCOURSESNOTE##", "<br /># In case, the admitted intake is less than 25% of the JNTUH sanctioned intake in any of the applied courses in all the previous three years but the admitted intake is at least 10 or above in one of the previous three years, then such courses are considered for affiliation with a minimum intake of 60, provided they meet other requirements as per norms. However, the faculty requirement is calculated based on sanctioned intake only.");
            //    if (NewReduce != 0 && OldReduce != 0)
            //    {
            //        contents = contents.Replace("##THREESTARCON##",
            //            "<br />*** The College has applied for decrease in intake from " + OldReduce + " to " +
            //            NewReduce + " in this course.");
            //        contents = contents.Replace("##THREEHASCON##",
            //            "<br />### In lieu of the decrease in intake in *** course the college has applied for increase in intake from " + OldIncrease + " to " + NewIncrease + " in this course.");
            //    }
            //    else if (sNewReduce != 0 && sOldReduce != 0)
            //    {
            //        contents = contents.Replace("##THREESTARCON##",
            //           "<br />*** The College has applied for decrease in intake from " + sOldReduce + " to " +
            //           sNewReduce + " in this course.");
            //        contents = contents.Replace("##THREEHASCON##",
            //            "<br />### In lieu of the decrease in intake in *** course the college has applied for increase in intake from " + sOldIncrease + " to " + sNewIncrease + " in this course.");
            //    }
            //    else
            //    {
            //        contents = contents.Replace("##THREESTARCON##", string.Empty);
            //        contents = contents.Replace("##THREEHASCON##", string.Empty);
            //    }
            //    //contents = contents.Replace("##DOLLERCOURSESNOTE##", "<br />$ Intake is reduced to 50% of highest of normal sanctioned course intake (subject to a maximum of 60) due to non-availability of Principal for the majority period of the academic year 2017-18.")
            //    contents = contents.Replace("##END##", "<b>R</b>- Required Faculty,<b>A</b>-Available Faculty,<b>R.Ph.D</b>-Required Ph.D Faculty,<b>A.Ph.D</b>-Available Ph.D Faculty. <br/><br/><p><b></b> II) The following Courses have either zero admissions due to non-grant of affiliation or admitted intake is less than 25% of JNTUH sanctioned intake for the previous three academic years. Hence,<b>&lsquo;No Affiliation is Granted&rsquo;</b> for these courses for the A.Y.2019-20 as per clause: 3.30 of affiliation regulations 2019-20.<br/></p>");
            //    contents = contents.Replace("##ENDTWO##", "<br/>2. Any data discrepancies may be brought to the notice of the University within two days from the date of this letter.");
            //    //contents = contents.Replace("##ENDSI##", "<br/><p><b></b> III) Affiliation is not granted for the following courses due to no class work / acute faculty shortage / lab equipment shortage / no academic ambience during surprise inspection.<br/></p>");
            //}

            //Read string contents using stream reader and convert html to parsed conent
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


        public string CollegeCoursesAllClear(int collegeId)
        {
            string courses = string.Empty;
            string twentyfivepercent = string.Empty;
            string sicourses = string.Empty;
            string assessments = string.Empty;
            string PharmDandPB = string.Empty;
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int prAy =
                db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
            List<CollegeIntakeExisting> collegeIntakeExisting = new List<CollegeIntakeExisting>();

            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);

            var degreeDetails = (from ie in db.jntuh_college_intake_existing
                                 join js in db.jntuh_specialization on ie.specializationId equals js.id
                                 join jd in db.jntuh_department on js.departmentId equals jd.id
                                 join deg in db.jntuh_degree on jd.degreeId equals deg.id
                                 join sh in db.jntuh_shift on ie.shiftId equals sh.id
                                 where (ie.collegeId == collegeId && ie.academicYearId == prAy)
                                 select new
                                 {
                                     ie.proposedIntake,
                                     js.specializationName,
                                     deg.degree,
                                     sh.shiftName,
                                     deg.degreeDisplayOrder,
                                     js.id
                                 }).OrderBy(d => d.degreeDisplayOrder).ThenBy(d => d.specializationName).ThenBy(d => d.shiftName).ToList();


            int rowCount = 0; int rowCounttwo = 0; int sirowCount = 0; int assessmentCount = 0; int assessmentCount1 = 0;

            //foreach (var item in degreeDetails)
            //{
            //faculty
            //string[] fStatus = NoFacultyDeficiencyCourse(collegeId, item.id).Split('$');


            //string fFlag = fStatus[0];
            //string facultyShortage = fStatus[1];
            //string phd = fStatus[2];

            ////labs
            //string[] lStatus = NoLabDeficiencyCourse(collegeId, item.id).Split('$');

            //string lFlag = lStatus[0];
            //string labs = lStatus[1];
            var faculty = new List<CollegeFacultyLabs>();

            var integreatedIds = new[] { 9, 39, 42, 75, 140, 180, 332, 364, 235 };

            var pharmacyids = new[] { 6, 24, 27, 30, 34, 44, 47, 52, 54, 55, 58, 60, 65, 78, 90, 95, 97, 104, 105, 107, 110, 114, 117, 118, 120, 127, 135, 136, 139, 146, 150, 159, 169, 202, 204, 206, 213, 219, 234, 237, 252, 253, 262, 263, 267, 283, 290, 295, 297, 298, 301, 302, 303, 313, 314, 315, 317, 318, 319, 320, 348, 353, 370, 376, 379, 384, 389, 392, 395, 410, 428, 436, 442, 445, 448, 454 };

            if (pharmacyids.Contains(collegeId))
            {
                faculty = PharmacyDeficienciesInFaculty(collegeId);
            }
            else if (integreatedIds.Contains(collegeId))
            {
                var integratedpharmacyfaculty = PharmacyDeficienciesInFaculty(collegeId);
                var integratedbtechfaculty = DeficienciesInFaculty(collegeId);
                faculty = integratedbtechfaculty.Concat(integratedpharmacyfaculty).ToList();
            }
            else
            {
                faculty = DeficienciesInFaculty(collegeId);
            }
            List<CollegeFacultyLabs> labs = DeficienciesInLabsnew(collegeId);
            //DeficienciesInLabs(collegeId);

            var collegeFacultyLabs = labs.FirstOrDefault(i => i.Degree == "B.Pharmacy");
            if (collegeFacultyLabs != null)
            {
                var bphramcylabs = collegeFacultyLabs.LabsDeficiency;
                if (bphramcylabs != "NIL")
                {
                    foreach (var c in labs.Where(i => i.Degree == "M.Pharmacy").ToList())
                    {
                        c.LabsDeficiency = "YES";
                    }
                }
            }

            foreach (var l in labs)
            {
                if (l.Degree == "B.Tech" && l.LabsDeficiency != "NIL")
                {
                    labs.Where(i => i.Department == l.Department && i.Degree == "M.Tech" && i.LabsDeficiency == "NIL").ToList().ForEach(c => c.LabsDeficiency = "YES");
                }

            }



            List<CollegeFacultyLabs> clearedCourses = new List<CollegeFacultyLabs>();
            List<CollegeFacultyLabs> deficiencyCourses = new List<CollegeFacultyLabs>();
            List<string> deficiencyDepartments = new List<string>();
            List<string> deficiencyDepartments1 = new List<string>();

            #region Pharmacy Affliation Letter Start

            if (pharmacyids.Contains(collegeId))
            {

                courses += "<table border='1' cellpadding='3' cellspacing='0' width='100%' style='font-family: Times New Roman; font-size: 11px; font-weight: normal; background-color: #fff; margin: 0 auto;'>";
                courses += "<tr>";
                courses += "<th colspan='1' align='center'><b>S.No</b></th>";
                courses += "<th colspan='10'><b>Name of the Course</b></th>";
                courses += "<th colspan='2' align='center'><b>Intake</b></th>";
                courses += "</tr>";

                PharmDandPB += "<br/><table border='1' cellpadding='3' cellspacing='0' width='100%' style='font-family: Times New Roman; font-size: 9px; font-weight: normal; background-color: #fff; margin: 0 auto;'>";
                PharmDandPB += "<tr>";
                PharmDandPB += "<th colspan='1' align='center'><b>S.No</b></th>";
                PharmDandPB += "<th colspan='2' align='center'><b> Course</b></th>";
                PharmDandPB += "<th colspan='2' align='center'><b>Deficiency</b></th>";
                PharmDandPB += "</tr>";

                assessments += "<table border='1' cellpadding='3' cellspacing='0' width='100%' style='font-family: Times New Roman; font-size: 9px; font-weight: normal; background-color: #fff; margin: 0 auto;'>";
                assessments += "<tr>";
                assessments += "<th rowspan='2' colspan='1' align='center'><b>S.No</b></th>";
                assessments += "<th rowspan='2' colspan='5' align='center'><b>Course</b></th>";
                assessments += "<th rowspan='2' colspan='2' align='center'><b>Intake</b></th>";
                assessments += "<th colspan='6' align='center'><b>Faculty Shortage</b></th>";
                assessments += "<th colspan='5' align='center'><b>Lab Shortage</b></th>";
                assessments += "</tr>";
                assessments += "<tr>";
                assessments += "<th colspan='1' align='center'><b>R</b></th>";
                assessments += "<th colspan='1' align='center'><b>A</b></th>";
                assessments += "<th colspan='2' align='center'><b>R (Ph.D)</b></th>";
                assessments += "<th colspan='2' align='center'><b>A (Ph.D)</b></th>";
                assessments += "<th colspan='5' align='center'><b>Name of the Lab(s)</b></th>";
                assessments += "</tr>";
                //Only Btech Affiliation Letters Filtering
                //faculty = faculty.Where(e => e.Degree == "B.Pharmacy" || e.Degree == "Pharm.D" || e.Degree == "Pharm.D PB")
                //    .Select(e => e)
                //    .ToList();
                labs =
                    labs.Where(e => e.Degree == "M.Pharmacy" || e.Degree == "B.Pharmacy" || e.Degree == "Pharm.D" || e.Degree == "Pharm.D PB")
                        .Select(e => e)
                        .ToList();

                faculty = faculty.Select(e => e).ToList();
                labs = labs.Select(e => e).ToList();

                //.Where(a => a.f.Deficiency == "NO" && a.f.PhdDeficiency != "YES" && a.l.LabsDeficiency == "NIL")

                int[] FSpecIds = faculty.Select(F => F.SpecializationId).ToArray();
                int[] LSpecIds = labs.Select(F => F.SpecializationId).ToArray();

                foreach (int id in FSpecIds)
                {
                    if (!LSpecIds.Contains(id))
                    {
                        CollegeFacultyLabs lab = new CollegeFacultyLabs();
                        lab.SpecializationId = id;
                        lab.Deficiency = "NIL";
                        lab.LabsDeficiency = "NIL";
                        labs.Add(lab);
                    }

                }


                clearedCourses = faculty.Join(labs, f => f.SpecializationId, l => l.SpecializationId, (f, l) => new { f, l })
                        .Where(a => a.f.Deficiency == "NO" && a.f.PhdDeficiency != "YES" && a.l.LabsDeficiency == "NIL")
                        .Select(a => new CollegeFacultyLabs
                        {
                            Degree = a.f.Degree,
                            Department = a.f.Department,
                            SpecializationId = a.f.SpecializationId,
                            Specialization = a.f.Specialization,
                            TotalIntake = a.f.TotalIntake,
                            Required = a.f.Required,
                            Available = a.f.Available,
                            isintakechange = a.l.isintakechange,
                            Deficiency = a.f.Deficiency,
                            Requiredphd = a.f.Requiredphd,
                            Availablephd = a.f.Availablephd,
                            PhdDeficiency = a.f.PhdDeficiency,
                            LabsDeficiency = a.l.LabsDeficiency
                        })
                        .OrderBy(a => a.DegreeDisplayOrder).ThenBy(a => a.Department).ThenBy(a => a.Specialization)
                        .ToList();

                deficiencyCourses = faculty.Join(labs, f => f.SpecializationId, l => l.SpecializationId, (f, l) => new { f, l })
                        .Where(a => (a.f.Deficiency == "YES" || a.f.PhdDeficiency == "YES") || a.l.LabsDeficiency != "NIL")
                        .Select(a => new CollegeFacultyLabs
                        {
                            Degree = a.f.Degree,
                            Department = a.f.Department,
                            SpecializationId = a.f.SpecializationId,
                            Specialization = a.f.Specialization,
                            TotalIntake = a.f.TotalIntake,
                            Required = a.f.Required,
                            Available = a.f.Available,
                            Requiredphd = a.f.Requiredphd,
                            Availablephd = a.f.Availablephd,
                            Deficiency = a.f.Deficiency,
                            isintakechange = a.l.isintakechange,
                            PhdDeficiency = a.f.PhdDeficiency,
                            LabsDeficiency = a.l.LabsDeficiency
                        })
                        .OrderBy(a => a.DegreeDisplayOrder).ThenBy(a => a.Department).ThenBy(a => a.Specialization)
                        .ToList();

                //if (collegeId == 319)
                //{
                //    deficiencyCourses.Add(clearedCourses.Where(e => e.Degree == "Pharm.D").Select(e => e).FirstOrDefault());
                //    deficiencyCourses.Add(clearedCourses.Where(e => e.Degree == "Pharm.D PB").Select(e => e).FirstOrDefault());
                //    clearedCourses = clearedCourses.Where(e => e.Degree == "B.Pharmacy").Select(e => e).ToList();
                //}


                foreach (var course in deficiencyCourses.OrderBy(a => a.Degree).ToList())
                {
                    if (!deficiencyDepartments.Contains(course.Department))
                    {
                        deficiencyDepartments.Add(course.Department);
                    }

                    assessments += "<tr>";
                    assessments += "<td colspan='1' align='center'>" + (assessmentCount + 1) + "</td>";

                    course.Shift = "1";
                    if (course.Shift == "1")
                    {
                        assessments += "<td colspan='5' >" + course.Degree + " - " + course.Specialization + "</td>";
                    }
                    int intake2 = 0;
                    if (course.Degree == "B.Pharmacy")
                        intake2 = course.TotalIntake >= 100 ? 100 : course.TotalIntake;
                    else if (course.Degree == "M.Pharmacy")
                        intake2 = 15;
                    else
                        intake2 = course.TotalIntake;
                    assessments += "<td colspan='2' align='center'>" + intake2 + "</td>";

                    assessments += "<td colspan='1' align='center'>" + course.Required + "</td>";
                    if (course.Degree == "B.Pharmacy")
                    {
                        int Required = course.Required;
                        int Shortage = Required - course.Available;
                        assessments += "<td colspan='1' align='center'>" + course.Available + "</td>";
                        assessments += "<td colspan='2' align='center'>0</td>";
                        assessments += "<td colspan='2' align='center'>0</td>";
                    }

                    else
                    {
                        assessments += "<td colspan='1' align='center'>" + course.Available + "</td>";
                        assessments += "<td colspan='2' align='center'>" + course.Requiredphd + "</td>";
                        assessments += "<td colspan='2' align='center'>" + course.Availablephd + "</td>";
                    }


                    assessments += "<td colspan='5' align='center'>" + (course.LabsDeficiency.Equals("No Equipement Uploaded") ? course.Specialization + " Lab Deficiency" : course.LabsDeficiency) + "</td>";
                    assessments += "</tr>";

                    assessmentCount++;

                }


                foreach (var course in clearedCourses.OrderBy(a => a.Degree).ToList())
                {
                    string degreeType = db.jntuh_degree.Join(db.jntuh_degree_type, d => d.degreeTypeId, t => t.id, (d, t) => new { d, t })
                                                       .Where(a => a.d.degree == course.Degree).Select(a => a.t.degreeType).FirstOrDefault();

                    List<string> clearedDepartments = clearedCourses.Select(a => a.Department).ToList(); //.Where(a => a.DegreeType == "UG")

                    if (deficiencyDepartments.Contains(course.Department) && !clearedDepartments.Contains(course.Department))
                    {
                        assessments += "<tr>";
                        assessments += "<td colspan='1' align='center'>" + (assessmentCount + 1) + "</td>";

                        course.Shift = "1";
                        if (course.Shift == "1")
                        {
                            assessments += "<td colspan='5' >" + course.Degree + " - " + course.Specialization + "</td>";
                        }
                        int intake3 = 0;
                        if (course.Degree == "B.Pharmacy")
                            intake3 = course.TotalIntake >= 100 ? 100 : course.TotalIntake;
                        else if (course.Degree == "M.Pharmacy")
                            intake3 = 15;
                        else
                            intake3 = course.TotalIntake;
                        assessments += "<td colspan='2' align='center'>" + intake3 + "</td>";
                        assessments += "<td colspan='1' align='center'>" + course.Required + "</td>";



                        int Required = course.Required;
                        int Shortage = Required - course.Available;
                        assessments += "<td colspan='1' align='center'>" + course.Available + "</td>";
                        assessments += "<td colspan='2' align='center'>" + course.Requiredphd + "</td>";
                        assessments += "<td colspan='2' align='center'>" + course.Availablephd + "</td>";

                        assessments += "<td colspan='5' align='center'>" + (course.LabsDeficiency.Equals("No Equipement Uploaded") ? course.Specialization + " Lab Deficiency" : course.LabsDeficiency) + "</td>";
                        assessments += "</tr>";

                        assessmentCount++;


                    }
                    else
                    {
                        if (course.TotalIntake != 0)
                        {
                            courses += "<tr>";
                            courses += "<td colspan='1' align='center'>" + (rowCount + 1) + ".</td>";
                            course.Shift = "1";

                            if (course.Shift == "1")
                            {
                                courses += "<td colspan='10'>" + course.Degree + " - " + course.Specialization + "</td>";
                            }
                            if (course.Degree == "M.Pharmacy")
                                courses += "<td colspan='2' align='center'>15</td>";
                            else if (course.Degree == "B.Pharmacy")
                            {
                                int PharmacyIntake = (int)course.TotalIntake >= 100 ? 100 : (int)course.TotalIntake;
                                var PharmacyPrincipalDefcollegeIds = new[] { 54, 58, 120, 253, 263, 370 };
                                if (PharmacyPrincipalDefcollegeIds.Contains(collegeId))
                                    courses += "<td colspan='2' align='center'>" + PharmacyIntake + " #" + "</td>";
                                else
                                    courses += "<td colspan='2' align='center'>" + PharmacyIntake + "</td>";
                            }
                            else
                                courses += "<td colspan='2' align='center'>" + course.TotalIntake + " </td>";

                            courses += "</tr>";
                            rowCount++;
                        }
                    }
                }
                int totalCleared = clearedCourses.Count();
                int totalZeroIntake = clearedCourses.Where(c => c.TotalIntake == 0).Count();

                if (clearedCourses.Count() == 0 || totalCleared == totalZeroIntake)
                {
                    courses += "<tr>";
                    courses += "<td colspan='13' align='center'>NIL</td>";
                    courses += "</tr>";
                }
                //This Code is Written by 11
                if (deficiencyCourses.Count() == 0)
                {
                    assessments += "<tr>";
                    assessments += "<td colspan='24' align='center'>NIL</td>";
                    assessments += "</tr>";
                }
                assessments += "</table>" + "</br>";
                courses += "</table>";
                PharmDandPB += "</table>";


                if (deficiencyDepartments.Contains("B.Pharmacy") || deficiencyDepartments.Contains("M.Pharmacy"))
                {
                    if (deficiencyDepartments.Contains("Pharm.D") || deficiencyDepartments.Contains("Pharm.D PB"))
                    {
                        if (collegeId == 319 || collegeId == 127)
                        {

                        }
                        else
                        {
                            assessments += PharmDandPB;
                        }
                    }
                    return courses + "$" + assessments + "$" + assessmentCount;
                }
                else
                {
                    if (assessmentCount1 > 0)
                        return courses + "$" + assessments + "$" + assessmentCount1;
                    // return courses + "$" + PharmDandPB + "$" + assessmentCount1;
                    else
                    {
                        //PharmDandPB = "";
                        return courses + "$" + assessments + "$" + assessmentCount1;
                    }
                }


            }

            else
            {
                courses += "<table border='1' cellpadding='3' cellspacing='0' width='100%' style='font-family: Times New Roman; font-size: 9px; font-weight: normal; background-color: #fff; margin: 0 auto;'>";
                courses += "<tr>";
                courses += "<th colspan='1' align='center'><b>S.No</b></th>";
                courses += "<th colspan='10'><b>Name of the Course</b></th>";
                courses += "<th colspan='2' align='center'><b>Intake</b></th>";
                courses += "</tr>";

                assessments += "<table border='1' cellpadding='3' cellspacing='0' width='100%' style='font-family: Times New Roman; font-size: 9px; font-weight: normal; background-color: #fff; margin: 0 auto;'>";
                assessments += "<tr>";
                assessments += "<th rowspan='2' colspan='1' align='center'><b>S.No</b></th>";
                assessments += "<th rowspan='2' colspan='5'><b>Course</b></th>";
                assessments += "<th colspan='4' align='center'><b>Faculty Shortage </b></th>";
                assessments += "<th colspan='5' align='center'><b>Lab Shortage</b></th>";
                assessments += "</tr>";
                assessments += "<tr>";
                assessments += "<th colspan='1' align='center'><b>R</b></th>";
                assessments += "<th colspan='1' align='center'><b>A</b></th>";
                assessments += "<th colspan='1' align='center'><b>R.Ph.D</b></th>";
                assessments += "<th colspan='1' align='center'><b>A.Ph.D</b></th>";
                assessments += "<th colspan='5' align='center'><b>Name of the Lab(s)</b></th>";
                assessments += "</tr>";

                //twentyfivepercent += "<p style='font-family: Times New Roman; font-size: 9px;'><b>#</b> The following Courses have either Zero Admissions due to non-grant of Affiliation or Admitted intake is less than 25% of JNTUH Sanctioned intake for the previous three academic years.Hence SCA has recommended for <b>&lsquo;No Admission Status&rsquo;</b> for these courses for the A.Y.2019-20 as per clause: 3.30 of Affiliation Regulations 2019-20.<br/></p>";
                twentyfivepercent += "<table border='1' cellpadding='3' cellspacing='0' width='100%' style='font-family: Times New Roman; font-size: 9px; font-weight: normal; background-color: #fff; margin: 0 auto;'>";
                twentyfivepercent += "<tr>";
                twentyfivepercent += "<th colspan='1' align='center'><b>S.No</b></th>";
                twentyfivepercent += "<th colspan='10'><b>Name of the Course</b></th>";
                twentyfivepercent += "<th colspan='2' align='center'><b>Intake</b></th>";
                twentyfivepercent += "</tr>";
                //SI Courses Table
                sicourses += "<table border='1' cellpadding='3' cellspacing='0' width='100%' style='font-family: Times New Roman; font-size: 9px; font-weight: normal; background-color: #fff; margin: 0 auto;'>";
                sicourses += "<tr>";
                sicourses += "<th colspan='1' align='center'><b>S.No</b></th>";
                sicourses += "<th colspan='10'><b>Name of the Course</b></th>";
                sicourses += "<th colspan='2' align='center'><b>Proposed Intake</b></th>";
                sicourses += "</tr>";
                //Only Btech Affiliation Letters Filtering
                faculty = faculty.Where(e => e.Degree == "5-Year MBA(Integrated)" || e.Degree == "M.Tech" || e.Degree == "MBA" || e.Degree == "MCA" || e.Degree == "B.Tech" || e.Degree == "B.Pharmacy" || e.Degree == "Pharm.D" || e.Degree == "Pharm.D PB" || e.Degree == "M.Pharmacy").Select(e => e).ToList();
                labs = labs.Where(e => e.Degree == "5-Year MBA(Integrated)" || e.Degree == "M.Tech" || e.Degree == "MBA" || e.Degree == "MCA" || e.Degree == "B.Tech" || e.Degree == "B.Pharmacy" || e.Degree == "Pharm.D" || e.Degree == "Pharm.D PB" || e.Degree == "M.Pharmacy").Select(e => e).ToList();
                //Only MBA And MCA
                //faculty = faculty.Where(e => e.Degree == "MBA" || e.Degree == "MCA").Select(e => e).ToList();
                //labs = labs.Where(e => e.Degree == "MBA" || e.Degree == "MCA").Select(e => e).ToList();


                //Add New Code for Pharma.D Labs not Add in Lab Master
                int[] Humanities = new int[] { 31, 37, 42, 48, 155, 156, 157, 158 };
                int[] FSpecIds = faculty.Where(e => !Humanities.Contains(e.SpecializationId)).Select(F => F.SpecializationId).ToArray();//
                int[] LSpecIds = labs.Select(F => F.SpecializationId).ToArray();
                int[] PharmacySpecualizationIds = new int[] { 12, 13, 18, 19, 114, 115, 116, 117, 118, 119, 120, 121, 122, 123, 124, 167, 169, 170, 171, 172 };

                var pharmacycollegesids = new[]
                {
                    6, 24, 27, 30, 34, 44, 47, 52, 54, 55, 58, 60, 65, 78, 90, 95, 97, 104, 105, 107, 110, 114, 117, 118, 120, 127, 135, 136, 139, 146, 150, 159, 169, 202, 204, 206, 213, 219, 234, 237, 252, 253, 262, 263, 267, 283, 290, 295, 297, 298, 301, 302, 303, 313, 314, 315, 317, 318, 319, 320, 348, 353, 370, 376, 379, 384, 389, 392, 395, 410, 428, 436, 442, 445, 448, 454
                };
                //Some Colleges only Put Lab Deficiency NIL ,ex.College Uploaded New Courses.  B.Tech and B.pharmacy

                //if (pharmacycollegesids.Contains(collegeId))
                //{
                //    foreach (int id in FSpecIds)
                //    {
                //        //if (PharmacySpecualizationIds.Contains(id))
                //        // {
                //        if (!LSpecIds.Contains(id))
                //        {
                //            CollegeFacultyLabs lab = new CollegeFacultyLabs();
                //            lab.SpecializationId = id;
                //            lab.Deficiency = "NIL";
                //            lab.LabsDeficiency = "NIL";
                //            labs.Add(lab);
                //        }
                //        // }
                //    }
                //}
                if (integreatedIds.Contains(collegeId))
                {
                    foreach (int id in FSpecIds)
                    {
                        if (PharmacySpecualizationIds.Contains(id))
                        {
                            if (!LSpecIds.Contains(id))
                            {
                                CollegeFacultyLabs lab = new CollegeFacultyLabs();
                                lab.SpecializationId = id;
                                lab.Deficiency = "NIL";
                                lab.LabsDeficiency = "NIL";
                                labs.Add(lab);
                            }
                        }
                    }
                }

                //MBA Lab Deficiency NIL 


                //foreach (int id in FSpecIds)
                //{
                //   if (id==14)
                //    {
                //    if (!LSpecIds.Contains(id))
                //    {
                //        CollegeFacultyLabs lab = new CollegeFacultyLabs();
                //        lab.SpecializationId = id;
                //        lab.Deficiency = "NIL";
                //        lab.LabsDeficiency = "NIL";
                //        labs.Add(lab);
                //    }
                //    else
                //    {
                //        labs.Where(e=>e.SpecializationId==14).ToList().ForEach(e=>e.LabsDeficiency="NIL");
                //    }
                //    }
                //}


                clearedCourses = faculty.Join(labs, f => f.SpecializationId, l => l.SpecializationId, (f, l) => new { f, l })
                                        .Where(a => a.f.ispercentage == false && a.f.Deficiency == "NO" && a.f.PhdDeficiency != "YES" && a.l.LabsDeficiency == "NIL")
                                        .Select(a => new CollegeFacultyLabs
                                        {
                                            Degree = a.f.Degree,
                                            Department = a.f.Department,
                                            SpecializationId = a.f.SpecializationId,
                                            Specialization = a.f.Specialization,
                                            TotalIntake = a.f.TotalIntake,
                                            Required = a.f.Required,
                                            Available = a.f.Available,
                                            Deficiency = a.f.Deficiency,
                                            Requiredphd = a.f.Requiredphd,
                                            Availablephd = a.f.Availablephd,
                                            ShiftId = a.f.ShiftId,
                                            PhdDeficiency = a.f.PhdDeficiency,
                                            LabsDeficiency = a.l.LabsDeficiency,
                                            isintakechange = a.f.isintakechange,
                                            ispercentage = a.f.ispercentage,
                                            DollerCourseIntake = a.f.DollerCourseIntake,
                                            ishashcourses = a.f.ishashcourses,
                                            isnewincreasecourse = a.f.isnewincreasecourse,
                                            isnewreducecourse = a.f.isnewreducecourse,
                                            isoldincreasecourse = a.f.isoldincreasecourse,
                                            isoldreducecourse = a.f.isoldreducecourse
                                        })
                                        .OrderBy(a => a.DegreeDisplayOrder).ThenBy(a => a.Department).ThenBy(a => a.Specialization)
                                        .ToList();

                //clearedCourses = faculty.Join(labs, f => f.SpecializationId, l => l.SpecializationId, (f, l) => new { f, l })
                //                        .Where(a => a.f.Deficiency == "NO" && a.f.PhdDeficiency != "YES" && a.l.LabsDeficiency == "NIL")
                //                        .Select(a => new CollegeFacultyLabs
                //                        {
                //                            Degree = a.f.Degree,
                //                            Department = a.f.Department,
                //                            SpecializationId = a.f.SpecializationId,
                //                            Specialization = a.f.Specialization,
                //                            TotalIntake = a.f.TotalIntake,
                //                            Required = a.f.Required,
                //                            Available = a.f.Available,
                //                            Deficiency = a.f.Deficiency,
                //                            Requiredphd = a.f.Requiredphd,
                //                            Availablephd = a.f.Availablephd,
                //                            PhdDeficiency = a.f.PhdDeficiency,
                //                            LabsDeficiency = a.l.LabsDeficiency
                //                        })
                //                        .OrderBy(a => a.DegreeDisplayOrder).ThenBy(a => a.Department).ThenBy(a => a.Specialization)
                //                        .ToList();

                deficiencyCourses = faculty.Join(labs, f => f.SpecializationId, l => l.SpecializationId, (f, l) => new { f, l })
                                            .Where(a => (a.f.ispercentage == true || a.f.Deficiency == "YES" || a.f.PhdDeficiency == "YES") || a.l.LabsDeficiency != "NIL")
                                            .Select(a => new CollegeFacultyLabs
                                            {
                                                Degree = a.f.Degree,
                                                Department = a.f.Department,
                                                SpecializationId = a.f.SpecializationId,
                                                Specialization = a.f.Specialization,
                                                TotalIntake = a.f.TotalIntake,
                                                Required = a.f.Required,
                                                Available = a.f.Available,
                                                Requiredphd = a.f.Requiredphd,
                                                Availablephd = a.f.Availablephd,
                                                Deficiency = a.f.Deficiency,
                                                ShiftId = a.f.ShiftId,
                                                DollerCourseIntake = a.f.DollerCourseIntake,
                                                PhdDeficiency = a.f.PhdDeficiency,
                                                LabsDeficiency = a.l.LabsDeficiency,
                                                isintakechange = a.f.isintakechange,
                                                ispercentage = a.f.ispercentage,
                                                ishashcourses = a.f.ishashcourses,
                                                isnewincreasecourse = a.f.isnewincreasecourse,
                                                isnewreducecourse = a.f.isnewreducecourse,
                                                isoldincreasecourse = a.f.isoldincreasecourse,
                                                isoldreducecourse = a.f.isoldreducecourse
                                            })
                                            .OrderBy(a => a.DegreeDisplayOrder).ThenBy(a => a.Department).ThenBy(a => a.Specialization)
                                            .ToList();




                foreach (var course in deficiencyCourses.Where(d => d.ispercentage != true).OrderBy(a => a.Degree).ToList())
                {
                    if (!deficiencyDepartments.Contains(course.Department))
                    {
                        deficiencyDepartments.Add(course.Department);
                    }

                    assessments += "<tr>";
                    assessments += "<td colspan='1' align='center'>" + (assessmentCount + 1) + "</td>";

                    //course.Shift = "1";
                    if (course.ShiftId == 1)
                    {
                        assessments += "<td colspan='5' >" + course.Degree + " - " + course.Specialization + "</td>";
                    }
                    if (course.ShiftId == 2)
                    {
                        assessments += "<td colspan='5' >" + course.Degree + " - " + course.Specialization + " - 2</td>";
                    }

                    //assessments += "<td colspan='2' align='center'>" + (course.Required - course.Available) + "</td>";
                    assessments += "<td colspan='1' align='center'>" + course.Required + "</td>";
                    assessments += "<td colspan='1' align='center'>" + course.Available + "</td>";
                    //assessments += "<td colspan='2' align='center'>" + course.PhdDeficiency + "</td>";
                    assessments += "<td colspan='1' align='center'>" + course.Requiredphd + "</td>";
                    assessments += "<td colspan='1' align='center'>" + course.Availablephd + "</td>";
                    assessments += "<td colspan='5' align='center'>" + (course.LabsDeficiency.Equals("No Equipement Uploaded") ? course.Specialization + " Lab Deficiency" : course.LabsDeficiency) + "</td>";
                    assessments += "</tr>";

                    assessmentCount++;

                    //old Code Commented By Narayan 
                    //var secondshift = db.jntuh_college_intake_existing.Where(e => e.collegeId == collegeId && e.specializationId == course.SpecializationId && e.shiftId == 2).Select(e => e).FirstOrDefault();

                    //if (secondshift != null)
                    //{
                    //    assessments += "<tr>";
                    //    assessments += "<td colspan='1' align='center'>" + (assessmentCount + 1) + "</td>";

                    //    course.Shift = "2";
                    //    if (course.Shift == "2")
                    //    {
                    //        assessments += "<td colspan='5' >" + course.Degree + " - " + course.Specialization + " - 2</td>";
                    //    }

                    //    //assessments += "<td colspan='2' align='center'>" + (course.Required - course.Available) + "</td>";
                    //    assessments += "<td colspan='1' align='center'>" + course.Required + "</td>";
                    //    assessments += "<td colspan='1' align='center'>" + course.Available + "</td>";
                    //    //assessments += "<td colspan='2' align='center'>" + course.PhdDeficiency + "</td>";
                    //    assessments += "<td colspan='1' align='center'>" + course.Requiredphd + "</td>";
                    //    assessments += "<td colspan='1' align='center'>" + course.Availablephd + "</td>";
                    //    assessments += "<td colspan='5' align='center'>" + (course.LabsDeficiency.Equals("No Equipement Uploaded") ? course.Specialization + " Lab Deficiency" : course.LabsDeficiency) + "</td>";
                    //    assessments += "</tr>";

                    //    assessmentCount++;
                    //}

                    //}
                }

                foreach (var course in clearedCourses.Where(a => a.DollerCourseIntake != 1).OrderBy(a => a.Degree).ToList())
                {
                    string degreeType = db.jntuh_degree.Join(db.jntuh_degree_type, d => d.degreeTypeId, t => t.id, (d, t) => new { d, t })
                                                       .Where(a => a.d.degree == course.Degree).Select(a => a.t.degreeType).FirstOrDefault();

                    List<string> clearedDepartments = clearedCourses.Select(a => a.Department).ToList(); //.Where(a => a.DegreeType == "UG")

                    if (deficiencyDepartments.Contains(course.Department) && !clearedDepartments.Contains(course.Department))
                    {
                        assessments += "<tr>";
                        assessments += "<td colspan='1' align='center'>" + (assessmentCount + 1) + "</td>";

                        //course.Shift = "1";
                        if (course.ShiftId == 1)
                        {
                            assessments += "<td colspan='5' >" + course.Degree + " - " + course.Specialization + "</td>";
                        }
                        else
                        {
                            assessments += "<td colspan='5' >" + course.Degree + " - " + course.Specialization + " - 2</td>";
                        }

                        //assessments += "<td colspan='2' align='center'>" + (course.Required - course.Available) + "</td>";
                        assessments += "<td colspan='1' align='center'>" + course.Required + "</td>";
                        assessments += "<td colspan='1' align='center'>" + course.Available + "</td>";
                        //assessments += "<td colspan='2' align='center'>" + course.PhdDeficiency + "</td>";
                        assessments += "<td colspan='1' align='center'>" + course.Requiredphd + "</td>";
                        assessments += "<td colspan='1' align='center'>" + course.Availablephd + "</td>";
                        assessments += "<td colspan='5' align='center'>" + (course.LabsDeficiency.Equals("No Equipement Uploaded") ? course.Specialization + " Lab Deficiency" : course.LabsDeficiency) + "</td>";
                        assessments += "</tr>";

                        assessmentCount++;

                        //var secondshift = db.jntuh_college_intake_existing.Where(e => e.collegeId == collegeId && e.specializationId == course.SpecializationId && e.shiftId == 2).Select(e => e).FirstOrDefault();

                        //if (secondshift != null)
                        //{
                        //    assessments += "<tr>";
                        //    assessments += "<td colspan='1' align='center'>" + (assessmentCount + 1) + "</td>";

                        //    course.Shift = "2";
                        //    if (course.Shift == "2")
                        //    {
                        //        assessments += "<td colspan='5' >" + course.Degree + " - " + course.Specialization + " - 2</td>";
                        //    }

                        //    //assessments += "<td colspan='2' align='center'>" + (course.Required - course.Available) + "</td>";
                        //    assessments += "<td colspan='1' align='center'>" + course.Required + "</td>";
                        //    assessments += "<td colspan='1' align='center'>" + course.Available + "</td>";
                        //    //assessments += "<td colspan='2' align='center'>" + course.PhdDeficiency + "</td>";
                        //    assessments += "<td colspan='1' align='center'>" + course.Requiredphd + "</td>";
                        //    assessments += "<td colspan='1' align='center'>" + course.Availablephd + "</td>";
                        //    assessments += "<td colspan='5' align='center'>" + (course.LabsDeficiency.Equals("No Equipement Uploaded") ? course.Specialization + " Lab Deficiency" : course.LabsDeficiency) + "</td>";
                        //    assessments += "</tr>";

                        //    assessmentCount++;
                        //}

                    }
                    else
                    {
                        if (course.TotalIntake != 0)
                        {
                            courses += "<tr>";
                            courses += "<td colspan='1' align='center'>" + (rowCount + 1) + ".</td>";
                            //course.Shift = "1";

                            if (course.ShiftId == 1)
                            {
                                courses += "<td colspan='10'>" + course.Degree + " - " + course.Specialization + "</td>";
                            }
                            else if (course.ShiftId == 2)
                            {
                                courses += "<td colspan='10'>" + course.Degree + " - " + course.Specialization +
                                           " - 2</td>";
                            }
                            else
                            {
                                courses += "<td colspan='10'>" + course.Degree + " - " + course.Specialization + "</td>";
                            }
                            if (course.Degree == "B.Pharmacy")
                            {
                                int intake1 = course.TotalIntake >= 100 ? 100 : (int)course.TotalIntake;
                                courses += "<td colspan='2' align='center'>" + intake1 + "</td>";
                            }
                            else if (course.Degree == "M.Pharmacy")
                                courses += "<td colspan='2' align='center'>15</td>";
                            else
                            {
                                if (course.isnewincreasecourse == true || course.isoldincreasecourse == true)
                                {
                                    courses += "<td colspan='2' align='center'>" + course.TotalIntake + " ###" + "</td>";
                                }
                                else if (course.isoldreducecourse == true || course.isnewreducecourse == true)
                                {
                                    courses += "<td colspan='2' align='center'>" + course.TotalIntake + " ***" + "</td>";
                                }
                                else if (course.ishashcourses == true)
                                {
                                    if (course.DollerCourseIntake != 0)
                                    {
                                        courses += "<td colspan='2' align='center'>" + course.DollerCourseIntake + " #" +
                                                   " ##" +
                                                   "</td>";
                                    }
                                    else
                                    {
                                        if (course.TotalIntake > 60)
                                        {
                                            courses += "<td colspan='2' align='center'>" + "60" + " #" + "</td>";
                                        }
                                        else
                                        {
                                            courses += "<td colspan='2' align='center'>" + course.TotalIntake + " #" + "</td>";
                                        }
                                    }


                                }
                                else if (course.isintakechange == true)
                                {
                                    if (course.DollerCourseIntake != 0)
                                    {
                                        courses += "<td colspan='2' align='center'>" + course.DollerCourseIntake + " *" + " ##" +
                                                   "</td>";
                                    }
                                    else
                                    {
                                        courses += "<td colspan='2' align='center'>" + course.TotalIntake + " *" + "</td>";
                                    }

                                }
                                //else if (course.DollerCourseIntake != 0)
                                //{
                                //    courses += "<td colspan='2' align='center'>" + course.DollerCourseIntake + "$" + "</td>";
                                //}
                                else
                                {
                                    if (course.DollerCourseIntake != 0)
                                    {
                                        courses += "<td colspan='2' align='center'>" + course.DollerCourseIntake + " ##" + "</td>";
                                    }
                                    else
                                    {
                                        courses += "<td colspan='2' align='center'>" + course.TotalIntake + "</td>";
                                    }

                                }
                            }
                            courses += "</tr>";

                            rowCount++;


                            //var secondshift = db.jntuh_college_intake_existing.Where(e => e.collegeId == collegeId && e.specializationId == course.SpecializationId && e.shiftId == 2).Select(e => e).FirstOrDefault();

                            //if (secondshift != null)
                            //{
                            //    int presentYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
                            //    int AY1 = db.jntuh_academic_year.Where(a => a.actualYear == (presentYear + 1)).Select(a => a.id).FirstOrDefault();
                            //    int DegreeId = db.jntuh_degree.Where(e => e.degree == course.Degree.Trim()).Select(e => e.id).FirstOrDefault();
                            //    int approvedIntake = GetIntake(collegeId, AY1, course.SpecializationId, 2, 1, DegreeId);

                            //    if (approvedIntake != 0)
                            //    {
                            //        courses += "<tr>";
                            //        courses += "<td colspan='1' align='center'>" + (rowCount + 1) + ".</td>";
                            //        course.Shift = "2";

                            //        if (course.Shift == "2")
                            //        {
                            //            courses += "<td colspan='10'>" + course.Degree + " - " + course.Specialization + " - 2</td>";
                            //        }

                            //        courses += "<td colspan='2' align='center'>" + GetIntake(collegeId, AY1, course.SpecializationId, 2, 1, DegreeId) + "</td>";
                            //        courses += "</tr>";

                            //        rowCount++;
                            //    }
                            //}
                        }
                    }
                }
                foreach (var coursetwo in deficiencyCourses.Where(w => w.ispercentage == true).OrderBy(a => a.Degree).ToList())
                {
                    twentyfivepercent += "<tr>";
                    twentyfivepercent += "<td colspan='1' align='center'>" + (rowCounttwo + 1) + ".</td>";
                    //course.Shift = "1";

                    if (coursetwo.ShiftId == 1)
                    {
                        twentyfivepercent += "<td colspan='10'>" + coursetwo.Degree + " - " + coursetwo.Specialization + "</td>";
                    }
                    else if (coursetwo.ShiftId == 2)
                    {
                        twentyfivepercent += "<td colspan='10'>" + coursetwo.Degree + " - " + coursetwo.Specialization +
                                   " - 2</td>";
                    }
                    else
                    {
                        twentyfivepercent += "<td colspan='10'>" + coursetwo.Degree + " - " + coursetwo.Specialization + "</td>";
                    }
                    twentyfivepercent += "<td colspan='2' align='center'>" + coursetwo.TotalIntake + "</td>";
                    twentyfivepercent += "</tr>";
                    rowCounttwo++;

                }
                foreach (var sicourseslist in clearedCourses.Where(w => w.DollerCourseIntake == 1).OrderBy(a => a.Degree).ToList())
                {
                    sicourses += "<tr>";
                    sicourses += "<td colspan='1' align='center'>" + (sirowCount + 1) + ".</td>";
                    //course.Shift = "1";

                    if (sicourseslist.ShiftId == 1)
                    {
                        sicourses += "<td colspan='10'>" + sicourseslist.Degree + " - " + sicourseslist.Specialization + "</td>";
                    }
                    else if (sicourseslist.ShiftId == 2)
                    {
                        sicourses += "<td colspan='10'>" + sicourseslist.Degree + " - " + sicourseslist.Specialization +
                                   " - 2</td>";
                    }
                    else
                    {
                        sicourses += "<td colspan='10'>" + sicourseslist.Degree + " - " + sicourseslist.Specialization + "</td>";
                    }
                    sicourses += "<td colspan='2' align='center'>" + sicourseslist.TotalIntake + "</td>";
                    sicourses += "</tr>";
                    sirowCount++;

                }

                int totalCleared = clearedCourses.Count();
                int totalZeroIntake = clearedCourses.Where(c => c.TotalIntake == 0).Count();
                if (deficiencyCourses.Where(w => w.ispercentage == false).Count() == 0)
                {
                    assessments += "<tr>";
                    assessments += "<td colspan='24' align='center'>NIL</td>";
                    assessments += "</tr>";
                }
                if (deficiencyCourses.Where(w => w.ispercentage == true).Count() == 0)
                {
                    twentyfivepercent += "<tr>";
                    twentyfivepercent += "<td colspan='13' align='center'>NIL</td>";
                    twentyfivepercent += "</tr>";
                }
                if (clearedCourses.Count() == 0 || totalCleared == totalZeroIntake)
                {
                    courses += "<tr>";
                    courses += "<td colspan='13' align='center'>NIL</td>";
                    courses += "</tr>";
                }
                if (clearedCourses.Where(a => a.DollerCourseIntake == 1).Count() == 0)
                {
                    sicourses += "<tr>";
                    sicourses += "<td colspan='13' align='center'>NIL</td>";
                    sicourses += "</tr>";
                }
                assessments += "</table>";
                courses += "</table>";
                sicourses += "</table>";
                twentyfivepercent += "</table>";
                return courses + "$" + assessments + "$" + twentyfivepercent + "$" + sicourses + "$" + assessmentCount + "";


            }

            #endregion Pharmacy Affliation Letter End
        }

        public List<CollegeFacultyLabs> DeficienciesInFaculty(int? collegeID)
        {
            string faculty = string.Empty;
            int facultycount = 0;
            faculty += "<table width='100%' border='0' cellpadding='5' cellspacing='0'>";
            faculty += "<tr>";
            faculty += "<td align='left'><b><u>Deficiencies in Faculty</u></b> (Department/ Specialization Wise):";
            faculty += "</tr>";
            faculty += "</table>";
            List<CollegeFacultyWithIntakeReport> allfacultyCounts = collegeFaculty(collegeID).ToList();

            List<CollegeFacultyWithIntakeReport> facultyCounts = allfacultyCounts.Where(c => c.shiftId == 1 || c.shiftId == 2).ToList();
            List<CollegeFacultyWithIntakeReport> facultyCountsmetechsecond = allfacultyCounts.Where(c => c.shiftId == 2).Select(e => e).ToList();
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
            //Zero Proposed & and 25 % percent
            List<CollegeFacultyWithIntakeReport> facultyCountper = allfacultyCounts.Where(c => (c.ispercentage == true && c.Proposedintake != 0 && c.Degree == "B.Tech") || c.Proposedintake == 0 && c.Degree == "B.Tech").Select(e => e).ToList();
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


            string[] admittedIntakeTwoormoreZeroDeptName1 = facultyCounts.Where(e => e.Degree == "B.Tech" && ((e.approvedIntake2 == 0 && e.approvedIntake3 == 0) || (e.approvedIntake3 == 0 && e.approvedIntake4 == 0) || (e.approvedIntake2 == 0 && e.approvedIntake4 == 0)) && !departments.Contains(e.Department)).Select(e => e.Department).Distinct().ToArray();
            string[] ProposedIntakeZeroDeptName = facultyCounts.Where(e => e.Degree == "B.Tech" && (e.Proposedintake == 0) && !departments.Contains(e.Specialization)).Select(e => e.Specialization).Distinct().ToArray();
            faculty += "<table width='100%' border='1' cellpadding='3' cellspacing='0' style='border-color: #ccc;'>";
            faculty += "<tr>";
            faculty += "<th style='text-align: center; vertical-align: top;'>SNo</th>";
            faculty += "<th style='text-align: left; vertical-align: top;' >Department</th>";
            faculty += "<th style='text-align: left; vertical-align: top;' >Degree</th>";
            faculty += "<th style='text-align: left; vertical-align: top;' >Specialization</th>";
            faculty += "<th style='text-align: center; vertical-align: top;' >Total Intake *</th>";
            faculty += "<th style='text-align: center; vertical-align: top;' >Specialization Wise Faculty Required</th>";
            faculty += "<th style='text-align: center; vertical-align: top;' >Specialization Wise Faculty Available</th>";
            faculty += "<th style='text-align: center; vertical-align: top;' >PG Specialization</th>";
            faculty += "<th style='text-align: center; vertical-align: top;' >Deficiency</th>";
            faculty += "<th style='text-align: center; vertical-align: top;' >Deficiency of Ph.D faculty</th>";
            faculty += "</tr>";

            var degreeTypes = db.jntuh_degree_type.Select(t => t).ToList();

            List<CollegeFacultyLabs> lstFaculty = new List<CollegeFacultyLabs>();
            int[] SpecializationIDS = db.jntuh_specialization.Where(S => S.departmentId == 43).Select(S => S.id).ToArray();
            int remainingFaculty2 = 0;
            foreach (var item in facultyCounts.Where(e => e.Proposedintake != 0 && !ProposedIntakeZeroDeptName.Contains(e.Specialization)).Select(e => e).ToList())//&& !admittedIntakeTwoormoreZeroDeptName1.Contains(e.Department)
            {
                distDeptcount = facultyCounts.Where(d => d.Department == item.Department).Distinct().Count();
                //if (item.Degree == "M.Tech" || item.Degree == "B.Tech")
                //    SpecializationwisePHDFaculty = facultyCounts.Where(D => D.Department == item.Department && D.Degree == "M.Tech").Distinct().Count();
                //else if (item.Degree == "MCA")
                //    SpecializationwisePHDFaculty = facultyCounts.Where(D => D.Department == item.Department && D.Degree == "MCA").Distinct().Count();
                //else if (item.Degree == "MBA")
                //    SpecializationwisePHDFaculty = facultyCounts.Where(D => D.Department == item.Department && D.Degree == "MBA").Distinct().Count();
                //TotalCount = facultyCounts.Where(D => D.Department == item.Department && (D.Degree == "M.Tech" || D.Degree == "B.Tech")).Distinct().Count();
                //SpecializationwisePHDFaculty = SpecializationwisePHDFaculty * 2;


                if (item.Degree == "M.Tech" || item.Degree == "MCA" || item.Degree == "MBA" || item.Degree == "5-Year MBA(Integrated)")
                {

                    if (item.Degree == "M.Tech")
                    {

                        SpecializationwisePHDFaculty = IntakeWisePhdForMtech(item.Proposedintake, item.shiftId);
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

                int indexnow = facultyCounts.IndexOf(item);


                if (indexnow > 0 && facultyCounts[indexnow].Department != facultyCounts[indexnow - 1].Department)
                {
                    deptloop = 1;
                }
                if ((item.collegeId == 72 && item.Department == "IT") || (item.collegeId == 130 && item.Department == "IT"))
                {
                    deptloop = 1;
                }
                departmentWiseRequiredFaculty = facultyCounts.Where(d => d.Department == item.Department).Select(d => d.requiredFaculty).Sum();

                string minimumRequirementMet = string.Empty;
                string PhdminimumRequirementMet = string.Empty;
                int facultyShortage = 0;
                int adjustedFaculty = 0;
                int adjustedPHDFaculty = 0;
                int tFaculty = 0;
                int othersRequiredfaculty = 0;
                if (item.Department == "MBA" || item.Department == "MCA")
                    tFaculty = (int)Math.Ceiling(Convert.ToDecimal(item.totalFaculty));//item.totalFaculty
                else
                    tFaculty = (int)Math.Ceiling(Convert.ToDecimal(item.specializationWiseFaculty));//item.totalFaculty
                int rFaculty = (int)Math.Ceiling(item.requiredFaculty);

                //if (departments.Contains(item.Department))
                //{
                //    rFaculty = (int)firstYearRequired;
                //    departmentWiseRequiredFaculty = (int)firstYearRequired;
                //}

                if (departments.Contains(item.Department))
                {
                    if (OthersSpecIds.Contains(item.specializationId))
                    {
                        rFaculty = 1;
                        othersRequiredfaculty = 1;
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
                        remainingFaculty = tFaculty - rFaculty;
                        adjustedFaculty = rFaculty;
                    }
                    else
                    {
                        minimumRequirementMet = "NO";
                        adjustedFaculty = tFaculty;
                        facultyShortage = rFaculty - tFaculty;
                        remainingFaculty = 0;
                    }

                    remainingPHDFaculty = item.phdFaculty;

                    #region old Phd Code
                    //if (remainingPHDFaculty >= 1 && (degreeType.Equals("PG")) && rFaculty <= adjustedFaculty && item.Degree != "MBA")
                    //{
                    //    adjustedPHDFaculty = 1; //remainingPHDFaculty;
                    //    remainingPHDFaculty = remainingPHDFaculty - 1;
                    //    //facultyCounts.Where(i => i.Department == item.Department).ToList().ForEach(I => I.phdFaculty = remainingPHDFaculty);
                    //}
                    //else if (remainingPHDFaculty >= 1 && (degreeType.Equals("PG")) && rFaculty <= adjustedFaculty && item.Degree == "MBA")
                    //{

                    //    if (item.totalIntake > 120)
                    //        adjustedPHDFaculty = remainingPHDFaculty;
                    //    else
                    //        adjustedPHDFaculty = 1;
                    //}
                    //else if (remainingPHDFaculty < 1 && (degreeType.Equals("PG")) && remainingPHDFaculty > 0)
                    //{
                    //    adjustedPHDFaculty = remainingPHDFaculty;

                    //}
                    #endregion

                    if (remainingPHDFaculty >= 2 && (degreeType.Equals("PG")) && item.SpecializationsphdFaculty > 0)
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
                        //}                       

                    }
                    else if (remainingPHDFaculty <= 1 && (degreeType.Equals("PG")) && item.SpecializationsphdFaculty > 0)
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

                        //if (item.Degree == "M.Tech")
                        //{
                        //    adjustedPHDFaculty = item.SpecializationsphdFaculty;
                        //    remainingPHDFaculty = remainingPHDFaculty - 1;
                        //    //Written By Narayana
                        //    //item.approvedIntake1 = p(adjustedPHDFaculty);
                        //    SpecializationwisePHDFaculty = adjustedPHDFaculty;
                        //    PhdminimumRequirementMet = "YES";
                        //}
                        //else
                        //{
                        //    adjustedPHDFaculty = remainingPHDFaculty;
                        //    //remainingPHDFaculty = remainingPHDFaculty - 1;
                        //    //Written By Narayana
                        //    //item.approvedIntake1 = PhdWiseIntakeForMBAandMCA(adjustedPHDFaculty);
                        //    //item.isintakechange = true;
                        //    if (remainingPHDFaculty >= SpecializationwisePHDFaculty)
                        //    {
                        //        PhdminimumRequirementMet = "YES";
                        //    }
                        //    else
                        //    {
                        //        if (remainingPHDFaculty == 0)
                        //        {
                        //            PhdminimumRequirementMet = "YES";

                        //        }
                        //        else
                        //        {
                        //            item.approvedIntake1 = PhdWiseIntakeForMBAandMCA(adjustedPHDFaculty);
                        //            PhdminimumRequirementMet = "NO";
                        //        }

                        //    }

                        //    if (SpecializationwisePHDFaculty != adjustedPHDFaculty)
                        //        item.isintakechange = true;
                        //    //SpecializationwisePHDFaculty = adjustedPHDFaculty;
                        //    //PhdminimumRequirementMet = "YES";
                        //}

                    }
                    else if (SpecializationwisePHDFaculty > 0 && (degreeType.Equals("UG")) && remainingPHDFaculty > 0)
                    {
                        //remainingPHDFaculty = remainingPHDFaculty - SpecializationwisePHDFaculty;
                        //adjustedPHDFaculty = SpecializationwisePHDFaculty;
                        if (remainingPHDFaculty >= SpecializationwisePHDFaculty)//item.SpecializationsphdFaculty
                        {
                            remainingPHDFaculty = remainingPHDFaculty - SpecializationwisePHDFaculty;
                            adjustedPHDFaculty = SpecializationwisePHDFaculty; //item.SpecializationsphdFaculty;
                            PhdminimumRequirementMet = "YES";
                        }
                        else if (remainingPHDFaculty <= SpecializationwisePHDFaculty)//item.SpecializationsphdFaculty
                        {
                            //This Code is Commented By Narayana on 28-04-2018
                            //adjustedPHDFaculty = remainingPHDFaculty;//SpecializationwisePHDFaculty;
                            //remainingPHDFaculty = 0;//remainingPHDFaculty - SpecializationwisePHDFaculty;
                            //PhdminimumRequirementMet = "NO";
                            if (item.Proposedintake > 60)
                            {
                                item.Proposedintake = GetIntakeBasedOnPhd(remainingPHDFaculty);
                                item.isintakechange = true;
                                SpecializationwisePHDFaculty = remainingPHDFaculty;
                                adjustedPHDFaculty = remainingPHDFaculty;
                                PhdminimumRequirementMet = "YES";
                            }
                            else
                            {
                                //item.Proposedintake = GetIntakeBasedOnPhd(remainingPHDFaculty);
                                //item.isintakechange = true;
                                SpecializationwisePHDFaculty = remainingPHDFaculty;
                                if (remainingPHDFaculty < 0)
                                {
                                    remainingPHDFaculty = 0;
                                }
                                adjustedPHDFaculty = remainingPHDFaculty;
                                PhdminimumRequirementMet = "YES";
                            }

                            // adjustedPHDFaculty = item.SpecializationsphdFaculty;

                        }



                    }
                    else if (SpecializationwisePHDFaculty > 0 && (degreeType.Equals("UG")) && remainingPHDFaculty <= 0)
                    {
                        // remainingPHDFaculty = SpecializationwisePHDFaculty - remainingPHDFaculty;
                        //This Code Commented By Narayana 28-04-2018
                        if (item.Proposedintake > 60)
                        {
                            item.Proposedintake = GetIntakeBasedOnPhd(remainingPHDFaculty);
                            item.isintakechange = true;
                            SpecializationwisePHDFaculty = remainingPHDFaculty;
                            adjustedPHDFaculty = remainingPHDFaculty;
                            PhdminimumRequirementMet = "YES";
                        }
                        //adjustedPHDFaculty = remainingPHDFaculty;
                        //PhdminimumRequirementMet = "NO";

                    }
                    else if (SpecializationwisePHDFaculty == 0 && (degreeType.Equals("UG")))
                    {
                        PhdminimumRequirementMet = "NO";
                    }
                    else if (adjustedPHDFaculty <= 0 && (degreeType.Equals("PG")))
                    {
                        PhdminimumRequirementMet = "YES";
                    }
                    //Dual Degree Checking
                    else if (remainingPHDFaculty >= 2 && (degreeType.Equals("Dual Degree")) && item.SpecializationsphdFaculty > 0)
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
                    else if (remainingPHDFaculty <= 1 && (degreeType.Equals("Dual Degree")) && item.SpecializationsphdFaculty >= 0)
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
                    else if (SpecializationwisePHDFaculty == 0 && (degreeType.Equals("Dual Degree")))
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
                    else if (SpecializationwisePHDFaculty > 0 && (degreeType.Equals("UG")) && remainingPHDFaculty > 0)
                    {
                        //remainingPHDFaculty = remainingPHDFaculty - SpecializationwisePHDFaculty;
                        //adjustedPHDFaculty = SpecializationwisePHDFaculty;
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
                    else if (SpecializationwisePHDFaculty > 0 && (degreeType.Equals("UG")) && remainingPHDFaculty <= 0)
                    {
                        // remainingPHDFaculty = SpecializationwisePHDFaculty - remainingPHDFaculty;
                        //This Code is Commented by Narayana 28-04-2018
                        //adjustedPHDFaculty = remainingPHDFaculty;
                        //PhdminimumRequirementMet = "NO";
                        if (item.Proposedintake > 60)
                        {
                            item.Proposedintake = GetIntakeBasedOnPhd(remainingPHDFaculty);
                            item.isintakechange = true;
                            SpecializationwisePHDFaculty = remainingPHDFaculty;
                            adjustedPHDFaculty = remainingPHDFaculty;
                            PhdminimumRequirementMet = "YES";
                        }
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
                    if (remainingPHDFaculty >= 2 && (degreeType.Equals("Dual Degree")) && item.SpecializationsphdFaculty > 0)
                    {
                        //remainingPHDFaculty = remainingPHDFaculty - item.SpecializationsphdFaculty;
                        //adjustedPHDFaculty = item.SpecializationsphdFaculty;   

                        //PhdminimumRequirementMet = "YES";
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
                    else if (remainingPHDFaculty <= 1 && (degreeType.Equals("Dual Degree")) && item.SpecializationsphdFaculty > 0)
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
                faculty += "<td class='col2' style='text-align: center; vertical-align: top;width:10px' rowspan='1'>" + (facultyCounts.IndexOf(item) + 1) + "</td>";
                faculty += "<td class='col2' style='text-align: left; vertical-align: top;'>" + item.Department + "</td>";
                faculty += "<td class='col2' style='text-align: left; vertical-align: top;'>" + item.Degree + "</td>";
                faculty += "<td class='col2' style='text-align: left; vertical-align: top;'>" + item.Specialization + "</td>";

                if (departments.Contains(item.Department))
                {
                    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + totalBtechFirstYearIntake + "</td>";
                    // faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + Math.Ceiling(firstYearRequired) + "</td>";

                    if (OthersSpecIds.Contains(item.specializationId))
                    {
                        faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + othersRequiredfaculty + "</td>";

                    }
                    else
                    {
                        faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + Math.Ceiling(firstYearRequired) + "</td>";

                    }



                }
                else
                {
                    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + item.totalIntake + "</td>";
                    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + Math.Ceiling(item.requiredFaculty) + "</td>";
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



                faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + adjustedFaculty + "</td>";

                if (item.Degree == "M.Tech")
                    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + item.SpecializationspgFaculty + "</td>";
                else
                    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + "" + "</td>";


                #region old Code
                //if (minimumRequirementMet == "YES")
                //{
                //    if (rFaculty <= adjustedFaculty)
                //        minimumRequirementMet = "NO";
                //    else
                //        minimumRequirementMet = "YES";
                //}

                //else if (minimumRequirementMet == "NO")
                //{
                //    if (rFaculty == adjustedFaculty)
                //        minimumRequirementMet = "NO";
                //    else
                //        minimumRequirementMet = "YES";
                //}
                #endregion

                if (minimumRequirementMet == "YES" && PhdminimumRequirementMet == "YES")
                {

                    if (rFaculty <= adjustedFaculty && SpecializationwisePHDFaculty <= adjustedPHDFaculty)
                    {
                        minimumRequirementMet = "NO";
                        PhdminimumRequirementMet = "NO";
                    }
                    else if (rFaculty > adjustedFaculty && SpecializationwisePHDFaculty <= adjustedPHDFaculty)
                    {
                        //  minimumRequirementMet = "Deficiency In faculty";
                        minimumRequirementMet = "YES";
                        PhdminimumRequirementMet = "NO";
                    }
                    else if (rFaculty <= adjustedFaculty && SpecializationwisePHDFaculty > adjustedPHDFaculty)
                    {
                        //  minimumRequirementMet = "Deficiency In Ph.D";

                        minimumRequirementMet = "NO";
                        PhdminimumRequirementMet = "YES";
                    }
                    else
                    {
                        // minimumRequirementMet = "Deficiency In faculty and Ph.D";
                        minimumRequirementMet = "YES";
                        PhdminimumRequirementMet = "YES";
                    }


                }
                else if (minimumRequirementMet == "NO" && PhdminimumRequirementMet == "YES")
                {

                    if (rFaculty <= adjustedFaculty && SpecializationwisePHDFaculty <= adjustedPHDFaculty)
                    {
                        //  minimumRequirementMet = "-";
                        minimumRequirementMet = "NO";
                        PhdminimumRequirementMet = "NO";
                    }
                    else if (rFaculty > adjustedFaculty && SpecializationwisePHDFaculty <= adjustedPHDFaculty)
                    {
                        //  minimumRequirementMet = "Deficiency In faculty";
                        minimumRequirementMet = "YES";
                        PhdminimumRequirementMet = "NO";
                    }
                    else if (rFaculty <= adjustedFaculty && SpecializationwisePHDFaculty > adjustedPHDFaculty)
                    {
                        // minimumRequirementMet = "Deficiency In Ph.D";
                        minimumRequirementMet = "NO";
                        PhdminimumRequirementMet = "YES";
                    }
                    else
                    {
                        // minimumRequirementMet = "Deficiency In faculty and Ph.D";
                        minimumRequirementMet = "YES";
                        PhdminimumRequirementMet = "YES";
                    }

                }
                else if (minimumRequirementMet == "YES" && PhdminimumRequirementMet == "NO")
                {

                    if (rFaculty <= adjustedFaculty && SpecializationwisePHDFaculty <= adjustedPHDFaculty)
                    {
                        // minimumRequirementMet = "-";
                        minimumRequirementMet = "NO";
                        PhdminimumRequirementMet = "NO";
                    }
                    else if (rFaculty > adjustedFaculty && SpecializationwisePHDFaculty <= adjustedPHDFaculty)
                    {
                        //  minimumRequirementMet = "Deficiency In faculty";
                        minimumRequirementMet = "YES";
                        PhdminimumRequirementMet = "NO";
                    }
                    else if (rFaculty <= adjustedFaculty && SpecializationwisePHDFaculty > adjustedPHDFaculty)
                    {
                        // minimumRequirementMet = "Deficiency In Ph.D";
                        minimumRequirementMet = "NO";
                        PhdminimumRequirementMet = "YES";
                    }
                    else
                    {
                        minimumRequirementMet = "YES";
                        PhdminimumRequirementMet = "YES";
                        // minimumRequirementMet = "Deficiency In faculty and Ph.D";
                    }

                }

                else if (minimumRequirementMet == "NO" && PhdminimumRequirementMet == "NO")
                {

                    if (rFaculty <= adjustedFaculty && SpecializationwisePHDFaculty <= adjustedPHDFaculty)
                    {
                        // minimumRequirementMet = "-";
                        minimumRequirementMet = "NO";
                        PhdminimumRequirementMet = "NO";
                    }
                    else if (rFaculty > adjustedFaculty && SpecializationwisePHDFaculty <= adjustedPHDFaculty)
                    {
                        // minimumRequirementMet = "Deficiency In faculty";
                        minimumRequirementMet = "YES";
                        PhdminimumRequirementMet = "NO";
                    }
                    else if (rFaculty <= adjustedFaculty && SpecializationwisePHDFaculty > adjustedPHDFaculty)
                    {
                        // minimumRequirementMet = "Deficiency In Ph.D";
                        minimumRequirementMet = "NO";
                        PhdminimumRequirementMet = "YES";
                    }
                    else
                    {
                        // minimumRequirementMet = "Deficiency In faculty and Ph.D";
                        minimumRequirementMet = "YES";
                        PhdminimumRequirementMet = "YES";
                    }
                }



                faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + minimumRequirementMet + "</td>";
                faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + PhdminimumRequirementMet + "</td>";
                //if (adjustedPHDFaculty >= 2 && degreeType.Equals("PG"))
                //{
                //    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>NO</td>";
                //}
                //else if (degreeType.Equals("PG"))
                //{
                //    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>YES</td>";
                //}
                //else
                //{
                //    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>-</td>";
                //}

                faculty += "</tr>";

                CollegeFacultyLabs newFaculty = new CollegeFacultyLabs();
                int degreeTypeId = degrees.Where(d => d.degree == item.Degree).Select(d => d.degreeTypeId).FirstOrDefault();
                newFaculty.DegreeType = degreeTypes.Where(t => t.id == degreeTypeId).Select(t => t.degreeType).FirstOrDefault();
                newFaculty.DegreeDisplayOrder = (int)degrees.Where(d => d.degree == item.Degree).Select(d => d.degreeDisplayOrder).FirstOrDefault();
                newFaculty.Degree = item.Degree;
                newFaculty.Department = item.Department;
                newFaculty.Specialization = item.Specialization;
                newFaculty.SpecializationId = item.specializationId;
                newFaculty.TotalIntake = item.Proposedintake;
                newFaculty.ShiftId = item.shiftId;
                newFaculty.isintakechange = item.isintakechange;
                newFaculty.ispercentage = item.ispercentage;
                newFaculty.ishashcourses = item.ishashcourses;
                newFaculty.DollerCourseIntake = item.dollercourseintake;
                newFaculty.isnewincreasecourse = item.isnewincreasecourse;
                newFaculty.isnewreducecourse = item.isnewreducecourse;
                newFaculty.isoldincreasecourse = item.isoldincreasecourse;
                newFaculty.isoldreducecourse = item.isoldreducecourse;
                if (departments.Contains(item.Department))
                {
                    //newFaculty.TotalIntake = totalBtechFirstYearIntake;
                    newFaculty.Required = (int)Math.Ceiling(firstYearRequired);
                }
                else
                {
                    // newFaculty.TotalIntake = item.totalIntake;
                    newFaculty.Required = (int)Math.Ceiling(item.requiredFaculty);
                }

                newFaculty.Available = adjustedFaculty;
                if (item.Degree == "M.Tech")
                {
                    newFaculty.Available = adjustedFaculty; //item.SpecializationspgFaculty;
                }
                newFaculty.Deficiency = minimumRequirementMet;
                newFaculty.PhdDeficiency = PhdminimumRequirementMet;
                newFaculty.Requiredphd = SpecializationwisePHDFaculty;
                newFaculty.Availablephd = adjustedPHDFaculty;



                //if (adjustedPHDFaculty >= 1 && degreeType.Equals("PG") && item.Degree != "MBA")
                //{
                //    newFaculty.PhdDeficiency = "NO";
                //}
                //else if (adjustedPHDFaculty >= 1 && degreeType.Equals("PG") && item.Degree == "MBA")
                //{
                //    if(item.totalIntake>120)
                //    {
                //       if(adjustedPHDFaculty>=2)
                //           newFaculty.PhdDeficiency = "NO";
                //        else
                //           newFaculty.PhdDeficiency = "YES";
                //    }
                //    else
                //    {
                //        if (adjustedPHDFaculty >= 1)
                //            newFaculty.PhdDeficiency = "NO";
                //        else
                //            newFaculty.PhdDeficiency = "YES";
                //    }

                //}
                //else if (degreeType.Equals("PG"))
                //{
                //    newFaculty.PhdDeficiency = "YES";
                //}
                //else
                //{
                //    newFaculty.PhdDeficiency = "-";
                //}

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

        public List<CollegeFacultyWithIntakeReport> collegeFaculty(int? collegeId)
        {
            List<CollegeIntakeExisting> collegeIntakeExisting = new List<CollegeIntakeExisting>();
            List<CollegeFacultyWithIntakeReport> intakedetailsList = new List<CollegeFacultyWithIntakeReport>();
            int studentcount = Convert.ToInt32(ConfigurationManager.AppSettings["studentcount"]);
            string CollegeAffiliationStatus = db.jntuh_college_affiliation.Where(u => u.collegeId == collegeId && u.affiliationTypeId == 7).Select(u => u.affiliationStatus).FirstOrDefault();
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
                    newIntake.courseStatus = item.courseStatus;
                    newIntake.updatedOn = item.updatedOn;
                    newIntake.updatedBy = item.updatedBy;
                    collegeIntakeExisting.Add(newIntake);
                }
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
                var jntuh_college_faculty_registered = db.jntuh_college_faculty_registered.Where(cf => cf.collegeId == collegeId).ToList();  // && cf.createdBy != 63809
                string[] strRegnos = jntuh_college_faculty_registered.Select(cf => cf.RegistrationNumber).ToArray();

                //education categoryIds UG,PG,PHD...........

                int pharmacyDeptId = db.jntuh_department.Where(d => d.departmentName == "Pharmacy").Select(d => d.id).FirstOrDefault();

                var jntuh_education_category = db.jntuh_education_category.ToList();

                // var registeredFaculty = db.jntuh_registered_faculty.Where(rf => strRegnos.Contains(rf.RegistrationNumber) && (rf.collegeId == null || rf.collegeId == collegeId)).ToList();
                var jntuh_college_faculty_registered_new = db.jntuh_college_principal_registered.Where(cf => cf.collegeId == collegeId).ToList();
                var principalRegno = jntuh_college_faculty_registered_new.Select(cf => cf.RegistrationNumber).FirstOrDefault();


                var registeredFaculty = new List<jntuh_registered_faculty>();

                registeredFaculty = principalRegno != null ? db.jntuh_registered_faculty.Where(rf => strRegnos.Contains(rf.RegistrationNumber.Trim())).ToList()
                : db.jntuh_registered_faculty.Where(rf => strRegnos.Contains(rf.RegistrationNumber.Trim())).ToList();//&& rf.RegistrationNumber != principalRegno


                //   var jntuh_registered_faculty1 =
                //registeredFaculty.Where(rf => rf.type != "Adjunct" && ((rf.NoForm16 == false || rf.NoForm16 == null) && (rf.NoForm26AS == false || rf.NoForm26AS == null) && (rf.Absent == false || rf.Absent == null) && (rf.AbsentforVerification == false || rf.AbsentforVerification == null) && (rf.OriginalCertificatesNotShown == false || rf.OriginalCertificatesNotShown == null) && (rf.Xeroxcopyofcertificates == false || rf.Xeroxcopyofcertificates == null) && (rf.NotQualifiedAsperAICTE == false || rf.NotQualifiedAsperAICTE == null)//&& rf.Noform16Verification == false && rf.NoForm16 == false
                //                                    && (rf.Noclass == false || rf.Noclass == null) && (rf.FakePHD == false || rf.FakePHD == null) && (rf.Genuinenessnotsubmitted == false || rf.Genuinenessnotsubmitted == null) && (rf.NoSCM == false || rf.NoSCM == null) && (rf.PANNumber != null) && (rf.IncompleteCertificates == false || rf.IncompleteCertificates == null) && (rf.Blacklistfaculy == false) && rf.NoRelevantUG != "Yes" && rf.NoRelevantPG != "Yes" && rf.NORelevantPHD != "Yes" && (rf.InvalidPANNumber == false || rf.InvalidPANNumber == null) && (rf.OriginalsVerifiedPHD != true) && (rf.OriginalsVerifiedUG != true) && (rf.Invaliddegree == false || rf.Invaliddegree == null) && rf.InvalidAadhaar != "Yes") && (rf.DepartmentId != 61 || rf.DepartmentId != 27)).Select(rf => new


                var jntuh_registered_faculty1 =
                   registeredFaculty.Where(rf => rf.type != "Adjunct" && ((rf.NotQualifiedAsperAICTE == false || rf.NotQualifiedAsperAICTE == null) && (rf.Noclass == false || rf.Noclass == null) && (rf.Absent == false || rf.Absent == null) && (rf.OriginalCertificatesNotShown == false || rf.OriginalCertificatesNotShown == null) &&
                                                         (rf.NoSCM == false || rf.NoSCM == null) && rf.NoRelevantUG != "Yes" && rf.NoRelevantPG != "Yes" && rf.NORelevantPHD != "Yes" && (rf.InvalidPANNumber == false || rf.InvalidPANNumber == null) && (rf.Blacklistfaculy == false) && (rf.AbsentforVerification == false || rf.AbsentforVerification == null) && (rf.DepartmentId != 61 || rf.DepartmentId != 27))).Select(rf => new
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
                jntuh_registered_faculty1 = jntuh_registered_faculty1.Where(e => e.HighestDegreeID >= 4).ToList();
                var jntuh_registered_faculty = jntuh_registered_faculty1.Select(rf => new
                {
                    RegistrationNumber = rf.RegistrationNumber,
                    Department = rf.DeptId != null ? jntuh_departments.Where(e => e.id == rf.DeptId).Select(e => e.departmentName).FirstOrDefault() : DeptNameBasedOnSpecialization.Where(e => e.Specid == rf.SpecializationId).Select(e => e.DeptName).FirstOrDefault(),
                    HighestDegree = jntuh_education_category.Where(c => c.id == rf.HighestDegreeID).Select(c => c.educationCategoryName).FirstOrDefault(),
                    Recruitedfor = jntuh_college_faculty_registered.Where(c => c.RegistrationNumber.Trim() == rf.RegistrationNumber.Trim()).Select(c => c.IdentifiedFor).FirstOrDefault(),
                    SpecializationId = jntuh_college_faculty_registered.Where(c => c.RegistrationNumber.Trim() == rf.RegistrationNumber.Trim()).Select(c => c.SpecializationId).FirstOrDefault(),
                    PanNumber = rf.PanNumber,
                    AadhaarNumber = rf.AadhaarNumber
                }).Where(e => e.Department != null).ToList();


                int[] StrPharmacy = new[] { 26, 27, 36, 39 };
                foreach (var item in collegeIntakeExisting.Where(D => !StrPharmacy.Contains(D.DepartmentID)).ToList())
                {
                    CollegeFacultyWithIntakeReport intakedetails = new CollegeFacultyWithIntakeReport();
                    int phdFaculty; int pgFaculty; int ugFaculty; int SpecializationphdFaculty = 0; int SpecializationpgFaculty = 0;

                    intakedetails.collegeId = item.collegeId;
                    intakedetails.collegeCode = jntuh_college.Where(c => c.id == item.collegeId).Select(c => c.collegeCode).FirstOrDefault();
                    intakedetails.collegeName = jntuh_college.Where(c => c.id == item.collegeId).Select(c => c.collegeName).FirstOrDefault();
                    intakedetails.Degree = item.Degree;
                    intakedetails.Department = item.Department;
                    intakedetails.Specialization = item.Specialization;
                    intakedetails.specializationId = item.specializationId;
                    intakedetails.DepartmentID = item.DepartmentID;
                    intakedetails.shiftId = item.shiftId;
                    intakedetails.isintakechange = false;
                    intakedetails.ispercentage = false;
                    intakedetails.ishashcourses = false;
                    intakedetails.isdollercourses = false;
                    item.courseStatus = db.jntuh_college_intake_existing.Where(
                        e =>
                            e.specializationId == item.specializationId && e.shiftId == item.shiftId &&
                            e.academicYearId == AY1 &&
                            e.collegeId == item.collegeId).Select(s => s.courseStatus).FirstOrDefault();
                    intakedetails.dollercourseintake = db.jntuh_college_intake_existing.Where(
                        e =>
                            e.specializationId == item.specializationId && e.shiftId == item.shiftId &&
                            e.academicYearId == AY1 &&
                            e.collegeId == item.collegeId).Select(s => s.approvedIntake).FirstOrDefault();
                    if (item.courseStatus == "P")
                    {
                        intakedetails.isdollercourses = true;
                    }
                    if (item.courseStatus == "NewReduce")
                    {
                        intakedetails.isnewreducecourse = true;

                    }
                    if (item.courseStatus == "OldIncrease")
                    {
                        intakedetails.isoldincreasecourse = true;

                    }
                    if (item.courseStatus == "OldReduce")
                    {
                        intakedetails.isoldreducecourse = true;

                    }
                    item.updatedBy =
                        db.jntuh_college_intake_existing.Where(
                            e =>
                                e.specializationId == item.specializationId && e.shiftId == item.shiftId && e.academicYearId == AY1 &&
                                e.collegeId == item.collegeId).Select(s => s.updatedBy).FirstOrDefault() == null
                            ? 0
                            : db.jntuh_college_intake_existing.Where(e => e.specializationId == item.specializationId && e.academicYearId == AY1 && e.shiftId == item.shiftId && e.collegeId == item.collegeId).Select(s => s.updatedBy).FirstOrDefault();
                    if (item.updatedBy == 8214)
                    {
                        intakedetails.isintakechange = true;
                    }

                    intakedetails.Proposedintake = GetIntake(item.collegeId, AY1, item.specializationId, item.shiftId, 1, item.degreeID);
                    intakedetails.approvedIntake1 = GetIntake(item.collegeId, AY1, item.specializationId, item.shiftId, 1, item.degreeID);
                    //Getting Admitted Intakes
                    intakedetails.admittedIntake1 = GetIntake(item.collegeId, AY1, item.specializationId, item.shiftId, 1, item.degreeID);
                    intakedetails.admittedIntake2 = GetIntake(item.collegeId, AY2, item.specializationId, item.shiftId, 1, item.degreeID);
                    intakedetails.admittedIntake3 = GetIntake(item.collegeId, AY3, item.specializationId, item.shiftId, 1, item.degreeID);
                    intakedetails.admittedIntake4 = GetIntake(item.collegeId, AY4, item.specializationId, item.shiftId, 1, item.degreeID);
                    intakedetails.admittedIntake5 = GetIntake(item.collegeId, AY5, item.specializationId, item.shiftId, 1, item.degreeID);

                    //Getting AICTE Intakes
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

                    //Getting Sanctionion (JNTU) intake Brach intake
                    intakedetails.SanctionIntake1 = GetIntake(item.collegeId, AY1, item.specializationId, item.shiftId, 0, item.degreeID);
                    intakedetails.SanctionIntake2 = GetIntake(item.collegeId, AY2, item.specializationId, item.shiftId, 0, item.degreeID);
                    intakedetails.SanctionIntake3 = GetIntake(item.collegeId, AY3, item.specializationId, item.shiftId, 0, item.degreeID);
                    intakedetails.SanctionIntake4 = GetIntake(item.collegeId, AY4, item.specializationId, item.shiftId, 0, item.degreeID);
                    intakedetails.SanctionIntake5 = GetIntake(item.collegeId, AY5, item.specializationId, item.shiftId, 0, item.degreeID);

                    intakedetails.AffiliationStatus2 = GetAcademicYear(item.collegeId, AY1, item.specializationId, item.shiftId, item.degreeID);
                    intakedetails.AffiliationStatus3 = GetAcademicYear(item.collegeId, AY2, item.specializationId, item.shiftId, item.degreeID);
                    intakedetails.AffiliationStatus4 = GetAcademicYear(item.collegeId, AY3, item.specializationId, item.shiftId, item.degreeID);


                    facultystudentRatio = Convert.ToInt32(jntuh_faculty_student_ratio_norms.Where(fn => fn.degreeId == item.degreeID).Select(fn => fn.Norms).FirstOrDefault().Split(':')[1].ToString());


                    if (item.Degree == "B.Tech")
                    {
                        if (CollegeAffiliationStatus == "Yes")
                        {
                            intakedetails.ispercentage = false;
                        }
                        else if (item.courseStatus != "NewIncrease")
                        {
                            #region This Code is commented on 02-07-2019 25% case Removing
                            int senondyearpercentage = 0;
                            int thirdyearpercentage = 0;
                            int fourthyearpercentage = 0;
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
                                    intakedetails.ishashcourses = true;
                                    //if (intakedetails.Proposedintake>60)
                                    //{
                                    //    intakedetails.Proposedintake = 60;
                                    //}                                  
                                }
                            }
                            #endregion
                        }

                        if (intakedetails.ispercentage == false)
                        {
                            //Getting from web.config AICTE Sanctioned-1 or JNTU Sanctioned-2 or Admitted Intake-3
                            int takecondition = Convert.ToInt32(ConfigurationManager.AppSettings["intakecondition"]);
                            if (item.courseStatus == "NewIncrease")
                            {
                                intakedetails.isnewincreasecourse = true;
                                facultyRatio = 3;
                            }
                            else if (item.courseStatus == "TwoYears")
                            {
                                //This Condition is for which course have only last Two Years we take only last Two Years Intake Based Faculty Ration written by Narayana Reddy.                               
                                if (takecondition == 1)
                                {
                                    if (item.Department == "CSE(SE)" || item.Department == "CSE(CS)" || item.Department == "CSE(AI&ML)" || item.Department == "CSE(DS)" || item.Department == "CSE(IoT)" || item.Department == "CSE(Networks)" || item.DepartmentID == 22)
                                    {
                                        intakedetails.totalIntake = GetBtechAdmittedIntake(intakedetails.SanctionIntake2);
                                    }
                                    else
                                    {
                                        intakedetails.totalIntake =
                                            GetBtechAdmittedIntake(intakedetails.AICTESanctionIntake2) +
                                            GetBtechAdmittedIntake(intakedetails.AICTESanctionIntake3);
                                    }
                                }
                                else if (takecondition == 2)
                                {
                                    if (item.Department == "CSE(SE)" || item.Department == "CSE(CS)" || item.Department == "CSE(AI&ML)" || item.Department == "CSE(DS)" || item.Department == "CSE(IoT)" || item.Department == "CSE(Networks)" || item.DepartmentID == 22)
                                    {
                                        intakedetails.totalIntake = GetBtechAdmittedIntake(intakedetails.SanctionIntake2);
                                    }
                                    else
                                    {
                                        intakedetails.totalIntake = GetBtechAdmittedIntake(intakedetails.SanctionIntake2) +
                                                                    GetBtechAdmittedIntake(intakedetails.SanctionIntake3);
                                    }
                                }
                                else
                                {
                                    if (item.Department == "CSE(SE)" || item.Department == "CSE(CS)" || item.Department == "CSE(AI&ML)" || item.Department == "CSE(DS)" || item.Department == "CSE(IoT)" || item.Department == "CSE(Networks)" || item.DepartmentID == 22)
                                    {
                                        intakedetails.totalIntake = GetBtechAdmittedIntake(intakedetails.SanctionIntake2);
                                    }
                                    else
                                    {
                                        intakedetails.totalIntake = GetBtechAdmittedIntake(intakedetails.admittedIntake2) +
                                                                         GetBtechAdmittedIntake(intakedetails.admittedIntake3);
                                    }
                                }
                                facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) /
                                        Convert.ToDecimal(facultystudentRatio);
                            }
                            else
                            {
                                if (takecondition == 1)
                                {
                                    if (item.Department == "CSE(SE)" || item.Department == "CSE(CS)" || item.Department == "CSE(AI&ML)" || item.Department == "CSE(DS)" || item.Department == "CSE(IoT)" || item.Department == "CSE(Networks)" || item.DepartmentID == 22)
                                    {
                                        intakedetails.totalIntake = GetBtechAdmittedIntake(intakedetails.SanctionIntake2);
                                    }
                                    else
                                    {
                                        intakedetails.totalIntake = GetBtechAdmittedIntake(intakedetails.AICTESanctionIntake2) + GetBtechAdmittedIntake(intakedetails.AICTESanctionIntake3) +
                                                       GetBtechAdmittedIntake(intakedetails.AICTESanctionIntake4);
                                    }

                                }
                                else if (takecondition == 2)
                                {
                                    if (item.Department == "CSE(SE)" || item.Department == "CSE(CS)" || item.Department == "CSE(AI&ML)" || item.Department == "CSE(DS)" || item.Department == "CSE(IoT)" || item.Department == "CSE(Networks)" || item.DepartmentID == 22)
                                    {
                                        intakedetails.totalIntake = GetBtechAdmittedIntake(intakedetails.SanctionIntake2);
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
                                    if (item.Department == "CSE(SE)" || item.Department == "CSE(CS)" || item.Department == "CSE(AI&ML)" || item.Department == "CSE(DS)" || item.Department == "CSE(IoT)" || item.Department == "CSE(Networks)" || item.DepartmentID == 22)
                                    {
                                        intakedetails.totalIntake = GetBtechAdmittedIntake(intakedetails.SanctionIntake2);
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
                            }

                            intakedetails.totalAdmittedIntake = (intakedetails.SanctionIntake2) + (intakedetails.SanctionIntake3) + (intakedetails.SanctionIntake4);
                        }
                    }
                    else if (item.Degree == "M.Tech" || item.Degree == "MBA")
                    {
                        if (item.Degree == "M.Tech" && item.shiftId == 1)
                        {
                            intakedetails.totalIntake = (intakedetails.Proposedintake) + (intakedetails.AICTESanctionIntake2);
                            facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) /
                                           Convert.ToDecimal(facultystudentRatio);
                            int fratio = (int)facultyRatio;
                            if (fratio < 3)
                            {
                                fratio = 3;
                                facultyRatio = Convert.ToDecimal(fratio);
                            }
                            //intakedetails.totalIntake = (intakedetails.approvedIntake1) + (intakedetails.approvedIntake2);
                            //facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) /
                            //               Convert.ToDecimal(facultystudentRatio);
                        }
                        if (item.Degree == "M.Tech" && item.shiftId == 2)
                        {
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
                            intakedetails.totalIntake = (intakedetails.Proposedintake) + (intakedetails.AICTESanctionIntake2);
                            facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) /
                                           Convert.ToDecimal(facultystudentRatio);
                        }
                        //Old Affilication Code
                        //intakedetails.totalIntake = (intakedetails.approvedIntake1) + (intakedetails.approvedIntake2);
                        //facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) /
                        //               Convert.ToDecimal(facultystudentRatio);

                    }
                    else if (item.Degree == "MCA")
                    {
                        intakedetails.totalIntake = (intakedetails.Proposedintake) + (intakedetails.AICTESanctionIntake2) +
                                                    (intakedetails.AICTESanctionIntake3);
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
                            intakedetails.specializationWiseFaculty = jntuh_registered_faculty.Where(f => f.Department == "Pharmacy" && f.Recruitedfor == "UG").Count();
                        }
                        else
                        {
                            intakedetails.specializationWiseFaculty = jntuh_registered_faculty.Count(f => f.Department == item.Department);//&& f.Recruitedfor == "UG"
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
                            intakedetails.specializationWiseFaculty = jntuh_registered_faculty.Count(f => f.Department == item.Department && f.SpecializationId == item.specializationId && (f.Recruitedfor == "PG" || f.Recruitedfor == "UG&PG"));
                            //&& (f.Recruitedfor == "PG" || f.Recruitedfor == "UG&PG")
                        }
                    }
                    if (strdegreetype == "Dual Degree")
                    {
                        intakedetails.specializationWiseFaculty = jntuh_registered_faculty.Count(f => f.SpecializationId == item.specializationId);//(f.Recruitedfor == "PG" || f.Recruitedfor == "UG&PG") &&
                    }

                    int noPanOrAadhaarcount = 0;

                    if (item.Degree == "B.Pharmacy" || item.Degree == "M.Pharmacy")
                    {
                        ugFaculty = jntuh_registered_faculty.Where(f => "UG" == f.HighestDegree && f.Department == "Pharmacy").Count();
                        pgFaculty = jntuh_registered_faculty.Where(f => ("PG" == f.HighestDegree || "M.Phil / Other PG Degree" == f.HighestDegree) && f.Department == "Pharmacy").Count();
                        phdFaculty = jntuh_registered_faculty.Where(f => "Ph.D" == f.HighestDegree && f.Department == "Pharmacy").Count();
                        noPanOrAadhaarcount = registeredFaculty.Where(f => f.DepartmentId == pharmacyDeptId && f.PANNumber == null && f.AadhaarNumber == null && (f.isApproved == null || f.isApproved == true)).Count();
                        intakedetails.Department = "Pharmacy";
                    }
                    else
                    {
                        ugFaculty = jntuh_registered_faculty.Count(f => "UG" == f.HighestDegree && f.Department == item.Department);
                        pgFaculty = jntuh_registered_faculty.Count(f => ("PG" == f.HighestDegree || "M.Phil / Other PG Degree" == f.HighestDegree) && f.Department == item.Department);
                        //if (item.Degree == "M.Tech")
                        //phdFaculty = jntuh_registered_faculty.Count(f => "Ph.D" == f.HighestDegree && f.Department == item.Department && f.SpecializationId==item.specializationId);
                        //else
                        phdFaculty = jntuh_registered_faculty.Count(f => "Ph.D" == f.HighestDegree && f.Department == item.Department);

                        var phd = jntuh_registered_faculty.Where(f => "Ph.D" == f.HighestDegree && f.Department == item.Department).ToList();
                        //  SpecializationphdFaculty = jntuh_registered_faculty.Count(f => "Ph.D" == f.HighestDegree && f.SpecializationId == item.specializationId);
                        if (item.Degree == "B.Tech")
                            SpecializationphdFaculty = jntuh_registered_faculty.Count(f => "Ph.D" == f.HighestDegree && (f.SpecializationId == item.specializationId || f.SpecializationId == null) && f.Department == item.Department);
                        else
                            SpecializationphdFaculty = jntuh_registered_faculty.Count(f => "Ph.D" == f.HighestDegree && f.SpecializationId == item.specializationId);
                        var reg = jntuh_registered_faculty.Where(f => f.SpecializationId == item.specializationId && (f.Recruitedfor == "PG" || f.Recruitedfor == "UG&PG")).Select(S => S.RegistrationNumber).ToList();
                        SpecializationpgFaculty = jntuh_registered_faculty.Count(f => f.SpecializationId == item.specializationId && (f.Recruitedfor == "PG" || f.Recruitedfor == "UG&PG"));
                        var regphdfaculty = jntuh_registered_faculty.Where(f => "Ph.D" == f.HighestDegree && f.SpecializationId == item.specializationId);

                    }

                    intakedetails.phdFaculty = phdFaculty;
                    intakedetails.SpecializationsphdFaculty = SpecializationphdFaculty;
                    intakedetails.SpecializationspgFaculty = SpecializationpgFaculty;
                    intakedetails.pgFaculty = pgFaculty;
                    intakedetails.ugFaculty = ugFaculty;
                    //intakedetails.ispercentage = item.ispercentage;
                    intakedetails.totalFaculty = (ugFaculty + pgFaculty + phdFaculty);
                    intakedetails.facultyWithoutPANAndAadhaar = noPanOrAadhaarcount;
                    //=============//

                    intakedetailsList.Add(intakedetails);
                }

                intakedetailsList = intakedetailsList.OrderBy(ei => ei.Department).ThenBy(ei => ei.degreeDisplayOrder).ThenBy(ei => ei.Specialization).ThenBy(ei => ei.shiftId).ToList();

                string[] strOtherDepartments = { "English", "Mathematics", "Physics", "Chemistry", "Others(CSE/IT)", "Others(CIVIL/MECH)", "Others(ECE/EEE)", "Others(MNGT/H&S)" };
                //string[] strOtherDepartments = intakedetailsList.Select(d => d.Department).ToArray();
                int btechdegreecount = intakedetailsList.Count(d => d.Degree == "B.Tech");
                if (btechdegreecount != 0)
                {
                    foreach (var department in strOtherDepartments)
                    {
                        //int speId = jntuh_specialization.Where(s => s.jntuh_department.departmentName == department).Select(s => s.id).FirstOrDefault();
                        //int ugFaculty = jntuh_registered_faculty.Where(f => "UG" == f.HighestDegree && f.Department == department).Count();
                        //int pgFaculty = jntuh_registered_faculty.Where(f => ("PG" == f.HighestDegree || "M.Phil" == f.HighestDegree) && f.Department == department).Count();
                        //int phdFaculty = jntuh_registered_faculty.Where(f => "Ph.D" == f.HighestDegree && f.Department == department).Count();

                        //int facultydeficiencyId = jntuh_college_faculty_deficiency.Where(fd => fd.CollegeId == collegeId && fd.SpecializationId == speId && fd.ShiftId == 1).Select(fd => fd.Id).FirstOrDefault();
                        //if (facultydeficiencyId == 0)
                        //{
                        //    intakedetailsList.Add(new CollegeFacultyWithIntakeReport { collegeId = (int)collegeId, Degree = "B.Tech", Department = department, Specialization = department, ugFaculty = ugFaculty, pgFaculty = pgFaculty, phdFaculty = phdFaculty, totalFaculty = ugFaculty + pgFaculty + phdFaculty, specializationId = speId, shiftId = 1 });
                        //}
                        //else
                        //{
                        //    int? swf = jntuh_college_faculty_deficiency.Where(fd => fd.Id == facultydeficiencyId).Select(fd => fd.SpecializationWiseFaculty).FirstOrDefault();
                        //    bool deficiency = jntuh_college_faculty_deficiency.Where(fd => fd.Id == facultydeficiencyId).Select(fd => fd.Deficiency).FirstOrDefault();
                        //    int shortage = jntuh_college_faculty_deficiency.Where(fd => fd.Id == facultydeficiencyId).Select(fd => fd.Shortage).FirstOrDefault();
                        //    intakedetailsList.Add(new CollegeFacultyWithIntakeReport { id = facultydeficiencyId, collegeId = (int)collegeId, Degree = "B.Tech", Department = department, Specialization = department, ugFaculty = ugFaculty, pgFaculty = pgFaculty, phdFaculty = phdFaculty, totalFaculty = ugFaculty + pgFaculty + phdFaculty, specializationId = speId, shiftId = 1, specializationWiseFaculty = (int)swf, deficiency = deficiency, shortage = shortage });
                        //}
                        var deptid = jntuh_departments.Where(i => i.departmentName == department).Select(i => i.id).FirstOrDefault();
                        var deptname = jntuh_departments.Where(i => i.departmentName == department).Select(i => i.departmentName).FirstOrDefault();
                        int speId = jntuh_specialization.Where(s => s.jntuh_department.departmentName == department).Select(s => s.id).FirstOrDefault();
                        int ugFaculty = jntuh_registered_faculty.Count(f => "UG" == f.HighestDegree && f.Department == department);
                        int pgFaculty = jntuh_registered_faculty.Count(f => ("PG" == f.HighestDegree || "M.Phil / Other PG Degree" == f.HighestDegree) && f.Department == department);
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
                            Proposedintake = 1,
                            specializationWiseFaculty = jntuh_registered_faculty.Count(f => f.Department == deptname)
                        });
                    }
                }
            }

            return intakedetailsList;
        }
        //Sample Dish TV Code

        public ActionResult AffiliationLetter1(string type)
        {


            string path = SaveCollegeDefficiencyLetterPdf1(type);
            path = path.Replace("/", "\\");
            if (!string.IsNullOrEmpty(path))
            {
                return File(path, "application/pdf", path.Substring(path.LastIndexOf('\\') + 1));
            }
            return View();
        }

        private string SaveCollegeDefficiencyLetterPdf1(string type)
        {
            string fullPath = string.Empty;
            //Set page size as A4
            var pdfDoc = new Document(PageSize.A4, 40, 40, 40, 40);

            // Create a new PdfWrite object, writing the output to a MemoryStream
            //open pdf
            var output = new MemoryStream();
            var writer = PdfWriter.GetInstance(pdfDoc, output);

            //  int InspectionPhaseId = db.jntuh_inspection_phase.Where(p => p.isActive == true).Select(p => p.id).SingleOrDefault();

            string path = Server.MapPath("~/Content/PDFReports/dishtv/");

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            Random rnd = new Random();
            fullPath = path + rnd.Next(0, 9999).ToString("D4") + "-Dishtv.pdf";
            PdfWriter.GetInstance(pdfDoc, new FileStream(fullPath, FileMode.Create));

            //Open PDF Document to write data
            pdfDoc.Open();

            //Assign Html content in a string to write in PDF
            string contents = string.Empty;

            StreamReader sr;

            //Read file from server path
            sr = System.IO.File.OpenText(Server.MapPath("~/Content/AffiliationProceedings123.html"));

            //store content in the variable
            contents = sr.ReadToEnd();
            sr.Close();


            //contents += "<table border='1' cellpadding='3' cellspacing='0' width='100%' height='50%' style='font-family: Times New Roman; font-size: 9px; font-weight: normal; background-color: #fff; margin: 0 auto;'>";
            //contents += "<tr>";
            //contents += "<td  align='left' style='width:10%'><b>1. Application's Name</b></td>";
            //contents += "<td align='left' style='width:5%'><b>Mr./Ms.</b></td>";
            //contents += "<td align='left' style='width:85%'><table border='1' cellpadding='10' cellspacing='0' height='50%' ><tr><td></td><td></td><td></td><td></td><td></td><td></td><td></td><td></td><td></td><td></td><td></td><td></td></tr></table></td>";
            //contents += "</tr>";
            //contents += "</table>";


            //Read string contents using stream reader and convert html to parsed conent
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

        #region CORRIGENDUM
        public ActionResult CorrigendumLetter(int collegeId, string type)
        {
            List<int> collegeIds = db.jntuh_college.Where(c => c.collegeCode != "WL").Select(c => c.id).ToList();

            string code = db.jntuh_college.Where(c => c.id == collegeId && c.isActive == true).Select(c => c.collegeCode).FirstOrDefault().ToUpper();

            string path = SaveCollegeCorrigendumLetterPdf(code, type);
            path = path.Replace("/", "\\");
            if (!string.IsNullOrEmpty(path))
            {
                return File(path, "application/pdf", path.Substring(path.LastIndexOf('\\') + 1));
            }
            return View();
        }

        private string SaveCollegeCorrigendumLetterPdf(string collegeCode, string type)
        {
            string fullPath = string.Empty;

            int Collegeid = db.jntuh_college.Where(C => C.collegeCode == collegeCode && C.isActive == true).Select(C => C.id).FirstOrDefault();
            var CollegeName = db.jntuh_college.Where(C => C.collegeCode == collegeCode && C.isActive == true).Select(C => C.collegeName).FirstOrDefault();
            string ECollegeid = UAAAS.Models.Utilities.EncryptString(Collegeid.ToString(), System.Web.Configuration.WebConfigurationManager.AppSettings["CryptoKey"]);


            //Set page size as A4
            var pdfDoc = new Document(PageSize.A4, 40, 40, 40, 40);

            // Create a new PdfWrite object, writing the output to a MemoryStream
            //open pdf
            var output = new MemoryStream();
            var writer = PdfWriter.GetInstance(pdfDoc, output);

            //int InspectionPhaseId = db.jntuh_inspection_phase.Where(p => p.isActive == true).Select(p => p.id).SingleOrDefault();

            string path = Server.MapPath("~/Content/PDFReports/AffiliationProceedings/");

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            fullPath = path + collegeCode + "-" + CollegeName + ".pdf";
            PdfWriter.GetInstance(pdfDoc, new FileStream(fullPath, FileMode.Create));

            //Open PDF Document to write data
            pdfDoc.Open();

            //Assign Html content in a string to write in PDF
            string contents = string.Empty;

            StreamReader sr;

            //Read file from server path
            sr = System.IO.File.OpenText(Server.MapPath("~/Content/CorrigendumLetter.html"));

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
            contents = contents.Replace("##SOCIETY_ADDRESS##", CollegeSocietyAddress);
            contents = contents.Replace("##COLLEGE_ADDRESS##", scheduleCollegeAddress);
            contents = contents.Replace("##COLLEGE_ADDRESS1##", scheduleCollegeAddress1);
            contents = contents.Replace("##LETTER_DATE##", DateTime.Now.ToString("dd-MM-yyy"));

            //var dates = db.jntuh_dates.Where(d => d.collegecode == collegeCode).Select(d => d).FirstOrDefault();
            //contents = contents.Replace("##APPLICATION_DATE##", ((DateTime)dates.applicationDate).ToString("dd-MM-yyy"));
            //contents = contents.Replace("##DEFICIENCY_REPORT_DATE##", ((DateTime)dates.deficiencyLetterDate).ToString("dd-MM-yyy"));
            //contents = contents.Replace("##HEARING_DATE##", dates.hearingDate != null ? ((DateTime)dates.hearingDate).ToString("dd-MM-yyy") : "");

            //var deficiencydates = db.jntuh_dates.Where(d => d.collegecode == collegeCode).Select(d => d).FirstOrDefault();
            var deficiencydates = db.jntuh_college_news.Where(d => d.collegeId == collegeid && d.title == "JNTUH - Deficiency Report for the Academic Year 2019-20-Reg.").Select(d => d).FirstOrDefault();
            var dates = db.jntuh_college_edit_status.Where(d => d.collegeId == collegeid && d.IsCollegeEditable == false).Select(d => d).FirstOrDefault();
            var Appeal = db.jntuh_appeal_college_edit_status.Where(d => d.collegeId == collegeid && d.IsCollegeEditable == false).Select(d => d).FirstOrDefault();
            if (Appeal != null)
            {
                contents = contents.Replace("##APPLICATION_DATE##", ((DateTime)Appeal.updatedOn).ToString("dd-MM-yyy") + ".");
            }
            else
            {
                contents = contents.Replace("##APPLICATION_DATE##", "NIL");
            }

            if (dates != null)
            {
                contents = contents.Replace("##116Submitted_DATE##", ((DateTime)dates.updatedOn).ToString("dd-MM-yyy") + ".");
            }
            else
            {
            }

            if (deficiencydates != null)
            {
                contents = contents.Replace("##DEFICIENCY_REPORT_DATE##", "28-04-2019 / 29-04-2019.");
            }
            else
            {
                contents = contents.Replace("##DEFICIENCY_REPORT_DATE##", "NIL");
            }

            string AppealSubmittedMessage = "Pursuant to the communication of deficiencies you have filed an appeal for reconsideration and the University reviewed the same or re inspection was conducted. Based on the above the University has accorded affiliation to the following courses.";
            string AppealNotSubmittedMessage = "Based on the above the University has accorded affiliation to the following courses.";
            contents = contents.Replace("##HEARING_DATE##", Appeal != null ? ((DateTime)Appeal.updatedOn).ToString("dd-MM-yyy") : "NIL");
            if (Appeal != null)
                contents = contents.Replace("##College_Message##", AppealSubmittedMessage);
            else
                contents = contents.Replace("##College_Message##", AppealNotSubmittedMessage);


            if (type == "All")
            {
                contents = contents.Replace("##COURSE_TABLE##", CollegeCoursesAll(collegeid));
            }
            else if (type == "AllClear")
            {
                string[] courses = CollegeCoursesAllClear(collegeid).Split('$');
                string defiencyRows = string.Empty;
                var pharmacyids = new[] { 6, 24, 27, 30, 34, 44, 47, 52, 54, 55, 58, 60, 65, 78, 90, 95, 97, 104, 105, 107, 110, 114, 117, 118, 120, 127, 135, 136, 139, 146, 150, 159, 169, 202, 204, 206, 213, 219, 234, 237, 253, 262, 263, 267, 283, 290, 295, 297, 298, 301, 302, 303, 313, 314, 315, 317, 318, 319, 320, 348, 353, 370, 376, 379, 384, 389, 392, 395, 410, 428, 436, 442, 445 };
                if (pharmacyids.Contains(collegeid))
                {
                    contents = contents.Replace("##COURSE_TABLE##", courses[0]);
                    contents = contents.Replace("##ASSESSMNET_TABLE##", courses[1]);
                    contents = contents.Replace("##TwentyFive_TABLE##", string.Empty);
                    contents = contents.Replace("##SI_TABLE##", string.Empty);
                    defiencyRows = courses[2];
                }
                else
                {
                    contents = contents.Replace("##COURSE_TABLE##", courses[0]);
                    contents = contents.Replace("##ASSESSMNET_TABLE##", courses[1]);
                    contents = contents.Replace("##TwentyFive_TABLE##", courses[2]);
                    defiencyRows = courses[4];
                }

                if (defiencyRows != "0")
                {
                    contents = contents.Replace("##HIDE_START##", "<!--");
                    contents = contents.Replace("##HIDE_END##", "-->");
                }
                else
                {
                    //contents = contents.Replace("##HIDE_START##", "<table border='1' style=''width:100%;text-align:center;padding-top:20px'><tr><td style='text-align:center'>NIL</td></tr></table>");
                    //contents = contents.Replace("##HIDE_START##", string.Empty);
                }
            }
            //
            var pharmacyids1 = new[] { 6, 24, 27, 30, 34, 44, 47, 52, 54, 55, 58, 60, 65, 78, 90, 95, 97, 104, 105, 107, 110, 114, 117, 118, 120, 127, 135, 136, 139, 146, 150, 159, 169, 202, 204, 206, 213, 219, 234, 237, 253, 262, 263, 267, 283, 290, 295, 297, 298, 301, 302, 303, 313, 314, 315, 317, 318, 319, 320, 348, 353, 370, 376, 379, 384, 389, 392, 395, 410, 428, 436, 442, 445 };
            if (pharmacyids1.Contains(collegeid))
            {
                contents = contents.Replace("##PHDSHOEARTAGE##", string.Empty);
                var PharmacyPrincipalDefcollegeids = new[] { 54, 58, 120, 253, 263, 370 };
                if (PharmacyPrincipalDefcollegeids.Contains(collegeid))
                    contents = contents.Replace("##PRINCIPALDEF##", "# Due to non-availability of Principal for the majority period during the Academic year 2018-19, the intake is reduced from 100 to 90 or 60 to 50 accordingly.");
                else
                    contents = contents.Replace("##PRINCIPALDEF##", string.Empty);
                contents = contents.Replace("##THREESTARCON##", string.Empty);
                contents = contents.Replace("##THREEHASCON##", string.Empty);
                contents = contents.Replace("##END##", "<br/><b>R</b>- Required Faculty,<b>A</b>-Available Faculty,<b>R.Ph.D</b>-Required Ph.D Faculty,<b>A.Ph.D</b>-Available Ph.D Faculty. <br/>* Any data discrepancies may be brought to the notice of the University within two days.<br/> The faculty requirement for Pharm.D / Pharm.D(PB) is included in B.Pharmacy course.");
                contents = contents.Replace("##ENDTWO##", string.Empty);
                contents = contents.Replace("##HASCOURSESNOTE##", string.Empty);
                //contents = contents.Replace("##DOLLERCOURSESNOTE##",string.Empty);

            }
            else
            {
                List<CollegeFacultyLabs> Courses = CollegeClearCourses(collegeid);
                var ishashcourses = Courses.Where(r => r.ishashcourses == true).Select(s => s.ishashcourses).FirstOrDefault();
                var isintakechange = Courses.Where(r => r.isintakechange == true).Select(s => s.isintakechange).FirstOrDefault();
                var principal = Courses.Where(r => r.DollerCourseIntake != 0).Select(s => s.DollerCourseIntake).FirstOrDefault();
                if (principal != 0)
                {
                    contents = contents.Replace("##PRINCIPALDEF##", "<br />## Due to non- availability of Principal for the majority period during the Academic year 2018-19, the intake is reduced to 50% of the normal JNTUH sanctioned intake (subject to a maximum of 60) in the course having highest sanctioned intake and also having least admissions during previous Academic Years.");
                }
                else
                {
                    contents = contents.Replace("##PRINCIPALDEF##", string.Empty);
                }
                if (ishashcourses == true)
                {
                    contents = contents.Replace("##HASCOURSESNOTE##", "<br /># In case, the admitted intake is less than 25% of the JNTUH sanctioned intake in any of the applied courses in all the previous three years but the admitted intake is at least 10 or above in one of the previous three years, then such courses are considered for affiliation with a minimum intake of 60, provided they meet other requirements as per norms. However, the faculty requirement is calculated based on sanctioned intake only.");
                }
                else
                {
                    contents = contents.Replace("##HASCOURSESNOTE##", string.Empty);
                }
                if (isintakechange == true)
                {
                    contents = contents.Replace("##PHDSHOEARTAGE##", "* In case, Ph.D qualified faculty members requirement is not met for the proposed intake, then the proposed intake is reduced proportionately as per the Ph.D possessing faculty members.");
                }
                else
                {
                    contents = contents.Replace("##PHDSHOEARTAGE##", string.Empty);
                }
                //contents = contents.Replace("##DOLLERCOURSESNOTE##", "<br />$ Intake is reduced to 50% of highest of normal sanctioned course intake (subject to a maximum of 60) due to non-availability of Principal for the majority period of the academic year 2017-18.")
                //contents = contents.Replace("##END##", "<b>R</b>- Required Faculty,<b>A</b>-Available Faculty,<b>R.Ph.D</b>-Required Ph.D Faculty,<b>A.Ph.D</b>-Available Ph.D Faculty. <br/><br/><p><b></b> II) The following Courses have either zero admissions due to non-grant of affiliation or admitted intake is less than 25% of JNTUH sanctioned intake for the previous three academic years. Hence,<b>&lsquo;No Affiliation is Granted&rsquo;</b> for these courses for the A.Y.2019-20 as per clause: 3.30 of affiliation regulations 2019-20.<br/></p>");
                //contents = contents.Replace("##ENDTWO##", "<br/>2. Any data discrepancies may be brought to the notice of the University within two days from the date of this letter.");
                //contents = contents.Replace("##ENDSI##", "<br/><p><b></b> III) Affiliation is not granted for the following courses due to no class work / acute faculty shortage / lab equipment shortage / no academic ambience during surprise inspection.<br/></p>");
            }

            //Read string contents using stream reader and convert html to parsed conent
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

        public List<CollegeFacultyLabs> CollegeClearCourses(int collegeId)
        {
            string courses = string.Empty;
            string twentyfivepercent = string.Empty;
            string sicourses = string.Empty;
            string assessments = string.Empty;
            string PharmDandPB = string.Empty;
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int prAy =
                db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
            List<CollegeIntakeExisting> collegeIntakeExisting = new List<CollegeIntakeExisting>();

            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);

            var degreeDetails = (from ie in db.jntuh_college_intake_existing
                                 join js in db.jntuh_specialization on ie.specializationId equals js.id
                                 join jd in db.jntuh_department on js.departmentId equals jd.id
                                 join deg in db.jntuh_degree on jd.degreeId equals deg.id
                                 join sh in db.jntuh_shift on ie.shiftId equals sh.id
                                 where (ie.collegeId == collegeId && ie.academicYearId == prAy)
                                 select new
                                 {
                                     ie.proposedIntake,
                                     js.specializationName,
                                     deg.degree,
                                     sh.shiftName,
                                     deg.degreeDisplayOrder,
                                     js.id
                                 }).OrderBy(d => d.degreeDisplayOrder).ThenBy(d => d.specializationName).ThenBy(d => d.shiftName).ToList();


            int rowCount = 0; int rowCounttwo = 0; int sirowCount = 0; int assessmentCount = 0; int assessmentCount1 = 0;


            var faculty = new List<CollegeFacultyLabs>();

            var integreatedIds = new[] { 9, 39, 42, 75, 140, 180, 332, 364, 235 };

            var pharmacyids = new[] { 6, 24, 27, 30, 34, 44, 47, 52, 54, 55, 58, 60, 65, 78, 90, 95, 97, 104, 105, 107, 110, 114, 117, 118, 120, 127, 135, 136, 139, 146, 150, 159, 169, 202, 204, 206, 213, 219, 234, 237, 252, 253, 262, 263, 267, 283, 290, 295, 297, 298, 301, 302, 303, 313, 314, 315, 317, 318, 319, 320, 348, 353, 370, 376, 379, 384, 389, 392, 395, 410, 428, 436, 442, 445, 448, 454 };

            if (pharmacyids.Contains(collegeId))
            {
                faculty = PharmacyDeficienciesInFaculty(collegeId);
            }
            else if (integreatedIds.Contains(collegeId))
            {
                var integratedpharmacyfaculty = PharmacyDeficienciesInFaculty(collegeId);
                var integratedbtechfaculty = DeficienciesInFaculty(collegeId);
                faculty = integratedbtechfaculty.Concat(integratedpharmacyfaculty).ToList();
            }
            else
            {
                faculty = DeficienciesInFaculty(collegeId);
            }
            List<CollegeFacultyLabs> labs = DeficienciesInLabsnew(collegeId);
            //DeficienciesInLabs(collegeId);

            var collegeFacultyLabs = labs.FirstOrDefault(i => i.Degree == "B.Pharmacy");
            if (collegeFacultyLabs != null)
            {
                var bphramcylabs = collegeFacultyLabs.LabsDeficiency;
                if (bphramcylabs != "NIL")
                {
                    foreach (var c in labs.Where(i => i.Degree == "M.Pharmacy").ToList())
                    {
                        c.LabsDeficiency = "YES";
                    }
                }
            }

            foreach (var l in labs)
            {
                if (l.Degree == "B.Tech" && l.LabsDeficiency != "NIL")
                {
                    labs.Where(i => i.Department == l.Department && i.Degree == "M.Tech" && i.LabsDeficiency == "NIL").ToList().ForEach(c => c.LabsDeficiency = "YES");
                }

            }



            List<CollegeFacultyLabs> clearedCourses = new List<CollegeFacultyLabs>();
            List<CollegeFacultyLabs> deficiencyCourses = new List<CollegeFacultyLabs>();
            List<string> deficiencyDepartments = new List<string>();
            List<string> deficiencyDepartments1 = new List<string>();

            #region Pharmacy Affliation Letter Start

            if (pharmacyids.Contains(collegeId))
            {

                courses += "<table border='1' cellpadding='3' cellspacing='0' width='100%' style='font-family: Times New Roman; font-size: 9px; font-weight: normal; background-color: #fff; margin: 0 auto;'>";
                courses += "<tr>";
                courses += "<th colspan='1' align='center'><b>S.No</b></th>";
                courses += "<th colspan='10'><b>Name of the Course</b></th>";
                courses += "<th colspan='2' align='center'><b>Intake</b></th>";
                courses += "</tr>";

                PharmDandPB += "<br/><table border='1' cellpadding='3' cellspacing='0' width='100%' style='font-family: Times New Roman; font-size: 9px; font-weight: normal; background-color: #fff; margin: 0 auto;'>";
                PharmDandPB += "<tr>";
                PharmDandPB += "<th colspan='1' align='center'><b>S.No</b></th>";
                PharmDandPB += "<th colspan='2' align='center'><b> Course</b></th>";
                PharmDandPB += "<th colspan='2' align='center'><b>Deficiency</b></th>";
                PharmDandPB += "</tr>";

                assessments += "<table border='1' cellpadding='3' cellspacing='0' width='100%' style='font-family: Times New Roman; font-size: 9px; font-weight: normal; background-color: #fff; margin: 0 auto;'>";
                assessments += "<tr>";
                assessments += "<th rowspan='2' colspan='1' align='center'><b>S.No</b></th>";
                assessments += "<th rowspan='2' colspan='5' align='center'><b>Course</b></th>";
                assessments += "<th rowspan='2' colspan='2' align='center'><b>Intake</b></th>";
                assessments += "<th colspan='6' align='center'><b>Faculty Shortage</b></th>";
                assessments += "<th colspan='5' align='center'><b>Lab Shortage</b></th>";
                assessments += "</tr>";
                assessments += "<tr>";
                assessments += "<th colspan='1' align='center'><b>R</b></th>";
                assessments += "<th colspan='1' align='center'><b>A</b></th>";
                assessments += "<th colspan='2' align='center'><b>R (Ph.D)</b></th>";
                assessments += "<th colspan='2' align='center'><b>A (Ph.D)</b></th>";
                assessments += "<th colspan='5' align='center'><b>Name of the Lab(s)</b></th>";
                assessments += "</tr>";

                labs =
                    labs.Where(e => e.Degree == "M.Pharmacy" || e.Degree == "B.Pharmacy" || e.Degree == "Pharm.D" || e.Degree == "Pharm.D PB")
                        .Select(e => e)
                        .ToList();

                faculty = faculty.Select(e => e).ToList();
                labs = labs.Select(e => e).ToList();

                //.Where(a => a.f.Deficiency == "NO" && a.f.PhdDeficiency != "YES" && a.l.LabsDeficiency == "NIL")

                int[] FSpecIds = faculty.Select(F => F.SpecializationId).ToArray();
                int[] LSpecIds = labs.Select(F => F.SpecializationId).ToArray();

                foreach (int id in FSpecIds)
                {
                    if (!LSpecIds.Contains(id))
                    {
                        CollegeFacultyLabs lab = new CollegeFacultyLabs();
                        lab.SpecializationId = id;
                        lab.Deficiency = "NIL";
                        lab.LabsDeficiency = "NIL";
                        labs.Add(lab);
                    }

                }


                clearedCourses = faculty.Join(labs, f => f.SpecializationId, l => l.SpecializationId, (f, l) => new { f, l })
                        .Where(a => a.f.Deficiency == "NO" && a.f.PhdDeficiency != "YES" && a.l.LabsDeficiency == "NIL")
                        .Select(a => new CollegeFacultyLabs
                        {
                            Degree = a.f.Degree,
                            Department = a.f.Department,
                            SpecializationId = a.f.SpecializationId,
                            Specialization = a.f.Specialization,
                            TotalIntake = a.f.TotalIntake,
                            Required = a.f.Required,
                            Available = a.f.Available,
                            isintakechange = a.l.isintakechange,
                            Deficiency = a.f.Deficiency,
                            Requiredphd = a.f.Requiredphd,
                            Availablephd = a.f.Availablephd,
                            PhdDeficiency = a.f.PhdDeficiency,
                            LabsDeficiency = a.l.LabsDeficiency
                        })
                        .OrderBy(a => a.DegreeDisplayOrder).ThenBy(a => a.Department).ThenBy(a => a.Specialization)
                        .ToList();

                deficiencyCourses = faculty.Join(labs, f => f.SpecializationId, l => l.SpecializationId, (f, l) => new { f, l })
                        .Where(a => (a.f.Deficiency == "YES" || a.f.PhdDeficiency == "YES") || a.l.LabsDeficiency != "NIL")
                        .Select(a => new CollegeFacultyLabs
                        {
                            Degree = a.f.Degree,
                            Department = a.f.Department,
                            SpecializationId = a.f.SpecializationId,
                            Specialization = a.f.Specialization,
                            TotalIntake = a.f.TotalIntake,
                            Required = a.f.Required,
                            Available = a.f.Available,
                            Requiredphd = a.f.Requiredphd,
                            Availablephd = a.f.Availablephd,
                            Deficiency = a.f.Deficiency,
                            isintakechange = a.l.isintakechange,
                            PhdDeficiency = a.f.PhdDeficiency,
                            LabsDeficiency = a.l.LabsDeficiency
                        })
                        .OrderBy(a => a.DegreeDisplayOrder).ThenBy(a => a.Department).ThenBy(a => a.Specialization)
                        .ToList();



                foreach (var course in deficiencyCourses.OrderBy(a => a.Degree).ToList())
                {
                    if (!deficiencyDepartments.Contains(course.Department))
                    {
                        deficiencyDepartments.Add(course.Department);
                    }

                    assessments += "<tr>";
                    assessments += "<td colspan='1' align='center'>" + (assessmentCount + 1) + "</td>";

                    course.Shift = "1";
                    if (course.Shift == "1")
                    {
                        assessments += "<td colspan='5' >" + course.Degree + " - " + course.Specialization + "</td>";
                    }
                    int intake2 = 0;
                    if (course.Degree == "B.Pharmacy")
                        intake2 = course.TotalIntake >= 100 ? 100 : course.TotalIntake;
                    else if (course.Degree == "M.Pharmacy")
                        intake2 = 15;
                    else
                        intake2 = course.TotalIntake;
                    assessments += "<td colspan='2' align='center'>" + intake2 + "</td>";

                    assessments += "<td colspan='1' align='center'>" + course.Required + "</td>";
                    if (course.Degree == "B.Pharmacy")
                    {
                        int Required = course.Required;
                        int Shortage = Required - course.Available;
                        assessments += "<td colspan='1' align='center'>" + course.Available + "</td>";
                        assessments += "<td colspan='2' align='center'>0</td>";
                        assessments += "<td colspan='2' align='center'>0</td>";
                    }

                    else
                    {
                        assessments += "<td colspan='1' align='center'>" + course.Available + "</td>";
                        assessments += "<td colspan='2' align='center'>" + course.Requiredphd + "</td>";
                        assessments += "<td colspan='2' align='center'>" + course.Availablephd + "</td>";
                    }


                    assessments += "<td colspan='5' align='center'>" + (course.LabsDeficiency.Equals("No Equipement Uploaded") ? course.Specialization + " Lab Deficiency" : course.LabsDeficiency) + "</td>";
                    assessments += "</tr>";

                    assessmentCount++;

                }


                foreach (var course in clearedCourses.OrderBy(a => a.Degree).ToList())
                {
                    string degreeType = db.jntuh_degree.Join(db.jntuh_degree_type, d => d.degreeTypeId, t => t.id, (d, t) => new { d, t })
                                                       .Where(a => a.d.degree == course.Degree).Select(a => a.t.degreeType).FirstOrDefault();

                    List<string> clearedDepartments = clearedCourses.Select(a => a.Department).ToList(); //.Where(a => a.DegreeType == "UG")

                    if (deficiencyDepartments.Contains(course.Department) && !clearedDepartments.Contains(course.Department))
                    {
                        assessments += "<tr>";
                        assessments += "<td colspan='1' align='center'>" + (assessmentCount + 1) + "</td>";

                        course.Shift = "1";
                        if (course.Shift == "1")
                        {
                            assessments += "<td colspan='5' >" + course.Degree + " - " + course.Specialization + "</td>";
                        }
                        int intake3 = 0;
                        if (course.Degree == "B.Pharmacy")
                            intake3 = course.TotalIntake >= 100 ? 100 : course.TotalIntake;
                        else if (course.Degree == "M.Pharmacy")
                            intake3 = 15;
                        else
                            intake3 = course.TotalIntake;
                        assessments += "<td colspan='2' align='center'>" + intake3 + "</td>";
                        assessments += "<td colspan='1' align='center'>" + course.Required + "</td>";



                        int Required = course.Required;
                        int Shortage = Required - course.Available;
                        assessments += "<td colspan='1' align='center'>" + course.Available + "</td>";
                        assessments += "<td colspan='2' align='center'>" + course.Requiredphd + "</td>";
                        assessments += "<td colspan='2' align='center'>" + course.Availablephd + "</td>";

                        assessments += "<td colspan='5' align='center'>" + (course.LabsDeficiency.Equals("No Equipement Uploaded") ? course.Specialization + " Lab Deficiency" : course.LabsDeficiency) + "</td>";
                        assessments += "</tr>";

                        assessmentCount++;


                    }
                    else
                    {
                        if (course.TotalIntake != 0)
                        {
                            courses += "<tr>";
                            courses += "<td colspan='1' align='center'>" + (rowCount + 1) + ".</td>";
                            course.Shift = "1";

                            if (course.Shift == "1")
                            {
                                courses += "<td colspan='10'>" + course.Degree + " - " + course.Specialization + "</td>";
                            }
                            if (course.Degree == "M.Pharmacy")
                                courses += "<td colspan='2' align='center'>15</td>";
                            else if (course.Degree == "B.Pharmacy")
                            {
                                int PharmacyIntake = (int)course.TotalIntake >= 100 ? 100 : (int)course.TotalIntake;
                                var PharmacyPrincipalDefcollegeIds = new[] { 54, 58, 120, 253, 263, 370 };
                                if (PharmacyPrincipalDefcollegeIds.Contains(collegeId))
                                    courses += "<td colspan='2' align='center'>" + PharmacyIntake + " #" + "</td>";
                                else
                                    courses += "<td colspan='2' align='center'>" + PharmacyIntake + "</td>";
                            }
                            else
                                courses += "<td colspan='2' align='center'>" + course.TotalIntake + " </td>";

                            courses += "</tr>";
                            rowCount++;
                        }
                    }
                }
                int totalCleared = clearedCourses.Count();
                int totalZeroIntake = clearedCourses.Where(c => c.TotalIntake == 0).Count();

                if (clearedCourses.Count() == 0 || totalCleared == totalZeroIntake)
                {
                    courses += "<tr>";
                    courses += "<td colspan='13' align='center'>NIL</td>";
                    courses += "</tr>";
                }
                //This Code is Written by 11
                if (deficiencyCourses.Count() == 0)
                {
                    assessments += "<tr>";
                    assessments += "<td colspan='24' align='center'>NIL</td>";
                    assessments += "</tr>";
                }
                assessments += "</table>" + "</br>";
                courses += "</table>";
                PharmDandPB += "</table>";


                if (deficiencyDepartments.Contains("B.Pharmacy") || deficiencyDepartments.Contains("M.Pharmacy"))
                {
                    if (deficiencyDepartments.Contains("Pharm.D") || deficiencyDepartments.Contains("Pharm.D PB"))
                    {
                        if (collegeId == 319 || collegeId == 127)
                        {

                        }
                        else
                        {
                            assessments += PharmDandPB;
                        }
                    }
                    //return courses + "$" + assessments + "$" + assessmentCount;
                }
                else
                {

                }


            }

            else
            {
                courses += "<table border='1' cellpadding='3' cellspacing='0' width='100%' style='font-family: Times New Roman; font-size: 9px; font-weight: normal; background-color: #fff; margin: 0 auto;'>";
                courses += "<tr>";
                courses += "<th colspan='1' align='center'><b>S.No</b></th>";
                courses += "<th colspan='10'><b>Name of the Course</b></th>";
                courses += "<th colspan='2' align='center'><b>Intake</b></th>";
                courses += "</tr>";

                assessments += "<table border='1' cellpadding='3' cellspacing='0' width='100%' style='font-family: Times New Roman; font-size: 9px; font-weight: normal; background-color: #fff; margin: 0 auto;'>";
                assessments += "<tr>";
                assessments += "<th rowspan='2' colspan='1' align='center'><b>S.No</b></th>";
                assessments += "<th rowspan='2' colspan='5'><b>Course</b></th>";
                assessments += "<th colspan='4' align='center'><b>Faculty Shortage </b></th>";
                assessments += "<th colspan='5' align='center'><b>Lab Shortage</b></th>";
                assessments += "</tr>";
                assessments += "<tr>";
                assessments += "<th colspan='1' align='center'><b>R</b></th>";
                assessments += "<th colspan='1' align='center'><b>A</b></th>";
                assessments += "<th colspan='1' align='center'><b>R.Ph.D</b></th>";
                assessments += "<th colspan='1' align='center'><b>A.Ph.D</b></th>";
                assessments += "<th colspan='5' align='center'><b>Name of the Lab(s)</b></th>";
                assessments += "</tr>";

                //twentyfivepercent += "<p style='font-family: Times New Roman; font-size: 9px;'><b>#</b> The following Courses have either Zero Admissions due to non-grant of Affiliation or Admitted intake is less than 25% of JNTUH Sanctioned intake for the previous three academic years.Hence SCA has recommended for <b>&lsquo;No Admission Status&rsquo;</b> for these courses for the A.Y.2019-20 as per clause: 3.30 of Affiliation Regulations 2019-20.<br/></p>";
                twentyfivepercent += "<table border='1' cellpadding='3' cellspacing='0' width='100%' style='font-family: Times New Roman; font-size: 9px; font-weight: normal; background-color: #fff; margin: 0 auto;'>";
                twentyfivepercent += "<tr>";
                twentyfivepercent += "<th colspan='1' align='center'><b>S.No</b></th>";
                twentyfivepercent += "<th colspan='10'><b>Name of the Course</b></th>";
                twentyfivepercent += "<th colspan='2' align='center'><b>Intake</b></th>";
                twentyfivepercent += "</tr>";
                //SI Courses Table
                sicourses += "<table border='1' cellpadding='3' cellspacing='0' width='100%' style='font-family: Times New Roman; font-size: 9px; font-weight: normal; background-color: #fff; margin: 0 auto;'>";
                sicourses += "<tr>";
                sicourses += "<th colspan='1' align='center'><b>S.No</b></th>";
                sicourses += "<th colspan='10'><b>Name of the Course</b></th>";
                sicourses += "<th colspan='2' align='center'><b>Proposed Intake</b></th>";
                sicourses += "</tr>";
                //Only Btech Affiliation Letters Filtering
                faculty = faculty.Where(e => e.Degree == "5-Year MBA(Integrated)" || e.Degree == "M.Tech" || e.Degree == "MBA" || e.Degree == "MCA" || e.Degree == "B.Tech" || e.Degree == "B.Pharmacy" || e.Degree == "Pharm.D" || e.Degree == "Pharm.D PB" || e.Degree == "M.Pharmacy").Select(e => e).ToList();
                labs = labs.Where(e => e.Degree == "5-Year MBA(Integrated)" || e.Degree == "M.Tech" || e.Degree == "MBA" || e.Degree == "MCA" || e.Degree == "B.Tech" || e.Degree == "B.Pharmacy" || e.Degree == "Pharm.D" || e.Degree == "Pharm.D PB" || e.Degree == "M.Pharmacy").Select(e => e).ToList();
                //Only MBA And MCA
                //faculty = faculty.Where(e => e.Degree == "MBA" || e.Degree == "MCA").Select(e => e).ToList();
                //labs = labs.Where(e => e.Degree == "MBA" || e.Degree == "MCA").Select(e => e).ToList();


                //Add New Code for Pharma.D Labs not Add in Lab Master
                int[] Humanities = new int[] { 31, 37, 42, 48, 155, 156, 157, 158 };
                int[] FSpecIds = faculty.Where(e => !Humanities.Contains(e.SpecializationId)).Select(F => F.SpecializationId).ToArray();//
                int[] LSpecIds = labs.Select(F => F.SpecializationId).ToArray();
                int[] PharmacySpecualizationIds = new int[] { 12, 13, 18, 19, 114, 115, 116, 117, 118, 119, 120, 121, 122, 123, 124, 167, 169, 170, 171, 172 };

                var pharmacycollegesids = new[]
                {
                    6, 24, 27, 30, 34, 44, 47, 52, 54, 55, 58, 60, 65, 78, 90, 95, 97, 104, 105, 107, 110, 114, 117, 118, 120, 127, 135, 136, 139, 146, 150, 159, 169, 202, 204, 206, 213, 219, 234, 237, 252, 253, 262, 263, 267, 283, 290, 295, 297, 298, 301, 302, 303, 313, 314, 315, 317, 318, 319, 320, 348, 353, 370, 376, 379, 384, 389, 392, 395, 410, 428, 436, 442, 445, 448, 454
                };
                //Some Colleges only Put Lab Deficiency NIL ,ex.College Uploaded New Courses.  B.Tech and B.pharmacy

                //if (pharmacycollegesids.Contains(collegeId))
                //{
                //    foreach (int id in FSpecIds)
                //    {
                //        //if (PharmacySpecualizationIds.Contains(id))
                //        // {
                //        if (!LSpecIds.Contains(id))
                //        {
                //            CollegeFacultyLabs lab = new CollegeFacultyLabs();
                //            lab.SpecializationId = id;
                //            lab.Deficiency = "NIL";
                //            lab.LabsDeficiency = "NIL";
                //            labs.Add(lab);
                //        }
                //        // }
                //    }
                //}
                if (integreatedIds.Contains(collegeId))
                {
                    foreach (int id in FSpecIds)
                    {
                        if (PharmacySpecualizationIds.Contains(id))
                        {
                            if (!LSpecIds.Contains(id))
                            {
                                CollegeFacultyLabs lab = new CollegeFacultyLabs();
                                lab.SpecializationId = id;
                                lab.Deficiency = "NIL";
                                lab.LabsDeficiency = "NIL";
                                labs.Add(lab);
                            }
                        }
                    }
                }

                //MBA Lab Deficiency NIL 


                //foreach (int id in FSpecIds)
                //{
                //   if (id==14)
                //    {
                //    if (!LSpecIds.Contains(id))
                //    {
                //        CollegeFacultyLabs lab = new CollegeFacultyLabs();
                //        lab.SpecializationId = id;
                //        lab.Deficiency = "NIL";
                //        lab.LabsDeficiency = "NIL";
                //        labs.Add(lab);
                //    }
                //    else
                //    {
                //        labs.Where(e=>e.SpecializationId==14).ToList().ForEach(e=>e.LabsDeficiency="NIL");
                //    }
                //    }
                //}


                clearedCourses = faculty.Join(labs, f => f.SpecializationId, l => l.SpecializationId, (f, l) => new { f, l })
                                        .Where(a => a.f.ispercentage == false && a.f.Deficiency == "NO" && a.f.PhdDeficiency != "YES" && a.l.LabsDeficiency == "NIL")
                                        .Select(a => new CollegeFacultyLabs
                                        {
                                            Degree = a.f.Degree,
                                            Department = a.f.Department,
                                            SpecializationId = a.f.SpecializationId,
                                            Specialization = a.f.Specialization,
                                            TotalIntake = a.f.TotalIntake,
                                            Required = a.f.Required,
                                            Available = a.f.Available,
                                            Deficiency = a.f.Deficiency,
                                            Requiredphd = a.f.Requiredphd,
                                            Availablephd = a.f.Availablephd,
                                            ShiftId = a.f.ShiftId,
                                            PhdDeficiency = a.f.PhdDeficiency,
                                            LabsDeficiency = a.l.LabsDeficiency,
                                            isintakechange = a.f.isintakechange,
                                            ispercentage = a.f.ispercentage,
                                            DollerCourseIntake = a.f.DollerCourseIntake,
                                            ishashcourses = a.f.ishashcourses,
                                            isnewincreasecourse = a.f.isnewincreasecourse,
                                            isnewreducecourse = a.f.isnewreducecourse,
                                            isoldincreasecourse = a.f.isoldincreasecourse,
                                            isoldreducecourse = a.f.isoldreducecourse
                                        })
                                        .OrderBy(a => a.DegreeDisplayOrder).ThenBy(a => a.Department).ThenBy(a => a.Specialization)
                                        .ToList();

                //clearedCourses = faculty.Join(labs, f => f.SpecializationId, l => l.SpecializationId, (f, l) => new { f, l })
                //                        .Where(a => a.f.Deficiency == "NO" && a.f.PhdDeficiency != "YES" && a.l.LabsDeficiency == "NIL")
                //                        .Select(a => new CollegeFacultyLabs
                //                        {
                //                            Degree = a.f.Degree,
                //                            Department = a.f.Department,
                //                            SpecializationId = a.f.SpecializationId,
                //                            Specialization = a.f.Specialization,
                //                            TotalIntake = a.f.TotalIntake,
                //                            Required = a.f.Required,
                //                            Available = a.f.Available,
                //                            Deficiency = a.f.Deficiency,
                //                            Requiredphd = a.f.Requiredphd,
                //                            Availablephd = a.f.Availablephd,
                //                            PhdDeficiency = a.f.PhdDeficiency,
                //                            LabsDeficiency = a.l.LabsDeficiency
                //                        })
                //                        .OrderBy(a => a.DegreeDisplayOrder).ThenBy(a => a.Department).ThenBy(a => a.Specialization)
                //                        .ToList();

                deficiencyCourses = faculty.Join(labs, f => f.SpecializationId, l => l.SpecializationId, (f, l) => new { f, l })
                                            .Where(a => (a.f.ispercentage == true || a.f.Deficiency == "YES" || a.f.PhdDeficiency == "YES") || a.l.LabsDeficiency != "NIL")
                                            .Select(a => new CollegeFacultyLabs
                                            {
                                                Degree = a.f.Degree,
                                                Department = a.f.Department,
                                                SpecializationId = a.f.SpecializationId,
                                                Specialization = a.f.Specialization,
                                                TotalIntake = a.f.TotalIntake,
                                                Required = a.f.Required,
                                                Available = a.f.Available,
                                                Requiredphd = a.f.Requiredphd,
                                                Availablephd = a.f.Availablephd,
                                                Deficiency = a.f.Deficiency,
                                                ShiftId = a.f.ShiftId,
                                                DollerCourseIntake = a.f.DollerCourseIntake,
                                                PhdDeficiency = a.f.PhdDeficiency,
                                                LabsDeficiency = a.l.LabsDeficiency,
                                                isintakechange = a.f.isintakechange,
                                                ispercentage = a.f.ispercentage,
                                                ishashcourses = a.f.ishashcourses,
                                                isnewincreasecourse = a.f.isnewincreasecourse,
                                                isnewreducecourse = a.f.isnewreducecourse,
                                                isoldincreasecourse = a.f.isoldincreasecourse,
                                                isoldreducecourse = a.f.isoldreducecourse
                                            })
                                            .OrderBy(a => a.DegreeDisplayOrder).ThenBy(a => a.Department).ThenBy(a => a.Specialization)
                                            .ToList();




                foreach (var course in deficiencyCourses.Where(d => d.ispercentage != true).OrderBy(a => a.Degree).ToList())
                {
                    if (!deficiencyDepartments.Contains(course.Department))
                    {
                        deficiencyDepartments.Add(course.Department);
                    }

                    assessments += "<tr>";
                    assessments += "<td colspan='1' align='center'>" + (assessmentCount + 1) + "</td>";

                    //course.Shift = "1";
                    if (course.ShiftId == 1)
                    {
                        assessments += "<td colspan='5' >" + course.Degree + " - " + course.Specialization + "</td>";
                    }
                    if (course.ShiftId == 2)
                    {
                        assessments += "<td colspan='5' >" + course.Degree + " - " + course.Specialization + " - 2</td>";
                    }

                    //assessments += "<td colspan='2' align='center'>" + (course.Required - course.Available) + "</td>";
                    assessments += "<td colspan='1' align='center'>" + course.Required + "</td>";
                    assessments += "<td colspan='1' align='center'>" + course.Available + "</td>";
                    //assessments += "<td colspan='2' align='center'>" + course.PhdDeficiency + "</td>";
                    assessments += "<td colspan='1' align='center'>" + course.Requiredphd + "</td>";
                    assessments += "<td colspan='1' align='center'>" + course.Availablephd + "</td>";
                    assessments += "<td colspan='5' align='center'>" + (course.LabsDeficiency.Equals("No Equipement Uploaded") ? course.Specialization + " Lab Deficiency" : course.LabsDeficiency) + "</td>";
                    assessments += "</tr>";

                    assessmentCount++;

                    //old Code Commented By Narayan 
                    //var secondshift = db.jntuh_college_intake_existing.Where(e => e.collegeId == collegeId && e.specializationId == course.SpecializationId && e.shiftId == 2).Select(e => e).FirstOrDefault();

                    //if (secondshift != null)
                    //{
                    //    assessments += "<tr>";
                    //    assessments += "<td colspan='1' align='center'>" + (assessmentCount + 1) + "</td>";

                    //    course.Shift = "2";
                    //    if (course.Shift == "2")
                    //    {
                    //        assessments += "<td colspan='5' >" + course.Degree + " - " + course.Specialization + " - 2</td>";
                    //    }

                    //    //assessments += "<td colspan='2' align='center'>" + (course.Required - course.Available) + "</td>";
                    //    assessments += "<td colspan='1' align='center'>" + course.Required + "</td>";
                    //    assessments += "<td colspan='1' align='center'>" + course.Available + "</td>";
                    //    //assessments += "<td colspan='2' align='center'>" + course.PhdDeficiency + "</td>";
                    //    assessments += "<td colspan='1' align='center'>" + course.Requiredphd + "</td>";
                    //    assessments += "<td colspan='1' align='center'>" + course.Availablephd + "</td>";
                    //    assessments += "<td colspan='5' align='center'>" + (course.LabsDeficiency.Equals("No Equipement Uploaded") ? course.Specialization + " Lab Deficiency" : course.LabsDeficiency) + "</td>";
                    //    assessments += "</tr>";

                    //    assessmentCount++;
                    //}

                    //}
                }

                foreach (var course in clearedCourses.Where(a => a.DollerCourseIntake != 1).OrderBy(a => a.Degree).ToList())
                {
                    string degreeType = db.jntuh_degree.Join(db.jntuh_degree_type, d => d.degreeTypeId, t => t.id, (d, t) => new { d, t })
                                                       .Where(a => a.d.degree == course.Degree).Select(a => a.t.degreeType).FirstOrDefault();

                    List<string> clearedDepartments = clearedCourses.Select(a => a.Department).ToList(); //.Where(a => a.DegreeType == "UG")

                    if (deficiencyDepartments.Contains(course.Department) && !clearedDepartments.Contains(course.Department))
                    {
                        assessments += "<tr>";
                        assessments += "<td colspan='1' align='center'>" + (assessmentCount + 1) + "</td>";

                        //course.Shift = "1";
                        if (course.ShiftId == 1)
                        {
                            assessments += "<td colspan='5' >" + course.Degree + " - " + course.Specialization + "</td>";
                        }
                        else
                        {
                            assessments += "<td colspan='5' >" + course.Degree + " - " + course.Specialization + " - 2</td>";
                        }

                        //assessments += "<td colspan='2' align='center'>" + (course.Required - course.Available) + "</td>";
                        assessments += "<td colspan='1' align='center'>" + course.Required + "</td>";
                        assessments += "<td colspan='1' align='center'>" + course.Available + "</td>";
                        //assessments += "<td colspan='2' align='center'>" + course.PhdDeficiency + "</td>";
                        assessments += "<td colspan='1' align='center'>" + course.Requiredphd + "</td>";
                        assessments += "<td colspan='1' align='center'>" + course.Availablephd + "</td>";
                        assessments += "<td colspan='5' align='center'>" + (course.LabsDeficiency.Equals("No Equipement Uploaded") ? course.Specialization + " Lab Deficiency" : course.LabsDeficiency) + "</td>";
                        assessments += "</tr>";

                        assessmentCount++;

                        //var secondshift = db.jntuh_college_intake_existing.Where(e => e.collegeId == collegeId && e.specializationId == course.SpecializationId && e.shiftId == 2).Select(e => e).FirstOrDefault();

                        //if (secondshift != null)
                        //{
                        //    assessments += "<tr>";
                        //    assessments += "<td colspan='1' align='center'>" + (assessmentCount + 1) + "</td>";

                        //    course.Shift = "2";
                        //    if (course.Shift == "2")
                        //    {
                        //        assessments += "<td colspan='5' >" + course.Degree + " - " + course.Specialization + " - 2</td>";
                        //    }

                        //    //assessments += "<td colspan='2' align='center'>" + (course.Required - course.Available) + "</td>";
                        //    assessments += "<td colspan='1' align='center'>" + course.Required + "</td>";
                        //    assessments += "<td colspan='1' align='center'>" + course.Available + "</td>";
                        //    //assessments += "<td colspan='2' align='center'>" + course.PhdDeficiency + "</td>";
                        //    assessments += "<td colspan='1' align='center'>" + course.Requiredphd + "</td>";
                        //    assessments += "<td colspan='1' align='center'>" + course.Availablephd + "</td>";
                        //    assessments += "<td colspan='5' align='center'>" + (course.LabsDeficiency.Equals("No Equipement Uploaded") ? course.Specialization + " Lab Deficiency" : course.LabsDeficiency) + "</td>";
                        //    assessments += "</tr>";

                        //    assessmentCount++;
                        //}

                    }
                    else
                    {
                        if (course.TotalIntake != 0)
                        {
                            courses += "<tr>";
                            courses += "<td colspan='1' align='center'>" + (rowCount + 1) + ".</td>";
                            //course.Shift = "1";

                            if (course.ShiftId == 1)
                            {
                                courses += "<td colspan='10'>" + course.Degree + " - " + course.Specialization + "</td>";
                            }
                            else if (course.ShiftId == 2)
                            {
                                courses += "<td colspan='10'>" + course.Degree + " - " + course.Specialization +
                                           " - 2</td>";
                            }
                            else
                            {
                                courses += "<td colspan='10'>" + course.Degree + " - " + course.Specialization + "</td>";
                            }
                            if (course.Degree == "B.Pharmacy")
                            {
                                int intake1 = course.TotalIntake >= 100 ? 100 : (int)course.TotalIntake;
                                courses += "<td colspan='2' align='center'>" + intake1 + "</td>";
                            }
                            else if (course.Degree == "M.Pharmacy")
                                courses += "<td colspan='2' align='center'>15</td>";
                            else
                            {
                                if (course.isnewincreasecourse == true || course.isoldincreasecourse == true)
                                {
                                    courses += "<td colspan='2' align='center'>" + course.TotalIntake + " ###" + "</td>";
                                }
                                else if (course.isoldreducecourse == true || course.isnewreducecourse == true)
                                {
                                    courses += "<td colspan='2' align='center'>" + course.TotalIntake + " ***" + "</td>";
                                }
                                else if (course.ishashcourses == true)
                                {
                                    if (course.DollerCourseIntake != 0)
                                    {
                                        courses += "<td colspan='2' align='center'>" + course.DollerCourseIntake + " #" +
                                                   " ##" +
                                                   "</td>";
                                    }
                                    else
                                    {
                                        if (course.TotalIntake > 60)
                                        {
                                            courses += "<td colspan='2' align='center'>" + "60" + " #" + "</td>";
                                        }
                                        else
                                        {
                                            courses += "<td colspan='2' align='center'>" + course.TotalIntake + " #" + "</td>";
                                        }
                                    }


                                }
                                else if (course.isintakechange == true)
                                {
                                    if (course.DollerCourseIntake != 0)
                                    {
                                        courses += "<td colspan='2' align='center'>" + course.DollerCourseIntake + " *" + " ##" +
                                                   "</td>";
                                    }
                                    else
                                    {
                                        courses += "<td colspan='2' align='center'>" + course.TotalIntake + " *" + "</td>";
                                    }

                                }
                                //else if (course.DollerCourseIntake != 0)
                                //{
                                //    courses += "<td colspan='2' align='center'>" + course.DollerCourseIntake + "$" + "</td>";
                                //}
                                else
                                {
                                    if (course.DollerCourseIntake != 0)
                                    {
                                        courses += "<td colspan='2' align='center'>" + course.DollerCourseIntake + " ##" + "</td>";
                                    }
                                    else
                                    {
                                        courses += "<td colspan='2' align='center'>" + course.TotalIntake + "</td>";
                                    }

                                }
                            }
                            courses += "</tr>";

                            rowCount++;



                        }
                    }
                }
                foreach (var coursetwo in deficiencyCourses.Where(w => w.ispercentage == true).OrderBy(a => a.Degree).ToList())
                {
                    twentyfivepercent += "<tr>";
                    twentyfivepercent += "<td colspan='1' align='center'>" + (rowCounttwo + 1) + ".</td>";
                    //course.Shift = "1";

                    if (coursetwo.ShiftId == 1)
                    {
                        twentyfivepercent += "<td colspan='10'>" + coursetwo.Degree + " - " + coursetwo.Specialization + "</td>";
                    }
                    else if (coursetwo.ShiftId == 2)
                    {
                        twentyfivepercent += "<td colspan='10'>" + coursetwo.Degree + " - " + coursetwo.Specialization +
                                   " - 2</td>";
                    }
                    else
                    {
                        twentyfivepercent += "<td colspan='10'>" + coursetwo.Degree + " - " + coursetwo.Specialization + "</td>";
                    }
                    twentyfivepercent += "<td colspan='2' align='center'>" + coursetwo.TotalIntake + "</td>";
                    twentyfivepercent += "</tr>";
                    rowCounttwo++;

                }
                foreach (var sicourseslist in clearedCourses.Where(w => w.DollerCourseIntake == 1).OrderBy(a => a.Degree).ToList())
                {
                    sicourses += "<tr>";
                    sicourses += "<td colspan='1' align='center'>" + (sirowCount + 1) + ".</td>";
                    //course.Shift = "1";

                    if (sicourseslist.ShiftId == 1)
                    {
                        sicourses += "<td colspan='10'>" + sicourseslist.Degree + " - " + sicourseslist.Specialization + "</td>";
                    }
                    else if (sicourseslist.ShiftId == 2)
                    {
                        sicourses += "<td colspan='10'>" + sicourseslist.Degree + " - " + sicourseslist.Specialization +
                                   " - 2</td>";
                    }
                    else
                    {
                        sicourses += "<td colspan='10'>" + sicourseslist.Degree + " - " + sicourseslist.Specialization + "</td>";
                    }
                    sicourses += "<td colspan='2' align='center'>" + sicourseslist.TotalIntake + "</td>";
                    sicourses += "</tr>";
                    sirowCount++;

                }

                int totalCleared = clearedCourses.Count();
                int totalZeroIntake = clearedCourses.Where(c => c.TotalIntake == 0).Count();
                if (deficiencyCourses.Where(w => w.ispercentage == false).Count() == 0)
                {
                    assessments += "<tr>";
                    assessments += "<td colspan='24' align='center'>NIL</td>";
                    assessments += "</tr>";
                }
                if (deficiencyCourses.Where(w => w.ispercentage == true).Count() == 0)
                {
                    twentyfivepercent += "<tr>";
                    twentyfivepercent += "<td colspan='13' align='center'>NIL</td>";
                    twentyfivepercent += "</tr>";
                }
                if (clearedCourses.Count() == 0 || totalCleared == totalZeroIntake)
                {
                    courses += "<tr>";
                    courses += "<td colspan='13' align='center'>NIL</td>";
                    courses += "</tr>";
                }
                if (clearedCourses.Where(a => a.DollerCourseIntake == 1).Count() == 0)
                {
                    sicourses += "<tr>";
                    sicourses += "<td colspan='13' align='center'>NIL</td>";
                    sicourses += "</tr>";
                }
                assessments += "</table>";
                courses += "</table>";
                sicourses += "</table>";
                twentyfivepercent += "</table>";


            }
            return clearedCourses.Where(a => a.DollerCourseIntake != 1).OrderBy(a => a.Degree).ToList();
            #endregion Pharmacy Affliation Letter End
        }
        #endregion

        public string CollegeCoursesAll(int collegeId)
        {
            string courses = string.Empty;
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int prAy =
                db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
            List<CollegeIntakeExisting> collegeIntakeExisting = new List<CollegeIntakeExisting>();

            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);

            var degreeDetails = (from ie in db.jntuh_approvedadmitted_intake
                                 join js in db.jntuh_specialization on ie.SpecializationId equals js.id
                                 join jd in db.jntuh_department on js.departmentId equals jd.id
                                 join deg in db.jntuh_degree on jd.degreeId equals deg.id
                                 join sh in db.jntuh_shift on ie.ShiftId equals sh.id
                                 where (ie.collegeId == collegeId && ie.AcademicYearId == prAy)
                                 select new
                                 {
                                     ie.ApprovedIntake,
                                     js.specializationName,
                                     deg.degree,
                                     sh.shiftName,
                                     deg.degreeDisplayOrder,
                                     js.id
                                 }).OrderBy(d => d.degreeDisplayOrder).ThenBy(d => d.specializationName).ThenBy(d => d.shiftName).ToList();
            courses += "<table border='1' cellpadding='3' cellspacing='0' width='100%' style='font-family: Times New Roman; font-size: 9px; font-weight: normal; background-color: #fff; margin: 0 auto;'>";
            courses += "<tr>";
            courses += "<th colspan='1' align='center'><b>S.No</b></th>";
            courses += "<th colspan='10' align='center'><b>Name of the Course(s)</b></th>";
            courses += "<th colspan='2' align='center'><b>Intake</b></th>";
            courses += "</tr>";

            int rowCount = 0;

            foreach (var item in degreeDetails)
            {
                ////faculty
                //string[] fStatus = NoFacultyDeficiencyCourse(collegeId, item.id).Split('$');


                //string fFlag = fStatus[0];
                //string intake = fStatus[1];

                ////labs
                //string lStatus = NoLabDeficiencyCourse(collegeId, item.id);

                //string lFlag = lStatus;

                //if (fFlag == "YES" && lFlag == "YES")
                //{
                courses += "<tr>";
                courses += "<td colspan='1' align='center'>" + (rowCount + 1) + ".</td>";

                if (item.shiftName == "1")
                {
                    courses += "<td colspan='10'>" + item.degree + " - " + item.specializationName + "</td>";
                }
                else
                {
                    courses += "<td colspan='10'>" + item.degree + " - " + item.specializationName + " - " + item.shiftName + "</td>";
                }

                courses += "<td colspan='2' align='center'>" + item.ApprovedIntake + "</td>";
                courses += "</tr>";

                rowCount++;
                //}

            }

            courses += "</table>";

            return courses;
        }

        public int GetIntakeBasedOnPhd(int phdcount)
        {
            int intake = 0;
            if (phdcount == 0)
            {
                intake = 60;
            }
            else if (phdcount == 1)
            {
                intake = 120;
            }
            else if (phdcount == 2)
            {
                intake = 180;
            }
            else if (phdcount == 3)
            {
                intake = 240;
            }
            else if (phdcount == 4)
            {
                intake = 300;
            }
            else if (phdcount == 5)
            {
                intake = 360;
            }
            else if (phdcount == 6)
            {
                intake = 420;
            }
            return intake;
        }

        public List<CollegeFacultyLabs> DeficienciesInLabs(int? collegeID)
        {
            string labs = string.Empty;

            labs += "<table width='100%' border='0' cellpadding='5' cellspacing='0'>";
            labs += "<tr>";
            labs += "<td align='left'><b><u>Deficiencies in Laboratory</u></b> (Department/ Specialization Wise):";
            labs += "</tr>";
            labs += "</table>";

            List<Lab> labsCount = collegeLabs(collegeID);

            var deficiencies = labsCount.GroupBy(l => new { l.degree, l.department, l.specializationName, l.specializationId }).Select(l => new { degree = l.Key.degree, department = l.Key.department, specializationid = l.Key.specializationId, specializationName = l.Key.specializationName, deficiencies = string.Empty }).ToList();

            labs += "<table width='100%' border='1' cellpadding='3' cellspacing='0' style='border-color: #ccc;'>";
            labs += "<tr>";
            labs += "<th style='text-align: center; width: 5%;'>S.No</th>";
            labs += "<th style='text-align: left; width: 10%;'>Degree</th>";
            labs += "<th style='text-align: left; width: 10%;'>Department</th>";
            labs += "<th style='text-align: left; width: 20%;'>Specialization</th>";
            labs += "<th style='text-align: center; '>Names of the Labs with Deficiency (Details Annexed)</th>";
            labs += "</tr>";

            var labMaster = db.jntuh_lab_master.ToList();
            //var collegeLabMaster = db.jntuh_college_laboratories_dataentry2.Where(l => l.CollegeID == collegeID).ToList();
            var jntuh_college_laboratories = db.jntuh_college_laboratories.Where(i => i.CollegeID == collegeID).ToList();
            List<CollegeFacultyLabs> lstFaculty = new List<CollegeFacultyLabs>();

            foreach (var item in deficiencies)
            {

                labs += "<tr>";
                labs += "<td style='text-align: center; width: 5%; '>" + (deficiencies.IndexOf(item) + 1) + "</td>";
                labs += "<td style=''>" + item.degree + "</td>";
                labs += "<td style=''>" + item.department + "</td>";
                labs += "<td style=''>" + item.specializationName + "</td>";

                string degreeType = db.jntuh_degree.Where(d => d.degree == item.degree).Select(d => d.jntuh_degree_type.degreeType).FirstOrDefault();

                var labsWithDeficiency = labsCount.Where(l => l.degree == item.degree && l.department == item.department && l.specializationName == item.specializationName && l.deficiency == true)
                    .Select(l => new { Deficiency = l.year + "-" + l.Semester + "-" + l.Labcode.Replace("-", "$") + "-" + l.specializationId }).Select(l => l.Deficiency).ToList();

                List<string> defs = new List<string>();
                labsWithDeficiency.ForEach(l =>
                {
                    if (l.Equals("No Equipement Uploaded"))
                    {
                        defs.Add(l);
                    }
                    else
                    {
                        string[] strLab = l.Split('-');

                        int specializationid = Convert.ToInt32(strLab[3]);
                        int year = Convert.ToInt32(strLab[0]);
                        int semester = Convert.ToInt32(strLab[1]);
                        string labCode = strLab[2].Replace("$", "-");

                        var requiredLabs = labMaster.Where(m => m.SpecializationID == specializationid && m.Year == year && m.Semester == semester && m.Labcode == labCode).Select(m => m.id).ToList();
                        int requiredCount = requiredLabs.Count();
                        //int availableCount = collegeLabMaster.Where(m => requiredLabs.Contains(m.EquipmentID)).Count();

                        int[] labmasterids = labMaster.Where(m => m.SpecializationID == specializationid && m.Year == year && m.Semester == semester && m.Labcode == labCode).Select(m => m.id).Distinct().ToArray();
                        int[] collegelabequipmentids = jntuh_college_laboratories.Where(i => labmasterids.Contains(i.EquipmentID) && i.EquipmentNo == 1).Select(i => i.id).Distinct().ToArray();

                        if (labmasterids.Count() != collegelabequipmentids.Count())//&& labCode!="14LAB"
                        {
                            string labName = labMaster.Where(m => m.SpecializationID == specializationid && m.Year == year && m.Semester == semester && m.Labcode == labCode).Select(m => m.LabName).FirstOrDefault();
                            if (!string.IsNullOrEmpty(labName))
                                defs.Add(year + "-" + semester + "-" + labName);
                            else
                                defs.Add(null);
                        }
                    }
                });

                labs += "<td style='; text-align: center'>" + (labsWithDeficiency.Count() == 0 ? "NIL" : (defs.Count() == 0 ? "NIL" : String.Join(", ", defs))) + "</td>";
                labs += "</tr>";

                CollegeFacultyLabs newFaculty = new CollegeFacultyLabs();
                newFaculty.Degree = item.degree;
                newFaculty.Department = item.department;
                newFaculty.Specialization = item.specializationName;
                newFaculty.SpecializationId = item.specializationid;
                newFaculty.LabsDeficiency = (labsWithDeficiency.Count() == 0 ? "NIL" : (defs.Count() == 0 ? "NIL" : String.Join(", ", defs)));

                lstFaculty.Add(newFaculty);
            }






            labs += "</table>";

            return lstFaculty;
        }

        public List<CollegeFacultyLabs> DeficienciesInLabsnew(int? collegeID)
        {
            List<Lab> lstlaboratories = new List<Lab>();
            List<Lab> collegeLabMaster = new List<Lab>();
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int prAy =
                db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
            List<CollegeFacultyLabs> lstFaculty = new List<CollegeFacultyLabs>();
            List<int> specializationIds = db.jntuh_college_intake_existing.Where(e => e.collegeId == collegeID && e.academicYearId == prAy && e.courseStatus != "Closure" && e.proposedIntake != 0).Select(e => e.specializationId).Distinct().ToList();
            List<int> NewspecializationIds = db.jntuh_college_intake_existing.Where(e => e.collegeId == collegeID && e.academicYearId == prAy && e.courseStatus != "Closure" && e.courseStatus == "New" && e.proposedIntake != 0).Select(e => e.specializationId).Distinct().ToList();

            var DepartmentsData = db.jntuh_specialization.Where(e => e.isActive == true && specializationIds.Contains(e.id)).Select(e => e.departmentId).Distinct().ToArray();

            var DegreeIds = db.jntuh_department.Where(e => e.isActive == true && DepartmentsData.Contains(e.id)).Select(e => e.degreeId).ToArray();

            if (DegreeIds.Contains(4))
            {
                specializationIds.Add(39);
            }

            string CollegeAffiliationStatus = db.jntuh_college_affiliation.Where(u => u.collegeId == collegeID && u.affiliationTypeId == 7).Select(u => u.affiliationStatus).FirstOrDefault();

            if (CollegeAffiliationStatus == "Yes")
            {
                collegeLabMaster = db.jntuh_lab_master.AsNoTracking().Where(l => specializationIds.Contains(l.SpecializationID) && l.CollegeId == collegeID && !l.EquipmentName.Contains("Desirable") && !l.EquipmentName.Contains("optional"))
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
                                                          LabName = l.LabName
                                                      }).OrderBy(l => l.degreeDisplayOrder).ThenBy(l => l.department).ThenBy(l => l.specializationName).ThenBy(l => l.year).ThenBy(l => l.Semester).Distinct().ToList();
            }
            else if (CollegeAffiliationStatus == "No" || CollegeAffiliationStatus == null)
            {
                if (specializationIds.Contains(33) || specializationIds.Contains(43))
                {
                    collegeLabMaster = db.jntuh_lab_master.AsNoTracking()
                                                          .Where(l => specializationIds.Contains(l.SpecializationID) && l.CollegeId == null && !l.EquipmentName.Contains("Desirable") && !l.EquipmentName.Contains("optional"))
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
                                                              LabName = l.LabName
                                                          }).OrderBy(l => l.degreeDisplayOrder).ThenBy(l => l.department).ThenBy(l => l.specializationName).ThenBy(l => l.year).ThenBy(l => l.Semester).Distinct().ToList();
                }
                else
                {
                    collegeLabMaster = db.jntuh_lab_master.AsNoTracking()
                                                          .Where(l => specializationIds.Contains(l.SpecializationID) && l.CollegeId == null && !l.EquipmentName.Contains("Desirable") && !l.EquipmentName.Contains("optional") && l.Labcode != "PH105BS")
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
                                                              LabName = l.LabName
                                                          }).OrderBy(l => l.degreeDisplayOrder).ThenBy(l => l.department).ThenBy(l => l.specializationName).ThenBy(l => l.year).ThenBy(l => l.Semester).Distinct().ToList();
                }

            }

            int[] labequipmentIds = collegeLabMaster.Select(i => i.EquipmentID).ToArray();

            int[] clgequipmentIDs = db.jntuh_college_laboratories.Where(l => l.CollegeID == collegeID && labequipmentIds.Contains(l.EquipmentID) && l.isActive == true).Select(i => i.EquipmentID).ToArray();

            int[] DeficiencyEquipmentIds = collegeLabMaster.Where(l => !clgequipmentIDs.Contains((int)l.EquipmentID)).Select(e => e.EquipmentID).ToArray();


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
                lstlabs.EquipmentName = item.EquipmentName;
                lstlabs.EquipmentNo = 1;
                lstlabs.degreeDisplayOrder = item.degreeDisplayOrder;
                if (DeficiencyEquipmentIds.Contains(item.EquipmentID))
                {
                    lstlabs.deficiency = true;
                }
                else
                {
                    lstlabs.deficiency = null;
                    lstlabs.id = 0;
                }
                lstlaboratories.Add(lstlabs);
            }

            lstlaboratories = lstlaboratories.OrderBy(l => l.degreeDisplayOrder).ThenBy(l => l.department).ThenBy(l => l.specializationName).ThenBy(l => l.year).ThenBy(l => l.Semester).Distinct().ToList();


            lstlaboratories = lstlaboratories.GroupBy(l => new { l.deficiency, l.year, l.Semester, l.Labcode, l.specializationId }).Select(s => s.First()).ToList();




            var deficiencies = lstlaboratories.GroupBy(l => new { l.degree, l.department, l.specializationName, l.specializationId }).Select(l => new { degree = l.Key.degree, department = l.Key.department, specializationid = l.Key.specializationId, specializationName = l.Key.specializationName, deficiencies = string.Empty }).ToList();

            var jntuh_college_laboratories = db.jntuh_college_laboratories.Where(i => i.CollegeID == collegeID && i.isActive == true).ToList();






            foreach (var item in deficiencies)
            {

                var labsWithDeficiency = lstlaboratories.Where(l => l.degree == item.degree && l.department == item.department && l.specializationName == item.specializationName && l.deficiency == true)
                   .Select(l => new { Deficiency = l.year + "-" + l.Semester + "-" + l.Labcode.Replace("-", "$") + "-" + l.specializationId }).Select(l => l.Deficiency).ToList();
                //labsWithDeficiency=labsWithDeficiency.GroupBy(m=>m.)
                List<string> defs = new List<string>();
                labsWithDeficiency.ForEach(l =>
                {

                    string[] strLab = l.Split('-');

                    int specializationid = Convert.ToInt32(strLab[3]);
                    int year = Convert.ToInt32(strLab[0]);
                    int semester = Convert.ToInt32(strLab[1]);
                    string labCode = strLab[2].Replace("$", "-");

                    var requiredLabs = lstlaboratories.Where(m => m.specializationId == specializationid && m.year == year && m.Semester == semester && m.Labcode == labCode).Select(m => m.EquipmentID).ToList();
                    int requiredCount = requiredLabs.Count();
                    int availableCount = jntuh_college_laboratories.Where(m => requiredLabs.Contains(m.EquipmentID)).Count();

                    int[] labmasterids = lstlaboratories.Where(m => m.specializationId == specializationid && m.year == year && m.Semester == semester && m.Labcode == labCode).Select(m => m.EquipmentID).Distinct().ToArray();
                    int[] collegelabequipmentids = jntuh_college_laboratories.Where(i => labmasterids.Contains(i.EquipmentID) && i.EquipmentNo == 1).Select(i => i.id).Distinct().ToArray();

                    if (requiredCount > availableCount && labmasterids.Count() != collegelabequipmentids.Count())//&& labCode!="14LAB"
                    {
                        string labName = lstlaboratories.Where(m => m.specializationId == specializationid && m.year == year && m.Semester == semester && m.Labcode == labCode).Select(m => m.LabName).FirstOrDefault();
                        string equipmentName = lstlaboratories.Where(m => m.specializationId == specializationid && m.year == year && m.Semester == semester && m.Labcode == labCode).Select(m => m.EquipmentName).FirstOrDefault();
                        if (!string.IsNullOrEmpty(labName))
                            defs.Add(year + "-" + semester + "-" + labName);
                        else
                            defs.Add(null);
                    }

                });







                CollegeFacultyLabs newFaculty = new CollegeFacultyLabs();
                newFaculty.Degree = item.degree;
                newFaculty.Department = item.department;
                newFaculty.Specialization = item.specializationName;
                newFaculty.SpecializationId = item.specializationid;
                if (NewspecializationIds.Contains(item.specializationid))
                {
                    newFaculty.LabsDeficiency = "NIL";
                }
                else
                {
                    newFaculty.LabsDeficiency = (labsWithDeficiency.Count() == 0 ? "NIL" : (defs.Count() == 0 ? "NIL" : String.Join(", ", defs)));
                }


                lstFaculty.Add(newFaculty);
            }

            return lstFaculty;
        }

        public string NoFacultyDeficiencyCourse(int? collegeID, int speciazliationID)
        {
            string faculty = string.Empty; string flag = string.Empty + "$" + string.Empty + "$" + string.Empty;
            string[] departments = new string[] { "English", "Mathematics", "Physics", "Chemistry", "Others" };

            List<CollegeFacultyWithIntakeReport> facultyCounts = collegeFaculty(collegeID).Where(c => c.shiftId == 1 && c.specializationId == speciazliationID && !departments.Contains(c.Department)).ToList();

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
                        minimumRequirementMet = "YES"; totalApprovedSpecializations += 1; courseApproved = 1;
                        remainingFaculty = tFaculty - rFaculty;
                        adjustedFaculty = rFaculty;
                    }
                    else
                    {
                        minimumRequirementMet = "NO"; totalDeficiencySpecializations += 1;
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
                        minimumRequirementMet = "YES"; totalApprovedSpecializations += 1; courseApproved = 1;
                        remainingFaculty = remainingFaculty - rFaculty;
                        adjustedFaculty = rFaculty;
                    }
                    else
                    {
                        minimumRequirementMet = "NO"; totalDeficiencySpecializations += 1;
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

                flag = flag + "$" + facultyShortage + "$" + (minimumRequirementMet.Equals(string.Empty) ? "0" : minimumRequirementMet);
            }

            if (count == 0)
            {
                flag = "YES";
            }
            return flag;
        }

        public string NoLabDeficiencyCourse(int? collegeID, int speciazliationID)
        {
            string labs = string.Empty; string flag = string.Empty;
            string[] departments = new string[] { "English", "Mathematics", "Physics", "Chemistry", "Others" };

            List<Lab> labsCount = collegeLabs(collegeID);
            List<Lab> labsCount1 = labsCount.Where(c => c.specializationId == speciazliationID && !departments.Contains(c.department)).ToList();

            var deficiencies = labsCount1.GroupBy(l => new { l.degree, l.department, l.specializationName })
                                         .Select(l => new { degree = l.Key.degree, department = l.Key.department, specializationName = l.Key.specializationName, deficiencies = string.Empty })
                                         .ToList();

            int totalSpecializations = deficiencies.Count();
            int totalDeficiencySpecializations = 0;
            int totalApprovedSpecializations = 0;
            int courseApproved = 0;

            List<jntuh_college> colleges = db.jntuh_college.ToList();
            string[] labsWithDeficiency = new string[] { };

            foreach (var item in deficiencies)
            {
                courseApproved = 0;
                string degreeType = db.jntuh_degree.Where(d => d.degree == item.degree).Select(d => d.jntuh_degree_type.degreeType).FirstOrDefault();

                labsWithDeficiency = labsCount.Where(l => l.degree == item.degree && l.department == item.department && l.specializationName == item.specializationName && l.deficiency == true)
                    .Select(l => new { Deficiency = degreeType.Equals("PG") ? l.specializationName + " Lab Deficiency" : l.year + "-" + l.Semester + "-" + l.LabName }).Select(l => l.Deficiency).ToArray();

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

            if (totalSpecializations == 0)
            { flag = "YES"; }

            return flag + "$" + (labsWithDeficiency.Count() == 0 ? "NIL" : String.Join(", ", labsWithDeficiency));
        }


        private int Max(int AdmittedIntake2, int AdmittedIntake3, int AdmittedIntake4)
        {
            return Math.Max(AdmittedIntake2, Math.Max(AdmittedIntake3, AdmittedIntake4));
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



            public int totalIntake { get; set; }
            public decimal requiredFaculty { get; set; }
            public int phdFaculty { get; set; }
            public int pgFaculty { get; set; }
            public int ugFaculty { get; set; }
            public int totalFaculty { get; set; }
            public int SpecializationsphdFaculty { get; set; }
            public int SpecializationspgFaculty { get; set; }
            public int specializationWiseFaculty { get; set; }
            public int facultyWithoutPANAndAadhaar { get; set; }
            public bool isintakechange { get; set; }
            public bool ispercentage { get; set; }
            public bool ishashcourses { get; set; }
            public bool isdollercourses { get; set; }
            public bool isnewincreasecourse { get; set; }
            public bool isnewreducecourse { get; set; }
            public bool isoldincreasecourse { get; set; }
            public bool isoldreducecourse { get; set; }

            public int dollercourseintake { get; set; }

            public bool isActive { get; set; }
            public DateTime? nbaFrom { get; set; }
            public DateTime? nbaTo { get; set; }

            public bool? deficiency { get; set; }
            public int shortage { get; set; }

            public string collegeRandomCode { get; set; }
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
            public int PharmacyspecializationWiseFaculty { get; set; }

            public string PharmacySpec1 { get; set; }
            public string PharmacySpec2 { get; set; }

            public IList<PharmacySpecilaizationList> PharmacySpecilaizationList { get; set; }




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

            //public bool AffiliationStatus1 { get; set; }
            public bool AffiliationStatus2 { get; set; }
            public bool AffiliationStatus3 { get; set; }
            public bool AffiliationStatus4 { get; set; }


        }

        public class PharmacySpecilaizationList
        {
            public string PharmacyspecName { get; set; }
            public string Specialization { get; set; }
            public int PharmacyspecWiseCount { get; set; }
        }

        private int GetIntake(int collegeId, int academicYearId, int specializationId, int shiftId, int flag, int DegreeId)
        {
            int intake = 0;
            var jntuh_academic_year = db.jntuh_academic_year.ToList();
            int presentYear = jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int AY1 = jntuh_academic_year.Where(a => a.actualYear == (presentYear + 1)).Select(a => a.id).FirstOrDefault();
            int AY2 = jntuh_academic_year.Where(a => a.actualYear == (presentYear)).Select(a => a.id).FirstOrDefault();
            int AY3 = jntuh_academic_year.Where(a => a.actualYear == (presentYear - 1)).Select(a => a.id).FirstOrDefault();
            int AY4 = jntuh_academic_year.Where(a => a.actualYear == (presentYear - 2)).Select(a => a.id).FirstOrDefault();
            int AY5 = jntuh_academic_year.Where(a => a.actualYear == (presentYear - 3)).Select(a => a.id).FirstOrDefault();
            //Degree B.Tech  
            if (DegreeId == 4)
            {
                //admitted
                if (flag == 1 && (academicYearId == AY2 || academicYearId == AY3 || academicYearId == AY4 || academicYearId == AY5))
                {
                    intake = db.jntuh_college_intake_existing.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == shiftId).Select(i => i.admittedIntake).FirstOrDefault();

                }
                else if (flag == 2 && (academicYearId == AY2 || academicYearId == AY3 || academicYearId == AY4 || academicYearId == AY5))
                {
                    intake = db.jntuh_college_intake_existing.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == shiftId).Select(i => i.aicteApprovedIntake).FirstOrDefault();

                }
                else if (flag == 1 && academicYearId == AY1)
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
                if (flag == 1 && academicYearId != AY1)
                {
                    intake = db.jntuh_college_intake_existing.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == shiftId).Select(i => i.admittedIntake).FirstOrDefault();

                }
                else if (flag == 1 && academicYearId == AY1)
                {
                    var inta = db.jntuh_college_intake_existing.FirstOrDefault(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == shiftId);
                    if (inta != null)
                    {
                        intake = Convert.ToInt32(inta.proposedIntake);
                    }

                }
                else if (flag == 2) //AICTE
                {
                    if (academicYearId == AY1)
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

        private bool GetAcademicYear(int collegeId, int academicYearId, int specializationId, int shiftId, int DegreeId)
        {
            var firstOrDefault = db.jntuh_college_intake_existing.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == shiftId).Select(i => i.courseAffiliatedStatus).FirstOrDefault();
            return firstOrDefault ?? false;
        }

        private int GetBtechAdmittedIntake(int Intake)
        {
            int BtechAdmittedIntake = 0;
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

        private int PhdWiseIntakeForMBAandMCA(int Phdcount)
        {
            int Intake = 0;
            if (Phdcount == 1)
            {
                Intake = 60;
            }
            else if (Phdcount == 2)
            {
                Intake = 120;
            }
            else if (Phdcount == 3)
            {
                Intake = 180;
            }
            else if (Phdcount == 4)
            {
                Intake = 240;
            }
            else if (Phdcount == 5)
            {
                Intake = 300;
            }
            else if (Phdcount == 6)
            {
                Intake = 360;
            }
            else if (Phdcount == 7)
            {
                Intake = 420;
            }

            return Intake;
        }

        public List<Lab> collegeLabs(int? collegeID)
        {
            List<Lab> lstlaboratories = new List<Lab>();
            List<Lab> collegeLabMaster = new List<Lab>();
            var jntuh_college_laboratories_deficiency = db.jntuh_college_laboratories_deficiency.Where(c => c.CollegeId == collegeID).ToList();
            string strcollegecode = db.jntuh_college_randamcodes.Where(r => r.CollegeId == collegeID).Select(r => r.RandamCode).FirstOrDefault();

            List<int> specializationIds = db.jntuh_college_intake_existing.Where(e => e.collegeId == collegeID && e.academicYearId == 9 && e.courseStatus != "Closure" && e.proposedIntake != 0).Select(e => e.specializationId).Distinct().ToList();
            // int[] Equipmentsids = db.jntuh_college_laboratories.Where(C => C.CollegeID == collegeID).Select(C => C.EquipmentID).ToArray();
            var DepartmentsData = db.jntuh_specialization.Where(e => e.isActive == true && specializationIds.Contains(e.id)).Select(e => e.departmentId).Distinct().ToArray();

            var DegreeIds = db.jntuh_department.Where(e => e.isActive == true && DepartmentsData.Contains(e.id)).Select(e => e.degreeId).ToArray();

            if (DegreeIds.Contains(4))
            {
                specializationIds.Add(39);
            }




            string CollegeAffiliationStatus = db.jntuh_college_affiliation.Where(u => u.collegeId == collegeID && u.affiliationTypeId == 7).Select(u => u.affiliationStatus).FirstOrDefault();

            if (CollegeAffiliationStatus == "Yes")
            {
                collegeLabMaster = db.jntuh_lab_master.AsNoTracking().Where(l => specializationIds.Contains(l.SpecializationID) && l.CollegeId == collegeID)
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
                                                          LabName = l.LabName
                                                      }).OrderBy(l => l.degreeDisplayOrder).ThenBy(l => l.department).ThenBy(l => l.specializationName).ThenBy(l => l.year).ThenBy(l => l.Semester).Distinct().ToList();
            }
            else if (CollegeAffiliationStatus == "No" || CollegeAffiliationStatus == null)
            {
                collegeLabMaster = db.jntuh_lab_master.AsNoTracking()
                                                           .Where(l => specializationIds.Contains(l.SpecializationID) && l.CollegeId == null)
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
                                                               LabName = l.LabName
                                                           }).OrderBy(l => l.degreeDisplayOrder).ThenBy(l => l.department).ThenBy(l => l.specializationName).ThenBy(l => l.year).ThenBy(l => l.Semester).Distinct().ToList();
            }

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
                //if (jntuh_college_laboratories_deficiency.Count() != 0)
                //{
                //    lstlabs.deficiency = jntuh_college_laboratories_deficiency.Where(ld => ld.LabCode == item.Labcode && ld.CollegeId == collegeID && ld.Year == item.year && ld.Semister == item.Semester && ld.SpecializationId == item.specializationId).Select(ld => ld.Deficiency).FirstOrDefault();
                //    lstlabs.id = jntuh_college_laboratories_deficiency.Where(ld => ld.LabCode == item.Labcode && ld.CollegeId == collegeID && ld.Year == item.year && ld.Semister == item.Semester && ld.SpecializationId == item.specializationId).Select(ld => ld.Id).FirstOrDefault();
                //}
                //else
                //{
                lstlabs.deficiency = null;
                lstlabs.id = 0;
                // }
                lstlaboratories.Add(lstlabs);
            }

            lstlaboratories = lstlaboratories.OrderBy(l => l.degreeDisplayOrder).ThenBy(l => l.department).ThenBy(l => l.specializationName).ThenBy(l => l.year).ThenBy(l => l.Semester).Distinct().ToList();

            return lstlaboratories;
        }

        public class Assessment
        {
            public int SNo { get; set; }
            public string Degree { get; set; }
            public string Specialization { get; set; }
            public int FacultyShortage { get; set; }
            public string Phd { get; set; }
            public string[] Labs { get; set; }
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
            public int ShiftId { get; set; }
            public int TotalIntake { get; set; }
            public int DollerCourseIntake { get; set; }
            public int Required { get; set; }
            public int Available { get; set; }
            public string Deficiency { get; set; }
            public string PhdDeficiency { get; set; }
            public string LabsDeficiency { get; set; }
            public int Requiredphd { get; set; }
            public int Availablephd { get; set; }
            public bool isintakechange { get; set; }
            public bool ispercentage { get; set; }
            public bool ishashcourses { get; set; }
            public bool isnewincreasecourse { get; set; }
            public bool isnewreducecourse { get; set; }
            public bool isoldincreasecourse { get; set; }
            public bool isoldreducecourse { get; set; }
        }

        #region ForPharmacyCollegeFaculty

        //public List<CollegeFacultyLabs> PharmacyDeficienciesInFaculty(int? collegeID)
        //{
        //    List<CollegeFacultyLabs> lstFaculty = new List<CollegeFacultyLabs>();

        //    string faculty = string.Empty;
        //    int? AddingFacultyCount = 0;
        //    int? TotalcollegeFaculty = 0;
        //    string facultyAdmittedIntakeZero = string.Empty;

        //    var jntuh_academic_year = db.jntuh_academic_year.ToList();
        //    int presentYear = jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
        //    int AY1 = jntuh_academic_year.Where(a => a.actualYear == (presentYear + 1)).Select(a => a.id).FirstOrDefault();
        //    int AY2 = jntuh_academic_year.Where(a => a.actualYear == (presentYear)).Select(a => a.id).FirstOrDefault();
        //    int AY3 = jntuh_academic_year.Where(a => a.actualYear == (presentYear - 1)).Select(a => a.id).FirstOrDefault();
        //    int AY4 = jntuh_academic_year.Where(a => a.actualYear == (presentYear - 2)).Select(a => a.id).FirstOrDefault();
        //    int AY5 = jntuh_academic_year.Where(a => a.actualYear == (presentYear - 3)).Select(a => a.id).FirstOrDefault();

        //    var Departments = db.jntuh_department.Where(d => d.degreeId == 2 || d.degreeId == 5 || d.degreeId == 9 || d.degreeId == 10).ToList();
        //    int[] Departmentids = Departments.Select(d => d.id).ToArray();
        //    var Specializations = db.jntuh_specialization.Where(s => Departmentids.Contains(s.departmentId)).ToList();
        //    int[] Specializationids = Specializations.Select(s => s.id).ToArray();

        //    List<jntuh_college_intake_existing> intake = db.jntuh_college_intake_existing.Where(i => i.collegeId == collegeID && Specializationids.Contains(i.specializationId) && i.courseStatus != "Closure").ToList();
        //    List<CollegeIntakeExisting> collegeIntakeExisting = new List<CollegeIntakeExisting>();
        //    List<CollegeIntakeExisting> CurrentyearcollegeIntakeExisting = new List<CollegeIntakeExisting>();

        //    foreach (var item in intake)
        //    {
        //        CollegeIntakeExisting newIntake = new CollegeIntakeExisting();
        //        newIntake.id = item.id;
        //        newIntake.collegeId = item.collegeId;
        //        newIntake.academicYearId = item.academicYearId;
        //        newIntake.shiftId = item.shiftId;
        //        newIntake.isActive = item.isActive;
        //        newIntake.nbaFrom = item.nbaFrom;
        //        newIntake.nbaTo = item.nbaTo;
        //        newIntake.specializationId = item.specializationId;
        //        newIntake.Specialization = item.jntuh_specialization.specializationName;
        //        newIntake.DepartmentID = item.jntuh_specialization.jntuh_department.id;
        //        newIntake.Department = item.jntuh_specialization.jntuh_department.departmentName;
        //        newIntake.degreeID = item.jntuh_specialization.jntuh_department.jntuh_degree.id;
        //        newIntake.Degree = item.jntuh_specialization.jntuh_department.jntuh_degree.degree;
        //        newIntake.degreeDisplayOrder = item.jntuh_specialization.jntuh_department.jntuh_degree.degreeDisplayOrder;
        //        newIntake.shiftId = item.shiftId;
        //        newIntake.Shift = item.jntuh_shift.shiftName;
        //        collegeIntakeExisting.Add(newIntake);
        //    }
        //    CurrentyearcollegeIntakeExisting = collegeIntakeExisting.Where(a => a.academicYearId == 10 && a.shiftId == 1).OrderBy(a => a.Specialization).ToList();
        //    string cid = collegeID.ToString();
        //    var FacultyData = db.jntuh_appeal_pharmacydata.Where(p => p.CollegeCode == cid).ToList();
        //    string[] AssignedFaculty = FacultyData.Select(p => p.Deficiency).ToArray();
        //    var registeredFaculty = db.jntuh_registered_faculty.Where(rf => AssignedFaculty.Contains(rf.RegistrationNumber.Trim())).ToList();
        //    var jntuh_registered_faculty1 = registeredFaculty
        //                                            .Select(rf => new
        //                                            {
        //                                                RegistrationNumber = rf.RegistrationNumber,
        //                                                HighestDegreeID = rf.jntuh_registered_faculty_education.Count() != 0 ? rf.jntuh_registered_faculty_education.Select(e => e.educationId).Max() : 0,
        //                                            }).Where(e => e.HighestDegreeID == 6).ToList();
        //    string[] PhdRegNO = jntuh_registered_faculty1.Select(e => e.RegistrationNumber).ToArray();

        //    string[] Collegefaculty = db.jntuh_college_faculty_registered.Where(CF => CF.collegeId == collegeID).Select(Cf => Cf.RegistrationNumber).ToArray();
        //    var collegeFacultycount1 = db.jntuh_registered_faculty.Where(rf => Collegefaculty.Contains(rf.RegistrationNumber)).ToList();
        //    var collegeFacultycount = collegeFacultycount1.Where(rf => rf.type != "Adjunct" && ((rf.Absent == false) && (rf.OriginalCertificatesNotShown == false) && (rf.Xeroxcopyofcertificates == false || rf.Xeroxcopyofcertificates == null) && (rf.NotQualifiedAsperAICTE == false || rf.NotQualifiedAsperAICTE == null)//&& rf.Noform16Verification == false && rf.NoForm16 == false
        //                                               && (rf.NoSCM == false || rf.NoSCM == null) && (rf.PANNumber != null) && (rf.IncompleteCertificates == false || rf.IncompleteCertificates == null) && (rf.Blacklistfaculy == false) && rf.NoRelevantUG != "Yes" && rf.NoRelevantPG != "Yes" && rf.NORelevantPHD != "Yes" && rf.NotIdentityfiedForanyProgram == false && (rf.InvalidPANNumber == false || rf.InvalidPANNumber == null) && (rf.OriginalsVerifiedPHD != true) && (rf.OriginalsVerifiedUG != true) && (rf.Invaliddegree == false || rf.Invaliddegree == null) && rf.BAS != "Yes") && rf.InvalidAadhaar != "Yes").Select(r => r.RegistrationNumber).ToList();
        //    TotalcollegeFaculty = collegeFacultycount.Count();
        //    int? Required = 0;
        //    int? Avilable = 0;
        //    int? PhDAvilable = 0;
        //    //int? TotalRequired = 0;
        //    //int? TotalAvilable = 0;
        //    int? TotalIntake = 0;
        //    int? PraposedIntake = 0;
        //    int Sno = 1;
        //    string strgroup = "";
        //    string PharmacyStatus = "";
        //    string specialization = "";

        //    string PharmD = "";


        //    foreach (var item in CurrentyearcollegeIntakeExisting)
        //    {
        //        specialization = item.specializationId.ToString();
        //        CollegeFacultyLabs newFaculty = new CollegeFacultyLabs();
        //        newFaculty.Degree = item.Degree;
        //        newFaculty.Department = item.Department;
        //        newFaculty.SpecializationId = item.specializationId;
        //        newFaculty.Specialization = item.Specialization;
        //        if (item.Specialization == "Pharm.D")
        //        {
        //            newFaculty.TotalIntake = 30;
        //            newFaculty.Required = 0;
        //            newFaculty.Available = 0;
        //        }
        //        else if (item.Specialization == "Pharm.D PB")
        //        {
        //            newFaculty.TotalIntake = 10;
        //            newFaculty.Required = 0;
        //            newFaculty.Available = 0;
        //        }
        //        else
        //        {
        //            newFaculty.TotalIntake = FacultyData.Where(p => p.CollegeCode == cid && p.Specialization == specialization).Select(p => p.TotalIntake).FirstOrDefault() != null ? (int)FacultyData.Where(p => p.CollegeCode == cid && p.Specialization == specialization).Select(p => p.ProposedIntake).FirstOrDefault() : 0;
        //            newFaculty.Required = FacultyData.Where(p => p.CollegeCode == cid && p.Specialization == specialization).Select(p => p.NoOfFacultyRequired).FirstOrDefault() != null ? (int)FacultyData.Where(p => p.CollegeCode == cid && p.Specialization == specialization).Select(p => p.NoOfFacultyRequired).FirstOrDefault() : 0;
        //            newFaculty.Available = FacultyData.Count(p => p.CollegeCode == cid && p.Specialization == specialization && p.Deficiency != null) != null ? (int)FacultyData.Count(p => p.CollegeCode == cid && p.Specialization == specialization && p.Deficiency != null) : 0;
        //        }

        //        if (item.Degree != "M.Pharmacy")
        //            newFaculty.Requiredphd = 0;
        //        else
        //            newFaculty.Requiredphd = 1;
        //        if (item.Degree != "M.Pharmacy")
        //            newFaculty.Availablephd = 0;
        //        else
        //            newFaculty.Availablephd = (int)FacultyData.Count(f => PhdRegNO.Contains(f.Deficiency) && f.Specialization == specialization) > 0 ? (int)FacultyData.Count(f => PhdRegNO.Contains(f.Deficiency) && f.Specialization == specialization) : 0;
        //        if (newFaculty.Required <= newFaculty.Available)
        //        {
        //            if (PharmacyStatus == "Deficiency")
        //            {
        //                if (item.Degree == "M.Pharmacy")
        //                {
        //                    newFaculty.Deficiency = "YES";
        //                }
        //                else if (item.Degree == "Pharm.D" || item.Degree == "Pharm.D PB")
        //                {
        //                    newFaculty.Deficiency = "YES";
        //                }
        //            }
        //            else
        //                newFaculty.Deficiency = "NO";

        //        }

        //        else
        //        {
        //            if (item.Degree == "B.Pharmacy")
        //            {
        //                PharmacyStatus = "Deficiency";
        //            }
        //            else if ((item.Degree == "Pharm.D" || item.Degree == "Pharm.D PB") && (PharmacyStatus != "Deficiency"))
        //            {
        //                newFaculty.Deficiency = "NO";
        //            }
        //            newFaculty.Deficiency = "YES";
        //        }
        //        if (item.Degree == "M.Pharmacy")
        //        {
        //            if (newFaculty.Availablephd > 0)
        //                newFaculty.PhdDeficiency = "NO";
        //            else
        //                newFaculty.PhdDeficiency = "YES";
        //        }
        //        else
        //        {
        //            newFaculty.PhdDeficiency = "NO";
        //        }
        //        newFaculty.ShiftId = 1;


        //        lstFaculty.Add(newFaculty);
        //    }
        //    return lstFaculty;
        //}

        public List<CollegeFacultyLabs> PharmacyDeficienciesInFaculty(int? collegeID)
        {
            string faculty = string.Empty;
            int? AddingFacultyCount = 0;
            int? TotalcollegeFaculty = 0;
            int? AdditionalFaculty = 0;

            List<PharmacyReportsClass> PharmacyAppealFaculty = new List<PharmacyReportsClass>();
            string facultyAdmittedIntakeZero = string.Empty;
            var jntuh_academic_year = db.jntuh_academic_year.ToList();
            int presentYear = jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();

            int AY0 = jntuh_academic_year.Where(a => a.actualYear == (presentYear + 1)).Select(a => a.id).FirstOrDefault();
            int AY1 = jntuh_academic_year.Where(a => a.actualYear == presentYear).Select(a => a.id).FirstOrDefault();
            int AY2 = jntuh_academic_year.Where(a => a.actualYear == (presentYear - 1)).Select(a => a.id).FirstOrDefault();
            int AY3 = jntuh_academic_year.Where(a => a.actualYear == (presentYear - 2)).Select(a => a.id).FirstOrDefault();
            int AY4 = jntuh_academic_year.Where(a => a.actualYear == (presentYear - 3)).Select(a => a.id).FirstOrDefault();
            int AY5 = jntuh_academic_year.Where(a => a.actualYear == (presentYear - 4)).Select(a => a.id).FirstOrDefault();

            List<jntuh_college_intake_existing> intake = db.jntuh_college_intake_existing.Where(i => i.collegeId == collegeID && i.academicYearId == 13 && i.proposedIntake != 0 && i.courseStatus != "Closure").ToList();
            List<CollegeIntakeExisting> collegeIntakeExisting = new List<CollegeIntakeExisting>();
            List<CollegeFacultyLabs> lstFaculty = new List<CollegeFacultyLabs>();

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

            collegeIntakeExisting = collegeIntakeExisting.GroupBy(a => new { a.specializationId, a.shiftId }).Select(a => a.First()).ToList();

            var jntuh_college = db.jntuh_college.Where(a => a.isActive == true).Select(q => q).ToList();

            string cid = collegeID.ToString();
            var PharmacyDepartmens = new int[] { 26, 36, 27, 39, 61 };
            var FacultyData = db.jntuh_appeal_pharmacydata.Where(p => p.CollegeCode == cid).ToList();
            string[] AssignedFaculty = FacultyData.Select(p => p.Deficiency).ToArray();
            var registeredFaculty = db.jntuh_registered_faculty.Where(rf => AssignedFaculty.Contains(rf.RegistrationNumber.Trim())).ToList();
            AdditionalFaculty = FacultyData.Where(a => a.CollegeCode == cid && a.Deficiency != null).Select(z => z.Deficiency).Count();

            int Count = 1;
            foreach (var item in collegeIntakeExisting.OrderBy(s => s.specializationId).ThenBy(a => a.shiftId).ToList())
            {
                if (item.Degree == "B.Pharmacy")
                {
                    for (int i = 1; i <= 4; i++)
                    {
                        PharmacyReportsClass Pharmacy = new PharmacyReportsClass();
                        Pharmacy.Collegeid = item.collegeId;
                        Pharmacy.Degree = item.Degree;
                        Pharmacy.DepartmentId = item.DepartmentID;
                        Pharmacy.Department = item.Department;
                        Pharmacy.SpecializationId = item.specializationId;
                        Pharmacy.Specialization = item.Specialization;
                        Pharmacy.ShiftId = item.shiftId;
                        switch (i)
                        {
                            case 1:
                                Pharmacy.PharmacySpecialization = "Group1 (Pharmaceutics , Industrial Pharmacy , Pharmaceutical Technology , 	Pharmaceutical Biotechnology , PRA)";
                                Pharmacy.GroupId = "1";
                                break;
                            case 2:
                                Pharmacy.PharmacySpecialization = "Group2 (Pharmaceutical Chemistry,Pharmaceutical Analysis , PAQA , QA , QAPRA , NIPER Medicinal Chemistry)";
                                Pharmacy.GroupId = "2";
                                break;
                            case 3:
                                Pharmacy.PharmacySpecialization = "Group3 (Pharmacology, Pharm-D, Pharm-DPB, Pharmacy Practice , Hospital Pharmacy , Clinical Pharmacy,  Hospital and Clinical Pharmacy)";
                                Pharmacy.GroupId = "3";
                                break;
                            default:
                                Pharmacy.PharmacySpecialization = "Group4 (Pharmacognosy, Pharmaceutical Chemistry , Phytopharmacy & Phytomedicine , NIPER  Natural Products , Pharmaceutical Biotechnology";
                                Pharmacy.GroupId = "4";
                                break;
                        }

                        Pharmacy.ProposedIntake = FacultyData.Where(f => f.CollegeCode == cid && f.Specialization == item.specializationId.ToString()).Select(f => f.ProposedIntake).FirstOrDefault();
                        Pharmacy.TotalIntake = FacultyData.Where(f => f.CollegeCode == cid && f.Specialization == item.specializationId.ToString()).Select(f => f.TotalIntake).FirstOrDefault();
                        Pharmacy.NoOfFacultyRequired = FacultyData.Where(f => f.CollegeCode == cid && f.Specialization == item.specializationId.ToString()).Select(f => f.NoOfFacultyRequired).FirstOrDefault();
                        Pharmacy.NoOfAvilableFaculty = FacultyData.Where(f => f.CollegeCode == cid && f.Specialization == item.specializationId.ToString() && f.Deficiency != null).Select(a => a.Deficiency).Distinct().Count();
                        Pharmacy.SpecializationwiseRequiredFaculty = FacultyData.Where(f => f.CollegeCode == cid && f.Specialization == item.specializationId.ToString() && f.PharmacySpecialization == Pharmacy.GroupId).Select(f => f.SpecializationWiseRequiredFaculty).FirstOrDefault();
                        Pharmacy.SpecializationwiseAvilableFaculty = FacultyData.Where(f => f.CollegeCode == cid && f.Specialization == item.specializationId.ToString() && f.PharmacySpecialization == Pharmacy.GroupId && f.Deficiency != null).Select(a => a.Deficiency).Distinct().Count();
                        if (Pharmacy.SpecializationwiseAvilableFaculty >= Pharmacy.SpecializationwiseRequiredFaculty)
                        {
                            Pharmacy.Deficiency = "No Deficiency";
                        }
                        else
                        {
                            Pharmacy.Deficiency = "Deficiency";
                        }

                        var AvailiableFacultyRegNos = FacultyData.Where(f => f.CollegeCode == cid && f.Specialization == item.specializationId.ToString() && f.PharmacySpecialization == Pharmacy.GroupId && f.Deficiency != null).Select(q => q.Deficiency).Distinct().ToList();

                        var PHDFacultyList = db.jntuh_registered_faculty.Where(q => AvailiableFacultyRegNos.Contains(q.RegistrationNumber)).Select(a => a).ToList();

                        //var PHDFacultyCleared = PHDFacultyList.Where(rf => rf.type != "Adjunct" && ((rf.NoForm16 == false || rf.NoForm16 == null) && (rf.NoForm26AS == false || rf.NoForm26AS == null) && (rf.Absent == false || rf.Absent == null) && (rf.AbsentforVerification == false || rf.AbsentforVerification == null) && (rf.OriginalCertificatesNotShown == false || rf.OriginalCertificatesNotShown == null) && (rf.Xeroxcopyofcertificates == false || rf.Xeroxcopyofcertificates == null) && (rf.NotQualifiedAsperAICTE == false || rf.NotQualifiedAsperAICTE == null)
                        //                              && (rf.Noclass == false || rf.Noclass == null) && (rf.FakePHD == false || rf.FakePHD == null) && (rf.Genuinenessnotsubmitted == false || rf.Genuinenessnotsubmitted == null) && (rf.NotconsideredPHD == false || rf.NotconsideredPHD == null) && (rf.NoPGspecialization == false || rf.NoPGspecialization == null) && (rf.NoSCM == false || rf.NoSCM == null) && (rf.PANNumber != null) && (rf.IncompleteCertificates == false || rf.IncompleteCertificates == null) && (rf.Blacklistfaculy == false) && rf.NoRelevantUG != "Yes" && rf.NoRelevantPG != "Yes" && rf.NORelevantPHD != "Yes" && (rf.InvalidPANNumber == false || rf.InvalidPANNumber == null) && (rf.OriginalsVerifiedPHD != true) && (rf.OriginalsVerifiedUG != true) && (rf.Invaliddegree == false || rf.Invaliddegree == null) && rf.InvalidAadhaar != "Yes")).Select(rf => rf).ToList();


                        var PHDFacultyCleared = PHDFacultyList.Where(rf => rf.type != "Adjunct" && ((rf.NotQualifiedAsperAICTE == false || rf.NotQualifiedAsperAICTE == null) && (rf.Noclass == false || rf.Noclass == null) && (rf.Absent == false || rf.Absent == null) && (rf.OriginalCertificatesNotShown == false || rf.OriginalCertificatesNotShown == null) &&
                                                         (rf.NoSCM == false || rf.NoSCM == null) && rf.NoRelevantUG != "Yes" && rf.NoRelevantPG != "Yes" && rf.NORelevantPHD != "Yes" && (rf.InvalidPANNumber == false || rf.InvalidPANNumber == null) && (rf.Blacklistfaculy == false) && (rf.AbsentforVerification == false || rf.AbsentforVerification == null) && (rf.DepartmentId != 61 || rf.DepartmentId != 27))).Select(rf => rf).ToList();

                        var PHDFaculty = PHDFacultyCleared.Select(q => q.id).ToList();
                        int PhdRegNOCount = db.jntuh_registered_faculty_education.Count(q => PHDFaculty.Contains(q.facultyId) && q.educationId == 6);

                        Pharmacy.PHdFaculty = PhdRegNOCount;
                        Pharmacy.IsActive = false;
                        PharmacyAppealFaculty.Add(Pharmacy);
                    }

                    PharmacyReportsClass BPharmacyObj = new PharmacyReportsClass();
                    BPharmacyObj.Collegeid = PharmacyAppealFaculty.Select(z => z.Collegeid).FirstOrDefault();
                    BPharmacyObj.Degree = "B.Pharmacy";
                    BPharmacyObj.DepartmentId = 26;
                    BPharmacyObj.Department = "B.Pharmacy";
                    BPharmacyObj.SpecializationId = 12;
                    BPharmacyObj.Specialization = "B.Pharmacy";
                    BPharmacyObj.ShiftId = PharmacyAppealFaculty.Select(z => z.ShiftId).FirstOrDefault();
                    BPharmacyObj.TotalIntake = PharmacyAppealFaculty.Where(a => a.DepartmentId == 26).Select(z => z.TotalIntake).FirstOrDefault();
                    BPharmacyObj.ProposedIntake = PharmacyAppealFaculty.Where(a => a.DepartmentId == 26).Select(z => z.ProposedIntake).FirstOrDefault();
                    BPharmacyObj.NoOfFacultyRequired = PharmacyAppealFaculty.Where(a => a.DepartmentId == 26).Select(z => z.NoOfFacultyRequired).FirstOrDefault();
                    BPharmacyObj.NoOfAvilableFaculty = PharmacyAppealFaculty.Where(a => a.DepartmentId == 26).Select(z => z.NoOfAvilableFaculty).FirstOrDefault();
                    BPharmacyObj.Deficiency = PharmacyAppealFaculty.Count(a => a.DepartmentId == 26 && a.Deficiency == "Deficiency") > 0 ? "Deficiency" : "No Deficiency";
                    BPharmacyObj.RequiredPHdFaculty = 0;
                    BPharmacyObj.PHdFaculty = PharmacyAppealFaculty.Where(a => a.DepartmentId == 26).Select(q => q.PHdFaculty).Sum();
                    BPharmacyObj.IsActive = true;
                    PharmacyAppealFaculty.Add(BPharmacyObj);
                }
                else if (item.Degree == "Pharm.D" || item.Degree == "Pharm.D PB")
                {
                    PharmacyReportsClass Pharmacy = new PharmacyReportsClass();
                    Pharmacy.Collegeid = item.collegeId;
                    Pharmacy.Degree = item.Degree;
                    Pharmacy.DepartmentId = item.DepartmentID;
                    Pharmacy.Department = item.Department;
                    Pharmacy.SpecializationId = item.specializationId;
                    Pharmacy.Specialization = item.Specialization;
                    Pharmacy.ShiftId = item.shiftId;
                    //Pharmacy.TotalIntake = (PharmDFirstYerintake + PharmDSecondYerintake + PharmDThirdYerintake + PharmDFourthYerintake + PharmDFifthhYerintake);
                    if (Pharmacy.SpecializationId == 18)
                        Pharmacy.ProposedIntake = 30;
                    else if (Pharmacy.SpecializationId == 19)
                        Pharmacy.ProposedIntake = 10;
                    //Pharmacy.ProposedIntake = GetPharmacyIntake(item.collegeId, AY0, item.specializationId, item.shiftId, 0);
                    if (PharmacyAppealFaculty.Count(a => a.DepartmentId == 26 && a.Deficiency == "Deficiency") > 0)
                        Pharmacy.Deficiency = "Deficiency";
                    else
                        Pharmacy.Deficiency = "No Deficiency";

                    Pharmacy.RequiredPHdFaculty = 0;
                    Pharmacy.IsActive = true;
                    PharmacyAppealFaculty.Add(Pharmacy);
                }
                else if (item.Degree == "M.Pharmacy")
                {
                    PharmacyReportsClass Pharmacy = new PharmacyReportsClass();
                    Pharmacy.Collegeid = item.collegeId;
                    Pharmacy.Degree = item.Degree;
                    Pharmacy.DepartmentId = item.DepartmentID;
                    Pharmacy.Department = item.Department;
                    Pharmacy.SpecializationId = item.specializationId;
                    Pharmacy.Specialization = item.Specialization;
                    Pharmacy.ShiftId = item.shiftId;
                    Pharmacy.ProposedIntake = FacultyData.Where(f => f.CollegeCode == cid && f.Specialization == item.specializationId.ToString()).Select(f => f.ProposedIntake).FirstOrDefault();
                    Pharmacy.TotalIntake = FacultyData.Where(f => f.CollegeCode == cid && f.Specialization == item.specializationId.ToString()).Select(f => f.TotalIntake).FirstOrDefault();

                    Pharmacy.NoOfFacultyRequired = FacultyData.Where(f => f.CollegeCode == cid && f.Specialization == item.specializationId.ToString()).Select(f => f.NoOfFacultyRequired).FirstOrDefault();
                    Pharmacy.NoOfAvilableFaculty = FacultyData.Count(f => f.CollegeCode == cid && f.Specialization == item.specializationId.ToString() && f.Deficiency != null);
                    Pharmacy.SpecializationwiseRequiredFaculty = FacultyData.Where(f => f.CollegeCode == cid && f.Specialization == item.specializationId.ToString()).Select(f => f.SpecializationWiseRequiredFaculty).FirstOrDefault();
                    Pharmacy.SpecializationwiseAvilableFaculty = FacultyData.Where(f => f.CollegeCode == cid && f.Specialization == item.specializationId.ToString() && f.Deficiency != null).Select(a => a.Deficiency).Distinct().Count();

                    Pharmacy.RequiredPHdFaculty = 1;

                    var AvailiableFacultyRegNos = FacultyData.Where(f => f.CollegeCode == cid && f.Specialization == item.specializationId.ToString() && f.Deficiency != null).Select(q => q.Deficiency).Distinct().ToList();

                    var PHDFacultyList = db.jntuh_registered_faculty.Where(q => AvailiableFacultyRegNos.Contains(q.RegistrationNumber)).Select(a => a).ToList();

                    //var PHDFacultyCleared = PHDFacultyList.Where(rf => rf.type != "Adjunct" && ((rf.NoForm16 == false || rf.NoForm16 == null) && (rf.NoForm26AS == false || rf.NoForm26AS == null) && (rf.Absent == false || rf.Absent == null) && (rf.AbsentforVerification == false || rf.AbsentforVerification == null) && (rf.OriginalCertificatesNotShown == false || rf.OriginalCertificatesNotShown == null) && (rf.Xeroxcopyofcertificates == false || rf.Xeroxcopyofcertificates == null) && (rf.NotQualifiedAsperAICTE == false || rf.NotQualifiedAsperAICTE == null)
                    //                                && (rf.Noclass == false || rf.Noclass == null) && (rf.FakePHD == false || rf.FakePHD == null) && (rf.Genuinenessnotsubmitted == false || rf.Genuinenessnotsubmitted == null) && (rf.NotconsideredPHD == false || rf.NotconsideredPHD == null) && (rf.NoPGspecialization == false || rf.NoPGspecialization == null) && (rf.NoSCM == false || rf.NoSCM == null) && (rf.PANNumber != null) && (rf.IncompleteCertificates == false || rf.IncompleteCertificates == null) && (rf.Blacklistfaculy == false) && rf.NoRelevantUG != "Yes" && rf.NoRelevantPG != "Yes" && rf.NORelevantPHD != "Yes" && (rf.InvalidPANNumber == false || rf.InvalidPANNumber == null) && (rf.OriginalsVerifiedPHD != true) && (rf.OriginalsVerifiedUG != true) && (rf.Invaliddegree == false || rf.Invaliddegree == null) && rf.InvalidAadhaar != "Yes")).Select(rf => rf).ToList();


                    var PHDFacultyCleared = PHDFacultyList.Where(rf => rf.type != "Adjunct" && ((rf.NotQualifiedAsperAICTE == false || rf.NotQualifiedAsperAICTE == null) && (rf.Noclass == false || rf.Noclass == null) && (rf.Absent == false || rf.Absent == null) && (rf.OriginalCertificatesNotShown == false || rf.OriginalCertificatesNotShown == null) &&
                                                         (rf.NoSCM == false || rf.NoSCM == null) && rf.NoRelevantUG != "Yes" && rf.NoRelevantPG != "Yes" && rf.NORelevantPHD != "Yes" && (rf.InvalidPANNumber == false || rf.InvalidPANNumber == null) && (rf.Blacklistfaculy == false) && (rf.AbsentforVerification == false || rf.AbsentforVerification == null) && (rf.DepartmentId != 61 || rf.DepartmentId != 27))).Select(rf => rf).ToList();

                    var PHDFaculty = PHDFacultyCleared.Select(q => q.id).ToList();
                    int PhdRegNOCount = db.jntuh_registered_faculty_education.Count(q => PHDFaculty.Contains(q.facultyId) && q.educationId == 6);

                    Pharmacy.PHdFaculty = PhdRegNOCount;

                    if (Pharmacy.SpecializationwiseAvilableFaculty >= Pharmacy.SpecializationwiseRequiredFaculty)
                    {
                        if (PharmacyAppealFaculty.Count(a => a.DepartmentId == 26 && a.Deficiency == "Deficiency") > 0)
                            Pharmacy.Deficiency = "Deficiency";
                        else
                            Pharmacy.Deficiency = "No Deficiency";
                    }
                    else
                    {
                        Pharmacy.Deficiency = "Deficiency";
                    }
                    Pharmacy.IsActive = true;
                    if (Pharmacy.ShiftId == 1)
                        PharmacyAppealFaculty.Add(Pharmacy);
                }
            }

            foreach (var Singlerecord in PharmacyAppealFaculty.Where(a => a.IsActive == true).ToList())
            {
                CollegeFacultyLabs newFaculty = new CollegeFacultyLabs();
                newFaculty.Degree = Singlerecord.Degree;
                newFaculty.Department = Singlerecord.Department;
                newFaculty.SpecializationId = Singlerecord.SpecializationId;
                newFaculty.Specialization = Singlerecord.Specialization;
                newFaculty.ShiftId = Singlerecord.ShiftId;
                if (newFaculty.SpecializationId == 12)
                    newFaculty.TotalIntake = Singlerecord.ProposedIntake >= 100 ? 100 : Convert.ToInt32(Singlerecord.ProposedIntake);
                else if (newFaculty.SpecializationId == 18)
                    newFaculty.TotalIntake = 30;
                else if (newFaculty.SpecializationId == 19)
                    newFaculty.TotalIntake = 10;
                else
                    newFaculty.TotalIntake = Singlerecord.ProposedIntake >= 15 ? 15 : Convert.ToInt32(Singlerecord.ProposedIntake);

                newFaculty.Required = Convert.ToInt32(Singlerecord.NoOfFacultyRequired);
                newFaculty.Available = Convert.ToInt32(Singlerecord.NoOfAvilableFaculty);
                newFaculty.Requiredphd = Convert.ToInt32(Singlerecord.RequiredPHdFaculty);
                newFaculty.Availablephd = Convert.ToInt32(Singlerecord.PHdFaculty);
                newFaculty.Deficiency = Singlerecord.Deficiency == "Deficiency" ? "YES" : "NO";
                lstFaculty.Add(newFaculty);
            }
            return lstFaculty;
        }

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
                //Commented on 18-06-2018 by Narayana Reddy
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

                //college Reg nos
                var jntuh_college_faculty_registered = db.jntuh_college_faculty_registered.Where(cf => cf.collegeId == collegeId).ToList();
                string[] strRegnos = jntuh_college_faculty_registered.Select(cf => cf.RegistrationNumber).ToArray();

                var jntuh_college_faculty_registered_new = db.jntuh_college_principal_registered.Where(cf => cf.collegeId == collegeId).ToList();
                var principalRegno = jntuh_college_faculty_registered_new.Select(cf => cf.RegistrationNumber).FirstOrDefault();

                //education categoryIds UG,PG,PHD...........
                var jntuh_specializations = db.jntuh_specialization.ToList();
                var jntuh_departments = db.jntuh_department.ToList();
                int pharmacyDeptId = db.jntuh_department.Where(d => d.departmentName == "Pharmacy").Select(d => d.id).FirstOrDefault();
                var collegeaffliations = db.jntuh_college_affliationstatus.AsNoTracking().ToList();
                var jntuh_education_category = db.jntuh_education_category.ToList();

                var registeredFaculty = db.jntuh_registered_faculty.Where(rf => strRegnos.Contains(rf.RegistrationNumber.Trim())).ToList();

                //var registeredFaculty = principalRegno != null ? db.jntuh_registered_faculty.Where(rf => strRegnos.Contains(rf.RegistrationNumber.Trim()) && rf.RegistrationNumber != principalRegno).ToList()
                //    : db.jntuh_registered_faculty.Where(rf => strRegnos.Contains(rf.RegistrationNumber.Trim())).ToList();

                //var jntuh_registered_faculty1 = registeredFaculty.Where(rf => rf.type != "Adjunct" && ((rf.NoForm16 == false || rf.NoForm16 == null) && (rf.NoForm26AS == false || rf.NoForm26AS == null) && (rf.Absent == false) && (rf.NotQualifiedAsperAICTE == false)//&& rf.Noform16Verification == false && rf.NoForm16 == false
                //                                      && (rf.NoSCM17 == false) && (rf.PANNumber != null) && (rf.IncompleteCertificates == false) && (rf.Blacklistfaculy == false) && (rf.Noclass == false || rf.Noclass == null) && rf.NoRelevantUG == "No" && rf.NoRelevantPG == "No" && rf.NORelevantPHD == "No" && rf.NotIdentityfiedForanyProgram == false && (rf.InvalidPANNumber == false || rf.InvalidPANNumber == null) && (rf.PhdUndertakingDocumentstatus == true || rf.PhdUndertakingDocumentstatus == null) && (rf.Invaliddegree == false || rf.Invaliddegree == null)))


                var scm = registeredFaculty.Where(i => i.NoSCM == true).ToList();
                //Reg nos related online facultyIds
                var jntuh_registered_faculty1 = registeredFaculty.Where(rf => rf.type != "Adjunct" && ((rf.NotQualifiedAsperAICTE == false || rf.NotQualifiedAsperAICTE == null) && (rf.Noclass == false || rf.Noclass == null) && (rf.Absent == false || rf.Absent == null) && (rf.OriginalCertificatesNotShown == false || rf.OriginalCertificatesNotShown == null) &&
                                                         (rf.NoSCM == false || rf.NoSCM == null) && rf.NoRelevantUG != "Yes" && rf.NoRelevantPG != "Yes" && rf.NORelevantPHD != "Yes" && (rf.InvalidPANNumber == false || rf.InvalidPANNumber == null) && (rf.Blacklistfaculy == false) && (rf.AbsentforVerification == false || rf.AbsentforVerification == null) && (rf.DepartmentId != 61 || rf.DepartmentId != 27)))
                                                        .Select(rf => new
                                                        {
                                                            RegistrationNumber = rf.RegistrationNumber,
                                                            Department = rf.jntuh_department != null ? rf.jntuh_department.departmentName : null,
                                                            //Department=
                                                            HighestDegreeID = rf.jntuh_registered_faculty_education.Count() != 0 ? rf.jntuh_registered_faculty_education.Select(e => e.educationId).Max() : 0,
                                                            IsApproved = rf.isApproved,
                                                            PanNumber = rf.PANNumber,
                                                            AadhaarNumber = rf.AadhaarNumber,
                                                            PGSpecializationId = rf.PGSpecialization,
                                                            UGDepartmentId = jntuh_college_faculty_registered.Where(C => C.RegistrationNumber == rf.RegistrationNumber).Select(C => C.DepartmentId).FirstOrDefault(),
                                                            SpecializationId = jntuh_college_faculty_registered.Where(C => C.RegistrationNumber == rf.RegistrationNumber).Select(C => C.SpecializationId).FirstOrDefault(),
                                                            jntuh_registered_faculty_education = rf.jntuh_registered_faculty_education
                                                        }).ToList();
                jntuh_registered_faculty1 = jntuh_registered_faculty1.Where(e => e.HighestDegreeID >= 4).ToList();
                var jntuh_registered_faculty = jntuh_registered_faculty1.Select(rf => new
                {
                    RegistrationNumber = rf.RegistrationNumber,
                    Department = rf.Department,
                    //Department=rf.UGDepartmentId!=null?jntuh_departments.Where(D=>D.id==rf.UGDepartmentId).Select(D=>D.departmentName).FirstOrDefault():"",
                    PGSpecializationId = rf.PGSpecializationId,
                    UGDepartmentId = rf.UGDepartmentId,
                    HighestDegree = jntuh_education_category.Where(c => c.id == rf.HighestDegreeID).Select(c => c.educationCategoryName).FirstOrDefault(),
                    Recruitedfor = jntuh_college_faculty_registered.Where(c => c.RegistrationNumber == rf.RegistrationNumber).Select(c => c.IdentifiedFor).FirstOrDefault(),
                    SpecializationId = jntuh_college_faculty_registered.Where(c => c.RegistrationNumber == rf.RegistrationNumber).Select(c => c.SpecializationId).FirstOrDefault(),
                    PanNumber = rf.PanNumber,
                    AadhaarNumber = rf.AadhaarNumber,
                    //registered_faculty_specialization = rf.jntuh_registered_faculty_education.Where(e => e.jntuh_education_category.educationCategoryName == "PG").FirstOrDefault().specialization != null ? rf.jntuh_registered_faculty_education.Where(e => e.jntuh_education_category.educationCategoryName == "PG").Select(i => i.specialization).FirstOrDefault().ToUpper().Trim() : ""
                    registered_faculty_specialization = rf.SpecializationId != null ? jntuh_specializations.Where(S => S.id == rf.SpecializationId).Select(S => S.specializationName).FirstOrDefault() : rf.jntuh_registered_faculty_education.Where(e => e.jntuh_education_category.educationCategoryName == "PG").FirstOrDefault().specialization != null ? rf.jntuh_registered_faculty_education.Where(e => e.jntuh_education_category.educationCategoryName == "PG").Select(i => i.specialization).FirstOrDefault().ToUpper().Trim() : "",
                }).ToList();


                var bpharmacyintake = 0;
                decimal BpharcyrequiredFaculty = 0;
                decimal PharmDrequiredFaculty = 0;
                decimal PharmDPBrequiredFaculty = 0;
                var pharmacydeptids = new[] { 26, 27, 36, 39 };
                if (collegeId == 42)
                    jntuh_registered_faculty = jntuh_registered_faculty.Where(R => R.UGDepartmentId == 26 || R.UGDepartmentId == 27 || R.UGDepartmentId == 36 || R.UGDepartmentId == 39).ToList();
                collegeIntakeExisting = collegeIntakeExisting.Where(i => pharmacydeptids.Contains(i.DepartmentID)).ToList();
                foreach (var item in collegeIntakeExisting)
                {
                    CollegeFacultyWithIntakeReport intakedetails = new CollegeFacultyWithIntakeReport();
                    int phdFaculty = 0;
                    int pgFaculty = 0;
                    int ugFaculty = 0;

                    intakedetails.collegeId = item.collegeId;
                    intakedetails.collegeCode = jntuh_college.Where(c => c.id == item.collegeId).Select(c => c.collegeCode).FirstOrDefault();
                    intakedetails.collegeName = jntuh_college.Where(c => c.id == item.collegeId).Select(c => c.collegeName).FirstOrDefault();
                    intakedetails.collegeRandomCode = randomcode;
                    intakedetails.Degree = item.Degree;
                    intakedetails.Department = item.Department;
                    intakedetails.Specialization = item.Specialization;
                    intakedetails.specializationId = item.specializationId;
                    intakedetails.DepartmentID = item.DepartmentID;
                    intakedetails.shiftId = item.shiftId;
                    var status = collegeaffliations.Where(i => i.DegreeID == item.degreeID && i.SpecializationId == item.specializationId && i.CollegeId == item.collegeId).ToList();
                    if (status.Count > 0)
                    {
                        intakedetails.AffliationStatus = "A";
                    }
                    intakedetails.approvedIntake1 = GetIntake(item.collegeId, AY1, item.specializationId, item.shiftId, 1, item.degreeID);
                    intakedetails.approvedIntake2 = GetIntake(item.collegeId, AY2, item.specializationId, item.shiftId, 1, item.degreeID);
                    intakedetails.approvedIntake3 = GetIntake(item.collegeId, AY3, item.specializationId, item.shiftId, 1, item.degreeID);
                    intakedetails.approvedIntake4 = GetIntake(item.collegeId, AY4, item.specializationId, item.shiftId, 1, item.degreeID);
                    intakedetails.approvedIntake5 = GetIntake(item.collegeId, AY5, item.specializationId, item.shiftId, 1, item.degreeID);
                    facultystudentRatio = Convert.ToInt32(jntuh_faculty_student_ratio_norms.Where(fn => fn.degreeId == item.degreeID).Select(fn => fn.Norms).FirstOrDefault().Split(':')[1].ToString());

                    if (item.Degree == "B.Tech")
                    {
                        intakedetails.totalIntake = (intakedetails.approvedIntake2) + (intakedetails.approvedIntake3) +
                                                    (intakedetails.approvedIntake4);
                        facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) /
                                       Convert.ToDecimal(facultystudentRatio);
                        intakedetails.requiredFaculty = Math.Round(facultyRatio, 2);
                        //intakedetails.A416TotalFaculty = registeredFaculty.Where(i => i.DepartmentId == item.DepartmentID && (i.jntuh_department.jntuh_degree.degree == "B.Tech" || i.jntuh_department.jntuh_degree.degree == "B.Pharmacy")).ToList().Count;
                    }
                    else if (item.Degree == "M.Tech" || item.Degree == "MBA")
                    {
                        intakedetails.totalIntake = (intakedetails.approvedIntake1) + (intakedetails.approvedIntake2);
                        facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) /
                                       Convert.ToDecimal(facultystudentRatio);
                        intakedetails.requiredFaculty = Math.Round(facultyRatio, 2);

                    }
                    else if (item.Degree == "MCA")
                    {
                        intakedetails.totalIntake = (intakedetails.approvedIntake1) + (intakedetails.approvedIntake2) +
                                                    (intakedetails.approvedIntake3);
                        facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) /
                                       Convert.ToDecimal(facultystudentRatio);
                        intakedetails.requiredFaculty = Math.Round(facultyRatio, 2);
                    }
                    else if (item.Degree == "B.Pharmacy")
                    {
                        intakedetails.totalIntake = (intakedetails.approvedIntake1) + (intakedetails.approvedIntake2) +
                                                    (intakedetails.approvedIntake3) + (intakedetails.approvedIntake4);
                        facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) / 15;
                        // intakedetails.requiredFaculty = Math.Round(facultyRatio, 2);
                        //var total = intakedetails.totalIntake > 400 ? 100 : 60;
                        //bpharmacyintake = total;
                        int PharmDCount = collegeIntakeExisting.Count(C => C.Degree == "Pharm.D");
                        bpharmacyintake = intakedetails.approvedIntake1 >= 100 ? 100 : 60;
                        if (PharmDCount > 0)
                            intakedetails.requiredFaculty = intakedetails.approvedIntake1 >= 100 ? 35 : 25;
                        else
                            intakedetails.requiredFaculty = intakedetails.approvedIntake1 >= 100 ? 25 : 15;
                        ApprovedIntake = intakedetails.approvedIntake1 >= 100 ? 100 : 60;
                        specializationId = intakedetails.specializationId;
                    }
                    else if (item.Degree == "M.Pharmacy")
                    {
                        intakedetails.totalIntake = (intakedetails.approvedIntake1) + (intakedetails.approvedIntake2);
                        facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) / 12;

                        //intakedetails.requiredFaculty = Math.Round(facultyRatio, 2);
                        intakedetails.requiredFaculty = 2;

                    }
                    else if (item.Degree == "Pharm.D")
                    {
                        intakedetails.totalIntake = pharmadTotalintake = (intakedetails.approvedIntake1) + (intakedetails.approvedIntake2) +
                                                    (intakedetails.approvedIntake3) + (intakedetails.approvedIntake4) +
                                                    (intakedetails.approvedIntake5);
                        facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) / 12;
                        intakedetails.requiredFaculty = Math.Round(facultyRatio, 2);
                        PharmaDApprovedIntake = intakedetails.approvedIntake1;
                        PharmaDspecializationId = intakedetails.specializationId;
                    }
                    else if (item.Degree == "Pharm.D PB")
                    {
                        intakedetails.totalIntake = pharmadPBTotalintake = (intakedetails.approvedIntake1) + (intakedetails.approvedIntake2);
                        facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) / 10;
                        intakedetails.requiredFaculty = Math.Round(facultyRatio, 2);
                        PharmaDPBApprovedIntake = intakedetails.approvedIntake1;
                        PharmaDPBspecializationId = intakedetails.specializationId;
                    }
                    else //MAM MTM Pharm.D Pharm.D PB
                    {
                        intakedetails.totalIntake = (intakedetails.approvedIntake1) + (intakedetails.approvedIntake2) +
                                                    (intakedetails.approvedIntake3) + (intakedetails.approvedIntake4) +
                                                    (intakedetails.approvedIntake5);
                        facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) / Convert.ToDecimal(facultystudentRatio);
                        intakedetails.requiredFaculty = Math.Round(facultyRatio, 2);
                    }


                    intakedetails.degreeDisplayOrder = item.degreeDisplayOrder;

                    //====================================
                    // intakedetails.specializationWiseFaculty = jntuh_registered_faculty.Where(s => s.SpecializationId == item.specializationId).Count();



                    string strdegreetype = jntuh_degree.Where(d => d.degree == item.Degree).Select(d => d.jntuh_degree_type.degreeType).FirstOrDefault();
                    if (strdegreetype == "UG")
                    {
                        if (item.Degree == "B.Pharmacy")
                        {
                            intakedetails.SortId = 1;
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
                            intakedetails.SortId = 4;
                            intakedetails.specializationWiseFaculty = jntuh_registered_faculty.Count(f => f.Department == "Pharmacy" && (f.Recruitedfor == "PG" || f.Recruitedfor == "UG&PG") && f.SpecializationId == item.specializationId);
                            switch (item.specializationId)
                            {
                                case 114://Hospital & Clinical Pharmacy
                                    intakedetails.PharmacySpec1 = "1 PHD & Specialized in Pharmacology/Pharmacognosy/HCP/Pharma Practice/Pharm D";
                                    //intakedetails.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.SpecializationId == item.specializationId || f.registered_faculty_specialization == "Pharmacology".ToUpper() || f.registered_faculty_specialization == "Pharmacognosy".ToUpper() || f.registered_faculty_specialization == "HCP" || f.registered_faculty_specialization == "Pharmacy Practice".ToUpper() || f.registered_faculty_specialization == "PHARMD".ToUpper() || f.registered_faculty_specialization == "PHARM D" || f.registered_faculty_specialization.Contains("HOSPITAL".ToUpper()) || f.registered_faculty_specialization == "Hospital & Clinical Pharmacy".ToUpper()));

                                    intakedetails.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.PGSpecializationId == item.specializationId));
                                    phdFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.PGSpecializationId == item.specializationId));
                                    intakedetails.AvailableFaculty = jntuh_registered_faculty.Count(f => f.PGSpecializationId == item.specializationId);
                                    //intakedetails.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.PGSpecializationId == 114));
                                    //phdFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.SpecializationId == item.specializationId || f.registered_faculty_specialization == "Hospital & Clinical Pharmacy".ToUpper()));
                                    break;
                                case 116://Pharmaceutical Analysis & Quality Assurance
                                    intakedetails.PharmacySpec1 = "1 PHD & Specialized in PAQA/QA/PA RA/Pharma Chemistry";
                                    //intakedetails.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.SpecializationId == item.specializationId || f.registered_faculty_specialization == "PAQA".ToUpper() || f.registered_faculty_specialization == "PA & QA".ToUpper() || f.registered_faculty_specialization == "QA" || f.registered_faculty_specialization == "PA RA" || f.registered_faculty_specialization == "Pharmaceutical Chemistry".ToUpper() || f.registered_faculty_specialization.Contains("QAPRA") || f.registered_faculty_specialization == "Pharmaceutical Analysis & Quality Assurance".ToUpper()));
                                    intakedetails.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.PGSpecializationId == item.specializationId));
                                    phdFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.PGSpecializationId == item.specializationId));
                                    intakedetails.AvailableFaculty = jntuh_registered_faculty.Count(f => f.PGSpecializationId == item.specializationId);
                                    break;
                                case 118://Pharmaceutical Management & Regulatory Affaires
                                    intakedetails.PharmacySpec1 = "1 PHD & Specialized in PAQA/QA/PMRA/Regulatory Affairs/Pharmaceutics";
                                    //intakedetails.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.SpecializationId == item.specializationId || f.registered_faculty_specialization == "PAQA".ToUpper() || f.registered_faculty_specialization == "PA & QA".ToUpper() || f.registered_faculty_specialization == "QA".ToUpper() || f.registered_faculty_specialization == "PMRA".ToUpper() || f.registered_faculty_specialization == "Regulatory Affairs".ToUpper() || f.registered_faculty_specialization == "Pharmaceutics".ToUpper() || f.registered_faculty_specialization == "Pharmaceutical Management & Regulatory Affaires".ToUpper()));
                                    intakedetails.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.PGSpecializationId == item.specializationId));
                                    phdFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.PGSpecializationId == item.specializationId));
                                    intakedetails.AvailableFaculty = jntuh_registered_faculty.Count(f => f.PGSpecializationId == item.specializationId);
                                    break;
                                case 120://Pharmaceutics
                                    intakedetails.PharmacySpec1 = "1 PHD & Specialized in Pharmaceutics/Pharmaceutical Technology/Industrial Pharmacy/Pharma Biotechnology";
                                    //intakedetails.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.SpecializationId == item.specializationId || f.registered_faculty_specialization == "Pharmaceutics".ToUpper() || f.registered_faculty_specialization == "Pharmaceutical Technology".ToUpper() || f.registered_faculty_specialization == "Industrial Pharmacy".ToUpper() || f.registered_faculty_specialization == "Pharmacy Biotechnology".ToUpper() || f.registered_faculty_specialization.Contains("Biotechnology".ToUpper()) || f.registered_faculty_specialization == "Pharmaceutics".ToUpper()));
                                    intakedetails.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.PGSpecializationId == item.specializationId));//|| f.registered_faculty_specialization == "Pharmaceutics".ToUpper();
                                    phdFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.PGSpecializationId == item.specializationId));// || f.registered_faculty_specialization == "Pharmaceutics".ToUpper()
                                    intakedetails.AvailableFaculty = jntuh_registered_faculty.Count(f => f.PGSpecializationId == item.specializationId);
                                    break;
                                case 122://Pharmacology
                                    intakedetails.PharmacySpec1 = "1 PHD & Specialized in Pharmacology/Pharmacognosy/HCP/Pharma Practice";
                                    //intakedetails.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.SpecializationId == item.specializationId || f.registered_faculty_specialization == "Pharmacology".ToUpper() || f.registered_faculty_specialization == "Pharmacognosy".ToUpper() || f.registered_faculty_specialization == "HCP".ToUpper() || f.registered_faculty_specialization == "Pharmacy Practice".ToUpper() || f.registered_faculty_specialization.Contains("HOSPITAL".ToUpper()) || f.registered_faculty_specialization == "Pharmacology".ToUpper()));
                                    intakedetails.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.PGSpecializationId == item.specializationId));
                                    phdFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.PGSpecializationId == item.specializationId));
                                    intakedetails.AvailableFaculty = jntuh_registered_faculty.Count(f => f.PGSpecializationId == item.specializationId);
                                    break;
                                case 124://Quality Assurance & Pharma Regulatory Affairs
                                    intakedetails.PharmacySpec1 = "1 PHD & Specialized in PAQA/QA/PA RA/Pharmaceuticalchemistry";
                                    //var s = jntuh_registered_faculty.Where(f => (f.registered_faculty_specialization == "PAQA".ToUpper() || f.registered_faculty_specialization == "PA & QA".ToUpper() ||
                                    //             f.registered_faculty_specialization == "QA".ToUpper() ||
                                    //             f.registered_faculty_specialization == "PA RA".ToUpper() ||
                                    //             f.registered_faculty_specialization == "Pharmaceutical Chemistry".ToUpper() || f.registered_faculty_specialization.Contains("QAPRA"))).ToList();
                                    //intakedetails.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.SpecializationId == item.specializationId || f.registered_faculty_specialization == "PAQA".ToUpper() || f.registered_faculty_specialization == "PA & QA".ToUpper() || f.registered_faculty_specialization == "QA".ToUpper() || f.registered_faculty_specialization == "PA RA".ToUpper() || f.registered_faculty_specialization == "Pharmaceutical Chemistry".ToUpper() || f.registered_faculty_specialization.Contains("QAPRA") || f.registered_faculty_specialization == "Quality Assurance & Pharma Regulatory Affairs".ToUpper()));
                                    intakedetails.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.PGSpecializationId == item.specializationId));
                                    phdFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.PGSpecializationId == item.specializationId));
                                    intakedetails.AvailableFaculty = jntuh_registered_faculty.Count(f => f.PGSpecializationId == item.specializationId);
                                    break;
                                case 115://Industrial Pharmacy
                                    intakedetails.PharmacySpec1 = "1 PHD & Specialized in Pharmaceutics/Pharmaceutical Technology/Industrial Pharmacy/Pharma Biotechnology";
                                    //intakedetails.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.SpecializationId == item.specializationId || f.registered_faculty_specialization == "Pharmaceutics".ToUpper() || f.registered_faculty_specialization == "Pharmaceutical Technology".ToUpper() || f.registered_faculty_specialization == "Industrial Pharmacy".ToUpper() || f.registered_faculty_specialization == "Pharmacy Biotechnology".ToUpper() || f.registered_faculty_specialization.Contains("Biotechnology".ToUpper())));
                                    intakedetails.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.PGSpecializationId == item.specializationId));
                                    phdFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.PGSpecializationId == item.specializationId));
                                    intakedetails.AvailableFaculty = jntuh_registered_faculty.Count(f => f.PGSpecializationId == item.specializationId);
                                    break;
                                case 121://Pharmacognosy
                                    intakedetails.PharmacySpec1 = "1 PHD & Specialized in Pharmacognosy/Pharma Biotechnology/Pharmacology";
                                    //intakedetails.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.SpecializationId == item.specializationId || f.registered_faculty_specialization == "Pharmacology".ToUpper() || f.registered_faculty_specialization == "Pharmacognosy".ToUpper() || f.registered_faculty_specialization == "Pharmacy Biotechnology".ToUpper() || f.registered_faculty_specialization.Contains("Biotechnology".ToUpper())));
                                    intakedetails.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.PGSpecializationId == item.specializationId));
                                    phdFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.PGSpecializationId == item.specializationId));
                                    intakedetails.AvailableFaculty = jntuh_registered_faculty.Count(f => f.PGSpecializationId == item.specializationId);
                                    break;
                                case 117://Pharmaceutical Chemistry
                                    intakedetails.PharmacySpec1 = "1 PHD & Specialized in PAQA/QA/Pharmaceuticalchemistry";
                                    //intakedetails.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.SpecializationId == item.specializationId || f.registered_faculty_specialization == "PAQA".ToUpper() || f.registered_faculty_specialization == "PA & QA".ToUpper() || f.registered_faculty_specialization == "QA".ToUpper() || f.registered_faculty_specialization == "Pharmaceutical Chemistry".ToUpper() || f.registered_faculty_specialization.Contains("QAPRA")));
                                    intakedetails.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.PGSpecializationId == item.specializationId));
                                    phdFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.PGSpecializationId == item.specializationId));
                                    intakedetails.AvailableFaculty = jntuh_registered_faculty.Count(f => f.PGSpecializationId == item.specializationId);
                                    break;
                                case 119://Pharmaceutical Technology (2011-12)
                                    intakedetails.PharmacySpec1 = "1 PHD & Specialized in Pharmaceutics/Pharmaceutical Technology/Industrial Pharmacy/Pharma Biotechnology";
                                    //intakedetails.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.SpecializationId == item.specializationId || f.registered_faculty_specialization == "Pharmaceutics".ToUpper() || f.registered_faculty_specialization == "Pharmaceutical Technology".ToUpper() || f.registered_faculty_specialization == "Industrial Pharmacy".ToUpper() || f.registered_faculty_specialization == "Pharmacy Biotechnology".ToUpper() || f.registered_faculty_specialization.Contains("Biotechnology".ToUpper()) || f.registered_faculty_specialization.Contains("Bio-Technology".ToUpper()) || f.registered_faculty_specialization.Contains("Pharmaceutical Technology (2011-12)".ToUpper())));
                                    intakedetails.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.PGSpecializationId == item.specializationId));
                                    phdFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.SpecializationId == item.specializationId));
                                    intakedetails.AvailableFaculty = jntuh_registered_faculty.Count(f => f.PGSpecializationId == item.specializationId);
                                    break;
                                case 123://Quality Assurance
                                    intakedetails.PharmacySpec1 = "1 PHD & Specialized in PAQA/QA/PA RA/Pharmaceuticalchemistry";
                                    //intakedetails.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.SpecializationId == item.specializationId || f.registered_faculty_specialization == "PAQA".ToUpper() || f.registered_faculty_specialization == "PA & QA".ToUpper() || f.registered_faculty_specialization == "QA".ToUpper() || f.registered_faculty_specialization == "PA RA".ToUpper() || f.registered_faculty_specialization == "Pharmaceutical Chemistry".ToUpper() || f.registered_faculty_specialization.Contains("QAPRA")));
                                    intakedetails.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.PGSpecializationId == item.specializationId));
                                    phdFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.PGSpecializationId == item.specializationId));
                                    intakedetails.AvailableFaculty = jntuh_registered_faculty.Count(f => f.PGSpecializationId == item.specializationId);
                                    break;
                                default:
                                    intakedetails.PharmacySpec1 = "";
                                    intakedetails.PharmacyspecializationWiseFaculty = 0;
                                    phdFaculty = 0;
                                    break;
                            }
                        }
                        else
                        {
                            intakedetails.specializationWiseFaculty = jntuh_registered_faculty.Count(f => f.Department == item.Department && (f.Recruitedfor == "PG" || f.Recruitedfor == "UG&PG") && f.SpecializationId == item.specializationId);
                        }
                    }

                    if (strdegreetype == "Dual Degree")
                    {
                        intakedetails.specializationWiseFaculty = jntuh_registered_faculty.Count(f => (f.Recruitedfor == "PG" || f.Recruitedfor == "UG&PG") && f.SpecializationId == item.specializationId);
                    }
                    intakedetails.id = jntuh_Bpharmacy_faculty_deficiency.Where(fd => fd.CollegeId == collegeId && fd.SpecializationId == item.specializationId && fd.ShiftId == item.shiftId).Select(fd => fd.Id).FirstOrDefault();

                    if (intakedetails.id > 0)
                    {
                        int? swf = jntuh_Bpharmacy_faculty_deficiency.Where(fd => fd.Id == intakedetails.id).Select(fd => fd.SpecializationWiseFaculty).FirstOrDefault();
                        if (swf != null)
                        {
                            intakedetails.specializationWiseFaculty = (int)swf;
                        }
                        intakedetails.deficiency = jntuh_Bpharmacy_faculty_deficiency.Where(fd => fd.Id == intakedetails.id).Select(fd => fd.Deficiency).FirstOrDefault();
                        intakedetails.shortage = jntuh_Bpharmacy_faculty_deficiency.Where(fd => fd.Id == intakedetails.id).Select(fd => fd.Shortage).FirstOrDefault();
                    }

                    //============================================

                    int noPanOrAadhaarcount = 0;

                    if (item.Degree == "B.Pharmacy")
                    {
                        intakedetails.SortId = 1;
                        BpharcyrequiredFaculty = Math.Round(intakedetails.requiredFaculty);
                        ugFaculty = jntuh_registered_faculty.Count(f => "UG" == f.HighestDegree && f.Department == "Pharmacy");
                        pgFaculty = jntuh_registered_faculty.Count(f => ("PG" == f.HighestDegree || "M.Phil" == f.HighestDegree) && f.Department == "Pharmacy");
                        phdFaculty = jntuh_registered_faculty.Count(f => "Ph.D" == f.HighestDegree && f.Department == "Pharmacy");
                        noPanOrAadhaarcount = registeredFaculty.Count(f => f.DepartmentId == pharmacyDeptId && f.PANNumber == null && f.AadhaarNumber == null && (f.isApproved == null || f.isApproved == true));
                        intakedetails.Department = "Pharmacy";
                    }
                    else if (item.Degree == "M.Pharmacy")
                    {
                        intakedetails.SortId = 4;
                        ugFaculty = jntuh_registered_faculty.Count(f => "UG" == f.HighestDegree && f.Department == "Pharmacy" && f.SpecializationId == item.specializationId);
                        pgFaculty = jntuh_registered_faculty.Count(f => ("PG" == f.HighestDegree || "M.Phil" == f.HighestDegree) && f.Department == "Pharmacy" && f.SpecializationId == item.specializationId);
                        //phdFaculty = jntuh_registered_faculty.Count(f => "Ph.D" == f.HighestDegree && f.Department == "Pharmacy" && f.SpecializationId == item.specializationId);
                        noPanOrAadhaarcount = registeredFaculty.Count(f => f.DepartmentId == pharmacyDeptId && f.PANNumber == null && f.AadhaarNumber == null && (f.isApproved == null || f.isApproved == true));
                        intakedetails.Department = "Pharmacy";
                    }
                    else if (item.Degree == "Pharm.D")
                    {
                        intakedetails.SortId = 2;
                        PharmDRequiredFaculty = PharmDrequiredFaculty = intakedetails.requiredFaculty;
                        ugFaculty = jntuh_registered_faculty.Count(f => "UG" == f.HighestDegree && f.Department == "Pharm.D" && f.SpecializationId == item.specializationId);
                        pgFaculty = jntuh_registered_faculty.Count(f => ("PG" == f.HighestDegree || "M.Phil" == f.HighestDegree) && f.Department == "Pharm.D" && f.SpecializationId == item.specializationId);
                        phdFaculty = jntuh_registered_faculty.Count(f => "Ph.D" == f.HighestDegree && f.Department == "Pharm.D");
                        noPanOrAadhaarcount = registeredFaculty.Count(f => f.DepartmentId == pharmacyDeptId && f.PANNumber == null && f.AadhaarNumber == null && (f.isApproved == null || f.isApproved == true));
                    }
                    else if (item.Degree == "Pharm.D PB")
                    {
                        intakedetails.SortId = 3;
                        PharmDPBRequiredFaculty = PharmDPBrequiredFaculty = intakedetails.requiredFaculty;
                        ugFaculty = jntuh_registered_faculty.Count(f => "UG" == f.HighestDegree && f.Department == "Pharm.D PB" && f.SpecializationId == item.specializationId);
                        pgFaculty = jntuh_registered_faculty.Count(f => ("PG" == f.HighestDegree || "M.Phil" == f.HighestDegree) && f.Department == "Pharm.D PB" && f.SpecializationId == item.specializationId);
                        phdFaculty = jntuh_registered_faculty.Count(f => "Ph.D" == f.HighestDegree && f.Department == "Pharm.D PB" && f.SpecializationId == item.specializationId);
                        noPanOrAadhaarcount = registeredFaculty.Count(f => f.DepartmentId == pharmacyDeptId && f.PANNumber == null && f.AadhaarNumber == null && (f.isApproved == null || f.isApproved == true));
                    }
                    else
                    {
                        ugFaculty = jntuh_registered_faculty.Count(f => "UG" == f.HighestDegree && f.Department == item.Department);
                        pgFaculty = jntuh_registered_faculty.Count(f => ("PG" == f.HighestDegree || "M.Phil" == f.HighestDegree) && f.Department == item.Department);
                        phdFaculty = jntuh_registered_faculty.Count(f => "Ph.D" == f.HighestDegree && f.Department == item.Department);
                        noPanOrAadhaarcount = registeredFaculty.Count(f => f.DepartmentId == item.DepartmentID && f.PANNumber == null && f.AadhaarNumber == null && (f.isApproved == null || f.isApproved == true));
                    }

                    intakedetails.phdFaculty = phdFaculty;
                    intakedetails.pgFaculty = pgFaculty;
                    intakedetails.ugFaculty = ugFaculty;
                    intakedetails.totalFaculty = (ugFaculty + pgFaculty + phdFaculty);

                    intakedetails.facultyWithoutPANAndAadhaar = noPanOrAadhaarcount;
                    //=============//


                    //intakedetails.PharmacySpecilaizationList = pharmacyspeclist;
                    intakedetailsList.Add(intakedetails);
                }
            #endregion

                var pharmdspeclist = new List<PharmacySpecilaizationList>
                {
                    //new PharmacySpecilaizationList()
                    //{
                    //    PharmacyspecName = "Pharmacy Practice",
                    //    Specialization = "Pharm.D"
                    //},
                    //new PharmacySpecilaizationList()
                    //{
                    //    PharmacyspecName = "Pharm D",
                    //    Specialization = "Pharm.D"
                    //}
                    new PharmacySpecilaizationList()
                    {
                        PharmacyspecName = "Group1 (Pharmacy Practice, Pharm D, Pharmacology)",
                        Specialization = "Pharm.D"
                    }
                };
                var pharmdpbspeclist = new List<PharmacySpecilaizationList>
                {
                    //new PharmacySpecilaizationList()
                    //{
                    //    PharmacyspecName = "Pharmacy Practice",
                    //    Specialization = "Pharm.D PB"
                    //},
                    //new PharmacySpecilaizationList()
                    //{
                    //    PharmacyspecName = "Pharm D",
                    //    Specialization = "Pharm.D PB"
                    //}
                    new PharmacySpecilaizationList()
                    {
                       PharmacyspecName = "Group1 (Pharmacy Practice, Pharm D, Pharmacology)",
                        Specialization = "Pharm.D PB"
                    }
                };

                var pharmacyspeclist = new List<PharmacySpecilaizationList>
                {
                    //new PharmacySpecilaizationList()
                    //{
                    //    PharmacyspecName = "Pharmaceutics",
                    //    Specialization = "B.Pharmacy"
                    //},
                    //new PharmacySpecilaizationList()
                    //{
                    //    PharmacyspecName = "Industrial Pharmacy",
                    //    Specialization = "B.Pharmacy"
                    //},
                    //new PharmacySpecilaizationList()
                    //{
                    //    PharmacyspecName = "Pharmacy BioTechnology",
                    //    Specialization = "B.Pharmacy"
                    //},
                    //new PharmacySpecilaizationList()
                    //{
                    //    PharmacyspecName = "Pharmaceutical Technology",
                    //    Specialization = "B.Pharmacy"
                    //},
                    //new PharmacySpecilaizationList()
                    //{
                    //    PharmacyspecName = "Pharmaceutical Chemistry",
                    //    Specialization = "B.Pharmacy"
                    //},
                    //new PharmacySpecilaizationList()
                    //{
                    //    PharmacyspecName = "Pharmacy Analysis",
                    //    Specialization = "B.Pharmacy"
                    //},

                    //new PharmacySpecilaizationList()
                    //{
                    //    PharmacyspecName = "PAQA",
                    //    Specialization = "B.Pharmacy"
                    //},
                    //new PharmacySpecilaizationList()
                    //{
                    //    PharmacyspecName = "Pharmacology",
                    //    Specialization = "B.Pharmacy"
                    //},
                    ////new PharmacySpecilaizationList()
                    ////{
                    ////    PharmacyspecName = "Pharma D",
                    ////    Specialization = "B.Pharmacy"
                    ////},
                    //new PharmacySpecilaizationList()
                    //{
                    //    PharmacyspecName = "Pharmacognosy",
                    //    Specialization = "B.Pharmacy"
                    //},
                    //new PharmacySpecilaizationList()
                    //{
                    //    PharmacyspecName = "English",
                    //    Specialization = "B.Pharmacy"
                    //},
                    //new PharmacySpecilaizationList()
                    //{
                    //    PharmacyspecName = "Mathematics",
                    //    Specialization = "B.Pharmacy"
                    //},
                    //new PharmacySpecilaizationList()
                    //{
                    //    PharmacyspecName = "Computers",
                    //    Specialization = "B.Pharmacy"
                    //},new PharmacySpecilaizationList()
                    //{
                    //    PharmacyspecName = "Computer Science",
                    //    Specialization = "B.Pharmacy"
                    //},
                    //new PharmacySpecilaizationList()
                    //{
                    //    PharmacyspecName = "Zoology",
                    //    Specialization = "B.Pharmacy"
                    //}


                     new PharmacySpecilaizationList()
                    {
                        PharmacyspecName = "Group1 (Pharmaceutics , Industrial Pharmacy)",//, Industrial Pharmacy, Pharmacy BioTechnology, Pharmaceutical Technology
                        Specialization = "B.Pharmacy"
                    },
                    new PharmacySpecilaizationList()
                    {
                        PharmacyspecName = "Group2 (Pharmaceutical Chemistry,Pharmaceutical Analysis, PAQA, QA, QAPRA)",
                        Specialization = "B.Pharmacy"
                    },
                    //new PharmacySpecilaizationList()
                    //{
                    //    PharmacyspecName = "Group3 (Pharmacy Analysis, PAQA)",
                    //    Specialization = "B.Pharmacy"
                    //},
                    new PharmacySpecilaizationList()
                    {
                        PharmacyspecName = "Group3 (Pharmacology, Pharm-D, Pharm-DPB, Pharmacy Practice)",
                        Specialization = "B.Pharmacy"
                    },
                    new PharmacySpecilaizationList()
                    {
                        PharmacyspecName = "Group4 (Pharmacognosy, Pharmaceutical Chemistry, Pharmaceutical Biotechonology, Phyto Pharmacy, Phyto medicine)",
                        Specialization = "B.Pharmacy"
                    },
                    //new PharmacySpecilaizationList()
                    //{
                    //    PharmacyspecName = "Group6 (English, Mathematics, Computers)",
                    //    Specialization = "B.Pharmacy"
                    //},
                };


                TotalcollegeFaculty = jntuh_registered_faculty.Count;

                #region All B.Pharmacy Specializations

                #region Commented Old Code Start
                //var reg_facultyspecilaizationsdistinct = jntuh_registered_faculty.Select(i => i.registered_faculty_specialization).Distinct().ToArray();
                //var group1Subcount = 0; var group2Subcount = 0; var group3Subcount = 0; var group4Subcount = 0; var group5Subcount = 0; var group6Subcount = 0;
                //var pharmadgroup1Subcount = 0; var pharmadPBgroup1Subcount = 0;
                //string subgroupconditionsmet;
                //string conditionbpharm = null;
                //string conditionpharmd = null;
                //string conditionphardpb = null;
                //foreach (var list in pharmacyspeclist)
                //{
                //    int phd;
                //    int pg;
                //    int ug;
                //    var bpharmacylist = new CollegeFacultyWithIntakeReport();
                //    bpharmacylist.Specialization = list.Specialization;
                //    bpharmacylist.PharmacySpec1 = list.PharmacyspecName;
                //    bpharmacylist.collegeId = (int)collegeId;
                //    bpharmacylist.collegeCode = jntuh_college.Where(c => c.id == collegeId).Select(c => c.collegeCode).FirstOrDefault();
                //    bpharmacylist.collegeName = jntuh_college.Where(c => c.id == collegeId).Select(c => c.collegeName).FirstOrDefault();
                //    bpharmacylist.collegeRandomCode = randomcode;
                //    bpharmacylist.shiftId = 1;
                //    bpharmacylist.Degree = "B.Pharmacy";
                //    bpharmacylist.Department = "Pharmacy";
                //    bpharmacylist.PharmacyGroup1 = "Group1";

                //    bpharmacylist.specializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper());
                //    //bpharmacylist.ugFaculty = ug = jntuh_registered_faculty.Count(f => "UG" == f.HighestDegree && f.Department == "Pharmacy");
                //    //bpharmacylist.pgFaculty = pg = jntuh_registered_faculty.Count(f => ("PG" == f.HighestDegree || "M.Phil" == f.HighestDegree) && f.Department == "Pharmacy");
                //    //bpharmacylist.phdFaculty = phd = jntuh_registered_faculty.Count(f => "Ph.D" == f.HighestDegree && f.Department == "Pharmacy");
                //    //bpharmacylist.totalFaculty = ug + pg + phd;

                //    bpharmacylist.PharmacySpecilaizationList = pharmacyspeclist;
                //    bpharmacylist.BphramacyrequiredFaculty = BpharcyrequiredFaculty;
                //    bpharmacylist.SortId = 1;
                //    bpharmacylist.approvedIntake1 = ApprovedIntake;
                //    bpharmacylist.specializationId = specializationId;
                //    #region bpharmacyspecializationcount

                //    if (list.PharmacyspecName == "Pharmaceutics")
                //    {
                //        bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper());
                //    }

                //    else if (list.PharmacyspecName == "Industrial Pharmacy")
                //    {
                //        bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper());
                //    }
                //    else if (list.PharmacyspecName == "Pharmacy BioTechnology")
                //    {
                //        bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper() ||
                //                        f.registered_faculty_specialization == "Bio-Technology".ToUpper());

                //    }
                //    else if (list.PharmacyspecName == "Pharmaceutical Technology")
                //    {
                //        bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "Pharmaceutical Technology (2011-12)".ToUpper() ||
                //            f.registered_faculty_specialization == "Pharmaceutical Technology".ToUpper());
                //        bpharmacylist.requiredFaculty = 3;
                //    }
                //    else if (list.PharmacyspecName == "Pharmaceutical Chemistry")
                //    {
                //        bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper());
                //        bpharmacylist.requiredFaculty = 2;
                //    }
                //    else if (list.PharmacyspecName == "Pharmacy Analysis")
                //    {
                //        bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper());
                //    }

                //    else if (list.PharmacyspecName == "PAQA")
                //    {
                //        bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper() ||
                //                     f.registered_faculty_specialization == "PA & QA".ToUpper() ||
                //            //f.registered_faculty_specialization == "Quality Assurance".ToUpper() ||
                //            //f.registered_faculty_specialization == "QualityAssurance".ToUpper() ||
                //                     f.registered_faculty_specialization == "QAPRA".ToUpper() ||
                //                     f.registered_faculty_specialization == "Pharmaceutical Analysis & Quality Assurance".ToUpper() ||
                //                     f.registered_faculty_specialization == "Quality Assurance & Pharma Regulatory Affairs".ToUpper());
                //        bpharmacylist.requiredFaculty = 1;
                //    }
                //    else if (list.PharmacyspecName == "Pharmacology")
                //    {
                //        bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper());
                //    }

                //    else if (list.PharmacyspecName == "Pharma D")
                //    {
                //        bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper() ||
                //                       f.registered_faculty_specialization == "PharmD".ToUpper() ||
                //                      f.registered_faculty_specialization == "Pharm D".ToUpper() ||
                //                      f.registered_faculty_specialization == "Pharm.D".ToUpper());
                //        bpharmacylist.requiredFaculty = 2;
                //    }
                //    else if (list.PharmacyspecName == "Pharmacognosy")
                //    {
                //        bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper() ||
                //                      f.registered_faculty_specialization == "Pharmacognosy & Phytochemistry".ToUpper() ||
                //                      f.registered_faculty_specialization == "Pharmacognosy&Phytochemistry".ToUpper());
                //        bpharmacylist.requiredFaculty = 2;
                //    }

                //    else if (list.PharmacyspecName == "English")
                //    {
                //        bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper());
                //    }
                //    else if (list.PharmacyspecName == "Mathematics")
                //    {
                //        bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper());
                //    }
                //    else if (list.PharmacyspecName == "Computers")
                //    {
                //        bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper());
                //    }
                //    else if (list.PharmacyspecName == "Computer Science")
                //    {
                //        bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper());
                //    }
                //    else if (list.PharmacyspecName == "Zoology")
                //    {
                //        bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper());
                //        bpharmacylist.requiredFaculty = bpharmacyintake == 100 ? 3 : 2;
                //    }
                //    #endregion





                //    if (list.PharmacyspecName == "Group1 (Pharmaceutics)" || list.PharmacyspecName == "Pharmaceutics" || list.PharmacyspecName == "Industrial Pharmacy" || list.PharmacyspecName == "Pharmacy BioTechnology" || list.PharmacyspecName == "Pharmaceutical Technology")
                //    {
                //        group1Subcount = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "Pharmaceutics".ToUpper());
                //        //jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "Industrial Pharmacy".ToUpper()) +
                //        //jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "Pharmacy BioTechnology".ToUpper()) +
                //        //jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "Bio-Technology".ToUpper()) +
                //        //jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "Pharmaceutical Technology (2011-12)".ToUpper()) +
                //        //jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "Pharmaceutical Technology".ToUpper());
                //        bpharmacylist.BPharmacySubGroup1Count = group1Subcount;
                //        bpharmacylist.BPharmacySubGroupRequired = bpharmacyintake >= 100 ? 6 : 4;
                //        bpharmacylist.PharmacySubGroup1 = "Group1 (Pharmaceutics)";
                //        bpharmacylist.requiredFaculty = bpharmacyintake >= 100 ? 6 : 4;
                //    }

                //    else if (list.PharmacyspecName == "Group2 (Pharmaceutical Chemistry,Pharmacy Analysis, PAQA)" || list.PharmacyspecName == "Pharmaceutical Chemistry")
                //    {
                //        group2Subcount = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "Pharmaceutical Chemistry".ToUpper()) +
                //                        jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "Pharmacy Analysis".ToUpper()) +
                //                        jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "PAQA".ToUpper()) +
                //                        jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "PA & QA".ToUpper()) +
                //                        jntuh_registered_faculty.Count(f => f.registered_faculty_specialization.Contains("QAPRA")) +
                //                        jntuh_registered_faculty.Count(f => f.registered_faculty_specialization.Contains("Pharmaceutical Analysis & Quality Assurance".ToUpper())) +
                //                        jntuh_registered_faculty.Count(f => f.registered_faculty_specialization.Contains("Quality Assurance & Pharma Regulatory Affairs".ToUpper()));
                //        bpharmacylist.BPharmacySubGroup1Count = group2Subcount;
                //        bpharmacylist.BPharmacySubGroupRequired = bpharmacyintake >= 100 ? 6 : 5;
                //        bpharmacylist.PharmacySubGroup1 = "Group2 (Pharmaceutical Chemistry,Pharmacy Analysis, PAQA)";
                //        bpharmacylist.requiredFaculty = bpharmacyintake >= 100 ? 6 : 5;
                //    }
                //    //else if (list.PharmacyspecName == "Group3 (Pharmacy Analysis, PAQA)" || list.PharmacyspecName == "Pharmacy Analysis" || list.PharmacyspecName == "PAQA" || list.PharmacyspecName == "PA & QA" || list.PharmacyspecName.Contains("QA"))
                //    //{
                //    //    var y = jntuh_registered_faculty.Where(f => f.registered_faculty_specialization == "Pharmacy Analysis".ToUpper()).ToList();
                //    //    var g = jntuh_registered_faculty.Where(f => f.registered_faculty_specialization.Contains("QA")).ToList();
                //    //    var g1 = jntuh_registered_faculty.Where(f => f.registered_faculty_specialization == "PAQA".ToUpper()).ToList();
                //    //    var g2 = jntuh_registered_faculty.Where(f => f.registered_faculty_specialization == "PA & QA".ToUpper()).ToList();

                //    //    group3Subcount = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "Pharmacy Analysis".ToUpper()) +
                //    //        jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "PAQA".ToUpper()) +
                //    //        jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "PA & QA".ToUpper()) +
                //    //        jntuh_registered_faculty.Count(f => f.registered_faculty_specialization.Contains("QAPRA")) +
                //    //        jntuh_registered_faculty.Count(f => f.registered_faculty_specialization.Contains("Pharmaceutical Analysis & Quality Assurance".ToUpper())) +
                //    //        jntuh_registered_faculty.Count(f => f.registered_faculty_specialization.Contains("Quality Assurance & Pharma Regulatory Affairs".ToUpper()));
                //    //    bpharmacylist.BPharmacySubGroup1Count = group3Subcount;
                //    //    bpharmacylist.BPharmacySubGroupRequired = 1;
                //    //    bpharmacylist.PharmacySubGroup1 = "Group3 (Pharmacy Analysis, PAQA)";
                //    //}

                //    else if (list.PharmacyspecName == "Group3 (Pharmacology)" || list.PharmacyspecName == "Pharmacology" || list.PharmacyspecName == "Pharma D" || list.PharmacyspecName == "Pharma.D" || list.PharmacyspecName == "Pharm D" || list.PharmacyspecName == "Pharm.D")
                //    {
                //        group3Subcount = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "Pharmacology".ToUpper());
                //        bpharmacylist.BPharmacySubGroup1Count = group3Subcount;
                //        bpharmacylist.BPharmacySubGroupRequired = bpharmacyintake >= 100 ? 5 : 4;
                //        bpharmacylist.PharmacySubGroup1 = "Group3 (Pharmacology)";
                //        bpharmacylist.requiredFaculty = bpharmacyintake >= 100 ? 5 : 4;
                //    }

                //    else if (list.PharmacyspecName == "Group4 (Pharmacognosy)" || list.PharmacyspecName == "Pharmacognosy")
                //    {
                //        group4Subcount = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "Pharmacognosy".ToUpper()) +
                //            jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "Pharmacognosy&Phytochemistryc".ToUpper()) +
                //            jntuh_registered_faculty.Count(f => f.registered_faculty_specialization.Contains("PHARMACOGNOSY & PHYTOCHEMISTRY".ToUpper()));
                //        bpharmacylist.BPharmacySubGroup1Count = group4Subcount;
                //        bpharmacylist.BPharmacySubGroupRequired = 3;
                //        bpharmacylist.PharmacySubGroup1 = "Group4 (Pharmacognosy)";
                //        bpharmacylist.requiredFaculty = 3;
                //    }

                //    //else if (list.PharmacyspecName == "Group6 (English, Mathematics, Computers)" || list.PharmacyspecName == "English" || list.PharmacyspecName == "Mathematics" || list.PharmacyspecName == "Computers" || list.PharmacyspecName == "Computer Science")//|| list.PharmacyspecName == "Zoology"
                //    //{
                //    //    group6Subcount = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "English".ToUpper()) +
                //    //        jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "Mathematics".ToUpper()) +
                //    //        jntuh_registered_faculty.Count(f => f.registered_faculty_specialization.Contains("COMPUTER")) +
                //    //        jntuh_registered_faculty.Count(f => f.registered_faculty_specialization.Contains("COMPUTER SCIENCE")) +
                //    //        jntuh_registered_faculty.Count(f => f.registered_faculty_specialization.Contains("CSE"));
                //    //    //jntuh_registered_faculty.Count(f => f.registered_faculty_specialization.Contains("ZOOLOGY"));
                //    //    bpharmacylist.BPharmacySubGroup1Count = group6Subcount;
                //    //    bpharmacylist.BPharmacySubGroupRequired = bpharmacyintake == 100 ? 3 : 2;
                //    //    bpharmacylist.PharmacySubGroup1 = "Group6 (English, Mathematics, Computers)";
                //    //}



                //    var id = jntuh_Bpharmacy_faculty_deficiency.Where(fd => fd.CollegeId == collegeId && fd.BpharmacySpecialization == list.PharmacyspecName && fd.ShiftId == 1).Select(fd => fd.Id).FirstOrDefault();
                //    if (id > 0)
                //    {
                //        var swf = jntuh_Bpharmacy_faculty_deficiency.Where(fd => fd.Id == id).Select(fd => fd.SpecializationWiseFaculty).FirstOrDefault();
                //        if (swf != null)
                //        {
                //            bpharmacylist.specializationWiseFaculty = (int)swf;
                //        }
                //        bpharmacylist.id = id;
                //        bpharmacylist.deficiency = jntuh_Bpharmacy_faculty_deficiency.Where(fd => fd.Id == id).Select(fd => fd.Deficiency).FirstOrDefault();
                //        bpharmacylist.shortage = jntuh_Bpharmacy_faculty_deficiency.Where(fd => fd.Id == id).Select(fd => fd.Shortage).FirstOrDefault();
                //    }

                //    intakedetailsList.Add(bpharmacylist);
                //}

                ////for pharma D specializations
                //var pharmaD = collegeIntakeExisting.Where(i => i.specializationId == 18).ToList();
                //if (pharmaD.Count > 0)
                //{
                //    foreach (var list in pharmdspeclist)
                //    {
                //        int phd;
                //        int pg;
                //        int ug;
                //        var bpharmacylist = new CollegeFacultyWithIntakeReport();
                //        bpharmacylist.Specialization = list.Specialization;
                //        bpharmacylist.PharmacySpec1 = list.PharmacyspecName;
                //        bpharmacylist.collegeId = (int)collegeId;
                //        bpharmacylist.collegeRandomCode = randomcode;
                //        bpharmacylist.collegeCode = jntuh_college.Where(c => c.id == collegeId).Select(c => c.collegeCode).FirstOrDefault();
                //        bpharmacylist.collegeName = jntuh_college.Where(c => c.id == collegeId).Select(c => c.collegeName).FirstOrDefault();
                //        bpharmacylist.shiftId = 1;
                //        bpharmacylist.Degree = "Pharm.D";
                //        bpharmacylist.Department = "Pharm.D";
                //        bpharmacylist.PharmacyGroup1 = "Group1";
                //        //bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper());
                //        bpharmacylist.specializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper());
                //        bpharmacylist.ugFaculty = ug = jntuh_registered_faculty.Count(f => "UG" == f.HighestDegree && f.Department == "Pharm.D");
                //        bpharmacylist.pgFaculty = pg = jntuh_registered_faculty.Count(f => ("PG" == f.HighestDegree || "M.Phil" == f.HighestDegree) && f.Department == "Pharm.D");
                //        bpharmacylist.phdFaculty = phd = jntuh_registered_faculty.Count(f => "Ph.D" == f.HighestDegree && f.Department == "Pharm.D");
                //        bpharmacylist.totalFaculty = ug + pg + phd;
                //        //bpharmacylist.requiredFaculty = BpharcyrequiredFaculty;
                //        bpharmacylist.PharmacySpecilaizationList = pharmacyspeclist;
                //        bpharmacylist.totalcollegefaculty = jntuh_registered_faculty.Count;
                //        bpharmacylist.pharmadrequiredfaculty = Math.Ceiling(PharmDrequiredFaculty);
                //        bpharmacylist.SortId = 2;
                //        bpharmacylist.approvedIntake1 = PharmaDApprovedIntake;
                //        bpharmacylist.specializationId = PharmaDspecializationId;
                //        #region pharmadSpecializationcount
                //        if (list.PharmacyspecName == "Group1 (Pharmacy Practice, Pharm D)" || list.PharmacyspecName == "Pharm D")
                //        {
                //            bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper() ||
                //                           f.registered_faculty_specialization == "PharmD".ToUpper() ||
                //                          f.registered_faculty_specialization == "Pharm D".ToUpper() ||
                //                          f.registered_faculty_specialization == "Pharma.D".ToUpper() ||
                //                          f.registered_faculty_specialization == "Pharma D".ToUpper());
                //        }
                //        else if (list.PharmacyspecName == "Group1 (Pharmacy Practice, Pharm D)" || list.PharmacyspecName == "Pharmacy Practice")
                //        {
                //            bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper());
                //        }
                //        #endregion


                //        if (list.PharmacyspecName == "Group1 (Pharmacy Practice, Pharm D)" || list.PharmacyspecName == "Pharmacy Practice" || list.PharmacyspecName == "Pharm D" || list.PharmacyspecName == "Pharm.D")
                //        {
                //            pharmadgroup1Subcount = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "Pharmacy Practice".ToUpper()) +
                //                        jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "Pharma D".ToUpper()) +
                //                        jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "Pharma.D".ToUpper()) +
                //                        jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "Pharm D".ToUpper()) +
                //                        jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "Pharm.D".ToUpper()) +
                //                        jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "PharmD".ToUpper());
                //            bpharmacylist.BPharmacySubGroup1Count = pharmadgroup1Subcount;
                //            bpharmacylist.BPharmacySubGroupRequired = pharmadTotalintake / 30;
                //            bpharmacylist.PharmacySubGroup1 = "Group1 (Pharmacy Practice, Pharm D)";
                //            bpharmacylist.requiredFaculty = Math.Ceiling(PharmDrequiredFaculty);
                //        }


                //        var id = jntuh_Bpharmacy_faculty_deficiency.Where(fd => fd.CollegeId == collegeId && fd.BpharmacySpecialization == list.PharmacyspecName && fd.ShiftId == 1).Select(fd => fd.Id).FirstOrDefault();
                //        if (id > 0)
                //        {
                //            var swf = jntuh_Bpharmacy_faculty_deficiency.Where(fd => fd.Id == id).Select(fd => fd.SpecializationWiseFaculty).FirstOrDefault();
                //            if (swf != null)
                //            {
                //                bpharmacylist.specializationWiseFaculty = (int)swf;
                //            }
                //            bpharmacylist.id = id;
                //            bpharmacylist.deficiency = jntuh_Bpharmacy_faculty_deficiency.Where(fd => fd.Id == id).Select(fd => fd.Deficiency).FirstOrDefault();
                //            bpharmacylist.shortage = jntuh_Bpharmacy_faculty_deficiency.Where(fd => fd.Id == id).Select(fd => fd.Shortage).FirstOrDefault();
                //        }

                //        intakedetailsList.Add(bpharmacylist);
                //    }
                //}


                ////for pharma.D PB specializations
                //var pharmaDPB = collegeIntakeExisting.Where(i => i.specializationId == 19).ToList();
                //if (pharmaDPB.Count > 0)
                //{
                //    foreach (var list in pharmdpbspeclist)
                //    {
                //        int phd;
                //        int pg;
                //        int ug;
                //        var bpharmacylist = new CollegeFacultyWithIntakeReport();
                //        bpharmacylist.Specialization = list.Specialization;
                //        bpharmacylist.PharmacySpec1 = list.PharmacyspecName;
                //        bpharmacylist.collegeId = (int)collegeId;
                //        bpharmacylist.collegeCode = jntuh_college.Where(c => c.id == collegeId).Select(c => c.collegeCode).FirstOrDefault();
                //        bpharmacylist.collegeName = jntuh_college.Where(c => c.id == collegeId).Select(c => c.collegeName).FirstOrDefault();
                //        bpharmacylist.collegeRandomCode = randomcode;
                //        bpharmacylist.shiftId = 1;
                //        bpharmacylist.Degree = "Pharm.D PB";
                //        bpharmacylist.Department = "Pharm.D PB";
                //        bpharmacylist.PharmacyGroup1 = "Group1";
                //        //bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper());
                //        bpharmacylist.specializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper());
                //        bpharmacylist.ugFaculty = ug = jntuh_registered_faculty.Count(f => "UG" == f.HighestDegree && f.Department == "Pharm.D PB");
                //        bpharmacylist.pgFaculty = pg = jntuh_registered_faculty.Count(f => ("PG" == f.HighestDegree || "M.Phil" == f.HighestDegree) && f.Department == "Pharm.D PB");
                //        bpharmacylist.phdFaculty = phd = jntuh_registered_faculty.Count(f => "Ph.D" == f.HighestDegree && f.Department == "Pharm.D PB");
                //        bpharmacylist.totalFaculty = ug + pg + phd;
                //        //bpharmacylist.requiredFaculty = BpharcyrequiredFaculty;
                //        bpharmacylist.pharmadPBrequiredfaculty = Math.Ceiling(PharmDPBrequiredFaculty);
                //        bpharmacylist.PharmacySpecilaizationList = pharmacyspeclist;
                //        bpharmacylist.totalcollegefaculty = jntuh_registered_faculty.Count;
                //        bpharmacylist.SortId = 3;
                //        bpharmacylist.approvedIntake1 = PharmaDPBApprovedIntake;
                //        bpharmacylist.specializationId = PharmaDPBspecializationId;
                //        #region pharmadPbSpecializationcount
                //        if (list.PharmacyspecName == "Group1 (Pharmacy Practice, Pharm D)" || list.PharmacyspecName == "Pharm D")
                //        {
                //            bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper() ||
                //                           f.registered_faculty_specialization == "PharmD".ToUpper() ||
                //                          f.registered_faculty_specialization == "Pharm D".ToUpper() ||
                //                          f.registered_faculty_specialization == "Pharma.D".ToUpper() ||
                //                          f.registered_faculty_specialization == "Pharma D".ToUpper());
                //        }
                //        else if (list.PharmacyspecName == "Group1 (Pharmacy Practice, Pharm D)" || list.PharmacyspecName == "Pharmacy Practice")
                //        {
                //            bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper());
                //        }
                //        #endregion


                //        if (list.PharmacyspecName == "Group1 (Pharmacy Practice, Pharm D)" || list.PharmacyspecName == "Pharmacy Practice" || list.PharmacyspecName == "Pharm D" || list.PharmacyspecName == "Pharm.D")
                //        {
                //            pharmadPBgroup1Subcount = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "Pharmacy Practice".ToUpper()) +
                //                            jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "Pharma D".ToUpper()) +
                //                            jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "Pharma.D".ToUpper()) +
                //                            jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "Pharm D".ToUpper()) +
                //                            jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "Pharm.D".ToUpper()) +
                //                        jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "PharmD".ToUpper()); ;
                //            bpharmacylist.BPharmacySubGroup1Count = pharmadPBgroup1Subcount;
                //            bpharmacylist.BPharmacySubGroupRequired = pharmadPBTotalintake / 10;
                //            bpharmacylist.PharmacySubGroup1 = "Group1 (Pharmacy Practice, Pharm D)";
                //            bpharmacylist.requiredFaculty = Math.Ceiling(PharmDPBrequiredFaculty);
                //        }


                //        var id = jntuh_Bpharmacy_faculty_deficiency.Where(fd => fd.CollegeId == collegeId && fd.BpharmacySpecialization == list.PharmacyspecName && fd.ShiftId == 1).Select(fd => fd.Id).FirstOrDefault();
                //        if (id > 0)
                //        {
                //            var swf = jntuh_Bpharmacy_faculty_deficiency.Where(fd => fd.Id == id).Select(fd => fd.SpecializationWiseFaculty).FirstOrDefault();
                //            if (swf != null)
                //            {
                //                bpharmacylist.specializationWiseFaculty = (int)swf;
                //            }
                //            bpharmacylist.id = id;
                //            bpharmacylist.deficiency = jntuh_Bpharmacy_faculty_deficiency.Where(fd => fd.Id == id).Select(fd => fd.Deficiency).FirstOrDefault();
                //            bpharmacylist.shortage = jntuh_Bpharmacy_faculty_deficiency.Where(fd => fd.Id == id).Select(fd => fd.Shortage).FirstOrDefault();
                //        }

                //        intakedetailsList.Add(bpharmacylist);
                //    }
                //}
                #endregion Commented Old Code End
                string PharmacyDeficiency = "";
                var reg_facultyspecilaizationsdistinct = jntuh_registered_faculty.Select(i => i.registered_faculty_specialization).Distinct().ToArray();
                var group1Subcount = 0; var group2Subcount = 0; var group3Subcount = 0; var group4Subcount = 0; var group5Subcount = 0; var group6Subcount = 0;
                var pharmadgroup1Subcount = 0; var pharmadPBgroup1Subcount = 0;
                string subgroupconditionsmet;
                string conditionbpharm = null;
                string conditionpharmd = null;
                string conditionphardpb = null;
                #region PharmD and PharmDPB
                int pharmaD = collegeIntakeExisting.Count(i => i.specializationId == 18);
                if (pharmaD > 0)
                {
                    List<CollegeFacultyWithIntakeReport> intakedetailsList1 = new List<CollegeFacultyWithIntakeReport>();
                    foreach (var list in pharmacyspeclist)
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
                        bpharmacylist.collegeRandomCode = randomcode;
                        bpharmacylist.shiftId = 1;
                        bpharmacylist.Degree = "B.Pharmacy";
                        bpharmacylist.Department = "Pharmacy";
                        bpharmacylist.PharmacyGroup1 = "Group1";

                        bpharmacylist.specializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper());

                        bpharmacylist.PharmacySpecilaizationList = pharmacyspeclist;
                        bpharmacylist.BphramacyrequiredFaculty = BpharcyrequiredFaculty;
                        bpharmacylist.SortId = 1;
                        #region bpharmacyspecializationcount

                        if (list.PharmacyspecName == "Pharmaceutics")
                        {
                            bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper());
                        }

                        else if (list.PharmacyspecName == "Industrial Pharmacy")
                        {
                            bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper());
                        }
                        else if (list.PharmacyspecName == "Pharmacy BioTechnology")
                        {
                            bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper() ||
                                            f.registered_faculty_specialization == "Bio-Technology".ToUpper());

                        }
                        else if (list.PharmacyspecName == "Pharmaceutical Technology")
                        {
                            bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "Pharmaceutical Technology (2011-12)".ToUpper() ||
                                f.registered_faculty_specialization == "Pharmaceutical Technology".ToUpper());
                            bpharmacylist.requiredFaculty = 3;
                        }
                        else if (list.PharmacyspecName == "Pharmaceutical Chemistry")
                        {
                            bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper());
                            bpharmacylist.requiredFaculty = 2;
                        }
                        else if (list.PharmacyspecName == "Pharmacy Analysis")
                        {
                            bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper());
                        }

                        else if (list.PharmacyspecName == "PAQA")
                        {
                            bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper() ||
                                         f.registered_faculty_specialization == "PA & QA".ToUpper() ||
                                         f.registered_faculty_specialization == "QAPRA".ToUpper() ||
                                         f.registered_faculty_specialization == "Pharmaceutical Analysis & Quality Assurance".ToUpper() ||
                                         f.registered_faculty_specialization == "Quality Assurance & Pharma Regulatory Affairs".ToUpper());
                            bpharmacylist.requiredFaculty = 1;
                        }
                        else if (list.PharmacyspecName == "Pharmacology")
                        {
                            bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper());
                        }

                        else if (list.PharmacyspecName == "Pharma D")
                        {
                            bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper() ||
                                           f.registered_faculty_specialization == "PharmD".ToUpper() ||
                                          f.registered_faculty_specialization == "Pharm D".ToUpper() ||
                                          f.registered_faculty_specialization == "Pharm.D".ToUpper());
                            bpharmacylist.requiredFaculty = 2;
                        }
                        else if (list.PharmacyspecName == "Pharmacognosy")
                        {
                            bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper() ||
                                          f.registered_faculty_specialization == "Pharmacognosy & Phytochemistry".ToUpper() ||
                                          f.registered_faculty_specialization == "Pharmacognosy&Phytochemistry".ToUpper());
                            bpharmacylist.requiredFaculty = 2;
                        }

                        else if (list.PharmacyspecName == "English")
                        {
                            bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper());
                        }
                        else if (list.PharmacyspecName == "Mathematics")
                        {
                            bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper());
                        }
                        else if (list.PharmacyspecName == "Computers")
                        {
                            bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper());
                        }
                        else if (list.PharmacyspecName == "Computer Science")
                        {
                            bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper());
                        }
                        else if (list.PharmacyspecName == "Zoology")
                        {
                            bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper());
                            bpharmacylist.requiredFaculty = bpharmacyintake == 100 ? 3 : 2;
                        }

                        #endregion





                        if (list.PharmacyspecName == "Group1 (Pharmaceutics , Industrial Pharmacy)" || list.PharmacyspecName == "Pharmaceutics" || list.PharmacyspecName == "Industrial Pharmacy" || list.PharmacyspecName == "Pharmacy BioTechnology" || list.PharmacyspecName == "Pharmaceutical Technology")
                        {
                            group1Subcount = jntuh_registered_faculty.Count(f => f.PGSpecializationId == 120 && f.RegistrationNumber != principalRegno) +
                                                jntuh_registered_faculty.Count(f => f.PGSpecializationId == 115 && f.RegistrationNumber != principalRegno);

                            bpharmacylist.BPharmacySubGroup1Count = group1Subcount;
                            bpharmacylist.BPharmacySubGroupRequired = bpharmacyintake >= 100 ? 10 : 7;
                            bpharmacylist.PharmacySubGroup1 = "Group1 (Pharmaceutics , Industrial Pharmacy)";
                            if (bpharmacylist.BPharmacySubGroupRequired > group1Subcount)
                                PharmacyDeficiency = "Deficiency";
                        }

                        else if (list.PharmacyspecName == "Group2 (Pharmaceutical Chemistry,Pharmaceutical Analysis, PAQA, QA, QAPRA)" || list.PharmacyspecName == "Pharmaceutical Chemistry")
                        {
                            //jntuh_registered_faculty.Count(f => f.registered_faculty_specialization.Contains("Quality Assurance & Pharma Regulatory Affairs".ToUpper()));

                            group2Subcount = jntuh_registered_faculty.Count(f => f.PGSpecializationId == 117 && f.RegistrationNumber != principalRegno) +
                                                 jntuh_registered_faculty.Count(f => f.PGSpecializationId == 116 && f.RegistrationNumber != principalRegno) +
                                                 jntuh_registered_faculty.Count(f => f.PGSpecializationId == 123 && f.RegistrationNumber != principalRegno) +
                                                 jntuh_registered_faculty.Count(f => f.PGSpecializationId == 124 && f.RegistrationNumber != principalRegno);
                            bpharmacylist.BPharmacySubGroup1Count = group2Subcount;
                            bpharmacylist.BPharmacySubGroupRequired = bpharmacyintake >= 100 ? 10 : 7;
                            bpharmacylist.PharmacySubGroup1 = "Group2 (Pharmaceutical Chemistry,Pharmaceutical Analysis, PAQA, QA, QAPRA)";
                            if (bpharmacylist.BPharmacySubGroupRequired > group2Subcount)
                                PharmacyDeficiency = "Deficiency";


                        }


                        else if (list.PharmacyspecName == "Group3 (Pharmacology, Pharm-D, Pharm-DPB, Pharmacy Practice)" || list.PharmacyspecName == "Pharmacology" || list.PharmacyspecName == "Pharma D" || list.PharmacyspecName == "Pharma.D" || list.PharmacyspecName == "Pharm D" || list.PharmacyspecName == "Pharm.D")
                        {
                            group3Subcount = jntuh_registered_faculty.Count(f => f.PGSpecializationId == 122 && f.RegistrationNumber != principalRegno) +
                                                jntuh_registered_faculty.Count(f => f.PGSpecializationId == 18 && f.RegistrationNumber != principalRegno) +
                                                jntuh_registered_faculty.Count(f => f.PGSpecializationId == 19 && f.RegistrationNumber != principalRegno);

                            switch (collegeId)
                            {

                                case 204:
                                    bpharmacylist.BPharmacySubGroupRequired = 6;
                                    break;
                                case 27:
                                case 219:
                                case 52:
                                    bpharmacylist.BPharmacySubGroupRequired = 7;
                                    break;
                                case 389:
                                    bpharmacylist.BPharmacySubGroupRequired = 8;
                                    break;
                                case 127:
                                case 428:
                                case 90:
                                case 392:
                                case 159:
                                    bpharmacylist.BPharmacySubGroupRequired = 10;
                                    break;
                                default:
                                    bpharmacylist.BPharmacySubGroupRequired = bpharmacyintake >= 100 ? 12 : 9;
                                    break;

                            }


                            bpharmacylist.BPharmacySubGroup1Count = group3Subcount;
                            //bpharmacylist.BPharmacySubGroupRequired = bpharmacyintake >= 100 ? 12 : 9;
                            bpharmacylist.PharmacySubGroup1 = "Group3 (Pharmacology, Pharm-D, Pharm-DPB, Pharmacy Practice)";
                            if (bpharmacylist.BPharmacySubGroupRequired > group3Subcount)
                                PharmacyDeficiency = "Deficiency";
                        }

                        else if (list.PharmacyspecName == "Group4 (Pharmacognosy, Pharmaceutical Chemistry, Pharmaceutical Biotechonology, Phyto Pharmacy, Phyto medicine)" || list.PharmacyspecName == "Pharmacognosy")
                        {

                            int PharmacognosySp = jntuh_registered_faculty.Count(f => f.PGSpecializationId == 121 && f.RegistrationNumber != principalRegno);
                            int PharmaceuticalChemistrySp = jntuh_registered_faculty.Count(f => f.PGSpecializationId == 117 && f.RegistrationNumber != principalRegno);
                            int PAQASp = jntuh_registered_faculty.Count(f => f.PGSpecializationId == 116 && f.RegistrationNumber != principalRegno);
                            int QASp = jntuh_registered_faculty.Count(f => f.PGSpecializationId == 123 && f.RegistrationNumber != principalRegno);
                            int QAPRASp = jntuh_registered_faculty.Count(f => f.PGSpecializationId == 124 && f.RegistrationNumber != principalRegno);
                            int Grop2Required = bpharmacyintake >= 100 ? 10 : 7;
                            int Grop4Required = bpharmacyintake >= 100 ? 4 : 3;
                            int Total1 = Grop2Required - (PAQASp + QAPRASp + QASp);
                            int Total = (PAQASp + QAPRASp + PharmaceuticalChemistrySp + QASp) - Grop2Required;
                            if (Total > 0)
                                group4Subcount = PharmacognosySp + (PharmaceuticalChemistrySp - (Total1 < 0 ? 0 : Total1));
                            else if (Total <= 0)
                                group4Subcount = PharmacognosySp;
                            else if (PharmacognosySp == Grop2Required)
                                group4Subcount = PharmacognosySp;
                            bpharmacylist.BPharmacySubGroup1Count = group4Subcount;
                            bpharmacylist.BPharmacySubGroupRequired = Grop4Required;
                            bpharmacylist.PharmacySubGroup1 = "Group4 (Pharmacognosy, Pharmaceutical Chemistry, Pharmaceutical Biotechonology, Phyto Pharmacy, Phyto medicine)";
                            if (bpharmacylist.BPharmacySubGroupRequired > group4Subcount)
                                PharmacyDeficiency = "Deficiency";
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

                        intakedetailsList1.Add(bpharmacylist);
                    }
                    if (PharmacyDeficiency != "Deficiency")
                    {
                        PharmacyandPharmDMeet = "Yes";
                        foreach (var item in intakedetailsList1)
                        {
                            intakedetailsList.Add(item);
                        }

                        #region Pharmd Start
                        var pharmaD2 = collegeIntakeExisting.Where(i => i.specializationId == 18).ToList();
                        if (pharmaD2.Count > 0)
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
                                bpharmacylist.collegeRandomCode = randomcode;
                                bpharmacylist.collegeCode = jntuh_college.Where(c => c.id == collegeId).Select(c => c.collegeCode).FirstOrDefault();
                                bpharmacylist.collegeName = jntuh_college.Where(c => c.id == collegeId).Select(c => c.collegeName).FirstOrDefault();
                                bpharmacylist.shiftId = 1;
                                bpharmacylist.Degree = "Pharm.D";
                                bpharmacylist.Department = "Pharm.D";
                                bpharmacylist.PharmacyGroup1 = "Group1";
                                //bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper());
                                bpharmacylist.specializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper());
                                bpharmacylist.ugFaculty = ug = jntuh_registered_faculty.Count(f => "UG" == f.HighestDegree && f.Department == "Pharm.D");
                                bpharmacylist.pgFaculty = pg = jntuh_registered_faculty.Count(f => ("PG" == f.HighestDegree || "M.Phil" == f.HighestDegree) && f.Department == "Pharm.D");
                                bpharmacylist.phdFaculty = phd = jntuh_registered_faculty.Count(f => "Ph.D" == f.HighestDegree && f.Department == "Pharm.D");
                                bpharmacylist.totalFaculty = ug + pg + phd;
                                //bpharmacylist.requiredFaculty = BpharcyrequiredFaculty;
                                bpharmacylist.PharmacySpecilaizationList = pharmacyspeclist;
                                bpharmacylist.totalcollegefaculty = jntuh_registered_faculty.Count;
                                bpharmacylist.pharmadrequiredfaculty = Math.Ceiling(PharmDrequiredFaculty);
                                bpharmacylist.SortId = 2;
                                #region pharmadSpecializationcount
                                if (list.PharmacyspecName == "Group1 (Pharmacy Practice, Pharm D, Pharmacology)" || list.PharmacyspecName == "Pharm D")
                                {
                                    //bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper() ||
                                    //               f.registered_faculty_specialization == "PharmD".ToUpper() ||
                                    //              f.registered_faculty_specialization == "Pharm D".ToUpper() ||
                                    //              f.registered_faculty_specialization == "Pharma.D".ToUpper() ||
                                    //              f.registered_faculty_specialization == "Pharma D".ToUpper());

                                    // bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.PGSpecializationId == 18 || f.PGSpecializationId == 122 );




                                    bpharmacylist.PharmacyspecializationWiseFaculty = 10;

                                }
                                else if (list.PharmacyspecName == "Group1 (Pharmacy Practice, Pharm D, Pharmacology)" || list.PharmacyspecName == "Pharmacy Practice")
                                {
                                    bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper());
                                }
                                #endregion


                                if (list.PharmacyspecName == "Group1 (Pharmacy Practice, Pharm D, Pharmacology)" || list.PharmacyspecName == "Pharmacy Practice" || list.PharmacyspecName == "Pharm D" || list.PharmacyspecName == "Pharm.D")
                                {


                                    bpharmacylist.BPharmacySubGroup1Count = 5;
                                    bpharmacylist.BPharmacySubGroupRequired = 5;
                                    bpharmacylist.PharmacySubGroup1 = "Group1 (Pharmacy Practice, Pharm D, Pharmacology)";
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
                        #endregion Pharmd End

                        #region PharmDPB Strt
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
                                bpharmacylist.collegeRandomCode = randomcode;
                                bpharmacylist.shiftId = 1;
                                bpharmacylist.Degree = "Pharm.D PB";
                                bpharmacylist.Department = "Pharm.D PB";
                                bpharmacylist.PharmacyGroup1 = "Group1";
                                //bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper());
                                bpharmacylist.specializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper());
                                bpharmacylist.ugFaculty = ug = jntuh_registered_faculty.Count(f => "UG" == f.HighestDegree && f.Department == "Pharm.D PB");
                                bpharmacylist.pgFaculty = pg = jntuh_registered_faculty.Count(f => ("PG" == f.HighestDegree || "M.Phil" == f.HighestDegree) && f.Department == "Pharm.D PB");
                                bpharmacylist.phdFaculty = phd = jntuh_registered_faculty.Count(f => "Ph.D" == f.HighestDegree && f.Department == "Pharm.D PB");
                                bpharmacylist.totalFaculty = ug + pg + phd;
                                //bpharmacylist.requiredFaculty = BpharcyrequiredFaculty;
                                bpharmacylist.pharmadPBrequiredfaculty = Math.Ceiling(PharmDPBrequiredFaculty);
                                bpharmacylist.PharmacySpecilaizationList = pharmacyspeclist;
                                bpharmacylist.totalcollegefaculty = jntuh_registered_faculty.Count;
                                bpharmacylist.SortId = 3;
                                #region pharmadPbSpecializationcount
                                if (list.PharmacyspecName == "Group1 (Pharmacy Practice, Pharm D, Pharmacology))" || list.PharmacyspecName == "Pharm D")
                                {
                                    bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper() ||
                                                   f.registered_faculty_specialization == "PharmD".ToUpper() ||
                                                  f.registered_faculty_specialization == "Pharm D".ToUpper() ||
                                                  f.registered_faculty_specialization == "Pharma.D".ToUpper() ||
                                                  f.registered_faculty_specialization == "Pharma D".ToUpper());
                                }
                                else if (list.PharmacyspecName == "Group1 (Pharmacy Practice, Pharm D, Pharmacology)" || list.PharmacyspecName == "Pharmacy Practice")
                                {
                                    bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper());
                                }
                                #endregion


                                if (list.PharmacyspecName == "Group1 (Pharmacy Practice, Pharm D, Pharmacology)" || list.PharmacyspecName == "Pharmacy Practice" || list.PharmacyspecName == "Pharm D" || list.PharmacyspecName == "Pharm.D")
                                {

                                    bpharmacylist.BPharmacySubGroup1Count = 2;
                                    bpharmacylist.BPharmacySubGroupRequired = 2;
                                    bpharmacylist.PharmacySubGroup1 = "Group1 (Pharmacy Practice, Pharm D, Pharmacology)";
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
                        #endregion PharmDPB End

                    }
                    else if (PharmacyDeficiency == "Deficiency")
                    {
                        #region Pharmd Start
                        var pharmaD2 = collegeIntakeExisting.Where(i => i.specializationId == 18).ToList();
                        if (pharmaD2.Count > 0)
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
                                bpharmacylist.collegeRandomCode = randomcode;
                                bpharmacylist.collegeCode = jntuh_college.Where(c => c.id == collegeId).Select(c => c.collegeCode).FirstOrDefault();
                                bpharmacylist.collegeName = jntuh_college.Where(c => c.id == collegeId).Select(c => c.collegeName).FirstOrDefault();
                                bpharmacylist.shiftId = 1;
                                bpharmacylist.Degree = "Pharm.D";
                                bpharmacylist.Department = "Pharm.D";
                                bpharmacylist.PharmacyGroup1 = "Group1";
                                //bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper());
                                bpharmacylist.specializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper());
                                bpharmacylist.ugFaculty = ug = jntuh_registered_faculty.Count(f => "UG" == f.HighestDegree && f.Department == "Pharm.D");
                                bpharmacylist.pgFaculty = pg = jntuh_registered_faculty.Count(f => ("PG" == f.HighestDegree || "M.Phil" == f.HighestDegree) && f.Department == "Pharm.D");
                                bpharmacylist.phdFaculty = phd = jntuh_registered_faculty.Count(f => "Ph.D" == f.HighestDegree && f.Department == "Pharm.D");
                                bpharmacylist.totalFaculty = ug + pg + phd;
                                //bpharmacylist.requiredFaculty = BpharcyrequiredFaculty;
                                bpharmacylist.PharmacySpecilaizationList = pharmacyspeclist;
                                bpharmacylist.totalcollegefaculty = jntuh_registered_faculty.Count;
                                bpharmacylist.pharmadrequiredfaculty = Math.Ceiling(PharmDrequiredFaculty);
                                bpharmacylist.SortId = 2;
                                #region pharmadSpecializationcount
                                if (list.PharmacyspecName == "Group1 (Pharmacy Practice, Pharm D, Pharmacology)" || list.PharmacyspecName == "Pharm D")
                                {

                                    bpharmacylist.PharmacyspecializationWiseFaculty = 0;

                                }
                                else if (list.PharmacyspecName == "Group1 (Pharmacy Practice, Pharm D, Pharmacology)" || list.PharmacyspecName == "Pharmacy Practice")
                                {
                                    bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper());
                                }
                                #endregion


                                if (list.PharmacyspecName == "Group1 (Pharmacy Practice, Pharm D, Pharmacology)" || list.PharmacyspecName == "Pharmacy Practice" || list.PharmacyspecName == "Pharm D" || list.PharmacyspecName == "Pharm.D")
                                {


                                    bpharmacylist.BPharmacySubGroup1Count = 0;
                                    bpharmacylist.BPharmacySubGroupRequired = 5;
                                    bpharmacylist.PharmacySubGroup1 = "Group1 (Pharmacy Practice, Pharm D, Pharmacology)";
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
                        #endregion Pharmd End

                        #region PharmDPB Strt
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
                                bpharmacylist.collegeRandomCode = randomcode;
                                bpharmacylist.shiftId = 1;
                                bpharmacylist.Degree = "Pharm.D PB";
                                bpharmacylist.Department = "Pharm.D PB";
                                bpharmacylist.PharmacyGroup1 = "Group1";
                                //bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper());
                                bpharmacylist.specializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper());
                                bpharmacylist.ugFaculty = ug = jntuh_registered_faculty.Count(f => "UG" == f.HighestDegree && f.Department == "Pharm.D PB");
                                bpharmacylist.pgFaculty = pg = jntuh_registered_faculty.Count(f => ("PG" == f.HighestDegree || "M.Phil" == f.HighestDegree) && f.Department == "Pharm.D PB");
                                bpharmacylist.phdFaculty = phd = jntuh_registered_faculty.Count(f => "Ph.D" == f.HighestDegree && f.Department == "Pharm.D PB");
                                bpharmacylist.totalFaculty = ug + pg + phd;
                                //bpharmacylist.requiredFaculty = BpharcyrequiredFaculty;
                                bpharmacylist.pharmadPBrequiredfaculty = Math.Ceiling(PharmDPBrequiredFaculty);
                                bpharmacylist.PharmacySpecilaizationList = pharmacyspeclist;
                                bpharmacylist.totalcollegefaculty = jntuh_registered_faculty.Count;
                                bpharmacylist.SortId = 3;
                                #region pharmadPbSpecializationcount
                                if (list.PharmacyspecName == "Group1 (Pharmacy Practice, Pharm D, Pharmacology))" || list.PharmacyspecName == "Pharm D")
                                {
                                    bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper() ||
                                                   f.registered_faculty_specialization == "PharmD".ToUpper() ||
                                                  f.registered_faculty_specialization == "Pharm D".ToUpper() ||
                                                  f.registered_faculty_specialization == "Pharma.D".ToUpper() ||
                                                  f.registered_faculty_specialization == "Pharma D".ToUpper());
                                }
                                else if (list.PharmacyspecName == "Group1 (Pharmacy Practice, Pharm D, Pharmacology)" || list.PharmacyspecName == "Pharmacy Practice")
                                {
                                    bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper());
                                }
                                #endregion


                                if (list.PharmacyspecName == "Group1 (Pharmacy Practice, Pharm D, Pharmacology)" || list.PharmacyspecName == "Pharmacy Practice" || list.PharmacyspecName == "Pharm D" || list.PharmacyspecName == "Pharm.D")
                                {

                                    bpharmacylist.BPharmacySubGroup1Count = 0;
                                    bpharmacylist.BPharmacySubGroupRequired = 2;
                                    bpharmacylist.PharmacySubGroup1 = "Group1 (Pharmacy Practice, Pharm D, Pharmacology)";
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
                        #endregion PharmDPB End

                    }
                }
                #endregion Pharmd And PharmDPB
                #region Pharmacy Only
                if (PharmacyDeficiency == "Deficiency" || pharmaD == 0)
                {
                    PharmacyandPharmDMeet = "No";
                    foreach (var list in pharmacyspeclist)
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
                        bpharmacylist.collegeRandomCode = randomcode;
                        bpharmacylist.shiftId = 1;
                        bpharmacylist.Degree = "B.Pharmacy";
                        bpharmacylist.Department = "Pharmacy";
                        bpharmacylist.PharmacyGroup1 = "Group1";

                        bpharmacylist.specializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper());
                        //bpharmacylist.ugFaculty = ug = jntuh_registered_faculty.Count(f => "UG" == f.HighestDegree && f.Department == "Pharmacy");
                        //bpharmacylist.pgFaculty = pg = jntuh_registered_faculty.Count(f => ("PG" == f.HighestDegree || "M.Phil" == f.HighestDegree) && f.Department == "Pharmacy");
                        //bpharmacylist.phdFaculty = phd = jntuh_registered_faculty.Count(f => "Ph.D" == f.HighestDegree && f.Department == "Pharmacy");
                        //bpharmacylist.totalFaculty = ug + pg + phd;
                        //bpharmacylist.requiredFaculty = BpharcyrequiredFaculty;
                        bpharmacylist.PharmacySpecilaizationList = pharmacyspeclist;
                        bpharmacylist.BphramacyrequiredFaculty = bpharmacyintake >= 100 ? 25 : 15;
                        bpharmacylist.SortId = 1;
                        #region bpharmacyspecializationcount

                        if (list.PharmacyspecName == "Pharmaceutics")
                        {
                            bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper());
                        }

                        else if (list.PharmacyspecName == "Industrial Pharmacy")
                        {
                            bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper());
                        }
                        else if (list.PharmacyspecName == "Pharmacy BioTechnology")
                        {
                            bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper() ||
                                            f.registered_faculty_specialization == "Bio-Technology".ToUpper());

                        }
                        else if (list.PharmacyspecName == "Pharmaceutical Technology")
                        {
                            bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "Pharmaceutical Technology (2011-12)".ToUpper() ||
                                f.registered_faculty_specialization == "Pharmaceutical Technology".ToUpper());
                            bpharmacylist.requiredFaculty = 3;
                        }
                        else if (list.PharmacyspecName == "Pharmaceutical Chemistry")
                        {
                            bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper());
                            bpharmacylist.requiredFaculty = 2;
                        }
                        else if (list.PharmacyspecName == "Pharmacy Analysis")
                        {
                            bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper());
                        }

                        else if (list.PharmacyspecName == "PAQA")
                        {
                            bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper() ||
                                         f.registered_faculty_specialization == "PA & QA".ToUpper() ||
                                //f.registered_faculty_specialization == "Quality Assurance".ToUpper() ||
                                //f.registered_faculty_specialization == "QualityAssurance".ToUpper() ||
                                         f.registered_faculty_specialization == "QAPRA".ToUpper() ||
                                         f.registered_faculty_specialization == "Pharmaceutical Analysis & Quality Assurance".ToUpper() ||
                                         f.registered_faculty_specialization == "Quality Assurance & Pharma Regulatory Affairs".ToUpper());
                            bpharmacylist.requiredFaculty = 1;
                        }
                        else if (list.PharmacyspecName == "Pharmacology")
                        {
                            bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper());
                        }

                        else if (list.PharmacyspecName == "Pharma D")
                        {
                            bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper() ||
                                           f.registered_faculty_specialization == "PharmD".ToUpper() ||
                                          f.registered_faculty_specialization == "Pharm D".ToUpper() ||
                                          f.registered_faculty_specialization == "Pharm.D".ToUpper());
                            bpharmacylist.requiredFaculty = 2;
                        }
                        else if (list.PharmacyspecName == "Pharmacognosy")
                        {
                            bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper() ||
                                          f.registered_faculty_specialization == "Pharmacognosy & Phytochemistry".ToUpper() ||
                                          f.registered_faculty_specialization == "Pharmacognosy&Phytochemistry".ToUpper());
                            bpharmacylist.requiredFaculty = 2;
                        }

                        else if (list.PharmacyspecName == "English")
                        {
                            bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper());
                        }
                        else if (list.PharmacyspecName == "Mathematics")
                        {
                            bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper());
                        }
                        else if (list.PharmacyspecName == "Computers")
                        {
                            bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper());
                        }
                        else if (list.PharmacyspecName == "Computer Science")
                        {
                            bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper());
                        }
                        else if (list.PharmacyspecName == "Zoology")
                        {
                            bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper());
                            bpharmacylist.requiredFaculty = bpharmacyintake == 100 ? 3 : 2;
                        }
                        #endregion



                        BpharcyrequiredFaculty = bpharmacyintake >= 100 ? 25 : 15;

                        if (list.PharmacyspecName == "Group1 (Pharmaceutics , Industrial Pharmacy)" || list.PharmacyspecName == "Pharmaceutics" || list.PharmacyspecName == "Industrial Pharmacy" || list.PharmacyspecName == "Pharmacy BioTechnology" || list.PharmacyspecName == "Pharmaceutical Technology")
                        {
                            //group1Subcount = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "Pharmaceutics".ToUpper());
                            group1Subcount = jntuh_registered_faculty.Count(f => f.PGSpecializationId == 120 && f.RegistrationNumber != principalRegno) +
                                                jntuh_registered_faculty.Count(f => f.PGSpecializationId == 115 && f.RegistrationNumber != principalRegno);
                            // jntuh_registered_faculty.Count(f => f.PGSpecializationId == 119)+
                            //jntuh_registered_faculty.Count(f => f.PGSpecializationId == 115);

                            //jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "Industrial Pharmacy".ToUpper()) +
                            //jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "Pharmacy BioTechnology".ToUpper()) +
                            //jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "Bio-Technology".ToUpper()) +
                            //jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "Pharmaceutical Technology (2011-12)".ToUpper()) +
                            //jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "Pharmaceutical Technology".ToUpper());
                            //int TotalIntake=0;
                            //if (bpharmacyintake == 0)
                            //    TotalIntake = intakedetails.approvedIntake1;
                            //else if (bpharmacyintake >= 0)
                            //    TotalIntake = bpharmacyintake;
                            bpharmacylist.BPharmacySubGroup1Count = group1Subcount;
                            bpharmacylist.BPharmacySubGroupRequired = bpharmacyintake >= 100 ? 8 : 5;
                            bpharmacylist.PharmacySubGroup1 = "Group1 (Pharmaceutics , Industrial Pharmacy)";
                            if (bpharmacylist.BPharmacySubGroupRequired > group1Subcount)
                                DeficiencyInPharmacy = "Deficiency";
                        }

                        else if (list.PharmacyspecName == "Group2 (Pharmaceutical Chemistry,Pharmaceutical Analysis, PAQA, QA, QAPRA)" || list.PharmacyspecName == "Pharmaceutical Chemistry")
                        {
                            //group2Subcount = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "Pharmaceutical Chemistry".ToUpper()) +
                            //                jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "Pharmacy Analysis".ToUpper()) +
                            //                jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "PAQA".ToUpper()) +
                            //                jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "PA & QA".ToUpper()) +
                            //                jntuh_registered_faculty.Count(f => f.registered_faculty_specialization.Contains("QAPRA")) +
                            //                jntuh_registered_faculty.Count(f => f.registered_faculty_specialization.Contains("Pharmaceutical Analysis & Quality Assurance".ToUpper())) +
                            //                jntuh_registered_faculty.Count(f => f.registered_faculty_specialization.Contains("Quality Assurance & Pharma Regulatory Affairs".ToUpper()));

                            group2Subcount = jntuh_registered_faculty.Count(f => f.PGSpecializationId == 117 && f.RegistrationNumber != principalRegno) +
                                                 jntuh_registered_faculty.Count(f => f.PGSpecializationId == 116 && f.RegistrationNumber != principalRegno) +
                                                 jntuh_registered_faculty.Count(f => f.PGSpecializationId == 123 && f.RegistrationNumber != principalRegno) +
                                                 jntuh_registered_faculty.Count(f => f.PGSpecializationId == 124 && f.RegistrationNumber != principalRegno);
                            bpharmacylist.BPharmacySubGroup1Count = group2Subcount;
                            bpharmacylist.BPharmacySubGroupRequired = bpharmacyintake >= 100 ? 7 : 4;
                            bpharmacylist.PharmacySubGroup1 = "Group2 (Pharmaceutical Chemistry,Pharmaceutical Analysis, PAQA, QA, QAPRA)";
                            if (bpharmacylist.BPharmacySubGroupRequired > group2Subcount)
                                DeficiencyInPharmacy = "Deficiency";


                        }
                        //else if (list.PharmacyspecName == "Group3 (Pharmacy Analysis, PAQA)" || list.PharmacyspecName == "Pharmacy Analysis" || list.PharmacyspecName == "PAQA" || list.PharmacyspecName == "PA & QA" || list.PharmacyspecName.Contains("QA"))
                        //{
                        //    var y = jntuh_registered_faculty.Where(f => f.registered_faculty_specialization == "Pharmacy Analysis".ToUpper()).ToList();
                        //    var g = jntuh_registered_faculty.Where(f => f.registered_faculty_specialization.Contains("QA")).ToList();
                        //    var g1 = jntuh_registered_faculty.Where(f => f.registered_faculty_specialization == "PAQA".ToUpper()).ToList();
                        //    var g2 = jntuh_registered_faculty.Where(f => f.registered_faculty_specialization == "PA & QA".ToUpper()).ToList();

                        //    group3Subcount = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "Pharmacy Analysis".ToUpper()) +
                        //        jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "PAQA".ToUpper()) +
                        //        jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "PA & QA".ToUpper()) +
                        //        jntuh_registered_faculty.Count(f => f.registered_faculty_specialization.Contains("QAPRA")) +
                        //        jntuh_registered_faculty.Count(f => f.registered_faculty_specialization.Contains("Pharmaceutical Analysis & Quality Assurance".ToUpper())) +
                        //        jntuh_registered_faculty.Count(f => f.registered_faculty_specialization.Contains("Quality Assurance & Pharma Regulatory Affairs".ToUpper()));
                        //    bpharmacylist.BPharmacySubGroup1Count = group3Subcount;
                        //    bpharmacylist.BPharmacySubGroupRequired = 1;
                        //    bpharmacylist.PharmacySubGroup1 = "Group3 (Pharmacy Analysis, PAQA)";
                        //}

                        else if (list.PharmacyspecName == "Group3 (Pharmacology, Pharm-D, Pharm-DPB, Pharmacy Practice)" || list.PharmacyspecName == "Pharmacology" || list.PharmacyspecName == "Pharma D" || list.PharmacyspecName == "Pharma.D" || list.PharmacyspecName == "Pharm D" || list.PharmacyspecName == "Pharm.D")
                        {
                            // group3Subcount = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "Pharmacology".ToUpper());
                            group3Subcount = jntuh_registered_faculty.Count(f => f.PGSpecializationId == 122 && f.RegistrationNumber != principalRegno) +
                                                jntuh_registered_faculty.Count(f => f.PGSpecializationId == 18 && f.RegistrationNumber != principalRegno) +
                                                jntuh_registered_faculty.Count(f => f.PGSpecializationId == 19 && f.RegistrationNumber != principalRegno);
                            // jntuh_registered_faculty.Count(f => f.PGSpecializationId == 114);

                            bpharmacylist.BPharmacySubGroup1Count = group3Subcount;
                            bpharmacylist.BPharmacySubGroupRequired = bpharmacyintake >= 100 ? 6 : 4;
                            bpharmacylist.PharmacySubGroup1 = "Group3 (Pharmacology, Pharm-D, Pharm-DPB, Pharmacy Practice)";
                            if (bpharmacylist.BPharmacySubGroupRequired > group3Subcount)
                                DeficiencyInPharmacy = "Deficiency";
                        }

                        else if (list.PharmacyspecName == "Group4 (Pharmacognosy, Pharmaceutical Chemistry, Pharmaceutical Biotechonology, Phyto Pharmacy, Phyto medicine)" || list.PharmacyspecName == "Pharmacognosy")
                        {
                            //group4Subcount = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "Pharmacognosy".ToUpper()) +
                            //    jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "Pharmacognosy&Phytochemistryc".ToUpper()) +
                            //    jntuh_registered_faculty.Count(f => f.registered_faculty_specialization.Contains("PHARMACOGNOSY & PHYTOCHEMISTRY".ToUpper()));

                            //group4Subcount = jntuh_registered_faculty.Count(f => f.PGSpecializationId == 121 && f.RegistrationNumber!=principalRegno);

                            int PharmacognosySp = jntuh_registered_faculty.Count(f => f.PGSpecializationId == 121 && f.RegistrationNumber != principalRegno);
                            int PharmaceuticalChemistrySp = jntuh_registered_faculty.Count(f => f.PGSpecializationId == 117 && f.RegistrationNumber != principalRegno);
                            int PAQASp = jntuh_registered_faculty.Count(f => f.PGSpecializationId == 116 && f.RegistrationNumber != principalRegno);
                            int QASp = jntuh_registered_faculty.Count(f => f.PGSpecializationId == 123 && f.RegistrationNumber != principalRegno);
                            int QAPRASp = jntuh_registered_faculty.Count(f => f.PGSpecializationId == 124 && f.RegistrationNumber != principalRegno);
                            int Group2Required = bpharmacyintake >= 100 ? 7 : 4;
                            int Group4Required = bpharmacyintake >= 100 ? 4 : 2;
                            int Total1 = Group2Required - (PAQASp + QAPRASp + QASp);
                            int Total = (PAQASp + QAPRASp + PharmaceuticalChemistrySp + QASp) - Group2Required;
                            if (Total > 0)
                                group4Subcount = PharmacognosySp + (PharmaceuticalChemistrySp - (Total1 < 0 ? 0 : Total1));
                            else if (Total <= 0)
                                group4Subcount = PharmacognosySp;
                            else if (PharmacognosySp == Group2Required)
                                group4Subcount = PharmacognosySp;
                            bpharmacylist.BPharmacySubGroup1Count = group4Subcount;
                            bpharmacylist.BPharmacySubGroupRequired = Group4Required;
                            bpharmacylist.PharmacySubGroup1 = "Group4 (Pharmacognosy, Pharmaceutical Chemistry, Pharmaceutical Biotechonology, Phyto Pharmacy, Phyto medicine)";
                            if (bpharmacylist.BPharmacySubGroupRequired > group4Subcount)
                                DeficiencyInPharmacy = "Deficiency";
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
                #endregion  Pharmacy Only

                if (BpharcyrequiredFaculty > 0)
                {

                    if (bpharmacyintake >= 100)
                    {
                        BpharcyrequiredFaculty = Math.Round(BpharcyrequiredFaculty) - 0;
                        ViewBag.BpharmacyrequiredFaculty = BpharcyrequiredFaculty;
                        BpharmacyrequiredFaculty = BpharcyrequiredFaculty;
                    }
                    else
                    {
                        BpharcyrequiredFaculty = Math.Round(BpharcyrequiredFaculty) - 0;
                        ViewBag.BpharmacyrequiredFaculty = BpharcyrequiredFaculty;
                        BpharmacyrequiredFaculty = BpharcyrequiredFaculty;
                    }
                    intakedetailsList.Where(i => i.PharmacyGroup1 != "Group1" && i.Specialization == "B.Pharmacy").ToList().ForEach(c => c.requiredFaculty = BpharcyrequiredFaculty);


                    if (jntuh_registered_faculty.Count >= BpharcyrequiredFaculty)
                    {
                        if (group1Subcount >= (bpharmacyintake >= 100 ? 8 : 5) && group2Subcount >= (bpharmacyintake >= 100 ? 7 : 4) && group3Subcount >= (bpharmacyintake >= 100 ? 6 : 4) && group4Subcount >= (bpharmacyintake >= 100 ? 4 : 2))//&& group5Subcount >= 2 && group6Subcount >= (bpharmacyintake == 100 ? 3 : 2)
                        {
                            subgroupconditionsmet = conditionbpharm = "No";
                        }
                        else
                        {
                            subgroupconditionsmet = conditionbpharm = "Yes";
                        }
                    }
                    else
                    {
                        subgroupconditionsmet = conditionbpharm = "Yes";
                    }

                    ViewBag.BpharmcyCondition = conditionbpharm;
                    bpharmacycondition = conditionbpharm;
                    intakedetailsList.Where(i => i.PharmacyGroup1 == "Group1" && i.Specialization == "B.Pharmacy").ToList().ForEach(c => c.BPharmacySubGroupMet = subgroupconditionsmet);


                }

                if (PharmDrequiredFaculty > 0)
                {
                    if (jntuh_registered_faculty.Count >= PharmDrequiredFaculty)
                    {
                        if (pharmadgroup1Subcount >= pharmadTotalintake / 30)
                        {
                            subgroupconditionsmet = conditionpharmd = "No";
                        }
                        else
                        {
                            subgroupconditionsmet = conditionpharmd = "Yes";
                        }
                    }
                    else
                    {
                        subgroupconditionsmet = conditionpharmd = "Yes";
                    }

                    ViewBag.PharmaDCondition = conditionpharmd;
                    pharmdcondition = conditionpharmd;
                    if (conditionbpharm == "No")
                    {
                        intakedetailsList.Where(i => i.PharmacyGroup1 == "Group1" && i.Specialization == "Pharm.D").ToList().ForEach(c => c.BPharmacySubGroupMet = subgroupconditionsmet);
                    }
                    else
                    {
                        intakedetailsList.Where(i => i.PharmacyGroup1 == "Group1" && i.Specialization == "Pharm.D").ToList().ForEach(c => c.BPharmacySubGroupMet = "Yes");
                    }


                }

                ViewBag.PharmDPBrequiredFaculty = PharmDPBrequiredFaculty;
                pharmadpbrequiredfaculty = PharmDPBrequiredFaculty;
                if (PharmDPBrequiredFaculty > 0)
                {
                    if (jntuh_registered_faculty.Count >= PharmDPBrequiredFaculty)
                    {
                        if (pharmadPBgroup1Subcount >= pharmadPBTotalintake / 10)
                        {
                            subgroupconditionsmet = conditionphardpb = "No";
                        }
                        else
                        {
                            subgroupconditionsmet = conditionphardpb = "Yes";
                        }
                    }
                    else
                    {
                        subgroupconditionsmet = conditionphardpb = "Yes";
                    }

                    ViewBag.PharmaDPBCondition = conditionphardpb;
                    pharmadpbcondition = conditionphardpb;
                    if (conditionbpharm == "No" && conditionpharmd == "No")
                    {
                        intakedetailsList.Where(i => i.PharmacyGroup1 == "Group1" && i.Specialization == "Pharm.D PB").ToList().ForEach(c => c.BPharmacySubGroupMet = subgroupconditionsmet);
                    }
                    else
                    {
                        intakedetailsList.Where(i => i.PharmacyGroup1 == "Group1" && i.Specialization == "Pharm.D PB").ToList().ForEach(c => c.BPharmacySubGroupMet = "Yes");
                    }

                }



                #endregion

                intakedetailsList = intakedetailsList.OrderBy(ei => ei.Department).ThenBy(ei => ei.degreeDisplayOrder).ThenBy(ei => ei.Specialization).ThenBy(ei => ei.shiftId).ToList();

                #region Faculty Appeal Deficiency Status
                //var jntuh_appeal_faculty = db.jntuh_appeal_faculty_registered.AsNoTracking().ToList();
                var jntuh_deparment = db.jntuh_department.AsNoTracking().ToList();
                foreach (var item in intakedetailsList.Where(i => i.Degree == "B.Pharmacy").ToList())
                {
                    var deparment = jntuh_deparment.FirstOrDefault(i => i.id == item.DepartmentID);
                    if (deparment != null)
                    {
                        var facultydefcount = 0;//(int)Math.Ceiling(item.requiredFaculty) - item.totalFaculty

                        if (jntuh_registered_faculty.Count >= BpharcyrequiredFaculty && (item.Degree == "B.Pharmacy"))
                        {
                            if (pharmaD > 0)
                            {
                                if (group1Subcount >= (bpharmacyintake >= 100 ? 10 : 7) && group2Subcount >= (bpharmacyintake >= 100 ? 10 : 7) && group3Subcount >= (bpharmacyintake >= 100 ? 12 : 9) && group4Subcount >= (bpharmacyintake >= 100 ? 4 : 3))//&& group5Subcount >= 2 && group6Subcount >= (bpharmacyintake == 100 ? 3 : 2)
                                {
                                    Allgroupscount = 0;
                                }
                                else
                                {
                                    //var bpharmacyIntake = (bpharmacyintake >= 100 ? 3 : 2);
                                    if (group1Subcount < (bpharmacyintake >= 100 ? 10 : 7))
                                    {
                                        var count = (bpharmacyintake >= 100 ? 10 : 7) - group1Subcount;
                                        Allgroupscount = Allgroupscount + count;
                                    }
                                    if (group2Subcount < (bpharmacyintake >= 100 ? 10 : 7))
                                    {
                                        var count = (bpharmacyintake >= 100 ? 10 : 7) - group2Subcount;
                                        Allgroupscount = Allgroupscount + count;
                                    }
                                    if (group3Subcount < (bpharmacyintake >= 100 ? 12 : 9))
                                    {
                                        var count = (bpharmacyintake >= 100 ? 12 : 9) - group3Subcount;
                                        Allgroupscount = Allgroupscount + count;
                                    }
                                    if (group4Subcount < (bpharmacyintake >= 100 ? 4 : 3))
                                    {
                                        var count = (bpharmacyintake >= 100 ? 4 : 3) - group4Subcount;
                                        count = count == 1 ? 0 : count;
                                        Allgroupscount = Allgroupscount + count;
                                    }
                                }
                            }
                            else
                            {
                                if (pharmaD > 0)
                                    if (group1Subcount >= (bpharmacyintake >= 100 ? 8 : 5) && group2Subcount >= (bpharmacyintake >= 100 ? 7 : 4) && group3Subcount >= (bpharmacyintake >= 100 ? 6 : 4) && group4Subcount >= (bpharmacyintake >= 100 ? 4 : 2))//&& group5Subcount >= 2 && group6Subcount >= (bpharmacyintake == 100 ? 3 : 2)
                                    {
                                        Allgroupscount = 0;
                                    }
                                    else
                                    {
                                        //var bpharmacyIntake = (bpharmacyintake >= 100 ? 3 : 2);
                                        if (group1Subcount < (bpharmacyintake >= 100 ? 8 : 5))
                                        {
                                            var count = (bpharmacyintake >= 100 ? 8 : 5) - group1Subcount;
                                            Allgroupscount = Allgroupscount + count;
                                        }
                                        if (group2Subcount < (bpharmacyintake >= 100 ? 7 : 4))
                                        {
                                            var count = (bpharmacyintake >= 100 ? 7 : 4) - group2Subcount;
                                            Allgroupscount = Allgroupscount + count;
                                        }
                                        if (group3Subcount < (bpharmacyintake >= 100 ? 6 : 4))
                                        {
                                            var count = (bpharmacyintake >= 100 ? 6 : 4) - group3Subcount;
                                            Allgroupscount = Allgroupscount + count;
                                        }
                                        if (group4Subcount < (bpharmacyintake >= 100 ? 4 : 2))
                                        {
                                            var count = (bpharmacyintake >= 100 ? 4 : 2) - group4Subcount;
                                            count = count == 1 ? 0 : count;
                                            Allgroupscount = Allgroupscount + count;
                                        }
                                    }
                            }

                            facultydefcount = Allgroupscount;
                        }

                        else if (jntuh_registered_faculty.Count < BpharcyrequiredFaculty && (item.Degree == "B.Pharmacy"))
                        {
                            if (pharmaD > 0)
                            {
                                if (group1Subcount >= (bpharmacyintake >= 100 ? 10 : 7) && group2Subcount >= (bpharmacyintake >= 100 ? 10 : 7) && group3Subcount >= (bpharmacyintake >= 100 ? 12 : 9) && group4Subcount >= (bpharmacyintake >= 100 ? 4 : 3))//&& group5Subcount >= 2 && group6Subcount >= (bpharmacyintake == 100 ? 3 : 2)
                                {
                                    Allgroupscount = 0;
                                }
                                else
                                {
                                    //var bpharmacyIntake = (bpharmacyintake >= 100 ? 3 : 2);
                                    if (group1Subcount < (bpharmacyintake >= 100 ? 10 : 7))
                                    {
                                        var count = (bpharmacyintake >= 100 ? 10 : 7) - group1Subcount;
                                        Allgroupscount = Allgroupscount + count;
                                    }
                                    if (group2Subcount < (bpharmacyintake >= 100 ? 10 : 7))
                                    {
                                        var count = (bpharmacyintake >= 100 ? 10 : 7) - group2Subcount;
                                        Allgroupscount = Allgroupscount + count;
                                    }
                                    if (group3Subcount < (bpharmacyintake >= 100 ? 12 : 9))
                                    {
                                        var count = (bpharmacyintake >= 100 ? 12 : 9) - group3Subcount;
                                        Allgroupscount = Allgroupscount + count;
                                    }
                                    if (group4Subcount < (bpharmacyintake >= 100 ? 4 : 3))
                                    {
                                        var count = (bpharmacyintake >= 100 ? 4 : 2) - group4Subcount;
                                        count = count == 1 ? 0 : count;
                                        Allgroupscount = Allgroupscount + count;
                                    }
                                }
                            }
                            else
                            {
                                if (group1Subcount >= (bpharmacyintake >= 100 ? 8 : 5) && group2Subcount >= (bpharmacyintake >= 100 ? 7 : 4) && group3Subcount >= (bpharmacyintake >= 100 ? 6 : 4) && group4Subcount >= (bpharmacyintake >= 100 ? 4 : 2))//&& group5Subcount >= 2 && group6Subcount >= (bpharmacyintake == 100 ? 3 : 2)
                                {
                                    Allgroupscount = 0;
                                }
                                else
                                {
                                    //var bpharmacyIntake = (bpharmacyintake >= 100 ? 3 : 2);
                                    if (group1Subcount < (bpharmacyintake >= 100 ? 8 : 5))
                                    {
                                        var count = (bpharmacyintake >= 100 ? 8 : 5) - group1Subcount;
                                        Allgroupscount = Allgroupscount + count;
                                    }
                                    if (group2Subcount < (bpharmacyintake >= 100 ? 7 : 4))
                                    {
                                        var count = (bpharmacyintake >= 100 ? 7 : 4) - group2Subcount;
                                        Allgroupscount = Allgroupscount + count;
                                    }
                                    if (group3Subcount < (bpharmacyintake >= 100 ? 6 : 4))
                                    {
                                        var count = (bpharmacyintake >= 100 ? 6 : 4) - group3Subcount;
                                        Allgroupscount = Allgroupscount + count;
                                    }
                                    if (group4Subcount < (bpharmacyintake >= 100 ? 4 : 2))
                                    {
                                        var count = (bpharmacyintake >= 100 ? 4 : 2) - group4Subcount;
                                        count = count == 1 ? 0 : count;
                                        Allgroupscount = Allgroupscount + count;
                                    }
                                }
                            }


                            var lessfaculty = BpharcyrequiredFaculty - jntuh_registered_faculty.Count;

                            if (lessfaculty > Allgroupscount)
                            {
                                facultydefcount = Allgroupscount;//(int)lessfaculty + 
                            }
                            else if (Allgroupscount > lessfaculty)
                            {
                                facultydefcount = Allgroupscount;//+ (int)lessfaculty
                            }
                        }

                        if (item.Degree == "B.Pharmacy")
                        {
                            if (Allgroupscount > 0)
                            {
                                //item.deficiency = true; 
                            }
                            ViewBag.BpharmacyRequired = facultydefcount;
                        }

                        //if (item.PharmacyspecializationWiseFaculty < 1 && item.Department == "M.Pharmacy" && item.requiredFaculty > 0)
                        //{
                        //    facultydefcount = (int)Math.Ceiling(item.requiredFaculty) + 1;
                        //}
                        //if (item.PharmacyspecializationWiseFaculty >= 1 && item.Department == "M.Pharmacy" && item.requiredFaculty > 0)
                        //{
                        //    facultydefcount = (int)Math.Ceiling(item.requiredFaculty);
                        //}
                        //if (item.Department == "Pharm.D" || item.Department == "Pharm.D PB")
                        //{
                        //    facultydefcount = item.BPharmacySubGroupRequired - item.BPharmacySubGroup1Count;
                        //}

                    }
                }


                #endregion
            }
            return intakedetailsList;
        }

        #endregion

        //MBA Colleges Affiliation PDF

        public ActionResult AffiliationLetterForMBAandMCA(int collegeId, string type)
        {
            List<int> collegeIds = db.jntuh_college.Where(c => c.collegeCode != "WL").Select(c => c.id).ToList();

            string code = db.jntuh_college.Where(c => c.id == collegeId && c.isActive == true).Select(c => c.collegeCode).FirstOrDefault().ToUpper();

            string path = SaveCollegeDefficiencyLetterForMBAandMCAPdf(code, type);
            path = path.Replace("/", "\\");
            if (!string.IsNullOrEmpty(path))
            {
                return File(path, "application/pdf", path.Substring(path.LastIndexOf('\\') + 1));
            }
            return View();
        }

        private string SaveCollegeDefficiencyLetterForMBAandMCAPdf(string collegeCode, string type)
        {
            string fullPath = string.Empty;

            int Collegeid = db.jntuh_college.Where(C => C.collegeCode == collegeCode && C.isActive == true).Select(C => C.id).FirstOrDefault();
            string ECollegeid = UAAAS.Models.Utilities.EncryptString(Collegeid.ToString(), System.Web.Configuration.WebConfigurationManager.AppSettings["CryptoKey"]);


            //Set page size as A4
            var pdfDoc = new Document(PageSize.A4, 40, 40, 40, 40);

            // Create a new PdfWrite object, writing the output to a MemoryStream
            //open pdf
            var output = new MemoryStream();
            var writer = PdfWriter.GetInstance(pdfDoc, output);

            int InspectionPhaseId = db.jntuh_inspection_phase.Where(p => p.isActive == true).Select(p => p.id).SingleOrDefault();

            string path = Server.MapPath("~/Content/PDFReports/AffiliationProceedings/");

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            fullPath = path + collegeCode + "-" + ECollegeid + "-AffiliationProceedings.pdf";
            PdfWriter.GetInstance(pdfDoc, new FileStream(fullPath, FileMode.Create));

            //Open PDF Document to write data
            pdfDoc.Open();

            //Assign Html content in a string to write in PDF
            string contents = string.Empty;

            StreamReader sr;

            //Read file from server path
            sr = System.IO.File.OpenText(Server.MapPath("~/Content/AffiliationProceedingsMBA.html"));

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

            string collegeName = db.jntuh_college.Where(c => c.collegeCode == collegeCode).Select(c => c.collegeName).FirstOrDefault();

            if (address != null)
            {
                scheduleCollegeAddress = collegeName + ",<br />" + address.address;
                scheduleCollegeAddress = scheduleCollegeAddress + ", " + address.townOrCity;
                scheduleCollegeAddress = scheduleCollegeAddress + ", " + address.mandal;
                scheduleCollegeAddress = scheduleCollegeAddress + ",<br />" + district;
                scheduleCollegeAddress = scheduleCollegeAddress + " - " + address.pincode;
            }

            if (address != null)
            {
                scheduleCollegeAddress1 = collegeName + ", " + address.address;
                scheduleCollegeAddress1 = scheduleCollegeAddress1 + ", " + address.townOrCity;
                scheduleCollegeAddress1 = scheduleCollegeAddress1 + ", " + address.mandal;
                scheduleCollegeAddress1 = scheduleCollegeAddress1 + ", " + district;
                scheduleCollegeAddress1 = scheduleCollegeAddress1 + " - " + address.pincode;
            }

            string CollegeSocietyAddress = string.Empty;
            if (societyAddress != null)
            {
                CollegeSocietyAddress = collegeName + ", " + societyAddress.address;
                CollegeSocietyAddress = CollegeSocietyAddress + ", " + societyAddress.townOrCity;
                CollegeSocietyAddress = CollegeSocietyAddress + ", " + societyAddress.mandal;
                CollegeSocietyAddress = CollegeSocietyAddress + ", " + district;
                CollegeSocietyAddress = CollegeSocietyAddress + " - " + societyAddress.pincode;
            }
            scheduleCollegeAddress = scheduleCollegeAddress.ToUpper().Replace(",", ", ").Replace("(", " (").Replace(")", ") ");
            CollegeSocietyAddress = CollegeSocietyAddress.ToUpper().Replace(",", ", ").Replace("(", " (").Replace(")", ") ");
            contents = contents.Replace("##SOCIETY_ADDRESS##", CollegeSocietyAddress);
            contents = contents.Replace("##COLLEGE_ADDRESS##", scheduleCollegeAddress);
            contents = contents.Replace("##COLLEGE_ADDRESS1##", scheduleCollegeAddress1);
            contents = contents.Replace("##LETTER_DATE##", DateTime.Now.ToString("dd-MM-yyy"));

            //var dates = db.jntuh_dates.Where(d => d.collegecode == collegeCode).Select(d => d).FirstOrDefault();
            //contents = contents.Replace("##APPLICATION_DATE##", ((DateTime)dates.applicationDate).ToString("dd-MM-yyy"));
            //contents = contents.Replace("##DEFICIENCY_REPORT_DATE##", ((DateTime)dates.deficiencyLetterDate).ToString("dd-MM-yyy"));
            //contents = contents.Replace("##HEARING_DATE##", dates.hearingDate != null ? ((DateTime)dates.hearingDate).ToString("dd-MM-yyy") : "");

            var deficiencydates = db.jntuh_dates.Where(d => d.collegecode == collegeCode).Select(d => d).FirstOrDefault();
            var dates = db.jntuh_college_edit_status.Where(d => d.collegeId == collegeid && d.IsCollegeEditable == false).Select(d => d).FirstOrDefault();
            var Appeal = db.jntuh_appeal_college_edit_status.Where(d => d.collegeId == collegeid && d.IsCollegeEditable == false).Select(d => d).FirstOrDefault();
            if (Appeal != null)
            {
                contents = contents.Replace("##APPLICATION_DATE##", ((DateTime)Appeal.updatedOn).ToString("dd-MM-yyy"));
            }
            else
            {
                contents = contents.Replace("##APPLICATION_DATE##", "NIL");
            }

            if (dates != null)
            {
                contents = contents.Replace("##116Submitted_DATE##", ((DateTime)dates.updatedOn).ToString("dd-MM-yyy"));
            }
            else
            {
            }

            if (deficiencydates != null)
            {
                contents = contents.Replace("##DEFICIENCY_REPORT_DATE##", ((DateTime)deficiencydates.deficiencyLetterDate).ToString("dd-MM-yyy"));
            }
            else
            {
                contents = contents.Replace("##DEFICIENCY_REPORT_DATE##", "NIL");
            }

            string AppealSubmittedMessage = "Pursuant to the communication of deficiencies you have filed an appeal for reconsideration and the University reviewed the same or re inspection was conducted. Based on the above the University has accorded affiliation to the following courses.";
            string AppealNotSubmittedMessage = "Based on the above the University has accorded affiliation to the following courses.";
            contents = contents.Replace("##HEARING_DATE##", Appeal != null ? ((DateTime)Appeal.updatedOn).ToString("dd-MM-yyy") : "NIL");
            if (Appeal != null)
                contents = contents.Replace("##College_Message##", AppealSubmittedMessage);
            else
                contents = contents.Replace("##College_Message##", AppealNotSubmittedMessage);


            if (type == "All")
            {
                contents = contents.Replace("##COURSE_TABLE##", CollegeCoursesAll(collegeid));
            }
            else if (type == "AllClear")
            {
                string[] courses = CollegeCoursesAllClearForMBA(collegeid).Split('$');
                contents = contents.Replace("##COURSE_TABLE##", courses[0]);
                contents = contents.Replace("##ASSESSMNET_TABLE##", courses[1]);
                string defiencyRows = courses[2];

                if (defiencyRows != "0")
                {
                    contents = contents.Replace("##HIDE_START##", "<!--");
                    contents = contents.Replace("##HIDE_END##", "-->");
                }
                else
                {
                    contents = contents.Replace("##HIDE_START##", "<table border='1' style=''width:100%;text-align:center;padding-top:20px'><tr><td style='text-align:center'>NIL</td></tr></table>");
                }
            }

            var pharmacyids = new[]{24,27,30,33,34,44,47,52,55,65,78,83,95,97,104,105,107,110,114,117,118,127,135,136,139,146,150,159,169,202,204,213,219,234,239,253,
                262,263,267,275,283,284,290,295,297,298,301,303,314,317,320,348,353,362,376,377,379,384,392,395,410,427,428,436,442,445,448,454,457,458,6,
                45,54,58,90,120,206,237,302,313,318,319,389,60,66,252,255,315,370};
            if (pharmacyids.Contains(collegeid))
            {
                contents = contents.Replace("##END##", "<br/><b>R</b>- Required Faculty, <b>A</b>-Available Faculty <br/>* Any data discrepancies may be brought to the notice of the University within two days.<br/># The faculty requirement for Pharm.D / Pharm.D(PB) is included in B.Pharmacy course.");
            }
            else
            {
                contents = contents.Replace("##END##", "<br/> <b>R</b>- Required Faculty, <b>A</b>-Available Faculty, <b>R.Ph.D</b>-Required Ph.D Faculty, <b>A.Ph.D</b>-Available Ph.D Faculty. <br/>* Any data discrepancies may be brought to the notice of the University within two days.");
            }






            //Read string contents using stream reader and convert html to parsed conent
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

        public string CollegeCoursesAllClearForMBA(int collegeId)
        {
            string courses = string.Empty;
            string twentyfivepercent = string.Empty;
            string assessments = string.Empty;
            string PharmDandPB = string.Empty;
            List<CollegeIntakeExisting> collegeIntakeExisting = new List<CollegeIntakeExisting>();

            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);

            var degreeDetails = (from ie in db.jntuh_college_intake_existing
                                 join js in db.jntuh_specialization on ie.specializationId equals js.id
                                 join jd in db.jntuh_department on js.departmentId equals jd.id
                                 join deg in db.jntuh_degree on jd.degreeId equals deg.id
                                 join sh in db.jntuh_shift on ie.shiftId equals sh.id
                                 where (ie.collegeId == collegeId && ie.academicYearId == 9)
                                 select new
                                 {
                                     ie.proposedIntake,
                                     js.specializationName,
                                     deg.degree,
                                     sh.shiftName,
                                     deg.degreeDisplayOrder,
                                     js.id
                                 }).OrderBy(d => d.degreeDisplayOrder).ThenBy(d => d.specializationName).ThenBy(d => d.shiftName).ToList();


            int rowCount = 0; int assessmentCount = 0; int assessmentCount1 = 0;

            //foreach (var item in degreeDetails)
            //{
            //faculty
            //string[] fStatus = NoFacultyDeficiencyCourse(collegeId, item.id).Split('$');


            //string fFlag = fStatus[0];
            //string facultyShortage = fStatus[1];
            //string phd = fStatus[2];

            ////labs
            //string[] lStatus = NoLabDeficiencyCourse(collegeId, item.id).Split('$');

            //string lFlag = lStatus[0];
            //string labs = lStatus[1];
            var faculty = new List<CollegeFacultyLabs>();

            var integreatedIds = new[] { 9, 18, 39, 42, 75, 140, 180, 332, 364, 375 };

            var pharmacyids = new[]{24,27,30,33,34,44,47,52,55,65,78,83,95,97,104,105,107,110,114,117,118,127,135,136,139,146,150,159,169,202,204,213,219,234,239,253,
                262,263,267,275,283,284,290,295,297,298,301,303,314,317,320,348,353,362,376,377,379,384,392,395,410,427,428,436,442,445,448,454,457,458,6,
                45,54,58,90,120,206,237,302,313,318,319,389,60,66,252,255,315,370};

            if (pharmacyids.Contains(collegeId))
            {
                faculty = PharmacyDeficienciesInFaculty(collegeId);
            }
            else if (integreatedIds.Contains(collegeId))
            {
                var integratedpharmacyfaculty = PharmacyDeficienciesInFaculty(collegeId);
                var integratedbtechfaculty = DeficienciesInFaculty(collegeId);
                faculty = integratedbtechfaculty.Concat(integratedpharmacyfaculty).ToList();
            }
            else
            {
                faculty = DeficienciesInFaculty(collegeId);
            }
            List<CollegeFacultyLabs> labs = DeficienciesInLabsnew(collegeId);
            //DeficienciesInLabs(collegeId);

            var collegeFacultyLabs = labs.FirstOrDefault(i => i.Degree == "B.Pharmacy");
            if (collegeFacultyLabs != null)
            {
                var bphramcylabs = collegeFacultyLabs.LabsDeficiency;
                if (bphramcylabs != "NIL")
                {
                    foreach (var c in labs.Where(i => i.Degree == "M.Pharmacy").ToList())
                    {
                        c.LabsDeficiency = "YES";
                    }
                }
            }

            foreach (var l in labs)
            {
                if (l.Degree == "B.Tech" && l.LabsDeficiency != "NIL")
                {
                    labs.Where(i => i.Department == l.Department && i.Degree == "M.Tech" && i.LabsDeficiency == "NIL").ToList().ForEach(c => c.LabsDeficiency = "YES");
                }

            }



            List<CollegeFacultyLabs> clearedCourses = new List<CollegeFacultyLabs>();
            List<CollegeFacultyLabs> deficiencyCourses = new List<CollegeFacultyLabs>();
            List<string> deficiencyDepartments = new List<string>();
            List<string> deficiencyDepartments1 = new List<string>();

            #region Pharmacy Affliation Letter Start

            if (pharmacyids.Contains(collegeId))
            {

                courses += "<table border='1' cellpadding='3' cellspacing='0' width='100%' style='font-family: Times New Roman; font-size: 9px; font-weight: normal; background-color: #fff; margin: 0 auto;'>";
                courses += "<tr>";
                courses += "<th colspan='1' align='center'><b>S.No</b></th>";
                courses += "<th colspan='10'><b>Name of the Course</b></th>";
                courses += "<th colspan='2' align='center'><b>Intake</b></th>";
                courses += "</tr>";

                PharmDandPB += "<br/><table border='1' cellpadding='3' cellspacing='0' width='100%' style='font-family: Times New Roman; font-size: 9px; font-weight: normal; background-color: #fff; margin: 0 auto;'>";
                PharmDandPB += "<tr>";
                PharmDandPB += "<th colspan='1' align='center'><b>S.No</b></th>";
                PharmDandPB += "<th colspan='2' align='center'><b> Course</b></th>";
                PharmDandPB += "<th colspan='2' align='center'><b>Deficiency</b></th>";
                PharmDandPB += "</tr>";

                assessments += "<table border='1' cellpadding='3' cellspacing='0' width='100%' style='font-family: Times New Roman; font-size: 9px; font-weight: normal; background-color: #fff; margin: 0 auto;'>";
                assessments += "<tr>";
                assessments += "<th rowspan='2' colspan='1' align='center'><b>S.No</b></th>";
                assessments += "<th rowspan='2' colspan='5' align='center'><b>Course</b></th>";
                assessments += "<th rowspan='2' colspan='2' align='center'><b>Intake</b></th>";
                assessments += "<th colspan='2' align='center'><b>Faculty Shortage *</b></th>";
                assessments += "<th colspan='5' align='center'><b>Lab Shortage</b></th>";
                assessments += "</tr>";
                assessments += "<tr>";
                assessments += "<th colspan='1' align='center'><b>R</b></th>";
                assessments += "<th colspan='1' align='center'><b>A</b></th>";
                // assessments += "<th colspan='1' align='center'><b>R.Ph.D</b></th>";
                //assessments += "<th colspan='1' align='center'><b>A.Ph.D</b></th>";
                assessments += "<th colspan='5' align='center'><b>Name of the Lab(s)</b></th>";
                assessments += "</tr>";
                //Only Btech Affiliation Letters Filtering
                faculty = faculty.Where(e => e.Degree == "B.Pharmacy" || e.Degree == "Pharm.D" || e.Degree == "Pharm.D PB").Select(e => e).ToList();
                labs = labs.Where(e => e.Degree == "B.Pharmacy" || e.Degree == "Pharm.D" || e.Degree == "Pharm.D PB").Select(e => e).ToList();

                faculty = faculty.Select(e => e).ToList();
                labs = labs.Select(e => e).ToList();

                //.Where(a => a.f.Deficiency == "NO" && a.f.PhdDeficiency != "YES" && a.l.LabsDeficiency == "NIL")

                int[] FSpecIds = faculty.Select(F => F.SpecializationId).ToArray();
                int[] LSpecIds = labs.Select(F => F.SpecializationId).ToArray();

                foreach (int id in FSpecIds)
                {
                    if (!LSpecIds.Contains(id))
                    {
                        CollegeFacultyLabs lab = new CollegeFacultyLabs();
                        lab.SpecializationId = id;
                        lab.Deficiency = "NIL";
                        lab.LabsDeficiency = "NIL";
                        labs.Add(lab);
                    }

                }


                clearedCourses = faculty.Join(labs, f => f.SpecializationId, l => l.SpecializationId, (f, l) => new { f, l })
                        .Where(a => a.f.Deficiency == "NO" && a.f.PhdDeficiency != "YES" && a.l.LabsDeficiency == "NIL")
                        .Select(a => new CollegeFacultyLabs
                        {
                            Degree = a.f.Degree,
                            Department = a.f.Department,
                            SpecializationId = a.f.SpecializationId,
                            Specialization = a.f.Specialization,
                            TotalIntake = a.f.TotalIntake,
                            Required = a.f.Required,
                            Available = a.f.Available,
                            Deficiency = a.f.Deficiency,
                            Requiredphd = a.f.Requiredphd,
                            Availablephd = a.f.Availablephd,
                            PhdDeficiency = a.f.PhdDeficiency,
                            LabsDeficiency = a.l.LabsDeficiency
                        })
                        .OrderBy(a => a.DegreeDisplayOrder).ThenBy(a => a.Department).ThenBy(a => a.Specialization)
                        .ToList();

                deficiencyCourses = faculty.Join(labs, f => f.SpecializationId, l => l.SpecializationId, (f, l) => new { f, l })
                        .Where(a => (a.f.Deficiency == "YES" || a.f.PhdDeficiency == "YES") || a.l.LabsDeficiency != "NIL")
                        .Select(a => new CollegeFacultyLabs
                        {
                            Degree = a.f.Degree,
                            Department = a.f.Department,
                            SpecializationId = a.f.SpecializationId,
                            Specialization = a.f.Specialization,
                            TotalIntake = a.f.TotalIntake,
                            Required = a.f.Required,
                            Available = a.f.Available,
                            Requiredphd = a.f.Requiredphd,
                            Availablephd = a.f.Availablephd,
                            Deficiency = a.f.Deficiency,
                            PhdDeficiency = a.f.PhdDeficiency,
                            LabsDeficiency = a.l.LabsDeficiency
                        })
                        .OrderBy(a => a.DegreeDisplayOrder).ThenBy(a => a.Department).ThenBy(a => a.Specialization)
                        .ToList();



                foreach (var course in deficiencyCourses)
                {
                    if (!deficiencyDepartments.Contains(course.Department))
                    {
                        deficiencyDepartments.Add(course.Department);
                    }
                    if (course.Degree == "B.Pharmacy")
                    {
                        assessments += "<tr>";
                        assessments += "<td colspan='1' align='center'>" + (assessmentCount + 1) + "</td>";

                        course.Shift = "1";
                        if (course.Shift == "1")
                        {
                            assessments += "<td colspan='5' >" + course.Degree + " - " + course.Specialization + "</td>";
                        }
                        assessments += "<td colspan='2' align='center'>" + course.TotalIntake + "</td>";
                        int Count = deficiencyCourses.Count(D => D.Degree == "Pharm.D" || D.Degree == "Pharm.D PB");
                        if (Count > 0)
                        {
                            int reqFaculty = course.TotalIntake >= 100 ? 35 : 25;
                            assessments += "<td colspan='1' align='center'>" + reqFaculty + "</td>";
                        }
                        else
                        {
                            assessments += "<td colspan='1' align='center'>" + course.Required + "</td>";
                        }
                        if (course.Degree == "B.Pharmacy")
                        {
                            int Required = course.Required;
                            int Shortage = Required - BpharmcyAvilableFaculty;
                            assessments += "<td colspan='1' align='center'>" + BpharmcyAvilableFaculty + "</td>";
                        }

                        else
                            assessments += "<td colspan='1' align='center'>" + course.Available + "</td>";
                        //assessments += "<td colspan='1' align='center'>" + course.Requiredphd + "</td>";
                        // assessments += "<td colspan='1' align='center'>" + course.Availablephd + "</td>";
                        assessments += "<td colspan='5' align='center'>" + (course.LabsDeficiency.Equals("No Equipement Uploaded") ? course.Specialization + " Lab Deficiency" : course.LabsDeficiency) + "</td>";
                        assessments += "</tr>";

                        assessmentCount++;

                        var secondshift = db.jntuh_college_intake_existing.Where(e => e.collegeId == collegeId && e.specializationId == course.SpecializationId && e.shiftId == 2).Select(e => e).FirstOrDefault();

                        if (secondshift != null)
                        {
                            assessments += "<tr>";
                            assessments += "<td colspan='1' align='center'>" + (assessmentCount + 1) + "</td>";

                            course.Shift = "2";
                            if (course.Shift == "2")
                            {
                                assessments += "<td colspan='5' >" + course.Degree + " - " + course.Specialization + " - 2</td>";
                            }
                            assessments += "<td colspan='2' align='center'>" + course.TotalIntake + "</td>";
                            int Count1 = deficiencyCourses.Count(D => D.Degree == "Pharm.D" || D.Degree == "Pharm.D PB");
                            if (Count1 > 0)
                            {
                                int reqFaculty = course.TotalIntake >= 100 ? 35 : 25;
                                assessments += "<td colspan='1' align='center'>" + reqFaculty + "</td>";
                            }
                            else
                            {
                                assessments += "<td colspan='1' align='center'>" + course.Required + "</td>";
                            }

                            int Required = course.Required;
                            int Shortage = Required - BpharmcyAvilableFaculty;
                            assessments += "<td colspan='1' align='center'>" + BpharmcyAvilableFaculty + "</td>";
                            //assessments += "<td colspan='1' align='center'>" + course.Requiredphd + "</td>";
                            //assessments += "<td colspan='1' align='center'>" + course.Availablephd + "</td>";
                            assessments += "<td colspan='5' align='center'>" + (course.LabsDeficiency.Equals("No Equipement Uploaded") ? course.Specialization + " Lab Deficiency" : course.LabsDeficiency) + "</td>";
                            assessments += "</tr>";

                            assessmentCount++;
                        }
                    }
                    else if (course.Degree == "Pharm.D" || course.Degree == "Pharm.D PB")
                    {

                        PharmDandPB += "<tr>";
                        PharmDandPB += "<td colspan='1' align='center'>" + (assessmentCount1 + 1) + "</td>";
                        PharmDandPB += "<td colspan='2' >" + course.Degree + " - " + course.Specialization + "&nbsp;&nbsp;  #</td>";
                        PharmDandPB += "<td colspan='2' align='center'> Yes</td>";
                        PharmDandPB += "</tr>";
                        assessmentCount1++;
                    }


                    //}
                }


                foreach (var course in clearedCourses)
                {
                    string degreeType = db.jntuh_degree.Join(db.jntuh_degree_type, d => d.degreeTypeId, t => t.id, (d, t) => new { d, t })
                                                       .Where(a => a.d.degree == course.Degree).Select(a => a.t.degreeType).FirstOrDefault();

                    List<string> clearedDepartments = clearedCourses.Select(a => a.Department).ToList(); //.Where(a => a.DegreeType == "UG")
                    if (course.Degree == "B.Pharmacy")
                    {
                        if (deficiencyDepartments.Contains(course.Department) && !clearedDepartments.Contains(course.Department))
                        {
                            assessments += "<tr>";
                            assessments += "<td colspan='1' align='center'>" + (assessmentCount + 1) + "</td>";

                            course.Shift = "1";
                            if (course.Shift == "1")
                            {
                                assessments += "<td colspan='5' >" + course.Degree + " - " + course.Specialization + "</td>";
                            }
                            assessments += "<td colspan='2' align='center'>" + course.TotalIntake + "</td>";
                            int Count = deficiencyCourses.Count(D => D.Degree == "Pharm.D" || D.Degree == "Pharm.D PB");
                            if (Count > 0)
                            {
                                int reqFaculty = course.TotalIntake >= 100 ? 35 : 25;
                                assessments += "<td colspan='1' align='center'>" + reqFaculty + "</td>";
                            }
                            else
                            {
                                assessments += "<td colspan='1' align='center'>" + course.Required + "</td>";
                            }
                            int Required = course.Required;
                            int Shortage = Required - BpharmcyAvilableFaculty;
                            assessments += "<td colspan='1' align='center'>" + BpharmcyAvilableFaculty + "</td>";
                            //assessments += "<td colspan='1' align='center'>" + course.Requiredphd + "</td>";
                            //assessments += "<td colspan='1' align='center'>" + course.Availablephd + "</td>";
                            assessments += "<td colspan='5' align='center'>" + (course.LabsDeficiency.Equals("No Equipement Uploaded") ? course.Specialization + " Lab Deficiency" : course.LabsDeficiency) + "</td>";
                            assessments += "</tr>";

                            assessmentCount++;

                            var secondshift = db.jntuh_college_intake_existing.Where(e => e.collegeId == collegeId && e.specializationId == course.SpecializationId && e.shiftId == 2).Select(e => e).FirstOrDefault();

                            if (secondshift != null)
                            {
                                assessments += "<tr>";
                                assessments += "<td colspan='1' align='center'>" + (assessmentCount + 1) + "</td>";

                                course.Shift = "2";
                                if (course.Shift == "2")
                                {
                                    assessments += "<td colspan='5' >" + course.Degree + " - " + course.Specialization + " - 2</td>";
                                }
                                assessments += "<td colspan='1' align='center'>" + course.TotalIntake + "</td>";
                                int Count2 = deficiencyCourses.Count(D => D.Degree == "Pharm.D" || D.Degree == "Pharm.D PB");
                                if (Count2 > 0)
                                {
                                    int reqFaculty = course.TotalIntake >= 100 ? 35 : 25;
                                    assessments += "<td colspan='1' align='center'>" + reqFaculty + "</td>";
                                }
                                else
                                {
                                    assessments += "<td colspan='1' align='center'>" + course.Required + "</td>";
                                }
                                if (course.Degree == "B.Pharmacy")
                                {
                                    int Required1 = course.Required;
                                    int Shortage1 = Required1 - BpharmcyAvilableFaculty;
                                    assessments += "<td colspan='1' align='center'>" + BpharmcyAvilableFaculty + "</td>";
                                }

                                else
                                    assessments += "<td colspan='1' align='center'>" + course.Available + "</td>";
                                //assessments += "<td colspan='1' align='center'>" + course.Requiredphd + "</td>";
                                //assessments += "<td colspan='1' align='center'>" + course.Availablephd + "</td>";
                                assessments += "<td colspan='5' align='center'>" + (course.LabsDeficiency.Equals("No Equipement Uploaded") ? course.Specialization + " Lab Deficiency" : course.LabsDeficiency) + "</td>";
                                assessments += "</tr>";

                                assessmentCount++;
                            }

                        }
                        else
                        {


                            if (course.TotalIntake != 0)
                            {
                                courses += "<tr>";
                                courses += "<td colspan='1' align='center'>" + (rowCount + 1) + ".</td>";
                                course.Shift = "1";

                                if (course.Shift == "1")
                                {
                                    courses += "<td colspan='10'>" + course.Degree + " - " + course.Specialization + "</td>";
                                }
                                if (course.Degree == "B.Pharmacy")
                                {
                                    int IntakeCount = (course.TotalIntake) >= 100 ? 100 : 60;
                                    courses += "<td colspan='2' align='center'>" + IntakeCount + "</td>";
                                }
                                else
                                {

                                    courses += "<td colspan='2' align='center'>" + course.TotalIntake + "</td>";
                                }

                                courses += "</tr>";

                                rowCount++;


                                var secondshift = db.jntuh_college_intake_existing.Where(e => e.collegeId == collegeId && e.specializationId == course.SpecializationId && e.shiftId == 2).Select(e => e).FirstOrDefault();

                                if (secondshift != null)
                                {
                                    int presentYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
                                    int AY1 = db.jntuh_academic_year.Where(a => a.actualYear == (presentYear + 1)).Select(a => a.id).FirstOrDefault();
                                    int DegreeId = db.jntuh_degree.Where(e => e.degree == course.Degree.Trim()).Select(e => e.id).FirstOrDefault();
                                    int approvedIntake = GetIntake(collegeId, AY1, course.SpecializationId, 2, 1, DegreeId);

                                    if (approvedIntake != 0)
                                    {
                                        courses += "<tr>";
                                        courses += "<td colspan='1' align='center'>" + (rowCount + 1) + ".</td>";
                                        course.Shift = "2";

                                        if (course.Shift == "2")
                                        {
                                            courses += "<td colspan='10'>" + course.Degree + " - " + course.Specialization + " - 2</td>";
                                        }

                                        courses += "<td colspan='2' align='center'>" + GetIntake(collegeId, AY1, course.SpecializationId, 2, 1, DegreeId) + "</td>";
                                        courses += "</tr>";

                                        rowCount++;
                                    }
                                }
                            }
                        }
                    }
                    else if (course.Degree == "Pharm.D" || course.Degree == "Pharm.D PB")
                    {
                        if (deficiencyDepartments.Contains(course.Department) && !clearedDepartments.Contains(course.Department))
                        {
                            PharmDandPB += "<tr>";
                            PharmDandPB += "<td colspan='1' align='center'>" + (assessmentCount1 + 1) + "</td>";
                            PharmDandPB += "<td colspan='2' >" + course.Degree + " - " + course.Specialization + "&nbsp;&nbsp; #</td>";
                            PharmDandPB += "<td colspan='2' align='center'> Yes</td>";
                            PharmDandPB += "</tr>";
                            assessmentCount1++;

                        }
                        else
                        {


                            if (course.TotalIntake != 0)
                            {
                                courses += "<tr>";
                                courses += "<td colspan='1' align='center'>" + (rowCount + 1) + ".</td>";
                                course.Shift = "1";

                                if (course.Shift == "1")
                                {
                                    courses += "<td colspan='10'>" + course.Degree + " - " + course.Specialization + "</td>";
                                }

                                courses += "<td colspan='2' align='center'>" + course.TotalIntake + "</td>";
                                courses += "</tr>";

                                rowCount++;


                                var secondshift = db.jntuh_college_intake_existing.Where(e => e.collegeId == collegeId && e.specializationId == course.SpecializationId && e.shiftId == 2).Select(e => e).FirstOrDefault();

                                if (secondshift != null)
                                {
                                    int presentYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
                                    int AY1 = db.jntuh_academic_year.Where(a => a.actualYear == (presentYear + 1)).Select(a => a.id).FirstOrDefault();
                                    int DegreeId = db.jntuh_degree.Where(e => e.degree == course.Degree.Trim()).Select(e => e.id).FirstOrDefault();
                                    int approvedIntake = GetIntake(collegeId, AY1, course.SpecializationId, 2, 1, DegreeId);

                                    if (approvedIntake != 0)
                                    {
                                        courses += "<tr>";
                                        courses += "<td colspan='1' align='center'>" + (rowCount + 1) + ".</td>";
                                        course.Shift = "2";

                                        if (course.Shift == "2")
                                        {
                                            courses += "<td colspan='10'>" + course.Degree + " - " + course.Specialization + " - 2</td>";
                                        }

                                        courses += "<td colspan='2' align='center'>" + GetIntake(collegeId, AY1, course.SpecializationId, 2, 1, DegreeId) + "</td>";
                                        courses += "</tr>";

                                        rowCount++;
                                    }
                                }
                            }
                        }
                    }

                }
                int totalCleared = clearedCourses.Count();
                int totalZeroIntake = clearedCourses.Where(c => c.TotalIntake == 0).Count();

                if (clearedCourses.Count() == 0 || totalCleared == totalZeroIntake)
                {
                    courses += "<tr>";
                    courses += "<td colspan='13' align='center'>NIL</td>";
                    courses += "</tr>";
                }

                assessments += "</table>" + "</br>";
                courses += "</table>";
                PharmDandPB += "</table>";

                // deficiencyDepartments1.Add("Pharm.D");
                //deficiencyDepartments1.Add("Pharm.D PB");
                if (deficiencyDepartments.Contains("Pharmacy"))
                {
                    if (deficiencyDepartments.Contains("Pharm.D") || deficiencyDepartments.Contains("Pharm.D PB"))
                    {
                        assessments += PharmDandPB;
                    }
                    return courses + "$" + assessments + "$" + assessmentCount;
                }
                else
                {
                    if (assessmentCount1 > 0)
                        return courses + "$" + PharmDandPB + "$" + assessmentCount1;
                    else
                    {
                        PharmDandPB = "";
                        return courses + "$" + PharmDandPB + "$" + assessmentCount1;
                    }
                }






            }
            else
            {


                courses += "<table border='1' cellpadding='3' cellspacing='0' width='100%' style='font-family: Times New Roman; font-size: 9px; font-weight: normal; background-color: #fff; margin: 0 auto;'>";
                courses += "<tr>";
                courses += "<th colspan='1' align='center'><b>S.No</b></th>";
                courses += "<th colspan='10'><b>Name of the Course</b></th>";
                courses += "<th colspan='2' align='center'><b>Intake</b></th>";
                courses += "</tr>";

                assessments += "<table border='1' cellpadding='3' cellspacing='0' width='100%' style='font-family: Times New Roman; font-size: 9px; font-weight: normal; background-color: #fff; margin: 0 auto;'>";
                assessments += "<tr>";
                assessments += "<th rowspan='2' colspan='1' align='center'><b>S.No</b></th>";
                assessments += "<th rowspan='2' colspan='5'><b>Course</b></th>";
                assessments += "<th colspan='4' align='center'><b>Faculty Shortage *</b></th>";
                assessments += "<th colspan='5' align='center'><b>Lab Shortage</b></th>";
                assessments += "</tr>";
                assessments += "<tr>";
                assessments += "<th colspan='1' align='center'><b>R</b></th>";
                assessments += "<th colspan='1' align='center'><b>A</b></th>";
                assessments += "<th colspan='1' align='center'><b>R.Ph.D</b></th>";
                assessments += "<th colspan='1' align='center'><b>A.Ph.D</b></th>";
                assessments += "<th colspan='5' align='center'><b>Name of the Lab(s)</b></th>";
                assessments += "</tr>";


                //Only Btech Affiliation Letters Filtering
                //faculty = faculty.Where(e => e.Degree == "B.Tech" || e.Degree == "B.Pharmacy" || e.Degree == "Pharm.D" || e.Degree == "Pharm.D PB").Select(e => e).ToList();
                //labs = labs.Where(e => e.Degree == "B.Tech" || e.Degree == "B.Pharmacy" || e.Degree == "Pharm.D" || e.Degree == "Pharm.D PB").Select(e => e).ToList();
                //Only MBA And MCA
                faculty = faculty.Where(e => e.Degree == "MBA" || e.Degree == "MCA").Select(e => e).ToList();
                labs = labs.Where(e => e.Degree == "MBA" || e.Degree == "MCA").Select(e => e).ToList();


                //Add New Code for Pharma.D Labs not Add in Lab Master
                int[] Humanities = new int[] { 31, 37, 42, 48, 155, 156, 157, 158 };
                int[] FSpecIds = faculty.Where(e => !Humanities.Contains(e.SpecializationId)).Select(F => F.SpecializationId).ToArray();//
                int[] LSpecIds = labs.Select(F => F.SpecializationId).ToArray();
                int[] PharmacySpecualizationIds = new int[] { 12, 18, 13, 114, 115, 116, 117, 118, 119, 120, 121, 122, 123, 124, 19, 68, 38, 374, 26, 108, 179, 192, 335, 196, 109, 9, 134, 70, 198, 180, 367, 32, 183, 171, 11, 399 };

                var pharmacycollegesids = new[]
                {
                    24, 27, 30, 33, 34, 44, 47, 52, 55, 65, 78, 83, 95, 97, 104, 105, 107, 110, 114, 117, 118, 127, 135,
                    136, 139, 146, 150, 159, 169, 202, 204, 213, 219, 234, 239, 253,
                    262, 263, 267, 275, 283, 284, 290, 295, 297, 298, 301, 303, 314, 317, 320, 348, 353, 362, 376, 377,
                    379, 384, 392, 395, 410, 427, 428, 436, 442, 445, 448, 454, 457, 458, 6,
                    45, 54, 58, 90, 120, 206, 237, 302, 313, 318, 319, 389, 60, 66, 252, 255, 315, 370, 108, 38,223
                };
                //Some Colleges only Put Lab Deficiency NIL ,ex.College Uploaded New Courses.  B.Tech and B.pharmacy

                //if (pharmacycollegesids.Contains(collegeId))
                //{
                //    foreach (int id in FSpecIds)
                //    {
                //        //if (PharmacySpecualizationIds.Contains(id))
                //        // {
                //        if (!LSpecIds.Contains(id))
                //        {
                //            CollegeFacultyLabs lab = new CollegeFacultyLabs();
                //            lab.SpecializationId = id;
                //            lab.Deficiency = "NIL";
                //            lab.LabsDeficiency = "NIL";
                //            labs.Add(lab);
                //        }
                //        // }
                //    }
                //}


                //MBA Lab Deficiency NIL 


                foreach (int id in FSpecIds)
                {
                    if (id == 14)
                    {
                        if (!LSpecIds.Contains(id))
                        {
                            CollegeFacultyLabs lab = new CollegeFacultyLabs();
                            lab.SpecializationId = id;
                            lab.Deficiency = "NIL";
                            lab.LabsDeficiency = "NIL";
                            labs.Add(lab);
                        }
                        else
                        {
                            labs.Where(e => e.SpecializationId == 14).ToList().ForEach(e => e.LabsDeficiency = "NIL");
                        }
                    }
                }




                clearedCourses = faculty.Join(labs, f => f.SpecializationId, l => l.SpecializationId, (f, l) => new { f, l })
                                        .Where(a => a.f.Deficiency == "NO" && a.f.PhdDeficiency != "YES" && a.l.LabsDeficiency == "NIL")
                                        .Select(a => new CollegeFacultyLabs
                                        {
                                            Degree = a.f.Degree,
                                            Department = a.f.Department,
                                            SpecializationId = a.f.SpecializationId,
                                            Specialization = a.f.Specialization,
                                            TotalIntake = a.f.TotalIntake,
                                            Required = a.f.Required,
                                            Available = a.f.Available,
                                            Deficiency = a.f.Deficiency,
                                            Requiredphd = a.f.Requiredphd,
                                            Availablephd = a.f.Availablephd,
                                            PhdDeficiency = a.f.PhdDeficiency,
                                            LabsDeficiency = a.l.LabsDeficiency
                                        })
                                        .OrderBy(a => a.DegreeDisplayOrder).ThenBy(a => a.Department).ThenBy(a => a.Specialization)
                                        .ToList();

                deficiencyCourses = faculty.Join(labs, f => f.SpecializationId, l => l.SpecializationId, (f, l) => new { f, l })
                                            .Where(a => (a.f.Deficiency == "YES" || a.f.PhdDeficiency == "YES") || a.l.LabsDeficiency != "NIL")
                                            .Select(a => new CollegeFacultyLabs
                                            {
                                                Degree = a.f.Degree,
                                                Department = a.f.Department,
                                                SpecializationId = a.f.SpecializationId,
                                                Specialization = a.f.Specialization,
                                                TotalIntake = a.f.TotalIntake,
                                                Required = a.f.Required,
                                                Available = a.f.Available,
                                                Requiredphd = a.f.Requiredphd,
                                                Availablephd = a.f.Availablephd,
                                                Deficiency = a.f.Deficiency,
                                                PhdDeficiency = a.f.PhdDeficiency,
                                                LabsDeficiency = a.l.LabsDeficiency
                                            })
                                            .OrderBy(a => a.DegreeDisplayOrder).ThenBy(a => a.Department).ThenBy(a => a.Specialization)
                                            .ToList();




                foreach (var course in deficiencyCourses)
                {
                    if (!deficiencyDepartments.Contains(course.Department))
                    {
                        deficiencyDepartments.Add(course.Department);
                    }

                    assessments += "<tr>";
                    assessments += "<td colspan='1' align='center'>" + (assessmentCount + 1) + "</td>";

                    course.Shift = "1";
                    if (course.Shift == "1")
                    {
                        assessments += "<td colspan='5' >" + course.Degree + " - " + course.Specialization + "</td>";
                    }

                    //assessments += "<td colspan='2' align='center'>" + (course.Required - course.Available) + "</td>";
                    assessments += "<td colspan='1' align='center'>" + course.Required + "</td>";
                    assessments += "<td colspan='1' align='center'>" + course.Available + "</td>";
                    //assessments += "<td colspan='2' align='center'>" + course.PhdDeficiency + "</td>";
                    assessments += "<td colspan='1' align='center'>" + course.Requiredphd + "</td>";
                    assessments += "<td colspan='1' align='center'>" + course.Availablephd + "</td>";
                    assessments += "<td colspan='5' align='center'>" + (course.LabsDeficiency.Equals("No Equipement Uploaded") ? course.Specialization + " Lab Deficiency" : course.LabsDeficiency) + "</td>";
                    assessments += "</tr>";

                    assessmentCount++;

                    var secondshift = db.jntuh_college_intake_existing.Where(e => e.collegeId == collegeId && e.specializationId == course.SpecializationId && e.shiftId == 2).Select(e => e).FirstOrDefault();

                    if (secondshift != null)
                    {
                        assessments += "<tr>";
                        assessments += "<td colspan='1' align='center'>" + (assessmentCount + 1) + "</td>";

                        course.Shift = "2";
                        if (course.Shift == "2")
                        {
                            assessments += "<td colspan='5' >" + course.Degree + " - " + course.Specialization + " - 2</td>";
                        }

                        //assessments += "<td colspan='2' align='center'>" + (course.Required - course.Available) + "</td>";
                        assessments += "<td colspan='1' align='center'>" + course.Required + "</td>";
                        assessments += "<td colspan='1' align='center'>" + course.Available + "</td>";
                        //assessments += "<td colspan='2' align='center'>" + course.PhdDeficiency + "</td>";
                        assessments += "<td colspan='1' align='center'>" + course.Requiredphd + "</td>";
                        assessments += "<td colspan='1' align='center'>" + course.Availablephd + "</td>";
                        assessments += "<td colspan='5' align='center'>" + (course.LabsDeficiency.Equals("No Equipement Uploaded") ? course.Specialization + " Lab Deficiency" : course.LabsDeficiency) + "</td>";
                        assessments += "</tr>";

                        assessmentCount++;
                    }

                    if (course.TotalIntake > 60 && course.Availablephd >= 1 && course.Available >= course.Required)
                    {
                        courses += "<tr>";
                        courses += "<td colspan='1' align='center'>" + (rowCount + 1) + ".</td>";
                        course.Shift = "1";

                        if (course.Shift == "1")
                        {
                            courses += "<td colspan='10'>" + course.Degree + " - " + course.Specialization + "</td>";
                        }

                        courses += "<td colspan='2' align='center'>" + PhdWiseIntakeForMBAandMCA(course.Availablephd) + "</td>";

                        courses += "</tr>";
                        rowCount++;
                        //  assessmentCount = 0;
                    }






                    //}
                }

                foreach (var course in clearedCourses)
                {
                    string degreeType = db.jntuh_degree.Join(db.jntuh_degree_type, d => d.degreeTypeId, t => t.id, (d, t) => new { d, t })
                                                       .Where(a => a.d.degree == course.Degree).Select(a => a.t.degreeType).FirstOrDefault();

                    List<string> clearedDepartments = clearedCourses.Select(a => a.Department).ToList(); //.Where(a => a.DegreeType == "UG")

                    if (deficiencyDepartments.Contains(course.Department) && !clearedDepartments.Contains(course.Department))
                    {
                        assessments += "<tr>";
                        assessments += "<td colspan='1' align='center'>" + (assessmentCount + 1) + "</td>";

                        course.Shift = "1";
                        if (course.Shift == "1")
                        {
                            assessments += "<td colspan='5' >" + course.Degree + " - " + course.Specialization + "</td>";
                        }

                        //assessments += "<td colspan='2' align='center'>" + (course.Required - course.Available) + "</td>";
                        assessments += "<td colspan='1' align='center'>" + course.Required + "</td>";
                        assessments += "<td colspan='1' align='center'>" + course.Available + "</td>";
                        //assessments += "<td colspan='2' align='center'>" + course.PhdDeficiency + "</td>";
                        assessments += "<td colspan='1' align='center'>" + course.Requiredphd + "</td>";
                        assessments += "<td colspan='1' align='center'>" + course.Availablephd + "</td>";
                        assessments += "<td colspan='5' align='center'>" + (course.LabsDeficiency.Equals("No Equipement Uploaded") ? course.Specialization + " Lab Deficiency" : course.LabsDeficiency) + "</td>";
                        assessments += "</tr>";

                        assessmentCount++;

                        var secondshift = db.jntuh_college_intake_existing.Where(e => e.collegeId == collegeId && e.specializationId == course.SpecializationId && e.shiftId == 2).Select(e => e).FirstOrDefault();

                        if (secondshift != null)
                        {
                            assessments += "<tr>";
                            assessments += "<td colspan='1' align='center'>" + (assessmentCount + 1) + "</td>";

                            course.Shift = "2";
                            if (course.Shift == "2")
                            {
                                assessments += "<td colspan='5' >" + course.Degree + " - " + course.Specialization + " - 2</td>";
                            }

                            //assessments += "<td colspan='2' align='center'>" + (course.Required - course.Available) + "</td>";
                            assessments += "<td colspan='1' align='center'>" + course.Required + "</td>";
                            assessments += "<td colspan='1' align='center'>" + course.Available + "</td>";
                            //assessments += "<td colspan='2' align='center'>" + course.PhdDeficiency + "</td>";
                            assessments += "<td colspan='1' align='center'>" + course.Requiredphd + "</td>";
                            assessments += "<td colspan='1' align='center'>" + course.Availablephd + "</td>";
                            assessments += "<td colspan='5' align='center'>" + (course.LabsDeficiency.Equals("No Equipement Uploaded") ? course.Specialization + " Lab Deficiency" : course.LabsDeficiency) + "</td>";
                            assessments += "</tr>";

                            assessmentCount++;
                        }

                    }
                    else
                    {
                        if (course.TotalIntake != 0)
                        {
                            courses += "<tr>";
                            courses += "<td colspan='1' align='center'>" + (rowCount + 1) + ".</td>";
                            course.Shift = "1";

                            if (course.Shift == "1")
                            {
                                courses += "<td colspan='10'>" + course.Degree + " - " + course.Specialization + "</td>";
                            }
                            if (course.Degree == "B.Pharmacy")
                            {
                                int intake1 = course.TotalIntake >= 100 ? 100 : 60;
                                courses += "<td colspan='2' align='center'>" + intake1 + "</td>";
                            }
                            else
                            {
                                courses += "<td colspan='2' align='center'>" + course.TotalIntake + "</td>";
                            }
                            courses += "</tr>";

                            rowCount++;


                            var secondshift = db.jntuh_college_intake_existing.Where(e => e.collegeId == collegeId && e.specializationId == course.SpecializationId && e.shiftId == 2).Select(e => e).FirstOrDefault();

                            if (secondshift != null)
                            {
                                int presentYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
                                int AY1 = db.jntuh_academic_year.Where(a => a.actualYear == (presentYear + 1)).Select(a => a.id).FirstOrDefault();
                                int DegreeId = db.jntuh_degree.Where(e => e.degree == course.Degree.Trim()).Select(e => e.id).FirstOrDefault();
                                int approvedIntake = GetIntake(collegeId, AY1, course.SpecializationId, 2, 1, DegreeId);

                                if (approvedIntake != 0)
                                {
                                    courses += "<tr>";
                                    courses += "<td colspan='1' align='center'>" + (rowCount + 1) + ".</td>";
                                    course.Shift = "2";

                                    if (course.Shift == "2")
                                    {
                                        courses += "<td colspan='10'>" + course.Degree + " - " + course.Specialization + " - 2</td>";
                                    }

                                    courses += "<td colspan='2' align='center'>" + GetIntake(collegeId, AY1, course.SpecializationId, 2, 1, DegreeId) + "</td>";
                                    courses += "</tr>";

                                    rowCount++;
                                }
                            }
                        }
                    }
                }

                int totalCleared = clearedCourses.Count();
                int totalZeroIntake = clearedCourses.Where(c => c.TotalIntake == 0).Count();

                if ((clearedCourses.Count() == 0 || totalCleared == totalZeroIntake) && rowCount > 0)
                {
                    //courses += "<tr>";
                    //courses += "<td colspan='13' align='center'>NIL</td>";
                    //courses += "</tr>";
                }
                else if (clearedCourses.Count() == 0 || totalCleared == totalZeroIntake)
                {
                    courses += "<tr>";
                    courses += "<td colspan='13' align='center'>NIL</td>";
                    courses += "</tr>";
                }

                assessments += "</table>";
                courses += "</table>";

                return courses + "$" + assessments + "$" + assessmentCount;


            }

            #endregion Pharmacy Affliation Letter End














        }


        //M.Tech Colleges Affiliation PDF

        public ActionResult AffiliationLetterForMTech(int collegeId, string type)
        {
            List<int> collegeIds = db.jntuh_college.Where(c => c.collegeCode != "WL").Select(c => c.id).ToList();

            string code = db.jntuh_college.Where(c => c.id == collegeId && c.isActive == true).Select(c => c.collegeCode).FirstOrDefault().ToUpper();

            string path = SaveCollegeDefficiencyLetterForMTechPdf(code, type);
            path = path.Replace("/", "\\");
            if (!string.IsNullOrEmpty(path))
            {
                return File(path, "application/pdf", path.Substring(path.LastIndexOf('\\') + 1));
            }
            return View();
        }

        private string SaveCollegeDefficiencyLetterForMTechPdf(string collegeCode, string type)
        {
            string fullPath = string.Empty;

            int Collegeid = db.jntuh_college.Where(C => C.collegeCode == collegeCode && C.isActive == true).Select(C => C.id).FirstOrDefault();
            string ECollegeid = UAAAS.Models.Utilities.EncryptString(Collegeid.ToString(), System.Web.Configuration.WebConfigurationManager.AppSettings["CryptoKey"]);


            //Set page size as A4
            var pdfDoc = new Document(PageSize.A4, 40, 40, 40, 40);

            // Create a new PdfWrite object, writing the output to a MemoryStream
            //open pdf
            var output = new MemoryStream();
            var writer = PdfWriter.GetInstance(pdfDoc, output);

            int InspectionPhaseId = db.jntuh_inspection_phase.Where(p => p.isActive == true).Select(p => p.id).SingleOrDefault();

            string path = Server.MapPath("~/Content/PDFReports/AffiliationProceedings/");

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            fullPath = path + collegeCode + "-" + ECollegeid + "-AffiliationProceedings.pdf";
            PdfWriter.GetInstance(pdfDoc, new FileStream(fullPath, FileMode.Create));

            //Open PDF Document to write data
            pdfDoc.Open();

            //Assign Html content in a string to write in PDF
            string contents = string.Empty;

            StreamReader sr;

            //Read file from server path
            sr = System.IO.File.OpenText(Server.MapPath("~/Content/AffiliationProceedingsMTech.html"));

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

            string collegeName = db.jntuh_college.Where(c => c.collegeCode == collegeCode).Select(c => c.collegeName).FirstOrDefault();

            if (address != null)
            {
                scheduleCollegeAddress = collegeName + ",<br />" + address.address;
                scheduleCollegeAddress = scheduleCollegeAddress + ", " + address.townOrCity;
                scheduleCollegeAddress = scheduleCollegeAddress + ", " + address.mandal;
                scheduleCollegeAddress = scheduleCollegeAddress + ",<br />" + district;
                scheduleCollegeAddress = scheduleCollegeAddress + " - " + address.pincode;
            }

            if (address != null)
            {
                scheduleCollegeAddress1 = collegeName + ", " + address.address;
                scheduleCollegeAddress1 = scheduleCollegeAddress1 + ", " + address.townOrCity;
                scheduleCollegeAddress1 = scheduleCollegeAddress1 + ", " + address.mandal;
                scheduleCollegeAddress1 = scheduleCollegeAddress1 + ", " + district;
                scheduleCollegeAddress1 = scheduleCollegeAddress1 + " - " + address.pincode;
            }

            string CollegeSocietyAddress = string.Empty;
            if (societyAddress != null)
            {
                CollegeSocietyAddress = collegeName + ", " + societyAddress.address;
                CollegeSocietyAddress = CollegeSocietyAddress + ", " + societyAddress.townOrCity;
                CollegeSocietyAddress = CollegeSocietyAddress + ", " + societyAddress.mandal;
                CollegeSocietyAddress = CollegeSocietyAddress + ", " + district;
                CollegeSocietyAddress = CollegeSocietyAddress + " - " + societyAddress.pincode;
            }
            scheduleCollegeAddress = scheduleCollegeAddress.ToUpper().Replace(",", ", ").Replace("(", " (").Replace(")", ") ");
            CollegeSocietyAddress = CollegeSocietyAddress.ToUpper().Replace(",", ", ").Replace("(", " (").Replace(")", ") ");
            contents = contents.Replace("##SOCIETY_ADDRESS##", CollegeSocietyAddress);
            contents = contents.Replace("##COLLEGE_ADDRESS##", scheduleCollegeAddress);
            contents = contents.Replace("##COLLEGE_ADDRESS1##", scheduleCollegeAddress1);
            contents = contents.Replace("##LETTER_DATE##", DateTime.Now.ToString("dd-MM-yyy"));

            //var dates = db.jntuh_dates.Where(d => d.collegecode == collegeCode).Select(d => d).FirstOrDefault();
            //contents = contents.Replace("##APPLICATION_DATE##", ((DateTime)dates.applicationDate).ToString("dd-MM-yyy"));
            //contents = contents.Replace("##DEFICIENCY_REPORT_DATE##", ((DateTime)dates.deficiencyLetterDate).ToString("dd-MM-yyy"));
            //contents = contents.Replace("##HEARING_DATE##", dates.hearingDate != null ? ((DateTime)dates.hearingDate).ToString("dd-MM-yyy") : "");

            var deficiencydates = db.jntuh_dates.Where(d => d.collegecode == collegeCode).Select(d => d).FirstOrDefault();
            var dates = db.jntuh_college_edit_status.Where(d => d.collegeId == collegeid && d.IsCollegeEditable == false).Select(d => d).FirstOrDefault();
            var Appeal = db.jntuh_appeal_college_edit_status.Where(d => d.collegeId == collegeid && d.IsCollegeEditable == false).Select(d => d).FirstOrDefault();
            if (Appeal != null)
            {
                contents = contents.Replace("##APPLICATION_DATE##", ((DateTime)Appeal.updatedOn).ToString("dd-MM-yyy"));
            }
            else
            {
                contents = contents.Replace("##APPLICATION_DATE##", "NIL");
            }

            if (dates != null)
            {
                contents = contents.Replace("##116Submitted_DATE##", ((DateTime)dates.updatedOn).ToString("dd-MM-yyy"));
            }
            else
            {
            }

            if (deficiencydates != null)
            {
                contents = contents.Replace("##DEFICIENCY_REPORT_DATE##", ((DateTime)deficiencydates.deficiencyLetterDate).ToString("dd-MM-yyy"));
            }
            else
            {
                contents = contents.Replace("##DEFICIENCY_REPORT_DATE##", "NIL");
            }

            string AppealSubmittedMessage = "Pursuant to the communication of deficiencies you have filed an appeal for reconsideration and the University reviewed the same or re inspection was conducted. Based on the above the University has accorded affiliation to the following courses.";
            string AppealNotSubmittedMessage = "Based on the above the University has accorded affiliation to the following courses.";
            contents = contents.Replace("##HEARING_DATE##", Appeal != null ? ((DateTime)Appeal.updatedOn).ToString("dd-MM-yyy") : "NIL");
            if (Appeal != null)
                contents = contents.Replace("##College_Message##", AppealSubmittedMessage);
            else
                contents = contents.Replace("##College_Message##", AppealNotSubmittedMessage);


            if (type == "All")
            {
                contents = contents.Replace("##COURSE_TABLE##", CollegeCoursesAll(collegeid));
            }
            else if (type == "AllClear")
            {
                string[] courses = CollegeCoursesAllClearForMTech(collegeid).Split('$');
                contents = contents.Replace("##COURSE_TABLE##", courses[0]);
                contents = contents.Replace("##ASSESSMNET_TABLE##", courses[1]);
                string defiencyRows = courses[2];

                if (defiencyRows != "0")
                {
                    contents = contents.Replace("##HIDE_START##", "<!--");
                    contents = contents.Replace("##HIDE_END##", "-->");
                }
                else
                {
                    contents = contents.Replace("##HIDE_START##", "<table border='1' style=''width:100%;text-align:center;padding-top:20px'><tr><td style='text-align:center'>NIL</td></tr></table>");
                }
            }

            var pharmacyids = new[]{24,27,30,33,34,44,47,52,55,65,78,83,95,97,104,105,107,110,114,117,118,127,135,136,139,146,150,159,169,202,204,213,219,234,239,253,
                262,263,267,275,283,284,290,295,297,298,301,303,314,317,320,348,353,362,376,377,379,384,392,395,410,427,428,436,442,445,448,454,457,458,6,
                45,54,58,90,120,206,237,302,313,318,319,389,60,66,252,255,315,370};
            if (pharmacyids.Contains(collegeid))
            {
                contents = contents.Replace("##END##", "<br/><b>R</b>- Required Faculty,<b>A</b>-Available Faculty,<b>R.Ph.D</b>-Required Ph.D Faculty,<b>A.Ph.D</b>-Available Ph.D Faculty.  <br/>* Any data discrepancies may be brought to the notice of the University within two days.<br/>");
                contents = contents.Replace("##DEGREE##", "M.Pharmacy");
            }
            else
            {
                contents = contents.Replace("##END##", "<br/> <b>R</b>- Required Faculty,<b>A</b>-Available Faculty,<b>R.Ph.D</b>-Required Ph.D Faculty,<b>A.Ph.D</b>-Available Ph.D Faculty. <br/>* Any data discrepancies may be brought to the notice of the University within two days.");
                contents = contents.Replace("##DEGREE##", "M.Tech");
            }






            //Read string contents using stream reader and convert html to parsed conent
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

        public string CollegeCoursesAllClearForMTech(int collegeId)
        {
            string courses = string.Empty;
            string assessments = string.Empty;
            string PharmDandPB = string.Empty;
            List<CollegeIntakeExisting> collegeIntakeExisting = new List<CollegeIntakeExisting>();

            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);

            var degreeDetails = (from ie in db.jntuh_college_intake_existing
                                 join js in db.jntuh_specialization on ie.specializationId equals js.id
                                 join jd in db.jntuh_department on js.departmentId equals jd.id
                                 join deg in db.jntuh_degree on jd.degreeId equals deg.id
                                 join sh in db.jntuh_shift on ie.shiftId equals sh.id
                                 where (ie.collegeId == collegeId && ie.academicYearId == 9)
                                 select new
                                 {
                                     ie.proposedIntake,
                                     js.specializationName,
                                     deg.degree,
                                     sh.shiftName,
                                     deg.degreeDisplayOrder,
                                     js.id
                                 }).OrderBy(d => d.degreeDisplayOrder).ThenBy(d => d.specializationName).ThenBy(d => d.shiftName).ToList();


            int rowCount = 0; int assessmentCount = 0; int assessmentCount1 = 0;

            //foreach (var item in degreeDetails)
            //{
            //faculty
            //string[] fStatus = NoFacultyDeficiencyCourse(collegeId, item.id).Split('$');


            //string fFlag = fStatus[0];
            //string facultyShortage = fStatus[1];
            //string phd = fStatus[2];

            ////labs
            //string[] lStatus = NoLabDeficiencyCourse(collegeId, item.id).Split('$');

            //string lFlag = lStatus[0];
            //string labs = lStatus[1];
            var faculty = new List<CollegeFacultyLabs>();

            var integreatedIds = new[] { 9, 18, 39, 42, 75, 140, 180, 332, 364, 375 };

            var pharmacyids = new[]{24,27,30,33,34,44,47,52,55,65,78,83,95,97,104,105,107,110,114,117,118,127,135,136,139,146,150,159,169,202,204,213,219,234,239,253,
                262,263,267,275,283,284,290,295,297,298,301,303,314,317,320,348,353,362,376,377,379,384,392,395,410,427,428,436,442,445,448,454,457,458,6,
                45,54,58,90,120,206,237,302,313,318,319,389,60,66,252,255,315,370};

            if (pharmacyids.Contains(collegeId))
            {
                faculty = PharmacyDeficienciesInFaculty(collegeId);
            }
            else if (integreatedIds.Contains(collegeId))
            {
                var integratedpharmacyfaculty = PharmacyDeficienciesInFaculty(collegeId);
                var integratedbtechfaculty = DeficienciesInFaculty(collegeId);
                faculty = integratedbtechfaculty.Concat(integratedpharmacyfaculty).ToList();
            }
            else
            {
                faculty = DeficienciesInFaculty(collegeId);
            }
            List<CollegeFacultyLabs> labs = DeficienciesInLabsnew(collegeId);
            //DeficienciesInLabs(collegeId);

            var collegeFacultyLabs = labs.FirstOrDefault(i => i.Degree == "B.Pharmacy");
            if (collegeFacultyLabs != null)
            {
                var bphramcylabs = collegeFacultyLabs.LabsDeficiency;
                if (bphramcylabs != "NIL")
                {
                    foreach (var c in labs.Where(i => i.Degree == "M.Pharmacy").ToList())
                    {
                        c.LabsDeficiency = "YES";
                    }
                }
            }

            foreach (var l in labs)
            {
                if (l.Degree == "B.Tech" && l.LabsDeficiency != "NIL")
                {
                    labs.Where(i => i.Department == l.Department && i.Degree == "M.Tech" && i.LabsDeficiency == "NIL").ToList().ForEach(c => c.LabsDeficiency = "YES");
                }

            }



            List<CollegeFacultyLabs> clearedCourses = new List<CollegeFacultyLabs>();
            List<CollegeFacultyLabs> deficiencyCourses = new List<CollegeFacultyLabs>();
            List<string> deficiencyDepartments = new List<string>();
            List<string> deficiencyDepartments1 = new List<string>();

            #region Pharmacy Affliation Letter Start

            if (pharmacyids.Contains(collegeId))
            {

                courses += "<table border='1' cellpadding='3' cellspacing='0' width='100%' style='font-family: Times New Roman; font-size: 9px; font-weight: normal; background-color: #fff; margin: 0 auto;'>";
                courses += "<tr>";
                courses += "<th colspan='1' align='center'><b>S.No</b></th>";
                courses += "<th colspan='10'><b>Name of the Course</b></th>";
                courses += "<th colspan='2' align='center'><b>Intake</b></th>";
                courses += "</tr>";

                PharmDandPB += "<br/><table border='1' cellpadding='3' cellspacing='0' width='100%' style='font-family: Times New Roman; font-size: 9px; font-weight: normal; background-color: #fff; margin: 0 auto;'>";
                PharmDandPB += "<tr>";
                PharmDandPB += "<th colspan='1' align='center'><b>S.No</b></th>";
                PharmDandPB += "<th colspan='2' align='center'><b> Course</b></th>";
                PharmDandPB += "<th colspan='2' align='center'><b>Deficiency</b></th>";
                PharmDandPB += "</tr>";

                assessments += "<table border='1' cellpadding='3' cellspacing='0' width='100%' style='font-family: Times New Roman; font-size: 9px; font-weight: normal; background-color: #fff; margin: 0 auto;'>";
                assessments += "<tr>";
                assessments += "<th rowspan='2' colspan='1' align='center'><b>S.No</b></th>";
                assessments += "<th rowspan='2' colspan='5' align='center'><b>Course</b></th>";
                assessments += "<th rowspan='2' colspan='2' align='center'><b>Intake</b></th>";
                assessments += "<th colspan='4' align='center'><b>Faculty Shortage *</b></th>";
                assessments += "<th colspan='3' align='center'><b>Lab Shortage</b></th>";
                assessments += "</tr>";
                assessments += "<tr>";
                assessments += "<th colspan='1' align='center'><b>R</b></th>";
                assessments += "<th colspan='1' align='center'><b>A</b></th>";
                assessments += "<th colspan='1' align='center'><b>R.Ph.D</b></th>";
                assessments += "<th colspan='1' align='center'><b>A.Ph.D</b></th>";
                assessments += "<th colspan='3' align='center'><b>Name of the Lab(s)</b></th>";
                assessments += "</tr>";
                //Only Btech Affiliation Letters Filtering
                //faculty =faculty.Where(e => e.Degree == "B.Pharmacy" || e.Degree == "Pharm.D" || e.Degree == "Pharm.D PB").Select(e => e).ToList();
                //labs =labs.Where(e => e.Degree == "B.Pharmacy" || e.Degree == "Pharm.D" || e.Degree == "Pharm.D PB").Select(e => e).ToList();
                //Only M.pharmacy

                faculty = faculty.Where(e => e.Degree == "M.Pharmacy").Select(e => e).ToList();
                labs = labs.Where(e => e.Degree == "M.Pharmacy").Select(e => e).ToList();
                faculty = faculty.Select(e => e).ToList();
                labs = labs.Select(e => e).ToList();

                //.Where(a => a.f.Deficiency == "NO" && a.f.PhdDeficiency != "YES" && a.l.LabsDeficiency == "NIL")

                int[] FSpecIds = faculty.Select(F => F.SpecializationId).ToArray();
                int[] LSpecIds = labs.Select(F => F.SpecializationId).ToArray();

                foreach (int id in FSpecIds)
                {
                    if (!LSpecIds.Contains(id))
                    {
                        CollegeFacultyLabs lab = new CollegeFacultyLabs();
                        lab.SpecializationId = id;
                        lab.Deficiency = "NIL";
                        lab.LabsDeficiency = "NIL";
                        labs.Add(lab);
                    }

                }


                clearedCourses = faculty.Join(labs, f => f.SpecializationId, l => l.SpecializationId, (f, l) => new { f, l })
                        .Where(a => a.f.Deficiency == "NO" && a.f.PhdDeficiency != "YES" && a.l.LabsDeficiency == "NIL")
                        .Select(a => new CollegeFacultyLabs
                        {
                            Degree = a.f.Degree,
                            Department = a.f.Department,
                            SpecializationId = a.f.SpecializationId,
                            Specialization = a.f.Specialization,
                            TotalIntake = a.f.TotalIntake,
                            Required = a.f.Required,
                            Available = a.f.Available,
                            Deficiency = a.f.Deficiency,
                            Requiredphd = a.f.Requiredphd,
                            Availablephd = a.f.Availablephd,
                            PhdDeficiency = a.f.PhdDeficiency,
                            LabsDeficiency = a.l.LabsDeficiency
                        })
                        .OrderBy(a => a.DegreeDisplayOrder).ThenBy(a => a.Department).ThenBy(a => a.Specialization)
                        .ToList();

                deficiencyCourses = faculty.Join(labs, f => f.SpecializationId, l => l.SpecializationId, (f, l) => new { f, l })
                        .Where(a => (a.f.Deficiency == "YES" || a.f.PhdDeficiency == "YES") || a.l.LabsDeficiency != "NIL")
                        .Select(a => new CollegeFacultyLabs
                        {
                            Degree = a.f.Degree,
                            Department = a.f.Department,
                            SpecializationId = a.f.SpecializationId,
                            Specialization = a.f.Specialization,
                            TotalIntake = a.f.TotalIntake,
                            Required = a.f.Required,
                            Available = a.f.Available,
                            Requiredphd = a.f.Requiredphd,
                            Availablephd = a.f.Availablephd,
                            Deficiency = a.f.Deficiency,
                            PhdDeficiency = a.f.PhdDeficiency,
                            LabsDeficiency = a.l.LabsDeficiency
                        })
                        .OrderBy(a => a.DegreeDisplayOrder).ThenBy(a => a.Department).ThenBy(a => a.Specialization)
                        .ToList();



                foreach (var course in deficiencyCourses)
                {
                    if (!deficiencyDepartments.Contains(course.Department))
                    {
                        deficiencyDepartments.Add(course.Department);
                    }
                    if (course.Degree == "M.Pharmacy")
                    {
                        assessments += "<tr>";
                        assessments += "<td colspan='1' align='center'>" + (assessmentCount + 1) + "</td>";

                        course.Shift = "1";
                        if (course.Shift == "1")
                        {
                            assessments += "<td colspan='5' >" + course.Degree + " - " + course.Specialization + "</td>";
                        }



                        if (course.Degree == "B.Pharmacy")
                        {

                            assessments += "<td colspan='2' align='center'>" + course.TotalIntake + "</td>";
                        }
                        else if (course.Degree == "M.Pharmacy")
                        {
                            assessments += "<td colspan='2' align='center'>15</td>";
                        }
                        else
                        {

                            assessments += "<td colspan='2' align='center'>" + course.TotalIntake + "</td>";
                        }






                        int Count = deficiencyCourses.Count(D => D.Degree == "Pharm.D" || D.Degree == "Pharm.D PB");
                        if (Count > 0)
                        {
                            int reqFaculty = course.TotalIntake >= 100 ? 35 : 25;
                            assessments += "<td colspan='1' align='center'>" + reqFaculty + "</td>";
                        }
                        else
                        {
                            if (course.Degree == "M.Pharmacy" && (course.SpecializationId == 119 || course.SpecializationId == 118 || course.SpecializationId == 114))
                            {
                                assessments += "<td colspan='1' align='center'>5</td>";
                            }
                            else
                            {
                                assessments += "<td colspan='1' align='center'>" + course.Required + "</td>";
                            }


                        }
                        if (course.Degree == "B.Pharmacy")
                        {
                            int Required = course.Required;
                            int Shortage = Required - BpharmcyAvilableFaculty;
                            assessments += "<td colspan='1' align='center'>" + BpharmcyAvilableFaculty + "</td>";
                        }

                        else
                            assessments += "<td colspan='1' align='center'>" + course.Available + "</td>";
                        assessments += "<td colspan='1' align='center'>1</td>";
                        assessments += "<td colspan='1' align='center'>0</td>";
                        assessments += "<td colspan='5' align='center'>" + (course.LabsDeficiency.Equals("No Equipement Uploaded") ? course.Specialization + " Lab Deficiency" : course.LabsDeficiency) + "</td>";
                        assessments += "</tr>";

                        assessmentCount++;

                        //var secondshift = db.jntuh_college_intake_existing.Where(e => e.collegeId == collegeId && e.specializationId == course.SpecializationId && e.shiftId == 2).Select(e => e).FirstOrDefault();

                        //if (secondshift != null)
                        //{
                        //    assessments += "<tr>";
                        //    assessments += "<td colspan='1' align='center'>" + (assessmentCount + 1) + "</td>";

                        //    course.Shift = "2";
                        //    if (course.Shift == "2")
                        //    {
                        //        assessments += "<td colspan='5' >" + course.Degree + " - " + course.Specialization + " - 2</td>";
                        //    }
                        //    assessments += "<td colspan='2' align='center'>" + course.TotalIntake + "</td>";
                        //    int Count1 = deficiencyCourses.Count(D => D.Degree == "Pharm.D" || D.Degree == "Pharm.D PB");
                        //    if (Count1 > 0)
                        //    {
                        //        int reqFaculty = course.TotalIntake >= 100 ? 35 : 25;
                        //        assessments += "<td colspan='1' align='center'>" + reqFaculty + "</td>";
                        //    }
                        //    else
                        //    {
                        //        assessments += "<td colspan='1' align='center'>" + course.Required + "</td>";
                        //    }

                        //    int Required = course.Required;
                        //    int Shortage = Required - BpharmcyAvilableFaculty;
                        //    assessments += "<td colspan='1' align='center'>" + BpharmcyAvilableFaculty + "</td>";
                        //    assessments += "<td colspan='1' align='center'>1</td>";
                        //    assessments += "<td colspan='1' align='center'>0</td>";
                        //    assessments += "<td colspan='5' align='center'>" + (course.LabsDeficiency.Equals("No Equipement Uploaded") ? course.Specialization + " Lab Deficiency" : course.LabsDeficiency) + "</td>";
                        //    assessments += "</tr>";

                        //    assessmentCount++;
                        //}
                    }
                    else if (course.Degree == "Pharm.D" || course.Degree == "Pharm.D PB")
                    {

                        PharmDandPB += "<tr>";
                        PharmDandPB += "<td colspan='1' align='center'>" + (assessmentCount1 + 1) + "</td>";
                        PharmDandPB += "<td colspan='2' >" + course.Degree + " - " + course.Specialization + "&nbsp;&nbsp;  #</td>";
                        PharmDandPB += "<td colspan='2' align='center'> Yes</td>";
                        PharmDandPB += "</tr>";
                        assessmentCount1++;
                    }


                    //}
                }


                foreach (var course in clearedCourses)
                {
                    string degreeType = db.jntuh_degree.Join(db.jntuh_degree_type, d => d.degreeTypeId, t => t.id, (d, t) => new { d, t })
                                                       .Where(a => a.d.degree == course.Degree).Select(a => a.t.degreeType).FirstOrDefault();

                    List<string> clearedDepartments = clearedCourses.Select(a => a.Department).ToList(); //.Where(a => a.DegreeType == "UG")
                    if (course.Degree == "M.Pharmacy")
                    {
                        if (deficiencyDepartments.Contains(course.Department) && !clearedDepartments.Contains(course.Department))
                        {
                            assessments += "<tr>";
                            assessments += "<td colspan='1' align='center'>" + (assessmentCount + 1) + "</td>";

                            course.Shift = "1";
                            if (course.Shift == "1")
                            {
                                assessments += "<td colspan='5' >" + course.Degree + " - " + course.Specialization + "</td>";
                            }
                            assessments += "<td colspan='2' align='center'>" + course.TotalIntake + "</td>";
                            int Count = deficiencyCourses.Count(D => D.Degree == "Pharm.D" || D.Degree == "Pharm.D PB");
                            if (Count > 0)
                            {
                                int reqFaculty = course.TotalIntake >= 100 ? 35 : 25;
                                assessments += "<td colspan='1' align='center'>" + reqFaculty + "</td>";
                            }
                            else
                            {
                                assessments += "<td colspan='1' align='center'>" + course.Required + "</td>";
                            }
                            int Required = course.Required;
                            int Shortage = Required - BpharmcyAvilableFaculty;
                            assessments += "<td colspan='1' align='center'>" + BpharmcyAvilableFaculty + "</td>";
                            //assessments += "<td colspan='1' align='center'>" + course.Requiredphd + "</td>";
                            //assessments += "<td colspan='1' align='center'>" + course.Availablephd + "</td>";
                            assessments += "<td colspan='5' align='center'>" + (course.LabsDeficiency.Equals("No Equipement Uploaded") ? course.Specialization + " Lab Deficiency" : course.LabsDeficiency) + "</td>";
                            assessments += "</tr>";

                            assessmentCount++;

                            //var secondshift = db.jntuh_college_intake_existing.Where(e => e.collegeId == collegeId && e.specializationId == course.SpecializationId && e.shiftId == 2).Select(e => e).FirstOrDefault();

                            //if (secondshift != null)
                            //{
                            //    assessments += "<tr>";
                            //    assessments += "<td colspan='1' align='center'>" + (assessmentCount + 1) + "</td>";

                            //    course.Shift = "2";
                            //    if (course.Shift == "2")
                            //    {
                            //        assessments += "<td colspan='5' >" + course.Degree + " - " + course.Specialization + " - 2</td>";
                            //    }
                            //    assessments += "<td colspan='1' align='center'>" + course.TotalIntake + "</td>";
                            //    int Count2 = deficiencyCourses.Count(D => D.Degree == "Pharm.D" || D.Degree == "Pharm.D PB");
                            //    if (Count2 > 0)
                            //    {
                            //        int reqFaculty = course.TotalIntake >= 100 ? 35 : 25;
                            //        assessments += "<td colspan='1' align='center'>" + reqFaculty + "</td>";
                            //    }
                            //    else
                            //    {
                            //        assessments += "<td colspan='1' align='center'>" + course.Required + "</td>";
                            //    }
                            //    if (course.Degree == "B.Pharmacy")
                            //    {
                            //        int Required1 = course.Required;
                            //        int Shortage1 = Required1 - BpharmcyAvilableFaculty;
                            //        assessments += "<td colspan='1' align='center'>" + BpharmcyAvilableFaculty + "</td>";
                            //    }

                            //    else
                            //        assessments += "<td colspan='1' align='center'>" + course.Available + "</td>";
                            //    assessments += "<td colspan='1' align='center'>1</td>";
                            //    assessments += "<td colspan='1' align='center'>0</td>";
                            //    assessments += "<td colspan='5' align='center'>" + (course.LabsDeficiency.Equals("No Equipement Uploaded") ? course.Specialization + " Lab Deficiency" : course.LabsDeficiency) + "</td>";
                            //    assessments += "</tr>";

                            //    assessmentCount++;
                            //}

                        }
                        else
                        {


                            if (course.TotalIntake != 0)
                            {
                                courses += "<tr>";
                                courses += "<td colspan='1' align='center'>" + (rowCount + 1) + ".</td>";
                                course.Shift = "1";

                                if (course.Shift == "1")
                                {
                                    courses += "<td colspan='10'>" + course.Degree + " - " + course.Specialization + "</td>";
                                }
                                if (course.Degree == "B.Pharmacy")
                                {
                                    int IntakeCount = (course.TotalIntake) >= 100 ? 100 : 60;
                                    courses += "<td colspan='2' align='center'>" + IntakeCount + "</td>";
                                }
                                else if (course.Degree == "M.Pharmacy")
                                {
                                    courses += "<td colspan='2' align='center'>15</td>";
                                }
                                else
                                {

                                    courses += "<td colspan='2' align='center'>" + course.TotalIntake + "</td>";
                                }

                                courses += "</tr>";

                                rowCount++;


                                //var secondshift = db.jntuh_college_intake_existing.Where(e => e.collegeId == collegeId && e.specializationId == course.SpecializationId && e.shiftId == 2).Select(e => e).FirstOrDefault();

                                //if (secondshift != null)
                                //{
                                //    int presentYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
                                //    int AY1 = db.jntuh_academic_year.Where(a => a.actualYear == (presentYear + 1)).Select(a => a.id).FirstOrDefault();
                                //    int DegreeId = db.jntuh_degree.Where(e => e.degree == course.Degree.Trim()).Select(e => e.id).FirstOrDefault();
                                //    int approvedIntake = GetIntake(collegeId, AY1, course.SpecializationId, 2, 1, DegreeId);

                                //    if (approvedIntake != 0)
                                //    {
                                //        courses += "<tr>";
                                //        courses += "<td colspan='1' align='center'>" + (rowCount + 1) + ".</td>";
                                //        course.Shift = "2";

                                //        if (course.Shift == "2")
                                //        {
                                //            courses += "<td colspan='10'>" + course.Degree + " - " + course.Specialization + " - 2</td>";
                                //        }

                                //        courses += "<td colspan='2' align='center'>" + GetIntake(collegeId, AY1, course.SpecializationId, 2, 1, DegreeId) + "</td>";
                                //        courses += "</tr>";

                                //        rowCount++;
                                //    }
                                //}
                            }
                        }
                    }
                    else if (course.Degree == "Pharm.D" || course.Degree == "Pharm.D PB")
                    {
                        if (deficiencyDepartments.Contains(course.Department) && !clearedDepartments.Contains(course.Department))
                        {
                            PharmDandPB += "<tr>";
                            PharmDandPB += "<td colspan='1' align='center'>" + (assessmentCount1 + 1) + "</td>";
                            PharmDandPB += "<td colspan='2' >" + course.Degree + " - " + course.Specialization + "&nbsp;&nbsp; #</td>";
                            PharmDandPB += "<td colspan='2' align='center'> Yes</td>";
                            PharmDandPB += "</tr>";
                            assessmentCount1++;

                        }
                        else
                        {


                            if (course.TotalIntake != 0)
                            {
                                courses += "<tr>";
                                courses += "<td colspan='1' align='center'>" + (rowCount + 1) + ".</td>";
                                course.Shift = "1";

                                if (course.Shift == "1")
                                {
                                    courses += "<td colspan='10'>" + course.Degree + " - " + course.Specialization + "</td>";
                                }

                                courses += "<td colspan='2' align='center'>" + course.TotalIntake + "</td>";
                                courses += "</tr>";

                                rowCount++;


                                var secondshift = db.jntuh_college_intake_existing.Where(e => e.collegeId == collegeId && e.specializationId == course.SpecializationId && e.shiftId == 2).Select(e => e).FirstOrDefault();

                                //if (secondshift != null)
                                //{
                                //    int presentYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
                                //    int AY1 = db.jntuh_academic_year.Where(a => a.actualYear == (presentYear + 1)).Select(a => a.id).FirstOrDefault();
                                //    int DegreeId = db.jntuh_degree.Where(e => e.degree == course.Degree.Trim()).Select(e => e.id).FirstOrDefault();
                                //    int approvedIntake = GetIntake(collegeId, AY1, course.SpecializationId, 2, 1, DegreeId);

                                //    if (approvedIntake != 0)
                                //    {
                                //        courses += "<tr>";
                                //        courses += "<td colspan='1' align='center'>" + (rowCount + 1) + ".</td>";
                                //        course.Shift = "2";

                                //        if (course.Shift == "2")
                                //        {
                                //            courses += "<td colspan='10'>" + course.Degree + " - " + course.Specialization + " - 2</td>";
                                //        }

                                //        courses += "<td colspan='2' align='center'>" + GetIntake(collegeId, AY1, course.SpecializationId, 2, 1, DegreeId) + "</td>";
                                //        courses += "</tr>";

                                //        rowCount++;
                                //    }
                                //}
                            }
                        }
                    }

                }
                int totalCleared = clearedCourses.Count();
                int totalZeroIntake = clearedCourses.Where(c => c.TotalIntake == 0).Count();

                if (clearedCourses.Count() == 0 || totalCleared == totalZeroIntake)
                {
                    courses += "<tr>";
                    courses += "<td colspan='13' align='center'>NIL</td>";
                    courses += "</tr>";
                }

                assessments += "</table>" + "</br>";
                courses += "</table>";
                PharmDandPB += "</table>";

                // deficiencyDepartments1.Add("Pharm.D");
                //deficiencyDepartments1.Add("Pharm.D PB");
                if (deficiencyDepartments.Contains("Pharmacy"))
                {
                    if (deficiencyDepartments.Contains("Pharm.D") || deficiencyDepartments.Contains("Pharm.D PB"))
                    {
                        assessments += PharmDandPB;
                    }
                    return courses + "$" + assessments + "$" + assessmentCount;
                }
                else
                {
                    if (assessmentCount1 > 0)
                        return courses + "$" + PharmDandPB + "$" + assessmentCount1;
                    else
                    {
                        PharmDandPB = "";
                        return courses + "$" + PharmDandPB + "$" + assessmentCount1;
                    }
                }






            }
            else
            {


                courses += "<table border='1' cellpadding='3' cellspacing='0' width='100%' style='font-family: Times New Roman; font-size: 9px; font-weight: normal; background-color: #fff; margin: 0 auto;'>";
                courses += "<tr>";
                courses += "<th colspan='1' align='center'><b>S.No</b></th>";
                courses += "<th colspan='10'><b>Name of the Course</b></th>";
                courses += "<th colspan='2' align='center'><b>Intake</b></th>";
                courses += "</tr>";

                assessments += "<table border='1' cellpadding='3' cellspacing='0' width='100%' style='font-family: Times New Roman; font-size: 9px; font-weight: normal; background-color: #fff; margin: 0 auto;'>";
                assessments += "<tr>";
                assessments += "<th rowspan='2' colspan='1' align='center'><b>S.No</b></th>";
                assessments += "<th rowspan='2' colspan='5'><b>Course</b></th>";
                assessments += "<th colspan='4' align='center'><b>Faculty Shortage *</b></th>";
                assessments += "<th colspan='5' align='center'><b>Lab Shortage</b></th>";
                assessments += "</tr>";
                assessments += "<tr>";
                assessments += "<th colspan='1' align='center'><b>R</b></th>";
                assessments += "<th colspan='1' align='center'><b>A</b></th>";
                assessments += "<th colspan='1' align='center'><b>R.Ph.D</b></th>";
                assessments += "<th colspan='1' align='center'><b>A.Ph.D</b></th>";
                assessments += "<th colspan='5' align='center'><b>Name of the Lab(s)</b></th>";
                assessments += "</tr>";
                //Only Btech Affiliation Letters Filtering
                //faculty = faculty.Where(e => e.Degree == "B.Tech" || e.Degree == "B.Pharmacy" || e.Degree == "Pharm.D" || e.Degree == "Pharm.D PB").Select(e => e).ToList();
                //labs = labs.Where(e => e.Degree == "B.Tech" || e.Degree == "B.Pharmacy" || e.Degree == "Pharm.D" || e.Degree == "Pharm.D PB").Select(e => e).ToList();
                //Only MBA And MCA
                //faculty = faculty.Where(e => e.Degree == "MBA" || e.Degree == "MCA").Select(e => e).ToList();
                //labs = labs.Where(e => e.Degree == "MBA" || e.Degree == "MCA").Select(e => e).ToList();
                //only M.Tech
                faculty = faculty.Where(e => e.Degree == "M.Tech" || e.Degree == "M.Pharmacy").Select(e => e).ToList();
                labs = labs.Where(e => e.Degree == "M.Tech" || e.Degree == "M.Pharmacy").Select(e => e).ToList();

                //Add New Code for Pharma.D Labs not Add in Lab Master
                int[] Humanities = new int[] { 31, 37, 42, 48, 155, 156, 157, 158 };
                int[] FSpecIds = faculty.Where(e => !Humanities.Contains(e.SpecializationId)).Select(F => F.SpecializationId).ToArray();//
                int[] LSpecIds = labs.Select(F => F.SpecializationId).ToArray();
                int[] PharmacySpecualizationIds = new int[] { 12, 18, 13, 114, 115, 116, 117, 118, 119, 120, 121, 122, 123, 124, 19, 68, 38, 374, 26, 108, 179, 192, 335, 196, 109, 9, 134, 70, 198, 180, 367, 32, 183, 171, 11, 399 };

                var pharmacycollegesids = new[]
                {
                    24, 27, 30, 33, 34, 44, 47, 52, 55, 65, 78, 83, 95, 97, 104, 105, 107, 110, 114, 117, 118, 127, 135,
                    136, 139, 146, 150, 159, 169, 202, 204, 213, 219, 234, 239, 253,
                    262, 263, 267, 275, 283, 284, 290, 295, 297, 298, 301, 303, 314, 317, 320, 348, 353, 362, 376, 377,
                    379, 384, 392, 395, 410, 427, 428, 436, 442, 445, 448, 454, 457, 458, 6,
                    45, 54, 58, 90, 120, 206, 237, 302, 313, 318, 319, 389, 60, 66, 252, 255, 315, 370, 108, 38,223,399,9
                };
                //Some Colleges only Put Lab Deficiency NIL ,ex.College Uploaded New Courses.  B.Tech and B.pharmacy

                if (pharmacycollegesids.Contains(collegeId))
                {
                    foreach (int id in FSpecIds)
                    {
                        //if (PharmacySpecualizationIds.Contains(id))
                        // {
                        if (!LSpecIds.Contains(id))
                        {
                            CollegeFacultyLabs lab = new CollegeFacultyLabs();
                            lab.SpecializationId = id;
                            lab.Deficiency = "NIL";
                            lab.LabsDeficiency = "NIL";
                            labs.Add(lab);
                        }
                        // }
                    }
                }


                //MBA Lab Deficiency NIL 


                //foreach (int id in FSpecIds)
                //{
                //    if (id == 14)
                //    {
                //        if (!LSpecIds.Contains(id))
                //        {
                //            CollegeFacultyLabs lab = new CollegeFacultyLabs();
                //            lab.SpecializationId = id;
                //            lab.Deficiency = "NIL";
                //            lab.LabsDeficiency = "NIL";
                //            labs.Add(lab);
                //        }
                //        else
                //        {
                //            labs.Where(e => e.SpecializationId == 14).ToList().ForEach(e => e.LabsDeficiency = "NIL");
                //        }
                //    }
                //}




                clearedCourses = faculty.Join(labs, f => f.SpecializationId, l => l.SpecializationId, (f, l) => new { f, l })
                                        .Where(a => a.f.Deficiency == "NO" && a.f.PhdDeficiency != "YES" && a.l.LabsDeficiency == "NIL")
                                        .Select(a => new CollegeFacultyLabs
                                        {
                                            Degree = a.f.Degree,
                                            Department = a.f.Department,
                                            SpecializationId = a.f.SpecializationId,
                                            Specialization = a.f.Specialization,
                                            TotalIntake = a.f.TotalIntake,
                                            Required = a.f.Required,
                                            Available = a.f.Available,
                                            Deficiency = a.f.Deficiency,
                                            Requiredphd = a.f.Requiredphd,
                                            Availablephd = a.f.Availablephd,
                                            PhdDeficiency = a.f.PhdDeficiency,
                                            LabsDeficiency = a.l.LabsDeficiency
                                        })
                                        .OrderBy(a => a.DegreeDisplayOrder).ThenBy(a => a.Department).ThenBy(a => a.Specialization)
                                        .ToList();

                deficiencyCourses = faculty.Join(labs, f => f.SpecializationId, l => l.SpecializationId, (f, l) => new { f, l })
                                            .Where(a => (a.f.Deficiency == "YES" || a.f.PhdDeficiency == "YES") || a.l.LabsDeficiency != "NIL")
                                            .Select(a => new CollegeFacultyLabs
                                            {
                                                Degree = a.f.Degree,
                                                Department = a.f.Department,
                                                SpecializationId = a.f.SpecializationId,
                                                Specialization = a.f.Specialization,
                                                TotalIntake = a.f.TotalIntake,
                                                Required = a.f.Required,
                                                Available = a.f.Available,
                                                Requiredphd = a.f.Requiredphd,
                                                Availablephd = a.f.Availablephd,
                                                Deficiency = a.f.Deficiency,
                                                PhdDeficiency = a.f.PhdDeficiency,
                                                LabsDeficiency = a.l.LabsDeficiency
                                            })
                                            .OrderBy(a => a.DegreeDisplayOrder).ThenBy(a => a.Department).ThenBy(a => a.Specialization)
                                            .ToList();




                foreach (var course in deficiencyCourses)
                {
                    if (!deficiencyDepartments.Contains(course.Department))
                    {
                        deficiencyDepartments.Add(course.Department);
                    }

                    assessments += "<tr>";
                    assessments += "<td colspan='1' align='center'>" + (assessmentCount + 1) + "</td>";

                    course.Shift = "1";
                    if (course.Shift == "1")
                    {
                        assessments += "<td colspan='5' >" + course.Degree + " - " + course.Specialization + "</td>";
                    }

                    //assessments += "<td colspan='2' align='center'>" + (course.Required - course.Available) + "</td>";
                    assessments += "<td colspan='1' align='center'>" + course.Required + "</td>";
                    assessments += "<td colspan='1' align='center'>" + course.Available + "</td>";
                    //assessments += "<td colspan='2' align='center'>" + course.PhdDeficiency + "</td>";
                    if (course.Degree == "M.Pharmacy")
                    {
                        assessments += "<td colspan='1' align='center'>1</td>";
                        assessments += "<td colspan='1' align='center'>0</td>";
                    }
                    else
                    {
                        assessments += "<td colspan='1' align='center'>" + course.Requiredphd + "</td>";
                        assessments += "<td colspan='1' align='center'>" + course.Availablephd + "</td>";
                    }

                    assessments += "<td colspan='5' align='center'>" + (course.LabsDeficiency.Equals("No Equipement Uploaded") ? course.Specialization + " Lab Deficiency" : course.LabsDeficiency) + "</td>";
                    assessments += "</tr>";

                    assessmentCount++;

                    var secondshift = db.jntuh_college_intake_existing.Where(e => e.collegeId == collegeId && e.specializationId == course.SpecializationId && e.shiftId == 2).Select(e => e).FirstOrDefault();

                    if (secondshift != null)
                    {
                        assessments += "<tr>";
                        assessments += "<td colspan='1' align='center'>" + (assessmentCount + 1) + "</td>";

                        course.Shift = "2";
                        if (course.Shift == "2")
                        {
                            assessments += "<td colspan='5' >" + course.Degree + " - " + course.Specialization + " - 2</td>";
                        }

                        //assessments += "<td colspan='2' align='center'>" + (course.Required - course.Available) + "</td>";
                        assessments += "<td colspan='1' align='center'>" + course.Required + "</td>";
                        assessments += "<td colspan='1' align='center'>" + course.Available + "</td>";
                        //assessments += "<td colspan='2' align='center'>" + course.PhdDeficiency + "</td>";
                        //assessments += "<td colspan='1' align='center'>" + course.Requiredphd + "</td>";
                        //assessments += "<td colspan='1' align='center'>" + course.Availablephd + "</td>";
                        if (course.Degree == "M.Pharmacy")
                        {
                            assessments += "<td colspan='1' align='center'>1</td>";
                            assessments += "<td colspan='1' align='center'>0</td>";
                        }
                        else
                        {
                            assessments += "<td colspan='1' align='center'>" + course.Requiredphd + "</td>";
                            assessments += "<td colspan='1' align='center'>" + course.Availablephd + "</td>";
                        }
                        assessments += "<td colspan='5' align='center'>" + (course.LabsDeficiency.Equals("No Equipement Uploaded") ? course.Specialization + " Lab Deficiency" : course.LabsDeficiency) + "</td>";
                        assessments += "</tr>";

                        assessmentCount++;
                    }

                    //}
                }

                foreach (var course in clearedCourses)
                {
                    string degreeType = db.jntuh_degree.Join(db.jntuh_degree_type, d => d.degreeTypeId, t => t.id, (d, t) => new { d, t })
                                                       .Where(a => a.d.degree == course.Degree).Select(a => a.t.degreeType).FirstOrDefault();

                    List<string> clearedDepartments = clearedCourses.Select(a => a.Department).ToList(); //.Where(a => a.DegreeType == "UG")

                    if (deficiencyDepartments.Contains(course.Department) && !clearedDepartments.Contains(course.Department))
                    {
                        assessments += "<tr>";
                        assessments += "<td colspan='1' align='center'>" + (assessmentCount + 1) + "</td>";

                        course.Shift = "1";
                        if (course.Shift == "1")
                        {
                            assessments += "<td colspan='5' >" + course.Degree + " - " + course.Specialization + "</td>";
                        }

                        //assessments += "<td colspan='2' align='center'>" + (course.Required - course.Available) + "</td>";
                        assessments += "<td colspan='1' align='center'>" + course.Required + "</td>";
                        assessments += "<td colspan='1' align='center'>" + course.Available + "</td>";
                        //assessments += "<td colspan='2' align='center'>" + course.PhdDeficiency + "</td>";
                        assessments += "<td colspan='1' align='center'>" + course.Requiredphd + "</td>";
                        assessments += "<td colspan='1' align='center'>" + course.Availablephd + "</td>";
                        assessments += "<td colspan='5' align='center'>" + (course.LabsDeficiency.Equals("No Equipement Uploaded") ? course.Specialization + " Lab Deficiency" : course.LabsDeficiency) + "</td>";
                        assessments += "</tr>";

                        assessmentCount++;

                        var secondshift = db.jntuh_college_intake_existing.Where(e => e.collegeId == collegeId && e.specializationId == course.SpecializationId && e.shiftId == 2).Select(e => e).FirstOrDefault();

                        if (secondshift != null)
                        {
                            assessments += "<tr>";
                            assessments += "<td colspan='1' align='center'>" + (assessmentCount + 1) + "</td>";

                            course.Shift = "2";
                            if (course.Shift == "2")
                            {
                                assessments += "<td colspan='5' >" + course.Degree + " - " + course.Specialization + " - 2</td>";
                            }

                            //assessments += "<td colspan='2' align='center'>" + (course.Required - course.Available) + "</td>";
                            assessments += "<td colspan='1' align='center'>" + course.Required + "</td>";
                            assessments += "<td colspan='1' align='center'>" + course.Available + "</td>";
                            //assessments += "<td colspan='2' align='center'>" + course.PhdDeficiency + "</td>";
                            assessments += "<td colspan='1' align='center'>" + course.Requiredphd + "</td>";
                            assessments += "<td colspan='1' align='center'>" + course.Availablephd + "</td>";
                            assessments += "<td colspan='5' align='center'>" + (course.LabsDeficiency.Equals("No Equipement Uploaded") ? course.Specialization + " Lab Deficiency" : course.LabsDeficiency) + "</td>";
                            assessments += "</tr>";

                            assessmentCount++;
                        }

                    }
                    else
                    {
                        if (course.TotalIntake != 0)
                        {
                            courses += "<tr>";
                            courses += "<td colspan='1' align='center'>" + (rowCount + 1) + ".</td>";
                            course.Shift = "1";

                            if (course.Shift == "1")
                            {
                                courses += "<td colspan='10'>" + course.Degree + " - " + course.Specialization + "</td>";
                            }
                            if (course.Degree == "B.Pharmacy")
                            {
                                int intake1 = course.TotalIntake >= 100 ? 100 : 60;
                                courses += "<td colspan='2' align='center'>" + intake1 + "</td>";
                            }
                            else if (course.Degree == "M.Pharmacy")
                            {
                                courses += "<td colspan='2' align='center'>15</td>";
                            }
                            else
                            {
                                courses += "<td colspan='2' align='center'>" + course.TotalIntake + "</td>";
                            }
                            courses += "</tr>";

                            rowCount++;


                            var secondshift = db.jntuh_college_intake_existing.Where(e => e.collegeId == collegeId && e.specializationId == course.SpecializationId && e.shiftId == 2).Select(e => e).FirstOrDefault();

                            if (secondshift != null)
                            {
                                int presentYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
                                int AY1 = db.jntuh_academic_year.Where(a => a.actualYear == (presentYear + 1)).Select(a => a.id).FirstOrDefault();
                                int DegreeId = db.jntuh_degree.Where(e => e.degree == course.Degree.Trim()).Select(e => e.id).FirstOrDefault();
                                int approvedIntake = GetIntake(collegeId, AY1, course.SpecializationId, 2, 1, DegreeId);

                                if (approvedIntake != 0)
                                {
                                    courses += "<tr>";
                                    courses += "<td colspan='1' align='center'>" + (rowCount + 1) + ".</td>";
                                    course.Shift = "2";

                                    if (course.Shift == "2")
                                    {
                                        courses += "<td colspan='10'>" + course.Degree + " - " + course.Specialization + " - 2</td>";
                                    }

                                    courses += "<td colspan='2' align='center'>" + GetIntake(collegeId, AY1, course.SpecializationId, 2, 1, DegreeId) + "</td>";
                                    courses += "</tr>";

                                    rowCount++;
                                }
                            }
                        }
                    }
                }
                int totalCleared = clearedCourses.Count();
                int totalZeroIntake = clearedCourses.Where(c => c.TotalIntake == 0).Count();

                if (clearedCourses.Count() == 0 || totalCleared == totalZeroIntake)
                {
                    courses += "<tr>";
                    courses += "<td colspan='13' align='center'>NIL</td>";
                    courses += "</tr>";
                }

                assessments += "</table>";
                courses += "</table>";

                return courses + "$" + assessments + "$" + assessmentCount;


            }

            #endregion Pharmacy Affliation Letter End














        }

        #region  CorrigendumLetterWord Start

        public ActionResult CorrigendumLetterWord(int collegeId, string type)
        {
            if ((collegeId != null && collegeId != 0) && !string.IsNullOrEmpty(type))
            {
                int collegeID = collegeId;
                Response.ClearContent();
                Response.ClearHeaders();
                Response.Buffer = true;
                string collegeCode = db.jntuh_college.Where(c => c.isActive == true && c.id == collegeID).Select(c => c.collegeCode).FirstOrDefault();
                Response.AddHeader("content-disposition", "attachment; filename=" + collegeCode + "-CorrigendumLetter-Report-" + collegeId + ".doc");
                Response.ContentType = "application/vnd.ms-word ";
                Response.Charset = string.Empty;
                StringBuilder str = new StringBuilder();
                str.Append(Header(collegeID));
                str.Append("<br />");
                str.Append(CollegeInformation(collegeID));
                str.Append("<br />");
                str.Append(Subject(collegeID, type));
                str.Append("<br />");

                string textpdf = str.ToString();

                Response.Output.Write(str.ToString());
                Response.Flush();
                Response.End();
            }

            return View();
        }

        public string Header(int collegeID)
        {
            string header = string.Empty;
            var collegeStatus = db.jntuh_collegestatus.Where(e => e.CollegeId == collegeID).Select(e => e).FirstOrDefault();
            if (collegeStatus != null)
            {
                header += "<table border='0' cellpadding='3' cellspacing='0' width='100%' style='font-family: Times New Roman; font-size: 14px; font-weight: normal; background-color: #fff;'>";
                header += "<tr>";
                header += "<tr style='padding-right:10px;'>";
                header += "<td width='3%'></td>";
                header += "<td width='30%' style='text-align:left;color: Blue; font-size: 9px; line-height:12px;'>Web &nbsp;: www.jntuh.ac.in<br />Email : pa2registrar@jntuh.ac.in<br />Res &nbsp;: +91-40-32517275<br />Fax &nbsp;: +91-40-23158665</td>";
                header += "<td width='31%' style='text-align:çenter;'><div style='text-align:çenter;'><img src='http://10.10.10.5:75/Content/Images/new_logo.jpg' height='70' width='60'/></div></td>";
                header += "<td width='31%' style='text-align:right;'><div><img src='http://10.10.10.5:75/Content/Images/NAAC1.png' height='70' width='60' /></div></td>";
                header += "<td width='5%'></td>";
                header += "</tr>";
                header += "</table>";
                header += "<table border='0' cellpadding='3' cellspacing='0' width='100%' style='font-family: Times New Roman; font-size: 14px; font-weight: normal; background-color: #fff;'>";
                header += "<tr>";
                header += "<td align='center' style='line-height: 14px;'>";
                header += "<div style='font-size: 10px; font-weight: normal;'>PROCEEDINGS OF THE</div>";
                header += "<div style='font-size: 14px; font-weight: normal;'><b>JAWAHARLAL NEHRU TECHNOLOGICAL UNIVERSITY HYDERABAD</b></div>";
                header += "<div style='font-size: 10px; color: Blue; font-weight: normal;'>(Established by Govt. Act No. 30 of 2008)</div>";
                header += "<div style=''font-size: 11px; color: Purple; font-weight: normal;'>Kukatpally, Hyderabad – 500 085, Telangana, India</div>";
                header += "</td>";
                header += "</tr>";
                header += "</table>";
                header += "<br/>";
                header += "<table border='0' cellpadding='3' cellspacing='0' width='100%' style='font-family: Times New Roman; font-size: 14px; font-weight: normal; background-color: #fff;'>";
                header += "<tr>";
                header += "<td align='left'>";
                header += "<b>Dr. N. YADAIAH</b><br />";
                header += "<b style='font-size: 10px;'>B.E (OUCE), M.Tech (IIT KGP),Ph.D (JNTU),</b><br />";
                header += "<span style='font-size: 10px; font-weight: normal; '>SMIEEE, FIE, FIETE, MSSI, MISTE , <br /></span>";
                header += "<span style='font-size: 10px; font-weight: normal; '>Professor of EEE, &<br /></span>";
                header += "<span style='font-size: 10px; font-weight: normal; '>REGISTRAR</span>";
                header += "</td>";
                header += "</tr>";
                header += "</table>";
                header += "<table border='0' cellpadding='0' cellspacing='0' width='100%'>";
                header += "<tr><td height='2' style='max-height: 2px; text-align: center'>&nbsp;<u><b>CORRIGENDUM</b></u></td></tr>";
                header += "</table>";
            }
            return header;
        }

        public string CollegeInformation(int? collegeID)
        {
            string collegeInformation = string.Empty;
            jntuh_college college = db.jntuh_college.Where(c => c.isActive == true && c.id == collegeID).Select(c => c).FirstOrDefault();
            var collegeStatus = db.jntuh_collegestatus.Where(e => e.CollegeId == collegeID).Select(e => e).FirstOrDefault();
            if (collegeStatus != null)
            {
                jntuh_address address = db.jntuh_address.Where(a => a.collegeId == collegeID && (a.addressTye.Equals("COLLEGE"))).Select(a => a).FirstOrDefault();

                string district = string.Empty;
                if (address != null)
                {
                    district = db.jntuh_district.Find(address.districtId).districtName;
                }
                string scheduleCollegeAddress = string.Empty;
                string collegeName = db.jntuh_college.Where(c => c.collegeCode == college.collegeCode && c.isActive == true).Select(c => c.collegeName).FirstOrDefault();

                if (address != null)
                {
                    scheduleCollegeAddress = collegeName + " <b>(cc:" + college.collegeCode + ")</b>" + ",<br />" + address.address;
                    scheduleCollegeAddress = scheduleCollegeAddress + ", " + address.townOrCity;
                    scheduleCollegeAddress = scheduleCollegeAddress + ", " + address.mandal;
                    scheduleCollegeAddress = scheduleCollegeAddress + ",<br />" + district;
                    scheduleCollegeAddress = scheduleCollegeAddress + " - " + address.pincode;
                }
                scheduleCollegeAddress = scheduleCollegeAddress.ToUpper().Replace(",", ", ").Replace("(", " (").Replace(")", ") ");

                collegeInformation += "<table border='0' cellpadding='3' cellspacing='0' style='font-family: Times New Roman;width:100%; font-size: 14px; font-weight: normal; background-color: #fff; margin: 0 auto;'>";
                collegeInformation += "<tr>";
                collegeInformation += "<td style='line-height: 12px;text-align:left;width:60%;' >To<br />The Principal / Secretary /Chairman <br />" + collegeName + "<b>(cc:" + college.collegeCode + ")</b>,<br/>" + address.address + "," + address.townOrCity + "," + address.mandal + ",<br/>" + district + " - " + address.pincode + "";
                collegeInformation += "<td valign='top' style='line-height: 12px;text-align:right;width:40%;' align='right'><b>Date: 04-06-2019</b></td>";
                collegeInformation += "</tr>";
                collegeInformation += "</table>";

            }

            return collegeInformation;
        }

        public string Subject(int collegeID, string type)
        {
            string sub = string.Empty;
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int prAy =
                db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
            var collegeStatus = db.jntuh_collegestatus.Where(e => e.CollegeId == collegeID).Select(e => e).FirstOrDefault();
            if (collegeStatus != null)
            {
                string AppealDate = string.Empty;
                string Deficiencydates = string.Empty;
                string SubmittedDATE = string.Empty;

                var deficiencydates = db.jntuh_college_news.Where(d => d.collegeId == collegeID && d.title == "JNTUH - Deficiency Report for the Academic Year 2019-20-Reg.").Select(d => d).FirstOrDefault();
                var dates = db.jntuh_college_edit_status.Where(d => d.collegeId == collegeID && d.academicyearId == prAy && d.IsCollegeEditable == false).Select(d => d).FirstOrDefault();
                var Appeal = db.jntuh_appeal_college_edit_status.Where(d => d.collegeId == collegeID && d.academicyearid == prAy && d.IsCollegeEditable == false).Select(d => d).FirstOrDefault();
                if (Appeal != null)
                {
                    AppealDate = ((DateTime)Appeal.updatedOn).ToString("dd-MM-yyyy");
                }
                else
                {
                    AppealDate = "NIL";
                }

                if (dates != null)
                {
                    SubmittedDATE = ((DateTime)dates.updatedOn).ToString("dd-MM-yyyy");
                }
                else
                {
                }

                if (deficiencydates != null)
                {
                    Deficiencydates = "28-04-2019 / 29-04-2019.";
                }
                else
                {
                    Deficiencydates = "NIL";
                }

                sub += "<table border='0' cellpadding='2' cellspacing='0' width='100%' style='font-family: Times New Roman; font-size: 14px; font-weight: normal; background-color: #fff;margin: 0 auto;'>";
                sub += "<tr>";
                sub += "<td align='right' width='7%' valign='top'>Sub :</td>";
                sub += "<td valign='top'>- Corrigendum - Communication of grant of affiliation for the Academic Year 2019-20-Reg.</td>";
                sub += "</tr>";
                sub += "<tr>";
                sub += "<td align='right' width='7%' valign='top'>Ref :</td>";
                sub += "<td valign='top'>1. Your College online application dated: " + SubmittedDATE + " for grant of affiliation for theacademic year 2019-20.</td>";
                sub += "</tr>";
                sub += "<tr>";
                sub += "<td align='right' width='7%' valign='top'>&nbsp;</td>";
                sub += "<td valign='top'>2. Deficiency report Dated: " + Deficiencydates + "</td>";
                sub += "</tr>";
                sub += "<tr>";
                sub += "<td align='right' width='7%' valign='top'>&nbsp;</td>";
                sub += "<td valign='top'>3. Online appeal submitted Dated: " + AppealDate + "</td>";
                sub += "</tr>";
                sub += "<tr>";
                sub += "<td align='right' width='7%' valign='top'>&nbsp;</td>";
                sub += "<td valign='top'>4. Affiliation Order dated : 14-05-2019.</td>";
                sub += "</tr>";
                sub += "<tr>";
                sub += "<td align='right' width='7%' valign='top'>&nbsp;</td>";
                sub += "<td valign='top'>5. Corrigendum Order dated : 01-06-2019.</td>";
                sub += "</tr>";
                sub += "<tr>";
                sub += "<td align='right' width='7%' valign='top'>&nbsp;</td>";
                sub += "<td valign='top'>6. Your Letter requesting for re verification of the existing data which was not considered by the Appellate Committee.</td>";
                sub += "</tr>";
                sub += "</table>";
                sub += "<table border='0' cellpadding='2' cellspacing='0' width='100%' style='font-family: Times New Roman; font-size: 14px; font-weight: normal; background-color: #fff; margin: 0 auto;'>";
                sub += "<tr>";
                sub += "<td align='justify' valign='justify' style='line-height: 18px;'>&nbsp;After going through your letter of representation, wherein you have brought to the notice of the University, the fact that the Appellate Committee has not considered some of the data already existing while considering your application for grant of Affiliation and requested for rectification / correction of the letter of Affiliation dated <br/>14-05-2019 issued by the University. The University after verification of the records is issuing this corrigendum in partial modification to the Letter of Grant of Affiliation/ Rejection communicated to you on 14-05-2019 and the earlier letter of affiliation/rejection may be read as under to the extent indicated for the following courses.</td>";
                sub += "</tr>";
                sub += "</table>";
                sub += "<br/>";
                if (type == "All")
                {
                    // contents = contents.Replace("##COURSE_TABLE##", CollegeCoursesAll(collegeid));
                }
                else if (type == "AllClear")
                {
                    string[] courses = CollegeCoursesAllClear(collegeID).Split('$');
                    string defiencyRows = string.Empty;
                    var pharmacyids = new[] { 6, 24, 27, 30, 34, 44, 47, 52, 54, 55, 58, 60, 65, 78, 90, 95, 97, 104, 105, 107, 110, 114, 117, 118, 120, 127, 135, 136, 139, 146, 150, 159, 169, 202, 204, 206, 213, 219, 234, 237, 253, 262, 263, 267, 283, 290, 295, 297, 298, 301, 302, 303, 313, 314, 315, 317, 318, 319, 320, 348, 353, 370, 376, 379, 384, 389, 392, 395, 410, 428, 436, 442, 445 };
                    if (pharmacyids.Contains(collegeID))
                    {
                        string ZeroIndexCourses = courses[0].ToString();
                        sub += ZeroIndexCourses;
                        //contents = contents.Replace("##COURSE_TABLE##", courses[0]);
                        //contents = contents.Replace("##ASSESSMNET_TABLE##", courses[1]);
                        //contents = contents.Replace("##TwentyFive_TABLE##", string.Empty);
                        //contents = contents.Replace("##SI_TABLE##", string.Empty);
                        //defiencyRows = courses[2];
                    }
                    else
                    {
                        string ZeroIndexCourses = courses[0].ToString();
                        sub += ZeroIndexCourses;
                        //contents = contents.Replace("##COURSE_TABLE##", courses[0]);
                        //contents = contents.Replace("##ASSESSMNET_TABLE##", courses[1]);
                        //contents = contents.Replace("##TwentyFive_TABLE##", courses[2]);
                        //defiencyRows = courses[4];
                    }
                }

                string PhDShortageText = string.Empty;
                string PrincipalText = string.Empty;
                string ThreeStarText = string.Empty;
                string ThreeHasText = string.Empty;
                string EndText = string.Empty;
                string EndTwoText = string.Empty;
                string HasCoursesNoteText = string.Empty;

                var pharmacyids1 = new[] { 6, 24, 27, 30, 34, 44, 47, 52, 54, 55, 58, 60, 65, 78, 90, 95, 97, 104, 105, 107, 110, 114, 117, 118, 120, 127, 135, 136, 139, 146, 150, 159, 169, 202, 204, 206, 213, 219, 234, 237, 253, 262, 263, 267, 283, 290, 295, 297, 298, 301, 302, 303, 313, 314, 315, 317, 318, 319, 320, 348, 353, 370, 376, 379, 384, 389, 392, 395, 410, 428, 436, 442, 445 };
                if (pharmacyids1.Contains(collegeID))
                {

                    //contents = contents.Replace("##PHDSHOEARTAGE##", string.Empty);

                    var PharmacyPrincipalDefcollegeids = new[] { 54, 58, 120, 253, 263, 370 };
                    if (PharmacyPrincipalDefcollegeids.Contains(collegeID))
                    {
                        PrincipalText += "<br/><p align='justify' style='font-size:14px;'># Due to non-availability of Principal for the majority period during the Academic year 2018-19, the intake is reduced from 100 to 90 or 60 to 50 accordingly.</p>";
                    }

                    EndText += "<br/><b>R</b>- Required Faculty,<b>A</b>-Available Faculty,<b>R.Ph.D</b>-Required Ph.D Faculty,<b>A.Ph.D</b>-Available Ph.D Faculty. <br/>* Any data discrepancies may be brought to the notice of the University within two days.<br/> The faculty requirement for Pharm.D / Pharm.D(PB) is included in B.Pharmacy course.";

                    //contents = contents.Replace("##THREESTARCON##", string.Empty);
                    //contents = contents.Replace("##THREEHASCON##", string.Empty);
                    //contents = contents.Replace("##END##", "<br/><b>R</b>- Required Faculty,<b>A</b>-Available Faculty,<b>R.Ph.D</b>-Required Ph.D Faculty,<b>A.Ph.D</b>-Available Ph.D Faculty. <br/>* Any data discrepancies may be brought to the notice of the University within two days.<br/> The faculty requirement for Pharm.D / Pharm.D(PB) is included in B.Pharmacy course.");
                    //contents = contents.Replace("##ENDTWO##", string.Empty);
                    //contents = contents.Replace("##HASCOURSESNOTE##", string.Empty);
                    //contents = contents.Replace("##DOLLERCOURSESNOTE##",string.Empty);

                }
                else
                {
                    List<CollegeFacultyLabs> Courses = CollegeClearCourses(collegeID);
                    var ishashcourses = Courses.Where(r => r.ishashcourses == true).Select(s => s.ishashcourses).FirstOrDefault();
                    var isintakechange = Courses.Where(r => r.isintakechange == true).Select(s => s.isintakechange).FirstOrDefault();
                    var principal = Courses.Where(r => r.DollerCourseIntake != 0).Select(s => s.DollerCourseIntake).FirstOrDefault();
                    if (principal != 0)
                    {
                        PrincipalText += "<br /><p align='justify' style='font-size:14px;'>## Due to non- availability of Principal for the majority period during the Academic year 2018-19, the intake is reduced to 50% of the normal JNTUH sanctioned intake (subject to a maximum of 60) in the course having highest sanctioned intake and also having least admissions during previous Academic Years.</p>";
                        // contents = contents.Replace("##PRINCIPALDEF##", "<br />## Due to non- availability of Principal for the majority period during the Academic year 2018-19, the intake is reduced to 50% of the normal JNTUH sanctioned intake (subject to a maximum of 60) in the course having highest sanctioned intake and also having least admissions during previous Academic Years.");
                    }

                    if (ishashcourses == true)
                    {
                        HasCoursesNoteText += "<br /><p align='justify' style='font-size:14px;'># In case, the admitted intake is less than 25% of the JNTUH sanctioned intake in any of the applied courses in all the previous three years but the admitted intake is at least 10 or above in one of the previous three years, then such courses are considered for affiliation with a minimum intake of 60, provided they meet other requirements as per norms. However, the faculty requirement is calculated based on sanctioned intake only.</p>";
                        //contents = contents.Replace("##HASCOURSESNOTE##", "<br /># In case, the admitted intake is less than 25% of the JNTUH sanctioned intake in any of the applied courses in all the previous three years but the admitted intake is at least 10 or above in one of the previous three years, then such courses are considered for affiliation with a minimum intake of 60, provided they meet other requirements as per norms. However, the faculty requirement is calculated based on sanctioned intake only.");
                    }

                    if (isintakechange == true)
                    {
                        PhDShortageText += "<br /><p align='justify' style='font-size:13px;line-height:16px;'>* In case, Ph.D qualified faculty members requirement is not met for the proposed intake, then the proposed intake is reduced proportionately as per the Ph.D possessing faculty members.</p>";
                        //contents = contents.Replace("##PHDSHOEARTAGE##", "* In case, Ph.D qualified faculty members requirement is not met for the proposed intake, then the proposed intake is reduced proportionately as per the Ph.D possessing faculty members.");
                    }

                    //contents = contents.Replace("##DOLLERCOURSESNOTE##", "<br />$ Intake is reduced to 50% of highest of normal sanctioned course intake (subject to a maximum of 60) due to non-availability of Principal for the majority period of the academic year 2017-18.")
                    //contents = contents.Replace("##END##", "<b>R</b>- Required Faculty,<b>A</b>-Available Faculty,<b>R.Ph.D</b>-Required Ph.D Faculty,<b>A.Ph.D</b>-Available Ph.D Faculty. <br/><br/><p><b></b> II) The following Courses have either zero admissions due to non-grant of affiliation or admitted intake is less than 25% of JNTUH sanctioned intake for the previous three academic years. Hence,<b>&lsquo;No Affiliation is Granted&rsquo;</b> for these courses for the A.Y.2019-20 as per clause: 3.30 of affiliation regulations 2019-20.<br/></p>");
                    //contents = contents.Replace("##ENDTWO##", "<br/>2. Any data discrepancies may be brought to the notice of the University within two days from the date of this letter.");
                    //contents = contents.Replace("##ENDSI##", "<br/><p><b></b> III) Affiliation is not granted for the following courses due to no class work / acute faculty shortage / lab equipment shortage / no academic ambience during surprise inspection.<br/></p>");
                }

                sub += PhDShortageText;
                sub += PrincipalText;
                sub += ThreeStarText;
                sub += ThreeHasText;
                sub += EndText;
                sub += EndTwoText;
                sub += HasCoursesNoteText;
                sub += "<br/>";
                sub += "<table border='0' cellpadding='3' cellspacing='0' width='100%' style='font-family: Times New Roman; font-size: 14px; font-weight: normal; background-color: #fff; margin: 0 auto;'>";
                sub += "<tr><td align='right'><b>Sd/-</b></td></tr>";
                sub += "<tr><td align='right'><b>REGISTRAR</b></td></tr>";
                sub += "</table>";
            }
            return sub;
        }



        #endregion  CorrigendumLetterWord End
    }
}
