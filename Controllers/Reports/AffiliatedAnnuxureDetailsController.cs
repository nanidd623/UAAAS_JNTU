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
namespace UAAAS.Controllers.Reports
{
    public class AffiliatedAnnuxureDetailsController : Controller
    {
        //
        // GET: /AffiliatedAnnuxureDetails/

       


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

        //public ActionResult Index()
        //{
        //    return View();
        //}

        //deficiency letter formats 



         public ActionResult Index()
         {
            
           
             List<AffilicationClass>  alldetails=new List<AffilicationClass>();
             var pharmacyids = new[]{24,27,30,33,34,44,47,52,55,65,78,83,95,97,104,105,107,110,114,117,118,127,135,136,139,146,150,159,169,202,204,213,219,234,239,253,
                262,263,267,275,283,284,290,295,297,298,301,303,314,317,320,348,353,362,376,377,379,384,392,395,410,427,428,436,442,445,448,454,457,458,6,
                45,54,58,90,120,206,237,302,313,318,319,389,60,66,252,255,315,370};




             int[] CollegeIds = db.jntuh_college_edit_status.Where(e => e.IsCollegeEditable == false).Select(e => e.collegeId).Take(200).ToArray();// //db.jntuh_college.Where(e=>e.isActive==true).Select(e => e.id).Distinct().ToArray();

             //&&!pharmacyids.Contains(e.collegeId)
          


            // var ids = CollegeIds.Skip(200).ToArray();
             foreach (var item in CollegeIds)
             {

                 alldetails.AddRange(CollegeCoursesAllClear(item));
                // alldetails = CollegeCoursesAllClear(collegeid);
             }
            
            // alllabs = alllabs.Where(e => e.LabName != "NIL").ToList();
             //int collegeid = 54;
             //alldetails = CollegeCoursesAllClear(collegeid);
             int count = alldetails.Count();
             string ReportHeader = "AffiliatedAnnuxure.xls";
             if (count != 0)
             {
                 Response.ClearContent();
                 Response.Buffer = true;
                 Response.AddHeader("content-disposition", "attachment; filename=" + ReportHeader);
                 Response.ContentType = "application/vnd.ms-excel";
                 return PartialView("~/Views/Reports/_AffiliatedAnnuxureReport.cshtml", alldetails);
             }


            return null;
        }




        public ActionResult AffiliationLetter(int collegeId, string type)
        {
            List<int> collegeIds = db.jntuh_college.Where(c => c.collegeCode != "WL").Select(c => c.id).ToList();

            string code = db.jntuh_college.Where(c => c.id == collegeId).Select(c => c.collegeCode).FirstOrDefault().ToUpper();

            string path = SaveCollegeDefficiencyLetterPdf(code, type);
            path = path.Replace("/", "\\");
            if (!string.IsNullOrEmpty(path))
            {
                return File(path, "application/pdf", path.Substring(path.LastIndexOf('\\') + 1));
            }
            return null;
        }

        private string SaveCollegeDefficiencyLetterPdf(string collegeCode, string type)
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

            //var dates = db.jntuh_dates.Where(d => d.collegecode == collegeCode).Select(d => d).FirstOrDefault();
            var dates = db.jntuh_college_edit_status.Where(d => d.collegeId == collegeid).Select(d => d).FirstOrDefault();
            var Appeal = db.jntuh_appeal_college_edit_status.Where(d => d.collegeId == collegeid && d.IsCollegeEditable == false).Select(d => d).FirstOrDefault();
            contents = contents.Replace("##APPLICATION_DATE##", ((DateTime)dates.updatedOn).ToString("dd-MM-yyy"));
            // contents = contents.Replace("##DEFICIENCY_REPORT_DATE##", ((DateTime)dates.deficiencyLetterDate).ToString("dd-MM-yyy"));

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
                //string[] courses = CollegeCoursesAllClear(collegeid).Split('$');
                //contents = contents.Replace("##COURSE_TABLE##", courses[0]);
                //contents = contents.Replace("##ASSESSMNET_TABLE##", courses[1]);
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

            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);

