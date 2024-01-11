using Ionic.Zip;
using iTextSharp.text;
using iTextSharp.text.html.simpleparser;
using iTextSharp.text.pdf;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Security;
using UAAAS.Models;

namespace UAAAS.Controllers
{
    public class BASReportController : BaseController
    {
        //
        // GET: /BASReport/

        private uaaasDBContext db = new uaaasDBContext();
        List<jntuh_academic_year> jntuh_academic_year_db = new List<jntuh_academic_year>();
        int actualYearDb = 0;
        int academicYearIdDb = 0;
        int PresentYearDb = 0;
        public BASReportController()
        {
            jntuh_academic_year_db = db.jntuh_academic_year.ToList();
            //actualYearDb = jntuh_academic_year_db.Where(w => w.isActive == true && w.isPresentAcademicYear == true).Select(e => e.actualYear).FirstOrDefault();
            //academicYearIdDb = jntuh_academic_year_db.Where(w => w.actualYear == (actualYearDb + 1)).Select(e => e.id).FirstOrDefault();
            //PresentYearDb = jntuh_academic_year_db.Where(w => w.actualYear == (actualYearDb + 1)).Select(e => e.actualYear).FirstOrDefault();

            actualYearDb = 2019;
            academicYearIdDb = 12;
            PresentYearDb = 2020;
        }

        public ActionResult Index()
        {
            List<SelectListItem> dates = new List<SelectListItem>();
            DateSelected sss = new DateSelected();

            var jntuh_academic_year = db.jntuh_academic_year.ToList();

            int actualYear = jntuh_academic_year.Where(e => e.isActive == true && e.isPresentAcademicYear == true).Select(e => e.actualYear).FirstOrDefault();

            int AY1 = jntuh_academic_year.Where(e => e.actualYear == (actualYear + 1)).Select(e => e.id).FirstOrDefault();
            int AY2 = jntuh_academic_year.Where(e => e.actualYear == (actualYear)).Select(e => e.id).FirstOrDefault();

            int[] Years = new int[] { AY1, AY2 };
            var Data = jntuh_academic_year.Where(s => Years.Contains(s.id)).Select(e => new
                {
                    Year = e.academicYear,
                    Yearid = e.id
                }).ToList();

            ViewBag.Academic_Year = Data;

            return View(sss);
        }

        public class DateSelected
        {
            public string Selectdate { get; set; }
            public string Month { get; set; }
            public string Academic_YearId { get; set; }
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
            public string[] PhdRegNos { get; set; }
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

