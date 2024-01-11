using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using UAAAS.Models;
using System.Web.Security;
using System.Data;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;
using it = iTextSharp.text;
using System.Globalization;
using iTextSharp.text.html.simpleparser;
using iTextSharp.text.html;
using iTextSharp.text.xml;
using System.Web.Configuration;
using System.Data.OleDb;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using System.Security.Cryptography;
using System.Net;
using Utilities = UAAAS.Models.Utilities;
using System.Data.Entity.Validation;
using System.Diagnostics;

namespace UAAAS.Controllers.College
{
    public class AppealFormatController : BaseController
    {

        private uaaasDBContext db = new uaaasDBContext();
        private string serverURL;

        #region CustomClasses

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
            public string collegeRandomCode { get; set; }
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
            public List<AnonymousLabclass> LabsListDefs1 { get; set; }
            public List<AnonymousMBAMACclass> MBAMACDetails { get; set; }
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

            public string PharmacyGroup1 { get; set; }


            public string PharmacySubGroup1 { get; set; }
            public int BPharmacySubGroup1Count { get; set; }
            public int BPharmacySubGroupRequired { get; set; }
            public string BPharmacySubGroupMet { get; set; }


            public string PharmacySpec1 { get; set; }
            public string PharmacySpec2 { get; set; }

            public IList<PharmacySpecilaizationList> PharmacySpecilaizationList { get; set; }

            //For collegeintake

            public List<CollegeIntakeExisting> CollegeIntakeExistings { get; set; }

            public List<CollegeIntakeExisting> CollegeIntakeSupportingDocuments { get; set; }

            public string AffliationStatus { get; set; }
            public decimal BphramacyrequiredFaculty { get; set; }
            public decimal pharmadrequiredfaculty { get; set; }
            public decimal pharmadPBrequiredfaculty { get; set; }
            public int totalcollegefaculty { get; set; }
            public int SortId { get; set; }

            public IList<CollegeFacultyWithIntakeReport> FacultyWithIntakeReports { get; set; }
            public int BtechAdjustedFaculty { get; set; }
            public int specializationWiseFacultyPHDFaculty { get; set; }
            public IList<CollegeFaculty> CollegeFaculties { get; set; }
            public IList<PhysicalLabMaster> PhysicalLabMasters { get; set; }
            public string Remarks { get; set; }
            public string FurtherAppealSupportingDoc { get; set; }
            public string DeclarationPath { get; set; }
        }

        public class AnonymousLabclass
        {
            public int? id { get; set; }
            public int? EquipmentID { get; set; }
            public string LabCode { get; set; }
            public string LabName { get; set; }
            public string EquipmentName { get; set; }
            public string LabName1 { get; set; }
            public string EquipmentName1 { get; set; }
            public int year { get; set; }
            public int? Semester { get; set; }
            public bool IsActive { get; set; }
        }

        public class AnonymousMBAMACclass
        {
            public int? id { get; set; }
            public string CollegeCode { get; set; }
            public int CollegeId { get; set; }
            public HttpPostedFileBase MACSupportingDoc { get; set; }
            public int? ComputerDeficiencyCount { get; set; }
        }

        public class PharmacySpecilaizationList
        {
            public string PharmacyspecName { get; set; }
            public string Specialization { get; set; }
            public int PharmacyspecWiseCount { get; set; }
        }

        private int GetIntake(int? collegeId, int academicYearId, int specializationId, int shiftId, int flag)
        {
            int intake = 0;

            //approved
            if (flag == 1 && academicYearId != 11)
            {
                intake = db.jntuh_college_intake_existing.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == shiftId).Select(i => i.approvedIntake).FirstOrDefault();

            }
            else if (flag == 1 && academicYearId == 11)
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
        #endregion

        #region NewCodeRequirement

        //Appeal Faculty Details
       


        [HttpGet]
        [Authorize(Roles = "Admin,SuperAdmin,College,FacultyVerification")]
        public ActionResult ViewLabDetails(int? id, int collegeId, int? eqpid, int? eqpno)
        {

            Lab laboratories = new Lab();
            laboratories.collegeId = collegeId;

            if (id != null)
            {
                ViewBag.IsUpdate = true;
                laboratories = (from m in db.jntuh_lab_master
                                join labs in db.jntuh_appeal_college_laboratories on m.id equals labs.EquipmentID
                                where (labs.CollegeID == collegeId && labs.id == id)
                                select new Lab
                                {
                                    id = labs.id,
                                    collegeId = collegeId,
                                    EquipmentID = labs.EquipmentID,
                                    EquipmentName = m.EquipmentName,
                                  //  LabEquipmentName = labs.EquipmentName,
                                    LabEquipmentName = string.IsNullOrEmpty(m.EquipmentName) ? labs.EquipmentName : m.EquipmentName,
                                    EquipmentNo = labs.EquipmentNo,
                                    Make = labs.Make,
                                    Model = labs.Model,
                                    EquipmentUniqueID = labs.EquipmentUniqueID,
                                    AvailableUnits = labs.AvailableUnits,
                                    AvailableArea = labs.AvailableArea,
                                    RoomNumber = labs.RoomNumber,
                                    createdBy = labs.createdBy,
                                    createdOn = labs.createdOn,
                                    IsActive = true,

                                    degreeId = m.DegreeID,
                                    departmentId = m.DepartmentID,
                                    specializationId = m.SpecializationID,
                                    degree = m.jntuh_degree.degree,
                                    department = m.jntuh_department.departmentName,
                                    specializationName = m.jntuh_specialization.specializationName,
                                    year = m.Year,
                                    Semester = m.Semester,
                                    Labcode = m.Labcode,
                                  //  LabName = m.LabName,
                                    LabName = string.IsNullOrEmpty(m.LabName) ? labs.LabName : m.LabName,
                                    EquipmentDateOfPurchasing = labs.EquipmentDateOfPurchasing,
                                    // EquipmentDateOfPurchasing1 = labs.EquipmentDateOfPurchasing != null ? string.Format("{0:yyyy-MM-dd}", labs.EquipmentDateOfPurchasing.Value) : null
                                    //,
                                    DelivaryChalanaDate = labs.DelivaryChalanaDate,
                                    ViewEquipmentPhoto = labs.EquipmentPhoto,
                                    ViewDelivaryChalanaImage = labs.DelivaryChalanaImage,
                                    ViewBankStatementImage = labs.BankStatementImage,
                                    ViewStockRegisterEntryImage = labs.StockRegisterEntryImage,
                                    ViewReVerificationScreenImage = labs.ReVerificationScreenShot
                                    // AffiliationStatus=labs.

                                }).FirstOrDefault();
                if (laboratories != null)
                {
                    laboratories.EquipmentDateOfPurchasing1 = laboratories.EquipmentDateOfPurchasing != null ? UAAAS.Models.Utilities.MMDDYY2DDMMYY(laboratories.EquipmentDateOfPurchasing.ToString()) : null;
                    laboratories.DelivaryChalanaDate1 = laboratories.EquipmentDateOfPurchasing != null ? UAAAS.Models.Utilities.MMDDYY2DDMMYY(laboratories.DelivaryChalanaDate.ToString()) : null;
                    return PartialView("_ViewLabsDetails", laboratories);
                }

            }

            return RedirectToAction("CollegeAppealDetails", new { collegeid = collegeId });
        }

        [Authorize(Roles = "Admin,DataEntry,FacultyVerification")]
        public ActionResult GetFacultyBASDetailsView(string RegistarationNumber)
        {
            int userId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int collegeID = db.jntuh_college_faculty_registered.Where(r => r.RegistrationNumber == RegistarationNumber).Select(e => e.collegeId).FirstOrDefault();
            if (RegistarationNumber != null)
            {
                var FacultyBASData = db.jntuh_college_basreport.Where(e => e.RegistrationNumber == RegistarationNumber).Select(e => e).ToList();
                if (FacultyBASData.Count() != 0 && FacultyBASData != null)
                {
                    GetFacultyBASDetails Faculty = new GetFacultyBASDetails();
                    Faculty.RegistarationNumber = RegistarationNumber;
                    string date = FacultyBASData.Select(e => e.joiningDate).FirstOrDefault().ToString();

                    Faculty.BasJoiningDate = date == null ? null : Convert.ToDateTime(date).ToString("dd-MM-yyyy");
                    Faculty.TotalWorkingDays = FacultyBASData.Select(e => e.totalworkingDays).Sum();
                    Faculty.TotalPresentDays = FacultyBASData.Select(e => e.NoofPresentDays).Sum();

                    foreach (var item in FacultyBASData)
                    {
                        if (item.month == "July")
                        {
                            Faculty.JulyTotalDays = item.totalworkingDays;
                            Faculty.JulyPresentDays = item.NoofPresentDays;
                        }

                        else if (item.month == "August")
                        {
                            Faculty.AugustTotalDays = item.totalworkingDays;
                            Faculty.AugustPresentDays = item.NoofPresentDays;
                        }

                        else if (item.month == "September")
                        {
                            Faculty.SeptemberTotalDays = item.totalworkingDays;
                            Faculty.SeptemberPresentDays = item.NoofPresentDays;
                        }

                        else if (item.month == "October")
                        {
                            Faculty.OctoberTotalDays = item.totalworkingDays;
                            Faculty.OctoberPresentDays = item.NoofPresentDays;
                        }

                        else if (item.month == "November")
                        {
                            Faculty.NovemberTotalDays = item.totalworkingDays;
                            Faculty.NovemberPresentDays = item.NoofPresentDays;
                        }

                        else if (item.month == "December")
                        {
                            Faculty.DecemberTotalDays = item.totalworkingDays;
                            Faculty.DecemberPresentDays = item.NoofPresentDays;
                        }

                        else if (item.month == "January")
                        {
                            Faculty.JanuaryTotalDays = item.totalworkingDays;
                            Faculty.JanuaryPresentDays = item.NoofPresentDays;
                        }

                        else if (item.month == "February")
                        {
                            Faculty.FebruaryTotalDays = item.totalworkingDays;
                            Faculty.FebruaryPresentDays = item.NoofPresentDays;
                        }

                        else if (item.month == "March")
                        {
                            Faculty.MarchTotalDays = item.totalworkingDays;
                            Faculty.MarchPresentDays = item.NoofPresentDays;
                        }
                        else if (item.month == "April")
                        {
                            Faculty.AprilTotalDays = item.totalworkingDays;
                            Faculty.AprilPresentDays = item.NoofPresentDays;
                        }
                    }


                    return PartialView("~/Views/AppealFormat/_GetFacultyBASDetails.cshtml", Faculty);
                }
                else
                {
                    return RedirectToAction("CollegeAppealDetails", new { collegeId = collegeID });
                }
            }
            else
            {
                return RedirectToAction("CollegeAppealDetails", new { collegeId = collegeID });
            }
            // return View();
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
            public int? TotalWorkingDays { get; set; }
            public int? TotalPresentDays { get; set; }
        }
        #endregion

        #region OldRequirement

        //Appeal Principal Details
        [Authorize(Roles = "Admin")]
        public ActionResult ViewAppealPrincipalDetails()
        {
            List<CollegeFaculty> principalFaculty = new List<CollegeFaculty>();
            var principalDetails = db.jntuh_appeal_principal_registered.AsNoTracking().ToList();
            var jntuh_registered_faculty = db.jntuh_registered_faculty.AsNoTracking().ToList();
            var jntuh_college = db.jntuh_college.AsNoTracking().ToList();
            if (principalDetails != null)
            {

                foreach (var item in principalDetails)
                {
                    CollegeFaculty faculty = new CollegeFaculty();
                    faculty.FacultyRegistrationNumber = item.RegistrationNumber;
                    faculty.collegeId = item.collegeId;
                    faculty.id = jntuh_registered_faculty.Where(e => e.RegistrationNumber.Trim() == item.RegistrationNumber.Trim()).Select(e => e.id).FirstOrDefault();
                    faculty.CollegeName = jntuh_college.Where(i => i.id == item.collegeId).Select(i => i.collegeName).FirstOrDefault();
                    faculty.facultyFirstName = jntuh_registered_faculty.Where(e => e.RegistrationNumber.Trim() == item.RegistrationNumber.Trim()).Select(e => e.FirstName).FirstOrDefault();
                    faculty.facultyLastName = jntuh_registered_faculty.Where(e => e.RegistrationNumber.Trim() == item.RegistrationNumber.Trim()).Select(e => e.LastName).FirstOrDefault();
                    faculty.facultySurname = jntuh_registered_faculty.Where(e => e.RegistrationNumber.Trim() == item.RegistrationNumber.Trim()).Select(e => e.MiddleName).FirstOrDefault();
                    faculty.ViewNotificationDocument = item.NOtificationReport;
                    faculty.ViewAppointmentOrderDocument = item.AppointMentOrder;
                    faculty.ViewJoiningReportDocument = item.JoiningOrder;
                    faculty.ViewSelectionCommitteeDocument = item.SelectionCommiteMinutes;
                    faculty.ViewPhdUndertakingDocument = item.PHDUndertakingDocument;
                    faculty.ViewPhysicalPresenceDocument = item.PhysicalPresenceonInspection;
                    principalFaculty.Add(faculty);
                }

            }

            return View(principalFaculty);
        }


        //View Appeal Supporting Documents
        public ActionResult ViewSupportingDocuments(int? collegeId)
        {
            List<CollegeIntakeExisting> collegeIntakeExisting = new List<CollegeIntakeExisting>();
            int[] CollegeIds = db.jntuh_appeal_college_intake_existing_supportingdocuments.GroupBy(e => e.collegeId).Select(e => e.Key).Distinct().ToArray();
            ViewBag.Colleges = db.jntuh_college.Where(e => e.isActive == true && CollegeIds.Contains(e.id)).Select(e => new { collegeId = e.id, collegeName = e.collegeCode + "_" + e.collegeName }).OrderBy(e => e.collegeName).ToList();
            if (collegeId != null)
            {
                var AppealIntakesupportingDocuments = db.jntuh_appeal_college_intake_existing_supportingdocuments.Where(e => e.collegeId == collegeId).ToList();
                var jntuh_specialization = db.jntuh_specialization.AsEnumerable().ToList();
                var jntuh_degree = db.jntuh_degree.AsNoTracking().ToList();
                var jntuh_department = db.jntuh_department.AsNoTracking().ToList();
                if (AppealIntakesupportingDocuments != null)
                {
                    foreach (var item in AppealIntakesupportingDocuments)
                    {
                        CollegeIntakeExisting newintake = new CollegeIntakeExisting();
                        newintake.specializationId = item.specializationId;
                        newintake.Specialization = jntuh_specialization.FirstOrDefault(e => e.id == item.specializationId).specializationName;
                        newintake.DepartmentID = jntuh_specialization.FirstOrDefault(e => e.id == item.specializationId).departmentId;
                        newintake.Department = jntuh_department.FirstOrDefault(e => e.id == newintake.DepartmentID).departmentName;
                        newintake.degreeID = jntuh_department.FirstOrDefault(e => e.id == newintake.DepartmentID).degreeId;
                        newintake.Degree = jntuh_degree.FirstOrDefault(e => e.id == newintake.degreeID).degree;
                        newintake.shiftId = item.shiftId;
                        newintake.ViewSCMApprovalLetter = item.SCM;
                        newintake.ViewForm16ApprovalLetter = item.FORM16;
                        collegeIntakeExisting.Add(newintake);
                    }
                    // collegeIntakeExisting = collegeIntakeExisting.GroupBy(e => new { e.specializationId, e.shiftId }).Select(e => e).ToList();

                }
                return View(collegeIntakeExisting);
            }
            else
            {
                return View(new List<CollegeIntakeExisting>());
            }

        }

        #endregion

        #region Colleges Appeal PdF Report Download Mehods