            var degreeDetails = (from ie in db.jntuh_college_intake_existing
                                 join js in db.jntuh_specialization on ie.specializationId equals js.id
                                 join jd in db.jntuh_department on js.departmentId equals jd.id
                                 join deg in db.jntuh_degree on jd.degreeId equals deg.id
                                 join sh in db.jntuh_shift on ie.shiftId equals sh.id
                                 where (ie.collegeId == collegeId && ie.academicYearId == 8)
                                 select new
                                 {
                                     ie.proposedIntake,
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

                courses += "<td colspan='2' align='center'>" + item.proposedIntake + "</td>";
                courses += "</tr>";

                rowCount++;
                //}

            }

            courses += "</table>";

            return courses;
        }

        public List<AffilicationClass> CollegeCoursesAllClear(int collegeId)
        {
            string courses = string.Empty;
            string assessments = string.Empty;
            var jntuh_college = db.jntuh_college.ToList();
            List<AffilicationClass> annuxeuelis = new List<AffilicationClass>();
            List<CollegeIntakeExisting> collegeIntakeExisting = new List<CollegeIntakeExisting>();

           // int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);

            var degreeDetails = (from ie in db.jntuh_college_intake_existing
                                 join js in db.jntuh_specialization on ie.specializationId equals js.id
                                 join jd in db.jntuh_department on js.departmentId equals jd.id
                                 join deg in db.jntuh_degree on jd.degreeId equals deg.id
                                 join sh in db.jntuh_shift on ie.shiftId equals sh.id
                                 where (ie.collegeId == collegeId && ie.academicYearId == 8)
                                 select new
                                 {
                                     ie.proposedIntake,
                                     js.specializationName,
                                     deg.degree,
                                     sh.shiftName,
                                     deg.degreeDisplayOrder,
                                     js.id
                                 }).OrderBy(d => d.degreeDisplayOrder).ThenBy(d => d.specializationName).ThenBy(d => d.shiftName).ToList();

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

            //if (fFlag == "YES" && lFlag == "YES")
            //{
            //}

            //else
            //{

            List<string> deficiencyDepartments = new List<string>();

            foreach (var course in deficiencyCourses)
            {
                AffilicationClass annuxurecls=new AffilicationClass();
                if (!deficiencyDepartments.Contains(course.Department))
                {
                    deficiencyDepartments.Add(course.Department);
                }

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

                if (((course.Required - course.Available) <= percentage && course.PhdDeficiency != "YES" &&
                     course.LabsDeficiency == "NIL" && bpharmacydeficiecny)) //|| isCourseAffiliated != false
                {

                }

                else
                {
                    
                
                assessments += "<tr>";
                assessments += "<td colspan='1' align='center'>" + (assessmentCount + 1) + "</td>";
                annuxurecls.SNo = assessmentCount + 1;
                annuxurecls.CollegeCode =jntuh_college.Where(e => e.id == collegeId).Select(e => e.collegeCode).FirstOrDefault();
                annuxurecls.CollegeName = jntuh_college.Where(e => e.id == collegeId).Select(e => e.collegeName).FirstOrDefault();
                annuxurecls.ProposedIntake = course.TotalIntake;
                course.Shift = "1";
                if (course.Shift == "1")
                {
                    assessments += "<td colspan='5' >" + course.Degree + " - " + course.Specialization + "</td>";
                    annuxurecls.Degree = course.Degree;
                    annuxurecls.Specialization = course.Specialization;
                    annuxurecls.Department = course.Department;
                  //  annuxurecls.
                }
                annuxurecls.Required = course.Required;
                    annuxurecls.Available = course.Available > 0 ? course.Available : 0;
                annuxurecls.TotalIntake = course.Required - course.Available;

                annuxurecls.PhdDeficiency = course.PhdDeficiency;
                annuxurecls.LabsDeficiency = course.LabsDeficiency.Equals("No Equipement Uploaded")? course.Specialization: course.LabsDeficiency;
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

                

                annuxeuelis.Add(annuxurecls);

                }
            }

            foreach (var course in clearedCourses)
            {
                string degreeType = db.jntuh_degree.Join(db.jntuh_degree_type, d => d.degreeTypeId, t => t.id, (d, t) => new { d, t })
                                                   .Where(a => a.d.degree == course.Degree).Select(a => a.t.degreeType).FirstOrDefault();

                List<string> clearedDepartments = clearedCourses.Select(a => a.Department).ToList(); //.Where(a => a.DegreeType == "UG")
                //degreeType != "UG" && 
                if (deficiencyDepartments.Contains(course.Department) && !clearedDepartments.Contains(course.Department))
                {
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

            if (clearedCourses.Count() == 0 || totalCleared == totalZeroIntake)
            {
                courses += "<tr>";
                courses += "<td colspan='13' align='center'>NIL</td>";
                courses += "</tr>";
            }

            assessments += "</table>";
            courses += "</table>";

           // return courses + "$" + assessments;
            return annuxeuelis;
        }

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
                else if (adjustedPHDFaculty >= 1 && degreeType.Equals("PG") && item.Degree == "MBA")
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
                        int availableCount = collegeLabMaster.Where(m => requiredLabs.Contains(m.EquipmentID)).Count();

                        int[] labmasterids = labMaster.Where(m => m.SpecializationID == specializationid && m.Year == year && m.Semester == semester && m.Labcode == labCode).Select(m => m.id).Distinct().ToArray();
                        int[] collegelabequipmentids = jntuh_college_laboratories.Where(i => labmasterids.Contains(i.EquipmentID) && i.EquipmentNo == 1).Select(i => i.id).Distinct().ToArray();

                        if (requiredCount > availableCount && labmasterids.Count() != collegelabequipmentids.Count())//&& labCode!="14LAB"
                        {
                            string labName = labMaster.Where(m => m.SpecializationID == specializationid && m.Year == year && m.Semester == semester && m.Labcode == labCode).Select(m => m.LabName).FirstOrDefault();
                            if (!string.IsNullOrEmpty(labName))
                                defs.Add(year + "-" + semester + "-" + labName);
                            //else
                            //    defs.Add(null);
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
                                                        && (rf.NoSCM != true) && (rf.PANNumber != null) && rf.Notin116 != true && rf.Blacklistfaculy != true && rf.PHDundertakingnotsubmitted != true) && (rf.DepartmentId != 61 || rf.DepartmentId != 27) && rf.type != "Adjunct" && rf.DepartmentId == 15).ToList();
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

            ////approved
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
            public int Available { get; set; }
            public string Deficiency { get; set; }
            public string PhdDeficiency { get; set; }
            public string LabsDeficiency { get; set; }
        }

        public class AffilicationClass
        {
            public string CollegeCode { get; set; }
            public string CollegeName { get; set; }
            public int SNo { get; set; }
            public int DegreeDisplayOrder { get; set; }
            public string DegreeType { get; set; }
            public string Degree { get; set; }
            public string Department { get; set; }
            public string Specialization { get; set; }
            public int SpecializationId { get; set; }
            public string Shift { get; set; }
            public int ProposedIntake { get; set; }
            public int TotalIntake { get; set; }
            public int Required { get; set; }
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
                    registered_faculty_specialization = rf.jntuh_registered_faculty_education.Where(e => e.jntuh_education_category.educationCategoryName == "PG").FirstOrDefault().specialization !=null ? rf.jntuh_registered_faculty_education.Where(e => e.jntuh_education_category.educationCategoryName == "PG").Select(i => i.specialization).FirstOrDefault().ToUpper().Trim() : ""
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
                Allgroupscount = 0;
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





    }
}