        public ActionResult Get_Months(int? YearId)
        {
            if (YearId != null)
            {
                if (YearId == 9)
                {
                    List<SelectListItem> Months = new List<SelectListItem>();
                    Months.Add(new SelectListItem() { Text = "August", Value = "8" });
                    Months.Add(new SelectListItem() { Text = "September", Value = "9" });
                    Months.Add(new SelectListItem() { Text = "October", Value = "10" });
                    Months.Add(new SelectListItem() { Text = "November", Value = "11" });
                    Months.Add(new SelectListItem() { Text = "December", Value = "12" });
                    return Json(new { Data = Months }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var Year = db.jntuh_academic_year.Where(s => s.id == YearId).Select(s => s.actualYear).FirstOrDefault();
                    int month = 01;
                    int day = 01;

                    List<SelectListItem> Months = new List<SelectListItem>();

                    DateTime now = new DateTime(Year, month, day);
                    var Curre = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(now.Month);
                    for (int i = now.Month; i < DateTime.Now.Month; i++)
                    {
                        var monthName = now.ToString("MMMM");
                        string CurrentMonth = now.Month.ToString();
                        Months.Add(new SelectListItem() { Text = monthName, Value = CurrentMonth });
                        now = now.AddMonths(1);
                    }
                    return Json(new { Data = Months }, JsonRequestBehavior.AllowGet);
                }

            }
            else
            {
                return RedirectToAction("Index");
            }
        }

        #region BASNotice Code Written by Siva

        #region BasNotice Parameters Written by Siva
        private string[] Non_HighestDegreeBasedRegNos;
        private int PHD_Count;
        private int Required_PHD_Count;
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

        public ActionResult DownloadFiles()
        {
            using (ZipFile zip = new ZipFile())
            {
                zip.AlternateEncodingUsage = ZipOption.AsNecessary;
                zip.AddDirectoryByName("Content");
                string[] filePaths = Directory.GetFiles(Server.MapPath("~/Content/PDFReports/BASNotice/FacultyMonthlyBAS/"));
                foreach (var row in filePaths)
                {
                    string filePath = row;
                    zip.AddFile(filePath);
                }
                Response.Clear();
                Response.BufferOutput = false;
                string zipName = String.Format("Zip_{0}.zip", DateTime.Now.ToString("yyyy-MMM-dd-HHmmss"));
                Response.ContentType = "application/zip";
                Response.AddHeader("content-disposition", "attachment; filename=" + zipName);
                zip.Save(Response.OutputStream);
                Response.End();
            }
            return View();
        }
        [Authorize(Roles = "Admin")]
        public ActionResult BASNotice(string id ,string type)
        {
            var CollegeIds = db.jntuh_college_edit_status.Where(a => a.academicyearId == academicYearIdDb && a.isSubmitted == true).Select(a => a.collegeId).ToList();
            //var CollegeIds = new int[]{ 27,263 };
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

            ZipFile zip = new ZipFile();
            zip.AlternateEncodingUsage = ZipOption.AsNecessary;
            zip.AddDirectoryByName("Faculty_" + type + "_BASReport");

            foreach (var item in CollegeIds)
            {
                string path = BASNoticeSave(item, type);
            }

            string[] filePaths = Directory.GetFiles(Server.MapPath("~/Content/PDFReports/BASNotice/Faculty/" + PresentYearDb + "/" + type +  "/"));

            foreach (var row in filePaths)
            {
                string filePath = row;
                zip.AddFile(filePath, "Faculty_" + type + "_BASReport");
            }

            Response.Clear();
            Response.BufferOutput = false;
            string zipName = String.Format(type + "BAS_{0}.zip", DateTime.Now.ToString("yyyy-MMM-dd-HHmmss"));
            Response.ContentType = "application/zip";
            Response.AddHeader("content-disposition", "attachment; filename=" + zipName);
            zip.Save(Response.OutputStream);
            Response.End();

            return View();
        }

        public string BASNoticeSave(int? collegeId, string type)
        {          
            jntuh_college CollegeData = db.jntuh_college.Where(c => c.id == collegeId).Select(c => c).FirstOrDefault();
            string collegeCode = CollegeData.collegeCode.ToUpper();

            string ECollegeid = UAAAS.Models.Utilities.EncryptString(collegeId.ToString(), System.Web.Configuration.WebConfigurationManager.AppSettings["CryptoKey"]);

            Document pdfDoc = new Document(PageSize.A4, 60, 50, 60, 60);

            // Create a new PdfWrite object, writing the output to a MemoryStream
            //open pdf
            var output = new MemoryStream();
            var writer = PdfWriter.GetInstance(pdfDoc, output);

            string path = Server.MapPath("~/Content/PDFReports/BASNotice/Faculty/" + PresentYearDb + "/" + type + "/");

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            string fullPath = path + CollegeData.collegeCode.ToUpper() + "-" + ECollegeid + "_" + type + "BASNotice.pdf";
            PdfWriter pdfWriter = PdfWriter.GetInstance(pdfDoc, new FileStream(fullPath, FileMode.Create));

            var file = CollegeData.collegeCode.ToUpper() + "-" + ECollegeid + "_" + type +"BASNotice.pdf";

            fullPath = string.Format("{0}/{1}", "/Content/PDFReports/BASNotice/Faculty/" + PresentYearDb + "/" + type + "/", file);

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

            contents = contents.Replace("##COLLEGE_CODE##", CollegeData.collegeCode.ToUpper());
            contents = contents.Replace("##month##", type);

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

            //Faculty List when DepartmentWise List
           // contents = contents.Replace("##COURSE_TABLE##", CollegeFaculty(collegeId, type));

            //Faculty List with required faculty normal List 
           // contents = contents.Replace("##COURSE_TABLE##", SurpriseStaffNew(collegeId, type));

            //Faculty List when normal List 
            contents = contents.Replace("##COURSE_TABLE##", CollegeFacultyList(collegeId, type));

            contents = contents.Replace("##Total_Faculty##", facultyCountnew.ToString());
            monthCount = monthCount == null ? 15 : monthCount;
            contents = contents.Replace(" ##DaysCount##", monthCount.ToString());
            var Difference = facultyCountnew - TotalCount1;
            //if (Difference > 0)
            //    contents = contents.Replace("##DifferenceCount##", Difference.ToString());
            //else
            //    contents = contents.Replace("##DifferenceCount##", "0");

            contents = contents.Replace("##DifferenceCount##", (facultyCountnew - TotalCount1).ToString());
          
            //Required Phd Count when DepartmentWise List 
           // contents = contents.Replace("##Total_PHD_Count##", Required_PHD_Count.ToString());

            //Required Phd Count when normal List 
            contents = contents.Replace("##Total_PHD_Count##", facultyCountnew1.ToString());
            
            if (facultyCountnew1 == 0)
            {
                facultyCountnew1 = 1;
            }
                    
            var parsedHtmlElements = HTMLWorker.ParseToList(new StringReader(contents), null);

            bool pageRotated = false;

            //Get each array values from parsed elements and add to the PDF document
            foreach (var htmlElement in parsedHtmlElements)
            {

                try
                {
                    if (htmlElement.Equals("<textarea>"))
                    {
                        pdfDoc.NewPage();
                    }

                    if (htmlElement.Chunks.Count >= 3)
                    {
                        if (htmlElement.Chunks.Count == 4)
                        {
                            pdfDoc.SetPageSize(iTextSharp.text.PageSize.A4.Rotate());
                            pdfDoc.SetMargins(50, 50, 60, 60);
                            pageRotated = true;
                        }
                        else
                        {
                            if (pageRotated == true)
                            {
                                pdfDoc.SetPageSize(iTextSharp.text.PageSize.A4);
                                pdfDoc.SetMargins(50, 50, 60, 60);
                                pageRotated = false;
                            }
                        }

                        pdfDoc.NewPage();

                    }
                    else
                    {
                        pdfDoc.Add((IElement)htmlElement);
                    }
                }
                catch (Exception ex)
                {
                    continue;

                }
                //if (htmlElement.Chunks.Count == 3)
                //{ pdfDoc.Add(Chunk.NEXTPAGE); }

                //pdfDoc.Add(htmlElement as IElement);
            }


            //Get each array values from parsed elements and add to the PDF document

            //foreach (var htmlElement in parsedHtmlElements)
            //{
            //    if (htmlElement.Chunks.Count == 3)
            //    { pdfDoc.Add(Chunk.NEXTPAGE); }

            //    pdfDoc.Add(htmlElement as IElement);
            //}

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

        //Display Department Wise Facultys Count....
        public string CollegeFaculty(int? collegeID, string type)
        {
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

            Required_PHD_Count = intakedetailsList.Where(i => i.shiftId == 1 && !strOtherDepartments.Contains(i.Department)).Select(i => i.SpecializationsphdFaculty).Sum();

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

            StaffDetails += "<table border='1' cellpadding='3' cellspacing='0' style='font-family: Times New Roman; font-size: 8px; font-weight: normal; background-color: #fff; margin: 0 auto;'>";
            StaffDetails += "<tr>";
            StaffDetails += "<td width='8%' style='text-align:center;'>S.No</td>";
            StaffDetails += "<td width='30%' style='text-align:left;'>Department</td>";
            StaffDetails += "<td width='30%' style='text-align:center;'>Required Faculty</td>";
            StaffDetails += "<td width='10%' style='text-align:center;'>Total Faculty</td>";
            StaffDetails += "<td width='26%' style='text-align:center;'>Number of faculty giving Biometric attendance regularly(for atleast 15 days in a month)</td>";
            StaffDetails += "<td width='26%' style='text-align:center;'>% of faculty not giving Biometric attendance regularly w.r.t required faculty</td>";

            StaffDetails += "</tr>";

            int Index = 1;

            foreach (var item in intakedetailsList.Where(a=>a.shiftId == 1).ToList())
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

                //if (type == "August")
                //{
                //    var August_attendance = Get_August(collegeID, type);
                //    LoginList = August_attendance.ToList();
                //    August_attendance.Clear();
                //}
                //else
                //{
                //    LoginList = jntuh_college_attendace.Where(a => a.AcademicYearId == AY1 && a.Month == type).ToList();
                //}
                LoginList = jntuh_college_attendace.Where(a => a.AcademicYearId == AY1 && a.Month == type).ToList();
                
                string[] RegNos = new string[] { "9211-150409-180107", "6317-150413-162713", "96150406-153506", "78150407-104506", "90150406-132723", "5264-150409-160644", "8518-150413-150717", "7995-150414-143317", "4687-150420-162710", "1549-150418-123933", "8891-150420-165037", "2375-150427-184049", "5612-170202-102750", "9234-160302-113847", "49150406-131709", "4261-160308-074435", "4171-150624-143950", "0707-150408-175902", "74150406-141818", "16150402-155455", "54150402-122257", "6348-150411-120653", "6058-150417-174615", "8105-150412-154535", "8558-160312-194806", "1116-160313-055503", "3205-160313-105704", "7748-160313-063954", "2328-150408-154626", "8922-150410-144623", "6400-150409-153507", "4241-150413-144757", "5387-150420-124115", "6442-150413-163535", "3141-160302-143355", "2624-170202-123226", "2778-150621-184256", "6002-150408-104417", "7601-150409-104220", "5724-150409-131120", "9549-150409-131219", "5480-150416-141938", "8460-150505-150705", "5325-150411-101220", "9134-150418-123351", "1235-170107-122241", "3995-150420-172501", "5157-151216-105135", "5993-170208-144156", "0827-160314-154038", "43150407-121703", "9675-170201-060349", "1761-170110-124328", "8780-170212-221004", "42150406-105659", "9741-150408-163934", "2195-150408-145520", "7984-160128-143658", "0756-160128-144719", "8629-160128-141146", "6328-160129-102154", "6339-160128-152920", "5003-150410-100751", "3111-150413-163611", "6889-150413-145248", "7968-170117-141626", "4390-150619-131209", "7037-150619-123808", "6090-150416-211652", "0650-150419-232914", "9644-150420-123408", "2204-150506-223734", "5007-160303-052952", "1221-160111-140608", "5323-160316-172042", "5758-150409-145824", "9690-150419-143601", "2845-160318-215132", "6585-150414-151718", "6518-160222-100511", "37150407-125255", "4939-150419-180848", "7509-150415-191610", "7038-150418-123607", "4375-150420-144936", "4925-160204-183924", "5271-160206-142514", "9379-150417-164707", "4561-160211-082340", "4517-150424-122333", "4503-150419-134140", "8391-150419-133206", "7549-150409-171452", "2289-150418-215024", "5999-150418-172740", "7521-150418-223714", "4740-150419-162142", "9374-150419-225506", "0752-150422-165906", "6713-150422-171112", "7419-150420-154436", "0059-150420-164303", "7867-160309-173829", "0152-160309-181208", "50150406-110631", "4769-150416-174812", "8005-150416-145941", "2516-150422-195357", "8508-150412-113146", "1003-170119-133836", "4904-170119-140309", "0588-170126-140848", "8291-150624-160237", "6023-151229-151344", "4534-150408-104328", "9794-160229-175017", "3909-151230-121957", "68150407-154601", "1147-150412-155421", "6851-170208-173310", "8532-170213-171543", "77150407-144017", "4281-150415-144925", "8238-161024-112302", "1949-150421-144423", "3666-150408-102957", "1854-150414-123210", "8628-150414-121943", "2834-150414-131904", "8474-150420-113501", "5314-150422-160253", "6998-150422-145056", "8739-150422-124734", "5241-150422-143019", "1831-150422-122747", "7170-150422-115651", "4386-150422-113334", "1097-150422-111446", "2563-150409-185844", "6483-150409-120156", "8889-150423-121239", "3174-150423-145633", "7141-150416-152448", "3831-150426-164813", "1364-150426-165916", "8420-150426-163755", "0778-150501-170330", "2771-150622-225307", "8776-150622-232358", "7503-150424-104100", "9155-150622-231402", "7447-150525-115526", "9731-150622-154029", "3217-150409-132413", "2351-160302-144825", "8336-160302-171055", "93150406-161145", "67150407-173554", "7618-150410-162949", "9986-150619-175911", "94150404-124125", "00150407-113102", "2180-150424-222555", "1126-150507-163545", "1808-150621-133359", "4266-150622-125140", "7977-150623-115417", "7690-150415-134357", "6734-150415-133015", "4696-150409-112735", "9742-150409-105139", "5207-150409-145858", "88150407-162910", "7306-150411-145154", "2980-150411-105638", "9381-150411-113008", "7210-150428-042100", "2316-150428-040056", "5018-170111-130147", "2421-170118-102956", "0676-150408-111501", "2469-160121-104311", "94150402-171218", "9492-160121-160214", "2681-150625-150854", "6076-160311-171551", "0661-160312-141716", "2280-150408-151134", "0208-150411-124221", "8076-150409-161147", "5204-150412-144814", "4443-150412-150351", "9010-150412-151234", "3264-150418-173255", "0922-150415-111243", "0852-150429-154729", "4149-150621-173825", "6620-150625-180248", "7810-161226-114059", "4815-160201-103423", "6809-150505-133845", "0067-150505-141355", "5661-150505-153407", "5246-150505-151341", "7132-150505-171425", "9033-150430-105308", "3057-150506-113420", "8389-150506-170835", "2511-150506-173242", "4009-150408-154747", "8440-150624-131011", "9393-160306-105928", "4283-160315-194106", "3404-170119-172558", "7762-170213-131706", "1475-150414-183509", "7134-160218-150219", "0414-150427-153301", "2067-150414-155501", "33150330-155256", "87150406-194332", "8362-150409-110622", "6180-150408-151453", "7098-150426-074226", "7386-150414-125915", "1466-170105-194422", "1997-170112-095532", "5841-170125-113934", "5612-170125-124150", "1782-150408-121508", "1781-150409-114620", "47150407-153633", "6498-150414-132024", "9077-150416-124631", "5914-150504-100508", "0940-150504-125126", "7835-150506-141716", "6244-150622-125503", "8145-150623-151823", "4830-150623-163945", "6188-150413-160703", "3295-151219-103214", "8352-150625-115421", "7697-150625-114147", "7437-150412-112238", "6903-150506-125905", "8943-150507-132216", "3855-150507-145302", "5572-150507-124125", "3430-150507-155250", "3059-150516-165055", "0336-150507-122950", "1400-150507-152048", "4765-170522-104257", "09150402-160427", "6421-151221-144330", "7885-160312-140130", "0052-160312-131632", "0127-160312-141609", "3725-160312-151706", "9069-160312-151048", "7922-160312-160244", "0682-160312-172016", "2685-160312-172344", "7093-160312-154220", "3195-160314-232954", "5912-150408-122341", "84150406-142457", "7913-150417-131457", "6808-150424-004516", "7368-150419-164942", "3038-150413-112451", "58150406-100658", "68150404-140248", "10150404-134548", "61150404-162737", "56150407-160735", "8021-150409-165612", "8901-150430-111830", "1584-150409-112324", "9049-150416-103205", "95150404-130909", "1083-150417-152920", "1809-150428-104045", "7477-150428-103245", "9865-150426-124940", "6104-150427-134207", "6315-160225-130444", "8441-150413-113304", "9811-150409-103922", "8964-150420-121821", "4723-160311-151729", "03150402-152426", "9373-150426-135654", "8422-150410-113036", "9552-150410-111820", "4713-150420-124126", "6087-170212-130603", "9966-170211-170427", "0319-150415-110441", "9914-150414-143319", "8688-150413-214801", "4046-150507-120439", "5688-160311-160356", "37150406-124138", "86150406-152725", "6080-160305-164900", "7098-160309-162246", "3941-160309-164350", "0502-170213-143616", "6329-170131-185402", "1338-150415-172930", "0443-150620-112146", "39150405-125954", "4439-150422-115107", "6970-150415-123739", "5363-150417-125713", "6048-160225-114522", "8477-160301-195215", "7934-160301-193415", "7595-150423-120919", "5464-150427-154315", "1054-160102-160709", "15150407-161016", "2585-150410-170055", "8344-150429-162201", "8833-150504-131131", "0184-150429-124639", "0767-150430-105420", "0799-150430-113108", "1784-150430-101515", "2900-150430-114213", "3100-150501-103154", "3253-150430-111321", "3477-150430-113036", "4437-150430-104224", "4756-150411-150337", "4861-150430-112525", "5200-150501-103751", "5107-150430-104415", "5360-150430-090404", "6453-150507-190621", "7074-150507-151336", "1705-170116-142649", "91150406-161756", "9383-160128-125153", "0376-150421-155232", "2043-160310-154642", "23150403-182551", "0220-150422-163321", "3271-150506-120301", "0831-160218-160442", "3138-160219-144523", "3570-160219-145952", "5730-160219-155537", "3603-160220-122613", "4448-160220-193243", "5148-160220-114832", "1924-160220-113523", "6796-160220-221344", "5301-160304-192359", "8226-160305-162320", "9581-160305-115025", "8823-160305-140040", "1429-150427-153420", "5607-150425-102421", "47150406-125841", "7217-160209-132527", "2719-150417-140130", "8501-150421-095920", "45150407-154450", "1067-150419-120449", "5555-160129-181224", "0855-160314-234426", "8195-160314-180632", "2658-160314-185351", "8213-160314-204848", "2521-160314-200255", "9616-160314-214031", "0244-160314-231118", "3845-160314-214240", "1475-160315-001332", "3897-160314-200045", "7710-160319-204556", "6424-160319-215612", "7341-160319-202433", "6897-160319-221313", "8988-160319-224714", "5086-150410-102356", "7920-150413-130841", "2861-150410-135151", "6794-160319-164923", "2163-160319-175204", "8260-160319-182908", "6607-160319-151901", "2549-160319-143150", "5086-160319-123814", "2359-160319-121236", "8319-160319-114531", "52150401-155113", "6429-150410-134158", "8471-150412-173932", "2546-150412-173119", "5430-150413-163549", "4595-150422-114349", "7960-150502-150257", "1556-150502-151010", "1620-150502-152356", "5563-150410-155500", "8462-150422-133254", "4469-150422-140349", "39150404-122803", "89150406-135317", "8733-160220-105912", "3280-160311-151200", "68150404-143104", "1620-150419-083055", "98150402-140537", "4240-150619-124624", "90150402-150047", "5077-150411-114936", "1735-160106-153643", "4305-160313-173609", "4707-160313-172938", "7992-160313-172334", "4292-160313-174602", "2969-160313-170725", "7697-160314-151405", "5247-160314-201702", "2991-160314-202954", "3831-170207-161803", "52150407-113402", "1185-150410-154249", "4181-150415-152826", "63150406-133241", "7082-160225-144210", "6833-150429-151901", "7084-150603-172353", "8010-150421-124203", "5725-150505-114121", "1394-150430-232728", "60150406-153912", "9700-150418-103531", "5407-150420-114851", "0261-150421-122215", "4944-150507-132508", "0878-160225-114919", "4393-150421-155815", "0967-160310-130928", "2599-150421-154955", "6087-160224-152646", "0546-160224-111407", "7909-160312-182449", "0487-160312-190833", "9358-170111-094327", "6379-170112-093250", "9888-150421-113110", "1166-150622-055114", "2952-150624-163859", "7893-150421-160431", "87150404-160809", "2876-150409-150305", "8578-150418-145228", "4050-150424-155306", "70150404-125108", "9618-150416-122424", "8941-150415-152633", "0959-170118-101333", "0774-150427-133052", "0785-160304-122835", "9791-150422-162245", "22150407-163124", "53150406-105254", "1528-160304-125739", "8238-161224-112812", "3531-150421-143805", "4278-150416-175043", "7787-150411-145051", "56150403-213949", "6604-150416-125428", "4857-150421-080322", "0023-160316-130354", "1472-160312-141517", "7735-150421-182347", "50150403-105221", "2813-150408-164256", "3538-150411-104526", "0560-150428-125331", "2763-160203-170600", "0170-160307-001639", "9304-160306-225725", "0171-160306-235900", "2124-160306-232603", "3439-160316-105705", "7980-160316-105741", "5725-150409-102427", "4711-150426-181413", "4892-150413-110154", "0895-150413-112139", "0093-150413-132750", "4698-160201-104955", "8540-150413-110353", "6074-150413-170744", "4574-150620-175558", "5645-150620-182613", "1693-150412-112919", "0158-150427-152718", "2008-150421-095104", "4272-160108-143318", "7884-150413-153150", "4800-150415-103452", "1136-160311-110044", "9656-170111-081909", "33150407-101427", "1266-150427-190250", "2093-170207-122958", "3507-150409-123348", "7541-150412-152809", "2825-150420-092811", "9437-150420-103610", "4992-150409-141335", "3498-150408-231802", "6166-150512-104107", "9768-150427-210434", "0205-150508-120436", "2637-150425-151316", "2513-150425-192936", "4188-150424-144352", "2456-150427-125655", "1660-150507-152429", "3570-150507-110913", "1601-150408-172051", "0148-150419-091522", "1442-161217-164741", "31150403-201030", "0941-170207-002441", "8575-170131-031553", "9965-170131-032636", "5691-170131-033654", "8645-170131-034806", "7939-170131-035846", "3351-170131-041646", "9837-170131-042413", "2633-170131-043329", "2964-170131-045303", "2440-170131-201700", "4894-170201-001106", "7664-160522-105414", "6997-170207-182304", "7197-170207-173901", "7397-160524-140443", "9865-160525-110126", "4556-160522-113614", "4469-150409-124255" };

                LoginList = LoginList.Where(l => !RegNos.Contains(l.RegistrationNo)).ToList();

                var PHD = LoginList.Where(l => New_PHD_Count1.Contains(l.RegistrationNo)).ToList();
               
                PHDPresentCount = PHD.Where(s => s.TotalPercentage >= monthCount).Count();
              

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

                    count++;
                    TotalCount += item.totalFaculty;
                    TotalCount1 += (phdpercentagecount + nonphdpercentagecount);
                    DifferenceCount += ((item.ugFaculty + item.pgFaculty + item.phdFaculty) - (phdpercentagecount + nonphdpercentagecount));
                }

               // string FullDepartment = item.Degree + "-" + item.Department + " " + item.Specialization;
                string FullDepartment = item.Degree + "-" + item.Specialization;
                StaffDetails += "<tr>";
                StaffDetails += "<td style='text-align:center;'>" + (Index++) + "</td>";
                StaffDetails += "<td style='text-align:left;'>" + FullDepartment + "</td>";
                StaffDetails += "<td style='text-align:center;'>" + item.requiredFaculty + "</td>";
                StaffDetails += "<td style='text-align:center;'>" + item.totalFaculty +"</td>";
                StaffDetails += "<td style='text-align:center;'>" + (phdpercentagecount + nonphdpercentagecount) +"</td>";
                StaffDetails += "<td style='text-align:center;'>" + (item.totalFaculty - (phdpercentagecount + nonphdpercentagecount)) +"</td>";
                StaffDetails += "</tr>";             
            }

            StaffDetails += "</table>";

            facultyCountnew1 = BTech_PHD_Count_New + BPharmacy_PHD_Count_New + MTech_PHD_Count_New + MPharmacy_PHD_Count_New + MBA_PHD_Count_New + 
                                    MCA_PHD_Count_New + MAM_PHD_Count_New;

          

            //    TotalCount = (TotalCount - intakedetailsList.Where(l => l.shiftId == 2).Select(l => l.totalFaculty).Sum());

            var New_intakedetails = intakedetailsList.Where(r => r.DegreeID != 0).GroupBy(r => r.DegreeID).ToList();

            var count_new = 1;
            foreach (var item2 in New_intakedetails)
            {
                var Degree_wise_ToltalCount = 0;
                var Degree_Wise_PHD_Count = 0;
                var Degree = string.Empty;
                var Facultyratio = Convert.ToInt32(jntuh_faculty_student_ratio_norms.Where(fn => fn.degreeId == item2.Key).Select(fn => fn.Norms).FirstOrDefault().Split(':')[1].ToString());
                dynamic TotalRequired_facultyCount = 0;
                if (item2.Key == 4)
                {
                    Degree = "B.Tech";
                    TotalRequired_facultyCount = (int)((Btech_Faculty_ProposedCount / Facultyratio) + intakedetailsList.Where(s => s.DegreeID == item2.Key).Select(s => s.requiredFaculty).Sum());
                    TotalRequired_facultyCount = TotalRequired_facultyCount - BTech_PHD_Count_New;
                    Degree_Wise_PHD_Count = BTech_PHD_Count_New;
                    Degree_wise_ToltalCount = TotalRequired_facultyCount + BTech_PHD_Count_New;
                }
                else if (item2.Key == 1)
                {
                    Degree = "M.Tech";
                    TotalRequired_facultyCount = (int)(intakedetailsList.Where(s => s.DegreeID == item2.Key && s.shiftId == 1).Select(s => s.requiredFaculty).Sum());
                    TotalRequired_facultyCount = TotalRequired_facultyCount - MTech_PHD_Count_New;
                    Degree_Wise_PHD_Count = MTech_PHD_Count_New;
                    Degree_wise_ToltalCount = TotalRequired_facultyCount + MTech_PHD_Count_New;
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
                    Degree_Wise_PHD_Count = BPharmacy_PHD_Count_New;

                    Degree_wise_ToltalCount = TotalRequired_facultyCount;

                }
                else if (item2.Key == 2)
                {
                    Degree = "M.Pharmacy";
                    TotalRequired_facultyCount = (int)(intakedetailsList.Where(s => s.DegreeID == item2.Key && s.shiftId == 1).Select(s => s.requiredFaculty).Sum());
                    TotalRequired_facultyCount = TotalRequired_facultyCount - MPharmacy_PHD_Count_New;
                    Degree_Wise_PHD_Count = MPharmacy_PHD_Count_New;
                    Degree_wise_ToltalCount = TotalRequired_facultyCount + MPharmacy_PHD_Count_New;
                }
                else if (item2.Key == 6)
                {
                    Degree = "MBA";
                    TotalRequired_facultyCount = (int)(intakedetailsList.Where(s => s.DegreeID == item2.Key).Select(s => s.requiredFaculty).Sum());
                    TotalRequired_facultyCount = TotalRequired_facultyCount - MBA_PHD_Count_New;
                    Degree_Wise_PHD_Count = MBA_PHD_Count_New;
                    Degree_wise_ToltalCount = TotalRequired_facultyCount + MBA_PHD_Count_New;
                }
                else if (item2.Key == 3)
                {
                    Degree = "MCA";
                    TotalRequired_facultyCount = (int)(intakedetailsList.Where(s => s.DegreeID == item2.Key).Select(s => s.requiredFaculty).Sum());
                    TotalRequired_facultyCount = TotalRequired_facultyCount - MCA_PHD_Count_New;
                    Degree_Wise_PHD_Count = MCA_PHD_Count_New;
                    Degree_wise_ToltalCount = TotalRequired_facultyCount + MCA_PHD_Count_New;
                }

                else if (item2.Key == 7)
                {
                    Degree = "MAM";
                    TotalRequired_facultyCount = (int)(intakedetailsList.Where(s => s.DegreeID == item2.Key).Select(s => s.requiredFaculty).Sum());
                    TotalRequired_facultyCount = TotalRequired_facultyCount - MAM_PHD_Count_New;
                    Degree_Wise_PHD_Count = MAM_PHD_Count_New;
                    Degree_wise_ToltalCount = TotalRequired_facultyCount + MAM_PHD_Count_New;
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
                    facultyCountnew1 += Degree_Wise_PHD_Count;
                    facultyCountnew2 += TotalRequired_facultyCount;
                    facultyCountnew += Degree_wise_ToltalCount;                  
                }

            }
            //    obj.Monthname = item5;
            //    obj.UploadedCount = TotalCount;
            //    obj.BiometricCount = TotalCount1;
            //    obj.BASDifferenceCount = DifferenceCount;
            //    obj.RequiredCount = facultyCountnew;
            //    obj.RequiredCountPercentage = Math.Round(((((double)facultyCountnew - (double)TotalCount1) / (double)facultyCountnew) * 100), 2);
            //    obj.RequiredPHDCount = facultyCountnew1 == 0 ? 1 : facultyCountnew1;
            //    obj.PHDPresentCount = PHDPresentCount;
            //    obj.PHDCountPercentage = Math.Round(((((double)obj.RequiredPHDCount - (double)PHDPresentCount) / (double)obj.RequiredPHDCount) * 100), 2);
            //    obj.RequiredDifferenceCount = facultyCountnew - TotalCount1;

            //    facultylist.Add(obj);
            

            //StaffDetails += "<table border='1' cellpadding='3' cellspacing='0' style='font-family: Times New Roman; font-size: 8px; font-weight: normal; background-color: #fff; margin: 0 auto;'>";
            //StaffDetails += "<tr>";
            //StaffDetails += "<td width:'2' style='text-align:center;'>S.No</td>";
            //StaffDetails += "<td width:'2' style='text-align:center;'>Month</td>";
            //StaffDetails += "<td width:'3' style='text-align:center;'>Number of faculty giving Biometric attendance regularly(for atleast 15 days in a month)</td>";
            //StaffDetails += "<td width:'3' style='text-align:center;'>% of faculty not giving Biometric attendance regularly w.r.t required faculty</td>";

            //StaffDetails += "</tr>";

            //int sno = 1;
            //double minPercentage = (double)0;
            //var per = facultylist.Where(f => f.RequiredCountPercentage >= minPercentage).Count();

            //foreach (var item in facultylist)
            //{

            //    StaffDetails += "<tr>";
            //    StaffDetails += "<td style='text-align:center;'>" + sno + "</td>";
            //    StaffDetails += "<td style='text-align:center;'>" + item.Monthname + ",2018</td>";
            //    StaffDetails += "<td style='text-align:center;'>" + item.BiometricCount + "</td>";
            //    if (item.RequiredCountPercentage >= minPercentage)
            //        StaffDetails += "<td style='text-align:center;'>" + item.RequiredCountPercentage + "</td>";
            //    else
            //    {
            //        StaffDetails += "<td style='text-align:center;'>0</td>";
            //    }
            //    StaffDetails += "</tr>";
            //    sno++;
            //}

            //StaffDetails += "</table>";
            return StaffDetails;

        }

