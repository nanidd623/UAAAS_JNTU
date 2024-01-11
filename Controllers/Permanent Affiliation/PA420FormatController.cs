using iTextSharp.text;
using iTextSharp.text.html.simpleparser;
using iTextSharp.text.pdf;
//using iTextSharp.tool.xml;
//using iTextSharp.tool.xml.html;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using UAAAS.Models;
using System.Web.Security;
using iTextSharp.text.html;
using System.Diagnostics;
using System.Data.Entity.Infrastructure;
using System.Drawing.Imaging;
using System.Data.Entity.Validation;
using System.Globalization;
using UAAAS.Models.Permanent_Affiliation;


namespace UAAAS.Controllers.Permanent_Affiliation
{
    public class PA420FormatController : Controller
    {
        private uaaasDBContext db = new uaaasDBContext();
        private string serverURL;
        private string test415PDF;
        private string barcodetext;
        private string LatefeeQrCodetext;
        public PA420FormatController()
        {

        }

        public ActionResult Index()
        {
            return View();
        }

        [Authorize(Roles = "College,Admin")]
        public ActionResult CollegeData(int preview, int collegeId)
        {
            try
            {
                serverURL = Request.Url.Scheme + System.Uri.SchemeDelimiter + Request.Url.Host + (Request.Url.IsDefaultPort ? "" : ":" + Request.Url.Port);
                //int collegeId = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(strcollegeId, WebConfigurationManager.AppSettings["CryptoKey"]));
                string s = ConfigurationManager.AppSettings["Test416PDF"];
                if (!String.IsNullOrEmpty(s))
                {
                    if (s.Equals("1"))
                        test415PDF = "YES";
                    else
                        test415PDF = "NO";
                }
                else
                {
                    test415PDF = "NO";
                }
                //collegeId = 4;
                string pdfPath = SaveCollegeDataPdf(preview, collegeId);
                string path = pdfPath.Replace("/", "\\");

                string collegeCode = db.jntuh_college.Find(collegeId).collegeCode;
                return File(path, "application/pdf", "PA-420-" + collegeCode + ".pdf");
            }
            catch (Exception ex)
            {
                return Json(new { status = "error", message = ex }, JsonRequestBehavior.AllowGet);
            }
        }

        #region SaveCollegeDataPdf