        [Authorize(Roles = "College,Admin")]
        public ActionResult AppealPdfReport(int preview, string strcollegeId)
        {
            serverURL = Request.Url.Scheme + System.Uri.SchemeDelimiter + Request.Url.Host + (Request.Url.IsDefaultPort ? "" : ":" + Request.Url.Port);
            int collegeId = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(strcollegeId, WebConfigurationManager.AppSettings["CryptoKey"]));
            string pdfPath = string.Empty;
            string collegeCode = db.jntuh_college.Find(collegeId).collegeCode;
            if (preview == 0)
            {
                pdfPath = SaveAppealPdfReport(preview, collegeId);
                pdfPath = pdfPath.Replace("/", "\\");
            }
            return File(pdfPath, "application/pdf", collegeCode + "_AppealPdfReport.pdf");
        }

        private string SaveAppealPdfReport(int preview, int collegeId)
        {
            string fullPath = string.Empty;
            //Set page size as A4
            Document pdfDoc = new Document(PageSize.A4, 60, 50, 60, 60);
            string path = Server.MapPath("~/Content/PDFReports/AppealPdfReports/");

            string collegeCode = db.jntuh_college.Find(collegeId).collegeCode;
            string collegeName = db.jntuh_college.Find(collegeId).collegeName;

            if (preview == 0)
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                fullPath = path + collegeCode + "_" + collegeName.Substring(0, 3) + "_" + DateTime.Now.ToString("yyyMMddHHmmss") + ".pdf";

                PdfWriter pdfWriter = PdfWriter.GetInstance(pdfDoc, new FileStream(fullPath, FileMode.Create));
                ITextEvents iTextEvents = new ITextEvents();
                iTextEvents.CollegeName = collegeName;
                iTextEvents.CollegeCode = collegeCode;
                iTextEvents.formType = "Acknowledgement";
                pdfWriter.PageEvent = iTextEvents;
            }

            //Open PDF Document to write data
            pdfDoc.Open();

            //Assign Html content in a string to write in PDF
            string contents = string.Empty;

            StreamReader sr;

            //Read file from server path        
            sr = System.IO.File.OpenText(Server.MapPath("~/Content/AppealPdfReport.html"));
            //store content in the variable
            contents = sr.ReadToEnd();
            sr.Close();

            contents = contents.Replace("##SERVERURL##", serverURL);
            contents = collegeInformation(collegeId, contents);
            contents = ExistingIntakeDetails(collegeId, contents);
            contents = PrincipalDirectorDetails(collegeId, contents);
            contents = collegeFacultyComplianceMembers(collegeId, contents);
            contents = collegeFacultyReverificationMembers(collegeId, contents);
            contents = LaboratoriesDetails(collegeId, contents);

            //contents = PhysicalLabs(collegeId, contents);
            contents = Remarks(collegeId, contents);
            contents = Signatures(collegeId, contents);
            //Read string contents using stream reader and convert html to parsed conent
            var parsedHtmlElements = HTMLWorker.ParseToList(new StringReader(contents), null);

            //Get each array values from parsed elements and add to the PDF document
            bool pageRotated = false;

            foreach (var htmlElement in parsedHtmlElements)
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
                        pdfDoc.SetMargins(60, 50, 60, 60);
                        pageRotated = true;
                    }
                    else
                    {
                        if (pageRotated == true)
                        {
                            //pdfDoc.SetPageSize(iTextSharp.text.PageSize.A4.Rotate());
                            //pdfDoc.SetMargins(60, 50, 60, 60);
                            //pageRotated = true;
                            pdfDoc.SetPageSize(iTextSharp.text.PageSize.A4);
                            pdfDoc.SetMargins(60, 50, 60, 60);
                            pageRotated = false;
                        }
                    }
                    pdfDoc.NewPage();
                }
                else
                {
                    pdfDoc.Add(htmlElement as IElement);
                }
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

        private string collegeInformation(int collegeId, string contents)
        {
            CollegeInformation collegeInformation = new CollegeInformation();

            #region from jntuh_college table
            jntuh_college collegeDetails = db.jntuh_college.Where(college => college.isActive == true && college.id == collegeId)
                                                           .FirstOrDefault();
            if (collegeDetails != null)
            {
                collegeInformation.collegeName = collegeDetails.collegeName;
                collegeInformation.collegeCode = collegeDetails.collegeCode;
                collegeInformation.eamcetCode = collegeDetails.eamcetCode;
                collegeInformation.icetCode = collegeDetails.icetCode;
            }
            contents = contents.Replace("##AUDITSCHEDULECOLLEGENAME##", collegeInformation.collegeName);
            contents = contents.Replace("##COLLEGE_NAME##", collegeInformation.collegeName);
            contents = contents.Replace("##COLLEGE_CODE##", collegeInformation.collegeCode);
            contents = contents.Replace("##EAMCET_CODE##", collegeInformation.eamcetCode);
            contents = contents.Replace("##ICET_CODE##", collegeInformation.icetCode);
            //   barcodetext += "College Code:" + collegeInformation.collegeCode + ";College Name:" + collegeInformation.collegeName;
            //  LatefeeQrCodetext += "College Code:" + collegeInformation.collegeCode + ";College Name:" + collegeInformation.collegeName;
            string strCollegeType = string.Empty;
            List<jntuh_college_type> collegeType = db.jntuh_college_type.Where(s => s.isActive == true).ToList();
            foreach (var item in collegeType)
            {
                string YesOrNo = "no_b";
                int existCollegeTypeId = db.jntuh_college.Where(college => college.isActive == true && college.id == collegeId)
                                                         .Select(college => college.collegeTypeID)
                                                         .FirstOrDefault();
                if (item.id == existCollegeTypeId)
                {
                    YesOrNo = "yes_b";

                    strCollegeType += string.Format("<img src='" + serverURL + "/Content/Images/checkbox_{0}.png' height='10' />&nbsp; {1} &nbsp; &nbsp; &nbsp;", YesOrNo, item.collegeType);
                }
            }

            contents = contents.Replace("##COLLEGE_TYPE##", strCollegeType);

            string strCollegeStatus = string.Empty;
            List<jntuh_college_status> jntuh_college_status = db.jntuh_college_status.Where(s => s.isActive == true).ToList();
            foreach (var item in jntuh_college_status)
            {
                string YesOrNo = "no_b";
                int existCollegeStatusId = db.jntuh_college.Where(college => college.isActive == true && college.id == collegeId)
                                                         .Select(college => college.collegeStatusID)
                                                         .FirstOrDefault();
                if (item.id == existCollegeStatusId)
                    YesOrNo = "yes_b";

                strCollegeStatus += string.Format("<img src='" + serverURL + "/Content/Images/checkbox_{0}.png' height='10' />&nbsp; {1} &nbsp; &nbsp; &nbsp;", YesOrNo, item.collegeStatus);
            }
            contents = contents.Replace("##College_Status##", strCollegeStatus);

            #endregion

            #region from jntuh_address table

            jntuh_address addressDetails = db.jntuh_address.Where(address => address.collegeId == collegeId && address.addressTye == "COLLEGE")
                                                           .FirstOrDefault();
            string state = string.Empty;
            string district = string.Empty;
            if (addressDetails != null)
            {
                collegeInformation.address = addressDetails.address;
                collegeInformation.townOrCity = addressDetails.townOrCity;
                collegeInformation.mandal = addressDetails.mandal;
                collegeInformation.pincode = addressDetails.pincode;
                collegeInformation.fax = addressDetails.fax;
                collegeInformation.landline = addressDetails.landline;
                collegeInformation.mobile = addressDetails.mobile;
                collegeInformation.email = addressDetails.email;
                collegeInformation.website = addressDetails.website;
                state = db.jntuh_state.Where(s => s.isActive == true && s.id == addressDetails.stateId).Select(s => s.stateName).FirstOrDefault();
                district = db.jntuh_district.Where(d => d.isActive == true && d.id == addressDetails.districtId).Select(d => d.districtName).FirstOrDefault();
            }
            contents = contents.Replace("##COLLEGE_ADDRESS##", collegeInformation.address);
            contents = contents.Replace("##COLLEGE_City/Town##", collegeInformation.townOrCity);
            contents = contents.Replace("##COLLEGE_Mandal##", collegeInformation.mandal);
            contents = contents.Replace("##COLLEGE_District##", district);
            contents = contents.Replace("##COLLEGE_State##", state);
            contents = contents.Replace("##COLLEGE_Pincode##", collegeInformation.pincode.ToString() == "0" ? "" : collegeInformation.pincode.ToString());
            contents = contents.Replace("##COLLEGE_Fax##", collegeInformation.fax);
            contents = contents.Replace("##COLLEGE_Landline##", collegeInformation.landline);
            contents = contents.Replace("##COLLEGE_Mobile##", collegeInformation.mobile);
            contents = contents.Replace("##COLLEGE_Email##", collegeInformation.email);
            contents = contents.Replace("##COLLEGE_Website##", collegeInformation.website);

            #endregion

            #region from jntuh_college_affiliation table
            int NACId = 0;
            string affiliationNAAC = string.Empty;
            int affiliationNAACId = 0;
            string affiliationNAACFromDate = string.Empty;
            string affiliationNAACToDate = string.Empty;
            string affiliationNAACYes = string.Empty;
            string affiliationNAACNo = string.Empty;
            string affiliationNAACGrade = string.Empty;
            string affiliationNAACCGPA = string.Empty;
            string collegeAffiliationType = string.Empty;
            string yes = "no_b";
            string no = "no_b";
            List<jntuh_affiliation_type> affiliationType = db.jntuh_affiliation_type.OrderBy(a => a.id).Where(a => a.isActive == true).ToList();
            foreach (var item in affiliationType)
            {
                if (item.affiliationType.Trim() == "NAAC")
                {
                    affiliationNAAC = item.affiliationType.Trim();
                    affiliationNAACId = item.id;
                }
                else
                {
                    if (item.affiliationType.Trim() == "NBA Status")
                    {
                        collegeAffiliationType += "<tr>";
                        collegeAffiliationType += "<td valign='top' colspan='4'>" + item.affiliationType + "</td>";
                        collegeAffiliationType += "<td valign='top' colspan='1' align='center'>:</td>";
                        collegeAffiliationType += "<td valign='top' colspan='5'>";
                        collegeAffiliationType += "##AFFILIATIONTYPEIMAGE" + item.id + "##";
                        collegeAffiliationType += "<td valign='top' colspan='4'>If Conferred, Period </td>";
                        collegeAffiliationType += "<td valign='top' colspan='1' align='center'>:</td>";
                        collegeAffiliationType += "<td valign='top' colspan='5'>From:##AFFILIATIONTYPEFROMDATE" + item.id + "## <br/>";
                        collegeAffiliationType += "Duration:##AFFILIATIONTYPEDURATION" + item.id + "##</td>";
                        collegeAffiliationType += "</tr>";
                        collegeAffiliationType += "<br />";
                        NACId = item.id;
                    }
                    else
                    {
                        collegeAffiliationType += "<tr>";
                        collegeAffiliationType += "<td valign='top' colspan='4'>" + item.affiliationType + "</td>";
                        collegeAffiliationType += "<td valign='top' colspan='1' align='center'>:</td>";
                        collegeAffiliationType += "<td valign='top' colspan='5'>";
                        collegeAffiliationType += "##AFFILIATIONTYPEIMAGE" + item.id + "##";
                        collegeAffiliationType += "<td valign='top' colspan='4'>If Yes, Period </td>";
                        collegeAffiliationType += "<td valign='top' colspan='1' align='center'>:</td>";
                        collegeAffiliationType += "<td valign='top' colspan='5'>From:##AFFILIATIONTYPEFROMDATE" + item.id + "## <br/>";
                        collegeAffiliationType += "To:##AFFILIATIONTYPETODATE" + item.id + "## <br/>";
                        collegeAffiliationType += "Duration:##AFFILIATIONTYPEDURATION" + item.id + "##</td>";
                        collegeAffiliationType += "</tr>";
                        collegeAffiliationType += "<br />";
                    }
                }
            }

            List<jntuh_college_affiliation> affiliationTypeDetails = db.jntuh_college_affiliation.Where(a => a.collegeId == collegeId).ToList();
            foreach (var affiliation in affiliationTypeDetails)
            {
                if (affiliationNAACId == affiliation.affiliationTypeId)
                {
                    affiliationNAACYes = "yes_b";
                    affiliationNAACNo = "no_b";
                    if (affiliation.affiliationGrade != null)
                    {
                        affiliationNAACGrade = affiliation.affiliationGrade;
                    }
                    if (affiliation.CGPA != null)
                    {
                        affiliationNAACCGPA = affiliation.CGPA;
                    }
                    if (affiliation.affiliationFromDate != null && affiliation.affiliationToDate != null)
                    {
                        string fromDate = UAAAS.Models.Utilities.MMDDYY2DDMMYY(affiliation.affiliationFromDate.ToString());
                        string toDate = UAAAS.Models.Utilities.MMDDYY2DDMMYY(affiliation.affiliationToDate.ToString());
                        affiliationNAACFromDate = fromDate;
                        affiliationNAACToDate = ((Convert.ToInt32(toDate.Substring(toDate.Length - 4))) - (Convert.ToInt32(fromDate.Substring(fromDate.Length - 4)))).ToString();
                    }
                }
                if (affiliation.affiliationTypeId == NACId)
                {
                    string image = string.Empty;
                    if (affiliation.affiliationFromDate != null)
                    {
                        string fDate = UAAAS.Models.Utilities.MMDDYY2DDMMYY(affiliation.affiliationFromDate.ToString());
                        collegeAffiliationType = collegeAffiliationType.Replace("##AFFILIATIONTYPEFROMDATE" + affiliation.affiliationTypeId + "##", fDate);
                        if (affiliation.affiliationToDate != null)
                        {
                            string duration = UAAAS.Models.Utilities.MMDDYY2DDMMYY(affiliation.affiliationToDate.ToString());
                            collegeAffiliationType = collegeAffiliationType.Replace("##AFFILIATIONTYPEDURATION" + affiliation.affiliationTypeId + "##", ((Convert.ToInt32(duration.Substring(duration.Length - 4))) - (Convert.ToInt32(fDate.Substring(fDate.Length - 4)))).ToString());
                        }
                        else
                        {
                            collegeAffiliationType = collegeAffiliationType.Replace("##AFFILIATIONTYPEDURATION" + affiliation.affiliationTypeId + "##", affiliation.affiliationDuration.ToString());
                        }
                        image = string.Format("<img src='" + serverURL + "/Content/Images/checkbox_yes_b.png' height='10' />&nbsp;Conferred&nbsp;<img src='" + serverURL + "/Content/Images/checkbox_no_b.png' height='10' />&nbsp;Applied&nbsp;<br/><img src='" + serverURL + "/Content/Images/checkbox_no_b.png' height='10' />&nbsp;Not Yet Applied</td>");
                        collegeAffiliationType = collegeAffiliationType.Replace("##AFFILIATIONTYPEIMAGE" + affiliation.affiliationTypeId + "##", image);
                    }
                    else if (affiliation.affiliationStatus == "Applied")
                    {
                        image = string.Format("<img src='" + serverURL + "/Content/Images/checkbox_no_b.png' height='10' />&nbsp;Conferred&nbsp;<img src='" + serverURL + "/Content/Images/checkbox_yes_b.png' height='10' />&nbsp;Applied&nbsp;<br/><img src='" + serverURL + "/Content/Images/checkbox_no_b.png' height='10' />&nbsp;Not Yet Applied</td>");
                        collegeAffiliationType = collegeAffiliationType.Replace("##AFFILIATIONTYPEIMAGE" + affiliation.affiliationTypeId + "##", image);
                    }
                }
                else
                {
                    if (affiliation.affiliationFromDate != null && affiliation.affiliationToDate != null)
                    {
                        yes = "yes_b";
                        no = "no_b";
                        string fDate = UAAAS.Models.Utilities.MMDDYY2DDMMYY(affiliation.affiliationFromDate.ToString());
                        string tDate = UAAAS.Models.Utilities.MMDDYY2DDMMYY(affiliation.affiliationToDate.ToString());
                        string duration = affiliation.affiliationDuration.ToString();
                        collegeAffiliationType = collegeAffiliationType.Replace("##AFFILIATIONTYPEFROMDATE" + affiliation.affiliationTypeId + "##", fDate);
                        collegeAffiliationType = collegeAffiliationType.Replace("##AFFILIATIONTYPETODATE" + affiliation.affiliationTypeId + "##", tDate);
                        collegeAffiliationType = collegeAffiliationType.Replace("##AFFILIATIONTYPEDURATION" + affiliation.affiliationTypeId + "##", duration);
                        collegeAffiliationType = collegeAffiliationType.Replace("##AFFILIATIONTYPEIMAGE" + affiliation.affiliationTypeId + "##", string.Format("<img src='" + serverURL + "/Content/Images/checkbox_{0}.png' height='10' />&nbsp;{1} &nbsp;&nbsp;<img src='" + serverURL + "/Content/Images/checkbox_{2}.png' height='10' />&nbsp;{3}", yes, "Yes", no, "No</td>"));
                    }
                }


            }
            foreach (var item in affiliationType)
            {
                if (item.affiliationType.Trim() == "NBA Status")
                {
                    string image = string.Empty;
                    collegeAffiliationType = collegeAffiliationType.Replace("##AFFILIATIONTYPEFROMDATE" + item.id + "##", string.Empty);
                    collegeAffiliationType = collegeAffiliationType.Replace("##AFFILIATIONTYPEDURATION" + item.id + "##", string.Empty);
                    image = string.Format("<img src='" + serverURL + "/Content/Images/checkbox_no_b.png' height='10' />&nbsp;Conferred&nbsp;<img src='" + serverURL + "/Content/Images/checkbox_no_b.png' height='10' />&nbsp;Applied&nbsp;<br/><img src='" + serverURL + "/Content/Images/checkbox_yes_b.png' height='10' />&nbsp;Not Yet Applied</td>");
                    collegeAffiliationType = collegeAffiliationType.Replace("##AFFILIATIONTYPEIMAGE" + item.id + "##", image);
                }
                else
                {
                    yes = "no_b";
                    no = "yes_b";
                    collegeAffiliationType = collegeAffiliationType.Replace("##AFFILIATIONTYPEFROMDATE" + item.id + "##", string.Empty);
                    collegeAffiliationType = collegeAffiliationType.Replace("##AFFILIATIONTYPETODATE" + item.id + "##", string.Empty);
                    collegeAffiliationType = collegeAffiliationType.Replace("##AFFILIATIONTYPEDURATION" + item.id + "##", string.Empty);
                    collegeAffiliationType = collegeAffiliationType.Replace("##AFFILIATIONTYPEIMAGE" + item.id + "##", string.Format("<img src='" + serverURL + "/Content/Images/checkbox_{0}.png' height='10' />&nbsp;{1} &nbsp;&nbsp;<img src='" + serverURL + "/Content/Images/checkbox_{2}.png' height='10' />&nbsp;{3}", yes, "Yes", no, "No</td>"));
                }
            }
            if (affiliationNAAC == "NAAC")
            {
                string image = string.Empty;
                int nackid = db.jntuh_affiliation_type.Where(at => at.affiliationType == "NAAC").Select(at => at.id).FirstOrDefault();

                var nackatype = db.jntuh_college_affiliation.Where(at => at.affiliationTypeId == nackid && at.collegeId == collegeId).Select(at => at).FirstOrDefault();
                if (nackatype != null)
                {
                    if (nackatype.affiliationFromDate != null && nackatype.affiliationToDate != null)
                    {
                        image = string.Format("<img src='" + serverURL + "/Content/Images/checkbox_yes_b.png' height='10' />&nbsp;Conferred&nbsp;<img src='" + serverURL + "/Content/Images/checkbox_no_b.png' height='10' />&nbsp;Applied&nbsp;&nbsp;<br/><img src='" + serverURL + "/Content/Images/checkbox_no_b.png' height='10' />&nbsp;Not Yet Applied</td>");
                    }
                    else if (nackatype.affiliationStatus == "Applied")
                    {
                        image = string.Format("<img src='" + serverURL + "/Content/Images/checkbox_no_b.png' height='10' />&nbsp;Conferred&nbsp;<img src='" + serverURL + "/Content/Images/checkbox_yes_b.png' height='10' />&nbsp;Applied&nbsp;&nbsp;<br/><img src='" + serverURL + "/Content/Images/checkbox_no_b.png' height='10' />&nbsp;Not Yet Applied</td>");
                    }
                    else
                    {
                        image = string.Format("<img src='" + serverURL + "/Content/Images/checkbox_no_b.png' height='10' />&nbsp;Conferred&nbsp;<img src='" + serverURL + "/Content/Images/checkbox_no_b.png' height='10' />&nbsp;Applied&nbsp;&nbsp;<br/><img src='" + serverURL + "/Content/Images/checkbox_yes_b.png' height='10' />&nbsp;Not Yet Applied</td>");
                    }
                }
                else
                {
                    image = string.Format("<img src='" + serverURL + "/Content/Images/checkbox_no_b.png' height='10' />&nbsp;Conferred&nbsp;<img src='" + serverURL + "/Content/Images/checkbox_no_b.png' height='10' />&nbsp;Applied&nbsp;&nbsp;<br/><img src='" + serverURL + "/Content/Images/checkbox_yes_b.png' height='10' />&nbsp;Not Yet Applied</td>");

                }
                collegeAffiliationType += "<tr>";
                if (nackatype != null)
                {
                    collegeAffiliationType += "<td valign='top' colspan='4'>NAAC</td>";
                    collegeAffiliationType += "<td valign='top' colspan='1' align='center'>:</td>";
                    collegeAffiliationType += "<td valign='top' colspan='5'>";
                    collegeAffiliationType += image;
                    collegeAffiliationType += "<td valign='top' colspan='4'>If Conferred,Period </td>";
                    collegeAffiliationType += "<td valign='top' colspan='1' align='center'>:</td>";
                    collegeAffiliationType += "<td valign='top' colspan='5'>From: " + UAAAS.Models.Utilities.MMDDYY2DDMMYY(nackatype.affiliationFromDate.ToString()).ToString() + "<br/>";
                    collegeAffiliationType += "To:&nbsp; " + UAAAS.Models.Utilities.MMDDYY2DDMMYY(nackatype.affiliationToDate.ToString()) + "<br/>";
                    collegeAffiliationType += "Duration: " + nackatype.affiliationDuration + "<br/>";
                    collegeAffiliationType += "Grade: " + nackatype.affiliationGrade + "<br/>";
                    collegeAffiliationType += "CGPA: " + nackatype.CGPA + "</td>";
                }
                else
                {
                    collegeAffiliationType += "<td valign='top' colspan='4'>NAAC</td>";
                    collegeAffiliationType += "<td valign='top' colspan='1' align='center'>:</td>";
                    collegeAffiliationType += "<td valign='top' colspan='5'>";
                    collegeAffiliationType += image;
                    collegeAffiliationType += "<td valign='top' colspan='4'>If Conferred,Period </td>";
                    collegeAffiliationType += "<td valign='top' colspan='1' align='center'>:</td>";
                    collegeAffiliationType += "<td valign='top' colspan='5'>From: <br/>";
                    collegeAffiliationType += "To:&nbsp; <br/>";
                    collegeAffiliationType += "Duration: <br/>";
                    collegeAffiliationType += "Grade:<br/>";
                    collegeAffiliationType += "CGPA: </td>";
                }

                collegeAffiliationType += "</tr>";
                collegeAffiliationType += "<br />";
            }
            contents = contents.Replace("##COLLEGE_AFFILIATIONTYPES##", collegeAffiliationType);

            #endregion

            #region from jntuh_college_degree table

            string strCollegeDegree = string.Empty;
            strCollegeDegree += "<table border='0' cellspacing='0' cellpadding='0'><tbody><tr>";
            List<jntuh_degree> collegeDegree = db.jntuh_degree.OrderBy(degree => degree.degreeDisplayOrder).Where(degree => degree.isActive == true).ToList();
            int count = 0;
            foreach (var item in collegeDegree)
            {
                strCollegeDegree += "<td width='10%'>" + string.Format("{0}&nbsp; {1}", "##COLLEGEDEGREEIMAGE" + item.id + "##", item.degree) + "</td>";
                count++;
                if (count % 5 == 0)
                {
                    strCollegeDegree += "</tr>";
                }
            }
            List<jntuh_college_degree> collegeDegrees = db.jntuh_college_degree.Where(degree => degree.isActive == true && degree.collegeId == collegeId).ToList();
            foreach (var degrees in collegeDegrees)
            {
                strCollegeDegree = strCollegeDegree.Replace("##COLLEGEDEGREEIMAGE" + degrees.degreeId + "##", string.Format("<img src='" + serverURL + "/Content/Images/checkbox_{0}.png' height='10' />", "yes_b"));
            }
            foreach (var item in collegeDegree)
            {
                strCollegeDegree = strCollegeDegree.Replace("##COLLEGEDEGREEIMAGE" + item.id + "##", string.Format("<img src='" + serverURL + "/Content/Images/checkbox_{0}.png' height='10' />", "no_b"));
            }
            strCollegeDegree += "</tbody></table>";
            contents = contents.Replace("##COLLEGE_DEGREE##", strCollegeDegree);
            #endregion
            return contents;
        }

        public string PrincipalDirectorDetails(int collegeId, string contents)
        {
            string strPrincipal = string.Empty;
            string principalReson = string.Empty;
            var actualYear = db.jntuh_academic_year.Where(q => q.isPresentAcademicYear == true && q.isActive == true).Select(w => w.actualYear).FirstOrDefault();
            var PresentYearId = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(z => z.id).FirstOrDefault();

            ////compaliance Principal Details
            var complaincereg = db.jntuh_appeal_principal_registered.Where(r => r.academicYearId == PresentYearId && r.collegeId == collegeId && r.NOtificationReport != null).Select(r => r.RegistrationNumber).FirstOrDefault();
            if (!string.IsNullOrEmpty(complaincereg))
            {
                var PrincipalDetails = db.jntuh_registered_faculty.Where(r => r.RegistrationNumber == complaincereg).Select(r => r).FirstOrDefault();
                if (PrincipalDetails != null)
                {
                    var education = db.jntuh_registered_faculty_education.Where(e => e.facultyId == PrincipalDetails.id).OrderByDescending(e => e.id).Select(e => e).FirstOrDefault();
                    if (!string.IsNullOrEmpty(PrincipalDetails.DeactivationReason))
                        principalReson = PrincipalDetails.DeactivationReason;

                    if (PrincipalDetails.BAS == "Yes")
                    {
                        if (!String.IsNullOrEmpty(principalReson))
                            principalReson += ",Not Fulfilling Biometric Attendance";
                        else
                            principalReson += "Not Fulfilling Biometric Attendance";
                    }
                    if (string.IsNullOrEmpty(principalReson))
                        strPrincipal += "<p><strong><u>1.Details of Compliance Principal</u></strong></p><br />";
                    else
                        strPrincipal += "<p><strong><u>Details of Compeliance Principal:</u></strong> (" + principalReson + ")</p><br />";

                    strPrincipal += "<table border='1' cellspacing='0' cellpadding='5'>";
                    strPrincipal += "<tbody>";
                    strPrincipal += "<tr>";
                    strPrincipal += "<td valign='top' colspan='4'>Registration Number</td>";
                    strPrincipal += "<td valign='top' colspan='1' align='center'>:</td>";
                    strPrincipal += "<td colspan='15' valign='top'>" + PrincipalDetails.RegistrationNumber + "</td>";
                    strPrincipal += "</tr>";

                    strPrincipal += "<tr>";
                    strPrincipal += "<td valign='top' colspan='4'>First Name</td>";
                    strPrincipal += "<td valign='top' colspan='1' align='center'>:</td>";
                    strPrincipal += "<td valign='top' colspan='5'>" + PrincipalDetails.FirstName + "</td>";
                    strPrincipal += "<td valign='top' colspan='4'>Last Name</td>";
                    strPrincipal += "<td valign='top' colspan='1' align='center'>:</td>";
                    strPrincipal += "<td valign='top' colspan='5'>" + PrincipalDetails.MiddleName + "</td>";
                    strPrincipal += "</tr>";

                    strPrincipal += "<tr>";
                    strPrincipal += "<td valign='top' colspan='4'>Surname</td>";
                    strPrincipal += "<td valign='top' colspan='1' align='center'>:</td>";
                    strPrincipal += "<td valign='top' colspan='5'>" + PrincipalDetails.LastName + "</td>";

                    strPrincipal += "<td valign='top' colspan='4'>Mobile (+91)</td>";
                    strPrincipal += "<td valign='top' colspan='1' align='center'>:</td>";
                    strPrincipal += "<td valign='top' colspan='5'>" + PrincipalDetails.Mobile + "</td>";
                    strPrincipal += "</tr>";

                    strPrincipal += "<tr>";
                    strPrincipal += "<td valign='top' colspan='4'>Email</td>";
                    strPrincipal += "<td valign='top' colspan='1' align='center'>:</td>";
                    strPrincipal += "<td colspan='15' valign='top'>" + PrincipalDetails.Email + "</td>";
                    strPrincipal += "</tr>";

                    strPrincipal += "</tbody>";
                    strPrincipal += "</table>";
                    strPrincipal += "<br />";
                }
            }
            ////Reverification Principal Details
            var regNo = db.jntuh_appeal_principal_registered.Where(r => r.academicYearId == PresentYearId && r.collegeId == collegeId && r.NOtificationReport == null).Select(r => r.RegistrationNumber).FirstOrDefault();
            var principalregNo = db.jntuh_college_principal_registered.Where(r => r.collegeId == collegeId).Select(r => r.RegistrationNumber).FirstOrDefault();
            if (!string.IsNullOrEmpty(regNo))
            {
                var PrincipalDetails = db.jntuh_registered_faculty.Where(r => r.RegistrationNumber == regNo).Select(r => r).FirstOrDefault();
                if (PrincipalDetails != null)
                {
                    var education = db.jntuh_registered_faculty_education.Where(e => e.facultyId == PrincipalDetails.id).OrderByDescending(e => e.id).Select(e => e).FirstOrDefault();
                    if (!string.IsNullOrEmpty(PrincipalDetails.DeactivationReason))
                        principalReson = PrincipalDetails.DeactivationReason;

                    if (PrincipalDetails.BAS == "Yes")
                    {
                        if (!String.IsNullOrEmpty(principalReson))
                            principalReson += ",Not Fulfilling Biometric Attendance";
                        else
                            principalReson += "Not Fulfilling Biometric Attendance";
                    }

                    if (string.IsNullOrEmpty(principalReson))
                        strPrincipal += "<p><strong><u>1.Details of Principal</u></strong></p><br />";
                    else
                        strPrincipal += "<p><strong><u>Details of Principal:</u></strong> (" + principalReson + ")</p><br />";

                    strPrincipal += "<table border='1' cellspacing='0' cellpadding='5'>";
                    strPrincipal += "<tbody>";
                    strPrincipal += "<tr>";
                    strPrincipal += "<td valign='top' colspan='4'>Registration Number</td>";
                    strPrincipal += "<td valign='top' colspan='1' align='center'>:</td>";
                    strPrincipal += "<td colspan='15' valign='top'>" + PrincipalDetails.RegistrationNumber + "</td>";
                    strPrincipal += "</tr>";

                    strPrincipal += "<tr>";
                    strPrincipal += "<td valign='top' colspan='4'>First Name</td>";
                    strPrincipal += "<td valign='top' colspan='1' align='center'>:</td>";
                    strPrincipal += "<td valign='top' colspan='5'>" + PrincipalDetails.FirstName + "</td>";
                    strPrincipal += "<td valign='top' colspan='4'>Last Name</td>";
                    strPrincipal += "<td valign='top' colspan='1' align='center'>:</td>";
                    strPrincipal += "<td valign='top' colspan='5'>" + PrincipalDetails.MiddleName + "</td>";
                    strPrincipal += "</tr>";

                    strPrincipal += "<tr>";
                    strPrincipal += "<td valign='top' colspan='4'>Surname</td>";
                    strPrincipal += "<td valign='top' colspan='1' align='center'>:</td>";
                    strPrincipal += "<td valign='top' colspan='5'>" + PrincipalDetails.LastName + "</td>";

                    strPrincipal += "<td valign='top' colspan='4'>Mobile (+91)</td>";
                    strPrincipal += "<td valign='top' colspan='1' align='center'>:</td>";
                    strPrincipal += "<td valign='top' colspan='5'>" + PrincipalDetails.Mobile + "</td>";
                    strPrincipal += "</tr>";

                    strPrincipal += "<tr>";
                    strPrincipal += "<td valign='top' colspan='4'>Email</td>";
                    strPrincipal += "<td valign='top' colspan='1' align='center'>:</td>";
                    strPrincipal += "<td colspan='15' valign='top'>" + PrincipalDetails.Email + "</td>";
                    strPrincipal += "</tr>";

                    strPrincipal += "<tr>";
                    strPrincipal += "</tr>";
                    strPrincipal += "</tbody>";
                    strPrincipal += "</table>";
                    strPrincipal += "<br />";
                }
                else
                {
                    strPrincipal += "<p><strong><u>1.Details of Principal:</u></strong> (PRINCIPAL DETAILS ARE NOT UPLOADED)</p><br />";
                    strPrincipal += "<table border='1' cellspacing='0' cellpadding='5'>";
                    strPrincipal += "<tbody>";
                    strPrincipal += "<tr>";
                    strPrincipal += "<td valign='top' colspan='4'>Registration Number</td>";
                    strPrincipal += "<td valign='top' colspan='1' align='center'>:</td>";
                    strPrincipal += "<td colspan='15' valign='top'></td>";
                    strPrincipal += "</tr>";

                    strPrincipal += "<tr>";
                    strPrincipal += "<td valign='top' colspan='4'>First Name</td>";
                    strPrincipal += "<td valign='top' colspan='1' align='center'>:</td>";
                    strPrincipal += "<td valign='top' colspan='5'></td>";
                    strPrincipal += "<td valign='top' colspan='4'>Last Name</td>";
                    strPrincipal += "<td valign='top' colspan='1' align='center'>:</td>";
                    strPrincipal += "<td valign='top' colspan='5'></td>";
                    strPrincipal += "</tr>";

                    strPrincipal += "<tr>";
                    strPrincipal += "<td valign='top' colspan='4'>Surname</td>";
                    strPrincipal += "<td valign='top' colspan='1' align='center'>:</td>";
                    strPrincipal += "<td valign='top' colspan='5'></td>";
                    strPrincipal += "<td valign='top' colspan='4'>Mobile (+91)</td>";
                    strPrincipal += "<td valign='top' colspan='1' align='center'>:</td>";
                    strPrincipal += "<td valign='top' colspan='15'></td>";
                    strPrincipal += "</tr>";

                    strPrincipal += "<tr>";
                    strPrincipal += "<td valign='top' colspan='4'>Email</td>";
                    strPrincipal += "<td valign='top' colspan='1' align='center'>:</td>";
                    strPrincipal += "<td colspan='15' valign='top'></td>";
                    strPrincipal += "</tr>";


                    strPrincipal += "<tr>";
                    strPrincipal += "<td valign='top' colspan='4'>Photo</td>";
                    strPrincipal += "<td valign='top' colspan='1' align='left'>:</td>";
                    strPrincipal += "<td colspan='5' valign='top'></td>";
                    strPrincipal += "</tr>";

                    strPrincipal += "</tbody>";
                    strPrincipal += "</table>";
                    strPrincipal += "<br />";
                }
            }

            if (string.IsNullOrEmpty(complaincereg) && string.IsNullOrEmpty(regNo))
            {
                if (!(string.IsNullOrEmpty(principalregNo)))
                {
                    var PrincipalDetails = db.jntuh_registered_faculty.Where(r => r.RegistrationNumber == principalregNo).Select(r => r).FirstOrDefault();
                    if (!string.IsNullOrEmpty(PrincipalDetails.DeactivationReason))
                        principalReson = PrincipalDetails.DeactivationReason;

                    if (PrincipalDetails.BAS == "Yes")
                    {
                        if (!String.IsNullOrEmpty(principalReson))
                            principalReson += ",Not Fulfilling Biometric Attendance";
                        else
                            principalReson += "Not Fulfilling Biometric Attendance";
                    }

                    if (string.IsNullOrEmpty(principalReson))
                        strPrincipal += "<p><strong><u>1.Details of Principal</u></strong></p><br />";
                    else
                        strPrincipal += "<p><strong><u>1.Details of Principal:</u></strong> (" + principalReson + ")</p><br />";

                    strPrincipal += "<table border='1' cellspacing='0' cellpadding='5'>";
                    strPrincipal += "<tbody>";
                    strPrincipal += "<tr>";
                    strPrincipal += "<td valign='top' colspan='4'>Registration Number</td>";
                    strPrincipal += "<td valign='top' colspan='1' align='center'>:</td>";
                    strPrincipal += "<td colspan='15' valign='top'>" + PrincipalDetails.RegistrationNumber + "</td>";
                    strPrincipal += "</tr>";

                    strPrincipal += "<tr>";
                    strPrincipal += "<td valign='top' colspan='4'>First Name</td>";
                    strPrincipal += "<td valign='top' colspan='1' align='center'>:</td>";
                    strPrincipal += "<td valign='top' colspan='5'>" + PrincipalDetails.FirstName + "</td>";
                    strPrincipal += "<td valign='top' colspan='4'>Last Name</td>";
                    strPrincipal += "<td valign='top' colspan='1' align='center'>:</td>";
                    strPrincipal += "<td valign='top' colspan='5'>" + PrincipalDetails.MiddleName + "</td>";
                    strPrincipal += "</tr>";

                    strPrincipal += "<tr>";
                    strPrincipal += "<td valign='top' colspan='4'>Surname</td>";
                    strPrincipal += "<td valign='top' colspan='1' align='center'>:</td>";
                    strPrincipal += "<td valign='top' colspan='5'>" + PrincipalDetails.LastName + "</td>";

                    strPrincipal += "<td valign='top' colspan='4'>Mobile (+91)</td>";
                    strPrincipal += "<td valign='top' colspan='1' align='center'>:</td>";
                    strPrincipal += "<td valign='top' colspan='5'>" + PrincipalDetails.Mobile + "</td>";
                    strPrincipal += "</tr>";

                    strPrincipal += "<tr>";
                    strPrincipal += "<td valign='top' colspan='4'>Email</td>";
                    strPrincipal += "<td valign='top' colspan='1' align='center'>:</td>";
                    strPrincipal += "<td colspan='15' valign='top'>" + PrincipalDetails.Email + "</td>";
                    strPrincipal += "</tr>";

                    strPrincipal += "<tr>";
                    strPrincipal += "</tr>";
                    strPrincipal += "</tbody>";
                    strPrincipal += "</table>";
                    strPrincipal += "<br />";
                }
            }
            contents = contents.Replace("##PRINCIPAL##", strPrincipal);
            return contents;
        }

        public string ExistingIntakeDetails(int collegeId, string contents)
        {
            // collegeId = 186;
            string strExistingIntakeDetails = string.Empty;
            int sno = 0;
            int totalApprovedIntake1 = 0;
            int totalApprovedIntake2 = 0;
            int totalApprovedIntake3 = 0;
            int totalApprovedIntake4 = 0;
            int totalApprovedIntake5 = 0;
            int totalAdmittedIntake1 = 0;
            int totalAdmittedIntake2 = 0;
            int totalAdmittedIntake3 = 0;
            int totalAdmittedIntake4 = 0;
            int totalAdmittedIntake5 = 0;
            int totalApproved = 0;
            int totalAdmited = 0;
            int totalPAYApproved = 0;
            int totalPAYAdmited = 0;
            int totalPAYProposed = 0;

            ViewBag.AcademicYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.academicYear).FirstOrDefault();
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            string FirstYear = String.Format("{0}-{1}", (actualYear).ToString(), (actualYear + 1).ToString().Substring(2, 2));
            string SecondYear = String.Format("{0}-{1}", (actualYear - 1).ToString(), (actualYear).ToString().Substring(2, 2));
            string ThirdYear = String.Format("{0}-{1}", (actualYear - 2).ToString(), (actualYear - 1).ToString().Substring(2, 2));
            string FourthYear = String.Format("{0}-{1}", (actualYear - 3).ToString(), (actualYear - 2).ToString().Substring(2, 2));
            string FifthYear = String.Format("{0}-{1}", (actualYear - 4).ToString(), (actualYear - 3).ToString().Substring(2, 2));

            int presentYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int AY1 = db.jntuh_academic_year.Where(a => a.actualYear == (presentYear)).Select(a => a.id).FirstOrDefault();
            int AY2 = db.jntuh_academic_year.Where(a => a.actualYear == (presentYear - 1)).Select(a => a.id).FirstOrDefault();
            int AY3 = db.jntuh_academic_year.Where(a => a.actualYear == (presentYear - 2)).Select(a => a.id).FirstOrDefault();
            int AY4 = db.jntuh_academic_year.Where(a => a.actualYear == (presentYear - 3)).Select(a => a.id).FirstOrDefault();
            int AY5 = db.jntuh_academic_year.Where(a => a.actualYear == (presentYear - 4)).Select(a => a.id).FirstOrDefault();

            int PAY = db.jntuh_academic_year.Where(a => a.actualYear == (presentYear + 1)).Select(a => a.id).FirstOrDefault();

            List<jntuh_appeal_college_intake_existing> intakeappealintake = db.jntuh_appeal_college_intake_existing.Where(i => i.collegeId == collegeId).ToList();
            List<jntuh_college_intake_existing> intake = db.jntuh_college_intake_existing.Where(i => i.collegeId == collegeId && i.academicYearId == PAY && i.proposedIntake != 0).ToList();
            List<CollegeIntakeExisting> collegeIntakeExisting = new List<CollegeIntakeExisting>();
            var jntuh_specialization = db.jntuh_specialization;
            var jntuh_department = db.jntuh_department;
            var jntuh_degree = db.jntuh_degree;
            var jntuh_shift = db.jntuh_shift;
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
                newIntake.Specialization = jntuh_specialization.Where(s => s.id == item.specializationId).Select(s => s.specializationName).FirstOrDefault();
                newIntake.DepartmentID = jntuh_specialization.Where(s => s.id == item.specializationId).Select(s => s.departmentId).FirstOrDefault();
                newIntake.Department = jntuh_department.Where(d => d.id == newIntake.DepartmentID).Select(d => d.departmentName).FirstOrDefault();
                newIntake.degreeID = jntuh_department.Where(d => d.id == newIntake.DepartmentID).Select(d => d.degreeId).FirstOrDefault();
                newIntake.Degree = jntuh_degree.Where(d => d.id == newIntake.degreeID).Select(d => d.degree).FirstOrDefault();
                newIntake.degreeDisplayOrder = jntuh_degree.Where(d => d.id == newIntake.degreeID).Select(d => d.degreeDisplayOrder).FirstOrDefault();
                newIntake.shiftId = item.shiftId;
                newIntake.Shift = jntuh_shift.Where(s => s.id == item.shiftId).Select(s => s.shiftName).FirstOrDefault();
                collegeIntakeExisting.Add(newIntake);
            }

            collegeIntakeExisting = collegeIntakeExisting.AsEnumerable().GroupBy(r => new { r.specializationId, r.shiftId }).Select(r => r.FirstOrDefault()).ToList();
            collegeIntakeExisting = collegeIntakeExisting.OrderBy(ei => ei.degreeDisplayOrder).ThenBy(ei => ei.Department).ThenBy(ei => ei.Specialization).ThenBy(ei => ei.shiftId).ToList();
            if (collegeIntakeExisting.Count() != 0)
            {
                strExistingIntakeDetails += "<table border='1' cellspacing='0' cellpadding='5' style='font-size: 8px;'>";
                strExistingIntakeDetails += "<tbody>";
                strExistingIntakeDetails += "<tr>";
                strExistingIntakeDetails += "<td width='28' rowspan='3' colspan='1'><p align='center'>S.No</p></td>";
                strExistingIntakeDetails += "<td width='56' rowspan='3' colspan='3'><p align='left'>Degree</p><p align='left'>*</p></td>";
                strExistingIntakeDetails += "<td width='63' rowspan='3' colspan='4'><p align='left'>Department</p><p align='left'>**</p></td>";
                strExistingIntakeDetails += "<td width='200' rowspan='3' colspan='4'><p align='left'>Specialization</p><p align='left'>***</p></td>";
                strExistingIntakeDetails += "<td width='500' colspan='10'><p align='center'>Sanctioned & Actual Admitted Intake as per Academic Year</p></td>";
                strExistingIntakeDetails += "<td  rowspan='3' colspan='1'><p align='left'>PI</p><p align='left'></p></td>";
                strExistingIntakeDetails += "<td  rowspan='3' colspan='1'><p align='left'>CS</p><p align='left'></p></td>";
                strExistingIntakeDetails += "<td width='100' colspan='4' valign='top'><p align='center'>NBA accreditation Period (if exists)</p></td></tr>";
                strExistingIntakeDetails += "<tr><td width='100' colspan='2'><p align='center'>" + FifthYear + "</p></td>";
                strExistingIntakeDetails += "<td width='100' colspan='2'><p align='center'>" + FourthYear + "</p></td>";
                strExistingIntakeDetails += "<td width='100' colspan='2'><p align='center'>" + ThirdYear + "</p></td>";
                strExistingIntakeDetails += "<td width='100' colspan='2'><p align='center'>" + SecondYear + "</p></td>";
                strExistingIntakeDetails += "<td width='100' colspan='2'><p align='center'>" + FirstYear + "</p></td>";

                strExistingIntakeDetails += "<td width='100' colspan='4' valign='top'><p align='center' style='font-style: 7px;'>(DD/MM/YYY)</p></td>";
                strExistingIntakeDetails += "</tr>";
                strExistingIntakeDetails += "<tr style='font-size: 7px;'>";
                strExistingIntakeDetails += "<td width='50' colspan='1'><p align='center'>S</p></td>";
                strExistingIntakeDetails += "<td width='50' colspan='1'><p align='center'>A</p></td>";
                strExistingIntakeDetails += "<td width='50' colspan='1'><p align='center'>S</p></td>";
                strExistingIntakeDetails += "<td width='50' colspan='1'><p align='center'>A</p></td>";
                strExistingIntakeDetails += "<td width='50' colspan='1'><p align='center'>S</p></td>";
                strExistingIntakeDetails += "<td width='50' colspan='1'><p align='center'>A</p></td>";
                strExistingIntakeDetails += "<td width='50' colspan='1'><p align='center'>S</p></td>";
                strExistingIntakeDetails += "<td width='50' colspan='1'><p align='center'>A</p></td>";
                strExistingIntakeDetails += "<td width='50' colspan='1'><p align='center'>S</p></td>";
                strExistingIntakeDetails += "<td width='50' colspan='1'><p align='center'>A</p></td>";
                strExistingIntakeDetails += "<td width='50' colspan='2' valign='top'><p align='center'>From</p></td>";
                strExistingIntakeDetails += "<td width='50' colspan='2' valign='top'><p align='center'>To</p></td>";
                strExistingIntakeDetails += "</tr>";

                foreach (var item in collegeIntakeExisting)
                {
                    sno++;

                    if (item.nbaFrom != null)
                        item.nbaFromDate = UAAAS.Models.Utilities.MMDDYY2DDMMYY(item.nbaFrom.ToString());
                    if (item.nbaTo != null)
                        item.nbaToDate = UAAAS.Models.Utilities.MMDDYY2DDMMYY(item.nbaTo.ToString());
                    item.ProposedIntake = intake.Where(i => i.collegeId == collegeId && i.academicYearId == PAY && i.specializationId == item.specializationId && i.shiftId == item.shiftId).Select(i => i.proposedIntake).FirstOrDefault();
                    if (item.ProposedIntake != null)
                        totalPAYProposed += (int)item.ProposedIntake;
                    item.courseStatus = intake.Where(i => i.collegeId == collegeId && i.academicYearId == PAY && i.specializationId == item.specializationId && i.shiftId == item.shiftId).Select(i => i.courseStatus).FirstOrDefault();

                    if (item.courseStatus == "Closure")
                        item.courseStatus = "C";
                    else if (item.courseStatus == "New")
                        item.courseStatus = "N";
                    else if (item.courseStatus == "Increase")
                        item.courseStatus = "I";
                    else if (item.courseStatus == "Nochange")
                        item.courseStatus = "NC";
                    else if (item.courseStatus == "Decrease")
                        item.courseStatus = "D";
                    else
                        item.courseStatus = "";

                    item.approvedIntake1 = GetIntake(collegeId, AY1, item.specializationId, item.shiftId, 1);
                    item.admittedIntake1 = GetIntake(collegeId, AY1, item.specializationId, item.shiftId, 0);
                    totalApprovedIntake1 += item.approvedIntake1;
                    totalAdmittedIntake1 += item.admittedIntake1;

                    item.approvedIntake2 = GetIntake(collegeId, AY2, item.specializationId, item.shiftId, 1);
                    item.admittedIntake2 = GetIntake(collegeId, AY2, item.specializationId, item.shiftId, 0);
                    totalApprovedIntake2 += item.approvedIntake2;
                    totalAdmittedIntake2 += item.admittedIntake2;

                    item.approvedIntake3 = GetIntake(collegeId, AY3, item.specializationId, item.shiftId, 1);
                    item.admittedIntake3 = GetIntake(collegeId, AY3, item.specializationId, item.shiftId, 0);
                    totalApprovedIntake3 += item.approvedIntake3;
                    totalAdmittedIntake3 += item.admittedIntake3;

                    item.approvedIntake4 = GetIntake(collegeId, AY4, item.specializationId, item.shiftId, 1);
                    item.admittedIntake4 = GetIntake(collegeId, AY4, item.specializationId, item.shiftId, 0);
                    totalApprovedIntake4 += item.approvedIntake4;
                    totalAdmittedIntake4 += item.admittedIntake4;

                    item.approvedIntake5 = GetIntake(collegeId, AY5, item.specializationId, item.shiftId, 1);
                    item.admittedIntake5 = GetIntake(collegeId, AY5, item.specializationId, item.shiftId, 0);
                    totalApprovedIntake5 += item.approvedIntake5;
                    totalAdmittedIntake5 += item.admittedIntake5;
                    string fontbold = string.Empty;
                    int? appealproposedintake = 0;
                    appealproposedintake = intakeappealintake.Where(i => i.collegeId == item.collegeId && i.specializationId == item.specializationId && i.shiftId == item.shiftId).Select(s => s.proposedIntake).FirstOrDefault();
                    //bold
                    if (appealproposedintake != null)
                    {
                        if (appealproposedintake == item.ProposedIntake)
                        { }
                        else
                            fontbold = "bold";
                    }

                    strExistingIntakeDetails += "<tr style='font-weight: " + fontbold + "'>";
                    strExistingIntakeDetails += "<td colspan='1' width='28'><p align='center'>" + sno + "</p></td>";
                    strExistingIntakeDetails += "<td colspan='3' width='56'>" + item.Degree + "</td>";
                    strExistingIntakeDetails += "<td colspan='4' width='63'>" + item.Department + "</td>";
                    strExistingIntakeDetails += "<td colspan='4' width='200'>" + item.Specialization + "</td>";
                    //  strExistingIntakeDetails += "<td colspan='1' width='42'>" + item.Shift + "</td>";
                    strExistingIntakeDetails += "<td colspan='1' width='50'>" + item.approvedIntake5.ToString() + "</td>";
                    strExistingIntakeDetails += "<td colspan='1' width='50'>" + item.admittedIntake5.ToString() + "</td>";
                    strExistingIntakeDetails += "<td colspan='1' width='50'>" + item.approvedIntake4.ToString() + "</td>";
                    strExistingIntakeDetails += "<td colspan='1' width='50'>" + item.admittedIntake4.ToString() + "</td>";
                    strExistingIntakeDetails += "<td colspan='1' width='50'>" + item.approvedIntake3.ToString() + "</td>";
                    strExistingIntakeDetails += "<td colspan='1' width='50'>" + item.admittedIntake3.ToString() + "</td>";
                    strExistingIntakeDetails += "<td colspan='1' width='50'>" + item.approvedIntake2.ToString() + "</td>";
                    strExistingIntakeDetails += "<td colspan='1' width='50'>" + item.admittedIntake2.ToString() + "</td>";
                    strExistingIntakeDetails += "<td colspan='1' width='42'>" + item.approvedIntake1.ToString() + "</td>";
                    strExistingIntakeDetails += "<td colspan='1' width='50'>" + item.admittedIntake1.ToString() + "</td>";
                    if (appealproposedintake != null)
                    {
                        strExistingIntakeDetails += "<td colspan='1'>" + appealproposedintake + "</td>";
                    }
                    else
                    {
                        strExistingIntakeDetails += "<td colspan='1'>" + item.ProposedIntake + "</td>";
                    }
                    strExistingIntakeDetails += "<td colspan='1'>" + item.courseStatus + "</td>";
                    strExistingIntakeDetails += "<td colspan='2' style='font-size: 8px;' width='50'>" + item.nbaFromDate + "</td>";
                    strExistingIntakeDetails += "<td colspan='2' style='font-size: 8px;' width='50'>" + item.nbaToDate + "</td>";
                    strExistingIntakeDetails += "</tr>";
                    if (item.Degree == "Pharm.D PB")//6
                    {
                        totalAdmited += item.admittedIntake1 + item.admittedIntake2 + item.admittedIntake3 + item.admittedIntake4 + item.admittedIntake5 + item.admittedIntake6;
                        totalApproved += item.approvedIntake1 + item.approvedIntake2 + item.approvedIntake3 + item.approvedIntake4 + item.approvedIntake5 + item.approvedIntake6;
                    }
                    else if (item.Degree == "MAM" || item.Degree == "MTM" || item.Degree == "Pharm.D")//5
                    {
                        totalAdmited += item.admittedIntake1 + item.admittedIntake2 + item.admittedIntake3 + item.admittedIntake4 + item.admittedIntake5;
                        totalApproved += item.approvedIntake1 + item.approvedIntake2 + item.approvedIntake3 + item.approvedIntake4 + item.approvedIntake5;
                    }
                    else if (item.Degree == "B.Tech" || item.Degree == "B.Pharmacy")//4
                    {
                        totalAdmited += item.admittedIntake1 + item.admittedIntake2 + item.admittedIntake3 + item.admittedIntake4;
                        totalApproved += item.approvedIntake1 + item.approvedIntake2 + item.approvedIntake3 + item.approvedIntake4;
                    }
                    else if (item.Degree == "MCA")//3
                    {
                        totalAdmited += item.admittedIntake1 + item.admittedIntake2 + item.admittedIntake3;
                        totalApproved += item.approvedIntake1 + item.approvedIntake2 + item.approvedIntake3;
                    }
                    else if (item.Degree == "M.Tech" || item.Degree == "M.Pharmacy" || item.Degree == "MBA") //2
                    {
                        totalAdmited += item.admittedIntake1 + item.admittedIntake2;
                        totalApproved += item.approvedIntake1 + item.approvedIntake2;
                    }
                }

                strExistingIntakeDetails += "<tr><td width='337' colspan='12'><p align='right'>Total =</p></td><td width='50' colspan='1' align='center'>" + totalApprovedIntake5 + "</td><td width='50' colspan='1' align='center'>" + totalAdmittedIntake5 + "</td><td width='50' colspan='1' align='center'>" + totalApprovedIntake4 + "</td><td width='50' colspan='1' align='center'>" + totalAdmittedIntake4 + "</td><td width='50' colspan='1' align='center'>" + totalApprovedIntake3 + "</td><td width='50' colspan='1' align='center'>" + totalAdmittedIntake3 + "</td><td width='50' colspan='1' align='center'>" + totalApprovedIntake2 + "</td><td width='50' colspan='1' align='center'>" + totalAdmittedIntake2 + "</td><td width='50' colspan='1' align='center'>" + totalApprovedIntake1 + "</td><td width='50' colspan='1' align='center'>" + totalAdmittedIntake1 + "</td><td width='50' colspan='1' align='center'>" + totalPAYProposed + "</td><td width='50' colspan='1' align='center'>" + "-" + "</td><td width='50' colspan='2' valign='top' align='center'></td><td width='50' colspan='2' valign='top' align='center'></td></tr>";
                strExistingIntakeDetails += "<tr><td colspan='12'><p align='right'>Total Admitted / Total Sanctioned =</p></td><td colspan='16' width='600'>" + totalAdmited + '/' + totalApproved + "</td></tr>";
                strExistingIntakeDetails += "</tbody></table>";
                contents = contents.Replace("##ExistingIntakeDetails##", strExistingIntakeDetails);
            }
            else
            {
                contents = contents.Replace("##ExistingIntakeDetails##", "<table border='1' cellspacing='0' cellpadding='5' style='font-size: 8px;'><tr><td style='text-align:center'>No Data Available</td></tr></table>");
            }
            return contents;
        }

        private string collegeFacultyComplianceMembers(int collegeId, string contents)
        {
            string collegeFaculty = string.Empty;
            int count = 1;
            string gender = string.Empty;
            string category = string.Empty;
            string department = string.Empty;
            string designation = string.Empty;
            string qualification = string.Empty;
            string dateOfAppointment = string.Empty;
            string teachingType = string.Empty;
            string ratified = string.Empty;

            var actualYear = db.jntuh_academic_year.Where(q => q.isPresentAcademicYear == true && q.isActive == true).Select(w => w.actualYear).FirstOrDefault();
            var PresentYearId = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(z => z.id).FirstOrDefault();

            #region TeachingFacultyLogic Begin
            List<CollegeFaculty> teachingFaculty = new List<CollegeFaculty>();
            List<jntuh_appeal_faculty_registered> regFaculty = db.jntuh_appeal_faculty_registered.Where(e => e.academicYearId == PresentYearId && e.collegeId == collegeId && e.NOtificationReport != null).Select(e => e).ToList();
            var AppealReverificationFacultyRegNos = regFaculty.Select(q => q.RegistrationNumber).Distinct().ToList();
            var jntuh_registered_facultys = db.jntuh_registered_faculty.Where(a => AppealReverificationFacultyRegNos.Contains(a.RegistrationNumber)).Select(F => F).ToList();
            var jntuh_spec = db.jntuh_specialization.AsNoTracking().ToList();

            foreach (var item in regFaculty)
            {
                CollegeFaculty collegeFacultynew = new CollegeFaculty();
                jntuh_registered_faculty rFaculty = jntuh_registered_facultys.FirstOrDefault(f => f.RegistrationNumber == item.RegistrationNumber.Trim());

                if (rFaculty != null)
                {
                    collegeFacultynew.id = rFaculty.id;
                    collegeFacultynew.TotalExperience = rFaculty.TotalExperience ?? 0;
                    collegeFacultynew.collegeId = collegeId;
                    collegeFacultynew.facultyType = rFaculty.type;
                    collegeFacultynew.facultyFirstName = rFaculty.FirstName;
                    collegeFacultynew.facultyLastName = rFaculty.MiddleName;
                    collegeFacultynew.facultySurname = rFaculty.LastName;
                    collegeFacultynew.facultyGenderId = rFaculty.GenderId;
                    collegeFacultynew.facultyFatherName = rFaculty.FatherOrHusbandName;
                    collegeFacultynew.photo = rFaculty.Photo;
                    if (rFaculty.DateOfBirth != null)
                        collegeFacultynew.dateOfBirth = UAAAS.Models.Utilities.MMDDYY2DDMMYY(rFaculty.DateOfBirth.ToString());

                    collegeFacultynew.facultyDesignationId = rFaculty.DesignationId != null ? (int)rFaculty.DesignationId : 0;
                    collegeFacultynew.designation = db.jntuh_designation.Where(d => d.id == collegeFacultynew.facultyDesignationId).Select(d => d.designation).FirstOrDefault();
                    collegeFacultynew.facultyOtherDesignation = rFaculty.OtherDesignation;
                    collegeFacultynew.IdentifiedFor = item.IdentifiedFor;
                    collegeFacultynew.facultyDepartmentId = item.DepartmentId != null ? (int)item.DepartmentId : 0;
                    collegeFacultynew.department = db.jntuh_department.Where(d => d.id == collegeFacultynew.facultyDepartmentId).Select(d => d.departmentName).FirstOrDefault();
                    collegeFacultynew.facultyOtherDepartment = rFaculty.OtherDepartment;
                    collegeFacultynew.SpecializationId = item.SpecializationId;
                    collegeFacultynew.SpecializationName = item.SpecializationId != null ? jntuh_spec.FirstOrDefault(i => i.id == item.SpecializationId).specializationName : "";
                    collegeFacultynew.RegisteredFacultyPGSpecializationName = rFaculty.PGSpecialization != null ? rFaculty.PGSpecialization != 0 ? jntuh_spec.FirstOrDefault(i => i.id == rFaculty.PGSpecialization).specializationName : "" : "";

                    collegeFacultynew.facultyEmail = rFaculty.Email;
                    collegeFacultynew.facultyMobile = rFaculty.Mobile;
                    collegeFacultynew.facultyPANNumber = rFaculty.PANNumber;
                    collegeFacultynew.facultyAadhaarNumber = rFaculty.AadhaarNumber;
                    collegeFacultynew.FacultyRegistrationNumber = rFaculty.RegistrationNumber;
                    collegeFacultynew.dateOfAppointment = UAAAS.Models.Utilities.MMDDYY2DDMMYY(rFaculty.DateOfAppointment.ToString());
                    teachingFaculty.Add(collegeFacultynew);
                }
            }

            jntuh_college_faculty_registered eFaculty = null;

            #endregion TeachingFacultyLogic End

            var facultyList = teachingFaculty.Select(e => e).ToList();
            int[] HumanitiesDeptIds = { 29, 30, 31, 32, 65, 66, 67, 68 };
            int[] DeptIDs = facultyList.Where(e => !HumanitiesDeptIds.Contains(e.facultyDepartmentId)).Select(d => d.facultyDepartmentId).Distinct().ToArray();
            var SpecBasedDept = facultyList.Select(d => new
            {
                deptid = d.facultyDepartmentId,
                specid = d.SpecializationId
            }).Distinct().ToList();

            if (DeptIDs.Count() > 0)
            {
                foreach (int deptId in DeptIDs)
                {
                    foreach (var item7 in SpecBasedDept)
                    {
                        if (deptId == item7.deptid)
                        {
                            if (item7.specid == 0)
                                collegeFaculty += ComplianceFaculty(facultyList.Where(g => g.facultyDepartmentId == deptId).OrderBy(s => s.facultyFirstName).ToList());
                            else
                                collegeFaculty += ComplianceFaculty(facultyList.Where(g => g.facultyDepartmentId == deptId && g.SpecializationId == item7.specid).OrderBy(s => s.facultyFirstName).ToList());
                        }
                    }
                }
            }

            var NewData = facultyList.Where(d => HumanitiesDeptIds.Contains(d.facultyDepartmentId)).Select(d => d).OrderBy(d => d.facultyDepartmentId).ToList();
            DeptIDs = NewData.OrderBy(d => d.department).Select(d => d.facultyDepartmentId).Distinct().ToArray();

            if (DeptIDs.Count() > 0)
            {
                foreach (int deptId in DeptIDs)
                {
                    collegeFaculty += ComplianceFaculty(facultyList.Where(g => g.facultyDepartmentId == deptId).OrderBy(g => g.facultyFirstName).ToList());
                }
            }

            contents = contents.Replace("##COLLEGETeachingFaculty##", collegeFaculty);
            return contents;
        }

        private string collegeFacultyReverificationMembers(int collegeId, string contents)
        {
            string collegeFaculty1 = string.Empty;
            int count = 1;
            string gender = string.Empty;
            string category = string.Empty;
            string department = string.Empty;
            string designation = string.Empty;
            string qualification = string.Empty;
            string dateOfAppointment = string.Empty;
            string teachingType = string.Empty;
            string ratified = string.Empty;

            var actualYear = db.jntuh_academic_year.Where(q => q.isPresentAcademicYear == true && q.isActive == true).Select(w => w.actualYear).FirstOrDefault();
            var PresentYearId = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(z => z.id).FirstOrDefault();

            #region TeachingFacultyLogic Begin
            List<CollegeFaculty> teachingFaculty = new List<CollegeFaculty>();
            List<jntuh_appeal_faculty_registered> regFaculty1 = db.jntuh_appeal_faculty_registered.Where(e => e.academicYearId == PresentYearId && e.collegeId == collegeId && e.AppealReverificationSupportingDocument != null).Select(e => e).ToList();
            var AppealReverificationFacultyRegNos = regFaculty1.Select(q => q.RegistrationNumber).Distinct().ToList();
            var jntuh_registered_facultys = db.jntuh_registered_faculty.Where(a => AppealReverificationFacultyRegNos.Contains(a.RegistrationNumber)).Select(F => F).ToList();
            var jntuh_designation = db.jntuh_designation.AsNoTracking().ToList();
            var jntuh_college_faculty_registred = db.jntuh_college_faculty_registered.Where(e => e.collegeId == collegeId).Select(e => e).ToList();
            var jntuh_department = db.jntuh_department.AsNoTracking().ToList();
            var jntuh_spec = db.jntuh_specialization.AsNoTracking().ToList();

            foreach (var item in regFaculty1)
            {
                string Reason = null;
                CollegeFaculty collegeFacultynew = new CollegeFaculty();
                jntuh_registered_faculty rFaculty = jntuh_registered_facultys.FirstOrDefault(f => f.RegistrationNumber == item.RegistrationNumber.Trim());
                if (rFaculty != null)
                {
                    collegeFacultynew.id = rFaculty.id;
                    collegeFacultynew.TotalExperience = rFaculty.TotalExperience ?? 0;
                    collegeFacultynew.collegeId = collegeId;
                    collegeFacultynew.facultyType = rFaculty.type;
                    collegeFacultynew.facultyFirstName = rFaculty.FirstName;
                    collegeFacultynew.facultyLastName = rFaculty.MiddleName;
                    collegeFacultynew.facultySurname = rFaculty.LastName;
                    collegeFacultynew.facultyGenderId = rFaculty.GenderId;
                    collegeFacultynew.facultyFatherName = rFaculty.FatherOrHusbandName;
                    collegeFacultynew.photo = rFaculty.Photo;

                    if (rFaculty.DateOfBirth != null)
                        collegeFacultynew.dateOfBirth = UAAAS.Models.Utilities.MMDDYY2DDMMYY(rFaculty.DateOfBirth.ToString());

                    collegeFacultynew.facultyDesignationId = rFaculty.DesignationId != null ? (int)rFaculty.DesignationId : 0;
                    collegeFacultynew.designation = jntuh_designation.Where(d => d.id == collegeFacultynew.facultyDesignationId).Select(d => d.designation).FirstOrDefault();
                    collegeFacultynew.facultyOtherDesignation = rFaculty.OtherDesignation;
                    collegeFacultynew.IdentifiedFor = item.IdentifiedFor;
                    collegeFacultynew.facultyDepartmentId = item.DepartmentId != null ? (int)item.DepartmentId : 0;
                    collegeFacultynew.department = jntuh_department.Where(d => d.id == collegeFacultynew.facultyDepartmentId).Select(d => d.departmentName).FirstOrDefault();

                    var FirstorDefaulty = jntuh_college_faculty_registred.FirstOrDefault(e => e.RegistrationNumber.Trim() == item.RegistrationNumber.Trim());
                    collegeFacultynew.SpecializationId = item.SpecializationId;
                    collegeFacultynew.SpecializationName = item.SpecializationId != null ? jntuh_spec.FirstOrDefault(i => i.id == item.SpecializationId).specializationName : "";
                    collegeFacultynew.RegisteredFacultyPGSpecializationName = rFaculty.PGSpecialization != null ? jntuh_spec.FirstOrDefault(i => i.id == rFaculty.PGSpecialization).specializationName : "";

                    //Faculty Flages and Reson
                    var jntuh_registered_faculty1 = jntuh_registered_facultys.Where(e => e.RegistrationNumber == rFaculty.RegistrationNumber).Select(rf => new
                    {
                        type = rf.type,
                        Absent = rf.Absent != null ? (bool)rf.Absent : false,
                        NotQualifiedAsperAICTE = rf.NotQualifiedAsperAICTE != null ? (bool)rf.NotQualifiedAsperAICTE : false,
                        InvalidPANNo = rf.InvalidPANNumber != null ? (bool)rf.InvalidPANNumber : false,
                        InCompleteCeritificates = rf.IncompleteCertificates != null ? (bool)rf.IncompleteCertificates : false,
                        PANNumber = rf.PANNumber,
                        XeroxcopyofcertificatesFlag = rf.Xeroxcopyofcertificates != null ? (bool)rf.Xeroxcopyofcertificates : false,
                        NOrelevantUgFlag = rf.NoRelevantUG == "Yes" ? true : false,
                        NOrelevantPgFlag = rf.NoRelevantPG == "Yes" ? true : false,
                        NOrelevantPhdFlag = rf.NORelevantPHD == "Yes" ? true : false,
                        BlacklistFaculty = rf.Blacklistfaculy != null ? (bool)rf.Blacklistfaculy : false,
                        OriginalCertificatesnotshownFlag = rf.OriginalCertificatesNotShown != null ? (bool)rf.OriginalCertificatesNotShown : false,
                        NoSCM = rf.NoSCM != null ? (bool)rf.NoSCM : false,
                        OriginalsVerifiedUG = rf.OriginalsVerifiedUG == true ? true : false,
                        OriginalsVerifiedPHD = rf.OriginalsVerifiedPHD == true ? true : false,
                        BASStatus = rf.BAS,
                        AadhaarFlag = rf.InvalidAadhaar,
                        Noclassinugpg = rf.Noclass == true ? true : false,
                        FakePHD = rf.FakePHD == true ? true : false,
                        Genuinenessnotsubmitted = rf.Genuinenessnotsubmitted == true ? true : false,
                        NotconsideredPHD = rf.NotconsideredPHD == true ? true : false,
                        NoPGspecialization = rf.NoPGspecialization == true ? true : false,
                        Invaliddegree = rf.Invaliddegree == true ? true : false,
                        AbsentforVerification = rf.AbsentforVerification == true ? true : false,

                        RegistrationNumber = rf.RegistrationNumber,
                        Department = rf.jntuh_department != null ? rf.jntuh_department.departmentName : "",
                        HighestDegreeID = rf.jntuh_registered_faculty_education.Count() != 0 ? rf.jntuh_registered_faculty_education.Where(q => q.educationId != 8).Select(e => e.educationId).Max() : 0,
                        IsApproved = rf.isApproved,
                        PanNumber = rf.PANNumber,
                        AadhaarNumber = rf.AadhaarNumber,
                        Photo = rf.Photo,
                        FullName = rf.FirstName + rf.MiddleName + rf.LastName,
                        FacultyEducation = rf.jntuh_registered_faculty_education,
                        DegreeId = rf.jntuh_registered_faculty_education.Count(e => e.facultyId == rf.id) > 0 ? rf.jntuh_registered_faculty_education.Where(e => e.facultyId == rf.id && e.educationId != 8).Select(e => e.educationId).Max() : 0,
                        DepartmentId = rf.DepartmentId
                    }).ToList();

                    var jntuh_registered_faculty = jntuh_registered_faculty1.Where(e => e.RegistrationNumber == rFaculty.RegistrationNumber).Select(rf => new
                    {
                        type = rf.type,
                        Absent = rf.Absent,
                        NotQualifiedAsperAICTE = rf.NotQualifiedAsperAICTE,
                        rf.InCompleteCeritificates,
                        rf.InvalidPANNo,
                        rf.NOrelevantPgFlag,
                        rf.NOrelevantUgFlag,
                        rf.NOrelevantPhdFlag,
                        rf.XeroxcopyofcertificatesFlag,
                        NoSCM = rf.NoSCM,
                        PANNumber = rf.PANNumber,
                        rf.OriginalsVerifiedUG,
                        rf.OriginalsVerifiedPHD,
                        rf.OriginalCertificatesnotshownFlag,
                        Blacklistfaculy = rf.BlacklistFaculty,
                        BASStatus = rf.BASStatus,
                        AadhaarFlag = rf.AadhaarFlag,
                        Noclassinugpg = rf.Noclassinugpg,
                        FakePHD = rf.FakePHD,
                        Genuinenessnotsubmitted = rf.Genuinenessnotsubmitted,
                        NotconsideredPHD = rf.NotconsideredPHD,
                        NoPGspecialization = rf.NoPGspecialization,
                        Invaliddegree = rf.Invaliddegree,
                        AbsentforVerification = rf.AbsentforVerification,

                        RegistrationNumber = rf.RegistrationNumber,
                        Department = rf.Department,
                        HighestDegree = db.jntuh_education_category.Where(c => c.id == rf.HighestDegreeID).Select(c => c.educationCategoryName).FirstOrDefault(),
                        Recruitedfor = db.jntuh_college_faculty_registered.Where(c => c.RegistrationNumber == rf.RegistrationNumber).Select(c => c.IdentifiedFor).FirstOrDefault(),
                        SpecializationId = db.jntuh_college_faculty_registered.Where(c => c.RegistrationNumber == rf.RegistrationNumber).Select(c => c.SpecializationId).FirstOrDefault(),
                        DeptId = db.jntuh_college_faculty_registered.Where(c => c.RegistrationNumber == rf.RegistrationNumber).Select(c => c.DepartmentId).FirstOrDefault(),
                        PanNumber = rf.PanNumber,
                        AadhaarNumber = rf.AadhaarNumber,
                        Photo = rf.Photo,
                        FullName = rf.FullName,
                        faculty_education = rf.FacultyEducation,
                        HighestDegreeID = rf.HighestDegreeID
                    }).FirstOrDefault();


                    if (jntuh_registered_faculty.Absent == true)
                        Reason += "Absent";
                    if (jntuh_registered_faculty.type == "Adjunct")
                    {
                        if (Reason != null)
                            Reason += ",Adjunct Faculty";
                        else
                            Reason += "Adjunct Faculty";
                    }

                    if (jntuh_registered_faculty.XeroxcopyofcertificatesFlag == true)
                    {
                        if (Reason != null)
                            Reason += ",Photo copy of Certificates";
                        else
                            Reason += "Photo copy of Certificates";
                    }

                    if (jntuh_registered_faculty.NOrelevantUgFlag == true)
                    {
                        if (Reason != null)
                            Reason += ",NO Relevant UG";
                        else
                            Reason += "NO Relevant UG";
                    }

                    if (jntuh_registered_faculty.NOrelevantPgFlag == true)
                    {
                        if (Reason != null)
                            Reason += ",NO Relevant PG";
                        else
                            Reason += "NO Relevant PG";
                    }

                    if (jntuh_registered_faculty.NOrelevantPhdFlag == true)
                    {
                        if (Reason != null)
                            Reason += ",NO Relevant PHD";
                        else
                            Reason += "NO Relevant PHD";
                    }

                    if (jntuh_registered_faculty.NotQualifiedAsperAICTE == true)
                    {
                        if (Reason != null)
                            Reason += ",Not Qualified as per AICTE/PCI";
                        else
                            Reason += "Not Qualified as per AICTE/PCI";
                    }

                    if (jntuh_registered_faculty.InvalidPANNo == true)
                    {
                        if (Reason != null)
                            Reason += ",InvalidPAN";
                        else
                            Reason += "InvalidPAN";
                    }

                    if (jntuh_registered_faculty.InCompleteCeritificates == true)
                    {
                        if (Reason != null)
                            Reason += ",InComplete Ceritificates";
                        else
                            Reason += "InComplete Ceritificates";
                    }

                    if (jntuh_registered_faculty.NoSCM == true)
                    {
                        if (Reason != null)
                            Reason += ",no/not valid SCM";
                        else
                            Reason += "no/not valid SCM";
                    }

                    if (jntuh_registered_faculty.OriginalCertificatesnotshownFlag == true)
                    {
                        if (Reason != null)
                            Reason += ",Orginal Certificates Not shown in College Inspection";
                        else
                            Reason += "Orginal Certificates Not shown in College Inspection";
                    }

                    if (jntuh_registered_faculty.PANNumber == null)
                    {
                        if (Reason != null)
                            Reason += ",No PAN Number";
                        else
                            Reason += "No PAN Number";
                    }
                    if (jntuh_registered_faculty.AadhaarFlag == "Yes")
                    {
                        if (Reason != null)
                            Reason += ",Invalid/Blur Aadhaar";
                        else
                            Reason += "Invalid/Blur Aadhaar";
                    }

                    if (jntuh_registered_faculty.BASStatus == "Yes")
                    {
                        if (Reason != null)
                            Reason += ",BAS Flag";
                        else
                            Reason += "BAS Flag";
                    }

                    if (jntuh_registered_faculty.OriginalsVerifiedUG == true)
                    {
                        if (Reason != null)
                            Reason += ",Complaint PHD Faculty";
                        else
                            Reason += "Complaint PHD Faculty";
                    }

                    if (jntuh_registered_faculty.OriginalsVerifiedPHD == true)
                    {
                        if (Reason != null)
                            Reason += ",No Guide Sign in PHD Thesis";
                        else
                            Reason += "No Guide Sign in PHD Thesis";
                    }
                    if (jntuh_registered_faculty.Blacklistfaculy == true)
                    {
                        if (Reason != null)
                            Reason += ",Blacklisted Faculty";
                        else
                            Reason += "Blacklisted Faculty";
                    }
                    if (jntuh_registered_faculty.Noclassinugpg == true)
                    {
                        if (Reason != null)
                            Reason += ",No Class in UG/PG";
                        else
                            Reason += "No Class in UG/PG";
                    }
                    if (jntuh_registered_faculty.FakePHD == true)
                    {
                        if (Reason != null)
                            Reason += ",Fake PhD";
                        else
                            Reason += "Fake PhD";
                    }
                    if (jntuh_registered_faculty.Genuinenessnotsubmitted == true)
                    {
                        if (Reason != null)
                            Reason += ",PHD Genuinity not Submitted ";
                        else
                            Reason += "PHD Genuinity not Submitted ";
                    }
                    if (jntuh_registered_faculty.NotconsideredPHD == true)
                    {
                        if (Reason != null)
                            Reason += ",Cosidered as a faculty but not cosidered his/her P.hD";
                        else
                            Reason += "Cosidered as a faculty but not cosidered his/her P.hD";
                    }
                    if (jntuh_registered_faculty.NoPGspecialization == true)
                    {
                        if (Reason != null)
                            Reason += ",no Specialization in PG";
                        else
                            Reason += "no Specialization in PG";
                    }
                    if (jntuh_registered_faculty.Invaliddegree == true)
                    {
                        if (Reason != null)
                            Reason += ",AICTE Not Approved University Degrees";
                        else
                            Reason += "AICTE Not Approved University Degrees";
                    }
                    if (jntuh_registered_faculty.AbsentforVerification == true)
                    {
                        if (Reason != null)
                            Reason += ",Absent for Physical Verification";
                        else
                            Reason += "Absent for Physical Verification";
                    }
                    collegeFacultynew.FacultyReson = Reason;

                    collegeFacultynew.facultyEmail = rFaculty.Email;
                    collegeFacultynew.facultyMobile = rFaculty.Mobile;
                    collegeFacultynew.facultyPANNumber = rFaculty.PANNumber;
                    collegeFacultynew.facultyAadhaarNumber = rFaculty.AadhaarNumber;
                    collegeFacultynew.FacultyRegistrationNumber = rFaculty.RegistrationNumber;
                    collegeFacultynew.dateOfAppointment = UAAAS.Models.Utilities.MMDDYY2DDMMYY(rFaculty.DateOfAppointment.ToString());
                    teachingFaculty.Add(collegeFacultynew);
                }
            }

            #endregion TeachingFacultyLogic End

            var facultyList = teachingFaculty.Select(e => e).ToList();
            int[] HumanitiesDeptIds = { 29, 30, 31, 32, 65, 66, 67, 68 };
            int[] DeptIDs = facultyList.Where(e => !HumanitiesDeptIds.Contains(e.facultyDepartmentId)).OrderBy(d => d.department).Select(d => d.facultyDepartmentId).Distinct().ToArray();

            var SpecBasedDept = facultyList.Select(d => new
            {
                deptid = d.facultyDepartmentId,
                specid = d.SpecializationId
            }).Distinct().ToList();

            if (DeptIDs.Count() > 0)
            {
                foreach (int deptId in DeptIDs)
                {
                    foreach (var item7 in SpecBasedDept)
                    {
                        if (deptId == item7.deptid)
                        {
                            if (item7.specid == 0)
                                collegeFaculty1 += ReverificationFaculty(facultyList.Where(g => g.facultyDepartmentId == deptId).OrderBy(s => s.facultyFirstName).ToList());
                            else
                                collegeFaculty1 += ReverificationFaculty(facultyList.Where(g => g.facultyDepartmentId == deptId && g.SpecializationId == item7.specid).OrderBy(s => s.facultyFirstName).ToList());
                        }
                    }
                }
            }
            var NewData = facultyList.Where(d => HumanitiesDeptIds.Contains(d.facultyDepartmentId)).Select(d => d).OrderBy(d => d.facultyDepartmentId).ToList();
            DeptIDs = NewData.OrderBy(d => d.department).Select(d => d.facultyDepartmentId).Distinct().ToArray();

            if (DeptIDs.Count() > 0)
            {
                foreach (int deptId in DeptIDs)
                {
                    collegeFaculty1 += ReverificationFaculty(facultyList.Where(g => g.facultyDepartmentId == deptId).OrderBy(g => g.facultyFirstName).ToList());
                }
            }
            contents = contents.Replace("##COLLEGEReverficationFaculty##", collegeFaculty1);

            return contents;
        }

        public string LaboratoriesDetails(int collegeId, string contents)
        {
            string strLaboratoriesDetails = string.Empty;
            string Equipmentdate = string.Empty;
            string chalanaDate = string.Empty;
            int sno = 1;
            var actualYear = db.jntuh_academic_year.Where(q => q.isPresentAcademicYear == true && q.isActive == true).Select(w => w.actualYear).FirstOrDefault();
            var PresentYearId = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(z => z.id).FirstOrDefault();

            List<Lab> labs = (from lm in db.jntuh_lab_master
                              join l in db.jntuh_appeal_college_laboratories on lm.id equals l.EquipmentID
                              where (l.CollegeID == collegeId && l.academicYearId == PresentYearId)
                              select new Lab
                              {
                                  degree = lm.jntuh_degree.degree,
                                  department = lm.jntuh_department.departmentName,
                                  specializationName = lm.jntuh_specialization.specializationName,
                                  Semester = lm.Semester,
                                  year = lm.Year,
                                  Labcode = lm.Labcode,
                                  LabName = lm.LabName,
                                  AvailableArea = l.AvailableArea,
                                  RoomNumber = l.RoomNumber,
                                  EquipmentName = l.EquipmentName,
                                  Make = l.Make,
                                  Model = l.Model,
                                  EquipmentUniqueID = l.EquipmentUniqueID,
                                  AvailableUnits = l.AvailableUnits,
                                  specializationId = lm.SpecializationID,
                                  ViewEquipmentPhoto = l.EquipmentPhoto,
                                  EquipmentDateOfPurchasing = l.EquipmentDateOfPurchasing,
                                  DelivaryChalanaDate = l.DelivaryChalanaDate
                              }).ToList();
            labs = labs.OrderBy(l => l.degree).ThenBy(l => l.department).ThenBy(l => l.specializationName).ThenBy(l => l.year).ThenBy(l => l.Semester).ToList();

            int[] SpecializationIDs = labs.Select(l => l.specializationId).Distinct().ToArray();

            foreach (var speclializationId in SpecializationIDs)
            {
                string strLabName = string.Empty;
                sno = 1;
                string strviewEquimentdata = string.Empty;
                var specializationDetails = db.jntuh_specialization.Where(s => s.id == speclializationId).Select(s => new
                {
                    specialization = s.specializationName,
                    department = s.jntuh_department.departmentName,
                    degree = s.jntuh_department.jntuh_degree.degree
                }).FirstOrDefault();

                strLabName = specializationDetails.degree + "- " + specializationDetails.department + "- " + specializationDetails.specialization;
                strLaboratoriesDetails += "<strong><u>" + strLabName + "</u></strong> <br /> <br />";
                strLaboratoriesDetails += "<table border='1' cellspacing='0' cellpadding='3'>";
                strLaboratoriesDetails += "<thead>";
                strLaboratoriesDetails += "<tr>";
                strLaboratoriesDetails += "<td  colspan='2'><p align='center'>S.No</p></td>";
                if (specializationDetails.degree == "B.Tech" || specializationDetails.degree == "B.Pharmacy")
                {
                    strLaboratoriesDetails += "<td  colspan='2'><p align='left'>Year</p></td>";
                    strLaboratoriesDetails += "<td  colspan='2'><p align='left'>Sem.</p></td>";
                    strLaboratoriesDetails += "<td  colspan='2'><p align='left'>Lab Code</p></td>";
                }
                strLaboratoriesDetails += "<td  colspan='8'><p align='left'>Lab Name</p></td>";
                strLaboratoriesDetails += "<td  colspan='3'><p align='left'>Area (in Sqm)</p></td>";
                strLaboratoriesDetails += "<td  colspan='2'><p align='left'>Room No</p></td>";
                strLaboratoriesDetails += "<td  colspan='8'><p align='left'>Equipment Name</p></td>";
                strLaboratoriesDetails += "<td  colspan='3'><p align='left'>Make</p></td>";
                strLaboratoriesDetails += "<td  colspan='3'><p align='left'>Model</p></td>";
                strLaboratoriesDetails += "<td  colspan='4'><p align='left'>Equipment UniqueID</p></td>";
                strLaboratoriesDetails += "<td  colspan='4'><p align='left'>Equiment Photo</p></td>";
                strLaboratoriesDetails += "<td  colspan='5'><p align='left'>EquipmentDateOfPurchasing</p></td>";
                strLaboratoriesDetails += "<td  colspan='5'><p align='left'>DelivaryChalanaDate</p></td>";
                strLaboratoriesDetails += "<td  colspan='3'><p align='left'>Available Units</p></td>";
                strLaboratoriesDetails += "</tr>";
                strLaboratoriesDetails += "</thead>";
                strLaboratoriesDetails += "<tbody>";

                foreach (var item in labs.Where(l => l.specializationId == speclializationId).ToList())
                {
                    if (!string.IsNullOrEmpty(item.ViewEquipmentPhoto))
                        strviewEquimentdata = "/Content/Upload/EquipmentsPhotos/" + item.ViewEquipmentPhoto;
                    else
                        strviewEquimentdata = "/Content/Images/no-photo.gif";

                    string path = @"~" + strviewEquimentdata.Replace("%20", " ");
                    path = System.Web.HttpContext.Current.Server.MapPath(path);
                    if (item.DelivaryChalanaDate != null)
                        chalanaDate = UAAAS.Models.Utilities.MMDDYY2DDMMYY(item.DelivaryChalanaDate.ToString());

                    if (item.EquipmentDateOfPurchasing != null)
                        Equipmentdate = UAAAS.Models.Utilities.MMDDYY2DDMMYY(item.EquipmentDateOfPurchasing.ToString());

                    strLaboratoriesDetails += "<tr>";
                    strLaboratoriesDetails += "<td  align='center' colspan='2'><p align='center'>" + sno + "</p></td>";
                    if (item.degree == "B.Tech" || item.degree == "B.Pharmacy")
                    {
                        strLaboratoriesDetails += "<td  align='left' colspan='2'>" + item.year + "</td>";
                        strLaboratoriesDetails += "<td  align='left' colspan='2'>" + item.Semester + "</td>";
                        strLaboratoriesDetails += "<td  align='left' colspan='2'>" + item.Labcode + "</td>";
                    }
                    strLaboratoriesDetails += "<td  align='left' colspan='8'>" + item.LabName + "</td>";
                    strLaboratoriesDetails += "<td  align='left' colspan='3'>" + item.AvailableArea + "</td>";
                    strLaboratoriesDetails += "<td  align='left' colspan='2'>" + item.RoomNumber + "</td>";
                    strLaboratoriesDetails += "<td  align='left' colspan='8'>" + item.EquipmentName + "</td>";
                    strLaboratoriesDetails += "<td  align='left' colspan='3'>" + item.Make + "</td>";
                    strLaboratoriesDetails += "<td  align='left' colspan='3'>" + item.Model + "</td>";
                    strLaboratoriesDetails += "<td  align='left' colspan='4'>" + item.EquipmentUniqueID + "</td>";

                    if (!string.IsNullOrEmpty(item.ViewEquipmentPhoto))
                    {
                        if (System.IO.File.Exists(path))
                            strLaboratoriesDetails += "<td  width='100' align='center' colspan='4'><p align='center'><img src='" + HtmlEncoder.Encode(path).Replace("'", "&#39;") + "' align='center'  width='40' height='50' /></p></td>";
                        else
                            strLaboratoriesDetails += "<td width='100' align='center' colspan='4'><p align='center'></p></td></tr>";
                    }
                    else
                        strLaboratoriesDetails += "<td width='100' align='center' colspan='4'><p align='center'></p></td></tr>";

                    strLaboratoriesDetails += "<td  align='left' colspan='5'>" + Equipmentdate + "</td>";
                    strLaboratoriesDetails += "<td  align='left' colspan='5'>" + chalanaDate + "</td>";
                    strLaboratoriesDetails += "<td  align='left' colspan='3'>" + item.AvailableUnits + "</td>";
                    strLaboratoriesDetails += "</tr>";
                    sno++;
                }
                strLaboratoriesDetails += "</tbody></table>";
            }
            contents = contents.Replace("##LaboratoriesDetails##", strLaboratoriesDetails);
            return contents;
        }

        public string PhysicalLabs(int collegeId, string contents)
        {
            string remarkstabledata = string.Empty;
            List<PhysicalLabMaster> physicallabs = new List<PhysicalLabMaster>();
            List<UAAAS.Controllers.Reports.DeficiencyReportWordController.physicalLabs> CollegePhysicalLabMaster = new List<UAAAS.Controllers.Reports.DeficiencyReportWordController.physicalLabs>();
            CollegePhysicalLabMaster =
                  db.jntuh_physical_labmaster_copy.Where(e => e.Collegeid == collegeId && e.Numberofrequiredlabs != null)
                      .Select(e => new UAAAS.Controllers.Reports.DeficiencyReportWordController.physicalLabs
                      {
                          department = db.jntuh_department.Where(d => d.id == e.DepartmentId).Select(s => s.departmentName).FirstOrDefault(),
                          NoOfRequiredLabs = e.Numberofrequiredlabs,
                          Labname = e.LabName,
                          year = e.Year,
                          semister = e.Semister,
                          LabCode = e.Labcode,
                          NoOfAvailabeLabs = db.jntuh_physical_labmaster_copy.Where(a => a.Collegeid == collegeId && a.Semister == e.Semister && a.Year == e.Year && a.Labcode == e.Labcode).Select(s => s.Numberofavilablelabs).FirstOrDefault() == null ? 0 : db.jntuh_physical_labmaster_copy.Where(a => a.Collegeid == collegeId && a.DepartmentId == e.DepartmentId && a.Semister == e.Semister && a.Year == e.Year && a.Labcode == e.Labcode).Select(s => s.Numberofavilablelabs).FirstOrDefault()
                      }).ToList();
            foreach (var item in CollegePhysicalLabMaster)
            {
                if (item.NoOfAvailabeLabs < item.NoOfRequiredLabs)
                {
                    PhysicalLabMaster PhysicalLabMaster = new PhysicalLabMaster();
                    PhysicalLabMaster.DepartmentName = item.department;
                    PhysicalLabMaster.NoofAvailable = (int)item.NoOfAvailabeLabs;
                    PhysicalLabMaster.NoofRequeried = (int)item.NoOfRequiredLabs;
                    PhysicalLabMaster.Labname = item.Labname;
                    physicallabs.Add(PhysicalLabMaster);
                }
            }

            if (physicallabs.Count != 0)
            {
                remarkstabledata += "<p><strong><u>2.Physical Labs Deficiency</u> :</strong></p>";
                remarkstabledata += "<br/>";
                remarkstabledata += "<table border='1' width='100%' cellspacing='0' cellpadding='5' style='font-size: 9px;'>";
                remarkstabledata += "<thead>";
                remarkstabledata += "<tr>";
                remarkstabledata += "<td width='10%'><span>S.No</span></td>";
                remarkstabledata += "<td width='30%'><span>Department</span></td>";
                remarkstabledata += "<td width='40%'><span>Physical Lab Name</span></td>";
                remarkstabledata += "<td width='10%'><span>Required</span></td>";
                remarkstabledata += "<td width='10%'><span>Available</span></td>";
                //remarkstabledata += "<td width='20%'><span>Deficiency</span></td>";
                remarkstabledata += "</tr>";
                remarkstabledata += "</thead>";
                int count = 1;
                remarkstabledata += "<tbody>";
                foreach (var item in physicallabs)
                {
                    remarkstabledata += "<tr>";
                    remarkstabledata += "<td width='10%'>" + count + "</td>";
                    remarkstabledata += "<td width='30%'style='text-align:center'>" + item.DepartmentName + "</td>";
                    remarkstabledata += "<td width='40%'style='text-align:center'>" + item.Labname + "</td>";
                    remarkstabledata += "<td width='10%'style='text-align:center'>" + item.NoofRequeried + "</td>";
                    if (item.NoofAvailable != 0)
                        remarkstabledata += "<td width='10%'style='text-align:center'>" + item.NoofAvailable + "</td>";
                    else
                        remarkstabledata += "<td width='10%'style='text-align:center'></td>";

                    remarkstabledata += "</tr>";
                    count++;
                }
                remarkstabledata += "</tbody>";
                remarkstabledata += "</table>";
            }
            else
            { }

            contents = contents.Replace("##PHYSICALLABS##", remarkstabledata);
            return contents;
        }

        public string Remarks(int collegeId, string contents)
        {
            string remarkstabledata = string.Empty;
            var actualYear = db.jntuh_academic_year.Where(q => q.isPresentAcademicYear == true && q.isActive == true).Select(w => w.actualYear).FirstOrDefault();
            var PresentYearId = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(z => z.id).FirstOrDefault();
            string remarks = db.jntuh_appeal_college_edit_status.Where(e => e.academicyearid == PresentYearId && e.collegeId == collegeId).Select(e => e.Remarks).FirstOrDefault();
            remarkstabledata += "<table border='1' width='100%' height='150px' cellspacing='0' cellpadding='50' style='font-size: 9px;'>";
            remarkstabledata += "<tbody>";
            remarkstabledata += "<tr>";
            remarkstabledata += "<td style='height: 150px'></td>";
            remarkstabledata += "</tr>";
            remarkstabledata += "</tbody>";
            remarkstabledata += "</table>";
            contents = contents.Replace("##REMARKS##", remarkstabledata);
            return contents;
        }

        public string Signatures(int collegeId, string contents)
        {
            string singnaturedata = string.Empty;
            singnaturedata += "<table width='100%' border='1' cellspacing='0' cellpadding='5' style='font-size: 9px;'>";
            singnaturedata += "<tbody>";
            singnaturedata += "<tr>";
            singnaturedata += "<td  width='8%'><p align='center' style='font-size: 10px';><b>S.No</b></p></td>";
            singnaturedata += "<td  width='50%'><p align='center' style='font-size: 10px';><b>Name & Designation</b></p></td>";
            singnaturedata += "<td  width='12%'><p p align='left' style='font-size: 10px';><b>Role</b></p></td>";
            singnaturedata += "<td width='30%'><p align='center' style='font-size: 10px';><b>Signature</b></p></td>";
            singnaturedata += "</tr>";
            List<Committemembers> Committemembers = GetCommitteNumbers().ToList();
            foreach (var item in Committemembers)
            {
                singnaturedata += "<tr>";
                singnaturedata += "<td  width='8%'><p align='center'>" + item.SNo + "</p></td>";
                singnaturedata += "<td  width='50%'><p align='lest'>" + item.Name + "</p></td>";
                singnaturedata += "<td  width='12%'><p p align='left'>" + item.Designation + "</p></td>";
                singnaturedata += "<td  width='30%'><p p align='left'></p></td>";
                singnaturedata += "</tr>";
            }
            singnaturedata += "</tbody>";
            singnaturedata += "</table>";
            contents = contents.Replace("##SIGNATURES##", singnaturedata);
            return contents;
        }

        private string ComplianceFaculty(List<CollegeFaculty> facultyList)
        {
            int count = 1;
            string collegeFaculty = string.Empty;
            string ContentFaculty = string.Empty;
            string gender = string.Empty;
            string category = string.Empty;
            string department = string.Empty;
            string degree = string.Empty;
            int degid = 0;
            string designation = string.Empty;
            string qualification = string.Empty;
            string dateOfAppointment = string.Empty;
            string teachingType = string.Empty;
            string ratified = string.Empty;
            string strcheckList = "<img alt='' src='" + serverURL + "/Content/Images/checkbox_no_b.png' height='8' />";

            int? deptid = null;
            string IdentifiedFor = string.Empty;
            string Specialization = string.Empty;
            if (facultyList.Count() > 0)
            {
                deptid = facultyList.FirstOrDefault().facultyDepartmentId;
                IdentifiedFor = facultyList.FirstOrDefault().IdentifiedFor;
                Specialization = facultyList.FirstOrDefault().SpecializationName;
            }
            else
                deptid = null;
            if (deptid == null || deptid == 0)
            {
                department = "New";
            }
            else
            {
                department = db.jntuh_department.Where(d => d.id == deptid).FirstOrDefault().departmentName;
                degid = db.jntuh_department.Where(d => d.id == deptid).FirstOrDefault().degreeId;
                degree = db.jntuh_degree.Where(d => d.id == degid).FirstOrDefault().degree;
            }

            if (IdentifiedFor == "UG")
                collegeFaculty += "<strong><u><p style='font-size: 10px;'>" + degree + "-" + department + "</p></u></strong> <br />";
            else if (IdentifiedFor == "PG")
                collegeFaculty += "<strong><u><p style='font-size: 10px;'>" + degree + "-" + Specialization + "</p></u></strong> <br />";
            else
                collegeFaculty += "<strong><u><p style='font-size: 10px;'>" + degree + "-" + Specialization + "</p></u></strong> <br />";


            collegeFaculty += "<table width='100%' border='1' cellspacing='0' cellpadding='5' style='font-size: 9px;'>";
            collegeFaculty += "<tbody>";
            collegeFaculty += "<tr>";
            collegeFaculty += "<td colspan='1'><p align='center' style='font-size: 10px';><b>SNo</b></p></td>";
            collegeFaculty += "<td colspan='4'><p align='center' style='font-size: 10px';><b>Registration Number</b></p></td>";
            collegeFaculty += "<td colspan='3'><p p align='left' style='font-size: 10px';><b>Faculty Name</b></p></td>";
            collegeFaculty += "<td colspan='2'><p align='center' style='font-size: 10px';><b>Pan Number</b></p></td>";
            collegeFaculty += "<td colspan='2'><p align='center' style='font-size: 10px';><b>Gender</b></p></td>";
            collegeFaculty += "<td colspan='3'><p align='left' style='font-size: 10px';><b>Designation</b></p></td>";
            collegeFaculty += "<td colspan='2'><p align='center' style='font-size: 10px';><b>Qualification</b></p></td>";
            collegeFaculty += "<td colspan='3'><p align='left' style='font-size: 10px';><b>Specilization</b></p></td>";
            collegeFaculty += "<td colspan='2'><p align='center' style='font-size: 10px';><b>Date of Appointment</b></p></td>";
            collegeFaculty += "<td colspan='3'><p align='center' style='font-size: 10px';><b>Remarks</b></p></td>";
            collegeFaculty += "</tr>";

            var jntuh_designations = db.jntuh_designation.Select(D => new { D.designation, D.id }).ToList();
            var jntuh_college_faculty_registereds = db.jntuh_college_faculty_registered.Select(F => new { F.RegistrationNumber, F.collegeId, F.IdentifiedFor }).ToList();
            var AppealReverificationFacultyRegNos = facultyList.Select(q => q.FacultyRegistrationNumber).Distinct().ToList();
            var jntuh_registered_facultys = db.jntuh_registered_faculty.Where(a => AppealReverificationFacultyRegNos.Contains(a.RegistrationNumber)).Select(F => new { F.RegistrationNumber, F.Photo }).ToList();

            foreach (var item in facultyList)
            {
                gender = item.facultyGenderId == 1 ? "M" : "F";
                designation = jntuh_designations.Where(d => d.id == item.facultyDesignationId).Select(d => d.designation).FirstOrDefault();
                if (item.dateOfAppointment != null)
                    dateOfAppointment = item.dateOfAppointment.ToString();

                qualification = db.jntuh_registered_faculty_education.OrderByDescending(education => education.educationId)
                                                         .Where(education => education.facultyId == item.id && education.educationId != 8)
                                                         .Select(education => education.courseStudied).FirstOrDefault();

                string identifiedfor = jntuh_college_faculty_registereds.Where(f => f.RegistrationNumber == item.FacultyRegistrationNumber && f.collegeId == item.collegeId).Select(f => f.IdentifiedFor).FirstOrDefault();
                string Facultyphoto = jntuh_registered_facultys.Where(F => F.RegistrationNumber == item.FacultyRegistrationNumber).Select(F => F.Photo).FirstOrDefault();
                collegeFaculty += "<tr>";
                collegeFaculty += "<td colspan='1'><p align='center'>" + count + "</p></td>";
                collegeFaculty += "<td colspan='4'><p align='center'>" + item.FacultyRegistrationNumber + "</p></td>";
                collegeFaculty += "<td colspan='3'><p p align='left'>" + (item.facultyFirstName + " " + item.facultyLastName + " " + item.facultySurname).ToUpper() + "</p></td>";
                collegeFaculty += "<td colspan='2'><p align='center'>" + item.facultyPANNumber + "</p></td>";
                collegeFaculty += "<td colspan='2'><p align='center'>" + gender + "</p></td>";
                collegeFaculty += "<td colspan='3'><p align='left'>" + designation + "</p></td>";
                collegeFaculty += "<td colspan='2'><p align='center'>" + qualification + "</p></td>";
                collegeFaculty += "<td colspan='3'><p align='left'>" + item.RegisteredFacultyPGSpecializationName + "</p></td>";
                collegeFaculty += "<td colspan='2'><p align='center'>" + dateOfAppointment + "</p></td>";
                collegeFaculty += "<td colspan='3'><p align='center'></p></td>";
                collegeFaculty += "</tr>";
                count++;
            }
            collegeFaculty += "</tbody>";
            collegeFaculty += "</table><br/>";
            return collegeFaculty;
        }

        private string ReverificationFaculty(List<CollegeFaculty> facultyList)
        {
            int count = 1;
            string collegeFaculty = string.Empty;
            string ContentFaculty = string.Empty;
            string gender = string.Empty;
            string category = string.Empty;
            string department = string.Empty;
            string degree = string.Empty;
            int degid = 0;
            string designation = string.Empty;
            string qualification = string.Empty;
            string dateOfAppointment = string.Empty;
            string basdateOfjoinning = string.Empty;
            string teachingType = string.Empty;
            string ratified = string.Empty;
            string strcheckList = "<img alt='' src='" + serverURL + "/Content/Images/checkbox_no_b.png' height='8' />";

            int? deptid = null;
            string IdentifiedFor = string.Empty;
            string Specialization = string.Empty;
            if (facultyList.Count() > 0)
            {
                deptid = facultyList.FirstOrDefault().facultyDepartmentId;
                IdentifiedFor = facultyList.FirstOrDefault().IdentifiedFor;
                Specialization = facultyList.FirstOrDefault().SpecializationName;
            }
            else
                deptid = null;
            if (deptid == null || deptid == 0)
            {
                department = "New";
            }
            else
            {
                department = db.jntuh_department.Where(d => d.id == deptid).FirstOrDefault().departmentName;
                degid = db.jntuh_department.Where(d => d.id == deptid).FirstOrDefault().degreeId;
                degree = db.jntuh_degree.Where(d => d.id == degid).FirstOrDefault().degree;
            }

            if (IdentifiedFor == "UG")
                collegeFaculty += "<strong><u><p style='font-size: 10px;'>" + degree + "-" + department + "</p></u></strong> <br />";
            else if (IdentifiedFor == "PG")
                collegeFaculty += "<strong><u><p style='font-size: 10px;'>" + degree + "-" + Specialization + "</p></u></strong> <br />";
            else
                collegeFaculty += "<strong><u><p style='font-size: 10px;'>" + degree + "-" + Specialization + "</p></u></strong> <br />";

            collegeFaculty += "<table border='1' cellspacing='0' cellpadding='5' style='font-size: 9px;'>";
            collegeFaculty += "<tbody>";
            collegeFaculty += "<tr>";
            collegeFaculty += "<td colspan='1'><p align='center' style='font-size: 10px;'><b>SNo</b></p></td>";
            collegeFaculty += "<td colspan='4'><p align='center' style='font-size: 10px;'><b>Registration Number</b></p></td>";
            collegeFaculty += "<td colspan='3'><p p align='left' style='font-size: 10px;'><b>Faculty Name</b></p></td>";
            collegeFaculty += "<td colspan='2'><p align='center' style='font-size: 10px;'><b>Pan Number</b></p></td>";
            collegeFaculty += "<td colspan='2'><p align='center' style='font-size: 10px;'><b>Gender</b></p></td>";
            collegeFaculty += "<td colspan='3'><p align='left' style='font-size: 10px;'><b>Designation</b></p></td>";
            collegeFaculty += "<td colspan='2'><p align='center' style='font-size: 10px;'><b>Qualification</b></p></td>";
            collegeFaculty += "<td colspan='3'><p align='left' style='font-size: 10px;'><b>Specilization</b></p></td>";
            collegeFaculty += "<td colspan='2'><p align='center' style='font-size: 10px;'><b>Date of BAS Joining</b></p></td>";
            collegeFaculty += "<td colspan='3'><p align='center' style='font-size: 10px;'><b>Reason</b></p></td>";
            collegeFaculty += "<td colspan='3'><p align='center' style='font-size: 10px;'><b>Remarks</b></p></td>";
            collegeFaculty += "</tr>";
            var jntuh_designations = db.jntuh_designation.Select(D => new { D.designation, D.id }).ToList();
            var jntuh_college_faculty_registereds = db.jntuh_college_faculty_registered.Select(F => new { F.RegistrationNumber, F.collegeId, F.IdentifiedFor }).ToList();
            var AppealReverificationFacultyRegNos = facultyList.Select(q => q.FacultyRegistrationNumber).Distinct().ToList();
            var jntuh_registered_facultys = db.jntuh_registered_faculty.Where(a => AppealReverificationFacultyRegNos.Contains(a.RegistrationNumber)).Select(F => new { F.RegistrationNumber, F.Photo }).ToList();

            foreach (var item in facultyList)
            {
                gender = item.facultyGenderId == 1 ? "M" : "F";
                designation = jntuh_designations.Where(d => d.id == item.facultyDesignationId).Select(d => d.designation).FirstOrDefault();
                DateTime? datejoin = new DateTime();
                datejoin = db.jntuh_college_basreport.OrderByDescending(z => z.joiningDate).Where(e => e.collegeId == item.collegeId && e.RegistrationNumber == item.FacultyRegistrationNumber).Select(s => s.joiningDate).FirstOrDefault();
                if (datejoin != null)
                    basdateOfjoinning = UAAAS.Models.Utilities.MMDDYY2DDMMYY(datejoin.ToString());

                if (item.dateOfAppointment != null)
                    dateOfAppointment = UAAAS.Models.Utilities.MMDDYY2DDMMYY(item.dateOfAppointment.ToString());

                qualification = db.jntuh_registered_faculty_education.OrderByDescending(education => education.educationId)
                                                         .Where(education => education.facultyId == item.id && education.educationId != 8)
                                                         .Select(education => education.courseStudied).FirstOrDefault();


                string identifiedfor = jntuh_college_faculty_registereds.Where(f => f.RegistrationNumber == item.FacultyRegistrationNumber && f.collegeId == item.collegeId).Select(f => f.IdentifiedFor).FirstOrDefault();
                string Facultyphoto = jntuh_registered_facultys.Where(F => F.RegistrationNumber == item.FacultyRegistrationNumber).Select(F => F.Photo).FirstOrDefault();
                collegeFaculty += "<tr>";
                collegeFaculty += "<td colspan='1'><p align='center'>" + count + "</p></td>";
                collegeFaculty += "<td colspan='4'><p align='center'>" + item.FacultyRegistrationNumber + "</p></td>";
                collegeFaculty += "<td colspan='3'><p p align='left'>" + (item.facultyFirstName + " " + item.facultyLastName + " " + item.facultySurname).ToUpper() + "</p></td>";
                collegeFaculty += "<td colspan='2'><p align='center'>" + item.facultyPANNumber + "</p></td>";
                collegeFaculty += "<td colspan='2'><p align='center'>" + gender + "</p></td>";
                collegeFaculty += "<td colspan='3'><p align='left'>" + designation + "</p></td>";
                collegeFaculty += "<td colspan='2'><p align='center'>" + qualification + "</p></td>";
                collegeFaculty += "<td colspan='3'><p align='left'>" + item.RegisteredFacultyPGSpecializationName + "</p></td>";
                collegeFaculty += "<td colspan='2'><p align='center'>" + basdateOfjoinning + "</p></td>";
                collegeFaculty += "<td colspan='3'><p align='center'>" + item.FacultyReson + "</p></td>";
                collegeFaculty += "<td colspan='3'><p align='center'></p></td>";
                collegeFaculty += "</tr>";
                count++;
            }
            collegeFaculty += "</tbody>";
            collegeFaculty += "</table><br/>";
            return collegeFaculty;
        }

        private int GetIntake(int collegeId, int academicYearId, int specializationId, int shiftId, int flag)
        {
            int intake = 0;
            if (flag == 1)
                intake = db.jntuh_college_intake_existing.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == shiftId).Select(i => i.approvedIntake).FirstOrDefault();
            else
                intake = db.jntuh_college_intake_existing.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == shiftId).Select(i => i.admittedIntake).FirstOrDefault();
            return intake;
        }

        public List<Committemembers> GetCommitteNumbers()
        {
            List<Committemembers> selectPersons = new List<Committemembers>();
            selectPersons.Add(new Committemembers { SNo = 1, Name = "Dr. A. Govardhan, Prof. of CSE & Rector, JNTUH", Designation = "Chairman" });
            selectPersons.Add(new Committemembers { SNo = 2, Name = "Dr. G.N. Srinivas, Prof. of EEE & Vice-Principal, JNTUH CEH", Designation = "Member" });
            selectPersons.Add(new Committemembers { SNo = 3, Name = "Dr. Dr. M. T. Naik, Prof. of Mech Engg, JNTUH CEH", Designation = "Member" });
            selectPersons.Add(new Committemembers { SNo = 4, Name = "Dr. K. Ramamohan Reddy, Prof. of CWR,IST, JNTUH", Designation = "Member" });
            selectPersons.Add(new Committemembers { SNo = 5, Name = "Dr. D. Sreenivasa Rao, Prof. of ECE, JNTUH CEH ", Designation = "Member" });
            return selectPersons;
        }

        public class Committemembers
        {
            public int SNo { get; set; }
            public string Name { get; set; }
            public string Designation { get; set; }
        }

        #endregion

        #region appeal Labs Pdf Reports Seperately

        [Authorize(Roles = "College,Admin")]
        public ActionResult AppealLabsData(int preview, string strcollegeId)
        {
            serverURL = Request.Url.Scheme + System.Uri.SchemeDelimiter + Request.Url.Host + (Request.Url.IsDefaultPort ? "" : ":" + Request.Url.Port);
            int collegeId = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(strcollegeId, WebConfigurationManager.AppSettings["CryptoKey"]));
            string pdfPath = string.Empty;
            string collegeCode = db.jntuh_college.Find(collegeId).collegeCode;
            if (preview == 0)
            {
                pdfPath = SaveAppealLabs(preview, collegeId);
                pdfPath = pdfPath.Replace("/", "\\");

            }

            return File(pdfPath, "application/pdf", collegeCode + "_AppealPdfReport.pdf");
        }

        private string SaveAppealLabs(int preview, int collegeId)
        {
            string fullPath = string.Empty;
            //Set page size as A4
            Document pdfDoc = new Document(PageSize.A4, 60, 50, 60, 60);
            string path = Server.MapPath("~/Content/PDFReports/AppealPdfReports/");

            string collegeCode = db.jntuh_college.Find(collegeId).collegeCode;
            string collegeName = db.jntuh_college.Find(collegeId).collegeName;

            if (preview == 0)
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                fullPath = path + collegeCode + "_" + collegeName.Substring(0, 3) + "_" + DateTime.Now.ToString("yyyMMddHHmmss") + ".pdf";

                PdfWriter pdfWriter = PdfWriter.GetInstance(pdfDoc, new FileStream(fullPath, FileMode.Create));
                ITextEvents iTextEvents = new ITextEvents();
                iTextEvents.CollegeName = collegeName;
                iTextEvents.CollegeCode = collegeCode;

                iTextEvents.formType = "Acknowledgement";
                pdfWriter.PageEvent = iTextEvents;

            }

            //Open PDF Document to write data
            pdfDoc.Open();

            //Assign Html content in a string to write in PDF
            string contents = string.Empty;

            StreamReader sr;

            //Read file from server path        
            sr = System.IO.File.OpenText(Server.MapPath("~/Content/AppealPdfReportlabs.html"));
            //store content in the variable
            contents = sr.ReadToEnd();
            sr.Close();

            contents = contents.Replace("##SERVERURL##", serverURL);
            contents = collegeInformation(collegeId, contents);
            contents = LaboratoriesDetails(collegeId, contents);
            //contents = PhysicalLabs(collegeId, contents);           
            //Read string contents using stream reader and convert html to parsed conent
            var parsedHtmlElements = HTMLWorker.ParseToList(new StringReader(contents), null);

            //Get each array values from parsed elements and add to the PDF document
            bool pageRotated = false;

            foreach (var htmlElement in parsedHtmlElements)
            {
                if (htmlElement.Equals("<textarea>"))
                {
                    pdfDoc.NewPage();
                }

                if (htmlElement.Chunks.Count >= 3)
                {
                    if (htmlElement.Chunks.Count == 7)
                    {
                        pdfDoc.SetPageSize(iTextSharp.text.PageSize.A4.Rotate());
                        pdfDoc.SetMargins(60, 50, 60, 60);
                        pageRotated = true;
                    }
                    else
                    {
                        if (pageRotated == true)
                        {
                            //pdfDoc.SetPageSize(iTextSharp.text.PageSize.A4.Rotate());
                            //pdfDoc.SetMargins(60, 50, 60, 60);
                            //pageRotated = true;
                            pdfDoc.SetPageSize(iTextSharp.text.PageSize.A4);
                            pdfDoc.SetMargins(60, 50, 60, 60);
                            pageRotated = false;
                        }
                    }

                    pdfDoc.NewPage();

                }
                else
                {
                    pdfDoc.Add(htmlElement as IElement);
                }
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

        #endregion

        #region For Faculty Add/Reactivate Logic

        [Authorize(Roles = "Admin")]
        public ActionResult CollegeAppealDetails(int? collegeId)
        {

            var colgids = db.jntuh_appeal_college_edit_status.Where(i => i.IsCollegeEditable == false && i.academicyearid == 11).GroupBy(e => e.collegeId).Select(e => e.Key).Distinct().ToArray();
            ViewBag.Colleges = db.jntuh_college.Where(e => e.isActive && colgids.Contains(e.id)).Select(e => new { collegeId = e.id, collegeName = e.collegeCode + "_" + e.collegeName }).OrderBy(e => e.collegeName).ToList();
            var appealfacultyDetails = db.jntuh_appeal_faculty_registered.Where(a => a.academicYearId == 11).ToList();

            var principalDetails = db.jntuh_appeal_principal_registered.Where(a => a.academicYearId == 11).ToList();
            var jntuh_registered_faculty = db.jntuh_registered_faculty.AsNoTracking().ToList();
            var jntuh_college = db.jntuh_college.AsNoTracking().ToList();
            var jntuh_department = db.jntuh_department.AsNoTracking().ToList();
            var jntuh_degreelist = db.jntuh_degree.AsNoTracking().ToList();
            var jntuh_specialization = db.jntuh_specialization.AsNoTracking().ToList();
            var jntuh_scmupload = db.jntuh_scmupload.Where(s => s.CollegeId == collegeId).Select(s => s).ToList();
            var jntuh_college_faculty_registereds = db.jntuh_college_faculty_registered.Where(C => C.collegeId == collegeId).ToList();
            var facultyDetails = new List<CollegeFacultyWithIntakeReport> { new CollegeFacultyWithIntakeReport() };
            var facultyWithIntakeReport = facultyDetails.FirstOrDefault();
            if (facultyWithIntakeReport != null)
            {
                facultyWithIntakeReport.CollegeFaculties = new List<CollegeFaculty>();
                facultyWithIntakeReport.LabsListDefs1 = new List<AnonymousLabclass>();
                facultyWithIntakeReport.CollegeIntakeExistings = new List<CollegeIntakeExisting>();
                facultyWithIntakeReport.CollegeIntakeSupportingDocuments = new List<CollegeIntakeExisting>();
            }
            #region Faculty
            if (collegeId != null)
            {
                //collegeId = 7;
                ViewBag.collegeId = collegeId;
                appealfacultyDetails = appealfacultyDetails.Where(e => e.collegeId == collegeId).ToList();
                if (appealfacultyDetails.Count > 0)
                {
                    foreach (var item in appealfacultyDetails)
                    {
                        var data = jntuh_registered_faculty.Where(e => e.RegistrationNumber.Trim() == item.RegistrationNumber.Trim()).Select(e => e).FirstOrDefault();
                        var Reason1 = string.Empty;
                        var newreason = string.Empty;
                        var faculty = new CollegeFaculty();
                        var labs = new AnonymousLabclass();
                        var intake = new CollegeIntakeExisting();
                        var intakesupportingDocuments = new CollegeIntakeExisting();
                        faculty.FacultyRegistrationNumber = item.RegistrationNumber;
                        faculty.collegeId = item.collegeId;
                        //  faculty.CollegeName = jntuh_college.Where(i => i.id == item.collegeId).Select(i => i.collegeName).FirstOrDefault();
                        faculty.id = data.id;
                        faculty.facultyFirstName = data.FirstName;
                        faculty.facultyLastName = data.LastName;
                        faculty.facultySurname = data.MiddleName;
                        faculty.facultyAadhaarNumber = item.AadhaarNumber;
                        faculty.BasstatusOld = data.BAS;
                        faculty.ViewNotificationDocument = item.NOtificationReport;
                        faculty.ViewAppointmentOrderDocument = item.AppointMentOrder;
                        faculty.ViewJoiningReportDocument = item.JoiningOrder;
                        faculty.facultyAadharDocument = item.AadhaarDocument;
                        if (item.SelectionCommiteMinutes != null)
                        {
                            faculty.ViewSelectionCommitteeDocument = item.SelectionCommiteMinutes;
                        }
                        else
                        {
                            faculty.Collegescmdocument =
                                jntuh_scmupload.Where(
                                    s => s.RegistrationNumber.Trim() == faculty.FacultyRegistrationNumber.Trim())
                                    .Select(s => s.SCMDocument)
                                    .FirstOrDefault();
                        }
                        faculty.ViewAppealReverificationSupportDoc = item.AppealReverificationSupportingDocument;
                        faculty.ViewAppealReverificationScreenShot = item.AppealReverificationScreenshot;

                        faculty.department = (item.DepartmentId != null)
                            ? jntuh_department.Where(D => D.id == item.DepartmentId)
                                .Select(D => D.departmentName)
                                .FirstOrDefault()
                            : "";
                        faculty.DegreeName = (item.DegreeId != null)
                            ? jntuh_degreelist.Where(D => D.id == item.DegreeId)
                                .Select(D => D.degree)
                                .FirstOrDefault()
                            : "";
                        faculty.SpecializationName = (item.SpecializationId != null)
                            ? jntuh_specialization.Where(D => D.id == item.SpecializationId)
                                .Select(D => D.specializationName)
                                .FirstOrDefault()
                            : "";
                        faculty.DegreeId = item.DegreeId;
                        faculty.SpecializationId = item.SpecializationId;
                        faculty.IdentifiedFor = item.IdentifiedFor;

                        //int? DepartmentId =jntuh_college_faculty_registereds.Where(C => C.RegistrationNumber == item.RegistrationNumber).Select(C => C.DepartmentId).FirstOrDefault();
                        //if(DepartmentId!=null)
                        //{
                        //    var jntuhDepartment = jntuh_department.FirstOrDefault(i => i.id == DepartmentId);
                        //    if (jntuhDepartment != null)
                        //        faculty.department = jntuhDepartment.departmentName;
                        //}
                        //else
                        //{
                        //    var jntuhDepartment = jntuh_department.FirstOrDefault(i => i.id == item.DepartmentId);
                        //    if (jntuhDepartment != null)
                        //        faculty.department = jntuhDepartment.departmentName;
                        //}

                        var degreeid = data.jntuh_registered_faculty_education.Count > 0 ? data.jntuh_registered_faculty_education.Select(e => e.educationId).Max() : 0;

                        //

                        if (data.Absent == true)
                            Reason1 += "Absent";
                        if (data.type == "Adjunct")
                        {
                            if (Reason1 != string.Empty)
                                Reason1 += ",Adjunct Faculty";
                            else
                                Reason1 += "Adjunct Faculty";
                        }
                        if (data.OriginalCertificatesNotShown == true)
                        {
                            if (Reason1 != string.Empty)
                                Reason1 += ",Orginal Certificates Not shown in College Inspection";
                            else
                                Reason1 += "Orginal Certificates Not shown in College Inspection";
                        }
                        if (data.Xeroxcopyofcertificates == true)
                        {
                            if (Reason1 != string.Empty)
                                Reason1 += ",Photo copy of Certificates";
                            else
                                Reason1 += "Photo copy of Certificates";
                        }
                        if (data.NotQualifiedAsperAICTE == true || degreeid < 4)
                        {
                            if (Reason1 != string.Empty)
                                Reason1 += ",Not Qualified as per AICTE/PCI";
                            else
                                Reason1 += "Not Qualified as per AICTE/PCI";
                        }
                        if (data.NoSCM == true)
                        {
                            if (Reason1 != string.Empty)
                                Reason1 += ",no/not valid SCM";
                            else
                                Reason1 += "no/not valid SCM";
                        }
                        if (data.PANNumber == null)
                        {
                            if (Reason1 != string.Empty)
                                Reason1 += ",No PAN Number";
                            else
                                Reason1 += "No PAN Number";
                        }
                        if (data.IncompleteCertificates == true)
                        {
                            if (Reason1 != string.Empty)
                                Reason1 += ",IncompleteCertificates";
                            else
                                Reason1 += "IncompleteCertificates";
                        }
                        if (data.Blacklistfaculy == true)
                        {
                            if (Reason1 != string.Empty)
                                Reason1 += ",Blacklisted Faculty";
                            else
                                Reason1 += "Blacklisted Faculty";
                        }
                        if (data.NoRelevantUG == "Yes")
                        {
                            if (Reason1 != string.Empty)
                                Reason1 += ",NO Relevant UG";
                            else
                                Reason1 += "NO Relevant UG";
                        }
                        if (data.NoRelevantPG == "Yes")
                        {
                            if (Reason1 != string.Empty)
                                Reason1 += ",NO Relevant PG";
                            else
                                Reason1 += "NO Relevant PG";
                        }
                        if (data.NORelevantPHD == "Yes")
                        {
                            if (Reason1 != string.Empty)
                                Reason1 += ",NO Relevant PHD";
                            else
                                Reason1 += "NO Relevant PHD";
                        }
                        if (data.InvalidPANNumber == true)
                        {
                            if (Reason1 != string.Empty)
                                Reason1 += ",InvalidPANNumber";
                            else
                                Reason1 += "InvalidPANNumber";
                        }
                        if (data.OriginalsVerifiedPHD == true)
                        {
                            if (Reason1 != string.Empty)
                                Reason1 += ",No Guide Sign in PHD Thesis";
                            else
                                Reason1 += "No Guide Sign in PHD Thesis";
                        }
                        if (data.OriginalsVerifiedUG == true)
                        {
                            if (Reason1 != string.Empty)
                                Reason1 += ",Complaint PHD Faculty";
                            else
                                Reason1 += "Complaint PHD Faculty";
                        }
                        if (data.Invaliddegree == true)
                        {
                            if (Reason1 != string.Empty)
                                Reason1 += ",AICTE Not Approved University Degrees";
                            else
                                Reason1 += "AICTE Not Approved University Degrees";
                        }
                        if (data.BAS == "Yes")
                        {
                            if (Reason1 != string.Empty)
                                Reason1 += ",BAS Flag";
                            else
                                Reason1 += "BAS Flag";
                        }
                        if (data.InvalidAadhaar == "Yes")
                        {
                            if (Reason1 != string.Empty)
                                Reason1 += ",Invalid/Blur Aadhaar";
                            else
                                Reason1 += "Invalid/Blur Aadhaar";
                        }
                        if (data.Noclass == true)
                        {
                            if (Reason1 != string.Empty)
                                Reason1 += ",No Class in UG/PG";
                            else
                                Reason1 += "No Class in UG/PG";
                        }
                        if (data.AbsentforVerification == true)
                        {
                            if (Reason1 != string.Empty)
                                Reason1 += ",Absentfor Physical Verification";
                            else
                                Reason1 += "Absentfor Physical Verification";
                        }
                        if (data.NotconsideredPHD == true)
                        {
                            if (Reason1 != string.Empty)
                                Reason1 += ",Cosidered as a faculty but not cosidered his/her P.hD";
                            else
                                Reason1 += "Cosidered as a faculty but not cosidered his/her P.hD";
                        }
                        if (data.NoPGspecialization == true)
                        {
                            if (Reason1 != string.Empty)
                                Reason1 += ",no Specialization in PG";
                            else
                                Reason1 += "no Specialization in PG";
                        }
                        if (data.Genuinenessnotsubmitted == true)
                        {
                            if (Reason1 != string.Empty)
                                Reason1 += ",PHD Genuinity not Submitted ";
                            else
                                Reason1 += "PHD Genuinity not Submitted";
                        }

                        //if (data.Absent == true)
                        //{
                        //    Reason1 = "ABSENT" + ",";
                        //}
                        //if (data.NotQualifiedAsperAICTE == true || degreeid < 4)
                        //{
                        //    Reason1 += "NOT QUALIFIED " + ",";
                        //}
                        //if (string.IsNullOrEmpty(data.PANNumber))
                        //{
                        //    Reason1 += "NO PAN" + ",";
                        //}
                        //if (data.DepartmentId == null)
                        //{
                        //    Reason1 += "No Department" + ",";
                        //}
                        //if (data.NoSCM17 == true)
                        //{
                        //    Reason1 += "NO SCM/RATIFICATION" + ",";
                        //}
                        //if (data.IncompleteCertificates == true)
                        //{
                        //    Reason1 += "Incomplete Certificates" + ",";
                        //}
                        //if (data.Blacklistfaculy == true)
                        //{
                        //    Reason1 += "Blacklisted" + ",";
                        //}
                        //if (data.MultipleRegInSameCollege == true)
                        //{
                        //    Reason1 += "Multiple Reg in Same College" + ",";
                        //}
                        //if (data.NoRelevantUG != "No")
                        //{
                        //    Reason1 += "No Relevant UG" + ",";
                        //}
                        //if (data.NoRelevantPG != "No")
                        //{
                        //    Reason1 += "No Relevant PG" + ",";
                        //}
                        //if (data.NORelevantPHD != "No")
                        //{
                        //    Reason1 += "No Relevant PHD" + ",";
                        //}
                        //if (data.NotIdentityfiedForanyProgram == true)
                        //{
                        //    Reason1 += "Not Identityfied for Any Program" + ",";
                        //}
                        //if (data.InvalidPANNumber == true)
                        //{
                        //    Reason1 += "Invalid PAN No" + ",";
                        //}
                        //if (data.PhdUndertakingDocumentstatus == false)
                        //{
                        //    Reason1 += "No Undertaking" + ",";
                        //}
                        //if (data.AppliedPAN == true)
                        //{
                        //    Reason1 += "Applied PAN" + ",";
                        //}
                        //if (data.SamePANUsedByMultipleFaculty == true)
                        //{
                        //    Reason1 += "Same PAN Usedby Multiple Faculty" + ",";
                        //}
                        //if (data.BASStatusOld == "N")
                        //{
                        //    Reason1 += "Same PAN Usedby Multiple Faculty" + ",";
                        //}








                        //if (Reason1 != "")
                        //{
                        //    Reason1 = Reason1.Substring(0, Reason1.Length - 1);
                        //    faculty.Reason = Reason1;
                        //}
                        if (!String.IsNullOrEmpty(Reason1))
                        {
                            faculty.Reason = Reason1;
                        }
                        //if (data.FalsePAN == true)
                        //{
                        //    newreason += "False Pan" + ",";
                        //}
                        //if (data.NoPhdUndertakingNew == true)
                        //{
                        //    newreason += "No Undertaking" + ",";
                        //}
                        //if (newreason != "")
                        //{
                        //    newreason = newreason.Substring(0, newreason.Length - 1);
                        //    faculty.NewReason = newreason;
                        //}

                        faculty.CollegeFacultiesliList = new List<CollegeFaculty>();
                        var collegeFacultyWithIntakeReport = facultyDetails.FirstOrDefault();
                        if (collegeFacultyWithIntakeReport != null)
                        {
                            collegeFacultyWithIntakeReport.LabsListDefs1.Add(labs);
                            collegeFacultyWithIntakeReport.CollegeFaculties.Add(faculty);
                            collegeFacultyWithIntakeReport.CollegeIntakeExistings.Add(intake);
                            collegeFacultyWithIntakeReport.CollegeIntakeSupportingDocuments.Add(intakesupportingDocuments);
                        }

                    }
                }
                else
                {
                    var faculty = new CollegeFaculty();
                    var labs = new AnonymousLabclass();
                    var intake = new CollegeIntakeExisting();
                    var intakesupportingDocuments = new CollegeIntakeExisting();
                    faculty.CollegeFacultiesliList = new List<CollegeFaculty>();
                    var collegeFacultyWithIntakeReport = facultyDetails.FirstOrDefault();
                    if (collegeFacultyWithIntakeReport != null)
                    {
                        collegeFacultyWithIntakeReport.CollegeFaculties.Add(faculty);
                        collegeFacultyWithIntakeReport.LabsListDefs1.Add(labs);
                        collegeFacultyWithIntakeReport.CollegeIntakeExistings.Add(intake);
                        collegeFacultyWithIntakeReport.CollegeIntakeSupportingDocuments.Add(intakesupportingDocuments);
                    }

                }
            #endregion

                #region Principal
                var Reason = string.Empty;
                var pnewreason = string.Empty;
                if (principalDetails.Count > 0)
                {
                    var item =
                        principalDetails.FirstOrDefault(i => i.collegeId == collegeId && i.NOtificationReport == null);
                    var appitem =
                        principalDetails.FirstOrDefault(i => i.collegeId == collegeId && i.NOtificationReport != null);
                    var principalitem = db.jntuh_college_principal_registered.Where(p => p.collegeId == collegeId).Select(s => s).FirstOrDefault();
                    var faculty = new CollegeFaculty();
                    if (item != null)
                    {
                        var data =
                            jntuh_registered_faculty.Where(
                                e => e.RegistrationNumber.Trim() == item.RegistrationNumber.Trim())
                                .Select(e => e)
                                .FirstOrDefault();

                        faculty.FacultyRegistrationNumber = item.RegistrationNumber;
                        faculty.collegeId = item.collegeId;
                        faculty.id = data.id;
                        faculty.CollegeName =
                            jntuh_college.Where(i => i.id == item.collegeId).Select(i => i.collegeName).FirstOrDefault();
                        faculty.facultyFirstName = data.FirstName;
                        faculty.facultyLastName = data.LastName;
                        faculty.facultySurname = data.MiddleName;
                        faculty.BasstatusOld = data.BAS;
                        faculty.ViewNotificationDocument = item.NOtificationReport;
                        faculty.ViewAppointmentOrderDocument = item.AppointMentOrder;
                        faculty.ViewJoiningReportDocument = item.JoiningOrder;
                        faculty.ViewSelectionCommitteeDocument = item.SelectionCommiteMinutes;
                        faculty.ViewPhdUndertakingDocument = item.PHDUndertakingDocument;
                        faculty.ViewPhysicalPresenceDocument = item.PhysicalPresenceonInspection;

                        //if (data.Absent == true)
                        //{
                        //    Reason = "NOT AVAILABLE" + ",";
                        //}
                        //if (data.NotQualifiedAsperAICTE == true)
                        //{
                        //    Reason += "NOT QUALIFIED " + ",";
                        //}
                        //if (data.InvalidPANNumber == true || string.IsNullOrEmpty(data.PANNumber))
                        //{
                        //    Reason += "NO PAN" + ",";
                        //}
                        //if (data.FalsePAN == true)
                        //{
                        //    Reason += "FALSE PAN" + ",";
                        //}
                        //if (data.NoSCM == true)
                        //{
                        //    Reason += "NO SCM/RATIFICATION" + ",";
                        //}
                        //if (data.IncompleteCertificates == true)
                        //{
                        //    Reason += "Incomplete Certificates" + ",";
                        //}
                        //if (data.PHDundertakingnotsubmitted == true)
                        //{
                        //    Reason += "No Undertaking" + ",";
                        //}
                        //if (data.Blacklistfaculy == true)
                        //{
                        //    Reason += "Blacklisted" + ",";
                        //}
                        //New Code Written by Narayana for Principal Reason
                        if (!string.IsNullOrEmpty(data.DeactivationReason))
                            Reason = data.DeactivationReason;
                        if (data.BAS == "Yes")
                        {
                            if (!String.IsNullOrEmpty(Reason))
                                Reason += ",Not Fulfilling Biometric Attendance";
                            else
                                Reason += "Not Fulfilling Biometric Attendance";
                        }

                        if (Reason != "")
                        {
                            Reason = Reason.Substring(0, Reason.Length - 1);
                            faculty.Reason = Reason;
                        }

                        //if (data.NoPhdUndertakingNew == true)
                        //{
                        //    pnewreason += "No Undertaking" + ",";
                        //}
                        //if (pnewreason != "")
                        //{
                        //    pnewreason = pnewreason.Substring(0, pnewreason.Length - 1);
                        //    faculty.NewReason = pnewreason;
                        //}




                        var collegeFacultyWithIntakeReport = facultyDetails.FirstOrDefault();
                        if (collegeFacultyWithIntakeReport != null)
                        {
                            var collegeFaculty = collegeFacultyWithIntakeReport.CollegeFaculties.FirstOrDefault();
                            if (collegeFaculty != null)
                            {
                                var firstOrDefault = collegeFaculty.CollegeFacultiesliList;
                                if (firstOrDefault != null)
                                    firstOrDefault.Add(faculty);
                            }
                        }
                    }
                    if (item == null && appitem != null)
                    {
                        var data =
                            jntuh_registered_faculty.Where(
                                e => e.RegistrationNumber.Trim() == appitem.RegistrationNumber.Trim())
                                .Select(e => e)
                                .FirstOrDefault();

                        faculty.FacultyRegistrationNumber = appitem.RegistrationNumber;
                        faculty.collegeId = appitem.collegeId;
                        faculty.id = data.id;
                        faculty.CollegeName =
                            jntuh_college.Where(i => i.id == appitem.collegeId).Select(i => i.collegeName).FirstOrDefault();
                        faculty.facultyFirstName = data.FirstName;
                        faculty.facultyLastName = data.LastName;
                        faculty.facultySurname = data.MiddleName;
                        faculty.BasstatusOld = data.BAS;
                        faculty.ViewNotificationDocument = appitem.NOtificationReport;
                        faculty.ViewAppointmentOrderDocument = appitem.AppointMentOrder;
                        faculty.ViewJoiningReportDocument = appitem.JoiningOrder;
                        faculty.ViewSelectionCommitteeDocument = appitem.SelectionCommiteMinutes;
                        faculty.ViewPhdUndertakingDocument = appitem.PHDUndertakingDocument;
                        faculty.ViewPhysicalPresenceDocument = appitem.PhysicalPresenceonInspection;

                        //if (data.Absent == true)
                        //{
                        //    Reason = "NOT AVAILABLE" + ",";
                        //}
                        //if (data.NotQualifiedAsperAICTE == true)
                        //{
                        //    Reason += "NOT QUALIFIED " + ",";
                        //}
                        //if (data.InvalidPANNumber == true || string.IsNullOrEmpty(data.PANNumber))
                        //{
                        //    Reason += "NO PAN" + ",";
                        //}
                        //if (data.FalsePAN == true)
                        //{
                        //    Reason += "FALSE PAN" + ",";
                        //}
                        //if (data.NoSCM == true)
                        //{
                        //    Reason += "NO SCM/RATIFICATION" + ",";
                        //}
                        //if (data.IncompleteCertificates == true)
                        //{
                        //    Reason += "Incomplete Certificates" + ",";
                        //}
                        //if (data.PHDundertakingnotsubmitted == true)
                        //{
                        //    Reason += "No Undertaking" + ",";
                        //}
                        //if (data.Blacklistfaculy == true)
                        //{
                        //    Reason += "Blacklisted" + ",";
                        //}
                        //New Code Written by Narayana for Principal Reason
                        if (!string.IsNullOrEmpty(data.DeactivationReason))
                            Reason = data.DeactivationReason;
                        if (data.BAS == "Yes")
                        {
                            if (!String.IsNullOrEmpty(Reason))
                                Reason += ",Not Fulfilling Biometric Attendance";
                            else
                                Reason += "Not Fulfilling Biometric Attendance";
                        }

                        if (Reason != "")
                        {
                            Reason = Reason.Substring(0, Reason.Length - 1);
                            faculty.Reason = Reason;
                        }

                        //if (data.NoPhdUndertakingNew == true)
                        //{
                        //    pnewreason += "No Undertaking" + ",";
                        //}
                        //if (pnewreason != "")
                        //{
                        //    pnewreason = pnewreason.Substring(0, pnewreason.Length - 1);
                        //    faculty.NewReason = pnewreason;
                        //}




                        var collegeFacultyWithIntakeReport = facultyDetails.FirstOrDefault();
                        if (collegeFacultyWithIntakeReport != null)
                        {
                            var collegeFaculty = collegeFacultyWithIntakeReport.CollegeFaculties.FirstOrDefault();
                            if (collegeFaculty != null)
                            {
                                var firstOrDefault = collegeFaculty.CollegeFacultiesliList;
                                if (firstOrDefault != null)
                                    firstOrDefault.Add(faculty);
                            }
                        }
                    }
                    if (item == null && appitem == null && principalitem != null)
                    {
                        var data =
                            jntuh_registered_faculty.Where(
                                e => e.RegistrationNumber.Trim() == principalitem.RegistrationNumber.Trim())
                                .Select(e => e)
                                .FirstOrDefault();

                        faculty.FacultyRegistrationNumber = principalitem.RegistrationNumber;
                        faculty.collegeId = principalitem.collegeId;
                        faculty.id = data.id;
                        faculty.CollegeName =
                            jntuh_college.Where(i => i.id == principalitem.collegeId).Select(i => i.collegeName).FirstOrDefault();
                        faculty.facultyFirstName = data.FirstName;
                        faculty.facultyLastName = data.LastName;
                        faculty.facultySurname = data.MiddleName;
                        faculty.BasstatusOld = data.BAS;
                        //faculty.ViewNotificationDocument = item.NOtificationReport;
                        //faculty.ViewAppointmentOrderDocument = item.AppointMentOrder;
                        //faculty.ViewJoiningReportDocument = item.JoiningOrder;
                        //faculty.ViewSelectionCommitteeDocument = item.SelectionCommiteMinutes;
                        //faculty.ViewPhdUndertakingDocument = item.PHDUndertakingDocument;
                        //faculty.ViewPhysicalPresenceDocument = item.PhysicalPresenceonInspection;

                        //if (data.Absent == true)
                        //{
                        //    Reason = "NOT AVAILABLE" + ",";
                        //}
                        //if (data.NotQualifiedAsperAICTE == true)
                        //{
                        //    Reason += "NOT QUALIFIED " + ",";
                        //}
                        //if (data.InvalidPANNumber == true || string.IsNullOrEmpty(data.PANNumber))
                        //{
                        //    Reason += "NO PAN" + ",";
                        //}
                        //if (data.FalsePAN == true)
                        //{
                        //    Reason += "FALSE PAN" + ",";
                        //}
                        //if (data.NoSCM == true)
                        //{
                        //    Reason += "NO SCM/RATIFICATION" + ",";
                        //}
                        //if (data.IncompleteCertificates == true)
                        //{
                        //    Reason += "Incomplete Certificates" + ",";
                        //}
                        //if (data.PHDundertakingnotsubmitted == true)
                        //{
                        //    Reason += "No Undertaking" + ",";
                        //}
                        //if (data.Blacklistfaculy == true)
                        //{
                        //    Reason += "Blacklisted" + ",";
                        //}
                        //New Code Written by Narayana for Principal Reason
                        if (!string.IsNullOrEmpty(data.DeactivationReason))
                            Reason = data.DeactivationReason;
                        if (data.BAS == "Yes")
                        {
                            if (!String.IsNullOrEmpty(Reason))
                                Reason += ",Not Fulfilling Biometric Attendance";
                            else
                                Reason += "Not Fulfilling Biometric Attendance";
                        }

                        if (Reason != "")
                        {
                            Reason = Reason.Substring(0, Reason.Length - 1);
                            faculty.Reason = Reason;
                        }

                        //if (data.NoPhdUndertakingNew == true)
                        //{
                        //    pnewreason += "No Undertaking" + ",";
                        //}
                        //if (pnewreason != "")
                        //{
                        //    pnewreason = pnewreason.Substring(0, pnewreason.Length - 1);
                        //    faculty.NewReason = pnewreason;
                        //}




                        var collegeFacultyWithIntakeReport = facultyDetails.FirstOrDefault();
                        if (collegeFacultyWithIntakeReport != null)
                        {
                            var collegeFaculty = collegeFacultyWithIntakeReport.CollegeFaculties.FirstOrDefault();
                            if (collegeFaculty != null)
                            {
                                var firstOrDefault = collegeFaculty.CollegeFacultiesliList;
                                if (firstOrDefault != null)
                                    firstOrDefault.Add(faculty);
                            }
                        }
                    }
                }


                #endregion

                #region For labs
                var collegeLabAnonymousLabclass = new List<AnonymousLabclass>();
                var equipids = db.jntuh_appeal_college_laboratories.Where(e => e.CollegeID == collegeId&&e.academicYearId==11).Select(e => e.EquipmentID).Distinct().ToArray();

                var CollegeAffiliationStatus = db.jntuh_college_affiliation.Where(u => u.collegeId == collegeId && u.affiliationTypeId == 7).Select(u => u.affiliationStatus).FirstOrDefault();
                var CollegeLaboratories = db.jntuh_appeal_college_laboratories.Where(C => C.CollegeID == collegeId&&C.academicYearId==11).ToList();
                if (CollegeAffiliationStatus == "Yes")
                {
                    collegeLabAnonymousLabclass = db.jntuh_lab_master.AsNoTracking()
                                                        .Where(l => equipids.Contains(l.id))
                                                        .Select(l => new AnonymousLabclass
                                                        {
                                                            id = db.jntuh_appeal_college_laboratories.Where(l1 => l1.EquipmentID == l.id && l1.EquipmentNo == 1 && l1.CollegeID == collegeId&&l1.academicYearId==11).Select(l1 => l1.id).FirstOrDefault(),
                                                            EquipmentID = l.id,


                                                            // LabName1 =  CollegeLaboratories.Where(E=>E.EquipmentID==l.id && E.CollegeID==collegeId).Select(E=>E.LabName).FirstOrDefault(),
                                                            //EquipmentName1 =  CollegeLaboratories.Where(E => E.EquipmentID == l.id && E.CollegeID == collegeId).Select(E => E.EquipmentName).FirstOrDefault(),

                                                            LabName = !string.IsNullOrEmpty(l.LabName) ? l.LabName : db.jntuh_appeal_college_laboratories.Where(E => E.EquipmentID == l.id && E.CollegeID == collegeId).Select(E => E.LabName).FirstOrDefault(),
                                                            EquipmentName = !string.IsNullOrEmpty(l.EquipmentName) ? l.EquipmentName : db.jntuh_appeal_college_laboratories.Where(E => E.EquipmentID == l.id && E.CollegeID == collegeId).Select(E => E.EquipmentName).FirstOrDefault(),
                                                            // EquipmentName = l.EquipmentName,
                                                            LabCode = l.Labcode,
                                                        })
                                                        .OrderBy(l => l.LabName)
                                                        .ThenBy(l => l.EquipmentName)
                                                        .ToList();

                    //collegeLabAnonymousLabclass = db.jntuh_lab_master.AsNoTracking()
                    //                                     .Where(l => equipids.Contains(l.id))
                    //                                     .Select(l => new AnonymousLabclass
                    //                                     {
                    //                                         id = db.jntuh_appeal_college_laboratories.Where(l1 => l1.EquipmentID == l.id && l1.EquipmentNo == 1 && l1.CollegeID == collegeId).Select(l1 => l1.id).FirstOrDefault(),
                    //                                         EquipmentID = l.id,


                    //                                        // LabName1 =  CollegeLaboratories.Where(E=>E.EquipmentID==l.id && E.CollegeID==collegeId).Select(E=>E.LabName).FirstOrDefault(),
                    //                                         //EquipmentName1 =  CollegeLaboratories.Where(E => E.EquipmentID == l.id && E.CollegeID == collegeId).Select(E => E.EquipmentName).FirstOrDefault(),

                    //                                         LabName = !string.IsNullOrEmpty(l.LabName) ? l.LabName : CollegeLaboratories.Select(E=>E.LabName).FirstOrDefault(),
                    //                                         EquipmentName = !string.IsNullOrEmpty(l.EquipmentName) ?l.EquipmentName:"BB",
                    //                                        // EquipmentName = l.EquipmentName,
                    //                                         LabCode = l.Labcode,
                    //                                     })
                    //                                     .OrderBy(l => l.LabName)
                    //                                     .ThenBy(l => l.EquipmentName)
                    //                                     .ToList();

                }
                else if (CollegeAffiliationStatus == "No" || CollegeAffiliationStatus == null)
                {

                    collegeLabAnonymousLabclass = db.jntuh_lab_master.AsNoTracking()
                                                       .Where(l => equipids.Contains(l.id) && l.Labcode != "TMP-CL")
                                                       .Select(l => new AnonymousLabclass
                                                       {
                                                           id = db.jntuh_appeal_college_laboratories.Where(l1 => l1.EquipmentID == l.id&&l1.academicYearId==11 && l1.EquipmentNo == 1 && l1.CollegeID == collegeId).Select(l1 => l1.id).FirstOrDefault(),
                                                           EquipmentID = l.id,
                                                           LabName = l.LabName,
                                                           EquipmentName = l.EquipmentName,
                                                           LabCode = l.Labcode,
                                                       })
                                                       .OrderBy(l => l.LabName)
                                                       .ThenBy(l => l.EquipmentName)
                                                       .ToList();
                }


                var list1 = collegeLabAnonymousLabclass.OrderBy(c => c.LabName).ThenBy(c => c.EquipmentName).Distinct().ToList();

                if (facultyDetails.Count > 0)
                {
                    var collegeFacultyWithIntakeReport = facultyDetails.FirstOrDefault();
                    if (collegeFacultyWithIntakeReport != null)
                        collegeFacultyWithIntakeReport.LabsListDefs1 = list1.ToList();
                }

                #endregion

                #region Intake
                var jntuh_academic_year = db.jntuh_academic_year.ToList();
                var jntuh_degree = db.jntuh_degree.AsNoTracking().ToList();
                var collegeIntakeExisting = new List<CollegeIntakeExisting>();
                var collegeIntakes1 = db.jntuh_appeal_college_intake_existing.AsNoTracking().Where(i => i.collegeId == collegeId).ToList();
                var collegeIntakes = db.jntuh_college_intake_existing.AsNoTracking().Where(i => i.collegeId == collegeId && i.academicYearId == 11 && i.proposedIntake != 0).ToList();
                var presentYear = jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
                var actualYear = jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
                ViewBag.FirstYear = String.Format("{0}-{1}", (actualYear).ToString(), (actualYear + 1).ToString().Substring(2, 2));
                ViewBag.SecondYear = String.Format("{0}-{1}", (actualYear - 1).ToString(), (actualYear).ToString().Substring(2, 2));
                ViewBag.ThirdYear = String.Format("{0}-{1}", (actualYear - 2).ToString(), (actualYear - 1).ToString().Substring(2, 2));
                ViewBag.FourthYear = String.Format("{0}-{1}", (actualYear - 3).ToString(), (actualYear - 2).ToString().Substring(2, 2));
                ViewBag.FifthYear = String.Format("{0}-{1}", (actualYear - 4).ToString(), (actualYear - 3).ToString().Substring(2, 2));

                var AY0 = jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();
                var AYY1 = jntuh_academic_year.Where(a => a.actualYear == (presentYear)).Select(a => a.id).FirstOrDefault();
                var AYY2 = jntuh_academic_year.Where(a => a.actualYear == (presentYear - 1)).Select(a => a.id).FirstOrDefault();
                var AYY3 = jntuh_academic_year.Where(a => a.actualYear == (presentYear - 2)).Select(a => a.id).FirstOrDefault();
                var AYY4 = jntuh_academic_year.Where(a => a.actualYear == (presentYear - 3)).Select(a => a.id).FirstOrDefault();
                var AYY5 = jntuh_academic_year.Where(a => a.actualYear == (presentYear - 4)).Select(a => a.id).FirstOrDefault();

                foreach (var item in collegeIntakes)
                {
                    var newIntake = new CollegeIntakeExisting();
                    newIntake.id = item.id;
                    newIntake.collegeId = item.collegeId;
                    newIntake.academicYearId = item.academicYearId;
                    newIntake.shiftId = item.shiftId;
                    newIntake.isActive = item.isActive;
                    newIntake.nbaFrom = item.nbaFrom;
                    newIntake.nbaTo = item.nbaTo;
                    newIntake.specializationId = item.specializationId;
                    var jntuhSpecialization = jntuh_specialization.FirstOrDefault(i => i.id == item.specializationId);
                    if (jntuhSpecialization != null)
                    {
                        newIntake.Specialization = jntuhSpecialization.specializationName;
                        newIntake.DepartmentID = jntuhSpecialization.departmentId;
                        newIntake.Department = jntuh_department.FirstOrDefault(i => i.id == jntuhSpecialization.departmentId).departmentName;
                        newIntake.degreeID = jntuh_department.FirstOrDefault(i => i.id == jntuhSpecialization.departmentId).degreeId;
                        newIntake.Degree = jntuh_degree.FirstOrDefault(i => i.id == newIntake.degreeID).degree;
                    }

                    //newIntake.AICTEApprovalLettr = item.IntakeApprovalLetter != null ? item + ".pdf" : "";
                    newIntake.AICTEApprovalLettr = item.NBAApproveLetter != null ? item + ".pdf" : "";
                    newIntake.shiftId = item.shiftId;
                    collegeIntakeExisting.Add(newIntake);
                }
                collegeIntakeExisting = collegeIntakeExisting.AsEnumerable().GroupBy(r => new { r.specializationId, r.shiftId, r.collegeId }).Select(r => r.First()).ToList();

                var intakelists = new List<CollegeIntakeExisting>();
                foreach (var item in collegeIntakeExisting)
                {

                    if (item.nbaFrom != null)
                        item.nbaFromDate = Utilities.MMDDYY2DDMMYY(item.nbaFrom.ToString());
                    if (item.nbaTo != null)
                        item.nbaToDate = Utilities.MMDDYY2DDMMYY(item.nbaTo.ToString());

                    //FLAG : 1 - Approved, 0 - Admitted
                    //var details = db.jntuh_appeal_college_intake_existing.Where(e => e.collegeId == collegeId && e.academicYearId == AY0 && e.specializationId == item.specializationId)
                    //                                          .Select(e => e)
                    //                                          .FirstOrDefault();
                    //if (details != null)
                    //{
                    //    item.ApprovedIntake = details.approvedIntake;
                    //    item.letterPath = details.approvalLetter;
                    //    item.ProposedIntake = details.proposedIntake;
                    //    item.courseStatus = details.courseStatus;
                    //}
                    //else
                    //{
                    //item.ApprovedIntake = item.approvedIntake;
                    //item.letterPath = item.approvalLetter;
                    item.ProposedIntake = GetIntake(collegeId, AY0, item.specializationId, item.shiftId, 1);
                    item.courseStatus = item.courseStatus;
                    //}
                    item.approvedIntake1 = GetIntake(collegeId, AYY1, item.specializationId, item.shiftId, 1);
                    item.admittedIntake1 = GetIntake(collegeId, AYY1, item.specializationId, item.shiftId, 0);

                    item.approvedIntake2 = GetIntake(collegeId, AYY2, item.specializationId, item.shiftId, 1);
                    item.admittedIntake2 = GetIntake(collegeId, AYY2, item.specializationId, item.shiftId, 0);

                    item.approvedIntake3 = GetIntake(collegeId, AYY3, item.specializationId, item.shiftId, 1);
                    item.admittedIntake3 = GetIntake(collegeId, AYY3, item.specializationId, item.shiftId, 0);

                    item.approvedIntake4 = GetIntake(collegeId, AYY4, item.specializationId, item.shiftId, 1);
                    item.admittedIntake4 = GetIntake(collegeId, AYY4, item.specializationId, item.shiftId, 0);

                    item.approvedIntake5 = GetIntake(collegeId, AYY5, item.specializationId, item.shiftId, 1);
                    item.admittedIntake5 = GetIntake(collegeId, AYY5, item.specializationId, item.shiftId, 0);

                    intakelists.Add(item);


                }

                facultyDetails.FirstOrDefault().CollegeIntakeExistings = intakelists.Count > 0 ? intakelists.Where(i => i.shiftId == 1).OrderBy(ei => ei.degreeDisplayOrder).ThenBy(ei => ei.Department).ThenBy(ei => ei.Specialization).ThenBy(ei => ei.shiftId).ToList() : new List<CollegeIntakeExisting>();
                #endregion

                #region IntakeSuppportingDocuments
                //This is code is commentted for 2018-2019 
                var supportingDocuments = db.jntuh_appeal_college_intake_existing_supportingdocuments.AsNoTracking().Where(i => i.collegeId == collegeId).ToList();
                var collegeIntakeSupportingDocs = new List<CollegeIntakeExisting>();
                foreach (var item in supportingDocuments)
                {
                    var newIntake = new CollegeIntakeExisting();
                    newIntake.id = item.id;
                    newIntake.collegeId = item.collegeId;
                    newIntake.academicYearId = item.academicYearId;
                    newIntake.shiftId = item.shiftId;
                    newIntake.specializationId = item.specializationId;
                    var jntuhSpecialization = jntuh_specialization.FirstOrDefault(i => i.id == item.specializationId);
                    if (jntuhSpecialization != null)
                    {
                        newIntake.Specialization = jntuhSpecialization.specializationName;
                        newIntake.DepartmentID = jntuhSpecialization.departmentId;
                        newIntake.Department = jntuh_department.FirstOrDefault(i => i.id == jntuhSpecialization.departmentId).departmentName;
                        newIntake.degreeID = jntuh_department.FirstOrDefault(i => i.id == jntuhSpecialization.departmentId).degreeId;
                        newIntake.Degree = jntuh_degree.FirstOrDefault(i => i.id == newIntake.degreeID).degree;
                    }

                    newIntake.ViewSCMApprovalLetter = item.SCM != null ? item.SCM + ".pdf" : "";
                    newIntake.ViewForm16ApprovalLetter = item.FORM16 != null ? item.FORM16 + ".pdf" : "";
                    newIntake.shiftId = item.shiftId;
                    collegeIntakeSupportingDocs.Add(newIntake);
                }
                collegeIntakeSupportingDocs = collegeIntakeSupportingDocs.AsEnumerable().GroupBy(r => new { r.specializationId, r.shiftId, r.collegeId }).Select(r => r.First()).ToList();


                facultyDetails.FirstOrDefault().CollegeIntakeSupportingDocuments = collegeIntakeSupportingDocs.Count > 0 ? collegeIntakeSupportingDocs.Where(i => i.shiftId == 1).OrderBy(ei => ei.degreeDisplayOrder).ThenBy(ei => ei.Department).ThenBy(ei => ei.Specialization).ThenBy(ei => ei.shiftId).ToList() : new List<CollegeIntakeExisting>();
                #endregion

                #region Remarks&OthersupportingDocs

                var collegeeditdetails = db.jntuh_appeal_college_edit_status.AsNoTracking().FirstOrDefault(i => i.collegeId == collegeId);

                if (collegeeditdetails != null && collegeeditdetails.DeclarationPath != null)
                {
                    facultyDetails.FirstOrDefault().DeclarationPath = collegeeditdetails.DeclarationPath;
                    facultyDetails.FirstOrDefault().Remarks = collegeeditdetails.Remarks;
                    facultyDetails.FirstOrDefault().FurtherAppealSupportingDoc = collegeeditdetails.FurtherAppealSupportingDocument;
                }

                #endregion


                #region Physical Lab Master

                ////List<PhysicalLabMaster> physicallabs = new List<PhysicalLabMaster>();
                ////physicallabs = db.jntuh_college_laboratories_deficiency.Where(e => e.CollegeId == collegeId).Select(e => new PhysicalLabMaster
                ////{
                ////    NoofAvailable = e.Semister,
                ////    DepartmentName = e.LabCode
                ////}).ToList();
                List<PhysicalLabMaster> physicallabs = new List<PhysicalLabMaster>();
                List<UAAAS.Controllers.Reports.DeficiencyReportWordController.physicalLabs> CollegePhysicalLabMaster = new List<UAAAS.Controllers.Reports.DeficiencyReportWordController.physicalLabs>();
                CollegePhysicalLabMaster =
                     db.jntuh_physical_labmaster_copy.Where(e => e.Collegeid == collegeId && e.Numberofrequiredlabs != null)
                         .Select(e => new UAAAS.Controllers.Reports.DeficiencyReportWordController.physicalLabs
                         {
                             department = db.jntuh_department.Where(d => d.id == e.DepartmentId).Select(s => s.departmentName).FirstOrDefault(),
                             NoOfRequiredLabs = e.Numberofrequiredlabs,
                             Labname = e.LabName,
                             year = e.Year,
                             semister = e.Semister,
                             LabCode = e.Labcode,
                             NoOfAvailabeLabs = db.jntuh_physical_labmaster_copy.Where(a => a.Collegeid == collegeId && a.Semister == e.Semister && a.Year == e.Year && a.Labcode == e.Labcode).Select(s => s.Numberofavilablelabs).FirstOrDefault() == null ? 0 : db.jntuh_physical_labmaster_copy.Where(a => a.Collegeid == collegeId && a.DepartmentId == e.DepartmentId && a.Semister == e.Semister && a.Year == e.Year && a.Labcode == e.Labcode).Select(s => s.Numberofavilablelabs).FirstOrDefault()
                         }).ToList();
                foreach (var item in CollegePhysicalLabMaster)
                {
                    if (item.NoOfAvailabeLabs < item.NoOfRequiredLabs)
                    {
                        PhysicalLabMaster PhysicalLabMaster = new PhysicalLabMaster();
                        PhysicalLabMaster.DepartmentName = item.department;
                        PhysicalLabMaster.NoofAvailable = (int)item.NoOfAvailabeLabs;
                        PhysicalLabMaster.NoofRequeried = (int)item.NoOfRequiredLabs;
                        PhysicalLabMaster.Labname = item.Labname;
                        physicallabs.Add(PhysicalLabMaster);
                    }
                }

                string physicalpath = db.jntuh_college_enclosures.Where(e => e.enclosureId == 26 && e.collegeID == collegeId&&e.academicyearId==11).Select(e => e.path).FirstOrDefault();
                if (!string.IsNullOrEmpty(physicalpath))
                {
                    if (physicallabs.Count != 0)
                    {
                        physicallabs[0].PhysicalLabUploadingview = physicalpath;
                    }
                }

                if (facultyDetails != null)
                {
                    facultyDetails.FirstOrDefault().PhysicalLabMasters = physicallabs;
                }
                #endregion


            }
            return View(facultyDetails);
        }

        //Appeal Faculty Details
        [Authorize(Roles = "Admin,SuperAdmin,FacultyVerification")]
        public ActionResult ViewFacultyDetails(int? collegeId)
        {
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int prAy =
                db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
            var colgids = db.jntuh_appeal_college_edit_status.Where(i => i.IsCollegeEditable == false && i.academicyearid == prAy).GroupBy(e => e.collegeId).Select(e => e.Key).Distinct().ToArray();
            ViewBag.Colleges = db.jntuh_college.Where(e => e.isActive && colgids.Contains(e.id)).Select(e => new { collegeId = e.id, collegeName = e.collegeCode + "_" + e.collegeName }).OrderBy(e => e.collegeName).ToList();
            
            //var jntuh_college = db.jntuh_college.AsNoTracking().ToList();
            var facultyDetails = new List<CollegeFaculty>();
            if (collegeId != null)
            {
                var appealfacultyDetails = db.jntuh_appeal_faculty_registered.Where(r => r.academicYearId == prAy && r.collegeId == collegeId).ToList();
                var principalDetails = db.jntuh_appeal_principal_registered.Where(r => r.academicYearId == prAy && r.collegeId == collegeId).ToList();
                var jntuh_registered_faculty = db.jntuh_registered_faculty.AsNoTracking().ToList();
                var departments = db.jntuh_department.AsNoTracking().ToList();
                var specialization = db.jntuh_specialization.AsNoTracking().ToList();
                var degree = db.jntuh_degree.AsNoTracking().ToList();
                var jntuh_registered_faculty_education = db.jntuh_registered_faculty_education.AsNoTracking();
                #region Principal
                var Reason1 = string.Empty;
                var NewReason1 = string.Empty;
                if (principalDetails.Count > 0)
                {
                    var item = principalDetails.FirstOrDefault(i => i.collegeId == collegeId);

                    if (item != null)
                    {
                        var data = jntuh_registered_faculty.Where(e => e.RegistrationNumber.Trim() == item.RegistrationNumber.Trim()).Select(e => e).FirstOrDefault();

                        ViewBag.RegistrationNumber = item.RegistrationNumber;
                        ViewBag.collegeId = item.collegeId;
                        if (data != null)
                        {
                            ViewBag.Status = true;
                            ViewBag.id = data.id;
                            ViewBag.FullName = data.FirstName + " " + data.LastName + " " + data.MiddleName;
                            ViewBag.ViewNotificationDocument = item.NOtificationReport;
                            ViewBag.ViewAppointmentOrderDocument = item.AppointMentOrder;
                            ViewBag.ViewJoiningReportDocument = item.JoiningOrder;
                            ViewBag.ViewSelectionCommitteeDocument = item.SelectionCommiteMinutes;
                            ViewBag.ViewPhdUndertakingDocument = item.PHDUndertakingDocument;
                            ViewBag.ViewPhysicalPresenceDocument = item.PhysicalPresenceonInspection;
                            ViewBag.isActive = item.isActive;
                            //if (data.Absent == true)
                            //{
                            //    Reason1 = "NOT AVAILABLE" + ",";
                            //}
                            //if (data.NotQualifiedAsperAICTE == true)
                            //{
                            //    Reason1 += "NOT QUALIFIED " + ",";
                            //}
                            //if (data.InvalidPANNumber == true)
                            //{
                            //    Reason1 += "NO PAN" + ",";
                            //}
                            //if (data.FalsePAN == true)
                            //{
                            //    Reason1 += "FALSE PAN" + ",";
                            //}
                            //if (data.NoSCM == true)
                            //{
                            //    Reason1 += "NO SCM/RATIFICATION" + ",";
                            //}
                            //if (data.IncompleteCertificates == true)
                            //{
                            //    Reason1 += "Incomplete Certificates" + ",";
                            //}
                            //if (data.PHDundertakingnotsubmitted == true)
                            //{
                            //    Reason1 += "No Undertaking" + ",";
                            //}
                            //if (data.Blacklistfaculy == true)
                            //{
                            //    Reason1 += "Blacklisted" + ",";
                            //}

                            //if (data.NoPhdUndertakingNew == true)
                            //{
                            //    Reason1 += "No Undertaking" + ",";
                            //}
                            //if (data.Blacklistfaculy == true)
                            //{
                            //    NewReason1 += "Blacklisted" + ",";
                            //}

                            if (!string.IsNullOrEmpty(data.DeactivationReason))
                            {
                                Reason1 = data.DeactivationReason;
                            }
                            if (data.BAS == "Yes")
                            {
                                if (!String.IsNullOrEmpty(Reason1))
                                    Reason1 += ",Not Fulfilling Biometric Attendance";
                                else
                                    Reason1 += "Not Fulfilling Biometric Attendance";
                            }         

                        }
                        if (Reason1 != "")
                        {
                            Reason1 = Reason1;
                            ViewBag.Reason = Reason1;
                        }
                    }
                }
                #endregion


                #region For Faculty
                appealfacultyDetails = appealfacultyDetails.Where(e => e.collegeId == collegeId).ToList();
                foreach (var item in appealfacultyDetails)
                {
                    var Reason = string.Empty;
                    var newreason = string.Empty;
                    var data = jntuh_registered_faculty.Where(e => e.RegistrationNumber.Trim() == item.RegistrationNumber.Trim()).Select(e => e).FirstOrDefault();
                    var degreeid = data != null && data.jntuh_registered_faculty_education.Where(r=>r.educationId!=8).Select(s=>s).ToList().Count > 0 ? data.jntuh_registered_faculty_education.Where(s=>s.educationId!=8).Select(e => e.educationId).Max() : 0;
                    if (data != null)
                    {
                        if (data.Absent == true)
                            Reason += "Absent";
                        if (data.type == "Adjunct")
                        {
                            if (Reason != string.Empty)
                                Reason += ",Adjunct Faculty";
                            else
                                Reason += "Adjunct Faculty";
                        }
                        if (data.OriginalCertificatesNotShown == true)
                        {
                            if (Reason != string.Empty)
                                Reason += ",Orginal Certificates Not shown in College Inspection";
                            else
                                Reason += "Orginal Certificates Not shown in College Inspection";
                        }
                        if (data.Xeroxcopyofcertificates == true)
                        {
                            if (Reason != string.Empty)
                                Reason += ",Photo copy of Certificates";
                            else
                                Reason += "Photo copy of Certificates";
                        }
                        if (data.NotQualifiedAsperAICTE == true||degreeid<4)
                        {
                            if (Reason != string.Empty)
                                Reason += ",Not Qualified as per AICTE/PCI";
                            else
                                Reason += "Not Qualified as per AICTE/PCI";
                        }
                        if (data.NoSCM == true)
                        {
                            if (Reason != string.Empty)
                                Reason += ",no/not valid SCM";
                            else
                                Reason += "no/not valid SCM";
                        }
                        if (data.PANNumber == null)
                        {
                            if (Reason != string.Empty)
                                Reason += ",No PAN Number";
                            else
                                Reason += "No PAN Number";
                        }
                        if (data.IncompleteCertificates == true)
                        {
                            if (Reason != string.Empty)
                                Reason += ",IncompleteCertificates";
                            else
                                Reason += "IncompleteCertificates";
                        }
                        if (data.Blacklistfaculy == true)
                        {
                            if (Reason != string.Empty)
                                Reason += ",Blacklisted Faculty";
                            else
                                Reason += "Blacklisted Faculty";
                        }
                        if (data.NoRelevantUG == "Yes")
                        {
                            if (Reason != string.Empty)
                                Reason += ",NO Relevant UG";
                            else
                                Reason += "NO Relevant UG";
                        }
                        if (data.NoRelevantPG == "Yes")
                        {
                            if (Reason != string.Empty)
                                Reason += ",NO Relevant PG";
                            else
                                Reason += "NO Relevant PG";
                        }
                        if (data.NORelevantPHD == "Yes")
                        {
                            if (Reason != string.Empty)
                                Reason += ",NO Relevant PHD";
                            else
                                Reason += "NO Relevant PHD";
                        }
                        if (data.InvalidPANNumber == true)
                        {
                            if (Reason != string.Empty)
                                Reason += ",InvalidPANNumber";
                            else
                                Reason += "InvalidPANNumber";
                        }
                        if (data.OriginalsVerifiedPHD == true)
                        {
                            if (Reason != string.Empty)
                                Reason += ",No Guide Sign in PHD Thesis";
                            else
                                Reason += "No Guide Sign in PHD Thesis";
                        }
                        if (data.OriginalsVerifiedUG == true)
                        {
                            if (Reason != string.Empty)
                                Reason += ",Complaint PHD Faculty";
                            else
                                Reason += "Complaint PHD Faculty";
                        }
                        if (data.Invaliddegree == true)
                        {
                            if (Reason != string.Empty)
                                Reason += ",AICTE Not Approved University Degrees";
                            else
                                Reason += "AICTE Not Approved University Degrees";
                        }
                        if (data.BAS == "Yes")
                        {
                            if (Reason != string.Empty)
                                Reason += ",BAS Flag";
                            else
                                Reason += "BAS Flag";
                        }
                        if (data.InvalidAadhaar == "Yes")
                        {
                            if (Reason != string.Empty)
                                Reason += ",Invalid/Blur Aadhaar";
                            else
                                Reason += "Invalid/Blur Aadhaar";
                        }
                        if (data.Noclass == true)
                        {
                            if (Reason != string.Empty)
                                Reason += ",No Class in UG/PG";
                            else
                                Reason += "No Class in UG/PG";
                        }
                        if (data.AbsentforVerification == true)
                        {
                            if (Reason != string.Empty)
                                Reason += ",Absentfor Physical Verification";
                            else
                                Reason += "Absentfor Physical Verification";
                        }
                        if (data.NotconsideredPHD == true)
                        {
                            if (Reason != string.Empty)
                                Reason += ",Cosidered as a faculty but not cosidered his/her P.hD";
                            else
                                Reason += "Cosidered as a faculty but not cosidered his/her P.hD";
                        }
                        if (data.NoPGspecialization == true)
                        {
                            if (Reason != string.Empty)
                                Reason += ",no Specialization in PG";
                            else
                                Reason += "no Specialization in PG";
                        }
                        if (data.Genuinenessnotsubmitted == true)
                        {
                            if (Reason != string.Empty)
                                Reason += ",PHD Genuinity not Submitted ";
                            else
                                Reason += "PHD Genuinity not Submitted";
                        }

                        //if (data.NotIdentityfiedForanyProgram == true)
                        //{
                        //    if (Reason != string.Empty)
                        //        Reason += ",NotIdentityFied ForAnyProgram";
                        //    else
                        //        Reason += "NotIdentityFied ForAnyProgram";
                        //}                      

                        var faculty = new CollegeFaculty
                        {
                            FacultyRegistrationNumber = item.RegistrationNumber,
                            collegeId = item.collegeId,
                            id = jntuh_registered_faculty.Where(e => e.RegistrationNumber.Trim() == item.RegistrationNumber.Trim()).Select(e => e.id).FirstOrDefault(),
                            facultyFirstName = data.FirstName,
                            facultyLastName = data.LastName,
                            facultySurname = data.MiddleName,
                            //department = (data.DepartmentId != null) ? departments.Where(D => D.id == data.DepartmentId).Select(D => D.departmentName).FirstOrDefault() : "",
                            IdentifiedFor=item.IdentifiedFor,
                            department = (item.DepartmentId != null) ? departments.Where(D => D.id == item.DepartmentId).Select(D => D.departmentName).FirstOrDefault() : "",
                            SpecializationName = (item.SpecializationId != null) ? specialization.Where(D => D.id == item.SpecializationId).Select(D => D.specializationName).FirstOrDefault() : "",
                            ViewNotificationDocument = item.NOtificationReport,
                            DegreeId =(item.DepartmentId != null) ? departments.Where(D => D.id == item.DepartmentId).Select(D => D.degreeId).FirstOrDefault() : 0,
                            DegreeName = (item.DegreeId != null) ? degree.Where(D => D.id == item.DegreeId).Select(D => D.degree).FirstOrDefault() : "",
                            ViewAppointmentOrderDocument = item.AppointMentOrder,
                            ViewJoiningReportDocument = item.JoiningOrder,
                            
                            ViewSelectionCommitteeDocument = item.SelectionCommiteMinutes,
                            ViewAppealReverificationSupportDoc = item.AppealReverificationSupportingDocument,
                            isActive = item.isActive,
                            Reason = Reason,
                            NewReason = newreason
                        };
                        facultyDetails.Add(faculty);
                    }
                }
                #endregion
                
            }
            return View(facultyDetails);
        }

        #region add Department and Specilaction
        //add Department Dialog

        [Authorize(Roles = "Admin")]
        public ActionResult AppealFacultyAddDepartment(string fid, int collegeid)//collegeid
        {
            var faculty = new CollegeFaculty();
            var fId = 0;
            if (!string.IsNullOrEmpty(fid))
            {
                fId = Convert.ToInt32(Utilities.DecryptString(fid, WebConfigurationManager.AppSettings["CryptoKey"]));
            }
            var facultyregistered = db.jntuh_registered_faculty.Find(fId);
            if (facultyregistered != null)
            {
                faculty.collegeId = collegeid;
                faculty.id = facultyregistered.id;
                faculty.facultyFirstName = facultyregistered.FirstName;
                faculty.facultyLastName = facultyregistered.LastName;
                faculty.facultySurname = facultyregistered.MiddleName;
                faculty.facultyDesignationId = facultyregistered.DesignationId;
                faculty.designation = db.jntuh_designation.Where(d => d.id == faculty.facultyDesignationId).Select(d => d.designation).FirstOrDefault();
                faculty.facultyOtherDesignation = facultyregistered.OtherDesignation;
                faculty.FacultyRegistrationNumber = facultyregistered.RegistrationNumber;

            }


            List<int> deptIds = new List<int>() {71, 72, 73, 74, 75, 76, 77, 78};
            var depts = db.jntuh_department.Join(db.jntuh_degree, dept => dept.degreeId, Deg => Deg.id, (dept, Deg) => new { dept = dept, Deg = Deg }).Where(e => e.dept.isActive == true && !deptIds.Contains(e.dept.id)).Select(e => new { id = e.dept.id, departmentName = e.Deg.degree + "-" + e.dept.departmentName }).ToList();
            ViewBag.department = depts;

            var Specialization = (from de in db.jntuh_degree
                join dep in db.jntuh_department on de.id equals dep.degreeId
                join spec in db.jntuh_specialization on dep.id equals spec.departmentId
                                  where dep.isActive == true && !deptIds.Contains(dep.id)
                select new
                {
                    id = spec.id,
                    spec = de.degree + "-" + spec.specializationName
                }).ToList();

            var pgSpecializations =db.jntuh_specialization.Select(e => new { id = e.id, spec = e.specializationName }).ToList();


            ViewBag.PGSpecializations = Specialization;

            return PartialView(faculty);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public ActionResult AppealFacultyAddDepartment(CollegeFaculty faculty)
        {
            int userId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            TempData["Error"] = null;
            var jntuh_registered_faculty = db.jntuh_registered_faculty.AsNoTracking().ToList();
            var jntuh_college_faculty_registered = db.jntuh_college_faculty_registered.AsNoTracking().ToList();
           // var jntuh_college_nodepartments = db.jntuh_college_nodepartments.AsNoTracking().ToList();

            var isExistingFaculty = jntuh_registered_faculty.FirstOrDefault(r => r.RegistrationNumber == faculty.FacultyRegistrationNumber);
            if (isExistingFaculty != null)
            {
               // jntuh_registered_faculty facultyregistered = db.jntuh_registered_faculty.Find(isExistingFaculty.id);
               //// facultyregistered.DepartmentId = faculty.facultyDepartmentId;
               // facultyregistered.Absent = false;
               // facultyregistered.NotQualifiedAsperAICTE = false;
               // facultyregistered.NoSCM17 = false;
               // facultyregistered.IncompleteCertificates = false;
               // facultyregistered.Blacklistfaculy = false;
               // facultyregistered.MultipleRegInSameCollege = false;
               // facultyregistered.NoRelevantUG = "No";
               // facultyregistered.NoRelevantPG = "No";
               // facultyregistered.NORelevantPHD = "No";
               // facultyregistered.NotIdentityfiedForanyProgram = false;
               // facultyregistered.InvalidPANNumber = false;
               // facultyregistered.PhdUndertakingDocumentstatus = true;
               // facultyregistered.AppliedPAN = false;
               // facultyregistered.SamePANUsedByMultipleFaculty = false;
               // facultyregistered.BASStatusOld = "Y";
               // //facultyregistered.MultipleRegInDiffCollege = false;
               // //facultyregistered.PhotoCopyofPAN = false;
               // //facultyregistered.LostPAN = false;
               // //facultyregistered.OriginalsVerifiedUG = false;
               // //facultyregistered.OriginalsVerifiedPG = false;
               // //facultyregistered.OriginalsVerifiedPHD = false;
               // //facultyregistered.FacultyVerificationStatus = false;
               // //facultyregistered.IncompleteCertificates = false;
               // //facultyregistered.FalsePAN = false;
               // facultyregistered.updatedBy = userId;
               // facultyregistered.updatedOn = DateTime.Now;
               // db.Entry(facultyregistered).State = EntityState.Modified;
               // db.SaveChanges();



                var iscollegefacultyexist =jntuh_college_faculty_registered.FirstOrDefault(r => r.RegistrationNumber == faculty.FacultyRegistrationNumber);
                if (iscollegefacultyexist != null)
                {
                    jntuh_college_faculty_registered collegefaculty =db.jntuh_college_faculty_registered.Find(iscollegefacultyexist.id);
                    collegefaculty.DepartmentId = faculty.facultyDepartmentId;
                    int degreeid =
                        db.jntuh_department.Where(d => d.id == collegefaculty.DepartmentId)
                            .Select(s => s.degreeId)
                            .FirstOrDefault();
                    if (degreeid == 4 || degreeid == 5)
                    {
                        collegefaculty.IdentifiedFor = "UG";
                        collegefaculty.SpecializationId = null;
                    }
                    else
                    {
                        collegefaculty.IdentifiedFor = "PG";
                        collegefaculty.SpecializationId = faculty.SpecializationId;
                    }
                    
                    db.Entry(collegefaculty).State = EntityState.Modified;
                    db.SaveChanges();
                }

                //var isappealfacuty =db.jntuh_appeal_faculty_registered.AsNoTracking().Where(e => e.RegistrationNumber == faculty.FacultyRegistrationNumber).Select(e => e).FirstOrDefault();

                //if (isappealfacuty != null)
                //{
                //    jntuh_appeal_faculty_registered appealgaculty =db.jntuh_appeal_faculty_registered.Find(isappealfacuty.id);
                //    appealgaculty.isActive = true;
                //    db.Entry(appealgaculty).State = EntityState.Modified;
                //    db.SaveChanges();
                //}

               

                TempData["Success"] = "Faculty Department & Specialization (" + faculty.FacultyRegistrationNumber + " ) Successfully Updated ..";
                TempData["Error"] = null;

            }
            return RedirectToAction("ViewFacultyDetails", "AppealFormat", new { @collegeid = faculty.collegeId });
        }

        #endregion
        [Authorize(Roles = "Admin,SuperAdmin,FacultyVerification")]
        public ActionResult ViewAppealLabsDetails(int? collegeId)
        {
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int prAy =
                db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
            var colgids = db.jntuh_appeal_college_edit_status.Where(i => i.IsCollegeEditable == false&&i.academicyearid==prAy).GroupBy(e => e.collegeId).Select(e => e.Key).Distinct().ToArray();
            ViewBag.Colleges = db.jntuh_college.Where(e => e.isActive && colgids.Contains(e.id)).Select(e => new { collegeId = e.id, collegeName = e.collegeCode + "_" + e.collegeName }).OrderBy(e => e.collegeName).ToList();
            var collegeLabAnonymousLabclass = new List<AnonymousLabclass>();
            if (collegeId != null)
            {
               
                var equipids = db.jntuh_appeal_college_laboratories.Where(e => e.CollegeID == collegeId&&e.academicYearId==prAy).Select(e => e.EquipmentID).Distinct().ToArray();

                var CollegeAffiliationStatus = db.jntuh_college_affiliation.Where(u => u.collegeId == collegeId && u.affiliationTypeId == 7).Select(u => u.affiliationStatus).FirstOrDefault();
               // var CollegeLaboratories = db.jntuh_appeal_college_laboratories.Where(C => C.CollegeID == collegeId).ToList();
                if (CollegeAffiliationStatus == "Yes")
                {
                    collegeLabAnonymousLabclass = db.jntuh_lab_master.AsNoTracking()
                                                        .Where(l => equipids.Contains(l.id) && l.CollegeId==collegeId)
                                                        .Select(l => new AnonymousLabclass
                                                        {
                                                            id = db.jntuh_appeal_college_laboratories.Where(l1 => l1.EquipmentID == l.id && l1.EquipmentNo == 1 && l1.CollegeID == collegeId).Select(l1 => l1.id).FirstOrDefault(),
                                                            EquipmentID = l.id,
                                                            IsActive = (bool) db.jntuh_appeal_college_laboratories.Where(l1 => l1.EquipmentID == l.id && l1.EquipmentNo == 1 && l1.CollegeID == collegeId).Select(l1 => l1.isActive).FirstOrDefault(),

                                                            // LabName1 =  CollegeLaboratories.Where(E=>E.EquipmentID==l.id && E.CollegeID==collegeId).Select(E=>E.LabName).FirstOrDefault(),
                                                            //EquipmentName1 =  CollegeLaboratories.Where(E => E.EquipmentID == l.id && E.CollegeID == collegeId).Select(E => E.EquipmentName).FirstOrDefault(),

                                                            LabName = !string.IsNullOrEmpty(l.LabName) ? l.LabName : db.jntuh_appeal_college_laboratories.Where(E => E.EquipmentID == l.id && E.CollegeID == collegeId).Select(E => E.LabName).FirstOrDefault(),
                                                            EquipmentName = !string.IsNullOrEmpty(l.EquipmentName) ? l.EquipmentName : db.jntuh_appeal_college_laboratories.Where(E => E.EquipmentID == l.id && E.CollegeID == collegeId).Select(E => E.EquipmentName).FirstOrDefault(),
                                                            // EquipmentName = l.EquipmentName,
                                                            LabCode = l.Labcode,
                                                        })
                                                        .OrderBy(l => l.LabName)
                                                        .ThenBy(l => l.EquipmentName)
                                                        .ToList();

                    //collegeLabAnonymousLabclass = db.jntuh_lab_master.AsNoTracking()
                    //                                     .Where(l => equipids.Contains(l.id))
                    //                                     .Select(l => new AnonymousLabclass
                    //                                     {
                    //                                         id = db.jntuh_appeal_college_laboratories.Where(l1 => l1.EquipmentID == l.id && l1.EquipmentNo == 1 && l1.CollegeID == collegeId).Select(l1 => l1.id).FirstOrDefault(),
                    //                                         EquipmentID = l.id,


                    //                                        // LabName1 =  CollegeLaboratories.Where(E=>E.EquipmentID==l.id && E.CollegeID==collegeId).Select(E=>E.LabName).FirstOrDefault(),
                    //                                         //EquipmentName1 =  CollegeLaboratories.Where(E => E.EquipmentID == l.id && E.CollegeID == collegeId).Select(E => E.EquipmentName).FirstOrDefault(),

                    //                                         LabName = !string.IsNullOrEmpty(l.LabName) ? l.LabName : CollegeLaboratories.Select(E=>E.LabName).FirstOrDefault(),
                    //                                         EquipmentName = !string.IsNullOrEmpty(l.EquipmentName) ?l.EquipmentName:"BB",
                    //                                        // EquipmentName = l.EquipmentName,
                    //                                         LabCode = l.Labcode,
                    //                                     })
                    //                                     .OrderBy(l => l.LabName)
                    //                                     .ThenBy(l => l.EquipmentName)
                    //                                     .ToList();

                }
                else if (CollegeAffiliationStatus == "No" || CollegeAffiliationStatus == null)
                {

                    collegeLabAnonymousLabclass = db.jntuh_lab_master.AsNoTracking()
                                                       .Where(l => equipids.Contains(l.id) && l.CollegeId==null)
                                                       .Select(l => new AnonymousLabclass
                                                       {
                                                           id = db.jntuh_appeal_college_laboratories.Where(l1 => l1.EquipmentID == l.id && l1.EquipmentNo == 1 && l1.CollegeID == collegeId).Select(l1 => l1.id).FirstOrDefault(),
                                                           EquipmentID = l.id,
                                                           IsActive = (bool)db.jntuh_appeal_college_laboratories.Where(l1 => l1.EquipmentID == l.id && l1.EquipmentNo == 1 && l1.CollegeID == collegeId).Select(l1 => l1.isActive).FirstOrDefault(),
                                                           LabName = l.LabName,
                                                           EquipmentName = l.EquipmentName,
                                                           LabCode = l.Labcode,
                                                       })
                                                       .OrderBy(l => l.LabName)
                                                       .ThenBy(l => l.EquipmentName)
                                                       .ToList();
                }


                collegeLabAnonymousLabclass = collegeLabAnonymousLabclass.OrderBy(c => c.LabName).ThenBy(c => c.EquipmentName).Distinct().ToList();
            }
            return View(collegeLabAnonymousLabclass);
        }

        [Authorize(Roles = "Admin,SuperAdmin,FacultyVerification")]
        public ActionResult AddingLabs(int appeallabId,int collegeId)
        {
             var userId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            if (appeallabId != 0 && collegeId != 0)
            {
                var appeallabdata =db.jntuh_appeal_college_laboratories.Where(e => e.CollegeID == collegeId && e.id == appeallabId).Select(e => e).FirstOrDefault();
                if (appeallabdata != null)
                {
                    var collegeLabsdata =db.jntuh_college_laboratories.Where(e => e.CollegeID == appeallabdata.CollegeID && e.EquipmentID == appeallabdata.EquipmentID).Select(e => e).FirstOrDefault();
                    if (collegeLabsdata==null)
                    {
                        jntuh_college_laboratories labs = new jntuh_college_laboratories();
                        labs.CollegeID = appeallabdata.CollegeID;
                        labs.EquipmentID = appeallabdata.EquipmentID;
                        labs.EquipmentName = appeallabdata.EquipmentName;
                        labs.EquipmentNo = appeallabdata.EquipmentNo;
                        labs.Make = appeallabdata.Make;
                        labs.Model = appeallabdata.Model;
                        labs.EquipmentUniqueID = appeallabdata.EquipmentUniqueID;
                        labs.AvailableUnits = appeallabdata.AvailableUnits;
                        labs.AvailableArea = appeallabdata.AvailableArea;
                        labs.RoomNumber = appeallabdata.RoomNumber;
                        labs.isActive = true;
                        labs.createdOn=DateTime.Now;
                        labs.createdBy = userId;
                        labs.EquipmentDateOfPurchasing = appeallabdata.EquipmentDateOfPurchasing;
                        labs.EquipmentPhoto = appeallabdata.EquipmentPhoto;
                        labs.DelivaryChalanaDate = appeallabdata.DelivaryChalanaDate;
                        labs.LabName = appeallabdata.LabName;
                        db.jntuh_college_laboratories.Add(labs);
                        db.SaveChanges();
                        TempData["Success"] = appeallabdata.EquipmentName+" Data Inserted Successfully.";
                        appeallabdata.isActive = false;
                        db.Entry(appeallabdata).State=EntityState.Modified;
                        db.SaveChanges();

                    }
                    else
                    {
                        if (collegeLabsdata.isActive == false)
                        {
                            collegeLabsdata.isActive = true;
                            db.Entry(collegeLabsdata).State=EntityState.Modified;
                            db.SaveChanges();
                            
                            appeallabdata.isActive = false;
                            db.Entry(appeallabdata).State = EntityState.Modified;
                            db.SaveChanges();
                            TempData["Success"] = appeallabdata.EquipmentName + " Data Updated Successfully.";
                        }
                        else
                        {
                            TempData["Error"] = appeallabdata.EquipmentName + " Data Insertation Failed.";
                        }
                    }
                }
                else
                {
                    TempData["Error"] = "Data not found...";
                }
                return RedirectToAction("ViewAppealLabsDetails", new { collegeId = collegeId });
            }
            else
            {
                return RedirectToAction("ViewAppealLabsDetails");
            }
        }

        [Authorize(Roles = "Admin,SuperAdmin,FacultyVerification")]
        public ActionResult FacultyAddingToCollege(string fid, int? collegeid)
        {
            var userId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            var fId = 0;
            var appealid = 0;
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int prAy =
                db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
            if (!string.IsNullOrEmpty(fid))
            {
                fId = Convert.ToInt32(Utilities.DecryptString(fid, WebConfigurationManager.AppSettings["CryptoKey"]));
            }
            var facultyregistered = db.jntuh_registered_faculty.Find(fId);
            var jntuhAppealFacultyRegistered = db.jntuh_appeal_faculty_registered.Where(r => r.RegistrationNumber == facultyregistered.RegistrationNumber.Trim()&&r.academicYearId==prAy).Select(s=>s).FirstOrDefault();
            if (jntuhAppealFacultyRegistered != null)
            {
                appealid = jntuhAppealFacultyRegistered.id;
            }
            var facultydetails = db.jntuh_appeal_faculty_registered.Find(appealid);
            var collegefacultyreg = db.jntuh_college_faculty_registered.AsNoTracking().Where(i => i.RegistrationNumber.Trim() == facultyregistered.RegistrationNumber.Trim()).ToList();
            var collegeFaculty = new jntuh_college_faculty_registered();
            if (facultydetails != null && collegefacultyreg.Count == 0)
            {
                collegeFaculty.collegeId = facultydetails.collegeId;
                collegeFaculty.RegistrationNumber = facultydetails.RegistrationNumber;
                collegeFaculty.IdentifiedFor = facultydetails.IdentifiedFor;
                collegeFaculty.SpecializationId = facultydetails.SpecializationId;
                collegeFaculty.DepartmentId = facultydetails.DepartmentId;
                collegeFaculty.existingFacultyId = facultydetails.existingFacultyId;
                collegeFaculty.AadhaarNumber = facultydetails.AadhaarNumber == null ? facultydetails.AadhaarNumber : facultydetails.AadhaarNumber.Trim();
                if (facultydetails.AadhaarDocument!=null)
                {
                    string[] values = facultydetails.AadhaarDocument.Split('/');
                    int i = values.Count() - 1;
                    collegeFaculty.AadhaarDocument = values[i].ToString();
                    
                    //Complaince Faculty Aadhar Document Copying to College Faculty folder
                    string NewDir = "D:/JNTUH/Prod/Content/Upload/Faculty/AADHAARCARDS/CollegeFacultyAadhaar";
                    if (System.IO.File.Exists("D:/JNTUH/Prod/Content/Upload/OnlineAppealDocuments/Faculty/FacultyAadhaarDocuments/"+ collegeFaculty.AadhaarDocument))
                    {
                        var appealfolder = Path.Combine("D:/JNTUH/Prod/Content/Upload/OnlineAppealDocuments/Faculty/FacultyAadhaarDocuments/",collegeFaculty.AadhaarDocument);
                        if (!System.IO.File.Exists("D:/JNTUH/Prod/Content/Upload/Faculty/AADHAARCARDS/CollegeFacultyAadhaar/" + collegeFaculty.AadhaarDocument))
                        {
                            System.IO.File.Copy(appealfolder, Path.Combine(NewDir,collegeFaculty.AadhaarDocument));
                        }
                    }
                }
                if (facultydetails.AppealReverificationScreenshot != null)
                {
                    string[] values = facultydetails.AppealReverificationScreenshot.Split('/');
                    int i = values.Count() - 1;
                    var allcertificates = values[i].ToString();

                    var facultyothercertifictes =
                        db.jntuh_registered_faculty_education.Where(
                            r => r.facultyId == facultyregistered.id && r.educationId == 8)
                            .Select(s => s)
                            .FirstOrDefault();
                    if (facultyothercertifictes == null)
                    {
                        string NewDir = "D:/JNTUH/Prod/Content/Upload/Faculty/Certificates/CertificatesPDF";
                        if (System.IO.File.Exists("D:/JNTUH/Prod/Content/Upload/OnlineAppealDocuments/Faculty/AppealReverificationScreenshot/" + allcertificates))
                        {
                            var appealfolder = Path.Combine("D:/JNTUH/Prod/Content/Upload/OnlineAppealDocuments/Faculty/AppealReverificationScreenshot/", allcertificates);
                            if (!System.IO.File.Exists("D:/JNTUH/Prod/Content/Upload/Faculty/Certificates/CertificatesPDF" + allcertificates))
                            {
                                System.IO.File.Copy(appealfolder, Path.Combine(NewDir, allcertificates));
                            }
                        }
                        jntuh_registered_faculty_education othereducation = new jntuh_registered_faculty_education();
                        othereducation.hallticketnumber = "0";
                        othereducation.facultyId = facultyregistered.id;
                        othereducation.educationId = 8;
                        othereducation.courseStudied = "Others";
                        othereducation.specialization = "Others";
                        othereducation.passedYear = 0;
                        othereducation.marksPercentage = 0;
                        othereducation.division = 0;
                        othereducation.boardOrUniversity = "Others";
                        othereducation.placeOfEducation = "Others";
                        othereducation.certificate = allcertificates;
                        othereducation.isActive = false;
                        othereducation.createdBy = userId;
                        othereducation.createdOn = DateTime.Now;
                        othereducation.updatedBy = null;
                        othereducation.updatedOn = null;
                        db.jntuh_registered_faculty_education.Add(othereducation);
                        db.SaveChanges();
                    }
                }
                collegeFaculty.FacultySpecializationId = facultydetails.FacultySpecializationId;
                collegeFaculty.createdBy = userId;
                collegeFaculty.createdOn = DateTime.Now;
                db.jntuh_college_faculty_registered.Add(collegeFaculty);
                db.SaveChanges();
                TempData["Success"] = collegeFaculty.RegistrationNumber + " Registration Number Is Successfully Added to College Faculty..";
                facultydetails.isActive = true;
                db.SaveChanges();
                if (facultyregistered!=null)
                {
                    facultyregistered.Absent = false;
                    facultyregistered.Xeroxcopyofcertificates = false;
                    facultyregistered.NoRelevantUG = "No";
                    facultyregistered.NoRelevantPG = "No";
                    facultyregistered.NORelevantPHD = "No";
                    facultyregistered.NotQualifiedAsperAICTE = false;
                    facultyregistered.InvalidPANNumber = false;
                    facultyregistered.IncompleteCertificates = false;
                    facultyregistered.NoSCM = false;
                    facultyregistered.OriginalCertificatesNotShown = false;
                    facultyregistered.NotIdentityfiedForanyProgram = false;
                    facultyregistered.Noclass = false;
                    facultyregistered.NotconsideredPHD = false;
                    facultyregistered.Genuinenessnotsubmitted = false;
                    facultyregistered.BAS = "No";
                    facultyregistered.InvalidAadhaar = "No";
                    facultyregistered.OriginalsVerifiedUG = false;
                    facultyregistered.OriginalsVerifiedPHD = false;
                    facultyregistered.AbsentforVerification = false;
                    facultyregistered.Invaliddegree = false;
                    facultyregistered.NoPGspecialization = false;
                    //facultyregistered.Blacklistfaculy = false;

                    //facultyregistered.FakePHD = false;
                    db.Entry(facultyregistered).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }
            else if (facultydetails != null && collegefacultyreg.Count > 0)
            {
                TempData["Success"] = "Registration Number Is already Added to College Faculty..";
                //var id = 0;
                //var createdby = 0;
                //DateTime? createdon = null;
                //var jntuhCollegeFacultyRegistered = collegefacultyreg.FirstOrDefault();
                //if (jntuhCollegeFacultyRegistered != null)
                //{
                //    id = jntuhCollegeFacultyRegistered.id;
                //    if (jntuhCollegeFacultyRegistered.createdBy != null)
                //        createdby = (int)jntuhCollegeFacultyRegistered.createdBy;
                //    createdon = jntuhCollegeFacultyRegistered.createdOn;
                //}
                //collegeFaculty.id = id;
                //collegeFaculty.collegeId = facultydetails.collegeId;
                //collegeFaculty.RegistrationNumber = facultydetails.RegistrationNumber;
                //collegeFaculty.IdentifiedFor = facultydetails.IdentifiedFor;
                //collegeFaculty.DepartmentId = facultydetails.DepartmentId;
                //collegeFaculty.existingFacultyId = facultydetails.existingFacultyId;
                //collegeFaculty.SpecializationId = facultydetails.SpecializationId;
                //collegeFaculty.AadhaarNumber = facultydetails.AadhaarNumber;
                //if (facultydetails.AadhaarDocument != null)
                //{
                //    string[] values = facultydetails.AadhaarDocument.Split('/');
                //    int i = values.Count() - 1;

                //    collegeFaculty.AadhaarDocument = values[i].ToString();
                //}
               
                //collegeFaculty.FacultySpecializationId = facultydetails.FacultySpecializationId;
                //collegeFaculty.createdBy = createdby;
                //collegeFaculty.createdOn = createdon;
                //collegeFaculty.updatedBy = userId;
                //collegeFaculty.updatedOn = DateTime.Now;
                //db.Entry(collegeFaculty).State = EntityState.Modified;
                //db.SaveChanges();
                //TempData["Success"] = "Registration Number Is Successfully Updated to College Faculty..";
                //facultydetails.isActive = true;
                //db.SaveChanges();
                //if (facultyregistered != null)
                //{
                //    facultyregistered.Absent = false;
                //    facultyregistered.Xeroxcopyofcertificates = false;
                //    facultyregistered.NoRelevantUG = "No";
                //    facultyregistered.NoRelevantPG = "No";
                //    facultyregistered.NORelevantPHD = "No";
                //    facultyregistered.NotQualifiedAsperAICTE = false;
                //    facultyregistered.InvalidPANNumber = false;
                //    facultyregistered.IncompleteCertificates = false;
                //    facultyregistered.NoSCM = false;
                //    facultyregistered.OriginalCertificatesNotShown = false;
                //    //facultyregistered.PANNumber = ;
                //    facultyregistered.NotIdentityfiedForanyProgram = false;
                //    facultyregistered.Invaliddegree = false;
                //    facultyregistered.BAS = "No";
                //    facultyregistered.InvalidAadhaar = "No";
                //    facultyregistered.OriginalsVerifiedUG = false;
                //    facultyregistered.OriginalsVerifiedPHD = false;
                //    db.Entry(facultyregistered).State = EntityState.Modified;
                //    db.SaveChanges();
                //}
            }
            else if (facultydetails == null)
            {
                TempData["Error"] = "Registration Number Was not found";
            }
            return RedirectToAction("ViewFacultyDetails", new { collegeid = collegeid });
        }

        [Authorize(Roles = "Admin,SuperAdmin,FacultyVerification")]
        public ActionResult FacultyReactivateAllFlags(string fid, int? collegeid)
        {
            var fId = 0;
            var id = 0;
            var userId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            if (!string.IsNullOrEmpty(fid))
            {
                fId = Convert.ToInt32(Utilities.DecryptString(fid, WebConfigurationManager.AppSettings["CryptoKey"]));
            }
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int prAy =
                db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
            var facultyregistered = db.jntuh_registered_faculty.Find(fId);
            var jntuhAppealFacultyRegistered = db.jntuh_appeal_faculty_registered.Where(a => a.RegistrationNumber == facultyregistered.RegistrationNumber.Trim() && a.academicYearId == prAy).Select(s=>s).FirstOrDefault();
            if (jntuhAppealFacultyRegistered != null)
            {
                id = jntuhAppealFacultyRegistered.id;
            }
            var facultydetails = db.jntuh_appeal_faculty_registered.Find(id);
            if (jntuhAppealFacultyRegistered ==null|| facultyregistered == null || facultydetails == null)
            {
                TempData["Error"] = "RegistrationNumber Not Found.";
                return RedirectToAction("ViewFacultyDetails", new {collegeid = collegeid});
            }

            //Adding Appeal Department Details to College Faculty
            if (jntuhAppealFacultyRegistered.IdentifiedFor != null && jntuhAppealFacultyRegistered.DepartmentId!=null)
            {
                var collegefacultyregistred =
                           db.jntuh_college_faculty_registered.Where(
                               r => r.RegistrationNumber == facultyregistered.RegistrationNumber.Trim())
                               .Select(s => s)
                               .FirstOrDefault();
                if (collegefacultyregistred.IdentifiedFor != jntuhAppealFacultyRegistered.IdentifiedFor.Trim())
                {
                    collegefacultyregistred.IdentifiedFor = jntuhAppealFacultyRegistered.IdentifiedFor.Trim();
                    collegefacultyregistred.updatedBy = userId;
                    collegefacultyregistred.updatedOn = DateTime.Now;
                }
                if (collegefacultyregistred.DepartmentId != jntuhAppealFacultyRegistered.DepartmentId)
                {
                    collegefacultyregistred.DepartmentId = jntuhAppealFacultyRegistered.DepartmentId;
                    collegefacultyregistred.updatedBy = userId;
                    collegefacultyregistred.updatedOn = DateTime.Now;
                }
                if (collegefacultyregistred.SpecializationId!=null)
                {
                    if (collegefacultyregistred.SpecializationId != jntuhAppealFacultyRegistered.SpecializationId)
                    {
                        collegefacultyregistred.SpecializationId = jntuhAppealFacultyRegistered.SpecializationId;
                        collegefacultyregistred.updatedBy = userId;
                        collegefacultyregistred.updatedOn = DateTime.Now;
                    }                  
                }
                if (facultyregistered.BAS == "Yes")
                {
                    if (facultydetails.AadhaarDocument != null)
                    {
                       
                        if (collegefacultyregistred != null)
                        {
                            string[] values = facultydetails.AadhaarDocument.Split('/');
                            int i = values.Count() - 1;
                            collegefacultyregistred.AadhaarNumber = facultydetails.AadhaarNumber != null
                                ? facultydetails.AadhaarNumber.Trim()
                                : facultydetails.AadhaarNumber;
                            collegefacultyregistred.AadhaarDocument = values[i].ToString();
                            //Complaince Faculty Aadhar Document Copying to College Faculty folder
                            string NewDir = "D:/JNTUH/Prod/Content/Upload/Faculty/AADHAARCARDS/CollegeFacultyAadhaar";
                            if (System.IO.File.Exists("D:/JNTUH/Prod/Content/Upload/OnlineAppealDocuments/Faculty/FacultyAadhaarDocuments/" + collegefacultyregistred.AadhaarDocument))
                            {
                                var appealfolder = Path.Combine("D:/JNTUH/Prod/Content/Upload/OnlineAppealDocuments/Faculty/FacultyAadhaarDocuments/", collegefacultyregistred.AadhaarDocument);
                                if (!System.IO.File.Exists("D:/JNTUH/Prod/Content/Upload/Faculty/AADHAARCARDS/CollegeFacultyAadhaar/" + collegefacultyregistred.AadhaarDocument))
                                {
                                    System.IO.File.Copy(appealfolder, Path.Combine(NewDir, collegefacultyregistred.AadhaarDocument));
                                }
                            }
                            collegefacultyregistred.updatedBy = userId;
                            collegefacultyregistred.updatedOn = DateTime.Now;
                        }
                    }
                }
                db.Entry(collegefacultyregistred).State = EntityState.Modified;
                db.SaveChanges();
            }


            #region Appeal Reverification Faculty if Faculty have Aadhaar Flag moveing Aadhar number & Aadhaar Document move to college faculty table;as like same all supporting document move to Faculty Education Details education Id=8
          

            if (facultyregistered.Xeroxcopyofcertificates == true || facultyregistered.IncompleteCertificates == true)
            {
                if (facultydetails.AppealReverificationSupportingDocument != null)
                {
                    string[] values = facultydetails.AppealReverificationSupportingDocument.Split('/');
                    int i = values.Count() - 1;
                    var allcertificates = values[i].ToString();
                    //string NewDir = "D:/JNTUH/Prod/Content/Upload/Faculty/Certificates";
                    //if (System.IO.File.Exists("D:/JNTUH/Prod/Content/Upload/OnlineAppealDocuments/Faculty/AppealReverificationSupportReports/" + allcertificates))
                    //{
                    //    var appealfolder = Path.Combine("D:/JNTUH/Prod/Content/Upload/OnlineAppealDocuments/Faculty/AppealReverificationSupportReports/", allcertificates);
                    //    if (!System.IO.File.Exists("D:/JNTUH/Prod/Content/Upload/Faculty/Certificates/" + allcertificates))
                    //    {
                    //        System.IO.File.Copy(appealfolder, Path.Combine(NewDir, allcertificates));
                    //    }
                    //}

                    string NewDir = "D:/JNTUH/Prod/Content/Upload/Faculty/Certificates/CertificatesPDF";
                    if (System.IO.File.Exists("D:/JNTUH/Prod/Content/Upload/OnlineAppealDocuments/Faculty/AppealReverificationSupportReports/" + allcertificates))
                    {
                        var appealfolder = Path.Combine("D:/JNTUH/Prod/Content/Upload/OnlineAppealDocuments/Faculty/AppealReverificationSupportReports/", allcertificates);
                        if (!System.IO.File.Exists("D:/JNTUH/Prod/Content/Upload/Faculty/Certificates/CertificatesPDF/" + allcertificates))
                        {
                            System.IO.File.Copy(appealfolder, Path.Combine(NewDir, allcertificates));
                        }
                    }

                    var facultyothercertifictes =
                        db.jntuh_registered_faculty_education.Where(
                            r => r.facultyId == facultyregistered.id && r.educationId == 8)
                            .Select(s => s)
                            .FirstOrDefault();
                    if (facultyothercertifictes == null)
                    {
                        jntuh_registered_faculty_education othereducation = new jntuh_registered_faculty_education();
                        othereducation.hallticketnumber = "0";
                        othereducation.facultyId = facultyregistered.id;
                        othereducation.educationId = 8;
                        othereducation.courseStudied = "Others";
                        othereducation.specialization = "Others";
                        othereducation.passedYear = 0;
                        othereducation.marksPercentage = 0;
                        othereducation.division = 0;
                        othereducation.boardOrUniversity = "Others";
                        othereducation.placeOfEducation = "Others";
                        othereducation.certificate = allcertificates;
                        othereducation.isActive = false;
                        othereducation.createdBy = userId;
                        othereducation.createdOn = DateTime.Now;
                        othereducation.updatedBy = null;
                        othereducation.updatedOn = null;
                        db.jntuh_registered_faculty_education.Add(othereducation);
                        db.SaveChanges();
                    }
                }
            }
            #endregion
            facultyregistered.Absent = false;
            facultyregistered.Xeroxcopyofcertificates = false;
            facultyregistered.NoRelevantUG = "No";
            facultyregistered.NoRelevantPG = "No";
            facultyregistered.NORelevantPHD = "No";
            facultyregistered.NotQualifiedAsperAICTE = false;
            facultyregistered.InvalidPANNumber = false;
            facultyregistered.IncompleteCertificates = false;
            facultyregistered.NoSCM = false;
            facultyregistered.OriginalCertificatesNotShown = false;
            facultyregistered.NotIdentityfiedForanyProgram = false;
            facultyregistered.Invaliddegree = false;
            facultyregistered.BAS = "No";
            facultyregistered.InvalidAadhaar = "No";
            facultyregistered.OriginalsVerifiedUG =false;
            facultyregistered.OriginalsVerifiedPHD = false;
            facultyregistered.Invaliddegree = false;
            facultyregistered.NotconsideredPHD = false;
            facultyregistered.Genuinenessnotsubmitted = false;           
            facultyregistered.NoSpecialization = false;
            facultyregistered.Noclass = false;

            //facultyregistered.AbsentforVerification = false;
            //facultyregistered.Blacklistfaculy = false;

            //facultyregistered.FakePHD = false;
            facultyregistered.updatedBy = userId;
            facultyregistered.updatedOn = DateTime.Now;
            db.Entry(facultyregistered).State = EntityState.Modified;
            db.SaveChanges();
            TempData["Success"] = "Faculty Registration Number ( " + facultyregistered.RegistrationNumber + " ) Successfully Re-activated..";

            if (facultydetails!=null)
            {
                facultydetails.isActive = true;
                db.SaveChanges();
            }
            
            return RedirectToAction("ViewFacultyDetails", new { collegeid = collegeid }); 
        }


        [Authorize(Roles = "Admin,SuperAdmin,FacultyVerification")]
        public ActionResult RegPrincipalAdding(string fid, int? collegeid)
        {
            var userId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            var fId = 0;
            var appealid = 0;
            if (!string.IsNullOrEmpty(fid))
            {
                fId = Convert.ToInt32(Utilities.DecryptString(fid, WebConfigurationManager.AppSettings["CryptoKey"]));
            }
           //var facultyregistered = db.jntuh_registered_faculty.Find(fId);
            var jntuhAppealFacultyRegistered = db.jntuh_appeal_principal_registered.AsNoTracking().FirstOrDefault(i => i.existingFacultyId == fId);
            if (jntuhAppealFacultyRegistered != null)
            {
                appealid = jntuhAppealFacultyRegistered.id;
            }
            var principaldetails = db.jntuh_appeal_principal_registered.Find(appealid);
            var collegeprincipalreg = db.jntuh_college_principal_registered.AsNoTracking().Where(i => i.collegeId == jntuhAppealFacultyRegistered.collegeId).ToList();
            var principalFaculty = new jntuh_college_principal_registered();
            if (principaldetails != null && collegeprincipalreg.Count == 0)
            {
                principalFaculty.collegeId = principaldetails.collegeId;
                principalFaculty.RegistrationNumber = principaldetails.RegistrationNumber;
                principalFaculty.AadhaarNumber = principaldetails.AadhaarNumber;
                if (principalFaculty.AadhaarDocument != null)
                {
                    string[] values = principalFaculty.AadhaarDocument.Split('/');
                    int i = values.Count() - 1;

                    principalFaculty.AadhaarDocument = values[i].ToString();
                }
                //principalFaculty.AadhaarDocument = principaldetails.AadhaarDocument;
                principalFaculty.isActive = true;
                principalFaculty.createdBy = userId;
                principalFaculty.createdOn = DateTime.Now;
                db.jntuh_college_principal_registered.Add(principalFaculty);
                db.SaveChanges();
                TempData["Success"] = "Registration Number Is Successfully Added to Principal Registered..";
                principaldetails.isActive = true;
                db.SaveChanges();
            }
            else if (principaldetails != null && collegeprincipalreg.Count > 0)
            {
                var id = 0;
                var createdby = 0;
                DateTime? createdon = null;
                var jntuhprincipalFacultyRegistered = collegeprincipalreg.FirstOrDefault();
                if (jntuhprincipalFacultyRegistered != null)
                {
                    id = jntuhprincipalFacultyRegistered.id;
                    if (jntuhprincipalFacultyRegistered.createdBy != null)
                        createdby = (int)jntuhprincipalFacultyRegistered.createdBy;
                    createdon = jntuhprincipalFacultyRegistered.createdOn;
                }
                principalFaculty.id = id;
                principalFaculty.collegeId = principaldetails.collegeId;
                principalFaculty.RegistrationNumber = principaldetails.RegistrationNumber;
                principalFaculty.AadhaarNumber = principaldetails.AadhaarNumber;
                if (principalFaculty.AadhaarDocument != null)
                {
                    string[] values = principalFaculty.AadhaarDocument.Split('/');
                    int i = values.Count() - 1;

                    principalFaculty.AadhaarDocument = values[i].ToString();
                }
                //principalFaculty.AadhaarDocument = principaldetails.AadhaarDocument;
                principalFaculty.isActive = true;
                principalFaculty.createdBy = createdby;
                principalFaculty.createdOn = createdon;
                principalFaculty.updatedBy = userId;
                principalFaculty.updatedOn = DateTime.Now;
                db.Entry(principalFaculty).State = EntityState.Modified;
                db.SaveChanges();
                TempData["Success"] = "Registration Number Is Successfully Updated to Principal Registered..";
                principaldetails.isActive = true;
                db.SaveChanges();
            }
            else if (principaldetails == null)
            {
                TempData["Error"] = "Registration Number Was not found";
            }
            return RedirectToAction("ViewFacultyDetails", new { collegeid = collegeid });
        }


        [Authorize(Roles = "Admin,SuperAdmin,FacultyVerification")]
        public ActionResult PrincipalReactivateAllFlags(string fid, int? collegeid)
        {
            var fId = 0;
            var id = 0;
            var userId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            if (!string.IsNullOrEmpty(fid))
            {
                fId = Convert.ToInt32(Utilities.DecryptString(fid, WebConfigurationManager.AppSettings["CryptoKey"]));
            }

            var facultyregistered = db.jntuh_registered_faculty.Find(fId);
            var jntuhAppealPrincipalRegistered = db.jntuh_appeal_principal_registered.AsNoTracking().FirstOrDefault(i => i.existingFacultyId == fId);
            if (jntuhAppealPrincipalRegistered != null)
            {
                id = jntuhAppealPrincipalRegistered.id;
            }
            var principaldetails = db.jntuh_appeal_principal_registered.Find(id);
            if (facultyregistered == null)
            {
                TempData["Error"] = "RegistrationNumber Not Found.";
                return RedirectToAction("ViewFacultyDetails", new { collegeid = collegeid });
            }

            //facultyregistered.Absent = false;
            //facultyregistered.InvalidPANNumber = false;
            //facultyregistered.NoSCM = false;
            //facultyregistered.NoForm16 = false;
            //facultyregistered.NotQualifiedAsperAICTE = false;
            //facultyregistered.MultipleRegInSameCollege = false;
            //facultyregistered.MultipleRegInDiffCollege = false;
            //facultyregistered.SamePANUsedByMultipleFaculty = false;
            //facultyregistered.PhotoCopyofPAN = false;
            //facultyregistered.AppliedPAN = false;
            //facultyregistered.LostPAN = false;
            //facultyregistered.OriginalsVerifiedUG = false;
            //facultyregistered.OriginalsVerifiedPG = false;
            //facultyregistered.OriginalsVerifiedPHD = false;
            //facultyregistered.FacultyVerificationStatus = false;
            //facultyregistered.IncompleteCertificates = false;
            //facultyregistered.FalsePAN = false;
            //facultyregistered.PHDundertakingnotsubmitted = false;
            //facultyregistered.Blacklistfaculy = false;
            facultyregistered.DeactivationReason =null;
            facultyregistered.isApproved = true;
            facultyregistered.updatedBy = userId;
            facultyregistered.updatedOn = DateTime.Now;
            db.SaveChanges();
            TempData["Success"] = "Faculty Registration Number ( " + facultyregistered.RegistrationNumber + " ) Successfully Re-activated..";

            if (principaldetails != null)
            {
                principaldetails.isActive = true;
                db.SaveChanges();
            }

            return RedirectToAction("ViewFacultyDetails", new { collegeid = collegeid });
        }

        #endregion

        #region View Compliance Faculty Details
        [Authorize(Roles = "Admin,SuperAdmin,FacultyVerification,College")]
        public ActionResult ViewComplianceFacultyDetails(string id, string strType)
        {

            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int collegeId = db.jntuh_college_users.Where(collegeUser => collegeUser.userID == userID).Select(collegeUser => collegeUser.collegeID).FirstOrDefault();

            if (collegeId == 0)
            {
                collegeId = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(id, WebConfigurationManager.AppSettings["CryptoKey"]));
            }

            var colgids = db.jntuh_appeal_college_edit_status.Where(i => i.IsCollegeEditable == false).GroupBy(e => e.collegeId).Select(e => e.Key).Distinct().ToArray();
            ViewBag.Colleges = db.jntuh_college.Where(e => e.isActive && colgids.Contains(e.id)).Select(e => new { collegeId = e.id, collegeName = e.collegeCode + "_" + e.collegeName }).OrderBy(e => e.collegeName).ToList();
            var appealfacultyDetails = db.jntuh_appeal_faculty_registered.AsNoTracking().ToList();
            var principalDetails = db.jntuh_appeal_principal_registered.AsNoTracking().ToList();
            var jntuh_registered_faculty = db.jntuh_registered_faculty.AsNoTracking().ToList();
            var departments = db.jntuh_department.AsNoTracking().ToList();
            //var jntuh_college = db.jntuh_college.AsNoTracking().ToList();
            var facultyDetails = new List<CollegeFaculty>();
            if (collegeId != null)
            {
                #region Principal
                var Reason1 = string.Empty;
                var NewReason1 = string.Empty;
                //if (principalDetails.Count > 0)
                //{
                //    var item = principalDetails.FirstOrDefault(i => i.collegeId == collegeId && i.isActive == false);

                //    if (item != null)
                //    {
                //        var data = jntuh_registered_faculty.Where(e => e.RegistrationNumber.Trim() == item.RegistrationNumber.Trim()).Select(e => e).FirstOrDefault();

                //        ViewBag.RegistrationNumber = item.RegistrationNumber;
                //        ViewBag.collegeId = item.collegeId;
                //        if (data != null)
                //        {
                //            ViewBag.Status = true;
                //            ViewBag.id = data.id;
                //            ViewBag.FullName = data.FirstName + " " + data.LastName + " " + data.MiddleName;
                //            ViewBag.ViewNotificationDocument = item.NOtificationReport;
                //            ViewBag.ViewAppointmentOrderDocument = item.AppointMentOrder;
                //            ViewBag.ViewJoiningReportDocument = item.JoiningOrder;
                //            ViewBag.ViewSelectionCommitteeDocument = item.SelectionCommiteMinutes;
                //            ViewBag.ViewPhdUndertakingDocument = item.PHDUndertakingDocument;
                //            ViewBag.ViewPhysicalPresenceDocument = item.PhysicalPresenceonInspection;
                //            ViewBag.isActive = item.isActive;
                //            if (data.Absent == true)
                //            {
                //                Reason1 = "NOT AVAILABLE" + ",";
                //            }
                //            if (data.NotQualifiedAsperAICTE == true)
                //            {
                //                Reason1 += "NOT QUALIFIED " + ",";
                //            }
                //            if (data.InvalidPANNumber == true)
                //            {
                //                Reason1 += "NO PAN" + ",";
                //            }
                //            if (data.FalsePAN == true)
                //            {
                //                Reason1 += "FALSE PAN" + ",";
                //            }
                //            if (data.NoSCM == true)
                //            {
                //                Reason1 += "NO SCM/RATIFICATION" + ",";
                //            }
                //            if (data.IncompleteCertificates == true)
                //            {
                //                Reason1 += "Incomplete Certificates" + ",";
                //            }
                //            if (data.PHDundertakingnotsubmitted == true)
                //            {
                //                Reason1 += "No Undertaking" + ",";
                //            }
                //            if (data.Blacklistfaculy == true)
                //            {
                //                Reason1 += "Blacklisted" + ",";
                //            }

                //            if (data.NoPhdUndertakingNew == true)
                //            {
                //                Reason1 += "No Undertaking" + ",";
                //            }
                //            if (data.Blacklistfaculy == true)
                //            {
                //                NewReason1 += "Blacklisted" + ",";
                //            }
                //        }
                //        if (Reason1 != "")
                //        {
                //            Reason1 = Reason1.Substring(0, Reason1.Length - 1);
                //            ViewBag.Reason = Reason1;
                //        }
                //        if (NewReason1 != "")
                //        {
                //            NewReason1 = NewReason1.Substring(0, NewReason1.Length - 1);
                //            ViewBag.NewReason = NewReason1;
                //        }
                //    }
                //}
                #endregion


                #region For Faculty
                appealfacultyDetails = appealfacultyDetails.Where(e => e.collegeId == collegeId && e.isActive == false).ToList();
                foreach (var item in appealfacultyDetails)
                {
                    var Reason = string.Empty;
                    var newreason = string.Empty;
                    var data = jntuh_registered_faculty.Where(e => e.RegistrationNumber.Trim() == item.RegistrationNumber.Trim()).Select(e => e).FirstOrDefault();
                    var degreeid = data != null && data.jntuh_registered_faculty_education.Count > 0 ? data.jntuh_registered_faculty_education.Select(e => e.educationId).Max() : 0;
                    if (data != null)
                    {
                        if (data.Absent == true)
                        {
                            Reason = "ABSENT" + ",";
                        }
                        if (data.NotQualifiedAsperAICTE == true || degreeid < 4)
                        {
                            Reason += "NOT QUALIFIED " + ",";
                        }
                        if (string.IsNullOrEmpty(data.PANNumber))
                        {
                            Reason += "NO PAN" + ",";
                        }
                        if (data.DepartmentId == null)
                        {
                            Reason1 += "No Department" + ",";
                        }
                        if (data.NoSCM == true)
                        {
                            Reason += "NO SCM/RATIFICATION" + ",";
                        }
                        if (data.PHDundertakingnotsubmitted == true)
                        {
                            Reason += "UNDERTAKING NOT CONFIRMED BY FACULTY" + ",";
                        }
                        if (data.Blacklistfaculy == true)
                        {
                            Reason += "Blacklisted" + ",";
                        }

                        if (Reason != "")
                        {
                            Reason = Reason.Substring(0, Reason.Length - 1);
                        }

                        if (data.FalsePAN == true)
                        {
                            newreason += "False Pan" + ",";
                        }
                        if (data.NoPhdUndertakingNew == true)
                        {
                            newreason += "UNDERTAKING NOT CONFIRMED BY FACULTY" + ",";
                        }
                        if (newreason != "")
                        {
                            newreason = newreason.Substring(0, newreason.Length - 1);
                        }

                        var faculty = new CollegeFaculty
                        {
                            FacultyRegistrationNumber = item.RegistrationNumber,
                            collegeId = item.collegeId,
                            id = jntuh_registered_faculty.Where(e => e.RegistrationNumber.Trim() == item.RegistrationNumber.Trim()).Select(e => e.id).FirstOrDefault(),
                            facultyFirstName = data.FirstName,
                            facultyLastName = data.LastName,
                            facultySurname = data.MiddleName,
                            department = (data.DepartmentId != null) ? departments.Where(D => D.id == data.DepartmentId).Select(D => D.departmentName).FirstOrDefault() : "",
                            ViewNotificationDocument = item.NOtificationReport,
                            ViewAppointmentOrderDocument = item.AppointMentOrder,
                            ViewJoiningReportDocument = item.JoiningOrder,
                            ViewSelectionCommitteeDocument = item.SelectionCommiteMinutes,
                            ViewAppealReverificationSupportDoc = item.AppealReverificationSupportingDocument,
                            isActive = item.isActive,
                            Reason = Reason,
                            NewReason = newreason
                        };
                        facultyDetails.Add(faculty);
                    }
                }
                #endregion

            }
            return View(facultyDetails);
        }
        #endregion

        //Compliance Faculty
        [Authorize(Roles = "Admin,SuperAdmin,FacultyVerification")]
        public ActionResult ComplianceFaculty()
        {
            if (Membership.GetUser() == null)
            {
                return RedirectToAction("LogOn", "Account");
            }
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int prAy =
                db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
            var appealcomplianceregs =new string[]{ "9077-171227-105627", "6798-170114-122804", "3166-190504-121851", "4696-180130-143541", "4116-180127-151944", "0797-170209-104131", "8873-170117-121544", "5473-180202-160353", "8164-150413-162627", "7961-160201-144754", "3272-180202-141038", "5237-170108-142109", "0961-170110-130137", "6337-170117-142333", "6650-170119-112637", "6259-180417-145526", "3104-171223-174625", "14150403-085021", "0145-150413-122917", "7807-190503-144608", "4278-150417-235845", "4501-190502-192107", "48150405-182335", "6020-190508-111529", "0271-160529-123746", "3531-150421-143805", "9652-190504-161826", "7686-190311-110057", "5080-190503-104156", "3117-190219-103153", "2728-190506-120534", "5814-190503-123911", "2741-190506-141705", "1753-170112-095603", "2212-190504-150748", "4262-160312-142634", "8287-190502-165202", "3764-190504-115105", "7060-190503-153436", "1901-190504-103040", "9756-190504-123322", "4166-190502-123718", "8398-190504-122140", "9300-150427-153637", "1727-180417-174731", "5536-190503-170752", "4997-150421-124141", "9120-190504-122446", "4107-150425-151419", "1085-190504-161805", "61150405-233540", "8893-190502-165322", "7830-190504-123008", "4357-170107-090638", "3880-190504-103759", "8196-170107-085348", "8065-190506-170516", "16150407-123531", "8224-161219-155559", "5845-190408-175258", "5783-190504-151340", "8937-190430-131849", "0920-190506-164322", "7522-150411-220514", "0245-170602-225751", "6602-150412-115959", "7954-160112-144506", "1713-190504-162941", "8253-190504-103036", "5875-190504-095924", "7529-190503-155109", "3400-190503-152955", "4633-190504-105742", "5410-190504-113713", "0805-150409-150937", "1324-170201-115719", "2467-190502-093116", "7053-160313-202120", "8624-190504-133158", "4298-170107-100304", "7367-150411-141824", "8042-190503-122314", "1518-150411-133208", "4976-190506-162421", "61150406-133939", "9728-190506-153611", "7784-150408-163005", "4036-190503-133552", "9469-190504-145044", "3107-190504-130439", "6839-190502-122519", "0491-190506-163221", "6743-190506-105618", "6449-190503-143308", "8384-180421-152151", "66150406-155949", "5004-171101-155430", "8313-171227-162029", "37150404-161401", "2289-170121-114212", "8863-150408-094224", "0615-190506-171853", "1134-160226-164507", "0925-190506-132232", "0511-190506-134109", "3376-190504-213244", "1716-190503-174708", "4938-160217-151637", "8162-190505-170107", "9484-190504-222228", "2655-160129-233519", "3371-190503-121011", "0405-190502-122235", "5163-190506-170839", "3828-190506-143012", "2494-160311-220507", "4869-190504-003918", "4280-190504-143842", "0082-190504-142144", "5225-170201-022849", "2966-190504-204125", "8854-190506-145826", "7126-170211-134536", "4529-180417-105032", "1246-190503-102516", "4387-190504-171702", "5241-190506-125957", "6910-180110-221858", "75150406-142739", "0488-190506-155558", "6037-180827-165420", "2246-150408-115418", "3056-170129-015900", "2169-190504-144756", "7762-150413-141721", "1959-190504-115112", "1093-160314-141102", "3498-151222-151036", "7818-190503-165115", "8340-151223-164445", "6432-150420-132131", "9477-190506-150254", "7931-190506-135843", "2268-170127-064021", "5622-190506-160603", "2050-190506-153824", "1200-190220-181732", "1311-190506-113902", "2480-160108-161311", "6709-180416-132131", "9171-190504-113923", "5311-190503-193530", "0733-190506-134107", "7358-170520-155849", "8501-180125-122737", "4026-160312-191809", "3364-190502-114614", "7003-180417-133427", "3053-160314-112133", "1331-170125-102230", "0437-161201-110359", "7272-190503-144523", "4034-190506-144059", "2226-180202-134854", "3085-190506-155456", "5878-150420-165638", "5053-190503-115547", "3838-190501-154520", "1503-190506-160705", "5568-190503-151217", "4084-190503-185215", "5921-190506-152822", "3214-190504-154543", "1514-190506-141053", "6570-190506-160550", "2108-160218-153132", "1618-161111-145445", "8463-160305-174420", "0664-170129-120105", "6739-150410-180605", "9116-160305-153204", "7541-160304-181219", "6555-170112-110914", "2930-190501-150844", "5319-160229-151814", "1776-190310-093323", "7056-190506-121432", "7939-180421-164719", "7902-160225-100058", "2079-190508-133755", "0760-190508-101748", "5358-190502-145633", "1059-190505-104637", "4111-190505-222941", "0242-160301-094615", "8346-190504-182217", "9125-190504-103502", "9679-190505-181347", "4551-190503-121017", "5417-190505-172641", "3234-190501-175620", "7943-150409-122441", "8574-150506-123244", "6867-190506-112140", "4669-170204-160815", "0788-170128-105006", "5469-150413-124607", "7313-170125-115717", "2292-170915-103935", "2561-190503-140205", "9412-190503-115229", "8099-190506-103449", "8748-190506-154237", "4660-190510-154812", "5054-180417-201239", "6050-150507-155309", "2551-170914-111716", "0904-190221-161321", "0704-180417-164923", "7552-190220-154710", "8515-190505-203931", "2725-190504-162522", "0890-190505-142752", "0206-160527-190725", "8534-190107-205249", "40150331-154324", "0591-170127-061904", "4749-190504-102844", "3842-170131-051131", "4557-190109-151025", "5374-180202-150025", "0138-150417-160547", "2781-160223-200239", "6134-160223-183148", "4079-190504-153600", "6858-180420-104206", "7010-190506-130230", "6467-190506-153922", "4992-150426-105757", "6801-190508-103224", "2984-160311-133903", "2129-190503-141313", "2594-190220-154318", "5171-190504-201905", "3813-150425-180214", "0438-180818-145936", "8564-190511-112035", "9128-161208-122830", "3619-190506-102919", "2203-190504-165348", "1908-190505-085737", "2277-190506-161834", "1873-190511-122049", "9279-171227-163632", "8789-150410-140528", "6364-170111-070522", "0128-150420-162813", "3370-190129-131923", "1223-190502-123436", "4426-190505-082337", "7664-190506-172345", "1794-190503-153019", "8782-160314-164728", "9098-190503-130709", "0871-190504-222005", "7418-160304-134543", "7935-180414-151448", "1600-190502-160039", "4212-190506-112017", "4368-180208-114640", "2954-190506-173923", "3041-171216-100407", "8085-190504-132307", "2777-190504-152826", "2367-160202-154910", "5241-160304-135143", "79150406-173009", "4974-160312-143203", "6610-190505-122903", "5816-190503-162916", "5385-190504-120723", "7581-190504-144304", "0001-190506-144037", "2656-171221-125244", "4315-150522-221718", "5392-190503-131034", "8205-150408-153335", "66150407-134049", "4911-190502-141439", "3871-180417-165731", "4113-190228-162133", "4508-160218-104524", "7592-150411-182704", "6923-180421-164155", "8540-150412-171427", "5354-190118-142824", "1504-160225-135321", "1717-190506-171459", "8931-190504-123507", "3826-170129-115321", "40150402-170443", "0199-180419-164815", "5387-190504-145915", "5126-190506-170117", "8284-151230-153326", "2237-170208-172133", "7171-190501-162703", "08150406-135959", "0629-180420-155932", "6082-170111-142111", "6401-190506-164211", "3438-190506-012117", "4298-160316-104022", "0803-160527-172216", "5800-150427-144201", "0404-190511-152726", "0936-160227-200854", "1570-150426-101456", "8117-190506-015022" };
            var appealfacultyDetails = db.jntuh_appeal_faculty_registered.Where(r => r.academicYearId==11& appealcomplianceregs.Contains(r.RegistrationNumber)).ToList();
            
            var jntuh_registered_faculty = db.jntuh_registered_faculty.Where(r => appealcomplianceregs.Contains(r.RegistrationNumber)).ToList();
            var facultyDetails = new List<CollegeFaculty>();
            var departments = db.jntuh_department.AsNoTracking().ToList();
            var specialization = db.jntuh_specialization.AsNoTracking().ToList();
            var degree = db.jntuh_degree.AsNoTracking().ToList();
            //var jntuh_registered_faculty_education = db.jntuh_registered_faculty_education.AsNoTracking();
            foreach (var item in appealfacultyDetails)
            {
                var Reason = string.Empty;
                var collegedetails = db.jntuh_college.Where(a => a.isActive == true&&a.id==item.collegeId).Select(s => s).FirstOrDefault();
                var newreason = string.Empty;
                var data = jntuh_registered_faculty.Where(e => e.RegistrationNumber.Trim() == item.RegistrationNumber.Trim()).Select(e => e).FirstOrDefault();
                var degreeid = data != null && data.jntuh_registered_faculty_education.Where(r => r.educationId != 8).Select(s => s).ToList().Count > 0 ? data.jntuh_registered_faculty_education.Where(s => s.educationId != 8).Select(e => e.educationId).Max() : 0;
                if (data != null)
                {
                    if (data.Absent == true)
                        Reason += "Absent";
                    if (data.type == "Adjunct")
                    {
                        if (Reason != string.Empty)
                            Reason += ",Adjunct Faculty";
                        else
                            Reason += "Adjunct Faculty";
                    }
                    if (data.OriginalCertificatesNotShown == true)
                    {
                        if (Reason != string.Empty)
                            Reason += ",Orginal Certificates Not shown in College Inspection";
                        else
                            Reason += "Orginal Certificates Not shown in College Inspection";
                    }
                    if (data.Xeroxcopyofcertificates == true)
                    {
                        if (Reason != string.Empty)
                            Reason += ",Photo copy of Certificates";
                        else
                            Reason += "Photo copy of Certificates";
                    }
                    if (data.NotQualifiedAsperAICTE == true || degreeid < 4)
                    {
                        if (Reason != string.Empty)
                            Reason += ",Not Qualified as per AICTE/PCI";
                        else
                            Reason += "Not Qualified as per AICTE/PCI";
                    }
                    if (data.NoSCM == true)
                    {
                        if (Reason != string.Empty)
                            Reason += ",no/not valid SCM";
                        else
                            Reason += "no/not valid SCM";
                    }
                    if (data.PANNumber == null)
                    {
                        if (Reason != string.Empty)
                            Reason += ",No PAN Number";
                        else
                            Reason += "No PAN Number";
                    }
                    if (data.IncompleteCertificates == true)
                    {
                        if (Reason != string.Empty)
                            Reason += ",IncompleteCertificates";
                        else
                            Reason += "IncompleteCertificates";
                    }
                    if (data.Blacklistfaculy == true)
                    {
                        if (Reason != string.Empty)
                            Reason += ",Blacklisted Faculty";
                        else
                            Reason += "Blacklisted Faculty";
                    }
                    if (data.NoRelevantUG == "Yes")
                    {
                        if (Reason != string.Empty)
                            Reason += ",NO Relevant UG";
                        else
                            Reason += "NO Relevant UG";
                    }
                    if (data.NoRelevantPG == "Yes")
                    {
                        if (Reason != string.Empty)
                            Reason += ",NO Relevant PG";
                        else
                            Reason += "NO Relevant PG";
                    }
                    if (data.NORelevantPHD == "Yes")
                    {
                        if (Reason != string.Empty)
                            Reason += ",NO Relevant PHD";
                        else
                            Reason += "NO Relevant PHD";
                    }
                    if (data.InvalidPANNumber == true)
                    {
                        if (Reason != string.Empty)
                            Reason += ",InvalidPANNumber";
                        else
                            Reason += "InvalidPANNumber";
                    }
                    if (data.OriginalsVerifiedPHD == true)
                    {
                        if (Reason != string.Empty)
                            Reason += ",No Guide Sign in PHD Thesis";
                        else
                            Reason += "No Guide Sign in PHD Thesis";
                    }
                    if (data.OriginalsVerifiedUG == true)
                    {
                        if (Reason != string.Empty)
                            Reason += ",Complaint PHD Faculty";
                        else
                            Reason += "Complaint PHD Faculty";
                    }
                    if (data.Invaliddegree == true)
                    {
                        if (Reason != string.Empty)
                            Reason += ",AICTE Not Approved University Degrees";
                        else
                            Reason += "AICTE Not Approved University Degrees";
                    }
                    if (data.BAS == "Yes")
                    {
                        if (Reason != string.Empty)
                            Reason += ",BAS Flag";
                        else
                            Reason += "BAS Flag";
                    }
                    if (data.InvalidAadhaar == "Yes")
                    {
                        if (Reason != string.Empty)
                            Reason += ",Invalid/Blur Aadhaar";
                        else
                            Reason += "Invalid/Blur Aadhaar";
                    }
                    if (data.Noclass == true)
                    {
                        if (Reason != string.Empty)
                            Reason += ",No Class in UG/PG";
                        else
                            Reason += "No Class in UG/PG";
                    }
                    if (data.AbsentforVerification == true)
                    {
                        if (Reason != string.Empty)
                            Reason += ",Absentfor Physical Verification";
                        else
                            Reason += "Absentfor Physical Verification";
                    }
                    if (data.NotconsideredPHD == true)
                    {
                        if (Reason != string.Empty)
                            Reason += ",Cosidered as a faculty but not cosidered his/her P.hD";
                        else
                            Reason += "Cosidered as a faculty but not cosidered his/her P.hD";
                    }
                    if (data.NoPGspecialization == true)
                    {
                        if (Reason != string.Empty)
                            Reason += ",no Specialization in PG";
                        else
                            Reason += "no Specialization in PG";
                    }
                    if (data.Genuinenessnotsubmitted == true)
                    {
                        if (Reason != string.Empty)
                            Reason += ",PHD Genuinity not Submitted ";
                        else
                            Reason += "PHD Genuinity not Submitted";
                    }
                    var faculty = new CollegeFaculty
                    {
                        FacultyRegistrationNumber = item.RegistrationNumber,
                        collegeId = item.collegeId,
                        CollegeCode = collegedetails.collegeCode,
                        CollegeName = collegedetails.collegeName,
                        id = jntuh_registered_faculty.Where(e => e.RegistrationNumber.Trim() == item.RegistrationNumber.Trim()).Select(e => e.id).FirstOrDefault(),
                        facultyFirstName = data.FirstName,
                        facultyLastName = data.LastName,
                        facultySurname = data.MiddleName,
                        //department = (data.DepartmentId != null) ? departments.Where(D => D.id == data.DepartmentId).Select(D => D.departmentName).FirstOrDefault() : "",
                        IdentifiedFor = item.IdentifiedFor,
                        department = (item.DepartmentId != null) ? departments.Where(D => D.id == item.DepartmentId).Select(D => D.departmentName).FirstOrDefault() : "",
                        SpecializationName = (item.SpecializationId != null) ? specialization.Where(D => D.id == item.SpecializationId).Select(D => D.specializationName).FirstOrDefault() : "",
                        ViewNotificationDocument = item.NOtificationReport,
                        DegreeId = (item.DepartmentId != null) ? departments.Where(D => D.id == item.DepartmentId).Select(D => D.degreeId).FirstOrDefault() : 0,
                        DegreeName = (item.DegreeId != null) ? degree.Where(D => D.id == item.DegreeId).Select(D => D.degree).FirstOrDefault() : "",
                        ViewAppointmentOrderDocument = item.AppointMentOrder,
                        ViewJoiningReportDocument = item.JoiningOrder,
                        ViewSelectionCommitteeDocument = item.SelectionCommiteMinutes,
                        ViewAppealReverificationSupportDoc = item.AppealReverificationSupportingDocument,
                        isActive = item.isActive,
                        Reason = Reason,
                        updatedOn = item.updatedOn,
                        NewReason = newreason
                    };
                    facultyDetails.Add(faculty);
                }
            }
            return View(facultyDetails.OrderBy(a=>a.CollegeName).ToList());
        }

        [Authorize(Roles = "Admin,SuperAdmin,FacultyVerification")]
        public ActionResult FacultyComplianceAddingToCollege(string fid, int? collegeid)
        {
            if (Membership.GetUser() == null)
            {
                return RedirectToAction("LogOn", "Account");
            }
            var userId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            var fId = 0;
            var appealid = 0;
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int prAy =
                db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
            if (!string.IsNullOrEmpty(fid))
            {
                fId = Convert.ToInt32(Utilities.DecryptString(fid, WebConfigurationManager.AppSettings["CryptoKey"]));
            }
            var facultyregistered = db.jntuh_registered_faculty.Find(fId);
            var jntuhAppealFacultyRegistered = db.jntuh_appeal_faculty_registered.Where(r => r.RegistrationNumber == facultyregistered.RegistrationNumber.Trim() && r.academicYearId == prAy).Select(s => s).FirstOrDefault();
            if (jntuhAppealFacultyRegistered != null)
            {
                appealid = jntuhAppealFacultyRegistered.id;
            }
            var facultydetails = db.jntuh_appeal_faculty_registered.Find(appealid);
            var collegefacultyreg = db.jntuh_college_faculty_registered.AsNoTracking().Where(i => i.RegistrationNumber.Trim() == facultyregistered.RegistrationNumber.Trim()).ToList();
            var collegeFaculty = new jntuh_college_faculty_registered();
            //if (facultydetails != null && collegefacultyreg.Count == 0)
            //{
            //    collegeFaculty.collegeId = facultydetails.collegeId;
            //    collegeFaculty.RegistrationNumber = facultydetails.RegistrationNumber;
            //    collegeFaculty.IdentifiedFor = facultydetails.IdentifiedFor;
            //    collegeFaculty.SpecializationId = facultydetails.SpecializationId;
            //    collegeFaculty.DepartmentId = facultydetails.DepartmentId;
            //    collegeFaculty.existingFacultyId = facultydetails.existingFacultyId;
            //    collegeFaculty.AadhaarNumber = facultydetails.AadhaarNumber == null ? facultydetails.AadhaarNumber : facultydetails.AadhaarNumber.Trim();
            //    if (facultydetails.AadhaarDocument != null)
            //    {
            //        string[] values = facultydetails.AadhaarDocument.Split('/');
            //        int i = values.Count() - 1;
            //        collegeFaculty.AadhaarDocument = values[i].ToString();

            //        //Complaince Faculty Aadhar Document Copying to College Faculty folder
            //        string NewDir = "D:/JNTUH/Prod/Content/Upload/Faculty/AADHAARCARDS/CollegeFacultyAadhaar";
            //        if (System.IO.File.Exists("D:/JNTUH/Prod/Content/Upload/OnlineAppealDocuments/Faculty/FacultyAadhaarDocuments/" + collegeFaculty.AadhaarDocument))
            //        {
            //            var appealfolder = Path.Combine("D:/JNTUH/Prod/Content/Upload/OnlineAppealDocuments/Faculty/FacultyAadhaarDocuments/", collegeFaculty.AadhaarDocument);
            //            if (!System.IO.File.Exists("D:/JNTUH/Prod/Content/Upload/Faculty/AADHAARCARDS/CollegeFacultyAadhaar/" + collegeFaculty.AadhaarDocument))
            //            {
            //                System.IO.File.Copy(appealfolder, Path.Combine(NewDir, collegeFaculty.AadhaarDocument));
            //            }
            //        }
            //    }
            //    if (facultydetails.AppealReverificationScreenshot != null)
            //    {
            //        string[] values = facultydetails.AppealReverificationScreenshot.Split('/');
            //        int i = values.Count() - 1;
            //        var allcertificates = values[i].ToString();

            //        var facultyothercertifictes =
            //            db.jntuh_registered_faculty_education.Where(
            //                r => r.facultyId == facultyregistered.id && r.educationId == 8)
            //                .Select(s => s)
            //                .FirstOrDefault();
            //        if (facultyothercertifictes == null)
            //        {
            //            string NewDir = "D:/JNTUH/Prod/Content/Upload/Faculty/Certificates/CertificatesPDF";
            //            if (System.IO.File.Exists("D:/JNTUH/Prod/Content/Upload/OnlineAppealDocuments/Faculty/AppealReverificationScreenshot/" + allcertificates))
            //            {
            //                var appealfolder = Path.Combine("D:/JNTUH/Prod/Content/Upload/OnlineAppealDocuments/Faculty/AppealReverificationScreenshot/", allcertificates);
            //                if (!System.IO.File.Exists("D:/JNTUH/Prod/Content/Upload/Faculty/Certificates/CertificatesPDF" + allcertificates))
            //                {
            //                    System.IO.File.Copy(appealfolder, Path.Combine(NewDir, allcertificates));
            //                }
            //            }
            //            jntuh_registered_faculty_education othereducation = new jntuh_registered_faculty_education();
            //            othereducation.hallticketnumber = "0";
            //            othereducation.facultyId = facultyregistered.id;
            //            othereducation.educationId = 8;
            //            othereducation.courseStudied = "Others";
            //            othereducation.specialization = "Others";
            //            othereducation.passedYear = 0;
            //            othereducation.marksPercentage = 0;
            //            othereducation.division = 0;
            //            othereducation.boardOrUniversity = "Others";
            //            othereducation.placeOfEducation = "Others";
            //            othereducation.certificate = allcertificates;
            //            othereducation.isActive = false;
            //            othereducation.createdBy = userId;
            //            othereducation.createdOn = DateTime.Now;
            //            othereducation.updatedBy = null;
            //            othereducation.updatedOn = null;
            //            db.jntuh_registered_faculty_education.Add(othereducation);
            //            db.SaveChanges();
            //        }
            //    }
                //collegeFaculty.FacultySpecializationId = facultydetails.FacultySpecializationId;
                //collegeFaculty.createdBy = userId;
                //collegeFaculty.createdOn = DateTime.Now;
                //db.jntuh_college_faculty_registered.Add(collegeFaculty);
                //db.SaveChanges();
                //TempData["Success"] =collegeFaculty.RegistrationNumber+ " Registration Number Is Successfully Added to College Faculty..";
            TempData["Success"] =collegeFaculty.RegistrationNumber+ " Registration Number Is Approved Successfully.But not Added to College Faculty..";
                facultydetails.isActive = true;
                facultydetails.updatedOn=DateTime.Now;
                facultydetails.updatedBy = userId;
                db.SaveChanges();
                if (facultyregistered != null)
                {
                    facultyregistered.Absent = false;
                    facultyregistered.Xeroxcopyofcertificates = false;
                    facultyregistered.NoRelevantUG = "No";
                    facultyregistered.NoRelevantPG = "No";
                    facultyregistered.NORelevantPHD = "No";
                    facultyregistered.NotQualifiedAsperAICTE = false;
                    facultyregistered.InvalidPANNumber = false;
                    facultyregistered.IncompleteCertificates = false;
                    facultyregistered.NoSCM = false;
                    facultyregistered.OriginalCertificatesNotShown = false;
                    facultyregistered.NotIdentityfiedForanyProgram = false;
                    facultyregistered.Noclass = false;
                    facultyregistered.NotconsideredPHD = false;
                    facultyregistered.Genuinenessnotsubmitted = false;
                    facultyregistered.BAS = "No";
                    facultyregistered.InvalidAadhaar = "No";
                    facultyregistered.OriginalsVerifiedUG = false;
                    facultyregistered.OriginalsVerifiedPHD = false;
                    facultyregistered.AbsentforVerification = false;
                    facultyregistered.Invaliddegree = false;
                    facultyregistered.NoPGspecialization = false;
                    db.Entry(facultyregistered).State = EntityState.Modified;
                    db.SaveChanges();
                }
            if(facultydetails != null && collegefacultyreg.Count > 0)
            {
                TempData["Success"] = "Registration Number Is already Added to College Faculty..";              
            }
            else if (facultydetails == null)
            {
                TempData["Error"] = "Registration Number Was not found";
            }
            return RedirectToAction("ComplianceFaculty");
        }

        [Authorize(Roles = "Admin,SuperAdmin,FacultyVerification")]
        public ActionResult FacultyNotAddedToCollege(string fid, int? collegeid)
        {
            if (Membership.GetUser() == null)
            {
                return RedirectToAction("LogOn", "Account");
            }
            var userId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            var fId = 0;
            var appealid = 0;
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int prAy =
                db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
            if (!string.IsNullOrEmpty(fid))
            {
                fId = Convert.ToInt32(Utilities.DecryptString(fid, WebConfigurationManager.AppSettings["CryptoKey"]));
            }
            var facultyregistered = db.jntuh_registered_faculty.Find(fId);
            var jntuhAppealFacultyRegistered = db.jntuh_appeal_faculty_registered.Where(r => r.RegistrationNumber == facultyregistered.RegistrationNumber.Trim() && r.academicYearId == prAy&&r.isActive==false).Select(s => s).FirstOrDefault();

            if (jntuhAppealFacultyRegistered!=null)
            {
                jntuhAppealFacultyRegistered.isActive =false;
                jntuhAppealFacultyRegistered.updatedOn =DateTime.Now;
                jntuhAppealFacultyRegistered.updatedBy = userId;
                db.Entry(facultyregistered).State = EntityState.Modified;
                db.SaveChanges();
                TempData["Error"] =jntuhAppealFacultyRegistered.RegistrationNumber+" Registration Number Is Not approved..";
            }
            return RedirectToAction("ComplianceFaculty");
        }
    }
}