        //Display Faculty List....
        public string CollegeFacultyList(int? CollegeId, string type)
        {
            string Content = string.Empty;
            int Count = 1;
            decimal cutoffpercentage = 65;
            if (CollegeId != null)
            {
                var jntuh_college_basreport = db.jntuh_college_basreport.Where(s => s.collegeId == CollegeId && s.year == PresentYearDb && s.month == type && s.type == 1).Select(e => e).ToList();
                var strRegnos = jntuh_college_basreport.Select(s => s.RegistrationNumber).ToList();
                var jntuh_college = db.jntuh_college.Where(a => a.id == CollegeId).FirstOrDefault();
                var jntuh_department = db.jntuh_department.ToList();
                var jntuh_registered_faculty = db.jntuh_registered_faculty.Where(s => strRegnos.Contains(s.RegistrationNumber)).Select(s => new { FirstName = s.FirstName, MiddleName = s.MiddleName, LastName = s.LastName, RegistrationNumber = s.RegistrationNumber }).ToList();
                List<FacultyMonthlyPresentDays> list = new List<FacultyMonthlyPresentDays>();

                string[] strOtherDepartments = { "English", "Mathematics", "Physics", "Chemistry", "Others(CSE/IT)", "Others(CIVIL/MECH)", "Others(ECE/EEE)", "Others(MNGT/H&S)" };

                foreach (var item in jntuh_college_basreport)
                {
                    FacultyMonthlyPresentDays student = new FacultyMonthlyPresentDays();
                    student.CollegeId = item.collegeId;
                    student.CollegeCode = jntuh_college.collegeCode;
                    student.CollegeName = jntuh_college.collegeName;
                    student.RegNo = item.RegistrationNumber;
                    student.Name = item.name;
                    student.Dept = jntuh_department.Where(a => a.id == item.departmentId).Select(e => e.departmentName).FirstOrDefault();
                    student.WorkingDays = item.totalworkingDays;
                    student.HoliDays = item.NoofHolidays;
                    student.PrsentDays = item.NoofPresentDays;
                    student.Percentage = Math.Round((((decimal)item.NoofPresentDays / (decimal)item.totalworkingDays) * 100), 2);
                    list.Add(student); 
                }

                var abovesixtypercentage = list.Where(q => q.Percentage >= cutoffpercentage).Select(z => z).ToList();
                var belowsixtypercentage = list.Where(q => q.Percentage < cutoffpercentage).Select(z => z).ToList();

                var abovesixtypercentageHumanitiesDepartments = abovesixtypercentage.Where(a => strOtherDepartments.Contains(a.Dept)).Select(s => s).ToList();
                var belowsixtypercentageHumanitiesDepartments = belowsixtypercentage.Where(a => strOtherDepartments.Contains(a.Dept)).Select(s => s).ToList();

                abovesixtypercentage = abovesixtypercentage.Where(a => !strOtherDepartments.Contains(a.Dept)).Select(s => s).ToList();
                belowsixtypercentage = belowsixtypercentage.Where(a => !strOtherDepartments.Contains(a.Dept)).Select(s => s).ToList();

                var  HumanitiesDepartments = list.Where(a => strOtherDepartments.Contains(a.Dept)).Select(s => s).ToList();

                list = list.Where(a => !strOtherDepartments.Contains(a.Dept)).Select(s => s).ToList();

                Content += "<p><strong>I. Faculty members possessing biometric attendance percentage less than 65 is tabulated below:</strong></p><br/>";
                Content += "<table border='1' cellspacing='0' cellpadding='3' style='font-size: 9px;'>";
                Content += "<tbody>";
                Content += "<tr style='font-weight:bold;'>";
                Content += "<td width='4%'><p align='center'>S.No</p></td>";
                Content += "<td width='15%'><p align='center'>RegistrationNumber</p></td>";
                Content += "<td width='15%'><p align='center'>Name</p></td>";
                Content += "<td width='12%'><p align='center'>Department</p></td>";
                Content += "<td width='8%'><p align='center'>Total Working Days</p></td>";
                Content += "<td width='8%'><p align='center'>Holidays Including Sundays</p></td>";
                Content += "<td width='8%'><p align='center'>Present days</p></td>";
                Content += "<td width='8%'><p align='center'>% of Presence</p></td>";
                Content += "</tr>";

                foreach (var item in belowsixtypercentage.OrderBy(w => w.Dept).ToList())
                {
                    Content += "<tr>";
                    Content += "<td width='4%' style='font-size: 10px;'><p align='center'>" + (Count++) + "</p></td>";
                    Content += "<td width='15%' style='font-size: 10px;'><p align='left'>" + item.RegNo + "</p></td>";
                    Content += "<td width='15%' style='font-size: 10px;'><p align='left'>" + item.Name + "</p></td>";
                    Content += "<td width='12%' style='font-size: 10px;'><p align='left'>" + item.Dept + "</p></td>";
                    Content += "<td width='8%' style='font-size: 10px;'><p align='right'>" + item.WorkingDays + "</p></td>";
                    Content += "<td width='8%' style='font-size: 10px;'><p align='right'>" + item.HoliDays + "</p></td>";
                    Content += "<td width='8%' style='font-size: 10px;'><p align='right'>" + item.PrsentDays + "</p></td>";
                    Content += "<td width='8%' style='font-size: 10px;'><p align='right'>" + item.Percentage + "</p></td>";
                    Content += "</tr>";
                }

                foreach (var item in belowsixtypercentageHumanitiesDepartments.OrderBy(w => w.Dept).ToList())
                {
                    Content += "<tr>";
                    Content += "<td width='4%' style='font-size: 10px;'><p align='center'>" + (Count++) + "</p></td>";
                    Content += "<td width='15%' style='font-size: 10px;'><p align='left'>" + item.RegNo + "</p></td>";
                    Content += "<td width='15%' style='font-size: 10px;'><p align='left'>" + item.Name + "</p></td>";
                    Content += "<td width='12%' style='font-size: 10px;'><p align='left'>" + item.Dept + "</p></td>";
                    Content += "<td width='8%' style='font-size: 10px;'><p align='right'>" + item.WorkingDays + "</p></td>";
                    Content += "<td width='8%' style='font-size: 10px;'><p align='right'>" + item.HoliDays + "</p></td>";
                    Content += "<td width='8%' style='font-size: 10px;'><p align='right'>" + item.PrsentDays + "</p></td>";
                    Content += "<td width='8%' style='font-size: 10px;'><p align='right'>" + item.Percentage + "</p></td>";
                    Content += "</tr>";
                }

                Content += "</tbody>";
                Content += "</table>";
                Content += "<br/>";
                Content += "<p>The above mentioned faculty members working in your college have got less attendance. This information is only to alert the college about the faculty members with attendance less than 65%. However, this is not a benchmark or a norm and the biometric attendance of the faculty members throughout the academic year 2019-20 will be reviewed and considered separately while granting affiliation for the ensuing A.Y 2020-21.</p><br/>";

                Content += "<p><strong>II. Faculty members possessing biometric attendance percentage equal to or greater than 65 is tabulated below:</strong></p><br/>";
                Content += "<table border='1' cellspacing='0' cellpadding='3' style='font-size: 9px;'>";
                Content += "<tbody>";
                Content += "<tr style='font-weight:bold;'>";
                Content += "<td width='4%'><p align='center'>S.No</p></td>";
                Content += "<td width='15%'><p align='center'>RegistrationNumber</p></td>";
                Content += "<td width='15%'><p align='center'>Name</p></td>";
                Content += "<td width='12%'><p align='center'>Department</p></td>";
                Content += "<td width='8%'><p align='center'>Total Working Days</p></td>";
                Content += "<td width='8%'><p align='center'>Holidays Including Sundays</p></td>";
                Content += "<td width='8%'><p align='center'>Present days</p></td>";
                Content += "<td width='8%'><p align='center'>% of Presence</p></td>";
                Content += "</tr>";

                foreach (var item in abovesixtypercentage.OrderBy(w => w.Dept).ToList())
                {
                    Content += "<tr>";
                    Content += "<td width='4%' style='font-size: 10px;'><p align='center'>" + (Count++) + "</p></td>";
                    Content += "<td width='15%' style='font-size: 10px;'><p align='left'>" + item.RegNo + "</p></td>";
                    Content += "<td width='15%' style='font-size: 10px;'><p align='left'>" + item.Name + "</p></td>";
                    Content += "<td width='12%' style='font-size: 10px;'><p align='left'>" + item.Dept + "</p></td>";
                    Content += "<td width='8%' style='font-size: 10px;'><p align='right'>" + item.WorkingDays + "</p></td>";
                    Content += "<td width='8%' style='font-size: 10px;'><p align='right'>" + item.HoliDays + "</p></td>";
                    Content += "<td width='8%' style='font-size: 10px;'><p align='right'>" + item.PrsentDays + "</p></td>";
                    Content += "<td width='8%' style='font-size: 10px;'><p align='right'>" + item.Percentage + "</p></td>";
                    Content += "</tr>";
                }

                foreach (var item in abovesixtypercentageHumanitiesDepartments.OrderBy(w => w.Dept).ToList())
                {
                    Content += "<tr>";
                    Content += "<td width='4%' style='font-size: 10px;'><p align='center'>" + (Count++) + "</p></td>";
                    Content += "<td width='15%' style='font-size: 10px;'><p align='left'>" + item.RegNo + "</p></td>";
                    Content += "<td width='15%' style='font-size: 10px;'><p align='left'>" + item.Name + "</p></td>";
                    Content += "<td width='12%' style='font-size: 10px;'><p align='left'>" + item.Dept + "</p></td>";
                    Content += "<td width='8%' style='font-size: 10px;'><p align='right'>" + item.WorkingDays + "</p></td>";
                    Content += "<td width='8%' style='font-size: 10px;'><p align='right'>" + item.HoliDays + "</p></td>";
                    Content += "<td width='8%' style='font-size: 10px;'><p align='right'>" + item.PrsentDays + "</p></td>";
                    Content += "<td width='8%' style='font-size: 10px;'><p align='right'>" + item.Percentage + "</p></td>";
                    Content += "</tr>";
                }

                Content += "</tbody>";
                Content += "</table>";

                //foreach (var item in list.OrderBy(w => w.Dept).ToList())
                //{
                //    Content += "<tr>";
                //    Content += "<td width='4%' style='font-size: 10px;'><p align='center'>" + (Count++) + "</p></td>";
                //    Content += "<td width='15%' style='font-size: 10px;'><p align='left'>" + item.RegNo + "</p></td>";
                //    Content += "<td width='15%' style='font-size: 10px;'><p align='left'>" + item.Name + "</p></td>";
                //    Content += "<td width='12%' style='font-size: 10px;'><p align='left'>" + item.Dept + "</p></td>";
                //    Content += "<td width='8%' style='font-size: 10px;'><p align='right'>" + item.WorkingDays + "</p></td>";
                //    Content += "<td width='8%' style='font-size: 10px;'><p align='right'>" + item.HoliDays + "</p></td>";
                //    Content += "<td width='8%' style='font-size: 10px;'><p align='right'>" + item.PrsentDays + "</p></td>";

                //    decimal percentage = ((decimal)item.PrsentDays / (decimal)item.WorkingDays) * 100;

                //    Content += "<td width='8%' style='font-size: 10px;'><p align='right'>" + Math.Round(percentage, 2) + "</p></td>";
                //    Content += "</tr>";
                //}

                //foreach (var item in HumanitiesDepartments.OrderBy(w => w.Dept).ToList())
                //{
                //    Content += "<tr>";
                //    Content += "<td width='4%' style='font-size: 10px;'><p align='center'>" + (Count++) + "</p></td>";
                //    Content += "<td width='15%' style='font-size: 10px;'><p align='left'>" + item.RegNo + "</p></td>";
                //    Content += "<td width='15%' style='font-size: 10px;'><p align='left'>" + item.Name + "</p></td>";
                //    Content += "<td width='12%' style='font-size: 10px;'><p align='left'>" + item.Dept + "</p></td>";
                //    Content += "<td width='8%' style='font-size: 10px;'><p align='right'>" + item.WorkingDays + "</p></td>";
                //    Content += "<td width='8%' style='font-size: 10px;'><p align='right'>" + item.HoliDays + "</p></td>";
                //    Content += "<td width='8%' style='font-size: 10px;'><p align='right'>" + item.PrsentDays + "</p></td>";

                //    decimal percentage = ((decimal)item.PrsentDays / (decimal)item.WorkingDays) * 100;

                //    Content += "<td width='8%' style='font-size: 10px;'><p align='right'>" + Math.Round(percentage, 2) + "</p></td>";
                //    Content += "</tr>";
                //}

                //Content += "</tbody>";
                //Content += "</table>";

                return Content;

            }
            return Content;
        }