        private string SaveCollegeDataPdf(int preview, int collegeId)
        {
            string fullPath = string.Empty;
            //Set page size as A4
            //var pdfDoc = new Document(PageSize.A4, 10, 10, 10, 10);
            Document pdfDoc = new Document(PageSize.A4, 60, 50, 60, 60);

            // Document pdfDoc = new Document(PageSize.A4, 50, 50,50,50);
            string path = Server.MapPath("~/Content/PDFReports");

            // Create a new PdfWrite object, writing the output to a MemoryStream
            //open pdf
            var output = new MemoryStream();
            var writer = PdfWriter.GetInstance(pdfDoc, output);

            // PageEventHelper pageEventHelper = new PageEventHelper();
            //  writer.PageEvent = pageEventHelper;
            //  writer.PageEvent.OnEndPage()
            var CollegeDataNew = db.jntuh_college.Where(c => c.id == collegeId).Select(c => new
            {
                Collegecode = c.collegeCode,
                CollegeName = c.collegeName
            }).SingleOrDefault();

            string collegeCode = CollegeDataNew.Collegecode;
            string collegeName = CollegeDataNew.CollegeName;

            if (preview == 0)
            {

                fullPath = path + "/CollegeData/A-420_" + collegeCode + "_" + DateTime.Now.ToString("yyyMMddHHmmss") + ".pdf";
                PdfWriter pdfWriter = PdfWriter.GetInstance(pdfDoc, new FileStream(fullPath, FileMode.Create));
                ITextEvents iTextEvents = new ITextEvents();
                iTextEvents.CollegeName = collegeName;
                iTextEvents.CollegeCode = collegeCode;
                // iTextEvents.formType = "A-414";
                iTextEvents.formType = "A-418";
                pdfWriter.PageEvent = iTextEvents;
            }
            else
            {

                fullPath = path + "/CollegeData/PA-420_" + collegeCode + "_" + DateTime.Now.ToString("yyyMMddHHmmss") +
                           ".pdf";
                PdfWriter pdfWriter = PdfWriter.GetInstance(pdfDoc, new FileStream(fullPath, FileMode.Create));
                ITextEvents iTextEvents = new ITextEvents();
                iTextEvents.CollegeName = collegeName;
                iTextEvents.CollegeCode = collegeCode;
                pdfWriter.PageEvent = iTextEvents;
            }

            //Open PDF Document to write data
            pdfDoc.Open();

            //Assign Html content in a string to write in PDF
            string contents = string.Empty;

            StreamReader sr;

            //Read file from server path

            sr = System.IO.File.OpenText(Server.MapPath("~/Content/PA-420.html"));

            //store content in the variable
            contents = sr.ReadToEnd();
            sr.Close();

            string tMethodNumber =
                db.jntuh_settings.Where(s => s.isLive == true).Select(s => s.testMobile).FirstOrDefault();
            string tCollegeID =
                db.jntuh_settings.Where(s => s.isLive == true).Select(s => s.testEmail).FirstOrDefault();

            string[] TestMethodNumber = new string[] { };
            int TestCollegeId = 0;

            if (!string.IsNullOrEmpty(tMethodNumber))
            {
                TestMethodNumber = tMethodNumber.Split(',');
            }

            if (!string.IsNullOrEmpty(tCollegeID))
            {
                TestCollegeId = int.Parse(tCollegeID.ToString());
            }

            var actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(q => q.actualYear).FirstOrDefault();
            int prAy = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();

            var cSpcIds =
           db.jntuh_college_intake_existing.Where(i => i.collegeId == collegeId && (i.academicYearId == prAy - 1 || i.academicYearId == prAy - 2 || i.academicYearId == prAy - 3) && (i.approvedIntake != 0 || i.aicteApprovedIntake != 0))
                //.GroupBy(r => new { r.specializationId })
               .Select(s => s.specializationId)
               .ToList();

            var DepartmentsData = db.jntuh_specialization.Where(e => e.isActive == true && cSpcIds.Contains(e.id)).Select(e => e.departmentId).Distinct().ToArray();

            var DegreeIds = db.jntuh_department.Where(e => e.isActive == true && DepartmentsData.Contains(e.id)).Select(e => e.degreeId).Distinct().ToList();
            //var jntuh_degree = db.jntuh_degree.Where(d => DegreeIds.Contains(d.id)).OrderBy(d => d.degreeDisplayOrder).ToList();

            contents = contents.Replace("##SERVERURL##", serverURL);
            
            //if (collegeId != TestCollegeId || (collegeId == TestCollegeId && !TestMethodNumber.Contains("1")))
            //    contents = affiliationType(collegeId, contents);
            //EventLog.WriteEntry("affiliationType", "affiliationType");
            if (collegeId != TestCollegeId || (collegeId == TestCollegeId && !TestMethodNumber.Contains("2")))
                contents = collegeInformation(collegeId, contents);
            //EventLog.WriteEntry("collegeInformation", "collegeInformation");
            if (collegeId != TestCollegeId || (collegeId == TestCollegeId && !TestMethodNumber.Contains("3")))
                contents = EducationalSocietyDetails(collegeId, contents);
            //EventLog.WriteEntry("EducationalSocietyDetails", "EducationalSocietyDetails");
            if (collegeId != TestCollegeId || (collegeId == TestCollegeId && !TestMethodNumber.Contains("3")))
                contents = AffiliationInformation(collegeId, contents);
            if (collegeId != TestCollegeId || (collegeId == TestCollegeId && !TestMethodNumber.Contains("4")))
                contents = PrincipalDirectorDetails(collegeId, contents);

            //EventLog.WriteEntry("PrincipalDirectorDetails", "PrincipalDirectorDetails");
            if (collegeId != TestCollegeId || (collegeId == TestCollegeId && !TestMethodNumber.Contains("5")))
                contents = ChairpersonDetails(collegeId, contents);
            //EventLog.WriteEntry("ChairpersonDetails", "ChairpersonDetails");
            if (collegeId != TestCollegeId || (collegeId == TestCollegeId && !TestMethodNumber.Contains("7")))
                contents = LandInformationDetails(collegeId, contents);
            //EventLog.WriteEntry("LandInformationDetails", "LandInformationDetails");
            if (collegeId != TestCollegeId || (collegeId == TestCollegeId && !TestMethodNumber.Contains("8")))
                contents = AdministrativeLandDetails(collegeId, contents);
            //EventLog.WriteEntry("AdministrativeLandDetails", "AdministrativeLandDetails");
            if (collegeId != TestCollegeId || (collegeId == TestCollegeId && !TestMethodNumber.Contains("9")))
                contents = InstructionalAreaDetails(collegeId, contents);
            if (collegeId != TestCollegeId || (collegeId == TestCollegeId && !TestMethodNumber.Contains("9")))
                contents = ExistingIntakeDetails(collegeId, contents);
            //EventLog.WriteEntry("InstructionalAreaDetails", "InstructionalAreaDetails");
            if (collegeId != TestCollegeId || (collegeId == TestCollegeId && !TestMethodNumber.Contains("9")))
                contents = RemedialTeaching(collegeId, contents);
            if (collegeId != TestCollegeId || (collegeId == TestCollegeId && !TestMethodNumber.Contains("2")))
                contents = FacultyInformation(collegeId, contents);
            if (collegeId != TestCollegeId || (collegeId == TestCollegeId && !TestMethodNumber.Contains("2")))
                contents = FinancialStandards(collegeId, contents);
            if (collegeId != TestCollegeId || (collegeId == TestCollegeId && !TestMethodNumber.Contains("2")))
                contents = CourtCases(collegeId, contents);
            if (collegeId != TestCollegeId || (collegeId == TestCollegeId && !TestMethodNumber.Contains("2")))
                contents = BooksList(collegeId, contents);
            if (collegeId != TestCollegeId || (collegeId == TestCollegeId && !TestMethodNumber.Contains("2")))
                contents = JournalsList(collegeId, contents);
            if (collegeId != TestCollegeId || (collegeId == TestCollegeId && !TestMethodNumber.Contains("2")))
                contents = EBooksList(collegeId, contents);
            if (collegeId != TestCollegeId || (collegeId == TestCollegeId && !TestMethodNumber.Contains("2")))
                contents = EJournalsList(collegeId, contents);
            if (collegeId != TestCollegeId || (collegeId == TestCollegeId && !TestMethodNumber.Contains("2")))
                contents = EssentialRequirements(collegeId, contents);
            if (collegeId != TestCollegeId || (collegeId == TestCollegeId && !TestMethodNumber.Contains("2")))
                contents = DesirableRequirements(collegeId, contents);
            if (collegeId != TestCollegeId || (collegeId == TestCollegeId && !TestMethodNumber.Contains("2")))
                contents = AnyOtherInformation(collegeId, contents);
            if (collegeId != TestCollegeId || (collegeId == TestCollegeId && !TestMethodNumber.Contains("2")))
                contents = BestPractisesAdopted(collegeId, contents);
            if (collegeId != TestCollegeId || (collegeId == TestCollegeId && !TestMethodNumber.Contains("2")))
                contents = FinancialStatus(collegeId, contents);
            if (collegeId != TestCollegeId || (collegeId == TestCollegeId && !TestMethodNumber.Contains("2")))
                contents = TeachingFaculty(collegeId, contents);
            if (collegeId != TestCollegeId || (collegeId == TestCollegeId && !TestMethodNumber.Contains("2")))
                contents = FacultyOppurtunities(collegeId, contents);
            if (collegeId != TestCollegeId || (collegeId == TestCollegeId && !TestMethodNumber.Contains("2")))
                contents = SelfAppraisal(collegeId, contents);
            if (collegeId != TestCollegeId || (collegeId == TestCollegeId && !TestMethodNumber.Contains("2")))
                contents = ResearchGrants(collegeId, contents);
            if (collegeId != TestCollegeId || (collegeId == TestCollegeId && !TestMethodNumber.Contains("2")))
                contents = MoUs(collegeId, contents);
            //if (collegeId != TestCollegeId || (collegeId == TestCollegeId && !TestMethodNumber.Contains("6")))
            //    contents = OthercollegesandOtherCoursesDetails(collegeId, contents);
            ////EventLog.WriteEntry("OthercollegesandOtherCoursesDetails", "OthercollegesandOtherCoursesDetails");



            //if (collegeId != TestCollegeId || (collegeId == TestCollegeId && !TestMethodNumber.Contains("10")))
            //    contents = ExistingIntakeDetails(collegeId, contents);
            ////EventLog.WriteEntry("ExistingIntakeDetails", "ExistingIntakeDetails");
            //if (collegeId != TestCollegeId || (collegeId == TestCollegeId && !TestMethodNumber.Contains("11")))
            //    contents = AcademicPerformanceDetails(collegeId, contents);
            ////EventLog.WriteEntry("AcademicPerformanceDetails", "AcademicPerformanceDetails");
            //if (collegeId != TestCollegeId || (collegeId == TestCollegeId && !TestMethodNumber.Contains("12")))
            //    contents = collegeTachingFacultyMembers(collegeId, contents);
            ////EventLog.WriteEntry("collegeTachingFacultyMembers", "collegeTachingFacultyMembers");
            //if (collegeId != TestCollegeId || (collegeId == TestCollegeId && !TestMethodNumber.Contains("13")))
            //    contents = collegeNonTachingFacultyMembers(collegeId, contents);
            ////EventLog.WriteEntry("collegeNonTachingFacultyMembers", "collegeNonTachingFacultyMembers");
            //if (collegeId != TestCollegeId || (collegeId == TestCollegeId && !TestMethodNumber.Contains("14")))
            //    contents = collegeTechnicalFacultyMembers(collegeId, contents);
            //// EventLog.WriteEntry("collegeTechnicalFacultyMembers", "collegeTechnicalFacultyMembers");
            //if (collegeId != TestCollegeId || (collegeId == TestCollegeId && !TestMethodNumber.Contains("15")))
            //    contents = LaboratoriesDetails(collegeId, contents);

            ////EventLog.WriteEntry("LaboratoriesDetails", "LaboratoriesDetails");
            //if (collegeId != TestCollegeId || (collegeId == TestCollegeId && !TestMethodNumber.Contains("16")))
            //    contents = LibraryDetails(collegeId, contents, DegreeIds);
            ////EventLog.WriteEntry("LibraryDetails", "LibraryDetails");
            //if (collegeId != TestCollegeId || (collegeId == TestCollegeId && !TestMethodNumber.Contains("17")))
            //    contents = InternetBandwidthDetails(collegeId, contents, DegreeIds);
            ////EventLog.WriteEntry("InternetBandwidthDetails", "InternetBandwidthDetails");
            //if (collegeId != TestCollegeId || (collegeId == TestCollegeId && !TestMethodNumber.Contains("18")))
            //    contents = LegalSystemSoftwareDetails(collegeId, contents, DegreeIds);
            ////EventLog.WriteEntry("LegalSystemSoftwareDetails", "LegalSystemSoftwareDetails");
            //if (collegeId != TestCollegeId || (collegeId == TestCollegeId && !TestMethodNumber.Contains("19")))
            //    contents = PrintersDetails(collegeId, contents, DegreeIds);
            ////EventLog.WriteEntry("PrintersDetails", "PrintersDetails");
            //if (collegeId != TestCollegeId || (collegeId == TestCollegeId && !TestMethodNumber.Contains("20")))
            //    contents = ExaminationBranchDetails(collegeId, contents);
            ////EventLog.WriteEntry("ExaminationBranchDetails", "ExaminationBranchDetails");
            //if (collegeId != TestCollegeId || (collegeId == TestCollegeId && !TestMethodNumber.Contains("21")))
            //    contents = DesirableRequirementsDetails(collegeId, contents);
            ////EventLog.WriteEntry("DesirableRequirementsDetails", "DesirableRequirementsDetails");
            //if (collegeId != TestCollegeId || (collegeId == TestCollegeId && !TestMethodNumber.Contains("22")))
            //    contents = AntiRaggingCommitteeDetails(collegeId, contents);
            ////EventLog.WriteEntry("AntiRaggingCommitteeDetails", "AntiRaggingCommitteeDetails");
            //if (collegeId != TestCollegeId || (collegeId == TestCollegeId && !TestMethodNumber.Contains("23")))
            //    contents = WomenProtectionCellDetails(collegeId, contents);
            ////EventLog.WriteEntry("WomenProtectionCellDetails", "WomenProtectionCellDetails");
            //if (collegeId != TestCollegeId || (collegeId == TestCollegeId && !TestMethodNumber.Contains("24")))
            //    contents = RTIDetails(collegeId, contents);
            ////EventLog.WriteEntry("RTIDetails", "RTIDetails");
            //if (collegeId != TestCollegeId || (collegeId == TestCollegeId && !TestMethodNumber.Contains("25")))
            //    contents = OtherDesirablesDetails(collegeId, contents);
            ////EventLog.WriteEntry("OtherDesirablesDetails", "OtherDesirablesDetails");
            //if (collegeId != TestCollegeId || (collegeId == TestCollegeId && !TestMethodNumber.Contains("26")))
            //    contents = CampusHostelMaintenanceDetails(collegeId, contents);
            ////EventLog.WriteEntry("CampusHostelMaintenanceDetails", "CampusHostelMaintenanceDetails");
            //if (collegeId != TestCollegeId || (collegeId == TestCollegeId && !TestMethodNumber.Contains("27")))
            //    contents = OperationalFundsDetails(collegeId, contents);
            ////EventLog.WriteEntry("OperationalFundsDetails", "OperationalFundsDetails");
            //if (collegeId != TestCollegeId || (collegeId == TestCollegeId && !TestMethodNumber.Contains("28")))
            //    contents = IncomeDetails(collegeId, contents);
            ////EventLog.WriteEntry("IncomeDetails", "IncomeDetails");
            //if (collegeId != TestCollegeId || (collegeId == TestCollegeId && !TestMethodNumber.Contains("29")))
            //    contents = ExpenditureDetails(collegeId, contents);
            ////EventLog.WriteEntry("ExpenditureDetails", "ExpenditureDetails");
            //if (collegeId != TestCollegeId || (collegeId == TestCollegeId && !TestMethodNumber.Contains("30")))
            //    contents = StudentsPlacementDetails(collegeId, contents);//, cSpcIds, DepartmentsData, DegreeIds
            ////EventLog.WriteEntry("StudentsPlacementDetails", "StudentsPlacementDetails");
            //if (collegeId != TestCollegeId || (collegeId == TestCollegeId && !TestMethodNumber.Contains("31")))
            //    contents = CollegePhotosDetails(collegeId, contents);
            ////EventLog.WriteEntry("CollegePhotosDetails", "CollegePhotosDetails");
            //if (collegeId != TestCollegeId || (collegeId == TestCollegeId && !TestMethodNumber.Contains("32")))
            //    contents = PaymentDetails(collegeId, contents);
            ////EventLog.WriteEntry("PaymentDetails", "PaymentDetails");
            //if (collegeId != TestCollegeId || (collegeId == TestCollegeId && !TestMethodNumber.Contains("33")))
            //    contents = PaymentOfFee(collegeId, contents);
            ////EventLog.WriteEntry("PaymentOfFee", "PaymentOfFee");
            //if (collegeId != TestCollegeId || (collegeId == TestCollegeId && !TestMethodNumber.Contains("34")))
            //    contents = collegeEnclosures(collegeId, contents);
            ////EventLog.WriteEntry("collegeEnclosures", "collegeEnclosures");
            //if (collegeId != TestCollegeId || (collegeId == TestCollegeId && !TestMethodNumber.Contains("35")))
            //    contents = DataModifications(collegeId, contents);
            //if (collegeId != TestCollegeId || (collegeId == TestCollegeId && !TestMethodNumber.Contains("36")))
            //    contents = AcademicAudit(collegeId, contents);
            //if (collegeId != TestCollegeId || (collegeId == TestCollegeId && !TestMethodNumber.Contains("37")))
            //    contents = ValueAddedPrograms(collegeId, contents);
            //if (collegeId != TestCollegeId || (collegeId == TestCollegeId && !TestMethodNumber.Contains("38")))
            //    contents = EssentialRequirements(collegeId, contents);
            System.GC.Collect();

            // EventLog.WriteEntry("DataModifications", "DataModifications");
            //if (collegeId != TestCollegeId || (collegeId == TestCollegeId && !TestMethodNumber.Contains("15")))
            //    contents = ExperimentDetails(collegeId, contents);

            //Commented on 19-12-2019
            contents = PaymentBillDetails(collegeId, contents);
            //contents = LateFeePaymentBillDetails(collegeId, contents);
            //contents = NOofPhysicalLabs(collegeId, contents);


            //contents = barcodegenerator(collegeId, contents);


            //contents = LateFeebarcodegenerator(collegeId, contents);
            //EventLog.WriteEntry("ExperimentDetails", "ExperimentDetails");
            //RAMESH: To identify Q6 A-416 error on server
            if (collegeId == TestCollegeId)
            {
                CreateFile(contents, collegeCode);
            }
            //Read string contents using stream reader and convert html to parsed conent

            List<IElement> parsedHtmlElements = HTMLWorker.ParseToList(new StringReader(contents), null);

            //Get each array values from parsed elements and add to the PDF document
            bool pageRotated = false;

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
                        if (htmlElement.Chunks.Count == 4 || htmlElement.Chunks.Count == 7)
                        {
                            pdfDoc.SetPageSize(iTextSharp.text.PageSize.A4.Rotate());
                            pdfDoc.SetMargins(60, 50, 60, 60);
                            pageRotated = true;
                        }
                        else
                        {
                            if (pageRotated == true)
                            {
                                pdfDoc.SetPageSize(iTextSharp.text.PageSize.A4);
                                pdfDoc.SetMargins(60, 50, 60, 60);
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

            string returnPath = string.Empty;
            returnPath = fullPath;
            return returnPath;
        }

        #endregion

        //to create text file
        public void CreateFile(string html, string collegecode)
        {
            string name = Server.MapPath("~/Content/PDFReports/ptemp/") + collegecode + "-" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".txt";

            FileInfo info = new FileInfo(name);
            if (!System.IO.File.Exists(name))
            {
                using (StreamWriter writer = info.CreateText())
                {
                    writer.Write(html);
                }
            }

        }
        //03/05/2014 End

        private string AffiliationInformation(int collegeId, string contents)
        {
            using (var db = new UAAAS.Models.uaaasDBContext())
            {
                #region from jntuh_college_affiliation table
                int NACId = 0;
                string affiliationNAAC = string.Empty;
                string affiliationNAACFromDate = string.Empty;
                string affiliationNAACToDate = string.Empty;
                string affiliationNAACYes = string.Empty;
                string affiliationNAACNo = string.Empty;
                string affiliationNAACGrade = string.Empty;
                string affiliationNAACCGPA = string.Empty;
                string collegeAffiliationType = string.Empty;
                string duration = string.Empty;
                string affStatus = string.Empty;
                var affiliationType = db.jntuh_affiliation_type
                    .OrderBy(a => a.id)
                    .Where(a => a.isActive)
                    .OrderBy(c => c.displayOrder)
                    .Select(it => new { it.id, it.affiliationType })
                    .ToList();
                var collegeAfflitaions = db.jntuh_college_affiliation
                    .Where(a => a.collegeId == collegeId)
                    .ToList();
                foreach (var item in affiliationType)
                {
                    var affiliationCollegeType = collegeAfflitaions.Where(g => g.affiliationTypeId == item.id && g.affiliationStatus == "Yes").OrderByDescending(a => a.affiliationToDate).Take(1);
                    if (affiliationCollegeType.Count() > 0)
                    {
                        affiliationNAACFromDate = affiliationCollegeType.FirstOrDefault().affiliationFromDate != null ? UAAAS.Models.Utilities.MMDDYY2DDMMYY(affiliationCollegeType.FirstOrDefault().affiliationFromDate.ToString()) : "";
                        affiliationNAACToDate = affiliationCollegeType.FirstOrDefault().affiliationToDate != null ? UAAAS.Models.Utilities.MMDDYY2DDMMYY(affiliationCollegeType.FirstOrDefault().affiliationToDate.ToString()) : "";
                        duration = Convert.ToString(affiliationCollegeType.FirstOrDefault().affiliationDuration);
                        affiliationNAACGrade = affiliationCollegeType.FirstOrDefault().affiliationGrade;
                        affiliationNAACCGPA = affiliationCollegeType.FirstOrDefault().CGPA;
                        affStatus = affiliationCollegeType.FirstOrDefault().affiliationStatus;
                    }
                    var NBANAASStratus = string.Empty;
                    if (!string.IsNullOrEmpty(affiliationNAACFromDate) && !string.IsNullOrEmpty(affiliationNAACToDate))
                    {
                        NBANAASStratus = "Conferred";
                    }
                    else if (affStatus == "Applied")
                    {
                        NBANAASStratus = "Applied";
                    }
                    else
                    {
                        NBANAASStratus = "Not Yet Applied";
                    }
                    switch (item.affiliationType.Trim())
                    {
                        case "NAAC":
                            collegeAffiliationType += "<td valign='top' style='font-size: 9px;' colspan='4'>" + item.affiliationType + "</td>";
                            collegeAffiliationType += "<td valign='top' style='font-size: 9px;' colspan='1' align='center'>:</td>";
                            collegeAffiliationType += "<td valign='top' style='font-size: 9px;' colspan='5'><p>";
                            collegeAffiliationType += NBANAASStratus + "</p></td>";
                            collegeAffiliationType += "<td valign='top' style='font-size: 9px;' colspan='4'>If Conferred, Period </td>";
                            collegeAffiliationType += "<td valign='top' style='font-size: 9px;' colspan='1' align='center'>:</td>";
                            collegeAffiliationType += "<td valign='top' style='font-size: 9px;' colspan='5'>From:" + affiliationNAACFromDate + "<br/>";
                            collegeAffiliationType += "TO:" + affiliationNAACToDate + "<br/>";
                            collegeAffiliationType += "Duration:" + duration + "<br/>";
                            collegeAffiliationType += "Grade: " + affiliationNAACGrade + "<br/>";
                            collegeAffiliationType += "CGPA: " + affiliationNAACCGPA + "</td>";
                            collegeAffiliationType += "</tr>";
                            collegeAffiliationType += "<br />";
                            NACId = item.id;
                            break;
                        //case "NBA Status":
                        //    collegeAffiliationType += "<tr>";
                        //    collegeAffiliationType += "<td valign='top' style='font-size: 9px;' colspan='4'>" + item.affiliationType + "</td>";
                        //    collegeAffiliationType += "<td valign='top' style='font-size: 9px;' colspan='1' align='center'>:</td>";
                        //    collegeAffiliationType += "<td valign='top' style='font-size: 9px;' colspan='5'><p>";
                        //    collegeAffiliationType += NBANAASStratus + "</p></td>";
                        //    collegeAffiliationType += "<td valign='top' style='font-size: 9px;' colspan='4'>If Conferred, Period </td>";
                        //    collegeAffiliationType += "<td valign='top' style='font-size: 9px;' colspan='1' align='center'>:</td>";
                        //    collegeAffiliationType += "<td valign='top' style='font-size: 9px;' colspan='5'>From:" + affiliationNAACFromDate + "<br/>";
                        //    collegeAffiliationType += "TO:" + affiliationNAACToDate + "<br/>";
                        //    collegeAffiliationType += "Duration:" + duration + "<br/>";
                        //    collegeAffiliationType += "Grade: " + affiliationNAACGrade + "<br/>";
                        //    collegeAffiliationType += "CGPA: " + affiliationNAACCGPA + "</td>";
                        //    collegeAffiliationType += "</tr>";
                        //    collegeAffiliationType += "<br />";
                        //    NACId = item.id;

                        //    break;
                        default:
                            collegeAffiliationType += "<tr>";
                            collegeAffiliationType += "<td valign='top' style='font-size: 9px;' colspan='4'>" + item.affiliationType + "</td>";
                            collegeAffiliationType += "<td valign='top' style='font-size: 9px;' colspan='1' align='center'>:</td>";
                            collegeAffiliationType += "<td valign='top' style='font-size: 9px;' colspan='5'><p>";
                            collegeAffiliationType += !string.IsNullOrEmpty(affiliationNAACFromDate) ? "YES" : "NO" + "</p></td>";
                            collegeAffiliationType += "<td valign='top' style='font-size: 9px;' colspan='4'>If Yes, Period </td>";
                            collegeAffiliationType += "<td valign='top' style='font-size: 9px;' colspan='1' align='center'>:</td>";
                            collegeAffiliationType += "<td valign='top' style='font-size: 9px;' colspan='5'>From:" + affiliationNAACFromDate + "<br/>";
                            collegeAffiliationType += "TO:" + affiliationNAACToDate + "<br/>";
                            collegeAffiliationType += "Duration:" + duration + "</td>";
                            collegeAffiliationType += "</tr>";
                            collegeAffiliationType += "<br />";
                            break;

                    }
                    collegeAffiliationType += "<br />";
                }
                contents = contents.Replace("##COLLEGE_AFFILIATIONTYPES##", collegeAffiliationType);


                #endregion
                return contents;
            }
        }

        private string collegeInformation(int collegeId, string contents)
        {
            using (var db = new UAAAS.Models.uaaasDBContext())
            {
                CollegeInformation collegeInformation = new CollegeInformation();
                IQueryable<jntuh_college> jntuh_college = db.jntuh_college.Where(s => s.isActive == true).Select(e => e);
                #region from jntuh_college table
                jntuh_college collegeDetails = jntuh_college.Where(college => college.id == collegeId)
                                                               .FirstOrDefault();
                if (collegeDetails != null)
                {
                    collegeInformation.collegeName = collegeDetails.collegeName;
                    collegeInformation.collegeCode = collegeDetails.collegeCode;
                    collegeInformation.eamcetCode = collegeDetails.eamcetCode;
                    collegeInformation.icetCode = collegeDetails.icetCode;
                    collegeInformation.pgcetCode = collegeDetails.pgcetCode;
                }
                contents = contents.Replace("##AUDITSCHEDULECOLLEGENAME##", collegeInformation.collegeName);
                contents = contents.Replace("##COLLEGE_NAME##", collegeInformation.collegeName);
                contents = contents.Replace("##FORMER_COLLEGE_NAME##", collegeInformation.formerCollegeName);
                contents = contents.Replace("##COLLEGE_CODE##", collegeInformation.collegeCode);
                contents = contents.Replace("##EAMCET_CODE##", collegeInformation.eamcetCode);
                contents = contents.Replace("##ICET_CODE##", collegeInformation.icetCode);
                contents = contents.Replace("##PGECET_Code##", collegeInformation.pgcetCode);
                barcodetext += "College Code:" + collegeInformation.collegeCode + ";College Name:" + collegeInformation.collegeName;
                LatefeeQrCodetext += "College Code:" + collegeInformation.collegeCode + ";College Name:" + collegeInformation.collegeName;
                string strCollegeType = string.Empty;
                //List<jntuh_college_type> collegeType = db.jntuh_college_type.Where(s => s.isActive == true).ToList();
                //foreach (var item in collegeType)
                //{
                //    string YesOrNo = "no_b";
                //    int existCollegeTypeId = db.jntuh_college.Where(college => college.isActive == true && college.id == collegeId)
                //                                             .Select(college => college.collegeTypeID)
                //                                             .FirstOrDefault();
                //    if (item.id == existCollegeTypeId)
                //    {
                //        YesOrNo = "yes_b";

                //        strCollegeType += string.Format("<img alt='' src='" + serverURL + "/Content/Images/checkbox_{0}.png' height='10' />&nbsp; {1} &nbsp; &nbsp; &nbsp;", YesOrNo, item.collegeType);
                //    }
                //}
                var CollegeType = jntuh_college.Where(college => college.isActive == true && college.id == collegeId)
                                        .Select(c => new { c.jntuh_college_type.collegeType, c.isActive }).Where(s => s.isActive == true).ToList();
                strCollegeType += CollegeType.FirstOrDefault().collegeType;
                contents = contents.Replace("##COLLEGE_TYPE##", strCollegeType);

                string strCollegeStatus = string.Empty;
                //List<jntuh_college_status> jntuh_college_status = db.jntuh_college_status.Where(s => s.isActive == true).ToList();

                var CollegeStatus = jntuh_college.Where(college => college.isActive == true && college.id == collegeId)
                                        .Select(c => new { c.jntuh_college_status.collegeStatus, c.isActive }).Where(s => s.isActive == true).ToList();
                strCollegeStatus += CollegeStatus.FirstOrDefault().collegeStatus;
                //foreach (var item in jntuh_college_status)
                //{
                //    string YesOrNo = "no_b";
                //    int existCollegeStatusId = db.jntuh_college.Where(college => college.isActive == true && college.id == collegeId)
                //                                             .Select(college => college.collegeStatusID)
                //                                             .FirstOrDefault();
                //    if (item.id == existCollegeStatusId)
                //        YesOrNo = "yes_b";

                //    strCollegeStatus += string.Format("<img alt='' src='" + serverURL + "/Content/Images/checkbox_{0}.png' height='10' />&nbsp; {1} &nbsp; &nbsp; &nbsp;", YesOrNo, item.collegeStatus);
                //}
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



                #region from jntuh_college_degree table

                string strCollegeDegree = string.Empty;
                strCollegeDegree += "<table border='0' cellspacing='0' cellpadding='0'><tbody><tr>";

                var collegeDegree = db.jntuh_college_degree.Where(degree => degree.isActive && degree.collegeId == collegeId)
                    .Select(g => new { g.jntuh_degree.degree, g.isActive }).ToList();


                int count = 0;
                foreach (var item in collegeDegree)
                {

                    strCollegeDegree += "<td width='10%'>" + item.degree + "</td>";

                    count++;
                    if (count % 5 == 0)
                    {
                        strCollegeDegree += "</tr>";
                    }
                }
                if (count % 5 != 0)
                {
                    strCollegeDegree += "</tr>";
                }
                //List<jntuh_college_degree> collegeDegrees = db.jntuh_college_degree.Where(degree => degree.isActive == true && degree.collegeId == collegeId).ToList();
                //foreach (var degrees in collegeDegrees)
                //{
                //    strCollegeDegree = strCollegeDegree.Replace("##COLLEGEDEGREEIMAGE" + degrees.degreeId + "##", string.Format("<img alt='' src='" + serverURL + "/Content/Images/checkbox_{0}.png' height='10' />", "yes_b"));
                //}
                //foreach (var item in collegeDegree)
                //{
                //    strCollegeDegree = strCollegeDegree.Replace("##COLLEGEDEGREEIMAGE" + item.id + "##", string.Format("<img alt='' src='" + serverURL + "/Content/Images/checkbox_{0}.png' height='10' />", "no_b"));
                //}
                strCollegeDegree += "</tbody></table>";
                contents = contents.Replace("##COLLEGE_DEGREE##", strCollegeDegree);
                #endregion
                // List<IElement> parsedHtmlElements = HTMLWorker.ParseToList(new StringReader(contents), null);
                return contents;
            }
        }

        private string PaymentBillDetails(int collegeId, string contents)
        {
            string Paymentbilldetails = string.Empty;
            string strPaymentDate = string.Empty;
            Paymentbilldetails = "";

            List<IElement> parsedHtmlElements3 = HTMLWorker.ParseToList(new StringReader(contents), null);

            string collegecode = db.jntuh_college.Where(e => e.id == collegeId).Select(e => e.collegeCode).FirstOrDefault();
            var actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(q => q.id).FirstOrDefault();
            int? PresentYear = actualYear + 1;
            List<jntuh_paymentresponse> payment = db.jntuh_paymentresponse.Where(a => a.AcademicYearId == PresentYear && a.CollegeId == collegecode && a.AuthStatus == "0300" && a.PaymentTypeID == 7).ToList();


            if (payment != null && payment.Count() != 0)
            {
                Paymentbilldetails += "<p align='left'><strong><u>Payment Details</u></strong></p><br />";
                Paymentbilldetails += "<table width='100%' border='1' cellspacing='0' cellpadding='5' style='font-size: 9px;'>";
                Paymentbilldetails += "<tbody>";
                foreach (var item in payment.Take(1))
                {
                    if (item.TxnDate != null)
                    {
                        strPaymentDate = item.TxnDate.ToString() == null ? string.Empty : UAAAS.Models.Utilities.MMDDYY2DDMMYY(item.TxnDate.ToString()).ToString();
                    }
                    Paymentbilldetails += "<tr>";
                    Paymentbilldetails += "<td width='35%'><p align='left'>Customer Id</p></td>";
                    Paymentbilldetails += "<td width='5%'><p align='center'>:</p></td>";
                    Paymentbilldetails += "<td width='60%'><p align='left'>" + item.CustomerID + "</p></td>";
                    Paymentbilldetails += "</tr>";
                    Paymentbilldetails += "<tr>";
                    Paymentbilldetails += "<td width='35%'><p align='left'>Transaction Ref.no</p></td>";
                    Paymentbilldetails += "<td width='5%'><p align='center'>:</p></td>";
                    Paymentbilldetails += "<td width='60%'><p align='left'>" + item.TxnReferenceNo + "</p></td>";
                    Paymentbilldetails += "</tr>";
                    Paymentbilldetails += "<tr>";
                    Paymentbilldetails += "<td width='35%'><p align='left'>Bank Refno</p></td>";
                    Paymentbilldetails += "<td width='5%'><p align='center'>:</p></td>";
                    Paymentbilldetails += "<td width='60%'><p align='left'>" + item.BankReferenceNo + "</p></td>";
                    Paymentbilldetails += "</tr>";
                    Paymentbilldetails += "<tr>";
                    Paymentbilldetails += "<td width='35%'><p align='left'>Transaction Amount</p></td>";
                    Paymentbilldetails += "<td width='5%'><p align='center'>:</p></td>";
                    Paymentbilldetails += "<td width='60%'><p align='left'>" + item.TxnAmount + "</p></td>";
                    Paymentbilldetails += "</tr>";
                    Paymentbilldetails += "<tr>";
                    Paymentbilldetails += "<td width='35%'><p align='left'>Transaction Date</p></td>";
                    Paymentbilldetails += "<td width='5%'><p align='center'>:</p></td>";
                    Paymentbilldetails += "<td width='60%'><p align='left'>" + strPaymentDate + "</p></td>";
                    Paymentbilldetails += "</tr>";
                    Paymentbilldetails += "<tr>";
                    Paymentbilldetails += "<td width='35%'><p align='left'>Payment Description</p></td>";
                    Paymentbilldetails += "<td width='5%'><p align='center'>:</p></td>";
                    Paymentbilldetails += "<td width='60%'><p align='left'>" + item.ErrorDescription + "</p></td>";
                    Paymentbilldetails += "</tr>";
                    barcodetext += ";Payment Date:" + strPaymentDate + ";Customer Id:" + item.CustomerID;
                }
                Paymentbilldetails += "</tbody></table>";
            }
            else
            {
                Paymentbilldetails += "<table border='1' cellspacing='0' cellpadding='5' style='font-size: 9px;'>";
                Paymentbilldetails += "<tbody>";
                Paymentbilldetails += "<tr>";
                Paymentbilldetails += "<td colspan='4'><p align='left'>Payment Date</p></td>";
                Paymentbilldetails += "<td colspan='3'><p align='left'>Reference Number</p></td>";
                Paymentbilldetails += "<td colspan='3'><p align='left'>Transaction Amount</p></td>";
                Paymentbilldetails += "</tr>";
                Paymentbilldetails += "<tr>";
                Paymentbilldetails += "<td colspan='10'><p align='center'>NIL</p></td>";
                Paymentbilldetails += "</tr>";
                Paymentbilldetails += "</tbody></table>";
            }

            contents = contents.Replace("##PaymentDetails1##", Paymentbilldetails);
            List<IElement> parsedHtmlElements = HTMLWorker.ParseToList(new StringReader(contents), null);
            var Paymentdate = payment.Select(e => e.TxnDate).FirstOrDefault();
            string paymentdatecurrentformat = string.Empty;
            if (Paymentdate != null && payment.Count() != 0)
            {
                paymentdatecurrentformat = UAAAS.Models.Utilities.MMDDYY2DDMMYY(Paymentdate.ToString());
            }
            else
            {
                paymentdatecurrentformat = string.Empty;
            }
            contents = contents.Replace("##PAYMENTDATE##", paymentdatecurrentformat);
            // List<IElement> parsedHtmlElements2 = HTMLWorker.ParseToList(new StringReader(contents), null);
            // contents = contents.Replace("##PAYMENTDATE##", "");
            #region application submission Date

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

            #endregion

            barcodetext += ";Online Application Date:" + datetime;
            contents = contents.Replace("##SUBMITTEDDATE##", datetime);
            return contents;
        }

        public string barcodegenerator(int collegeId, string contents)
        {

            string str = string.Empty;
            string strDataModifications = string.Empty;
            string strimagedetails = string.Empty;
            string strimagebarcodedetails = string.Empty;
            var collegeCode = db.jntuh_college.Where(college => college.isActive == true && college.id == collegeId).Select(college => college.collegeCode).FirstOrDefault();
            var challanaNO = db.jntuh_paymentresponse.Where(college => college.CollegeId == collegeCode && college.AuthStatus == "0300").Select(college => college.CustomerID).FirstOrDefault();

            if (challanaNO != null)
            {

                /////QR Code GEneration Code
                Gma.QrCodeNet.Encoding.QrEncoder qrEncoder = new Gma.QrCodeNet.Encoding.QrEncoder(Gma.QrCodeNet.Encoding.ErrorCorrectionLevel.H);
                Gma.QrCodeNet.Encoding.QrCode qrCode = new Gma.QrCodeNet.Encoding.QrCode();
                qrEncoder.TryEncode(barcodetext, out qrCode);
                Gma.QrCodeNet.Encoding.Windows.Render.GraphicsRenderer renderer = new Gma.QrCodeNet.Encoding.Windows.Render.GraphicsRenderer(new Gma.QrCodeNet.Encoding.Windows.Render.FixedModuleSize(4, Gma.QrCodeNet.Encoding.Windows.Render.QuietZoneModules.Four), System.Drawing.Brushes.Black, System.Drawing.Brushes.White);

                Stream memoryStream = new MemoryStream();
                renderer.WriteToStream(qrCode.Matrix, ImageFormat.Png, memoryStream);
                // very important to reset memory stream to a starting position, otherwise you would get 0 bytes returned
                memoryStream.Position = 0;

                var resultStream = new FileStreamResult(memoryStream, "image/png");
                resultStream.FileDownloadName = String.Format("{0}.png", collegeCode);


                System.Drawing.Image v = System.Drawing.Image.FromStream(memoryStream);
                if (!Directory.Exists(Server.MapPath("~/Content/Upload/EquipmentsPhotos")))
                {
                    Directory.CreateDirectory(Server.MapPath("~/Content/Upload/EquipmentsPhotos"));
                }
                var ext = resultStream.ContentType;
                var Filename = resultStream.FileDownloadName;

                System.Drawing.Image img = v;
                img.Save(string.Format("{0}/{1}", Server.MapPath("~/Content/Upload/EquipmentsPhotos"), Filename));

                if (Filename != null)
                {
                    strimagedetails = "/Content/Upload/EquipmentsPhotos/" + Filename;
                }
                else
                {
                    strimagedetails = string.Empty;
                }
                string path = @"~" + strimagedetails;
                path = System.Web.HttpContext.Current.Server.MapPath(path);


                if (challanaNO != null)
                {
                    strimagebarcodedetails = "/Content/Upload/EquipmentsPhotos/" + challanaNO + ".png";
                }
                else
                {
                    strimagebarcodedetails = string.Empty;
                }
                string path1 = @"~" + strimagebarcodedetails;
                path1 = System.Web.HttpContext.Current.Server.MapPath(path1);

                strDataModifications += "<table><tr>";

                if (System.IO.File.Exists(path))
                {
                    strDataModifications += "<td><img src='" + path + "' align='left'  width='100' height='100' /></td>";
                    // strDataModifications += "<td><img src=" + serverURL + "" + strimagedetails + " align='left'  width='100' height='100' /></td>";
                }
                else
                {
                    strDataModifications += "<td width='100' style='vertical-align:top' align='left' colspan='4'><p align='center'></p></td>";
                }
                strDataModifications += "</tr></table>";
            }

            contents = contents.Replace("##QRcode##", strDataModifications);
            return contents;
        }

        public string EducationalSocietyDetails(int collegeId, string contents)
        {
            using (var db = new UAAAS.Models.uaaasDBContext())
            {
                jntuh_college_establishment collegeestablishment = db.jntuh_college_establishment.Where(e => e.collegeId == collegeId).Select(e => e).FirstOrDefault();
                if (collegeestablishment != null)
                {
                    jntuh_address address = db.jntuh_address.FirstOrDefault(a => a.collegeId == collegeId && a.addressTye == "SOCIETY");
                    jntuh_state state = new jntuh_state();
                    jntuh_district district = new jntuh_district();
                    if (address != null)
                    {
                        state = db.jntuh_state.FirstOrDefault(s => s.id == address.stateId && s.isActive);
                        district = db.jntuh_district.FirstOrDefault(d => d.id == address.districtId && d.isActive);
                    }
                    contents = contents.Replace("##SocietyYearofEstablishment##", collegeestablishment.societyEstablishmentYear.ToString());
                    contents = contents.Replace("##SocietyRegisteredNumber##", collegeestablishment.societyRegisterNumber);
                    contents = contents.Replace("##SocietyName##", collegeestablishment.societyName);
                    contents = contents.Replace("##SocietyOldName##", collegeestablishment.oldsocityname);
                    contents = contents.Replace("##SocietyAddress##", address.address);
                    contents = contents.Replace("##SocietyCity/Town##", address.townOrCity);
                    contents = contents.Replace("##SocietyMandal##", address.mandal);
                    contents = contents.Replace("##SocietyDistrict##", district.districtName);
                    contents = contents.Replace("##SocietyState##", state.stateName);
                    contents = contents.Replace("##SocietyPincode##", address.pincode.ToString());
                    contents = contents.Replace("##SocietyFax##", address.fax.ToString());
                    contents = contents.Replace("##SocietyLandline##", address.landline);
                    contents = contents.Replace("##SocietyMobile##", address.mobile);
                    contents = contents.Replace("##SocietyEmail##", address.email);
                    contents = contents.Replace("##SocietyWebsite##", address.website);

                    string firstApprovalDateByAICTE = collegeestablishment.firstApprovalDateByAICTE.ToString() == null ? string.Empty : UAAAS.Models.Utilities.MMDDYY2DDMMYY(collegeestablishment.firstApprovalDateByAICTE.ToString()).ToString();
                    string firstAffiliationDateByJNTU = collegeestablishment.firstAffiliationDateByJNTU.ToString() == null ? string.Empty : UAAAS.Models.Utilities.MMDDYY2DDMMYY(collegeestablishment.firstAffiliationDateByJNTU.ToString()).ToString();
                    contents = contents.Replace("##YearofEstablishmentoftheInstitution##", collegeestablishment.instituteEstablishedYear.ToString());
                    contents = contents.Replace("##DateonwhichfirstapprovalwasaccordedbytheAICTE##", firstApprovalDateByAICTE);
                    contents = contents.Replace("##DateonwhichfirstaffiliationwasaccordedbytheJNTU##", firstAffiliationDateByJNTU);
                    contents = contents.Replace("##YearofcommencementofFirstBatch##", collegeestablishment.firstBatchCommencementYear.ToString());
                }
                else
                {
                    contents = contents.Replace("##SocietyYearofEstablishment##", string.Empty);
                    contents = contents.Replace("##SocietyRegisteredNumber##", string.Empty);
                    contents = contents.Replace("##SocietyName##", string.Empty);
                    contents = contents.Replace("##SocietyAddress##", string.Empty);
                    contents = contents.Replace("##SocietyCity/Town##", string.Empty);
                    contents = contents.Replace("##SocietyMandal##", string.Empty);
                    contents = contents.Replace("##SocietyDistrict##", string.Empty);
                    contents = contents.Replace("##SocietyState##", string.Empty);
                    contents = contents.Replace("##SocietyPincode##", string.Empty);
                    contents = contents.Replace("##SocietyFax##", string.Empty);
                    contents = contents.Replace("##SocietyLandline##", string.Empty);
                    contents = contents.Replace("##SocietyMobile##", string.Empty);
                    contents = contents.Replace("##SocietyEmail##", string.Empty);
                    contents = contents.Replace("##SocietyWebsite##", string.Empty);
                    contents = contents.Replace("##YearofEstablishmentoftheInstitution##", string.Empty);
                    contents = contents.Replace("##DateonwhichfirstapprovalwasaccordedbytheAICTE##", string.Empty);
                    contents = contents.Replace("##DateonwhichfirstaffiliationwasaccordedbytheJNTU##", string.Empty);
                    contents = contents.Replace("##YearofcommencementofFirstBatch##", string.Empty);
                }
                //List<IElement> parsedHtmlElements = HTMLWorker.ParseToList(new StringReader(contents), null);
                return contents;
            }
        }

        public string PrincipalDirectorDetails(int collegeId, string contents)
        {
            using (var db = new UAAAS.Models.uaaasDBContext())
            {
                int directorID = db.jntuh_college_principal_director.Where(e => e.collegeId == collegeId && e.type.Equals("DIRECTOR")).Select(e => e.id).FirstOrDefault();

                jntuh_college_principal_director director = db.jntuh_college_principal_director.Find(directorID);
                string strcheckList = "<img alt='' src='~/Content/Images/checkbox_no_b.png' height='8' />";
                // string strcheckList = "<img alt='' src='" + serverURL + "/Content/Images/checkbox_no_b.png' height='8' />";
                string strPrincipal = string.Empty;
                ////Principal Details
                var regNo = db.jntuh_college_principal_registered.Where(r => r.collegeId == collegeId).Select(r => r.RegistrationNumber).FirstOrDefault();
                if (!string.IsNullOrEmpty(regNo))
                {
                    var PrincipalDetails = db.jntuh_registered_faculty.FirstOrDefault(r => r.RegistrationNumber == regNo);

                    if (PrincipalDetails != null)
                    {
                        var education = db.jntuh_registered_faculty_education.Where(e => e.facultyId == PrincipalDetails.id).OrderByDescending(e => e.id).Select(e => e).FirstOrDefault();
                        var Prinicipal_BAS_DAYS_Count = db.jntuh_college_basreport.Where(s => s.RegistrationNumber == PrincipalDetails.RegistrationNumber).Select(s => s).ToList();

                        int? WorkingDays = Prinicipal_BAS_DAYS_Count.Select(f => f.totalworkingDays).Sum();
                        int? PresentDays = Prinicipal_BAS_DAYS_Count.Select(f => f.NoofPresentDays).Sum();
                        int? HoliDays = Prinicipal_BAS_DAYS_Count.Select(f => f.NoofHolidays).Sum();


                        strPrincipal += "<p><strong><u>Details of Principal</u></strong></p><br />";
                        strPrincipal += "<table border='1' cellspacing='0' cellpadding='5'>";
                        strPrincipal += "<tbody>";
                        strPrincipal += "<tr>";
                        strPrincipal += "<td valign='top' colspan='4'>Registration Number</td>";
                        strPrincipal += "<td valign='top' colspan='1' align='center'>:</td>";
                        strPrincipal += "<td colspan='15' valign='top'>" + PrincipalDetails.RegistrationNumber + "</td>";
                        // strPrincipal += "<td valign='top' colspan='2' align='center'>BAS</td>";
                        //  strPrincipal += "<td valign='top' colspan='8' align='center'><b>Yes</b> " + strcheckList + " &nbsp;&nbsp;<b>&nbsp;&nbsp;No</b> " + strcheckList + " &nbsp;&nbsp;<b>&nbsp;&nbsp;Error</b> " + strcheckList + "</td>";
                        //strPrincipal += "<td  valign='top' colspan='4'>PAN Number</td><td align='center'>:</td><td valign='top' colspan='5'>" + PrincipalDetails.PANNumber + "</td>";
                        //strPrincipal += "<td  valign='top' colspan='4'>P.W.D/T.W.D</td><td align='center'>:</td><td valign='top' colspan='5'>" + PresentDays + "/" + WorkingDays + "</td>";
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
                        strPrincipal += "<td valign='top' colspan='4'></td>";
                        strPrincipal += "<td valign='top' colspan='1' align='center'></td>";
                        if (education != null)
                        {
                            if (education.courseStudied != null)
                            {
                                //strPrincipal += "<td valign='top' colspan='5'>" + education.courseStudied + "</td>";
                                strPrincipal += "<td valign='top' colspan='5'></td>";
                            }
                            else
                            {
                                strPrincipal += "<td valign='top' colspan='5'></td>";
                            }
                        }
                        else
                        {
                            strPrincipal += "<td valign='top' colspan='5'></td>";
                        }
                        strPrincipal += "</tr>";

                        strPrincipal += "<tr>";
                        strPrincipal += "<td valign='top' colspan='4'>Department </td>";
                        strPrincipal += "<td valign='top' colspan='1' align='center'>: </td>";
                        if (education != null)
                        {
                            if (education.specialization != null)
                            {
                                strPrincipal += "<td valign='top' colspan='5'>" + education.specialization + "</td>";
                            }
                            else
                            {
                                strPrincipal += "<td valign='top' colspan='5'></td>";
                            }
                        }
                        else
                        {
                            strPrincipal += "<td valign='top' colspan='5'></td>";
                        }
                        strPrincipal += "<td valign='top' colspan='4' style='font-size: 8px;'>Date of Appointment </td>";
                        strPrincipal += "<td valign='top' colspan='1' align='center'>: </td>";
                        string AppDate = PrincipalDetails.DateOfAppointment.ToString() == null ? string.Empty : UAAAS.Models.Utilities.MMDDYY2DDMMYY(PrincipalDetails.DateOfAppointment.ToString()).ToString();
                        strPrincipal += "<td valign='top' colspan='5'>" + AppDate + "</td>";
                        strPrincipal += "</tr>";

                        strPrincipal += "<tr>";
                        string BirthDate = PrincipalDetails.DateOfBirth.ToString() == null ? string.Empty : UAAAS.Models.Utilities.MMDDYY2DDMMYY(PrincipalDetails.DateOfBirth.ToString()).ToString();
                        strPrincipal += "<td valign='top' colspan='4'>Date of Birth</td>";
                        strPrincipal += "<td valign='top' colspan='1' align='center'>:</td>";
                        strPrincipal += "<td valign='top' colspan='5'>" + BirthDate + "</td>";

                        strPrincipal += "<td valign='top' colspan='4'>Fax</td>";
                        strPrincipal += "<td valign='top' colspan='1' align='center'>:</td>";
                        strPrincipal += "<td valign='top' colspan='5'>" + db.jntuh_address.Where(c => c.collegeId == collegeId).Select(c => c.fax).FirstOrDefault() + "</td>";
                        strPrincipal += "</tr>";

                        strPrincipal += "<tr>";
                        strPrincipal += "<td valign='top' colspan='4'>Landline</td>";
                        strPrincipal += "<td valign='top' colspan='1' align='center'>:</td>";
                        strPrincipal += "<td valign='top' colspan='5'>" + db.jntuh_address.Where(c => c.collegeId == collegeId).Select(c => c.landline).FirstOrDefault() + "</td>";

                        strPrincipal += "<td valign='top' colspan='4'>Mobile</td>";
                        strPrincipal += "<td valign='top' colspan='1' align='center'>:</td>";
                        strPrincipal += "<td valign='top' colspan='5'>" + PrincipalDetails.Mobile + "</td>";
                        strPrincipal += "</tr>";

                        strPrincipal += "<tr>";
                        strPrincipal += "<td valign='top' colspan='4'>Email</td>";
                        strPrincipal += "<td valign='top' colspan='1' align='center'>:</td>";
                        strPrincipal += "<td colspan='15' valign='top'>" + PrincipalDetails.Email + "</td>";
                        strPrincipal += "</tr>";

                        strPrincipal += "<tr>";
                        strPrincipal += "<td valign='top' colspan='4'> </td>";
                        strPrincipal += "<td valign='top' colspan='1' align='center'>:</td>";
                        strPrincipal += "<td colspan='5' valign='top'> </td>";
                        strPrincipal += "<td valign='top' colspan='4'>Photo</td>";
                        strPrincipal += "<td valign='top' colspan='1' align='center'>:</td>";
                        if (!string.IsNullOrEmpty(PrincipalDetails.Photo))
                        {
                            string Parsing = string.Empty;
                            string strPrincipalPhoto = string.Empty;
                            string path = "http://jntuhaac.in/Content/Upload/Faculty/Photos/" + PrincipalDetails.Photo;
                            //  string path = @"~/Content/Upload/Faculty/Photos/" + PrincipalDetails.Photo;
                            //  path = System.Web.HttpContext.Current.Server.MapPath(path);

                            #region With-Out Html Parsing
                            try
                            {
                                if (!string.IsNullOrEmpty(path))
                                // if (System.IO.File.Exists(path))
                                {
                                    Parsing += "<p align='center'><img src='" + HtmlEncoder.Encode(path.Trim()) + "' align='center'  width='40' height='45' /></p>";
                                    var ParseEliments = HTMLWorker.ParseToList(new StringReader(Parsing), null);

                                    if (path.Contains("."))
                                    {
                                        strPrincipalPhoto = "<img alt=''src='" + HtmlEncoder.Encode(path.Trim()) + "' align='center'  height='45' />";
                                        // strPrincipalPhoto = "<img alt='' src='" + serverURL + "/Content/Upload/Faculty/Photos/" + PrincipalDetails.Photo + "'" + " align='center'  height='50' />";
                                        strPrincipal += "<td colspan='5' valign='top' >" + strPrincipalPhoto + "</td>";
                                    }
                                    else
                                        strPrincipal += "<td colspan='5' valign='top' style='height: 50px;'>&nbsp;</td>";
                                }
                                else
                                {
                                    if (test415PDF.Equals("YES"))
                                    {
                                        strPrincipal += "<td colspan='5' valign='top'>&nbsp;</td>";
                                    }
                                    else
                                    {
                                        strPrincipal += "<td colspan='5' valign='top' style='height: 50px;'>&nbsp;</td>";
                                    }
                                }

                            }
                            catch (Exception ex)
                            {
                                strPrincipal += "<td colspan='5' valign='top' style='height: 50px;'>&nbsp;</td>";
                                //continue;
                            }
                            #endregion

                        }
                        else
                        {
                            strPrincipal += "<td colspan='5' valign='top' style='height: 50px;'>&nbsp;</td>";
                        }
                        strPrincipal += "</tr>";
                        strPrincipal += "</tbody>";
                        strPrincipal += "</table>";
                    }
                    else
                    {
                        strPrincipal += "<p><strong><u>Details of Principal:</u></strong> (PRINCIPAL DETAILS ARE NOT UPLOADED)</p><br />";

                        #region old code
                        //strPrincipal += "<table border='1' cellspacing='0' cellpadding='5'>";
                        //strPrincipal += "<tbody>";
                        //strPrincipal += "<tr>";
                        //strPrincipal += "<td valign='top' colspan='4'>Registration Number</td>";
                        //strPrincipal += "<td valign='top' colspan='1' align='center'>:</td>";
                        //strPrincipal += "<td colspan='15' valign='top'></td>";
                        //strPrincipal += "</tr>";

                        //strPrincipal += "<tr>";
                        //strPrincipal += "<td valign='top' colspan='4'>First Name</td>";
                        //strPrincipal += "<td valign='top' colspan='1' align='center'>:</td>";
                        //strPrincipal += "<td valign='top' colspan='5'></td>";
                        //strPrincipal += "<td valign='top' colspan='4'>Last Name</td>";
                        //strPrincipal += "<td valign='top' colspan='1' align='center'>:</td>";
                        //strPrincipal += "<td valign='top' colspan='5'></td>";
                        //strPrincipal += "</tr>";
                        //strPrincipal += "<tr>";
                        //strPrincipal += "<td valign='top' colspan='4'>Surname</td>";
                        //strPrincipal += "<td valign='top' colspan='1' align='center'>:</td>";
                        //strPrincipal += "<td valign='top' colspan='5'></td>";
                        //strPrincipal += "<td valign='top' colspan='4'>Qualification</td>";
                        //strPrincipal += "<td valign='top' colspan='1' align='center'>:</td>";
                        //strPrincipal += "<td valign='top' colspan='5'></td>";
                        //strPrincipal += "</tr>";

                        //strPrincipal += "<tr>";
                        //strPrincipal += "<td valign='top' colspan='4'>Department </td>";
                        //strPrincipal += "<td valign='top' colspan='1' align='center'>: </td>";
                        //strPrincipal += "<td valign='top' colspan='5'></td>";
                        //strPrincipal += "<td valign='top' colspan='4' style='font-size: 8px;'>Date of Appointment </td>";
                        //strPrincipal += "<td valign='top' colspan='1' align='center'>: </td>";
                        //strPrincipal += "<td valign='top' colspan='5'></td>";
                        //strPrincipal += "</tr>";

                        //strPrincipal += "<tr>";
                        //strPrincipal += "<td valign='top' colspan='4'>Date of Birth</td>";
                        //strPrincipal += "<td valign='top' colspan='1' align='center'>:</td>";
                        //strPrincipal += "<td valign='top' colspan='5'></td>";

                        //strPrincipal += "<td valign='top' colspan='4'>Mobile (+91)</td>";
                        //strPrincipal += "<td valign='top' colspan='1' align='center'>:</td>";
                        //strPrincipal += "<td valign='top' colspan='5'></td>";
                        //strPrincipal += "</tr>";

                        //strPrincipal += "<tr>";
                        //strPrincipal += "<td valign='top' colspan='4'>Email</td>";
                        //strPrincipal += "<td valign='top' colspan='1' align='center'>:</td>";
                        //strPrincipal += "<td colspan='15' valign='top'></td>";
                        //strPrincipal += "</tr>";


                        //strPrincipal += "<tr>";
                        //strPrincipal += "<td valign='top' colspan='4'>Ratified</td>";
                        //strPrincipal += "<td valign='top' colspan='1' align='center'>:</td>";
                        //strPrincipal += "<td colspan='5' valign='top'></td>";
                        #endregion

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
                        strPrincipal += "<td valign='top' colspan='4'></td>";
                        strPrincipal += "<td valign='top' colspan='1' align='center'></td>";
                        strPrincipal += "<td valign='top' colspan='5'></td>";
                        strPrincipal += "</tr>";

                        strPrincipal += "<tr>";
                        strPrincipal += "<td valign='top' colspan='4'>Department </td>";
                        strPrincipal += "<td valign='top' colspan='1' align='center'>: </td>";
                        strPrincipal += "<td valign='top' colspan='5'></td>";
                        strPrincipal += "<td valign='top' colspan='4' style='font-size: 8px;'>Date of Appointment </td>";
                        strPrincipal += "<td valign='top' colspan='1' align='center'>: </td>";
                        strPrincipal += "<td valign='top' colspan='5'></td>";
                        strPrincipal += "</tr>";

                        strPrincipal += "<tr>";
                        strPrincipal += "<td valign='top' colspan='4'>Date of Birth</td>";
                        strPrincipal += "<td valign='top' colspan='1' align='center'>:</td>";
                        strPrincipal += "<td valign='top' colspan='5'></td>";

                        strPrincipal += "<td valign='top' colspan='4'>Fax</td>";
                        strPrincipal += "<td valign='top' colspan='1' align='center'>:</td>";
                        strPrincipal += "<td valign='top' colspan='5'></td>";
                        strPrincipal += "</tr>";

                        strPrincipal += "<tr>";
                        strPrincipal += "<td valign='top' colspan='4'>Landline</td>";
                        strPrincipal += "<td valign='top' colspan='1' align='center'>:</td>";
                        strPrincipal += "<td valign='top' colspan='5'></td>";

                        strPrincipal += "<td valign='top' colspan='4'>Mobile</td>";
                        strPrincipal += "<td valign='top' colspan='1' align='center'>:</td>";
                        strPrincipal += "<td valign='top' colspan='5'></td>";
                        strPrincipal += "</tr>";

                        strPrincipal += "<tr>";
                        strPrincipal += "<td valign='top' colspan='4'>Email</td>";
                        strPrincipal += "<td valign='top' colspan='1' align='center'>:</td>";
                        strPrincipal += "<td colspan='15' valign='top'></td>";
                        strPrincipal += "</tr>";

                        strPrincipal += "<tr>";
                        strPrincipal += "<td valign='top' colspan='4'> </td>";
                        strPrincipal += "<td valign='top' colspan='1' align='center'>:</td>";
                        strPrincipal += "<td colspan='5' valign='top'> </td>";

                        strPrincipal += "<td valign='top' colspan='4'>Photo</td>";
                        strPrincipal += "<td valign='top' colspan='1' align='center'>:</td>";
                        strPrincipal += "<td colspan='5' valign='top'></td>";
                        strPrincipal += "</tr>";

                        strPrincipal += "</tbody>";
                        strPrincipal += "</table>";
                    }
                }
                else
                {
                    strPrincipal += "<p><strong><u>Details of Principal:</u></strong> (PRINCIPAL DETAILS ARE NOT UPLOADED)</p><br />";

                    #region old code
                    //strPrincipal += "<table border='1' cellspacing='0' cellpadding='5'>";
                    //strPrincipal += "<tbody>";
                    //strPrincipal += "<tr>";
                    //strPrincipal += "<td valign='top' colspan='4'>Registration Number</td>";
                    //strPrincipal += "<td valign='top' colspan='1' align='center'>:</td>";
                    //strPrincipal += "<td colspan='15' valign='top'></td>";
                    //strPrincipal += "</tr>";

                    //strPrincipal += "<tr>";
                    //strPrincipal += "<td valign='top' colspan='4'>First Name</td>";
                    //strPrincipal += "<td valign='top' colspan='1' align='center'>:</td>";
                    //strPrincipal += "<td valign='top' colspan='5'></td>";
                    //strPrincipal += "<td valign='top' colspan='4'>Last Name</td>";
                    //strPrincipal += "<td valign='top' colspan='1' align='center'>:</td>";
                    //strPrincipal += "<td valign='top' colspan='5'></td>";
                    //strPrincipal += "</tr>";
                    //strPrincipal += "<tr>";
                    //strPrincipal += "<td valign='top' colspan='4'>Surname</td>";
                    //strPrincipal += "<td valign='top' colspan='1' align='center'>:</td>";
                    //strPrincipal += "<td valign='top' colspan='5'></td>";
                    //strPrincipal += "<td valign='top' colspan='4'>Qualification</td>";
                    //strPrincipal += "<td valign='top' colspan='1' align='center'>:</td>";
                    //strPrincipal += "<td valign='top' colspan='5'></td>";
                    //strPrincipal += "</tr>";

                    //strPrincipal += "<tr>";
                    //strPrincipal += "<td valign='top' colspan='4'>Department </td>";
                    //strPrincipal += "<td valign='top' colspan='1' align='center'>: </td>";
                    //strPrincipal += "<td valign='top' colspan='5'></td>";
                    //strPrincipal += "<td valign='top' colspan='4' style='font-size: 8px;'>Date of Appointment </td>";
                    //strPrincipal += "<td valign='top' colspan='1' align='center'>: </td>";
                    //strPrincipal += "<td valign='top' colspan='5'></td>";
                    //strPrincipal += "</tr>";

                    //strPrincipal += "<tr>";
                    //strPrincipal += "<td valign='top' colspan='4'>Date of Birth</td>";
                    //strPrincipal += "<td valign='top' colspan='1' align='center'>:</td>";
                    //strPrincipal += "<td valign='top' colspan='5'></td>";

                    //strPrincipal += "<td valign='top' colspan='4'>Mobile (+91)</td>";
                    //strPrincipal += "<td valign='top' colspan='1' align='center'>:</td>";
                    //strPrincipal += "<td valign='top' colspan='5'></td>";
                    //strPrincipal += "</tr>";

                    //strPrincipal += "<tr>";
                    //strPrincipal += "<td valign='top' colspan='4'>Email</td>";
                    //strPrincipal += "<td valign='top' colspan='1' align='center'>:</td>";
                    //strPrincipal += "<td colspan='15' valign='top'></td>";
                    //strPrincipal += "</tr>";


                    //strPrincipal += "<tr>";
                    //strPrincipal += "<td valign='top' colspan='4'>Ratified</td>";
                    //strPrincipal += "<td valign='top' colspan='1' align='center'>:</td>";
                    //strPrincipal += "<td colspan='5' valign='top'></td>";
                    #endregion

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
                    strPrincipal += "<td valign='top' colspan='4'></td>";
                    strPrincipal += "<td valign='top' colspan='1' align='center'></td>";
                    strPrincipal += "<td valign='top' colspan='5'></td>";
                    strPrincipal += "</tr>";

                    strPrincipal += "<tr>";
                    strPrincipal += "<td valign='top' colspan='4'>Department </td>";
                    strPrincipal += "<td valign='top' colspan='1' align='center'>: </td>";
                    strPrincipal += "<td valign='top' colspan='5'></td>";
                    strPrincipal += "<td valign='top' colspan='4' style='font-size: 8px;'>Date of Appointment </td>";
                    strPrincipal += "<td valign='top' colspan='1' align='center'>: </td>";
                    strPrincipal += "<td valign='top' colspan='5'></td>";
                    strPrincipal += "</tr>";

                    strPrincipal += "<tr>";
                    strPrincipal += "<td valign='top' colspan='4'>Date of Birth</td>";
                    strPrincipal += "<td valign='top' colspan='1' align='center'>:</td>";
                    strPrincipal += "<td valign='top' colspan='5'></td>";

                    strPrincipal += "<td valign='top' colspan='4'>Fax</td>";
                    strPrincipal += "<td valign='top' colspan='1' align='center'>:</td>";
                    strPrincipal += "<td valign='top' colspan='5'></td>";
                    strPrincipal += "</tr>";

                    strPrincipal += "<tr>";
                    strPrincipal += "<td valign='top' colspan='4'>Landline</td>";
                    strPrincipal += "<td valign='top' colspan='1' align='center'>:</td>";
                    strPrincipal += "<td valign='top' colspan='5'></td>";

                    strPrincipal += "<td valign='top' colspan='4'>Mobile</td>";
                    strPrincipal += "<td valign='top' colspan='1' align='center'>:</td>";
                    strPrincipal += "<td valign='top' colspan='5'></td>";
                    strPrincipal += "</tr>";

                    strPrincipal += "<tr>";
                    strPrincipal += "<td valign='top' colspan='4'>Email</td>";
                    strPrincipal += "<td valign='top' colspan='1' align='center'>:</td>";
                    strPrincipal += "<td colspan='15' valign='top'></td>";
                    strPrincipal += "</tr>";

                    strPrincipal += "<tr>";
                    strPrincipal += "<td valign='top' colspan='4'> </td>";
                    strPrincipal += "<td valign='top' colspan='1' align='center'>:</td>";
                    strPrincipal += "<td colspan='5' valign='top'> </td>";

                    strPrincipal += "<td valign='top' colspan='4'>Photo</td>";
                    strPrincipal += "<td valign='top' colspan='1' align='center'>:</td>";
                    strPrincipal += "<td colspan='5' valign='top'></td>";
                    strPrincipal += "</tr>";

                    strPrincipal += "</tbody>";
                    strPrincipal += "</table>";
                }

                contents = contents.Replace("##PRINCIPAL##", strPrincipal);


                //Director Details
                if (director != null)
                {
                    string strDirectorPhdSubjects = string.Empty;
                    string strDirectorQualification = string.Empty;
                    string dateOfAppointment = director.dateOfAppointment.ToString() == null ? string.Empty : UAAAS.Models.Utilities.MMDDYY2DDMMYY(director.dateOfAppointment.ToString()).ToString();
                    string dateOfBirth = director.dateOfBirth.ToString() == null ? string.Empty : UAAAS.Models.Utilities.MMDDYY2DDMMYY(director.dateOfBirth.ToString()).ToString();
                    contents = contents.Replace("##DirectorTitle##", "<p><strong><u>Details of Director:</u></strong></p><br />");
                    contents = contents.Replace("##DirectorFirstName##", director.firstName);
                    contents = contents.Replace("##DirectorLastName##", director.lastName);
                    contents = contents.Replace("##DirectorSurname##", director.surname);
                    if (director.qualificationId == 1)
                    {
                        // strDirectorQualification = "<img alt='' src='" + serverURL + "/Content/Images/checkbox_yes_b.png' height='10' />&nbsp;Doctorate<img alt='' src='" + serverURL + "/Content/Images/checkbox_no_b.png' height='10' />&nbsp;Non-Doctorate";
                        strDirectorQualification = "Doctorate";

                    }
                    else
                    {
                        //strDirectorQualification = "<img alt='' src='" + serverURL + "/Content/Images/checkbox_no_b.png' height='10' />&nbsp;Doctorate<img alt='' src='" + serverURL + "/Content/Images/checkbox_yes_b.png' height='10' />&nbsp;Non-Doctorate";
                        strDirectorQualification = "Non-Doctorate";

                    }
                    //strPhdSubjects
                    var jntuh_phd_subject = db.jntuh_phd_subject.Where(p => p.isActive).Select(it => new { it.id, it.phdSubjectName }).ToList();
                    if (jntuh_phd_subject != null)
                    {
                        foreach (var item in jntuh_phd_subject)
                        {
                            string yesORno = "no_b";
                            if (director.phdId != null)
                            {
                                if (director.phdId == item.id)
                                {
                                    strDirectorPhdSubjects += item.phdSubjectName;
                                }
                            }
                            else
                            {
                                strDirectorPhdSubjects = string.Empty;
                            }
                        }
                    }
                    contents = contents.Replace("##DirectorPhdSubjects##", strDirectorPhdSubjects);
                    contents = contents.Replace("##DirectorPhDAwardedFrom##", director.phdFromUniversity);
                    contents = contents.Replace("##DirectorYear##", director.phdYear.ToString());
                    jntuh_department department = db.jntuh_department.FirstOrDefault(d => d.id == director.departmentId && d.isActive);//.Select(d => d).FirstOrDefault();
                    if (department != null)
                    {
                        contents = contents.Replace("##DirectorDepartment##", department.departmentName);
                    }

                    contents = contents.Replace("##DirectorQualification##", strDirectorQualification);
                    contents = contents.Replace("##DirectorDateofAppointment##", dateOfAppointment);
                    contents = contents.Replace("##DirectorDateofBirth##", dateOfBirth);
                    contents = contents.Replace("##DirectorFax##", director.fax);
                    contents = contents.Replace("##DirectorLandline##", director.landline);
                    contents = contents.Replace("##Mobile##", director.mobile);
                    contents = contents.Replace("##DirectorMobile##", director.mobile);
                    contents = contents.Replace("##DirectorEmail##", director.email.ToString());
                    if (!string.IsNullOrEmpty(director.photo))
                    {
                        string directorParsing = string.Empty;
                        string strDirectorPhoto = string.Empty;
                        string path = "http://jntuhaac.in/Content/Upload/PrincipalDirectorPhotos/" + director.photo.Trim();
                        // string path = @"~/Content/Upload/PrincipalDirectorPhotos/" + director.photo;
                        //  path = System.Web.HttpContext.Current.Server.MapPath(path);

                        #region With-Out Html Parsing
                        try
                        {
                            if (!string.IsNullOrEmpty(path))
                            // if (System.IO.File.Exists(path))
                            {

                                directorParsing += "<p align='center'><img src='" + HtmlEncoder.Encode(path.Trim()) + "' align='center'  width='40' height='45' /></p>";
                                var ParseEliments = HTMLWorker.ParseToList(new StringReader(directorParsing), null);

                                if (path.Contains("."))
                                {
                                    strDirectorPhoto = "<img alt=''src='" + HtmlEncoder.Encode(path.Trim()) + "' align='center'  height='45' />";
                                    // strPrincipalPhoto = "<img alt='' src='" + serverURL + "/Content/Upload/Faculty/Photos/" + PrincipalDetails.Photo + "'" + " align='center'  height='50' />";
                                    contents = contents.Replace("##DirectorPhoto##", strDirectorPhoto);
                                }
                                else
                                    contents = contents.Replace("##DirectorPhoto##", "&nbsp;");

                            }
                            else
                            {
                                if (test415PDF.Equals("YES"))
                                {
                                    contents = contents.Replace("##DirectorPhoto##", "&nbsp;");
                                    // contents = contents.Replace("##DirectorPhoto##", strDirectorPhoto);
                                }
                                else
                                {
                                    contents = contents.Replace("##DirectorPhoto##", "&nbsp;");
                                }
                            }

                        }
                        catch (Exception ex)
                        {
                            contents = contents.Replace("##DirectorPhoto##", "&nbsp;");
                            //  continue;
                        }
                        #endregion

                    }
                    else
                    {
                        contents = contents.Replace("##DirectorPhoto##", "&nbsp;");
                    }
                }
                else
                {
                    string strDirectorQualification = string.Empty;
                    string dateOfAppointment = string.Empty;
                    string dateOfBirth = string.Empty;
                    contents = contents.Replace("##DirectorTitle##", "<p><strong><u>Details of Director:</u></strong> (DIRECTOR DETAILS ARE NOT UPLOADED)</p><br />");
                    contents = contents.Replace("##DirectorFirstName##", string.Empty);
                    contents = contents.Replace("##DirectorLastName##", string.Empty);
                    contents = contents.Replace("##DirectorSurname##", string.Empty);
                    contents = contents.Replace("##DirectorDateofAppointment##", dateOfAppointment);
                    contents = contents.Replace("##DirectorDateofBirth##", dateOfBirth);
                    contents = contents.Replace("##DirectorFax##", string.Empty);
                    contents = contents.Replace("##DirectorLandline##", string.Empty);
                    contents = contents.Replace("##Mobile##", string.Empty);
                    contents = contents.Replace("##DirectorMobile##", string.Empty);
                    contents = contents.Replace("##DirectorEmail##", string.Empty);

                    strDirectorQualification = "";

                    contents = contents.Replace("##DirectorQualification##", strDirectorQualification);

                    contents = contents.Replace("##DirectorPhdSubjects##", string.Empty);
                    contents = contents.Replace("##DirectorPhDAwardedFrom##", string.Empty);
                    contents = contents.Replace("##DirectorYear##", string.Empty);
                    contents = contents.Replace("##DirectorDepartment##", string.Empty);
                    contents = contents.Replace("##DirectorPhoto##", string.Empty);
                }

                // List<IElement> parsedHtmlElements = HTMLWorker.ParseToList(new StringReader(contents), null);
                return contents;
            }
        }

        public string ChairpersonDetails(int collegeId, string contents)
        {
            using (var db = new UAAAS.Models.uaaasDBContext())
            {
                jntuh_college_chairperson chairperson = db.jntuh_college_chairperson.FirstOrDefault(c => c.collegeId == collegeId);//.Select(c => c).FirstOrDefault();
                if (chairperson != null)
                {
                    jntuh_chairperson_designation designation = db.jntuh_chairperson_designation.FirstOrDefault(d => d.id == chairperson.designationId && d.isActive == true);//.Select(d => d).FirstOrDefault();
                    jntuh_address address = db.jntuh_address.FirstOrDefault(a => a.collegeId == chairperson.collegeId && a.addressTye == "SECRETARY");//.Select(a => a).FirstOrDefault();
                    jntuh_state state = new jntuh_state();
                    jntuh_district district = new jntuh_district();
                    if (address != null)
                    {
                        state = db.jntuh_state.FirstOrDefault(s => s.id == address.stateId && s.isActive);//.Select(s => s).FirstOrDefault();
                        district = db.jntuh_district.FirstOrDefault(d => d.id == address.districtId && d.isActive);//.Select(d => d).FirstOrDefault();
                    }
                    contents = contents.Replace("##ChairpersonFirstName##", chairperson.firstName);
                    contents = contents.Replace("##ChairpersonLastName##", chairperson.lastName);
                    contents = contents.Replace("##ChairpersonSurname##", chairperson.surname);
                    contents = contents.Replace("##ChairpersonDesignation##", designation.designationName);
                    contents = contents.Replace("##ChairpersonAddress##", address.address);
                    contents = contents.Replace("##ChairpersonCity/Town##", address.townOrCity);
                    contents = contents.Replace("##ChairpersonMandal##", address.mandal);
                    contents = contents.Replace("##ChairpersonDistrict##", district.districtName);
                    contents = contents.Replace("##ChairpersonState##", state.stateName);
                    contents = contents.Replace("##ChairpersonPincode##", address.pincode.ToString());
                    contents = contents.Replace("##ChairpersonFax##", address.fax);
                    contents = contents.Replace("##ChairpersonLandline##", address.landline);
                    contents = contents.Replace("##ChairpersonMobile##", address.mobile);
                    contents = contents.Replace("##ChairpersonEmail##", address.email);
                    if (!string.IsNullOrEmpty(chairperson.Photo))
                    {
                        string chairpersonParsing = string.Empty;
                        string strchairpersonPhoto = string.Empty;
                        string path = "http://jntuhaac.in/Content/Upload/College/ChairPerson/" + chairperson.Photo.Trim();
                        // string path = @"~/Content/Upload/PrincipalDirectorPhotos/" + director.photo;
                        //  path = System.Web.HttpContext.Current.Server.MapPath(path);

                        #region With-Out Html Parsing
                        try
                        {
                            if (!string.IsNullOrEmpty(path))
                            // if (System.IO.File.Exists(path))
                            {

                                chairpersonParsing += "<p align='center'><img src='" + HtmlEncoder.Encode(path.Trim()) + "' align='center'  width='40' height='45' /></p>";
                                var ParseEliments = HTMLWorker.ParseToList(new StringReader(chairpersonParsing), null);

                                if (path.Contains("."))
                                {
                                    strchairpersonPhoto = "<img alt=''src='" + HtmlEncoder.Encode(path.Trim()) + "' align='center'  height='45' />";
                                    // strPrincipalPhoto = "<img alt='' src='" + serverURL + "/Content/Upload/Faculty/Photos/" + PrincipalDetails.Photo + "'" + " align='center'  height='50' />";
                                    contents = contents.Replace("##ChairpersonPhoto##", strchairpersonPhoto);
                                }
                                else
                                    contents = contents.Replace("##ChairpersonPhoto##", "&nbsp;");

                            }
                            else
                            {
                                if (test415PDF.Equals("YES"))
                                {
                                    contents = contents.Replace("##ChairpersonPhoto##", "&nbsp;");
                                    // contents = contents.Replace("##DirectorPhoto##", strDirectorPhoto);
                                }
                                else
                                {
                                    contents = contents.Replace("##ChairpersonPhoto##", "&nbsp;");
                                }
                            }

                        }
                        catch (Exception ex)
                        {
                            contents = contents.Replace("##ChairpersonPhoto##", "&nbsp;");
                            //  continue;
                        }
                        #endregion

                    }
                    else
                    {
                        contents = contents.Replace("##ChairpersonPhoto##", "&nbsp;");
                    }
                }
                else
                {

                    contents = contents.Replace("##ChairpersonFirstName##", string.Empty);
                    contents = contents.Replace("##ChairpersonLastName##", string.Empty);
                    contents = contents.Replace("##ChairpersonSurname##", string.Empty);
                    contents = contents.Replace("##ChairpersonDesignation##", string.Empty);
                    contents = contents.Replace("##ChairpersonAddress##", string.Empty);
                    contents = contents.Replace("##ChairpersonCity/Town##", string.Empty);
                    contents = contents.Replace("##ChairpersonMandal##", string.Empty);
                    contents = contents.Replace("##ChairpersonDistrict##", string.Empty);
                    contents = contents.Replace("##ChairpersonState##", string.Empty);
                    contents = contents.Replace("##ChairpersonPincode##", string.Empty);
                    contents = contents.Replace("##ChairpersonFax##", string.Empty);
                    contents = contents.Replace("##ChairpersonLandline##", string.Empty);
                    contents = contents.Replace("##ChairpersonMobile##", string.Empty);
                    contents = contents.Replace("##ChairpersonEmail##", string.Empty);
                    contents = contents.Replace("##ChairpersonPhoto##", string.Empty);
                }
                //  List<IElement> parsedHtmlElements = HTMLWorker.ParseToList(new StringReader(contents), null);
                return contents;
            }
        }

        public string LandInformationDetails(int collegeId, string contents)
        {
            using (var db = new UAAAS.Models.uaaasDBContext())
            {
                string strLandInformationDetails = string.Empty;
                int sno = 0;
                string IssuedDate = "";
                jntuh_college_land jntuh_college_land = db.jntuh_college_land.FirstOrDefault(l => l.collegeId == collegeId);//.Select(l => l).FirstOrDefault();
                strLandInformationDetails += "<table border='0' cellspacing='0' cellpadding='5'><tbody>";
                if (jntuh_college_land != null)
                {
                    strLandInformationDetails += "<tr><td width='165'>Total Land Area</td><td width='24'>:</td><td width='502'>" + jntuh_college_land.areaInAcres + " Acres</td></tr>";

                    //LandType          
                    string[] selectedLandType = jntuh_college_land.landTypeID.ToString().Split(' ');
                    List<Item> lstLandType = new List<Item>();
                    foreach (var type in db.jntuh_land_type.Where(l => l.isActive == true))
                    {
                        string strType = type.id.ToString();
                        lstLandType.Add(new Item { id = type.id, name = type.landType, selected = selectedLandType.Contains(strType) ? 1 : 0 });
                    }
                    if (lstLandType != null)
                    {
                        strLandInformationDetails += "<tr><td width='165'>Land Type</td><td width='24'>:</td><td width='502'>";
                        foreach (var item in lstLandType)
                        {
                            //string yesOrNo = "no_b";
                            if (item.selected == 1)
                            {
                                //yesOrNo = "yes_b";
                                strLandInformationDetails += "&nbsp;" + item.name + "&nbsp; &nbsp; &nbsp;";
                            }
                            //strLandInformationDetails += "<img alt='' src='" + serverURL + "/Content/Images/checkbox_" + yesOrNo + ".png' height='10' />&nbsp;" + item.name + "&nbsp; &nbsp; &nbsp;";
                        }
                        strLandInformationDetails += "</td></tr>";
                    }
                    //Registration Type
                    string[] selectedLandRegistrationType = jntuh_college_land.landRegistrationTypeId.ToString().Split(' ');
                    List<Item> lstLandRegistrationtype = new List<Item>();
                    foreach (var type in db.jntuh_land_registration_type.Where(r => r.isActive).ToList())
                    {
                        string strtype = type.id.ToString();
                        lstLandRegistrationtype.Add(new Item { id = type.id, name = type.landRegistrationType, selected = selectedLandRegistrationType.Contains(strtype) ? 1 : 0 });
                    }
                    if (lstLandRegistrationtype != null)
                    {
                        strLandInformationDetails += "<tr><td width='165'>Land Registration Type</td><td width='24'>:</td><td width='502'>";
                        foreach (var item in lstLandRegistrationtype)
                        {
                            //string yesOrNo = "no_b";
                            if (item.selected == 1)
                            {
                                //yesOrNo = "yes_b";
                                strLandInformationDetails += "&nbsp;" + item.name + "&nbsp; &nbsp;";
                            }
                            //strLandInformationDetails += "<img alt='' src='" + serverURL + "/Content/Images/checkbox_" + yesOrNo + ".png' height='10' />&nbsp;" + item.name + "&nbsp; &nbsp;";
                        }
                        strLandInformationDetails += "</td></tr>";
                    }
                    //Land Category            
                    string[] selectedLandCategory = jntuh_college_land.landCategoryId.ToString().Split(' ');
                    List<Item> lstLandCategory = new List<Item>();
                    foreach (var type in db.jntuh_land_category.Where(c => c.isActive).ToList())
                    {
                        string strtype = type.id.ToString();
                        lstLandCategory.Add(new Item { id = type.id, name = type.landCategory, selected = selectedLandCategory.Contains(strtype) ? 1 : 0 });
                    }
                    if (lstLandCategory != null)
                    {
                        strLandInformationDetails += "<tr><td width='165'>Land Category</td><td width='24'>:</td><td width='502'>";
                        foreach (var item in lstLandCategory)
                        {
                            // string yesOrNo = "no_b";
                            if (item.selected == 1)
                            {
                                //yesOrNo = "yes_b";
                                strLandInformationDetails += "&nbsp;" + item.name + "&nbsp; &nbsp; &nbsp;";
                            }
                            //strLandInformationDetails += "<img alt='' src='" + serverURL + "/Content/Images/checkbox_" + yesOrNo + ".png' height='10' />&nbsp;" + item.name + "&nbsp; &nbsp; &nbsp;";
                        }
                        strLandInformationDetails += "</td></tr>";
                    }
                    strLandInformationDetails += "<tr><td width='165' valign='top'><strong>Registration details</strong></td><td width='24' valign='top'>:</td><td width='502'></td></tr>";
                    //Registration Details :
                    List<jntuh_college_land_registration> jntuh_college_land_registration = db.jntuh_college_land_registration
                        .Where(r => r.collegeId == collegeId && r.isActive)
                        .ToList();
                    strLandInformationDetails += "<tr><td width='691' colspan='3' valign='top'>";
                    strLandInformationDetails += "<table border='1' cellspacing='0' cellpadding='5'><tbody>";
                    strLandInformationDetails += "<tr><td colspan='1'>S.No</td><td colspan='4'>Registration Date</td><td colspan='2'>Area in Acres</td><td colspan='3'>Document Number</td><td colspan='4'>Survey Number</td><td colspan='5'>Location/Village</td></tr>";

                    if (jntuh_college_land_registration != null)
                    {
                        foreach (var item in jntuh_college_land_registration)
                        {
                            sno++;
                            if (item.landRegistraionDate != null)
                            {
                                IssuedDate = UAAAS.Models.Utilities.MMDDYY2DDMMYY(item.landRegistraionDate.ToString());
                            }
                            else
                            {
                                IssuedDate = string.Empty;
                            }
                            strLandInformationDetails += "<tr><td colspan='1'>" + sno + "</td><td colspan='4'>" + IssuedDate + "</td><td colspan='2'>" + item.landAreaInAcres + "</td><td colspan='3'>" + item.landDocumentNumber + "</td><td colspan='4'>" + item.landSurveyNumber + "</td><td colspan='5' valign='top'>" + item.landLocation + "</td></tr>";
                        }
                    }
                    strLandInformationDetails += "</tbody></table>";
                    strLandInformationDetails += "</td></tr>";
                    //Land Conversion Certificate
                    strLandInformationDetails += "<tr><td width='691' colspan='3' valign='top'><strong>Land Conversion Certificate :</strong></td></tr>";
                    strLandInformationDetails += "<tr><td width='165' valign='top'>Issued by</td><td width='24' valign='top'>:</td><td width='502'>" + jntuh_college_land.conversioncertificateissuedBy + "</td></tr>";
                    if (jntuh_college_land.conversionCertificateIssuedDate != null)
                    {
                        IssuedDate = UAAAS.Models.Utilities.MMDDYY2DDMMYY(jntuh_college_land.conversionCertificateIssuedDate.ToString());
                    }
                    else
                    {
                        IssuedDate = string.Empty; ;
                    }
                    strLandInformationDetails += "<tr><td width='165' valign='top'>Issued Date</td><td width='24' valign='top'>:</td><td width='502'>" + IssuedDate + " </td></tr>";
                    strLandInformationDetails += "<tr><td width='165' valign='top'>Issued Purpose</td><td width='24' valign='top'>:</td><td width='502'>" + jntuh_college_land.conversionCertificateIssuedPurpose + "</td></tr>";
                    //Building Plan
                    strLandInformationDetails += "<tr><td width='691' colspan='3' valign='top'><strong>Building Plan</strong> in the name of the proposed institution prepared by Architect and Approved by Competent Authority :</td></tr>";
                    strLandInformationDetails += "<tr><td width='165' valign='top'>Issued by</td><td width='24' valign='top'>:</td><td width='502'>" + jntuh_college_land.buildingPlanIssuedBy + "</td></tr>";
                    if (jntuh_college_land.buildingPlanIssuedDate != null)
                    {
                        IssuedDate = UAAAS.Models.Utilities.MMDDYY2DDMMYY(jntuh_college_land.buildingPlanIssuedDate.ToString());
                    }
                    else
                    {
                        IssuedDate = string.Empty;
                    }
                    strLandInformationDetails += "<tr><td width='165' valign='top'>Issued Date</td><td width='24' valign='top'>:</td><td width='502'>" + IssuedDate + " </td></tr>";
                    //Master Plan
                    strLandInformationDetails += "<tr><td width='691' colspan='3' valign='top'><strong>Master Plan</strong> in the name of the proposed institution prepared by Architect and Approved by Competent Authority :</td></tr>";
                    strLandInformationDetails += "<tr><td width='165' valign='top'>Issued by</td><td width='24' valign='top'>:</td><td width='502'>" + jntuh_college_land.masterPlanIssuedBy + "</td></tr>";
                    if (jntuh_college_land.masterPlanIssuedDate != null)
                    {
                        IssuedDate = UAAAS.Models.Utilities.MMDDYY2DDMMYY(jntuh_college_land.masterPlanIssuedDate.ToString());
                    }
                    else
                    {
                        IssuedDate = string.Empty;
                    }
                    strLandInformationDetails += "<tr><td width='165' valign='top'>Issued Date</td><td width='24' valign='top'>:</td><td width='502'>" + IssuedDate + " </td></tr>";
                    //Compound Wall/Fencing
                    if (jntuh_college_land.compoundWall == true)
                    {
                        strLandInformationDetails += "<tr><td width='165' valign='top' style='font-size: 8px;'>Compound Wall/Fencing</td><td width='24' valign='top'>:</td><td width='502'>&nbsp;Yes &nbsp; &nbsp; &nbsp;</td></tr>";

                    }
                    else
                    {

                        strLandInformationDetails += "<tr><td width='165' valign='top' style='font-size: 8px;'>Compound Wall/Fencing</td><td width='24' valign='top'>:</td><td width='502'>&nbsp;No</td></tr>";
                    }
                    //ApproachRoad
                    string[] selectedApproachRoad = jntuh_college_land.approachRoadId.ToString().Split(' ');
                    List<Item> lstApproachRoad = new List<Item>();
                    foreach (var type in db.jntuh_approach_road.Where(a => a.isActive).ToList())
                    {
                        string strtype = type.id.ToString();
                        lstApproachRoad.Add(new Item { id = type.id, name = type.approachRoadType, selected = selectedApproachRoad.Contains(strtype) ? 1 : 0 });
                    }
                    if (lstApproachRoad != null)
                    {
                        strLandInformationDetails += "<tr><td width='165' valign='top'>Approach Road</td><td width='24' valign='top'>:</td><td width='502'>";
                        foreach (var item in lstApproachRoad)
                        {
                            if (item.selected == 1)
                            {
                                strLandInformationDetails += "&nbsp;" + item.name + " &nbsp; &nbsp; &nbsp;";
                            }
                        }
                        strLandInformationDetails += "</td></tr>";
                    }
                    //PowerSupply
                    string[] selectedPowerSupply = jntuh_college_land.powerSupplyId.ToString().Split(' ');
                    List<Item> lstPowerSupply = new List<Item>();
                    foreach (var type in db.jntuh_facility_status.Where(p => p.isActive).ToList())
                    {
                        string strtype = type.id.ToString();
                        lstPowerSupply.Add(new Item { id = type.id, name = type.facilityStatus, selected = selectedPowerSupply.Contains(strtype) ? 1 : 0 });
                    }
                    if (lstPowerSupply != null)
                    {
                        strLandInformationDetails += "<tr><td width='165' valign='top'>Power Supply</td><td width='24' valign='top'>:</td><td width='502'>";
                        foreach (var item in lstPowerSupply)
                        {
                            if (item.selected == 1)
                            {
                                strLandInformationDetails += "&nbsp;" + item.name + "&nbsp; &nbsp; &nbsp;";
                            }
                        }
                        strLandInformationDetails += "</td></tr>";
                    }
                    //WaterSupply
                    string[] selectedWaterSupply = jntuh_college_land.WaterSupplyId.ToString().Split(' ');
                    List<Item> lstWaterSupply = new List<Item>();
                    foreach (var type in db.jntuh_facility_status.Where(w => w.isActive).ToList())
                    {
                        string strtype = type.id.ToString();
                        lstWaterSupply.Add(new Item { id = type.id, name = type.facilityStatus, selected = selectedWaterSupply.Contains(strtype) ? 1 : 0 });
                    }
                    if (lstWaterSupply != null)
                    {
                        strLandInformationDetails += "<tr><td width='165' valign='top'>Water Supply</td><td width='24' valign='top'>:</td><td width='502'>";
                        foreach (var item in lstWaterSupply)
                        {
                            if (item.selected == 1)
                            {
                                strLandInformationDetails += "&nbsp;" + item.name + " &nbsp; &nbsp; &nbsp;";
                            }
                        }
                        strLandInformationDetails += "</td></tr>";
                    }
                    //Drinkingwater
                    string[] selectedDrinkingwater = jntuh_college_land.drinkingWaterId.ToString().Split(' ');
                    List<Item> lstDrinkingWater = new List<Item>();
                    foreach (var type in db.jntuh_water_type.Where(d => d.isActive).ToList())
                    {
                        string strtype = type.id.ToString();
                        lstDrinkingWater.Add(new Item { id = type.id, name = type.waterType, selected = selectedDrinkingwater.Contains(strtype) ? 1 : 0 });
                    }
                    if (lstDrinkingWater != null)
                    {
                        strLandInformationDetails += "<tr><td width='165' valign='top'>Drinking Water</td><td width='24' valign='top'>:</td><td width='502'>";
                        foreach (var item in lstDrinkingWater)
                        {
                            if (item.selected == 1)
                            {
                                strLandInformationDetails += "&nbsp;" + item.name + " &nbsp; &nbsp; &nbsp;";
                            }
                        }
                        strLandInformationDetails += "</td></tr>";
                    }
                    if (jntuh_college_land.IsPurifiedWater == true)
                    {
                        strLandInformationDetails += "<tr><td width='165' valign='top'>Is Water Purified?</td><td width='24' valign='top'>:</td><td width='502'>&nbsp;Yes &nbsp;&nbsp;&nbsp;</td></tr>";

                    }
                    else
                    {
                        //strLandInformationDetails += "<tr><td width='165' valign='top'>Is Water Purified?</td><td width='24' valign='top'>:</td><td width='502'><img alt='' src='" + serverURL + "/Content/Images/checkbox_no_b.png' height='10' />&nbsp;Yes &nbsp;&nbsp;&nbsp;<img alt='' src='" + serverURL + "/Content/Images/checkbox_yes_b.png' height='10' />&nbsp;No</td></tr>";
                        strLandInformationDetails += "<tr><td width='165' valign='top'>Is Water Purified?</td><td width='24' valign='top'>:</td><td width='502'>&nbsp;No</td></tr>";
                    }
                    strLandInformationDetails += "<tr><td width='165' valign='top'>Potable water</td><td width='24' valign='top'>:</td><td width='502'>" + jntuh_college_land.potableWaterPerDay + " (in Liters per day)</td></tr>";
                }
                else
                {
                    strLandInformationDetails += "<tr><td width='165'>Total Land Area</td><td width='24'>:</td><td width='502'>___________ Acres</td></tr>";

                    //LandType          

                    strLandInformationDetails += "<tr><td width='165'>Land Type</td><td width='24'>:</td><td width='502'>";
                    strLandInformationDetails += "";
                    strLandInformationDetails += "";
                    strLandInformationDetails += "</td></tr>";

                    //Registration Type
                    strLandInformationDetails += "<tr><td width='165'>Land Registration Type</td><td width='24'>:</td><td width='502'>";
                    strLandInformationDetails += "";
                    strLandInformationDetails += "";
                    strLandInformationDetails += "";
                    strLandInformationDetails += "</td></tr>";

                    //Land Category            

                    strLandInformationDetails += "<tr><td width='165'>Land Category</td><td width='24'>:</td><td width='502'>";
                    strLandInformationDetails += "";
                    strLandInformationDetails += "";
                    strLandInformationDetails += "";
                    strLandInformationDetails += "";
                    strLandInformationDetails += "";
                    strLandInformationDetails += "</td></tr>";
                    strLandInformationDetails += "<tr><td width='165' valign='top'>Registration details</td><td width='24' valign='top'>:</td><td width='502'></td></tr>";
                    //Registration Details :

                    strLandInformationDetails += "<tr><td width='691' colspan='3' valign='top'>";
                    strLandInformationDetails += "<table border='1' cellspacing='0' cellpadding='5'><tbody>";
                    strLandInformationDetails += "<tr><td width='56'>S.No</td><td width='135'>Registration Date (DD/MM/YYYY)</td><td width='69'>Area in Acres</td><td width='147'>Document Number</td><td width='144'>Survey Number</td><td width='126'>Location/Village</td></tr>";



                    strLandInformationDetails += "</tbody></table>";
                    strLandInformationDetails += "</td></tr>";
                    //Land Conversion Certificate
                    strLandInformationDetails += "<tr><td width='691' colspan='3' valign='top'>Land Conversion Certificate :</td></tr>";
                    strLandInformationDetails += "<tr><td width='165' valign='top'>Issued by</td><td width='24' valign='top'>:</td><td width='502'>" + string.Empty + "</td></tr>";

                    strLandInformationDetails += "<tr><td width='165' valign='top'>Issued Date</td><td width='24' valign='top'>:</td><td width='502'>" + string.Empty + "(DD/MM/YYYY)</td></tr>";
                    strLandInformationDetails += "<tr><td width='165' valign='top'>Issued Purpose</td><td width='24' valign='top'>:</td><td width='502'>" + string.Empty + "</td></tr>";
                    //Building Plan
                    strLandInformationDetails += "<tr><td width='691' colspan='3' valign='top'><strong>Building Plan</strong> in the name of the proposed institution prepared by Architect and Approved by Competent Authority :</td></tr>";
                    strLandInformationDetails += "<tr><td width='165' valign='top'>Issued by</td><td width='24' valign='top'>:</td><td width='502'>" + string.Empty + "</td></tr>";

                    strLandInformationDetails += "<tr><td width='165' valign='top'>Issued Date</td><td width='24' valign='top'>:</td><td width='502'>" + string.Empty + "(DD/MM/YYYY)</td></tr>";
                    //Master Plan
                    strLandInformationDetails += "<tr><td width='691' colspan='3' valign='top'>Master Plan in the name of the proposed institution prepared by Architect and Approved by Competent Authority :</td></tr>";
                    strLandInformationDetails += "<tr><td width='165' valign='top'>Issued by</td><td width='24' valign='top'>:</td><td width='502'>" + string.Empty + "</td></tr>";

                    strLandInformationDetails += "<tr><td width='165' valign='top'>Issued Date</td><td width='24' valign='top'>:</td><td width='502'>" + string.Empty + "(DD/MM/YYYY)</td></tr>";
                    //Compound Wall/Fencing

                    strLandInformationDetails += "<tr><td width='165' valign='top'>Compound Wall/Fencing</td><td width='24' valign='top'>:</td><td width='502'></td></tr>";

                    //ApproachRoad

                    strLandInformationDetails += "<tr><td width='165' valign='top'>Approach Road</td><td width='24' valign='top'>:</td><td width='502'>";
                    strLandInformationDetails += "";
                    strLandInformationDetails += "";
                    strLandInformationDetails += "";
                    strLandInformationDetails += "";
                    strLandInformationDetails += "</td></tr>";

                    //PowerSupply
                    strLandInformationDetails += "<tr><td width='165' valign='top'>Power Supply</td><td width='24' valign='top'>:</td><td width='502'>";
                    strLandInformationDetails += "";
                    strLandInformationDetails += "</td></tr>";

                    //WaterSupply
                    strLandInformationDetails += "<tr><td width='165' valign='top'>Water Supply</td><td width='24' valign='top'>:</td><td width='502'>";
                    strLandInformationDetails += "";
                    strLandInformationDetails += "</td></tr>";

                    //Drinkingwater

                    strLandInformationDetails += "<tr><td width='165' valign='top'>Drinking Water</td><td width='24' valign='top'>:</td><td width='502'>";
                    strLandInformationDetails += "";
                    strLandInformationDetails += "</td></tr>";
                    strLandInformationDetails += "<tr><td width='165' valign='top'>Is Water Purified?</td><td width='24' valign='top'>:</td><td width='502'></td></tr>";
                    strLandInformationDetails += "<tr><td width='165' valign='top'>Potable water</td><td width='24' valign='top'>:</td><td width='502'>_______________(in Liters per day)</td></tr>";
                }

                strLandInformationDetails += "</tbody></table>";
                contents = contents.Replace("##LandInformationDetails##", strLandInformationDetails);
                // List<IElement> parsedHtmlElements = HTMLWorker.ParseToList(new StringReader(contents), null);
                return contents;
            }
        }

        public string AdministrativeLandDetails(int collegeId, string contents)
        {
            using (var db = new UAAAS.Models.uaaasDBContext())
            {
                string strAdministrativeLandDetails = string.Empty;
                decimal totalArea = 0;
                strAdministrativeLandDetails += "<table border='1' cellspacing='0' cellpadding='5'><tbody>";
                strAdministrativeLandDetails += "<tr>";
                strAdministrativeLandDetails += "<td width='24%'><p><b>Type</b></p></td>";
                strAdministrativeLandDetails += "<td width='18%'><p align='left'><b>Program</b></p></td>";
                strAdministrativeLandDetails += "<td width='9%'><p align='center'><b>Available Rooms</b></p></td>";
                strAdministrativeLandDetails += "<td width='9%'><p align='center'><b>Available Area</b></p></td>";
                strAdministrativeLandDetails += "</tr>";
                IQueryable<jntuh_college_area> jntuh_college_area = db.jntuh_college_area.Where(s => s.collegeId == collegeId).Select(e => e);
                IQueryable<jntuh_program_type> jntuh_program_type = db.jntuh_program_type.Select(e => e);

                List<AdminLand> land = db.jntuh_area_requirement.Where(r => r.isActive == true && r.areaType == "ADMINISTRATIVE").OrderBy(r => r.areaTypeDisplayOrder)
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
                                            availableArea = jntuh_college_area.Where(a => a.areaRequirementId == r.id).Select(a => a.availableArea).FirstOrDefault()
                                        }).Where(g => g.availableRooms != null && g.availableRooms != 0).ToList();
                if (land != null)
                {
                    foreach (var item in land)
                    {
                        string programType = jntuh_program_type.Where(p => p.id == item.programId).Select(p => p.programType).FirstOrDefault();
                        if (programType == null)
                        {
                            programType = string.Empty;
                        }

                        strAdministrativeLandDetails += "<tr>";
                        strAdministrativeLandDetails += "<td width='24%'><p>" + item.requirementType + "</p></td>";
                        strAdministrativeLandDetails += "<td width='18%'>" + programType + "</td>";
                        if (item.availableRooms != null)
                        {
                            strAdministrativeLandDetails += "<td width='9%' align='right'>" + (int)item.availableRooms + "</td>";
                        }
                        else
                        {
                            strAdministrativeLandDetails += "<td width='9%' align='right'>" + item.availableRooms + "</td>";
                        }
                        strAdministrativeLandDetails += "<td width='9%' align='right'>" + item.availableArea + "</td>";
                        strAdministrativeLandDetails += "</tr>";
                        if (item.availableArea != null)
                        {
                            totalArea += (decimal)item.availableArea;
                        }
                    }
                }
                strAdministrativeLandDetails += "<tr>";
                strAdministrativeLandDetails += "<td colspan='3' align='right'><b>Total</b></td>";
                strAdministrativeLandDetails += "<td width='9%' align='right'>" + totalArea + "</td>";
                strAdministrativeLandDetails += "</tr>";
                strAdministrativeLandDetails += "</tbody></table>";
                contents = contents.Replace("##AdministrativeLandDetails##", strAdministrativeLandDetails);
                // List<IElement> parsedHtmlElements = HTMLWorker.ParseToList(new StringReader(contents), null);
                return contents;
            }
        }

