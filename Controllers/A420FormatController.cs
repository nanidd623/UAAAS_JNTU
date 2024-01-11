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

namespace UAAAS.Controllers
{
    [ErrorHandling]
    public class A420FormatController : Controller
    {
        //
        // GET: /A418Format/
        private uaaasDBContext db = new uaaasDBContext();
        private string serverURL;
        private string test415PDF;
        private string barcodetext;
        private string LatefeeQrCodetext;
        public A420FormatController()
        {
            //serverURL = System.Web.HttpContext.Current.Server.MapPath("~/");
            //serverURL = "" + serverURL + "/";

            //this.standardPdfRenderer = new StandardPdfRenderer();
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
                return File(path, "application/pdf", "A-423-" + collegeCode + ".pdf");
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

                fullPath = path + "/CollegeData/A-423_" + collegeCode + "_" + DateTime.Now.ToString("yyyMMddHHmmss") +
                           ".pdf";
                PdfWriter pdfWriter = PdfWriter.GetInstance(pdfDoc, new FileStream(fullPath, FileMode.Create));
                ITextEvents iTextEvents = new ITextEvents();
                iTextEvents.CollegeName = collegeName;
                iTextEvents.CollegeCode = collegeCode;
                // iTextEvents.formType = "A-414";
                iTextEvents.formType = "A-423";
                pdfWriter.PageEvent = iTextEvents;
            }
            else
            {

                fullPath = path + "/CollegeData/A-423_" + collegeCode + "_" + DateTime.Now.ToString("yyyMMddHHmmss") +
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

            sr = System.IO.File.OpenText(Server.MapPath("~/Content/A-420.html"));

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

            if (collegeId != TestCollegeId || (collegeId == TestCollegeId && !TestMethodNumber.Contains("1")))
                contents = affiliationType(collegeId, contents);
            //EventLog.WriteEntry("affiliationType", "affiliationType");
            if (collegeId != TestCollegeId || (collegeId == TestCollegeId && !TestMethodNumber.Contains("2")))
                contents = collegeInformation(collegeId, contents);
            //EventLog.WriteEntry("collegeInformation", "collegeInformation");
            if (collegeId != TestCollegeId || (collegeId == TestCollegeId && !TestMethodNumber.Contains("3")))
                contents = EducationalSocietyDetails(collegeId, contents);
            //EventLog.WriteEntry("EducationalSocietyDetails", "EducationalSocietyDetails");
            if (collegeId != TestCollegeId || (collegeId == TestCollegeId && !TestMethodNumber.Contains("4")))
                contents = PrincipalDirectorDetails(collegeId, contents);

            //EventLog.WriteEntry("PrincipalDirectorDetails", "PrincipalDirectorDetails");
            if (collegeId != TestCollegeId || (collegeId == TestCollegeId && !TestMethodNumber.Contains("5")))
                contents = ChairpersonDetails(collegeId, contents);
            //EventLog.WriteEntry("ChairpersonDetails", "ChairpersonDetails");
            if (collegeId != TestCollegeId || (collegeId == TestCollegeId && !TestMethodNumber.Contains("6")))
                contents = OthercollegesandOtherCoursesDetails(collegeId, contents);
            //EventLog.WriteEntry("OthercollegesandOtherCoursesDetails", "OthercollegesandOtherCoursesDetails");
            if (collegeId != TestCollegeId || (collegeId == TestCollegeId && !TestMethodNumber.Contains("7")))
                contents = LandInformationDetails(collegeId, contents);
            //EventLog.WriteEntry("LandInformationDetails", "LandInformationDetails");
            if (collegeId != TestCollegeId || (collegeId == TestCollegeId && !TestMethodNumber.Contains("8")))
                contents = AdministrativeLandDetails(collegeId, contents);
            //EventLog.WriteEntry("AdministrativeLandDetails", "AdministrativeLandDetails");
            if (collegeId != TestCollegeId || (collegeId == TestCollegeId && !TestMethodNumber.Contains("9")))
                contents = InstructionalAreaDetails(collegeId, contents);
            //EventLog.WriteEntry("InstructionalAreaDetails", "InstructionalAreaDetails");
            if (collegeId != TestCollegeId || (collegeId == TestCollegeId && !TestMethodNumber.Contains("10")))
                contents = ExistingIntakeDetails(collegeId, contents);
            //EventLog.WriteEntry("ExistingIntakeDetails", "ExistingIntakeDetails");
            if (collegeId != TestCollegeId || (collegeId == TestCollegeId && !TestMethodNumber.Contains("11")))
                contents = AcademicPerformanceDetails(collegeId, contents);
            //EventLog.WriteEntry("AcademicPerformanceDetails", "AcademicPerformanceDetails");
            if (collegeId != TestCollegeId || (collegeId == TestCollegeId && !TestMethodNumber.Contains("12")))
                contents = collegeTachingFacultyMembers(collegeId, contents);
            //EventLog.WriteEntry("collegeTachingFacultyMembers", "collegeTachingFacultyMembers");
            if (collegeId != TestCollegeId || (collegeId == TestCollegeId && !TestMethodNumber.Contains("13")))
                contents = collegeNonTachingFacultyMembers(collegeId, contents);
            //EventLog.WriteEntry("collegeNonTachingFacultyMembers", "collegeNonTachingFacultyMembers");
            if (collegeId != TestCollegeId || (collegeId == TestCollegeId && !TestMethodNumber.Contains("14")))
                contents = collegeTechnicalFacultyMembers(collegeId, contents);
            // EventLog.WriteEntry("collegeTechnicalFacultyMembers", "collegeTechnicalFacultyMembers");
            if (collegeId != TestCollegeId || (collegeId == TestCollegeId && !TestMethodNumber.Contains("15")))
                contents = LaboratoriesDetails(collegeId, contents);

            //EventLog.WriteEntry("LaboratoriesDetails", "LaboratoriesDetails");
            if (collegeId != TestCollegeId || (collegeId == TestCollegeId && !TestMethodNumber.Contains("16")))
                contents = LibraryDetails(collegeId, contents, DegreeIds);
            //EventLog.WriteEntry("LibraryDetails", "LibraryDetails");
            if (collegeId != TestCollegeId || (collegeId == TestCollegeId && !TestMethodNumber.Contains("17")))
                contents = InternetBandwidthDetails(collegeId, contents, DegreeIds);
            //EventLog.WriteEntry("InternetBandwidthDetails", "InternetBandwidthDetails");
            if (collegeId != TestCollegeId || (collegeId == TestCollegeId && !TestMethodNumber.Contains("18")))
                contents = LegalSystemSoftwareDetails(collegeId, contents, DegreeIds);
            //EventLog.WriteEntry("LegalSystemSoftwareDetails", "LegalSystemSoftwareDetails");
            if (collegeId != TestCollegeId || (collegeId == TestCollegeId && !TestMethodNumber.Contains("19")))
                contents = PrintersDetails(collegeId, contents, DegreeIds);
            //EventLog.WriteEntry("PrintersDetails", "PrintersDetails");
            if (collegeId != TestCollegeId || (collegeId == TestCollegeId && !TestMethodNumber.Contains("20")))
                contents = ExaminationBranchDetails(collegeId, contents);
            //EventLog.WriteEntry("ExaminationBranchDetails", "ExaminationBranchDetails");
            if (collegeId != TestCollegeId || (collegeId == TestCollegeId && !TestMethodNumber.Contains("21")))
                contents = DesirableRequirementsDetails(collegeId, contents);
            //EventLog.WriteEntry("DesirableRequirementsDetails", "DesirableRequirementsDetails");
            if (collegeId != TestCollegeId || (collegeId == TestCollegeId && !TestMethodNumber.Contains("22")))
                contents = AntiRaggingCommitteeDetails(collegeId, contents);
            //EventLog.WriteEntry("AntiRaggingCommitteeDetails", "AntiRaggingCommitteeDetails");
            if (collegeId != TestCollegeId || (collegeId == TestCollegeId && !TestMethodNumber.Contains("23")))
                contents = WomenProtectionCellDetails(collegeId, contents);
            //EventLog.WriteEntry("WomenProtectionCellDetails", "WomenProtectionCellDetails");
            if (collegeId != TestCollegeId || (collegeId == TestCollegeId && !TestMethodNumber.Contains("24")))
                contents = RTIDetails(collegeId, contents);
            //EventLog.WriteEntry("RTIDetails", "RTIDetails");
            if (collegeId != TestCollegeId || (collegeId == TestCollegeId && !TestMethodNumber.Contains("25")))
                contents = OtherDesirablesDetails(collegeId, contents);
            //EventLog.WriteEntry("OtherDesirablesDetails", "OtherDesirablesDetails");
            if (collegeId != TestCollegeId || (collegeId == TestCollegeId && !TestMethodNumber.Contains("26")))
                contents = CampusHostelMaintenanceDetails(collegeId, contents);
            //EventLog.WriteEntry("CampusHostelMaintenanceDetails", "CampusHostelMaintenanceDetails");
            if (collegeId != TestCollegeId || (collegeId == TestCollegeId && !TestMethodNumber.Contains("27")))
                contents = OperationalFundsDetails(collegeId, contents);
            //EventLog.WriteEntry("OperationalFundsDetails", "OperationalFundsDetails");
            if (collegeId != TestCollegeId || (collegeId == TestCollegeId && !TestMethodNumber.Contains("28")))
                contents = IncomeDetails(collegeId, contents);
            //EventLog.WriteEntry("IncomeDetails", "IncomeDetails");
            if (collegeId != TestCollegeId || (collegeId == TestCollegeId && !TestMethodNumber.Contains("29")))
                contents = ExpenditureDetails(collegeId, contents);
            //EventLog.WriteEntry("ExpenditureDetails", "ExpenditureDetails");
            if (collegeId != TestCollegeId || (collegeId == TestCollegeId && !TestMethodNumber.Contains("30")))
                contents = StudentsPlacementDetails(collegeId, contents);//, cSpcIds, DepartmentsData, DegreeIds
            //EventLog.WriteEntry("StudentsPlacementDetails", "StudentsPlacementDetails");
            if (collegeId != TestCollegeId || (collegeId == TestCollegeId && !TestMethodNumber.Contains("31")))
                contents = CollegePhotosDetails(collegeId, contents);
            //EventLog.WriteEntry("CollegePhotosDetails", "CollegePhotosDetails");
            if (collegeId != TestCollegeId || (collegeId == TestCollegeId && !TestMethodNumber.Contains("32")))
                contents = PaymentDetails(collegeId, contents);
            //EventLog.WriteEntry("PaymentDetails", "PaymentDetails");
            if (collegeId != TestCollegeId || (collegeId == TestCollegeId && !TestMethodNumber.Contains("33")))
                contents = PaymentOfFee(collegeId, contents);
            //EventLog.WriteEntry("PaymentOfFee", "PaymentOfFee");
            if (collegeId != TestCollegeId || (collegeId == TestCollegeId && !TestMethodNumber.Contains("34")))
                contents = collegeEnclosures(collegeId, contents);
            //EventLog.WriteEntry("collegeEnclosures", "collegeEnclosures");
            if (collegeId != TestCollegeId || (collegeId == TestCollegeId && !TestMethodNumber.Contains("35")))
                contents = DataModifications(collegeId, contents);
            if (collegeId != TestCollegeId || (collegeId == TestCollegeId && !TestMethodNumber.Contains("36")))
                contents = AcademicAudit(collegeId, contents);
            if (collegeId != TestCollegeId || (collegeId == TestCollegeId && !TestMethodNumber.Contains("37")))
                contents = ValueAddedPrograms(collegeId, contents);
            if (collegeId != TestCollegeId || (collegeId == TestCollegeId && !TestMethodNumber.Contains("38")))
                contents = EssentialRequirements(collegeId, contents);
            if (collegeId != TestCollegeId || (collegeId == TestCollegeId && !TestMethodNumber.Contains("39")))
                contents = JHubRequirements(collegeId, contents);
            if (collegeId != TestCollegeId || (collegeId == TestCollegeId && !TestMethodNumber.Contains("40")))
                contents = CollegeAffiliationFee(collegeId, contents);
            System.GC.Collect();

            // EventLog.WriteEntry("DataModifications", "DataModifications");
            //if (collegeId != TestCollegeId || (collegeId == TestCollegeId && !TestMethodNumber.Contains("15")))
            //    contents = ExperimentDetails(collegeId, contents);

            //Commented on 19-12-2019
            contents = PaymentBillDetails(collegeId, contents);
            contents = LateFeePaymentBillDetails(collegeId, contents);
            contents = NOofPhysicalLabs(collegeId, contents);


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
            string name = Server.MapPath("~/Content/PDFReports/temp/") + collegecode + "-" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".txt";

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

        public List<ModeOfPaymentModel> paymentsModes = new List<ModeOfPaymentModel>();
        public PlayGroundTypeModel[] playGroundTypes = new[]
            {
                new PlayGroundTypeModel { id = "1", Name = "Square" },
                new PlayGroundTypeModel { id = "2", Name = "Rectangle" },
                new PlayGroundTypeModel { id = "3", Name = "Round" },
                new PlayGroundTypeModel { id = "4", Name = "Oval" },
                new PlayGroundTypeModel { id = "5", Name = "Cricket" },
                new PlayGroundTypeModel { id = "6", Name = "Other" }
            };

        public List<PlayGroundTypeModel> playGroundType = new List<PlayGroundTypeModel>();

        public ModeOfTransportModel[] transportMode = new[]
            {
                new ModeOfTransportModel { id = "1", Name = "College Transport" },
                new ModeOfTransportModel { id = "2", Name = "Public Transport" },
                new ModeOfTransportModel { id = "3", Name = "Other" }
            };
        public List<ModeOfTransportModel> transportModes = new List<ModeOfTransportModel>();

        public ModeOfPaymentModel[] paymentMode = new[]
            {
                new ModeOfPaymentModel { id = "1", Name = "Cash" },
                new ModeOfPaymentModel { id = "2", Name = "Cheque" },
                new ModeOfPaymentModel { id = "3", Name = "Bank Transfer" },
                new ModeOfPaymentModel { id = "4", Name = "Other" }
            };

        private string affiliationType(int collegeId, string contents)
        {
            using (var db = new UAAAS.Models.uaaasDBContext())
            {
                string strCollegeAffiliationType = string.Empty;
                List<jntuh_college_affiliation_type> affiliationType = db.jntuh_college_affiliation_type
                                                                         .Where(affiliation => affiliation.isActive == true)
                                                                         .OrderBy(affiliation => affiliation.DisplayOrder)
                                                                         .ToList();
                foreach (var item in affiliationType)
                {
                    string YesOrNo = "no";
                    int selectedId = db.jntuh_college.Where(college => college.id == collegeId)
                                                     .Select(college => college.collegeAffiliationTypeID)
                                                     .FirstOrDefault();
                    if (item.id == selectedId)
                    {
                        YesOrNo = "yes_b";

                        //strCollegeAffiliationType += string.Format("<img alt='' src='" + serverURL + "/Content/Images/checkbox_{0}.png' height='10' />&nbsp; {1}<br/>", YesOrNo, item.collegeAffiliationType);
                        strCollegeAffiliationType += string.Format("{0}<br/>", item.collegeAffiliationType);
                    }
                }
                contents = contents.Replace("##COLLEGE_AFFILIATIONTYPE##", strCollegeAffiliationType);
                //List<IElement> parsedHtmlElements = HTMLWorker.ParseToList(new StringReader(contents), null);
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
                string duration = string.Empty;
                string affStatus = string.Empty;
                string yes = "no_b";
                string no = "no_b";
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
                    var affiliationCollegeType = collegeAfflitaions.Where(g => g.affiliationTypeId == item.id);
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
                            collegeAffiliationType += "<tr>";
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
                        case "NBA Status":
                            collegeAffiliationType += "<tr>";
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

            int actualYear1 = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int prAy = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear1 + 1)).Select(s => s.id).FirstOrDefault();
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
                        strPrincipal += "<td colspan='5' valign='top'>" + PrincipalDetails.RegistrationNumber + "</td>";
                        // strPrincipal += "<td valign='top' colspan='2' align='center'>BAS</td>";
                        //  strPrincipal += "<td valign='top' colspan='8' align='center'><b>Yes</b> " + strcheckList + " &nbsp;&nbsp;<b>&nbsp;&nbsp;No</b> " + strcheckList + " &nbsp;&nbsp;<b>&nbsp;&nbsp;Error</b> " + strcheckList + "</td>";
                        strPrincipal += "<td  valign='top' colspan='4'>PAN Number</td><td align='center'>:</td><td valign='top' colspan='5'>" + PrincipalDetails.PANNumber + "</td>";
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
                        strPrincipal += "<td valign='top' colspan='4'>Qualification</td>";
                        strPrincipal += "<td valign='top' colspan='1' align='center'>:</td>";
                        if (education != null)
                        {
                            if (education.courseStudied != null)
                            {
                                strPrincipal += "<td valign='top' colspan='5'>" + education.courseStudied + "</td>";
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
                        strPrincipal += "<td valign='top' colspan='5'>&nbsp;</td>";
                        strPrincipal += "</tr>";

                        strPrincipal += "<tr>";
                        strPrincipal += "<td valign='top' colspan='4'>Date of Birth</td>";
                        strPrincipal += "<td valign='top' colspan='1' align='center'>:</td>";
                        strPrincipal += "<td valign='top' colspan='5'>" + PrincipalDetails.DateOfBirth + "</td>";

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
                        strPrincipal += "<td valign='top' colspan='4'>Qualification</td>";
                        strPrincipal += "<td valign='top' colspan='1' align='center'>:</td>";
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

                        strPrincipal += "<td valign='top' colspan='4'>Mobile (+91)</td>";
                        strPrincipal += "<td valign='top' colspan='1' align='center'>:</td>";
                        strPrincipal += "<td valign='top' colspan='5'></td>";
                        strPrincipal += "</tr>";

                        strPrincipal += "<tr>";
                        strPrincipal += "<td valign='top' colspan='4'>Email</td>";
                        strPrincipal += "<td valign='top' colspan='1' align='center'>:</td>";
                        strPrincipal += "<td colspan='15' valign='top'></td>";
                        strPrincipal += "</tr>";


                        strPrincipal += "<tr>";
                        strPrincipal += "<td valign='top' colspan='4'>Ratified</td>";
                        strPrincipal += "<td valign='top' colspan='1' align='center'>:</td>";
                        strPrincipal += "<td colspan='5' valign='top'></td>";

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
                    strPrincipal += "<td valign='top' colspan='4'>Qualification</td>";
                    strPrincipal += "<td valign='top' colspan='1' align='center'>:</td>";
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

                    strPrincipal += "<td valign='top' colspan='4'>Mobile (+91)</td>";
                    strPrincipal += "<td valign='top' colspan='1' align='center'>:</td>";
                    strPrincipal += "<td valign='top' colspan='5'></td>";
                    strPrincipal += "</tr>";

                    strPrincipal += "<tr>";
                    strPrincipal += "<td valign='top' colspan='4'>Email</td>";
                    strPrincipal += "<td valign='top' colspan='1' align='center'>:</td>";
                    strPrincipal += "<td colspan='15' valign='top'></td>";
                    strPrincipal += "</tr>";


                    strPrincipal += "<tr>";
                    strPrincipal += "<td valign='top' colspan='4'>Ratified</td>";
                    strPrincipal += "<td valign='top' colspan='1' align='center'>:</td>";
                    strPrincipal += "<td colspan='5' valign='top'></td>";

                    strPrincipal += "<td valign='top' colspan='4'>Photo</td>";
                    strPrincipal += "<td valign='top' colspan='1' align='center'>:</td>";
                    strPrincipal += "<td colspan='5' valign='top'></td>";
                    strPrincipal += "</tr>";

                    strPrincipal += "</tbody>";
                    strPrincipal += "</table>";
                    //strPrincipal += "<br />";
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
                }
                //  List<IElement> parsedHtmlElements = HTMLWorker.ParseToList(new StringReader(contents), null);
                return contents;
            }
        }

        public string OthercollegesandOtherCoursesDetails(int collegeId, string contents)
        {
            using (var db = new UAAAS.Models.uaaasDBContext())
            {
                string strOthercollegesDetails = string.Empty;
                string strOthercoursesDetails = string.Empty;
                string strSocietyOthercollegesDetails = string.Empty;

                int othercollegesno = 0;
                int otherCoursesno = 0;
                int societyOthercollegesno = 0;

                #region OtherCollegesion
                List<OtherCollege> otherCollege = db.jntuh_society_other_colleges.Where(a => a.collegeId == collegeId).Select(a =>
                                             new OtherCollege
                                             {
                                                 id = a.id,
                                                 collegeId = a.collegeId,
                                                 collegeName = a.collegeName,
                                                 affiliatedUniversityId = a.affiliatedUniversityId,
                                                 otherUniversityName = a.otherUniversityName,
                                                 universityName = a.jntuh_university.universityName,
                                                 yearOfEstablishment = a.yearOfEstablishment
                                             }).ToList();

                strOthercollegesDetails += "<table border='1' cellspacing='0' cellpadding='5'><tbody>";
                strOthercollegesDetails += "<tr><td colspan='2'>S.No</td><td colspan='10'>Name of the College/Institution</td><td colspan='4' valign='top'>Established Year</td><td colspan='5'>Affiliated University *</td></tr>";
                if (otherCollege.Count != 0)
                {
                    foreach (var item in otherCollege)
                    {
                        othercollegesno++;
                        strOthercollegesDetails += "<tr><td colspan='2' valign='top'>" + othercollegesno + "</td><td colspan='10' valign='top'>" + item.collegeName + "</td><td colspan='4' valign='top'>" + item.yearOfEstablishment + "</td><td colspan='5' valign='top'>" + item.universityName + "</td></tr>";
                    }
                }
                else
                {
                    strOthercollegesDetails += "<tr><td colspan='2' valign='top'>&nbsp;</td><td colspan='10' valign='top'>&nbsp;<br /><br /></td><td colspan='4' valign='top'>&nbsp;</td><td colspan='5' valign='top'>&nbsp;</td></tr>";
                }

                strOthercollegesDetails += "</tbody></table>";
                #endregion

                #region OtherCourses
                List<OtherCourse> otherCourse = db.jntuh_college_other_university_courses.Where(a => a.collegeId == collegeId).Select(a =>
                                                     new OtherCourse
                                                     {
                                                         id = a.id,
                                                         collegeId = a.collegeId,
                                                         courseName = a.courseName,
                                                         affiliatedUniversityId = a.affiliatedUniversityId,
                                                         otherUniversityName = a.otherUniversityName,
                                                         jntuh_college = a.jntuh_college,
                                                         jntuh_university = a.jntuh_university,
                                                         universityName = a.jntuh_university.universityName
                                                     }).ToList();

                strOthercoursesDetails += "<table border='1' cellspacing='0' cellpadding='5'><tbody>";
                strOthercoursesDetails += "<tr><td colspan='2'>S.No</td><td colspan='10'>Name of the Course</td><td colspan='4'>Affiliated University *</td></tr>";
                if (otherCourse.Count != 0)
                {
                    foreach (var item in otherCourse)
                    {
                        otherCoursesno++;
                        strOthercoursesDetails += "<tr><td colspan='2' valign='top'>" + otherCoursesno + "</td><td colspan='10' valign='top'>" + item.courseName + "</td><td colspan='4' valign='top'>" + item.universityName + "</td></tr>";
                    }
                }
                else
                {
                    strOthercoursesDetails += "<tr><td colspan='2' valign='top'>&nbsp;<br /></td><td colspan='10' valign='top'>&nbsp;</td><td colspan='4' valign='top'>&nbsp;</td></tr>";
                    strOthercoursesDetails += "<tr><td colspan='2' valign='top'>&nbsp;<br /></td><td colspan='10' valign='top'>&nbsp;</td><td colspan='4' valign='top'>&nbsp;</td></tr>";
                    strOthercoursesDetails += "<tr><td colspan='2' valign='top'>&nbsp;<br /></td><td colspan='10' valign='top'>&nbsp;</td><td colspan='4' valign='top'>&nbsp;</td></tr>";
                    strOthercoursesDetails += "<tr><td colspan='2' valign='top'>&nbsp;<br /></td><td colspan='10' valign='top'>&nbsp;</td><td colspan='4' valign='top'>&nbsp;</td></tr>";
                    strOthercoursesDetails += "<tr><td colspan='2' valign='top'>&nbsp;<br /></td><td colspan='10' valign='top'>&nbsp;</td><td colspan='4' valign='top'>&nbsp;</td></tr>";
                }

                strOthercoursesDetails += "</tbody></table>";
                #endregion

                #region OtherCollegesion
                List<OtherCollege> societyOtherCollege = db.jntuh_society_other_locations_colleges.Where(a => a.collegeId == collegeId).Select(a =>
                                             new OtherCollege
                                             {
                                                 id = a.id,
                                                 collegeId = a.collegeId,
                                                 collegeName = a.collegeName,
                                                 affiliatedUniversityId = a.affiliatedUniversityId,
                                                 otherUniversityName = a.collegeLocation,
                                                 universityName = a.jntuh_university.universityName,
                                                 yearOfEstablishment = a.yearOfEstablishment
                                             }).ToList();

                strSocietyOthercollegesDetails += "<table border='1' cellspacing='0' cellpadding='5'><tbody>";
                strSocietyOthercollegesDetails += "<tr><td colspan='2'>S.No</td><td colspan='8'>Name of the College / Institution with Address</td><td colspan='4' valign='top'>Established Year</td><td colspan='4' valign='top'>Location</td><td colspan='5'>Affiliated University *</td></tr>";
                if (societyOtherCollege.Count != 0)
                {
                    foreach (var item in societyOtherCollege)
                    {
                        societyOthercollegesno++;
                        strSocietyOthercollegesDetails += "<tr><td colspan='2' valign='top'>" + societyOthercollegesno + "</td><td colspan='8' valign='top'>" + item.collegeName + "</td><td colspan='4' valign='top'>" + item.yearOfEstablishment + "</td><td colspan='4' valign='top'>" + item.otherUniversityName + "</td><td colspan='5' valign='top'>" + item.universityName + "</td></tr>";
                    }
                }
                else
                {
                    strSocietyOthercollegesDetails += "<tr><td colspan='2' valign='top'>&nbsp;<br /></td><td colspan='8' valign='top'>&nbsp;</td><td colspan='4' valign='top'>&nbsp;</td><td colspan='4' valign='top'>&nbsp;</td><td colspan='5' valign='top'>&nbsp;</td></tr>";
                    strSocietyOthercollegesDetails += "<tr><td colspan='2' valign='top'>&nbsp;<br /></td><td colspan='8' valign='top'>&nbsp;</td><td colspan='4' valign='top'>&nbsp;</td><td colspan='4' valign='top'>&nbsp;</td><td colspan='5' valign='top'>&nbsp;</td></tr>";
                    strSocietyOthercollegesDetails += "<tr><td colspan='2' valign='top'>&nbsp;<br /></td><td colspan='8' valign='top'>&nbsp;</td><td colspan='4' valign='top'>&nbsp;</td><td colspan='4' valign='top'>&nbsp;</td><td colspan='5' valign='top'>&nbsp;</td></tr>";
                    strSocietyOthercollegesDetails += "<tr><td colspan='2' valign='top'>&nbsp;<br /></td><td colspan='8' valign='top'>&nbsp;</td><td colspan='4' valign='top'>&nbsp;</td><td colspan='4' valign='top'>&nbsp;</td><td colspan='5' valign='top'>&nbsp;</td></tr>";
                    strSocietyOthercollegesDetails += "<tr><td colspan='2' valign='top'>&nbsp;<br /></td><td colspan='8' valign='top'>&nbsp;</td><td colspan='4' valign='top'>&nbsp;</td><td colspan='4' valign='top'>&nbsp;</td><td colspan='5' valign='top'>&nbsp;</td></tr>";
                }

                strSocietyOthercollegesDetails += "</tbody></table>";
                #endregion

                contents = contents.Replace("##OthercollegesDetails##", strOthercollegesDetails);
                contents = contents.Replace("##OthercoursesDetails##", strOthercoursesDetails);
                contents = contents.Replace("##SocietyOthercollegesDetails##", strSocietyOthercollegesDetails);
                //List<IElement> parsedHtmlElements = HTMLWorker.ParseToList(new StringReader(contents), null);
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
                strAdministrativeLandDetails += "<td width='35%'><p><b>Type</b></p></td>";
                //strAdministrativeLandDetails += "<td width='18%'><p align='left'><b>Program</b></p></td>";
                strAdministrativeLandDetails += "<td width='9%'><p align='center'><b>Required Rooms</b></p></td>";
                strAdministrativeLandDetails += "<td width='9%'><p align='center'><b>Uploaded Rooms</b></p></td>";
                strAdministrativeLandDetails += "<td width='5%'><p align='center'><b>CF</b></p></td>";
                strAdministrativeLandDetails += "<td width='9%'><p align='center'><b>Required Area</b></p></td>";
                strAdministrativeLandDetails += "<td width='9%'><p align='center'><b>Uploaded Area</b></p></td>";
                strAdministrativeLandDetails += "<td width='5%'><p align='center'><b>CF</b></p></td>";
                strAdministrativeLandDetails += "</tr>";
                IQueryable<jntuh_college_area> jntuh_college_area = db.jntuh_college_area.Where(s => s.collegeId == collegeId).Select(e => e);
                IQueryable<jntuh_program_type> jntuh_program_type = db.jntuh_program_type.Select(e => e);
                int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
                int prAy = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();

                List<int> collegeDegrees = db.jntuh_college_degree
                                         .Join(db.jntuh_degree, cd => cd.degreeId, d => d.id, (cd, d) => new { cd, d })
                                         .Where(s => s.d.isActive == true && s.cd.collegeId == collegeId && s.cd.isActive == true)
                                         .OrderBy(s => s.d.degreeDisplayOrder)
                                         .Select(s => s.d.id).ToList();

                var collegeSpecs = (from ie in db.jntuh_college_intake_existing
                                    join s in db.jntuh_specialization on ie.specializationId equals s.id
                                    join d in db.jntuh_department on s.departmentId equals d.id
                                    join de in db.jntuh_degree on d.degreeId equals de.id
                                    where ie.academicYearId == (prAy) && (ie.proposedIntake != 0) && ie.collegeId == collegeId && de.id == 4
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

                        var requiredRooms = string.Empty;
                        var requiredArea = string.Empty;
                        //if (item.id == 6) // FacultyRooms
                        //{
                        //    requiredRooms = collegeFacultyCount.ToString();
                        //    requiredArea = collegeFacultyCount * 5;
                        //}
                        //else if (item.id == 7) // Cabin for HOD
                        //{
                        //    requiredRooms = rrCount.ToString();
                        //    requiredArea = rrCount * 20;
                        //}
                        //else
                        //{
                        //    requiredRooms = Convert.ToInt16(item.requiredRooms).ToString();
                        //    requiredArea = Convert.ToDecimal(item.requiredArea);
                        //}

                        strAdministrativeLandDetails += "<tr>";
                        strAdministrativeLandDetails += "<td width='35%'><p>" + item.requirementType + "</p></td>";
                        //strAdministrativeLandDetails += "<td width='18%'>" + programType + "</td>";
                        strAdministrativeLandDetails += "<td width='9%' align='center'>" + requiredRooms + "</td>";
                        if (item.availableRooms != null)
                        {
                            strAdministrativeLandDetails += "<td width='9%' align='center'>" + (int)item.availableRooms + "</td>";
                        }
                        else
                        {
                            strAdministrativeLandDetails += "<td width='9%' align='center'>" + item.availableRooms + "</td>";
                        }
                        strAdministrativeLandDetails += "<td width='5%' valign='top' align='center'></td>";
                        strAdministrativeLandDetails += "<td width='9%' align='center'>" + requiredArea + "</td>";
                        strAdministrativeLandDetails += "<td width='9%' align='center'>" + item.availableArea + "</td>";
                        strAdministrativeLandDetails += "<td width='5%' valign='top' align='center'></td>";
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
                strInstructionalAreaDetails += "<tr style='background:#808080'><td width='28%'><p><b>Requirement Type</b></p></td><td width='10%'><p align='center'><b>Required Rooms</b></p></td><td width='10%'><p align='center'><b>Uploaded Rooms</b></p></td><td width='5%'><p align='center'><b>CF</b></p></td><td width='10%'><p align='center'><b>Required Area</b></p></td><td width='10%'><p align='center'><b>Uploaded Area</b></p></td><td width='5%'><p align='center'><b>CF</b></p></td></tr>";
                int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
                int prAy = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();

                //var collegeSpecs = (from ie in db.jntuh_college_intake_existing
                //                    join s in db.jntuh_specialization on ie.specializationId equals s.id
                //                    join d in db.jntuh_department on s.departmentId equals d.id
                //                    join de in db.jntuh_degree on d.degreeId equals de.id
                //                    where ie.academicYearId == (prAy) && (ie.proposedIntake != 0) && ie.collegeId == collegeId && de.id == 4
                //                    orderby de.degreeDisplayOrder
                //                    select d.id
                //).Distinct().ToList();

                //var collegeSpecApprovedSum = (from ie in db.jntuh_approvedadmitted_intake
                //                              join s in db.jntuh_specialization on ie.SpecializationId equals s.id
                //                              join d in db.jntuh_department on s.departmentId equals d.id
                //                              join de in db.jntuh_degree on d.degreeId equals de.id
                //                              where (ie.AcademicYearId == (prAy - 1) || ie.AcademicYearId == (prAy - 2) || ie.AcademicYearId == (prAy - 3)) && (ie.ApprovedIntake != 0) && ie.collegeId == collegeId && de.id == 4
                //                              orderby de.degreeDisplayOrder
                //                              select ie.ApprovedIntake
                //).Sum();

                //var collegeSpecSum = (from ie in db.jntuh_college_intake_existing
                //                      join s in db.jntuh_specialization on ie.specializationId equals s.id
                //                      join d in db.jntuh_department on s.departmentId equals d.id
                //                      join de in db.jntuh_degree on d.degreeId equals de.id
                //                      where ie.academicYearId == (prAy) && (ie.proposedIntake != 0) && ie.collegeId == collegeId && de.id == 4
                //                      orderby de.degreeDisplayOrder
                //                      select ie.proposedIntake
                //).Sum();

                //var MtechcollegeSpecs = (from ie in db.jntuh_college_intake_existing
                //                         join s in db.jntuh_specialization on ie.specializationId equals s.id
                //                         join d in db.jntuh_department on s.departmentId equals d.id
                //                         join de in db.jntuh_degree on d.degreeId equals de.id
                //                         where ie.academicYearId == (prAy) && (ie.proposedIntake != 0) && ie.collegeId == collegeId && de.id == 1
                //                         orderby de.degreeDisplayOrder
                //                         select d.id
                //).Distinct().ToList();

                //var MtechcollegeSpecSum = (from ie in db.jntuh_college_intake_existing
                //                           join s in db.jntuh_specialization on ie.specializationId equals s.id
                //                           join d in db.jntuh_department on s.departmentId equals d.id
                //                           join de in db.jntuh_degree on d.degreeId equals de.id
                //                           where ie.academicYearId == (prAy) && (ie.proposedIntake != 0) && ie.collegeId == collegeId && de.id == 1
                //                           orderby de.degreeDisplayOrder
                //                           select ie.proposedIntake
                //).Sum();

                //var MBAProposedSpecSum = (from ie in db.jntuh_college_intake_existing
                //                          join s in db.jntuh_specialization on ie.specializationId equals s.id
                //                          join d in db.jntuh_department on s.departmentId equals d.id
                //                          join de in db.jntuh_degree on d.degreeId equals de.id
                //                          where ie.academicYearId == (prAy) && (ie.proposedIntake != 0) && ie.collegeId == collegeId && de.id == 6
                //                          orderby de.degreeDisplayOrder
                //                          select ie.proposedIntake
                //).Sum();

                ////var MBAApprovedSum = (from ie in db.jntuh_approvedadmitted_intake
                ////                      join s in db.jntuh_specialization on ie.SpecializationId equals s.id
                ////                      join d in db.jntuh_department on s.departmentId equals d.id
                ////                      join de in db.jntuh_degree on d.degreeId equals de.id
                ////                      where (ie.AcademicYearId == (prAy - 1)) && (ie.ApprovedIntake != 0) && ie.collegeId == collegeId && de.id == 6
                ////                      orderby de.degreeDisplayOrder
                ////                      select ie.ApprovedIntake
                ////).Sum();

                //var MCAProposedSpecSum = (from ie in db.jntuh_college_intake_existing
                //                          join s in db.jntuh_specialization on ie.specializationId equals s.id
                //                          join d in db.jntuh_department on s.departmentId equals d.id
                //                          join de in db.jntuh_degree on d.degreeId equals de.id
                //                          where ie.academicYearId == (prAy) && (ie.proposedIntake != 0) && ie.collegeId == collegeId && de.id == 3
                //                          orderby de.degreeDisplayOrder
                //                          select ie.proposedIntake
                //).Sum();

                ////var MCAApprovedSum = (from ie in db.jntuh_approvedadmitted_intake
                ////                      join s in db.jntuh_specialization on ie.SpecializationId equals s.id
                ////                      join d in db.jntuh_department on s.departmentId equals d.id
                ////                      join de in db.jntuh_degree on d.degreeId equals de.id
                ////                      where (ie.AcademicYearId == (prAy - 1)) && (ie.ApprovedIntake != 0) && ie.collegeId == collegeId && de.id == 3
                ////                      orderby de.degreeDisplayOrder
                ////                      select ie.ApprovedIntake
                ////).Sum();

                //var mPharmacySpecSum = (from ie in db.jntuh_college_intake_existing
                //                    join s in db.jntuh_specialization on ie.specializationId equals s.id
                //                    join d in db.jntuh_department on s.departmentId equals d.id
                //                    join de in db.jntuh_degree on d.degreeId equals de.id
                //                    where ie.academicYearId == (prAy) && (ie.proposedIntake != 0) && ie.collegeId == collegeId && de.id == 2
                //                    orderby de.degreeDisplayOrder
                //                    select d.id
                //).Distinct().ToList();

                List<string> collegeDegrees = db.jntuh_college_degree
                                         .Join(db.jntuh_degree, cd => cd.degreeId, d => d.id, (cd, d) => new { cd, d })
                                         .Where(s => s.d.isActive == true && s.cd.collegeId == collegeId && s.cd.isActive == true)
                                         .OrderBy(s => s.d.degreeDisplayOrder)
                                         .Select(s => s.d.degree).ToList();

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
                                     availableArea = jntuh_college_area.Where(a => a.areaRequirementId == r.id).Select(a => a.availableArea).FirstOrDefault()
                                 }).ToList();

                    strInstructionalAreaDetails += "<tr>";
                    strInstructionalAreaDetails += "<td colspan='7' style='width: 200%'><p><b>" + programTypeDegree.programType + "</b></p></td>";
                    strInstructionalAreaDetails += "</tr>";
                    foreach (var i in land)
                    {
                        var requiredRooms = string.Empty;
                        var requiredArea = string.Empty;

                        //if (i.programId == 4)
                        //{
                        //    if (i.id == 27) // ClassRooms - B.Tech
                        //    {
                        //        var calc = ((collegeSpecSum + collegeSpecApprovedSum) / 60) * 0.5;
                        //        var rrooms = (int)Math.Ceiling(Convert.ToDouble(calc));
                        //        var rarea = rrooms * 66;
                        //        requiredRooms = rrooms.ToString();
                        //        requiredArea = rarea.ToString();
                        //    }
                        //    else if (i.id == 28) // TutorialRooms - B.Tech
                        //    {
                        //        var calc = ((collegeSpecSum + collegeSpecApprovedSum) / 60) * 0.5;
                        //        var rrooms = (int)Math.Ceiling(Convert.ToDouble(calc)) * 0.25;
                        //        var rarea = rrooms * 33;
                        //        requiredRooms = Math.Ceiling(rrooms).ToString();
                        //        requiredArea = Math.Ceiling(rarea).ToString();
                        //    }
                        //    else if (i.id == 30) // Workshop (all courses) - B.Tech
                        //    {
                        //        if (collegeSpecSum <= 600)
                        //        {
                        //            requiredRooms = Convert.ToInt16(i.requiredRooms).ToString();
                        //            requiredArea = Convert.ToDecimal(i.requiredArea).ToString();
                        //        }
                        //        else if (collegeSpecSum > 600 && collegeSpecSum <= 1200)
                        //        {
                        //            requiredRooms = "2";
                        //            requiredArea = "400";
                        //        }
                        //        else if (collegeSpecSum > 1200 && collegeSpecSum <= 1800)
                        //        {
                        //            requiredRooms = "3";
                        //            requiredArea = "600";
                        //        }
                        //        else
                        //        {
                        //            requiredRooms = "4";
                        //            requiredArea = "800";
                        //        }
                        //    }
                        //    else if (i.id == 32) // Computer Center - B.Tech
                        //    {
                        //        if (collegeSpecSum <= 600)
                        //        {
                        //            requiredRooms = Convert.ToInt16(i.requiredRooms).ToString();
                        //            requiredArea = Convert.ToDecimal(i.requiredArea).ToString();
                        //        }
                        //        else if (collegeSpecSum > 600 && collegeSpecSum <= 1200)
                        //        {
                        //            requiredRooms = "2";
                        //            requiredArea = "300";
                        //        }
                        //        else if (collegeSpecSum > 1200 && collegeSpecSum <= 1800)
                        //        {
                        //            requiredRooms = "3";
                        //            requiredArea = "450";
                        //        }
                        //        else
                        //        {
                        //            requiredRooms = "4";
                        //            requiredArea = "600";
                        //        }
                        //    }
                        //    else if (i.id == 33) // Drawing Hall/CAD Centre - B.Tech
                        //    {
                        //        if (collegeSpecSum <= 600)
                        //        {
                        //            requiredRooms = Convert.ToInt16(i.requiredRooms).ToString();
                        //            requiredArea = Convert.ToDecimal(i.requiredArea).ToString();
                        //        }
                        //        else if (collegeSpecSum > 600 && collegeSpecSum <= 1200)
                        //        {
                        //            requiredRooms = "2";
                        //            requiredArea = "264";
                        //        }
                        //        else if (collegeSpecSum > 1200 && collegeSpecSum <= 1800)
                        //        {
                        //            requiredRooms = "3";
                        //            requiredArea = "396";
                        //        }
                        //        else
                        //        {
                        //            requiredRooms = "4";
                        //            requiredArea = "528";
                        //        }
                        //    }
                        //    else if (i.id == 34) // Library - B.Tech
                        //    {
                        //        requiredRooms = Convert.ToInt16(i.requiredRooms).ToString();
                        //        var calc = ((collegeSpecApprovedSum - 420) / 60) * 50;
                        //        requiredArea = calc.ToString();
                        //    }
                        //    else if (i.id == 94) // Language Laboratory - B.Tech
                        //    {
                        //        var rrooms = collegeSpecSum / 300;
                        //        var rarea = rrooms * 33;
                        //        requiredRooms = Math.Ceiling(Convert.ToDecimal(rrooms)).ToString();
                        //        requiredArea = rarea.ToString();
                        //    }
                        //    else if (i.id == 29) // Laboratory other than first Year - B.Tech
                        //    {
                        //        var rrooms = (collegeSpecs.Count * 2) + 4;
                        //        var rarea = rrooms * 66;
                        //        requiredRooms = rrooms.ToString();
                        //        requiredArea = rarea.ToString();
                        //    }
                        //    else
                        //    {
                        //        requiredRooms = Convert.ToInt16(i.requiredRooms).ToString();
                        //        requiredArea = Convert.ToDecimal(i.requiredArea).ToString();
                        //    }
                        //}
                        //else if (i.programId == 5)
                        //{
                        //    if (i.id == 36) // ClassRooms - M.Tech
                        //    {
                        //        var calc = MtechcollegeSpecs.Count * 2;
                        //        var rrooms = (int)Math.Ceiling(Convert.ToDouble(calc));
                        //        var rarea = rrooms * 33;
                        //        requiredRooms = rrooms.ToString();
                        //        requiredArea = rarea.ToString();
                        //    }
                        //    else if (i.id == 37) // Laboratory - M.Tech
                        //    {
                        //        var rrooms = (MtechcollegeSpecs.Count);
                        //        var rarea = rrooms * 66;
                        //        requiredRooms = rrooms.ToString();
                        //        requiredArea = rarea.ToString();
                        //    }
                        //    else if (i.id == 38) // Research Laboratory - M.Tech
                        //    {
                        //        var rrooms = (MtechcollegeSpecs.Count);
                        //        var rarea = rrooms * 66;
                        //        requiredRooms = rrooms.ToString();
                        //        requiredArea = rarea.ToString();
                        //    }
                        //    else
                        //    {
                        //        requiredRooms = Convert.ToInt16(i.requiredRooms).ToString();
                        //        requiredArea = Convert.ToDecimal(i.requiredArea).ToString();
                        //    }
                        //}
                        //else if (i.programId == 8)
                        //{
                        //    if (i.id == 58) // ClassRooms - MBA
                        //    {
                        //        var calc = ((MBAProposedSpecSum * 2 ) / 60);
                        //        var rrooms = (int)Math.Ceiling(Convert.ToDouble(calc));
                        //        var rarea = rrooms * 66;
                        //        requiredRooms = rrooms.ToString();
                        //        requiredArea = rarea.ToString();
                        //    }
                        //    else if (i.id == 59) // TutorialRooms - MBA
                        //    {
                        //        var calc = ((MBAProposedSpecSum * 2) / 60);
                        //        var rrooms = (int)Math.Ceiling(Convert.ToDouble(calc)) * 0.25;
                        //        var rarea = rrooms * 33;
                        //        requiredRooms = Math.Ceiling(rrooms).ToString();
                        //        requiredArea = Math.Ceiling(rarea).ToString();
                        //    }
                        //    else
                        //    {
                        //        requiredRooms = Convert.ToInt16(i.requiredRooms).ToString();
                        //        requiredArea = Convert.ToDecimal(i.requiredArea).ToString();
                        //    }
                        //}
                        //else if (i.programId == 9)
                        //{
                        //    if (i.id == 63) // ClassRooms - MCA
                        //    {
                        //        var calc = ((MCAProposedSpecSum * 2) / 60);
                        //        var rrooms = (int)Math.Ceiling(Convert.ToDouble(calc));
                        //        var rarea = rrooms * 66;
                        //        requiredRooms = rrooms.ToString();
                        //        requiredArea = rarea.ToString();
                        //    }
                        //    else if (i.id == 64) // TutorialRooms - MCA
                        //    {
                        //        var calc = ((MCAProposedSpecSum * 2) / 60);
                        //        var rrooms = (int)Math.Ceiling(Convert.ToDouble(calc)) * 0.25;
                        //        var rarea = rrooms * 33;
                        //        requiredRooms = Math.Ceiling(rrooms).ToString();
                        //        requiredArea = Math.Ceiling(rarea).ToString();
                        //    }
                        //    else
                        //    {
                        //        requiredRooms = Convert.ToInt16(i.requiredRooms).ToString();
                        //        requiredArea = Convert.ToDecimal(i.requiredArea).ToString();
                        //    }
                        //}
                        //else if (i.programId == 6)
                        //{
                        //    requiredRooms = Convert.ToInt16(i.requiredRooms).ToString();
                        //    requiredArea = Convert.ToDecimal(i.requiredArea).ToString();
                        //}
                        //else if (i.programId == 7)
                        //{
                        //    if (i.id == 51) // ClassRooms - M.Pharmacy
                        //    {
                        //        var rrooms = mPharmacySpecSum.Count * 2;
                        //        var rarea = rrooms * 36;
                        //        requiredRooms = rrooms.ToString();
                        //        requiredArea = rarea.ToString();
                        //    }
                        //    else if (i.id == 52) // ClassRooms - M.Pharmacy
                        //    {
                        //        var rrooms = mPharmacySpecSum.Count * 2;
                        //        var rarea = (rrooms * 75) + 10;
                        //        requiredRooms = rrooms.ToString();
                        //        requiredArea = rarea.ToString();
                        //    }
                        //    else
                        //    {
                        //        requiredRooms = Convert.ToInt16(i.requiredRooms).ToString();
                        //        requiredArea = Convert.ToDecimal(i.requiredArea).ToString();
                        //    }
                        //}
                        if (requiredRooms == "" || requiredArea == "")
                        {
                            requiredRooms = "";
                            requiredArea = "";
                        }

                        strInstructionalAreaDetails += "<tr>";
                        strInstructionalAreaDetails += "<td width='28%'><p>" + i.requirementType + "</p></td>";
                        strInstructionalAreaDetails += "<td width='10%' align='center'> " + requiredRooms + " </td>";
                        if (i.availableRooms != null)
                        {
                            strInstructionalAreaDetails += "<td width='10%' align='center'>" + (int)i.availableRooms + "</td>";
                        }
                        else
                        {
                            strInstructionalAreaDetails += "<td width='10%' align='center'>" + i.availableRooms + "</td>";
                        }
                        strInstructionalAreaDetails += "<td width='5%' valign='top' align='center'></td>";
                        strInstructionalAreaDetails += "<td width='10%' align='center'> " + requiredArea + " </td>";
                        strInstructionalAreaDetails += "<td width='10%' align='center'>" + i.availableArea + "</td>";
                        strInstructionalAreaDetails += "<td width='5%' valign='top' align='center'></td>";
                        strInstructionalAreaDetails += "</tr>";
                        if (i.availableArea != null)
                        {
                            totalArea += (int)i.availableArea;
                        }
                    }
                    //}
                }

                //Degree related requirement types
                //List<AdminLand> programIds = (from a in db.jntuh_college_area
                //                              join ar in db.jntuh_area_requirement
                //                              on a.areaRequirementId equals ar.id
                //                              where (ar.isActive && ar.areaType == "INSTRUCTIONAL" && a.collegeId == collegeId)
                //                              orderby ar.areaTypeDisplayOrder
                //                              select new AdminLand
                //                              {
                //                                  programId = ar.programId
                //                              }).Distinct().ToList();
                //var ids = programIds.Select(it => it.programId).ToList();
                //var programtypes = db.jntuh_program_type.Where(d => d.isActive == true && ids.Contains(d.id)).ToList();
                //if (programIds != null)
                //{
                //    foreach (var item in programIds)
                //    {
                //        int programId = (int)item.programId;
                //        string programType = programtypes.Where(it => it.id == item.programId).Select(d => d.programType).FirstOrDefault();
                //        List<AdminLand> land = db.jntuh_area_requirement.Where(r => r.isActive == true && r.areaType == "INSTRUCTIONAL" && r.programId == programId).OrderBy(r => r.areaTypeDisplayOrder)
                //                     .Select(r => new AdminLand
                //                     {
                //                         id = r.id,
                //                         requirementType = r.requirementType,
                //                         programId = r.programId,
                //                         requiredRooms = r.requiredRooms,
                //                         requiredRoomsCalculation = r.requiredRoomsCalculation,
                //                         requiredArea = r.requiredArea,
                //                         requiredAreaCalculation = r.requiredAreaCalculation,
                //                         areaTypeDescription = r.areaTypeDescription,
                //                         areaTypeDisplayOrder = r.areaTypeDisplayOrder,
                //                         jntuh_program_type = db.jntuh_program_type.Where(p => p.id == r.programId).FirstOrDefault(),
                //                         availableRooms = jntuh_college_area.Where(a => a.areaRequirementId == r.id).Select(a => a.availableRooms).FirstOrDefault(),
                //                         availableArea = jntuh_college_area.Where(a => a.areaRequirementId == r.id).Select(a => a.availableArea).FirstOrDefault()
                //                     }).Where(g => g.availableRooms != null && g.availableRooms != 0).ToList();
                //        if (land != null)
                //        {
                //            strInstructionalAreaDetails += "<tr>";
                //            strInstructionalAreaDetails += "<td colspan='7' style='width: 200%'><p><b>" + programType + "</b></p></td>";
                //            strInstructionalAreaDetails += "</tr>";
                //            foreach (var i in land)
                //            {
                //                strInstructionalAreaDetails += "<tr>";
                //                strInstructionalAreaDetails += "<td width='28%'><p>" + i.requirementType + "</p></td>";
                //                strInstructionalAreaDetails += "<td width='10%' align='center'> 1.00 </td>";
                //                if (i.availableRooms != null)
                //                {
                //                    strInstructionalAreaDetails += "<td width='10%' align='center'>" + (int)i.availableRooms + "</td>";
                //                }
                //                else
                //                {
                //                    strInstructionalAreaDetails += "<td width='10%' align='center'>" + i.availableRooms + "</td>";
                //                }
                //                strInstructionalAreaDetails += "<td width='5%' valign='top' align='center'></td>";
                //                strInstructionalAreaDetails += "<td width='10%' align='center'> 0.00 </td>";
                //                strInstructionalAreaDetails += "<td width='10%' align='center'>" + i.availableArea + "</td>";
                //                strInstructionalAreaDetails += "<td width='5%' valign='top' align='center'></td>";
                //                strInstructionalAreaDetails += "</tr>";
                //                if (i.availableArea != null)
                //                {
                //                    totalArea += (int)i.availableArea;
                //                }
                //            }
                //        }
                //    }

                //}
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
                strExistingIntakeDetails += "<td width='550' colspan='9'><p align='center'>Sanctioned & Actual Admitted Intake as per Academic Year</p></td>";
                strExistingIntakeDetails += "<td  rowspan='3' colspan='1'><p align='left'>PI</p></td>";
                strExistingIntakeDetails += "<td  rowspan='3' colspan='1'><p align='left'>CS</p></td>";
                strExistingIntakeDetails += "<td  rowspan='3' colspan='1'><p align='left'>CF</p></td>";
                strExistingIntakeDetails += "<td width='100' colspan='4' valign='top'><p align='center'>NBA accreditation Period (if exists)</p></td></tr>";
                strExistingIntakeDetails += "<tr><td width='100' colspan='3'><p align='center'>" + ThirdYear + "</p></td>";
                strExistingIntakeDetails += "<td width='100' colspan='3'><p align='center'>" + SecondYear + "</p></td>";
                strExistingIntakeDetails += "<td width='100' colspan='3'><p align='center'>" + FirstYear + "</p></td>";


                strExistingIntakeDetails += "<td width='100' colspan='4' valign='top'><p align='center' style='font-style: 7px;'>(DD/MM/YYY)</p></td>";
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
                strExistingIntakeDetails += "<td width='50' colspan='2' valign='top'><p align='center'>From</p></td>";
                strExistingIntakeDetails += "<td width='50' colspan='2' valign='top'><p align='center'>To</p></td>";
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
                    if (PgDegreeIds.Contains(item.degreeID))
                        strExistingIntakeDetails += "<td colspan='1' width='50'>" + item.admittedIntake3.ToString() + "</td>";
                    else
                        strExistingIntakeDetails += "<td colspan='1' width='50'>" + item.ExambranchadmittedIntakeR3.ToString() + "</td>";
                    strExistingIntakeDetails += "<td colspan='1' width='50'>" + item.AICTEapprovedIntake2.ToString() + "</td>";
                    strExistingIntakeDetails += "<td colspan='1' width='50'>" + item.approvedIntake2.ToString() + "</td>";
                    if (PgDegreeIds.Contains(item.degreeID))
                        strExistingIntakeDetails += "<td colspan='1' width='50'>" + item.admittedIntake2.ToString() + "</td>";
                    else
                        strExistingIntakeDetails += "<td colspan='1' width='50'>" + item.ExambranchadmittedIntakeR2.ToString() + "</td>";
                    strExistingIntakeDetails += "<td colspan='1' width='50'>" + item.AICTEapprovedIntake1.ToString() + "</td>";
                    strExistingIntakeDetails += "<td colspan='1' width='42'>" + item.approvedIntake1.ToString() + "</td>";
                    if (PgDegreeIds.Contains(item.degreeID))
                        strExistingIntakeDetails += "<td colspan='1' width='50'>" + item.admittedIntake1.ToString() + "</td>";
                    else
                        strExistingIntakeDetails += "<td colspan='1' width='50'>" + item.ExambranchadmittedIntakeR1.ToString() + "</td>";
                    strExistingIntakeDetails += "<td colspan='1'>" + item.ProposedIntake + "</td>";
                    strExistingIntakeDetails += "<td colspan='1'>" + item.courseStatus + "</td>";
                    strExistingIntakeDetails += "<td colspan='1'></td>";
                    strExistingIntakeDetails += "<td colspan='2' style='font-size: 8px;' width='50'>" + item.nbaFromDate + "</td>";
                    strExistingIntakeDetails += "<td colspan='2' style='font-size: 8px;' width='50'>" + item.nbaToDate + "</td>";
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
                strExistingIntakeDetails += "<td width='50' colspan='1' align='center'>" + totalPAYApproved + "</td>";
                strExistingIntakeDetails += "<td width='50' colspan='1' align='center'>" + totalPAYProposed + "</td>";
                strExistingIntakeDetails += "<td width='50' colspan='1' valign='top' align='center'></td>";
                strExistingIntakeDetails += "<td width='50' colspan='2' valign='top' align='center'></td>";
                strExistingIntakeDetails += "<td width='50' colspan='2' valign='top' align='center'></td>";
                strExistingIntakeDetails += "</tr>";
                strExistingIntakeDetails += "<tr><td colspan='13'><p align='right'>Total Admitted / Total Sanctioned =</p></td><td colspan='18' width='600'>" + totalExaminationBranchAdmittedIntake + '/' + totalApproved + "</td></tr>";
                strExistingIntakeDetails += "<tr><td  colspan='13'><p align='right'>Total AICTE ApprovedIntake</p></td><td colspan='18' width='600'>" + totalAICTEApproved + "</td></tr>";
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

        private int GetIntake(int collegeId, int academicYearId, int specializationId, int shiftId, int flag)
        {
            using (var db = new uaaasDBContext())
            {
                int intake = 0;

                if (flag == 1)
                    intake = db.jntuh_college_intake_existing.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == shiftId).Select(i => i.approvedIntake).FirstOrDefault();
                else if (flag == 0)
                    intake = db.jntuh_college_intake_existing.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == shiftId).Select(i => i.admittedIntake).FirstOrDefault();
                else
                    intake = db.jntuh_college_intake_existing.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == shiftId).Select(i => i.aicteApprovedIntake).FirstOrDefault();
                return intake;
            }
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

        public class CollegeDegree
        {
            public int id { get; set; }
            public string degree { get; set; }
        }

        //A417
        public string LaboratoriesDetails(int collegeId, string contents)
        {
            using (var db = new uaaasDBContext())
            {
                string strLaboratoriesDetails = string.Empty;
                int sno = 1;
                int SpecializationId = 0;
                string strviewEquimentdata = string.Empty;
                string Equipmentdate = string.Empty;
                string chalanaDate = string.Empty;
                int LabsCount = 0;
                var actualYear = db.jntuh_academic_year.Where(s => s.isActive == true && s.isPresentAcademicYear == true).Select(s => s.actualYear).FirstOrDefault();
                var PrevoiusYearId = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear)).Select(s => s.id).FirstOrDefault();
                var PresentYearId = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();

                int[] collegeSpecializationIDs = db.jntuh_college_intake_existing.Where(e => e.collegeId == collegeId && e.academicYearId == PrevoiusYearId).Select(e => e.specializationId).Distinct().ToArray();
                string CollegeAffiliationStatus = db.jntuh_college_affiliation.Where(u => u.collegeId == collegeId && u.affiliationTypeId == 7).Select(u => u.affiliationStatus).FirstOrDefault();
                int[] Equipmentsids = db.jntuh_college_laboratories.Where(C => C.CollegeID == collegeId).Select(C => C.EquipmentID).ToArray();
                int[] DegreeIds = db.jntuh_lab_master.Where(L => Equipmentsids.Contains(L.id)).Select(L => L.DegreeID).Distinct().ToArray();
                if (DegreeIds.Contains(4))
                    SpecializationId = 39;
                else
                    SpecializationId = 0;


                List<Lab> labs = new List<Lab>();

                if (CollegeAffiliationStatus == "Yes")
                {
                    List<Lab> labsuploaded = new List<Lab>();
                    List<Lab> labsnotuploaded = new List<Lab>();

                    labsuploaded = (from lm in db.jntuh_lab_master.AsNoTracking()
                                    join l in db.jntuh_college_laboratories.AsNoTracking() on lm.id equals l.EquipmentID
                                    where lm.CollegeId == collegeId && Equipmentsids.Contains(lm.id)
                                    //(collegeSpecializationIDs.Contains(lm.SpecializationID) || lm.SpecializationID == SpecializationId) &&
                                    select new Lab
                                    {
                                        EquipmentID = lm.id,
                                        tempEquipmentId = l.EquipmentID,
                                        degree = lm.jntuh_degree.degree,
                                        degreeId = lm.DegreeID,
                                        department = lm.jntuh_department.departmentName,
                                        specializationName = lm.jntuh_specialization.specializationName,
                                        Semester = lm.Semester,
                                        year = lm.Year,
                                        Labcode = lm.Labcode,
                                        LabName = lm.LabName,
                                        AvailableArea = l.AvailableArea,
                                        RoomNumber = l.RoomNumber,
                                        EquipmentName = lm.EquipmentName ?? l.EquipmentName,
                                        Make = l.Make,
                                        Model = l.Model,
                                        EquipmentUniqueID = l.EquipmentUniqueID,
                                        AvailableUnits = l.AvailableUnits,
                                        specializationId = lm.SpecializationID,
                                        CircuitType = lm.jntuh_department.CircuitType == false ? "A" : "B",
                                        isCircuit = lm.jntuh_department.CircuitType,
                                        DisplayOrder = lm.jntuh_department.DisplayOrder,
                                        degreeTypeId = lm.jntuh_degree.degreeTypeId,
                                        ViewEquipmentPhoto = l.EquipmentPhoto,
                                        EquipmentDateOfPurchasing = l.EquipmentDateOfPurchasing,
                                        DelivaryChalanaDate = l.DelivaryChalanaDate
                                    }).ToList();

                    labsnotuploaded = (from lm in db.jntuh_lab_master.AsNoTracking()
                                       join l in db.jntuh_college_laboratories.AsNoTracking() on lm.id equals l.EquipmentID
                                       where lm.CollegeId == collegeId && !Equipmentsids.Contains(lm.id)
                                       //(collegeSpecializationIDs.Contains(lm.SpecializationID) || lm.SpecializationID == SpecializationId) && lm.CollegeId == collegeId  
                                       select new Lab
                                       {
                                           EquipmentID = lm.id,
                                           tempEquipmentId = l.EquipmentID,
                                           degree = lm.jntuh_degree.degree,
                                           degreeId = lm.DegreeID,
                                           department = lm.jntuh_department.departmentName,
                                           specializationName = lm.jntuh_specialization.specializationName,
                                           Semester = lm.Semester,
                                           year = lm.Year,
                                           Labcode = lm.Labcode,
                                           LabName = lm.LabName,
                                           AvailableArea = l.AvailableArea,
                                           RoomNumber = l.RoomNumber,
                                           EquipmentName = lm.EquipmentName ?? l.EquipmentName,
                                           Make = l.Make,
                                           Model = l.Model,
                                           EquipmentUniqueID = l.EquipmentUniqueID,
                                           AvailableUnits = l.AvailableUnits,
                                           specializationId = lm.SpecializationID,
                                           CircuitType = lm.jntuh_department.CircuitType == false ? "A" : "B",
                                           isCircuit = lm.jntuh_department.CircuitType,
                                           DisplayOrder = lm.jntuh_department.DisplayOrder,
                                           degreeTypeId = lm.jntuh_degree.degreeTypeId,
                                           ViewEquipmentPhoto = l.EquipmentPhoto,
                                           EquipmentDateOfPurchasing = l.EquipmentDateOfPurchasing,
                                           DelivaryChalanaDate = l.DelivaryChalanaDate
                                       }).ToList();

                    labs.AddRange(labsuploaded);
                    labs.AddRange(labsnotuploaded);
                }
                else
                {

                    List<Lab> labsuploaded = new List<Lab>();
                    List<Lab> labsnotuploaded = new List<Lab>();

                    labsuploaded = (from lm in db.jntuh_lab_master.AsNoTracking()
                                    join l in db.jntuh_college_laboratories.AsNoTracking() on lm.id equals l.EquipmentID
                                    where Equipmentsids.Contains(lm.id) && (collegeSpecializationIDs.Contains(lm.SpecializationID) || lm.SpecializationID == SpecializationId) && l.CollegeID == collegeId && lm.CollegeId == null
                                    select new Lab
                                    {
                                        EquipmentID = lm.id,
                                        tempEquipmentId = l.EquipmentID,
                                        degree = lm.jntuh_degree.degree,
                                        degreeId = lm.DegreeID,
                                        department = lm.jntuh_department.departmentName,
                                        specializationName = lm.jntuh_specialization.specializationName,
                                        Semester = lm.Semester,
                                        year = lm.Year,
                                        Labcode = lm.Labcode,
                                        LabName = lm.LabName,
                                        AvailableArea = l.AvailableArea,
                                        RoomNumber = l.RoomNumber,
                                        EquipmentName = lm.EquipmentName ?? l.EquipmentName,
                                        Make = l.Make,
                                        Model = l.Model,
                                        EquipmentUniqueID = l.EquipmentUniqueID,
                                        AvailableUnits = l.AvailableUnits,
                                        specializationId = lm.SpecializationID,
                                        CircuitType = lm.jntuh_department.CircuitType == false ? "A" : "B",
                                        isCircuit = lm.jntuh_department.CircuitType,
                                        DisplayOrder = lm.jntuh_department.DisplayOrder,
                                        degreeTypeId = lm.jntuh_degree.degreeTypeId,
                                        ViewEquipmentPhoto = l.EquipmentPhoto,
                                        EquipmentDateOfPurchasing = l.EquipmentDateOfPurchasing,
                                        DelivaryChalanaDate = l.DelivaryChalanaDate
                                    }).ToList();

                    labsnotuploaded = (from lm in db.jntuh_lab_master.AsNoTracking()
                                       // join l in db.jntuh_college_laboratories.AsNoTracking() on lm.id equals l.EquipmentID
                                       where !Equipmentsids.Contains(lm.id) && (collegeSpecializationIDs.Contains(lm.SpecializationID) || lm.SpecializationID == SpecializationId) && lm.CollegeId == null
                                       select new Lab
                                       {
                                           EquipmentID = lm.id,
                                           tempEquipmentId = 0,//l.EquipmentID,
                                           degree = lm.jntuh_degree.degree,
                                           degreeId = lm.DegreeID,
                                           department = lm.jntuh_department.departmentName,
                                           specializationName = lm.jntuh_specialization.specializationName,
                                           Semester = lm.Semester,
                                           year = lm.Year,
                                           Labcode = lm.Labcode,
                                           LabName = lm.LabName,
                                           AvailableArea = 0,
                                           RoomNumber = string.Empty,
                                           EquipmentName = lm.EquipmentName ?? "",
                                           Make = string.Empty,
                                           Model = string.Empty,
                                           EquipmentUniqueID = string.Empty,
                                           AvailableUnits = 0,
                                           specializationId = lm.SpecializationID,
                                           CircuitType = lm.jntuh_department.CircuitType == false ? "A" : "B",
                                           isCircuit = lm.jntuh_department.CircuitType,
                                           DisplayOrder = lm.jntuh_department.DisplayOrder,
                                           degreeTypeId = lm.jntuh_degree.degreeTypeId,
                                           ViewEquipmentPhoto = string.Empty,
                                           EquipmentDateOfPurchasing = null,
                                           DelivaryChalanaDate = null
                                       }).ToList();


                    labs.AddRange(labsuploaded);
                    labs.AddRange(labsnotuploaded);
                }
                int[] CseSpecs = new[] { 2, 6, 177, 178, 179, 180, 181, 182, 183, 184, 185, 186, 187, 188, 189 };
                List<int> SpecializationIDs = new List<int>() { };
                //int[] CollegeIds = new int[] { 9, 39, 42, 75, 364 }; // 'WJ','TK','7Q','7Z','D0'
                //int[] pharmacySpecIds = new int[] { 12, 13, 18, 19, 114, 115, 116, 117, 118, 119, 120, 121, 122, 123, 124, 167, 169, 170, 171, 172 }; // 'WJ','TK','7Q','7Z','D0'
                //if (CollegeIds.Contains(collegeId))
                //{
                //    labs = labs.Where(i => pharmacySpecIds.Contains(i.specializationId)).ToList();
                //}

                if (DegreeIds.Contains(4))
                    // SpecializationIDs =(from a in labs orderby a.department,a.degree select a.specializationId).Distinct().ToList();
                    SpecializationIDs = labs.OrderBy(s => s.department).ThenBy(s => s.degree).Select(s => s.specializationId).Distinct().ToList();
                //SpecializationIDs = labs.Where(i => CseSpecs.Contains(i.specializationId)).OrderBy(s => s.department).ThenBy(s => s.degree).Select(s => s.specializationId).Distinct().ToList();
                else
                    // SpecializationIDs = (from a in labs where a.specializationId != 39 orderby a.department, a.degree select a.specializationId).Distinct().ToList();
                    SpecializationIDs = labs.Where(s => s.specializationId != 39).OrderBy(s => s.department).ThenBy(s => s.degree).Select(s => s.specializationId).Distinct().ToList();

                if (SpecializationIDs.Contains(39))
                    SpecializationIDs.Remove(39);

                if (DegreeIds.Contains(4))
                    SpecializationIDs.Add(39);

                var specializations = db.jntuh_specialization.Where(it => SpecializationIDs.Contains(it.id)).Select(s => new
                {
                    s.id,
                    specialization = s.specializationName,
                    department = s.jntuh_department.departmentName,
                    degree = s.jntuh_department.jntuh_degree.degree,
                    deptId = s.jntuh_department.id,

                }).OrderBy(e => e.deptId).ToList();

                if (CollegeAffiliationStatus != "Yes" && !SpecializationIDs.Contains(33) && !SpecializationIDs.Contains(43))
                    labs = labs.Where(x => x.Labcode != "PH105BS").ToList();


                foreach (var speclializationId in SpecializationIDs)
                {
                    string strLabName = string.Empty;
                    sno = 1;
                    var specializationDetails = specializations.FirstOrDefault(s => s.id == speclializationId);
                    var LabsData = labs.Where(l => l.specializationId == speclializationId).OrderBy(e => e.year).ThenBy(e => e.Semester).ThenBy(e => e.Labcode).ThenBy(e => e.LabName).GroupBy(s => new { s.year, s.Semester, s.Labcode }).Select(e => e.First()).ToList();
                    strLabName = "Degree: " + specializationDetails.degree + ",&nbsp;&nbsp;&nbsp; Department: " + specializationDetails.department + ",&nbsp;&nbsp;&nbsp; Specilization: " + specializationDetails.specialization;

                    strLaboratoriesDetails += "<table border='1' cellspacing='0' cellpadding='3'>";
                    strLaboratoriesDetails += "<thead>";
                    strLaboratoriesDetails += "<tr>";
                    strLaboratoriesDetails += "<th colspan='39'> <strong>" + strLabName + "</strong></th>";
                    strLaboratoriesDetails += "</tr>";
                    string strLaboratoriesDetails1 = "";
                    foreach (var item6 in LabsData)
                    {
                        var labnamebasedonlabcode = labs.Where(s => s.Labcode == item6.Labcode && s.year == item6.year && s.Semester == item6.Semester && s.specializationId == item6.specializationId).Select(e => e).ToList();
                        string LabcodeAndLabname = "Year :" + labnamebasedonlabcode.Select(e => e.year).FirstOrDefault() + ",&nbsp;&nbsp; Sem :" + labnamebasedonlabcode.Select(s => s.Semester).FirstOrDefault() + " LabCode : " + item6.Labcode + ",&nbsp;&nbsp;&nbsp; LabName : " + labnamebasedonlabcode.Select(s => s.LabName).FirstOrDefault() + " ";
                        strLaboratoriesDetails += "<tr><th colspan='39' style='font-size:11px;'><strong>" + LabcodeAndLabname + "</strong></th></tr>";
                        strLaboratoriesDetails += "<tr>";
                        strLaboratoriesDetails += "<th  colspan='1'><p align='center'>S.No</p></th>";
                        strLaboratoriesDetails += "<th  colspan='2'><p align='left'>Area (in Sqm)</p></th>";
                        strLaboratoriesDetails += "<th  colspan='3'><p align='left'>Room No</p></th>";
                        strLaboratoriesDetails += "<th  colspan='11'><p align='left'>Equipment Name</p></th>";
                        strLaboratoriesDetails += "<th  colspan='4'><p align='left'>Make</p></th>";
                        strLaboratoriesDetails += "<th  colspan='3'><p align='left'>Model</p></th>";
                        strLaboratoriesDetails += "<th  colspan='4'><p align='left'>Equipment UniqueID</p></th>";
                        strLaboratoriesDetails += "<th  colspan='3'><p align='left'>Equipment Photo</p></th>";
                        strLaboratoriesDetails += "<th  colspan='3'><p align='left'>Equipment DateOfPurchase </p></th>";
                        strLaboratoriesDetails += "<th  colspan='3'><p align='left'>Delivery ChallanDate</p></th>";
                        strLaboratoriesDetails += "<th  colspan='1'><p align='left'>AU</p></th>";
                        strLaboratoriesDetails += "<th  colspan='1'><p align='left'>AUWC</p></th>";
                        strLaboratoriesDetails += "</tr>";
                        strLaboratoriesDetails += "</thead>";

                        strLaboratoriesDetails += "<tbody>";

                        List<Lab> SpecializationWiseLabs = new List<Lab>();

                        foreach (var item in labs.Where(l => l.specializationId == speclializationId && l.Labcode == item6.Labcode && l.year == item6.year && l.Semester == item6.Semester).OrderBy(e => e.year).ThenBy(e => e.Semester).ToList())//
                        {
                            try
                            {
                                if (item.ViewEquipmentPhoto != null && item.ViewEquipmentPhoto != "")
                                {
                                    strviewEquimentdata = "http://jntuhaac.in/Content/Upload/EquipmentsPhotos/" + item.ViewEquipmentPhoto.Trim();
                                    // strviewEquimentdata = "/Content/Upload/EquipmentsPhotos/" + item.ViewEquipmentPhoto.Trim();
                                }
                                else
                                {
                                    strviewEquimentdata = "http://jntuhaac.in/Content/Images/no-photo.gif";
                                    // strviewEquimentdata = "~/Content/Images/no-photo.gif";
                                }

                                string path = strviewEquimentdata.Replace("%20", " ");
                                // string path = @"~" + strviewEquimentdata.Replace("%20", " ");
                                // path = System.Web.HttpContext.Current.Server.MapPath(path);

                                if (item.DelivaryChalanaDate != null)
                                    chalanaDate = UAAAS.Models.Utilities.MMDDYY2DDMMYY(item.DelivaryChalanaDate.ToString());
                                else
                                    chalanaDate = "";

                                if (item.EquipmentDateOfPurchasing != null)
                                    Equipmentdate = UAAAS.Models.Utilities.MMDDYY2DDMMYY(item.EquipmentDateOfPurchasing.ToString());
                                else
                                    Equipmentdate = "";

                                strLaboratoriesDetails += "<tr>";
                                strLaboratoriesDetails += "<td  align='center' colspan='1'><p align='center'>" + sno + "</p></td>";

                                if (item.AvailableArea != null)
                                    strLaboratoriesDetails += "<td  align='left' colspan='2'>" + item.AvailableArea.ToString().Replace("&", "&amp;") + "</td>";
                                else
                                    strLaboratoriesDetails += "<td  align='left' colspan='2'>" + item.AvailableArea + "</td>";

                                if (item.RoomNumber != null)
                                    strLaboratoriesDetails += "<td  align='left' colspan='3'>" + item.RoomNumber.Replace("&", "&amp;") + "</td>";
                                else
                                    strLaboratoriesDetails += "<td  align='left' colspan='3'>" + item.RoomNumber + "</td>";

                                if (item.EquipmentName != null)
                                    strLaboratoriesDetails += "<td  align='left' colspan='11'>" + item.EquipmentName.Replace("&", "&amp;") + "</td>";
                                else
                                    strLaboratoriesDetails += "<td  align='left' colspan='11'>" + item.EquipmentName + "</td>";

                                if (item.Make != null)
                                    strLaboratoriesDetails += "<td  align='left' colspan='4'>" + item.Make.Replace("&", "&amp;") + "</td>";
                                else
                                    strLaboratoriesDetails += "<td  align='left' colspan='4'>" + item.Make + "</td>";

                                if (item.Model != null)
                                    strLaboratoriesDetails += "<td  align='left' colspan='3'>" + item.Model.Replace("&", "&amp;") + "</td>";
                                else
                                    strLaboratoriesDetails += "<td  align='left' colspan='3'>" + item.Model + "</td>";

                                if (item.EquipmentUniqueID != null)
                                    strLaboratoriesDetails += "<td  align='left' colspan='4'>" + item.EquipmentUniqueID.Replace("&", "&amp;") + "</td>";
                                else
                                    strLaboratoriesDetails += "<td  align='left' colspan='4'>" + item.EquipmentUniqueID + "</td>";

                                #region With-Out Html Parsing

                                //if (System.IO.File.Exists(path))
                                //{
                                //    //strLaboratoriesDetails += "<td  width='100' align='center' colspan='4'><p align='center'><img src=" + serverURL + "" + strviewEquimentdata + " align='center'  width='40' height='50' /></p></td>";
                                //    //strLaboratoriesDetails += "<td  width='100' align='center' colspan='3'><p align='center'><img src='" + HtmlEncoder.Encode(path).Replace("'", "&#39;") + "' align='center'  width='40' height='50' /></p></td>";
                                //    strLaboratoriesDetails += "<td  width='100' align='center' colspan='3'><p align='center'><img src='" + HtmlEncoder.Encode(path.Trim()).Replace("'", "&#39;") + "' align='center'  width='40' height='50' /></p></td>";
                                //    //strLaboratoriesDetails += "<td  width='100' align='center' colspan='3'><p align='center'>"+ HtmlEncoder.Encode(path.Trim()).Replace("'", "&#39;") + "</p></td>";

                                //}
                                //else
                                //{
                                //    strLaboratoriesDetails += "<td width='100' align='center' colspan='3'><p align='center'></p></td>";
                                //}

                                #endregion

                                #region With-Out Html Parsing
                                try
                                {
                                    if (!string.IsNullOrEmpty(path))
                                    // if (System.IO.File.Exists(path))
                                    {
                                        //server 3

                                        if (item.ViewEquipmentPhoto == "380-52436-20170308-141942183287-f.jpg")
                                        {

                                        }

                                        strLaboratoriesDetails1 += "<p align='center'><img src='" + HtmlEncoder.Encode(path.Trim()).Replace("'", "&#39;") + "' align='center'  width='40' height='40' /></p>";
                                        var ParseEliments = HTMLWorker.ParseToList(new StringReader(strLaboratoriesDetails1), null);

                                        if (path.Contains("."))
                                            strLaboratoriesDetails += "<td  width='100' align='center' colspan='3'><p align='center'><img src='" + HtmlEncoder.Encode(path.Trim()).Replace("'", "&#39;") + "' align='center'  width='40' height='40' /></p></td>";
                                        else
                                            strLaboratoriesDetails += "<td width='100' align='center' colspan='3'><p align='center'>--------</p></td>";


                                        //server 1
                                        // strLaboratoriesDetails += "<td  width='100' align='center' colspan='3'><p align='center'><img src=" + serverURL + "" + strviewEquimentdata + " align='center'  width='40' height='50' /></p></td>";



                                    }
                                    else
                                    {
                                        strLaboratoriesDetails += "<td align='center' colspan='3'><p align='center'>--</p></td>";
                                    }

                                }
                                catch (Exception ex)
                                {

                                    strLaboratoriesDetails +=
                                        "<td width='100' align='center' colspan='3'><p align='center'>--</p></td>";
                                    strLaboratoriesDetails += "<td  align='left' colspan='3'>" + Equipmentdate + "</td>";
                                    strLaboratoriesDetails += "<td  align='left' colspan='3'>" + chalanaDate + "</td>";
                                    strLaboratoriesDetails += "<td  align='right' colspan='1' style='width:10%;' >" +
                                                              item.AvailableUnits + "</td>";
                                    strLaboratoriesDetails += "<td  align='left' colspan='1'></td>";
                                    strLaboratoriesDetails += "</tr>";
                                    sno++;
                                    LabsCount++;
                                    Equipmentdate = chalanaDate = "";
                                    continue;
                                }
                                #endregion

                                #region Html PArsing

                                //strLaboratoriesDetails1 = "";
                                //strLaboratoriesDetails1 = strLaboratoriesDetails;
                                ////strLaboratoriesDetails1 += "<td  width='100' align='center' colspan='3'><p align='center'><img src='" + HtmlEncoder.Encode(path.Trim()).Replace("'", "&#39;") + "' align='center'  width='40' height='50' /></p></td>";
                                //try
                                //{
                                //    // var ParseEliments = HTMLWorker.ParseToList(new StringReader(strLaboratoriesDetails1), null);
                                //    if (!string.IsNullOrEmpty(path))
                                //    // if (System.IO.File.Exists(path))
                                //    {
                                //        //int count = 0;
                                //        //string specialChar = @"!#$%&()=?»«@£§€{};':<>,";
                                //        //foreach (var item1 in specialChar)
                                //        //{
                                //        //    if (path.Contains(item1))
                                //        //    {
                                //        //        count++;

                                //        //    }

                                //        //}
                                //        //if (count == 0)
                                //        //{
                                //        //    // strLaboratoriesDetails += "<td  width='100' align='center' colspan='3'><p align='center'><img src='" + HtmlEncoder.Encode(path.Trim()).Replace("'", "&#39;") + "' align='center'  width='40' height='50' /></p></td>";
                                //        //    strLaboratoriesDetails += "<td  width='100' align='center' colspan='3'><p align='center'><img src='" + HtmlEncoder.Encode(path.Trim()).Replace("'", "&#39;") + "' align='center'  width='40' height='50' /></p></td>";
                                //        //}
                                //        //else
                                //        //{
                                //        //    strLaboratoriesDetails += "<td width='100' align='center' colspan='3'><p align='center'>--------</p></td>";
                                //        //}
                                //        //strLaboratoriesDetails += "<td  align='center' colspan='3'><p align='center'><img src='" + strviewEquimentdata + "' align='center'  width='40' height='40' /></p></td>";
                                //         strLaboratoriesDetails += "<td  width='100' align='center' colspan='4'><p align='center'><img src=" + serverURL + "" + strviewEquimentdata + " align='center'  width='40' height='50' /></p></td>";
                                //        //strLaboratoriesDetails += "<td  width='100' align='center' colspan='3'><p align='center'><img src='" + HtmlEncoder.Encode(path).Replace("'", "&#39;") + "' align='center'  width='40' height='50' /></p></td>";

                                //        //strLaboratoriesDetails += "<td  width='100' align='center' colspan='3'><p align='center'>"+ HtmlEncoder.Encode(path.Trim()).Replace("'", "&#39;") + "</p></td>";

                                //    }
                                //    else
                                //    {
                                //        strLaboratoriesDetails += "<td align='center' colspan='3'><p align='center'>--</p></td>";
                                //    }
                                //}
                                //catch
                                //{
                                //    strLaboratoriesDetails += "<td  align='center' colspan='3'><p align='center'>--</p></td>";
                                //    strLaboratoriesDetails += "<td  align='left' colspan='3'>" + Equipmentdate + "</td>";
                                //    strLaboratoriesDetails += "<td  align='left' colspan='3'>" + chalanaDate + "</td>";
                                //    strLaboratoriesDetails += "<td  align='center' colspan='1'>" + item.AvailableUnits + "</td>";
                                //    strLaboratoriesDetails += "<td  align='left' colspan='1'></td>";
                                //    strLaboratoriesDetails += "</tr>";
                                //    sno++;
                                //    Equipmentdate = chalanaDate = "";
                                //    continue;
                                //}

                                #endregion

                                strLaboratoriesDetails += "<td  align='left' colspan='3'>" + Equipmentdate + "</td>";
                                strLaboratoriesDetails += "<td  align='left' colspan='3'>" + chalanaDate + "</td>";
                                strLaboratoriesDetails += "<td  align='center' colspan='1'>" + item.AvailableUnits + "</td>";
                                strLaboratoriesDetails += "<td  align='left' colspan='1'></td>";
                                strLaboratoriesDetails += "</tr>";
                                sno++;
                                LabsCount++;
                                Equipmentdate = chalanaDate = "";
                            }
                            catch (Exception x)
                            {
                                throw x;
                            }
                        }
                    }
                    strLaboratoriesDetails += "</tbody></table><br/>";
                }
                // strLaboratoriesDetails += "<table><tr><td>Total Labs Count</td><td>" + LabsCount + "</td></tr></table>";
                //  strLaboratoriesDetails += GetFirstYearLabDetails(collegeId, contents);
                strLaboratoriesDetails += GetNonCircuteLabSummarySheet(labs, collegeId);
                strLaboratoriesDetails += "<br />";
                contents = contents.Replace("##LaboratoriesDetails##", strLaboratoriesDetails);
                //List<IElement> parsedHtmlElements = HTMLWorker.ParseToList(new StringReader(contents), null);
                return contents;
            }
        }

        public string NOofPhysicalLabs(int collegeId, string contents)
        {
            string available = string.Empty;
            string strExperimentaldetails = string.Empty;
            List<UAAAS.Controllers.LabsController.physicalLab> ObjPhysicalLab = new List<UAAAS.Controllers.LabsController.physicalLab>();
            string CollegeAffiliationStatus = db.jntuh_college_affiliation.Where(u => u.collegeId == collegeId && u.affiliationTypeId == 7).Select(u => u.affiliationStatus).FirstOrDefault();
            var jntuh_physical_lab = db.jntuh_physical_labmaster.Where(k => k.Collegeid == collegeId).Select(k => new { Id = k.Id, LabCode = k.Labcode, deptId = k.DepartmentId, NumberofLabs = k.Numberofavilablelabs }).ToList();
            if (CollegeAffiliationStatus == "Yes")
            {
                ObjPhysicalLab = (from lab in db.jntuh_lab_master
                                  join deg in db.jntuh_degree on lab.DegreeID equals deg.id
                                  join dep in db.jntuh_department on lab.DepartmentID equals dep.id
                                  join spec in db.jntuh_specialization on lab.SpecializationID equals spec.id
                                  where lab.CollegeId == collegeId && deg.id == 4
                                  select new UAAAS.Controllers.LabsController.physicalLab
                                  {
                                      Labid = lab.id,
                                      collegeId = (int)lab.CollegeId,
                                      degreeid = lab.DegreeID,
                                      departmentid = lab.DepartmentID,
                                      specializationid = lab.SpecializationID,
                                      degree = deg.degree,
                                      specialization = spec.specializationName,
                                      department = dep.departmentName,
                                      year = lab.Year,
                                      semister = lab.Semester,
                                      Labname = lab.LabName,
                                      LabCode = lab.Labcode.Trim().ToUpper()
                                  }).ToList();
            }
            else
            {
                var actualYear = db.jntuh_academic_year.Where(s => s.isActive == true && s.isPresentAcademicYear == true).Select(s => s.actualYear).FirstOrDefault();
                var PrevoiusYearId = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear)).Select(s => s.id).FirstOrDefault();
                var PresentYearId = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
                IQueryable<jntuh_specialization> jntuh_specialization = db.jntuh_specialization.Select(s => s);
                int[] specializationIDs = db.jntuh_college_intake_existing.Where(e => e.collegeId == collegeId && e.academicYearId == PrevoiusYearId).Select(e => e.specializationId).ToArray();
                int[] zeroProposedSpecializationIDs = db.jntuh_college_intake_existing.Where(e => e.collegeId == collegeId && e.academicYearId == PrevoiusYearId && e.proposedIntake == 0).Select(e => e.specializationId).ToArray();
                var DepartmentsData = jntuh_specialization.Where(e => e.isActive == true && specializationIDs.Contains(e.id) && !zeroProposedSpecializationIDs.Contains(e.id)).Select(e => e.departmentId).Distinct().ToArray();
                var DegreeIds = db.jntuh_department.Where(e => e.isActive == true && DepartmentsData.Contains(e.id)).Select(e => e.degreeId).Distinct().ToArray();

                //UAAAS.Controllers.LabsController
                if (DegreeIds.Contains(4))
                {
                    if (specializationIDs.Contains(134))
                    {
                        ObjPhysicalLab = (from lab in db.jntuh_lab_master
                                          join deg in db.jntuh_degree on lab.DegreeID equals deg.id
                                          join dep in db.jntuh_department on lab.DepartmentID equals dep.id
                                          join spec in jntuh_specialization on lab.SpecializationID equals spec.id
                                          where lab.CollegeId == null && deg.id == 4 && (DepartmentsData.Contains(lab.DepartmentID) || lab.DepartmentID == 34)
                                          select new UAAAS.Controllers.LabsController.physicalLab
                                          {
                                              Labid = lab.id,
                                              collegeId = collegeId,
                                              degreeid = lab.DegreeID,
                                              departmentid = lab.DepartmentID,
                                              specializationid = lab.SpecializationID,
                                              degree = deg.degree,
                                              specialization = spec.specializationName,
                                              department = dep.departmentName,
                                              year = lab.Year,
                                              semister = lab.Semester,
                                              Labname = lab.LabName,
                                              LabCode = lab.Labcode.Trim().ToUpper()
                                          }).ToList();
                    }
                    else
                    {
                        try
                        {
                            ObjPhysicalLab = (from lab in db.jntuh_lab_master
                                              join deg in db.jntuh_degree on lab.DegreeID equals deg.id
                                              join dep in db.jntuh_department on lab.DepartmentID equals dep.id
                                              join spec in jntuh_specialization on lab.SpecializationID equals spec.id
                                              where lab.CollegeId == null && deg.id == 4 && (DepartmentsData.Contains(lab.DepartmentID) || lab.DepartmentID == 34) && lab.SpecializationID != 134
                                              select new UAAAS.Controllers.LabsController.physicalLab
                                              {
                                                  Labid = lab.id,
                                                  collegeId = collegeId,
                                                  degreeid = lab.DegreeID,
                                                  departmentid = lab.DepartmentID,
                                                  specializationid = lab.SpecializationID,
                                                  degree = deg.degree,
                                                  specialization = spec.specializationName,
                                                  department = dep.departmentName,
                                                  year = lab.Year,
                                                  semister = lab.Semester,
                                                  Labname = lab.LabName,
                                                  LabCode = lab.Labcode.Trim().ToUpper()
                                              }).ToList();
                        }
                        catch (DbEntityValidationException dbEx)
                        {
                            foreach (var validationErrors in dbEx.EntityValidationErrors)
                            {
                                foreach (var validationError in validationErrors.ValidationErrors)
                                {
                                    Trace.TraceInformation("Property:{0} Error :{1}",
                                        validationError.PropertyName,
                                        validationError.ErrorMessage);
                                }
                            }
                        }
                    }
                }
                else
                {
                    try
                    {
                        ObjPhysicalLab = (from lab in db.jntuh_lab_master
                                          join deg in db.jntuh_degree on lab.DegreeID equals deg.id
                                          join dep in db.jntuh_department on lab.DepartmentID equals dep.id
                                          join spec in jntuh_specialization on lab.SpecializationID equals spec.id
                                          where lab.CollegeId == null && deg.id == 4 && DepartmentsData.Contains(lab.DepartmentID)
                                          select new UAAAS.Controllers.LabsController.physicalLab
                                          {
                                              Labid = lab.id,
                                              collegeId = collegeId,
                                              degreeid = lab.DegreeID,
                                              departmentid = lab.DepartmentID,
                                              specializationid = lab.SpecializationID,
                                              degree = deg.degree,
                                              specialization = spec.specializationName,
                                              department = dep.departmentName,
                                              year = lab.Year,
                                              semister = lab.Semester,
                                              Labname = lab.LabName,
                                              LabCode = lab.Labcode.Trim().ToUpper()
                                          }).ToList();
                    }
                    catch (DbEntityValidationException dbEx)
                    {
                        foreach (var validationErrors in dbEx.EntityValidationErrors)
                        {
                            foreach (var validationError in validationErrors.ValidationErrors)
                            {
                                Trace.TraceInformation("Property:{0} Error :{1}",
                                    validationError.PropertyName,
                                    validationError.ErrorMessage);
                            }
                        }
                    }
                }

                if (CollegeAffiliationStatus != "Yes" && !specializationIDs.Contains(33) && !specializationIDs.Contains(43))
                    ObjPhysicalLab = ObjPhysicalLab.Where(x => x.LabCode != "PH105BS").ToList();
            }



            ObjPhysicalLab = ObjPhysicalLab.GroupBy(e => new { e.LabCode, e.departmentid }).Select(e => new UAAAS.Controllers.LabsController.physicalLab
            {
                Labid = e.FirstOrDefault().Labid,
                collegeId = e.FirstOrDefault().collegeId,
                degreeid = e.FirstOrDefault().degreeid,
                departmentid = e.FirstOrDefault().departmentid,
                specializationid = e.FirstOrDefault().specializationid,
                degree = e.FirstOrDefault().degree,
                specialization = e.FirstOrDefault().specialization,
                department = e.FirstOrDefault().department,
                year = e.FirstOrDefault().year,
                semister = e.FirstOrDefault().semister,
                Labname = e.FirstOrDefault().Labname,
                LabCode = e.FirstOrDefault().LabCode,
                physicalId = jntuh_physical_lab.Where(d => d.LabCode == e.Key.LabCode && d.deptId == e.Key.departmentid).Select(d => d.NumberofLabs).FirstOrDefault() ?? 0

            }).ToList();

            var DeptIds = ObjPhysicalLab.Select(e => e.departmentid).Distinct().ToArray();

            var Departments = db.jntuh_department.Where(it => DeptIds.Contains(it.id)).Select(s => new
            {
                DeptId = s.id,
                department = s.departmentName,
                degree = s.jntuh_degree.degree

            }).OrderBy(e => e.DeptId).ToList();

            foreach (var deptid in DeptIds)
            {
                string strDepartmentName = string.Empty;
                var departmentDetails = Departments.FirstOrDefault(s => s.DeptId == deptid);

                strDepartmentName = departmentDetails.degree + " - " + departmentDetails.department;

                strExperimentaldetails += "<strong><u>" + strDepartmentName + "</u></strong> <br /> <br />";
                strExperimentaldetails += "<table border='1' style='font-size: 9px;' cellspacing='0' cellpadding='3'>";
                strExperimentaldetails += "<thead>";
                strExperimentaldetails += "<tr>";
                strExperimentaldetails += "<td width='7%'><p align='center'>S.No</p></td>";
                strExperimentaldetails += "<td width='8%'><p align='center'>Year</p></td>";
                strExperimentaldetails += "<td width='10%'><p align='center'>Semister</p></td>";
                strExperimentaldetails += "<td width='15%'><p align='center'>Lab Code</p></td>";
                strExperimentaldetails += "<td width='50%'><p align='center'>Lab Name</p></td>";
                strExperimentaldetails += "<td width='10%'><p align='center'>Commitee Findings</p></td>";
                strExperimentaldetails += "</tr>";
                strExperimentaldetails += "</thead>";
                strExperimentaldetails += "<tbody>";
                int sno = 1;
                foreach (var data in ObjPhysicalLab.Where(e => e.departmentid == deptid).OrderBy(e => e.year).ThenBy(e => e.semister).ToList())
                {
                    strExperimentaldetails += "<tr>";
                    strExperimentaldetails += "<td width='7%'  align='center' ><p align='center'>" + sno + "</p></td>";
                    strExperimentaldetails += "<td  width='8%' align='left'>" + data.year + "</td>";
                    strExperimentaldetails += "<td width='10%' align='left'>" + data.semister + "</td>";
                    strExperimentaldetails += "<td width='15%' align='left'>" + data.LabCode + "</td>";
                    strExperimentaldetails += "<td width='50%' align='left'>" + data.Labname + "</td>";
                    strExperimentaldetails += "<td width='10%' align='left'>" + "" + "</td>";
                    strExperimentaldetails += "</tr>";
                    sno++;
                }
                strExperimentaldetails += "</tbody>";
                strExperimentaldetails += "</table>";
            }
            contents = contents.Replace("##NoofPhysicalLabs##", strExperimentaldetails);
            //List<IElement> parsedHtmlElements = HTMLWorker.ParseToList(new StringReader(contents), null);
            return contents;
        }

        private string GetNonCircuteLabSummarySheet(List<Lab> labdata, int collegeId)
        {
            using (var db = new uaaasDBContext())
            {
                var actualYear = db.jntuh_academic_year.Where(s => s.isActive == true && s.isPresentAcademicYear == true).Select(s => s.actualYear).FirstOrDefault();
                var PrevoiusYearId = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear)).Select(s => s.id).FirstOrDefault();
                //var PresentYearId = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();

                int[] collegeZeroProposedSpecializationIDs = db.jntuh_college_intake_existing.Where(e => e.collegeId == collegeId && e.academicYearId == PrevoiusYearId && e.proposedIntake == 0).Select(e => e.specializationId).Distinct().ToArray();
                int[] Equipmentsids = db.jntuh_college_laboratories.Where(C => C.CollegeID == collegeId).Select(C => C.EquipmentID).ToArray();
                int[] DegreeIds = db.jntuh_lab_master.Where(L => Equipmentsids.Contains(L.id)).Select(L => L.DegreeID).Distinct().ToArray();

                labdata = labdata.ToList();

                var summarySheet = labdata.Where(i => !collegeZeroProposedSpecializationIDs.Contains(i.specializationId)).GroupBy(h => new { h.department, h.year, h.Semester, h.LabName, h.degree, h.specializationName, h.specializationId })
                    .Select(s => new
                    {
                        degree = s.Key.degree,
                        specialization = s.Key.specializationName,
                        department = s.Key.department,
                        year = s.Key.year,
                        Semester = s.Key.Semester,
                        LabName = s.Key.LabName,
                        SpecId = s.Key.specializationId,
                        Count = s.Count(g => g.tempEquipmentId != null)
                    });
                string strSummarySheet = string.Empty;
                strSummarySheet += "<br /><strong><p><u>14 a).Lab Summary Sheet</u>:</p></strong> <br />";



                if (summarySheet.Count() > 0)
                {
                    //strSummarySheet += "<table border='1' cellspacing='0' cellpadding='5' style='font-size: 9px;'>";
                    //strSummarySheet += "<tbody>";
                    //strSummarySheet += "<tr>";
                    //strSummarySheet += "<td colspan='1' rowspan='2' ><p align='center'>S.No</p></td>";
                    ////strSummarySheet += "<td colspan='1' rowspan='2' ><p align='left'>Degree</p></td>";
                    ////strSummarySheet += "<td colspan='2' rowspan='2' ><p align='left'>Department</p></td>";
                    ////strSummarySheet += "<td colspan='2' rowspan='2' ><p align='left'>Specialization</p></td>";
                    //strSummarySheet += "<td colspan='1' rowspan='2' ><p align='left'>Year</p></td>";
                    //strSummarySheet += "<td colspan='1' rowspan='2' ><p align='left'>Sem</p></td>";
                    //strSummarySheet += "<td colspan='4' rowspan='2' ><p align='left'>Lab Name</p></td>";
                    //strSummarySheet += "<td colspan='2' ><p align='left'>No. of Equipment</p></td>";
                    //strSummarySheet += "</tr>";
                    //strSummarySheet += "<tr>";
                    //strSummarySheet += "<td colspan='1' ><p align='left'>Upload</p></td>";
                    //strSummarySheet += "<td colspan='1' ><p align='left'>CF</p></td>";
                    //strSummarySheet += "</tr>";

                    //int countdata = 1;
                    //foreach (var item in summarySheet.OrderBy(e => e.department).ThenBy(e => e.degree).ThenBy(e => e.year).ThenBy(e => e.Semester))
                    //{
                    //    strSummarySheet += "<tr>";
                    //    strSummarySheet += "<td colspan='1' ><p align='center'>" + countdata + "</p></td>";
                    //    //strSummarySheet += "<td colspan='1' ><p align='center'>" + item.degree + "</p></td>";
                    //    //strSummarySheet += "<td colspan='2' ><p align='left'>" + item.department + "</p></td>";
                    //    //strSummarySheet += "<td colspan='2' ><p align='left'>" + item.specialization + "</p></td>";
                    //    strSummarySheet += "<td colspan='1' ><p align='left'>" + item.year + "</p></td>";
                    //    strSummarySheet += "<td colspan='1' ><p align='left'>" + item.Semester + "</p></td>";
                    //    strSummarySheet += "<td colspan='4' ><p align='left'>" + item.LabName + "</p></td>";
                    //    strSummarySheet += "<td colspan='1' ><p align='left'>" + item.Count + "</p></td>";
                    //    strSummarySheet += "<td colspan='1' ><p align='left'>&nbsp;</p></td>";
                    //    strSummarySheet += "</tr>";
                    //    countdata++;

                    //}
                    //strSummarySheet += "<table border='1' cellspacing='0' cellpadding='5' style='font-size: 9px;'>";
                    //strSummarySheet += "<tbody>";
                    //strSummarySheet += "<tr>";
                    //strSummarySheet += "<td colspan='1' rowspan='2' ><p align='center'>S.No</p></td>";
                    //strSummarySheet += "<td colspan='1' rowspan='2' ><p align='left'>Year</p></td>";
                    //strSummarySheet += "<td colspan='1' rowspan='2' ><p align='left'>Sem</p></td>";
                    //strSummarySheet += "<td colspan='4' rowspan='2' ><p align='left'>Lab Name</p></td>";
                    //strSummarySheet += "<td colspan='2' ><p align='left'>No. of Equipment</p></td>";
                    //strSummarySheet += "</tr>";
                    //strSummarySheet += "<tr>";
                    //strSummarySheet += "<td colspan='1' ><p align='left'>Upload</p></td>";
                    //strSummarySheet += "<td colspan='1' ><p align='left'>CF</p></td>";
                    //strSummarySheet += "</tr>";


                    foreach (var item in summarySheet.GroupBy(e => e.SpecId).ToList())
                    {
                        var labRecord = summarySheet.Where(i => i.SpecId == item.Key).ToList();
                        var degreeName = labRecord.FirstOrDefault().degree + " - " + labRecord.FirstOrDefault().specialization;

                        strSummarySheet += "<strong><u>" + degreeName + "</u></strong> <br /> <br />";
                        strSummarySheet += "<table border='1' style='font-size: 9px;' cellspacing='0' cellpadding='3'>";
                        strSummarySheet += "<thead>";
                        strSummarySheet += "<tr>";
                        strSummarySheet += "<td colspan='1' rowspan='2' ><p align='center'>S.No</p></td>";
                        strSummarySheet += "<td colspan='1' rowspan='2' ><p align='left'>Year</p></td>";
                        strSummarySheet += "<td colspan='1' rowspan='2' ><p align='left'>Sem</p></td>";
                        strSummarySheet += "<td colspan='4' rowspan='2' ><p align='left'>Lab Name</p></td>";
                        strSummarySheet += "<td colspan='2' ><p align='left'>No. of Equipment</p></td>";
                        strSummarySheet += "</tr>";
                        strSummarySheet += "<tr>";
                        strSummarySheet += "<td colspan='1' ><p align='center'>Required</p></td>";
                        strSummarySheet += "<td colspan='1' ><p align='center'>Available</p></td>";
                        strSummarySheet += "</tr>";
                        strSummarySheet += "</thead>";
                        strSummarySheet += "<tbody>";
                        int countdata = 1;
                        //strSummarySheet += "<thead>";
                        //strSummarySheet += "<tr>";
                        //strSummarySheet += "<th colspan='11'> <strong>" + degreeName + "</strong></th>";
                        //strSummarySheet += "</tr>";
                        //strSummarySheet += "</thead>";
                        foreach (var labsheetsrecords in labRecord)
                        {
                            strSummarySheet += "<tr>";
                            strSummarySheet += "<td colspan='1' ><p align='center'>" + countdata + "</p></td>";
                            //strSummarySheet += "<td colspan='1' ><p align='center'>" + item.degree + "</p></td>";
                            //strSummarySheet += "<td colspan='2' ><p align='left'>" + item.department + "</p></td>";
                            //strSummarySheet += "<td colspan='2' ><p align='left'>" + item.specialization + "</p></td>";
                            strSummarySheet += "<td colspan='1' ><p align='left'>" + labsheetsrecords.year + "</p></td>";
                            strSummarySheet += "<td colspan='1' ><p align='left'>" + labsheetsrecords.Semester + "</p></td>";
                            strSummarySheet += "<td colspan='4' ><p align='left'>" + labsheetsrecords.LabName + "</p></td>";
                            strSummarySheet += "<td colspan='1' ><p align='Center'>" + labsheetsrecords.Count + "</p></td>";
                            strSummarySheet += "<td colspan='1' ><p align='left'>&nbsp;</p></td>";
                            strSummarySheet += "</tr>";
                            countdata++;
                        }
                        strSummarySheet += "</tbody>";
                        strSummarySheet += "</table>";
                    }

                }
                else
                {
                    strSummarySheet += "<p style='font-size: 9px;'><i>Non-Circuit labs for B.Tech / B.Pharmacy not available.</i></p>";
                }

                return strSummarySheet;
            }
        }

        public string LibraryInformation(int collegeId, string contents)
        {
            using (var db = new uaaasDBContext())
            {
                string strLibraryInformation = string.Empty;

                #region Librarian Information
                jntuh_college_library library = db.jntuh_college_library.Where(l => l.collegeId == collegeId).Select(l => l).FirstOrDefault();

                if (library != null)
                {
                    //strLibraryInformation += "<p><strong><u>15. Library Information</u> :</strong></p><br />";
                    strLibraryInformation += "<table border='0' cellpadding='3' cellspacing='0' id='page151' style='font-size: 9px;'>";
                    strLibraryInformation += "<tr><td colspan='1'>a)</td><td colspan='7'>Name of the Librarian </td><td colspan='1'>:</td><td colspan='7'>" + library.librarianName + "</td></tr>";
                    strLibraryInformation += "<tr><td colspan='1'>b)</td><td colspan='7'>Qualifications of the Librarian </td><td colspan='1'>:</td><td colspan='7'>" + library.librarianQualifications + "</td></tr>";
                    strLibraryInformation += "<tr><td colspan='1'>c)</td><td colspan='7'>Library phone number (Landline/Mobile) </td><td colspan='1'>:</td><td colspan='7'>" + library.libraryPhoneNumber + "</td></tr>";
                    strLibraryInformation += "<tr><td colspan='1'>d)</td><td colspan='7'>Number of Supporting Staff </td><td colspan='1'>:</td><td colspan='7'>" + library.totalSupportingStaff + "</td></tr>";
                    strLibraryInformation += "<tr><td colspan='1'>e)</td><td colspan='7'>Total Number of Titles </td><td colspan='1'>:</td><td colspan='7'>" + library.totalTitles + "</td></tr>";
                    strLibraryInformation += "<tr><td colspan='1'>f)</td><td colspan='7'>Total Number of Volumes </td><td colspan='1'>:</td><td colspan='7'>" + library.totalVolumes + "</td></tr>";
                    strLibraryInformation += "<tr><td colspan='1'>g)</td><td colspan='7'>Total Number of National Journals </td><td colspan='1'>:</td><td colspan='7'>" + library.totalNationalJournals + "</td></tr>";
                    strLibraryInformation += "<tr><td colspan='1'>h)</td><td colspan='7'>Total Number of International National Journals </td><td colspan='1'>:</td><td colspan='7'>" + library.totalInternationalJournals + "</td></tr>";
                    strLibraryInformation += "<tr><td colspan='1'>i)</td><td colspan='7'>No. of E-journals </td><td colspan='1'>:</td><td colspan='7'>" + library.totalEJournals + "</td></tr>";
                    strLibraryInformation += "<tr><td colspan='1'>j)</td><td colspan='7'>Seating Capacity of Library </td><td colspan='1'>:</td><td colspan='7'>" + library.librarySeatingCapacity + "</td></tr>";
                    strLibraryInformation += "<tr><td colspan='1'>k)</td><td colspan='7'>Working Hours of library </td><td colspan='1'>:</td><td colspan='7'>" + library.libraryWorkingHoursFrom + " to " + library.libraryWorkingHoursTo + "</td></tr>";
                    strLibraryInformation += "</table>";
                }
                else
                {
                    strLibraryInformation += "<table border='0' cellpadding='3' cellspacing='0' id='page151' style='font-size: 9px;'>";
                    strLibraryInformation += "<tr><td colspan='1'>a)</td><td colspan='7'>Name of the Librarian </td><td colspan='1'>:</td><td colspan='7'></td></tr>";
                    strLibraryInformation += "<tr><td colspan='1'>b)</td><td colspan='7'>Qualifications of the Librarian </td><td colspan='1'>:</td><td colspan='7'></td></tr>";
                    strLibraryInformation += "<tr><td colspan='1'>c)</td><td colspan='7'>Library phone number (Landline/Mobile) </td><td colspan='1'>:</td><td colspan='7'></td></tr>";
                    strLibraryInformation += "<tr><td colspan='1'>d)</td><td colspan='7'>Number of Supporting Staff </td><td colspan='1'>:</td><td colspan='7'></td></tr>";
                    strLibraryInformation += "<tr><td colspan='1'>e)</td><td colspan='7'>Total Number of Titles </td><td colspan='1'>:</td><td colspan='7'></td></tr>";
                    strLibraryInformation += "<tr><td colspan='1'>f)</td><td colspan='7'>Total Number of Volumes </td><td colspan='1'>:</td><td colspan='7'></td></tr>";
                    strLibraryInformation += "<tr><td colspan='1'>g)</td><td colspan='7'>Total Number of National Journals </td><td colspan='1'>:</td><td colspan='7'></td></tr>";
                    strLibraryInformation += "<tr><td colspan='1'>h)</td><td colspan='7'>Total Number of International National Journals </td><td colspan='1'>:</td><td colspan='7'></td></tr>";
                    strLibraryInformation += "<tr><td colspan='1'>i)</td><td colspan='7'>No. of E-journals </td><td colspan='1'>:</td><td colspan='7'></td></tr>";
                    strLibraryInformation += "<tr><td colspan='1'>j)</td><td colspan='7'>Seating Capacity of Library </td><td colspan='1'>:</td><td colspan='7'></td></tr>";
                    strLibraryInformation += "<tr><td colspan='1'>k)</td><td colspan='7'>Working Hours of library </td><td colspan='1'>:</td><td colspan='7'></td></tr>";
                    strLibraryInformation += "</table>";
                }

                #endregion

                contents = contents.Replace("##LibraryInformation##", contents);
                //List<IElement> parsedHtmlElements = HTMLWorker.ParseToList(new StringReader(contents), null);
                return contents;
            }
        }

        public string LibraryBooks(int collegeId, string contents)
        {
            using (var db = new uaaasDBContext())
            {
                string strLibraryBooks = string.Empty;
                strLibraryBooks += "<table border='1' cellspacing='0' cellpadding='3' style='font-size: 9px;'><tbody>";
                strLibraryBooks += "<tr>";
                strLibraryBooks += "<td colspan='3'><p>Degree</p></td>";
                strLibraryBooks += "<td colspan='2'><p align='center'>Titles</p></td>";
                strLibraryBooks += "<td colspan='3'><p align='center'>PAY-Titles</p></td>";
                strLibraryBooks += "<td colspan='2'><p align='center'>Volume</p></td>";
                strLibraryBooks += "<td colspan='3'><p align='center'>PAY-Volume</p></td>";
                strLibraryBooks += "<td colspan='3'><p align='center'>National Print Journals</p></td>";
                strLibraryBooks += "<td colspan='3'><p align='center'>PAY-National Print Journals</p></td>";
                strLibraryBooks += "<td colspan='4'><p align='center'>International Print Journals</p></td>";
                strLibraryBooks += "<td colspan='4'><p align='center'>PAY-International Print Journals</p></td>";
                strLibraryBooks += "<td colspan='3'><p align='center'>e-Journals</p></td>";
                strLibraryBooks += "<td colspan='3'><p align='center'>PAY-e-Journals</p></td>";
                strLibraryBooks += "<td colspan='4'><p align='center'>Valid Subscription Number of e-Journals</p></td>";
                strLibraryBooks += "</tr>";

                List<LibraryDetails> libraryDetails = (from collegeDegree in db.jntuh_college_degree
                                                       join degre in db.jntuh_degree on collegeDegree.degreeId equals degre.id
                                                       where (collegeDegree.collegeId == collegeId && collegeDegree.isActive == true)
                                                       orderby degre.degree
                                                       select new LibraryDetails
                                                       {
                                                           degreeId = collegeDegree.degreeId,
                                                           degree = degre.degree,
                                                           totalTitles = null,
                                                           totalVolumes = null,
                                                           totalNationalJournals = null,
                                                           totalInternationalJournals = null,
                                                           totalEJournals = null
                                                       }).ToList();
                var degreeids = libraryDetails.Select(it1 => it1.degreeId).ToList();
                var jntuh_college_library_details = db.jntuh_college_library_details.Where(it => it.collegeId == collegeId
                    && degreeids.Contains(it.degreeId)).Select(it => new
                    {
                        it.degreeId,
                        it.totalTitles,
                        it.totalVolumes,
                        it.totalNationalJournals,
                        it.totalInternationalJournals,
                        it.totalEJournals
                    }).ToList();
                if (libraryDetails != null)
                {
                    foreach (var item in libraryDetails)
                    {
                        item.totalTitles = jntuh_college_library_details.Where(collegeLibrary =>
                                                                                  collegeLibrary.degreeId == item.degreeId)
                                                                           .Select(collegeLibrary => collegeLibrary.totalTitles)
                                                                           .FirstOrDefault();
                        item.totalVolumes = jntuh_college_library_details.Where(collegeLibrary =>
                                                                                  collegeLibrary.degreeId == item.degreeId)
                                                                           .Select(collegeLibrary => collegeLibrary.totalVolumes)
                                                                           .FirstOrDefault();
                        item.totalNationalJournals = jntuh_college_library_details.Where(collegeLibrary =>
                                                                                  collegeLibrary.degreeId == item.degreeId)
                                                                           .Select(collegeLibrary => collegeLibrary.totalNationalJournals)
                                                                           .FirstOrDefault();
                        item.totalInternationalJournals = jntuh_college_library_details.Where(collegeLibrary =>
                                                                                  collegeLibrary.degreeId == item.degreeId)
                                                                           .Select(collegeLibrary => collegeLibrary.totalInternationalJournals)
                                                                           .FirstOrDefault();
                        item.totalEJournals = jntuh_college_library_details.Where(collegeLibrary =>
                                                                                  collegeLibrary.degreeId == item.degreeId)
                                                                           .Select(collegeLibrary => collegeLibrary.totalEJournals)
                                                                           .FirstOrDefault();
                        strLibraryBooks += "<tr>";
                        strLibraryBooks += "<td><p>" + item.degree + "</p></td>";
                        strLibraryBooks += "<td colspan='3' align='center'>" + item.totalTitles + "</td>";
                        strLibraryBooks += "<td colspan='2' align='center'>" + item.newTitles + "</td>";
                        strLibraryBooks += "<td colspan='3' align='center'>" + item.totalVolumes + "</td>";
                        strLibraryBooks += "<td colspan='2' align='center'>" + item.newVolumes + "</td>";
                        strLibraryBooks += "<td colspan='3' align='center'>" + item.totalNationalJournals + "</td>";
                        strLibraryBooks += "<td colspan='3' align='center'>" + item.newNationalJournals + "</td>";
                        strLibraryBooks += "<td colspan='4' align='center'>" + item.totalInternationalJournals + "</td>";
                        strLibraryBooks += "<td colspan='4' align='center'>" + item.totalInternationalJournals + "</td>";
                        strLibraryBooks += "<td colspan='3' align='center'>" + item.totalEJournals + "</td>";
                        strLibraryBooks += "<td colspan='3' align='center'>" + item.newEJournals + "</td>";
                        strLibraryBooks += "<td colspan='4' align='center'>" + item.EJournalsSubscriptionNumber + "</td>";
                        strLibraryBooks += "</tr>";
                    }
                }
                strLibraryBooks += "</tbody></table><br />";
                strLibraryBooks += "<b>PAY-</b> Present Academic Year";
                //strLibraryBooks += "T-Number of Titles PAYT-Number of Titles added in this Academic Year V-Number of Volume PAYV-Number of Volumes added in this Academic Year NJ-Number of National Print Journals PAYNJ-Number of National Print Journals added in this Academic Year IJ-Number of International Print Journals PAYIJ-Number of International Print Journals added in this Academic Year EJ-Number of e-Journals PAYEJ-Number of e-Journals added in this Academic Year VSNoofEJ-Valid Subscription Number of e-Journals";
                strLibraryBooks += "<br />";
                contents = contents.Replace("##LibraryBooks##", strLibraryBooks);
                // List<IElement> parsedHtmlElements = HTMLWorker.ParseToList(new StringReader(contents), null);
                return contents;
            }
        }

        public string Computers(int collegeId, string contents)
        {
            using (var db = new uaaasDBContext())
            {
                #region Computer Lab Facilities
                string strComputers = string.Empty;
                strComputers += "<p style='font-size: 9px;'><u><strong>Computer Lab Facilities</strong></u> :</p><br /><p>i) Printers availability :</p><p>ii) Working Hours of Computer Lab : From: _________ (HH:MM) To : _________ (HH:MM)</p><p>iii) Internet accessibility (timings) : From: _________ (HH:MM) To : _________ (HH:MM)</p><br />";
                #endregion

                #region /Computers – Students Ratio

                strComputers += "<table border='1' cellspacing='0' cellpadding='5' style='font-size: 9px;'><tbody>";
                //<td width='98' colspan='2'><p align='center'>Total Strength</p></td>
                strComputers += "<tr><td width='112' colspan='2'><p>Degree</p></td><td width='151' colspan='2'><p align='center'>Available Computers</p></td></tr>";
                List<jntuh_college_degree> jntuh_college_degree = db.jntuh_college_degree.Where(d => d.collegeId == collegeId && d.isActive == true).ToList();

                List<ComputerStudentRatioDetails> computerStudentDetails = new List<ComputerStudentRatioDetails>();
                if (jntuh_college_degree != null)
                {
                    var degreeids = jntuh_college_degree.Select(it1 => it1.degreeId).ToList();
                    var jntuh_college_computer_student_ratio = db.jntuh_college_computer_student_ratio.Where(it => it.collegeId == collegeId
                        && degreeids.Contains(it.degreeId)).Select(it => new
                        {
                            it.degreeId,
                            it.availableComputers
                        }).ToList();
                    var jntuh_degree = db.jntuh_degree.Where(it => it.isActive && degreeids.Contains(it.id))
                        .Select(it => new
                        {
                            it.id,
                            it.degree
                        }).ToList(); ;
                    foreach (var item in jntuh_college_degree)
                    {
                        int degreeId = item.degreeId;
                        string degree = jntuh_degree.Where(d => d.id == item.degreeId).Select(d => d.degree).FirstOrDefault();
                        // int totalIntake = GetIntake(item.degreeId, collegeId);
                        int availableComputers = jntuh_college_computer_student_ratio.Where(computerStudenRatio =>
                                                                                                computerStudenRatio.degreeId == item.degreeId)
                                                                                          .Select(computerStudenRatio => computerStudenRatio.availableComputers)
                                                                                          .FirstOrDefault();
                        //<td width='98' align='center' colspan='2'>" + totalIntake + "</td>
                        strComputers += "<tr><td width='112' colspan='2'><p>" + degree + "</p></td><td width='151' align='center' colspan='2'>" + availableComputers + "</td></tr>";
                    }
                }
                strComputers += "</tbody></table>";
                #endregion

                contents = contents.Replace("##Computers##", strComputers);
                //List<IElement> parsedHtmlElements = HTMLWorker.ParseToList(new StringReader(contents), null);
                return contents;
            }
        }

        public string LibraryDetails(int collegeId, string contents, List<int> degreeIds)
        {
            using (var db = new uaaasDBContext())
            {
                string strLibraryDetails = string.Empty;

                #region Librarian Information
                jntuh_college_library library = db.jntuh_college_library.Where(l => l.collegeId == collegeId).Select(l => l).FirstOrDefault();
                strLibraryDetails += "<table border='0' cellpadding='0' cellspacing='0' id='page152' style='font-size: 9px;'><tr>";
                strLibraryDetails += "<td align='left' valign='top'>";
                if (library != null)
                {
                    strLibraryDetails += "<p><strong><u>16. Library Information</u> :</strong></p><br />";
                    strLibraryDetails += "<table border='0' cellpadding='3' cellspacing='0' id='page153' style='font-size: 9px;'>";
                    strLibraryDetails += "<tr><td colspan='1'>a)</td><td colspan='7'>Name of the Librarian </td><td colspan='1'>:</td><td colspan='7'>" + library.librarianName + "</td></tr>";
                    strLibraryDetails += "<tr><td colspan='1'>b)</td><td colspan='7'>Qualifications of the Librarian </td><td colspan='1'>:</td><td colspan='7'>" + library.librarianQualifications + "</td></tr>";
                    strLibraryDetails += "<tr><td colspan='1'>c)</td><td colspan='7'>Library phone number (Landline/Mobile) </td><td colspan='1'>:</td><td colspan='7'>" + library.libraryPhoneNumber + "</td></tr>";
                    strLibraryDetails += "<tr><td colspan='1'>d)</td><td colspan='7'>Number of Supporting Staff </td><td colspan='1'>:</td><td colspan='7'>" + library.totalSupportingStaff + "</td></tr>";
                    strLibraryDetails += "<tr><td colspan='1'>e)</td><td colspan='7'>Total Number of Titles </td><td colspan='1'>:</td><td colspan='7'>" + library.totalTitles + "</td></tr>";
                    strLibraryDetails += "<tr><td colspan='1'>f)</td><td colspan='7'>Total Number of Volumes </td><td colspan='1'>:</td><td colspan='7'>" + library.totalVolumes + "</td></tr>";
                    strLibraryDetails += "<tr><td colspan='1'>g)</td><td colspan='7'>Total Number of National Journals </td><td colspan='1'>:</td><td colspan='7'>" + library.totalNationalJournals + "</td></tr>";
                    strLibraryDetails += "<tr><td colspan='1'>h)</td><td colspan='7'>Total Number of International National Journals </td><td colspan='1'>:</td><td colspan='7'>" + library.totalInternationalJournals + "</td></tr>";
                    strLibraryDetails += "<tr><td colspan='1'>i)</td><td colspan='7'>No. of E-journals </td><td colspan='1'>:</td><td colspan='7'>" + library.totalEJournals + "</td></tr>";
                    strLibraryDetails += "<tr><td colspan='1'>j)</td><td colspan='7'>Seating Capacity of Library </td><td colspan='1'>:</td><td colspan='7'>" + library.librarySeatingCapacity + "</td></tr>";
                    strLibraryDetails += "<tr><td colspan='1'>k)</td><td colspan='7'>Working Hours of library </td><td colspan='1'>:</td><td colspan='7'>" + library.libraryWorkingHoursFrom + " to " + library.libraryWorkingHoursTo + "</td></tr>";
                    strLibraryDetails += "</table><br />";
                }
                else
                {
                    strLibraryDetails += "<p><strong><u>16. Library Information</u> :</strong></p><br />";
                    strLibraryDetails += "<table border='0' cellpadding='3' cellspacing='0' id='page153' style='font-size: 9px;'>";
                    strLibraryDetails += "<tr><td colspan='1'>a)</td><td colspan='7'>Name of the Librarian </td><td colspan='1'>:</td><td colspan='7'></td></tr>";
                    strLibraryDetails += "<tr><td colspan='1'>b)</td><td colspan='7'>Qualifications of the Librarian </td><td colspan='1'>:</td><td colspan='7'></td></tr>";
                    strLibraryDetails += "<tr><td colspan='1'>c)</td><td colspan='7'>Library phone number (Landline/Mobile) </td><td colspan='1'>:</td><td colspan='7'></td></tr>";
                    strLibraryDetails += "<tr><td colspan='1'>d)</td><td colspan='7'>Number of Supporting Staff </td><td colspan='1'>:</td><td colspan='7'></td></tr>";
                    strLibraryDetails += "<tr><td colspan='1'>e)</td><td colspan='7'>Total Number of Titles </td><td colspan='1'>:</td><td colspan='7'></td></tr>";
                    strLibraryDetails += "<tr><td colspan='1'>f)</td><td colspan='7'>Total Number of Volumes </td><td colspan='1'>:</td><td colspan='7'></td></tr>";
                    strLibraryDetails += "<tr><td colspan='1'>g)</td><td colspan='7'>Total Number of National Journals </td><td colspan='1'>:</td><td colspan='7'></td></tr>";
                    strLibraryDetails += "<tr><td colspan='1'>h)</td><td colspan='7'>Total Number of International National Journals </td><td colspan='1'>:</td><td colspan='7'></td></tr>";
                    strLibraryDetails += "<tr><td colspan='1'>i)</td><td colspan='7'>No. of E-journals </td><td colspan='1'>:</td><td colspan='7'></td></tr>";
                    strLibraryDetails += "<tr><td colspan='1'>j)</td><td colspan='7'>Seating Capacity of Library </td><td colspan='1'>:</td><td colspan='7'></td></tr>";
                    strLibraryDetails += "<tr><td colspan='1'>k)</td><td colspan='7'>Working Hours of library </td><td colspan='1'>:</td><td colspan='7'></td></tr>";
                    strLibraryDetails += "</table><br />";
                }
                #endregion

                #region Library Details

                strLibraryDetails += "<p><strong><u>17. Library Books</u> :</strong></p><br />";
                strLibraryDetails += "<table border='1' cellspacing='0' cellpadding='3' style='font-size: 9px;'><tbody>";
                strLibraryDetails += "<tr>";
                strLibraryDetails += "<td colspan='2'><p>SNo</p></td>";
                strLibraryDetails += "<td colspan='2'><p>Degree</p></td>";
                strLibraryDetails += "<td colspan='2'><p align='center'>T</p></td>";
                strLibraryDetails += "<td colspan='2'><p align='center'>PAYT</p></td>";
                strLibraryDetails += "<td colspan='2'><p align='center'>CF</p></td>";
                strLibraryDetails += "<td colspan='2'><p align='center'>V</p></td>";
                strLibraryDetails += "<td colspan='2'><p align='center'>PAYV</p></td>";
                strLibraryDetails += "<td colspan='2'><p align='center'>CF</p></td>";
                strLibraryDetails += "<td colspan='2'><p align='center'>NJ</p></td>";
                strLibraryDetails += "<td colspan='2'><p align='center'>PAYNJ</p></td>";
                strLibraryDetails += "<td colspan='2'><p align='center'>CF</p></td>";
                strLibraryDetails += "<td colspan='2'><p align='center'>IJ</p></td>";
                strLibraryDetails += "<td colspan='2'><p align='center'>PAYIJ</p></td>";
                strLibraryDetails += "<td colspan='2'><p align='center'>CF</p></td>";
                strLibraryDetails += "<td colspan='2'><p align='center'>EJ</p></td>";
                strLibraryDetails += "<td colspan='2'><p align='center'>PAYEJ</p></td>";
                strLibraryDetails += "<td colspan='2'><p align='center'>CF</p></td>";
                strLibraryDetails += "<td colspan='4'><p align='center'>VSNoofEJ</p></td>";
                strLibraryDetails += "</tr>";

                // var actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(q => q.actualYear).FirstOrDefault();
                // int prAy = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();

                // var cSpcIds =
                //db.jntuh_college_intake_existing.Where(i => i.collegeId == collegeId && (i.academicYearId == prAy - 1 || i.academicYearId == prAy - 2 || i.academicYearId == prAy - 3) && (i.approvedIntake != 0 && i.approvedIntake != null))
                //     //.GroupBy(r => new { r.specializationId })
                //    .Select(s => s.specializationId)
                //    .ToList();

                // var DepartmentsData = db.jntuh_specialization.Where(e => e.isActive == true && cSpcIds.Contains(e.id)).Select(e => e.departmentId).Distinct().ToArray();

                // var DegreeIds = db.jntuh_department.Where(e => e.isActive == true && DepartmentsData.Contains(e.id)).Select(e => e.degreeId).Distinct().ToList();


                List<LibraryDetails> libraryDetails = (from collegeDegree in degreeIds
                                                       join degre in db.jntuh_degree on collegeDegree equals degre.id
                                                       where (degre.isActive == true)
                                                       orderby degre.degree
                                                       select new LibraryDetails
                                                       {
                                                           degreeId = collegeDegree,
                                                           degree = degre.degree,
                                                           totalTitles = null,
                                                           totalVolumes = null,
                                                           totalNationalJournals = null,
                                                           totalInternationalJournals = null,
                                                           totalEJournals = null,
                                                           newTitles = null,
                                                           newVolumes = null,
                                                           newNationalJournals = null,
                                                           newInternationalJournals = null,
                                                           newEJournals = null
                                                       }).ToList();

                var jntuh_college_library_details = db.jntuh_college_library_details.Where(l => l.collegeId == collegeId).ToList();

                if (libraryDetails != null)
                {
                    int sno = 1;
                    foreach (var item in libraryDetails)
                    {
                        var lib = jntuh_college_library_details.Where(l => l.collegeId == collegeId && l.degreeId == item.degreeId).Select(l => l).FirstOrDefault();

                        strLibraryDetails += "<tr>";
                        strLibraryDetails += "<td colspan='2'  align='center'><p>" + sno + "</p></td>";
                        strLibraryDetails += "<td colspan='2'  align='center'><p>" + item.degree + "</p></td>";
                        if (lib != null)
                        {
                            strLibraryDetails += "<td colspan='2' align='center'>" + lib.totalTitles + "</td>";
                            strLibraryDetails += "<td colspan='2' align='center'>" + lib.newTitles + "</td>";
                            strLibraryDetails += "<td colspan='2'><p align='center'></p></td>";
                            strLibraryDetails += "<td colspan='2' align='center'>" + lib.totalVolumes + "</td>";
                            strLibraryDetails += "<td colspan='2' align='center'>" + lib.newVolumes + "</td>";
                            strLibraryDetails += "<td colspan='2'><p align='center'></p></td>";
                            strLibraryDetails += "<td colspan='2' align='center'>" + lib.totalNationalJournals + "</td>";
                            strLibraryDetails += "<td colspan='2' align='center'>" + lib.newNationalJournals + "</td>";
                            strLibraryDetails += "<td colspan='2'><p align='center'></p></td>";
                            strLibraryDetails += "<td colspan='2' align='center'>" + lib.totalInternationalJournals + "</td>";
                            strLibraryDetails += "<td colspan='2' align='center'>" + lib.totalInternationalJournals + "</td>";
                            strLibraryDetails += "<td colspan='2'><p align='center'></p></td>";
                            strLibraryDetails += "<td colspan='2' align='center'>" + lib.totalEJournals + "</td>";
                            strLibraryDetails += "<td colspan='2' align='center'>" + lib.newEJournals + "</td>";
                            strLibraryDetails += "<td colspan='2'><p align='center'></p></td>";
                            strLibraryDetails += "<td colspan='4' align='center'>" + lib.subscription + "</td>";
                        }
                        else
                        {
                            strLibraryDetails += "<td colspan='2' align='center'></td>";
                            strLibraryDetails += "<td colspan='2' align='center'></td>";
                            strLibraryDetails += "<td colspan='2'><p align='center'></p></td>";
                            strLibraryDetails += "<td colspan='2' align='center'></td>";
                            strLibraryDetails += "<td colspan='2' align='center'></td>";
                            strLibraryDetails += "<td colspan='2'><p align='center'></p></td>";
                            strLibraryDetails += "<td colspan='2' align='center'></td>";
                            strLibraryDetails += "<td colspan='2' align='center'></td>";
                            strLibraryDetails += "<td colspan='2'><p align='center'></p></td>";
                            strLibraryDetails += "<td colspan='2' align='center'></td>";
                            strLibraryDetails += "<td colspan='2' align='center'></td>";
                            strLibraryDetails += "<td colspan='2'><p align='center'></p></td>";
                            strLibraryDetails += "<td colspan='2' align='center'></td>";
                            strLibraryDetails += "<td colspan='2' align='center'></td>";
                            strLibraryDetails += "<td colspan='2'><p align='center'></p></td>";
                            strLibraryDetails += "<td colspan='4' align='center'></td>";
                        }

                        strLibraryDetails += "</tr>";
                        sno++;
                    }
                }
                strLibraryDetails += "</tbody></table><br />";
                strLibraryDetails += "<b>T</b>-Number of Titles,&nbsp;";
                strLibraryDetails += "<b>PAYT</b>-Number of Titles added in this Academic Year,&nbsp;";
                strLibraryDetails += "<b>V</b>-Number of Volume,&nbsp;";
                strLibraryDetails += "<b>PAYV</b>-Number of Volumes added in this Academic Year,&nbsp;";
                strLibraryDetails += "<b>NJ</b>-Number of National Print Journals,&nbsp;";
                strLibraryDetails += "<b>PAYNJ</b>-Number of National Print Journals added in this Academic Year,&nbsp;";
                strLibraryDetails += "<b>IJ</b>-Number of International Print Journals,&nbsp;";
                strLibraryDetails += "<b>PAYIJ</b>-Number of International Print Journals added in this Academic Year,&nbsp;";
                strLibraryDetails += "<b>EJ</b>-Number of e-Journals,&nbsp;";
                strLibraryDetails += "<b>PAYEJ</b>-Number of e-Journals added in this Academic Year,&nbsp;";
                strLibraryDetails += "<b>VSNoofEJ</b>-Valid Subscription Number of e-Journals,&nbsp;";
                strLibraryDetails += "<b>CF</b>-Committee Findings";
                strLibraryDetails += "<br /><br />";
                #endregion

                #region Computer Lab Facilities
                strLibraryDetails += "<p style='font-size: 9px;'><strong>18. Computers</strong> :</p><br />";
                //strLibraryDetails += "<p><u><strong>Computer Lab Facilities</strong></u> :</p><br />";
                strLibraryDetails += "<p>i) Printers availability :</p>";
                strLibraryDetails += "<p>ii) Working Hours of Computer Lab : From: _________ (HH:MM) To : _________ (HH:MM)</p><p>iii) Internet accessibility (timings) : From: _________ (HH:MM) To : _________ (HH:MM)</p><br />";

                #endregion

                #region /Computers – Students Ratio
                strLibraryDetails += "<table border='1' cellspacing='0' cellpadding='3' style='font-size: 9px;width:75%;'><tbody>";
                strLibraryDetails += "<tr>";
                strLibraryDetails += "<td  colspan='2'><p><b>Degree</b></p></td>";
                strLibraryDetails += "<td  colspan='2'><p align='center'><b>Available Computers</b></p></td>";
                strLibraryDetails += "<td  colspan='2'><p align='center'><b>Committee Findings</b></p></td>";
                strLibraryDetails += "</tr>";

                //List<jntuh_college_degree> jntuh_college_degree = db.jntuh_college_degree.Where(d => DegreeIds.Contains(d.id) && d.isActive == true).ToList();

                var jntuh_college_degree = db.jntuh_degree.Where(d => degreeIds.Contains(d.id) && d.isActive == true).OrderBy(d => d.degreeDisplayOrder).ToList();

                List<ComputerStudentRatioDetails> computerStudentDetails = new List<ComputerStudentRatioDetails>();
                if (jntuh_college_degree != null)
                {
                    var jntuh_college_computer_student_ratio = db.jntuh_college_computer_student_ratio.Where(d => d.collegeId == collegeId).ToList();
                    var jntuh_degree = db.jntuh_degree;
                    foreach (var item in jntuh_college_degree)
                    {

                        int degreeId = item.id;
                        string degree = jntuh_degree.Where(d => d.isActive == true && d.id == item.id).Select(d => d.degree).FirstOrDefault();
                        int availableComputers = jntuh_college_computer_student_ratio.Where(computerStudenRatio => computerStudenRatio.collegeId == collegeId &&
                                                                                                computerStudenRatio.degreeId == item.id)
                                                                                          .Select(computerStudenRatio => computerStudenRatio.availableComputers)
                                                                                          .FirstOrDefault();
                        strLibraryDetails += "<tr>";
                        strLibraryDetails += "<td colspan='2'><p>" + degree + "</p></td>";
                        strLibraryDetails += "<td align='right' colspan='2'>" + availableComputers + "</td>";
                        strLibraryDetails += "<td align='center' colspan='2'></td>";
                        strLibraryDetails += "</tr>";
                    }
                }
                strLibraryDetails += "</tbody></table>";
                #endregion
                //
                strLibraryDetails += "</td></tr></table><br />";
                int enclosureId = db.jntuh_enclosures.Where(e => e.documentName == "MAC-ADDRESS LIST").Select(e => e.id).FirstOrDefault();
                if (enclosureId > 0)
                {
                    strLibraryDetails += ReadExcelData(db.jntuh_college_enclosures.Where(d => d.collegeID == collegeId && d.enclosureId == enclosureId).ToList());
                }
                contents = contents.Replace("##LibraryDetails##", strLibraryDetails);
                //List<IElement> parsedHtmlElements = HTMLWorker.ParseToList(new StringReader(contents), null);
                return contents;
            }
        }

        private string ReadExcelData(List<jntuh_college_enclosures> list)
        {
            using (var db = new uaaasDBContext())
            {
                string enclosures = string.Empty;
                var MacPath = list.Distinct();
                int NewCollegeId = list.Select(c => c.collegeID).Distinct().FirstOrDefault();
                try
                {
                    int rowCount = 1;
                    int CheckingCount = 0;
                    IQueryable<jntuh_college_macaddress> jntuh_college_macaddress = db.jntuh_college_macaddress.Where(s => s.collegeId == NewCollegeId).Select(d => d);

                    var jntuh_college_macaddress_new = jntuh_college_macaddress.GroupBy(s => s.labname).ToDictionary(key => key.Key, value => value.Select(i => i.labname)).ToList();

                    enclosures += "<p style='text-align:left;'><b><u>MAC-ADDRESS LIST</u></b></p><br/>";
                    foreach (var item5 in jntuh_college_macaddress_new)
                    {

                        var data = jntuh_college_macaddress.Where(s => s.labname == item5.Key).Select(e => e).ToList();
                        enclosures += "<p style='float:left'><b><u>" + data.Select(e => e.labname).FirstOrDefault() + "-" + data.Select(e => e.location).FirstOrDefault() + "</u></b></p>";
                        enclosures += "<br/>";
                        enclosures += "<table border='1' cellspacing='0' cellpadding='3' style='font-size: 9px;'>";
                        enclosures += "<tr>";
                        enclosures += "<th style='width:5px'><p>SNo.</p></th>";
                        enclosures += "<th style='width:10px' colspan='2'><p>MAC-ADDRESS</p></th>";
                        enclosures += "<th style='width:5px'><p>SNo.</p></th>";
                        enclosures += "<th style='width:10px' colspan='2'><p>MAC-ADDRESS</p></th>";
                        enclosures += "<th style='width:5px'><p>SNo.</p></th>";
                        enclosures += "<th style='width:10px' colspan='2'><p>MAC-ADDRESS</p></th>";
                        enclosures += "</tr>";

                        string TrtagClosingCheck = null;
                        int DataCount = data.Count();
                        int change = 1;
                        foreach (var item4 in data)
                        {
                            for (int i = change; i <= change; i++)
                            {
                                if (i == 1)
                                {
                                    enclosures += "<tr>";
                                    enclosures += "<td align='center'><p>" + rowCount + "</p></td>";

                                    if (item4.macaddress != null)
                                    {
                                        enclosures += "<td colspan='2'><p>" + item4.macaddress + "</p></td>";
                                    }
                                    else
                                    {
                                        enclosures += "<td colspan='2'><p>" + string.Empty + "</p></td>";
                                    }
                                    TrtagClosingCheck = "One";
                                }
                                else if (i == 2)
                                {
                                    enclosures += "<td align='center'><p>" + rowCount + "</p></td>";
                                    if (item4.macaddress != null)
                                    {
                                        enclosures += "<td colspan='2'><p>" + item4.macaddress + "</p></td>";
                                    }
                                    else
                                    {
                                        enclosures += "<td colspan='2'><p>" + string.Empty + "</p></td>";
                                    }
                                    TrtagClosingCheck = "Two";
                                }
                                else
                                {
                                    enclosures += "<td align='center'><p>" + rowCount + "</p></td>";
                                    if (item4.macaddress != null)
                                    {
                                        enclosures += "<td colspan='2'><p>" + item4.macaddress + "</p></td>";
                                    }
                                    else
                                    {
                                        enclosures += "<td colspan='2'><p>" + string.Empty + "</p></td>";
                                    }
                                    enclosures += "</tr>";
                                    TrtagClosingCheck = "Three";
                                }
                            }

                            if (change == 1)
                                change = 2;
                            else if (change == 2)
                                change = 3;
                            else
                                change = 1;

                            if (rowCount == DataCount)
                            {
                                if (TrtagClosingCheck == "One")
                                {
                                    enclosures += "<td align='center'><p>--</p></td><td colspan='2'><p>--</p></td><td align='center'><p>--</p></td><td colspan='2'><p>--</p></td>";
                                    enclosures += "</tr>";
                                }
                                else if (TrtagClosingCheck == "Two")
                                {
                                    enclosures += "<td><p>--</p></td><td colspan='2'><p>--</p></td>";
                                    enclosures += "</tr>";
                                }
                            }
                            rowCount++;
                        }
                        enclosures += "</table>";
                        rowCount = 1;
                        CheckingCount++;
                    }
                }
                catch (Exception)
                {
                    enclosures += "<p style='font-size: 9px;'><i>Can't read the Junk file uploaded by the college.</i></p>";
                }
                //List<IElement> parsedHtmlElements = HTMLWorker.ParseToList(new StringReader(enclosures), null);
                return enclosures;
            }
        }

        public string InternetBandwidthDetails(int collegeId, string contents, List<int> degreeIds)
        {

            using (var db = new uaaasDBContext())
            {
                // var actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(q => q.actualYear).FirstOrDefault();
                // int prAy = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
                // var cSpcIds =
                //db.jntuh_college_intake_existing.Where(i => i.collegeId == collegeId && (i.academicYearId == prAy - 1 || i.academicYearId == prAy - 2 || i.academicYearId == prAy - 3) && (i.approvedIntake != 0 && i.approvedIntake != null))
                //     //.GroupBy(r => new { r.specializationId })
                //    .Select(s => s.specializationId)
                //    .ToList();

                // var DepartmentsData = db.jntuh_specialization.Where(e => e.isActive == true && cSpcIds.Contains(e.id)).Select(e => e.departmentId).Distinct().ToArray();

                // var DegreeIds = db.jntuh_department.Where(e => e.isActive == true && DepartmentsData.Contains(e.id)).Select(e => e.degreeId).Distinct().ToList();

                string strInternetBandwidthDetails = string.Empty;
                strInternetBandwidthDetails += "<table border='1' cellspacing='0' cellpadding='3' style='font-size: 9px;'><tbody>";
                strInternetBandwidthDetails += "<tr>";
                strInternetBandwidthDetails += "<td  colspan='2'><p><b>Degree</b></p></td>";
                strInternetBandwidthDetails += "<td  colspan='2'><p align='center'><b>Available Bandwidth</b></p></td>";
                strInternetBandwidthDetails += "<td  colspan='2'><p align='center'><b>Committee Findings</b></p></td>";
                strInternetBandwidthDetails += "</tr>";
                //List<jntuh_college_degree> collegeDegree = db.jntuh_college_degree.Where(d => d.collegeId == collegeId && d.isActive == true).ToList();

                var jntuh_degree = db.jntuh_degree.Where(d => degreeIds.Contains(d.id)).OrderBy(d => d.degreeDisplayOrder).ToList();
                var jntuh_college_internet_bandwidth = db.jntuh_college_internet_bandwidth.Where(i => i.collegeId == collegeId).ToList();
                foreach (var item in jntuh_degree)
                {
                    string degree = jntuh_degree.Where(d => d.isActive == true && d.id == item.id).Select(d => d.degree).FirstOrDefault();
                    decimal availableInternetSpeed = jntuh_college_internet_bandwidth.Where(internetbandwidth => internetbandwidth.collegeId == collegeId &&
                                                                                            internetbandwidth.degreeId == item.id)
                                                                                     .Select(internetbandwidth => internetbandwidth.availableInternetSpeed)
                                                                                     .FirstOrDefault();
                    strInternetBandwidthDetails += "<tr>";
                    strInternetBandwidthDetails += "<td  colspan='2'><p>" + degree + "</p></td>";
                    strInternetBandwidthDetails += "<td  align='center' colspan='2'>" + availableInternetSpeed + "</td>";
                    strInternetBandwidthDetails += "<td  align='center' colspan='2'></td>";
                    strInternetBandwidthDetails += "</tr>";
                }
                strInternetBandwidthDetails += "</tbody></table>";
                contents = contents.Replace("##InternetBandwidthDetails##", strInternetBandwidthDetails);
                //List<IElement> parsedHtmlElements = HTMLWorker.ParseToList(new StringReader(contents), null);
                return contents;
            }
        }

        public string LegalSystemSoftwareDetails(int collegeId, string contents, List<int> degreeIds)
        {
            using (var db = new uaaasDBContext())
            {
                string strLegalSystemSoftwareDetails = string.Empty;
                strLegalSystemSoftwareDetails += "<table border='1' cellspacing='0' cellpadding='3' style='font-size: 9px;'><tbody>";
                strLegalSystemSoftwareDetails += "<tr>";
                strLegalSystemSoftwareDetails += "<td  rowspan='2' colspan='2'><p><b>Degree</b></p></td>";
                strLegalSystemSoftwareDetails += "<td  colspan='4'><p align='center'><b>Available</b></p></td>";
                strLegalSystemSoftwareDetails += "<td  colspan='4'><p align='center'><b>Committee Findings</b></p></td>";
                strLegalSystemSoftwareDetails += "</tr>";
                strLegalSystemSoftwareDetails += "<tr>";
                strLegalSystemSoftwareDetails += "<td  colspan='2'><p align='center'><b>System Software</b></p></td>";
                strLegalSystemSoftwareDetails += "<td  colspan='2'><p align='center'><b>Application Software</b></p></td>";
                strLegalSystemSoftwareDetails += "<td  colspan='2'><p align='center'><b>System Software</b></p></td>";
                strLegalSystemSoftwareDetails += "<td  colspan='2'><p align='center'><b>Application Software</b></p></td>";
                strLegalSystemSoftwareDetails += "</tr>";

                // var actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(q => q.actualYear).FirstOrDefault();
                // int prAy = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();

                // var cSpcIds =
                //db.jntuh_college_intake_existing.Where(i => i.collegeId == collegeId && (i.academicYearId == prAy - 1 || i.academicYearId == prAy - 2 || i.academicYearId == prAy - 3) && (i.approvedIntake != 0 && i.approvedIntake != null))
                //     //.GroupBy(r => new { r.specializationId })
                //    .Select(s => s.specializationId)
                //    .ToList();

                // var DepartmentsData = db.jntuh_specialization.Where(e => e.isActive == true && cSpcIds.Contains(e.id)).Select(e => e.departmentId).Distinct().ToArray();

                // var DegreeIds = db.jntuh_department.Where(e => e.isActive == true && DepartmentsData.Contains(e.id)).Select(e => e.degreeId).Distinct().ToList();

                List<CollegeLegalSoftwarDetails> legalSoftwarDetails = (from CollegeDegree in degreeIds
                                                                        join Degree in db.jntuh_degree on CollegeDegree equals Degree.id
                                                                        orderby Degree.degreeDisplayOrder
                                                                        where (Degree.isActive == true && Degree.isActive == true)
                                                                        select new CollegeLegalSoftwarDetails
                                                                        {
                                                                            id = CollegeDegree,
                                                                            degreeId = CollegeDegree,
                                                                            degree = Degree.degree,
                                                                            availableApplicationSoftware = 0,
                                                                            availableSystemSoftware = 0
                                                                        }).ToList();
                if (legalSoftwarDetails != null)
                {
                    var ids = legalSoftwarDetails.Select(l => l.degreeId).ToList();
                    var jntuh_college_legal_softwares = db.jntuh_college_legal_software.Where(it => it.collegeId == collegeId &&
                        ids.Contains(it.degreeId))
                        .Select(it => new
                        {
                            it.degreeId,
                            it.availableApplicationSoftware,
                            it.availableSystemSoftware
                        }).ToList();
                    foreach (var item in legalSoftwarDetails)
                    {
                        item.availableApplicationSoftware = jntuh_college_legal_softwares.Where(legalSoftware =>
                                                                                                  legalSoftware.degreeId == item.degreeId)
                                                                                           .Select(legalSoftware => legalSoftware.availableApplicationSoftware)
                                                                                           .FirstOrDefault();
                        item.availableSystemSoftware = jntuh_college_legal_softwares.Where(legalSoftware =>
                                                                                                  legalSoftware.degreeId == item.degreeId)
                                                                                           .Select(legalSoftware => legalSoftware.availableSystemSoftware)
                                                                                           .FirstOrDefault();
                        strLegalSystemSoftwareDetails += "<tr>";
                        strLegalSystemSoftwareDetails += "<td  colspan='2'><p>" + item.degree + "</p></td>";
                        strLegalSystemSoftwareDetails += "<td  align='center' colspan='2'>" + item.availableSystemSoftware + "</td>";
                        strLegalSystemSoftwareDetails += "<td  align='center' colspan='2'>" + item.availableApplicationSoftware + "</td>";
                        strLegalSystemSoftwareDetails += "<td  align='center' colspan='2'></td>";
                        strLegalSystemSoftwareDetails += "<td  align='center' colspan='2'></td>";
                        strLegalSystemSoftwareDetails += "</tr>";
                    }
                }
                strLegalSystemSoftwareDetails += "</tbody></table>";
                contents = contents.Replace("##LegalSystemSoftwareDetails##", strLegalSystemSoftwareDetails);
                // List<IElement> parsedHtmlElements = HTMLWorker.ParseToList(new StringReader(contents), null);
                return contents;
            }
        }

        public string PrintersDetails(int collegeId, string contents, List<int> degreeIds)
        {
            using (var db = new uaaasDBContext())
            {
                string strPrintersDetails = string.Empty;
                strPrintersDetails += "<table border='1' cellspacing='0' cellpadding='3' style='font-size: 9px;'><tbody>";
                strPrintersDetails += "<tr>";
                strPrintersDetails += "<td  colspan='2'><p><b>Degree</b></p></td>";
                strPrintersDetails += "<td  valign='top' colspan='2'><p align='center'><b>Available Printers</b></p></td>";
                strPrintersDetails += "<td  valign='top' colspan='2'><p align='center'><b>Committee Findings</b></p></td>";
                strPrintersDetails += "</tr>";
                List<CollegePrinterDetails> PrinterDetails = (from CollegeDegree in degreeIds
                                                              join Degree in db.jntuh_degree on CollegeDegree equals Degree.id
                                                              orderby Degree.degreeDisplayOrder
                                                              where (Degree.isActive == true)
                                                              select new CollegePrinterDetails
                                                              {
                                                                  degreeId = CollegeDegree,
                                                                  degree = Degree.degree,
                                                                  availableComputers = 0,
                                                                  availablePrinters = 0
                                                              }).ToList();
                if (PrinterDetails != null)
                {
                    var jntuh_college_computer_lab_printers = db.jntuh_college_computer_lab_printers.Where(p => p.collegeId == collegeId).ToList();

                    foreach (var item in PrinterDetails)
                    {
                        int availablePrinters = jntuh_college_computer_lab_printers.Where(p => p.collegeId == collegeId && p.degreeId == item.degreeId).Select(p => p.availablePrinters).FirstOrDefault();

                        strPrintersDetails += "<tr>";
                        strPrintersDetails += "<td  colspan='2'><p>" + item.degree + "</p></td>";
                        strPrintersDetails += "<td  valign='top' align='center' colspan='2'>" + availablePrinters + "</td>";
                        strPrintersDetails += "<td  valign='top' align='center' colspan='2'></td>";
                        strPrintersDetails += "</tr>";

                    }
                }
                strPrintersDetails += "</tbody></table>";
                contents = contents.Replace("##PrintersDetails##", strPrintersDetails);
                return contents;
            }
        }

        public string ExaminationBranchDetails(int collegeId, string contents)
        {
            using (var db = new uaaasDBContext())
            {
                string strExaminationBranchDetails = string.Empty;
                int staffsno = 0;
                int edepsno = 0;
                string strstaff = string.Empty;
                jntuh_college_examination_branch examinationBranch = db.jntuh_college_examination_branch.Where(e => e.collegeId == collegeId).Select(e => e).FirstOrDefault();
                jntuh_college_examination_branch_security examinationBranchSecurity = db.jntuh_college_examination_branch_security.Where(e => e.collegeId == collegeId).Select(e => e).FirstOrDefault();
                List<jntuh_college_examination_branch_staff> staffMembers = db.jntuh_college_examination_branch_staff.Where(s => s.collegeId == collegeId).Select(s => s).ToList();
                List<CollegeEDEPDetails> edepDetails = (from e in db.jntuh_edep_equipment
                                                        join eb in db.jntuh_college_examination_branch_edep
                                                        on e.id equals eb.EDEPEquipmentId
                                                        where (e.isActive == true && eb.collegeId == collegeId)
                                                        select new CollegeEDEPDetails
                                                        {
                                                            EDEPEquipmentId = eb.EDEPEquipmentId,
                                                            EDEPEquipment = e.equipmentName,
                                                            ActualValue = eb.ActualValue,
                                                            id = e.id
                                                        }).ToList();


                strExaminationBranchDetails += "<p style='font-size: 9px;'><strong><u>25. Examination Branch</u> :</strong></p><br />";
                strExaminationBranchDetails += "<table border='0' cellpadding='0' cellspacing='0' id='page17' style='font-size: 9px;'><tr>";
                if (examinationBranch != null)
                {
                    strExaminationBranchDetails += "<td align='left' valign='top'><p>a) Examination branch exists :&nbsp;Yes</p><p>b) If Yes, Area (In Square meters) : " + examinationBranch.examinationBranchArea + "</p><p>c) Staff Members : <em>(Please specify the details in the table below)</em></p><br />";
                }
                else
                {
                    strExaminationBranchDetails += "<td align='left' valign='top'><p>a) Examination branch exists :</p><p>b) If Yes, Area (In Square meters) : ___________</p><p>c) Staff Members : <em>(Please specify the details in the table below)</em></p><br />";
                }
                //Staff Members Start
                strExaminationBranchDetails += "<table border='1' cellspacing='0' cellpadding='3' style='font-size: 9px;'><tbody>";
                strExaminationBranchDetails += "<tr><td width='57'><p align='center'>S.No</p></td><td width='336'><p align='left'>Name of the Staff</p></td><td width='133'><p align='left'>Designation</p></td><td width='165'><p align='left'>Teaching/Non-Teaching</p></td></tr>";
                if (staffMembers != null)
                {
                    foreach (var item in staffMembers)
                    {
                        staffsno++;
                        if (item.isTeachingStaff == true)
                        {
                            strstaff = "Teaching";
                        }
                        else
                        {
                            strstaff = "Non-Teaching";
                        }
                        strExaminationBranchDetails += "<tr><td width='57'><p align='center'>" + staffsno + "</p></td><td width='336' align='left'>" + item.staffName + "</td><td width='133' align='left'>" + item.staffDesignation + "</td><td width='165' align='left'>" + strstaff + "</td></tr>";
                    }
                }
                strExaminationBranchDetails += "</tbody></table><br />";
                //Staff Members End


                if (examinationBranch != null)
                {
                    strExaminationBranchDetails += "<p style='font-size: 9px;'>e) Confidential room for question paper preparation :";
                    if (examinationBranch.isConfidenatialRoomExists == true)
                    {
                        strExaminationBranchDetails += "&nbsp;Yes";
                    }
                    else
                    {
                        strExaminationBranchDetails += "&nbsp;No";
                    }
                    strExaminationBranchDetails += "</p><p style='font-size: 9px;'>f) The examination branch is located adjacent to the Principal’s room :";
                    if (examinationBranch.isAdjacentPrincipalRoom == true)
                    {
                        strExaminationBranchDetails += "&nbsp;Yes";
                    }
                    else
                    {
                        strExaminationBranchDetails += "&nbsp;No";
                    }
                    strExaminationBranchDetails += "</p><p style='font-size: 9px;'>g) Details of measures taken by the college to maintain the Confidentiality/Security of the Examination Branch :</p><p style='font-size: 9px;'>1)" + examinationBranchSecurity.securityMesearesTaken1 + "</u></p><p style='font-size: 9px;'>2)" + examinationBranchSecurity.securityMesearesTaken2 + "</u></p><p style='font-size: 9px;'>3)" + examinationBranchSecurity.securityMesearesTaken3 + "</u></p></td></tr></table>";
                }
                else
                {
                    strExaminationBranchDetails += "<p style='font-size: 9px;'>e) Confidential room for question paper preparation :";
                    strExaminationBranchDetails += "";
                    strExaminationBranchDetails += "</p><p style='font-size: 9px;'>f) The examination branch is located adjacent to the Principal’s room :";
                    strExaminationBranchDetails += "";
                    strExaminationBranchDetails += "</p><p style='font-size: 9px;'>g) Details of measures taken by the college to maintain the Confidentiality/Security of the Examination Branch :</p><p style='font-size: 9px;'>1)" + string.Empty + "</u></p><p style='font-size: 9px;'>2)" + string.Empty + "</u></p><p style='font-size: 9px;'>3)" + string.Empty + "</u></p></td></tr></table>";
                }

                //EDEP Start
                #region EDEP
                strExaminationBranchDetails += "<br />";
                strExaminationBranchDetails += "<p style='font-size: 9px'><strong><u>26. EDEP Equipment</u> :</strong></p><br />";
                //strExaminationBranchDetails += "<p style='font-size: 9px;'>d) <u><strong>Equipment for EDEP Examination</strong></u> :</p><br />";
                strExaminationBranchDetails += "<table border='1' cellspacing='0' cellpadding='3' style='font-size: 9px;'><tbody>";
                strExaminationBranchDetails += "<tr><td width='57'><p align='center'>S.No</p></td><td width='468'><p align='center'>Equipment</p></td><td width='82'><p align='center'>Number Available</p></td></tr>";
                //foreach
                if (edepDetails != null)
                {
                    var ids = edepDetails.Select(e => e.id).ToList();
                    var jntuh_edep_equipments = db.jntuh_edep_equipment.Where(it => it.isActive && ids.Contains(it.id))
                        .Select(it => new
                        {
                            it.id,
                            it.normsEDEP
                        }).ToList();
                    foreach (var item in edepDetails)
                    {
                        edepsno++;
                        var normsEDEP = jntuh_edep_equipments.Where(e => e.id == item.id).Select(e => e.normsEDEP).FirstOrDefault();
                        strExaminationBranchDetails += "<tr><td width='57'><p align='center'>" + edepsno + "</p></td><td width='468'><p>" + item.EDEPEquipment + "</p></td><td width='82' align='center'>" + item.ActualValue + "</td></tr>";
                    }
                }

                strExaminationBranchDetails += "</tbody></table>";
                #endregion
                //EDEP End
                contents = contents.Replace("##ExaminationBranchDetails##", strExaminationBranchDetails);
                return contents;
            }
        }

        public string DesirableRequirementsDetails(int collegeId, string contents)
        {
            using (var db = new uaaasDBContext())
            {
                string strDesirableRequirements = string.Empty;
                int committeeSNo = 0;
                #region GrievanceRedressalCommittee
                strDesirableRequirements += "<table border='1' cellspacing='0' cellpadding='4'><tbody>";
                strDesirableRequirements += " <tr><td width='57'><p align='center'>S.No.</p></td><td width='430'><p align='left'>Name</p></td><td width='204'><p align='left'>Designation in the committee</p></td></tr>";

                List<GrievanceRedressalCommittee> committee = (from gc in db.jntuh_college_grievance_committee
                                                               join d in db.jntuh_grc_designation on gc.memberDesignation equals d.id
                                                               join ad in db.jntuh_designation on gc.actualDesignation equals ad.id
                                                               where gc.collegeId == collegeId
                                                               select new GrievanceRedressalCommittee
                                                               {
                                                                   id = gc.id,
                                                                   collegeId = gc.collegeId,
                                                                   memberDesignation = gc.memberDesignation,
                                                                   memberName = gc.memberName,
                                                                   designationName = d.Designation

                                                               }).OrderBy(r => r.memberName).ToList();
                int committeecount = committee.Count();
                if (committee != null)
                {
                    foreach (var item in committee)
                    {
                        committeeSNo++;
                        strDesirableRequirements += "<tr><td width='57'><p align='center'>" + committeeSNo + "</p></td><td width='430' align='left'>" + item.memberName + "</td><td width='204' align='left'>" + item.designationName + "</td></tr>";
                    }
                }
                strDesirableRequirements += "</tbody></table>";
                strDesirableRequirements += "<p><br />Total Complaints Received: " + committeecount + "</u> (Please specify 5 major complaints briefly)</p><br>";

                #endregion

                #region GrievanceRedressalComplaints
                strDesirableRequirements += "<table border='1' cellspacing='0' cellpadding='4'><tbody>";
                strDesirableRequirements += " <tr><td width='57'><p align='center'>S.No.</p></td><td width='384'><p align='left'>Complaint</p></td><td width='250' align='left'><p>Action Taken</p></td></tr>";
                int complaintsSNo = 0;

                List<GrievanceRedressalComplaints> complaints = db.jntuh_college_grievance_complaints.Where(a => a.collegeId == collegeId).Select(a =>
                                                 new GrievanceRedressalComplaints
                                                 {
                                                     id = a.id,
                                                     collegeId = a.collegeId,
                                                     complaintReceived = a.complaintReceived,
                                                     actionsTaken = a.actionsTaken
                                                 }).OrderBy(r => r.actionsTaken).ToList();

                if (complaints.Count() == 0 || complaints == null)
                {
                    strDesirableRequirements += " <tr><td colspan='3'><p align='center'>NIL</p></td></tr>";
                }
                else
                {
                    foreach (var item in complaints)
                    {
                        complaintsSNo++;
                        strDesirableRequirements += " <tr><td width='57'><p align='center'>" + complaintsSNo + "</p></td><td width='384' align='left'>" + item.complaintReceived + "</td><td width='250' align='left'>" + item.actionsTaken + "</td></tr>";
                    }
                }
                strDesirableRequirements += "</tbody></table>";
                #endregion
                contents = contents.Replace("##DesirableRequirements##", strDesirableRequirements);
                return contents;
            }
        }

        public string AntiRaggingCommitteeDetails(int collegeId, string contents)
        {
            using (var db = new uaaasDBContext())
            {

                string strAntiRaggingCommittee = string.Empty;
                int antiRaggingCommitteeSNo = 0;
                int antiRaggingComplaintsSNo = 0;
                #region AntiRaggingCommittee
                strAntiRaggingCommittee += "<table border='1' cellspacing='0' cellpadding='4'><tbody>";
                strAntiRaggingCommittee += " <tr><td width='57'><p align='center'>S.No.</p></td><td width='430' align='left'><p>Name</p></td><td width='204'><p align='left'>Designation in the committee</p></td></tr>";

                List<AntiRaggingCommittee> antiRaggingCommittee = (from a in db.jntuh_college_antiragging_committee
                                                                   join d in db.jntuh_grc_designation on a.memberDesignation equals d.id
                                                                   join ad in db.jntuh_designation on a.actualDesignation equals ad.id
                                                                   where a.collegeId == collegeId
                                                                   select new AntiRaggingCommittee
                                                                   {
                                                                       id = a.id,
                                                                       collegeId = a.collegeId,
                                                                       memberDesignation = a.memberDesignation,
                                                                       memberName = a.memberName,
                                                                       designationName = d.Designation
                                                                   }).OrderBy(r => r.memberName).ToList();

                int antiRaggingcommitteecount = antiRaggingCommittee.Count();
                if (antiRaggingCommittee != null)
                {
                    foreach (var item in antiRaggingCommittee)
                    {
                        antiRaggingCommitteeSNo++;
                        strAntiRaggingCommittee += "<tr><td width='57'><p align='center'>" + antiRaggingCommitteeSNo + "</p></td><td width='430' align='left'>" + item.memberName + "</td><td width='204' align='left'>" + item.designationName + "</td></tr>";
                    }
                }
                strAntiRaggingCommittee += "</tbody></table>";
                strAntiRaggingCommittee += "<p><br />Total Complaints Received: " + antiRaggingcommitteecount + "</u> (Please specify 5 major complaints briefly)</p><br>";

                #endregion

                #region AntiRaggingComplaints
                strAntiRaggingCommittee += "<table border='1' cellspacing='0' cellpadding='4'><tbody>";
                strAntiRaggingCommittee += " <tr><td width='57'><p align='center'>S.No.</p></td><td width='384'><p align='left'>Complaint</p></td><td width='250' align='left' align='left'><p align='left'>Action Taken</p></td></tr>";


                List<AntiRaggingComplaints> antiRaggingComplaints = db.jntuh_college_antiragging_complaints.Where(a => a.collegeId == collegeId).Select(a =>
                                                  new AntiRaggingComplaints
                                                  {
                                                      id = a.id,
                                                      collegeId = a.collegeId,
                                                      complaintReceived = a.complaintReceived,
                                                      actionsTaken = a.actionsTaken,
                                                  }).OrderBy(r => r.actionsTaken).ToList();

                if (antiRaggingComplaints.Count() == 0 || antiRaggingComplaints == null)
                {
                    strAntiRaggingCommittee += " <tr><td colspan='3'><p align='center'>NIL</p></td></tr>";
                }
                else
                {
                    foreach (var item in antiRaggingComplaints)
                    {
                        antiRaggingComplaintsSNo++;
                        strAntiRaggingCommittee += " <tr><td width='57'><p align='center'>" + antiRaggingComplaintsSNo + "</p></td><td width='384' align='left'>" + item.complaintReceived + "</td><td width='250' align='left'>" + item.actionsTaken + "</td></tr>";
                    }
                }
                strAntiRaggingCommittee += "</tbody></table>";
                #endregion
                contents = contents.Replace("##AntiRaggingCommittee##", strAntiRaggingCommittee);
                return contents;
            }
        }

        public string WomenProtectionCellDetails(int collegeId, string contents)
        {
            using (var db = new uaaasDBContext())
            {
                string strwomenProtectionCell = string.Empty;
                int womenProtectionCellSNo = 0;
                int womenProtectionCellComplaintsSNo = 0;
                #region WomenProtectionCell
                strwomenProtectionCell += "<table border='1' cellspacing='0' cellpadding='4'><tbody>";
                strwomenProtectionCell += " <tr><td width='57'><p align='center'>S.No.</p></td><td width='430' align='left'><p>Name</p></td><td width='204'><p align='left'>Designation in the committee</p></td></tr>";

                List<WomenProtectionCell> womenProtectionCell = (from a in db.jntuh_college_women_protection_cell
                                                                 join d in db.jntuh_grc_designation on a.memberDesignation equals d.id
                                                                 where a.collegeId == collegeId
                                                                 select new WomenProtectionCell
                                                                 {
                                                                     id = a.id,
                                                                     collegeId = a.collegeId,
                                                                     memberDesignation = a.memberDesignation,
                                                                     memberName = a.memberName,
                                                                     designationName = d.Designation
                                                                 }).OrderBy(r => r.memberName).ToList();

                int womenProtectionCellcount = womenProtectionCell.Count();
                if (womenProtectionCell != null)
                {
                    foreach (var item in womenProtectionCell)
                    {
                        womenProtectionCellSNo++;
                        strwomenProtectionCell += "<tr><td width='57'><p align='center'>" + womenProtectionCellSNo + "</p></td><td width='430' align='left'>" + item.memberName + "</td><td width='204' align='left'>" + item.designationName + "</td></tr>";
                    }
                }
                strwomenProtectionCell += "</tbody></table>";
                strwomenProtectionCell += "<p><br />Total Complaints Received: " + womenProtectionCellcount + "</u> (Please specify 5 major complaints briefly)</p><br>";

                #endregion

                #region WomenProtectionCellComplaints
                strwomenProtectionCell += "<table border='1' cellspacing='0' cellpadding='4'><tbody>";
                strwomenProtectionCell += " <tr><td width='57'><p align='center'>S.No.</p></td><td width='384'><p align='left'>Complaint</p></td><td width='250' align='left' align='left'><p align='left'>Action Taken</p></td></tr>";


                List<WomenProtectionCellComplaints> womenProtectionCellComplaints = db.jntuh_college_women_protection_cell_complaints.Where(a => a.collegeId == collegeId).Select(a =>
                                                  new WomenProtectionCellComplaints
                                                  {
                                                      id = a.id,
                                                      collegeId = a.collegeId,
                                                      complaintReceived = a.complaintReceived,
                                                      actionsTaken = a.actionsTaken,
                                                  }).OrderBy(r => r.actionsTaken).ToList();

                if (womenProtectionCellComplaints.Count() == 0 || womenProtectionCellComplaints == null)
                {
                    strwomenProtectionCell += " <tr><td colspan='3'><p align='center'>NIL</p></td></tr>";
                }
                else
                {
                    foreach (var item in womenProtectionCellComplaints)
                    {
                        womenProtectionCellComplaintsSNo++;
                        strwomenProtectionCell += " <tr><td width='57'><p align='center'>" + womenProtectionCellComplaintsSNo + "</p></td><td width='384' align='left'>" + item.complaintReceived + "</td><td width='250' align='left'>" + item.actionsTaken + "</td></tr>";
                    }
                }
                strwomenProtectionCell += "</tbody></table>";
                #endregion
                contents = contents.Replace("##WomenProtectionCell##", strwomenProtectionCell);
                return contents;
            }
        }

        public string RTIDetails(int collegeId, string contents)
        {
            using (var db = new uaaasDBContext())
            {
                string strRTIDetails = string.Empty;
                int rtiSNo = 0;
                int rtiComplaintsSNo = 0;
                #region RTIDetails
                strRTIDetails += "<table border='1' cellspacing='0' cellpadding='4'><tbody>";
                strRTIDetails += " <tr><td width='57'><p align='center'>S.No.</p></td><td width='430' align='left'><p>Name</p></td><td width='204'><p align='left'>Designation in the committee</p></td></tr>";

                List<RTIDetails> rtiDetails = (from a in db.jntuh_college_rti_details
                                               join d in db.jntuh_grc_designation on a.memberDesignation equals d.id
                                               where a.collegeId == collegeId
                                               select new RTIDetails
                                               {
                                                   id = a.id,
                                                   collegeId = a.collegeId,
                                                   memberDesignation = a.memberDesignation,
                                                   memberName = a.memberName,
                                                   designationName = d.Designation
                                               }).OrderBy(r => r.memberName).ToList();

                int rtiDetailscount = rtiDetails.Count();
                if (rtiDetails != null)
                {
                    foreach (var item in rtiDetails)
                    {
                        rtiSNo++;
                        strRTIDetails += "<tr><td width='57'><p align='center'>" + rtiSNo + "</p></td><td width='430' align='left'>" + item.memberName + "</td><td width='204' align='left'>" + item.designationName + "</td></tr>";
                    }
                }
                strRTIDetails += "</tbody></table>";
                strRTIDetails += "<p><br />Total Complaints Received: " + rtiDetailscount + "</u> (Please specify 5 major complaints briefly)</p><br>";

                #endregion

                #region RTIComplaints
                strRTIDetails += "<table border='1' cellspacing='0' cellpadding='4'><tbody>";
                strRTIDetails += " <tr><td width='57'><p align='center'>S.No.</p></td><td width='384'><p align='left'>Complaint</p></td><td width='250' align='left' align='left'><p align='left'>Action Taken</p></td></tr>";


                List<RTIComplaints> rtiComplaints = db.jntuh_college_rti_complaints.Where(a => a.collegeId == collegeId).Select(a =>
                                                  new RTIComplaints
                                                  {
                                                      id = a.id,
                                                      collegeId = a.collegeId,
                                                      complaintReceived = a.complaintReceived,
                                                      actionsTaken = a.actionsTaken,
                                                  }).OrderBy(r => r.actionsTaken).ToList();

                if (rtiComplaints.Count() == 0 || rtiComplaints == null)
                {
                    strRTIDetails += "<tr><td colspan='3'><p align='center'>NIL</p></td></tr>";
                }
                else
                {
                    foreach (var item in rtiComplaints)
                    {
                        rtiComplaintsSNo++;
                        strRTIDetails += " <tr><td width='57'><p align='center'>" + rtiComplaintsSNo + "</p></td><td width='384' align='left'>" + item.complaintReceived + "</td><td width='250' align='left'>" + item.actionsTaken + "</td></tr>";
                    }
                }
                strRTIDetails += "</tbody></table>";
                #endregion
                contents = contents.Replace("##RTIDetails##", strRTIDetails);
                return contents;
            }
        }

        public string OtherDesirablesDetails(int collegeId, string contents)
        {
            using (var db = new uaaasDBContext())
            {
                string strOtherDesirables = string.Empty;
                #region SportsFacilities
                CollegePlayground collegePlayground = new CollegePlayground();
                jntuh_college_desirable_others jntuh_college_desirable_others = db.jntuh_college_desirable_others.Where(a => a.collegeId == collegeId).Select(a => a).FirstOrDefault(); ;

                strOtherDesirables += "<p style='font-size: 9px;'><strong><u>31. Sports & Games</u> :</strong></p><br />";
                strOtherDesirables += "<table border='0' cellpadding='0' cellspacing='0' id='page20' style='font-size: 9px;'>";
                if (jntuh_college_desirable_others != null)
                {
                    strOtherDesirables += "<tr>";
                    strOtherDesirables += "<td align='left' valign='top' style='font-size: 9px;'>";
                    strOtherDesirables += "<p>Number of Playgrounds :" + jntuh_college_desirable_others.totalPlaygrounds + "</p>";

                    string[] selectedPlayGroundType = jntuh_college_desirable_others.playgroundType.Split('|').ToArray();

                    foreach (var type in playGroundTypes)
                    {
                        string strtype = type.id.ToString();
                        playGroundType.Add(new PlayGroundTypeModel { id = type.id, Name = type.Name, Checked = selectedPlayGroundType.Contains(strtype) ? 1 : 0 });
                    }

                    collegePlayground.GroundTypes = playGroundType;
                    if (playGroundType != null)
                    {
                        strOtherDesirables += "<p style='font-size: 9px;'>Playground(s) Type :";
                        foreach (var item in playGroundType)
                        {
                            if (item.Checked == 1)
                            {
                                strOtherDesirables += "&nbsp;" + item.Name;
                            }
                            // strOtherDesirables += "<img alt='' src='" + serverURL + "/Content/Images/checkbox_" + YesOrNo + ".png' height='10' />&nbsp;" + item.Name;
                        }
                        strOtherDesirables += "</p>";
                    }

                    List<CollegeSports> indoorGames = (from s in db.jntuh_college_sports
                                                       join st in db.jntuh_sports_type on s.sportsTypeId equals st.id
                                                       where (st.isActive == true && s.sportsTypeId == 1 && s.collegeId == collegeId)
                                                       select new CollegeSports
                                                       {
                                                           sportsTypeId = s.sportsTypeId,
                                                           sportsFacility = s.sportsFacility
                                                       }).ToList();
                    if (indoorGames != null)
                    {
                        strOtherDesirables += "<br><p style='font-size: 9px;'>Indoor games/sports : <br />";
                        int indoorId = 0;
                        foreach (var item in indoorGames)
                        {
                            indoorId++;
                            strOtherDesirables += indoorId + "." + item.sportsFacility + "&nbsp;&nbsp;";
                        }
                        strOtherDesirables += "</p>";

                    }
                    List<CollegeSports> outdoorGames = (from s in db.jntuh_college_sports
                                                        join st in db.jntuh_sports_type on s.sportsTypeId equals st.id
                                                        where (st.isActive == true && s.sportsTypeId == 2 && s.collegeId == collegeId)
                                                        select new CollegeSports
                                                        {
                                                            sportsTypeId = s.sportsTypeId,
                                                            sportsFacility = s.sportsFacility
                                                        }).ToList();
                    if (outdoorGames != null)
                    {

                        strOtherDesirables += "<p style='font-size: 9px;'>Outdoor games/sports : <br />";
                        int outdoorId = 0;
                        foreach (var item in outdoorGames)
                        {
                            outdoorId++;
                            strOtherDesirables += outdoorId + "." + item.sportsFacility + "&nbsp;&nbsp;";
                        }
                        strOtherDesirables += "</p>";

                    }

                    string[] selectedTransportType = jntuh_college_desirable_others.modeOfTransport.Split('|').ToArray();

                    foreach (var type in transportMode)
                    {
                        string strtype = type.id.ToString();
                        transportModes.Add(new ModeOfTransportModel { id = type.id, Name = type.Name, Checked = selectedTransportType.Contains(strtype) ? 1 : 0 });
                    }

                    collegePlayground.TransportModes = transportModes;
                    if (transportModes != null)
                    {
                        strOtherDesirables += "<br><p style='font-size: 9px;'>Mode of Transport to reach the Institute :";
                        foreach (var item in transportModes)
                        {

                            //string YesOrNo = "no_b";
                            if (item.Checked == 1)
                            {
                                //YesOrNo = "yes_b";
                                strOtherDesirables += "&nbsp;" + item.Name;
                            }
                            //strOtherDesirables += "<img alt='' src='" + serverURL + "/Content/Images/checkbox_" + YesOrNo + ".png' height='10' />&nbsp;" + item.Name;
                        }
                        strOtherDesirables += "</p>";
                    }
                    strOtherDesirables += "<p style='font-size: 9px;'>&nbsp; &nbsp; Number of buses (own) available in the college : " + jntuh_college_desirable_others.numberOfBus + "</u></p><p style='font-size: 9px;'>&nbsp; &nbsp; Number of other transport vehicles (own) in the college : " + jntuh_college_desirable_others.numberOfOtherVehicles + "</u></p>";

                    string[] selectedPaymentType = jntuh_college_desirable_others.modeOfPayment.Split('|').ToArray();

                    foreach (var type in paymentMode)
                    {
                        string strtype = type.id.ToString();
                        paymentsModes.Add(new ModeOfPaymentModel { id = type.id, Name = type.Name, Checked = selectedPaymentType.Contains(strtype) ? 1 : 0 });
                    }
                    if (paymentsModes != null)
                    {
                        strOtherDesirables += "<br><p style='font-size: 9px;'>Mode of Payment of Salary :";
                        foreach (var item in paymentsModes)
                        {

                            string YesOrNo = "no_b";
                            if (item.Checked == 1)
                            {
                                //YesOrNo = "yes_b";
                                strOtherDesirables += "&nbsp;" + item.Name;
                            }
                            //strOtherDesirables += "<img alt='' src='" + serverURL + "/Content/Images/checkbox_" + YesOrNo + ".png' height='10' />&nbsp;" + item.Name;
                        }
                        strOtherDesirables += "</p><br />";
                    }


                    #region OtherDesirables
                    strOtherDesirables += "<p><strong><u>32. Desirable Requirements</u> :</strong></p><br />";
                    //strOtherDesirables += " <p style='font-size: 9px;'>";
                    //strOtherDesirables += " f) <strong>Other Desirables:</strong></p><br />";
                    strOtherDesirables += " <table border='1' cellspacing='0' cellpadding='4' style='font-size: 9px;'><tbody>";
                    List<OtherDesirableRequirements> otherDesirableRequiremetns = (from d in db.jntuh_college_desirable_requirement
                                                                                   join r in db.jntuh_desirable_requirement_type on d.requirementTypeID equals r.id
                                                                                   where (r.isActive == true && d.collegeId == collegeId && r.isHostelRequirement == false)

                                                                                   select new OtherDesirableRequirements
                                                                                   {
                                                                                       id = d.requirementTypeID,
                                                                                       requirementType = r.requirementType,
                                                                                       isSelected = d.isAvaiable == true ? "true" : "false",
                                                                                       governingBodymeetings = (int)d.governingBodyMeetings
                                                                                   }).ToList();

                    if (otherDesirableRequiremetns != null)
                    {
                        foreach (var item in otherDesirableRequiremetns)
                        {
                            if (item.requirementType == "No. of Governing Body meetings held in the past one academic year")
                            {
                                if (item.governingBodymeetings == 2)
                                {
                                    strOtherDesirables += "<tr><td width='547'><p>" + item.requirementType.Replace("&", "&amp;") + "</p></td><td width='165'><p>One or more</p></td></tr>";
                                }
                                else
                                {
                                    if (item.isSelected == "true")
                                    {
                                        strOtherDesirables += "<tr><td width='547'><p>" + item.requirementType.Replace("&", "&amp;") + "</p></td><td width='165'><p>NIL</p></td></tr>";
                                    }
                                    else
                                    {
                                        strOtherDesirables += "<tr><td width='547'><p>" + item.requirementType.Replace("&", "&amp;") + "</p></td><td width='165'><p>One</p></td></tr>";
                                    }
                                }
                            }
                            else
                            {
                                if (item.isSelected == "true")
                                {
                                    strOtherDesirables += "<tr><td width='547'><p> " + item.requirementType.Replace("&", "&amp;") + "</p></td><td width='165'><p>Yes</p></td></tr>";
                                }
                                else
                                {
                                    strOtherDesirables += "<tr><td width='547'><p>  " + item.requirementType.Replace("&", "&amp;") + "</p></td><td width='165'><p>No</p></td></tr>";
                                }
                            }
                        }
                    }
                    //hotcode one line
                    // strOtherDesirables += "<tr><td width='547'><p>No. of Governing Body meetings held in the past one academic year</p></td><td width='165'><p><img alt='' src='" + serverURL + "/Content/Images/checkbox_no_b.png' height='10' />&nbsp;NIL<img alt='' src='" + serverURL + "/Content/Images/checkbox_no_b.png' height='10' />&nbsp;One</p>&nbsp;<img alt='' src='" + serverURL + "/Content/Images/checkbox_yes_b.png' height='10' />&nbsp;One or more</p></td></tr>";
                    strOtherDesirables += "</tbody></table>";
                    #endregion
                }
                else
                {
                    strOtherDesirables += "<tr><td align='left' valign='top' style='font-size: 9px;'>";
                    //strOtherDesirables += "<p>e) <b>Sports facilities</b> :</p>";
                    strOtherDesirables += "<p style='font-size: 9px;'>Number of Playgrounds :" + string.Empty + "</p>";
                    strOtherDesirables += "<p style='font-size: 9px;'>Playground(s) Type :";

                    strOtherDesirables += "";
                    strOtherDesirables += "</p>";

                    strOtherDesirables += "<br><p style='font-size: 9px;'>Indoor games/sports : <br />";
                    strOtherDesirables += "&nbsp;&nbsp;";
                    strOtherDesirables += "</p>";

                    strOtherDesirables += "<p style='font-size: 9px;'>Outdoor games/sports : <br />";
                    strOtherDesirables += "&nbsp;&nbsp;";
                    strOtherDesirables += "</p>";

                    strOtherDesirables += "<br><p style='font-size: 9px;'>Mode of Transport to reach the Institute :";

                    strOtherDesirables += "";
                    strOtherDesirables += "</p>";

                    strOtherDesirables += "<p style='font-size: 9px;'>&nbsp; &nbsp; Number of buses (own) available in the college : " + string.Empty + "</p><p style='font-size: 9px;'>&nbsp; &nbsp; Number of other transport vehicles (own) in the college : " + string.Empty + "</p>";

                    strOtherDesirables += "<br><p style='font-size: 9px;'>Mode of Payment of Salary :";

                    strOtherDesirables += "";
                    strOtherDesirables += "</p><br />";



                    #region OtherDesirables


                    strOtherDesirables += "<p><strong><u>32. Desirable Requirements</u> :</strong></p><br />";
                    strOtherDesirables += " <p style='font-size: 9px;'>f) <b>Other Desirables</b></p><br /> <table border='1' cellspacing='0' cellpadding='4' style='font-size: 9px;'><tbody>";
                    List<OtherDesirableRequirements> otherDesirableRequiremetns = (from d in db.jntuh_college_desirable_requirement
                                                                                   join r in db.jntuh_desirable_requirement_type on d.requirementTypeID equals r.id
                                                                                   where (r.isActive == true && d.collegeId == collegeId && r.isHostelRequirement == false)
                                                                                   select new OtherDesirableRequirements
                                                                                   {
                                                                                       id = d.requirementTypeID,
                                                                                       requirementType = r.requirementType,
                                                                                       isSelected = d.isAvaiable == true ? "true" : "false",
                                                                                       governingBodymeetings = 0
                                                                                   }).ToList();

                    if (otherDesirableRequiremetns != null)
                    {
                        foreach (var item in otherDesirableRequiremetns)
                        {
                            if (item.requirementType == "No. of Governing Body meetings held in the past one academic year")
                            {
                                strOtherDesirables += "<tr><td width='547'><p>" + item.requirementType + "</p></td><td width='165'>></td></tr>";
                            }
                            else
                            {
                                strOtherDesirables += "<tr><td width='547'><p> " + item.requirementType + "</p></td><td width='165'><p></td></tr>";
                            }
                        }
                    }
                    strOtherDesirables += "</tbody></table>";
                    #endregion
                }

                strOtherDesirables += "</td></tr></table>";
                #endregion
                contents = contents.Replace("##OtherDesirables##", strOtherDesirables);
                return contents;
            }
        }

        public string OtherDesirablesDetailsNewUnderProcess(int collegeId, string contents)
        {
            using (var db = new uaaasDBContext())
            {
                string strOtherDesirables = string.Empty;
                #region SportsFacilities
                CollegePlayground collegePlayground = new CollegePlayground();
                int desirableId = db.jntuh_college_desirable_others.Where(a => a.collegeId == collegeId).Select(a => a.id).FirstOrDefault();
                jntuh_college_desirable_others jntuh_college_desirable_others = db.jntuh_college_desirable_others.Find(desirableId);

                strOtherDesirables += "<p style='font-size: 9px;'><strong><u>30. Sports &amp; Games</u> :</strong></p><br />";
                strOtherDesirables += "<table border='0' cellpadding='0' cellspacing='0' id='page20' style='font-size: 9px;'>";
                if (jntuh_college_desirable_others != null)
                {
                    strOtherDesirables += "<tr>";
                    strOtherDesirables += "<td align='left' colspan=3>Number of Playgrounds :</td>";
                    strOtherDesirables += "<td align='left' colspan=8>" + jntuh_college_desirable_others.totalPlaygrounds + "</td>";
                    strOtherDesirables += "</tr>";

                    string[] selectedPlayGroundType = jntuh_college_desirable_others.playgroundType.Split('|').ToArray();

                    foreach (var type in playGroundTypes)
                    {
                        string strtype = type.id.ToString();
                        playGroundType.Add(new PlayGroundTypeModel { id = type.id, Name = type.Name, Checked = selectedPlayGroundType.Contains(strtype) ? 1 : 0 });
                    }

                    collegePlayground.GroundTypes = playGroundType;
                    if (playGroundType != null)
                    {
                        strOtherDesirables += "<tr>";
                        strOtherDesirables += "<td align='left' colspan=3>Playground(s) Type :</td>";
                        strOtherDesirables += "<td align='left' colspan=8>";
                        foreach (var item in playGroundType)
                        {

                            //string YesOrNo = "no_b";
                            if (item.Checked == 1)
                            {
                                //YesOrNo = "yes_b";
                                strOtherDesirables += "&nbsp;&nbsp;&nbsp;" + item.Name;
                            }
                            //strOtherDesirables += "<img alt='' src='" + serverURL + "/Content/Images/checkbox_" + YesOrNo + ".png' height='10' />&nbsp;&nbsp;&nbsp;" + item.Name;
                        }
                        strOtherDesirables += "</td>";
                        strOtherDesirables += "</tr>";
                    }

                    string[] selectedTransportType = jntuh_college_desirable_others.modeOfTransport.Split('|').ToArray();

                    foreach (var type in transportMode)
                    {
                        string strtype = type.id.ToString();
                        transportModes.Add(new ModeOfTransportModel { id = type.id, Name = type.Name, Checked = selectedTransportType.Contains(strtype) ? 1 : 0 });
                    }

                    collegePlayground.TransportModes = transportModes;
                    if (transportModes != null)
                    {
                        strOtherDesirables += "<tr>";
                        strOtherDesirables += "<td align='left' colspan=3>Mode of Transport to reach the Institute :</td>";
                        strOtherDesirables += "<td align='left' colspan=8>";
                        foreach (var item in transportModes)
                        {

                            //string YesOrNo = "no_b";
                            if (item.Checked == 1)
                            {
                                //   YesOrNo = "yes_b";
                                strOtherDesirables += "&nbsp;" + item.Name;
                            }
                            // strOtherDesirables += "<img alt='' src='" + serverURL + "/Content/Images/checkbox_" + YesOrNo + ".png' height='10' />&nbsp;" + item.Name;
                        }
                        strOtherDesirables += "</td>";
                        strOtherDesirables += "</tr>";
                    }
                    strOtherDesirables += "<tr>";
                    strOtherDesirables += "<td align='left' colspan=3>Number of buses (own) available in the college : </td>";
                    strOtherDesirables += "<td align='left' colspan=8>" + jntuh_college_desirable_others.numberOfBus + "</td>";
                    strOtherDesirables += "</tr>";

                    strOtherDesirables += "<tr>";
                    strOtherDesirables += "<td align='left' colspan=3>Number of other transport vehicles (own) in the college : </td>";
                    strOtherDesirables += "<td align='left' colspan=8>" + jntuh_college_desirable_others.numberOfOtherVehicles + "</td>";
                    strOtherDesirables += "</tr>";

                    string[] selectedPaymentType = jntuh_college_desirable_others.modeOfPayment.Split('|').ToArray();

                    foreach (var type in paymentMode)
                    {
                        string strtype = type.id.ToString();
                        paymentsModes.Add(new ModeOfPaymentModel { id = type.id, Name = type.Name, Checked = selectedPaymentType.Contains(strtype) ? 1 : 0 });
                    }
                    if (paymentsModes != null)
                    {
                        strOtherDesirables += "<tr>";
                        strOtherDesirables += "<td align='left' colspan=3>Mode of Payment of Salary :</td>";
                        strOtherDesirables += "<td align='left' colspan=8>";
                        foreach (var item in paymentsModes)
                        {
                            //string YesOrNo = "no_b";
                            if (item.Checked == 1)
                            {
                                //YesOrNo = "yes_b";
                                strOtherDesirables += "&nbsp;" + item.Name;
                            }
                            // strOtherDesirables += "<img alt='' src='" + serverURL + "/Content/Images/checkbox_" + YesOrNo + ".png' height='10' />&nbsp;" + item.Name;
                        }
                        strOtherDesirables += "</td>";
                        strOtherDesirables += "</tr>";
                    }

                    List<CollegeSports> indoorGames = (from s in db.jntuh_college_sports
                                                       join st in db.jntuh_sports_type on s.sportsTypeId equals st.id
                                                       where (st.isActive == true && s.sportsTypeId == 1 && s.collegeId == collegeId)
                                                       select new CollegeSports
                                                       {
                                                           sportsTypeId = s.sportsTypeId,
                                                           sportsFacility = s.sportsFacility
                                                       }).ToList();
                    if (indoorGames != null)
                    {
                        strOtherDesirables += "<br><p style='font-size: 9px;'>Indoor games/sports : <br />";
                        int indoorId = 0;
                        foreach (var item in indoorGames)
                        {
                            indoorId++;
                            strOtherDesirables += indoorId + "." + item.sportsFacility + "&nbsp;&nbsp;";
                        }
                        strOtherDesirables += "</p>";

                    }
                    List<CollegeSports> outdoorGames = (from s in db.jntuh_college_sports
                                                        join st in db.jntuh_sports_type on s.sportsTypeId equals st.id
                                                        where (st.isActive == true && s.sportsTypeId == 2 && s.collegeId == collegeId)
                                                        select new CollegeSports
                                                        {
                                                            sportsTypeId = s.sportsTypeId,
                                                            sportsFacility = s.sportsFacility
                                                        }).ToList();
                    if (outdoorGames != null)
                    {

                        strOtherDesirables += "<p style='font-size: 9px;'>Outdoor games/sports : <br />";
                        int outdoorId = 0;
                        foreach (var item in outdoorGames)
                        {
                            outdoorId++;
                            strOtherDesirables += outdoorId + "." + item.sportsFacility + "&nbsp;&nbsp;";
                        }
                        strOtherDesirables += "</p>";

                    }

                    #region OtherDesirables
                    strOtherDesirables += "<p><strong><u>31. Desirable Requirements</u> :</strong></p><br />";
                    strOtherDesirables += " <p style='font-size: 9px;'>";
                    //strOtherDesirables += " f) <strong>Other Desirables:</strong></p><br />";
                    strOtherDesirables += " <table border='1' cellspacing='0' cellpadding='4' style='font-size: 9px;'><tbody>";
                    List<OtherDesirableRequirements> otherDesirableRequiremetns = (from d in db.jntuh_college_desirable_requirement
                                                                                   join r in db.jntuh_desirable_requirement_type on d.requirementTypeID equals r.id
                                                                                   where (r.isActive == true && d.collegeId == collegeId && r.isHostelRequirement == false)

                                                                                   select new OtherDesirableRequirements
                                                                                   {
                                                                                       id = d.requirementTypeID,
                                                                                       requirementType = r.requirementType,
                                                                                       isSelected = d.isAvaiable == true ? "true" : "false",
                                                                                       governingBodymeetings = (int)d.governingBodyMeetings
                                                                                   }).ToList();

                    if (otherDesirableRequiremetns != null)
                    {
                        foreach (var item in otherDesirableRequiremetns)
                        {
                            if (item.requirementType == "No. of Governing Body meetings held in the past one academic year")
                            {
                                if (item.governingBodymeetings == 2)
                                {
                                    strOtherDesirables += "<tr><td width='547'><p>" + item.requirementType + "</p></td><td width='165'><p>One or more</p></td></tr>";
                                }
                                else
                                {
                                    if (item.isSelected == "true")
                                    {
                                        strOtherDesirables += "<tr><td width='547'><p>" + item.requirementType + "</p></td><td width='165'><p>NIL</p></td></tr>";
                                    }
                                    else
                                    {
                                        strOtherDesirables += "<tr><td width='547'><p>" + item.requirementType + "</p></td><td width='165'><p>One</p></td></tr>";
                                    }
                                }
                            }
                            else
                            {
                                if (item.isSelected == "true")
                                {
                                    strOtherDesirables += "<tr><td width='547'><p> " + item.requirementType + "</p></td><td width='165'><p>Yes</p></td></tr>";
                                }
                                else
                                {
                                    strOtherDesirables += "<tr><td width='547'><p>  " + item.requirementType + "</p></td><td width='165'><p>No</p></td></tr>";
                                }
                            }
                        }
                    }
                    //hotcode one line
                    // strOtherDesirables += "<tr><td width='547'><p>No. of Governing Body meetings held in the past one academic year</p></td><td width='165'><p><img alt='' src='" + serverURL + "/Content/Images/checkbox_no_b.png' height='10' />&nbsp;NIL<img alt='' src='" + serverURL + "/Content/Images/checkbox_no_b.png' height='10' />&nbsp;One</p>&nbsp;<img alt='' src='" + serverURL + "/Content/Images/checkbox_yes_b.png' height='10' />&nbsp;One or more</p></td></tr>";
                    strOtherDesirables += "</tbody></table>";
                    #endregion
                }
                else
                {
                    strOtherDesirables += "<tr><td align='left' valign='top' style='font-size: 9px;'><br /><p>e) <b>Sports facilities</b> :</p><p style='font-size: 9px;'>Number of Playgrounds :" + string.Empty + "</u></p>";
                    strOtherDesirables += "<p style='font-size: 9px;'>Playground(s) Type :";

                    strOtherDesirables += "";
                    strOtherDesirables += "</p>";

                    strOtherDesirables += "<br><p style='font-size: 9px;'>Indoor games/sports : <br />";
                    strOtherDesirables += "&nbsp;&nbsp;";
                    strOtherDesirables += "</p>";

                    strOtherDesirables += "<p style='font-size: 9px;'>Outdoor games/sports : <br />";
                    strOtherDesirables += "&nbsp;&nbsp;";
                    strOtherDesirables += "</p>";

                    strOtherDesirables += "<br><p style='font-size: 9px;'>Mode of Transport to reach the Institute :";

                    strOtherDesirables += "</p>";

                    strOtherDesirables += "<p style='font-size: 9px;'>&nbsp; &nbsp; Number of buses (own) available in the college : " + string.Empty + "</u></p><p style='font-size: 9px;'>&nbsp; &nbsp; Number of other transport vehicles (own) in the college : " + string.Empty + "</u></p>";

                    strOtherDesirables += "<br><p style='font-size: 9px;'>Mode of Payment of Salary :";

                    strOtherDesirables += "</p><br />";

                    #region OtherDesirables


                    strOtherDesirables += "<p><strong><u>31. Desirable Requirements</u> :</strong></p><br />";
                    strOtherDesirables += " <p style='font-size: 9px;'>f) <b>Other Desirables</b></p><br /> <table border='1' cellspacing='0' cellpadding='4' style='font-size: 9px;'><tbody>";
                    List<OtherDesirableRequirements> otherDesirableRequiremetns = (from d in db.jntuh_college_desirable_requirement
                                                                                   join r in db.jntuh_desirable_requirement_type on d.requirementTypeID equals r.id
                                                                                   where (r.isActive == true && d.collegeId == collegeId && r.isHostelRequirement == false)
                                                                                   select new OtherDesirableRequirements
                                                                                   {
                                                                                       id = d.requirementTypeID,
                                                                                       requirementType = r.requirementType,
                                                                                       isSelected = d.isAvaiable == true ? "true" : "false",
                                                                                       governingBodymeetings = 0
                                                                                   }).ToList();

                    if (otherDesirableRequiremetns != null)
                    {
                        foreach (var item in otherDesirableRequiremetns)
                        {
                            if (item.requirementType == "No. of Governing Body meetings held in the past one academic year")
                            {
                                strOtherDesirables += "<tr><td width='547'><p>" + item.requirementType + "</p></td><td width='165'></td></tr>";
                            }
                            else
                            {
                                strOtherDesirables += "<tr><td width='547'><p> " + item.requirementType + "</p></td><td width='165'></td></tr>";
                            }
                        }
                    }
                    strOtherDesirables += "</tbody></table>";
                    #endregion
                }

                strOtherDesirables += "</td></tr></table>";
                #endregion
                contents = contents.Replace("##OtherDesirables##", strOtherDesirables);
                return contents;
            }
        }

        public string CampusHostelMaintenanceDetails(int collegeId, string contents)
        {
            using (var db = new uaaasDBContext())
            {
                string strCampusHostelMaintenance = string.Empty;
                strCampusHostelMaintenance += "<table border='1' cellspacing='0' cellpadding='3'  style='font-size: 9px;'><tbody>";
                int hmcount = db.jntuh_college_hostel_maintenance.Where(c => c.collegeId == collegeId).Select(c => c.collegeId).Count();
                List<HostelRequirements> hostelRequiremetns = (from d in db.jntuh_college_hostel_maintenance
                                                               join r in db.jntuh_desirable_requirement_type on d.requirementTypeID equals r.id
                                                               where (r.isActive == true && d.collegeId == collegeId && r.isHostelRequirement == true)
                                                               select new HostelRequirements
                                                               {
                                                                   id = d.requirementTypeID,
                                                                   requirementType = r.requirementType,
                                                                   isSelected = d.isAvaiable == true ? "true" : "false"
                                                               }).ToList();

                if (hostelRequiremetns != null)
                {
                    foreach (var item in hostelRequiremetns)
                    {
                        if (hmcount != 0)
                        {
                            if (item.isSelected == "true")
                            {
                                strCampusHostelMaintenance += "<tr><td width='531'><p>" + item.requirementType.Replace("&", "&amp;") + "</p></td><td width='160'><p>Yes</p></td></tr>";
                            }
                            else
                            {
                                strCampusHostelMaintenance += "<tr><td width='531'><p>" + item.requirementType.Replace("&", "&amp;") + "</p></td><td width='160'><p>No</p></td></tr>";
                            }
                        }
                        else
                        {
                            strCampusHostelMaintenance += "<tr><td width='531'><p>" + item.requirementType.Replace("&", "&amp;") + "</p></td><td width='160'>&nbsp;</td></tr>";
                        }
                    }
                }
                strCampusHostelMaintenance += "</tbody></table>";
                contents = contents.Replace("##CampusHostelMaintenance##", strCampusHostelMaintenance);
                return contents;
            }
        }

        public string OperationalFundsDetails(int collegeId, string contents)
        {
            using (var db = new uaaasDBContext())
            {
                string strOperationalFunds = string.Empty;
                int sno = 0;
                decimal total = 0;
                strOperationalFunds += "<table border='1' cellspacing='0' cellpadding='3' style='font-size: 9px;'><tbody>";
                strOperationalFunds += "<tr><td width='33'><p align='center'> S.</p><p align='center'>No</p></td><td width='96'><p align='left'>Name of the Bank</p>  </td><td width='168'><p align='left'>Branch &amp;</p><p align='left'>Address of the Bank</p> </td> <td width='108'> <p align='center'> Account No.</p></td><td width='96'><p align='center'>Cash Balance </p> <p align='center'>(Rs. in Lakhs) </p> </td> <td width='96'><p align='center'>FDR, if any (Excluding FDR submitted to AICTE)</p> </td><td width='94'><p align='center'>Total amount (Rs)</p><p align='center'>(in Lakhs)</p></td></tr>";
                List<jntuh_college_funds> jntuh_college_funds = db.jntuh_college_funds.Where(f => f.collegeId == collegeId).ToList();
                if (jntuh_college_funds != null)
                {
                    foreach (var item in jntuh_college_funds)
                    {
                        sno++;
                        total += Convert.ToDecimal(item.cashBalance) + Convert.ToDecimal(item.FDR);
                        strOperationalFunds += "<tr><td width='33' valign='top'><p align='center'>" + sno + "</p></td><td width='96' align='left'>" + item.bankName + "<br /><br /></td><td width='168' valign='top' align='left'>" + item.bankBranch + "</td><td width='108' align='center'>" + item.bankAccountNumber + "</td><td width='96' align='right'>" + item.cashBalance + "</td><td width='96' align='right'>" + item.FDR + "</td><td width='94' align='right'>" + total + "</td></tr>";
                    }
                }
                strOperationalFunds += " </tbody></table>";
                contents = contents.Replace("##OperationalFunds##", strOperationalFunds);
                return contents;
            }

        }

        public string IncomeDetails(int collegeId, string contents)
        {
            using (var db = new uaaasDBContext())
            {
                string strIncomeDetails = string.Empty;
                int sno = 0;
                decimal amount = 0;
                strIncomeDetails += "<table border='1' cellspacing='0' cellpadding='3'><tbody>";
                strIncomeDetails += "<tr><td width='84'>  <p align='center'>S.No.</p> </td><td width='446'> <p align='left'>Source of Income</p></td><td width='160'> <p align='right'>Rupees in Lakhs</p></td></tr>";
                List<CollegeIncome> incomeType = db.jntuh_college_income_type.Where(income => income.isActive == true)
                                                                            .Select(income => new CollegeIncome
                                                                            {
                                                                                incomeTypeID = income.id,
                                                                                incomeType = income.sourceOfIncome,
                                                                                incomeAmount = 0,
                                                                            }).ToList();
                if (incomeType != null)
                {
                    var incomeTypeIDs = incomeType.Select(it1 => it1.incomeTypeID).ToList();
                    var jntuh_college_incomes = db.jntuh_college_income.Where(it => it.collegeId == collegeId && incomeTypeIDs.Contains(it.incomeTypeID))
                            .Select(it => new
                            {
                                it.incomeTypeID,
                                it.incomeAmount
                            }).ToList();
                    foreach (var item in incomeType)
                    {
                        sno++;
                        item.incomeAmount = jntuh_college_incomes.Where(collegeIncomeType =>
                                                                        collegeIncomeType.incomeTypeID == item.incomeTypeID)
                                                                 .Select(collegeIncomeType => collegeIncomeType.incomeAmount).FirstOrDefault();

                        amount += item.incomeAmount;
                        strIncomeDetails += "<tr><td width='84'><p align='center'>" + sno + "</p></td><td width='446'><p align='left'>" + item.incomeType + "</p></td><td width='160' align='right'>" + item.incomeAmount + "</td></tr>";
                    }
                    strIncomeDetails += "<tr><td width='84'><p align='center'></p></td><td width='446'><p align='right'> TOTAL   </p> </td> <td width='160'> <p align='right'> " + amount + "</p> </td></tr>";
                }
                strIncomeDetails += " </tbody></table>";
                contents = contents.Replace("##IncomeDetails##", strIncomeDetails);
                return contents;
            }
        }

        public string ExpenditureDetails(int collegeId, string contents)
        {
            using (var db = new uaaasDBContext())
            {
                string expenditureDetails = string.Empty;
                expenditureDetails += " <table border='1' cellspacing='0' cellpadding='3'><tbody> <tr> <td width='84'>  <p align='center'>S.No.</p> </td><td width='446'> <p>Expenditure  </p>  </td><td width='160'> <p align='center'>Rupees in Lakhs</p></td></tr>";
                int sno = 0;

                List<CollegeExpenditure> expenditure = db.jntuh_college_expenditure_type.Where(expenditureType => expenditureType.isActive == true)
                                                         .Select(expenditureType => new CollegeExpenditure
                                                         {
                                                             expenditureTypeID = expenditureType.id,
                                                             expenditure = expenditureType.expenditure,
                                                             expenditureAmount = 0
                                                         }).ToList();
                decimal amount = 0;
                if (expenditure != null)
                {
                    var expenditureTypeIDs = expenditure.Select(it1 => it1.expenditureTypeID).ToList();
                    var jntuh_college_expenditures = db.jntuh_college_expenditure
                        .Where(it => it.collegeId == collegeId && expenditureTypeIDs.Contains(it.expenditureTypeID))
                        .Select(it => new
                        {
                            it.expenditureTypeID,
                            it.expenditureAmount
                        }).ToList();
                    foreach (var item in expenditure)
                    {
                        sno++;
                        item.expenditureAmount = jntuh_college_expenditures.Where(e => e.expenditureTypeID == item.expenditureTypeID)
                            .Select(e => e.expenditureAmount).FirstOrDefault();

                        string ccexpenditure;
                        if (item.expenditureTypeID == 8)
                        {
                            ccexpenditure = item.expenditureAmount == 6 ? "6th PayScale" : (item.expenditureAmount == 7 ? "7th PayScale" : "NA");
                        }
                        else
                        {
                            amount += item.expenditureAmount;
                            ccexpenditure = item.expenditureAmount.ToString(CultureInfo.InvariantCulture);
                        }
                        expenditureDetails += " <tr><td width='84'><p align='center'>" + sno + "</p> </td><td width='446'><p>" + item.expenditure + "</p> </td><td width='160' align='right'>" + ccexpenditure + "</td></tr>";
                    }
                    expenditureDetails += "<tr><td width='84'><p align='center'></p></td><td width='446'><p align='right'> TOTAL   </p> </td> <td width='160'> <p align='right'> " + amount + "</p> </td></tr>";
                }
                expenditureDetails += " </tbody></table>";
                contents = contents.Replace("##ExpenditureDetails##", expenditureDetails);
                return contents;
            }
        }

        private string StudentsPlacementDetails(int collegeId, string contents)//, List<int> specializationIds, int[] departmentIds, List<int> degreeIds
        {
            using (var db = new uaaasDBContext())
            {
                string strCollegePlacement = string.Empty;
                int count = 1;
                decimal? totalpassed1 = 0;
                decimal? totalplaced1 = 0;
                decimal? totalpassed2 = 0;
                decimal? totalplaced2 = 0;
                decimal? totalpassed3 = 0;
                decimal? totalplaced3 = 0;
                IQueryable<jntuh_academic_year> jntuh_academic_year = db.jntuh_academic_year.Select(e => e);
                decimal actualYear = jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();

                contents = contents.Replace("##COLLEGESTUDENTSPLACEMENTSACADEMICYEARTHREE##", String.Format("{0}-{1}", (actualYear).ToString(), (actualYear + 1).ToString().Substring(2, 2)));
                contents = contents.Replace("##COLLEGESTUDENTSPLACEMENTSACADEMICYEARTWO##", String.Format("{0}-{1}", (actualYear - 1).ToString(), (actualYear).ToString().Substring(2, 2)));
                contents = contents.Replace("##COLLEGESTUDENTSPLACEMENTSACADEMICYEARONE##", String.Format("{0}-{1}", (actualYear - 2).ToString(), (actualYear - 1).ToString().Substring(2, 2)));

                int presentYear = jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
                int AY1 = jntuh_academic_year.Where(a => a.actualYear == presentYear).Select(a => a.id).FirstOrDefault();
                int AY2 = jntuh_academic_year.Where(a => a.actualYear == (presentYear - 1)).Select(a => a.id).FirstOrDefault();
                int AY3 = jntuh_academic_year.Where(a => a.actualYear == (presentYear - 2)).Select(a => a.id).FirstOrDefault();

                List<jntuh_college_placement> placements = db.jntuh_college_placement.Where(i => i.collegeId == collegeId).ToList();

                List<CollegePlacement> collegePlacement = new List<CollegePlacement>();
                var specializationIds = placements.Select(it => it.specializationId).ToList();
                var jntuh_specializations = db.jntuh_specialization.Where(s => specializationIds.Contains(s.id))
               .Select(it => new { it.id, it.specializationName, it.departmentId }).ToList();
                var departmentIds = jntuh_specializations.Select(it => it.departmentId).ToList();
                var jntuh_departments = db.jntuh_department.Where(d => departmentIds.Contains(d.id))
                    .Select(it => new { it.id, it.departmentName, it.degreeId }).ToList();
                var degreeIds = jntuh_departments.Select(it1 => it1.degreeId).ToList();
                var jntuh_degrees = db.jntuh_degree.Where(it => degreeIds.Contains(it.id))
                    .Select(it => new { it.id, it.degree, it.degreeDisplayOrder }).ToList();
                // var jntuh_shifts = db.jntuh_shift.Where(it => placements.Select(p => p.shiftId).Contains(it.id)).Select(it => new { it.id, it.shiftName }).ToList();
                foreach (var item in placements)
                {
                    CollegePlacement newPlacement = new CollegePlacement();
                    newPlacement.id = item.id;
                    newPlacement.collegeId = item.collegeId;
                    newPlacement.academicYearId = item.academicYearId;
                    newPlacement.specializationId = item.specializationId;
                    newPlacement.specialization = jntuh_specializations.Where(s => s.id == item.specializationId).Select(s => s.specializationName).FirstOrDefault();
                    newPlacement.departmentID = jntuh_specializations.Where(s => s.id == item.specializationId).Select(s => s.departmentId).FirstOrDefault();
                    newPlacement.department = jntuh_departments.Where(d => d.id == newPlacement.departmentID).Select(d => d.departmentName).FirstOrDefault();
                    newPlacement.degreeID = jntuh_departments.Where(d => d.id == newPlacement.departmentID).Select(d => d.degreeId).FirstOrDefault();
                    newPlacement.degree = jntuh_degrees.Where(d => d.id == newPlacement.degreeID).Select(d => d.degree).FirstOrDefault();
                    newPlacement.degreeDisplayOrder = jntuh_degrees.Where(d => d.id == newPlacement.degreeID).Select(d => d.degreeDisplayOrder).FirstOrDefault();
                    collegePlacement.Add(newPlacement);
                }

                collegePlacement = collegePlacement.AsEnumerable().GroupBy(p => p.specializationId).Select(p => p.First()).ToList();
                collegePlacement = collegePlacement.OrderBy(ei => ei.degreeDisplayOrder).ThenBy(ei => ei.department).ThenBy(ei => ei.specialization).ToList();
                var specializationIds1 = collegePlacement.Select(it1 => it1.specializationId).ToList();

                var jntuh_college_placements = placements.Where(it => specializationIds1.Contains(it.specializationId)).ToList();

                foreach (var item in collegePlacement)
                {
                    item.totalStudentsPassed1 = GetStudents(jntuh_college_placements, collegeId, AY1, item.specializationId, 1);
                    item.totalStudentsPlaced1 = GetStudents(jntuh_college_placements, collegeId, AY1, item.specializationId, 0);

                    item.totalStudentsPassed2 = GetStudents(jntuh_college_placements, collegeId, AY2, item.specializationId, 1);
                    item.totalStudentsPlaced2 = GetStudents(jntuh_college_placements, collegeId, AY2, item.specializationId, 0);

                    item.totalStudentsPassed3 = GetStudents(jntuh_college_placements, collegeId, AY3, item.specializationId, 1);
                    item.totalStudentsPlaced3 = GetStudents(jntuh_college_placements, collegeId, AY3, item.specializationId, 0);
                }
                collegePlacement = collegePlacement.OrderBy(p => p.degree).ToList();
                if (collegePlacement.Count() > 0)
                {

                    foreach (var item in collegePlacement)
                    {
                        item.totalStudentsPassed1 = GetStudents(jntuh_college_placements, collegeId, AY1, item.specializationId, 1);
                        item.totalStudentsPlaced1 = GetStudents(jntuh_college_placements, collegeId, AY1, item.specializationId, 0);

                        item.totalStudentsPassed2 = GetStudents(jntuh_college_placements, collegeId, AY2, item.specializationId, 1);
                        item.totalStudentsPlaced2 = GetStudents(collegeId, AY2, item.specializationId, 0);

                        item.totalStudentsPassed3 = GetStudents(jntuh_college_placements, collegeId, AY3, item.specializationId, 1);
                        item.totalStudentsPlaced3 = GetStudents(jntuh_college_placements, collegeId, AY3, item.specializationId, 0);
                        totalpassed1 += item.totalStudentsPassed1;
                        totalplaced1 += item.totalStudentsPlaced1;
                        totalpassed2 += item.totalStudentsPassed2;
                        totalplaced2 += item.totalStudentsPlaced2;
                        totalpassed3 += item.totalStudentsPassed3;
                        totalplaced3 += item.totalStudentsPlaced3;

                        strCollegePlacement += "<td colspan='1'><p align='center'>" + count + "</p></td>";
                        strCollegePlacement += "<td colspan='2' style='font-size:9px'><p>" + item.degree + "</p></td>";
                        strCollegePlacement += "<td colspan='2' style='font-size:9px'><p>" + item.department + "</p></td>";
                        strCollegePlacement += "<td colspan='3' style='font-size:9px'><p>" + item.specialization + "</p></td>";
                        strCollegePlacement += "<td align='center'><p>" + item.totalStudentsPassed3 + "</p></td>";
                        strCollegePlacement += "<td align='center'><p>" + item.totalStudentsPlaced3 + "</p></td>";
                        strCollegePlacement += "<td align='center'><p>" + item.totalStudentsPassed2 + "</p></td>";
                        strCollegePlacement += "<td align='center'><p>" + item.totalStudentsPlaced2 + "</p></td>";
                        strCollegePlacement += "<td align='center'><p>" + item.totalStudentsPassed1 + "</p></td>";
                        strCollegePlacement += "<td align='center'><p>" + item.totalStudentsPlaced1 + "</p></td>";
                        count++;
                    }
                }
                contents = contents.Replace("##COLLEGESTUDENTSPLACEMENTSINFORMATION##", strCollegePlacement);
                contents = contents.Replace("##COLLEGESTUDENTSPLACEMENTSINFORMATIONTOTALPASSED1##", totalpassed1.ToString());
                contents = contents.Replace("##COLLEGESTUDENTSPLACEMENTSINFORMATIONTOTALPLACED1##", totalplaced1.ToString());
                contents = contents.Replace("##COLLEGESTUDENTSPLACEMENTSINFORMATIONTOTALPASSED2##", totalpassed2.ToString());
                contents = contents.Replace("##COLLEGESTUDENTSPLACEMENTSINFORMATIONTOTALPLACED2##", totalplaced2.ToString());
                contents = contents.Replace("##COLLEGESTUDENTSPLACEMENTSINFORMATIONTOTALPASSED3##", totalpassed3.ToString());
                contents = contents.Replace("##COLLEGESTUDENTSPLACEMENTSINFORMATIONTOTALPLACED3##", totalplaced3.ToString());
                return contents;
            }
        }

        private int? GetStudents(int collegeId, int academicYearId, int specializationId, int flag)
        {
            using (var db = new uaaasDBContext())
            {
                int? student = 0;

                if (flag == 1)
                    student = db.jntuh_college_placement.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId).Select(i => i.totalStudentsPassed == null ? 0 : i.totalStudentsPassed.Value).FirstOrDefault();
                else
                    student = db.jntuh_college_placement.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId).Select(i => i.totalStudentsPlaced == null ? 0 : i.totalStudentsPlaced.Value).FirstOrDefault();
                return student == null ? (int?)null : Convert.ToInt32(student);
            }
        }

        private int? GetStudents(List<jntuh_college_placement> placements, int collegeId, int academicYearId, int specializationId, int flag)
        {
            int? student = 0;

            if (flag == 1)
                student = placements.Where(i => i.academicYearId == academicYearId && i.specializationId == specializationId).Select(i => i.totalStudentsPassed == null ? 0 : i.totalStudentsPassed.Value).FirstOrDefault();
            else
                student = placements.Where(i => i.academicYearId == academicYearId && i.specializationId == specializationId).Select(i => i.totalStudentsPlaced == null ? 0 : i.totalStudentsPlaced.Value).FirstOrDefault();
            return student == null ? (int?)null : Convert.ToInt32(student);

        }


        public string CollegePhotosDetails(int collegeId, string contents)
        {
            using (var db = new uaaasDBContext())
            {
                string strCollegePhotosDetails = string.Empty;
                int sno = 0;
                string strScannedCopy = string.Empty;
                strCollegePhotosDetails += "<table border='1' cellspacing='0' cellpadding='5'><tbody>";
                strCollegePhotosDetails += "<tr><td width='80' colspan='1'><p align='center'>S.No</p></td><td width='200' colspan='8'><p align='center'>Name of Photo</p></td><td width='100' colspan='5'><p align='center'>Photo</p></td></tr>";
                IEnumerable<CollegeDocuments> collegeDocuments = db.jntuh_college_document.Where(a => a.collegeId == collegeId)
                                                                     .Select(a => new CollegeDocuments
                                                                     {
                                                                         id = a.id,
                                                                         collegeId = collegeId,
                                                                         documentId = a.documentId,
                                                                         documentName = db.jntuh_documents_required.Where(d => d.id == a.documentId).Select(d => d.documentName).FirstOrDefault(),
                                                                         scannedCopy = a.scannedCopy
                                                                     }).OrderBy(d => d.documentId).ToList();

                if (collegeDocuments != null)
                {
                    foreach (var item in collegeDocuments)
                    {
                        sno++;
                        if (item.scannedCopy != null)
                        {
                            strScannedCopy = item.scannedCopy.Replace("~", "");
                        }
                        else
                        {
                            strScannedCopy = string.Empty;
                        }
                        string CollegePhotosParsing = string.Empty;
                        string path = "http://jntuhaac.in" + strScannedCopy.Trim();
                        // string path = @"~" + strScannedCopy;
                        //  path = System.Web.HttpContext.Current.Server.MapPath(path);

                        if (!string.IsNullOrEmpty(path))
                        {
                            #region With-Out Html Parsing
                            try
                            {
                                if (!string.IsNullOrEmpty(path))
                                // if (System.IO.File.Exists(path))
                                {

                                    CollegePhotosParsing += "<p align='center'><img src='" + HtmlEncoder.Encode(path.Trim()) + "' align='center'  width='40' height='45' /></p>";
                                    var ParseEliments = HTMLWorker.ParseToList(new StringReader(CollegePhotosParsing), null);

                                    if (path.Contains("."))
                                    {
                                        strCollegePhotosDetails += "<tr><td width='80' colspan='1'><p align='center'>" + sno + "</p></td><td width='200' colspan='8'><p align='left'>" + item.documentName + "</p></td><td width='100' align='center' colspan='5'><p align='center'><img alt='' src=" + path + " align='center'  width='80' height='50' /></p></td></tr>";
                                    }
                                    else
                                        strCollegePhotosDetails += "<tr><td width='80' colspan='1'><p align='center'>" + sno + "</p></td><td width='200' colspan='8'><p align='left'>" + item.documentName + "</p></td><td width='100' align='center' colspan='5'><p align='center'></p></td></tr>";

                                }
                            }
                            catch (Exception ex)
                            {
                                strCollegePhotosDetails += "<tr><td width='80' colspan='1'><p align='center'>" + sno + "</p></td><td width='200' colspan='8'><p align='left'>" + item.documentName + "</p></td><td width='100' align='center' colspan='5'><p align='center'></p></td></tr>";
                                continue;
                            }
                            #endregion

                        }
                        else
                        {
                            if (test415PDF.Equals("YES"))
                            {
                                // strCollegePhotosDetails += "<tr><td width='80' colspan='1'><p align='center'>" + sno + "</p></td><td width='200' colspan='8'><p align='left'>" + item.documentName + "</p></td><td width='100' align='center' colspan='5'><p align='center'><img alt='' src=" + "http://112.133.193.228:75" + "" + strScannedCopy + " align='center'  width='80' height='50' /></p></td></tr>";
                                strCollegePhotosDetails += "<tr><td width='80' colspan='1'><p align='center'>" + sno + "</p></td><td width='200' colspan='8'><p align='left'>" + item.documentName + "</p></td><td width='100' align='center' colspan='5'><p align='center'>&nbsp;</p></td></tr>";
                            }
                            else
                            {
                                strCollegePhotosDetails += "<tr><td width='80' colspan='1'><p align='center'>" + sno + "</p></td><td width='200' colspan='8'><p align='left'>" + item.documentName + "</p></td><td width='100' align='center' colspan='5'><p align='center'>&nbsp;</p></td></tr>";
                            }
                        }

                    }
                }

                strCollegePhotosDetails += "</tbody></table>";
                contents = contents.Replace("##CollegePhotosDetails##", strCollegePhotosDetails);
                // List<IElement> parsedHtmlElements = HTMLWorker.ParseToList(new StringReader(contents), null);
                return contents;
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

        private string collegeEnclosures(int collegeId, string contents)
        {
            using (var db = new uaaasDBContext())
            {
                string collegeEnclosures = string.Empty;

                var enclosures = db.jntuh_enclosures.Where(d => d.isActive == true).Select(d => d).ToList();
                //var enclosures = db.jntuh_college_enclosures_hardcopy.Where(d => d.isActive == true && d.collegeID == collegeId).OrderBy(d => d.id).Select(d => d).ToList();
                collegeEnclosures += "<tr>";
                collegeEnclosures += "<td colspan='1'><p align='center'>S.No</p></td>";
                collegeEnclosures += "<td colspan='12' align='left'><p>Document Name</p></td>";
                collegeEnclosures += "<td colspan='3'><p align='center'>Uploaded</p></td>";
                collegeEnclosures += "</tr>";

                var jntuh_college_enclosures_hardcopy = db.jntuh_college_enclosures_hardcopy.Where(e => e.collegeID == collegeId).ToList();
                int count = 1;
                foreach (var item in enclosures)
                {

                    collegeEnclosures += "<tr>";
                    collegeEnclosures += "<td colspan='1' align='center'><p align='center'>" + count + "</p></td>";
                    collegeEnclosures += "<td colspan='12' align='left'><p>" + item.documentName + "</p></td>";
                    int encount = jntuh_college_enclosures_hardcopy.Where(e => e.enclosureId == item.id && e.collegeID == collegeId && e.isSelected).Count();

                    if (item.documentName != "Affidavit on Rs 100/- non-judicial stamp paper")
                    {
                        if (encount > 0)
                        {
                            collegeEnclosures += "<td colspan='3' align='center'><p align='center'>Yes</p></td>";
                        }
                        else
                        {
                            collegeEnclosures += "<td colspan='3' align='center'><p align='center'>No&nbsp;</p></td>";
                        }
                    }
                    else
                    {
                        collegeEnclosures += "<td colspan='3' align='center'><p align='center'>&nbsp;</p></td>";
                    }

                    collegeEnclosures += "</tr>";

                    count++;
                }

                contents = contents.Replace("##COLLEGEENCLOSURES##", collegeEnclosures);
                return contents;
            }
        }

        private int GetIntake(int degreeId, int collegeId)
        {
            using (var db = new uaaasDBContext())
            {
                int totalIntake = 0;
                int duration = Convert.ToInt32(db.jntuh_degree.Where(d => d.id == degreeId).Select(d => d.degreeDuration).FirstOrDefault());
                int presentAcademicYearId = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.id).FirstOrDefault();
                int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.id == presentAcademicYearId).Select(a => a.actualYear).FirstOrDefault();
                int AcademicYearId1 = db.jntuh_academic_year.Where(a => a.isActive == true && a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();
                int AcademicYearId2 = db.jntuh_academic_year.Where(a => a.isActive == true && a.actualYear == (actualYear - 1)).Select(a => a.id).FirstOrDefault();
                int AcademicYearId3 = db.jntuh_academic_year.Where(a => a.isActive == true && a.actualYear == (actualYear - 2)).Select(a => a.id).FirstOrDefault();
                int AcademicYearId4 = db.jntuh_academic_year.Where(a => a.isActive == true && a.actualYear == (actualYear - 3)).Select(a => a.id).FirstOrDefault();
                int[] specializationsId = (from d in db.jntuh_college_degree
                                           join de in db.jntuh_department on d.degreeId equals de.degreeId
                                           join s in db.jntuh_specialization on de.id equals s.departmentId
                                           join ProposedIntakeExisting in db.jntuh_college_intake_proposed on s.id equals ProposedIntakeExisting.specializationId
                                           where (d.degreeId == degreeId && d.isActive == true && d.collegeId == collegeId && ProposedIntakeExisting.collegeId == collegeId)
                                           select ProposedIntakeExisting.specializationId).Distinct().ToArray();


                var intakeProposed = db.jntuh_college_intake_proposed.Where(it =>
                    specializationsId.Contains(it.specializationId)
                    && it.collegeId == collegeId
                    && it.academicYearId == AcademicYearId1).Select(it => new
                    {
                        it.shiftId,
                        it.specializationId
                    }).ToList();
                foreach (var specializationId in specializationsId)
                {
                    int totalIntake1 = 0;
                    int totalIntake2 = 0;
                    int totalIntake3 = 0;
                    int totalIntake4 = 0;
                    int totalIntake5 = 0;
                    int[] shiftId1 = intakeProposed.Where(e => e.specializationId == specializationId)
                        .Select(e => e.shiftId).ToArray();
                    var intakesProposed = db.jntuh_college_intake_proposed.Where(it => shiftId1.Contains(it.shiftId)
                        && it.collegeId == collegeId
                        && it.specializationId == specializationId).Select(it => new
                        {
                            it.shiftId,
                            it.academicYearId,
                            it.proposedIntake
                        }).ToList();

                    var intakesExising = db.jntuh_college_intake_existing.Where(it => shiftId1.Contains(it.shiftId)
                      && it.collegeId == collegeId
                      && it.specializationId == specializationId).Select(it => new
                      {
                          it.shiftId,
                          it.academicYearId,
                          it.approvedIntake

                      }).ToList();
                    foreach (var sId1 in shiftId1)
                    {
                        totalIntake1 += intakesProposed.Where(e =>
                            e.academicYearId == AcademicYearId1
                                //&& e.collegeId == collegeId 
                                //&& e.specializationId == specializationId 
                            && e.shiftId == sId1).Select(a => a.proposedIntake).FirstOrDefault();

                        totalIntake2 += intakesExising.Where(e =>
                            e.academicYearId == presentAcademicYearId
                                //&& e.collegeId == collegeId 
                                //&& e.specializationId == specializationId 
                            && e.shiftId == sId1).Select(a => a.approvedIntake).FirstOrDefault();

                        totalIntake3 += intakesExising.Where(e =>
                            e.academicYearId == AcademicYearId2
                                //&& e.collegeId == collegeId 
                                //&& e.specializationId == specializationId 
                            && e.shiftId == sId1).Select(a => a.approvedIntake).FirstOrDefault();

                        totalIntake4 += intakesExising.Where(e =>
                            e.academicYearId == AcademicYearId3
                                //&& e.collegeId == collegeId 
                                //&& e.specializationId == specializationId 
                            && e.shiftId == sId1).Select(a => a.approvedIntake).FirstOrDefault();

                        totalIntake5 += intakesExising.Where(e =>
                            e.academicYearId == AcademicYearId4
                                // && e.collegeId == collegeId 
                                //&& e.specializationId == specializationId 
                            && e.shiftId == sId1).Select(a => a.approvedIntake).FirstOrDefault();
                    }
                    if (duration >= 5)
                    {
                        totalIntake += totalIntake1 + totalIntake2 + totalIntake3 + totalIntake4 + totalIntake5;
                    }
                    if (duration == 4)
                    {
                        totalIntake += totalIntake1 + totalIntake2 + totalIntake3 + totalIntake4;
                    }
                    if (duration == 3)
                    {
                        totalIntake += totalIntake1 + totalIntake2 + totalIntake3;
                    }
                    if (duration == 2)
                    {
                        totalIntake += totalIntake1 + totalIntake2;
                    }
                    if (duration == 1)
                    {
                        totalIntake += totalIntake1;
                    }
                }

                return totalIntake;
            }
        }

        #region New Code With college Teaching faculty logic

        private string collegeTachingFacultyMembers(int collegeId, string contents)
        {
            using (var db = new uaaasDBContext())
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
                int[] DeptIDs = new int[] { };
                //int[] CollegeIds = new int[] { 9, 39, 42, 75, 364 }; // 'WJ','TK','7Q','7Z','D0'
                //int[] pharmacyDeptIds = new int[] { 26, 27, 36, 39 }; // 'WJ','TK','7Q','7Z','D0'
                string ratified = string.Empty;
                List<jntuh_college_faculty_registered> cFaculty = db.jntuh_college_faculty_registered.Where(rf => rf.collegeId == collegeId).ToList();
                string[] strRegNoS = cFaculty.Select(rf => rf.RegistrationNumber).ToArray();

                //if (CollegeIds.Contains(collegeId))
                //{
                //    strRegNoS = cFaculty.Where(i => pharmacyDeptIds.Contains((int)i.DepartmentId)).Select(rf => rf.RegistrationNumber).ToArray();
                //}

                IQueryable<jntuh_designation> jntuh_designation = db.jntuh_designation.Select(e => e);
                IQueryable<jntuh_department> jntuh_department = db.jntuh_department.Select(d => d);
                IQueryable<jntuh_specialization> jntuh_specialization = db.jntuh_specialization.Select(d => d);

                #region TeachingFacultyLogic Begin
                int Facultytype = db.jntuh_faculty_type.Where(t => t.facultyType == "Teaching").Select(t => t.id).FirstOrDefault();

                List<CollegeFaculty> teachingFaculty = new List<CollegeFaculty>();

                List<jntuh_college_faculty_registered> regFaculty = cFaculty;
                var jntuh_registered_facultys = db.jntuh_registered_faculty.Where(s => strRegNoS.Contains(s.RegistrationNumber)).Select(F => F).ToList();

                var facultyids = jntuh_registered_facultys.Select(s => s.id).ToList();

                var jntuh_registered_faculty_educations = db.jntuh_registered_faculty_education.Where(s => facultyids.Contains(s.facultyId) && s.educationId != 8).ToList();


                foreach (var item in regFaculty)
                {
                    CollegeFaculty collegeFacultynew = new CollegeFaculty();
                    jntuh_registered_faculty rFaculty = jntuh_registered_facultys.Where(f => f.RegistrationNumber == item.RegistrationNumber.Trim()).FirstOrDefault();
                    if (rFaculty != null)
                    {
                        collegeFacultynew.id = rFaculty.id;
                        collegeFacultynew.TotalExperience = rFaculty.TotalExperience != null ? rFaculty.TotalExperience : 0;
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
                        collegeFacultynew.facultyDepartmentId = item.DepartmentId != null ? (int)item.DepartmentId : 0;
                        collegeFacultynew.department = jntuh_department.Where(d => d.id == collegeFacultynew.facultyDepartmentId).Select(d => d.departmentName).FirstOrDefault();
                        collegeFacultynew.SpecializationId = item.SpecializationId != null ? (int)item.SpecializationId : 0;
                        collegeFacultynew.SpecializationName = jntuh_specialization.Where(s => s.id == collegeFacultynew.SpecializationId).Select(s => s.specializationName).FirstOrDefault();
                        collegeFacultynew.facultyOtherDepartment = rFaculty.OtherDepartment;
                        collegeFacultynew.grossSalary = rFaculty.grosssalary;
                        collegeFacultynew.facultyEmail = rFaculty.Email;
                        collegeFacultynew.facultyMobile = rFaculty.Mobile;
                        collegeFacultynew.facultyPANNumber = rFaculty.PANNumber;
                        collegeFacultynew.facultyAadhaarNumber = item.AadhaarNumber;
                        collegeFacultynew.FacultyRegistrationNumber = rFaculty.RegistrationNumber.Trim();
                        collegeFacultynew.dateOfAppointment = UAAAS.Models.Utilities.MMDDYY2DDMMYY(rFaculty.DateOfAppointment.ToString());
                        collegeFacultynew.FacultyDeactivationReasion = jntuh_registered_facultys.Where(f => f.RegistrationNumber == collegeFacultynew.FacultyRegistrationNumber).Select(i => i.DeactivationReason).FirstOrDefault();
                        collegeFacultynew.PANDeactivationReasion = rFaculty.PanDeactivationReason;
                        collegeFacultynew.BlackList = (bool)rFaculty.Blacklistfaculy;
                        collegeFacultynew.HighestQualification = jntuh_registered_faculty_educations.OrderByDescending(education => education.educationId)
                                                               .Where(education => education.facultyId == rFaculty.id)
                                                               .Select(education => education.educationId).FirstOrDefault();
                        teachingFaculty.Add(collegeFacultynew);
                    }
                }

                var jntuh_college_faculty_registereds = cFaculty;

                jntuh_college_faculty_registered eFaculty = null;
                var jntuh_designations = jntuh_designation.Select(d => new { d.id, d.designation }).ToList();
                var jntuh_departments = jntuh_department.Select(d => new { d.id, d.departmentName }).ToList();

                #endregion TeachingFacultyLogic End

                var facultyList = teachingFaculty.Where(f => f.facultyType != "Adjunct" && f.BlackList == false).Select(e => e).OrderBy(e => e.DegreeName).ThenBy(e => e.department).ToList();
                var RegNum = facultyList.Where(F => F.FacultyRegistrationNumber == "33150405-143554").Select(F => F);

                int[] HumanitiesDeptIds = { 29, 30, 31, 32, 60, 65, 66, 67, 68, 69, 70, 71, 72, 73, 74, 75, 76, 77, 78 };

                DeptIDs = facultyList.Where(d => !HumanitiesDeptIds.Contains(d.facultyDepartmentId)).Select(d => d.facultyDepartmentId).Distinct().ToArray();

                var SpecBasedDept = facultyList.Select(d => new
                {
                    deptid = d.facultyDepartmentId,
                    specid = d.SpecializationId
                }).Distinct().ToList();

                var noregnos = facultyList.Where(i => i.FacultyRegistrationNumber == null).ToList();

                var type = teachingFaculty.Where(f => f.facultyType == "Adjunct" && f.BlackList == false).ToList();

                var BlackListFaculty = teachingFaculty.Where(e => e.BlackList == true).Select(e => e).ToList();

                if (DeptIDs.Count() > 0)
                {
                    foreach (int deptId in DeptIDs)
                    {
                        foreach (var item7 in SpecBasedDept)
                        {
                            if (deptId == item7.deptid)
                            {
                                //UG Faculty
                                if (item7.specid == 0)
                                    collegeFaculty += TeachingFaculty(facultyList.Where(g => g.facultyDepartmentId == deptId).OrderBy(g => g.facultyFirstName).ToList());
                                //Pg Faculty
                                else
                                    collegeFaculty += TeachingFaculty(facultyList.Where(g => g.facultyDepartmentId == deptId && g.SpecializationId == item7.specid).OrderBy(g => g.facultyFirstName).ToList());
                            }
                            else
                            { }
                        }
                    }
                }

                #region Checking HumanitiesDepartments
                var NewData = facultyList.Where(d => HumanitiesDeptIds.Contains(d.facultyDepartmentId)).Select(d => d).OrderBy(d => d.facultyDepartmentId).ToList();
                DeptIDs = NewData.Select(d => d.facultyDepartmentId).Distinct().ToArray();

                if (DeptIDs.Count() > 0)
                {
                    foreach (int deptId in DeptIDs)
                    {
                        collegeFaculty += TeachingFaculty(facultyList.Where(g => g.facultyDepartmentId == deptId).OrderBy(g => g.facultyFirstName).ToList());
                    }
                }
                #endregion

                DeptIDs = facultyList.Select(d => d.facultyDepartmentId).Distinct().ToArray();
                var blacklistfacultyid = BlackListFaculty.Select(e => e.id).ToList();

                var FacultyRegData = db.jntuh_registered_faculty_education.Where(e => blacklistfacultyid.Contains(e.facultyId) && e.educationId != 8).Select(e => e).ToList();

                if (type.Count() > 0)
                {
                    collegeFaculty += TeachingFaculty(type);
                }

                if (BlackListFaculty.Count() != 0)
                {
                    int blackcount = 1;
                    collegeFaculty += "<strong><p style='font-size: 9px;'><u><b>Blacklisted Faculty:</b></u></p></strong>";
                    collegeFaculty += "<p style='font-size: 9px;'>The following faculty uploaded by the college are blacklisted during Academic Year 2018-19 Affiliation Process and hence will not be considered during this Affiliation Process for the Academic Year 2020-21.</p> <br/>";
                    collegeFaculty += "<table border='1' cellspacing='0' cellpadding='3' style='font-size: 9px;'>";
                    collegeFaculty += "<tbody>";
                    collegeFaculty += "<tr>";
                    collegeFaculty += "<td width='3%'><p align='center'>SNo</p></td>";
                    collegeFaculty += "<td width='25%'><p align='center'>Reg.No.</p></td>";
                    collegeFaculty += "<td width='15%'><p align='left'>Faculty Name</p></td>";
                    collegeFaculty += "<td width='4%'><p align='center'>Gender</p></td>";
                    collegeFaculty += "<td width='4%'><p align='left'>Desg</p></td>";
                    collegeFaculty += "<td width='5%'><p align='left'>Qualification </p></td>";
                    collegeFaculty += "<td width='5%'><p align='left'>Specilization</p></td>";
                    collegeFaculty += "<td width='10%'><p align='left'>Date of Appointment</p></td>";
                    collegeFaculty += "<td width='10%'><p align='center'>Pan Number</p></td>";
                    collegeFaculty += "<td width='10%'><p align='center'>Aadhaar Number</p></td>";
                    collegeFaculty += "<td width='9%'><p align='center'>Photo</p></td>";
                    collegeFaculty += "</tr>";


                    foreach (var blacklistitem in BlackListFaculty)
                    {
                        string Facultyphoto = jntuh_registered_facultys.Where(F => F.RegistrationNumber == blacklistitem.FacultyRegistrationNumber).Select(F => F.Photo).FirstOrDefault();
                        if (blacklistitem.facultyGenderId == 1)
                            gender = "M";
                        else
                            gender = "F";

                        designation = jntuh_designations.Where(d => d.id == blacklistitem.facultyDesignationId).Select(d => d.designation).FirstOrDefault();
                        if (blacklistitem.dateOfAppointment != null)
                        {
                            dateOfAppointment = UAAAS.Models.Utilities.MMDDYY2DDMMYY(blacklistitem.dateOfAppointment.ToString());
                        }

                        qualification = FacultyRegData.Where(e => e.facultyId == blacklistitem.id).OrderByDescending(education => education.educationId).Select(education => education.courseStudied).FirstOrDefault();

                        string strSpecialization = string.Empty;
                        if (FacultyRegData.Count() > 0)
                        {
                            var Specialization = FacultyRegData.Where(e => e.facultyId == blacklistitem.id).OrderByDescending(g => g.passedYear).FirstOrDefault().specialization;
                            strSpecialization = Specialization;
                        }

                        string identifiedfor = cFaculty.Where(f => f.RegistrationNumber == blacklistitem.FacultyRegistrationNumber).Select(f => f.IdentifiedFor).FirstOrDefault();

                        collegeFaculty += "<tr>";
                        collegeFaculty += "<td width='3%' style='font-size: 8px;'><p align='center'>" + blackcount + "</p></td>";
                        collegeFaculty += "<td width='25%' style='font-size: 8px;'><p align='center'>" + blacklistitem.FacultyRegistrationNumber + "</p></td>";
                        collegeFaculty += "<td width='15%' style='font-size: 8px;'><p align='left'>" + (blacklistitem.facultyFirstName + " " + blacklistitem.facultySurname + " " + blacklistitem.facultyLastName).ToUpper() + "</p></td>";
                        collegeFaculty += "<td width='4%' style='font-size: 8px;'><p align='center'>" + gender + "</p></td>";

                        if (!string.IsNullOrEmpty(designation))
                        {
                            if (blacklistitem.facultyDesignationId == 1)
                            {
                                collegeFaculty += "<td width='4%' style='font-size: 10px;'><p align='left'>" + "P" + "</p></td>";
                            }
                            else if (blacklistitem.facultyDesignationId == 2)
                            {
                                collegeFaculty += "<td width='4%' style='font-size: 10px;'><p align='left'>" + "ASP" + "</p></td>";
                            }
                            else if (blacklistitem.facultyDesignationId == 3)
                            {
                                collegeFaculty += "<td width='4%' style='font-size: 10px;'><p align='left'>" + "AP" + "</p></td>";
                            }
                            else
                            {
                                collegeFaculty += "<td width='4%' style='font-size: 8px;'><p align='left'>&nbsp;</p></td>";
                            }
                        }
                        else
                        {
                            collegeFaculty += "<td width='4%' style='font-size: 8px;'><p align='left'>&nbsp;</p></td>";
                        }

                        if (!string.IsNullOrEmpty(qualification))
                        {
                            collegeFaculty += "<td width='4%' style='font-size: 8px;'><p align='left'>" + qualification.ToUpper() + "</p></td>";
                        }
                        else
                        {
                            collegeFaculty += "<td width='4%' style='font-size: 8px;'><p align='left'>&nbsp;</p></td>";
                        }

                        if (!string.IsNullOrEmpty(strSpecialization))
                        {
                            collegeFaculty += "<td width='5%' style='font-size: 8px;'><p align='left'>" + strSpecialization.ToUpper().Replace("&", "&amp;") + "</p></td>";
                        }
                        else
                        {
                            collegeFaculty += "<td width='5%' style='font-size: 8px;'><p align='left'>&nbsp;</p></td>";
                        }

                        collegeFaculty += "<td width='10%' style='font-size: 8px;'><p align='left'>" + dateOfAppointment + "</p></td>";
                        collegeFaculty += "<td width='10%'><p align='center'>" + blacklistitem.facultyPANNumber + "</p></td>";
                        collegeFaculty += "<td width='10%'><p align='center'>" + blacklistitem.facultyAadhaarNumber + "</p></td>";

                        if (!string.IsNullOrEmpty(Facultyphoto))
                        {
                            string FacultyParsing = string.Empty;
                            string strFacultyPhoto = string.Empty;
                            string path = "http://jntuhaac.in/Content/Upload/Faculty/Photos/" + Facultyphoto.Trim();
                            //  string path = @"~/Content/Upload/Faculty/Photos/" + Facultyphoto.Trim();
                            // path = System.Web.HttpContext.Current.Server.MapPath(path);

                            #region With-Out Html Parsing
                            try
                            {
                                if (!string.IsNullOrEmpty(path))
                                // if (System.IO.File.Exists(path))
                                {
                                    FacultyParsing += "<p align='center'><img src='" + HtmlEncoder.Encode(path.Trim()) + "' align='center'  width='40' height='45' /></p>";
                                    var ParseEliments = HTMLWorker.ParseToList(new StringReader(FacultyParsing), null);

                                    if (path.Contains("."))
                                    {
                                        strFacultyPhoto = "<img alt=''src='" + HtmlEncoder.Encode(path.Trim()) + "' align='center'  height='45' />";
                                        // strPrincipalPhoto = "<img alt='' src='" + serverURL + "/Content/Upload/Faculty/Photos/" + PrincipalDetails.Photo + "'" + " align='center'  height='50' />";
                                        collegeFaculty += "<td width='9%'><p align='center'>" + strFacultyPhoto + "</p></td>";
                                    }
                                    else
                                    {
                                        collegeFaculty += "<td ><p align='center'>&nbsp;</p></td>";
                                    }
                                }
                                else
                                {
                                    if (test415PDF.Equals("YES"))
                                    {
                                        collegeFaculty += "<td ><p align='center'>&nbsp;</p></td>";
                                    }
                                    else
                                    {
                                        collegeFaculty += "<td ><p align='center'>&nbsp;</p></td>";
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                collegeFaculty += "<td ><p align='center'>&nbsp;</p></td>";
                                continue;
                            }
                            #endregion
                        }
                        else
                        {
                            collegeFaculty += "<td width='9%'><p align='center'>&nbsp;</p></td>";
                        }
                        collegeFaculty += "</tr>";
                        blackcount++;
                    }
                    collegeFaculty += "</tbody>";
                    collegeFaculty += "</table><br/>";
                }

                //Total Faculty And Blacklist Faculty Count
                collegeFaculty += "<br/><table border='1' cellspacing='0' cellpadding='3' style='font-size: 9px;'>";
                collegeFaculty += "<tr>";
                collegeFaculty += "<th><b>Total Faculty:</b></th>";
                collegeFaculty += "<td style='text-align:center'><b>" + facultyList.Count() + "</b></td>";
                collegeFaculty += "<th><b>Total Blacklist Faculty:</b></th>";
                collegeFaculty += "<td style='text-align:center'><b>" + BlackListFaculty.Count() + "</b></td>";
                collegeFaculty += "</tr>";
                collegeFaculty += "<tr>";
                // collegeFaculty += "<th><b>Total Adjunct Faculty:</b></th>";
                // collegeFaculty += "<td style='text-align:center'><b>" + type.Count() + "</b></td>";
                collegeFaculty += "<th colspan='2'><b>Total Absent Faculty:</b></th>";
                collegeFaculty += "<td colspan='2'></td>";
                collegeFaculty += "</tr>";
                collegeFaculty += "</table><br/>";


                if (DeptIDs.Count() > 0)
                {
                    collegeFaculty += "<strong><p style='font-size: 9px;'><u>11 a).Regular Teaching Faculty Summary Sheet</u>:</p></strong> <br />";
                    collegeFaculty += "<table border='1' cellspacing='0' cellpadding='3' style='font-size: 9px;'>";
                    collegeFaculty += "<tbody>";
                    collegeFaculty += "<tr>";
                    collegeFaculty += "<td colspan='1' rowspan='3'><p align='center'>SNo</p></td>";
                    collegeFaculty += "<td colspan='1' rowspan='3'><p align='left'>Degree</p></td>";
                    collegeFaculty += "<td colspan='2' rowspan='3'><p align='center'>Department / Specialization</p></td>";
                    collegeFaculty += "<td colspan='4'><p align='center'>Total number of faculty available</p></td>";
                    //collegeFaculty += "<td colspan='2' rowspan='2'><p align='left'>Total faculty Ratified </p></td>";
                    //collegeFaculty += "<td colspan='2' rowspan='2'><p align='left'>Total faculty Selected After A418 </p></td>";
                    collegeFaculty += "</tr>";
                    collegeFaculty += "<tr>";
                    collegeFaculty += "<td colspan='2'><p align='center'>UG</p></td>";
                    collegeFaculty += "<td colspan='2'><p align='center'>PG</p></td>";
                    collegeFaculty += "</tr>";
                    collegeFaculty += "<tr>";
                    collegeFaculty += "<td colspan='1'><p align='center'>Uploaded</p></td>";
                    collegeFaculty += "<td colspan='1'><p align='left'>Available as per CF</p></td>";
                    collegeFaculty += "<td colspan='1'><p align='center'>Uploaded</p></td>";
                    collegeFaculty += "<td colspan='1'><p align='left'>Available as per CF </p></td>";
                    //collegeFaculty += "<td colspan='2'><p align='left'>Available as per CF</p></td>";

                    collegeFaculty += "</tr>";

                    var summarydata = facultyList.Join(cFaculty, i => i.FacultyRegistrationNumber, j => j.RegistrationNumber, (i, j) => new { i, j })
                                                 .GroupBy(a => new { a.i.facultyDepartmentId, a.i.facultyDesignationId })
                                                 .Select(g => new
                                                 {
                                                     deptid = g.Key.facultyDepartmentId,
                                                     UG = g.Count(d => d.j.IdentifiedFor == "UG"),
                                                     PG = g.Count(d => d.j.IdentifiedFor == "PG"),
                                                     UGPG = g.Count(d => d.j.IdentifiedFor == "UG&PG"),
                                                     FC = g.Count(c => c.i.isFacultyRatifiedByJNTU == true)
                                                 })
                                                .Join(db.jntuh_department, r => r.deptid, ro => ro.id, (r, ro) => new { r, ro })
                                                .Join(db.jntuh_degree, x => x.ro.degreeId, y => y.id, (x, y) => new { x, y })
                                                .Select(z => new { z.y.degree, z.x.ro.departmentName, z.x.r.UG, z.x.r.PG, z.x.r.UGPG, z.x.r.FC })
                                                .GroupBy(h => new { h.degree, h.departmentName })
                                                .Select(i => new
                                                {
                                                    i.Key.degree,
                                                    i.Key.departmentName,
                                                    UG = i.Sum(j => j.UG),
                                                    PG = i.Sum(j => j.PG),
                                                    UGPG = i.Sum(j => j.UGPG),
                                                    FC = i.Max(j => j.FC)
                                                }).OrderBy(e => e.departmentName)
                                                .ToList();
                    int countdata = 1;
                    foreach (var item in summarydata)
                    {
                        collegeFaculty += "<tr>";
                        collegeFaculty += "<td colspan='1'><p align='center'>" + countdata + "</p></td>";
                        collegeFaculty += "<td colspan='1'><p align='left'>" + item.degree + "</p></td>";
                        collegeFaculty += "<td colspan='2'><p align='left'>" + item.departmentName + "</p></td>";
                        collegeFaculty += "<td colspan='1'><p align='center'>" + item.UG + "</p></td>";
                        collegeFaculty += "<td colspan='1'><p align='left'>&nbsp;</p></td>";
                        collegeFaculty += "<td colspan='1'><p align='center'>" + item.PG + "</p></td>";
                        collegeFaculty += "<td colspan='1'><p align='left'>&nbsp;</p></td>";
                        //collegeFaculty += "<td colspan='2'><p align='left'>&nbsp;</p></td>";
                        collegeFaculty += "</tr>";
                        countdata++;
                    }
                    collegeFaculty += "</tbody>";
                    collegeFaculty += "</table><br />";
                }

                contents = contents.Replace("##COLLEGETeachingFaculty##", collegeFaculty);
                //List<IElement> parsedHtmlElements = HTMLWorker.ParseToList(new StringReader(contents), null);
                return contents;
            }
        }

        private string TeachingFaculty(List<CollegeFaculty> facultyList)
        {
            int count = 1;
            int Oldcount = 1;
            string strcheckList = "<img alt='' src='http://jntuhaac.in/Content/Images/checkbox_no_b.png' height='8' />";
            string collegeFaculty = string.Empty;
            string ContentFaculty = string.Empty;
            string gender = string.Empty;
            string category = string.Empty;
            string department = string.Empty;
            string designation = string.Empty;
            string qualification = string.Empty;
            string dateOfAppointment = string.Empty;
            string teachingType = string.Empty;
            string ratified = string.Empty;

            var jntuh_specialization = db.jntuh_specialization.ToList();
            int? deptid = null;
            int? specid = null;
            string Spec = null;
            if (facultyList.Count() > 0)
            {
                deptid = facultyList.FirstOrDefault().facultyDepartmentId;
                specid = facultyList.FirstOrDefault().SpecializationId;
                Spec = facultyList.FirstOrDefault().SpecializationName;
            }
            else
                deptid = null;
            if (deptid == null || deptid == 0)
            {
                department = "Others (No Department/Specialization)";
            }
            else
            {
                department = db.jntuh_department.Where(d => d.id == deptid).Select(d => d.jntuh_degree.degree + "-" + d.departmentName).FirstOrDefault();
                if (Spec != null)
                    department += "-" + Spec;
            }
            var collegeIds = facultyList.Select(f => f.collegeId).ToList();

            var Bas_Days = db.jntuh_college_basreport.Where(s => collegeIds.Contains(s.collegeId)).Select(e => e).ToList();

            if (facultyList.Count() > 0 && facultyList.FirstOrDefault().facultyType == "Adjunct")
            {
                department = "Adjunct Faculty";

                #region adjunct

                collegeFaculty += "<br /><table border='0' cellspacing='0' cellpadding='3' style='font-size: 9px;'>";
                collegeFaculty += "<tbody>";
                collegeFaculty += "<tr><td>";
                collegeFaculty += "<strong><u>" + department + "</u> (Existing Faculty)</strong> ";
                collegeFaculty += "</td></tr>";
                collegeFaculty += "<tr><td>";
                collegeFaculty += "<b>NOC-</b> No Objection Certificate From Parent Organization,<b>PBM-</b> Professional Body Members,<b>ISCM-</b> Internal Selection Committee Minutes";
                collegeFaculty += "</td></tr>";
                collegeFaculty += "</tbody>";
                collegeFaculty += "</table>";
                collegeFaculty += "<table border='1' cellspacing='0' cellpadding='3' style='font-size: 9px;'>";
                collegeFaculty += "<tbody>";
                collegeFaculty += "<tr>";
                collegeFaculty += "<td  width='4%'><p align='center'>SNo</p></td>";
                collegeFaculty += "<td  width='14%'><p align='center'>Registration Number</p></td>";
                collegeFaculty += "<td  width='12%'><p align='left'>Faculty Name</p></td>";
                collegeFaculty += "<td  width='3%'><p align='center'>Gender</p></td>";
                collegeFaculty += "<td  width='4%'><p align='left'>Designation</p></td>";
                collegeFaculty += "<td  width='6%'><p align='left'>Qualification </p></td>";
                collegeFaculty += "<td  width='9%'><p align='left'>Date of Appointment</p></td>";
                collegeFaculty += "<td  width='11%'><p align='center'>Pan Number</p></td>";
                collegeFaculty += "<td  width='11%'><p align='center'>Aadhaar Number</p></td>";
                collegeFaculty += "<td  width='7%'><p align='center'>BAS Joining Date</p></td>";
                collegeFaculty += "<td  width='3%'><p align='center'>NOC</p></td>";
                collegeFaculty += "<td  width='3%'><p align='center'>PBM</p></td>";
                collegeFaculty += "<td  width='3%'><p align='center'>ISCM</p></td>";
                collegeFaculty += "<td  width='7%'><p align='center'>Photo</p></td>";
                collegeFaculty += "<td  width='3%'><p align='center'>CF</p></td>";
                collegeFaculty += "</tr>";

                var designationIds = facultyList.Select(f => f.facultyDesignationId).ToList();
                var jntuh_designations = db.jntuh_designation.Where(it => designationIds.Contains(it.id)).Select(it => new
                {
                    it.id,
                    it.designation
                }).ToList();
                var fids = facultyList.Select(f => f.id).ToList();
                var jntuh_registered_faculty_educations = db.jntuh_registered_faculty_education
                    .OrderByDescending(education => education.educationId)
                    .Where(it => fids.Contains(it.facultyId) && it.educationId != 8).Select(it => new
                    {
                        it.educationId,
                        it.facultyId,
                        it.courseStudied,
                        it.passedYear,
                        it.specialization
                    }).ToList();
                // var collegeIds = facultyList.Select(f => f.collegeId).ToList();
                var regnumbs = facultyList.Select(f => f.FacultyRegistrationNumber).ToList();
                var jntuh_college_faculty_registered = db.jntuh_college_faculty_registered.Where(it => collegeIds.Contains(it.collegeId)
                    && regnumbs.Contains(it.RegistrationNumber)).Select(it => new
                    {
                        it.collegeId,
                        it.id,
                        it.RegistrationNumber,
                        it.IdentifiedFor
                    }).ToList();

                var jntuh_registered_facultys = db.jntuh_registered_faculty.Where(a => fids.Contains(a.id)).Select(F => new { F.RegistrationNumber, F.Photo }).ToList();

                string Ftype = "";
                //var Bas_Days = db.jntuh_college_basreport.Where(s => collegeIds.Contains(s.collegeId)).Select(e => e).ToList();

                foreach (var item in facultyList.OrderByDescending(a => a.HighestQualification).ToList())
                {
                    if (item.facultyGenderId == 1)
                    {
                        gender = "M";
                    }
                    else
                    {
                        gender = "F";
                    }

                    designation = jntuh_designations.Where(d => d.id == item.facultyDesignationId).Select(d => d.designation).FirstOrDefault();
                    if (item.dateOfAppointment != null)
                    {
                        dateOfAppointment = UAAAS.Models.Utilities.MMDDYY2DDMMYY(item.dateOfAppointment.ToString());
                    }

                    qualification = jntuh_registered_faculty_educations.OrderByDescending(education => education.educationId)
                                                           .Where(education => education.facultyId == item.id)
                                                           .Select(education => education.courseStudied).FirstOrDefault();

                    string strSpecialization = string.Empty;
                    var Specialization = jntuh_specialization.Where(s => s.id == item.SpecializationId).Select(s => s.specializationName).FirstOrDefault();
                    strSpecialization = Specialization;

                    string identifiedfor = jntuh_college_faculty_registered.Where(f => f.RegistrationNumber == item.FacultyRegistrationNumber && f.collegeId == item.collegeId).Select(f => f.IdentifiedFor).FirstOrDefault();

                    var Faculty_Data = Bas_Days.Where(e => e.RegistrationNumber == item.FacultyRegistrationNumber).Select(e => e).ToList();
                    DateTime? Old_JiningDate = Faculty_Data.OrderByDescending(a => a.joiningDate).Select(f => f.joiningDate).FirstOrDefault();
                    string JoiningDate = UAAAS.Models.Utilities.MMDDYY2DDMMYY(Old_JiningDate.ToString());
                    if (item.HighestQualification == 6)
                        collegeFaculty += "<tr style='font-weight:bold;'>";
                    else
                        collegeFaculty += "<tr>";
                    collegeFaculty += "<td  style='font-size: 8px;'><p align='center'>" + count + "</p></td>";
                    if (item.HighestQualification == 6)
                        collegeFaculty += "<td  style='font-size: 8px;'><p align='center'>" + item.FacultyRegistrationNumber + "&nbsp;&nbsp;&nbsp;" + strcheckList + "&nbsp;&nbsp;&nbsp;&nbsp;" + "<b>(Ph.D)</b> </p></td>";
                    else
                        collegeFaculty += "<td  style='font-size: 8px;'><p align='center'>" + item.FacultyRegistrationNumber + "&nbsp;&nbsp;&nbsp;" + strcheckList + "</p></td>";
                    collegeFaculty += "<td  style='font-size: 8px;'><p align='left'>" + (item.facultyFirstName + " " + item.facultySurname + " " + item.facultyLastName).ToUpper() + "&nbsp;&nbsp;&nbsp;" + strcheckList + "</p></td>";
                    collegeFaculty += "<td  style='font-size: 8px;'><p align='center'>" + gender + "</p></td>";

                    if (!string.IsNullOrEmpty(designation))
                    {
                        if (item.facultyDesignationId == 1)
                        {
                            collegeFaculty += "<td  style='font-size:8px;'><p align='left'><strong>" + "P" + "</strong></p></td>";
                        }
                        else if (item.facultyDesignationId == 2)
                        {
                            collegeFaculty += "<td  style='font-size:8px;'><p align='left'><strong>" + "ASP" + "</strong></p></td>";
                        }
                        else if (item.facultyDesignationId == 3)
                        {
                            collegeFaculty += "<td style='font-size:8px;'><p align='left'><strong>" + "AP" + "</strong></p></td>";
                        }
                    }
                    else
                    {
                        collegeFaculty += "<td style='font-size:8px;'><p align='left'>&nbsp;</p></td>";
                    }

                    if (!string.IsNullOrEmpty(qualification))
                    {
                        collegeFaculty += "<td  style='font-size: 8px;'><p align='left'>" + qualification.ToUpper() + "&nbsp;&nbsp;&nbsp;<br/><br/>" + strcheckList + "</p></td>";
                    }
                    else
                    {
                        collegeFaculty += "<td style='font-size: 8px;'><p align='left'>&nbsp;</p></td>";
                    }

                    string Facultyphoto = jntuh_registered_facultys.Where(F => F.RegistrationNumber == item.FacultyRegistrationNumber).Select(F => F.Photo).FirstOrDefault();
                    collegeFaculty += "<td  style='font-size: 8px;'><p align='left'>" + dateOfAppointment + "&nbsp;&nbsp;&nbsp;" + strcheckList + "</p></td>";
                    collegeFaculty += "<td ><p align='center'>" + item.facultyPANNumber + "&nbsp;&nbsp;&nbsp;<br/>" + strcheckList + "</p></td>";
                    collegeFaculty += "<td ><p align='center'>" + item.facultyAadhaarNumber + "&nbsp;&nbsp;&nbsp;" + strcheckList + "</p></td>";
                    collegeFaculty += "<td ><p align='center'>" + JoiningDate + "</p></td>";
                    //collegeFaculty += "<td ><p align='center'>" + PresentDays + "/" + WorkingDays + "</p></td>";
                    collegeFaculty += "<td  style='font-size: 8px;'><p align='center'>" + strcheckList + "</p></td>";
                    collegeFaculty += "<td  style='font-size: 8px;'><p align='center'>" + strcheckList + "</p></td>";
                    collegeFaculty += "<td  style='font-size: 8px;'><p align='center'>" + strcheckList + "</p></td>";
                    if (!string.IsNullOrEmpty(Facultyphoto))
                    {
                        string FacultyParsing = string.Empty;
                        string strFacultyPhoto = string.Empty;
                        string path = "http://jntuhaac.in/Content/Upload/Faculty/Photos/" + Facultyphoto.Trim();
                        //  string path = @"~/Content/Upload/Faculty/Photos/" + Facultyphoto.Trim(); 
                        //  path = System.Web.HttpContext.Current.Server.MapPath(path);
                        //  if (System.IO.File.Exists(path))

                        #region With-Out Html Parsing
                        try
                        {
                            if (!string.IsNullOrEmpty(path))
                            // if (System.IO.File.Exists(path))
                            {
                                FacultyParsing += "<p align='center'><img src='" + HtmlEncoder.Encode(path.Trim()) + "' align='center'  width='40' height='45' /></p>";
                                var ParseEliments = HTMLWorker.ParseToList(new StringReader(FacultyParsing), null);

                                if (path.Contains("."))
                                {
                                    strFacultyPhoto = "<img alt=''src='" + HtmlEncoder.Encode(path.Trim()) + "' align='center'  height='45' />";
                                    // strPrincipalPhoto = "<img alt='' src='" + serverURL + "/Content/Upload/Faculty/Photos/" + PrincipalDetails.Photo + "'" + " align='center'  height='50' />";
                                    collegeFaculty += "<td><p align='center'>" + strFacultyPhoto + "</p></td>";
                                }
                                else
                                    collegeFaculty += "<td ><p align='center'>&nbsp;</p></td>";
                            }
                            else
                            {
                                if (test415PDF.Equals("YES"))
                                {
                                    collegeFaculty += "<td ><p align='center'>&nbsp;</p></td>";
                                }
                                else
                                {
                                    collegeFaculty += "<td ><p align='center'>&nbsp;</p></td>";
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            collegeFaculty += "<td ><p align='center'>&nbsp;</p></td>";
                            continue;
                        }
                        #endregion
                    }
                    else
                    {
                        collegeFaculty += "<td><p align='center'>&nbsp;</p></td>";
                    }

                    collegeFaculty += "<td><p align='center'>" + strcheckList + "</p></td>";
                    collegeFaculty += "</tr>";
                    count++;
                }

                collegeFaculty += "</tbody>";
                collegeFaculty += "</table>";

                #endregion
            }
            else
            {
                #region Departmentwise OldFaculty
                var SameRegNos = facultyList.Select(s => s.FacultyRegistrationNumber).ToList();

                string OldCollegeFaculty = string.Empty;

                OldCollegeFaculty += "<br /><table border='0' cellspacing='0' cellpadding='3' style='font-size: 9px;'>";
                OldCollegeFaculty += "<tbody>";
                OldCollegeFaculty += "<tr><td>";
                OldCollegeFaculty += "<strong><u>" + department + "</u>(Existing Faculty)</strong> ";
                OldCollegeFaculty += "</td></tr>";
                OldCollegeFaculty += "</tbody>";
                OldCollegeFaculty += "</table>";

                OldCollegeFaculty += "<table border='1' cellspacing='0' cellpadding='3' style='font-size:9px;'>";
                OldCollegeFaculty += "<tbody>";
                OldCollegeFaculty += "<tr>";
                OldCollegeFaculty += "<td  width='4%'><p align='center'>SNo</p></td>";
                OldCollegeFaculty += "<td  width='14%'><p align='center'>Registration Number</p></td>";
                OldCollegeFaculty += "<td  width='10%'><p align='left'>Faculty Name</p></td>";
                OldCollegeFaculty += "<td  width='3%'><p align='center'>Gender</p></td>";
                OldCollegeFaculty += "<td  width='4%'><p align='left'>Designation</p></td>";
                OldCollegeFaculty += "<td  width='6%'><p align='left'>Qualification </p></td>";
                OldCollegeFaculty += "<td  width='9%'><p align='left'>Date of Appointment</p></td>";
                OldCollegeFaculty += "<td  width='11%'><p align='center'>Pan Number</p></td>";
                OldCollegeFaculty += "<td  width='12%'><p align='center'>Aadhaar Number</p></td>";
                OldCollegeFaculty += "<td  width='3%'><p align='center'>SCM</p></td>";
                OldCollegeFaculty += "<td  width='3%'><p align='center'>F16</p></td>";
                OldCollegeFaculty += "<td  width='3%'><p align='center'>26AS</p></td>";
                OldCollegeFaculty += "<td  width='6%'><p align='center'>Gross Salary</p></td>";
                //collegeFaculty += "<td  width='6%'><p align='center'>P.W.D/T.W.D</p></td>";
                OldCollegeFaculty += "<td width='7%'><p align='center'>Photo</p></td>";
                OldCollegeFaculty += "<td width='2%'><p align='center'>CF</p></td>";
                OldCollegeFaculty += "<td width='5%'><p align='center'>Remarks</p></td>";
                OldCollegeFaculty += "</tr>";

                int OldCollegeId = 0;
                if (facultyList.Count() > 0)
                {
                    OldCollegeId = facultyList.FirstOrDefault().collegeId;
                }
                var OldCollegeFacultyList = new List<jntuh_college_previous_academic_faculty>();
                if (specid == null || specid == 0)
                    OldCollegeFacultyList = db.jntuh_college_previous_academic_faculty.Where(a => a.collegeId == OldCollegeId && a.DepartmentId == deptid).Select(a => a).ToList();
                else
                    OldCollegeFacultyList = db.jntuh_college_previous_academic_faculty.Where(a => a.collegeId == OldCollegeId && a.DepartmentId == deptid && a.SpecializationId == specid).Select(a => a).ToList();

                var OldRegNos = OldCollegeFacultyList.Select(a => a.RegistrationNumber).ToList();
                var Old_jntuh_registered_faculty = db.jntuh_registered_faculty.Where(a => OldRegNos.Contains(a.RegistrationNumber)).Select(w => w).ToList();

                var OlddesignationIds = Old_jntuh_registered_faculty.Select(f => f.DesignationId).ToList();
                var Oldjntuh_designations = db.jntuh_designation.Where(it => OlddesignationIds.Contains(it.id)).Select(it => new
                {
                    it.id,
                    it.designation
                }).ToList();

                var OldFacultyIds = Old_jntuh_registered_faculty.Select(q => q.id).ToList();
                var Oldjntuh_registered_faculty_educations = db.jntuh_registered_faculty_education
                   .OrderByDescending(education => education.educationId)
                   .Where(it => OldFacultyIds.Contains(it.facultyId) && it.educationId != 8).Select(it => new
                   {
                       it.educationId,
                       it.facultyId,
                       it.courseStudied,
                       it.passedYear,
                       it.specialization
                   }).ToList();

                var jntuh_regstrd_faculty_exps = db.jntuh_registered_faculty_experience.AsNoTracking().Where(o => o.createdBycollegeId == OldCollegeId).Select(s => s).ToList();
                var jntuh_phdfaculty = db.jntuh_faculty_phddetails.AsNoTracking().ToList();
                #region oldTeachingfaculty
                List<CollegeFaculty> OldFaciltyList = new List<CollegeFaculty>();
                foreach (var item in OldCollegeFacultyList)
                {
                    CollegeFaculty collegeFacultynew = new CollegeFaculty();
                    jntuh_registered_faculty rFaculty = Old_jntuh_registered_faculty.Where(f => f.RegistrationNumber == item.RegistrationNumber.Trim()).FirstOrDefault();
                    if (rFaculty != null)
                    {
                        collegeFacultynew.id = rFaculty.id;
                        collegeFacultynew.TotalExperience = rFaculty.TotalExperience != null ? rFaculty.TotalExperience : 0;
                        collegeFacultynew.collegeId = OldCollegeId;
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
                        collegeFacultynew.designation = Oldjntuh_designations.Where(d => d.id == collegeFacultynew.facultyDesignationId).Select(d => d.designation).FirstOrDefault();
                        collegeFacultynew.facultyOtherDesignation = rFaculty.OtherDesignation;
                        collegeFacultynew.facultyDepartmentId = item.DepartmentId != null ? (int)item.DepartmentId : 0;
                        collegeFacultynew.department = db.jntuh_department.Where(d => d.id == collegeFacultynew.facultyDepartmentId).Select(d => d.departmentName).FirstOrDefault();
                        collegeFacultynew.SpecializationId = item.SpecializationId != null ? (int)item.SpecializationId : 0;
                        collegeFacultynew.SpecializationName = jntuh_specialization.Where(s => s.id == collegeFacultynew.SpecializationId).Select(s => s.specializationName).FirstOrDefault();
                        collegeFacultynew.facultyOtherDepartment = rFaculty.OtherDepartment;
                        collegeFacultynew.grossSalary = rFaculty.grosssalary;
                        collegeFacultynew.facultyEmail = rFaculty.Email;
                        collegeFacultynew.facultyMobile = rFaculty.Mobile;
                        collegeFacultynew.facultyPANNumber = rFaculty.PANNumber;
                        collegeFacultynew.facultyAadhaarNumber = item.AadhaarNumber;
                        collegeFacultynew.FacultyRegistrationNumber = rFaculty.RegistrationNumber.Trim();
                        //collegeFacultynew.dateOfAppointment = UAAAS.Models.Utilities.MMDDYY2DDMMYY(rFaculty.DateOfAppointment.ToString());
                        collegeFacultynew.FacultyDeactivationReasion = Old_jntuh_registered_faculty.Where(f => f.RegistrationNumber == collegeFacultynew.FacultyRegistrationNumber).Select(i => i.DeactivationReason).FirstOrDefault();
                        collegeFacultynew.PANDeactivationReasion = rFaculty.PanDeactivationReason;
                        collegeFacultynew.BlackList = (bool)rFaculty.Blacklistfaculy;
                        collegeFacultynew.HighestQualification = Oldjntuh_registered_faculty_educations.OrderByDescending(education => education.educationId)
                                                                .Where(education => education.facultyId == rFaculty.id)
                                                                .Select(education => education.educationId).FirstOrDefault();
                        if (jntuh_regstrd_faculty_exps != null)
                        {
                            var regfacultyExp = jntuh_regstrd_faculty_exps.Where(i => i.facultyId == rFaculty.id).LastOrDefault();
                            if (regfacultyExp != null)
                            {
                                collegeFacultynew.facultygrosssalA21 = regfacultyExp.FacultyGrossSalary;
                                collegeFacultynew.dateOfAppointment = UAAAS.Models.Utilities.MMDDYY2DDMMYY(regfacultyExp.facultyDateOfAppointment.ToString());
                            }
                        }
                        collegeFacultynew.Phd2pages = jntuh_phdfaculty.Where(i => i.Facultyid == rFaculty.id).Count() > 0 ? true : false;
                        //Last Year Faculty Check
                        collegeFacultynew.PrevoiusYearFaculty = SameRegNos.Contains(collegeFacultynew.FacultyRegistrationNumber) ? true : false;
                        OldFaciltyList.Add(collegeFacultynew);
                    }
                }
                #endregion

                int OldFacultyCount = OldFaciltyList.Where(q => q.PrevoiusYearFaculty == true).Select(a => a.FacultyRegistrationNumber).Count();
                if (OldFacultyCount == 0)
                {
                    OldCollegeFaculty += "<tr><td colspan='16'><p align='center'>NIL</p></td></tr>";
                }
                else
                {
                    foreach (var Old in OldFaciltyList.Where(q => q.PrevoiusYearFaculty == true).OrderByDescending(a => a.HighestQualification).ToList())
                    {
                        string Oldgender = string.Empty;
                        string OlddateOfAppointment = string.Empty;
                        string Olddepartment = string.Empty;
                        string Olddesignation = string.Empty;
                        string Oldqualification = string.Empty;

                        if (Old.facultyGenderId == 1)
                            Oldgender = "M";
                        else
                            Oldgender = "F";

                        Olddesignation = Oldjntuh_designations.Where(d => d.id == Old.facultyDesignationId).Select(d => d.designation).FirstOrDefault();

                        if (Old.dateOfAppointment != null)
                        {
                            OlddateOfAppointment = UAAAS.Models.Utilities.MMDDYY2DDMMYY(Old.dateOfAppointment.ToString());
                        }

                        Oldqualification = Oldjntuh_registered_faculty_educations.OrderByDescending(education => education.educationId)
                                                                .Where(education => education.facultyId == Old.id)
                                                                .Select(education => education.courseStudied).FirstOrDefault();

                        string strSpecialization = string.Empty;
                        var Specialization = jntuh_specialization.Where(s => s.id == Old.SpecializationId).Select(s => s.specializationName).FirstOrDefault();
                        strSpecialization = Specialization;

                        string identifiedfor = Old.IdentifiedFor;

                        var Faculty_Data = Bas_Days.Where(e => e.RegistrationNumber == Old.FacultyRegistrationNumber).Select(e => e).ToList();
                        DateTime? Old_JiningDate = Faculty_Data.OrderByDescending(s => s.joiningDate).Select(f => f.joiningDate).FirstOrDefault();
                        string JoiningDate = UAAAS.Models.Utilities.MMDDYY2DDMMYY(Old_JiningDate.ToString());

                        string Facultyphoto = Old_jntuh_registered_faculty.Where(F => F.RegistrationNumber == Old.FacultyRegistrationNumber).Select(F => F.Photo).FirstOrDefault();

                        if (Old.HighestQualification == 6 || Old.Phd2pages == true)
                            OldCollegeFaculty += "<tr style='font-weight:bold;'>";
                        else
                            OldCollegeFaculty += "<tr>";

                        OldCollegeFaculty += "<td  style='font-size: 8px;'><p align='center'>" + Oldcount + "</p></td>";
                        if (Old.HighestQualification == 6 || Old.Phd2pages == true)
                            OldCollegeFaculty += "<td  style='font-size: 8px;'><p align='center'>" + Old.FacultyRegistrationNumber + "&nbsp;&nbsp;&nbsp;" + strcheckList + "&nbsp;&nbsp;&nbsp;&nbsp;" + "<b>(Ph.D)</b> </p></td>";
                        else
                            OldCollegeFaculty += "<td  style='font-size: 8px;'><p align='center'>" + Old.FacultyRegistrationNumber + "&nbsp;&nbsp;&nbsp;" + strcheckList + "</p></td>";
                        OldCollegeFaculty += "<td  style='font-size: 8px;'><p align='left'>" + (Old.facultyFirstName + " " + Old.facultySurname + " " + Old.facultyLastName).ToUpper() + "&nbsp;&nbsp;&nbsp;" + strcheckList + "</p></td>";
                        OldCollegeFaculty += "<td  style='font-size: 8px;'><p align='center'>" + Oldgender + "</p></td>";

                        if (!string.IsNullOrEmpty(Olddesignation))
                        {
                            if (Old.facultyDesignationId == 1)
                                OldCollegeFaculty += "<td  style='font-size:8px;'><p align='left'>" + "P" + "</p></td>";
                            else if (Old.facultyDesignationId == 2)
                                OldCollegeFaculty += "<td style='font-size:8px;'><p align='left'>" + "ASP" + "</p></td>";
                            else if (Old.facultyDesignationId == 3)
                                OldCollegeFaculty += "<td  style='font-size:8px;'><p align='left'>" + "AP" + "</p></td>";
                            else
                                OldCollegeFaculty += "<td  style='font-size:8px;'><p align='left'>&nbsp;</p></td>";
                        }
                        else
                            OldCollegeFaculty += "<td  style='font-size:8px;'><p align='left'>&nbsp;</p></td>";

                        if (!string.IsNullOrEmpty(Oldqualification))
                            OldCollegeFaculty += "<td  style='font-size: 8px;'><p align='left'>" + Oldqualification.ToUpper() + "&nbsp;&nbsp;&nbsp;<br/><br/>" + strcheckList + "</p></td>";
                        else
                            OldCollegeFaculty += "<td  style='font-size: 8px;'><p align='left'>&nbsp;</p></td>";

                        OldCollegeFaculty += "<td  style='font-size: 8px;'><p align='left'>" + OlddateOfAppointment + "&nbsp;&nbsp;&nbsp;" + strcheckList + "</p></td>";
                        OldCollegeFaculty += "<td  style='font-size: 8px;'><p align='center'>" + Old.facultyPANNumber + "&nbsp;&nbsp;" + strcheckList + "</p></td>";
                        OldCollegeFaculty += "<td ><p align='center'>" + Old.facultyAadhaarNumber + "&nbsp;&nbsp;" + strcheckList + "</p></td>";
                        OldCollegeFaculty += "<td  style='font-size: 8px;'><p align='center'>" + strcheckList + "</p></td>";
                        OldCollegeFaculty += "<td ><p align='center'>" + strcheckList + "</p></td>";
                        OldCollegeFaculty += "<td ><p align='center'>" + strcheckList + "</p></td>";
                        OldCollegeFaculty += "<td ><p align='center'>" + Old.facultygrosssalA21 + "</p></td>";
                        if (!string.IsNullOrEmpty(Facultyphoto))
                        {
                            string FacultyParsing = string.Empty;
                            string strFacultyPhoto = string.Empty;
                            string path = "http://jntuhaac.in/Content/Upload/Faculty/Photos/" + Facultyphoto.Trim();
                            // string path = @"~/Content/Upload/Faculty/Photos/" + Facultyphoto.Trim();
                            // path = System.Web.HttpContext.Current.Server.MapPath(path);
                            //  if (System.IO.File.Exists(path))

                            #region With-Out Html Parsing
                            try
                            {
                                if (!string.IsNullOrEmpty(path))
                                // if (System.IO.File.Exists(path))
                                {
                                    FacultyParsing += "<p align='center'><img src='" + HtmlEncoder.Encode(path.Trim()) + "' align='center'  width='40' height='45' /></p>";
                                    var ParseEliments = HTMLWorker.ParseToList(new StringReader(FacultyParsing), null);

                                    if (path.Contains("."))
                                    {
                                        strFacultyPhoto = "<img alt=''src='" + HtmlEncoder.Encode(path.Trim()) + "' align='center'  height='45' />";
                                        // strPrincipalPhoto = "<img alt='' src='" + serverURL + "/Content/Upload/Faculty/Photos/" + PrincipalDetails.Photo + "'" + " align='center'  height='50' />";
                                        OldCollegeFaculty += "<td ><p align='center'>" + strFacultyPhoto + "</p></td>";
                                    }
                                    else
                                    {
                                        OldCollegeFaculty += "<td ><p align='center'>&nbsp;</p></td>";
                                    }
                                }
                                else
                                {
                                    if (test415PDF.Equals("YES"))
                                    {
                                        OldCollegeFaculty += "<td ><p align='center'>&nbsp;</p></td>";
                                    }
                                    else
                                    {
                                        OldCollegeFaculty += "<td ><p align='center'>&nbsp;</p></td>";
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                OldCollegeFaculty += "<td ><p align='center'>&nbsp;</p></td>";
                                OldCollegeFaculty += "<td ><p align='center'>" + strcheckList + "</p></td>";
                                OldCollegeFaculty += "<td><p align='center'></p></td>";
                                OldCollegeFaculty += "</tr>";
                                continue;
                            }
                            #endregion
                        }
                        else
                            OldCollegeFaculty += "<td ><p align='center'>&nbsp;</p></td>";

                        OldCollegeFaculty += "<td ><p align='center'>" + strcheckList + "</p></td>";
                        OldCollegeFaculty += "<td><p align='center'></p></td>";
                        OldCollegeFaculty += "</tr>";
                        Oldcount++;
                    }
                }
                OldCollegeFaculty += "</tbody>";
                OldCollegeFaculty += "</table><br/>";
                collegeFaculty += OldCollegeFaculty;

                #endregion

                #region teaching Faculty

                collegeFaculty += "<br /><table border='0' cellspacing='0' cellpadding='3' style='font-size: 9px;'>";
                collegeFaculty += "<tbody>";
                collegeFaculty += "<tr><td>";
                collegeFaculty += "<strong><u>" + department + "</u>(Newly Added Faculty)</strong> ";
                collegeFaculty += "</td></tr>";
                collegeFaculty += "</tbody>";
                collegeFaculty += "</table>";

                collegeFaculty += "<table border='1' cellspacing='0' cellpadding='3' style='font-size:9px;'>";
                collegeFaculty += "<tbody>";
                collegeFaculty += "<tr>";
                collegeFaculty += "<td  width='4%'><p align='center'>SNo</p></td>";
                collegeFaculty += "<td  width='14%'><p align='center'>Registration Number</p></td>";
                collegeFaculty += "<td  width='10%'><p align='left'>Faculty Name</p></td>";
                collegeFaculty += "<td  width='3%'><p align='center'>Gender</p></td>";
                collegeFaculty += "<td  width='4%'><p align='left'>Designation</p></td>";
                collegeFaculty += "<td  width='6%'><p align='left'>Qualification </p></td>";
                collegeFaculty += "<td  width='9%'><p align='left'>Date of Appointment</p></td>";
                collegeFaculty += "<td  width='11%'><p align='center'>Pan Number</p></td>";
                collegeFaculty += "<td  width='12%'><p align='center'>Aadhaar Number</p></td>";
                collegeFaculty += "<td  width='3%'><p align='center'>SCM</p></td>";
                collegeFaculty += "<td  width='3%'><p align='center'>F16</p></td>";
                collegeFaculty += "<td  width='3%'><p align='center'>26AS</p></td>";
                collegeFaculty += "<td  width='6%'><p align='center'>Gross Salary</p></td>";
                //collegeFaculty += "<td  width='6%'><p align='center'>P.W.D/T.W.D</p></td>";
                collegeFaculty += "<td width='7%'><p align='center'>Photo</p></td>";
                collegeFaculty += "<td  width='2%'><p align='center'>CF</p></td>";
                collegeFaculty += "<td  width='5%'><p align='center'>Remarks</p></td>";
                collegeFaculty += "</tr>";

                var designationIds = facultyList.Select(f => f.facultyDesignationId).ToList();
                var jntuh_designations = db.jntuh_designation.Where(it => designationIds.Contains(it.id)).Select(it => new
                {
                    it.id,
                    it.designation
                }).ToList();
                var fids = facultyList.Select(f => f.id).ToList();
                var jntuh_registered_faculty_educations = db.jntuh_registered_faculty_education
                    .OrderByDescending(education => education.educationId)
                    .Where(it => fids.Contains(it.facultyId) && it.educationId != 8).Select(it => new
                    {
                        it.educationId,
                        it.facultyId,
                        it.courseStudied,
                        it.passedYear,
                        it.specialization
                    }).ToList();

                var regnumbs = facultyList.Select(f => f.FacultyRegistrationNumber).ToList();
                var jntuh_college_faculty_registered = db.jntuh_college_faculty_registered.Where(it => collegeIds.Contains(it.collegeId)
                    && regnumbs.Contains(it.RegistrationNumber)).Select(it => new
                    {
                        it.collegeId,
                        it.id,
                        it.RegistrationNumber,
                        it.IdentifiedFor
                    }).ToList();

                var jntuh_registered_facultys = db.jntuh_registered_faculty.Where(a => fids.Contains(a.id)).Select(F => new { F.RegistrationNumber, F.Photo }).ToList();
                //var jntuh_phdfaculty = db.jntuh_faculty_phddetails.AsNoTracking().ToList();
                string Ftype = "";
                var nophotos = facultyList.Select(i => i.photo).ToList();

                var AlreadyExistedFaculty = OldFaciltyList.Where(a => a.PrevoiusYearFaculty == true).Select(a => a.FacultyRegistrationNumber).ToList();
                int NewlyFacultyCount = facultyList.Where(a => !AlreadyExistedFaculty.Contains(a.FacultyRegistrationNumber)).Select(a => a.FacultyRegistrationNumber).Count();
                if (NewlyFacultyCount == 0)
                {
                    collegeFaculty += "<tr><td colspan='15'><p align='center'>NIL</p></td></tr>";
                }
                else
                {
                    foreach (var item in facultyList.Where(a => !AlreadyExistedFaculty.Contains(a.FacultyRegistrationNumber)).OrderByDescending(s => s.HighestQualification).ToList())
                    {
                        item.Phd2pages = jntuh_phdfaculty.Where(i => i.Facultyid == item.id).Count() > 0 ? true : false;
                        if (item.facultyGenderId == 1)
                            gender = "M";
                        else
                            gender = "F";

                        designation = jntuh_designations.Where(d => d.id == item.facultyDesignationId).Select(d => d.designation).FirstOrDefault();
                        //if (item.dateOfAppointment != null)
                        //    dateOfAppointment = UAAAS.Models.Utilities.MMDDYY2DDMMYY(item.dateOfAppointment.ToString());

                        qualification = jntuh_registered_faculty_educations.OrderByDescending(education => education.educationId)
                                                                .Where(education => education.facultyId == item.id)
                                                                .Select(education => education.courseStudied).FirstOrDefault();

                        string strSpecialization = string.Empty;
                        var Specialization = jntuh_specialization.Where(s => s.id == item.SpecializationId).Select(s => s.specializationName).FirstOrDefault();
                        strSpecialization = Specialization;

                        string identifiedfor = jntuh_college_faculty_registered.Where(f => f.RegistrationNumber == item.FacultyRegistrationNumber && f.collegeId == item.collegeId).Select(f => f.IdentifiedFor).FirstOrDefault();

                        var Faculty_Data = Bas_Days.Where(e => e.RegistrationNumber == item.FacultyRegistrationNumber).Select(e => e).ToList();
                        DateTime? Old_JiningDate = Faculty_Data.OrderByDescending(a => a.joiningDate).Select(f => f.joiningDate).FirstOrDefault();
                        string JoiningDate = UAAAS.Models.Utilities.MMDDYY2DDMMYY(Old_JiningDate.ToString());

                        if (jntuh_regstrd_faculty_exps != null)
                        {
                            var regfacultyExp = jntuh_regstrd_faculty_exps.Where(i => i.facultyId == item.id).LastOrDefault();
                            if (regfacultyExp != null)
                            {
                                item.facultygrosssalA21 = regfacultyExp.FacultyGrossSalary;
                                dateOfAppointment = UAAAS.Models.Utilities.MMDDYY2DDMMYY(regfacultyExp.facultyDateOfAppointment.ToString());
                            }
                        }

                        string Facultyphoto = jntuh_registered_facultys.Where(F => F.RegistrationNumber == item.FacultyRegistrationNumber).Select(F => F.Photo).FirstOrDefault();
                        if (item.HighestQualification == 6 || item.Phd2pages == true)
                            collegeFaculty += "<tr style='font-weight:bold;'>";
                        else
                            collegeFaculty += "<tr>";
                        collegeFaculty += "<td  style='font-size: 8px;'><p align='center'>" + count + "</p></td>";
                        if (item.HighestQualification == 6 || item.Phd2pages == true)
                            collegeFaculty += "<td  style='font-size: 8px;'><p align='center'>" + item.FacultyRegistrationNumber + "&nbsp;&nbsp;&nbsp;" + strcheckList + "&nbsp;&nbsp;&nbsp;&nbsp;" + "<b>(Ph.D)</b> </p></td>";
                        else
                            collegeFaculty += "<td  style='font-size: 8px;'><p align='center'>" + item.FacultyRegistrationNumber + "&nbsp;&nbsp;&nbsp;" + strcheckList + "</p></td>";
                        collegeFaculty += "<td  style='font-size: 8px;'><p align='left'>" + (item.facultyFirstName + " " + item.facultySurname + " " + item.facultyLastName).ToUpper() + "&nbsp;&nbsp;&nbsp;" + strcheckList + "</p></td>";
                        collegeFaculty += "<td  style='font-size: 8px;'><p align='center'>" + gender + "</p></td>";

                        if (!string.IsNullOrEmpty(designation))
                        {
                            if (item.facultyDesignationId == 1)
                                collegeFaculty += "<td  style='font-size:8px;'><p align='left'>" + "P" + "</p></td>";
                            else if (item.facultyDesignationId == 2)
                                collegeFaculty += "<td style='font-size:8px;'><p align='left'>" + "ASP" + "</p></td>";
                            else if (item.facultyDesignationId == 3)
                                collegeFaculty += "<td style='font-size:8px;'><p align='left'>" + "AP" + "</p></td>";
                            else
                                collegeFaculty += "<td style='font-size:8px;'><p align='left'>&nbsp;</p></td>";
                        }
                        else
                            collegeFaculty += "<td  style='font-size:8px;'><p align='left'>&nbsp;</p></td>";

                        if (!string.IsNullOrEmpty(qualification))
                            collegeFaculty += "<td  style='font-size: 8px;'><p align='left'>" + qualification.ToUpper() + "&nbsp;&nbsp;&nbsp;<br/><br/>" + strcheckList + "</p></td>";
                        else
                            collegeFaculty += "<td  style='font-size: 8px;'><p align='left'>&nbsp;</p></td>";

                        collegeFaculty += "<td  style='font-size: 8px;'><p align='left'>" + dateOfAppointment + "&nbsp;&nbsp;&nbsp;" + strcheckList + "</p></td>";
                        collegeFaculty += "<td  style='font-size: 8px;'><p align='center'>" + item.facultyPANNumber + "&nbsp;&nbsp;" + strcheckList + "</p></td>";
                        collegeFaculty += "<td ><p align='center'>" + item.facultyAadhaarNumber + "&nbsp;&nbsp;" + strcheckList + "</p></td>";
                        collegeFaculty += "<td  style='font-size: 8px;'><p align='center'>" + strcheckList + "</p></td>";
                        collegeFaculty += "<td ><p align='center'>" + strcheckList + "</p></td>";
                        collegeFaculty += "<td ><p align='center'>" + strcheckList + "</p></td>";
                        collegeFaculty += "<td ><p align='center'>" + item.facultygrosssalA21 + "</p></td>";

                        if (!string.IsNullOrEmpty(Facultyphoto))
                        {
                            string FacultyParsing = string.Empty;
                            string strFacultyPhoto = string.Empty;
                            string path = "http://jntuhaac.in/Content/Upload/Faculty/Photos/" + Facultyphoto.Trim();
                            // string path = @"~/Content/Upload/Faculty/Photos/" + Facultyphoto.Trim();
                            // path = System.Web.HttpContext.Current.Server.MapPath(path);
                            //  if (System.IO.File.Exists(path))

                            #region With-Out Html Parsing
                            try
                            {
                                if (!string.IsNullOrEmpty(path))
                                // if (System.IO.File.Exists(path))
                                {
                                    FacultyParsing += "<p align='center'><img src='" + HtmlEncoder.Encode(path.Trim()) + "' align='center'  width='40' height='45' /></p>";
                                    var ParseEliments = HTMLWorker.ParseToList(new StringReader(FacultyParsing), null);

                                    if (path.Contains("."))
                                    {
                                        strFacultyPhoto = "<img alt=''src='" + HtmlEncoder.Encode(path.Trim()) + "' align='center'  height='45' />";
                                        // strPrincipalPhoto = "<img alt='' src='" + serverURL + "/Content/Upload/Faculty/Photos/" + PrincipalDetails.Photo + "'" + " align='center'  height='50' />";
                                        collegeFaculty += "<td><p align='center'>" + strFacultyPhoto + "</p></td>";
                                    }
                                    else
                                    {
                                        collegeFaculty += "<td><p align='center'>&nbsp;</p></td>";
                                    }
                                }
                                else
                                {
                                    if (test415PDF.Equals("YES"))
                                    {
                                        collegeFaculty += "<td><p align='center'>&nbsp;</p></td>";
                                    }
                                    else
                                    {
                                        collegeFaculty += "<td><p align='center'>&nbsp;</p></td>";
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                collegeFaculty += "<td><p align='center'>&nbsp;</p></td>";
                                collegeFaculty += "<td><p align='center'>" + strcheckList + "</p></td>";
                                collegeFaculty += "<td><p align='center'></p></td>";
                                collegeFaculty += "</tr>";
                                continue;
                            }
                            #endregion
                        }
                        else
                        {
                            collegeFaculty += "<td><p align='center'>&nbsp;</p></td>";
                        }

                        collegeFaculty += "<td><p align='center'>" + strcheckList + "</p></td>";
                        collegeFaculty += "<td><p align='center'></p></td>";
                        collegeFaculty += "</tr>";
                        count++;
                    }
                }


                collegeFaculty += "</tbody>";
                collegeFaculty += "</table>";
                #endregion
            }
            //List<IElement> parsedHtmlElements = HTMLWorker.ParseToList(new StringReader(collegeFaculty), null);

            return collegeFaculty;
        }
        #endregion

        private string collegeNonTachingFacultyMembers(int collegeId, string contents)
        {
            using (var db = new uaaasDBContext())
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

                IQueryable<jntuh_faculty_type> jntuh_faculty_type = db.jntuh_faculty_type.Select(e => e);

                int teachingFacultyTypeId = jntuh_faculty_type.Where(f => f.facultyType == "Non-Teaching").Select(f => f.id).FirstOrDefault();
                string ratified = string.Empty;
                List<jntuh_college_faculty> facultyList = db.jntuh_college_faculty
                                                            .Where(faculty => faculty.collegeId == collegeId && faculty.facultyTypeId == teachingFacultyTypeId)
                                                            .ToList();
                facultyList = facultyList.OrderBy(faculty => faculty.facultyDepartmentId)
                                         .ThenBy(faculty => faculty.facultyDesignationId)
                                         .ThenBy(faculty => faculty.facultyFirstName)
                                         .ToList();

                collegeFaculty += "<table border='1' cellspacing='0' cellpadding='3' style='font-size: 9px;'>";
                collegeFaculty += "<tbody>";
                collegeFaculty += "<tr>";
                collegeFaculty += "<td colspan='1'><p align='center'>SNo</p></td>";
                collegeFaculty += "<td colspan='5'><p align='left'>Faculty Name</p></td>";
                collegeFaculty += "<td colspan='1'><p align='center'>Gender</p></td>";
                collegeFaculty += "<td colspan='2'><p align='left'>Qualification </p></td>";
                collegeFaculty += "<td colspan='3'><p align='left'>Designation</p></td>";
                collegeFaculty += "<td colspan='2'><p align='center'>Experience</p></td>";
                collegeFaculty += "<td colspan='2'><p align='left'>Date of Appointment</p></td>";
                collegeFaculty += "<td colspan='2'><p align='center'>Gross Salary</p></td>";
                collegeFaculty += "</tr>";
                var facultyDesignationIds = facultyList.Select(f => f.facultyDesignationId).ToList();
                var jntuh_designations = db.jntuh_designation.Where(it => facultyDesignationIds.Contains(it.id)).Select(it => new
                {
                    it.id,
                    it.designation
                }).ToList();
                var ids = facultyList.Select(f => f.id).ToList();
                var jntuh_faculty_educations = db.jntuh_faculty_education
                    .OrderByDescending(education => education.educationId)
                    .Where(it => ids.Contains(it.id)).Select(it => new
                    {
                        facultyId = it.id,
                        it.courseStudied
                    }).ToList();
                var facultyTypeIds = facultyList.Select(f => f.facultyTypeId).ToList();
                var jntuh_faculty_types = jntuh_faculty_type.Where(it => facultyTypeIds.Contains(it.id)).Select(it => new
                {
                    it.id,
                    it.facultyType
                }).ToList();
                foreach (var item in facultyList)
                {
                    if (item.facultyGenderId == 1)
                    {
                        gender = "M";
                    }
                    else
                    {
                        gender = "F";
                    }

                    designation = jntuh_designations.Where(d => d.id == item.facultyDesignationId).Select(d => d.designation).FirstOrDefault();
                    if (item.facultyDateOfAppointment != null)
                    {
                        dateOfAppointment = UAAAS.Models.Utilities.MMDDYY2DDMMYY(item.facultyDateOfAppointment.ToString());
                    }
                    if (item.facultyTypeId == teachingFacultyTypeId && item.isFacultyRatifiedByJNTU)
                    {
                        ratified = "Yes";
                    }
                    else
                    {
                        ratified = "No";
                    }


                    qualification = jntuh_faculty_educations.Where(education => education.facultyId == item.id)
                                                             .Select(education => education.courseStudied).FirstOrDefault();

                    teachingType = jntuh_faculty_types.Where(f => f.id == item.facultyTypeId).Select(f => f.facultyType).FirstOrDefault();
                    collegeFaculty += "<tr>";
                    collegeFaculty += "<td colspan='1'><p align='center'>" + count + "</p></td>";
                    collegeFaculty += "<td colspan='5'><p align='left'>" + item.facultyFirstName + " " + item.facultyLastName + " " + item.facultySurname + "</p></td>";
                    collegeFaculty += "<td colspan='1'><p align='center'>" + gender + "</p></td>";

                    collegeFaculty += "<td colspan='2'><p align='left'>" + qualification + "</p></td>";
                    collegeFaculty += "<td colspan='3'><p align='left'>" + designation + "</p></td>";
                    collegeFaculty += "<td colspan='2'><p align='center'>" + item.facultyPreviousExperience + "</p></td>";
                    collegeFaculty += "<td colspan='2'><p align='left'>" + dateOfAppointment + "</p></td>";
                    collegeFaculty += "<td colspan='2'><p align='center'>" + item.grossSalary + "</p></td>";

                    collegeFaculty += "</tr>";
                    count++;
                }
                collegeFaculty += "</tbody>";
                collegeFaculty += "</table>";
                contents = contents.Replace("##COLLEGENonTeachingFaculty##", collegeFaculty);
                return contents;
            }
        }

        private string collegeTechnicalFacultyMembers(int collegeId, string contents)
        {
            using (var db = new uaaasDBContext())
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
                int teachingFacultyTypeId = db.jntuh_faculty_type.Where(f => f.facultyType == "Technical").Select(f => f.id).FirstOrDefault();
                string ratified = string.Empty;
                List<jntuh_college_faculty> facultyList = db.jntuh_college_faculty
                                                            .Where(faculty => faculty.collegeId == collegeId && faculty.facultyTypeId == teachingFacultyTypeId)
                                                            .ToList();
                facultyList = facultyList.OrderBy(faculty => faculty.facultyDepartmentId)
                                         .ThenBy(faculty => faculty.facultyDesignationId)
                                         .ThenBy(faculty => faculty.facultyFirstName)
                                         .ToList();
                //int[] pharmacyDeptIds = new int[] { 26, 27, 36, 39 };
                //var DeptIDs = facultyList.Where(a => pharmacyDeptIds.Contains(a.facultyDepartmentId) && a.facultyDepartmentId != null).Select(d => d.facultyDepartmentId).Distinct().ToList();
                var DeptIDs = facultyList.Where(a => a.facultyDepartmentId != null).Select(d => d.facultyDepartmentId).Distinct().ToList();

                foreach (var deptId in DeptIDs)
                {
                    collegeFaculty += TechnicalFaculty(facultyList.Where(a => a.facultyDepartmentId == deptId).ToList(), teachingFacultyTypeId);
                }


                collegeFaculty += "<strong><p style='font-size: 9px;'><u>12 a).Technical Faculty Summary Sheet</u>:</p></strong><br />";
                collegeFaculty += "<table border='1' cellspacing='0' cellpadding='3' style='font-size: 9px;'>";
                collegeFaculty += "<tbody>";
                collegeFaculty += "<tr>";
                collegeFaculty += "<td colspan='1' rowspan='2'><p align='center'>SNo</p></td>";
                collegeFaculty += "<td colspan='2' rowspan='2'><p align='left'>Degree</p></td>";
                collegeFaculty += "<td colspan='3' rowspan='2'><p align='center'>Department / Specialization</p></td>";
                collegeFaculty += "<td colspan='6'><p align='center'>Total number of Technical Faculty  </p></td>";
                collegeFaculty += "</tr>";
                collegeFaculty += "<tr>";
                collegeFaculty += "<td colspan='3'><p align='center'>Uploaded </p></td>";
                collegeFaculty += "<td colspan='3'><p align='center'>Available as per CF </p></td>";
                collegeFaculty += "</tr>";

                int countdata = 1;

                var summarydata = facultyList.Where(rf => rf.collegeId == collegeId).GroupBy(a => new { a.facultyDepartmentId }).Select(g => new
                {
                    deptid = g.Key.facultyDepartmentId,
                    uplodedcount = g.Count()
                }).Join(db.jntuh_department, r => r.deptid, ro => ro.id, (r, ro) => new { r, ro })
                                                        .Join(db.jntuh_degree, x => x.ro.degreeId, y => y.id, (x, y) => new { x, y })
                                                        .Select(k => new { k.y.degree, k.x.ro.departmentName, k.x.r.uplodedcount })
                                                       .ToList();

                foreach (var item in summarydata)
                {
                    collegeFaculty += "<tr>";
                    collegeFaculty += "<td colspan='1'><p align='center'>" + countdata + "</p></td>";
                    collegeFaculty += "<td colspan='2'><p align='left'>" + item.degree + "</p></td>";
                    collegeFaculty += "<td colspan='3'><p align='center'>" + item.departmentName + "</p></td>";
                    collegeFaculty += "<td colspan='3'><p align='center'>" + item.uplodedcount + "</p></td>";
                    collegeFaculty += "<td colspan='3'><p align='left'>&nbsp;</p></td>";

                    collegeFaculty += "</tr>";
                    countdata++;
                }
                collegeFaculty += "</tbody>";
                collegeFaculty += "</table>";

                contents = contents.Replace("##COLLEGETechnicalFaculty##", collegeFaculty);
                return contents;
            }
        }

        private string TechnicalFaculty(List<jntuh_college_faculty> facultyList, int teachingFacultyTypeId)
        {
            using (var db = new uaaasDBContext())
            {
                int count = 1;
                string collegeFaculty = string.Empty;
                string ContentFaculty = string.Empty;
                string gender = string.Empty;
                string category = string.Empty;
                string department = string.Empty;
                string designation = string.Empty;
                string qualification = string.Empty;
                string dateOfAppointment = string.Empty;
                string teachingType = string.Empty;
                string ratified = string.Empty;
                int? deptid = facultyList.FirstOrDefault().facultyDepartmentId;

                department = db.jntuh_department.Where(d => d.id == deptid).FirstOrDefault().departmentName;

                collegeFaculty += "<p style='font-size: 9px;'><strong><u>" + department + " </u></strong></p><br />";
                collegeFaculty += "<table border='1' cellspacing='0' cellpadding='3' style='font-size: 9px;'>";
                collegeFaculty += "<tbody>";
                collegeFaculty += "<tr>";
                collegeFaculty += "<td colspan='1'><p align='center'>SNo</p></td>";
                collegeFaculty += "<td colspan='5'><p align='left'>Faculty Name</p></td>";
                collegeFaculty += "<td colspan='1'><p align='center'>Gender</p></td>";
                collegeFaculty += "<td colspan='2'><p align='left'>Qualification </p></td>";
                collegeFaculty += "<td colspan='3'><p align='left'>Designation</p></td>";
                collegeFaculty += "<td colspan='2'><p align='center'>Experience</p></td>";
                collegeFaculty += "<td colspan='2'><p align='left'>Date of Appointment</p></td>";
                collegeFaculty += "<td colspan='2'><p align='center'>Gross Salary</p></td>";

                collegeFaculty += "</tr>";
                var facultyDepartmentIds = facultyList.Select(f => f.facultyDepartmentId).ToList();
                var jntuh_departments = db.jntuh_department.Where(it => facultyDepartmentIds.Contains(it.id)).Select(it => new
                {
                    it.id,
                    it.departmentName
                }).ToList();
                var facultyDesignationIds = facultyList.Select(f => f.facultyDesignationId).ToList();
                var jntuh_designations = db.jntuh_designation.Where(it => facultyDesignationIds.Contains(it.id)).Select(it => new
                {
                    it.id,
                    it.designation
                }).ToList();
                var facultyIds = facultyList.Select(a => a.id).ToList();
                var jntuh_faculty_education = db.jntuh_faculty_education.Where(a => facultyIds.Contains(a.facultyId)).OrderByDescending(education => education.educationId)
                                                             .Select(education => new { education.facultyId, education.educationId, education.courseStudied }).ToList();

                foreach (var item in facultyList)
                {
                    if (item.facultyGenderId == 1)
                    {
                        gender = "M";
                    }
                    else
                    {
                        gender = "F";
                    }
                    department = jntuh_departments.Where(d => d.id == item.facultyDepartmentId).Select(d => d.departmentName).FirstOrDefault();
                    designation = jntuh_designations.Where(d => d.id == item.facultyDesignationId).Select(d => d.designation).FirstOrDefault();
                    if (item.facultyDateOfAppointment != null)
                    {
                        dateOfAppointment = UAAAS.Models.Utilities.MMDDYY2DDMMYY(item.facultyDateOfAppointment.ToString());
                    }
                    if (item.facultyTypeId == teachingFacultyTypeId && item.isFacultyRatifiedByJNTU == true)
                    {
                        ratified = "Yes";
                    }
                    else
                    {
                        ratified = "No";
                    }

                    qualification = jntuh_faculty_education.OrderByDescending(education => education.educationId)
                                                              .Where(education => education.facultyId == item.id)
                                                              .Select(education => education.courseStudied).FirstOrDefault();
                    teachingType = db.jntuh_faculty_type.Where(f => f.id == item.facultyTypeId).Select(f => f.facultyType).FirstOrDefault();
                    collegeFaculty += "<tr>";
                    collegeFaculty += "<td colspan='1'><p align='center'>" + count + "</p></td>";
                    collegeFaculty += "<td colspan='5'><p align='left'>" + item.facultyFirstName + " " + item.facultyLastName + " " + item.facultySurname + "</p></td>";
                    collegeFaculty += "<td colspan='1'><p align='center'>" + gender + "</p></td>";
                    collegeFaculty += "<td colspan='2'><p align='left'>" + qualification + "</p></td>";
                    collegeFaculty += "<td colspan='3'><p align='left'>" + designation + "</p></td>";
                    collegeFaculty += "<td colspan='2'><p align='center'>" + item.facultyPreviousExperience + "</p></td>";
                    collegeFaculty += "<td colspan='2'><p align='left'>" + dateOfAppointment + "</p></td>";

                    collegeFaculty += "<td colspan='2'><p align='center'>" + item.grossSalary + "</p></td>";

                    collegeFaculty += "</tr>";
                    count++;
                }
                collegeFaculty += "</tbody>";
                collegeFaculty += "</table>";
                return collegeFaculty;
            }
        }

        public string DataModifications(int collegeId, string contents)
        {
            using (var db = new uaaasDBContext())
            {
                string strDataModifications = string.Empty;
                int sno = 1;
                #region DataModifications
                strDataModifications += "<table border='1' cellspacing='0' cellpadding='4' style='font-size: 9px;'>";
                strDataModifications += "<tbody>";
                strDataModifications += "<tr>";
                strDataModifications += "<td colspan='2'><p align='center'>S.No.</p></td>";
                strDataModifications += "<td colspan='2' align='left'><p>Form No</p></td>";
                strDataModifications += "<td colspan=10><p align='left'>Justification</p></td>";
                strDataModifications += "</tr>";

                var datamodification = db.jntuh_college_staticdata_modifications.Where(d => d.collegeId == collegeId).OrderBy(d => d.formNo).ToList();
                if (datamodification != null)
                {
                    foreach (var item in datamodification)
                    {
                        strDataModifications += "<tr>";
                        strDataModifications += "<td colspan='2'><p align='center'>" + sno + "</p></td>";
                        strDataModifications += "<td colspan='2' align='left'>" + item.formNo + "</td>";
                        strDataModifications += "<td colspan='10' align='left'>" + item.justification + "</td>";
                        strDataModifications += "</tr>";
                        sno++;
                    }
                }
                strDataModifications += "</tbody>";
                strDataModifications += "</table>";

                #endregion
                contents = contents.Replace("##DataModifications##", strDataModifications);
                return contents;
            }


        }

        private string LateFeePaymentBillDetails(int collegeId, string contents)
        {
            string Paymentbilldetails = string.Empty;
            string strPaymentDate = string.Empty;
            Paymentbilldetails = "";
            var datetime = "";
            string paymentdatecurrentformat = "";
            string collegecode = db.jntuh_college.Where(e => e.id == collegeId).Select(e => e.collegeCode).FirstOrDefault();
            var actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(q => q.id).FirstOrDefault();
            int? PresentYear = actualYear + 1;
            List<jntuh_paymentresponse> payment = db.jntuh_paymentresponse.Where(a => a.AcademicYearId == PresentYear && a.CollegeId == collegecode && a.AuthStatus == "0300" && a.PaymentTypeID == 8).ToList();

            if (payment != null && payment.Count() != 0)
            {
                Paymentbilldetails += "<p align='left'><strong><u>LateFee Payment Details</u></strong></p><br />";
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
                }
                Paymentbilldetails += "</tbody></table>";
                contents = contents.Replace("##LateFeePaymentDetails1##", Paymentbilldetails);
                var Paymentdate = payment.Select(e => e.TxnDate).FirstOrDefault();

                string Latefeedate = Paymentdate.ToString() == null ? string.Empty : UAAAS.Models.Utilities.MMDDYY2DDMMYY(Paymentdate.ToString()).ToString();
                paymentdatecurrentformat += "<p>Your  LateFee Payment Submission Date : " + Latefeedate + "</p>";
                //contents = contents.Replace("##LATEFEEPAYMENTDATE##", paymentdatecurrentformat);

                #region application submission Date

                // var Updateondate = (from i in db.jntuh_college_edit_status join j in db.jntuh_college on i.collegeId equals j.id where (i.IsCollegeEditable == false && j.id == collegeId) select j.updatedOn).FirstOrDefault();
                int actualYear1 = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
                int prAy = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear1 + 1)).Select(s => s.id).FirstOrDefault();
                var Updateondate = db.jntuh_college_edit_status.Where(i => i.IsCollegeEditable == false && i.collegeId == collegeId && i.academicyearId == prAy).Select(I => I.updatedOn).FirstOrDefault();

                if (Updateondate != null)
                {
                    datetime = UAAAS.Models.Utilities.MMDDYY2DDMMYY(Updateondate.ToString());
                    // datetime = Updateondate.ToString();
                }
                else
                {
                    datetime = string.Empty;
                }

                #endregion
                string LatefeeCodetext = " <p>Your Online Application Submission Date : " + datetime + "</p>";

                LatefeeQrCodetext += ";Online Application Date:" + datetime;
                contents = contents.Replace("##LATEFEESUBMITTEDDATE##", LatefeeCodetext);
                contents = contents.Replace("##LATEFEEPAYMENTDATE##", paymentdatecurrentformat);
            }
            else
            {
                contents = contents.Replace("##LateFeePaymentDetails1##", Paymentbilldetails);
                contents = contents.Replace("##LATEFEEPAYMENTDATE##", paymentdatecurrentformat);
                contents = contents.Replace("##LATEFEESUBMITTEDDATE##", datetime);
            }


            return contents;
        }

        public string LateFeebarcodegenerator(int collegeId, string contents)
        {

            string str = string.Empty;
            string strDataModifications = string.Empty;
            string strimagedetails = string.Empty;
            string strimagebarcodedetails = string.Empty;
            var collegeCode = db.jntuh_college.Where(college => college.isActive == true && college.id == collegeId).Select(college => college.collegeCode).FirstOrDefault();
            var challanaNO = db.jntuh_paymentresponse.Where(college => college.CollegeId == collegeCode && college.AuthStatus == "0300" && college.PaymentTypeID == 8).Select(college => college.CustomerID).FirstOrDefault();

            if (challanaNO != null)
            {

                /////QR Code GEneration Code
                Gma.QrCodeNet.Encoding.QrEncoder qrEncoder = new Gma.QrCodeNet.Encoding.QrEncoder(Gma.QrCodeNet.Encoding.ErrorCorrectionLevel.H);
                Gma.QrCodeNet.Encoding.QrCode qrCode = new Gma.QrCodeNet.Encoding.QrCode();
                qrEncoder.TryEncode(LatefeeQrCodetext, out qrCode);
                Gma.QrCodeNet.Encoding.Windows.Render.GraphicsRenderer renderer = new Gma.QrCodeNet.Encoding.Windows.Render.GraphicsRenderer(new Gma.QrCodeNet.Encoding.Windows.Render.FixedModuleSize(4, Gma.QrCodeNet.Encoding.Windows.Render.QuietZoneModules.Four), System.Drawing.Brushes.Black, System.Drawing.Brushes.White);

                Stream memoryStream = new MemoryStream();
                renderer.WriteToStream(qrCode.Matrix, ImageFormat.Png, memoryStream);
                // very important to reset memory stream to a starting position, otherwise you would get 0 bytes returned
                memoryStream.Position = 0;

                var resultStream = new FileStreamResult(memoryStream, "image/png");
                resultStream.FileDownloadName = String.Format("{0}.png", collegeCode);


                System.Drawing.Image v = System.Drawing.Image.FromStream(memoryStream);
                if (!Directory.Exists(Server.MapPath("~/Content/Upload/LateFeeQrCodePhotos")))
                {
                    Directory.CreateDirectory(Server.MapPath("~/Content/Upload/LateFeeQrCodePhotos"));
                }
                var ext = resultStream.ContentType;
                var Filename = resultStream.FileDownloadName;

                System.Drawing.Image img = v;
                img.Save(string.Format("{0}/{1}", Server.MapPath("~/Content/Upload/LateFeeQrCodePhotos"), Filename));

                if (Filename != null)
                {
                    strimagedetails = "/Content/Upload/LateFeeQrCodePhotos/" + Filename;
                }
                else
                {
                    strimagedetails = string.Empty;
                }
                string path = @"~" + strimagedetails;
                path = System.Web.HttpContext.Current.Server.MapPath(path);


                if (challanaNO != null)
                {
                    strimagebarcodedetails = "/Content/Upload/LateFeeQrCodePhotos/" + challanaNO + ".png";
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

            contents = contents.Replace("##LATEFEEQRcode##", strDataModifications);
            return contents;
        }

        public string AcademicAudit(int collegeId, string contents)
        {
            int SNO = 1;
            string AcademicAudit = string.Empty;
            var AcademicAuditList = db.jntuh_academic_audit.Where(a => a.isActive == true).ToList();
            AcademicAudit += "<table  border='1' cellspacing='0' cellpadding='4' style='width:100px;'><tbody>";
            AcademicAudit += " <tr><td colspan='1'><p align='center'>S.No.</p></td><td colspan='5'><p align='left'>Audit Item</p></td><td colspan='10' align='left'><p>Remarks</p></td></tr>";

            if (AcademicAuditList.Count() == 0 && AcademicAuditList == null)
            {
                AcademicAudit += " <tr><td colspan='4'><p align='center'>NIL</p></td></tr>";

            }
            else
            {
                foreach (var item in AcademicAuditList)
                {
                    AcademicAudit += " <tr style='height:500px;'><td style='vertical-align:top;' colspan='1'><p align='center'>" + SNO + "</p></td><td style='vertical-align:top;' colspan='5' align='left'>" + item.academicauditType + "</td><td colspan='10' align='left'>" + item.remarks + "<br/><br/><br/><br/><br/><br/></td></tr>";
                    SNO++;
                }
            }
            AcademicAudit += "</tbody></table>";
            contents = contents.Replace("##AcademicAudit##", AcademicAudit);
            List<IElement> parsedHtmlElements = HTMLWorker.ParseToList(new StringReader(contents), null);
            return contents;
        }

        public string ValueAddedPrograms(int collegeId, string contents)
        {
            var SNO = 1;
            var ValueAddedPrograms = string.Empty;
            var masteractivities = db.jntuh_extracurricularactivities.Where(i => i.isactive == true && i.activitytype == 1).ToList();
            var extraactivities = db.jntuh_college_extracurricularactivities.Where(i => i.isactive == true && i.collegeid == collegeId).ToList();

            var valueAddedProgramsList = extraactivities.Select(item =>
            {
                var jntuhExtracurricularactivities = masteractivities.FirstOrDefault(i => i.sno == item.activityid);
                return jntuhExtracurricularactivities != null ? new CollegeExtraCirActivities
                                                                            {
                                                                                activityid = item.activityid,
                                                                                activityDesc = jntuhExtracurricularactivities.activitydescription,
                                                                                activitystatus = item.activitystatus,
                                                                                supportingdocuments = item.supportingdocuments,
                                                                                remarks = item.remarks
                                                                            } : null;
            }).ToList();

            ValueAddedPrograms += "<table  border='1' cellspacing='0' cellpadding='4' style='width:100px;'><tbody>";
            ValueAddedPrograms += " <tr><td colspan='1'><p align='center'>S.No.</p></td><td colspan='12'><p align='left'>Description</p></td><td colspan='3' align='left'><p>Status</p></td></tr>";

            if (valueAddedProgramsList.Any())
            {
                foreach (var item in valueAddedProgramsList)
                {
                    var status = item.activitystatus ? "Yes" : "No";
                    if (item.activityid == 5 || item.activityid == 6 || item.activityid == 7)
                    {
                        if (item.activityid == 5)
                            status = item.activitystatus ? "Yes ( " + item.remarks + " Lakhs)" : "No";
                        else
                            status = item.activitystatus ? "Yes ( " + item.remarks + " )" : "No";
                    }
                    ValueAddedPrograms += " <tr><td style='vertical-align:top;' colspan='1'><p align='center'>" + SNO + "</p></td><td style='vertical-align:top;' colspan='12' align='left'>" + item.activityDesc + "</td><td colspan='3' align='left'>" + status + "</td></tr>";
                    SNO++;
                }
            }
            else
            {
                ValueAddedPrograms += " <tr><td colspan='4'><p align='center'>NIL</p></td></tr>";

            }
            ValueAddedPrograms += "</tbody></table>";
            contents = contents.Replace("##ValueAddedPrograms##", ValueAddedPrograms);
            var parsedHtmlElements = HTMLWorker.ParseToList(new StringReader(contents), null);
            return contents;
        }

        public string EssentialRequirements(int collegeId, string contents)
        {
            var SNO = 1;
            var EssentialRequirements = string.Empty;
            var masteractivities = db.jntuh_essential_requirements.Where(i => i.isactive == true && i.essentialtype == 1).ToList();
            var extraactivities = db.jntuh_college_essential_requirements.Where(i => i.isactive == true && i.collegeid == collegeId).ToList();
            var essentialRequirementsList = (from item in extraactivities
                                             let jntuhEssentialRequirements = masteractivities.FirstOrDefault(i => i.id == item.essentialid)
                                             where jntuhEssentialRequirements != null
                                             select new CollegeEssentialReq()
                                             {
                                                 essentialid = item.essentialid,
                                                 essentialDesc = jntuhEssentialRequirements.essentialdescription,
                                                 essentialstatus = item.essentialstatus,
                                                 supportingdocuments = item.supportintdocument
                                             }).ToList();

            EssentialRequirements += "<table  border='1' cellspacing='0' cellpadding='4' style='width:100px;'><tbody>";
            EssentialRequirements += " <tr><td colspan='1'><p align='center'>S.No.</p></td><td colspan='12'><p align='left'>Description</p></td><td colspan='3' align='left'><p>Status</p></td></tr>";

            if (essentialRequirementsList.Any())
            {
                foreach (var item in essentialRequirementsList)
                {
                    var status = item.essentialstatus ? "Yes" : "No";
                    EssentialRequirements += " <tr><td style='vertical-align:top;' colspan='1'><p align='center'>" + SNO + "</p></td><td style='vertical-align:top;' colspan='12' align='left'>" + item.essentialDesc + "</td><td colspan='3' align='left'>" + status + "</td></tr>";
                    SNO++;
                }
            }
            else
            {
                EssentialRequirements += " <tr><td colspan='4'><p align='center'>NIL</p></td></tr>";

            }
            EssentialRequirements += "</tbody></table>";
            contents = contents.Replace("##EssentialRequirements##", EssentialRequirements);
            var parsedHtmlElements = HTMLWorker.ParseToList(new StringReader(contents), null);
            return contents;
        }

        public string JHubRequirements(int collegeId, string contents)
        {
            var SNO = 1;
            var EssentialRequirements = string.Empty;
            var masteractivities = db.jntuh_jhubactivities.Where(i => i.isactive == true && i.activitytype == 1).ToList();
            var extraactivities = db.jntuh_college_jhubactivities.Where(i => i.isactive == true && i.collegeid == collegeId).ToList();
            var essentialRequirementsList = (from item in extraactivities
                                             let jntuhEssentialRequirements = masteractivities.FirstOrDefault(i => i.sno == item.activityid)
                                             where jntuhEssentialRequirements != null
                                             select new CollegeEssentialReq()
                                             {
                                                 essentialid = item.activityid,
                                                 essentialDesc = jntuhEssentialRequirements.activitydescription,
                                                 essentialstatus = item.activitystatus,
                                                 //supportingdocuments = item.supportingdocuments
                                             }).ToList();

            EssentialRequirements += "<table  border='1' cellspacing='0' cellpadding='4' style='width:100px;'><tbody>";
            EssentialRequirements += " <tr><td colspan='1'><p align='center'>S.No.</p></td><td colspan='12'><p align='left'>Description</p></td><td colspan='3' align='left'><p>Status</p></td></tr>";

            if (essentialRequirementsList.Any())
            {
                foreach (var item in essentialRequirementsList)
                {
                    var status = item.essentialstatus ? "Yes" : "No";
                    EssentialRequirements += " <tr><td style='vertical-align:top;' colspan='1'><p align='center'>" + SNO + "</p></td><td style='vertical-align:top;' colspan='12' align='left'>" + item.essentialDesc + "</td><td colspan='3' align='left'>" + status + "</td></tr>";
                    SNO++;
                }
            }
            else
            {
                EssentialRequirements += " <tr><td colspan='4'><p align='center'>NIL</p></td></tr>";

            }
            EssentialRequirements += "</tbody></table>";
            contents = contents.Replace("##JHubRequirements##", EssentialRequirements);
            var parsedHtmlElements = HTMLWorker.ParseToList(new StringReader(contents), null);
            return contents;
        }

        public string CollegeAffiliationFee(int collegeId, string contents)
        {
            using (var db = new uaaasDBContext())
            {
                string strPrintersDetails = string.Empty;
                strPrintersDetails += "<table border='1' cellspacing='0' cellpadding='3' style='font-size: 9px;'><tbody>";
                strPrintersDetails += "<tr>";
                //strPrintersDetails += "<td  colspan='2'><p><b>Degree</b></p></td>";
                strPrintersDetails += "<td  valign='top' colspan='2'><p align='center'><b>Affiliation Fee 2022-23</b></p></td>";
                strPrintersDetails += "<td  valign='top' colspan='2'><p align='center'><b>Affiliation Fee Upto 2021-22</b></p></td>";
                strPrintersDetails += "<td  valign='top' colspan='2'><p align='center'><b>Common Service Fee Upto 2021-22</b></p></td>";
                strPrintersDetails += "</tr>";
                var actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(q => q.actualYear).FirstOrDefault();
                int prAy = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
                var AffiliationFee = db.jntuh_college_paymentoffee.Where(e => e.collegeId == collegeId && e.FeeTypeID == 5 && e.academicyearId == (prAy - 2)).Select(e => e.duesAmount).FirstOrDefault();
                var CommanserviceFee = db.jntuh_college_paymentoffee.Where(e => e.collegeId == collegeId && e.FeeTypeID == 3 && e.academicyearId == (prAy - 2)).Select(e => e.duesAmount).FirstOrDefault();

                var AffiliationFeeDues2022_23 = db.jntuh_college_paymentoffee.Where(e => e.collegeId == collegeId && e.FeeTypeID == 5 && e.academicyearId == (prAy - 1)).Select(e => e.duesAmount).FirstOrDefault();
                //ViewBag.AffiliationFeeDues2022_23 = AffiliationFeeDues2022_23;

                string clgAffFee = string.Empty;
                string cmnInspFee = string.Empty;
                if (AffiliationFee != null)
                {
                    decimal amount = Convert.ToDecimal(AffiliationFee);
                    if (amount >= 0)
                        clgAffFee = AffiliationFee;
                    else
                        clgAffFee = "N/A";
                }
                else
                {
                    clgAffFee = "N/A";
                }
                if (CommanserviceFee != null)
                {
                    decimal amount = Convert.ToDecimal(CommanserviceFee);
                    if (amount >= 0)
                        cmnInspFee = CommanserviceFee;
                    else
                        cmnInspFee = "N/A";
                }
                else
                {
                    cmnInspFee = "N/A";
                }
                strPrintersDetails += "<tr>";
                //strPrintersDetails += "<td  colspan='2'><p>" + item.degree + "</p></td>";
                strPrintersDetails += "<td  valign='top' align='center' colspan='2'>" + AffiliationFeeDues2022_23 + "</td>";
                strPrintersDetails += "<td  valign='top' align='center' colspan='2'>" + clgAffFee + "</td>";
                strPrintersDetails += "<td  valign='top' align='center' colspan='2'>" + cmnInspFee + "</td>";
                strPrintersDetails += "</tr>";
                strPrintersDetails += "</tbody></table>";
                contents = contents.Replace("##ClgInspFee##", strPrintersDetails);
                return contents;
            }
        }
    }
}