        //Display Overall Faculty Count Attendance Greater than or Equal to 15 days....
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
           // presentYear = presentYear - 1;
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
                intakedetails.approvedIntake1 = GetIntake(item.collegeId, AY1, item.specializationId, item.shiftId, 1, item.degreeID);
                intakedetails.approvedIntake2 = GetIntake(item.collegeId, AY2, item.specializationId, item.shiftId, 1, item.degreeID);
                intakedetails.approvedIntake3 = GetIntake(item.collegeId, AY3, item.specializationId, item.shiftId, 1, item.degreeID);
                intakedetails.approvedIntake4 = GetIntake(item.collegeId, AY4, item.specializationId, item.shiftId, 1, item.degreeID);

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
                DeptId = jntuh_college_faculty_registered.Where(c => c.RegistrationNumber.Trim() == rf.RegistrationNumber.Trim()).Select(c => c.DepartmentId).FirstOrDefault(),
                SpecializationId = jntuh_college_faculty_registered.Where(c => c.RegistrationNumber.Trim() == rf.RegistrationNumber.Trim()).Select(c => c.SpecializationId).FirstOrDefault(),
                HighestDegreeID = rf.jntuh_registered_faculty_education.Count() != 0 ? rf.jntuh_registered_faculty_education.Select(e => e.educationId).Max() : 0,
                IsApproved = rf.isApproved,
                PanNumber = rf.PANNumber,
                AadhaarNumber = rf.AadhaarNumber,
                NoForm16 = rf.NoForm16,
                TotalExperience = rf.TotalExperience
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
                    if (item.shiftId == 1)
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
                        pgFaculty = jntuh_registered_faculty.Count(f => ("PG" == f.HighestDegree || "M.Phil / Other PG Degree" == f.HighestDegree) &&
                                    f.Department == "M.Pharmacy" && f.SpecializationId == item.specializationId);
                        phdFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && f.Department == "M.Pharmacy" && f.SpecializationId == item.specializationId);
                        intakedetails.NonPhdRegNos = jntuh_registered_faculty.Where(f => ("PG" == f.HighestDegree || "M.Phil / Other PG Degree" == f.HighestDegree) &&
                                    f.Department == "M.Pharmacy" && f.SpecializationId == item.specializationId).Select(r => r.RegistrationNumber).ToArray();
                        intakedetails.PhdRegNos = jntuh_registered_faculty.Where(f => ("Ph.D" == f.HighestDegree) && f.Department == "M.Pharmacy" && f.SpecializationId == item.specializationId).Select(r => r.RegistrationNumber).ToArray();
                    }
                    else if (item.Degree == "Pharm.D")
                    {
                        ugFaculty = jntuh_registered_faculty.Count(f => ("UG" == f.HighestDegree) && f.Department == "Pharm.D");
                        pgFaculty = jntuh_registered_faculty.Count(f => ("PG" == f.HighestDegree || "M.Phil / Other PG Degree" == f.HighestDegree) && f.Department == "Pharm.D");
                        phdFaculty = jntuh_registered_faculty.Count(f => "Ph.D" == f.HighestDegree && f.Department == "Pharm.D");
                        intakedetails.NonPhdRegNos = jntuh_registered_faculty.Where(f => ("PG" == f.HighestDegree || "M.Phil / Other PG Degree" == f.HighestDegree) && f.Department == "Pharm.D").Select(r => r.RegistrationNumber).ToArray();
                        intakedetails.PhdRegNos = jntuh_registered_faculty.Where(f => "Ph.D" == f.HighestDegree && f.Department == "Pharm.D").Select(r => r.RegistrationNumber).ToArray();
                        intakedetails.Department = "Pharm.D";
                    }
                    else if (item.Degree == "Pharm.D PB")
                    {

                        ugFaculty = jntuh_registered_faculty.Count(f => ("UG" == f.HighestDegree) && f.Department == "Pharm.D PB");
                        pgFaculty = jntuh_registered_faculty.Count(f => ("PG" == f.HighestDegree || "M.Phil / Other PG Degree" == f.HighestDegree) && f.Department == "Pharm.D PB");
                        phdFaculty = jntuh_registered_faculty.Count(f => "Ph.D" == f.HighestDegree && f.Department == "Pharm.D PB");
                        intakedetails.NonPhdRegNos = jntuh_registered_faculty.Where(f => ("PG" == f.HighestDegree || "M.Phil / Other PG Degree" == f.HighestDegree) && f.Department == "Pharm.D PB").Select(r => r.RegistrationNumber).ToArray();
                        intakedetails.PhdRegNos = jntuh_registered_faculty.Where(f => "Ph.D" == f.HighestDegree && f.Department == "Pharm.D PB").Select(r => r.RegistrationNumber).ToArray();
                        intakedetails.Department = "Pharm.D PB";
                    }

