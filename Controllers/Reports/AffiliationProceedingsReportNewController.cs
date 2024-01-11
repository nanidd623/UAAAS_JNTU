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
using System.Data.OleDb;
using System.Web.Configuration;
using MySql.Data.MySqlClient;

namespace UAAAS.Controllers.Reports
{
    [ErrorHandling]
    public class AffiliationProceedingsNewReportController : BaseController
    {

        string xecelfilename = System.Configuration.ConfigurationManager.AppSettings["excelflename"];
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
        private bool bpharmacydeficiecny { get; set; }
        public ActionResult Index()
        {
            return View();
        }

        //deficiency letter formats 
        //[Authorize(Roles = "Admin,DataEntry,FacultyVerification,College")]
        public ActionResult AffiliationLetter(string cid, string type)
        {
            int collegeId = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(cid, WebConfigurationManager.AppSettings["CryptoKey"]));

            List<int> collegeIds = db.jntuh_college.Where(c => c.collegeCode != "WL").Select(c => c.id).ToList();

            string code = db.jntuh_college.Where(c => c.id == collegeId).Select(c => c.collegeCode).FirstOrDefault().ToUpper();

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
            //Set page size as A4
            //var pdfDoc = new Document(PageSize.A4, 40, 40, 40, 40);
            Document pdfDoc = new Document(PageSize.A4, 60, 50, 60, 60);

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

            fullPath = path + collegeCode+"-" + ECollegeid + "-AffiliationProceedings.pdf";
            PdfWriter pdfWriter = PdfWriter.GetInstance(pdfDoc, new FileStream(fullPath, FileMode.Create));
            //ITextEvents iTextEvents = new ITextEvents();
            //iTextEvents.CollegeName = "";
            //iTextEvents.CollegeCode = "";
            //iTextEvents.formType = "";
            //pdfWriter.PageEvent = iTextEvents;

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

            #region For Deficiency & Hearing Dates

            //var DefDate = "";
            //var HearingDate = "";

            //switch (collegeCode)
            //{
            //    case "EF":
            //        DefDate = "12-05-2016";
            //        HearingDate = "NIL";
            //        break;
            //    case "WH":
            //        DefDate = "17-05-2016";
            //        HearingDate = "NIL";
            //        break;
            //    case "T2":
            //        DefDate = "10-05-2016";
            //        HearingDate = "NIL";
            //        break;
            //    case "6B":
            //        DefDate = "16-05-2016";
            //        HearingDate = "NIL";
            //        break;
            //    case "BU":
            //        DefDate = "11-05-2016";
            //        HearingDate = "NIL";
            //        break;
            //    case "RR":
            //        DefDate = "18-05-2016";
            //        HearingDate = "17-06-2016";
            //        break;
            //    case "5F":
            //        DefDate = "19-05-2016";
            //        HearingDate = "NIL";
            //        break;
            //    case "45":
            //        DefDate = "12-05-2016";
            //        HearingDate = "NIL";
            //        break;
            //    case "VJ":
            //        DefDate = "11-05-2016";
            //        HearingDate = "23-06-2016";
            //        break;
            //}
            #endregion

            var dates = db.jntuh_college_edit_status.Where(d => d.collegeId == collegeid).Select(d => d).FirstOrDefault();
            contents = contents.Replace("##116Submitted_DATE##", ((DateTime)dates.updatedOn).ToString("dd-MM-yyy"));

            var Appeal = db.jntuh_appeal_college_edit_status.Where(d => d.collegeId == collegeid && d.IsCollegeEditable == false).Select(d => d).FirstOrDefault();
            if (Appeal != null && Appeal.updatedOn != null)
            {
                contents = contents.Replace("##APPLICATION_DATE##", ((DateTime)Appeal.updatedOn).ToString("dd-MM-yyy"));
            }
            else
            {
                contents = contents.Replace("##APPLICATION_DATE##", "NIL");
            }
            //Commented on 18-06-2018 by Narayana Reddy
            //var deficiencydate = db.jntuh_college_deficiencyandheringdates.Where(e => e.CollegeCode == collegeCode).Select(e => e).FirstOrDefault();
            //if (deficiencydate != null)
            //{
                //contents = contents.Replace("##DEFICIENCY_REPORT_DATE##", deficiencydate.DeficiencyReportDate != null ? deficiencydate.DeficiencyReportDate : "NIL");
                //contents = contents.Replace("##HEARING_DATE##", deficiencydate.HearingDate != null ? deficiencydate.HearingDate : "NIL");

            //}
            //else
            //{
                contents = contents.Replace("##DEFICIENCY_REPORT_DATE##", "NIL");
                contents = contents.Replace("##HEARING_DATE##", "NIL");
            //}



          //  contents = contents.Replace("##DEFICIENCY_REPORT_DATE##", DefDate);
            string AppealSubmittedMessage = "Pursuant to the communication of deficiencies you have filed an appeal for reconsideration and the University reviewed the same or re inspection was conducted. Based on the above the University has accorded affiliation to the following courses.";
            string AppealNotSubmittedMessage = "Based on the above the University has accorded affiliation to the following courses.";
          //  contents = contents.Replace("##HEARING_DATE##", HearingDate);
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

        public string CollegeCoursesAll(int collegeId)
        {
            string courses = string.Empty;

            List<CollegeIntakeExisting> collegeIntakeExisting = new List<CollegeIntakeExisting>();

            //int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);

            var degreeDetails = (from ie in db.jntuh_college_intake_existing
                                 join js in db.jntuh_specialization on ie.specializationId equals js.id
                                 join jd in db.jntuh_department on js.departmentId equals jd.id
                                 join deg in db.jntuh_degree on jd.degreeId equals deg.id
                                 join sh in db.jntuh_shift on ie.shiftId equals sh.id
                                 where (ie.collegeId == collegeId && ie.academicYearId == 3)
                                 select new
                                 {
                                     ie.approvedIntake,
                                     js.specializationName,
                                     deg.degree,
                                     sh.shiftName,
                                     deg.degreeDisplayOrder,
                                     js.id
                                 }).OrderBy(d => d.degreeDisplayOrder).ThenBy(d => d.specializationName).ThenBy(d => d.shiftName).ToList();
            courses += "<table border='1' cellpadding='3' cellspacing='0' width='100%' style='font-family: Times New Roman; font-size: 9px; font-weight: normal; background-color: #fff; margin: 0 auto;'>";
            courses += "<tr>";
            courses += "<th colspan='1' align='center'><b>S.No</b></th>";
            courses += "<th colspan='10'><b>Name of the Course(s)</b></th>";
            courses += "<th colspan='2' align='center'><b>Intake 2015-16</b></th>";
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

                courses += "<td colspan='2' align='center'>" + item.approvedIntake + "</td>";
                courses += "</tr>";

                rowCount++;
                //}

            }

            courses += "</table>";

            return courses;
        }

        public string CollegeCoursesAllClear(int collegeId)
        {
            string courses = string.Empty;
            string assessments = string.Empty;

            List<CollegeIntakeExisting> collegeIntakeExisting = new List<CollegeIntakeExisting>();

            //int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);

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
            assessments += "<th colspan='2' align='center'><b>No</b></th>";
            assessments += "<th colspan='2' align='center'><b>Deficiency of Doctorates</b></th>";
            assessments += "<th colspan='5' align='center'><b>Name of the Lab(s)</b></th>";
            assessments += "</tr>";

            int rowCount = 0; int assessmentCount = 0;

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
            List<CollegeFacultyLabs> labs = DeficienciesInLabs(collegeId);

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

            List<CollegeFacultyLabs> clearedCourses = faculty.Join(labs, f => f.SpecializationId, l => l.SpecializationId, (f, l) => new { f, l })
                                        .Where(a => a.f.Deficiency == "NO" && a.f.PhdDeficiency != "YES" && a.l.LabsDeficiency == "NIL")
                                        .Select(a => new CollegeFacultyLabs
                                        {
                                            DegreeType = a.f.DegreeType,
                                            DegreeDisplayOrder = a.f.DegreeDisplayOrder,
                                            Degree = a.f.Degree,
                                            Department = a.f.Department,
                                            SpecializationId = a.f.SpecializationId,
                                            Specialization = a.f.Specialization,
                                            TotalIntake = a.f.TotalIntake,
                                            Required = a.f.Required,
                                            Available = a.f.Available,
                                            Deficiency = a.f.Deficiency,
                                            PhdDeficiency = a.f.PhdDeficiency,
                                            LabsDeficiency = a.l.LabsDeficiency
                                        })
                                        .OrderBy(a => a.DegreeDisplayOrder).ThenBy(a => a.Department).ThenBy(a => a.Specialization)
                                        .ToList();

            List<CollegeFacultyLabs> deficiencyCourses = faculty.Join(labs, f => f.SpecializationId, l => l.SpecializationId, (f, l) => new { f, l })
                                        .Where(a => (a.f.Deficiency == "YES" || a.f.PhdDeficiency == "YES") || a.l.LabsDeficiency != "NIL")
                                        .Select(a => new CollegeFacultyLabs
                                        {
                                            DegreeType = a.f.DegreeType,
                                            DegreeDisplayOrder = a.f.DegreeDisplayOrder,
                                            Degree = a.f.Degree,
                                            Department = a.f.Department,
                                            SpecializationId = a.f.SpecializationId,
                                            Specialization = a.f.Specialization,
                                            TotalIntake = a.f.TotalIntake,
                                            Required = a.f.Required,
                                            Available = a.f.Available,
                                            Deficiency = a.f.Deficiency,
                                            PhdDeficiency = a.f.PhdDeficiency,
                                            LabsDeficiency = a.l.LabsDeficiency
                                        })
                                        .OrderBy(a => a.DegreeDisplayOrder).ThenBy(a => a.Department).ThenBy(a => a.Specialization)
                                        .ToList();
            #region
            //string[] departments = new string[] { "English", "Mathematics", "Physics", "Chemistry" };

            //var humanities = faculty.Where(f => departments.Contains(f.Department))
            //                        .Select(f => new
            //                        {
            //                            totalIntake = f.TotalIntake,
            //                            required = f.Required,
            //                            available = f.Available
            //                        }).ToList();

            //var humanitiesTotalIntake = humanities.Select(h => h.totalIntake).Sum();
            //var humanitiesTotalRequired = humanities.Select(h => h.required).Sum();
            //var humanitiesTotalAvailable = humanities.Select(h => h.available).Sum();

            //decimal hPercentage = 0;
            //hPercentage = (humanitiesTotalRequired * 10) / 100;

            //var hDeficiency = false;

            //if ((humanitiesTotalRequired - humanitiesTotalAvailable) <= hPercentage)
            //{
            //    hDeficiency = false;
            //}
            //else
            //{
            //    hDeficiency = true;
            //}
            #endregion

            int affiliatedCount = 0; int defiencyRows = 0;

            List<string> deficiencyDepartments = new List<string>();

            foreach (var course in deficiencyCourses)
            {
                if (!deficiencyDepartments.Contains(course.Department))
                {
                    deficiencyDepartments.Add(course.Department);
                }

                //FIVE percent relaxation for faculty only
                decimal percentage = 0;

                if (course.LabsDeficiency == "NIL" && (course.Degree == "B.Pharmacy" || course.Degree == "M.Pharmacy" || course.Degree == "Pharm.D" || course.Degree == "Pharm.D PB"))
                {
                    percentage = (course.Required * 5) / 100;
                    percentage = percentage > 1 ? 1 : percentage;
                    bpharmacydeficiecny = course.Deficiency != "YES";
                    if ((course.Required - course.Available) <= 1 && percentage > 0)
                    {
                        bpharmacydeficiecny = true;
                        deficiencyCourses.Where(i => i.Degree == "M.Pharmacy" && i.Available == i.Required).ToList().ForEach(i => i.Deficiency = "NO");
                        deficiencyCourses.Where(i => i.Degree == "Pharm.D" && i.Available == i.Required).ToList().ForEach(i => i.Deficiency = "NO");
                        deficiencyCourses.Where(i => i.Degree == "Pharm.D PB" && i.Available == i.Required).ToList().ForEach(i => i.Deficiency = "NO");
                    }
                }


                if (course.LabsDeficiency == "NIL" && (course.Degree == "B.Tech" || course.Degree == "M.Tech" || course.Degree == "MBA" || course.Degree == "MCA"))
                {
                    percentage = (course.Required * 10) / 100;
                    percentage = percentage > 2 ? 2 : percentage;
                    bpharmacydeficiecny = true;
                }

                int presentYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
                int AY1 = db.jntuh_academic_year.Where(a => a.actualYear == (presentYear + 1)).Select(a => a.id).FirstOrDefault();
                bool isCourseAffiliated = false;
                //Commented on 18-06-2018 by Narayana Reddy
                //var affiliation = db.jntuh_college_intake_existing_datentry2.Where(d => d.collegeId == collegeId && d.specializationId == course.SpecializationId && d.isAffiliated == true).Select(d => d).FirstOrDefault();

                //if (affiliation != null)
                //{
                //    if (affiliation.isAffiliated == true)
                //    {
                //        isCourseAffiliated = true;
                //    }
                //}

                if (((course.Required - course.Available) <= percentage && course.PhdDeficiency != "YES" && course.LabsDeficiency == "NIL" && bpharmacydeficiecny))//|| isCourseAffiliated != false
                {
                    if (course.TotalIntake != 0)
                    {
                        affiliatedCount++;

                        courses += "<tr>";
                        courses += "<td colspan='1' align='center'>" + (rowCount + 1) + ".</td>";
                        course.Shift = "1";

                        string test = "/ " + course.Deficiency + "/ " + course.LabsDeficiency + "/ " + course.Required + "/" + course.Available + "/ " + (course.Required - course.Available) + "/ " + percentage;

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
                            int approvedIntake = GetIntake(collegeId, AY1, course.SpecializationId, 2, 1);

                            if (approvedIntake != 0)
                            {
                                courses += "<tr>";
                                courses += "<td colspan='1' align='center'>" + (rowCount + 1) + ".</td>";
                                course.Shift = "2";

                                if (course.Shift == "2")
                                {
                                    courses += "<td colspan='10'>" + course.Degree + " - " + course.Specialization + " - 2</td>";
                                }

                                courses += "<td colspan='2' align='center'>" + GetIntake(collegeId, AY1, course.SpecializationId, 2, 1) + "</td>";
                                courses += "</tr>";

                                rowCount++;
                            }
                        }
                    }
                }
                else //if (isCourseAffiliated == false)
                {
                    defiencyRows++;

                    assessments += "<tr>";
                    assessments += "<td colspan='1' align='center'>" + (assessmentCount + 1) + "</td>";

                    course.Shift = "1";
                    if (course.Shift == "1")
                    {
                        assessments += "<td colspan='5' >" + course.Degree + " - " + course.Specialization + "</td>";
                    }

                    assessments += "<td colspan='2' align='center'>" + (course.Required - course.Available) + "</td>";
                    assessments += "<td colspan='2' align='center'>" + course.PhdDeficiency + "</td>";
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

                        assessments += "<td colspan='2' align='center'>" + (course.Required - course.Available) + "</td>";
                        assessments += "<td colspan='2' align='center'>" + course.PhdDeficiency + "</td>";
                        assessments += "<td colspan='5' align='center'>" + (course.LabsDeficiency.Equals("No Equipement Uploaded") ? course.Specialization + " Lab Deficiency" : course.LabsDeficiency) + "</td>";
                        assessments += "</tr>";

                        assessmentCount++;
                    }
                }

            }

            foreach (var course in clearedCourses)
            {
                string degreeType = db.jntuh_degree.Join(db.jntuh_degree_type, d => d.degreeTypeId, t => t.id, (d, t) => new { d, t })
                                                   .Where(a => a.d.degree == course.Degree).Select(a => a.t.degreeType).FirstOrDefault();

                List<string> clearedDepartments = clearedCourses.Select(a => a.Department).ToList();//Where(a => a.DegreeType == "UG").
                //degreeType != "UG" && 
                if (deficiencyDepartments.Contains(course.Department) && !clearedDepartments.Contains(course.Department))
                {
                    defiencyRows++;
                    assessments += "<tr>";
                    assessments += "<td colspan='1' align='center'>" + (assessmentCount + 1) + "</td>";

                    course.Shift = "1";
                    if (course.Shift == "1")
                    {
                        assessments += "<td colspan='5' >" + course.Degree + " - " + course.Specialization + "</td>";
                    }

                    assessments += "<td colspan='2' align='center'>" + (course.Required - course.Available) + "</td>";
                    assessments += "<td colspan='2' align='center'>" + course.PhdDeficiency + "</td>";
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

                        assessments += "<td colspan='2' align='center'>" + (course.Required - course.Available) + "</td>";
                        assessments += "<td colspan='2' align='center'>" + course.PhdDeficiency + "</td>";
                        assessments += "<td colspan='5' align='center'>" + (course.LabsDeficiency.Equals("No Equipement Uploaded") ? course.Specialization + " Lab Deficiency" : course.LabsDeficiency) + "</td>";
                        assessments += "</tr>";

                        assessmentCount++;
                    }

                }
                else
                {
                    if (course.TotalIntake != 0)
                    {
                        affiliatedCount++;
                        string test = "/ " + course.Deficiency + "/ " + course.LabsDeficiency + "/ " + course.Required + "/" + course.Available + "/ " + (course.Required - course.Available) + "/ ";

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

                            int approvedIntake = GetIntake(collegeId, AY1, course.SpecializationId, 2, 1);

                            if (approvedIntake != 0)
                            {
                                courses += "<tr>";
                                courses += "<td colspan='1' align='center'>" + (rowCount + 1) + ".</td>";
                                course.Shift = "2";

                                if (course.Shift == "2")
                                {
                                    courses += "<td colspan='10'>" + course.Degree + " - " + course.Specialization + " - 2</td>";
                                }

                                courses += "<td colspan='2' align='center'>" + GetIntake(collegeId, AY1, course.SpecializationId, 2, 1) + "</td>";
                                courses += "</tr>";

                                rowCount++;
                            }
                        }
                    }
                }
            }
            int totalCleared = clearedCourses.Count();
            int totalZeroIntake = clearedCourses.Where(c => c.TotalIntake == 0).Count();

            if (affiliatedCount == 0)
            {
                courses += "<tr>";
                courses += "<td colspan='13' align='center'>NIL</td>";
                courses += "</tr>";
            }

            

            assessments += "</table>";
            courses += "</table>";

            return courses + "$" + assessments + "$" + defiencyRows;
        }

        //public List<CollegeFacultyLabs> DeficienciesInFaculty(int? collegeID)
        //{
        //    #region Old code


        //    //string faculty = string.Empty;

        //    //faculty += "<table width='100%' border='0' cellpadding='5' cellspacing='0'>";
        //    //faculty += "<tr>";
        //    //faculty += "<td align='left'><b><u>Deficiencies in Faculty</u></b> (Department/ Specialization Wise):";
        //    //faculty += "</tr>";
        //    //faculty += "</table>";

        //    //List<CollegeFacultyWithIntakeReport> facultyCounts = collegeFaculty(collegeID).Where(c => c.shiftId == 1).ToList();

        //    //var count = facultyCounts.Count();
        //    //var distDeptcount = 1;
        //    //var deptloop = 1;
        //    //decimal departmentWiseRequiredFaculty = 0;

        //    //string[] departments = new string[] { "English", "Mathematics", "Physics", "Chemistry", "Others" };

        //    //int totalBtechFirstYearIntake = facultyCounts.Where(d => d.Degree == "B.Tech").Select(d => d.approvedIntake1).Sum();
        //    //var firstYearRequired = Math.Ceiling((double)totalBtechFirstYearIntake / 120);
        //    //int remainingFaculty = 0;
        //    //int remainingPHDFaculty = 0;

        //    //faculty += "<table width='100%' border='1' cellpadding='3' cellspacing='0' style='border-color: #ccc;'>";
        //    //faculty += "<tr>";
        //    //faculty += "<th style='text-align: center; vertical-align: top;'>SNo</th>";
        //    //faculty += "<th style='text-align: left; vertical-align: top;' >Department</th>";
        //    //faculty += "<th style='text-align: left; vertical-align: top;' >Degree</th>";
        //    //faculty += "<th style='text-align: left; vertical-align: top;' >Specialization</th>";
        //    //faculty += "<th style='text-align: center; vertical-align: top;' >Total Intake *</th>";
        //    //faculty += "<th style='text-align: center; vertical-align: top;' >Specialization Wise Faculty Required</th>";
        //    //faculty += "<th style='text-align: center; vertical-align: top;' >Specialization Wise Faculty Available</th>";
        //    //faculty += "<th style='text-align: center; vertical-align: top;' >Deficiency</th>";
        //    //faculty += "<th style='text-align: center; vertical-align: top;' >Deficiency of Ph.D faculty</th>";
        //    //faculty += "</tr>";

        //    //var degreeTypes = db.jntuh_degree_type.Select(t => t).ToList();
        //    //var degrees = db.jntuh_degree.Select(t => t).ToList();

        //    //List<CollegeFacultyLabs> lstFaculty = new List<CollegeFacultyLabs>();

        //    //foreach (var item in facultyCounts)
        //    //{
        //    //    //item.requiredFaculty = (item.requiredFaculty - item.requiredFaculty * 10 / 100);
        //    //    distDeptcount = facultyCounts.Where(d => d.Department == item.Department).Distinct().Count();

        //    //    int indexnow = facultyCounts.IndexOf(item);

        //    //    if (indexnow > 0 && facultyCounts[indexnow].Department != facultyCounts[indexnow - 1].Department)
        //    //    {
        //    //        deptloop = 1;
        //    //    }

        //    //    departmentWiseRequiredFaculty = facultyCounts.Where(d => d.Department == item.Department).Select(d => d.requiredFaculty).Sum();

        //    //    string minimumRequirementMet = string.Empty;
        //    //    int facultyShortage = 0;
        //    //    int adjustedFaculty = 0;
        //    //    int adjustedPHDFaculty = 0;

        //    //    int tFaculty = (int)Math.Ceiling(Convert.ToDecimal(item.totalFaculty));
        //    //    int rFaculty = (int)Math.Ceiling(item.requiredFaculty);

        //    //    if (departments.Contains(item.Department))
        //    //    {
        //    //        rFaculty = (int)firstYearRequired;
        //    //        departmentWiseRequiredFaculty = (int)firstYearRequired;
        //    //    }

        //    //    var degreeType = db.jntuh_degree.Where(d => d.degree == item.Degree).Select(d => d.jntuh_degree_type.degreeType).FirstOrDefault();

        //    //    if (deptloop == 1)
        //    //    {
        //    //        if (rFaculty <= tFaculty)
        //    //        {
        //    //            minimumRequirementMet = "NO";
        //    //            remainingFaculty = tFaculty - rFaculty;
        //    //            adjustedFaculty = rFaculty;
        //    //        }
        //    //        else
        //    //        {
        //    //            minimumRequirementMet = "YES";
        //    //            adjustedFaculty = tFaculty;
        //    //            facultyShortage = rFaculty - tFaculty;
        //    //        }

        //    //        remainingPHDFaculty = item.phdFaculty;

        //    //        if (remainingPHDFaculty > 0 && degreeType.Equals("PG"))
        //    //        {
        //    //            adjustedPHDFaculty = 1;
        //    //            remainingPHDFaculty = remainingPHDFaculty - 1;
        //    //        }
        //    //    }
        //    //    else
        //    //    {
        //    //        if (rFaculty <= remainingFaculty)
        //    //        {
        //    //            minimumRequirementMet = "NO";
        //    //            remainingFaculty = remainingFaculty - rFaculty;
        //    //            adjustedFaculty = rFaculty;
        //    //        }
        //    //        else
        //    //        {
        //    //            minimumRequirementMet = "YES";
        //    //            adjustedFaculty = remainingFaculty;
        //    //            facultyShortage = rFaculty - remainingFaculty;
        //    //            remainingFaculty = 0;
        //    //        }

        //    //        if (remainingPHDFaculty > 0 && degreeType.Equals("PG"))
        //    //        {
        //    //            adjustedPHDFaculty = 1;
        //    //            remainingPHDFaculty = remainingPHDFaculty - 1;
        //    //        }
        //    //    }

        //    //    faculty += "<tr>";
        //    //    faculty += "<td class='col2' style='text-align: center; vertical-align: top;width:10px' rowspan='1'>" + (facultyCounts.IndexOf(item) + 1) + "</td>";
        //    //    faculty += "<td class='col2' style='text-align: left; vertical-align: top;'>" + item.Department + "</td>";
        //    //    faculty += "<td class='col2' style='text-align: left; vertical-align: top;'>" + item.Degree + "</td>";
        //    //    faculty += "<td class='col2' style='text-align: left; vertical-align: top;'>" + item.Specialization + "</td>";

        //    //    if (departments.Contains(item.Department))
        //    //    {
        //    //        faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + totalBtechFirstYearIntake + "</td>";
        //    //        faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + Math.Ceiling(firstYearRequired) + "</td>";
        //    //    }
        //    //    else
        //    //    {
        //    //        faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + item.totalIntake + "</td>";
        //    //        faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + Math.Ceiling(item.requiredFaculty) + "</td>";
        //    //    }

        //    //    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + adjustedFaculty + "</td>";
        //    //    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + minimumRequirementMet + "</td>";

        //    //    if (adjustedPHDFaculty > 0)
        //    //    {
        //    //        faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>NO</td>";
        //    //    }
        //    //    else if (degreeType.Equals("PG"))
        //    //    {
        //    //        faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>YES</td>";
        //    //    }
        //    //    else
        //    //    {
        //    //        faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>-</td>";
        //    //    }

        //    //    faculty += "</tr>";

        //    //    CollegeFacultyLabs newFaculty = new CollegeFacultyLabs();
        //    //    int degreeTypeId = degrees.Where(d => d.degree == item.Degree).Select(d => d.degreeTypeId).FirstOrDefault();
        //    //    newFaculty.DegreeType = degreeTypes.Where(t => t.id == degreeTypeId).Select(t => t.degreeType).FirstOrDefault();
        //    //    newFaculty.DegreeDisplayOrder = (int)degrees.Where(d => d.degree == item.Degree).Select(d => d.degreeDisplayOrder).FirstOrDefault();
        //    //    newFaculty.Degree = item.Degree;
        //    //    newFaculty.Department = item.Department;
        //    //    newFaculty.Specialization = item.Specialization;
        //    //    newFaculty.SpecializationId = item.specializationId;
        //    //    newFaculty.TotalIntake = item.approvedIntake1;

        //    //    if (departments.Contains(item.Department))
        //    //    {
        //    //        //newFaculty.TotalIntake = totalBtechFirstYearIntake;
        //    //        newFaculty.Required = (int)Math.Ceiling(firstYearRequired);
        //    //        newFaculty.Available = item.totalFaculty;
        //    //    }
        //    //    else
        //    //    {
        //    //        //newFaculty.TotalIntake = item.totalIntake;
        //    //        newFaculty.Required = (int)Math.Ceiling(item.requiredFaculty);
        //    //        newFaculty.Available = adjustedFaculty;
        //    //    }

        //    //    newFaculty.Deficiency = minimumRequirementMet;

        //    //    if (adjustedPHDFaculty > 0)
        //    //    {
        //    //        newFaculty.PhdDeficiency = "NO";
        //    //    }
        //    //    else if (degreeType.Equals("PG"))
        //    //    {
        //    //        newFaculty.PhdDeficiency = "YES";
        //    //    }
        //    //    else
        //    //    {
        //    //        newFaculty.PhdDeficiency = "-";
        //    //    }

        //    //    lstFaculty.Add(newFaculty);
        //    //    deptloop++;
        //    //}

        //    //faculty += "</table>";

        //    //faculty += "<table width='100%' border='0' cellpadding='5' cellspacing='0'>";
        //    //faculty += "<tr>";
        //    //faculty += "<td align='left'>* II, III & IV Year for B.Tech; I & II Year for M.Tech";
        //    //faculty += "</tr>";
        //    //faculty += "</table>";

        //    //return lstFaculty;

        //    #endregion

        //    string faculty = string.Empty;
        //    int facultycount = 0;
        //    faculty += "<table width='100%' border='0' cellpadding='5' cellspacing='0'>";
        //    faculty += "<tr>";
        //    faculty += "<td align='left'><b><u>Deficiencies in Faculty</u></b> (Department/ Specialization Wise):";
        //    faculty += "</tr>";
        //    faculty += "</table>";

        //    List<CollegeFacultyWithIntakeReport> facultyCounts = collegeFaculty(collegeID).Where(c => c.shiftId == 1).ToList();

        //    var count = facultyCounts.Count();
        //    var distDeptcount = 1;
        //    var deptloop = 1;
        //    decimal departmentWiseRequiredFaculty = 0;

        //    string[] departments = new string[] { "English", "Mathematics", "Physics", "Chemistry", "Others" };

        //    int totalBtechFirstYearIntake = facultyCounts.Where(d => d.Degree == "B.Tech").Select(d => d.approvedIntake1).Sum();
        //    var degrees = db.jntuh_degree.ToList();
        //    var firstYearRequired = Math.Ceiling((double)totalBtechFirstYearIntake / 120);
        //    int remainingFaculty = 0;
        //    int remainingPHDFaculty = 0;
        //    int SpecializationwisePHDFaculty = 0;
        //    int SpecializationwisePGFaculty = 0;
        //    int TotalCount = 0;

        //    faculty += "<table width='100%' border='1' cellpadding='3' cellspacing='0' style='border-color: #ccc;'>";
        //    faculty += "<tr>";
        //    faculty += "<th style='text-align: center; vertical-align: top;'>SNo</th>";
        //    faculty += "<th style='text-align: left; vertical-align: top;' >Department</th>";
        //    faculty += "<th style='text-align: left; vertical-align: top;' >Degree</th>";
        //    faculty += "<th style='text-align: left; vertical-align: top;' >Specialization</th>";
        //    faculty += "<th style='text-align: center; vertical-align: top;' >Total Intake *</th>";
        //    faculty += "<th style='text-align: center; vertical-align: top;' >Specialization Wise Faculty Required</th>";
        //    faculty += "<th style='text-align: center; vertical-align: top;' >Specialization Wise Faculty Available</th>";
        //    faculty += "<th style='text-align: center; vertical-align: top;' >PG Specialization</th>";
        //    faculty += "<th style='text-align: center; vertical-align: top;' >Deficiency</th>";
        //    faculty += "<th style='text-align: center; vertical-align: top;' >Deficiency of Ph.D faculty</th>";
        //    faculty += "</tr>";

        //    var degreeTypes = db.jntuh_degree_type.Select(t => t).ToList();

        //    List<CollegeFacultyLabs> lstFaculty = new List<CollegeFacultyLabs>();
        //    int[] SpecializationIDS = db.jntuh_specialization.Where(S => S.departmentId == 43).Select(S => S.id).ToArray();
        //    int remainingFaculty2 = 0;
        //    foreach (var item in facultyCounts)
        //    {
        //        distDeptcount = facultyCounts.Where(d => d.Department == item.Department).Distinct().Count();
        //        if (item.Degree == "M.Tech" || item.Degree == "B.Tech")
        //            SpecializationwisePHDFaculty = facultyCounts.Where(D => D.Department == item.Department && D.Degree == "M.Tech").Distinct().Count();
        //        else if (item.Degree == "MCA")
        //            SpecializationwisePHDFaculty = facultyCounts.Where(D => D.Department == item.Department && D.Degree == "MCA").Distinct().Count();
        //        else if (item.Degree == "MBA")
        //            SpecializationwisePHDFaculty = facultyCounts.Where(D => D.Department == item.Department && D.Degree == "MBA").Distinct().Count();
        //        TotalCount = facultyCounts.Where(D => D.Department == item.Department && (D.Degree == "M.Tech" || D.Degree == "B.Tech")).Distinct().Count();
        //        SpecializationwisePHDFaculty = SpecializationwisePHDFaculty * 2;


        //        if (SpecializationIDS.Contains(item.specializationId))
        //        {
        //            int SpecializationwisePGFaculty1 = facultyCounts.Where(S => S.specializationId == item.specializationId).Count();
        //            SpecializationwisePGFaculty = facultyCounts.Where(S => S.specializationId == item.specializationId).Select(S => S.SpecializationspgFaculty).FirstOrDefault();

        //        }
        //        int indexnow = facultyCounts.IndexOf(item);


        //        if (indexnow > 0 && facultyCounts[indexnow].Department != facultyCounts[indexnow - 1].Department)
        //        {
        //            deptloop = 1;
        //        }

        //        departmentWiseRequiredFaculty = facultyCounts.Where(d => d.Department == item.Department).Select(d => d.requiredFaculty).Sum();

        //        string minimumRequirementMet = string.Empty;
        //        int facultyShortage = 0;
        //        int adjustedFaculty = 0;
        //        int adjustedPHDFaculty = 0;
        //        int tFaculty = 0;
        //        if (item.Department == "MBA" || item.Department == "MCA")
        //            tFaculty = (int)Math.Ceiling(Convert.ToDecimal(item.totalFaculty));//item.totalFaculty
        //        else
        //            tFaculty = (int)Math.Ceiling(Convert.ToDecimal(item.specializationWiseFaculty));//item.totalFaculty
        //        int rFaculty = (int)Math.Ceiling(item.requiredFaculty);

        //        if (departments.Contains(item.Department))
        //        {
        //            rFaculty = (int)firstYearRequired;
        //            departmentWiseRequiredFaculty = (int)firstYearRequired;
        //        }

        //        var degreeType = degrees.Where(d => d.degree == item.Degree).Select(d => d.jntuh_degree_type.degreeType).FirstOrDefault();

        //        if (deptloop == 1)
        //        {
        //            if (rFaculty <= tFaculty)
        //            {
        //                minimumRequirementMet = "YES";
        //                remainingFaculty = tFaculty - rFaculty;
        //                adjustedFaculty = rFaculty;
        //            }
        //            else
        //            {
        //                minimumRequirementMet = "NO";
        //                adjustedFaculty = tFaculty;
        //                facultyShortage = rFaculty - tFaculty;
        //            }

        //            remainingPHDFaculty = item.phdFaculty;

        //            //if (remainingPHDFaculty > 2 && degreeType.Equals("PG"))
        //            //{
        //            //    adjustedPHDFaculty = 1;
        //            //    remainingPHDFaculty = remainingPHDFaculty - 1;
        //            //}
        //            if (remainingPHDFaculty >= 2 && (degreeType.Equals("PG")))
        //            {
        //                adjustedPHDFaculty = remainingPHDFaculty;
        //                remainingPHDFaculty = remainingPHDFaculty - 1;
        //            }
        //            else if (remainingPHDFaculty <= 0 && (degreeType.Equals("PG")))
        //            {
        //                adjustedPHDFaculty = remainingPHDFaculty;
        //                remainingPHDFaculty = remainingPHDFaculty - 1;
        //            }
        //        }
        //        else
        //        {
        //            if (rFaculty <= remainingFaculty)
        //            {
        //                minimumRequirementMet = "YES";
        //                //remainingFaculty = remainingFaculty - rFaculty;
        //                //adjustedFaculty = rFaculty;
        //                if (rFaculty <= item.specializationWiseFaculty)
        //                {
        //                    remainingFaculty = remainingFaculty - rFaculty;
        //                    adjustedFaculty = rFaculty;
        //                }

        //                else if (rFaculty >= item.specializationWiseFaculty)
        //                {
        //                    remainingFaculty = remainingFaculty - item.specializationWiseFaculty;
        //                    adjustedFaculty = item.specializationWiseFaculty;
        //                }
        //            }
        //            else
        //            {
        //                minimumRequirementMet = "NO";
        //                adjustedFaculty = remainingFaculty;
        //                facultyShortage = rFaculty - remainingFaculty;
        //                remainingFaculty = 0;
        //            }

        //            //if (remainingPHDFaculty > 0 && degreeType.Equals("PG"))
        //            //{
        //            //    adjustedPHDFaculty = 1;
        //            //    remainingPHDFaculty = remainingPHDFaculty - 1;
        //            //}
        //            remainingPHDFaculty = item.phdFaculty;
        //            if (remainingPHDFaculty >= 2 && (degreeType.Equals("PG")))
        //            {

        //                adjustedPHDFaculty = remainingPHDFaculty;
        //                remainingPHDFaculty = remainingPHDFaculty - 1;
        //            }
        //            else if (remainingPHDFaculty <= 0 && (degreeType.Equals("PG")))
        //            {

        //                adjustedPHDFaculty = remainingPHDFaculty;

        //                remainingPHDFaculty = remainingPHDFaculty - 1;
        //            }


        //        }

        //        faculty += "<tr>";
        //        faculty += "<td class='col2' style='text-align: center; vertical-align: top;width:10px' rowspan='1'>" + (facultyCounts.IndexOf(item) + 1) + "</td>";
        //        faculty += "<td class='col2' style='text-align: left; vertical-align: top;'>" + item.Department + "</td>";
        //        faculty += "<td class='col2' style='text-align: left; vertical-align: top;'>" + item.Degree + "</td>";
        //        faculty += "<td class='col2' style='text-align: left; vertical-align: top;'>" + item.Specialization + "</td>";

        //        if (departments.Contains(item.Department))
        //        {
        //            faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + totalBtechFirstYearIntake + "</td>";
        //            faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + Math.Ceiling(firstYearRequired) + "</td>";
        //        }
        //        else
        //        {
        //            faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + item.totalIntake + "</td>";
        //            faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + Math.Ceiling(item.requiredFaculty) + "</td>";
        //        }
        //        facultycount = facultycount + item.specializationWiseFaculty;
        //        if (adjustedFaculty > 0)
        //            adjustedFaculty = adjustedFaculty;
        //        else
        //            adjustedFaculty = 0;
        //        faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + adjustedFaculty + "</td>";

        //        if (item.Degree == "M.Tech")
        //            faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + item.SpecializationspgFaculty + "</td>";
        //        else
        //            faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + "" + "</td>";



        //        if (minimumRequirementMet == "YES")
        //        {
        //            if (rFaculty <= adjustedFaculty)
        //                minimumRequirementMet = "NO";
        //            else
        //                minimumRequirementMet = "YES";
        //        }

        //        else if (minimumRequirementMet == "NO")
        //        {
        //            if (rFaculty == adjustedFaculty)
        //                minimumRequirementMet = "NO";
        //            else
        //                minimumRequirementMet = "YES";
        //        }
        //        faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + minimumRequirementMet + "</td>";

        //        if (adjustedPHDFaculty >= 2 && degreeType.Equals("PG"))
        //        {
        //            faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>NO</td>";
        //        }
        //        else if (degreeType.Equals("PG"))
        //        {
        //            faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>YES</td>";
        //        }
        //        else
        //        {
        //            faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>-</td>";
        //        }

        //        faculty += "</tr>";

        //        CollegeFacultyLabs newFaculty = new CollegeFacultyLabs();
        //        int degreeTypeId = degrees.Where(d => d.degree == item.Degree).Select(d => d.degreeTypeId).FirstOrDefault();
        //        newFaculty.DegreeType = degreeTypes.Where(t => t.id == degreeTypeId).Select(t => t.degreeType).FirstOrDefault();
        //        newFaculty.DegreeDisplayOrder = (int)degrees.Where(d => d.degree == item.Degree).Select(d => d.degreeDisplayOrder).FirstOrDefault();
        //        newFaculty.Degree = item.Degree;
        //        newFaculty.Department = item.Department;
        //        newFaculty.Specialization = item.Specialization;
        //        newFaculty.SpecializationId = item.specializationId;
        //        newFaculty.TotalIntake = item.approvedIntake1;

        //        if (departments.Contains(item.Department))
        //        {
        //            //newFaculty.TotalIntake = totalBtechFirstYearIntake;
        //            newFaculty.Required = (int)Math.Ceiling(firstYearRequired);
        //        }
        //        else
        //        {
        //            // newFaculty.TotalIntake = item.totalIntake;
        //            newFaculty.Required = (int)Math.Ceiling(item.requiredFaculty);
        //        }

        //        newFaculty.Available = adjustedFaculty;
        //        if (item.Degree == "M.Tech")
        //        {
        //            newFaculty.Available = item.SpecializationspgFaculty;
        //        }
        //        newFaculty.Deficiency = minimumRequirementMet;

        //        if (adjustedPHDFaculty >= 2 && degreeType.Equals("PG"))
        //        {
        //            newFaculty.PhdDeficiency = "NO";
        //        }
        //        else if (degreeType.Equals("PG"))
        //        {
        //            newFaculty.PhdDeficiency = "YES";
        //        }
        //        else
        //        {
        //            newFaculty.PhdDeficiency = "-";
        //        }

        //        lstFaculty.Add(newFaculty);
        //        deptloop++;
        //    }

        //    faculty += "</table>";

        //    faculty += "<table width='100%' border='0' cellpadding='5' cellspacing='0'>";
        //    faculty += "<tr>";
        //    faculty += "<td align='left'>* II, III & IV Year for B.Tech; I & II Year for M.Tech";
        //    faculty += "</tr>";
        //    faculty += "</table>";

        //    return lstFaculty;



        //}

        public List<CollegeFacultyLabs> DeficienciesInFaculty(int? collegeID)
        {
            #region Old code


            //string faculty = string.Empty;

            //faculty += "<table width='100%' border='0' cellpadding='5' cellspacing='0'>";
            //faculty += "<tr>";
            //faculty += "<td align='left'><b><u>Deficiencies in Faculty</u></b> (Department/ Specialization Wise):";
            //faculty += "</tr>";
            //faculty += "</table>";

            //List<CollegeFacultyWithIntakeReport> facultyCounts = collegeFaculty(collegeID).Where(c => c.shiftId == 1).ToList();

            //var count = facultyCounts.Count();
            //var distDeptcount = 1;
            //var deptloop = 1;
            //decimal departmentWiseRequiredFaculty = 0;

            //string[] departments = new string[] { "English", "Mathematics", "Physics", "Chemistry", "Others" };

            //int totalBtechFirstYearIntake = facultyCounts.Where(d => d.Degree == "B.Tech").Select(d => d.approvedIntake1).Sum();
            //var firstYearRequired = Math.Ceiling((double)totalBtechFirstYearIntake / 120);
            //int remainingFaculty = 0;
            //int remainingPHDFaculty = 0;

            //faculty += "<table width='100%' border='1' cellpadding='3' cellspacing='0' style='border-color: #ccc;'>";
            //faculty += "<tr>";
            //faculty += "<th style='text-align: center; vertical-align: top;'>SNo</th>";
            //faculty += "<th style='text-align: left; vertical-align: top;' >Department</th>";
            //faculty += "<th style='text-align: left; vertical-align: top;' >Degree</th>";
            //faculty += "<th style='text-align: left; vertical-align: top;' >Specialization</th>";
            //faculty += "<th style='text-align: center; vertical-align: top;' >Total Intake *</th>";
            //faculty += "<th style='text-align: center; vertical-align: top;' >Specialization Wise Faculty Required</th>";
            //faculty += "<th style='text-align: center; vertical-align: top;' >Specialization Wise Faculty Available</th>";
            //faculty += "<th style='text-align: center; vertical-align: top;' >Deficiency</th>";
            //faculty += "<th style='text-align: center; vertical-align: top;' >Deficiency of Ph.D faculty</th>";
            //faculty += "</tr>";

            //var degreeTypes = db.jntuh_degree_type.Select(t => t).ToList();
            //var degrees = db.jntuh_degree.Select(t => t).ToList();

            //List<CollegeFacultyLabs> lstFaculty = new List<CollegeFacultyLabs>();

            //foreach (var item in facultyCounts)
            //{
            //    //item.requiredFaculty = (item.requiredFaculty - item.requiredFaculty * 10 / 100);
            //    distDeptcount = facultyCounts.Where(d => d.Department == item.Department).Distinct().Count();

            //    int indexnow = facultyCounts.IndexOf(item);

            //    if (indexnow > 0 && facultyCounts[indexnow].Department != facultyCounts[indexnow - 1].Department)
            //    {
            //        deptloop = 1;
            //    }

            //    departmentWiseRequiredFaculty = facultyCounts.Where(d => d.Department == item.Department).Select(d => d.requiredFaculty).Sum();

            //    string minimumRequirementMet = string.Empty;
            //    int facultyShortage = 0;
            //    int adjustedFaculty = 0;
            //    int adjustedPHDFaculty = 0;

            //    int tFaculty = (int)Math.Ceiling(Convert.ToDecimal(item.totalFaculty));
            //    int rFaculty = (int)Math.Ceiling(item.requiredFaculty);

            //    if (departments.Contains(item.Department))
            //    {
            //        rFaculty = (int)firstYearRequired;
            //        departmentWiseRequiredFaculty = (int)firstYearRequired;
            //    }

            //    var degreeType = db.jntuh_degree.Where(d => d.degree == item.Degree).Select(d => d.jntuh_degree_type.degreeType).FirstOrDefault();

            //    if (deptloop == 1)
            //    {
            //        if (rFaculty <= tFaculty)
            //        {
            //            minimumRequirementMet = "NO";
            //            remainingFaculty = tFaculty - rFaculty;
            //            adjustedFaculty = rFaculty;
            //        }
            //        else
            //        {
            //            minimumRequirementMet = "YES";
            //            adjustedFaculty = tFaculty;
            //            facultyShortage = rFaculty - tFaculty;
            //        }

            //        remainingPHDFaculty = item.phdFaculty;

            //        if (remainingPHDFaculty > 0 && degreeType.Equals("PG"))
            //        {
            //            adjustedPHDFaculty = 1;
            //            remainingPHDFaculty = remainingPHDFaculty - 1;
            //        }
            //    }
            //    else
            //    {
            //        if (rFaculty <= remainingFaculty)
            //        {
            //            minimumRequirementMet = "NO";
            //            remainingFaculty = remainingFaculty - rFaculty;
            //            adjustedFaculty = rFaculty;
            //        }
            //        else
            //        {
            //            minimumRequirementMet = "YES";
            //            adjustedFaculty = remainingFaculty;
            //            facultyShortage = rFaculty - remainingFaculty;
            //            remainingFaculty = 0;
            //        }

            //        if (remainingPHDFaculty > 0 && degreeType.Equals("PG"))
            //        {
            //            adjustedPHDFaculty = 1;
            //            remainingPHDFaculty = remainingPHDFaculty - 1;
            //        }
            //    }

            //    faculty += "<tr>";
            //    faculty += "<td class='col2' style='text-align: center; vertical-align: top;width:10px' rowspan='1'>" + (facultyCounts.IndexOf(item) + 1) + "</td>";
            //    faculty += "<td class='col2' style='text-align: left; vertical-align: top;'>" + item.Department + "</td>";
            //    faculty += "<td class='col2' style='text-align: left; vertical-align: top;'>" + item.Degree + "</td>";
            //    faculty += "<td class='col2' style='text-align: left; vertical-align: top;'>" + item.Specialization + "</td>";

            //    if (departments.Contains(item.Department))
            //    {
            //        faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + totalBtechFirstYearIntake + "</td>";
            //        faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + Math.Ceiling(firstYearRequired) + "</td>";
            //    }
            //    else
            //    {
            //        faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + item.totalIntake + "</td>";
            //        faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + Math.Ceiling(item.requiredFaculty) + "</td>";
            //    }

            //    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + adjustedFaculty + "</td>";
            //    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + minimumRequirementMet + "</td>";

            //    if (adjustedPHDFaculty > 0)
            //    {
            //        faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>NO</td>";
            //    }
            //    else if (degreeType.Equals("PG"))
            //    {
            //        faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>YES</td>";
            //    }
            //    else
            //    {
            //        faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>-</td>";
            //    }

            //    faculty += "</tr>";

            //    CollegeFacultyLabs newFaculty = new CollegeFacultyLabs();
            //    int degreeTypeId = degrees.Where(d => d.degree == item.Degree).Select(d => d.degreeTypeId).FirstOrDefault();
            //    newFaculty.DegreeType = degreeTypes.Where(t => t.id == degreeTypeId).Select(t => t.degreeType).FirstOrDefault();
            //    newFaculty.DegreeDisplayOrder = (int)degrees.Where(d => d.degree == item.Degree).Select(d => d.degreeDisplayOrder).FirstOrDefault();
            //    newFaculty.Degree = item.Degree;
            //    newFaculty.Department = item.Department;
            //    newFaculty.Specialization = item.Specialization;
            //    newFaculty.SpecializationId = item.specializationId;
            //    newFaculty.TotalIntake = item.approvedIntake1;

            //    if (departments.Contains(item.Department))
            //    {
            //        //newFaculty.TotalIntake = totalBtechFirstYearIntake;
            //        newFaculty.Required = (int)Math.Ceiling(firstYearRequired);
            //        newFaculty.Available = item.totalFaculty;
            //    }
            //    else
            //    {
            //        //newFaculty.TotalIntake = item.totalIntake;
            //        newFaculty.Required = (int)Math.Ceiling(item.requiredFaculty);
            //        newFaculty.Available = adjustedFaculty;
            //    }

            //    newFaculty.Deficiency = minimumRequirementMet;

            //    if (adjustedPHDFaculty > 0)
            //    {
            //        newFaculty.PhdDeficiency = "NO";
            //    }
            //    else if (degreeType.Equals("PG"))
            //    {
            //        newFaculty.PhdDeficiency = "YES";
            //    }
            //    else
            //    {
            //        newFaculty.PhdDeficiency = "-";
            //    }

            //    lstFaculty.Add(newFaculty);
            //    deptloop++;
            //}

            //faculty += "</table>";

            //faculty += "<table width='100%' border='0' cellpadding='5' cellspacing='0'>";
            //faculty += "<tr>";
            //faculty += "<td align='left'>* II, III & IV Year for B.Tech; I & II Year for M.Tech";
            //faculty += "</tr>";
            //faculty += "</table>";

            //return lstFaculty;

            #endregion

            string faculty = string.Empty;
            int facultycount = 0;
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
            var degrees = db.jntuh_degree.ToList();
            var firstYearRequired = Math.Ceiling((double)totalBtechFirstYearIntake / 120);
            int remainingFaculty = 0;
            int remainingPHDFaculty = 0;
            int SpecializationwisePHDFaculty = 0;
            int SpecializationwisePGFaculty = 0;
            int TotalCount = 0;

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
                int facultyShortage = 0;
                int adjustedFaculty = 0;
                int adjustedPHDFaculty = 0;
                int tFaculty = 0;
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
                        remainingFaculty = tFaculty - rFaculty;
                        adjustedFaculty = rFaculty;
                    }
                    else
                    {
                        minimumRequirementMet = "NO";
                        adjustedFaculty = tFaculty;
                        facultyShortage = rFaculty - tFaculty;
                    }

                    remainingPHDFaculty = item.phdFaculty;

                    //if (remainingPHDFaculty > 2 && degreeType.Equals("PG"))
                    //{
                    //    adjustedPHDFaculty = 1;
                    //    remainingPHDFaculty = remainingPHDFaculty - 1;
                    //}
                    if (remainingPHDFaculty >= 1 && (degreeType.Equals("PG")) && rFaculty <= adjustedFaculty && item.Degree != "MBA")
                    {
                        adjustedPHDFaculty = 1; //remainingPHDFaculty;
                        remainingPHDFaculty = remainingPHDFaculty - 1;
                       // facultyCounts.Where(i => i.Department == item.Department).ToList().ForEach(I => I.phdFaculty = remainingPHDFaculty);
                    }
                    else if (remainingPHDFaculty >= 1 && (degreeType.Equals("PG")) && rFaculty <= adjustedFaculty && item.Degree == "MBA")
                    {
                        if (item.totalIntake > 120)
                            adjustedPHDFaculty = remainingPHDFaculty;
                        else
                            adjustedPHDFaculty = 1;
                    }
                    else if (remainingPHDFaculty >= 1 && (degreeType.Equals("PG")) && (rFaculty - adjustedFaculty == 1) && item.Degree == "MBA")
                    {
                        if (item.totalIntake > 120)
                            adjustedPHDFaculty = remainingPHDFaculty;
                        else
                            adjustedPHDFaculty = 1;
                    }
                    else if (remainingPHDFaculty < 1 && (degreeType.Equals("PG")) && remainingPHDFaculty > 0)
                    {
                        adjustedPHDFaculty = remainingPHDFaculty;
                        // remainingPHDFaculty = remainingPHDFaculty - 1;
                    }
                }
                else
                {
                    if (rFaculty <= remainingFaculty)
                    {
                        minimumRequirementMet = "YES";
                        //remainingFaculty = remainingFaculty - rFaculty;
                        //adjustedFaculty = rFaculty;
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
                        adjustedFaculty = remainingFaculty;
                        facultyShortage = rFaculty - remainingFaculty;
                        remainingFaculty = 0;
                    }

                    //if (remainingPHDFaculty > 0 && degreeType.Equals("PG"))
                    //{
                    //    adjustedPHDFaculty = 1;
                    //    remainingPHDFaculty = remainingPHDFaculty - 1;
                    //}
                    remainingPHDFaculty = item.phdFaculty;
                    if (remainingPHDFaculty >= 1 && (degreeType.Equals("PG")) && rFaculty <= adjustedFaculty && item.Degree != "MBA")
                    {
                        adjustedPHDFaculty = 1; //remainingPHDFaculty;
                        remainingPHDFaculty = remainingPHDFaculty - 1;
                        //facultyCounts.Where(i => i.Department == item.Department).ToList().ForEach(I => I.phdFaculty = remainingPHDFaculty);
                    }
                    else if (remainingPHDFaculty >= 1 && (degreeType.Equals("PG")) && rFaculty <= adjustedFaculty && item.Degree == "MBA")
                    {
                        if (item.totalIntake > 120)
                            adjustedPHDFaculty = remainingPHDFaculty;
                        else
                            adjustedPHDFaculty = 1;
                    }
                    else if (remainingPHDFaculty >= 1 && (degreeType.Equals("PG")) && (rFaculty - adjustedFaculty == 1) && item.Degree == "MBA")
                    {
                        if (item.totalIntake > 120)
                            adjustedPHDFaculty = remainingPHDFaculty;
                        else
                            adjustedPHDFaculty = 1;
                    }
                    else if (remainingPHDFaculty < 1 && (degreeType.Equals("PG")) && remainingPHDFaculty > 0)
                    {

                        adjustedPHDFaculty = remainingPHDFaculty;

                        // remainingPHDFaculty = remainingPHDFaculty - 1;
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
                facultycount = facultycount + item.specializationWiseFaculty;
                if (adjustedFaculty > 0)
                    adjustedFaculty = adjustedFaculty;
                else
                    adjustedFaculty = 0;
                faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + adjustedFaculty + "</td>";

                if (item.Degree == "M.Tech")
                    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + item.SpecializationspgFaculty + "</td>";
                else
                    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + "" + "</td>";



                if (minimumRequirementMet == "YES")
                {
                    if (rFaculty <= adjustedFaculty)
                        minimumRequirementMet = "NO";
                    else
                        minimumRequirementMet = "YES";
                }

                else if (minimumRequirementMet == "NO")
                {
                    if (rFaculty == adjustedFaculty)
                        minimumRequirementMet = "NO";
                    else
                        minimumRequirementMet = "YES";
                }
                faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + minimumRequirementMet + "</td>";

                if (adjustedPHDFaculty >= 2 && degreeType.Equals("PG"))
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

                if (adjustedPHDFaculty >= 1 && degreeType.Equals("PG") && item.Degree != "MBA")
                {
                    newFaculty.PhdDeficiency = "NO";
                }
                else if (adjustedPHDFaculty >=1 && degreeType.Equals("PG") && item.Degree == "MBA")
                {
                    if (item.totalIntake > 120)
                    {
                        if (adjustedPHDFaculty >= 2)
                            newFaculty.PhdDeficiency = "NO";
                        else
                            newFaculty.PhdDeficiency = "YES";
                    }
                    else
                    {
                        if (adjustedPHDFaculty >= 1)
                            newFaculty.PhdDeficiency = "NO";
                        else
                            newFaculty.PhdDeficiency = "YES";
                    }
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




        public List<CollegeFacultyLabs> DeficienciesInLabs(int? collegeID)
        {
            string labs = string.Empty;

            labs += "<table width='100%' border='0' cellpadding='5' cellspacing='0'>";
            labs += "<tr>";
            labs += "<td align='left'><b><u>Deficiencies in Laboratory</u></b> (Department/ Specialization Wise):";
            labs += "</tr>";
            labs += "</table>";

            List<Lab> labsCount = collegeLabs(collegeID);

            var deficiencies = labsCount.GroupBy(l => new { l.degree, l.department, l.specializationName, l.specializationId })
                                        .Select(l => new { degree = l.Key.degree, department = l.Key.department, specializationid = l.Key.specializationId, specializationName = l.Key.specializationName, deficiencies = string.Empty })
                                        .ToList();

            labs += "<table width='100%' border='1' cellpadding='3' cellspacing='0' style='border-color: #ccc;'>";
            labs += "<tr>";
            labs += "<th style='text-align: center; width: 5%;'>S.No</th>";
            labs += "<th style='text-align: left; width: 10%;'>Degree</th>";
            labs += "<th style='text-align: left; width: 10%;'>Department</th>";
            labs += "<th style='text-align: left; width: 20%;'>Specialization</th>";
            labs += "<th style='text-align: center; '>Names of the Labs with Deficiency (Details Annexed)</th>";
            labs += "</tr>";

            var labMaster = db.jntuh_lab_master.ToList();
            var collegeLabMaster = db.jntuh_college_laboratories_dataentry2.Where(l => l.CollegeID == collegeID).ToList();
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
                    .Select(l => new { Deficiency = l.year + "-" + l.Semester + "-" + l.Labcode.Replace("-", "$") + "-" + l.specializationId }).Select(l => l.Deficiency).ToList();//degreeType.Equals("PG") ? "No Equipement Uploaded" :

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
                        int availableCount = collegeLabMaster.Where(m => requiredLabs.Contains(m.EquipmentID)).Count();

                        int[] labmasterids = labMaster.Where(m => m.SpecializationID == specializationid && m.Year == year && m.Semester == semester && m.Labcode == labCode).Select(m => m.id).Distinct().ToArray();
                        int[] collegelabequipmentids = jntuh_college_laboratories.Where(i => labmasterids.Contains(i.EquipmentID) && i.EquipmentNo == 1).Select(i => i.id).Distinct().ToArray();

                        if (requiredCount > availableCount && labmasterids.Count() != collegelabequipmentids.Count() && labCode != "14LAB")
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

        public List<CollegeFacultyWithIntakeReport> collegeFaculty(int? collegeId)
        {

            #region old code
            //List<CollegeIntakeExisting> collegeIntakeExisting = new List<CollegeIntakeExisting>();
            //List<CollegeFacultyWithIntakeReport> intakedetailsList = new List<CollegeFacultyWithIntakeReport>();

            //if (collegeId != null)
            //{
            //    var jntuh_college_faculty_deficiency = db.jntuh_college_faculty_deficiency.Where(fd => fd.CollegeId == collegeId).ToList();
            //    var jntuh_specialization = db.jntuh_specialization.ToList();

            //    int[] collegeIDs = null;
            //    int facultystudentRatio = 0;
            //    decimal facultyRatio = 0m;
            //    if (collegeId != 0)
            //    {
            //        collegeIDs = db.jntuh_college.Where(c => c.isActive == true && c.id == collegeId).Select(c => c.id).ToArray();
            //    }
            //    else
            //    {
            //        collegeIDs = db.jntuh_college.Where(c => c.isActive == true).Select(c => c.id).ToArray();
            //    }
            //    var jntuh_academic_year = db.jntuh_academic_year.ToList();
            //    var jntuh_college = db.jntuh_college.Where(c => c.isActive == true).ToList();
            //    var jntuh_faculty_student_ratio_norms = db.jntuh_faculty_student_ratio_norms.Where(f => f.isActive == true).ToList();
            //    var jntuh_degree = db.jntuh_degree.ToList();

            //    int presentYear = jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            //    int AY1 = jntuh_academic_year.Where(a => a.actualYear == (presentYear + 1)).Select(a => a.id).FirstOrDefault();
            //    int AY2 = jntuh_academic_year.Where(a => a.actualYear == (presentYear)).Select(a => a.id).FirstOrDefault();
            //    int AY3 = jntuh_academic_year.Where(a => a.actualYear == (presentYear - 1)).Select(a => a.id).FirstOrDefault();
            //    int AY4 = jntuh_academic_year.Where(a => a.actualYear == (presentYear - 2)).Select(a => a.id).FirstOrDefault();

            //    List<jntuh_college_intake_existing> intake = db.jntuh_college_intake_existing.Where(i => collegeIDs.Contains(i.collegeId)).ToList();
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

            //    collegeIntakeExisting = collegeIntakeExisting.AsEnumerable().GroupBy(r => new { r.specializationId, r.shiftId, r.collegeId }).Select(r => r.First()).ToList();

            //    //college Reg nos
            //    var jntuh_college_faculty_registered = db.jntuh_college_faculty_registered.Where(cf => cf.collegeId == collegeId).ToList();
            //    string[] strRegnos = jntuh_college_faculty_registered.Select(cf => cf.RegistrationNumber).ToArray();

            //    //education categoryIds UG,PG,PHD...........

            //    int pharmacyDeptId = db.jntuh_department.Where(d => d.departmentName == "Pharmacy").Select(d => d.id).FirstOrDefault();

            //    var jntuh_education_category = db.jntuh_education_category.ToList();

            //    var registeredFaculty = db.jntuh_registered_faculty.Where(rf => strRegnos.Contains(rf.RegistrationNumber) && (rf.collegeId == null || rf.collegeId == collegeId)).ToList();
            //    //Reg nos related online facultyIds
            //    var jntuh_registered_faculty1 = registeredFaculty.Where(rf => rf.DepartmentId != null && (rf.isApproved == null || rf.isApproved == true) && (rf.PANNumber != null || rf.AadhaarNumber != null))
            //                                     .Select(rf => new
            //                                     {
            //                                         RegistrationNumber = rf.RegistrationNumber,
            //                                         Department = rf.jntuh_department.departmentName,
            //                                         HighestDegreeID = rf.jntuh_registered_faculty_education.Count() != 0 ? rf.jntuh_registered_faculty_education.Select(e => e.educationId).Max() : 0,
            //                                         IsApproved = rf.isApproved,
            //                                         PanNumber = rf.PANNumber,
            //                                         AadhaarNumber = rf.AadhaarNumber
            //                                     }).ToList();
            //    jntuh_registered_faculty1 = jntuh_registered_faculty1.Where(e => e.HighestDegreeID != 0).ToList();
            //    var jntuh_registered_faculty = jntuh_registered_faculty1.Select(rf => new
            //    {
            //        RegistrationNumber = rf.RegistrationNumber,
            //        Department = rf.Department,
            //        HighestDegree = jntuh_education_category.Where(c => c.id == rf.HighestDegreeID).Select(c => c.educationCategoryName).FirstOrDefault(),
            //        Recruitedfor = jntuh_college_faculty_registered.Where(c => c.RegistrationNumber == rf.RegistrationNumber).Select(c => c.IdentifiedFor).FirstOrDefault(),
            //        SpecializationId = jntuh_college_faculty_registered.Where(c => c.RegistrationNumber == rf.RegistrationNumber).Select(c => c.SpecializationId).FirstOrDefault(),
            //        PanNumber = rf.PanNumber,
            //        AadhaarNumber = rf.AadhaarNumber
            //    }).Where(e => e.Department != null)
            //                                     .ToList();

            //    foreach (var item in collegeIntakeExisting)
            //    {
            //        CollegeFacultyWithIntakeReport intakedetails = new CollegeFacultyWithIntakeReport();
            //        int phdFaculty = 0;
            //        int pgFaculty = 0;
            //        int ugFaculty = 0;

            //        intakedetails.collegeId = item.collegeId;
            //        intakedetails.collegeCode = jntuh_college.Where(c => c.id == item.collegeId).Select(c => c.collegeCode).FirstOrDefault();
            //        intakedetails.collegeName = jntuh_college.Where(c => c.id == item.collegeId).Select(c => c.collegeName).FirstOrDefault();
            //        intakedetails.Degree = item.Degree;
            //        intakedetails.Department = item.Department;
            //        intakedetails.Specialization = item.Specialization;
            //        intakedetails.specializationId = item.specializationId;
            //        intakedetails.DepartmentID = item.DepartmentID;
            //        intakedetails.shiftId = item.shiftId;

            //        intakedetails.approvedIntake1 = GetIntake(item.collegeId, AY1, item.specializationId, item.shiftId, 1);
            //        intakedetails.approvedIntake2 = GetIntake(item.collegeId, AY2, item.specializationId, item.shiftId, 1);
            //        intakedetails.approvedIntake3 = GetIntake(item.collegeId, AY3, item.specializationId, item.shiftId, 1);
            //        intakedetails.approvedIntake4 = GetIntake(item.collegeId, AY4, item.specializationId, item.shiftId, 1);
            //        facultystudentRatio = Convert.ToInt32(jntuh_faculty_student_ratio_norms.Where(fn => fn.degreeId == item.degreeID).Select(fn => fn.Norms).FirstOrDefault().Split(':')[1].ToString());

            //        if (item.Degree == "B.Tech" || item.Degree == "B.Pharmacy")
            //        {
            //            intakedetails.totalIntake = (intakedetails.approvedIntake2) + (intakedetails.approvedIntake3) + (intakedetails.approvedIntake4);
            //            facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) / Convert.ToDecimal(facultystudentRatio);
            //        }
            //        else if (item.Degree == "M.Tech" || item.Degree == "M.Pharmacy" || item.Degree == "MBA")
            //        {
            //            intakedetails.totalIntake = (intakedetails.approvedIntake1) + (intakedetails.approvedIntake2);
            //            facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) / Convert.ToDecimal(facultystudentRatio);

            //        }
            //        else if (item.Degree == "MCA")
            //        {
            //            intakedetails.totalIntake = (intakedetails.approvedIntake1) + (intakedetails.approvedIntake2) + (intakedetails.approvedIntake3);
            //            facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) / Convert.ToDecimal(facultystudentRatio);
            //        }
            //        else //MAM MTM Pharm.D Pharm.D PB
            //        {
            //            intakedetails.totalIntake = (intakedetails.approvedIntake1) + (intakedetails.approvedIntake2) + (intakedetails.approvedIntake3) + (intakedetails.approvedIntake4);
            //            facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) / Convert.ToDecimal(facultystudentRatio);
            //        }

            //        intakedetails.requiredFaculty = Math.Round(facultyRatio, 2);
            //        intakedetails.degreeDisplayOrder = item.degreeDisplayOrder;

            //        //====================================
            //        // intakedetails.specializationWiseFaculty = jntuh_registered_faculty.Where(s => s.SpecializationId == item.specializationId).Count();

            //        string strdegreetype = jntuh_degree.Where(d => d.degree == item.Degree).Select(d => d.jntuh_degree_type.degreeType).FirstOrDefault();
            //        if (strdegreetype == "UG")
            //        {
            //            if (item.Degree == "B.Pharmacy")
            //            {
            //                intakedetails.specializationWiseFaculty = jntuh_registered_faculty.Where(f => f.Department == "Pharmacy" && f.Recruitedfor == "UG").Count();
            //            }
            //            else
            //            {
            //                intakedetails.specializationWiseFaculty = jntuh_registered_faculty.Where(f => f.Department == item.Department && f.Recruitedfor == "UG").Count();
            //            }
            //        }

            //        if (strdegreetype == "PG")
            //        {
            //            if (item.Degree == "M.Pharmacy")
            //            {
            //                intakedetails.specializationWiseFaculty = jntuh_registered_faculty.Where(f => f.Department == "Pharmacy" && (f.Recruitedfor == "PG" || f.Recruitedfor == "UG&PG") && f.SpecializationId == item.specializationId).Count();
            //            }
            //            else
            //            {
            //                intakedetails.specializationWiseFaculty = jntuh_registered_faculty.Where(f => f.Department == item.Department && (f.Recruitedfor == "PG" || f.Recruitedfor == "UG&PG") && f.SpecializationId == item.specializationId).Count();
            //            }
            //        }
            //        intakedetails.id = jntuh_college_faculty_deficiency.Where(fd => fd.CollegeId == collegeId && fd.SpecializationId == item.specializationId && fd.ShiftId == item.shiftId).Select(fd => fd.Id).FirstOrDefault();

            //        if (intakedetails.id > 0)
            //        {
            //            int? swf = jntuh_college_faculty_deficiency.Where(fd => fd.Id == intakedetails.id).Select(fd => fd.SpecializationWiseFaculty).FirstOrDefault();
            //            if (swf != null)
            //            {
            //                intakedetails.specializationWiseFaculty = (int)swf;
            //            }
            //            intakedetails.deficiency = jntuh_college_faculty_deficiency.Where(fd => fd.Id == intakedetails.id).Select(fd => fd.Deficiency).FirstOrDefault();
            //            intakedetails.shortage = jntuh_college_faculty_deficiency.Where(fd => fd.Id == intakedetails.id).Select(fd => fd.Shortage).FirstOrDefault();
            //        }

            //        //============================================

            //        int noPanOrAadhaarcount = 0;

            //        if (item.Degree == "B.Pharmacy" || item.Degree == "M.Pharmacy")
            //        {
            //            ugFaculty = jntuh_registered_faculty.Where(f => "UG" == f.HighestDegree && f.Department == "Pharmacy").Count();
            //            pgFaculty = jntuh_registered_faculty.Where(f => ("PG" == f.HighestDegree || "M.Phil" == f.HighestDegree) && f.Department == "Pharmacy").Count();
            //            phdFaculty = jntuh_registered_faculty.Where(f => "Ph.D" == f.HighestDegree && f.Department == "Pharmacy").Count();
            //            noPanOrAadhaarcount = registeredFaculty.Where(f => f.DepartmentId == pharmacyDeptId && f.PANNumber == null && f.AadhaarNumber == null && (f.isApproved == null || f.isApproved == true)).Count();
            //            intakedetails.Department = "Pharmacy";
            //        }
            //        else
            //        {
            //            ugFaculty = jntuh_registered_faculty.Where(f => "UG" == f.HighestDegree && f.Department == item.Department).Count();
            //            pgFaculty = jntuh_registered_faculty.Where(f => ("PG" == f.HighestDegree || "M.Phil" == f.HighestDegree) && f.Department == item.Department).Count();
            //            phdFaculty = jntuh_registered_faculty.Where(f => "Ph.D" == f.HighestDegree && f.Department == item.Department).Count();
            //            noPanOrAadhaarcount = registeredFaculty.Where(f => f.DepartmentId == item.DepartmentID && f.PANNumber == null && f.AadhaarNumber == null && (f.isApproved == null || f.isApproved == true)).Count();
            //        }

            //        intakedetails.phdFaculty = phdFaculty;
            //        intakedetails.pgFaculty = pgFaculty;
            //        intakedetails.ugFaculty = ugFaculty;
            //        intakedetails.totalFaculty = (ugFaculty + pgFaculty + phdFaculty);
            //        intakedetails.facultyWithoutPANAndAadhaar = noPanOrAadhaarcount;
            //        //=============//

            //        intakedetailsList.Add(intakedetails);
            //    }

            //    intakedetailsList = intakedetailsList.OrderBy(ei => ei.Department).ThenBy(ei => ei.degreeDisplayOrder).ThenBy(ei => ei.Specialization).ThenBy(ei => ei.shiftId).ToList();

            //    string[] strOtherDepartments = { "English", "Mathematics", "Physics", "Chemistry", "Others" };
            //    int btechdegreecount = intakedetailsList.Where(d => d.Degree == "B.Tech").Count();
            //    if (btechdegreecount != 0)
            //    {
            //        foreach (var department in strOtherDepartments)
            //        {
            //            int speId = jntuh_specialization.Where(s => s.jntuh_department.departmentName == department).Select(s => s.id).FirstOrDefault();
            //            int ugFaculty = jntuh_registered_faculty.Where(f => "UG" == f.HighestDegree && f.Department == department).Count();
            //            int pgFaculty = jntuh_registered_faculty.Where(f => ("PG" == f.HighestDegree || "M.Phil" == f.HighestDegree) && f.Department == department).Count();
            //            int phdFaculty = jntuh_registered_faculty.Where(f => "Ph.D" == f.HighestDegree && f.Department == department).Count();

            //            int facultydeficiencyId = jntuh_college_faculty_deficiency.Where(fd => fd.CollegeId == collegeId && fd.SpecializationId == speId && fd.ShiftId == 1).Select(fd => fd.Id).FirstOrDefault();
            //            if (facultydeficiencyId == 0)
            //            {
            //                intakedetailsList.Add(new CollegeFacultyWithIntakeReport { collegeId = (int)collegeId, Degree = "B.Tech", Department = department, Specialization = department, ugFaculty = ugFaculty, pgFaculty = pgFaculty, phdFaculty = phdFaculty, totalFaculty = ugFaculty + pgFaculty + phdFaculty, specializationId = speId, shiftId = 1 });
            //            }
            //            else
            //            {
            //                int? swf = jntuh_college_faculty_deficiency.Where(fd => fd.Id == facultydeficiencyId).Select(fd => fd.SpecializationWiseFaculty).FirstOrDefault();
            //                bool deficiency = jntuh_college_faculty_deficiency.Where(fd => fd.Id == facultydeficiencyId).Select(fd => fd.Deficiency).FirstOrDefault();
            //                int shortage = jntuh_college_faculty_deficiency.Where(fd => fd.Id == facultydeficiencyId).Select(fd => fd.Shortage).FirstOrDefault();
            //                intakedetailsList.Add(new CollegeFacultyWithIntakeReport { id = facultydeficiencyId, collegeId = (int)collegeId, Degree = "B.Tech", Department = department, Specialization = department, ugFaculty = ugFaculty, pgFaculty = pgFaculty, phdFaculty = phdFaculty, totalFaculty = ugFaculty + pgFaculty + phdFaculty, specializationId = speId, shiftId = 1, specializationWiseFaculty = (int)swf, deficiency = deficiency, shortage = shortage });
            //            }
            //        }
            //    }
            //}

            //return intakedetailsList;
            #endregion

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
                var jntuh_college_faculty_registered = db.jntuh_college_faculty_registered.Where(cf => cf.collegeId == collegeId).ToList();  // && cf.createdBy != 63809
                string[] strRegnos = jntuh_college_faculty_registered.Select(cf => cf.RegistrationNumber).ToArray();

                //education categoryIds UG,PG,PHD...........

                int pharmacyDeptId = db.jntuh_department.Where(d => d.departmentName == "Pharmacy").Select(d => d.id).FirstOrDefault();

                var jntuh_education_category = db.jntuh_education_category.ToList();

                // var registeredFaculty = db.jntuh_registered_faculty.Where(rf => strRegnos.Contains(rf.RegistrationNumber) && (rf.collegeId == null || rf.collegeId == collegeId)).ToList();
                var jntuh_college_faculty_registered_new = db.jntuh_college_principal_registered.Where(cf => cf.collegeId == collegeId).ToList();
                var principalRegno = jntuh_college_faculty_registered_new.Select(cf => cf.RegistrationNumber).FirstOrDefault();


                //int[] CollegeIDS = new int[] { 113, 172, 360,327,80,23,186,29,111,222,164 };

                int[] CollegeIDS = new int[] { 7, 8, 12, 35, 50, 56, 69, 74, 77, 84, 85, 86, 88, 91, 111, 113, 128, 137, 141, 144, 145, 147, 151, 152, 162, 165, 166, 176, 185, 186, 193, 194, 211, 215, 222, 223, 225, 230, 238, 245, 249, 250, 261, 264, 276, 282, 288, 293, 299, 300, 306, 307, 327, 342, 352, 374, 382, 385, 414, 429, 435, 443, 4, 20, 70, 72, 81, 87, 116, 124, 130, 148, 156, 172, 177, 182, 187, 195, 197, 214, 218, 228, 241, 242, 247, 256, 287, 334, 336, 360, 365, 43, 266, 41, 79, 80, 103, 121, 129, 138, 155, 163, 164, 201, 227, 244, 254, 260, 269, 271, 286, 321, 324, 329, 338, 368, 369, 373, 400, 403, 455, 11, 22, 23, 26, 29, 32, 38, 40, 46, 68, 108, 109, 115, 123, 134, 153, 170, 171, 175, 178, 179, 180, 183, 184, 188, 189, 192, 196, 207, 210, 243, 291, 310, 316, 326, 330, 349, 367, 399, 420, 441, 100, 158, 168, 236, 259, 304, 309, 350, 408, 39, 75, 332, 364 };

                var registeredFaculty = new List<jntuh_registered_faculty>();
                if (CollegeIDS.Contains((int)collegeId))
                {
                    registeredFaculty = db.jntuh_registered_faculty.Where(rf => strRegnos.Contains(rf.RegistrationNumber.Trim())).ToList();
                }
                else
                {
                    registeredFaculty = principalRegno != null ? db.jntuh_registered_faculty.Where(rf => strRegnos.Contains(rf.RegistrationNumber.Trim()) && rf.RegistrationNumber != principalRegno).ToList()
                    : db.jntuh_registered_faculty.Where(rf => strRegnos.Contains(rf.RegistrationNumber.Trim())).ToList();
                }


                //Reg nos related online facultyIds
                var MecFaculty = registeredFaculty.Where(rf => rf.DepartmentId != null && ((rf.Absent != true) && (rf.NotQualifiedAsperAICTE != true)
                                                        && (rf.NoSCM != true) && (rf.PANNumber != null) && rf.Notin116 != true && rf.Blacklistfaculy != true && rf.PHDundertakingnotsubmitted != true) && (rf.DepartmentId != 61 || rf.DepartmentId != 27) && rf.type != "Adjunct"  && rf.DepartmentId==15).ToList();
                var jntuh_registered_faculty1 = registeredFaculty.Where(rf => rf.DepartmentId != null && ((rf.Absent != true) && (rf.NotQualifiedAsperAICTE != true)
                                                        && (rf.NoSCM != true) && (rf.PANNumber != null) && rf.Notin116 != true && rf.Blacklistfaculy != true && rf.PHDundertakingnotsubmitted != true) && (rf.DepartmentId != 61 || rf.DepartmentId != 27) && rf.type != "Adjunct")
                                                 .Select(rf => new
                                                 {
                                                     RegistrationNumber = rf.RegistrationNumber.Trim(),
                                                     Department = rf.jntuh_department.departmentName,
                                                     HighestDegreeID = rf.jntuh_registered_faculty_education.Count() != 0 ? rf.jntuh_registered_faculty_education.Select(e => e.educationId).Max() : 0,
                                                     IsApproved = rf.isApproved,
                                                     PanNumber = rf.PANNumber,
                                                     AadhaarNumber = rf.AadhaarNumber,
                                                     NoForm16 = rf.NoForm16,
                                                     TotalExperience = rf.TotalExperience
                                                 }).ToList();
                jntuh_registered_faculty1 = jntuh_registered_faculty1.Where(e => e.HighestDegreeID >= 4).ToList();
                var jntuh_registered_faculty = jntuh_registered_faculty1.Select(rf => new
                {
                    RegistrationNumber = rf.RegistrationNumber,
                    Department = rf.Department,
                    HighestDegree = jntuh_education_category.Where(c => c.id == rf.HighestDegreeID).Select(c => c.educationCategoryName).FirstOrDefault(),
                    Recruitedfor = jntuh_college_faculty_registered.Where(c => c.RegistrationNumber.Trim() == rf.RegistrationNumber.Trim()).Select(c => c.IdentifiedFor).FirstOrDefault(),
                    SpecializationId = jntuh_college_faculty_registered.Where(c => c.RegistrationNumber.Trim() == rf.RegistrationNumber.Trim()).Select(c => c.SpecializationId).FirstOrDefault(),
                    PanNumber = rf.PanNumber,
                    AadhaarNumber = rf.AadhaarNumber
                }).Where(e => e.Department != null)
                                                 .ToList();


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
                    intakedetails.approvedIntake1 = GetIntake(item.collegeId, AY1, item.specializationId, item.shiftId, 1);
                    intakedetails.approvedIntake2 = GetIntake(item.collegeId, AY2, item.specializationId, item.shiftId, 1);
                    intakedetails.approvedIntake3 = GetIntake(item.collegeId, AY3, item.specializationId, item.shiftId, 1);
                    intakedetails.approvedIntake4 = GetIntake(item.collegeId, AY4, item.specializationId, item.shiftId, 1);
                    intakedetails.approvedIntake5 = GetIntake(item.collegeId, AY5, item.specializationId, item.shiftId, 1);
                    facultystudentRatio = Convert.ToInt32(jntuh_faculty_student_ratio_norms.Where(fn => fn.degreeId == item.degreeID).Select(fn => fn.Norms).FirstOrDefault().Split(':')[1].ToString());


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
                            intakedetails.specializationWiseFaculty = jntuh_registered_faculty.Where(f => f.Department == "Pharmacy" && f.Recruitedfor == "UG").Count();
                        }
                        else
                        {
                            intakedetails.specializationWiseFaculty = jntuh_registered_faculty.Where(f => f.Department == item.Department).Count();//&& f.Recruitedfor == "UG"
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
                            intakedetails.specializationWiseFaculty = jntuh_registered_faculty.Where(f => f.Department == item.Department && f.SpecializationId == item.specializationId).Count();//&& (f.Recruitedfor == "PG" || f.Recruitedfor == "UG&PG")
                        }
                    }
                    //intakedetails.id = jntuh_college_faculty_deficiency.Where(fd => fd.CollegeId == collegeId && fd.SpecializationId == item.specializationId && fd.ShiftId == item.shiftId).Select(fd => fd.Id).FirstOrDefault();

                    //if (intakedetails.id > 0)
                    //{
                    //    int? swf = jntuh_college_faculty_deficiency.Where(fd => fd.Id == intakedetails.id).Select(fd => fd.SpecializationWiseFaculty).FirstOrDefault();
                    //    if (swf != null)
                    //    {
                    //        intakedetails.specializationWiseFaculty = (int)swf;
                    //    }
                    //    intakedetails.deficiency = jntuh_college_faculty_deficiency.Where(fd => fd.Id == intakedetails.id).Select(fd => fd.Deficiency).FirstOrDefault();
                    //    intakedetails.shortage = jntuh_college_faculty_deficiency.Where(fd => fd.Id == intakedetails.id).Select(fd => fd.Shortage).FirstOrDefault();
                    //}

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
                        //ugFaculty = jntuh_registered_faculty.Where(f => "UG" == f.HighestDegree && f.Department == item.Department).Count();
                        //pgFaculty = jntuh_registered_faculty.Where(f => ("PG" == f.HighestDegree || "M.Phil" == f.HighestDegree) && f.Department == item.Department).Count();
                        //phdFaculty = jntuh_registered_faculty.Where(f => "Ph.D" == f.HighestDegree && f.Department == item.Department).Count();
                        //noPanOrAadhaarcount = registeredFaculty.Where(f => f.DepartmentId == item.DepartmentID && f.PANNumber == null && f.AadhaarNumber == null && (f.isApproved == null || f.isApproved == true)).Count();


                        ugFaculty = jntuh_registered_faculty.Count(f => "UG" == f.HighestDegree && f.Department == item.Department);
                        pgFaculty = jntuh_registered_faculty.Count(f => ("PG" == f.HighestDegree || "M.Phil" == f.HighestDegree) && f.Department == item.Department);
                        if (item.Degree == "M.Tech")
                            phdFaculty = jntuh_registered_faculty.Count(f => "Ph.D" == f.HighestDegree && f.Department == item.Department && f.SpecializationId == item.specializationId);
                        else
                            phdFaculty = jntuh_registered_faculty.Count(f => "Ph.D" == f.HighestDegree && f.Department == item.Department);

                        var phd = jntuh_registered_faculty.Where(f => "Ph.D" == f.HighestDegree && f.Department == item.Department).ToList();
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

                            specializationWiseFaculty = jntuh_registered_faculty.Count(f => f.Department == deptname)
                        });
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
            public int DegreeID { get; set; }
            public string RegistrationNumber { get; set; }
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
            public int pgFaculty { get; set; }
            public int ugFaculty { get; set; }
            public int totalFaculty { get; set; }
            public int SpecializationsphdFaculty { get; set; }
            public int SpecializationspgFaculty { get; set; }
            public int specializationWiseFaculty { get; set; }
            public int facultyWithoutPANAndAadhaar { get; set; }

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
             #region BASNotice Properties Code Written by Siva
            public string[] PhdRegNos{get;set;}
            public string[] NonPhdRegNos { get; set; }
            public int admittedIntake2 { get; set; }
            public int admittedIntake3 { get; set; }
            public int admittedIntake4 { get; set; }
            #endregion
        }

        public class PharmacySpecilaizationList
        {
            public string PharmacyspecName { get; set; }
            public string Specialization { get; set; }
            public int PharmacyspecWiseCount { get; set; }
        }

        private int GetIntake(int collegeId, int academicYearId, int specializationId, int shiftId, int flag)
        {
            int intake = 0;

            //approved
            //if (flag == 1)
            //{
            //    intake = db.jntuh_college_intake_existing.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == shiftId).Select(i => i.approvedIntake).FirstOrDefault();

            //}
            //else //admitted
            //{
            //    intake = db.jntuh_college_intake_existing.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == shiftId).Select(i => i.admittedIntake).FirstOrDefault();
            //}

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

        public List<Lab> collegeLabs(int? collegeID)
        {
            List<Lab> lstlaboratories = new List<Lab>();
            List<Lab> collegeLabMaster = new List<Lab>();
            var jntuh_college_laboratories_deficiency = db.jntuh_college_laboratories_deficiency.Where(c => c.CollegeId == collegeID).ToList();

            string strcollegecode = db.jntuh_college_randamcodes.Where(r => r.CollegeId == collegeID).Select(r => r.RandamCode).FirstOrDefault();
            int[] specializationIds = db.jntuh_college_intake_existing.Where(e => e.collegeId == collegeID).Select(e => e.specializationId).Distinct().ToArray();
            string CollegeAffiliationStatus = db.jntuh_college_affiliation.Where(u => u.collegeId == collegeID && u.affiliationTypeId == 7).Select(u => u.affiliationStatus).FirstOrDefault();
            //List<Lab> collegeLabMaster = db.jntuh_lab_master.AsNoTracking()
            //                                            .Where(l => specializationIds.Contains(l.SpecializationID))
            //                                            .Select(l => new Lab
            //                                            {
            //                                                ////// EquipmentID=l.id,                                                               
            //                                                degreeId = l.DegreeID,
            //                                                degree = l.jntuh_degree.degree,
            //                                                degreeDisplayOrder = l.jntuh_degree.degreeDisplayOrder,
            //                                                departmentId = l.DepartmentID,
            //                                                department = l.jntuh_department.departmentName,
            //                                                specializationId = l.SpecializationID,
            //                                                specializationName = l.jntuh_specialization.specializationName,
            //                                                year = l.Year,
            //                                                Semester = l.Semester,
            //                                                Labcode = l.Labcode,
            //                                                LabName = l.LabName
            //                                            })
            //                                            .OrderBy(l => l.degreeDisplayOrder)
            //                                            .ThenBy(l => l.department)
            //                                            .ThenBy(l => l.specializationName)
            //                                            .ThenBy(l => l.year).ThenBy(l => l.Semester).Distinct()
            //                                            .ToList();
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
            else if (CollegeAffiliationStatus == "No" || CollegeAffiliationStatus == null || CollegeAffiliationStatus == "Not Yet Applied")
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
                    lstlabs.deficiency = jntuh_college_laboratories_deficiency.Where(ld => ld.LabCode == item.Labcode && ld.CollegeId == collegeID && ld.Year == item.year && ld.Semister == item.Semester && ld.SpecializationId == item.specializationId).Select(ld => ld.Deficiency).FirstOrDefault();
                    lstlabs.id = jntuh_college_laboratories_deficiency.Where(ld => ld.LabCode == item.Labcode && ld.CollegeId == collegeID && ld.Year == item.year && ld.Semister == item.Semester && ld.SpecializationId == item.specializationId).Select(ld => ld.Id).FirstOrDefault();
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
            public int TotalIntake { get; set; }
            public int Required { get; set; }
            public decimal Requiredfaculty { get; set; }
            public int Available { get; set; }
            public string Deficiency { get; set; }
            public string PhdDeficiency { get; set; }
            public string LabsDeficiency { get; set; }
        }

        #region ForPharmacyCollegeFaculty

        public List<CollegeFacultyLabs> PharmacyDeficienciesInFaculty(int? collegeID)
        {
            string faculty = string.Empty;

            faculty += "<table width='100%' border='0' cellpadding='5' cellspacing='0'>";
            faculty += "<tr>";
            faculty += "<td align='left'><b><u>Deficiencies in Faculty</u></b> (Department/ Specialization Wise):";
            faculty += "</tr>";
            faculty += "</table>";

            List<CollegeFacultyWithIntakeReport> facultyCounts = PharmacyCollegeFaculty(collegeID).Where(c => c.shiftId == 1).OrderBy(i => i.SortId).ToList();//Where(c => c.shiftId == 1)

            var count = facultyCounts.Count();
            var distDeptcount = 1;
            var deptloop = 1;
            var specloop = 1;
            decimal departmentWiseRequiredFaculty = 0;

            string[] departments = new string[] { "English", "Mathematics", "Physics", "Chemistry", "Others" };

            int totalBtechFirstYearIntake = facultyCounts.Where(d => d.Degree == "B.Tech").Select(d => d.approvedIntake1).Sum();
            var degrees = db.jntuh_degree.ToList();
            var firstYearRequired = Math.Ceiling((double)totalBtechFirstYearIntake / 150);
            int remainingFaculty = 0;
            int remainingPHDFaculty = 0;
            var remainingphramdFaculty = 0;
            var distSpeccount = 0;
            var totalusedfaculty = 0;
            var remainingmpharmacyfaculty = 0;
            //if (PharmDRequiredFaculty == 0)
            //{
            //    var facultycount = TotalcollegeFaculty - (int)Math.Ceiling(ViewBag.BpharmacyrequiredFaculty);
            //    remainingmpharmacyfaculty = facultycount;
            //}
            if (TotalcollegeFaculty > 0)
            {
                var facultycount = TotalcollegeFaculty - (int)Math.Ceiling(BpharmacyrequiredFaculty);
                remainingmpharmacyfaculty = facultycount;
                if (PharmDRequiredFaculty > 0)
                {
                    remainingmpharmacyfaculty = remainingmpharmacyfaculty - (int)Math.Ceiling(PharmDRequiredFaculty);
                }
                if (ViewBag.PharmDPBrequiredFaculty > 0)
                {
                    remainingmpharmacyfaculty = remainingmpharmacyfaculty - (int)Math.Ceiling(PharmDPBRequiredFaculty);
                }

                if (remainingmpharmacyfaculty < 0)
                {
                    remainingmpharmacyfaculty = 0;
                }
            }
            faculty += "<table width='100%' border='1' cellpadding='3' cellspacing='0' style='border-color: #ccc;font-size:13px'>";
            faculty += "<tr>";
            faculty += "<th style='text-align: center; vertical-align: top;'>SNo</th>";
            faculty += "<th style='text-align: left; vertical-align: top;' >Department</th>";
            faculty += "<th style='text-align: left; vertical-align: top;' >Degree</th>";
            faculty += "<th style='text-align: left; vertical-align: top;' >Specialization</th>";
            //faculty += "<th style='text-align: left; vertical-align: top;' >Status</th>";
            faculty += "<th style='text-align: center; vertical-align: top;' >Total Intake *</th>";
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
            List<CollegeFacultyLabs> lstFaculty = new List<CollegeFacultyLabs>();
            var degreeTypes = db.jntuh_degree_type.Select(t => t).ToList();
            foreach (var item in facultyCounts)
            {
                var pharmadsubgroupmet = "";
                distDeptcount = facultyCounts.Where(d => d.Department == item.Department).Distinct().Count();
                distSpeccount = facultyCounts.Where(d => d.Specialization == item.Specialization && d.Degree == item.Degree).Distinct().Count();
                int indexnow = facultyCounts.IndexOf(item);

                if (indexnow > 0 && facultyCounts[indexnow].Department != facultyCounts[indexnow - 1].Department)
                {
                    deptloop = 1;
                }
                if (indexnow > 0 && facultyCounts[indexnow].Specialization != facultyCounts[indexnow - 1].Specialization)
                {
                    specloop = 1;
                }
                departmentWiseRequiredFaculty = facultyCounts.Where(d => d.Department == item.Department).Select(d => d.requiredFaculty).Sum();

                string minimumRequirementMet = string.Empty;
                int facultyShortage = 0;
                int adjustedFaculty = 0;
                int adjustedPHDFaculty = 0;

                int tFaculty = (int)Math.Ceiling(Convert.ToDecimal(item.totalFaculty));//totalFaculty
                int rFaculty = (int)Math.Ceiling(item.requiredFaculty);

                if (departments.Contains(item.Department))
                {
                    rFaculty = (int)firstYearRequired;
                    departmentWiseRequiredFaculty = (int)firstYearRequired;
                }

                var degreeType = degrees.Where(d => d.degree == item.Degree).Select(d => d.jntuh_degree_type.degreeType).FirstOrDefault();


                if (item.Degree == "Pharm.D")//&& @ViewBag.BpharmcyCondition == "No"
                {
                    tFaculty = item.totalcollegefaculty;
                    rFaculty = (int)Math.Ceiling(BpharmacyrequiredFaculty);

                    var pharmadreqfaculty = (int)Math.Ceiling(item.pharmadrequiredfaculty);
                    if (deptloop == 1)
                    {
                        if (tFaculty >= rFaculty && ((tFaculty - rFaculty) >= pharmadreqfaculty))
                        {
                            minimumRequirementMet = "NO";
                            adjustedFaculty = tFaculty - rFaculty;

                            if (adjustedFaculty > pharmadreqfaculty)
                            {
                                remainingphramdFaculty = adjustedFaculty - pharmadreqfaculty;
                                adjustedFaculty = pharmadreqfaculty;
                            }
                        }
                        else
                        {
                            minimumRequirementMet = "YES";
                            remainingphramdFaculty = tFaculty - rFaculty;
                        }
                    }
                }

                if (item.Degree == "Pharm.D PB")//&& @ViewBag.BpharmcyCondition == "No" && @ViewBag.PharmaDCondition == "No"
                {
                    tFaculty = item.totalcollegefaculty;
                    rFaculty = (int)Math.Ceiling(BpharmacyrequiredFaculty);

                    var pharmadpbreqfaculty = (int)Math.Ceiling(item.pharmadPBrequiredfaculty);
                    if (deptloop == 1)
                    {
                        if (tFaculty >= rFaculty && ((tFaculty - rFaculty) >= pharmadpbreqfaculty))
                        {
                            minimumRequirementMet = "NO";
                            adjustedFaculty = tFaculty - rFaculty;

                            if (remainingphramdFaculty > pharmadpbreqfaculty)
                            {
                                adjustedFaculty = pharmadpbreqfaculty;
                                remainingphramdFaculty = remainingphramdFaculty - pharmadpbreqfaculty;
                            }
                            else
                            {
                                adjustedFaculty = remainingphramdFaculty;
                            }
                        }
                        else
                        {
                            minimumRequirementMet = "YES";
                        }
                    }
                }



                if (item.Degree == "B.Pharmacy" && indexnow > 0 && item.PharmacySubGroup1 != "SubGroup6")
                {
                    tFaculty = (int)Math.Ceiling(Convert.ToDecimal(item.BPharmacySubGroup1Count));
                    rFaculty = item.BPharmacySubGroupRequired;
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
                        if (rFaculty <= remainingFaculty && tFaculty >= rFaculty)
                        {
                            minimumRequirementMet = "YES";
                            remainingFaculty = remainingFaculty + (tFaculty - rFaculty);
                            adjustedFaculty = rFaculty;
                        }
                        else if (tFaculty >= rFaculty)
                        {
                            minimumRequirementMet = "NO";
                            adjustedFaculty = rFaculty;
                            facultyShortage = rFaculty - remainingFaculty;
                            //remainingFaculty = 0;
                        }
                        else if (tFaculty < rFaculty)
                        {
                            minimumRequirementMet = "NO";
                            adjustedFaculty = tFaculty;
                            remainingFaculty = remainingFaculty - tFaculty;
                        }
                        if (remainingPHDFaculty > 0 && degreeType.Equals("PG"))
                        {
                            adjustedPHDFaculty = 1;
                            remainingPHDFaculty = remainingPHDFaculty - 1;
                        }
                    }

                }

                else if (item.Degree == "B.Pharmacy" && indexnow == 0 && item.PharmacySubGroup1 != "SubGroup6")
                {
                    tFaculty = (int)Math.Ceiling(Convert.ToDecimal(item.BPharmacySubGroup1Count));
                    rFaculty = item.BPharmacySubGroupRequired;
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
                        if (rFaculty <= remainingFaculty && tFaculty >= rFaculty)
                        {
                            minimumRequirementMet = "YES";
                            remainingFaculty = remainingFaculty - rFaculty;
                            adjustedFaculty = rFaculty;
                        }
                        else if (tFaculty >= rFaculty)
                        {
                            minimumRequirementMet = "NO";
                            adjustedFaculty = remainingFaculty;
                            facultyShortage = rFaculty - remainingFaculty;
                            remainingFaculty = 0;
                        }
                        else if (tFaculty < rFaculty)
                        {
                            minimumRequirementMet = "NO";
                            adjustedFaculty = tFaculty;
                            //remainingFaculty = remainingFaculty - tFaculty;
                        }




                        if (remainingPHDFaculty > 0 && degreeType.Equals("PG"))
                        {
                            adjustedPHDFaculty = 1;
                            remainingPHDFaculty = remainingPHDFaculty - 1;
                        }
                    }

                }

                ////New
                //var mpharmremainingfaculty = TotalcollegeFaculty - (int)Math.Ceiling(BpharmacyrequiredFaculty);
                //if (item.Degree == "M.Pharmacy" && PharmDRequiredFaculty <= 0 && item.PharmacyspecializationWiseFaculty >= 1)
                //{
                //    tFaculty = TotalcollegeFaculty;
                //    rFaculty = (int)Math.Ceiling(BpharmacyrequiredFaculty);
                //    //remainingFaculty = tFaculty - rFaculty;

                //    var mpharmacyreqfaculty = (int)Math.Ceiling(item.requiredFaculty);
                //    if (tFaculty >= rFaculty && ((tFaculty - rFaculty) >= mpharmacyreqfaculty))
                //    {
                //        minimumRequirementMet = "YES";
                //        adjustedFaculty = tFaculty - rFaculty;

                //        if (remainingFaculty > mpharmacyreqfaculty)
                //        {
                //            adjustedFaculty = mpharmacyreqfaculty;
                //            remainingFaculty = remainingFaculty - mpharmacyreqfaculty;
                //        }
                //        else
                //        {
                //            adjustedFaculty = remainingFaculty;
                //        }
                //    }
                //}

//New


                else if (item.Degree == "M.Pharmacy" && item.PharmacyspecializationWiseFaculty >= 1)//&& (PharmDRequiredFaculty > 0 || PharmDPBRequiredFaculty > 0)
                {
                    //tFaculty = (int)Math.Ceiling(Convert.ToDecimal(list[i].specializationWiseFaculty));PharmDRequiredFaculty
                    //if (remainingphramdFaculty > 0)
                    //{
                    //    tFaculty = remainingphramdFaculty;
                    //}
                    //else
                    //{
                    //    tFaculty = remainingFaculty;
                    //}

                    tFaculty = remainingmpharmacyfaculty;
                    rFaculty = (int)Math.Ceiling(item.requiredFaculty);


                    if (rFaculty <= tFaculty && remainingphramdFaculty > 0)
                    {
                        minimumRequirementMet = "NO";
                        remainingphramdFaculty = remainingphramdFaculty - rFaculty;
                        adjustedFaculty = rFaculty;
                    }
                    else if (rFaculty <= tFaculty && remainingphramdFaculty == 0 && remainingmpharmacyfaculty > 0)
                    {
                        minimumRequirementMet = "NO";
                        remainingFaculty = remainingFaculty - rFaculty;
                        adjustedFaculty = rFaculty;
                        remainingmpharmacyfaculty = remainingmpharmacyfaculty - rFaculty;
                    }

                    //else if (tFaculty <= rFaculty && remainingphramdFaculty > 0)
                    //{
                    //    remainingphramdFaculty = remainingphramdFaculty - rFaculty;
                    //    adjustedFaculty = tFaculty;
                    //}
                    else if (tFaculty <= rFaculty && remainingphramdFaculty == 0 && remainingmpharmacyfaculty > 0)
                    {
                        minimumRequirementMet = "YES";
                        remainingFaculty = remainingFaculty - rFaculty;
                        adjustedFaculty = tFaculty;
                        remainingmpharmacyfaculty = remainingmpharmacyfaculty - tFaculty;
                    }
                    else if (tFaculty <= rFaculty && remainingphramdFaculty == 0 && remainingmpharmacyfaculty == 0)
                    {
                        minimumRequirementMet = "YES";
                        remainingFaculty = remainingFaculty - rFaculty;
                        adjustedFaculty = tFaculty;
                        remainingmpharmacyfaculty = remainingmpharmacyfaculty - tFaculty;
                    }
                    else if (tFaculty <= rFaculty && remainingmpharmacyfaculty == 0)
                    {
                        minimumRequirementMet = "YES";
                        remainingFaculty = remainingFaculty - rFaculty;
                        adjustedFaculty = tFaculty;
                        remainingmpharmacyfaculty = remainingmpharmacyfaculty - tFaculty;
                    }
                    else if (tFaculty <= rFaculty && remainingmpharmacyfaculty > 0)
                    {
                        minimumRequirementMet = "YES";
                        remainingFaculty = remainingFaculty - rFaculty;
                        adjustedFaculty = tFaculty;
                        remainingmpharmacyfaculty = remainingmpharmacyfaculty - tFaculty;
                    }
                    if (remainingPHDFaculty > 0 && degreeType.Equals("PG"))
                    {
                        adjustedPHDFaculty = 1;
                        remainingPHDFaculty = remainingPHDFaculty - 1;
                    }
                }



                if ((item.Degree == "B.Pharmacy") && item.PharmacyGroup1 == "Group1")
                {
                    if (item.BPharmacySubGroup1Count >= item.BPharmacySubGroupRequired)
                    {
                        item.BPharmacySubGroupMet = "No Deficiency";
                    }
                    else
                    {
                        item.BPharmacySubGroupMet = "Deficiency";//Yes
                    }
                }

                else if (item.Degree == "Pharm.D" && item.PharmacyGroup1 == "Group1")
                {
                    if (item.BPharmacySubGroup1Count >= item.BPharmacySubGroupRequired && adjustedFaculty >= Math.Ceiling(PharmDRequiredFaculty) && bpharmacycondition == "No")
                    {
                        item.BPharmacySubGroupMet = "No Deficiency";
                        pharmadsubgroupmet = "No";
                        //minimumRequirementMet = "NO";
                    }
                    else
                    {
                        item.BPharmacySubGroupMet = "Deficiency";
                        //minimumRequirementMet = "YES";
                    }
                }

                else if (item.Degree == "Pharm.D PB" && item.PharmacyGroup1 == "Group1")
                {
                    if (item.BPharmacySubGroup1Count >= item.BPharmacySubGroupRequired && adjustedFaculty >= Math.Ceiling(pharmadpbrequiredfaculty) && bpharmacycondition == "No" && pharmadsubgroupmet == "No")
                    {
                        item.BPharmacySubGroupMet = "No Deficiency";
                        //minimumRequirementMet = "NO";
                    }
                    else
                    {
                        item.BPharmacySubGroupMet = "Deficiency";
                        //minimumRequirementMet = "YES";
                    }
                }



                if (item.Degree == "M.Pharmacy" && item.PharmacyspecializationWiseFaculty >= 1)//&& @adjustedFaculty == rFaculty
                {
                    if (bpharmacycondition == "No")//&& pharmdcondition == "No" && pharmadpbcondition == "No"
                    {
                        item.BPharmacySubGroupMet = "No Deficiency";
                        //minimumRequirementMet = "NO";
                    }
                    //else if (bpharmacycondition == "No")//&& pharmdcondition == "No" && pharmadpbrequiredfaculty == 0
                    //{
                    //    item.BPharmacySubGroupMet = "No";
                    //}
                    else
                    {
                        item.BPharmacySubGroupMet = "Deficiency";
                        //minimumRequirementMet = "YES";
                    }

                }
                else if (item.Degree == "M.Pharmacy" && item.PharmacyspecializationWiseFaculty < 1)
                {
                    item.BPharmacySubGroupMet = "Deficiency";
                    minimumRequirementMet = "YES";
                }
                //else if (item.Degree == "M.Pharmacy")
                //{
                //    item.BPharmacySubGroupMet = "Yes";
                //}


                faculty += "<tr>";
                faculty += "<td class='col2' style='text-align: center; vertical-align: top;width:10px' rowspan='1'>" + (facultyCounts.IndexOf(item) + 1) + "</td>";

                faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + item.Department + "</td>";
                if (specloop == 1)
                    faculty += "<td rowspan='" + distSpeccount + "'  class='col2' style='text-align: center; vertical-align: center;'>" + item.Degree + "</td>";
                if (specloop == 1)
                    faculty += "<td rowspan='" + distSpeccount + "'  class='col2' style='text-align: center; vertical-align: center;'>" + item.Specialization + "</td>";


                //faculty += "<td class='col2' style='text-align: center; vertical-align: top;font-weight: bold'>" + item.AffliationStatus + "</td>";
                if (item.totalIntake > 0)
                {
                    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + item.totalIntake + "</td>";
                }
                else
                {
                    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'></td>";
                }

                if (Math.Ceiling(item.requiredFaculty) > 0)
                {
                    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + Math.Ceiling(item.requiredFaculty) + " </td>";
                }
                else
                {
                    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'></td>";
                }

                if (item.Degree == "B.Pharmacy" && item.PharmacySubGroup1 == null)
                {
                    if (TotalcollegeFaculty > Math.Ceiling(item.requiredFaculty))
                    {
                        faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + Math.Ceiling(item.requiredFaculty) + " </td>";
                    }
                    else
                    {
                        faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + TotalcollegeFaculty + " </td>";
                    }

                }
                else if (item.Degree == "M.Pharmacy" || item.Degree == "Pharm.D" || item.Degree == "Pharm.D PB")
                {
                    totalusedfaculty = totalusedfaculty + adjustedFaculty;
                    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + adjustedFaculty + " </td>";
                }
                else if (item.Degree == "B.Pharmacy" && item.PharmacySubGroup1 != null)
                {
                    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'> </td>";
                }

                faculty += "<td class='col2' style='text-align: left; vertical-align: top;'>" + item.PharmacySubGroup1 + "</td>";
                if (item.BPharmacySubGroupRequired > 0 && item.Degree != "M.Pharmacy")
                {
                    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + item.BPharmacySubGroupRequired + "</td>";
                }
                //else if (item.Degree == "M.Pharmacy")
                //{
                //    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + item.BPharmacySubGroupRequired + "</td>";
                //}
                else
                {
                    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'></td>";
                }
                if (item.BPharmacySubGroupRequired > 0 && item.Degree != "M.Pharmacy")
                {
                    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + adjustedFaculty + "</td>";
                }
                else if (item.Degree == "M.Pharmacy")
                {
                    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + adjustedFaculty + "</td>";
                }
                else
                {
                    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'></td>";
                }

                if (item.BPharmacySubGroupMet == null && item.Degree == "B.Pharmacy")
                {
                    if (bpharmacycondition == "No")
                    {
                        faculty += "<td class='col2' style='text-align: center; vertical-align: top;'><b> B.Pharmacy No Deficiency.</b></td>";
                    }
                    else
                    {
                        faculty += "<td class='col2' style='text-align: center; vertical-align: top;'><b> B.Pharmacy Deficiency Exists & Hence Other Degrees will not be considered. </b></td>";
                    }
                }
                else
                {
                    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + item.BPharmacySubGroupMet + "</td>";
                }

                if (item.phdFaculty > 0 || item.totalIntake > 0)
                {
                    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + item.phdFaculty + "</td>";
                }
                else
                {
                    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'></td>";
                }

                //if (adjustedPHDFaculty > 0)
                //{
                //    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>NO</td>";
                //}
                //else if (item.approvedIntake1 > 0)
                //{
                //    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>YES</td>";
                //}
                //else
                //{
                //    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>NO</td>";
                //}

                faculty += "</tr>";

                if (minimumRequirementMet == "YES" && item.Degree == "B.Pharmacy")
                {
                    if (rFaculty <= adjustedFaculty)
                        minimumRequirementMet = "NO";
                    else
                        minimumRequirementMet = "YES";
                }

                else if (minimumRequirementMet == "NO" && item.Degree == "B.Pharmacy")
                {
                    if (rFaculty == adjustedFaculty)
                        minimumRequirementMet = "NO";
                    else
                        minimumRequirementMet = "YES";
                }

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
                }
                else
                {
                    // newFaculty.TotalIntake = item.totalIntake;
                    newFaculty.Required = (int)Math.Ceiling(item.requiredFaculty);
                }

                newFaculty.Available = adjustedFaculty;
                if (item.Degree != "B.Pharmacy" && Allgroupscount > 0)
                {
                    newFaculty.Deficiency = "YES";
                }
                else if (item.Degree != "B.Pharmacy" && Allgroupscount == 0)
                {
                    newFaculty.Deficiency = minimumRequirementMet;
                }

                if (item.Degree == "B.Pharmacy")
                {
                    newFaculty.Required = (int)Math.Ceiling(BpharmacyrequiredFaculty);
                    newFaculty.Available = (int)Math.Ceiling(BpharmacyrequiredFaculty) - Allgroupscount;
                    newFaculty.Deficiency = Allgroupscount > 0 ? "YES" : "NO";
                }


                if (item.PharmacyspecializationWiseFaculty >= 1 && degreeType.Equals("PG") && item.approvedIntake1 > 0)
                {
                    newFaculty.PhdDeficiency = "NO";
                }
                else if (degreeType.Equals("PG") && item.approvedIntake1 > 0)
                {
                    newFaculty.PhdDeficiency = "YES";
                }
                else
                {
                    newFaculty.PhdDeficiency = "-";
                }
                if (specloop == 1)
                    lstFaculty.Add(newFaculty);

                deptloop++;
                specloop++;
            }


            faculty += "</table>";

            faculty += "<table width='100%' border='0' cellpadding='5' cellspacing='0'>";
            faculty += "<tr>";
            faculty += "<td align='left'>* II, III & IV Year for B.Pharmacy";
            faculty += "<td align='left'>* I, II Year for M.Pharmacy";
            faculty += "<td align='left'>* I, II, III , IV & V Year for Pharm.D";
            faculty += "<td align='left'>* IV, V Year for Pharm.D PB";
            faculty += "</tr>";
            faculty += "</table>";

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
                //var jntuh_Bpharmacy_faculty_deficiency = db.jntuh_bpharmacy_faculty_deficiency.Where(fd => fd.CollegeId == collegeId).ToList();
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

                int pharmacyDeptId = db.jntuh_department.Where(d => d.departmentName == "Pharmacy").Select(d => d.id).FirstOrDefault();
                var collegeaffliations = db.jntuh_college_affliationstatus.AsNoTracking().ToList();
                var jntuh_education_category = db.jntuh_education_category.ToList();

                var registeredFaculty = principalRegno != null ? db.jntuh_registered_faculty.Where(rf => strRegnos.Contains(rf.RegistrationNumber.Trim()) && rf.RegistrationNumber != principalRegno).ToList()
                    : db.jntuh_registered_faculty.Where(rf => strRegnos.Contains(rf.RegistrationNumber.Trim())).ToList();
                var scm = registeredFaculty.Where(i => i.NoSCM == true).ToList();
                //Reg nos related online facultyIds
                var jntuh_registered_faculty1 = registeredFaculty.Where(rf => ((rf.Absent != true) && (rf.NotQualifiedAsperAICTE != true)
                                                        && (rf.NoSCM != true) && (rf.PANNumber != null) && (rf.PHDundertakingnotsubmitted != true)
                                                        && (rf.Notin116 != true) && (rf.Blacklistfaculy != true))).Select(rf => new
                                                        {
                                                            RegistrationNumber = rf.RegistrationNumber,
                                                            Department = rf.jntuh_department != null ? rf.jntuh_department.departmentName : null,
                                                            HighestDegreeID = rf.jntuh_registered_faculty_education.Count() != 0 ? rf.jntuh_registered_faculty_education.Select(e => e.educationId).Max() : 0,
                                                            IsApproved = rf.isApproved,
                                                            PanNumber = rf.PANNumber,
                                                            AadhaarNumber = rf.AadhaarNumber,
                                                            jntuh_registered_faculty_education = rf.jntuh_registered_faculty_education
                                                        }).ToList();
                jntuh_registered_faculty1 = jntuh_registered_faculty1.Where(e => e.HighestDegreeID >= 4).ToList();
                var jntuh_registered_faculty = jntuh_registered_faculty1.Select(rf => new
                {
                    RegistrationNumber = rf.RegistrationNumber,
                    Department = rf.Department,
                    HighestDegree = jntuh_education_category.Where(c => c.id == rf.HighestDegreeID).Select(c => c.educationCategoryName).FirstOrDefault(),
                    Recruitedfor = jntuh_college_faculty_registered.Where(c => c.RegistrationNumber == rf.RegistrationNumber).Select(c => c.IdentifiedFor).FirstOrDefault(),
                    SpecializationId = jntuh_college_faculty_registered.Where(c => c.RegistrationNumber == rf.RegistrationNumber).Select(c => c.SpecializationId).FirstOrDefault(),
                    PanNumber = rf.PanNumber,
                    AadhaarNumber = rf.AadhaarNumber,
                    registered_faculty_specialization = rf.jntuh_registered_faculty_education.Where(e => e.jntuh_education_category.educationCategoryName == "PG").FirstOrDefault().specialization != null ? rf.jntuh_registered_faculty_education.Where(e => e.jntuh_education_category.educationCategoryName == "PG").Select(i => i.specialization).FirstOrDefault().ToUpper().Trim() : ""
                }).ToList();


                var bpharmacyintake = 0;
                decimal BpharcyrequiredFaculty = 0;
                decimal PharmDrequiredFaculty = 0;
                decimal PharmDPBrequiredFaculty = 0;
                var pharmacydeptids = new[] { 26, 27, 36, 39 };
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
                    intakedetails.approvedIntake1 = GetIntake(item.collegeId, AY1, item.specializationId, item.shiftId, 1);
                    intakedetails.approvedIntake2 = GetIntake(item.collegeId, AY2, item.specializationId, item.shiftId, 1);
                    intakedetails.approvedIntake3 = GetIntake(item.collegeId, AY3, item.specializationId, item.shiftId, 1);
                    intakedetails.approvedIntake4 = GetIntake(item.collegeId, AY4, item.specializationId, item.shiftId, 1);
                    intakedetails.approvedIntake5 = GetIntake(item.collegeId, AY5, item.specializationId, item.shiftId, 1);
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
                        intakedetails.requiredFaculty = Math.Round(facultyRatio, 2);
                        var total = intakedetails.totalIntake > 400 ? 100 : 60;
                        bpharmacyintake = total;
                        ApprovedIntake = intakedetails.approvedIntake1;
                        specializationId = intakedetails.specializationId;
                    }
                    else if (item.Degree == "M.Pharmacy")
                    {
                        intakedetails.totalIntake = (intakedetails.approvedIntake1) + (intakedetails.approvedIntake2);
                        facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) / 12;
                        intakedetails.requiredFaculty = Math.Round(facultyRatio, 2);

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
                                    intakedetails.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.SpecializationId == item.specializationId || f.registered_faculty_specialization == "Hospital & Clinical Pharmacy".ToUpper()));
                                    phdFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.SpecializationId == item.specializationId || f.registered_faculty_specialization == "Hospital & Clinical Pharmacy".ToUpper()));
                                    break;
                                case 116://Pharmaceutical Analysis & Quality Assurance
                                    intakedetails.PharmacySpec1 = "1 PHD & Specialized in PAQA/QA/PA RA/Pharma Chemistry";
                                    //intakedetails.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.SpecializationId == item.specializationId || f.registered_faculty_specialization == "PAQA".ToUpper() || f.registered_faculty_specialization == "PA & QA".ToUpper() || f.registered_faculty_specialization == "QA" || f.registered_faculty_specialization == "PA RA" || f.registered_faculty_specialization == "Pharmaceutical Chemistry".ToUpper() || f.registered_faculty_specialization.Contains("QAPRA") || f.registered_faculty_specialization == "Pharmaceutical Analysis & Quality Assurance".ToUpper()));
                                    intakedetails.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.SpecializationId == item.specializationId || f.registered_faculty_specialization == "Pharmaceutical Analysis & Quality Assurance".ToUpper()));
                                    phdFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.SpecializationId == item.specializationId || f.registered_faculty_specialization == "Pharmaceutical Analysis & Quality Assurance".ToUpper()));
                                    break;
                                case 118://Pharmaceutical Management & Regulatory Affaires
                                    intakedetails.PharmacySpec1 = "1 PHD & Specialized in PAQA/QA/PMRA/Regulatory Affairs/Pharmaceutics";
                                    //intakedetails.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.SpecializationId == item.specializationId || f.registered_faculty_specialization == "PAQA".ToUpper() || f.registered_faculty_specialization == "PA & QA".ToUpper() || f.registered_faculty_specialization == "QA".ToUpper() || f.registered_faculty_specialization == "PMRA".ToUpper() || f.registered_faculty_specialization == "Regulatory Affairs".ToUpper() || f.registered_faculty_specialization == "Pharmaceutics".ToUpper() || f.registered_faculty_specialization == "Pharmaceutical Management & Regulatory Affaires".ToUpper()));
                                    intakedetails.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.SpecializationId == item.specializationId || f.registered_faculty_specialization == "Pharmaceutical Management & Regulatory Affaires".ToUpper()));
                                    phdFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.SpecializationId == item.specializationId || f.registered_faculty_specialization == "Pharmaceutical Management & Regulatory Affaires".ToUpper()));
                                    break;
                                case 120://Pharmaceutics
                                    intakedetails.PharmacySpec1 = "1 PHD & Specialized in Pharmaceutics/Pharmaceutical Technology/Industrial Pharmacy/Pharma Biotechnology";
                                    //intakedetails.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.SpecializationId == item.specializationId || f.registered_faculty_specialization == "Pharmaceutics".ToUpper() || f.registered_faculty_specialization == "Pharmaceutical Technology".ToUpper() || f.registered_faculty_specialization == "Industrial Pharmacy".ToUpper() || f.registered_faculty_specialization == "Pharmacy Biotechnology".ToUpper() || f.registered_faculty_specialization.Contains("Biotechnology".ToUpper()) || f.registered_faculty_specialization == "Pharmaceutics".ToUpper()));
                                    intakedetails.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.SpecializationId == item.specializationId || f.registered_faculty_specialization == "Pharmaceutics".ToUpper()));
                                    phdFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.SpecializationId == item.specializationId || f.registered_faculty_specialization == "Pharmaceutics".ToUpper()));
                                    break;
                                case 122://Pharmacology
                                    intakedetails.PharmacySpec1 = "1 PHD & Specialized in Pharmacology/Pharmacognosy/HCP/Pharma Practice";
                                    //intakedetails.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.SpecializationId == item.specializationId || f.registered_faculty_specialization == "Pharmacology".ToUpper() || f.registered_faculty_specialization == "Pharmacognosy".ToUpper() || f.registered_faculty_specialization == "HCP".ToUpper() || f.registered_faculty_specialization == "Pharmacy Practice".ToUpper() || f.registered_faculty_specialization.Contains("HOSPITAL".ToUpper()) || f.registered_faculty_specialization == "Pharmacology".ToUpper()));
                                    intakedetails.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.SpecializationId == item.specializationId || f.registered_faculty_specialization == "Pharmacology".ToUpper()));
                                    phdFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.SpecializationId == item.specializationId || f.registered_faculty_specialization == "Pharmacology".ToUpper()));
                                    break;
                                case 124://Quality Assurance & Pharma Regulatory Affairs
                                    intakedetails.PharmacySpec1 = "1 PHD & Specialized in PAQA/QA/PA RA/Pharmaceuticalchemistry";
                                    var s = jntuh_registered_faculty.Where(f => (f.registered_faculty_specialization == "PAQA".ToUpper() || f.registered_faculty_specialization == "PA & QA".ToUpper() ||
                                                 f.registered_faculty_specialization == "QA".ToUpper() ||
                                                 f.registered_faculty_specialization == "PA RA".ToUpper() ||
                                                 f.registered_faculty_specialization == "Pharmaceutical Chemistry".ToUpper() || f.registered_faculty_specialization.Contains("QAPRA"))).ToList();
                                    //intakedetails.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.SpecializationId == item.specializationId || f.registered_faculty_specialization == "PAQA".ToUpper() || f.registered_faculty_specialization == "PA & QA".ToUpper() || f.registered_faculty_specialization == "QA".ToUpper() || f.registered_faculty_specialization == "PA RA".ToUpper() || f.registered_faculty_specialization == "Pharmaceutical Chemistry".ToUpper() || f.registered_faculty_specialization.Contains("QAPRA") || f.registered_faculty_specialization == "Quality Assurance & Pharma Regulatory Affairs".ToUpper()));
                                    intakedetails.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.SpecializationId == item.specializationId || f.registered_faculty_specialization == "Quality Assurance & Pharma Regulatory Affairs".ToUpper()));
                                    phdFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.SpecializationId == item.specializationId || f.registered_faculty_specialization == "Quality Assurance & Pharma Regulatory Affairs".ToUpper()));
                                    break;
                                case 115://Industrial Pharmacy
                                    intakedetails.PharmacySpec1 = "1 PHD & Specialized in Pharmaceutics/Pharmaceutical Technology/Industrial Pharmacy/Pharma Biotechnology";
                                    //intakedetails.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.SpecializationId == item.specializationId || f.registered_faculty_specialization == "Pharmaceutics".ToUpper() || f.registered_faculty_specialization == "Pharmaceutical Technology".ToUpper() || f.registered_faculty_specialization == "Industrial Pharmacy".ToUpper() || f.registered_faculty_specialization == "Pharmacy Biotechnology".ToUpper() || f.registered_faculty_specialization.Contains("Biotechnology".ToUpper())));
                                    intakedetails.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.SpecializationId == item.specializationId || f.registered_faculty_specialization == "Industrial Pharmacy".ToUpper()));
                                    phdFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.SpecializationId == item.specializationId || f.registered_faculty_specialization == "Industrial Pharmacy".ToUpper()));
                                    break;
                                case 121://Pharmacognosy
                                    intakedetails.PharmacySpec1 = "1 PHD & Specialized in Pharmacognosy/Pharma Biotechnology/Pharmacology";
                                    //intakedetails.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.SpecializationId == item.specializationId || f.registered_faculty_specialization == "Pharmacology".ToUpper() || f.registered_faculty_specialization == "Pharmacognosy".ToUpper() || f.registered_faculty_specialization == "Pharmacy Biotechnology".ToUpper() || f.registered_faculty_specialization.Contains("Biotechnology".ToUpper())));
                                    intakedetails.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.SpecializationId == item.specializationId || f.registered_faculty_specialization == "Pharmacognosy".ToUpper()));
                                    phdFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.SpecializationId == item.specializationId || f.registered_faculty_specialization == "Pharmacognosy".ToUpper()));
                                    break;
                                case 117://Pharmaceutical Chemistry
                                    intakedetails.PharmacySpec1 = "1 PHD & Specialized in PAQA/QA/Pharmaceuticalchemistry";
                                    //intakedetails.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.SpecializationId == item.specializationId || f.registered_faculty_specialization == "PAQA".ToUpper() || f.registered_faculty_specialization == "PA & QA".ToUpper() || f.registered_faculty_specialization == "QA".ToUpper() || f.registered_faculty_specialization == "Pharmaceutical Chemistry".ToUpper() || f.registered_faculty_specialization.Contains("QAPRA")));
                                    intakedetails.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.SpecializationId == item.specializationId || f.registered_faculty_specialization == "Pharmaceutical Chemistry".ToUpper()));
                                    phdFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.SpecializationId == item.specializationId || f.registered_faculty_specialization == "Pharmaceutical Chemistry".ToUpper()));
                                    break;
                                case 119://Pharmaceutical Technology (2011-12)
                                    intakedetails.PharmacySpec1 = "1 PHD & Specialized in Pharmaceutics/Pharmaceutical Technology/Industrial Pharmacy/Pharma Biotechnology";
                                    //intakedetails.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.SpecializationId == item.specializationId || f.registered_faculty_specialization == "Pharmaceutics".ToUpper() || f.registered_faculty_specialization == "Pharmaceutical Technology".ToUpper() || f.registered_faculty_specialization == "Industrial Pharmacy".ToUpper() || f.registered_faculty_specialization == "Pharmacy Biotechnology".ToUpper() || f.registered_faculty_specialization.Contains("Biotechnology".ToUpper()) || f.registered_faculty_specialization.Contains("Bio-Technology".ToUpper()) || f.registered_faculty_specialization.Contains("Pharmaceutical Technology (2011-12)".ToUpper())));
                                    intakedetails.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.SpecializationId == item.specializationId || f.registered_faculty_specialization == "Pharmaceutical Technology (2011-12)".ToUpper()));
                                    phdFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.SpecializationId == item.specializationId || f.registered_faculty_specialization == "Pharmaceutical Technology (2011-12)".ToUpper()));
                                    break;
                                case 123://Quality Assurance
                                    intakedetails.PharmacySpec1 = "1 PHD & Specialized in PAQA/QA/PA RA/Pharmaceuticalchemistry";
                                    //intakedetails.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.SpecializationId == item.specializationId || f.registered_faculty_specialization == "PAQA".ToUpper() || f.registered_faculty_specialization == "PA & QA".ToUpper() || f.registered_faculty_specialization == "QA".ToUpper() || f.registered_faculty_specialization == "PA RA".ToUpper() || f.registered_faculty_specialization == "Pharmaceutical Chemistry".ToUpper() || f.registered_faculty_specialization.Contains("QAPRA")));
                                    intakedetails.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.SpecializationId == item.specializationId || f.registered_faculty_specialization == "Quality Assurance".ToUpper()));
                                    phdFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.SpecializationId == item.specializationId || f.registered_faculty_specialization == "Quality Assurance".ToUpper()));
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
                    //intakedetails.id = jntuh_Bpharmacy_faculty_deficiency.Where(fd => fd.CollegeId == collegeId && fd.SpecializationId == item.specializationId && fd.ShiftId == item.shiftId).Select(fd => fd.Id).FirstOrDefault();

                    if (intakedetails.id > 0)
                    {
                        //int? swf = jntuh_Bpharmacy_faculty_deficiency.Where(fd => fd.Id == intakedetails.id).Select(fd => fd.SpecializationWiseFaculty).FirstOrDefault();
                        //if (swf != null)
                        //{
                        //    intakedetails.specializationWiseFaculty = (int)swf;
                        //}
                        //intakedetails.deficiency = jntuh_Bpharmacy_faculty_deficiency.Where(fd => fd.Id == intakedetails.id).Select(fd => fd.Deficiency).FirstOrDefault();
                        //intakedetails.shortage = jntuh_Bpharmacy_faculty_deficiency.Where(fd => fd.Id == intakedetails.id).Select(fd => fd.Shortage).FirstOrDefault();
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
                        PharmacyspecName = "Group1 (Pharmacy Practice, Pharm D)",
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
                        PharmacyspecName = "Group1 (Pharmacy Practice, Pharm D)",
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
                        PharmacyspecName = "Group1 (Pharmaceutics)",//, Industrial Pharmacy, Pharmacy BioTechnology, Pharmaceutical Technology
                        Specialization = "B.Pharmacy"
                    },
                    new PharmacySpecilaizationList()
                    {
                        PharmacyspecName = "Group2 (Pharmaceutical Chemistry,Pharmacy Analysis, PAQA)",
                        Specialization = "B.Pharmacy"
                    },
                    //new PharmacySpecilaizationList()
                    //{
                    //    PharmacyspecName = "Group3 (Pharmacy Analysis, PAQA)",
                    //    Specialization = "B.Pharmacy"
                    //},
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
                    //new PharmacySpecilaizationList()
                    //{
                    //    PharmacyspecName = "Group6 (English, Mathematics, Computers)",
                    //    Specialization = "B.Pharmacy"
                    //},
                };


                TotalcollegeFaculty = jntuh_registered_faculty.Count;

                #region All B.Pharmacy Specializations

                var reg_facultyspecilaizationsdistinct = jntuh_registered_faculty.Select(i => i.registered_faculty_specialization).Distinct().ToArray();
                var group1Subcount = 0; var group2Subcount = 0; var group3Subcount = 0; var group4Subcount = 0; var group5Subcount = 0; var group6Subcount = 0;
                var pharmadgroup1Subcount = 0; var pharmadPBgroup1Subcount = 0;
                string subgroupconditionsmet;
                string conditionbpharm = null;
                string conditionpharmd = null;
                string conditionphardpb = null;
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

                    bpharmacylist.PharmacySpecilaizationList = pharmacyspeclist;
                    bpharmacylist.BphramacyrequiredFaculty = BpharcyrequiredFaculty;
                    bpharmacylist.SortId = 1;
                    bpharmacylist.approvedIntake1 = ApprovedIntake;
                    bpharmacylist.specializationId = specializationId;
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





                    if (list.PharmacyspecName == "Group1 (Pharmaceutics)" || list.PharmacyspecName == "Pharmaceutics" || list.PharmacyspecName == "Industrial Pharmacy" || list.PharmacyspecName == "Pharmacy BioTechnology" || list.PharmacyspecName == "Pharmaceutical Technology")
                    {
                        group1Subcount = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "Pharmaceutics".ToUpper());
                        //jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "Industrial Pharmacy".ToUpper()) +
                        //jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "Pharmacy BioTechnology".ToUpper()) +
                        //jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "Bio-Technology".ToUpper()) +
                        //jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "Pharmaceutical Technology (2011-12)".ToUpper()) +
                        //jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "Pharmaceutical Technology".ToUpper());
                        bpharmacylist.BPharmacySubGroup1Count = group1Subcount;
                        bpharmacylist.BPharmacySubGroupRequired = bpharmacyintake >= 100 ? 6 : 4;
                        bpharmacylist.PharmacySubGroup1 = "Group1 (Pharmaceutics)";
                        bpharmacylist.requiredFaculty = bpharmacyintake >= 100 ? 6 : 4;
                    }

                    else if (list.PharmacyspecName == "Group2 (Pharmaceutical Chemistry,Pharmacy Analysis, PAQA)" || list.PharmacyspecName == "Pharmaceutical Chemistry")
                    {
                        group2Subcount = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "Pharmaceutical Chemistry".ToUpper()) +
                                        jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "Pharmacy Analysis".ToUpper()) +
                                        jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "PAQA".ToUpper()) +
                                        jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "PA & QA".ToUpper()) +
                                        jntuh_registered_faculty.Count(f => f.registered_faculty_specialization.Contains("QAPRA")) +
                                        jntuh_registered_faculty.Count(f => f.registered_faculty_specialization.Contains("Pharmaceutical Analysis & Quality Assurance".ToUpper())) +
                                        jntuh_registered_faculty.Count(f => f.registered_faculty_specialization.Contains("Quality Assurance & Pharma Regulatory Affairs".ToUpper()));
                        bpharmacylist.BPharmacySubGroup1Count = group2Subcount;
                        bpharmacylist.BPharmacySubGroupRequired = bpharmacyintake >= 100 ? 6 : 5;
                        bpharmacylist.PharmacySubGroup1 = "Group2 (Pharmaceutical Chemistry,Pharmacy Analysis, PAQA)";
                        bpharmacylist.requiredFaculty = bpharmacyintake >= 100 ? 6 : 5;
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

                    else if (list.PharmacyspecName == "Group3 (Pharmacology)" || list.PharmacyspecName == "Pharmacology" || list.PharmacyspecName == "Pharma D" || list.PharmacyspecName == "Pharma.D" || list.PharmacyspecName == "Pharm D" || list.PharmacyspecName == "Pharm.D")
                    {
                        group3Subcount = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "Pharmacology".ToUpper());
                        bpharmacylist.BPharmacySubGroup1Count = group3Subcount;
                        bpharmacylist.BPharmacySubGroupRequired = bpharmacyintake >= 100 ? 5 : 4;
                        bpharmacylist.PharmacySubGroup1 = "Group3 (Pharmacology)";
                        bpharmacylist.requiredFaculty = bpharmacyintake >= 100 ? 5 : 4;
                    }

                    else if (list.PharmacyspecName == "Group4 (Pharmacognosy)" || list.PharmacyspecName == "Pharmacognosy")
                    {
                        group4Subcount = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "Pharmacognosy".ToUpper()) +
                            jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "Pharmacognosy&Phytochemistryc".ToUpper()) +
                            jntuh_registered_faculty.Count(f => f.registered_faculty_specialization.Contains("PHARMACOGNOSY & PHYTOCHEMISTRY".ToUpper()));
                        bpharmacylist.BPharmacySubGroup1Count = group4Subcount;
                        bpharmacylist.BPharmacySubGroupRequired = 3;
                        bpharmacylist.PharmacySubGroup1 = "Group4 (Pharmacognosy)";
                        bpharmacylist.requiredFaculty = 3;
                    }

                    //else if (list.PharmacyspecName == "Group6 (English, Mathematics, Computers)" || list.PharmacyspecName == "English" || list.PharmacyspecName == "Mathematics" || list.PharmacyspecName == "Computers" || list.PharmacyspecName == "Computer Science")//|| list.PharmacyspecName == "Zoology"
                    //{
                    //    group6Subcount = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "English".ToUpper()) +
                    //        jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "Mathematics".ToUpper()) +
                    //        jntuh_registered_faculty.Count(f => f.registered_faculty_specialization.Contains("COMPUTER")) +
                    //        jntuh_registered_faculty.Count(f => f.registered_faculty_specialization.Contains("COMPUTER SCIENCE")) +
                    //        jntuh_registered_faculty.Count(f => f.registered_faculty_specialization.Contains("CSE"));
                    //    //jntuh_registered_faculty.Count(f => f.registered_faculty_specialization.Contains("ZOOLOGY"));
                    //    bpharmacylist.BPharmacySubGroup1Count = group6Subcount;
                    //    bpharmacylist.BPharmacySubGroupRequired = bpharmacyintake == 100 ? 3 : 2;
                    //    bpharmacylist.PharmacySubGroup1 = "Group6 (English, Mathematics, Computers)";
                    //}



                    //var id = jntuh_Bpharmacy_faculty_deficiency.Where(fd => fd.CollegeId == collegeId && fd.BpharmacySpecialization == list.PharmacyspecName && fd.ShiftId == 1).Select(fd => fd.Id).FirstOrDefault();
                    //if (id > 0)
                    //{
                    //    var swf = jntuh_Bpharmacy_faculty_deficiency.Where(fd => fd.Id == id).Select(fd => fd.SpecializationWiseFaculty).FirstOrDefault();
                    //    if (swf != null)
                    //    {
                    //        bpharmacylist.specializationWiseFaculty = (int)swf;
                    //    }
                    //    bpharmacylist.id = id;
                    //    bpharmacylist.deficiency = jntuh_Bpharmacy_faculty_deficiency.Where(fd => fd.Id == id).Select(fd => fd.Deficiency).FirstOrDefault();
                    //    bpharmacylist.shortage = jntuh_Bpharmacy_faculty_deficiency.Where(fd => fd.Id == id).Select(fd => fd.Shortage).FirstOrDefault();
                    //}

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
                        bpharmacylist.approvedIntake1 = PharmaDApprovedIntake;
                        bpharmacylist.specializationId = PharmaDspecializationId;
                        #region pharmadSpecializationcount
                        if (list.PharmacyspecName == "Group1 (Pharmacy Practice, Pharm D)" || list.PharmacyspecName == "Pharm D")
                        {
                            bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper() ||
                                           f.registered_faculty_specialization == "PharmD".ToUpper() ||
                                          f.registered_faculty_specialization == "Pharm D".ToUpper() ||
                                          f.registered_faculty_specialization == "Pharma.D".ToUpper() ||
                                          f.registered_faculty_specialization == "Pharma D".ToUpper());
                        }
                        else if (list.PharmacyspecName == "Group1 (Pharmacy Practice, Pharm D)" || list.PharmacyspecName == "Pharmacy Practice")
                        {
                            bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper());
                        }
                        #endregion


                        if (list.PharmacyspecName == "Group1 (Pharmacy Practice, Pharm D)" || list.PharmacyspecName == "Pharmacy Practice" || list.PharmacyspecName == "Pharm D" || list.PharmacyspecName == "Pharm.D")
                        {
                            pharmadgroup1Subcount = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "Pharmacy Practice".ToUpper()) +
                                        jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "Pharma D".ToUpper()) +
                                        jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "Pharma.D".ToUpper()) +
                                        jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "Pharm D".ToUpper()) +
                                        jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "Pharm.D".ToUpper()) +
                                        jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "PharmD".ToUpper());
                            bpharmacylist.BPharmacySubGroup1Count = pharmadgroup1Subcount;
                            bpharmacylist.BPharmacySubGroupRequired = pharmadTotalintake / 30;
                            bpharmacylist.PharmacySubGroup1 = "Group1 (Pharmacy Practice, Pharm D)";
                            bpharmacylist.requiredFaculty = Math.Ceiling(PharmDrequiredFaculty);
                        }


                        //var id = jntuh_Bpharmacy_faculty_deficiency.Where(fd => fd.CollegeId == collegeId && fd.BpharmacySpecialization == list.PharmacyspecName && fd.ShiftId == 1).Select(fd => fd.Id).FirstOrDefault();
                        //if (id > 0)
                        //{
                        //    var swf = jntuh_Bpharmacy_faculty_deficiency.Where(fd => fd.Id == id).Select(fd => fd.SpecializationWiseFaculty).FirstOrDefault();
                        //    if (swf != null)
                        //    {
                        //        bpharmacylist.specializationWiseFaculty = (int)swf;
                        //    }
                        //    bpharmacylist.id = id;
                        //    bpharmacylist.deficiency = jntuh_Bpharmacy_faculty_deficiency.Where(fd => fd.Id == id).Select(fd => fd.Deficiency).FirstOrDefault();
                        //    bpharmacylist.shortage = jntuh_Bpharmacy_faculty_deficiency.Where(fd => fd.Id == id).Select(fd => fd.Shortage).FirstOrDefault();
                        //}

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
                        bpharmacylist.approvedIntake1 = PharmaDPBApprovedIntake;
                        bpharmacylist.specializationId = PharmaDPBspecializationId;
                        #region pharmadPbSpecializationcount
                        if (list.PharmacyspecName == "Group1 (Pharmacy Practice, Pharm D)" || list.PharmacyspecName == "Pharm D")
                        {
                            bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper() ||
                                           f.registered_faculty_specialization == "PharmD".ToUpper() ||
                                          f.registered_faculty_specialization == "Pharm D".ToUpper() ||
                                          f.registered_faculty_specialization == "Pharma.D".ToUpper() ||
                                          f.registered_faculty_specialization == "Pharma D".ToUpper());
                        }
                        else if (list.PharmacyspecName == "Group1 (Pharmacy Practice, Pharm D)" || list.PharmacyspecName == "Pharmacy Practice")
                        {
                            bpharmacylist.PharmacyspecializationWiseFaculty = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == list.PharmacyspecName.ToUpper());
                        }
                        #endregion


                        if (list.PharmacyspecName == "Group1 (Pharmacy Practice, Pharm D)" || list.PharmacyspecName == "Pharmacy Practice" || list.PharmacyspecName == "Pharm D" || list.PharmacyspecName == "Pharm.D")
                        {
                            pharmadPBgroup1Subcount = jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "Pharmacy Practice".ToUpper()) +
                                            jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "Pharma D".ToUpper()) +
                                            jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "Pharma.D".ToUpper()) +
                                            jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "Pharm D".ToUpper()) +
                                            jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "Pharm.D".ToUpper()) +
                                        jntuh_registered_faculty.Count(f => f.registered_faculty_specialization == "PharmD".ToUpper()); ;
                            bpharmacylist.BPharmacySubGroup1Count = pharmadPBgroup1Subcount;
                            bpharmacylist.BPharmacySubGroupRequired = pharmadPBTotalintake / 10;
                            bpharmacylist.PharmacySubGroup1 = "Group1 (Pharmacy Practice, Pharm D)";
                            bpharmacylist.requiredFaculty = Math.Ceiling(PharmDPBrequiredFaculty);
                        }


                        //var id = jntuh_Bpharmacy_faculty_deficiency.Where(fd => fd.CollegeId == collegeId && fd.BpharmacySpecialization == list.PharmacyspecName && fd.ShiftId == 1).Select(fd => fd.Id).FirstOrDefault();
                        //if (id > 0)
                        //{
                        //    var swf = jntuh_Bpharmacy_faculty_deficiency.Where(fd => fd.Id == id).Select(fd => fd.SpecializationWiseFaculty).FirstOrDefault();
                        //    if (swf != null)
                        //    {
                        //        bpharmacylist.specializationWiseFaculty = (int)swf;
                        //    }
                        //    bpharmacylist.id = id;
                        //    bpharmacylist.deficiency = jntuh_Bpharmacy_faculty_deficiency.Where(fd => fd.Id == id).Select(fd => fd.Deficiency).FirstOrDefault();
                        //    bpharmacylist.shortage = jntuh_Bpharmacy_faculty_deficiency.Where(fd => fd.Id == id).Select(fd => fd.Shortage).FirstOrDefault();
                        //}

                        intakedetailsList.Add(bpharmacylist);
                    }
                }

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
                        if (group1Subcount >= (bpharmacyintake >= 100 ? 6 : 4) && group2Subcount >= (bpharmacyintake >= 100 ? 6 : 5) && group3Subcount >= (bpharmacyintake >= 100 ? 5 : 4) && group4Subcount >= 3)//&& group5Subcount >= 2 && group6Subcount >= (bpharmacyintake == 100 ? 3 : 2)
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
                            if (group1Subcount >= (bpharmacyintake >= 100 ? 6 : 4) && group2Subcount >= (bpharmacyintake >= 100 ? 6 : 5) && group3Subcount >= (bpharmacyintake >= 100 ? 5 : 4) && group4Subcount >= 3)//&& group5Subcount >= 2 && group6Subcount >= (bpharmacyintake == 100 ? 3 : 2)
                            {
                                Allgroupscount = 0;
                            }
                            else
                            {
                                //var bpharmacyIntake = (bpharmacyintake >= 100 ? 3 : 2);
                                if (group1Subcount < (bpharmacyintake >= 100 ? 6 : 4))
                                {
                                    var count = (bpharmacyintake >= 100 ? 6 : 4) - group1Subcount;
                                    Allgroupscount = Allgroupscount + count;
                                }
                                if (group2Subcount < (bpharmacyintake >= 100 ? 6 : 5))
                                {
                                    var count = (bpharmacyintake >= 100 ? 6 : 5) - group2Subcount;
                                    Allgroupscount = Allgroupscount + count;
                                }
                                if (group3Subcount < (bpharmacyintake >= 100 ? 5 : 4))
                                {
                                    var count = (bpharmacyintake >= 100 ? 5 : 4) - group3Subcount;
                                    Allgroupscount = Allgroupscount + count;
                                }
                                if (group4Subcount < 3)
                                {
                                    var count = 3 - group4Subcount;
                                    count = count == 1 ? 0 : count;
                                    Allgroupscount = Allgroupscount + count;
                                }
                            }
                            facultydefcount = Allgroupscount;
                        }

                        else if (jntuh_registered_faculty.Count < BpharcyrequiredFaculty && (item.Degree == "B.Pharmacy"))
                        {
                            if (group1Subcount >= (bpharmacyintake >= 100 ? 6 : 4) && group2Subcount >= (bpharmacyintake >= 100 ? 6 : 5) && group3Subcount >= (bpharmacyintake >= 100 ? 5 : 4) && group4Subcount >= 3)//&& group5Subcount >= 2 && group6Subcount >= (bpharmacyintake == 100 ? 3 : 2)
                            {
                                Allgroupscount = 0;
                            }
                            else
                            {
                                //var bpharmacyIntake = (bpharmacyintake >= 100 ? 3 : 2);
                                if (group1Subcount < (bpharmacyintake >= 100 ? 6 : 4))
                                {
                                    var count = (bpharmacyintake >= 100 ? 6 : 4) - group1Subcount;
                                    Allgroupscount = Allgroupscount + count;
                                }
                                if (group2Subcount < (bpharmacyintake >= 100 ? 6 : 5))
                                {
                                    var count = (bpharmacyintake >= 100 ? 6 : 5) - group2Subcount;
                                    Allgroupscount = Allgroupscount + count;
                                }
                                if (group3Subcount < (bpharmacyintake >= 100 ? 5 : 4))
                                {
                                    var count = (bpharmacyintake >= 100 ? 5 : 4) - group3Subcount;
                                    Allgroupscount = Allgroupscount + count;
                                }
                                if (group4Subcount < 3)
                                {
                                    var count = 3 - group4Subcount;
                                    count = count == 1 ? 0 : count;
                                    Allgroupscount = Allgroupscount + count;
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



        #region BASNotice Code Written by Siva

        #region BasNotice Parameters Written by Siva
        private string[] Non_HighestDegreeBasedRegNos;
        private int PHD_Count;
        private int PHDPresentCount;
        private int TotalCount;
        private int TotalCount1;
        private int DifferenceCount;
        private string MonthNameNew;
        private int facultyRatiocount;
        private int Btech_Faculty_ProposedCount;
        private int Mtech_Faculty_ProposedCount;
        private int MBA_Faculty_ProposedCount;
        private int MCA_Faculty_ProposedCount;
        private int BPharmacy_Faculty_ProposedCount;
        private int Mpharmacy_Faculty_ProposedCount;
        private int Pharm_D_Faculty_ProposedCount;
        private int Pharm_DPB_Faculty_ProposedCount;
        private int MAM_Faculty_ProposedCount;
        private int MTM_Faculty_ProposedCount;
        private int facultyCountnew;
        private int facultyCountnew1;
        private int facultyCountnew2;
        private int BTech_PHD_Count_New;
        private int MTech_PHD_Count_New;
        private int BPharmacy_PHD_Count_New;
        private int MPharmacy_PHD_Count_New;
        private int PharmD_PHD_Count_New;
        private int PharmDPB_PHD_Count_New;
        private int MCA_PHD_Count_New;
        private int MBA_PHD_Count_New;
        private int MAM_PHD_Count_New;
        private int Bas_NOTPhd_Count = 0;
        private int Bas_Phd_Count = 0;
        private bool CheckPercentage = false;
        private int monthCount;


        #endregion


        public ActionResult BASNotice(string cid, string type)
        {
            // type = "December";
            int collegeId = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(cid, WebConfigurationManager.AppSettings["CryptoKey"]));
            var jntuh_college = db.jntuh_college.Where(c => c.id == collegeId).Select(e => e).FirstOrDefault();


            string collegeCode = jntuh_college.collegeCode.ToUpper();
            string ECollegeid = UAAAS.Models.Utilities.EncryptString(collegeId.ToString(), System.Web.Configuration.WebConfigurationManager.AppSettings["CryptoKey"]);

            List<SelectListItem> MonthNameAndIds = new List<SelectListItem>();
            MonthNameAndIds.Add(new SelectListItem() { Text = "January", Value = "1" });
            MonthNameAndIds.Add(new SelectListItem() { Text = "February", Value = "2" });
            MonthNameAndIds.Add(new SelectListItem() { Text = "March", Value = "3" });
            MonthNameAndIds.Add(new SelectListItem() { Text = "April", Value = "4" });
            MonthNameAndIds.Add(new SelectListItem() { Text = "May", Value = "5" });
            MonthNameAndIds.Add(new SelectListItem() { Text = "June", Value = "6" });
            MonthNameAndIds.Add(new SelectListItem() { Text = "July", Value = "7" });
            MonthNameAndIds.Add(new SelectListItem() { Text = "August", Value = "8" });
            MonthNameAndIds.Add(new SelectListItem() { Text = "September", Value = "9" });
            MonthNameAndIds.Add(new SelectListItem() { Text = "October", Value = "10" });
            MonthNameAndIds.Add(new SelectListItem() { Text = "November", Value = "11" });
            MonthNameAndIds.Add(new SelectListItem() { Text = "December", Value = "12" });

            type = MonthNameAndIds.Where(s => s.Value == type).Select(s => s.Text).FirstOrDefault();

            string path = BASNoticeSave(collegeId, type);


            path = path.Replace("/", "\\");
            if (!string.IsNullOrEmpty(path))
            {
                return File(path, "application/pdf", path.Substring(path.LastIndexOf('\\') + 1));
            }
            return View();
        }

        public string BASNoticeSave(int? collegeId, string type)
        {
            //type = "December";
            jntuh_college CollegeData = db.jntuh_college.Where(c => c.id == collegeId).Select(c => c).FirstOrDefault();
            string collegeCode = CollegeData.collegeCode.ToUpper();

            string ECollegeid = UAAAS.Models.Utilities.EncryptString(collegeId.ToString(), System.Web.Configuration.WebConfigurationManager.AppSettings["CryptoKey"]);

            Document pdfDoc = new Document(PageSize.A4, 60, 50, 60, 60);

            // Create a new PdfWrite object, writing the output to a MemoryStream
            //open pdf
            var output = new MemoryStream();
            var writer = PdfWriter.GetInstance(pdfDoc, output);

            string path = Server.MapPath("~/Content/PDFReports/BASNotice/");


            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);

            }
            string fullPath = path + CollegeData.collegeCode.ToUpper() + "-" + ECollegeid + "BASNotice_Month.pdf";
            // string fullPath = path + CollegeData.collegeCode.ToUpper() + "-" + ECollegeid + "BASNotice_" + type + ".pdf";
            PdfWriter pdfWriter = PdfWriter.GetInstance(pdfDoc, new FileStream(fullPath, FileMode.Create));

            var file = CollegeData.collegeCode.ToUpper() + "-" + ECollegeid + "BASNotice_Month.pdf";
            // var file = CollegeData.collegeCode.ToUpper() + "-" + ECollegeid + "BASNotice_" + type + ".pdf";
            fullPath = string.Format("{0}/{1}", "/Content/PDFReports/BASNotice", file);


            //var check = db.jntuh_college_news.Where(n => n.navigateURL == fullPath).ToList();
            //if (check.Count == 0 || check.Count == null)
            //{
            //    jntuh_college_news jntuh_college_news = new jntuh_college_news();
            //    jntuh_college_news.collegeId = CollegeData.id;
            //    jntuh_college_news.title = type + "-BASNotice" + ".PDF";
            //    jntuh_college_news.navigateURL = fullPath;
            //    jntuh_college_news.startDate = null;
            //    jntuh_college_news.endDate = null;
            //    jntuh_college_news.isActive = true;
            //    jntuh_college_news.isLatest = true;
            //    jntuh_college_news.createdOn = DateTime.Now;
            //    jntuh_college_news.createdBy = 1;
            //    jntuh_college_news.updatedOn = null;
            //    jntuh_college_news.updatedBy = null;

            //    db.jntuh_college_news.Add(jntuh_college_news);
            //    db.SaveChanges();
            //}
            //else
            //{

            //}

            //Open PDF Document to write data
            pdfDoc.Open();

            //Assign Html content in a string to write in PDF
            string contents = string.Empty;

            StreamReader sr;

            //Read file from server path
            sr = System.IO.File.OpenText(Server.MapPath("~/Content/BASNotice.html"));

            //store content in the variable
            contents = sr.ReadToEnd();
            sr.Close();

            // contents = contents.Replace("##Monthname##", type.Substring(0,3));
            contents = contents.Replace("##COLLEGE_CODE##", CollegeData.collegeCode.ToUpper());
            contents = contents.Replace("##Monthnamenew##", type);




            jntuh_address address = db.jntuh_address.Where(a => a.collegeId == CollegeData.id && (a.addressTye.Equals("COLLEGE"))).Select(a => a).FirstOrDefault();
            jntuh_address societyAddress = db.jntuh_address.Where(ad => (ad.collegeId == CollegeData.id) && (ad.addressTye.Equals("SOCIETY"))).Select(ad => ad).FirstOrDefault();


            //string PHD_Count_mew = PHD_Count.ToString();

            string district = string.Empty;
            if (address != null)
            {
                district = db.jntuh_district.Find(address.districtId).districtName;
            }
            string scheduleCollegeAddress = string.Empty;
            string scheduleCollegeAddress1 = string.Empty;



            if (address != null)
            {
                scheduleCollegeAddress = CollegeData.collegeName + ",<br />" + address.address;
                scheduleCollegeAddress = scheduleCollegeAddress + ", " + address.townOrCity;
                scheduleCollegeAddress = scheduleCollegeAddress + ", " + address.mandal;
                scheduleCollegeAddress = scheduleCollegeAddress + ",<br />" + district;
                scheduleCollegeAddress = scheduleCollegeAddress + " - " + address.pincode;
            }

            if (address != null)
            {
                scheduleCollegeAddress1 = CollegeData.collegeName + ", " + address.address;
                scheduleCollegeAddress1 = scheduleCollegeAddress1 + ", " + address.townOrCity;
                scheduleCollegeAddress1 = scheduleCollegeAddress1 + ", " + address.mandal;
                scheduleCollegeAddress1 = scheduleCollegeAddress1 + ", " + district;
                scheduleCollegeAddress1 = scheduleCollegeAddress1 + " - " + address.pincode;
            }

            string CollegeSocietyAddress = string.Empty;
            if (societyAddress != null)
            {
                CollegeSocietyAddress = CollegeData.collegeName + ", " + societyAddress.address;
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
            contents = contents.Replace("##CurrentDate##", DateTime.Now.ToString("dd-MM-yyy"));

            var dates = db.jntuh_college_edit_status.Where(d => d.collegeId == CollegeData.id).Select(d => d).FirstOrDefault();
            contents = contents.Replace("##116Submitted_DATE##", ((DateTime)dates.updatedOn).ToString("dd-MM-yyy"));

            // contents = contents.Replace("##COURSE_TABLE##", SurpriseStaff(collegeId,type));
            contents = contents.Replace("##COURSE_TABLE##", SurpriseStaffNew(collegeId, type));
            contents = contents.Replace("##Total_Faculty_AttendanceCount##", facultyCountnew.ToString());
            monthCount = monthCount == null ? 15 : monthCount;
            contents = contents.Replace(" ##DaysCount##", monthCount.ToString());
            var Difference = facultyCountnew - TotalCount1;
            //if (Difference > 0)
            //    contents = contents.Replace("##DifferenceCount##", Difference.ToString());
            //else
            //    contents = contents.Replace("##DifferenceCount##", "0");

            contents = contents.Replace("##DifferenceCount##", (facultyCountnew - TotalCount1).ToString());
            // contents = contents.Replace("##Month##", type);
            // contents = contents.Replace("##DifferenceCount##", DifferenceCount.ToString());
            contents = contents.Replace("##Total_PHD_Count##", facultyCountnew1.ToString());
            if (facultyCountnew1 == 0)
            {
                facultyCountnew1 = 1;
            }
            //if (PHDPresentCount == facultyCountnew1)
            //    contents = contents.Replace("##Total_PHD_Count##", "It is also noticed that out of <b><u>" + facultyCountnew1 + "</u></b> Ph.D faculty required <b><u>" + PHDPresentCount + "</u></b> are attending the College.");
            //else
            //    contents = contents.Replace("##Total_PHD_Count##", "It is also noticed that out of <b><u>" + facultyCountnew1 + "</u></b> Ph.D faculty required <b><u>" + PHDPresentCount + "</u></b> are attending the College.");
            if (type == "August")
            {
                contents = contents.Replace("##Text##", "");
            }
            else
            {
                contents = contents.Replace("##Text##", "However, subsequently you are given Edit option to update any changes in faculty position due to various reasons from <b><u>11-09-2017</u></b> to <b><u>19-09-2017</u></b>.");

            }
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
            string returnpath = string.Empty;
            returnpath = fullPath;
            return returnpath;

        }

        public string SurpriseStaff(int? collegeID, string type)
        {
            var year = DateTime.Now.Year.ToString();
            string StaffDetails = string.Empty;

            string MonthName = string.Empty;
            int count = 1;
            monthCount = 15;
            int facultystudentRatio = 0;
            var jntuh_faculty_student_ratio_norms = db.jntuh_faculty_student_ratio_norms.ToList();


            string[] strOtherDepartments = { "English", "Mathematics", "Physics", "Chemistry", "Others(CSE/IT)", "Others(CIVIL/MECH)", "Others(ECE/EEE)", "Others(MNGT/H&S)" };

            List<CollegeFacultyWithIntakeReport> intakedetailsList = Get_PHD_Faculty(collegeID);

            List<string> New_PHD_Count1 = new List<string>();

            PHD_Count = intakedetailsList.Where(i => i.shiftId == 1 && !strOtherDepartments.Contains(i.Department)).Select(i => i.phdFaculty).Sum();

            foreach (var item in intakedetailsList.Where(i => i.shiftId == 1).ToList())
            {
                foreach (var item1 in item.PhdRegNos)
                {
                    New_PHD_Count1.Add(item1);
                }

            }




            int totalBtechFirstYearIntake = intakedetailsList.Where(d => d.Degree == "B.Tech" && !strOtherDepartments.Contains(d.Department)).Select(d => d.approvedIntake1).Sum();

            string[] ProposedIntakeZeroDeptName = intakedetailsList.Where(e => e.Degree == "B.Tech" && (e.approvedIntake1 == 0) && !strOtherDepartments.Contains(e.Department)).Select(e => e.Specialization).Distinct().ToArray();

            List<jntuh_college_attendace> LoginList = new List<jntuh_college_attendace>();
            LoginList = db.jntuh_college_attendace.Where(a => a.Collegeid == collegeID && a.Month == type).ToList();

            string[] RegNos = new string[] { "9211-150409-180107", "6317-150413-162713", "96150406-153506", "78150407-104506", "90150406-132723", "5264-150409-160644", "8518-150413-150717", "7995-150414-143317", "4687-150420-162710", "1549-150418-123933", "8891-150420-165037", "2375-150427-184049", "5612-170202-102750", "9234-160302-113847", "49150406-131709", "4261-160308-074435", "4171-150624-143950", "0707-150408-175902", "74150406-141818", "16150402-155455", "54150402-122257", "6348-150411-120653", "6058-150417-174615", "8105-150412-154535", "8558-160312-194806", "1116-160313-055503", "3205-160313-105704", "7748-160313-063954", "2328-150408-154626", "8922-150410-144623", "6400-150409-153507", "4241-150413-144757", "5387-150420-124115", "6442-150413-163535", "3141-160302-143355", "2624-170202-123226", "2778-150621-184256", "6002-150408-104417", "7601-150409-104220", "5724-150409-131120", "9549-150409-131219", "5480-150416-141938", "8460-150505-150705", "5325-150411-101220", "9134-150418-123351", "1235-170107-122241", "3995-150420-172501", "5157-151216-105135", "5993-170208-144156", "0827-160314-154038", "43150407-121703", "9675-170201-060349", "1761-170110-124328", "8780-170212-221004", "42150406-105659", "9741-150408-163934", "2195-150408-145520", "7984-160128-143658", "0756-160128-144719", "8629-160128-141146", "6328-160129-102154", "6339-160128-152920", "5003-150410-100751", "3111-150413-163611", "6889-150413-145248", "7968-170117-141626", "4390-150619-131209", "7037-150619-123808", "6090-150416-211652", "0650-150419-232914", "9644-150420-123408", "2204-150506-223734", "5007-160303-052952", "1221-160111-140608", "5323-160316-172042", "5758-150409-145824", "9690-150419-143601", "2845-160318-215132", "6585-150414-151718", "6518-160222-100511", "37150407-125255", "4939-150419-180848", "7509-150415-191610", "7038-150418-123607", "4375-150420-144936", "4925-160204-183924", "5271-160206-142514", "9379-150417-164707", "4561-160211-082340", "4517-150424-122333", "4503-150419-134140", "8391-150419-133206", "7549-150409-171452", "2289-150418-215024", "5999-150418-172740", "7521-150418-223714", "4740-150419-162142", "9374-150419-225506", "0752-150422-165906", "6713-150422-171112", "7419-150420-154436", "0059-150420-164303", "7867-160309-173829", "0152-160309-181208", "50150406-110631", "4769-150416-174812", "8005-150416-145941", "2516-150422-195357", "8508-150412-113146", "1003-170119-133836", "4904-170119-140309", "0588-170126-140848", "8291-150624-160237", "6023-151229-151344", "4534-150408-104328", "9794-160229-175017", "3909-151230-121957", "68150407-154601", "1147-150412-155421", "6851-170208-173310", "8532-170213-171543", "77150407-144017", "4281-150415-144925", "8238-161024-112302", "1949-150421-144423", "3666-150408-102957", "1854-150414-123210", "8628-150414-121943", "2834-150414-131904", "8474-150420-113501", "5314-150422-160253", "6998-150422-145056", "8739-150422-124734", "5241-150422-143019", "1831-150422-122747", "7170-150422-115651", "4386-150422-113334", "1097-150422-111446", "2563-150409-185844", "6483-150409-120156", "8889-150423-121239", "3174-150423-145633", "7141-150416-152448", "3831-150426-164813", "1364-150426-165916", "8420-150426-163755", "0778-150501-170330", "2771-150622-225307", "8776-150622-232358", "7503-150424-104100", "9155-150622-231402", "7447-150525-115526", "9731-150622-154029", "3217-150409-132413", "2351-160302-144825", "8336-160302-171055", "93150406-161145", "67150407-173554", "7618-150410-162949", "9986-150619-175911", "94150404-124125", "00150407-113102", "2180-150424-222555", "1126-150507-163545", "1808-150621-133359", "4266-150622-125140", "7977-150623-115417", "7690-150415-134357", "6734-150415-133015", "4696-150409-112735", "9742-150409-105139", "5207-150409-145858", "88150407-162910", "7306-150411-145154", "2980-150411-105638", "9381-150411-113008", "7210-150428-042100", "2316-150428-040056", "5018-170111-130147", "2421-170118-102956", "0676-150408-111501", "2469-160121-104311", "94150402-171218", "9492-160121-160214", "2681-150625-150854", "6076-160311-171551", "0661-160312-141716", "2280-150408-151134", "0208-150411-124221", "8076-150409-161147", "5204-150412-144814", "4443-150412-150351", "9010-150412-151234", "3264-150418-173255", "0922-150415-111243", "0852-150429-154729", "4149-150621-173825", "6620-150625-180248", "7810-161226-114059", "4815-160201-103423", "6809-150505-133845", "0067-150505-141355", "5661-150505-153407", "5246-150505-151341", "7132-150505-171425", "9033-150430-105308", "3057-150506-113420", "8389-150506-170835", "2511-150506-173242", "4009-150408-154747", "8440-150624-131011", "9393-160306-105928", "4283-160315-194106", "3404-170119-172558", "7762-170213-131706", "1475-150414-183509", "7134-160218-150219", "0414-150427-153301", "2067-150414-155501", "33150330-155256", "87150406-194332", "8362-150409-110622", "6180-150408-151453", "7098-150426-074226", "7386-150414-125915", "1466-170105-194422", "1997-170112-095532", "5841-170125-113934", "5612-170125-124150", "1782-150408-121508", "1781-150409-114620", "47150407-153633", "6498-150414-132024", "9077-150416-124631", "5914-150504-100508", "0940-150504-125126", "7835-150506-141716", "6244-150622-125503", "8145-150623-151823", "4830-150623-163945", "6188-150413-160703", "3295-151219-103214", "8352-150625-115421", "7697-150625-114147", "7437-150412-112238", "6903-150506-125905", "8943-150507-132216", "3855-150507-145302", "5572-150507-124125", "3430-150507-155250", "3059-150516-165055", "0336-150507-122950", "1400-150507-152048", "4765-170522-104257", "09150402-160427", "6421-151221-144330", "7885-160312-140130", "0052-160312-131632", "0127-160312-141609", "3725-160312-151706", "9069-160312-151048", "7922-160312-160244", "0682-160312-172016", "2685-160312-172344", "7093-160312-154220", "3195-160314-232954", "5912-150408-122341", "84150406-142457", "7913-150417-131457", "6808-150424-004516", "7368-150419-164942", "3038-150413-112451", "58150406-100658", "68150404-140248", "10150404-134548", "61150404-162737", "56150407-160735", "8021-150409-165612", "8901-150430-111830", "1584-150409-112324", "9049-150416-103205", "95150404-130909", "1083-150417-152920", "1809-150428-104045", "7477-150428-103245", "9865-150426-124940", "6104-150427-134207", "6315-160225-130444", "8441-150413-113304", "9811-150409-103922", "8964-150420-121821", "4723-160311-151729", "03150402-152426", "9373-150426-135654", "8422-150410-113036", "9552-150410-111820", "4713-150420-124126", "6087-170212-130603", "9966-170211-170427", "0319-150415-110441", "9914-150414-143319", "8688-150413-214801", "4046-150507-120439", "5688-160311-160356", "37150406-124138", "86150406-152725", "6080-160305-164900", "7098-160309-162246", "3941-160309-164350", "0502-170213-143616", "6329-170131-185402", "1338-150415-172930", "0443-150620-112146", "39150405-125954", "4439-150422-115107", "6970-150415-123739", "5363-150417-125713", "6048-160225-114522", "8477-160301-195215", "7934-160301-193415", "7595-150423-120919", "5464-150427-154315", "1054-160102-160709", "15150407-161016", "2585-150410-170055", "8344-150429-162201", "8833-150504-131131", "0184-150429-124639", "0767-150430-105420", "0799-150430-113108", "1784-150430-101515", "2900-150430-114213", "3100-150501-103154", "3253-150430-111321", "3477-150430-113036", "4437-150430-104224", "4756-150411-150337", "4861-150430-112525", "5200-150501-103751", "5107-150430-104415", "5360-150430-090404", "6453-150507-190621", "7074-150507-151336", "1705-170116-142649", "91150406-161756", "9383-160128-125153", "0376-150421-155232", "2043-160310-154642", "23150403-182551", "0220-150422-163321", "3271-150506-120301", "0831-160218-160442", "3138-160219-144523", "3570-160219-145952", "5730-160219-155537", "3603-160220-122613", "4448-160220-193243", "5148-160220-114832", "1924-160220-113523", "6796-160220-221344", "5301-160304-192359", "8226-160305-162320", "9581-160305-115025", "8823-160305-140040", "1429-150427-153420", "5607-150425-102421", "47150406-125841", "7217-160209-132527", "2719-150417-140130", "8501-150421-095920", "45150407-154450", "1067-150419-120449", "5555-160129-181224", "0855-160314-234426", "8195-160314-180632", "2658-160314-185351", "8213-160314-204848", "2521-160314-200255", "9616-160314-214031", "0244-160314-231118", "3845-160314-214240", "1475-160315-001332", "3897-160314-200045", "7710-160319-204556", "6424-160319-215612", "7341-160319-202433", "6897-160319-221313", "8988-160319-224714", "5086-150410-102356", "7920-150413-130841", "2861-150410-135151", "6794-160319-164923", "2163-160319-175204", "8260-160319-182908", "6607-160319-151901", "2549-160319-143150", "5086-160319-123814", "2359-160319-121236", "8319-160319-114531", "52150401-155113", "6429-150410-134158", "8471-150412-173932", "2546-150412-173119", "5430-150413-163549", "4595-150422-114349", "7960-150502-150257", "1556-150502-151010", "1620-150502-152356", "5563-150410-155500", "8462-150422-133254", "4469-150422-140349", "39150404-122803", "89150406-135317", "8733-160220-105912", "3280-160311-151200", "68150404-143104", "1620-150419-083055", "98150402-140537", "4240-150619-124624", "90150402-150047", "5077-150411-114936", "1735-160106-153643", "4305-160313-173609", "4707-160313-172938", "7992-160313-172334", "4292-160313-174602", "2969-160313-170725", "7697-160314-151405", "5247-160314-201702", "2991-160314-202954", "3831-170207-161803", "52150407-113402", "1185-150410-154249", "4181-150415-152826", "63150406-133241", "7082-160225-144210", "6833-150429-151901", "7084-150603-172353", "8010-150421-124203", "5725-150505-114121", "1394-150430-232728", "60150406-153912", "9700-150418-103531", "5407-150420-114851", "0261-150421-122215", "4944-150507-132508", "0878-160225-114919", "4393-150421-155815", "0967-160310-130928", "2599-150421-154955", "6087-160224-152646", "0546-160224-111407", "7909-160312-182449", "0487-160312-190833", "9358-170111-094327", "6379-170112-093250", "9888-150421-113110", "1166-150622-055114", "2952-150624-163859", "7893-150421-160431", "87150404-160809", "2876-150409-150305", "8578-150418-145228", "4050-150424-155306", "70150404-125108", "9618-150416-122424", "8941-150415-152633", "0959-170118-101333", "0774-150427-133052", "0785-160304-122835", "9791-150422-162245", "22150407-163124", "53150406-105254", "1528-160304-125739", "8238-161224-112812", "3531-150421-143805", "4278-150416-175043", "7787-150411-145051", "56150403-213949", "6604-150416-125428", "4857-150421-080322", "0023-160316-130354", "1472-160312-141517", "7735-150421-182347", "50150403-105221", "2813-150408-164256", "3538-150411-104526", "0560-150428-125331", "2763-160203-170600", "0170-160307-001639", "9304-160306-225725", "0171-160306-235900", "2124-160306-232603", "3439-160316-105705", "7980-160316-105741", "5725-150409-102427", "4711-150426-181413", "4892-150413-110154", "0895-150413-112139", "0093-150413-132750", "4698-160201-104955", "8540-150413-110353", "6074-150413-170744", "4574-150620-175558", "5645-150620-182613", "1693-150412-112919", "0158-150427-152718", "2008-150421-095104", "4272-160108-143318", "7884-150413-153150", "4800-150415-103452", "1136-160311-110044", "9656-170111-081909", "33150407-101427", "1266-150427-190250", "2093-170207-122958", "3507-150409-123348", "7541-150412-152809", "2825-150420-092811", "9437-150420-103610", "4992-150409-141335", "3498-150408-231802", "6166-150512-104107", "9768-150427-210434", "0205-150508-120436", "2637-150425-151316", "2513-150425-192936", "4188-150424-144352", "2456-150427-125655", "1660-150507-152429", "3570-150507-110913", "1601-150408-172051", "0148-150419-091522", "1442-161217-164741", "31150403-201030", "0941-170207-002441", "8575-170131-031553", "9965-170131-032636", "5691-170131-033654", "8645-170131-034806", "7939-170131-035846", "3351-170131-041646", "9837-170131-042413", "2633-170131-043329", "2964-170131-045303", "2440-170131-201700", "4894-170201-001106", "7664-160522-105414", "6997-170207-182304", "7197-170207-173901", "7397-160524-140443", "9865-160525-110126", "4556-160522-113614", "4469-150409-124255" };

            LoginList = LoginList.Where(l => !RegNos.Contains(l.RegistrationNo)).ToList();
            #region Excel File Code
            //string filePath = string.Empty;
            //List<BasNotice> LoginList = new List<BasNotice>();
            //if (ModelState.IsValid)
            //{
            //    dynamic Result;
            //    string appfilepath = Server.MapPath("~/Content/");
            //    string appfilename = "BASMonthlyReportFileNew.xlsx";
            //    filePath = appfilepath + appfilename;
            //    string extension = ".xlsx";
            //    string conString = string.Empty;
            //    switch (extension)
            //    {
            //        case ".xls":
            //            conString = ConfigurationManager.ConnectionStrings["Excel03ConString"].ConnectionString;
            //            break;
            //        case ".xlsx":
            //            conString = ConfigurationManager.ConnectionStrings["Excel07ConString"].ConnectionString;
            //            break;
            //    }
            //    conString = string.Format(conString, filePath);
            //    using (OleDbConnection connExcel = new OleDbConnection(conString))
            //    {
            //        using (OleDbCommand cmdExcel = new OleDbCommand())
            //        {
            //            using (OleDbDataAdapter odaExcel = new OleDbDataAdapter())
            //            {
            //                DataTable dt = new DataTable();
            //                cmdExcel.Connection = connExcel;
            //                connExcel.Open();
            //                DataTable dtExcelSchema;

            //                dtExcelSchema = connExcel.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
            //                string sheetName = string.Empty; 

            //                if(type=="August")
            //                {
            //                    sheetName = dtExcelSchema.Rows[1]["Table_Name"].ToString();
            //                    string[] Array = sheetName.Split('_');
            //                    SheetMonthName = Array[0].ToString();
            //                }
            //                else if (type == "September")
            //                {
            //                    sheetName = dtExcelSchema.Rows[2]["Table_Name"].ToString();
            //                    string[] Array = sheetName.Split('_');
            //                    SheetMonthName = Array[0].ToString();
            //                }
            //                else
            //                {
            //                    sheetName = dtExcelSchema.Rows[3]["Table_Name"].ToString();
            //                    string[] Array = sheetName.Split('_');
            //                    SheetMonthName = Array[0].ToString();
            //                }

            //               // string sheetName = dtExcelSchema.Rows[2]["Table_Name"].ToString();
            //                //string sheetName = dtExcelSchema.Rows[3]["Table_Name"].ToString(); 
            //                connExcel.Close();
            //                connExcel.Open();
            //                cmdExcel.CommandText = "SELECT * From [" + sheetName + "]";
            //                odaExcel.SelectCommand = cmdExcel;
            //                odaExcel.Fill(dt);
            //                connExcel.Close();
            //                int i = 1;

            //                var query=from row in dt.AsEnumerable()
            //                          select row;

            //                foreach (DataRow row in dt.Rows)
            //                {


            //                        BasNotice loginadd = new BasNotice();
            //                        loginadd.CollegeId = Convert.ToInt32(row["Collegeid"]);
            //                        loginadd.CollegeCode = row["CollegeCode"].ToString();
            //                        loginadd.DeptId = Convert.ToInt32(row["EmpDeptID"]);
            //                        loginadd.RegistrationNo = row["RegistrationNo"].ToString();
            //                        MonthName=row["Month"].ToString();
            //                        loginadd.one = row["ONE"].ToString();
            //                        loginadd.two = row["TWO"].ToString();
            //                        loginadd.three = row["THREE"].ToString();
            //                        loginadd.four = row["FOUR"].ToString();
            //                        loginadd.five = row["FIVE"].ToString();
            //                        loginadd.six = row["SIX"].ToString();
            //                        loginadd.seven = row["SEVEN"].ToString();
            //                        loginadd.eight = row["EAIGHT"].ToString();
            //                        loginadd.nine = row["NINE"].ToString();
            //                        loginadd.ten = row["TEN"].ToString();
            //                        loginadd.eleven = row["ELEVEN"].ToString();
            //                        loginadd.twelve = row["TWELVE"].ToString();
            //                        loginadd.THIRTEEN = row["THIRTEEN"].ToString();
            //                        loginadd.Fourteen = row["FOURTEEN"].ToString();
            //                        loginadd.Fifthteen = row["FIFGHTEEN"].ToString();
            //                        loginadd.sixteen = row["SIXTEEN"].ToString();
            //                        loginadd.Sevenghteen = row["SEVENGHTEEN"].ToString();
            //                        loginadd.Eightghteen = row["EAIGHTEEN"].ToString();
            //                        loginadd.Nineghteen = row["NINGHTEEN"].ToString();
            //                        loginadd.Twenty = row["TWENTY"].ToString();
            //                        loginadd.Twentyone = row["TWENTY ONE"].ToString();
            //                        loginadd.Twentytwo = row["TWENTY TWO"].ToString();
            //                        loginadd.Twentythree = row["TWENTY THREE"].ToString();                                   
            //                        loginadd.Twentyfour = row["TWENTY FOUR"].ToString();
            //                        loginadd.Twentyfive = row["TWENTY FIVE"].ToString();
            //                        loginadd.Twentysix = row["TWENTY SIX"].ToString();
            //                        loginadd.Twentyseven = row["TWENTY SEVEN"].ToString();
            //                        loginadd.Twentyeight = row["TWEENTY EAIGHT"].ToString();
            //                        loginadd.Twentynine = row["TWENTY NINE"].ToString();
            //                        loginadd.THIRTY = row["THIRTY"].ToString();
            //                        loginadd.THIRTYone = row["THIRTY ONE"].ToString();
            //                        loginadd.Totalpercentage = Convert.ToDouble(row["TotalPercentage"]);                                  
            //                        LoginList.Add(loginadd);


            //                }




            //            }
            //        }
            //    }

            //}
            #endregion
            // MonthNameNew = MonthName == null ? SheetMonthName : MonthName;
            // StaffDetails += "<p style='text-align:center;'><b><u>Table-I</u></b></p><br/>";
            //StaffDetails += "<table border='1' cellpadding='3' cellspacing='0' width='100%' style='font-family: Times New Roman; font-size: 8px; font-weight: normal; background-color: #fff; margin: 0 auto;'>";
            //StaffDetails += "<tr>";
            //StaffDetails += "<th rowspan='2' style='text-align:center;'>S.No</th>";
            //StaffDetails += "<th rowspan='2' style='text-align:center;'>Program</th>";
            //StaffDetails += "<th rowspan='2' style='text-align:center;'>Branch / Speciliazation</th>";
            //StaffDetails += "<th rowspan='2' style='text-align:center;'>Intake for which affiliation granted</th>";
            //StaffDetails += "<th colspan='3' style='text-align:center;'>Faculty shown as per College Portal for the Month of " + type.Substring(0,3) + "-" + year + "</th>";
            //StaffDetails += "<th colspan='3' style='text-align:center;'>Faculty as per Biometric Attendance for the Month of " + type.Substring(0,3) + "-" + year + "</th>";
            //StaffDetails += "<th rowspan='2' style='text-align:center;'>Difference</th>";
            //StaffDetails += "</tr>";
            //StaffDetails += "<tr>";
            //StaffDetails += "<th style='text-align:center;'>With Ph.D</th>";
            //StaffDetails += "<th style='text-align:center;'>Without Ph.D</th>";
            //StaffDetails += "<th style='text-align:center;'>Total</th>";
            //StaffDetails += "<th style='text-align:center;'>With Ph.D</th>";
            //StaffDetails += "<th style='text-align:center;'>Without Ph.D</th>";
            //StaffDetails += "<th style='text-align:center;'>Total</th>";
            //StaffDetails += "</tr>";

            var PHD = LoginList.Where(l => New_PHD_Count1.Contains(l.RegistrationNo)).ToList();


            if (type == "August" || type == "October")
            {
                PHDPresentCount = PHD.Where(s => s.TotalPercentage >= monthCount).Count();
                //PHDPresentCount = PHD.Count(p => p.ONE == "Yes" || p.TWO == "Yes" || p.THREE == "Yes" || p.FOUR == "Yes" || p.FIVE == "Yes" || p.SIX == "Yes" || p.SEVEN == "Yes" || p.EAIGHT == "Yes" || p.NINE == "Yes" || p.TEN == "Yes"
                //             || p.ELEVEN == "Yes" || p.TWELVE == "Yes" || p.THIRTEEN == "Yes" || p.FOURTEEN == "Yes" || p.FIFGHTEEN == "Yes" || p.SIXTEEN == "Yes" || p.SEVENGHTEEN == "Yes" || p.EAIGHTEEN == "Yes" || p.NINGHTEEN == "Yes" || p.TWENTY == "Yes" || p.TWENTY_ONE == "Yes"
                //             || p.TWENTY_TWO == "Yes" || p.TWENTY_THREE == "Yes" || p.TWENTY_FOUR == "Yes" || p.TWENTY_FIVE == "Yes" || p.TWENTY_SIX == "Yes" || p.TWENTY_SEVEN == "Yes" ||  p.TWEENTY_EAIGHT == "Yes" || p.TWENTY_NINE == "Yes" || p.THIRTY == "Yes" || p.THIRTY_ONE=="YES");
            }
            else
            {
                PHDPresentCount = PHD.Where(s => s.TotalPercentage >= monthCount).Count();
                //PHDPresentCount = PHD.Count(p => p.ONE == "Yes" || p.TWO == "Yes" || p.THREE == "Yes" || p.FOUR == "Yes" || p.FIVE == "Yes" || p.SIX == "Yes" || p.SEVEN == "Yes" || p.EAIGHT == "Yes" || p.NINE == "Yes" || p.TEN == "Yes"
                //             || p.ELEVEN == "Yes" || p.TWELVE == "Yes" || p.THIRTEEN == "Yes" || p.FOURTEEN == "Yes" || p.FIFGHTEEN == "Yes" || p.SIXTEEN == "Yes" || p.SEVENGHTEEN == "Yes" || p.EAIGHTEEN == "Yes" || p.NINGHTEEN == "Yes" || p.TWENTY == "Yes" || p.TWENTY_ONE == "Yes"
                //             || p.TWENTY_TWO == "Yes" || p.TWENTY_THREE == "Yes" || p.TWENTY_FOUR == "Yes" || p.TWENTY_FIVE == "Yes" || p.TWENTY_SIX == "Yes" || p.TWENTY_SEVEN == "Yes" || p.TWEENTY_EAIGHT == "Yes" || p.TWENTY_NINE == "Yes" || p.THIRTY == "Yes");
            }



            foreach (var item in intakedetailsList)
            {
                // var IntakeCount=0;
                var phdpercentagecount = 0;
                var nonphdpercentagecount = 0;
                if (!strOtherDepartments.Contains(item.Department))
                {

                    if (item.Degree == "B.Tech")
                    {
                        Btech_Faculty_ProposedCount += item.approvedIntake1;
                        BTech_PHD_Count_New += item.SpecializationsphdFaculty;

                    }
                    else if (item.Degree == "M.Tech")
                    {
                        Mtech_Faculty_ProposedCount += item.approvedIntake1;
                        MTech_PHD_Count_New += item.SpecializationsphdFaculty;
                    }
                    else if (item.Degree == "B.Pharmacy")
                    {
                        BPharmacy_Faculty_ProposedCount += item.approvedIntake1;
                    }
                    else if (item.Degree == "M.Pharmacy")
                    {
                        Mpharmacy_Faculty_ProposedCount += item.approvedIntake1;
                    }
                    else if (item.Degree == "MBA")
                    {
                        MBA_Faculty_ProposedCount += item.approvedIntake1;
                        MBA_PHD_Count_New += item.SpecializationsphdFaculty;
                    }
                    else if (item.Degree == "MCA")
                    {
                        MCA_Faculty_ProposedCount += item.approvedIntake1;
                        MCA_PHD_Count_New += item.SpecializationsphdFaculty;
                    }
                    else if (item.Degree == "Pharm.D")
                    {
                        Pharm_D_Faculty_ProposedCount += item.approvedIntake1;
                    }
                    else if (item.Degree == "Pharm.D PB")
                    {
                        Pharm_DPB_Faculty_ProposedCount += item.approvedIntake1;
                    }
                    else if (item.Degree == "MAM")
                    {
                        MAM_Faculty_ProposedCount += item.approvedIntake1;
                        MAM_PHD_Count_New += item.SpecializationsphdFaculty;
                    }
                    else
                    {
                        MTM_Faculty_ProposedCount += item.approvedIntake1;
                    }

                    var LoginList_New = LoginList.Where(s => s.EmpDeptID == item.DepartmentID).ToList();

                    if (item.PhdRegNos.Length != 0)
                    {
                        var PhdFacultyList = LoginList_New.Where(r => item.PhdRegNos.Contains(r.RegistrationNo)).ToList();

                        if (type == "August" || type == "October")
                        {
                            phdpercentagecount = PhdFacultyList.Where(s => s.TotalPercentage >= monthCount).Count();
                            //phdpercentagecount = PhdFacultyList.Count(p => p.ONE == "Yes" || p.TWO == "Yes" || p.THREE == "Yes" || p.FOUR == "Yes" || p.FIVE == "Yes" || p.SIX == "Yes" || p.SEVEN == "Yes" || p.EAIGHT == "Yes" || p.NINE == "Yes" || p.TEN == "Yes"
                            // || p.ELEVEN == "Yes" || p.TWELVE == "Yes" || p.THIRTEEN == "Yes" || p.FOURTEEN == "Yes" || p.FIFGHTEEN == "Yes" || p.SIXTEEN == "Yes" || p.SEVENGHTEEN == "Yes" || p.EAIGHTEEN == "Yes" || p.NINGHTEEN == "Yes" || p.TWENTY == "Yes" || p.TWENTY_ONE == "Yes"
                            // || p.TWENTY_TWO == "Yes" || p.TWENTY_THREE == "Yes" || p.TWENTY_FOUR == "Yes" || p.TWENTY_FIVE == "Yes" || p.TWENTY_SIX == "Yes" || p.TWENTY_SEVEN == "Yes" || p.TWEENTY_EAIGHT == "Yes" || p.TWENTY_NINE == "Yes" || p.THIRTY == "Yes" || p.THIRTY_ONE == "YES");

                            if (item.NonPhdRegNos.Length != 0)
                            {
                                var FacultyList = LoginList_New.Where(r => item.NonPhdRegNos.Contains(r.RegistrationNo)).ToList();
                                nonphdpercentagecount = FacultyList.Where(s => s.TotalPercentage >= monthCount).Count();
                                //   nonphdpercentagecount = FacultyList.Count(p => p.ONE == "Yes" || p.TWO == "Yes" || p.THREE == "Yes" || p.FOUR == "Yes" || p.FIVE == "Yes" || p.SIX == "Yes" || p.SEVEN == "Yes" || p.EAIGHT == "Yes" || p.NINE == "Yes" || p.TEN == "Yes"
                                //|| p.ELEVEN == "Yes" || p.TWELVE == "Yes" || p.THIRTEEN == "Yes" || p.FOURTEEN == "Yes" || p.FIFGHTEEN == "Yes" || p.SIXTEEN == "Yes" || p.SEVENGHTEEN == "Yes" || p.EAIGHTEEN == "Yes" || p.NINGHTEEN == "Yes" || p.TWENTY == "Yes" || p.TWENTY_ONE == "Yes"
                                //|| p.TWENTY_TWO == "Yes" || p.TWENTY_THREE == "Yes" || p.TWENTY_FOUR == "Yes" || p.TWENTY_FIVE == "Yes" || p.TWENTY_SIX == "Yes" || p.TWENTY_SEVEN == "Yes" || p.TWEENTY_EAIGHT == "Yes" || p.TWENTY_NINE == "Yes" || p.THIRTY == "Yes" || p.THIRTY_ONE == "YES");
                            }
                            else
                            {
                                nonphdpercentagecount = 0;
                            }

                        }
                        else
                        {
                            phdpercentagecount = PhdFacultyList.Where(s => s.TotalPercentage >= monthCount).Count();
                            //phdpercentagecount = PhdFacultyList.Count(p => p.ONE == "Yes" || p.TWO == "Yes" || p.THREE == "Yes" || p.FOUR == "Yes" || p.FIVE == "Yes" || p.SIX == "Yes" || p.SEVEN == "Yes" || p.EAIGHT == "Yes" || p.NINE == "Yes" || p.TEN == "Yes"
                            // || p.ELEVEN == "Yes" || p.TWELVE == "Yes" || p.THIRTEEN == "Yes" || p.FOURTEEN == "Yes" || p.FIFGHTEEN == "Yes" || p.SIXTEEN == "Yes" || p.SEVENGHTEEN == "Yes" || p.EAIGHTEEN == "Yes" || p.NINGHTEEN == "Yes" || p.TWENTY == "Yes" || p.TWENTY_ONE == "Yes"
                            // || p.TWENTY_TWO == "Yes" || p.TWENTY_THREE == "Yes" || p.TWENTY_FOUR == "Yes" || p.TWENTY_FIVE == "Yes" || p.TWENTY_SIX == "Yes" || p.TWENTY_SEVEN == "Yes" || p.TWEENTY_EAIGHT == "Yes" || p.TWENTY_NINE == "Yes" || p.THIRTY == "Yes");

                            if (item.NonPhdRegNos.Length != 0)
                            {
                                var FacultyList = LoginList_New.Where(r => item.NonPhdRegNos.Contains(r.RegistrationNo)).ToList();
                                nonphdpercentagecount = FacultyList.Where(s => s.TotalPercentage >= monthCount).Count();
                                //     nonphdpercentagecount = FacultyList.Count(p => p.ONE == "Yes" || p.TWO == "Yes" || p.THREE == "Yes" || p.FOUR == "Yes" || p.FIVE == "Yes" || p.SIX == "Yes" || p.SEVEN == "Yes" || p.EAIGHT == "Yes" || p.NINE == "Yes" || p.TEN == "Yes"
                                //|| p.ELEVEN == "Yes" || p.TWELVE == "Yes" || p.THIRTEEN == "Yes" || p.FOURTEEN == "Yes" || p.FIFGHTEEN == "Yes" || p.SIXTEEN == "Yes" || p.SEVENGHTEEN == "Yes" || p.EAIGHTEEN == "Yes" || p.NINGHTEEN == "Yes" || p.TWENTY == "Yes" || p.TWENTY_ONE == "Yes"
                                //|| p.TWENTY_TWO == "Yes" || p.TWENTY_THREE == "Yes" || p.TWENTY_FOUR == "Yes" || p.TWENTY_FIVE == "Yes" || p.TWENTY_SIX == "Yes" || p.TWENTY_SEVEN == "Yes" || p.TWEENTY_EAIGHT == "Yes" || p.TWENTY_NINE == "Yes" || p.THIRTY == "Yes");
                            }
                            else
                            {
                                nonphdpercentagecount = 0;
                            }

                        }

                    }
                    else
                    {
                        if (type == "August" || type == "October")
                        {
                            if (item.NonPhdRegNos.Length != 0)
                            {
                                var FacultyList = LoginList_New.Where(f => item.NonPhdRegNos.Contains(f.RegistrationNo)).ToList();
                                nonphdpercentagecount = FacultyList.Where(s => s.TotalPercentage >= monthCount).Count();
                                // nonphdpercentagecount = FacultyList.Count(p => p.ONE == "Yes" || p.TWO == "Yes" || p.THREE == "Yes" || p.FOUR == "Yes" || p.FIVE == "Yes" || p.SIX == "Yes" || p.SEVEN == "Yes" || p.EAIGHT == "Yes" || p.NINE == "Yes" || p.TEN == "Yes"
                                //|| p.ELEVEN == "Yes" || p.TWELVE == "Yes" || p.THIRTEEN == "Yes" || p.FOURTEEN == "Yes" || p.FIFGHTEEN == "Yes" || p.SIXTEEN == "Yes" || p.SEVENGHTEEN == "Yes" || p.EAIGHTEEN == "Yes" || p.NINGHTEEN == "Yes" || p.TWENTY == "Yes" || p.TWENTY_ONE == "Yes"
                                //|| p.TWENTY_TWO == "Yes" || p.TWENTY_THREE == "Yes" || p.TWENTY_FOUR == "Yes" || p.TWENTY_FIVE == "Yes" || p.TWENTY_SIX == "Yes" || p.TWENTY_SEVEN == "Yes" || p.TWEENTY_EAIGHT == "Yes" || p.TWENTY_NINE == "Yes" || p.THIRTY == "Yes" || p.THIRTY_ONE == "YES");
                            }
                            else
                            {
                                nonphdpercentagecount = 0;
                            }

                        }
                        else
                        {
                            if (item.NonPhdRegNos.Length != 0)
                            {
                                var FacultyList = LoginList_New.Where(f => item.NonPhdRegNos.Contains(f.RegistrationNo)).ToList();
                                nonphdpercentagecount = FacultyList.Where(s => s.TotalPercentage >= monthCount).Count();
                                // nonphdpercentagecount = FacultyList.Count(p => p.ONE == "Yes" || p.TWO == "Yes" || p.THREE == "Yes" || p.FOUR == "Yes" || p.FIVE == "Yes" || p.SIX == "Yes" || p.SEVEN == "Yes" || p.EAIGHT == "Yes" || p.NINE == "Yes" || p.TEN == "Yes"
                                //|| p.ELEVEN == "Yes" || p.TWELVE == "Yes" || p.THIRTEEN == "Yes" || p.FOURTEEN == "Yes" || p.FIFGHTEEN == "Yes" || p.SIXTEEN == "Yes" || p.SEVENGHTEEN == "Yes" || p.EAIGHTEEN == "Yes" || p.NINGHTEEN == "Yes" || p.TWENTY == "Yes" || p.TWENTY_ONE == "Yes"
                                //|| p.TWENTY_TWO == "Yes" || p.TWENTY_THREE == "Yes" || p.TWENTY_FOUR == "Yes" || p.TWENTY_FIVE == "Yes" || p.TWENTY_SIX == "Yes" || p.TWENTY_SEVEN == "Yes" || p.TWEENTY_EAIGHT == "Yes" || p.TWENTY_NINE == "Yes" || p.THIRTY == "Yes");
                            }
                            else
                            {
                                nonphdpercentagecount = 0;
                            }
                        }
                    }
                    //StaffDetails += "<tr>";
                    //StaffDetails += "<td style='text-align:center;'>" + count + "</td>";
                    //StaffDetails += "<td style='text-align:center;'>" + item.Degree + "</td>";
                    //if (item.shiftId == 1)
                    //    StaffDetails += "<td style='text-align:left;'>" + item.Specialization + "</td>";
                    //else if (item.shiftId == 2)
                    //    StaffDetails += "<td style='text-align:left;'>" + item.Specialization + "<span>(II)</span></td>";
                    //if (item.approvedIntake1 != 0)
                    //StaffDetails += "<td style='text-align:center;'>" + item.approvedIntake1 + "</td>";
                    //else if(item.totalIntake !=0)
                    //    StaffDetails += "<td style='text-align:center;'>" + 0 + "</td>";
                    //else
                    //    StaffDetails += "<td style='text-align:center;'>NA</td>";
                    //StaffDetails += "<td style='text-align:center;'>" + item.phdFaculty + "</td>";
                    //StaffDetails += "<td style='text-align:center;'>" + (item.ugFaculty + item.pgFaculty) + "</td>";

                    //StaffDetails += "<td style='text-align:center;'>" + item.totalFaculty + "</td>";
                    //StaffDetails += "<td style='text-align:center;'>" + phdpercentagecount + "</td><td style='text-align:center;'>" + nonphdpercentagecount + "</td><td style='text-align:center;'>" + (phdpercentagecount + nonphdpercentagecount) + "</td><td style='text-align:center;'>" + ((item.ugFaculty + item.pgFaculty + item.phdFaculty) - (phdpercentagecount + nonphdpercentagecount)) + "</td>";
                    //StaffDetails += "</tr>";
                    //count++;
                    TotalCount += item.totalFaculty;


                    if (item.shiftId == 1)
                    {
                        Bas_Phd_Count += phdpercentagecount;
                        Bas_NOTPhd_Count += nonphdpercentagecount;
                        TotalCount1 += (phdpercentagecount + nonphdpercentagecount);
                    }
                    else { }
                    if (item.shiftId == 1)
                        DifferenceCount += ((item.ugFaculty + item.pgFaculty + item.phdFaculty) - (phdpercentagecount + nonphdpercentagecount));
                    else { }


                }
                else
                {
                    var LoginList_New = LoginList.Where(s => s.EmpDeptID == item.DepartmentID).ToList();

                    if (item.PhdRegNos.Length != 0)
                    {
                        var PhdFacultyList = LoginList_New.Where(r => item.PhdRegNos.Contains(r.RegistrationNo)).ToList();

                        if (type == "August" || type == "October")
                        {
                            phdpercentagecount = PhdFacultyList.Where(s => s.TotalPercentage >= monthCount).Count();
                            //phdpercentagecount = PhdFacultyList.Count(p => p.ONE == "Yes" || p.TWO == "Yes" || p.THREE == "Yes" || p.FOUR == "Yes" || p.FIVE == "Yes" || p.SIX == "Yes" || p.SEVEN == "Yes" || p.EAIGHT == "Yes" || p.NINE == "Yes" || p.TEN == "Yes"
                            // || p.ELEVEN == "Yes" || p.TWELVE == "Yes" || p.THIRTEEN == "Yes" || p.FOURTEEN == "Yes" || p.FIFGHTEEN == "Yes" || p.SIXTEEN == "Yes" || p.SEVENGHTEEN == "Yes" || p.EAIGHTEEN == "Yes" || p.NINGHTEEN == "Yes" || p.TWENTY == "Yes" || p.TWENTY_ONE == "Yes"
                            // || p.TWENTY_TWO == "Yes" || p.TWENTY_THREE == "Yes" || p.TWENTY_FOUR == "Yes" || p.TWENTY_FIVE == "Yes" || p.TWENTY_SIX == "Yes" || p.TWENTY_SEVEN == "Yes" || p.TWEENTY_EAIGHT == "Yes" || p.TWENTY_NINE == "Yes" || p.THIRTY == "Yes" || p.THIRTY_ONE == "YES");

                            if (item.NonPhdRegNos.Length != 0)
                            {
                                var FacultyList = LoginList_New.Where(r => item.NonPhdRegNos.Contains(r.RegistrationNo)).ToList();
                                nonphdpercentagecount = FacultyList.Where(s => s.TotalPercentage >= monthCount).Count();
                                //    nonphdpercentagecount = FacultyList.Count(p => p.ONE == "Yes" || p.TWO == "Yes" || p.THREE == "Yes" || p.FOUR == "Yes" || p.FIVE == "Yes" || p.SIX == "Yes" || p.SEVEN == "Yes" || p.EAIGHT == "Yes" || p.NINE == "Yes" || p.TEN == "Yes"
                                //|| p.ELEVEN == "Yes" || p.TWELVE == "Yes" || p.THIRTEEN == "Yes" || p.FOURTEEN == "Yes" || p.FIFGHTEEN == "Yes" || p.SIXTEEN == "Yes" || p.SEVENGHTEEN == "Yes" || p.EAIGHTEEN == "Yes" || p.NINGHTEEN == "Yes" || p.TWENTY == "Yes" || p.TWENTY_ONE == "Yes"
                                //|| p.TWENTY_TWO == "Yes" || p.TWENTY_THREE == "Yes" || p.TWENTY_FOUR == "Yes" || p.TWENTY_FIVE == "Yes" || p.TWENTY_SIX == "Yes" || p.TWENTY_SEVEN == "Yes" || p.TWEENTY_EAIGHT == "Yes" || p.TWENTY_NINE == "Yes" || p.THIRTY == "Yes" || p.THIRTY_ONE == "YES");
                            }
                            else
                            {
                                nonphdpercentagecount = 0;
                            }

                        }
                        else
                        {
                            phdpercentagecount = PhdFacultyList.Where(s => s.TotalPercentage >= monthCount).Count();
                            //phdpercentagecount = PhdFacultyList.Count(p => p.ONE == "Yes" || p.TWO == "Yes" || p.THREE == "Yes" || p.FOUR == "Yes" || p.FIVE == "Yes" || p.SIX == "Yes" || p.SEVEN == "Yes" || p.EAIGHT == "Yes" || p.NINE == "Yes" || p.TEN == "Yes"
                            //  || p.ELEVEN == "Yes" || p.TWELVE == "Yes" || p.THIRTEEN == "Yes" || p.FOURTEEN == "Yes" || p.FIFGHTEEN == "Yes" || p.SIXTEEN == "Yes" || p.SEVENGHTEEN == "Yes" || p.EAIGHTEEN == "Yes" || p.NINGHTEEN == "Yes" || p.TWENTY == "Yes" || p.TWENTY_ONE == "Yes"
                            //  || p.TWENTY_TWO == "Yes" || p.TWENTY_THREE == "Yes" || p.TWENTY_FOUR == "Yes" || p.TWENTY_FIVE == "Yes" || p.TWENTY_SIX == "Yes" || p.TWENTY_SEVEN == "Yes" || p.TWEENTY_EAIGHT == "Yes" || p.TWENTY_NINE == "Yes" || p.THIRTY == "Yes");

                            if (item.NonPhdRegNos.Length != 0)
                            {
                                var FacultyList = LoginList_New.Where(r => item.NonPhdRegNos.Contains(r.RegistrationNo)).ToList();
                                nonphdpercentagecount = FacultyList.Where(s => s.TotalPercentage >= monthCount).Count();
                                //    nonphdpercentagecount = FacultyList.Count(p => p.ONE == "Yes" || p.TWO == "Yes" || p.THREE == "Yes" || p.FOUR == "Yes" || p.FIVE == "Yes" || p.SIX == "Yes" || p.SEVEN == "Yes" || p.EAIGHT == "Yes" || p.NINE == "Yes" || p.TEN == "Yes"
                                //|| p.ELEVEN == "Yes" || p.TWELVE == "Yes" || p.THIRTEEN == "Yes" || p.FOURTEEN == "Yes" || p.FIFGHTEEN == "Yes" || p.SIXTEEN == "Yes" || p.SEVENGHTEEN == "Yes" || p.EAIGHTEEN == "Yes" || p.NINGHTEEN == "Yes" || p.TWENTY == "Yes" || p.TWENTY_ONE == "Yes"
                                //|| p.TWENTY_TWO == "Yes" || p.TWENTY_THREE == "Yes" || p.TWENTY_FOUR == "Yes" || p.TWENTY_FIVE == "Yes" || p.TWENTY_SIX == "Yes" || p.TWENTY_SEVEN == "Yes" || p.TWEENTY_EAIGHT == "Yes" || p.TWENTY_NINE == "Yes" || p.THIRTY == "Yes");
                            }
                            else
                            {
                                nonphdpercentagecount = 0;
                            }

                        }
                    }
                    else
                    {

                        if (type == "August" || type == "October")
                        {
                            if (item.NonPhdRegNos.Length != 0)
                            {
                                var FacultyList = LoginList_New.Where(r => item.NonPhdRegNos.Contains(r.RegistrationNo)).ToList();
                                nonphdpercentagecount = FacultyList.Where(s => s.TotalPercentage >= monthCount).Count();
                                //   nonphdpercentagecount = FacultyList.Count(p => p.ONE == "Yes" || p.TWO == "Yes" || p.THREE == "Yes" || p.FOUR == "Yes" || p.FIVE == "Yes" || p.SIX == "Yes" || p.SEVEN == "Yes" || p.EAIGHT == "Yes" || p.NINE == "Yes" || p.TEN == "Yes"
                                //|| p.ELEVEN == "Yes" || p.TWELVE == "Yes" || p.THIRTEEN == "Yes" || p.FOURTEEN == "Yes" || p.FIFGHTEEN == "Yes" || p.SIXTEEN == "Yes" || p.SEVENGHTEEN == "Yes" || p.EAIGHTEEN == "Yes" || p.NINGHTEEN == "Yes" || p.TWENTY == "Yes" || p.TWENTY_ONE == "Yes"
                                //|| p.TWENTY_TWO == "Yes" || p.TWENTY_THREE == "Yes" || p.TWENTY_FOUR == "Yes" || p.TWENTY_FIVE == "Yes" || p.TWENTY_SIX == "Yes" || p.TWENTY_SEVEN == "Yes" || p.TWEENTY_EAIGHT == "Yes" || p.TWENTY_NINE == "Yes" || p.THIRTY == "Yes" || p.THIRTY_ONE == "YES");
                            }
                            else
                            {
                                nonphdpercentagecount = 0;
                            }

                        }
                        else
                        {
                            if (item.NonPhdRegNos.Length != 0)
                            {
                                var FacultyList = LoginList_New.Where(r => item.NonPhdRegNos.Contains(r.RegistrationNo)).ToList();
                                nonphdpercentagecount = FacultyList.Where(s => s.TotalPercentage >= monthCount).Count();
                                //   nonphdpercentagecount = FacultyList.Count(p => p.ONE == "Yes" || p.TWO == "Yes" || p.THREE == "Yes" || p.FOUR == "Yes" || p.FIVE == "Yes" || p.SIX == "Yes" || p.SEVEN == "Yes" || p.EAIGHT == "Yes" || p.NINE == "Yes" || p.TEN == "Yes"
                                //|| p.ELEVEN == "Yes" || p.TWELVE == "Yes" || p.THIRTEEN == "Yes" || p.FOURTEEN == "Yes" || p.FIFGHTEEN == "Yes" || p.SIXTEEN == "Yes" || p.SEVENGHTEEN == "Yes" || p.EAIGHTEEN == "Yes" || p.NINGHTEEN == "Yes" || p.TWENTY == "Yes" || p.TWENTY_ONE == "Yes"
                                //|| p.TWENTY_TWO == "Yes" || p.TWENTY_THREE == "Yes" || p.TWENTY_FOUR == "Yes" || p.TWENTY_FIVE == "Yes" || p.TWENTY_SIX == "Yes" || p.TWENTY_SEVEN == "Yes" || p.TWEENTY_EAIGHT == "Yes" || p.TWENTY_NINE == "Yes" || p.THIRTY == "Yes");
                            }
                            else
                            {
                                nonphdpercentagecount = 0;
                            }
                        }
                    }
                    //StaffDetails += "<tr>";
                    //StaffDetails += "<td style='text-align:center;'>" + count + "</td>";
                    //StaffDetails += "<td style='text-align:center;'>" + item.Degree + "</td>";
                    //if (item.shiftId == 1)
                    //    StaffDetails += "<td style='text-align:left;'>" + item.Specialization + "</td>";
                    //else if (item.shiftId == 2)
                    //    StaffDetails += "<td style='text-align:left;'>" + item.Specialization + "<span>(II)</span></td>";
                    //StaffDetails += "<td style='text-align:center;'>" + totalBtechFirstYearIntake + "</td>";
                    //StaffDetails += "<td style='text-align:center;'>" + item.phdFaculty + "</td>";
                    //StaffDetails += "<td style='text-align:center;'>" + (item.ugFaculty + item.pgFaculty) + "</td>";

                    //StaffDetails += "<td style='text-align:center;'>" + item.totalFaculty + "</td>";
                    //StaffDetails += "<td style='text-align:center;'>" + phdpercentagecount + "</td><td style='text-align:center;'>" + nonphdpercentagecount + "</td><td style='text-align:center;'>" + (phdpercentagecount + nonphdpercentagecount) + "</td><td style='text-align:center;'>" + ((item.ugFaculty + item.pgFaculty + item.phdFaculty) - (phdpercentagecount + nonphdpercentagecount)) + "</td>";
                    //StaffDetails += "</tr>";
                    //count++;
                    TotalCount += item.totalFaculty;
                    TotalCount1 += (phdpercentagecount + nonphdpercentagecount);
                    Bas_Phd_Count += phdpercentagecount;
                    Bas_NOTPhd_Count += nonphdpercentagecount;
                    DifferenceCount += ((item.ugFaculty + item.pgFaculty + item.phdFaculty) - (phdpercentagecount + nonphdpercentagecount));
                }


            }
            TotalCount = (TotalCount - intakedetailsList.Where(l => l.shiftId == 2).Select(l => l.totalFaculty).Sum());



            //StaffDetails += "<tr><td  style='text-align:center;' colspan='6'><b>Total Faculty Count</b></td><td style='text-align:center;'>" + TotalCount + "</td><td style='text-align:center;'>" + Bas_Phd_Count + "</td><td style='text-align:center;'>" + Bas_NOTPhd_Count + "</td><td style='text-align:center;'>" + TotalCount1 + "</td><td style='text-align:center;'>" + DifferenceCount + "</td></tr>";
            //StaffDetails += "</table></br>";
            //StaffDetails += "<p style='text-align:left;font-family: Times New Roman; font-size: 8px;'>NA-Not Affiliated</p></br>";



            var New_intakedetails = intakedetailsList.Where(r => r.DegreeID != 0).GroupBy(r => r.DegreeID).ToList();


            StaffDetails += "<br/><p style='text-align:center;'><b><u>Table-I - Total No.of Faculty required as per Affiliated intake</u></b></p><br/>";
            StaffDetails += "<table border='1' cellpadding='3' cellspacing='0' width='100%' style='font-family: Times New Roman; font-size: 8px; font-weight: normal; background-color: #fff; margin: 0 auto;'>";
            StaffDetails += "<tr>";
            StaffDetails += "<td style='text-align:center;'>S.No</td>";
            StaffDetails += "<td style='text-align:center;'>Program</td>";
            StaffDetails += "<td style='text-align:center;'>With Ph.D</td>";
            StaffDetails += "<td style='text-align:center;'>Without Ph.D</td>";
            StaffDetails += "<td style='text-align:center;'>Total Faculty</td>";
            StaffDetails += "</tr>";

            var count_new = 1;
            foreach (var item2 in New_intakedetails)
            {
                var data_latest = 0;
                var Degree_PHD_Count = 0;
                var Degree = string.Empty;
                var Facultyratio = Convert.ToInt32(jntuh_faculty_student_ratio_norms.Where(fn => fn.degreeId == item2.Key).Select(fn => fn.Norms).FirstOrDefault().Split(':')[1].ToString());
                dynamic TotalRequired_facultyCount = 0;
                if (item2.Key == 4)
                {
                    Degree = "B.Tech";
                    TotalRequired_facultyCount = (int)((Btech_Faculty_ProposedCount / Facultyratio) + intakedetailsList.Where(s => s.DegreeID == item2.Key).Select(s => s.requiredFaculty).Sum());
                    TotalRequired_facultyCount = TotalRequired_facultyCount - BTech_PHD_Count_New;
                    Degree_PHD_Count = BTech_PHD_Count_New;

                    data_latest = TotalRequired_facultyCount + BTech_PHD_Count_New;

                }
                else if (item2.Key == 1)
                {
                    Degree = "M.Tech";
                    TotalRequired_facultyCount = (int)(intakedetailsList.Where(s => s.DegreeID == item2.Key && s.shiftId == 1).Select(s => s.requiredFaculty).Sum());
                    //TotalRequired_facultyCount = (int)((Mtech_Faculty_ProposedCount / Facultyratio) + intakedetailsList.Where(s => s.DegreeID == item2.Key).Select(s => s.requiredFaculty).Sum());
                    TotalRequired_facultyCount = TotalRequired_facultyCount - MTech_PHD_Count_New;
                    Degree_PHD_Count = MTech_PHD_Count_New;

                    data_latest = TotalRequired_facultyCount + MTech_PHD_Count_New;

                }
                else if (item2.Key == 5)
                {
                    var PharmdDegree = intakedetailsList.Where(s => s.academicYearId == 9 && s.Degree == "Pharm.D").Select(s => s.Degree).FirstOrDefault();
                    var PharmdpbDegree = intakedetailsList.Where(s => s.academicYearId == 9 && s.Degree == "Pharm.D PB").Select(s => s.Degree).FirstOrDefault();
                    PharmdDegree = PharmdDegree != null ? "+" + PharmdDegree : "";
                    PharmdpbDegree = PharmdpbDegree != null ? "+" + PharmdpbDegree : "";
                    Degree = "B.Pharmacy" + PharmdDegree + "" + PharmdpbDegree;
                    TotalRequired_facultyCount = (int)(intakedetailsList.Where(s => s.DegreeID == item2.Key).Select(s => s.requiredFaculty).Sum());
                    // TotalRequired_facultyCount = (int)((BPharmacy_Faculty_ProposedCount / Facultyratio) + intakedetailsList.Where(s => s.DegreeID == item2.Key).Select(s => s.requiredFaculty).Sum());

                }
                else if (item2.Key == 2)
                {
                    Degree = "M.Pharmacy";
                    TotalRequired_facultyCount = (int)(intakedetailsList.Where(s => s.DegreeID == item2.Key && s.shiftId == 1).Select(s => s.requiredFaculty).Sum());
                    // TotalRequired_facultyCount = (int)((Mpharmacy_Faculty_ProposedCount / Facultyratio) + intakedetailsList.Where(s => s.DegreeID == item2.Key).Select(s => s.requiredFaculty).Sum());

                }
                else if (item2.Key == 6)
                {
                    Degree = "MBA";
                    TotalRequired_facultyCount = (int)(intakedetailsList.Where(s => s.DegreeID == item2.Key).Select(s => s.requiredFaculty).Sum());

                    TotalRequired_facultyCount = TotalRequired_facultyCount - MBA_PHD_Count_New;
                    Degree_PHD_Count = MBA_PHD_Count_New;

                    data_latest = TotalRequired_facultyCount + MBA_PHD_Count_New;

                }
                else if (item2.Key == 3)
                {
                    Degree = "MCA";
                    TotalRequired_facultyCount = (int)(intakedetailsList.Where(s => s.DegreeID == item2.Key).Select(s => s.requiredFaculty).Sum());

                    TotalRequired_facultyCount = TotalRequired_facultyCount - MCA_PHD_Count_New;
                    Degree_PHD_Count = MCA_PHD_Count_New;

                    data_latest = TotalRequired_facultyCount + MCA_PHD_Count_New;

                }

                else if (item2.Key == 7)
                {
                    Degree = "MAM";
                    TotalRequired_facultyCount = (int)(intakedetailsList.Where(s => s.DegreeID == item2.Key).Select(s => s.requiredFaculty).Sum());
                    TotalRequired_facultyCount = TotalRequired_facultyCount - MAM_PHD_Count_New;
                    Degree_PHD_Count = MAM_PHD_Count_New;


                    data_latest = TotalRequired_facultyCount + MAM_PHD_Count_New;

                }
                else
                {
                    Degree = "MTM";
                    TotalRequired_facultyCount = (int)(intakedetailsList.Where(s => s.DegreeID == item2.Key).Select(s => s.requiredFaculty).Sum());


                }
                if (item2.Key == 9 || item2.Key == 10)
                {
                    //Not Dispaly
                }
                else
                {
                    StaffDetails += "<tr>";
                    StaffDetails += "<td style='text-align:center;'>" + count_new + "</td>";
                    StaffDetails += "<td style='text-align:center;'>" + Degree + "</td>";
                    StaffDetails += "<td style='text-align:center;'>" + Degree_PHD_Count + "</td>";
                    StaffDetails += "<td style='text-align:center;'>" + TotalRequired_facultyCount + "</td>";
                    StaffDetails += "<td style='text-align:center;'>" + data_latest + "</td>";
                    StaffDetails += "</tr>";
                    count_new++;
                    facultyCountnew1 += Degree_PHD_Count;
                    facultyCountnew2 += TotalRequired_facultyCount;
                    facultyCountnew += data_latest;
                    // facultyCountnew += (int)TotalRequired_facultyCount;
                }

            }
            StaffDetails += "<tr><td style='text-align:center;' colspan='2'><b>Total Required Faculty Count</b></td><td style='text-align:center;' >" + facultyCountnew1 + "</td><td style='text-align:center;' >" + facultyCountnew2 + "</td><td style='text-align:center;'>" + facultyCountnew + "</td></tr>";
            StaffDetails += "</table>";



            int difference = facultyCountnew - TotalCount1;
            var Percentage = ((double)difference / (double)facultyCountnew) * 100;
            int CollegePersentage = Convert.ToInt32(Math.Round(Percentage, 0));
            var MinPercentage = Convert.ToInt32(Math.Round(50.00, 0));

            //if (CollegePersentage >= MinPercentage)
            //{
            //    CheckPercentage = true;
            //    return StaffDetails;
            //}
            //else
            //{
            //    CheckPercentage = false;
            //    StaffDetails= string.Empty;
            //    return StaffDetails;
            //}



            return StaffDetails;
        }


        public List<CollegeFacultyWithIntakeReport> StudentAndClasswork(int? collegeId)
        {
            List<CollegeIntakeExisting> collegeIntakeExisting = new List<CollegeIntakeExisting>();
            List<CollegeFacultyWithIntakeReport> intakedetailsList = new List<CollegeFacultyWithIntakeReport>();
            var jntuh_college = db.jntuh_college.Where(c => c.isActive == true).ToList();
            int[] collegeIDs = null;
            if (collegeId != 0)
            {
                collegeIDs = jntuh_college.Where(c => c.id == collegeId).Select(c => c.id).ToArray();
            }
            else
            {
                collegeIDs = jntuh_college.Select(c => c.id).ToArray();
            }

            var jntuh_academic_year = db.jntuh_academic_year.ToList();


            int presentYear = jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            presentYear = presentYear - 1;
            int AY1 = jntuh_academic_year.Where(a => a.actualYear == (presentYear + 1)).Select(a => a.id).FirstOrDefault();
            int AY2 = jntuh_academic_year.Where(a => a.actualYear == (presentYear)).Select(a => a.id).FirstOrDefault();
            int AY3 = jntuh_academic_year.Where(a => a.actualYear == (presentYear - 1)).Select(a => a.id).FirstOrDefault();
            int AY4 = jntuh_academic_year.Where(a => a.actualYear == (presentYear - 2)).Select(a => a.id).FirstOrDefault();
            int AY5 = jntuh_academic_year.Where(a => a.actualYear == (presentYear - 3)).Select(a => a.id).FirstOrDefault();

            // List<jntuh_approvedadmitted_intake> intake = db.jntuh_approvedadmitted_intake.Where(c => collegeIDs.Contains(c.collegeId) && c.AcademicYearId==9).ToList();
            List<jntuh_approvedadmitted_intake> intake = db.jntuh_approvedadmitted_intake.Where(c => collegeIDs.Contains(c.collegeId)).ToList();


            foreach (var item in intake)
            {
                CollegeIntakeExisting newIntake = new CollegeIntakeExisting();
                newIntake.id = item.id;
                newIntake.collegeId = item.collegeId;
                newIntake.academicYearId = item.AcademicYearId;
                newIntake.shiftId = item.ShiftId;
                newIntake.isActive = item.IsActive;
                newIntake.specializationId = item.SpecializationId;
                newIntake.Specialization = item.jntuh_specialization.specializationName;
                newIntake.DepartmentID = item.jntuh_specialization.jntuh_department.id;
                newIntake.Department = item.jntuh_specialization.jntuh_department.departmentName;
                newIntake.degreeID = item.jntuh_specialization.jntuh_department.jntuh_degree.id;
                newIntake.Degree = item.jntuh_specialization.jntuh_department.jntuh_degree.degree;
                newIntake.degreeDisplayOrder = item.jntuh_specialization.jntuh_department.jntuh_degree.degreeDisplayOrder;
                newIntake.shiftId = item.ShiftId;
                newIntake.Shift = item.jntuh_shift.shiftName;
                collegeIntakeExisting.Add(newIntake);
            }

            collegeIntakeExisting = collegeIntakeExisting.AsEnumerable().GroupBy(r => new { r.specializationId, r.shiftId, r.collegeId }).Select(r => r.First()).ToList();
            //int[] StrPharmacy = new[] { 26, 27, 36, 39 };
            foreach (var item in collegeIntakeExisting)
            {
                var intakedetails = new CollegeFacultyWithIntakeReport();

                intakedetails.collegeId = item.collegeId;
                intakedetails.collegeCode = jntuh_college.Where(c => c.id == item.collegeId).Select(c => c.collegeCode).FirstOrDefault();
                intakedetails.collegeName = jntuh_college.Where(c => c.id == item.collegeId).Select(c => c.collegeName).FirstOrDefault();
                intakedetails.academicYearId = item.academicYearId;
                intakedetails.Degree = item.Degree;
                intakedetails.Department = item.Department;
                intakedetails.Specialization = item.Specialization;
                intakedetails.DegreeID = item.degreeID;
                intakedetails.specializationId = item.specializationId;
                intakedetails.DepartmentID = item.DepartmentID;
                intakedetails.shiftId = item.shiftId;
                intakedetails.degreeDisplayOrder = item.degreeDisplayOrder;
                intakedetails.approvedIntake1 = SurpriseGetIntake(item.collegeId, AY1, item.specializationId, item.shiftId, 1, item.degreeID);
                intakedetails.approvedIntake2 = SurpriseGetIntake(item.collegeId, AY2, item.specializationId, item.shiftId, 1, item.degreeID);
                intakedetails.approvedIntake3 = SurpriseGetIntake(item.collegeId, AY3, item.specializationId, item.shiftId, 1, item.degreeID);
                intakedetails.approvedIntake4 = SurpriseGetIntake(item.collegeId, AY4, item.specializationId, item.shiftId, 1, item.degreeID);

                intakedetailsList.Add(intakedetails);
            }



            return intakedetailsList;
        }

        public List<CollegeFacultyWithIntakeReport> Get_PHD_Faculty(int? collegeId)
        {
            int facultystudentRatio = 0;
            decimal facultyRatio = 0m;
            var jntuh_specialization = db.jntuh_specialization.ToList();
            var jntuh_education_category = db.jntuh_education_category.ToList();
            var jntuh_departments = db.jntuh_department.ToList();
            var jntuh_faculty_student_ratio_norms = db.jntuh_faculty_student_ratio_norms.ToList();
            var DeptNameBasedOnSpecialization = (from a in jntuh_departments
                                                 join b in jntuh_specialization on a.id equals b.departmentId
                                                 select new
                                                 {
                                                     DeptId = a.id,
                                                     DeptName = a.departmentName,
                                                     Specid = b.id

                                                 }).ToList();

            var jntuh_college_faculty_registered = db.jntuh_college_faculty_registered.Where(r => r.collegeId == collegeId).ToList();
            var StrRegNos = jntuh_college_faculty_registered.Select(r => r.RegistrationNumber).ToArray();

            var registeredFaculty = db.jntuh_registered_faculty.Where(r => StrRegNos.Contains(r.RegistrationNumber)).ToList();

            var jntuh_college = db.jntuh_college.Where(c => c.isActive == true).ToList();

            var jntuh_registered_faculty1 = registeredFaculty.Select(rf => new
            {
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
                // NotqualifiedAsperAICTE = rf.NotQualifiedAsperAICTE
            }).ToList();

            Non_HighestDegreeBasedRegNos = jntuh_registered_faculty1.Where(e => e.HighestDegreeID <= 3).Select(r => r.RegistrationNumber).ToArray();
            var files = jntuh_registered_faculty1.Where(e => e.HighestDegreeID <= 3).ToList();
            jntuh_registered_faculty1 = jntuh_registered_faculty1.Where(e => e.HighestDegreeID >= 4).ToList();

            string[] RegNosNEW = new string[] { "9211-150409-180107", "6317-150413-162713", "96150406-153506", "78150407-104506", "90150406-132723", "5264-150409-160644", "8518-150413-150717", "7995-150414-143317", "4687-150420-162710", "1549-150418-123933", "8891-150420-165037", "2375-150427-184049", "5612-170202-102750", "9234-160302-113847", "49150406-131709", "4261-160308-074435", "4171-150624-143950", "0707-150408-175902", "74150406-141818", "16150402-155455", "54150402-122257", "6348-150411-120653", "6058-150417-174615", "8105-150412-154535", "8558-160312-194806", "1116-160313-055503", "3205-160313-105704", "7748-160313-063954", "2328-150408-154626", "8922-150410-144623", "6400-150409-153507", "4241-150413-144757", "5387-150420-124115", "6442-150413-163535", "3141-160302-143355", "2624-170202-123226", "2778-150621-184256", "6002-150408-104417", "7601-150409-104220", "5724-150409-131120", "9549-150409-131219", "5480-150416-141938", "8460-150505-150705", "5325-150411-101220", "9134-150418-123351", "1235-170107-122241", "3995-150420-172501", "5157-151216-105135", "5993-170208-144156", "0827-160314-154038", "43150407-121703", "9675-170201-060349", "1761-170110-124328", "8780-170212-221004", "42150406-105659", "9741-150408-163934", "2195-150408-145520", "7984-160128-143658", "0756-160128-144719", "8629-160128-141146", "6328-160129-102154", "6339-160128-152920", "5003-150410-100751", "3111-150413-163611", "6889-150413-145248", "7968-170117-141626", "4390-150619-131209", "7037-150619-123808", "6090-150416-211652", "0650-150419-232914", "9644-150420-123408", "2204-150506-223734", "5007-160303-052952", "1221-160111-140608", "5323-160316-172042", "5758-150409-145824", "9690-150419-143601", "2845-160318-215132", "6585-150414-151718", "6518-160222-100511", "37150407-125255", "4939-150419-180848", "7509-150415-191610", "7038-150418-123607", "4375-150420-144936", "4925-160204-183924", "5271-160206-142514", "9379-150417-164707", "4561-160211-082340", "4517-150424-122333", "4503-150419-134140", "8391-150419-133206", "7549-150409-171452", "2289-150418-215024", "5999-150418-172740", "7521-150418-223714", "4740-150419-162142", "9374-150419-225506", "0752-150422-165906", "6713-150422-171112", "7419-150420-154436", "0059-150420-164303", "7867-160309-173829", "0152-160309-181208", "50150406-110631", "4769-150416-174812", "8005-150416-145941", "2516-150422-195357", "8508-150412-113146", "1003-170119-133836", "4904-170119-140309", "0588-170126-140848", "8291-150624-160237", "6023-151229-151344", "4534-150408-104328", "9794-160229-175017", "3909-151230-121957", "68150407-154601", "1147-150412-155421", "6851-170208-173310", "8532-170213-171543", "77150407-144017", "4281-150415-144925", "8238-161024-112302", "1949-150421-144423", "3666-150408-102957", "1854-150414-123210", "8628-150414-121943", "2834-150414-131904", "8474-150420-113501", "5314-150422-160253", "6998-150422-145056", "8739-150422-124734", "5241-150422-143019", "1831-150422-122747", "7170-150422-115651", "4386-150422-113334", "1097-150422-111446", "2563-150409-185844", "6483-150409-120156", "8889-150423-121239", "3174-150423-145633", "7141-150416-152448", "3831-150426-164813", "1364-150426-165916", "8420-150426-163755", "0778-150501-170330", "2771-150622-225307", "8776-150622-232358", "7503-150424-104100", "9155-150622-231402", "7447-150525-115526", "9731-150622-154029", "3217-150409-132413", "2351-160302-144825", "8336-160302-171055", "93150406-161145", "67150407-173554", "7618-150410-162949", "9986-150619-175911", "94150404-124125", "00150407-113102", "2180-150424-222555", "1126-150507-163545", "1808-150621-133359", "4266-150622-125140", "7977-150623-115417", "7690-150415-134357", "6734-150415-133015", "4696-150409-112735", "9742-150409-105139", "5207-150409-145858", "88150407-162910", "7306-150411-145154", "2980-150411-105638", "9381-150411-113008", "7210-150428-042100", "2316-150428-040056", "5018-170111-130147", "2421-170118-102956", "0676-150408-111501", "2469-160121-104311", "94150402-171218", "9492-160121-160214", "2681-150625-150854", "6076-160311-171551", "0661-160312-141716", "2280-150408-151134", "0208-150411-124221", "8076-150409-161147", "5204-150412-144814", "4443-150412-150351", "9010-150412-151234", "3264-150418-173255", "0922-150415-111243", "0852-150429-154729", "4149-150621-173825", "6620-150625-180248", "7810-161226-114059", "4815-160201-103423", "6809-150505-133845", "0067-150505-141355", "5661-150505-153407", "5246-150505-151341", "7132-150505-171425", "9033-150430-105308", "3057-150506-113420", "8389-150506-170835", "2511-150506-173242", "4009-150408-154747", "8440-150624-131011", "9393-160306-105928", "4283-160315-194106", "3404-170119-172558", "7762-170213-131706", "1475-150414-183509", "7134-160218-150219", "0414-150427-153301", "2067-150414-155501", "33150330-155256", "87150406-194332", "8362-150409-110622", "6180-150408-151453", "7098-150426-074226", "7386-150414-125915", "1466-170105-194422", "1997-170112-095532", "5841-170125-113934", "5612-170125-124150", "1782-150408-121508", "1781-150409-114620", "47150407-153633", "6498-150414-132024", "9077-150416-124631", "5914-150504-100508", "0940-150504-125126", "7835-150506-141716", "6244-150622-125503", "8145-150623-151823", "4830-150623-163945", "6188-150413-160703", "3295-151219-103214", "8352-150625-115421", "7697-150625-114147", "7437-150412-112238", "6903-150506-125905", "8943-150507-132216", "3855-150507-145302", "5572-150507-124125", "3430-150507-155250", "3059-150516-165055", "0336-150507-122950", "1400-150507-152048", "4765-170522-104257", "09150402-160427", "6421-151221-144330", "7885-160312-140130", "0052-160312-131632", "0127-160312-141609", "3725-160312-151706", "9069-160312-151048", "7922-160312-160244", "0682-160312-172016", "2685-160312-172344", "7093-160312-154220", "3195-160314-232954", "5912-150408-122341", "84150406-142457", "7913-150417-131457", "6808-150424-004516", "7368-150419-164942", "3038-150413-112451", "58150406-100658", "68150404-140248", "10150404-134548", "61150404-162737", "56150407-160735", "8021-150409-165612", "8901-150430-111830", "1584-150409-112324", "9049-150416-103205", "95150404-130909", "1083-150417-152920", "1809-150428-104045", "7477-150428-103245", "9865-150426-124940", "6104-150427-134207", "6315-160225-130444", "8441-150413-113304", "9811-150409-103922", "8964-150420-121821", "4723-160311-151729", "03150402-152426", "9373-150426-135654", "8422-150410-113036", "9552-150410-111820", "4713-150420-124126", "6087-170212-130603", "9966-170211-170427", "0319-150415-110441", "9914-150414-143319", "8688-150413-214801", "4046-150507-120439", "5688-160311-160356", "37150406-124138", "86150406-152725", "6080-160305-164900", "7098-160309-162246", "3941-160309-164350", "0502-170213-143616", "6329-170131-185402", "1338-150415-172930", "0443-150620-112146", "39150405-125954", "4439-150422-115107", "6970-150415-123739", "5363-150417-125713", "6048-160225-114522", "8477-160301-195215", "7934-160301-193415", "7595-150423-120919", "5464-150427-154315", "1054-160102-160709", "15150407-161016", "2585-150410-170055", "8344-150429-162201", "8833-150504-131131", "0184-150429-124639", "0767-150430-105420", "0799-150430-113108", "1784-150430-101515", "2900-150430-114213", "3100-150501-103154", "3253-150430-111321", "3477-150430-113036", "4437-150430-104224", "4756-150411-150337", "4861-150430-112525", "5200-150501-103751", "5107-150430-104415", "5360-150430-090404", "6453-150507-190621", "7074-150507-151336", "1705-170116-142649", "91150406-161756", "9383-160128-125153", "0376-150421-155232", "2043-160310-154642", "23150403-182551", "0220-150422-163321", "3271-150506-120301", "0831-160218-160442", "3138-160219-144523", "3570-160219-145952", "5730-160219-155537", "3603-160220-122613", "4448-160220-193243", "5148-160220-114832", "1924-160220-113523", "6796-160220-221344", "5301-160304-192359", "8226-160305-162320", "9581-160305-115025", "8823-160305-140040", "1429-150427-153420", "5607-150425-102421", "47150406-125841", "7217-160209-132527", "2719-150417-140130", "8501-150421-095920", "45150407-154450", "1067-150419-120449", "5555-160129-181224", "0855-160314-234426", "8195-160314-180632", "2658-160314-185351", "8213-160314-204848", "2521-160314-200255", "9616-160314-214031", "0244-160314-231118", "3845-160314-214240", "1475-160315-001332", "3897-160314-200045", "7710-160319-204556", "6424-160319-215612", "7341-160319-202433", "6897-160319-221313", "8988-160319-224714", "5086-150410-102356", "7920-150413-130841", "2861-150410-135151", "6794-160319-164923", "2163-160319-175204", "8260-160319-182908", "6607-160319-151901", "2549-160319-143150", "5086-160319-123814", "2359-160319-121236", "8319-160319-114531", "52150401-155113", "6429-150410-134158", "8471-150412-173932", "2546-150412-173119", "5430-150413-163549", "4595-150422-114349", "7960-150502-150257", "1556-150502-151010", "1620-150502-152356", "5563-150410-155500", "8462-150422-133254", "4469-150422-140349", "39150404-122803", "89150406-135317", "8733-160220-105912", "3280-160311-151200", "68150404-143104", "1620-150419-083055", "98150402-140537", "4240-150619-124624", "90150402-150047", "5077-150411-114936", "1735-160106-153643", "4305-160313-173609", "4707-160313-172938", "7992-160313-172334", "4292-160313-174602", "2969-160313-170725", "7697-160314-151405", "5247-160314-201702", "2991-160314-202954", "3831-170207-161803", "52150407-113402", "1185-150410-154249", "4181-150415-152826", "63150406-133241", "7082-160225-144210", "6833-150429-151901", "7084-150603-172353", "8010-150421-124203", "5725-150505-114121", "1394-150430-232728", "60150406-153912", "9700-150418-103531", "5407-150420-114851", "0261-150421-122215", "4944-150507-132508", "0878-160225-114919", "4393-150421-155815", "0967-160310-130928", "2599-150421-154955", "6087-160224-152646", "0546-160224-111407", "7909-160312-182449", "0487-160312-190833", "9358-170111-094327", "6379-170112-093250", "9888-150421-113110", "1166-150622-055114", "2952-150624-163859", "7893-150421-160431", "87150404-160809", "2876-150409-150305", "8578-150418-145228", "4050-150424-155306", "70150404-125108", "9618-150416-122424", "8941-150415-152633", "0959-170118-101333", "0774-150427-133052", "0785-160304-122835", "9791-150422-162245", "22150407-163124", "53150406-105254", "1528-160304-125739", "8238-161224-112812", "3531-150421-143805", "4278-150416-175043", "7787-150411-145051", "56150403-213949", "6604-150416-125428", "4857-150421-080322", "0023-160316-130354", "1472-160312-141517", "7735-150421-182347", "50150403-105221", "2813-150408-164256", "3538-150411-104526", "0560-150428-125331", "2763-160203-170600", "0170-160307-001639", "9304-160306-225725", "0171-160306-235900", "2124-160306-232603", "3439-160316-105705", "7980-160316-105741", "5725-150409-102427", "4711-150426-181413", "4892-150413-110154", "0895-150413-112139", "0093-150413-132750", "4698-160201-104955", "8540-150413-110353", "6074-150413-170744", "4574-150620-175558", "5645-150620-182613", "1693-150412-112919", "0158-150427-152718", "2008-150421-095104", "4272-160108-143318", "7884-150413-153150", "4800-150415-103452", "1136-160311-110044", "9656-170111-081909", "33150407-101427", "1266-150427-190250", "2093-170207-122958", "3507-150409-123348", "7541-150412-152809", "2825-150420-092811", "9437-150420-103610", "4992-150409-141335", "3498-150408-231802", "6166-150512-104107", "9768-150427-210434", "0205-150508-120436", "2637-150425-151316", "2513-150425-192936", "4188-150424-144352", "2456-150427-125655", "1660-150507-152429", "3570-150507-110913", "1601-150408-172051", "0148-150419-091522", "1442-161217-164741", "31150403-201030", "0941-170207-002441", "8575-170131-031553", "9965-170131-032636", "5691-170131-033654", "8645-170131-034806", "7939-170131-035846", "3351-170131-041646", "9837-170131-042413", "2633-170131-043329", "2964-170131-045303", "2440-170131-201700", "4894-170201-001106", "7664-160522-105414", "6997-170207-182304", "7197-170207-173901", "7397-160524-140443", "9865-160525-110126", "4556-160522-113614", "4469-150409-124255" };

            jntuh_registered_faculty1 = jntuh_registered_faculty1.Where(s => !RegNosNEW.Contains(s.RegistrationNumber)).ToList();

            var jntuh_registered_faculty = jntuh_registered_faculty1.Select(rf => new
            {
                RegistrationNumber = rf.RegistrationNumber,
                DepartmentId = rf.DeptId != null ? jntuh_departments.Where(e => e.id == rf.DeptId).Select(e => e.id).FirstOrDefault() : DeptNameBasedOnSpecialization.Where(e => e.Specid == rf.SpecializationId).Select(e => e.DeptId).FirstOrDefault(),
                Department = rf.DeptId != null ? jntuh_departments.Where(e => e.id == rf.DeptId).Select(e => e.departmentName).FirstOrDefault() : DeptNameBasedOnSpecialization.Where(e => e.Specid == rf.SpecializationId).Select(e => e.DeptName).FirstOrDefault(),
                HighestDegree = jntuh_education_category.Where(c => c.id == rf.HighestDegreeID).Select(c => c.educationCategoryName).FirstOrDefault(),
                Recruitedfor = jntuh_college_faculty_registered.Where(c => c.RegistrationNumber.Trim() == rf.RegistrationNumber.Trim()).Select(c => c.IdentifiedFor).FirstOrDefault(),
                // NotqualifiedAsperAICTE = rf.NotqualifiedAsperAICTE == null ? false : rf.NotqualifiedAsperAICTE,
                rf.SpecializationId


            }).Where(e => e.Department != null).ToList();

            jntuh_registered_faculty = jntuh_registered_faculty.Distinct().ToList();

            List<CollegeFacultyWithIntakeReport> intakedetailsList = StudentAndClasswork(collegeId);
            List<CollegeFacultyWithIntakeReport> intakedetailsList1 = new List<CollegeFacultyWithIntakeReport>();

            foreach (var item in intakedetailsList)
            {

                var intakedetails = new CollegeFacultyWithIntakeReport();
                int phdFaculty; int pgFaculty; int ugFaculty; int SpecializationpgFaculty = 0; int SpecializationphdFaculty = 0;

                intakedetails.collegeId = item.collegeId;
                intakedetails.collegeCode = jntuh_college.Where(c => c.id == item.collegeId).Select(c => c.collegeCode).FirstOrDefault();
                intakedetails.collegeName = jntuh_college.Where(c => c.id == item.collegeId).Select(c => c.collegeName).FirstOrDefault();
                intakedetails.academicYearId = item.academicYearId;
                intakedetails.RegistrationNumber = item.RegistrationNumber;
                intakedetails.Degree = item.Degree;
                intakedetails.Department = item.Department;
                intakedetails.Specialization = item.Specialization;
                intakedetails.DegreeID = item.DegreeID;
                intakedetails.specializationId = item.specializationId;
                intakedetails.DepartmentID = item.DepartmentID;
                intakedetails.shiftId = item.shiftId;
                intakedetails.approvedIntake1 = item.approvedIntake1;

                facultystudentRatio = Convert.ToInt32(jntuh_faculty_student_ratio_norms.Where(fn => fn.degreeId == item.DegreeID).Select(fn => fn.Norms).FirstOrDefault().Split(':')[1].ToString());

                if (item.Degree == "B.Tech")
                {

                    intakedetails.admittedIntake2 = GetBtechAdmittedIntake(item.approvedIntake2);
                    intakedetails.admittedIntake3 = GetBtechAdmittedIntake(item.approvedIntake3);
                    intakedetails.admittedIntake4 = GetBtechAdmittedIntake(item.approvedIntake4);

                    intakedetails.totalIntake = (intakedetails.admittedIntake2) + (intakedetails.admittedIntake3) +
                                                (intakedetails.admittedIntake4);
                    facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) /
                                   Convert.ToDecimal(facultystudentRatio);
                }
                else if (item.Degree == "M.Tech" || item.Degree == "MBA")
                {
                    intakedetails.totalIntake = (item.approvedIntake1) + (item.approvedIntake2);
                    facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) /
                                   Convert.ToDecimal(facultystudentRatio);
                    facultyRatio = Math.Round(facultyRatio);

                    if (item.Degree == "M.Tech")
                    {
                        if ((int)facultyRatio == 0)
                            facultyRatio = 0;
                        else if ((int)facultyRatio <= 3)
                            facultyRatio = 3;
                        else
                            facultyRatio = facultyRatio;
                    }


                }
                else if (item.Degree == "MCA")
                {
                    intakedetails.totalIntake = (item.approvedIntake1) + (item.approvedIntake2) +
                                                (item.approvedIntake3);
                    facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) /
                                   Convert.ToDecimal(facultystudentRatio);
                    
                }
                else if (item.Degree == "B.Pharmacy")
                {
                    intakedetails.totalIntake = (item.approvedIntake1) + (item.approvedIntake2) +
                                         (item.approvedIntake3) + (item.approvedIntake4);
                    var BpharData = intakedetailsList.Where(s => s.academicYearId == 9).ToList();

                    var approvedIntake = BpharData.Where(s => s.Degree == "B.Pharmacy").Select(s => s.approvedIntake1).SingleOrDefault();
                    if (approvedIntake == 0 || approvedIntake == null)
                    {


                    }
                    else
                    {
                        var pharm = BpharData.Where(s => s.Degree == "Pharm.D" || s.Degree == "Pharm.D PB").ToList();
                        if (approvedIntake <= 60)
                        {
                            if (pharm.Count == 0 || pharm.Count == null)
                            {
                                facultyRatio = 15;
                            }
                            else
                            {
                                facultyRatio = 26;
                            }
                        }
                        else
                        {
                            if (pharm.Count == 0 || pharm.Count == null)
                            {
                                facultyRatio = 25;
                            }
                            else
                            {
                                facultyRatio = 36;
                            }
                        }
                    }

                }
                else if (item.Degree == "M.Pharmacy")
                {
                    intakedetails.totalIntake = (item.approvedIntake1) + (item.approvedIntake2);
                    facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) / facultystudentRatio;
                    facultyRatio = Math.Round(facultyRatio);
                   
                  //  decimal size = Convert.ToDecimal(3);
                  
                    if ((int)facultyRatio == 0)
                        facultyRatio = 0;
                    else if ((int)facultyRatio <= 3)
                        facultyRatio = 3;
                    else
                        facultyRatio = facultyRatio;


                }
                else if (item.Degree == "Pharm.D")
                {
                    intakedetails.totalIntake = (item.approvedIntake1) + (item.approvedIntake2) +
                                                (item.approvedIntake3) + (item.approvedIntake4) +
                                                (item.approvedIntake5);
                    facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) / facultystudentRatio;
                }
                else if (item.Degree == "Pharm.D PB")
                {
                    intakedetails.totalIntake = (item.approvedIntake1) + (item.approvedIntake2);
                    facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) / facultystudentRatio;
                }
                else //MAM MTM
                {
                    intakedetails.totalIntake = (item.approvedIntake1) + (item.approvedIntake2) +
                                                (item.approvedIntake3) + (item.approvedIntake4) +
                                                (item.approvedIntake5);
                    facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) / Convert.ToDecimal(facultystudentRatio);
                }

                intakedetails.requiredFaculty = Math.Round(facultyRatio);
                intakedetails.degreeDisplayOrder = item.degreeDisplayOrder;


                if (item.Degree == "M.Tech" || item.Degree == "MCA" || item.Degree == "MBA" || item.Degree == "MAM")
                {

                    if (item.Degree == "M.Tech")
                    {
                        if (intakedetails.shiftId == 1)
                        {
                            if (item.approvedIntake1 == 0)
                            {
                                if (intakedetails.totalIntake == 0)
                                {
                                    SpecializationphdFaculty = 0;
                                }
                                else
                                {
                                    SpecializationphdFaculty = 1;
                                }

                            }
                            else
                            {
                                SpecializationphdFaculty = IntakeWisePhdForMtech(item.approvedIntake1);
                            }


                        }
                    }
                    else if (item.Degree == "MCA" || item.Degree == "MBA")
                    {
                        if (intakedetails.shiftId == 1)
                        {
                            if (item.approvedIntake1 == 0)
                            {
                                if (intakedetails.totalIntake == 0)
                                {
                                    SpecializationphdFaculty = 0;
                                }
                                else
                                {
                                    SpecializationphdFaculty = 1;
                                }

                            }
                            else
                            {
                                SpecializationphdFaculty = IntakeWisePhdForMBAandMCA(item.approvedIntake1);
                            }


                        }

                        // SpecializationphdFaculty = IntakeWisePhdForMBAandMCA(item.approvedIntake1);
                    }
                    else if (item.Degree == "MAM")
                    {
                        SpecializationphdFaculty = IntakeWisePhdForMBAandMCA(item.approvedIntake1);
                    }


                }
                else if (item.Degree == "B.Tech")
                {
                    SpecializationphdFaculty = IntakeWisePhdForBtech(item.approvedIntake1);
                }
                else if (item.Degree == "M.Pharmacy")
                {
                    if(item.shiftId ==1)
                    {
                        if (intakedetails.totalIntake == 0 || intakedetails.totalIntake == null)
                        {
                            SpecializationphdFaculty = 0;
                        }
                        else
                        {
                            SpecializationphdFaculty = 1;
                        }
                       
                    }
                    
                }

                if (item.Degree == "B.Pharmacy")
                {

                    ugFaculty = jntuh_registered_faculty.Count(f => ("UG" == f.HighestDegree) && f.Department == "B.Pharmacy");
                    pgFaculty = jntuh_registered_faculty.Count(f => "PG" == f.HighestDegree && f.Department == "B.Pharmacy");
                    phdFaculty = jntuh_registered_faculty.Count(f => "Ph.D" == f.HighestDegree && f.Department == "B.Pharmacy");
                    intakedetails.NonPhdRegNos = jntuh_registered_faculty.Where(f => "PG" == f.HighestDegree && f.Department == "B.Pharmacy").Select(r => r.RegistrationNumber).ToArray();
                    intakedetails.PhdRegNos = jntuh_registered_faculty.Where(f => "Ph.D" == f.HighestDegree && f.Department == "B.Pharmacy").Select(r => r.RegistrationNumber).ToArray();
                    intakedetails.Department = "Pharmacy";
                }
                else
                    if (item.Degree == "M.Pharmacy")
                    {

                        ugFaculty = jntuh_registered_faculty.Count(f => ("UG" == f.HighestDegree) && f.Department == "M.Pharmacy" && f.SpecializationId == item.specializationId);
                        pgFaculty = jntuh_registered_faculty.Count(f => ("PG" == f.HighestDegree || "M.Phil" == f.HighestDegree) &&
                                    f.Department == "M.Pharmacy" && f.SpecializationId == item.specializationId);
                        phdFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && f.Department == "M.Pharmacy" && f.SpecializationId == item.specializationId);
                        intakedetails.NonPhdRegNos = jntuh_registered_faculty.Where(f => ("PG" == f.HighestDegree || "M.Phil" == f.HighestDegree) &&
                                    f.Department == "M.Pharmacy" && f.SpecializationId == item.specializationId).Select(r => r.RegistrationNumber).ToArray();
                        intakedetails.PhdRegNos = jntuh_registered_faculty.Where(f => ("Ph.D" == f.HighestDegree) && f.Department == "M.Pharmacy" && f.SpecializationId == item.specializationId).Select(r => r.RegistrationNumber).ToArray();

                    }

                    else if (item.Degree == "Pharm.D")
                    {
                        ugFaculty = jntuh_registered_faculty.Count(f => ("UG" == f.HighestDegree) && f.Department == "Pharm.D");
                        pgFaculty = jntuh_registered_faculty.Count(f => ("PG" == f.HighestDegree || "M.Phil" == f.HighestDegree) && f.Department == "Pharm.D");
                        phdFaculty = jntuh_registered_faculty.Count(f => "Ph.D" == f.HighestDegree && f.Department == "Pharm.D");
                        intakedetails.NonPhdRegNos = jntuh_registered_faculty.Where(f => ("PG" == f.HighestDegree || "M.Phil" == f.HighestDegree) && f.Department == "Pharm.D").Select(r => r.RegistrationNumber).ToArray();
                        intakedetails.PhdRegNos = jntuh_registered_faculty.Where(f => "Ph.D" == f.HighestDegree && f.Department == "Pharm.D").Select(r => r.RegistrationNumber).ToArray();
                        intakedetails.Department = "Pharm.D";
                    }
                    else if (item.Degree == "Pharm.D PB")
                    {

                        ugFaculty = jntuh_registered_faculty.Count(f => ("UG" == f.HighestDegree) && f.Department == "Pharm.D PB");
                        pgFaculty = jntuh_registered_faculty.Count(f => ("PG" == f.HighestDegree || "M.Phil" == f.HighestDegree) && f.Department == "Pharm.D PB");
                        phdFaculty = jntuh_registered_faculty.Count(f => "Ph.D" == f.HighestDegree && f.Department == "Pharm.D PB");
                        intakedetails.NonPhdRegNos = jntuh_registered_faculty.Where(f => ("PG" == f.HighestDegree || "M.Phil" == f.HighestDegree) && f.Department == "Pharm.D PB").Select(r => r.RegistrationNumber).ToArray();
                        intakedetails.PhdRegNos = jntuh_registered_faculty.Where(f => "Ph.D" == f.HighestDegree && f.Department == "Pharm.D PB").Select(r => r.RegistrationNumber).ToArray();
                        intakedetails.Department = "Pharm.D PB";
                    }

                    else if (item.Degree == "B.Tech")
                    {
                        var sss = jntuh_registered_faculty.Where(f => f.DepartmentId == item.DepartmentID).ToList();
                        ugFaculty = jntuh_registered_faculty.Count(f => ("UG" == f.HighestDegree) && f.DepartmentId == item.DepartmentID);
                        pgFaculty = jntuh_registered_faculty.Count(f => ("PG" == f.HighestDegree || "M.Phil" == f.HighestDegree) && f.DepartmentId == item.DepartmentID);
                        phdFaculty = jntuh_registered_faculty.Count(f => "Ph.D" == f.HighestDegree && f.DepartmentId == item.DepartmentID);
                        intakedetails.NonPhdRegNos = jntuh_registered_faculty.Where(f => ("PG" == f.HighestDegree || "M.Phil" == f.HighestDegree) && f.DepartmentId == item.DepartmentID).Select(r => r.RegistrationNumber).ToArray();
                        intakedetails.PhdRegNos = jntuh_registered_faculty.Where(f => "Ph.D" == f.HighestDegree && f.DepartmentId == item.DepartmentID).Select(r => r.RegistrationNumber).ToArray();
                    }
                    else if (item.Degree == "M.Tech")
                    {

                        ugFaculty = jntuh_registered_faculty.Count(f => ("UG" == f.HighestDegree) && f.SpecializationId == item.specializationId && f.DepartmentId == item.DepartmentID);
                        pgFaculty = jntuh_registered_faculty.Count(f => ("PG" == f.HighestDegree || "M.Phil" == f.HighestDegree) && f.SpecializationId == item.specializationId && f.DepartmentId == item.DepartmentID);
                        phdFaculty = jntuh_registered_faculty.Count(f => "Ph.D" == f.HighestDegree && f.SpecializationId == item.specializationId && f.DepartmentId == item.DepartmentID);
                        intakedetails.NonPhdRegNos = jntuh_registered_faculty.Where(f => ("PG" == f.HighestDegree || "M.Phil" == f.HighestDegree) && f.SpecializationId == item.specializationId && f.DepartmentId == item.DepartmentID).Select(r => r.RegistrationNumber).ToArray();
                        intakedetails.PhdRegNos = jntuh_registered_faculty.Where(f => "Ph.D" == f.HighestDegree && f.SpecializationId == item.specializationId && f.DepartmentId == item.DepartmentID).Select(r => r.RegistrationNumber).ToArray();
                    }
                    else
                    {

                        ugFaculty = jntuh_registered_faculty.Count(f => ("UG" == f.HighestDegree) && f.Department == item.Department);
                        pgFaculty = jntuh_registered_faculty.Count(f => ("PG" == f.HighestDegree || "M.Phil" == f.HighestDegree) && f.Department == item.Department);
                        phdFaculty = jntuh_registered_faculty.Count(f => "Ph.D" == f.HighestDegree && f.Department == item.Department);
                        intakedetails.NonPhdRegNos = jntuh_registered_faculty.Where(f => ("PG" == f.HighestDegree || "M.Phil" == f.HighestDegree) && f.Department == item.Department).Select(r => r.RegistrationNumber).ToArray();
                        intakedetails.PhdRegNos = jntuh_registered_faculty.Where(f => "Ph.D" == f.HighestDegree && f.Department == item.Department).Select(r => r.RegistrationNumber).ToArray();

                    }

                intakedetails.phdFaculty = phdFaculty;
                intakedetails.SpecializationsphdFaculty = SpecializationphdFaculty;
                intakedetails.pgFaculty = pgFaculty;
                intakedetails.ugFaculty = ugFaculty;
                intakedetails.totalFaculty = (ugFaculty + pgFaculty + phdFaculty);
                intakedetailsList1.Add(intakedetails);

            }

            string[] strOtherDepartments = { "English", "Mathematics", "Physics", "Chemistry", "Others(CSE/IT)", "Others(CIVIL/MECH)", "Others(ECE/EEE)", "Others(MNGT/H&S)" };
            var btechdegreecount = intakedetailsList1.Count(d => d.Degree == "B.Tech");
            if (btechdegreecount > 0)
            {
                foreach (var department in strOtherDepartments)
                {
                    int academicYearId = 9;
                    var deptid = jntuh_departments.Where(i => i.departmentName == department).Select(i => i.id).FirstOrDefault();
                    var deptname = jntuh_departments.Where(i => i.departmentName == department).Select(i => i.departmentName).FirstOrDefault();
                    int speId = jntuh_specialization.Where(s => s.jntuh_department.departmentName == department).Select(s => s.id).FirstOrDefault();
                    int ugFaculty = jntuh_registered_faculty.Count(f => "UG" == f.HighestDegree && f.Department == department);
                    int pgFaculty = jntuh_registered_faculty.Count(f => ("PG" == f.HighestDegree || "M.Phil" == f.HighestDegree) && f.Department == department);
                    var pgreg = jntuh_registered_faculty.Where(f => ("PG" == f.HighestDegree || "M.Phil" == f.HighestDegree) && f.Department == department).Select(e => e.RegistrationNumber).ToList();
                    int phdFaculty = jntuh_registered_faculty.Count(f => "Ph.D" == f.HighestDegree && f.Department == department);
                    string[] RegNos = jntuh_registered_faculty.Where(f => "Ph.D" == f.HighestDegree && f.Department == department).Select(e => e.RegistrationNumber).ToArray();
                    string[] NonRegNos = jntuh_registered_faculty.Where(f => ("PG" == f.HighestDegree || "M.Phil" == f.HighestDegree) && f.Department == department).Select(e => e.RegistrationNumber).ToArray();
                    intakedetailsList1.Add(new CollegeFacultyWithIntakeReport
                    {
                        collegeId = (int)collegeId,
                        academicYearId = academicYearId,
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
                        PhdRegNos = RegNos,
                        NonPhdRegNos = NonRegNos,
                        approvedIntake1 = 1
                    });
                }
            }

            return intakedetailsList1;
        }

        private int SurpriseGetIntake(int collegeId, int academicYearId, int specializationId, int shiftId, int flag, int DegreeId)
        {
            int intake = 0;
            int[] IntegratedCollegeIds = { 9, 11, 26, 32, 38, 68, 70, 108, 109, 134, 171, 179, 180, 183, 192, 196, 198, 335, 367, 374, 399 };

            // Degree B.Tech  
            if (DegreeId == 4)
            {
                //admitted
                if (flag == 1 && (academicYearId == 8 || academicYearId == 3 || academicYearId == 2))
                {
                    intake = db.jntuh_college_intake_existing.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == shiftId).Select(i => i.admittedIntake).FirstOrDefault();
                }
                else if (flag == 1 && academicYearId == 9)
                {
                    var inta = db.jntuh_approvedadmitted_intake.FirstOrDefault(i => i.collegeId == collegeId && i.AcademicYearId == academicYearId && i.SpecializationId == specializationId && i.ShiftId == shiftId);
                    if (inta != null)
                    {
                        intake = Convert.ToInt32(inta.ApprovedIntake);
                    }

                }
                else   //approved
                {
                    intake = db.jntuh_approvedadmitted_intake.Where(i => i.collegeId == collegeId && i.AcademicYearId == academicYearId && i.SpecializationId == specializationId && i.ShiftId == shiftId).Select(i => i.ApprovedIntake).FirstOrDefault();
                }
            }
            else
            {
                //approved
                intake = db.jntuh_approvedadmitted_intake.Where(i => i.collegeId == collegeId && i.AcademicYearId == academicYearId && i.SpecializationId == specializationId && i.ShiftId == shiftId).Select(i => i.ApprovedIntake).FirstOrDefault();
            }


            return intake;
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

        private int IntakeWisePhdForMtech(int Intake)
        {
            int Phdcount = 0;
            if (Intake > 0 && Intake <= 30)
            {
                Phdcount = 2;
            }
            else if (Intake > 30 && Intake <= 60)
            {
                Phdcount = 4;
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

        // public class BasNotice
        //{
        //    public int CollegeId { get; set; }
        //    public string CollegeCode { get; set; }
        //    public int DeptId { get; set; }
        //    public string RegistrationNo { get; set; }
        //    public string one { get; set; }
        //    public string two { get; set; }
        //    public string three { get; set; }
        //    public string four { get; set; }
        //    public string five { get; set; }
        //    public string six { get; set; }
        //    public string seven { get; set; }
        //    public string eight { get; set; }
        //    public string nine { get; set; }
        //    public string ten { get; set; }
        //    public string eleven { get; set; }
        //    public string twelve { get; set; }
        //    public string THIRTEEN { get; set; }
        //    public string Fourteen { get; set; }
        //    public string Fifthteen { get; set; }
        //    public string sixteen { get; set; }
        //    public string Sevenghteen { get; set; }
        //    public string Eightghteen { get; set; }
        //    public string Nineghteen { get; set; }
        //    public string Twenty { get; set; }
        //    public string Twentyone { get; set; }
        //    public string Twentytwo { get; set; }
        //    public string Twentythree { get; set; }
        //    public string Twentyfour { get; set; }
        //    public string Twentyfive { get; set; }
        //    public string Twentysix { get; set; }
        //    public string Twentyseven { get; set; }
        //    public string Twentyeight { get; set; }
        //    public string Twentynine { get; set; }
        //    public string THIRTY { get; set; }
        //    public string THIRTYone { get; set; }
        //    public double Totalpercentage { get; set; }
        //}

        public class PHDFaculty
        {
            public string[] RegistrationNumber { get; set; }
        }

        #region Excel file For all colleges per BASNotice
        public FacultyCountNew SurpriseStaffCount(int? collegeID, string type)
        {
            TotalCount = 0;
            TotalCount1 = 0;
            facultyCountnew = 0;
            facultyCountnew1 = 0;
            PHDPresentCount = 0;
            DifferenceCount = 0;
            Btech_Faculty_ProposedCount = 0;
            BTech_PHD_Count_New = 0;
            MTech_PHD_Count_New = 0;
            MCA_PHD_Count_New = 0;
            MBA_PHD_Count_New = 0;
            MAM_PHD_Count_New = 0;
            string StaffDetails = string.Empty;
            //string RequiredFacultyDetails = string.Empty;
            string SheetMonthName = string.Empty;
            string MonthName = string.Empty;
            int count = 1;
            int facultystudentRatio = 0;
            monthCount = 15;
            var jntuh_faculty_student_ratio_norms = db.jntuh_faculty_student_ratio_norms.ToList();


            List<CollegeFacultyWithIntakeReport> intakedetailsList = Get_PHD_Faculty(collegeID);

            List<string> New_PHD_Count1 = new List<string>();

            PHD_Count = intakedetailsList.Where(i => i.shiftId == 1).Select(i => i.phdFaculty).Sum();

            foreach (var item in intakedetailsList.Where(i => i.shiftId == 1).ToList())
            {
                foreach (var item1 in item.PhdRegNos)
                {
                    New_PHD_Count1.Add(item1);
                }

            }




            string[] strOtherDepartments = { "English", "Mathematics", "Physics", "Chemistry", "Others(CSE/IT)", "Others(CIVIL/MECH)", "Others(ECE/EEE)", "Others(MNGT/H&S)" };

            int totalBtechFirstYearIntake = intakedetailsList.Where(d => d.Degree == "B.Tech" && !strOtherDepartments.Contains(d.Department)).Select(d => d.approvedIntake1).Sum();

            string[] ProposedIntakeZeroDeptName = intakedetailsList.Where(e => e.Degree == "B.Tech" && (e.approvedIntake1 == 0) && !strOtherDepartments.Contains(e.Department)).Select(e => e.Specialization).Distinct().ToArray();

            List<jntuh_college_attendace> LoginList = new List<jntuh_college_attendace>();
            LoginList = db.jntuh_college_attendace.Where(a => a.Collegeid == collegeID && a.Month == type).ToList();

            string[] RegNos = new string[] { "9211-150409-180107", "6317-150413-162713", "96150406-153506", "78150407-104506", "90150406-132723", "5264-150409-160644", "8518-150413-150717", "7995-150414-143317", "4687-150420-162710", "1549-150418-123933", "8891-150420-165037", "2375-150427-184049", "5612-170202-102750", "9234-160302-113847", "49150406-131709", "4261-160308-074435", "4171-150624-143950", "0707-150408-175902", "74150406-141818", "16150402-155455", "54150402-122257", "6348-150411-120653", "6058-150417-174615", "8105-150412-154535", "8558-160312-194806", "1116-160313-055503", "3205-160313-105704", "7748-160313-063954", "2328-150408-154626", "8922-150410-144623", "6400-150409-153507", "4241-150413-144757", "5387-150420-124115", "6442-150413-163535", "3141-160302-143355", "2624-170202-123226", "2778-150621-184256", "6002-150408-104417", "7601-150409-104220", "5724-150409-131120", "9549-150409-131219", "5480-150416-141938", "8460-150505-150705", "5325-150411-101220", "9134-150418-123351", "1235-170107-122241", "3995-150420-172501", "5157-151216-105135", "5993-170208-144156", "0827-160314-154038", "43150407-121703", "9675-170201-060349", "1761-170110-124328", "8780-170212-221004", "42150406-105659", "9741-150408-163934", "2195-150408-145520", "7984-160128-143658", "0756-160128-144719", "8629-160128-141146", "6328-160129-102154", "6339-160128-152920", "5003-150410-100751", "3111-150413-163611", "6889-150413-145248", "7968-170117-141626", "4390-150619-131209", "7037-150619-123808", "6090-150416-211652", "0650-150419-232914", "9644-150420-123408", "2204-150506-223734", "5007-160303-052952", "1221-160111-140608", "5323-160316-172042", "5758-150409-145824", "9690-150419-143601", "2845-160318-215132", "6585-150414-151718", "6518-160222-100511", "37150407-125255", "4939-150419-180848", "7509-150415-191610", "7038-150418-123607", "4375-150420-144936", "4925-160204-183924", "5271-160206-142514", "9379-150417-164707", "4561-160211-082340", "4517-150424-122333", "4503-150419-134140", "8391-150419-133206", "7549-150409-171452", "2289-150418-215024", "5999-150418-172740", "7521-150418-223714", "4740-150419-162142", "9374-150419-225506", "0752-150422-165906", "6713-150422-171112", "7419-150420-154436", "0059-150420-164303", "7867-160309-173829", "0152-160309-181208", "50150406-110631", "4769-150416-174812", "8005-150416-145941", "2516-150422-195357", "8508-150412-113146", "1003-170119-133836", "4904-170119-140309", "0588-170126-140848", "8291-150624-160237", "6023-151229-151344", "4534-150408-104328", "9794-160229-175017", "3909-151230-121957", "68150407-154601", "1147-150412-155421", "6851-170208-173310", "8532-170213-171543", "77150407-144017", "4281-150415-144925", "8238-161024-112302", "1949-150421-144423", "3666-150408-102957", "1854-150414-123210", "8628-150414-121943", "2834-150414-131904", "8474-150420-113501", "5314-150422-160253", "6998-150422-145056", "8739-150422-124734", "5241-150422-143019", "1831-150422-122747", "7170-150422-115651", "4386-150422-113334", "1097-150422-111446", "2563-150409-185844", "6483-150409-120156", "8889-150423-121239", "3174-150423-145633", "7141-150416-152448", "3831-150426-164813", "1364-150426-165916", "8420-150426-163755", "0778-150501-170330", "2771-150622-225307", "8776-150622-232358", "7503-150424-104100", "9155-150622-231402", "7447-150525-115526", "9731-150622-154029", "3217-150409-132413", "2351-160302-144825", "8336-160302-171055", "93150406-161145", "67150407-173554", "7618-150410-162949", "9986-150619-175911", "94150404-124125", "00150407-113102", "2180-150424-222555", "1126-150507-163545", "1808-150621-133359", "4266-150622-125140", "7977-150623-115417", "7690-150415-134357", "6734-150415-133015", "4696-150409-112735", "9742-150409-105139", "5207-150409-145858", "88150407-162910", "7306-150411-145154", "2980-150411-105638", "9381-150411-113008", "7210-150428-042100", "2316-150428-040056", "5018-170111-130147", "2421-170118-102956", "0676-150408-111501", "2469-160121-104311", "94150402-171218", "9492-160121-160214", "2681-150625-150854", "6076-160311-171551", "0661-160312-141716", "2280-150408-151134", "0208-150411-124221", "8076-150409-161147", "5204-150412-144814", "4443-150412-150351", "9010-150412-151234", "3264-150418-173255", "0922-150415-111243", "0852-150429-154729", "4149-150621-173825", "6620-150625-180248", "7810-161226-114059", "4815-160201-103423", "6809-150505-133845", "0067-150505-141355", "5661-150505-153407", "5246-150505-151341", "7132-150505-171425", "9033-150430-105308", "3057-150506-113420", "8389-150506-170835", "2511-150506-173242", "4009-150408-154747", "8440-150624-131011", "9393-160306-105928", "4283-160315-194106", "3404-170119-172558", "7762-170213-131706", "1475-150414-183509", "7134-160218-150219", "0414-150427-153301", "2067-150414-155501", "33150330-155256", "87150406-194332", "8362-150409-110622", "6180-150408-151453", "7098-150426-074226", "7386-150414-125915", "1466-170105-194422", "1997-170112-095532", "5841-170125-113934", "5612-170125-124150", "1782-150408-121508", "1781-150409-114620", "47150407-153633", "6498-150414-132024", "9077-150416-124631", "5914-150504-100508", "0940-150504-125126", "7835-150506-141716", "6244-150622-125503", "8145-150623-151823", "4830-150623-163945", "6188-150413-160703", "3295-151219-103214", "8352-150625-115421", "7697-150625-114147", "7437-150412-112238", "6903-150506-125905", "8943-150507-132216", "3855-150507-145302", "5572-150507-124125", "3430-150507-155250", "3059-150516-165055", "0336-150507-122950", "1400-150507-152048", "4765-170522-104257", "09150402-160427", "6421-151221-144330", "7885-160312-140130", "0052-160312-131632", "0127-160312-141609", "3725-160312-151706", "9069-160312-151048", "7922-160312-160244", "0682-160312-172016", "2685-160312-172344", "7093-160312-154220", "3195-160314-232954", "5912-150408-122341", "84150406-142457", "7913-150417-131457", "6808-150424-004516", "7368-150419-164942", "3038-150413-112451", "58150406-100658", "68150404-140248", "10150404-134548", "61150404-162737", "56150407-160735", "8021-150409-165612", "8901-150430-111830", "1584-150409-112324", "9049-150416-103205", "95150404-130909", "1083-150417-152920", "1809-150428-104045", "7477-150428-103245", "9865-150426-124940", "6104-150427-134207", "6315-160225-130444", "8441-150413-113304", "9811-150409-103922", "8964-150420-121821", "4723-160311-151729", "03150402-152426", "9373-150426-135654", "8422-150410-113036", "9552-150410-111820", "4713-150420-124126", "6087-170212-130603", "9966-170211-170427", "0319-150415-110441", "9914-150414-143319", "8688-150413-214801", "4046-150507-120439", "5688-160311-160356", "37150406-124138", "86150406-152725", "6080-160305-164900", "7098-160309-162246", "3941-160309-164350", "0502-170213-143616", "6329-170131-185402", "1338-150415-172930", "0443-150620-112146", "39150405-125954", "4439-150422-115107", "6970-150415-123739", "5363-150417-125713", "6048-160225-114522", "8477-160301-195215", "7934-160301-193415", "7595-150423-120919", "5464-150427-154315", "1054-160102-160709", "15150407-161016", "2585-150410-170055", "8344-150429-162201", "8833-150504-131131", "0184-150429-124639", "0767-150430-105420", "0799-150430-113108", "1784-150430-101515", "2900-150430-114213", "3100-150501-103154", "3253-150430-111321", "3477-150430-113036", "4437-150430-104224", "4756-150411-150337", "4861-150430-112525", "5200-150501-103751", "5107-150430-104415", "5360-150430-090404", "6453-150507-190621", "7074-150507-151336", "1705-170116-142649", "91150406-161756", "9383-160128-125153", "0376-150421-155232", "2043-160310-154642", "23150403-182551", "0220-150422-163321", "3271-150506-120301", "0831-160218-160442", "3138-160219-144523", "3570-160219-145952", "5730-160219-155537", "3603-160220-122613", "4448-160220-193243", "5148-160220-114832", "1924-160220-113523", "6796-160220-221344", "5301-160304-192359", "8226-160305-162320", "9581-160305-115025", "8823-160305-140040", "1429-150427-153420", "5607-150425-102421", "47150406-125841", "7217-160209-132527", "2719-150417-140130", "8501-150421-095920", "45150407-154450", "1067-150419-120449", "5555-160129-181224", "0855-160314-234426", "8195-160314-180632", "2658-160314-185351", "8213-160314-204848", "2521-160314-200255", "9616-160314-214031", "0244-160314-231118", "3845-160314-214240", "1475-160315-001332", "3897-160314-200045", "7710-160319-204556", "6424-160319-215612", "7341-160319-202433", "6897-160319-221313", "8988-160319-224714", "5086-150410-102356", "7920-150413-130841", "2861-150410-135151", "6794-160319-164923", "2163-160319-175204", "8260-160319-182908", "6607-160319-151901", "2549-160319-143150", "5086-160319-123814", "2359-160319-121236", "8319-160319-114531", "52150401-155113", "6429-150410-134158", "8471-150412-173932", "2546-150412-173119", "5430-150413-163549", "4595-150422-114349", "7960-150502-150257", "1556-150502-151010", "1620-150502-152356", "5563-150410-155500", "8462-150422-133254", "4469-150422-140349", "39150404-122803", "89150406-135317", "8733-160220-105912", "3280-160311-151200", "68150404-143104", "1620-150419-083055", "98150402-140537", "4240-150619-124624", "90150402-150047", "5077-150411-114936", "1735-160106-153643", "4305-160313-173609", "4707-160313-172938", "7992-160313-172334", "4292-160313-174602", "2969-160313-170725", "7697-160314-151405", "5247-160314-201702", "2991-160314-202954", "3831-170207-161803", "52150407-113402", "1185-150410-154249", "4181-150415-152826", "63150406-133241", "7082-160225-144210", "6833-150429-151901", "7084-150603-172353", "8010-150421-124203", "5725-150505-114121", "1394-150430-232728", "60150406-153912", "9700-150418-103531", "5407-150420-114851", "0261-150421-122215", "4944-150507-132508", "0878-160225-114919", "4393-150421-155815", "0967-160310-130928", "2599-150421-154955", "6087-160224-152646", "0546-160224-111407", "7909-160312-182449", "0487-160312-190833", "9358-170111-094327", "6379-170112-093250", "9888-150421-113110", "1166-150622-055114", "2952-150624-163859", "7893-150421-160431", "87150404-160809", "2876-150409-150305", "8578-150418-145228", "4050-150424-155306", "70150404-125108", "9618-150416-122424", "8941-150415-152633", "0959-170118-101333", "0774-150427-133052", "0785-160304-122835", "9791-150422-162245", "22150407-163124", "53150406-105254", "1528-160304-125739", "8238-161224-112812", "3531-150421-143805", "4278-150416-175043", "7787-150411-145051", "56150403-213949", "6604-150416-125428", "4857-150421-080322", "0023-160316-130354", "1472-160312-141517", "7735-150421-182347", "50150403-105221", "2813-150408-164256", "3538-150411-104526", "0560-150428-125331", "2763-160203-170600", "0170-160307-001639", "9304-160306-225725", "0171-160306-235900", "2124-160306-232603", "3439-160316-105705", "7980-160316-105741", "5725-150409-102427", "4711-150426-181413", "4892-150413-110154", "0895-150413-112139", "0093-150413-132750", "4698-160201-104955", "8540-150413-110353", "6074-150413-170744", "4574-150620-175558", "5645-150620-182613", "1693-150412-112919", "0158-150427-152718", "2008-150421-095104", "4272-160108-143318", "7884-150413-153150", "4800-150415-103452", "1136-160311-110044", "9656-170111-081909", "33150407-101427", "1266-150427-190250", "2093-170207-122958", "3507-150409-123348", "7541-150412-152809", "2825-150420-092811", "9437-150420-103610", "4992-150409-141335", "3498-150408-231802", "6166-150512-104107", "9768-150427-210434", "0205-150508-120436", "2637-150425-151316", "2513-150425-192936", "4188-150424-144352", "2456-150427-125655", "1660-150507-152429", "3570-150507-110913", "1601-150408-172051", "0148-150419-091522", "1442-161217-164741", "31150403-201030", "0941-170207-002441", "8575-170131-031553", "9965-170131-032636", "5691-170131-033654", "8645-170131-034806", "7939-170131-035846", "3351-170131-041646", "9837-170131-042413", "2633-170131-043329", "2964-170131-045303", "2440-170131-201700", "4894-170201-001106", "7664-160522-105414", "6997-170207-182304", "7197-170207-173901", "7397-160524-140443", "9865-160525-110126", "4556-160522-113614", "4469-150409-124255" };

            LoginList = LoginList.Where(l => !RegNos.Contains(l.RegistrationNo)).ToList();


            MonthNameNew = MonthName == null ? SheetMonthName : MonthName;
            StaffDetails += "<p style='text-align:center;'><b><u>Table-I</u></b></p><br/>";
            StaffDetails += "<table border='1' cellpadding='3' cellspacing='0' width='100%' style='font-family: Times New Roman; font-size: 8px; font-weight: normal; background-color: #fff; margin: 0 auto;'>";
            StaffDetails += "<tr>";
            StaffDetails += "<th rowspan='2' style='text-align:center;'>S.No</th>";
            StaffDetails += "<th rowspan='2' style='text-align:center;'>Program</th>";
            StaffDetails += "<th rowspan='2' style='text-align:center;'>Branch / Speciliazation</th>";
            StaffDetails += "<th rowspan='2' style='text-align:center;'>Intake for which affiliation granted</th>";
            StaffDetails += "<th colspan='3' style='text-align:center;'>Faculty shown as per College Portal</th>";
            StaffDetails += "<th colspan='3' style='text-align:center;'>Faculty as per Biometric Attendance for the Month of " + type + "</th>";
            StaffDetails += "<th rowspan='2' style='text-align:center;'>Difference</th>";
            StaffDetails += "</tr>";
            StaffDetails += "<tr>";
            StaffDetails += "<th style='text-align:center;'>With Ph.D</th>";
            StaffDetails += "<th style='text-align:center;'>Without Ph.D</th>";
            StaffDetails += "<th style='text-align:center;'>Total</th>";
            StaffDetails += "<th style='text-align:center;'>With Ph.D</th>";
            StaffDetails += "<th style='text-align:center;'>Without Ph.D</th>";
            StaffDetails += "<th style='text-align:center;'>Total</th>";
            StaffDetails += "</tr>";


            var PHD = LoginList.Where(l => New_PHD_Count1.Contains(l.RegistrationNo)).ToList();



            if (type == "August" || type == "October")
            {
                PHDPresentCount = PHD.Where(s => s.TotalPercentage >= monthCount).Count();

            }
            else
            {
                PHDPresentCount = PHD.Where(s => s.TotalPercentage >= monthCount).Count();

            }



            foreach (var item in intakedetailsList)
            {
                //var IntakeCount = 0;
                var phdpercentagecount = 0;
                var nonphdpercentagecount = 0;
                if (!strOtherDepartments.Contains(item.Department))
                {

                    if (item.Degree == "B.Tech")
                    {
                        Btech_Faculty_ProposedCount += item.approvedIntake1;
                        BTech_PHD_Count_New += item.SpecializationsphdFaculty;

                    }
                    else if (item.Degree == "M.Tech")
                    {
                        Mtech_Faculty_ProposedCount += item.approvedIntake1;

                        MTech_PHD_Count_New += item.SpecializationsphdFaculty;
                    }
                    else if (item.Degree == "B.Pharmacy")
                    {
                        BPharmacy_Faculty_ProposedCount += item.approvedIntake1;
                    }
                    else if (item.Degree == "M.Pharmacy")
                    {
                        Mpharmacy_Faculty_ProposedCount += item.approvedIntake1;
                    }
                    else if (item.Degree == "MBA")
                    {
                        MBA_Faculty_ProposedCount += item.approvedIntake1;
                        MBA_PHD_Count_New += item.SpecializationsphdFaculty;
                    }
                    else if (item.Degree == "MCA")
                    {
                        MCA_Faculty_ProposedCount += item.approvedIntake1;
                        MCA_PHD_Count_New += item.SpecializationsphdFaculty;
                    }
                    else if (item.Degree == "Pharm.D")
                    {
                        Pharm_D_Faculty_ProposedCount += item.approvedIntake1;
                    }
                    else if (item.Degree == "Pharm.D PB")
                    {
                        Pharm_DPB_Faculty_ProposedCount += item.approvedIntake1;
                    }
                    else if (item.Degree == "MAM")
                    {
                        MAM_Faculty_ProposedCount += item.approvedIntake1;
                        MAM_PHD_Count_New += item.SpecializationsphdFaculty;
                    }
                    else
                    {
                        MTM_Faculty_ProposedCount += item.approvedIntake1;
                    }

                    var LoginList_New = LoginList.Where(s => s.EmpDeptID == item.DepartmentID).ToList();

                    if (item.PhdRegNos.Length != 0)
                    {
                        var PhdFacultyList = LoginList_New.Where(r => item.PhdRegNos.Contains(r.RegistrationNo)).ToList();

                        if (type == "August" || type == "October")
                        {
                            phdpercentagecount = PhdFacultyList.Where(s => s.TotalPercentage >= monthCount).Count();


                            if (item.NonPhdRegNos.Length != 0)
                            {
                                var FacultyList = LoginList_New.Where(r => item.NonPhdRegNos.Contains(r.RegistrationNo)).ToList();
                                nonphdpercentagecount = FacultyList.Where(s => s.TotalPercentage >= monthCount).Count();

                            }
                            else
                            {
                                nonphdpercentagecount = 0;
                            }

                        }
                        else
                        {
                            phdpercentagecount = PhdFacultyList.Where(s => s.TotalPercentage >= monthCount).Count();


                            if (item.NonPhdRegNos.Length != 0)
                            {
                                var FacultyList = LoginList_New.Where(r => item.NonPhdRegNos.Contains(r.RegistrationNo)).ToList();
                                nonphdpercentagecount = FacultyList.Where(s => s.TotalPercentage >= monthCount).Count();

                            }
                            else
                            {
                                nonphdpercentagecount = 0;
                            }

                        }



                    }
                    else
                    {
                        if (type == "August" || type == "October")
                        {
                            if (item.NonPhdRegNos.Length != 0)
                            {
                                var FacultyList = LoginList_New.Where(f => item.NonPhdRegNos.Contains(f.RegistrationNo)).ToList();
                                nonphdpercentagecount = FacultyList.Where(s => s.TotalPercentage >= monthCount).Count();

                            }
                            else
                            {
                                nonphdpercentagecount = 0;
                            }

                        }
                        else
                        {
                            if (item.NonPhdRegNos.Length != 0)
                            {
                                var FacultyList = LoginList_New.Where(f => item.NonPhdRegNos.Contains(f.RegistrationNo)).ToList();
                                nonphdpercentagecount = FacultyList.Where(s => s.TotalPercentage >= monthCount).Count();

                            }
                            else
                            {
                                nonphdpercentagecount = 0;
                            }
                        }
                    }
                    StaffDetails += "<tr>";
                    StaffDetails += "<td style='text-align:center;'>" + count + "</td>";
                    StaffDetails += "<td style='text-align:center;'>" + item.Degree + "</td>";
                    if (item.shiftId == 1)
                        StaffDetails += "<td style='text-align:left;'>" + item.Specialization + "</td>";
                    else if (item.shiftId == 2)
                        StaffDetails += "<td style='text-align:left;'>" + item.Specialization + "<span>(II)</span></td>";
                    if (item.approvedIntake1 != 0)
                        StaffDetails += "<td style='text-align:center;'>" + item.approvedIntake1 + "</td>";
                    else if (item.totalIntake != 0)
                        StaffDetails += "<td style='text-align:center;'>" + 0 + "</td>";
                    else
                        StaffDetails += "<td style='text-align:center;'>NA</td>";
                    StaffDetails += "<td style='text-align:center;'>" + item.phdFaculty + "</td>";
                    StaffDetails += "<td style='text-align:center;'>" + (item.ugFaculty + item.pgFaculty) + "</td>";

                    StaffDetails += "<td style='text-align:center;'>" + item.totalFaculty + "</td>";
                    StaffDetails += "<td style='text-align:center;'>" + phdpercentagecount + "</td><td style='text-align:center;'>" + nonphdpercentagecount + "</td><td style='text-align:center;'>" + (phdpercentagecount + nonphdpercentagecount) + "</td><td style='text-align:center;'>" + ((item.ugFaculty + item.pgFaculty + item.phdFaculty) - (phdpercentagecount + nonphdpercentagecount)) + "</td>";
                    StaffDetails += "</tr>";
                    count++;
                    TotalCount += item.totalFaculty;
                    if (item.shiftId == 1)
                        TotalCount1 += (phdpercentagecount + nonphdpercentagecount);
                    else { }
                    if (item.shiftId == 1)
                        DifferenceCount += ((item.ugFaculty + item.pgFaculty + item.phdFaculty) - (phdpercentagecount + nonphdpercentagecount));
                    else { }


                }
                else
                {
                    var LoginList_New = LoginList.Where(s => s.EmpDeptID == item.DepartmentID).ToList();

                    if (item.PhdRegNos.Length != 0)
                    {
                        var PhdFacultyList = LoginList_New.Where(r => item.PhdRegNos.Contains(r.RegistrationNo)).ToList();

                        if (type == "August" || type == "October")
                        {
                            phdpercentagecount = PhdFacultyList.Where(s => s.TotalPercentage >= monthCount).Count();


                            if (item.NonPhdRegNos.Length != 0)
                            {
                                var FacultyList = LoginList_New.Where(r => item.NonPhdRegNos.Contains(r.RegistrationNo)).ToList();
                                nonphdpercentagecount = FacultyList.Where(s => s.TotalPercentage >= monthCount).Count();

                            }
                            else
                            {
                                nonphdpercentagecount = 0;
                            }

                        }
                        else
                        {
                            phdpercentagecount = PhdFacultyList.Where(s => s.TotalPercentage >= monthCount).Count();


                            if (item.NonPhdRegNos.Length != 0)
                            {
                                var FacultyList = LoginList_New.Where(r => item.NonPhdRegNos.Contains(r.RegistrationNo)).ToList();
                                nonphdpercentagecount = FacultyList.Where(s => s.TotalPercentage >= monthCount).Count();

                            }
                            else
                            {
                                nonphdpercentagecount = 0;
                            }

                        }
                    }
                    else
                    {

                        if (type == "August" || type == "October")
                        {
                            if (item.NonPhdRegNos.Length != 0)
                            {
                                var FacultyList = LoginList_New.Where(r => item.NonPhdRegNos.Contains(r.RegistrationNo)).ToList();
                                nonphdpercentagecount = FacultyList.Where(s => s.TotalPercentage >= monthCount).Count();

                            }
                            else
                            {
                                nonphdpercentagecount = 0;
                            }

                        }
                        else
                        {
                            if (item.NonPhdRegNos.Length != 0)
                            {
                                var FacultyList = LoginList_New.Where(r => item.NonPhdRegNos.Contains(r.RegistrationNo)).ToList();
                                nonphdpercentagecount = FacultyList.Where(s => s.TotalPercentage >= monthCount).Count();

                            }
                            else
                            {
                                nonphdpercentagecount = 0;
                            }
                        }
                    }
                    StaffDetails += "<tr>";
                    StaffDetails += "<td style='text-align:center;'>" + count + "</td>";
                    StaffDetails += "<td style='text-align:center;'>" + item.Degree + "</td>";
                    if (item.shiftId == 1)
                        StaffDetails += "<td style='text-align:left;'>" + item.Specialization + "</td>";
                    else if (item.shiftId == 2)
                        StaffDetails += "<td style='text-align:left;'>" + item.Specialization + "<span>(II)</span></td>";
                    StaffDetails += "<td style='text-align:center;'>" + totalBtechFirstYearIntake + "</td>";
                    StaffDetails += "<td style='text-align:center;'>" + item.phdFaculty + "</td>";
                    StaffDetails += "<td style='text-align:center;'>" + (item.ugFaculty + item.pgFaculty) + "</td>";

                    StaffDetails += "<td style='text-align:center;'>" + item.totalFaculty + "</td>";
                    StaffDetails += "<td style='text-align:center;'>" + phdpercentagecount + "</td><td style='text-align:center;'>" + nonphdpercentagecount + "</td><td style='text-align:center;'>" + (phdpercentagecount + nonphdpercentagecount) + "</td><td style='text-align:center;'>" + ((item.ugFaculty + item.pgFaculty + item.phdFaculty) - (phdpercentagecount + nonphdpercentagecount)) + "</td>";
                    StaffDetails += "</tr>";
                    count++;
                    TotalCount += item.totalFaculty;
                    TotalCount1 += (phdpercentagecount + nonphdpercentagecount);
                    DifferenceCount += ((item.ugFaculty + item.pgFaculty + item.phdFaculty) - (phdpercentagecount + nonphdpercentagecount));
                }


            }
            TotalCount = (TotalCount - intakedetailsList.Where(l => l.shiftId == 2).Select(l => l.totalFaculty).Sum());



            StaffDetails += "<tr><td  style='text-align:right;' colspan='6'><b>Total Uploaded Faculty Count</b></td><td style='text-align:center;'>" + TotalCount + "</td><td style='text-align:right;' colspan='2'><b>Biometric-Faculty-Count</b><td style='text-align:center;'>" + TotalCount1 + "</td><td style='text-align:center;'>" + DifferenceCount + "</td></tr>";

            StaffDetails += "</table></br>";
            StaffDetails += "<p style='text-align:left;font-family: Times New Roman; font-size: 8px;'>NA-Not Affiliated</p></br>";



            var New_intakedetails = intakedetailsList.Where(r => r.DegreeID != 0).GroupBy(r => r.DegreeID).ToList();


            StaffDetails += "<br/><p style='text-align:center;'><b><u>Table-II - Total No.of Faculty required as per Affiliated intake</u></b></p><br/>";
            StaffDetails += "<table border='1' cellpadding='3' cellspacing='0' width='100%' style='font-family: Times New Roman; font-size: 8px; font-weight: normal; background-color: #fff; margin: 0 auto;'>";
            StaffDetails += "<tr>";
            StaffDetails += "<td style='text-align:center;'>S.No</td>";
            StaffDetails += "<td style='text-align:center;'>Program</td>";
            StaffDetails += "<td style='text-align:center;'>Total</td>";
            StaffDetails += "</tr>";

            var count_new = 1;
            foreach (var item2 in New_intakedetails)
            {
                var data_latest = 0;
                var Degree_PHD_Count = 0;
                var Degree = string.Empty;
                var Facultyratio = Convert.ToInt32(jntuh_faculty_student_ratio_norms.Where(fn => fn.degreeId == item2.Key).Select(fn => fn.Norms).FirstOrDefault().Split(':')[1].ToString());
                dynamic TotalRequired_facultyCount = 0;
                if (item2.Key == 4)
                {
                    Degree = "B.Tech";
                    TotalRequired_facultyCount = (int)((Btech_Faculty_ProposedCount / Facultyratio) + intakedetailsList.Where(s => s.DegreeID == item2.Key).Select(s => s.requiredFaculty).Sum());
                    TotalRequired_facultyCount = TotalRequired_facultyCount - BTech_PHD_Count_New;
                    Degree_PHD_Count = BTech_PHD_Count_New;

                    data_latest = TotalRequired_facultyCount + BTech_PHD_Count_New;

                }
                else if (item2.Key == 1)
                {
                    Degree = "M.Tech";
                    TotalRequired_facultyCount = (int)(intakedetailsList.Where(s => s.DegreeID == item2.Key && s.shiftId == 1).Select(s => s.requiredFaculty).Sum());

                    TotalRequired_facultyCount = TotalRequired_facultyCount - MTech_PHD_Count_New;
                    Degree_PHD_Count = MTech_PHD_Count_New;

                    data_latest = TotalRequired_facultyCount + MTech_PHD_Count_New;

                }
                else if (item2.Key == 5)
                {
                    var PharmdDegree = intakedetailsList.Where(s => s.academicYearId == 9 && s.Degree == "Pharm.D").Select(s => s.Degree).FirstOrDefault();
                    var PharmdpbDegree = intakedetailsList.Where(s => s.academicYearId == 9 && s.Degree == "Pharm.D PB").Select(s => s.Degree).FirstOrDefault();
                    PharmdDegree = PharmdDegree != null ? "+" + PharmdDegree : "";
                    PharmdpbDegree = PharmdpbDegree != null ? "+" + PharmdpbDegree : "";
                    Degree = "B.Pharmacy" + PharmdDegree + "" + PharmdpbDegree;
                    TotalRequired_facultyCount = (int)(intakedetailsList.Where(s => s.DegreeID == item2.Key).Select(s => s.requiredFaculty).Sum());


                }
                else if (item2.Key == 2)
                {
                    Degree = "M.Pharmacy";
                    TotalRequired_facultyCount = (int)(intakedetailsList.Where(s => s.DegreeID == item2.Key && s.shiftId == 1).Select(s => s.requiredFaculty).Sum());


                }
                else if (item2.Key == 6)
                {
                    Degree = "MBA";
                    TotalRequired_facultyCount = (int)(intakedetailsList.Where(s => s.DegreeID == item2.Key).Select(s => s.requiredFaculty).Sum());
                    TotalRequired_facultyCount = TotalRequired_facultyCount - MBA_PHD_Count_New;
                    Degree_PHD_Count = MBA_PHD_Count_New;
                    data_latest = TotalRequired_facultyCount + MBA_PHD_Count_New;

                }
                else if (item2.Key == 3)
                {
                    Degree = "MCA";
                    TotalRequired_facultyCount = (int)(intakedetailsList.Where(s => s.DegreeID == item2.Key).Select(s => s.requiredFaculty).Sum());
                    TotalRequired_facultyCount = TotalRequired_facultyCount - MCA_PHD_Count_New;
                    Degree_PHD_Count = MCA_PHD_Count_New;
                    data_latest = TotalRequired_facultyCount + MCA_PHD_Count_New;

                }

                else if (item2.Key == 7)
                {
                    Degree = "MAM";
                    TotalRequired_facultyCount = (int)(intakedetailsList.Where(s => s.DegreeID == item2.Key).Select(s => s.requiredFaculty).Sum());
                    TotalRequired_facultyCount = TotalRequired_facultyCount - MAM_PHD_Count_New;
                    Degree_PHD_Count = MAM_PHD_Count_New;
                    data_latest = TotalRequired_facultyCount + MAM_PHD_Count_New;

                }
                else
                {
                    Degree = "MTM";
                    TotalRequired_facultyCount = (int)(intakedetailsList.Where(s => s.DegreeID == item2.Key).Select(s => s.requiredFaculty).Sum());
                }
                if (item2.Key == 9 || item2.Key == 10)
                {
                    //Not Dispaly
                }
                else
                {
                    StaffDetails += "<tr>";
                    StaffDetails += "<td style='text-align:center;'>" + count_new + "</td>";
                    StaffDetails += "<td style='text-align:center;'>" + Degree + "</td>";
                    StaffDetails += "<td style='text-align:center;'>" + Degree_PHD_Count + "</td>";
                    StaffDetails += "<td style='text-align:center;'>" + TotalRequired_facultyCount + "</td>";
                    StaffDetails += "<td style='text-align:center;'>" + data_latest + "</td>";
                    StaffDetails += "</tr>";
                    count_new++;
                    facultyCountnew1 += Degree_PHD_Count;
                    facultyCountnew2 += TotalRequired_facultyCount;
                    facultyCountnew += data_latest;
                    // facultyCountnew += (int)TotalRequired_facultyCount;
                }

            }
            StaffDetails += "<tr><td style='text-align:center;' colspan='2'><b>TotalRequired FacultyCount</b></td><td style='text-align:center;' >" + facultyCountnew1 + "</td><td style='text-align:center;' >" + facultyCountnew2 + "</td><td style='text-align:center;'>" + facultyCountnew + "</td></tr>";
            StaffDetails += "</table>";


            FacultyCountNew obj = new FacultyCountNew();
            obj.UploadedCount = TotalCount;
            obj.BiometricCount = TotalCount1;
            obj.BASDifferenceCount = DifferenceCount;
            obj.RequiredCount = facultyCountnew;
            obj.RequiredCountPercentage = Math.Round(((((double)facultyCountnew - (double)TotalCount1) / (double)facultyCountnew) * 100), 2);
            obj.RequiredPHDCount = facultyCountnew1 == 0 ? 1 : facultyCountnew1;
            obj.PHDPresentCount = PHDPresentCount;
            obj.PHDCountPercentage = Math.Round(((((double)obj.RequiredPHDCount - (double)PHDPresentCount) / (double)obj.RequiredPHDCount) * 100), 2);
            obj.RequiredDifferenceCount = facultyCountnew - TotalCount1;





            //  var ddd = Math.Round(sss,2);
            return obj;

        }

        public ActionResult ExcelFileDownload(string type)
        {
            List<FacultyCountNew> list = new List<FacultyCountNew>();
            type = "September";
            int[] PharmacyCollegeids = { 6, 9, 24, 27, 30, 34, 39, 42, 44, 45, 47, 52, 54, 55, 58, 60, 65, 75, 78, 90, 95, 97, 104, 105, 107, 110, 114, 117, 118, 120, 127, 135, 136, 139, 140, 146, 150, 159, 169, 180, 202, 204, 206, 213, 219, 234, 235, 237, 252, 253, 262, 263, 267, 283, 290, 295, 297, 298, 301, 302, 303, 313, 314, 317, 318, 319, 320, 332, 333, 348, 353, 364, 370, 376, 379, 384, 389, 392, 395, 410, 428, 436, 442, 445, 448, 454 };
            //4, 5, 6, 7, 8, 9, 11, 12, 17, 20, 22, 23, 24, 26, 27, 29, 30, 32, 34, 35, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 48, 50, 52, 54, 55, 56, 58, 60, 65, 67, 68, 69, 70, 72, 74, 75, 77, 78, 79, 80, 81, 84, 85, 86, 87, 88, 90, 91, 95, 97, 100, 103, 104, 105, 107, 108, 109, 110, 111, 113, 114, 116, 117, 118, 119, 120, 121, 122, 123, 124, 125, 127, 128, 129, 130, 132, 134, 135, 136, 137, 138, 139, 140, 141, 143, 144, 145, 146, 147, 148, 150, 152, 153, 155, 156, 157, 158, 159, 161, 162, 163, 164, 165, 166, 168, 169, 171, 172, 173, 174, 175, 176, 177, 178, 179, 180, 182, 183, 184, 185, 186, 187, 188, 189, 192, 193, 194, 195, 196, 197, 198, 201, 202, 203, 204, 206, 207, 210, 211, 213, 214, 218, 219, 222, 223, 225, 227, 228, 229, 230, 234, 235, 237, 238, 241, 242, 243, 244, 245, 249, 250, 252, 253, 254, 256, 260, 261, 262, 263, 264, 266, 267, 269, 271, 273, 276, 279, 282, 283, 286, 287, 290, 291, 292, 293, 295, 296, 297, 298, 299, 300, 301, 302, 303, 304, 306, 307, 308, 309, 310, 313, 314, 316, 317, 318, 319, 320, 321, 324, 325, 326, 327, 329, 330, 332, 334, 335, 336, 342, 343, 348, 349, 350, 352, 353, 355, 360, 364, 365, 366, 367, 368, 369, 370, 371, 373, 374, 376, 379, 380, 382, 384, 385, 386, 389, 391, 392, 393, 394, 395, 399, 400, 410, 411, 414, 416, 420, 423, 428, 429, 430, 435, 436, 439, 441, 442, 443, 445, 448, 452, 454, 455

            //4,5,7,8,11,12,17,20,22,23,26,29,32,35,38,40,41,43,46,48,50,56,67,68,69,70,72,74,77,79,80,81,84,85,86,87,88,91,100,103,108,109,111,113,116,119,121,122,123,124,125,128,129,130,132,134,137,138,141,143,144,145,147,148,152,153,155,156,157,158,161,162,163,164,165,166,168,171,172,173,174,175,176,177,178,179,182,183,184,185,186,187,188,189,192,193,194,195,196,197,198,201,203,207,210,211,214,218,222,223,225,227,228,229,230,238,241,242,243,244,245,249,250,254,256,260,261,264,266,269,271,273,276,279,282,286,287,291,292,293,296,299,300,304,306,307,308,309,310,316,321,324,325,326,327,329,330,333,334,335,336,342,343,349,350,352,355,360,365,366,367,368,369,371,373,374,380,382,385,386,391,393,394,399,400,411,414,416,420,423,429,430,435,439,441,443,452,455
            //4,5,7,8,11,12,17,20,22,23,26,29,32,35,38,40,41,43,46,48,50,56,67,68,69,70,72,74,77,79,80,81,84,85,86,87,88,91,100,103,108,109,111,113,116,119,121,122,123,124,125,128,129,130,132,134,137,138,141,143,144,145,147,148,152,153,155,156,157,158,161,162,163,164,165,166,168,171,172,173,174,175,176,177,178,179,182,183,184,185,186,187,188,189,192,193,194,195,196,197,198,201,203,207,210,211,214,218,222,223,225,227,228,229,230,238,241,242,243,244,245,249,250,254,256,260,261,264,266,269,271,273,276,279,282,286,287,291,292,293,296,299,300,304,306,307,308,309,310,316,321,324,325,326,327,329,330,333,334,335,336,342,343,349,350,352,355,360,365,366,367,368,369,371,373,374,380,382,385,386,391,393,394,399,400,411,414,416,420,423,429,430,435,439,441,443,452,455
            int[] CollegeIDSList = { 4, 7 };
            var jntuh_college = db.jntuh_college.ToList();
            var jntuh_college_faculty_registered = db.jntuh_college_faculty_registered.ToList();
            foreach (var item in CollegeIDSList)
            {
                FacultyCountNew obj = new FacultyCountNew();
                obj = SurpriseStaffCount(item, type);
                obj.PortalFaculty = jntuh_college_faculty_registered.Where(s => s.collegeId == item).Select(s => s.RegistrationNumber).Count();
                obj.COllegecode = jntuh_college.Where(s => s.id == item).Select(s => s.collegeCode).SingleOrDefault();
                obj.CollegeName = jntuh_college.Where(s => s.id == item).Select(s => s.collegeName).SingleOrDefault();
                list.Add(obj);
            }



            Response.ClearContent();
            Response.Buffer = true;
            Response.AddHeader("content-disposition", "attachment; filename=Faculty.xls");
            Response.ContentType = "application/vnd.ms-excel";
            return PartialView("_ExcelDownloadNew", list.ToList());

        }
        #endregion
        public string SurpriseStaffNew(int? collegeID, string type)
        {
            // string[] months = { "August", "September", "October", "November" };
            string[] months = { type };
            var year = DateTime.Now.Year.ToString();
            string StaffDetails = string.Empty;
            string MonthName = string.Empty;
            int count = 1;
            monthCount = 15;
            int facultystudentRatio = 0;
            var jntuh_faculty_student_ratio_norms = db.jntuh_faculty_student_ratio_norms.ToList();

            string[] strOtherDepartments = { "English", "Mathematics", "Physics", "Chemistry", "Others(CSE/IT)", "Others(CIVIL/MECH)", "Others(ECE/EEE)", "Others(MNGT/H&S)" };

            List<CollegeFacultyWithIntakeReport> intakedetailsList = Get_PHD_Faculty(collegeID);

            List<string> New_PHD_Count1 = new List<string>();

            PHD_Count = intakedetailsList.Where(i => i.shiftId == 1 && !strOtherDepartments.Contains(i.Department)).Select(i => i.phdFaculty).Sum();

            foreach (var item in intakedetailsList.Where(i => i.shiftId == 1).ToList())
            {
                foreach (var item1 in item.PhdRegNos)
                {
                    New_PHD_Count1.Add(item1);
                }

            }

            int totalBtechFirstYearIntake = intakedetailsList.Where(d => d.Degree == "B.Tech" && !strOtherDepartments.Contains(d.Department)).Select(d => d.approvedIntake1).Sum();

            string[] ProposedIntakeZeroDeptName = intakedetailsList.Where(e => e.Degree == "B.Tech" && (e.approvedIntake1 == 0) && !strOtherDepartments.Contains(e.Department)).Select(e => e.Specialization).Distinct().ToArray();

            List<jntuh_college_attendace> jntuh_college_attendace = db.jntuh_college_attendace.Where(a => a.Collegeid == collegeID).ToList();

            List<jntuh_college_attendace> LoginList = new List<jntuh_college_attendace>();
            List<FacultyCountNew> facultylist = new List<FacultyCountNew>();

            var jntuh_academic_year = db.jntuh_academic_year.ToList();
            int actualYear = jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(s => s.actualYear).FirstOrDefault();

            int AY1 = jntuh_academic_year.Where(s => s.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();

            foreach (var item5 in months)
            {

                facultyCountnew1 = 0;
                TotalCount1 = 0;
                DifferenceCount = 0;
                facultyCountnew = 0;
                Btech_Faculty_ProposedCount = 0;
                BTech_PHD_Count_New = 0;
                MTech_PHD_Count_New = 0;
                BPharmacy_PHD_Count_New = 0;
                MPharmacy_PHD_Count_New = 0;
                PharmD_PHD_Count_New = 0;
                PharmDPB_PHD_Count_New = 0;
                MCA_PHD_Count_New = 0;
                MBA_PHD_Count_New = 0;
                MAM_PHD_Count_New = 0;
                FacultyCountNew obj = new FacultyCountNew();

                if (item5 == "August")
                {
                    var August_attendance = Get_August(collegeID, item5);
                    LoginList = August_attendance.ToList();
                    August_attendance.Clear();
                }
                else
                {
                    LoginList = jntuh_college_attendace.Where(a => a.AcademicYearId == AY1 && a.Month == item5).ToList();
                }


                //  LoginList = db.jntuh_college_attendace.Where(a => a.Collegeid == collegeID && a.Month == item5).ToList();

                string[] RegNos = new string[] { "9211-150409-180107", "6317-150413-162713", "96150406-153506", "78150407-104506", "90150406-132723", "5264-150409-160644", "8518-150413-150717", "7995-150414-143317", "4687-150420-162710", "1549-150418-123933", "8891-150420-165037", "2375-150427-184049", "5612-170202-102750", "9234-160302-113847", "49150406-131709", "4261-160308-074435", "4171-150624-143950", "0707-150408-175902", "74150406-141818", "16150402-155455", "54150402-122257", "6348-150411-120653", "6058-150417-174615", "8105-150412-154535", "8558-160312-194806", "1116-160313-055503", "3205-160313-105704", "7748-160313-063954", "2328-150408-154626", "8922-150410-144623", "6400-150409-153507", "4241-150413-144757", "5387-150420-124115", "6442-150413-163535", "3141-160302-143355", "2624-170202-123226", "2778-150621-184256", "6002-150408-104417", "7601-150409-104220", "5724-150409-131120", "9549-150409-131219", "5480-150416-141938", "8460-150505-150705", "5325-150411-101220", "9134-150418-123351", "1235-170107-122241", "3995-150420-172501", "5157-151216-105135", "5993-170208-144156", "0827-160314-154038", "43150407-121703", "9675-170201-060349", "1761-170110-124328", "8780-170212-221004", "42150406-105659", "9741-150408-163934", "2195-150408-145520", "7984-160128-143658", "0756-160128-144719", "8629-160128-141146", "6328-160129-102154", "6339-160128-152920", "5003-150410-100751", "3111-150413-163611", "6889-150413-145248", "7968-170117-141626", "4390-150619-131209", "7037-150619-123808", "6090-150416-211652", "0650-150419-232914", "9644-150420-123408", "2204-150506-223734", "5007-160303-052952", "1221-160111-140608", "5323-160316-172042", "5758-150409-145824", "9690-150419-143601", "2845-160318-215132", "6585-150414-151718", "6518-160222-100511", "37150407-125255", "4939-150419-180848", "7509-150415-191610", "7038-150418-123607", "4375-150420-144936", "4925-160204-183924", "5271-160206-142514", "9379-150417-164707", "4561-160211-082340", "4517-150424-122333", "4503-150419-134140", "8391-150419-133206", "7549-150409-171452", "2289-150418-215024", "5999-150418-172740", "7521-150418-223714", "4740-150419-162142", "9374-150419-225506", "0752-150422-165906", "6713-150422-171112", "7419-150420-154436", "0059-150420-164303", "7867-160309-173829", "0152-160309-181208", "50150406-110631", "4769-150416-174812", "8005-150416-145941", "2516-150422-195357", "8508-150412-113146", "1003-170119-133836", "4904-170119-140309", "0588-170126-140848", "8291-150624-160237", "6023-151229-151344", "4534-150408-104328", "9794-160229-175017", "3909-151230-121957", "68150407-154601", "1147-150412-155421", "6851-170208-173310", "8532-170213-171543", "77150407-144017", "4281-150415-144925", "8238-161024-112302", "1949-150421-144423", "3666-150408-102957", "1854-150414-123210", "8628-150414-121943", "2834-150414-131904", "8474-150420-113501", "5314-150422-160253", "6998-150422-145056", "8739-150422-124734", "5241-150422-143019", "1831-150422-122747", "7170-150422-115651", "4386-150422-113334", "1097-150422-111446", "2563-150409-185844", "6483-150409-120156", "8889-150423-121239", "3174-150423-145633", "7141-150416-152448", "3831-150426-164813", "1364-150426-165916", "8420-150426-163755", "0778-150501-170330", "2771-150622-225307", "8776-150622-232358", "7503-150424-104100", "9155-150622-231402", "7447-150525-115526", "9731-150622-154029", "3217-150409-132413", "2351-160302-144825", "8336-160302-171055", "93150406-161145", "67150407-173554", "7618-150410-162949", "9986-150619-175911", "94150404-124125", "00150407-113102", "2180-150424-222555", "1126-150507-163545", "1808-150621-133359", "4266-150622-125140", "7977-150623-115417", "7690-150415-134357", "6734-150415-133015", "4696-150409-112735", "9742-150409-105139", "5207-150409-145858", "88150407-162910", "7306-150411-145154", "2980-150411-105638", "9381-150411-113008", "7210-150428-042100", "2316-150428-040056", "5018-170111-130147", "2421-170118-102956", "0676-150408-111501", "2469-160121-104311", "94150402-171218", "9492-160121-160214", "2681-150625-150854", "6076-160311-171551", "0661-160312-141716", "2280-150408-151134", "0208-150411-124221", "8076-150409-161147", "5204-150412-144814", "4443-150412-150351", "9010-150412-151234", "3264-150418-173255", "0922-150415-111243", "0852-150429-154729", "4149-150621-173825", "6620-150625-180248", "7810-161226-114059", "4815-160201-103423", "6809-150505-133845", "0067-150505-141355", "5661-150505-153407", "5246-150505-151341", "7132-150505-171425", "9033-150430-105308", "3057-150506-113420", "8389-150506-170835", "2511-150506-173242", "4009-150408-154747", "8440-150624-131011", "9393-160306-105928", "4283-160315-194106", "3404-170119-172558", "7762-170213-131706", "1475-150414-183509", "7134-160218-150219", "0414-150427-153301", "2067-150414-155501", "33150330-155256", "87150406-194332", "8362-150409-110622", "6180-150408-151453", "7098-150426-074226", "7386-150414-125915", "1466-170105-194422", "1997-170112-095532", "5841-170125-113934", "5612-170125-124150", "1782-150408-121508", "1781-150409-114620", "47150407-153633", "6498-150414-132024", "9077-150416-124631", "5914-150504-100508", "0940-150504-125126", "7835-150506-141716", "6244-150622-125503", "8145-150623-151823", "4830-150623-163945", "6188-150413-160703", "3295-151219-103214", "8352-150625-115421", "7697-150625-114147", "7437-150412-112238", "6903-150506-125905", "8943-150507-132216", "3855-150507-145302", "5572-150507-124125", "3430-150507-155250", "3059-150516-165055", "0336-150507-122950", "1400-150507-152048", "4765-170522-104257", "09150402-160427", "6421-151221-144330", "7885-160312-140130", "0052-160312-131632", "0127-160312-141609", "3725-160312-151706", "9069-160312-151048", "7922-160312-160244", "0682-160312-172016", "2685-160312-172344", "7093-160312-154220", "3195-160314-232954", "5912-150408-122341", "84150406-142457", "7913-150417-131457", "6808-150424-004516", "7368-150419-164942", "3038-150413-112451", "58150406-100658", "68150404-140248", "10150404-134548", "61150404-162737", "56150407-160735", "8021-150409-165612", "8901-150430-111830", "1584-150409-112324", "9049-150416-103205", "95150404-130909", "1083-150417-152920", "1809-150428-104045", "7477-150428-103245", "9865-150426-124940", "6104-150427-134207", "6315-160225-130444", "8441-150413-113304", "9811-150409-103922", "8964-150420-121821", "4723-160311-151729", "03150402-152426", "9373-150426-135654", "8422-150410-113036", "9552-150410-111820", "4713-150420-124126", "6087-170212-130603", "9966-170211-170427", "0319-150415-110441", "9914-150414-143319", "8688-150413-214801", "4046-150507-120439", "5688-160311-160356", "37150406-124138", "86150406-152725", "6080-160305-164900", "7098-160309-162246", "3941-160309-164350", "0502-170213-143616", "6329-170131-185402", "1338-150415-172930", "0443-150620-112146", "39150405-125954", "4439-150422-115107", "6970-150415-123739", "5363-150417-125713", "6048-160225-114522", "8477-160301-195215", "7934-160301-193415", "7595-150423-120919", "5464-150427-154315", "1054-160102-160709", "15150407-161016", "2585-150410-170055", "8344-150429-162201", "8833-150504-131131", "0184-150429-124639", "0767-150430-105420", "0799-150430-113108", "1784-150430-101515", "2900-150430-114213", "3100-150501-103154", "3253-150430-111321", "3477-150430-113036", "4437-150430-104224", "4756-150411-150337", "4861-150430-112525", "5200-150501-103751", "5107-150430-104415", "5360-150430-090404", "6453-150507-190621", "7074-150507-151336", "1705-170116-142649", "91150406-161756", "9383-160128-125153", "0376-150421-155232", "2043-160310-154642", "23150403-182551", "0220-150422-163321", "3271-150506-120301", "0831-160218-160442", "3138-160219-144523", "3570-160219-145952", "5730-160219-155537", "3603-160220-122613", "4448-160220-193243", "5148-160220-114832", "1924-160220-113523", "6796-160220-221344", "5301-160304-192359", "8226-160305-162320", "9581-160305-115025", "8823-160305-140040", "1429-150427-153420", "5607-150425-102421", "47150406-125841", "7217-160209-132527", "2719-150417-140130", "8501-150421-095920", "45150407-154450", "1067-150419-120449", "5555-160129-181224", "0855-160314-234426", "8195-160314-180632", "2658-160314-185351", "8213-160314-204848", "2521-160314-200255", "9616-160314-214031", "0244-160314-231118", "3845-160314-214240", "1475-160315-001332", "3897-160314-200045", "7710-160319-204556", "6424-160319-215612", "7341-160319-202433", "6897-160319-221313", "8988-160319-224714", "5086-150410-102356", "7920-150413-130841", "2861-150410-135151", "6794-160319-164923", "2163-160319-175204", "8260-160319-182908", "6607-160319-151901", "2549-160319-143150", "5086-160319-123814", "2359-160319-121236", "8319-160319-114531", "52150401-155113", "6429-150410-134158", "8471-150412-173932", "2546-150412-173119", "5430-150413-163549", "4595-150422-114349", "7960-150502-150257", "1556-150502-151010", "1620-150502-152356", "5563-150410-155500", "8462-150422-133254", "4469-150422-140349", "39150404-122803", "89150406-135317", "8733-160220-105912", "3280-160311-151200", "68150404-143104", "1620-150419-083055", "98150402-140537", "4240-150619-124624", "90150402-150047", "5077-150411-114936", "1735-160106-153643", "4305-160313-173609", "4707-160313-172938", "7992-160313-172334", "4292-160313-174602", "2969-160313-170725", "7697-160314-151405", "5247-160314-201702", "2991-160314-202954", "3831-170207-161803", "52150407-113402", "1185-150410-154249", "4181-150415-152826", "63150406-133241", "7082-160225-144210", "6833-150429-151901", "7084-150603-172353", "8010-150421-124203", "5725-150505-114121", "1394-150430-232728", "60150406-153912", "9700-150418-103531", "5407-150420-114851", "0261-150421-122215", "4944-150507-132508", "0878-160225-114919", "4393-150421-155815", "0967-160310-130928", "2599-150421-154955", "6087-160224-152646", "0546-160224-111407", "7909-160312-182449", "0487-160312-190833", "9358-170111-094327", "6379-170112-093250", "9888-150421-113110", "1166-150622-055114", "2952-150624-163859", "7893-150421-160431", "87150404-160809", "2876-150409-150305", "8578-150418-145228", "4050-150424-155306", "70150404-125108", "9618-150416-122424", "8941-150415-152633", "0959-170118-101333", "0774-150427-133052", "0785-160304-122835", "9791-150422-162245", "22150407-163124", "53150406-105254", "1528-160304-125739", "8238-161224-112812", "3531-150421-143805", "4278-150416-175043", "7787-150411-145051", "56150403-213949", "6604-150416-125428", "4857-150421-080322", "0023-160316-130354", "1472-160312-141517", "7735-150421-182347", "50150403-105221", "2813-150408-164256", "3538-150411-104526", "0560-150428-125331", "2763-160203-170600", "0170-160307-001639", "9304-160306-225725", "0171-160306-235900", "2124-160306-232603", "3439-160316-105705", "7980-160316-105741", "5725-150409-102427", "4711-150426-181413", "4892-150413-110154", "0895-150413-112139", "0093-150413-132750", "4698-160201-104955", "8540-150413-110353", "6074-150413-170744", "4574-150620-175558", "5645-150620-182613", "1693-150412-112919", "0158-150427-152718", "2008-150421-095104", "4272-160108-143318", "7884-150413-153150", "4800-150415-103452", "1136-160311-110044", "9656-170111-081909", "33150407-101427", "1266-150427-190250", "2093-170207-122958", "3507-150409-123348", "7541-150412-152809", "2825-150420-092811", "9437-150420-103610", "4992-150409-141335", "3498-150408-231802", "6166-150512-104107", "9768-150427-210434", "0205-150508-120436", "2637-150425-151316", "2513-150425-192936", "4188-150424-144352", "2456-150427-125655", "1660-150507-152429", "3570-150507-110913", "1601-150408-172051", "0148-150419-091522", "1442-161217-164741", "31150403-201030", "0941-170207-002441", "8575-170131-031553", "9965-170131-032636", "5691-170131-033654", "8645-170131-034806", "7939-170131-035846", "3351-170131-041646", "9837-170131-042413", "2633-170131-043329", "2964-170131-045303", "2440-170131-201700", "4894-170201-001106", "7664-160522-105414", "6997-170207-182304", "7197-170207-173901", "7397-160524-140443", "9865-160525-110126", "4556-160522-113614", "4469-150409-124255" };

                LoginList = LoginList.Where(l => !RegNos.Contains(l.RegistrationNo)).ToList();





                var PHD = LoginList.Where(l => New_PHD_Count1.Contains(l.RegistrationNo)).ToList();



                if (item5 == "August" || item5 == "October")
                {
                    PHDPresentCount = PHD.Where(s => s.TotalPercentage >= monthCount).Count();

                }
                else
                {
                    PHDPresentCount = PHD.Where(s => s.TotalPercentage >= monthCount).Count();

                }



                foreach (var item in intakedetailsList)
                {
                    //var IntakeCount = 0;
                    var phdpercentagecount = 0;
                    var nonphdpercentagecount = 0;
                    if (!strOtherDepartments.Contains(item.Department))
                    {

                        if (item.Degree == "B.Tech")
                        {
                            Btech_Faculty_ProposedCount += item.approvedIntake1;
                            BTech_PHD_Count_New += item.SpecializationsphdFaculty;

                        }
                        else if (item.Degree == "M.Tech")
                        {
                            Mtech_Faculty_ProposedCount += item.approvedIntake1;

                            MTech_PHD_Count_New += item.SpecializationsphdFaculty;
                        }
                        else if (item.Degree == "B.Pharmacy")
                        {
                            BPharmacy_Faculty_ProposedCount += item.approvedIntake1;
                          //  BPharmacy_PHD_Count_New += item.SpecializationsphdFaculty;
                        }
                        else if (item.Degree == "M.Pharmacy")
                        {
                            Mpharmacy_Faculty_ProposedCount += item.approvedIntake1;
                            MPharmacy_PHD_Count_New += item.SpecializationsphdFaculty;
                        }
                        else if (item.Degree == "MBA")
                        {
                            MBA_Faculty_ProposedCount += item.approvedIntake1;
                            MBA_PHD_Count_New += item.SpecializationsphdFaculty;
                        }
                        else if (item.Degree == "MCA")
                        {
                            MCA_Faculty_ProposedCount += item.approvedIntake1;
                            MCA_PHD_Count_New += item.SpecializationsphdFaculty;
                        }
                        else if (item.Degree == "Pharm.D")
                        {
                            Pharm_D_Faculty_ProposedCount += item.approvedIntake1;
                           // PharmD_PHD_Count_New += item.SpecializationsphdFaculty;
                        }
                        else if (item.Degree == "Pharm.D PB")
                        {
                            Pharm_DPB_Faculty_ProposedCount += item.approvedIntake1;
                           // PharmDPB_PHD_Count_New += item.SpecializationsphdFaculty;
                        }
                        else if (item.Degree == "MAM")
                        {
                            MAM_Faculty_ProposedCount += item.approvedIntake1;
                            MAM_PHD_Count_New += item.SpecializationsphdFaculty;
                        }
                        else
                        {
                            MTM_Faculty_ProposedCount += item.approvedIntake1;
                        }

                        var LoginList_New = LoginList.Where(s => s.EmpDeptID == item.DepartmentID).ToList();

                        if (item.PhdRegNos.Length != 0)
                        {
                            var PhdFacultyList = LoginList_New.Where(r => item.PhdRegNos.Contains(r.RegistrationNo)).ToList();

                            phdpercentagecount = PhdFacultyList.Where(s => s.TotalPercentage >= monthCount).Count();


                            if (item.NonPhdRegNos.Length != 0)
                            {
                                var FacultyList = LoginList_New.Where(r => item.NonPhdRegNos.Contains(r.RegistrationNo)).ToList();
                                nonphdpercentagecount = FacultyList.Where(s => s.TotalPercentage >= monthCount).Count();

                            }
                            else
                            {
                                nonphdpercentagecount = 0;
                            }

                            //if (item5 == "August" || item5 == "October")
                            //{
                              

                            //}
                            //else
                            //{
                            //    phdpercentagecount = PhdFacultyList.Where(s => s.TotalPercentage >= monthCount).Count();


                            //    if (item.NonPhdRegNos.Length != 0)
                            //    {
                            //        var FacultyList = LoginList_New.Where(r => item.NonPhdRegNos.Contains(r.RegistrationNo)).ToList();
                            //        nonphdpercentagecount = FacultyList.Where(s => s.TotalPercentage >= monthCount).Count();

                            //    }
                            //    else
                            //    {
                            //        nonphdpercentagecount = 0;
                            //    }

                            //}



                        }
                        else
                        {
                            if (item.NonPhdRegNos.Length != 0)
                            {
                                var FacultyList = LoginList_New.Where(f => item.NonPhdRegNos.Contains(f.RegistrationNo)).ToList();
                                nonphdpercentagecount = FacultyList.Where(s => s.TotalPercentage >= monthCount).Count();

                            }
                            else
                            {
                                nonphdpercentagecount = 0;
                            }

                            //if (item5 == "August" || item5 == "October")
                            //{
                            //    if (item.NonPhdRegNos.Length != 0)
                            //    {
                            //        var FacultyList = LoginList_New.Where(f => item.NonPhdRegNos.Contains(f.RegistrationNo)).ToList();
                            //        nonphdpercentagecount = FacultyList.Where(s => s.TotalPercentage >= monthCount).Count();

                            //    }
                            //    else
                            //    {
                            //        nonphdpercentagecount = 0;
                            //    }

                            //}
                            //else
                            //{
                            //    if (item.NonPhdRegNos.Length != 0)
                            //    {
                            //        var FacultyList = LoginList_New.Where(f => item.NonPhdRegNos.Contains(f.RegistrationNo)).ToList();
                            //        nonphdpercentagecount = FacultyList.Where(s => s.TotalPercentage >= monthCount).Count();

                            //    }
                            //    else
                            //    {
                            //        nonphdpercentagecount = 0;
                            //    }
                            //}
                        }

                        count++;
                        TotalCount += item.totalFaculty;
                        if (item.shiftId == 1)
                            TotalCount1 += (phdpercentagecount + nonphdpercentagecount);
                        else { }
                        if (item.shiftId == 1)
                            DifferenceCount += ((item.ugFaculty + item.pgFaculty + item.phdFaculty) - (phdpercentagecount + nonphdpercentagecount));
                        else { }


                    }
                    else
                    {
                        var LoginList_New = LoginList.Where(s => s.EmpDeptID == item.DepartmentID).ToList();

                        if (item.PhdRegNos.Length != 0)
                        {
                            var PhdFacultyList = LoginList_New.Where(r => item.PhdRegNos.Contains(r.RegistrationNo)).ToList();

                            phdpercentagecount = PhdFacultyList.Where(s => s.TotalPercentage >= monthCount).Count();


                            if (item.NonPhdRegNos.Length != 0)
                            {
                                var FacultyList = LoginList_New.Where(r => item.NonPhdRegNos.Contains(r.RegistrationNo)).ToList();
                                nonphdpercentagecount = FacultyList.Where(s => s.TotalPercentage >= monthCount).Count();

                            }
                            else
                            {
                                nonphdpercentagecount = 0;
                            }

                            //if (item5 == "August" || item5 == "October")
                            //{
                            //    phdpercentagecount = PhdFacultyList.Where(s => s.TotalPercentage >= monthCount).Count();


                            //    if (item.NonPhdRegNos.Length != 0)
                            //    {
                            //        var FacultyList = LoginList_New.Where(r => item.NonPhdRegNos.Contains(r.RegistrationNo)).ToList();
                            //        nonphdpercentagecount = FacultyList.Where(s => s.TotalPercentage >= monthCount).Count();

                            //    }
                            //    else
                            //    {
                            //        nonphdpercentagecount = 0;
                            //    }

                            //}
                            //else
                            //{
                            //    phdpercentagecount = PhdFacultyList.Where(s => s.TotalPercentage >= monthCount).Count();


                            //    if (item.NonPhdRegNos.Length != 0)
                            //    {
                            //        var FacultyList = LoginList_New.Where(r => item.NonPhdRegNos.Contains(r.RegistrationNo)).ToList();
                            //        nonphdpercentagecount = FacultyList.Where(s => s.TotalPercentage >= monthCount).Count();

                            //    }
                            //    else
                            //    {
                            //        nonphdpercentagecount = 0;
                            //    }

                            //}
                        }
                        else
                        {
                            if (item.NonPhdRegNos.Length != 0)
                            {
                                var FacultyList = LoginList_New.Where(r => item.NonPhdRegNos.Contains(r.RegistrationNo)).ToList();
                                nonphdpercentagecount = FacultyList.Where(s => s.TotalPercentage >= monthCount).Count();

                            }
                            else
                            {
                                nonphdpercentagecount = 0;
                            }


                            //if (item5 == "August" || item5 == "October")
                            //{
                            //    if (item.NonPhdRegNos.Length != 0)
                            //    {
                            //        var FacultyList = LoginList_New.Where(r => item.NonPhdRegNos.Contains(r.RegistrationNo)).ToList();
                            //        nonphdpercentagecount = FacultyList.Where(s => s.TotalPercentage >= monthCount).Count();

                            //    }
                            //    else
                            //    {
                            //        nonphdpercentagecount = 0;
                            //    }

                            //}
                            //else
                            //{
                            //    if (item.NonPhdRegNos.Length != 0)
                            //    {
                            //        var FacultyList = LoginList_New.Where(r => item.NonPhdRegNos.Contains(r.RegistrationNo)).ToList();
                            //        nonphdpercentagecount = FacultyList.Where(s => s.TotalPercentage >= monthCount).Count();

                            //    }
                            //    else
                            //    {
                            //        nonphdpercentagecount = 0;
                            //    }
                            //}
                        }

                        count++;
                        TotalCount += item.totalFaculty;
                        TotalCount1 += (phdpercentagecount + nonphdpercentagecount);
                        DifferenceCount += ((item.ugFaculty + item.pgFaculty + item.phdFaculty) - (phdpercentagecount + nonphdpercentagecount));
                    }


                }
                TotalCount = (TotalCount - intakedetailsList.Where(l => l.shiftId == 2).Select(l => l.totalFaculty).Sum());

                var New_intakedetails = intakedetailsList.Where(r => r.DegreeID != 0).GroupBy(r => r.DegreeID).ToList();

                var count_new = 1;
                foreach (var item2 in New_intakedetails)
                {
                    var data_latest = 0;
                    var Degree_PHD_Count = 0;
                    var Degree = string.Empty;
                    var Facultyratio = Convert.ToInt32(jntuh_faculty_student_ratio_norms.Where(fn => fn.degreeId == item2.Key).Select(fn => fn.Norms).FirstOrDefault().Split(':')[1].ToString());
                    dynamic TotalRequired_facultyCount = 0;
                    if (item2.Key == 4)
                    {
                        Degree = "B.Tech";
                        TotalRequired_facultyCount = (int)((Btech_Faculty_ProposedCount / Facultyratio) + intakedetailsList.Where(s => s.DegreeID == item2.Key).Select(s => s.requiredFaculty).Sum());
                        TotalRequired_facultyCount = TotalRequired_facultyCount - BTech_PHD_Count_New;
                        Degree_PHD_Count = BTech_PHD_Count_New;

                        data_latest = TotalRequired_facultyCount + BTech_PHD_Count_New;

                    }
                    else if (item2.Key == 1)
                    {
                        Degree = "M.Tech";
                        TotalRequired_facultyCount = (int)(intakedetailsList.Where(s => s.DegreeID == item2.Key && s.shiftId == 1).Select(s => s.requiredFaculty).Sum());

                        TotalRequired_facultyCount = TotalRequired_facultyCount - MTech_PHD_Count_New;
                        Degree_PHD_Count = MTech_PHD_Count_New;

                        data_latest = TotalRequired_facultyCount + MTech_PHD_Count_New;

                    }
                    else if (item2.Key == 5)
                    {
                        //var PharmdDegree = intakedetailsList.Where(s => s.academicYearId == 9 && s.Degree == "Pharm.D").Select(s => s.Degree).FirstOrDefault();
                        //var PharmdpbDegree = intakedetailsList.Where(s => s.academicYearId == 9 && s.Degree == "Pharm.D PB").Select(s => s.Degree).FirstOrDefault();
                        //PharmdDegree = PharmdDegree != null ? "+" + PharmdDegree : "";
                        //PharmdpbDegree = PharmdpbDegree != null ? "+" + PharmdpbDegree : "";
                        //Degree = "B.Pharmacy" + PharmdDegree + "" + PharmdpbDegree;
                        Degree = "B.Pharmacy";
                        TotalRequired_facultyCount = (int)(intakedetailsList.Where(s => s.DegreeID == item2.Key).Select(s => s.requiredFaculty).Sum());
                       // TotalRequired_facultyCount = TotalRequired_facultyCount - BPharmacy_PHD_Count_New;
                        Degree_PHD_Count = BPharmacy_PHD_Count_New;

                        data_latest = TotalRequired_facultyCount;

                    }
                    else if (item2.Key == 2)
                    {
                        Degree = "M.Pharmacy";
                        TotalRequired_facultyCount = (int)(intakedetailsList.Where(s => s.DegreeID == item2.Key && s.shiftId == 1).Select(s => s.requiredFaculty).Sum());
                        TotalRequired_facultyCount = TotalRequired_facultyCount - MPharmacy_PHD_Count_New;
                        Degree_PHD_Count = MPharmacy_PHD_Count_New;
                        data_latest = TotalRequired_facultyCount + MPharmacy_PHD_Count_New;
                    }
                    else if (item2.Key == 6)
                    {
                        Degree = "MBA";
                        TotalRequired_facultyCount = (int)(intakedetailsList.Where(s => s.DegreeID == item2.Key).Select(s => s.requiredFaculty).Sum());
                        TotalRequired_facultyCount = TotalRequired_facultyCount - MBA_PHD_Count_New;
                        Degree_PHD_Count = MBA_PHD_Count_New;
                        data_latest = TotalRequired_facultyCount + MBA_PHD_Count_New;

                    }
                    else if (item2.Key == 3)
                    {
                        Degree = "MCA";
                        TotalRequired_facultyCount = (int)(intakedetailsList.Where(s => s.DegreeID == item2.Key).Select(s => s.requiredFaculty).Sum());
                        TotalRequired_facultyCount = TotalRequired_facultyCount - MCA_PHD_Count_New;
                        Degree_PHD_Count = MCA_PHD_Count_New;
                        data_latest = TotalRequired_facultyCount + MCA_PHD_Count_New;

                    }

                    else if (item2.Key == 7)
                    {
                        Degree = "MAM";
                        TotalRequired_facultyCount = (int)(intakedetailsList.Where(s => s.DegreeID == item2.Key).Select(s => s.requiredFaculty).Sum());
                        TotalRequired_facultyCount = TotalRequired_facultyCount - MAM_PHD_Count_New;
                        Degree_PHD_Count = MAM_PHD_Count_New;
                        data_latest = TotalRequired_facultyCount + MAM_PHD_Count_New;

                    }
                    else
                    {
                        Degree = "MTM";
                        TotalRequired_facultyCount = (int)(intakedetailsList.Where(s => s.DegreeID == item2.Key).Select(s => s.requiredFaculty).Sum());
                    }
                    if (item2.Key == 9 || item2.Key == 10)
                    {
                        //Not Dispaly
                    }
                    else
                    {

                        count_new++;
                        facultyCountnew1 += Degree_PHD_Count;
                        facultyCountnew2 += TotalRequired_facultyCount;
                        facultyCountnew += data_latest;
                        // facultyCountnew += (int)TotalRequired_facultyCount;
                    }

                }
                obj.Monthname = item5;
                obj.UploadedCount = TotalCount;
                obj.BiometricCount = TotalCount1;
                obj.BASDifferenceCount = DifferenceCount;
                obj.RequiredCount = facultyCountnew;
                obj.RequiredCountPercentage = Math.Round(((((double)facultyCountnew - (double)TotalCount1) / (double)facultyCountnew) * 100), 2);
                obj.RequiredPHDCount = facultyCountnew1 == 0 ? 1 : facultyCountnew1;
                obj.PHDPresentCount = PHDPresentCount;
                obj.PHDCountPercentage = Math.Round(((((double)obj.RequiredPHDCount - (double)PHDPresentCount) / (double)obj.RequiredPHDCount) * 100), 2);
                obj.RequiredDifferenceCount = facultyCountnew - TotalCount1;

                facultylist.Add(obj);
            }

            StaffDetails += "<table border='1' cellpadding='3' cellspacing='0' style='font-family: Times New Roman; font-size: 8px; font-weight: normal; background-color: #fff; margin: 0 auto;'>";
            StaffDetails += "<tr>";
            StaffDetails += "<td width:'2' style='text-align:center;'>S.No</td>";
            StaffDetails += "<td width:'2' style='text-align:center;'>Month</td>";
            StaffDetails += "<td width:'3' style='text-align:center;'>Number of faculty giving Biometric attendance regularly(for atleast 15 days in a month)</td>";
            StaffDetails += "<td width:'3' style='text-align:center;'>% of faculty not giving Biometric attendance regularly w.r.t required faculty</td>";

            StaffDetails += "</tr>";

            int sno = 1;
            double minPercentage = (double)0;
            var per = facultylist.Where(f => f.RequiredCountPercentage >= minPercentage).Count();
          
            foreach (var item in facultylist)
            {

                StaffDetails += "<tr>";
                StaffDetails += "<td style='text-align:center;'>" + sno + "</td>";
                StaffDetails += "<td style='text-align:center;'>" + item.Monthname + ",2018</td>";
                StaffDetails += "<td style='text-align:center;'>" + item.BiometricCount + "</td>";
                if (item.RequiredCountPercentage >= minPercentage)
                    StaffDetails += "<td style='text-align:center;'>" + item.RequiredCountPercentage + "</td>";
                else
                {
                    StaffDetails += "<td style='text-align:center;'>0</td>";
                }
                StaffDetails += "</tr>";
                sno++;
            }
           
            StaffDetails += "</table>";
            return StaffDetails;

        }

        public List<jntuh_college_attendace> Get_August(int? collegeId, string month)
        {
            List<jntuh_college_attendace> Attendance = new List<jntuh_college_attendace>();
            MySqlConnection con = new MySqlConnection("DataSource=10.10.10.15;user Id=jntu;password=jntu#12345;database=dataentryappeal20170911;");
            // MySqlConnection con = new MySqlConnection("DataSource=10.10.10.103;user Id=root;password=jntu123;database=dataentryappeal20170911;");
            con.Open();
            string query = "select * from jntuh_college_attendace where Collegeid =" + collegeId + " and Month ='" + month + "'";
            MySqlCommand cmd = new MySqlCommand(query, con);
            //if(con.State==true)
            //con
            MySqlDataReader dr = cmd.ExecuteReader();

            if (dr.HasRows)
            {
                while (dr.Read())
                {
                    jntuh_college_attendace data = new jntuh_college_attendace();
                    data.Collegeid = Convert.ToInt32(dr[1]);
                    data.CollegeCode = dr[3].ToString();
                    data.AcademicYearId = Convert.ToInt32(dr[2]);
                    data.EmpDeptID = Convert.ToInt32(dr[4]);
                    data.RegistrationNo = dr[5].ToString();
                    data.Month = dr[6].ToString();
                    data.TotalPercentage = Convert.ToDouble(dr[38]);
                    Attendance.Add(data);
                }
            }
            con.Close();

            return Attendance;
        }

        public List<FacultyCountNew> SurpriseStaffNew1(int? collegeID)
        {
            string[] months = { "August", "September", "October", "November" };
            var year = DateTime.Now.Year.ToString();
            string StaffDetails = string.Empty;
            string MonthName = string.Empty;
            int count = 1;
            monthCount = 15;
            int facultystudentRatio = 0;
            var jntuh_faculty_student_ratio_norms = db.jntuh_faculty_student_ratio_norms.ToList();

            string[] strOtherDepartments = { "English", "Mathematics", "Physics", "Chemistry", "Others(CSE/IT)", "Others(CIVIL/MECH)", "Others(ECE/EEE)", "Others(MNGT/H&S)" };

            List<CollegeFacultyWithIntakeReport> intakedetailsList = Get_PHD_Faculty(collegeID);

            List<string> New_PHD_Count1 = new List<string>();

            PHD_Count = intakedetailsList.Where(i => i.shiftId == 1 && !strOtherDepartments.Contains(i.Department)).Select(i => i.phdFaculty).Sum();

            foreach (var item in intakedetailsList.Where(i => i.shiftId == 1).ToList())
            {
                foreach (var item1 in item.PhdRegNos)
                {
                    New_PHD_Count1.Add(item1);
                }

            }

            int totalBtechFirstYearIntake = intakedetailsList.Where(d => d.Degree == "B.Tech" && !strOtherDepartments.Contains(d.Department)).Select(d => d.approvedIntake1).Sum();

            string[] ProposedIntakeZeroDeptName = intakedetailsList.Where(e => e.Degree == "B.Tech" && (e.approvedIntake1 == 0) && !strOtherDepartments.Contains(e.Department)).Select(e => e.Specialization).Distinct().ToArray();

            List<jntuh_college_attendace> jntuh_college_attendace = db.jntuh_college_attendace.Where(a => a.Collegeid == collegeID).ToList();

            List<jntuh_college_attendace> LoginList = new List<jntuh_college_attendace>();
            List<FacultyCountNew> facultylist = new List<FacultyCountNew>();




            foreach (var item5 in months)
            {

                facultyCountnew1 = 0;
                TotalCount1 = 0;
                DifferenceCount = 0;
                facultyCountnew = 0;
                Btech_Faculty_ProposedCount = 0;
                BTech_PHD_Count_New = 0;
                MTech_PHD_Count_New = 0;
                MCA_PHD_Count_New = 0;
                MBA_PHD_Count_New = 0;
                MAM_PHD_Count_New = 0;
                FacultyCountNew obj = new FacultyCountNew();

                if (item5 == "August")
                {
                    var August_attendance = Get_August(collegeID, item5);
                    LoginList = August_attendance.ToList();
                    August_attendance.Clear();
                }
                else
                {
                    LoginList = jntuh_college_attendace.Where(a => a.Month == item5).ToList();
                }


                //  LoginList = db.jntuh_college_attendace.Where(a => a.Collegeid == collegeID && a.Month == item5).ToList();

                string[] RegNos = new string[] { "9211-150409-180107", "6317-150413-162713", "96150406-153506", "78150407-104506", "90150406-132723", "5264-150409-160644", "8518-150413-150717", "7995-150414-143317", "4687-150420-162710", "1549-150418-123933", "8891-150420-165037", "2375-150427-184049", "5612-170202-102750", "9234-160302-113847", "49150406-131709", "4261-160308-074435", "4171-150624-143950", "0707-150408-175902", "74150406-141818", "16150402-155455", "54150402-122257", "6348-150411-120653", "6058-150417-174615", "8105-150412-154535", "8558-160312-194806", "1116-160313-055503", "3205-160313-105704", "7748-160313-063954", "2328-150408-154626", "8922-150410-144623", "6400-150409-153507", "4241-150413-144757", "5387-150420-124115", "6442-150413-163535", "3141-160302-143355", "2624-170202-123226", "2778-150621-184256", "6002-150408-104417", "7601-150409-104220", "5724-150409-131120", "9549-150409-131219", "5480-150416-141938", "8460-150505-150705", "5325-150411-101220", "9134-150418-123351", "1235-170107-122241", "3995-150420-172501", "5157-151216-105135", "5993-170208-144156", "0827-160314-154038", "43150407-121703", "9675-170201-060349", "1761-170110-124328", "8780-170212-221004", "42150406-105659", "9741-150408-163934", "2195-150408-145520", "7984-160128-143658", "0756-160128-144719", "8629-160128-141146", "6328-160129-102154", "6339-160128-152920", "5003-150410-100751", "3111-150413-163611", "6889-150413-145248", "7968-170117-141626", "4390-150619-131209", "7037-150619-123808", "6090-150416-211652", "0650-150419-232914", "9644-150420-123408", "2204-150506-223734", "5007-160303-052952", "1221-160111-140608", "5323-160316-172042", "5758-150409-145824", "9690-150419-143601", "2845-160318-215132", "6585-150414-151718", "6518-160222-100511", "37150407-125255", "4939-150419-180848", "7509-150415-191610", "7038-150418-123607", "4375-150420-144936", "4925-160204-183924", "5271-160206-142514", "9379-150417-164707", "4561-160211-082340", "4517-150424-122333", "4503-150419-134140", "8391-150419-133206", "7549-150409-171452", "2289-150418-215024", "5999-150418-172740", "7521-150418-223714", "4740-150419-162142", "9374-150419-225506", "0752-150422-165906", "6713-150422-171112", "7419-150420-154436", "0059-150420-164303", "7867-160309-173829", "0152-160309-181208", "50150406-110631", "4769-150416-174812", "8005-150416-145941", "2516-150422-195357", "8508-150412-113146", "1003-170119-133836", "4904-170119-140309", "0588-170126-140848", "8291-150624-160237", "6023-151229-151344", "4534-150408-104328", "9794-160229-175017", "3909-151230-121957", "68150407-154601", "1147-150412-155421", "6851-170208-173310", "8532-170213-171543", "77150407-144017", "4281-150415-144925", "8238-161024-112302", "1949-150421-144423", "3666-150408-102957", "1854-150414-123210", "8628-150414-121943", "2834-150414-131904", "8474-150420-113501", "5314-150422-160253", "6998-150422-145056", "8739-150422-124734", "5241-150422-143019", "1831-150422-122747", "7170-150422-115651", "4386-150422-113334", "1097-150422-111446", "2563-150409-185844", "6483-150409-120156", "8889-150423-121239", "3174-150423-145633", "7141-150416-152448", "3831-150426-164813", "1364-150426-165916", "8420-150426-163755", "0778-150501-170330", "2771-150622-225307", "8776-150622-232358", "7503-150424-104100", "9155-150622-231402", "7447-150525-115526", "9731-150622-154029", "3217-150409-132413", "2351-160302-144825", "8336-160302-171055", "93150406-161145", "67150407-173554", "7618-150410-162949", "9986-150619-175911", "94150404-124125", "00150407-113102", "2180-150424-222555", "1126-150507-163545", "1808-150621-133359", "4266-150622-125140", "7977-150623-115417", "7690-150415-134357", "6734-150415-133015", "4696-150409-112735", "9742-150409-105139", "5207-150409-145858", "88150407-162910", "7306-150411-145154", "2980-150411-105638", "9381-150411-113008", "7210-150428-042100", "2316-150428-040056", "5018-170111-130147", "2421-170118-102956", "0676-150408-111501", "2469-160121-104311", "94150402-171218", "9492-160121-160214", "2681-150625-150854", "6076-160311-171551", "0661-160312-141716", "2280-150408-151134", "0208-150411-124221", "8076-150409-161147", "5204-150412-144814", "4443-150412-150351", "9010-150412-151234", "3264-150418-173255", "0922-150415-111243", "0852-150429-154729", "4149-150621-173825", "6620-150625-180248", "7810-161226-114059", "4815-160201-103423", "6809-150505-133845", "0067-150505-141355", "5661-150505-153407", "5246-150505-151341", "7132-150505-171425", "9033-150430-105308", "3057-150506-113420", "8389-150506-170835", "2511-150506-173242", "4009-150408-154747", "8440-150624-131011", "9393-160306-105928", "4283-160315-194106", "3404-170119-172558", "7762-170213-131706", "1475-150414-183509", "7134-160218-150219", "0414-150427-153301", "2067-150414-155501", "33150330-155256", "87150406-194332", "8362-150409-110622", "6180-150408-151453", "7098-150426-074226", "7386-150414-125915", "1466-170105-194422", "1997-170112-095532", "5841-170125-113934", "5612-170125-124150", "1782-150408-121508", "1781-150409-114620", "47150407-153633", "6498-150414-132024", "9077-150416-124631", "5914-150504-100508", "0940-150504-125126", "7835-150506-141716", "6244-150622-125503", "8145-150623-151823", "4830-150623-163945", "6188-150413-160703", "3295-151219-103214", "8352-150625-115421", "7697-150625-114147", "7437-150412-112238", "6903-150506-125905", "8943-150507-132216", "3855-150507-145302", "5572-150507-124125", "3430-150507-155250", "3059-150516-165055", "0336-150507-122950", "1400-150507-152048", "4765-170522-104257", "09150402-160427", "6421-151221-144330", "7885-160312-140130", "0052-160312-131632", "0127-160312-141609", "3725-160312-151706", "9069-160312-151048", "7922-160312-160244", "0682-160312-172016", "2685-160312-172344", "7093-160312-154220", "3195-160314-232954", "5912-150408-122341", "84150406-142457", "7913-150417-131457", "6808-150424-004516", "7368-150419-164942", "3038-150413-112451", "58150406-100658", "68150404-140248", "10150404-134548", "61150404-162737", "56150407-160735", "8021-150409-165612", "8901-150430-111830", "1584-150409-112324", "9049-150416-103205", "95150404-130909", "1083-150417-152920", "1809-150428-104045", "7477-150428-103245", "9865-150426-124940", "6104-150427-134207", "6315-160225-130444", "8441-150413-113304", "9811-150409-103922", "8964-150420-121821", "4723-160311-151729", "03150402-152426", "9373-150426-135654", "8422-150410-113036", "9552-150410-111820", "4713-150420-124126", "6087-170212-130603", "9966-170211-170427", "0319-150415-110441", "9914-150414-143319", "8688-150413-214801", "4046-150507-120439", "5688-160311-160356", "37150406-124138", "86150406-152725", "6080-160305-164900", "7098-160309-162246", "3941-160309-164350", "0502-170213-143616", "6329-170131-185402", "1338-150415-172930", "0443-150620-112146", "39150405-125954", "4439-150422-115107", "6970-150415-123739", "5363-150417-125713", "6048-160225-114522", "8477-160301-195215", "7934-160301-193415", "7595-150423-120919", "5464-150427-154315", "1054-160102-160709", "15150407-161016", "2585-150410-170055", "8344-150429-162201", "8833-150504-131131", "0184-150429-124639", "0767-150430-105420", "0799-150430-113108", "1784-150430-101515", "2900-150430-114213", "3100-150501-103154", "3253-150430-111321", "3477-150430-113036", "4437-150430-104224", "4756-150411-150337", "4861-150430-112525", "5200-150501-103751", "5107-150430-104415", "5360-150430-090404", "6453-150507-190621", "7074-150507-151336", "1705-170116-142649", "91150406-161756", "9383-160128-125153", "0376-150421-155232", "2043-160310-154642", "23150403-182551", "0220-150422-163321", "3271-150506-120301", "0831-160218-160442", "3138-160219-144523", "3570-160219-145952", "5730-160219-155537", "3603-160220-122613", "4448-160220-193243", "5148-160220-114832", "1924-160220-113523", "6796-160220-221344", "5301-160304-192359", "8226-160305-162320", "9581-160305-115025", "8823-160305-140040", "1429-150427-153420", "5607-150425-102421", "47150406-125841", "7217-160209-132527", "2719-150417-140130", "8501-150421-095920", "45150407-154450", "1067-150419-120449", "5555-160129-181224", "0855-160314-234426", "8195-160314-180632", "2658-160314-185351", "8213-160314-204848", "2521-160314-200255", "9616-160314-214031", "0244-160314-231118", "3845-160314-214240", "1475-160315-001332", "3897-160314-200045", "7710-160319-204556", "6424-160319-215612", "7341-160319-202433", "6897-160319-221313", "8988-160319-224714", "5086-150410-102356", "7920-150413-130841", "2861-150410-135151", "6794-160319-164923", "2163-160319-175204", "8260-160319-182908", "6607-160319-151901", "2549-160319-143150", "5086-160319-123814", "2359-160319-121236", "8319-160319-114531", "52150401-155113", "6429-150410-134158", "8471-150412-173932", "2546-150412-173119", "5430-150413-163549", "4595-150422-114349", "7960-150502-150257", "1556-150502-151010", "1620-150502-152356", "5563-150410-155500", "8462-150422-133254", "4469-150422-140349", "39150404-122803", "89150406-135317", "8733-160220-105912", "3280-160311-151200", "68150404-143104", "1620-150419-083055", "98150402-140537", "4240-150619-124624", "90150402-150047", "5077-150411-114936", "1735-160106-153643", "4305-160313-173609", "4707-160313-172938", "7992-160313-172334", "4292-160313-174602", "2969-160313-170725", "7697-160314-151405", "5247-160314-201702", "2991-160314-202954", "3831-170207-161803", "52150407-113402", "1185-150410-154249", "4181-150415-152826", "63150406-133241", "7082-160225-144210", "6833-150429-151901", "7084-150603-172353", "8010-150421-124203", "5725-150505-114121", "1394-150430-232728", "60150406-153912", "9700-150418-103531", "5407-150420-114851", "0261-150421-122215", "4944-150507-132508", "0878-160225-114919", "4393-150421-155815", "0967-160310-130928", "2599-150421-154955", "6087-160224-152646", "0546-160224-111407", "7909-160312-182449", "0487-160312-190833", "9358-170111-094327", "6379-170112-093250", "9888-150421-113110", "1166-150622-055114", "2952-150624-163859", "7893-150421-160431", "87150404-160809", "2876-150409-150305", "8578-150418-145228", "4050-150424-155306", "70150404-125108", "9618-150416-122424", "8941-150415-152633", "0959-170118-101333", "0774-150427-133052", "0785-160304-122835", "9791-150422-162245", "22150407-163124", "53150406-105254", "1528-160304-125739", "8238-161224-112812", "3531-150421-143805", "4278-150416-175043", "7787-150411-145051", "56150403-213949", "6604-150416-125428", "4857-150421-080322", "0023-160316-130354", "1472-160312-141517", "7735-150421-182347", "50150403-105221", "2813-150408-164256", "3538-150411-104526", "0560-150428-125331", "2763-160203-170600", "0170-160307-001639", "9304-160306-225725", "0171-160306-235900", "2124-160306-232603", "3439-160316-105705", "7980-160316-105741", "5725-150409-102427", "4711-150426-181413", "4892-150413-110154", "0895-150413-112139", "0093-150413-132750", "4698-160201-104955", "8540-150413-110353", "6074-150413-170744", "4574-150620-175558", "5645-150620-182613", "1693-150412-112919", "0158-150427-152718", "2008-150421-095104", "4272-160108-143318", "7884-150413-153150", "4800-150415-103452", "1136-160311-110044", "9656-170111-081909", "33150407-101427", "1266-150427-190250", "2093-170207-122958", "3507-150409-123348", "7541-150412-152809", "2825-150420-092811", "9437-150420-103610", "4992-150409-141335", "3498-150408-231802", "6166-150512-104107", "9768-150427-210434", "0205-150508-120436", "2637-150425-151316", "2513-150425-192936", "4188-150424-144352", "2456-150427-125655", "1660-150507-152429", "3570-150507-110913", "1601-150408-172051", "0148-150419-091522", "1442-161217-164741", "31150403-201030", "0941-170207-002441", "8575-170131-031553", "9965-170131-032636", "5691-170131-033654", "8645-170131-034806", "7939-170131-035846", "3351-170131-041646", "9837-170131-042413", "2633-170131-043329", "2964-170131-045303", "2440-170131-201700", "4894-170201-001106", "7664-160522-105414", "6997-170207-182304", "7197-170207-173901", "7397-160524-140443", "9865-160525-110126", "4556-160522-113614", "4469-150409-124255" };

                LoginList = LoginList.Where(l => !RegNos.Contains(l.RegistrationNo)).ToList();





                var PHD = LoginList.Where(l => New_PHD_Count1.Contains(l.RegistrationNo)).ToList();



                if (item5 == "August" || item5 == "October")
                {
                    PHDPresentCount = PHD.Where(s => s.TotalPercentage >= monthCount).Count();

                }
                else
                {
                    PHDPresentCount = PHD.Where(s => s.TotalPercentage >= monthCount).Count();

                }



                foreach (var item in intakedetailsList)
                {
                    //var IntakeCount = 0;
                    var phdpercentagecount = 0;
                    var nonphdpercentagecount = 0;
                    if (!strOtherDepartments.Contains(item.Department))
                    {

                        if (item.Degree == "B.Tech")
                        {
                            Btech_Faculty_ProposedCount += item.approvedIntake1;
                            BTech_PHD_Count_New += item.SpecializationsphdFaculty;

                        }
                        else if (item.Degree == "M.Tech")
                        {
                            Mtech_Faculty_ProposedCount += item.approvedIntake1;

                            MTech_PHD_Count_New += item.SpecializationsphdFaculty;
                        }
                        else if (item.Degree == "B.Pharmacy")
                        {
                            BPharmacy_Faculty_ProposedCount += item.approvedIntake1;
                        }
                        else if (item.Degree == "M.Pharmacy")
                        {
                            Mpharmacy_Faculty_ProposedCount += item.approvedIntake1;
                        }
                        else if (item.Degree == "MBA")
                        {
                            MBA_Faculty_ProposedCount += item.approvedIntake1;
                            MBA_PHD_Count_New += item.SpecializationsphdFaculty;
                        }
                        else if (item.Degree == "MCA")
                        {
                            MCA_Faculty_ProposedCount += item.approvedIntake1;
                            MCA_PHD_Count_New += item.SpecializationsphdFaculty;
                        }
                        else if (item.Degree == "Pharm.D")
                        {
                            Pharm_D_Faculty_ProposedCount += item.approvedIntake1;
                        }
                        else if (item.Degree == "Pharm.D PB")
                        {
                            Pharm_DPB_Faculty_ProposedCount += item.approvedIntake1;
                        }
                        else if (item.Degree == "MAM")
                        {
                            MAM_Faculty_ProposedCount += item.approvedIntake1;
                            MAM_PHD_Count_New += item.SpecializationsphdFaculty;
                        }
                        else
                        {
                            MTM_Faculty_ProposedCount += item.approvedIntake1;
                        }

                        var LoginList_New = LoginList.Where(s => s.EmpDeptID == item.DepartmentID).ToList();

                        if (item.PhdRegNos.Length != 0)
                        {
                            var PhdFacultyList = LoginList_New.Where(r => item.PhdRegNos.Contains(r.RegistrationNo)).ToList();

                            if (item5 == "August" || item5 == "October")
                            {
                                phdpercentagecount = PhdFacultyList.Where(s => s.TotalPercentage >= monthCount).Count();


                                if (item.NonPhdRegNos.Length != 0)
                                {
                                    var FacultyList = LoginList_New.Where(r => item.NonPhdRegNos.Contains(r.RegistrationNo)).ToList();
                                    nonphdpercentagecount = FacultyList.Where(s => s.TotalPercentage >= monthCount).Count();

                                }
                                else
                                {
                                    nonphdpercentagecount = 0;
                                }

                            }
                            else
                            {
                                phdpercentagecount = PhdFacultyList.Where(s => s.TotalPercentage >= monthCount).Count();


                                if (item.NonPhdRegNos.Length != 0)
                                {
                                    var FacultyList = LoginList_New.Where(r => item.NonPhdRegNos.Contains(r.RegistrationNo)).ToList();
                                    nonphdpercentagecount = FacultyList.Where(s => s.TotalPercentage >= monthCount).Count();

                                }
                                else
                                {
                                    nonphdpercentagecount = 0;
                                }

                            }



                        }
                        else
                        {
                            if (item5 == "August" || item5 == "October")
                            {
                                if (item.NonPhdRegNos.Length != 0)
                                {
                                    var FacultyList = LoginList_New.Where(f => item.NonPhdRegNos.Contains(f.RegistrationNo)).ToList();
                                    nonphdpercentagecount = FacultyList.Where(s => s.TotalPercentage >= monthCount).Count();

                                }
                                else
                                {
                                    nonphdpercentagecount = 0;
                                }

                            }
                            else
                            {
                                if (item.NonPhdRegNos.Length != 0)
                                {
                                    var FacultyList = LoginList_New.Where(f => item.NonPhdRegNos.Contains(f.RegistrationNo)).ToList();
                                    nonphdpercentagecount = FacultyList.Where(s => s.TotalPercentage >= monthCount).Count();

                                }
                                else
                                {
                                    nonphdpercentagecount = 0;
                                }
                            }
                        }

                        count++;
                        TotalCount += item.totalFaculty;
                        if (item.shiftId == 1)
                            TotalCount1 += (phdpercentagecount + nonphdpercentagecount);
                        else { }
                        if (item.shiftId == 1)
                            DifferenceCount += ((item.ugFaculty + item.pgFaculty + item.phdFaculty) - (phdpercentagecount + nonphdpercentagecount));
                        else { }


                    }
                    else
                    {
                        var LoginList_New = LoginList.Where(s => s.EmpDeptID == item.DepartmentID).ToList();

                        if (item.PhdRegNos.Length != 0)
                        {
                            var PhdFacultyList = LoginList_New.Where(r => item.PhdRegNos.Contains(r.RegistrationNo)).ToList();

                            if (item5 == "August" || item5 == "October")
                            {
                                phdpercentagecount = PhdFacultyList.Where(s => s.TotalPercentage >= monthCount).Count();


                                if (item.NonPhdRegNos.Length != 0)
                                {
                                    var FacultyList = LoginList_New.Where(r => item.NonPhdRegNos.Contains(r.RegistrationNo)).ToList();
                                    nonphdpercentagecount = FacultyList.Where(s => s.TotalPercentage >= monthCount).Count();

                                }
                                else
                                {
                                    nonphdpercentagecount = 0;
                                }

                            }
                            else
                            {
                                phdpercentagecount = PhdFacultyList.Where(s => s.TotalPercentage >= monthCount).Count();


                                if (item.NonPhdRegNos.Length != 0)
                                {
                                    var FacultyList = LoginList_New.Where(r => item.NonPhdRegNos.Contains(r.RegistrationNo)).ToList();
                                    nonphdpercentagecount = FacultyList.Where(s => s.TotalPercentage >= monthCount).Count();

                                }
                                else
                                {
                                    nonphdpercentagecount = 0;
                                }

                            }
                        }
                        else
                        {

                            if (item5 == "August" || item5 == "October")
                            {
                                if (item.NonPhdRegNos.Length != 0)
                                {
                                    var FacultyList = LoginList_New.Where(r => item.NonPhdRegNos.Contains(r.RegistrationNo)).ToList();
                                    nonphdpercentagecount = FacultyList.Where(s => s.TotalPercentage >= monthCount).Count();

                                }
                                else
                                {
                                    nonphdpercentagecount = 0;
                                }

                            }
                            else
                            {
                                if (item.NonPhdRegNos.Length != 0)
                                {
                                    var FacultyList = LoginList_New.Where(r => item.NonPhdRegNos.Contains(r.RegistrationNo)).ToList();
                                    nonphdpercentagecount = FacultyList.Where(s => s.TotalPercentage >= monthCount).Count();

                                }
                                else
                                {
                                    nonphdpercentagecount = 0;
                                }
                            }
                        }

                        count++;
                        TotalCount += item.totalFaculty;
                        TotalCount1 += (phdpercentagecount + nonphdpercentagecount);
                        DifferenceCount += ((item.ugFaculty + item.pgFaculty + item.phdFaculty) - (phdpercentagecount + nonphdpercentagecount));
                    }


                }
                TotalCount = (TotalCount - intakedetailsList.Where(l => l.shiftId == 2).Select(l => l.totalFaculty).Sum());







                var New_intakedetails = intakedetailsList.Where(r => r.DegreeID != 0).GroupBy(r => r.DegreeID).ToList();




                var count_new = 1;
                foreach (var item2 in New_intakedetails)
                {
                    var data_latest = 0;
                    var Degree_PHD_Count = 0;
                    var Degree = string.Empty;
                    var Facultyratio = Convert.ToInt32(jntuh_faculty_student_ratio_norms.Where(fn => fn.degreeId == item2.Key).Select(fn => fn.Norms).FirstOrDefault().Split(':')[1].ToString());
                    dynamic TotalRequired_facultyCount = 0;
                    if (item2.Key == 4)
                    {
                        Degree = "B.Tech";
                        TotalRequired_facultyCount = (int)((Btech_Faculty_ProposedCount / Facultyratio) + intakedetailsList.Where(s => s.DegreeID == item2.Key).Select(s => s.requiredFaculty).Sum());
                        TotalRequired_facultyCount = TotalRequired_facultyCount - BTech_PHD_Count_New;
                        Degree_PHD_Count = BTech_PHD_Count_New;

                        data_latest = TotalRequired_facultyCount + BTech_PHD_Count_New;

                    }
                    else if (item2.Key == 1)
                    {
                        Degree = "M.Tech";
                        TotalRequired_facultyCount = (int)(intakedetailsList.Where(s => s.DegreeID == item2.Key && s.shiftId == 1).Select(s => s.requiredFaculty).Sum());

                        TotalRequired_facultyCount = TotalRequired_facultyCount - MTech_PHD_Count_New;
                        Degree_PHD_Count = MTech_PHD_Count_New;

                        data_latest = TotalRequired_facultyCount + MTech_PHD_Count_New;

                    }
                    else if (item2.Key == 5)
                    {
                        var PharmdDegree = intakedetailsList.Where(s => s.academicYearId == 9 && s.Degree == "Pharm.D").Select(s => s.Degree).FirstOrDefault();
                        var PharmdpbDegree = intakedetailsList.Where(s => s.academicYearId == 9 && s.Degree == "Pharm.D PB").Select(s => s.Degree).FirstOrDefault();
                        PharmdDegree = PharmdDegree != null ? "+" + PharmdDegree : "";
                        PharmdpbDegree = PharmdpbDegree != null ? "+" + PharmdpbDegree : "";
                        Degree = "B.Pharmacy" + PharmdDegree + "" + PharmdpbDegree;
                        TotalRequired_facultyCount = (int)(intakedetailsList.Where(s => s.DegreeID == item2.Key).Select(s => s.requiredFaculty).Sum());


                    }
                    else if (item2.Key == 2)
                    {
                        Degree = "M.Pharmacy";
                        TotalRequired_facultyCount = (int)(intakedetailsList.Where(s => s.DegreeID == item2.Key && s.shiftId == 1).Select(s => s.requiredFaculty).Sum());


                    }
                    else if (item2.Key == 6)
                    {
                        Degree = "MBA";
                        TotalRequired_facultyCount = (int)(intakedetailsList.Where(s => s.DegreeID == item2.Key).Select(s => s.requiredFaculty).Sum());
                        TotalRequired_facultyCount = TotalRequired_facultyCount - MBA_PHD_Count_New;
                        Degree_PHD_Count = MBA_PHD_Count_New;
                        data_latest = TotalRequired_facultyCount + MBA_PHD_Count_New;

                    }
                    else if (item2.Key == 3)
                    {
                        Degree = "MCA";
                        TotalRequired_facultyCount = (int)(intakedetailsList.Where(s => s.DegreeID == item2.Key).Select(s => s.requiredFaculty).Sum());
                        TotalRequired_facultyCount = TotalRequired_facultyCount - MCA_PHD_Count_New;
                        Degree_PHD_Count = MCA_PHD_Count_New;
                        data_latest = TotalRequired_facultyCount + MCA_PHD_Count_New;

                    }

                    else if (item2.Key == 7)
                    {
                        Degree = "MAM";
                        TotalRequired_facultyCount = (int)(intakedetailsList.Where(s => s.DegreeID == item2.Key).Select(s => s.requiredFaculty).Sum());
                        TotalRequired_facultyCount = TotalRequired_facultyCount - MAM_PHD_Count_New;
                        Degree_PHD_Count = MAM_PHD_Count_New;
                        data_latest = TotalRequired_facultyCount + MAM_PHD_Count_New;

                    }
                    else
                    {
                        Degree = "MTM";
                        TotalRequired_facultyCount = (int)(intakedetailsList.Where(s => s.DegreeID == item2.Key).Select(s => s.requiredFaculty).Sum());
                    }
                    if (item2.Key == 9 || item2.Key == 10)
                    {
                        //Not Dispaly
                    }
                    else
                    {

                        count_new++;
                        facultyCountnew1 += Degree_PHD_Count;
                        facultyCountnew2 += TotalRequired_facultyCount;
                        facultyCountnew += data_latest;
                        // facultyCountnew += (int)TotalRequired_facultyCount;
                    }

                }
                obj.CollegeId = collegeID;
                obj.Monthname = item5;
                obj.UploadedCount = TotalCount;
                obj.BiometricCount = TotalCount1;
                obj.BASDifferenceCount = DifferenceCount;
                obj.RequiredCount = facultyCountnew;
                obj.RequiredCountPercentage = Math.Round(((((double)facultyCountnew - (double)TotalCount1) / (double)facultyCountnew) * 100), 2);
                obj.RequiredPHDCount = facultyCountnew1 == 0 ? 1 : facultyCountnew1;
                obj.PHDPresentCount = PHDPresentCount;
                obj.PHDCountPercentage = Math.Round(((((double)obj.RequiredPHDCount - (double)PHDPresentCount) / (double)obj.RequiredPHDCount) * 100), 2);
                obj.RequiredDifferenceCount = facultyCountnew - TotalCount1;

                facultylist.Add(obj);
            }

            StaffDetails += "<table border='1' cellpadding='3' cellspacing='0' width='100%' style='font-family: Times New Roman; font-size: 8px; font-weight: normal; background-color: #fff; margin: 0 auto;'>";
            StaffDetails += "<tr>";
            StaffDetails += "<td style='text-align:center;'>S.No</td>";
            StaffDetails += "<td style='text-align:center;'>Month</td>";
            StaffDetails += "<td style='text-align:center;'>Number of faculty not giving Biometric attendance regularly(for atleast 15 days in a month)</td>";
            StaffDetails += "<td style='text-align:center;'>% of faculty not giving Biometric attendance regularly w.r.t required faculty</td>";

            StaffDetails += "</tr>";

            int sno = 1;
            double minPercentage = (double)40;
            foreach (var item in facultylist)
            {
                if (item.RequiredCountPercentage >= minPercentage)
                {
                    StaffDetails += "<tr>";
                    StaffDetails += "<td style='text-align:center;'>" + sno + "</td>";
                    StaffDetails += "<td style='text-align:center;'>" + item.Monthname + ",2017</td>";
                    StaffDetails += "<td style='text-align:center;'>" + item.BASDifferenceCount + "</td>";
                    StaffDetails += "<td style='text-align:center;'>" + item.RequiredCountPercentage + "</td>";
                    StaffDetails += "</tr>";
                    sno++;
                }
                //else
                //{
                //    if(sno==1)
                //    {

                //    }
                //}

                //if (item.RequiredCountPercentage >= minPercentage)
                //    StaffDetails += "<td style='text-align:center;'>" + item.BASDifferenceCount + "</td>";
                //else
                //    StaffDetails += "<td style='text-align:center;'>NA</td>";
                //if (item.RequiredCountPercentage >= minPercentage)
                //    StaffDetails += "<td style='text-align:center;'>" + item.RequiredCountPercentage + "</td>";
                //else
                //    StaffDetails += "<td style='text-align:center;'>NA</td>";


            }

            StaffDetails += "</table>";
            //   return StaffDetails;
            return facultylist;
        }

        #endregion 



    }
}