        public string InstructionalAreaDetails(int collegeId, string contents)
        {
            using (var db = new UAAAS.Models.uaaasDBContext())
            {
                string strInstructionalAreaDetails = string.Empty;
                decimal totalArea = 0;
                IQueryable<jntuh_college_area> jntuh_college_area = db.jntuh_college_area.Where(s => s.collegeId == collegeId).Select(e => e);
                strInstructionalAreaDetails += "<table border='1' cellspacing='0' cellpadding='5'><tbody>";
                strInstructionalAreaDetails += "<tr style='background:#808080'><td width='28%'><p><b>Requirement Type</b></p></td><td width='10%'><p align='center'><b>Available Rooms</b></p></td><td width='10%'><p align='center'><b>Available Area</b></p></td></tr>";

                //Degree related requirement types
                List<AdminLand> programIds = (from a in db.jntuh_college_area
                                              join ar in db.jntuh_area_requirement
                                              on a.areaRequirementId equals ar.id
                                              where (ar.isActive && ar.areaType == "INSTRUCTIONAL" && a.collegeId == collegeId)
                                              orderby ar.areaTypeDisplayOrder
                                              select new AdminLand
                                              {
                                                  programId = ar.programId
                                              }).Distinct().ToList();
                var ids = programIds.Select(it => it.programId).ToList();
                var programtypes = db.jntuh_program_type.Where(d => d.isActive == true && ids.Contains(d.id)).ToList();
                if (programIds != null)
                {
                    foreach (var item in programIds)
                    {
                        int programId = (int)item.programId;
                        string programType = programtypes.Where(it => it.id == item.programId).Select(d => d.programType).FirstOrDefault();
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
                                         availableArea = jntuh_college_area.Where(a => a.areaRequirementId == r.id).Select(a => a.availableArea).FirstOrDefault()
                                     }).Where(g => g.availableRooms != null && g.availableRooms != 0).ToList();
                        if (land != null)
                        {
                            strInstructionalAreaDetails += "<tr>";
                            strInstructionalAreaDetails += "<td colspan='5' style='width: 200%'><p><b>" + programType + "</b></p></td>";
                            strInstructionalAreaDetails += "</tr>";
                            foreach (var i in land)
                            {
                                strInstructionalAreaDetails += "<tr>";
                                strInstructionalAreaDetails += "<td width='28%'><p>" + i.requirementType + "</p></td>";
                                if (i.availableRooms != null)
                                {
                                    strInstructionalAreaDetails += "<td width='10%' align='right'>" + (int)i.availableRooms + "</td>";
                                }
                                else
                                {
                                    strInstructionalAreaDetails += "<td width='10%' align='right'>" + i.availableRooms + "</td>";
                                }
                                strInstructionalAreaDetails += "<td width='10%' align='right'>" + i.availableArea + "</td>";
                                strInstructionalAreaDetails += "</tr>";
                                if (i.availableArea != null)
                                {
                                    totalArea += (int)i.availableArea;
                                }
                            }
                        }
                    }

                }
                strInstructionalAreaDetails += "<tr>";
                strInstructionalAreaDetails += "<td colspan='2' align='right'>Total</td>";
                strInstructionalAreaDetails += "<td width='10%' align='right'>" + totalArea + "</td>";
                strInstructionalAreaDetails += "</tr>";
                strInstructionalAreaDetails += "</tbody></table>";
                contents = contents.Replace("##InstructionalAreaDetails##", strInstructionalAreaDetails);
                //List<IElement> parsedHtmlElements = HTMLWorker.ParseToList(new StringReader(contents), null);
                return contents;
            }
        }

        public string ExistingIntakeDetails(int collegeId, string contents)
        {
            using (var db = new UAAAS.Models.uaaasDBContext())
            {
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
                int totalAICTEApprovedIntake1 = 0;
                int totalAICTEApprovedIntake2 = 0;
                int totalAICTEApprovedIntake3 = 0;
                int totalAICTEApprovedIntake4 = 0;
                int totalAICTEApprovedIntake5 = 0;
                int totalExaminationBranchAdmittedIntake1 = 0;
                int totalExaminationBranchAdmittedIntake2 = 0;
                int totalExaminationBranchAdmittedIntake3 = 0;
                int totalExaminationBranchAdmittedIntake4 = 0;
                int totalExaminationBranchAdmittedIntake5 = 0;
                int totalAICTEApproved = 0;
                int totalApproved = 0;
                int totalAdmited = 0;
                int totalExaminationBranchAdmittedIntake = 0;
                int totalPAYApproved = 0;
                int totalPAYAdmited = 0;
                int totalPAYProposed = 0;
                IQueryable<jntuh_academic_year> jntuh_academic_year = db.jntuh_academic_year.AsNoTracking().Select(e => e);
                ViewBag.AcademicYear = jntuh_academic_year.Where(a => a.isActive && a.isPresentAcademicYear).Select(a => a.academicYear).FirstOrDefault();
                int actualYear = jntuh_academic_year.Where(a => a.isActive && a.isPresentAcademicYear).Select(a => a.actualYear).FirstOrDefault();

                string FirstYear = String.Format("{0}-{1}", (actualYear).ToString(), (actualYear + 1).ToString().Substring(2, 2));
                string SecondYear = String.Format("{0}-{1}", (actualYear - 1).ToString(), (actualYear).ToString().Substring(2, 2));
                string ThirdYear = String.Format("{0}-{1}", (actualYear - 2).ToString(), (actualYear - 1).ToString().Substring(2, 2));
                string FourthYear = String.Format("{0}-{1}", (actualYear - 3).ToString(), (actualYear - 2).ToString().Substring(2, 2));
                string FifthYear = String.Format("{0}-{1}", (actualYear - 4).ToString(), (actualYear - 3).ToString().Substring(2, 2));

                int presentYear = jntuh_academic_year.Where(a => a.isActive && a.isPresentAcademicYear).Select(a => a.actualYear).FirstOrDefault();

                int AY1 = jntuh_academic_year.Where(a => a.actualYear == (presentYear)).Select(a => a.id).FirstOrDefault();
                int AY2 = jntuh_academic_year.Where(a => a.actualYear == (presentYear - 1)).Select(a => a.id).FirstOrDefault();
                int AY3 = jntuh_academic_year.Where(a => a.actualYear == (presentYear - 2)).Select(a => a.id).FirstOrDefault();
                int AY4 = jntuh_academic_year.Where(a => a.actualYear == (presentYear - 3)).Select(a => a.id).FirstOrDefault();
                int AY5 = jntuh_academic_year.Where(a => a.actualYear == (presentYear - 4)).Select(a => a.id).FirstOrDefault();

                int PAY = jntuh_academic_year.Where(a => a.actualYear == (presentYear + 1)).Select(a => a.id).FirstOrDefault();

                strExistingIntakeDetails += "<table border='1' cellspacing='0' cellpadding='5' style='font-size: 8px;'>";
                strExistingIntakeDetails += "<tbody>";
                strExistingIntakeDetails += "<tr>";
                strExistingIntakeDetails += "<td width='28' rowspan='3' colspan='1'><p align='center'>S.No</p></td>";
                strExistingIntakeDetails += "<td width='45' rowspan='3' colspan='3'><p align='left'>Degree</p><p align='left'>*</p></td>";
                strExistingIntakeDetails += "<td width='55' rowspan='3' colspan='4'><p align='left'>Department</p><p align='left'>**</p></td>";
                strExistingIntakeDetails += "<td width='170' rowspan='3' colspan='4'><p align='left'>Specialization</p><p align='left'>***</p></td>";
                strExistingIntakeDetails += "<td width='42' rowspan='3' colspan='1' style='font-size: 9px; line-height: 10px;'><p align='center'>Shift</p><p align='center'>#</p></td>";
                strExistingIntakeDetails += "<td width='550' colspan='9'><p align='center'>Sanctioned & Actual Admitted Intake as per Academic Year</p></td></tr>";
                //strExistingIntakeDetails += "<td  rowspan='3' colspan='1'><p align='left'>PI</p></td>";
                //strExistingIntakeDetails += "<td  rowspan='3' colspan='1'><p align='left'>CS</p></td>";
                //strExistingIntakeDetails += "<td  rowspan='3' colspan='1'><p align='left'>CF</p></td>";
                //strExistingIntakeDetails += "<td width='100' colspan='4' valign='top'><p align='center'>NBA accreditation Period (if exists)</p></td></tr>";
                strExistingIntakeDetails += "<tr><td width='100' colspan='3'><p align='center'>" + ThirdYear + "</p></td>";
                strExistingIntakeDetails += "<td width='100' colspan='3'><p align='center'>" + SecondYear + "</p></td>";
                strExistingIntakeDetails += "<td width='100' colspan='3'><p align='center'>" + FirstYear + "</p></td>";


                //strExistingIntakeDetails += "<td width='100' colspan='4' valign='top'><p align='center' style='font-style: 7px;'>(DD/MM/YYY)</p></td>";
                strExistingIntakeDetails += "</tr>";
                strExistingIntakeDetails += "<tr style='font-size: 7px;'>";
                strExistingIntakeDetails += "<td width='50' colspan='1'><p align='center'>AS</p></td>";
                strExistingIntakeDetails += "<td width='55' colspan='1'><p align='center'>JS</p></td>";
                strExistingIntakeDetails += "<td width='55' colspan='1'><p align='center'>AA</p></td>";
                strExistingIntakeDetails += "<td width='50' colspan='1'><p align='center'>AS</p></td>";
                strExistingIntakeDetails += "<td width='55' colspan='1'><p align='center'>JS</p></td>";
                strExistingIntakeDetails += "<td width='55' colspan='1'><p align='center'>AA</p></td>";
                strExistingIntakeDetails += "<td width='50' colspan='1'><p align='center'>AS</p></td>";
                strExistingIntakeDetails += "<td width='55' colspan='1'><p align='center'>JS</p></td>";
                strExistingIntakeDetails += "<td width='55' colspan='1'><p align='center'>AA</p></td>";
                //strExistingIntakeDetails += "<td width='50' colspan='2' valign='top'><p align='center'>From</p></td>";
                //strExistingIntakeDetails += "<td width='50' colspan='2' valign='top'><p align='center'>To</p></td>";
                strExistingIntakeDetails += "</tr>";

                int[] inactivespids = db.jntuh_specialization.Where(s => s.isActive == false).Select(s => s.id).ToArray();
                IQueryable<jntuh_college_intake_existing> intake = db.jntuh_college_intake_existing.Where(i => i.collegeId == collegeId && !inactivespids.Contains(i.specializationId)).Select(e => e);
                List<CollegeIntakeExisting> collegeIntakeExisting = new List<CollegeIntakeExisting>();
                var specializationIds = intake.Select(it => it.specializationId).ToList();

                var jntuh_specializations = db.jntuh_specialization.Where(s => specializationIds.Contains(s.id))
                   .Select(it => new { it.id, it.specializationName, it.departmentId }).ToList();
                var departmentIds = jntuh_specializations.Select(it => it.departmentId).ToList();
                var jntuh_departments = db.jntuh_department.Where(d => departmentIds.Contains(d.id))
                    .Select(it => new { it.id, it.departmentName, it.degreeId }).ToList();
                var degreeids = jntuh_departments.Select(it1 => it1.degreeId).ToList();
                var jntuh_degrees = db.jntuh_degree.Where(it => degreeids.Contains(it.id))
                    .Select(it => new { it.id, it.degree, it.degreeDisplayOrder }).ToList();
                var shiftids = intake.Select(p => p.shiftId).ToList();
                var jntuh_shifts = db.jntuh_shift.Where(it => shiftids.Contains(it.id)).Select(it => new { it.id, it.shiftName }).ToList();
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
                    newIntake.Specialization = jntuh_specializations.Where(s => s.id == item.specializationId).Select(s => s.specializationName).FirstOrDefault();
                    newIntake.DepartmentID = jntuh_specializations.Where(s => s.id == item.specializationId).Select(s => s.departmentId).FirstOrDefault();
                    newIntake.Department = jntuh_departments.Where(d => d.id == newIntake.DepartmentID).Select(d => d.departmentName).FirstOrDefault();
                    newIntake.degreeID = jntuh_departments.Where(d => d.id == newIntake.DepartmentID).Select(d => d.degreeId).FirstOrDefault();
                    newIntake.Degree = jntuh_degrees.Where(d => d.id == newIntake.degreeID).Select(d => d.degree).FirstOrDefault();
                    newIntake.degreeDisplayOrder = jntuh_degrees.Where(d => d.id == newIntake.degreeID).Select(d => d.degreeDisplayOrder).FirstOrDefault();
                    newIntake.shiftId = item.shiftId;
                    newIntake.Shift = jntuh_shifts.Where(s => s.id == item.shiftId).Select(s => s.shiftName).FirstOrDefault();
                    collegeIntakeExisting.Add(newIntake);
                }

                collegeIntakeExisting = collegeIntakeExisting.AsEnumerable().GroupBy(r => new { r.specializationId, r.shiftId }).Select(r => r.FirstOrDefault()).ToList();
                collegeIntakeExisting = collegeIntakeExisting.OrderBy(ei => ei.degreeDisplayOrder).ThenBy(ei => ei.Department).ThenBy(ei => ei.Specialization).ThenBy(ei => ei.shiftId).ToList();
                var PgDegreeIds = new int[] { 1, 2, 3, 6, 7, 8, 9, 10 };
                foreach (var item in collegeIntakeExisting)
                {
                    sno++;

                    if (item.nbaFrom != null)
                        item.nbaFromDate = UAAAS.Models.Utilities.MMDDYY2DDMMYY(item.nbaFrom.ToString());
                    if (item.nbaTo != null)
                        item.nbaToDate = UAAAS.Models.Utilities.MMDDYY2DDMMYY(item.nbaTo.ToString());
                    item.ProposedIntake = intake.Where(i => i.academicYearId == PAY && i.specializationId == item.specializationId && i.shiftId == item.shiftId).Select(i => i.proposedIntake).FirstOrDefault();
                    if (item.ProposedIntake != null)
                        totalPAYProposed += (int)item.ProposedIntake;
                    item.courseStatus = intake.Where(i => i.academicYearId == PAY && i.specializationId == item.specializationId && i.shiftId == item.shiftId).Select(i => i.courseStatus).FirstOrDefault();

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

                    item.AICTEapprovedIntake1 = GetIntake(collegeId, AY1, item.specializationId, item.shiftId, 2);
                    item.approvedIntake1 = GetIntake(collegeId, AY1, item.specializationId, item.shiftId, 1);
                    item.admittedIntake1 = GetIntake(collegeId, AY1, item.specializationId, item.shiftId, 0);
                    item.ExambranchadmittedIntakeR1 = GetExaminationBranchIntake(collegeId, AY1, item.specializationId, item.shiftId, 0);
                    totalApprovedIntake1 += item.approvedIntake1;
                    totalAdmittedIntake1 += item.admittedIntake1;
                    totalAICTEApprovedIntake1 += item.AICTEapprovedIntake1;
                    if (PgDegreeIds.Contains(item.degreeID))
                        totalExaminationBranchAdmittedIntake1 += item.admittedIntake1;
                    else
                        totalExaminationBranchAdmittedIntake1 += item.ExambranchadmittedIntakeR1;
                    //totalExaminationBranchAdmittedIntake1 += item.ExambranchadmittedIntakeR1;

                    item.AICTEapprovedIntake2 = GetIntake(collegeId, AY2, item.specializationId, item.shiftId, 2);
                    item.approvedIntake2 = GetIntake(collegeId, AY2, item.specializationId, item.shiftId, 1);
                    item.admittedIntake2 = GetIntake(collegeId, AY2, item.specializationId, item.shiftId, 0);
                    item.ExambranchadmittedIntakeR2 = GetExaminationBranchIntake(collegeId, AY2, item.specializationId, item.shiftId, 0);
                    totalApprovedIntake2 += item.approvedIntake2;
                    totalAdmittedIntake2 += item.admittedIntake2;
                    totalAICTEApprovedIntake2 += item.AICTEapprovedIntake2;
                    if (PgDegreeIds.Contains(item.degreeID))
                        totalExaminationBranchAdmittedIntake2 += item.admittedIntake2;
                    else
                        totalExaminationBranchAdmittedIntake2 += item.ExambranchadmittedIntakeR2;
                    //totalExaminationBranchAdmittedIntake2 += item.ExambranchadmittedIntakeR2;

                    item.AICTEapprovedIntake3 = GetIntake(collegeId, AY3, item.specializationId, item.shiftId, 2);
                    item.approvedIntake3 = GetIntake(collegeId, AY3, item.specializationId, item.shiftId, 1);
                    item.admittedIntake3 = GetIntake(collegeId, AY3, item.specializationId, item.shiftId, 0);
                    item.ExambranchadmittedIntakeR3 = GetExaminationBranchIntake(collegeId, AY3, item.specializationId, item.shiftId, 0);
                    totalApprovedIntake3 += item.approvedIntake3;
                    totalAdmittedIntake3 += item.admittedIntake3;
                    totalAICTEApprovedIntake3 += item.AICTEapprovedIntake3;
                    if (PgDegreeIds.Contains(item.degreeID))
                        totalExaminationBranchAdmittedIntake3 += item.admittedIntake3;
                    else
                        totalExaminationBranchAdmittedIntake3 += item.ExambranchadmittedIntakeR3;
                    //totalExaminationBranchAdmittedIntake3 += item.ExambranchadmittedIntakeR3;

                    item.AICTEapprovedIntake4 = GetIntake(collegeId, AY4, item.specializationId, item.shiftId, 2);
                    item.approvedIntake4 = GetIntake(collegeId, AY4, item.specializationId, item.shiftId, 1);
                    item.admittedIntake4 = GetIntake(collegeId, AY4, item.specializationId, item.shiftId, 0);
                    item.ExambranchadmittedIntakeR4 = GetExaminationBranchIntake(collegeId, AY4, item.specializationId, item.shiftId, 0);
                    totalApprovedIntake4 += item.approvedIntake4;
                    totalAdmittedIntake4 += item.admittedIntake4;
                    totalAICTEApprovedIntake4 += item.AICTEapprovedIntake4;
                    if (PgDegreeIds.Contains(item.degreeID))
                        totalExaminationBranchAdmittedIntake4 += item.admittedIntake4;
                    else
                        totalExaminationBranchAdmittedIntake4 += item.ExambranchadmittedIntakeR4;
                    //totalExaminationBranchAdmittedIntake4 += item.ExambranchadmittedIntakeR4;

                    item.AICTEapprovedIntake5 = GetIntake(collegeId, AY5, item.specializationId, item.shiftId, 2);
                    item.approvedIntake5 = GetIntake(collegeId, AY5, item.specializationId, item.shiftId, 1);
                    item.admittedIntake5 = GetIntake(collegeId, AY5, item.specializationId, item.shiftId, 0);
                    item.ExambranchadmittedIntakeR5 = GetExaminationBranchIntake(collegeId, AY5, item.specializationId, item.shiftId, 0);
                    totalApprovedIntake5 += item.approvedIntake5;
                    totalAdmittedIntake5 += item.admittedIntake5;
                    totalAICTEApprovedIntake5 += item.AICTEapprovedIntake5;
                    if (PgDegreeIds.Contains(item.degreeID))
                        totalExaminationBranchAdmittedIntake5 += item.admittedIntake5;
                    else
                        totalExaminationBranchAdmittedIntake5 += item.ExambranchadmittedIntakeR5;
                    //totalExaminationBranchAdmittedIntake5 += item.ExambranchadmittedIntakeR5;

                    strExistingIntakeDetails += "<tr>";
                    strExistingIntakeDetails += "<td colspan='1' width='28'><p align='center'>" + sno + "</p></td>";
                    strExistingIntakeDetails += "<td colspan='3' width='56'>" + item.Degree + "</td>";
                    strExistingIntakeDetails += "<td colspan='4' width='63'>" + item.Department + "</td>";
                    strExistingIntakeDetails += "<td colspan='4' width='200'>" + item.Specialization + "</td>";
                    strExistingIntakeDetails += "<td colspan='1' width='42'>" + item.Shift + "</td>";
                    strExistingIntakeDetails += "<td colspan='1' width='50'>" + item.AICTEapprovedIntake3.ToString() + "</td>";
                    strExistingIntakeDetails += "<td colspan='1' width='50'>" + item.approvedIntake3.ToString() + "</td>";
                    //if (PgDegreeIds.Contains(item.degreeID))
                    strExistingIntakeDetails += "<td colspan='1' width='50'>" + item.admittedIntake3.ToString() + "</td>";
                    //else
                    //strExistingIntakeDetails += "<td colspan='1' width='50'>" + item.ExambranchadmittedIntakeR3.ToString() + "</td>";
                    strExistingIntakeDetails += "<td colspan='1' width='50'>" + item.AICTEapprovedIntake2.ToString() + "</td>";
                    strExistingIntakeDetails += "<td colspan='1' width='50'>" + item.approvedIntake2.ToString() + "</td>";
                    //if (PgDegreeIds.Contains(item.degreeID))
                    strExistingIntakeDetails += "<td colspan='1' width='50'>" + item.admittedIntake2.ToString() + "</td>";
                    //else
                    //strExistingIntakeDetails += "<td colspan='1' width='50'>" + item.ExambranchadmittedIntakeR2.ToString() + "</td>";
                    strExistingIntakeDetails += "<td colspan='1' width='50'>" + item.AICTEapprovedIntake1.ToString() + "</td>";
                    strExistingIntakeDetails += "<td colspan='1' width='42'>" + item.approvedIntake1.ToString() + "</td>";
                    //if (PgDegreeIds.Contains(item.degreeID))
                    strExistingIntakeDetails += "<td colspan='1' width='50'>" + item.admittedIntake1.ToString() + "</td>";
                    //else
                    //strExistingIntakeDetails += "<td colspan='1' width='50'>" + item.ExambranchadmittedIntakeR1.ToString() + "</td>";
                    //strExistingIntakeDetails += "<td colspan='1'>" + item.ProposedIntake + "</td>";
                    //strExistingIntakeDetails += "<td colspan='1'>" + item.courseStatus + "</td>";
                    //strExistingIntakeDetails += "<td colspan='1'></td>";
                    //strExistingIntakeDetails += "<td colspan='2' style='font-size: 8px;' width='50'>" + item.nbaFromDate + "</td>";
                    //strExistingIntakeDetails += "<td colspan='2' style='font-size: 8px;' width='50'>" + item.nbaToDate + "</td>";
                    strExistingIntakeDetails += "</tr>";
                    if (item.Degree == "Pharm.D PB")//6
                    {
                        totalAICTEApproved += item.AICTEapprovedIntake1 + item.AICTEapprovedIntake2 + item.AICTEapprovedIntake3 + item.AICTEapprovedIntake4 + item.AICTEapprovedIntake5 + item.AICTEapprovedIntake6;
                        totalAdmited += item.admittedIntake1 + item.admittedIntake2 + item.admittedIntake3 + item.admittedIntake4 + item.admittedIntake5 + item.admittedIntake6;
                        totalApproved += item.approvedIntake1 + item.approvedIntake2 + item.approvedIntake3 + item.approvedIntake4 + item.approvedIntake5 + item.approvedIntake6;
                        totalExaminationBranchAdmittedIntake += item.admittedIntake1 + item.admittedIntake2 + item.admittedIntake3 + item.admittedIntake4 + item.admittedIntake5 + item.admittedIntake6;
                        // totalExaminationBranchAdmittedIntake += item.ExambranchadmittedIntakeR1 + item.ExambranchadmittedIntakeR2 + item.ExambranchadmittedIntakeR3 + item.ExambranchadmittedIntakeR4 + item.ExambranchadmittedIntakeR5;
                    }
                    else if (item.Degree == "MAM" || item.Degree == "MTM" || item.Degree == "Pharm.D")//5
                    {
                        totalAICTEApproved += item.AICTEapprovedIntake1 + item.AICTEapprovedIntake2 + item.AICTEapprovedIntake3 + item.AICTEapprovedIntake4 + item.AICTEapprovedIntake5;
                        totalAdmited += item.admittedIntake1 + item.admittedIntake2 + item.admittedIntake3 + item.admittedIntake4 + item.admittedIntake5;
                        totalApproved += item.approvedIntake1 + item.approvedIntake2 + item.approvedIntake3 + item.approvedIntake4 + item.approvedIntake5;
                        totalExaminationBranchAdmittedIntake += item.admittedIntake1 + item.admittedIntake2 + item.admittedIntake3 + item.admittedIntake4 + item.admittedIntake5;
                        //totalExaminationBranchAdmittedIntake += item.ExambranchadmittedIntakeR1 + item.ExambranchadmittedIntakeR2 + item.ExambranchadmittedIntakeR3 + item.ExambranchadmittedIntakeR4 + item.ExambranchadmittedIntakeR5;
                    }
                    else if (item.Degree == "B.Tech" || item.Degree == "B.Pharmacy")//4
                    {
                        totalAICTEApproved += item.AICTEapprovedIntake1 + item.AICTEapprovedIntake2 + item.AICTEapprovedIntake3 + item.AICTEapprovedIntake4;
                        totalAdmited += item.admittedIntake1 + item.admittedIntake2 + item.admittedIntake3 + item.admittedIntake4;
                        totalApproved += item.approvedIntake1 + item.approvedIntake2 + item.approvedIntake3 + item.approvedIntake4;
                        totalExaminationBranchAdmittedIntake += item.ExambranchadmittedIntakeR1 + item.ExambranchadmittedIntakeR2 + item.ExambranchadmittedIntakeR3 + item.ExambranchadmittedIntakeR4;
                    }
                    else if (item.Degree == "MCA")//3
                    {
                        totalAICTEApproved += item.AICTEapprovedIntake1 + item.AICTEapprovedIntake2 + item.AICTEapprovedIntake3;
                        totalAdmited += item.admittedIntake1 + item.admittedIntake2 + item.admittedIntake3;
                        totalApproved += item.approvedIntake1 + item.approvedIntake2 + item.approvedIntake3;
                        totalExaminationBranchAdmittedIntake += item.admittedIntake1 + item.admittedIntake2 + item.admittedIntake3;
                        //totalExaminationBranchAdmittedIntake += item.ExambranchadmittedIntakeR1 + item.ExambranchadmittedIntakeR2 + item.ExambranchadmittedIntakeR3;
                    }
                    else if (item.Degree == "M.Tech" || item.Degree == "M.Pharmacy" || item.Degree == "MBA") //2
                    {
                        totalAICTEApproved += item.AICTEapprovedIntake1 + item.AICTEapprovedIntake2;
                        totalAdmited += item.admittedIntake1 + item.admittedIntake2;
                        totalApproved += item.approvedIntake1 + item.approvedIntake2;
                        totalExaminationBranchAdmittedIntake += item.admittedIntake1 + item.admittedIntake2;
                        //totalExaminationBranchAdmittedIntake += item.ExambranchadmittedIntakeR1 + item.ExambranchadmittedIntakeR2;
                    }
                }

                strExistingIntakeDetails += "<tr>";
                strExistingIntakeDetails += "<td width='337' colspan='13'><p align='right'>Total =</p></td>";
                strExistingIntakeDetails += "<td width='50' colspan='1' align='center'>" + totalAICTEApprovedIntake3 + "</td>";
                strExistingIntakeDetails += "<td width='50' colspan='1' align='center'>" + totalApprovedIntake3 + "</td>";
                //strExistingIntakeDetails += "<td width='50' colspan='1' align='center'>" + totalAdmittedIntake3 + "</td>";
                strExistingIntakeDetails += "<td width='50' colspan='1' align='center'>" + totalExaminationBranchAdmittedIntake3 + "</td>";
                strExistingIntakeDetails += "<td width='50' colspan='1' align='center'>" + totalAICTEApprovedIntake2 + "</td>";
                strExistingIntakeDetails += "<td width='50' colspan='1' align='center'>" + totalApprovedIntake2 + "</td>";
                // strExistingIntakeDetails += "<td width='50' colspan='1' align='center'>" + totalAdmittedIntake2 + "</td>";
                strExistingIntakeDetails += "<td width='50' colspan='1' align='center'>" + totalExaminationBranchAdmittedIntake2 + "</td>";
                strExistingIntakeDetails += "<td width='50' colspan='1' align='center'>" + totalAICTEApprovedIntake1 + "</td>";
                strExistingIntakeDetails += "<td width='50' colspan='1' align='center'>" + totalApprovedIntake1 + "</td>";
                //strExistingIntakeDetails += "<td width='50' colspan='1' align='center'>" + totalAdmittedIntake1 + "</td>";
                strExistingIntakeDetails += "<td width='50' colspan='1' align='center'>" + totalExaminationBranchAdmittedIntake1 + "</td>";
                //strExistingIntakeDetails += "<td width='50' colspan='1' align='center'>" + totalPAYApproved + "</td>";
                //strExistingIntakeDetails += "<td width='50' colspan='1' align='center'>" + totalPAYProposed + "</td>";
                //strExistingIntakeDetails += "<td width='50' colspan='1' valign='top' align='center'></td>";
                //strExistingIntakeDetails += "<td width='50' colspan='2' valign='top' align='center'></td>";
                //strExistingIntakeDetails += "<td width='50' colspan='2' valign='top' align='center'></td>";
                strExistingIntakeDetails += "</tr>";
                strExistingIntakeDetails += "<tr><td colspan='13'><p align='right'>Total Admitted / Total Sanctioned =</p></td><td colspan='11' width='600'>" + totalExaminationBranchAdmittedIntake + '/' + totalApproved + "</td></tr>";
                strExistingIntakeDetails += "<tr><td  colspan='13'><p align='right'>Total AICTE ApprovedIntake</p></td><td colspan='11' width='600'>" + totalAICTEApproved + "</td></tr>";
                // strExistingIntakeDetails += "<tr><td  colspan='13'><p align='right'>Total Examiniation Branch Addmitted Intake</p></td><td colspan='18' width='600'>" + totalExaminationBranchAdmittedIntake + "</td></tr>";
                strExistingIntakeDetails += "</tbody></table>";
                contents = contents.Replace("##ExistingIntakeDetails##", strExistingIntakeDetails);
                //List<IElement> parsedHtmlElements = HTMLWorker.ParseToList(new StringReader(contents), null);
                return contents;
            }
        }

        public string AcademicPerformanceDetails(int collegeId, string contents)
        {
            using (var db = new UAAAS.Models.uaaasDBContext())
            {
                string strAcademicPerformanceDetails = string.Empty;
                int sno = 0;
                int totalAppearedStudents1 = 0;
                int totalPassedStudents1 = 0;
                decimal totalPassPercentage1 = 0;
                int totalAppearedStudents2 = 0;
                int totalPassedStudents2 = 0;
                decimal totalPassPercentage2 = 0;
                int totalAppearedStudents3 = 0;
                int totalPassedStudents3 = 0;
                decimal totalPassPercentage3 = 0;
                int totalAppearedStudents4 = 0;
                int totalPassedStudents4 = 0;
                decimal totalPassPercentage4 = 0;

                int ugAppearedStudents1 = 0;
                int ugPassedStudents1 = 0;
                decimal ugPassPercentage1 = 0;

                int ugAppearedStudents2 = 0;
                int ugPassedStudents2 = 0;
                decimal ugPassPercentage2 = 0;

                int ugAppearedStudents3 = 0;
                int ugPassedStudents3 = 0;
                decimal ugPassPercentage3 = 0;

                int ugAppearedStudents4 = 0;
                int ugPassedStudents4 = 0;
                decimal ugPassPercentage4 = 0;

                int pgAppearedStudents1 = 0;
                int pgPassedStudents1 = 0;
                decimal pgPassPercentage1 = 0;

                int pgAppearedStudents2 = 0;
                int pgPassedStudents2 = 0;
                decimal pgPassPercentage2 = 0;

                int pgAppearedStudents3 = 0;
                int pgPassedStudents3 = 0;
                decimal pgPassPercentage3 = 0;

                int pgAppearedStudents4 = 0;
                int pgPassedStudents4 = 0;
                decimal pgPassPercentage4 = 0;
                strAcademicPerformanceDetails += "<table border='1' cellspacing='0' cellpadding='5'><tbody>";
                IQueryable<jntuh_academic_year> jntuh_academic_year = db.jntuh_academic_year.Select(y => y);
                string AcademicYear = jntuh_academic_year.Where(a => a.isActive && a.isPresentAcademicYear).Select(a => a.academicYear).FirstOrDefault();
                int actualYear = jntuh_academic_year.Where(a => a.isActive && a.isPresentAcademicYear).Select(a => a.actualYear).FirstOrDefault();
                int AYID = jntuh_academic_year.Where(a => a.actualYear == actualYear).Select(a => a.id).FirstOrDefault();
                strAcademicPerformanceDetails += "<tr>";
                strAcademicPerformanceDetails += "<td width='40' rowspan='3' colspan='2'><p align='center'>S. No</p></td>";
                strAcademicPerformanceDetails += "<td width='78' rowspan='3' colspan='3'><p align='left'>Degree</p><p align='left'>*</p></td>";
                strAcademicPerformanceDetails += "<td width='59' rowspan='3' colspan='4'><p align='left'>Department</p><p align='left'>**</p></td>";
                strAcademicPerformanceDetails += "<td width='148' rowspan='3' colspan='5' align='left'><p align='left'>Specialization</p><p align='left'>***</p></td>";
                strAcademicPerformanceDetails += "<td width='42' rowspan='3' colspan='2'><p align='center'>Shift</p><p align='center'>#</p></td>";
                strAcademicPerformanceDetails += "<td width='673' colspan='24'><p align='center'>Academic Performance during the Year " + AcademicYear + "</p></td>";
                strAcademicPerformanceDetails += "</tr>";
                strAcademicPerformanceDetails += "<tr>";
                strAcademicPerformanceDetails += "<td width='168' colspan='6'><p align='center'>FOURTH YEAR</p></td>";
                strAcademicPerformanceDetails += "<td width='168' colspan='6'><p align='center'>THIRD YEAR</p></td>";
                strAcademicPerformanceDetails += "<td width='168' colspan='6'><p align='center'>SECOND YEAR</p></td>";
                strAcademicPerformanceDetails += "<td width='169' colspan='6'><p align='center'>FIRST YEAR</p></td>";
                strAcademicPerformanceDetails += "</tr>";
                strAcademicPerformanceDetails += "<tr style='font-size: 8px; line-height: 9px;'>";
                strAcademicPerformanceDetails += "<td width='60' colspan='2'><p align='center'>A</p></td>";
                strAcademicPerformanceDetails += "<td width='60' colspan='2'><p align='center'>P</p></td>";
                strAcademicPerformanceDetails += "<td width='48' colspan='2'><p align='center'>%</p></td>";
                strAcademicPerformanceDetails += "<td width='60' colspan='2'><p align='center'>A</p></td>";
                strAcademicPerformanceDetails += "<td width='60' colspan='2'><p align='center'>P</p></td>";
                strAcademicPerformanceDetails += "<td width='48' colspan='2'><p align='center'>%</p></td>";
                strAcademicPerformanceDetails += "<td width='60' colspan='2'><p align='center'>A</p></td>";
                strAcademicPerformanceDetails += "<td width='60' colspan='2'><p align='center'>P</p></td>";
                strAcademicPerformanceDetails += "<td width='48' colspan='2'><p align='center'>%</p></td>";
                strAcademicPerformanceDetails += "<td width='60' colspan='2'><p align='center'>A</p></td>";
                strAcademicPerformanceDetails += "<td width='60' colspan='2'><p align='center'>P</p></td>";
                strAcademicPerformanceDetails += "<td width='49' colspan='2'><p align='center'>%</p></td>";
                strAcademicPerformanceDetails += "</tr>";
                List<jntuh_college_academic_performance> performance = db.jntuh_college_academic_performance.Where(i => i.collegeId == collegeId).ToList();

                List<CollegeAcademicPerformance> collegeAcademicPerformance = new List<CollegeAcademicPerformance>();
                var specializationIds = performance.Select(it => it.specializationId).ToList();
                var jntuh_specializations = db.jntuh_specialization.Where(s => specializationIds.Contains(s.id))
                    .Select(it => new { it.id, it.specializationName, it.departmentId }).ToList();
                var departmentIds = jntuh_specializations.Select(it => it.departmentId).ToList();
                var jntuh_departments = db.jntuh_department.Where(d => departmentIds.Contains(d.id))
                    .Select(it => new { it.id, it.departmentName, it.degreeId }).ToList();
                var degreeIds = jntuh_departments.Select(it1 => it1.degreeId).ToList();
                var jntuh_degrees = db.jntuh_degree.Where(it => degreeIds.Contains(it.id))
                    .Select(it => new { it.id, it.degree, it.degreeDisplayOrder }).ToList();
                var shiftIds = performance.Select(p => p.shiftId).ToList();
                var jntuh_shifts = db.jntuh_shift.Where(it => shiftIds.Contains(it.id)).Select(it => new { it.id, it.shiftName }).ToList();
                foreach (var item in performance)
                {
                    CollegeAcademicPerformance newPerformance = new CollegeAcademicPerformance();
                    newPerformance.id = item.id;
                    newPerformance.collegeId = item.collegeId;
                    newPerformance.academicYearId = item.academicYearId;
                    newPerformance.shiftId = item.shiftId;
                    newPerformance.isActive = item.isActive;
                    newPerformance.specializationId = item.specializationId;
                    newPerformance.Specialization = jntuh_specializations.Where(s => s.id == item.specializationId).Select(s => s.specializationName).FirstOrDefault();
                    newPerformance.DepartmentID = jntuh_specializations.Where(s => s.id == item.specializationId).Select(s => s.departmentId).FirstOrDefault();
                    newPerformance.Department = jntuh_departments.Where(d => d.id == newPerformance.DepartmentID).Select(d => d.departmentName).FirstOrDefault();
                    newPerformance.degreeID = jntuh_departments.Where(d => d.id == newPerformance.DepartmentID).Select(d => d.degreeId).FirstOrDefault();
                    newPerformance.Degree = jntuh_degrees.Where(d => d.id == newPerformance.degreeID).Select(d => d.degree).FirstOrDefault();
                    newPerformance.degreeDisplayOrder = jntuh_degrees.Where(d => d.id == newPerformance.degreeID).Select(d => d.degreeDisplayOrder).FirstOrDefault();
                    newPerformance.Shift = jntuh_shifts.Where(s => s.id == item.shiftId).Select(s => s.shiftName).FirstOrDefault();
                    collegeAcademicPerformance.Add(newPerformance);
                }

                collegeAcademicPerformance = collegeAcademicPerformance.AsEnumerable().GroupBy(r => new { r.specializationId, r.shiftId }).Select(r => r.FirstOrDefault()).ToList();
                collegeAcademicPerformance = collegeAcademicPerformance.OrderBy(ei => ei.degreeDisplayOrder).ThenBy(ei => ei.Department).ThenBy(ei => ei.Specialization).ThenBy(ei => ei.shiftId).ToList();

                foreach (var item in collegeAcademicPerformance)
                {
                    sno++;
                    item.appearedStudents1 = Convert.ToInt32(GetDetails(collegeId, AYID, item.specializationId, item.shiftId, 1, 1));
                    item.passedStudents1 = Convert.ToInt32(GetDetails(collegeId, AYID, item.specializationId, item.shiftId, 1, 2));
                    item.passPercentage1 = Convert.ToDecimal(GetDetails(collegeId, AYID, item.specializationId, item.shiftId, 1, 3));
                    totalAppearedStudents1 += item.appearedStudents1;
                    totalPassedStudents1 += item.passedStudents1;

                    item.appearedStudents2 = Convert.ToInt32(GetDetails(collegeId, AYID, item.specializationId, item.shiftId, 2, 1));
                    item.passedStudents2 = Convert.ToInt32(GetDetails(collegeId, AYID, item.specializationId, item.shiftId, 2, 2));
                    item.passPercentage2 = Convert.ToDecimal(GetDetails(collegeId, AYID, item.specializationId, item.shiftId, 2, 3));
                    totalAppearedStudents2 += item.appearedStudents2;
                    totalPassedStudents2 += item.passedStudents2;

                    item.appearedStudents3 = Convert.ToInt32(GetDetails(collegeId, AYID, item.specializationId, item.shiftId, 3, 1));
                    item.passedStudents3 = Convert.ToInt32(GetDetails(collegeId, AYID, item.specializationId, item.shiftId, 3, 2));
                    item.passPercentage3 = Convert.ToDecimal(GetDetails(collegeId, AYID, item.specializationId, item.shiftId, 3, 3));
                    totalAppearedStudents3 += item.appearedStudents3;
                    totalPassedStudents3 += item.passedStudents3;

                    item.appearedStudents4 = Convert.ToInt32(GetDetails(collegeId, AYID, item.specializationId, item.shiftId, 4, 1));
                    item.passedStudents4 = Convert.ToInt32(GetDetails(collegeId, AYID, item.specializationId, item.shiftId, 4, 2));
                    item.passPercentage4 = Convert.ToDecimal(GetDetails(collegeId, AYID, item.specializationId, item.shiftId, 4, 3));
                    totalAppearedStudents4 += item.appearedStudents4;
                    totalPassedStudents4 += item.passedStudents4;

                    if (item.Degree == "B.Tech" || item.Degree == "B.Pharmacy")
                    {
                        ugAppearedStudents1 += item.appearedStudents1;
                        ugAppearedStudents2 += item.appearedStudents2;
                        ugAppearedStudents3 += item.appearedStudents3;
                        ugAppearedStudents4 += item.appearedStudents4;

                        ugPassedStudents1 += item.passedStudents1;
                        ugPassedStudents2 += item.passedStudents2;
                        ugPassedStudents3 += item.passedStudents3;
                        ugPassedStudents4 += item.passedStudents4;

                    }
                    else
                    {
                        pgAppearedStudents1 += item.appearedStudents1;
                        pgAppearedStudents2 += item.appearedStudents2;
                        pgAppearedStudents3 += item.appearedStudents3;
                        pgAppearedStudents4 += item.appearedStudents4;

                        pgPassedStudents1 += item.passedStudents1;
                        pgPassedStudents2 += item.passedStudents2;
                        pgPassedStudents3 += item.passedStudents3;
                        pgPassedStudents4 += item.passedStudents4;

                    }

                    strAcademicPerformanceDetails += "<tr><td width='40' colspan='2'><p align='center'>" + sno + "</p></td><td width='78' colspan='3'>" + item.Degree + "</td><td width='59' colspan='4'>" + item.Department + "</td><td width='148' colspan='5'>" + item.Specialization + "</td><td width='42' align='center' colspan='2'>" + item.Shift + "</td><td width='60' align='center' colspan='2'>" + item.appearedStudents4 + "</td><td width='60' align='center' colspan='2'>" + item.passedStudents4 + "</td><td width='48' align='center' colspan='2'>" + item.passPercentage4 + "</td><td width='60' align='center' colspan='2'>" + item.appearedStudents3 + "</td><td width='60' align='center' colspan='2'>" + item.passedStudents3 + " </td><td width='48' align='center' colspan='2'>" + item.passPercentage3 + "</td><td width='60' align='center' colspan='2'>" + item.appearedStudents2 + "</td><td width='60' align='center' colspan='2'>" + item.passedStudents2 + "</td><td width='48' align='center' colspan='2'>" + item.passPercentage2 + "</td><td width='60' align='center' colspan='2'>" + item.appearedStudents1 + "</td><td width='60' align='center' colspan='2'>" + item.passedStudents1 + "</td><td width='49' align='center' colspan='2'>" + item.passPercentage1 + "</td></tr>";

                }

                if (totalAppearedStudents1 != 0 && totalPassedStudents1 != 0)
                {
                    totalPassPercentage1 = (Convert.ToDecimal(totalPassedStudents1) / Convert.ToDecimal(totalAppearedStudents1)) * 100;
                    totalPassPercentage1 = Convert.ToDecimal(String.Format("{0:0.00}", totalPassPercentage1));
                }
                else
                {
                    totalPassPercentage1 = Convert.ToDecimal("0.00");
                }
                if (totalAppearedStudents2 != 0 && totalPassedStudents2 != 0)
                {
                    totalPassPercentage2 = (Convert.ToDecimal(totalPassedStudents2) / Convert.ToDecimal(totalAppearedStudents2)) * 100;
                    totalPassPercentage2 = Convert.ToDecimal(String.Format("{0:0.00}", totalPassPercentage2));
                }
                else
                {
                    totalPassPercentage2 = Convert.ToDecimal("0.00");
                }
                if (totalAppearedStudents3 != 0 && totalPassedStudents3 != 0)
                {
                    totalPassPercentage3 = (Convert.ToDecimal(totalPassedStudents3) / Convert.ToDecimal(totalAppearedStudents3)) * 100;
                    totalPassPercentage3 = Convert.ToDecimal(String.Format("{0:0.00}", totalPassPercentage3));
                }
                else
                {
                    totalPassPercentage3 = Convert.ToDecimal("0.00");
                }
                if (totalAppearedStudents4 != 0 && totalPassedStudents4 != 0)
                {
                    totalPassPercentage4 = (Convert.ToDecimal(totalPassedStudents4) / Convert.ToDecimal(totalAppearedStudents4)) * 100;
                    totalPassPercentage4 = Convert.ToDecimal(String.Format("{0:0.00}", totalPassPercentage4));
                }
                else
                {
                    totalPassPercentage4 = Convert.ToDecimal("0.00");
                }
                if (ugAppearedStudents1 != 0 && ugPassedStudents1 != 0)
                {
                    ugPassPercentage1 = (Convert.ToDecimal(ugPassedStudents1) / Convert.ToDecimal(ugAppearedStudents1)) * 100;
                    ugPassPercentage1 = Convert.ToDecimal(String.Format("{0:0.00}", ugPassPercentage1));
                }
                else
                {
                    ugPassPercentage1 = Convert.ToDecimal("0.00");
                }
                if (ugAppearedStudents2 != 0 && ugPassedStudents2 != 0)
                {
                    ugPassPercentage2 = (Convert.ToDecimal(ugPassedStudents2) / Convert.ToDecimal(ugAppearedStudents2)) * 100;
                    ugPassPercentage2 = Convert.ToDecimal(String.Format("{0:0.00}", ugPassPercentage2));
                }
                else
                {
                    ugPassPercentage2 = Convert.ToDecimal("0.00");
                }
                if (ugAppearedStudents3 != 0 && ugPassedStudents3 != 0)
                {
                    ugPassPercentage3 = (Convert.ToDecimal(ugPassedStudents3) / Convert.ToDecimal(ugAppearedStudents3)) * 100;
                    ugPassPercentage3 = Convert.ToDecimal(String.Format("{0:0.00}", ugPassPercentage3));
                }
                else
                {
                    ugPassPercentage3 = Convert.ToDecimal("0.00");
                }
                if (ugAppearedStudents4 != 0 && ugPassedStudents4 != 0)
                {
                    ugPassPercentage4 = (Convert.ToDecimal(ugPassedStudents4) / Convert.ToDecimal(ugAppearedStudents4)) * 100;
                    ugPassPercentage4 = Convert.ToDecimal(String.Format("{0:0.00}", ugPassPercentage4));
                }
                else
                {
                    ugPassPercentage4 = Convert.ToDecimal("0.00");
                }
                if (pgAppearedStudents1 != 0 && pgPassedStudents1 != 0)
                {
                    pgPassPercentage1 = (Convert.ToDecimal(pgPassedStudents1) / Convert.ToDecimal(pgAppearedStudents1)) * 100;
                    pgPassPercentage1 = Convert.ToDecimal(String.Format("{0:0.00}", pgPassPercentage1));
                }
                else
                {
                    pgPassPercentage1 = Convert.ToDecimal("0.00");
                }
                if (pgAppearedStudents2 != 0 && pgPassedStudents2 != 0)
                {
                    pgPassPercentage2 = (Convert.ToDecimal(pgPassedStudents2) / Convert.ToDecimal(pgAppearedStudents2)) * 100;
                    pgPassPercentage2 = Convert.ToDecimal(String.Format("{0:0.00}", pgPassPercentage2));
                }
                else
                {
                    pgPassPercentage2 = Convert.ToDecimal("0.00");
                }
                if (pgAppearedStudents3 != 0 && pgPassedStudents3 != 0)
                {
                    pgPassPercentage3 = (Convert.ToDecimal(pgPassedStudents3) / Convert.ToDecimal(pgAppearedStudents3)) * 100;
                    pgPassPercentage3 = Convert.ToDecimal(String.Format("{0:0.00}", pgPassPercentage3));
                }
                else
                {
                    pgPassPercentage3 = Convert.ToDecimal("0.00");
                }
                if (pgAppearedStudents4 != 0 && pgPassedStudents4 != 0)
                {
                    pgPassPercentage4 = (Convert.ToDecimal(pgPassedStudents4) / Convert.ToDecimal(pgAppearedStudents4)) * 100;
                    pgPassPercentage4 = Convert.ToDecimal(String.Format("{0:0.00}", pgPassPercentage4));
                }
                else
                {
                    pgPassPercentage4 = Convert.ToDecimal("0.00");
                }

                strAcademicPerformanceDetails += "<tr><td width='367' colspan='16'><p align='right'>Total</p></td><td width='60' align='center' colspan='2'>" + totalAppearedStudents4 + "</td><td width='60' align='center' colspan='2'>" + totalPassedStudents4 + "</td><td width='48' align='center' colspan='2'>" + totalPassPercentage4 + "</td><td width='60' align='center' colspan='2'>" + totalAppearedStudents3 + "</td><td width='60' align='center' colspan='2'>" + totalPassedStudents3 + "</td><td width='48' align='center' colspan='2'>" + totalPassPercentage3 + "</td><td width='60' align='center' colspan='2'>" + totalAppearedStudents2 + "</td><td width='60' align='center' colspan='2'>" + totalPassedStudents2 + "</td><td width='48' align='center' colspan='2'>" + totalPassPercentage2 + "</td><td width='60' align='center' colspan='2'>" + totalAppearedStudents1 + "</td><td width='60' align='center' colspan='2'>" + totalPassedStudents1 + "</td><td width='49' align='center' colspan='2'>" + totalPassPercentage1 + "</td></tr>";
                strAcademicPerformanceDetails += "<tr style='font-size: 8px;'><td width='367' colspan='16'><p align='right'>(Pass % Year wise UG)</p></td><td width='168' colspan='6'><p>UG First Year % : " + ugPassPercentage1 + "</p></td><td width='168' colspan='6'><p>UG Second Year % : " + ugPassPercentage2 + "</p></td><td width='168' colspan='6'><p>UG Third Year % : " + ugPassPercentage3 + "</p></td><td width='169' colspan='6'><p>UG Fourth Year % : " + ugPassPercentage4 + "</p></td></tr>";
                strAcademicPerformanceDetails += "<tr style='font-size: 8px;'><td width='367' colspan='16'><p align='right'>(Pass % Year wise PG)</p></td><td width='168' colspan='6'><p>PG First Year % : " + pgPassPercentage1 + "</p></td><td width='168' colspan='6'><p>PG Second Year % : " + pgPassPercentage2 + "</p></td><td width='168' colspan='6'><p>PG Third Year % : " + pgPassPercentage3 + "</p></td><td width='169' colspan='6'></td></tr>";

                strAcademicPerformanceDetails += "</tbody></table>";
                contents = contents.Replace("##AcademicPerformanceDetails##", strAcademicPerformanceDetails);
                //List<IElement> parsedHtmlElements = HTMLWorker.ParseToList(new StringReader(contents), null);
                return contents;
            }
        }

        private string RemedialTeaching(int collegeId, string contents)
        {
            string strRemedial = string.Empty;
            strRemedial += "<table  border='1' cellspacing='0' cellpadding='4' style='width:100px;'>";
            strRemedial += "<thead>";
            strRemedial += "<tr>";
            strRemedial += "<th>S.No</th>";
            strRemedial += "<th style='text-align:center'>Description</th>";
            strRemedial += "<th>Status</th>";
            strRemedial += "<th>Remarks</th>";
            strRemedial += "</tr>";
            strRemedial += "</thead><tbody><tr>";

            var masteractivities = db.jntuh_extracurricularactivities.Where(i => i.isactive && i.activitytype == 9).Select(i => i.sno).ToArray();
            if (masteractivities.Count() > 0)
            {
                var collegeExtracurricularactivities = db.jntuh_college_extracurricularactivities.FirstOrDefault(e => masteractivities.Contains(e.activityid) && e.collegeid == collegeId);
                strRemedial += "<td>1</td>";
                strRemedial += "<td>" + db.jntuh_extracurricularactivities.Where(e => e.activitytype == 9).Select(e => e.activitydescription).FirstOrDefault() + "</td>";
                if (collegeExtracurricularactivities != null)
                {
                    string YesorNo = collegeExtracurricularactivities.activitystatus == true ? "Yes" : "No";
                    strRemedial += string.IsNullOrEmpty(YesorNo) ? "<td>--</td>" : "<td>" + YesorNo + "</td>";
                    strRemedial += string.IsNullOrEmpty(collegeExtracurricularactivities.remarks) ? "<td>--</td>" : "<td>" + collegeExtracurricularactivities.remarks + "</td>";
                }
                else
                {
                    strRemedial += "<td>--</td><td>--</td>";
                }
            }
            strRemedial += "</tr></tbody></table>";
            contents = contents.Replace("##RemedialTeachingDetails##", strRemedial);
            return contents;
        }

        private string FacultyInformation(int collegeId, string contents)
        {
            int sno = 1;
            string strFacultyInfo = string.Empty;
            strFacultyInfo += "<table border='1' cellspacing='0' cellpadding='5'>";
            strFacultyInfo += "<thead>";
            strFacultyInfo += "<tr>";
            strFacultyInfo += "<th width='250' colspan='10'>S.No</th>";
            strFacultyInfo += "<th width='250' colspan='10'>Academic Year</th>";
            strFacultyInfo += "<th width='250' colspan='10'>Degree</th>";
            strFacultyInfo += "<th width='250' colspan='10'>Staff: Student Ratio</th>";
            strFacultyInfo += "<th width='250' colspan='10'>Total No. of Faculty on Role</th>";
            strFacultyInfo += "<th width='250' colspan='10'>Total No. of Faculty Terminated / Resigned</th>";
            strFacultyInfo += "<th width='250' colspan='10'>Retention Percentage</th>";
            strFacultyInfo += "<th width='250' colspan='10'>Pay scale implemented</th>";
            strFacultyInfo += "<th width='250' colspan='10'>Assistant Prof. Pay scale</th>";
            strFacultyInfo += "<th width='250' colspan='10'>Assistant Prof. Pay</th>";
            strFacultyInfo += "<th width='250' colspan='10'>Associate Prof. Pay scale</th>";
            strFacultyInfo += "<th width='250' colspan='10'>Associate Prof. Pay</th>";
            strFacultyInfo += "<th width='250' colspan='10'>Professor Pay scale</th>";
            strFacultyInfo += "<th width='250' colspan='10'>Professor Pay</th>";
            strFacultyInfo += "<th width='250' colspan='10'>Oppurtunities Provided to faculty</th>";
            strFacultyInfo += "</tr>";
            strFacultyInfo += "</thead><tbody>";

            var facultyList = db.jntuh_college_faculty_information.Where(c => c.collegeid == collegeId && c.isactive == true).OrderByDescending(c => c.academicyear).ToList();
            foreach (var item in facultyList)
            {
                strFacultyInfo += "<tr>";
                strFacultyInfo += "<td width='250' colspan='10'>" + sno + "</td>";
                strFacultyInfo += "<td width='250' colspan='10'>" + db.jntuh_academic_year.Where(a => a.id == item.academicyear).Select(a => a.academicYear).FirstOrDefault() + "</td>";
                strFacultyInfo += "<td width='250' colspan='10'>" + db.jntuh_degree.Where(a => a.id == item.degree).Select(a => a.degree).FirstOrDefault() + "</td>";
                strFacultyInfo += "<td width='250' colspan='10'>" + item.staff + " : " + item.student + "</td>";
                strFacultyInfo += "<td width='250' colspan='10'>" + item.numberoffacultyonrole + "</td>";
                strFacultyInfo += "<td width='250' colspan='10'>" + item.numberoffacultyterminatedorregined + "</td>";
                strFacultyInfo += "<td width='250' colspan='10'>" + item.retentionpercentage + "</td>";
                strFacultyInfo += item.payscaleimplemented ? "<td width='250' colspan='10'>Yes</td>" : "<td width='250' colspan='10'>No</td>";
                strFacultyInfo += string.IsNullOrEmpty(item.asstprofpayscale) ? "<td width='250' colspan='10'>NA</td>" : "<td width='250' colspan='10'>" + item.asstprofpayscale + "</td>";
                strFacultyInfo += string.IsNullOrEmpty(item.asstprofpay) ? "<td width='250' colspan='10'>NA</td>" : "<td width='250' colspan='10'>" + item.asstprofpay + "</td>";
                strFacultyInfo += string.IsNullOrEmpty(item.assocprofpayscale) ? "<td width='250' colspan='10'>NA</td>" : "<td width='250' colspan='10'>" + item.assocprofpayscale + "</td>";
                strFacultyInfo += string.IsNullOrEmpty(item.assocprofpay) ? "<td width='250' colspan='10'>NA</td>" : "<td width='250' colspan='10'>" + item.assocprofpay + "</td>";
                strFacultyInfo += string.IsNullOrEmpty(item.profpayscale) ? "<td width='250' colspan='10'>NA</td>" : "<td width='250' colspan='10'>" + item.profpayscale + "</td>";
                strFacultyInfo += string.IsNullOrEmpty(item.profpays) ? "<td width='250' colspan='10'>NA</td>" : "<td width='250' colspan='10'>" + item.profpays + "</td>";
                strFacultyInfo += Convert.ToBoolean(item.oppurtunities) ? "<td width='250' colspan='10'>Yes</td>" : "<td width='250' colspan='10'>No</td>";
                strFacultyInfo += "</tr>";
                sno++;
            }
            strFacultyInfo += "</tbody></table>";
            contents = contents.Replace("##FacultyInformation##", strFacultyInfo);
            return contents;
        }

        private string FinancialStandards(int collegeId, string contents)
        {
            int sno = 1;
            string strFinStand = string.Empty;

            strFinStand += "<table border='1' cellspacing='0' cellpadding='5'>";
            strFinStand += "<thead>";
            strFinStand += "<tr>";
            strFinStand += "<th>S.No</th>";
            strFinStand += "<th>Academic Year</th>";
            strFinStand += "<th>Degree</th>";
            strFinStand += "<th>Number of Students</th>";
            strFinStand += "<th>Sanctioned Amount (In Lakhs.)</th>";
            strFinStand += "</tr>";
            strFinStand += "</thead><tbody>";

            var finStandsList = db.jntuh_college_financialstandards.Where(c => c.collegeid == collegeId && c.isactive == true).OrderByDescending(c => c.academicyear).ToList();

            foreach (var item in finStandsList)
            {
                strFinStand += "<tr>";
                strFinStand += "<td>" + sno + "</td>";
                strFinStand += "<td>" + db.jntuh_academic_year.Where(a => a.id == item.academicyear).Select(a => a.academicYear).FirstOrDefault() + "</td>";
                strFinStand += "<td>" + db.jntuh_degree.Where(a => a.id == item.degree).Select(a => a.degree).FirstOrDefault() + "</td>";
                strFinStand += "<td>" + item.noofstudents + "</td>";
                strFinStand += "<td>" + item.totalamount + "</td>";
                strFinStand += "</tr>";
                sno++;
            }

            strFinStand += "</tbody></table>";
            contents = contents.Replace("##FinancialStandardsInformation##", strFinStand);
            return contents;
        }

        private string CourtCases(int collegeId, string contents)
        {
            int sno = 1;
            string strCourtCases = string.Empty;

            strCourtCases += "<table border='1' cellspacing='0' cellpadding='5'>";
            strCourtCases += "<thead>";
            strCourtCases += "<tr>";
            strCourtCases += "<th>S.No</th>";
            strCourtCases += "<th>WP / SL / other No.</th>";
            strCourtCases += "<th>Year of filing</th>";
            strCourtCases += "<th>Prayer of the Petitioner</th>";
            strCourtCases += "<th>If JNTUH is one of the respondents, mention the position</th>";
            strCourtCases += "</tr>";
            strCourtCases += "</thead><tbody>";

            var courtCasesList = db.jntuh_college_courtcases.Where(c => c.collegeid == collegeId && c.isactive == true).OrderByDescending(c => c.yearoffilling).ToList();
            foreach (var item in courtCasesList)
            {
                strCourtCases += "<tr>";
                strCourtCases += "<td>" + sno + "</td>";
                strCourtCases += "<td>" + item.wporslorotherno + "</td>";
                strCourtCases += "<td>" + db.jntuh_academic_year.Where(a => a.id == item.yearoffilling).Select(a => a.academicYear).FirstOrDefault() + "</td>";
                strCourtCases += "<td>" + item.prayerofthepetitioner + "</td>";
                strCourtCases += "<td>" + item.respondents + "</td>";
                strCourtCases += "</tr>";
                sno++;
            }

            strCourtCases += "</tbody></table>";
            contents = contents.Replace("##CourtCasesInformation##", strCourtCases);
            return contents;
        }

        private string BooksList(int collegeId, string contents)
        {
            int sno = 1;
            string strBooks = string.Empty;

            strBooks += "<table border='1' cellspacing='0' cellpadding='5'>";
            strBooks += "<thead>";
            strBooks += "<tr>";
            strBooks += "<th>S.No</th>";
            strBooks += "<th>Degree</th>";
            strBooks += "<th>Academic Year</th>";
            strBooks += "<th>Number of Books</th>";
            strBooks += "<th>Amount spent</th>";

            var booksJournalsList = db.jntuh_college_booksandjournals.Where(c => c.collegeid == collegeId && c.isactive == true && c.essentialtype == 1).OrderByDescending(c => c.academicyearId).ToList();

            foreach (var item in booksJournalsList)
            {
                strBooks += "<tr>";
                strBooks += "<td>" + sno + "</td>";
                strBooks += "<td>" + db.jntuh_degree.Where(a => a.id == item.degreeid).Select(a => a.degree).FirstOrDefault() + "</td>";
                strBooks += "<td>" + db.jntuh_academic_year.Where(a => a.id == item.academicyearId).Select(a => a.academicYear).FirstOrDefault() + "</td>";
                strBooks += "<td>" + item.numberofbooks + "</td>";
                strBooks += "<td>" + item.amountspent + "</td>";
                strBooks += "</tr>";
                sno++;
            }

            strBooks += "</tbody></table>";
            contents = contents.Replace("##BooksList##", strBooks);
            return contents;
        }

        private string JournalsList(int collegeId, string contents)
        {
            int sno = 1;
            string strJournals = string.Empty;

            strJournals += "<table border='1' cellspacing='0' cellpadding='5'>";
            strJournals += "<thead>";
            strJournals += "<tr>";
            strJournals += "<th>S.No</th>";
            strJournals += "<th>Degree</th>";
            strJournals += "<th>Academic Year</th>";
            strJournals += "<th>Number of Journals</th>";
            strJournals += "<th>Amount spent</th>";

            var booksJournalsList = db.jntuh_college_booksandjournals.Where(c => c.collegeid == collegeId && c.isactive == true && c.essentialtype == 2).OrderByDescending(c => c.academicyearId).ToList();

            foreach (var item in booksJournalsList)
            {
                strJournals += "<tr>";
                strJournals += "<td>" + sno + "</td>";
                strJournals += "<td>" + db.jntuh_degree.Where(a => a.id == item.degreeid).Select(a => a.degree).FirstOrDefault() + "</td>";
                strJournals += "<td>" + db.jntuh_academic_year.Where(a => a.id == item.academicyearId).Select(a => a.academicYear).FirstOrDefault() + "</td>";
                strJournals += "<td>" + item.numberofbooks + "</td>";
                strJournals += "<td>" + item.amountspent + "</td>";
                strJournals += "</tr>";
                sno++;
            }

            strJournals += "</tbody></table>";
            contents = contents.Replace("##JournalsList##", strJournals);
            return contents;
        }

        private string EBooksList(int collegeId, string contents)
        {
            int sno = 1;
            string strEBooks = string.Empty;

            strEBooks += "<table border='1' cellspacing='0' cellpadding='5'>";
            strEBooks += "<thead>";
            strEBooks += "<tr>";
            strEBooks += "<th>S.No</th>";
            strEBooks += "<th>Degree</th>";
            strEBooks += "<th>Academic Year</th>";
            strEBooks += "<th>Number of e-Books</th>";
            strEBooks += "<th>Number of Computers Added</th>";
            strEBooks += "<th>Amount spent</th>";

            var booksJournalsList = db.jntuh_college_booksandjournals.Where(c => c.collegeid == collegeId && c.isactive == true && c.essentialtype == 3).OrderByDescending(c => c.academicyearId).ToList();

            foreach (var item in booksJournalsList)
            {
                strEBooks += "<tr>";
                strEBooks += "<td>" + sno + "</td>";
                strEBooks += "<td>" + db.jntuh_degree.Where(a => a.id == item.degreeid).Select(a => a.degree).FirstOrDefault() + "</td>";
                strEBooks += "<td>" + db.jntuh_academic_year.Where(a => a.id == item.academicyearId).Select(a => a.academicYear).FirstOrDefault() + "</td>";
                strEBooks += "<td>" + item.numberofbooks + "</td>";
                strEBooks += "<td>" + item.noofcomputers + "</td>";
                strEBooks += "<td>" + item.amountspent + "</td>";
                strEBooks += "</tr>";
                sno++;
            }

            strEBooks += "</tbody></table>";
            contents = contents.Replace("##EBooksList##", strEBooks);
            return contents;
        }

        private string EJournalsList(int collegeId, string contents)
        {
            int sno = 1;
            string strEJournals = string.Empty;

            strEJournals += "<table border='1' cellspacing='0' cellpadding='5'>";
            strEJournals += "<thead>";
            strEJournals += "<tr>";
            strEJournals += "<th>S.No</th>";
            strEJournals += "<th>Degree</th>";
            strEJournals += "<th>Academic Year</th>";
            strEJournals += "<th>Number of e-Journals</th>";
            strEJournals += "<th>Number of Computers Added</th>";
            strEJournals += "<th>Amount spent</th>";

            var booksJournalsList = db.jntuh_college_booksandjournals.Where(c => c.collegeid == collegeId && c.isactive == true && c.essentialtype == 4).OrderByDescending(c => c.academicyearId).ToList();

            foreach (var item in booksJournalsList)
            {
                strEJournals += "<tr>";
                strEJournals += "<td>" + sno + "</td>";
                strEJournals += "<td>" + db.jntuh_degree.Where(a => a.id == item.degreeid).Select(a => a.degree).FirstOrDefault() + "</td>";
                strEJournals += "<td>" + db.jntuh_academic_year.Where(a => a.id == item.academicyearId).Select(a => a.academicYear).FirstOrDefault() + "</td>";
                strEJournals += "<td>" + item.numberofbooks + "</td>";
                strEJournals += "<td>" + item.noofcomputers + "</td>";
                strEJournals += "<td>" + item.amountspent + "</td>";
                strEJournals += "</tr>";
                sno++;
            }

            strEJournals += "</tbody></table>";
            contents = contents.Replace("##EJournalsList##", strEJournals);
            return contents;
        }

        private string EssentialRequirements(int collegeId, string contents)
        {
            var SNO = 1;
            var EssentialRequirements = string.Empty;
            var masteractivities = db.jntuh_extracurricularactivities.Where(i => i.isactive == true && i.activitytype == 15).ToList();
            var extraactivities = db.jntuh_college_extracurricularactivities.Where(i => i.isactive == true && i.collegeid == collegeId).ToList();
            var essentialRequirementsList = (from item in extraactivities
                                             let jntuhExtracurricularactivities = masteractivities.FirstOrDefault(i => i.sno == item.activityid)
                                             where jntuhExtracurricularactivities != null
                                             select new CollegeExtraCirActivities()
                                             {
                                                 activityDesc = jntuhExtracurricularactivities.activitydescription,
                                                 activitystatus = item.activitystatus,
                                                 remarks = item.remarks
                                             }).ToList();

            EssentialRequirements += "<table  border='1' cellspacing='0' cellpadding='4' style='width:100px;'><tbody>";
            EssentialRequirements += " <tr><td colspan='1'><p align='center'>S.No.</p></td><td colspan='12'><p align='center'>Description</p></td><td colspan='3' align='left'><p>Status</p></td><td colspan='3' align='left'><p>Remarks</p></td></tr>";

            if (essentialRequirementsList.Any())
            {
                foreach (var item in essentialRequirementsList)
                {
                    var status = item.activitystatus ? "Yes" : "No";
                    EssentialRequirements += " <tr><td style='vertical-align:top;' colspan='1'><p align='center'>" + SNO + "</p></td><td style='vertical-align:top;' colspan='12' align='left'>" + item.activityDesc + "</td><td colspan='3' align='left'>" + status + "</td><td colspan='3' align='left'>" + item.remarks + "</td></tr>";
                    SNO++;
                }
            }
            else
            {
                EssentialRequirements += " <tr><td colspan='4'><p align='center'>NIL</p></td></tr>";

            }
            EssentialRequirements += "</tbody></table>";
            contents = contents.Replace("##EssentialRequirements##", EssentialRequirements);
            return contents;
        }

        private string DesirableRequirements(int collegeId, string contents)
        {
            var SNO = 1;
            var DesirableRequirements = string.Empty;
            var masteractivities = db.jntuh_extracurricularactivities.Where(i => i.isactive == true && i.activitytype == 16).ToList();
            var extraactivities = db.jntuh_college_extracurricularactivities.Where(i => i.isactive == true && i.collegeid == collegeId).ToList();
            var lstactivities = (from item in extraactivities
                                 let jntuhExtracurricularactivities = masteractivities.FirstOrDefault(i => i.sno == item.activityid)
                                 where jntuhExtracurricularactivities != null
                                 select new CollegeExtraCirActivities()
                                 {
                                     activityDesc = jntuhExtracurricularactivities.activitydescription,
                                     activitystatus = item.activitystatus,
                                     remarks = item.remarks
                                 }).ToList();

            DesirableRequirements += "<table  border='1' cellspacing='0' cellpadding='4' style='width:100px;'><tbody>";
            DesirableRequirements += " <tr><td colspan='1'><p align='center'>S.No.</p></td><td colspan='12'><p align='center'>Description</p></td><td colspan='3' align='left'><p>Status</p></td><td colspan='3' align='left'><p>Remarks</p></td></tr>";

            if (DesirableRequirements.Any())
            {
                foreach (var item in lstactivities)
                {
                    var status = item.activitystatus ? "Yes" : "No";
                    DesirableRequirements += " <tr><td style='vertical-align:top;' colspan='1'><p align='center'>" + SNO + "</p></td><td style='vertical-align:top;' colspan='12' align='left'>" + item.activityDesc + "</td><td colspan='3' align='left'>" + status + "</td><td colspan='3' align='left'>" + item.remarks + "</td></tr>";
                    SNO++;
                }
            }
            else
            {
                DesirableRequirements += " <tr><td colspan='4'><p align='center'>NIL</p></td></tr>";

            }
            DesirableRequirements += "</tbody></table>";
            contents = contents.Replace("##DesirableRequirements##", DesirableRequirements);
            return contents;
        }

        private string AnyOtherInformation(int collegeId, string contents)
        {
            var SNO = 1;
            var AnyOtherInfo = string.Empty;
            var masteractivities = db.jntuh_extracurricularactivities.Where(i => i.isactive == true && i.activitytype == 19).ToList();
            var extraactivities = db.jntuh_college_extracurricularactivities.Where(i => i.isactive == true && i.collegeid == collegeId).ToList();
            var lstactivities = (from item in extraactivities
                                 let jntuhExtracurricularactivities = masteractivities.FirstOrDefault(i => i.sno == item.activityid)
                                 where jntuhExtracurricularactivities != null
                                 select new CollegeExtraCirActivities()
                                 {
                                     activityDesc = jntuhExtracurricularactivities.activitydescription,
                                     activitystatus = item.activitystatus,
                                     remarks = item.remarks
                                 }).ToList();

            AnyOtherInfo += "<table  border='1' cellspacing='0' cellpadding='4' style='width:100px;'><tbody>";
            AnyOtherInfo += " <tr><td colspan='1'><p align='center'>S.No.</p></td><td colspan='12'><p align='center'>Description</p></td><td colspan='3' align='left'><p>Status</p></td><td colspan='3' align='left'><p>Remarks</p></td></tr>";

            if (AnyOtherInfo.Any())
            {
                foreach (var item in lstactivities)
                {
                    var status = item.activitystatus ? "Yes" : "No";
                    AnyOtherInfo += " <tr><td style='vertical-align:top;' colspan='1'><p align='center'>" + SNO + "</p></td><td style='vertical-align:top;' colspan='12' align='left'>" + item.activityDesc + "</td><td colspan='3' align='left'>" + status + "</td><td colspan='3' align='left'>" + item.remarks + "</td></tr>";
                    SNO++;
                }
            }
            else
            {
                AnyOtherInfo += " <tr><td colspan='4'><p align='center'>NIL</p></td></tr>";

            }
            AnyOtherInfo += "</tbody></table>";
            contents = contents.Replace("##AnyOtherInformation##", AnyOtherInfo);
            return contents;
        }

        private string BestPractisesAdopted(int collegeId, string contents)
        {
            var SNO = 1;
            var BestPractisesAdopted = string.Empty;
            var masteractivities = db.jntuh_extracurricularactivities.Where(i => i.isactive == true && i.activitytype == 12).ToList();
            var extraactivities = db.jntuh_college_extracurricularactivities.Where(i => i.isactive == true && i.collegeid == collegeId).ToList();
            var lstactivities = (from item in extraactivities
                                 let jntuhExtracurricularactivities = masteractivities.FirstOrDefault(i => i.sno == item.activityid)
                                 where jntuhExtracurricularactivities != null
                                 select new CollegeExtraCirActivities()
                                 {
                                     activityDesc = jntuhExtracurricularactivities.activitydescription,
                                     activitystatus = item.activitystatus,
                                     remarks = item.remarks
                                 }).ToList();

            BestPractisesAdopted += "<table  border='1' cellspacing='0' cellpadding='4' style='width:100px;'><tbody>";
            BestPractisesAdopted += " <tr><td colspan='1'><p align='center'>S.No.</p></td><td colspan='12'><p align='center'>Description</p></td><td colspan='3' align='left'><p>Status</p></td><td colspan='3' align='left'><p>Remarks</p></td></tr>";

            if (BestPractisesAdopted.Any())
            {
                foreach (var item in lstactivities)
                {
                    var status = item.activitystatus ? "Yes" : "No";
                    BestPractisesAdopted += " <tr><td style='vertical-align:top;' colspan='1'><p align='center'>" + SNO + "</p></td><td style='vertical-align:top;' colspan='12' align='left'>" + item.activityDesc + "</td><td colspan='3' align='left'>" + status + "</td><td colspan='3' align='left'>" + item.remarks + "</td></tr>";
                    SNO++;
                }
            }
            else
            {
                BestPractisesAdopted += " <tr><td colspan='4'><p align='center'>NIL</p></td></tr>";

            }
            BestPractisesAdopted += "</tbody></table>";
            contents = contents.Replace("##BestPractisesAdopted##", BestPractisesAdopted);
            return contents;
        }

        private string FinancialStatus(int collegeId, string contents)
        {
            int sno = 1;
            string FinancialStatus = string.Empty;

            FinancialStatus += "<table border='1' cellspacing='0' cellpadding='5'>";
            FinancialStatus += "<thead>";
            FinancialStatus += "<tr>";
            FinancialStatus += "<th>S.No</th>";
            FinancialStatus += "<th>Name of the Bank</th>";
            FinancialStatus += "<th>Branch</th>";
            FinancialStatus += "<th>Address</th>";
            FinancialStatus += "<th>Account No</th>";
            FinancialStatus += "<th>FDR, if any (Excluding FDR submitted to AICTE)</th>";
            FinancialStatus += "<th>Cash Balance<br />(Rs. in Lakhs)</th>";
            FinancialStatus += "<th>Total</th>";
            FinancialStatus += "</tr>";
            FinancialStatus += "</thead><tbody>";

            List<OperationalFunds> operationalFunds = db.jntuh_college_funds.Where(a => a.collegeId == collegeId).Select(a =>
                                              new OperationalFunds
                                              {
                                                  bankName = a.bankName,
                                                  bankBranch = a.bankBranch,
                                                  bankAddress = a.bankAddress,
                                                  bankAccountNumber = a.bankAccountNumber,
                                                  FDR = a.FDR,
                                                  cashBalance = a.cashBalance,
                                                  total = a.cashBalance + a.FDR,
                                              }).ToList();

            foreach (var item in operationalFunds)
            {
                FinancialStatus += "<tr>";
                FinancialStatus += "<td>" + sno + "</td>";
                FinancialStatus += "<td>" + item.bankName + "</td>";
                FinancialStatus += "<td>" + item.bankBranch + "</td>";
                FinancialStatus += "<td>" + item.bankAddress + "</td>";
                FinancialStatus += "<td>" + item.bankAccountNumber + "</td>";
                FinancialStatus += "<td>" + item.FDR + "</td>";
                FinancialStatus += "<td>" + item.cashBalance + "</td>";
                FinancialStatus += "<td>" + item.total + "</td>";
                FinancialStatus += "</tr>";
                sno++;
            }

            FinancialStatus += "</tbody></table>";
            contents = contents.Replace("##FinancialStatus##", FinancialStatus);
            return contents;
        }

        private string TeachingFaculty(int collegeId, string contents)
        {
            int sno = 1;
            string TeachingFaculty = string.Empty;

            TeachingFaculty += "<table border='1' cellspacing='0' cellpadding='5'>";
            TeachingFaculty += "<thead>";
            TeachingFaculty += "<tr>";
            TeachingFaculty += "<th>S.No</th>";
            TeachingFaculty += "<th>Name</th>";
            TeachingFaculty += "<th>Registration Number</th>";
            TeachingFaculty += "<th>Designation</th>";
            TeachingFaculty += "<th>Department</th>";
            TeachingFaculty += "<th>Specialization</th>";
            TeachingFaculty += "</tr>";
            TeachingFaculty += "</thead><tbody>";

            var TeachingFacultyData = db.jntuh_college_faculty_registered.Join(db.jntuh_registered_faculty,
                    CLGREG => CLGREG.RegistrationNumber, REG => REG.RegistrationNumber,
                    (CLGREG, REG) => new { CLGREG = CLGREG, REG = REG }).Where(e => e.CLGREG.collegeId == collegeId).Select(e => new
                    {
                        e.REG.FirstName,
                        e.REG.MiddleName,
                        e.REG.LastName,
                        e.REG.RegistrationNumber,
                        e.REG.id,
                        collegeid = e.CLGREG.id,
                        depId = e.CLGREG.DepartmentId,
                        Aadhaarno = e.CLGREG.AadhaarNumber,
                        AadhaarDoc = e.CLGREG.AadhaarDocument,
                        specId = e.CLGREG.SpecializationId,
                        IdentfdFor = e.CLGREG.IdentifiedFor,
                        e.REG.DesignationId,
                        e.CLGREG.DepartmentId,
                        e.REG.Absent,
                        e.REG.NotQualifiedAsperAICTE,
                        e.REG.PANNumber,
                        e.REG.NoSCM,
                        e.REG.PHDundertakingnotsubmitted,
                        e.REG.Blacklistfaculy
                    }).ToList();

            var jntuh_departments = db.jntuh_department.AsNoTracking().ToList();
            var jntuh_designations = db.jntuh_designation.AsNoTracking().ToList();
            var jntuh_specializations = db.jntuh_specialization.AsNoTracking().ToList();

            foreach (var data in TeachingFacultyData)
            {
                TeachingFaculty += "<tr>";
                TeachingFaculty += "<td>" + sno + "</td>";
                if (data.Blacklistfaculy == true)
                {
                    TeachingFaculty += "<td>" + data.FirstName + " " + data.MiddleName + " " + data.LastName + "<span style='color: red'>   (Blacklist)</span></td>";
                }
                else
                {
                    TeachingFaculty += "<td>" + data.FirstName + " " + data.MiddleName + " " + data.LastName + "</td>";
                }
                TeachingFaculty += "<td>" + data.RegistrationNumber + "</td>";
                if (data.DesignationId != null)
                {
                    int desigId = (int)data.DesignationId;
                    TeachingFaculty += "<td>" + jntuh_designations.Where(e => e.id == desigId).Select(e => e.designation).FirstOrDefault() + "</td>";
                }
                else
                {
                    TeachingFaculty += "<td></td>";
                }
                if (data.DepartmentId != null)
                {
                    int deptId = (int)data.DepartmentId;
                    TeachingFaculty += "<td>" + jntuh_departments.Where(e => e.id == deptId).Select(e => e.departmentName).FirstOrDefault() + "</td>";
                }
                else
                {
                    TeachingFaculty += "<td></td>";
                }
                if (data.specId != null)
                {
                    int SpecId = (int)data.specId;
                    TeachingFaculty += "<td>" + jntuh_specializations.Where(e => e.id == SpecId).Select(e => e.specializationName).FirstOrDefault() + "</td>";
                }
                else
                {
                    TeachingFaculty += "<td></td>";
                }
                TeachingFaculty += "</tr>";
                sno++;
            }

            TeachingFaculty += "</tbody></table>";
            contents = contents.Replace("##TeachingFaculty##", TeachingFaculty);
            return contents;
        }

        private string FacultyOppurtunities(int collegeId, string contents)
        {
            int sno = 1;
            string FacultyOppurtunities = string.Empty;

            FacultyOppurtunities += "<table border='1' cellspacing='0' cellpadding='5'>";
            FacultyOppurtunities += "<thead>";
            FacultyOppurtunities += "<tr>";
            FacultyOppurtunities += "<th>S.No</th>";
            FacultyOppurtunities += "<th style=''text-align:center'>Opportunity Type</th>";
            FacultyOppurtunities += "<th>Count</th>";
            FacultyOppurtunities += "</tr>";
            FacultyOppurtunities += "</thead><tbody>";

            var collgeSelfAppraisals = db.jntuh_college_selfappraisal.Where(i => i.isactive && i.collegeid == collegeId).ToList();
            var lstSelfAppraisals = db.jntuh_selfappraisal.AsNoTracking().Where(i => i.selfappraisaldescriptiontype == 10 && i.isactive == true).Select(i => new SelfAppraisalModel
            {
                Id = i.id,
                Selfappraisaldescription = i.selfappraisaldescription,
                Selfappraisaldescriptiontype = (int)i.selfappraisaldescriptiontype,
            }).ToList();
            foreach (var lst in lstSelfAppraisals)
            {
                lst.CollegeSelfAppraisalsCount = collgeSelfAppraisals.Count(i => i.selfappraisalid == lst.Id);
            }

            foreach (var item in lstSelfAppraisals)
            {
                FacultyOppurtunities += "<tr>";
                FacultyOppurtunities += "<td>" + sno + "</td>";
                FacultyOppurtunities += "<td>" + item.Selfappraisaldescription + "</td>";
                FacultyOppurtunities += "<td>" + item.CollegeSelfAppraisalsCount + "</td>";
                FacultyOppurtunities += "</tr>";
                sno++;
            }

            FacultyOppurtunities += "</tbody></table>";
            contents = contents.Replace("##FacultyOppurtunities##", FacultyOppurtunities);
            return contents;
        }

        private string SelfAppraisal(int collegeId, string contents)
        {
            int sno = 1;
            string SelfAppraisal = string.Empty;

            SelfAppraisal += "<table border='1' cellspacing='0' cellpadding='5'>";
            SelfAppraisal += "<thead>";
            SelfAppraisal += "<tr>";
            SelfAppraisal += "<th>S.No</th>";
            SelfAppraisal += "<th style=''text-align:center'>Self-Appraisal Type</th>";
            SelfAppraisal += "<th>Count</th>";
            SelfAppraisal += "</tr>";
            SelfAppraisal += "</thead><tbody>";

            var collgeSelfAppraisals = db.jntuh_college_selfappraisal.Where(i => i.isactive && i.collegeid == collegeId).ToList();
            var lstSelfAppraisals = db.jntuh_selfappraisal.AsNoTracking().Where(i => i.selfappraisaldescriptiontype == 1 && i.isactive == true).Select(i => new SelfAppraisalModel
            {
                Id = i.id,
                Selfappraisaldescription = i.selfappraisaldescription,
                Selfappraisaldescriptiontype = (int)i.selfappraisaldescriptiontype,
            }).ToList();
            foreach (var lst in lstSelfAppraisals)
            {
                lst.CollegeSelfAppraisalsCount = collgeSelfAppraisals.Count(i => i.selfappraisalid == lst.Id);
            }

            foreach (var item in lstSelfAppraisals)
            {
                SelfAppraisal += "<tr>";
                SelfAppraisal += "<td>" + sno + "</td>";
                SelfAppraisal += "<td>" + item.Selfappraisaldescription + "</td>";
                SelfAppraisal += "<td>" + item.CollegeSelfAppraisalsCount + "</td>";
                SelfAppraisal += "</tr>";
                sno++;
            }

            SelfAppraisal += "</tbody></table>";
            contents = contents.Replace("##SelfAppraisal##", SelfAppraisal);
            return contents;
        }

        private string ResearchGrants(int collegeId, string contents)
        {
            int sno = 1;
            string ResearchGrants = string.Empty;

            ResearchGrants += "<table border='1' cellspacing='0' cellpadding='5'>";
            ResearchGrants += "<thead>";
            ResearchGrants += "<tr>";
            ResearchGrants += "<th>S.No</th>";
            ResearchGrants += "<th style=''text-align:center'>Self-Appraisal Type</th>";
            ResearchGrants += "<th>Count</th>";
            ResearchGrants += "</tr>";
            ResearchGrants += "</thead><tbody>";

            var collgeSelfAppraisals = db.jntuh_college_selfappraisal.Where(i => i.isactive && i.collegeid == collegeId).ToList();
            var lstSelfAppraisals = db.jntuh_selfappraisal.AsNoTracking().Where(i => i.selfappraisaldescriptiontype == 2 && i.isactive == true).Select(i => new SelfAppraisalModel
            {
                Id = i.id,
                Selfappraisaldescription = i.selfappraisaldescription,
                Selfappraisaldescriptiontype = (int)i.selfappraisaldescriptiontype,
            }).ToList();
            foreach (var lst in lstSelfAppraisals)
            {
                lst.CollegeSelfAppraisalsCount = collgeSelfAppraisals.Count(i => i.selfappraisalid == lst.Id);
            }

            foreach (var item in lstSelfAppraisals)
            {
                ResearchGrants += "<tr>";
                ResearchGrants += "<td>" + sno + "</td>";
                ResearchGrants += "<td>" + item.Selfappraisaldescription + "</td>";
                ResearchGrants += "<td>" + item.CollegeSelfAppraisalsCount + "</td>";
                ResearchGrants += "</tr>";
                sno++;
            }

            ResearchGrants += "</tbody></table>";
            contents = contents.Replace("##ResearchGrants##", ResearchGrants);
            return contents;
        }

        private string MoUs(int collegeId, string contents)
        {
            int sno = 1;
            string MoUs = string.Empty;

            MoUs += "<table border='1' cellspacing='0' cellpadding='5'>";
            MoUs += "<thead>";
            MoUs += "<tr>";
            MoUs += "<th>S.No</th>";
            MoUs += "<th style=''text-align:center'>Self-Appraisal Type</th>";
            MoUs += "<th>Count</th>";
            MoUs += "</tr>";
            MoUs += "</thead><tbody>";

            var collgeSelfAppraisals = db.jntuh_college_selfappraisal.Where(i => i.isactive && i.collegeid == collegeId).ToList();
            var lstSelfAppraisals = db.jntuh_selfappraisal.AsNoTracking().Where(i => i.selfappraisaldescriptiontype == 3 && i.isactive == true).Select(i => new SelfAppraisalModel
            {
                Id = i.id,
                Selfappraisaldescription = i.selfappraisaldescription,
                Selfappraisaldescriptiontype = (int)i.selfappraisaldescriptiontype,
            }).ToList();
            foreach (var lst in lstSelfAppraisals)
            {
                lst.CollegeSelfAppraisalsCount = collgeSelfAppraisals.Count(i => i.selfappraisalid == lst.Id);
            }

            foreach (var item in lstSelfAppraisals)
            {
                MoUs += "<tr>";
                MoUs += "<td>" + sno + "</td>";
                MoUs += "<td>" + item.Selfappraisaldescription + "</td>";
                MoUs += "<td>" + item.CollegeSelfAppraisalsCount + "</td>";
                MoUs += "</tr>";
                sno++;
            }

            MoUs += "</tbody></table>";
            contents = contents.Replace("##MoUs##", MoUs);
            return contents;
        }

        private string GetDetails(int collegeId, int academicYearId, int specializationId, int shiftId, int yearInDegree, int flag)
        {
            using (var db = new uaaasDBContext())
            {
                string value = string.Empty;

                if (flag == 1)
                    value = db.jntuh_college_academic_performance.Where(a => a.collegeId == collegeId && a.academicYearId == academicYearId && a.specializationId == specializationId && a.shiftId == shiftId && a.yearInDegreeId == yearInDegree).Select(a => a.appearedStudents).FirstOrDefault().ToString();
                else if (flag == 2)
                    value = db.jntuh_college_academic_performance.Where(a => a.collegeId == collegeId && a.academicYearId == academicYearId && a.specializationId == specializationId && a.shiftId == shiftId && a.yearInDegreeId == yearInDegree).Select(a => a.passedStudents).FirstOrDefault().ToString();
                else if (flag == 3)
                    value = db.jntuh_college_academic_performance.Where(a => a.collegeId == collegeId && a.academicYearId == academicYearId && a.specializationId == specializationId && a.shiftId == shiftId && a.yearInDegreeId == yearInDegree).Select(a => a.passPercentage).FirstOrDefault().ToString();

                return value;
            }
        }

        //private int GetIntake(int collegeId, int academicYearId, int specializationId, int shiftId, int flag)
        //{
        //    using (var db = new uaaasDBContext())
        //    {
        //        int intake = 0;

        //        if (flag == 1)
        //            intake = db.jntuh_college_intake_existing.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == shiftId).Select(i => i.approvedIntake).FirstOrDefault();
        //        else if (flag == 0)
        //            intake = db.jntuh_college_intake_existing.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == shiftId).Select(i => i.admittedIntake).FirstOrDefault();
        //        else
        //            intake = db.jntuh_college_intake_existing.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == shiftId).Select(i => i.aicteApprovedIntake).FirstOrDefault();
        //        return intake;
        //    }
        //}

        private int GetIntake(int collegeId, int academicYearId, int specializationId, int shiftId, int flag)
        {
            int intake = 0;
            //int presentYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            //int AYID = db.jntuh_academic_year.Where(a => a.actualYear == (presentYear + 1)).Select(a => a.id).FirstOrDefault();

            //approved
            if (flag == 1)
            {
                intake =
                    db.jntuh_college_intake_existing.Where(
                        i =>
                            i.collegeId == collegeId && i.academicYearId == academicYearId &&
                            i.specializationId == specializationId && i.shiftId == shiftId)
                        .Select(i => i.approvedIntake)
                        .FirstOrDefault();

                //RAMESH: NOT REQUIRED AS PROPOSED INTAKE IS COMING FROM EXISTING TABLE ONLY

                ////to get proposedIntake vale for AY-2014-15
                //if (intake == 0 && academicYearId == AYID)
                //{
                //    intake = db.jntuh_college_intake_proposed.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == shiftId).Select(i => i.proposedIntake).FirstOrDefault();
                //}
            }
            else if (flag == 2)
            {
                intake =
                    db.jntuh_college_intake_existing.Where(
                        i =>
                            i.collegeId == collegeId && i.academicYearId == academicYearId &&
                            i.specializationId == specializationId && i.shiftId == shiftId)
                        .Select(i => i.aicteApprovedIntake)
                        .FirstOrDefault();
            }
            else if (flag == 3) //Narayana Reddy- Regular admitted Intake as per Exam Branch data
            {
                intake =
                    db.jntuh_college_intake_existing.Where(
                        i =>
                            i.collegeId == collegeId && i.academicYearId == academicYearId &&
                            i.specializationId == specializationId && i.shiftId == shiftId)
                        .Select(i => i.admittedIntakeasperExambranch_R)
                        .FirstOrDefault();
            }
            else if (flag == 4) //Narayana Reddy-lateral admitted Intake as per Exam Branch data
            {
                intake =
                    db.jntuh_college_intake_existing.Where(
                        i =>
                            i.collegeId == collegeId && i.academicYearId == academicYearId &&
                            i.specializationId == specializationId && i.shiftId == shiftId)
                        .Select(i => i.admittedIntakeasperExambranch_L)
                        .FirstOrDefault();
            }
            else //admitted
            {
                intake =
                    db.jntuh_college_intake_existing.Where(
                        i =>
                            i.collegeId == collegeId && i.academicYearId == academicYearId &&
                            i.specializationId == specializationId && i.shiftId == shiftId)
                        .Select(i => i.admittedIntake)
                        .FirstOrDefault();
            }

            return intake;
        }

        private int GetExaminationBranchIntake(int collegeId, int academicYearId, int specializationId, int shiftId, int flag)
        {
            using (var db = new uaaasDBContext())
            {
                int intake = 0;
                intake = db.jntuh_approvedadmitted_intake.Where(i => i.collegeId == collegeId && i.AcademicYearId == academicYearId && i.SpecializationId == specializationId && i.ShiftId == shiftId).Select(i => i.AdmittedIntake).FirstOrDefault();

                return intake;
            }
        }

        public string PaymentDetails(int collegeId, string contents)
        {
            using (var db = new uaaasDBContext())
            {
                string strPaymentDetails = string.Empty;
                int sno = 1;
                string strPaymentStatus = string.Empty;
                string strPaymentDate = string.Empty;
                strPaymentDetails += "<table border='1' cellspacing='0' cellpadding='5' style='font-size: 9px;'>";
                strPaymentDetails += "<tbody>";
                strPaymentDetails += "<tr>";
                strPaymentDetails += "<td colspan='2'><p align='left'>S.No</p></td>";
                strPaymentDetails += "<td colspan='4'><p align='left'>Payment Date</p></td>";
                strPaymentDetails += "<td colspan='3'><p align='left'>Payment Type</p></td>";
                strPaymentDetails += "<td colspan='3'><p align='left'>DD Number</p></td>";
                strPaymentDetails += "<td colspan='4'><p align='left'>Branch</p></td>";
                strPaymentDetails += "<td colspan='3'><p align='left'>Payment Status</p></td>";
                strPaymentDetails += "<td colspan='2'><p align='center'>Total Amount</p></td>";
                strPaymentDetails += "</tr>";
                IEnumerable<CollegePayment> collegePayment = db.jntuh_college_payment.Where(a => a.collegeId == collegeId)
                                                               .Select(a => new CollegePayment
                                                               {
                                                                   id = a.id,
                                                                   collegeId = collegeId,
                                                                   paymentDate = a.paymentDate,
                                                                   paymentType = a.paymentType,
                                                                   paymentNumber = a.paymentNumber,
                                                                   paymentStatus = a.paymentStatus,
                                                                   paymentAmount = a.paymentAmount,
                                                                   paymentBranch = a.paymentBranch,
                                                                   paymentLocation = a.paymentLocation
                                                               }).OrderBy(a => a.paymentDate).ToList();

                if (collegePayment != null)
                {

                    foreach (var item in collegePayment)
                    {
                        if (item.paymentDate != null)
                        {
                            strPaymentDate = item.paymentDate.ToString() == null ? string.Empty : UAAAS.Models.Utilities.MMDDYY2DDMMYY(item.paymentDate.ToString()).ToString();
                        }
                        if (item.paymentStatus == 0)
                        {
                            strPaymentStatus = "Pending";
                        }
                        else
                        {
                            strPaymentStatus = "Paid";
                        }
                        strPaymentDetails += "<tr>";
                        strPaymentDetails += "<td colspan='2'><p align='left'>" + sno + "</p></td>";
                        strPaymentDetails += "<td colspan='4'><p align='left'>" + strPaymentDate + "</p></td>";
                        strPaymentDetails += "<td  colspan='3' align='left'>DD</td>";
                        strPaymentDetails += "<td  colspan='3' align='left'>" + item.paymentNumber + "</td>";
                        strPaymentDetails += "<td  colspan='4' align='left'>" + item.paymentBranch + ' ' + item.paymentLocation + "</td>";
                        strPaymentDetails += "<td  colspan='3' align='left'>" + strPaymentStatus + "</td>";
                        strPaymentDetails += "<td  colspan='2' align='right'>" + item.paymentAmount + "</td>";
                        strPaymentDetails += "</tr>";
                        sno++;
                    }
                }
                strPaymentDetails += "</tbody></table>";
                contents = contents.Replace("##PaymentDetails##", strPaymentDetails);
                return contents;
            }
        }

        public string PaymentOfFee(int collegeId, string contents)
        {
            using (var db = new uaaasDBContext())
            {
                string strPaymentOfFee = string.Empty;
                int sno = 1;
                strPaymentOfFee += "<table border='1' cellspacing='0' cellpadding='5' style='font-size: 9px;'>";
                strPaymentOfFee += "<tbody>";
                strPaymentOfFee += "<tr>";
                strPaymentOfFee += "<td colspan='2'><p align='left'>S.No</p></td>";
                strPaymentOfFee += "<td colspan='6'><p align='left'>Fee Type</p></td>";
                strPaymentOfFee += "<td colspan='4'><p align='left'>Paid(Till Date)</p></td>";
                strPaymentOfFee += "<td colspan='4'><p align='left'>Dues(if any)</p></td>";
                strPaymentOfFee += "</tr>";

                var paymentFee = db.jntuh_college_paymentoffee.Where(p => p.collegeId == collegeId)
                                                            .Select(p => new
                                                            {
                                                                feeType = p.jntuh_college_paymentoffee_type.FeeType,
                                                                paidAmount = p.paidAmount,
                                                                duesAmoount = p.duesAmount
                                                            }).ToList();


                if (paymentFee != null)
                {
                    foreach (var item in paymentFee)
                    {
                        strPaymentOfFee += "<tr>";
                        strPaymentOfFee += "<td colspan='2'><p align='left'>" + sno + "</p></td>";
                        strPaymentOfFee += "<td colspan='6'><p align='left'>" + item.feeType + "</p></td>";
                        strPaymentOfFee += "<td colspan='4' align='left'>" + item.paidAmount + "</td>";
                        strPaymentOfFee += "<td colspan='4' align='left'>" + item.duesAmoount + "</td>";
                        strPaymentOfFee += "</tr>";
                        sno++;
                    }
                }
                strPaymentOfFee += "</tbody></table>";
                contents = contents.Replace("##PaymentOfFee##", strPaymentOfFee);
                return contents;
            }
        }
    }
}