                    else if (item.Degree == "B.Tech")
                    {
                        var sss = jntuh_registered_faculty.Where(f => f.DepartmentId == item.DepartmentID).ToList();
                        ugFaculty = jntuh_registered_faculty.Count(f => ("UG" == f.HighestDegree) && f.DepartmentId == item.DepartmentID);
                        pgFaculty = jntuh_registered_faculty.Count(f => ("PG" == f.HighestDegree || "M.Phil / Other PG Degree" == f.HighestDegree) && f.DepartmentId == item.DepartmentID);
                        phdFaculty = jntuh_registered_faculty.Count(f => "Ph.D" == f.HighestDegree && f.DepartmentId == item.DepartmentID);
                        intakedetails.NonPhdRegNos = jntuh_registered_faculty.Where(f => ("PG" == f.HighestDegree || "M.Phil / Other PG Degree" == f.HighestDegree) && f.DepartmentId == item.DepartmentID).Select(r => r.RegistrationNumber).ToArray();
                        intakedetails.PhdRegNos = jntuh_registered_faculty.Where(f => "Ph.D" == f.HighestDegree && f.DepartmentId == item.DepartmentID).Select(r => r.RegistrationNumber).ToArray();
                    }
                    else if (item.Degree == "M.Tech")
                    {

                        ugFaculty = jntuh_registered_faculty.Count(f => ("UG" == f.HighestDegree) && f.SpecializationId == item.specializationId && f.DepartmentId == item.DepartmentID);
                        pgFaculty = jntuh_registered_faculty.Count(f => ("PG" == f.HighestDegree || "M.Phil / Other PG Degree" == f.HighestDegree) && f.SpecializationId == item.specializationId && f.DepartmentId == item.DepartmentID);
                        phdFaculty = jntuh_registered_faculty.Count(f => "Ph.D" == f.HighestDegree && f.SpecializationId == item.specializationId && f.DepartmentId == item.DepartmentID);
                        intakedetails.NonPhdRegNos = jntuh_registered_faculty.Where(f => ("PG" == f.HighestDegree || "M.Phil / Other PG Degree" == f.HighestDegree) && f.SpecializationId == item.specializationId && f.DepartmentId == item.DepartmentID).Select(r => r.RegistrationNumber).ToArray();
                        intakedetails.PhdRegNos = jntuh_registered_faculty.Where(f => "Ph.D" == f.HighestDegree && f.SpecializationId == item.specializationId && f.DepartmentId == item.DepartmentID).Select(r => r.RegistrationNumber).ToArray();
                    }
                    else
                    {

                        ugFaculty = jntuh_registered_faculty.Count(f => ("UG" == f.HighestDegree) && f.Department == item.Department);
                        pgFaculty = jntuh_registered_faculty.Count(f => ("PG" == f.HighestDegree || "M.Phil / Other PG Degree" == f.HighestDegree) && f.Department == item.Department);
                        phdFaculty = jntuh_registered_faculty.Count(f => "Ph.D" == f.HighestDegree && f.Department == item.Department);
                        intakedetails.NonPhdRegNos = jntuh_registered_faculty.Where(f => ("PG" == f.HighestDegree || "M.Phil / Other PG Degree" == f.HighestDegree) && f.Department == item.Department).Select(r => r.RegistrationNumber).ToArray();
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
                    int pgFaculty = jntuh_registered_faculty.Count(f => ("PG" == f.HighestDegree || "M.Phil / Other PG Degree" == f.HighestDegree) && f.Department == department);
                    var pgreg = jntuh_registered_faculty.Where(f => ("PG" == f.HighestDegree || "M.Phil / Other PG Degree" == f.HighestDegree) && f.Department == department).Select(e => e.RegistrationNumber).ToList();
                    int phdFaculty = jntuh_registered_faculty.Count(f => "Ph.D" == f.HighestDegree && f.Department == department);
                    string[] RegNos = jntuh_registered_faculty.Where(f => "Ph.D" == f.HighestDegree && f.Department == department).Select(e => e.RegistrationNumber).ToArray();
                    string[] NonRegNos = jntuh_registered_faculty.Where(f => ("PG" == f.HighestDegree || "M.Phil / Other PG Degree" == f.HighestDegree) && f.Department == department).Select(e => e.RegistrationNumber).ToArray();
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

        private int GetIntake(int collegeId, int academicYearId, int specializationId, int shiftId, int flag, int DegreeId)
        {
            int intake = 0;
            int[] IntegratedCollegeIds = { 9, 11, 26, 32, 38, 68, 70, 108, 109, 134, 171, 179, 180, 183, 192, 196, 198, 335, 367, 374, 399 };

            // Degree B.Tech  
            if (DegreeId == 4)
            {
                //admitted
                if (flag == 1 && (academicYearId == 9 || academicYearId == 8 || academicYearId == 3))
                {
                    intake = db.jntuh_college_intake_existing.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == shiftId).Select(i => i.admittedIntake).FirstOrDefault();
                }
                else if (flag == 1 && academicYearId == 10)
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

            var requiredFacultyCount = intakedetailsList.Where(d=> !strOtherDepartments.Contains(d.Department)).Select(d => d.requiredFaculty).Sum();

            var requiredFacultyCourses = intakedetailsList.Where(d => !strOtherDepartments.Contains(d.Department)).Select(d => d).ToList();

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
               
                LoginList = jntuh_college_attendace.Where(a => a.AcademicYearId == AY1 && a.Month == item5).ToList();

                //  LoginList = db.jntuh_college_attendace.Where(a => a.Collegeid == collegeID && a.Month == item5).ToList();

                string[] RegNos = new string[] { "9211-150409-180107", "6317-150413-162713", "96150406-153506", "78150407-104506", "90150406-132723", "5264-150409-160644", "8518-150413-150717", "7995-150414-143317", "4687-150420-162710", "1549-150418-123933", "8891-150420-165037", "2375-150427-184049", "5612-170202-102750", "9234-160302-113847", "49150406-131709", "4261-160308-074435", "4171-150624-143950", "0707-150408-175902", "74150406-141818", "16150402-155455", "54150402-122257", "6348-150411-120653", "6058-150417-174615", "8105-150412-154535", "8558-160312-194806", "1116-160313-055503", "3205-160313-105704", "7748-160313-063954", "2328-150408-154626", "8922-150410-144623", "6400-150409-153507", "4241-150413-144757", "5387-150420-124115", "6442-150413-163535", "3141-160302-143355", "2624-170202-123226", "2778-150621-184256", "6002-150408-104417", "7601-150409-104220", "5724-150409-131120", "9549-150409-131219", "5480-150416-141938", "8460-150505-150705", "5325-150411-101220", "9134-150418-123351", "1235-170107-122241", "3995-150420-172501", "5157-151216-105135", "5993-170208-144156", "0827-160314-154038", "43150407-121703", "9675-170201-060349", "1761-170110-124328", "8780-170212-221004", "42150406-105659", "9741-150408-163934", "2195-150408-145520", "7984-160128-143658", "0756-160128-144719", "8629-160128-141146", "6328-160129-102154", "6339-160128-152920", "5003-150410-100751", "3111-150413-163611", "6889-150413-145248", "7968-170117-141626", "4390-150619-131209", "7037-150619-123808", "6090-150416-211652", "0650-150419-232914", "9644-150420-123408", "2204-150506-223734", "5007-160303-052952", "1221-160111-140608", "5323-160316-172042", "5758-150409-145824", "9690-150419-143601", "2845-160318-215132", "6585-150414-151718", "6518-160222-100511", "37150407-125255", "4939-150419-180848", "7509-150415-191610", "7038-150418-123607", "4375-150420-144936", "4925-160204-183924", "5271-160206-142514", "9379-150417-164707", "4561-160211-082340", "4517-150424-122333", "4503-150419-134140", "8391-150419-133206", "7549-150409-171452", "2289-150418-215024", "5999-150418-172740", "7521-150418-223714", "4740-150419-162142", "9374-150419-225506", "0752-150422-165906", "6713-150422-171112", "7419-150420-154436", "0059-150420-164303", "7867-160309-173829", "0152-160309-181208", "50150406-110631", "4769-150416-174812", "8005-150416-145941", "2516-150422-195357", "8508-150412-113146", "1003-170119-133836", "4904-170119-140309", "0588-170126-140848", "8291-150624-160237", "6023-151229-151344", "4534-150408-104328", "9794-160229-175017", "3909-151230-121957", "68150407-154601", "1147-150412-155421", "6851-170208-173310", "8532-170213-171543", "77150407-144017", "4281-150415-144925", "8238-161024-112302", "1949-150421-144423", "3666-150408-102957", "1854-150414-123210", "8628-150414-121943", "2834-150414-131904", "8474-150420-113501", "5314-150422-160253", "6998-150422-145056", "8739-150422-124734", "5241-150422-143019", "1831-150422-122747", "7170-150422-115651", "4386-150422-113334", "1097-150422-111446", "2563-150409-185844", "6483-150409-120156", "8889-150423-121239", "3174-150423-145633", "7141-150416-152448", "3831-150426-164813", "1364-150426-165916", "8420-150426-163755", "0778-150501-170330", "2771-150622-225307", "8776-150622-232358", "7503-150424-104100", "9155-150622-231402", "7447-150525-115526", "9731-150622-154029", "3217-150409-132413", "2351-160302-144825", "8336-160302-171055", "93150406-161145", "67150407-173554", "7618-150410-162949", "9986-150619-175911", "94150404-124125", "00150407-113102", "2180-150424-222555", "1126-150507-163545", "1808-150621-133359", "4266-150622-125140", "7977-150623-115417", "7690-150415-134357", "6734-150415-133015", "4696-150409-112735", "9742-150409-105139", "5207-150409-145858", "88150407-162910", "7306-150411-145154", "2980-150411-105638", "9381-150411-113008", "7210-150428-042100", "2316-150428-040056", "5018-170111-130147", "2421-170118-102956", "0676-150408-111501", "2469-160121-104311", "94150402-171218", "9492-160121-160214", "2681-150625-150854", "6076-160311-171551", "0661-160312-141716", "2280-150408-151134", "0208-150411-124221", "8076-150409-161147", "5204-150412-144814", "4443-150412-150351", "9010-150412-151234", "3264-150418-173255", "0922-150415-111243", "0852-150429-154729", "4149-150621-173825", "6620-150625-180248", "7810-161226-114059", "4815-160201-103423", "6809-150505-133845", "0067-150505-141355", "5661-150505-153407", "5246-150505-151341", "7132-150505-171425", "9033-150430-105308", "3057-150506-113420", "8389-150506-170835", "2511-150506-173242", "4009-150408-154747", "8440-150624-131011", "9393-160306-105928", "4283-160315-194106", "3404-170119-172558", "7762-170213-131706", "1475-150414-183509", "7134-160218-150219", "0414-150427-153301", "2067-150414-155501", "33150330-155256", "87150406-194332", "8362-150409-110622", "6180-150408-151453", "7098-150426-074226", "7386-150414-125915", "1466-170105-194422", "1997-170112-095532", "5841-170125-113934", "5612-170125-124150", "1782-150408-121508", "1781-150409-114620", "47150407-153633", "6498-150414-132024", "9077-150416-124631", "5914-150504-100508", "0940-150504-125126", "7835-150506-141716", "6244-150622-125503", "8145-150623-151823", "4830-150623-163945", "6188-150413-160703", "3295-151219-103214", "8352-150625-115421", "7697-150625-114147", "7437-150412-112238", "6903-150506-125905", "8943-150507-132216", "3855-150507-145302", "5572-150507-124125", "3430-150507-155250", "3059-150516-165055", "0336-150507-122950", "1400-150507-152048", "4765-170522-104257", "09150402-160427", "6421-151221-144330", "7885-160312-140130", "0052-160312-131632", "0127-160312-141609", "3725-160312-151706", "9069-160312-151048", "7922-160312-160244", "0682-160312-172016", "2685-160312-172344", "7093-160312-154220", "3195-160314-232954", "5912-150408-122341", "84150406-142457", "7913-150417-131457", "6808-150424-004516", "7368-150419-164942", "3038-150413-112451", "58150406-100658", "68150404-140248", "10150404-134548", "61150404-162737", "56150407-160735", "8021-150409-165612", "8901-150430-111830", "1584-150409-112324", "9049-150416-103205", "95150404-130909", "1083-150417-152920", "1809-150428-104045", "7477-150428-103245", "9865-150426-124940", "6104-150427-134207", "6315-160225-130444", "8441-150413-113304", "9811-150409-103922", "8964-150420-121821", "4723-160311-151729", "03150402-152426", "9373-150426-135654", "8422-150410-113036", "9552-150410-111820", "4713-150420-124126", "6087-170212-130603", "9966-170211-170427", "0319-150415-110441", "9914-150414-143319", "8688-150413-214801", "4046-150507-120439", "5688-160311-160356", "37150406-124138", "86150406-152725", "6080-160305-164900", "7098-160309-162246", "3941-160309-164350", "0502-170213-143616", "6329-170131-185402", "1338-150415-172930", "0443-150620-112146", "39150405-125954", "4439-150422-115107", "6970-150415-123739", "5363-150417-125713", "6048-160225-114522", "8477-160301-195215", "7934-160301-193415", "7595-150423-120919", "5464-150427-154315", "1054-160102-160709", "15150407-161016", "2585-150410-170055", "8344-150429-162201", "8833-150504-131131", "0184-150429-124639", "0767-150430-105420", "0799-150430-113108", "1784-150430-101515", "2900-150430-114213", "3100-150501-103154", "3253-150430-111321", "3477-150430-113036", "4437-150430-104224", "4756-150411-150337", "4861-150430-112525", "5200-150501-103751", "5107-150430-104415", "5360-150430-090404", "6453-150507-190621", "7074-150507-151336", "1705-170116-142649", "91150406-161756", "9383-160128-125153", "0376-150421-155232", "2043-160310-154642", "23150403-182551", "0220-150422-163321", "3271-150506-120301", "0831-160218-160442", "3138-160219-144523", "3570-160219-145952", "5730-160219-155537", "3603-160220-122613", "4448-160220-193243", "5148-160220-114832", "1924-160220-113523", "6796-160220-221344", "5301-160304-192359", "8226-160305-162320", "9581-160305-115025", "8823-160305-140040", "1429-150427-153420", "5607-150425-102421", "47150406-125841", "7217-160209-132527", "2719-150417-140130", "8501-150421-095920", "45150407-154450", "1067-150419-120449", "5555-160129-181224", "0855-160314-234426", "8195-160314-180632", "2658-160314-185351", "8213-160314-204848", "2521-160314-200255", "9616-160314-214031", "0244-160314-231118", "3845-160314-214240", "1475-160315-001332", "3897-160314-200045", "7710-160319-204556", "6424-160319-215612", "7341-160319-202433", "6897-160319-221313", "8988-160319-224714", "5086-150410-102356", "7920-150413-130841", "2861-150410-135151", "6794-160319-164923", "2163-160319-175204", "8260-160319-182908", "6607-160319-151901", "2549-160319-143150", "5086-160319-123814", "2359-160319-121236", "8319-160319-114531", "52150401-155113", "6429-150410-134158", "8471-150412-173932", "2546-150412-173119", "5430-150413-163549", "4595-150422-114349", "7960-150502-150257", "1556-150502-151010", "1620-150502-152356", "5563-150410-155500", "8462-150422-133254", "4469-150422-140349", "39150404-122803", "89150406-135317", "8733-160220-105912", "3280-160311-151200", "68150404-143104", "1620-150419-083055", "98150402-140537", "4240-150619-124624", "90150402-150047", "5077-150411-114936", "1735-160106-153643", "4305-160313-173609", "4707-160313-172938", "7992-160313-172334", "4292-160313-174602", "2969-160313-170725", "7697-160314-151405", "5247-160314-201702", "2991-160314-202954", "3831-170207-161803", "52150407-113402", "1185-150410-154249", "4181-150415-152826", "63150406-133241", "7082-160225-144210", "6833-150429-151901", "7084-150603-172353", "8010-150421-124203", "5725-150505-114121", "1394-150430-232728", "60150406-153912", "9700-150418-103531", "5407-150420-114851", "0261-150421-122215", "4944-150507-132508", "0878-160225-114919", "4393-150421-155815", "0967-160310-130928", "2599-150421-154955", "6087-160224-152646", "0546-160224-111407", "7909-160312-182449", "0487-160312-190833", "9358-170111-094327", "6379-170112-093250", "9888-150421-113110", "1166-150622-055114", "2952-150624-163859", "7893-150421-160431", "87150404-160809", "2876-150409-150305", "8578-150418-145228", "4050-150424-155306", "70150404-125108", "9618-150416-122424", "8941-150415-152633", "0959-170118-101333", "0774-150427-133052", "0785-160304-122835", "9791-150422-162245", "22150407-163124", "53150406-105254", "1528-160304-125739", "8238-161224-112812", "3531-150421-143805", "4278-150416-175043", "7787-150411-145051", "56150403-213949", "6604-150416-125428", "4857-150421-080322", "0023-160316-130354", "1472-160312-141517", "7735-150421-182347", "50150403-105221", "2813-150408-164256", "3538-150411-104526", "0560-150428-125331", "2763-160203-170600", "0170-160307-001639", "9304-160306-225725", "0171-160306-235900", "2124-160306-232603", "3439-160316-105705", "7980-160316-105741", "5725-150409-102427", "4711-150426-181413", "4892-150413-110154", "0895-150413-112139", "0093-150413-132750", "4698-160201-104955", "8540-150413-110353", "6074-150413-170744", "4574-150620-175558", "5645-150620-182613", "1693-150412-112919", "0158-150427-152718", "2008-150421-095104", "4272-160108-143318", "7884-150413-153150", "4800-150415-103452", "1136-160311-110044", "9656-170111-081909", "33150407-101427", "1266-150427-190250", "2093-170207-122958", "3507-150409-123348", "7541-150412-152809", "2825-150420-092811", "9437-150420-103610", "4992-150409-141335", "3498-150408-231802", "6166-150512-104107", "9768-150427-210434", "0205-150508-120436", "2637-150425-151316", "2513-150425-192936", "4188-150424-144352", "2456-150427-125655", "1660-150507-152429", "3570-150507-110913", "1601-150408-172051", "0148-150419-091522", "1442-161217-164741", "31150403-201030", "0941-170207-002441", "8575-170131-031553", "9965-170131-032636", "5691-170131-033654", "8645-170131-034806", "7939-170131-035846", "3351-170131-041646", "9837-170131-042413", "2633-170131-043329", "2964-170131-045303", "2440-170131-201700", "4894-170201-001106", "7664-160522-105414", "6997-170207-182304", "7197-170207-173901", "7397-160524-140443", "9865-160525-110126", "4556-160522-113614", "4469-150409-124255" };

                LoginList = LoginList.Where(l => !RegNos.Contains(l.RegistrationNo)).ToList();

                var PHD = LoginList.Where(l => New_PHD_Count1.Contains(l.RegistrationNo)).ToList();

                PHDPresentCount = PHD.Where(s => s.TotalPercentage >= monthCount).Count();

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

            //StaffDetails += "<table border='1' cellpadding='3' cellspacing='0' style='font-family: Times New Roman; font-size: 8px; font-weight: normal; background-color: #fff; margin: 0 auto;'>";
            //StaffDetails += "<tr>";
            //StaffDetails += "<td width:'2' style='text-align:center;'>S.No</td>";
            //StaffDetails += "<td width:'2' style='text-align:center;'>Month</td>";
            //StaffDetails += "<td width:'3' style='text-align:center;'>Number of faculty giving Biometric attendance regularly(for atleast 15 days in a month)</td>";
            //StaffDetails += "<td width:'3' style='text-align:center;'>% of faculty not giving Biometric attendance regularly w.r.t required faculty</td>";

            //StaffDetails += "</tr>";

            //int sno = 1;
            //double minPercentage = (double)0;
            //var per = facultylist.Where(f => f.RequiredCountPercentage >= minPercentage).Count();

            //foreach (var item in facultylist)
            //{
            //    StaffDetails += "<tr>";
            //    StaffDetails += "<td style='text-align:center;'>" + sno + "</td>";
            //    StaffDetails += "<td style='text-align:center;'>" + item.Monthname + ",2018</td>";
            //    StaffDetails += "<td style='text-align:center;'>" + item.BiometricCount + "</td>";
            //    if (item.RequiredCountPercentage >= minPercentage)
            //        StaffDetails += "<td style='text-align:center;'>" + item.RequiredCountPercentage + "</td>";
            //    else
            //    {
            //        StaffDetails += "<td style='text-align:center;'>0</td>";
            //    }
            //    StaffDetails += "</tr>";
            //    sno++;
            //}

            //StaffDetails += "</table>";

            StaffDetails = CollegeFacultyList(collegeID,type);

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

        public class PHDFaculty
        {
            public string[] RegistrationNumber { get; set; }
        }

        #endregion

        #region Student BAS Report Code
        #region Student Multiple Months Attendance Report

        [Authorize(Roles = "Admin")]
        public ActionResult StudentBASNotice(string id)
        {
            int collegeId = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(id.ToString(), System.Web.Configuration.WebConfigurationManager.AppSettings["CryptoKey"]));
            var jntuh_college = db.jntuh_college.Where(c => c.id == collegeId).Select(e => e).FirstOrDefault();

           // var CollegeIds = db.jntuh_college_edit_status.Where(a => a.isSubmitted == true).Select(a => a.collegeId).ToList();
            var CollegeIds = new int[] { 4, 5, 6, 7, 9, 11, 12, 20, 23, 24, 26, 27, 29, 30, 32, 33, 34, 35, 38, 39, 40, 41, 42, 44, 47, 50, 54, 55, 56, 58, 60, 65, 67, 68, 69, 70, 72, 74, 75, 77, 79, 80, 85, 87, 88, 90, 97, 100, 104, 105, 108, 109, 110, 111, 114, 117, 118, 119, 120, 123, 127, 128, 129, 134, 135, 136, 140, 144, 145, 146, 147, 148, 150, 152, 153, 157, 158, 159, 162, 164, 165, 168, 169, 171, 174, 175, 179, 180, 182, 183, 185, 187, 188, 192, 193, 194, 195, 196, 198, 202, 203, 204, 206, 211, 214, 218, 219, 222, 223, 229, 234, 235, 241, 242, 250, 253, 254, 256, 260, 261, 262, 263, 264, 266, 267, 271, 276, 279, 282, 283, 286, 290, 292, 293, 295, 296, 297, 298, 299, 300, 301, 302, 303, 304, 306, 307, 308, 309, 310, 313, 316, 318, 319, 320, 321, 325, 326, 327, 329, 332, 335, 342, 343, 348, 350, 352, 353, 355, 360, 365, 367, 368, 374, 376, 379, 380, 384, 385, 386, 392, 394, 399, 400, 410, 411, 414, 423, 428, 429, 430, 435, 439, 441, 442, 443, 452, 454 };

            //string collegeCode = jntuh_college.collegeCode.ToUpper();
            //string ECollegeid = UAAAS.Models.Utilities.EncryptString(collegeId.ToString(), System.Web.Configuration.WebConfigurationManager.AppSettings["CryptoKey"]);

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

           // string type = MonthNameAndIds.Where(s => s.Value == Obj.Month).Select(s => s.Text).FirstOrDefault();
           
            string path = StudentBASNoticeSave(collegeId);
            path = path.Replace("/", "\\");
            if (!string.IsNullOrEmpty(path))
            {
                return File(path, "application/pdf", path.Substring(path.LastIndexOf('\\') + 1));
            }
            return View();
        }

        [Authorize(Roles = "Admin")]
        public string StudentBASNoticeSave(int? collegeId)
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

            string path = Server.MapPath("~/Content/PDFReports/BASNotice/StudentMonthlyBASReport/");

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            string fullPath = path + CollegeData.collegeCode.ToUpper() + "-" + ECollegeid + "Student_BASNotice.pdf";
            // string fullPath = path + CollegeData.collegeCode.ToUpper() + "-" + ECollegeid + "BASNotice_" + type + ".pdf";
            PdfWriter pdfWriter = PdfWriter.GetInstance(pdfDoc, new FileStream(fullPath, FileMode.Create));

            var file = CollegeData.collegeCode.ToUpper() + "-" + ECollegeid + "Student_BASNotice.pdf";
            // var file = CollegeData.collegeCode.ToUpper() + "-" + ECollegeid + "BASNotice_" + type + ".pdf";
            fullPath = string.Format("{0}/{1}", "/Content/PDFReports/BASNotice/StudentMonthlyBASReport/", file);
        
            //Open PDF Document to write data
            pdfDoc.Open();

            //Assign Html content in a string to write in PDF
            string contents = string.Empty;

            StreamReader sr;

            //Read file from server path
            sr = System.IO.File.OpenText(Server.MapPath("~/Content/StudentBASReport.html"));

            //store content in the variable
            contents = sr.ReadToEnd();
            sr.Close();

            contents = contents.Replace("##COLLEGE_CODE##", CollegeData.collegeCode.ToUpper());
        
            jntuh_address address = db.jntuh_address.Where(a => a.collegeId == CollegeData.id && (a.addressTye.Equals("COLLEGE"))).Select(a => a).FirstOrDefault();
            jntuh_address societyAddress = db.jntuh_address.Where(ad => (ad.collegeId == CollegeData.id) && (ad.addressTye.Equals("SOCIETY"))).Select(ad => ad).FirstOrDefault();

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

            contents = contents.Replace("##StudentsData##", StudentsMultipleMonthsData(collegeId));

            var parsedHtmlElements = HTMLWorker.ParseToList(new StringReader(contents), null);

            bool pageRotated = false;

            //Get each array values from parsed elements and add to the PDF document
            foreach (var htmlElement in parsedHtmlElements)
            {
                try
                {
                    if (htmlElement.Equals("<textarea>"))
                    {
                        pdfDoc.NewPage();
                    }

                    if (htmlElement.Chunks.Count >= 3)
                    {
                        if (htmlElement.Chunks.Count == 4)
                        {
                            pdfDoc.SetPageSize(iTextSharp.text.PageSize.A4.Rotate());
                            pdfDoc.SetMargins(50, 50, 60, 60);
                            pageRotated = true;
                        }
                        else
                        {
                            if (pageRotated == true)
                            {
                                pdfDoc.SetPageSize(iTextSharp.text.PageSize.A4);
                                pdfDoc.SetMargins(50, 50, 60, 60);
                                pageRotated = false;
                            }
                        }
                        pdfDoc.NewPage();
                    }
                    else
                    {
                        pdfDoc.Add((IElement)htmlElement);
                    }
                }
                catch (Exception ex)
                {
                    continue;

                }
                //if (htmlElement.Chunks.Count == 3)
                //{ pdfDoc.Add(Chunk.NEXTPAGE); }

                //pdfDoc.Add(htmlElement as IElement);
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

         [Authorize(Roles = "Admin")]
         public string StudentsMultipleMonthsData(int? CollegeId)
         {
             string Content = string.Empty;
             int Count = 1;
             if (CollegeId != null)
             {
                 //var jntuh_registered_faculty_copy_old = db.jntuh_registered_faculty_copy_old.Where(s => s.collegeId == CollegeId).Select(e => e).ToList();

                 //List<StudentsAttendance> list = new List<StudentsAttendance>();

                 //Content += "<table border='1' cellspacing='0' cellpadding='3' style='font-size: 9px;'>";
                 //Content += "<tbody>";
                 //Content += "<tr style='font-weight:bold;'>";
                 //Content += "<td width='4%'><p align='center'>S.No</p></td>";
                 //Content += "<td width='9%'><p align='center'>Department</p></td>";
                 //Content += "<td width='20%'><p align='center'>Name</p></td>";
                 //Content += "<td width='9%'><p align='center'>HallTicket No</p></td>";
                 //Content += "<td width='6%'><p align='center'>October</p></td>";
                 //Content += "<td width='7%'><p align='center'>November</p></td>";
                 //Content += "<td width='7%'><p align='center'>December</p></td>";
                 //Content += "<td width='6%'><p align='center'>January</p></td>";
                 //Content += "<td width='6%'><p align='center'>February</p></td>";
                 //Content += "<td width='5%'><p align='center'>March</p></td>";
                 //Content += "<td width='5%'><p align='center'>April</p></td>";
                 //Content += "<td width='4%'><p align='center'>May</p></td>";
                 //Content += "<td width='4%'><p align='center'>June</p></td>";
                 //Content += "<td width='8%'><p align='center'>Total working days (205)</p></td>";
                 //Content += "</tr>";

                 //foreach (var item in jntuh_registered_faculty_copy_old)
                 //{
                 //    Content += "<tr>";
                 //    Content += "<td width='4%' style='font-size: 10px;'><p align='center'>" + (Count++) + "</p></td>";
                 //    Content += "<td width='9%' style='font-size: 10px;'><p align='left'>" + item.LastName + "</p></td>";
                 //    Content += "<td width='20%' style='font-size: 10px;'><p align='left'>" + item.FirstName + "</p></td>";
                 //    Content += "<td width='9%' style='font-size: 10px;'><p align='left'>" + item.RegistrationNumber + "</p></td>";
                 //    Content += "<td width='6%' style='font-size: 10px;'><p align='right'>" + item.OrganizationName + "</p></td>";
                 //    Content += "<td width='7%' style='font-size: 10px;'><p align='right'>" + item.DesignationId + "</p></td>";
                 //    Content += "<td width='7%' style='font-size: 10px;'><p align='right'>" + item.OtherDesignation + "</p></td>";
                 //    Content += "<td width='6%' style='font-size: 10px;'><p align='right'>" + item.DepartmentId + "</p></td>";
                 //    Content += "<td width='6%' style='font-size: 10px;'><p align='right'>" + item.OtherDepartment + "</p></td>";
                 //    Content += "<td width='5%' style='font-size: 10px;'><p align='right'>" + item.grosssalary + "</p></td>";
                 //    Content += "<td width='5%' style='font-size: 10px;'><p align='right'>" + item.ProceedingsNumber + "</p></td>";
                 //    Content += "<td width='4%' style='font-size: 10px;'><p align='right'>" + item.TotalExperience + "</p></td>";
                 //    Content += "<td width='4%' style='font-size: 10px;'><p align='right'>" + item.AadhaarNumber + "</p></td>";
                 //    Content += "<td width='8%' style='font-size: 10px;'><p align='center'>" + item.TotalExperiencePresentCollege + "</p></td>";

                 //    Content += "</tr>";

                 //}

                 Content += "</tbody>";
                 Content += "</table>";

                 return Content;

             }
             return Content;

         }
        #endregion

        #region Not Enrolled Students Report

        [Authorize(Roles = "Admin")]
        public ActionResult NotEnrolledStudentsPDF(string id)
        {
            int collegeId = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(id.ToString(), System.Web.Configuration.WebConfigurationManager.AppSettings["CryptoKey"]));

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

            //  string type = MonthNameAndIds.Where(s => s.Value == Obj.Month).Select(s => s.Text).FirstOrDefault();

            string path = NotEnrolledStudentsSave(collegeId);


            path = path.Replace("/", "\\");
            if (!string.IsNullOrEmpty(path))
            {
                return File(path, "application/pdf", path.Substring(path.LastIndexOf('\\') + 1));
            }
            return View();
        }

        [Authorize(Roles = "Admin")]
        public string NotEnrolledStudentsSave(int? collegeId)
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

            string path = Server.MapPath("~/Content/PDFReports/BASNotice/Not_Enrolled_Students/");

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            string fullPath = path + CollegeData.collegeCode.ToUpper() + "-" + ECollegeid + "NotEnrolledStudents.pdf";
            PdfWriter pdfWriter = PdfWriter.GetInstance(pdfDoc, new FileStream(fullPath, FileMode.Create));

            var file = CollegeData.collegeCode.ToUpper() + "-" + ECollegeid + "NotEnrolledStudents.pdf";
            fullPath = string.Format("{0}/{1}", "/Content/PDFReports/BASNotice/Not_Enrolled_Students/", file);




            //Open PDF Document to write data
            pdfDoc.Open();

            //Assign Html content in a string to write in PDF
            string contents = string.Empty;

            StreamReader sr;

            //Read file from server path
            sr = System.IO.File.OpenText(Server.MapPath("~/Content/NotEnrolledStudents.html"));

            //store content in the variable
            contents = sr.ReadToEnd();
            sr.Close();

           
            contents = contents.Replace("##COLLEGE_CODE##", CollegeData.collegeCode.ToUpper());
          

            jntuh_address address = db.jntuh_address.Where(a => a.collegeId == CollegeData.id && (a.addressTye.Equals("COLLEGE"))).Select(a => a).FirstOrDefault();
            jntuh_address societyAddress = db.jntuh_address.Where(ad => (ad.collegeId == CollegeData.id) && (ad.addressTye.Equals("SOCIETY"))).Select(ad => ad).FirstOrDefault();

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



           // contents = contents.Replace("##StudentsData##", NotEnrolledStudents(collegeId));

            var parsedHtmlElements = HTMLWorker.ParseToList(new StringReader(contents), null);

            bool pageRotated = false;

            //Get each array values from parsed elements and add to the PDF document
            foreach (var htmlElement in parsedHtmlElements)
            {

                try
                {
                    if (htmlElement.Equals("<textarea>"))
                    {
                        pdfDoc.NewPage();
                    }

                    if (htmlElement.Chunks.Count >= 3)
                    {
                        if (htmlElement.Chunks.Count == 4)
                        {
                            pdfDoc.SetPageSize(iTextSharp.text.PageSize.A4.Rotate());
                            pdfDoc.SetMargins(50, 50, 60, 60);
                            pageRotated = true;
                        }
                        else
                        {
                            if (pageRotated == true)
                            {
                                pdfDoc.SetPageSize(iTextSharp.text.PageSize.A4);
                                pdfDoc.SetMargins(50, 50, 60, 60);
                                pageRotated = false;
                            }
                        }

                        pdfDoc.NewPage();

                    }
                    else
                    {
                        pdfDoc.Add((IElement)htmlElement);
                    }
                }
                catch (Exception ex)
                {
                    continue;

                }
               
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

        //[Authorize(Roles = "Admin")]
        //public string NotEnrolledStudents(int? CollegeId)
        //{
        //    string Content = string.Empty;
        //    int Count = 1;
        //    if (CollegeId != null)
        //    {
        //        var jntuh_registered_faculty_education11 = db.jntuh_registered_faculty_education11.Where(s => s.educationId == CollegeId).Select(e => e).ToList();

        //        List<NotEnrolledData> list = new List<NotEnrolledData>();

        //        Content += "<table border='1' cellspacing='0' cellpadding='3' style='font-size: 9px;'>";
        //        Content += "<tbody>";
        //        Content += "<tr style='font-weight:bold;'>";
        //        Content += "<td width='5%'><p align='center'>S.No</p></td>";
        //        Content += "<td width='20%'><p align='center'>Department</p></td>";
        //        Content += "<td width='45%'><p align='center'>Name</p></td>";
        //        Content += "<td width='30%'><p align='center'>HallTicket No</p></td>";
        //        Content += "</tr>";

        //        foreach (var item in jntuh_registered_faculty_education11)
        //        {
        //            Content += "<tr>";
        //            Content += "<td width='5%' style='font-size: 10px;'><p align='center'>" + (Count++) + "</p></td>";
        //            Content += "<td width='20%' style='font-size: 10px;'><p align='left'>" + item.boardOrUniversity + "</p></td>";
        //            Content += "<td width='45%' style='font-size: 10px;'><p align='left'>" + item.certificate + "</p></td>";
        //            Content += "<td width='30%' style='font-size: 10px;'><p align='left'>" + item.placeOfEducation + "</p></td>";
        //            Content += "</tr>";
        //        }

        //        Content += "</tbody>";
        //        Content += "</table>";

        //        return Content;
        //    }
        //    return Content;
        //}

        #endregion

        #region PG Students Monthly Attendance Report
        //Second Year
        [Authorize(Roles = "Admin")]
        public ActionResult StudentMonthlyBASNotice(string id ,string type)
        {        
            var CollegeIds = db.jntuh_college_basreport.Where(s => s.type == 2 && s.year == PresentYearDb && s.month == type).Select(e => e.collegeId).Distinct().ToList();
           
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

            ZipFile zip = new ZipFile();
            zip.AlternateEncodingUsage = ZipOption.AsNecessary;
            zip.AddDirectoryByName("SecondYearStudent_" + type + "_BASReports");

            foreach (var item in CollegeIds)
            {
                string path = StudentMonthlyBASNoticeSave(item, type);
            }

            string[] filePaths = Directory.GetFiles(Server.MapPath("~/Content/PDFReports/BASNotice/Student/" + PresentYearDb + "/" + type + "/SecondYear/"));

            foreach (var row in filePaths)
            {
                string filePath = row;
                zip.AddFile(filePath, "SecondYearStudent_" + type + "_BASReports");
            }

            Response.Clear();
            Response.BufferOutput = false;
            string zipName = String.Format("SecondYearStudent_" + type + "_BAS_{0}.zip", DateTime.Now.ToString("yyyy-MMM-dd-HHmmss"));
            Response.ContentType = "application/zip";
            Response.AddHeader("content-disposition", "attachment; filename=" + zipName);
            zip.Save(Response.OutputStream);
            Response.End();
       
            return View();
        }

        [Authorize(Roles = "Admin")]
        public string StudentMonthlyBASNoticeSave(int? collegeId,string type)
        {
           // string type = "August";
            jntuh_college CollegeData = db.jntuh_college.Where(c => c.id == collegeId).Select(c => c).FirstOrDefault();
            string collegeCode = CollegeData.collegeCode.ToUpper();

            string ECollegeid = UAAAS.Models.Utilities.EncryptString(collegeId.ToString(), System.Web.Configuration.WebConfigurationManager.AppSettings["CryptoKey"]);

            Document pdfDoc = new Document(PageSize.A4, 60, 50, 60, 60);

            // Create a new PdfWrite object, writing the output to a MemoryStream
            //open pdf
            var output = new MemoryStream();
            var writer = PdfWriter.GetInstance(pdfDoc, output);

            string path = Server.MapPath("~/Content/PDFReports/BASNotice/Student/" + PresentYearDb + "/" + type + "/SecondYear/");

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            string fullPath = path + CollegeData.collegeCode.ToUpper() + "-" + ECollegeid + "SecondYearStudent_BASNotice.pdf";
            // string fullPath = path + CollegeData.collegeCode.ToUpper() + "-" + ECollegeid + "BASNotice_" + type + ".pdf";
            PdfWriter pdfWriter = PdfWriter.GetInstance(pdfDoc, new FileStream(fullPath, FileMode.Create));

            var file = CollegeData.collegeCode.ToUpper() + "-" + ECollegeid + "SecondYearStudent_BASNotice.pdf";
            // var file = CollegeData.collegeCode.ToUpper() + "-" + ECollegeid + "BASNotice_" + type + ".pdf";
            fullPath = string.Format("{0}/{1}", "/Content/PDFReports/BASNotice/Student/" + PresentYearDb + "/" + type + "/SecondYear/", file);

            //Open PDF Document to write data
            pdfDoc.Open();

            //Assign Html content in a string to write in PDF
            string contents = string.Empty;

            StreamReader sr;

            //Read file from server path
            sr = System.IO.File.OpenText(Server.MapPath("~/Content/StudentBASReport.html"));

            //store content in the variable
            contents = sr.ReadToEnd();
            sr.Close();

            // contents = contents.Replace("##Monthname##", type.Substring(0,3));
            contents = contents.Replace("##COLLEGE_CODE##", CollegeData.collegeCode.ToUpper());
             contents = contents.Replace("##month##", type);

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

            contents = contents.Replace("##StudentsData##", StudentsMonthlyBASData(collegeId ,type));

            var parsedHtmlElements = HTMLWorker.ParseToList(new StringReader(contents), null);

            bool pageRotated = false;

            //Get each array values from parsed elements and add to the PDF document
            foreach (var htmlElement in parsedHtmlElements)
            {

                try
                {
                    if (htmlElement.Equals("<textarea>"))
                    {
                        pdfDoc.NewPage();
                    }

                    if (htmlElement.Chunks.Count >= 3)
                    {
                        if (htmlElement.Chunks.Count == 4)
                        {
                            pdfDoc.SetPageSize(iTextSharp.text.PageSize.A4.Rotate());
                            pdfDoc.SetMargins(50, 50, 60, 60);
                            pageRotated = true;
                        }
                        else
                        {
                            if (pageRotated == true)
                            {
                                pdfDoc.SetPageSize(iTextSharp.text.PageSize.A4);
                                pdfDoc.SetMargins(50, 50, 60, 60);
                                pageRotated = false;
                            }
                        }

                        pdfDoc.NewPage();

                    }
                    else
                    {
                        pdfDoc.Add((IElement)htmlElement);
                    }
                }
                catch (Exception ex)
                {
                    continue;

                }
                //if (htmlElement.Chunks.Count == 3)
                //{ pdfDoc.Add(Chunk.NEXTPAGE); }

                //pdfDoc.Add(htmlElement as IElement);
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

        [Authorize(Roles = "Admin")]
        public string StudentsMonthlyBASData(int? CollegeId ,string type)
        {
            string Content = string.Empty;
            int Count = 1;
            if (CollegeId != null)
            {              
                var jntuh_college_basreport = db.jntuh_college_basreport.Where(s => s.collegeId == CollegeId && s.type == 2 && s.year == PresentYearDb && s.month == type).Select(e => e).ToList();
                var jntuh_college = db.jntuh_college.ToList();
                var jntuh_department = db.jntuh_department.ToList();
                List<StudentMonthlyPresentDays> list = new List<StudentMonthlyPresentDays>();

                string[] strOtherDepartments = { "English", "Mathematics", "Physics", "Chemistry", "Others(CSE/IT)", "Others(CIVIL/MECH)", "Others(ECE/EEE)", "Others(MNGT/H&S)" };

                foreach (var item in jntuh_college_basreport)
                {
                    StudentMonthlyPresentDays student = new StudentMonthlyPresentDays();
                    student.CollegeId = item.collegeId;
                    student.CollegeCode = jntuh_college.Where(e=>e.id == item.collegeId).Select(a=>a.collegeCode).FirstOrDefault();
                    student.CollegeName = jntuh_college.Where(e => e.id == item.collegeId).Select(a => a.collegeName).FirstOrDefault();
                    student.HTNo = item.RegistrationNumber;
                    student.Name = item.name;
                    student.Dept = jntuh_department.Where(a=>a.id == item.departmentId).Select(e=>e.departmentName).FirstOrDefault();
                    student.WorkingDays = item.totalworkingDays;
                    student.HoliDays = item.NoofHolidays;
                    student.PrsentDays = item.NoofPresentDays;

                    list.Add(student);
                }

                var HumanitiesDepartments = list.Where(a => strOtherDepartments.Contains(a.Dept)).Select(s => s).ToList();

                list = list.Where(a => !strOtherDepartments.Contains(a.Dept)).Select(s => s).ToList();

                Content += "<table border='1' cellspacing='0' cellpadding='3' style='font-size: 9px;'>";
                Content += "<tbody>";
                Content += "<tr style='font-weight:bold;'>";
                Content += "<td width='4%'><p align='center'>S.No</p></td>";
                Content += "<td width='12%'><p align='center'>HallTicket No</p></td>";
                Content += "<td width='15%'><p align='center'>Name</p></td>";
                Content += "<td width='12%'><p align='center'>Department</p></td>";
                Content += "<td width='8%'><p align='center'>Total Working Days</p></td>";
                Content += "<td width='8%'><p align='center'>Holidays Including Sundays</p></td>";
                Content += "<td width='8%'><p align='center'>Present days</p></td>";
                Content += "<td width='8%'><p align='center'>% of Presence</p></td>";
                Content += "</tr>";

                foreach (var item in list.OrderBy(w=>w.Dept).ToList())
                {
                    Content += "<tr>";
                    Content += "<td width='4%' style='font-size: 10px;'><p align='center'>" + (Count++) + "</p></td>";
                    Content += "<td width='12%' style='font-size: 10px;'><p align='left'>" + item.HTNo + "</p></td>";
                    Content += "<td width='15%' style='font-size: 10px;'><p align='left'>" + item.Name + "</p></td>";
                    Content += "<td width='12%' style='font-size: 10px;'><p align='left'>" + item.Dept + "</p></td>";
                    Content += "<td width='8%' style='font-size: 10px;'><p align='right'>" + item.WorkingDays + "</p></td>";
                    Content += "<td width='8%' style='font-size: 10px;'><p align='right'>" + item.HoliDays + "</p></td>";
                    Content += "<td width='8%' style='font-size: 10px;'><p align='right'>" + item.PrsentDays + "</p></td>";
                    decimal percentage = ((decimal)item.PrsentDays / (decimal)item.WorkingDays) * 100;

                    Content += "<td width='8%' style='font-size: 10px;'><p align='right'>" + Math.Round(percentage,2) + "</p></td>";
                    Content += "</tr>";
                }

                foreach (var item in HumanitiesDepartments.OrderBy(w => w.Dept).ToList())
                {
                    Content += "<tr>";
                    Content += "<td width='4%' style='font-size: 10px;'><p align='center'>" + (Count++) + "</p></td>";
                    Content += "<td width='15%' style='font-size: 10px;'><p align='left'>" + item.HTNo + "</p></td>";
                    Content += "<td width='15%' style='font-size: 10px;'><p align='left'>" + item.Name + "</p></td>";
                    Content += "<td width='12%' style='font-size: 10px;'><p align='left'>" + item.Dept + "</p></td>";
                    Content += "<td width='8%' style='font-size: 10px;'><p align='right'>" + item.WorkingDays + "</p></td>";
                    Content += "<td width='8%' style='font-size: 10px;'><p align='right'>" + item.HoliDays + "</p></td>";
                    Content += "<td width='8%' style='font-size: 10px;'><p align='right'>" + item.PrsentDays + "</p></td>";

                    decimal percentage = ((decimal)item.PrsentDays / (decimal)item.WorkingDays) * 100;

                    Content += "<td width='8%' style='font-size: 10px;'><p align='right'>" + Math.Round(percentage, 2) + "</p></td>";
                    Content += "</tr>";
                }

                Content += "</tbody>";
                Content += "</table>";

                return Content;
            }
            return Content;

        }

        //First Year
        [Authorize(Roles = "Admin")]
        public ActionResult FirstYearStudentsMonthlyBASNotice(string id, string type)
        {          
            var CollegeIds = db.jntuh_college_basreport.Where(s => s.type == 3 && s.year == PresentYearDb && s.month == type).Select(e => e.collegeId).Distinct().ToList();

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

            ZipFile zip = new ZipFile();
            zip.AlternateEncodingUsage = ZipOption.AsNecessary;
            zip.AddDirectoryByName("FirstYearStudent_" + type + "_BASReports");

            foreach (var item in CollegeIds)
            {
                string path = FirstYearStudentsMonthlyBASNoticeSave(item, type);
            }

            string[] filePaths = Directory.GetFiles(Server.MapPath("~/Content/PDFReports/BASNotice/Student/" + PresentYearDb + "/" + type + "/FirstYear/"));

            foreach (var row in filePaths)
            {
                string filePath = row;
                zip.AddFile(filePath, "FirstYearStudent_" + type + "_BASReports");
            }

            Response.Clear();
            Response.BufferOutput = false;
            string zipName = String.Format("FirstYearStudent_" + type + "_BAS_{0}.zip", DateTime.Now.ToString("yyyy-MMM-dd-HHmmss"));
            Response.ContentType = "application/zip";
            Response.AddHeader("content-disposition", "attachment; filename=" + zipName);
            zip.Save(Response.OutputStream);
            Response.End();

            return View();
        }

        [Authorize(Roles = "Admin")]
        public string FirstYearStudentsMonthlyBASNoticeSave(int? collegeId, string type)
        {
            // string type = "August";
            jntuh_college CollegeData = db.jntuh_college.Where(c => c.id == collegeId).Select(c => c).FirstOrDefault();
            string collegeCode = CollegeData.collegeCode.ToUpper();

            string ECollegeid = UAAAS.Models.Utilities.EncryptString(collegeId.ToString(), System.Web.Configuration.WebConfigurationManager.AppSettings["CryptoKey"]);

            Document pdfDoc = new Document(PageSize.A4, 60, 50, 60, 60);

            // Create a new PdfWrite object, writing the output to a MemoryStream
            //open pdf
            var output = new MemoryStream();
            var writer = PdfWriter.GetInstance(pdfDoc, output);

            string path = Server.MapPath("~/Content/PDFReports/BASNotice/Student/" + PresentYearDb + "/" + type + "/FirstYear/");

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            string fullPath = path + CollegeData.collegeCode.ToUpper() + "-" + ECollegeid + "FirstYearStudent_BASNotice.pdf";
            // string fullPath = path + CollegeData.collegeCode.ToUpper() + "-" + ECollegeid + "BASNotice_" + type + ".pdf";
            PdfWriter pdfWriter = PdfWriter.GetInstance(pdfDoc, new FileStream(fullPath, FileMode.Create));

            var file = CollegeData.collegeCode.ToUpper() + "-" + ECollegeid + "FirstYearStudent_BASNotice.pdf";
            // var file = CollegeData.collegeCode.ToUpper() + "-" + ECollegeid + "BASNotice_" + type + ".pdf";
            fullPath = string.Format("{0}/{1}", "/Content/PDFReports/BASNotice/Student/" + PresentYearDb + "/" + type + "/FirstYear/", file);

            //Open PDF Document to write data
            pdfDoc.Open();

            //Assign Html content in a string to write in PDF
            string contents = string.Empty;

            StreamReader sr;

            //Read file from server path
            sr = System.IO.File.OpenText(Server.MapPath("~/Content/FirstYearPGStudentsBAS.html"));

            //store content in the variable
            contents = sr.ReadToEnd();
            sr.Close();

            // contents = contents.Replace("##Monthname##", type.Substring(0,3));
            contents = contents.Replace("##COLLEGE_CODE##", CollegeData.collegeCode.ToUpper());
            contents = contents.Replace("##month##", type);

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

            contents = contents.Replace("##StudentsData##", FirstYearStudentsMonthlyBASData(collegeId, type));

            var parsedHtmlElements = HTMLWorker.ParseToList(new StringReader(contents), null);

            bool pageRotated = false;

            //Get each array values from parsed elements and add to the PDF document
            foreach (var htmlElement in parsedHtmlElements)
            {

                try
                {
                    if (htmlElement.Equals("<textarea>"))
                    {
                        pdfDoc.NewPage();
                    }

                    if (htmlElement.Chunks.Count >= 3)
                    {
                        if (htmlElement.Chunks.Count == 4)
                        {
                            pdfDoc.SetPageSize(iTextSharp.text.PageSize.A4.Rotate());
                            pdfDoc.SetMargins(50, 50, 60, 60);
                            pageRotated = true;
                        }
                        else
                        {
                            if (pageRotated == true)
                            {
                                pdfDoc.SetPageSize(iTextSharp.text.PageSize.A4);
                                pdfDoc.SetMargins(50, 50, 60, 60);
                                pageRotated = false;
                            }
                        }

                        pdfDoc.NewPage();

                    }
                    else
                    {
                        pdfDoc.Add((IElement)htmlElement);
                    }
                }
                catch (Exception ex)
                {
                    continue;

                }
                //if (htmlElement.Chunks.Count == 3)
                //{ pdfDoc.Add(Chunk.NEXTPAGE); }

                //pdfDoc.Add(htmlElement as IElement);
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

        [Authorize(Roles = "Admin")]
        public string FirstYearStudentsMonthlyBASData(int? CollegeId, string type)
        {
            string Content = string.Empty;
            int Count = 1;
            if (CollegeId != null)
            {
                var jntuh_college_basreport = db.jntuh_college_basreport.Where(s => s.collegeId == CollegeId && s.type == 3 && s.year == PresentYearDb && s.month == type).Select(e => e).ToList();
                var jntuh_college = db.jntuh_college.ToList();
                var jntuh_department = db.jntuh_department.ToList();
                List<StudentMonthlyPresentDays> list = new List<StudentMonthlyPresentDays>();

                string[] strOtherDepartments = { "English", "Mathematics", "Physics", "Chemistry", "Others(CSE/IT)", "Others(CIVIL/MECH)", "Others(ECE/EEE)", "Others(MNGT/H&S)" };

                foreach (var item in jntuh_college_basreport)
                {
                    StudentMonthlyPresentDays student = new StudentMonthlyPresentDays();
                    student.CollegeId = item.collegeId;
                    student.CollegeCode = jntuh_college.Where(e => e.id == item.collegeId).Select(a => a.collegeCode).FirstOrDefault();
                    student.CollegeName = jntuh_college.Where(e => e.id == item.collegeId).Select(a => a.collegeName).FirstOrDefault();
                    student.HTNo = item.RegistrationNumber;
                    student.Name = item.name;
                    student.Dept = jntuh_department.Where(a => a.id == item.departmentId).Select(e => e.departmentName).FirstOrDefault();
                    student.WorkingDays = item.totalworkingDays;
                    student.HoliDays = item.NoofHolidays;
                    student.PrsentDays = item.NoofPresentDays;

                    list.Add(student);
                }

                var HumanitiesDepartments = list.Where(a => strOtherDepartments.Contains(a.Dept)).Select(s => s).ToList();

                list = list.Where(a => !strOtherDepartments.Contains(a.Dept)).Select(s => s).ToList();

                Content += "<table border='1' cellspacing='0' cellpadding='3' style='font-size: 9px;'>";
                Content += "<tbody>";
                Content += "<tr style='font-weight:bold;'>";
                Content += "<td width='4%'><p align='center'>S.No</p></td>";
                Content += "<td width='12%'><p align='center'>HallTicket No</p></td>";
                Content += "<td width='15%'><p align='center'>Name</p></td>";
                Content += "<td width='12%'><p align='center'>Department</p></td>";
                Content += "<td width='8%'><p align='center'>Total Working Days</p></td>";
                Content += "<td width='8%'><p align='center'>Holidays Including Sundays</p></td>";
                Content += "<td width='8%'><p align='center'>Present days</p></td>";
                Content += "<td width='8%'><p align='center'>% of Presence</p></td>";
                Content += "</tr>";

                foreach (var item in list.OrderBy(w => w.Dept).ToList())
                {
                    Content += "<tr>";
                    Content += "<td width='4%' style='font-size: 10px;'><p align='center'>" + (Count++) + "</p></td>";
                    Content += "<td width='12%' style='font-size: 10px;'><p align='left'>" + item.HTNo + "</p></td>";
                    Content += "<td width='15%' style='font-size: 10px;'><p align='left'>" + item.Name + "</p></td>";
                    Content += "<td width='12%' style='font-size: 10px;'><p align='left'>" + item.Dept + "</p></td>";
                    Content += "<td width='8%' style='font-size: 10px;'><p align='right'>" + item.WorkingDays + "</p></td>";
                    Content += "<td width='8%' style='font-size: 10px;'><p align='right'>" + item.HoliDays + "</p></td>";
                    Content += "<td width='8%' style='font-size: 10px;'><p align='right'>" + item.PrsentDays + "</p></td>";
                    decimal percentage = ((decimal)item.PrsentDays / (decimal)item.WorkingDays) * 100;

                    Content += "<td width='8%' style='font-size: 10px;'><p align='right'>" + Math.Round(percentage, 2) + "</p></td>";
                    Content += "</tr>";
                }

                foreach (var item in HumanitiesDepartments.OrderBy(w => w.Dept).ToList())
                {
                    Content += "<tr>";
                    Content += "<td width='4%' style='font-size: 10px;'><p align='center'>" + (Count++) + "</p></td>";
                    Content += "<td width='15%' style='font-size: 10px;'><p align='left'>" + item.HTNo + "</p></td>";
                    Content += "<td width='15%' style='font-size: 10px;'><p align='left'>" + item.Name + "</p></td>";
                    Content += "<td width='12%' style='font-size: 10px;'><p align='left'>" + item.Dept + "</p></td>";
                    Content += "<td width='8%' style='font-size: 10px;'><p align='right'>" + item.WorkingDays + "</p></td>";
                    Content += "<td width='8%' style='font-size: 10px;'><p align='right'>" + item.HoliDays + "</p></td>";
                    Content += "<td width='8%' style='font-size: 10px;'><p align='right'>" + item.PrsentDays + "</p></td>";

                    decimal percentage = ((decimal)item.PrsentDays / (decimal)item.WorkingDays) * 100;

                    Content += "<td width='8%' style='font-size: 10px;'><p align='right'>" + Math.Round(percentage, 2) + "</p></td>";
                    Content += "</tr>";
                }

                Content += "</tbody>";
                Content += "</table>";

                return Content;
            }
            return Content;

        }

        #endregion

        #region UG Students Monthly Attendance Report
        //Pharm.D
        [Authorize(Roles = "Admin")]
        public ActionResult PharmDStudentsMonthlyBASNotice(string id, string type)
        {
            var CollegeIds = db.jntuh_college_basreport.Where(s => s.type == 4 && s.year == PresentYearDb && s.month == type).Select(e => e.collegeId).Distinct().ToList();
            //var CollegeIds = new int[4]{ 9,117,106,47};
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

            ZipFile zip = new ZipFile();
            zip.AlternateEncodingUsage = ZipOption.AsNecessary;
            zip.AddDirectoryByName("PharmDStudent_" + type + "_BASReports");

            foreach (var item in CollegeIds)
            {
                string path = PharmDStudentsMonthlyBASNoticeSave(item, type);
            }

            string[] filePaths = Directory.GetFiles(Server.MapPath("~/Content/PDFReports/BASNotice/Student/" + PresentYearDb + "/" + type + "/PharmD/"));

            foreach (var row in filePaths)
            {
                string filePath = row;
                zip.AddFile(filePath, "PharmDStudent_" + type + "_BASReports");
            }

            Response.Clear();
            Response.BufferOutput = false;
            string zipName = String.Format("PharmDStudent_" + type + "_BAS_{0}.zip", DateTime.Now.ToString("yyyy-MMM-dd-HHmmss"));
            Response.ContentType = "application/zip";
            Response.AddHeader("content-disposition", "attachment; filename=" + zipName);
            zip.Save(Response.OutputStream);
            Response.End();

            return View();
        }

        [Authorize(Roles = "Admin")]
        public string PharmDStudentsMonthlyBASNoticeSave(int? collegeId, string type)
        {
            // string type = "August";
            jntuh_college CollegeData = db.jntuh_college.Where(c => c.id == collegeId).Select(c => c).FirstOrDefault();
            string collegeCode = CollegeData.collegeCode.ToUpper();

            string ECollegeid = UAAAS.Models.Utilities.EncryptString(collegeId.ToString(), System.Web.Configuration.WebConfigurationManager.AppSettings["CryptoKey"]);

            Document pdfDoc = new Document(PageSize.A4, 60, 50, 60, 60);

            // Create a new PdfWrite object, writing the output to a MemoryStream
            //open pdf
            var output = new MemoryStream();
            var writer = PdfWriter.GetInstance(pdfDoc, output);

            string path = Server.MapPath("~/Content/PDFReports/BASNotice/Student/" + PresentYearDb + "/" + type + "/PharmD/");

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            string fullPath = path + CollegeData.collegeCode.ToUpper() + "-" + ECollegeid + "PharmDStudent_BASNotice.pdf";
            // string fullPath = path + CollegeData.collegeCode.ToUpper() + "-" + ECollegeid + "BASNotice_" + type + ".pdf";
            PdfWriter pdfWriter = PdfWriter.GetInstance(pdfDoc, new FileStream(fullPath, FileMode.Create));

            var file = CollegeData.collegeCode.ToUpper() + "-" + ECollegeid + "PharmDStudent_BASNotice.pdf";
            // var file = CollegeData.collegeCode.ToUpper() + "-" + ECollegeid + "BASNotice_" + type + ".pdf";
            fullPath = string.Format("{0}/{1}", "/Content/PDFReports/BASNotice/Student/" + PresentYearDb + "/" + type + "/PharmD/", file);

            //Open PDF Document to write data
            pdfDoc.Open();

            //Assign Html content in a string to write in PDF
            string contents = string.Empty;

            StreamReader sr;

            //Read file from server path
            sr = System.IO.File.OpenText(Server.MapPath("~/Content/PharmDStudentsBAS.html"));

            //store content in the variable
            contents = sr.ReadToEnd();
            sr.Close();

            // contents = contents.Replace("##Monthname##", type.Substring(0,3));
            contents = contents.Replace("##COLLEGE_CODE##", CollegeData.collegeCode.ToUpper());
            contents = contents.Replace("##month##", type);

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

            contents = contents.Replace("##StudentsData##", PharmDStudentsMonthlyBASData(collegeId, type));

            var parsedHtmlElements = HTMLWorker.ParseToList(new StringReader(contents), null);

            bool pageRotated = false;

            //Get each array values from parsed elements and add to the PDF document
            foreach (var htmlElement in parsedHtmlElements)
            {

                try
                {
                    if (htmlElement.Equals("<textarea>"))
                    {
                        pdfDoc.NewPage();
                    }

                    if (htmlElement.Chunks.Count >= 3)
                    {
                        if (htmlElement.Chunks.Count == 4)
                        {
                            pdfDoc.SetPageSize(iTextSharp.text.PageSize.A4.Rotate());
                            pdfDoc.SetMargins(50, 50, 60, 60);
                            pageRotated = true;
                        }
                        else
                        {
                            if (pageRotated == true)
                            {
                                pdfDoc.SetPageSize(iTextSharp.text.PageSize.A4);
                                pdfDoc.SetMargins(50, 50, 60, 60);
                                pageRotated = false;
                            }
                        }

                        pdfDoc.NewPage();

                    }
                    else
                    {
                        pdfDoc.Add((IElement)htmlElement);
                    }
                }
                catch (Exception ex)
                {
                    continue;

                }
                //if (htmlElement.Chunks.Count == 3)
                //{ pdfDoc.Add(Chunk.NEXTPAGE); }

                //pdfDoc.Add(htmlElement as IElement);
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

        [Authorize(Roles = "Admin")]
        public string PharmDStudentsMonthlyBASData(int? CollegeId, string type)
        {
            string Content = string.Empty;
            int Count = 1;
            if (CollegeId != null)
            {
                var jntuh_college_basreport = db.jntuh_college_basreport.Where(s => s.collegeId == CollegeId && s.type == 4 && s.year == PresentYearDb && s.month == type).Select(e => e).ToList();
                var jntuh_college = db.jntuh_college.ToList();
                var jntuh_department = db.jntuh_department.ToList();
                List<StudentMonthlyPresentDays> list = new List<StudentMonthlyPresentDays>();

                string[] strOtherDepartments = { "English", "Mathematics", "Physics", "Chemistry", "Others(CSE/IT)", "Others(CIVIL/MECH)", "Others(ECE/EEE)", "Others(MNGT/H&S)" };

                foreach (var item in jntuh_college_basreport)
                {
                    StudentMonthlyPresentDays student = new StudentMonthlyPresentDays();
                    student.CollegeId = item.collegeId;
                    student.CollegeCode = jntuh_college.Where(e => e.id == item.collegeId).Select(a => a.collegeCode).FirstOrDefault();
                    student.CollegeName = jntuh_college.Where(e => e.id == item.collegeId).Select(a => a.collegeName).FirstOrDefault();
                    student.HTNo = item.RegistrationNumber;
                    student.Name = item.name;
                    student.Dept = jntuh_department.Where(a => a.id == item.departmentId).Select(e => e.departmentName).FirstOrDefault();
                    student.WorkingDays = item.totalworkingDays;
                    student.HoliDays = item.NoofHolidays;
                    student.PrsentDays = item.NoofPresentDays;

                    list.Add(student);
                }

                var HumanitiesDepartments = list.Where(a => strOtherDepartments.Contains(a.Dept)).Select(s => s).ToList();

                list = list.Where(a => !strOtherDepartments.Contains(a.Dept)).Select(s => s).ToList();

                Content += "<table border='1' cellspacing='0' cellpadding='3' style='font-size: 9px;'>";
                Content += "<tbody>";
                Content += "<tr style='font-weight:bold;'>";
                Content += "<td width='4%'><p align='center'>S.No</p></td>";
                Content += "<td width='12%'><p align='center'>HallTicket No</p></td>";
                Content += "<td width='15%'><p align='center'>Name</p></td>";
                Content += "<td width='12%'><p align='center'>Department</p></td>";
                Content += "<td width='8%'><p align='center'>Total Working Days</p></td>";
                Content += "<td width='8%'><p align='center'>Holidays Including Sundays</p></td>";
                Content += "<td width='8%'><p align='center'>Present days</p></td>";
                Content += "<td width='8%'><p align='center'>% of Presence</p></td>";
                Content += "</tr>";

                foreach (var item in list.OrderBy(w => w.HTNo).ToList())
                {
                    Content += "<tr>";
                    Content += "<td width='4%' style='font-size: 10px;'><p align='center'>" + (Count++) + "</p></td>";
                    Content += "<td width='12%' style='font-size: 10px;'><p align='left'>" + item.HTNo + "</p></td>";
                    Content += "<td width='15%' style='font-size: 10px;'><p align='left'>" + item.Name + "</p></td>";
                    Content += "<td width='12%' style='font-size: 10px;'><p align='left'>" + item.Dept + "</p></td>";
                    Content += "<td width='8%' style='font-size: 10px;'><p align='right'>" + item.WorkingDays + "</p></td>";
                    Content += "<td width='8%' style='font-size: 10px;'><p align='right'>" + item.HoliDays + "</p></td>";
                    Content += "<td width='8%' style='font-size: 10px;'><p align='right'>" + item.PrsentDays + "</p></td>";
                    decimal percentage = ((decimal)item.PrsentDays / (decimal)item.WorkingDays) * 100;

                    Content += "<td width='8%' style='font-size: 10px;'><p align='right'>" + Math.Round(percentage, 2) + "</p></td>";
                    Content += "</tr>";
                }

                foreach (var item in HumanitiesDepartments.OrderBy(w => w.HTNo).ToList())
                {
                    Content += "<tr>";
                    Content += "<td width='4%' style='font-size: 10px;'><p align='center'>" + (Count++) + "</p></td>";
                    Content += "<td width='15%' style='font-size: 10px;'><p align='left'>" + item.HTNo + "</p></td>";
                    Content += "<td width='15%' style='font-size: 10px;'><p align='left'>" + item.Name + "</p></td>";
                    Content += "<td width='12%' style='font-size: 10px;'><p align='left'>" + item.Dept + "</p></td>";
                    Content += "<td width='8%' style='font-size: 10px;'><p align='right'>" + item.WorkingDays + "</p></td>";
                    Content += "<td width='8%' style='font-size: 10px;'><p align='right'>" + item.HoliDays + "</p></td>";
                    Content += "<td width='8%' style='font-size: 10px;'><p align='right'>" + item.PrsentDays + "</p></td>";

                    decimal percentage = ((decimal)item.PrsentDays / (decimal)item.WorkingDays) * 100;

                    Content += "<td width='8%' style='font-size: 10px;'><p align='right'>" + Math.Round(percentage, 2) + "</p></td>";
                    Content += "</tr>";
                }

                Content += "</tbody>";
                Content += "</table>";

                return Content;
            }
            return Content;

        }

        #endregion
        #endregion
    }

    public class StudentMonthlyPresentDays
    {
        public int? CollegeId { get; set; }
        public string CollegeCode { get; set; }
        public string CollegeName { get; set; }
        public string HTNo { get; set; }
        public string Name { get; set; }
        public string Dept { get; set; }
        public int? WorkingDays { get; set; }
        public int? HoliDays { get; set; }
        public int? PrsentDays { get; set; }
        public string Percentage { get; set; }
    }

    public class FacultyMonthlyPresentDays
    {
        public int? CollegeId { get; set; }
        public string CollegeCode { get; set; }
        public string CollegeName { get; set; }
        public string RegNo { get; set; }
        public string Name { get; set; }
        public string Dept { get; set; }
        public int? WorkingDays { get; set; }
        public int? HoliDays { get; set; }
        public int? PrsentDays { get; set; }
        public decimal Percentage { get; set; }
    }
}
